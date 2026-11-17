// The 32-bit TWAIN path through the real 32-bit helper process (S3b-8 of the .NET 10 move, docs/port-notes/S3b-8.md, section 8; decision 23).
// Each test starts copies of the helper with the fake DSM of TwainTestDsm (--test-twain-dsm) and ends them; HelperTestBase checks that none
// is left. No scanner, no driver, no real DSM: the helper's TWAIN module runs on the fake in its own 32-bit process.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using CodeBase;
using Helper32Tests.Support;
using ImagingCapture.Metadata;
using ImagingCapture.Session;
using ImagingDeviceManager.Capture;
using ImagingDeviceManager.Helper32;
using ImagingDeviceManager.Journal;
using ImagingDeviceManager.Tests;
using ImagingDeviceManager.TwainProtocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalHelper32;

namespace Helper32Tests.Twain {
	[TestClass]
	public class TwainHelperProcessTests:HelperTestBase {
		private readonly List<IDisposable> _listCleanup=new List<IDisposable>();
		private readonly List<string> _listFolders=new List<string>();

		[TestCleanup]
		public void CleanupTwain() {
			for(int i=_listCleanup.Count-1;i>=0;i--) {
				try {
					_listCleanup[i].Dispose();
				}
				catch {
				}
			}
			_listCleanup.Clear();
			foreach(string folder in _listFolders) {
				try {
					Directory.Delete(folder,true);
				}
				catch {
				}
			}
			_listFolders.Clear();
		}

		#region Rig
		private Helper32Client NewTwainClient(HelperCopy helperCopy,string scenario) {
			Helper32Client client=new Helper32Client(helperCopy.PathExe,Helper32Protocol.ARG_TEST_TWAIN_DSM+" "+typeof(TwainTestDsm).Assembly.GetName().Name+" "
				+typeof(TwainTestDsm).FullName+" "+scenario);
			_listCleanup.Add(client);
			return client;
		}

