using System;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace DontBeLazy.Domain.Tests;

public class ProfileTests
{
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException(string? invalidName)
    {
        Action act = () => new Profile(invalidName, false);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateName_Valid_ShouldUpdate()
    {
        var p = new Profile("Old", false);
        p.UpdateName("New");
        p.Name.Should().Be("New");
    }

    [Fact]
    public void UpdateName_Invalid_ShouldThrow()
    {
        var p = new Profile("Old", false);
        Action act = () => p.UpdateName("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddRemoveClear_ShouldWork()
    {
        var profile = new Profile("Test", false);
        var entry = new ProfileEntry(profile.Id, ProfileEntryType.App, "code.exe");
        
        profile.AddEntry(entry);
        profile.Entries.Should().Contain(entry);
        
        profile.RemoveEntry(entry);
        profile.Entries.Should().BeEmpty();
        
        profile.AddEntry(entry);
        profile.ClearEntries();
        profile.Entries.Should().BeEmpty();
    }

    [Fact]
    public void AddEntry_ExceedingLimit_ShouldThrowInvalidOperationException()
    {
        var profile = new Profile("Limit Test", false);
        for(int i = 0; i < 50; i++)
            profile.AddEntry(new ProfileEntry(profile.Id, ProfileEntryType.Website, $"site{i}.com"));

        var extraEntry = new ProfileEntry(profile.Id, ProfileEntryType.Website, "extra.com");
        Action act = () => profile.AddEntry(extraEntry);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ProfileEntry_WithInvalidWebsiteFormat_ShouldThrowArgumentException()
    {
        var profileId = ProfileId.New();
        Action act1 = () => new ProfileEntry(profileId, ProfileEntryType.Website, "http://github.com");
        act1.Should().Throw<ArgumentException>();
    }
}
