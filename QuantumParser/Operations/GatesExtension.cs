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

namespace QuantumParser.Operations
{
    public static class GatesExtension
    {
        public static void CNot(this QuantumComputer comp, RegisterRef target, RegisterRef control)
        {
            Register root = comp.GetRootRegister(control, target);
            int c = control.OffsetToRoot;
            int t = target.OffsetToRoot;

            root.CNot(t, c);
        }

        public static void Toffoli(this QuantumComputer comp, RegisterRef target, params RegisterRef[] controls)
        {
            Register root = comp.GetRootRegister(comp.GetRootRegister(controls), target);
            root.Toffoli(target.OffsetToRoot, controls.Select<RegisterRef, int>(x => x.OffsetToRoot).ToArray<int>());
        }

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
    }
}
