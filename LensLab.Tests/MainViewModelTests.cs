using System.Collections.Generic;
using System.Threading.Tasks;
using LensLab.Data.Entities;
using LensLab.Models;
using LensLab.Services;
using LensLab.ViewModels;
using Moq;
using Xunit;

namespace LensLab.Tests;

public class MainViewModelTests
{
    private readonly Mock<IFileDialogService> _fileDialogMock = new();
    private readonly Mock<IDialogService> _dialogMock = new();
    private readonly Mock<IFilterLoaderService> _filterLoaderMock = new();
    private readonly Mock<IImageProcessingService> _imageProcessingMock = new();
    private readonly Mock<IImageLoaderService> _imageLoaderMock = new();
    private readonly Mock<IImageExporterService> _imageExporterMock = new();
    private readonly Mock<IPresetManagerService> _presetManagerMock = new();
    private readonly Mock<IRecentFilesService> _recentFilesMock = new();

    private readonly FilterGalleryViewModel _filterGalleryViewModel;
    private readonly AdjustmentViewModel _adjustmentViewModel;

    public MainViewModelTests()
    {
        _filterGalleryViewModel = new FilterGalleryViewModel(_filterLoaderMock.Object, _imageProcessingMock.Object, _dialogMock.Object, _fileDialogMock.Object);
        _adjustmentViewModel = new AdjustmentViewModel();
    }

    private MainViewModel CreateSut()
    {
        return new MainViewModel(
            _fileDialogMock.Object,
            _dialogMock.Object,
            _filterLoaderMock.Object,
            _imageProcessingMock.Object,
            _imageLoaderMock.Object,
            _imageExporterMock.Object,
            _presetManagerMock.Object,
            _recentFilesMock.Object,
            _filterGalleryViewModel,
            _adjustmentViewModel);
    }

    [Fact]
    public async Task InitializeAsync_LoadsFilters_SetsStatusText()
    {
        // Arrange
        var samplePresets = new List<FilterPreset>
        {
            new FilterPreset { Name = "TestPreset1", Manufacturer = "Kodak" },
            new FilterPreset { Name = "TestPreset2", Manufacturer = "Fuji" }
        };
        _filterLoaderMock.Setup(x => x.LoadAllFiltersAsync()).ReturnsAsync(samplePresets);

        var sut = CreateSut();

        // Act
        await sut.InitializeAsync();

        // Assert
        Assert.Equal(2, sut.Filters.Count);
        Assert.Equal("2 filters loaded", sut.StatusText);
        Assert.False(sut.IsBusy);
    }

    [Fact]
    public async Task OpenImageCommand_WhenDialogCancelled_DoesNotChangeCurrentImage()
    {
        // Arrange
        _fileDialogMock.Setup(x => x.OpenImageFile()).Returns((string?)null);

        var sut = CreateSut();

        // Act
        await sut.OpenImageCommand.ExecuteAsync(null);

        // Assert
        Assert.Null(sut.CurrentImage);
        _imageLoaderMock.Verify(x => x.LoadImageAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OpenImageCommand_WithValidFile_LoadsImageModel()
    {
        // Arrange
        string testPath = "C:\\test\\sample.jpg";
        _fileDialogMock.Setup(x => x.OpenImageFile()).Returns(testPath);

        var imageModel = new ImageModel
        {
            FilePath = testPath,
            FileName = "sample.jpg",
            Width = 800,
            Height = 600
        };

        _imageLoaderMock.Setup(x => x.LoadImageAsync(testPath)).ReturnsAsync(imageModel);

        var sut = CreateSut();

        // Act
        await sut.OpenImageCommand.ExecuteAsync(null);

        // Assert
        Assert.NotNull(sut.CurrentImage);
        Assert.Equal("sample.jpg", sut.CurrentImage.FileName);
        _recentFilesMock.Verify(x => x.AddRecentFileAsync(testPath), Times.Once);
    }

    [Fact]
    public async Task SavePresetDialogConfirmCommand_WhenNameIsEmpty_ShowsWarningDialog()
    {
        // Arrange
        var sut = CreateSut();
        sut.NewPresetName = "   ";

        // Act
        await sut.SavePresetDialogConfirmCommand.ExecuteAsync(null);

        // Assert
        _dialogMock.Verify(x => x.ShowWarning("Save Preset", "Preset name cannot be empty."), Times.Once);
    }

    [Fact]
    public async Task SavePresetDialogConfirmCommand_WithValidName_CallsPresetManagerService()
    {
        // Arrange
        var sut = CreateSut();
        sut.NewPresetName = "Custom Preset 1";
        _presetManagerMock.Setup(x => x.SaveCustomPresetAsync("Custom Preset 1", It.IsAny<FilterPreset?>(), It.IsAny<double>(), It.IsAny<AdjustmentSettings>())).ReturnsAsync(true);
        _filterLoaderMock.Setup(x => x.LoadAllFiltersAsync()).ReturnsAsync(new List<FilterPreset>());

        // Act
        await sut.SavePresetDialogConfirmCommand.ExecuteAsync(null);

        // Assert
        _presetManagerMock.Verify(x => x.SaveCustomPresetAsync("Custom Preset 1", It.IsAny<FilterPreset?>(), It.IsAny<double>(), It.IsAny<AdjustmentSettings>()), Times.Once);
        Assert.False(sut.IsSavePresetDialogVisible);
    }

    [Fact]
    public async Task ExportImageCommand_WhenNoImageLoaded_ShowsWarningDialog()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await sut.ExportImageCommand.ExecuteAsync(null);

        // Assert
        _dialogMock.Verify(x => x.ShowWarning("Export Warning", "No active image loaded to export."), Times.Once);
        _imageExporterMock.Verify(x => x.ExportImageAsync(It.IsAny<ImageModel>(), It.IsAny<AdjustmentSettings>(), It.IsAny<FilterPreset?>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public async Task ExportImageCommand_WhenImageLoaded_DelegatesToExporterService()
    {
        // Arrange
        var sut = CreateSut();
        sut.CurrentImage = new ImageModel { FileName = "test.jpg" };
        _imageExporterMock.Setup(x => x.ExportImageAsync(It.IsAny<ImageModel>(), It.IsAny<AdjustmentSettings>(), It.IsAny<FilterPreset?>(), It.IsAny<double>())).ReturnsAsync(true);

        // Act
        await sut.ExportImageCommand.ExecuteAsync(null);

        // Assert
        _imageExporterMock.Verify(x => x.ExportImageAsync(sut.CurrentImage, It.IsAny<AdjustmentSettings>(), It.IsAny<FilterPreset?>(), It.IsAny<double>()), Times.Once);
    }

    [Fact]
    public async Task OpenRecentFileCommand_WhenFileDoesNotExist_ShowsErrorDialog()
    {
        // Arrange
        var sut = CreateSut();
        var recentFile = new RecentFile { FilePath = "C:\\non_existent\\missing.jpg", FileName = "missing.jpg" };

        // Act
        await sut.OpenRecentFileCommand.ExecuteAsync(recentFile);

        // Assert
        _dialogMock.Verify(x => x.ShowError("File Not Found", It.IsAny<string>(), null), Times.Once);
    }
}
