using System;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.ValueObjects;
using DontBeLazy.Ports.DTOs;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Repositories;
using DontBeLazy.Ports.Outbound.Services;
using DontBeLazy.UseCases.Mappers;

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

    public async Task<SessionHistoryDto> StartSessionAsync(Guid? taskId, string taskName, Guid? profileId, int expectedSeconds)
    {
        if (_sessionState.CurrentSession != null && _sessionState.CurrentSession.CompletionStatus == null)
            throw new InvalidOperationException("A session is already active.");

        var settings = await _settingsRepository.GetSettingsAsync();

        Profile? profile = null;
        if (profileId != null)
            profile = await _profileRepository.GetByIdAsync(new ProfileId(profileId.Value));

        var domainTaskId = taskId.HasValue ? new TaskId(taskId.Value) : (TaskId?)null;
        var session = new SessionHistory(domainTaskId, taskName, profile?.Name, expectedSeconds, settings.GlobalStrictMode);

        if (profile != null)
        {
            foreach (var entry in profile.Entries)
                session.AddSnapshot(new SessionProfileSnapshot(session.Id, entry.Type, entry.Value, entry.ExePath));

            await _processPort.ApplyProfileAsync(session.Snapshots);
        }

        await _sessionRepository.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        _sessionState.StartSession(session, profile);
        _sessionState.UpdateLastTick(_clockPort.GetTickCount());

        return DtoMapper.ToDto(session);
    }

    public async Task SyncSessionTimeAsync(Guid sessionId, int tickSeconds)
    {
        if (_sessionState.CurrentSession == null || _sessionState.CurrentSession.Id.Value != sessionId)
            throw new InvalidOperationException("Session is not active.");

        var currentTicks = _clockPort.GetTickCount();
        var elapsedReal = (currentTicks - _sessionState.LastTickAmount).TotalSeconds;
        var validSeconds = Math.Min(tickSeconds, (int)elapsedReal + 2);

        if (validSeconds > 0)
        {
            try 
            {
                _sessionState.CurrentSession.IncrementActualSeconds(validSeconds);
                await _sessionRepository.UpdateAsync(_sessionState.CurrentSession);
                await _unitOfWork.SaveChangesAsync();
            }
            catch 
            {
                // Silently swallow syncing DB exceptions to prevent crashing the OnTimerTick loop.
            }
        }

        _sessionState.UpdateLastTick(currentTicks);
    }

    public Task PauseSessionAsync(Guid sessionId) => Task.CompletedTask;

    public Task ResumeSessionAsync(Guid sessionId)
    {
        _sessionState.UpdateLastTick(_clockPort.GetTickCount());
        return Task.CompletedTask;
    }

    public async Task CompleteSessionAsync(Guid sessionId, CompletionStatusDto status)
    {
        if (_sessionState.CurrentSession == null || _sessionState.CurrentSession.Id.Value != sessionId)
            throw new InvalidOperationException("Session is not active.");

        try
        {
            _sessionState.CurrentSession.CompleteSession(DtoMapper.ToDomain(status));
            await _sessionRepository.UpdateAsync(_sessionState.CurrentSession);
            await _unitOfWork.SaveChangesAsync();
        }
        finally
        {
            await _processPort.ClearRestrictionsAsync();
            _sessionState.Clear();
        }
    }

    public async Task LogBlockedAttemptAsync(Guid sessionId)
    {
        if (_sessionState.CurrentSession == null || _sessionState.CurrentSession.Id.Value != sessionId) return;

        _sessionState.CurrentSession.IncrementBlockedCount();
        await _sessionRepository.UpdateAsync(_sessionState.CurrentSession);
        await _unitOfWork.SaveChangesAsync();
    }

    public Task<SessionHistoryDto?> GetCurrentSessionAsync()
    {
        var dto = _sessionState.CurrentSession == null ? null : DtoMapper.ToDto(_sessionState.CurrentSession);
        return Task.FromResult(dto);
    }
}
