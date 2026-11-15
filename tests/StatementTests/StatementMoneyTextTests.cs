// S3a-3 (net10 plan): statement money text without negative zero. The printed statement and invoice totals (SheetFiller.totInsBalValsHelper,
// "c") and the e-statement XML that vendors read (EHG, ClaimX, EDS, POS and Tesia, "F2" in the current culture) format sums of doubles. A
// payment of -0.80 with charges of 0.10 and 0.70 sums to about -1.1e-16, not 0. .NET Framework writes that as "$0.00" and "0.00"; .NET writes
// "-$0.00" (or "($0.00)") and "-0.00" unless the sign is dropped. The sites go through SheetFiller.GetMoneyText, GetInvoiceTotalValues,
// GetTotalInsBalanceValues and Bridges.EStatementAmounts.GetText, which these tests call with the near-zero fixture: the .NET Framework text on
// both runtimes. Before/after equality with the replaced ToString calls on fixture amounts (the .NET Framework record; inconclusive on .NET),
// and a source scan that the five bridges write every double amount through EStatementAmounts. No database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.Bridges;

namespace StatementTests {
	[TestClass]
	public class StatementMoneyTextTests {
		private CultureInfo _cultureInfoOld;

		[TestInitialize]
		public void TestInitialize() {
			_cultureInfoOld=Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo("en-US");
		}

		[TestCleanup]
		public void TestCleanup() {
			Thread.CurrentThread.CurrentCulture=_cultureInfoOld;
		}

		private const double _paymentValue=-0.80;
		private const double _procAmt=0.10;
		private const double _adjAmt=0.70;

		///<summary>The invoice's balance remaining line in SheetFiller: (paymentValue+procAmt+adjAmt+payPlanAmt), in that order.</summary>
		private static double NearZeroTotal() {
			double payPlanAmt=0;
			return _paymentValue+_procAmt+_adjAmt+payPlanAmt;
		}

		private static bool IsNetFramework() {
			return typeof(object).Assembly.GetName().Name=="mscorlib";
		}

		#region Pins: the .NET Framework text on both runtimes
		[TestMethod]
		public void Pin_NearZeroTotal_IsATinyNegativeNumber() {
			double total=NearZeroTotal();
			Assert.IsTrue(total<0 && total>-1e-15,"The fixture sums to a tiny negative number, not to zero: "+total.ToString("R",CultureInfo.InvariantCulture));
			Assert.IsTrue((_procAmt+_adjAmt)-0.80<0,"0.10+0.70 is just under 0.80, so a balance of total-insEst is a tiny negative number too.");
		}

		[TestMethod]
		public void Pin_SheetFiller_InvoiceTotals() {
			string[] arrayValues=SheetFiller.GetInvoiceTotalValues(_procAmt,_adjAmt,0,_paymentValue);
			Assert.AreEqual(6,arrayValues.Length);
			Assert.AreEqual("$0.10",arrayValues[0],"Procedures.");
			Assert.AreEqual("$0.70",arrayValues[1],"Adjustments.");
			Assert.AreEqual("$0.80",arrayValues[2],"Procedures, adjustments and pay plan charges.");
			Assert.AreEqual(GetOldMoneyTextOfNegativePayment(),arrayValues[3],"Payments.");
			Assert.AreEqual("$0.00",arrayValues[4],"The balance remaining, about -1.1e-16: .NET writes -$0.00 for ToString(\"c\").");
			Assert.AreEqual("$0.00",arrayValues[5],"Pay plan charges.");
		}

		[TestMethod]
		public void Pin_SheetFiller_StatementTotals() {
			string[] arrayValues=SheetFiller.GetTotalInsBalanceValues(_procAmt+_adjAmt,0.80);
			CollectionAssert.AreEqual(new[] { "$0.80","$0.80","$0.00","","","" },arrayValues,"Total, insurance estimate, balance of about -1.1e-16.");
			arrayValues=SheetFiller.GetTotalInsBalanceValues(NearZeroTotal(),0);
			CollectionAssert.AreEqual(new[] { "$0.00","$0.00","$0.00","","","" },arrayValues,"A limited statement whose total is about -1.1e-16.");
			Assert.AreEqual("$0.00",SheetFiller.GetMoneyText(NearZeroTotal()));
			Assert.AreEqual("$0.00",SheetFiller.GetMoneyText(-0.004));
			Assert.AreEqual("$12.50",SheetFiller.GetMoneyText(12.5));
			Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo("fr-CA");
			Assert.AreEqual((0d).ToString("c"),SheetFiller.GetMoneyText(NearZeroTotal()),"The current culture is kept (fr-CA: 0,00 $).");
			StringAssert.StartsWith(SheetFiller.GetMoneyText(NearZeroTotal()),"0,00");
		}

