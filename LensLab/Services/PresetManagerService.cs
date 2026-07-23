using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LensLab.Models;

namespace LensLab.Services;

/// <summary>
/// Default implementation of <see cref="IPresetManagerService"/>.
/// Saves custom presets to %APPDATA%/LensLab/CustomPresets.
/// </summary>
public class PresetManagerService : IPresetManagerService
{
    private readonly IDialogService _dialogService;

    public PresetManagerService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public async Task<bool> SaveCustomPresetAsync(string presetName, FilterPreset? activeFilter, double filterOpacity, AdjustmentSettings settings)
    {
        if (string.IsNullOrWhiteSpace(presetName))
        {
            _dialogService.ShowWarning("Save Preset Warning", "Preset name cannot be empty.");
            return false;
        }

        try
        {
            double blend = filterOpacity / 100.0;

            var preset = new FilterPreset
            {
                Name = presetName.Trim(),
                Manufacturer = "User",
                Category = "User",
                Description = $"Custom preset created on {DateTime.Now:d}",
                RecommendedFor = "Custom"
            };

            // Merge active filter preset values + manual slider adjustments
            if (activeFilter != null && filterOpacity > 0.0)
            {
                preset.Temperature = activeFilter.Temperature * blend + settings.Temperature;
                preset.Tint = activeFilter.Tint * blend + settings.Tint;
                preset.RedMultiplier = 1.0 + (activeFilter.RedMultiplier - 1.0) * blend;
                preset.GreenMultiplier = 1.0 + (activeFilter.GreenMultiplier - 1.0) * blend;
                preset.BlueMultiplier = 1.0 + (activeFilter.BlueMultiplier - 1.0) * blend;

                preset.Brightness = activeFilter.Brightness * blend + settings.Brightness;
                preset.Contrast = activeFilter.Contrast * blend + settings.Contrast;
                preset.Saturation = activeFilter.Saturation * blend + settings.Saturation;
                preset.Highlights = activeFilter.Highlights * blend + settings.Highlights;
                preset.Shadows = activeFilter.Shadows * blend + settings.Shadows;
                preset.Fade = activeFilter.Fade * blend + settings.Fade;
                preset.Grain = activeFilter.Grain * blend + settings.Grain;
                preset.Vignette = activeFilter.Vignette * blend + settings.Vignette;

                // Blend HSL offsets
                preset.HueRed = activeFilter.HueRed * blend;
                preset.SaturationRed = activeFilter.SaturationRed * blend;
                preset.LuminanceRed = activeFilter.LuminanceRed * blend;

                preset.HueOrange = activeFilter.HueOrange * blend;
                preset.SaturationOrange = activeFilter.SaturationOrange * blend;
                preset.LuminanceOrange = activeFilter.LuminanceOrange * blend;

                preset.HueYellow = activeFilter.HueYellow * blend;
                preset.SaturationYellow = activeFilter.SaturationYellow * blend;
                preset.LuminanceYellow = activeFilter.LuminanceYellow * blend;

                preset.HueGreen = activeFilter.HueGreen * blend;
                preset.SaturationGreen = activeFilter.SaturationGreen * blend;
                preset.LuminanceGreen = activeFilter.LuminanceGreen * blend;

                preset.HueAqua = activeFilter.HueAqua * blend;
                preset.SaturationAqua = activeFilter.SaturationAqua * blend;
                preset.LuminanceAqua = activeFilter.LuminanceAqua * blend;

                preset.HueBlue = activeFilter.HueBlue * blend;
                preset.SaturationBlue = activeFilter.SaturationBlue * blend;
                preset.LuminanceBlue = activeFilter.LuminanceBlue * blend;

                preset.HuePurple = activeFilter.HuePurple * blend;
                preset.SaturationPurple = activeFilter.SaturationPurple * blend;
                preset.LuminancePurple = activeFilter.LuminancePurple * blend;

                preset.HueMagenta = activeFilter.HueMagenta * blend;
                preset.SaturationMagenta = activeFilter.SaturationMagenta * blend;
                preset.LuminanceMagenta = activeFilter.LuminanceMagenta * blend;
            }
            else
            {
                preset.Temperature = settings.Temperature;
                preset.Tint = settings.Tint;
                preset.RedMultiplier = 1.0;
                preset.GreenMultiplier = 1.0;
                preset.BlueMultiplier = 1.0;

                preset.Brightness = settings.Brightness;
                preset.Contrast = settings.Contrast;
                preset.Saturation = settings.Saturation;
                preset.Highlights = settings.Highlights;
                preset.Shadows = settings.Shadows;
                preset.Fade = settings.Fade;
                preset.Grain = settings.Grain;
                preset.Vignette = settings.Vignette;
            }

            // Save under %APPDATA%/LensLab/CustomPresets
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string customDir = Path.Combine(appData, "LensLab", "CustomPresets");

            if (!Directory.Exists(customDir))
            {
                Directory.CreateDirectory(customDir);
            }

            string sanitizedName = string.Join("_", presetName.Split(Path.GetInvalidFileNameChars()));
            string filePath = Path.Combine(customDir, sanitizedName + ".json");

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonContent = JsonSerializer.Serialize(preset, options);
            await File.WriteAllTextAsync(filePath, jsonContent);

            _dialogService.ShowInformation("Preset Saved", $"Preset '{presetName}' saved successfully.");
            return true;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError("Preset Save Error", $"Failed to save custom preset '{presetName}'.", ex);
            return false;
        }
    }
}
