using System.Collections.Generic;
using System.Threading.Tasks;
using LensLab.Models;

namespace LensLab.Services;

/// <summary>
/// Service interface for loading and deserializing film emulation presets from JSON configuration files.
/// </summary>
public interface IFilterLoaderService
{
    /// <summary>
    /// Asynchronously loads all filter presets (*.json) from standard resources and AppData custom preset folders.
    /// </summary>
    /// <returns>A list of sorted filter presets.</returns>
    Task<List<FilterPreset>> LoadAllFiltersAsync();
}
