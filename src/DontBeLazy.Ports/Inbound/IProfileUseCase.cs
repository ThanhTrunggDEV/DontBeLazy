using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;

namespace DontBeLazy.Ports.Inbound;

public interface IProfileUseCase
{
    Task<IReadOnlyCollection<Profile>> GetAllProfilesAsync();
    Task<Profile> GetProfileByIdAsync(ProfileId profileId);
    Task<Profile> GetDefaultProfileAsync();
    Task<Profile> CreateProfileAsync(string name, bool isDefault);
    Task UpdateProfileNameAsync(ProfileId profileId, string newName);
    Task DeleteProfileAsync(ProfileId profileId);
}
