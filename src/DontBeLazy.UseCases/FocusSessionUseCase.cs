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
    private readonly ActiveSessionState _sessionState;
    private readonly IUnitOfWork _unitOfWork;

    public FocusSessionUseCase(
        ISessionRepository sessionRepository,
        IProfileRepository profileRepository,
        ISystemSettingsRepository settingsRepository,
        IMonotonicClockPort clockPort,
        IStrictEnginePort processPort,
        ActiveSessionState sessionState,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _profileRepository = profileRepository;
        _settingsRepository = settingsRepository;
        _clockPort = clockPort;
        _processPort = processPort;
        _sessionState = sessionState;
        _unitOfWork = unitOfWork;
    }

    public async Task<SessionHistory> StartSessionAsync(TaskId? taskId, string taskName, ProfileId? profileId, int expectedSeconds)
    {
        if (_sessionState.CurrentSession != null && _sessionState.CurrentSession.CompletionStatus == null)
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
        await _unitOfWork.SaveChangesAsync();
        
        _sessionState.StartSession(session, profile);
        _sessionState.UpdateLastTick(_clockPort.GetTickCount());

        return session;
    }

    public async Task SyncSessionTimeAsync(SessionId sessionId, int tickSeconds)
    {
        if (_sessionState.CurrentSession == null || _sessionState.CurrentSession.Id != sessionId)
            throw new InvalidOperationException("Session is not active.");

        // Security check via Monotonic Clock
        var currentTicks = _clockPort.GetTickCount();
        var elapsedReal = (currentTicks - _sessionState.LastTickAmount).TotalSeconds;
        
        // If system reports tickSeconds=1 but real elapsed is drastically different (e.g. system suspended or tampered),
        // we use Math.Min to prevent arbitrary time-travel jumps filling up actual seconds unfairly.
        var validSeconds = Math.Min(tickSeconds, (int)elapsedReal + 2); // 2s buffer
        if (validSeconds > 0)
        {
            _sessionState.CurrentSession.IncrementActualSeconds(validSeconds);
            await _sessionRepository.UpdateAsync(_sessionState.CurrentSession);
            await _unitOfWork.SaveChangesAsync();
        }

        _sessionState.UpdateLastTick(currentTicks);
    }

    public Task PauseSessionAsync(SessionId sessionId)
    {
        // Minimal logic for pause
        return Task.CompletedTask;
    }

    public Task ResumeSessionAsync(SessionId sessionId)
    {
        _sessionState.UpdateLastTick(_clockPort.GetTickCount());
        return Task.CompletedTask;
    }

    public async Task CompleteSessionAsync(SessionId sessionId, CompletionStatus status)
    {
        if (_sessionState.CurrentSession == null || _sessionState.CurrentSession.Id != sessionId)
            throw new InvalidOperationException("Session is not active.");

        _sessionState.CurrentSession.CompleteSession(status);
        await _sessionRepository.UpdateAsync(_sessionState.CurrentSession);
        await _unitOfWork.SaveChangesAsync();
        
        // Release block engine logic here if applicable
        await _processPort.ClearRestrictionsAsync();
        
        _sessionState.Clear();
    }

    public async Task LogBlockedAttemptAsync(SessionId sessionId)
    {
        if (_sessionState.CurrentSession == null || _sessionState.CurrentSession.Id != sessionId) return;
        
        _sessionState.CurrentSession.IncrementBlockedCount();
        await _sessionRepository.UpdateAsync(_sessionState.CurrentSession);
        await _unitOfWork.SaveChangesAsync();
    }

    public Task<SessionHistory> GetCurrentSessionAsync()
    {
        return Task.FromResult(_sessionState.CurrentSession!);
    }
}
