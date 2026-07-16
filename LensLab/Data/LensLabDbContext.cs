using Microsoft.EntityFrameworkCore;
using LensLab.Data.Entities;
using System.IO;
using System;

namespace LensLab.Data;

/// <summary>
/// Entity Framework Core database context for the LensLab SQLite database.
/// </summary>
public class LensLabDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the database set for recently opened files.
    /// </summary>
    public DbSet<RecentFile> RecentFiles => Set<RecentFile>();

    /// <summary>
    /// Gets or sets the database set for saved projects.
    /// </summary>
    public DbSet<SavedProject> SavedProjects => Set<SavedProject>();

    /// <summary>
    /// Gets or sets the database set for custom filters.
    /// </summary>
    public DbSet<CustomFilter> CustomFilters => Set<CustomFilter>();

    /// <summary>
    /// Configures the DbContext to use a local SQLite database file in the output directory.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lenslab.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }
}
