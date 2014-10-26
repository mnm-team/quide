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

using QuantumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantumParser
{
    public struct RegisterRef
    {
        //private Register _root;
        //private int _offsetToRoot;

        //public Register Root
        //{
        //    get { return _root; }
        //}

        //public int OffsetToRoot
        //{
        //    get { return _offsetToRoot; }
        //}

        //public Qubit(Register root, int offsetToRoot)
        //{
        //    this._root = root;
        //    this._offsetToRoot = offsetToRoot;
        //}

        public Register Register
        {
            get;
            set;
        }

        public int Offset
        {
            get;
            set;
        }

        //public int Width
        //{
        //    get;
        //    set;
        //}

        public int OffsetToRoot
        {
            get
            {
                return Register.OffsetToRoot + Offset;
            }
        }

        public RegisterRefModel ToRefModel()
        {
            return new RegisterRefModel()
            {
                Register = Register.Model,
                Offset = Offset + Register.OffsetToModel
            };
        }
    }
}
