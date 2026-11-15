// S3a-3 (net10 plan): a statement whose totals sum to about -1e-16. The printed statement and invoice totals (SheetFiller, "c" on doubles)
// and the e-statement XML that vendors read (EHG, ClaimX, EDS, POS and Tesia, "F2" in the current culture) format sums of doubles. A payment
// of -0.80 with charges of 0.10 and 0.70 sums to about -1.1e-16, not 0. .NET Framework writes that as "$0.00" and "0.00"; .NET writes
// "-$0.00" (or "($0.00)") and "-0.00". These sites are not edited in S3a-3 (docs/port-notes/S3a-3.md, section 4): S3a-4's golden statement
// output includes this case. A pin of ODNumberText (both runtimes) and the .NET Framework record of the text the sites give today
// (inconclusive on .NET). No database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Globalization;
using System.Threading;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatementTests {
	[TestClass]
	public class StatementMoneyTextTests {
		private CultureInfo _cultureInfoOld;

		[TestInitialize]
		public void TestInitialize() {
			_cultureInfoOld=Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
		}

		[TestCleanup]
		public void TestCleanup() {
			Thread.CurrentThread.CurrentCulture=_cultureInfoOld;
		}

		///<summary>The invoice's "total" line in SheetFiller: (paymentValue+procAmt+adjAmt+payPlanAmt), in that order.</summary>
		private static double NearZeroTotal() {
			double paymentValue=-0.80;
			double procAmt=0.10;
			double adjAmt=0.70;
			double payPlanAmt=0;
			return paymentValue+procAmt+adjAmt+payPlanAmt;
		}

		[TestMethod]
		public void Pin_NearZeroTotal_NoSignWithODNumberText() {
			double total=NearZeroTotal();
			Assert.IsTrue(total<0 && total>-1e-15,"The fixture sums to a tiny negative number, not to zero: "+total.ToString("R",CultureInfo.InvariantCulture));
			Assert.AreEqual("$0.00",ODNumberText.ToStringNoNegativeZero(total,"c",CultureInfo.CurrentCulture));
			Assert.AreEqual("0.00",ODNumberText.ToStringNoNegativeZero(total,"F2",CultureInfo.CurrentCulture));
			Assert.AreEqual("0.00",ODNumberText.ToStringNoNegativeZero(total,"F2",CultureInfo.InvariantCulture));
		}

		[TestMethod]
		public void Record_NearZeroTotal_TextTheStatementSitesGiveToday() {
			if(typeof(object).Assembly.GetName().Name!="mscorlib") {
				Assert.Inconclusive("The .NET Framework record; on .NET these sites write a sign until they are routed through ODNumberText.");
			}
			double total=NearZeroTotal();
			Assert.AreEqual("$0.00",total.ToString("c"),"SheetFiller's statement and invoice totals.");
			Assert.AreEqual("0.00",total.ToString("F2"),"The e-statement XML (current culture).");
			Assert.AreEqual(ODNumberText.ToStringNoNegativeZero(total,"c",CultureInfo.CurrentCulture),total.ToString("c"));
			Assert.AreEqual(ODNumberText.ToStringNoNegativeZero(total,"F2",CultureInfo.CurrentCulture),total.ToString("F2"));
			Thread.CurrentThread.CurrentCulture=new CultureInfo("fr-CA");
			Assert.AreEqual("0,00",total.ToString("F2"),"In a comma culture the e-statement XML has a comma today.");
		}
	}
}
