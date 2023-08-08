#region

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

#endregion

namespace AvaloniaGUI.Views.Controls.Custom;

public class GroupBox : TemplatedControl
{
    public static readonly StyledProperty<string> HeaderProperty = AvaloniaProperty.Register<GroupBox, string>(
        nameof(Header));

    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly StyledProperty<IControl> ContentProperty = AvaloniaProperty.Register<GroupBox, IControl>(
        nameof(Content));

    [Content]
    public IControl Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
}