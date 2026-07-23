using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LensLab.Data.Entities;
using LensLab.Models;
using LensLab.Services;

namespace LensLab.ViewModels;

/// <summary>
/// The main ViewModel for the application. Controls general UI state and orchestrates actions
/// across child ViewModels and interface-backed services.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IFileDialogService _fileDialogService;
    private readonly IDialogService _dialogService;
    private readonly IFilterLoaderService _filterLoaderService;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IImageLoaderService _imageLoaderService;
    private readonly IImageExporterService _imageExporterService;
    private readonly IPresetManagerService _presetManagerService;
    private readonly IRecentFilesService _recentFilesService;

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
    /// Initializes a new instance of the <see cref="MainViewModel"/> class with full constructor injection.
    /// </summary>
    public MainViewModel(
        IFileDialogService fileDialogService,
        IDialogService dialogService,
        IFilterLoaderService filterLoaderService,
        IImageProcessingService imageProcessingService,
        IImageLoaderService imageLoaderService,
        IImageExporterService imageExporterService,
        IPresetManagerService presetManagerService,
        IRecentFilesService recentFilesService,
        FilterGalleryViewModel filterGallery,
        AdjustmentViewModel adjustments)
    {
        _fileDialogService = fileDialogService;
        _dialogService = dialogService;
        _filterLoaderService = filterLoaderService;
        _imageProcessingService = imageProcessingService;
        _imageLoaderService = imageLoaderService;
        _imageExporterService = imageExporterService;
        _presetManagerService = presetManagerService;
        _recentFilesService = recentFilesService;

        FilterGallery = filterGallery;
        FilterGallery.FilterSelected += OnFilterSelected;

        Adjustments = adjustments;
        Adjustments.AdjustmentChanged += OnAdjustmentChanged;

        // Initialize and load recent files from SQLite database
        _ = LoadRecentFilesAsync();
    }

    /// <summary>
    /// Asynchronously initializes the ViewModel, loading all available filter presets.
    /// </summary>
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
            _dialogService.ShowError("Initialization Error", "Failed to load filter presets.", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Handler that executes whenever a slider value or opacity changes.
    /// Cancels active rendering, debounces inputs, and starts a new preview render.
    /// </summary>
    private async void OnAdjustmentChanged(object? sender, EventArgs e)
    {
        if (CurrentImage == null || CurrentImage.PreviewImage == null) return;

        try
        {
            _renderCts?.Cancel();
            _renderCts = new CancellationTokenSource();
            var token = _renderCts.Token;

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
            _dialogService.ShowError("Adjustment Error", "Failed to apply slider adjustments.", ex);
        }
    }

    /// <summary>
    /// Handler that executes whenever a filter selection changes in the gallery.
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
            _dialogService.ShowError("Filter Error", "Failed to apply filter preset.", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Asynchronously opens an image file using <see cref="IImageLoaderService"/>.
    /// </summary>
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

            var imageModel = await _imageLoaderService.LoadImageAsync(filePath);
            if (imageModel == null)
            {
                StatusText = "Error loading image";
                return;
            }

            CurrentImage = imageModel;
            StatusText = CurrentImage.FileName ?? "Image loaded";

            await _recentFilesService.AddRecentFileAsync(filePath);
            await LoadRecentFilesAsync();

            if (CurrentImage.OriginalImage != null)
            {
                await FilterGallery.LoadFiltersAsync(CurrentImage.OriginalImage);
            }
        }
        catch (Exception ex)
        {
            StatusText = "Error loading image";
            _dialogService.ShowError("Image Loading Error", "Failed to open selected image.", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ShowSavePresetDialog()
    {
        NewPresetName = string.Empty;
        IsSavePresetDialogVisible = true;
    }

    [RelayCommand]
    private async Task SavePresetDialogConfirmAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPresetName))
        {
            StatusText = "Preset name cannot be empty";
            _dialogService.ShowWarning("Save Preset", "Preset name cannot be empty.");
            return;
        }

        string presetName = NewPresetName.Trim();
        IsSavePresetDialogVisible = false;

        IsBusy = true;
        StatusText = $"Saving preset '{presetName}'...";

        var activeFilter = FilterGallery.SelectedFilter?.Filter;
        bool success = await _presetManagerService.SaveCustomPresetAsync(
            presetName,
            activeFilter,
            Adjustments.FilterOpacity,
            Adjustments.Settings);

        if (success)
        {
            Filters = await _filterLoaderService.LoadAllFiltersAsync();
            if (CurrentImage != null && CurrentImage.OriginalImage != null)
            {
                await FilterGallery.LoadFiltersAsync(CurrentImage.OriginalImage);
            }
            StatusText = $"Preset '{presetName}' saved successfully";
        }
        else
        {
            StatusText = "Error saving preset";
        }

        IsBusy = false;
    }

    [RelayCommand]
    private void SavePresetDialogCancel()
    {
        IsSavePresetDialogVisible = false;
    }

    /// <summary>
    /// Exports the currently active image at full resolution using <see cref="IImageExporterService"/>.
    /// </summary>
    [RelayCommand]
    private async Task ExportImageAsync()
    {
        if (CurrentImage == null)
        {
            _dialogService.ShowWarning("Export Warning", "No active image loaded to export.");
            return;
        }

        try
        {
            IsBusy = true;
            StatusText = "Exporting image in full resolution...";

            var filter = FilterGallery.SelectedFilter?.Filter;
            bool success = await _imageExporterService.ExportImageAsync(
                CurrentImage,
                Adjustments.Settings,
                filter,
                Adjustments.FilterOpacity);

            StatusText = success ? "Image exported successfully!" : "Export cancelled";
        }
        catch (Exception ex)
        {
            StatusText = "Error exporting image";
            _dialogService.ShowError("Export Error", "An error occurred while exporting the image.", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Loads recent files from SQLite database using <see cref="IRecentFilesService"/>.
    /// </summary>
    public async Task LoadRecentFilesAsync()
    {
        try
        {
            var files = await _recentFilesService.GetRecentFilesAsync();
            var dispatcher = System.Windows.Application.Current?.Dispatcher;

            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(() => PopulateRecentFiles(files));
            }
            else
            {
                PopulateRecentFiles(files);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError("Recent Files Error", "Failed to load recent files history.", ex);
        }
    }

    private void PopulateRecentFiles(IEnumerable<RecentFile> items)
    {
        RecentFiles.Clear();
        foreach (var item in items)
        {
            RecentFiles.Add(item);
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
            _dialogService.ShowError("File Not Found", "The requested file no longer exists at this path.");
            return;
        }

        await OpenImageAsync(file.FilePath);
    }
}
