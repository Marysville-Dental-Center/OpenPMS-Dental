// DateTime.AddDays, AddHours, AddMinutes, AddSeconds and AddMilliseconds (and DateTimeOffset's) as the .NET Framework computed them (decision
// 24 of docs/specs/net10-plan.md, package S3b-FMT, docs/port-notes/S3b-FMT.md). The .NET Framework rounded the value to the nearest
// millisecond (half away from zero) before adding it; .NET 7 and later keep every tick, so a value with a fraction no binary double holds
// exactly comes out a tick short or long: DateTime.AddHours(20d/3) is 06:40:00 later on the .NET Framework and 06:39:59.9999999 on .NET. The
// same .NET change as TimeSpan.FromX, which S3b-5 routed through ODTimeSpan. Every call of the shipped code with an argument that is not a
// whole number (by its type: a double that is not an integral value converted to double, and not a whole-number constant) goes through here;
// tools\FormatSweep finds them and the guard test fails on a new one. tests\FormatParity\net48-truth.txt holds the .NET Framework results.
//
// Compiled into CodeBase (public) and linked into the shipped projects that do not reference CodeBase (internal there, OD_NUMBERTEXT_LINKED).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;

namespace CodeBase {
#if OD_NUMBERTEXT_LINKED
	internal
#else
	public
#endif
	static class ODDateTimeAdd {
		private const long MILLIS_PER_SECOND=1000;
		private const long MILLIS_PER_MINUTE=60000;
		private const long MILLIS_PER_HOUR=3600000;
		private const long MILLIS_PER_DAY=86400000;
		///<summary>The .NET Framework's DateTime.MaxMillis: the milliseconds from 0001-01-01 to 10000-01-01.</summary>
		private const long MAX_MILLIS=3652059L*MILLIS_PER_DAY;

		public static DateTime AddDaysNetFx(this DateTime dateTime,double value) {
			return Add(dateTime,value,MILLIS_PER_DAY);
		}

		public static DateTime AddHoursNetFx(this DateTime dateTime,double value) {
			return Add(dateTime,value,MILLIS_PER_HOUR);
		}

		public static DateTime AddMinutesNetFx(this DateTime dateTime,double value) {
			return Add(dateTime,value,MILLIS_PER_MINUTE);
		}

		public static DateTime AddSecondsNetFx(this DateTime dateTime,double value) {
			return Add(dateTime,value,MILLIS_PER_SECOND);
		}

		public static DateTime AddMillisecondsNetFx(this DateTime dateTime,double value) {
			return Add(dateTime,value,1);
		}

		public static DateTimeOffset AddDaysNetFx(this DateTimeOffset dateTimeOffset,double value) {
			return new DateTimeOffset(Add(dateTimeOffset.DateTime,value,MILLIS_PER_DAY),dateTimeOffset.Offset);
		}

		public static DateTimeOffset AddHoursNetFx(this DateTimeOffset dateTimeOffset,double value) {
			return new DateTimeOffset(Add(dateTimeOffset.DateTime,value,MILLIS_PER_HOUR),dateTimeOffset.Offset);
		}

		public static DateTimeOffset AddMinutesNetFx(this DateTimeOffset dateTimeOffset,double value) {
			return new DateTimeOffset(Add(dateTimeOffset.DateTime,value,MILLIS_PER_MINUTE),dateTimeOffset.Offset);
		}

		public static DateTimeOffset AddSecondsNetFx(this DateTimeOffset dateTimeOffset,double value) {
			return new DateTimeOffset(Add(dateTimeOffset.DateTime,value,MILLIS_PER_SECOND),dateTimeOffset.Offset);
		}

		public static DateTimeOffset AddMillisecondsNetFx(this DateTimeOffset dateTimeOffset,double value) {
			return new DateTimeOffset(Add(dateTimeOffset.DateTime,value,1),dateTimeOffset.Offset);
		}

		///<summary>The .NET Framework's DateTime.Add(double value,int scale): the value in milliseconds rounded half away from zero, refused with
		///ArgumentOutOfRangeException beyond the span of DateTime (and for NaN, which the .NET Framework's cast made long.MinValue; .NET's makes
		///it 0), then added as ticks (AddTicks refuses a result out of range, as there).</summary>
		private static DateTime Add(DateTime dateTime,double value,long scale) {
			double millis=value*scale+(value>=0 ? 0.5 : -0.5);
			if(double.IsNaN(millis) || millis>=MAX_MILLIS || millis<=-MAX_MILLIS) {
				throw new ArgumentOutOfRangeException(nameof(value),"Value to add was out of range.");
			}
			return dateTime.AddTicks((long)millis*TimeSpan.TicksPerMillisecond);
		}
	}
}
