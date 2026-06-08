namespace LensLab.Services;

/// <summary>
/// Defines a contract for showing file dialogs, allowing file selection 
/// without directly coupling the ViewModels to standard Windows APIs.
/// </summary>
public interface IFileDialogService
{
    /// <summary>
    /// Displays a file open dialog for selecting an image file.
    /// </summary>
    /// <returns>The full path of the selected image file, or null if the selection was cancelled.</returns>
    string? OpenImageFile();
}
