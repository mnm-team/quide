/*
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

namespace QuIDE.QuantumModel.Gates
{
    public abstract class CustomGate : Gate
    {
        public abstract string FunctionName { get; }

        public override RegisterRefModel? Control => null;

        protected RegisterPartModel incRegPart(RegisterPartModel regRef, RegisterModel register, int afterOffset,
            int delta = 1)
        {
            if (regRef.Register != register) return regRef;

            if (regRef.Width + delta == register.Qubits.Count)
            {
                return regRef with { Width = regRef.Width + delta };
            }

            if (regRef.Offset > afterOffset)
            {
                return regRef with { Offset = regRef.Offset + delta };
            }

            if (regRef.Offset + regRef.Width - 1 > afterOffset)
            {
                return regRef with { Width = regRef.Width + delta };
            }

            return regRef;
        }

        protected RegisterPartModel copyRegPart(RegisterPartModel regRef, int referenceBeginRow)
        {
            return new RegisterPartModel
            {
                Register = null,
                Offset = regRef.OffsetToRoot - referenceBeginRow,
                Width = regRef.Width
            };
        }
    }
}