		private string NewFolder() {
			string folder=Path.Combine(Path.GetTempPath(),"TwainHelperProcessTests_"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(folder);
			_listFolders.Add(folder);
			return folder;
		}

		///<summary>The program's native DSM in this (64-bit) process: a fake that lists the names given.</summary>
		private static DsmLoader NativeLister(params string[] arrayNames) {
			return (out string error) => {
				error="";
				FakeTwainDsm fake=new FakeTwainDsm();
				fake.ListSourceNames=arrayNames.ToList();
				return fake;
			};
		}

		private static TwainSourcePath NewPath(DsmLoader dsmLoaderNative,ITwainHelperChannel channel) {
			return new TwainSourcePath(dsmLoaderNative,() => channel,() => true,true) { ListTimeout=TimeSpan.FromSeconds(60) };
		}

		private static TwainCaptureOptions NewOptions() {
			return new TwainCaptureOptions { ImagingDeviceNum=12,TickMilliseconds=20,RefusedDisableWait=TimeSpan.FromSeconds(2),OpenTimeout=TimeSpan.FromSeconds(20) };
		}

		///<summary>Every event of a session, with a wait.</summary>
		private sealed class Recorder {
			private readonly object _lock=new object();
			public readonly List<AcquiredImage> ListImages=new List<AcquiredImage>();
			public readonly List<TwainCaptureEnded> ListEnded=new List<TwainCaptureEnded>();
			public readonly List<TwainCaptureStatus> ListStatus=new List<TwainCaptureStatus>();
			public readonly List<string> ListLog=new List<string>();

			public Recorder(ITwainCaptureSession session) {
				session.ImageAcquired+=x => Record(() => ListImages.Add(x));
				session.Ended+=x => Record(() => ListEnded.Add(x));
				session.StatusChanged+=x => Record(() => ListStatus.Add(x));
				session.Logged+=x => Record(() => ListLog.Add(x));
			}

			private void Record(Action action) {
				lock(_lock) {
					action();
					Monitor.PulseAll(_lock);
				}
			}

			public void WaitUntil(Func<bool> condition,string what,int timeoutMs=60000) {
				DateTime end=DateTime.UtcNow.AddMilliseconds(timeoutMs);
				lock(_lock) {
					while(!condition()) {
						TimeSpan left=end-DateTime.UtcNow;
						if(left<=TimeSpan.Zero) {
							Assert.Fail("Timed out waiting for "+what+".\r\n"+string.Join("\r\n",ListLog));
						}
						Monitor.Wait(_lock,left);
					}
				}
			}

			public TwainCaptureEnded WaitForEnded(int enableId,int timeoutMs=60000) {
				Assert.IsTrue(enableId>0,"Enable returned 0.");
				WaitUntil(() => ListEnded.Any(x => x.EnableId==enableId),"the end of enable "+enableId,timeoutMs);
				lock(_lock) {
					return ListEnded.Single(x => x.EnableId==enableId);
				}
			}

			public List<AcquiredImage> Images {
				get {
					lock(_lock) {
						return new List<AcquiredImage>(ListImages);
					}
				}
			}
		}

		///<summary>A router on the native fake (its listing and its in-process sessions) and the helper channel, whose sessions write to folder.
		///</summary>
		private TwainCaptureSessionRouter NewRouter(DsmLoader dsmLoaderNative,TwainHelperChannel channel,string folder,TimeSpan callTimeout) {
			CaptureFolders folders=CaptureFolders.CreateForFolder(folder);
			return new TwainCaptureSessionRouter(NewPath(dsmLoaderNative,channel),
				() => new TwainCaptureSession(dsmLoaderNative,folders,() => DateTime.Now),
				() => new TwainHelperCaptureSession(channel,folders,folder) { CallTimeout=callTimeout },folders);
		}

		private static void AssertSameImage(AcquiredImage expected,AcquiredImage actual,string what) {
			foreach(PropertyInfo property in typeof(AcquiredImage).GetProperties(BindingFlags.Public|BindingFlags.Instance)) {
				if(new[] { "JournalPath","QuarantinePath","ReceivedAt","Tag","Gray16","Dib","SidecarFields" }.Contains(property.Name)) {
					continue;
				}
				Assert.AreEqual(property.GetValue(expected),property.GetValue(actual),what+": "+property.Name);
			}
			if(expected.Gray16!=null) {
				CollectionAssert.AreEqual(expected.Gray16,actual.Gray16,what+": the 16-bit samples");
			}
			else {
				Assert.IsNull(actual.Gray16);
				CollectionAssert.AreEqual(expected.Dib,actual.Dib,what+": the DIB");
			}
		}

		private static void AssertSameEnded(TwainCaptureEnded expected,TwainCaptureEnded actual,string what) {
			foreach(PropertyInfo property in typeof(TwainCaptureEnded).GetProperties(BindingFlags.Public|BindingFlags.Instance)) {
				if(property.Name=="EndedAt" || property.Name=="Tag") {
					continue;
				}
				Assert.AreEqual(property.GetValue(expected),property.GetValue(actual),what+": "+property.Name);
			}
		}

		private static void AssertSameSidecar(string pathExpected,string pathActual,string what) {
			Dictionary<string,string> dictExpected=CaptureFiles.ReadSidecar(Path.ChangeExtension(pathExpected,".txt"));
			Dictionary<string,string> dictActual=CaptureFiles.ReadSidecar(Path.ChangeExtension(pathActual,".txt"));
			foreach(KeyValuePair<string,string> pair in dictExpected) {
				if(new[] { "ReceivedAt","AcquireId","SessionGuid","ImageFile","ConfigFile" }.Contains(pair.Key)) {
					continue;
				}
				Assert.IsTrue(dictActual.ContainsKey(pair.Key),what+": no "+pair.Key+" in the helper's sidecar");
				Assert.AreEqual(pair.Value,dictActual[pair.Key],what+": sidecar "+pair.Key);
			}
			Assert.AreEqual(dictExpected.Count,dictActual.Count,what+": the number of sidecar lines");
			CollectionAssert.AreEqual(File.ReadAllBytes(pathExpected),File.ReadAllBytes(pathActual),what+": the journal copy's bytes");
		}
		#endregion Rig

		///<summary>The TWAIN module of the 32-bit helper: version 1, a 32-bit process whose TWAIN structures have S3a-12's x86 sizes, and the 32-bit
		///fake DSM's sources.</summary>
		[TestMethod]
		public void Helper_Is32Bit_X86Structures_ListsThe32BitSources() {
			HelperCopy helperCopy=NewCopy();
			Helper32Client client=NewTwainClient(helperCopy,TwainTestDsm.SCENARIO_CAPTURE);
			TwainHelperChannel channel=new TwainHelperChannel(client);
			byte[] dataReply;
			string[] arrayHello=channel.Call(TwainHelperCommands.Hello,new string[0],null,out dataReply,TimeSpan.FromSeconds(60));
			Assert.AreEqual("2",arrayHello[0],"The TWAIN module's version (2 since the open result carries IsSourceNotListed).");
			Assert.AreEqual("32",arrayHello[1],"The helper is a 32-bit process.");
			Assert.AreEqual("8",arrayHello[3],"TW_USERINTERFACE in the helper: 8 bytes (x86).");
			Assert.AreEqual("38",arrayHello[4],"TW_IMAGEMEMXFER in the helper: 38 bytes (x86).");
			Assert.AreEqual(12,Marshal.SizeOf(typeof(TW_USERINTERFACE)),"And 12 in this 64-bit test process.");
			string error;
			List<TwainSourceEntry> listSources=NewPath(NativeLister("Native Scanner"),channel).ListHelper32(out error);
			Assert.AreEqual("",error);
			CollectionAssert.AreEquivalent(new[] { TwainTestDsm.SCANNER_32,TwainTestDsm.SENSOR },listSources.Select(x => x.ProductName).ToArray());
			Assert.IsTrue(listSources.All(x => x.Dsm==TwainDsmKind.Helper32 && x.DsmLibrary=="TestTwainDsm (32-bit fake)"));
		}

		///<summary>A source only the 32-bit DSM lists captures through the helper with the same images (the raw 16-bit samples of the memory
		///transfer, the 8-bit DIB), bit depth, DAT_EXTIMAGEINFO capture metadata (with the keys of the helper's own copy of the parser), end reasons
		///and journal copies as the same fake source in this process; the device list stores it with the marker, and the stored text opens it.
		///The in-process baseline runs and closes first: the router asks the program's own DSM first, in a session that waits for, or refuses
		///next to, another session of this process with the same ProductName (S3b-8 section 2.1), as a real source open twice would be.
		///</summary>
		[TestMethod]
		public void SourceOnlyIn32BitDsm_CapturesThroughTheHelper_SameAsInProcess() {
			HelperCopy helperCopy=NewCopy();
			Helper32Client client=NewTwainClient(helperCopy,TwainTestDsm.SCENARIO_CAPTURE);
			TwainHelperChannel channel=new TwainHelperChannel(client);
			DsmLoader dsmLoaderNative=NativeLister("Native Scanner");
			string error;
			List<TwainSourceEntry> listSources=NewPath(dsmLoaderNative,channel).ListAll(out error);
			Assert.AreEqual("",error);
			TwainSourceEntry entrySensor=listSources.Single(x => x.ProductName==TwainTestDsm.SENSOR);
			Assert.AreEqual(TwainDsmKind.Helper32,entrySensor.Dsm);
			string stored=entrySensor.StoredText;
			Assert.AreEqual(TwainTestDsm.SENSOR+TwainSourceChoice.Marker32,stored,"The choice records the 32-bit DSM.");
			//In this process, the baseline.
			string folderIn=NewFolder();
			TwainCaptureSession captureIn=new TwainCaptureSession((out string errorLoad) => {
				errorLoad="";
				return TwainTestDsm.CreateFake(TwainTestDsm.SCENARIO_CAPTURE);
			},CaptureFolders.CreateForFolder(folderIn),() => DateTime.Now);
			_listCleanup.Add(new DisposeAction(() => {
				captureIn.BeginClose();
				captureIn.WaitForClose(TimeSpan.FromSeconds(15));
			}));
			captureIn.FuncReadMetadataKeys=CaptureMetadataParser.ReadKeys;
			captureIn.QuarantineContext=new TwainCaptureQuarantineContext(77,"Doe, Jane");
			Recorder recorderIn=new Recorder(captureIn);
			Assert.IsTrue(captureIn.Open(TwainTestDsm.SENSOR,NewOptions()).IsOpen);
			Dictionary<int,TwainCaptureEnded> dictEndedIn=new Dictionary<int,TwainCaptureEnded>();
			foreach(int bits in new[] { 16,8 }) {
				dictEndedIn[bits]=recorderIn.WaitForEnded(captureIn.Enable(new TwainEnableRequest { Bits=bits,Tag=new object(),TargetLabel="Slot "+bits }));
			}
			TwainCaptureSourceInfo sourceInfoIn=captureIn.SourceInfo;
			captureIn.BeginClose();
			Assert.IsTrue(captureIn.WaitForClose(TimeSpan.FromSeconds(15)),"The baseline session closes.");
			//Through the helper, as the program opens a stored choice.
			string folderHelper=NewFolder();
			TwainCaptureSessionRouter router=NewRouter(dsmLoaderNative,channel,folderHelper,TimeSpan.FromSeconds(60));
			_listCleanup.Add(new DisposeAction(() => {
				router.BeginClose();
				router.WaitForClose(TimeSpan.FromSeconds(15));
			}));
			router.FuncReadMetadataKeys=CaptureMetadataParser.ReadKeys;
			router.QuarantineContext=new TwainCaptureQuarantineContext(77,"Doe, Jane");
			Recorder recorderHelper=new Recorder(router);
			TwainCaptureOpenResult result=router.Open(stored,NewOptions());
			Assert.IsTrue(result.IsOpen,result.ErrorText);
			Assert.IsInstanceOfType(router.Inner,typeof(TwainHelperCaptureSession));
			Assert.AreEqual(TwainDsmKind.Helper32,router.Resolution.Dsm);
			Assert.AreEqual(TwainTestDsm.SENSOR,router.ProductName);
			StringAssert.Contains(router.SourceInfo.DsmName,"32-bit, in the helper");
			Assert.IsTrue(sourceInfoIn.DriverIdentity.IsSameAs(router.SourceInfo.DriverIdentity),"The record is tied to the same driver.");
			foreach(int bits in new[] { 16,8 }) {
				object tag=new object();
				TwainEnableRequest request=new TwainEnableRequest { Bits=bits,Tag=tag,TargetLabel="Slot "+bits };
				TwainCaptureEnded endedIn=dictEndedIn[bits];
				TwainCaptureEnded endedHelper=recorderHelper.WaitForEnded(router.Enable(request));
				Assert.AreEqual(TwainCaptureEndReason.Completed,endedHelper.Reason,endedHelper.ErrorText);
				Assert.AreEqual(bits,endedHelper.BitDepthInEffect);
				AssertSameEnded(endedIn,endedHelper,bits+"-bit enable");
				Assert.AreSame(tag,endedHelper.Tag);
			}
			List<AcquiredImage> listIn=recorderIn.Images;
			List<AcquiredImage> listHelper=recorderHelper.Images;
			Assert.AreEqual(2,listHelper.Count);
			for(int i=0;i<2;i++) {
				AssertSameImage(listIn[i],listHelper[i],"image "+(i+1));
				AssertSameSidecar(listIn[i].JournalPath,listHelper[i].JournalPath,"image "+(i+1));
				StringAssert.StartsWith(listHelper[i].JournalPath,folderHelper);
			}
			Assert.IsTrue(listHelper[0].IsGray16);
			Assert.AreEqual(TwainTestDsm.Sample16(5,3),listHelper[0].Gray16[3*6+5]);
			Assert.AreEqual(TwainTransferMechanism.Memory,listHelper[0].TransferMechanism);
			Dictionary<string,string> dictSidecar=CaptureFiles.ReadSidecar(Path.ChangeExtension(listHelper[0].JournalPath,".txt"));
			Assert.AreEqual("cid-16",dictSidecar["CaptureId"],"The helper's copy of the metadata parser read the keys.");
			Assert.AreEqual("77",dictSidecar["PatNum"]);
			Assert.AreEqual(1,helperCopy.GetProcesses().Count,"One helper for the listing and the session.");
			router.BeginClose();
			Assert.IsTrue(router.WaitForClose(TimeSpan.FromSeconds(30)));
			Assert.AreEqual(TwainCaptureState.Closed,router.State);
		}

		///<summary>A source both DSMs list runs in this process (native first): the helper is never started.</summary>
		[TestMethod]
		public void SourceInBothDsms_UsedIn64Bit_HelperNotStarted() {
			HelperCopy helperCopy=NewCopy();
			Helper32Client client=NewTwainClient(helperCopy,TwainTestDsm.SCENARIO_CAPTURE);
			TwainHelperChannel channel=new TwainHelperChannel(client);
			string folder=NewFolder();
			//The program's own DSM lists the sensor too (its 64-bit driver): the same fake in this process.
			DsmLoader dsmLoaderNative=(out string errorLoad) => {
				errorLoad="";
				return TwainTestDsm.CreateFake(TwainTestDsm.SCENARIO_CAPTURE);
			};
			TwainCaptureSessionRouter router=NewRouter(dsmLoaderNative,channel,folder,TimeSpan.FromSeconds(60));
			_listCleanup.Add(new DisposeAction(() => {
				router.BeginClose();
				router.WaitForClose(TimeSpan.FromSeconds(15));
			}));
			Recorder recorder=new Recorder(router);
			Assert.IsTrue(router.Open(TwainTestDsm.SENSOR+TwainSourceChoice.Marker32,NewOptions()).IsOpen);
			Assert.IsInstanceOfType(router.Inner,typeof(TwainCaptureSession));
			Assert.AreEqual(TwainDsmKind.Native,router.Resolution.Dsm);
			TwainCaptureEnded ended=recorder.WaitForEnded(router.Enable(new TwainEnableRequest { Bits=16,TargetLabel="Slot 1" }));
			Assert.AreEqual(16,ended.BitDepthInEffect);
			Assert.AreEqual(0,client.ProcessId,"The helper was never started.");
			Assert.AreEqual(0,helperCopy.GetProcesses().Count);
		}

		private TwainCaptureSessionRouter OpenNeverEnding(HelperCopy helperCopy,TimeSpan callTimeout,out Helper32Client client,out Recorder recorder,
			out int enableId,out string folder)
		{
			client=NewTwainClient(helperCopy,TwainTestDsm.SCENARIO_NEVER);
			TwainHelperChannel channel=new TwainHelperChannel(client);
			folder=NewFolder();
			TwainCaptureSessionRouter router=NewRouter(NativeLister("Native Scanner"),channel,folder,callTimeout);
			TwainCaptureSessionRouter routerClose=router;
			_listCleanup.Add(new DisposeAction(() => {
				routerClose.BeginClose();
				routerClose.WaitForClose(TimeSpan.FromSeconds(15));
			}));
			recorder=new Recorder(router);
			TwainCaptureOpenResult result=router.Open(TwainTestDsm.SENSOR,NewOptions());
			Assert.IsTrue(result.IsOpen,result.ErrorText);
			Assert.IsInstanceOfType(router.Inner,typeof(TwainHelperCaptureSession),"A choice from before S3b-8 (no marker) runs through the helper.");
			enableId=router.Enable(new TwainEnableRequest { Bits=8,TargetLabel="Slot 1" });
			Recorder recorderWait=recorder;
			recorder.WaitUntil(() => recorderWait.ListStatus.Any(x => x.State==TwainCaptureState.WaitingForImage),"waiting for the exposure");
			return router;
		}

		private static void AssertCleanEnd(TwainCaptureSessionRouter router,Recorder recorder,int enableId,string fragment,string folder) {
			TwainCaptureEnded ended=recorder.WaitForEnded(enableId);
			Assert.AreEqual(TwainCaptureEndReason.Error,ended.Reason);
			Assert.IsTrue(ended.SourceMayHoldImages);
			StringAssert.Contains(ended.ErrorText,fragment);
			StringAssert.Contains(ended.ErrorText,"do not re-expose yet");
			Assert.IsTrue(router.WaitForClose(TimeSpan.FromSeconds(10)),"No hang.");
			Assert.AreEqual(TwainCaptureState.Closed,router.State);
			StringAssert.Contains(File.ReadAllText(Path.Combine(folder,CaptureFolders.SubfolderCapture,CaptureLog.FileName)),fragment);
		}

		///<summary>The helper dies in the middle of a session: the enable ends with Error and the message, the session closes, no hang.</summary>
		[TestMethod]
		public void HelperKilledMidSession_EnableEndsWithTheMessage_SessionClosed() {
			HelperCopy helperCopy=NewCopy();
			Helper32Client client;
			Recorder recorder;
			int enableId;
			string folder;
			TwainCaptureSessionRouter router=OpenNeverEnding(helperCopy,TimeSpan.FromSeconds(30),out client,out recorder,out enableId,out folder);
			using(Process process=Process.GetProcessById(client.ProcessId)) {
				process.Kill();//a copy this test started
				process.WaitForExit(10000);
			}
			AssertCleanEnd(router,recorder,enableId,"program ended during the capture session",folder);
		}

		///<summary>The helper stops answering (its process is suspended): the call times out, the helper client ends it, and the session ends as
		///above.</summary>
		[TestMethod]
		public void HelperNeverAnswers_TimesOut_EnableEndsWithTheMessage_SessionClosed() {
			HelperCopy helperCopy=NewCopy();
			Helper32Client client;
			Recorder recorder;
			int enableId;
			string folder;
			TwainCaptureSessionRouter router=OpenNeverEnding(helperCopy,TimeSpan.FromSeconds(3),out client,out recorder,out enableId,out folder);
			using(Process process=Process.GetProcessById(client.ProcessId)) {
				Assert.AreEqual(0,NtSuspendProcess(process.Handle),"The helper copy is suspended.");
			}
			AssertCleanEnd(router,recorder,enableId,"stopped answering",folder);
			Assert.AreEqual(0,client.ProcessId,"The helper client ended the helper that did not answer.");
		}

		///<summary>The helper is missing: a stored 32-bit choice fails to open with the reason, and an unmarked one is reported not found by the
		///in-process session, as before; nothing is started.</summary>
		[TestMethod]
		public void HelperMissing_32BitChoiceFailsWithTheReason_NothingStarted() {
			string folder=NewFolder();
			Helper32Client client=new Helper32Client(Path.Combine(folder,"Helper32",Helper32Client.HELPER_FILE_NAME));
			_listCleanup.Add(client);
			TwainHelperChannel channel=new TwainHelperChannel(client);
			TwainCaptureSessionRouter router=NewRouter(NativeLister("Native Scanner"),channel,folder,TimeSpan.FromSeconds(10));
			TwainCaptureOpenResult result=router.Open(TwainTestDsm.SENSOR+TwainSourceChoice.Marker32,NewOptions());
			Assert.IsFalse(result.IsOpen);
			Assert.AreEqual(TwainCaptureOpenError.Other,result.Error);
			StringAssert.Contains(result.ErrorText,"could not list the 32-bit TWAIN sources");
			StringAssert.Contains(result.ErrorText,"was not found");
			Assert.IsTrue(router.WaitForClose(TimeSpan.Zero));
			TwainCaptureSessionRouter routerOld=NewRouter(NativeLister("Native Scanner"),channel,folder,TimeSpan.FromSeconds(10));
			TwainCaptureOpenResult resultOld=routerOld.Open(TwainTestDsm.SENSOR,NewOptions());
			Assert.AreEqual(TwainCaptureOpenError.SourceNotFound,resultOld.Error,resultOld.ErrorText);
			Assert.AreEqual(0,client.ProcessId);
		}

		///<summary>The helper stops answering while it lists the 32-bit sources: the listing times out, the helper is ended, and the device list
		///still has the native sources with the reason.</summary>
		[TestMethod]
		public void ListingHangs_TimesOut_HelperEnded_NativeSourcesStillListed() {
			HelperCopy helperCopy=NewCopy();
			Helper32Client client=NewTwainClient(helperCopy,TwainTestDsm.SCENARIO_HANG_LIST);
			TwainHelperChannel channel=new TwainHelperChannel(client);
			TwainSourcePath path=new TwainSourcePath(NativeLister("Native Scanner"),() => channel,() => true,true) { ListTimeout=TimeSpan.FromSeconds(5) };
			string error;
			List<TwainSourceEntry> listSources=path.ListAll(out error);
			CollectionAssert.AreEqual(new[] { "Native Scanner" },listSources.Select(x => x.StoredText).ToArray());
			StringAssert.Contains(error,"did not answer");
			Assert.AreEqual(0,client.ProcessId,"The helper that did not answer was ended.");
		}

		private sealed class DisposeAction:IDisposable {
			private readonly Action _action;

			public DisposeAction(Action action) {
				_action=action;
			}

			public void Dispose() {
				_action();
			}
		}

		[DllImport("ntdll.dll")]
		private static extern int NtSuspendProcess(IntPtr processHandle);
	}
}
