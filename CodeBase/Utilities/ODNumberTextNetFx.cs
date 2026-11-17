// Doubles and floats written as text the way the .NET Framework wrote them (decision 24 of docs/specs/net10-plan.md, owner 2026-11-16: "Keep
// Open Dental's 1.01"; package S3b-FMT, docs/port-notes/S3b-FMT.md). The .NET Framework turned a double into 15 significant digits (a float
// into 7), rounded half away from zero (the x86 runtime, whose text Open Dental wrote, from about 17 digits it had computed: GetDigits), and
// then applied the format to those digits; .NET (Core 3.0 and later) formats the exact binary value
// for the standard formats (F, N, C, P, E: 1.005 with "F2" is 1.00 there, 1.01 here), rounds a value exactly half way between two 15-digit
// numbers to the even one for the custom formats and "G15" (650217936677028.5 is ...028 there, ...029 here), writes the shortest text that
// round-trips without a format (0.1+0.2 is 0.30000000000000004 there, 0.3 here), and keeps the sign of a negative value that rounds to zero
// ("-0.00", "($0.00)" there, "0.00", "$0.00" here). tests\FormatParity\net48-truth.txt is the .NET Framework 4.8 record these methods are
// tested against (FormatParityTests). Every place of the shipped code that writes a double or float as text calls these (tools\FormatSweep
// finds them; the guard test fails on a new one).
//
// This file is compiled into CodeBase (public) and linked into the shipped projects that do not reference CodeBase (internal there, with
// OD_NUMBERTEXT_LINKED defined), so it uses nothing else of CodeBase. C# 7.3.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace CodeBase {
#if OD_NUMBERTEXT_LINKED
	internal
#else
	public
