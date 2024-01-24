#region

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaGUI.ViewModels.Controls;

#endregion

namespace AvaloniaGUI.Views.Controls;

public partial class PropertiesPane : UserControl
{
    public PropertiesPane()
    {
        InitializeComponent();
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PropertiesViewModel vm = DataContext as PropertiesViewModel;
        if (e.AddedItems.Count <= 0) return;

        ComboBoxItem item = e.AddedItems[0] as ComboBoxItem;
        vm.SetAngle(item.Content as string);
    }

    // TODO: maybe move to ViewModel? probably solved by binding to enabled property
    private void a_TextChanged(object sender, AvaloniaPropertyChangedEventArgs avaloniaPropertyChangedEventArgs)
    {
        // PropertiesViewModel vm = DataContext as PropertiesViewModel;
        //
        // ApplyButton.IsEnabled = !(
        //     a00.GetBindingExpression(TextBox.TextProperty).HasValidationError ||
        //     a01.GetBindingExpression(TextBox.TextProperty).HasValidationError ||
        //     a10.GetBindingExpression(TextBox.TextProperty).HasValidationError ||
        //     a11.GetBindingExpression(TextBox.TextProperty).HasValidationError ||
        //     vm.Matrix == null);
    }

    private void methodBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PropertiesViewModel vm = DataContext as PropertiesViewModel;
        if (e.AddedItems.Count > 0)
        {
            vm.MethodIndex = methodBox.SelectedIndex;
        }
    }

    private void addParam_Click(object sender, RoutedEventArgs e)
    {
        PropertiesViewModel vm = DataContext as PropertiesViewModel;
        vm.AddParam();
    }
}