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
    public abstract class SingleGate : Gate
    {
        private RegisterRefModel? _control;
        private RegisterRefModel _target;

        public SingleGate(RegisterRefModel target, RegisterRefModel? control = null)
        {
            _control = control;
            _target = target;
        }

        public override int Begin
        {
            get
            {
                if (_control.HasValue)
                {
                    return Math.Min(_control.Value.OffsetToRoot, _target.OffsetToRoot);
                }
                else
                {
                    return _target.OffsetToRoot;
                }
            }
        }

        public override int End
        {
            get
            {
                if (_control.HasValue)
                {
                    return Math.Max(_control.Value.OffsetToRoot, _target.OffsetToRoot);
                }
                else
                {
                    return _target.OffsetToRoot;
                }
            }
        }

        public override RegisterRefModel Target
        {
            get { return _target; }
        }

        public override RegisterRefModel? Control
        {
            get { return _control; }
        }

        public override void IncrementRow(RegisterModel register, int afterOffset, int delta = 1)
        {
            if (_target.Register == register && _target.Offset > afterOffset)
            {
                _target.Offset += delta;
            }
            if (_control.HasValue)
            {
                if (_control.Value.Register == register && _control.Value.Offset > afterOffset)
                {
                    _control = new RegisterRefModel() { Register = register, Offset = _control.Value.Offset + delta };
                }
            }
        }

        protected Tuple<RegisterRefModel, RegisterRefModel?> CopyRefs(int referenceBeginRow)
        {
            RegisterRefModel targetRef = new RegisterRefModel() { 
                Register = null, 
                Offset = _target.OffsetToRoot - referenceBeginRow 
            };
            RegisterRefModel? controlRef = null;
            if (_control.HasValue)
            {
                controlRef = new RegisterRefModel()
                {
                    Register = null,
                    Offset = _control.Value.OffsetToRoot - referenceBeginRow
                };
            }
            return new Tuple<RegisterRefModel, RegisterRefModel?>(targetRef, controlRef);
        }
    }
}
