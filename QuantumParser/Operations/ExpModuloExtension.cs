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
    // Quantum networks for elementary arithmetic operations
    // Vlatko Vedral, Adriano Barenco, and Artur Ekert
    // JULY 1996
    // page 149
    // FIG. 2 and FIG. 3
    public static class ExpModuloExtension
    {
        // register X - initially loaded with x
        // register X_1 - initially loaded with 1
        // W = width(a) = width(N)
        // width(b) = width(c) = width(x1) = W + 1
        // width(X) is 2 * W
        // register N - initially loaded with N
        // other registers - initially 0
        // after computation register X_1 changes and contains: [(a^x) mod N]
        // Insecure version: registers widths etc. are not checked
        public static void ExpModulo(
            this QuantumComputer comp,
            Register a,
            Register b,
            Register c,
            Register N,
            Register x1,
            Register x,
            int valueA,
            int valueN)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a, b, c, N, x1, x, valueA, valueN };
                comp.AddParametricGate("ExpModulo", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            bool firstRegisterB = false;
            int pow_a_2 = valueA;

            for (int i = 0; i < x.Width; i++)
            {
                // finding the inversion modulo of pow_a_2
                int inv_mod = InversionModulo(pow_a_2, valueN);

                if (firstRegisterB)
                {
                    comp.CMultModulo(a, x1, c, N, b, x[i],
                        (ulong)pow_a_2, (ulong)valueN);
                    comp.InverseCMultModulo(a, b, c, N, x1, x[i],
                        (ulong)inv_mod, (ulong)valueN);
                }
                else
                {
                    comp.CMultModulo(a, b, c, N, x1, x[i],
                        (ulong)pow_a_2, (ulong)valueN);
                    comp.InverseCMultModulo(a, x1, c, N, b, x[i],
                        (ulong)inv_mod, (ulong)valueN);
                }
                pow_a_2 = (pow_a_2 * pow_a_2) % valueN;
                firstRegisterB = !firstRegisterB;
            }
        }


        // register X - initially loaded with x
        // register X_1 - initially loaded with 1
        // width(x1) = W + 1
        // width(X) is 2 * W
        // after computation register X_1 changes and contains: [(a^x) mod N]
        // Secure version: throws Exceptions if arguments are invalid
        public static void ExpModulo(
            this QuantumComputer comp,
            Register x,
            Register x1,
            int valueA,
            int valueN)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, x, x1, valueA, valueN };
                comp.AddParametricGate("ExpModulo", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            Validate(x, x1, valueN);

            Register a = comp.NewRegister(0, x1.Width - 1);
            Register b = comp.NewRegister(0, x1.Width);
            Register c = comp.NewRegister(0, x1.Width);
            Register N = comp.NewRegister((ulong)valueN, x1.Width - 1);

            comp.ExpModulo(a, b, c, N, x1, x, valueA, valueN);

            comp.DeleteRegister(ref N);
            comp.DeleteRegister(ref c);
            comp.DeleteRegister(ref b);
            comp.DeleteRegister(ref a);
        }

        private static void Validate(
            Register x,
            Register x1,
            int valueN)
        {
            if (x.Width != 2 * (x1.Width - 1))
            {
                throw new System.ArgumentException("Width(x) must equal 2 * ( width(x1) - 1 )");
            }
            if ((valueN >> x1.Width - 1) > 0)
            {
                throw new System.ArgumentException("Register x1 is too small. It must have enough space to store N, and one qubit more.");
            }
        }

        public static int InversionModulo(int a, int N)
        {
            int u = 1;
            int w = a;
            int x = 0;
            int z = N;

            while (w != 0)
            {
                if (w < z)
                {
                    int tmp = u;
                    u = x;
                    x = tmp;

                    tmp = w;
                    w = z;
                    z = tmp;
                }
                int q = w / z; // obliczamy iloraz całkowity
                u = u - (q * x); // od równania (1) odejmujemy równanie (2) wymnożone przez q
                w = w - (q * z);
            }
            if (z != 1)
            { // dla z różnego od 1 nie istnieje odwrotność modulo
                return -1;
            }
            if (x < 0)
            {
                x = x + N; // ujemne x sprowadzamy do wartości dodatnich
            }
            return x; // x jest poszukiwaną odwrotnością modulo
        }
    }
}
