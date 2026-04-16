using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Ports.DTOs;

namespace DontBeLazy.Ports.Inbound;

public interface IQuoteUseCase
{
    Task<IReadOnlyCollection<QuoteDto>> GetAllQuotesAsync();
    Task<QuoteDto?> GetQuoteForEventAsync(QuoteEventTypeDto eventType, string language);
    Task<string> GenerateAiGuiltTripAsync(string taskName, string language);
    Task<QuoteDto> AddQuoteAsync(string content, string author, QuoteEventTypeDto type, string lang);
    Task UpdateQuoteAsync(Guid quoteId, string newContent, string newAuthor);
    Task DeleteQuoteAsync(Guid quoteId);
}
