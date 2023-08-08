#region

using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

#endregion

namespace AvaloniaGUI.CodeHelpers;

public static class ErrorMessageHelper
{
    /// <summary>
    /// Custom message box.
    /// </summary>
    /// <param name="title">Title</param>
    /// <param name="msg">Message</param>
    /// <param name="icon">Icon look up <see cref="Icon"/> enum for available options other then default error.</param>
    public static async void ShowMessage(string msg = "An error occurred. Please contact the developer.",
        string title = "Error", Icon icon = Icon.Error)
    {
        var messageBoxWindow =
            MessageBoxManager.GetMessageBoxStandard(title, msg, ButtonEnum.Ok,
                icon);

        await messageBoxWindow.ShowAsync();
    }
}