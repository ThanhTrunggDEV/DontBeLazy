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

    private readonly string _hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc", "hosts");
    private ManagementEventWatcher? _processStartWatcher;
    private System.Threading.CancellationTokenSource? _pollingCts;
    
    // Core distracting domains
    private readonly string[] _distractingDomains = new[] {
        "facebook.com", "www.facebook.com",
        "youtube.com", "www.youtube.com",
        "instagram.com", "www.instagram.com",
        "tiktok.com", "www.tiktok.com",
        "twitter.com", "www.twitter.com",
        "x.com", "www.x.com"
    };

    // Core distracting apps (without .exe)
    private readonly string[] _distractingApps = new[] { 
        "discord", "steam", "riotclient", "leagueoflegends", 
        "epicgameslauncher", "spotify", "zalo", "telegram" 
    };

    private List<string> _allowedAppNames = new();

    public WindowsStrictEngine()
    {
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

        try
        {
            if (!File.Exists(_hostsPath)) return;

            var hostsContent = File.ReadAllText(_hostsPath);
            if (!hostsContent.Contains("### DONTBELAZY BLOCK START ###"))
            {
                var blockLines = "\n### DONTBELAZY BLOCK START ###\n";
                foreach(var d in domainsToBlock)
                {
                    blockLines += $"127.0.0.1 {d}\n";
                }
                blockLines += "### DONTBELAZY BLOCK END ###\n";
                File.AppendAllText(_hostsPath, blockLines);
                FlushDns();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error modifying hosts file. Run as Admin? {ex.Message}");
        }
    }

    private void RestoreHosts()
    {
        try
        {
            if (!File.Exists(_hostsPath)) return;

            var lines = File.ReadAllLines(_hostsPath);
            var newLines = new List<string>();
            bool skip = false;
            foreach(var line in lines)
            {
                if (line.Contains("### DONTBELAZY BLOCK START ###")) skip = true;
                if (!skip) newLines.Add(line);
                if (line.Contains("### DONTBELAZY BLOCK END ###")) skip = false;
            }
            File.WriteAllLines(_hostsPath, newLines);
            FlushDns();
        }
        catch
        {
            // Require Admin rights implicitly
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
            _pollingCts.Cancel();
            _pollingCts.Dispose();
            _pollingCts = null;
        }

        if (_processStartWatcher != null)
        {
            _processStartWatcher.Stop();
            _processStartWatcher.EventArrived -= ProcessStarted;
            _processStartWatcher.Dispose();
            _processStartWatcher = null;
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
        RestoreHosts(); // Important: Cleanup when app crashes or closes
    }
}
