#region

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

#endregion

namespace AvaloniaGUI.Views.Dialog;

public partial class CompositeInput : UserControl
{
    public CompositeInput()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        nameBox.Focus();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}