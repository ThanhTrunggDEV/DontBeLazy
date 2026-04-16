using System.Threading.Tasks;

namespace DontBeLazy.Ports.Outbound.Services;

public interface IAiProfileAssistantPort
{
    Task<string> GenerateSmartProfileAsync(string intent);
}
