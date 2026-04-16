using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.ValueObjects;
using DontBeLazy.Ports.Outbound.Repositories;

namespace DontBeLazy.SqliteDataAccess.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly AppDbContext _context;

    public ProfileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Profile>> GetAllAsync()
    {
        return await _context.Profiles
            .Include(p => p.Entries)
            .ToListAsync();
    }

    public async Task<Profile?> GetByIdAsync(ProfileId id)
    {
        return await _context.Profiles
            .Include(p => p.Entries)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Profile?> GetDefaultProfileAsync()
    {
        return await _context.Profiles
            .Include(p => p.Entries)
            .FirstOrDefaultAsync(p => p.IsDefault);
    }

    public async Task AddAsync(Profile profile)
    {
        await _context.Profiles.AddAsync(profile);
    }

    public async Task UpdateAsync(Profile profile)
    {
        _context.Profiles.Update(profile);
    }

    public async Task DeleteAsync(ProfileId id)
    {
        var profile = await _context.Profiles.FindAsync(id);
        if (profile != null)
        {
            _context.Profiles.Remove(profile);
        }
    }
}
