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

namespace Quantum.Operations
{
    /// <summary>
    /// Simple extension for applying Hadamard gates on each qubit in <see cref="Quantum.Register"/>. 
    /// Such operation is called Walsh-Hadamard transform.
    /// </summary>
    public static class WalshExtension
    {
        /// <summary>
        /// Applies the Walsh-Hadamard transform on given register. 
        /// In other words, applies Hadamard gate on every qubit in the register.
        /// </summary>
        /// <param name="comp">The <see cref="Quantum.QuantumComputer"/> instance.</param>
        /// <param name="register">The <see cref="Quantum.Register"/> on which the operation is performed.</param>
        public static void Walsh(this QuantumComputer comp, Register register)
        {
            for (int i = 0; i < register.Width; i++)
            {
                register.Hadamard(i);
            }
        }
    }
}
