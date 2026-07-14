using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using LensLab.Models;
using LensLab.Services;

namespace LensLab.ViewModels;

/// <summary>
/// ViewModel managing the collection of filter previews and selection logic.
/// </summary>
public partial class FilterGalleryViewModel : ObservableObject
{
    private readonly FilterLoaderService _filterLoaderService;
    private readonly ImageProcessingService _imageProcessingService;

    [ObservableProperty]
    private ObservableCollection<FilterPreviewItem> _filters = new();

    [ObservableProperty]
    private FilterPreviewItem? _selectedFilter;

    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Event triggered when a filter preset is selected.
    /// Passes the selected preset (or null if deselected).
    /// </summary>
    public event EventHandler<FilterPreset?>? FilterSelected;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterGalleryViewModel"/> class.
    /// </summary>
    public FilterGalleryViewModel()
    {
        _filterLoaderService = new FilterLoaderService();
        _imageProcessingService = new ImageProcessingService();
    }

    /// <summary>
    /// Asynchronously loads all filter presets, scales down the source image,
    /// applies the presets in parallel, and populates the filter list with thumbnails.
    /// </summary>
    /// <param name="sourceImage">The source image to generate thumbnails from.</param>
    public async Task LoadFiltersAsync(BitmapSource sourceImage)
    {
        if (sourceImage == null) return;

        try
        {
            IsLoading = true;

            // Load presets from JSON files
            var loadedPresets = await _filterLoaderService.LoadAllFiltersAsync();

            // Sort presets by category order (Portrait -> Vintage -> Street -> Cinematic -> Monochrome)
            Func<string, int> getCategoryWeight = (cat) => cat switch
            {
                "Portrait" => 0,
                "Vintage & Warm" => 1,
                "Street & Moody" => 2,
                "Cinematic & Flat" => 3,
                "Monochrome" => 4,
                _ => 5
            };

            var presets = loadedPresets
                .OrderBy(p => getCategoryWeight(p.Category))
                .ThenBy(p => p.Name)
                .ToList();

            // Downscale the image to 120px width to speed up thumbnail generation
            double scale = 120.0 / sourceImage.PixelWidth;
            var scaleTransform = new ScaleTransform(scale, scale);
            var resizedBitmap = new TransformedBitmap(sourceImage, scaleTransform);
            resizedBitmap.Freeze();

            // Generate thumbnails in parallel
            var tasks = presets.Select(async preset =>
            {
                // Applying the filter to the downscaled thumbnail
                var thumbnail = await _imageProcessingService.ApplyEditAsync(
                    resizedBitmap, 
                    new AdjustmentSettings(), 
                    preset, 
                    CancellationToken.None);
                
                return new FilterPreviewItem(preset, thumbnail);
            });

            var previewItems = await Task.WhenAll(tasks);

            Filters.Clear();
            foreach (var item in previewItems)
            {
                Filters.Add(item);
            }

            // Select none initially
            SelectedFilter = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading gallery: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedFilterChanged(FilterPreviewItem? oldValue, FilterPreviewItem? newValue)
    {
        // Adjust the IsSelected states in the previews
        if (oldValue != null) oldValue.IsSelected = false;
        if (newValue != null) newValue.IsSelected = true;

        FilterSelected?.Invoke(this, newValue?.Filter);
    }
}
