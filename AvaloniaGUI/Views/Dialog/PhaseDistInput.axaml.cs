#region

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

#endregion

namespace AvaloniaGUI.Views.Dialog;

public partial class PhaseDistInput : UserControl
{
    public PhaseDistInput()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}