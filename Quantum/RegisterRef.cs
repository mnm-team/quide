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

namespace Quantum
{
    /// <summary>
    /// This structure represents a reference to a qubit in quantum register.
    /// </summary>
    public struct RegisterRef
    {
        /// <summary>
        /// The register containing the referenced qubit.
        /// </summary>
        public Register Register
        {
            get;
            set;
        }

        /// <summary>
        /// The offset of referenced qubit in the <see cref="RegisterRef.Register"/>. 
        /// 0 means the Least Significant Bit.
        /// </summary>
        public int Offset
        {
            get;
            set;
        }

        /// <summary>
        /// <para>
        /// The offset of referenced qubit, but in the root register. 
        /// It is the same as <see cref="RegisterRef.Offset"/>, when the <see cref="RegisterRef.Register"/> is independent. 
        /// </para>
        /// <para>
        /// The idea of root register is described in <see cref="QuantumComputer.GetRootRegister"/> ,
        /// or in <see cref="Quantum.Register.GetAmplitudes"/>.
        /// </para>
        /// </summary>
        public int OffsetToRoot
        {
            get
            {
                return Register.OffsetToRoot + Offset;
            }
        }
    }
}
