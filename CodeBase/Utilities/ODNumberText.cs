// Numbers written as text that a machine reads back (SQL, X12 837 files, claim files, signature keys), formatted so the text is the same on
// .NET Framework and on .NET (net10 plan, package S3a-3). .NET Core 3.0 changed two things: double.ToString() and float.ToString() give the
// shortest text that round-trips instead of 15 (double) or 7 (float) significant digits, and a negative number that rounds to zero keeps
// its sign ("-0.00", "-0"). .NET Framework always wrote a value that rounds to zero without a sign. Everything here is a no-op on net48.
// Since S3b-FMT (decision 24) every double and float the program writes as text goes through ToStringNetFx (ODNumberTextNetFx.cs).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Globalization;

namespace CodeBase {
	public static partial class ODNumberText {
		///<summary>value.ToString(format,formatProvider) as the .NET Framework wrote it (ToStringNetFx, ODNumberTextNetFx.cs): S3a-3 wrote a
		///negative value (or negative zero) whose text has no digit but zeros as zero, without a sign; since S3b-FMT (decision 24) the digits are
		///the .NET Framework's too (15 significant digits, rounded half away from zero, then the format). Example: -0.001 with "F2" gives "0.00"
		///and 1.005 with "F2" gives "1.01" on both runtimes (.NET gives "-0.00" and "1.00" otherwise).</summary>
		public static string ToStringNoNegativeZero(double value,string format,IFormatProvider formatProvider) {
			return ToStringNetFx(value,format,formatProvider);
		}

		///<summary>The float version of ToStringNoNegativeZero (7 significant digits).</summary>
		public static string ToStringNoNegativeZero(float value,string format,IFormatProvider formatProvider) {
			return ToStringNetFx(value,format,formatProvider);
		}

		///<summary>The text double.ToString(formatProvider) gives on .NET Framework: 15 significant digits ("G15", a value half way between two
		///15-digit numbers rounded away from zero), and no sign on zero. On .NET double.ToString() gives up to 17 digits (0.1+0.2 is
		///"0.30000000000000004" there, "0.3" here).</summary>
		public static string ToStringG15(double value,IFormatProvider formatProvider) {
			return ToStringNetFx(value,formatProvider);
		}

		///<summary>The text float.ToString(formatProvider) gives on .NET Framework: 7 significant digits ("G7"), and no sign on zero.</summary>
		public static string ToStringG7(float value,IFormatProvider formatProvider) {
			return ToStringNetFx(value,formatProvider);
		}

		///<summary>True for -0.0, which equals 0 but has its sign bit set.</summary>
		public static bool IsNegativeZero(double value) {
			return value==0 && BitConverter.DoubleToInt64Bits(value)!=0;
		}

		///<summary>True for -0.0f.</summary>
		public static bool IsNegativeZero(float value) {
			return IsNegativeZero((double)value);
		}

		///<summary>The parsed value as .NET Framework returned it. .NET Framework threw OverflowException for a number too large for the type,
		///and returned infinity only for the exact infinity symbol of the format (NumberFormatInfo.PositiveInfinitySymbol, NegativeInfinitySymbol,
		///after trimming white space); .NET returns infinity for both. .NET Framework never returned negative zero ("-0" and "-1e-400" gave 0);
		///.NET does. So: throws OverflowException for an infinity the text did not spell as the symbol, and gives 0 for negative zero.</summary>
		public static double GetParsedAsNetFramework(double value,string text,IFormatProvider formatProvider) {
			if(double.IsInfinity(value) && !IsInfinitySymbol(text,formatProvider)) {
				throw new OverflowException("The number is too large for a double.");
			}
			if(value==0) {
				return 0;//no negative zero
			}
			return value;
		}

		///<summary>The float version of GetParsedAsNetFramework. .NET Framework parsed a float as a double and then cast it, so a negative number
		///too small for a float (but not for a double, like -1e-320) gave negative zero there; that case stays negative zero.</summary>
		public static float GetParsedAsNetFramework(float value,string text,IFormatProvider formatProvider) {
			if(float.IsInfinity(value) && !IsInfinitySymbol(text,formatProvider)) {
				throw new OverflowException("The number is too large for a float.");
			}
			if(value==0) {
				if(IsNegativeZero(value)
					&& double.TryParse(text,NumberStyles.Float|NumberStyles.AllowThousands,formatProvider,out double valueDouble)
					&& valueDouble<0)
				{
					return value;
				}
				return 0;//no other negative zero
			}
			return value;
		}

		///<summary>True when the trimmed text is exactly the format's positive or negative infinity symbol, the only text .NET Framework parsed to
		///infinity.</summary>
		private static bool IsInfinitySymbol(string text,IFormatProvider formatProvider) {
			if(text==null) {
				return false;
			}
			NumberFormatInfo numberFormatInfo=NumberFormatInfo.GetInstance(formatProvider);
			string textTrimmed=text.Trim();
			return textTrimmed==numberFormatInfo.PositiveInfinitySymbol || textTrimmed==numberFormatInfo.NegativeInfinitySymbol;
		}
	}
}
