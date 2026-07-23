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

    /// <summary>
    /// Displays a file save dialog for exporting an image file.
    /// </summary>
    /// <param name="defaultFileName">Default file name suggested in the dialog.</param>
    /// <returns>The full destination path, or null if cancelled.</returns>
    string? SaveImageFile(string defaultFileName);
}
