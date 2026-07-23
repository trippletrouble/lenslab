using System;

namespace LensLab.Services;

/// <summary>
/// Service interface for displaying user-facing notifications, error dialogs, and confirmation prompts.
/// Decouples ViewModels from WPF UI framework controls (e.g., MessageBox).
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Displays an error dialog to the user with optional exception details.
    /// </summary>
    void ShowError(string title, string message, Exception? exception = null);

    /// <summary>
    /// Displays a warning dialog to the user.
    /// </summary>
    void ShowWarning(string title, string message);

    /// <summary>
    /// Displays an informational message to the user.
    /// </summary>
    void ShowInformation(string title, string message);

    /// <summary>
    /// Displays a confirmation dialog (Yes/No) to the user.
    /// </summary>
    bool Confirm(string title, string message);
}
