/**
    This file is part of QuIDE.

    QuIDE - The Quantum IDE
    Copyright (C) 2014  Joanna Patrzyk, Bart³omiej Patrzyk

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
using System.Windows;

namespace Technewlogic.WpfDialogManagement.Contracts
{
	public interface IDialog
	{
		DialogMode Mode { get; }
		DialogResultState Result { get; }
		DialogCloseBehavior CloseBehavior { get; set; }

		Action Ok { get; set; }
		Action Cancel { get; set; }
		Action Yes { get; set; }
		Action No { get; set; }

		bool CanOk { get; set; }
		bool CanCancel { get; set; }
		bool CanYes { get; set; }
		bool CanNo { get; set; }

		string OkText { get; set; }
		string CancelText { get; set; }
		string YesText { get; set; }
		string NoText { get; set; }

		string Caption { get; set; }

		VerticalAlignment VerticalDialogAlignment { set; }
		HorizontalAlignment HorizontalDialogAlignment { set; }

		void Show();
		void Close();
	}
}