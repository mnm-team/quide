#region

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

#endregion

namespace QuIDE.Views.Controls.Custom;

public class GroupBox : TemplatedControl
{
    public static readonly StyledProperty<string> HeaderProperty = AvaloniaProperty.Register<GroupBox, string>(
        nameof(Header));

    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly StyledProperty<Control> ContentProperty = AvaloniaProperty.Register<GroupBox, Control>(
        nameof(Content));

    [Content]
    public Control Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
}