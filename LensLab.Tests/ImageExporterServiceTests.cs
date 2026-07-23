using System.Threading.Tasks;
using LensLab.Models;
using LensLab.Services;
using Moq;
using Xunit;

namespace LensLab.Tests;

public class ImageExporterServiceTests
{
    private readonly Mock<IFileDialogService> _fileDialogMock = new();
    private readonly Mock<IDialogService> _dialogMock = new();
    private readonly Mock<IImageProcessingService> _imageProcessingMock = new();

    private ImageExporterService CreateSut()
    {
        return new ImageExporterService(_fileDialogMock.Object, _dialogMock.Object, _imageProcessingMock.Object);
    }

    [Fact]
    public async Task ExportImageAsync_WhenImageModelIsNull_ShowsWarningAndReturnsFalse()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        bool result = await sut.ExportImageAsync(null!, new AdjustmentSettings(), null, 100.0);

        // Assert
        Assert.False(result);
        _dialogMock.Verify(x => x.ShowWarning("Export Warning", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ExportImageAsync_WhenFileDialogCancelled_ReturnsFalse()
    {
        // Arrange
        var imageModel = new ImageModel { FileName = "sample.jpg" };
        _fileDialogMock.Setup(x => x.SaveImageFile(It.IsAny<string>())).Returns((string?)null);

        var sut = CreateSut();

        // Act
        bool result = await sut.ExportImageAsync(imageModel, new AdjustmentSettings(), null, 100.0);

        // Assert
        Assert.False(result);
    }
}
