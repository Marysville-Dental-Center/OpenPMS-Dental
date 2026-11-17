// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using ImagingDeviceManager.Capture;
using ImagingDeviceManager.Helper32;
using ImagingDeviceManager.Journal;
using ImagingDeviceManager.TwainProtocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingDeviceManager.Tests {
	///<summary>The 32-bit TWAIN path of S3b-8 (docs/port-notes/S3b-8.md) in this process: TwainHelperCaptureSession over an in-memory channel to a
	///TwainHelperServer (the helper's side, here on a FakeTwainDsm), every structure through TwainHelperWire's bytes. The same fake source gives
	///the same images, bit depth, metadata, end reasons and journal sidecars as TwainCaptureSession in process (CaptureSessionHarness); the
	///helper's failures (it crashes, stops answering, is missing, is replaced) end the session cleanly; the native-first choice (TwainSourcePath,
	///TwainCaptureSessionRouter) and the stored choice (TwainSourceChoice). The real 32-bit helper process runs the same in Helper32Tests
	///(Twain\TwainHelperProcessTests.cs).</summary>
	[TestClass]
	public class TwainHelperPathTests {
		private readonly List<IDisposable> _listCleanup=new List<IDisposable>();

		[TestCleanup]
		public void Cleanup() {
			for(int i=_listCleanup.Count-1;i>=0;i--) {
				try {
					_listCleanup[i].Dispose();
				}
				catch {
				}
			}
			_listCleanup.Clear();
		}

		private T Track<T>(T disposable) where T:IDisposable {
			_listCleanup.Add(disposable);
			return disposable;
		}

		#region Rigs
		///<summary>An in-memory ITwainHelperChannel to a TwainHelperServer: one call at a time, as the helper client makes them, with the bytes the
		///wire wrote. Failures are injected per call.</summary>
		internal sealed class MemoryChannel:ITwainHelperChannel,IDisposable {
			public readonly TwainHelperServer Server;
			private readonly object _lock=new object();
			public readonly List<string> ListCommands=new List<string>();
			///<summary>Called before each call with the command; an exception it returns is thrown instead of the call (the server is not reached).</summary>
			public Func<string,TwainHelperException> FuncFail;
			///<summary>While FuncIsPollHeld says so, a Poll waits for this gate before it reaches the helper (a helper slow to answer).</summary>
			public readonly ManualResetEvent EventPollGate=new ManualResetEvent(true);
			public Func<bool> FuncIsPollHeld;
			public int ProcessIdValue=4242;
			public int CountAllowSetForeground;

			public MemoryChannel(TwainHelperServer server) {
				Server=server;
			}

			public int ProcessId => ProcessIdValue;

			public string[] Call(string command,string[] args,byte[] data,out byte[] dataReply,TimeSpan timeout) {
				if(command==TwainHelperCommands.Poll && FuncIsPollHeld!=null && FuncIsPollHeld()) {
					EventPollGate.WaitOne();
				}
				lock(_lock) {
					ListCommands.Add(command);
					TwainHelperException exception=FuncFail?.Invoke(command);
					if(exception!=null) {
						dataReply=null;
						throw exception;
					}
					try {
						return Server.Handle(command,args,data==null ? null : (byte[])data.Clone(),out dataReply);
					}
					catch(Exception ex) {
						throw new TwainHelperException(TwainHelperFailure.Remote,ex.Message,ex);
					}
				}
			}

			public void AllowSetForeground() {
				Interlocked.Increment(ref CountAllowSetForeground);
			}

			public int CountCommands(string command) {
				lock(_lock) {
					return ListCommands.Count(x => x==command);
				}
			}

			public void Dispose() {
				EventPollGate.Set();
				Server.Dispose();
			}
		}

		///<summary>Every event of a session, in order, with a wait.</summary>
		internal sealed class Recorder {
			private readonly object _lock=new object();
			public readonly List<AcquiredImage> ListImages=new List<AcquiredImage>();
			public readonly List<AcquiredImage> ListQuarantined=new List<AcquiredImage>();
			public readonly List<TwainCaptureEnded> ListEnded=new List<TwainCaptureEnded>();
			public readonly List<TwainCaptureStatus> ListStatus=new List<TwainCaptureStatus>();
			public readonly List<string> ListLog=new List<string>();
			public readonly HashSet<int> HashSetThreads=new HashSet<int>();

			public Recorder(ITwainCaptureSession session) {
				session.ImageAcquired+=x => Record(() => ListImages.Add(x));
				session.ImageQuarantined+=x => Record(() => ListQuarantined.Add(x));
				session.Ended+=x => Record(() => ListEnded.Add(x));
				session.StatusChanged+=x => Record(() => ListStatus.Add(x));
				session.Logged+=x => Record(() => ListLog.Add(x));
			}

			private void Record(Action action) {
				lock(_lock) {
					HashSetThreads.Add(Thread.CurrentThread.ManagedThreadId);
					action();
					Monitor.PulseAll(_lock);
				}
			}

			public void WaitUntil(Func<bool> condition,string what,int timeoutMs=15000) {
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

			public TwainCaptureEnded WaitForEnded(int enableId,int timeoutMs=15000) {
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

		///<summary>The helper's side on a fake with the contract 1.1 profile, and the program's TwainHelperCaptureSession to it, with the capture
		///folders of a temporary folder; the options are CaptureSessionHarness's.</summary>
		internal sealed class HelperRig:IDisposable {
			public readonly FakeTwainDsm Fake=new FakeTwainDsm();
			public readonly FakeHelperSource Helper=new FakeHelperSource();
			public readonly string Folder;
			public readonly MemoryChannel Channel;
			public readonly TwainHelperCaptureSession Capture;
			public readonly Recorder Recorder;
			public readonly TwainCaptureOptions Options=new TwainCaptureOptions {
				ImagingDeviceNum=12,
				TickMilliseconds=20,
				RefusedDisableWait=TimeSpan.FromSeconds(2),
				OpenTimeout=TimeSpan.FromSeconds(10),
			};

			public HelperRig() {
				Fake.AddHelperSource(Helper);
				Folder=Path.Combine(Path.GetTempPath(),"TwainHelperPathTests_"+Guid.NewGuid().ToString("N"));
				Directory.CreateDirectory(Folder);
				TwainHelperServer server=new TwainHelperServer(Load,json => ReadKeys(json),isTestFolderAllowed:true);
				Channel=new MemoryChannel(server);
				Capture=new TwainHelperCaptureSession(Channel,CaptureFolders.CreateForFolder(Folder),Folder) { CallTimeout=TimeSpan.FromSeconds(5) };
				Recorder=new Recorder(Capture);
			}

			private ITwainDsm Load(out string errorText) {
				errorText="";
				return Fake;
			}

			public TwainCaptureOpenResult Open() {
				return Capture.Open(Helper.ProductName,Options);
			}

			public void OpenOrFail() {
				TwainCaptureOpenResult result=Open();
				Assert.IsTrue(result.IsOpen,result.ErrorText);
			}

			public void Close() {
				Capture.BeginClose();
				Assert.IsTrue(Capture.WaitForClose(TimeSpan.FromSeconds(15)),"The helper session ends.\r\n"+string.Join("\r\n",Recorder.ListLog));
				Assert.AreEqual(TwainCaptureState.Closed,Capture.State);
			}

			public void Dispose() {
				try {
					Capture.BeginClose();
					Capture.WaitForClose(TimeSpan.FromSeconds(15));
				}
				catch {
				}
				Channel.Dispose();
				try {
					Directory.Delete(Folder,true);
				}
				catch {
				}
			}
		}

		///<summary>The parser both sides give their sessions in these tests (the helper compiles ImagingCapture's; here a stand-in that reads the
		///fake's captureId and sequence).</summary>
		internal static CaptureMetadataKeys ReadKeys(string json) {
			if(string.IsNullOrEmpty(json)) {
				return null;
			}
			System.Text.RegularExpressions.Match matchId=System.Text.RegularExpressions.Regex.Match(json,"\"captureId\":\"([^\"]*)\"");
			System.Text.RegularExpressions.Match matchSequence=System.Text.RegularExpressions.Regex.Match(json,"\"sequence\":(\\d+)");
			return new CaptureMetadataKeys { CaptureId=matchId.Success ? matchId.Groups[1].Value : null,
				Sequence=matchSequence.Success ? int.Parse(matchSequence.Groups[1].Value) : (int?)null };
		}

		///<summary>The same configuration on both fake sources.</summary>
		private static void Configure(FakeHelperSource helper,Action<FakeHelperSource> actionConfigure) {
			helper.ListExposures.Clear();
			actionConfigure(helper);
		}

		private static FakePage Exposure16(int width,int height,string captureId) {
			FakePage page=TwainCapturePathTests.ExposureWith16(width,height);
			page.CaptureId=captureId;
			page.Original16.CaptureId=captureId;
			return page;
		}

		private static FakePage Exposure8(int width,string captureId) {
			FakePage page=SessionHarness.Exposure(width);
			page.CaptureId=captureId;
			return page;
		}
		#endregion Rigs

		#region Comparisons
		///<summary>Every property of AcquiredImage the source gave or the session set, except the files' paths, the receive time and Tag.</summary>
		private static void AssertSameImage(AcquiredImage expected,AcquiredImage actual,string what) {
			foreach(PropertyInfo property in typeof(AcquiredImage).GetProperties(BindingFlags.Public|BindingFlags.Instance)) {
				if(new[] { "JournalPath","QuarantinePath","ReceivedAt","Tag","Gray16","Dib","SidecarFields" }.Contains(property.Name)) {
					continue;
				}
				Assert.AreEqual(property.GetValue(expected),property.GetValue(actual),what+": "+property.Name);
			}
			if(expected.Gray16==null) {
				Assert.IsNull(actual.Gray16,what+": Gray16");
			}
			else {
				CollectionAssert.AreEqual(expected.Gray16,actual.Gray16,what+": the 16-bit samples");
			}
			if(expected.Dib==null) {
				Assert.IsNull(actual.Dib,what+": Dib");
			}
			else {
				CollectionAssert.AreEqual(expected.Dib,actual.Dib,what+": the DIB");
			}
			CollectionAssert.AreEqual(expected.SidecarFields??new List<KeyValuePair<string,string>>(),
				actual.SidecarFields??new List<KeyValuePair<string,string>>(),what+": SidecarFields");
		}

		private static void AssertSameEnded(TwainCaptureEnded expected,TwainCaptureEnded actual,string what) {
			foreach(PropertyInfo property in typeof(TwainCaptureEnded).GetProperties(BindingFlags.Public|BindingFlags.Instance)) {
				if(new[] { "EndedAt","Tag" }.Contains(property.Name)) {
					continue;
				}
				Assert.AreEqual(property.GetValue(expected),property.GetValue(actual),what+": "+property.Name);
			}
		}

		///<summary>The journal copies' sidecars hold the same lines but those of the time, the session and the file name.</summary>
		private static void AssertSameSidecar(string pathExpected,string pathActual,string what) {
			Dictionary<string,string> dictExpected=CaptureFiles.ReadSidecar(Path.ChangeExtension(pathExpected,".txt"));
			Dictionary<string,string> dictActual=CaptureFiles.ReadSidecar(Path.ChangeExtension(pathActual,".txt"));
			string[] arraySkip={ "ReceivedAt","AcquireId","SessionGuid","ImageFile","ConfigFile" };
			foreach(KeyValuePair<string,string> pair in dictExpected) {
				if(arraySkip.Contains(pair.Key)) {
					continue;
				}
				Assert.IsTrue(dictActual.ContainsKey(pair.Key),what+": the helper's sidecar has no "+pair.Key);
				Assert.AreEqual(pair.Value,dictActual[pair.Key],what+": sidecar "+pair.Key);
			}
			Assert.AreEqual(dictExpected.Count,dictActual.Count,what+": the number of sidecar lines");
			CollectionAssert.AreEqual(File.ReadAllBytes(pathExpected),File.ReadAllBytes(pathActual),what+": the journal copy's bytes");
		}
		#endregion Comparisons

		#region Same images, bit depth and metadata as in process
		///<summary>A 16-bit enable then an 8-bit one, on the same fake source in process and through the helper's side: the same images (the raw
		///16-bit samples of the memory transfer, the 8-bit DIB), bit depth, DAT_EXTIMAGEINFO capture metadata, end reasons, journal copies and
		///sidecars (with the metadata keys), and the same Tags; the source info is the same but for the DSM's name, which says the helper.</summary>
		[TestMethod]
		public void Bits16Then8_SameImagesDepthMetadataAndJournal_AsInProcess() {
			CaptureSessionHarness harness=Track(new CaptureSessionHarness());
			HelperRig rig=Track(new HelperRig());
			foreach(FakeHelperSource helper in new[] { harness.Helper,rig.Helper }) {
				Configure(helper,x => {
					x.ListExposures.Add(Exposure16(6,4,"cid-16"));
					x.ListExposures.Add(Exposure8(9,"cid-8"));
				});
			}
			harness.Capture.FuncReadMetadataKeys=ReadKeys;
			rig.Capture.FuncReadMetadataKeys=ReadKeys;
			harness.Capture.QuarantineContext=new TwainCaptureQuarantineContext(77,"Doe, Jane");
			rig.Capture.QuarantineContext=new TwainCaptureQuarantineContext(77,"Doe, Jane");
			object tag16=new object();
			object tag8=new object();
			TwainEnableRequest request16=new TwainEnableRequest { Bits=16,Tag=tag16,TargetLabel="Slot 1",
				SidecarFields=new List<KeyValuePair<string,string>> { new KeyValuePair<string,string>("MountNum","5") } };
			TwainEnableRequest request8=new TwainEnableRequest { Bits=8,Tag=tag8,TargetLabel="Slot 2",ReasonFallback="the caller's reason" };
			//In process first, then through the helper's side: in this one test process the two sessions would otherwise share the source, which
			//TwainCaptureSession refuses (two sessions never drive one source); in the program the helper's session is in another process.
			harness.OpenOrFail();
			TwainCaptureSourceInfo infoIn=harness.Capture.SourceInfo;
			string productNameIn=harness.Capture.ProductName;
			TwainCaptureEnded endedIn16=harness.WaitForEnded(harness.Capture.Enable(request16));
			TwainCaptureEnded endedIn8=harness.WaitForEnded(harness.Capture.Enable(request8));
			harness.Close();
			rig.OpenOrFail();
			Assert.AreEqual(infoIn.DsmName+" (32-bit, in the helper OpenPmsDentalHelper32)",rig.Capture.SourceInfo.DsmName);
			Assert.IsTrue(infoIn.DriverIdentity.IsSameAs(rig.Capture.SourceInfo.DriverIdentity));
			CollectionAssert.AreEqual(infoIn.ListBitDepths,rig.Capture.SourceInfo.ListBitDepths);
			Assert.AreEqual(infoIn.IsMetadataListed,rig.Capture.SourceInfo.IsMetadataListed);
			Assert.AreEqual(productNameIn,rig.Capture.ProductName);
			int enableIdHelper16=rig.Capture.Enable(request16);
			TwainCaptureEnded endedHelper16=rig.Recorder.WaitForEnded(enableIdHelper16);
			TwainCaptureEnded endedHelper8=rig.Recorder.WaitForEnded(rig.Capture.Enable(request8));
			Assert.AreEqual(16,endedHelper16.BitDepthInEffect);
			Assert.AreEqual(TwainCaptureEndReason.Completed,endedHelper16.Reason,endedHelper16.ErrorText);
			AssertSameEnded(endedIn16,endedHelper16,"16-bit enable");
			AssertSameEnded(endedIn8,endedHelper8,"8-bit enable");
			Assert.AreSame(tag16,endedHelper16.Tag);
			Assert.AreSame(tag8,endedHelper8.Tag);
			List<AcquiredImage> listIn=harness.Images;
			List<AcquiredImage> listHelper=rig.Recorder.Images;
			Assert.AreEqual(2,listHelper.Count);
			Assert.AreEqual(listIn.Count,listHelper.Count);
			for(int i=0;i<listIn.Count;i++) {
				AssertSameImage(listIn[i],listHelper[i],"image "+(i+1));
				AssertSameSidecar(listIn[i].JournalPath,listHelper[i].JournalPath,"image "+(i+1));
				StringAssert.StartsWith(listHelper[i].JournalPath,rig.Folder,"The helper's session wrote the journal copy into the session's folder.");
			}
			Assert.IsTrue(listHelper[0].IsGray16);
			Assert.AreEqual(TwainCapturePathTests.Sample16(5,3),listHelper[0].Gray16[3*6+5]);
			Assert.AreEqual(TwainTransferMechanism.Memory,listHelper[0].TransferMechanism);
			StringAssert.Contains(listHelper[0].MetadataJson,"cid-16");
			Assert.AreSame(tag16,listHelper[0].Tag);
			Assert.AreSame(tag8,listHelper[1].Tag);
			Assert.AreEqual(enableIdHelper16,listHelper[0].EnableId);
			Dictionary<string,string> dictSidecar=CaptureFiles.ReadSidecar(Path.ChangeExtension(listHelper[0].JournalPath,".txt"));
			Assert.AreEqual("77",dictSidecar["PatNum"]);
			Assert.AreEqual("cid-16",dictSidecar["CaptureId"]);
			Assert.AreEqual(rig.Capture.SessionGuid.ToString("D"),dictSidecar["SessionGuid"]);
			Assert.AreEqual("5",dictSidecar["MountNum"]);
			Assert.IsTrue(rig.Channel.CountAllowSetForeground>=2,"The source's window may come to the foreground at each enable.");
			rig.Close();
			Assert.AreEqual(0,rig.Channel.Server.CountSessions,"The helper forgot the closed session.");
		}

		///<summary>An image larger than one ReadData (TwainHelperCommands.DATA_CHUNK_MAX) comes back whole, in pieces.</summary>
		[TestMethod]
		public void LargeImage_ReadInPieces_SameSamples() {
			HelperRig rig=Track(new HelperRig());
			Configure(rig.Helper,x => x.ListExposures.Add(Exposure16(2200,2000,"cid-large")));
			rig.OpenOrFail();
			TwainCaptureEnded ended=rig.Recorder.WaitForEnded(rig.Capture.Enable(new TwainEnableRequest { Bits=16,TargetLabel="Slot 1" }),60000);
			Assert.AreEqual(TwainCaptureEndReason.Completed,ended.Reason,ended.ErrorText);
			AcquiredImage image=rig.Recorder.Images.Single();
			Assert.AreEqual(2200*2000,image.Gray16.Length);
			Assert.AreEqual(TwainCapturePathTests.Sample16(2199,1999),image.Gray16[1999*2200+2199]);
			Assert.AreEqual(TwainCapturePathTests.Sample16(17,1234),image.Gray16[1234*2200+17]);
			Assert.IsTrue(rig.Channel.CountCommands(TwainHelperCommands.ReadData)>=2,"More than one piece.");
			rig.Close();
		}

		///<summary>The source closes the session with its own condition code (MSG_CLOSEDSREQ): the same code, text and reason as in process.</summary>
		[TestMethod]
		public void CloseRequestWithEndCode_SameCodeAndText_AsInProcess() {
			CaptureSessionHarness harness=Track(new CaptureSessionHarness());
			HelperRig rig=Track(new HelperRig());
			foreach(FakeHelperSource helper in new[] { harness.Helper,rig.Helper }) {
				Configure(helper,x => {
					x.End=FakeHelperEnd.Fail;//no exposure: the sensor software fails, CLOSEDSREQ with its code
					x.EndCode=FakeHelperSource.CodeNoSensor;
				});
			}
			harness.OpenOrFail();
			TwainCaptureEnded endedIn=harness.WaitForEnded(harness.Capture.Enable(new TwainEnableRequest { Bits=8,TargetLabel="Slot 1" }));
			harness.Close();
			rig.OpenOrFail();
			TwainCaptureEnded endedHelper=rig.Recorder.WaitForEnded(rig.Capture.Enable(new TwainEnableRequest { Bits=8,TargetLabel="Slot 1" }));
			Assert.AreEqual(TwainCaptureEndReason.CloseRequested,endedHelper.Reason);
			Assert.AreEqual(FakeHelperSource.CodeNoSensor,endedHelper.ConditionCode);
			Assert.AreEqual(FakeHelperSource.GetText(FakeHelperSource.CodeNoSensor),endedHelper.ConditionText);
			AssertSameEnded(endedIn,endedHelper,"close request");
			rig.Close();
		}

		///<summary>A stop in the safe order with safe disable in effect: the same Ended (Stopped, the stop reason, safe disable in effect) and the
		///same statuses as in process.</summary>
		[TestMethod]
		public void SafeDisableCancel_SameStopAndStatuses_AsInProcess() {
			CaptureSessionHarness harness=Track(new CaptureSessionHarness());
			HelperRig rig=Track(new HelperRig());
			foreach(TwainCaptureOptions options in new[] { harness.Options,rig.Options }) {
				options.DisableIsSafe=true;
				options.SafeDisableCap=FakeHelperSource.CapSafeDisable;
				options.RefusedDisableCodes=new List<ushort> { FakeHelperSource.CodeDisableImagesPending,FakeHelperSource.CodeDisableClosing };
			}
			foreach(FakeHelperSource helper in new[] { harness.Helper,rig.Helper }) {
				Configure(helper,x => x.End=FakeHelperEnd.Never);
			}
			harness.OpenOrFail();
			int enableIdIn=harness.Capture.Enable(new TwainEnableRequest { Bits=8,TargetLabel="Slot 1" });
			harness.WaitUntil(() => harness.ListStatus.Any(x => x.State==TwainCaptureState.WaitingForImage),"waiting for the exposure");
			harness.Capture.Cancel();
			TwainCaptureEnded endedIn=harness.WaitForEnded(enableIdIn);
			harness.Close();
			rig.OpenOrFail();
			int enableIdHelper=rig.Capture.Enable(new TwainEnableRequest { Bits=8,TargetLabel="Slot 1" });
			rig.Recorder.WaitUntil(() => rig.Recorder.ListStatus.Any(x => x.State==TwainCaptureState.WaitingForImage),"waiting for the exposure");
			Assert.IsTrue(rig.Recorder.ListStatus.First(x => x.State==TwainCaptureState.WaitingForImage).IsSafeDisableInEffect);
			rig.Capture.Cancel();
			TwainCaptureEnded endedHelper=rig.Recorder.WaitForEnded(enableIdHelper);
			Assert.AreEqual(TwainCaptureEndReason.Stopped,endedHelper.Reason);
			Assert.AreEqual(TwainCaptureStopReason.Cancel,endedHelper.StopReason);
			Assert.IsTrue(endedHelper.IsSafeDisableInEffect);
			AssertSameEnded(endedIn,endedHelper,"safe stop");
			rig.Close();
			CollectionAssert.AreEqual(harness.Statuses.Select(x => x.State).ToList(),rig.Recorder.ListStatus.Select(x => x.State).ToList(),
				"The same states in the same order.");
		}

		///<summary>Every event is raised on the pump thread, which is not the caller's; the session's options and product name are kept.</summary>
		[TestMethod]
		public void Events_RaisedOnOnePumpThread() {
			HelperRig rig=Track(new HelperRig());
			Configure(rig.Helper,x => x.ListExposures.Add(Exposure8(8,"cid-a")));
			rig.OpenOrFail();
			rig.Recorder.WaitForEnded(rig.Capture.Enable(new TwainEnableRequest { Bits=8,TargetLabel="Slot 1" }));
			rig.Close();
			Assert.AreEqual(1,rig.Recorder.HashSetThreads.Count,"Every event on one thread.");
			Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId,rig.Recorder.HashSetThreads.Single());
			Assert.AreEqual(12,rig.Capture.Options.ImagingDeviceNum);
			Assert.AreEqual(rig.Helper.ProductName,rig.Capture.ProductName);
		}
		#endregion Same images, bit depth and metadata as in process

		#region BeginClose, quarantine, confirm
		///<summary>An image the helper's session raised before it learnt of BeginClose: not raised as ImageAcquired (none is, once BeginClose has
		///returned), but quarantined in the program with its journal copy's sidecar set to Delivered=Quarantine, and raised as ImageQuarantined.</summary>
		[TestMethod]
		public void BeginClose_ImageRaisedInTheHelperBeforeIt_QuarantinedNotRaised() {
			HelperRig rig=Track(new HelperRig());
			Configure(rig.Helper,x => x.ListExposures.Add(Exposure8(8,"cid-late")));
			rig.Capture.QuarantineContext=new TwainCaptureQuarantineContext(42,"Late, Image");
			rig.OpenOrFail();
			//Once the enable has gone to the helper, the pump's next Poll waits: the helper's session raises the image meanwhile (its journal copy
			//is written first), and the program learns of it only after BeginClose.
			rig.Channel.EventPollGate.Reset();
			rig.Channel.FuncIsPollHeld=() => rig.Channel.CountCommands(TwainHelperCommands.Enable)>0;
			rig.Capture.Enable(new TwainEnableRequest { Bits=8,TargetLabel="Slot 1" });
			DateTime end=DateTime.UtcNow.AddSeconds(15);
			while(Directory.GetFiles(rig.Folder,"*.bmp",SearchOption.AllDirectories).Length==0 && DateTime.UtcNow<end) {
				Thread.Sleep(20);
			}
			Assert.AreEqual(1,Directory.GetFiles(rig.Folder,"*.bmp",SearchOption.AllDirectories).Length,"The helper's session wrote the journal copy.");
			Thread.Sleep(200);
			rig.Capture.BeginClose();
			rig.Channel.EventPollGate.Set();
			Assert.IsTrue(rig.Capture.WaitForClose(TimeSpan.FromSeconds(15)));
			Assert.AreEqual(0,rig.Recorder.Images.Count,"No ImageAcquired after BeginClose.");
			AcquiredImage image=rig.Recorder.ListQuarantined.Single();
			Assert.IsNotNull(image.QuarantinePath);
			Assert.IsTrue(File.Exists(image.QuarantinePath));
			Dictionary<string,string> dictJournal=CaptureFiles.ReadSidecar(Path.ChangeExtension(image.JournalPath,".txt"));
			Assert.AreEqual(CaptureJournal.DeliveredQuarantine,dictJournal["Delivered"]);
			Dictionary<string,string> dictQuarantine=CaptureFiles.ReadSidecar(Path.ChangeExtension(image.QuarantinePath,".txt"));
			Assert.AreEqual("42",dictQuarantine["PatNum"]);
		}

		///<summary>QuarantineImage and ConfirmRedeliveredJournalCopy run in the program's process on the twin, with the record the helper's session
		///wrote: also after the helper is gone.</summary>
		[TestMethod]
		public void QuarantineImage_InTheProgram_AlsoAfterTheHelperIsGone() {
			HelperRig rig=Track(new HelperRig());
			Configure(rig.Helper,x => x.ListExposures.Add(Exposure8(8,"cid-q")));
			rig.Capture.QuarantineContext=new TwainCaptureQuarantineContext(31,"Quarantine, Test");
			rig.OpenOrFail();
			rig.Recorder.WaitForEnded(rig.Capture.Enable(new TwainEnableRequest { Bits=8,TargetLabel="Slot 1" }));
			AcquiredImage image=rig.Recorder.Images.Single();
			rig.Channel.FuncFail=command => new TwainHelperException(TwainHelperFailure.Crashed,"synthetic: the helper ended");
			rig.Recorder.WaitUntil(() => rig.Recorder.ListStatus.Any(x => x.State==TwainCaptureState.Closed),"the end after the crash");
			string error;
			string path=rig.Capture.QuarantineImage(image,"The save failed (synthetic).",false,out error);
			Assert.IsNotNull(path,error);
			Assert.AreEqual("31",CaptureFiles.ReadSidecar(Path.ChangeExtension(path,".txt"))["PatNum"]);
			Assert.AreEqual(CaptureJournal.DeliveredQuarantine,CaptureFiles.ReadSidecar(Path.ChangeExtension(image.JournalPath,".txt"))["Delivered"]);
			Assert.AreEqual(rig.Capture.SessionGuid.ToString("D"),CaptureFiles.ReadSidecar(Path.ChangeExtension(path,".txt"))["SessionGuid"]);
		}
		#endregion BeginClose, quarantine, confirm

		#region The helper's failures end the session cleanly
		private static void AssertEndedByFailure(HelperRig rig,int enableId,string fragment) {
			TwainCaptureEnded ended=rig.Recorder.WaitForEnded(enableId);
			Assert.AreEqual(TwainCaptureEndReason.Error,ended.Reason);
			Assert.AreEqual(TwainCaptureStopReason.Error,ended.StopReason);
			Assert.IsTrue(ended.SourceMayHoldImages,"Images the helper held were not received.");
			StringAssert.Contains(ended.ErrorText,fragment);
			StringAssert.Contains(ended.ErrorText,"do not re-expose yet");
			Assert.IsTrue(rig.Capture.WaitForClose(TimeSpan.FromSeconds(10)),"The session is closed, no hang.");
			Assert.AreEqual(TwainCaptureState.Closed,rig.Capture.State);
			Assert.AreEqual(TwainCaptureState.Closed,rig.Recorder.ListStatus.Last().State);
			Assert.AreEqual(0,rig.Capture.Enable(new TwainEnableRequest()),"Nothing more is enabled.");
			string log=File.ReadAllText(Path.Combine(rig.Folder,CaptureFolders.SubfolderCapture,CaptureLog.FileName));
			StringAssert.Contains(log,fragment);
		}

		private static int EnableAndWaitForExposure(HelperRig rig) {
			int enableId=rig.Capture.Enable(new TwainEnableRequest { Bits=8,TargetLabel="Slot 1" });
			rig.Recorder.WaitUntil(() => rig.Recorder.ListStatus.Any(x => x.State==TwainCaptureState.WaitingForImage),"waiting for the exposure");
			return enableId;
		}

		[TestMethod]
		public void HelperCrashesMidSession_EnableEndsWithError_SessionClosed() {
			HelperRig rig=Track(new HelperRig());
			Configure(rig.Helper,x => x.End=FakeHelperEnd.Never);
			rig.OpenOrFail();
			int enableId=EnableAndWaitForExposure(rig);
			rig.Channel.FuncFail=command => new TwainHelperException(TwainHelperFailure.Crashed,"The 32-bit helper program (OpenPmsDentalHelper32.exe) "
				+"ended during Twain Poll (exit code -1).");
			AssertEndedByFailure(rig,enableId,"program ended during the capture session");
		}

		[TestMethod]
		public void HelperStopsAnswering_EnableEndsWithError_SessionClosed() {
			HelperRig rig=Track(new HelperRig());
			Configure(rig.Helper,x => x.End=FakeHelperEnd.Never);
			rig.OpenOrFail();
			int enableId=EnableAndWaitForExposure(rig);
			rig.Channel.FuncFail=command => new TwainHelperException(TwainHelperFailure.TimedOut,"The 32-bit helper program did not answer Twain Poll "
				+"within 5 seconds.");
			AssertEndedByFailure(rig,enableId,"stopped answering");
		}

		[TestMethod]
		public void HelperReplacedBetweenCalls_EnableEndsWithError_SessionClosed() {
			HelperRig rig=Track(new HelperRig());
			Configure(rig.Helper,x => x.End=FakeHelperEnd.Never);
			rig.OpenOrFail();
			int enableId=EnableAndWaitForExposure(rig);
			rig.Channel.ProcessIdValue=5151;//Another helper answers: the one that had the source open is gone.
			AssertEndedByFailure(rig,enableId,"program ended during the capture session");
		}

		[TestMethod]
		public void HelperLostTheSession_RemoteError_EnableEndsWithError_SessionClosed() {
			HelperRig rig=Track(new HelperRig());
			Configure(rig.Helper,x => x.End=FakeHelperEnd.Never);
			rig.OpenOrFail();
			int enableId=EnableAndWaitForExposure(rig);
			rig.Channel.FuncFail=command => command==TwainHelperCommands.Poll
				? new TwainHelperException(TwainHelperFailure.Remote,"This 32-bit helper has no TWAIN session x.") : null;
			AssertEndedByFailure(rig,enableId,"program failed during the capture session");
		}

		///<summary>No helper (missing, or it does not start): the open fails at once with the reason, nothing is left open.</summary>
		[TestMethod]
		public void HelperMissing_OpenFailsWithTheReason_NothingOpen() {
			HelperRig rig=Track(new HelperRig());
			rig.Channel.FuncFail=command => new TwainHelperException(TwainHelperFailure.Missing,"The 32-bit helper program OpenPmsDentalHelper32.exe was "
				+"not found at C:\\Program Files\\Open PMS - Dental\\Helper32\\OpenPmsDentalHelper32.exe.");
			TwainCaptureOpenResult result=rig.Open();
			Assert.IsFalse(result.IsOpen);
			Assert.AreEqual(TwainCaptureOpenError.Other,result.Error);
			StringAssert.Contains(result.ErrorText,"could not be started");
			StringAssert.Contains(result.ErrorText,"was not found");
			Assert.AreEqual(TwainCaptureState.Closed,rig.Capture.State);
			Assert.IsTrue(rig.Capture.WaitForClose(TimeSpan.Zero));
			Assert.AreEqual(0,rig.Capture.Enable(new TwainEnableRequest()));
			Assert.AreEqual(0,rig.Fake.CountEnable);
		}

		///<summary>The helper's open fails (the source is not there): its result, as the in-process session gives it, and the session is gone in
		///the helper too.</summary>
		[TestMethod]
		public void OpenSourceNotFound_SameResultAsInProcess_HelperForgetsIt() {
			CaptureSessionHarness harness=Track(new CaptureSessionHarness());
			HelperRig rig=Track(new HelperRig());
			TwainCaptureOpenResult resultIn=harness.Capture.Open("No Such Source",harness.Options);
			TwainCaptureOpenResult resultHelper=rig.Capture.Open("No Such Source",rig.Options);
			Assert.IsFalse(resultHelper.IsOpen);
			Assert.AreEqual(resultIn.Error,resultHelper.Error);
			Assert.AreEqual(resultIn.ErrorText,resultHelper.ErrorText);
			Assert.AreEqual(0,rig.Channel.Server.CountSessions);
			Assert.IsTrue(rig.Capture.WaitForClose(TimeSpan.Zero));
		}
		#endregion The helper's failures end the session cleanly

		#region Native first, the stored choice
		///<summary>A native fake DSM and the 32-bit one behind a helper channel, each with its own sources.</summary>
		private sealed class PathRig:IDisposable {
			public readonly FakeTwainDsm FakeNative=new FakeTwainDsm();
			public readonly FakeTwainDsm Fake32=new FakeTwainDsm();
			public readonly MemoryChannel Channel;
			public bool Is32BitDsmPresent=true;
			public int CountCreateNative;
			public int CountCreateHelper;

			public PathRig(List<string> listNative,List<string> list32) {
				FakeNative.ListSourceNames=listNative;
				Fake32.ListSourceNames=list32;
				Channel=new MemoryChannel(new TwainHelperServer((out string error) => {
					error="";
					return Fake32;
				},null,false));
			}

			public TwainSourcePath CreatePath(bool is64BitProcess=true,bool hasChannel=true) {
				return new TwainSourcePath((out string error) => {
					error="";
					return FakeNative;
				},hasChannel ? (Func<ITwainHelperChannel>)(() => Channel) : null,() => Is32BitDsmPresent,is64BitProcess);
			}

			public void Dispose() {
				Channel.Dispose();
			}
		}

		[TestMethod]
		public void Resolve_NativeFirst_32BitOnlyThroughTheHelper() {
			PathRig rig=Track(new PathRig(new List<string> { "Both Sensor","Native Only" },new List<string> { "Both Sensor","Old Sensor" }));
			TwainSourcePath path=rig.CreatePath();
			Assert.AreEqual(TwainDsmKind.Native,path.Resolve("Both Sensor").Dsm,"A source in both runs in 64-bit.");
			Assert.AreEqual(TwainDsmKind.Native,path.Resolve("Native Only").Dsm);
			TwainSourceResolution resolutionOld=path.Resolve("Old Sensor");
			Assert.AreEqual(TwainDsmKind.Helper32,resolutionOld.Dsm,"A choice from before S3b-8 (no marker) still works through the helper.");
			Assert.AreEqual("Old Sensor",resolutionOld.ProductName);
			Assert.IsNull(resolutionOld.ErrorText);
			TwainSourceResolution resolutionMarked=path.Resolve("Old Sensor"+TwainSourceChoice.Marker32);
			Assert.AreEqual(TwainDsmKind.Helper32,resolutionMarked.Dsm);
			Assert.AreEqual("Old Sensor",resolutionMarked.ProductName);
			TwainSourceResolution resolutionNowNative=path.Resolve("Both Sensor"+TwainSourceChoice.Marker32);
			Assert.AreEqual(TwainDsmKind.Native,resolutionNowNative.Dsm,"Chosen from the 32-bit DSM, and has a 64-bit driver now: 64-bit.");
			Assert.AreEqual("Both Sensor",resolutionNowNative.ProductName);
			StringAssert.Contains(resolutionNowNative.Reason,"has a 64-bit driver now");
			TwainSourceResolution resolutionGone=path.Resolve("Gone Sensor"+TwainSourceChoice.Marker32);
			Assert.AreEqual(TwainDsmKind.Helper32,resolutionGone.Dsm);
			StringAssert.Contains(resolutionGone.ErrorText,"\"Gone Sensor\" is not installed");
			TwainSourceResolution resolutionNowhere=path.Resolve("Gone Sensor");
			Assert.AreEqual(TwainDsmKind.Native,resolutionNowhere.Dsm,"Listed nowhere: the in-process session reports it not found, as before.");
			Assert.IsNull(resolutionNowhere.ErrorText);
		}

		[TestMethod]
		public void Resolve_A32BitProgram_OrNo32BitDsm_NeverTheHelper() {
			PathRig rig=Track(new PathRig(new List<string> { "Native Only" },new List<string> { "Old Sensor" }));
			Assert.AreEqual(TwainDsmKind.Native,rig.CreatePath(is64BitProcess:false).Resolve("Old Sensor").Dsm);
			Assert.AreEqual(TwainDsmKind.Native,rig.CreatePath(hasChannel:false).Resolve("Old Sensor").Dsm);
			Assert.AreEqual(0,rig.Channel.ListCommands.Count,"The helper is not asked.");
			rig.Is32BitDsmPresent=false;
			Assert.AreEqual(TwainDsmKind.Native,rig.CreatePath().Resolve("Old Sensor").Dsm);
			Assert.AreEqual(0,rig.Channel.ListCommands.Count,"No 32-bit DSM: the helper is not started.");
			TwainSourceResolution resolution=rig.CreatePath().Resolve("Old Sensor"+TwainSourceChoice.Marker32);
			StringAssert.Contains(resolution.ErrorText,"No 32-bit TWAIN data source manager is installed");
		}

		[TestMethod]
		public void ListAll_NativeFirst_32BitOnlyWithTheMarker_EachOnce() {
			PathRig rig=Track(new PathRig(new List<string> { "Both Sensor","Native Only" },new List<string> { "both sensor","Old Sensor" }));
			string error;
			List<TwainSourceEntry> listSources=rig.CreatePath().ListAll(out error);
			Assert.AreEqual("",error);
			CollectionAssert.AreEqual(new[] { "Both Sensor","Native Only","Old Sensor"+TwainSourceChoice.Marker32 },
				listSources.Select(x => x.StoredText).ToArray());
			CollectionAssert.AreEqual(new[] { TwainDsmKind.Native,TwainDsmKind.Native,TwainDsmKind.Helper32 },listSources.Select(x => x.Dsm).ToArray());
			Assert.AreEqual("FakeTwainDsm",listSources[2].DsmLibrary);
			CollectionAssert.Contains(rig.Channel.ListCommands,TwainHelperCommands.Hello);
		}

		[TestMethod]
		public void ListHelper32_HelperFails_ErrorTextAndNoSources() {
			PathRig rig=Track(new PathRig(new List<string> { "Native Only" },new List<string> { "Old Sensor" }));
			rig.Channel.FuncFail=command => new TwainHelperException(TwainHelperFailure.TimedOut,"did not answer Twain Hello within 60 seconds");
			string error;
			List<TwainSourceEntry> listSources=rig.CreatePath().ListAll(out error);
			CollectionAssert.AreEqual(new[] { "Native Only" },listSources.Select(x => x.StoredText).ToArray());
			StringAssert.Contains(error,"could not list the 32-bit TWAIN sources");
			TwainSourceResolution resolution=rig.CreatePath().Resolve("Old Sensor"+TwainSourceChoice.Marker32);
			StringAssert.Contains(resolution.ErrorText,"did not answer");
		}

		///<summary>The router: the session the choice needs, with what was set before Open; a failed resolution opens nothing.</summary>
		[TestMethod]
		public void Router_CreatesTheChosenSession_WithWhatWasSetBefore() {
			PathRig rig=Track(new PathRig(new List<string> { "Native Only" },new List<string> { "Old Sensor" }));
			StubSession stubNative=new StubSession();
			StubSession stubHelper=new StubSession();
			Func<string,CaptureMetadataKeys> funcKeys=ReadKeys;
			Guid guid=Guid.NewGuid();
			TwainCaptureSessionRouter router=new TwainCaptureSessionRouter(rig.CreatePath(),() => stubNative,() => stubHelper,null);
			router.SessionGuid=guid;
			router.QuarantineContext=new TwainCaptureQuarantineContext(9,"Router, Test");
			router.FuncReadMetadataKeys=funcKeys;
			string logged=null;
			router.Logged+=x => logged=x;
			router.Open("Old Sensor"+TwainSourceChoice.Marker32,new TwainCaptureOptions());
			Assert.AreSame(stubHelper,router.Inner);
			Assert.AreEqual("Old Sensor",stubHelper.OpenedName,"The ProductName without the marker is opened.");
			Assert.AreEqual(guid,stubHelper.SessionGuid);
			Assert.AreEqual(9,stubHelper.QuarantineContext.PatNum);
			Assert.AreSame(funcKeys,stubHelper.FuncReadMetadataKeys);
			stubHelper.RaiseLogged("a line");
			Assert.AreEqual("a line",logged,"The chosen session's events are the router's.");
			TwainCaptureSessionRouter routerNative=new TwainCaptureSessionRouter(rig.CreatePath(),() => stubNative,() => stubHelper,null);
			routerNative.Open("Native Only",new TwainCaptureOptions());
			Assert.AreSame(stubNative,routerNative.Inner);
			Assert.AreEqual("Native Only",stubNative.OpenedName);
			TwainCaptureSessionRouter routerGone=new TwainCaptureSessionRouter(rig.CreatePath(),() => new StubSession(),() => new StubSession(),null);
			TwainCaptureOpenResult result=routerGone.Open("Gone"+TwainSourceChoice.Marker32,new TwainCaptureOptions());
			Assert.IsFalse(result.IsOpen);
			Assert.AreEqual(TwainCaptureOpenError.Other,result.Error,"Other, so that the window shows the reason.");
			Assert.IsNull(((StubSession)routerGone.Inner).OpenedName,"Nothing is opened.");
		}

		///<summary>The stored text: a 64-bit choice is its ProductName, a 32-bit-only one carries the marker, and both read back.</summary>
		[TestMethod]
		public void SourceChoice_RoundTrips_AndAnOldChoiceIsUnspecified() {
			Assert.AreEqual("Sensor X",TwainSourceChoice.Format("Sensor X",TwainDsmKind.Native));
			Assert.AreEqual("Sensor X (32-bit TWAIN)",TwainSourceChoice.Format(" Sensor X ",TwainDsmKind.Helper32));
			TwainSourceChoice choice32=TwainSourceChoice.Parse("Sensor X (32-bit TWAIN)");
			Assert.AreEqual("Sensor X",choice32.ProductName);
			Assert.AreEqual(TwainDsmKind.Helper32,choice32.Dsm);
			Assert.AreEqual("Sensor X (32-bit TWAIN)",choice32.StoredText);
			TwainSourceChoice choiceOld=TwainSourceChoice.Parse(" Sensor X ");
			Assert.AreEqual("Sensor X",choiceOld.ProductName);
			Assert.AreEqual(TwainDsmKind.Unspecified,choiceOld.Dsm);
			Assert.AreEqual("",TwainSourceChoice.Parse(null).ProductName);
			Assert.AreEqual(TwainDsmKind.Unspecified,TwainSourceChoice.Parse(TwainSourceChoice.Marker32.Trim()).Dsm,"The marker alone names no source.");
			Assert.AreEqual("Sensor X",TwainSourceChoice.GetProductName("Sensor X (32-BIT twain)"),"The marker ignoring case.");
		}

		///<summary>A session that records what the router did with it.</summary>
		private sealed class StubSession:ITwainCaptureSession {
			public string OpenedName;
			public TwainCaptureState State => TwainCaptureState.Closed;
			public Guid SessionGuid { get; set; }
			public string ProductName => OpenedName??"";
			public TwainCaptureOptions Options => new TwainCaptureOptions();
			public TwainCaptureSourceInfo SourceInfo => null;
			public TwainCaptureQuarantineContext QuarantineContext { get; set; }
			public Func<string,CaptureMetadataKeys> FuncReadMetadataKeys { get; set; }
			public event Action<AcquiredImage> ImageAcquired;
			public event Action<TwainCaptureEnded> Ended;
			public event Action<TwainCaptureStatus> StatusChanged;
			public event Action<AcquiredImage> ImageQuarantined;
			public event Action<string> Logged;

			public TwainCaptureOpenResult Open(string productName,TwainCaptureOptions options) {
				OpenedName=productName;
				return new TwainCaptureOpenResult { IsOpen=true };
			}

			public void RaiseLogged(string line) {
				Logged?.Invoke(line);
				ImageAcquired?.Invoke(null);
				Ended?.Invoke(null);
				StatusChanged?.Invoke(null);
				ImageQuarantined?.Invoke(null);
			}

			public int Enable(TwainEnableRequest request) { return 0; }
			public int RequestRedelivery(TwainEnableRequest request) { return 0; }
			public void Disable(TwainCaptureStopReason reason) { }
			public void Cancel() { }
			public void BeginClose() { }
			public bool WaitForClose(TimeSpan timeout) { return true; }
			public string QuarantineImage(AcquiredImage image,string reason,bool isPatientUnknown,out string error) { error=""; return null; }
			public bool ConfirmRedeliveredJournalCopy(AcquiredImage image,out string error) { error=""; return false; }
			public void Dispose() { }
		}
		#endregion Native first, the stored choice
	}
}
