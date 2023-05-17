#region

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

#endregion

namespace AvaloniaGUI.Views.Controls;

public class GateButton : TemplatedControl
{
    public GateButton()
    {
        //null?
        this.PointerPressed += PointerPressed;
        this.AddHandler(DragDrop.DropEvent, Drop);
        this.AddHandler(DragDrop.DragEnterEvent, DragEnter);
        this.AddHandler(DragDrop.DragOverEvent, DragOver);
    }

    #region Events

    public static readonly AttachedProperty<EventHandler<PointerPressedEventArgs>> PointerPressedProperty =
        AvaloniaProperty.RegisterAttached<GateButton, EventHandler<PointerPressedEventArgs>>(
            nameof(PointerPressed), typeof(GateButton));

    public new EventHandler<PointerPressedEventArgs> PointerPressed
    {
        get => GetValue(PointerPressedProperty);
        set => SetValue(PointerPressedProperty, value);
    }

    public static readonly StyledProperty<EventHandler<DragEventArgs>> DropProperty =
        AvaloniaProperty.Register<GateButton, EventHandler<DragEventArgs>>(
            nameof(Drop));

    public EventHandler<DragEventArgs> Drop
    {
        get => GetValue(DropProperty);
        set => SetValue(DropProperty, value);
    }

    public static readonly StyledProperty<EventHandler<DragEventArgs>> DragEnterProperty =
        AvaloniaProperty.Register<GateButton, EventHandler<DragEventArgs>>(
            nameof(DragEnter));

    public EventHandler<DragEventArgs> DragEnter
    {
        get => GetValue(DragEnterProperty);
        set => SetValue(DragEnterProperty, value);
    }

    public static readonly StyledProperty<EventHandler<DragEventArgs>> DragOverProperty =
        AvaloniaProperty.Register<GateButton, EventHandler<DragEventArgs>>(
            nameof(DragOver));

    public EventHandler<DragEventArgs> DragOver
    {
        get => GetValue(DragOverProperty);
        set => SetValue(DragOverProperty, value);
    }

    #endregion

    public static readonly StyledProperty<bool> EnabledProperty = AvaloniaProperty.Register<GateButton, bool>(
        nameof(Enabled));

    public bool Enabled
    {
        get => GetValue(EnabledProperty);
        set => SetValue(EnabledProperty, value);
    }

    public new static readonly StyledProperty<RelativePoint> RenderTransformProperty =
        AvaloniaProperty.Register<GateButton, RelativePoint>(
            nameof(RenderTransform));

    public new RelativePoint RenderTransform
    {
        get => GetValue(RenderTransformProperty);
        set => SetValue(RenderTransformProperty, value);
    }

    public static readonly StyledProperty<IControl> ContentProperty = AvaloniaProperty.Register<GateButton, IControl>(
        nameof(Content));

    public IControl Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
}