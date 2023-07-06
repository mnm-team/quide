#region

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        methodBox.SelectedIndex = 0;
        // Height = ActualHeight;
        // Width = ActualWidth;
    }

    private void methodBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // mandatory
        if (_dataContext is null) return;

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


    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        gateBox = this.FindControl<ComboBox>("gateBox");
        methodBox = this.FindControl<ListBox>("methodBox");
    }

    private void ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_dataContext is null) return;

        _dataContext.DialogInputValid = _dataContext.IsValid;
    }
}