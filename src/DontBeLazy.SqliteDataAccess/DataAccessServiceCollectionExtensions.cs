using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DontBeLazy.Ports.Outbound.Repositories;
using DontBeLazy.SqliteDataAccess.Repositories;

namespace DontBeLazy.SqliteDataAccess;

public static class DataAccessServiceCollectionExtensions
{
    public static IServiceCollection AddSqliteDataAccess(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ISystemSettingsRepository, SystemSettingsRepository>();
        services.AddScoped<IQuoteRepository, QuoteRepository>();

        return services;
    }
}
