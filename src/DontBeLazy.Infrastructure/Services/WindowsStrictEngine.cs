using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Runtime.Versioning;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Ports.Outbound.Services;

namespace DontBeLazy.Infrastructure.Services;

[SupportedOSPlatform("windows")]
public class WindowsStrictEngine : IStrictEnginePort, IDisposable
{
    [DllImport("user32.dll")]
    private static extern bool LockWorkStation();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    private readonly string _hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc", "hosts");
    private ManagementEventWatcher? _processStartWatcher;
    private System.Threading.CancellationTokenSource? _pollingCts;
    private LocalRedirectServer? _redirectServer;
    
    // Core distracting domains
    private readonly string[] _distractingDomains = new[] {
        "facebook.com", "www.facebook.com",
        "youtube.com", "www.youtube.com",
        "instagram.com", "www.instagram.com",
        "tiktok.com", "www.tiktok.com",
        "twitter.com", "www.twitter.com",
        "x.com", "www.x.com",
        "reddit.com", "www.reddit.com",
        "netflix.com", "www.netflix.com",
        "twitch.tv", "www.twitch.tv",
        "pinterest.com", "www.pinterest.com",
        // Comics / Manga / Novels (Vietnamese)
        "nettruyen.com", "nettruyen.net", "nettruyen.vn", "nettruyenco.vn", "nettruyenbb.com", "nettruyenvn.com", "nettruyenww.com",
        "truyenqq.com", "truyenqq.net", "truyenqq.vn", "truyenqqvip.com", "truyenqqpro.com", "truyenqqto.com",
        "pops.vn", "mangatoon.mobi", "waka.vn",
        "truyenfull.vn", "truyenfull.com", "sstruyen.com", "sstruyen.vn", "dtruyen.com",
        "nhattruyen.com", "nhattruyen.vip", "saytruyen.net", "saytruyen.com",
        "cuutruyen.net", "cmanga.com", "baotangtruyentranh.com", "blogtruyen.vn", "thichdoctruyen.vip",
        "truyentranh.net", "webtruyen.com", "truyenmacom.com", "sangtacviet.pro", "sangtacviet.vip",
        "truyenwiki.com", "truyenchu.vn", "truyencv.com", "truyennhuatv.com", "goctruyen.com",
        "truyenhay.com", "truyenmoi.vn", "metruyencv.com", "doctruyen.com", "doctruyen3q.com",
        "truyensieucap.com", "truyenchon.com", "truyengi.com", "truyenkinhdien.com", "truyencotich.vn", 
        "truyenvip.com", "meotruyentranh.com", "truyentranhaudio.com",
        // Text Novels (Vietnamese)
        "wattpad.com", "truyenyy.com", "truyenyy.vip", "tangthuvien.vn", "wikidich.com", "wikidichvip.com", "enovel.vn",
        // Movies / Streaming
        "phimmoi.net", "motphim.net", "tvhay.org", "vieon.vn", "fptplay.vn", "galaxyplay.vn", "vtvgiaitri.vn",
        // Forums & News (Vietnam)
        "voz.vn", "tinhte.vn", "webtretho.com", "otofun.net", "kenh14.vn", "znews.vn", "vnexpress.net", "24h.com.vn"
    };

    // Core distracting apps (without .exe)
    private readonly string[] _distractingApps = new[] { 
        // Launchers & Social
        "discord", "steam", "steamwebhelper", "riotclient", "riotclientux", 
        "epicgameslauncher", "eadesktop", "origin", "upc", "ubisoftgamelauncher",
        "battle.net", "xboxapp", 
        // Games
        "leagueoflegends", "valorant", "cs2", "csgo", "dota2", "minecraftlauncher",
        "robloxplayerbeta", "roblox", "gta5", "gtavlauncher", "playgtav",
        "r5apex", "tslgame", "overwatch", "genshinimpact", "HonkaiImpact3rd",
        "pubglite", "fifazf", "dnf", "wutheringwaves", "starrail",
        // Emulators
        "dnplayer", "nox", "HD-Player", "memu",
        // Media & Chat
        "spotify", "zalo", "telegram", "whatsapp", "messenger", "skype", "viber", "netflix", "itunes", "capcut",
        // System
        "taskmgr", "vctray"
    };

    private List<string> _allowedAppNames = new();

    public WindowsStrictEngine(DontBeLazy.Ports.Outbound.Services.IAppLogger logger)
    {
        _redirectServer = new LocalRedirectServer(logger);
        RestoreHosts(); // Self-heal: cleanup any leftover blocks from previous crashes
    }

    public Task ApplyProfileAsync(IReadOnlyCollection<SessionProfileSnapshot> profiles)
    {
        // 1. Process Domains (Website Whitelist protects against Blacklist)
        var allowedDomains = profiles
            .Where(p => p.Type == Domain.Enums.ProfileEntryType.Website)
            .Select(p => p.Value.ToLowerInvariant())
            .ToList();

        var domainsToBlock = _distractingDomains.Where(d => !allowedDomains.Any(a => d.Contains(a))).ToList();
        if (domainsToBlock.Any())
        {
            _redirectServer?.Start();
        }
        ApplyHostsBlocking(domainsToBlock);

        // 2. Process Applications
        _allowedAppNames = profiles
            .Where(p => p.Type == Domain.Enums.ProfileEntryType.App)
            .Select(p => Path.GetFileNameWithoutExtension(p.Value).ToLowerInvariant())
            .ToList();

        // 3. Initial Sweep of Active Processes
        SweepExistingDistractions();

        StartProcessWatcher();

        return Task.CompletedTask;
    }

    public Task ClearRestrictionsAsync()
    {
        StopProcessWatcher();
        _redirectServer?.Stop();
        RestoreHosts();
        return Task.CompletedTask;
    }

