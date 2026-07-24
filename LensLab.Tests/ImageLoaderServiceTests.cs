using System.Threading.Tasks;
using LensLab.Services;
using Moq;
using Xunit;

namespace LensLab.Tests;

public class ImageLoaderServiceTests
{
    private readonly Mock<IDialogService> _dialogMock = new();

    private ImageLoaderService CreateSut()
    {
        return new ImageLoaderService(_dialogMock.Object);
    }

    [Fact]
    public async Task LoadImageAsync_WhenFilePathIsNullOrEmpty_ShowsErrorAndReturnsNull()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = await sut.LoadImageAsync("");

        // Assert
        Assert.Null(result);
        _dialogMock.Verify(x => x.ShowError("File Error", It.IsAny<string>(), null), Times.Once);
    }

    [Fact]
    public async Task LoadImageAsync_WhenFileDoesNotExist_ShowsErrorAndReturnsNull()
    {
        // Arrange
        var sut = CreateSut();
        string nonExistentPath = "C:\\non_existent_folder_123\\image.png";

        // Act
        var result = await sut.LoadImageAsync(nonExistentPath);

        // Assert
        Assert.Null(result);
        _dialogMock.Verify(x => x.ShowError("File Error", It.IsAny<string>(), null), Times.Once);
    }
}
