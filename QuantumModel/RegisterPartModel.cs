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
    public struct RegisterPartModel : IEquatable<RegisterPartModel>
    {
        public RegisterModel Register
        {
            get;
            set;
        }

        public int Offset
        {
            get;
            set;
        }

        public int Width
        {
            get;
            set;
        }

        public int EndOffsetToRoot
        {
            get
            {
                if (Register != null)
                {
                    return Register.OffsetToRoot + Offset + Width - 1;
                }
                return Offset + Width - 1;
            }
        }

        public int OffsetToRoot
        {
            get
            {
                if (Register != null)
                {
                    return Register.OffsetToRoot + Offset;
                }
                return Offset;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is RegisterPartModel)
            {
                return this.Equals((RegisterPartModel)obj);
            }
            return false;
        }

        public bool Equals(RegisterPartModel obj)
        {
            return (OffsetToRoot == obj.OffsetToRoot &&
                Width == obj.Width);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
