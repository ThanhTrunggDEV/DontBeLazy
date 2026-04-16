using System;

namespace DontBeLazy.Ports.Outbound.Services;

/// <summary>
/// Port cung cấp quyền truy cập vào thời gian thực dựa trên phần cứng (tránh bị thao túng bởi người dùng đổi giờ OS).
/// Sử dụng cho tính năng Time-Travel Protection (UC03).
/// </summary>
public interface IMonotonicClockPort
{
    /// <summary>
    /// Lấy giá trị tổng thời gian (tick system) đã trôi qua kể từ khi máy tính khởi động.
    /// Giá trị này chỉ thay đổi tịnh tiến, không bị ảnh hưởng bởi việc đổi Timezone hoặc Clock OS.
    /// </summary>
    TimeSpan GetTickCount();
}
