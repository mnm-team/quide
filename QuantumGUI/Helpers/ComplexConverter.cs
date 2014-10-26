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
using System.Windows.Data;
using Quantum.Helpers;

namespace QuIDE.Helpers
{
    public class ComplexConverter : IValueConverter
    {
        private IFormatProvider _formatter = new ComplexFormatter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Complex? arg = value as Complex?;
            if (arg.HasValue)
            {
                return String.Format(_formatter, "{0:I2}", arg.Value);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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
}
