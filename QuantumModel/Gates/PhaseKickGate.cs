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
using System.Text;
using System.Windows;

namespace QuantumModel
{
    public class PhaseKickGate : MultiControlledGate
    {
        private double _gamma;

        public PhaseKickGate(double gamma, RegisterRefModel target, params RegisterRefModel[] controls)
            : base(target, controls)
        {
            _gamma = gamma;
        }

        public override GateName Name
        {
            get { return GateName.PhaseKick; }
        }

        public double Gamma
        {
            get { return _gamma; }
        }

        public override Gate Copy(int referenceBeginRow)
        {
            Tuple<RegisterRefModel, RegisterRefModel[]> refs = CopyRefs(referenceBeginRow);
            return new PhaseKickGate(Gamma, refs.Item1, refs.Item2);
        }
    }
}
