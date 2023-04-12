using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaGUI.Views;

public partial class CircuitDesigner : UserControl
{
    public CircuitDesigner()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}