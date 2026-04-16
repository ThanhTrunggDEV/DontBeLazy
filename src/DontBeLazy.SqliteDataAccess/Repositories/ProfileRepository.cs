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

    public async Task EnsureDefaultProfileAsync()
    {
        var exists = await _context.Profiles.AnyAsync(p => p.IsDefault);
        if (!exists)
        {
            var defaultProfile = new Profile("Default Profile", true);
            await _context.Profiles.AddAsync(defaultProfile);
            await _context.SaveChangesAsync();
        }
    }

    public Task AddAsync(Profile profile)
    {
        _context.Profiles.Add(profile);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Profile profile)
    {
        _context.Profiles.Update(profile);
        return Task.CompletedTask;
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
