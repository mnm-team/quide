﻿#region

using QuIDE.CodeHelpers;

#endregion

namespace QuIDE.ViewModels.Dialog;

public class PhaseDistInputViewModel : ViewModelBase
{
    private string _distText = string.Empty;

    public int? Dist => int.TryParse(_distText, out var dist) ? dist : null;

    [IntegerNumber]
    public string DistText
    {
        get => _distText;
        set
        {
            _distText = value;
            OnPropertyChanged(nameof(DistText));

            var isValid = int.TryParse(value, out _);

            DialogInputValid = isValid;
        }
    }
}