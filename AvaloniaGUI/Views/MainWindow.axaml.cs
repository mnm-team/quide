#region

using System.ComponentModel;
using System.Globalization;
using System.Threading;
using Avalonia.Controls;
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

        var vm = DataContext as MainWindowViewModel;
        vm?.SetWindow(this);
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        // TODO: handle window closing
        bool canClose = true; //_dataContext.Window_Closing();
        if (!canClose)
        {
            e.Cancel = true;
        }
    }

    private void CompositeTool_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;

        vm?.CompositeSelected();
    }
}