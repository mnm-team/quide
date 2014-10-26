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
using System.Numerics;
using System.Text;
using System.Windows;


namespace QuantumModel
{
    public class ComputerModel
    {
        #region Events

        public event RoutedEventHandler StepChanged;
        private void OnStepChanged()
        {
            if (StepChanged != null)
            {
                StepChanged(this, new RoutedEventArgs());
            }
        }

        public event RoutedEventHandler SelectionChanged;
        private void OnSelectionChanged()
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new RoutedEventArgs());
            }
        }

        #endregion // Events


        #region Fields

        private ObservableCollection<RegisterModel> _registers;
        private ObservableCollection<StepModel> _steps;
        //private Dictionary<int, string> _stepConsoleOutput;

        private Selection? _selectedItems;
        private int _currentStep;
        private int _stepForNewGates = -1;

        private List<List<Gate>> _clipboard;
        private Selection? _clipboardSelection;

        private Dictionary<string, List<Gate>> _compositeGates = new Dictionary<string,List<Gate>>();

        #endregion // Fields


        #region Public Constants

        public const int InitialQubitsCount = 2;

        public const int InitialRegistersCount = 1;

        public const int InitialStepsCount = 8;

        #endregion // Public Constants


        #region Model Properties

        public ObservableCollection<RegisterModel> Registers
        {
            get
            {
                if (_registers == null)
                {
                    CreateRegisters();
                }
                return _registers;
            }
            set { _registers = value; }
        }

        public int TotalWidth
        {
            get
            {
                int totalWidth = 0;
                foreach (RegisterModel reg in _registers)
                {
                    totalWidth += reg.Qubits.Count;
                }
                return totalWidth;
            }
        }

        public ObservableCollection<StepModel> Steps
        {
            get
            {
                if (_steps == null)
                {
                    CreateSteps();
                }
                return _steps;
            }
            set { _steps = value; }
        }

        public int CurrentStep
        {
            get
            {
                return _currentStep;
            }
            set
            {
                if (value != _currentStep)
                {
                    _currentStep = value;
                    OnStepChanged();
                }
            }
        }

        public Selection? SelectedItems
        {
            get { return _selectedItems; }
            private set
            {
                if (value != _selectedItems)
                {
                    UnselectedItems = _selectedItems;
                    _selectedItems = value;
                    OnSelectionChanged();
                }
            }
        }

        public Selection? UnselectedItems
        {
            get;
            private set;
        }

        public Dictionary<string, List<Gate>> CompositeGates
        {
            get { return _compositeGates; }
        }

        #endregion // Model Properties


        #region Constructor

        private ComputerModel()
        {
        }

        #endregion // Constructor

        #region Public Methods

        public static ComputerModel CreateModelForGUI()
        {
            ComputerModel toReturn = new ComputerModel();
            toReturn.CreateRegisters();
            toReturn.CreateSteps();
            return toReturn;
        }

        public static ComputerModel CreateModelForParser()
        {
            ComputerModel toReturn = new ComputerModel();
            toReturn._registers = new ObservableCollection<RegisterModel>();
            toReturn._steps = new ObservableCollection<StepModel>();
            return toReturn;
        }

        //only for GUI, after add gete in last step
        public void AddStepAfter(int column)
        {
            if (_steps.Count == column + 1)
            {
                _steps.Add(new StepModel(Registers));
            }
        }

        //only for GUI
        public void InsertQubitBelow(int registerIndex, int row)
        {
            if (_currentStep != 0)
            {
                return;
            }
            InsertQubitAbove(registerIndex, row - 1);
        }

        //only for GUI
        public void InsertQubitAbove(int registerIndex, int row)
        {
            if (_currentStep != 0)
            {
                return;
            }

            SelectedItems = null;

            RegisterModel reg = _registers[registerIndex];
            reg.InsertQubit(row + 1);

            int offset = row + 1;
            int i = 0;
            for (; i < registerIndex; i++)
            {
                _registers[i].IncrementOffset();
            }
            i++;
            for (; i < _registers.Count; i++)
            {
                offset += _registers[i].Qubits.Count;
            }

            foreach (StepModel step in Steps)
            {
                int gatesNum = reg.Qubits.Count - row - 2;
                for (int j = offset; j < gatesNum + offset; )
                {
                    int prevEnd = step.Gates[j].End;
                    step.Gates[j].IncrementRow(reg, row);
                    j = prevEnd + 1;
                }

                if (offset > 0 && step.Gates[offset - 1].End >= offset)
                {
                    //multiqubit gate
                    step.Gates.Insert(offset, step.Gates[offset - 1]);
                }
                else
                {
                    step.Gates.Insert(offset, new EmptyGate(new RegisterRefModel() { Register = reg, Offset = row + 1 }));
                }
            }
        }

        public void InsertRegisterBelow(int registerIndex, int width, Dictionary<ulong, Complex> initValues)
        {
            if (_currentStep != 0)
            {
                return;
            }
            InsertRegisterAbove(registerIndex + 1, width, initValues);
        }

        public void InsertRegisterAbove(int registerIndex, int width, Dictionary<ulong, Complex> initValues)
        {
            if (_currentStep != 0)
            {
                return;
            }

            SelectedItems = null;

            int offsetToRoot = 0;
            for (int i = _registers.Count - 1; i > registerIndex - 1; i--)
            {
                offsetToRoot += _registers[i].Qubits.Count;

                //increment registerIndex => updateName
                _registers[i].UpdateIndex(i + 1);
            }
            RegisterModel register = new RegisterModel(registerIndex, offsetToRoot, width, initValues);
            _registers.Insert(registerIndex, register);

            // update offset to Root
            int offsetChange = register.Qubits.Count;
            for (int i = 0; i < registerIndex; i++)
            {
                _registers[i].IncrementOffset(offsetChange);
            }

            foreach (StepModel step in Steps)
            {
                if (offsetToRoot > 0 && step.Gates[offsetToRoot - 1].End >= offsetToRoot)
                {
                    //multiqubit gate
                    for (int i = 0; i < offsetChange; i++)
                    {
                        step.Gates.Insert(offsetToRoot + i, step.Gates[offsetToRoot - 1]);
                    }
                }
                else
                {
                    for (int i = 0; i < offsetChange; i++)
                    {
                        step.Gates.Insert(offsetToRoot + i, new EmptyGate(new RegisterRefModel() { Register = register, Offset = i }));
                    }
                }
            }
        }

        public void DeleteQubit(int registerIndex, int row)
        {
            if (_currentStep != 0)
            {
                return;
            }

            SelectedItems = null;

            RegisterModel reg = _registers[registerIndex];
            int offsetToRoot = reg.OffsetToRoot + row;

            foreach (StepModel step in Steps)
            {
                Gate oldGate = step.Gates[offsetToRoot];
                if (oldGate.Name != GateName.Empty)
                {
                    for (int j = oldGate.Begin; j <= oldGate.End; j++)
                    {
                        RegisterRefModel gateRef = GetRefFromOffset(j);
                        Gate newGate = new EmptyGate(gateRef);
                        step.SetGate(newGate);
                    }
                }

                for (int i = offsetToRoot + 1; i < reg.OffsetToRoot + reg.Qubits.Count; i++)
                {
                    step.Gates[i].IncrementRow(reg, row, -1);
                    i = step.Gates[i].End + 1;
                }

                //step.Gates.RemoveAt(offsetToRoot);

            }

            reg.Qubits.RemoveAt(row);

            for (int i = 0; i < registerIndex; i++)
            {
                _registers[i].IncrementOffset(-1);
            }

            foreach (StepModel step in Steps)
            {
                step.Gates.RemoveAt(offsetToRoot);
            }
        }

        public void DeleteRegister(int registerIndex)
        {
            if (_currentStep != 0)
            {
                return;
            }

            SelectedItems = null;

            RegisterModel reg = _registers[registerIndex];
            int regBegin = reg.OffsetToRoot;
            int regEnd = reg.OffsetToRoot + reg.Qubits.Count - 1;
            int regLength = reg.Qubits.Count;

            foreach (StepModel step in Steps)
            {
                Gate firstOld = step.Gates[regBegin];
                if (firstOld.Begin < regBegin)
                {
                    for (int j = firstOld.Begin; j <= firstOld.End; j++)
                    {
                        RegisterRefModel gateRef = GetRefFromOffset(j);
                        Gate newGate = new EmptyGate(gateRef);
                        step.SetGate(newGate);
                    }
                }
                Gate lastOld = step.Gates[regEnd];
                if (lastOld.End > regEnd)
                {
                    for (int j = lastOld.Begin; j <= lastOld.End; j++)
                    {
                        RegisterRefModel gateRef = GetRefFromOffset(j);
                        Gate newGate = new EmptyGate(gateRef);
                        step.SetGate(newGate);
                    }
                }

                for (int i = 0; i < regLength; i++)
                {
                    step.Gates.RemoveAt(regBegin);
                }
            }

            int k = 0;
            for (; k < registerIndex; k++)
            {
                _registers[k].IncrementOffset(-regLength);
            }
            k++;
            for (; k < _registers.Count; k++)
            {
                _registers[k].UpdateIndex(k - 1);
            }

            _registers.RemoveAt(registerIndex);
        }

        public void InsertStepLeft(int column)
        {
            if (_currentStep > 0 && _currentStep >= column)
            {
                return;
            }

            SelectedItems = null;

            _steps.Insert(column, new StepModel(Registers));
        }

        public void InsertStepRight(int column)
        {
            SelectedItems = null;

            _steps.Insert(column + 1, new StepModel(Registers));
        }

        public void DeleteStep(int column)
        {
            SelectedItems = null;

            _steps.RemoveAt(column);
        }

        public void AddRegister(RegisterModel register)
        {
            // for parser, if registerModel is not null

            // update offset to Root
            int offsetChange = register.Qubits.Count;
            foreach (RegisterModel reg in _registers)
            {
                reg.IncrementOffset(offsetChange);
            }

            _registers.Add(register);

            foreach (StepModel step in Steps)
            {
                for (int i = 0; i < offsetChange; i++)
                {
                    step.Gates.Insert(i, new EmptyGate(new RegisterRefModel() { Register = register, Offset = i }));
                }
            }
        }

        public void SetStepForGates(int column)
        {
            _stepForNewGates = column;
        }

        public void ResetStepForGates()
        {
            _stepForNewGates = -1;
        }

        public void AddGate(Gate gate)
        {
            //StepModel currentStep = _steps.Count > 0 ? _steps[_steps.Count - 1] : null;
            //if (currentStep != null && currentStep.HasPlace(gate.Begin, gate.End))
            //{
            //    currentStep.SetGate(gate);
            //}
            //else
            //{

            if (_stepForNewGates == -1)
            {
                StepModel step = new StepModel(Registers);
                step.SetGate(gate);
                _steps.Add(step);
            }
            else
            {
                InsertStepRight(_stepForNewGates);
                _stepForNewGates++;
                _steps[_stepForNewGates].SetGate(gate);
            }
                
            //}
        }

        public RegisterRefModel GetRefFromOffset(int offsetToRoot)
        {
            int tmp = 0;
            for (int i = _registers.Count - 1; i >= 0; i--)
            {
                int qubits = _registers[i].Qubits.Count;
                if (tmp + qubits > offsetToRoot)
                {
                    return new RegisterRefModel() { Register = _registers[i], Offset = offsetToRoot - tmp };
                }
                tmp += qubits;
            }
            throw new IndexOutOfRangeException();
        }

        public bool MeasurementExist(int column)
        {
            StepModel step = _steps[column];
            foreach (Gate g in step.Gates)
            {
                if (g.Name == GateName.Measure)
                {
                    return true;
                }
            }
            return false;
        }

        public bool CanStepBack(int column)
        {
            StepModel step = _steps[column];
            foreach (Gate g in step.Gates)
            {
                if (g.Name == GateName.Measure)
                {
                    return false;
                }
                if (g.Name == GateName.Parametric)
                {
                    ParametricGate cg = g as ParametricGate;
                    if (cg.InverseMethod == null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsSelected(int row, int column)
        {
            if (_selectedItems.HasValue)
            {
                if (column >= _selectedItems.Value.BeginColumn && column <= _selectedItems.Value.EndColumn)
                {
                    if (row >= _selectedItems.Value.BeginRow && row <= _selectedItems.Value.EndRow)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Select(int pressedRow, int row, int pressedColumn, int column)
        {
            SelectedItems = new Selection(pressedRow, row, pressedColumn, column);
        }

        public void Delete()
        {
            if (_selectedItems.HasValue)
            {
                if (_selectedItems.Value.RowSpan == 1 && _selectedItems.Value.ColumnSpan == 1)
                {
                    int column = _selectedItems.Value.BeginColumn;
                    int row = _selectedItems.Value.BeginRow;
                    Gate oldGate = _steps[column].Gates[row];
                    if (oldGate.Name != GateName.Empty)
                    {
                        for (int i = oldGate.Begin; i <= oldGate.End; i++)
                        {
                            RegisterRefModel gateRef = GetRefFromOffset(i);
                            Gate newGate = new EmptyGate(gateRef);
                            _steps[column].SetGate(newGate);
                        }

                        if (oldGate is MultiControlledGate)
                        {
                            MultiControlledGate mcg = oldGate as MultiControlledGate;
                            RegisterRefModel rowRef = GetRefFromOffset(row);
                            if (mcg.Controls.Contains<RegisterRefModel>(rowRef))
                            {
                                RegisterRefModel[] toRemove = new RegisterRefModel[] { rowRef };
                                RegisterRefModel[] newControls = mcg.Controls.Except<RegisterRefModel>(toRemove).ToArray();
                                Gate toAdd = null;
                                if (oldGate.Name == GateName.PhaseKick)
                                {
                                    PhaseKickGate pk = oldGate as PhaseKickGate;
                                    toAdd = new PhaseKickGate(pk.Gamma, pk.Target, newControls);
                                }
                                else if (oldGate.Name == GateName.CPhaseShift)
                                {
                                    CPhaseShiftGate cps = oldGate as CPhaseShiftGate;
                                    toAdd = new CPhaseShiftGate(cps.Dist, cps.Target, newControls);
                                }
                                else if (oldGate.Name == GateName.InvCPhaseShift)
                                {
                                    InvCPhaseShiftGate icps = oldGate as InvCPhaseShiftGate;
                                    toAdd = new InvCPhaseShiftGate(icps.Dist, icps.Target, newControls);
                                }
                                else // Toffoli
                                {
                                    if (newControls.Length > 1)
                                    {
                                        toAdd = new ToffoliGate(oldGate.Target, newControls);
                                    }
                                    else
                                    {
                                        toAdd = new CNotGate(oldGate.Target, newControls[0]);
                                    }
                                }
                                _steps[column].SetGate(toAdd);
                            }
                        }
                        else if (oldGate.Name == GateName.CNot)
                        {
                            if (oldGate.Control.Value.OffsetToRoot == row)
                            {
                                Gate toAdd = new SigmaXGate(oldGate.Target);
                                _steps[column].SetGate(toAdd);
                            }
                        }
                        else if (oldGate is SingleGate)
                        {
                            SingleGate sg = oldGate as SingleGate;
                            if (sg.Control.HasValue)
                            {
                                if (sg.Control.Value.OffsetToRoot == row)
                                {
                                    Gate toAdd = null;
                                    switch (sg.Name)
                                    {
                                        case GateName.Hadamard:
                                            toAdd = new HadamardGate(sg.Target);
                                            break;
                                        case GateName.PhaseScale:
                                            toAdd = new PhaseScaleGate((sg as PhaseScaleGate).Gamma, sg.Target);
                                            break;
                                        case GateName.RotateX:
                                            toAdd = new RotateXGate((sg as RotateXGate).Gamma, sg.Target);
                                            break;
                                        case GateName.RotateY:
                                            toAdd = new RotateYGate((sg as RotateYGate).Gamma, sg.Target);
                                            break;
                                        case GateName.RotateZ:
                                            toAdd = new RotateZGate((sg as RotateZGate).Gamma, sg.Target);
                                            break;
                                        case GateName.SigmaX:
                                            toAdd = new SigmaXGate(sg.Target);
                                            break;
                                        case GateName.SigmaY:
                                            toAdd = new SigmaYGate(sg.Target);
                                            break;
                                        case GateName.SigmaZ:
                                            toAdd = new SigmaZGate(sg.Target);
                                            break;
                                        case GateName.SqrtX:
                                            toAdd = new SqrtXGate(sg.Target);
                                            break;
                                        case GateName.Unitary:
                                            toAdd = new UnitaryGate((sg as UnitaryGate).Matrix, sg.Target);
                                            break;
                                        default:
                                            break;
                                    }
                                    _steps[column].SetGate(toAdd);
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int i = _selectedItems.Value.BeginColumn; i <= _selectedItems.Value.EndColumn; i++)
                    {
                        for (int j = _selectedItems.Value.BeginRow; j <= _selectedItems.Value.EndRow; j++)
                        {
                            Gate oldGate = _steps[i].Gates[j];

                            for (int k = oldGate.Begin; k <= oldGate.End; k++)
                            {
                                RegisterRefModel gateRef = GetRefFromOffset(k);
                                Gate newGate = new EmptyGate(gateRef);
                                _steps[i].SetGate(newGate);
                            }
                            j = oldGate.End;
                        }
                    }
                }
                
            }
        }

        public void Cut()
        {
            CutCopy(true);
        }

        public void Copy()
        {
            CutCopy(false);
        }

        public void Paste()
        {
            if (_selectedItems.HasValue)
            {
                int clickedColumn = _selectedItems.Value.BeginColumn;
                int clickedRow = _selectedItems.Value.EndRow;

                if (_clipboard != null)
                {
                    int endColumn = clickedColumn + _clipboardSelection.Value.ColumnSpan - 1;
                    int beginRow = clickedRow - _clipboardSelection.Value.RowSpan + 1;

                    int rightColumn = Math.Min(endColumn, _steps.Count - 1);
                    int bottomRow = Math.Max(beginRow, 0);

                    int newUpperRow = bottomRow + _clipboardSelection.Value.RowSpan - 1;

                    // remove old gates
                    for (int i = clickedColumn; i <= rightColumn; i++)
                    {
                        Gate beginGate = _steps[i].Gates[bottomRow];
                        if (beginGate.Begin < bottomRow)
                        {
                            for (int j = beginGate.Begin; j <= beginGate.End; j++)
                            {
                                RegisterRefModel gateRef = GetRefFromOffset(j);
                                Gate newGate = new EmptyGate(gateRef);
                                _steps[i].SetGate(newGate);
                            }
                        }
                        Gate endGate = _steps[i].Gates[clickedRow];
                        if (endGate.End > clickedRow)
                        {
                            for (int j = endGate.Begin; j <= endGate.End; j++)
                            {
                                RegisterRefModel gateRef = GetRefFromOffset(j);
                                Gate newGate = new EmptyGate(gateRef);
                                _steps[i].SetGate(newGate);
                            }
                        }
                    }

                    // make place, if it was too little
                    for (int i = rightColumn; i < endColumn; i++)
                    {
                        InsertStepRight(i);
                    }
                    for (int i = bottomRow; i > beginRow; i--)
                    {
                        InsertQubitBelow(_registers.Count - 1, 0);
                    }

                    // paste
                    for (int i = 0; i < _clipboard.Count; i++)
                    {
                        foreach (Gate oldGate in _clipboard[i])
                        {
                            Gate newGate = PasteGate(oldGate, bottomRow);
                            _steps[i + clickedColumn].SetGate(newGate);
                        }
                    }

                    // and move selection
                    SelectedItems = new Selection
                        (bottomRow,
                        newUpperRow,
                        clickedColumn,
                        endColumn);
                }
            }
        }

        public List<Gate> GetSelectedGates()
        {
            List<Gate> selected = null;
            if (_selectedItems.HasValue)
            {
                selected = new List<Gate>();
                for (int i = _selectedItems.Value.BeginColumn; i <= _selectedItems.Value.EndColumn; i++)
                {
                    for (int j = _selectedItems.Value.BeginRow; j <= _selectedItems.Value.EndRow; j++)
                    {
                        if ((_steps[i].Gates[j].Begin >= _selectedItems.Value.BeginRow) &&
                            (_steps[i].Gates[j].End <= _selectedItems.Value.EndRow))
                        {
                            Gate oldGate = _steps[i].Gates[j];
                            if (oldGate.Name != GateName.Empty)
                            {
                                selected.Add(oldGate.Copy(_selectedItems.Value.BeginRow));
                            }
                            j = oldGate.End;
                        }
                    }
                }
                if (selected.Count < 2)
                {
                    throw new Exception("Too few gates selected. Composite gate can be created from at least 2 gates.");
                }
            }
            else
            {
                throw new Exception("No items selected.");
            }
            return selected;
        }

        public void MakeComposite(string name, List<Gate> toGroup)
        {
            Delete();

            RegisterPartModel target = new RegisterPartModel()
            {
                Register = null,
                Offset = 0,
                Width = toGroup.Select<Gate, int>(x => x.End).Max() + 1
            };
            CompositeGate cg = new CompositeGate(name, target);

            foreach (StepModel step in _steps)
            {
                for (int j = 0; j < step.Gates.Count; j++)
                {
                    Gate oldGate = step.Gates[j];
                    CustomGate custom = oldGate as CustomGate;
                    if (custom != null && custom.FunctionName.Equals(name))
                    {
                        for (int k = oldGate.Begin; k <= oldGate.End; k++)
                        {
                            RegisterRefModel gateRef = GetRefFromOffset(k);
                            Gate newGate = new EmptyGate(gateRef);
                            step.SetGate(newGate);
                        }
                        Gate replacement = PasteGate(cg, oldGate.Begin);
                        if (step.HasPlace(replacement.Begin, replacement.End))
                        {
                            step.SetGate(replacement);
                        }
                    }
                    j = oldGate.End;
                }
            }

            _compositeGates[name] = toGroup;
            
            //CompositeGate cg = new CompositeGate(name, toGroup, target);
            
            Gate toPaste = PasteGate(cg, _selectedItems.Value.BeginRow);

            bool notPasted = true;
            int i = _selectedItems.Value.BeginColumn;
            while (notPasted && i <= _selectedItems.Value.EndColumn)
            {
                if (_steps[i].HasPlace(toPaste.Begin, toPaste.End))
                {
                    _steps[i].SetGate(toPaste);
                    notPasted = false;
                }
                i++;
            }
            if (notPasted)
            {
                InsertStepLeft(_selectedItems.Value.BeginColumn);
                _steps[_selectedItems.Value.BeginColumn].SetGate(toPaste);
            }
        }

        public int MinWidthForComposite(string name)
        {
            List<Gate> result = null;
            _compositeGates.TryGetValue(name, out result);
            if (result != null && result.Count > 0)
            {
                int minWidth = result.Select<Gate, int>(x => x.End).Max() + 1;
                return minWidth;
            }
            else
            {
                throw new Exception("No such method.");
            }
        }

        public List<Gate> FindComposite(string name)
        {
            List<Gate> result = null;
            _compositeGates.TryGetValue(name, out result);
            return result;
        }

        public List<Gate> GetActualGates(CompositeGate cg)
        {
            List<Gate> defined;
            if (_compositeGates.TryGetValue(cg.FunctionName, out defined))
            {
                return defined.Select<Gate, Gate>(x => PasteGate(x, cg.Begin)).ToList();
            }
            return null;
        }


        #endregion // Public Methods


        #region Private Helpers

        private void CutCopy(bool cut)
        {
            if (_selectedItems.HasValue)
            {
                _clipboard = new List<List<Gate>>();

                _clipboardSelection = new Selection(
                    0,
                    _selectedItems.Value.RowSpan - 1,
                    0,
                    _selectedItems.Value.ColumnSpan - 1);

                for (int i = _selectedItems.Value.BeginColumn; i <= _selectedItems.Value.EndColumn; i++)
                {
                    List<Gate> current = new List<Gate>();
                    _clipboard.Add(current);

                    for (int j = _selectedItems.Value.BeginRow; j <= _selectedItems.Value.EndRow; j++)
                    {
                        if ((_steps[i].Gates[j].Begin >= _selectedItems.Value.BeginRow) &&
                            (_steps[i].Gates[j].End <= _selectedItems.Value.EndRow))
                        {
                            Gate oldGate = _steps[i].Gates[j];
                            
                            current.Add(oldGate.Copy(_selectedItems.Value.BeginRow));
                            
                            if (cut && oldGate.Name != GateName.Empty)
                            {
                                for (; j <= oldGate.End; j++)
                                {
                                    RegisterRefModel gateRef = GetRefFromOffset(j);
                                    Gate newGate = new EmptyGate(gateRef);
                                    _steps[i].SetGate(newGate);
                                }
                            }
                            j = oldGate.End;
                        }
                    }
                }
            }
        }

        private Gate PasteGate(Gate gate, int referenceBeginRow)
        {
            RegisterRefModel targetRef = GetRefFromOffset(gate.Target.OffsetToRoot + referenceBeginRow);
            RegisterRefModel? controlRef = null;
            if (gate.Control.HasValue)
            {
                controlRef = GetRefFromOffset(gate.Control.Value.OffsetToRoot + referenceBeginRow);
            }

            switch (gate.Name)
            {
                case GateName.CNot:
                    return new CNotGate(targetRef, controlRef.Value);
                case GateName.Parametric:
                    return PasteParametricGate(gate as ParametricGate, referenceBeginRow);
                case GateName.Composite:
                    CompositeGate cg = gate as CompositeGate;
                    RegisterRefModel partEndRef = GetRefFromOffset(cg.TargetRegister.EndOffsetToRoot + referenceBeginRow);
                    if (partEndRef.Register.OffsetToRoot == targetRef.Register.OffsetToRoot)
                    {
                        RegisterPartModel target = new RegisterPartModel()
                        {
                            Register = targetRef.Register,
                            Offset = targetRef.Offset,
                            Width = cg.TargetRegister.Width
                        };
                        return new CompositeGate(cg.FunctionName, target);
                    }
                    else
                    {
                        RegisterPartModel target = new RegisterPartModel()
                        {
                            Register = null,
                            Offset = cg.TargetRegister.Offset + referenceBeginRow,
                            Width = cg.TargetRegister.Width
                        };
                        return new CompositeGate(cg.FunctionName, target);
                    }
                case GateName.CPhaseShift:
                    CPhaseShiftGate cps = gate as CPhaseShiftGate;
                    RegisterRefModel[] controls = cps.Controls.Select<RegisterRefModel, RegisterRefModel>(x =>
                        GetRefFromOffset(x.OffsetToRoot + referenceBeginRow)).ToArray<RegisterRefModel>();
                    return new CPhaseShiftGate(cps.Dist, targetRef, controls);
                case GateName.Empty:
                    return new EmptyGate(targetRef);
                case GateName.Hadamard:
                    return new HadamardGate(targetRef, controlRef);
                case GateName.InvCPhaseShift:
                    InvCPhaseShiftGate icps = gate as InvCPhaseShiftGate;
                    RegisterRefModel[] icontrols = icps.Controls.Select<RegisterRefModel, RegisterRefModel>(x =>
                        GetRefFromOffset(x.OffsetToRoot + referenceBeginRow)).ToArray<RegisterRefModel>();
                    return new InvCPhaseShiftGate(icps.Dist, targetRef, icontrols);
                case GateName.Measure:
                    MeasureGate mg = gate as MeasureGate;
                    RegisterRefModel beginRef = GetRefFromOffset(mg.Begin + referenceBeginRow);
                    RegisterRefModel endRef = GetRefFromOffset(mg.End + referenceBeginRow);
                    return new MeasureGate(beginRef, endRef);
                case GateName.PhaseKick:
                    PhaseKickGate pk = gate as PhaseKickGate;
                    RegisterRefModel[] controls1 = pk.Controls.Select<RegisterRefModel, RegisterRefModel>(x =>
                        GetRefFromOffset(x.OffsetToRoot + referenceBeginRow)).ToArray<RegisterRefModel>();
                    return new PhaseKickGate(pk.Gamma, targetRef, controls1);
                case GateName.PhaseScale:
                    PhaseScaleGate ps = gate as PhaseScaleGate;
                    return new PhaseScaleGate(ps.Gamma, targetRef, controlRef);
                case GateName.RotateX:
                    RotateXGate rx = gate as RotateXGate;
                    return new RotateXGate(rx.Gamma, targetRef, controlRef);
                case GateName.RotateY:
                    RotateYGate ry = gate as RotateYGate;
                    return new RotateYGate(ry.Gamma, targetRef, controlRef);
                case GateName.RotateZ:
                    RotateZGate rz = gate as RotateZGate;
                    return new RotateZGate(rz.Gamma, targetRef, controlRef);
                case GateName.SigmaX:
                    return new SigmaXGate(targetRef);
                case GateName.SigmaY:
                    return new SigmaYGate(targetRef, controlRef);
                case GateName.SigmaZ:
                    return new SigmaZGate(targetRef, controlRef);
                case GateName.SqrtX:
                    return new SqrtXGate(targetRef, controlRef);
                case GateName.Toffoli:
                    ToffoliGate t = gate as ToffoliGate;
                    RegisterRefModel[] tcontrols = t.Controls.Select<RegisterRefModel, RegisterRefModel>(x =>
                        GetRefFromOffset(x.OffsetToRoot + referenceBeginRow)).ToArray<RegisterRefModel>();
                    return new ToffoliGate(targetRef, tcontrols);
                case GateName.Unitary:
                    UnitaryGate u = gate as UnitaryGate;
                    return new UnitaryGate(u.Matrix, targetRef, controlRef);
                default:
                    return null;
            }
        }

        private ParametricGate PasteParametricGate(ParametricGate gate, int referenceBeginRow)
        {
            Func<RegisterRefModel, RegisterRefModel> pasteRegRef = rrm =>
            {
                return GetRefFromOffset(rrm.OffsetToRoot + referenceBeginRow);
            };

            Func<RegisterPartModel, RegisterPartModel> pasteRegPart = rpm =>
            {
                RegisterRefModel beginRef = GetRefFromOffset(rpm.OffsetToRoot + referenceBeginRow);
                int width = Math.Min(beginRef.Register.Qubits.Count - beginRef.Offset, rpm.Width);

                return new RegisterPartModel()
                {
                    Register = beginRef.Register,
                    Offset = beginRef.Offset,
                    Width = width
                };
                
            };

            object[] parameters = gate.Parameters;
            object[] newPars = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].GetType() == typeof(RegisterRefModel))
                {
                    newPars[i] = pasteRegRef((RegisterRefModel)parameters[i]);
                }
                else if (parameters[i].GetType() == typeof(RegisterPartModel))
                {
                    newPars[i] = pasteRegPart((RegisterPartModel)parameters[i]);
                }
                else if (parameters[i].GetType() == typeof(RegisterRefModel[]))
                {
                    newPars[i] = (parameters[i] as RegisterRefModel[])
                        .Select<RegisterRefModel, RegisterRefModel>(x => pasteRegRef(x)).ToArray();
                }
                else if (parameters[i].GetType() == typeof(RegisterPartModel[]))
                {
                    newPars[i] = (parameters[i] as RegisterPartModel[])
                        .Select<RegisterPartModel, RegisterPartModel>(x => pasteRegPart(x)).ToArray();
                }
                else
                {
                    newPars[i] = parameters[i];
                }
            }
            ParametricGate cg = new ParametricGate(gate.Method, gate.ComputationMethod, 
                gate.InverseMethod, gate.MethodCode, newPars);
            return cg;
        }

        private void CreateRegisters()
        {
            _registers = new ObservableCollection<RegisterModel>();
            int offsetToRoot = 0;
            for (int i = 0; i < InitialRegistersCount; i++)
            {
                RegisterModel newReg = new RegisterModel(InitialRegistersCount - i - 1, offsetToRoot);
                _registers.Insert(0, newReg);
                offsetToRoot += newReg.Qubits.Count;
            }
        }

        private void CreateSteps()
        {
            _steps = new ObservableCollection<StepModel>();
            for (int i = 0; i < InitialStepsCount; i++)
            {
                _steps.Add(new StepModel(Registers));
            }
        }
        #endregion // Private Helpers
    }
}
