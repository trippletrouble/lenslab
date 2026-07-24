using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using LensLab.Converters;
using Xunit;

namespace LensLab.Tests;

public class BoolToVisibilityConverterTests
{
    private readonly BoolToVisibilityConverter _converter = new();
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [Fact]
    public void Convert_WithTrue_ReturnsVisible()
    {
        var result = _converter.Convert(true, typeof(Visibility), null, _culture);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithFalse_ReturnsCollapsed()
    {
        var result = _converter.Convert(false, typeof(Visibility), null, _culture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_WithNull_ReturnsCollapsed()
    {
        var result = _converter.Convert(null, typeof(Visibility), null, _culture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_WithNonNullObject_ReturnsVisible()
    {
        var result = _converter.Convert(new object(), typeof(Visibility), null, _culture);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithInverseParameter_InvertsVisibility()
    {
        var trueInverted = _converter.Convert(true, typeof(Visibility), "Inverse", _culture);
        var falseInverted = _converter.Convert(false, typeof(Visibility), "Inverse", _culture);

        Assert.Equal(Visibility.Collapsed, trueInverted);
        Assert.Equal(Visibility.Visible, falseInverted);
    }

    [Fact]
    public void ConvertBack_ReturnsBindingDoNothing()
    {
        var result = _converter.ConvertBack(Visibility.Visible, typeof(bool), null, _culture);
        Assert.Equal(Binding.DoNothing, result);
    }
}
