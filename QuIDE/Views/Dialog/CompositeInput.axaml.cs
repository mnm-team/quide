#region

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QuIDE.ViewModels.Dialog;

#endregion

namespace QuIDE.Views.Dialog;

public partial class CompositeInput : UserControl
{
    private readonly CompositeInputViewModel _dataContext;

    // mandatory for xaml file to initialize
    public CompositeInput()
    {
    }

    public CompositeInput(CompositeInputViewModel vm) : this()
    {
        DataContext = vm;
        _dataContext = vm;

        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Focus();

        // TODO: doesnt want to focus to input box right away
        //nameBox = this.FindControl<TextBox>("nameBox");
        //nameBox.Focus();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}