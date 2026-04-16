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
    [InlineData("eng")]
    [InlineData("")]
    public void Constructor_WithInvalidLanguage_ShouldThrowArgumentException(string invalidLang)
    {
        Action act = () => new SystemSettings(true, true, invalidLang, true);
        act.Should().Throw<ArgumentException>().WithMessage("*must be 'vi' or 'en'*");
    }

    [Fact]
    public void Constructor_WithValidLanguage_ShouldCreateSettings()
    {
        var settingsVi = new SystemSettings(true, true, "vi", true);
        var settingsEn = new SystemSettings(false, false, "en", false);

        settingsVi.QuoteLanguage.Should().Be("vi");
        settingsEn.QuoteLanguage.Should().Be("en");
    }
}

public class QuoteTests
{
    [Fact]
    public void Constructor_WithEmptyContent_ShouldThrowArgumentException()
    {
        Action act = () => new Quote("   ", "Author", QuoteEventType.PreFocus, "vi", false);
        act.Should().Throw<ArgumentException>().WithMessage("*content cannot be empty*");
    }

    [Fact]
    public void UpdateContent_WhenBundled_ShouldThrowInvalidOperationException()
    {
        var quote = new Quote("Content", "Author", QuoteEventType.PreFocus, "vi", true); // Bundled

        Action act = () => quote.UpdateContent("New", "New Author");
        act.Should().Throw<InvalidOperationException>().WithMessage("*Cannot modify a bundled quote*");
    }
}
