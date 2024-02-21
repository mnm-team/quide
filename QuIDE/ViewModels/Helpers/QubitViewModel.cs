#region

using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using QuIDE.CodeHelpers;
using QuIDE.QuantumModel;
using QuIDE.ViewModels.Dialog;
using QuIDE.Views.Dialog;

#endregion

namespace QuIDE.ViewModels.Helpers;

public class QubitViewModel : ViewModelBase
{
    public QubitViewModel(ComputerModel model, int registerIndex, int rowIndex, DialogManager dialogManager)
    {
        _model = model;
        _registerIndex = registerIndex;
        Index = rowIndex;
        _model.StepChanged += _model_CurrentStepChanged;
        _deleteRegister = new DelegateCommand(DeleteRegister, x => model.Registers.Count > 1);

        _dialogManager = dialogManager;
    }


    private void _model_CurrentStepChanged(object sender, EventArgs eventArgs)
    {
        OnPropertyChanged(nameof(IsEnabled));
    }

    private readonly ComputerModel _model;

    private int _registerIndex;

    private DelegateCommand _changeValue;
    private DelegateCommand _editRegister;
    private DelegateCommand _insertQubitAbove;
    private DelegateCommand _insertQubitBelow;
    private DelegateCommand _insertRegisterAbove;
    private DelegateCommand _insertRegisterBelow;
    private DelegateCommand _deleteQubit;
    private DelegateCommand _deleteRegister;

    private readonly DialogManager _dialogManager;


    public ICommand ChangeValueCommand
    {
        get
        {
            if (_changeValue == null) _changeValue = new DelegateCommand(ChangeValue, x => true);

            return _changeValue;
        }
    }

    public ICommand EditRegisterCommand
    {
        get
        {
            if (_editRegister == null) _editRegister = new DelegateCommand(EditRegister, x => true);

            return _editRegister;
        }
    }

    public ICommand InsertQubitAboveCommand
    {
        get
        {
            if (_insertQubitAbove == null) _insertQubitAbove = new DelegateCommand(InsertQubitAbove, x => true);

            return _insertQubitAbove;
        }
    }

    public ICommand InsertQubitBelowCommand
    {
        get
        {
            if (_insertQubitBelow == null) _insertQubitBelow = new DelegateCommand(InsertQubitBelow, x => true);

            return _insertQubitBelow;
        }
    }

    public ICommand InsertRegisterAboveCommand
    {
        get
        {
            if (_insertRegisterAbove == null)
                _insertRegisterAbove = new DelegateCommand(InsertRegisterAbove, x => true);

            return _insertRegisterAbove;
        }
    }

    public ICommand InsertRegisterBelowCommand
    {
        get
        {
            if (_insertRegisterBelow == null)
                _insertRegisterBelow = new DelegateCommand(InsertRegisterBelow, x => true);

            return _insertRegisterBelow;
        }
    }

    public ICommand DeleteQubitCommand
    {
        get
        {
            if (_deleteQubit == null) _deleteQubit = new DelegateCommand(DeleteQubit, x => true);

            return _deleteQubit;
        }
    }

    public ICommand DeleteRegisterCommand
    {
        get
        {
            if (_deleteRegister == null) _deleteRegister = new DelegateCommand(DeleteRegister, x => false);

            return _deleteRegister;
        }
    }

    public QubitModel Value
    {
        get => _model.Registers[_registerIndex].Qubits[Index];
        set
        {
            if (value == _model.Registers[_registerIndex].Qubits[Index])
                return;

            // update model :
            _model.Registers[_registerIndex].Qubits[Index] = value;

            OnPropertyChanged(nameof(QubitImage));
        }
    }

    public string RegisterName => _model.Registers[_registerIndex].Name;

    public int Index { get; private set; }

    public VisualBrush QubitImage
    {
        get
        {
            return Value switch
            {
                QubitModel.Zero => Application.Current.FindResource("ImgQubit0") as VisualBrush,
                QubitModel.One => Application.Current.FindResource("ImgQubit1") as VisualBrush,
                _ => Application.Current.FindResource("ImgQubitMixed") as VisualBrush
            };
        }
    }

    public bool IsEnabled => _model.CurrentStep == 0;


    public void Refresh()
    {
        OnPropertyChanged(nameof(QubitImage));
    }

    private void ChangeValue(object parameter)
    {
        var old = Value;
        _model.Registers[_registerIndex].ResetQubit(Index, old);
        Value = old == QubitModel.Zero ? QubitModel.One : QubitModel.Zero;
    }

    private void InsertQubitAbove(object parameter)
    {
        _model.InsertQubitAbove(_registerIndex, Index);
    }

    private void InsertQubitBelow(object parameter)
    {
        _model.InsertQubitBelow(_registerIndex, Index);
    }

    private async void EditRegister(object parameter)
    {
        var vm = new NewRegisterInputViewModel();
        var input = new NewRegisterInput(vm);
        await _dialogManager.ShowDialogAsync(input, () =>
        {
            var width = (int)vm.Width;
            var initStates = vm.GetInitStates();
            if (width <= 0 || initStates.Count <= 0) return;

            var toAdd = width - _model.Registers[_registerIndex].Qubits.Count;
            switch (toAdd)
            {
                case > 0:
                {
                    for (var i = 0; i < toAdd; i++)
                        _model.InsertQubitAbove(_registerIndex, _model.Registers[_registerIndex].Qubits.Count - 1);
                    break;
                }
                case < 0:
                {
                    for (var i = 0; i < -toAdd; i++)
                        _model.DeleteQubit(_registerIndex, _model.Registers[_registerIndex].Qubits.Count - 1);
                    break;
                }
            }

            _model.Registers[_registerIndex].InitStates = initStates;
        });
    }

    private async void InsertRegisterAbove(object parameter)
    {
        var vm = new NewRegisterInputViewModel();
        var input = new NewRegisterInput(vm);
        await _dialogManager.ShowDialogAsync(input, () =>
        {
            var width = (int)vm.Width;
            var initStates = vm.GetInitStates();
            if (width > 0 && initStates.Count > 0) _model.InsertRegisterAbove(_registerIndex, width, initStates);
        });
    }

    private async void InsertRegisterBelow(object parameter)
    {
        var vm = new NewRegisterInputViewModel();
        var input = new NewRegisterInput(vm);
        await _dialogManager.ShowDialogAsync(input, () =>
        {
            var width = (int)vm.Width;
            var initStates = vm.GetInitStates();
            if (width > 0 && initStates.Count > 0) _model.InsertRegisterBelow(_registerIndex, width, initStates);
        });
    }

    private void DeleteQubit(object parameter)
    {
        _model.DeleteQubit(_registerIndex, Index);
    }

    private void DeleteRegister(object parameter)
    {
        _model.DeleteRegister(_registerIndex);
    }

    public void IncrementRow(int delta = 1)
    {
        Index += delta;
        OnPropertyChanged(nameof(Index));
        OnPropertyChanged(nameof(QubitImage));
    }

    public void IncrementRegisterIndex(int delta = 1)
    {
        _registerIndex += delta;
        OnPropertyChanged(nameof(RegisterName));
    }

    public void UpdateDeleteRegisterCommand(bool canExecute)
    {
        _deleteRegister = new DelegateCommand(DeleteRegister, x => canExecute);
        OnPropertyChanged(nameof(DeleteRegisterCommand));
    }

    public void UpdateDeleteQubitCommand(bool canExecute)
    {
        _deleteQubit = new DelegateCommand(DeleteQubit, x => canExecute);
        OnPropertyChanged(nameof(DeleteQubitCommand));
    }
}