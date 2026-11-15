// S3a-3 (net10 plan): the amounts and quantities of the X12 837 files (4010 and 5010) and the RAMQ claim file, written with InvariantCulture and
// the text .NET Framework gives ("G15" for doubles, "G7" for floats, no sign on an amount that rounds to zero). Pins that hold on both runtimes,
// and before/after equality with the replaced expressions on fixture amounts (the .NET Framework record; inconclusive on .NET). A source scan of the
// two X837 files checks the inline sites the helpers do not reach. No database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CodeBase;
using ForkMigrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using RamqClaims=OpenPmsDentalBusiness.Eclaims.Ramq;

namespace BCLAIMTests {
	[TestClass]
	public class ClaimNumberTextTests {
		private static readonly double _negativeZero=BitConverter.Int64BitsToDouble(long.MinValue);
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

		///<summary>Fees, payments and write-offs as a claim has them: whole and cent amounts, values that round to zero, negative adjustments, large
		///fees and 2000 seeded random amounts with up to four decimals.</summary>
		private static List<double> GetFixtureAmounts() {
			List<double> listAmounts=new List<double> { 0,_negativeZero,0.01,0.99,1,12.5,125.5,99.995,100.005,1234.56,99999.99,0.1+0.2,1.0/3,-0.001,
				-0.004,-0.005,-12.5,1e15,0.07,19.99,4.35,0.285 };
			Random random=new Random(837);
			for(int i=0;i<2000;i++) {
				double amount=Math.Round(random.NextDouble()*Math.Pow(10,random.Next(0,6)),random.Next(0,5));
				if(i%5==0) {
					amount=-amount;
				}
				if(i%7==0) {
					amount=random.NextDouble()*(i%2==0 ? 1 : -0.01);//sums of doubles that are not exact
				}
				listAmounts.Add(amount);
			}
			return listAmounts;
		}

		#region Pins
		[TestMethod]
		public void Pins_X12AmountsHaveAPeriodAndNoNegativeZero() {
			Assert.AreEqual("12.50",X837_5010.AmountToStr(12.5));
			Assert.AreEqual("0.00",X837_5010.AmountToStr(-0.004),".NET writes -0.00 for ToString(\"F2\").");
			Assert.AreEqual("0.00",X837_5010.AmountToStr(_negativeZero));
			Assert.AreEqual(".00",X837_5010.AmountToStrNoLeading(-0.001));
			Assert.AreEqual(".99",X837_5010.AmountToStrNoLeading(0.99));
			Assert.AreEqual("125.50",X837_5010.AmountToStrNoLeading(125.5));
			Assert.AreEqual("125.5",ODNumberText.ToStringG15(125.5,CultureInfo.InvariantCulture),"The charge amounts (SV102, SV203, SV302) as today.");
			Assert.AreEqual("0.3",ODNumberText.ToStringG15(0.1+0.2,CultureInfo.InvariantCulture));
			using(new Culture("fr-CA")) {
				Assert.AreEqual("12.50",X837_5010.AmountToStr(12.5),"X12 always has a period (fr-CA gave 12,50 before S3a-3).");
				Assert.AreEqual("125.50",X837_5010.AmountToStrNoLeading(125.5));
			}
		}

		[TestMethod]
		public void Pins_RamqDigits() {
			Assert.AreEqual("012550",RamqClaims.GetAmountDigits(125.5));
			Assert.AreEqual("000000",RamqClaims.GetAmountDigits(-0.001),".NET writes -0.00 for ToString(\"F\").");
			Assert.AreEqual("001255",RamqClaims.GetClaimTotalDigits(125.5),"TOT_DEM drops the trailing zero, as it always did (S3a-3 records it).");
			Assert.AreEqual("001255",RamqClaims.GetClaimTotalDigits(1255),"The same digits as 125.5.");
			Assert.AreEqual("000003",RamqClaims.GetClaimTotalDigits(0.1+0.2),".NET writes 0.30000000000000004 for ToString().");
			using(new Culture("fr-CA")) {
				Assert.AreEqual("012550",RamqClaims.GetAmountDigits(125.5),"fr-CA gave 125,50 before S3a-3.");
				Assert.AreEqual("001255",RamqClaims.GetClaimTotalDigits(125.5),"fr-CA gave 0125,5 before S3a-3.");
			}
		}
		#endregion Pins

