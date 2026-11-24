// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using ImagingDeviceManager.Capture;
using ImagingDeviceManager.Journal;
using ImagingDeviceManager.TwainProtocol;

namespace ImagingDeviceManager.Helper32 {
	///<summary>The helper's side of the 32-bit TWAIN path (S3b-8; docs/port-notes/S3b-8.md): the commands of TwainHelperCommands on this
	///process's own DSM. In the 32-bit helper (OpenPmsDentalHelper32\Twain\TwainModule.cs) that is the 32-bit TWAINDSM.dll of SysWOW64 (or the
	///legacy twain_32.dll, NativeTwainDsm's 32-bit order). Each session is a whole TwainCaptureSession of this library, which drives the source
	///on its own STA worker thread with its own message loop and proxy window (the window the DSM and the source need), writes the journal copy
	///of every image before it raises it, quarantines what arrives after BeginClose, and logs to the capture log, all exactly as in the program's
	///own process; this class only queues what the session raises for the program's Poll, keeps the pixels of each image until ReadData takes
	///them, and maps the program's enable ids to the session's.
	///
	///Handle is called on the helper's one main thread, one command at a time; the session's events arrive on its worker. Open blocks the
	///main thread until the source is open (TwainCaptureSession.Open), ListSources first waits for the helper's sessions that are closing
	///(at most ListWaitTimeout), every other command returns at once, and Poll waits at most
	///TwainHelperCommands.POLL_WAIT_MAX_MS for an event.</summary>
	internal sealed class TwainHelperServer:IDisposable {
		private readonly DsmLoader _dsmLoader;
		private readonly Func<string,CaptureMetadataKeys> _funcReadMetadataKeys;
		///<summary>Tests: Open's capture folder argument is honoured (the helper was started with the test DSM). Otherwise the default folders.</summary>
		private readonly bool _isTestFolderAllowed;
		private readonly Func<DateTime> _funcNow;
		private readonly Dictionary<string,ServerSession> _dictSessions=new Dictionary<string,ServerSession>();
		///<summary>Every TwainCaptureSession this helper created that may not have closed yet, also one that Release or a failed Open dropped from
		///_dictSessions after a bounded wait: ListSources waits for them (WaitForSessionsBeforeListing).</summary>
		private readonly List<TwainCaptureSession> _listCaptures=new List<TwainCaptureSession>();

		///<summary>How long ListSources waits for the helper's sessions that are closing, well within the program's 60 s for a listing call
		///(TwainSourcePath.TimeSpanListHelper). Tests shorten it.</summary>
		internal TimeSpan ListWaitTimeout { get; set; }=TimeSpan.FromSeconds(30);

		///<param name="dsmLoader">This process's DSM (NativeTwainDsm.Load), or a test's.</param>
		///<param name="funcReadMetadataKeys">The capture metadata parser (ImagingCapture's CaptureMetadataParser.ReadKeys, compiled into the
		///helper), used for a session whose program side asked for it.</param>
		public TwainHelperServer(DsmLoader dsmLoader,Func<string,CaptureMetadataKeys> funcReadMetadataKeys,bool isTestFolderAllowed,
			Func<DateTime> funcNow=null)
		{
			_dsmLoader=dsmLoader;
			_funcReadMetadataKeys=funcReadMetadataKeys;
			_isTestFolderAllowed=isTestFolderAllowed;
			_funcNow=funcNow??(() => DateTime.Now);
		}

		///<summary>The sessions this helper has open, for tests.</summary>
		public int CountSessions {
			get {
				lock(_dictSessions) {
					return _dictSessions.Count;
				}
			}
		}

