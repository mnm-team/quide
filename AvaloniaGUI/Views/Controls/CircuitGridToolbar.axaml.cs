using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaGUI.ViewModels;

namespace AvaloniaGUI.Views.Controls;

public partial class CircuitGridToolbar : UserControl
{
    public CircuitGridToolbar()
    {
        InitializeComponent();
    }

    private void CompositeTool_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        MainWindowViewModel.CompositeSelected();
    }

    private void CompositeSelectionFocused(object sender, GotFocusEventArgs e)
    {
        if (e.Source is not ComboBox comboBox) return;

        this.FindControl<RadioButton>("compositeTool").IsChecked = true;

        if (comboBox.SelectedItem is null) return;

        MainWindowViewModel.CompositeSelected();
    }
}