		#region Before and after
		[TestMethod]
		public void BeforeAfter_X12AndRamq_EqualTheReplacedExpressions() {
			if(typeof(object).Assembly.GetName().Name!="mscorlib") {
				Assert.Inconclusive("Before/after equality is the .NET Framework record; on .NET the pins of this class hold instead.");
			}
			//The office cultures with a period: the replaced expressions used the current culture.
			foreach(string cultureName in new[] { "en-US","en-CA","en-GB" }) {
				using(new Culture(cultureName)) {
					foreach(double amount in GetFixtureAmounts()) {
						string message=amount.ToString("R",CultureInfo.InvariantCulture)+" "+cultureName;
						//X837_5010 AMT02 (ShareOfCost) and X837_4010 AMT02: ToString("F2") and ToString("f2").
						Assert.AreEqual(amount.ToString("F2"),X837_5010.AmountToStr(amount),message);
						Assert.AreEqual(amount.ToString("f2"),ODNumberText.ToStringNoNegativeZero(amount,"f2",CultureInfo.InvariantCulture),message);
						//X837_4010 AMT*D: ToString("F").
						Assert.AreEqual(amount.ToString("F"),ODNumberText.ToStringNoNegativeZero(amount,"F",CultureInfo.InvariantCulture),message);
						//X837_5010 CAS, AMT and SVD amounts.
						Assert.AreEqual(OldAmountToStrNoLeading(amount),X837_5010.AmountToStrNoLeading(amount),message);
						//The charge amounts SV102, SV203, SV302 (both versions): FeeBilled.ToString().
						Assert.AreEqual(amount.ToString(),ODNumberText.ToStringG15(amount,CultureInfo.InvariantCulture),message);
						//CLM02 and PS1: a decimal with "f2".
						decimal amountDecimal=(decimal)(Math.Abs(amount)<1e14 ? amount : 0);
						Assert.AreEqual(amountDecimal.ToString("f2"),amountDecimal.ToString("f2",CultureInfo.InvariantCulture),message);
						//RAMQ TOT_DEM and MNT.
						Assert.AreEqual(amount.ToString().Replace(".","").PadLeft(6,'0'),RamqClaims.GetClaimTotalDigits(amount),message);
						Assert.AreEqual(amount.ToString("F").Replace(".","").PadLeft(6,'0'),RamqClaims.GetAmountDigits(amount),message);
					}
					//CTP04 (X837_5010): the drug quantity, a float, ToString().
					Random random=new Random(4);
					for(int i=0;i<1000;i++) {
						float drugQty=(float)Math.Round(random.NextDouble()*Math.Pow(10,random.Next(0,6)),random.Next(0,4));
						Assert.AreEqual(drugQty.ToString(),ODNumberText.ToStringG7(drugQty,CultureInfo.InvariantCulture),drugQty.ToString("R"));
					}
				}
			}
		}

		#region Source scan
		///<summary>The inline X12 sites (CLM02, PS1, SV102, SV203, SV302, CTP04, AMT02 and AMT*D in X837_5010.cs and X837_4010.cs) are not reached by
		///the helper tests above, so the source is read: a later edit that brings back FeeBilled.ToString(), DrugQty.ToString() or ToString("f2")
		///without a format provider fails here.</summary>
		[TestMethod]
		public void SourceScan_X837AmountsNameTheirFormatAndProvider() {
			List<string> listOffenders=new List<string>();
			int countCharges=0;
			int countDrugQty=0;
			foreach(string fileName in new[] { "X837_5010.cs","X837_4010.cs" }) {
				string source=File.ReadAllText(TestDb.FindRepoFile(Path.Combine("OpenPmsDentalBusiness","X12",fileName)));
				listOffenders.AddRange(FindNumberTextOffenders(source).Select(x => fileName+": "+x));
				string code=StripComments(source);
				countCharges+=Regex.Matches(code,@"ODNumberText\.ToStringG15\(claimProcs\[j\]\.FeeBilled,CultureInfo\.InvariantCulture\)").Count;
				countDrugQty+=Regex.Matches(code,@"ODNumberText\.ToStringG7\(proc\.DrugQty,CultureInfo\.InvariantCulture\)").Count;
			}
			Assert.AreEqual(0,listOffenders.Count,"X12 numbers need an explicit format and InvariantCulture (S3a-3):\r\n"+string.Join("\r\n",listOffenders));
			Assert.AreEqual(5,countCharges,"SV102, SV203, SV302 (5010) and SV102, SV302 (4010) go through ToStringG15.");
			Assert.AreEqual(1,countDrugQty,"CTP04 (5010) goes through ToStringG7.");
		}

