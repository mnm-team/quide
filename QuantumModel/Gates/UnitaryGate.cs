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

using Quantum;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows;

namespace QuantumModel
{
    public class UnitaryGate : SingleGate
    {
        private Complex[,] _matrix;

        public UnitaryGate(Complex[,] matrix, RegisterRefModel target, RegisterRefModel? control = null)
            
            : base(target, control)
        {
            _matrix = matrix;
        }

        public override GateName Name
        {
            get { return GateName.Unitary; }
        }

        public override Gate Copy(int referenceBeginRow)
        {
            Tuple<RegisterRefModel, RegisterRefModel?> refs = CopyRefs(referenceBeginRow);
            return new UnitaryGate(_matrix, refs.Item1, refs.Item2);
        }

        public Complex[,] Matrix
        {
            get { return _matrix; }
        }
    }
}
