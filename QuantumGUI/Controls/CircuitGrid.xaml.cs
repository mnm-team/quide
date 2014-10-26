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
using QuantumModel;
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
using System.Windows.Threading;

namespace QuIDE.Controls
{
    public partial class CircuitGrid : UserControl
    {
        private Line line;
        private SolidColorBrush black = new SolidColorBrush(Colors.Black);

        public CircuitGrid()
        {
            InitializeComponent();
        }

        private void LayoutRoot_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);
            CircuitGridVM vm = DataContext as CircuitGridVM;
            vm.LayoutRoot_PreviewMouseWheel(e);
        }

        private void GateButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Control source = sender as Control;

            bool shiftPressed = false;
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                shiftPressed = true;
            }

            if (HomeVM.SelectedAction == ActionName.Control && !shiftPressed)
            {
                Button button = source as Button;
                Point coordinates = button.TransformToVisual(drawing).Transform(new Point(0, 0));

                CircuitGridVM circuitVM = DataContext as CircuitGridVM;

                double diameter = 12;
                double centerX = coordinates.X + 0.5 * CircuitGridVM.GateWidth;
                double centerY = coordinates.Y + 0.5 * CircuitGridVM.QubitSize;

                Ellipse ctrlPoint = new Ellipse();
                ctrlPoint.Width = diameter;
                ctrlPoint.Height = diameter;
                
                ctrlPoint.Fill = black;
                ctrlPoint.Stroke = black;
                ctrlPoint.StrokeThickness = 1;
                ctrlPoint.AllowDrop = true;
                ctrlPoint.Drop += ctrlPoint_Drop;

                drawing.Children.Add(ctrlPoint);
                Canvas.SetTop(ctrlPoint, centerY - 0.5 * diameter);
                Canvas.SetLeft(ctrlPoint, centerX - 0.5 * diameter);

                line = new Line();
                line.X1 = line.X2 = centerX;
                line.Y1 = line.Y2 = centerY;

                line.Stroke = black;
                line.StrokeThickness = 1;
                drawing.Children.Add(line);

                Action emptyDelegate = delegate { };
                drawing.Dispatcher.Invoke(emptyDelegate, DispatcherPriority.Render);
            }

            GateVM vm = source.DataContext as GateVM;
            Tuple<int, RegisterRefModel> data = new Tuple<int,RegisterRefModel>(vm.Column, vm.Row);
            DragDrop.DoDragDrop(source, data, DragDropEffects.All);
        }

        void ctrlPoint_Drop(object sender, DragEventArgs e)
        {
            line = null;
            drawing.Children.Clear();
        }

        private void drawing_Drop(object sender, DragEventArgs e)
        {
            line = null;
            drawing.Children.Clear();
        }

        private void GateButton_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Tuple<int, RegisterRefModel>)))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void GateButton_DragOver(object sender, DragEventArgs e)
        {
            if (line != null)
            {
                Point mouse = e.GetPosition(drawing);
                line.X2 = mouse.X - 5;
                line.Y2 = mouse.Y - 5;
            }
        }

        private void GateButton_Drop(object sender, DragEventArgs e)
        {
            Control target = sender as Control;
            Tuple<int, RegisterRefModel> data = e.Data.GetData(typeof(Tuple<int, RegisterRefModel>)) as Tuple<int, RegisterRefModel>;
            GateVM vm = target.DataContext as GateVM;

            vm.SetGate(data.Item1, data.Item2, e.KeyStates);

            line = null;
            drawing.Children.Clear();

            CircuitGridVM circuitVM = DataContext as CircuitGridVM;
            circuitVM.SelectedObject = vm;
        }

        private void GatesScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                RegisterScroll.ScrollToVerticalOffset(e.VerticalOffset);
                GatesScroll.ScrollToVerticalOffset(RegisterScroll.VerticalOffset);
            }

            // if added step
            if (e.ExtentWidthChange > 0)
            {
                CircuitGridVM circuitVM = DataContext as CircuitGridVM;
                int addedColumn = circuitVM.LastStepAdded;
                if (addedColumn > 0)
                {
                    // if newly added step is not fully visible
                    double scrollNeeded = e.ExtentWidthChange * (addedColumn + 1) - GatesScroll.HorizontalOffset - GatesScroll.ViewportWidth;
                    if (scrollNeeded > 0)
                    {
                        GatesScroll.ScrollToHorizontalOffset(GatesScroll.HorizontalOffset + scrollNeeded);
                    }
                }
            }
        }

        private void RegisterScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            GatesScroll.ScrollToVerticalOffset(e.VerticalOffset);
        }
    }
}
