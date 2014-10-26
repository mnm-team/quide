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
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace QuIDE.ViewModels
{
    public class StateVM : ViewModelBase
    {
        #region Fields

        // TODO Magic Number - moze DependencyProperty?
        private double _rectangleMaxWidth = 125;

        private OutputState _model;
        private bool[] _bits;
        private double _rectangleWidth;
        private double _relativeProbability;

        #endregion // Fields


        #region Constructor

        public StateVM(OutputState model)
        {
            _model = model;
            _bits = new bool[_model.Width];
        }

        #endregion // Constructor


        #region Presentation Properties

        public double Probability
        {
            get
            {
                return _model.Probability;
            }
        }

        public float ProbabilityFloat
        {
            get
            {
                return (float)_model.Probability;
            }
        }

        public double RectangleWidth
        {
            get
            {
                return _rectangleWidth;
            }
        }

        public double RectangleMaxWidth
        {
            get
            {
                return _rectangleMaxWidth;
            }
            set
            {
                if (value == _rectangleMaxWidth)
                {
                    return;
                }
                _rectangleMaxWidth = value;
                OnPropertyChanged("RectangleMaxWidth");
            }
        }

        public double RelativeProbability
        {
            get
            {
                return _relativeProbability;
            }
            set
            {
                _relativeProbability = value;
                _rectangleWidth = _relativeProbability * _rectangleMaxWidth;
                OnPropertyChanged("RelativeProbability");
                OnPropertyChanged("RectangleWidth");
            }
        }

        public char[] Bits
        {
            get
            {
                return _model.Bits;
            }
        }

        public ulong Value
        {
            get
            {
                return _model.Value;
            }
        }

        public Complex? Amplitude
        {
            get
            {
                return _model.Amplitude;
            }
        }

        public string Representation
        {
            get
            {
                return _model.Representation;
            }
        }

        public OutputState Model
        {
            get { return _model; }
        }

        #endregion // Presentation Properties


        #region Private Helpers

        #endregion // Private Helpers
    }
}
