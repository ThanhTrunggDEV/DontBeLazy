using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DontBeLazy.WPF.Services;

/// <summary>
/// Checks the GitHub Releases API for updates and applies them.
/// Supports pre-release builds (e.g. v0.2.0-beta).
/// </summary>
public class UpdaterService
{
    private const string Owner  = "ThanhTrunggDEV";
    private const string Repo   = "DontBeLazy";
    // /releases (not /releases/latest) to include pre-releases
    private const string ApiUrl = $"https://api.github.com/repos/{Owner}/{Repo}/releases";

    private readonly HttpClient _http;

    public UpdaterService()
    {
        _http = new HttpClient();
        _http.DefaultRequestHeaders.UserAgent.ParseAdd($"DontBeLazy/{CurrentVersion}");
    }

    /// <summary>Current assembly version (e.g. "0.1.0").</summary>
    public static string CurrentVersion
    {
        get
        {
            var v = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 1, 0);
            return $"{v.Major}.{v.Minor}.{v.Build}";
        }
    }

    /// <summary>
    /// Strips pre-release suffix from a version string so Version.TryParse works.
    /// "0.2.0-beta" → "0.2.0"
    /// </summary>
    private static string StripPreRelease(string tag)
    {
        tag = tag.TrimStart('v');
        var dash = tag.IndexOf('-');
        return dash >= 0 ? tag[..dash] : tag;
    }

    /// <summary>
    /// Checks GitHub for the most recent release (including pre-releases).
    /// Returns null if already up to date or the check fails.
    /// </summary>
    public async Task<ReleaseInfo?> CheckForUpdateAsync()
    {
        try
        {
            var releases = await _http.GetFromJsonAsync<GitHubRelease[]>(ApiUrl);
            if (releases == null || releases.Length == 0) return null;

            // Pick the most recent release (first in list = newest) — includes pre-releases
            var release = releases[0];

            var latestNumeric = StripPreRelease(release.TagName);
            if (!Version.TryParse(latestNumeric, out var latest)) return null;

            var currentNumeric = StripPreRelease(CurrentVersion);
            if (!Version.TryParse(currentNumeric, out var current)) return null;

            if (latest <= current) return null;  // already up to date

            // Prefer MSI, fall back to portable zip
            var asset = Array.Find(release.Assets,
                            a => a.Name.Contains("win-x64") && a.Name.EndsWith(".msi"))
                      ?? Array.Find(release.Assets,
                            a => a.Name.Contains("win-x64") && a.Name.EndsWith(".zip"));
            if (asset == null) return null;

            return new ReleaseInfo(latestNumeric, release.TagName, release.Body ?? string.Empty,
                                   asset.BrowserDownloadUrl, asset.Size);
        }
        catch
        {
            return null;  // graceful failure — network down, etc.
        }
    }

    /// <summary>
    /// Downloads the asset (MSI preferred, portable zip fallback), installs, and restarts.
    /// Progress is reported as 0→100 during download.
    /// </summary>
    public async Task DownloadAndApplyAsync(ReleaseInfo release,
                                            IProgress<int>? progress = null,
                                            Action? beforeRestart = null)
    {
        var isMsi    = release.DownloadUrl.EndsWith(".msi", StringComparison.OrdinalIgnoreCase);
        var ext      = isMsi ? ".msi" : ".zip";
        var tempFile = Path.Combine(Path.GetTempPath(), $"DontBeLazy-{release.Version}{ext}");

        // ── Download ──────────────────────────────────────────────
        using var response = await _http.GetAsync(release.DownloadUrl,
                                                  HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var total = release.SizeBytes > 0 ? release.SizeBytes : (long?)null;
        await using var downloadStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream     = File.Create(tempFile);

        var buffer    = new byte[81920];
        long received = 0;
        int  read;
        while ((read = await downloadStream.ReadAsync(buffer)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, read));
            received += read;
            if (total.HasValue)
                progress?.Report((int)(received * 100 / total.Value));
        }
        progress?.Report(100);
        fileStream.Close();

        beforeRestart?.Invoke();

        if (isMsi)
        {
            // ── MSI path: msiexec handles install + shortcuts + uninstall ──
            // /qb = basic UI (progress bar); REBOOT=ReallySuppress prevents forced restart
            Process.Start(new ProcessStartInfo(
                "msiexec.exe",
                $"/i \"{tempFile}\" /qb REBOOT=ReallySuppress")
            {
                UseShellExecute = true   // required for UAC elevation prompt
            });
            System.Windows.Application.Current.Shutdown();
        }
        else
        {
            // ── ZIP path: extract + cmd auto-restart script ───────────────
            var tempDir = Path.Combine(Path.GetTempPath(), $"DontBeLazy-update-{release.Version}");
            var exeDir  = Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;

            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
            ZipFile.ExtractToDirectory(tempFile, tempDir);

            var pid     = Environment.ProcessId;
            var exeName = Path.GetFileName(Environment.ProcessPath ?? "DontBeLazy.exe");
            var script  = Path.Combine(Path.GetTempPath(), "dontbelazy_update.cmd");

            await File.WriteAllTextAsync(script,
                $"""
                @echo off
                timeout /t 2 /nobreak >nul
                taskkill /PID {pid} /F >nul 2>&1
                timeout /t 1 /nobreak >nul
                xcopy /E /Y /I "{tempDir}" "{exeDir}"
                start "" "{Path.Combine(exeDir, exeName)}"
                del "%~f0"
                """);

            Process.Start(new ProcessStartInfo("cmd.exe", $"/c \"{script}\"")
            {
                CreateNoWindow  = true,
                UseShellExecute = false,
                WindowStyle     = ProcessWindowStyle.Hidden
            });
            System.Windows.Application.Current.Shutdown();
        }
    }

    // ── GitHub API models ─────────────────────────────────────────
    private sealed record GitHubRelease(
        [property: JsonPropertyName("tag_name")] string TagName,
        [property: JsonPropertyName("body")]     string? Body,
        [property: JsonPropertyName("assets")]   GitHubAsset[] Assets);

    private sealed record GitHubAsset(
        [property: JsonPropertyName("name")]                  string Name,
        [property: JsonPropertyName("browser_download_url")] string BrowserDownloadUrl,
        [property: JsonPropertyName("size")]                  long Size);
}

/// <summary>Information about an available update.</summary>
public record ReleaseInfo(
    string Version,
    string TagName,
    string ReleaseNotes,
    string DownloadUrl,
    long   SizeBytes);
