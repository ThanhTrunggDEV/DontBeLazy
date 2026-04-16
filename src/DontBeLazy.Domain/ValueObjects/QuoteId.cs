using System;

namespace DontBeLazy.Domain.ValueObjects;

public readonly record struct QuoteId(Guid Value)
{
    public static QuoteId New() => new(Guid.NewGuid());
    public static readonly QuoteId Empty = new(Guid.Empty);
}
