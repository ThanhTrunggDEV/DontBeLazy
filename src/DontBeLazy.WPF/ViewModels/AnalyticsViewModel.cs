using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DontBeLazy.Ports.DTOs;
using DontBeLazy.Ports.Inbound;

namespace DontBeLazy.WPF.ViewModels;

public partial class AnalyticsViewModel : ObservableObject
{
    private readonly IAnalyticsUseCase _analyticsUseCase;

    [ObservableProperty] private int _currentStreak;
    [ObservableProperty] private string _streakLabel = "0 ngày liên tiếp 🔥";
    [ObservableProperty] private ObservableCollection<DayStatDto> _weekBars = new();
    [ObservableProperty] private double _totalFocusHoursThisWeek;
    [ObservableProperty] private int _totalSessionsThisWeek;

    public AnalyticsViewModel(IAnalyticsUseCase analyticsUseCase)
    {
        _analyticsUseCase = analyticsUseCase;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        CurrentStreak = await _analyticsUseCase.GetCurrentStreakAsync();
        StreakLabel = CurrentStreak > 0
            ? $"{CurrentStreak} ngày liên tiếp 🔥"
            : "Chưa có streak – bắt đầu ngay hôm nay!";

        var today = DateTime.Today;
        var monday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        var stats = await _analyticsUseCase.GetDashboardStatsAsync(monday, today.AddDays(1));

        WeekBars = new ObservableCollection<DayStatDto>(stats.DailyStats);
        TotalFocusHoursThisWeek = stats.TotalFocusHoursThisWeek;
        TotalSessionsThisWeek = stats.TotalSessionsThisWeek;
    }
}
