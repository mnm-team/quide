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
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Specialized;
using QuantumModel;

namespace QuIDE.ViewModels
{
    public class StepVM : ViewModelBase
    {
        #region Fields

        private ObservableCollection<GateVM> _gates;

        private ComputerModel _model;

        private int _column;

        private Thickness _stepMargin;

        private static readonly Thickness currentMargin = new Thickness(3, 0, 0, 0);
        private static readonly Thickness noMargin = new Thickness(0, 0, 0, 0);

        #endregion // Fields


        #region Constructor

        public StepVM(ComputerModel model, int column)
        {
            _model = model;
            _column = column;
            _model.Steps[_column].Gates.CollectionChanged += Gates_CollectionChanged;
            _gates = CreateGatesFromModel();
        }

        #endregion // Constructor


        #region Presentation Properties

        public ObservableCollection<GateVM> Gates
        {
            get
            {
                if (_gates == null)
                {
                    _gates = CreateGatesFromModel();
                }
                return _gates;
            }
        }

        public double ScaleCenterY
        {
            get
            {
                return Gates.Count * CircuitGridVM.QubitScaleCenter;
            }
        }

        public Thickness StepMargin
        {
            get
            {
                if (_stepMargin == null)
                {
                    if (_model.CurrentStep == _column)
                    {
                        return currentMargin;
                    }
                    return noMargin;
                }
                return _stepMargin;
            }
            set
            {
                _stepMargin = value;
                OnPropertyChanged("StepMargin");
            }
        }

        #endregion // Presentation Properties


        #region Public Methids

        public void SetAsCurrent()
        {
            StepMargin = currentMargin;
        }

        public void UnsetAsCurrent()
        {
            StepMargin = noMargin;
        }

        public void IncrementColumn(int columnDelta = 1)
        {
            _column += columnDelta;
            foreach (GateVM g in _gates)
            {
                g.ChangeColumn(columnDelta);
            }
            if (_model.CurrentStep == _column)
            {
                SetAsCurrent();
            }
            else
            {
                UnsetAsCurrent();
            }
        }

        #endregion // Public Methods


        #region Private Helpers

        private ObservableCollection<GateVM> CreateGatesFromModel()
        {
            ObservableCollection<GateVM> gates = new ObservableCollection<GateVM>();
            for (int i = 0; i < _model.Steps[_column].Gates.Count; i++)
            {
                Gate gate = _model.Steps[_column].Gates[i];
                RegisterRefModel gateRow = _model.GetRefFromOffset(i);
                gates.Add(new GateVM(_model, gateRow, _column));
            }
            return gates;
        }

        private void Gates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Gate gate;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    int addIndex = e.NewStartingIndex;
                    foreach (object item in e.NewItems)
                    {
                        gate = item as Gate;
                        if (gate != null)
                        {
                            RegisterRefModel gateRow = _model.GetRefFromOffset(addIndex);
                            Gates.Insert(addIndex, new GateVM(_model, gateRow, _column));
                            for (int i = addIndex + 1; i < _gates.Count; i++)
                            {
                                _gates[i].UpdateRow(i);
                            }
                            if (_gates.Count == 2)
                            {
                                _gates[0].UpdateDeleteRowCommand(true);
                                _gates[1].UpdateDeleteRowCommand(true);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (object item in e.NewItems)
                    {
                        gate = item as Gate;
                        for (int i = gate.Begin; i <= gate.End; i++)
                        {
                            Gates[i].Refresh();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    int oldRow = e.OldStartingIndex;
                    foreach (object item in e.OldItems)
                    {
                        if (item is Gate)
                        {
                            Gates.RemoveAt(oldRow);
                            for (int i = oldRow; i < _gates.Count; i++)
                            {
                                _gates[i].UpdateRow(i);
                            }
                            if (_gates.Count == 1)
                            {
                                _gates[0].UpdateDeleteRowCommand(false);
                            }
                        }
                    }
                    break;
            }
            OnPropertyChanged("ScaleCenterY");
        }

        #endregion // Private Helpers
    }
}
