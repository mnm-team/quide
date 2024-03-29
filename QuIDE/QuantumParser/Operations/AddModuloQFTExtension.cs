﻿/*
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

#region

using System;
using Quantum;

#endregion

namespace QuIDE.QuantumParser.Operations
{
    public static class AddModuloQFTExtension
    {
        public static void AddModuloQFTPhi(this QuantumComputer comp, ulong a, ulong N, RegisterRef ctrl, Register b,
            params RegisterRef[] controls)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a, N, ctrl, b, controls };
                comp.AddParametricGate("AddModuloQFTPhi", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            comp.AddQFTPhi(a, b, controls);
            comp.InverseAddQFTPhi(N, b);

            comp.InverseQFT(b);
            comp.CNot(ctrl, b[b.Width - 1]);
            comp.QFT(b);

            comp.AddQFTPhi(N, b, ctrl);
            comp.InverseAddQFTPhi(a, b, controls);

            comp.InverseQFT(b);
            comp.SigmaX(b[b.Width - 1]);
            comp.CNot(ctrl, b[b.Width - 1]);
            comp.SigmaX(b[b.Width - 1]);
            comp.QFT(b);

            comp.AddQFTPhi(a, b, controls);
        }

        public static void InverseAddModuloQFTPhi(this QuantumComputer comp, ulong a, ulong N, RegisterRef ctrl,
            Register b, params RegisterRef[] controls)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a, N, ctrl, b, controls };
                comp.AddParametricGate("InverseAddModuloQFTPhi", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            comp.InverseAddQFTPhi(a, b, controls);

            comp.InverseQFT(b);
            comp.SigmaX(b[b.Width - 1]);
            comp.CNot(ctrl, b[b.Width - 1]);
            comp.SigmaX(b[b.Width - 1]);
            comp.QFT(b);

            comp.AddQFTPhi(a, b, controls);
            comp.InverseAddQFTPhi(N, b, ctrl);

            comp.InverseQFT(b);
            comp.CNot(ctrl, b[b.Width - 1]);
            comp.QFT(b);

            comp.AddQFTPhi(N, b);
            comp.InverseAddQFTPhi(a, b, controls);
        }

        public static void AddModuloQFT(this QuantumComputer comp, ulong a, ulong N, Register b,
            params RegisterRef[] controls)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a, N, b, controls };
                comp.AddParametricGate("AddModuloQFT", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            Register ctrl = comp.NewRegister(0, 1);
            comp.AddModuloQFT(a, N, ctrl, b, controls);
        }

        public static void InverseAddModuloQFT(this QuantumComputer comp, ulong a, ulong N, Register b,
            params RegisterRef[] controls)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a, N, b, controls };
                comp.AddParametricGate("InverseAddModuloQFT", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            Register ctrl = comp.NewRegister(0, 1);
            comp.InverseAddModuloQFT(a, N, ctrl, b, controls);
        }

        public static void AddModuloQFT(this QuantumComputer comp, ulong a, ulong N, RegisterRef ctrl, Register b,
            params RegisterRef[] controls)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a, N, ctrl, b, controls };
                comp.AddParametricGate("AddModuloQFT", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            Validate(a, b, N);
            comp.QFT(b);
            comp.AddModuloQFTPhi(a, N, ctrl, b, controls);
            comp.InverseQFT(b);
        }

        public static void InverseAddModuloQFT(this QuantumComputer comp, ulong a, ulong N, RegisterRef ctrl,
            Register b, params RegisterRef[] controls)
        {
            if (comp.Group)
            {
                object[] parameters = new object[] { comp, a, N, ctrl, b, controls };
                comp.AddParametricGate("InverseAddModuloQFT", parameters);
                return;
            }
            else
            {
                comp.Group = true;
            }

            Validate(a, b, N);
            comp.QFT(b);
            comp.InverseAddModuloQFTPhi(a, N, ctrl, b, controls);
            comp.InverseQFT(b);
        }

        private static void Validate(ulong a, Register b, ulong N)
        {
            if (b.Width != Utils.CalculateRegisterWidth(N) + 1)
            {
                throw new ArgumentException("Register b must be able to contain N + 1 bit");
            }
        }
    }
}