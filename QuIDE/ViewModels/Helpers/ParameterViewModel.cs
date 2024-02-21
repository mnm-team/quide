#region

using System;
using System.ComponentModel;
using System.Text;
using QuIDE.QuantumModel;
using QuIDE.QuantumParser;

#endregion

namespace QuIDE.ViewModels.Helpers;

public class ParameterViewModel : ViewModelBase
{
    private string _name;

    private Type _type;
    private string _typeString;

    private object _value;
    private string _valueString;

    private bool _isValid = false;
    private string _validationMessage = "Parameter not set";

    private readonly bool _paramsArray;
    private readonly bool _nextParamArray;

    public ParameterViewModel(string name, Type type, object value = null,
        bool paramsArray = false, bool nextParamArray = false)
    {
        _name = name;

        _type = type;
        _typeString = TypeToString(_type);

        _paramsArray = paramsArray;
        _nextParamArray = nextParamArray;
        if (_nextParamArray)
        {
            _isValid = true;
            _validationMessage = null;
        }

        if (value != null)
        {
            ValueString = ValueToString(value);
        }

        OnPropertyChanged(nameof(ValidationMessage));
    }

    public string Name => _name;

    public string TypeString => _typeString;

    public string ValueString
    {
        get => _valueString;
        set
        {
            _valueString = value;
            _value = StringToValue(_valueString, _type);
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(ValueString));
            OnPropertyChanged(nameof(IsValid));
            OnPropertyChanged(nameof(ValidationMessage));
        }
    }

    public object Value => _value;

    public bool IsValid => _isValid;

    public string ValidationMessage
    {
        get
        {
            if (!_isValid)
            {
                return _name + ": " + _validationMessage;
            }

            return null;
        }
    }

    public bool VarParamsVisibility => _paramsArray;


    private string TypeToString(Type type)
    {
        string[] split = type.ToString().Split('.');
        return split[split.Length - 1];
    }

    private string ValueToString(object value)
    {
        Type type = value.GetType();
        if (type == typeof(RegisterRefModel))
        {
            RegisterRefModel rrm = (RegisterRefModel)value;
            StringBuilder sb = new StringBuilder();
            if (rrm.Register != null)
            {
                sb.Append(rrm.Register.Name);
            }
            else
            {
                sb.Append("root");
            }

            sb.Append('[').Append(rrm.Offset).Append(']');
            return sb.ToString();
        }

        if (type != typeof(RegisterPartModel)) return value.ToString();

        RegisterPartModel rm = (RegisterPartModel)value;
        if (rm.Register != null)
        {
            if (rm.Width == rm.Register.Qubits.Count)
            {
                return rm.Register.Name;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(rm.Register.Name);
                sb.Append('[').Append(rm.Offset).Append(", ").Append(rm.Width).Append(']');
                return sb.ToString();
            }
        }
        else
        {
            CircuitEvaluator eval = CircuitEvaluator.GetInstance();
            int rootWidth = eval.RootRegister.Width;
            if (rm.Width == rootWidth)
            {
                return "root";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("root");
                sb.Append('[').Append(rm.Offset).Append(", ").Append(rm.Width).Append(']');
                return sb.ToString();
            }
        }
    }

    private object StringToValue(string text, Type type)
    {
        object toReturn = null;
        try
        {
            CircuitEvaluator eval = CircuitEvaluator.GetInstance();

            if (type == typeof(RegisterRef))
            {
                toReturn = eval.ResolveRegisterRef(text, _nextParamArray);
            }
            else if (type == typeof(Register))
            {
                toReturn = eval.ResolveRegister(text, _nextParamArray);
            }
            else
            {
                TypeConverter converter = TypeDescriptor.GetConverter(type);
                toReturn = converter.ConvertFromInvariantString(text);
            }

            _isValid = true;
            _validationMessage = null;
        }
        catch (Exception ex)
        {
            _isValid = false;
            _validationMessage = ex.Message;
        }

        return toReturn;
    }
}