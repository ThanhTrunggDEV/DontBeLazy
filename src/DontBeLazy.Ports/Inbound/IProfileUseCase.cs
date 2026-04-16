using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Ports.DTOs;

namespace DontBeLazy.Ports.Inbound;

public interface IProfileUseCase
{
    Task<IReadOnlyCollection<ProfileDto>> GetAllProfilesAsync();
    Task<ProfileDto> GetProfileByIdAsync(Guid profileId);
    Task<ProfileDto> GetDefaultProfileAsync();
    Task<ProfileDto> CreateProfileAsync(string name, bool isDefault);
    Task UpdateProfileNameAsync(Guid profileId, string newName);
    Task DeleteProfileAsync(Guid profileId);
}
