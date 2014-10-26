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

using Quantum;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows;

namespace QuantumModel
{
    public class Output
    {
        #region Events

        public event RoutedEventHandler OutputChanged;
        private void OnOutputChanged()
        {
            if (OutputChanged != null)
            {
                OutputChanged(this, new RoutedEventArgs());
            }
        }

        #endregion // Events


        #region Fields

        private RegisterPartModel? _selectedRegister = null;
        private List<OutputState> _states = new List<OutputState>();

        #endregion // Fields


        #region Constructor

        public Output()
        {
        }

        //public Output(ObservableCollection<RegisterModel> initRegisters)
        //{
        //    _states = new List<OutputState>();
        //    ulong initValue = 0;
        //    ulong pow2 = 1;
        //    int width = 0;
        //    for (int i = 0; i < initRegisters.Count; i++)
        //    {
        //        RegisterModel reg = initRegisters[i];
        //        width += reg.Qubits.Count;

        //        for (int j = 0; j < reg.Qubits.Count; j++)
        //        {
        //            if (reg.Qubits[j] == QubitModel.One)
        //            {
        //                initValue += pow2;
        //            }
        //            pow2 *= 2;
        //        }
        //    }
        //    OutputState first = new OutputState(initValue, Complex.One, width);
        //    _states.Add(first);
        //}

        #endregion // Constructor


        #region Model Properties

        public List<OutputState> States
        {
            get
            {
                return _states;
            }
        }

        public RegisterPartModel? SelectedRegister
        {
            get
            {
                return _selectedRegister;
            }
        }

        public int Width
        {
            get
            {
                if (_states == null || _states.Count == 0)
                {
                    return 0;
                }
                else
                {
                    return _states[0].Width;
                }
            }
        }

        #endregion // Model Properties


        #region Public Methods

        public void Update(Register rootRegister, RegisterPartModel? regModel = null)
        {
            Register register = rootRegister;

            if (regModel.HasValue)
            {
                _selectedRegister = regModel.Value;
            }

            if (_selectedRegister.HasValue && _selectedRegister.Value.Width < rootRegister.Width)
            {
                register = rootRegister[_selectedRegister.Value.OffsetToRoot, _selectedRegister.Value.Width];
            }

            _states.Clear();

            IReadOnlyDictionary<ulong, Complex> amplitudes = register.GetAmplitudes();
            if (amplitudes != null)
            {
                foreach (ulong state in amplitudes.Keys)
                {
                    OutputState newState = new OutputState(state, amplitudes[state], register.Width);
                    _states.Add(newState);
                }
            }
            else
            {
                IReadOnlyDictionary<ulong, double> probabilities = register.GetProbabilities();
                foreach (ulong state in probabilities.Keys)
                {
                    OutputState newState = new OutputState(state, probabilities[state], register.Width);
                    _states.Add(newState);
                }
            }
            OnOutputChanged();
        }

        #endregion // Public Methods
    }
}
