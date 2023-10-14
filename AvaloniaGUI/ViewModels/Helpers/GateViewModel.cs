#region

using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
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
    #region Constructor

    public GateViewModel(ComputerModel model, RegisterRefModel row, int column, DialogManager dialogManager)
    {
        _model = model;
        _row = row;
        Column = column;
        _model.StepChanged += _model_CurrentStepChanged;

        _dialogManager = dialogManager;
    }

    #endregion // Constructor


    #region Private Helpers

    private void _model_CurrentStepChanged(object? sender, EventArgs eventArgs)
    {
        IsEnabled = _model.CurrentStep <= Column;
        OnPropertyChanged(nameof(IsEnabled));
    }

    #endregion // Private Helpers

    #region Fields

    private readonly ComputerModel _model;

    private RegisterRefModel _row;

    private bool _isSelected;

    private DelegateCommand _insertRowAbove;
    private DelegateCommand _insertRowBelow;
    private DelegateCommand _insertColumnLeft;
    private DelegateCommand _insertColumnRight;
    private DelegateCommand _deleteRow;
    private DelegateCommand _deleteColumn;

    private readonly DialogManager _dialogManager;

    #endregion // Fields


    #region Model Properties

    public Gate Value => _model.Steps[Column].Gates[_row.OffsetToRoot];

    public RegisterRefModel Row => _row;

    public int Column { get; private set; }

    #endregion // Model Properties


    #region Presentation Properties

    public string GateText
    {
        get
        {
            if (Value is not CustomGate) return string.Empty;

            var cg = Value as CustomGate;

            // text should only be displayed in the middle gate
            return (cg.End + cg.Begin + 1) / 2 == _row.OffsetToRoot ? cg.FunctionName : string.Empty;
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
            if (Value is not CustomGate) return CircuitGridViewModel.GateHeight;

            var endMinusBegin = Value.End - Value.Begin;
            return CircuitGridViewModel.GateHeight + (endMinusBegin + 1) / 2 * CircuitGridViewModel.QubitSize;
        }
    }


    public double SelectionOpacity => _isSelected ? 0.25 : 0;

    public VisualBrush BackgroundImage
    {
        get
        {
            var gate = Value;
            var brush = SwitchBackgroundBrush(gate);
            //RenderOptions.SetBitmapInterpolationMode(brush,BitmapInterpolationMode.LowQuality));
            return brush;
        }
    }

    private VisualBrush SwitchBackgroundBrush(Gate gate)
    {
        if (gate.Control.HasValue)
        {
            if (_row.OffsetToRoot == gate.Begin)
            {
                if (_row.Equals(gate.Control.Value)) return Application.Current.FindResource("ImgDownC") as VisualBrush;

                return Application.Current.FindResource("ImgDown") as VisualBrush;
            }

            if (_row.OffsetToRoot != gate.End) return Application.Current.FindResource("ImgLine") as VisualBrush;

            if (_row.Equals(gate.Control.Value)) return Application.Current.FindResource("ImgUpC") as VisualBrush;

            return Application.Current.FindResource("ImgUp") as VisualBrush;
        }

        if (gate is not MultiControlledGate) return Application.Current.FindResource("ImgEmpty") as VisualBrush;

        if (_row.OffsetToRoot == gate.Begin)
        {
            if (_row.OffsetToRoot != gate.End) return Application.Current.FindResource("ImgDown") as VisualBrush;

            return Application.Current.FindResource("ImgEmpty") as VisualBrush;
        }

        if (_row.OffsetToRoot == gate.End) return Application.Current.FindResource("ImgUp") as VisualBrush;

        return Application.Current.FindResource("ImgLine") as VisualBrush;
    }

    public VisualBrush? GateImage
    {
        get
        {
            var gate = Value;


            if (gate.Name == GateName.Empty) return null;

            if (gate is CustomGate cg)
            {
                if (_row.OffsetToRoot == cg.Begin)
                {
                    if (_row.OffsetToRoot == cg.End)
                        return Application.Current.FindResource("ToolComposite") as VisualBrush;

                    return Application.Current.FindResource("DownComposite") as VisualBrush;
                }

                if (_row.OffsetToRoot == cg.End) return Application.Current.FindResource("UpComposite") as VisualBrush;

                return Application.Current.FindResource("CenterComposite") as VisualBrush;
            }

            if (gate.Name == GateName.Measure) return Application.Current.FindResource("ToolMeasure") as VisualBrush;

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

            if (controlledGate.Controls.Contains(_row)) return Application.Current.FindResource("ImgC") as VisualBrush;

            return Application.Current.FindResource("ImgLine") as VisualBrush;
        }
    }

    public bool IsEnabled { get; private set; } = true;

    public ICommand InsertRowAboveCommand
    {
        get
        {
            if (_insertRowAbove == null) _insertRowAbove = new DelegateCommand(InsertRowAbove, x => true);

            return _insertRowAbove;
        }
    }

    public ICommand InsertRowBelowCommand
    {
        get
        {
            if (_insertRowBelow == null) _insertRowBelow = new DelegateCommand(InsertRowBelow, x => true);

            return _insertRowBelow;
        }
    }

    public ICommand InsertColumnLeftCommand
    {
        get
        {
            if (_insertColumnLeft == null) _insertColumnLeft = new DelegateCommand(InsertColumnLeft, x => true);

            return _insertColumnLeft;
        }
    }

    public ICommand InsertColumnRightCommand
    {
        get
        {
            if (_insertColumnRight == null) _insertColumnRight = new DelegateCommand(InsertColumnRight, x => true);

            return _insertColumnRight;
        }
    }

    public ICommand DeleteRowCommand
    {
        get
        {
            if (_deleteRow == null) _deleteRow = new DelegateCommand(DeleteRow, x => true);

            return _deleteRow;
        }
    }

    public ICommand DeleteColumnCommand
    {
        get
        {
            if (_deleteColumn == null) _deleteColumn = new DelegateCommand(DeleteColumn, x => true);

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
        Column += columnDelta;
        Refresh();
    }

    public async void SetGate(int pressedColumn, RegisterRefModel pressedRow, KeyModifiers keyStates)
    {
        var action = MainWindowViewModel.SelectedAction;

        // make selection
        if (keyStates.HasFlag(KeyModifiers.Shift))
        {
            // move selection
            if (keyStates.HasFlag(KeyModifiers.Control))
            {
                if (!_model.IsSelected(pressedRow.OffsetToRoot, pressedColumn)) return;

                _model.Cut();
                _model.Select(_row.OffsetToRoot, _row.OffsetToRoot, Column, Column);
                _model.Paste();
            }
            else
            {
                _model.Select(pressedRow.OffsetToRoot, _row.OffsetToRoot, pressedColumn, Column);
            }
        }
        else
        {
            Gate oldGate;
            switch (action)
            {
                case ActionName.Empty:
                    oldGate = _model.Steps[Column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name != GateName.Empty)
                    {
                        for (var i = oldGate.Begin; i <= oldGate.End; i++)
                        {
                            var gateRef = _model.GetRefFromOffset(i);
                            Gate newGate = new EmptyGate(gateRef);
                            _model.Steps[Column].SetGate(newGate);
                        }

                        if (oldGate is MultiControlledGate mcg)
                        {
                            if (mcg.Controls.Contains(_row))
                            {
                                RegisterRefModel[] toRemove = { _row };
                                var newControls =
                                    mcg.Controls.Except(toRemove).ToArray();
                                Gate toAdd;
                                switch (mcg.Name)
                                {
                                    case GateName.PhaseKick:
                                    {
                                        var pk = mcg as PhaseKickGate;
                                        toAdd = new PhaseKickGate(pk.Gamma, pk.Target, newControls);
                                        break;
                                    }
                                    case GateName.CPhaseShift:
                                    {
                                        var cps = mcg as CPhaseShiftGate;
                                        toAdd = new CPhaseShiftGate(cps.Dist, cps.Target, newControls);
                                        break;
                                    }
                                    case GateName.InvCPhaseShift:
                                    {
                                        var icps = mcg as InvCPhaseShiftGate;
                                        toAdd = new InvCPhaseShiftGate(icps.Dist, icps.Target, newControls);
                                        break;
                                    }
                                    // Toffoli
                                    default:
                                    {
                                        if (newControls.Length > 1)
                                            toAdd = new ToffoliGate(mcg.Target, newControls);
                                        else
                                            toAdd = new CNotGate(mcg.Target, newControls[0]);

                                        break;
                                    }
                                }

                                _model.Steps[Column].SetGate(toAdd);
                            }
                        }
                        else if (oldGate.Name == GateName.CNot)
                        {
                            if (oldGate.Control.Value.OffsetToRoot == _row.OffsetToRoot)
                            {
                                Gate toAdd = new SigmaXGate(oldGate.Target);
                                _model.Steps[Column].SetGate(toAdd);
                            }
                        }
                        else if (oldGate is SingleGate { Control: not null } sg)
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
                                }

                                _model.Steps[Column].SetGate(toAdd);
                            }
                        }
                    }

                    break;
                case ActionName.Hadamard:
                    oldGate = _model.Steps[Column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name == GateName.Empty)
                    {
                        _model.Steps[Column].SetGate(new HadamardGate(_row));
                        _model.AddStepAfter(Column);
                    }

                    break;
                case ActionName.SigmaX:
                    oldGate = _model.Steps[Column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name == GateName.Empty)
                    {
                        _model.Steps[Column].SetGate(new SigmaXGate(_row));
                        _model.AddStepAfter(Column);
                    }

                    break;
                case ActionName.SigmaY:
                    oldGate = _model.Steps[Column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name == GateName.Empty)
                    {
                        _model.Steps[Column].SetGate(new SigmaYGate(_row));
                        _model.AddStepAfter(Column);
                    }

                    break;
                case ActionName.SigmaZ:
                    oldGate = _model.Steps[Column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name == GateName.Empty)
                    {
                        _model.Steps[Column].SetGate(new SigmaZGate(_row));
                        _model.AddStepAfter(Column);
                    }

                    break;
                case ActionName.SqrtX:
                    oldGate = _model.Steps[Column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name == GateName.Empty)
                    {
                        _model.Steps[Column].SetGate(new SqrtXGate(_row));
                        _model.AddStepAfter(Column);
                    }

                    break;
                case ActionName.RotateX:
                case ActionName.RotateY:
                case ActionName.RotateZ:
                case ActionName.PhaseKick:
                case ActionName.PhaseScale:
                    oldGate = _model.Steps[Column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name == GateName.Empty)
                    {
                        var gammaVM = new GammaInputViewModel();

                        await _dialogManager.ShowDialogAsync(new GammaInput(gammaVM), () =>
                        {
                            var gamma = gammaVM.Gamma;
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
                    oldGate = _model.Steps[Column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name == GateName.Empty)
                    {
                        var matrixVM = new MatrixInputViewModel();

                        await _dialogManager.ShowDialogAsync(new MatrixInput(matrixVM), () =>
                        {
                            var matrix = matrixVM.Matrix;
                            if (matrix == null) return;

                            _model.Steps[Column].SetGate(new UnitaryGate(matrix, _row));
                            _model.AddStepAfter(Column);
                        });
                    }

                    break;
                case ActionName.Control:
                    if (pressedColumn == Column)
                    {
                        oldGate = _model.Steps[Column].Gates[_row.OffsetToRoot];
                        if (!_row.Equals(pressedRow))
                            switch (oldGate.Name)
                            {
                                case GateName.CNot:
                                {
                                    var oldCnot = oldGate as CNotGate;
                                    if (pressedRow.OffsetToRoot < oldGate.Begin)
                                    {
                                        if (_model.Steps[Column].HasPlace(pressedRow.OffsetToRoot, oldGate.Begin - 1))
                                            _model.Steps[Column].SetGate(new ToffoliGate(oldCnot.Target,
                                                oldCnot.Control.Value, pressedRow));
                                    }
                                    else if (pressedRow.OffsetToRoot > oldGate.End)
                                    {
                                        if (_model.Steps[Column].HasPlace(oldGate.End + 1, pressedRow.OffsetToRoot))
                                            _model.Steps[Column].SetGate(new ToffoliGate(oldCnot.Target,
                                                oldCnot.Control.Value, pressedRow));
                                    }
                                    else // new Control inside
                                    {
                                        //check if not doubled
                                        if (_row.OffsetToRoot != oldCnot.Target.OffsetToRoot &&
                                            _row.OffsetToRoot != oldCnot.Control.Value.OffsetToRoot)
                                            _model.Steps[Column].SetGate(new ToffoliGate(oldCnot.Target,
                                                oldCnot.Control.Value, _row));
                                    }

                                    break;
                                }
                                case GateName.Toffoli:
                                {
                                    var oldT = oldGate as ToffoliGate;
                                    if (pressedRow.OffsetToRoot < oldGate.Begin)
                                    {
                                        if (_model.Steps[Column].HasPlace(pressedRow.OffsetToRoot, oldGate.Begin - 1))
                                            _model.Steps[Column]
                                                .SetGate(new ToffoliGate(oldT.Target, pressedRow, oldT.Controls));
                                    }
                                    else if (pressedRow.OffsetToRoot > oldGate.End)
                                    {
                                        if (_model.Steps[Column].HasPlace(oldGate.End + 1, pressedRow.OffsetToRoot))
                                            _model.Steps[Column]
                                                .SetGate(new ToffoliGate(oldT.Target, pressedRow, oldT.Controls));
                                    }
                                    else // new Control inside
                                    {
                                        //check if not doubled
                                        if (_row.OffsetToRoot != oldT.Target.OffsetToRoot &&
                                            !oldT.Controls.Contains(_row))
                                            _model.Steps[Column]
                                                .SetGate(new ToffoliGate(oldT.Target, _row, oldT.Controls));
                                    }

                                    break;
                                }
                                case GateName.CPhaseShift:
                                {
                                    var oldT = oldGate as CPhaseShiftGate;
                                    if (pressedRow.OffsetToRoot < oldGate.Begin)
                                    {
                                        if (_model.Steps[Column].HasPlace(pressedRow.OffsetToRoot, oldGate.Begin - 1))
                                        {
                                            var cList = oldT.Controls.ToList();
                                            cList.Add(pressedRow);
                                            var cParams = cList.ToArray<RegisterRefModel>();
                                            _model.Steps[Column]
                                                .SetGate(new CPhaseShiftGate(oldT.Dist, oldT.Target, cParams));
                                        }
                                    }
                                    else if (pressedRow.OffsetToRoot > oldGate.End)
                                    {
                                        if (_model.Steps[Column].HasPlace(oldGate.End + 1, pressedRow.OffsetToRoot))
                                        {
                                            var cList = oldT.Controls.ToList();
                                            cList.Add(pressedRow);
                                            var cParams = cList.ToArray<RegisterRefModel>();
                                            _model.Steps[Column]
                                                .SetGate(new CPhaseShiftGate(oldT.Dist, oldT.Target, cParams));
                                        }
                                    }
                                    else // new Control inside
                                    {
                                        var cList = oldT.Controls.ToList();
                                        cList.Add(_row);
                                        var cParams = cList.ToArray<RegisterRefModel>();
                                        _model.Steps[Column]
                                            .SetGate(new CPhaseShiftGate(oldT.Dist, oldT.Target, cParams));
                                    }

                                    break;
                                }
                                case GateName.InvCPhaseShift:
                                {
                                    var oldT = oldGate as InvCPhaseShiftGate;
                                    if (pressedRow.OffsetToRoot < oldGate.Begin)
                                    {
                                        if (_model.Steps[Column].HasPlace(pressedRow.OffsetToRoot, oldGate.Begin - 1))
                                        {
                                            var cList = oldT.Controls.ToList();
                                            cList.Add(pressedRow);
                                            var cParams = cList.ToArray<RegisterRefModel>();
                                            _model.Steps[Column]
                                                .SetGate(new InvCPhaseShiftGate(oldT.Dist, oldT.Target, cParams));
                                        }
                                    }
                                    else if (pressedRow.OffsetToRoot > oldGate.End)
                                    {
                                        if (_model.Steps[Column].HasPlace(oldGate.End + 1, pressedRow.OffsetToRoot))
                                        {
                                            var cList = oldT.Controls.ToList();
                                            cList.Add(pressedRow);
                                            var cParams = cList.ToArray<RegisterRefModel>();
                                            _model.Steps[Column]
                                                .SetGate(new InvCPhaseShiftGate(oldT.Dist, oldT.Target, cParams));
                                        }
                                    }
                                    else // new Control inside
                                    {
                                        var cList = oldT.Controls.ToList();
                                        cList.Add(_row);
                                        var cParams = cList.ToArray<RegisterRefModel>();
                                        _model.Steps[Column]
                                            .SetGate(new InvCPhaseShiftGate(oldT.Dist, oldT.Target, cParams));
                                    }

                                    break;
                                }
                                case GateName.PhaseKick:
                                {
                                    var oldT = oldGate as PhaseKickGate;
                                    if (pressedRow.OffsetToRoot < oldGate.Begin)
                                    {
                                        if (_model.Steps[Column].HasPlace(pressedRow.OffsetToRoot, oldGate.Begin - 1))
                                        {
                                            var cList = oldT.Controls.ToList();
                                            cList.Add(pressedRow);
                                            var cParams = cList.ToArray<RegisterRefModel>();
                                            _model.Steps[Column]
                                                .SetGate(new PhaseKickGate(oldT.Gamma, oldT.Target, cParams));
                                        }
                                    }
                                    else if (pressedRow.OffsetToRoot > oldGate.End)
                                    {
                                        if (_model.Steps[Column].HasPlace(oldGate.End + 1, pressedRow.OffsetToRoot))
                                        {
                                            var cList = oldT.Controls.ToList();
                                            cList.Add(pressedRow);
                                            var cParams = cList.ToArray<RegisterRefModel>();
                                            _model.Steps[Column]
                                                .SetGate(new PhaseKickGate(oldT.Gamma, oldT.Target, cParams));
                                        }
                                    }
                                    else // new Control inside
                                    {
                                        var cList = oldT.Controls.ToList();
                                        cList.Add(_row);
                                        var cParams = cList.ToArray<RegisterRefModel>();
                                        _model.Steps[Column]
                                            .SetGate(new PhaseKickGate(oldT.Gamma, oldT.Target, cParams));
                                    }

                                    break;
                                }
                                default:
                                {
                                    if (oldGate.Control == null)
                                    {
                                        var lastEmptyRow = _row.OffsetToRoot - 1;
                                        if (pressedRow.OffsetToRoot > _row.OffsetToRoot) lastEmptyRow += 2;

                                        if (_model.Steps[Column].HasPlace(pressedRow.OffsetToRoot, lastEmptyRow))
                                            switch (oldGate.Name)
                                            {
                                                case GateName.Hadamard:
                                                    _model.Steps[Column].SetGate(new HadamardGate(_row, pressedRow));
                                                    break;
                                                case GateName.PhaseScale:
                                                    var oldPs = oldGate as PhaseScaleGate;
                                                    _model.Steps[Column]
                                                        .SetGate(new PhaseScaleGate(oldPs.Gamma, _row, pressedRow));
                                                    break;
                                                case GateName.RotateX:
                                                    var oldRx = oldGate as RotateXGate;
                                                    _model.Steps[Column]
                                                        .SetGate(new RotateXGate(oldRx.Gamma, _row, pressedRow));
                                                    break;
                                                case GateName.RotateY:
                                                    var oldRy = oldGate as RotateYGate;
                                                    _model.Steps[Column]
                                                        .SetGate(new RotateYGate(oldRy.Gamma, _row, pressedRow));
                                                    break;
                                                case GateName.RotateZ:
                                                    var oldRz = oldGate as RotateZGate;
                                                    _model.Steps[Column]
                                                        .SetGate(new RotateZGate(oldRz.Gamma, _row, pressedRow));
                                                    break;
                                                case GateName.SigmaX:
                                                    _model.Steps[Column].SetGate(new CNotGate(_row, pressedRow));
                                                    break;
                                                case GateName.SigmaY:
                                                    _model.Steps[Column].SetGate(new SigmaYGate(_row, pressedRow));
                                                    break;
                                                case GateName.SigmaZ:
                                                    _model.Steps[Column].SetGate(new SigmaZGate(_row, pressedRow));
                                                    break;
                                                case GateName.SqrtX:
                                                    _model.Steps[Column].SetGate(new SqrtXGate(_row, pressedRow));
                                                    break;
                                                case GateName.Unitary:
                                                    var oldU = oldGate as UnitaryGate;
                                                    _model.Steps[Column]
                                                        .SetGate(new UnitaryGate(oldU.Matrix, _row, pressedRow));
                                                    break;
                                                case GateName.Empty:
                                                    _model.Steps[Column].SetGate(new CNotGate(_row, pressedRow));
                                                    _model.AddStepAfter(Column);
                                                    break;
                                            }
                                    }

                                    break;
                                }
                            }
                        else // add control inside (only CNot or Toffoli)
                            switch (oldGate.Name)
                            {
                                case GateName.CNot:
                                {
                                    var oldCnot = oldGate as CNotGate;
                                    //check if not doubled
                                    if (_row.OffsetToRoot != oldCnot.Target.OffsetToRoot &&
                                        _row.OffsetToRoot != oldCnot.Control.Value.OffsetToRoot)
                                        _model.Steps[Column]
                                            .SetGate(new ToffoliGate(oldCnot.Target, _row, oldCnot.Control.Value));

                                    break;
                                }
                                case GateName.Toffoli:
                                {
                                    var oldT = oldGate as ToffoliGate;
                                    //check if not doubled
                                    if (_row.OffsetToRoot != oldT.Target.OffsetToRoot &&
                                        !oldT.Controls.Contains(_row))
                                        _model.Steps[Column]
                                            .SetGate(new ToffoliGate(oldT.Target, _row, oldT.Controls));

                                    break;
                                }
                                case GateName.CPhaseShift:
                                {
                                    var oldT = oldGate as CPhaseShiftGate;
                                    //check if not doubled
                                    if (_row.OffsetToRoot != oldT.Target.OffsetToRoot &&
                                        !oldT.Controls.Contains(_row))
                                    {
                                        var cList = oldT.Controls.ToList();
                                        cList.Add(_row);
                                        var cParams = cList.ToArray<RegisterRefModel>();
                                        _model.Steps[Column]
                                            .SetGate(new CPhaseShiftGate(oldT.Dist, oldT.Target, cParams));
                                    }

                                    break;
                                }
                                case GateName.InvCPhaseShift:
                                {
                                    var oldT = oldGate as InvCPhaseShiftGate;
                                    //check if not doubled
                                    if (_row.OffsetToRoot != oldT.Target.OffsetToRoot &&
                                        !oldT.Controls.Contains(_row))
                                    {
                                        var cList = oldT.Controls.ToList();
                                        cList.Add(_row);
                                        var cParams = cList.ToArray<RegisterRefModel>();
                                        _model.Steps[Column]
                                            .SetGate(new InvCPhaseShiftGate(oldT.Dist, oldT.Target, cParams));
                                    }

                                    break;
                                }
                                case GateName.PhaseKick:
                                {
                                    var oldT = oldGate as PhaseKickGate;
                                    //check if not doubled
                                    if (_row.OffsetToRoot != oldT.Target.OffsetToRoot &&
                                        !oldT.Controls.Contains(_row))
                                    {
                                        var cList = oldT.Controls.ToList();
                                        cList.Add(_row);
                                        var cParams = cList.ToArray<RegisterRefModel>();
                                        _model.Steps[Column]
                                            .SetGate(new PhaseKickGate(oldT.Gamma, oldT.Target, cParams));
                                    }

                                    break;
                                }
                            }
                    }

                    break;
                case ActionName.CPhaseShift:
                    await SetCPhaseShift(pressedColumn, pressedRow, false);

                    break;
                case ActionName.InvCPhaseShift:
                    await SetCPhaseShift(pressedColumn, pressedRow, true);

                    break;
                case ActionName.Measure:
                    if (pressedColumn == Column)
                        if (_model.Steps[Column].HasPlace(pressedRow.OffsetToRoot, _row.OffsetToRoot))
                        {
                            _model.Steps[Column].SetGate(new MeasureGate(pressedRow, _row));
                            _model.AddStepAfter(Column);
                        }

                    break;
                case ActionName.Ungroup:
                    oldGate = _model.Steps[Column].Gates[_row.OffsetToRoot];
                    switch (oldGate.Name)
                    {
                        case GateName.Parametric:
                        {
                            var eval = CircuitEvaluator.GetInstance();

                            try
                            {
                                _model.SetStepForGates(Column);

                                var oldColumns = _model.Steps.Count;
                                var oldBegin = oldGate.Begin;
                                var oldEnd = oldGate.End;

                                eval.Decompose(oldGate as ParametricGate);
                                var columnsAdded = _model.Steps.Count - oldColumns;

                                _model.ResetStepForGates();

                                //remove old composite gate
                                for (var i = oldGate.Begin; i <= oldGate.End; i++)
                                {
                                    var gateRef = _model.GetRefFromOffset(i);
                                    Gate newGate = new EmptyGate(gateRef);
                                    _model.Steps[Column].SetGate(newGate);
                                }

                                //delete step on _column (if it is empty)
                                var isEmpty = true;
                                var j = 0;
                                while (isEmpty && j < _model.Steps[Column].Gates.Count)
                                {
                                    if (_model.Steps[Column].Gates[j].Name != GateName.Empty) isEmpty = false;

                                    j++;
                                }

                                if (isEmpty)
                                {
                                    _model.DeleteStep(Column);
                                    _model.Select(oldBegin, oldEnd, Column, Column + columnsAdded - 1);
                                }
                                else
                                {
                                    _model.Select(oldBegin, oldEnd, Column + 1, Column + columnsAdded);
                                }
                            }
                            catch (Exception ex)
                            {
                                var msg = "Unable to ungroup gate. Its parameters are invalid.\n" +
                                          "Inner exception:\n" +
                                          (ex.InnerException == null ? ex.Message : ex.InnerException.Message);
                                SimpleDialogHandler.ShowSimpleMessage(msg);
                            }

                            break;
                        }
                        case GateName.Composite:
                        {
                            var cg = oldGate as CompositeGate;
                            var toAdd = _model.GetActualGates(cg);
                            var column = Column;
                            foreach (var g in toAdd.Where(g => g.Name != GateName.Empty))
                            {
                                _model.InsertStepRight(column);
                                column++;
                                _model.Steps[column].SetGate(g);
                            }

                            //remove old composite gate
                            for (var i = oldGate.Begin; i <= oldGate.End; i++)
                            {
                                var gateRef = _model.GetRefFromOffset(i);
                                Gate newGate = new EmptyGate(gateRef);
                                _model.Steps[Column].SetGate(newGate);
                            }

                            //delete step on _column (if it is empty)
                            var isEmpty = true;
                            var j = 0;
                            while (isEmpty && j < _model.Steps[Column].Gates.Count)
                            {
                                if (_model.Steps[Column].Gates[j].Name != GateName.Empty) isEmpty = false;

                                j++;
                            }

                            if (isEmpty) _model.DeleteStep(Column);

                            break;
                        }
                    }

                    break;
                case ActionName.Composite:
                    oldGate = _model.Steps[Column].Gates[_row.OffsetToRoot];
                    if (oldGate.Name == GateName.Empty)
                    {
                        var eval = CircuitEvaluator.GetInstance();
                        var dict = eval.GetExtensionGates();

                        if (string.IsNullOrWhiteSpace(MainWindowViewModel.SelectedCompositeStatic)) break;

                        var parametricInputVM = new ParametricInputViewModel(
                            MainWindowViewModel.SelectedCompositeStatic, dict,
                            _model.CompositeGates);
                        await _dialogManager.ShowDialogAsync(new ParametricInput(parametricInputVM), () =>
                        {
                            try
                            {
                                if (!parametricInputVM.IsValid) return;

                                if (parametricInputVM.Method != null)
                                {
                                    var cg = eval.CreateParametricGate(parametricInputVM.Method,
                                        parametricInputVM.ParamValues);
                                    _model.Steps[Column].SetGate(cg);
                                    _model.AddStepAfter(Column);
                                }
                                else if (parametricInputVM.CopositeGateTarget != null)
                                {
                                    var minWidth = _model.MinWidthForComposite(parametricInputVM.FunctionName);
                                    if (parametricInputVM.CopositeGateTarget.Value.Width < minWidth)
                                    {
                                        var sb =
                                            new StringBuilder("Entered parameter has too small width.\n");
                                        sb.Append("Entered width: ");
                                        sb.Append(parametricInputVM.CopositeGateTarget.Value.Width).AppendLine();
                                        sb.Append("Minimum width: ");
                                        sb.Append(minWidth).AppendLine();
                                        throw new Exception(sb.ToString());
                                    }

                                    var cg = new CompositeGate(parametricInputVM.FunctionName,
                                        parametricInputVM.CopositeGateTarget.Value);
                                    _model.Steps[Column].SetGate(cg);
                                    _model.AddStepAfter(Column);
                                }
                            }
                            catch (Exception ex)
                            {
                                SimpleDialogHandler.ShowSimpleMessage(ex.Message);
                            }
                        });
                    }

                    break;
                case ActionName.Selection:
                    _model.Select(pressedRow.OffsetToRoot, _row.OffsetToRoot, pressedColumn, Column);
                    break;
                case ActionName.Pointer:
                default:
                    break;
            }
        }
    }

    private async Task SetCPhaseShift(int pressedColumn, RegisterRefModel pressedRow, bool inverse)
    {
        if (pressedColumn != Column) return;

        if (!_model.Steps[Column].HasPlace(pressedRow.OffsetToRoot, _row.OffsetToRoot)) return;

        var phaseDistInputVM = new PhaseDistInputViewModel
        {
            DistText = Math.Abs(pressedRow.OffsetToRoot - _row.OffsetToRoot).ToString()
        };

        await _dialogManager.ShowDialogAsync(new PhaseDistInput(phaseDistInputVM), () =>
        {
            var dist = phaseDistInputVM.Dist;
            if (!dist.HasValue) return;

            if (inverse)
                _model.Steps[Column].SetGate(_row.Equals(pressedRow)
                    ? new InvCPhaseShiftGate(dist.Value, _row)
                    : new InvCPhaseShiftGate(dist.Value, _row, pressedRow));
            else
                _model.Steps[Column].SetGate(_row.Equals(pressedRow)
                    ? new CPhaseShiftGate(dist.Value, _row)
                    : new CPhaseShiftGate(dist.Value, _row, pressedRow));

            _model.AddStepAfter(Column);
        });
    }

    private void SetCustomGates(Gate gate)
    {
        _model.Steps[Column].SetGate(gate);
        _model.AddStepAfter(Column);
    }

    public void ChangeAngle(double gamma)
    {
        var oldGate = _model.Steps[Column].Gates[_row.OffsetToRoot];
        switch (oldGate.Name)
        {
            case GateName.RotateX:
                _model.Steps[Column].SetGate(new RotateXGate(gamma, oldGate.Target, oldGate.Control));
                break;
            case GateName.RotateY:
                _model.Steps[Column].SetGate(new RotateYGate(gamma, oldGate.Target, oldGate.Control));
                break;
            case GateName.RotateZ:
                _model.Steps[Column].SetGate(new RotateZGate(gamma, oldGate.Target, oldGate.Control));
                break;
            case GateName.PhaseKick:
                _model.Steps[Column]
                    .SetGate(new PhaseKickGate(gamma, oldGate.Target, (oldGate as PhaseKickGate).Controls));
                break;
            default:
                _model.Steps[Column].SetGate(new PhaseScaleGate(gamma, oldGate.Target, oldGate.Control));
                break;
        }

        OnPropertyChanged(nameof(Value));
    }

    public void ChangePhaseDist(int dist)
    {
        var oldGate = _model.Steps[Column].Gates[_row.OffsetToRoot];
        switch (oldGate.Name)
        {
            case GateName.CPhaseShift:
            {
                var cps = oldGate as CPhaseShiftGate;
                _model.Steps[Column].SetGate(new CPhaseShiftGate(dist, oldGate.Target, cps.Controls));
                break;
            }
            case GateName.InvCPhaseShift:
            {
                var icps = oldGate as InvCPhaseShiftGate;
                _model.Steps[Column].SetGate(new InvCPhaseShiftGate(dist, oldGate.Target, icps.Controls));
                break;
            }
        }

        OnPropertyChanged(nameof(Value));
    }

    public void ChangeUnitaryMatrix(Complex[,] matrix)
    {
        var oldGate = _model.Steps[Column].Gates[_row.OffsetToRoot];
        _model.Steps[Column].SetGate(new UnitaryGate(matrix, oldGate.Target, oldGate.Control));
        OnPropertyChanged(nameof(Value));
    }

    public void ChangeParametricParams(object[] parameters)
    {
        var oldGate = _model.Steps[Column].Gates[_row.OffsetToRoot];
        var cg = oldGate as ParametricGate;
        var eval = CircuitEvaluator.GetInstance();
        try
        {
            var newCg = eval.CreateParametricGate(cg.Method, parameters);

            for (var i = oldGate.Begin; i <= oldGate.End; i++)
            {
                var gateRef = _model.GetRefFromOffset(i);
                Gate newGate = new EmptyGate(gateRef);
                _model.Steps[Column].SetGate(newGate);
            }

            if (_model.Steps[Column].HasPlace(newCg.Begin, newCg.End))
            {
                _model.Steps[Column].SetGate(newCg);
                _model.Select(newCg.Begin, newCg.End, Column, Column);
                _model.AddStepAfter(Column);
            }
            else
            {
                _model.InsertStepRight(Column);
                _model.Steps[Column + 1].SetGate(newCg);
                _model.Select(newCg.Begin, newCg.End, Column, Column + 1);
                _model.AddStepAfter(Column + 1);
            }
        }
        catch (Exception ex)
        {
            var msg = "Unable to add gate. The parameters are invalid.\n" +
                      "Inner exception:\n" +
                      (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            SimpleDialogHandler.ShowSimpleMessage(msg);
        }
    }

    public void ChangeCompositeTarget(RegisterPartModel target)
    {
        var oldGate = _model.Steps[Column].Gates[_row.OffsetToRoot];
        var cg = oldGate as CompositeGate;
        var newCg = new CompositeGate(cg.FunctionName, target);

        for (var i = oldGate.Begin; i <= oldGate.End; i++)
        {
            var gateRef = _model.GetRefFromOffset(i);
            Gate newGate = new EmptyGate(gateRef);
            _model.Steps[Column].SetGate(newGate);
        }

        if (_model.Steps[Column].HasPlace(newCg.Begin, newCg.End))
        {
            _model.Steps[Column].SetGate(newCg);
            _model.AddStepAfter(Column);
        }
        else
        {
            _model.InsertStepRight(Column);
            _model.Steps[Column + 1].SetGate(newCg);
            _model.AddStepAfter(Column + 1);
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
        _model.InsertStepLeft(Column);
    }

    public void InsertColumnRight(object parameter)
    {
        _model.InsertStepRight(Column);
    }

    public void DeleteRow(object parameter)
    {
        if (_model.Registers[_row.Register.Index].Qubits.Count > 1)
            _model.DeleteQubit(_row.Register.Index, _row.Offset);
        else
            _model.DeleteRegister(_row.Register.Index);
    }

    public void DeleteColumn(object parameter)
    {
        _model.DeleteStep(Column);
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
}