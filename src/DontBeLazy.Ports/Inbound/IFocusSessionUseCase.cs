using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;

namespace DontBeLazy.Ports.Inbound;

public interface IFocusSessionUseCase
{
    Task<SessionHistory> StartSessionAsync(TaskId? taskId, string taskName, ProfileId? profileId, int expectedSeconds);
    Task SyncSessionTimeAsync(SessionId sessionId, int tickSeconds);
    Task PauseSessionAsync(SessionId sessionId);
    Task ResumeSessionAsync(SessionId sessionId);
    Task CompleteSessionAsync(SessionId sessionId, CompletionStatus status);
    Task LogBlockedAttemptAsync(SessionId sessionId);
    Task<SessionHistory> GetCurrentSessionAsync();
}
