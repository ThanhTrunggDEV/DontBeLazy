using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;

namespace DontBeLazy.Ports.Inbound;

public interface IAnalyticsUseCase
{
    // Lấy thống kê cho Dashboard (UC05)
    Task<object> GetDashboardStatsAsync(DateTime startDate, DateTime endDate); // Temporary using object, later can map to DTO
    Task<int> GetCurrentStreakAsync();
    
    // Quản lý dữ liệu session
    Task DeleteSessionHistoryAsync(DateTime startDate, DateTime endDate);
    Task<string> ExportSessionsToCsvAsync();
}
