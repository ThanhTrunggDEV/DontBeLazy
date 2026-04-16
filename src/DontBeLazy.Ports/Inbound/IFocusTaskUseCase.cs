using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;

namespace DontBeLazy.Ports.Inbound;

public interface IFocusTaskUseCase
{
    Task<IReadOnlyCollection<FocusTask>> GetAllTasksAsync();
    Task<FocusTask> CreateTaskAsync(string name, int expectedMinutes, ProfileId? profileId = null, bool? perTaskStrictMode = null);
    Task UpdateTaskAsync(TaskId taskId, string name, int expectedMinutes, ProfileId? profileId, bool? perTaskStrictMode);
    Task DeleteTaskAsync(TaskId taskId);
    Task ChangeTaskStatusAsync(TaskId taskId, DontBeLazy.Domain.Enums.TaskStatus newStatus);
    Task SetTaskRecurringAsync(TaskId taskId, RecurringType type, string config);
    Task UpdateTaskSortOrderAsync(TaskId taskId, int newSortOrder);
}
