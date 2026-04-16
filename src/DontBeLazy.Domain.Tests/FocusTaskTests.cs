using System;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;
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
        task.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
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
    public void UpdateDetails_WithValidData_ShouldUpdate()
    {
        var task = new FocusTask("Task", 60);
        var pId = ProfileId.New();

        task.UpdateDetails("New Task", 120, pId, true);

        task.Name.Should().Be("New Task");
        task.ExpectedMinutes.Should().Be(120);
        task.ProfileId.Should().Be(pId);
        task.PerTaskStrictMode.Should().BeTrue();
        task.UpdatedAt.Should().HaveValue();
    }

    [Fact]
    public void UpdateDetails_WithInvalidData_ShouldThrow()
    {
        var task = new FocusTask("Task", 60);

        Action act1 = () => task.UpdateDetails("", 60, null, null);
        Action act2 = () => task.UpdateDetails("Test", 250, null, null);

        act1.Should().Throw<ArgumentException>();
        act2.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ChangeStatus_ToInvalidState_ShouldThrowInvalidOperationException()
    {
        var task = new FocusTask("Valid Name", 60);
        Action act = () => task.ChangeStatus(Domain.Enums.TaskStatus.Abandoned);
        
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ChangeStatus_ToValidState_ShouldUpdateStatus()
    {
        var task = new FocusTask("Valid Name", 60);
        task.ChangeStatus(Domain.Enums.TaskStatus.Active);
        task.ChangeStatus(Domain.Enums.TaskStatus.Done);
        
        task.Status.Should().Be(Domain.Enums.TaskStatus.Done);
        task.LastDoneDate.Should().HaveValue();
    }

    [Fact]
    public void SetRecurring_ValidConfig_ShouldUpdate()
    {
        var task = new FocusTask("Valid Name", 60);
        task.SetRecurring(RecurringType.Daily, null);

        task.TaskType.Should().Be(TaskType.Recurring);
        task.RecurringType.Should().Be(RecurringType.Daily);
    }

    [Fact]
    public void SetRecurring_WeeklyWithoutConfig_ShouldThrowArgumentException()
    {
        var task = new FocusTask("Valid Name", 60);
        Action act = () => task.SetRecurring(RecurringType.Weekly, "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetRecurring_CustomWithInvalidConfig_ShouldThrowArgumentException()
    {
        var task = new FocusTask("Valid Name", 60);
        Action act1 = () => task.SetRecurring(RecurringType.Custom, "abc");
        act1.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateSortOrder_ShouldUpdate()
    {
        var task = new FocusTask("Test", 60);
        task.UpdateSortOrder(5);
        task.SortOrder.Should().Be(5);
    }

    [Fact]
    public void SetPaused_ShouldUpdate()
    {
        var task = new FocusTask("Test", 60);
        task.SetPaused(true);
        task.IsPaused.Should().BeTrue();
    }
}
