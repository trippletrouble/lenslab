using System;
using System.Windows;
using LensLab.Services;
using LensLab.ViewModels;
using LensLab.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LensLab
{
    /// <summary>
    /// Interaction logic for App.xaml. Bootstraps the application using Microsoft.Extensions.DependencyInjection.
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; } = null!;

        /// <summary>
        /// Handles application startup logic. Configures the DI container, resolves MainWindow and MainViewModel,
        /// and initializes application filter presets asynchronously.
        /// </summary>
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = mainViewModel;

            mainWindow.Show();

            // Load filters asynchronously after the window is shown to keep UI responsive
            await mainViewModel.InitializeAsync();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Register Services with Interfaces
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IFilterLoaderService, FilterLoaderService>();
            services.AddSingleton<IImageProcessingService, ImageProcessingService>();
            services.AddSingleton<IImageLoaderService, ImageLoaderService>();
            services.AddSingleton<IImageExporterService, ImageExporterService>();
            services.AddSingleton<IPresetManagerService, PresetManagerService>();
            services.AddSingleton<IRecentFilesService, RecentFilesService>();

            // Register ViewModels
            services.AddTransient<FilterGalleryViewModel>();
            services.AddTransient<AdjustmentViewModel>();
            services.AddTransient<MainViewModel>();

            // Register Views
            services.AddTransient<MainWindow>();
        }
    }
}
