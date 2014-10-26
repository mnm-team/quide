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
    /// Extension methods for performing modular exponentiation of quantum registers.
    /// </summary>
    /// <remarks>
    /// The implementation is based on "Quantum networks for elementary arithmetic operations",
    /// Vlatko Vedral, Adriano Barenco, and Artur Ekert,
    /// JULY 1996,
    /// page 149,
    /// FIG. 2 and FIG. 3
    /// </remarks>
    public static class ExpModuloExtension
    {
        /// <summary>
        /// <para>
        /// Performs (a^x) modulo N, for given integers a and N. The x (one value or a superposition) 
        /// is given in the input register x.
        /// </para>
        /// <para>
        /// After computation, the result (or results, when x stores superposition of multiple integers) is stored in 
        /// register x1.
        /// </para>
        /// <para>
        /// This method is a variant of <c>ExpModulo</c> function, which operates directly on quantum registers given as arguments.
        /// It neither allocates nor frees any additional registers. It is thus recommended, when there is a need for performing
        /// modular exponentiation repeatedly. However, this variant has strict requirements for the width of each given register
        /// and if they are not fullfilled, the method gives unexpected results.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// There are precise requirements for the width of each register given as argument. They result from a need for carry bits,
        /// overflow flag and a requirement for ensuring that the operation is inversible.
        /// </para>
        /// <para>
        /// Let <b>WIDTH</b> equals the number of bits required to store N.
        /// </para>
        /// <para>
        /// The width of x register must equal <b>2 * WIDTH</b>. This value results from the requirements of Peter Shor's
        /// algorithm. Such a width ensures that the probability of getting the right result will be enough high.
        /// </para>
        /// </remarks>
        /// <param name="comp">The QuantumComputer instance.</param>
        /// <param name="a">Accumulator register. Its initial value must be 0. Its width must equal <b>WIDTH</b> (See Remarks).</param>
        /// <param name="b">Helper register. Its initial value must be 0. Its width must equal <b>WIDTH + 1</b> (See Remarks).</param>
        /// <param name="c">Register for storing carry bits. Its initial value must be 0. Its width must equal <b>WIDTH + 1</b> (See Remarks).</param>
        /// <param name="N">Register for N. Its initial value must equal N. Its width must equal <b>WIDTH</b> (See Remarks).</param>
        /// <param name="x1">Output register. Its initial value must equal 1. Its width must equal <b>WIDTH + 1</b> (See Remarks).</param>
        /// <param name="x">Register for x. Its initial value could be any integer or a superposition of multiple integers. Its width must equal <b>2 * WIDTH</b> (See Remarks).</param>
        /// <param name="valueA">Integer value of a. (For computing (a^x) modulo N).</param>
        /// <param name="valueN">Integer value of N. (For computing (a^x) modulo N).</param>
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

        /// <summary>
        /// <para>
        /// Performs (a^x) modulo N, for given integers a and N. The x (one value or a superposition) 
        /// is given in the input register x.
        /// </para>
        /// <para>
        /// After computation, the result (or results, when x stores superposition of multiple integers) is stored in 
        /// register x1.
        /// </para>
        /// <para>
        /// This method is a simple variant of 
        /// <see cref="ExpModulo(QuantumComputer, Register, Register, Register, Register, Register, Register, int, int)"/>.
        /// It allocates additional registers and frees them at the end. 
        /// This variant has also requirements for the width of each given register
        /// but if they are not fullfilled, the exception is thrown.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// There are precise requirements for the width of each register given as argument. They result from a need for carry bits,
        /// overflow flag and a requirement for ensuring that the operation is inversible.
        /// </para>
        /// <para>
        /// Let <b>WIDTH</b> equals the number of bits required to store N.
        /// </para>
        /// <para>
        /// The width of x register must equal <b>2 * WIDTH</b>. This value results from the requirements of Peter Shor's
        /// algorithm. Such a width ensures that the probability of getting the right result will be enough high.
        /// </para>
        /// </remarks>
        /// <param name="comp">The QuantumComputer instance.</param>
        /// <param name="x1">Output register. Its initial value must equal 1. Its width must equal <b>WIDTH + 1</b> (See Remarks).</param>
        /// <param name="x">Register for x. Its initial value could be any integer or a superposition of multiple integers. Its width must equal <b>2 * WIDTH</b> (See Remarks).</param>
        /// <param name="valueA">Integer value of a. (For computing (a^x) modulo N).</param>
        /// <param name="valueN">Integer value of N. (For computing (a^x) modulo N).</param>
        public static void ExpModulo(
            this QuantumComputer comp,
            Register x,
            Register x1,
            int valueA,
            int valueN)
        {
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

        /// <summary>
        /// Computes the modular multiplicative inverse a modulo N, for given integers a and N.
        /// <para>
        /// The multiplicative inverse of a modulo N exists if and only if a and N are coprime (i.e., if gcd(a, N) = 1).
        /// </para>
        /// </summary>
        /// <param name="a">Integer for which we wish to get a modular inversion.</param>
        /// <param name="N">The integer N.</param>
        /// <returns>Value x such that [(a * x) modulo N] equals 1.</returns>
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
