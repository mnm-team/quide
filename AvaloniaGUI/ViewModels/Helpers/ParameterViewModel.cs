#region

using System;
using System.ComponentModel;
using System.Text;
using AvaloniaGUI.ViewModels.MainModels.QuantumModel;
using AvaloniaGUI.ViewModels.MainModels.QuantumParser;

#endregion

namespace AvaloniaGUI.ViewModels.Helpers;

public class ParameterViewModel : ViewModelBase
{
    private string _name;

    private Type _type;
    private string _typeString;

    private object _value;
    private string _valueString;

    private bool _isValid = false;
    private string _validationMessage = "Parameter not set";

    private bool _paramsArray;
    private bool _nextParamArray;

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

        OnPropertyChanged("ValidationMessage");
    }

    public string Name
    {
        get { return _name; }
    }

    public string TypeString
    {
        get { return _typeString; }
    }

    public string ValueString
    {
        get { return _valueString; }
        set
        {
            _valueString = value;
            _value = StringToValue(_valueString, _type);
            OnPropertyChanged("Value");
            OnPropertyChanged("ValueString");
            OnPropertyChanged("IsValid");
            OnPropertyChanged("ValidationMessage");
        }
    }

    public object Value
    {
        get { return _value; }
    }

    public bool IsValid
    {
        get { return _isValid; }
    }

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

    // TODO: whats this
    // https://mcraiha.github.io/xaml/wpf/avalonia/2020/03/03/Differences-in-wpf-and-avalonia.html
    public bool VarParamsVisibility
    {
        get { return _paramsArray; }
    }


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

            sb.Append("[").Append(rrm.Offset).Append("]");
            return sb.ToString();
        }

        if (type == typeof(RegisterPartModel))
        {
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
                    sb.Append("[").Append(rm.Offset).Append(", ").Append(rm.Width).Append("]");
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
                    sb.Append("[").Append(rm.Offset).Append(", ").Append(rm.Width).Append("]");
                    return sb.ToString();
                }
            }
        }

        return value.ToString();
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