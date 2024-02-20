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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using Quantum.Helpers;

namespace Quantum
{
    /// <summary>
    ///     The basic unit needed for performing quantum computation.
    ///     <remarks>
    ///         <para>
    ///             QuantumComputer may have zero to many Registers, but at least one is required to perform quantum
    ///             computations.
    ///             Register consist of qubits, which are targets for quantum gates.
    ///             To apply a quantum gate, use methods of Register class (for computation on its qubits)
    ///             or extension methods of QuantumComputer class (for computation on qubits in different registers; see
    ///             <see cref="N:Quantum.Operations" />).
    ///         </para>
    ///         <para>
    ///             Register is not an independent class. It could be created and deleted only by
    ///             <see cref="QuantumComputer" />.
    ///         </para>
    ///     </remarks>
    ///     <seealso cref="QuantumComputer" />
    ///     <seealso cref="N:Quantum.Operations" />
    /// </summary>
    public class Register : IRegister
    {
        #region Internal Properties

        internal Register Root { get; private set; }

        #endregion //Internal Properties

        #region Operators

        /// <summary>
        ///     For completeness. The Register could be implicitly casted to the reference to its first qubit.
        ///     Like in arrays, where an address of an array is also the address of its first element.
        /// </summary>
        /// <param name="register">Register to cast.</param>
        /// <returns>Reference to first qubit in register.</returns>
        public static implicit operator RegisterRef(Register register)
        {
            return register[0];
        }

        #endregion // Operators

        #region private fields

        private Dictionary<ulong, Complex> _amplitudes;

        private List<Register> _childRegisters;

        private Random _random;

        #endregion

        #region Index Properties

        /// <summary>
        ///     Returns reference to single qubit in register.
        /// </summary>
        /// <param name="index">Describes qubit's offset in this Register. Index = 0 means the Least Significant Bit.</param>
        /// <returns>Returns reference to single qubit in register.</returns>
        public RegisterRef this[int index] =>
            new RegisterRef
            {
                Register = this,
                Offset = index
            };

        /// <summary>
        ///     Returns a sub-register, which begins at given offset and has given width.
        /// </summary>
        /// <param name="offset">
        ///     Begin of the sub-register. Offset = 0 indicates the Least Significant Bit of base register. Offset
        ///     must be non-negative integer.
        /// </param>
        /// <param name="width">Width of the sub-register. Must be positive integer.</param>
        /// <returns>Register, which is a part of base register, of given offset and width.</returns>
        public Register this[int offset, int width]
        {
            get
            {
                var toReturn = new Register(width, OffsetToRoot + offset, Root);
                Root._childRegisters.Add(toReturn);
                return toReturn;
            }
        }

        #endregion //Index Properties

        #region Public Properties

        /// <summary>
        ///     The number of qubits contained in Register.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        ///     Right after creation, register is independent and its OffsetToRoot equals 0.
        ///     But after operations on many registers, included this, Register could be entangled with others and is no longer
        ///     independent.
        ///     The group of entangled registers has its RootRegister. Each register in such group is de facto a part of the Root.
        ///     OffsetToRoot describes offset (from Least Significant Bit, which offset is 0) of the part in RootRegister.
        /// </summary>
        public int OffsetToRoot { get; private set; }

        #endregion //Public Properties


        #region Constructors

        internal Register(ulong initval, int width, Random random, int? size = null)
        {
            Root = this;
            OffsetToRoot = 0;
            Width = width;
            _amplitudes = size.HasValue
                ? new Dictionary<ulong, Complex>(size.Value)
                :
                //this.amplitudes = new SortedDictionary<ulong, Complex>();
                //this.amplitudes = new ConcurrentDictionary<ulong, Complex>();
                new Dictionary<ulong, Complex>();
            //this.amplitudes = new SortedDictionary<ulong, Complex>();
            //this.amplitudes = new ConcurrentDictionary<ulong, Complex>();
            _amplitudes[initval] = 1;
            _random = random;
            _childRegisters = new List<Register>();
        }

        internal Register(IDictionary<ulong, Complex> initStates, int width, Random random, int? size = null)
        {
            var sum = initStates.Values.Sum(amplitude => Math.Pow(amplitude.Magnitude, 2));
            var limit = 1.0 / ((ulong)1 << Width) * QuantumComputer.Epsilon;
            if (Math.Abs(sum - 1.0) > limit)
            {
                var sqrtSum = Math.Sqrt(sum);
                var states = initStates.Keys.ToArray();
                //we need to normalize
                foreach (var s in states) initStates[s] /= sqrtSum;
            }

            Root = this;
            OffsetToRoot = 0;
            Width = width;
            _amplitudes = size.HasValue
                ? new Dictionary<ulong, Complex>(size.Value)
                :
                //this.amplitudes = new SortedDictionary<ulong, Complex>();
                //this.amplitudes = new ConcurrentDictionary<ulong, Complex>();
                new Dictionary<ulong, Complex>();
            //this.amplitudes = new SortedDictionary<ulong, Complex>();
            //this.amplitudes = new ConcurrentDictionary<ulong, Complex>();
            foreach (var pair in initStates) _amplitudes.Add(pair.Key, pair.Value);
            _random = random;
            _childRegisters = new List<Register>();
        }

        // only used in QuantumComputer, GetRootRegister(...)
        internal Register(Random random)
        {
            Root = this;
            OffsetToRoot = 0;
            Width = 0;
            _amplitudes = null;
            _random = random;
            _childRegisters = new List<Register>();
        }

        // only used in property this[int, int]
        private Register(int width, int offsetToRoot, Register root)
        {
            Root = root;
            OffsetToRoot = offsetToRoot;
            Width = width;
        }

