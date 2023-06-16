#region

using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaGUI.CodeHelpers;
using AvaloniaGUI.ViewModels.MainModels.QuantumModel;

#endregion

namespace AvaloniaGUI.ViewModels.Helpers;

public class QubitViewModel : ViewModelBase
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

    public QubitViewModel(ComputerModel model, int registerIndex, int rowIndex)
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
        get => _model.Registers[_registerIndex].Qubits[_rowIndex];
        set
        {
            if (value == _model.Registers[_registerIndex].Qubits[_rowIndex])
                return;

            // update model :
            _model.Registers[_registerIndex].Qubits[_rowIndex] = value;

            OnPropertyChanged(nameof(QubitImage));
        }
    }

    public string RegisterName => _model.Registers[_registerIndex].Name;

    public int Index => _rowIndex;

    public VisualBrush QubitImage
    {
        get
        {
            if (Value == QubitModel.Zero)
            {
                return Application.Current.FindResource("ImgQubit0") as VisualBrush;
            }
            else if (Value == QubitModel.One)
            {
                return Application.Current.FindResource("ImgQubit1") as VisualBrush;
            }
            else
            {
                return Application.Current.FindResource("ImgQubitMixed") as VisualBrush;
            }
        }
    }

    public bool IsEnabled
    {
        get { return _model.CurrentStep == 0; }
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
        _model.Registers[_registerIndex].ResetQubit(_rowIndex, old);
        if (old == QubitModel.Zero)
        {
            Value = QubitModel.One;
        }
        else
        {
            Value = QubitModel.Zero;
        }
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
        // TODO: how to use DialogManager here
        // MainWindow window = App.Current.MainWindow as MainWindow;
        // NewRegisterInputViewModel vm = new NewRegisterInputViewModel();
        // NewRegisterInput input = new NewRegisterInput(vm);
        // ICustomContentDialog dialog = window.DialogManager.CreateCustomContentDialog(input, DialogMode.OkCancel);
        // dialog.Ok = () =>
        // {
        //     // to update bindings:
        //     input.normalize.Focus();
        //     if (!Validation.GetHasError(input.widthBox) &&
        //         !Validation.GetHasError(input.statesGrid))
        //     {
        //         int width = (int)vm.Width;
        //         Dictionary<ulong, Complex> initStates = vm.GetInitStates();
        //         if (width > 0 && initStates.Count > 0)
        //         {
        //             int toAdd = width - _model.Registers[_registerIndex].Qubits.Count;
        //             if (toAdd > 0)
        //             {
        //                 for (int i = 0; i < toAdd; i++)
        //                 {
        //                     _model.InsertQubitAbove(_registerIndex, _model.Registers[_registerIndex].Qubits.Count - 1);
        //                 }
        //             }
        //             else if (toAdd < 0)
        //             {
        //                 for (int i = 0; i < -toAdd; i++)
        //                 {
        //                     _model.DeleteQubit(_registerIndex, _model.Registers[_registerIndex].Qubits.Count - 1);
        //                 }
        //             }
        //
        //             _model.Registers[_registerIndex].InitStates = initStates;
        //         }
        //     }
        // };
        // dialog.Show();
    }

    public void InsertRegisterAbove(object parameter)
    {
        // MainWindow window = App.Current.MainWindow as MainWindow;
        // NewRegisterInputViewModel vm = new NewRegisterInputViewModel();
        // NewRegisterInput input = new NewRegisterInput(vm);
        // ICustomContentDialog dialog = window.DialogManager.CreateCustomContentDialog(input, DialogMode.OkCancel);
        // dialog.Ok = () =>
        // {
        //     // to update bindings:
        //     input.normalize.Focus();
        //     if (!Validation.GetHasError(input.widthBox) &&
        //         !Validation.GetHasError(input.statesGrid))
        //     {
        //         int width = (int)vm.Width;
        //         Dictionary<ulong, Complex> initStates = vm.GetInitStates();
        //         if (width > 0 && initStates.Count > 0)
        //         {
        //             _model.InsertRegisterAbove(_registerIndex, width, initStates);
        //         }
        //     }
        // };
        // dialog.Show();
    }

    public void InsertRegisterBelow(object parameter)
    {
        // MainWindow window = App.Current.MainWindow as MainWindow;
        // NewRegisterInputViewModel vm = new NewRegisterInputViewModel();
        // NewRegisterInput input = new NewRegisterInput(vm);
        // ICustomContentDialog dialog = window.DialogManager.CreateCustomContentDialog(input, DialogMode.OkCancel);
        // dialog.Ok = () =>
        // {
        //     // to update bindings:
        //     input.normalize.Focus();
        //     if (!Validation.GetHasError(input.widthBox) &&
        //         !Validation.GetHasError(input.statesGrid))
        //     {
        //         int width = (int)vm.Width;
        //         Dictionary<ulong, Complex> initStates = vm.GetInitStates();
        //         if (width > 0 && initStates.Count > 0)
        //         {
        //             _model.InsertRegisterBelow(_registerIndex, width, initStates);
        //         }
        //     }
        // };
        // dialog.Show();
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

    private void _model_CurrentStepChanged(object? sender, EventArgs eventArgs)
    {
        OnPropertyChanged("IsEnabled");
    }

    #endregion // Private Helpers
}