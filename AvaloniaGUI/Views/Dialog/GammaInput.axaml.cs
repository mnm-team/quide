#region

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaGUI.ViewModels.Dialog;

#endregion

namespace AvaloniaGUI.Views.Dialog;

public partial class GammaInput : UserControl
{
    private GammaInputViewModel _dataContext;

    public GammaInput()
    {
    }

    public GammaInput(GammaInputViewModel vm) : this()
    {
        DataContext = vm;
        _dataContext = vm;

        InitializeComponent();
    }

    // protected override void OnDataContextChanged(EventArgs e)
    // {
    //     base.OnDataContextChanged(e);
    //
    //     _dataContext = DataContext as GammaInputViewModel;
    // }

    // private void UserControl_Loaded(object sender, RoutedEventArgs e)
    // {
    //     gammaBox.Focus();
    // }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // mandatory
        if (_dataContext is null) return;

        if (e.AddedItems.Count <= 0) return;

        ComboBoxItem item = e.AddedItems[0] as ComboBoxItem;
        _dataContext.SetAngle(item.Content as string);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}