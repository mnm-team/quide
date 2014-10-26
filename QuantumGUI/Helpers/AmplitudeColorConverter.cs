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
using System.Net;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QuIDE.Helpers
{
    public class AmplitudeColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts complex number to color in RGB color space. 
        /// Firstly converts the number to HSV color, and then to RGB. 
        /// The HSV to RGB transformation is based on a standard algorithm.
        /// Special thanks to 
        /// http://www.algorytm.org/modele-barw/transformacja-hsv-rgb.html
        /// for presenting the conversion algorithm.
        /// </summary>
        /// <param name="value">The complex number.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>The brush (SolidColorBrush) of the color representing the number on the complex plane.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Color color = Colors.LightGray;
            
            Complex? amplitude = value as Complex?;           
            if (amplitude.HasValue)
            {
                double hue = amplitude.Value.Phase * 180 / Math.PI;
                if (hue < 0)
                {
                    hue += 360;
                }
                double sat = 0.75;
                double val = 1.0;

                double red = 0, grn = 0, blu = 0;

                hue /= 60;
                int i = (int)Math.Floor(hue);
                double f = hue - i;
                double p = val * (1 - sat);
                double q = val * (1 - (sat * f));
                double t = val * (1 - (sat * (1 - f)));
                if (i == 0) { red = val; grn = t; blu = p; }
                else if (i == 1) { red = q; grn = val; blu = p; }
                else if (i == 2) { red = p; grn = val; blu = t; }
                else if (i == 3) { red = p; grn = q; blu = val; }
                else if (i == 4) { red = t; grn = p; blu = val; }
                else if (i == 5) { red = val; grn = p; blu = q; }

                byte R = (byte)(red * 255);
                byte G = (byte)(grn * 255);
                byte B = (byte)(blu * 255);

                color = Color.FromArgb(255, R, G, B);
            }
           
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;  
        }
    }
}
