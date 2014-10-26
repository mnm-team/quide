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
    public class EmptyGate : Gate
    {
        private RegisterRefModel _row;

        public EmptyGate(RegisterRefModel row)
        {
            _row = row;
        }

        public override GateName Name
        {
            get { return GateName.Empty; }
        }

        public override int Begin
        {
            get { return _row.OffsetToRoot; }
        }

        public override int End
        {
            get { return _row.OffsetToRoot; }
        }

        public override RegisterRefModel Target
        {
            get { return _row; }
        }

        public override RegisterRefModel? Control
        {
            get { return null; }
        }

        public override void IncrementRow(RegisterModel register, int afterOffset, int delta = 1)
        {
            if (_row.Register == register && _row.Offset > afterOffset)
            {
                _row.Offset += delta;
            }
        }

        public override Gate Copy(int referenceBeginRow)
        {
            RegisterRefModel targetRef = new RegisterRefModel()
            {
                Register = null,
                Offset = _row.OffsetToRoot - referenceBeginRow
            };
            return new EmptyGate(targetRef);
        }
    }
}
