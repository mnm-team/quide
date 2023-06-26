#region

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using AvaloniaGUI.Views.Dialog;

#endregion

namespace AvaloniaGUI.CodeHelpers;

public class DialogManager
{
    private readonly Window _hostWindow;

    public DialogManager(Window window)
    {
        _hostWindow = window;
    }

    public async Task ShowDialogAsync(UserControl content, Action function)
    {
        var dialog = new BasicDialogWindow(content);
        var result = await dialog.ShowDialog<DialogToken>(_hostWindow);

        if (result is DialogToken.Cancel) return;

        function();
    }
}