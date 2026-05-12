using System.Threading.Tasks;
using DontBeLazy.Ports.DTOs;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Repositories;
using DontBeLazy.UseCases.Mappers;
using DontBeLazy.Domain.Entities;

namespace DontBeLazy.UseCases.Settings;

public class SystemSettingsUseCase : ISystemSettingsUseCase
{
    private readonly ISystemSettingsRepository _settingsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DontBeLazy.Ports.Outbound.Services.IStrictEnginePort _strictEnginePort;
    private readonly DontBeLazy.UseCases.ActiveSessionState _sessionState;

    public SystemSettingsUseCase(
        ISystemSettingsRepository settingsRepository, 
        IUnitOfWork unitOfWork,
        DontBeLazy.Ports.Outbound.Services.IStrictEnginePort strictEnginePort,
        DontBeLazy.UseCases.ActiveSessionState sessionState)
    {
        _settingsRepository = settingsRepository;
        _unitOfWork = unitOfWork;
        _strictEnginePort = strictEnginePort;
        _sessionState = sessionState;
    }

    public async Task<SystemSettingsDto> GetSettingsAsync()
    {
        var settings = await _settingsRepository.GetSettingsAsync();
        return DtoMapper.ToDto(settings);
    }

    public async Task UpdateSettingsAsync(bool globalStrictMode, bool enableQuotes, string quoteLanguage, bool darkTheme,
        string? geminiApiKey = null, string geminiModel = "gemini-2.5-flash")
    {
        var settings = await _settingsRepository.GetSettingsAsync();
        
        bool globalStrictModeChanged = settings.GlobalStrictMode != globalStrictMode;

        settings.UpdatePreferences(globalStrictMode, enableQuotes, quoteLanguage, darkTheme, geminiApiKey, geminiModel);
        await _settingsRepository.UpdateSettingsAsync(settings);
        await _unitOfWork.SaveChangesAsync();

        if (globalStrictModeChanged && _sessionState.CurrentSession == null)
        {
            if (globalStrictMode)
            {
                await _strictEnginePort.ApplyProfileAsync(new System.Collections.Generic.List<DontBeLazy.Domain.Entities.SessionProfileSnapshot>());
            }
            else
            {
                await _strictEnginePort.ClearRestrictionsAsync();
            }
        }
    }
}