		[TestMethod]
		public void SourceScan_FindsTheCallsItLooksFor() {
			string source="string a=proc.FeeBilled.ToString();\n"
				+"string b=amount.ToString(\"f2\");\n"
				+"string c=proc.DrugQty.ToString(CultureInfo.InvariantCulture);//a provider but no format: shortest round trip on .NET\n"
				+"string d=amount.ToString( \"F\" );\n"
				+"//string e=proc.FeeBilled.ToString();\n"
				+"/* string f=amount.ToString(\"F2\"); */\n"
				+"string g=\"x.ToString(\\\"f2\\\")\";\n"
				+"string h=amount.ToString(\"F2\",CultureInfo.InvariantCulture)+claimFee.ToString(\"f2\",CultureInfo.InvariantCulture);\n"
				+"string i=ODNumberText.ToStringG15(proc.FeeBilled,CultureInfo.InvariantCulture)+proc.FeeBilled.ToString(\"G15\",CultureInfo.InvariantCulture);\n";
			List<string> listOffenders=FindNumberTextOffenders(source);
			Assert.AreEqual(4,listOffenders.Count,string.Join("\r\n",listOffenders));
			StringAssert.Contains(listOffenders[0],"FeeBilled.ToString()");
			StringAssert.Contains(listOffenders[1],"ToString(\"f2\")");
			StringAssert.Contains(listOffenders[2],"DrugQty.ToString(CultureInfo.InvariantCulture)");
			StringAssert.Contains(listOffenders[3],"ToString( \"F\" )");
		}

		///<summary>The calls in the code (comments and string literals left out) that format an X12 number without a format provider: any
		///FeeBilled.ToString(...) or DrugQty.ToString(...) without both a format and a provider, and ToString("f2"), ToString("F2"),
		///ToString("f") or ToString("F") with no provider.</summary>
		private static List<string> FindNumberTextOffenders(string source) {
			string code=StripComments(source);
			List<string> listOffenders=new List<string>();
			foreach(Match match in Regex.Matches(code,@"(?<receiver>\b(?:FeeBilled|DrugQty)\s*)?\.ToString\(")) {
				string args=GetArguments(code,match.Index+match.Length);
				bool hasProvider=HasTopLevelComma(args) || Regex.IsMatch(args,@"CultureInfo|NumberFormatInfo|IFormatProvider|FormatProvider");
				bool isMoneyFormat=Regex.IsMatch(args,@"^\s*""(?:f2|F2|f|F)""");
				bool isFailing;
				if(match.Groups["receiver"].Success) {
					isFailing=!hasProvider || !Regex.IsMatch(args,@"^\s*""");
				}
				else {
					isFailing=isMoneyFormat && !hasProvider;
				}
				if(isFailing) {
					int line=code.Substring(0,match.Index).Count(x => x=='\n')+1;
					listOffenders.Add("line "+line+": "+code.Substring(match.Index,Math.Min(code.Length-match.Index,match.Length+args.Length+1)).Trim());
				}
			}
			return listOffenders;
		}

