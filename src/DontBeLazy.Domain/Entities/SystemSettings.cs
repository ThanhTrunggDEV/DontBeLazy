using DontBeLazy.Domain.ValueObjects;

namespace DontBeLazy.Domain.Entities;

public class SystemSettings
{
    public SettingsId Id { get; private set; } = SettingsId.New();
    public bool GlobalStrictMode { get; private set; }
    public bool EnableQuotes { get; private set; }
    public string QuoteLanguage { get; private set; } = "vi";
    public bool DarkTheme { get; private set; }
    public string? GeminiApiKey { get; private set; }
    public string GeminiModel { get; private set; } = "gemini-2.5-flash";

    public SystemSettings(bool globalStrictMode, bool enableQuotes, string quoteLanguage, bool darkTheme,
        string? geminiApiKey = null, string geminiModel = "gemini-2.5-flash")
    {
        if (quoteLanguage != "vi" && quoteLanguage != "en")
            throw new System.ArgumentException("Quote language must be 'vi' or 'en'.");

        GlobalStrictMode = globalStrictMode;
        EnableQuotes = enableQuotes;
        QuoteLanguage = quoteLanguage;
        DarkTheme = darkTheme;
        GeminiApiKey = geminiApiKey;
        GeminiModel = geminiModel;
    }

    public void UpdatePreferences(bool globalStrictMode, bool enableQuotes, string quoteLanguage, bool darkTheme,
        string? geminiApiKey = null, string geminiModel = "gemini-2.5-flash")
    {
        if (quoteLanguage != "vi" && quoteLanguage != "en")
            throw new System.ArgumentException("Quote language must be 'vi' or 'en'.");

        GlobalStrictMode = globalStrictMode;
        EnableQuotes = enableQuotes;
        QuoteLanguage = quoteLanguage;
        DarkTheme = darkTheme;
        GeminiApiKey = geminiApiKey;
        GeminiModel = geminiModel;
    }

#pragma warning disable CS8618 // EF Core constructor
    private SystemSettings() {}
#pragma warning restore CS8618
}
