// The Schick CDR, Topaz and Guru modules in a running helper, and the helper's message pump between two requests (S3a-14 of the .NET 10 move,
// docs/port-notes/S3a-14.md, sections 5 and 6). No vendor code is loaded and no vendor program is started: Schick and Guru get only a command
// they do not know (which proves the module is there and loaded nothing), and Topaz gets the synthetic control of tests\TopazStandIn. Nothing
// shows a window: the pump test's window is never made visible. Each test starts copies of the helper from this folder and ends them;
// HelperTestBase checks that none is left.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using CodeBase;
using Helper32Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalHelper32;
using OpenPmsDentalHelper32.Guru;
using OpenPmsDentalHelper32.Schick;
using OpenPmsDentalHelper32.Topaz;

namespace Helper32Tests {
	[TestClass]
	public class VendorModulesProcessTests:HelperTestBase {
		private static readonly TimeSpan _timeSpanCall=TimeSpan.FromSeconds(60);

		[DllImport("user32.dll",SetLastError=true)]
		private static extern IntPtr SendMessageTimeout(IntPtr hWnd,uint msg,IntPtr wParam,IntPtr lParam,uint flags,uint timeout,out IntPtr result);

		private const uint SMTO_NORMAL=0x0000;

		///<summary>The DLLs loaded in the helper's process now (it is 32-bit, as this test process is).</summary>
		private static List<string> GetModuleNames(int processId) {
			using(Process process=Process.GetProcessById(processId)) {
				return process.Modules.Cast<ProcessModule>().Select(x => x.ModuleName).ToList();
			}
		}

		[TestMethod]
		public void TheVendorModules_AreInTheHelper_AndLoadNothingUntilTheirCommands() {
			HelperCopy helperCopy=NewCopy();
			using(Helper32Client client=new Helper32Client(helperCopy.PathExe)) {
				foreach(string module in new[] { SchickCommands.MODULE,TopazCommands.MODULE,GuruCommands.MODULE }) {
					Helper32Exception exception=Assert.ThrowsExactly<Helper32Exception>(() => client.Call(module,"NoSuchCommand",_timeSpanCall));
					Assert.AreEqual(Helper32Failure.Remote,exception.Failure);
					StringAssert.Contains(exception.Message,"module has no command NoSuchCommand","The helper has the module "+module+".");
				}
				List<string> listModules=GetModuleNames(client.ProcessId);
				Assert.IsFalse(listModules.Any(x => x.Equals("MedVisorInterface.dll",StringComparison.OrdinalIgnoreCase)),string.Join(", ",listModules));
				Assert.IsFalse(listModules.Any(x => x.Equals("SigPlusNET.dll",StringComparison.OrdinalIgnoreCase)));
				Assert.IsFalse(listModules.Any(x => x.StartsWith("CDR",StringComparison.OrdinalIgnoreCase)));
			}
		}

		[TestMethod]
		public void Topaz_TheControlRunsInTheHelper_AndItsPictureComesBack() {
			HelperCopy helperCopy=NewCopy();
			string pathStandIn=Path.Combine(HelperCopy.Folder,"TopazStandIn.dll");
			using(Helper32Client client=new Helper32Client(helperCopy.PathExe)) {
				Assert.AreEqual("",client.Call(TopazCommands.MODULE,TopazCommands.LOAD,_timeSpanCall,pathStandIn)[0]);
				string id=client.Call(TopazCommands.MODULE,TopazCommands.CREATE,_timeSpanCall,pathStandIn)[0];
				StringAssert.StartsWith(id,client.ProcessId.ToString(CultureInfo.InvariantCulture)+"-");
				CollectionAssert.AreEqual(new[] { TopazCommands.TYPE_VOID,"" },client.Call(TopazCommands.MODULE,TopazCommands.CALL,_timeSpanCall,id,
					"SetSigString",TopazCommands.TYPE_STRING,"valid"));
				CollectionAssert.AreEqual(new[] { TopazCommands.TYPE_INT,"250" },client.Call(TopazCommands.MODULE,TopazCommands.CALL,_timeSpanCall,id,
					"NumberOfTabletPoints"));
				CollectionAssert.AreEqual(new[] { TopazCommands.TYPE_STRING,pathStandIn },client.Call(TopazCommands.MODULE,TopazCommands.CALL,
					_timeSpanCall,id,"GetAssemblyLocation"),"The helper loaded the file the program found.");
				byte[] arrayPng=client.Call(TopazCommands.MODULE,TopazCommands.PICTURE,new[] { id,"362","79" },null,_timeSpanCall).Data;
				using(MemoryStream memoryStream=new MemoryStream(arrayPng))
				using(Bitmap bitmap=new Bitmap(memoryStream)) {
					Assert.AreEqual(new Size(362,79),bitmap.Size);
					Assert.IsTrue(bitmap.GetPixel(0,0).R<64,"The signature's picture, drawn in the helper.");
				}
				//What the control threw crosses as a Remote failure with its type, which TopazWrapper.SetTopazSigString swallows as before.
				Helper32Exception exception=Assert.ThrowsExactly<Helper32Exception>(() => client.Call(TopazCommands.MODULE,TopazCommands.CALL,
					_timeSpanCall,id,"SetSigString",TopazCommands.TYPE_STRING,"throw"));
				Assert.AreEqual(Helper32Failure.Remote,exception.Failure);
				Assert.AreEqual(typeof(IndexOutOfRangeException).FullName,exception.RemoteTypeName);
				client.Call(TopazCommands.MODULE,TopazCommands.RELEASE,_timeSpanCall,id);
				exception=Assert.ThrowsExactly<Helper32Exception>(() => client.Call(TopazCommands.MODULE,TopazCommands.CALL,_timeSpanCall,id,
					"NumberOfTabletPoints"));
				Assert.AreEqual(typeof(TopazControlGoneException).FullName,exception.RemoteTypeName,"TopazSignatureView creates its control again.");
			}
		}

