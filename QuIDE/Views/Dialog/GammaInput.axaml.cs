﻿#region

using Avalonia.Controls;
using QuIDE.ViewModels.Dialog;

#endregion

namespace QuIDE.Views.Dialog;

public partial class GammaInput : UserControl
{
    private readonly GammaInputViewModel _dataContext;

    // mandatory for xaml file to initialize
    public GammaInput()
    {
    }

    public GammaInput(GammaInputViewModel vm) : this()
    {
        DataContext = vm;
        _dataContext = vm;

        InitializeComponent();
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // mandatory
        if (_dataContext is null) return;

        if (e.AddedItems.Count <= 0) return;

        ComboBoxItem item = e.AddedItems[0] as ComboBoxItem;
        _dataContext.SetAngle(item.Content as string);
    }
}