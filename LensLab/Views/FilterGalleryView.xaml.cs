using System.Windows;
using System.Windows.Controls;
using LensLab.Models;
using LensLab.ViewModels;

namespace LensLab.Views
{
    /// <summary>
    /// Interaction logic for FilterGalleryView.xaml
    /// </summary>
    public partial class FilterGalleryView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterGalleryView"/> class.
        /// </summary>
        public FilterGalleryView()
        {
            InitializeComponent();
        }

        private async void DeletePreset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is FilterPreviewItem item)
            {
                var window = Window.GetWindow(this) as MahApps.Metro.Controls.MetroWindow;
                if (window != null)
                {
                    var settings = new MahApps.Metro.Controls.Dialogs.MetroDialogSettings
                    {
                        AffirmativeButtonText = "Delete",
                        NegativeButtonText = "Cancel",
                        ColorScheme = MahApps.Metro.Controls.Dialogs.MetroDialogColorScheme.Theme,
                        AnimateShow = true,
                        AnimateHide = true
                    };

                    MahApps.Metro.Controls.Dialogs.MessageDialogResult result = await MahApps.Metro.Controls.Dialogs.DialogManager.ShowMessageAsync(
                        window,
                        "Delete Custom Preset",
                        $"Are you sure you want to permanently delete the custom preset '{item.Filter.Name}'?",
                        MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative,
                        settings);

                    if (result == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
                    {
                        if (DataContext is FilterGalleryViewModel viewModel)
                        {
                            viewModel.DeletePresetCommand.Execute(item);
                        }
                    }
                }
            }
        }
    }
}
