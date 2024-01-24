#region

#endregion

#region

using System.Collections.Generic;
using System.Reflection;
using QuIDE.CodeHelpers;
using QuIDE.ViewModels.MainModels.QuantumModel;

#endregion

namespace QuIDE.ViewModels.Dialog;

public class CompositeInputViewModel : ViewModelBase
{
    private readonly Dictionary<string, List<MethodInfo>> _extensionGates;
    private readonly ComputerModel _model;
    private string _name = string.Empty;

    private bool _nameValid;

    public CompositeInputViewModel(Dictionary<string, List<MethodInfo>> extensionGates, ComputerModel model)
    {
        _extensionGates = extensionGates;
        _model = model;
    }

    public CompositeInputViewModel()
    {
    }

    [CompositeName]
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            ValidateName();
        }
    }

    public string ValidationMessage => _nameValid || string.IsNullOrEmpty(Name)
        ? string.Empty
        : "Another composite gate with the same name already exist. Please choose other name.";

    private void ValidateName()
    {
        if (string.IsNullOrEmpty(Name)) return;

        // Name not already existing
        _nameValid = !(_extensionGates.ContainsKey(Name) || _model.FindComposite(Name) is not null);
        DialogInputValid = _nameValid;

        OnPropertyChanged(nameof(ValidationMessage));
    }
}