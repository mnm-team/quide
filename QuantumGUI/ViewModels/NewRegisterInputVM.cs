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

using Quantum.Helpers;
using QuIDE.Helpers;
using QuantumModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuIDE.ViewModels
{
    public class NewRegisterInputVM : ViewModelBase
    {
        #region Fields

        private uint _width = ComputerModel.InitialQubitsCount;
        private ObservableCollection<InitState> _initStates;

        private DelegateCommand _add;
        private DelegateCommand _normalize;

        #endregion // Fields

        #region Public Properties
        public uint Width
        {
            get { return _width; }
            set
            {
                if (value < _width)
                {
                    TrimTooWideStates(value);
                }
                _width = value;
                _add.CanExecute(null);
            }
        }

        public ObservableCollection<InitState> InitStates
        {
            get
            {
                if (_initStates == null)
                {
                    _initStates = CreateInitStates();
                    _initStates.CollectionChanged += _initStates_CollectionChanged;
                }
                return _initStates;
            }
            set
            {
                _initStates = value;
                OnPropertyChanged("InitStates");
            }
        }

        public ICommand AddCommand
        {
            get
            {
                if (_add == null)
                {
                    _add = new DelegateCommand(Add, x =>
                    {
                        if (Math.Pow(2, _width) > InitStates.Count)
                        {
                            return true;
                        }
                        //if (Width.HasValue)
                        //{
                        //    if (Math.Pow(2, Width.Value) > InitStates.Count)
                        //    {
                        //        return true;
                        //    }
                        //}
                        return false;
                    });
                }
                return _add;
            }
        }

        public ICommand NormalizeCommand
        {
            get
            {
                if (_normalize == null)
                {
                    //_normalize = new DelegateCommand(Normalize, x => Width.HasValue);
                    _normalize = new DelegateCommand(Normalize, x => true);
                }
                return _normalize;
            }
        }

        #endregion // Public Properties


        #region Public Methods
        public void Add(object parameter)
        {
            HashSet<ulong> values = new HashSet<ulong>();
            foreach (var item in InitStates)
            {
                values.Add(item.Value);
            }
            //ulong max = (ulong)1 << Width.Value;
            ulong max = (ulong)1 << (int)_width;
            ulong i = 0;
            InitState added = null;

            while(added == null && i < max)
            {
                if (!values.Contains(i))
                {
                    added = new InitState() { Value = i, Amplitude = Complex.Zero };
                }
                i++;
            }

            if (added != null)
            {
                InitStates.Add(added);
            }
        }

        public void Normalize(object parameter)
        {
            double sum = 0;
            foreach (InitState state in InitStates)
            {
                sum += Math.Pow(state.Amplitude.Magnitude, 2);
            }
            double limit = (1.0 / ((ulong)1 <<(int)_width)) * Quantum.QuantumComputer.Epsilon;
            
            if (Math.Abs(sum - 1.0) > limit)
            {
                double sqrtSum = Math.Sqrt(sum);
                //we need to normalize
                var oldStates = _initStates;
                var newStates = new ObservableCollection<InitState>();
                foreach (var state in oldStates)
                {
                    newStates.Add(new InitState() { Value = state.Value, Amplitude = state.Amplitude / sqrtSum });
                }
                InitStates = newStates;
            }
        }

        public Dictionary<ulong, Complex> GetInitStates()
        {
            Dictionary<ulong, Complex> states = new Dictionary<ulong, Complex>();
            foreach (var item in InitStates)
            {
                if(item.Amplitude != Complex.Zero) {
                    states[item.Value] = item.Amplitude;
                }
            }
            return states;
        }
        #endregion // Public Methods


        #region Private Helpers
        private ObservableCollection<InitState> CreateInitStates()
        {
            ObservableCollection<InitState> states = new ObservableCollection<InitState>();
            states.Add(new InitState() { Value = 0, Amplitude = Complex.One });
            return states;
        }

        private void _initStates_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    _add.CanExecute(null);
                    break;
            }
        }

        private void TrimTooWideStates(uint newWidth)
        {
            ulong max = (ulong)1 << (int)newWidth;

            var oldStates = _initStates;
            var newStates = new ObservableCollection<InitState>();
            foreach (var state in oldStates)
            {
                if (state.Value < max)
                {
                    newStates.Add(state);
                }
            }
            InitStates = newStates;
        }

        //private ObservableCollection<OutputState> CreateInitStates()
        //{
        //    ObservableCollection<OutputState> states = new ObservableCollection<OutputState>();
        //    states.Add(new OutputState(0, Complex.One, ComputerModel.InitialQubitsCount));
        //    return states;
        //}

        #endregion // Private Helpers
    }

    public class InitState
    {
        private ulong _value;
        private Complex _amplitude;
        private string _amplitudeString;
        
        private static IFormatProvider formatter = new ComplexFormatter();

        public ulong Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public Complex Amplitude
        {
            get { return _amplitude; }
            set 
            { 
                _amplitude = value;
                _amplitudeString = string.Format(formatter, "{0:I2}", _amplitude);
            }
        }

        public string AmplitudeString
        {
            get { return _amplitudeString; }
            set
            {
                _amplitudeString = value;
                Complex number;
                if (ComplexParser.TryParse(value, out number))
                {
                    // Number is valid 
                    _amplitude = number;
                }
            }
        }
    }
}
