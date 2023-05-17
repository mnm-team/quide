#region

using System;
using System.Collections.Generic;
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

        // perform drag drop operation for control gate
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

            //Action emptyDelegate = delegate { };
            //Drawing.Dispatcher.Invoke(emptyDelegate, DispatcherPriority.Render);
        }

        GateViewModel vm = source.DataContext as GateViewModel;

        // fetch grid with gates to draw
        IDataObject data = new Tuple<int, RegisterRefModel>(vm.Column, vm.Row) as IDataObject;
        DragDrop.DoDragDrop(e, data, DragDropEffects.Link);
    }

    void ctrlPoint_Drop(object? sender, PointerEventArgs pointerEventArgs)
    {
        line = null;
        Drawing.Children.Clear();
    }

    private void Drawing_Drop(object? sender, DragEventArgs e)
    {
        line = null;
        Drawing.Children.Clear();
    }

    private void GateButton_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.Contains(typeof(Tuple<int, RegisterRefModel>).ToString()))
        {
            e.DragEffects = DragDropEffects.Link; //All
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

    private void GateButton_Drop(object sender, DragEventArgs e)
    {
        Control target = sender as Control;

        // check for Tuple<int, RegisterRefModel>
        var dataFormat = typeof(Tuple<int, RegisterRefModel>);

        if (e.Source.GetType() != dataFormat)
            return;

        Tuple<int, RegisterRefModel> data =
            e.Source as Tuple<int, RegisterRefModel>;
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

//     void SetupDnd(string suffix, Action<DataObject> factory, DragDropEffects effects)
//         {
//             var dragMe = this.Get<Border>("DragMe" + suffix);
//             var dragState = this.Get<TextBlock>("DragState" + suffix);
//
//             async void DoDrag(object? sender, Avalonia.Input.PointerPressedEventArgs e)
//             {
//                 var dragData = new DataObject();
//                 factory(dragData);
//
//                 var result = await DragDrop.DoDragDrop(e, dragData, effects);
//                 switch (result)
//                 {
//                     case DragDropEffects.Move:
//                         dragState.Text = "Data was moved";
//                         break;
//                     case DragDropEffects.Copy:
//                         dragState.Text = "Data was copied";
//                         break;
//                     case DragDropEffects.Link:
//                         dragState.Text = "Data was linked";
//                         break;
//                     case DragDropEffects.None:
//                         dragState.Text = "The drag operation was canceled";
//                         break;
//                     default:
//                         dragState.Text = "Unknown result";
//                         break;
//                 }
//             }
//
//             void DragOver(object? sender, DragEventArgs e)
//             {
//                 if (e.Source is Control c && c.Name == "MoveTarget")
//                 {
//                     e.DragEffects = e.DragEffects & (DragDropEffects.Move);
//                 }
//                 else
//                 {
//                     e.DragEffects = e.DragEffects & (DragDropEffects.Copy);
//                 }
//
//                 // Only allow if the dragged data contains text or filenames.
//                 if (!e.Data.Contains(DataFormats.Text)
//                     && !e.Data.Contains(DataFormats.Files)
//                     && !e.Data.Contains(CustomFormat))
//                     e.DragEffects = DragDropEffects.None;
//             }
//
//             async void Drop(object? sender, DragEventArgs e)
//             {
//                 if (e.Source is Control c && c.Name == "MoveTarget")
//                 {
//                     e.DragEffects = e.DragEffects & (DragDropEffects.Move);
//                 }
//                 else
//                 {
//                     e.DragEffects = e.DragEffects & (DragDropEffects.Copy);
//                 }
//
//                 if (e.Data.Contains(DataFormats.Text))
//                 {
//                     _dropState.Text = e.Data.GetText();
//                 }
//                 else if (e.Data.Contains(DataFormats.Files))
//                 {
//                     var files = e.Data.GetFiles() ?? Array.Empty<IStorageItem>();
//                     var contentStr = "";
//
//                     foreach (var item in files)
//                     {
//                         if (item is IStorageFile file)
//                         {
//                             var content = await DialogsPage.ReadTextFromFile(file, 1000);
//                             contentStr += $"File {item.Name}:{Environment.NewLine}{content}{Environment.NewLine}{Environment.NewLine}";
//                         }
//                         else if (item is IStorageFolder folder)
//                         {
//                             var childrenCount = 0;
//                             await foreach (var _ in folder.GetItemsAsync())
//                             {
//                                 childrenCount++;
//                             }
//                             contentStr += $"Folder {item.Name}: items {childrenCount}{Environment.NewLine}{Environment.NewLine}";
//                         }
//                     }
//
//                     _dropState.Text = contentStr;
//                 }
// #pragma warning disable CS0618 // Type or member is obsolete
//                 else if (e.Data.Contains(DataFormats.FileNames))
//                 {
//                     var files = e.Data.GetFileNames();
//                     _dropState.Text = string.Join(Environment.NewLine, files ?? Array.Empty<string>());
//                 }
// #pragma warning restore CS0618 // Type or member is obsolete
//                 else if (e.Data.Contains(CustomFormat))
//                 {
//                     _dropState.Text = "Custom: " + e.Data.Get(CustomFormat);
//                 }
//             }
//
//             dragMe.PointerPressed += DoDrag;
//
//             AddHandler(DragDrop.DropEvent, Drop);
//             AddHandler(DragDrop.DragOverEvent, DragOver);
//         }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        Drawing = this.FindControl<Canvas>("drawing");
        Drawing.AddHandler(DragDrop.DropEvent, Drawing_Drop);

        AddButtonHandlers();
    }

    private void AddButtonHandlers()
    {
        var parentItemsControl = this.FindControl<ItemsControl>("stepItemsControl");

        var buttons = FindVisualChildren<Button>(parentItemsControl);

        foreach (var button in buttons)
        {
            // doesnt get hit
            button.PointerPressed += GateButton_MouseDown;
            button.AddHandler(DragDrop.DropEvent, GateButton_Drop);
            button.AddHandler(DragDrop.DragEnterEvent, GateButton_DragEnter);
            button.AddHandler(DragDrop.DragOverEvent, GateButton_DragOver);
        }
    }

    private IEnumerable<T> FindVisualChildren<T>(IControl control) where T : IControl
    {
        if (control != null)
        {
            for (int i = 0; i < control.VisualChildren.Count; i++)
            {
                if (control.VisualChildren[i] is T)
                {
                    yield return (T)control.VisualChildren[i];
                }

                foreach (var child in FindVisualChildren<T>((IControl)control.VisualChildren[i]))
                {
                    yield return child;
                }
            }
        }
    }
}