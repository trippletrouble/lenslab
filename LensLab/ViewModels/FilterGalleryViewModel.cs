using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LensLab.Models;
using LensLab.Services;
using Microsoft.Win32;

namespace LensLab.ViewModels;

/// <summary>
/// ViewModel managing the collection of filter previews and selection logic.
/// </summary>
public partial class FilterGalleryViewModel : ObservableObject
{
    private readonly FilterLoaderService _filterLoaderService;
    private readonly ImageProcessingService _imageProcessingService;
    private BitmapSource? _originalImageSource;

    [ObservableProperty]
    private ObservableCollection<FilterPreviewItem> _filters = new();

    [ObservableProperty]
    private FilterPreviewItem? _selectedFilter;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isDeleteConfirmDialogVisible;

    [ObservableProperty]
    private FilterPreviewItem? _presetToDelete;

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
        _originalImageSource = sourceImage;

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

    /// <summary>
    /// Selects the next filter in the gallery list.
    /// </summary>
    public void SelectNextFilter()
    {
        if (Filters == null || Filters.Count == 0) return;
        
        int index = SelectedFilter != null ? Filters.IndexOf(SelectedFilter) : -1;
        if (index < Filters.Count - 1)
        {
            SelectedFilter = Filters[index + 1];
        }
    }

    /// <summary>
    /// Selects the previous filter in the gallery list.
    /// </summary>
    public void SelectPreviousFilter()
    {
        if (Filters == null || Filters.Count == 0) return;
        
        int index = SelectedFilter != null ? Filters.IndexOf(SelectedFilter) : -1;
        if (index > 0)
        {
            SelectedFilter = Filters[index - 1];
        }
        else if (index == -1 && Filters.Count > 0)
        {
            SelectedFilter = Filters[0];
        }
    }

    /// <summary>
    /// Command to delete a custom filter preset.
    /// </summary>
    [RelayCommand]
    private void DeletePreset(FilterPreviewItem item)
    {
        if (item == null || !item.IsCustom) return;

        try
        {
            // Sanitize file name
            string sanitizedName = string.Join("_", item.Filter.Name.Split(System.IO.Path.GetInvalidFileNameChars()));
            string customDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CustomPresets");
            string filePath = System.IO.Path.Combine(customDir, sanitizedName + ".json");

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Remove from collection
            Filters.Remove(item);

            // If it was selected, clear selection
            if (SelectedFilter == item)
            {
                SelectedFilter = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting preset: {ex.Message}");
        }
    }

    /// <summary>
    /// Shows the deletion confirmation overlay for the specified preset.
    /// </summary>
    [RelayCommand]
    private void ShowDeleteConfirm(FilterPreviewItem item)
    {
        if (item == null) return;
        PresetToDelete = item;
        IsDeleteConfirmDialogVisible = true;
    }

    /// <summary>
    /// Confirms the deletion of the preset, executes the delete command, and closes the overlay.
    /// </summary>
    [RelayCommand]
    private void ConfirmDelete()
    {
        if (PresetToDelete != null)
        {
            DeletePreset(PresetToDelete);
            PresetToDelete = null;
        }
        IsDeleteConfirmDialogVisible = false;
    }

    /// <summary>
    /// Cancels the deletion and closes the overlay.
    /// </summary>
    [RelayCommand]
    private void CancelDelete()
    {
        PresetToDelete = null;
        IsDeleteConfirmDialogVisible = false;
    }

    /// <summary>
    /// Command to export the currently selected custom filter preset as a JSON file.
    /// </summary>
    [RelayCommand]
    private void ExportPreset()
    {
        if (SelectedFilter == null || !SelectedFilter.IsCustom) return;

        try
        {
            // Sanitize file name for default dialog name
            string sanitizedName = string.Join("_", SelectedFilter.Filter.Name.Split(Path.GetInvalidFileNameChars()));

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON Preset File (*.json)|*.json",
                Title = "Export Preset File",
                FileName = sanitizedName
            };

            if (saveFileDialog.ShowDialog() != true) return;

            string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CustomPresets", sanitizedName + ".json");

            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, saveFileDialog.FileName, overwrite: true);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error exporting preset: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to import a custom preset JSON file, validate it, and copy it to the local app presets directory.
    /// </summary>
    [RelayCommand]
    private async Task ImportPresetAsync()
    {
        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Preset File (*.json)|*.json",
                Title = "Import Custom Preset"
            };

            if (openFileDialog.ShowDialog() != true) return;

            string sourcePath = openFileDialog.FileName;
            string jsonContent = await File.ReadAllTextAsync(sourcePath);

            // Validate format by trying to deserialize
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var preset = JsonSerializer.Deserialize<FilterPreset>(jsonContent, options);

            if (preset == null || string.IsNullOrWhiteSpace(preset.Name))
            {
                System.Windows.MessageBox.Show("Invalid preset file format. The preset must contain a valid Name.", "Import Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            // Standardize fields to force User categorization
            preset.Manufacturer = "User";
            preset.Category = "User";
            if (string.IsNullOrWhiteSpace(preset.RecommendedFor))
            {
                preset.RecommendedFor = "Custom";
            }

            // Save to local custom directory
            string customDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CustomPresets");
            if (!Directory.Exists(customDir))
            {
                Directory.CreateDirectory(customDir);
            }

            string destFileName = string.Join("_", preset.Name.Split(Path.GetInvalidFileNameChars())) + ".json";
            string destPath = Path.Combine(customDir, destFileName);

            var writeOptions = new JsonSerializerOptions { WriteIndented = true };
            string formattedJson = JsonSerializer.Serialize(preset, writeOptions);
            await File.WriteAllTextAsync(destPath, formattedJson);

            // Reload gallery previews if we have a loaded original image reference
            if (_originalImageSource != null)
            {
                await LoadFiltersAsync(_originalImageSource);
            }
            else
            {
                System.Windows.MessageBox.Show($"Preset '{preset.Name}' imported successfully. It will be visible once an image is loaded.", "Import Successful", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error importing preset: {ex.Message}", "Import Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
