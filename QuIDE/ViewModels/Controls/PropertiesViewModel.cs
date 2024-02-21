#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media.Imaging;
using QuIDE.CodeHelpers;
using QuIDE.Properties;
using QuIDE.ViewModels.Helpers;
using Quantum.Helpers;
using QuIDE.QuantumModel;
using QuIDE.QuantumModel.Gates;
using QuIDE.QuantumParser;
using QuIDE.QuantumParser.Validation;

#endregion

namespace QuIDE.ViewModels.Controls;

public class PropertiesViewModel : ViewModelBase
{
    private enum SelectedType
    {
        State,
        RotateGate,
        PhaseShiftGate,
        UnitaryGate,
        CompositeGate,
        OtherGate,
        None
    }


    #region Fields

    private SelectedType _selectedType;
    private OutputState _selectedState;
    private GateViewModel _selectedGate;

    private const double _lineLength = 60;
    private static ComplexFormatter _complexFormatter = new();

    private DelegateCommand _selectUnit;
    private DelegateCommand _applyGamma;
    private bool _rad;
    private double _gammaRad;
    private string _gammaString;

    private Complex[,] _matrix = new Complex[2, 2] { { 0, 0 }, { 0, 0 } };
    private bool _isUnitary;
    private string _a00Text = string.Empty;
    private string _a01Text = string.Empty;
    private string _a10Text = string.Empty;
    private string _a11Text = string.Empty;
    private DelegateCommand _applyMatrix;
    private DelegateCommand _cancelMatrix;

    private int _dist;
    private string _distString;
    private DelegateCommand _applyDist;

    private string _name;
    private int _methodIndex;
    private Dictionary<string, List<MethodInfo>> _allParametrics;
    private List<MethodInfo> _candidates;
    private string[] _candidateNames;
    private string[][] _paramsNames;
    private object[][] _paramsValues;
    private bool[] _hasParamArray;
    private ParameterViewModel[] _parameters;
    private DelegateCommand _applyParams;

    private CircuitGridViewModel _trackedCircuitGrid;
    private OutputGridViewModel _trackedOutputGrid;

    private bool _inputValid;

    #endregion // Fields


    #region Constructor

    public PropertiesViewModel(CircuitGridViewModel circuitGrid, OutputGridViewModel outputGrid)
    {
        SelectedObject = outputGrid.SelectedObject;
        AddSelectionAndQubitsTracing(circuitGrid);
        AddSelectionTracing(outputGrid);

        CircuitEvaluator eval = CircuitEvaluator.GetInstance();
        _allParametrics = eval.GetExtensionGates();
    }

    public PropertiesViewModel()
    {
    }

    #endregion // Constructor


    #region Presentation Properties

    public static BitmapInterpolationMode ScalingQuality => BitmapInterpolationMode.MediumQuality;

    public object SelectedObject
    {
        get
        {
            if (_selectedType == SelectedType.State)
            {
                return _selectedState;
            }

            return _selectedGate;
        }
        set
        {
            switch (value)
            {
                case null:
                    _selectedType = SelectedType.None;
                    break;
                case OutputState state:
                {
                    _selectedState = state;
                    _selectedType = SelectedType.State;
                    OnPropertyChanged(nameof(Value));
                    OnPropertyChanged(nameof(Qubits));
                    OnPropertyChanged(nameof(Probability));
                    if (_selectedState.Amplitude.HasValue)
                    {
                        OnPropertyChanged(nameof(Amplitude));
                        OnPropertyChanged(nameof(Phase));
                        OnPropertyChanged(nameof(StatePaneLineEnd));
                    }

                    break;
                }
                case GateViewModel gateViewModel:
                {
                    _selectedGate = gateViewModel;
                    Gate gate = _selectedGate.Value;
                    switch (gate.Name)
                    {
                        case GateName.PhaseKick:
                            RefreshRotatePane((gate as PhaseKickGate).Gamma);
                            break;
                        case GateName.PhaseScale:
                            RefreshRotatePane((gate as PhaseScaleGate).Gamma);
                            break;
                        case GateName.RotateX:
                            RefreshRotatePane((gate as RotateXGate).Gamma);
                            break;
                        case GateName.RotateY:
                            RefreshRotatePane((gate as RotateYGate).Gamma);
                            break;
                        case GateName.RotateZ:
                            RefreshRotatePane((gate as RotateZGate).Gamma);
                            break;
                        case GateName.CPhaseShift:
                            RefreshPhaseShiftPane((gate as CPhaseShiftGate).Dist);
                            break;
                        case GateName.InvCPhaseShift:
                            RefreshPhaseShiftPane((gate as InvCPhaseShiftGate).Dist);
                            break;
                        case GateName.Unitary:
                            RefreshUnitaryPane((gate as UnitaryGate).Matrix);
                            break;
                        case GateName.Composite:
                        case GateName.Parametric:
                            RefreshCompositePane(gate as CustomGate);
                            break;
                        default:
                            _selectedType = SelectedType.OtherGate;
                            break;
                    }

                    OnPropertyChanged(nameof(ImageSource));
                    break;
                }
            }

            OnPropertyChanged(nameof(StatePaneVisibility));
            OnPropertyChanged(nameof(AmplitudeVisibility));
            OnPropertyChanged(nameof(RotateGateVisibility));
            OnPropertyChanged(nameof(PhaseShiftVisibility));
            OnPropertyChanged(nameof(UnitaryPaneVisibility));
            OnPropertyChanged(nameof(CompositePaneVisibility));
            OnPropertyChanged(nameof(GatePaneVisibility));
        }
    }

