#region

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaGUI.ViewModels.Dialog;

#endregion

namespace AvaloniaGUI.Views.Dialog;

public partial class ParametricInput : UserControl
{
    private readonly ParametricInputViewModel _dataContext;

    // mandatory for xaml file to initialize
    public ParametricInput()
    {
    }

    public ParametricInput(ParametricInputViewModel vm) : this()
    {
        DataContext = vm;
        _dataContext = vm;

        InitializeComponent();

        methodBox.SelectedIndex = 0;
    }

    private void methodBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // mandatory
        if (_dataContext is null || methodBox is null) return;

        // when gateBox_SelectionChanged was called just before, methodBox.SelectedIndex gets set to -1 and
        // therefore non usable as initializer for _dataContext.MethodIndex.PopulateParams()
        if (methodBox.SelectedIndex < 0)
        {
            _dataContext.MethodIndex = 0;
            return;
        }

        _dataContext.MethodIndex = methodBox.SelectedIndex;
    }

    private void gateBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // mandatory
        if (_dataContext is null || gateBox is null) return;

        _dataContext.GateIndex = gateBox.SelectedIndex;
        _dataContext.MethodIndex = 0;
    }

    private void addParam_Click(object sender, RoutedEventArgs e)
    {
        _dataContext.AddParam();
    }

    private void ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_dataContext is null) return;

        _dataContext.DialogInputValid = _dataContext.IsValid;
    }
}