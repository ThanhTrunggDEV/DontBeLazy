using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.ValueObjects;

namespace DontBeLazy.Ports.Outbound.Repositories;

public interface ISessionRepository
{
    Task<IReadOnlyCollection<SessionHistory>> GetRecentSessionsAsync(int limit = 50);
    Task<SessionHistory?> GetByIdAsync(SessionId id);
    Task AddAsync(SessionHistory session);
    Task UpdateAsync(SessionHistory session);
}
