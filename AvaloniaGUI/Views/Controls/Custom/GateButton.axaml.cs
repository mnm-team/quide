#region

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;

#endregion

namespace AvaloniaGUI.Views.Controls.Custom;

public class GateButton : TemplatedControl
{
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var draggableButton = e.NameScope.Find<Button>("DraggableButton");

        draggableButton.AddHandler(PointerPressedEvent, MousePressed, RoutingStrategies.Tunnel);
        draggableButton.AddHandler(DragDrop.DropEvent, Drop);
        draggableButton.AddHandler(DragDrop.DragEnterEvent, DragEnter);
        draggableButton.AddHandler(DragDrop.DragOverEvent, DragOver);
    }

    #region Events

    private static readonly StyledProperty<EventHandler<PointerPressedEventArgs>> PointerPressedProperty =
        AvaloniaProperty.Register<GateButton, EventHandler<PointerPressedEventArgs>>(
            nameof(MousePressed));

    public EventHandler<PointerPressedEventArgs> MousePressed
    {
        get => GetValue(PointerPressedProperty);
        set => SetValue(PointerPressedProperty, value);
    }

    private static readonly StyledProperty<EventHandler<DragEventArgs>> DropProperty =
        AvaloniaProperty.Register<GateButton, EventHandler<DragEventArgs>>(
            nameof(Drop));

    public EventHandler<DragEventArgs> Drop
    {
        get => GetValue(DropProperty);
        set => SetValue(DropProperty, value);
    }

    private static readonly StyledProperty<EventHandler<DragEventArgs>> DragEnterProperty =
        AvaloniaProperty.Register<GateButton, EventHandler<DragEventArgs>>(
            nameof(DragEnter));

    public EventHandler<DragEventArgs> DragEnter
    {
        get => GetValue(DragEnterProperty);
        set => SetValue(DragEnterProperty, value);
    }

    private static readonly StyledProperty<EventHandler<DragEventArgs>> DragOverProperty =
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

    public static readonly StyledProperty<Control> ContentProperty = AvaloniaProperty.Register<GateButton, Control>(
        nameof(Content));

    [Content]
    public Control Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly StyledProperty<ITransform> TransformProperty =
        AvaloniaProperty.Register<GateButton, ITransform>(
            nameof(Transform));

    public ITransform Transform
    {
        get => GetValue(TransformProperty);
        set => SetValue(TransformProperty, value);
    }

    public new static readonly StyledProperty<ContextMenu> ContextMenuProperty =
        AvaloniaProperty.Register<GateButton, ContextMenu>(
            nameof(ContextMenu));

    public new ContextMenu ContextMenu
    {
        get => GetValue(ContextMenuProperty);
        set => SetValue(ContextMenuProperty, value);
    }
}