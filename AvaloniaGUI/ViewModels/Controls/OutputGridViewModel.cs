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

    private int _selectedIndex;

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
        _selectedRegister = new ParameterViewModel("register", typeof(Register));
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
        get { return _selectedIndex; }
        set
        {
            _selectedIndex = value;
            OnPropertyChanged(nameof(SelectedIndex));
            OnSelectionChanged();
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

    private string[] _SAS = new string[] { "S", "A", "S" };

    public string[] SAS
    {
        get => _SAS;
        set => _SAS = value;
    }

    public bool ShowAll
    {
        get { return _showAll; }
        set
        {
            if (_showAll != value)
            {
                _showAll = value;
                OnPropertyChanged(nameof(ShowAll));
                int prevIndex = SelectedIndex;
                _states = CreateStates();
                _states = ScaleProbability(_states);
                OnPropertyChanged(nameof(States));
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
                OnPropertyChanged(nameof(ScaleRelative));
                _states = ScaleProbability(_states);
            }
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

            OnPropertyChanged(nameof(States));

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
            OnPropertyChanged(nameof(States));
            SelectedIndex = States.Length - 1 - prevIndex;
        }
    }

    public void AddQubitsTracing(CircuitGridViewModel circuitGrid)
    {
        if (_trackedCircuitGrid != circuitGrid)
        {
            circuitGrid.QubitsChanged += circuitGrid_QubitsChanged;
            _trackedCircuitGrid = circuitGrid;
        }
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
                    if (states[i] == null)
                    {
                        if (amplitudeHasValue)
                        {
                            states[i] = new StateViewModel(new OutputState((ulong)i, Complex.Zero, width));
                        }
                        else
                        {
                            states[i] = new StateViewModel(new OutputState((ulong)i, 0.0, width));
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
                StateViewModel[] states = new StateViewModel[statesCount];
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

        return new StateViewModel[] { new StateViewModel(new OutputState(0, Complex.Zero, _outputModel.Width)) };
    }

    private StateViewModel[] ScaleProbability(StateViewModel[] states)
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

    private static void PrintException(Exception e)
    {
        string message = e.Message;
        if (e.InnerException != null)
        {
            message = message + ":\n" + e.InnerException.Message;
        }

        ErrorMessageHelper.ShowError(message);
    }

    #endregion // Private Helpers
}