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

namespace QuantumParser.Operations
{
    public static class SwapExtension
    {
        // Swap two bits
        public static void Swap(this QuantumComputer comp, RegisterRef r1, RegisterRef r2)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, r1, r2 };
                comp.AddParametricGate("Swap", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            Register root = comp.GetRootRegister(r1, r2);
            int target1 = r1.OffsetToRoot;
            int target2 = r2.OffsetToRoot;

            root.CNot(target1, target2);
            root.CNot(target2, target1);
            root.CNot(target1, target2);
        }

        public static void InverseSwap(this QuantumComputer comp, RegisterRef r1, RegisterRef r2)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, r1, r2 };
                comp.AddParametricGate("InverseSwap", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            comp.Swap(r1, r2);
        }

        public static void Swap(this QuantumComputer comp, Register r1, Register r2, RegisterRef control)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, r1, r2, control };
                comp.AddParametricGate("Swap", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            Validate(r1, r2);

            Register root = comp.GetRootRegister(r1, r2, control);
            int target1 = r1.OffsetToRoot;
            int target2 = r2.OffsetToRoot;
            int ctrl = control.OffsetToRoot;

            for (int i = 0; i < r1.Width; i++)
            {
                //comp.Toffoli(root[target1 + i], root[target2 + i], root[ctrl]);
                //comp.Toffoli(root[target2 + i], root[target1 + i], root[ctrl]);
                //comp.Toffoli(root[target1 + i], root[target2 + i], root[ctrl]);
                root.Toffoli(target1 + i, target2 + i, ctrl);
                root.Toffoli(target2 + i, target1 + i, ctrl);
                root.Toffoli(target1 + i, target2 + i, ctrl);
            }
        }

        public static void InverseSwap(this QuantumComputer comp, Register r1, Register r2, RegisterRef control)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, r1, r2, control };
                comp.AddParametricGate("InverseSwap", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            comp.Swap(r1, r2, control);
        }

        public static void Reverse(this QuantumComputer comp, Register a)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a };
                comp.AddParametricGate("Reverse", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            for (int i = 0; i < a.Width / 2; i++)
            {
                comp.Swap(a[i], a[a.Width - 1 - i]);
            }
        }

        public static void InverseReverse(this QuantumComputer comp, Register a)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a };
                comp.AddParametricGate("InverseReverse", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            comp.Reverse(a);
        }

        private static void Validate(Register a, Register b)
        {
            if (b.Width != a.Width)
            {
                throw new System.ArgumentException("Register b must be exactly the same size as register a.");
            }
        }
    }
}
