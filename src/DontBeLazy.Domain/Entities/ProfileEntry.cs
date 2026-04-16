using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;

namespace DontBeLazy.Domain.Entities;

public class ProfileEntry
{
    public ProfileEntryId Id { get; private set; }
    public ProfileId ProfileId { get; private set; }
    
    public ProfileEntryType Type { get; private set; }
    public string Value { get; private set; }
    public string? ExePath { get; private set; }

    public ProfileEntry(ProfileId profileId, ProfileEntryType type, string value, string? exePath = null)
    {
        Id = ProfileEntryId.New();
        ProfileId = profileId;
        Type = type;
        Value = value;
        ExePath = exePath;
    }
}
