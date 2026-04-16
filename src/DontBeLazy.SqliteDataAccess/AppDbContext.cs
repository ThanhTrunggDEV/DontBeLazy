using System;
using Microsoft.EntityFrameworkCore;
using DontBeLazy.Domain.Entities;
using DontBeLazy.Domain.ValueObjects;
using DontBeLazy.SqliteDataAccess.Converters;

namespace DontBeLazy.SqliteDataAccess;

public class AppDbContext : DbContext
{
    public DbSet<FocusTask> Tasks { get; set; } = null!;
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<SessionHistory> Sessions { get; set; } = null!;
    public DbSet<SystemSettings> Settings { get; set; } = null!;
    public DbSet<Quote> Quotes { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        
        configurationBuilder.Properties<TaskId>().HaveConversion<TaskIdConverter>();
        configurationBuilder.Properties<ProfileId>().HaveConversion<ProfileIdConverter>();
        configurationBuilder.Properties<ProfileEntryId>().HaveConversion<ProfileEntryIdConverter>();
        configurationBuilder.Properties<SessionId>().HaveConversion<SessionIdConverter>();
        configurationBuilder.Properties<QuoteId>().HaveConversion<QuoteIdConverter>();
        configurationBuilder.Properties<SettingsId>().HaveConversion<SettingsIdConverter>();
        // SnapshotId configured in OnModelCreating directly due to it being part of an owned or dependent entity edge cases, or we can configure it here.
        configurationBuilder.Properties<SnapshotId>().HaveConversion<SnapshotIdConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- FocusTask Configuration ---
        modelBuilder.Entity<FocusTask>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).HasMaxLength(200).IsRequired();
            e.Property(t => t.Status).HasConversion<string>();
            e.Property(t => t.TaskType).HasConversion<string>();
            e.Property(t => t.RecurringType).HasConversion<string>();
        });

        // --- Profile & ProfileEntry Configuration ---
        modelBuilder.Entity<Profile>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(100).IsRequired();
            
            // 1-N Relationship Profile -> ProfileEntry
            e.HasMany(p => p.Entries)
             .WithOne()
             .HasForeignKey(pe => pe.ProfileId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);
             
            // Configure backing field for Entries
            e.Metadata.FindNavigation(nameof(Profile.Entries))!
             .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ProfileEntry>(e =>
        {
            e.HasKey(pe => pe.Id);
            e.Property(pe => pe.Type).HasConversion<string>();
            e.Property(pe => pe.Value).IsRequired();
        });

        // --- SessionHistory Configuration ---
        modelBuilder.Entity<SessionHistory>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.CompletionStatus).HasConversion<string>();
            
            // 1-N Relationship SessionHistory -> SessionProfileSnapshot
            e.HasMany(s => s.Snapshots)
             .WithOne()
             .HasForeignKey(sp => sp.SessionId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);

            e.Metadata.FindNavigation(nameof(SessionHistory.Snapshots))!
             .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<SessionProfileSnapshot>(e =>
        {
            e.HasKey(sp => sp.Id);
            e.Property(sp => sp.Type).HasConversion<string>();
            e.Property(sp => sp.Value).IsRequired();
            e.Property(sp => sp.Id).HasConversion<SnapshotIdConverter>();
        });

        // --- SystemSettings Configuration ---
        modelBuilder.Entity<SystemSettings>(e =>
        {
            e.HasKey(s => s.Id);
            // Default ID for singleton pattern is generally configured or hardcoded in repo.
        });

        // --- Quote Configuration ---
        modelBuilder.Entity<Quote>(e =>
        {
            e.HasKey(q => q.Id);
            e.Property(q => q.EventType).HasConversion<string>();
        });
    }
}
