#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using AvaloniaGUI.Views.Dialog;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;

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
    ///     Creates a dialog window with the given content and shows it. If OK is pressed, specified action will be
    ///     additionally
    ///     executed.
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

    public async Task<DialogToken> ShowConfirmationDialogAsync(string msg, string title = "Confirm?")
    {
        var dialog = MessageBoxManager.GetMessageBoxCustom(
            new MessageBoxCustomParams
            {
                ButtonDefinitions = new List<ButtonDefinition>
                {
                    new() { Name = "Yes", IsDefault = true },
                    new() { Name = "Cancel", IsCancel = true }
                },
                ContentTitle = title,
                ContentMessage = msg,
                Icon = Icon.Question,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                MaxWidth = 500,
                MaxHeight = 800,
                SizeToContent = SizeToContent.WidthAndHeight,
                ShowInCenter = true,
                Topmost = false
            });

        // result is text on clicked button
        var resultString = await dialog.ShowWindowDialogAsync(_hostWindow);

        return resultString is "Yes" ? DialogToken.OK : DialogToken.Cancel;
    }

    public async Task<IStorageFile> SaveFileDialog(TextDocument documentToSave, string defaultExtension = ".cs")
    {
        var handler = _hostWindow.StorageProvider;

        var location = await handler.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

        if (location is null) throw new FileNotFoundException("Documents folder not found.");

        var options = new FilePickerSaveOptions
        {
            SuggestedStartLocation = location,
            SuggestedFileName = documentToSave.FileName,
            DefaultExtension = defaultExtension,
            FileTypeChoices = new[]
            {
                new FilePickerFileType("C# file")
                {
                    Patterns = new[] { "*" + defaultExtension }
                }
            },
            ShowOverwritePrompt = true
        };

        return await handler.SaveFilePickerAsync(options);
    }

    public async Task<IReadOnlyList<IStorageFile>> OpenFileDialog(string defaultExtension = ".cs")
    {
        var handler = _hostWindow.StorageProvider;

        var location = await handler.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

        if (location is null) throw new FileNotFoundException("Documents folder not found.");

        // change evaluation here to support different or multiple file types
        var fileType = new FilePickerFileType("C# file")
        {
            Patterns = new[] { "*" + defaultExtension }
        };

        var options = new FilePickerOpenOptions
        {
            SuggestedStartLocation = location,
            FileTypeFilter = new[]
            {
                fileType
            },
            AllowMultiple = true
        };

        return await handler.OpenFilePickerAsync(options);
    }
}