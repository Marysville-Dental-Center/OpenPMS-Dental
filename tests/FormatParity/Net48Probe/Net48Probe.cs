// The .NET Framework 4.8 record for package S3b-FMT of the .NET 10 move (decision 24, docs/port-notes/S3b-FMT.md): the text double.ToString
// and float.ToString give, with every format the scan finds in the shipped code and without one, in en-US and in the invariant culture, and
// the results of DateTime.AddX, DateTimeOffset.AddX and TimeSpan.FromX for fractional arguments. Not shipped, not part of any project: it is
// compiled with the C# 5 compiler of the .NET Framework 4.8 folder and run on that runtime by Make-Net48Truth.ps1, which writes the output to
// tests\FormatParity\net48-truth.txt. FormatParityTests reads that file and runs every row against the helpers on .NET 10.
// Synthetic values only. Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Net48Probe {
	public static class Program {
		public static int Main(string[] args) {
			if(args.Length!=2) {
				Console.Error.WriteLine("Usage: Net48Probe <formats.txt> <output.txt>");
				return 2;
			}
			if(typeof(object).Assembly.GetName().Name!="mscorlib") {
				Console.Error.WriteLine("Not .NET Framework.");
				return 3;
			}
			List<string> listFormats=ReadFormats(args[0]);
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.Append("# The .NET Framework record of S3b-FMT (decision 24 of docs/specs/net10-plan.md), written by tests\\FormatParity\\Net48Probe on\n");
			stringBuilder.Append("# .NET Framework "+Environment.Version+" ("+(IntPtr.Size==4 ? "x86" : "x64")+" process); docs/port-notes/S3b-FMT.md says how. Do not edit by hand.\n");
			stringBuilder.Append("# Tab-separated, ASCII; a character outside 0x20-0x7E is written \\uXXXX and a backslash \\\\.\n");
			stringBuilder.Append("#   F <index> <format>                      a format (<none>: ToString(provider), <null>: ToString(null,..), <empty>: ToString(empty,..))\n");
			stringBuilder.Append("#   N <D|S> <bits> <inv|en-US> <index> <text> a double (D, 64 bits) or float (S, 32 bits) with that format and culture\n");
			stringBuilder.Append("#   T <method> <argument bits> <ticks>      DateTime(2026,11,16,8,0,0).AddX, DateTimeOffset.AddX (offset 0) or TimeSpan.FromX;\n");
			stringBuilder.Append("#                                          the ticks of the result, or EXC:<exception type>\n");
			for(int i=0;i<listFormats.Count;i++) {
				stringBuilder.Append("F\t"+i+"\t"+Escape(listFormats[i])+"\n");
			}
			CultureInfo[] arrayCultures=new CultureInfo[] { CultureInfo.InvariantCulture,CultureInfo.GetCultureInfo("en-US") };
			CultureInfo[] arrayInvariant=new CultureInfo[] { CultureInfo.InvariantCulture };
			List<double> listDoubles=GetDoubles();
			for(int v=0;v<listDoubles.Count;v++) {
				double value=listDoubles[v];
				string bits=BitConverter.DoubleToInt64Bits(value).ToString("X16");
				foreach(CultureInfo cultureInfo in v<_countFixed ? arrayCultures : arrayInvariant) {
					for(int i=0;i<listFormats.Count;i++) {
						string text=FormatOne(value,listFormats[i],cultureInfo);
						stringBuilder.Append("N\tD\t"+bits+"\t"+CultureName(cultureInfo)+"\t"+i+"\t"+Escape(text)+"\n");
					}
				}
			}
			List<float> listFloats=GetFloats();
			for(int v=0;v<listFloats.Count;v++) {
				float value=listFloats[v];
				string bits=BitConverter.ToInt32(BitConverter.GetBytes(value),0).ToString("X8");
				foreach(CultureInfo cultureInfo in v<_countFixed ? arrayCultures : arrayInvariant) {
					for(int i=0;i<listFormats.Count;i++) {
						string text=FormatOne(value,listFormats[i],cultureInfo);
						stringBuilder.Append("N\tS\t"+bits+"\t"+CultureName(cultureInfo)+"\t"+i+"\t"+Escape(text)+"\n");
					}
				}
			}
			DateTime dateTime=new DateTime(2026,11,16,8,0,0);
			DateTimeOffset dateTimeOffset=new DateTimeOffset(dateTime,TimeSpan.Zero);
			foreach(double value in GetTimeArguments()) {
				string bits=BitConverter.DoubleToInt64Bits(value).ToString("X16");
				double v=value;
				AppendTime(stringBuilder,"DateTime.AddDays",bits,delegate { return dateTime.AddDays(v).Ticks; });
				AppendTime(stringBuilder,"DateTime.AddHours",bits,delegate { return dateTime.AddHours(v).Ticks; });
				AppendTime(stringBuilder,"DateTime.AddMinutes",bits,delegate { return dateTime.AddMinutes(v).Ticks; });
				AppendTime(stringBuilder,"DateTime.AddSeconds",bits,delegate { return dateTime.AddSeconds(v).Ticks; });
				AppendTime(stringBuilder,"DateTime.AddMilliseconds",bits,delegate { return dateTime.AddMilliseconds(v).Ticks; });
				AppendTime(stringBuilder,"DateTimeOffset.AddDays",bits,delegate { return dateTimeOffset.AddDays(v).UtcTicks; });
				AppendTime(stringBuilder,"DateTimeOffset.AddHours",bits,delegate { return dateTimeOffset.AddHours(v).UtcTicks; });
				AppendTime(stringBuilder,"DateTimeOffset.AddMinutes",bits,delegate { return dateTimeOffset.AddMinutes(v).UtcTicks; });
				AppendTime(stringBuilder,"DateTimeOffset.AddSeconds",bits,delegate { return dateTimeOffset.AddSeconds(v).UtcTicks; });
				AppendTime(stringBuilder,"DateTimeOffset.AddMilliseconds",bits,delegate { return dateTimeOffset.AddMilliseconds(v).UtcTicks; });
				AppendTime(stringBuilder,"TimeSpan.FromDays",bits,delegate { return TimeSpan.FromDays(v).Ticks; });
				AppendTime(stringBuilder,"TimeSpan.FromHours",bits,delegate { return TimeSpan.FromHours(v).Ticks; });
				AppendTime(stringBuilder,"TimeSpan.FromMinutes",bits,delegate { return TimeSpan.FromMinutes(v).Ticks; });
				AppendTime(stringBuilder,"TimeSpan.FromSeconds",bits,delegate { return TimeSpan.FromSeconds(v).Ticks; });
				AppendTime(stringBuilder,"TimeSpan.FromMilliseconds",bits,delegate { return TimeSpan.FromMilliseconds(v).Ticks; });
			}
			File.WriteAllText(args[1],stringBuilder.ToString(),new UTF8Encoding(false));
			return 0;
		}

		private static List<string> ReadFormats(string path) {
			List<string> listFormats=new List<string>();
			foreach(string line in File.ReadAllLines(path)) {
				if(line.Length==0 || line.StartsWith("//")) {
					continue;
				}
				listFormats.Add(Unescape(line));
			}
			return listFormats;
		}

		private static string CultureName(CultureInfo cultureInfo) {
			return cultureInfo.Name=="" ? "inv" : cultureInfo.Name;
		}

		private static string FormatOne(double value,string format,CultureInfo cultureInfo) {
			try {
				if(format=="<none>") {
					return value.ToString(cultureInfo);
				}
				if(format=="<null>") {
					return value.ToString(null,cultureInfo);
				}
				return value.ToString(format=="<empty>" ? "" : format,cultureInfo);
			}
			catch(Exception ex) {
				return "EXC:"+ex.GetType().Name;
			}
		}

		private static string FormatOne(float value,string format,CultureInfo cultureInfo) {
			try {
				if(format=="<none>") {
					return value.ToString(cultureInfo);
				}
				if(format=="<null>") {
					return value.ToString(null,cultureInfo);
				}
				return value.ToString(format=="<empty>" ? "" : format,cultureInfo);
			}
			catch(Exception ex) {
				return "EXC:"+ex.GetType().Name;
			}
		}

		private delegate long TicksGetter();

		private static void AppendTime(StringBuilder stringBuilder,string method,string bits,TicksGetter ticksGetter) {
			string result;
			try {
				result=ticksGetter().ToString(CultureInfo.InvariantCulture);
			}
			catch(Exception ex) {
				result="EXC:"+ex.GetType().Name;
			}
			stringBuilder.Append("T\t"+method+"\t"+bits+"\t"+result+"\n");
		}

		///<summary>Half steps, the values around zero, computed sums, the edges of the type, and seeded random values: money amounts in cents,
		///half cents (the 1.005 case), sums and differences of amounts, fractions, and values of every size.</summary>
		///<summary>The count of the fixed values at the start of the last list made: they are written in both cultures, the random ones after them in
		///the invariant culture only (the cultures differ only in symbols and patterns, which the fixed values show).</summary>
		private static int _countFixed;

		private static List<double> GetDoubles() {
			double negativeZero=BitConverter.Int64BitsToDouble(long.MinValue);
			double one=1;//not a constant, so the sums below are computed at run time, as the program computes them
			List<double> listValues=new List<double> { 0,negativeZero,0.005,1.005,2.675,1.115,10.075,1234567.125,-0.0,-0.001,-0.004,-0.0049,-0.005,
				-0.00499999,-0.009,-0.01,-0.05,-0.5,0.5,1.5,2.5,-1.5,-2.5,0.125,0.375,1.25,1.35,1.45,0.045,0.015,0.025,0.035,0.0005,
				one/10+one*2/10,one/3,-one/3,one*2/3,one/7,0.07,19.99,4.35,0.285,1.1,2.2+one*1.1,99.995,-99.995,125.5,-125.5,1234567.891,
				646.06,-603.95,42.11,646.06-1250.01,0.3*3,3703.68,1e15,999999999999999.5,1e16,1e17,123456789012345678,650217936677028.5,
				-650217936677028.5,333333333333333.31,1e21,1e28,7.9e28,1e29,1.2345678901234567e30,1e-4,1e-5,-1e-5,0.000123456789012345,
				1e-13,1.23456789012345e-15,1e-20,-1e-20,double.Epsilon,-double.Epsilon,double.MaxValue,double.MinValue,double.NaN,
				double.PositiveInfinity,double.NegativeInfinity,0.95,0.995,0.9995,9.995,99.5,999.5,0.000005,0.0000005,1.000000000000005,
				12345.678949999999,0.1,0.2,0.30000000000000004,0.7999999999999999,-0.4999999999999999,100,-100,1000000,0.01,-0.01 };
			_countFixed=listValues.Count;
			Random random=new Random(20261116);
			for(int i=0;i<40;i++) {//money in cents
				double value=random.Next(-10000000,10000000)/100.0;
				listValues.Add(value);
			}
			for(int i=0;i<50;i++) {//half cents and half tenths: the half steps of "0.00" and "0.0"
				long cents=random.Next(0,1000000);
				listValues.Add((cents*10+5)/1000.0);
				if(i%4==0) {
					listValues.Add(-(cents*10+5)/1000.0);
					listValues.Add((cents%10000*10+5)/100.0);
				}
			}
			for(int i=0;i<30;i++) {//sums, differences and shares of amounts, as the program computes balances
				double a=random.Next(0,1000000)/100.0;
				double b=random.Next(0,1000000)/100.0;
				double c=random.Next(1,10000)/100.0;
				listValues.Add(a-b);
				listValues.Add(a+b-c);
				listValues.Add(a*0.15);
				listValues.Add(c/3);
			}
			for(int i=0;i<50;i++) {//every size
				double value=random.NextDouble()*Math.Pow(10,random.Next(-12,22));
				if(i%3==0) {
					value=Math.Round(value,random.Next(0,6));
				}
				listValues.Add(i%2==0 ? -value : value);
			}
			return listValues;
		}

		private static List<float> GetFloats() {
			float one=1;
			List<float> listValues=new List<float> { 0,-0f*one,0.1f,0.2f,0.1f+0.2f*one,1.1f,1.005f,2.675f,0.125f,12.5f,0.3f,0.005f,-0.001f,
				-0.004f,-0.005f,16777216f,16777217f,1.0000001f,123456.7f,1234567.8f,3.4028235E+38f,-3.4028235E+38f,float.Epsilon,1e-10f,
				float.NaN,float.PositiveInfinity,float.NegativeInfinity,one/3,2.5f,0.45f,99.995f,646.06f };
			_countFixed=listValues.Count;
			Random random=new Random(1116);
			for(int i=0;i<50;i++) {
				float value=(float)(random.NextDouble()*Math.Pow(10,random.Next(-6,12)));
				if(i%3==0) {
					value=(float)Math.Round(value,random.Next(0,4));
				}
				listValues.Add(i%2==0 ? -value : value);
			}
			for(int i=0;i<20;i++) {
				listValues.Add((random.Next(0,100000)*10+5)/1000f);
			}
			return listValues;
		}

		///<summary>Fractional arguments of DateTime.AddX and TimeSpan.FromX: thirds, tenths, half milliseconds of each unit, values just below and
		///above a half, NaN, the infinities and values out of range.</summary>
		private static List<double> GetTimeArguments() {
			double one=1;
			List<double> listValues=new List<double> { 0,1,-1,20*one/3,one/3,2*one/3,-one/3,-2*one/3,0.1,0.7,-0.1,2.5,-2.5,0.5,-0.5,1.5,0.25,1.1,
				1.0000001,0.0005,-0.0005,0.0004999,0.0015,0.0025,1e-7,-1e-7,7.5/60,59.9999,123.456789,0.001,0.0001,1e-4/36,one/60,one/24,
				one/1440,one/86400,8.75,0.35,1.15,1.35,2.675,0.000694444444444444,0.0416666666666667,0.0166666666666667,0.00001157407407407,
				37.5/60,22.5,90.25,1e9,1e11,1e15,-1e15,1e20,double.NaN,double.PositiveInfinity,double.NegativeInfinity,double.Epsilon };
			Random random=new Random(1611);
			for(int i=0;i<60;i++) {
				double value=random.NextDouble()*Math.Pow(10,random.Next(-4,4));
				listValues.Add(i%2==0 ? -value : value);
			}
			for(int i=0;i<30;i++) {
				listValues.Add(random.Next(0,10000)/random.Next(1,97));//integer division, then minutes by 7, 12, 60 and so on
				listValues.Add(random.Next(0,10000)/(double)random.Next(1,97));
			}
			return listValues;
		}

		private static string Escape(string text) {
			StringBuilder stringBuilder=new StringBuilder();
			foreach(char c in text) {
				if(c=='\\') {
					stringBuilder.Append("\\\\");
				}
				else if(c<0x20 || c>0x7E) {
					stringBuilder.Append("\\u"+((int)c).ToString("X4"));
				}
				else {
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}

		private static string Unescape(string text) {
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
	}
}
