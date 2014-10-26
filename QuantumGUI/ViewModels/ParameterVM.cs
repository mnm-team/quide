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

using QuantumModel;
using QuantumParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuIDE.ViewModels
{
    public class ParameterVM : ViewModelBase
    {
        private string _name;
        
        private Type _type;
        private string _typeString;

        private object _value;
        private string _valueString;

        private bool _isValid = false;
        private string _validationMessage = "Parameter not set";

        private bool _paramsArray;
        private bool _nextParamArray;

        public ParameterVM(string name, Type type, object value = null, 
            bool paramsArray = false, bool nextParamArray = false)
        {
            _name = name;
            
            _type = type;
            _typeString = TypeToString(_type);

            _paramsArray = paramsArray;
            _nextParamArray = nextParamArray;
            if (_nextParamArray)
            {
                _isValid = true;
                _validationMessage = null;
            }

            if (value != null)
            {
                ValueString = ValueToString(value);
            }

            OnPropertyChanged("ValidationMessage");
        }

        public string Name
        {
            get { return _name; }
        }

        public string TypeString
        {
            get { return _typeString; }
        }

        public string ValueString
        {
            get { return _valueString; }
            set
            {
                _valueString = value;
                _value = StringToValue(_valueString, _type);
                OnPropertyChanged("Value");
                OnPropertyChanged("ValueString");
                OnPropertyChanged("IsValid");
                OnPropertyChanged("ValidationMessage");
            }
        }

        public object Value
        {
            get { return _value; }
        }

        public bool IsValid
        {
            get { return _isValid; }
        }

        public string ValidationMessage
        {
            get
            {
                if (!_isValid)
                {
                    return _name + ": " + _validationMessage;
                }
                return null;
            }
        }

        public Visibility VarParamsVisibility
        {
            get
            {
                if (_paramsArray)
                {
                    return Visibility.Visible;
                }
                return Visibility.Hidden;
            }
        }


        private string TypeToString(Type type)
        {
            string[] split = type.ToString().Split('.');
            return split[split.Length - 1];
        }

        private string ValueToString(object value)
        {
            Type type = value.GetType();
            if (type == typeof(RegisterRefModel))
            {
                RegisterRefModel rrm = (RegisterRefModel)value;
                StringBuilder sb = new StringBuilder();
                if (rrm.Register != null)
                {
                    sb.Append(rrm.Register.Name);
                }
                else
                {
                    sb.Append("root");
                }
                sb.Append("[").Append(rrm.Offset).Append("]");
                return sb.ToString();
            }
            if (type == typeof(RegisterPartModel))
            {
                RegisterPartModel rm = (RegisterPartModel)value;
                if (rm.Register != null)
                {
                    if (rm.Width == rm.Register.Qubits.Count)
                    {
                        return rm.Register.Name;
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(rm.Register.Name);
                        sb.Append("[").Append(rm.Offset).Append(", ").Append(rm.Width).Append("]");
                        return sb.ToString();
                    }
                }
                else
                {
                    CircuitEvaluator eval = CircuitEvaluator.GetInstance();
                    int rootWidth = eval.RootRegister.Width;
                    if (rm.Width == rootWidth)
                    {
                        return "root";
                    }
                    else {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("root");
                        sb.Append("[").Append(rm.Offset).Append(", ").Append(rm.Width).Append("]");
                        return sb.ToString();
                    }
                }
            }
            return value.ToString();
        }

        private object StringToValue(string text, Type type)
        {
            object toReturn = null;
            try
            {
                CircuitEvaluator eval = CircuitEvaluator.GetInstance();

                if (type == typeof(QuantumParser.RegisterRef))
                {
                    toReturn = eval.ResolveRegisterRef(text, _nextParamArray);
                }
                else if (type == typeof(QuantumParser.Register))
                {
                    toReturn = eval.ResolveRegister(text, _nextParamArray);
                }
                else
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(type);
                    toReturn = converter.ConvertFromInvariantString(text);
                }
                _isValid = true;
                _validationMessage = null;
            }
            catch (Exception ex)
            {
                _isValid = false;
                _validationMessage = ex.Message;
            }
            return toReturn;
        }
    }


}
