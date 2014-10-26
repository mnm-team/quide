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
using System.Linq;
using System.Collections.Generic;
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
using QuantumParser;

namespace QuIDE.ViewModels
{
    public enum SortField
    {
        Value,
        Probability
    }

    public class OutputGridVM : ViewModelBase
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
        private Output _outputModel;

        private StateVM[] _states;

        private string[] _registersNames;
        private ParameterVM _selectedRegister;

        private int _selectedIndex;
        //private StateVM _selectedItem;

        private SortField _sortBy = SortField.Value;
        private bool _sortDesc = false;

        private double _maxProbability;

        private bool _showAll;
        private bool _scaleRelative;

        #endregion // Fields


        #region Constructor

        // only for design
        public OutputGridVM()
        {
            _selectedRegister = new ParameterVM("register", typeof(QuantumParser.Register));
        }

        //public OutputGridVM(Output model)
        //{
        //    _model = model;
        //    _model.OutputChanged += _model_OutputChanged;
        //}

        #endregion // Constructor


        #region Model Properties

        public StateVM[] States
        {
            get
            {
                if (_states == null)
                {
                    _states = CreateStatesFromModel();
                }
                return _states;
            }
        }

        public OutputState SelectedObject
        {
            get
            {
                if (_selectedIndex >= 0 && _selectedIndex < States.Length)
                {
                    return States[_selectedIndex].Model;
                }
                //if (_selectedItem != null)
                //{
                //    return _selectedItem.Model;
                //}
                return null;
            }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                OnPropertyChanged("SelectedIndex");
                OnSelectionChanged();
            }
        }

        //public StateVM SelectedItem
        //{
        //    get { return _selectedItem; }
        //    set
        //    {
        //        _selectedItem = value;
        //        for (int i = 0; i < _states.Length; i++)
        //        {
        //            if (_selectedItem == _states[i])
        //            {
        //                _selectedIndex = i;
        //                break;
        //            }
        //        }
        //        OnSelectionChanged();
        //    }
        //}

        #endregion // Model Properties


        #region Presentation Properties

        public string[] RegistersNames
        {
            get
            {
                if (_registersNames == null)
                {
                    _registersNames = new string[] { "root" };
                }
                return _registersNames;
            }
            set
            {
                _registersNames = value;
                OnPropertyChanged("RegistersNames");
            }
        }

        public bool ShowAll
        {
            get { return _showAll; }
            set
            {
                if (_showAll != value)
                {
                    _showAll = value;
                    OnPropertyChanged("ShowAll");
                    int prevIndex = SelectedIndex;
                    _states = CreateStates();
                    _states = ScaleProbability(_states);
                    OnPropertyChanged("States");
                    SelectedIndex = prevIndex;
                }
            }
        }

        public bool ScaleRelative
        {
            get { return _scaleRelative; }
            set
            {
                if (_scaleRelative != value)
                {
                    _scaleRelative = value;
                    OnPropertyChanged("ScaleRelative");
                    _states = ScaleProbability(_states);
                }
            }
        }

        #endregion // Presentation Properties


        #region Public Methods

        public void LoadModel(ComputerModel model, Output outputModel)
        {
            _model = model;
            _outputModel = outputModel;
            _outputModel.OutputChanged += _model_OutputChanged;
            
            string[] regNames = new string[_model.Registers.Count + 1];
            regNames[0] = "root";
            for (int i = 1; i < regNames.Length; i++)
            {
                regNames[i] = _model.Registers[i - 1].Name;
            }
            RegistersNames = regNames;
        }

        public void SetRegister(string value)
        {
            if (!value.Equals(_selectedRegister.ValueString))
            {
                _selectedRegister.ValueString = value;
                if (_selectedRegister.IsValid)
                {
                    try
                    {
                        CircuitEvaluator eval = CircuitEvaluator.GetInstance();
                        Register selected = _selectedRegister.Value as Register;
                        _outputModel.Update(eval.RootRegister, selected.ToPartModel());
                        OnSelectionChanged();
                    }
                    catch (Exception e)
                    {
                        PrintException(e);
                    }
                }
                else
                {
                    throw new Exception(_selectedRegister.ValidationMessage);
                }
            }
        }

        public void Sort(SortField field)
        {
            int prevIndex = SelectedIndex;

            if (field != _sortBy)
            {
                ulong selectedState = States[prevIndex].Value;

                _sortBy = field;
                _sortDesc = false;

                switch (_sortBy)
                {
                    case SortField.Probability:
                        if (_sortDesc)
                        {
                            _states = States.OrderByDescending(x => x.Probability).ToArray();
                        }
                        else
                        {
                            _states = States.OrderBy(x => x.Probability).ToArray();
                        }
                        break;
                    case SortField.Value:
                    default:
                        if (_sortDesc)
                        {
                            _states = States.OrderByDescending(x => x.Value).ToArray();
                        }
                        else
                        {
                            _states = States.OrderBy(x => x.Value).ToArray();
                        }
                        break;
                }
                OnPropertyChanged("States");

                for (int i = 0; i < _states.Length; i++)
                {
                    if (_states[i].Value == selectedState)
                    {
                        SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                _sortDesc = !_sortDesc;
                _states = States.Reverse().ToArray();
                OnPropertyChanged("States");
                SelectedIndex = States.Length - 1 - prevIndex;
            }
        }

        #endregion // Public Methods


        #region Private Helpers

        private void _model_OutputChanged(object sender, RoutedEventArgs e)
        {
            string[] regNames = new string[_model.Registers.Count + 1];
            regNames[0] = "root";
            for (int i = 1; i < regNames.Length; i++)
            {
                regNames[i] = _model.Registers[i - 1].Name;
            }
            RegistersNames = regNames;

            int prevIndex = SelectedIndex;
            _states = CreateStatesFromModel();
            OnPropertyChanged("States");
            SelectedIndex = prevIndex;
        }
        
        private StateVM[] CreateStatesFromModel()
        {
            SetMaxProbability();
            StateVM[] states = CreateStates();
            return ScaleProbability(states);
        }

        private void SetMaxProbability()
        {
            _maxProbability = _outputModel.States.Max<OutputState>(x => x.Probability);
        }

        private StateVM[] CreateStates()
        {
            try
            {
                if (_showAll)
                {
                    int width = _outputModel.Width;
                    int statesCount = (int)Math.Pow(2, width);
                    StateVM[] states = new StateVM[statesCount];

                    foreach (OutputState state in _outputModel.States)
                    {
                        states[state.Value] = new StateVM(state);
                    }

                    bool amplitudeHasValue = _outputModel.States.First().Amplitude.HasValue;

                    for (int i = 0; i < statesCount; i++)
                    {
                        if (states[i] == null)
                        {
                            if (amplitudeHasValue)
                            {
                                states[i] = new StateVM(new OutputState((ulong)i, Complex.Zero, width));
                            }
                            else
                            {
                                states[i] = new StateVM(new OutputState((ulong)i, 0.0, width));
                            }
                        }
                    }

                    switch (_sortBy)
                    {
                        case SortField.Probability:
                            if (_sortDesc)
                            {
                                return states.OrderByDescending(x => x.Probability).ToArray();
                            }
                            return states.OrderBy(x => x.Probability).ToArray();
                        case SortField.Value:
                        default:
                            if (_sortDesc)
                            {
                                return states.Reverse().ToArray();
                            }
                            return states;
                    }
                }
                else
                {
                    int statesCount = _outputModel.States.Count;
                    StateVM[] states = new StateVM[statesCount];
                    int i = 0;

                    IEnumerable<OutputState> sorted = _outputModel.States;
                    switch (_sortBy)
                    {
                        case SortField.Probability:
                            if (_sortDesc)
                            {
                                sorted = _outputModel.States.OrderByDescending(x => x.Probability);
                            }
                            else
                            {
                                sorted = _outputModel.States.OrderBy(x => x.Probability);
                            }
                            break;
                        case SortField.Value:
                        default:
                            if (_sortDesc)
                            {
                                sorted = _outputModel.States.OrderByDescending(x => x.Value);
                            }
                            else
                            {
                                sorted = _outputModel.States.OrderBy(x => x.Value);
                            }
                            break;
                    }

                    foreach (OutputState state in sorted)
                    {
                        states[i] = new StateVM(state);
                        i++;
                    }
                    return states;
                }
            }
            catch (Exception e)
            {
                PrintException(e);
            }
            return new StateVM[] { new StateVM(new OutputState(0, Complex.Zero, _outputModel.Width)) };
        }

        private StateVM[] ScaleProbability(StateVM[] states)
        {
            for (int i = 0; i < states.Length; i++)
            {
                if (_scaleRelative)
                {
                    states[i].RelativeProbability = states[i].Probability / _maxProbability;
                }
                else
                {
                    states[i].RelativeProbability = states[i].Probability;
                }
            }
            return states;
        }

        private void PrintException(Exception e)
        {
            string message = e.Message;
            if (e.InnerException != null)
            {
                message = message + ":\n" + e.InnerException.Message;
            }
            MessageBox.Show(message);
        }

        #endregion // Private Helpers
    }
}
