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
    public void AddEntry_ExceedingLimit_ShouldThrowInvalidOperationException()
    {
        var profile = new Profile("Limit Test", false);
        
        // Fill to 50
        for(int i = 0; i < 50; i++)
        {
            var entry = new ProfileEntry(profile.Id, ProfileEntryType.Website, $"site{i}.com");
            profile.AddEntry(entry);
        }

        // Try adding 51st
        var extraEntry = new ProfileEntry(profile.Id, ProfileEntryType.Website, "extra.com");
        Action act = () => profile.AddEntry(extraEntry);
        
        act.Should().Throw<InvalidOperationException>().WithMessage("*maximum of 50 entries*");
    }

    [Fact]
    public void ProfileEntry_WithInvalidWebsiteFormat_ShouldThrowArgumentException()
    {
        var profileId = ProfileId.New();

        Action act1 = () => new ProfileEntry(profileId, ProfileEntryType.Website, "http://github.com");
        Action act2 = () => new ProfileEntry(profileId, ProfileEntryType.Website, "https://github.com");
        Action act3 = () => new ProfileEntry(profileId, ProfileEntryType.Website, "space in domain.com");

        act1.Should().Throw<ArgumentException>().WithMessage("*clean domain without protocol or spaces*");
        act2.Should().Throw<ArgumentException>().WithMessage("*clean domain without protocol or spaces*");
        act3.Should().Throw<ArgumentException>().WithMessage("*clean domain without protocol or spaces*");
    }

    [Fact]
    public void ProfileEntry_WithValidWebsiteFormat_ShouldCreateEntry()
    {
        var profileId = ProfileId.New();
        var entry = new ProfileEntry(profileId, ProfileEntryType.Website, "github.com");

        entry.Value.Should().Be("github.com");
    }
}
