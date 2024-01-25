#region

using System;
using System.Globalization;
using System.Threading;
using Avalonia.Controls;
using QuIDE.ViewModels;

#endregion

namespace QuIDE.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        var current = CultureInfo.CurrentCulture;
        var invariant = CultureInfo.InvariantCulture;

        var myCulture = new CultureInfo(current.Name)
        {
            NumberFormat = invariant.NumberFormat
        };

        Thread.CurrentThread.CurrentCulture = myCulture;
        Thread.CurrentThread.CurrentUICulture = myCulture;

        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        var vm = DataContext as MainWindowViewModel;
        vm?.InitializeWindow(this);
    }
}