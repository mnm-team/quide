#region

using System.Globalization;
using System.Threading;
using Avalonia.Controls;

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
}