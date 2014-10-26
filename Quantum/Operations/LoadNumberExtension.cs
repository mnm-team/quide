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
    public static class LoadNumberExtension
    {
        // controlled loading a number into register
        // using Toffoli gates
        // if controlBits are empty, the number is loaded unconditionally
        public static void LoadNumber(this QuantumComputer comp, Register target, ulong number, params RegisterRef[] controlBits)
        {
            Validate(target, number);

            int controlLength = controlBits.Length;           

            int i = 0;
            ulong tmpN = number;
            while (tmpN > 0)
            {
                int rest = (int)(tmpN % 2);
                tmpN = tmpN / 2;

                if (rest == 1)
                {
                    if (controlLength > 1)
                    {
                        comp.Toffoli(target[i], controlBits);
                    }
                    else if (controlLength > 0)
                    {
                        comp.CNot(target[i], controlBits[0]);
                    }
                    else
                    {
                        target.SigmaX(i);
                    }
                }
                i++;
            }
        }

        private static void Validate(Register target, ulong number)
        {
            if ((number >> target.Width) > 0)
            {
                throw new System.ArgumentException("Target register is too small. It must have enough space to store loaded number.");
            }
        }
    }
}
