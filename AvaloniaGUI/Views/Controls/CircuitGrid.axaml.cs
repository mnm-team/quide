using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaGUI.Views.Controls;

public partial class CircuitGrid : UserControl
{
    public CircuitGrid()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}