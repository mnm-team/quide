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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuIDE.ViewModels
{
    public class ParametricInputVM : ViewModelBase
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
        
        private ParameterVM[] _parameters;

        private Dictionary<string, List<Gate>> _allComposites;


        public ParametricInputVM(string name, 
            Dictionary<string, List<MethodInfo>> allParametrics, 
            Dictionary<string, List<Gate>> allComposites)
        {
            _name = name;
            _allParametrics = allParametrics;
            _allComposites = allComposites;

            _compositeNames = allParametrics.Keys.Concat(allComposites.Keys).Distinct().ToArray();

            for (int i = 0; i < _compositeNames.Length; i++)
            {
                if (_compositeNames[i].Equals(name))
                {
                    _gateIndex = i;
                    break;
                }
            }
            PopulateCandidates();
        }

        public int GateIndex
        {
            get { return _gateIndex; }
            set
            {
                _gateIndex = value;
                PopulateCandidates();
                OnPropertyChanged("Candidates");
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

        public string[] CompositeNames
        {
            get { return _compositeNames; }
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
            get { return _parameters.All(x => x.IsValid); }
        }

        public object[] ParamValues
        {
            get
            {
                if(_hasParamArray[_methodIndex]) 
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

            List<string> parNamesList = _paramsNames[_methodIndex].ToList();
            parNamesList.Add(parName);
            _paramsNames[_methodIndex] = parNamesList.ToArray();
            
            List<ParameterVM> parList = _parameters.ToList();                     
            parList.Add(toAdd);
            _parameters = parList.ToArray();

            OnPropertyChanged("Parameters");
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
                sb.Append(functionName).Append("(Register ");
                sb.Append(regName);
                sb.Append(")");

                _candidateNames[0] = sb.ToString();
            }
        }

        private void PopulateParams()
        {
            if (_candidates != null)
            {
                ParameterInfo[] infos = _candidates[_methodIndex].GetParameters();

                _parameters = new ParameterVM[infos.Length - 1];

                for (int i = 1; i < infos.Length; i++)
                {
                    Type type = infos[i].ParameterType;
                    bool paramsArray = (i == infos.Length - 1 && _hasParamArray[_methodIndex]);
                    if (paramsArray)
                    {
                        _parameters[i - 1] = new ParameterVM(_paramsNames[_methodIndex][i], type.GetElementType(), null, true, true);
                    }
                    else
                    {
                        _parameters[i - 1] = new ParameterVM(_paramsNames[_methodIndex][i], type);
                    }
                }
            }
            else // Composite Gate
            {
                _parameters = new ParameterVM[1];
                _parameters[0] = new ParameterVM(_paramsNames[_methodIndex][1], typeof(QuantumParser.Register));
            }
        }

        private string TypeToString(Type type)
        {
            string[] split = type.ToString().Split('.');
            return split[split.Length - 1];
        }

    }


}
