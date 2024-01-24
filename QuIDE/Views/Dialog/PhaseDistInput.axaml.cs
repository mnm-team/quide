#region

using Avalonia.Controls;
using QuIDE.ViewModels.Dialog;

#endregion

namespace QuIDE.Views.Dialog;

public partial class PhaseDistInput : UserControl
{
    // mandatory for xaml file to initialize
    public PhaseDistInput()
    {
    }

    public PhaseDistInput(PhaseDistInputViewModel vm) : this()
    {
        DataContext = vm;
        InitializeComponent();
    }
}