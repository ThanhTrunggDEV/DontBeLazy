using System;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace DontBeLazy.Domain.Tests;

public class FocusTaskTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateTask()
    {
        var task = new FocusTask("Learn C#", 60);

        task.Name.Should().Be("Learn C#");
        task.ExpectedMinutes.Should().Be(60);
        task.Status.Should().Be(Domain.Enums.TaskStatus.Pending);
        task.TaskType.Should().Be(TaskType.OneTime);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException(string? invalidName)
    {
        Action act = () => new FocusTask(invalidName, 60);
        act.Should().Throw<ArgumentException>().WithMessage("*Task name must be between 1 and 200 characters.*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(241)]
    public void Constructor_WithInvalidMinutes_ShouldThrowArgumentException(int invalidMinutes)
    {
        Action act = () => new FocusTask("Valid Name", invalidMinutes);
        act.Should().Throw<ArgumentException>().WithMessage("*Expected minutes must be between 1 and 240.*");
    }

    [Fact]
    public void ChangeStatus_ToInvalidState_ShouldThrowInvalidOperationException()
    {
        var task = new FocusTask("Valid Name", 60); // Starts at Pending
        
        // Cannot jump directly from Pending to Abandoned
        Action act = () => task.ChangeStatus(Domain.Enums.TaskStatus.Abandoned);
        
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Cannot transition task from Pending to Abandoned.*");
    }

    [Fact]
    public void ChangeStatus_ToValidState_ShouldUpdateStatus()
    {
        var task = new FocusTask("Valid Name", 60);
        
        task.ChangeStatus(Domain.Enums.TaskStatus.Active);
        task.Status.Should().Be(Domain.Enums.TaskStatus.Active);
        
        task.ChangeStatus(Domain.Enums.TaskStatus.Done);
        task.Status.Should().Be(Domain.Enums.TaskStatus.Done);
        task.LastDoneDate.Should().HaveValue();
    }

    [Fact]
    public void SetRecurring_WeeklyWithoutConfig_ShouldThrowArgumentException()
    {
        var task = new FocusTask("Valid Name", 60);

        Action act = () => task.SetRecurring(RecurringType.Weekly, "");
        act.Should().Throw<ArgumentException>().WithMessage("*Weekly recurring task must have a day config*");
    }

    [Fact]
    public void SetRecurring_CustomWithInvalidConfig_ShouldThrowArgumentException()
    {
        var task = new FocusTask("Valid Name", 60);

        Action act1 = () => task.SetRecurring(RecurringType.Custom, "abc");
        Action act2 = () => task.SetRecurring(RecurringType.Custom, "-5");

        act1.Should().Throw<ArgumentException>().WithMessage("*Custom recurring task must have a positive integer config for days.*");
        act2.Should().Throw<ArgumentException>().WithMessage("*Custom recurring task must have a positive integer config for days.*");
    }
}
