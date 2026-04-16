using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DontBeLazy.WPF.Services;

namespace DontBeLazy.WPF.ViewModels;

public partial class UpdateViewModel : ObservableObject
{
    private readonly UpdaterService _updater;

    [ObservableProperty] private bool _isChecking;
    [ObservableProperty] private bool _isUpdateAvailable;
    [ObservableProperty] private bool _isDownloading;
    [ObservableProperty] private int  _downloadProgress;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private string _latestVersion = string.Empty;
    [ObservableProperty] private string _releaseNotes  = string.Empty;

    private ReleaseInfo? _pendingRelease;

    public string CurrentVersion => UpdaterService.CurrentVersion;

    public UpdateViewModel(UpdaterService updater)
    {
        _updater = updater;
    }

    [RelayCommand]
    public async Task CheckForUpdateAsync()
    {
        IsChecking      = true;
        IsUpdateAvailable = false;
        StatusMessage   = "Đang kiểm tra cập nhật...";
        try
        {
            _pendingRelease = await _updater.CheckForUpdateAsync();
            if (_pendingRelease != null)
            {
                LatestVersion     = _pendingRelease.Version;
                ReleaseNotes      = _pendingRelease.ReleaseNotes;
                IsUpdateAvailable = true;
                StatusMessage     = $"Phiên bản mới: v{_pendingRelease.Version}";
            }
            else
            {
                StatusMessage = $"Đang dùng phiên bản mới nhất (v{CurrentVersion})";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi kiểm tra: {ex.Message}";
        }
        finally
        {
            IsChecking = false;
        }
    }

    [RelayCommand]
    public async Task DownloadAndInstallAsync()
    {
        if (_pendingRelease == null) return;
        IsDownloading   = true;
        StatusMessage   = "Đang tải về...";
        DownloadProgress = 0;

        var progress = new Progress<int>(p =>
        {
            DownloadProgress = p;
            StatusMessage    = $"Đang tải... {p}%";
        });

        try
        {
            await _updater.DownloadAndApplyAsync(_pendingRelease, progress,
                beforeRestart: () => StatusMessage = "Đang cài đặt và khởi động lại...");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi cài đặt: {ex.Message}";
            IsDownloading = false;
        }
    }
}