        #endregion

        #region Internal methods

        // only used in QuantumComputer, GetRootRegister(...)
        internal void AddRegister(Register register)
        {
            if (Root == register.Root || Root == null || register.Root == null) return;
            if (Root != this)
            {
                Root.AddRegister(register);
                return;
            }

            var rootToAdd = register.Root;
            var newAmplitudes = rootToAdd.RemoveAmplitudes();
            var newChildren = rootToAdd.RemoveChildren();
            newChildren.Add(rootToAdd);
            foreach (var child in newChildren)
            {
                child.AppendToRoot(this);
                _childRegisters.Add(child);
            }

            var oldWidth = Width;
            Width += rootToAdd.Width;

            if (_amplitudes == null)
            {
                _amplitudes = newAmplitudes;
            }
            else
            {
                ulong newState;
                if (newAmplitudes.Count == 1)
                {
                    var stateToAdd = newAmplitudes.Keys.FirstOrDefault();
                    // if stateToAdd == 0, there is nothing more to do
                    if (stateToAdd <= 0) return;
                    var states = _amplitudes.Keys.ToArray();
                    var length = states.Length;
                    for (var k = 0; k < length; k++)
                    {
                        var state = states[k];
                        // add on the side of Most Significant Bit
                        newState = (stateToAdd << oldWidth) | state;
                        _amplitudes[newState] = _amplitudes[state];
                        // remove old state:
                        _amplitudes.Remove(state);
                    }
                }
                else
                {
                    var states = _amplitudes.Keys.ToArray();
                    var length = states.Length;
                    var zeroExist = newAmplitudes.TryGetValue(0, out var zeroAmplitude);
                    if (zeroExist) newAmplitudes[0] = 1;
                    foreach (var pair in newAmplitudes)
                        for (var k = 0; k < length; k++)
                        {
                            var state = states[k];
                            // add on the side of Most Significant Bit
                            newState = (pair.Key << oldWidth) | state;
                            _amplitudes[newState] = _amplitudes[state] * pair.Value;
                        }

                    if (zeroExist)
                        for (var k = 0; k < length; k++)
                        {
                            var state = states[k];
                            _amplitudes[state] = _amplitudes[state] * zeroAmplitude;
                        }
                    else
                        for (var k = 0; k < length; k++)
                        {
                            var state = states[k];
                            // remove old state:
                            _amplitudes.Remove(state);
                        }
                }
            }
        }

        private Dictionary<ulong, Complex> RemoveAmplitudes()
        {
            var tmp = _amplitudes;
            _amplitudes = null;
            return tmp;
        }

        private List<Register> RemoveChildren()
        {
            var tmp = _childRegisters;
            _childRegisters = null;
            return tmp;
        }

        private void AppendToRoot(Register root)
        {
            Root = root.Root;
            OffsetToRoot += root.Width;
            _random = null;
        }

        private void DetachRoot()
        {
            Root = null;
        }

        internal void Delete()
        {
            if (Root != this)
            {
                Root.Delete(this);
            }
            else
            {
                _amplitudes = null;
                foreach (var child in _childRegisters) child.DetachRoot();
                _childRegisters = null;
            }

            Root = null;
            OffsetToRoot = -1;
            Width = 0;
            _random = null;
        }

        private void Delete(Register toDelete)
        {
            // before deletion, register must be measured
            // to remove any entanglements
            var measured = toDelete.Measure();

            var offsetToRemove = toDelete.OffsetToRoot;
            var widthToRemove = toDelete.Width;

            var newChildren = new List<Register>();

            foreach (var reg in _childRegisters)
                // all childs included in register being deleted
                // are also to delete
                if (offsetToRemove <= reg.OffsetToRoot &&
                    offsetToRemove + widthToRemove >= reg.OffsetToRoot + reg.Width)
                {
                    reg.DetachRoot();
                }
                else if (offsetToRemove + widthToRemove <= reg.OffsetToRoot)
                {
                    // all registers neither included nor containing register toDelete
                    // but their OffsetToRoot must be updated
                    reg.OffsetToRoot -= widthToRemove;
                    newChildren.Add(reg);
                }
                else if (offsetToRemove >= reg.OffsetToRoot &&
                         offsetToRemove + widthToRemove <= reg.OffsetToRoot + reg.Width)
                {
                    // all childs containing register toDelete
                    // their Width must be updated
                    reg.Width -= widthToRemove;

                    var reg1 = reg;
                    var existTheSame =
                        newChildren.Any(x => x.OffsetToRoot == reg1.OffsetToRoot && x.Width == reg1.Width);
                    if (!existTheSame) newChildren.Add(reg);
                }
                else
                {
                    newChildren.Add(reg);
                }

            _childRegisters = null;
            _childRegisters = newChildren;

            Width -= toDelete.Width;

            var states = _amplitudes.Keys.ToArray();

            // if one of old states could be also a new state,
            // we must change amplitudes in order
            if (measured << offsetToRemove < (ulong)1 << Width) Array.Sort(states);

            var length = states.Length;
            for (var k = 0; k < length; k++)
            {
                var state = states[k];

                var rightBits = state % ((ulong)1 << offsetToRemove);
                var leftBits = state >> (offsetToRemove + widthToRemove);
                var newState = (leftBits << offsetToRemove) | rightBits;
                _amplitudes[newState] = _amplitudes[state];
                if (newState != state) _amplitudes.Remove(state);
            }
        }

