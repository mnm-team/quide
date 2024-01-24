#region

using System;
using System.Globalization;
using System.Reflection;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

#endregion

namespace AvaloniaGUI.CodeHelpers;

/// <summary>
/// <para>
/// Converts a string path to a bitmap asset. Necessary to get images.Source update when binding to string paths.
/// </para>
/// <para>
/// The asset must be in the same assembly as the program. If it isn't,
/// specify "avares://<assemblynamehere>/" in front of the path to the asset.
/// </para>
/// </summary>
public class ImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Display no image (path was set to null)
        if (string.IsNullOrEmpty(value as string))
            return null;

        if (value is not string rawUri || !targetType.IsAssignableFrom(typeof(Bitmap)))
            throw new NotSupportedException();

        Uri uri;

        // Allow for assembly overrides
        if (rawUri.StartsWith("avares://"))
        {
            uri = new Uri(rawUri);
        }
        else
        {
            string assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            uri = new Uri($"avares://{assemblyName}{rawUri}");
        }

        var asset = AssetLoader.Open(uri);

        return new Bitmap(asset);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}