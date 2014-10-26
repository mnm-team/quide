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
    public class MeasureGate : Gate
    {
        private RegisterRefModel _beginRow;
        private RegisterRefModel _endRow;

        public MeasureGate(RegisterRefModel beginRow, RegisterRefModel endRow)
        {
            if (beginRow.OffsetToRoot > endRow.OffsetToRoot)
            {
                _beginRow = endRow;
                _endRow = beginRow;
            }
            else
            {
                _beginRow = beginRow;
                _endRow = endRow;
            }           
        }

        public MeasureGate(RegisterRefModel row)
        {
            _beginRow = row;
            _endRow = row;
        }

        public override GateName Name
        {
            get { return GateName.Measure; }
        }

        public override int Begin
        {
            get { return _beginRow.OffsetToRoot; }
        }

        public override int End
        {
            get { return _endRow.OffsetToRoot; }
        }

        public override RegisterRefModel Target
        {
            get { return _endRow; }
        }

        public override RegisterRefModel? Control
        {
            get { return null; }
        }

        public override void IncrementRow(RegisterModel register, int afterOffset, int delta = 1)
        {
            if (_beginRow.Register == register && _beginRow.Offset > afterOffset)
            {
                _beginRow.Offset += delta;
            }
            if (_endRow.Register == register && _endRow.Offset > afterOffset)
            {
                _endRow.Offset += delta;
            }
        }

        public override Gate Copy(int referenceBeginRow)
        {
            RegisterRefModel beginRef = new RegisterRefModel()
            {
                Register = null,
                Offset = _beginRow.OffsetToRoot - referenceBeginRow
            };
            RegisterRefModel endRef = new RegisterRefModel()
            {
                Register = null,
                Offset = _endRow.OffsetToRoot - referenceBeginRow
            };
            return new MeasureGate(beginRef, endRef);
        }

        public RegisterRefModel BeginRow
        {
            get { return _beginRow; }
        }

        public RegisterRefModel EndRow
        {
            get { return _endRow; }
        }
    }
}
