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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Technewlogic.WpfDialogManagement.Contracts;

namespace Technewlogic.WpfDialogManagement
{
	class MessageDialog : DialogBase, IMessageDialog
	{
		public MessageDialog(
			IDialogHost dialogHost, 
			DialogMode dialogMode,
			string message,
			Dispatcher dispatcher)
			: base(dialogHost, dialogMode, dispatcher)
		{
			InvokeUICall(() =>
				{
					_messageTextBlock = new TextBlock
					{
						Text = message,
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Center,
						TextWrapping = TextWrapping.Wrap,
					};
					SetContent(_messageTextBlock);
				});
		}

		private TextBlock _messageTextBlock;

		#region Implementation of IMessageDialog

		public string Message
		{
			get
			{
				var text = string.Empty;
				InvokeUICall(
					() => text = _messageTextBlock.Text);
				return text;
			}
			set
			{
				InvokeUICall(
					() => _messageTextBlock.Text = value);
			}
		}

		#endregion
	}
}