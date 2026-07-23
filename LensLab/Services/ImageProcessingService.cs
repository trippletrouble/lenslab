using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LensLab.Models;

namespace LensLab.Services;

/// <summary>
/// Provides high-performance pixel-level image processing functions, 
/// including color space conversions and core adjustments.
/// </summary>
public class ImageProcessingService : IImageProcessingService
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

    /// <summary>
    /// Adjusts the RGB channels of the pixel array by multiplying with specific factors in-place.
    /// </summary>
    /// <param name="pixels">The pixel data array to modify in-place (Bgra32 format).</param>
    /// <param name="rMul">Red channel multiplier.</param>
    /// <param name="gMul">Green channel multiplier.</param>
    /// <param name="bMul">Blue channel multiplier.</param>
    /// <param name="stride">The stride of the pixel data.</param>
    public void ApplyRgbMultipliers(byte[] pixels, double rMul, double gMul, double bMul, int stride)
    {
        if (pixels == null)
        {
            throw new ArgumentNullException(nameof(pixels));
        }

        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = ClampToByte(pixels[i] * bMul);       // Blue
            pixels[i + 1] = ClampToByte(pixels[i + 1] * gMul); // Green
            pixels[i + 2] = ClampToByte(pixels[i + 2] * rMul); // Red
        }
    }

    /// <summary>
    /// Adjusts the temperature (warmth/coolness) of the pixel array in-place.
    /// Positive values shift towards warm yellow/orange, negative values shift towards cool blue.
    /// </summary>
    /// <param name="pixels">The pixel data array to modify in-place (Bgra32 format).</param>
    /// <param name="amount">The adjustment offset (-100 to +100).</param>
    /// <param name="stride">The stride of the pixel data.</param>
    public void ApplyTemperature(byte[] pixels, double amount, int stride)
    {
        if (pixels == null)
        {
            throw new ArgumentNullException(nameof(pixels));
        }

        double offset = amount * 0.5;

        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = ClampToByte(pixels[i] - offset);       // Blue (decreased for warm, increased for cool)
            pixels[i + 2] = ClampToByte(pixels[i + 2] + offset); // Red (increased for warm, decreased for cool)
        }
    }

    /// <summary>
    /// Adjusts the tint (green/magenta) of the pixel array in-place.
    /// Positive values shift towards green, negative values shift towards magenta.
    /// </summary>
    /// <param name="pixels">The pixel data array to modify in-place (Bgra32 format).</param>
    /// <param name="amount">The adjustment offset (-100 to +100).</param>
    /// <param name="stride">The stride of the pixel data.</param>
    public void ApplyTint(byte[] pixels, double amount, int stride)
    {
        if (pixels == null)
        {
            throw new ArgumentNullException(nameof(pixels));
        }

        double greenOffset = amount * 0.5;
        double magentaOffset = amount * 0.25;

        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = ClampToByte(pixels[i] - magentaOffset);       // Blue
            pixels[i + 1] = ClampToByte(pixels[i + 1] + greenOffset);  // Green
            pixels[i + 2] = ClampToByte(pixels[i + 2] - magentaOffset); // Red
        }
    }

    /// <summary>
    /// Asynchronously applies all active adjustments (preset filter values and slider adjustments combined) 
    /// to the source image, running the heavy computations on a background thread.
    /// </summary>
    /// <param name="source">The source BitmapSource.</param>
    /// <param name="settings">The current user adjustment settings.</param>
    /// <param name="filter">The currently active filter preset, or null.</param>
    /// <param name="ct">The CancellationToken to cancel the operation.</param>
    /// <returns>A new, frozen BitmapSource representing the edited image.</returns>
    /// <exception cref="ArgumentNullException">Thrown if source or settings is null.</exception>
    public async Task<BitmapSource> ApplyEditAsync(BitmapSource source, AdjustmentSettings settings, FilterPreset? filter, CancellationToken ct)
    {
        return await ApplyEditAsync(source, settings, filter, 100.0, ct);
    }

    /// <summary>
    /// Asynchronously applies all active adjustments (preset filter values, filter opacity blending, and slider adjustments combined) 
    /// to the source image, running the heavy computations on a background thread.
    /// </summary>
    /// <param name="source">The source BitmapSource.</param>
    /// <param name="settings">The current user adjustment settings.</param>
    /// <param name="filter">The currently active filter preset, or null.</param>
    /// <param name="filterOpacity">The opacity of the preset filter (0 to 100).</param>
    /// <param name="ct">The CancellationToken to cancel the operation.</param>
    /// <returns>A new, frozen BitmapSource representing the edited image.</returns>
    /// <exception cref="ArgumentNullException">Thrown if source or settings is null.</exception>
    public async Task<BitmapSource> ApplyEditAsync(
        BitmapSource source, 
        AdjustmentSettings settings, 
        FilterPreset? filter, 
        double filterOpacity, 
        CancellationToken ct)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();

            byte[] originalPixels = GetPixelData(source);
            byte[] pixels = (byte[])originalPixels.Clone();
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;

            // Step 1: Apply ONLY the preset filter (if active and opacity > 0)
            if (filter != null && filterOpacity > 0.0)
            {
                // Apply filter specific temperature & tint
                if (filter.Temperature != 0.0) ApplyTemperature(pixels, filter.Temperature, stride);
                if (filter.Tint != 0.0) ApplyTint(pixels, filter.Tint, stride);
                
                // Apply RGB multipliers
                if (filter.RedMultiplier != 1.0 || filter.GreenMultiplier != 1.0 || filter.BlueMultiplier != 1.0)
                {
                    ApplyRgbMultipliers(pixels, filter.RedMultiplier, filter.GreenMultiplier, filter.BlueMultiplier, stride);
                }

                // Apply HSL offsets
                ApplyHslOffsets(pixels, filter, stride);

                // Apply filter specific exposure adjustments
                if (filter.Brightness != 0.0) ApplyBrightness(pixels, filter.Brightness, stride);
                if (filter.Contrast != 0.0) ApplyContrast(pixels, filter.Contrast, stride);
                if (filter.Saturation != 0.0) ApplySaturation(pixels, filter.Saturation, stride);
                if (filter.Highlights != 0.0) ApplyHighlights(pixels, filter.Highlights, stride);
                if (filter.Shadows != 0.0) ApplyShadows(pixels, filter.Shadows, stride);
                if (filter.Fade != 0.0) ApplyFade(pixels, filter.Fade, stride);
                if (filter.Grain != 0.0) ApplyGrain(pixels, filter.Grain, stride, seed: 42);
                if (filter.Vignette != 0.0) ApplyVignette(pixels, filter.Vignette, width, height, stride);

                ct.ThrowIfCancellationRequested();

                // Blend filter results with original pixels based on filterOpacity
                if (filterOpacity < 100.0)
                {
                    double blend = filterOpacity / 100.0;
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        pixels[i] = (byte)(originalPixels[i] * (1.0 - blend) + pixels[i] * blend);
                    }
                }
            }

            ct.ThrowIfCancellationRequested();

            // Step 2: Apply manual user slider settings ON TOP of the (blended) image
            if (settings.Temperature != 0.0) ApplyTemperature(pixels, settings.Temperature, stride);
            if (settings.Tint != 0.0) ApplyTint(pixels, settings.Tint, stride);
            
            if (settings.Brightness != 0.0) ApplyBrightness(pixels, settings.Brightness, stride);
            if (settings.Contrast != 0.0) ApplyContrast(pixels, settings.Contrast, stride);
            if (settings.Saturation != 0.0) ApplySaturation(pixels, settings.Saturation, stride);
            if (settings.Highlights != 0.0) ApplyHighlights(pixels, settings.Highlights, stride);
            if (settings.Shadows != 0.0) ApplyShadows(pixels, settings.Shadows, stride);
            if (settings.Fade != 0.0) ApplyFade(pixels, settings.Fade, stride);
            if (settings.Grain != 0.0) ApplyGrain(pixels, settings.Grain, stride, seed: 99);
            if (settings.Vignette != 0.0) ApplyVignette(pixels, settings.Vignette, width, height, stride);

            ct.ThrowIfCancellationRequested();

            return CreateBitmapFromPixels(pixels, width, height, PixelFormats.Bgra32, stride);
        }, ct);
    }

    /// <summary>
    /// Applies custom HSL adjustments to color channels based on target colors.
    /// Hue coordinates are interpolated on the color wheel between standard color centers.
    /// </summary>
    /// <param name="pixels">The pixel data array to modify in-place (Bgra32 format).</param>
    /// <param name="filter">The filter preset defining the HSL channel offsets.</param>
    /// <param name="stride">The stride of the pixel data.</param>
    public void ApplyHslOffsets(byte[] pixels, FilterPreset filter, int stride)
    {
        if (pixels == null) throw new ArgumentNullException(nameof(pixels));
        if (filter == null) throw new ArgumentNullException(nameof(filter));

        // Define centers of the 8 HSL color channels on the Hue circle (0-360)
        double[] centers = { 0.0, 30.0, 60.0, 120.0, 180.0, 240.0, 300.0, 330.0, 360.0 };

        double[] hueOffsets = { 
            filter.HueRed, filter.HueOrange, filter.HueYellow, filter.HueGreen, 
            filter.HueAqua, filter.HueBlue, filter.HuePurple, filter.HueMagenta, 
            filter.HueRed 
        };

        double[] satOffsets = { 
            filter.SaturationRed, filter.SaturationOrange, filter.SaturationYellow, filter.SaturationGreen, 
            filter.SaturationAqua, filter.SaturationBlue, filter.SaturationPurple, filter.SaturationMagenta, 
            filter.SaturationRed 
        };

        double[] lumOffsets = { 
            filter.LuminanceRed, filter.LuminanceOrange, filter.LuminanceYellow, filter.LuminanceGreen, 
            filter.LuminanceAqua, filter.LuminanceBlue, filter.LuminancePurple, filter.LuminanceMagenta, 
            filter.LuminanceRed 
        };

        bool hasActiveOffsets = false;
        for (int i = 0; i < 8; i++)
        {
            if (hueOffsets[i] != 0.0 || satOffsets[i] != 0.0 || lumOffsets[i] != 0.0)
            {
                hasActiveOffsets = true;
                break;
            }
        }
        if (!hasActiveOffsets) return;

        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte b = pixels[i];
            byte g = pixels[i + 1];
            byte r = pixels[i + 2];

            if (r != g || g != b)
            {
                var hsl = RgbToHsl(r, g, b);
                double h = hsl.H;

                int idx = 0;
                for (int j = 0; j < centers.Length - 1; j++)
                {
                    if (h >= centers[j] && h <= centers[j + 1])
                    {
                        idx = j;
                        break;
                    }
                }

                double range = centers[idx + 1] - centers[idx];
                double weight = (h - centers[idx]) / range;

                double dH = (1.0 - weight) * hueOffsets[idx] + weight * hueOffsets[idx + 1];
                double dS = (1.0 - weight) * satOffsets[idx] + weight * satOffsets[idx + 1];
                double dL = (1.0 - weight) * lumOffsets[idx] + weight * lumOffsets[idx + 1];

                double newH = (h + dH) % 360.0;
                if (newH < 0.0) newH += 360.0;

                double newS = hsl.S + (dS / 100.0);
                if (newS < 0.0) newS = 0.0;
                if (newS > 1.0) newS = 1.0;

                double newL = hsl.L + (dL / 100.0);
                if (newL < 0.0) newL = 0.0;
                if (newL > 1.0) newL = 1.0;

                var rgb = HslToRgb(newH, newS, newL);
                pixels[i] = rgb.B;
                pixels[i + 1] = rgb.G;
                pixels[i + 2] = rgb.R;
            }
        }
    }

    /// <summary>
    /// Adjusts highlights (Luminance > 170) of the pixel array in-place.
    /// Uses a weight ramp to ensure smooth transitions.
    /// </summary>
    /// <param name="pixels">The pixel data array to modify in-place (Bgra32 format).</param>
    /// <param name="amount">The adjustment offset (-100 to +100).</param>
    /// <param name="stride">The stride of the pixel data.</param>
    public void ApplyHighlights(byte[] pixels, double amount, int stride)
    {
        if (pixels == null) throw new ArgumentNullException(nameof(pixels));

        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte b = pixels[i];
            byte g = pixels[i + 1];
            byte r = pixels[i + 2];

            double luminance = 0.299 * r + 0.587 * g + 0.114 * b;

            if (luminance > 170.0)
            {
                double weight = (luminance - 170.0) / (255.0 - 170.0);
                double offset = amount * weight;

                pixels[i] = ClampToByte(pixels[i] + offset);       // Blue
                pixels[i + 1] = ClampToByte(pixels[i + 1] + offset); // Green
                pixels[i + 2] = ClampToByte(pixels[i + 2] + offset); // Red
            }
        }
    }

    /// <summary>
    /// Adjusts shadows (Luminance < 85) of the pixel array in-place.
    /// Uses a weight ramp to ensure smooth transitions.
    /// </summary>
    /// <param name="pixels">The pixel data array to modify in-place (Bgra32 format).</param>
    /// <param name="amount">The adjustment offset (-100 to +100).</param>
    /// <param name="stride">The stride of the pixel data.</param>
    public void ApplyShadows(byte[] pixels, double amount, int stride)
    {
        if (pixels == null) throw new ArgumentNullException(nameof(pixels));

        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte b = pixels[i];
            byte g = pixels[i + 1];
            byte r = pixels[i + 2];

            double luminance = 0.299 * r + 0.587 * g + 0.114 * b;

            if (luminance < 85.0)
            {
                double weight = (85.0 - luminance) / 85.0;
                double offset = amount * weight;

                pixels[i] = ClampToByte(pixels[i] + offset);       // Blue
                pixels[i + 1] = ClampToByte(pixels[i + 1] + offset); // Green
                pixels[i + 2] = ClampToByte(pixels[i + 2] + offset); // Red
            }
        }
    }

    /// <summary>
    /// Compresses shadows in-place to emulate a faded, matte black analog paper look.
    /// </summary>
    /// <param name="pixels">The pixel data array to modify in-place (Bgra32 format).</param>
    /// <param name="amount">The intensity of the fade (0 to 100).</param>
    /// <param name="stride">The stride of the pixel data.</param>
    public void ApplyFade(byte[] pixels, double amount, int stride)
    {
        if (pixels == null) throw new ArgumentNullException(nameof(pixels));

        double minimum = (amount / 100.0) * 40.0;

        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = ClampToByte(minimum + pixels[i] * (255.0 - minimum) / 255.0);       // Blue
            pixels[i + 1] = ClampToByte(minimum + pixels[i + 1] * (255.0 - minimum) / 255.0); // Green
            pixels[i + 2] = ClampToByte(minimum + pixels[i + 2] * (255.0 - minimum) / 255.0); // Red
        }
    }

    /// <summary>
    /// Applies seeded monochromatic grain/noise in-place to emulate analog film grain.
    /// </summary>
    /// <param name="pixels">The pixel data array to modify in-place (Bgra32 format).</param>
    /// <param name="amount">The intensity of the grain (0 to 100).</param>
    /// <param name="stride">The stride of the pixel data.</param>
    /// <param name="seed">A random seed value for reproducibility.</param>
    public void ApplyGrain(byte[] pixels, double amount, int stride, int seed)
    {
        if (pixels == null) throw new ArgumentNullException(nameof(pixels));

        var random = new Random(seed);

        for (int i = 0; i < pixels.Length; i += 4)
        {
            double noise = (random.NextDouble() * 2.0 * amount) - amount;

            pixels[i] = ClampToByte(pixels[i] + noise);       // Blue
            pixels[i + 1] = ClampToByte(pixels[i + 1] + noise); // Green
            pixels[i + 2] = ClampToByte(pixels[i + 2] + noise); // Red
        }
    }

    /// <summary>
    /// Applies a radial vignette in-place, darkening pixels closer to the image boundaries.
    /// </summary>
    /// <param name="pixels">The pixel data array to modify in-place (Bgra32 format).</param>
    /// <param name="amount">The intensity of the vignette (0 to 100).</param>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="stride">The stride of the pixel data.</param>
    public void ApplyVignette(byte[] pixels, double amount, int width, int height, int stride)
    {
        if (pixels == null) throw new ArgumentNullException(nameof(pixels));

        double cx = width / 2.0;
        double cy = height / 2.0;
        double maxDistSq = cx * cx + cy * cy;

        for (int y = 0; y < height; y++)
        {
            int rowOffset = y * stride;
            double dy = y - cy;
            double dySq = dy * dy;

            for (int x = 0; x < width; x++)
            {
                int i = rowOffset + (x * 4);
                double dx = x - cx;

                // Squared distance factor (avoids Math.Sqrt)
                double distSq = (dx * dx + dySq) / maxDistSq;
                
                double factor = 1.0 - (amount / 100.0) * distSq;
                if (factor < 0.0) factor = 0.0;

                pixels[i] = ClampToByte(pixels[i] * factor);       // Blue
                pixels[i + 1] = ClampToByte(pixels[i + 1] * factor); // Green
                pixels[i + 2] = ClampToByte(pixels[i + 2] * factor); // Red
            }
        }
    }
}
