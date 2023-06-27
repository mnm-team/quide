#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaGUI.CodeHelpers;
using AvaloniaGUI.ViewModels.Controls;
using AvaloniaGUI.ViewModels.Dialog;
using AvaloniaGUI.ViewModels.MainModels.QuantumModel;
using AvaloniaGUI.ViewModels.MainModels.QuantumModel.Gates;
using AvaloniaGUI.ViewModels.MainModels.QuantumParser;
using AvaloniaGUI.Views.Dialog;

#endregion

namespace AvaloniaGUI.ViewModels.Helpers;

public class GateViewModel : ViewModelBase
{
    #region Fields

    private ComputerModel _model;

    private RegisterRefModel _row;
    private int _column;

    private bool _isEnabled = true;
    private bool _isSelected = false;

    private DelegateCommand _insertRowAbove;
    private DelegateCommand _insertRowBelow;
    private DelegateCommand _insertColumnLeft;
    private DelegateCommand _insertColumnRight;
    private DelegateCommand _deleteRow;
    private DelegateCommand _deleteColumn;

    private readonly DialogManager _dialogManager;

    #endregion // Fields


    #region Constructor

    public GateViewModel(ComputerModel model, RegisterRefModel row, int column, DialogManager dialogManager)
    {
        _model = model;
        _row = row;
        _column = column;
        _model.StepChanged += _model_CurrentStepChanged;

        _dialogManager = dialogManager;
    }

    #endregion // Constructor


    #region Model Properties

    public Gate Value => _model.Steps[_column].Gates[_row.OffsetToRoot];

    public RegisterRefModel Row => _row;

    public int Column => _column;

    #endregion // Model Properties


    #region Presentation Properties

    public string GateText
    {
        get
        {
            if (Value is not CustomGate) return null;

            CustomGate cg = Value as CustomGate;

            return (cg.End + cg.Begin + 1) / 2 == _row.OffsetToRoot ? cg.FunctionName : null;
        }
    }

    public double GateHeight =>
        Value is CustomGate &&
        Value.Begin != Value.End
            ? CircuitGridViewModel.QubitSize
            : CircuitGridViewModel.GateHeight;

    public double GateTextHeight
    {
        get
        {
            if (Value is CustomGate)
            {
                int endMinusBegin = Value.End - Value.Begin;
                return CircuitGridViewModel.GateHeight + ((endMinusBegin + 1) / 2) * CircuitGridViewModel.QubitSize;
            }
            else
            {
                return CircuitGridViewModel.GateHeight;
            }
        }
    }


    public double SelectionOpacity => _isSelected ? 0.25 : 0;

    public VisualBrush BackgroundImage
    {
        get
        {
            Gate gate = Value;
            if (gate.Control.HasValue)
            {
                if (_row.OffsetToRoot == gate.Begin)
                {
                    if (_row.Equals(gate.Control.Value))
                    {
                        return Application.Current.FindResource("ImgDownC") as VisualBrush;
                    }

                    return Application.Current.FindResource("ImgDown") as VisualBrush;
                }

                if (_row.OffsetToRoot != gate.End) return Application.Current.FindResource("ImgLine") as VisualBrush;

                if (_row.Equals(gate.Control.Value))
                {
                    return Application.Current.FindResource("ImgUpC") as VisualBrush;
                }

                return Application.Current.FindResource("ImgUp") as VisualBrush;
            }

            if (gate is not MultiControlledGate) return Application.Current.FindResource("ImgEmpty") as VisualBrush;

            if (_row.OffsetToRoot == gate.Begin)
            {
                if (_row.OffsetToRoot != gate.End)
                {
                    return Application.Current.FindResource("ImgDown") as VisualBrush;
                }

                return Application.Current.FindResource("ImgEmpty") as VisualBrush;
            }

            if (_row.OffsetToRoot == gate.End)
            {
                return Application.Current.FindResource("ImgUp") as VisualBrush;
            }

            return Application.Current.FindResource("ImgLine") as VisualBrush;
        }
    }

    public VisualBrush GateImage
    {
        get
        {
            Gate gate = Value;


            if (gate.Name == GateName.Empty)
            {
                return null;
            }

            if (gate is CustomGate cg)
            {
                if (_row.OffsetToRoot == cg.Begin)
                {
                    if (_row.OffsetToRoot == cg.End)
                    {
                        return Application.Current.FindResource("ToolComposite") as VisualBrush;
                    }

                    return Application.Current.FindResource("DownComposite") as VisualBrush;
                }

                if (_row.OffsetToRoot == cg.End)
                {
                    return Application.Current.FindResource("UpComposite") as VisualBrush;
                }

                return Application.Current.FindResource("CenterComposite") as VisualBrush;
            }

            if (gate.Name == GateName.Measure)
            {
                return Application.Current.FindResource("ToolMeasure") as VisualBrush;
            }

            if (_row.Equals(gate.Target))
            {
                VisualBrush brush;
                switch (gate.Name)
                {
                    case GateName.Hadamard:
                        brush = Application.Current.FindResource("ToolH") as VisualBrush;
                        break;
                    case GateName.SigmaX:
                        brush = Application.Current.FindResource("ToolX") as VisualBrush;
                        break;
                    case GateName.SigmaY:
                        brush = Application.Current.FindResource("ToolY") as VisualBrush;
                        break;
                    case GateName.SigmaZ:
                        brush = Application.Current.FindResource("ToolZ") as VisualBrush;
                        break;
                    case GateName.SqrtX:
                        brush = Application.Current.FindResource("ToolSqrtX") as VisualBrush;
                        break;
                    case GateName.RotateX:
                        brush = Application.Current.FindResource("ToolRotateX") as VisualBrush;
                        break;
                    case GateName.RotateY:
                        brush = Application.Current.FindResource("ToolRotateY") as VisualBrush;
                        break;
                    case GateName.RotateZ:
                        brush = Application.Current.FindResource("ToolRotateZ") as VisualBrush;
                        break;
                    case GateName.PhaseKick:
                        brush = Application.Current.FindResource("ToolPhaseKick") as VisualBrush;
                        break;
                    case GateName.PhaseScale:
                        brush = Application.Current.FindResource("ToolPhaseScale") as VisualBrush;
                        break;
                    case GateName.Unitary:
                        brush = Application.Current.FindResource("ToolU") as VisualBrush;
                        break;
                    case GateName.CNot:
                    case GateName.Toffoli:
                        brush = Application.Current.FindResource("ImgNot") as VisualBrush;
                        break;
                    case GateName.CPhaseShift:
                        brush = Application.Current.FindResource("ToolCPhaseShift") as VisualBrush;
                        break;
                    case GateName.InvCPhaseShift:
                        brush = Application.Current.FindResource("ToolInvCPhaseShift") as VisualBrush;
                        break;
                    default:
                        return null;
                }

                return brush;
            }

            if (gate is not MultiControlledGate controlledGate) return null;

            if (controlledGate.Controls.Contains(_row))
            {
                return Application.Current.FindResource("ImgC") as VisualBrush;
            }

            return Application.Current.FindResource("ImgLine") as VisualBrush;
        }
    }

