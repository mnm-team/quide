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

using QuIDE.Controls;
using QuIDE.Helpers;
using QuantumModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Technewlogic.WpfDialogManagement;
using Technewlogic.WpfDialogManagement.Contracts;

namespace QuIDE.ViewModels
{
    public class QubitVM : ViewModelBase
    {
        #region Fields

        private ComputerModel _model;

        private int _registerIndex;
        private int _rowIndex;

        private DelegateCommand _changeValue;
        private DelegateCommand _editRegister;
        private DelegateCommand _insertQubitAbove;
        private DelegateCommand _insertQubitBelow;
        private DelegateCommand _insertRegisterAbove;
        private DelegateCommand _insertRegisterBelow;
        private DelegateCommand _deleteQubit;
        private DelegateCommand _deleteRegister;

        #endregion // Fields


        #region Constructor

        public QubitVM(ComputerModel model, int registerIndex, int rowIndex)
        {
            _model = model;
            _registerIndex = registerIndex;
            _rowIndex = rowIndex;
            _model.StepChanged += _model_CurrentStepChanged;
            _deleteRegister = new DelegateCommand(DeleteRegister, x => model.Registers.Count > 1);
        }

        #endregion // Constructor


        #region Properties

        public ICommand ChangeValueCommand
        {
            get
            {
                if (_changeValue == null)
                {
                    _changeValue = new DelegateCommand(ChangeValue, x => true);
                }
                return _changeValue;
            }
        }

        public ICommand EditRegisterCommand
        {
            get
            {
                if (_editRegister == null)
                {
                    _editRegister = new DelegateCommand(EditRegister, x => true);
                }
                return _editRegister;
            }
        }

        public ICommand InsertQubitAboveCommand
        {
            get
            {
                if (_insertQubitAbove == null)
                {
                    _insertQubitAbove = new DelegateCommand(InsertQubitAbove, x => true);
                }
                return _insertQubitAbove;
            }
        }

        public ICommand InsertQubitBelowCommand
        {
            get
            {
                if (_insertQubitBelow == null)
                {
                    _insertQubitBelow = new DelegateCommand(InsertQubitBelow, x => true);
                }
                return _insertQubitBelow;
            }
        }

        public ICommand InsertRegisterAboveCommand
        {
            get
            {
                if (_insertRegisterAbove == null)
                {
                    _insertRegisterAbove = new DelegateCommand(InsertRegisterAbove, x => true);
                }
                return _insertRegisterAbove;
            }
        }

        public ICommand InsertRegisterBelowCommand
        {
            get
            {
                if (_insertRegisterBelow == null)
                {
                    _insertRegisterBelow = new DelegateCommand(InsertRegisterBelow, x => true);
                }
                return _insertRegisterBelow;
            }
        }

        public ICommand DeleteQubitCommand
        {
            get
            {
                if (_deleteQubit == null)
                {
                    _deleteQubit = new DelegateCommand(DeleteQubit, x => true);
                }
                return _deleteQubit;
            }
        }

        public ICommand DeleteRegisterCommand
        {
            get
            {
                if (_deleteRegister == null)
                {
                    _deleteRegister = new DelegateCommand(DeleteRegister, x => false);
                }
                return _deleteRegister;
            }
        }

        public QubitModel Value
        {
            get 
            {
                return _model.Registers[_registerIndex].Qubits[_rowIndex];
            }
            set
            {
                if (value == _model.Registers[_registerIndex].Qubits[_rowIndex])
                    return;

                // update model :
                _model.Registers[_registerIndex].Qubits[_rowIndex] = value;
                
                OnPropertyChanged("QubitImage");
            }

        }

        public string RegisterName
        {
            get { return _model.Registers[_registerIndex].Name; }
        }

        public int Index
        {
            get { return _rowIndex; }
        }

        public DrawingBrush QubitImage
        {
            get
            {
                if (Value == QubitModel.Zero)
                {
                    return Application.Current.FindResource("ImgQubit0") as DrawingBrush;
                }
                else if (Value == QubitModel.One)
                {
                    return Application.Current.FindResource("ImgQubit1") as DrawingBrush;
                }
                else
                {
                    return Application.Current.FindResource("ImgQubitMixed") as DrawingBrush;
                }
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _model.CurrentStep == 0;
            }
        }

        #endregion // Properties


        #region Public Methods

        public void Refresh()
        {
            OnPropertyChanged("QubitImage");
        }
        public void ChangeValue(object parameter)
        {
            QubitModel old = Value;
            if (old == QubitModel.Zero)
            {
                Value = QubitModel.One;
            }
            else
            {
                Value = QubitModel.Zero;
            }
            _model.Registers[_registerIndex].ResetQubit(_rowIndex, old);
        }

