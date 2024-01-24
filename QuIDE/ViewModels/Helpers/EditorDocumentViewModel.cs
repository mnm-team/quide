using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Indentation.CSharp;
using AvaloniaEdit.TextMate;
using QuIDE.CodeHelpers;
using CommunityToolkit.Mvvm.Input;
using TextMateSharp.Grammars;

namespace QuIDE.ViewModels.Helpers;

public partial class EditorDocumentViewModel : ViewModelBase
{
    private readonly Delegate _notifyEditorCommands;
    private string _currentlySavedText;
    private TextEditor _editor;
    private ThemeName _editorTheme;

    private bool _isModified;
    private string _location;

    private RegistryOptions _registryOptions;
    private Language _selectedLanguage;
    private TextMate.Installation _textMateInstallation;

    public EditorDocumentViewModel(TextDocument document, bool isModified, Delegate notifyEditorCommands,
        string location) :
        this()
    {
        _notifyEditorCommands = notifyEditorCommands;
        _isModified = isModified;
        _location = location;

        InitializeEditor(document);
    }

    public EditorDocumentViewModel()
    {
        //Avalonia designer needs this
    }

    public ObservableCollection<Language> Languages { get; set; }

    /// <summary>
    ///     Currently selected Language. Add more languages to <see cref="Languages" /> and implement their
    ///     TextMate installation here.
    /// </summary>
    public Language SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (value is null) return; //TODO: weirdly null whenever creating new document a second time
            if (value == _selectedLanguage) return;

            _selectedLanguage = value;
            _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(value.Id));
            OnPropertyChanged(nameof(SelectedLanguage));
        }
    }

    public bool IsModified
    {
        get => _isModified;
        set
        {
            _isModified = value;
            OnPropertyChanged(nameof(IsModified));
            // notify EditorViewModel to update its commands
            _notifyEditorCommands.DynamicInvoke();
        }
    }

    public string Location
    {
        get => _location;
        set
        {
            _location = value;
            OnPropertyChanged(nameof(Location));
        }
    }

    public TextEditor Editor
    {
        get => _editor;
        set
        {
            _editor = value;
            OnPropertyChanged(nameof(Editor));
        }
    }

    public void DisposeTextMate()
    {
        _textMateInstallation.Dispose();
    }

    private static ObservableCollection<Language> SetSupportedLanguages(RegistryOptions registryOptions)
    {
        return new ObservableCollection<Language>
        {
            registryOptions.GetLanguageByExtension(".cs") // Add C# language
        };
    }

    private void InitializeEditor(TextDocument document)
    {
        _editorTheme = ThemeName.LightPlus;
        _registryOptions = new RegistryOptions(
            _editorTheme);

        Languages = SetSupportedLanguages(_registryOptions);
        _selectedLanguage = Languages[0];

        _currentlySavedText = document.Text;

        // handler to update IsModified property
        document.UpdateFinished += (_, _) =>
        {
            // always new document
            IsModified = string.IsNullOrWhiteSpace(_location) || _currentlySavedText != document.Text;
        };

        _editor = new TextEditor
        {
            Document = document,
            ShowLineNumbers = true,
            Margin = new Thickness(5),
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            FontFamily = new FontFamily("Cascadia Code,Consolas,Menlo,Monospace"),
            FontSize = 16,
            FontWeight = FontWeight.Light,
            Background = Brushes.White,

            ContextMenu = new ContextMenu
            {
                ItemsSource = new List<MenuItem>
                {
                    new()
                    {
                        Header = "Copy", InputGesture = new KeyGesture(Key.C, KeyModifiers.Control),
                        Command = CopyCommand
                    },
                    new()
                    {
                        Header = "Paste", InputGesture = new KeyGesture(Key.V, KeyModifiers.Control),
                        Command = PasteCommand
                    },
                    new()
                    {
                        Header = "Cut", InputGesture = new KeyGesture(Key.X, KeyModifiers.Control), Command = CutCommand
                    }
                }
            },
            Options = new TextEditorOptions
            {
                ShowBoxForControlCharacters = true,
                ColumnRulerPositions = new List<int> { 80, 100 }
            }
        };

        _editor.TextArea.IndentationStrategy = new CSharpIndentationStrategy(_editor.Options);

        var csharpLanguage = _registryOptions.GetLanguageByExtension(".cs");

        _textMateInstallation = _editor.InstallTextMate(_registryOptions);
        _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(csharpLanguage.Id));
    }

    [RelayCommand]
    private void Copy()
    {
        _editor.Copy();
    }

    [RelayCommand]
    private void Paste()
    {
        _editor.Paste();
    }

    [RelayCommand]
    private void Cut()
    {
        _editor.Cut();
    }

    [RelayCommand]
    private void Undo()
    {
        _editor.Undo();
    }

    [RelayCommand]
    private void Redo()
    {
        _editor.Redo();
    }

    public async void SaveDocumentAs(IStorageFile file)
    {
        try
        {
            await using var stream = await file.OpenWriteAsync();
            await using var streamWriter = new StreamWriter(stream);
            // Write some content to the file.
            var text = Editor.Document.Text;
            await streamWriter.WriteLineAsync(text);

            _currentlySavedText = text;
            IsModified = false;
            Location = file.TryGetLocalPath();
        }
        catch (Exception e)
        {
            SimpleDialogHandler.ShowSimpleMessage(e.Message);
        }
    }

    public void SaveDocument()
    {
        if (Location is null) return;

        try
        {
            var text = Editor.Document.Text;
            File.WriteAllText(Location, text);

            _currentlySavedText = text;
            IsModified = false;
        }
        catch (Exception e)
        {
            SimpleDialogHandler.ShowSimpleMessage(e.Message);
        }
    }
}