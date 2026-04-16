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

    private bool _globalStrictMode;
    public bool GlobalStrictMode
    {
        get => _globalStrictMode;
        set
        {
            if (_globalStrictMode == value) return;
            
            // Intercept turning OFF strict mode from UI
            if (IsLoaded && !value && !_isInternalSet)
            {
                OnPropertyChanged(nameof(GlobalStrictMode)); // revert UI visually
                FrictionInput = string.Empty;
                IsStrictConfirmDialogOpen = true;
                return;
            }

            SetProperty(ref _globalStrictMode, value);
            if (IsLoaded && !_isInternalSet) _ = SaveAsync();
        }
    }

    private bool _isInternalSet;

    [ObservableProperty] private bool _isStrictConfirmDialogOpen;
    [ObservableProperty] private string _frictionInput = string.Empty;

    public bool CanConfirmDisableStrict => FrictionInput?.Trim() == "Tôi chấp nhận giảm mức độ kỷ luật";
    
    partial void OnFrictionInputChanged(string value) => OnPropertyChanged(nameof(CanConfirmDisableStrict));

    [RelayCommand]
    private async Task ConfirmDisableStrictAsync()
    {
        if (!CanConfirmDisableStrict) return;
        _isInternalSet = true;
        GlobalStrictMode = false;
        _isInternalSet = false;
        IsStrictConfirmDialogOpen = false;
        await SaveAsync();
    }

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
        _isInternalSet = true;
        GlobalStrictMode = settings.GlobalStrictMode;
        _isInternalSet = false;
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
