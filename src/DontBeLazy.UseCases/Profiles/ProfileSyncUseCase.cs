using System;
using System.Text.Json;
using System.Threading.Tasks;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Repositories;

namespace DontBeLazy.UseCases.Profiles;

public class ProfileSyncUseCase : IProfileSyncUseCase
{
    private readonly IProfileRepository _profileRepository;
    
    public ProfileSyncUseCase(IProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<string> ExportProfilesAsync()
    {
        var profiles = await _profileRepository.GetAllAsync();
        return JsonSerializer.Serialize(profiles, new JsonSerializerOptions { WriteIndented = true });
    }

    public Task ImportProfilesAsync(string jsonContent, bool overwriteExisting)
    {
        throw new NotImplementedException("Importing profiles is not fully supported yet because it involves syncing or replacing existing database entries with potentially new IDs. To be implemented.");
    }
}
