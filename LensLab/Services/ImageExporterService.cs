using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using LensLab.Models;

namespace LensLab.Services;

/// <summary>
/// Default implementation of <see cref="IImageExporterService"/>.
/// </summary>
public class ImageExporterService : IImageExporterService
{
    private readonly IFileDialogService _fileDialogService;
    private readonly IDialogService _dialogService;
    private readonly IImageProcessingService _imageProcessingService;

    public ImageExporterService(
        IFileDialogService fileDialogService,
        IDialogService dialogService,
        IImageProcessingService imageProcessingService)
    {
        _fileDialogService = fileDialogService;
        _dialogService = dialogService;
        _imageProcessingService = imageProcessingService;
    }

    public async Task<bool> ExportImageAsync(ImageModel imageModel, AdjustmentSettings settings, FilterPreset? filter, double opacity)
    {
        if (imageModel == null || imageModel.OriginalImage == null)
        {
            _dialogService.ShowWarning("Export Warning", "No active image available for export.");
            return false;
        }

        string defaultName = Path.GetFileNameWithoutExtension(imageModel.FileName) + "_edited";
        string? destinationPath = _fileDialogService.SaveImageFile(defaultName);

        if (string.IsNullOrEmpty(destinationPath))
        {
            return false;
        }

        try
        {
            string ext = Path.GetExtension(destinationPath).ToLower();

            // Render off-thread at full resolution
            BitmapSource processedBitmap = await Task.Run(async () =>
            {
                return await _imageProcessingService.ApplyEditAsync(
                    imageModel.OriginalImage,
                    settings,
                    filter,
                    opacity,
                    CancellationToken.None);
            });

            // Save file to disk off-thread
            await Task.Run(() =>
            {
                using var fileStream = new FileStream(destinationPath, FileMode.Create);
                BitmapEncoder encoder = (ext == ".png") ? (BitmapEncoder)new PngBitmapEncoder() : new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(processedBitmap));
                encoder.Save(fileStream);
            });

            _dialogService.ShowInformation("Export Success", "Image exported successfully!");
            return true;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError("Export Failure", $"Failed to export image to '{destinationPath}'.", ex);
            return false;
        }
    }
}
