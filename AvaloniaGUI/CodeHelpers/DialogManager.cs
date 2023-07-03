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

    /// <summary>
    /// Creates a dialog window with the given content and shows it. If OK is pressed, specified action will be additionally
    /// executed.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="function"></param>
    public async Task ShowDialogAsync(UserControl content, Action function)
    {
        var dialog = new BasicDialogWindow(content);
        var result = await dialog.ShowDialog<DialogToken>(_hostWindow);

        if (result is DialogToken.Cancel) return;

        function();
    }
}