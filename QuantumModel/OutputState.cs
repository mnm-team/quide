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
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows;

namespace QuantumModel
{
    public class OutputState
    {
        #region Fields

        private double _probability;
        private char[] _bits;
        private string _representation;
        private Complex? _amplitute;
        private ulong _value;
        private int _width;

        #endregion // Fields


        #region Constructor

        public OutputState(ulong initialValue, Complex amplitude, int width)
        {
            _width = width;
            _bits = new char[width];
            _value = initialValue;
            UpdateBits(initialValue);
            _amplitute = amplitude;
            _probability = Math.Pow(amplitude.Magnitude, 2);
        }

        public OutputState(ulong initialValue, double probability, int width)
        {
            _width = width;
            _bits = new char[width];
            _value = initialValue;
            UpdateBits(initialValue);
            _amplitute = null;
            _probability = probability;
        }

        #endregion // Constructor


        #region Model Properties

        public double Probability
        {
            get
            {
                return _probability;
            }
            //set { _probability = value; }
        }

        public char[] Bits
        {
            get
            {
                return _bits;
            }
        }

        public Complex? Amplitude
        {
            get
            {
                return _amplitute;
            }
            //set { _amplitute = value; }
        }

        public ulong Value
        {
            get
            {
                return _value;
            }
            //set 
            //{ 
            //    _value = value;
            //    UpdateBits(_value);
            //}
        }

        public string Representation
        {
            get
            {
                return _representation;
            }
        }

        public int Width
        {
            get
            {
                return _width;
            }
        }

        #endregion // Model Properties


        #region Public Methods

        #endregion // Public Methods


        #region Private Helpers

        private void UpdateBits(ulong value)
        {
            StringBuilder representation = new StringBuilder();
            for (int i = 0; i < _bits.Length; i++)
            {
                char bit = (value % 2) == 1 ? '1' : '0';
                _bits[i] = bit;
                representation.Insert(0, bit.ToString());
                value = value / 2;
            }
            representation.Insert(0, "|");
            representation.Append(">");
            _representation = representation.ToString();
        }

        #endregion // Private Helpers
    }
}
