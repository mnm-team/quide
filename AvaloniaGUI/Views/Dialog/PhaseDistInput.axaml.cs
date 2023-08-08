#region

using Avalonia.Controls;
using AvaloniaGUI.ViewModels.Dialog;

#endregion

namespace AvaloniaGUI.Views.Dialog;

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