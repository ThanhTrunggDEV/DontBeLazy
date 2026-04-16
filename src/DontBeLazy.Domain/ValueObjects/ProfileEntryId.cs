using System;

namespace DontBeLazy.Domain.ValueObjects;

public readonly record struct ProfileEntryId(Guid Value)
{
    public static ProfileEntryId New() => new(Guid.NewGuid());
    public static readonly ProfileEntryId Empty = new(Guid.Empty);
}
