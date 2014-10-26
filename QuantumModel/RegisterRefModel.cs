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
    public struct RegisterRefModel : IEquatable<RegisterRefModel>
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
            if (obj is RegisterRefModel)
            {
                return this.Equals((RegisterRefModel)obj);
            }
            return false;
        }

        public bool Equals(RegisterRefModel obj)
        {
            return (OffsetToRoot == obj.OffsetToRoot);
        }

        public static bool operator ==(RegisterRefModel r1, RegisterRefModel r2)
        {
            return r1.Equals(r2);
        }

        public static bool operator !=(RegisterRefModel r1, RegisterRefModel r2)
        {
            return !r1.Equals(r2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
