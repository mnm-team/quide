#region

using Avalonia.Controls;
using QuIDE.ViewModels.Dialog;

#endregion

namespace QuIDE.Views.Dialog;

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