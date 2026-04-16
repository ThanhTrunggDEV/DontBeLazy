using System;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace DontBeLazy.Domain.Tests;

public class SessionHistoryTests
{
    private SessionHistory CreateValidSession()
    {
        var taskId = TaskId.New();
        return new SessionHistory(taskId, "Test Task", "Test Profile", 60 * 60, true);
    }

    [Fact]
    public void Constructor_WithValidData_ShouldInitializeCorrectly()
    {
        var session = CreateValidSession();

        session.ExpectedSeconds.Should().Be(3600);
        session.ActualSeconds.Should().Be(0);
        session.CompletionStatus.Should().BeNull();
        session.WasStrictMode.Should().BeTrue();
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
    public void CompleteSession_Twice_ShouldThrowInvalidOperationException()
    {
        var session = CreateValidSession();
        session.CompleteSession(CompletionStatus.Completed);
        
        Action act = () => session.CompleteSession(CompletionStatus.Abandoned);
        
        act.Should().Throw<InvalidOperationException>().WithMessage("*Session is already completed or abandoned.*");
    }

    [Fact]
    public void IncrementActualSeconds_AfterCompletion_ShouldThrowException()
    {
        var session = CreateValidSession();
        session.CompleteSession(CompletionStatus.Completed);
        
        Action act = () => session.IncrementActualSeconds(100);
        
        act.Should().Throw<InvalidOperationException>().WithMessage("*Cannot update time for a completed or abandoned session.*");
    }

    [Fact]
    public void AddSnapshot_ShouldAddToCollection()
    {
        var session = CreateValidSession();
        var snapshot = new SessionProfileSnapshot(session.Id, ProfileEntryType.Website, "github.com", null);

        session.AddSnapshot(snapshot);

        session.Snapshots.Should().ContainSingle();
        session.Snapshots.Should().Contain(snapshot);
    }
}
