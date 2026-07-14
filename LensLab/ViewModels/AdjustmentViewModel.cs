using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LensLab.Models;

namespace LensLab.ViewModels;

/// <summary>
/// ViewModel managing user manual slider adjustments and filter strength.
/// </summary>
public partial class AdjustmentViewModel : ObservableObject
{
    /// <summary>
    /// Gets the settings container holding the raw slider values.
    /// </summary>
    public AdjustmentSettings Settings { get; } = new();

    [ObservableProperty]
    private double _filterOpacity = 100.0;

    /// <summary>
    /// Event triggered when any slider adjustment value or filter opacity changes.
    /// </summary>
    public event EventHandler? AdjustmentChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdjustmentViewModel"/> class.
    /// </summary>
    public AdjustmentViewModel()
    {
        // Bubble up property changed notifications from settings container
        Settings.PropertyChanged += OnSettingsPropertyChanged;
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        AdjustmentChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnFilterOpacityChanged(double value)
    {
        AdjustmentChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Command to reset all adjustments and opacity back to defaults.
    /// </summary>
    [RelayCommand]
    private void ResetAll()
    {
        Settings.Reset();
        FilterOpacity = 100.0;
    }
}
