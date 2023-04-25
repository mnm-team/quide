#region

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

#endregion

namespace AvaloniaGUI.Views.Dialog;

public partial class ParametricInput : UserControl
{
    public ParametricInput()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}