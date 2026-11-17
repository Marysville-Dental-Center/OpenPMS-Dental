// A program process for the tests that end one (S3a-13, docs/port-notes/S3a-13.md, section 8): a TestProbeHost child that starts its own
// helper through Helper32Client, as the program does.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using CodeBase;
using OpenPmsDentalHelper32;

namespace Helper32Tests.Support {
	///<summary>Runs in the TestProbeHost child (TestProbeHost calls Handle for each line of the test).</summary>
	public static class ProgramProbe {
		private static Helper32Client _client;

		public static string Handle(string command,string[] args) {
			switch(command) {
				case "Start"://args: the helper copy's path. Starts the helper with its first call and returns its process id.
					_client=new Helper32Client(args[0],HelperCopy.ArgsFakeModule);
					string[] arrayPing=_client.Call(Helper32Protocol.MODULE_HOST,Helper32Protocol.COMMAND_PING,TimeSpan.FromSeconds(60));
					return arrayPing[1];
				case "Pid":
					return Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);
				case "Count":
					return _client.Call(FakeModule.MODULE,"Count",TimeSpan.FromSeconds(60))[0];
				case "Die"://this program process ends abruptly shortly after its reply, without disposing its client (a crash, an end in Task Manager)
					Thread thread=new Thread(() => {
						Thread.Sleep(300);
						Process.GetCurrentProcess().Kill();
					});
					thread.IsBackground=true;
					thread.Start();
					return "dying";
				case "SleepInHelper"://a call that is still running in the helper when this process dies
					Thread threadCall=new Thread(() => {
						try {
							_client.Call(FakeModule.MODULE,"Sleep",TimeSpan.FromMinutes(5),"600000");
						}
						catch(Exception) {
							//the process ends meanwhile
						}
					});
					threadCall.IsBackground=true;
					threadCall.Start();
					Thread.Sleep(1000);
					return "calling";
				default:
					throw new ArgumentException("No command "+command);
			}
		}
	}
}
