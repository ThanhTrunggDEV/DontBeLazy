using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Repositories;

namespace DontBeLazy.UseCases.Settings;

public class SystemSettingsUseCase : ISystemSettingsUseCase
{
    private readonly ISystemSettingsRepository _settingsRepository;

    public SystemSettingsUseCase(ISystemSettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async Task<SystemSettings> GetSettingsAsync()
    {
        return await _settingsRepository.GetSettingsAsync();
    }

    public async Task UpdateSettingsAsync(bool globalStrictMode, bool enableQuotes, string quoteLanguage, bool darkTheme)
    {
        var settings = await _settingsRepository.GetSettingsAsync();
        settings.UpdatePreferences(globalStrictMode, enableQuotes, quoteLanguage, darkTheme);
        await _settingsRepository.UpdateSettingsAsync(settings);
    }
}
