#region

using System.Collections.Generic;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;

#endregion

namespace QuIDE.CodeHelpers;

public static class SimpleDialogHandler
{
    /// <summary>
    ///     Custom message box.
    /// </summary>
    /// <param name="title">Title</param>
    /// <param name="msg">Message</param>
    /// <param name="icon">Icon look up <see cref="Icon" /> enum for available options other then default error.</param>
    public static async void ShowSimpleMessage(string msg = "An error occurred. Please contact the developer.",
        string title = "Error", Icon icon = Icon.Error)
    {
        var messageBoxWindow = MessageBoxManager.GetMessageBoxCustom(
            new MessageBoxCustomParams
            {
                ButtonDefinitions = new List<ButtonDefinition>
                {
                    new() { Name = "OK", IsDefault = true }
                },
                ContentTitle = title,
                ContentMessage = msg,
                Icon = icon,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                MaxWidth = 500,
                MaxHeight = 800,
                SizeToContent = SizeToContent.WidthAndHeight,
                ShowInCenter = true,
                Topmost = false
            });

        await messageBoxWindow.ShowAsync();
    }
}