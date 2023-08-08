#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Avalonia.Interactivity;
using AvaloniaGUI.CodeHelpers;
using AvaloniaGUI.ViewModels.Helpers;
using AvaloniaGUI.ViewModels.MainModels.QuantumModel;
using AvaloniaGUI.ViewModels.MainModels.QuantumParser;

#endregion

namespace AvaloniaGUI.ViewModels.Controls;

public enum SortField
{
    Value,
    Probability
}

public class OutputGridViewModel : ViewModelBase
{
    #region Events

    public event EventHandler? SelectionChanged;

    private void OnSelectionChanged()
    {
        SelectionChanged?.Invoke(this, new RoutedEventArgs());
    }

    #endregion // Events

    #region Fields

    private ComputerModel _model;
    private OutputViewModel _outputModel;

    private StateViewModel[]? _states;

    private string[] _registersNames;
    private ParameterViewModel _selectedRegister;

    private int _selectedIndex = 0;

    private SortField _sortBy = SortField.Value;
    private bool _sortDesc = false;

    private double _maxProbability;

    private bool _showAll;
    private bool _scaleRelative;

    private CircuitGridViewModel _trackedCircuitGrid;

    #endregion // Fields

    #region Constructor

    // only for design
    public OutputGridViewModel()
    {
        _selectedRegister = new ParameterViewModel("register", typeof(Register), "root");
    }

    #endregion // Constructor


    #region Model Properties

    public StateViewModel[] States
    {
        get { return _states ??= CreateStatesFromModel(); }
    }

