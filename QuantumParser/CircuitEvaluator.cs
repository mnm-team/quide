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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuantumParser
{
    public class CircuitEvaluator
    {
        #region Fields

        private static CircuitEvaluator _instance;

        private QuantumComputer _comp;
        private StepEvaluator _stepEvaluator;

        #endregion // Fields


        #region Public Properties

        public Quantum.Register RootRegister
        {
            get
            {
                return _comp.GetSourceRoot();
            }
        }

        public ComputerModel Model
        {
            get
            {
                return _comp.GetModel();
            }
        }
        
        #endregion // Public Properties


        #region Constructor

        private CircuitEvaluator()
        {
            _comp = QuantumComputer.GetInstance();
        }
        
        #endregion // Constructor


        #region Public Methods

        public static CircuitEvaluator GetInstance()
        {
            if (_instance == null)
            {
                _instance = new CircuitEvaluator();
            }
            return _instance;
        }

        public Output InitFromModel(ComputerModel model)
        {
            return _comp.InitFromModel(model);
        }

        public StepEvaluator GetStepEvaluator()
        {
            _stepEvaluator = new StepEvaluator(_comp);
            return _stepEvaluator;
        }

        public void LoadLibMethods(Assembly asm)
        {
            _comp.LoadLibMethods(asm);
        }

        public void LoadParserMethods(Assembly asm)
        {
            _comp.LoadParserMethods(asm);
        }

        public void LoadMethodsCodes(Dictionary<string, List<MethodCode>> methods)
        {
            _comp.LoadMethodsCodes(methods);
        }

        public Dictionary<string, List<MethodInfo>> GetExtensionGates()
        {
            return _comp.ExtensionGates;
        }

        public Dictionary<string, List<MethodCode>> GetMethodsCodes()
        {
            return _comp.MethodsCodes;
        }

        public Dictionary<string, List<Gate>> GetCompositeGates()
        {
            return _comp.GetModel().CompositeGates;
        }

        public void Decompose(ParametricGate cg)
        {
            _comp.Decompose(cg);
        }

        public ParametricGate CreateParametricGate(MethodInfo method, object[] parameters)
        {
            parameters[0] = _comp;
            return _comp.CreateParametricGate(method, parameters);
        }

        public RegisterRef? ResolveRegisterRef(string text, bool canBeEmpty)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                if (canBeEmpty)
                {
                    return null;
                }
                throw new Exception("Empty value");
            }
            string regex = @"^\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*\[\s*([1-9][0-9]*|[0-9])\s*\]\s*$";
            Match match = Regex.Match(text, regex);

            if (!match.Success)
            {
                throw new Exception("Invalid format. Please use the form: <registerName>[<index>]");
            }

            string regName = match.Groups[1].Value;
            string offsetString = match.Groups[2].Value;

            Register reg = _comp.FindRegister(regName);

            if (reg == null)
            {
                throw new Exception("No such register: " + regName);
            }

            int offset;
            if (!int.TryParse(offsetString, out offset))
            {
                throw new Exception("The index is not integer. Please use the form: <registerName>[<index>]");
            }

            if (offset < 0)
            {
                StringBuilder sb = new StringBuilder("Cannot reference to ");
                sb.Append(text);
                throw new Exception(sb.ToString());
            }
            if (offset >= reg.Width)
            {
                StringBuilder sb = new StringBuilder("Cannot reference to ");
                sb.Append(text);
                sb.Append(" - the register has only ").Append(reg.Width)
                    .Append(" qubits, numbered from 0 to ").Append(reg.Width - 1);
                throw new Exception(sb.ToString());
            }

            RegisterRef toReturn = new RegisterRef()
            {
                Register = reg,
                Offset = offset
            };
            return toReturn;
        }

        public Register ResolveRegister(string text, bool canBeEmpty)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                if (canBeEmpty)
                {
                    return null;
                }
                throw new Exception("Empty value");
            }

            string regex = @"^\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*(\[\s*([1-9][0-9]*|[0-9])\s*\,\s*([1-9][0-9]*|[0-9])\s*\])*\s*$";
            Match match = Regex.Match(text, regex);

            if (!match.Success)
            {
                throw new Exception("Invalid format. Please use the form: <registerName> or <registerName>[<offset>, <width>]");
            }

            string regName = match.Groups[1].Value;
            Register reg = _comp.FindRegister(regName);
            if (reg == null)
            {
                throw new Exception("No such register: " + regName);
            }

            if (match.Groups[2].Success)
            {
                string offsetString = match.Groups[3].Value;
                string widthString = match.Groups[4].Value;

                int offset;
                if (!int.TryParse(offsetString, out offset))
                {
                    throw new Exception("The offset is not integer. Please use the form: <registerName> or <registerName>[<offset>, <width>]");
                }
                int width;
                if (!int.TryParse(widthString, out width))
                {
                    throw new Exception("The width is not integer. Please use the form: <registerName> or <registerName>[<offset>, <width>]");
                }

                if (offset < 0 || offset >= reg.Width)
                {
                    StringBuilder sb = new StringBuilder("Cannot reference to ");
                    sb.Append(text);
                    sb.Append(" - invalid offset value (").Append(offset).Append(")");
                    throw new Exception(sb.ToString());
                }
                if (width <= 0 || offset + width > reg.Width)
                {
                    StringBuilder sb = new StringBuilder("Cannot reference to ");
                    sb.Append(text);
                    sb.Append(" - invalid width (").Append(width).Append(")");
                    throw new Exception(sb.ToString());
                }

                if (width < reg.Width)
                {
                    return reg[offset, width];
                }
                else
                {
                    return reg;
                }
            }
            else
            {
                return reg;
            }
        }

        #endregion // Public Mehods
    }
}
