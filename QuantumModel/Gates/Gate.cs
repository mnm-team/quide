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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace QuantumModel
{
    public enum GateName
    {
        Empty,
        Hadamard,
        SigmaX,
        SigmaY,
        SigmaZ,
        SqrtX,
        PhaseKick,
        PhaseScale,
        RotateX,
        RotateY,
        RotateZ,
        Unitary,
        CNot,
        Toffoli,
        InvCPhaseShift,
        CPhaseShift,
        Measure,
        Parametric,
        Composite
    }

    public abstract class Gate
    {
        public abstract GateName Name
        {
            get;
        }

        public abstract int Begin
        {
            get;
        }

        public abstract int End
        {
            get;
        }

        public abstract RegisterRefModel Target
        {
            get;
        }

        public abstract RegisterRefModel? Control
        {
            get;
        }

        public abstract void IncrementRow(RegisterModel register, int afterOffset, int delta = 1);

        public abstract Gate Copy(int referenceBeginRow);
    }
}
