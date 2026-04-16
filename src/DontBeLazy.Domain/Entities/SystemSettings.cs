namespace DontBeLazy.Domain.Entities;

public class SystemSettings
{
    public int Id { get; private set; } = 1;
    public bool GlobalStrictMode { get; private set; }
    public bool EnableQuotes { get; private set; }
    public string QuoteLanguage { get; private set; } = "vi";
    public bool DarkTheme { get; private set; }

    public SystemSettings(bool globalStrictMode, bool enableQuotes, string quoteLanguage, bool darkTheme)
    {
        GlobalStrictMode = globalStrictMode;
        EnableQuotes = enableQuotes;
        QuoteLanguage = quoteLanguage;
        DarkTheme = darkTheme;
    }

    public void UpdatePreferences(bool globalStrictMode, bool enableQuotes, string quoteLanguage, bool darkTheme)
    {
        GlobalStrictMode = globalStrictMode;
        EnableQuotes = enableQuotes;
        QuoteLanguage = quoteLanguage;
        DarkTheme = darkTheme;
    }
}
