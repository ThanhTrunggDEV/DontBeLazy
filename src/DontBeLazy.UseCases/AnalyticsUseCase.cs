using System;
using System.Linq;
using System.Threading.Tasks;
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

    public async Task<object> GetDashboardStatsAsync(DateTime startDate, DateTime endDate)
    {
        var sessions = await _sessionRepository.GetSessionsByDateRangeAsync(startDate, endDate);
        
        var completedSessions = sessions.Where(s => s.CompletionStatus == DontBeLazy.Domain.Enums.CompletionStatus.Completed).ToList();
        
        return new 
        {
            TotalFocusTimeSeconds = completedSessions.Sum(s => s.ActualSeconds),
            TotalSessions = sessions.Count,
            CompletedSessions = completedSessions.Count,
            AbandonedSessions = sessions.Count(s => s.CompletionStatus == DontBeLazy.Domain.Enums.CompletionStatus.Abandoned),
            TotalBlockedAttempts = sessions.Sum(s => s.BlockedCount)
        };
    }

    public async Task<int> GetCurrentStreakAsync()
    {
        var recentSessions = await _sessionRepository.GetRecentSessionsAsync(100);
        
        int streak = 0;
        var checkDate = DateTime.Now.Date;
        
        var sessionDates = recentSessions
            .Where(s => s.CompletionStatus == DontBeLazy.Domain.Enums.CompletionStatus.Completed)
            .Select(s => s.FocusStartDate.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        // Very basic streak logic
        var expectedDate = checkDate;
        
        foreach (var date in sessionDates)
        {
            // If date is today, or date is yesterday and we haven't found today yet (streak=0)
            if (date == expectedDate || (date == checkDate.AddDays(-1) && streak == 0))
            {
                streak++;
                expectedDate = date.AddDays(-1); // next expected date is the day before the matched date
            }
            else
            {
                break;
            }
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
        
        foreach(var s in sessions)
        {
            sb.AppendLine($"{s.Id.Value},{s.SnapshotTaskName},{s.FocusStartDate:yyyy-MM-dd HH:mm:ss},{s.ActualSeconds},{s.CompletionStatus},{s.BlockedCount}");
        }
        
        return sb.ToString();
    }
}
