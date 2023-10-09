using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Indentation.CSharp;
using AvaloniaEdit.TextMate;
using CommunityToolkit.Mvvm.Input;
using TextMateSharp.Grammars;

namespace AvaloniaGUI.ViewModels.Helpers;

public partial class EditorDocumentViewModel : ViewModelBase
{
    private TextEditor _editor;
    private ThemeName _editorTheme;
    private string _header;
    private bool _isModified;

    private RegistryOptions _registryOptions;
    private Language _selectedLanguage;
    private TextMate.Installation _textMateInstallation;

    public EditorDocumentViewModel(string header, string content) :
        this()
    {
        _header = header;
        _isModified = false;

        InitializeEditor(content);
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
            // TODO: after tab switching null value gets passed here -> combobox no selection
            if (value is null) return;
            if (value == _selectedLanguage) return;

            _selectedLanguage = value;
            _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(value.Id));
            OnPropertyChanged(nameof(SelectedLanguage));
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

    public string Header
    {
        get => _header;
        set
        {
            _header = value;
            OnPropertyChanged(nameof(Header));
        }
    }

    public bool IsModified
    {
        get => _isModified;
        set
        {
            _isModified = value;
            OnPropertyChanged(nameof(Header));
        }
    }

    private static ObservableCollection<Language> SetSupportedLanguages(RegistryOptions registryOptions)
    {
        return new ObservableCollection<Language>
        {
            registryOptions.GetLanguageByExtension(".cs") // Add C# language
        };
    }

    private void InitializeEditor(string content = "")
    {
        _editorTheme = ThemeName.Light;
        _registryOptions = new RegistryOptions(
            _editorTheme);

        Languages = SetSupportedLanguages(_registryOptions);
        _selectedLanguage = Languages[0];

        var editor = new TextEditor
        {
            Text = content,
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

        editor.TextArea.IndentationStrategy = new CSharpIndentationStrategy(editor.Options);

        var csharpLanguage = _registryOptions.GetLanguageByExtension(".cs");

        _textMateInstallation = editor.InstallTextMate(_registryOptions);
        _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(csharpLanguage.Id));

        _editor = editor;
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
}