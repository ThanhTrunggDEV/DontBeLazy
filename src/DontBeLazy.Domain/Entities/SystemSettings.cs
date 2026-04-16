using DontBeLazy.Domain.ValueObjects;

namespace DontBeLazy.Domain.Entities;

public class SystemSettings
{
    public SettingsId Id { get; private set; } = SettingsId.New();
    public bool GlobalStrictMode { get; private set; }
    public bool EnableQuotes { get; private set; }
    public string QuoteLanguage { get; private set; } = "vi";
    public bool DarkTheme { get; private set; }

    public SystemSettings(bool globalStrictMode, bool enableQuotes, string quoteLanguage, bool darkTheme)
    {
        if (quoteLanguage != "vi" && quoteLanguage != "en")
            throw new System.ArgumentException("Quote language must be 'vi' or 'en'.");

        GlobalStrictMode = globalStrictMode;
        EnableQuotes = enableQuotes;
        QuoteLanguage = quoteLanguage;
        DarkTheme = darkTheme;
    }

    public void UpdatePreferences(bool globalStrictMode, bool enableQuotes, string quoteLanguage, bool darkTheme)
    {
        if (quoteLanguage != "vi" && quoteLanguage != "en")
            throw new System.ArgumentException("Quote language must be 'vi' or 'en'.");

        GlobalStrictMode = globalStrictMode;
        EnableQuotes = enableQuotes;
        QuoteLanguage = quoteLanguage;
        DarkTheme = darkTheme;
    }

#pragma warning disable CS8618 // EF Core constructor
    private SystemSettings() {}
#pragma warning restore CS8618
}
