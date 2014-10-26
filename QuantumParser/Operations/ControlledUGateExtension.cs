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
using System.Text;
using System.Threading.Tasks;

namespace QuantumParser.Operations
{
    public static class ControlledUGateExtension
    {
        public static void ControlledUaGate(this QuantumComputer comp, ulong a, ulong N, Register x, RegisterRef control)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a, N, x, control };
                comp.AddParametricGate("ControlledUaGate", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            Register ctrl = comp.NewRegister(0, 1);
            Register reg0 = comp.NewRegister(0, x.Width);
            comp.ControlledUaGate(a, N, ctrl, x, reg0, control);
        }

        public static void ControlledUaGate(this QuantumComputer comp, ulong a, ulong N, RegisterRef ctrl, Register x, Register reg0, RegisterRef control)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a, N, ctrl, x, reg0, control };
                comp.AddParametricGate("ControlledUaGate", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            Validate(x, N);

            int? invA = Quantum.Utils.InversionModulo((int)a, (int)N);

            if (invA == null)
            {
                throw new ArgumentException("No inversion for specified a = " + a);
            }

            comp.MultModuloQFT(a, N, ctrl, x, reg0, control);
            comp.Swap(x, reg0, control);
            comp.InverseMultModuloQFT((ulong)invA, N, ctrl, x, reg0, control);

        }

        public static void InverseControlledUaGate(this QuantumComputer comp, ulong a, ulong N, Register x, RegisterRef control)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a, N, x, control };
                comp.AddParametricGate("InverseControlledUaGate", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            Register ctrl = comp.NewRegister(0, 1);
            Register reg0 = comp.NewRegister(0, x.Width);
            comp.InverseControlledUaGate(a, N, ctrl, x, reg0, control);
        }

        public static void InverseControlledUaGate(this QuantumComputer comp, ulong a, ulong N, RegisterRef ctrl, Register x, Register reg0, RegisterRef control)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a, N, ctrl, x, reg0, control };
                comp.AddParametricGate("InverseControlledUaGate", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            Validate(x, N);

            int? invA = Quantum.Utils.InversionModulo((int)a, (int)N);

            if (invA == null)
            {
                throw new ArgumentException("No inversion for specified a");
            }

            comp.MultModuloQFT((ulong)invA, N, ctrl, x, reg0, control);
            comp.Swap(reg0, x, control);
            comp.InverseMultModuloQFT(a, N, ctrl, x, reg0, control);

        }

        private static void Validate(Register b, ulong N)
        {

            if (b.Width < Quantum.Utils.CalculateRegisterWidth(N) + 1)
            {
                throw new System.ArgumentException("Register b must be able to contain N + 1 bit");
            }
        }

    }
}
