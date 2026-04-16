using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;

namespace DontBeLazy.Ports.Outbound.Services;

public interface IStrictEnginePort
{
    Task ApplyProfileAsync(IReadOnlyCollection<SessionProfileSnapshot> profiles);
    Task ClearRestrictionsAsync();
    Task LockScreenAsync();
}
