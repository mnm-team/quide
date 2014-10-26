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
using System.Numerics;
using System.Text;

namespace Quantum.Operations
{
    /// <summary>
    /// <para>
    /// Important extensions for performing any operation referencing more than one register, 
    /// e.g. C-NOT where the control bit is in different register than the target bit.
    /// </para>
    /// <para>
    /// This class contains every quantum gate operation implemented in <see cref="Quantum.Register"/> class, 
    /// which enables performing them on qubits in different registers.
    /// </para>
    /// </summary>
    public static class GatesExtension
    {
        /// <summary>
        /// <para>
        /// Performs a controlled not operation (C-NOT Gate).
        /// The target bit gets inverted if the control bit is enabled.
        /// The operation can be written as the unitary operation matrix:
        /// </para>
        /// <img src="../../Images/imgCNot.png" />
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer"/> instance.</param>
        /// <param name="target">The reference to target qubit.</param>
        /// <param name="control">The reference to control qubit.</param>
        public static void CNot(this QuantumComputer comp, RegisterRef target, RegisterRef control)
        {
            Register root = comp.GetRootRegister(control, target);
            int c = control.OffsetToRoot;
            int t = target.OffsetToRoot;

            root.CNot(t, c);
        }

        /// <summary>
        /// <para>
        /// Applies Toffoli gate. If all of the control bits are enabled, the target bit gets inverted.
        /// This gate with more than two control bits is not considered elementary and is not available on all physical realizations of a quantum computer.
        /// Toffoli gate with two control bits can be represented by unitary matrix:
        /// </para>
        /// <img src="../../Images/imgToffoli.png" />
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer"/> instance.</param>
        /// <param name="target">The reference to target qubit.</param>
        /// <param name="controls">The references to control qubits. There must be at least two control qubits.</param>
        public static void Toffoli(this QuantumComputer comp, RegisterRef target, params RegisterRef[] controls)
        {
            Register root = comp.GetRootRegister(comp.GetRootRegister(controls), target);
            root.Toffoli(target.OffsetToRoot, controls.Select<RegisterRef, int>(x => x.OffsetToRoot).ToArray<int>());
        }

        /// <summary>
        /// <para>
        /// Performs a conditional phase kick (or phase shift) on the registers' state by the angle PI / 2 ^ dist.
        /// The operation is represented by the unitary matrix:
        /// </para>
        /// <img src="../../Images/imgCPhase.png" />
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer"/> instance.</param>
        /// <param name="dist">Value of k in the angle of phase shift. The angle equals: PI / 2 ^ k.</param>
        /// <param name="target">The reference to target qubit.</param>
        /// <param name="controls">The references to control qubits. If there is no control qubit given, the gate is run unconditionally.</param>
        public static void CPhaseShift(this QuantumComputer comp, int dist, RegisterRef target, params RegisterRef[] controls)
        {
            if (controls.Length == 0)
            {
                Register root = comp.GetRootRegister(target);
                int t = target.OffsetToRoot;
                root.CPhaseShift(dist, t);
            }
            else
            {
                Register root = comp.GetRootRegister(target, comp.GetRootRegister(controls));
                int t = target.OffsetToRoot;
                root.CPhaseShift(dist, t, controls.Select<RegisterRef, int>(x => x.OffsetToRoot).ToArray<int>());
            }
        }

        /// <summary>
        /// <para>
        /// Performs any arbitrary unitary operation on target qubit. The operation is described by unitary matrix of complex numbers, as follows:
        /// </para>
        /// <img src="../../Images/imgGate1.png" />
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer"/> instance.</param>
        /// <param name="matrix">The 2x2 matrix of complex numbers, which describes the unitary operation.</param>
        /// <param name="target">The reference to target qubit.</param>
        /// <param name="control">Optional argument. If given, the method performs controlled gate operation. Destribes the reference to control qubit.</param>
        public static void Gate1(this QuantumComputer comp, Complex[,] matrix, RegisterRef target, RegisterRef? control = null)
        {
            if (control.HasValue)
            {
                Register root = comp.GetRootRegister(control.Value, target);
                int c = control.Value.OffsetToRoot;
                int t = target.OffsetToRoot;
                root.Gate1(matrix, t, c);
            }
            else
            {
                Register root = comp.GetRootRegister(target);
                int t = target.OffsetToRoot;
                root.Gate1(matrix, t);
            }
        }

