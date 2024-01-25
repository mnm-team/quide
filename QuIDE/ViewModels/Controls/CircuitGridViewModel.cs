#region

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using QuIDE.CodeHelpers;
using QuIDE.ViewModels.Helpers;
using QuIDE.ViewModels.MainModels.QuantumModel;

#endregion

namespace QuIDE.ViewModels.Controls;

public class CircuitGridViewModel : ViewModelBase
{
    #region Public Methods

    public void LayoutRoot_PreviewMouseWheel(PointerWheelEventArgs e)
    {
        if (e.KeyModifiers != KeyModifiers.Control) return;

        var newScaleFactor = ScaleFactor;
        newScaleFactor += e.Delta.Y > 0 ? 0.1 : -0.1;
        if (newScaleFactor >= _scaleFactorMin && newScaleFactor <= _scaleFactorMax) ScaleFactor = newScaleFactor;
    }

    #endregion // Public Methods

    #region Events

    public event EventHandler SelectionChanged;

    private void OnSelectionChanged()
    {
        SelectionChanged?.Invoke(this, new RoutedEventArgs());
    }

    public event EventHandler QubitsChanged;

    private void OnQubitsChanged()
    {
        QubitsChanged?.Invoke(this, new RoutedEventArgs());
    }

    #endregion // Events


    #region Fields

    private readonly ComputerModel _model;

    private ObservableCollection<RegisterViewModel> _registers;
    private ObservableCollection<StepViewModel> _steps;

    private int _currentStep;

    private double _scaleFactor = 0.75;
    private readonly double _scaleFactorMax = 4.0;
    private readonly double _scaleFactorMin = 0.1;

    private GateViewModel _selectedObject;

    private readonly DialogManager _dialogManager;

    #endregion // Fields


    #region Constructor

    //only for Avalonia designer
    public CircuitGridViewModel()
    {
    }