        private IReadOnlyDictionary<ulong, double> GetProbabilities(Register child)
        {
            ulong mask = 0;
            var offset = child.OffsetToRoot;
            for (var i = 0; i < child.Width; i++)
            {
                var positionMask = (ulong)1 << (i + offset);
                mask |= positionMask;
            }

            var probabilities = new Dictionary<ulong, double>(_amplitudes.Count);
            foreach (var pair in _amplitudes)
            {
                var maskedState = (pair.Key & mask) >> offset;
                probabilities.TryGetValue(maskedState, out var old);
                probabilities[maskedState] = old + Math.Pow(pair.Value.Magnitude, 2);
            }

            return probabilities;
        }

        private IReadOnlyDictionary<ulong, Complex> GetAmplitudes(Register child)
        {
            ulong mask = 0;
            var offset = child.OffsetToRoot;
            for (var i = 0; i < child.Width; i++)
            {
                var positionMask = (ulong)1 << (i + offset);
                mask |= positionMask;
            }

            var negatedMask = ~mask;
            var negMaskedState = _amplitudes.First().Key & negatedMask;

            var toReturn = new Dictionary<ulong, Complex>(_amplitudes.Count);
            foreach (var pair in _amplitudes)
            {
                var maskedState = (pair.Key & mask) >> offset;

                // foreach check, if the remaining qubits are always the same
                var negMasked = pair.Key & negatedMask;
                if (negMasked != negMaskedState) return null;
                toReturn[maskedState] = pair.Value;
            }

            return new ReadOnlyDictionary<ulong, Complex>(toReturn);
        }

        private void Reset(Register child, ulong oldValue, ulong newValue = 0)
        {
            var xorMask = (oldValue ^ newValue) << child.OffsetToRoot;

            var states = _amplitudes.Keys.ToArray();
            var length = states.Length;
            for (var k = 0; k < length; k++)
            {
                var state = states[k];
                var newState = state ^ xorMask;
                _amplitudes[newState] = _amplitudes[state];
                _amplitudes.Remove(state);
            }
        }

        private ulong Measure(Register child)
        {
            ulong mask = 0;
            var offset = child.OffsetToRoot;
            for (var i = 0; i < child.Width; i++)
            {
                var positionMask = (ulong)1 << (i + offset);
                mask |= positionMask;
            }

            // TODO limit ?
            var randomDouble = _random.NextDouble();

            // probabilities of possible measurement results
            var probs = GetProbabilities(child);

            ulong measured = 0;
            foreach (var pair in probs)
            {
                randomDouble -= pair.Value;
                measured = pair.Key;
                if (0 >= randomDouble) break;
            }

            var states = _amplitudes.Keys.ToArray();
            var length = states.Length;
            for (var k = 0; k < length; k++)
            {
                var state = states[k];
                if ((state & mask) >> offset == measured)
                    _amplitudes[state] /= Math.Sqrt(probs[measured]);
                else
                    _amplitudes.Remove(state);
            }

            return measured;
        }

        #endregion // Internal methods

        #region Public methods

        /// <summary>
        ///     Returns the integer value stored in register, if there is only one possibility.
        ///     If the register is a superposition of multiple states, the method returns null.
        /// </summary>
        /// <returns>
        ///     The nonnegative integer value stored in register, if there is only one possibility.
        ///     Null, if the register is a superposition of multiple states.
        /// </returns>
        public ulong? GetValue()
        {
            var probabilities = GetProbabilities();
            if (probabilities.Count == 1) return probabilities.Keys.First();
            return null;
        }

        /// <summary>
        ///     Returns probabilities of each possible state of register.
        ///     It means that after measurement register remains in particular state with corresponding probability.
        /// </summary>
        /// <returns>Dictionary, where key is possible state and value is the probability of that state.</returns>
        public IReadOnlyDictionary<ulong, double> GetProbabilities()
        {
            if (Root != this) return Root.GetProbabilities(this);
            var probabilities = new Dictionary<ulong, double>(_amplitudes.Count);
            foreach (var pair in _amplitudes) probabilities[pair.Key] = Math.Pow(pair.Value.Magnitude, 2);
            return probabilities;
        }

        /// <summary>
        ///     <para>
        ///         Returns amplitudes of each possible state of register.
        ///         Amplitude is a complex number, which squared magnitude is the probability of the state.
        ///     </para>
        ///     <para>
        ///         If register is entangled, the amplitudes cannot be computed.
        ///         Thus, the method GetAmplitudes() returns null for such registers.
        ///         The method returns meaningful value, if register is RootRegister, or if it was not a part of multi-register
        ///         operations (see <see cref="N:Quantum.Operations" />).
        ///         Even if the register is not entangled with any other, but was used in multi-register operation, the returned
        ///         value is null because such operation connect participating registers into one RootRegister.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para>
        ///         Dictionary, where key is a possible state, and value is amplitude of that state.
        ///     </para>
        ///     <para>
        ///         Null, if register is a part of other register (e.g. RootRegister, after participating in multi-register
        ///         operation).
        ///     </para>
        /// </returns>
        public IReadOnlyDictionary<ulong, Complex> GetAmplitudes()
        {
            return Root != this ? Root.GetAmplitudes(this) : new ReadOnlyDictionary<ulong, Complex>(_amplitudes);
        }

        /// <summary>
        ///     Returns a vector of amplitudes of each possible state.
        ///     If register is a part of other register, this method returns null (see <see cref="Register.GetAmplitudes()" />).
        /// </summary>
        /// <seealso cref="Register.GetAmplitudes()" />
        /// <returns>
        ///     An array of complex numbers representing amplitudes of each register's state, or null (see
        ///     <see cref="Register.GetAmplitudes()" />.
        /// </returns>
        public Complex[] GetVector()
        {
            IReadOnlyDictionary<ulong, Complex> tmpAmpl = _amplitudes;
            if (Root != this) tmpAmpl = Root.GetAmplitudes(this);
            if (tmpAmpl == null) return null;
            var statesCount = (ulong)1 << Width;
            var vector = new Complex[statesCount];
            for (ulong i = 0; i < statesCount; i++)
                if (tmpAmpl.TryGetValue(i, out var tmp))
                    vector[i] = tmp;
                else
                    vector[i] = 0;
            return vector;
        }

