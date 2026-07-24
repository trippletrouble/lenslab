using System;
using System.Windows.Media.Imaging;
using LensLab.Services;
using Xunit;

namespace LensLab.Tests;

/// <summary>
/// Contains unit tests for core pixel processing methods in <see cref="ImageProcessingService"/>.
/// </summary>
public class ImageProcessingServiceTests
{
    private readonly ImageProcessingService _sut = new();

    [Fact]
    public void ClampToByte_ShouldClampUpperAndLowerBounds()
    {
        Assert.Equal((byte)0, _sut.ClampToByte(-50.0));
        Assert.Equal((byte)255, _sut.ClampToByte(300.0));
        Assert.Equal((byte)128, _sut.ClampToByte(128.0));
    }

    [Fact]
    public void RgbToHsl_And_HslToRgb_ShouldRoundtripCorrectly()
    {
        byte r = 200;
        byte g = 100;
        byte b = 50;

        var (h, s, l) = _sut.RgbToHsl(r, g, b);
        var (convertedR, convertedG, convertedB) = _sut.HslToRgb(h, s, l);

        Assert.InRange(convertedR, r - 1, r + 1);
        Assert.InRange(convertedG, g - 1, g + 1);
        Assert.InRange(convertedB, b - 1, b + 1);
    }

    [Fact]
    public void ApplyBrightness_ShouldModifyPixelValues()
    {
        byte[] pixels = { 100, 100, 100, 255 };

        _sut.ApplyBrightness(pixels, 20.0, stride: 4);

        Assert.Equal((byte)120, pixels[0]); // Blue
        Assert.Equal((byte)120, pixels[1]); // Green
        Assert.Equal((byte)120, pixels[2]); // Red
        Assert.Equal((byte)255, pixels[3]); // Alpha
    }

    [Fact]
    public void GetPixelData_ThrowsArgumentNullException_WhenSourceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.GetPixelData(null!));
    }

    [Fact]
    public void CreateBitmapFromPixels_CreatesValidBitmapSource()
    {
        byte[] pixels = new byte[16];
        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = 255;     // Blue
            pixels[i + 1] = 0;   // Green
            pixels[i + 2] = 0;   // Red
            pixels[i + 3] = 255; // Alpha
        }

        BitmapSource bitmap = _sut.CreateBitmapFromPixels(pixels, 2, 2, System.Windows.Media.PixelFormats.Bgra32, 8);

        Assert.NotNull(bitmap);
        Assert.Equal(2, bitmap.PixelWidth);
        Assert.Equal(2, bitmap.PixelHeight);
        Assert.True(bitmap.IsFrozen);
    }
}
