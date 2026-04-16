using System.Linq;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.Enums;
using DontBeLazy.Domain.ValueObjects;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Repositories;

namespace DontBeLazy.UseCases.Profiles;

public class ProfileEntryUseCase : IProfileEntryUseCase
{
    private readonly IProfileRepository _profileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProfileEntryUseCase(IProfileRepository profileRepository, IUnitOfWork unitOfWork)
    {
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task AddProfileEntryAsync(ProfileId profileId, ProfileEntryType type, string value, string? exePath = null)
    {
        var profile = await _profileRepository.GetByIdAsync(profileId);
        if (profile == null)
            throw new System.Collections.Generic.KeyNotFoundException($"Profile {profileId} not found.");

        var entry = new ProfileEntry(profileId, type, value, exePath);
        profile.AddEntry(entry);
        
        await _profileRepository.UpdateAsync(profile);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveProfileEntryAsync(ProfileId profileId, ProfileEntryId entryId)
    {
        var profile = await _profileRepository.GetByIdAsync(profileId);
        if (profile == null)
            throw new System.Collections.Generic.KeyNotFoundException($"Profile {profileId} not found.");

        var entry = profile.Entries.FirstOrDefault(e => e.Id == entryId);
        if (entry != null)
        {
            profile.RemoveEntry(entry);
            await _profileRepository.UpdateAsync(profile);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task ClearProfileEntriesAsync(ProfileId profileId)
    {
        var profile = await _profileRepository.GetByIdAsync(profileId);
        if (profile == null)
            throw new System.Collections.Generic.KeyNotFoundException($"Profile {profileId} not found.");

        profile.ClearEntries();
        await _profileRepository.UpdateAsync(profile);
        await _unitOfWork.SaveChangesAsync();
    }
}
