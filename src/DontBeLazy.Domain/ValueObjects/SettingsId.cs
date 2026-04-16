using System;

namespace DontBeLazy.Domain.ValueObjects;

public readonly record struct SettingsId(Guid Value)
{
    public static SettingsId New() => new(Guid.NewGuid());
    public static readonly SettingsId Empty = new(Guid.Empty);
}
