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

using QuIDE.ViewModels;
using System;
using System.Collections.Generic;
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
    public partial class PropertiesPane : UserControl
    {
        public PropertiesPane()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PropertiesVM vm = DataContext as PropertiesVM;
            if (e.AddedItems.Count > 0)
            {
                ComboBoxItem item = e.AddedItems[0] as ComboBoxItem;
                vm.SetAngle(item.Content as string);
            }
        }

        private void a_TextChanged(object sender, TextChangedEventArgs e)
        {
            PropertiesVM vm = DataContext as PropertiesVM;

            ApplyButton.IsEnabled = !(
                a00.GetBindingExpression(TextBox.TextProperty).HasValidationError ||
                a01.GetBindingExpression(TextBox.TextProperty).HasValidationError ||
                a10.GetBindingExpression(TextBox.TextProperty).HasValidationError ||
                a11.GetBindingExpression(TextBox.TextProperty).HasValidationError ||
                vm.Matrix == null);
        }

        private void methodBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PropertiesVM vm = DataContext as PropertiesVM;
            if (e.AddedItems.Count > 0)
            {
                vm.MethodIndex = methodBox.SelectedIndex;
            }
        }

        private void addParam_Click(object sender, RoutedEventArgs e)
        {
            PropertiesVM vm = DataContext as PropertiesVM;
            vm.AddParam();
        }
    }
}
