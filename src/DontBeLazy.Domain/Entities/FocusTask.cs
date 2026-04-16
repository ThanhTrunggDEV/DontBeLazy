using System;
using DontBeLazy.Domain.Enums;
using TaskStatus = DontBeLazy.Domain.Enums.TaskStatus;

namespace DontBeLazy.Domain.Entities;

public class FocusTask
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public int ExpectedMinutes { get; private set; }
    public int? ProfileId { get; private set; }
    public bool? PerTaskStrictMode { get; private set; }
    public TaskStatus Status { get; private set; } = TaskStatus.Pending;
    public int SortOrder { get; private set; }
    
    public TaskType TaskType { get; private set; } = TaskType.OneTime;
    public RecurringType? RecurringType { get; private set; }
    public string? RecurringConfig { get; private set; }
    public bool IsPaused { get; private set; }
    
    public DateTime? LastDoneDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public FocusTask(string name, int expectedMinutes)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
            throw new ArgumentException("Task name must be between 1 and 200 characters.");
            
        if (expectedMinutes <= 0 || expectedMinutes > 240)
            throw new ArgumentException("Expected minutes must be between 1 and 240.");

        Name = name;
        ExpectedMinutes = expectedMinutes;
        CreatedAt = DateTime.Now;
    }

    public void UpdateDetails(string name, int expectedMinutes, int? profileId, bool? perTaskStrictMode)
    {
        Name = name;
        ExpectedMinutes = expectedMinutes;
        ProfileId = profileId;
        PerTaskStrictMode = perTaskStrictMode;
        UpdatedAt = DateTime.Now;
    }

    public void SetRecurring(RecurringType type, string config)
    {
        TaskType = TaskType.Recurring;
        RecurringType = type;
        RecurringConfig = config;
        UpdatedAt = DateTime.Now;
    }

    public void ChangeStatus(TaskStatus newStatus)
    {
        Status = newStatus;
        if (newStatus == TaskStatus.Done || newStatus == TaskStatus.Abandoned)
        {
            LastDoneDate = DateTime.Now.Date;
        }
        UpdatedAt = DateTime.Now;
    }

    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdatedAt = DateTime.Now;
    }

    public void SetPaused(bool isPaused)
    {
        IsPaused = isPaused;
        UpdatedAt = DateTime.Now;
    }
}
