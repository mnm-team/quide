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
using QuantumModel;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace QuIDE.ViewModels
{
    public class RegisterVM : ViewModelBase
    {
        #region Fields

        private ComputerModel _model;

        private int _registerIndex;

        private ObservableCollection<QubitVM> _qubits;

        #endregion // Fields


        #region Constructor

        public RegisterVM(ComputerModel model, int registerIndex)
        {            
            _model = model;
            _registerIndex = registerIndex;
            _model.Registers[_registerIndex].Qubits.CollectionChanged += Qubits_CollectionChanged;
            _model.StepChanged += _model_StepChanged;
        }
        
        #endregion // Constructor


        #region Presentation Properties

        public ObservableCollection<QubitVM> Qubits
        {
            get
            {
                if (_qubits == null)
                {
                    _qubits = CreateQubitsFromModel();
                }
                return _qubits;
            }
        }

        public string Name
        {
            get
            {
                return _model.Registers[_registerIndex].Name;
            }
        }

        public double ButtonHeight
        {
            get
            {
                return Qubits.Count * CircuitGridVM.QubitSize;
            }
        }

        public double ScaleCenterY
        {
            get
            {
                return Qubits.Count * CircuitGridVM.QubitScaleCenter;
            }
        }

        public bool AddQubitEnabled
        {
            get
            {
                return _model.CurrentStep == 0;
            }
        }

        public void IncrementIndex(int delta = 1)
        {
            _registerIndex += delta;
            foreach (QubitVM qubit in _qubits)
            {
                qubit.IncrementRegisterIndex(delta);
            }
        }

        #endregion // Presentation Properties


        #region Public Methods

        #endregion // Public Methods


        #region Private Helpers

        private ObservableCollection<QubitVM> CreateQubitsFromModel()
        {
            ObservableCollection<QubitVM> qubits = new ObservableCollection<QubitVM>();
            for (int i = 0; i < _model.Registers[_registerIndex].Qubits.Count; i++)
            {
                qubits.Add(new QubitVM(_model, _registerIndex, i));
            }
            return qubits;
        }

        private void Qubits_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            QubitModel qubit;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object item in e.NewItems)
                    {
                        int newRow = e.NewStartingIndex;
                        if (item is QubitModel)
                        {
                            qubit = (QubitModel)item;
                            _qubits.Insert(newRow, new QubitVM(_model, _registerIndex, newRow));
                            for (int i = newRow + 1; i < _qubits.Count; i++)
                            {
                                _qubits[i].IncrementRow();
                            }
                            if (_qubits.Count == 2)
                            {
                                _qubits[0].UpdateDeleteQubitCommand(true);
                                _qubits[1].UpdateDeleteQubitCommand(true);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (object item in e.OldItems)
                    {
                        int oldRow = e.OldStartingIndex;
                        if (item is QubitModel)
                        {
                            _qubits.RemoveAt(oldRow);
                            for (int i = oldRow; i < _qubits.Count; i++)
                            {
                                _qubits[i].IncrementRow(-1);
                            }
                            if (_qubits.Count == 1)
                            {
                                _qubits[0].UpdateDeleteQubitCommand(false);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    _qubits[e.NewStartingIndex].Refresh();
                    break;
            }
            OnPropertyChanged("ScaleCenterY");
            OnPropertyChanged("ButtonHeight");
        }

        private void _model_StepChanged(object sender, RoutedEventArgs e)
        {
            OnPropertyChanged("AddQubitEnabled");
        }

        #endregion // Private Helpers
    }
}
