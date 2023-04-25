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

    public double Gamma
    {
        get { return _gammaRad; }
    }

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
            if (_rad != value)
            {
                _rad = value;
                OnPropertyChanged("Rad");
                GammaText = GammaToString();
            }
        }
    }

    [DoubleType]
    public string GammaText
    {
        get { return _gammaString; }
        set
        {
            double result;
            if (double.TryParse(value, out result))
            {
                if (_rad)
                {
                    _gammaRad = result;
                }
                else
                {
                    _gammaRad = result * Math.PI / 180;
                }

                _gammaString = GammaToString();
                OnPropertyChanged("GammaText");
            }
        }
    }

    public void SelectUnit(object parameter)
    {
        if (string.Equals("Rad", parameter as string))
        {
            Rad = true;
        }
        else
        {
            Rad = false;
        }
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
        if (_rad)
        {
            return string.Format("{0:N5}", _gammaRad);
        }
        else
        {
            return string.Format("{0:N2}", _gammaRad * 180 / Math.PI);
        }
    }
}