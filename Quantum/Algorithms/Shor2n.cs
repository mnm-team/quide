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
using System.Numerics;
using Quantum.Operations;

namespace Quantum.Algorithms
{
    public class Shor2n : Shor
    {
        private readonly int a;

        //Quantum
        private QuantumComputer comp;
        private Register ctrl;
        private ulong[] expTab;

        private int L;

        //Parameters
        private readonly int N;
        private Register reg0;
        private Register regC;
        private Register regX;
        private byte[] result;
        private int width;

        public Shor2n(int N, int a)
        {
            this.N = N;
            this.a = a;

            if (N < 15) throw new ArgumentException("Invalid number", "N");
        }

        public int FindPeriod()
        {
            Initialize();
            ClassicalPreprocess();
            QuantumComputation();
            var result = ClassicalPostprocess();
            Dispose();
            return result;
        }


        public void Initialize()
        {
            width = Utils.CalculateRegisterWidth((ulong)N);
            L = 2 * width;

            comp = QuantumComputer.GetInstance();

            //Register regTemp = comp.NewRegister(1, 2 * width + 3, (int)(Math.Pow(2, 2 * (width + 1))));
            var regTemp = comp.NewRegister(1, 2 * width + 3);

            regX = regTemp[0, width];
            reg0 = regTemp[width, width + 1];
            regC = regTemp[2 * width + 1, 1];
            ctrl = regTemp[2 * width + 2, 1];

            //this.regX = comp.NewRegister(1, width);
            //this.regC = comp.NewRegister(0, 1);

            //this.ctrl = comp.NewRegister(0, 1);
            //this.reg0 = comp.NewRegister(0, width + 1);
            //this.reg0 = comp.NewRegister(0, width);
            //this.reg0 = comp.NewRegister(0, width + 1, (int)(Math.Pow(2, 2 * (width + 1))));

            result = new byte[L];
            expTab = new ulong[L];
        }

        public void ClassicalPreprocess()
        {
            //Console.WriteLine("Before preprocess");
            for (var i = 0; i < L; i++) expTab[i] = (ulong)BigInteger.ModPow(a, BigInteger.Pow(2, i), N);
            //Console.WriteLine("After preprocess");
        }

        public void QuantumComputation()
        {
            for (var i = 0; i < L; i++)
            {
                //Console.WriteLine("Quantum {0} / {1}", i, L);
                comp.Hadamard(regC[0]);
                //Console.WriteLine("Before CTRL-Ua");
                comp.ControlledUaGate(expTab[L - 1 - i], (ulong)N, ctrl, regX, reg0, regC[0]);
                //Console.WriteLine("After CTRL-Ua");
                if (i > 0)
                {
                    var gamma = 0.0;

                    for (var k = 0; k < i; k++)
                        if (result[k] > 0)
                            gamma += 1 / (double)(1 << (i - k));

                    gamma *= -1 * Math.PI;

                    comp.PhaseKick(gamma, regC[0]);
                }

                comp.Hadamard(regC[0]);

                result[i] = regC.Measure(0);

                if (result[i] != 0) comp.SigmaX(regC[0]);
            }
        }

        public int ClassicalPostprocess()
        {
            var Q = 1 << (2 * width);
            var inputMeasured = 0;

            for (var i = 0; i < 2 * width; i++) inputMeasured += (1 << i) * result[i];

            /*int reversed = Utils.getReverseBits(inputMeasured, 2 * width);
            Console.WriteLine("rev = {0}, int = {1}", reversed, inputMeasured);
            inputMeasured = reversed;*/

            var finalResult = Utils.FractionalApproximation(inputMeasured, Q, 2 * width);
            Console.WriteLine("Fractional approximation:  {0} / {1}, y = {2}, width = {3}", finalResult.Item1,
                finalResult.Item2, inputMeasured, width);
            //if (finalResult.Item2 % 2 == 1) // odd denominator
            //{
            //    // try multiplication by 2
            //    return finalResult.Item2 * 2;
            //}
            return finalResult.Item2;
        }

        public void Dispose()
        {
            comp.DeleteRegister(ref reg0);
            comp.DeleteRegister(ref regX);
            comp.DeleteRegister(ref regC);
            comp.DeleteRegister(ref ctrl);
            comp = null;
        }

        //static public int FindPeriod(int N, int a)
        //{

        //    if (N < 15)
        //    {
        //        throw new System.ArgumentException("Invalid number", "N");
        //    }

        //    int width = Utils.CalculateRegisterWidth((ulong)N);

        //    //Console.WriteLine("Width for N: {0}", width);
        //    //Console.WriteLine("Total register width (2 * w + 3) : {0}", 2 * width + 3);

        //    //init
        //    QuantumComputer comp = QuantumComputer.GetInstance();

        //    //input register
        //    Register regX = comp.NewRegister(1, width);

        //    // output register (must contain 1):
        //    Register regC = comp.NewRegister(0, 1);

        //    Register ctrl = comp.NewRegister(0, 1);
        //    Register reg0 = comp.NewRegister(0, width);

        //    byte[] result = new byte[width];

        //    for (int i = 0; i < width; i++)
        //    {
        //        comp.Hadamard(regC[0]);

        //        int j = width - i - 1;


        //        ulong exp = (ulong)BigInteger.ModPow(a, 1 << j, N);

        //        Console.WriteLine("a = {0}, exp = {1}, j = {2}, 1 << j = {3}", a, exp, j, (ulong)1 << j);

        //        comp.ControlledUaGate(exp, (ulong)N, ctrl, regX, reg0, regC[0]);

        //        if (i > 0)
        //        {
        //            double gamma = 0.0;

        //            for (int k = 2; k <= i+1; k++) {
        //                gamma += result[i+1-k]/(1 << k);
        //                //Console.WriteLine("k = {0}", k);
        //            }

        //            gamma *= (-2) * Math.PI;

        //            comp.PhaseKick(gamma, regC[0]);
        //        }

        //        comp.Hadamard(regC[0]);

        //        result[i] = regC.Measure(0);

        //        if (result[i] != 0) {
        //            comp.SigmaX(regC[0]);
        //        }

        //    }


        //    // getting the input register
        //    int Q = (int)(1 << width);
        //    int inputMeasured = 0;

        //    for (int i = 0; i < width; i++) {
        //        inputMeasured += (1 << i) * result[i];
        //    }

        //    Console.WriteLine("Input measured = {0}", inputMeasured);
        //    Console.WriteLine("Q = {0}", Q);

        //    Tuple<int, int> finalResult = Utils.FractionalApproximation(inputMeasured, Q, width);

        //    Console.WriteLine("Fractional approximation:  {0} / {1}", finalResult.Item1, finalResult.Item2);

        //    if (finalResult.Item2 % 2 == 1) // odd denominator
        //    {
        //        // try multiplication by 2
        //        return finalResult.Item2 * 2;
        //    }
        //    return finalResult.Item2;
        //}
    }
}