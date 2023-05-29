#region

using System.Numerics;
using AvaloniaGUI.ViewModels.MainModels.QuantumModel;

#endregion

namespace AvaloniaGUI.ViewModels.Helpers;

public class StateViewModel : ViewModelBase
{
    #region Fields

    private double _rectangleMaxWidth = 125;

    private OutputState _model;
    private bool[] _bits;
    private double _rectangleWidth;
    private double _relativeProbability;

    #endregion // Fields


    #region Constructor

    public StateViewModel(OutputState model)
    {
        _model = model;
        _bits = new bool[_model.Width];
    }

    #endregion // Constructor


    #region Presentation Properties

    public double Probability
    {
        get { return _model.Probability; }
    }

    public float ProbabilityFloat
    {
        get { return (float)_model.Probability; }
    }

    public double RectangleWidth
    {
        get { return _rectangleWidth; }
    }

    public double RectangleMaxWidth
    {
        get { return _rectangleMaxWidth; }
        set
        {
            if (value == _rectangleMaxWidth)
            {
                return;
            }

            _rectangleMaxWidth = value;
            OnPropertyChanged(nameof(RectangleMaxWidth));
        }
    }

    public double RelativeProbability
    {
        get { return _relativeProbability; }
        set
        {
            _relativeProbability = value;
            _rectangleWidth = _relativeProbability * _rectangleMaxWidth;
            OnPropertyChanged(nameof(RelativeProbability));
            OnPropertyChanged(nameof(RectangleWidth));
        }
    }

    public char[] Bits
    {
        get { return _model.Bits; }
    }

    public ulong Value
    {
        get { return _model.Value; }
    }

    public Complex? Amplitude
    {
        get { return _model.Amplitude; }
    }

    public string Representation
    {
        get { return _model.Representation; }
    }

    public OutputState Model
    {
        get { return _model; }
    }

    #endregion // Presentation Properties


    #region Private Helpers

    #endregion // Private Helpers
}