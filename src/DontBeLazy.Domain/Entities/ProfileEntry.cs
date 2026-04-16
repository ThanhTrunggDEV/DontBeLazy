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
        if (string.IsNullOrWhiteSpace(value))
            throw new System.ArgumentException("Value cannot be empty.");

        if (type == ProfileEntryType.Website && (value.Contains("http://") || value.Contains("https://") || value.Contains(" ")))
            throw new System.ArgumentException("Website value must be a clean domain without protocol or spaces (e.g., 'github.com').");

        Id = ProfileEntryId.New();
        ProfileId = profileId;
        Type = type;
        Value = value;
        ExePath = exePath;
    }

    public void Update(ProfileEntryType type, string value, string? exePath = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new System.ArgumentException("Value cannot be empty.");

        if (type == ProfileEntryType.Website && (value.Contains("http://") || value.Contains("https://") || value.Contains(" ")))
            throw new System.ArgumentException("Website value must be a clean domain without protocol or spaces (e.g., 'github.com').");

        Type = type;
        Value = value;
        ExePath = exePath;
    }

#pragma warning disable CS8618 // EF Core constructor
    private ProfileEntry() {}
#pragma warning restore CS8618
}
