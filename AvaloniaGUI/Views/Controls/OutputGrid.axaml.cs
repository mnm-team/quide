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
    public OutputGrid()
    {
        InitializeComponent();
    }

    private void statesList_GotFocus(object sender, GotFocusEventArgs e)
    {
        //TODO: statesList null? so this.FindControl?
        if (statesList.SelectedItem != null)
        {
            OutputGridViewModel vm = DataContext as OutputGridViewModel;
            vm.SelectedIndex = statesList.SelectedIndex;
        }
    }

    private void registerBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            OutputGridViewModel vm = DataContext as OutputGridViewModel;
            try
            {
                vm.SetRegister(registerBox.SelectedItem.ToString());
                statesList.Focus();
            }
            catch (Exception ex)
            {
                ErrorMessageHelper.ShowError(ex.Message);
            }
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
            ErrorMessageHelper.ShowError(ex.Message);
        }
    }

    private void SortValue_Click(object sender, RoutedEventArgs e)
    {
        OutputGridViewModel vm = DataContext as OutputGridViewModel;
        vm.Sort(SortField.Value);
    }

    private void SortProbability_Click(object sender, RoutedEventArgs e)
    {
        OutputGridViewModel vm = DataContext as OutputGridViewModel;
        vm.Sort(SortField.Probability);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}