    public CircuitGridViewModel(ComputerModel model, DialogManager dialogManager)
    {
        _model = model;
        _dialogManager = dialogManager;

        // they need dialogManager
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
            if (_registers == null) _registers = CreateRegistersFromModel();

            return _registers;
        }
    }

    public ObservableCollection<StepViewModel> Steps
    {
        get
        {
            if (_steps == null) _steps = CreateStepsFromModel();

            return _steps;
        }
    }

    public GateViewModel SelectedObject
    {
        get => _selectedObject;
        set
        {
            _selectedObject = value;
            OnSelectionChanged();
        }
    }

    #endregion // Model Properties


    #region Presentation Properties

    public static double GateWidth => 96;

    public static double GateHeight => 40;

    public static double GateTextTranslate => (GateWidth - GateHeight) / 2;

    public static double GateTextCanvasTop => (QubitSize - GateHeight) / 2;

    public static double QubitSize => 64;

    public static RelativePoint QubitScaleCenter => new(0, QubitSize / 2, RelativeUnit.Absolute);

    public RelativePoint ScaleCenterY => new(0, _model.TotalWidth * QubitScaleCenter.Point.Y, RelativeUnit.Absolute);

    public double ScaleFactor
    {
        get => _scaleFactor;
        private set
        {
            if (value == _scaleFactor) return;

            _scaleFactor = value;
            OnPropertyChanged(nameof(ScaleFactor));
        }
    }

    public bool AddRegisterEnabled => _model.CurrentStep == 0;

    public int LastStepAdded { get; private set; }

    #endregion // Presentation Properties


    #region Private Helpers

    private void CurrentStepChanged(object sender, EventArgs eventArgs)
    {
        if (_currentStep < _steps.Count) _steps[_currentStep].UnsetAsCurrent();

        _currentStep = _model.CurrentStep;
        if (_currentStep < _steps.Count) _steps[_currentStep].SetAsCurrent();

        OnPropertyChanged("AddQubitEnabled"); // TODO: remove??
    }

    private void _model_SelectionChanged(object sender, EventArgs eventArgs)
    {
        if (_model.UnselectedItems.HasValue)
        {
            var unselected = _model.UnselectedItems.Value;
            for (var i = unselected.BeginColumn; i <= unselected.EndColumn; i++)
            for (var j = unselected.BeginRow; j <= unselected.EndRow; j++)
                _steps[i].Gates[j].RefreshSelection(false);
        }

        if (_model.SelectedItems.HasValue)
        {
            var selected = _model.SelectedItems.Value;
            for (var i = selected.BeginColumn; i <= selected.EndColumn; i++)
            for (var j = selected.BeginRow; j <= selected.EndRow; j++)
                _steps[i].Gates[j].RefreshSelection(true);

            var column = selected.BeginColumn;
            if (column == selected.EndColumn &&
                _steps[column].Gates[selected.BeginRow] == _steps[column].Gates[selected.EndRow])
                SelectedObject = _steps[column].Gates[selected.BeginRow];
        }

        OnSelectionChanged();
    }

    private ObservableCollection<RegisterViewModel> CreateRegistersFromModel()
    {
        var registers = new ObservableCollection<RegisterViewModel>();
        for (var i = 0; i < _model.Registers.Count; i++)
        {
            var reg = new RegisterViewModel(_model, i, _dialogManager);
            reg.QubitsChanged += registers_QubitsChanged;
            registers.Add(reg);
        }

        return registers;
    }

    private void registers_QubitsChanged(object sender, EventArgs eventArgs)
    {
        OnQubitsChanged();
    }

    private ObservableCollection<StepViewModel> CreateStepsFromModel()
    {
        var steps = new ObservableCollection<StepViewModel>();
        for (var i = 0; i < _model.Steps.Count; i++) steps.Add(new StepViewModel(_model, i, _dialogManager));

        steps[_model.CurrentStep].SetAsCurrent();
        return steps;
    }

    private void Steps_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        StepModel step;
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                step = e.NewItems[0] as StepModel;
                var newColumn = e.NewStartingIndex;
                if (step != null)
                {
                    Steps.Insert(newColumn, new StepViewModel(_model, newColumn, _dialogManager));
                    for (var i = newColumn + 1; i < _steps.Count; i++) _steps[i].IncrementColumn();

                    LastStepAdded = newColumn;
                    if (_steps.Count == 2)
                    {
                        foreach (var gate in _steps[0].Gates) gate.UpdateDeleteColumnCommand(true);

                        foreach (var gate in _steps[1].Gates) gate.UpdateDeleteColumnCommand(true);
                    }
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                var oldColumn = e.OldStartingIndex;
                step = e.OldItems[0] as StepModel;
                if (step != null)
                {
                    Steps.RemoveAt(oldColumn);
                    for (var i = oldColumn; i < _steps.Count; i++) _steps[i].IncrementColumn(-1);

                    if (_steps.Count == 1)
                        foreach (var gate in _steps[0].Gates)
                            gate.UpdateDeleteColumnCommand(false);
                }

                break;
        }
    }

    private void Registers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in e.NewItems)
                {
                    var newRow = e.NewStartingIndex;
                    if (item is RegisterModel)
                    {
                        var reg = new RegisterViewModel(_model, newRow, _dialogManager);
                        reg.QubitsChanged += registers_QubitsChanged;
                        Registers.Insert(newRow, reg);
                        for (var i = newRow + 1; i < _registers.Count; i++) _registers[i].IncrementIndex();

                        if (_registers.Count != 2) continue;

                        foreach (var qubit in _registers[0].Qubits) qubit.UpdateDeleteRegisterCommand(true);

                        foreach (var qubit in _registers[1].Qubits) qubit.UpdateDeleteRegisterCommand(true);
                    }
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems)
                {
                    var oldRow = e.OldStartingIndex;
                    if (item is not RegisterModel) continue;

                    Registers.RemoveAt(oldRow);
                    for (var i = oldRow; i < _registers.Count; i++) _registers[i].IncrementIndex(-1);

                    if (_registers.Count == 1)
                        foreach (var qubit in _registers[0].Qubits)
                            qubit.UpdateDeleteRegisterCommand(false);

                    foreach (var step in _steps)
                        for (var i = 0; i < step.Gates.Count; i++)
                            step.Gates[i].UpdateRow(i);
                }

                break;
        }

        OnPropertyChanged(nameof(ScaleCenterY));
        OnSelectionChanged();
    }

    #endregion // Private Helpers
}