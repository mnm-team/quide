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

using QuantumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantumParser.Operations
{

    public static class AddExtension
    {
        public static void Carry(this QuantumComputer comp,
            RegisterRef rc0, RegisterRef ra0, RegisterRef rb0, RegisterRef rc1)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, rc0, ra0, rb0, rc1 };
                comp.AddParametricGate("Carry", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            Register root = comp.GetRootRegister(rc0, ra0, rb0, rc1);
            int c0 = rc0.OffsetToRoot;
            int a0 = ra0.OffsetToRoot;
            int b0 = rb0.OffsetToRoot;
            int c1 = rc1.OffsetToRoot;

            root.Toffoli(c1, a0, b0);
            root.CNot(b0, a0);
            root.Toffoli(c1, c0, b0);
        }

        public static void InverseCarry(this QuantumComputer comp,
            RegisterRef rc0, RegisterRef ra0, RegisterRef rb0, RegisterRef rc1)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, rc0, ra0, rb0, rc1 };
                comp.AddParametricGate("InverseCarry", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            Register root = comp.GetRootRegister(rc0, ra0, rb0, rc1);
            int c0 = rc0.OffsetToRoot;
            int a0 = ra0.OffsetToRoot;
            int b0 = rb0.OffsetToRoot;
            int c1 = rc1.OffsetToRoot;
            root.Toffoli(c1, c0, b0);
            root.CNot(b0, a0);
            root.Toffoli(c1, a0, b0);
        }

        public static void Sum(this QuantumComputer comp,
            RegisterRef refA, RegisterRef refB, RegisterRef refTarget)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, refA, refB, refTarget };
                comp.AddParametricGate("Sum", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            Register root = comp.GetRootRegister(refA, refB, refTarget);
            int a = refA.OffsetToRoot;
            int b = refB.OffsetToRoot;
            int target = refTarget.OffsetToRoot;
            root.CNot(target, a);
            root.CNot(target, b);
        }

        public static void InverseSum(this QuantumComputer comp,
            RegisterRef refA, RegisterRef refB, RegisterRef refTarget)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, refA, refB, refTarget };
                comp.AddParametricGate("Sum", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            comp.Sum(refA, refB, refTarget);
        }

        // Add(a, b, 0) -> (a, a+b, 0)
        // Registers a, b and c must not overlap
        // Registers a and b have the same width
        // Register c is used for storing carries and must be minimum one bit wider than register a (or b)
        // Initial value of c must be 0       
        public static void Add(this QuantumComputer comp,
            Register a, Register b, Register c)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a, b, c };
                comp.AddParametricGate("Add", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

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

        // Add(a, b) -> (a, a+b)
        // Registers a and b must not overlap
        // Registers a and b have the same width
        // Initially, there is scratch register used
        public static void Add(this QuantumComputer comp, Register a, Register b)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a, b };
                comp.AddParametricGate("Add", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            Validate(a, b);

            Register c = comp.NewRegister(0, a.Width);
            comp.Add(a, b, c);
            comp.DeleteRegister(ref c);
        }

        // InverseAdd(a, a+b, 0) -> (a, b, 0)
        public static void InverseAdd(this QuantumComputer comp,
            Register a, Register b, Register c)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a, b, c };
                comp.AddParametricGate("InverseAdd", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

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

        // InverseAdd(a, a+b) -> (a, b)
        // Using scratch register inside
        public static void InverseAdd(this QuantumComputer comp, Register a, Register b)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a, b };
                comp.AddParametricGate("InverseAdd", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

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
