using System;
using System.Windows;

namespace LensLab.Services;

/// <summary>
/// Default WPF implementation of <see cref="IDialogService"/> using <see cref="MessageBox"/>.
/// </summary>
public class DialogService : IDialogService
{
    public void ShowError(string title, string message, Exception? exception = null)
    {
        string fullMessage = message;
        if (exception != null)
        {
            fullMessage += $"\n\nDetails: {exception.Message}";
        }

        MessageBox.Show(fullMessage, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowWarning(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public void ShowInformation(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public bool Confirm(string title, string message)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }
}