		///<summary>Runs one command. Throws for a failed or unknown command (the program gets the message, Helper32Failure.Remote).</summary>
		public string[] Handle(string command,string[] args,byte[] data,out byte[] dataReply) {
			dataReply=null;
			args=args??new string[0];
			switch(command) {
				case TwainHelperCommands.Hello:
					return new[] { TwainHelperCommands.VERSION.ToString(CultureInfo.InvariantCulture),Environment.Is64BitProcess ? "64" : "32",
						GetDsmLibraryOrEmpty(),System.Runtime.InteropServices.Marshal.SizeOf(typeof(TW_USERINTERFACE)).ToString(CultureInfo.InvariantCulture),
						System.Runtime.InteropServices.Marshal.SizeOf(typeof(TW_IMAGEMEMXFER)).ToString(CultureInfo.InvariantCulture) };
				case TwainHelperCommands.ListSources:
					return ListSources(out dataReply);
				case TwainHelperCommands.Open:
					return Open(args,data,out dataReply);
			}
			ServerSession session=GetSession(args);
			switch(command) {
				case TwainHelperCommands.Enable:
					return session.Enable(args,data);
				case TwainHelperCommands.Disable:
					session.Capture.Disable(ParseEnum<TwainCaptureStopReason>(Arg(args,1)));
					return new string[0];
				case TwainHelperCommands.BeginClose:
					session.Capture.BeginClose();
					return new string[0];
				case TwainHelperCommands.SetQuarantineContext:
					session.Capture.QuarantineContext=new TwainCaptureQuarantineContext(ParseLong(Arg(args,1)),Arg(args,2));
					return new string[0];
				case TwainHelperCommands.Poll:
					return session.Poll(ParseInt(Arg(args,1)),out dataReply);
				case TwainHelperCommands.ReadData:
					dataReply=session.ReadData(Arg(args,1),ParseLong(Arg(args,2)),ParseInt(Arg(args,3)));
					return new string[0];
				case TwainHelperCommands.Release:
					Release(session);
					return new string[0];
				default:
					throw new InvalidOperationException("The TWAIN module has no command "+command+".");
			}
		}

		///<summary>Closes every session (the helper is ending).</summary>
		public void Dispose() {
			List<ServerSession> listSessions;
			lock(_dictSessions) {
				listSessions=_dictSessions.Values.ToList();
				_dictSessions.Clear();
			}
			foreach(ServerSession session in listSessions) {
				session.Capture.BeginClose();
			}
			foreach(ServerSession session in listSessions) {
				session.Capture.WaitForClose(TimeSpan.FromSeconds(5));
			}
		}

		#region Commands
		private string GetDsmLibraryOrEmpty() {
			foreach(string path in NativeTwainDsm.GetCandidatePaths()) {
				if(File.Exists(path)) {
					return path;
				}
			}
			return "";
		}

		///<summary>Before ListSources enumerates the DSM (MSG_OPENDSM loads every installed driver and asks each its identity; the TWAIN DSM
		///has no lock): waits while a session of this helper is closing, so that its MSG_DISABLEDS and MSG_CLOSEDS come first, as an in-process
		///session waits for another (TwainCaptureSession.WaitForOtherSessions); false with errorText when one is open and not closing (its worker
		///may call the DSM at any time) or did not finish closing within ListWaitTimeout. Called on the helper's main thread, which also runs Open,
		///so no session is opening meanwhile; a timed-out Open has asked its session to close.</summary>
		private bool WaitForSessionsBeforeListing(out string errorText) {
			errorText="";
			Stopwatch stopwatch=Stopwatch.StartNew();
			while(true) {
				TwainCaptureSession captureClosing=null;
				lock(_listCaptures) {
					_listCaptures.RemoveAll(x => x.WaitForClose(TimeSpan.Zero));
					foreach(TwainCaptureSession capture in _listCaptures) {
						if(!capture.IsCloseRequested) {
							errorText="The 32-bit TWAIN sources are not listed while a capture session of the 32-bit helper has the TWAIN source \""
								+capture.ProductName+"\" open. Close that capture window first.";
							return false;
						}
						captureClosing=capture;
					}
				}
				if(captureClosing==null) {
					return true;
				}
				TimeSpan timeLeft=ListWaitTimeout-stopwatch.Elapsed;
				if(timeLeft<=TimeSpan.Zero || !captureClosing.WaitForClose(timeLeft)) {
					errorText="The 32-bit TWAIN sources are not listed: the previous capture session of the TWAIN source \""+captureClosing.ProductName
						+"\" in the 32-bit helper is still closing. Try again in a moment.";
					return false;
				}
			}
		}

