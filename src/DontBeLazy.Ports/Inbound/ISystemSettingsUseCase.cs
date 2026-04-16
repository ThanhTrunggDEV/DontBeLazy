using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;

namespace DontBeLazy.Ports.Inbound;

public interface ISystemSettingsUseCase
{
    Task<SystemSettings> GetSettingsAsync();
    Task UpdateSettingsAsync(bool globalStrictMode, bool enableQuotes, string quoteLanguage, bool darkTheme);
}
