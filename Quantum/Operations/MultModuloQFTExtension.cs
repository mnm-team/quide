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
    public static class MultModuloQFTExtension
    {
        public static void MultModuloQFT(this QuantumComputer comp, ulong a, ulong N, Register x, Register b, RegisterRef control)
        {
            Register ctrl = comp.NewRegister(0, 1);
            comp.MultModuloQFT(a, N, ctrl, x, b, control);
        }

        public static void InverseMultModuloQFT(this QuantumComputer comp, ulong a, ulong N, Register x, Register b, RegisterRef control)
        {
            Register ctrl = comp.NewRegister(0, 1);
            comp.InverseMultModuloQFT(a, N, ctrl, x, b, control);
        }

        public static void MultModuloQFT(this QuantumComputer comp, ulong a, ulong N, RegisterRef ctrl, Register x, Register b, RegisterRef control)
        {
            Validate(a, b, N);
            comp.QFT(b);

            for (int i = 0; i < x.Width; i++)
            {
                //Console.WriteLine("MultModulo i = {2}, a = {0}, N = {1}", a, N, i);
                comp.AddModuloQFTPhi(((((ulong)1 << i) * a) % N), N, ctrl, b, x[i], control);
            }

            comp.InverseQFT(b);
        }

        public static void InverseMultModuloQFT(this QuantumComputer comp, ulong a, ulong N, RegisterRef ctrl, Register x, Register b, RegisterRef control)
        {
            Validate(a, b, N);
            comp.QFT(b);

            for (int i = x.Width - 1; i >= 0; i--)
            {
                //Console.WriteLine("IverseMultModulo i = {2}, a = {0}, N = {1}", a, N, i);
                comp.InverseAddModuloQFTPhi(((((ulong)1 << i) * a) % N), N, ctrl, b, x[i], control);
            }

            comp.InverseQFT(b);
        }

        private static void Validate(ulong a, Register b, ulong N)
        {

            if (b.Width < Utils.CalculateRegisterWidth(N) + 1)
            {
                //throw new System.ArgumentException("Register b must be able to contain N + 1 bit");
            }
        }
    }
}
