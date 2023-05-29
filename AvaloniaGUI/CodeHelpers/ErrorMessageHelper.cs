#region

using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;

#endregion

namespace AvaloniaGUI.CodeHelpers;

public static class ErrorMessageHelper
{
    public static void ShowError(string msg)
    {
        var message =
            "Invalid value typed:\n" + msg;

        var messageBoxWindow =
            MessageBoxManager.GetMessageBoxStandardWindow("Invalid register", message, ButtonEnum.Ok,
                Icon.Error);

        messageBoxWindow.Show();
    }
}