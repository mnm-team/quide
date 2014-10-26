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

using Quantum.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Quantum.Algorithms
{
    public class Shor7n : Shor
    {
        //Parameters
        private int N = 0;
        private int a;
        private int width;
        private int inputMeasured;
        //private byte[] result;
        //private ulong[] expTab;
        private double kMax;

        //Quantum
        private QuantumComputer comp = null;
        private Register regX = null;
        private Register regX1 = null;
        Register rega;
        Register regb;
        Register regc;
        Register regN;

        public Shor7n(int N, int a)
        {
            this.N = N;
            this.a = a;

            if (N < 15)
            {
                throw new System.ArgumentException("Invalid number", "N");
            }
        }

        public int FindPeriod()
        {
            this.Initialize();
            this.ClassicalPreprocess();
            this.QuantumComputation();
            int result = this.ClassicalPostprocess();
            this.Dispose();
            return result;
        }



        public void Initialize()
        {
            this.width = Utils.CalculateRegisterWidth((ulong)N);
            this.kMax = Math.Log(2 * width, 2);

            this.comp = QuantumComputer.GetInstance();

            regX = comp.NewRegister(0, 2 * width/*, (int)(Math.Pow(2, 2 * width))*/);
            regX1 = comp.NewRegister(1, width + 1);
            rega = comp.NewRegister(0, regX1.Width - 1);
            regb = comp.NewRegister(0, regX1.Width);
            regc = comp.NewRegister(0, regX1.Width);
            regN = comp.NewRegister((ulong)N, regX1.Width - 1);

            

            //result = new byte[width];
            //expTab = new ulong[width];
        }

        public void ClassicalPreprocess()
        {

        }

        public void QuantumComputation()
        {
            comp.Walsh(regX);

            // perform exp_mod_N
            //comp.ExpModulo(regX, regX1, a, N);
            comp.ExpModulo(rega, regb, regc, regN, regX1, regX, a, N);

            // output register is no longer needed
            // we can measure it or delete, to save memory
            //regX1.Measure();

            // perform Quantum Fourier Transform on the input register
            comp.QFT(regX);
            //comp.AQFT(regX, kMax);

            // getting the input register
            inputMeasured = (int)regX.Measure();
            int reversed = Utils.getReverseBits(inputMeasured, 2 * width);
            Console.WriteLine("rev = {0}, int = {1}", reversed, inputMeasured);
            inputMeasured = reversed;
        }

        public int ClassicalPostprocess()
        {
            int Q = (int)(1 << 2*width);

            Tuple<int, int> result = Utils.FractionalApproximation(inputMeasured, Q, 2*width);
            Console.WriteLine("Fractional approximation:  {0} / {1}, y = {2}, width = {3}", result.Item1, result.Item2, inputMeasured, width);
            //if (result.Item2 % 2 == 1) // odd denominator
            //{
            //    // try multiplication by 2
            //    return result.Item2 * 2;
            //}
            return result.Item2;
        }

        public void Dispose()
        {
            comp.DeleteRegister(ref regX);
            comp.DeleteRegister(ref regX1);
            comp.DeleteRegister(ref rega);
            comp.DeleteRegister(ref regb);
            comp.DeleteRegister(ref regc);
            comp.DeleteRegister(ref regN);

            comp = null;
        }


        // Finds period of the function a^x mod N
        //static public int FindPeriod(int N, int a)
        //{

        //    if (N < 15)
        //    {
        //        throw new System.ArgumentException("Invalid number", "N");
        //    }
        //    ulong ulongN = (ulong)N;

        //    int width = Utils.CalculateRegisterWidth(ulongN);

        //    Console.WriteLine("Width for N: {0}", width);
        //    Console.WriteLine("Total register width (7 * w + 2) : {0}", 7 * width + 2);

        //    //init
        //    QuantumComputer comp = QuantumComputer.GetInstance();

        //    //input register
        //    Register regX = comp.NewRegister(0, 2 * width, (int)(Math.Pow(2, 2 * width)));

        //    // output register (must contain 1):
        //    Register regX1 = comp.NewRegister(1, width + 1);

        //    // perform Walsh-Hadamard transform on the input register
        //    // input register can contains N^2 so it is 2*width long
        //    Console.WriteLine("Applying Walsh-Hadamard transform on the input register...");
        //    comp.Walsh(regX);

        //    // perform exp_mod_N
        //    Console.WriteLine("Applying f(x) = a^x mod N ...");
        //    comp.ExpModulo(regX, regX1, a, N);

        //    // output register is no longer needed
        //    // we can measure it or delete, to save memory
        //    regX1.Measure();

        //    // perform Quantum Fourier Transform on the input register
        //    Console.WriteLine("Applying QFT on the input register...");
        //    comp.QFT(regX);

        //    // getting the input register
        //    int Q = (int)(1 << width);
        //    int inputMeasured = (int)regX.Measure();

        //    Console.WriteLine("Input measured = {0}", inputMeasured);
        //    Console.WriteLine("Q = {0}", Q);

        //    Tuple<int, int> result = Utils.FractionalApproximation(inputMeasured, Q, width);

        //    Console.WriteLine("Fractional approximation:  {0} / {1}", result.Item1, result.Item2);

        //    if (result.Item2 % 2 == 1) // odd denominator
        //    {
        //        // try multiplication by 2
        //        return result.Item2 * 2;
        //    }
        //    return result.Item2;
        //}
    }
}
