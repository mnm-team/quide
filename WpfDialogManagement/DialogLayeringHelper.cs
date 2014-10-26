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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Technewlogic.WpfDialogManagement
{
	class DialogLayeringHelper : IDialogHost
	{
		public DialogLayeringHelper(ContentControl parent)
		{
			_parent = parent;
		}

		private readonly ContentControl _parent;
		private readonly List<object> _layerStack = new List<object>();

		public bool HasDialogLayers { get { return _layerStack.Any(); } }

		#region Implementation of IDialogHost

		public void ShowDialog(DialogBaseControl dialog)
		{
			_layerStack.Add(_parent.Content);
			_parent.Content = dialog;
		}

		public void HideDialog(DialogBaseControl dialog)
		{
			if (_parent.Content == dialog)
			{
				var oldContent = _layerStack.Last();
				_layerStack.Remove(oldContent);
				_parent.Content = oldContent;
			}
			else
				_layerStack.Remove(dialog);
		}

		public FrameworkElement GetCurrentContent()
		{
			return _parent;
		}

		#endregion
	}
}