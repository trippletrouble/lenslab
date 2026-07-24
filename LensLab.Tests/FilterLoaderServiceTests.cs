using System.Threading.Tasks;
using LensLab.Services;
using Xunit;

namespace LensLab.Tests;

/// <summary>
/// Contains unit tests for film preset JSON loading in <see cref="FilterLoaderService"/>.
/// </summary>
public class FilterLoaderServiceTests
{
    private readonly FilterLoaderService _service = new();

    [Fact]
    public async Task LoadAllFiltersAsync_ShouldLoadPresetsFromResources()
    {
        var presets = await _service.LoadAllFiltersAsync();

        // Assert preset collection is non-null and loads preset entries
        Assert.NotNull(presets);
        Assert.NotEmpty(presets);
    }
}
