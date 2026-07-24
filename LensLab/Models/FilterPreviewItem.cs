using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LensLab.Models;

/// <summary>
/// Represents a preview item for a filter preset, containing the preset details,
/// a processed thumbnail image, and selection status.
/// </summary>
public partial class FilterPreviewItem : ObservableObject
{
    /// <summary>
    /// Gets or sets the filter preset associated with this preview.
    /// </summary>
    [ObservableProperty]
    private FilterPreset _filter = null!;

    /// <summary>
    /// Gets or sets the thumbnail image with the filter applied.
    /// </summary>
    [ObservableProperty]
    private BitmapSource? _thumbnail;

    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// Gets a value indicating whether this is a custom preset created by the user.
    /// </summary>
    public bool IsCustom => Filter?.Manufacturer != null && Filter.Manufacturer.Equals("User", System.StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterPreviewItem"/> class.
    /// </summary>
    public FilterPreviewItem()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterPreviewItem"/> class with parameters.
    /// </summary>
    /// <param name="filter">The filter preset.</param>
    /// <param name="thumbnail">The thumbnail image.</param>
    public FilterPreviewItem(FilterPreset filter, BitmapSource? thumbnail)
    {
        Filter = filter;
        Thumbnail = thumbnail;
    }
}
