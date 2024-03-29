﻿/*
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

namespace Quantum.Operations
{
    /// <summary>
    ///     Extensions performing Swap operations.
    /// </summary>
    public static class SwapExtension
    {
        /// <summary>
        ///     Swaps the values of two given qubits.
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer" /> instance.</param>
        /// <param name="r1">The reference to the first swapped qubit.</param>
        /// <param name="r2">The reference to the second swapped qubit.</param>
        public static void Swap(this QuantumComputer comp, RegisterRef r1, RegisterRef r2)
        {
            var root = comp.GetRootRegister(r1, r2);
            var target1 = r1.OffsetToRoot;
            var target2 = r2.OffsetToRoot;

            root.CNot(target1, target2);
            root.CNot(target2, target1);
            root.CNot(target1, target2);
        }

        /// <summary>
        ///     The controlled Swap. Swaps the values of two given qubits, if the control qubit is set.
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer" /> instance.</param>
        /// <param name="r1">The reference to the first swapped qubit.</param>
        /// <param name="r2">The reference to the second swapped qubit.</param>
        /// <param name="control">The reference to the control qubit.</param>
        public static void Swap(this QuantumComputer comp, Register r1, Register r2, RegisterRef control)
        {
            Validate(r1, r2);

            var root = comp.GetRootRegister(r1, r2, control);
            var target1 = r1.OffsetToRoot;
            var target2 = r2.OffsetToRoot;
            var ctrl = control.OffsetToRoot;

            for (var i = 0; i < r1.Width; i++)
            {
                //comp.Toffoli(root[target1 + i], root[target2 + i], root[ctrl]);
                //comp.Toffoli(root[target2 + i], root[target1 + i], root[ctrl]);
                //comp.Toffoli(root[target1 + i], root[target2 + i], root[ctrl]);
                root.Toffoli(target1 + i, target2 + i, ctrl);
                root.Toffoli(target2 + i, target1 + i, ctrl);
                root.Toffoli(target1 + i, target2 + i, ctrl);
            }
        }

        public static void Reverse(this QuantumComputer comp, Register a)
        {
            for (var i = 0; i < a.Width / 2; i++) comp.Swap(a[i], a[a.Width - 1 - i]);
        }

        private static void Validate(Register a, Register b)
        {
            if (b.Width != a.Width)
                throw new ArgumentException("Register b must be exactly the same size as register a.");
        }
    }
}