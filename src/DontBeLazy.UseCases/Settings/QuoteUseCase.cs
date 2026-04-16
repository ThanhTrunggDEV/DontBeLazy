using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.ValueObjects;
using DontBeLazy.Ports.DTOs;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Repositories;
using DontBeLazy.Ports.Outbound.Services;
using DontBeLazy.UseCases.Mappers;

namespace DontBeLazy.UseCases.Settings;

public class QuoteUseCase : IQuoteUseCase
{
    private readonly IQuoteRepository _quoteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiGuiltTripPort _aiGuiltTrip;

    public QuoteUseCase(
        IQuoteRepository quoteRepository,
        IUnitOfWork unitOfWork,
        IAiGuiltTripPort aiGuiltTrip)
    {
        _quoteRepository = quoteRepository;
        _unitOfWork = unitOfWork;
        _aiGuiltTrip = aiGuiltTrip;
    }

    public async Task<IReadOnlyCollection<QuoteDto>> GetAllQuotesAsync()
    {
        var quotes = await _quoteRepository.GetAllAsync();
        return quotes.Select(DtoMapper.ToDto).ToList();
    }

    public async Task<QuoteDto?> GetQuoteForEventAsync(QuoteEventTypeDto eventType, string language)
    {
        var quotes = await _quoteRepository.GetByEventTypeAsync(DtoMapper.ToDomain(eventType), language);
        if (!quotes.Any()) return null;
        var selected = quotes.ElementAt(new Random().Next(quotes.Count));
        return DtoMapper.ToDto(selected);
    }

    public async Task<string> GenerateAiGuiltTripAsync(string taskName, string language)
    {
        try
        {
            var quote = await _aiGuiltTrip.GenerateGuiltTripQuoteAsync(taskName, language);
            if (!string.IsNullOrWhiteSpace(quote) && !quote.StartsWith("[AI MOCK]"))
                return quote;
        }
        catch { /* fallback to DB */ }

        // Fallback: pick random DB quote
        var quotes = await _quoteRepository.GetByEventTypeAsync(
            DontBeLazy.Domain.Enums.QuoteEventType.GiveUp, language);
        if (quotes.Any())
            return quotes.ElementAt(new Random().Next(quotes.Count)).Content;

        return language == "vi"
            ? $"Bạn chỉ cần cố thêm một chút nữa thôi! Đừng bỏ cuộc khi đang làm '{taskName}'."
            : $"You're so close! Don't give up on '{taskName}' now.";
    }

    public async Task<QuoteDto> AddQuoteAsync(string content, string author, QuoteEventTypeDto type, string lang)
    {
        var quote = new Quote(content, author, DtoMapper.ToDomain(type), lang, isBundled: false);
        await _quoteRepository.AddAsync(quote);
        await _unitOfWork.SaveChangesAsync();
        return DtoMapper.ToDto(quote);
    }

    public async Task UpdateQuoteAsync(Guid quoteId, string newContent, string newAuthor)
    {
        var quote = await _quoteRepository.GetByIdAsync(new QuoteId(quoteId));
        if (quote == null) throw new KeyNotFoundException($"Quote {quoteId} not found.");
        quote.UpdateContent(newContent, newAuthor);
        await _quoteRepository.UpdateAsync(quote);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteQuoteAsync(Guid quoteId)
    {
        var quote = await _quoteRepository.GetByIdAsync(new QuoteId(quoteId));
        if (quote == null) throw new KeyNotFoundException($"Quote {quoteId} not found.");
        if (quote.IsBundled) throw new InvalidOperationException("Cannot delete a bundled quote.");
        await _quoteRepository.DeleteAsync(new QuoteId(quoteId));
        await _unitOfWork.SaveChangesAsync();
    }
}
