#region

using System;
using System.IO;
using System.Text;
using Avalonia.Interactivity;

#endregion

namespace QuIDE.CodeHelpers;

public class ConsoleWriter : StringWriter
{
    #region Fields

    private string _text;
    private StringBuilder _stringBuilder;
    private StringWriter _stringWriter;

    #endregion // Fields


    #region Constructor

    public ConsoleWriter()
    {
        Text = "";
        _stringBuilder = new StringBuilder();
        _stringWriter = new StringWriter(_stringBuilder);
    }

    #endregion // Ctor


    #region Public Properties

    public string Text
    {
        get { return _text; }
        set
        {
            _text = value;
            OnTextChanged();
        }
    }

    #endregion // Public Properties


    #region Events

    public event EventHandler TextChanged;

    private void OnTextChanged()
    {
        TextChanged?.Invoke(this, new RoutedEventArgs());
    }

    #endregion // Events


    #region StringWriter Methods

    public override void Write(bool value)
    {
        _stringWriter.Write(value);
        Text = _stringBuilder.ToString();
    }

    public override void Write(char value)
    {
        _stringWriter.Write(value);
        Text = _stringBuilder.ToString();
    }

    public override void Write(char[] buffer)
    {
        _stringWriter.Write(buffer);
        Text = _stringBuilder.ToString();
    }

    public override void Write(decimal value)
    {
        _stringWriter.Write(value);
        Text = _stringBuilder.ToString();
    }

    public override void Write(double value)
    {
        _stringWriter.Write(value);
        Text = _stringBuilder.ToString();
    }

    public override void Write(float value)
    {
        _stringWriter.Write(value);
        Text = _stringBuilder.ToString();
    }

    public override void Write(int value)
    {
        _stringWriter.Write(value);
        Text = _stringBuilder.ToString();
    }

    public override void Write(long value)
    {
        _stringWriter.Write(value);
        Text = _stringBuilder.ToString();
    }

    public override void Write(object value)
    {
        _stringWriter.Write(value);
        Text = _stringBuilder.ToString();
    }

    public override void Write(string value)
    {
        _stringWriter.Write(value);
        Text = _stringBuilder.ToString();
    }

    public override void Write(uint value)
    {
        _stringWriter.Write(value);
        Text = _stringBuilder.ToString();
    }

    public override void Write(ulong value)
    {
        _stringWriter.Write(value);
        Text = _stringBuilder.ToString();
    }

    public override void Write(string format, object arg0)
    {
        _stringWriter.Write(format, arg0);
        Text = _stringBuilder.ToString();
    }

    public override void Write(string format, params object[] arg)
    {
        _stringWriter.Write(format, arg);
        Text = _stringBuilder.ToString();
    }

    public override void Write(char[] buffer, int index, int count)
    {
        _stringWriter.Write(buffer, index, count);
        Text = _stringBuilder.ToString();
    }

    public override void Write(string format, object arg0, object arg1)
    {
        _stringWriter.Write(format, arg0, arg1);
        Text = _stringBuilder.ToString();
    }

    public override void Write(string format, object arg0, object arg1, object arg2)
    {
        _stringWriter.Write(format, arg0, arg1, arg2);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine()
    {
        _stringWriter.WriteLine();
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(bool value)
    {
        _stringWriter.WriteLine(value);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(char value)
    {
        _stringWriter.WriteLine(value);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(char[] buffer)
    {
        _stringWriter.WriteLine(buffer);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(decimal value)
    {
        _stringWriter.WriteLine(value);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(double value)
    {
        _stringWriter.WriteLine(value);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(float value)
    {
        _stringWriter.WriteLine(value);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(int value)
    {
        _stringWriter.WriteLine(value);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(long value)
    {
        _stringWriter.WriteLine(value);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(object value)
    {
        _stringWriter.WriteLine(value);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(string value)
    {
        _stringWriter.WriteLine(value);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(uint value)
    {
        _stringWriter.WriteLine(value);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(ulong value)
    {
        _stringWriter.WriteLine(value);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(string format, object arg0)
    {
        _stringWriter.WriteLine(format, arg0);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(string format, params object[] arg)
    {
        _stringWriter.WriteLine(format, arg);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(char[] buffer, int index, int count)
    {
        _stringWriter.WriteLine(buffer, index, count);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(string format, object arg0, object arg1)
    {
        _stringWriter.WriteLine(format, arg0, arg1);
        Text = _stringBuilder.ToString();
    }

    public override void WriteLine(string format, object arg0, object arg1, object arg2)
    {
        _stringWriter.WriteLine(format, arg0, arg1, arg2);
        Text = _stringBuilder.ToString();
    }

    #endregion // StringWriter Methods


    #region Public Methods

    public void Reset()
    {
        Text = "";
        _stringBuilder = new StringBuilder();
        _stringWriter = new StringWriter(_stringBuilder);
    }

    #endregion // Public Methods
}