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
            try
            {
                dbContext.Database.Migrate(); // auto-applies all pending migrations
            }
            catch (System.Exception ex) when (ex.Message.Contains("already exists"))
            {
                // Self-healing: if the DB was made with EnsureCreated (beta versions), it has no migration history.
                // Migrate() tries to run InitialCreate and crashes because 'Profiles' already exists.
                // We manually inject the history record for InitialCreate so Migrate() skips it.
                var connection = dbContext.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open) connection.Open();
                
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                            ""MigrationId"" TEXT NOT NULL CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY,
                            ""ProductVersion"" TEXT NOT NULL
                        );
                        INSERT OR IGNORE INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                        VALUES ('20260416085506_InitialCreate', '9.0.4');
                    ";
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
                
                // Retry migration for any missing schemas (e.g. AddGeminiConfigToSettings)
                dbContext.Database.Migrate();
            }

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
        // Use LocalAppData so MSI installs don't fail due to read-only Program Files
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(localAppData, "DontBeLazy");
        Directory.CreateDirectory(appFolder);
        var dbPath = Path.Combine(appFolder, "dontbelazy.db");

        // Smooth upgrade path for portable beta users who had db in exe folder
        var oldDbPath = Path.Combine(AppContext.BaseDirectory, "dontbelazy.db");
        if (File.Exists(oldDbPath) && !File.Exists(dbPath))
        {
            try { File.Copy(oldDbPath, dbPath); } catch { }
        }

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
