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
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace QuantumModel
{
    public class StepModel
    {
        #region Fields

        private ObservableCollection<Gate> _gates;

        #endregion // Fields


        #region Constructor

        public StepModel(IList<RegisterModel> initRegisters)
        {
            _gates = CreateGates(initRegisters);
        }

        #endregion // Constructor


        #region Model Properties

        public ObservableCollection<Gate> Gates
        {
            get { return _gates; }
            set { _gates = value; }
        }
        
        #endregion // Model Properties


        #region Public Methods

        public void SetGate(Gate gate)
        {
            int beginRow = gate.Begin;
            int endRow = gate.End;
            for (int i = beginRow; i <= endRow; i++)
            {
                _gates[i] = gate;
            }
        }

        public bool HasPlace(int beginRow, int endRow)
        {
            bool hasPlace = true;
            if (beginRow > endRow)
            {
                for (int i = endRow; i <= beginRow; i++)
                {
                    if (_gates[i].Name != GateName.Empty)
                    {
                        hasPlace = false;
                    }
                }
            }
            else
            {
                for (int i = beginRow; i <= endRow; i++)
                {
                    if (_gates[i].Name != GateName.Empty)
                    {
                        hasPlace = false;
                    }
                }
            }
            return hasPlace;
        }

        #endregion // Public Methods


        #region Private Helpers

        private ObservableCollection<Gate> CreateGates(IList<RegisterModel> initRegisters)
        {
            ObservableCollection<Gate> gates = new ObservableCollection<Gate>();
            for (int i = initRegisters.Count - 1; i >= 0; i--)
            {
                RegisterModel reg = initRegisters[i];
                IList<QubitModel> initQubits = reg.Qubits;
                int j = 0;
                for (; j < initQubits.Count; j++)
                {
                    gates.Add(new EmptyGate(new RegisterRefModel() { Register = reg, Offset = j }));
                }
            }
            return gates;
        }

        #endregion // Private Helpers
    }
}
