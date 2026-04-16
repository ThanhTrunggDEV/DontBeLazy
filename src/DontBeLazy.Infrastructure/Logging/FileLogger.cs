using System;
using System.IO;
using System.Runtime.Versioning;
using DontBeLazy.Ports.Outbound.Services;

namespace DontBeLazy.Infrastructure.Logging;

[SupportedOSPlatform("windows")]
public sealed class FileLogger : IAppLogger, IDisposable
{
    private readonly string _logDirectory;
    private readonly object _lock = new();
    private const int MaxDaysToKeep = 7;

    public FileLogger()
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DontBeLazy", "logs");

        Directory.CreateDirectory(_logDirectory);
        PurgeOldLogs();
    }

    private string CurrentLogPath =>
        Path.Combine(_logDirectory, $"app-{DateTime.Now:yyyy-MM-dd}.log");

    public void Info(string message)    => Write("INFO   ", message, null);
    public void Warning(string message) => Write("WARNING", message, null);
    public void Error(string message, Exception? ex = null) => Write("ERROR  ", message, ex);

    private void Write(string level, string message, Exception? ex)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var line = $"[{timestamp}] [{level}] {message}";
        if (ex != null)
            line += $"\n              Exception: {ex.GetType().Name}: {ex.Message}\n              StackTrace: {ex.StackTrace}";

        lock (_lock)
        {
            try
            {
                File.AppendAllText(CurrentLogPath, line + Environment.NewLine);
            }
            catch
            {
                // Never crash the app because of a logging failure
            }
        }
    }

    private void PurgeOldLogs()
    {
        try
        {
            var cutoff = DateTime.Now.AddDays(-MaxDaysToKeep);
            foreach (var file in Directory.GetFiles(_logDirectory, "app-*.log"))
            {
                if (File.GetLastWriteTime(file) < cutoff)
                    File.Delete(file);
            }
        }
        catch { /* best effort */ }
    }

    public void Dispose() { /* StreamWriter not held open; nothing to dispose */ }
}
