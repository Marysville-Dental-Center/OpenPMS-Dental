// TimeSpan.FromDays, FromHours, FromMinutes, FromSeconds and FromMilliseconds as the .NET Framework computed them (S3b-5 of the .NET 10 move,
// docs/port-notes/S3b-5.md). The .NET Framework rounded the value to the nearest millisecond; .NET (Core 3.0 and later) keeps every tick, so a
// value with a fraction no binary double holds exactly comes out a tick short: TimeSpan.FromHours(20d/3) is 06:40:00 on the .NET Framework and
// 06:39:59.9999999 on .NET, which the time card window showed and which made the appointment grid's rounding take the minute below (UnitTests'
// ClockEventsTests, TimeCardRules tests and AppointmentsTests found it). Every call of the program with a value that is not a whole-number
// literal goes through here, so the program computes and shows the times it did on the .NET Framework.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;

namespace CodeBase {
	public static class ODTimeSpan {
		private const double MILLIS_PER_SECOND=1000;
		private const double MILLIS_PER_MINUTE=60000;
		private const double MILLIS_PER_HOUR=3600000;
		private const double MILLIS_PER_DAY=86400000;

		public static TimeSpan FromDays(double value) {
			return Interval(value,MILLIS_PER_DAY);
		}

		public static TimeSpan FromHours(double value) {
			return Interval(value,MILLIS_PER_HOUR);
		}

		public static TimeSpan FromMinutes(double value) {
			return Interval(value,MILLIS_PER_MINUTE);
		}

		public static TimeSpan FromSeconds(double value) {
			return Interval(value,MILLIS_PER_SECOND);
		}

		public static TimeSpan FromMilliseconds(double value) {
			return Interval(value,1);
		}

		///<summary>The .NET Framework's TimeSpan.Interval: the value in milliseconds, rounded half away from zero to a whole millisecond. NaN
		///throws ArgumentException and a value out of TimeSpan's range OverflowException, as there.</summary>
		private static TimeSpan Interval(double value,double scale) {
			if(double.IsNaN(value)) {
				throw new ArgumentException("TimeSpan does not accept floating point Not-a-Number values.");
			}
			double millis=value*scale+(value>=0 ? 0.5 : -0.5);
			if(millis>long.MaxValue/TimeSpan.TicksPerMillisecond || millis<long.MinValue/TimeSpan.TicksPerMillisecond) {
				throw new OverflowException("TimeSpan overflowed because the duration is too long.");
			}
			return new TimeSpan((long)millis*TimeSpan.TicksPerMillisecond);
		}
	}
}
