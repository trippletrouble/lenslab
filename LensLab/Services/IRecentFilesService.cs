using System.Collections.Generic;
using System.Threading.Tasks;
using LensLab.Data.Entities;

namespace LensLab.Services;

/// <summary>
/// Service interface for managing recent files in local SQLite EF Core database.
/// </summary>
public interface IRecentFilesService
{
    /// <summary>
    /// Fetches the top 5 most recently opened files from SQLite database.
    /// </summary>
    Task<List<RecentFile>> GetRecentFilesAsync();

    /// <summary>
    /// Adds or updates a recent file path and prunes history down to 5 items.
    /// </summary>
    Task AddRecentFileAsync(string filePath);
}
