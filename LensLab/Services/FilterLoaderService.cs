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
/// Custom presets are loaded from %APPDATA%/LensLab/CustomPresets.
/// </summary>
public class FilterLoaderService : IFilterLoaderService
{
    private readonly IDialogService? _dialogService;

    public FilterLoaderService(IDialogService? dialogService = null)
    {
        _dialogService = dialogService;
    }

    /// <summary>
    /// Asynchronously loads all filter presets (*.json) from "Resources/Filters/" and AppData custom directory.
    /// </summary>
    public async Task<List<FilterPreset>> LoadAllFiltersAsync()
    {
        return await Task.Run(() =>
        {
            var presets = new List<FilterPreset>();

            string filtersDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Filters");
            
            // Custom presets stored in user AppData folder (%APPDATA%/LensLab/CustomPresets)
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string customDir = Path.Combine(appData, "LensLab", "CustomPresets");

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
                        Debug.WriteLine($"Failed to load filter preset from file '{file}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error scanning files: {ex.Message}");
                _dialogService?.ShowError("Filter Loading Error", "Failed to scan preset directory.", ex);
            }

            // Sort presets first by Manufacturer, then by Name
            return presets
                .OrderBy(p => p.Manufacturer, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        });
    }
}
