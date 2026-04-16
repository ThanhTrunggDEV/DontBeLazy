using System;
using System.Linq;
using System.Threading.Tasks;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Ports.DTOs;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Repositories;

namespace DontBeLazy.UseCases;

public class AnalyticsUseCase : IAnalyticsUseCase
{
    private readonly ISessionRepository _sessionRepository;

    public AnalyticsUseCase(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(DateTime startDate, DateTime endDate)
    {
        var sessions = await _sessionRepository.GetSessionsByDateRangeAsync(startDate, endDate);
        var completed = sessions.Where(s => s.CompletionStatus == CompletionStatus.Completed).ToList();

        // Build daily stats for the range
        var days = Enumerable.Range(0, (endDate.Date - startDate.Date).Days)
            .Select(i => startDate.Date.AddDays(i))
            .ToList();

        var dailyStats = days.Select(day =>
        {
            var daySessions = completed.Where(s => s.FocusStartDate.Date == day).ToList();
            var hours = daySessions.Sum(s => s.ActualSeconds) / 3600.0;
            return new DayStatDto(
                day.ToString("ddd"),
                hours,
                day == DateTime.Today
            );
        }).ToList();

        return new DashboardStatsDto(
            TotalFocusHoursThisWeek: completed.Sum(s => s.ActualSeconds) / 3600.0,
            TotalSessionsThisWeek: sessions.Count,
            CurrentStreak: await GetCurrentStreakAsync(),
            DailyStats: dailyStats
        );
    }

    public async Task<int> GetCurrentStreakAsync()
    {
        var recentSessions = await _sessionRepository.GetRecentSessionsAsync(100);

        var sessionDates = recentSessions
            .Where(s => s.CompletionStatus == CompletionStatus.Completed)
            .Select(s => s.FocusStartDate.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        int streak = 0;
        var expectedDate = DateTime.Today;

        foreach (var date in sessionDates)
        {
            if (date == expectedDate || (date == DateTime.Today.AddDays(-1) && streak == 0))
            {
                streak++;
                expectedDate = date.AddDays(-1);
            }
            else break;
        }

        return streak;
    }

    public async Task DeleteSessionHistoryAsync(DateTime startDate, DateTime endDate)
    {
        await _sessionRepository.DeleteByDateRangeAsync(startDate, endDate);
    }

    public async Task<string> ExportSessionsToCsvAsync()
    {
        var sessions = await _sessionRepository.GetRecentSessionsAsync(1000);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("SessionId,TaskName,StartDate,ActualSeconds,Status,BlockedCount");

        foreach (var s in sessions)
            sb.AppendLine($"{s.Id.Value},{s.SnapshotTaskName},{s.FocusStartDate:yyyy-MM-dd HH:mm:ss},{s.ActualSeconds},{s.CompletionStatus},{s.BlockedCount}");

        return sb.ToString();
    }
}
