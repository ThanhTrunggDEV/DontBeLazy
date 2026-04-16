using System;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace DontBeLazy.Domain.Tests;

public class SessionHistoryTests
{
    private SessionHistory CreateValidSession() => new(TaskId.New(), "Test Task", "Test Profile", 3600, true);

    [Fact]
    public void Constructor_WithValidData_ShouldInitializeCorrectly()
    {
        var session = CreateValidSession();
        session.ExpectedSeconds.Should().Be(3600);
        session.SnapshotTaskName.Should().Be("Test Task");
        session.SnapshotProfileName.Should().Be("Test Profile");
    }

    [Fact]
    public void IncrementActualSeconds_ValidSession_ShouldIncreaseTime()
    {
        var session = CreateValidSession();
        session.IncrementActualSeconds(300);
        session.ActualSeconds.Should().Be(300);
        session.GetRemainingSeconds().Should().Be(3300);
    }
    
    [Fact]
    public void IncrementBlockedCount_ShouldIncreaseCount()
    {
        var session = CreateValidSession();
        session.IncrementBlockedCount();
        session.BlockedCount.Should().Be(1);
    }

    [Fact]
    public void CompleteSession_Twice_ShouldThrowInvalidOperationException()
    {
        var session = CreateValidSession();
        session.CompleteSession(CompletionStatus.Completed);
        Action act = () => session.CompleteSession(CompletionStatus.Abandoned);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CompleteSession_Early_ShouldJustUpdateStatusAndDate()
    {
        var session = CreateValidSession();
        session.CompleteSession(CompletionStatus.CompletedEarly);
        session.CompletionStatus.Should().Be(CompletionStatus.CompletedEarly);
        session.FocusEndDate.Should().HaveValue();
    }

    [Fact]
    public void IncrementActualSeconds_AfterCompletion_ShouldThrowException()
    {
        var session = CreateValidSession();
        session.CompleteSession(CompletionStatus.Completed);
        Action act = () => session.IncrementActualSeconds(100);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddSnapshot_ShouldAddToCollection()
    {
        var session = CreateValidSession();
        var snapshot = new SessionProfileSnapshot(session.Id, ProfileEntryType.Website, "github.com");
        session.AddSnapshot(snapshot);
        session.Snapshots.Should().ContainSingle();
    }
}
