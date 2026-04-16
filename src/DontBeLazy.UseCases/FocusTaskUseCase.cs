using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Repositories;

namespace DontBeLazy.UseCases;

public class FocusTaskUseCase : IFocusTaskUseCase
{
    private readonly ITaskRepository _taskRepository;

    public FocusTaskUseCase(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IReadOnlyCollection<FocusTask>> GetAllTasksAsync()
    {
        return await _taskRepository.GetAllAsync();
    }

    public async Task<FocusTask> CreateTaskAsync(string name, int expectedMinutes, ProfileId? profileId = null, bool? perTaskStrictMode = null)
    {
        var task = new FocusTask(name, expectedMinutes);
        task.UpdateDetails(name, expectedMinutes, profileId, perTaskStrictMode);
        
        await _taskRepository.AddAsync(task);
        return task;
    }

    public async Task UpdateTaskAsync(TaskId taskId, string name, int expectedMinutes, ProfileId? profileId, bool? perTaskStrictMode)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new System.Collections.Generic.KeyNotFoundException($"Task {taskId} not found.");

        task.UpdateDetails(name, expectedMinutes, profileId, perTaskStrictMode);
        await _taskRepository.UpdateAsync(task);
    }

    public async Task DeleteTaskAsync(TaskId taskId)
    {
        await _taskRepository.DeleteAsync(taskId);
    }

    public async Task ChangeTaskStatusAsync(TaskId taskId, DontBeLazy.Domain.Enums.TaskStatus newStatus)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new System.Collections.Generic.KeyNotFoundException($"Task {taskId} not found.");

        task.ChangeStatus(newStatus);
        await _taskRepository.UpdateAsync(task);
    }

    public async Task SetTaskRecurringAsync(TaskId taskId, RecurringType type, string config)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new System.Collections.Generic.KeyNotFoundException($"Task {taskId} not found.");

        task.SetRecurring(type, config);
        await _taskRepository.UpdateAsync(task);
    }

    public async Task UpdateTaskSortOrderAsync(TaskId taskId, int newSortOrder)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new System.Collections.Generic.KeyNotFoundException($"Task {taskId} not found.");

        task.UpdateSortOrder(newSortOrder);
        await _taskRepository.UpdateAsync(task);
    }

    public async Task PauseTaskAsync(TaskId taskId, bool isPaused)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new System.Collections.Generic.KeyNotFoundException($"Task {taskId} not found.");

        task.SetPaused(isPaused);
        await _taskRepository.UpdateAsync(task);
    }
}