    #region State Pane

    public bool StatePaneVisibility => _selectedType == SelectedType.State;

    public bool AmplitudeVisibility =>
        _selectedType == SelectedType.State
        && _selectedState.Amplitude.HasValue;

    public string Value => $"|{_selectedState.Value}>";

    public string Qubits => _selectedState.Representation;

    public string Probability => $"{_selectedState.Probability}";

    public string Amplitude => string.Format(_complexFormatter, "{0:I5}", _selectedState.Amplitude);

    public double Phase
    {
        get
        {
            if (_selectedState is { Amplitude: { } })
            {
                return -_selectedState.Amplitude.Value.Phase * 180 / Math.PI;
            }

            return 0;
        }
    }

    public List<Point> ArrowPositions
    {
        get
        {
            var lineReference = StatePaneLineEnd;

            var bottom = new Point(lineReference.X, lineReference.Y - 6);
            var right = new Point(lineReference.X + 15, lineReference.Y);
            var top = new Point(lineReference.X, lineReference.Y + 6);

            return new List<Point> { bottom, right, top };
        }
    }

    public Point StatePaneLineEnd => _selectedState is { Amplitude: { } }
        ? new Point(_lineLength * Math.Cos(_selectedState.Amplitude.Value.Phase),
            -_lineLength * Math.Sin(_selectedState.Amplitude.Value.Phase))
        : new Point(0, 0);

    #endregion // State Pane


    #region Rotate Gate Pane

    public bool RotateGateVisibility => _selectedType == SelectedType.RotateGate;

    public ICommand SelectUnitCommand
    {
        get
        {
            if (_selectUnit == null)
            {
                _selectUnit = new DelegateCommand(SelectUnit, x => true);
            }

            return _selectUnit;
        }
    }

    public ICommand ApplyGammaCommand
    {
        get
        {
            if (_applyGamma == null)
            {
                _applyGamma = new DelegateCommand(ApplyGamma, x => true);
            }

            return _applyGamma;
        }
    }

    public bool Rad
    {
        get => _rad;
        set
        {
            if (_rad == value) return;
            _rad = value;
            OnPropertyChanged(nameof(Rad));
            Gamma = GammaToString();
        }
    }

    public string Gamma
    {
        get => _gammaString;
        set
        {
            if (!double.TryParse(value, out var result)) return;
            if (_rad)
            {
                _gammaRad = result;
            }
            else
            {
                _gammaRad = result * Math.PI / 180;
            }

            _gammaString = GammaToString();
            OnPropertyChanged(nameof(Gamma));
        }
    }

    #endregion // Rotate Gate Pane


    #region PhaseShift Gate Pane

    public bool PhaseShiftVisibility => _selectedType == SelectedType.PhaseShiftGate;

    public ICommand ApplyDistCommand
    {
        get
        {
            if (_applyDist == null)
            {
                _applyDist = new DelegateCommand(ApplyDist, x => true);
            }

            return _applyDist;
        }
    }

    public string PhaseDist
    {
        get => _distString;
        set
        {
            if (!int.TryParse(value, out var result)) return;

            _distString = result.ToString();
            OnPropertyChanged(nameof(PhaseDist));
        }
    }

    #endregion // PhaseShift Gate Pane


    #region Unitary Gate Pane

    public bool UnitaryPaneVisibility => _selectedType == SelectedType.UnitaryGate;