        /// <summary>
        /// <para>
        /// Applies the Hadamard gate to the target qubit.
        /// The unitary matrix for this operation is:
        /// </para>
        /// <img src="../../Images/imgH.png" />
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer"/> instance.</param>
        /// <param name="target">The reference to target qubit.</param>
        /// <param name="control">Optional argument. If given, the method performs controlled gate operation. 
        /// Destribes the reference to control qubit.</param>
        public static void Hadamard(this QuantumComputer comp, RegisterRef target, RegisterRef? control = null)
        {
            if (control.HasValue)
            {
                Register root = comp.GetRootRegister(control.Value, target);
                int c = control.Value.OffsetToRoot;
                int t = target.OffsetToRoot;
                root.Hadamard(t, c);
            }
            else
            {
                Register root = comp.GetRootRegister(target);
                int t = target.OffsetToRoot;
                root.Hadamard(t);
            }
        }

        /// <summary>
        /// <para>
        /// Performs a inversed conditional phase kick (or phase shift) on the registers' state by the angle PI / 2 ^ dist.
        /// The operation is represented by the unitary matrix:
        /// </para>
        /// <img src="../../Images/imgInvCPhase.png" />
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer"/> instance.</param>
        /// <param name="dist">Value of k in the angle of phase shift. The angle equals: - PI / 2 ^ k.</param>
        /// <param name="target">The reference to target qubit.</param>
        /// <param name="controls">The references to control qubits. If there is no control qubit given, the gate is run unconditionally.</param>
        public static void InverseCPhaseShift(this QuantumComputer comp, int dist, RegisterRef target, params RegisterRef[] controls)
        {
            if (controls.Length == 0)
            {
                Register root = comp.GetRootRegister(target);
                int t = target.OffsetToRoot;
                root.InverseCPhaseShift(dist, t);
            }
            else
            {
                Register root = comp.GetRootRegister(target, comp.GetRootRegister(controls));
                int t = target.OffsetToRoot;
                root.InverseCPhaseShift(dist, t, controls.Select<RegisterRef, int>(x => x.OffsetToRoot).ToArray<int>());
            }  
        }

        /// <summary>
        /// <para>
        /// Performs a phase kick (or phase shift) on the the registers' state.
        /// The operation is represented by the unitary matrix:
        /// </para>
        /// <img src="../../Images/imgPhaseKick.png" />
        /// <para>
        /// The controlled version of this operation can be written as an other unitary matrix:
        /// </para>
        /// <img src="../../Images/imgCPhaseKick.png" />
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer"/> instance.</param>
        /// <param name="gamma">The phase value.</param>
        /// <param name="target">The reference to target qubit.</param>
        /// <param name="control">Optional argument. If given, the method performs controlled gate operation. 
        /// Destribes the reference to control qubit.</param>
        public static void PhaseKick(this QuantumComputer comp, double gamma, RegisterRef target, params RegisterRef[] controls)
        {
            if (controls.Length == 0)
            {
                Register root = comp.GetRootRegister(target);
                int t = target.OffsetToRoot;
                root.PhaseKick(gamma, t);
            }
            else
            {
                Register root = comp.GetRootRegister(target, comp.GetRootRegister(controls));
                int t = target.OffsetToRoot;
                root.PhaseKick(gamma, t, controls.Select<RegisterRef, int>(x => x.OffsetToRoot).ToArray<int>());
            }
        }

