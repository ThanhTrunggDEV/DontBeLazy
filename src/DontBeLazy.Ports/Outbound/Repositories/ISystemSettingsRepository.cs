using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;

namespace DontBeLazy.Ports.Outbound.Repositories;

public interface ISystemSettingsRepository
{
    Task<SystemSettings> GetSettingsAsync();
    Task UpdateSettingsAsync(SystemSettings settings);
}
