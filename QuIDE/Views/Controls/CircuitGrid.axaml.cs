#region

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using QuIDE.ViewModels;
using QuIDE.ViewModels.Controls;
using QuIDE.ViewModels.Helpers;
using QuIDE.ViewModels.MainModels.QuantumModel;

#endregion

namespace QuIDE.Views.Controls;

public partial class CircuitGrid : UserControl
{
    private readonly SolidColorBrush _drawingColor = new(Colors.Black);
    private Line _line;

    public CircuitGrid()
    {
        InitializeComponent();
        drawing.AddHandler(DragDrop.DropEvent, Drawing_Drop);
    }

    private void LayoutRoot_PreviewMouseWheel(object sender, PointerWheelEventArgs e)
    {
        var vm = DataContext as CircuitGridViewModel;
        vm.LayoutRoot_PreviewMouseWheel(e);
    }

    /// <summary>
    ///     Is called by PointerPressed event on GateButton.
    /// </summary>
    /// <param name="sender">GateButton</param>
    /// <param name="e">event</param>
    private void GateButton_MouseDown(object sender, PointerPressedEventArgs e)
    {
        var source = sender as Control;

        if (e.GetCurrentPoint(source).Properties.IsRightButtonPressed) return;

        var shiftPressed = e.KeyModifiers == KeyModifiers.Shift;

        var vm = source.DataContext as GateViewModel;

        // perform drawing operation for control gate
        if (MainWindowViewModel.SelectedAction == ActionName.Control && !shiftPressed)
        {
            var button = source as Button;

            var coordinates = new Point(0, 0).Transform(button.TransformToVisual(drawing)
                .GetValueOrDefault()) / (DataContext as CircuitGridViewModel).ScaleFactor;

            const double diameter = 12;

            var centerX = coordinates.X + 0.5 * CircuitGridViewModel.GateWidth;
            var centerY = coordinates.Y + 0.5 * CircuitGridViewModel.QubitSize;

            var ctrlPoint = new Ellipse
            {
                Width = diameter,
                Height = diameter,
                Fill = _drawingColor,
                Stroke = _drawingColor,
                StrokeThickness = 1
            };

            ctrlPoint.SetValue(DragDrop.AllowDropProperty, true); //AllowDrop = true;
            ctrlPoint.AddHandler(DragDrop.DropEvent, ctrlPoint_Drop);

            drawing.Children.Add(ctrlPoint);
            Canvas.SetTop(ctrlPoint, centerY - 0.5 * diameter);
            Canvas.SetLeft(ctrlPoint, centerX - 0.5 * diameter);

            _line = new Line
            {
                StartPoint = new Point(centerX, centerY),
                EndPoint = new Point(centerX, centerY),
                Stroke = _drawingColor,
                StrokeThickness = 1
            };

            drawing.Children.Add(_line);
        }

        // fetch grid with gates to draw
        var data = new Tuple<int, RegisterRefModel>(vm.Column, vm.Row);

        var dragData = new DataObject();
        dragData.Set(typeof(Tuple<int, RegisterRefModel>).ToString(), data);

        DragDrop.DoDragDrop(e, dragData, DragDropEffects.Link);
    }

    private void ctrlPoint_Drop(object sender, PointerEventArgs pointerEventArgs)
    {
        _line = null;
        drawing.Children.Clear();
    }

    private void Drawing_Drop(object sender, DragEventArgs e)
    {
        _line = null;
        drawing.Children.Clear();
    }

    private void GateButton_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.Contains(typeof(Tuple<int, RegisterRefModel>).ToString()))
        {
            e.DragEffects = DragDropEffects.Link; //All
            return;
        }

        e.DragEffects = DragDropEffects.None;
    }

    private void GateButton_DragOver(object sender, DragEventArgs e)
    {
        if (_line == null) return;

        var scaleFactor = (DataContext as CircuitGridViewModel).ScaleFactor;

        var offset = new Vector(-10, 4);

        var mouse = (e.GetPosition(drawing) + offset) / scaleFactor;

        _line.EndPoint = new Point(mouse.X, mouse.Y);
    }

    private void GateButton_Drop(object sender, DragEventArgs e)
    {
        var target = sender as Control;

        // check for Tuple<int, RegisterRefModel>
        var dataFormat = typeof(Tuple<int, RegisterRefModel>).ToString();

        if (!e.Data.Contains(dataFormat))
            return;

        var data =
            e.Data.Get(dataFormat) as Tuple<int, RegisterRefModel>;
        var vm = target.DataContext as GateViewModel;

        vm.SetGate(data.Item1, data.Item2, e.KeyModifiers);

        _line = null;
        drawing.Children.Clear();

        var circuitVM = DataContext as CircuitGridViewModel;
        circuitVM.SelectedObject = vm;
    }

    private void GatesScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        //TODO:
        // if (e.OffsetDelta.Y != 0) //e.VerticalChange != 0)
        // {
        //     RegisterScroll.ScrollToVerticalOffset(e.VerticalOffset);
        //     GatesScroll.ScrollToVerticalOffset = RegisterScroll.VerticalOffset;
        // }

        // if added step
        var extentWidthChange = e.ExtentDelta.X;
        if (!(extentWidthChange > 0)) return;

        var circuitVM = DataContext as CircuitGridViewModel;
        var addedColumn = circuitVM.LastStepAdded;

        if (addedColumn <= 0) return;

        // if newly added step is not fully visible
        var scrollNeeded = extentWidthChange * (addedColumn + 1) - GatesScroll.Offset.X -
                           GatesScroll.Viewport.Width;
        if (scrollNeeded > 0)
        {
            //GatesScroll.ScrollToHorizontalOffset(GatesScroll.HorizontalOffset + scrollNeeded);
        }
    }

    private void RegisterScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        //GatesScroll.ScrollToVerticalOffset(e.VerticalOffset);
    }
}