		[TestMethod]
		public void Pin_EStatementAmounts_NearZero() {
			Assert.AreEqual("0.00",EStatementAmounts.GetText(NearZeroTotal()),".NET writes -0.00 for ToString(\"F2\").");
			Assert.AreEqual("0.00",EStatementAmounts.GetText((_procAmt+_adjAmt)-0.80),"PatientShare and AmountDue (amountDue-insEst).");
			Assert.AreEqual("-0.80",EStatementAmounts.GetText(_paymentValue));
			Assert.AreEqual("125.50",EStatementAmounts.GetText(125.5));
			StringBuilder stringBuilder=new StringBuilder();
			XmlWriterSettings xmlWriterSettings=new XmlWriterSettings { OmitXmlDeclaration=true };
			using(XmlWriter writer=XmlWriter.Create(stringBuilder,xmlWriterSettings)) {
				writer.WriteStartElement("AccountSummary");
				writer.WriteElementString("AmountDue",EStatementAmounts.GetText(NearZeroTotal()));
				writer.WriteEndElement();
			}
			Assert.AreEqual("<AccountSummary><AmountDue>0.00</AmountDue></AccountSummary>",stringBuilder.ToString());
			Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo("fr-CA");
			Assert.AreEqual("0,00",EStatementAmounts.GetText(NearZeroTotal()),"The current culture is kept: a comma culture has a comma, as it always had.");
		}

		///<summary>The payments line of the fixture as ToString("c") writes -0.80 in en-US on this runtime (a negative amount, not a negative zero:
		///its pattern is the culture's).</summary>
		private static string GetOldMoneyTextOfNegativePayment() {
			return _paymentValue.ToString("c",CultureInfo.GetCultureInfo("en-US"));
		}
		#endregion Pins

		#region Before and after (the .NET Framework record)
		///<summary>Statement amounts: whole and cent amounts, values that round to zero, sums of doubles that are not exact, and 2000 seeded random
		///amounts.</summary>
		private static List<double> GetFixtureAmounts() {
			List<double> listAmounts=new List<double> { 0,BitConverter.Int64BitsToDouble(long.MinValue),NearZeroTotal(),(_procAmt+_adjAmt)-0.80,0.01,
				-0.01,-0.004,-0.005,0.005,12.5,-12.5,125.5,99.995,1234.56,-1234.56,99999.99,0.1+0.2,1.0/3 };
			Random random=new Random(20261115);
			for(int i=0;i<2000;i++) {
				double amount=Math.Round(random.NextDouble()*Math.Pow(10,random.Next(0,6)),random.Next(0,3));
				if(i%3==0) {
					amount=-amount;
				}
				if(i%7==0) {
					amount=random.NextDouble()*0.01-0.005;
				}
				listAmounts.Add(amount);
			}
			return listAmounts;
		}

