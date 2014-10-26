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

using Quantum.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Quantum
{
    public static class Utils
    {
        static public int CalculateRegisterWidth(ulong number)
        {
            if (number == 0)
            {
                return 1;
            }

            int i = 0;
            while (number > 0)
            {
                number >>= 1;
                i++;
            }
            return i;
        }

        static public Tuple<int, int> FractionalApproximation(int a, int b, int width)
        {
            double f = (double)a / (double)b;
            double g = f;
            int i, num2 = 0, den2 = 1, num1 = 1, den1 = 0, num = 0, den = 0;
            int max = 1 << width;

            do
            {
                i = (int)g;  // integer part
                g = 1.0 / (g - i);  // reciprocal of the fractional part

                if (i * den1 + den2 > max) // if denominator is too big
                {
                    break;
                }

                // new numerator and denominator
                num = i * num1 + num2;
                den = i * den1 + den2;

                // previous nominators and denominators are memorized
                num2 = num1;
                den2 = den1;
                num1 = num;
                den1 = den;

            }
            while (Math.Abs(((double)num / (double)den) - f) > 1.0 / (2 * max));
            // this condition is from Shor algorithm

            return new Tuple<int, int>(num, den);
        }

        public static string Print(this Complex[] vector)
        {
            IFormatProvider formatter = new ComplexFormatter();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < vector.Length; i++)
            {
                sb.AppendLine(String.Format(formatter, "[ {0:I5} ]", vector[i]));
            }
            return sb.ToString();
        }

        public static bool[] getBinaryRepresentation(ulong i, int width)
        {
            bool[] bits = new bool[width];
            ulong k = i;
            for (int j = 0; j < width; j++)
            {
                if (k % 2 == 1)
                {
                    bits[j] = true;
                }
                else
                {
                    bits[j] = false;
                }
                k /= 2;
            }
            return bits;
        }

        public static int getReverseBits(int number, int width)
        {
            bool[] bits = getBinaryRepresentation((ulong)number, width);

            int exp = 1;
            int revNum = 0;

            for (int i = width - 1; i >= 0; i--)
            {
                if (bits[i])
                {
                    revNum += exp;
                }
                exp *= 2;
            }
            return revNum;
        }

        public static int? InversionModulo(int a, int N)
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
                return null;
            }
            if (x < 0)
            {
                x = x + N; // ujemne x sprowadzamy do wartości dodatnich
            }
            return x; // x jest poszukiwaną odwrotnością modulo
        }

        public static ulong gcd(ulong a, ulong b)
        {
            ulong c = a;
            ulong d = b;
            ulong t = 0;

            while (d != 0)
            {
                t = d;
                d = c % d;
                c = t;
            }
            return c;
        }

        public static int calculatePeriod(int a, int N)
        {
            int i = 1;
            while (true)
            {
                if (BigInteger.ModPow(a, i, N) == 1)
                {
                    break;
                }
                i++;
            }
            return i;
        }

        //HACK funkcja tablicująca wartości dla phasekicka
    }
}
