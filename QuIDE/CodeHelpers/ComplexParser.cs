#region

using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

#endregion

namespace AvaloniaGUI.CodeHelpers;

public class ComplexParser
{
    public static bool TryParse(string text, out Complex number)
    {
        string regexPattern =
            // Match any float, negative or positive, group it
            @"^\s*([-+]?\s*(\d+\.\d+|\d*)\s*i|[-+]?\s*(\d+\.\d+|\d+))" +
            // ... possibly following that with whitespace
            @"\s*" +
            // Match any other float, and save it
            @"([-+]\s*(\d+\.\d+|\d*)\s*i|[-+]\s*(\d+\.\d+|\d+))?\s*$";

        Regex regex = new Regex(regexPattern);

        Match match = regex.Match(text);
        if (match.Groups.Count == 7)
        {
            double real = 0, img = 0;
            if (!string.IsNullOrWhiteSpace(match.Groups[3].Value)) // without i
            {
                if (match.Groups[1].Value.Contains('-'))
                {
                    real -= double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                }
                else
                {
                    real += double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                }
            }
            else //i
            {
                double tmp;
                if (match.Groups[1].Value.Contains('-'))
                {
                    if (double.TryParse(match.Groups[2].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out tmp))
                    {
                        img -= tmp;
                    }
                    else
                    {
                        img -= 1;
                    }
                }
                else
                {
                    if (double.TryParse(match.Groups[2].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out tmp))
                    {
                        img += tmp;
                    }
                    else
                    {
                        img += 1;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(match.Groups[6].Value)) // without i
            {
                if (match.Groups[4].Value.Contains('-'))
                {
                    real -= double.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
                }
                else
                {
                    real += double.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
                }
            }
            else if (match.Groups[4].Value.Contains('i'))
            {
                double tmp;
                if (match.Groups[4].Value.Contains('-'))
                {
                    if (double.TryParse(match.Groups[5].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out tmp))
                    {
                        img -= tmp;
                    }
                    else
                    {
                        img -= 1;
                    }
                }
                else
                {
                    if (double.TryParse(match.Groups[5].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out tmp))
                    {
                        img += tmp;
                    }
                    else
                    {
                        img += 1;
                    }
                }
            }

            number = new Complex(real, img);
            return true;
        }

        number = Complex.Zero;
        return false;
    }
}