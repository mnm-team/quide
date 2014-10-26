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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuIDE.ViewModels
{
    public class GammaInputVM : ViewModelBase
    {
        private DelegateCommand _selectUnit;
        private bool _rad;
        private double _gammaRad;
        private string _gammaString;

        public double Gamma
        {
            get
            {
                return _gammaRad;
            }
        }

        public ICommand SelectUnitCommand
        {
            get
            {
                if (_selectUnit == null)
                {
                    _selectUnit = new DelegateCommand(SelectUnit, x => true);
                }
                return _selectUnit;
            }
        }

        public bool Rad
        {
            get { return _rad; }
            set
            {
                if (_rad != value)
                {
                    _rad = value;
                    OnPropertyChanged("Rad");
                    GammaText = GammaToString();
                }
            }
        }

        public string GammaText
        {
            get { return _gammaString; }
            set
            {
                double result;
                if (double.TryParse(value, out result))
                {
                    if (_rad)
                    {
                        _gammaRad = result;
                    }
                    else
                    {
                        _gammaRad = result * Math.PI / 180;
                    }
                    _gammaString = GammaToString();
                    OnPropertyChanged("GammaText");
                }
            }
        }

        public void SelectUnit(object parameter)
        {
            if (string.Equals("Rad", parameter as string))
            {
                Rad = true;
            }
            else
            {
                Rad = false;
            }
        }

        public void SetAngle(string value)
        {
            if (string.Equals(value, Resources.Pi))
            {
                _gammaRad = Math.PI;
                GammaText = GammaToString();
            }
            else if (string.Equals(value, Resources.Pi_2))
            {
                _gammaRad = Math.PI / 2.0;
                GammaText = GammaToString();
            }
            else if (string.Equals(value, Resources.Pi_3))
            {
                _gammaRad = Math.PI / 3.0;
                GammaText = GammaToString();
            }
            else if (string.Equals(value, Resources.Pi_4))
            {
                _gammaRad = Math.PI / 4.0;
                GammaText = GammaToString();
            }
            else if (string.Equals(value, Resources.Pi_6))
            {
                _gammaRad = Math.PI / 6.0;
                GammaText = GammaToString();
            }
            else if (string.Equals(value, Resources.Pi_8))
            {
                _gammaRad = Math.PI / 8.0;
                GammaText = GammaToString();
            }
        }

        private string GammaToString()
        {
            if (_rad)
            {
                return string.Format("{0:N5}", _gammaRad);
            }
            else
            {
                return string.Format("{0:N2}", _gammaRad * 180 / Math.PI);
            }
        }
    }


    public class DoubleValidationRule : ValidationRule
    {
        //double minMargin;
        //double maxMargin;

        //public double MinMargin
        //{
        //    get { return this.minMargin; }
        //    set { this.minMargin = value; }
        //}

        //public double MaxMargin
        //{
        //    get { return this.maxMargin; }
        //    set { this.maxMargin = value; }
        //}

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double margin;

            // Is a number? 
            if (!double.TryParse((string)value, out margin))
            {
                return new ValidationResult(false, "Not a number.");
            }

            //// Is in range? 
            //if ((margin < this.minMargin) || (margin > this.maxMargin))
            //{
            //    string msg = string.Format("Margin must be between {0} and {1}.", this.minMargin, this.maxMargin);
            //    return new ValidationResult(false, msg);
            //}

            // Number is valid 
            return new ValidationResult(true, null);
        }
    }


}