    public bool IsEnabled => _isEnabled;

    public ICommand InsertRowAboveCommand
    {
        get
        {
            if (_insertRowAbove == null)
            {
                _insertRowAbove = new DelegateCommand(InsertRowAbove, x => true);
            }

            return _insertRowAbove;
        }
    }

    public ICommand InsertRowBelowCommand
    {
        get
        {
            if (_insertRowBelow == null)
            {
                _insertRowBelow = new DelegateCommand(InsertRowBelow, x => true);
            }

            return _insertRowBelow;
        }
    }

    public ICommand InsertColumnLeftCommand
    {
        get
        {
            if (_insertColumnLeft == null)
            {
                _insertColumnLeft = new DelegateCommand(InsertColumnLeft, x => true);
            }

            return _insertColumnLeft;
        }
    }

    public ICommand InsertColumnRightCommand
    {
        get
        {
            if (_insertColumnRight == null)
            {
                _insertColumnRight = new DelegateCommand(InsertColumnRight, x => true);
            }

            return _insertColumnRight;
        }
    }

    public ICommand DeleteRowCommand
    {
        get
        {
            if (_deleteRow == null)
            {
                _deleteRow = new DelegateCommand(DeleteRow, x => true);
            }

            return _deleteRow;
        }
    }

    public ICommand DeleteColumnCommand
    {
        get
        {
            if (_deleteColumn == null)
            {
                _deleteColumn = new DelegateCommand(DeleteColumn, x => true);
            }

            return _deleteColumn;
        }
    }

    #endregion // Presentation Properties


    #region Public Methods

    public void UpdateRow(int offsetToRoot)
    {
        _row = _model.GetRefFromOffset(offsetToRoot);
        Refresh();
    }

    public void ChangeColumn(int columnDelta)
    {
        _column += columnDelta;
        Refresh();
    }

