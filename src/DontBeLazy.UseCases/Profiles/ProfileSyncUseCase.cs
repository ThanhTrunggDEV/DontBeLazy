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

    public Task<string> ExportProfilesAsync()
    {
        // Simple mock for exporting json. In production, use System.Text.Json to serialize profiles.
        return Task.FromResult("{ \"exported\": true }");
    }

    public Task ImportProfilesAsync(string jsonContent, bool overwriteExisting)
    {
        // Simple mock for parsing JSON string to domain entities and calling Replace/Merge
        return Task.CompletedTask;
    }
}
