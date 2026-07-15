using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LensLab.Models;

namespace LensLab.Services;

/// <summary>
/// Service responsible for loading and deserializing film emulation filter presets from local JSON files.
/// </summary>
public class FilterLoaderService
{
    /// <summary>
    /// Asynchronously loads all filter presets (*.json) from the "Resources/Filters/" directory.
    /// In case of deserialization errors in individual files, the error is logged to debug output and the file is skipped.
    /// </summary>
    /// <returns>A sorted list of filter presets, ordered by Manufacturer, then by Name.</returns>
    public async Task<List<FilterPreset>> LoadAllFiltersAsync()
    {
        return await Task.Run(() =>
        {
            var presets = new List<FilterPreset>();

            string filtersDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Filters");
            string customDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CustomPresets");

            // Ensure custom presets folder exists
            try
            {
                if (!Directory.Exists(customDir))
                {
                    Directory.CreateDirectory(customDir);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to create custom presets directory: {ex.Message}");
            }

            var files = new List<string>();
            if (Directory.Exists(filtersDir))
            {
                try
                {
                    files.AddRange(Directory.GetFiles(filtersDir, "*.json"));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error scanning filters directory: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine($"Filters directory not found at: {filtersDir}");
            }

            if (Directory.Exists(customDir))
            {
                try
                {
                    files.AddRange(Directory.GetFiles(customDir, "*.json"));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error scanning custom presets directory: {ex.Message}");
                }
            }

            if (files.Count == 0)
            {
                return presets;
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                foreach (var file in files)
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(file);
                        var preset = JsonSerializer.Deserialize<FilterPreset>(jsonContent, options);
                        if (preset != null)
                        {
                            presets.Add(preset);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error and skip the malformed preset file to prevent app-wide failures
                        Debug.WriteLine($"Failed to load filter preset from file '{file}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error scanning files: {ex.Message}");
            }

            // Sort presets first by Manufacturer, then by Name
            return presets
                .OrderBy(p => p.Manufacturer, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        });
    }
}
