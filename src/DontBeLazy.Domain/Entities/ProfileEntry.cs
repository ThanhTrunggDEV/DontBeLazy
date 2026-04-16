using DontBeLazy.Domain.Enums;

namespace DontBeLazy.Domain.Entities;

public class ProfileEntry
{
    public int Id { get; private set; }
    public int ProfileId { get; private set; }
    
    public ProfileEntryType Type { get; private set; }
    public string Value { get; private set; }
    public string? ExePath { get; private set; }

    public ProfileEntry(int profileId, ProfileEntryType type, string value, string? exePath = null)
    {
        ProfileId = profileId;
        Type = type;
        Value = value;
        ExePath = exePath;
    }
}
