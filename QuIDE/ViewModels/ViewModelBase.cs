﻿#region

using System.ComponentModel;

#endregion

namespace QuIDE.ViewModels;

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

    private bool _dialogInputValid;

    /// <summary>
    /// Useful for binding OK button in dialog windows
    /// </summary>
    public bool DialogInputValid
    {
        get => _dialogInputValid;
        set
        {
            _dialogInputValid = value;
            OnPropertyChanged(nameof(DialogInputValid));
        }
    }
}