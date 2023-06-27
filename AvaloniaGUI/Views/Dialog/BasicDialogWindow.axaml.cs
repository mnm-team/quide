#region

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaGUI.CodeHelpers;
using AvaloniaGUI.ViewModels;

#endregion

namespace AvaloniaGUI.Views.Dialog;

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
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    public void OK_Clicked(object sender, RoutedEventArgs e)
    {
        Close(DialogToken.OK);
    }

    public void Cancel_Clicked(object sender, RoutedEventArgs e)
    {
        Close(DialogToken.Cancel);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        DialogContentControl = this.FindControl<ContentControl>("DialogContentControl");

        DialogContentControl.Content = _content;
    }
}