using LensLab.ViewModels;
using Xunit;

namespace LensLab.Tests;

/// <summary>
/// Contains unit tests for slider adjustments in <see cref="AdjustmentViewModel"/>.
/// </summary>
public class AdjustmentViewModelTests
{
    [Fact]
    public void ResetAll_ShouldResetSettingsToDefault()
    {
        var viewModel = new AdjustmentViewModel();

        viewModel.Settings.Brightness = 50.0;
        viewModel.Settings.Contrast = -25.0;
        viewModel.FilterOpacity = 50.0;

        viewModel.ResetAllCommand.Execute(null);

        Assert.Equal(0.0, viewModel.Settings.Brightness);
        Assert.Equal(0.0, viewModel.Settings.Contrast);
        Assert.Equal(100.0, viewModel.FilterOpacity);
    }

    [Fact]
    public void AdjustmentChanged_ShouldFireWhenSliderChanges()
    {
        var viewModel = new AdjustmentViewModel();
        bool eventFired = false;

        viewModel.AdjustmentChanged += (sender, args) => eventFired = true;

        viewModel.Settings.Brightness = 15.0;

        Assert.True(eventFired);
    }
}
