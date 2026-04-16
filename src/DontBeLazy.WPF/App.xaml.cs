using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DontBeLazy.Infrastructure;
using DontBeLazy.SqliteDataAccess;
using DontBeLazy.UseCases;
using DontBeLazy.WPF.Services;
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
            dbContext.Database.Migrate(); // auto-applies all pending migrations

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

        // Set window icon from the .ico file next to the exe
        var icoPath = Path.Combine(AppContext.BaseDirectory, "app.ico");
        if (File.Exists(icoPath))
        {
            using var stream = File.OpenRead(icoPath);
            mainWindow.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(
                stream, System.Windows.Media.Imaging.BitmapCreateOptions.None,
                System.Windows.Media.Imaging.BitmapCacheOption.OnLoad);
        }

        mainWindow.Show();

        // Background update check — non-blocking, silent on failure
        _ = Services.GetRequiredService<UpdateViewModel>().CheckForUpdateAsync();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register lower layers (for DI wiring only)
        var dbPath = Path.Combine(AppContext.BaseDirectory, "dontbelazy.db");
        services.AddSqliteDataAccess($"Data Source={dbPath}");
        services.AddUseCases();
        services.AddInfrastructureServices();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProfilesViewModel>();
        services.AddTransient<FocusSessionViewModel>();
        services.AddTransient<AnalyticsViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddSingleton<UpdaterService>();
        services.AddSingleton<UpdateViewModel>();

        // Register Views
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Services?.Dispose();
        base.OnExit(e);
    }
}
