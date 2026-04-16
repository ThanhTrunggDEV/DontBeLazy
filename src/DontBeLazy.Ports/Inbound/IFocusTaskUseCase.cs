using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Ports.DTOs;

namespace DontBeLazy.Ports.Inbound;

public interface IFocusTaskUseCase
{
    Task<IReadOnlyCollection<FocusTaskDto>> GetAllTasksAsync();
    Task<FocusTaskDto> CreateTaskAsync(string name, int expectedMinutes, Guid? profileId = null, bool? perTaskStrictMode = null);
    Task UpdateTaskAsync(Guid taskId, string name, int expectedMinutes, Guid? profileId, bool? perTaskStrictMode);
    Task DeleteTaskAsync(Guid taskId);
    Task ChangeTaskStatusAsync(Guid taskId, TaskStatusDto newStatus);
    Task SetTaskRecurringAsync(Guid taskId, string recurringType, string config);
    Task UpdateTaskSortOrderAsync(Guid taskId, int newSortOrder);
    Task PauseTaskAsync(Guid taskId, bool isPaused);
}
