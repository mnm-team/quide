#region

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaGUI.ViewModels.Helpers;
using AvaloniaGUI.ViewModels.MainModels.QuantumModel;

#endregion

namespace AvaloniaGUI.ViewModels.Controls;

public class CircuitGridViewModel : ViewModelBase
{
    #region Events

    public event EventHandler? SelectionChanged;

    private void OnSelectionChanged()
    {
        SelectionChanged?.Invoke(this, new RoutedEventArgs());
    }

    public event EventHandler? QubitsChanged;

    private void OnQubitsChanged()
    {
        QubitsChanged?.Invoke(this, new RoutedEventArgs());
    }

    #endregion // Events


    #region Fields

    private ComputerModel _model;

    private ObservableCollection<RegisterViewModel> _registers;
    private ObservableCollection<StepViewModel> _steps;

    private int _currentStep;

    private double _scaleFactor = 0.75;
    private double _scaleFactorMax = 4.0;
    private double _scaleFactorMin = 0.1;

    private GateViewModel _selectedObject;

    #endregion // Fields


    #region Constructor

    //only for design
    public CircuitGridViewModel()
        : this(ComputerModel.CreateModelForGUI())
    {
    }

    public CircuitGridViewModel(ComputerModel model)
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

    public ObservableCollection<RegisterViewModel> Registers
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

    public ObservableCollection<StepViewModel> Steps
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

    public GateViewModel SelectedObject
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

    public static RelativePoint QubitScaleCenter
    {
        get { return new RelativePoint(0, QubitSize / 2, RelativeUnit.Absolute); }
    }

    public double ScaleCenterY
    {
        get { return _model.TotalWidth * QubitScaleCenter.Point.Y; }
    }

    public double ScaleFactor
    {
        get { return _scaleFactor; }
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
        get { return _model.CurrentStep == 0; }
    }

    public int LastStepAdded { get; private set; }

    #endregion // Presentation Properties


    #region Public Methods

    public void LayoutRoot_PreviewMouseWheel(PointerWheelEventArgs e)
    {
        //TODO: avalonia pointer events and key gestures 
        // if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
        //     Keyboard.IsKeyDown(Key.RightCtrl))
        // {
        //     double newScaleFactor = ScaleFactor;
        //     newScaleFactor += (e.Delta.Length > 0) ? 0.1 : -0.1;
        //     if (newScaleFactor >= _scaleFactorMin && newScaleFactor <= _scaleFactorMax)
        //     {
        //         ScaleFactor = newScaleFactor;
        //     }
        // }
    }

    #endregion // Public Methods


    #region Private Helpers

    private void CurrentStepChanged(object? sender, EventArgs eventArgs)
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

    void _model_SelectionChanged(object? sender, EventArgs eventArgs)
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
            if (column == selected.EndColumn &&
                _steps[column].Gates[selected.BeginRow] == _steps[column].Gates[selected.EndRow])
            {
                SelectedObject = _steps[column].Gates[selected.BeginRow];
            }
        }

        OnSelectionChanged();
    }

    private ObservableCollection<RegisterViewModel> CreateRegistersFromModel()
    {
        ObservableCollection<RegisterViewModel> registers = new ObservableCollection<RegisterViewModel>();
        for (int i = 0; i < _model.Registers.Count; i++)
        {
            RegisterViewModel reg = new RegisterViewModel(_model, i);
            reg.QubitsChanged += registers_QubitsChanged;
            registers.Add(reg);
        }

        return registers;
    }

    private void registers_QubitsChanged(object? sender, EventArgs eventArgs)
    {
        OnQubitsChanged();
    }

    private ObservableCollection<StepViewModel> CreateStepsFromModel()
    {
        ObservableCollection<StepViewModel> steps = new ObservableCollection<StepViewModel>();
        for (int i = 0; i < _model.Steps.Count; i++)
        {
            steps.Add(new StepViewModel(_model, i));
        }

        steps[_model.CurrentStep].SetAsCurrent();
        return steps;
    }

    private void Steps_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        StepModel step;
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                step = e.NewItems[0] as StepModel;
                int newColumn = e.NewStartingIndex;
                if (step != null)
                {
                    Steps.Insert(newColumn, new StepViewModel(_model, newColumn));
                    for (int i = newColumn + 1; i < _steps.Count; i++)
                    {
                        _steps[i].IncrementColumn();
                    }

                    LastStepAdded = newColumn;
                    if (_steps.Count == 2)
                    {
                        foreach (GateViewModel gate in _steps[0].Gates)
                        {
                            gate.UpdateDeleteColumnCommand(true);
                        }

                        foreach (GateViewModel gate in _steps[1].Gates)
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
                        foreach (GateViewModel gate in _steps[0].Gates)
                        {
                            gate.UpdateDeleteColumnCommand(false);
                        }
                    }
                }

                break;
        }
    }

    private void Registers_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (object item in e.NewItems)
                {
                    int newRow = e.NewStartingIndex;
                    if (item is RegisterModel)
                    {
                        RegisterViewModel reg = new RegisterViewModel(_model, newRow);
                        reg.QubitsChanged += registers_QubitsChanged;
                        Registers.Insert(newRow, reg);
                        for (int i = newRow + 1; i < _registers.Count; i++)
                        {
                            _registers[i].IncrementIndex();
                        }

                        if (_registers.Count == 2)
                        {
                            foreach (QubitViewModel qubit in _registers[0].Qubits)
                            {
                                qubit.UpdateDeleteRegisterCommand(true);
                            }

                            foreach (QubitViewModel qubit in _registers[1].Qubits)
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
                            foreach (QubitViewModel qubit in _registers[0].Qubits)
                            {
                                qubit.UpdateDeleteRegisterCommand(false);
                            }
                        }

                        foreach (StepViewModel step in _steps)
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
        OnSelectionChanged();
    }

    #endregion // Private Helpers
}