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
    public class CompositeGate : CustomGate
    {
        private string _name;
        //private List<Gate> _gates;
        private RegisterPartModel _targetRegister;

        //public CompositeGate(string name, List<Gate> gates, RegisterPartModel target)
        //{
        //    _name = name;
        //    _gates = gates;
        //    _targetRegister = target;
        //}

        public CompositeGate(string name, RegisterPartModel target)
        {
            _name = name;
            _targetRegister = target;
        }

        public override GateName Name
        {
            get { return GateName.Composite; }
        }

        public override int Begin
        {
            get
            {
                return _targetRegister.OffsetToRoot;
            }
        }

        public override int End
        {
            get
            {
                return _targetRegister.EndOffsetToRoot;
            }
        }

        public override RegisterRefModel Target
        {
            get
            {
                return new RegisterRefModel()
                {
                    Register = _targetRegister.Register,
                    Offset = _targetRegister.Offset
                };
            }
        }

        public override void IncrementRow(RegisterModel register, int afterOffset, int delta = 1)
        {
            _targetRegister = incRegPart(_targetRegister, register, afterOffset, delta);
        }

        public override Gate Copy(int referenceBeginRow)
        {
            RegisterPartModel newTarget = copyRegPart(_targetRegister, referenceBeginRow);
            //return new CompositeGate(_name, _gates, newTarget);
            return new CompositeGate(_name, newTarget);
        }

        public override string FunctionName
        {
            get { return _name; }
        }

        //public List<Gate> Gates
        //{
        //    get { return _gates; }
        //}

        public RegisterPartModel TargetRegister
        {
            get { return _targetRegister; }
        }
    }
}
