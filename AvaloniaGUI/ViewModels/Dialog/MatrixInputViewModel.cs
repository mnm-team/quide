#region

using System.Numerics;
using AvaloniaGUI.CodeHelpers;
using AvaloniaGUI.Properties;
using AvaloniaGUI.ViewModels.MainModels.QuantumParser.Validation;

#endregion

namespace AvaloniaGUI.ViewModels.Dialog;

public class MatrixInputViewModel : ViewModelBase
{
    private string _a00Text = string.Empty;
    private string _a01Text = string.Empty;
    private string _a10Text = string.Empty;
    private string _a11Text = string.Empty;

    private Complex[,] _matrix = new Complex[2, 2] { { 0, 0 }, { 0, 0 } };

    private bool _isUnitary;

    [ComplexNumber]
    public string A00Text
    {
        get => _a00Text;
        set
        {
            _a00Text = value;
            ComplexParser.TryParse(_a00Text, out _matrix[0, 0]);
            ValidateMatrix();
        }
    }

    [ComplexNumber]
    public string A01Text
    {
        get => _a01Text;
        set
        {
            _a01Text = value;
            ComplexParser.TryParse(_a01Text, out _matrix[0, 1]);
            ValidateMatrix();
        }
    }

    [ComplexNumber]
    public string A10Text
    {
        get => _a10Text;
        set
        {
            _a10Text = value;
            ComplexParser.TryParse(_a10Text, out _matrix[1, 0]);
            ValidateMatrix();
        }
    }

    [ComplexNumber]
    public string A11Text
    {
        get => _a11Text;
        set
        {
            _a11Text = value;
            ComplexParser.TryParse(_a11Text, out _matrix[1, 1]);
            ValidateMatrix();
        }
    }

    public string ValidationMessage => _isUnitary ? string.Empty : Resources.MatrixNotUnitary;

    public Complex[,] Matrix => _isUnitary ? _matrix : null;

    private void ValidateMatrix()
    {
        _isUnitary = MatrixValidator.IsUnitary2x2(_matrix);
        DialogInputValid = _isUnitary;
        OnPropertyChanged(nameof(ValidationMessage));
    }
}