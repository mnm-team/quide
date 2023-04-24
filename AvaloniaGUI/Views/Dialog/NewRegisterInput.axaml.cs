#region

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

#endregion

namespace AvaloniaGUI.Views.Dialog;

public partial class NewRegisterInput : UserControl
{
    public NewRegisterInput()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}