        /// <summary>
        /// <para>
        /// Adds a global phase on the registers' state.
        /// The operation is represented by the unitary matrix:
        /// </para>
        /// <img src="../../Images/imgPhaseScale.png" />
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer"/> instance.</param>
        /// <param name="gamma">The phase that is added to the registers' state.</param>
        /// <param name="target">The reference to target qubit.</param>
        /// <param name="control">Optional argument. If given, the method performs controlled gate operation. 
        /// Destribes the reference to control qubit.</param>
        public static void PhaseScale(this QuantumComputer comp, double gamma, RegisterRef target, RegisterRef? control = null)
        {
            if (control.HasValue)
            {
                Register root = comp.GetRootRegister(control.Value, target);
                int c = control.Value.OffsetToRoot;
                int t = target.OffsetToRoot;
                root.PhaseScale(gamma, t, c);
            }
            else
            {
                Register root = comp.GetRootRegister(target);
                int t = target.OffsetToRoot;
                root.PhaseScale(gamma, t);
            }
        }

        /// <summary>
        /// <para>
        /// Performs a rotation of the target qubit about the x-axis of the Bloch sphere.
        /// The angle of rotation is given in first argument (double gamma).
        /// The unitary matrix of this operation is:
        /// </para>
        /// <img src="../../Images/imgRotateX.png" />
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer"/> instance.</param>
        /// <param name="gamma">The angle of rotation.</param>
        /// <param name="target">The reference to target qubit.</param>
        /// <param name="control">Optional argument. If given, the method performs controlled gate operation. 
        /// Destribes the reference to control qubit.</param>
        public static void RotateX(this QuantumComputer comp, double gamma, RegisterRef target, RegisterRef? control = null)
        {
            if (control.HasValue)
            {
                Register root = comp.GetRootRegister(control.Value, target);
                int c = control.Value.OffsetToRoot;
                int t = target.OffsetToRoot;
                root.RotateX(gamma, t, c);
            }
            else
            {
                Register root = comp.GetRootRegister(target);
                int t = target.OffsetToRoot;
                root.RotateX(gamma, t);
            }
        }

        /// <summary>
        /// <para>
        /// Performs a rotation of the target qubit about the y-axis of the Bloch sphere.
        /// The angle of rotation is given in first argument (double gamma).
        /// The operation is represented by the unitary matrix:
        /// </para>
        /// <img src="../../Images/imgRotateY.png" />
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer"/> instance.</param>
        /// <param name="gamma">The angle of rotation.</param>
        /// <param name="target">The reference to target qubit.</param>
        /// <param name="control">Optional argument. If given, the method performs controlled gate operation. 
        /// Destribes the reference to control qubit.</param>
        public static void RotateY(this QuantumComputer comp, double gamma, RegisterRef target, RegisterRef? control = null)
        {
            if (control.HasValue)
            {
                Register root = comp.GetRootRegister(control.Value, target);
                int c = control.Value.OffsetToRoot;
                int t = target.OffsetToRoot;
                root.RotateY(gamma, t, c);
            }
            else
            {
                Register root = comp.GetRootRegister(target);
                int t = target.OffsetToRoot;
                root.RotateY(gamma, t);
            }
        }

        /// <summary>
        /// <para>
        /// Performs a rotation of the target qubit about the z-axis of the Bloch sphere.
        /// The angle of rotation is given in first argument (double gamma).
        /// The operation is represented by the unitary matrix:
        /// </para>
        /// <img src="../../Images/imgRotateZ.png" />
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer"/> instance.</param>
        /// <param name="gamma">The angle of rotation.</param>
        /// <param name="target">The reference to target qubit.</param>
        /// <param name="control">Optional argument. If given, the method performs controlled gate operation. 
        /// Destribes the reference to control qubit.</param>
        public static void RotateZ(this QuantumComputer comp, double gamma, RegisterRef target, RegisterRef? control = null)
        {
            if (control.HasValue)
            {
                Register root = comp.GetRootRegister(control.Value, target);
                int c = control.Value.OffsetToRoot;
                int t = target.OffsetToRoot;
                root.RotateZ(gamma, t, c);
            }
            else
            {
                Register root = comp.GetRootRegister(target);
                int t = target.OffsetToRoot;
                root.RotateZ(gamma, t);
            }
        }