		private string[] ListSources(out byte[] dataReply) {
			string errorText="";
			ITwainDsm dsm=null;
			string errorBusy;
			if(!WaitForSessionsBeforeListing(out errorBusy)) {
				dataReply=TwainHelperWire.Write(w => TwainHelperWire.WriteSources(w,new List<TwainSourceEntry>()));
				return new[] { "",errorBusy };
			}
			try {
				dsm=_dsmLoader(out errorText);
			}
			catch(Exception ex) {
				errorText=ex.Message;
			}
			List<TwainSourceEntry> listSources=new List<TwainSourceEntry>();
			if(dsm==null) {
				dataReply=TwainHelperWire.Write(w => TwainHelperWire.WriteSources(w,listSources));
				return new[] { "",errorText??"" };
			}
			string library=dsm.LibraryName??"";
			ITwainDsm dsmLoaded=dsm;
			using(TwainSession session=new TwainSession((out string error) => {
				error="";
				return dsmLoaded;
			}))
			{
				List<TW_IDENTITY> listIdentities=session.GetSources();
				if(listIdentities==null) {
					errorText=session.ErrorText;
				}
				else {
					foreach(TW_IDENTITY identity in listIdentities) {
						string productName=(identity.ProductName??"").Trim();
						if(productName=="" || listSources.Any(x => string.Equals(x.ProductName,productName,StringComparison.OrdinalIgnoreCase))) {
							continue;
						}
						listSources.Add(new TwainSourceEntry {
							ProductName=productName,
							Manufacturer=identity.Manufacturer??"",
							ProductFamily=identity.ProductFamily??"",
							VersionMajor=identity.Version.MajorNum,
							VersionMinor=identity.Version.MinorNum,
							VersionInfo=identity.Version.Info??"",
							Dsm=TwainDsmKind.Helper32,
							DsmLibrary=library,
						});
					}
				}
			}
			dataReply=TwainHelperWire.Write(w => TwainHelperWire.WriteSources(w,listSources));
			return new[] { library,errorText??"" };
		}

		private string[] Open(string[] args,byte[] data,out byte[] dataReply) {
			string sessionId=Arg(args,0);
			if(sessionId=="") {
				throw new ArgumentException("Open needs a session id.");
			}
			lock(_dictSessions) {
				if(_dictSessions.ContainsKey(sessionId)) {
					throw new InvalidOperationException("The TWAIN session "+sessionId+" is already open in this helper.");
				}
			}
			string productName=Arg(args,1);
			Guid sessionGuid;
			if(!Guid.TryParseExact(Arg(args,2),"D",out sessionGuid)) {
				sessionGuid=Guid.NewGuid();
			}
			bool isReadingMetadataKeys=Arg(args,3)=="1";
			string folderTest=Arg(args,4);
			long patNum=Arg(args,5)=="" ? 0 : ParseLong(Arg(args,5));
			string patientName=Arg(args,6);
			TwainCaptureOptions options=TwainHelperWire.Read(data,TwainHelperWire.ReadOptions);
			CaptureFolders folders=_isTestFolderAllowed && folderTest!="" ? CaptureFolders.CreateForFolder(folderTest) : CaptureFolders.CreateDefault();
			TwainCaptureSession capture=new TwainCaptureSession(_dsmLoader,folders,_funcNow);
			capture.SessionGuid=sessionGuid;
			capture.QuarantineContext=new TwainCaptureQuarantineContext(patNum,patientName);
			if(isReadingMetadataKeys) {
				capture.FuncReadMetadataKeys=_funcReadMetadataKeys;
			}
			ServerSession session=new ServerSession(sessionId,capture);
			lock(_dictSessions) {
				_dictSessions[sessionId]=session;
			}
			lock(_listCaptures) {
				_listCaptures.Add(capture);
			}
			TwainCaptureOpenResult result=capture.Open(productName,options);
			if(result==null || !result.IsOpen) {
				//The worker has ended after a failed open (or closes when a timed-out open finishes): the program polls nothing for it.
				capture.BeginClose();
				capture.WaitForClose(TimeSpan.FromSeconds(10));
				lock(_dictSessions) {
					_dictSessions.Remove(sessionId);
				}
			}
			TwainCaptureOpenResult resultSent=result??new TwainCaptureOpenResult { Error=TwainCaptureOpenError.Other,ErrorText="The source did not open." };
			dataReply=TwainHelperWire.Write(w => TwainHelperWire.WriteOpenResult(w,resultSent));
			return new[] { resultSent.IsOpen ? "1" : "0" };
		}

