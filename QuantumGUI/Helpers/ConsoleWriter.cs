/**
    This file is part of QuIDE.

    QuIDE - The Quantum IDE
    Copyright (C) 2014  Joanna Patrzyk, Bartłomiej Patrzyk

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QuIDE.Helpers
{
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

        public event RoutedEventHandler TextChanged;
        private void OnTextChanged()
        {
            if (TextChanged != null)
            {
                TextChanged(this, new RoutedEventArgs());
            }
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
}
