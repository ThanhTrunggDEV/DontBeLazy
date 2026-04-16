using System.Threading.Tasks;
using DontBeLazy.Ports.DTOs;

namespace DontBeLazy.Ports.Inbound;

public interface IFocusSessionUseCase
{
    Task<SessionHistoryDto> StartSessionAsync(Guid? taskId, string taskName, Guid? profileId, int expectedSeconds);
    Task SyncSessionTimeAsync(Guid sessionId, int tickSeconds);
    Task PauseSessionAsync(Guid sessionId);
    Task ResumeSessionAsync(Guid sessionId);
    Task CompleteSessionAsync(Guid sessionId, CompletionStatusDto status);
    Task LogBlockedAttemptAsync(Guid sessionId);
    Task<SessionHistoryDto?> GetCurrentSessionAsync();
}
