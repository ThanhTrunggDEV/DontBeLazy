using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Ports.DTOs;

namespace DontBeLazy.UseCases.Mappers;

/// <summary>
/// Maps domain entities → DTOs. Keeps mapping logic in one place (SRP).
/// </summary>
internal static class DtoMapper
{
    public static FocusTaskDto ToDto(FocusTask task) => new(
        task.Id.Value,
        task.Name,
        task.ExpectedMinutes,
        (TaskStatusDto)task.Status,
        task.ProfileId?.Value,
        null,           // ProfileName resolved by caller if needed
        task.IsPaused,
        task.SortOrder
    );

    public static ProfileDto ToDto(Profile profile) => new(
        profile.Id.Value,
        profile.Name,
        profile.IsDefault,
        profile.Entries.Select(ToDto).ToList()
    );

    public static ProfileEntryDto ToDto(ProfileEntry entry) => new(
        entry.Id.Value,
        entry.ProfileId.Value,
        (ProfileEntryTypeDto)entry.Type,
        entry.Value,
        entry.ExePath
    );

    public static SessionHistoryDto ToDto(SessionHistory session) => new(
        session.Id.Value,
        session.SnapshotTaskName,
        session.ExpectedSeconds,
        session.ActualSeconds,
        session.CompletionStatus.HasValue ? (CompletionStatusDto)session.CompletionStatus.Value : null,
        session.FocusStartDate
    );

    public static SystemSettingsDto ToDto(SystemSettings settings) => new(
        settings.GlobalStrictMode,
        settings.EnableQuotes,
        settings.QuoteLanguage,
        settings.DarkTheme
    );

    public static QuoteDto ToDto(Quote quote) => new(
        quote.Id.Value,
        quote.Content,
        quote.Author,
        (QuoteEventTypeDto)quote.EventType,
        quote.Language
    );

    // Reverse: DTO enums → Domain enums
    public static Domain.Enums.TaskStatus ToDomain(TaskStatusDto dto) => (Domain.Enums.TaskStatus)dto;
    public static ProfileEntryType ToDomain(ProfileEntryTypeDto dto)   => (ProfileEntryType)dto;
    public static CompletionStatus ToDomain(CompletionStatusDto dto)   => (CompletionStatus)dto;
    public static QuoteEventType   ToDomain(QuoteEventTypeDto dto)     => (QuoteEventType)dto;
}
