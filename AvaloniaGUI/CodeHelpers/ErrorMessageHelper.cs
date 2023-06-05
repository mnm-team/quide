#region

using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;

#endregion

namespace AvaloniaGUI.CodeHelpers;

public static class ErrorMessageHelper
{
    /// <summary>
    /// Custom message box
    /// </summary>
    /// <param name="title">Title</param>
    /// <param name="msg">Message</param>
    /// <param name="icon">Icon look up <see cref="Icon"/> enum for available options other then default error.</param>
    public static void ShowMessage(string msg = "An error occurred. Please contact the developer.",
        string title = "Error", Icon icon = Icon.Error)
    {
        var messageBoxWindow =
            MessageBoxManager.GetMessageBoxStandardWindow(title, msg, ButtonEnum.Ok,
                icon);

        messageBoxWindow.Show();
    }
}