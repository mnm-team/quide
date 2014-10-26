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
    public static class WalshExtension
    {
        //Apply a walsh-hadamard transform on whole register
        public static void Walsh(this QuantumComputer comp, Register register)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, register };
                comp.AddParametricGate("Walsh", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            for (int i = 0; i < register.Width; i++)
            {
                register.Hadamard(i);
            }
        }

        public static void InverseWalsh(this QuantumComputer comp, Register register)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, register };
                comp.AddParametricGate("InverseWalsh", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            comp.Walsh(register);
        }
    }
}
