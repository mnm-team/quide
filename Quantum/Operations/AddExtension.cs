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
    /// <para>
    /// Extension methods for performing simple, classically-inspired addition.
    /// </para>
    /// <para>
    /// To add two registers, call <see cref="Add(QuantumComputer, Register, Register)"/>. To inverse the operation (perform a subtraction), 
    /// call <see cref="InverseAdd(QuantumComputer, Register, Register)"/>. The rest of methods here are helpers used for performing these two.
    /// Nevertheless, each can be called independently.
    /// </para>
    /// </summary>
    public static class AddExtension
    {
        /// <summary>
        /// Sets the carry bit (in point of fact, a carry qubit). 
        /// Used with <see cref="Sum(QuantumComputer, RegisterRef, RegisterRef, RegisterRef)"/> to perform addition.
        /// </summary>
        /// <param name="comp">The QuantumComputer instance.</param>
        /// <param name="rc0">The reference to previous carry qubit.</param>
        /// <param name="ra0">The reference to qubit, for which the carry bit is computed, in the first register.</param>
        /// <param name="rb0">The reference to qubit, for which the carry bit is computed, in the second register.</param>
        /// <param name="rc1">The reference to qubit for storing the resulted carry value.</param>
        public static void Carry(this QuantumComputer comp,
            RegisterRef rc0, RegisterRef ra0, RegisterRef rb0, RegisterRef rc1)
        {
            Register root = comp.GetRootRegister(rc0, ra0, rb0, rc1);
            int c0 = rc0.OffsetToRoot;
            int a0 = ra0.OffsetToRoot;
            int b0 = rb0.OffsetToRoot;
            int c1 = rc1.OffsetToRoot;

            root.Toffoli(c1, a0, b0);
            root.CNot(b0, a0);
            root.Toffoli(c1, c0, b0);
        }

        /// <summary>
        /// Inversion of <see cref="Carry(QuantumComputer, RegisterRef, RegisterRef, RegisterRef, RegisterRef)"/> method.
        /// </summary>
        /// <param name="comp">The QuantumComputer instance.</param>
        /// <param name="rc0">The reference to previous carry qubit.</param>
        /// <param name="ra0">The reference to qubit, for which the carry bit was computed, in the first register.</param>
        /// <param name="rb0">The reference to qubit, for which the carry bit was computed, in the second register.</param>
        /// <param name="rc1">The reference to qubit storing the resulted carry value.</param>
        public static void InverseCarry(this QuantumComputer comp,
            RegisterRef rc0, RegisterRef ra0, RegisterRef rb0, RegisterRef rc1)
        {
            Register root = comp.GetRootRegister(rc0, ra0, rb0, rc1);
            int c0 = rc0.OffsetToRoot;
            int a0 = ra0.OffsetToRoot;
            int b0 = rb0.OffsetToRoot;
            int c1 = rc1.OffsetToRoot;
            root.Toffoli(c1, c0, b0);
            root.CNot(b0, a0);
            root.Toffoli(c1, a0, b0);
        }

        /// <summary>
        /// Computes the least significant bit of sum of two given bits. 
        /// Used with <see cref="Carry(QuantumComputer, RegisterRef, RegisterRef, RegisterRef, RegisterRef)"/> to perform addition.
        /// </summary>
        /// <param name="comp">The QuantumComputer instance.</param>
        /// <param name="refA">The reference to first qubit being sumed.</param>
        /// <param name="refB">The reference to second qubit being sumed.</param>
        /// <param name="refTarget">The reference to qubit for storing the result.</param>
        public static void Sum(this QuantumComputer comp,
            RegisterRef refA, RegisterRef refB, RegisterRef refTarget)
        {
            Register root = comp.GetRootRegister(refA, refB, refTarget);
            int a = refA.OffsetToRoot;
            int b = refB.OffsetToRoot;
            int target = refTarget.OffsetToRoot;
            root.CNot(target, a);
            root.CNot(target, b);
        }

        /// <summary>
        /// <para>
        /// Adds two registers. The result is stored in the second. An extra register is needed for storing carry bits.
        /// </para>
        /// <para>
        /// Add(a, b, 0) -> (a, a+b, 0)
        /// </para>
        /// <para>
        /// In order to improve performance, this method do not check if arguments are valid.
        /// They must satisfy following conditions:
        /// <list type="bullet">
        /// <item>Registers a, b and c must not overlap</item>
        /// <item>Registers a and c must have the same width</item>
        /// <item>Register b must be exactly one bit wider than register a (or c)</item>
        /// <item>Initial value of c must be 0</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="comp">The QuantumComputer instance.</param>
        /// <param name="a">The first register to sum. Its value remains unchanged.</param>
        /// <param name="b">The second register to sum. After performing this operation, it contains the sum result.</param>
        /// <param name="c">The extra register for storing carry bits.</param>
        public static void Add(this QuantumComputer comp, 
            Register a, Register b, Register c)
        {
            int width = a.Width;
            int i = 0;
            for (; i < width - 1; i++)
            {
                comp.Carry(c[i], a[i], b[i], c[i + 1]);
            }
            comp.Carry(c[i], a[i], b[i], b[i + 1]);

            comp.CNot(b[i], a[i]);
            comp.Sum(c[i], a[i], b[i]);
            i--;
            for (; i >= 0; i--)
            {
                comp.InverseCarry(c[i], a[i], b[i], c[i + 1]);
                comp.Sum(c[i], a[i], b[i]);
            }
        }

        /// <summary>
        /// <para>
        /// Adds two registers. The result is stored in the second. No extra registers are required.
        /// </para>
        /// <para>
        /// Add(a, b, 0) -> (a, a+b, 0)
        /// </para>
        /// <para>
        /// This method checks if arguments are valid. If not, an exception is thrown.
        /// The following conditions must be satisfied:
        /// <list type="bullet">
        /// <item>Registers a and b must not overlap</item>
        /// <item>Register b must be exactly one bit wider than register a</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="comp">The QuantumComputer instance.</param>
        /// <param name="a">The first register to sum. Its value remains unchanged.</param>
        /// <param name="b">The second register to sum. After performing this operation, it contains the sum result.</param>
        public static void Add(this QuantumComputer comp, Register a, Register b)
        {
            Validate(a, b);

            Register c = comp.NewRegister(0, a.Width);           
            comp.Add(a, b, c);
            comp.DeleteRegister(ref c);
        }

        /// <summary>
        /// <para>
        /// Performs an exact inversion of <see cref="Add(QuantumComputer, Register, Register, Register)"/> method.
        /// </para>
        /// <para>
        /// InverseAdd(a, a+b, 0) -> (a, b, 0)
        /// </para>
        /// <para>
        /// In order to improve performance, this method do not check if arguments are valid.
        /// They must satisfy following conditions:
        /// <list type="bullet">
        /// <item>Registers a, b and c must not overlap</item>
        /// <item>Registers a and c must have the same width</item>
        /// <item>Register b must be exactly one bit wider than register a (or c)</item>
        /// <item>Initial value of c must be 0</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="comp">The QuantumComputer instance.</param>
        /// <param name="a">The first register used to inverse the sum. Its value remains unchanged.</param>
        /// <param name="b">The second register used to inverse the sum. After performing this operation, it contains the inversion result.</param>
        /// <param name="c">The extra register for storing carry bits.</param>
        public static void InverseAdd(this QuantumComputer comp,
            Register a, Register b, Register c)
        {
            int width = a.Width;
            int i = 0;
            for (; i < width - 1; i++)
            {
                comp.Sum(c[i], a[i], b[i]);
                comp.Carry(c[i], a[i], b[i], c[i + 1]);
            }
            comp.Sum(c[i], a[i], b[i]);
            comp.CNot(b[i], a[i]);
            comp.InverseCarry(c[i], a[i], b[i], b[i + 1]);           
            i--;
            for (; i >= 0; i--)
            {
                comp.InverseCarry(c[i], a[i], b[i], c[i + 1]);
            }
        }

        /// <summary>
        /// <para>
        /// Performs an exact inversion of <see cref="Add(QuantumComputer, Register, Register)"/> method.
        /// </para>
        /// <para>
        /// InverseAdd(a, a+b, 0) -> (a, b, 0)
        /// </para>
        /// <para>
        /// This method checks if arguments are valid. If not, an exception is thrown.
        /// The following conditions must be satisfied:
        /// <list type="bullet">
        /// <item>Registers a and b must not overlap</item>
        /// <item>Register b must be exactly one bit wider than register a</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="comp">The QuantumComputer instance.</param>
        /// <param name="a">The first register used to inverse the sum. Its value remains unchanged.</param>
        /// <param name="b">The second register used to inverse the sum. After performing this operation, it contains the inversion result.</param>
        public static void InverseAdd(this QuantumComputer comp, Register a, Register b)
        {
            Validate(a, b);

            Register c = comp.NewRegister(0, a.Width);
            comp.InverseAdd(a, b, c);
            comp.DeleteRegister(ref c);
        }

        private static void Validate(Register a, Register b) 
        {
            if (b.Width != a.Width + 1)
            {
                throw new System.ArgumentException("Register b must be exactly one qubit wider than register a.");
            }
        }
    }
}
