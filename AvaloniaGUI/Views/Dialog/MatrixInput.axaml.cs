#region

using Avalonia.Controls;
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
}