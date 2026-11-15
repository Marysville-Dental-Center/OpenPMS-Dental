// The amounts of the e-statement XML that the billing vendors read (EHG, ClaimX, EDS, POS and Tesia), written in one place (net10 plan, S3a-3).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Globalization;
using CodeBase;

namespace OpenPmsDentalBusiness.Bridges {
	public static class EStatementAmounts {
		///<summary>An amount as the e-statement XML has always written it: "F2" in the current culture, but without a sign on an amount that rounds
		///to zero. A sum of doubles such as -0.80+0.10+0.70 is about -1.1e-16; .NET Framework writes "0.00" for it and .NET "-0.00". The same text
		///as ToString("F2") on .NET Framework. The culture stays the current one, as before: a comma culture gets a comma, as it always did.</summary>
		public static string GetText(double amount) {
			return ODNumberText.ToStringNoNegativeZero(amount,"F2",CultureInfo.CurrentCulture);
		}
	}
}
