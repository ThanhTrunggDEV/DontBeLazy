using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Ports.Inbound;

namespace DontBeLazy.WPF.ViewModels;

public partial class AnalyticsViewModel : ObservableObject
{
    private readonly IAnalyticsUseCase _analyticsUseCase;

    [ObservableProperty]
    private int _currentStreak;

    [ObservableProperty]
    private string _streakLabel = "0 ngày liên tiếp 🔥";

    [ObservableProperty]
    private ObservableCollection<WeekBarItem> _weekBars = new();

    [ObservableProperty]
    private double _totalFocusHoursThisWeek;

    [ObservableProperty]
    private int _totalSessionsThisWeek;

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

        // Build dummy week bars (update if stats DTO defined later)
        WeekBars = new ObservableCollection<WeekBarItem>(
            Enumerable.Range(0, 7).Select(i =>
            {
                var day = monday.AddDays(i);
                return new WeekBarItem
                {
                    DayLabel = day.ToString("ddd"),
                    Hours = day > today ? 0 : Random.Shared.NextDouble() * 4, // Replace with real data
                    IsToday = day.Date == today
                };
            }));

        TotalFocusHoursThisWeek = WeekBars.Sum(b => b.Hours);
        TotalSessionsThisWeek = WeekBars.Count(b => b.Hours > 0);
    }
}

public class WeekBarItem
{
    public string DayLabel { get; set; } = string.Empty;
    public double Hours { get; set; }
    public bool IsToday { get; set; }
    public double BarHeight => Math.Max(Hours * 30, 4);
}
