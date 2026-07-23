using System.Threading.Tasks;
using LensLab.Models;

namespace LensLab.Services;

/// <summary>
/// Service interface for creating and saving custom filter presets to %APPDATA%/LensLab/CustomPresets.
/// </summary>
public interface IPresetManagerService
{
    /// <summary>
    /// Blends current slider settings and active filter properties into a custom FilterPreset JSON file.
    /// </summary>
    Task<bool> SaveCustomPresetAsync(string presetName, FilterPreset? activeFilter, double filterOpacity, AdjustmentSettings settings);
}
