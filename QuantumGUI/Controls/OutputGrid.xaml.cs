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

using QuIDE.Helpers;
using QuIDE.ViewModels;
using QuantumModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace QuIDE.Controls
{
    public partial class OutputGrid : UserControl
    {
        public OutputGrid()
        {
            InitializeComponent();
        }

        private void statesList_GotFocus(object sender, RoutedEventArgs e)
        {
            if (statesList.SelectedItem != null)
            {
                OutputGridVM vm = DataContext as OutputGridVM;
                vm.SelectedIndex = statesList.SelectedIndex;
            }
        }

        private void registerBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OutputGridVM vm = DataContext as OutputGridVM;
                try
                {
                    vm.SetRegister(registerBox.Text);
                    statesList.Focus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Invalid value typed:\n" + ex.Message,
                        "Invalid register",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void registerBox_DropDownClosed(object sender, EventArgs e)
        {
            OutputGridVM vm = DataContext as OutputGridVM;

            try
            {
                vm.SetRegister(registerBox.Text);
                statesList.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Invalid register:\n" + ex.Message,
                    "Invalid register",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SortValue_Click(object sender, RoutedEventArgs e)
        {
            OutputGridVM vm = DataContext as OutputGridVM;
            vm.Sort(SortField.Value);
        }

        private void SortProbability_Click(object sender, RoutedEventArgs e)
        {
            OutputGridVM vm = DataContext as OutputGridVM;
            vm.Sort(SortField.Probability);
        }
    }
}
