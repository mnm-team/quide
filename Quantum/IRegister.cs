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

using System.Collections.Generic;
using System.Numerics;

namespace Quantum
{
    internal interface IRegister
    {
        int OffsetToRoot { get; }
        RegisterRef this[int index] { get; }
        Register this[int offset, int width] { get; }
        int Width { get; }
        void CNot(int target, int control);
        void CPhaseShift(int dist, int target, params int[] controls);
        void Gate1(Complex[,] matrix, int target, int? control = null);
        ulong? GetValue();
        IReadOnlyDictionary<ulong, Complex> GetAmplitudes();
        IReadOnlyDictionary<ulong, double> GetProbabilities();
        Complex[] GetVector();
        void Hadamard(int target, int? control = null);
        void InverseCPhaseShift(int dist, int target, params int[] controls);
        void Reset(ulong newValue = 0);
        ulong Measure();
        byte Measure(int position);
        void PhaseKick(double gamma, int target, params int[] controls);
        void PhaseScale(double gamma, int target, int? control = null);
        void RotateX(double gamma, int target, int? control = null);
        void RotateY(double gamma, int target, int? control = null);
        void RotateZ(double gamma, int target, int? control = null);
        void SigmaX(int target, int? control = null);
        void SigmaY(int target, int? control = null);
        void SigmaZ(int target, int? control = null);
        void SqrtX(int target, int? control = null);
        void Toffoli(int target, params int[] controls);
        string ToString();
    }
}