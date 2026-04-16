using System.Threading.Tasks;

namespace DontBeLazy.Ports.Outbound.Services;

public interface IAiGuiltTripPort
{
    Task<string> GenerateGuiltTripQuoteAsync(string taskName, string language);
}
