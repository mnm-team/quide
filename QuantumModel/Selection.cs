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

namespace QuantumModel
{
    public struct Selection : IEquatable<Selection>
    {
        //private RegisterRefModel _beginRow;
        //private RegisterRefModel _endRow;

        private int _beginRow;
        private int _endRow;

        private int _beginColumn;
        private int _endColumn;

        //public RegisterRefModel BeginRow
        //{
        //    get { return _beginRow; }
        //}

        //public RegisterRefModel EndRow
        //{
        //    get { return _endRow; }
        //}

        public int BeginRow
        {
            get { return _beginRow; }
        }

        public int EndRow
        {
            get { return _endRow; }
        }

        public int BeginColumn
        {
            get { return _beginColumn; }
        }

        public int EndColumn
        {
            get { return _endColumn; }
        }

        public int RowSpan
        {
            get { return _endRow - _beginRow + 1; }
        }

        public int ColumnSpan
        {
            get { return _endColumn - _beginColumn + 1; }
        }

        public Selection(int beginRow, int endRow, int beginColumn, int endColumn)
        {
            if (beginColumn <= endColumn)
            {
                _beginColumn = beginColumn;
                _endColumn = endColumn;
            }
            else
            {
                _beginColumn = endColumn;
                _endColumn = beginColumn;
            }

            if (beginRow <= endRow)
            {
                _beginRow = beginRow;
                _endRow = endRow;
            }
            else
            {
                _beginRow = endRow;
                _endRow = beginRow;
            }
        }
        public override bool Equals(object obj)
        {
            if (obj is Selection)
            {
                return this.Equals((Selection)obj);
            }
            return false;
        }

        public bool Equals(Selection obj)
        {
            return (BeginRow == obj.BeginRow &&
                EndRow == obj.EndRow &&
                BeginColumn == obj.BeginColumn &&
                EndColumn == obj.EndColumn);
        }

        public static bool operator ==(Selection s1, Selection s2)
        {
            return s1.Equals(s2);
        }

        public static bool operator !=(Selection s1, Selection s2)
        {
            return !s1.Equals(s2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
