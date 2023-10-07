using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using AvaloniaGUI.ViewModels.Helpers;
using ReactiveUI;

namespace AvaloniaGUI.ViewModels.Controls;

public class EditorViewModel : ViewModelBase
{
    private static string _exampleCode = "using Quantum;\n" +
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

        GenerateDocumentCommand = ReactiveCommand.Create<(string, string)>(GenerateDocument);
        ExecuteDocumentCommand = ReactiveCommand.Create(ExecuteDocument);
        CloseDocumentCommand = ReactiveCommand.Create<string>(CloseDocument);
        NewDocumentCommand = ReactiveCommand.Create(NewDocument);
        OpenDocumentCommand = ReactiveCommand.Create(OpenDocument);
        SaveDocumentCommand = ReactiveCommand.Create<string?>(SaveDocument);
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

    public ReactiveCommand<(string, string), Unit> GenerateDocumentCommand { get; }
    public ReactiveCommand<Unit, Unit> ExecuteDocumentCommand { get; }
    public ReactiveCommand<string, Unit> CloseDocumentCommand { get; }
    public ReactiveCommand<Unit, Unit> NewDocumentCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenDocumentCommand { get; }
    public ReactiveCommand<string?, Unit> SaveDocumentCommand { get; }

    private void GenerateDocument((string, string) headerAndContent)
    {
        // TODO: check if unique
        Documents.Add(new EditorDocumentViewModel(headerAndContent.Item1, headerAndContent.Item2));
    }

    private void ExecuteDocument()
    {
        // TODO: execute selectedDocument
    }

    private void CloseDocument(string header)
    {
        var documentToRemove = Documents.FirstOrDefault(x => x.Header == header);
        if (documentToRemove != null) Documents.Remove(documentToRemove);
    }

    private void NewDocument()
    {
    }

    private void OpenDocument()
    {
    }

    private void SaveDocument(string? location)
    {
    }
}