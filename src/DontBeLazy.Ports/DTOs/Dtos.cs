namespace DontBeLazy.Ports.DTOs;

public record FocusTaskDto(
    Guid Id,
    string Name,
    int ExpectedMinutes,
    TaskStatusDto Status,
    Guid? ProfileId,
    string? ProfileName,
    bool IsPaused,
    int SortOrder
);

public record ProfileDto(
    Guid Id,
    string Name,
    bool IsDefault,
    IReadOnlyList<ProfileEntryDto> Entries
);

public record ProfileEntryDto(
    Guid Id,
    Guid ProfileId,
    ProfileEntryTypeDto Type,
    string Value,
    string? ExePath
);

public record SessionHistoryDto(
    Guid Id,
    string TaskName,
    int ExpectedSeconds,
    int ActualSeconds,
    CompletionStatusDto? CompletionStatus,
    DateTime CreatedAt
);

public record SystemSettingsDto(
    bool GlobalStrictMode,
    bool EnableQuotes,
    string QuoteLanguage,
    bool DarkTheme
);

public record QuoteDto(
    Guid Id,
    string Content,
    string Author,
    QuoteEventTypeDto EventType,
    string Language
);

public record DashboardStatsDto(
    double TotalFocusHoursThisWeek,
    int TotalSessionsThisWeek,
    int CurrentStreak,
    IReadOnlyList<DayStatDto> DailyStats
);

public record DayStatDto(
    string DayLabel,
    double Hours,
    bool IsToday
);
