#region

using System.ComponentModel;

#endregion

namespace AvaloniaGUI.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler == null) return;

        var e = new PropertyChangedEventArgs(propertyName);
        handler(this, e);
    }

    #endregion // INotifyPropertyChanged Members

    /// <summary>
    /// Useful for binding OK button in dialog windows
    /// </summary>
    public bool DialogInputValid { get; set; }
}