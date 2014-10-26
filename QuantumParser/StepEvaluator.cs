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
using Quantum.Operations;
using QuantumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuantumParser
{
    public class StepEvaluator
    {
        #region Fields

        private QuantumComputer _comp;

        #endregion // Fields


        #region Constructor

        internal StepEvaluator(QuantumComputer comp)
        {
            _comp = comp;
        }

        #endregion // Constructor


        #region Public Properties

        #endregion // Public Properties


        #region Public Methods

        public bool RunStep(IList<Gate> gates, bool runBackward = false, bool runComposite = false)
        {
            Quantum.Register root = _comp.GetSourceRoot();

            bool somethingChanged = false;
            int i = 0;
            int maxI = (runComposite ? 1 : gates.Count);
            while (i < maxI)
            {
                Gate gate = gates[i];
                int? control = null;
                if (gate.Control.HasValue)
                {
                    control = gate.Control.Value.OffsetToRoot;
                }
                switch (gate.Name)
                {
                    case GateName.Hadamard: // the same as inversion
                        root.Hadamard(gate.Target.OffsetToRoot, control);
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.SigmaX: // the same as inversion
                        root.SigmaX(gate.Target.OffsetToRoot);
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.SigmaY: // the same as inversion
                        root.SigmaY(gate.Target.OffsetToRoot, control);
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.SigmaZ: // the same as inversion
                        root.SigmaZ(gate.Target.OffsetToRoot, control);
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.SqrtX:
                        if (runBackward)
                        {
                            Complex[,] m = new Complex[2, 2];
                            Complex a = new Complex(0.5, -0.5);
                            Complex b = new Complex(0.5, 0.5);
                            m[0, 0] = b;
                            m[0, 1] = a;
                            m[1, 0] = a;
                            m[1, 1] = b;
                            root.Gate1(m, gate.Target.OffsetToRoot, control);
                        }
                        else
                        {
                            root.SqrtX(gate.Target.OffsetToRoot, control);
                        }
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.RotateX:
                        RotateXGate rx = gate as RotateXGate;
                        if (runBackward)
                        {
                            root.RotateX(-rx.Gamma, rx.Target.OffsetToRoot, control);
                        }
                        else
                        {
                            root.RotateX(rx.Gamma, rx.Target.OffsetToRoot, control);
                        }
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.RotateY:
                        RotateYGate ry = gate as RotateYGate;
                        if (runBackward)
                        {
                            root.RotateY(-ry.Gamma, ry.Target.OffsetToRoot, control);
                        }
                        else
                        {
                            root.RotateY(ry.Gamma, ry.Target.OffsetToRoot, control);
                        }
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.RotateZ:
                        RotateZGate rz = gate as RotateZGate;
                        if (runBackward)
                        {
                            root.RotateZ(-rz.Gamma, rz.Target.OffsetToRoot, control);
                        }
                        else
                        {
                            root.RotateZ(rz.Gamma, rz.Target.OffsetToRoot, control);
                        }
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.PhaseKick:
                        PhaseKickGate pk = gate as PhaseKickGate;
                        if (pk.Controls.Length > 0)
                        {
                            int[] controls1 = pk.Controls.Select<RegisterRefModel, int>(x => x.OffsetToRoot).ToArray<int>();
                            if (runBackward)
                            {
                                root.PhaseKick(-pk.Gamma, pk.Target.OffsetToRoot, controls1);
                            }
                            else
                            {
                                root.PhaseKick(pk.Gamma, pk.Target.OffsetToRoot, controls1);
                            }
                        }
                        else
                        {
                            if (runBackward)
                            {
                                root.PhaseKick(-pk.Gamma, pk.Target.OffsetToRoot);
                            }
                            else
                            {
                                root.PhaseKick(pk.Gamma, pk.Target.OffsetToRoot);
                            }
                        }
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.PhaseScale:
                        PhaseScaleGate ps = gate as PhaseScaleGate;
                        if (runBackward)
                        {
                            root.PhaseScale(-ps.Gamma, ps.Target.OffsetToRoot, control);
                        }
                        else
                        {
                            root.PhaseScale(ps.Gamma, ps.Target.OffsetToRoot, control);
                        }
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                   case GateName.CPhaseShift:
                        CPhaseShiftGate cps = gate as CPhaseShiftGate;
                        if (cps.Controls.Length > 0)
                        {
                            int[] controls1 = cps.Controls.Select<RegisterRefModel, int>(x => x.OffsetToRoot).ToArray<int>();
                            if (runBackward)
                            {
                                root.InverseCPhaseShift(cps.Dist, cps.Target.OffsetToRoot, controls1);
                            }
                            else
                            {
                                root.CPhaseShift(cps.Dist, cps.Target.OffsetToRoot, controls1);
                            }
                        }
                        else
                        {
                            if (runBackward)
                            {
                                root.InverseCPhaseShift(cps.Dist, cps.Target.OffsetToRoot);
                            }
                            else
                            {
                                root.CPhaseShift(cps.Dist, cps.Target.OffsetToRoot);
                            }
                        }
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.InvCPhaseShift:
                        InvCPhaseShiftGate icps = gate as InvCPhaseShiftGate;
                        if (icps.Controls.Length > 0)
                        {
                            int[] controls1 = icps.Controls.Select<RegisterRefModel, int>(x => x.OffsetToRoot).ToArray<int>();
                            if (runBackward)
                            {
                                root.CPhaseShift(icps.Dist, icps.Target.OffsetToRoot, controls1);
                            }
                            else
                            {
                                root.InverseCPhaseShift(icps.Dist, icps.Target.OffsetToRoot, controls1);
                            }
                        }
                        else
                        {
                            if (runBackward)
                            {
                                root.CPhaseShift(icps.Dist, icps.Target.OffsetToRoot);
                            }
                            else
                            {
                                root.InverseCPhaseShift(icps.Dist, icps.Target.OffsetToRoot);
                            }
                        }
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.CNot:
                        CNotGate cn = gate as CNotGate;
                        root.CNot(cn.Target.OffsetToRoot, cn.Control.Value.OffsetToRoot);
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.Toffoli:
                        ToffoliGate t = gate as ToffoliGate;
                        int[] controls = t.Controls.Select<RegisterRefModel, int>(x => x.OffsetToRoot).ToArray<int>();
                        root.Toffoli(t.Target.OffsetToRoot, controls);
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.Unitary:
                        UnitaryGate u = gate as UnitaryGate;
                        if (runBackward)
                        {
                            Complex[,] m = new Complex[2, 2];
                            m[0, 0] = Complex.Conjugate(u.Matrix[0, 0]);
                            m[0, 1] = Complex.Conjugate(u.Matrix[1, 0]);
                            m[1, 0] = Complex.Conjugate(u.Matrix[0, 1]);
                            m[1, 1] = Complex.Conjugate(u.Matrix[1, 1]);
                            root.Gate1(m, gate.Target.OffsetToRoot, control);
                        }
                        else
                        {
                            root.Gate1(u.Matrix, u.Target.OffsetToRoot, control);
                        }
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.Measure:
                        MeasureGate mg = gate as MeasureGate;
                        for (int j = mg.Begin; j <= mg.End; j++)
                        {
                            root.Measure(j);
                        }
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.Parametric:
                        ParametricGate pg = gate as ParametricGate;
                        _comp.Evaluate(pg, runBackward);
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.Composite:
                        CompositeGate cg = gate as CompositeGate;
                        List<Gate> actual = _comp.GetActualGates(cg);
                        if (runBackward)
                        {
                            actual.Reverse();
                        }
                        foreach (Gate g in actual)
                        {
                            // TODO refactor RunStep() into RunGate()
                            List<Gate> toRun = new List<Gate>() { g };
                            RunStep(toRun, runBackward, true);
                        }
                        somethingChanged = true;
                        i = gate.End + 1;
                        break;
                    case GateName.Empty:
                    default:
                        i++;
                        break;
                }
            }
            return somethingChanged;
        }

        public bool RunToEnd(IList<StepModel> steps, int currentStep)
        {
            bool somethingChanged = false;
            for (int i = currentStep; i < steps.Count; i++)
            {
                bool changedInStep = RunStep(steps[i].Gates);
                somethingChanged = somethingChanged || changedInStep;
            }
            return somethingChanged;
        }

        #endregion // Public Methods


        #region Private Methods

        #endregion // Private Methods
    }
}
