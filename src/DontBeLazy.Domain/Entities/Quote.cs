using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;

namespace DontBeLazy.Domain.Entities;

public class Quote
{
    public QuoteId Id { get; private set; }
    public string Content { get; private set; }
    public string? Author { get; private set; }
    public QuoteEventType EventType { get; private set; }
    public string Language { get; private set; }
    public bool IsBundled { get; private set; }

    public Quote(string content, string? author, QuoteEventType eventType, string language, bool isBundled)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new System.ArgumentException("Quote content cannot be empty.");

        Id = QuoteId.New();
        Content = content;
        Author = author;
        EventType = eventType;
        Language = language;
        IsBundled = isBundled;
    }

    public void UpdateContent(string content, string? author)
    {
        if (IsBundled)
            throw new System.InvalidOperationException("Cannot modify a bundled quote.");
        
        if (string.IsNullOrWhiteSpace(content))
            throw new System.ArgumentException("Quote content cannot be empty.");
            
        Content = content;
        Author = author;
    }

#pragma warning disable CS8618 // EF Core constructor
    private Quote() {}
#pragma warning restore CS8618
}
