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
	/// <summary>
	/// Not used so far...
	/// </summary>
	public interface IDialogConfig
	{
		IDialogConfig Mode(DialogMode mode);
		IDialogConfig CloseBehavior(DialogCloseBehavior closeBehavior);

		IDialogConfig Ok(Action del);
		IDialogConfig Cancel(Action del);
		IDialogConfig Yes(Action del);
		IDialogConfig No(Action del);

		IDialogConfig OkText(string value);
		IDialogConfig CancelText(string value);
		IDialogConfig YesText(string value);
		IDialogConfig NoText(string value);

		IDialogConfig Caption(string value);

		IDialogConfig VerticalDialogAlignment(VerticalAlignment verticalAlignment);
		IDialogConfig HorizontalDialogAlignment(HorizontalAlignment horizontalAlignment);

		IDialog CreateDialog();
	}
}