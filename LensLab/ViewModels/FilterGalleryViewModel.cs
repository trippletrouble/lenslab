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

namespace LensLab.ViewModels;

/// <summary>
/// ViewModel managing the collection of filter previews and selection logic.
/// </summary>
public partial class FilterGalleryViewModel : ObservableObject
{
    private readonly IFilterLoaderService _filterLoaderService;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IDialogService? _dialogService;
    private readonly IFileDialogService? _fileDialogService;
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
    /// Initializes a new instance of the <see cref="FilterGalleryViewModel"/> class with dependency injection.
    /// </summary>
    public FilterGalleryViewModel(
        IFilterLoaderService filterLoaderService,
        IImageProcessingService imageProcessingService,
        IDialogService? dialogService = null,
        IFileDialogService? fileDialogService = null)
    {
        _filterLoaderService = filterLoaderService;
        _imageProcessingService = imageProcessingService;
        _dialogService = dialogService;
        _fileDialogService = fileDialogService;
    }

    /// <summary>
    /// Helper method to get the AppData custom presets directory (%APPDATA%/LensLab/CustomPresets).
    /// </summary>
    private string GetCustomPresetsDirectory()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "LensLab", "CustomPresets");
    }

    /// <summary>
    /// Asynchronously loads all filter presets, scales down the source image,
    /// applies the presets in parallel, and populates the filter list with thumbnails.
    /// </summary>
    public async Task LoadFiltersAsync(BitmapSource sourceImage)
    {
        if (sourceImage == null) return;

        try
        {
            IsLoading = true;
            _originalImageSource = sourceImage;

            // Load presets asynchronously
            var presets = await _filterLoaderService.LoadAllFiltersAsync();

            // Downscale source image to 120px width for fast thumbnail rendering
            BitmapSource thumbnailSource = await Task.Run(() =>
            {
                double targetWidth = 120.0;
                double scale = targetWidth / sourceImage.PixelWidth;
                var scaleTransform = new ScaleTransform(scale, scale);
                var resized = new TransformedBitmap(sourceImage, scaleTransform);
                resized.Freeze();
                return (BitmapSource)resized;
            });

            // Process thumbnails in parallel off-thread
            var emptySettings = new AdjustmentSettings();

            var previewItems = await Task.Run(async () =>
            {
                var items = new List<FilterPreviewItem>();

                // Add "None" option first
                items.Add(new FilterPreviewItem
                {
                    Filter = new FilterPreset { Name = "Original", Category = "Default" },
                    Thumbnail = thumbnailSource,
                    IsSelected = SelectedFilter == null || SelectedFilter.Filter.Name == "Original"
                });

                var tasks = presets.Select(async preset =>
                {
                    var processedThumbnail = await _imageProcessingService.ApplyEditAsync(
                        thumbnailSource,
                        emptySettings,
                        preset,
                        100.0,
                        CancellationToken.None);

                    return new FilterPreviewItem
                    {
                        Filter = preset,
                        Thumbnail = processedThumbnail,
                        IsSelected = SelectedFilter != null && SelectedFilter.Filter.Name == preset.Name
                    };
                });

                var results = await Task.WhenAll(tasks);
                items.AddRange(results);
                return items;
            });

            Filters = new ObservableCollection<FilterPreviewItem>(previewItems);

            // Re-select currently selected filter if it exists in the new list
            if (SelectedFilter != null)
            {
                var match = Filters.FirstOrDefault(f => f.Filter.Name == SelectedFilter.Filter.Name);
                if (match != null)
                {
                    SelectedFilter = match;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading filter previews: {ex.Message}");
            _dialogService?.ShowError("Filter Preview Error", "Failed to generate filter preview thumbnails.", ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedFilterChanged(FilterPreviewItem? value)
    {
        // Update IsSelected property on all preview items
        foreach (var item in Filters)
        {
            item.IsSelected = (item == value);
        }

        // Trigger selection event (null if "Original" / None is selected)
        FilterPreset? presetToApply = (value != null && value.Filter.Name != "Original") ? value.Filter : null;
        FilterSelected?.Invoke(this, presetToApply);
    }

    /// <summary>
    /// Command to select the next filter in the gallery.
    /// </summary>
    [RelayCommand]
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
    /// Command to select the previous filter in the gallery.
    /// </summary>
    [RelayCommand]
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
            string sanitizedName = string.Join("_", item.Filter.Name.Split(Path.GetInvalidFileNameChars()));
            string customDir = GetCustomPresetsDirectory();
            string filePath = Path.Combine(customDir, sanitizedName + ".json");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            Filters.Remove(item);

            if (SelectedFilter == item)
            {
                SelectedFilter = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting preset: {ex.Message}");
            _dialogService?.ShowError("Preset Delete Error", $"Failed to delete preset '{item.Filter.Name}'.", ex);
        }
    }

    [RelayCommand]
    private void ShowDeleteConfirm(FilterPreviewItem item)
    {
        if (item == null) return;
        PresetToDelete = item;
        IsDeleteConfirmDialogVisible = true;
    }

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
            string sanitizedName = string.Join("_", SelectedFilter.Filter.Name.Split(Path.GetInvalidFileNameChars()));
            string? destinationPath = _fileDialogService?.SaveImageFile(sanitizedName);

            if (string.IsNullOrEmpty(destinationPath)) return;

            string sourcePath = Path.Combine(GetCustomPresetsDirectory(), sanitizedName + ".json");

            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, destinationPath, overwrite: true);
                _dialogService?.ShowInformation("Preset Export", "Custom preset exported successfully.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error exporting preset: {ex.Message}");
            _dialogService?.ShowError("Preset Export Error", "Failed to export preset file.", ex);
        }
    }

    /// <summary>
    /// Command to import a custom preset JSON file, validate it, and copy it to AppData custom presets.
    /// </summary>
    [RelayCommand]
    private async Task ImportPresetAsync()
    {
        try
        {
            string? sourcePath = _fileDialogService?.OpenImageFile(); // or file picker
            if (string.IsNullOrEmpty(sourcePath)) return;

            string jsonContent = await File.ReadAllTextAsync(sourcePath);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var preset = JsonSerializer.Deserialize<FilterPreset>(jsonContent, options);

            if (preset == null || string.IsNullOrWhiteSpace(preset.Name))
            {
                _dialogService?.ShowError("Import Error", "Invalid preset file format. The preset must contain a valid Name.");
                return;
            }

            preset.Manufacturer = "User";
            preset.Category = "User";
            if (string.IsNullOrWhiteSpace(preset.RecommendedFor))
            {
                preset.RecommendedFor = "Custom";
            }

            string customDir = GetCustomPresetsDirectory();
            if (!Directory.Exists(customDir))
            {
                Directory.CreateDirectory(customDir);
            }

            string destFileName = string.Join("_", preset.Name.Split(Path.GetInvalidFileNameChars())) + ".json";
            string destPath = Path.Combine(customDir, destFileName);

            var writeOptions = new JsonSerializerOptions { WriteIndented = true };
            string formattedJson = JsonSerializer.Serialize(preset, writeOptions);
            await File.WriteAllTextAsync(destPath, formattedJson);

            if (_originalImageSource != null)
            {
                await LoadFiltersAsync(_originalImageSource);
            }

            _dialogService?.ShowInformation("Import Success", $"Preset '{preset.Name}' imported successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error importing preset: {ex.Message}");
            _dialogService?.ShowError("Import Error", "Failed to import preset file.", ex);
        }
    }
}
