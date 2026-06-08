using System.Windows;
using MahApps.Metro.Controls;
using LensLab.ViewModels;

namespace LensLab.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MetroWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        var files = e.Data.GetData(DataFormats.FileDrop) as string[];
        if (files?.Length > 0)
        {
            var filePath = files[0];
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.OpenImageCommand.Execute(filePath);
            }
        }
    }
}
