using System.Threading.Tasks;
using LensLab.Services;
using Moq;
using Xunit;

namespace LensLab.Tests;

public class RecentFilesServiceTests
{
    private readonly Mock<IDialogService> _dialogMock = new();

    [Fact]
    public async Task GetRecentFilesAsync_ReturnsNonNullList()
    {
        // Arrange
        var sut = new RecentFilesService(_dialogMock.Object);

        // Act
        var result = await sut.GetRecentFilesAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task AddRecentFileAsync_WhenFilePathIsNullOrEmpty_DoesNotThrow()
    {
        // Arrange
        var sut = new RecentFilesService(_dialogMock.Object);

        // Act & Assert
        await sut.AddRecentFileAsync("");
        await sut.AddRecentFileAsync(null!);
    }
}
