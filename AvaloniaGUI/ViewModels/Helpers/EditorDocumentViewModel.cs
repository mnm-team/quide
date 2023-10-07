namespace AvaloniaGUI.ViewModels.Helpers;

public enum DocumentType
{
    CSharp,
    QSharp
}

public class EditorDocumentViewModel : ViewModelBase
{
    private string _content;
    private DocumentType _documentType;
    private string _header;
    private bool _isModified;


    public EditorDocumentViewModel(string header, string content, DocumentType documentType = DocumentType.CSharp) :
        this()
    {
        _header = header;
        _isModified = false;
        _content = content;
    }

    public EditorDocumentViewModel()
    {
        //Avalonia designer needs this
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

    public string Content
    {
        get => _content;
        set
        {
            _content = value;
            OnPropertyChanged(nameof(Content));
        }
    }
}