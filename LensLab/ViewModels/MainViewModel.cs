using System;
using System.IO;
using System.Threading.Tasks;
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

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private bool _isBusy = false;

    [ObservableProperty]
    private ImageModel? _currentImage;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    public MainViewModel()
    {
        _fileDialogService = new FileDialogService();
    }

    /// <summary>
    /// Asynchronously opens an image file. If no filePath is provided, shows the file dialog.
    /// Loads the image into memory with OnLoad cache option, freezes it for safety, and sets CurrentImage.
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

            CurrentImage = new ImageModel
            {
                OriginalImage = loadedBitmap,
                CurrentImage = loadedBitmap,
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Width = loadedBitmap.PixelWidth,
                Height = loadedBitmap.PixelHeight
            };

            StatusText = CurrentImage.FileName ?? "Image loaded";
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
