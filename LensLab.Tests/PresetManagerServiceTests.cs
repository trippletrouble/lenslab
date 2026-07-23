using System.IO;
using System.Threading.Tasks;
using LensLab.Models;
using LensLab.Services;
using Moq;
using Xunit;

namespace LensLab.Tests;

public class PresetManagerServiceTests
{
    private readonly Mock<IDialogService> _dialogMock = new();

    [Fact]
    public async Task SaveCustomPresetAsync_WhenNameIsEmpty_ShowsWarningAndReturnsFalse()
    {
        // Arrange
        var sut = new PresetManagerService(_dialogMock.Object);
        var settings = new AdjustmentSettings();

        // Act
        bool result = await sut.SaveCustomPresetAsync("", null, 100.0, settings);

        // Assert
        Assert.False(result);
        _dialogMock.Verify(x => x.ShowWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SaveCustomPresetAsync_WithValidName_SavesJsonFileAndShowsInformation()
    {
        // Arrange
        var sut = new PresetManagerService(_dialogMock.Object);
        var settings = new AdjustmentSettings { Brightness = 15.0, Contrast = 10.0 };
        string presetName = "UnitTestPreset_" + System.Guid.NewGuid().ToString("N");

        // Act
        bool result = await sut.SaveCustomPresetAsync(presetName, null, 100.0, settings);

        // Assert
        Assert.True(result);
        _dialogMock.Verify(x => x.ShowInformation("Preset Saved", It.IsAny<string>()), Times.Once);

        // Clean up created preset file
        string appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
        string targetFile = Path.Combine(appData, "LensLab", "CustomPresets", presetName + ".json");

        if (File.Exists(targetFile))
        {
            File.Delete(targetFile);
        }
    }
}
