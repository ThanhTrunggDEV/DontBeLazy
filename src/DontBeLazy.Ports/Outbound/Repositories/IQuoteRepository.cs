using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;

namespace DontBeLazy.Ports.Outbound.Repositories;

public interface IQuoteRepository
{
    Task<IReadOnlyCollection<Quote>> GetAllAsync();
    Task<IReadOnlyCollection<Quote>> GetByEventTypeAsync(QuoteEventType eventType, string language);
    Task<Quote?> GetByIdAsync(QuoteId id);
    Task AddAsync(Quote quote);
    Task UpdateAsync(Quote quote);
    Task DeleteAsync(QuoteId id);
}
