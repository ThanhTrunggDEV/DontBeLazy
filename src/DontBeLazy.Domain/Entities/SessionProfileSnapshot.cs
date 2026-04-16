using DontBeLazy.Domain.Enums;

namespace DontBeLazy.Domain.Entities;

public class SessionProfileSnapshot
{
    public int Id { get; private set; }
    public int SessionId { get; private set; }
    public ProfileEntryType Type { get; private set; }
    public string Value { get; private set; }
    public string? ExePath { get; private set; }

    public SessionProfileSnapshot(int sessionId, ProfileEntryType type, string value, string? exePath = null)
    {
        SessionId = sessionId;
        Type = type;
        Value = value;
        ExePath = exePath;
    }
}
