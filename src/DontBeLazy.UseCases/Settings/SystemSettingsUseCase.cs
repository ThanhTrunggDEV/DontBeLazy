using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Repositories;

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

    public async Task<SystemSettings> GetSettingsAsync()
    {
        return await _settingsRepository.GetSettingsAsync();
    }

    public async Task UpdateSettingsAsync(bool globalStrictMode, bool enableQuotes, string quoteLanguage, bool darkTheme)
    {
        var settings = await _settingsRepository.GetSettingsAsync();
        settings.UpdatePreferences(globalStrictMode, enableQuotes, quoteLanguage, darkTheme);
        await _settingsRepository.UpdateSettingsAsync(settings);
        await _unitOfWork.SaveChangesAsync();
    }
}
