using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace DontBeLazy.Domain.Tests;

public class ValueObjectTests
{
    [Fact]
    public void TaskId_New_ShouldCreateUniqueIds()
    {
        var id1 = TaskId.New();
        var id2 = TaskId.New();
        
        id1.Should().NotBe(id2);
        id1.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void OtherIds_New_ShouldCreateUniqueIds()
    {
        ProfileId.New().Should().NotBe(ProfileId.New());
        ProfileEntryId.New().Should().NotBe(ProfileEntryId.New());
        SessionId.New().Should().NotBe(SessionId.New());
    }

    [Fact]
    public void Snapshot_Constructor_ShouldSetProperties()
    {
        var sId = SessionId.New();
        var snap = new SessionProfileSnapshot(sId, ProfileEntryType.Website, "test", "path");

        snap.SessionId.Should().Be(sId);
        snap.Type.Should().Be(ProfileEntryType.Website);
        snap.Value.Should().Be("test");
        snap.ExePath.Should().Be("path");
    }
}
