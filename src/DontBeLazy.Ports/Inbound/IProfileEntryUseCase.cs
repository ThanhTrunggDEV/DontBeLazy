using System.Threading.Tasks;
using DontBeLazy.Ports.DTOs;

namespace DontBeLazy.Ports.Inbound;

public interface IProfileEntryUseCase
{
    Task AddProfileEntryAsync(Guid profileId, ProfileEntryTypeDto type, string value, string? exePath = null);
    Task RemoveProfileEntryAsync(Guid profileId, Guid entryId);
    Task ClearProfileEntriesAsync(Guid profileId);
}
