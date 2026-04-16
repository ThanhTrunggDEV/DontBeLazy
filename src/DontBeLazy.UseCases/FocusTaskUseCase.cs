using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.ValueObjects;
using DontBeLazy.Ports.DTOs;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Repositories;
using DontBeLazy.UseCases.Mappers;

namespace DontBeLazy.UseCases;

public class FocusTaskUseCase : IFocusTaskUseCase
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FocusTaskUseCase(ITaskRepository taskRepository, IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<FocusTaskDto>> GetAllTasksAsync()
    {
        var tasks = await _taskRepository.GetAllAsync();
        return tasks.Select(DtoMapper.ToDto).ToList();
    }

    public async Task<FocusTaskDto> CreateTaskAsync(string name, int expectedMinutes, Guid? profileId = null, bool? perTaskStrictMode = null)
    {
        var task = new FocusTask(name, expectedMinutes);
        if (profileId.HasValue || perTaskStrictMode.HasValue)
        {
            var pid = profileId.HasValue ? new ProfileId(profileId.Value) : (ProfileId?)null;
            task.UpdateDetails(name, expectedMinutes, pid, perTaskStrictMode);
        }

        await _taskRepository.AddAsync(task);
        await _unitOfWork.SaveChangesAsync();
        return DtoMapper.ToDto(task);
    }

    public async Task UpdateTaskAsync(Guid taskId, string name, int expectedMinutes, Guid? profileId, bool? perTaskStrictMode)
    {
        var task = await _taskRepository.GetByIdAsync(new TaskId(taskId));
        if (task == null) throw new KeyNotFoundException($"Task {taskId} not found.");

        var pid = profileId.HasValue ? new ProfileId(profileId.Value) : (ProfileId?)null;
        task.UpdateDetails(name, expectedMinutes, pid, perTaskStrictMode);
        await _taskRepository.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteTaskAsync(Guid taskId)
    {
        await _taskRepository.DeleteAsync(new TaskId(taskId));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangeTaskStatusAsync(Guid taskId, TaskStatusDto newStatus)
    {
        var task = await _taskRepository.GetByIdAsync(new TaskId(taskId));
        if (task == null) throw new KeyNotFoundException($"Task {taskId} not found.");

        task.ChangeStatus(DtoMapper.ToDomain(newStatus));
        await _taskRepository.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SetTaskRecurringAsync(Guid taskId, string recurringType, string config)
    {
        var task = await _taskRepository.GetByIdAsync(new TaskId(taskId));
        if (task == null) throw new KeyNotFoundException($"Task {taskId} not found.");

        if (Enum.TryParse<Domain.Enums.RecurringType>(recurringType, out var type))
            task.SetRecurring(type, config);

        await _taskRepository.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateTaskSortOrderAsync(Guid taskId, int newSortOrder)
    {
        var task = await _taskRepository.GetByIdAsync(new TaskId(taskId));
        if (task == null) throw new KeyNotFoundException($"Task {taskId} not found.");

        task.UpdateSortOrder(newSortOrder);
        await _taskRepository.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PauseTaskAsync(Guid taskId, bool isPaused)
    {
        var task = await _taskRepository.GetByIdAsync(new TaskId(taskId));
        if (task == null) throw new KeyNotFoundException($"Task {taskId} not found.");

        task.SetPaused(isPaused);
        await _taskRepository.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();
    }
}
