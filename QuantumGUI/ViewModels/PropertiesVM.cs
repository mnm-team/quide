/**
    This file is part of QuIDE.

    QuIDE - The Quantum IDE
    Copyright (C) 2014  Joanna Patrzyk, Bartłomiej Patrzyk

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using QuantumModel;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Quantum.Helpers;
using QuIDE.Helpers;
using QuIDE.Properties;
using System.Reflection;
using System.Text;
using QuantumParser;
using QuantumParser.Validation;
using System.Collections;

namespace QuIDE.ViewModels
{
    public class PropertiesVM : ViewModelBase
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
        private GateVM _selectedGate;

        private const double _lineLength = 60;
        private static ComplexFormatter _complexFormatter = new ComplexFormatter();

        private DelegateCommand _selectUnit;
        private DelegateCommand _applyGamma;
        private bool _rad;
        private double _gammaRad;
        private string _gammaString;

        private Complex[,] _matrix = new Complex[2, 2] { { 0, 0 }, { 0, 0 } };
        private bool _isUnitary;
        private string _a00Text = String.Empty;
        private string _a01Text = String.Empty;
        private string _a10Text = String.Empty;
        private string _a11Text = String.Empty;
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
        private ParameterVM[] _parameters;
        private DelegateCommand _applyParams;

        #endregion // Fields


        #region Constructor

        // only for design
        public PropertiesVM()
        {
        }

        public PropertiesVM(CircuitGridVM circuitGrid, OutputGridVM outputGrid)
        {
            SelectedObject = outputGrid.SelectedObject;
            AddSelectionTracing(circuitGrid);
            AddSelectionTracing(outputGrid);

            CircuitEvaluator eval = CircuitEvaluator.GetInstance();
            _allParametrics = eval.GetExtensionGates();
        }

        #endregion // Constructor



        #region Presentation Properties

        public object SelectedObject
        {
            get
            {
                if (_selectedType == SelectedType.State)
                {
                    return _selectedState;
                }
                else
                {
                    return _selectedGate;
                }
            }
            set
            {
                if (value == null)
                {
                    _selectedType = SelectedType.None;
                }
                else if (value is OutputState)
                {
                    _selectedState = value as OutputState;
                    _selectedType = SelectedType.State;
                    OnPropertyChanged("Value");
                    OnPropertyChanged("Qubits");
                    OnPropertyChanged("Probability");
                    if (_selectedState.Amplitude.HasValue)
                    {
                        OnPropertyChanged("Amplitude");
                        OnPropertyChanged("Phase");
                        OnPropertyChanged("LineX2");
                        OnPropertyChanged("LineY2");

                    }
                }
                else if (value is GateVM)
                {
                    _selectedGate = value as GateVM;
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
                    OnPropertyChanged("ImageSource");
                }
                OnPropertyChanged("StatePaneVisibility");
                OnPropertyChanged("AmplitudeVisibility");
                OnPropertyChanged("RotateGateVisibility");
                OnPropertyChanged("PhaseShiftVisibility");
                OnPropertyChanged("UnitaryPaneVisibility");
                OnPropertyChanged("CompositePaneVisibility");
                OnPropertyChanged("GatePaneVisibility");
            }
        }

        #region State Pane

        public Visibility StatePaneVisibility
        {
            get
            {
                if (_selectedType == SelectedType.State)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Hidden;
                }
            }
        }

        public Visibility AmplitudeVisibility
        {
            get
            {
                if (_selectedType == SelectedType.State
                    && _selectedState.Amplitude.HasValue)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public string Value
        {
            get
            {
                return string.Format("|{0}>", _selectedState.Value);
            }
        }

        public string Qubits
        {
            get { return _selectedState.Representation; }
        }

        public string Probability
        {
            get { return string.Format("{0}", _selectedState.Probability); }
        }

        public string Amplitude
        {
            get
            {
                return string.Format(_complexFormatter, "{0:I5}", _selectedState.Amplitude);
            }
        }

        public double Phase
        {
            get
            {
                if (_selectedState != null && _selectedState.Amplitude.HasValue)
                {
                    return -_selectedState.Amplitude.Value.Phase * 180 / Math.PI;
                }
                return 0;
            }
        }

        public double LineX2
        {
            get
            {
                if (_selectedState != null && _selectedState.Amplitude.HasValue)
                {
                    return _lineLength * Math.Cos(_selectedState.Amplitude.Value.Phase);
                }
                return 0;
            }
        }

        public double LineY2
        {
            get
            {
                if (_selectedState != null && _selectedState.Amplitude.HasValue)
                {
                    return -_lineLength * Math.Sin(_selectedState.Amplitude.Value.Phase);
                }
                return 0;
            }
        }
        #endregion // State Pane


        #region Rotate Gate Pane

        public Visibility RotateGateVisibility
        {
            get
            {
                if (_selectedType == SelectedType.RotateGate)
                {
                    return Visibility.Visible;
                }
                return Visibility.Hidden;
            }
        }

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
            get { return _rad; }
            set
            {
                if (_rad != value)
                {
                    _rad = value;
                    OnPropertyChanged("Rad");
                    Gamma = GammaToString();
                }
            }
        }

        public string Gamma
        {
            get { return _gammaString;  }
            set
            {
                double result;
                if (double.TryParse(value, out result))
                {
                    if (_rad)
                    {
                        _gammaRad = result;
                    }
                    else
                    {
                        _gammaRad = result * Math.PI / 180;
                    }
                    _gammaString = GammaToString();
                    OnPropertyChanged("Gamma");
                }
            }
        }

        #endregion // Rotate Gate Pane


        #region PhaseShift Gate Pane

        public Visibility PhaseShiftVisibility
        {
            get
            {
                if (_selectedType == SelectedType.PhaseShiftGate)
                {
                    return Visibility.Visible;
                }
                return Visibility.Hidden;
            }
        }

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
            get { return _distString; }
            set
            {
                int result;
                if (int.TryParse(value, out result))
                {
                    _distString = result.ToString();
                    OnPropertyChanged("PhaseDist");
                }
            }
        }

        #endregion // PhaseShift Gate Pane


        #region Unitary Gate Pane
        public Visibility UnitaryPaneVisibility
        {
            get
            {
                if (_selectedType == SelectedType.UnitaryGate)
                {
                    return Visibility.Visible;
                }
                return Visibility.Hidden;
            }
        }

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

        public string A00Text
        {
            get
            {
                return _a00Text;
            }
            set
            {
                if (value != _a00Text)
                {
                    _a00Text = value;
                    OnPropertyChanged("A00Text");
                    ComplexParser.TryParse(_a00Text, out _matrix[0, 0]);
                    ValidateMatrix();
                }
            }
        }

        public string A01Text
        {
            get
            {
                return _a01Text;
            }
            set
            {
                if (value != _a01Text)
                {
                    _a01Text = value;
                    OnPropertyChanged("A01Text");
                    ComplexParser.TryParse(_a01Text, out _matrix[0, 1]);
                    ValidateMatrix();
                }
            }
        }

        public string A10Text
        {
            get
            {
                return _a10Text;
            }
            set
            {
                if (value != _a10Text)
                {
                    _a10Text = value;
                    OnPropertyChanged("A10Text");
                    ComplexParser.TryParse(_a10Text, out _matrix[1, 0]);
                    ValidateMatrix();
                }
            }
        }

        public string A11Text
        {
            get
            {
                return _a11Text;
            }
            set
            {
                if (value != _a11Text)
                {
                    _a11Text = value;
                    OnPropertyChanged("A11Text");
                    ComplexParser.TryParse(_a11Text, out _matrix[1, 1]);
                    ValidateMatrix();
                }
            }
        }

        public string ValidationMessage
        {
            get
            {
                if (_isUnitary)
                {
                    return string.Empty;
                }
                else
                {
                    return Resources.MatrixNotUnitary;
                }
            }
        }

        public Complex[,] Matrix
        {
            get
            {
                if (_isUnitary)
                {
                    return _matrix;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion // Unitary Gate Pane


        #region Composite Gate Pane

        public Visibility CompositePaneVisibility
        {
            get
            {
                if (_selectedType == SelectedType.CompositeGate)
                {
                    return Visibility.Visible;
                }
                return Visibility.Hidden;
            }
        }

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
            get { return _methodIndex; }
            set
            {
                _methodIndex = value;
                PopulateParams();
                OnPropertyChanged("Parameters");
                OnPropertyChanged("ParamsNames");
            }
        }

        public string[] Candidates
        {
            get { return _candidateNames; }
        }

        public ParameterVM[] Parameters
        {
            get { return _parameters; }
        }

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

        public RegisterPartModel? CopositeGateTarget
        {
            get
            {
                QuantumParser.Register reg = _parameters[0].Value as QuantumParser.Register;
                if (reg != null)
                {
                    return reg.ToPartModel();
                }
                return null;
            }
        }

        #endregion // Composite Gate Pane


        #region Gate Pane
        public Visibility GatePaneVisibility
        {
            get
            {
                if (_selectedType == SelectedType.OtherGate)
                {
                    return Visibility.Visible;
                }
                return Visibility.Hidden;
            }
        }

        public string ImageSource
        {
            get
            {
                if (_selectedType != SelectedType.State)
                {
                    switch (_selectedGate.Value.Name)
                    {
                        case GateName.CNot:
                            return "/QuIDE;component/Images/imgCNot.gif";
                        case GateName.CPhaseShift:
                            return "/QuIDE;component/Images/imgRk.gif";
                        case GateName.Hadamard:
                            return "/QuIDE;component/Images/imgH.gif";
                        case GateName.InvCPhaseShift:
                            return "/QuIDE;component/Images/imgInvRk.gif";
                        case GateName.PhaseKick:
                            return "/QuIDE;component/Images/imgR.gif";
                        case GateName.PhaseScale:
                            return "/QuIDE;component/Images/imgTheta.gif";
                        case GateName.RotateX:
                            return "/QuIDE;component/Images/imgRx.gif";
                        case GateName.RotateY:
                            return "/QuIDE;component/Images/imgRy.gif";
                        case GateName.RotateZ:
                            return "/QuIDE;component/Images/imgRz.gif";
                        case GateName.SigmaX:
                            return "/QuIDE;component/Images/imgSigmaX.gif";
                        case GateName.SigmaY:
                            return "/QuIDE;component/Images/imgSigmaY.gif";
                        case GateName.SigmaZ:
                            return "/QuIDE;component/Images/imgSigmaZ.gif";
                        case GateName.SqrtX:
                            return "/QuIDE;component/Images/imgSqrtX.gif";
                        case GateName.Toffoli:
                            return "/QuIDE;component/Images/imgToffoliSmall.gif";
                        default:
                            return null;
                    }
                }
                return null;
            }
        }
        #endregion // Gate Pane

        #endregion // Presentation Properties


        #region Public Methods

        public void LoadParametrics(Dictionary<string, List<MethodInfo>> allParametrics)
        {
            _allParametrics = allParametrics;
        }

        public void AddSelectionTracing(OutputGridVM outputGrid)
        {
            outputGrid.SelectionChanged += outputGrid_SelectionChanged;
        }

        public void AddSelectionTracing(CircuitGridVM circuitGrid)
        {
            circuitGrid.SelectionChanged += circuitGrid_SelectionChanged;
        }

        public void SelectUnit(object parameter)
        {
            if (string.Equals("Rad", parameter as string))
            {
                Rad = true;
            }
            else
            {
                Rad = false;
            }
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
                _selectedGate.ChangeCompositeTarget(CopositeGateTarget.Value);
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

            ParameterVM toAdd = new ParameterVM(parName, type, null, false, true);
            toAdd.PropertyChanged += parameter_PropertyChanged;

            List<string> parNamesList = _paramsNames[_methodIndex].ToList();
            parNamesList.Add(parName);
            _paramsNames[_methodIndex] = parNamesList.ToArray();

            List<ParameterVM> parList = _parameters.ToList();
            parList.Add(toAdd);
            _parameters = parList.ToArray();

            OnPropertyChanged("Parameters");
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

        private void outputGrid_SelectionChanged(object sender, RoutedEventArgs e)
        {
            OutputGridVM outputGrid = sender as OutputGridVM;
            SelectedObject = outputGrid.SelectedObject;
        }

        private void circuitGrid_SelectionChanged(object sender, RoutedEventArgs e)
        {
            CircuitGridVM grid = sender as CircuitGridVM;
            SelectedObject = grid.SelectedObject;
        }

        private string GammaToString()
        {
            if (_rad)
            {
                return string.Format("{0:N5}", _gammaRad);
            }
            else
            {
                return string.Format("{0:N2}", _gammaRad * 180 / Math.PI);
            }
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
            if (matrix != null)
            {
                _matrix = matrix;
                _selectedType = SelectedType.UnitaryGate;
                _a00Text = string.Format(_complexFormatter, "{0:K10}", _matrix[0, 0]);
                _a01Text = string.Format(_complexFormatter, "{0:K10}", _matrix[0, 1]);
                _a10Text = string.Format(_complexFormatter, "{0:K10}", _matrix[1, 0]);
                _a11Text = string.Format(_complexFormatter, "{0:K10}", _matrix[1, 1]);
                ValidateMatrix();
                OnPropertyChanged("A00Text");
                OnPropertyChanged("A01Text");
                OnPropertyChanged("A10Text");
                OnPropertyChanged("A11Text");
            }
        }

        private void RefreshCompositePane(CustomGate gate)
        {
            _selectedType = SelectedType.CompositeGate;
            _name = gate.FunctionName;

            if (gate.Name == GateName.Parametric)
            {
                ParametricGate cg = gate as ParametricGate;
                PopulateCandidates();
                OnPropertyChanged("Candidates");
                int methodIndex = _candidates.IndexOf(cg.Method);
                _paramsValues[methodIndex] = cg.Parameters;
                MethodIndex = methodIndex;
            }
            else // Composite
            {
                CompositeGate cg = gate as CompositeGate;
                PopulateCandidates();
                OnPropertyChanged("Candidates");
                _paramsValues[0] = new object[] { null, cg.TargetRegister };
                MethodIndex = 0;
            }
        }

        private void ValidateMatrix()
        {
            _isUnitary = MatrixValidator.IsUnitary2x2(_matrix);
            OnPropertyChanged("ValidationMessage");
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
                        Type type = infos[j].ParameterType;
                        if (j == infos.Length - 1 &&
                            infos[j].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
                        {
                            _hasParamArray[i] = true;
                        }
                        _paramsNames[i][j] = infos[j].Name;
                    }

                    StringBuilder sb = new StringBuilder();
                    sb.Append(TypeToString(method.ReturnType)).Append(" ");
                    sb.Append(method.Name).Append("(");
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
                        sb.Append(TypeToString(infos[j].ParameterType)).Append(" ");
                        sb.Append(_paramsNames[i][j]);
                    }
                    sb.Append(")");
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
                sb.Append(")");

                _candidateNames[0] = sb.ToString();
            }
        }
        private void PopulateParams()
        {
            if (_allParametrics.ContainsKey(_name))
            {
                ParameterInfo[] infos = _candidates[_methodIndex].GetParameters();
                List<ParameterVM> parList = new List<ParameterVM>();

                for (int i = 1; i < infos.Length; i++)
                {
                    Type type = infos[i].ParameterType;
                    bool paramsArray = (i == infos.Length - 1 && _hasParamArray[_methodIndex]);
                    object value = null;
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
                                ParameterVM toAdd = new ParameterVM(parName, type.GetElementType(), item, first, true);
                                toAdd.PropertyChanged += parameter_PropertyChanged;
                                parList.Add(toAdd);
                                first = false;
                            }
                        }
                        else
                        {
                            value = _paramsValues[_methodIndex][i];
                            ParameterVM toAdd = new ParameterVM(_paramsNames[_methodIndex][i], type, value);
                            toAdd.PropertyChanged += parameter_PropertyChanged;
                            parList.Add(toAdd);
                        }
                    }
                    else
                    {
                        if (paramsArray)
                        {
                            ParameterVM toAdd = new ParameterVM(_paramsNames[_methodIndex][i], type.GetElementType(), null, true, true);
                            toAdd.PropertyChanged += parameter_PropertyChanged;
                            parList.Add(toAdd);
                        }
                        else
                        {
                            ParameterVM toAdd = new ParameterVM(_paramsNames[_methodIndex][i], type);
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
                _parameters = new ParameterVM[1];
                ParameterVM toAdd = new ParameterVM(_paramsNames[_methodIndex][1], typeof(QuantumParser.Register), value);
                toAdd.PropertyChanged += parameter_PropertyChanged;
                _parameters[0] = toAdd;
            }
        }

        private void parameter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsValid"))
            {
                OnPropertyChanged("IsValid");
            }
        }

        private string TypeToString(Type type)
        {
            string[] split = type.ToString().Split('.');
            return split[split.Length - 1];
        }

        #endregion // Private Helpers
    }
}
