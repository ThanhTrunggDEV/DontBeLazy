using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Ports.Outbound.Repositories;

namespace DontBeLazy.SqliteDataAccess.Repositories;

public class SystemSettingsRepository : ISystemSettingsRepository
{
    private readonly AppDbContext _context;

    public SystemSettingsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SystemSettings> GetSettingsAsync()
    {
        // No seeding here — seed is handled via EnsureDefaultSettingsAsync() at app startup.
        // This strictly reads existing data. Caller must ensure settings exist beforehand.
        var settings = await _context.Settings.FirstOrDefaultAsync();
        
        if (settings == null)
            throw new System.InvalidOperationException(
                "SystemSettings have not been initialized. Call EnsureDefaultSettingsAsync() at application startup.");
        
        return settings;
    }

    /// <summary>
    /// Called ONCE at application startup to seed default settings if not present.
    /// Immediately calls SaveChangesAsync internally as this is an infrastructure-level init,
    /// not a business transaction — safe to bypass UoW since it only runs when DB is empty.
    /// </summary>
    public async Task EnsureDefaultSettingsAsync()
    {
        var exists = await _context.Settings.AnyAsync();
        if (!exists)
        {
            var defaults = new SystemSettings(false, true, "vi", false);
            await _context.Settings.AddAsync(defaults);
            await _context.SaveChangesAsync();
        }
    }

    public Task UpdateSettingsAsync(SystemSettings settings)
    {
        _context.Settings.Update(settings);
        return Task.CompletedTask;
    }
}
