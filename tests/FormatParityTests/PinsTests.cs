// The cases decision 24 of docs/specs/net10-plan.md names, pinned (S3b-FMT, docs/port-notes/S3b-FMT.md): each holds on .NET 10 only through
// the helpers; the record (net48-truth.txt) holds every one of them too.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Globalization;
using System.Text;
using CodeBase;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FormatParityTests {
	[TestClass]
	public class PinsTests {
		private static readonly CultureInfo _cultureInfoUs=CultureInfo.GetCultureInfo("en-US");
		private static readonly CultureInfo _cultureInfoInv=CultureInfo.InvariantCulture;
		private static readonly double _negativeZero=BitConverter.Int64BitsToDouble(long.MinValue);

		[TestMethod]
		public void HalfSteps_RoundAwayFromZero_AsOnTheNetFramework() {
			//(a) of the package: .NET formats the exact binary value for the standard formats; 1.005 is 1.00499999999999989...
			Assert.AreEqual("1.00",1.005.ToString("F2",_cultureInfoInv),"This is .NET 10's own text; the helper keeps Open Dental's 1.01.");
			Assert.AreEqual("1.01",1.005.ToStringNetFx("F2",_cultureInfoInv));
			Assert.AreEqual("1.01",1.005.ToStringNetFx("0.00",_cultureInfoInv));
			Assert.AreEqual("1.01",1.005.ToStringNetFx("N2",_cultureInfoInv));
			Assert.AreEqual("$1.01",1.005.ToStringNetFx("C",_cultureInfoUs));
			Assert.AreEqual("2.68",2.675.ToStringNetFx("F2",_cultureInfoInv));
			Assert.AreEqual("1.12",1.115.ToStringNetFx("F2",_cultureInfoInv));
			Assert.AreEqual("10.08",10.075.ToStringNetFx("F2",_cultureInfoInv));
			Assert.AreEqual("0.01",0.005.ToStringNetFx("F2",_cultureInfoInv));
			Assert.AreEqual("1,234,567.13",1234567.125.ToStringNetFx("N2",_cultureInfoInv),"An exact half: away from zero, as there.");
			Assert.AreEqual("100.00",99.995.ToStringNetFx("F2",_cultureInfoInv));
			Assert.AreEqual("-100.00",(-99.995).ToStringNetFx("F2",_cultureInfoInv));
		}

		[TestMethod]
		public void ValuesThatRoundToZero_HaveNoSign() {
			//(b): .NET writes "-0.00" and "($0.00)".
			Assert.AreEqual("-0.00",(-0.001).ToString("F2",_cultureInfoInv),"This is .NET 10's own text.");
			Assert.AreEqual("0.00",(-0.001).ToStringNetFx("F2",_cultureInfoInv));
			Assert.AreEqual("$0.00",(-0.001).ToStringNetFx("C",_cultureInfoUs));
			Assert.AreEqual("0.00",(-0.004).ToStringNetFx("0.00",_cultureInfoInv));
			Assert.AreEqual("0",_negativeZero.ToStringNetFx(_cultureInfoInv));
			Assert.AreEqual("0.00",_negativeZero.ToStringNetFx("N2",_cultureInfoInv));
			Assert.AreEqual("-0.01",(-0.005).ToStringNetFx("F2",_cultureInfoInv),"Rounds away from zero and keeps its sign.");
			Assert.AreEqual("($0.01)",(-0.005).ToStringNetFx("C",_cultureInfoUs));
		}

		[TestMethod]
		public void WithoutAFormat_FifteenDigits() {
			//(c): .NET writes the shortest text that round-trips.
			Assert.AreEqual("0.30000000000000004",(0.1+0.2).ToString(_cultureInfoInv),"This is .NET 10's own text.");
			Assert.AreEqual("0.3",(0.1+0.2).ToStringNetFx(_cultureInfoInv));
			Assert.AreEqual("0.333333333333333",(1/3.0).ToStringNetFx(_cultureInfoInv));
			Assert.AreEqual("-603.95",(42.11-646.06).ToStringNetFx(_cultureInfoInv));
			Assert.AreEqual("1E+15",1e15.ToStringNetFx(_cultureInfoInv));
			Assert.AreEqual("650217936677029",650217936677028.5.ToStringNetFx(_cultureInfoInv),"An exact half of the 15th digit: away from zero.");
			Assert.AreEqual("-650217936677029",(-650217936677028.5).ToStringNetFx(_cultureInfoInv));
			Assert.AreEqual("1.677722E+07",16777217f.ToStringNetFx(_cultureInfoInv),"A float: seven digits.");
			Assert.AreEqual("0.3",(0.1f+0.2f).ToStringNetFx(_cultureInfoInv));
			Assert.AreEqual("",((double?)null).ToStringNetFx());
			Assert.AreEqual("NaN",double.NaN.ToStringNetFx("F2",_cultureInfoInv));
			Assert.AreEqual("-Infinity",double.NegativeInfinity.ToStringNetFx("D",_cultureInfoInv),"The symbol for any format, as there.");
		}

		[TestMethod]
		public void CompositeFormatsAndInterpolation_UseTheHelper() {
			double value=0.1+0.2;
			Assert.AreEqual("0.3 1.01",string.Format(_cultureInfoInv,"{0} {1:F2}",value.AsNetFx(),1.005.AsNetFx()));
			Assert.AreEqual("[  1.01]",string.Format(_cultureInfoInv,"[{0,6:F2}]",1.005.AsNetFx()),"The alignment stays.");
			double? valueNull=null;
			Assert.AreEqual("x",string.Format("x{0}",valueNull.AsNetFx()));
			Assert.AreEqual("a0.3",string.Concat("a",value.AsNetFx()));
			FormattableString formattableString=$"{value.AsNetFx()}|{1.005.AsNetFx():N2}";
			Assert.AreEqual("0,3|1,01",formattableString.ToString(CultureInfo.GetCultureInfo("fr-FR")),"The culture of the formattable string applies.");
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.AppendFormat(_cultureInfoInv,"{0:0.00}",1.005.AsNetFx());
			Assert.AreEqual("1.01",stringBuilder.ToString());
			Assert.AreEqual("0.3",ODNumberText.ToStringNetFxIfNumber(value,_cultureInfoInv));
			Assert.AreEqual("abc",ODNumberText.ToStringNetFxIfNumber("abc",_cultureInfoInv));
			Assert.AreEqual("",ODNumberText.ToStringNetFxIfNumber(DBNull.Value,_cultureInfoInv));
		}

		[TestMethod]
		public void SOut_WritesTheNetFrameworkSql() {
			//GoldenOutputTests' SqlText_*: POut.Double of a value whose "0.00" text is on a half step.
			double value=BitConverter.Int64BitsToDouble(0x42f2f2a36ecd5555);
			Assert.AreEqual("333333333333333.00",SOut.Double(value));
			Assert.AreEqual("333333333333333",SOut.Double(value,false,true));
			Assert.AreEqual("0.00",SOut.Double(-0.001));
		}

		[TestMethod]
		public void DateTimeAdd_RoundsToTheMillisecond() {
			//(d): .NET 7 and later keep every tick.
			DateTime dateTime=new DateTime(2026,11,16,8,0,0);
			Assert.AreEqual(dateTime.AddTicks(5000),dateTime.AddMilliseconds(0.5),"This is .NET 10's own result: half a millisecond.");
			Assert.AreEqual(dateTime.AddTicks(1234567),dateTime.AddSeconds(0.1234567),"This is .NET 10's own result.");
			Assert.AreEqual(dateTime.AddMilliseconds(123),dateTime.AddSecondsNetFx(0.1234567),"Rounded to the millisecond, as there.");
			Assert.AreEqual(new DateTime(2026,11,16,14,40,0),dateTime.AddHoursNetFx(20/3d));
			Assert.AreEqual(new DateTime(2026,11,16,8,20,0),dateTime.AddMinutesNetFx(20/60d*60));
			Assert.AreEqual(dateTime.AddMilliseconds(1),dateTime.AddMillisecondsNetFx(0.5),"Half a millisecond: away from zero.");
			Assert.AreEqual(dateTime,dateTime.AddMillisecondsNetFx(0.4999));
			Assert.AreEqual(dateTime.AddMilliseconds(-1),dateTime.AddMillisecondsNetFx(-0.5));
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => dateTime.AddHoursNetFx(double.NaN));
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => dateTime.AddDaysNetFx(1e10));
			DateTimeOffset dateTimeOffset=new DateTimeOffset(dateTime,TimeSpan.FromHours(-5));
			Assert.AreEqual(new DateTimeOffset(new DateTime(2026,11,16,14,40,0),TimeSpan.FromHours(-5)),dateTimeOffset.AddHoursNetFx(20/3d));
		}
	}
}
