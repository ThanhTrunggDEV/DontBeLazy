using System;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;

namespace DontBeLazy.Domain.Entities;

public class SessionHistory
{
    public SessionId Id { get; private set; }
    public TaskId? TaskId { get; private set; }
    public string SnapshotTaskName { get; private set; }
    public string? SnapshotProfileName { get; private set; }
    
    public DateTime FocusStartDate { get; private set; }
    public DateTime? FocusEndDate { get; private set; }
    
    public int ExpectedSeconds { get; private set; }
    public int ActualSeconds { get; private set; }
    
    public CompletionStatus? CompletionStatus { get; private set; }
    public int BlockedCount { get; private set; }
    public bool WasStrictMode { get; private set; }

    // Navigation property
    private System.Collections.Generic.List<SessionProfileSnapshot> _snapshots = new();
    public System.Collections.Generic.IReadOnlyCollection<SessionProfileSnapshot> Snapshots => _snapshots.AsReadOnly();

    public SessionHistory(TaskId? taskId, string taskName, string? profileName, int expectedSeconds, bool wasStrictMode)
    {
        Id = SessionId.New();
        TaskId = taskId;
        SnapshotTaskName = taskName;
        SnapshotProfileName = profileName;
        ExpectedSeconds = expectedSeconds;
        WasStrictMode = wasStrictMode;
        FocusStartDate = DateTime.Now;
        ActualSeconds = 0;
        BlockedCount = 0;
    }

    public void IncrementActualSeconds(int seconds)
    {
        if (CompletionStatus != null)
            throw new InvalidOperationException("Cannot update time for a completed or abandoned session.");
            
        ActualSeconds += seconds;
    }

    public void CompleteSession(CompletionStatus status)
    {
        if (CompletionStatus != null)
            throw new InvalidOperationException("Session is already completed or abandoned.");

        if (status == Enums.CompletionStatus.CompletedEarly)
        {
            // The actual logic of completing early should update ActualSeconds accurately up to that point
            // This is just a state change.
        }
        
        CompletionStatus = status;
        FocusEndDate = DateTime.Now;
    }

    public void IncrementBlockedCount()
    {
        BlockedCount++;
    }

    public int GetRemainingSeconds()
    {
        return Math.Max(0, ExpectedSeconds - ActualSeconds);
    }

    public void AddSnapshot(SessionProfileSnapshot snapshot)
    {
        _snapshots.Add(snapshot);
    }

#pragma warning disable CS8618 // EF Core constructor
    private SessionHistory() {}
#pragma warning restore CS8618
}
