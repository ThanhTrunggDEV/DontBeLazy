using System;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Repositories;
using DontBeLazy.Ports.Outbound.Services;

namespace DontBeLazy.UseCases;

public class FocusSessionUseCase : IFocusSessionUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly ISystemSettingsRepository _settingsRepository;
    private readonly IMonotonicClockPort _clockPort;
    private readonly IStrictEnginePort _processPort;
    
    // Simplistic approach: in a real desktop application, the "current session" 
    // might be tracked in memory by a singleton state engine. Since UseCases are scoped, 
    // we use a static singleton for demo, or ideally it should be handled via a Singleton StateService.
    private static SessionHistory? _currentSession;
    private static Profile? _currentProfile;
    private static TimeSpan _lastTickAmount;

    public FocusSessionUseCase(
        ISessionRepository sessionRepository,
        IProfileRepository profileRepository,
        ISystemSettingsRepository settingsRepository,
        IMonotonicClockPort clockPort,
        IStrictEnginePort processPort)
    {
        _sessionRepository = sessionRepository;
        _profileRepository = profileRepository;
        _settingsRepository = settingsRepository;
        _clockPort = clockPort;
        _processPort = processPort;
    }

    public async Task<SessionHistory> StartSessionAsync(TaskId? taskId, string taskName, ProfileId? profileId, int expectedSeconds)
    {
        if (_currentSession != null && _currentSession.CompletionStatus == null)
            throw new InvalidOperationException("A session is already active.");

        bool globalStrictMode = false;
        var settings = await _settingsRepository.GetSettingsAsync();
        globalStrictMode = settings.GlobalStrictMode;

        Profile? profile = null;
        if (profileId != null)
        {
            profile = await _profileRepository.GetByIdAsync(profileId.Value);
        }

        var session = new SessionHistory(taskId, taskName, profile?.Name, expectedSeconds, globalStrictMode);

        if (profile != null)
        {
            // Add initial snapshot first
            foreach(var entry in profile.Entries)
            {
                session.AddSnapshot(new SessionProfileSnapshot(session.Id, entry.Type, entry.Value, entry.ExePath));
            }
            
            // Apply restrictions via block engine
            await _processPort.ApplyProfileAsync(session.Snapshots);
        }

        await _sessionRepository.AddAsync(session);
        
        _currentSession = session;
        _currentProfile = profile;
        _lastTickAmount = _clockPort.GetTickCount();

        return session;
    }

    public async Task SyncSessionTimeAsync(SessionId sessionId, int tickSeconds)
    {
        if (_currentSession == null || _currentSession.Id != sessionId)
            throw new InvalidOperationException("Session is not active.");

        // Security check via Monotonic Clock
        var currentTicks = _clockPort.GetTickCount();
        var elapsedReal = (currentTicks - _lastTickAmount).TotalSeconds;
        
        // If system reports tickSeconds=1 but real elapsed is drastically different (e.g. system suspended or tampered),
        // we use Math.Min to prevent arbitrary time-travel jumps filling up actual seconds unfairly.
        var validSeconds = Math.Min(tickSeconds, (int)elapsedReal + 2); // 2s buffer
        if (validSeconds > 0)
        {
            _currentSession.IncrementActualSeconds(validSeconds);
            await _sessionRepository.UpdateAsync(_currentSession);
        }

        _lastTickAmount = currentTicks;
    }

    public Task PauseSessionAsync(SessionId sessionId)
    {
        // Minimal logic for pause
        return Task.CompletedTask;
    }

    public Task ResumeSessionAsync(SessionId sessionId)
    {
        _lastTickAmount = _clockPort.GetTickCount();
        return Task.CompletedTask;
    }

    public async Task CompleteSessionAsync(SessionId sessionId, CompletionStatus status)
    {
        if (_currentSession == null || _currentSession.Id != sessionId)
            throw new InvalidOperationException("Session is not active.");

        _currentSession.CompleteSession(status);
        await _sessionRepository.UpdateAsync(_currentSession);
        
        // Release block engine logic here if applicable
        await _processPort.ClearRestrictionsAsync();
        
        _currentSession = null;
        _currentProfile = null;
    }

    public async Task LogBlockedAttemptAsync(SessionId sessionId)
    {
        if (_currentSession == null || _currentSession.Id != sessionId) return;
        
        _currentSession.IncrementBlockedCount();
        await _sessionRepository.UpdateAsync(_currentSession);
    }

    public Task<SessionHistory> GetCurrentSessionAsync()
    {
        return Task.FromResult(_currentSession!);
    }
}
