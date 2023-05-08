#region

using System.ComponentModel;
using System.Globalization;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaGUI.ViewModels;

#endregion

namespace AvaloniaGUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        CultureInfo current = CultureInfo.CurrentCulture;
        CultureInfo invariant = CultureInfo.InvariantCulture;

        CultureInfo myCulture = new CultureInfo(current.Name)
        {
            NumberFormat = invariant.NumberFormat
        };

        Thread.CurrentThread.CurrentCulture = myCulture;
        Thread.CurrentThread.CurrentUICulture = myCulture;

        InitializeComponent();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        // TODO: handle window closing
        bool canClose = true; //MainWindowViewModel.Window_Closing();
        if (!canClose)
        {
            e.Cancel = true;
        }
    }

    private void compositeTool_Checked(object sender, RoutedEventArgs e)
    {
        if (cb.SelectedItem != null)
        {
            // TODO: fix this
            tb.IsVisible = true; //System.Windows.IsVisible.Hidden;
            compositeTool.IsChecked = true;
            MainWindowViewModel.SelectedComposite = cb.SelectedItem as string;
            // MainWindowViewModel.SelectAction("Composite");
        }
        else
        {
            compositeTool.IsChecked = false;
            //MainWindowViewModel.SelectAction("Pointer");
        }
    }

    private void compositeTool_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBox cb = sender as ComboBox;
        if (cb.SelectedItem != null)
        {
            compositeTool.IsChecked = true;
            MainWindowViewModel.SelectedComposite = cb.SelectedItem as string;
            //MainWindowViewModel.SelectAction("Composite");
        }
    }

    // private void RadioButton_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    // {
    //     RadioButton rb = sender as RadioButton;
    //     if (rb != null)
    //     {
    //         rb.IsChecked = true;
    //         MainWindowViewModel.SelectAction(rb.CommandParameter);
    //     }
    // }
}