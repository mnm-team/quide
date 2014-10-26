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
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantum.Helpers
{
    /// <summary>
    /// The class based on an example from Microsoft MSDN Library -
    /// API reference of the System.Numerics.Complex structure.
    /// http://msdn.microsoft.com/en-us/library/system.numerics.complex(v=vs.110).aspx
    /// </summary>
    public class ComplexFormatter : IFormatProvider, ICustomFormatter
    {
        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
                return this;
            else
                return null;
        }

        public string Format(string format, object arg,
                             IFormatProvider provider)
        {
            if (arg is Complex)
            {
                Complex c1 = (Complex)arg;
                // Check if the format string has a precision specifier. 
                int precision;
                string fmtString = String.Empty;
                if (format.Length > 1)
                {
                    try
                    {
                        precision = Int32.Parse(format.Substring(1));
                    }
                    catch (FormatException)
                    {
                        precision = 0;
                    }
                    fmtString = "N" + precision.ToString();
                }

                bool trimZeros = (format.Substring(0, 1).Equals("K", StringComparison.OrdinalIgnoreCase));

                string toReturn = c1.Real.ToString(fmtString);
                if (trimZeros)
                {
                    toReturn = toReturn.TrimEnd('0');
                    toReturn = toReturn.TrimEnd('.');
                }

                if (c1.Real >= 0)
                {
                    toReturn = " " + toReturn;
                }
                if (c1.Imaginary < 0)
                {
                    double absI = -c1.Imaginary;
                    toReturn += " - " + absI.ToString(fmtString);
                }
                else
                {
                    toReturn += " + " + c1.Imaginary.ToString(fmtString);
                }

                if (trimZeros)
                {
                    toReturn = toReturn.TrimEnd('0');
                    toReturn = toReturn.TrimEnd('.');
                }

                if (format.Substring(0, 1).Equals("I", StringComparison.OrdinalIgnoreCase) ||
                    format.Substring(0, 1).Equals("K", StringComparison.OrdinalIgnoreCase))
                    return toReturn + "i";
                else if (format.Substring(0, 1).Equals("J", StringComparison.OrdinalIgnoreCase))
                    return toReturn + "j";
                else
                    return c1.ToString(format, provider);
            }
            else
            {
                if (arg is IFormattable)
                    return ((IFormattable)arg).ToString(format, provider);
                else if (arg != null)
                    return arg.ToString();
                else
                    return String.Empty;
            }
        }
    }
}
