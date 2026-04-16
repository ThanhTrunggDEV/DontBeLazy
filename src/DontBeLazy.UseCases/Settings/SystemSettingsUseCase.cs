using System.Threading.Tasks;
using DontBeLazy.Ports.DTOs;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Repositories;
using DontBeLazy.UseCases.Mappers;

namespace DontBeLazy.UseCases.Settings;

public class SystemSettingsUseCase : ISystemSettingsUseCase
{
    private readonly ISystemSettingsRepository _settingsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SystemSettingsUseCase(ISystemSettingsRepository settingsRepository, IUnitOfWork unitOfWork)
    {
        _settingsRepository = settingsRepository;
        _unitOfWork = unitOfWork;
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
        settings.UpdatePreferences(globalStrictMode, enableQuotes, quoteLanguage, darkTheme, geminiApiKey, geminiModel);
        await _settingsRepository.UpdateSettingsAsync(settings);
        await _unitOfWork.SaveChangesAsync();
    }
}
