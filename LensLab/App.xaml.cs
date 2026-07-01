using System.Windows;

namespace LensLab
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Handles application startup logic. Instantiates the MainViewModel and MainWindow,
        /// sets the DataContext explicitly, and triggers the async initialization of the presets.
        /// </summary>
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainViewModel = new ViewModels.MainViewModel();
            var mainWindow = new Views.MainWindow
            {
                DataContext = mainViewModel
            };

            mainWindow.Show();

            // Load filters asynchronously after the window is shown to keep UI responsive
            await mainViewModel.InitializeAsync();
        }
    }
}
