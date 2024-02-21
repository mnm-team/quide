#region

using System.Numerics;
using QuIDE.QuantumModel;

#endregion

namespace QuIDE.ViewModels.Helpers;

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

    public double Probability => _model.Probability;

    public float ProbabilityFloat => (float)_model.Probability;

    public double RectangleWidth => _rectangleWidth;

    public double RectangleMaxWidth
    {
        get => _rectangleMaxWidth;
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
        get => _relativeProbability;
        set
        {
            _relativeProbability = value;
            _rectangleWidth = _relativeProbability * _rectangleMaxWidth;
            OnPropertyChanged(nameof(RelativeProbability));
            OnPropertyChanged(nameof(RectangleWidth));
        }
    }

    public char[] Bits => _model.Bits;

    public ulong Value => _model.Value;

    public Complex? Amplitude => _model.Amplitude;

    public string Representation => _model.Representation;

    public OutputState Model => _model;

    #endregion // Presentation Properties
}