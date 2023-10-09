using System.Collections.ObjectModel;
using System.Linq;
using AvaloniaGUI.ViewModels.Helpers;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaGUI.ViewModels.Controls;

public partial class EditorViewModel : ViewModelBase
{
    private static readonly string _exampleCode = "using Quantum;\n" +
                                                  "using Quantum.Operations;\n" +
                                                  "using System;\n" +
                                                  "using System.Numerics;\n" +
                                                  "using System.Collections.Generic;\n\n" +
                                                  "namespace QuantumConsole\n" +
                                                  "{\n" +
                                                  "\tpublic class QuantumTest\n" +
                                                  "\t{\n" +
                                                  "\t\tpublic static void Main()\n" +
                                                  "\t\t{\n" +
                                                  "\t\t\tQuantumComputer comp = QuantumComputer.GetInstance();\n\n" +
                                                  "\t\t\t// create new register with initial value = 0, and width = 3 \n" +
                                                  "\t\t\tRegister x = comp.NewRegister(0, 3);\n\n" +
                                                  "\t\t\t// example: apply Hadamard Gate on qubit number 0 (least significant) \n" +
                                                  "\t\t\t//x.Hadamard(0);\n" +
                                                  "\t\t}\n" +
                                                  "\t}\n" +
                                                  "}\n";

    private EditorDocumentViewModel? _selectedDocument;

    public EditorViewModel()
    {
        // should be initialized as in original
        Documents = new ObservableCollection<EditorDocumentViewModel>
            { new("Document 1", "Content 1"), new("Document2", "Content 2") };
        SelectedDocument = Documents[1];
    }

    public ObservableCollection<EditorDocumentViewModel> Documents { get; set; }

    public EditorDocumentViewModel? SelectedDocument
    {
        get => _selectedDocument;
        set
        {
            _selectedDocument = value;
            OnPropertyChanged(nameof(SelectedDocument));
        }
    }

    [RelayCommand]
    private void GenerateDocument((string, string) headerAndContent)
    {
        // TODO: check if unique
        Documents.Add(new EditorDocumentViewModel(headerAndContent.Item1, headerAndContent.Item2));
    }

    [RelayCommand]
    private void ExecuteDocument()
    {
        // TODO: execute selectedDocument
    }

    [RelayCommand]
    private void CloseDocument(string header)
    {
        var documentToRemove = Documents.FirstOrDefault(x => x.Header == header);
        if (documentToRemove != null) Documents.Remove(documentToRemove);
    }

    [RelayCommand]
    private void NewDocument()
    {
        var newDocument = new EditorDocumentViewModel("New Document", _exampleCode);
        Documents.Add(newDocument);
        SelectedDocument = newDocument;
    }

    [RelayCommand]
    private void OpenDocument()
    {
    }

    [RelayCommand]
    private void SaveDocument(string? location)
    {
    }

    [RelayCommand]
    private void Copy()
    {
        // TODO: doesnt want to fire and .subscribe() doesnt work without argument
        SelectedDocument?.CopyCommand.Execute(null);
    }

    [RelayCommand]
    private void Paste()
    {
        SelectedDocument?.PasteCommand.Execute(null);
    }

    [RelayCommand]
    private void Cut()
    {
        SelectedDocument?.CutCommand.Execute(null);
    }
}