    public async void SetGate(int pressedColumn, RegisterRefModel pressedRow, KeyModifiers keyStates)
    {
        ActionName action = MainWindowViewModel.SelectedAction;

        // make selection
        if (keyStates.HasFlag(KeyModifiers.Shift))
        {
            // move selection
            if (keyStates.HasFlag(KeyModifiers.Control))
            {
                if (!_model.IsSelected(pressedRow.OffsetToRoot, pressedColumn)) return;

                _model.Cut();
                _model.Select(_row.OffsetToRoot, _row.OffsetToRoot, _column, _column);
                _model.Paste();
            }
            else
            {
                _model.Select(pressedRow.OffsetToRoot, _row.OffsetToRoot, pressedColumn, _column);
            }
        }
        else
        {
            Gate oldGate;
            switch (action)
            {
                case ActionName.Empty:
                    oldGate = _model.Steps[_column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name != GateName.Empty)
                    {
                        for (int i = oldGate.Begin; i <= oldGate.End; i++)
                        {
                            RegisterRefModel gateRef = _model.GetRefFromOffset(i);
                            Gate newGate = new EmptyGate(gateRef);
                            _model.Steps[_column].SetGate(newGate);
                        }

                        if (oldGate is MultiControlledGate mcg)
                        {
                            if (mcg.Controls.Contains(_row))
                            {
                                RegisterRefModel[] toRemove = new[] { _row };
                                RegisterRefModel[] newControls =
                                    mcg.Controls.Except(toRemove).ToArray();
                                Gate toAdd;
                                switch (mcg.Name)
                                {
                                    case GateName.PhaseKick:
                                    {
                                        PhaseKickGate pk = mcg as PhaseKickGate;
                                        toAdd = new PhaseKickGate(pk.Gamma, pk.Target, newControls);
                                        break;
                                    }
                                    case GateName.CPhaseShift:
                                    {
                                        CPhaseShiftGate cps = mcg as CPhaseShiftGate;
                                        toAdd = new CPhaseShiftGate(cps.Dist, cps.Target, newControls);
                                        break;
                                    }
                                    case GateName.InvCPhaseShift:
                                    {
                                        InvCPhaseShiftGate icps = mcg as InvCPhaseShiftGate;
                                        toAdd = new InvCPhaseShiftGate(icps.Dist, icps.Target, newControls);
                                        break;
                                    }
                                    // Toffoli
                                    default:
                                    {
                                        if (newControls.Length > 1)
                                        {
                                            toAdd = new ToffoliGate(mcg.Target, newControls);
                                        }
                                        else
                                        {
                                            toAdd = new CNotGate(mcg.Target, newControls[0]);
                                        }

                                        break;
                                    }
                                }

                                _model.Steps[_column].SetGate(toAdd);
                            }
                        }
                        else if (oldGate.Name == GateName.CNot)
                        {
                            if (oldGate.Control.Value.OffsetToRoot == _row.OffsetToRoot)
                            {
                                Gate toAdd = new SigmaXGate(oldGate.Target);
                                _model.Steps[_column].SetGate(toAdd);
                            }
                        }
                        else if (oldGate is SingleGate { Control: { } } sg)
                        {
                            if (sg.Control.Value.OffsetToRoot == _row.OffsetToRoot)
                            {
                                Gate toAdd = null;
                                switch (sg.Name)
                                {
                                    case GateName.Hadamard:
                                        toAdd = new HadamardGate(sg.Target);
                                        break;
                                    case GateName.PhaseScale:
                                        toAdd = new PhaseScaleGate((sg as PhaseScaleGate).Gamma, sg.Target);
                                        break;
                                    case GateName.RotateX:
                                        toAdd = new RotateXGate((sg as RotateXGate).Gamma, sg.Target);
                                        break;
                                    case GateName.RotateY:
                                        toAdd = new RotateYGate((sg as RotateYGate).Gamma, sg.Target);
                                        break;
                                    case GateName.RotateZ:
                                        toAdd = new RotateZGate((sg as RotateZGate).Gamma, sg.Target);
                                        break;
                                    case GateName.SigmaX:
                                        toAdd = new SigmaXGate(sg.Target);
                                        break;
                                    case GateName.SigmaY:
                                        toAdd = new SigmaYGate(sg.Target);
                                        break;
                                    case GateName.SigmaZ:
                                        toAdd = new SigmaZGate(sg.Target);
                                        break;
                                    case GateName.SqrtX:
                                        toAdd = new SqrtXGate(sg.Target);
                                        break;
                                    case GateName.Unitary:
                                        toAdd = new UnitaryGate((sg as UnitaryGate).Matrix, sg.Target);
                                        break;
                                    default:
                                        break;
                                }

                                _model.Steps[_column].SetGate(toAdd);
                            }
                        }
                    }

                    break;
                case ActionName.Hadamard:
                    oldGate = _model.Steps[_column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name == GateName.Empty)
                    {
                        _model.Steps[_column].SetGate(new HadamardGate(_row));
                        _model.AddStepAfter(_column);
                    }

                    break;
                case ActionName.SigmaX:
                    oldGate = _model.Steps[_column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name == GateName.Empty)
                    {
                        _model.Steps[_column].SetGate(new SigmaXGate(_row));
                        _model.AddStepAfter(_column);
                    }

                    break;
                case ActionName.SigmaY:
                    oldGate = _model.Steps[_column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name == GateName.Empty)
                    {
                        _model.Steps[_column].SetGate(new SigmaYGate(_row));
                        _model.AddStepAfter(_column);
                    }

                    break;
                case ActionName.SigmaZ:
                    oldGate = _model.Steps[_column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name == GateName.Empty)
                    {
                        _model.Steps[_column].SetGate(new SigmaZGate(_row));
                        _model.AddStepAfter(_column);
                    }

                    break;
                case ActionName.SqrtX:
                    oldGate = _model.Steps[_column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name == GateName.Empty)
                    {
                        _model.Steps[_column].SetGate(new SqrtXGate(_row));
                        _model.AddStepAfter(_column);
                    }

                    break;
                case ActionName.RotateX:
                case ActionName.RotateY:
                case ActionName.RotateZ:
                case ActionName.PhaseKick:
                case ActionName.PhaseScale:
                    oldGate = _model.Steps[_column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name == GateName.Empty)
                    {
                        GammaInputViewModel gammaVM = new GammaInputViewModel();

                        await _dialogManager.ShowDialogAsync(new GammaInput(gammaVM), () =>
                        {
                            double gamma = gammaVM.Gamma;
                            switch (action)
                            {
                                case ActionName.RotateX:
                                    SetCustomGates(new RotateXGate(gamma, _row));
                                    break;
                                case ActionName.RotateY:
                                    SetCustomGates(new RotateYGate(gamma, _row));
                                    break;
                                case ActionName.RotateZ:
                                    SetCustomGates(new RotateZGate(gamma, _row));
                                    break;
                                case ActionName.PhaseKick:
                                    SetCustomGates(new PhaseKickGate(gamma, _row));
                                    break;
                                default:
                                    SetCustomGates(new PhaseScaleGate(gamma, _row));
                                    break;
                            }
                        });
                    }

                    break;
                case ActionName.Unitary:
                    oldGate = _model.Steps[_column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name == GateName.Empty)
                    {
                        MatrixInputViewModel matrixVM = new MatrixInputViewModel();

                        await _dialogManager.ShowDialogAsync(new MatrixInput(matrixVM), () =>
                        {
                            Complex[,] matrix = matrixVM.Matrix;
                            if (matrix == null) return;

                            _model.Steps[_column].SetGate(new UnitaryGate(matrix, _row));
                            _model.AddStepAfter(_column);
                        });
                    }

                    break;
                case ActionName.Control:
                    if (pressedColumn == _column)
                    {
                        oldGate = _model.Steps[_column].Gates[_row.OffsetToRoot];
                        if (!_row.Equals(pressedRow))
                        {
                            switch (oldGate.Name)
                            {
                                case GateName.CNot:
                                {
                                    CNotGate oldCnot = oldGate as CNotGate;
                                    if (pressedRow.OffsetToRoot < oldGate.Begin)
                                    {
                                        if (_model.Steps[_column].HasPlace(pressedRow.OffsetToRoot, oldGate.Begin - 1))
                                        {
                                            _model.Steps[_column].SetGate(new ToffoliGate(oldCnot.Target,
                                                oldCnot.Control.Value, pressedRow));
                                        }
                                    }
                                    else if (pressedRow.OffsetToRoot > oldGate.End)
                                    {
                                        if (_model.Steps[_column].HasPlace(oldGate.End + 1, pressedRow.OffsetToRoot))
                                        {
                                            _model.Steps[_column].SetGate(new ToffoliGate(oldCnot.Target,
                                                oldCnot.Control.Value, pressedRow));
                                        }
                                    }
                                    else // new Control inside
                                    {
                                        //check if not doubled
                                        if (_row.OffsetToRoot != oldCnot.Target.OffsetToRoot &&
                                            _row.OffsetToRoot != oldCnot.Control.Value.OffsetToRoot)
                                        {
                                            _model.Steps[_column].SetGate(new ToffoliGate(oldCnot.Target,
                                                oldCnot.Control.Value, _row));
                                        }
                                    }

                                    break;
                                }
                                case GateName.Toffoli:
                                {
                                    ToffoliGate oldT = oldGate as ToffoliGate;
                                    if (pressedRow.OffsetToRoot < oldGate.Begin)
                                    {
                                        if (_model.Steps[_column].HasPlace(pressedRow.OffsetToRoot, oldGate.Begin - 1))
                                        {
                                            _model.Steps[_column]
                                                .SetGate(new ToffoliGate(oldT.Target, pressedRow, oldT.Controls));
                                        }
                                    }
                                    else if (pressedRow.OffsetToRoot > oldGate.End)
                                    {
                                        if (_model.Steps[_column].HasPlace(oldGate.End + 1, pressedRow.OffsetToRoot))
                                        {
                                            _model.Steps[_column]
                                                .SetGate(new ToffoliGate(oldT.Target, pressedRow, oldT.Controls));
                                        }
                                    }
                                    else // new Control inside
                                    {
                                        //check if not doubled
                                        if (_row.OffsetToRoot != oldT.Target.OffsetToRoot &&
                                            !oldT.Controls.Contains(_row))
                                        {
                                            _model.Steps[_column]
                                                .SetGate(new ToffoliGate(oldT.Target, _row, oldT.Controls));
                                        }
                                    }

                                    break;
                                }
                                case GateName.CPhaseShift:
                                {
                                    CPhaseShiftGate oldT = oldGate as CPhaseShiftGate;
                                    if (pressedRow.OffsetToRoot < oldGate.Begin)
                                    {
                                        if (_model.Steps[_column].HasPlace(pressedRow.OffsetToRoot, oldGate.Begin - 1))
                                        {
                                            List<RegisterRefModel> cList = oldT.Controls.ToList();
                                            cList.Add(pressedRow);
                                            RegisterRefModel[] cParams = cList.ToArray<RegisterRefModel>();
                                            _model.Steps[_column]
                                                .SetGate(new CPhaseShiftGate(oldT.Dist, oldT.Target, cParams));
                                        }
                                    }
                                    else if (pressedRow.OffsetToRoot > oldGate.End)
                                    {
                                        if (_model.Steps[_column].HasPlace(oldGate.End + 1, pressedRow.OffsetToRoot))
                                        {
                                            List<RegisterRefModel> cList = oldT.Controls.ToList();
                                            cList.Add(pressedRow);
                                            RegisterRefModel[] cParams = cList.ToArray<RegisterRefModel>();
                                            _model.Steps[_column]
                                                .SetGate(new CPhaseShiftGate(oldT.Dist, oldT.Target, cParams));
                                        }
                                    }
                                    else // new Control inside
                                    {
                                        List<RegisterRefModel> cList = oldT.Controls.ToList();
                                        cList.Add(_row);
                                        RegisterRefModel[] cParams = cList.ToArray<RegisterRefModel>();
                                        _model.Steps[_column]
                                            .SetGate(new CPhaseShiftGate(oldT.Dist, oldT.Target, cParams));
                                    }

                                    break;
                                }
                                case GateName.InvCPhaseShift:
                                {
                                    InvCPhaseShiftGate oldT = oldGate as InvCPhaseShiftGate;
                                    if (pressedRow.OffsetToRoot < oldGate.Begin)
                                    {
                                        if (_model.Steps[_column].HasPlace(pressedRow.OffsetToRoot, oldGate.Begin - 1))
                                        {
                                            List<RegisterRefModel> cList = oldT.Controls.ToList();
                                            cList.Add(pressedRow);
                                            RegisterRefModel[] cParams = cList.ToArray<RegisterRefModel>();
                                            _model.Steps[_column]
                                                .SetGate(new InvCPhaseShiftGate(oldT.Dist, oldT.Target, cParams));
                                        }
                                    }
                                    else if (pressedRow.OffsetToRoot > oldGate.End)
                                    {
                                        if (_model.Steps[_column].HasPlace(oldGate.End + 1, pressedRow.OffsetToRoot))
                                        {
                                            List<RegisterRefModel> cList = oldT.Controls.ToList();
                                            cList.Add(pressedRow);
                                            RegisterRefModel[] cParams = cList.ToArray<RegisterRefModel>();
                                            _model.Steps[_column]
                                                .SetGate(new InvCPhaseShiftGate(oldT.Dist, oldT.Target, cParams));
                                        }
                                    }
                                    else // new Control inside
                                    {
                                        List<RegisterRefModel> cList = oldT.Controls.ToList();
                                        cList.Add(_row);
                                        RegisterRefModel[] cParams = cList.ToArray<RegisterRefModel>();
                                        _model.Steps[_column]
                                            .SetGate(new InvCPhaseShiftGate(oldT.Dist, oldT.Target, cParams));
                                    }

                                    break;
                                }
                                case GateName.PhaseKick:
                                {
                                    PhaseKickGate oldT = oldGate as PhaseKickGate;
                                    if (pressedRow.OffsetToRoot < oldGate.Begin)
                                    {
                                        if (_model.Steps[_column].HasPlace(pressedRow.OffsetToRoot, oldGate.Begin - 1))
                                        {
                                            List<RegisterRefModel> cList = oldT.Controls.ToList();
                                            cList.Add(pressedRow);
                                            RegisterRefModel[] cParams = cList.ToArray<RegisterRefModel>();
                                            _model.Steps[_column]
                                                .SetGate(new PhaseKickGate(oldT.Gamma, oldT.Target, cParams));
                                        }
                                    }
                                    else if (pressedRow.OffsetToRoot > oldGate.End)
                                    {
                                        if (_model.Steps[_column].HasPlace(oldGate.End + 1, pressedRow.OffsetToRoot))
                                        {
                                            List<RegisterRefModel> cList = oldT.Controls.ToList();
                                            cList.Add(pressedRow);
                                            RegisterRefModel[] cParams = cList.ToArray<RegisterRefModel>();
                                            _model.Steps[_column]
                                                .SetGate(new PhaseKickGate(oldT.Gamma, oldT.Target, cParams));
                                        }
                                    }
                                    else // new Control inside
                                    {
                                        List<RegisterRefModel> cList = oldT.Controls.ToList();
                                        cList.Add(_row);
                                        RegisterRefModel[] cParams = cList.ToArray<RegisterRefModel>();
                                        _model.Steps[_column]
                                            .SetGate(new PhaseKickGate(oldT.Gamma, oldT.Target, cParams));
                                    }

                                    break;
                                }
                                default:
                                {
                                    if (oldGate.Control == null)
                                    {
                                        int lastEmptyRow = _row.OffsetToRoot - 1;
                                        if (pressedRow.OffsetToRoot > _row.OffsetToRoot)
                                        {
                                            lastEmptyRow += 2;
                                        }

                                        if (_model.Steps[_column].HasPlace(pressedRow.OffsetToRoot, lastEmptyRow))
                                        {
                                            switch (oldGate.Name)
                                            {
                                                case GateName.Hadamard:
                                                    _model.Steps[_column].SetGate(new HadamardGate(_row, pressedRow));
                                                    break;
                                                case GateName.PhaseScale:
                                                    PhaseScaleGate oldPs = oldGate as PhaseScaleGate;
                                                    _model.Steps[_column]
                                                        .SetGate(new PhaseScaleGate(oldPs.Gamma, _row, pressedRow));
                                                    break;
                                                case GateName.RotateX:
                                                    RotateXGate oldRx = oldGate as RotateXGate;
                                                    _model.Steps[_column]
                                                        .SetGate(new RotateXGate(oldRx.Gamma, _row, pressedRow));
                                                    break;
                                                case GateName.RotateY:
                                                    RotateYGate oldRy = oldGate as RotateYGate;
                                                    _model.Steps[_column]
                                                        .SetGate(new RotateYGate(oldRy.Gamma, _row, pressedRow));
                                                    break;
                                                case GateName.RotateZ:
                                                    RotateZGate oldRz = oldGate as RotateZGate;
                                                    _model.Steps[_column]
                                                        .SetGate(new RotateZGate(oldRz.Gamma, _row, pressedRow));
                                                    break;
                                                case GateName.SigmaX:
                                                    _model.Steps[_column].SetGate(new CNotGate(_row, pressedRow));
                                                    break;
                                                case GateName.SigmaY:
                                                    _model.Steps[_column].SetGate(new SigmaYGate(_row, pressedRow));
                                                    break;
                                                case GateName.SigmaZ:
                                                    _model.Steps[_column].SetGate(new SigmaZGate(_row, pressedRow));
                                                    break;
                                                case GateName.SqrtX:
                                                    _model.Steps[_column].SetGate(new SqrtXGate(_row, pressedRow));
                                                    break;
                                                case GateName.Unitary:
                                                    UnitaryGate oldU = oldGate as UnitaryGate;
                                                    _model.Steps[_column]
                                                        .SetGate(new UnitaryGate(oldU.Matrix, _row, pressedRow));
                                                    break;
                                                case GateName.Empty:
                                                    _model.Steps[_column].SetGate(new CNotGate(_row, pressedRow));
                                                    _model.AddStepAfter(_column);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                        else // add control inside (only CNot or Toffoli)
                        {
                            switch (oldGate.Name)
                            {
                                case GateName.CNot:
                                {
                                    CNotGate oldCnot = oldGate as CNotGate;
                                    //check if not doubled
                                    if (_row.OffsetToRoot != oldCnot.Target.OffsetToRoot &&
                                        _row.OffsetToRoot != oldCnot.Control.Value.OffsetToRoot)
                                    {
                                        _model.Steps[_column]
                                            .SetGate(new ToffoliGate(oldCnot.Target, _row, oldCnot.Control.Value));
                                    }

                                    break;
                                }
                                case GateName.Toffoli:
                                {
                                    ToffoliGate oldT = oldGate as ToffoliGate;
                                    //check if not doubled
                                    if (_row.OffsetToRoot != oldT.Target.OffsetToRoot &&
                                        !oldT.Controls.Contains<RegisterRefModel>(_row))
                                    {
                                        _model.Steps[_column]
                                            .SetGate(new ToffoliGate(oldT.Target, _row, oldT.Controls));
                                    }

                                    break;
                                }
                                case GateName.CPhaseShift:
                                {
                                    CPhaseShiftGate oldT = oldGate as CPhaseShiftGate;
                                    //check if not doubled
                                    if (_row.OffsetToRoot != oldT.Target.OffsetToRoot &&
                                        !oldT.Controls.Contains<RegisterRefModel>(_row))
                                    {
                                        List<RegisterRefModel> cList = oldT.Controls.ToList<RegisterRefModel>();
                                        cList.Add(_row);
                                        RegisterRefModel[] cParams = cList.ToArray<RegisterRefModel>();
                                        _model.Steps[_column]
                                            .SetGate(new CPhaseShiftGate(oldT.Dist, oldT.Target, cParams));
                                    }

                                    break;
                                }
                                case GateName.InvCPhaseShift:
                                {
                                    InvCPhaseShiftGate oldT = oldGate as InvCPhaseShiftGate;
                                    //check if not doubled
                                    if (_row.OffsetToRoot != oldT.Target.OffsetToRoot &&
                                        !oldT.Controls.Contains<RegisterRefModel>(_row))
                                    {
                                        List<RegisterRefModel> cList = oldT.Controls.ToList<RegisterRefModel>();
                                        cList.Add(_row);
                                        RegisterRefModel[] cParams = cList.ToArray<RegisterRefModel>();
                                        _model.Steps[_column]
                                            .SetGate(new InvCPhaseShiftGate(oldT.Dist, oldT.Target, cParams));
                                    }

                                    break;
                                }
                                case GateName.PhaseKick:
                                {
                                    PhaseKickGate oldT = oldGate as PhaseKickGate;
                                    //check if not doubled
                                    if (_row.OffsetToRoot != oldT.Target.OffsetToRoot &&
                                        !oldT.Controls.Contains<RegisterRefModel>(_row))
                                    {
                                        List<RegisterRefModel> cList = oldT.Controls.ToList<RegisterRefModel>();
                                        cList.Add(_row);
                                        RegisterRefModel[] cParams = cList.ToArray<RegisterRefModel>();
                                        _model.Steps[_column]
                                            .SetGate(new PhaseKickGate(oldT.Gamma, oldT.Target, cParams));
                                    }

                                    break;
                                }
                            }
                        }
                    }

                    break;
                case ActionName.CPhaseShift:
                    if (pressedColumn == _column)
                    {
                        if (_model.Steps[_column].HasPlace(pressedRow.OffsetToRoot, _row.OffsetToRoot))
                        {
                            // MainWindow window1 = App.Current.MainWindow as MainWindow;
                            // PhaseDistInputViewModel vm = new PhaseDistInputViewModel();
                            // vm.DistText = Math.Abs(pressedRow.OffsetToRoot - _row.OffsetToRoot).ToString();
                            //
                            // ICustomContentDialog dialog1 = window1.DialogManager.CreateCustomContentDialog(
                            //     new PhaseDistInput(vm), DialogMode.OkCancel);
                            // dialog1.Ok = () =>
                            // {
                            //     int? dist = vm.Dist;
                            //     if (dist.HasValue)
                            //     {
                            //         if (_row.Equals(pressedRow))
                            //         {
                            //             _model.Steps[_column].SetGate(new CPhaseShiftGate(dist.Value, _row));
                            //         }
                            //         else
                            //         {
                            //             _model.Steps[_column]
                            //                 .SetGate(new CPhaseShiftGate(dist.Value, _row, pressedRow));
                            //         }
                            //
                            //         _model.AddStepAfter(_column);
                            //     }
                            // };
                            // dialog1.Show();
                        }
                    }

                    break;
                case ActionName.InvCPhaseShift:
                    if (pressedColumn == _column)
                    {
                        if (_model.Steps[_column].HasPlace(pressedRow.OffsetToRoot, _row.OffsetToRoot))
                        {
                            // MainWindow window1 = App.Current.MainWindow as MainWindow;
                            // PhaseDistInputViewModel vm = new PhaseDistInputViewModel();
                            // vm.DistText = Math.Abs(pressedRow.OffsetToRoot - _row.OffsetToRoot).ToString();
                            //
                            // ICustomContentDialog dialog1 = window1.DialogManager.CreateCustomContentDialog(
                            //     new PhaseDistInput(vm), DialogMode.OkCancel);
                            // dialog1.Ok = () =>
                            // {
                            //     int? dist = vm.Dist;
                            //     if (dist.HasValue)
                            //     {
                            //         if (_row.Equals(pressedRow))
                            //         {
                            //             _model.Steps[_column].SetGate(new InvCPhaseShiftGate(dist.Value, _row));
                            //         }
                            //         else
                            //         {
                            //             _model.Steps[_column]
                            //                 .SetGate(new InvCPhaseShiftGate(dist.Value, _row, pressedRow));
                            //         }
                            //
                            //         _model.AddStepAfter(_column);
                            //     }
                            // };
                            // dialog1.Show();
                        }
                    }

                    break;
                case ActionName.Measure:
                    if (pressedColumn == _column)
                    {
                        if (_model.Steps[_column].HasPlace(pressedRow.OffsetToRoot, _row.OffsetToRoot))
                        {
                            _model.Steps[_column].SetGate(new MeasureGate(pressedRow, _row));
                            _model.AddStepAfter(_column);
                        }
                    }

                    break;
                case ActionName.Ungroup:
                    oldGate = _model.Steps[_column].Gates[_row.OffsetToRoot];
                    switch (oldGate.Name)
                    {
                        case GateName.Parametric:
                        {
                            CircuitEvaluator eval = CircuitEvaluator.GetInstance();

                            try
                            {
                                _model.SetStepForGates(_column);

                                int oldColumns = _model.Steps.Count;
                                int oldBegin = oldGate.Begin;
                                int oldEnd = oldGate.End;

                                eval.Decompose(oldGate as ParametricGate);
                                int columnsAdded = _model.Steps.Count - oldColumns;

                                _model.ResetStepForGates();

                                //remove old composite gate
                                for (int i = oldGate.Begin; i <= oldGate.End; i++)
                                {
                                    RegisterRefModel gateRef = _model.GetRefFromOffset(i);
                                    Gate newGate = new EmptyGate(gateRef);
                                    _model.Steps[_column].SetGate(newGate);
                                }

                                //delete step on _column (if it is empty)
                                bool isEmpty = true;
                                int j = 0;
                                while (isEmpty && j < _model.Steps[_column].Gates.Count)
                                {
                                    if (_model.Steps[_column].Gates[j].Name != GateName.Empty)
                                    {
                                        isEmpty = false;
                                    }

                                    j++;
                                }

                                if (isEmpty)
                                {
                                    _model.DeleteStep(_column);
                                    _model.Select(oldBegin, oldEnd, _column, _column + columnsAdded - 1);
                                }
                                else
                                {
                                    _model.Select(oldBegin, oldEnd, _column + 1, _column + columnsAdded);
                                }
                            }
                            catch (Exception ex)
                            {
                                string msg = "Unable to ungroup gate. Its parameters are invalid.\n" +
                                             "Inner exception:\n" +
                                             (ex.InnerException == null ? ex.Message : ex.InnerException.Message);
                                ErrorMessageHelper.ShowMessage(msg);
                            }

                            break;
                        }
                        case GateName.Composite:
                        {
                            CompositeGate cg = oldGate as CompositeGate;
                            List<Gate> toAdd = _model.GetActualGates(cg);
                            int column = _column;
                            foreach (Gate g in toAdd)
                            {
                                if (g.Name == GateName.Empty) continue;

                                _model.InsertStepRight(column);
                                column++;
                                _model.Steps[column].SetGate(g);
                            }

                            //remove old composite gate
                            for (int i = oldGate.Begin; i <= oldGate.End; i++)
                            {
                                RegisterRefModel gateRef = _model.GetRefFromOffset(i);
                                Gate newGate = new EmptyGate(gateRef);
                                _model.Steps[_column].SetGate(newGate);
                            }

                            //delete step on _column (if it is empty)
                            bool isEmpty = true;
                            int j = 0;
                            while (isEmpty && j < _model.Steps[_column].Gates.Count)
                            {
                                if (_model.Steps[_column].Gates[j].Name != GateName.Empty)
                                {
                                    isEmpty = false;
                                }

                                j++;
                            }

                            if (isEmpty)
                            {
                                _model.DeleteStep(_column);
                            }

                            break;
                        }
                    }

                    break;
                case ActionName.Composite:
                    oldGate = _model.Steps[_column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name == GateName.Empty)
                    {
                        //TODO:
                        //MainWindow window1 = App.Current.MainWindow as MainWindow;

                        CircuitEvaluator eval = CircuitEvaluator.GetInstance();
                        Dictionary<string, List<MethodInfo>> dict = eval.GetExtensionGates();
                        if (!string.IsNullOrWhiteSpace(MainWindowViewModel.SelectedComposite))
                        {
                            // ParametricInputViewModel vm = new ParametricInputViewModel(
                            //     MainWindowViewModel.SelectedComposite, dict,
                            //     _model.CompositeGates);
                            // ParametricInput ci = new ParametricInput(vm);
                            // ICustomContentDialog dialog1 = window1.DialogManager
                            //     .CreateCustomContentDialog(ci, DialogMode.OkCancel);
                            // dialog1.Ok = () =>
                            // {
                            //     try
                            //     {
                            //         if (vm.IsValid)
                            //         {
                            //             if (vm.Method != null)
                            //             {
                            //                 ParametricGate cg = eval.CreateParametricGate(vm.Method, vm.ParamValues);
                            //                 _model.Steps[_column].SetGate(cg);
                            //                 _model.AddStepAfter(_column);
                            //             }
                            //             else if (vm.CopositeGateTarget != null)
                            //             {
                            //                 int minWidth = _model.MinWidthForComposite(vm.FunctionName);
                            //                 if (vm.CopositeGateTarget.Value.Width < minWidth)
                            //                 {
                            //                     StringBuilder sb =
                            //                         new StringBuilder("Entered parameter has too small width.\n");
                            //                     sb.Append("Entered width: ");
                            //                     sb.Append(vm.CopositeGateTarget.Value.Width).AppendLine();
                            //                     sb.Append("Minimum width: ");
                            //                     sb.Append(minWidth).AppendLine();
                            //                     throw new Exception(sb.ToString());
                            //                 }
                            //                 else
                            //                 {
                            //                     CompositeGate cg = new CompositeGate(vm.FunctionName,
                            //                         vm.CopositeGateTarget.Value);
                            //                     _model.Steps[_column].SetGate(cg);
                            //                     _model.AddStepAfter(_column);
                            //                 }
                            //             }
                            //         }
                            //     }
                            //     catch (Exception ex)
                            //     {
                            //         string msg = "Unable to add gate. The parameters are invalid.\n" +
                            //                      "Inner exception:\n" +
                            //                      (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                            //         MessageBox.Show(
                            //             msg,
                            //             "Unable to add gate",
                            //             MessageBoxButton.OK,
                            //             MessageBoxImage.Error);
                            //     }
                            // };
                            // dialog1.Show();
                        }
                    }

                    break;
                case ActionName.Selection:
                    _model.Select(pressedRow.OffsetToRoot, _row.OffsetToRoot, pressedColumn, _column);
                    break;
                case ActionName.Pointer:
                default:
                    break;
            }
        }
    }

    private void SetCustomGates(Gate gate)
    {
        _model.Steps[_column].SetGate(gate);
        _model.AddStepAfter(_column);
    }

    public void ChangeAngle(double gamma)
    {
        Gate oldGate = _model.Steps[_column].Gates[_row.OffsetToRoot];
        switch (oldGate.Name)
        {
            case GateName.RotateX:
                _model.Steps[_column].SetGate(new RotateXGate(gamma, oldGate.Target, oldGate.Control));
                break;
            case GateName.RotateY:
                _model.Steps[_column].SetGate(new RotateYGate(gamma, oldGate.Target, oldGate.Control));
                break;
            case GateName.RotateZ:
                _model.Steps[_column].SetGate(new RotateZGate(gamma, oldGate.Target, oldGate.Control));
                break;
            case GateName.PhaseKick:
                _model.Steps[_column]
                    .SetGate(new PhaseKickGate(gamma, oldGate.Target, (oldGate as PhaseKickGate).Controls));
                break;
            default:
                _model.Steps[_column].SetGate(new PhaseScaleGate(gamma, oldGate.Target, oldGate.Control));
                break;
        }

        OnPropertyChanged(nameof(Value));
    }

    public void ChangePhaseDist(int dist)
    {
        Gate oldGate = _model.Steps[_column].Gates[_row.OffsetToRoot];
        switch (oldGate.Name)
        {
            case GateName.CPhaseShift:
            {
                CPhaseShiftGate cps = oldGate as CPhaseShiftGate;
                _model.Steps[_column].SetGate(new CPhaseShiftGate(dist, oldGate.Target, cps.Controls));
                break;
            }
            case GateName.InvCPhaseShift:
            {
                InvCPhaseShiftGate icps = oldGate as InvCPhaseShiftGate;
                _model.Steps[_column].SetGate(new InvCPhaseShiftGate(dist, oldGate.Target, icps.Controls));
                break;
            }
        }

        OnPropertyChanged(nameof(Value));
    }

    public void ChangeUnitaryMatrix(Complex[,] matrix)
    {
        Gate oldGate = _model.Steps[_column].Gates[_row.OffsetToRoot];
        _model.Steps[_column].SetGate(new UnitaryGate(matrix, oldGate.Target, oldGate.Control));
        OnPropertyChanged(nameof(Value));
    }

    public void ChangeParametricParams(object[] parameters)
    {
        Gate oldGate = _model.Steps[_column].Gates[_row.OffsetToRoot];
        ParametricGate cg = oldGate as ParametricGate;
        CircuitEvaluator eval = CircuitEvaluator.GetInstance();
        try
        {
            ParametricGate newCg = eval.CreateParametricGate(cg.Method, parameters);

            for (int i = oldGate.Begin; i <= oldGate.End; i++)
            {
                RegisterRefModel gateRef = _model.GetRefFromOffset(i);
                Gate newGate = new EmptyGate(gateRef);
                _model.Steps[_column].SetGate(newGate);
            }

            if (_model.Steps[_column].HasPlace(newCg.Begin, newCg.End))
            {
                _model.Steps[_column].SetGate(newCg);
                _model.Select(newCg.Begin, newCg.End, _column, _column);
                _model.AddStepAfter(_column);
            }
            else
            {
                _model.InsertStepRight(_column);
                _model.Steps[_column + 1].SetGate(newCg);
                _model.Select(newCg.Begin, newCg.End, _column, _column + 1);
                _model.AddStepAfter(_column + 1);
            }
        }
        catch (Exception ex)
        {
            string msg = "Unable to add gate. The parameters are invalid.\n" +
                         "Inner exception:\n" +
                         (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            ErrorMessageHelper.ShowMessage(msg);
        }
    }

    public void ChangeCompositeTarget(RegisterPartModel target)
    {
        Gate oldGate = _model.Steps[_column].Gates[_row.OffsetToRoot];
        CompositeGate cg = oldGate as CompositeGate;
        CompositeGate newCg = new CompositeGate(cg.FunctionName, target);

        for (int i = oldGate.Begin; i <= oldGate.End; i++)
        {
            RegisterRefModel gateRef = _model.GetRefFromOffset(i);
            Gate newGate = new EmptyGate(gateRef);
            _model.Steps[_column].SetGate(newGate);
        }

        if (_model.Steps[_column].HasPlace(newCg.Begin, newCg.End))
        {
            _model.Steps[_column].SetGate(newCg);
            _model.AddStepAfter(_column);
        }
        else
        {
            _model.InsertStepRight(_column);
            _model.Steps[_column + 1].SetGate(newCg);
            _model.AddStepAfter(_column + 1);
        }
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(GateText));
        OnPropertyChanged(nameof(GateHeight));
        OnPropertyChanged(nameof(GateTextHeight));
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(GateImage));
        OnPropertyChanged(nameof(BackgroundImage));
    }

    public void RefreshSelection(bool isSelected)
    {
        _isSelected = isSelected;
        OnPropertyChanged(nameof(SelectionOpacity));
    }

    public void InsertRowAbove(object parameter)
    {
        _model.InsertQubitAbove(_row.Register.Index, _row.Offset);
    }

    public void InsertRowBelow(object parameter)
    {
        _model.InsertQubitBelow(_row.Register.Index, _row.Offset);
    }

    public void InsertColumnLeft(object parameter)
    {
        _model.InsertStepLeft(_column);
    }

    public void InsertColumnRight(object parameter)
    {
        _model.InsertStepRight(_column);
    }

    public void DeleteRow(object parameter)
    {
        if (_model.Registers[_row.Register.Index].Qubits.Count > 1)
        {
            _model.DeleteQubit(_row.Register.Index, _row.Offset);
        }
        else
        {
            _model.DeleteRegister(_row.Register.Index);
        }
    }

    public void DeleteColumn(object parameter)
    {
        _model.DeleteStep(_column);
    }

    public void UpdateDeleteRowCommand(bool canExecute)
    {
        _deleteRow = new DelegateCommand(DeleteRow, x => canExecute);
        OnPropertyChanged(nameof(DeleteRowCommand));
    }

    public void UpdateDeleteColumnCommand(bool canExecute)
    {
        _deleteColumn = new DelegateCommand(DeleteColumn, x => canExecute);
        OnPropertyChanged(nameof(DeleteColumnCommand));
    }

    #endregion // Internal Public Methods


    #region Private Helpers

    private void _model_CurrentStepChanged(object? sender, EventArgs eventArgs)
    {
        _isEnabled = (_model.CurrentStep <= _column);
        OnPropertyChanged(nameof(IsEnabled));
    }

    #endregion // Private Helpers
}