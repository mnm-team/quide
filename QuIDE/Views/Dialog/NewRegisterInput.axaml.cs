#region

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaGUI.ViewModels.Dialog;

#endregion

namespace AvaloniaGUI.Views.Dialog;

public partial class NewRegisterInput : UserControl
{
    private readonly NewRegisterInputViewModel _dataContext;

    // mandatory for xaml file to initialize
    public NewRegisterInput()
    {
    }

    public NewRegisterInput(NewRegisterInputViewModel vm) : this()
    {
        DataContext = vm;
        _dataContext = vm;

        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Focus();

        widthBox = this.FindControl<TextBox>("widthBox");
        // TODO: doesnt want to focus to input box right away
        //widthBox.Focus();
        widthBox.SelectAll();
    }

    private void ValidateViewModel()
    {
        _dataContext.DialogInputValid = _dataContext.InputsValid;
    }

    private void StatesGrid_OnPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_dataContext is null) return;
        ValidateViewModel();
    }

    private void WidthBox_OnPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_dataContext is null) return;
        ValidateViewModel();
    }
}