        public void InsertQubitAbove(object parameter)
        {
            _model.InsertQubitAbove(_registerIndex, _rowIndex);
        }

        public void InsertQubitBelow(object parameter)
        {
            _model.InsertQubitBelow(_registerIndex, _rowIndex);
        }

        public void EditRegister(object parameter)
        {
            MainWindow window = App.Current.MainWindow as MainWindow;
            NewRegisterInputVM vm = new NewRegisterInputVM();
            NewRegisterInput input = new NewRegisterInput(vm);
            ICustomContentDialog dialog = window.DialogManager.CreateCustomContentDialog(input, DialogMode.OkCancel);
            dialog.Ok = () =>
            {
                // to update bindings:
                input.normalize.Focus();
                if (!Validation.GetHasError(input.widthBox) &&
                !Validation.GetHasError(input.statesGrid))
                {
                    int width = (int)vm.Width;
                    Dictionary<ulong, Complex> initStates = vm.GetInitStates();
                    if (width > 0 && initStates.Count > 0)
                    {
                        int toAdd = width - _model.Registers[_registerIndex].Qubits.Count;
                        if (toAdd > 0)
                        {
                            for (int i = 0; i < toAdd; i++)
                            {
                                _model.InsertQubitAbove(_registerIndex, _model.Registers[_registerIndex].Qubits.Count - 1);
                            }
                        }
                        else if (toAdd < 0)
                        {
                            for (int i = 0; i < -toAdd; i++)
                            {
                                _model.DeleteQubit(_registerIndex, _model.Registers[_registerIndex].Qubits.Count - 1);
                            }
                        }
                        _model.Registers[_registerIndex].InitStates = initStates;
                    }
                }
            };
            dialog.Show();
        }

        public void InsertRegisterAbove(object parameter)
        {
            MainWindow window = App.Current.MainWindow as MainWindow;
            NewRegisterInputVM vm = new NewRegisterInputVM();
            NewRegisterInput input = new NewRegisterInput(vm);
            ICustomContentDialog dialog = window.DialogManager.CreateCustomContentDialog(input, DialogMode.OkCancel);
            dialog.Ok = () =>
                {
                    // to update bindings:
                    input.normalize.Focus();
                    if (!Validation.GetHasError(input.widthBox) &&
                    !Validation.GetHasError(input.statesGrid))
                    {
                        int width = (int)vm.Width;
                        Dictionary<ulong, Complex> initStates = vm.GetInitStates();
                        if (width > 0 && initStates.Count > 0)
                        {
                            _model.InsertRegisterAbove(_registerIndex, width, initStates);
                        }
                    }
                };
            dialog.Show();
        }

        public void InsertRegisterBelow(object parameter)
        {
            MainWindow window = App.Current.MainWindow as MainWindow;
            NewRegisterInputVM vm = new NewRegisterInputVM();
            NewRegisterInput input = new NewRegisterInput(vm);
            ICustomContentDialog dialog = window.DialogManager.CreateCustomContentDialog(input, DialogMode.OkCancel);
            dialog.Ok = () =>
            {
                // to update bindings:
                input.normalize.Focus();
                if (!Validation.GetHasError(input.widthBox) &&
                    !Validation.GetHasError(input.statesGrid))
                {
                    int width = (int)vm.Width;
                    Dictionary<ulong, Complex> initStates = vm.GetInitStates();
                    if (width > 0 && initStates.Count > 0)
                    {
                        _model.InsertRegisterBelow(_registerIndex, width, initStates);
                    }
                }
            };
            dialog.Show();
        }

        public void DeleteQubit(object parameter)
        {
            _model.DeleteQubit(_registerIndex, _rowIndex);
        }

        public void DeleteRegister(object parameter)
        {
            _model.DeleteRegister(_registerIndex);
        }

        public void IncrementRow(int delta = 1)
        {
            _rowIndex += delta;
            OnPropertyChanged("Index");
            OnPropertyChanged("QubitImage");
        }

        public void IncrementRegisterIndex(int delta = 1)
        {
            _registerIndex += delta;
            OnPropertyChanged("RegisterName");
        }

        public void UpdateDeleteRegisterCommand(bool canExecute)
        {
            _deleteRegister = new DelegateCommand(DeleteRegister, x => canExecute);
            OnPropertyChanged("DeleteRegisterCommand");
        }

        public void UpdateDeleteQubitCommand(bool canExecute)
        {
            _deleteQubit = new DelegateCommand(DeleteQubit, x => canExecute);
            OnPropertyChanged("DeleteQubitCommand");
        }

        #endregion // Public Methods


        #region Private Helpers

        private void _model_CurrentStepChanged(object sender, RoutedEventArgs e)
        {
            OnPropertyChanged("IsEnabled");
        }

        #endregion // Private Helpers
    }
}
