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

using System.Collections.Generic;
using System.Numerics;

#endregion

namespace QuIDE.QuantumParser
{
    interface IQuantumComputer
    {
        void DeleteRegister(ref Register register);
        Register GetRootRegister(params RegisterRef[] refs);
        Register TensorProduct(Register r1, Register r2);
        Register NewRegister(IDictionary<ulong, Complex> initStates, int width);
        Register NewRegister(ulong initval, int width, int? size = null);
    }
}