        public void Reset(ulong newValue = 0)
        {
            if (Root != this)
            {
                var measured = Measure();
                if (measured != newValue) Root.Reset(this, measured, newValue);
                return;
            }

            _amplitudes.Clear();
            _amplitudes[newValue] = 1;
        }

        /// <summary>
        ///     Performs measurement of whole register.
        /// </summary>
        /// <returns>Measured state of the register.</returns>
        public ulong Measure()
        {
            if (Root != this)
                //ulong pow2 = 1;
                //ulong sum = 0;
                //byte result;
                //for (int i = 0; i < this.Width; i++)
                //{
                //    result = Root.Measure(OffsetToRoot + i);
                //    sum += result * pow2;
                //    pow2 *= 2;
                //}
                //return sum;
                return Root.Measure(this);
            var randomDouble = _random.NextDouble();
            double sum = 0;
            ulong measured = 0;
            foreach (var state in _amplitudes.Keys)
            {
                sum += Math.Pow(_amplitudes[state].Magnitude, 2);
                measured = state;
                if (sum >= randomDouble) break;
            }

            _amplitudes.Clear();
            _amplitudes[measured] = 1;
            return measured;
        }

        /// <summary>
        ///     Performs measurement of single qubit in register.
        ///     After measurement, the width of the register remains the same.
        /// </summary>
        /// <param name="position">The position (offset) of qubit to measure. Position = 0 indicates the Least Significant Bit.</param>
        /// <returns>0 or 1 - the measured value of the qubit.</returns>
        public byte Measure(int position)
        {
            if (Root != this) return Root.Measure(position + OffsetToRoot);

            // TODO limit ?

            var randomDouble = _random.NextDouble();

            var positionMask = (ulong)1 << position;
            double zeroProbability = 0;
            double oneProbability = 0;

            // sum up the probability of 0
            foreach (var state in _amplitudes.Keys)
                if ((state & positionMask) == 0)
                    zeroProbability += Math.Pow(_amplitudes[state].Magnitude, 2);
                else
                    oneProbability += Math.Pow(_amplitudes[state].Magnitude, 2);

            byte measured = 0;
            if (randomDouble > zeroProbability) measured = 1;
            var states = _amplitudes.Keys.ToArray();
            var length = states.Length;
            for (var k = 0; k < length; k++)
            {
                var state = states[k];
                if ((state & positionMask) == 0)
                {
                    if (measured == 0)
                        _amplitudes[state] /= Math.Sqrt(zeroProbability);
                    else
                        _amplitudes.Remove(state);
                }
                else
                {
                    if (measured == 0)
                        _amplitudes.Remove(state);
                    else
                        _amplitudes[state] /= Math.Sqrt(oneProbability);
                }
            }

            return measured;
        }

        /// <summary>
        ///     <para>
        ///         Performs a controlled not operation (C-NOT Gate).
        ///         The target bit gets inverted if the control bit is enabled.
        ///         The operation can be written as the unitary operation matrix:
        ///     </para>
        ///     <img src="../img/imgCNot.gif" />
        /// </summary>
        /// <param name="target">The position of target qubit in register (0 indicates the Least Significant Bit).</param>
        /// <param name="control">The position of control qubit in register (0 indicates the Least Significant Bit).</param>
        public void CNot(int target, int control)
        {
            if (Root != this)
            {
                Root.CNot(target + OffsetToRoot, control + OffsetToRoot);
                return;
            }

            SigmaX(target, control);
        }

