using System.Windows.Media.Imaging;

namespace LensLab.Models;

/// <summary>
/// Represents the loaded image and its associated metadata (dimensions, filepath, etc.).
/// Holds the original un-edited image reference and the current processed image reference.
/// </summary>
public class ImageModel
{
    /// <summary>
    /// Gets or sets the original unmodified image source.
    /// </summary>
    public BitmapSource? OriginalImage { get; set; }

    /// <summary>
    /// Gets or sets the current processed image source (with edits/filters applied).
    /// </summary>
    public BitmapSource? CurrentImage { get; set; }

    /// <summary>
    /// Gets or sets the absolute file path of the image.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the name of the file (including extension).
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Gets or sets the image width in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the image height in pixels.
    /// </summary>
    public int Height { get; set; }
}
