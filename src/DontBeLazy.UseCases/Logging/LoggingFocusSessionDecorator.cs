using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DontBeLazy.Ports.DTOs;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Services;

namespace DontBeLazy.UseCases.Logging;

/// <summary>
/// Decorator: Wraps IFocusSessionUseCase and logs every user-triggered
/// session action (start, complete, restore, discard, etc.) to the app log file.
/// </summary>
public sealed class LoggingFocusSessionDecorator : IFocusSessionUseCase
{
    private readonly IFocusSessionUseCase _inner;
    private readonly IAppLogger _logger;

    public LoggingFocusSessionDecorator(IFocusSessionUseCase inner, IAppLogger logger)
    {
        _inner  = inner;
        _logger = logger;
    }

    public async Task<SessionHistoryDto> StartSessionAsync(Guid? taskId, string taskName, Guid? profileId, int expectedSeconds)
    {
        _logger.Info($"[Session] StartSessionAsync → taskId={taskId}, task='{taskName}', profileId={profileId}, expectedSec={expectedSeconds}");
        var sw = Stopwatch.StartNew();
        try
        {
            var result = await _inner.StartSessionAsync(taskId, taskName, profileId, expectedSeconds);
            _logger.Info($"[Session] StartSessionAsync ✓ sessionId={result.Id}, duration={sw.ElapsedMilliseconds}ms");
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error($"[Session] StartSessionAsync ✗ failed after {sw.ElapsedMilliseconds}ms", ex);
            throw;
        }
    }

    public async Task SyncSessionTimeAsync(Guid sessionId, int tickSeconds)
    {
        // High-frequency call — only log errors to avoid log spam
        try
        {
            await _inner.SyncSessionTimeAsync(sessionId, tickSeconds);
        }
        catch (Exception ex)
        {
            _logger.Error($"[Session] SyncSessionTimeAsync ✗ sessionId={sessionId}, tick={tickSeconds}s", ex);
            throw;
        }
    }

    public Task PauseSessionAsync(Guid sessionId)
    {
        _logger.Info($"[Session] PauseSessionAsync → sessionId={sessionId}");
        return _inner.PauseSessionAsync(sessionId);
    }

    public Task ResumeSessionAsync(Guid sessionId)
    {
        _logger.Info($"[Session] ResumeSessionAsync → sessionId={sessionId}");
        return _inner.ResumeSessionAsync(sessionId);
    }

    public async Task CompleteSessionAsync(Guid sessionId, CompletionStatusDto status)
    {
        _logger.Info($"[Session] CompleteSessionAsync → sessionId={sessionId}, status={status}");
        var sw = Stopwatch.StartNew();
        try
        {
            await _inner.CompleteSessionAsync(sessionId, status);
            _logger.Info($"[Session] CompleteSessionAsync ✓ done in {sw.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            _logger.Error($"[Session] CompleteSessionAsync ✗ failed after {sw.ElapsedMilliseconds}ms", ex);
            throw;
        }
    }

    public Task LogBlockedAttemptAsync(Guid sessionId)
    {
        _logger.Warning($"[Session] LogBlockedAttemptAsync → BLOCKED attempt detected, sessionId={sessionId}");
        return _inner.LogBlockedAttemptAsync(sessionId);
    }

    public Task<SessionHistoryDto?> GetCurrentSessionAsync()
        => _inner.GetCurrentSessionAsync();

    public Task<SessionHistoryDto?> GetIncompleteSessionAsync()
        => _inner.GetIncompleteSessionAsync();

    public async Task<SessionHistoryDto> RestoreSessionAsync(Guid sessionId)
    {
        _logger.Info($"[Session] RestoreSessionAsync → Restoring orphan session={sessionId}");
        var sw = Stopwatch.StartNew();
        try
        {
            var result = await _inner.RestoreSessionAsync(sessionId);
            _logger.Info($"[Session] RestoreSessionAsync ✓ done in {sw.ElapsedMilliseconds}ms");
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error($"[Session] RestoreSessionAsync ✗ failed after {sw.ElapsedMilliseconds}ms", ex);
            throw;
        }
    }

    public async Task DiscardSessionAsync(Guid sessionId)
    {
        _logger.Warning($"[Session] DiscardSessionAsync → Discarding orphan session={sessionId}");
        var sw = Stopwatch.StartNew();
        try
        {
            await _inner.DiscardSessionAsync(sessionId);
            _logger.Info($"[Session] DiscardSessionAsync ✓ done in {sw.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            _logger.Error($"[Session] DiscardSessionAsync ✗ failed after {sw.ElapsedMilliseconds}ms", ex);
            throw;
        }
    }
}
