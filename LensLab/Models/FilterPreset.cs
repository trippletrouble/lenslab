using System.Text.Json.Serialization;

namespace LensLab.Models;

/// <summary>
/// Represents a film emulation filter preset containing pixel adjustment values.
/// This is a pure data container used for deserializing presets from JSON configuration files.
/// </summary>
public class FilterPreset
{
    /// <summary>
    /// Gets or sets the display name of the filter preset.
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the film manufacturer (e.g., Fujifilm, Kodak).
    /// </summary>
    [JsonPropertyName("Manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the overall brightness adjustment (Range: -100 to +100).
    /// </summary>
    [JsonPropertyName("Brightness")]
    public double Brightness { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the contrast adjustment (Range: -100 to +100).
    /// </summary>
    [JsonPropertyName("Contrast")]
    public double Contrast { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the color saturation adjustment (Range: -100 to +100).
    /// </summary>
    [JsonPropertyName("Saturation")]
    public double Saturation { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the temperature adjustment (Range: -100 to +100).
    /// Positive values shift towards warm yellow/orange, negative values shift towards cool blue.
    /// </summary>
    [JsonPropertyName("Temperature")]
    public double Temperature { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the tint adjustment (Range: -100 to +100).
    /// Positive values shift towards green, negative values shift towards magenta.
    /// </summary>
    [JsonPropertyName("Tint")]
    public double Tint { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the red channel multiplier factor (Default: 1.0).
    /// </summary>
    [JsonPropertyName("RedMultiplier")]
    public double RedMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the green channel multiplier factor (Default: 1.0).
    /// </summary>
    [JsonPropertyName("GreenMultiplier")]
    public double GreenMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the blue channel multiplier factor (Default: 1.0).
    /// </summary>
    [JsonPropertyName("BlueMultiplier")]
    public double BlueMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the grain effect intensity (Range: 0 to 100).
    /// </summary>
    [JsonPropertyName("Grain")]
    public double Grain { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the vignette effect intensity (Range: 0 to 100).
    /// </summary>
    [JsonPropertyName("Vignette")]
    public double Vignette { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the matte/shadow fade effect intensity (Range: 0 to 100).
    /// </summary>
    [JsonPropertyName("Fade")]
    public double Fade { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the highlights adjustment (Range: -100 to +100).
    /// </summary>
    [JsonPropertyName("Highlights")]
    public double Highlights { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the shadows adjustment (Range: -100 to +100).
    /// </summary>
    [JsonPropertyName("Shadows")]
    public double Shadows { get; set; } = 0.0;
}
