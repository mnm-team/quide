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

namespace QuantumParser.Validation
{
    public class MatrixValidator
    {
        public static bool IsUnitary2x2(Complex[,] matrix)
        {
            double epsilon = Quantum.QuantumComputer.Epsilon;

            if(matrix == null ||
                matrix.GetLength(0) != 2 ||
                matrix.GetLength(1) != 2)
            {
                return false;
            }

            bool isUnitary = false;

            Complex[,] conjugate = new Complex[2, 2];
            conjugate[0, 0] = Complex.Conjugate(matrix[0, 0]);
            conjugate[0, 1] = Complex.Conjugate(matrix[1, 0]);
            conjugate[1, 0] = Complex.Conjugate(matrix[0, 1]);
            conjugate[1, 1] = Complex.Conjugate(matrix[1, 1]);

            Complex[,] con_x_mat = new Complex[2, 2];
            Complex[,] mat_x_con = new Complex[2, 2];

            con_x_mat[0, 0] = conjugate[0, 0] * matrix[0, 0] + conjugate[0, 1] * matrix[1, 0];
            con_x_mat[0, 1] = conjugate[0, 0] * matrix[0, 1] + conjugate[0, 1] * matrix[1, 1];
            con_x_mat[1, 0] = conjugate[1, 0] * matrix[0, 0] + conjugate[1, 1] * matrix[1, 0];
            con_x_mat[1, 1] = conjugate[1, 0] * matrix[0, 1] + conjugate[1, 1] * matrix[1, 1];

            mat_x_con[0, 0] = matrix[0, 0] * conjugate[0, 0] + matrix[0, 1] * conjugate[1, 0];
            mat_x_con[0, 1] = matrix[0, 0] * conjugate[0, 1] + matrix[0, 1] * conjugate[1, 1];
            mat_x_con[1, 0] = matrix[1, 0] * conjugate[0, 0] + matrix[1, 1] * conjugate[1, 0];
            mat_x_con[1, 1] = matrix[1, 0] * conjugate[0, 1] + matrix[1, 1] * conjugate[1, 1];

            if ((con_x_mat[0, 0] - 1).Magnitude < epsilon &&
                (con_x_mat[1, 1] - 1).Magnitude < epsilon &&
                (con_x_mat[0, 1]).Magnitude < epsilon &&
                (con_x_mat[1, 0]).Magnitude < epsilon &&
                (mat_x_con[0, 0] - 1).Magnitude < epsilon &&
                (mat_x_con[1, 1] - 1).Magnitude < epsilon &&
                (mat_x_con[0, 1]).Magnitude < epsilon &&
                (mat_x_con[1, 0]).Magnitude < epsilon)
            {
                isUnitary = true;
            }
            else
            {
                isUnitary = false;
            }
            return isUnitary;
        }
    }
}
