#region

using System;
using AvaloniaGUI.CodeHelpers;

#endregion

namespace AvaloniaGUI.ViewModels.Dialog;

public class PhaseDistInputViewModel : ViewModelBase
{
    private string _distText = String.Empty;

    public int? Dist
    {
        get
        {
            int dist;
            if (int.TryParse(_distText, out dist))
            {
                return dist;
            }

            return null;
        }
    }

    [IntegerType]
    public string DistText
    {
        get { return _distText; }
        set
        {
            _distText = value;
            OnPropertyChanged("DistText");
        }
    }
}