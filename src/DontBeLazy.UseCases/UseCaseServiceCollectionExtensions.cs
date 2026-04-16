using Microsoft.Extensions.DependencyInjection;
using DontBeLazy.Ports.Inbound;
using DontBeLazy.Ports.Outbound.Services;
using DontBeLazy.UseCases.Profiles;
using DontBeLazy.UseCases.Settings;
using DontBeLazy.UseCases.Logging;

namespace DontBeLazy.UseCases;

public static class UseCaseServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        // State engine
        services.AddSingleton<ActiveSessionState>();

        // Use Cases — real implementations registered under concrete type for decorator resolution
        services.AddScoped<IFocusTaskUseCase, FocusTaskUseCase>();
        services.AddScoped<FocusSessionUseCase>();   // concrete registration
        services.AddScoped<IFocusSessionUseCase>(sp =>
            new LoggingFocusSessionDecorator(
                sp.GetRequiredService<FocusSessionUseCase>(),
                sp.GetRequiredService<IAppLogger>()));

        services.AddScoped<IProfileUseCase, ProfileUseCase>();
        services.AddScoped<IProfileEntryUseCase, ProfileEntryUseCase>();
        services.AddScoped<IProfileSyncUseCase, ProfileSyncUseCase>();
        services.AddScoped<ISystemSettingsUseCase, SystemSettingsUseCase>();
        services.AddScoped<IQuoteUseCase, QuoteUseCase>();
        services.AddScoped<IAnalyticsUseCase, AnalyticsUseCase>();

        return services;
    }
}
