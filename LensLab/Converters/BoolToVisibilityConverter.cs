using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LensLab.Converters;

/// <summary>
/// Converts a boolean value (or the nullability of an object) to a WPF Visibility value.
/// Also supports inversion by passing "Inverse" as the converter parameter.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean or object reference to a Visibility value.
    /// </summary>
    /// <param name="value">The value to convert (bool or object).</param>
    /// <param name="targetType">The type of the binding target property (Visibility).</param>
    /// <param name="parameter">Optional parameter. Pass "Inverse" to invert the visibility behavior.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>Visibility.Visible if true or not-null (unless inverted); otherwise, Visibility.Collapsed.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool flag = false;

        if (value is bool booleanValue)
        {
            flag = booleanValue;
        }
        else if (value != null)
        {
            // For general object types, not-null counts as true (visible)
            flag = true;
        }

        bool invert = false;
        if (parameter is string paramStr && paramStr.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
        {
            invert = true;
        }

        if (invert)
        {
            flag = !flag;
        }

        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Converting back from Visibility is not supported.
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
