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

using Quantum;
using QuantumModel;
using QuantumParser.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Windows;

namespace QuantumParser
{
    public class Register : IRegister
    {
        #region Fields

        private ComputerModel _compModel;

        private RegisterModel _model;
        private int _offsetToModel;

        private Quantum.Register _register;

        #endregion


        #region Constructors

        internal Register(ComputerModel compModel, Quantum.Register reg, RegisterModel regModel, int offsetToModel = 0)
        {
            _compModel = compModel;
            _register = reg;
            _model = regModel;
            _offsetToModel = offsetToModel;
        }

        #endregion // Constructors

        #region Public Properties

        public Quantum.Register SourceRegister
        {
            get { return _register; }
        }

        public RegisterModel Model
        {
            get { return _model; }
        }

        public int OffsetToModel
        {
            get { return _offsetToModel; }
        }

        #endregion // Public Properties

        #region IRegister Properties
        public RegisterRef this[int index]
        {
            get
            {
                ValidateIndex(index);
                Quantum.RegisterRef regRef = _register[index];
                return new RegisterRef() { Register = this, Offset = regRef.Offset };
            }
        }

        public Register this[int offset, int width]
        {
            get
            {
                ValidateOffsetWidth(offset, width);
                Quantum.Register reg = _register[offset, width];
                return new Register(_compModel, reg, _model, _offsetToModel + offset);
            }
        }

        public int Width
        {
            get { return _register.Width; }
        }

        public int OffsetToRoot
        {
            get { return _register.OffsetToRoot; }
        }
        #endregion // IRegister Properties

        #region IRegister Methods

        public ulong? GetValue()
        {
            return _register.GetValue();
        }

        public IReadOnlyDictionary<ulong, double> GetProbabilities()
        {
            return _register.GetProbabilities();
        }

        public IReadOnlyDictionary<ulong, Complex> GetAmplitudes()
        {
            return _register.GetAmplitudes();
        }

        public Complex[] GetVector()
        {
            return _register.GetVector();
        }

        public void Reset(ulong newValue = 0)
        {
            QuantumComputer comp = QuantumComputer.GetInstance();
            object[] parameters = new object[] { comp, this, newValue };
            comp.AddParametricGate("Reset", parameters);
            
            _register.Reset(newValue);
        }

        public void CNot(int target, int control)
        {
            Validate(target, "CNot", "target");
            Validate(control, "CNot", "control");

            RegisterRefModel controlRef, targetRef;
            if (_model == null)
            {
                controlRef = _compModel.GetRefFromOffset(control);
                targetRef = _compModel.GetRefFromOffset(target);
            }
            else
            {
                controlRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + control };
                targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
            }
            CNotGate gate = new CNotGate(targetRef, controlRef);
            AddGate(gate);
        }

        public void PhaseKick(double gamma, int target, params int[] controls)
        {
            Validate(target, "PhaseKick", "target");
            for (int i = 0; i < controls.Length; i++)
            {
                Validate(controls[i], "PhaseKick", "controls[" + i + "]");
            }

            RegisterRefModel targetRef;
            RegisterRefModel[] controlsRefs;
            if (_model == null)
            {
                targetRef = _compModel.GetRefFromOffset(target);
                controlsRefs = controls
                    .Select<int, RegisterRefModel>(x => _compModel.GetRefFromOffset(x))
                    .ToArray<RegisterRefModel>();
            }
            else
            {
                targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                controlsRefs = controls
                    .Select<int, RegisterRefModel>(x => new RegisterRefModel() { Register = _model, Offset = _offsetToModel + x })
                    .ToArray<RegisterRefModel>();
            }
            PhaseKickGate gate = new PhaseKickGate(gamma, targetRef, controlsRefs);
            AddGate(gate);
        }

        public void CPhaseShift(int dist, int target, params int[] controls)
        {
            Validate(target, "CPhaseShift", "target");
            for (int i = 0; i < controls.Length; i++)
            {
                Validate(controls[i], "CPhaseShift", "controls[" + i + "]");
            }

            RegisterRefModel targetRef;
            RegisterRefModel[] controlsRefs;
            if (_model == null)
            {
                targetRef = _compModel.GetRefFromOffset(target);
                controlsRefs = controls
                    .Select<int, RegisterRefModel>(x => _compModel.GetRefFromOffset(x))
                    .ToArray<RegisterRefModel>();

            }
            else
            {
                targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                controlsRefs = controls
                    .Select<int, RegisterRefModel>(x => new RegisterRefModel() { Register = _model, Offset = _offsetToModel + x })
                    .ToArray<RegisterRefModel>();
            }
            CPhaseShiftGate gate = new CPhaseShiftGate(dist, targetRef, controlsRefs);
            AddGate(gate);
        }

