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

    /// <summary>
    /// Invoked when a key is pressed. Handles Up/Down arrow navigation for film presets.
    /// </summary>
    protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Key == System.Windows.Input.Key.Up)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.FilterGallery.SelectPreviousFilter();
                e.Handled = true;
            }
        }
        else if (e.Key == System.Windows.Input.Key.Down)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.FilterGallery.SelectNextFilter();
                e.Handled = true;
            }
        }
    }
}
