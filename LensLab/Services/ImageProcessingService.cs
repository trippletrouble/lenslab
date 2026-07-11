using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LensLab.Services;

/// <summary>
/// Provides high-performance pixel-level image processing functions, 
/// including color space conversions and core adjustments.
/// </summary>
public class ImageProcessingService
{
    /// <summary>
    /// Extracts the raw pixel byte array from a BitmapSource.
    /// The source image is converted to Bgra32 if it is not already in that format.
    /// </summary>
    /// <param name="source">The source BitmapSource.</param>
    /// <returns>A byte array representing the pixel data in Bgra32 format.</returns>
    /// <exception cref="ArgumentNullException">Thrown if source is null.</exception>
    public byte[] GetPixelData(BitmapSource source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        BitmapSource formattedSource = source;
        if (source.Format != PixelFormats.Bgra32)
        {
            formattedSource = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        }

        int width = formattedSource.PixelWidth;
        int height = formattedSource.PixelHeight;
        int stride = width * 4; // 4 bytes per pixel for Bgra32
        byte[] pixels = new byte[height * stride];

        formattedSource.CopyPixels(pixels, stride, 0);
        return pixels;
    }

    /// <summary>
    /// Creates a BitmapSource from a raw pixel byte array.
    /// The resulting bitmap is frozen to ensure it can be safely used across different threads.
    /// </summary>
    /// <param name="pixels">The pixel data byte array.</param>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="format">The pixel format.</param>
    /// <param name="stride">The stride of the image.</param>
    /// <returns>A frozen BitmapSource representing the image.</returns>
    /// <exception cref="ArgumentNullException">Thrown if pixels is null.</exception>
    public BitmapSource CreateBitmapFromPixels(byte[] pixels, int width, int height, PixelFormat format, int stride)
    {
        if (pixels == null)
        {
            throw new ArgumentNullException(nameof(pixels));
        }

        BitmapSource bitmap = BitmapSource.Create(
            width,
            height,
            96, // Horizontal DPI
            96, // Vertical DPI
            format,
            null, // Palette
            pixels,
            stride);

        bitmap.Freeze(); // Crucial for WPF to pass the bitmap across background threads to the UI thread
        return bitmap;
    }

    /// <summary>
    /// Clamps a double value to the bounds of a byte (0 to 255).
    /// </summary>
    /// <param name="value">The double value to clamp.</param>
    /// <returns>A clamped byte value.</returns>
    public byte ClampToByte(double value)
    {
        if (value < 0.0) return 0;
        if (value > 255.0) return 255;
        return (byte)value;
    }

    /// <summary>
    /// Converts RGB color coordinates to HSL.
    /// </summary>
    /// <param name="r">Red component (0-255).</param>
    /// <param name="g">Green component (0-255).</param>
    /// <param name="b">Blue component (0-255).</param>
    /// <returns>A tuple containing Hue (0-360), Saturation (0-1), and Lightness (0-1).</returns>
    public (double H, double S, double L) RgbToHsl(byte r, byte g, byte b)
    {
        double rd = r / 255.0;
        double gd = g / 255.0;
        double bd = b / 255.0;

        double max = Math.Max(rd, Math.Max(gd, bd));
        double min = Math.Min(rd, Math.Min(gd, bd));

        double h = 0.0;
        double s = 0.0;
        double l = (max + min) / 2.0;

        if (max != min)
        {
            double d = max - min;
            s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

            if (max == rd)
            {
                h = (gd - bd) / d + (gd < bd ? 6.0 : 0.0);
            }
            else if (max == gd)
            {
                h = (bd - rd) / d + 2.0;
            }
            else if (max == bd)
            {
                h = (rd - gd) / d + 4.0;
            }

            h *= 60.0;
        }

        return (h, s, l);
    }

    /// <summary>
    /// Converts HSL color coordinates back to RGB.
    /// </summary>
    /// <param name="h">Hue component (0-360).</param>
    /// <param name="s">Saturation component (0-1).</param>
    /// <param name="l">Lightness component (0-1).</param>
    /// <returns>A tuple containing the converted Red, Green, and Blue bytes.</returns>
    public (byte R, byte G, byte B) HslToRgb(double h, double s, double l)
    {
        double r, g, b;

        if (s == 0.0)
        {
            r = g = b = l; // achromatic
        }
        else
        {
            double q = l < 0.5 ? l * (1.0 + s) : l + s - l * s;
            double p = 2.0 * l - q;

            r = HueToRgb(p, q, h / 360.0 + 1.0 / 3.0);
            g = HueToRgb(p, q, h / 360.0);
            b = HueToRgb(p, q, h / 360.0 - 1.0 / 3.0);
        }

        return (ClampToByte(r * 255.0), ClampToByte(g * 255.0), ClampToByte(b * 255.0));
    }

