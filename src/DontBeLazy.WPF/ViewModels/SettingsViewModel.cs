using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Ports.Inbound;
using MaterialDesignThemes.Wpf;

namespace DontBeLazy.WPF.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISystemSettingsUseCase _settingsUseCase;

    [ObservableProperty]
    private bool _globalStrictMode;

    [ObservableProperty]
    private bool _enableGuiltTrip;

    [ObservableProperty]
    private string _quoteLanguage = "vi";

    [ObservableProperty]
    private bool _darkTheme = true;

    [ObservableProperty]
    private bool _isLoaded;

    public SettingsViewModel(ISystemSettingsUseCase settingsUseCase)
    {
        _settingsUseCase = settingsUseCase;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        var settings = await _settingsUseCase.GetSettingsAsync();
        GlobalStrictMode = settings.GlobalStrictMode;
        EnableGuiltTrip = settings.EnableQuotes;
        QuoteLanguage = settings.QuoteLanguage ?? "vi";
        DarkTheme = settings.DarkTheme;
        IsLoaded = true;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await _settingsUseCase.UpdateSettingsAsync(GlobalStrictMode, EnableGuiltTrip, QuoteLanguage, DarkTheme);
    }

    partial void OnGlobalStrictModeChanged(bool value) { if (IsLoaded) _ = SaveAsync(); }
    partial void OnEnableGuiltTripChanged(bool value) { if (IsLoaded) _ = SaveAsync(); }
    partial void OnQuoteLanguageChanged(string value) { if (IsLoaded) _ = SaveAsync(); }
    
    partial void OnDarkThemeChanged(bool value) 
    {
        if (IsLoaded) _ = SaveAsync();
        
        var helper = new MaterialDesignThemes.Wpf.PaletteHelper();
        var theme = helper.GetTheme();
        theme.SetBaseTheme(value ? MaterialDesignThemes.Wpf.BaseTheme.Dark : MaterialDesignThemes.Wpf.BaseTheme.Light);
        helper.SetTheme(theme);
    }
}
