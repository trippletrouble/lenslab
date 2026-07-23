using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LensLab.Data;
using LensLab.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LensLab.Services;

/// <summary>
/// Default implementation of <see cref="IRecentFilesService"/> using EF Core SQLite context.
/// </summary>
public class RecentFilesService : IRecentFilesService
{
    private readonly IDialogService? _dialogService;

    public RecentFilesService(IDialogService? dialogService = null)
    {
        _dialogService = dialogService;
    }

    public async Task<List<RecentFile>> GetRecentFilesAsync()
    {
        try
        {
            using var db = new LensLabDbContext();
            await db.Database.EnsureCreatedAsync();

            return await db.RecentFiles
                .OrderByDescending(f => f.LastOpened)
                .Take(5)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching recent files: {ex.Message}");
            _dialogService?.ShowError("Database Error", "Failed to load recent files history.", ex);
            return new List<RecentFile>();
        }
    }

    public async Task AddRecentFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        try
        {
            using var db = new LensLabDbContext();
            await db.Database.EnsureCreatedAsync();

            var existing = await db.RecentFiles.FirstOrDefaultAsync(f => f.FilePath == filePath);
            if (existing != null)
            {
                existing.LastOpened = DateTime.Now;
                db.RecentFiles.Update(existing);
            }
            else
            {
                var recent = new RecentFile
                {
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    LastOpened = DateTime.Now
                };
                await db.RecentFiles.AddAsync(recent);
            }

            await db.SaveChangesAsync();

            // Prune to keep top 5
            var all = await db.RecentFiles.OrderByDescending(f => f.LastOpened).ToListAsync();
            if (all.Count > 5)
            {
                for (int i = 5; i < all.Count; i++)
                {
                    db.RecentFiles.Remove(all[i]);
                }
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding recent file: {ex.Message}");
            _dialogService?.ShowError("Database Error", "Failed to save recent file history.", ex);
        }
    }
}
