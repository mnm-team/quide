#region

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QuIDE.CodeHelpers;
using QuIDE.ViewModels;

#endregion

namespace QuIDE.Views.Dialog;

public partial class BasicDialogWindow : Window
{
    private readonly UserControl _content;

    public BasicDialogWindow()
    {
    }

    public BasicDialogWindow(UserControl content) : this()
    {
        _content = content;
        DataContext = (ViewModelBase)content.DataContext;
        InitializeComponent(); // TODO: devtools dont wire up correctly, maybe due to focus issues for keyboard

        DialogContentControl.Content = _content;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        DialogContentControl.Focus();
    }

    public void OK_Clicked(object sender, RoutedEventArgs e)
    {
        Close(DialogToken.OK);
    }

    public void Cancel_Clicked(object sender, RoutedEventArgs e)
    {
        Close(DialogToken.Cancel);
    }
}