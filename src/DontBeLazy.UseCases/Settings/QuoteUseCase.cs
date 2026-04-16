using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Repositories;

namespace DontBeLazy.UseCases.Settings;

public class QuoteUseCase : IQuoteUseCase
{
    private readonly IQuoteRepository _quoteRepository;

    public QuoteUseCase(IQuoteRepository quoteRepository)
    {
        _quoteRepository = quoteRepository;
    }

    public async Task<IReadOnlyCollection<Quote>> GetAllQuotesAsync()
    {
        return await _quoteRepository.GetAllAsync();
    }

    public async Task<Quote> GetQuoteForEventAsync(QuoteEventType eventType, string language)
    {
        var quotes = await _quoteRepository.GetByEventTypeAsync(eventType, language);
        if (!quotes.Any())
            throw new KeyNotFoundException($"No quotes found for event {eventType} in language '{language}'.");

        // Pick a random quote
        var rand = new Random();
        var selected = quotes.ElementAt(rand.Next(quotes.Count));
        return selected;
    }

    public async Task<Quote> AddQuoteAsync(string content, string author, QuoteEventType type, string lang)
    {
        var quote = new Quote(content, author, type, lang, isBundled: false);
        await _quoteRepository.AddAsync(quote);
        return quote;
    }

    public async Task UpdateQuoteAsync(QuoteId quoteId, string newContent, string newAuthor)
    {
        var quote = await _quoteRepository.GetByIdAsync(quoteId);
        if (quote == null)
            throw new KeyNotFoundException($"Quote {quoteId} not found.");

        quote.UpdateContent(newContent, newAuthor);
        await _quoteRepository.UpdateAsync(quote);
    }

    public async Task DeleteQuoteAsync(QuoteId quoteId)
    {
        var quote = await _quoteRepository.GetByIdAsync(quoteId);
        if (quote == null)
            throw new KeyNotFoundException($"Quote {quoteId} not found.");
            
        // Can throw error if quote is bundled, but Repository doesn't enforce, we can do it here:
        if (quote.IsBundled)
            throw new InvalidOperationException("Cannot delete a bundled quote.");

        await _quoteRepository.DeleteAsync(quoteId);
    }
}
