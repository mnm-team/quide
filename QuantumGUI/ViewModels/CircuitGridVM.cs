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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.Specialized;
using QuIDE.Controls;
using QuIDE.Helpers;
using QuantumModel;

namespace QuIDE.ViewModels
{

    public class CircuitGridVM : ViewModelBase
    {
        #region Events

        public event RoutedEventHandler SelectionChanged;
        private void OnSelectionChanged()
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new RoutedEventArgs());
            }
        }

        #endregion // Events


        #region Fields

        private ComputerModel _model;

        private ObservableCollection<RegisterVM> _registers;
        private ObservableCollection<StepVM> _steps;

        private int _currentStep;

        private double _scaleFactor = 0.75;
        private double _scaleFactorMax = 4.0;
        private double _scaleFactorMin = 0.1;

        private GateVM _selectedObject;

        #endregion // Fields


        #region Constructor

        //only for design
        public CircuitGridVM()
            : this(ComputerModel.CreateModelForGUI())
        {
        }

        public CircuitGridVM(ComputerModel model)
        {
            _model = model;
            _registers = CreateRegistersFromModel();
            _steps = CreateStepsFromModel();

            _model.Steps.CollectionChanged += Steps_CollectionChanged;
            _model.Registers.CollectionChanged += Registers_CollectionChanged;
            
            _model.StepChanged += CurrentStepChanged;
            _model.SelectionChanged += _model_SelectionChanged;
        }

        #endregion // Constructor


        #region Model Properties

        public ObservableCollection<RegisterVM> Registers
        {
            get
            {
                if (_registers == null)
                {
                    _registers = CreateRegistersFromModel();
                }
                return _registers;
            }
        }

        public ObservableCollection<StepVM> Steps
        {
            get
            {
                if (_steps == null)
                {
                    _steps = CreateStepsFromModel();
                }
                return _steps;
            }
        }

        public GateVM SelectedObject
        {
            get { return _selectedObject; }
            set
            {
                _selectedObject = value;
                OnSelectionChanged();
            }
        }

        #endregion // Model Properties


        #region Presentation Properties

        public static double GateWidth
        {
            get { return 96; }
        }

        public static double GateHeight
        {
            get { return 40; }
        }

        public static double GateTextTranslate
        {
            get { return (GateWidth - GateHeight) / 2; }
        }

        public static double GateTextCanvasTop
        {
            get { return (QubitSize - GateHeight) / 2; }
        }

        public static double QubitSize
        {
            get { return 64; }
        }

        public static double QubitScaleCenter
        {
            get { return QubitSize / 2; }
        }

        public double ScaleCenterY
        {
            get
            {
                return _model.TotalWidth * QubitScaleCenter;
            }
        }

        public double ScaleFactor
        {
            get
            {
                return _scaleFactor;
            }
            private set
            {
                if (value != _scaleFactor)
                {
                    _scaleFactor = value;
                    OnPropertyChanged("ScaleFactor");
                }
            }
        }

        public bool AddRegisterEnabled
        {
            get
            {
                return _model.CurrentStep == 0;
            }
        }

        public int LastStepAdded
        {
            get;
            private set;
        }

        #endregion // Presentation Properties


        #region Public Methods

        public void LayoutRoot_PreviewMouseWheel(MouseWheelEventArgs e)
        {
            
            if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl))
            {
                double newScaleFactor = ScaleFactor;
                newScaleFactor += (e.Delta > 0) ? 0.1 : -0.1;
                if (newScaleFactor >= _scaleFactorMin && newScaleFactor <= _scaleFactorMax)
                {
                    ScaleFactor = newScaleFactor;
                }
            }
        }

        #endregion // Public Methods


        #region Private Helpers

        private void CurrentStepChanged(object sender, RoutedEventArgs e)
        {
            if (_currentStep < _steps.Count)
            {
                _steps[_currentStep].UnsetAsCurrent();
            }
            _currentStep = _model.CurrentStep;
            if (_currentStep < _steps.Count)
            {
                _steps[_currentStep].SetAsCurrent();
            }
            OnPropertyChanged("AddQubitEnabled");
        }

        void _model_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (_model.UnselectedItems.HasValue)
            {
                Selection unselected = _model.UnselectedItems.Value;
                for (int i = unselected.BeginColumn; i <= unselected.EndColumn; i++)
                {
                    for (int j = unselected.BeginRow; j <= unselected.EndRow; j++)
                    {
                        _steps[i].Gates[j].RefreshSelection(false);
                    }
                }
            }
            if (_model.SelectedItems.HasValue)
            {
                Selection selected = _model.SelectedItems.Value;
                for (int i = selected.BeginColumn; i <= selected.EndColumn; i++)
                {
                    for (int j = selected.BeginRow; j <= selected.EndRow; j++)
                    {
                        _steps[i].Gates[j].RefreshSelection(true);
                    }
                }
                int column = selected.BeginColumn;
                if(column == selected.EndColumn &&
                    _steps[column].Gates[selected.BeginRow] == _steps[column].Gates[selected.EndRow])
                {
                    SelectedObject = _steps[column].Gates[selected.BeginRow];
                }
            }
            OnSelectionChanged();
        }

        private ObservableCollection<RegisterVM> CreateRegistersFromModel()
        {
            ObservableCollection<RegisterVM> registers = new ObservableCollection<RegisterVM>();
            for (int i = 0; i < _model.Registers.Count; i++)
            {
                registers.Add(new RegisterVM(_model, i));
            }
            return registers;
        }

        private ObservableCollection<StepVM> CreateStepsFromModel()
        {
            ObservableCollection<StepVM> steps = new ObservableCollection<StepVM>();
            for (int i = 0; i < _model.Steps.Count; i++)
            {
                steps.Add(new StepVM(_model, i));
            }
            steps[_model.CurrentStep].SetAsCurrent();
            return steps;
        }

        private void Steps_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            StepModel step;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    step = e.NewItems[0] as StepModel;
                    int newColumn = e.NewStartingIndex;
                    if (step != null)
                    {
                        Steps.Insert(newColumn, new StepVM(_model, newColumn));
                        for (int i = newColumn + 1; i < _steps.Count; i++)
                        {
                            _steps[i].IncrementColumn();
                        }
                        LastStepAdded = newColumn;
                        if (_steps.Count == 2)
                        {
                            foreach (GateVM gate in _steps[0].Gates)
                            {
                                gate.UpdateDeleteColumnCommand(true);
                            }
                            foreach (GateVM gate in _steps[1].Gates)
                            {
                                gate.UpdateDeleteColumnCommand(true);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    int oldColumn = e.OldStartingIndex;
                    step = e.OldItems[0] as StepModel;
                    if (step != null)
                    {
                        Steps.RemoveAt(oldColumn);
                        for (int i = oldColumn; i < _steps.Count; i++)
                        {
                            _steps[i].IncrementColumn(-1);
                        }
                        if (_steps.Count == 1)
                        {
                            foreach (GateVM gate in _steps[0].Gates)
                            {
                                gate.UpdateDeleteColumnCommand(false);
                            }
                        }
                    }
                    break;
            }
        }

        private void Registers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RegisterModel reg;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object item in e.NewItems)
                    {
                        int newRow = e.NewStartingIndex;
                        if (item is RegisterModel)
                        {
                            reg = (RegisterModel)item;
                            Registers.Insert(newRow, new RegisterVM(_model, newRow));
                            for (int i = newRow + 1; i < _registers.Count; i++)
                            {
                                _registers[i].IncrementIndex();
                            }
                            if (_registers.Count == 2)
                            {
                                foreach (QubitVM qubit in _registers[0].Qubits)
                                {
                                    qubit.UpdateDeleteRegisterCommand(true);
                                }
                                foreach (QubitVM qubit in _registers[1].Qubits)
                                {
                                    qubit.UpdateDeleteRegisterCommand(true);
                                }
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (object item in e.OldItems)
                    {
                        int oldRow = e.OldStartingIndex;
                        if (item is RegisterModel)
                        {
                            Registers.RemoveAt(oldRow);
                            for (int i = oldRow; i < _registers.Count; i++)
                            {
                                _registers[i].IncrementIndex(-1);
                            }
                            if (_registers.Count == 1)
                            {
                                foreach (QubitVM qubit in _registers[0].Qubits)
                                {
                                    qubit.UpdateDeleteRegisterCommand(false);
                                }
                            }
                            foreach (StepVM step in _steps)
                            {
                                for (int i = 0; i < step.Gates.Count; i++)
                                {
                                    step.Gates[i].UpdateRow(i);
                                }
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
