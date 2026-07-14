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

    // HSL Red
    [JsonPropertyName("HueRed")]
    public double HueRed { get; set; } = 0.0;
    [JsonPropertyName("SaturationRed")]
    public double SaturationRed { get; set; } = 0.0;
    [JsonPropertyName("LuminanceRed")]
    public double LuminanceRed { get; set; } = 0.0;

    // HSL Orange
    [JsonPropertyName("HueOrange")]
    public double HueOrange { get; set; } = 0.0;
    [JsonPropertyName("SaturationOrange")]
    public double SaturationOrange { get; set; } = 0.0;
    [JsonPropertyName("LuminanceOrange")]
    public double LuminanceOrange { get; set; } = 0.0;

    // HSL Yellow
    [JsonPropertyName("HueYellow")]
    public double HueYellow { get; set; } = 0.0;
    [JsonPropertyName("SaturationYellow")]
    public double SaturationYellow { get; set; } = 0.0;
    [JsonPropertyName("LuminanceYellow")]
    public double LuminanceYellow { get; set; } = 0.0;

    // HSL Green
    [JsonPropertyName("HueGreen")]
    public double HueGreen { get; set; } = 0.0;
    [JsonPropertyName("SaturationGreen")]
    public double SaturationGreen { get; set; } = 0.0;
    [JsonPropertyName("LuminanceGreen")]
    public double LuminanceGreen { get; set; } = 0.0;

    // HSL Aqua
    [JsonPropertyName("HueAqua")]
    public double HueAqua { get; set; } = 0.0;
    [JsonPropertyName("SaturationAqua")]
    public double SaturationAqua { get; set; } = 0.0;
    [JsonPropertyName("LuminanceAqua")]
    public double LuminanceAqua { get; set; } = 0.0;

    // HSL Blue
    [JsonPropertyName("HueBlue")]
    public double HueBlue { get; set; } = 0.0;
    [JsonPropertyName("SaturationBlue")]
    public double SaturationBlue { get; set; } = 0.0;
    [JsonPropertyName("LuminanceBlue")]
    public double LuminanceBlue { get; set; } = 0.0;

    // HSL Purple
    [JsonPropertyName("HuePurple")]
    public double HuePurple { get; set; } = 0.0;
    [JsonPropertyName("SaturationPurple")]
    public double SaturationPurple { get; set; } = 0.0;
    [JsonPropertyName("LuminancePurple")]
    public double LuminancePurple { get; set; } = 0.0;

    // HSL Magenta
    [JsonPropertyName("HueMagenta")]
    public double HueMagenta { get; set; } = 0.0;
    [JsonPropertyName("SaturationMagenta")]
    public double SaturationMagenta { get; set; } = 0.0;
    [JsonPropertyName("LuminanceMagenta")]
    public double LuminanceMagenta { get; set; } = 0.0;

    // Preset Metadata
    [JsonPropertyName("Category")]
    public string Category { get; set; } = "General";

    [JsonPropertyName("Description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("RecommendedFor")]
    public string RecommendedFor { get; set; } = "";
}
