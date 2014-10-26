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
    public static class QFTExtension
    {
        //public static void QFT(this QuantumComputer comp, Register register)
        //{
        //    int width = register.Width;
        //    for (int i = width - 1; i >= 0; i--)
        //    {
        //        register.Hadamard(i);
        //        for (int j = i - 1; j >= 0; j--)
        //        {
        //            register.CPhaseShift(i - j, i, j);
        //        }
        //    }
        //}

        //public static void InverseQFT(this QuantumComputer comp, Register register)
        //{
        //    int width = register.Width;
        //    for (int i = 0; i < width; i++)
        //    {
        //        for (int j = 0; j < i; j++)
        //        {
        //            register.InverseCPhaseShift(i - j, i, j);
        //        }
        //        register.Hadamard(i);
        //    }
        //}
        public static void QFT(this QuantumComputer comp, Register register)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, register };
                comp.AddParametricGate("QFT", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            int width = register.Width;
            for (int i = width - 1; i >= 0; i--)
            {
                for (int j = width - 1; j > i; j--)
                {
                    register.CPhaseShift(i - j, i, j);
                }
                register.Hadamard(i);
            }
        }

        public static void InverseQFT(this QuantumComputer comp, Register register)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, register };
                comp.AddParametricGate("InverseQFT", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            int width = register.Width;
            for (int i = 0; i < width; i++)
            {
                register.Hadamard(i);

                for (int j = i + 1; j < width; j++)
                {
                    register.InverseCPhaseShift(i - j, i, j);
                }
            }
        }
    }
}
