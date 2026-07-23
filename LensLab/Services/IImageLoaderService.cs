using System.Threading.Tasks;
using LensLab.Models;

namespace LensLab.Services;

/// <summary>
/// Service interface for loading images asynchronously, creating cross-thread frozen BitmapSource instances,
/// and generating 1200px scaled preview images for fluid live editing.
/// </summary>
public interface IImageLoaderService
{
    /// <summary>
    /// Asynchronously loads an image from the specified file path, creates a scaled 1200px preview copy,
    /// and returns an ImageModel instance.
    /// </summary>
    /// <param name="filePath">Path to the image file.</param>
    /// <returns>Populated ImageModel, or null if loading fails.</returns>
    Task<ImageModel?> LoadImageAsync(string filePath);
}
