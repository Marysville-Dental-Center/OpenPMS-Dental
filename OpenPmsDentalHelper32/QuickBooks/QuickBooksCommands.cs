// The QuickBooks Desktop module's commands and their arguments (S3a-13 of the .NET 10 move, docs/port-notes/S3a-13.md, section 6). Compiled into
// the helper and linked into WpfControlsOD (Bridges\QuickBooks.cs), so the program and the helper name them alike.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Globalization;

namespace OpenPmsDentalHelper32.QuickBooks {
	///<summary>Every command takes the company file first, as the program reads it (PrefName.QuickBooksCompanyFile, or the setup window's text for
	///TestConnection). The values that today's code read from the program's state or culture are computed by the program and sent as text, so the
	///helper's culture and its lack of a database change nothing: the deposit's amount as amount.ToString("F") in the program's culture, and
	///whether the class goes into the deposit (a class was given and PrefName.QuickBooksClassRefsEnabled).</summary>
	internal static class QuickBooksCommands {
		public const string MODULE="QuickBooks";
		///<summary>Args: company file. Results: the message today's TestConnection returned ("Connection to QuickBooks was successful." or
		///"Error: " and the exception's message).</summary>
		public const string TEST_CONNECTION="TestConnection";
		///<summary>Args: company file. Results: the full names of the active accounts.</summary>
		public const string GET_LIST_OF_ACCOUNTS="GetListOfAccounts";
		///<summary>Args: company file. Results: the full names of the active classes.</summary>
		public const string GET_LIST_OF_CLASSES="GetListOfClasses";
		///<summary>Args: company file, the date (DateTime.ToBinary, invariant), the deposit account, the income account, the amount text, the memo,
		///the class, and "1" when the class goes into the deposit ("0" otherwise). Results: none.</summary>
		public const string CREATE_DEPOSIT="CreateDeposit";

		public static string DateToText(DateTime dateTime) {
			return dateTime.ToBinary().ToString(CultureInfo.InvariantCulture);
		}

		public static DateTime DateFromText(string text) {
			return DateTime.FromBinary(long.Parse(text,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture));
		}

		public static string BoolToText(bool value) {
			return value ? "1" : "0";
		}

		public static bool BoolFromText(string text) {
			return text=="1";
		}
	}
}
