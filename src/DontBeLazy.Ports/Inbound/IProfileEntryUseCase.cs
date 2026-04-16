using System.Threading.Tasks;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;

namespace DontBeLazy.Ports.Inbound;

public interface IProfileEntryUseCase
{
    Task AddProfileEntryAsync(ProfileId profileId, ProfileEntryType type, string value, string? exePath = null);
    Task RemoveProfileEntryAsync(ProfileId profileId, ProfileEntryId entryId);
    Task ClearProfileEntriesAsync(ProfileId profileId);
}
