using System.Collections.Generic;
using System.Threading.Tasks;
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
}
