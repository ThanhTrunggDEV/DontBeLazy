using System;
using DontBeLazy.Ports.Outbound.Services;

namespace DontBeLazy.Infrastructure.Services;

/// <summary>
/// Cung cấp thời gian thực dựa trên số tick của CPU kể từ khi khởi động hệ thống.
/// Chống mạo danh thời gian (Time-travel protection) do việc đổi giờ hệ thống không ảnh hưởng đến số tick.
/// </summary>
public class MonotonicClockAdapter : IMonotonicClockPort
{
    public TimeSpan GetTickCount()
    {
        // Environment.TickCount64 trả về số milliseconds từ lúc máy bật.
        return TimeSpan.FromMilliseconds(Environment.TickCount64);
    }
}
