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

namespace Quantum.Operations
{
    public static class TrivialInverseExtension
    {
        public static void InverseSum(this QuantumComputer comp,
            RegisterRef refA, RegisterRef refB, RegisterRef refTarget)
        {
            comp.Sum(refA, refB, refTarget);
        }

        public static void InverseLoadNumber(this QuantumComputer comp, Register target, ulong number, params RegisterRef[] controlBits)
        {
            comp.LoadNumber(target, number, controlBits);
        }

        public static void InverseSwap(this QuantumComputer comp, RegisterRef r1, RegisterRef r2)
        {
            comp.Swap(r1, r2);
        }

        public static void InverseSwap(this QuantumComputer comp, Register r1, Register r2, RegisterRef control)
        {
            comp.Swap(r1, r2, control);
        }

        public static void InverseReverse(this QuantumComputer comp, Register a)
        {
            comp.Reverse(a);
        }

        public static void InverseWalsh(this QuantumComputer comp, Register register)
        {
            comp.Walsh(register);
        }
    }
}