		private ServerSession GetSession(string[] args) {
			string sessionId=Arg(args,0);
			lock(_dictSessions) {
				ServerSession session;
				if(_dictSessions.TryGetValue(sessionId,out session)) {
					return session;
				}
			}
			throw new InvalidOperationException("This 32-bit helper has no TWAIN session "+sessionId+" (it was started again, or the session ended).");
		}

		private void Release(ServerSession session) {
			session.Capture.BeginClose();
			session.Capture.WaitForClose(TimeSpan.FromSeconds(10));
			lock(_dictSessions) {
				_dictSessions.Remove(session.Id);
			}
			session.ClearPayloads();
		}
		#endregion Commands

		#region Arguments
		private static string Arg(string[] args,int index) {
			return index<args.Length ? (args[index]??"") : "";
		}

		private static int ParseInt(string text) {
			int value;
			if(!int.TryParse(text,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out value)) {
				throw new ArgumentException("Not a number: "+text);
			}
			return value;
		}

		private static long ParseLong(string text) {
			long value;
			if(!long.TryParse(text,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out value)) {
				throw new ArgumentException("Not a number: "+text);
			}
			return value;
		}

		private static T ParseEnum<T>(string text) where T:struct {
			T value;
			if(!Enum.TryParse(text,false,out value) || !Enum.IsDefined(typeof(T),value)) {
				throw new ArgumentException("Not a "+typeof(T).Name+": "+text);
			}
			return value;
		}
		#endregion Arguments

		///<summary>One TwainCaptureSession of the helper and what it raised that the program has not polled yet.</summary>
		private sealed class ServerSession {
			public readonly string Id;
			public readonly TwainCaptureSession Capture;
			private readonly object _lock=new object();
			private readonly List<TwainHelperEvent> _listEvents=new List<TwainHelperEvent>();
			private readonly AutoResetEvent _eventQueued=new AutoResetEvent(false);
			///<summary>The session's enable id to the program's, and back.</summary>
			private readonly Dictionary<int,int> _dictToProgram=new Dictionary<int,int>();
			private readonly Dictionary<int,int> _dictToSession=new Dictionary<int,int>();
			private readonly Dictionary<string,byte[]> _dictPayloads=new Dictionary<string,byte[]>();

			public ServerSession(string id,TwainCaptureSession capture) {
				Id=id;
				Capture=capture;
				capture.StatusChanged+=status => {
					TwainCaptureStatus statusSent=new TwainCaptureStatus { State=status.State,EnableId=ToProgram(status.EnableId),Text=status.Text,
						IsSafeDisableInEffect=status.IsSafeDisableInEffect,LayoutUnusableText=status.LayoutUnusableText,DeviceEvent=status.DeviceEvent };
					Queue(new TwainHelperEvent { Kind=TwainHelperEventKind.Status,Status=statusSent });
				};
				capture.Ended+=ended => {
					ended.EnableId=ToProgram(ended.EnableId);
					Queue(new TwainHelperEvent { Kind=TwainHelperEventKind.Ended,Ended=ended });
				};
				capture.ImageAcquired+=image => QueueImage(TwainHelperEventKind.ImageAcquired,image);
				capture.ImageQuarantined+=image => QueueImage(TwainHelperEventKind.ImageQuarantined,image);
				capture.Logged+=line => Queue(new TwainHelperEvent { Kind=TwainHelperEventKind.Logged,Line=line });
			}

			private int ToProgram(int enableIdSession) {
				lock(_lock) {
					int enableIdProgram;
					return _dictToProgram.TryGetValue(enableIdSession,out enableIdProgram) ? enableIdProgram : enableIdSession;
				}
			}

			private int ToSession(int enableIdProgram) {
				lock(_lock) {
					int enableIdSession;
					return _dictToSession.TryGetValue(enableIdProgram,out enableIdSession) ? enableIdSession : enableIdProgram;
				}
			}

			private void Queue(TwainHelperEvent helperEvent) {
				lock(_lock) {
					_listEvents.Add(helperEvent);
				}
				_eventQueued.Set();
			}