    public ICommand ApplyMatrixCommand
    {
        get
        {
            if (_applyMatrix == null)
            {
                _applyMatrix = new DelegateCommand(ApplyMatrix, x => true);
            }

            return _applyMatrix;
        }
    }

    public ICommand CancelMatrixCommand
    {
        get
        {
            if (_cancelMatrix == null)
            {
                _cancelMatrix = new DelegateCommand(CancelMatrix, x => true);
            }

            return _cancelMatrix;
        }
    }

    [ComplexNumber]
    public string A00Text
    {
        get => _a00Text;
        set
        {
            if (value == _a00Text) return;

            _a00Text = value;
            OnPropertyChanged(nameof(A00Text));
            ComplexParser.TryParse(_a00Text, out _matrix[0, 0]);
            ValidateMatrix();
        }
    }

    [ComplexNumber]
    public string A01Text
    {
        get => _a01Text;
        set
        {
            if (value == _a01Text) return;

            _a01Text = value;
            OnPropertyChanged(nameof(A01Text));
            ComplexParser.TryParse(_a01Text, out _matrix[0, 1]);
            ValidateMatrix();
        }
    }

    [ComplexNumber]
    public string A10Text
    {
        get => _a10Text;
        set
        {
            if (value == _a10Text) return;

            _a10Text = value;
            OnPropertyChanged(nameof(A10Text));
            ComplexParser.TryParse(_a10Text, out _matrix[1, 0]);
            ValidateMatrix();
        }
    }

    [ComplexNumber]
    public string A11Text
    {
        get => _a11Text;
        set
        {
            if (value == _a11Text) return;

            _a11Text = value;
            OnPropertyChanged(nameof(A11Text));
            ComplexParser.TryParse(_a11Text, out _matrix[1, 1]);
            ValidateMatrix();
        }
    }

    public bool InputValid
    {
        get => _inputValid;
        set
        {
            if (value == _inputValid) return;

            _inputValid = value;
            OnPropertyChanged(nameof(InputValid));
        }
    }

    public string ValidationMessage => _isUnitary ? string.Empty : Resources.MatrixNotUnitary;

    public Complex[,] Matrix => _isUnitary ? _matrix : null;

    #endregion // Unitary Gate Pane


    #region Composite Gate Pane

    public bool CompositePaneVisibility => _selectedType == SelectedType.CompositeGate;

    public ICommand ApplyParamsCommand
    {
        get
        {
            if (_applyParams == null)
            {
                _applyParams = new DelegateCommand(ApplyParams, x => true);
            }

            return _applyParams;
        }
    }

    public int MethodIndex
    {
        get => _methodIndex;
        set
        {
            _methodIndex = value;
            PopulateParams();
            OnPropertyChanged(nameof(Parameters));
            OnPropertyChanged("ParamsNames"); // TODO: existing?
        }
    }

    public string[] Candidates => _candidateNames;

    public ParameterViewModel[] Parameters => _parameters;

    public bool IsValid
    {
        get
        {
            if (_parameters != null)
            {
                return _parameters.All(x => x.IsValid);
            }

            return false;
        }
    }

    public object[] ParamValues
    {
        get
        {
            if (_hasParamArray[_methodIndex])
            {
                ParameterInfo[] infos = _candidates[_methodIndex].GetParameters();
                object[] values = new object[infos.Length];
                // normal parameters
                for (int i = 1; i < infos.Length - 1; i++)
                {
                    values[i] = _parameters[i - 1].Value;
                }

                //params
                Type type = infos.Last().ParameterType.GetElementType();
                List<object> parList = new List<object>();
                int k = infos.Length - 2;
                bool notNull = true;
                while (notNull && k < _parameters.Length)
                {
                    object val = _parameters[k].Value;
                    if (val == null)
                    {
                        notNull = false;
                    }
                    else
                    {
                        parList.Add(val);
                    }

                    k++;
                }

                Array a = Array.CreateInstance(type, parList.Count);
                for (int i = 0; i < parList.Count; i++)
                {
                    a.SetValue(parList[i], i);
                }

                values[infos.Length - 1] = a;

                return values;
            }
            else
            {
                object[] values = new object[_parameters.Length + 1];
                for (int i = 0; i < _parameters.Length; i++)
                {
                    values[i + 1] = _parameters[i].Value;
                }

                return values;
            }
        }
    }

    public RegisterPartModel? CompositeGateTarget
    {
        get
        {
            Register reg = _parameters[0].Value as Register;
            return reg?.ToPartModel();
        }
    }

