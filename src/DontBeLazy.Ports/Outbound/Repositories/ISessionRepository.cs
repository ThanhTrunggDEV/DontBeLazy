using System.Collections.Generic;
using System.Threading.Tasks;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.ValueObjects;

namespace DontBeLazy.Ports.Outbound.Repositories;

public interface ISessionRepository
{
    Task<IReadOnlyCollection<SessionHistory>> GetRecentSessionsAsync(int limit = 50);
    Task<IReadOnlyCollection<SessionHistory>> GetSessionsByDateRangeAsync(System.DateTime startDate, System.DateTime endDate);
    Task<SessionHistory?> GetByIdAsync(SessionId id);
    Task<SessionHistory?> GetIncompleteSessionAsync();
    Task AddAsync(SessionHistory session);
    Task UpdateAsync(SessionHistory session);
    Task DeleteByDateRangeAsync(System.DateTime startDate, System.DateTime endDate);
}
