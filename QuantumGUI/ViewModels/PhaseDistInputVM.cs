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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace QuIDE.ViewModels
{
    public class PhaseDistInputVM : ViewModelBase
    {
        private string _distText = String.Empty;

        public int? Dist
        {
            get
            {
                int dist;
                if (int.TryParse(_distText, out dist))
                {
                    return dist;
                }
                return null;
            }
        }

        public string DistText
        {
            get
            {
                return _distText;
            }
            set
            {
                _distText = value;
                OnPropertyChanged("DistText");
            }
        }
    }


    public class IntValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int parsed;

            // Is a number? 
            if (!int.TryParse((string)value, out parsed))
            {
                return new ValidationResult(false, "Not a number.");
            }
            return new ValidationResult(true, null);
        }
    }


}
