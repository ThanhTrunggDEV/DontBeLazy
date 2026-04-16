using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DontBeLazy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DontBeLazy.WPF.Views;

public partial class SettingsView : UserControl
{
    private UpdateViewModel? _updateVm;

    public SettingsView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<SettingsViewModel>();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            await vm.LoadDataCommand.ExecuteAsync(null);

        // Wire update panel
        _updateVm = App.Services.GetRequiredService<UpdateViewModel>();
        _updateVm.PropertyChanged += OnUpdateVmChanged;
        RefreshUpdateUI();
    }

    private void OnUpdateVmChanged(object? sender, PropertyChangedEventArgs e)
        => Dispatcher.Invoke(RefreshUpdateUI);

    private void RefreshUpdateUI()
    {
        if (_updateVm == null) return;

        CurrentVersionText.Text  = $"Phiên bản hiện tại: v{_updateVm.CurrentVersion}";
        AppInfoText.Text         = $"Don't Be Lazy v{_updateVm.CurrentVersion}";
        UpdateStatusText.Text    = _updateVm.StatusMessage;
        CheckUpdateBtn.IsEnabled = !_updateVm.IsChecking && !_updateVm.IsDownloading;

        DownloadProgress.Visibility  = _updateVm.IsDownloading ? Visibility.Visible : Visibility.Collapsed;
        DownloadProgress.Value       = _updateVm.DownloadProgress;

        ReleaseNotesBorder.Visibility = _updateVm.IsUpdateAvailable ? Visibility.Visible : Visibility.Collapsed;
        ReleaseNotesText.Text         = _updateVm.ReleaseNotes;

        InstallBtn.Visibility   = _updateVm.IsUpdateAvailable && !_updateVm.IsDownloading
                                  ? Visibility.Visible : Visibility.Collapsed;
        InstallBtnText.Text     = _updateVm.IsDownloading
                                  ? $"Đang tải... {_updateVm.DownloadProgress}%"
                                  : $"Tải v{_updateVm.LatestVersion} và cài đặt";

        // Show green status when latest, amber when available
        UpdateStatusText.Foreground = _updateVm.IsUpdateAvailable
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xCC, 0x00))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x9E, 0x9E, 0x9E));
    }

    private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (_updateVm == null) return;
        await _updateVm.CheckForUpdateCommand.ExecuteAsync(null);
    }

    private async void Install_Click(object sender, RoutedEventArgs e)
    {
        if (_updateVm == null) return;
        InstallBtn.IsEnabled = false;
        await _updateVm.DownloadAndInstallCommand.ExecuteAsync(null);
    }
}