			///<summary>The image's pixels stay here until ReadData takes them; the event carries the rest. The session window owns the image from
			///now on, as in the program's process: nothing here keeps it.</summary>
			private void QueueImage(TwainHelperEventKind kind,AcquiredImage image) {
				TwainHelperEvent helperEvent=new TwainHelperEvent { Kind=kind };
				byte[] arrayPixels=null;
				if(image.Gray16!=null) {
					arrayPixels=TwainHelperWire.Gray16ToBytes(image.Gray16);
					helperEvent.IsPayloadGray16=true;
				}
				else if(image.Dib!=null) {
					arrayPixels=image.Dib;
				}
				AcquiredImage imageSent=CopyWithoutPixels(image);
				imageSent.EnableId=ToProgram(image.EnableId);
				helperEvent.Image=imageSent;
				lock(_lock) {
					if(arrayPixels!=null) {
						helperEvent.PayloadId=Guid.NewGuid().ToString("N");
						helperEvent.PayloadLength=arrayPixels.Length;
						_dictPayloads[helperEvent.PayloadId]=arrayPixels;
					}
					_listEvents.Add(helperEvent);
				}
				_eventQueued.Set();
			}

			private static AcquiredImage CopyWithoutPixels(AcquiredImage image) {
				byte[] data=TwainHelperWire.Write(w => TwainHelperWire.WriteImage(w,image,isPixelsInline:false));
				return TwainHelperWire.Read(data,TwainHelperWire.ReadImage);
			}

			public string[] Enable(string[] args,byte[] data) {
				int enableIdProgram=ParseInt(Arg(args,1));
				bool isRedelivery=Arg(args,2)=="1";
				TwainEnableRequest request=TwainHelperWire.Read(data,TwainHelperWire.ReadRequest);
				int enableIdSession;
				lock(_lock) {//Held while the session posts the enable, so that its first event is mapped to the program's id.
					enableIdSession=isRedelivery ? Capture.RequestRedelivery(request) : Capture.Enable(request);
					if(enableIdSession!=0) {
						_dictToProgram[enableIdSession]=enableIdProgram;
						_dictToSession[enableIdProgram]=enableIdSession;
					}
				}
				return new[] { enableIdSession.ToString(CultureInfo.InvariantCulture) };
			}

			public string[] Poll(int waitMs,out byte[] dataReply) {
				int wait=Math.Max(0,Math.Min(waitMs,TwainHelperCommands.POLL_WAIT_MAX_MS));
				bool isEmpty;
				lock(_lock) {
					isEmpty=_listEvents.Count==0;
				}
				if(isEmpty && wait>0) {
					_eventQueued.WaitOne(wait);
				}
				//Closed first, then the events: the worker raises its last status before it marks the session closed, so once closed every event
				//is in the queue.
				bool isClosed=Capture.WaitForClose(TimeSpan.Zero);
				List<TwainHelperEvent> listEvents;
				lock(_lock) {
					listEvents=new List<TwainHelperEvent>(_listEvents);
					_listEvents.Clear();
				}
				dataReply=TwainHelperWire.Write(w => TwainHelperWire.WriteEvents(w,listEvents));
				return new[] { isClosed ? "1" : "0" };
			}

			public byte[] ReadData(string payloadId,long offset,int count) {
				lock(_lock) {
					byte[] arrayPixels;
					if(!_dictPayloads.TryGetValue(payloadId,out arrayPixels)) {
						throw new InvalidOperationException("No image data "+payloadId+" in this helper.");
					}
					if(offset<0 || offset>arrayPixels.Length || count<0 || count>TwainHelperCommands.DATA_CHUNK_MAX) {
						throw new ArgumentOutOfRangeException(nameof(offset),"No bytes "+offset+" to "+(offset+count)+" of "+arrayPixels.Length+".");
					}
					int length=(int)Math.Min(count,arrayPixels.Length-offset);
					byte[] arrayChunk=new byte[length];
					Buffer.BlockCopy(arrayPixels,(int)offset,arrayChunk,0,length);
					if(offset+length>=arrayPixels.Length) {
						_dictPayloads.Remove(payloadId);
					}
					return arrayChunk;
				}
			}

			public void ClearPayloads() {
				lock(_lock) {
					_dictPayloads.Clear();
					_listEvents.Clear();
				}
			}
		}
	}
}