        /// <summary>
        /// <para>
        /// Performs a Sigma X Pauli's Gate on target qubit. Actually, it is a simple Not.
        /// The unitary operation matrix is:
        /// </para>
        /// <img src="../../Images/imgSigmaX.png" /> 
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer"/> instance.</param>
        /// <param name="target">The reference to target qubit.</param>
        /// <param name="control">Optional argument. If given, the method performs controlled gate operation. 
        /// Destribes the reference to control qubit.</param>
        public static void SigmaX(this QuantumComputer comp, RegisterRef target, RegisterRef? control = null)
        {
            if (control.HasValue)
            {
                Register root = comp.GetRootRegister(control.Value, target);
                int c = control.Value.OffsetToRoot;
                int t = target.OffsetToRoot;
                root.CNot(t, c);
            }
            else
            {
                Register root = comp.GetRootRegister(target);
                int t = target.OffsetToRoot;
                root.SigmaX(t);
            }
        }

        /// <summary>
        /// <para>
        /// Performs a Sigma Y Pauli's Gate on target qubit.
        /// The operation is represented by unitary matrix:
        /// </para>
        /// <img src="../../Images/imgSigmaY.png" />
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer"/> instance.</param>
        /// <param name="target">The reference to target qubit.</param>
        /// <param name="control">Optional argument. If given, the method performs controlled gate operation. 
        /// Destribes the reference to control qubit.</param>
        public static void SigmaY(this QuantumComputer comp, RegisterRef target, RegisterRef? control = null)
        {
            if (control.HasValue)
            {
                Register root = comp.GetRootRegister(control.Value, target);
                int c = control.Value.OffsetToRoot;
                int t = target.OffsetToRoot;
                root.SigmaY(t, c);
            }
            else
            {
                Register root = comp.GetRootRegister(target);
                int t = target.OffsetToRoot;
                root.SigmaY(t);
            }
        }

        /// <summary>
        /// <para>
        /// Performs a Sigma Z Pauli's Gate on target qubit.
        /// The operation is represented by unitary matrix:
        /// </para>
        /// <img src="../../Images/imgSigmaZ.png" />
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer"/> instance.</param>
        /// <param name="target">The reference to target qubit.</param>
        /// <param name="control">Optional argument. If given, the method performs controlled gate operation. 
        /// Destribes the reference to control qubit.</param>
        public static void SigmaZ(this QuantumComputer comp, RegisterRef target, RegisterRef? control = null)
        {
            if (control.HasValue)
            {
                Register root = comp.GetRootRegister(control.Value, target);
                int c = control.Value.OffsetToRoot;
                int t = target.OffsetToRoot;
                root.SigmaZ(t, c);
            }
            else
            {
                Register root = comp.GetRootRegister(target);
                int t = target.OffsetToRoot;
                root.SigmaZ(t);
            }
        }

        /// <summary>
        /// <para>
        /// Performs the Square Root of Not on the target qubit.
        /// The Square Root of Not Gate is such a gate, that applied twice, performs Not operation.
        /// The operation can be represented as the unitary matrix:
        /// </para>
        /// <img src="../../Images/imgSqrtX.png" />
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer"/> instance.</param>
        /// <param name="target">The reference to target qubit.</param>
        /// <param name="control">Optional argument. If given, the method performs controlled gate operation. 
        /// Destribes the reference to control qubit.</param>
        public static void SqrtX(this QuantumComputer comp, RegisterRef target, RegisterRef? control = null)
        {
            if (control.HasValue)
            {
                Register root = comp.GetRootRegister(control.Value, target);
                int c = control.Value.OffsetToRoot;
                int t = target.OffsetToRoot;
                root.SqrtX(t, c);
            }
            else
            {
                Register root = comp.GetRootRegister(target);
                int t = target.OffsetToRoot;
                root.SqrtX(t);
            }
        }

        public static void Reset(this QuantumComputer comp, Register register, ulong newValue = 0)
        {
            register.Reset(newValue);
        }
    }
}
