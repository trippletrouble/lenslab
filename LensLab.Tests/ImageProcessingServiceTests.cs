using System.Windows.Media.Imaging;
using LensLab.Services;
using Xunit;

namespace LensLab.Tests;

/// <summary>
/// Contains unit tests for core pixel processing methods in <see cref="ImageProcessingService"/>.
/// </summary>
public class ImageProcessingServiceTests
{
    private readonly ImageProcessingService _service = new();

    [Fact]
    public void ClampToByte_ShouldClampUpperAndLowerBounds()
    {
        // Assert lower bound clamp
        Assert.Equal((byte)0, _service.ClampToByte(-50.0));

        // Assert upper bound clamp
        Assert.Equal((byte)255, _service.ClampToByte(300.0));

        // Assert valid range preserved
        Assert.Equal((byte)128, _service.ClampToByte(128.0));
    }

    [Fact]
    public void RgbToHsl_And_HslToRgb_ShouldRoundtripCorrectly()
    {
        byte r = 200;
        byte g = 100;
        byte b = 50;

        // Convert RGB to HSL
        var (h, s, l) = _service.RgbToHsl(r, g, b);

        // Convert HSL back to RGB
        var (convertedR, convertedG, convertedB) = _service.HslToRgb(h, s, l);

        // Assert color components match original values within 1-byte rounding tolerance
        Assert.InRange(convertedR, r - 1, r + 1);
        Assert.InRange(convertedG, g - 1, g + 1);
        Assert.InRange(convertedB, b - 1, b + 1);
    }

    [Fact]
    public void ApplyBrightness_ShouldModifyPixelValues()
    {
        // 1 pixel (Bgra32 = 4 bytes: B, G, R, A)
        byte[] pixels = { 100, 100, 100, 255 };

        _service.ApplyBrightness(pixels, 20.0, stride: 4);

        // Assert RGB channels increased by offset (100 + 20 = 120)
        Assert.Equal((byte)120, pixels[0]); // Blue
        Assert.Equal((byte)120, pixels[1]); // Green
        Assert.Equal((byte)120, pixels[2]); // Red
        Assert.Equal((byte)255, pixels[3]); // Alpha unchanged
    }
}
