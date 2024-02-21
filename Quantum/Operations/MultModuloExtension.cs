/*
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
    public static class MultModuloExtension
    {
        // register x - initially loaded with x
        // register N - initially loaded with N
        // control - control bit
        // other registers - initially 0
        // after computation register B changes and contains: [(a*x) mod N]
        // Insecure version: registers widths etc. are not checked
        public static void CMultModulo(
            this QuantumComputer comp,
            Register a,
            Register b,
            Register c,
            Register N,
            Register x,
            RegisterRef control,
            ulong valueA,
            ulong valueN)
        {
            ulong power2 = 1;
            for (var i = 0; i < x.Width; i++, power2 *= 2)
            {
                // loading A register with (2^i * a) mod N
                var toLoad = valueA * power2 % valueN;
                comp.LoadNumber(a, toLoad, control, x[i]);

                // adding [(2^i * a) + B] modulo N
                comp.AddModulo(a, b, c, N, valueN);

                // unloading [(2^i * a) mod N] from A register
                comp.LoadNumber(a, toLoad, control, x[i]);
            }

            // if control == 0
            // then B register contains still 0
            // so we copy X register into B register
            comp.SigmaX(control);
            for (var i = 0; i < x.Width; i++) comp.Toffoli(b[i], control, x[i]);
            comp.SigmaX(control);
        }

        // register x - initially loaded with x
        // register b - initially loaded with 0
        // after computation register b changes and contains: [(a*x) mod N]
        // Secure version: throws Exceptions if arguments are invalid
        public static void CMultModulo(
            this QuantumComputer comp,
            Register x,
            Register b,
            RegisterRef control,
            ulong valueA,
            ulong valueN)
        {
            Validate(x, b, valueN);

            var a = comp.NewRegister(0, x.Width - 1);
            var c = comp.NewRegister(0, x.Width);
            var N = comp.NewRegister(valueN, x.Width - 1);

            comp.CMultModulo(a, b, c, N, x, control, valueA, valueN);

            comp.DeleteRegister(ref N);
            comp.DeleteRegister(ref c);
            comp.DeleteRegister(ref a);
        }

        // Insecure version: registers widths etc. are not checked
        public static void InverseCMultModulo(
            this QuantumComputer comp,
            Register a,
            Register b,
            Register c,
            Register N,
            Register x,
            RegisterRef control,
            ulong valueA,
            ulong valueN)
        {
            // if control == 0
            // then the X register is copied into B register
            // so we uncopy it remaining 0
            comp.SigmaX(control);
            for (var i = 0; i < x.Width; i++) comp.Toffoli(b[i], control, x[i]);
            comp.SigmaX(control);

            var power2 = (ulong)Math.Pow(2, x.Width - 1);
            for (var i = x.Width - 1;
                 i >= 0;
                 i--, power2 /= 2)
            {
                // loading A register with (2^i * a) mod N
                var toLoad = valueA * power2 % valueN;
                comp.LoadNumber(a, toLoad, control, x[i]);

                // inverse adding [(2^i * a) + B] modulo N
                comp.InverseAddModulo(a, b, c, N, valueN);

                // unloading [(2^i * a) mod N] from A register
                comp.LoadNumber(a, toLoad, control, x[i]);
            }
        }

        // Secure version: throws Exceptions if arguments are invalid
        public static void InverseCMultModulo(
            this QuantumComputer comp,
            Register x,
            Register b,
            RegisterRef control,
            ulong valueA,
            ulong valueN)
        {
            Validate(x, b, valueN);

            var a = comp.NewRegister(0, x.Width - 1);
            var c = comp.NewRegister(0, x.Width);
            var N = comp.NewRegister(valueN, x.Width - 1);

            comp.InverseCMultModulo(a, b, c, N, x, control, valueA, valueN);

            comp.DeleteRegister(ref N);
            comp.DeleteRegister(ref c);
            comp.DeleteRegister(ref a);
        }

        private static void Validate(
            Register x,
            Register b,
            ulong valueN)
        {
            if (b.Width != x.Width) throw new ArgumentException("Registers b and x must have the same width.");
            if (valueN >> (x.Width - 1) > 0)
                throw new ArgumentException(
                    "Register x is too small. It must have enough space to store N, and one qubit more.");
        }
    }
}