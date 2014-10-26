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
using System.Threading;
using System.Windows.Threading;
using Technewlogic.WpfDialogManagement.Contracts;

namespace Technewlogic.WpfDialogManagement
{
	class WaitProgressDialog : DialogBase, IProgressDialog
	{
		#region Factory

		public static IWaitDialog CreateWaitDialog(
			IDialogHost dialogHost,
			DialogMode dialogMode,
			Dispatcher dispatcher)
		{
			IWaitDialog dialog = null;
			dispatcher.Invoke(
				new Action(() => dialog = new WaitProgressDialog(
					dialogHost, dialogMode, true, dispatcher)),
				DispatcherPriority.DataBind);
			return dialog;
		}

		public static IProgressDialog CreateProgressDialog(
			IDialogHost dialogHost,
			DialogMode dialogMode,
			Dispatcher dispatcher)
		{
			IProgressDialog dialog = null;
			dispatcher.Invoke(
				new Action(() => dialog = new WaitProgressDialog(
					dialogHost, dialogMode, false, dispatcher)),
				DispatcherPriority.DataBind);
			return dialog;
		}

		#endregion

		private WaitProgressDialog(
			IDialogHost dialogHost,
			DialogMode dialogMode,
			bool showWaitAnimation,
			Dispatcher dispatcher)
			: base(dialogHost, dialogMode, dispatcher)
		{
			_waitProgressDialogControl = new WaitProgressDialogControl(showWaitAnimation);
			SetContent(_waitProgressDialogControl);
		}

		private readonly WaitProgressDialogControl _waitProgressDialogControl;
		private bool _isReady;

		#region Implementation of IMessageDialog

		public string Message
		{
			get
			{
				var text = string.Empty;
				InvokeUICall(
					() => text = _waitProgressDialogControl.DisplayText);
				return text;
			}
			set
			{
				InvokeUICall(
					() => _waitProgressDialogControl.DisplayText = value);
			}
		}

		#endregion

		#region Implementation of IWaitDialog

		public Action WorkerReady { get; set; }
		public bool CloseWhenWorkerFinished { get; set; }

		private string _readyMessage;
		public string ReadyMessage
		{
			get { return _readyMessage; }
			set
			{
				_readyMessage = value;
				if (_isReady)
					InvokeUICall(
						() => _waitProgressDialogControl.DisplayText = value);
			}
		}

		private readonly ManualResetEvent _beginWork =
			new ManualResetEvent(false);

		public void Show(Action workerMethod)
		{
			ThreadPool.QueueUserWorkItem(o =>
			{
				try
				{
					_beginWork.WaitOne(-1);

					workerMethod();

					InvokeUICall(() =>
					{
						_isReady = true;

						if (WorkerReady != null)
							WorkerReady();

						if (CloseWhenWorkerFinished)
						{
							Close();
							return;
						}

						_waitProgressDialogControl.DisplayText = ReadyMessage;
						_waitProgressDialogControl.Finish();

						DialogBaseControl.RemoveButtons();
						DialogBaseControl.AddOkButton();
					});
				}
				catch (Exception ex)
				{
					InvokeUICall(() =>
					{
						Close();
						throw ex;
					});
				}
			});

			Show();

			_beginWork.Set();
		}

		public new void InvokeUICall(Action uiWorker)
		{
			base.InvokeUICall(uiWorker);
		}

		#endregion

		#region Implementation of IProgressDialog

		public int Progress
		{
			get
			{
				var progress = 0;
				InvokeUICall(
					() => progress = _waitProgressDialogControl.Progress);
				return progress;
			}
			set
			{
				InvokeUICall(
					() => _waitProgressDialogControl.Progress = value);
			}
		}

		#endregion
	}
}