    #endregion // Composite Gate Pane


    #region Gate Pane

    public bool GatePaneVisibility => _selectedType == SelectedType.OtherGate;

    public string ImageSource
    {
        get
        {
            if (_selectedType == SelectedType.State) return string.Empty;

            const string imagePath = $"/Assets/Images/";

            return _selectedGate.Value.Name switch
            {
                GateName.CNot => $"{imagePath}imgCNot.gif",
                GateName.CPhaseShift => $"{imagePath}imgRk.gif",
                GateName.Hadamard => $"{imagePath}imgH.gif",
                GateName.InvCPhaseShift => $"{imagePath}imgInvRk.gif",
                GateName.PhaseKick => $"{imagePath}imgR.gif",
                GateName.PhaseScale => $"{imagePath}imgTheta.gif",
                GateName.RotateX => $"{imagePath}imgRx.gif",
                GateName.RotateY => $"{imagePath}imgRy.gif",
                GateName.RotateZ => $"{imagePath}imgRz.gif",
                GateName.SigmaX => $"{imagePath}imgSigmaX.gif",
                GateName.SigmaY => $"{imagePath}imgSigmaY.gif",
                GateName.SigmaZ => $"{imagePath}imgSigmaZ.gif",
                GateName.SqrtX => $"{imagePath}imgSqrtX.gif",
                GateName.Toffoli => $"{imagePath}imgToffoliSmall.gif",
                _ => null
            } ?? string.Empty;
        }
    }

    #endregion // Gate Pane

    #endregion // Presentation Properties


    #region Public Methods

    public void LoadParametrics(Dictionary<string, List<MethodInfo>> allParametrics)
    {
        _allParametrics = allParametrics;
    }

    public void AddSelectionTracing(OutputGridViewModel outputGrid)
    {
        if (_trackedOutputGrid == outputGrid) return;
        outputGrid.SelectionChanged += outputGrid_SelectionChanged;
        _trackedOutputGrid = outputGrid;
    }

    public void AddSelectionAndQubitsTracing(CircuitGridViewModel circuitGrid)
    {
        if (_trackedCircuitGrid == circuitGrid) return;
        circuitGrid.SelectionChanged += circuitGrid_SelectionChanged;
        circuitGrid.QubitsChanged += circuitGrid_QubitsChanged;
        _trackedCircuitGrid = circuitGrid;
    }

    public void SelectUnit(object parameter)
    {
        Rad = string.Equals("Rad", parameter as string);
    }

    public void ApplyGamma(object parameter)
    {
        _selectedGate.ChangeAngle(_gammaRad);
    }

    public void ApplyDist(object parameter)
    {
        _selectedGate.ChangePhaseDist(_dist);
    }

    public void ApplyParams(object parameter)
    {
        if (_allParametrics.ContainsKey(_name))
        {
            _selectedGate.ChangeParametricParams(ParamValues);
        }
        else
        {
            _selectedGate.ChangeCompositeTarget(CompositeGateTarget.Value);
        }
    }

    public void AddParam()
    {
        ParameterInfo[] infos = _candidates[_methodIndex].GetParameters();
        ParameterInfo info = infos.Last();

        Type type = info.ParameterType.GetElementType();
        int parNum = _parameters.Length - infos.Length + 2;
        string oldParName = _paramsNames[_methodIndex].Last();
        string prefix = oldParName.Split('[')[0];
        string parName = prefix + "[" + parNum + "]";

        ParameterViewModel toAdd = new ParameterViewModel(parName, type, null, false, true);
        toAdd.PropertyChanged += parameter_PropertyChanged;

        List<string> parNamesList = _paramsNames[_methodIndex].ToList();
        parNamesList.Add(parName);
        _paramsNames[_methodIndex] = parNamesList.ToArray();

        List<ParameterViewModel> parList = _parameters.ToList();
        parList.Add(toAdd);
        _parameters = parList.ToArray();

        OnPropertyChanged(nameof(Parameters));
    }

