using System.Threading.Tasks;
using DontBeLazy.Ports.DTOs;

namespace DontBeLazy.Ports.Inbound;

public interface ISystemSettingsUseCase
{
    Task<SystemSettingsDto> GetSettingsAsync();
    Task UpdateSettingsAsync(bool globalStrictMode, bool enableQuotes, string quoteLanguage, bool darkTheme);
}
