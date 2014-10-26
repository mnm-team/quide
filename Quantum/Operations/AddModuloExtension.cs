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
    public static class AddModuloExtension
    {
        // register A - initially loaded with a
        // register B - initially loaded with b
        // register C - initially 0, exactly one bit wider than A, stores overflow bit
        // register N - initially loaded with N
        // after computation in register B: (a+b) mod N
        // other registers dont change their states
        // register B must be exactly one bit wider than A to store carry bit
        // register B must be exactly one bit wider than N to store carry bit
        // registers A, N must be the same length
        // Insecure version: registers widths etc. are not checked
        public static void AddModulo(
            this QuantumComputer comp,
            Register a,
            Register b,
            Register c,
            Register N,
            ulong valueN)
        {
            RegisterRef carry = b[b.Width - 1];
            RegisterRef overflow = c[c.Width - 1];

            comp.Add(a, b, c);
            comp.InverseAdd(N, b, c);
            comp.SigmaX(carry);
            comp.CNot(overflow, carry);
            comp.SigmaX(carry);

            //resetting N
            comp.LoadNumber(N, valueN, overflow);

            comp.Add(N, b, c);

            // now we have [(a+b) mod N] in B register
            // next steps lead to recover the initial state of registers N and overflow bit

            //setting N back
            comp.LoadNumber(N, valueN, overflow);

            comp.InverseAdd(a, b, c);
            comp.CNot(overflow, carry);
            comp.Add(a, b, c);
        }

        // register A - initially loaded with a
        // register B - initially loaded with b
        // after computation in register B: (a+b) mod N
        // using scratch registers inside
        // register B must be exactly one bit wider than A to store carry bit
        // Secure version: throws Exceptions if arguments are invalid
        public static void AddModulo(
            this QuantumComputer comp,
            Register a,
            Register b,
            ulong valueN)
        {
            Validate(a, b, valueN);

            Register c = comp.NewRegister(0, a.Width + 1);
            Register N = comp.NewRegister(valueN, a.Width);
            
            comp.AddModulo(a, b, c, N, valueN);
            
            comp.DeleteRegister(ref N);
            comp.DeleteRegister(ref c);
        }

        // Insecure version: registers widths etc. are not checked
        public static void InverseAddModulo(
            this QuantumComputer comp,
            Register a,
            Register b,
            Register c,
            Register N,
            ulong valueN)
        {
            RegisterRef carry = b[b.Width - 1];
            RegisterRef overflow = c[c.Width - 1];

            comp.InverseAdd(a, b, c);
            comp.CNot(overflow, carry);
            comp.Add(a, b, c);

            //resetting N:
            comp.LoadNumber(N, valueN, overflow);

            comp.InverseAdd(N, b, c);

            //setting N back:
            comp.LoadNumber(N, valueN, overflow);

            comp.SigmaX(carry);
            comp.CNot(overflow, carry);
            comp.SigmaX(carry);
            comp.Add(N, b, c);
            comp.InverseAdd(a, b, c);
        }

        //Inverse using scratch registers
        // Secure version: throws Exceptions if arguments are invalid
        public static void InverseAddModulo(
            this QuantumComputer comp,
            Register a,
            Register b,
            ulong valueN)
        {
            Validate(a, b, valueN);

            Register c = comp.NewRegister(0, a.Width + 1);
            Register N = comp.NewRegister(valueN, a.Width);

            comp.InverseAddModulo(a, b, c, N, valueN);

            comp.DeleteRegister(ref N);
            comp.DeleteRegister(ref c);
        }

        private static void Validate(
            Register a,
            Register b,
            ulong valueN)
        {
            if (b.Width != a.Width + 1)
            {
                throw new System.ArgumentException("Register b must be exactly one bit wider than register a to store carry bit.");
            }
            if ((valueN >> a.Width) > 0)
            {
                throw new System.ArgumentException("Register a is too small. It must have enough space to store N.");
            }
            foreach (var pair in a.GetProbabilities())
            {
                if (pair.Key >= valueN)
                {
                    throw new System.ArgumentException("There is a >= N.");
                }
            }
            foreach (var pair in b.GetProbabilities())
            {
                if (pair.Key >= valueN)
                {
                    throw new System.ArgumentException("There is b >= N.");
                }
            }
        }
    }
}