        /// <summary>
        ///     <para>
        ///         Applies Toffoli gate. If all of the control bits are enabled, the target bit gets inverted.
        ///         This gate with more than two control bits is not considered elementary and is not available on all physical
        ///         realizations of a quantum computer.
        ///         Toffoli gate with two control bits can be represented by unitary matrix:
        ///     </para>
        ///     <img src="../img/imgToffoli.gif" />
        /// </summary>
        /// <example>
        ///     <code>
        ///  namespace QuantumConsole
        /// {
        ///     public class QuantumTest
        ///     {
        ///         public static void Main()
        ///         {
        /// 			QuantumComputer comp = QuantumComputer.GetInstance();
        ///             
        /// 			Register x = comp.NewRegister(1, 3);
        /// 			
        /// 			Console.WriteLine("Register x: \n{0}", x);
        /// 			
        /// 			x.Toffoli(2, 0, 1);
        /// 			
        /// 			Console.WriteLine("Register x after Toffoli(2, 0, 1): \n{0}", x);
        /// 			
        /// 			Register y = comp.NewRegister(7, 4);
        /// 			
        /// 			Console.WriteLine("Register y: \n{0}", y);
        /// 			
        /// 			y.Toffoli(3, 0, 1, 2);
        /// 			
        /// 			Console.WriteLine("Register y after Toffoli(3, 0, 1, 2): \n{0}", y);		
        ///         }
        ///     }
        /// }
        ///  </code>
        /// </example>
        /// <param name="target">The position of target qubit in register (0 indicates the Least Significant Bit).</param>
        /// <param name="controls">The positions of control qubits in register (0 indicates the Least Significant Bit).</param>
        public void Toffoli(int target, params int[] controls)
        {
            if (Root != this)
            {
                for (var i = 0; i < controls.Length; i++) controls[i] += OffsetToRoot;
                Root.Toffoli(target + OffsetToRoot, controls);
                return;
            }

            if (controls.Length < 2) throw new ArgumentException("Too few control bits");

            var controlsLength = controls.Length;
            var done = new HashSet<ulong>();
            var states = _amplitudes.Keys.ToArray();
            var length = states.Length;
            for (var k = 0; k < length; k++)
            {
                var state = states[k];
                if (done.Contains(state)) continue;
                // Flip the target bit of a basis state if all control bits are set
                var i = 0;
                while (i < controlsLength && (state & ((ulong)1 << controls[i])) != 0) i++;

                if (i != controlsLength) continue;
                //state ^= ((ulong)1 << target);
                var reversedTargetState = state ^ ((ulong)1 << target);

                var t = _amplitudes[state];

                var reversedTargetStateExist =
                    _amplitudes.TryGetValue(reversedTargetState, out var tnot);

                _amplitudes[reversedTargetState] = t;

                if (reversedTargetStateExist)
                {
                    _amplitudes[state] = tnot;
                    done.Add(reversedTargetState);
                }
                else
                {
                    _amplitudes.Remove(state);
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Performs a Sigma X Pauli's Gate on target qubit. Actually, it is a simple Not.
        ///         The unitary operation matrix is:
        ///     </para>
        ///     <img src="../img/imgSigmaX.gif" />
        /// </summary>
        /// <param name="target">The position of target qubit in register (0 indicates the Least Significant Bit).</param>
        /// <param name="control">
        ///     Optional argument. If given, the method performs controlled Sigma X. This argument destribes the
        ///     control qubit's position.
        /// </param>
        public void SigmaX(int target, int? control = null)
        {
            if (Root != this)
            {
                Root.SigmaX(target + OffsetToRoot, control + OffsetToRoot);
                return;
            }

            //TODO throw in every control gate
            if (control == target) throw new ArgumentException("Target and control bits are the same");

            var done = new HashSet<ulong>();
            var states = _amplitudes.Keys.ToArray();
            var length = states.Length;
            for (var k = 0; k < length; k++)
            {
                var state = states[k];
                if (done.Contains(state)) continue;
                //determine if the control of the basis state is set or not needed
                var controlIsSet = control == null || (state & ((ulong)1 << control.Value)) != 0;

                // Flip the target bit of a basis state if the control bit is set
                if (!controlIsSet) continue;
                var t = _amplitudes[state];
                //state ^= ((ulong)1 << target);
                var reversedTargetState = state ^ ((ulong)1 << target);

                var reversedTargetStateExist =
                    _amplitudes.TryGetValue(reversedTargetState, out var tnot);

                _amplitudes[reversedTargetState] = t;

                if (reversedTargetStateExist)
                {
                    _amplitudes[state] = tnot;
                    done.Add(reversedTargetState);
                }
                else
                {
                    _amplitudes.Remove(state);
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Performs a Sigma Y Pauli's Gate on target qubit.
        ///         The operation is represented by unitary matrix:
        ///     </para>
        ///     <img src="../img/imgSigmaY.gif" />
        /// </summary>
        /// <param name="target">The position of target qubit in register (0 indicates the Least Significant Bit).</param>
        /// <param name="control">
        ///     Optional argument. If given, the method performs controlled Sigma Y. This argument destribes the
        ///     control qubit's position.
        /// </param>
        public void SigmaY(int target, int? control = null)
        {
            if (Root != this)
            {
                Root.SigmaY(target + OffsetToRoot, control + OffsetToRoot);
                return;
            }

            var done = new HashSet<ulong>();
            var states = _amplitudes.Keys.ToArray();
            var length = states.Length;
            for (var k = 0; k < length; k++)
            {
                var state = states[k];
                if (done.Contains(state)) continue;
                //determine if the control of the basis state is set or not needed
                var controlIsSet = control == null || (state & ((ulong)1 << control.Value)) != 0;

                if (!controlIsSet) continue;
                //state ^= ((ulong)1 << target);
                var reversedTargetState = state ^ ((ulong)1 << target);

                var t = _amplitudes[state];

                var reversedTargetStateExist =
                    _amplitudes.TryGetValue(reversedTargetState, out var tnot);
                if (reversedTargetStateExist)
                {
                    // Flip the target bit of each basis state and multiply with +/- i
                    if ((reversedTargetState & ((ulong)1 << target)) != 0)
                    {
                        _amplitudes[reversedTargetState] = t * Complex.ImaginaryOne;
                        _amplitudes[state] = -tnot * Complex.ImaginaryOne;
                    }
                    else
                    {
                        _amplitudes[reversedTargetState] = -t * Complex.ImaginaryOne;
                        _amplitudes[state] = tnot * Complex.ImaginaryOne;
                    }

                    done.Add(reversedTargetState);
                }
                else
                {
                    // Flip the target bit of each basis state and multiply with +/- i
                    if ((reversedTargetState & ((ulong)1 << target)) != 0)
                        _amplitudes[reversedTargetState] = t * Complex.ImaginaryOne;
                    else
                        _amplitudes[reversedTargetState] = -t * Complex.ImaginaryOne;
                    _amplitudes.Remove(state);
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Performs a Sigma Z Pauli's Gate on target qubit.
        ///         The operation is represented by unitary matrix:
        ///     </para>
        ///     <img src="../img/imgSigmaZ.gif" />
        /// </summary>
        /// <param name="target">The position of target qubit in register (0 indicates the Least Significant Bit).</param>
        /// <param name="control">
        ///     Optional argument. If given, the method performs controlled Sigma Z. This argument destribes the
        ///     control qubit's position.
        /// </param>
        public void SigmaZ(int target, int? control = null)
        {
            if (Root != this)
            {
                Root.SigmaZ(target + OffsetToRoot, control + OffsetToRoot);
                return;
            }

            var states = _amplitudes.Keys.ToArray();
            var length = states.Length;
            for (var k = 0; k < length; k++)
            {
                var state = states[k];

                //determine if the control of the basis state is set or not needed
                var controlIsSet = control == null || (state & ((ulong)1 << control.Value)) != 0;
                if (!controlIsSet) continue;
                // Multiply with -1 if the target bit is set
                if ((state & ((ulong)1 << target)) != 0) _amplitudes[state] *= -1;
            }
        }

        /// <summary>
        ///     <para>
        ///         Performs any arbitrary unitary operation on target qubit. The operation is described by unitary matrix of
        ///         complex numbers, as follows:
        ///     </para>
        ///     <img src="../img/imgU.gif" />
        /// </summary>
        /// <param name="matrix">The 2x2 matrix of complex numbers, which describes the unitary operation.</param>
        /// <param name="target">The position of target qubit in register (0 indicates the Least Significant Bit).</param>
        /// <param name="control">
        ///     Optional argument. If given, the method performs controlled unitary operation. This argument
        ///     destribes the control qubit's position.
        /// </param>
        public void Gate1(Complex[,] matrix, int target, int? control = null)
        {
            if (Root != this)
            {
                Root.Gate1(matrix, target + OffsetToRoot, control + OffsetToRoot);
                return;
            }

            if (matrix.GetLength(0) != 2 || matrix.GetLength(1) != 2)
                throw new ArgumentException("Matrix is not 2x2", nameof(matrix));
            var limit = 1.0 / ((ulong)1 << Width) * QuantumComputer.Epsilon;

            var done = new HashSet<ulong>();
            var states = _amplitudes.Keys.ToArray();
            var length = states.Length;

            for (var i = 0; i < length; i++)
            {
                var state = states[i];
                if (done.Contains(state)) continue;
                //determine if the control of the basis state is set or not needed
                var controlIsSet = control == null || (state & ((ulong)1 << control.Value)) != 0;
                if (!controlIsSet) continue;
                //determine if the target of the basis state is set 
                var targetIsSet = (state & ((ulong)1 << target)) != 0;

                var t = _amplitudes[state];
                var reversedTargetState = state ^ ((ulong)1 << target);

                var reversedTargetStateExist =
                    _amplitudes.TryGetValue(reversedTargetState, out var tnot);

                Complex newT;
                Complex newTnot;
                if (targetIsSet)
                {
                    newT = matrix[1, 0] * tnot + matrix[1, 1] * t;
                    newTnot = matrix[0, 0] * tnot + matrix[0, 1] * t;

                    // if new amplitudes are extremely small, remove states
                    if (Math.Pow(newT.Magnitude, 2) < limit)
                        _amplitudes.Remove(state);
                    else
                        _amplitudes[state] = newT;
                    if (Math.Pow(newTnot.Magnitude, 2) < limit)
                        _amplitudes.Remove(reversedTargetState);
                    else
                        _amplitudes[reversedTargetState] = newTnot;
                }
                else
                {
                    newT = matrix[0, 0] * t + matrix[0, 1] * tnot;
                    newTnot = matrix[1, 0] * t + matrix[1, 1] * tnot;
                    // if new amplitudes are extremely small, remove states
                    if (Math.Pow(newT.Magnitude, 2) < limit)
                        _amplitudes.Remove(state);
                    else
                        _amplitudes[state] = newT;
                    if (Math.Pow(newTnot.Magnitude, 2) < limit)
                        _amplitudes.Remove(reversedTargetState);
                    else
                        _amplitudes[reversedTargetState] = newTnot;
                }

                if (reversedTargetStateExist) done.Add(reversedTargetState);
            }
        }

        /// <summary>
        ///     <para>
        ///         Applies the Hadamard gate to the target qubit.
        ///         The unitary matrix for this operation is:
        ///     </para>
        ///     <img src="../img/imgH.gif" />
        /// </summary>
        /// <param name="target">The position of target qubit in register (0 indicates the Least Significant Bit).</param>
        /// <param name="control">
        ///     Optional argument. If given, the method applies a controlled Hadamard gate. This argument
        ///     destribes the control qubit's position.
        /// </param>
        public void Hadamard(int target, int? control = null)
        {
            if (Root != this)
            {
                Root.Hadamard(target + OffsetToRoot, control + OffsetToRoot);
                return;
            }

            var a = new Complex(Math.Sqrt(1.0 / 2.0), 0);
            Complex[,] m = { { a, a }, { a, -a } };
            Gate1(m, target, control);
        }

        /// <summary>
        ///     <para>
        ///         Performs the Square Root of Not on the target qubit.
        ///         The Square Root of Not Gate is such a gate, that applied twice, performs Not operation.
        ///         The operation can be represented as the unitary matrix:
        ///     </para>
        ///     <img src="../img/imgSqrtX.gif" />
        /// </summary>
        /// <param name="target">The position of target qubit in register (0 indicates the Least Significant Bit).</param>
        /// <param name="control">
        ///     Optional argument. If given, the method performs a controlled Square root of Not. This argument
        ///     destribes the control qubit's position.
        /// </param>
        public void SqrtX(int target, int? control = null)
        {
            if (Root != this)
            {
                Root.SqrtX(target + OffsetToRoot, control + OffsetToRoot);
                return;
            }

            var a = new Complex(1.0, 0.0) / new Complex(1.0, 1.0);
            Complex[,] m = { { a, a * Complex.ImaginaryOne }, { a * Complex.ImaginaryOne, a } };
            Gate1(m, target, control);
        }

        /// <summary>
        ///     <para>
        ///         Performs a rotation of the target qubit about the x-axis of the Bloch sphere.
        ///         The angle of rotation is given in first argument (double gamma).
        ///         The unitary matrix of this operation is:
        ///     </para>
        ///     <img src="../img/imgRx.gif" />
        /// </summary>
        /// <param name="gamma">The angle of rotation.</param>
        /// <param name="target">The position of target qubit in register (0 indicates the Least Significant Bit).</param>
        /// <param name="control">
        ///     Optional argument. If given, the method performs a controlled rotation. This argument destribes
        ///     the control qubit's position.
        /// </param>
        public void RotateX(double gamma, int target, int? control = null)
        {
            if (Root != this)
            {
                Root.RotateX(gamma, target + OffsetToRoot, control + OffsetToRoot);
                return;
            }

            var m = new Complex[2, 2];
            m[0, 0] = Math.Cos(gamma / 2.0);
            m[0, 1] = -Complex.ImaginaryOne * Math.Sin(gamma / 2.0);
            m[1, 0] = -Complex.ImaginaryOne * Math.Sin(gamma / 2.0);
            m[1, 1] = Math.Cos(gamma / 2.0);
            Gate1(m, target, control);
        }

        /// <summary>
        ///     <para>
        ///         Performs a rotation of the target qubit about the y-axis of the Bloch sphere.
        ///         The angle of rotation is given in first argument (double gamma).
        ///         The operation is represented by the unitary matrix:
        ///     </para>
        ///     <img src="../img/imgRy.gif" />
        /// </summary>
        /// <param name="gamma">The angle of rotation.</param>
        /// <param name="target">The position of target qubit in register (0 indicates the Least Significant Bit).</param>
        /// <param name="control">
        ///     Optional argument. If given, the method performs a controlled rotation. This argument destribes
        ///     the control qubit's position.
        /// </param>
        public void RotateY(double gamma, int target, int? control = null)
        {
            if (Root != this)
            {
                Root.RotateY(gamma, target + OffsetToRoot, control + OffsetToRoot);
                return;
            }

            var m = new Complex[2, 2];
            m[0, 0] = Math.Cos(gamma / 2.0);
            m[0, 1] = -Math.Sin(gamma / 2.0);
            m[1, 0] = Math.Sin(gamma / 2.0);
            m[1, 1] = Math.Cos(gamma / 2.0);
            Gate1(m, target, control);
        }

        /// <summary>
        ///     <para>
        ///         Performs a rotation of the target qubit about the z-axis of the Bloch sphere.
        ///         The angle of rotation is given in first argument (double gamma).
        ///         The operation is represented by the unitary matrix:
        ///     </para>
        ///     <img src="../img/imgRz.gif" />
        /// </summary>
        /// <param name="gamma">The angle of rotation.</param>
        /// <param name="target">The position of target qubit in register (0 indicates the Least Significant Bit).</param>
        /// <param name="control">
        ///     Optional argument. If given, the method performs a controlled rotation. This argument destribes
        ///     the control qubit's position.
        /// </param>
        public void RotateZ(double gamma, int target, int? control = null)
        {
            if (Root != this)
            {
                Root.RotateZ(gamma, target + OffsetToRoot, control + OffsetToRoot);
                return;
            }

            var z = Complex.Exp(Complex.ImaginaryOne * (gamma / 2.0));
            var divZ = Complex.Exp(-Complex.ImaginaryOne * (gamma / 2.0));
            var states = _amplitudes.Keys.ToArray();
            var length = states.Length;
            for (var k = 0; k < length; k++)
            {
                var state = states[k];

                //determine if the control of the basis state is set or not needed
                var controlIsSet = control == null || (state & ((ulong)1 << control.Value)) != 0;
                if (!controlIsSet) continue;
                // Multiply if the target bit is set
                if ((state & ((ulong)1 << target)) != 0)
                    _amplitudes[state] *= z;
                else
                    _amplitudes[state] *= divZ;
            }
        }

        /// <summary>
        ///     <para>
        ///         Adds a global phase on the register's state.
        ///         The operation is represented by the unitary matrix:
        ///     </para>
        ///     <img src="../img/imgTheta.gif" />
        /// </summary>
        /// <param name="gamma">The phase that is added to the register's state.</param>
        /// <param name="target">The position of target qubit in register (0 indicates the Least Significant Bit).</param>
        /// <param name="control">
        ///     Optional argument. If given, the method performs a controlled phase scale operation. This
        ///     argument destribes the control qubit's position.
        /// </param>
        public void PhaseScale(double gamma, int target, int? control = null)
        {
            if (Root != this)
            {
                Root.PhaseScale(gamma, target + OffsetToRoot, control + OffsetToRoot);
                return;
            }

            if (control.HasValue)
            {
                var m = new Complex[2, 2];
                var z = Complex.Exp(Complex.ImaginaryOne * gamma);
                m[0, 0] = z;
                m[0, 1] = Complex.Zero;
                m[1, 0] = Complex.Zero;
                m[1, 1] = z;
                Gate1(m, target, control);
            }
            else
            {
                var z = Complex.Exp(Complex.ImaginaryOne * gamma);
                var states = _amplitudes.Keys.ToArray();
                var length = states.Length;
                for (var k = 0; k < length; k++)
                {
                    var state = states[k];
                    _amplitudes[state] *= z;
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Performs a phase kick (or phase shift) on the the register's state.
        ///         The operation is represented by the unitary matrix:
        ///     </para>
        ///     <img src="../img/imgR.gif" />
        /// </summary>
        /// <param name="gamma">The phase value.</param>
        /// <param name="target">The position of target qubit in register (0 indicates the Least Significant Bit).</param>
        /// <param name="controls">
        ///     Optional argument. If given, the method performs a controlled phase shift operation. This
        ///     argument destribes the control qubit's position.
        /// </param>
        public void PhaseKick(double gamma, int target, params int[] controls)
        {
            if (Root != this)
            {
                for (var i = 0; i < controls.Length; i++) controls[i] += OffsetToRoot;
                Root.PhaseKick(gamma, target + OffsetToRoot, controls);
                return;
            }

            var z = Complex.Exp(Complex.ImaginaryOne * gamma);
            var states = _amplitudes.Keys.ToArray();
            var length = states.Length;
            var controlsLength = controls.Length;
            for (var k = 0; k < length; k++)
            {
                var state = states[k];

                // Multiply if the target bit is set
                if ((state & ((ulong)1 << target)) == 0) continue;
                var i = 0;
                if (controlsLength > 0)
                    while (i < controlsLength && (state & ((ulong)1 << controls[i])) != 0)
                        i++;
                if (controlsLength == 0 || (controlsLength > 0 && i == controlsLength)) _amplitudes[state] *= z;
            }
        }

        /// <summary>
        ///     <para>
        ///         Performs a conditional phase kick (or phase shift) on the register's state by the angle PI / 2 ^ dist.
        ///         The operation is represented by the unitary matrix:
        ///     </para>
        ///     <img src="../img/imgRk.gif" />
        /// </summary>
        /// <param name="dist">Value of k in the angle of phase shift. The angle equals: PI / 2 ^ k.</param>
        /// <param name="target">The position of target qubit in register (0 indicates the Least Significant Bit).</param>
        /// <param name="controls">The positions of control qubits in register (0 indicates the Least Significant Bit).</param>
        public void CPhaseShift(int dist, int target, params int[] controls)
        {
            if (Root != this)
            {
                for (var i = 0; i < controls.Length; i++) controls[i] += OffsetToRoot;
                Root.CPhaseShift(dist, target + OffsetToRoot, controls);
                return;
            }

            var controlsLength = controls.Length;
            var z = Complex.Exp(Complex.ImaginaryOne * Math.PI / ((ulong)1 << Math.Abs(dist)));
            var states = _amplitudes.Keys.ToArray();
            var length = states.Length;
            for (var k = 0; k < length; k++)
            {
                var state = states[k];
                // Multiply if the target and control bits are set
                if ((state & ((ulong)1 << target)) == 0) continue;
                var i = 0;
                if (controlsLength > 0)
                    while (i < controlsLength && (state & ((ulong)1 << controls[i])) != 0)
                        i++;
                if (controlsLength == 0 || (controlsLength > 0 && i == controlsLength)) _amplitudes[state] *= z;
            }
        }

        /// <summary>
        ///     <para>
        ///         Performs a inversed conditional phase kick (or phase shift) on the register's state by the angle PI / 2 ^ dist.
        ///         The operation is represented by the unitary matrix:
        ///     </para>
        ///     <img src="../img/imgInvRk.gif" />
        /// </summary>
        /// <param name="dist">Value of k in the angle of phase shift. The angle equals: - PI / 2 ^ k.</param>
        /// <param name="target">The position of target qubit in register (0 indicates the Least Significant Bit).</param>
        /// <param name="controls">The position of control qubits in register.</param>
        public void InverseCPhaseShift(int dist, int target, params int[] controls)
        {
            if (Root != this)
            {
                for (var i = 0; i < controls.Length; i++) controls[i] += OffsetToRoot;
                Root.InverseCPhaseShift(dist, target + OffsetToRoot, controls);
                return;
            }

            var controlsLength = controls.Length;
            var z = Complex.Exp(-Complex.ImaginaryOne * Math.PI / ((ulong)1 << Math.Abs(dist)));
            var states = _amplitudes.Keys.ToArray();
            var length = states.Length;
            for (var k = 0; k < length; k++)
            {
                var state = states[k];
                // Multiply if the target and control bits are set
                if ((state & ((ulong)1 << target)) == 0) continue;
                var i = 0;
                if (controlsLength > 0)
                    while (i < controlsLength && (state & ((ulong)1 << controls[i])) != 0)
                        i++;
                if (controlsLength == 0 || i == controlsLength) _amplitudes[state] *= z;
            }
        }

        /// <summary>
        ///     Overrides native ToString() method, for friendly printing Register's content.
        /// </summary>
        /// <returns>Formatted string representing register. Prints its possible states and their probabilities.</returns>
        public override string ToString()
        {
            if (Root == null) return null;
            var buf = new StringBuilder();
            IFormatProvider formatter = new ComplexFormatter();

            var probabilities = GetProbabilities();
            var tmpAmpl = GetAmplitudes();
            var max = (ulong)(1 << Width);
            for (ulong i = 0; i < max; i++)
            {
                if (!probabilities.TryGetValue(i, out var prob)) continue;
                if (tmpAmpl != null && tmpAmpl.TryGetValue(i, out var amplitude))
                    buf.Append(string.Format(formatter, "{0:I5} ", amplitude));
                else
                    buf.Append("                    ");
                var tmp = "|" + i + ">";
                buf.Append($"{tmp,-12}");
                buf.Append($"{(float)prob,-14}").Append("|");

                for (var j = Width - 1; j >= 0; j--)
                {
                    if (j % 4 == 3) buf.Append(" ");
                    buf.Append((((ulong)1 << j) & i) >> j);
                }

                buf.AppendLine(">");
            }

            return buf.ToString();
        }

        #endregion
    }
}