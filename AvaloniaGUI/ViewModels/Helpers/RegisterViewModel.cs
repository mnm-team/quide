#region

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Interactivity;
using AvaloniaGUI.CodeHelpers;
using AvaloniaGUI.ViewModels.Controls;
using AvaloniaGUI.ViewModels.MainModels.QuantumModel;

#endregion

namespace AvaloniaGUI.ViewModels.Helpers;

public class RegisterViewModel : ViewModelBase
{
    #region Events

    public event EventHandler? QubitsChanged;

    private void OnQubitsChanged()
    {
        QubitsChanged?.Invoke(this, new RoutedEventArgs());
    }

    #endregion // Events

    #region Fields

    private readonly ComputerModel _model;

    private int _registerIndex;

    private ObservableCollection<QubitViewModel> _qubits;

    private readonly DialogManager _dialogManager;

    #endregion // Fields


    #region Constructor

    public RegisterViewModel(ComputerModel model, int registerIndex, DialogManager dialogManager)
    {
        _model = model;
        _registerIndex = registerIndex;

        _dialogManager = dialogManager;

        _model.Registers[_registerIndex].Qubits.CollectionChanged += Qubits_CollectionChanged;
        _model.StepChanged += _model_StepChanged;
    }

    #endregion // Constructor


    #region Presentation Properties

    public ObservableCollection<QubitViewModel> Qubits
    {
        get
        {
            if (_qubits == null)
            {
                _qubits = CreateQubitsFromModel();
            }

            return _qubits;
        }
    }

    public string Name => _model.Registers[_registerIndex].Name;

    public double ButtonHeight => Qubits.Count * CircuitGridViewModel.QubitSize;

    public RelativePoint ScaleCenterY =>
        new(0, Qubits.Count * CircuitGridViewModel.QubitScaleCenter.Point.Y,
            RelativeUnit.Absolute);

    public bool AddQubitEnabled => _model.CurrentStep == 0;

    public void IncrementIndex(int delta = 1)
    {
        _registerIndex += delta;
        foreach (QubitViewModel qubit in _qubits)
        {
            qubit.IncrementRegisterIndex(delta);
        }
    }

    #endregion // Presentation Properties


    #region Public Methods

    #endregion // Public Methods


    #region Private Helpers

    private ObservableCollection<QubitViewModel> CreateQubitsFromModel()
    {
        ObservableCollection<QubitViewModel> qubits = new ObservableCollection<QubitViewModel>();
        for (int i = 0; i < _model.Registers[_registerIndex].Qubits.Count; i++)
        {
            qubits.Add(new QubitViewModel(_model, _registerIndex, i, _dialogManager));
        }

        return qubits;
    }

    private void Qubits_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (object item in e.NewItems)
                {
                    int newRow = e.NewStartingIndex;
                    if (item is not QubitModel) continue;

                    _qubits.Insert(newRow, new QubitViewModel(_model, _registerIndex, newRow, _dialogManager));
                    for (int i = newRow + 1; i < _qubits.Count; i++)
                    {
                        _qubits[i].IncrementRow();
                    }

                    if (_qubits.Count != 2) continue;

                    _qubits[0].UpdateDeleteQubitCommand(true);
                    _qubits[1].UpdateDeleteQubitCommand(true);
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (object item in e.OldItems)
                {
                    int oldRow = e.OldStartingIndex;
                    if (item is not QubitModel) continue;

                    _qubits.RemoveAt(oldRow);
                    for (int i = oldRow; i < _qubits.Count; i++)
                    {
                        _qubits[i].IncrementRow(-1);
                    }

                    if (_qubits.Count == 1)
                    {
                        _qubits[0].UpdateDeleteQubitCommand(false);
                    }
                }

                break;
            case NotifyCollectionChangedAction.Replace:
                _qubits[e.NewStartingIndex].Refresh();
                break;
        }

        OnPropertyChanged(nameof(ScaleCenterY));
        OnPropertyChanged(nameof(ButtonHeight));
        OnQubitsChanged();
    }

    private void _model_StepChanged(object? sender, EventArgs eventArgs)
    {
        OnPropertyChanged(nameof(AddQubitEnabled));
    }

    #endregion // Private Helpers
}