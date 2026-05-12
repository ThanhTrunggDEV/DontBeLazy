using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DontBeLazy.Ports.Inbound;

namespace DontBeLazy.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableObject? _currentView;

    [ObservableProperty]
    private int _selectedNavIndex;

    private readonly DashboardViewModel _dashboardVm;
    private readonly ProfilesViewModel _profilesVm;
    private readonly FocusSessionViewModel _focusSessionVm;
    private readonly AnalyticsViewModel _analyticsVm;
    private readonly SettingsViewModel _settingsVm;

    private readonly IFocusSessionUseCase _focusSessionUseCase;

    public MainViewModel(
        IFocusSessionUseCase focusSessionUseCase,
        DashboardViewModel dashboardVm,
        ProfilesViewModel profilesVm,
        FocusSessionViewModel focusSessionVm,
        AnalyticsViewModel analyticsVm,
        SettingsViewModel settingsVm)
    {
        _focusSessionUseCase = focusSessionUseCase;
        _dashboardVm = dashboardVm;
        _profilesVm = profilesVm;
        _focusSessionVm = focusSessionVm;
        _analyticsVm = analyticsVm;
        _settingsVm = settingsVm;

        CurrentView = _dashboardVm;
        SelectedNavIndex = 0;
    }

    [RelayCommand]
    private void NavigateTo(string viewName)
    {
        CurrentView = viewName switch
        {
            "Dashboard" => _dashboardVm,
            "Profiles" => _profilesVm,
            "Focus" => _focusSessionVm,
            "Analytics" => _analyticsVm,
            "Settings" => _settingsVm,
            _ => _dashboardVm
        };
    }

    public async System.Threading.Tasks.Task InitializeAsync()
    {
        var orphanSession = await _focusSessionUseCase.GetIncompleteSessionAsync();
        if (orphanSession != null)
        {
            await _focusSessionUseCase.RestoreSessionAsync(orphanSession.Id);
            // Update focus VM to reflect the restored session
            _focusSessionVm.LoadRestoredSession(orphanSession);
            NavigateTo("Focus");
        }
    }
}
