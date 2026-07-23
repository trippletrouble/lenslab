using System.Threading.Tasks;
using LensLab.Models;

namespace LensLab.Services;

/// <summary>
/// Service interface for rendering and exporting edited images at full resolution.
/// </summary>
public interface IImageExporterService
{
    /// <summary>
    /// Prompts the user for a destination file using <see cref="IFileDialogService"/>,
    /// renders the active image edits at full resolution off-thread, and saves to JPG or PNG.
    /// </summary>
    /// <returns>True if export succeeded; false otherwise.</returns>
    Task<bool> ExportImageAsync(ImageModel imageModel, AdjustmentSettings settings, FilterPreset? filter, double opacity);
}
