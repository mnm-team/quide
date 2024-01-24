#region

using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using QuIDE.CodeHelpers;
using QuIDE.ViewModels.Controls;

#endregion

namespace QuIDE.Views.Controls;

public partial class OutputGrid : UserControl
{
    private OutputGridViewModel _dataContext;

    public OutputGrid()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        _dataContext = DataContext as OutputGridViewModel;
    }

    private void statesList_GotFocus(object sender, GotFocusEventArgs e)
    {
        if (statesList.SelectedItem == null) return;

        _dataContext.SelectedIndex = statesList.SelectedIndex;
    }

    private void RegisterBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            // has to be here, because on initial window loading is executed before DataContext is set.
            // After that it wont ever be null again
            //if (_dataContext is null) return;

            //_dataContext.SetRegister(registerBox.SelectedItem.ToString());
            statesList.Focus();
        }
        catch (Exception ex)
        {
            SimpleDialogHandler.ShowSimpleMessage(ex.Message);
        }
    }

    private void registerBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;

        // if key is Enter, apply selection
        var vm = DataContext as OutputGridViewModel;
        try
        {
            vm.SetRegister(registerBox.SelectedItem.ToString());
            statesList.Focus();
        }
        catch (Exception ex)
        {
            SimpleDialogHandler.ShowSimpleMessage(ex.Message);
        }
    }

    private void registerBox_DropDownClosed(object sender, EventArgs e)
    {
        var vm = DataContext as OutputGridViewModel;

        try
        {
            vm.SetRegister(registerBox.SelectedItem.ToString());
            statesList.Focus();
        }
        catch (Exception ex)
        {
            SimpleDialogHandler.ShowSimpleMessage(ex.Message);
        }
    }

    private void SortValue_Click(object sender, RoutedEventArgs e)
    {
        _dataContext.Sort(SortField.Value);
    }

    private void SortProbability_Click(object sender, RoutedEventArgs e)
    {
        _dataContext.Sort(SortField.Probability);
    }
}