// Helper32Client.CallPumping and WaitWithThreadWindowsDisabled (S3a-14's review, docs/port-notes/S3a-14.md, sections 3 and 5.3): a call that
// shows a window to the user (Schick CDR's exam window, Topaz's signing window) runs on another thread while the calling thread dispatches its
// messages, so the program's windows keep painting. No window is shown: the test windows are never made visible. Each test starts copies of the
// helper from this folder and ends them; HelperTestBase checks that none is left.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using CodeBase;
using Helper32Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Helper32Tests {
	[TestClass]
	public class CallPumpingTests:HelperTestBase {
		private static readonly TimeSpan _timeSpanCall=TimeSpan.FromSeconds(60);

		[DllImport("user32.dll",SetLastError=true)]
		private static extern IntPtr SendMessageTimeout(IntPtr hWnd,uint msg,IntPtr wParam,IntPtr lParam,uint flags,uint timeout,out IntPtr result);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsWindowEnabled(IntPtr hWnd);

		private const uint SMTO_NORMAL=0x0000;

		[TestMethod]
		public void CallPumping_TheCallersWindowsAnswerDuringTheCall_AndItsResultComesBack() {
			//Before the review Schick.cs called the helper with Call on the program's UI thread: a window of that thread did not answer until
			//the call ended (CDR starting, its create-exam window waiting for the user), and Windows marked the program "Not responding".
			HelperCopy helperCopy=NewCopy();
			using(Helper32Client client=new Helper32Client(helperCopy.PathExe,HelperCopy.ArgsFakeModule))
			using(ManualResetEvent manualResetEventReady=new ManualResetEvent(false)) {
				IntPtr hWnd=IntPtr.Zero;
				int threadIdCaller=0;
				string[] arrayResults=null;
				Exception exceptionThread=null;
				Thread thread=new Thread(() => {
					try {
						using(TestWindow window=new TestWindow()) {
							hWnd=window.Handle;
							threadIdCaller=Thread.CurrentThread.ManagedThreadId;
							manualResetEventReady.Set();
							arrayResults=client.CallPumping(FakeModule.MODULE,"Sleep",new[] { "4000" },null,_timeSpanCall,isReactivating:false).Results;
							Assert.AreEqual(3,window.CountMessages,"Every message was answered during the call.");
							Assert.AreEqual(threadIdCaller,window.ThreadIdLast,"Dispatched on the calling thread.");
						}
					}
					catch(Exception exception) {
						exceptionThread=exception;
						manualResetEventReady.Set();
					}
				});
				thread.SetApartmentState(ApartmentState.STA);
				thread.IsBackground=true;
				thread.Start();
				Assert.IsTrue(manualResetEventReady.WaitOne(10000));
				Assert.IsNull(exceptionThread);
				DateTime dateTimeDeadline=DateTime.UtcNow.AddSeconds(30);
				while(!client.IsCallPumping && DateTime.UtcNow<dateTimeDeadline) {
					Thread.Sleep(20);
				}
				Assert.IsTrue(client.IsCallPumping,"The pumping call started.");
				for(int i=0;i<3;i++) {
					IntPtr isSent=SendMessageTimeout(hWnd,FakeModule.WM_TEST,IntPtr.Zero,IntPtr.Zero,SMTO_NORMAL,1000,out IntPtr result);
					Assert.AreNotEqual(IntPtr.Zero,isSent,"The calling thread's window did not answer within 1 second during the call (error "
						+Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture)+").");
					Assert.AreEqual(FakeModule.ANSWER,result.ToInt32());
					Assert.IsTrue(client.IsCallPumping,"Still in the call (the helper sleeps 4 seconds).");
				}
				Assert.IsTrue(thread.Join(30000),"The call ends.");
				Assert.IsNull(exceptionThread,exceptionThread?.ToString());
				CollectionAssert.AreEqual(new[] { "slept" },arrayResults);
				Assert.IsFalse(client.IsCallPumping);
				Assert.AreEqual("1",client.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0],"The same helper answers the next call.");
			}
		}

		[TestMethod]
		public void CallPumping_ThrowsTheCallsFailureItself() {
			//What the module threw and a timeout come as the Helper32Exception that Call throws (Schick.cs and TopazWrapper catch that type), not
			//wrapped by the wait in an AggregateException.
			HelperCopy helperCopy=NewCopy();
			using(Helper32Client client=new Helper32Client(helperCopy.PathExe,HelperCopy.ArgsFakeModule)) {
				Helper32Exception exception=Assert.ThrowsExactly<Helper32Exception>(() => client.CallPumping(FakeModule.MODULE,"Throw",
					new[] { "from the module" },null,_timeSpanCall,isReactivating:false));
				Assert.AreEqual(Helper32Failure.Remote,exception.Failure);
				Assert.AreEqual("from the module",exception.Message);
				Assert.IsFalse(client.IsCallPumping);
				//The timeout: the helper is ended, with everything it held, and the next call starts a new one.
				Assert.AreEqual("1",client.CallPumping(FakeModule.MODULE,"Count",new string[0],null,_timeSpanCall,isReactivating:false).Results[0]);
				int processIdFirst=client.ProcessId;
				exception=Assert.ThrowsExactly<Helper32Exception>(() => client.CallPumping(FakeModule.MODULE,"Sleep",new[] { "20000" },null,
					TimeSpan.FromSeconds(1),isReactivating:false));
				Assert.AreEqual(Helper32Failure.TimedOut,exception.Failure);
				Assert.AreEqual(0,client.ProcessId,"The helper was ended.");
				Assert.IsFalse(client.IsCallPumping);
				Assert.AreEqual("1",client.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0],"A new helper: the state of the first one is gone.");
				Assert.AreNotEqual(processIdFirst,client.ProcessId);
			}
		}

		[TestMethod]
		public void WaitWithThreadWindowsDisabled_RunsFuncOnAnotherThread_AndLeavesHiddenWindowsAlone() {
			//Only visible enabled windows are disabled (as a modal window of the program disables them); a hidden window keeps taking messages and
			//stays enabled. Visible windows are disabled and enabled again: launch checks S2 and T3 (no test shows a window).
			Exception exceptionThread=null;
			Thread thread=new Thread(() => {
				try {
					using(TestWindow window=new TestWindow()) {
						int threadIdCaller=Thread.CurrentThread.ManagedThreadId;
						bool isEnabledDuring=false;
						int threadIdFunc=Helper32Client.WaitWithThreadWindowsDisabled(() => {
							isEnabledDuring=IsWindowEnabled(window.Handle);
							return Thread.CurrentThread.ManagedThreadId;
						},isReactivating:true);
						Assert.AreNotEqual(threadIdCaller,threadIdFunc,"func ran on another thread.");
						Assert.IsTrue(isEnabledDuring,"A hidden window is not disabled.");
						Assert.IsTrue(IsWindowEnabled(window.Handle));
						InvalidOperationException exception=Assert.ThrowsExactly<InvalidOperationException>(() =>
							Helper32Client.WaitWithThreadWindowsDisabled<int>(() => throw new InvalidOperationException("thrown"),isReactivating:false));
						Assert.AreEqual("thrown",exception.Message);
					}
				}
				catch(Exception exception) {
					exceptionThread=exception;
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground=true;
			thread.Start();
			Assert.IsTrue(thread.Join(30000));
			Assert.IsNull(exceptionThread,exceptionThread?.ToString());
		}

		[TestMethod]
		public void SchickAndTopaz_CallTheHelperPumping() {
			//Schick.cs's ShowExam (CDR starting, its create-exam window) and Topaz's signing window wait with the program's messages dispatched.
			//Schick's exam window stays in front (no reactivation); the signing window closes with the call, so the program's window is activated.
			string schick=TestPaths.ReadSource("WpfControlsOD","Bridges","Schick.cs");
			StringAssert.Contains(schick,"Helper32Client.Default.CallPumping(SchickCommands.MODULE,SchickCommands.SHOW_EXAM,new[] { pat.LName,pat.FName,patID },null,_timeSpanCall,");
			StringAssert.Contains(schick,"isReactivating:false);");
			Assert.IsFalse(schick.Contains(".Call(SchickCommands."),"Schick.cs makes no call that blocks the program's thread.");
			string view=TestPaths.ReadSource("OpenPmsDentalBusiness","UI","TopazSignatureView.cs");
			StringAssert.Contains(view,"_helper32Client.CallPumping(TopazCommands.MODULE,TopazCommands.SIGN,arrayArgs,null,_timeSpanSign,");
			StringAssert.Contains(view,"isReactivating:true);");
			StringAssert.Contains(view,"if(_helper32Client.IsCallPumping) {","A Topaz box painted during a pumping call does not wait for the helper.");
			Assert.IsFalse(TestPaths.ReadSource("OpenPmsDentalBusiness","UI","TopazWrapper.cs").Contains("DoEvents"),
				"TopazWrapper uses the client's wait, not its own.");
		}

		///<summary>A hidden top-level window (never shown) that answers FakeModule.WM_TEST and counts the messages.</summary>
		private sealed class TestWindow:System.Windows.Forms.NativeWindow,IDisposable {
			public int CountMessages { get; private set; }
			public int ThreadIdLast { get; private set; }

			public TestWindow() {
				System.Windows.Forms.CreateParams createParams=new System.Windows.Forms.CreateParams();
				createParams.Caption="Helper32Tests pumping call window";
				createParams.ExStyle=0x00000080;//WS_EX_TOOLWINDOW
				CreateHandle(createParams);
			}

			protected override void WndProc(ref System.Windows.Forms.Message m) {
				if(m.Msg==FakeModule.WM_TEST) {
					CountMessages++;
					ThreadIdLast=Thread.CurrentThread.ManagedThreadId;
					m.Result=new IntPtr(FakeModule.ANSWER);
					return;
				}
				base.WndProc(ref m);
			}

			public void Dispose() {
				DestroyHandle();
			}
		}
	}
}
