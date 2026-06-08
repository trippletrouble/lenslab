using System.Windows.Controls;

namespace LensLab.Views;

/// <summary>
/// Interaction logic for ImageEditorView.xaml.
/// Displays the current image preview or a placeholder when no image is loaded.
/// </summary>
public partial class ImageEditorView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageEditorView"/> class.
    /// </summary>
    public ImageEditorView()
    {
        InitializeComponent();
    }
}
