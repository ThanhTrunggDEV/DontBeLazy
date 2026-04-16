using System.Threading.Tasks;

namespace DontBeLazy.Ports.Outbound.Services;

/// <summary>
/// Cổng giao tiếp với các dịch vụ AI (OpenAI, Gemini...) để tạo ra các tính năng thông minh.
/// Ví dụ: Tạo ra các lời lẽ "đe dọa" hoặc "đánh vào lòng tự trọng" tùy theo task người dùng định từ bỏ.
/// </summary>
public interface IAiMotivatorPort
{
    /// <summary>
    /// Gửi thông tin Task và lịch sử lười biếng để AI sinh ra một câu nói sát thương tâm lý (Guilt-trip).
    /// </summary>
    /// <param name="taskName">Tên công việc đang định từ bỏ</param>
    /// <param name="language">Ngôn ngữ (vi, en)</param>
    /// <returns>Câu quote được AI sinh ra</returns>
    Task<string> GenerateGuiltTripQuoteAsync(string taskName, string language);

    /// <summary>
    /// Dựa vào danh sách task trong ngày để AI gợi ý thứ tự làm việc (Sorting).
    /// </summary>
    Task<string> AnalyzeAndSuggestTaskPriorityAsync(string dailyTasksDump);
}
