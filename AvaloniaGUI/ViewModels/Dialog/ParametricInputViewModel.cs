#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AvaloniaGUI.ViewModels.Helpers;
using AvaloniaGUI.ViewModels.MainModels.QuantumModel;
using AvaloniaGUI.ViewModels.MainModels.QuantumModel.Gates;
using AvaloniaGUI.ViewModels.MainModels.QuantumParser;

#endregion

namespace AvaloniaGUI.ViewModels.Dialog;

public class ParametricInputViewModel : ViewModelBase
{
    private string _name;

    private int _gateIndex;
    private int _methodIndex;

    private Dictionary<string, List<MethodInfo>> _allParametrics;
    private List<MethodInfo> _candidates;

    private string[] _compositeNames;
    private string[] _candidateNames;
    private string[][] _paramsNames;
    private bool[] _hasParamArray;

    private ParameterViewModel[] _parameters;

    private Dictionary<string, List<Gate>> _allComposites;


    public ParametricInputViewModel(string name,
        Dictionary<string, List<MethodInfo>> allParametrics,
        Dictionary<string, List<Gate>> allComposites)
    {
        _name = name;
        _allParametrics = allParametrics;
        _allComposites = allComposites;

        _compositeNames = allParametrics.Keys.Concat(allComposites.Keys).Distinct().ToArray();

        for (int i = 0; i < _compositeNames.Length; i++)
        {
            if (!_compositeNames[i].Equals(name)) continue;

            _gateIndex = i;
            break;
        }

        PopulateCandidates();
    }

    public int GateIndex
    {
        get => _gateIndex;
        set
        {
            _gateIndex = value;
            PopulateCandidates();
            OnPropertyChanged(nameof(Candidates));
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
        }
    }

    public MethodInfo Method
    {
        get
        {
            if (_candidates != null && _methodIndex != -1)
            {
                return _candidates[_methodIndex];
            }

            return null;
        }
    }

    public string FunctionName
    {
        get
        {
            if (_compositeNames != null && _gateIndex > -1 && _gateIndex < _compositeNames.Length)
            {
                return _compositeNames[_gateIndex];
            }

            return null;
        }
    }

    public string[] CompositeNames => _compositeNames;

    public string[] Candidates => _candidateNames;

    public ParameterViewModel[] Parameters => _parameters;

    public bool IsValid
    {
        get { return _parameters.All(x => x.IsValid); }
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
            Register reg = _parameters[0].Value as Register;
            return reg?.ToPartModel();
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

        List<string> parNamesList = _paramsNames[_methodIndex].ToList();
        parNamesList.Add(parName);
        _paramsNames[_methodIndex] = parNamesList.ToArray();

        List<ParameterViewModel> parList = _parameters.ToList();
        parList.Add(toAdd);
        _parameters = parList.ToArray();

        OnPropertyChanged(nameof(Parameters));
    }

    private void PopulateCandidates()
    {
        string functionName = _compositeNames[_gateIndex];
        if (_allParametrics.ContainsKey(functionName))
        {
            _candidates = _allParametrics[functionName];

            _paramsNames = new string[_candidates.Count][];
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
            _candidateNames = new string[1];
            _hasParamArray = new bool[1];

            _paramsNames[0] = new string[2];
            _hasParamArray[0] = false;

            const string compName = "comp";
            const string regName = "regA";

            _paramsNames[0][0] = compName;
            _paramsNames[0][1] = regName;

            StringBuilder sb = new StringBuilder();
            sb.Append("Void ");
            sb.Append(functionName).Append("(Register ");
            sb.Append(regName);
            sb.Append(')');

            _candidateNames[0] = sb.ToString();
        }
    }

    private void PopulateParams()
    {
        if (_candidates != null)
        {
            ParameterInfo[] infos = _candidates[_methodIndex].GetParameters();

            _parameters = new ParameterViewModel[infos.Length - 1];

            for (int i = 1; i < infos.Length; i++)
            {
                Type type = infos[i].ParameterType;
                bool paramsArray = (i == infos.Length - 1 && _hasParamArray[_methodIndex]);
                if (paramsArray)
                {
                    _parameters[i - 1] = new ParameterViewModel(_paramsNames[_methodIndex][i], type.GetElementType(),
                        null,
                        true, true);
                }
                else
                {
                    _parameters[i - 1] = new ParameterViewModel(_paramsNames[_methodIndex][i], type);
                }
            }
        }
        else // Composite Gate
        {
            _parameters = new ParameterViewModel[1];
            _parameters[0] = new ParameterViewModel(_paramsNames[_methodIndex][1], typeof(Register));
        }
    }

    private string TypeToString(Type type)
    {
        string[] split = type.ToString().Split('.');
        return split[split.Length - 1];
    }
}