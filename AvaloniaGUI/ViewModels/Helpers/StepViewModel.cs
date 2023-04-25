#region

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
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

    private static readonly Thickness currentMargin = new Thickness(3, 0, 0, 0);
    private static readonly Thickness noMargin = new Thickness(0, 0, 0, 0);

    #endregion // Fields


    #region Constructor

    public StepViewModel(ComputerModel model, int column)
    {
        _model = model;
        _column = column;
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

    public double ScaleCenterY
    {
        get { return Gates.Count * CircuitGridViewModel.QubitScaleCenter; }
    }

    public Thickness StepMargin
    {
        get
        {
            if (_stepMargin == null)
            {
                if (_model.CurrentStep == _column)
                {
                    return currentMargin;
                }

                return noMargin;
            }

            return _stepMargin;
        }
        set
        {
            _stepMargin = value;
            OnPropertyChanged("StepMargin");
        }
    }

    #endregion // Presentation Properties


    #region Public Methids

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
            Gate gate = _model.Steps[_column].Gates[i];
            RegisterRefModel gateRow = _model.GetRefFromOffset(i);
            gates.Add(new GateViewModel(_model, gateRow, _column));
        }

        return gates;
    }

    private void Gates_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Gate gate;
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                int addIndex = e.NewStartingIndex;
                foreach (object item in e.NewItems)
                {
                    gate = item as Gate;
                    if (gate != null)
                    {
                        RegisterRefModel gateRow = _model.GetRefFromOffset(addIndex);
                        Gates.Insert(addIndex, new GateViewModel(_model, gateRow, _column));
                        for (int i = addIndex + 1; i < _gates.Count; i++)
                        {
                            _gates[i].UpdateRow(i);
                        }

                        if (_gates.Count == 2)
                        {
                            _gates[0].UpdateDeleteRowCommand(true);
                            _gates[1].UpdateDeleteRowCommand(true);
                        }
                    }
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
                    if (item is Gate)
                    {
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
                }

                break;
        }

        OnPropertyChanged("ScaleCenterY");
    }

    #endregion // Private Helpers
}