    public OutputState SelectedObject
    {
        get
        {
            if (_selectedIndex >= 0 && _selectedIndex < States.Length)
            {
                return States[_selectedIndex].Model;
            }

            return null;
        }
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            _selectedIndex = value;
            OnPropertyChanged(nameof(SelectedIndex));
            OnSelectionChanged();
        }
    }

    public string SelectedRegister
    {
        get => _selectedRegister.ValueString;
        set
        {
            // TODO: probably investigate again
            // somehow gets called after bitflip in CircuitGrid with null value
            if (value is null) return;

            if (value.Equals(_selectedRegister.ValueString)) return;

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

    #endregion // Model Properties


    #region Presentation Properties

    public string[] RegistersNames
    {
        get
        {
            if (_registersNames == null)
            {
                _registersNames = new[] { "root" };
            }

            return _registersNames;
        }
        set
        {
            _registersNames = value;
            OnPropertyChanged(nameof(RegistersNames));
        }
    }

    public bool ShowAll
    {
        get => _showAll;
        set
        {
            if (_showAll == value) return;

            _showAll = value;
            OnPropertyChanged(nameof(ShowAll));
            int prevIndex = SelectedIndex;
            _states = CreateStates();
            _states = ScaleProbability(_states);
            OnPropertyChanged(nameof(States));
            SelectedIndex = prevIndex;
        }
    }

    public bool ScaleRelative
    {
        get => _scaleRelative;
        set
        {
            if (_scaleRelative == value) return;
            _scaleRelative = value;
            OnPropertyChanged(nameof(ScaleRelative));
            _states = ScaleProbability(_states);
        }
    }

    #endregion // Presentation Properties


    #region Public Methods

    public void LoadModel(ComputerModel model, OutputViewModel outputModel)
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
        if (value.Equals(_selectedRegister.ValueString)) return;

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
                    _states = _sortDesc
                        ? States.OrderByDescending(x => x.Probability).ToArray()
                        : States.OrderBy(x => x.Probability).ToArray();

                    break;
                case SortField.Value:
                default:
                    _states = _sortDesc
                        ? States.OrderByDescending(x => x.Value).ToArray()
                        : States.OrderBy(x => x.Value).ToArray();

                    break;
            }

            OnPropertyChanged(nameof(States));

            for (int i = 0; i < _states.Length; i++)
            {
                if (_states[i].Value != selectedState) continue;

                SelectedIndex = i;
                break;
            }
        }
        else
        {
            _sortDesc = !_sortDesc;
            _states = States.Reverse().ToArray();
            OnPropertyChanged(nameof(States));
            SelectedIndex = States.Length - 1 - prevIndex;
        }
    }

    public void AddQubitsTracing(CircuitGridViewModel circuitGrid)
    {
        if (_trackedCircuitGrid == circuitGrid) return;

        circuitGrid.QubitsChanged += circuitGrid_QubitsChanged;
        _trackedCircuitGrid = circuitGrid;
    }

    #endregion // Public Methods


    #region Private Helpers

    private void circuitGrid_QubitsChanged(object? sender, EventArgs eventArgs)
    {
        OnPropertyChanged(nameof(States));
    }

    private void _model_OutputChanged(object? sender, EventArgs eventArgs)
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
        OnPropertyChanged(nameof(States));
        SelectedIndex = prevIndex;
    }

    private StateViewModel[] CreateStatesFromModel()
    {
        SetMaxProbability();
        StateViewModel[] states = CreateStates();
        return ScaleProbability(states);
    }

    private void SetMaxProbability()
    {
        _maxProbability = _outputModel.States.Max(x => x.Probability);
    }

    private StateViewModel[] CreateStates()
    {
        try
        {
            if (_showAll)
            {
                int width = _outputModel.Width;
                int statesCount = (int)Math.Pow(2, width);
                StateViewModel[] states = new StateViewModel[statesCount];

                foreach (OutputState state in _outputModel.States)
                {
                    states[state.Value] = new StateViewModel(state);
                }

                bool amplitudeHasValue = _outputModel.States.First().Amplitude.HasValue;

                for (int i = 0; i < statesCount; i++)
                {
                    if (states[i] != null) continue;

                    if (amplitudeHasValue)
                    {
                        states[i] = new StateViewModel(new OutputState((ulong)i, Complex.Zero, width));
                    }
                    else
                    {
                        states[i] = new StateViewModel(new OutputState((ulong)i, 0.0, width));
                    }
                }

                switch (_sortBy)
                {
                    case SortField.Probability:
                        return _sortDesc
                            ? states.OrderByDescending(x => x.Probability).ToArray()
                            : states.OrderBy(x => x.Probability).ToArray();

                    case SortField.Value:
                    default:
                        return _sortDesc ? states.Reverse().ToArray() : states;
                }
            }
            else
            {
                int statesCount = _outputModel.States.Count;
                StateViewModel[] states = new StateViewModel[statesCount];
                int i = 0;

                IEnumerable<OutputState> sorted = _outputModel.States;
                switch (_sortBy)
                {
                    case SortField.Probability:
                        sorted = _sortDesc
                            ? _outputModel.States.OrderByDescending(x => x.Probability)
                            : _outputModel.States.OrderBy(x => x.Probability);

                        break;
                    case SortField.Value:
                    default:
                        sorted = _sortDesc
                            ? _outputModel.States.OrderByDescending(x => x.Value)
                            : _outputModel.States.OrderBy(x => x.Value);

                        break;
                }

                foreach (OutputState state in sorted)
                {
                    states[i] = new StateViewModel(state);
                    i++;
                }

                return states;
            }
        }
        catch (Exception e)
        {
            PrintException(e);
        }

        return new[] { new StateViewModel(new OutputState(0, Complex.Zero, _outputModel.Width)) };
    }

    private StateViewModel[] ScaleProbability(StateViewModel[] states)
    {
        foreach (var t in states)
        {
            if (_scaleRelative)
            {
                t.RelativeProbability = t.Probability / _maxProbability;
            }
            else
            {
                t.RelativeProbability = t.Probability;
            }
        }

        return states;
    }

    private static void PrintException(Exception e)
    {
        string message = e.Message;
        if (e.InnerException != null)
        {
            message = message + ":\n" + e.InnerException.Message;
        }

        ErrorMessageHelper.ShowMessage(message);
    }

    #endregion // Private Helpers
}