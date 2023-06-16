#region

using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaGUI.CodeHelpers;
using AvaloniaGUI.ViewModels.Controls;

#endregion

namespace AvaloniaGUI.Views.Controls;

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

    private void RegisterBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            // has to be here, because on initial window loading is executed before DataContext is set.
            // After that it wont ever be null again
            if (_dataContext is null) return;

            // could be simplified in future to just use binding to _dataContext._selectedRegister.Value as reactive Property
            _dataContext.SetRegister(registerBox.SelectedItem.ToString());
            statesList.Focus();
        }
        catch (Exception ex)
        {
            ErrorMessageHelper.ShowMessage(ex.Message);
        }
    }

    private void registerBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;

        // if key is Enter, apply selection
        OutputGridViewModel vm = DataContext as OutputGridViewModel;
        try
        {
            vm.SetRegister(registerBox.SelectedItem.ToString());
            statesList.Focus();
        }
        catch (Exception ex)
        {
            ErrorMessageHelper.ShowMessage(ex.Message);
        }
    }

    private void registerBox_DropDownClosed(object sender, EventArgs e)
    {
        OutputGridViewModel vm = DataContext as OutputGridViewModel;

        try
        {
            vm.SetRegister(registerBox.SelectedItem.ToString());
            statesList.Focus();
        }
        catch (Exception ex)
        {
            ErrorMessageHelper.ShowMessage(ex.Message);
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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        registerBox = this.FindControl<ComboBox>("registerBox");
        statesList = this.FindControl<DataGrid>("statesList");
    }
}