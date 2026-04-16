using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;

namespace DontBeLazy.Domain.Entities;

public class SessionProfileSnapshot
{
    public SnapshotId Id { get; private set; }
    public SessionId SessionId { get; private set; }
    public ProfileEntryType Type { get; private set; }
    public string Value { get; private set; }
    public string? ExePath { get; private set; }

    public SessionProfileSnapshot(SessionId sessionId, ProfileEntryType type, string value, string? exePath = null)
    {
        Id = SnapshotId.New();
        SessionId = sessionId;
        Type = type;
        Value = value;
        ExePath = exePath;
    }

#pragma warning disable CS8618 // EF Core constructor
    private SessionProfileSnapshot() {}
#pragma warning restore CS8618
}
