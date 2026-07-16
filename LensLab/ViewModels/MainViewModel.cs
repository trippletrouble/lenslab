using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LensLab.Models;
using LensLab.Services;
using Microsoft.EntityFrameworkCore;
using LensLab.Data;
using LensLab.Data.Entities;

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
    [NotifyPropertyChangedFor(nameof(IsImageLoaded))]
    private ImageModel? _currentImage;

    [ObservableProperty]
    private List<FilterPreset> _filters = new();

    /// <summary>
    /// Gets the list of recently opened files.
    /// </summary>
    public ObservableCollection<RecentFile> RecentFiles { get; } = new();

    /// <summary>
    /// Gets a value indicating whether a photo is currently loaded in the editor.
    /// </summary>
    public bool IsImageLoaded => CurrentImage != null;

    [ObservableProperty]
    private bool _isSavePresetDialogVisible = false;

    [ObservableProperty]
    private string _newPresetName = string.Empty;

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

        // Initialize and load recent files from SQLite database
        _ = LoadRecentFilesAsync();
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

            // Log to recent files database
            _ = AddRecentFileAsync(filePath);

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

    /// <summary>
    /// Command to show the custom save preset dialog overlay.
    /// </summary>
    [RelayCommand]
    private void ShowSavePresetDialog()
    {
        NewPresetName = string.Empty;
        IsSavePresetDialogVisible = true;
    }

    /// <summary>
    /// Command to confirm saving the custom preset and hide the dialog.
    /// </summary>
    [RelayCommand]
    private async Task SavePresetDialogConfirmAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPresetName))
        {
            StatusText = "Preset name cannot be empty";
            return;
        }

        string presetName = NewPresetName.Trim();
        IsSavePresetDialogVisible = false;
        await SavePresetInternalAsync(presetName);
    }

    /// <summary>
    /// Command to cancel the save preset dialog.
    /// </summary>
    [RelayCommand]
    private void SavePresetDialogCancel()
    {
        IsSavePresetDialogVisible = false;
    }

    /// <summary>
    /// Saves the current adjustments as a custom preset JSON file and updates the gallery.
    /// </summary>
    private async Task SavePresetInternalAsync(string presetName)
    {
        try
        {
            IsBusy = true;
            StatusText = $"Saving preset '{presetName}'...";

            // Gather active filter settings and opacity
            var activeFilter = FilterGallery.SelectedFilter?.Filter;
            double opacity = Adjustments.FilterOpacity;
            double blend = opacity / 100.0;

            var preset = new FilterPreset
            {
                Name = presetName,
                Manufacturer = "User",
                Category = "User",
                Description = $"Custom preset created on {DateTime.Now:d}",
                RecommendedFor = "Custom"
            };

            // Merge active filter preset values + manual slider adjustments
            if (activeFilter != null && opacity > 0.0)
            {
                preset.Temperature = activeFilter.Temperature * blend + Adjustments.Settings.Temperature;
                preset.Tint = activeFilter.Tint * blend + Adjustments.Settings.Tint;
                preset.RedMultiplier = 1.0 + (activeFilter.RedMultiplier - 1.0) * blend;
                preset.GreenMultiplier = 1.0 + (activeFilter.GreenMultiplier - 1.0) * blend;
                preset.BlueMultiplier = 1.0 + (activeFilter.BlueMultiplier - 1.0) * blend;

                preset.Brightness = activeFilter.Brightness * blend + Adjustments.Settings.Brightness;
                preset.Contrast = activeFilter.Contrast * blend + Adjustments.Settings.Contrast;
                preset.Saturation = activeFilter.Saturation * blend + Adjustments.Settings.Saturation;
                preset.Highlights = activeFilter.Highlights * blend + Adjustments.Settings.Highlights;
                preset.Shadows = activeFilter.Shadows * blend + Adjustments.Settings.Shadows;
                preset.Fade = activeFilter.Fade * blend + Adjustments.Settings.Fade;
                preset.Grain = activeFilter.Grain * blend + Adjustments.Settings.Grain;
                preset.Vignette = activeFilter.Vignette * blend + Adjustments.Settings.Vignette;

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
                preset.Temperature = Adjustments.Settings.Temperature;
                preset.Tint = Adjustments.Settings.Tint;
                preset.RedMultiplier = 1.0;
                preset.GreenMultiplier = 1.0;
                preset.BlueMultiplier = 1.0;

                preset.Brightness = Adjustments.Settings.Brightness;
                preset.Contrast = Adjustments.Settings.Contrast;
                preset.Saturation = Adjustments.Settings.Saturation;
                preset.Highlights = Adjustments.Settings.Highlights;
                preset.Shadows = Adjustments.Settings.Shadows;
                preset.Fade = Adjustments.Settings.Fade;
                preset.Grain = Adjustments.Settings.Grain;
                preset.Vignette = Adjustments.Settings.Vignette;
            }

            // Create directories and write file asynchronously
            string customDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CustomPresets");
            if (!Directory.Exists(customDir))
            {
                Directory.CreateDirectory(customDir);
            }

            // Sanitize file name by replacing invalid characters with underscore
            string sanitizedName = string.Join("_", presetName.Split(Path.GetInvalidFileNameChars()));
            string filePath = Path.Combine(customDir, sanitizedName + ".json");

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonContent = JsonSerializer.Serialize(preset, options);
            await File.WriteAllTextAsync(filePath, jsonContent);

            // Reload filters and update gallery
            Filters = await _filterLoaderService.LoadAllFiltersAsync();
            if (CurrentImage != null && CurrentImage.OriginalImage != null)
            {
                await FilterGallery.LoadFiltersAsync(CurrentImage.OriginalImage);
            }

            StatusText = $"Preset '{presetName}' saved successfully";
        }
        catch (Exception ex)
        {
            StatusText = "Error saving custom preset";
            System.Diagnostics.Debug.WriteLine($"Error saving preset: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Asynchronously exports the currently active image at full original resolution,
    /// applying the selected filter preset and manual slider settings, then saving to JPG or PNG.
    /// </summary>
    [RelayCommand]
    private async Task ExportImageAsync()
    {
        if (CurrentImage == null || CurrentImage.OriginalImage == null) return;

        try
        {
            // Open SaveFileDialog to choose filename and format
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JPEG Image (*.jpg;*.jpeg)|*.jpg;*.jpeg|PNG Image (*.png)|*.png",
                Title = "Export Edited Image",
                FileName = Path.GetFileNameWithoutExtension(CurrentImage.FileName) + "_edited"
            };

            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            IsBusy = true;
            StatusText = "Exporting image in full resolution...";

            string filePath = saveFileDialog.FileName;
            string ext = Path.GetExtension(filePath).ToLower();

            // Gather active filter and manual adjustments
            var filter = FilterGallery.SelectedFilter?.Filter;
            double opacity = Adjustments.FilterOpacity;
            var settings = Adjustments.Settings;

            // Render off-thread at full resolution
            BitmapSource processedBitmap = await Task.Run(async () =>
            {
                return await _imageProcessingService.ApplyEditAsync(
                    CurrentImage.OriginalImage,
                    settings,
                    filter,
                    opacity,
                    CancellationToken.None);
            });

            // Save file to disk off-thread
            await Task.Run(() =>
            {
                using var fileStream = new FileStream(filePath, FileMode.Create);
                BitmapEncoder encoder = (ext == ".png") ? (BitmapEncoder)new PngBitmapEncoder() : new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(processedBitmap));
                encoder.Save(fileStream);
            });

            StatusText = "Image exported successfully!";
        }
        catch (Exception ex)
        {
            StatusText = "Error exporting image";
            System.Diagnostics.Debug.WriteLine($"Error exporting image: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Asynchronously initializes the database schema if needed and loads the top 5 recent files.
    /// </summary>
    public async Task LoadRecentFilesAsync()
    {
        try
        {
            using var db = new LensLabDbContext();
            // Automatically create database and tables if they don't exist
            await db.Database.EnsureCreatedAsync();

            var list = await db.RecentFiles
                .OrderByDescending(f => f.LastOpened)
                .Take(5)
                .ToListAsync();

            RecentFiles.Clear();
            foreach (var item in list)
            {
                RecentFiles.Add(item);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading recent files: {ex.Message}");
        }
    }

    /// <summary>
    /// Adds a file path to the recent files database or updates its last opened timestamp.
    /// prunes older items to keep only 5.
    /// </summary>
    private async Task AddRecentFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        try
        {
            using var db = new LensLabDbContext();
            var existing = await db.RecentFiles.FirstOrDefaultAsync(f => f.FilePath == filePath);
            if (existing != null)
            {
                existing.LastOpened = DateTime.Now;
                db.RecentFiles.Update(existing);
            }
            else
            {
                var recent = new RecentFile
                {
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    LastOpened = DateTime.Now
                };
                await db.RecentFiles.AddAsync(recent);
            }

            await db.SaveChangesAsync();

            // Keep only top 5, delete older ones
            var all = await db.RecentFiles.OrderByDescending(f => f.LastOpened).ToListAsync();
            if (all.Count > 5)
            {
                for (int i = 5; i < all.Count; i++)
                {
                    db.RecentFiles.Remove(all[i]);
                }
                await db.SaveChangesAsync();
            }

            // Refresh collection
            await LoadRecentFilesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding recent file: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to open a recent file clicked from the dashboard.
    /// </summary>
    [RelayCommand]
    private async Task OpenRecentFileAsync(RecentFile file)
    {
        if (file == null) return;
        
        if (!File.Exists(file.FilePath))
        {
            System.Windows.MessageBox.Show("Die Datei existiert nicht mehr an diesem Ort.", "Fehler beim Öffnen", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return;
        }

        await OpenImageAsync(file.FilePath);
    }
}
