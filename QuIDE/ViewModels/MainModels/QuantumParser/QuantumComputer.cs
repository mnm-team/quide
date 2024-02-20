/*
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

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Reflection;
using QuIDE.ViewModels.MainModels.QuantumModel;
using QuIDE.ViewModels.MainModels.QuantumModel.Gates;

#endregion

namespace QuIDE.ViewModels.MainModels.QuantumParser
{
    public class QuantumComputer : IQuantumComputer
    {
        #region Fields

        private static QuantumComputer _instance;

        private ComputerModel _model;
        private OutputViewModel _outputModel;

        //private List<Gate> _gatesContainer;

        private Quantum.QuantumComputer _comp;
        private Quantum.Register _quantumRoot;

        private Register _parserRoot;

        private Dictionary<string, List<MethodInfo>> _parserExtensionGates;
        private Dictionary<string, List<MethodInfo>> _tempParserExtensionGates;
        private Dictionary<string, List<MethodInfo>> _allParserExtensionGates;

        private Dictionary<string, List<MethodInfo>> _libExtensionGates;
        private Dictionary<string, List<MethodInfo>> _tempLibExtensionGates;
        private Dictionary<string, List<MethodInfo>> _allLibExtensionGates;

        private Dictionary<string, List<MethodCode>> _methodsCodes;

        private Dictionary<RegisterModel, Register> _modelRefs;

        private bool _listenToModelChanges = true;

        #endregion


        #region Internal Fields and Properties

        internal Dictionary<string, List<MethodInfo>> ExtensionGates
        {
            get
            {
                if (_allParserExtensionGates != null) return _allParserExtensionGates;

                if (_parserExtensionGates == null)
                {
                    IEnumerable<MethodInfo> methods =
                        Parser.GetExtensionMethods(Assembly.GetAssembly(GetType()), GetType());
                    _parserExtensionGates = LoadExtMethods(methods);
                }

                _allParserExtensionGates = new Dictionary<string, List<MethodInfo>>(_parserExtensionGates);

                return _allParserExtensionGates;
            }
        }

        internal Dictionary<string, List<MethodInfo>> LibExtensionGates
        {
            get
            {
                if (_allLibExtensionGates != null) return _allLibExtensionGates;

                if (_libExtensionGates == null)
                {
                    IEnumerable<MethodInfo> methods = Parser.GetExtensionMethods(
                        Assembly.GetAssembly(typeof(Quantum.QuantumComputer)),
                        typeof(Quantum.QuantumComputer));
                    _libExtensionGates = LoadExtMethods(methods);
                }

                _allLibExtensionGates = new Dictionary<string, List<MethodInfo>>(_libExtensionGates);

                return _allLibExtensionGates;
            }
        }

        internal Dictionary<string, List<MethodCode>> MethodsCodes => _methodsCodes;

        #endregion // Public Fields and Properties


        #region Constructor

        private QuantumComputer()
        {
            _comp = Quantum.QuantumComputer.GetInstance();

            _model = ComputerModel.CreateModelForParser();
            _model.Registers.CollectionChanged += Registers_CollectionChanged;

            _modelRefs = new Dictionary<RegisterModel, Register>();
        }

        #endregion // Constructor


        #region Public Methods

        public static QuantumComputer GetInstance()
        {
            if (_instance == null)
            {
                _instance = new QuantumComputer();
            }

            return _instance;
        }

        public Register NewRegister(ulong initval, int width, int? size = null)
        {
            Quantum.Register reg = _comp.NewRegister(initval, width, size);
            Register created = Init(reg);
            _modelRefs[created.Model] = created;
            created.Model.Qubits.CollectionChanged += Qubits_CollectionChanged;
            return created;
        }

        public Register NewRegister(IDictionary<ulong, Complex> initStates, int width)
        {
            Quantum.Register reg = _comp.NewRegister(initStates, width);
            Register created = Init(reg);
            _modelRefs[created.Model] = created;
            created.Model.Qubits.CollectionChanged += Qubits_CollectionChanged;
            return created;
        }

        public void DeleteRegister(ref Register register)
        {
            register.Reset();
        }

        public Register GetRootRegister(params RegisterRef[] refs)
        {
            return _parserRoot;
        }

        public Register TensorProduct(Register r1, Register r2)
        {
            throw new NotImplementedException();
        }

        #endregion // Public Methods

        #region Public Methods for parsing

        public bool Group = true;

        public void AddParametricGate(string name, object[] parameters)
        {
            MethodInfo rightMethod = FindExtension(name, parameters, ExtensionGates);
            if (rightMethod == null) return;

            ParametricGate cg = CreateParametricGate(rightMethod, parameters);
            AddGate(cg);
        }

        #endregion // Public Methods for parsing


        #region Internal Methods

        internal void LoadParserMethods(Assembly asm)
        {
            IEnumerable<MethodInfo> methods = Parser.GetExtensionMethods(asm, GetType());
            _tempParserExtensionGates = LoadExtMethods(methods);
            if (_parserExtensionGates == null)
            {
                IEnumerable<MethodInfo> methods1 =
                    Parser.GetExtensionMethods(Assembly.GetAssembly(GetType()), GetType());
                _parserExtensionGates = LoadExtMethods(methods1);
            }

            _allParserExtensionGates =
                MergeLeft(_parserExtensionGates, _tempParserExtensionGates);
        }

        internal void LoadLibMethods(Assembly asm)
        {
            IEnumerable<MethodInfo> methods = Parser.GetExtensionMethods(asm, typeof(Quantum.QuantumComputer));
            _tempLibExtensionGates = LoadExtMethods(methods);
            if (_libExtensionGates == null)
            {
                IEnumerable<MethodInfo> methods1 = Parser.GetExtensionMethods(
                    Assembly.GetAssembly(typeof(Quantum.QuantumComputer)),
                    typeof(Quantum.QuantumComputer));
                _libExtensionGates = LoadExtMethods(methods1);
            }

            _allLibExtensionGates = MergeLeft(_libExtensionGates, _tempLibExtensionGates);
        }

        internal void LoadMethodsCodes(Dictionary<string, List<MethodCode>> methods)
        {
            _methodsCodes = methods;
        }

        // for Parser
        internal ComputerModel GetModel()
        {
            return _model;
        }

        internal Quantum.Register GetSourceRoot()
        {
            return _comp.GetRootRegister(_parserRoot.SourceRegister);
        }

        internal OutputViewModel InitFromModel(ComputerModel model)
        {
            Reset(model);
            foreach (var regModel in model.Registers)
            {
                NewFromModel(regModel);
            }

            if (_outputModel == null)
            {
                _outputModel = new OutputViewModel();
            }

            RegisterPartModel? prev = _outputModel.SelectedRegister;
            if (prev.HasValue && prev.Value.Register != null)
            {
                Register reg = FindRegister(prev.Value.Register.Name);
                if (reg != null && reg.Width >= prev.Value.Offset + prev.Value.Width)
                {
                    _outputModel.Update(GetSourceRoot(), prev.Value with { Register = reg.Model });
                }
                else
                {
                    _outputModel.Update(GetSourceRoot());
                }
            }
            else
            {
                _outputModel.Update(GetSourceRoot());
            }

            return _outputModel;
        }

        internal Register NewFromModel(RegisterModel regModel)
        {
            Quantum.Register reg = _comp.NewRegister(regModel.InitStates, regModel.Qubits.Count);
            Register created = InitFromModel(reg, regModel);

            if (!_modelRefs.ContainsKey(created.Model))
            {
                regModel.Qubits.CollectionChanged += Qubits_CollectionChanged;
            }

            _modelRefs[created.Model] = created;
            return created;
        }

        internal void Decompose(ParametricGate gate)
        {
            MethodInfo rightMethod = gate.Method;
            if (rightMethod == null) return;

            object[] parameters = gate.Parameters.Select<object, object>(x =>
            {
                return x switch
                {
                    RegisterRefModel registerRefModel => ModelToRef(registerRefModel),
                    RegisterPartModel model => ModelToRef(model),
                    RegisterRefModel[] models => models.Select(ModelToRef).ToArray(),
                    RegisterPartModel[] registerPartModels => registerPartModels.Select(ModelToRef).ToArray(),
                    _ => x
                };
            }).ToArray();
            parameters[0] = this;

            Group = false;
            rightMethod.Invoke(this, parameters);
        }

        internal void Evaluate(ParametricGate gate, bool runBackward)
        {
            var rightMethod = runBackward ? gate.InverseMethod : gate.ComputationMethod;

            if (rightMethod == null) return;

            object FromModel(object x)
            {
                return x switch
                {
                    RegisterRefModel model => ModelToSource(model),
                    RegisterPartModel model => ModelToSource(model),
                    _ => x
                };
            }

            object[] parameters = new object[gate.Parameters.Length];
            ParameterInfo[] infos = rightMethod.GetParameters();
            for (int i = 1; i < parameters.Length; i++)
            {
                object par = gate.Parameters[i];
                if (infos[i].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
                {
                    IEnumerable pars = par as IEnumerable;
                    ArrayList newPars = new ArrayList();
                    foreach (var item in pars)
                    {
                        newPars.Add(FromModel(item));
                    }

                    Array arr = newPars.ToArray(infos[i].ParameterType.GetElementType());
                    parameters[i] = arr;
                }
                else
                {
                    parameters[i] = FromModel(par);
                }
            }

            parameters[0] = _comp;

            rightMethod.Invoke(_comp, parameters);
        }

        internal ParametricGate CreateParametricGate(MethodInfo method, object[] parameters)
        {
            string name = method.Name;
            string invName = name;
            string[] sep = { "Inverse" };
            if (name.StartsWith("Inverse"))
            {
                string[] splitted = name.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                invName = splitted[0];
            }
            else
            {
                invName = "Inverse" + name;
            }

            MethodInfo computationMethod = FindExtension(name, parameters, LibExtensionGates);
            MethodInfo inverseMethod = FindExtension(invName, parameters, LibExtensionGates);
            MethodCode methodCode = FindMethodCode(computationMethod);
            string methodBody = null;
            if (methodCode != null)
            {
                methodBody = methodCode.Code;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                Type t = parameters[i].GetType();
                if (t == typeof(RegisterRef))
                {
                    parameters[i] = ((RegisterRef)parameters[i]).ToRefModel();
                }
                else if (t == typeof(Register))
                {
                    parameters[i] = (parameters[i] as Register).ToPartModel();
                }
                else if (t == typeof(RegisterRef[]))
                {
                    parameters[i] = ((RegisterRef[])parameters[i])
                        .Select(x => x.ToRefModel()).ToArray();
                }
                else if (t == typeof(Register[]))
                {
                    parameters[i] = (parameters[i] as Register[])
                        .Select(x => x.ToPartModel()).ToArray();
                }
            }

            ParametricGate cg = new ParametricGate(method, computationMethod, inverseMethod, methodBody, parameters);
            return cg;
        }


        internal List<Gate> GetActualGates(CompositeGate cg)
        {
            return _model.GetActualGates(cg);
        }

        internal Register FindRegister(string name)
        {
            if ("root".Equals(name))
            {
                return _parserRoot;
            }

            if (_modelRefs != null)
            {
                foreach (Register item in _modelRefs.Values)
                {
                    if (item.Model.Name.Equals(name))
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        internal void Reset(ComputerModel model)
        {
            if (_quantumRoot != null)
            {
                _comp.DeleteRegister(ref _quantumRoot);
            }

            if (_model != model)
            {
                _model = model;
                _model.Registers.CollectionChanged += Registers_CollectionChanged;
                _modelRefs = new Dictionary<RegisterModel, Register>();
            }
        }

        internal void AddGate(Gate gate)
        {
            _model.AddGate(gate);
        }

        internal RegisterRef ModelToRef(RegisterRefModel model)
        {
            if (model.Register != null)
            {
                return new RegisterRef
                {
                    Register = _modelRefs[model.Register],
                    Offset = model.Offset
                };
            }
            else
            {
                return new RegisterRef
                {
                    Register = _parserRoot,
                    Offset = model.Offset
                };
            }
        }

        internal Register ModelToRef(RegisterPartModel model)
        {
            Register toReturn = _parserRoot;
            if (model.Register != null)
            {
                toReturn = _modelRefs[model.Register];
            }

            return model.Width == toReturn.Width ? toReturn : toReturn[model.Offset, model.Width];
        }

        internal Quantum.RegisterRef ModelToSource(RegisterRefModel model)
        {
            if (model.Register != null)
            {
                return new Quantum.RegisterRef
                {
                    Register = _modelRefs[model.Register].SourceRegister,
                    Offset = model.Offset
                };
            }
            else
            {
                return new Quantum.RegisterRef
                {
                    Register = _parserRoot.SourceRegister,
                    Offset = model.Offset
                };
            }
        }

        internal Quantum.Register ModelToSource(RegisterPartModel model)
        {
            Quantum.Register toReturn = _parserRoot.SourceRegister;
            if (model.Register != null)
            {
                toReturn = _modelRefs[model.Register].SourceRegister;
            }

            return model.Width == toReturn.Width ? toReturn : toReturn[model.Offset, model.Width];
        }

        #endregion // Internal Methods


        #region Private Helpers

        private Register Init(Quantum.Register reg)
        {
            RegisterModel regModel = new RegisterModel(
                _model.Registers.Count, 0, reg.Width, reg.GetAmplitudes());

            _quantumRoot = _quantumRoot == null ? _comp.GetRootRegister(reg) : _comp.GetRootRegister(_quantumRoot, reg);

            _parserRoot = new Register(_model, _quantumRoot, null, 0);

            _listenToModelChanges = false;
            _model.AddRegister(regModel);
            _listenToModelChanges = true;

            return new Register(_model, reg, regModel);
        }

        private Register InitFromModel(Quantum.Register reg, RegisterModel regModel)
        {
            if (_quantumRoot == null)
            {
                _quantumRoot = _comp.GetRootRegister(reg);
            }
            else
            {
                _quantumRoot = _comp.GetRootRegister(_quantumRoot, reg);
            }

            _parserRoot = new Register(_model, _quantumRoot, null, 0);
            return new Register(_model, reg, regModel);
        }

        private void Registers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_listenToModelChanges)
            {
                InitFromModel(_model);
            }
        }

        private void Qubits_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            InitFromModel(_model);
        }

        private Dictionary<string, List<MethodInfo>> LoadExtMethods(IEnumerable<MethodInfo> methods)
        {
            Dictionary<string, List<MethodInfo>> dict = new Dictionary<string, List<MethodInfo>>();
            MethodInfo[] marr = methods.ToArray();
            foreach (var item in marr)
            {
                if (dict.ContainsKey(item.Name))
                {
                    dict[item.Name].Add(item);
                }
                else
                {
                    dict[item.Name] = new List<MethodInfo> { item };
                }
            }

            return dict;
        }

        private static Dictionary<K, V> MergeLeft<K, V>(Dictionary<K, V> oldDict, Dictionary<K, V> newDict)
        {
            Dictionary<K, V> toReturn = new Dictionary<K, V>(oldDict);
            foreach (var pair in newDict)
            {
                toReturn[pair.Key] = pair.Value;
            }

            return toReturn;
        }

        private MethodInfo FindExtension(string name, object[] parameters, Dictionary<string, List<MethodInfo>> dict)
        {
            if (dict == null || !dict.TryGetValue(name, out List<MethodInfo> sameName)) return null;

            foreach (MethodInfo item in sameName)
            {
                ParameterInfo[] pars = item.GetParameters();
                if (pars.Length != parameters.Length) continue;

                bool match = true;
                int i = 0;
                while (match && i < pars.Length)
                {
                    Type actual = parameters[i].GetType();
                    if (pars[i].ParameterType != actual)
                    {
                        if (i == pars.Length - 1 &&
                            pars[i].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
                        {
                            if (actual != typeof(RegisterRef[]) &&
                                actual != typeof(Register[]))
                            {
                                match = false;
                            }
                        }
                        else
                        {
                            if (actual != typeof(QuantumComputer) &&
                                actual != typeof(RegisterRef) &&
                                actual != typeof(Register))
                            {
                                match = false;
                            }
                        }
                    }

                    i++;
                }

                if (match)
                {
                    return item;
                }
            }

            return null;
        }

        private MethodCode FindMethodCode(MethodInfo info)
        {
            ParameterInfo[] parameters = info.GetParameters();

            if (_methodsCodes == null || !_methodsCodes.TryGetValue(info.Name, out List<MethodCode> sameName))
                return null;

            foreach (MethodCode item in sameName)
            {
                string[] parTypes = item.ParametersTypes;
                if (parTypes.Length != parameters.Length) continue;

                bool match = true;
                int i = 0;
                while (match && i < parTypes.Length)
                {
                    Type actual = parameters[i].ParameterType;
                    if (!actual.ToString().Contains(parTypes[i]))
                    {
                        match = false;
                    }

                    i++;
                }

                if (match)
                {
                    return item;
                }
            }

            return null;
        }

        #endregion // Private Helpers
    }
}