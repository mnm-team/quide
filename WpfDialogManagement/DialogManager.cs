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
using System.Windows.Controls;
using System.Windows.Threading;
using Technewlogic.WpfDialogManagement.Contracts;

namespace Technewlogic.WpfDialogManagement
{
	public class DialogManager : IDialogManager
	{
		public DialogManager(
			ContentControl parent,
			Dispatcher dispatcher)
		{
			_dispatcher = dispatcher;
			_dialogHost = new DialogLayeringHelper(parent);
		}

		private readonly Dispatcher _dispatcher;
		private readonly IDialogHost _dialogHost;

		#region Implementation of IDialogManager

		public IMessageDialog CreateMessageDialog(string message, DialogMode dialogMode)
		{
			IMessageDialog dialog = null;
			InvokeInUIThread(() =>
			{
				dialog = new MessageDialog(_dialogHost, dialogMode, message, _dispatcher);
			});
			return dialog;
		}

		public IMessageDialog CreateMessageDialog(string message, string caption, DialogMode dialogMode)
		{
			IMessageDialog dialog = null;
			InvokeInUIThread(() =>
			{
				dialog = new MessageDialog(_dialogHost, dialogMode, message, _dispatcher)
				{
					Caption = caption
				};
			});
			return dialog;
		}

		public IProgressDialog CreateProgressDialog(DialogMode dialogMode)
		{
			IProgressDialog dialog = null;
			InvokeInUIThread(() =>
			{
				dialog = WaitProgressDialog.CreateProgressDialog(_dialogHost, dialogMode, _dispatcher);
				dialog.CloseWhenWorkerFinished = true;
			});
			return dialog;
		}

		public IProgressDialog CreateProgressDialog(string message, DialogMode dialogMode)
		{
			IProgressDialog dialog = null;
			InvokeInUIThread(() =>
			{
				dialog = WaitProgressDialog.CreateProgressDialog(_dialogHost, dialogMode, _dispatcher);
				dialog.CloseWhenWorkerFinished = true;
				dialog.Message = message;
			});
			return dialog;
		}

		public IProgressDialog CreateProgressDialog(string message, string readyMessage, DialogMode dialogMode)
		{
			IProgressDialog dialog = null;
			InvokeInUIThread(() =>
			{
				dialog = WaitProgressDialog.CreateProgressDialog(_dialogHost, dialogMode, _dispatcher);
				dialog.CloseWhenWorkerFinished = false;
				dialog.ReadyMessage = readyMessage;
				dialog.Message = message;
			});
			return dialog;
		}

		public IWaitDialog CreateWaitDialog(DialogMode dialogMode)
		{
			IWaitDialog dialog = null;
			InvokeInUIThread(() =>
			{
				dialog = WaitProgressDialog.CreateWaitDialog(_dialogHost, dialogMode, _dispatcher);
				dialog.CloseWhenWorkerFinished = true;
			});
			return dialog;
		}

		public IWaitDialog CreateWaitDialog(string message, DialogMode dialogMode)
		{
			IWaitDialog dialog = null;
			InvokeInUIThread(() =>
			{
				dialog = WaitProgressDialog.CreateWaitDialog(_dialogHost, dialogMode, _dispatcher);
				dialog.CloseWhenWorkerFinished = true;
				dialog.Message = message;
			});
			return dialog;
		}

		public IWaitDialog CreateWaitDialog(string message, string readyMessage, DialogMode dialogMode)
		{
			IWaitDialog dialog = null;
			InvokeInUIThread(() =>
			{
				dialog = WaitProgressDialog.CreateWaitDialog(_dialogHost, dialogMode, _dispatcher);
				dialog.CloseWhenWorkerFinished = false;
				dialog.Message = message;
				dialog.ReadyMessage = readyMessage;
			});
			return dialog;
		}

		public ICustomContentDialog CreateCustomContentDialog(object content, DialogMode dialogMode)
		{
			ICustomContentDialog dialog = null;
			InvokeInUIThread(() =>
			{
				dialog = new CustomContentDialog(_dialogHost, dialogMode, content, _dispatcher);
			});
			return dialog;
		}

		public ICustomContentDialog CreateCustomContentDialog(object content, string caption, DialogMode dialogMode)
		{
			ICustomContentDialog dialog = null;
			InvokeInUIThread(() =>
			{
				dialog = new CustomContentDialog(_dialogHost, dialogMode, content, _dispatcher)
				{
					Caption = caption
				};
			});
			return dialog;
		}

		#endregion

		private void InvokeInUIThread(Action del)
		{
			_dispatcher.Invoke(del);
		}
	}
}