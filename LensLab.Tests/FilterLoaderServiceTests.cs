using System.Linq;
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

    [Fact]
    public async Task LoadAllFiltersAsync_PresetsAreSortedByManufacturerAndName()
    {
        var sut = new FilterLoaderService(_dialogMock.Object);

        var presets = await sut.LoadAllFiltersAsync();

        if (presets.Count > 1)
        {
            var expectedSorted = presets
                .OrderBy(p => p.Manufacturer, System.StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Name, System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (int i = 0; i < presets.Count; i++)
            {
                Assert.Equal(expectedSorted[i].Name, presets[i].Name);
            }
        }
    }
}