    public void SetAngle(string value)
    {
        if (string.Equals(value, Resources.Pi))
        {
            _gammaRad = Math.PI;
            Gamma = GammaToString();
        }
        else if (string.Equals(value, Resources.Pi_2))
        {
            _gammaRad = Math.PI / 2.0;
            Gamma = GammaToString();
        }
        else if (string.Equals(value, Resources.Pi_3))
        {
            _gammaRad = Math.PI / 3.0;
            Gamma = GammaToString();
        }
        else if (string.Equals(value, Resources.Pi_4))
        {
            _gammaRad = Math.PI / 4.0;
            Gamma = GammaToString();
        }
        else if (string.Equals(value, Resources.Pi_6))
        {
            _gammaRad = Math.PI / 6.0;
            Gamma = GammaToString();
        }
        else if (string.Equals(value, Resources.Pi_8))
        {
            _gammaRad = Math.PI / 8.0;
            Gamma = GammaToString();
        }
    }

    public void ApplyMatrix(object parameter)
    {
        Complex[,] matrix = Matrix;
        if (matrix != null)
        {
            _selectedGate.ChangeUnitaryMatrix(matrix);
        }
    }

    public void CancelMatrix(object parameter)
    {
        Complex[,] oldMatrix = (_selectedGate.Value as UnitaryGate).Matrix;
        RefreshUnitaryPane(oldMatrix);
    }

    #endregion // Public Methods


    #region Private Helpers

    private void outputGrid_SelectionChanged(object sender, EventArgs eventArgs)
    {
        OutputGridViewModel outputGrid = sender as OutputGridViewModel;
        SelectedObject = outputGrid.SelectedObject;
    }

    private void circuitGrid_SelectionChanged(object sender, EventArgs eventArgs)
    {
        CircuitGridViewModel grid = sender as CircuitGridViewModel;
        SelectedObject = grid.SelectedObject;
    }

