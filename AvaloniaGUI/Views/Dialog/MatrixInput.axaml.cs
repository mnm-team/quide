#region

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaGUI.ViewModels.Dialog;

#endregion

namespace AvaloniaGUI.Views.Dialog;

public partial class MatrixInput : UserControl
{
    // mandatory for xaml file to initialize
    public MatrixInput()
    {
    }

    public MatrixInput(MatrixInputViewModel vm) : this()
    {
        DataContext = vm;

        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}