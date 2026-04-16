using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.ValueObjects;
using DontBeLazy.Ports.DTOs;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Repositories;
using DontBeLazy.UseCases.Mappers;

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

    public async Task AddProfileEntryAsync(Guid profileId, ProfileEntryTypeDto type, string value, string? exePath = null)
    {
        var profile = await _profileRepository.GetByIdAsync(new ProfileId(profileId));
        if (profile == null) throw new KeyNotFoundException($"Profile {profileId} not found.");

        var entry = new ProfileEntry(new ProfileId(profileId), DtoMapper.ToDomain(type), value, exePath);
        profile.AddEntry(entry);
        await _profileRepository.UpdateAsync(profile);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveProfileEntryAsync(Guid profileId, Guid entryId)
    {
        var profile = await _profileRepository.GetByIdAsync(new ProfileId(profileId));
        if (profile == null) throw new KeyNotFoundException($"Profile {profileId} not found.");

        var entry = profile.Entries.FirstOrDefault(e => e.Id.Value == entryId);
        if (entry != null)
        {
            profile.RemoveEntry(entry);
            await _profileRepository.UpdateAsync(profile);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task UpdateProfileEntryAsync(Guid profileId, Guid entryId, ProfileEntryTypeDto type, string value, string? exePath = null)
    {
        var profile = await _profileRepository.GetByIdAsync(new ProfileId(profileId));
        if (profile == null) throw new KeyNotFoundException($"Profile {profileId} not found.");

        var entry = profile.Entries.FirstOrDefault(e => e.Id.Value == entryId);
        if (entry != null)
        {
            entry.Update(DtoMapper.ToDomain(type), value, exePath);
            await _profileRepository.UpdateAsync(profile);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task ClearProfileEntriesAsync(Guid profileId)
    {
        var profile = await _profileRepository.GetByIdAsync(new ProfileId(profileId));
        if (profile == null) throw new KeyNotFoundException($"Profile {profileId} not found.");

        profile.ClearEntries();
        await _profileRepository.UpdateAsync(profile);
        await _unitOfWork.SaveChangesAsync();
    }
}
