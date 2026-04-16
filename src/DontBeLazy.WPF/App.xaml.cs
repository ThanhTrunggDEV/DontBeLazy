using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DontBeLazy.Infrastructure;
using DontBeLazy.SqliteDataAccess;
using DontBeLazy.UseCases;
using DontBeLazy.WPF.ViewModels;
using DontBeLazy.WPF.Views;
using MaterialDesignThemes.Wpf;

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

        // Ensure DB is created and seeded with defaults
        using (var scope = Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureCreated();

            var settingsRepo = scope.ServiceProvider.GetRequiredService<DontBeLazy.Ports.Outbound.Repositories.ISystemSettingsRepository>();
            settingsRepo.EnsureDefaultSettingsAsync().GetAwaiter().GetResult();
            
            var profileRepo = scope.ServiceProvider.GetRequiredService<DontBeLazy.Ports.Outbound.Repositories.IProfileRepository>();
            profileRepo.EnsureDefaultProfileAsync().GetAwaiter().GetResult();

            // Apply global exact theme from DB on startup
            var settings = settingsRepo.GetSettingsAsync().GetAwaiter().GetResult();
            var helper = new MaterialDesignThemes.Wpf.PaletteHelper();
            var theme = helper.GetTheme();
            theme.SetBaseTheme(settings.DarkTheme ? MaterialDesignThemes.Wpf.BaseTheme.Dark : MaterialDesignThemes.Wpf.BaseTheme.Light);
            helper.SetTheme(theme);
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
