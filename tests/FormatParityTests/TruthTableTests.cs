// Every row of the .NET Framework 4.8 record (tests\FormatParity\net48-truth.txt, Make-Net48Truth.ps1) against the helpers on .NET 10
// (S3b-FMT, decision 24 of docs/specs/net10-plan.md, docs/port-notes/S3b-FMT.md).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FormatParityTests {
	[TestClass]
	public class TruthTableTests {
		private static TruthTable _truthTable;

		///<summary>The record, read once: formats, number rows (with the x64 .NET Framework's text where it differs from the x86 one) and time
		///rows.</summary>
		internal class TruthTable {
			public Dictionary<int,string> DictFormats=new Dictionary<int,string>();
			public List<string[]> ListNumberRows=new List<string[]>();
			///<summary>Key "D|bits|culture|index" to the x64 text where it differs.</summary>
			public Dictionary<string,string> DictAlternatives=new Dictionary<string,string>();
			public List<string[]> ListTimeRows=new List<string[]>();
		}

		internal static TruthTable GetTruthTable() {
			if(_truthTable!=null) {
				return _truthTable;
			}
			string path=Path.Combine(AppContext.BaseDirectory,"FormatParity","net48-truth.txt");
			Assert.IsTrue(File.Exists(path),"The record is copied next to the tests: "+path);
			TruthTable truthTable=new TruthTable();
			foreach(string line in File.ReadLines(path)) {
				if(line.Length==0 || line.StartsWith("#")) {
					continue;
				}
				string[] arrayParts=line.Split('\t');
				switch(arrayParts[0]) {
					case "F":
						truthTable.DictFormats[int.Parse(arrayParts[1],CultureInfo.InvariantCulture)]=Unescape(arrayParts[2]);
						break;
					case "N":
						truthTable.ListNumberRows.Add(arrayParts);
						break;
					case "X":
						truthTable.DictAlternatives[arrayParts[1]+"|"+arrayParts[2]+"|"+arrayParts[3]+"|"+arrayParts[4]]=Unescape(arrayParts[5]);
						break;
					case "T":
						truthTable.ListTimeRows.Add(arrayParts);
						break;
					default:
						Assert.Fail("Unknown line: "+line);
						break;
				}
			}
			_truthTable=truthTable;
			return truthTable;
		}

		internal static string Unescape(string text) {
			StringBuilder stringBuilder=new StringBuilder();
			for(int i=0;i<text.Length;i++) {
				if(text[i]=='\\' && i+1<text.Length && text[i+1]=='\\') {
					stringBuilder.Append('\\');
					i++;
				}
				else if(text[i]=='\\' && i+5<text.Length && text[i+1]=='u') {
					stringBuilder.Append((char)Convert.ToInt32(text.Substring(i+2,4),16));
					i+=5;
				}
				else {
					stringBuilder.Append(text[i]);
				}
			}
			return stringBuilder.ToString();
		}

		private static CultureInfo GetCulture(string name) {
			return name=="inv" ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(name);
		}

		///<summary>The format of a row: null for &lt;none&gt; and &lt;null&gt;, "" for &lt;empty&gt;.</summary>
		private static string GetFormat(string format) {
			if(format=="<none>" || format=="<null>") {
				return null;
			}
			if(format=="<empty>") {
				return "";
			}
			return format;
		}

		private static string Run(Func<string> func) {
			try {
				return func();
			}
			catch(Exception ex) {
				return "EXC:"+ex.GetType().Name;
			}
		}

		private static bool IsRoundTrip(string format) {
			return format=="R" || format=="r";
		}

		[TestMethod]
		public void Record_HasTheRowsItWasMadeWith() {
			TruthTable truthTable=GetTruthTable();
			Assert.IsTrue(truthTable.DictFormats.Count>=100,"formats: "+truthTable.DictFormats.Count);
			Assert.IsTrue(truthTable.ListNumberRows.Count>60000,"number rows: "+truthTable.ListNumberRows.Count);
			Assert.IsTrue(truthTable.ListTimeRows.Count>2000,"time rows: "+truthTable.ListTimeRows.Count);
			Assert.IsTrue(truthTable.DictFormats.Values.Contains("0.00") && truthTable.DictFormats.Values.Contains("c")
				&& truthTable.DictFormats.Values.Contains("<none>"));
			//The record differs from .NET 10's own text in thousands of rows: it is the .NET Framework's.
			int countDiffering=0;
			foreach(string[] arrayParts in truthTable.ListNumberRows.Where(x => x[1]=="D")) {
				double value=BitConverter.Int64BitsToDouble(long.Parse(arrayParts[2],NumberStyles.HexNumber,CultureInfo.InvariantCulture));
				string format=truthTable.DictFormats[int.Parse(arrayParts[4],CultureInfo.InvariantCulture)];
				CultureInfo cultureInfo=GetCulture(arrayParts[3]);
				string textNet=Run(() => format=="<none>" ? value.ToString(cultureInfo) : value.ToString(GetFormat(format),cultureInfo));
				if(textNet!=Unescape(arrayParts[5])) {
					countDiffering++;
				}
			}
			Assert.IsTrue(countDiffering>1000,".NET 10's own text differs from the record in "+countDiffering+" double rows.");
		}

		///<summary>Every double and float row: ToStringNetFx gives the .NET Framework's text (the x86 runtime's, or the x64 runtime's where the two
		///differ: the 17th digit of a few G17 and E16 texts). "R" is the one exact format: the helper gives .NET's shortest round-trip text (for
		///negative zero "0", as the .NET Framework), and both texts read back to the value.</summary>
		[TestMethod]
		public void NumberRows_ToStringNetFx_GivesTheNetFrameworkText() {
			TruthTable truthTable=GetTruthTable();
			List<string> listFailures=new List<string>();
			int countChecked=0;
			int countRoundTrip=0;
			foreach(string[] arrayParts in truthTable.ListNumberRows) {
				string formatRow=truthTable.DictFormats[int.Parse(arrayParts[4],CultureInfo.InvariantCulture)];
				string format=GetFormat(formatRow);
				CultureInfo cultureInfo=GetCulture(arrayParts[3]);
				string expected=Unescape(arrayParts[5]);
				string actual;
				string textNet;
				double valueAsDouble;
				if(arrayParts[1]=="D") {
					double value=BitConverter.Int64BitsToDouble(long.Parse(arrayParts[2],NumberStyles.HexNumber,CultureInfo.InvariantCulture));
					valueAsDouble=value;
					actual=Run(() => formatRow=="<none>" ? value.ToStringNetFx(cultureInfo) : value.ToStringNetFx(format,cultureInfo));
					textNet=Run(() => value.ToString(format,cultureInfo));
				}
				else {
					float value=BitConverter.Int32BitsToSingle(int.Parse(arrayParts[2],NumberStyles.HexNumber,CultureInfo.InvariantCulture));
					valueAsDouble=value;
					actual=Run(() => formatRow=="<none>" ? value.ToStringNetFx(cultureInfo) : value.ToStringNetFx(format,cultureInfo));
					textNet=Run(() => value.ToString(format,cultureInfo));
				}
				countChecked++;
				if(IsRoundTrip(formatRow)) {
					countRoundTrip++;
					if(actual!=textNet && actual!=expected) {//.NET's text, or the .NET Framework's where the helper's zero rule gives it ("0" for -0)
						listFailures.Add("R is .NET's: "+string.Join(" ",arrayParts)+" got ["+actual+"] .NET ["+textNet+"]");
					}
					else if(!double.IsNaN(valueAsDouble) && !double.IsInfinity(valueAsDouble)) {
						bool isFloat=arrayParts[1]=="S";
						double readActual=isFloat ? float.Parse(actual,cultureInfo) : double.Parse(actual,cultureInfo);
						double readExpected=isFloat ? float.Parse(expected,cultureInfo) : double.Parse(expected,cultureInfo);
						if(readActual!=valueAsDouble || readExpected!=valueAsDouble) {
							listFailures.Add("R round trip: "+string.Join(" ",arrayParts)+" got ["+actual+"]");
						}
					}
					continue;
				}
				if(actual==expected) {
					continue;
				}
				string alternative;
				if(truthTable.DictAlternatives.TryGetValue(arrayParts[1]+"|"+arrayParts[2]+"|"+arrayParts[3]+"|"+arrayParts[4],out alternative)
					&& actual==alternative)
				{
					continue;
				}
				listFailures.Add(string.Join(" ",arrayParts.Take(5))+" ["+formatRow+"] expected ["+expected+"] got ["+actual+"]");
			}
			Assert.IsTrue(countChecked>60000 && countRoundTrip>1000,"rows "+countChecked+", R rows "+countRoundTrip);
			Assert.AreEqual(0,listFailures.Count,listFailures.Count+" rows differ:\r\n"+string.Join("\r\n",listFailures.Take(40)));
		}

		///<summary>The same rows through AsNetFx in a composite format and an interpolated string: the format item's format and the composite's
		///culture reach the helper (a sample of every 7th row, every format).</summary>
		[TestMethod]
		public void NumberRows_AsNetFx_InCompositeFormatsAndInterpolation() {
			TruthTable truthTable=GetTruthTable();
			List<string> listFailures=new List<string>();
			int countChecked=0;
			for(int i=0;i<truthTable.ListNumberRows.Count;i+=7) {
				string[] arrayParts=truthTable.ListNumberRows[i];
				string formatRow=truthTable.DictFormats[int.Parse(arrayParts[4],CultureInfo.InvariantCulture)];
				if(IsRoundTrip(formatRow) || formatRow=="<empty>" || formatRow.Contains("{") || formatRow.Contains("}")) {
					continue;
				}
				string format=GetFormat(formatRow);
				CultureInfo cultureInfo=GetCulture(arrayParts[3]);
				object argument;
				if(arrayParts[1]=="D") {
					argument=BitConverter.Int64BitsToDouble(long.Parse(arrayParts[2],NumberStyles.HexNumber,CultureInfo.InvariantCulture)).AsNetFx();
				}
				else {
					argument=BitConverter.Int32BitsToSingle(int.Parse(arrayParts[2],NumberStyles.HexNumber,CultureInfo.InvariantCulture)).AsNetFx();
				}
				string expected=Unescape(arrayParts[5]);
				string alternative;
				truthTable.DictAlternatives.TryGetValue(arrayParts[1]+"|"+arrayParts[2]+"|"+arrayParts[3]+"|"+arrayParts[4],out alternative);
				string composite=format==null ? "{0}" : "{0:"+format+"}";
				string actual=Run(() => string.Format(cultureInfo,composite,argument));
				if(actual!=expected && actual!=alternative) {
					listFailures.Add("Format "+string.Join(" ",arrayParts.Take(5))+" expected ["+expected+"] got ["+actual+"]");
				}
				countChecked++;
			}
			Assert.IsTrue(countChecked>5000,"rows "+countChecked);
			Assert.AreEqual(0,listFailures.Count,string.Join("\r\n",listFailures.Take(40)));
		}

		///<summary>Every time row: ODDateTimeAdd's AddXNetFx and ODTimeSpan's FromX give the .NET Framework's ticks, or its exception.</summary>
		[TestMethod]
		public void TimeRows_GiveTheNetFrameworkTicks() {
			TruthTable truthTable=GetTruthTable();
			DateTime dateTime=new DateTime(2026,11,16,8,0,0);
			DateTimeOffset dateTimeOffset=new DateTimeOffset(dateTime,TimeSpan.Zero);
			List<string> listFailures=new List<string>();
			int countDifferingOnNet=0;
			foreach(string[] arrayParts in truthTable.ListTimeRows) {
				double value=BitConverter.Int64BitsToDouble(long.Parse(arrayParts[2],NumberStyles.HexNumber,CultureInfo.InvariantCulture));
				Func<long> funcHelper;
				Func<long> funcNet;
				switch(arrayParts[1]) {
					case "DateTime.AddDays": funcHelper=() => dateTime.AddDaysNetFx(value).Ticks; funcNet=() => dateTime.AddDays(value).Ticks; break;
					case "DateTime.AddHours": funcHelper=() => dateTime.AddHoursNetFx(value).Ticks; funcNet=() => dateTime.AddHours(value).Ticks; break;
					case "DateTime.AddMinutes": funcHelper=() => dateTime.AddMinutesNetFx(value).Ticks; funcNet=() => dateTime.AddMinutes(value).Ticks; break;
					case "DateTime.AddSeconds": funcHelper=() => dateTime.AddSecondsNetFx(value).Ticks; funcNet=() => dateTime.AddSeconds(value).Ticks; break;
					case "DateTime.AddMilliseconds": funcHelper=() => dateTime.AddMillisecondsNetFx(value).Ticks; funcNet=() => dateTime.AddMilliseconds(value).Ticks; break;
					case "DateTimeOffset.AddDays": funcHelper=() => dateTimeOffset.AddDaysNetFx(value).UtcTicks; funcNet=() => dateTimeOffset.AddDays(value).UtcTicks; break;
					case "DateTimeOffset.AddHours": funcHelper=() => dateTimeOffset.AddHoursNetFx(value).UtcTicks; funcNet=() => dateTimeOffset.AddHours(value).UtcTicks; break;
					case "DateTimeOffset.AddMinutes": funcHelper=() => dateTimeOffset.AddMinutesNetFx(value).UtcTicks; funcNet=() => dateTimeOffset.AddMinutes(value).UtcTicks; break;
					case "DateTimeOffset.AddSeconds": funcHelper=() => dateTimeOffset.AddSecondsNetFx(value).UtcTicks; funcNet=() => dateTimeOffset.AddSeconds(value).UtcTicks; break;
					case "DateTimeOffset.AddMilliseconds": funcHelper=() => dateTimeOffset.AddMillisecondsNetFx(value).UtcTicks; funcNet=() => dateTimeOffset.AddMilliseconds(value).UtcTicks; break;
					case "TimeSpan.FromDays": funcHelper=() => ODTimeSpan.FromDays(value).Ticks; funcNet=() => TimeSpan.FromDays(value).Ticks; break;
					case "TimeSpan.FromHours": funcHelper=() => ODTimeSpan.FromHours(value).Ticks; funcNet=() => TimeSpan.FromHours(value).Ticks; break;
					case "TimeSpan.FromMinutes": funcHelper=() => ODTimeSpan.FromMinutes(value).Ticks; funcNet=() => TimeSpan.FromMinutes(value).Ticks; break;
					case "TimeSpan.FromSeconds": funcHelper=() => ODTimeSpan.FromSeconds(value).Ticks; funcNet=() => TimeSpan.FromSeconds(value).Ticks; break;
					case "TimeSpan.FromMilliseconds": funcHelper=() => ODTimeSpan.FromMilliseconds(value).Ticks; funcNet=() => TimeSpan.FromMilliseconds(value).Ticks; break;
					default:
						Assert.Fail("Unknown method: "+arrayParts[1]);
						return;
				}
				string actual=Run(() => funcHelper().ToString(CultureInfo.InvariantCulture));
				string textNet=Run(() => funcNet().ToString(CultureInfo.InvariantCulture));
				if(textNet!=arrayParts[3]) {
					countDifferingOnNet++;
				}
				if(actual!=arrayParts[3]) {
					listFailures.Add(string.Join(" ",arrayParts)+" got "+actual);
				}
			}
			Assert.IsTrue(truthTable.ListTimeRows.Count>2000);
			Assert.IsTrue(countDifferingOnNet>500,".NET 10's own results differ from the record in "+countDifferingOnNet+" rows.");
			Assert.AreEqual(0,listFailures.Count,string.Join("\r\n",listFailures.Take(40)));
		}
	}
}
