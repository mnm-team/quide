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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuantumModel
{
    public class ParametricGate : CustomGate
    {
        private MethodInfo _method;
        private MethodInfo _computationMethod;
        private MethodInfo _inverseMethod;
        private string _methodCode;

        private object[] _parameters;
        private RegisterPartModel[] _regs;
        private RegisterRefModel[] _regRefs;

        public ParametricGate(MethodInfo method, MethodInfo computationMethod, 
            MethodInfo inverseMethod, string methodCode, object[] parameters)
        {
            _method = method;
            _computationMethod = computationMethod;
            _inverseMethod = inverseMethod;
            _methodCode = methodCode;

            _parameters = parameters;
            UpdateRefs();
        }

        public override GateName Name
        {
            get { return GateName.Parametric; }
        }

        public override int Begin
        {
            get 
            {
                int regRefsMin = int.MaxValue;
                int regsMin = int.MaxValue;
                if (_regRefs.Length > 0)
                {
                    regRefsMin = _regRefs.Min<RegisterRefModel>(x => x.OffsetToRoot);
                }
                if (_regs.Length > 0)
                {
                    regsMin = _regs.Min<RegisterPartModel>(x => x.OffsetToRoot);
                }
                return Math.Min(regRefsMin, regsMin);
            }
        }

        public override int End
        {
            get
            {
                int regRefsMax = 0;
                int regsMax = 0;
                if (_regRefs.Length > 0)
                {
                    regRefsMax = _regRefs.Max<RegisterRefModel>(x => x.OffsetToRoot);
                }
                if (_regs.Length > 0)
                {
                    regsMax = _regs.Max<RegisterPartModel>(x => x.EndOffsetToRoot);
                }
                return Math.Max(regRefsMax, regsMax);
            }
        }

        public override RegisterRefModel Target
        {
            get
            {
                if (_regRefs.Length > 0)
                {
                    return _regRefs[0];
                }
                if (_regs.Length > 0)
                {
                    return new RegisterRefModel() { Register = _regs[0].Register, Offset = _regs[0].Offset };
                }
                // should not happen
                return new RegisterRefModel() { Register = null, Offset = 0 };
            }
        }

        public override void IncrementRow(RegisterModel register, int afterOffset, int delta = 1)
        {
            Func<RegisterRefModel, RegisterRefModel> incRegRef = regRef =>
            {
                if (regRef.Register == register && regRef.Offset > afterOffset)
                {
                    return new RegisterRefModel()
                    {
                        Register = regRef.Register,
                        Offset = regRef.Offset + delta
                    };
                }
                return regRef;
            };

            for (int i = 0; i < _parameters.Length; i++)
            {
                if (_parameters[i] is RegisterRefModel)
                {
                    _parameters[i] = incRegRef((RegisterRefModel)_parameters[i]);
                }
                else if (_parameters[i] is RegisterPartModel)
                {
                    _parameters[i] = incRegPart((RegisterPartModel)_parameters[i], register, afterOffset, delta);
                }
                else if (_parameters[i] is RegisterRefModel[])
                {
                    _parameters[i] = (_parameters[i] as RegisterRefModel[])
                        .Select<RegisterRefModel, RegisterRefModel>(x => incRegRef(x)).ToArray();
                }
                else if (_parameters[i] is RegisterPartModel[])
                {
                    _parameters[i] = (_parameters[i] as RegisterPartModel[])
                        .Select<RegisterPartModel, RegisterPartModel>(x => incRegPart(x, register, afterOffset, delta)).ToArray();
                }
            }
            UpdateRefs();
        }

        public override Gate Copy(int referenceBeginRow)
        {
            Func<RegisterRefModel, RegisterRefModel> copyRegRef = rrm =>
                {
                    return new RegisterRefModel() 
                    { 
                        Register = null, 
                        Offset = rrm.OffsetToRoot - referenceBeginRow 
                    };
                };

            object[] newPars = new object[_parameters.Length];
            for (int i = 0; i < _parameters.Length; i++)
            {
                if (_parameters[i].GetType() == typeof(RegisterRefModel))
                {
                    newPars[i] = copyRegRef((RegisterRefModel)_parameters[i]);
                }
                else if (_parameters[i].GetType() == typeof(RegisterPartModel))
                {
                    newPars[i] = copyRegPart((RegisterPartModel)_parameters[i], referenceBeginRow);
                }
                else if (_parameters[i].GetType() == typeof(RegisterRefModel[]))
                {
                    newPars[i] = (_parameters[i] as RegisterRefModel[])
                        .Select<RegisterRefModel, RegisterRefModel>(x => copyRegRef(x)).ToArray();
                }
                else if (_parameters[i].GetType() == typeof(RegisterPartModel[]))
                {
                    newPars[i] = (_parameters[i] as RegisterPartModel[])
                        .Select<RegisterPartModel, RegisterPartModel>(x => copyRegPart(x, referenceBeginRow)).ToArray();
                }
                else
                {
                    newPars[i] = _parameters[i];
                }
            }
            ParametricGate cg = new ParametricGate(_method, _computationMethod, _inverseMethod, _methodCode, newPars);
            return cg;
        }
        
        public override string FunctionName
        {
            get { return _method.Name; }
        }

        public MethodInfo Method
        {
            get { return _method; }
        }

        public MethodInfo ComputationMethod
        {
            get { return _computationMethod; }
        }

        public MethodInfo InverseMethod
        {
            get { return _inverseMethod; }
        }

        public string MethodCode
        {
            get { return _methodCode; }
        }

        public object[] Parameters
        {
            get { return _parameters; }
        }

        private void UpdateRefs()
        {
            List<RegisterPartModel> regList = _parameters.OfType<RegisterPartModel>().ToList();
            List<RegisterRefModel> refList = _parameters.OfType<RegisterRefModel>().ToList();

            object last = _parameters.Last();
            if (last.GetType() == typeof(RegisterPartModel[]))
            {
                regList.AddRange(last as RegisterPartModel[]);
            }
            else if (last.GetType() == typeof(RegisterRefModel[]))
            {
                refList.AddRange(last as RegisterRefModel[]);
            }
            _regs = regList.ToArray();
            _regRefs = refList.ToArray();
        }
    }
}
