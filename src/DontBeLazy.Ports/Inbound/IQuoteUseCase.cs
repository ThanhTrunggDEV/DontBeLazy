using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;

namespace DontBeLazy.Ports.Inbound;

public interface IQuoteUseCase
{
    Task<IReadOnlyCollection<Quote>> GetAllQuotesAsync();
    Task<Quote> GetQuoteForEventAsync(DontBeLazy.Domain.Enums.QuoteEventType eventType, string language);
    Task<Quote> AddQuoteAsync(string content, string author, DontBeLazy.Domain.Enums.QuoteEventType type, string lang);
    Task UpdateQuoteAsync(DontBeLazy.Domain.ValueObjects.QuoteId quoteId, string newContent, string newAuthor);
    Task DeleteQuoteAsync(DontBeLazy.Domain.ValueObjects.QuoteId quoteId);
}
