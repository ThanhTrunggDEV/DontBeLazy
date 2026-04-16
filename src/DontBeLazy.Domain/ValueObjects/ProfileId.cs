using System;

namespace DontBeLazy.Domain.ValueObjects;

public readonly record struct ProfileId(Guid Value)
{
    public static ProfileId New() => new(Guid.NewGuid());
    public static readonly ProfileId Empty = new(Guid.Empty);
}
