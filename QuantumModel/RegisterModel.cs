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
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace QuantumModel
{
    public class RegisterModel
    {
        #region Fields

        private int _index;
        private string _name;
        private int _offsetToRoot;
        private ObservableCollection<QubitModel> _qubits;

        private Dictionary<ulong, Complex> _initStates;

        //private bool _deactivateListener = false;

        #endregion // Fields


        #region Model Properties

        public string Name
        {
            get { return _name; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _name = value;
                }
            }
        }

        public int Index
        {
            get { return _index; }
        }

        public int OffsetToRoot
        {
            get { return _offsetToRoot; }
        }

        public Dictionary<ulong, Complex> InitStates
        {
            get { return _initStates; }
            set
            {
                _initStates = value;
                UpdateQubits();
            }
        }

        public ObservableCollection<QubitModel> Qubits
        {
            get
            {
                if (_qubits == null)
                {
                    _qubits = CreateQubits(ComputerModel.InitialQubitsCount);
                    //_qubits.CollectionChanged += _qubits_CollectionChanged;
                }
                return _qubits;
            }
            set
            {
                _qubits = value;
                //_qubits.CollectionChanged += _qubits_CollectionChanged;
            }
        }

        #endregion // Model Properties


        #region Constructor

        public RegisterModel(int index, int offsetToRoot,
            int initWidth = ComputerModel.InitialQubitsCount,
            IReadOnlyDictionary<ulong, Complex> initStates = null)
        {
            _index = index;
            _name = GenerateName(index);
            
            _offsetToRoot = offsetToRoot;
            _initStates = new Dictionary<ulong, Complex>();
            if (initStates == null)
            {
                _initStates[0] = Complex.One;
            }
            else
            {
                foreach (var pair in initStates)
                {
                    _initStates.Add(pair.Key, pair.Value);
                }
            }           
            Qubits = CreateQubits(initWidth);
        }

        #endregion // Constructor


        #region Public Methods

        public void ResetQubit(int position, QubitModel oldValue)
        {
            if (oldValue == QubitModel.Unknown)
            {
                for (int i = 0; i < _qubits.Count; i++)
                {
                    QubitModel q = _qubits[i];
                    if (q == QubitModel.Unknown)
                    {
                        _qubits[i] = QubitModel.Zero;
                    }
                }
                UpdateInitStates();
            }
            else
            {
                UpdateInitStates(position);
            }
        }

        #endregion // Public Methods


        #region Internal Methods

        internal void AddNewQubit()
        {
            int newRow = _qubits.Count;
            QubitModel newQubit = QubitModel.Zero;
            _qubits.Add(newQubit);
        }

        internal void InsertQubit(int position)
        {
            QubitModel newQubit = QubitModel.Zero;
            _qubits.Insert(position, newQubit);
        }

        internal void IncrementOffset(int value = 1)
        {
            _offsetToRoot += value;
        }

        internal void UpdateIndex(int newIndex)
        {
            _index = newIndex;
            Name = GenerateName(newIndex);
        }
        #endregion // Internal Methods


        #region Private Helpers

        private string GenerateName(int index)
        {
            int xLetterAscii = 120;
            int aLetterAscii = 97;
            char letter;
            if (index < 3)
            {
                letter = Convert.ToChar(xLetterAscii + index);
            }
            else if (index < xLetterAscii - aLetterAscii)
            {
                letter = Convert.ToChar(aLetterAscii + index - 3);
            }
            else
            {
                return "r" + index;
            }
            return letter.ToString();
        }

        //private void _qubits_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    if (!_deactivateListener)
        //    {
        //        switch (e.Action)
        //        {
        //            case NotifyCollectionChangedAction.Replace:
        //                //List<QubitModel> tmp = new List<QubitModel>(_qubits);
        //                QubitModel oldQubit = (QubitModel)(e.OldItems[0]);
        //                if (oldQubit == QubitModel.Unknown)
        //                {
        //                    for (int i = 0; i < _qubits.Count; i++)
        //                    {
        //                        QubitModel q = _qubits[i];
        //                        if (q == QubitModel.Unknown)
        //                        {
        //                            _qubits[i] = QubitModel.Zero;
        //                        }
        //                    }
        //                    //Qubits = new ObservableCollection<QubitModel>(tmp);
        //                    UpdateInitStates();
        //                }
        //                else
        //                {
        //                    UpdateInitStates(e.NewStartingIndex);
        //                }
        //                break;
        //        }
        //    }
        //}

        private void UpdateInitStates()
        {
            ulong initValue = 0;
            ulong pow2 = 1;
            int width = _qubits.Count;
            for (int j = 0; j < width; j++)
            {
                if (_qubits[j] == QubitModel.One)
                {
                    initValue += pow2;
                }
                pow2 *= 2;
            }
            _initStates = new Dictionary<ulong, Complex>();
            _initStates[initValue] = Complex.One;
        }

        private void UpdateInitStates(int index)
        {
            HashSet<ulong> done = new HashSet<ulong>();
            ulong[] states = _initStates.Keys.ToArray<ulong>();
            int length = states.Length;
            Complex t, tnot;
            for (int k = 0; k < length; k++)
            {
                ulong state = states[k];
                if (!done.Contains(state))
                {
                    // Flip the target bit of each basis state
                    //state ^= ((ulong)1 << target);
                    ulong reversedTargetState = state ^ ((ulong)1 << index);

                    tnot = 0;
                    t = _initStates[state];

                    bool reversedTargetStateExist =
                            _initStates.TryGetValue(reversedTargetState, out tnot);

                    _initStates[reversedTargetState] = t;

                    if (reversedTargetStateExist)
                    {
                        _initStates[state] = tnot;
                        done.Add(reversedTargetState);
                    }
                    else
                    {
                        _initStates.Remove(state);
                    }
                }
            }
        }

        private void UpdateQubits()
        {
            if(_qubits == null) 
            {
                return;
            }
            if (_initStates == null || _initStates.Count == 0)
            {
                _initStates = new Dictionary<ulong, Complex>();
                _initStates[0] = Complex.One;
            }

            //_deactivateListener = true;

            if (_initStates.Count == 1)
            {
                ulong tmpState = _initStates.Keys.First<ulong>();
                int i = 0;
                while (tmpState > 0 && i < _qubits.Count)
                {
                    if (tmpState % 2 > 0)
                    {
                        _qubits[i] = QubitModel.One;
                    }
                    else
                    {
                        _qubits[i] = QubitModel.Zero;
                    }
                    tmpState = tmpState / 2;
                    i++;
                }
                while (i < _qubits.Count)
                {
                    _qubits[i] = QubitModel.Zero;
                    i++;
                }
            }
            else
            {
                for (int i = 0; i < _qubits.Count; i++)
                {
                    _qubits[i] = QubitModel.Unknown;
                }
            }
            //_deactivateListener = false;
        }

        private ObservableCollection<QubitModel> CreateQubits(int initWidth)
        {
            ObservableCollection<QubitModel> qubits = new ObservableCollection<QubitModel>();

            if (_initStates == null || _initStates.Count == 0)
            {
                _initStates = new Dictionary<ulong, Complex>();
                _initStates[0] = Complex.One;
            }

            if (_initStates.Count == 1)
            {
                ulong tmpState = _initStates.Keys.First<ulong>();
                while (tmpState > 0)
                {
                    if (tmpState % 2 > 0)
                    {
                        qubits.Add(QubitModel.One);
                    }
                    else
                    {
                        qubits.Add(QubitModel.Zero);
                    }
                    tmpState = tmpState / 2;
                }
                while (qubits.Count < initWidth)
                {
                    qubits.Add(QubitModel.Zero);
                }
            }
            else
            {
                for (int i = 0; i < initWidth; i++)
                {
                    qubits.Add(QubitModel.Unknown);
                }
            }
            return qubits;
        }

        #endregion // Private Helpers
    }
}