		///<summary>The text between the opening parenthesis (just before start) and its closing one, with string literals skipped over.</summary>
		private static string GetArguments(string code,int start) {
			int depth=1;
			int i=start;
			while(i<code.Length) {
				char c=code[i];
				if(c=='"') {
					i++;
					while(i<code.Length && code[i]!='"') {
						i+=(code[i]=='\\' ? 2 : 1);
					}
				}
				else if(c=='(') {
					depth++;
				}
				else if(c==')') {
					depth--;
					if(depth==0) {
						return code.Substring(start,i-start);
					}
				}
				i++;
			}
			return code.Substring(start);
		}

		///<summary>True when the arguments have a comma outside parentheses and string literals, so more than one argument.</summary>
		private static bool HasTopLevelComma(string args) {
			int depth=0;
			for(int i=0;i<args.Length;i++) {
				char c=args[i];
				if(c=='"') {
					i++;
					while(i<args.Length && args[i]!='"') {
						i+=(args[i]=='\\' ? 2 : 1);
					}
				}
				else if(c=='(' || c=='[' || c=='{') {
					depth++;
				}
				else if(c==')' || c==']' || c=='}') {
					depth--;
				}
				else if(c==',' && depth==0) {
					return true;
				}
			}
			return false;
		}

		///<summary>The source with // and /* */ comments blanked and the content of string literals blanked (their quotes kept), so a call written
		///in a comment or inside a string is not counted. Line breaks are kept, so line numbers stay right. Verbatim and character literals are
		///handled well enough for these files.</summary>
		private static string StripComments(string source) {
			StringBuilder stringBuilder=new StringBuilder(source.Length);
			int i=0;
			while(i<source.Length) {
				char c=source[i];
				if(c=='/' && i+1<source.Length && source[i+1]=='/') {
					while(i<source.Length && source[i]!='\n') {
						i++;
					}
				}
				else if(c=='/' && i+1<source.Length && source[i+1]=='*') {
					i+=2;
					while(i<source.Length && !(source[i]=='*' && i+1<source.Length && source[i+1]=='/')) {
						if(source[i]=='\n') {
							stringBuilder.Append('\n');
						}
						i++;
					}
					i+=2;
				}
				else if(c=='\'') {
					int end=source.IndexOf('\'',i+(i+1<source.Length && source[i+1]=='\\' ? 3 : 2));
					end=(end<0 ? source.Length-1 : end);
					stringBuilder.Append("' '");
					i=end+1;
				}
				else if(c=='"') {
					bool isVerbatim=(i>0 && source[i-1]=='@');
					int start=i;
					i++;
					while(i<source.Length) {
						if(isVerbatim && source[i]=='"' && i+1<source.Length && source[i+1]=='"') {
							i+=2;
							continue;
						}
						if(!isVerbatim && source[i]=='\\') {
							i+=2;
							continue;
						}
						if(source[i]=='"') {
							break;
						}
						i++;
					}
					string literal=source.Substring(start,Math.Min(source.Length,i+1)-start);
					//Keep the short format literals the scan reads ("f2", "F", "G15"); blank the rest.
					if(Regex.IsMatch(literal,@"^""[A-Za-z0-9.#]{0,6}""$")) {
						stringBuilder.Append(literal);
					}
					else {
						stringBuilder.Append("\"\"");
					}
					i++;
				}
				else {
					stringBuilder.Append(c);
					i++;
				}
			}
			return stringBuilder.ToString();
		}
		#endregion Source scan

		///<summary>X837_5010.AmountToStrNoLeading before S3a-3.</summary>
		private static string OldAmountToStrNoLeading(double amount) {
			string result=amount.ToString("F");
			int i=0;
			while(i<result.Length-1 && result[i]=='0') {
				i++;
			}
			return result.Substring(i);
		}
		#endregion Before and after

		///<summary>Sets the thread's culture and puts it back.</summary>
		private class Culture:IDisposable {
			private readonly CultureInfo _cultureInfoOld=Thread.CurrentThread.CurrentCulture;

			public Culture(string name) {
				Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo(name);
			}

			public void Dispose() {
				Thread.CurrentThread.CurrentCulture=_cultureInfoOld;
			}
		}
	}
}
