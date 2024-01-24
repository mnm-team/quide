#region

using System;
using System.Globalization;
using System.Numerics;
using Avalonia.Data.Converters;
using Quantum.Helpers;

#endregion

namespace QuIDE.CodeHelpers;

public class ComplexConverter : IValueConverter
{
    private IFormatProvider _formatter = new ComplexFormatter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Complex? arg = value as Complex?;
        if (arg.HasValue)
        {
            return String.Format(_formatter, "{0:I2}", arg.Value);
        }

        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Complex number;

        // Is a number? 
        if (ComplexParser.TryParse((string)value, out number))
        {
            // Number is valid 
            return number;
        }

        return null;
    }
}