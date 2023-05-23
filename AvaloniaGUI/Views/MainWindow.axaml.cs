#region

using System.ComponentModel;
using System.Globalization;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaGUI.ViewModels;
using NP.Avalonia.UniDock;

#endregion

namespace AvaloniaGUI.Views;

public partial class MainWindow : Window
{
    private DockManager _dockManager;

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

        _dockManager = (DockManager)this.FindResource("TheDockManager")!;

        _dockManager.RestoreFromFile("DefaultLayout.xml");

        // Button _saveLayoutButton =
        //     this.FindControl<Button>("SaveLayoutButton");
        //
        // _saveLayoutButton.Click += _saveLayoutButton_Click;
    }

    /// <summary>
    /// Would save new default file under bin/.../DefaultLayout.xml
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _saveLayoutButton_Click(object? sender, RoutedEventArgs e)
    {
        _dockManager.SaveToFile("./DefaultLayout.xml");
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