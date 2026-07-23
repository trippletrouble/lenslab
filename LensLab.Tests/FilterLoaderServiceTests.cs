using System.Threading.Tasks;
using LensLab.Services;
using Moq;
using Xunit;

namespace LensLab.Tests;

/// <summary>
/// Contains unit tests for film preset JSON loading in <see cref="FilterLoaderService"/>.
/// </summary>
public class FilterLoaderServiceTests
{
    private readonly Mock<IDialogService> _dialogMock = new();

    [Fact]
    public async Task LoadAllFiltersAsync_ShouldLoadPresetsFromResources()
    {
        var sut = new FilterLoaderService(_dialogMock.Object);

        var presets = await sut.LoadAllFiltersAsync();

        Assert.NotNull(presets);
    }
}
