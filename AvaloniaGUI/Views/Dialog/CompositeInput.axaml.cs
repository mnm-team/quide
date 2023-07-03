#region

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaGUI.ViewModels.Dialog;

#endregion

namespace AvaloniaGUI.Views.Dialog;

public partial class CompositeInput : UserControl
{
    private readonly CompositeInputViewModel _dataContext;

    // mandatory for xaml file to initialize
    public CompositeInput()
    {
    }

    public CompositeInput(CompositeInputViewModel vm) : this()
    {
        DataContext = vm;
        _dataContext = vm;

        InitializeComponent();
    }

    // private void UserControl_Loaded(object sender, RoutedEventArgs e)
    // {
    //     nameBox.Focus();
    // }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}