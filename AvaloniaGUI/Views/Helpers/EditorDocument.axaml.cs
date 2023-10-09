using Avalonia.Controls;
using AvaloniaGUI.ViewModels.Helpers;

namespace AvaloniaGUI.Views.Helpers;

public partial class EditorDocument : UserControl
{
    public EditorDocument()
    {
        InitializeComponent();
    }

    private void SyntaxModeCombo_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // TODO: no effect
        var comboBox = (ComboBox)sender!;
        comboBox.SelectedValue = ((EditorDocumentViewModel)DataContext!).SelectedLanguage;
    }
}