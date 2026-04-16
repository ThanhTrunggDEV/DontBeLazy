using System;

namespace DontBeLazy.Domain.ValueObjects;

public readonly record struct SessionId(Guid Value)
{
    public static SessionId New() => new(Guid.NewGuid());
    public static readonly SessionId Empty = new(Guid.Empty);
}
