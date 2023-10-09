using Avalonia.Controls;

namespace AvaloniaGUI.Views.Controls;

public partial class Editor : UserControl
{
    public Editor()
    {
        InitializeComponent();
    }

    private void TabControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // if (DataContext is null) return;
        //
        // var selectedTab = ((TabControl)sender!).SelectedItem;
        //
        // if(selectedTab is null) return;
        // selectedTab = (TabItem)selectedTab;
        // var selectedDocument = (EditorDocumentViewModel)((TabItem)selectedTab).DataContext!;
        // ((EditorViewModel)DataContext).SelectedDocument = selectedDocument;
    }
}