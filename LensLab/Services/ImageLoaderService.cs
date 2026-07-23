using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LensLab.Models;

namespace LensLab.Services;

/// <summary>
/// Default implementation of <see cref="IImageLoaderService"/> for loading and scaling images.
/// </summary>
public class ImageLoaderService : IImageLoaderService
{
    private readonly IDialogService _dialogService;

    public ImageLoaderService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public async Task<ImageModel?> LoadImageAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            _dialogService.ShowError("File Error", $"The file at path '{filePath}' does not exist.");
            return null;
        }

        try
        {
            // Load the image asynchronously off-thread to prevent UI freezing
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

            return new ImageModel
            {
                OriginalImage = loadedBitmap,
                CurrentImage = previewBitmap,
                PreviewImage = previewBitmap,
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Width = loadedBitmap.PixelWidth,
                Height = loadedBitmap.PixelHeight
            };
        }
        catch (Exception ex)
        {
            _dialogService.ShowError("Image Loading Failure", $"Failed to load image file: {Path.GetFileName(filePath)}", ex);
            return null;
        }
    }
}
