using DontBeLazy.Domain.Enums;

namespace DontBeLazy.Domain.Entities;

public class Quote
{
    public int Id { get; private set; }
    public string Content { get; private set; }
    public string? Author { get; private set; }
    public QuoteEventType EventType { get; private set; }
    public string Language { get; private set; }
    public bool IsBundled { get; private set; }

    public Quote(string content, string? author, QuoteEventType eventType, string language, bool isBundled)
    {
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
            
        Content = content;
        Author = author;
    }
}
