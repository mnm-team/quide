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
    public static class AddQFTExtension
    {
        public static void AddQFTPhi(this QuantumComputer comp, Register a, Register b, params RegisterRef[] controls)
        {
            Validate(a, b);

            for (int j = b.Width - 1; j >= 0; j--)
            {
                for (int i = j; i >= 0; i--)
                {
                    List<RegisterRef> list = controls.ToList<RegisterRef>();
                    list.Add(a[i]);
                    RegisterRef[] controls2 = list.ToArray();
                    comp.CPhaseShift(Math.Abs(j - i), b[j], controls2);
                }
            }

        }

        public static void InverseAddQFTPhi(this QuantumComputer comp, Register a, Register b, params RegisterRef[] controls)
        {
            Validate(a, b);

            for (int j = 0; j < b.Width; j++)
            {
                for (int i = 0; i <= j; i++)
                {
                    List<RegisterRef> list = controls.ToList<RegisterRef>();
                    list.Add(a[i]);
                    RegisterRef[] controls2 = list.ToArray();
                    comp.InverseCPhaseShift(Math.Abs(j - i), b[j], controls2);
                }
            }
        }

        public static void AddQFT(this QuantumComputer comp, Register a, Register b, params RegisterRef[] controls)
        {
            Validate(a, b);

            comp.QFT(b);
            comp.AddQFTPhi(a, b, controls);
            comp.InverseQFT(b);
        }

        public static void InverseAddQFT(this QuantumComputer comp, Register a, Register b, params RegisterRef[] controls)
        {
            Validate(a, b);
            comp.QFT(b);
            comp.InverseAddQFTPhi(a, b, controls);
            comp.InverseQFT(b);
        }

        public static void AddQFTPhi(this QuantumComputer comp, ulong a, Register b, params RegisterRef[] controls)
        {
            bool[] aBin = Utils.getBinaryRepresentation(a, b.Width);

            for (int i = b.Width - 1; i >= 0; i--)
            {
                //comp.ClassicalCPhaseShift(b[j], aBin[j], b.Width - j, controls);
                double exp = 0.0;

                for (int j = i; j >= 0; j--)
                {
                    //Console.WriteLine("InverseAdd i = {2}, N = {1}, a = {0}", a, j, i);
                    if (aBin[j])
                    {
                        //comp.CPhaseShift(Math.Abs(i - j), b[i], controls);
                        exp += (double)1 / ((double)(1 << (i - j)));
                    }
                }
                exp *= Math.PI;
                comp.PhaseKick(exp, b[i], controls);
            }

        }

        public static void InverseAddQFTPhi(this QuantumComputer comp, ulong a, Register b, params RegisterRef[] controls)
        {
            bool[] aBin = Utils.getBinaryRepresentation(a, b.Width);
            //for (int j = 0; j < b.Width; j++)
            //{
            //    for (int i = 0; i <= j; i++)
            //    {
            //        //Console.WriteLine("InverseAdd i = {2}, N = {1}, a = {0}", a, j, i);
            //        if (aBin[i])
            //        {
            //            //HACK przerobić na PhaseKick (odwrotny argument!!!)
            //            comp.InverseCPhaseShift(Math.Abs(j - i), b[j], controls);
            //        }
            //    }
            //}
            for (int i = 0; i < b.Width; i++)
            {
                double exp = 0.0;

                for (int j = i; j >= 0; j--)
                {
                    if (aBin[j])
                    {
                        exp += (double)1 / ((double)(1 << (i - j)));
                    }
                }
                exp *= ((double)-1) * Math.PI;
                comp.PhaseKick(exp, b[i], controls);
            }
        }

        public static void AddQFT(this QuantumComputer comp, ulong a, Register b, params RegisterRef[] controls)
        {
            Validate(a, b);
            comp.QFT(b);
            comp.AddQFTPhi(a, b, controls);
            comp.InverseQFT(b);
        }

        public static void InverseAddQFT(this QuantumComputer comp, ulong a, Register b, params RegisterRef[] controls)
        {
            Validate(a, b);
            comp.QFT(b);
            comp.InverseAddQFTPhi(a, b, controls);
            comp.InverseQFT(b);
        }

        private static void Validate(Register a, Register b)
        {
            if (b.Width != a.Width)
            {
                throw new System.ArgumentException("Register b must be exactly the same size as register a.");
            }
        }
        private static void Validate(ulong a, Register b)
        {

            if (b.Width < Utils.CalculateRegisterWidth(a) + 1)
            {
                //throw new System.ArgumentException("Register b must be larger or the same size as register a.");
            }
        }
    }

}