		[TestMethod]
		public void Pump_TheHelpersWindowsAnswerWhileItWaitsForTheNextRequest() {
			//Before S3a-14 the helper waited for a request in a read that pumped nothing, so a window on its thread (Topaz's control, CDR's COM
			//objects) did not answer until the next request came: SendMessageTimeout timed out.
			HelperCopy helperCopy=NewCopy();
			using(Helper32Client client=new Helper32Client(helperCopy.PathExe,HelperCopy.ArgsFakeModule)) {
				string[] arrayWindow=client.Call(FakeModule.MODULE,"CreateWindow",_timeSpanCall);
				IntPtr hWnd=new IntPtr(long.Parse(arrayWindow[0],CultureInfo.InvariantCulture));
				string threadIdMain=client.Call(FakeModule.MODULE,"Apartment",_timeSpanCall)[1];
				Assert.AreEqual(threadIdMain,arrayWindow[1],"The window belongs to the helper's main thread, as the modules' objects do.");
				//The helper is idle now, waiting for the next request.
				for(int i=0;i<3;i++) {
					Thread.Sleep(200);
					IntPtr isSent=SendMessageTimeout(hWnd,FakeModule.WM_TEST,IntPtr.Zero,IntPtr.Zero,SMTO_NORMAL,5000,out IntPtr result);
					Assert.AreNotEqual(IntPtr.Zero,isSent,"The helper's window did not answer within 5 seconds while the helper waited (error "
						+Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture)+").");
					Assert.AreEqual(FakeModule.ANSWER,result.ToInt32());
				}
				CollectionAssert.AreEqual(new[] { "3",threadIdMain },client.Call(FakeModule.MODULE,"WindowMessages",_timeSpanCall),
					"Dispatched on the helper's main thread, between two requests.");
				//The requests still come in order after the pumping.
				Assert.AreEqual("1",client.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0]);
				Assert.AreEqual("2",client.Call(FakeModule.MODULE,"Count",_timeSpanCall)[0]);
			}
		}

		[TestMethod]
		public void Pump_WaitPumping_DispatchesUntilItsHandleIsSet() {
			//MessagePumpingStream's wait on its own: an STA thread with a window waits on an event; the window answers meanwhile; setting the event
			//ends the wait.
			using(ManualResetEvent manualResetEventDone=new ManualResetEvent(false))
			using(ManualResetEvent manualResetEventReady=new ManualResetEvent(false)) {
				IntPtr hWnd=IntPtr.Zero;
				Exception exceptionThread=null;
				bool isWaitOver=false;
				Thread thread=new Thread(() => {
					try {
						using(TestWindow window=new TestWindow()) {
							hWnd=window.Handle;
							manualResetEventReady.Set();
							MessagePumpingStream.WaitPumping(manualResetEventDone,() => manualResetEventDone.WaitOne(0));
							isWaitOver=true;
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
				IntPtr isSent=SendMessageTimeout(hWnd,FakeModule.WM_TEST,IntPtr.Zero,IntPtr.Zero,SMTO_NORMAL,5000,out IntPtr result);
				Assert.AreNotEqual(IntPtr.Zero,isSent);
				Assert.AreEqual(FakeModule.ANSWER,result.ToInt32());
				Assert.IsFalse(isWaitOver);
				manualResetEventDone.Set();
				Assert.IsTrue(thread.Join(10000),"The wait ends when its handle is set.");
				Assert.IsTrue(isWaitOver);
				Assert.IsNull(exceptionThread);
			}
		}

		///<summary>A hidden top-level window (never shown) that answers FakeModule.WM_TEST.</summary>
		private sealed class TestWindow:System.Windows.Forms.NativeWindow,IDisposable {
			public TestWindow() {
				System.Windows.Forms.CreateParams createParams=new System.Windows.Forms.CreateParams();
				createParams.Caption="Helper32Tests wait window";
				createParams.ExStyle=0x00000080;//WS_EX_TOOLWINDOW
				CreateHandle(createParams);
			}

			protected override void WndProc(ref System.Windows.Forms.Message m) {
				if(m.Msg==FakeModule.WM_TEST) {
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
