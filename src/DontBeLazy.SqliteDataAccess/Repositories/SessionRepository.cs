using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.ValueObjects;
using DontBeLazy.Ports.Outbound.Repositories;

namespace DontBeLazy.SqliteDataAccess.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _context;

    public SessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<SessionHistory>> GetRecentSessionsAsync(int limit = 50)
    {
        return await _context.Sessions
            .Include(s => s.Snapshots)
            .OrderByDescending(s => s.FocusStartDate)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<SessionHistory>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Sessions
            .Include(s => s.Snapshots)
            .Where(s => s.FocusStartDate >= startDate && s.FocusStartDate <= endDate)
            .OrderByDescending(s => s.FocusStartDate)
            .ToListAsync();
    }

    public async Task<SessionHistory?> GetByIdAsync(SessionId id)
    {
        return await _context.Sessions
            .Include(s => s.Snapshots)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(SessionHistory session)
    {
        await _context.Sessions.AddAsync(session);
    }

    public async Task UpdateAsync(SessionHistory session)
    {
        _context.Sessions.Update(session);
    }

    public async Task DeleteByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var targetSessions = await _context.Sessions
            .Where(s => s.FocusStartDate >= startDate && s.FocusStartDate <= endDate)
            .ToListAsync();
            
        if (targetSessions.Any())
        {
            _context.Sessions.RemoveRange(targetSessions);
        }
    }
}
