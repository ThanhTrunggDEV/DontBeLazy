using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Ports.DTOs;

namespace DontBeLazy.Ports.Inbound;

public interface IAnalyticsUseCase
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(DateTime startDate, DateTime endDate);
    Task<int> GetCurrentStreakAsync();
    Task DeleteSessionHistoryAsync(DateTime startDate, DateTime endDate);
    Task<string> ExportSessionsToCsvAsync();
}