#endif
	static partial class ODNumberText {
		///<summary>Significant digits of a double before a format is applied, as on the .NET Framework (DOUBLE_PRECISION).</summary>
		private const int PRECISION_DOUBLE=15;
		///<summary>Significant digits of a float (FLOAT_PRECISION).</summary>
		private const int PRECISION_FLOAT=7;
		///<summary>Significant digits of a double for "G16", "G17" and "E15" and longer: 17. The .NET Framework's x64 runtime gave them correctly
		///rounded, its x86 runtime not always (the 17th digit of 0.0001 is 1 there); these are the correctly rounded ones.</summary>
		private const int PRECISION_DOUBLE_LONG=17;
		///<summary>Significant digits of a float for "G8" and longer and "E7" and longer: 9.</summary>
		private const int PRECISION_FLOAT_LONG=9;
		///<summary>The largest scale of a decimal, and the most digits a decimal holds without loss.</summary>
		private const int DECIMAL_MAX_SCALE=28;

		#region Double
		///<summary>value.ToString() as the .NET Framework wrote it: 15 significant digits ("G15"), in the current culture.</summary>
		public static string ToStringNetFx(this double value) {
			return FormatNetFx(value,null,null,false);
		}

		///<summary>value.ToString(format) as the .NET Framework wrote it, in the current culture.</summary>
		public static string ToStringNetFx(this double value,string format) {
			return FormatNetFx(value,format,null,false);
		}

		///<summary>value.ToString(formatProvider) as the .NET Framework wrote it.</summary>
		public static string ToStringNetFx(this double value,IFormatProvider formatProvider) {
			return FormatNetFx(value,null,formatProvider,false);
		}

		///<summary>value.ToString(format,formatProvider) as the .NET Framework wrote it: the value rounded half away from zero to 15 significant
		///digits (17 for "G16", "G17", "E15" and longer, which are written as .NET writes them: exact), then the format applied to those digits,
		///and no sign on a value that is written as zero. "R" is written as .NET writes it (the shortest text that round-trips).</summary>
		public static string ToStringNetFx(this double value,string format,IFormatProvider formatProvider) {
			return FormatNetFx(value,format,formatProvider,false);
		}

		///<summary>Nullable&lt;double&gt;.ToString() as the .NET Framework wrote it: "" for null.</summary>
		public static string ToStringNetFx(this double? value) {
			if(value==null) {
				return "";
			}
			return FormatNetFx(value.Value,null,null,false);
		}
		#endregion Double

		#region Float
		///<summary>value.ToString() as the .NET Framework wrote it: 7 significant digits ("G7"), in the current culture.</summary>
		public static string ToStringNetFx(this float value) {
			return FormatNetFx(value,null,null,true);
		}

		///<summary>value.ToString(format) as the .NET Framework wrote it, in the current culture.</summary>
		public static string ToStringNetFx(this float value,string format) {
			return FormatNetFx(value,format,null,true);
		}

		///<summary>value.ToString(formatProvider) as the .NET Framework wrote it.</summary>
		public static string ToStringNetFx(this float value,IFormatProvider formatProvider) {
			return FormatNetFx(value,null,formatProvider,true);
		}

		///<summary>value.ToString(format,formatProvider) as the .NET Framework wrote it: 7 significant digits (9 for "G8" and longer and "E7" and
		///longer), then the format. "R" as .NET writes it.</summary>
		public static string ToStringNetFx(this float value,string format,IFormatProvider formatProvider) {
			return FormatNetFx(value,format,formatProvider,true);
		}

		///<summary>Nullable&lt;float&gt;.ToString() as the .NET Framework wrote it: "" for null.</summary>
		public static string ToStringNetFx(this float? value) {
			if(value==null) {
				return "";
			}
			return FormatNetFx(value.Value,null,null,true);
		}
		#endregion Float

		#region Composite formatting
		///<summary>The double as an argument of a composite format (string.Format, StringBuilder.AppendFormat, TextWriter.Write, an interpolated
		///string): its text, with or without a format item's format, is the .NET Framework's, in the culture the composite format uses.</summary>
		public static NetFxNumber AsNetFx(this double value) {
			return new NetFxNumber(value,false);
		}

		///<summary>The float as an argument of a composite format; see the double version.</summary>
		public static NetFxNumber AsNetFx(this float value) {
			return new NetFxNumber(value,true);
		}

		///<summary>The nullable double as an argument of a composite format: null stays null (written as ""), a value as the double version.</summary>
		public static object AsNetFx(this double? value) {
			if(value==null) {
				return null;
			}
			return new NetFxNumber(value.Value,false);
		}

		///<summary>The nullable float as an argument of a composite format; see the nullable double version.</summary>
		public static object AsNetFx(this float? value) {
			if(value==null) {
				return null;
			}
			return new NetFxNumber(value.Value,true);
		}

		///<summary>An argument as string.Format would format it, with a boxed double or float written as the .NET Framework wrote it and any
		///other value as it is: for the places that hand an object of unknown type to a composite format or to ToString (a data table's cell).
		///Null gives null, as string.Format's arguments allow.</summary>
		public static object AsNetFxIfNumber(object value) {
			if(value is double) {
				return new NetFxNumber((double)value,false);
			}
			if(value is float) {
				return new NetFxNumber((float)value,true);
			}
			return value;
		}

		///<summary>value.ToString() for an object of unknown type, with a double or float written as the .NET Framework wrote it (current culture):
		///a data table cell whose column may hold doubles (a user query's sum, a report table's computed column), for one. Throws
		///NullReferenceException for null, as value.ToString() does.</summary>
		public static string ToStringNetFxIfNumber(object value) {
			if(value is double) {
				return FormatNetFx((double)value,null,null,false);
			}
			if(value is float) {
				return FormatNetFx((float)value,null,null,true);
			}
			return value.ToString();
		}

		///<summary>Convert.ToString(value,formatProvider) for an object of unknown type, with a double or float written as the .NET Framework
		///wrote it: a data table cell whose column may hold doubles, for one.</summary>
		public static string ToStringNetFxIfNumber(object value,IFormatProvider formatProvider) {
			if(value is double) {
				return FormatNetFx((double)value,null,formatProvider,false);
			}
			if(value is float) {
				return FormatNetFx((float)value,null,formatProvider,true);
			}
			return Convert.ToString(value,formatProvider);
		}
		#endregion Composite formatting

		#region The formatter
		///<summary>A double or float that formats itself as the .NET Framework did: string.Format, StringBuilder.AppendFormat and interpolated
		///strings call IFormattable.ToString with the format item's format and their culture; ToString() and string concatenation call
		///ToString().</summary>
		public struct NetFxNumber : IFormattable {
			private readonly double _value;
			private readonly bool _isFloat;

			public NetFxNumber(double value,bool isFloat) {
				_value=value;
				_isFloat=isFloat;
			}

			public double Value {
				get { return _value; }
			}

			public string ToString(string format,IFormatProvider formatProvider) {
				return FormatNetFx(_value,format,formatProvider,_isFloat);
			}

			public override string ToString() {
				return FormatNetFx(_value,null,null,_isFloat);
			}
		}

		///<summary>The digits of a value rounded to some significant digits: value = 0.Digits * 10^Scale, Digits without trailing zeros ("" for
		///zero), as the .NET Framework's NUMBER.</summary>
		private struct NetFxDigits {
			public bool IsNegative;
			public string Digits;
			public int Scale;
		}

		///<summary>The .NET Framework's FormatDouble and FormatSingle (COMNumber, clr\src\classlibnative\bcltype\number.cpp of .NET Framework
		///4.8): which digits, then NumberToString or NumberToStringFormat on them. The digits are formatted as a decimal holding exactly those
		///digits, which .NET formats with the same NumberToString and NumberToStringFormat (and without a sign on zero); a value whose digits a
		///decimal cannot hold (beyond 7.9E+28, or digits past the 28th decimal place) is formatted here ("G", "E"), as a BigInteger (the other
		///formats, large values) or rounded to 28 decimal places (small values).</summary>
		private static string FormatNetFx(double value,string format,IFormatProvider formatProvider,bool isFloat) {
			if(double.IsNaN(value) || double.IsInfinity(value)) {
				//The symbol, whatever the format (even one a double does not take, such as "D"), as the .NET Framework wrote it.
				NumberFormatInfo numberFormatInfoSymbols=NumberFormatInfo.GetInstance(formatProvider);
				if(double.IsNaN(value)) {
					return numberFormatInfoSymbols.NaNSymbol;
				}
				return value>0 ? numberFormatInfoSymbols.PositiveInfinitySymbol : numberFormatInfoSymbols.NegativeInfinitySymbol;
			}
			int digits;
			char fmt=ParseFormatSpecifier(format,out digits);
			int precision=isFloat ? PRECISION_FLOAT : PRECISION_DOUBLE;
			char fmtUpper=(char)(fmt & 0xFFDF);
			switch(fmtUpper) {
				case 'R':
					//The .NET Framework wrote 15 (7) digits if they round-tripped, else 17 (9), and the 17th digit of its x86 and x64 runtimes differ
					//for some values; .NET writes the shortest text that round-trips. Both round-trip; .NET's is kept (an exact format).
					return FormatExact(value,format,formatProvider,isFloat);
				case 'E':
					if(digits>precision-1) {
						precision=isFloat ? PRECISION_FLOAT_LONG : PRECISION_DOUBLE_LONG;//"E15" and longer: 17 digits (a float's "E7": 9)
					}
					break;
				case 'G':
					if(digits>precision) {
						precision=isFloat ? PRECISION_FLOAT_LONG : PRECISION_DOUBLE_LONG;//"G16" and longer: 17 digits (a float's "G8": 9)
					}
					break;
				case 'C':
				case 'F':
				case 'N':
				case 'P':
				case '\0'://custom
					break;
				default:
					return FormatExact(value,format,formatProvider,isFloat);//not a format of a double: FormatException, as on the .NET Framework
			}
			NetFxDigits netFxDigits=GetDigits(value,precision);
			if(fmtUpper=='G' && digits<1) {
				format=(fmt=='g' ? "g" : "G")+precision.ToString(CultureInfo.InvariantCulture);//the .NET Framework's "G" is "G15" for a double, "G7" for a float
				digits=precision;
			}
			if(fmt=='\0' && IsCustomFormatNotStandardOnNet(format)) {
				//A format the .NET Framework read as custom and .NET as standard ("F123"); only .NET's double formatter reads it as custom here.
				return FormatExact(value,format,formatProvider,isFloat);
			}
			decimal valueDecimal;
			if(TryGetDecimal(netFxDigits,out valueDecimal)) {
				return valueDecimal.ToString(format,formatProvider);
			}
			//Beyond a decimal.
			NumberFormatInfo numberFormatInfo=NumberFormatInfo.GetInstance(formatProvider);
			if(fmtUpper=='G') {
				return FormatGeneral(netFxDigits,digits,fmt=='g' ? 'e' : 'E',numberFormatInfo);
			}
			if(fmtUpper=='E') {
				return FormatScientific(netFxDigits,digits<0 ? 6 : digits,fmt,numberFormatInfo);
			}
			if(fmt=='\0' && HasExponent(format)) {
				return FormatExact(value,format,formatProvider,isFloat);//custom with an exponent: .NET uses 15 (7) digits for custom formats too
			}
			int exponent=netFxDigits.Scale-netFxDigits.Digits.Length;
			if(exponent>=0) {
				BigInteger bigInteger=BigInteger.Parse(netFxDigits.Digits,CultureInfo.InvariantCulture)*BigInteger.Pow(10,exponent);
				if(netFxDigits.IsNegative) {
					bigInteger=-bigInteger;
				}
				return bigInteger.ToString(format,formatProvider);
			}
			//Digits past the 28th decimal place (the value is below 1E-13): rounded to 28 places, which no format of the code shows.
			return GetDecimalRoundedToMaxScale(netFxDigits).ToString(format,formatProvider);
		}

		///<summary>value.ToString(format,formatProvider) as .NET writes it, with no sign on a value written as zero.</summary>
		private static string FormatExact(double value,string format,IFormatProvider formatProvider,bool isFloat) {
			string text=isFloat ? ((float)value).ToString(format,formatProvider) : value.ToString(format,formatProvider);
			if(value<0 || (value==0 && BitConverter.DoubleToInt64Bits(value)!=0)) {
				string textAbs=isFloat ? ((float)-value).ToString(format,formatProvider) : (-value).ToString(format,formatProvider);
				string textZero=isFloat ? (0f).ToString(format,formatProvider) : (0d).ToString(format,formatProvider);
				if(textAbs==textZero) {
					return textZero;
				}
			}
			return text;
		}

		///<summary>The .NET Framework's ParseFormatSpecifier: a letter followed by at most two digits is a standard format ('\0' for a custom
		///one); null or "" is 'G' without digits (-1).</summary>
		private static char ParseFormatSpecifier(string format,out int digits) {
			digits=-1;
			if(string.IsNullOrEmpty(format)) {
				return 'G';
			}
			char ch=format[0];
			if((ch>='A' && ch<='Z') || (ch>='a' && ch<='z')) {
				int i=1;
				int n=-1;
				if(i<format.Length && format[i]>='0' && format[i]<='9') {
					n=format[i++]-'0';
					while(i<format.Length && format[i]>='0' && format[i]<='9') {
						n=n*10+format[i++]-'0';
						if(n>=10) {
							break;
						}
					}
				}
				if(i==format.Length) {
					digits=n;
					return ch;
				}
			}
			return '\0';
		}

		///<summary>True for a format that is custom on the .NET Framework (a letter and three or more digits) and standard on .NET.</summary>
		private static bool IsCustomFormatNotStandardOnNet(string format) {
			if(string.IsNullOrEmpty(format) || format.Length<2) {
				return false;
			}
			char ch=format[0];
			if(!((ch>='A' && ch<='Z') || (ch>='a' && ch<='z'))) {
				return false;
			}
			for(int i=1;i<format.Length;i++) {
				if(format[i]<'0' || format[i]>'9') {
					return false;
				}
			}
			return true;
		}

		///<summary>True when a custom format has an exponent part (E or e followed by +, - or 0, outside quotes and escapes).</summary>
		private static bool HasExponent(string format) {
			char quote='\0';
			for(int i=0;i<format.Length;i++) {
				char ch=format[i];
				if(quote!='\0') {
					if(ch==quote) {
						quote='\0';
					}
					continue;
				}
				if(ch=='\\') {
					i++;
					continue;
				}
				if(ch=='\'' || ch=='"') {
					quote=ch;
					continue;
				}
				if((ch=='E' || ch=='e') && i+1<format.Length && (format[i+1]=='+' || format[i+1]=='-' || format[i+1]=='0')) {
					return true;
				}
			}
			return false;
		}

		///<summary>The value rounded half away from zero to precision significant digits, as the x86 .NET Framework's DoubleToNumber gave them.
		///.NET's "E16" gives 17 correctly rounded digits (for 15; precision+2 in general). For a double's 15 digits (no format, "G", "F2", "N2",
		///"C", "0.00" and every other format but the 17-digit ones) the x86 runtime rounded digits it had computed with about 17 digits of
		///accuracy, not the exact value: so these 17 digits are rounded, and 17 digits that end in 50 round up (736331.0849999995, whose exact
		///digits after the 15th are 4999..., is 736331.085 and "F2" 736331.09 there, as here; the x64 runtime rounded the exact value, 736331.08).
		///That matches the x86 record except where the exact digits after the 15th read 0.4949 to 0.4995, where x86's digits fall on either side
		///of half way (docs/port-notes/S3b-FMT.md, "x86 digits"). For the other precisions (a float's 7 and 9, a double's 17) the exact value is
		///rounded, as both runtimes did: only when the digits after the kept ones read 5 followed by zeros can it lie on either side of half way,
		///and then all of its digits are read ("E800": a double has at most 767 significant digits).</summary>
		private static NetFxDigits GetDigits(double value,int precision) {
			NetFxDigits netFxDigits=new NetFxDigits();
			netFxDigits.IsNegative=value<0 || (value==0 && BitConverter.DoubleToInt64Bits(value)!=0);
			if(value==0) {
				netFxDigits.Digits="";
				netFxDigits.Scale=0;
				return netFxDigits;
			}
			//precision+2 correctly rounded digits: d.dddE+xxx
			string text=Math.Abs(value).ToString("E"+(precision+1).ToString(CultureInfo.InvariantCulture),CultureInfo.InvariantCulture);
			string digitsAll=text.Substring(0,1)+text.Substring(2,precision+1);
			int exponent=int.Parse(text.Substring(precision+4),NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture);
			bool isHalfOrMore=digitsAll[precision]>='5';
			if(precision!=PRECISION_DOUBLE && digitsAll[precision]=='5' && IsAllZeros(digitsAll,precision+1)) {
				string textExact=Math.Abs(value).ToString("E800",CultureInfo.InvariantCulture);
				string digitsExact=textExact.Substring(0,1)+textExact.Substring(2,800);
				isHalfOrMore=digitsExact[precision]>='5';
			}
			char[] arrayDigits=digitsAll.Substring(0,precision).ToCharArray();
			int scale=exponent+1;
			if(isHalfOrMore) {
				int i=precision-1;
				while(i>=0 && arrayDigits[i]=='9') {
					arrayDigits[i]='0';
					i--;
				}
				if(i<0) {
					arrayDigits[0]='1';//999.. became 1000..
					scale++;
				}
				else {
					arrayDigits[i]++;
				}
			}
			netFxDigits.Digits=new string(arrayDigits).TrimEnd('0');
			netFxDigits.Scale=scale;
			return netFxDigits;
		}

		private static bool IsAllZeros(string text,int start) {
			for(int i=start;i<text.Length;i++) {
				if(text[i]!='0') {
					return false;
				}
			}
			return true;
		}

		///<summary>The digits as a decimal, if a decimal holds them exactly (value below 7.9E+28 and no digit past the 28th decimal place).</summary>
		private static bool TryGetDecimal(NetFxDigits netFxDigits,out decimal valueDecimal) {
			valueDecimal=0m;
			if(netFxDigits.Digits.Length==0) {
				return true;//zero, without a sign
			}
			int exponent=netFxDigits.Scale-netFxDigits.Digits.Length;//value = Digits * 10^exponent
			ulong mantissa=ulong.Parse(netFxDigits.Digits,NumberStyles.None,CultureInfo.InvariantCulture);
			if(exponent<0) {
				if(-exponent>DECIMAL_MAX_SCALE) {
					return false;
				}
				valueDecimal=new decimal((int)(uint)mantissa,(int)(uint)(mantissa>>32),0,netFxDigits.IsNegative,(byte)-exponent);
				return true;
			}
			if(netFxDigits.Digits.Length+exponent>DECIMAL_MAX_SCALE) {
				return false;
			}
			valueDecimal=new decimal(mantissa);
			for(int i=0;i<exponent;i++) {
				valueDecimal*=10m;
			}
			if(netFxDigits.IsNegative) {
				valueDecimal=-valueDecimal;
			}
			return true;
		}

		///<summary>A value below 1E-13 whose digits go past the 28th decimal place, rounded half away from zero to 28 places (zero, or a few
		///digits).</summary>
		private static decimal GetDecimalRoundedToMaxScale(NetFxDigits netFxDigits) {
			int exponent=netFxDigits.Scale-netFxDigits.Digits.Length;
			int countKept=netFxDigits.Digits.Length-(-exponent-DECIMAL_MAX_SCALE);//digits at places 1..28
			ulong mantissa=0;
			if(countKept>=0) {
				string digitsKept=netFxDigits.Digits.Substring(0,countKept);
				mantissa=digitsKept.Length==0 ? 0 : ulong.Parse(digitsKept,NumberStyles.None,CultureInfo.InvariantCulture);
				if(netFxDigits.Digits[countKept]>='5') {
					mantissa++;
				}
			}
			if(mantissa==0) {
				return 0m;
			}
			return new decimal((int)(uint)mantissa,(int)(uint)(mantissa>>32),0,netFxDigits.IsNegative,DECIMAL_MAX_SCALE);
		}

		///<summary>The digits rounded half away from zero to count significant digits (the .NET Framework's RoundNumber).</summary>
		private static NetFxDigits RoundDigits(NetFxDigits netFxDigits,int count) {
			if(netFxDigits.Digits.Length<=count) {
				return netFxDigits;
			}
			NetFxDigits netFxDigitsRounded=netFxDigits;
			char[] arrayDigits=netFxDigits.Digits.Substring(0,count).ToCharArray();
			if(netFxDigits.Digits[count]>='5') {
				int i=count-1;
				while(i>=0 && arrayDigits[i]=='9') {
					arrayDigits[i]='0';
					i--;
				}
				if(i<0) {
					arrayDigits=new char[] { '1' };
					netFxDigitsRounded.Scale++;
				}
				else {
					arrayDigits[i]++;
				}
			}
			netFxDigitsRounded.Digits=new string(arrayDigits).TrimEnd('0');
			if(netFxDigitsRounded.Digits.Length==0) {
				netFxDigitsRounded.Scale=0;
			}
			return netFxDigitsRounded;
		}

		///<summary>The .NET Framework's 'G' format (NumberToString and FormatGeneral) for digits a decimal cannot hold.</summary>
		private static string FormatGeneral(NetFxDigits netFxDigits,int maxDigits,char expChar,NumberFormatInfo numberFormatInfo) {
			NetFxDigits netFxDigitsRounded=RoundDigits(netFxDigits,maxDigits);
			string digitsText=netFxDigitsRounded.Digits;
			StringBuilder stringBuilder=new StringBuilder();
			if(digitsText.Length==0) {
				return "0";
			}
			if(netFxDigitsRounded.IsNegative) {
				stringBuilder.Append(numberFormatInfo.NegativeSign);
			}
			int digPos=netFxDigitsRounded.Scale;
			bool isScientific=false;
			if(digPos>maxDigits || digPos<-3) {
				digPos=1;
				isScientific=true;
			}
			int index=0;
			if(digPos>0) {
				while(digPos>0) {
					stringBuilder.Append(index<digitsText.Length ? digitsText[index++] : '0');
					digPos--;
				}
			}
			else {
				stringBuilder.Append('0');
			}
			if(index<digitsText.Length || digPos<0) {
				stringBuilder.Append(numberFormatInfo.NumberDecimalSeparator);
				while(digPos<0) {
					stringBuilder.Append('0');
					digPos++;
				}
				while(index<digitsText.Length) {
					stringBuilder.Append(digitsText[index++]);
				}
			}
			if(isScientific) {
				AppendExponent(stringBuilder,netFxDigitsRounded.Scale-1,expChar,2,numberFormatInfo);
			}
			return stringBuilder.ToString();
		}

		///<summary>The .NET Framework's 'E' format for digits a decimal cannot hold.</summary>
		private static string FormatScientific(NetFxDigits netFxDigits,int decimals,char fmt,NumberFormatInfo numberFormatInfo) {
			NetFxDigits netFxDigitsRounded=RoundDigits(netFxDigits,decimals+1);
			string digitsText=netFxDigitsRounded.Digits;
			StringBuilder stringBuilder=new StringBuilder();
			if(netFxDigitsRounded.IsNegative && digitsText.Length>0) {
				stringBuilder.Append(numberFormatInfo.NegativeSign);
			}
			stringBuilder.Append(digitsText.Length>0 ? digitsText[0] : '0');
			if(decimals>0) {
				stringBuilder.Append(numberFormatInfo.NumberDecimalSeparator);
				for(int i=1;i<=decimals;i++) {
					stringBuilder.Append(i<digitsText.Length ? digitsText[i] : '0');
				}
			}
			int exponent=digitsText.Length==0 ? 0 : netFxDigitsRounded.Scale-1;
			AppendExponent(stringBuilder,exponent,fmt=='e' ? 'e' : 'E',3,numberFormatInfo);
			return stringBuilder.ToString();
		}

		private static void AppendExponent(StringBuilder stringBuilder,int exponent,char expChar,int minDigits,NumberFormatInfo numberFormatInfo) {
			stringBuilder.Append(expChar);
			if(exponent<0) {
				stringBuilder.Append(numberFormatInfo.NegativeSign);
				exponent=-exponent;
			}
			else {
				stringBuilder.Append(numberFormatInfo.PositiveSign);
			}
			stringBuilder.Append(exponent.ToString(CultureInfo.InvariantCulture).PadLeft(minDigits,'0'));
		}
		#endregion The formatter
	}
}
