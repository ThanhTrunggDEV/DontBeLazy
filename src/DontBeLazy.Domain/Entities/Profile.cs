using System;
using System.Collections.Generic;
using DontBeLazy.Domain.ValueObjects;

namespace DontBeLazy.Domain.Entities;

public class Profile
{
    public ProfileId Id { get; private set; }
    public string Name { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation property
    private readonly List<ProfileEntry> _entries = new();
    public IReadOnlyCollection<ProfileEntry> Entries => _entries.AsReadOnly();

    public Profile(string name, bool isDefault)
    {
        Id = ProfileId.New();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Profile name cannot be empty.");

        Name = name;
        IsDefault = isDefault;
        CreatedAt = DateTime.Now;
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Profile name cannot be empty.");

        Name = newName;
        UpdatedAt = DateTime.Now;
    }

    public void AddEntry(ProfileEntry entry)
    {
        if (_entries.Count >= 50)
            throw new InvalidOperationException("A profile can have a maximum of 50 entries.");

        _entries.Add(entry);
        UpdatedAt = DateTime.Now;
    }
    
    public void RemoveEntry(ProfileEntry entry)
    {
        _entries.Remove(entry);
        UpdatedAt = DateTime.Now;
    }

    public void ClearEntries()
    {
        _entries.Clear();
        UpdatedAt = DateTime.Now;
    }

#pragma warning disable CS8618 // EF Core constructor
    private Profile() {}
#pragma warning restore CS8618
}
