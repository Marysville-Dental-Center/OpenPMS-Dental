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

			///<summary>What the helper's server loads: Fake, or what the rig's caller wrapped it in.</summary>
			private readonly ITwainDsm _dsm;

			public HelperRig():this(null) {
			}

			///<summary>funcWrap: the DSM the helper's server loads, made from Fake (a GatedDsm), or null for Fake itself.</summary>
			public HelperRig(Func<FakeTwainDsm,ITwainDsm> funcWrap) {
				_dsm=funcWrap==null ? Fake : funcWrap(Fake);
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
				return _dsm;
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
		///<summary>A native fake DSM and the 32-bit one behind a helper channel, each with its own sources, and routers whose in-process sessions
		///run on the native fake (capture folders in a temporary folder) and whose helper sessions are StubSessions.</summary>
		private sealed class PathRig:IDisposable {
			public readonly FakeTwainDsm FakeNative=new FakeTwainDsm();
			public readonly FakeTwainDsm Fake32=new FakeTwainDsm();
			public readonly MemoryChannel Channel;
			public readonly string Folder;
			public bool Is32BitDsmPresent=true;
			///<summary>The DSM the native sessions and the native listing load: FakeNative, or a GatedDsm around it.</summary>
			public ITwainDsm DsmNative;
			public readonly List<TwainCaptureSession> ListNativeSessions=new List<TwainCaptureSession>();
			public readonly List<StubSession> ListHelperSessions=new List<StubSession>();
			public readonly TwainCaptureOptions Options=new TwainCaptureOptions {
				TickMilliseconds=20,
				RefusedDisableWait=TimeSpan.FromSeconds(2),
				OpenTimeout=TimeSpan.FromSeconds(10),
			};

			public PathRig(List<string> listNative,List<string> list32) {
				FakeNative.ListSourceNames=listNative;
				Fake32.ListSourceNames=list32;
				DsmNative=FakeNative;
				Channel=new MemoryChannel(new TwainHelperServer((out string error) => {
					error="";
					return Fake32;
				},null,isTestFolderAllowed:true));//a helper session a test opens writes into Folder
				Folder=Path.Combine(Path.GetTempPath(),"TwainHelperPathTests_"+Guid.NewGuid().ToString("N"));
				Directory.CreateDirectory(Folder);
			}

			public ITwainDsm LoadNative(out string error) {
				error="";
				return DsmNative;
			}

			public TwainSourcePath CreatePath(bool is64BitProcess=true,bool hasChannel=true) {
				return new TwainSourcePath(LoadNative,hasChannel ? (Func<ITwainHelperChannel>)(() => Channel) : null,() => Is32BitDsmPresent,is64BitProcess);
			}

			public TwainCaptureSession CreateNativeSession() {
				TwainCaptureSession session=new TwainCaptureSession(LoadNative,CaptureFolders.CreateForFolder(Folder),() => DateTime.Now);
				lock(ListNativeSessions) {
					ListNativeSessions.Add(session);
				}
				return session;
			}

			public TwainCaptureSessionRouter CreateRouter(bool is64BitProcess=true,bool hasChannel=true) {
				return new TwainCaptureSessionRouter(CreatePath(is64BitProcess,hasChannel),CreateNativeSession,() => {
					StubSession stub=new StubSession();
					lock(ListHelperSessions) {
						ListHelperSessions.Add(stub);
					}
					return stub;
				},CaptureFolders.CreateForFolder(Folder));
			}

			///<summary>Opens storedText in a new router and closes what it opened; the router, for its Resolution and Inner.</summary>
			public TwainCaptureSessionRouter OpenAndClose(string storedText,out TwainCaptureOpenResult result,bool is64BitProcess=true,bool hasChannel=true) {
				TwainCaptureSessionRouter router=CreateRouter(is64BitProcess,hasChannel);
				result=router.Open(storedText,Options);
				router.BeginClose();
				Assert.IsTrue(router.WaitForClose(TimeSpan.FromSeconds(15)),"The router's session closes.");
				return router;
			}

			public int CountListSources => Channel.CountCommands(TwainHelperCommands.ListSources);

			public void Dispose() {
				List<TwainCaptureSession> listSessions;
				lock(ListNativeSessions) {
					listSessions=new List<TwainCaptureSession>(ListNativeSessions);
				}
				foreach(TwainCaptureSession session in listSessions) {
					session.BeginClose();
					session.WaitForClose(TimeSpan.FromSeconds(15));
				}
				Channel.Dispose();
				try {
					Directory.Delete(Folder,true);
				}
				catch {
				}
			}
		}

		///<summary>Native first: a source the program's DSM lists opens in process and the helper is not asked; one only the 32-bit DSM lists
		///runs through the helper, with or without the marker; a marked choice that has a 64-bit driver now runs in 64-bit; a marked choice listed
		///nowhere fails with the 32-bit reason; an unmarked one listed nowhere is reported not found by the in-process session, as before.
		///</summary>
		[TestMethod]
		public void Router_NativeFirst_32BitOnlyThroughTheHelper() {
			PathRig rig=Track(new PathRig(new List<string> { "Both Sensor","Native Only" },new List<string> { "Both Sensor","Old Sensor" }));
			TwainCaptureOpenResult result;
			TwainCaptureSessionRouter routerBoth=rig.OpenAndClose("Both Sensor",out result);
			Assert.IsTrue(result.IsOpen,result.ErrorText);
			Assert.IsInstanceOfType(routerBoth.Inner,typeof(TwainCaptureSession),"A source in both runs in 64-bit.");
			Assert.AreEqual(TwainDsmKind.Native,routerBoth.Resolution.Dsm);
			Assert.AreEqual(0,rig.CountListSources,"A source the program's DSM lists: the helper is not asked.");
			TwainCaptureSessionRouter routerNative=rig.OpenAndClose("Native Only",out result);
			Assert.IsTrue(result.IsOpen,result.ErrorText);
			Assert.AreEqual(TwainDsmKind.Native,routerNative.Resolution.Dsm);
			Assert.AreEqual(0,rig.CountListSources);
			TwainCaptureSessionRouter routerOld=rig.OpenAndClose("Old Sensor",out result);
			Assert.IsTrue(result.IsOpen,result.ErrorText);
			Assert.AreEqual(TwainDsmKind.Helper32,routerOld.Resolution.Dsm,"A choice from before S3b-8 (no marker) still works through the helper.");
			Assert.AreEqual("Old Sensor",routerOld.Resolution.ProductName);
			Assert.IsNull(routerOld.Resolution.ErrorText);
			Assert.AreSame(rig.ListHelperSessions.Last(),routerOld.Inner);
			Assert.AreEqual("Old Sensor",rig.ListHelperSessions.Last().OpenedName);
			TwainCaptureSessionRouter routerMarked=rig.OpenAndClose("Old Sensor"+TwainSourceChoice.Marker32,out result);
			Assert.IsTrue(result.IsOpen,result.ErrorText);
			Assert.AreEqual(TwainDsmKind.Helper32,routerMarked.Resolution.Dsm);
			Assert.AreEqual("Old Sensor",rig.ListHelperSessions.Last().OpenedName,"The ProductName without the marker is opened.");
			int countHelperSessions=rig.ListHelperSessions.Count;
			TwainCaptureSessionRouter routerNowNative=rig.OpenAndClose("Both Sensor"+TwainSourceChoice.Marker32,out result);
			Assert.IsTrue(result.IsOpen,result.ErrorText);
			Assert.AreEqual(TwainDsmKind.Native,routerNowNative.Resolution.Dsm,"Chosen from the 32-bit DSM, and has a 64-bit driver now: 64-bit.");
			Assert.AreEqual("Both Sensor",routerNowNative.Resolution.ProductName);
			StringAssert.Contains(routerNowNative.Resolution.Reason,"has a 64-bit driver now");
			Assert.AreEqual(countHelperSessions,rig.ListHelperSessions.Count,"No helper session.");
			TwainCaptureSessionRouter routerGone=rig.OpenAndClose("Gone Sensor"+TwainSourceChoice.Marker32,out result);
			Assert.IsFalse(result.IsOpen);
			Assert.AreEqual(TwainCaptureOpenError.Other,result.Error,"Other, so that the window shows the reason.");
			StringAssert.Contains(result.ErrorText,"\"Gone Sensor\" is not installed");
			Assert.AreEqual(TwainDsmKind.Helper32,routerGone.Resolution.Dsm);
			Assert.AreEqual(countHelperSessions,rig.ListHelperSessions.Count,"Nothing is opened in the helper.");
			TwainCaptureSessionRouter routerNowhere=rig.OpenAndClose("Gone Sensor",out result);
			Assert.AreEqual(TwainCaptureOpenError.SourceNotFound,result.Error,"Listed nowhere: the in-process session reports it not found, as before.");
			Assert.AreEqual(TwainDsmKind.Native,routerNowhere.Resolution.Dsm);
			Assert.IsNull(routerNowhere.Resolution.ErrorText);
			Assert.AreEqual(countHelperSessions,rig.ListHelperSessions.Count);
		}

		///<summary>The program's DSM is asked by the exact name, as the earlier listing compared whole names: a 32-bit-only "Sensor" is not taken
		///for a 64-bit "Sensor Pro"; an unmarked name that only starts a native source's name still opens it in process, as before S3b-8.</summary>
		[TestMethod]
		public void Router_NativeByTheExactName_APrefixIsNotA64BitSource() {
			PathRig rig=Track(new PathRig(new List<string> { "Sensor Pro" },new List<string> { "Sensor" }));
			TwainCaptureOpenResult result;
			TwainCaptureSessionRouter router=rig.OpenAndClose("Sensor",out result);
			Assert.IsTrue(result.IsOpen,result.ErrorText);
			Assert.AreEqual(TwainDsmKind.Helper32,router.Resolution.Dsm);
			Assert.AreEqual("Sensor",rig.ListHelperSessions.Single().OpenedName);
			TwainCaptureSessionRouter routerPrefix=rig.OpenAndClose("Sensor P",out result);
			Assert.IsTrue(result.IsOpen,result.ErrorText);
			Assert.IsInstanceOfType(routerPrefix.Inner,typeof(TwainCaptureSession));
			Assert.AreEqual("Sensor Pro",routerPrefix.Inner.ProductName,"The in-process session's own match, as before.");
			Assert.AreEqual(1,rig.ListHelperSessions.Count);
		}

		///<summary>A source the program's DSM lists whose MSG_OPENDS fails stays a 64-bit failure: the 32-bit DSM, which lists it too, is not
		///asked (a source available in 64-bit is always used in 64-bit).</summary>
		[TestMethod]
		public void Router_ListedNativelyButFailsToOpen_NotTriedIn32Bit() {
			PathRig rig=Track(new PathRig(new List<string> { "Both Sensor" },new List<string> { "Both Sensor" }));
			GatedDsm gated=new GatedDsm(rig.FakeNative) { IsOpenDsFailing=true };
			rig.DsmNative=gated;
			TwainCaptureOpenResult result;
			TwainCaptureSessionRouter router=rig.OpenAndClose("Both Sensor",out result);
			Assert.IsFalse(result.IsOpen);
			Assert.AreEqual(TwainCaptureOpenError.SourceNotFound,result.Error);
			Assert.IsFalse(result.IsSourceNotListed);
			Assert.AreEqual(TwainDsmKind.Native,router.Resolution.Dsm);
			Assert.AreEqual(0,rig.CountListSources,"The helper is not asked.");
			Assert.AreEqual(0,rig.ListHelperSessions.Count);
		}

		///<summary>The review finding of S3b-8: the open of a source whose earlier in-process session is still closing (MSG_DISABLEDS blocks) sends
		///no DSM_Entry at all, not even to enumerate, until that session's MSG_CLOSEDS; then it opens the source in process. The fake DSM records
		///the thread of every call.</summary>
		[TestMethod]
		public void Router_AnotherSessionClosingTheSource_NoDsmEntryBeforeItsCloseDs() {
			PathRig rig=Track(new PathRig(new List<string>(),new List<string> { "Old Sensor" }));
			FakeHelperSource helper=new FakeHelperSource();
			Configure(helper,x => x.End=FakeHelperEnd.Never);//a sensor waiting for an exposure
			rig.FakeNative.AddHelperSource(helper);
			GatedDsm gated=new GatedDsm(rig.FakeNative);
			rig.DsmNative=gated;
			TwainCaptureSession sessionOld=rig.CreateNativeSession();
			Recorder recorderOld=new Recorder(sessionOld);
			TwainCaptureOpenResult resultOld=sessionOld.Open(helper.ProductName,rig.Options);
			Assert.IsTrue(resultOld.IsOpen,resultOld.ErrorText);
			Assert.IsTrue(sessionOld.Enable(new TwainEnableRequest { Bits=8,TargetLabel="Slot 1" })>0);
			recorderOld.WaitUntil(() => recorderOld.ListStatus.Any(x => x.State==TwainCaptureState.WaitingForImage),"waiting for the exposure");
			gated.IsDisableBlocking=true;
			sessionOld.BeginClose();//the window closes; the worker blocks in MSG_DISABLEDS
			Assert.IsTrue(gated.EventDisableReached.WaitOne(10000),"The old session is in MSG_DISABLEDS.");
			TwainCaptureSessionRouter router=rig.CreateRouter();
			TwainCaptureOpenResult resultNew=null;
			Thread threadOpen=new Thread(() => resultNew=router.Open(helper.ProductName,rig.Options)) { IsBackground=true };//as the window's thread-pool open
			threadOpen.Start();
			bool isOpenDoneEarly=threadOpen.Join(700);//time enough for an open that enumerated outside the session to reach the DSM
			gated.EventDisableGate.Set();
			Assert.IsTrue(threadOpen.Join(20000),"The new open finishes once the old session has closed.");
			Assert.IsFalse(isOpenDoneEarly,"The new open waited for the old session.");
			Assert.IsTrue(resultNew.IsOpen,resultNew.ErrorText);
			Assert.IsInstanceOfType(router.Inner,typeof(TwainCaptureSession));
			Assert.IsTrue(sessionOld.WaitForClose(TimeSpan.Zero));
			List<KeyValuePair<int,string>> listEntries=gated.Entries;
			int idxClosed=AssertOnlyTheClosingThreadUntilClosed(listEntries);
			Assert.IsTrue(listEntries.FindLastIndex(x => x.Value=="OPENDS")>idxClosed,"The new open opened the source afterwards.\r\n"+Describe(listEntries));
			Assert.AreEqual(0,rig.CountListSources,"The helper is not asked.");
			router.BeginClose();
			Assert.IsTrue(router.WaitForClose(TimeSpan.FromSeconds(15)));
		}

		///<summary>The helper lists the 32-bit sources only once no session of the helper is closing: ListSources waits for a session in
		///MSG_DISABLEDS and enumerates after its MSG_CLOSEDS; while a session is open and not closing it refuses with the reason and calls nothing.
		///</summary>
		[TestMethod]
		public void HelperListSources_WaitsForAClosingSession_RefusesWhileOneIsOpen() {
			GatedDsm gated=null;
			HelperRig rig=Track(new HelperRig(fake => gated=new GatedDsm(fake)));
			Configure(rig.Helper,x => x.End=FakeHelperEnd.Never);
			rig.OpenOrFail();
			byte[] dataReply;
			string[] arrayRefused=rig.Channel.Call(TwainHelperCommands.ListSources,new string[0],null,out dataReply,TimeSpan.FromSeconds(30));
			StringAssert.Contains(arrayRefused[1],"Close that capture window first");
			Assert.AreEqual(0,TwainHelperWire.Read(dataReply,TwainHelperWire.ReadSources).Count);
			int threadTest=Thread.CurrentThread.ManagedThreadId;
			Assert.IsFalse(gated.Entries.Any(x => x.Key==threadTest),"The refused listing called nothing.");
			EnableAndWaitForExposure(rig);
			gated.IsDisableBlocking=true;
			rig.Capture.BeginClose();
			Assert.IsTrue(gated.EventDisableReached.WaitOne(10000),"The helper's session is in MSG_DISABLEDS.");
			string[] arrayResults=null;
			List<TwainSourceEntry> listSources=null;
			int threadList=0;
			Thread threadListing=new Thread(() => {
				threadList=Thread.CurrentThread.ManagedThreadId;
				byte[] dataList;
				arrayResults=rig.Channel.Call(TwainHelperCommands.ListSources,new string[0],null,out dataList,TimeSpan.FromSeconds(30));
				listSources=TwainHelperWire.Read(dataList,TwainHelperWire.ReadSources);
			}) { IsBackground=true };
			threadListing.Start();
			bool isListDoneEarly=threadListing.Join(700);
			gated.EventDisableGate.Set();
			Assert.IsTrue(threadListing.Join(20000),"The listing finishes once the session has closed.");
			Assert.IsFalse(isListDoneEarly,"The listing waited for the closing session.");
			Assert.AreEqual("",arrayResults[1],"No error.");
			CollectionAssert.Contains(listSources.Select(x => x.ProductName).ToList(),rig.Helper.ProductName);
			List<KeyValuePair<int,string>> listEntries=gated.Entries;
			int idxClosed=AssertOnlyTheClosingThreadUntilClosed(listEntries);
			Assert.IsTrue(listEntries.FindIndex(x => x.Key==threadList)>idxClosed,"The listing enumerated afterwards.\r\n"+Describe(listEntries));
			Assert.IsTrue(rig.Capture.WaitForClose(TimeSpan.FromSeconds(15)));
		}

		///<summary>From the first MSG_DISABLEDS (a session that is closing) until that thread's MSG_CLOSEDS and MSG_CLOSEDSM, every DSM_Entry is
		///that thread's: nothing else called the DSM meanwhile. The closing thread is alive throughout, so no other thread has its id. Returns the
		///index of its last call of the close (MSG_CLOSEDSM, or MSG_CLOSEDS).</summary>
		private static int AssertOnlyTheClosingThreadUntilClosed(List<KeyValuePair<int,string>> listEntries) {
			int idxDisable=listEntries.FindIndex(x => x.Value=="DISABLEDS");
			Assert.IsTrue(idxDisable>=0,"No MSG_DISABLEDS.\r\n"+Describe(listEntries));
			int threadClosing=listEntries[idxDisable].Key;
			int idxCloseDs=listEntries.FindIndex(idxDisable,x => x.Key==threadClosing && x.Value=="CLOSEDS");
			Assert.IsTrue(idxCloseDs>idxDisable,"The closing session closed the source.\r\n"+Describe(listEntries));
			int idxCloseDsm=listEntries.FindIndex(idxCloseDs,x => x.Key==threadClosing && x.Value=="CLOSEDSM");
			int idxClosed=idxCloseDsm>idxCloseDs ? idxCloseDsm : idxCloseDs;
			for(int i=idxDisable;i<=idxClosed;i++) {
				Assert.AreEqual(threadClosing,listEntries[i].Key,"Call "+i+" ("+listEntries[i].Value+") came from another thread while the session was "
					+"closing.\r\n"+Describe(listEntries));
			}
			return idxClosed;
		}

		private static string Describe(List<KeyValuePair<int,string>> listEntries) {
			return string.Join("\r\n",listEntries.Select((x,i) => i+": thread "+x.Key+" "+x.Value));
		}

		[TestMethod]
		public void Router_A32BitProgram_OrNo32BitDsm_NeverTheHelper() {
			PathRig rig=Track(new PathRig(new List<string> { "Native Only" },new List<string> { "Old Sensor" }));
			TwainCaptureOpenResult result;
			Assert.AreEqual(TwainDsmKind.Native,rig.OpenAndClose("Old Sensor",out result,is64BitProcess:false).Resolution.Dsm);
			Assert.AreEqual(TwainCaptureOpenError.SourceNotFound,result.Error);
			Assert.AreEqual(TwainDsmKind.Native,rig.OpenAndClose("Old Sensor",out result,hasChannel:false).Resolution.Dsm);
			Assert.AreEqual(0,rig.Channel.ListCommands.Count,"The helper is not asked.");
			rig.Is32BitDsmPresent=false;
			Assert.AreEqual(TwainDsmKind.Native,rig.OpenAndClose("Old Sensor",out result).Resolution.Dsm);
			Assert.AreEqual(0,rig.Channel.ListCommands.Count,"No 32-bit DSM: the helper is not started.");
			rig.OpenAndClose("Old Sensor"+TwainSourceChoice.Marker32,out result);
			StringAssert.Contains(result.ErrorText,"No 32-bit TWAIN data source manager is installed");
			Assert.AreEqual(0,rig.ListHelperSessions.Count);
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
			TwainCaptureOpenResult result;
			rig.OpenAndClose("Old Sensor"+TwainSourceChoice.Marker32,out result);
			Assert.IsFalse(result.IsOpen);
			StringAssert.Contains(result.ErrorText,"did not answer");
		}

		///<summary>An unmarked choice (stored before S3b-8, as every choice of the 32-bit program is) that only the 32-bit fake lists, while the
		///helper cannot list: the open fails with Other and the helper's reason, not "not found", and nothing is opened in the helper.</summary>
		private static void AssertUnmarkedHelperFailure(PathRig rig,string fragment) {
			int countHelperSessions=rig.ListHelperSessions.Count;
			TwainCaptureOpenResult result;
			TwainCaptureSessionRouter router=rig.OpenAndClose("Old Sensor",out result);
			Assert.IsFalse(result.IsOpen);
			Assert.AreEqual(TwainCaptureOpenError.Other,result.Error,"Other, so that the window shows the helper's reason and not \"not found\": "
				+result.ErrorText);
			StringAssert.Contains(result.ErrorText,"\"Old Sensor\" is not listed by the 64-bit TWAIN data source manager");
			StringAssert.Contains(result.ErrorText,"the 32-bit sources could not be checked");
			StringAssert.Contains(result.ErrorText,fragment);
			Assert.AreEqual(TwainDsmKind.Native,router.Resolution.Dsm);
			Assert.IsNull(router.Resolution.ErrorText);
			StringAssert.Contains(router.Resolution.Helper32ErrorText,fragment);
			Assert.AreEqual(countHelperSessions,rig.ListHelperSessions.Count,"Nothing is opened in the helper.");
			string pathLog=Path.Combine(CaptureFolders.CreateForFolder(rig.Folder).GetPath(CaptureFolders.SubfolderCapture),CaptureLog.FileName);
			StringAssert.Contains(File.ReadAllText(pathLog),"The source cannot be opened: "+result.ErrorText,"The capture log has the reason.");
		}

		///<summary>The review finding of S3b-8 (fix 3): the helper is missing (quarantined, not installed) at open time for an unmarked choice.
		///</summary>
		[TestMethod]
		public void Router_UnmarkedChoice_HelperMissing_OtherWithTheReason() {
			PathRig rig=Track(new PathRig(new List<string> { "Native Only" },new List<string> { "Old Sensor" }));
			rig.Channel.FuncFail=command => new TwainHelperException(TwainHelperFailure.Missing,"The 32-bit helper program OpenPmsDentalHelper32.exe was "
				+"not found at C:\\Program Files\\Open PMS - Dental\\Helper32\\OpenPmsDentalHelper32.exe.");
			AssertUnmarkedHelperFailure(rig,"OpenPmsDentalHelper32.exe was not found");
			//The same without the program's DSM: not "TWAIN is not installed", the helper's reason.
			rig.DsmNative=null;
			TwainCaptureOpenResult result;
			rig.OpenAndClose("Old Sensor",out result);
			Assert.AreEqual(TwainCaptureOpenError.Other,result.Error,result.ErrorText);
			StringAssert.Contains(result.ErrorText,"The 64-bit TWAIN data source manager is not installed");
			StringAssert.Contains(result.ErrorText,"OpenPmsDentalHelper32.exe was not found");
			//The helper back: the same unmarked choice opens through it.
			rig.DsmNative=rig.FakeNative;
			rig.Channel.FuncFail=null;
			TwainCaptureSessionRouter router=rig.OpenAndClose("Old Sensor",out result);
			Assert.IsTrue(result.IsOpen,result.ErrorText);
			Assert.AreEqual(TwainDsmKind.Helper32,router.Resolution.Dsm);
			Assert.IsNull(router.Resolution.Helper32ErrorText);
		}

		///<summary>The review finding of S3b-8 (fix 3): the helper does not answer while it lists, for an unmarked choice.</summary>
		[TestMethod]
		public void Router_UnmarkedChoice_HelperTimesOut_OtherWithTheReason() {
			PathRig rig=Track(new PathRig(new List<string> { "Native Only" },new List<string> { "Old Sensor" }));
			rig.Channel.FuncFail=command => command==TwainHelperCommands.ListSources
				? new TwainHelperException(TwainHelperFailure.TimedOut,"The 32-bit helper program did not answer Twain ListSources within 60 seconds.")
				: null;
			AssertUnmarkedHelperFailure(rig,"did not answer Twain ListSources within 60 seconds");
		}

		///<summary>The review finding of S3b-8 (fix 3): the helper refuses to list while another of its capture sessions has a source open, for an
		///unmarked choice; and an unmarked name the 64-bit DSM does not list, with no 32-bit DSM at all, is still "not found" as before.</summary>
		[TestMethod]
		public void Router_UnmarkedChoice_HelperBusyWithAnotherSession_OtherWithTheReason() {
			PathRig rig=Track(new PathRig(new List<string> { "Native Only" },new List<string> { "Old Sensor","Busy Scanner" }));
			TwainHelperCaptureSession sessionBusy=new TwainHelperCaptureSession(rig.Channel,CaptureFolders.CreateForFolder(rig.Folder),rig.Folder) {
				CallTimeout=TimeSpan.FromSeconds(5)
			};
			try {
				TwainCaptureOpenResult resultBusy=sessionBusy.Open("Busy Scanner",rig.Options);
				Assert.IsTrue(resultBusy.IsOpen,resultBusy.ErrorText);
				AssertUnmarkedHelperFailure(rig,"Close that capture window first");
			}
			finally {
				sessionBusy.BeginClose();
				Assert.IsTrue(sessionBusy.WaitForClose(TimeSpan.FromSeconds(15)),"The helper session on the other source closes.");
			}
			rig.Is32BitDsmPresent=false;
			TwainCaptureOpenResult result;
			TwainCaptureSessionRouter router=rig.OpenAndClose("Old Sensor",out result);
			Assert.AreEqual(TwainCaptureOpenError.SourceNotFound,result.Error,"No 32-bit DSM: not found, as before.");
			Assert.IsNull(router.Resolution.Helper32ErrorText);
		}

		///<summary>The router: the session the choice needs, with what was set before Open; the in-process session is asked first, by the whole
		///stored text and then by the ProductName, exactly; a failed resolution opens nothing in the helper; a close during the choice opens nothing.
		///</summary>
		[TestMethod]
		public void Router_CreatesTheChosenSession_WithWhatWasSetBefore() {
			PathRig rig=Track(new PathRig(new List<string> { "Native Only" },new List<string> { "Old Sensor" }));
			StubSession stubNative=new StubSession { SetNames=new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Native Only" } };
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
			CollectionAssert.AreEqual(new[] { "Old Sensor"+TwainSourceChoice.Marker32,"Old Sensor" },stubNative.ListOpened,
				"The program's own DSM first: the whole stored text, then the ProductName.");
			Assert.IsTrue(stubNative.ListExact.All(x => x),"Exactly.");
			Assert.AreSame(stubHelper,router.Inner);
			Assert.AreEqual("Old Sensor",stubHelper.OpenedName,"The ProductName without the marker is opened.");
			Assert.IsFalse(stubHelper.ListExact.Single(),"The helper's session opens the name its DSM listed.");
			Assert.AreEqual(guid,stubHelper.SessionGuid);
			Assert.AreEqual(9,stubHelper.QuarantineContext.PatNum);
			Assert.AreSame(funcKeys,stubHelper.FuncReadMetadataKeys);
			stubHelper.RaiseLogged("a line");
			Assert.AreEqual("a line",logged,"The chosen session's events are the router's.");
			stubNative.RaiseLogged("an old line");
			Assert.AreEqual("a line",logged,"The session that is no longer Inner raises nothing through the router.");
			StubSession stubNative2=new StubSession { SetNames=new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Native Only" } };
			StubSession stubHelper2=new StubSession();
			TwainCaptureSessionRouter routerNative=new TwainCaptureSessionRouter(rig.CreatePath(),() => stubNative2,() => stubHelper2,null);
			routerNative.Open("Native Only",new TwainCaptureOptions());
			Assert.AreSame(stubNative2,routerNative.Inner);
			CollectionAssert.AreEqual(new[] { "Native Only" },stubNative2.ListOpened);
			Assert.IsNull(stubHelper2.OpenedName);
			StubSession stubHelperGone=new StubSession();
			int countHelpers=0;
			TwainCaptureSessionRouter routerGone=new TwainCaptureSessionRouter(rig.CreatePath(),() => new StubSession { SetNames=new HashSet<string>() },
				() => {
					countHelpers++;
					return stubHelperGone;
				},null);
			TwainCaptureOpenResult result=routerGone.Open("Gone"+TwainSourceChoice.Marker32,new TwainCaptureOptions());
			Assert.IsFalse(result.IsOpen);
			Assert.AreEqual(TwainCaptureOpenError.Other,result.Error,"Other, so that the window shows the reason.");
			Assert.AreEqual(0,countHelpers,"Nothing is opened in the helper.");
			StubSession stubHelperClosed=new StubSession();
			TwainCaptureSessionRouter routerClosed=null;
			StubSession stubNativeClosed=new StubSession { SetNames=new HashSet<string>(),FuncOnOpen=() => routerClosed.BeginClose() };
			routerClosed=new TwainCaptureSessionRouter(rig.CreatePath(),() => stubNativeClosed,() => stubHelperClosed,null);
			TwainCaptureOpenResult resultClosed=routerClosed.Open("Old Sensor",new TwainCaptureOptions());
			Assert.IsFalse(resultClosed.IsOpen);
			StringAssert.Contains(resultClosed.ErrorText,"closed while it was opening");
			Assert.IsNull(stubHelperClosed.OpenedName,"Closed during the choice: the helper's session is not opened.");
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

		///<summary>A DSM around a FakeTwainDsm that records the thread and the triplet of every DSM_Entry, can hold MSG_DISABLEDS at a gate (a
		///source whose disable blocks while its software closes), and can fail MSG_OPENDS.</summary>
		internal sealed class GatedDsm:ITwainDsm {
			public readonly FakeTwainDsm Fake;
			private readonly object _lock=new object();
			private readonly List<KeyValuePair<int,string>> _listEntries=new List<KeyValuePair<int,string>>();
			///<summary>MSG_DISABLEDS waits for this while IsDisableBlocking.</summary>
			public readonly ManualResetEvent EventDisableGate=new ManualResetEvent(false);
			///<summary>Set when a MSG_DISABLEDS reached the gate.</summary>
			public readonly ManualResetEvent EventDisableReached=new ManualResetEvent(false);
			public volatile bool IsDisableBlocking;
			public bool IsOpenDsFailing;

			public GatedDsm(FakeTwainDsm fake) {
				Fake=fake;
			}

			public string LibraryName => Fake.LibraryName;

			///<summary>Every call so far: the managed thread id and the triplet's name.</summary>
			public List<KeyValuePair<int,string>> Entries {
				get {
					lock(_lock) {
						return new List<KeyValuePair<int,string>>(_listEntries);
					}
				}
			}

			public ushort Entry(IntPtr origin,IntPtr dest,uint dg,ushort dat,ushort msg,IntPtr data) {
				lock(_lock) {
					_listEntries.Add(new KeyValuePair<int,string>(Thread.CurrentThread.ManagedThreadId,Describe(dat,msg)));
				}
				if(dat==Tw.DAT_USERINTERFACE && msg==Tw.MSG_DISABLEDS && IsDisableBlocking) {
					EventDisableReached.Set();
					EventDisableGate.WaitOne();
				}
				if(IsOpenDsFailing && dat==Tw.DAT_IDENTITY && msg==Tw.MSG_OPENDS) {
					return Tw.TWRC_FAILURE;
				}
				return Fake.Entry(origin,dest,dg,dat,msg,data);
			}

			private static string Describe(ushort dat,ushort msg) {
				if(dat==Tw.DAT_PARENT) {
					return msg==Tw.MSG_OPENDSM ? "OPENDSM" : (msg==Tw.MSG_CLOSEDSM ? "CLOSEDSM" : "PARENT");
				}
				if(dat==Tw.DAT_IDENTITY) {
					switch(msg) {
						case Tw.MSG_OPENDS: return "OPENDS";
						case Tw.MSG_CLOSEDS: return "CLOSEDS";
						case Tw.MSG_GETFIRST: return "GETFIRST";
						case Tw.MSG_GETNEXT: return "GETNEXT";
					}
				}
				if(dat==Tw.DAT_USERINTERFACE) {
					return msg==Tw.MSG_DISABLEDS ? "DISABLEDS" : (msg==Tw.MSG_ENABLEDS ? "ENABLEDS" : "USERINTERFACE");
				}
				return "DAT 0x"+dat.ToString("X4")+" MSG 0x"+msg.ToString("X4");
			}

			public void Dispose() {
				Fake.Dispose();
			}
		}

		///<summary>A session that records what the router did with it. SetNames: the names its "DSM" lists (an open of another fails as not
		///listed); null opens every name.</summary>
		private sealed class StubSession:ITwainCaptureSession {
			public string OpenedName;
			public HashSet<string> SetNames;
			public readonly List<string> ListOpened=new List<string>();
			public readonly List<bool> ListExact=new List<bool>();
			public Action FuncOnOpen;
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
				ListOpened.Add(productName);
				ListExact.Add(options!=null && options.IsSourceNameExact);
				FuncOnOpen?.Invoke();
				if(SetNames!=null && !SetNames.Contains(productName)) {
					return new TwainCaptureOpenResult { Error=TwainCaptureOpenError.SourceNotFound,ErrorText="not installed",IsSourceNotListed=true };
				}
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
