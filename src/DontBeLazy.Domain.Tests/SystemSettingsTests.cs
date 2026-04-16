using System;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace DontBeLazy.Domain.Tests;

public class SystemSettingsTests
{
    [Theory]
    [InlineData("fr")]
    [InlineData("")]
    public void Constructor_WithInvalidLanguage_ShouldThrowArgumentException(string invalidLang)
    {
        Action act = () => new SystemSettings(true, true, invalidLang, true);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithValidLanguage_ShouldCreateSettings()
    {
        var settingsVi = new SystemSettings(true, true, "vi", true);
        settingsVi.QuoteLanguage.Should().Be("vi");
    }

    [Fact]
    public void UpdatePreferences_WithValidLang_ShouldUpdate()
    {
        var settings = new SystemSettings(false, false, "en", false);
        settings.UpdatePreferences(true, true, "vi", true);
        
        settings.QuoteLanguage.Should().Be("vi");
        settings.GlobalStrictMode.Should().BeTrue();
    }
    
    [Fact]
    public void UpdatePreferences_WithInvalidLang_ShouldThrow()
    {
        var settings = new SystemSettings(false, false, "en", false);
        Action act = () => settings.UpdatePreferences(true, true, "fr", true);
        
        act.Should().Throw<ArgumentException>();
    }
}

public class QuoteTests
{
    [Fact]
    public void Constructor_WithEmptyContent_ShouldThrowArgumentException()
    {
        Action act = () => new Quote("   ", "Author", QuoteEventType.PreFocus, "vi", false);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateContent_Valid_ShouldUpdate()
    {
        var quote = new Quote("Content", "Author", QuoteEventType.PreFocus, "vi", false);
        quote.UpdateContent("New", "New Author");
        quote.Content.Should().Be("New");
    }

    [Fact]
    public void UpdateContent_WhenBundled_ShouldThrowInvalidOperationException()
    {
        var quote = new Quote("Content", "Author", QuoteEventType.PreFocus, "vi", true);
        Action act = () => quote.UpdateContent("New", "New Author");
        act.Should().Throw<InvalidOperationException>();
    }
}