    private void circuitGrid_QubitsChanged(object sender, EventArgs eventArgs)
    {
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(Qubits));
        OnPropertyChanged(nameof(Probability));
    }

    private string GammaToString()
    {
        return _rad ? $"{_gammaRad:N5}" : $"{_gammaRad * 180 / Math.PI:N2}";
    }

    private void RefreshRotatePane(double gamma)
    {
        _gammaRad = gamma;
        _selectedType = SelectedType.RotateGate;
        Gamma = GammaToString();
    }

    private void RefreshPhaseShiftPane(int dist)
    {
        _dist = dist;
        _selectedType = SelectedType.PhaseShiftGate;
        PhaseDist = _dist.ToString();
    }

    private void RefreshUnitaryPane(Complex[,] matrix)
    {
        if (matrix == null) return;

        _matrix = matrix;
        _selectedType = SelectedType.UnitaryGate;
        _a00Text = string.Format(_complexFormatter, "{0:K10}", _matrix[0, 0]);
        _a01Text = string.Format(_complexFormatter, "{0:K10}", _matrix[0, 1]);
        _a10Text = string.Format(_complexFormatter, "{0:K10}", _matrix[1, 0]);
        _a11Text = string.Format(_complexFormatter, "{0:K10}", _matrix[1, 1]);
        ValidateMatrix();
        OnPropertyChanged(nameof(A00Text));
        OnPropertyChanged(nameof(A01Text));
        OnPropertyChanged(nameof(A10Text));
        OnPropertyChanged(nameof(A11Text));
    }

    private void RefreshCompositePane(CustomGate gate)
    {
        _selectedType = SelectedType.CompositeGate;
        _name = gate.FunctionName;

        if (gate.Name == GateName.Parametric)
        {
            ParametricGate cg = gate as ParametricGate;
            PopulateCandidates();
            OnPropertyChanged(nameof(Candidates));
            int methodIndex = _candidates.IndexOf(cg.Method);
            _paramsValues[methodIndex] = cg.Parameters;
            MethodIndex = methodIndex;
        }
        else // Composite
        {
            CompositeGate cg = gate as CompositeGate;
            PopulateCandidates();
            OnPropertyChanged(nameof(Candidates));
            _paramsValues[0] = new object[] { null, cg.TargetRegister };
            MethodIndex = 0;
        }
    }

    private void ValidateMatrix()
    {
        _isUnitary = MatrixValidator.IsUnitary2x2(_matrix);

        InputValid = _isUnitary;
        OnPropertyChanged(nameof(ValidationMessage));
    }

    private void PopulateCandidates()
    {
        if (_allParametrics.ContainsKey(_name))
        {
            _candidates = _allParametrics[_name];

            _paramsNames = new string[_candidates.Count][];
            _paramsValues = new object[_candidates.Count][];
            _candidateNames = new string[_candidates.Count];
            _hasParamArray = new bool[_candidates.Count];

            for (int i = 0; i < _candidates.Count; i++)
            {
                MethodInfo method = _candidates[i];
                ParameterInfo[] infos = method.GetParameters();
                _paramsNames[i] = new string[infos.Length];
                _paramsNames[i][0] = infos[0].Name;

                _hasParamArray[i] = false;

                for (int j = 1; j < infos.Length; j++)
                {
                    if (j == infos.Length - 1 &&
                        infos[j].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
                    {
                        _hasParamArray[i] = true;
                    }

                    _paramsNames[i][j] = infos[j].Name;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(TypeToString(method.ReturnType)).Append(' ');
                sb.Append(method.Name).Append('(');
                for (int j = 1; j < infos.Length; j++)
                {
                    if (j > 1)
                    {
                        sb.Append(", ");
                    }

                    if (j == infos.Length - 1 && _hasParamArray[i])
                    {
                        sb.Append("params ");
                    }

                    sb.Append(TypeToString(infos[j].ParameterType)).Append(' ');
                    sb.Append(_paramsNames[i][j]);
                }

                sb.Append(')');
                _candidateNames[i] = sb.ToString();
            }
        }
        else // Composite with List<Gate>
        {
            _paramsNames = new string[1][];
            _paramsValues = new object[1][];
            _candidateNames = new string[1];
            _hasParamArray = new bool[1];

            _paramsNames[0] = new string[2];
            _hasParamArray[0] = false;

            string compName = "comp";
            string regName = "regA";

            _paramsNames[0][0] = compName;
            _paramsNames[0][1] = regName;

            StringBuilder sb = new StringBuilder();
            sb.Append("Void ");
            sb.Append(_name).Append("(Register ");
            sb.Append(regName);
            sb.Append(')');

            _candidateNames[0] = sb.ToString();
        }
    }

    private void PopulateParams()
    {
        if (_allParametrics.ContainsKey(_name))
        {
            ParameterInfo[] infos = _candidates[_methodIndex].GetParameters();
            List<ParameterViewModel> parList = new List<ParameterViewModel>();

            for (int i = 1; i < infos.Length; i++)
            {
                Type type = infos[i].ParameterType;
                bool paramsArray = (i == infos.Length - 1 && _hasParamArray[_methodIndex]);
                if (_paramsValues[_methodIndex] != null)
                {
                    if (paramsArray)
                    {
                        IEnumerable pars = _paramsValues[_methodIndex][i] as IEnumerable;
                        int parNum = 0;
                        bool first = true;
                        foreach (var item in pars)
                        {
                            string oldParName = _paramsNames[_methodIndex][i];
                            string prefix = oldParName.Split('[')[0];
                            string parName = prefix + "[" + parNum + "]";
                            parNum++;
                            ParameterViewModel toAdd =
                                new ParameterViewModel(parName, type.GetElementType(), item, first, true);
                            toAdd.PropertyChanged += parameter_PropertyChanged;
                            parList.Add(toAdd);
                            first = false;
                        }
                    }
                    else
                    {
                        var value = _paramsValues[_methodIndex][i];
                        ParameterViewModel toAdd = new ParameterViewModel(_paramsNames[_methodIndex][i], type, value);
                        toAdd.PropertyChanged += parameter_PropertyChanged;
                        parList.Add(toAdd);
                    }
                }
                else
                {
                    if (paramsArray)
                    {
                        ParameterViewModel toAdd = new ParameterViewModel(_paramsNames[_methodIndex][i],
                            type.GetElementType(), null, true, true);
                        toAdd.PropertyChanged += parameter_PropertyChanged;
                        parList.Add(toAdd);
                    }
                    else
                    {
                        ParameterViewModel toAdd = new ParameterViewModel(_paramsNames[_methodIndex][i], type);
                        toAdd.PropertyChanged += parameter_PropertyChanged;
                        parList.Add(toAdd);
                    }
                }
            }

            _parameters = parList.ToArray();
        }
        else // Composite Gate
        {
            object value = _paramsValues[0][1];
            _parameters = new ParameterViewModel[1];
            ParameterViewModel toAdd = new ParameterViewModel(_paramsNames[_methodIndex][1], typeof(Register), value);
            toAdd.PropertyChanged += parameter_PropertyChanged;
            _parameters[0] = toAdd;
        }
    }

    private void parameter_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName.Equals("IsValid"))
        {
            OnPropertyChanged(nameof(IsValid));
        }
    }

    private string TypeToString(Type type)
    {
        string[] split = type.ToString().Split('.');
        return split[split.Length - 1];
    }

    #endregion // Private Helpers
}