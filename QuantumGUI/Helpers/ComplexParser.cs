/**
    This file is part of QuIDE.

    QuIDE - The Quantum IDE
    Copyright (C) 2014  Joanna Patrzyk, Bartłomiej Patrzyk

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuIDE.Helpers
{
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
}
