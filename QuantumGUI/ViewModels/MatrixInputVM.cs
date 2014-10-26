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

using QuIDE.Helpers;
using QuIDE.Properties;
using QuantumParser.Validation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace QuIDE.ViewModels
{
    public class MatrixInputVM : ViewModelBase
    {
        private string _a00Text = String.Empty;
        private string _a01Text = String.Empty;
        private string _a10Text = String.Empty;
        private string _a11Text = String.Empty;

        private Complex[,] _matrix = new Complex[2, 2] { { 0, 0 }, { 0, 0 } };

        private bool _isUnitary;

        public string A00Text
        {
            get
            {
                return _a00Text;
            }
            set
            {
                _a00Text = value;
                ComplexParser.TryParse(_a00Text, out _matrix[0, 0]);
                ValidateMatrix();
            }
        }

        public string A01Text
        {
            get
            {
                return _a01Text;
            }
            set
            {
                _a01Text = value;
                ComplexParser.TryParse(_a01Text, out _matrix[0, 1]);
                ValidateMatrix();
            }
        }

        public string A10Text
        {
            get
            {
                return _a10Text;
            }
            set
            {
                _a10Text = value;
                ComplexParser.TryParse(_a10Text, out _matrix[1, 0]);
                ValidateMatrix();
            }
        }

        public string A11Text
        {
            get
            {
                return _a11Text;
            }
            set
            {
                _a11Text = value;
                ComplexParser.TryParse(_a11Text, out _matrix[1, 1]);
                ValidateMatrix();
            }
        }

        public string ValidationMessage
        {
            get
            {
                if (_isUnitary)
                {
                    return string.Empty;
                }
                else
                {
                    return Resources.MatrixNotUnitary;
                }
            }
        }

        public Complex[,] Matrix
        {
            get
            {
                if (_isUnitary)
                {
                    return _matrix;
                }
                else
                {
                    return null;
                }
            }
        }

        private void ValidateMatrix()
        {
            _isUnitary = MatrixValidator.IsUnitary2x2(_matrix);
            OnPropertyChanged("ValidationMessage");
        }
    }


    public class ComplexValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            Complex number;

            // Is a number? 
            if (!ComplexParser.TryParse((string)value, out number))
            {
                return new ValidationResult(false, "Not a complex number.");
            }

            // Number is valid 
            return new ValidationResult(true, null);
        }
    }
}
