#region

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaGUI.ViewModels;
using AvaloniaGUI.ViewModels.Controls;
using AvaloniaGUI.ViewModels.Helpers;
using AvaloniaGUI.ViewModels.MainModels.QuantumModel;

#endregion

namespace AvaloniaGUI.Views.Controls;

public partial class CircuitGrid : UserControl
{
    private Line line;
    private SolidColorBrush black = new SolidColorBrush(Colors.Black);
    private Canvas Drawing;

    public CircuitGrid()
    {
        InitializeComponent();
    }

    private void LayoutRoot_PreviewMouseWheel(object sender, PointerWheelEventArgs e)
    {
        //TODO: fix this
        //base.OnPreviewMouseWheel(e);
        CircuitGridViewModel vm = DataContext as CircuitGridViewModel;
        vm.LayoutRoot_PreviewMouseWheel(e);
    }

    private void GateButton_MouseDown(object sender, PointerPressedEventArgs e)
    {
        Control source = sender as Control;

        bool shiftPressed = false;
        // if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
        // {
        //     shiftPressed = true;
        // }

        if (MainWindowViewModel.SelectedAction == ActionName.Control && !shiftPressed)
        {
            Button button = source as Button;

            Point coordinates =
                new Point(0, 0).Transform(button.TransformToVisual(Drawing)
                    .GetValueOrDefault()); //.Transform(new Point(0, 0));

            CircuitGridViewModel circuitVM = DataContext as CircuitGridViewModel;

            double diameter = 12;
            double centerX = coordinates.X + 0.5 * CircuitGridViewModel.GateWidth;
            double centerY = coordinates.Y + 0.5 * CircuitGridViewModel.QubitSize;

            Ellipse ctrlPoint = new Ellipse();
            ctrlPoint.Width = diameter;
            ctrlPoint.Height = diameter;

            ctrlPoint.Fill = black;
            ctrlPoint.Stroke = black;
            ctrlPoint.StrokeThickness = 1;
            //ctrlPoint.AllowDrop = true;
            ctrlPoint.PointerReleased += ctrlPoint_Drop;

            Drawing.Children.Add(ctrlPoint);
            Canvas.SetTop(ctrlPoint, centerY - 0.5 * diameter);
            Canvas.SetLeft(ctrlPoint, centerX - 0.5 * diameter);

            line = new Line();
            line.StartPoint = new Point(centerX, centerY);
            line.EndPoint = new Point(centerX, centerX);

            line.Stroke = black;
            line.StrokeThickness = 1;
            Drawing.Children.Add(line);

            Action emptyDelegate = delegate { };
            //Drawing.Dispatcher.Invoke(emptyDelegate, DispatcherPriority.Render);
        }

        GateViewModel vm = source.DataContext as GateViewModel;
        Tuple<int, RegisterRefModel> data = new Tuple<int, RegisterRefModel>(vm.Column, vm.Row);
        //DragDrop.DoDragDrop(source, data, DragDropEffects.All);
    }

    void ctrlPoint_Drop(object? sender, PointerEventArgs pointerEventArgs)
    {
        line = null;
        Drawing.Children.Clear();
    }

    private void drawing_Drop(object? sender, PointerReleasedEventArgs pointerReleasedEventArgs)
    {
        line = null;
        Drawing.Children.Clear();
    }

    private void GateButton_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.Contains(typeof(Tuple<int, RegisterRefModel>).ToString()))
        {
            e.DragEffects = DragDropEffects.Move; //All
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void GateButton_DragOver(object sender, DragEventArgs e)
    {
        if (line != null)
        {
            Point mouse = e.GetPosition(Drawing);
            //line.X2 = mouse.X - 5;
            //line.Y2 = mouse.Y - 5;
            line.StartPoint = new Point(0, 0);
            line.EndPoint = new Point(mouse.X - 5, mouse.Y - 5);
        }
    }

    private void GateButton_Drop(object? sender, DragEventArgs e)
    {
        Control target = sender as Control;

        // check for Tuple<int, RegisterRefModel>
        var dataFormat = typeof(Tuple<int, RegisterRefModel>).ToString();

        if (!e.Data.Contains(dataFormat))
            return;

        Tuple<int, RegisterRefModel> data =
            e.Data.Get(dataFormat) as Tuple<int, RegisterRefModel>;
        GateViewModel vm = target.DataContext as GateViewModel;

        vm.SetGate(data.Item1, data.Item2, e.KeyModifiers);

        line = null;
        Drawing.Children.Clear();

        CircuitGridViewModel circuitVM = DataContext as CircuitGridViewModel;
        circuitVM.SelectedObject = vm;
    }

    private void GatesScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        /*if (e.OffsetDelta.Y != 0) //e.VerticalChange != 0)
        {
            RegisterScroll.ScrollToVerticalOffset(e.VerticalOffset);
            GatesScroll.ScrollToVerticalOffset = RegisterScroll.VerticalOffset;
        }

        // if added step
        if (e.ExtentDelta.X > 0) //e.ExtentWidthChange > 0)
        {
            CircuitGridViewModel circuitVM = DataContext as CircuitGridViewModel;
            int addedColumn = circuitVM.LastStepAdded;
            if (addedColumn > 0)
            {
                // if newly added step is not fully visible
                double scrollNeeded = e.ExtentWidthChange * (addedColumn + 1) - GatesScroll.HorizontalOffset -
                                      GatesScroll.ViewportWidth;
                if (scrollNeeded > 0)
                {
                    GatesScroll.ScrollToHorizontalOffset(GatesScroll.HorizontalOffset + scrollNeeded);
                }
            }
        }*/
    }

    private void RegisterScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        //GatesScroll.ScrollToVerticalOffset(e.VerticalOffset);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        Drawing = this.FindControl<Canvas>("drawing");
    }
}