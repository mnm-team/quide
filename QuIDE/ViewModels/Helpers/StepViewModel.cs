#region

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using AvaloniaGUI.CodeHelpers;
using AvaloniaGUI.ViewModels.Controls;
using AvaloniaGUI.ViewModels.MainModels.QuantumModel;
using AvaloniaGUI.ViewModels.MainModels.QuantumModel.Gates;

#endregion

namespace AvaloniaGUI.ViewModels.Helpers;

public class StepViewModel : ViewModelBase
{
    #region Fields

    private ObservableCollection<GateViewModel> _gates;

    private ComputerModel _model;

    private int _column;

    private Thickness _stepMargin;

    private static readonly Thickness currentMargin = new(3, 0, 0, 0);
    private static readonly Thickness noMargin = new(0, 0, 0, 0);

    private readonly DialogManager _dialogManager;

    #endregion // Fields


    #region Constructor

    public StepViewModel(ComputerModel model, int column, DialogManager dialogManager)
    {
        _model = model;
        _column = column;

        _dialogManager = dialogManager;

        // they need dialogManager
        _model.Steps[_column].Gates.CollectionChanged += Gates_CollectionChanged;
        _gates = CreateGatesFromModel();
    }

    #endregion // Constructor


    #region Presentation Properties

    public ObservableCollection<GateViewModel> Gates
    {
        get
        {
            if (_gates == null)
            {
                _gates = CreateGatesFromModel();
            }

            return _gates;
        }
    }

    
    // not enabled, because adds blue bottom line to the grid
    public RelativePoint ScaleCenterY =>
        new(0, Gates.Count * CircuitGridViewModel.QubitScaleCenter.Point.Y, RelativeUnit.Absolute);

    public Thickness StepMargin
    {
        get
        {
            return _stepMargin;
        }
        set
        {
            _stepMargin = value;
            OnPropertyChanged(nameof(StepMargin));
        }
    }

    #endregion // Presentation Properties


    #region Public Methods

    public void SetAsCurrent()
    {
        StepMargin = currentMargin;
    }

    public void UnsetAsCurrent()
    {
        StepMargin = noMargin;
    }

    public void IncrementColumn(int columnDelta = 1)
    {
        _column += columnDelta;
        foreach (GateViewModel g in _gates)
        {
            g.ChangeColumn(columnDelta);
        }

        if (_model.CurrentStep == _column)
        {
            SetAsCurrent();
        }
        else
        {
            UnsetAsCurrent();
        }
    }

    #endregion // Public Methods


    #region Private Helpers

    private ObservableCollection<GateViewModel> CreateGatesFromModel()
    {
        ObservableCollection<GateViewModel> gates = new ObservableCollection<GateViewModel>();
        for (int i = 0; i < _model.Steps[_column].Gates.Count; i++)
        {
            RegisterRefModel gateRow = _model.GetRefFromOffset(i);
            gates.Add(new GateViewModel(_model, gateRow, _column, _dialogManager));
        }

        return gates;
    }

    private void Gates_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (_dialogManager is null) return;

        Gate gate;
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                int addIndex = e.NewStartingIndex;
                foreach (object item in e.NewItems)
                {
                    gate = item as Gate;
                    if (gate == null) continue;

                    RegisterRefModel gateRow = _model.GetRefFromOffset(addIndex);
                    Gates.Insert(addIndex, new GateViewModel(_model, gateRow, _column, _dialogManager));
                    for (int i = addIndex + 1; i < _gates.Count; i++)
                    {
                        _gates[i].UpdateRow(i);
                    }

                    if (_gates.Count != 2) continue;

                    _gates[0].UpdateDeleteRowCommand(true);
                    _gates[1].UpdateDeleteRowCommand(true);
                }

                break;
            case NotifyCollectionChangedAction.Replace:
                foreach (object item in e.NewItems)
                {
                    gate = item as Gate;
                    for (int i = gate.Begin; i <= gate.End; i++)
                    {
                        Gates[i].Refresh();
                    }
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                int oldRow = e.OldStartingIndex;
                foreach (object item in e.OldItems)
                {
                    if (item is not Gate) continue;

                    Gates.RemoveAt(oldRow);
                    for (int i = oldRow; i < _gates.Count; i++)
                    {
                        _gates[i].UpdateRow(i);
                    }

                    if (_gates.Count == 1)
                    {
                        _gates[0].UpdateDeleteRowCommand(false);
                    }
                }

                break;
        }

        OnPropertyChanged(nameof(ScaleCenterY));
    }

    #endregion // Private Helpers
}