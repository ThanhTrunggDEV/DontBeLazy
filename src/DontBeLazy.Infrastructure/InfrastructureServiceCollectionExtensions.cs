using System.Net.Http;
using DontBeLazy.Ports.Outbound.Services;
using DontBeLazy.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DontBeLazy.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IMonotonicClockPort, MonotonicClockAdapter>();
        services.AddSingleton<IStrictEnginePort, WindowsStrictEngine>();

        // Phân giải HttpClient cho Gemini
        services.AddHttpClient<GeminiAiServices>();
        
        services.AddTransient<IAiTaskAssistantPort>(sp => sp.GetRequiredService<GeminiAiServices>());
        services.AddTransient<IAiGuiltTripPort>(sp => sp.GetRequiredService<GeminiAiServices>());
        services.AddTransient<IAiProfileAssistantPort>(sp => sp.GetRequiredService<GeminiAiServices>());

        return services;
    }
}
