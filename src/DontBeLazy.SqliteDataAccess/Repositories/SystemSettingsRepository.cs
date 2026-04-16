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
        var settings = await _context.Settings.FirstOrDefaultAsync();
        
        if (settings == null)
        {
            // Seed default if not exists
            settings = new SystemSettings(false, true, "vi", false);
            await _context.Settings.AddAsync(settings);
            // Must save seed immediately otherwise tracking fails or subsequent calls duplicate
            await _context.SaveChangesAsync();
        }
        
        return settings;
    }

    public async Task UpdateSettingsAsync(SystemSettings settings)
    {
        _context.Settings.Update(settings);
    }
}
