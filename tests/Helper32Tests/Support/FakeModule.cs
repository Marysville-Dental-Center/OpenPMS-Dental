// The fake module the protocol tests run in a copy of the helper (S3a-13, docs/port-notes/S3a-13.md, section 8), loaded with --test-module.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace Helper32Tests.Support {
	///<summary>Runs in the helper's process: TestModule calls Handle for each request to the module "Test". Only .NET Framework types here, so
	///the helper loads nothing else of this assembly's references.</summary>
	public static class FakeModule {
		public const string MODULE="Test";

		public static string[] Handle(string command,string[] args,byte[] data,out byte[] dataReply) {
			dataReply=null;
			switch(command) {
				case "Echo"://the arguments and the data back
					dataReply=data;
					return args;
				case "Throw":
					throw new InvalidOperationException(args[0]);
				case "Exit"://the helper ends during the call with this exit code
					Environment.Exit(int.Parse(args[0],CultureInfo.InvariantCulture));
					return null;
				case "Kill"://the helper ends abruptly during the call, as in a crash of vendor code
					Process.GetCurrentProcess().Kill();
					Thread.Sleep(Timeout.Infinite);
					return null;
				case "Sleep":
					Thread.Sleep(int.Parse(args[0],CultureInfo.InvariantCulture));
					return new[] { "slept" };
				case "Apartment":
					return new[] { Thread.CurrentThread.GetApartmentState().ToString(),Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture) };
				case "Count"://state that lives as long as the helper
					_count++;
					return new[] { _count.ToString(CultureInfo.InvariantCulture) };
				default:
					throw new ArgumentException("No command "+command);
			}
		}

		private static int _count;
	}
}
