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
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Quantum
{
    /// <summary>
    /// The main class for quantum computation.
    /// <remarks>
    /// <para>
    /// QuantumComputer is a singleton, which instance can be obtained by calling <see cref="QuantumComputer.GetInstance"/> method.
    /// QuantumComputer can create and delete the <see cref="Register"/> class (Registers cannot be created independently by <b>new</b> operator).
    /// Quantum computations can be performed by <see cref="Register"/> class, but also by QuantumComputer class, by using its extension methods located in namespace <see cref="N:Quantum.Operations"/>.
    /// </para>
    /// </remarks>
    /// </summary>
    public class QuantumComputer : IQuantumComputer
    {
        private static QuantumComputer _instance;

        private Random _random;

        /// <summary>
        /// Constant value of Epsilon. This is a threshold for double values. 
        /// It is used mainly in performing quantum gates operation.
        /// When the absolute amplitude value (real or imaginary) during computations reach the value smaller than Epsilon, it is rounded to 0.
        /// </summary>
        public const double Epsilon = 1e-6;

        private QuantumComputer()
        {
            _random = new Random();
        }

        /// <summary>
        /// Method for obtaining QuantumComputer's singleton instance.
        /// </summary>
        /// <returns>QuantumComputer object.</returns>
        public static QuantumComputer GetInstance()
        {
            if (_instance == null)
            {
                _instance = new QuantumComputer();
            }
            return _instance;
        }

        /// <summary>
        /// Creates new <see cref="Register"/>, with given width and initial state.
        /// </summary>
        /// <param name="initval">Unsigned long integer, which binary representation is the initial configuration of qubits in new register.</param>
        /// <param name="width">The number of qubits in newly created register.</param>
        /// <param name="size">
        /// Optional argument. Describes size of data structure for storing all possible register states and their amplitudes.
        /// That inner data structure dynamically resizes itself, but if it could be very inefficient, this argument enables to reserve enough memory in advance.
        /// </param>
        /// <returns>Newly created register.</returns>
        public Register NewRegister(ulong initval, int width, int? size = null)
        {
            return new Register(initval, width, _random, size);
        }

        /// <summary>
        /// Creates new <see cref="Register"/>, with given width and initial states with their amplitudes.
        /// Very useful, when there is a need for creating register in nontrivial state, with some possible values.
        /// </summary>
        /// <param name="initStates">Dictionary, where keys represents states, and values their amplitudes.</param>
        /// <param name="width">The number of qubits in new register.</param>
        /// <returns>Newly created register.</returns>
        public Register NewRegister(IDictionary<ulong, Complex> initStates, int width)
        {
            if (initStates == null || initStates.Count == 0)
            {
                return new Register(0, width, _random);
            }
            else
            {
                return new Register(initStates, width, _random);
            }          
        }

        /// <summary>
        /// Deletes register, given in argument, with its references, which enables freeing the memory by garbage collector.
        /// Registers consume large amount of memory, so this method is very useful when computations are complex and requires many registers.
        /// </summary>
        /// <param name="register">Register to delete. After deletion, this parameter becomes null.</param>
        public void DeleteRegister(ref Register register)
        {
            register.Delete();
            register = null;
        }

        /// <summary>
        /// Connects given registers into one root register and returns that root.
        /// If registers was already connected, returns simply their root register.
        /// It is strongly required before doing any operation on qubits in different registers.
        /// This method is used at the begin of every operation from namespace <see cref="N:Quantum.Operations"/>, as these methods usually operate on more than one register.
        /// </summary>
        /// <param name="refs">Registers or references to single qubits. Registers could be passed here, because they are implicitly casted to RegisterRef type.</param>
        /// <returns>Root register, that contains connected registers given in arguments.</returns>
        public Register GetRootRegister(params RegisterRef[] refs)
        {
            if (refs.Length == 0)
            {
                return null;
            }
            else if (refs.Length == 1)
            {
                return refs[0].Register.Root;
            }
            else if (refs.Length == 2)
            {
                if (refs[0].Register.Root != refs[1].Register.Root)
                {
                    Register newRoot = new Register(_random);
                    newRoot.AddRegister(refs[1].Register);
                    newRoot.AddRegister(refs[0].Register);

                    return newRoot;        
                }
                else
                {
                    return refs[0].Register.Root;
                }
            }
            else
            {
                Register firstRoot = GetRootRegister(refs[0], refs[1]);
                RegisterRef[] newParams = new RegisterRef[refs.Length - 1];
                newParams[0] = firstRoot;
                Array.Copy(refs, 2, newParams, 1, refs.Length - 2);
                return GetRootRegister(newParams);
            }
        }

        /// <summary>
        /// Returns the tensor product of given registers. It has the same result as connecting registers into one in the method <see cref="QuantumComputer.GetRootRegister"/>.
        /// However, tensor product could be obtained only when given arguments are independent registers (neither connected with nor being a part of any other register).
        /// </summary>
        /// <param name="r1">First register</param>
        /// <param name="r2">Second register</param>
        /// <returns>Tensor product, where second register's qubits are least significant bits. Null, if argments are not independent registers.</returns>
        public Register TensorProduct(Register r1, Register r2)
        {
            if (r1 == r1.Root && r2 == r2.Root)
            {
                return GetRootRegister(r1, r2);
            }
            else
            {
                return null;
            }
        }
    }
}
