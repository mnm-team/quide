#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Windows.Input;
using AvaloniaGUI.CodeHelpers;
using AvaloniaGUI.ViewModels.MainModels.QuantumModel;
using Quantum;
using Quantum.Helpers;

#endregion

namespace AvaloniaGUI.ViewModels.Dialog;

public class NewRegisterInputViewModel : ViewModelBase
{
    #region Fields

    private uint _width = ComputerModel.InitialQubitsCount;
    private string _widthString = ComputerModel.InitialQubitsCount.ToString();
    private ObservableCollection<InitState> _initStates;

    private DelegateCommand _add;
    private DelegateCommand _normalize;

    #endregion // Fields

    #region Public Properties

    /// <summary>
    /// Number of qubits in register
    /// </summary>
    public uint Width
    {
        get => _width;
        private set
        {
            if (value < _width)
            {
                TrimTooWideStates(value);
            }

            _width = value;
            _add.CanExecute(null);
        }
    }

    [UIntegerNumber]
    public string WidthString
    {
        get => _widthString;
        set
        {
            _widthString = value;
            OnPropertyChanged(nameof(WidthString));

            if (!uint.TryParse(value, out var result)) return;

            Width = result;
        }
    }

    public ObservableCollection<InitState> InitStates
    {
        get
        {
            if (_initStates == null)
            {
                _initStates = CreateInitStates();
                _initStates.CollectionChanged += _initStates_CollectionChanged;
            }

            return _initStates;
        }
        set
        {
            _initStates = value;
            OnPropertyChanged(nameof(InitStates));
        }
    }

    public ICommand AddCommand
    {
        get
        {
            if (_add == null)
            {
                _add = new DelegateCommand(Add, x => Math.Pow(2, _width) > InitStates.Count);
            }

            return _add;
        }
    }

    public ICommand NormalizeCommand
    {
        get
        {
            if (_normalize == null)
            {
                _normalize = new DelegateCommand(Normalize, x => true);
            }

            return _normalize;
        }
    }

    #endregion // Public Properties


    #region Public Methods

    public void Add(object parameter)
    {
        HashSet<ulong> values = new HashSet<ulong>();
        foreach (var item in InitStates)
        {
            values.Add(item.Value);
        }

        ulong max = (ulong)1 << (int)_width;
        ulong i = 0;
        InitState added = null;

        while (added == null && i < max)
        {
            if (!values.Contains(i))
            {
                added = new InitState { Value = i, Amplitude = Complex.Zero };
            }

            i++;
        }

        if (added != null)
        {
            InitStates.Add(added);
        }
    }

    public void Normalize(object parameter)
    {
        double sum = 0;
        foreach (InitState state in InitStates)
        {
            sum += Math.Pow(state.Amplitude.Magnitude, 2);
        }

        double limit = (1.0 / ((ulong)1 << (int)_width)) * QuantumComputer.Epsilon;

        if (!(Math.Abs(sum - 1.0) > limit)) return;

        double sqrtSum = Math.Sqrt(sum);
        //we need to normalize
        var oldStates = _initStates;
        var newStates = new ObservableCollection<InitState>();
        foreach (var state in oldStates)
        {
            newStates.Add(new InitState { Value = state.Value, Amplitude = state.Amplitude / sqrtSum });
        }

        InitStates = newStates;
    }

    public Dictionary<ulong, Complex> GetInitStates()
    {
        Dictionary<ulong, Complex> states = new Dictionary<ulong, Complex>();
        foreach (var item in InitStates)
        {
            if (item.Amplitude != Complex.Zero)
            {
                states[item.Value] = item.Amplitude;
            }
        }

        return states;
    }

    public bool InputsValid => uint.TryParse(WidthString, out _) &&
                               InitStates.All(x => ComplexParser.TryParse(x.AmplitudeString, out _));

    #endregion // Public Methods


    #region Private Helpers

    private ObservableCollection<InitState> CreateInitStates()
    {
        ObservableCollection<InitState> states = new ObservableCollection<InitState>
            { new() { Value = 0, Amplitude = Complex.One } };
        return states;
    }

    private void _initStates_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
                _add.CanExecute(null);
                break;
        }
    }

    private void TrimTooWideStates(uint newWidth)
    {
        ulong max = (ulong)1 << (int)newWidth;

        var oldStates = _initStates;
        var newStates = new ObservableCollection<InitState>();
        foreach (var state in oldStates)
        {
            if (state.Value < max)
            {
                newStates.Add(state);
            }
        }

        InitStates = newStates;
    }

    #endregion // Private Helpers
}

/// <summary>
/// Collection of initial states
/// </summary>
public class InitState
{
    private Complex _amplitude;
    private string _amplitudeString;

    private static readonly IFormatProvider Formatter = new ComplexFormatter();

    public ulong Value { get; set; }

    public Complex Amplitude
    {
        get => _amplitude;
        init
        {
            _amplitude = value;
            _amplitudeString = string.Format(Formatter, "{0:I2}", _amplitude);
        }
    }

    [ComplexNumber]
    public string AmplitudeString
    {
        get => _amplitudeString;
        set
        {
            // is indeed used by DataGrid
            _amplitudeString = value;
            if (!ComplexParser.TryParse(value, out var number)) return;

            // Number is valid 
            _amplitude = number;
        }
    }
}