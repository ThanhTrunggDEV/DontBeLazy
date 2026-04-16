using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.ValueObjects;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Repositories;

namespace DontBeLazy.UseCases.Profiles;

public class ProfileUseCase : IProfileUseCase
{
    private readonly IProfileRepository _profileRepository;

    public ProfileUseCase(IProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<IReadOnlyCollection<Profile>> GetAllProfilesAsync()
    {
        return await _profileRepository.GetAllAsync();
    }

    public async Task<Profile> GetProfileByIdAsync(ProfileId profileId)
    {
        var profile = await _profileRepository.GetByIdAsync(profileId);
        if (profile == null)
            throw new System.Collections.Generic.KeyNotFoundException($"Profile {profileId} not found.");
            
        return profile;
    }

    public async Task<Profile> GetDefaultProfileAsync()
    {
        var profile = await _profileRepository.GetDefaultProfileAsync();
        if (profile == null)
        {
            // Seed default if none
            profile = new Profile("Default Profile", true);
            await _profileRepository.AddAsync(profile);
            // It is safe to assume default creation doesn't heavily violate UI UoW constraint if it occurs as a silent backend normalization, but strictly adhering to no-save policy means UI will handle this seed instead. 
            // Wait, if it doesn't save here, the caller must save.
        }
        return profile;
    }

    public async Task<Profile> CreateProfileAsync(string name, bool isDefault)
    {
        var profile = new Profile(name, isDefault);
        await _profileRepository.AddAsync(profile);
        return profile;
    }

    public async Task UpdateProfileNameAsync(ProfileId profileId, string newName)
    {
        var profile = await _profileRepository.GetByIdAsync(profileId);
        if (profile == null)
            throw new System.Collections.Generic.KeyNotFoundException($"Profile {profileId} not found.");

        profile.UpdateName(newName);
        await _profileRepository.UpdateAsync(profile);
    }

    public async Task DeleteProfileAsync(ProfileId profileId)
    {
        var profile = await _profileRepository.GetByIdAsync(profileId);
        if (profile == null)
            throw new System.Collections.Generic.KeyNotFoundException($"Profile {profileId} not found.");

        await _profileRepository.DeleteAsync(profileId);
    }
}
