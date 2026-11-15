// S3a-3 (net10 plan): the ANSI code page named explicitly (ODEncodings), and machine-read numbers with the text .NET Framework gives
// (ODNumberText, SOut, SIn). Two kinds of tests: pins that hold on both runtimes (the text .NET would change, written as .NET Framework writes
// it), and before/after equality on fixture data, which compares the new code with the code it replaced on .NET Framework, where the
// replaced code is the record (on .NET these are inconclusive: that runtime gives different text on purpose).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CodeBase;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BINBTests {
	[TestClass]
	public class NumberTextTests {
		private static readonly double _negativeZero=BitConverter.Int64BitsToDouble(long.MinValue);
		private static readonly string[] _arrayCultureNames=new[] { "en-US","en-CA","fr-CA","de-DE","en-GB" };
		private CultureInfo _cultureInfoOld;

		[TestInitialize]
		public void TestInitialize() {
			_cultureInfoOld=Thread.CurrentThread.CurrentCulture;
		}

		[TestCleanup]
		public void TestCleanup() {
			Thread.CurrentThread.CurrentCulture=_cultureInfoOld;
		}

		#region Fixture data
		///<summary>True on .NET Framework, where the replaced code's output is the record the new code must equal.</summary>
		internal static bool IsNetFramework() {
			return typeof(object).Assembly.GetName().Name=="mscorlib";
		}

		internal static void RequireNetFramework() {
			if(!IsNetFramework()) {
				Assert.Inconclusive("Before/after equality is the .NET Framework record; on .NET the pins of this class hold instead.");
			}
		}

		///<summary>Amounts, the values around rounding to zero, the edges of the type, and 3000 seeded random values of every size.</summary>
		internal static List<double> GetFixtureDoubles() {
			List<double> listValues=new List<double> { 0,_negativeZero,0.001,-0.001,-0.004,-0.0049,-0.005,0.005,-0.00499999,0.1,0.2,0.1+0.2,1.0/3,
				-1.0/3,2.675,1.005,99.995,-99.995,125.5,-125.5,1234567.891,1e15,1e16,1e17,123456789012345678,1e-5,-1e-5,1e-20,-1e-20,
				double.Epsilon,-double.Epsilon,double.MaxValue,double.MinValue,double.NaN,double.PositiveInfinity,double.NegativeInfinity,
				0.07,19.99,-0.01,-0.009,4.35,0.285,1.1,2.2+1.1 };
			Random random=new Random(20261115);
			for(int i=0;i<3000;i++) {
				double value=random.NextDouble()*Math.Pow(10,random.Next(-8,17));
				if(i%3==0) {
					value=Math.Round(value,random.Next(0,5));
				}
				if(i%2==0) {
					value=-value;
				}
				listValues.Add(value);
			}
			return listValues;
		}

		internal static List<float> GetFixtureFloats() {
			List<float> listValues=new List<float> { 0,-0f*1,(float)_negativeZero,0.1f,1.1f,16777216f,16777217f,1.0000001f,-0.001f,3.4028235E+38f,
				float.Epsilon,float.NaN,float.PositiveInfinity,float.NegativeInfinity,12.5f,0.3f };
			Random random=new Random(1115);
			for(int i=0;i<2000;i++) {
				float value=(float)(random.NextDouble()*Math.Pow(10,random.Next(-6,10)));
				listValues.Add(i%2==0 ? -value : value);
			}
			return listValues;
		}

		private static string Bits(double value) {
			return BitConverter.DoubleToInt64Bits(value).ToString("X16");
		}

		private static string Bits(float value) {
			return BitConverter.ToString(BitConverter.GetBytes(value));
		}
		#endregion Fixture data

		#region Pins: the same text on both runtimes
		[TestMethod]
		public void Pins_NegativeZeroIsWrittenWithoutASign() {
			Assert.AreEqual("0.00",ODNumberText.ToStringNoNegativeZero(_negativeZero,"F2",CultureInfo.InvariantCulture));
			Assert.AreEqual("0.00",ODNumberText.ToStringNoNegativeZero(Math.Round(-0.004,2),"F2",CultureInfo.InvariantCulture));
			Assert.AreEqual("0.00",ODNumberText.ToStringNoNegativeZero(-0.004,"F2",CultureInfo.InvariantCulture));
			Assert.AreEqual("-0.01",ODNumberText.ToStringNoNegativeZero(-0.005,"F2",CultureInfo.InvariantCulture),"Rounds away from zero, keeps the sign.");
			Assert.AreEqual("0",ODNumberText.ToStringG15(_negativeZero,CultureInfo.InvariantCulture));
			Assert.AreEqual("-1E-20",ODNumberText.ToStringG15(-1e-20,CultureInfo.InvariantCulture),"Not zero: the sign stays.");
			Assert.AreEqual("0",ODNumberText.ToStringG7(-0f*1,CultureInfo.InvariantCulture));
			Assert.AreEqual("0.00",SOut.Double(-0.001));
			Assert.AreEqual("0.00",SOut.Double(_negativeZero,2));
			Assert.AreEqual("0",SOut.Double(_negativeZero,false,true));
			Assert.AreEqual("$0.00",ODNumberText.ToStringNoNegativeZero(-0.001,"C",CultureInfo.GetCultureInfo("en-US")));
			Assert.IsFalse(ODNumberText.IsNegativeZero(0));
			Assert.IsTrue(ODNumberText.IsNegativeZero(_negativeZero));
		}

		[TestMethod]
		public void Pins_FifteenDigitsForDoublesAndSevenForFloats() {
			Assert.AreEqual("0.3",ODNumberText.ToStringG15(0.1+0.2,CultureInfo.InvariantCulture),".NET writes 0.30000000000000004 for ToString().");
			Assert.AreEqual("0.333333333333333",ODNumberText.ToStringG15(1.0/3,CultureInfo.InvariantCulture));
			Assert.AreEqual("1E+15",ODNumberText.ToStringG15(1e15,CultureInfo.InvariantCulture));
			Assert.AreEqual("125.5",ODNumberText.ToStringG15(125.5,CultureInfo.InvariantCulture));
			Assert.AreEqual("125,5",ODNumberText.ToStringG15(125.5,CultureInfo.GetCultureInfo("fr-CA")));
			Assert.AreEqual("0.3",SOut.Double(0.1+0.2,false,true));
			Assert.AreEqual("1.677722E+07",ODNumberText.ToStringG7(16777217f,CultureInfo.InvariantCulture),".NET writes 16777216 for ToString().");
			Assert.AreEqual("1",SOut.Float(1.0000001f));
		}

		[TestMethod]
		public void Pins_SInTreatsOverflowAsInvalid() {
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
			Assert.AreEqual(0,SIn.Double("1e400"),"An overflow is invalid (0), as on .NET Framework; .NET parses it to infinity.");
			Assert.AreEqual(0,SIn.Double("-1e400"));
			Assert.AreEqual(0,SIn.Double("1e400",doUseEnUSFormat:true));
			Assert.ThrowsExactly<OverflowException>(() => SIn.Float("1e40"),"A float overflow throws, as on .NET Framework.");
			Assert.AreEqual(Bits(0d),Bits(SIn.Double("-0")),"Never negative zero.");
			Assert.AreEqual(Bits(0d),Bits(SIn.Double("-1e-400")));
			Assert.AreEqual(Bits(0f),Bits(SIn.Float("-0")));
			Assert.AreEqual(12.5,SIn.Double("12.5"));
			string symbol=CultureInfo.CurrentCulture.NumberFormat.PositiveInfinitySymbol;
			Assert.AreEqual(double.PositiveInfinity,SIn.Double(symbol),"The culture's infinity symbol is still infinity.");
			//What .NET returns for an overflow, handed to the check directly, so this holds on .NET Framework too.
			Assert.ThrowsExactly<OverflowException>(() => ODNumberText.GetParsedAsNetFramework(double.PositiveInfinity,"1e400",CultureInfo.CurrentCulture));
			Assert.ThrowsExactly<OverflowException>(() => ODNumberText.GetParsedAsNetFramework(double.NegativeInfinity,"-1e400",CultureInfo.CurrentCulture));
			Assert.ThrowsExactly<OverflowException>(() => ODNumberText.GetParsedAsNetFramework(float.PositiveInfinity,"1e40",CultureInfo.InvariantCulture));
			Assert.ThrowsExactly<OverflowException>(() => ODNumberText.GetParsedAsNetFramework(double.PositiveInfinity,"infinity",CultureInfo.InvariantCulture),
				".NET Framework took only the exact symbol.");
			Assert.AreEqual(double.PositiveInfinity,ODNumberText.GetParsedAsNetFramework(double.PositiveInfinity," "+symbol+" ",CultureInfo.CurrentCulture));
			Assert.AreEqual(double.NegativeInfinity,ODNumberText.GetParsedAsNetFramework(double.NegativeInfinity,
				CultureInfo.InvariantCulture.NumberFormat.NegativeInfinitySymbol,CultureInfo.InvariantCulture));
			Assert.AreEqual(Bits(0d),Bits(ODNumberText.GetParsedAsNetFramework(_negativeZero,"-0",CultureInfo.CurrentCulture)));
			Assert.IsTrue(double.IsNaN(ODNumberText.GetParsedAsNetFramework(double.NaN,"NaN",CultureInfo.CurrentCulture)));
		}

		[TestMethod]
		public void Pins_AnsiEncodingIsTheCulturesAnsiPage() {
			Dictionary<string,int> dictCodePages=new Dictionary<string,int> {
				{ "en-US",1252 },{ "en-CA",1252 },{ "fr-CA",1252 },{ "de-DE",1252 },{ "pl-PL",1250 },{ "ru-RU",1251 },{ "el-GR",1253 },{ "ja-JP",932 },
			};
			foreach(KeyValuePair<string,int> keyValuePair in dictCodePages) {
				Thread.CurrentThread.CurrentCulture=new CultureInfo(keyValuePair.Key);
				Assert.AreEqual(keyValuePair.Value,ODEncodings.GetAnsiEncoding().CodePage,keyValuePair.Key);
			}
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
			string text="café € – ü";
			CollectionAssert.AreEqual(Encoding.GetEncoding(1252).GetBytes(text),ODEncodings.GetAnsiEncoding().GetBytes(text));
			CollectionAssert.AreEqual(new byte[] { 0x63,0x61,0x66,0xE9,0x20,0x80,0x20,0x96,0x20,0xFC },ODEncodings.GetAnsiEncoding().GetBytes(text));
		}
		#endregion Pins

		#region Before and after (the .NET Framework record)
		[TestMethod]
		public void BeforeAfter_AnsiEncodingIsEncodingDefault() {
			RequireNetFramework();
			//Encoding.Default is the system locale's ANSI page; the new code takes the culture's. They agree whenever the two share their page,
			//as for every culture of the office. The test runs in the computer's own culture and in each culture with the system's page.
			if(CultureInfo.CurrentCulture.TextInfo.ANSICodePage!=Encoding.Default.CodePage) {
				Assert.Inconclusive("This computer's culture and system locale have different ANSI pages.");
			}
			Assert.AreEqual(Encoding.Default.CodePage,ODEncodings.GetAnsiEncoding().CodePage);
			byte[] arrayAll=Enumerable.Range(0,256).Select(x => (byte)x).ToArray();
			string textAll=Encoding.Default.GetString(arrayAll);
			Assert.AreEqual(textAll,ODEncodings.GetAnsiEncoding().GetString(arrayAll));
			CollectionAssert.AreEqual(Encoding.Default.GetBytes(textAll),ODEncodings.GetAnsiEncoding().GetBytes(textAll));
			foreach(CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.SpecificCultures)) {
				if(cultureInfo.TextInfo.ANSICodePage!=Encoding.Default.CodePage) {
					continue;
				}
				Thread.CurrentThread.CurrentCulture=cultureInfo;
				Assert.AreEqual(textAll,ODEncodings.GetAnsiEncoding().GetString(arrayAll),cultureInfo.Name);
			}
		}

		[TestMethod]
		public void BeforeAfter_NoNegativeZero_EqualsToString() {
			RequireNetFramework();
			string[] arrayFormats=new[] { "F2","f2","f","F","N2","C","0.00","0.00#","0.00##","G15","G" };
			List<double> listValues=GetFixtureDoubles();
			List<CultureInfo> listCultures=_arrayCultureNames.Select(x => CultureInfo.GetCultureInfo(x)).Concat(new[] { CultureInfo.InvariantCulture }).ToList();
			int count=0;
			foreach(CultureInfo cultureInfo in listCultures) {
				foreach(string format in arrayFormats) {
					foreach(double value in listValues) {
						Assert.AreEqual(value.ToString(format,cultureInfo),ODNumberText.ToStringNoNegativeZero(value,format,cultureInfo),
							value.ToString("R",CultureInfo.InvariantCulture)+" "+format+" "+cultureInfo.Name);
						count++;
					}
				}
				foreach(double value in listValues) {
					Assert.AreEqual(value.ToString(cultureInfo),ODNumberText.ToStringG15(value,cultureInfo),value.ToString("R",CultureInfo.InvariantCulture));
				}
				foreach(float value in GetFixtureFloats()) {
					Assert.AreEqual(value.ToString(cultureInfo),ODNumberText.ToStringG7(value,cultureInfo),value.ToString("R",CultureInfo.InvariantCulture));
					Assert.AreEqual(value.ToString("F2",cultureInfo),ODNumberText.ToStringNoNegativeZero(value,"F2",cultureInfo));
				}
			}
			Assert.IsTrue(count>100000);
		}

		[TestMethod]
		public void BeforeAfter_SOut_EqualsTheReplacedCode() {
			RequireNetFramework();
			NumberFormatInfo numberFormatInfoEnUs=new NumberFormatInfo();
			numberFormatInfoEnUs.NumberDecimalSeparator=".";
			numberFormatInfoEnUs.NumberGroupSeparator=",";
			foreach(string cultureName in _arrayCultureNames) {
				Thread.CurrentThread.CurrentCulture=new CultureInfo(cultureName);
				foreach(double value in GetFixtureDoubles()) {
					string message=value.ToString("R",CultureInfo.InvariantCulture)+" "+cultureName;
					Assert.AreEqual(value.ToString("f",CultureInfo.InvariantCulture),SOut.Double(value),message);
					Assert.AreEqual(value.ToString(numberFormatInfoEnUs),SOut.Double(value,false,true),message);
					Assert.AreEqual(value.ToString(),SOut.Double(value,false,false),message);
					Assert.AreEqual(value.ToString("0.00",CultureInfo.InvariantCulture),SOut.Double(value,2),message);
					Assert.AreEqual(value.ToString("0.00#",CultureInfo.InvariantCulture),SOut.Double(value,3),message);
					Assert.AreEqual(value.ToString("0.00##",CultureInfo.InvariantCulture),SOut.Double(value,4),message);
				}
				foreach(float value in GetFixtureFloats()) {
					Assert.AreEqual(value.ToString(CultureInfo.InvariantCulture),SOut.Float(value),value.ToString("R",CultureInfo.InvariantCulture));
				}
			}
		}

		[TestMethod]
		public void BeforeAfter_SIn_EqualsTheReplacedCode() {
			RequireNetFramework();
			List<string> listTexts=new List<string> { "0","-0","-0.00","1.5","1,5","-1,5","1e400","-1e400","1e-400","-1e-400","1E308","1.8E308","abc",
				" 12.5 ","1,234.5","1.234,5","Infinity","-Infinity","infinity","∞","-∞","+∞"," ∞ ","NaN","1e40","-1e40","3.4E38",
				"3.5E38","1e-50"," ","$5" };
			foreach(double value in GetFixtureDoubles().Take(400)) {
				listTexts.Add(value.ToString(CultureInfo.InvariantCulture));
				listTexts.Add(value.ToString("R",CultureInfo.InvariantCulture));
			}
			foreach(string cultureName in _arrayCultureNames) {
				Thread.CurrentThread.CurrentCulture=new CultureInfo(cultureName);
				listTexts.Add(CultureInfo.CurrentCulture.NumberFormat.PositiveInfinitySymbol);
				listTexts.Add(CultureInfo.CurrentCulture.NumberFormat.NegativeInfinitySymbol);
				listTexts.Add(CultureInfo.CurrentCulture.NumberFormat.NaNSymbol);
				foreach(string text in listTexts) {
					string message="'"+text+"' "+cultureName;
					Assert.AreEqual(Bits(OldSInDouble(text,false)),Bits(SIn.Double(text)),message);
					Assert.AreEqual(Bits(OldSInDouble(text,true)),Bits(SIn.Double(text,true)),message);
					Assert.AreEqual(RunOldOrNew(() => Bits(OldSInFloat(text))),RunOldOrNew(() => Bits(SIn.Float(text))),message);
				}
			}
		}

		///<summary>SIn.Double before S3a-3.</summary>
		private static double OldSInDouble(string myString,bool doUseEnUSFormat) {
			if(myString=="") {
				return 0;
			}
			try {
				if(doUseEnUSFormat) {
					NumberFormatInfo format=new NumberFormatInfo();
					format.NumberDecimalSeparator=".";
					format.NumberGroupSeparator=",";
					return System.Convert.ToDouble(myString,format);
				}
				return System.Convert.ToDouble(myString);
			}
			catch(Exception) {
				return 0;
			}
		}

		///<summary>SIn.Float before S3a-3.</summary>
		private static float OldSInFloat(string myString) {
			if(myString=="") {
				return 0;
			}
			try {
				return System.Convert.ToSingle(myString);
			}
			catch(Exception) {
				return System.Convert.ToSingle(myString,CultureInfo.InvariantCulture);
			}
		}

		///<summary>The result, or the type of the exception thrown.</summary>
		private static string RunOldOrNew(Func<string> func) {
			try {
				return func();
			}
			catch(Exception ex) {
				return ex.GetType().Name;
			}
		}
		#endregion Before and after
	}
}
