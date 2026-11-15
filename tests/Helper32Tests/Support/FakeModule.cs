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
				case "CreateWindow"://a hidden top-level window on the helper's main thread, as a vendor control's (S3a-14); never shown
					if(_windowPump==null) {
						_windowPump=new PumpWindow();
					}
					return new[] { _windowPump.Handle.ToInt64().ToString(CultureInfo.InvariantCulture),
						Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture) };
				case "WindowMessages"://how many test messages the window got, and on which thread
					return new[] { _windowPump.CountMessages.ToString(CultureInfo.InvariantCulture),
						_windowPump.ThreadIdLast.ToString(CultureInfo.InvariantCulture) };
				default:
					throw new ArgumentException("No command "+command);
			}
		}

		private static int _count;
		private static PumpWindow _windowPump;

		///<summary>WM_APP+0x42, the message the pump tests send.</summary>
		public const int WM_TEST=0x8000+0x42;
		///<summary>What the window answers to WM_TEST.</summary>
		public const int ANSWER=0x5A5A;

		///<summary>A top-level window without WS_VISIBLE (never shown, no task bar button: a tool window), that answers WM_TEST.</summary>
		private sealed class PumpWindow:System.Windows.Forms.NativeWindow {
			public int CountMessages { get; private set; }
			public int ThreadIdLast { get; private set; }

			public PumpWindow() {
				System.Windows.Forms.CreateParams createParams=new System.Windows.Forms.CreateParams();
				createParams.Caption="Helper32Tests pump window";
				createParams.Style=0;//WS_OVERLAPPED, not visible
				createParams.ExStyle=0x00000080;//WS_EX_TOOLWINDOW
				CreateHandle(createParams);
			}

			protected override void WndProc(ref System.Windows.Forms.Message m) {
				if(m.Msg==WM_TEST) {
					CountMessages++;
					ThreadIdLast=Thread.CurrentThread.ManagedThreadId;
					m.Result=new IntPtr(ANSWER);
					return;
				}
				base.WndProc(ref m);
			}
		}
	}
}