    /// <summary>
    /// Helper function for HSL to RGB conversion.
    /// </summary>
    private double HueToRgb(double p, double q, double t)
    {
        if (t < 0.0) t += 1.0;
        if (t > 1.0) t -= 1.0;
        if (t < 1.0 / 6.0) return p + (q - p) * 6.0 * t;
        if (t < 1.0 / 2.0) return q;
        if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6.0;
        return p;
    }

    /// <summary>
    /// Adjusts the brightness of the pixel array by adding a flat offset in-place.
    /// </summary>
    /// <param name="pixels">The pixel data array to modify in-place (Bgra32 format).</param>
    /// <param name="amount">The adjustment offset (-100 to +100).</param>
    /// <param name="stride">The stride of the pixel data.</param>
    /// <exception cref="ArgumentNullException">Thrown if pixels is null.</exception>
    public void ApplyBrightness(byte[] pixels, double amount, int stride)
    {
        if (pixels == null)
        {
            throw new ArgumentNullException(nameof(pixels));
        }

        double offset = amount;

        for (int i = 0; i < pixels.Length; i += 4)
        {
            // BGRA structure
            pixels[i] = ClampToByte(pixels[i] + offset);       // Blue
            pixels[i + 1] = ClampToByte(pixels[i + 1] + offset); // Green
            pixels[i + 2] = ClampToByte(pixels[i + 2] + offset); // Red
        }
    }

    /// <summary>
    /// Adjusts the contrast of the pixel array in-place using a non-linear factor formula.
    /// </summary>
    /// <param name="pixels">The pixel data array to modify in-place (Bgra32 format).</param>
    /// <param name="amount">The adjustment offset (-100 to +100).</param>
    /// <param name="stride">The stride of the pixel data.</param>
    /// <exception cref="ArgumentNullException">Thrown if pixels is null.</exception>
    public void ApplyContrast(byte[] pixels, double amount, int stride)
    {
        if (pixels == null)
        {
            throw new ArgumentNullException(nameof(pixels));
        }

        // Contrast formula: factor = (259 * (contrast + 255)) / (255 * (259 - contrast))
        double factor = (259.0 * (amount + 255.0)) / (255.0 * (259.0 - amount));

        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = ClampToByte(factor * (pixels[i] - 128.0) + 128.0);       // Blue
            pixels[i + 1] = ClampToByte(factor * (pixels[i + 1] - 128.0) + 128.0); // Green
            pixels[i + 2] = ClampToByte(factor * (pixels[i + 2] - 128.0) + 128.0); // Red
        }
    }

    /// <summary>
    /// Adjusts the color saturation of the pixel array in-place using RGB-HSL-RGB conversion.
    /// </summary>
    /// <param name="pixels">The pixel data array to modify in-place (Bgra32 format).</param>
    /// <param name="amount">The saturation change (-100 to +100).</param>
    /// <param name="stride">The stride of the pixel data.</param>
    /// <exception cref="ArgumentNullException">Thrown if pixels is null.</exception>
    public void ApplySaturation(byte[] pixels, double amount, int stride)
    {
        if (pixels == null)
        {
            throw new ArgumentNullException(nameof(pixels));
        }

        double factor = 1.0 + (amount / 100.0);

        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte b = pixels[i];
            byte g = pixels[i + 1];
            byte r = pixels[i + 2];

            // Grayscale check - if all channels are equal, saturation adjustment has no effect.
            if (r != g || g != b)
            {
                var hsl = RgbToHsl(r, g, b);
                
                double newS = hsl.S * factor;
                if (newS < 0.0) newS = 0.0;
                if (newS > 1.0) newS = 1.0;

                var rgb = HslToRgb(hsl.H, newS, hsl.L);
                pixels[i] = rgb.B;
                pixels[i + 1] = rgb.G;
                pixels[i + 2] = rgb.R;
            }
        }
    }
}