        public void Gate1(Complex[,] matrix, int target, int? control = null)
        {
            Validate(target, "Gate1", "target");
            if (control.HasValue)
            {
                Validate(control.Value, "Gate1", "control");
            }
            ValidateMatrix(matrix, "Gate1", "matrix");

            RegisterRefModel targetRef;
            RegisterRefModel? controlRef;

            if (control == null)
            {
                controlRef = null;
                if (_model == null)
                {
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            else
            {
                if (_model == null)
                {
                    controlRef = _compModel.GetRefFromOffset(control.Value);
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    controlRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + control.Value };
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            UnitaryGate gate = new UnitaryGate(matrix, targetRef, controlRef);
            AddGate(gate);
        }

        public void Hadamard(int target, int? control = null)
        {
            Validate(target, "Hadamard", "target");
            if (control.HasValue)
            {
                Validate(control.Value, "Hadamard", "control");
            }

            RegisterRefModel targetRef;
            RegisterRefModel? controlRef;

            if (control == null)
            {
                controlRef = null;
                if (_model == null)
                {
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            else
            {
                if (_model == null)
                {
                    controlRef = _compModel.GetRefFromOffset(control.Value);
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    controlRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + control.Value };
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            HadamardGate gate = new HadamardGate(targetRef, controlRef);
            AddGate(gate);
        }

        public void InverseCPhaseShift(int dist, int target, params int[] controls)
        {
            Validate(target, "InverseCPhaseShift", "target");
            for (int i = 0; i < controls.Length; i++)
            {
                Validate(controls[i], "InverseCPhaseShift", "controls[" + i + "]");
            }

            RegisterRefModel targetRef;
            RegisterRefModel[] controlsRefs;
            if (_model == null)
            {
                targetRef = _compModel.GetRefFromOffset(target);
                controlsRefs = controls
                    .Select<int, RegisterRefModel>(x => _compModel.GetRefFromOffset(x))
                    .ToArray<RegisterRefModel>();

            }
            else
            {
                targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                controlsRefs = controls
                    .Select<int, RegisterRefModel>(x => new RegisterRefModel() { Register = _model, Offset = _offsetToModel + x })
                    .ToArray<RegisterRefModel>();
            }
            InvCPhaseShiftGate gate = new InvCPhaseShiftGate(dist, targetRef, controlsRefs);
            AddGate(gate);
        }

        public ulong Measure()
        {
            RegisterRefModel beginRef, endRef;
            if (_model == null)
            {
                beginRef = _compModel.GetRefFromOffset(0);
                endRef = _compModel.GetRefFromOffset(_register.Width - 1);
            }
            else
            {
                beginRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel };
                endRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + _register.Width - 1 };
            }
            MeasureGate gate = new MeasureGate(beginRef, endRef);
            AddGate(gate);

            return _register.Measure();
        }

        public byte Measure(int position)
        {
            Validate(position, "Measure", "position");

            RegisterRefModel targetRef;
            if (_model == null)
            {
                targetRef = _compModel.GetRefFromOffset(position);
            }
            else
            {
                targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + position };
            }
            MeasureGate gate = new MeasureGate(targetRef);
            AddGate(gate);

            return _register.Measure(position);
        }

        public void PhaseScale(double gamma, int target, int? control = null)
        {
            Validate(target, "PhaseScale", "target");
            if (control.HasValue)
            {
                Validate(control.Value, "PhaseScale", "control");
            }

            RegisterRefModel targetRef;
            RegisterRefModel? controlRef;

            if (control == null)
            {
                controlRef = null;
                if (_model == null)
                {
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            else
            {
                if (_model == null)
                {
                    controlRef = _compModel.GetRefFromOffset(control.Value);
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    controlRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + control.Value };
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            PhaseScaleGate gate = new PhaseScaleGate(gamma, targetRef, controlRef);
            AddGate(gate);
        }

        public void RotateX(double gamma, int target, int? control = null)
        {
            Validate(target, "RotateX", "target");
            if (control.HasValue)
            {
                Validate(control.Value, "RotateX", "control");
            }

            RegisterRefModel targetRef;
            RegisterRefModel? controlRef;

            if (control == null)
            {
                controlRef = null;
                if (_model == null)
                {
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            else
            {
                if (_model == null)
                {
                    controlRef = _compModel.GetRefFromOffset(control.Value);
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    controlRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + control.Value };
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            RotateXGate gate = new RotateXGate(gamma, targetRef, controlRef);
            AddGate(gate);
        }

        public void RotateY(double gamma, int target, int? control = null)
        {
            Validate(target, "RotateY", "target");
            if (control.HasValue)
            {
                Validate(control.Value, "RotateY", "control");
            }

            RegisterRefModel targetRef;
            RegisterRefModel? controlRef;

            if (control == null)
            {
                controlRef = null;
                if (_model == null)
                {
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            else
            {
                if (_model == null)
                {
                    controlRef = _compModel.GetRefFromOffset(control.Value);
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    controlRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + control.Value };
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            RotateYGate gate = new RotateYGate(gamma, targetRef, controlRef);
            AddGate(gate);
        }

        public void RotateZ(double gamma, int target, int? control = null)
        {
            Validate(target, "RotateZ", "target");
            if (control.HasValue)
            {
                Validate(control.Value, "RotateZ", "control");
            }

            RegisterRefModel targetRef;
            RegisterRefModel? controlRef;

            if (control == null)
            {
                controlRef = null;
                if (_model == null)
                {
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            else
            {
                if (_model == null)
                {
                    controlRef = _compModel.GetRefFromOffset(control.Value);
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    controlRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + control.Value };
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            RotateZGate gate = new RotateZGate(gamma, targetRef, controlRef);
            AddGate(gate);
        }

        public void SigmaX(int target, int? control = null)
        {
            Validate(target, "SigmaX", "target");
            if (control.HasValue)
            {
                Validate(control.Value, "SigmaX", "control");
            }

            RegisterRefModel targetRef;
            RegisterRefModel? controlRef;

            if (control == null)
            {
                controlRef = null;
                if (_model == null)
                {
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
                SigmaXGate gate = new SigmaXGate(targetRef);
                AddGate(gate);
            }
            else
            {
                if (_model == null)
                {
                    controlRef = _compModel.GetRefFromOffset(control.Value);
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    controlRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + control.Value };
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
                CNotGate gate = new CNotGate(targetRef, controlRef.Value);
                AddGate(gate);
            }            
        }

        public void SigmaY(int target, int? control = null)
        {
            Validate(target, "SigmaY", "target");
            if (control.HasValue)
            {
                Validate(control.Value, "SigmaY", "control");
            }

            RegisterRefModel targetRef;
            RegisterRefModel? controlRef;

            if (control == null)
            {
                controlRef = null;
                if (_model == null)
                {
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            else
            {
                if (_model == null)
                {
                    controlRef = _compModel.GetRefFromOffset(control.Value);
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    controlRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + control.Value };
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            SigmaYGate gate = new SigmaYGate(targetRef, controlRef);
            AddGate(gate);
        }

        public void SigmaZ(int target, int? control = null)
        {
            Validate(target, "SigmaZ", "target");
            if (control.HasValue)
            {
                Validate(control.Value, "SigmaZ", "control");
            }

            RegisterRefModel targetRef;
            RegisterRefModel? controlRef;

            if (control == null)
            {
                controlRef = null;
                if (_model == null)
                {
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            else
            {
                if (_model == null)
                {
                    controlRef = _compModel.GetRefFromOffset(control.Value);
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    controlRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + control.Value };
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            SigmaZGate gate = new SigmaZGate(targetRef, controlRef);
            AddGate(gate);
        }

        public void SqrtX(int target, int? control = null)
        {
            Validate(target, "SqrtX", "target");
            if (control.HasValue)
            {
                Validate(control.Value, "SqrtX", "control");
            }

            RegisterRefModel targetRef;
            RegisterRefModel? controlRef;

            if (control == null)
            {
                controlRef = null;
                if (_model == null)
                {
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            else
            {
                if (_model == null)
                {
                    controlRef = _compModel.GetRefFromOffset(control.Value);
                    targetRef = _compModel.GetRefFromOffset(target);
                }
                else
                {
                    controlRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + control.Value };
                    targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                }
            }
            SqrtXGate gate = new SqrtXGate(targetRef, controlRef);
            AddGate(gate);
        }

        public void Toffoli(int target, params int[] controls)
        {
            ValidateToffoli(controls);
            Validate(target, "Toffoli", "target");
            for (int i = 0; i < controls.Length; i++)
            {
                Validate(controls[i], "Toffoli", "controls[" + i + "]");
            }

            if (_model == null)
            {
                RegisterRefModel[] toffoliParams = controls
                    .Select<int, RegisterRefModel>(x => _compModel.GetRefFromOffset(x))
                    .ToArray<RegisterRefModel>();
                ToffoliGate gate = new ToffoliGate(_compModel.GetRefFromOffset(target), toffoliParams);
                AddGate(gate);
            }
            else
            {
                RegisterRefModel[] toffoliParams = controls
                    .Select<int, RegisterRefModel>(x => new RegisterRefModel() { Register = _model, Offset = _offsetToModel + x })
                    .ToArray<RegisterRefModel>();
                RegisterRefModel targetRef = new RegisterRefModel() { Register = _model, Offset = _offsetToModel + target };
                ToffoliGate gate = new ToffoliGate(targetRef, toffoliParams);
                AddGate(gate);
            }
        }

        public override string ToString()
        {
            return _register.ToString();
        }
        #endregion // IRegister Methods


        #region Operators
        public static implicit operator RegisterRef(Register register)
        {
            return register[0];
        }
        #endregion // Operators


        #region Internal Methods and Properties

        public RegisterPartModel ToPartModel()
        {
            return new RegisterPartModel()
            {
                Register = _model,
                Offset = _offsetToModel,
                Width = _register.Width
            };
        }

        #endregion // Internal Methods and Properties


        #region Private Helpers
        private void AddGate(Gate gate)
        {
            QuantumComputer comp = QuantumComputer.GetInstance();
            comp.AddGate(gate);
        }

        private void ValidateIndex(int index)
        {
            string modelName = _model != null ? _model.Name : "root";
            if (index < 0)
            {
                StringBuilder sb = new StringBuilder("\nCannot reference to ");
                sb.Append(modelName).Append("[").Append(index).Append("]");
                throw new IndexOutOfRangeException(sb.ToString());
            }
            if (index >= _register.Width)
            {
                StringBuilder sb = new StringBuilder("\nCannot reference to ");
                sb.Append(modelName).Append("[").Append(index).Append("]");
                sb.Append(" - the register has only ").Append(_register.Width)
                    .Append(" qubits, numbered from 0 to ").Append(_register.Width - 1);
                throw new IndexOutOfRangeException(sb.ToString());
            }
        }

        private void ValidateOffsetWidth(int offset, int width)
        {
            string modelName = _model != null ? _model.Name : "root";
            if (offset < 0 || offset >= _register.Width)
            {
                StringBuilder sb = new StringBuilder("\nCannot reference to ");
                sb.Append(modelName).Append("[").Append(offset)
                    .Append(", ").Append(width).Append("]");
                sb.Append(" - invalid offset value (").Append(offset).Append(")");
                throw new IndexOutOfRangeException(sb.ToString());
            }
            if (width <= 0 || offset + width > _register.Width)
            {
                StringBuilder sb = new StringBuilder("\nCannot reference to ");
                sb.Append(modelName).Append("[").Append(offset)
                    .Append(", ").Append(width).Append("]");
                sb.Append(" - invalid width (").Append(width).Append(")");
                throw new IndexOutOfRangeException(sb.ToString());
            }
        }

        private void Validate(int index, string methodName, string paramName)
        {
            string modelName = _model != null ? _model.Name : "root";
            if (index < 0)
            {
                StringBuilder sb = new StringBuilder("\n");
                sb.Append(modelName).Append(".").Append(methodName).Append(": ");
                sb.Append("Invalid ").Append(paramName).Append(": ").Append(index);
                throw new ArgumentOutOfRangeException(paramName, index, sb.ToString());
            }
            if (index >= _register.Width)
            {
                StringBuilder sb = new StringBuilder("\n");
                sb.Append(modelName).Append(".").Append(methodName).Append(": ");
                sb.Append("Invalid ").Append(paramName).Append(": ").Append(index);
                sb.Append(" - the register has only ").Append(_register.Width)
                    .Append(" qubits, numbered from 0 to ").Append(_register.Width - 1);
                throw new ArgumentOutOfRangeException(paramName, index, sb.ToString());
            }
        }

        private void ValidateMatrix(Complex[,] matrix, string methodName, string paramName) 
        {
            string modelName = _model != null ? _model.Name : "root";
            if (!MatrixValidator.IsUnitary2x2(matrix))
            {
                StringBuilder sb = new StringBuilder("\n");
                sb.Append(modelName).Append(".").Append(methodName).Append(": ");
                sb.Append("The matrix is not unitary 2x2.");
                throw new ArgumentException(sb.ToString(), paramName);
            }
        }

        private void ValidateToffoli(int[] controls)
        {
            string modelName = _model != null ? _model.Name : "root";
            if (controls.Length < 2)
            {
                StringBuilder sb = new StringBuilder("\n");
                sb.Append(modelName).Append(".").Append("Toffoli").Append(": ");
                sb.Append("Too few control bits.");
                throw new ArgumentException(sb.ToString(), "controls");
            }
        }

        #endregion
    }
}
