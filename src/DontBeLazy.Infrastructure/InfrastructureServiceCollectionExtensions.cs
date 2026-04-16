using System.Net.Http;
using DontBeLazy.Ports.Outbound.Services;
using DontBeLazy.Infrastructure.Services;
using DontBeLazy.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace DontBeLazy.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IMonotonicClockPort, MonotonicClockAdapter>();

        // Logger — singleton shared across all decorators
        services.AddSingleton<IAppLogger, FileLogger>();

        // StrictEngine: real impl wrapped by logging decorator
        services.AddSingleton<WindowsStrictEngine>();
        services.AddSingleton<IStrictEnginePort>(sp =>
            new LoggingStrictEngineDecorator(
                sp.GetRequiredService<WindowsStrictEngine>(),
                sp.GetRequiredService<IAppLogger>()));

        // Gemini AI services
        services.AddHttpClient<GeminiAiServices>();
        services.AddTransient<IAiTaskAssistantPort>(sp => sp.GetRequiredService<GeminiAiServices>());
        services.AddTransient<IAiGuiltTripPort>(sp => sp.GetRequiredService<GeminiAiServices>());
        services.AddTransient<IAiProfileAssistantPort>(sp => sp.GetRequiredService<GeminiAiServices>());

        return services;
    }
}
