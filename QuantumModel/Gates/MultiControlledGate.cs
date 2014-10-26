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

using Quantum;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace QuantumModel
{
    public abstract class MultiControlledGate : Gate
    {
        private RegisterRefModel[] _controls;
        private RegisterRefModel _target;

        public MultiControlledGate(RegisterRefModel target, params RegisterRefModel[] controls)
        {
            _controls = controls;
            _target = target;
        }

        public MultiControlledGate(RegisterRefModel target, RegisterRefModel control, params RegisterRefModel[] restControls)
        {
            _controls = new RegisterRefModel[restControls.Length + 1];
            _controls[0] = control;
            Array.Copy(restControls, 0, _controls, 1, restControls.Length);
            _target = target;
        }

        public override int Begin
        {
            get
            {
                if (_controls.Length > 0)
                {
                    int minControls = _controls.Min<RegisterRefModel>(x => x.OffsetToRoot);
                    return Math.Min(minControls, _target.OffsetToRoot);
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
                if (_controls.Length > 0)
                {
                    int maxControls = _controls.Max<RegisterRefModel>(x => x.OffsetToRoot);
                    return Math.Max(maxControls, _target.OffsetToRoot);
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
            get { return null; }
        }

        public override void IncrementRow(RegisterModel register, int afterOffset, int delta = 1)
        {
            if (_target.Register == register && _target.Offset > afterOffset)
            {
                _target.Offset += delta;
            }
            for (int i = 0; i < _controls.Length; i++)
            {
                if (_controls[i].Register == register && _controls[i].Offset > afterOffset)
                {
                    _controls[i].Offset += delta;
                }
            }
        }

        public RegisterRefModel[] Controls
        {
            get { return _controls; }
        }

        protected Tuple<RegisterRefModel, RegisterRefModel[]> CopyRefs(int referenceBeginRow)
        {
            RegisterRefModel targetRef = new RegisterRefModel()
            {
                Register = null,
                Offset = _target.OffsetToRoot - referenceBeginRow
            };
            RegisterRefModel[] controls = _controls.Select<RegisterRefModel, RegisterRefModel>(x =>
                new RegisterRefModel() { Register = null, Offset = x.OffsetToRoot - referenceBeginRow })
                .ToArray<RegisterRefModel>();

            return new Tuple<RegisterRefModel, RegisterRefModel[]>(targetRef, controls);
        }
    }
}
