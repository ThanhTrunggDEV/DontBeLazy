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
    private readonly IUnitOfWork _unitOfWork;

    public ProfileUseCase(IProfileRepository profileRepository, IUnitOfWork unitOfWork)
    {
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
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
            throw new System.InvalidOperationException("Default Profile has not been initialized. Call EnsureDefaultProfileAsync() at application startup.");
            
        return profile;
    }

    public async Task<Profile> CreateProfileAsync(string name, bool isDefault)
    {
        var profile = new Profile(name, isDefault);
        await _profileRepository.AddAsync(profile);
        await _unitOfWork.SaveChangesAsync();
        return profile;
    }

    public async Task UpdateProfileNameAsync(ProfileId profileId, string newName)
    {
        var profile = await _profileRepository.GetByIdAsync(profileId);
        if (profile == null)
            throw new System.Collections.Generic.KeyNotFoundException($"Profile {profileId} not found.");

        profile.UpdateName(newName);
        await _profileRepository.UpdateAsync(profile);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteProfileAsync(ProfileId profileId)
    {
        var profile = await _profileRepository.GetByIdAsync(profileId);
        if (profile == null)
            throw new System.Collections.Generic.KeyNotFoundException($"Profile {profileId} not found.");

        await _profileRepository.DeleteAsync(profileId);
        await _unitOfWork.SaveChangesAsync();
    }
}