		[TestMethod]
		public void BeforeAfter_StatementMoneyText_EqualsTheReplacedToString() {
			if(!IsNetFramework()) {
				Assert.Inconclusive("Before/after equality is the .NET Framework record; on .NET the pins of this class hold instead.");
			}
			List<double> listAmounts=GetFixtureAmounts();
			foreach(string cultureName in new[] { "en-US","en-CA","fr-CA","en-GB" }) {
				Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo(cultureName);
				for(int i=0;i<listAmounts.Count;i++) {
					double amount=listAmounts[i];
					string message=amount.ToString("R",CultureInfo.InvariantCulture)+" "+cultureName;
					Assert.AreEqual(amount.ToString("c"),SheetFiller.GetMoneyText(amount),message);
					Assert.AreEqual(amount.ToString("F2"),EStatementAmounts.GetText(amount),message);
					double procAmt=amount;
					double adjAmt=listAmounts[(i+1)%listAmounts.Count];
					double payPlanAmt=listAmounts[(i+2)%listAmounts.Count];
					double paymentValue=listAmounts[(i+3)%listAmounts.Count];
					//The six invoice lines as totInsBalValsHelper wrote them before S3a-3.
					CollectionAssert.AreEqual(new[] { procAmt.ToString("c"),adjAmt.ToString("c"),(procAmt+adjAmt+payPlanAmt).ToString("c"),
						paymentValue.ToString("c"),(paymentValue + procAmt + adjAmt + payPlanAmt).ToString("c"),payPlanAmt.ToString("c") },
						SheetFiller.GetInvoiceTotalValues(procAmt,adjAmt,payPlanAmt,paymentValue),message);
					//Total, insurance estimate and balance.
					CollectionAssert.AreEqual(new[] { procAmt.ToString("c"),adjAmt.ToString("c"),(procAmt-adjAmt).ToString("c"),"","","" },
						SheetFiller.GetTotalInsBalanceValues(procAmt,adjAmt),message);
				}
			}
		}
		#endregion Before and after

		#region Source scan
		///<summary>The five e-statement bridges write every double amount through EStatementAmounts.GetText; only PayPlanDue, a decimal (no negative
		///zero on either runtime), keeps ToString("F2"). Tesia lives in WpfControlsOD, which this project does not reference, so the scan is its
		///proof that it calls the tested helper.</summary>
		[TestMethod]
		public void SourceScan_EStatementBridgesWriteAmountsThroughTheHelper() {
			Dictionary<string,int> dictCounts=new Dictionary<string,int> {
				{ Path.Combine("OpenPmsDentalBusiness","Bridges","EHG_statements.cs"),9 },
				{ Path.Combine("OpenPmsDentalBusiness","Bridges","ClaimX_Statements.cs"),8 },
				{ Path.Combine("OpenPmsDentalBusiness","Bridges","EDS_Statements.cs"),7 },
				{ Path.Combine("OpenPmsDentalBusiness","Bridges","POS_statements.cs"),7 },
				{ Path.Combine("WpfControlsOD","Bridges","Tesia_statements.cs"),6 },
			};
			foreach(KeyValuePair<string,int> keyValuePair in dictCounts) {
				string source=File.ReadAllText(TestDb.FindRepoFile(keyValuePair.Key));
				List<string> listOffenders=Regex.Matches(source,@"[\w.()\[\]-]+\.ToString\(\s*""[fFcCnN]\d?""\s*\)").Cast<Match>()
					.Select(x => x.Value)
					.Where(x => x!="payPlanDue.ToString(\"F2\")")
					.ToList();
				Assert.AreEqual(0,listOffenders.Count,keyValuePair.Key+": "+string.Join(", ",listOffenders));
				Assert.AreEqual(keyValuePair.Value,Regex.Matches(source,@"EStatementAmounts\.GetText\(").Count,keyValuePair.Key);
			}
		}

		///<summary>Every total line of totInsBalValsHelper goes through GetMoneyText (no ToString("c") left in it).</summary>
		[TestMethod]
		public void SourceScan_SheetFillerTotalsGoThroughGetMoneyText() {
			string source=File.ReadAllText(TestDb.FindRepoFile(Path.Combine("OpenPmsDentalBusiness","SheetFramework","SheetFiller.cs"))).Replace("\r\n","\n");
			int start=source.IndexOf("private static string[] totInsBalValsHelper(",StringComparison.Ordinal);
			Assert.IsTrue(start>=0);
			int end=source.IndexOf("\n\t\t}\n",start,StringComparison.Ordinal);
			string body=source.Substring(start,end-start);
			Assert.IsFalse(Regex.IsMatch(body,@"\.ToString\(\s*""[cC]""\s*\)"),"totInsBalValsHelper formats its totals through GetMoneyText.");
			Assert.AreEqual(4,Regex.Matches(body,@"\bGetMoneyText\(").Count,"The single-line totals.");
			Assert.AreEqual(1,Regex.Matches(body,@"\bGetInvoiceTotalValues\(").Count);
			Assert.AreEqual(4,Regex.Matches(body,@"\bGetTotalInsBalanceValues\(").Count);
		}
		#endregion Source scan
	}
}
