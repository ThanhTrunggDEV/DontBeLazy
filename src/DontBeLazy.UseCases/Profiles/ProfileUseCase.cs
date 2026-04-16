using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.ValueObjects;
using DontBeLazy.Ports.DTOs;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Repositories;
using DontBeLazy.UseCases.Mappers;

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

    public async Task<IReadOnlyCollection<ProfileDto>> GetAllProfilesAsync()
    {
        var profiles = await _profileRepository.GetAllAsync();
        return profiles.Select(DtoMapper.ToDto).ToList();
    }

    public async Task<ProfileDto> GetProfileByIdAsync(Guid profileId)
    {
        var profile = await _profileRepository.GetByIdAsync(new ProfileId(profileId));
        if (profile == null) throw new KeyNotFoundException($"Profile {profileId} not found.");
        return DtoMapper.ToDto(profile);
    }

    public async Task<ProfileDto> GetDefaultProfileAsync()
    {
        var profile = await _profileRepository.GetDefaultProfileAsync();
        if (profile == null) throw new InvalidOperationException("Default Profile has not been initialized.");
        return DtoMapper.ToDto(profile);
    }

    public async Task<ProfileDto> CreateProfileAsync(string name, bool isDefault)
    {
        var profile = new Profile(name, isDefault);
        await _profileRepository.AddAsync(profile);
        await _unitOfWork.SaveChangesAsync();
        return DtoMapper.ToDto(profile);
    }

    public async Task UpdateProfileNameAsync(Guid profileId, string newName)
    {
        var profile = await _profileRepository.GetByIdAsync(new ProfileId(profileId));
        if (profile == null) throw new KeyNotFoundException($"Profile {profileId} not found.");
        profile.UpdateName(newName);
        await _profileRepository.UpdateAsync(profile);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteProfileAsync(Guid profileId)
    {
        var profile = await _profileRepository.GetByIdAsync(new ProfileId(profileId));
        if (profile == null) throw new KeyNotFoundException($"Profile {profileId} not found.");
        await _profileRepository.DeleteAsync(new ProfileId(profileId));
        await _unitOfWork.SaveChangesAsync();
    }
}
