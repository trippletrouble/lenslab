using CommunityToolkit.Mvvm.ComponentModel;

namespace LensLab.Models;

/// <summary>
/// Represents the user adjustment settings for photo editing.
/// This model notifies subscribers of changes so that properties can be bound directly to UI sliders.
/// </summary>
public partial class AdjustmentSettings : ObservableObject
{
    [ObservableProperty]
    private double _brightness = 0.0;

    [ObservableProperty]
    private double _contrast = 0.0;

    [ObservableProperty]
    private double _saturation = 0.0;

    [ObservableProperty]
    private double _temperature = 0.0;

    [ObservableProperty]
    private double _tint = 0.0;

    [ObservableProperty]
    private double _grain = 0.0;

    [ObservableProperty]
    private double _vignette = 0.0;

    [ObservableProperty]
    private double _fade = 0.0;

    [ObservableProperty]
    private double _highlights = 0.0;

    [ObservableProperty]
    private double _shadows = 0.0;

    /// <summary>
    /// Resets all adjustment settings to their default values (0.0).
    /// </summary>
    public void Reset()
    {
        Brightness = 0.0;
        Contrast = 0.0;
        Saturation = 0.0;
        Temperature = 0.0;
        Tint = 0.0;
        Grain = 0.0;
        Vignette = 0.0;
        Fade = 0.0;
        Highlights = 0.0;
        Shadows = 0.0;
    }
}
