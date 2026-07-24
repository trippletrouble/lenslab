using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LensLab.Models;

namespace LensLab.Services;

/// <summary>
/// Service interface for digital image manipulation and film filter processing.
/// </summary>
public interface IImageProcessingService
{
    /// <summary>
    /// Asynchronously applies all active filter presets and manual slider adjustments to a source image.
    /// </summary>
    Task<BitmapSource> ApplyEditAsync(
        BitmapSource source,
        AdjustmentSettings settings,
        FilterPreset? filter,
        double filterOpacity,
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates a frozen, thread-safe BitmapSource from raw BGRA pixel bytes.
    /// </summary>
    BitmapSource CreateBitmapFromPixels(byte[] pixels, int width, int height, PixelFormat format, int stride);

    /// <summary>
    /// Extracts raw BGRA pixel bytes from a BitmapSource.
    /// </summary>
    byte[] GetPixelData(BitmapSource source);
}
