using System;

namespace DontBeLazy.Domain.ValueObjects;

public readonly record struct TaskId(Guid Value)
{
    public static TaskId New() => new(Guid.NewGuid());
    public static readonly TaskId Empty = new(Guid.Empty);
}
