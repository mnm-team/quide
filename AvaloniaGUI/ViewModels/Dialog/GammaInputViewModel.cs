#region

using System;
using System.Windows.Input;
using AvaloniaGUI.CodeHelpers;
using AvaloniaGUI.Properties;

#endregion

namespace AvaloniaGUI.ViewModels.Dialog;

public class GammaInputViewModel : ViewModelBase
{
    private DelegateCommand _selectUnit;
    private bool _rad;
    private double _gammaRad;
    private string _gammaString;

    public double Gamma => _gammaRad;

    public ICommand SelectUnitCommand
    {
        get
        {
            if (_selectUnit == null)
            {
                _selectUnit = new DelegateCommand(SelectUnit, x => true);
            }

            return _selectUnit;
        }
    }

    public bool Rad
    {
        get { return _rad; }
        set
        {
            if (_rad == value) return;

            _rad = value;
            OnPropertyChanged(nameof(Rad));
            GammaText = GammaToString();
        }
    }

    [DoubleType]
    public string GammaText
    {
        get => _gammaString;
        set
        {
            if (!double.TryParse(value, out var result)) return;

            if (_rad)
            {
                _gammaRad = result;
            }
            else
            {
                _gammaRad = result * Math.PI / 180;
            }

            _gammaString = GammaToString();
            OnPropertyChanged(nameof(GammaText));
        }
    }

    public void SelectUnit(object parameter)
    {
        Rad = string.Equals("Rad", parameter as string);
    }

    public void SetAngle(string value)
    {
        if (string.Equals(value, Resources.Pi))
        {
            _gammaRad = Math.PI;
            GammaText = GammaToString();
        }
        else if (string.Equals(value, Resources.Pi_2))
        {
            _gammaRad = Math.PI / 2.0;
            GammaText = GammaToString();
        }
        else if (string.Equals(value, Resources.Pi_3))
        {
            _gammaRad = Math.PI / 3.0;
            GammaText = GammaToString();
        }
        else if (string.Equals(value, Resources.Pi_4))
        {
            _gammaRad = Math.PI / 4.0;
            GammaText = GammaToString();
        }
        else if (string.Equals(value, Resources.Pi_6))
        {
            _gammaRad = Math.PI / 6.0;
            GammaText = GammaToString();
        }
        else if (string.Equals(value, Resources.Pi_8))
        {
            _gammaRad = Math.PI / 8.0;
            GammaText = GammaToString();
        }
    }

    private string GammaToString()
    {
        return _rad ? $"{_gammaRad:N5}" : $"{_gammaRad * 180 / Math.PI:N2}";
    }
}