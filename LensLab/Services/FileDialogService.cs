using Microsoft.Win32;

namespace LensLab.Services;

/// <summary>
/// Service implementation of <see cref="IFileDialogService"/> using standard Windows file dialogs.
/// </summary>
public class FileDialogService : IFileDialogService
{
    /// <summary>
    /// Displays a standard open file dialog configured for common image file types.
    /// </summary>
    /// <returns>The path of the selected file, or null if the user cancelled the dialog.</returns>
    public string? OpenImageFile()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.tif",
            Title = "Select an Image File"
        };

        bool? result = openFileDialog.ShowDialog();

        return result == true ? openFileDialog.FileName : null;
    }

    /// <summary>
    /// Displays a standard save file dialog configured for exporting JPEG/PNG images.
    /// </summary>
    /// <param name="defaultFileName">Default suggested file name.</param>
    /// <returns>Destination path, or null if cancelled.</returns>
    public string? SaveImageFile(string defaultFileName)
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "JPEG Image (*.jpg;*.jpeg)|*.jpg;*.jpeg|PNG Image (*.png)|*.png",
            Title = "Export Edited Image",
            FileName = defaultFileName
        };

        bool? result = saveFileDialog.ShowDialog();

        return result == true ? saveFileDialog.FileName : null;
    }
}