    public Task LockScreenAsync()
    {
        LockWorkStation();
        return Task.CompletedTask;
    }

    private void ApplyHostsBlocking(List<string> domainsToBlock)
    {
        RestoreHosts(); // Always ensure a clean state before blocking

        RetryFileOperation(() =>
        {
            if (!File.Exists(_hostsPath)) return;

            var hostsContent = File.ReadAllText(_hostsPath);
            if (!hostsContent.Contains("### DONTBELAZY BLOCK START ###"))
            {
                var blockLines = "\n### DONTBELAZY BLOCK START ###\n";
                foreach (var d in domainsToBlock)
                {
                    blockLines += $"127.0.0.1 {d}\n";
                }
                blockLines += "### DONTBELAZY BLOCK END ###\n";
                File.AppendAllText(_hostsPath, blockLines);
                FlushDns();
            }
        });
    }

    private void RestoreHosts()
    {
        RetryFileOperation(() =>
        {
            if (!File.Exists(_hostsPath)) return;

            var lines = File.ReadAllLines(_hostsPath);
            var newLines = new List<string>();
            bool skip = false;
            foreach (var line in lines)
            {
                if (line.Contains("### DONTBELAZY BLOCK START ###")) skip = true;
                if (!skip) newLines.Add(line);
                if (line.Contains("### DONTBELAZY BLOCK END ###")) skip = false;
            }
            File.WriteAllLines(_hostsPath, newLines);
            FlushDns();
        });
    }

    private void RetryFileOperation(Action fileAction)
    {
        int retries = 5;
        int delayMs = 200;
        for (int i = 0; i < retries; i++)
        {
            try
            {
                fileAction();
                return;
            }
            catch (IOException)
            {
                if (i == retries - 1)
                {
                    MessageBox(IntPtr.Zero, 
                        "DontBeLazy không thể chặn Website vì file hosts đang bị Hệ điều hành (hoặc Antivirus) khoá.\n\nVui lòng tạm thời tắt tính năng bảo vệ hệ thống của phần mềm diệt virus hoặc thêm Ngoại lệ (Exclusions).", 
                        "Lỗi phân quyền", 
                        0x30); // 0x30 = MB_ICONWARNING
                    return;
                }
                System.Threading.Thread.Sleep(delayMs);
            }
            catch (Exception ex)
            {
                MessageBox(IntPtr.Zero, 
                    $"Lỗi cấp quyền (System): {ex.Message}\nKhông thể áp dụng chế độ tập trung cho trình duyệt.", 
                    "Lỗi hệ thống", 
                    0x10); // 0x10 = MB_ICONERROR
                return;
            }
        }
    }

    private void FlushDns()
    {
        try 
        {
            Process.Start(new ProcessStartInfo("ipconfig", "/flushdns") { CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden })?.WaitForExit();
        }
        catch 
        {
            // Ignore if ipconfig is unavailable
        }
    }

    private void StartProcessWatcher()
    {
        StopProcessWatcher(); // Ensure clean state

        // Fallback polling loop (ensures app blocking works even if WMI is denied)
        _pollingCts = new System.Threading.CancellationTokenSource();
        _ = Task.Run(() => PollingLoop(_pollingCts.Token));

        try 
        {
            var query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
            _processStartWatcher = new ManagementEventWatcher(query);
            _processStartWatcher.EventArrived += ProcessStarted;
            _processStartWatcher.Start();
        }
        catch
        {
            // Ignore WMI errors if missing admin rights
        }
    }

    private async Task PollingLoop(System.Threading.CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            SweepExistingDistractions();
            try { await Task.Delay(2000, token); }
            catch (TaskCanceledException) { break; }
        }
    }

    private void StopProcessWatcher()
    {
        if (_pollingCts != null)
        {
            try 
            {
                _pollingCts.Cancel();
                _pollingCts.Dispose();
            }
            catch { }
            finally { _pollingCts = null; }
        }

        if (_processStartWatcher != null)
        {
            try
            {
                _processStartWatcher.Stop();
                _processStartWatcher.EventArrived -= ProcessStarted;
                _processStartWatcher.Dispose();
            }
            catch { }
            finally { _processStartWatcher = null; }
        }
    }

    private void SweepExistingDistractions()
    {
        try
        {
            var runningProcesses = Process.GetProcesses();
            foreach (var p in runningProcesses)
            {
                var processNameNoExt = p.ProcessName.ToLowerInvariant();
                if (_distractingApps.Contains(processNameNoExt) && !_allowedAppNames.Contains(processNameNoExt))
                {
                    try
                    {
                        p.Kill();
                    }
                    catch
                    {
                        // Ignore access denied on kill
                    }
                }
            }
        }
        catch
        {
            // Ignore access denied on GetProcesses
        }
    }

    private void ProcessStarted(object sender, EventArrivedEventArgs e)
    {
        try 
        {
            var processName = e.NewEvent.Properties["ProcessName"]?.Value?.ToString();
            if (string.IsNullOrEmpty(processName)) return;

            var processNameNoExt = Path.GetFileNameWithoutExtension(processName).ToLowerInvariant();
            
            // Check if process is a known distractor and not actively whitelisted
            if (_distractingApps.Contains(processNameNoExt) && !_allowedAppNames.Contains(processNameNoExt))
            {
                var processId = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);
                var p = Process.GetProcessById(processId);
                p.Kill();
            }
        }
        catch
        {
            // Ignore access denied errors during kill
        }
    }

    public void Dispose()
    {
        StopProcessWatcher();
        _redirectServer?.Dispose();
        RestoreHosts(); // Important: Cleanup when app crashes or closes
    }
}
