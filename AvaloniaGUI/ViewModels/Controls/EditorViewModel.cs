using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using AvaloniaGUI.CodeHelpers;
using AvaloniaGUI.ViewModels.Helpers;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaGUI.ViewModels.Controls;

public partial class EditorViewModel : ViewModelBase
{
    private const string ExampleCode = """
                                       using Quantum;
                                       using Quantum.Operations;
                                       using System;
                                       using System.Numerics;
                                       using System.Collections.Generic;

                                       namespace QuantumConsole
                                       {
                                           public class QuantumTest
                                           {
                                               public static void Main()
                                               {
                                                   QuantumComputer comp = QuantumComputer.GetInstance();
                                       
                                                   // create new register with initial value = 0, and width = 3
                                                   Register x = comp.NewRegister(0, 3);
                                       
                                                   // example: apply Hadamard Gate on qubit number 0 (least significant)
                                                   //x.Hadamard(0);
                                               }
                                           }
                                       }
                                       """;

    private readonly CodeGenerator _codeGenerator = new();

    private readonly DialogManager _dialogManager;
    private readonly Delegate _notifyMainWindowCommands;

    private int _newFilenameCount = 1;

    private EditorDocumentViewModel _selectedDocument;

    public EditorViewModel(DialogManager dialogManager, Delegate notifyMainWindowCommands) : this()
    {
        _dialogManager = dialogManager;
        _notifyMainWindowCommands = notifyMainWindowCommands;
    }

    public EditorViewModel()
    {
        //Avalonia designer needs this
    }

    public ObservableCollection<EditorDocumentViewModel> Documents { get; set; } = new();

    public EditorDocumentViewModel SelectedDocument
    {
        get => _selectedDocument;
        set
        {
            _selectedDocument = value;
            OnPropertyChanged(nameof(SelectedDocument));
            // either last remaining document was closed or the first new document was created
            if (Documents.Count <= 1) NotifyCommands();
        }
    }

    [RelayCommand]
    private async Task CloseDocument(string fileName)
    {
        var documentToRemove = Documents.FirstOrDefault(x => x.Editor.Document.FileName == fileName);
        if (documentToRemove is null)
            throw new NullReferenceException("Document to remove was not found.This should not happen.");
        if (documentToRemove.IsModified)
        {
            // unsafe to remove -> display dialog for confirmation
            var result = await _dialogManager.ShowConfirmationDialogAsync("Are you sure you want to close this file?");
            if (result is DialogToken.Cancel) return;
        }

        if (Documents.Count > 1)
        {
            // set selected document to the one before the one we are closing
            var currentIndex = Documents.IndexOf(documentToRemove);
            var priorDocument = Documents.ElementAtOrDefault(currentIndex - 1);
            SelectedDocument = priorDocument;
        }
        else
        {
            SelectedDocument = null;
        }

        RemoveDocument(documentToRemove);
    }

    private void RemoveDocument(EditorDocumentViewModel document)
    {
        document.DisposeTextMate();
        Documents.Remove(document);
    }

    [RelayCommand]
    private void NewDocument()
    {
        AddNewDocument(CreateNewFileName(), ExampleCode, true);
    }

    [RelayCommand]
    private async Task OpenDocument()
    {
        var filesToOpen = await _dialogManager.OpenFileDialog();
        if (filesToOpen.Count == 0) return;

        foreach (var storageFile in filesToOpen)
        {
            // dont open already opened files
            if(Documents.Any(x => x.Editor.Document.FileName == storageFile.Name)) continue;
            
            await using var stream = await storageFile.OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            // Reads all the content of file as a text.
            var fileContent = await streamReader.ReadToEndAsync();

            AddNewDocument(storageFile.Name, fileContent, false, storageFile.TryGetLocalPath());
        }
    }

    [RelayCommand(CanExecute = nameof(Savable))]
    private async Task SaveDocument()
    {
        if (SelectedDocument is null) return;

        var location = SelectedDocument.Location;
        
        // if file was not saved before, use saveAs to actually set location as well
        if (location is null)
        {
            await SaveDocumentAs();
            return;
        }
        
        SelectedDocument.SaveDocument();
        NotifySaveCommands();
    }

    [RelayCommand(CanExecute = nameof(SavableAs))]
    private async Task SaveDocumentAs()
    {
        if (SelectedDocument is null) return;
        var fileToSave = await _dialogManager.SaveFileDialog(SelectedDocument.Editor.Document);
        if (fileToSave is null) return;

        // update filename in editor if user changed it during save dialog
        if (fileToSave.Name != SelectedDocument.Editor.Document.FileName)
            SelectedDocument.Editor.Document.FileName = fileToSave.Name;
        SelectedDocument.SaveDocumentAs(fileToSave);
        NotifySaveCommands();
    }

    [RelayCommand(CanExecute = nameof(CanPrintExecute))]
    private void PrintDocument()
    {
        // TODO: originally not implemented
    }

    private static bool CanPrintExecute()
    {
        return false;
    }

    [RelayCommand]
    private void GenerateCode()
    {
        var code = _codeGenerator.GenerateCode();
        AddNewDocument(CreateNewFileName(), code, true);
    }

    [RelayCommand(CanExecute = nameof(SelectionAvailable))]
    private void Copy()
    {
        SelectedDocument?.CopyCommand.Execute(null);
    }

    [RelayCommand(CanExecute = nameof(SelectionAvailable))]
    private void Paste()
    {
        SelectedDocument?.PasteCommand.Execute(null);
    }

    [RelayCommand(CanExecute = nameof(SelectionAvailable))]
    private void Cut()
    {
        SelectedDocument?.CutCommand.Execute(null);
    }

    [RelayCommand(CanExecute = nameof(SelectionAvailable))]
    private void Undo()
    {
        SelectedDocument?.UndoCommand.Execute(null);
    }

    [RelayCommand(CanExecute = nameof(SelectionAvailable))]
    private void Redo()
    {
        SelectedDocument?.RedoCommand.Execute(null);
    }

    private bool SelectionAvailable()
    {
        return SelectedDocument is not null;
    }

    private bool Savable()
    {
        return SelectedDocument is not null && SelectedDocument.IsModified;
    }

    private bool SavableAs()
    {
        return SelectedDocument is not null;
    }

    private void NotifyCommands()
    {
        CopyCommand.NotifyCanExecuteChanged();
        PasteCommand.NotifyCanExecuteChanged();
        CutCommand.NotifyCanExecuteChanged();
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();

        // notify main window commands
        _notifyMainWindowCommands.DynamicInvoke();
    }

    private void NotifySaveCommands()
    {
        SaveDocumentCommand.NotifyCanExecuteChanged();
        SaveDocumentAsCommand.NotifyCanExecuteChanged();
    }

    private string CreateNewFileName()
    {
        var name = "Class" + _newFilenameCount + ".cs";
        _newFilenameCount++;
        return name;
    }

    private void AddNewDocument(string header, string content, bool isModified, string location = null)
    {
        var newDocument = new TextDocument
        {
            FileName = header,
            Text = content
        };
        var newDocumentTab = new EditorDocumentViewModel(newDocument, isModified, NotifySaveCommands, location);
        Documents.Add(newDocumentTab);
        SelectedDocument = newDocumentTab;
        NotifySaveCommands();
    }

    public async Task<bool> EditorCanClose()
    {
        var closable = Documents.All(editorDocumentViewModel => !editorDocumentViewModel.IsModified);
        if (closable) return true;

        var result = await _dialogManager.ShowConfirmationDialogAsync(
            "There are unsaved changes. Are you sure you want to exit?");
        return result is DialogToken.OK;
    }
}