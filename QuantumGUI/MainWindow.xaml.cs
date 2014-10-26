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

using Microsoft.Win32;
using QuIDE.Controls;
using QuIDE.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Technewlogic.WpfDialogManagement;

namespace QuIDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HomeVM _dataContext;

        public DialogManager DialogManager
        {
            get;
            private set;
        }

        public MainWindow()
        {
            CultureInfo current = CultureInfo.CurrentCulture;
            CultureInfo invariant = CultureInfo.InvariantCulture;

            CultureInfo myCulture = new CultureInfo(current.Name);
            myCulture.NumberFormat = invariant.NumberFormat;

            Thread.CurrentThread.CurrentCulture = myCulture;
            Thread.CurrentThread.CurrentUICulture = myCulture;

            _dataContext = new HomeVM();
            InitializeComponent();
            DialogManager = new DialogManager(this, Dispatcher);          
            LayoutRoot.DataContext = _dataContext;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool canClose = _dataContext.Window_Closing();
            if (!canClose)
            {
                e.Cancel = true;
            }
        }

        private void compositeTool_Checked(object sender, RoutedEventArgs e)
        {
            if (cb.SelectedValue != null)
            {
                tb.Visibility = System.Windows.Visibility.Hidden;
                compositeTool.IsChecked = true;
                HomeVM.SelectedComposite = cb.SelectedValue as string;
                _dataContext.SelectAction("Composite");
            }
            else
            {
                compositeTool.IsChecked = false;
                _dataContext.SelectAction("Pointer");
            }
        }

        private void compositeTool_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb.SelectedValue != null)
            {
                compositeTool.IsChecked = true;
                HomeVM.SelectedComposite = cb.SelectedValue as string;
                _dataContext.SelectAction("Composite");
            }
            
        }

        private void Always_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Cut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _dataContext.CutGates(null);
        }

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _dataContext.CopyGates(null);
        }

        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _dataContext.PasteGates(null);
        }

        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _dataContext.DeleteGates(null);
        }

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _dataContext.New(null);
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _dataContext.Open(null);
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _dataContext.Save(null);
        }

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _dataContext.SaveAs(null);
        }

        private void Restart_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _dataContext.Restart(null);
        }

        private void PrevStep_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _dataContext.PrevStep(null);
        }

        private void NextStep_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _dataContext.NextStep(null);
        }

        private void RunToEnd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _dataContext.RunToEnd(null);
        }

        private void RadioButton_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                rb.IsChecked = true;
                _dataContext.SelectAction(rb.CommandParameter);
            }
        }

        private void next_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _dataContext.NextStep(null);
        }

        private void restart_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _dataContext.Restart(null);
        }

        private void prev_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _dataContext.PrevStep(null);
        }

        private void run_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _dataContext.RunToEnd(null);
        }

        private void clear_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _dataContext.ClearCircuit(null);
        }

        private void cut_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _dataContext.CutGates(null);
        }

        private void copy_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _dataContext.CopyGates(null);
        }

        private void paste_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _dataContext.PasteGates(null);
        }

        private void group_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _dataContext.MakeComposite(null);
        }

        private void Calculator_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _dataContext.ShowCalculator();
        }

        private void About_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _dataContext.ShowAbout();
        }

        private void delete_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _dataContext.DeleteGates(null);
        }

        private void generateCode_Click(object sender, RoutedEventArgs e)
        {
            _dataContext.GenerateCode(null);
        }
    }
}
