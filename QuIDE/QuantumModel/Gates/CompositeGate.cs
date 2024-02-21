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
    public class CompositeGate : CustomGate
    {
        private readonly string _name;
        private RegisterPartModel _targetRegister;

        public CompositeGate(string name, RegisterPartModel target)
        {
            _name = name;
            _targetRegister = target;
        }

        public override GateName Name => GateName.Composite;

        public override int Begin => _targetRegister.OffsetToRoot;

        public override int End => _targetRegister.EndOffsetToRoot;

        public override RegisterRefModel Target =>
            new()
            {
                Register = _targetRegister.Register,
                Offset = _targetRegister.Offset
            };

        public override void IncrementRow(RegisterModel register, int afterOffset, int delta = 1)
        {
            _targetRegister = incRegPart(_targetRegister, register, afterOffset, delta);
        }

        public override Gate Copy(int referenceBeginRow)
        {
            RegisterPartModel newTarget = copyRegPart(_targetRegister, referenceBeginRow);
            return new CompositeGate(_name, newTarget);
        }

        public override string FunctionName => _name;

        public RegisterPartModel TargetRegister => _targetRegister;
    }
}