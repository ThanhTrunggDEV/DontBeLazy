using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DontBeLazy.Infrastructure;
using DontBeLazy.SqliteDataAccess;
using DontBeLazy.UseCases;
using DontBeLazy.WPF.ViewModels;
using DontBeLazy.WPF.Views;

namespace DontBeLazy.WPF;

public partial class App : Application
{
    public static ServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        // Ensure DB is created
        using (var scope = Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureCreated();
        }

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register lower layers (for DI wiring only)
        services.AddSqliteDataAccess("Data Source=dontbelazy.db");
        services.AddUseCases();
        services.AddInfrastructureServices();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProfilesViewModel>();
        services.AddTransient<FocusSessionViewModel>();
        services.AddTransient<AnalyticsViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Register Views
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Services?.Dispose();
        base.OnExit(e);
    }
}
