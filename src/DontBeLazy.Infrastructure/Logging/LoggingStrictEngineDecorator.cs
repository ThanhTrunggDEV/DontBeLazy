using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Ports.Outbound.Services;

namespace DontBeLazy.Infrastructure.Logging;

/// <summary>
/// Decorator: Wraps IStrictEnginePort and logs every system-level operation
/// (apply profile, clear restrictions, lock screen) to the app log file.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class LoggingStrictEngineDecorator : IStrictEnginePort
{
    private readonly IStrictEnginePort _inner;
    private readonly IAppLogger _logger;

    public LoggingStrictEngineDecorator(IStrictEnginePort inner, IAppLogger logger)
    {
        _inner  = inner;
        _logger = logger;
    }

    public async Task ApplyProfileAsync(IReadOnlyCollection<SessionProfileSnapshot> profiles)
    {
        _logger.Info($"[StrictEngine] ApplyProfileAsync → Applying {profiles.Count} profile rule(s).");
        var sw = Stopwatch.StartNew();
        try
        {
            await _inner.ApplyProfileAsync(profiles);
            _logger.Info($"[StrictEngine] ApplyProfileAsync ✓ done in {sw.ElapsedMilliseconds}ms.");
        }
        catch (Exception ex)
        {
            _logger.Error($"[StrictEngine] ApplyProfileAsync ✗ failed after {sw.ElapsedMilliseconds}ms.", ex);
            throw;
        }
    }

    public async Task ClearRestrictionsAsync()
    {
        _logger.Info("[StrictEngine] ClearRestrictionsAsync → Restoring hosts and stopping process watcher.");
        var sw = Stopwatch.StartNew();
        try
        {
            await _inner.ClearRestrictionsAsync();
            _logger.Info($"[StrictEngine] ClearRestrictionsAsync ✓ done in {sw.ElapsedMilliseconds}ms.");
        }
        catch (Exception ex)
        {
            _logger.Error($"[StrictEngine] ClearRestrictionsAsync ✗ failed after {sw.ElapsedMilliseconds}ms.", ex);
            throw;
        }
    }

    public async Task LockScreenAsync()
    {
        _logger.Warning("[StrictEngine] LockScreenAsync → Initiating workstation lock.");
        var sw = Stopwatch.StartNew();
        try
        {
            await _inner.LockScreenAsync();
            _logger.Info($"[StrictEngine] LockScreenAsync ✓ done in {sw.ElapsedMilliseconds}ms.");
        }
        catch (Exception ex)
        {
            _logger.Error($"[StrictEngine] LockScreenAsync ✗ failed after {sw.ElapsedMilliseconds}ms.", ex);
            throw;
        }
    }
}
