using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LensLab.Models;
using LensLab.Services;

namespace LensLab.ViewModels;

/// <summary>
/// The main ViewModel for the application. Controls general UI state,
/// status message logging, and orchestrates actions across views.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IFileDialogService _fileDialogService;
    private readonly FilterLoaderService _filterLoaderService;
    private readonly ImageProcessingService _imageProcessingService;
    private CancellationTokenSource? _renderCts;

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private bool _isBusy = false;

    [ObservableProperty]
    private ImageModel? _currentImage;

    [ObservableProperty]
    private List<FilterPreset> _filters = new();

    /// <summary>
    /// Gets the ViewModel for the filter gallery.
    /// </summary>
    public FilterGalleryViewModel FilterGallery { get; }

    /// <summary>
    /// Gets the ViewModel for manual slider adjustments.
    /// </summary>
    public AdjustmentViewModel Adjustments { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    public MainViewModel()
    {
        _fileDialogService = new FileDialogService();
        _filterLoaderService = new FilterLoaderService();
        _imageProcessingService = new ImageProcessingService();
        
        FilterGallery = new FilterGalleryViewModel();
        FilterGallery.FilterSelected += OnFilterSelected;

        Adjustments = new AdjustmentViewModel();
        Adjustments.AdjustmentChanged += OnAdjustmentChanged;
    }

    /// <summary>
    /// Asynchronously initializes the ViewModel, loading all available filters.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    public async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            StatusText = "Loading filters...";
            Filters = await _filterLoaderService.LoadAllFiltersAsync();
            StatusText = $"{Filters.Count} filters loaded";
        }
        catch (Exception ex)
        {
            StatusText = "Error loading filters";
            System.Diagnostics.Debug.WriteLine($"Error initializing filters: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Handler that executes whenever a slider value or opacity changes.
    /// Cancels any active rendering, debounces inputs, and starts a new one on the Preview image.
    /// </summary>
    private async void OnAdjustmentChanged(object? sender, EventArgs e)
    {
        if (CurrentImage == null || CurrentImage.PreviewImage == null) return;

        try
        {
            _renderCts?.Cancel();
            _renderCts = new CancellationTokenSource();
            var token = _renderCts.Token;


            // Get selected filter from gallery
            var filter = FilterGallery.SelectedFilter?.Filter;

            var processedImage = await _imageProcessingService.ApplyEditAsync(
                CurrentImage.PreviewImage,
                Adjustments.Settings,
                filter,
                Adjustments.FilterOpacity,
                token);

            CurrentImage.CurrentImage = processedImage;
            OnPropertyChanged(nameof(CurrentImage));
        }
        catch (OperationCanceledException)
        {
            // Silently ignore cancellation
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying adjustments: {ex.Message}");
        }
    }

    /// <summary>
    /// Handler that executes whenever a filter selection changes in the gallery.
    /// Applies the selected filter to the preview image and updates the viewport.
    /// </summary>
    private async void OnFilterSelected(object? sender, FilterPreset? filter)
    {
        if (CurrentImage == null || CurrentImage.PreviewImage == null) return;

        try
        {
            IsBusy = true;
            StatusText = filter != null ? $"Applying {filter.Name}..." : "Removing filter...";

            _renderCts?.Cancel();
            _renderCts = new CancellationTokenSource();
            var token = _renderCts.Token;

            // Apply selected filter combined with current slider settings
            var processedImage = await _imageProcessingService.ApplyEditAsync(
                CurrentImage.PreviewImage,
                Adjustments.Settings,
                filter,
                Adjustments.FilterOpacity,
                token);

            CurrentImage.CurrentImage = processedImage;
            OnPropertyChanged(nameof(CurrentImage));
            
            StatusText = filter != null ? $"{filter.Name} applied" : "Filter removed";
        }
        catch (OperationCanceledException)
        {
            // Silently ignore cancellation
        }
        catch (Exception ex)
        {
            StatusText = "Error applying filter";
            System.Diagnostics.Debug.WriteLine($"Error applying filter: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Asynchronously opens an image file. If no filePath is provided, shows the file dialog.
    /// Loads the image into memory, generates a 1200px scaled preview version, and sets CurrentImage.
    /// </summary>
    /// <param name="filePath">Optional file path to load directly.</param>
    [RelayCommand]
    private async Task OpenImageAsync(string? filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = _fileDialogService.OpenImageFile();
            }

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            IsBusy = true;
            StatusText = "Loading image...";

            // Load the image asynchronously to prevent UI freezing
            BitmapSource loadedBitmap = await Task.Run(() =>
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze(); // Crucial for cross-thread access in WPF
                return (BitmapSource)bitmap;
            });

            // Generate scaled preview (max 1200px dimension) for live UI slider updates
            BitmapSource previewBitmap = await Task.Run(() =>
            {
                double maxDim = Math.Max(loadedBitmap.PixelWidth, loadedBitmap.PixelHeight);
                if (maxDim > 1200.0)
                {
                    double scale = 1200.0 / maxDim;
                    var scaleTransform = new ScaleTransform(scale, scale);
                    var resized = new TransformedBitmap(loadedBitmap, scaleTransform);
                    resized.Freeze();
                    return (BitmapSource)resized;
                }
                return loadedBitmap;
            });

            CurrentImage = new ImageModel
            {
                OriginalImage = loadedBitmap,
                CurrentImage = previewBitmap,
                PreviewImage = previewBitmap,
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Width = loadedBitmap.PixelWidth,
                Height = loadedBitmap.PixelHeight
            };

            StatusText = CurrentImage.FileName ?? "Image loaded";

            // Trigger thumbnail previews generation in parallel
            await FilterGallery.LoadFiltersAsync(loadedBitmap);
        }
        catch (Exception ex)
        {
            StatusText = "Error loading image";
            System.Diagnostics.Debug.WriteLine($"Error loading image: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
