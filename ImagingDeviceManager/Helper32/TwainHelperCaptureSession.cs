// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using ImagingDeviceManager.Capture;
using ImagingDeviceManager.Journal;

namespace ImagingDeviceManager.Helper32 {
	///<summary>A capture session whose source runs in the 32-bit helper (S3b-8; docs/port-notes/S3b-8.md, decision 23): for a TWAIN source that
	///only the 32-bit DSM lists. The helper runs a whole TwainCaptureSession of this library on its 32-bit DSM (TwainHelperServer): the source,
	///its UI, the negotiation (bit depth, memory transfer, DAT_EXTIMAGEINFO), the safe stops and the journal copy of each image are the same code
	///as in the program's process. This class is ITwainCaptureSession on the program's side, so the session window, Test capture and the stored
	///record do not care which path ran:
	///- Open sends the open and waits for it (as TwainCaptureSession.Open does), then a pump thread of this class polls the helper's session and
	///  raises what it raised, in order, with the program's enable ids and Tags, and each image with its pixels (read in pieces, ReadData).
	///- Enable, RequestRedelivery, Disable, Cancel, BeginClose and QuarantineContext only post a command to the pump and return at once; it sends
	///  them in order. Enable returns the program's enable id at once.
	///- BeginClose: no ImageAcquired is raised once it has returned (as TwainCaptureSession); an image the helper raised before it learnt of the
	///  close is quarantined here and raised as ImageQuarantined. QuarantineImage and ConfirmRedeliveredJournalCopy run in the program's process,
	///  on a twin TwainCaptureSession that is never opened (SetTwinOf), with the same folders and the same record, so they work after the helper
	///  is gone too.
	///- The helper's failures end the session cleanly, never a hang: a helper that is missing, does not start, crashes, stops answering (every
	///  call has a timeout, after which the helper client ends it) or was replaced ends every running enable with Ended Error (SourceMayHoldImages,
	///  since images the helper held were not received; their journal copies, written before they were raised, stay for recovery), raises
	///  StatusChanged Closed and marks the session closed, with a clear message in ErrorText and in the capture log.
	///Events are raised on the pump thread, which is not the caller's; handlers must return quickly and may call every member but Open.</summary>
	public sealed class TwainHelperCaptureSession:ITwainCaptureSession {
		#region Fields
		private const string PREFIX_MESSAGE="The 32-bit TWAIN helper";
		private readonly ITwainHelperChannel _channel;
		private readonly CaptureFolders _folders;
		///<summary>Tests: the capture folder the helper's session writes to ("" for the default).</summary>
		private readonly string _folderImagingTest;
		private readonly object _lock=new object();
		///<summary>As TwainCaptureSession's: held while an ImageAcquired is decided and raised, and by BeginClose while it sets _isClosing.</summary>
		private readonly object _lockRaiseImage=new object();
		private readonly LinkedList<PumpCommand> _listCommands=new LinkedList<PumpCommand>();
		private readonly AutoResetEvent _eventCommand=new AutoResetEvent(false);
		private readonly ManualResetEvent _eventClosed=new ManualResetEvent(true);
		///<summary>Every enable posted, by the program's enable id, until its Ended was raised.</summary>
		private readonly Dictionary<int,PostedEnable> _dictEnables=new Dictionary<int,PostedEnable>();
		private readonly string _sessionId=Guid.NewGuid().ToString("N");
		private TwainCaptureSession _twin;
		private Thread _thread;
		private volatile bool _isClosing;
		private volatile int _state=(int)TwainCaptureState.Closed;
		private int _enableIdLast;
		private int _processIdOpen;
		private TwainCaptureOptions _options=new TwainCaptureOptions();
		private TwainCaptureQuarantineContext _quarantineContext=new TwainCaptureQuarantineContext();
		private volatile string _productName="";
		private volatile TwainCaptureSourceInfo _sourceInfo;
		private Func<string,CaptureMetadataKeys> _funcReadMetadataKeys;
		#endregion Fields

		///<summary>A session through channel, with the capture folders under %ProgramData%\OpenPMS-Dental\Imaging (the helper uses the same).
		///</summary>
		public TwainHelperCaptureSession(ITwainHelperChannel channel):this(channel,CaptureFolders.CreateDefault(),"") {
		}

		///<summary>For tests: other capture folders, which the helper uses too when it runs a test DSM.</summary>
		internal TwainHelperCaptureSession(ITwainHelperChannel channel,CaptureFolders folders,string folderImagingTest) {
			_channel=channel;
			_folders=folders;
			_folderImagingTest=folderImagingTest??"";
			SessionGuid=Guid.NewGuid();
		}

		#region Timeouts
		///<summary>How long one Poll, Enable, Disable or ReadData may take before the helper is ended. Tests shorten it.</summary>
		internal TimeSpan CallTimeout { get; set; }=TimeSpan.FromSeconds(30);
		///<summary>Added to TwainCaptureOptions.OpenTimeout for the open: the helper's start (Helper32Client allows 30 s) and the DSM's load.</summary>
		internal TimeSpan OpenTimeoutExtra { get; set; }=TimeSpan.FromSeconds(45);
		///<summary>How long one Poll waits in the helper for an event.</summary>
		internal int PollWaitMs { get; set; }=100;
		#endregion Timeouts

		#region Properties and events
		public TwainCaptureState State => (TwainCaptureState)_state;
		public Guid SessionGuid { get; set; }
		public string ProductName => _productName;
		public TwainCaptureSourceInfo SourceInfo => _sourceInfo;

		public TwainCaptureOptions Options {
			get {
				lock(_lock) {
					return _options;
				}
			}
		}

		public TwainCaptureQuarantineContext QuarantineContext {
			get {
				lock(_lock) {
					return _quarantineContext;
				}
			}
			set {
				TwainCaptureQuarantineContext context=value??new TwainCaptureQuarantineContext();
				lock(_lock) {
					_quarantineContext=context;
					if(_twin!=null) {
						_twin.QuarantineContext=context;
					}
					if(IsPumpRunning()) {
						_listCommands.AddLast(new PumpCommand { Command=TwainHelperCommands.SetQuarantineContext,
							Args=new[] { context.PatNum.ToString(CultureInfo.InvariantCulture),context.PatientName } });
						_eventCommand.Set();
					}
				}
			}
		}

		///<summary>Only whether it is set crosses: the helper reads the keys with its own copy of the capture metadata parser (ImagingCapture's
		///CaptureMetadataParser, compiled into the helper), the one the program sets here. The twin (quarantine, confirm) uses this function.</summary>
		public Func<string,CaptureMetadataKeys> FuncReadMetadataKeys {
			get {
				return _funcReadMetadataKeys;
			}
			set {
				_funcReadMetadataKeys=value;
				lock(_lock) {
					if(_twin!=null) {
						_twin.FuncReadMetadataKeys=value;
					}
				}
			}
		}

		public event Action<AcquiredImage> ImageAcquired;
		public event Action<TwainCaptureEnded> Ended;
		public event Action<TwainCaptureStatus> StatusChanged;
		public event Action<AcquiredImage> ImageQuarantined;
		public event Action<string> Logged;

		private bool IsPumpThread => _thread!=null && Thread.CurrentThread==_thread;
		#endregion Properties and events

		#region Commands (any thread)
		private sealed class PumpCommand {
			public string Command;
			public string[] Args;
			public byte[] Data;
			///<summary>Enable: the program's id, and the request (with its Tag).</summary>
			public PostedEnable Enable;
		}

		private sealed class PostedEnable {
			public int EnableId;
			public bool IsRedelivery;
			public TwainEnableRequest Request;
		}

		public TwainCaptureOpenResult Open(string productName,TwainCaptureOptions options) {
			if(IsPumpThread) {
				throw new InvalidOperationException("TwainHelperCaptureSession.Open cannot be called from one of its event handlers.");
			}
			TwainCaptureOptions optionsOpen=options==null ? new TwainCaptureOptions() : options.Clone();
			lock(_lock) {
				if(State!=TwainCaptureState.Closed || IsPumpRunning()) {
					return new TwainCaptureOpenResult { Error=TwainCaptureOpenError.AlreadyOpen,ErrorText="The capture session is already open." };
				}
				_options=optionsOpen;
				_isClosing=false;
				_sourceInfo=null;
				_productName="";
				_listCommands.Clear();
				_dictEnables.Clear();
				_state=(int)TwainCaptureState.Opening;
				_eventClosed.Reset();
				_twin=new TwainCaptureSession(null,_folders,null);
				_twin.SessionGuid=SessionGuid;
				_twin.QuarantineContext=_quarantineContext;
				_twin.FuncReadMetadataKeys=_funcReadMetadataKeys;
			}
			string productNameOpen=(productName??"").Trim();
			WriteLog("Opening \""+productNameOpen+"\" through the 32-bit helper (a source that only the 32-bit TWAIN data source manager lists).");
			TwainCaptureOpenResult result;
			try {
				_processIdOpen=0;
				byte[] dataReply;
				TimeSpan timeout=(optionsOpen.OpenTimeout>TimeSpan.Zero ? optionsOpen.OpenTimeout : TimeSpan.FromSeconds(60))+OpenTimeoutExtra;
				_channel.Call(TwainHelperCommands.Open,new[] { _sessionId,productNameOpen,SessionGuid.ToString("D"),_funcReadMetadataKeys!=null ? "1" : "0",
					_folderImagingTest },TwainHelperWire.Write(w => TwainHelperWire.WriteOptions(w,optionsOpen)),out dataReply,timeout);
				_processIdOpen=_channel.ProcessId;
				result=TwainHelperWire.Read(dataReply,TwainHelperWire.ReadOpenResult);
			}
			catch(TwainHelperException ex) {
				result=new TwainCaptureOpenResult { Error=TwainCaptureOpenError.Other,ErrorText=GetFailureText(ex,"opening the TWAIN source \""
					+productNameOpen+"\"") };
			}
			catch(InvalidDataException ex) {
				result=new TwainCaptureOpenResult { Error=TwainCaptureOpenError.Other,ErrorText=PREFIX_MESSAGE+"'s answer to the open could not be read: "
					+ex.Message };
			}
			if(!result.IsOpen) {
				WriteLog("The source could not be opened through the 32-bit helper: "+(result.ErrorText??result.Error.ToString()));
				lock(_lock) {
					_state=(int)TwainCaptureState.Closed;
					_eventClosed.Set();
				}
				return result;
			}
			if(result.SourceInfo!=null) {
				result.SourceInfo.DsmName=(result.SourceInfo.DsmName??"")+" (32-bit, in the helper "+"OpenPmsDentalHelper32"+")";
			}
			lock(_lock) {
				_sourceInfo=result.SourceInfo;
				_productName=result.SourceInfo?.ProductName??productNameOpen;
				_twin.SetTwinOf(_productName,optionsOpen,result.SourceInfo);
				if(State==TwainCaptureState.Opening) {
					_state=(int)TwainCaptureState.Open;
				}
				_thread=new Thread(PumpMain) { IsBackground=true,Name="TWAIN helper "+SessionGuid.ToString("N").Substring(0,8) };
			}
			_thread.Start();
			return result;
		}

		public int Enable(TwainEnableRequest request) {
			return PostEnable(request,false);
		}

		public int RequestRedelivery(TwainEnableRequest request) {
			return PostEnable(request,true);
		}

		private int PostEnable(TwainEnableRequest request,bool isRedelivery) {
			TwainEnableRequest requestPosted=request??new TwainEnableRequest();
			int enableId;
			lock(_lock) {
				if(!IsPumpRunning() || _isClosing) {
					return 0;
				}
				enableId=++_enableIdLast;
				PostedEnable posted=new PostedEnable { EnableId=enableId,IsRedelivery=isRedelivery,Request=requestPosted };
				_dictEnables[enableId]=posted;
				_listCommands.AddLast(new PumpCommand { Command=TwainHelperCommands.Enable,
					Args=new[] { enableId.ToString(CultureInfo.InvariantCulture),isRedelivery ? "1" : "0" },
					Data=TwainHelperWire.Write(w => TwainHelperWire.WriteRequest(w,requestPosted)),Enable=posted });
				_eventCommand.Set();
			}
			//The source's own window opens at the enable, in the helper's process: let it come to the foreground, as it could in the program's.
			_channel.AllowSetForeground();
			return enableId;
		}

		public void Disable(TwainCaptureStopReason reason) {
			TwainCaptureStopReason reasonSent=reason==TwainCaptureStopReason.None ? TwainCaptureStopReason.Other : reason;
			PostCommand(TwainHelperCommands.Disable,reasonSent.ToString());
		}

		public void Cancel() {
			Disable(TwainCaptureStopReason.Cancel);
		}

		public void BeginClose() {
			lock(_lockRaiseImage) {//Waits for an ImageAcquired that is being raised; none is raised after this.
				lock(_lock) {
					if(!IsPumpRunning()) {
						return;
					}
					if(!_isClosing) {
						_isClosing=true;
						_listCommands.AddLast(new PumpCommand { Command=TwainHelperCommands.BeginClose,Args=new string[0] });
					}
					_eventCommand.Set();
				}
			}
		}

		public bool WaitForClose(TimeSpan timeout) {
			return _eventClosed.WaitOne(timeout);
		}

		public void Dispose() {
			BeginClose();
		}

		private void PostCommand(string command,params string[] args) {
			lock(_lock) {
				if(!IsPumpRunning()) {
					return;
				}
				_listCommands.AddLast(new PumpCommand { Command=command,Args=args });
				_eventCommand.Set();
			}
		}

		private bool IsPumpRunning() {
			return _thread!=null && _thread.IsAlive && !_eventClosed.WaitOne(0);
		}
		#endregion Commands (any thread)

		#region Pump
		private void PumpMain() {
			try {
				while(true) {
					PumpCommand command;
					while((command=TakeCommand())!=null) {
						Send(command);
					}
					bool hasCommands;
					lock(_lock) {
						hasCommands=_listCommands.Count>0;
					}
					byte[] dataReply;
					string[] arrayResults=Call(TwainHelperCommands.Poll,new[] { hasCommands ? "0" : PollWaitMs.ToString(CultureInfo.InvariantCulture) },
						null,out dataReply);
					List<TwainHelperEvent> listEvents=TwainHelperWire.Read(dataReply,TwainHelperWire.ReadEvents);
					foreach(TwainHelperEvent helperEvent in listEvents) {
						Dispatch(helperEvent);
					}
					if(arrayResults.Length>0 && arrayResults[0]=="1") {
						Finish(null);
						return;
					}
				}
			}
			catch(TwainHelperException ex) {
				Finish(ex);
			}
			catch(Exception ex) {
				Finish(new TwainHelperException(TwainHelperFailure.Protocol,PREFIX_MESSAGE+"'s session failed: "+ex.Message,ex));
			}
		}

		private PumpCommand TakeCommand() {
			lock(_lock) {
				if(_listCommands.Count==0) {
					return null;
				}
				PumpCommand command=_listCommands.First.Value;
				_listCommands.RemoveFirst();
				return command;
			}
		}

		private void Send(PumpCommand command) {
			List<string> listArgs=new List<string>(command.Args??new string[0]);
			byte[] dataReply;
			string[] arrayResults=Call(command.Command,listArgs.ToArray(),command.Data,out dataReply);
			if(command.Enable!=null && (arrayResults.Length==0 || arrayResults[0]=="0")) {
				//The helper's session refused it (it was closing): as TwainCaptureSession ends a refused enable, so the window is not left waiting.
				RaiseEnded(new TwainCaptureEnded { EnableId=command.Enable.EnableId,Tag=command.Enable.Request.Tag,IsRedelivery=command.Enable.IsRedelivery,
					Reason=TwainCaptureEndReason.Refused,BitsRequested=TwainBitDepth.Normalize(command.Enable.Request.Bits),
					ErrorText="The capture session is closed.",EndedAt=DateTime.Now });
			}
		}

		///<summary>A call of this session's: the session id first. Throws TwainHelperException, also when another helper answers than the one the
		///session was opened in (Replaced).</summary>
		private string[] Call(string command,string[] args,byte[] data,out byte[] dataReply) {
			string[] arrayArgs=new string[(args??new string[0]).Length+1];
			arrayArgs[0]=_sessionId;
			Array.Copy(args??new string[0],0,arrayArgs,1,arrayArgs.Length-1);
			string[] arrayResults=_channel.Call(command,arrayArgs,data,out dataReply,CallTimeout);
			int processId=_channel.ProcessId;
			if(_processIdOpen!=0 && processId!=_processIdOpen) {
				throw new TwainHelperException(TwainHelperFailure.Replaced,PREFIX_MESSAGE+" that had the source open (process "+_processIdOpen
					+") is gone; another one answered.");
			}
			return arrayResults??new string[0];
		}

		private void Dispatch(TwainHelperEvent helperEvent) {
			switch(helperEvent.Kind) {
				case TwainHelperEventKind.Status:
					_state=(int)helperEvent.Status.State;
					Raise(StatusChanged,helperEvent.Status,"StatusChanged");
					return;
				case TwainHelperEventKind.Ended:
					PostedEnable posted;
					lock(_lock) {
						_dictEnables.TryGetValue(helperEvent.Ended.EnableId,out posted);
					}
					helperEvent.Ended.Tag=posted?.Request.Tag;
					RaiseEnded(helperEvent.Ended);
					return;
				case TwainHelperEventKind.ImageAcquired:
					DeliverImage(helperEvent);
					return;
				case TwainHelperEventKind.ImageQuarantined:
					ReadPixels(helperEvent);
					helperEvent.Image.Tag=GetTag(helperEvent.Image.EnableId);
					Raise(ImageQuarantined,helperEvent.Image,"ImageQuarantined");
					return;
				case TwainHelperEventKind.Logged:
					Raise(Logged,helperEvent.Line,"Logged");
					return;
			}
		}

		private object GetTag(int enableId) {
			lock(_lock) {
				PostedEnable posted;
				return _dictEnables.TryGetValue(enableId,out posted) ? posted.Request.Tag : null;
			}
		}

		///<summary>An image the helper's session raised: with its pixels and Tag, raised as ImageAcquired unless BeginClose has begun; then it goes
		///to the quarantine (and its journal copy's sidecar says so), as TwainCaptureSession does with an image whose journal copy was being written
		///when BeginClose was called.</summary>
		private void DeliverImage(TwainHelperEvent helperEvent) {
			AcquiredImage image=helperEvent.Image;
			ReadPixels(helperEvent);
			image.Tag=GetTag(image.EnableId);
			lock(_lockRaiseImage) {
				if(!_isClosing) {
					Raise(ImageAcquired,image,"ImageAcquired");
					return;
				}
			}
			string error;
			string path=QuarantineImage(image,"It arrived after the capture window began to close."
				+(image.IsRedelivery ? " It is a redelivered image that the operator did not confirm, so its patient is unknown." : ""),image.IsRedelivery,
				out error);
			WriteLog("Capture event: an image of enable "+image.EnableId+" arrived while the session closed; "+(path==null
				? "it could not be quarantined ("+error+")"+(image.JournalPath==null ? "" : "; its journal copy is the only one")
				: "quarantined as "+Path.GetFileName(path))+".");
			Raise(ImageQuarantined,image,"ImageQuarantined");
		}

		///<summary>Reads the image's pixels from the helper in pieces of at most DATA_CHUNK_MAX.</summary>
		private void ReadPixels(TwainHelperEvent helperEvent) {
			if(string.IsNullOrEmpty(helperEvent.PayloadId)) {
				return;
			}
			byte[] arrayPixels=new byte[helperEvent.PayloadLength];
			long offset=0;
			while(offset<arrayPixels.Length) {
				int count=(int)Math.Min(TwainHelperCommands.DATA_CHUNK_MAX,arrayPixels.Length-offset);
				byte[] dataReply;
				Call(TwainHelperCommands.ReadData,new[] { helperEvent.PayloadId,offset.ToString(CultureInfo.InvariantCulture),
					count.ToString(CultureInfo.InvariantCulture) },null,out dataReply);
				if(dataReply==null || dataReply.Length!=count) {
					throw new TwainHelperException(TwainHelperFailure.Protocol,PREFIX_MESSAGE+" sent "+(dataReply==null ? 0 : dataReply.Length)+" bytes of an "
						+"image where "+count+" were asked for.");
				}
				Buffer.BlockCopy(dataReply,0,arrayPixels,(int)offset,count);
				offset+=count;
			}
			if(helperEvent.IsPayloadGray16) {
				helperEvent.Image.Gray16=TwainHelperWire.BytesToGray16(arrayPixels);
			}
			else {
				helperEvent.Image.Dib=arrayPixels;
			}
		}

		///<summary>The session has ended: the helper's session closed (failure null; the helper forgets it), or the helper failed. A failure ends
		///every enable that has no Ended yet with Reason Error, raises StatusChanged Closed and writes the reason to the capture log. Then the session
		///is closed (WaitForClose returns).</summary>
		private void Finish(TwainHelperException failure) {
			if(failure==null) {
				try {
					byte[] dataReply;
					Call(TwainHelperCommands.Release,new string[0],null,out dataReply);
				}
				catch(TwainHelperException ex) {
					WriteLog("Releasing the session in the 32-bit helper failed: "+ex.Message);
				}
			}
			else {
				string text=GetFailureText(failure,"the capture session");
				WriteLog("Capture event: "+text);
				Raise(Logged,"Capture session "+SessionGuid.ToString("N").Substring(0,8)+": "+text,"Logged");
				if(failure.Failure==TwainHelperFailure.Remote || failure.Failure==TwainHelperFailure.Protocol) {
					//The helper still runs: its session is closed and forgotten (the source's safe order is the helper session's own).
					try {
						byte[] dataReply;
						_channel.Call(TwainHelperCommands.Release,new[] { _sessionId },null,out dataReply,CallTimeout);
					}
					catch(TwainHelperException) {
						//Nothing more can be done; the helper ends with the program.
					}
				}
				List<PostedEnable> listOpen;
				lock(_lock) {
					listOpen=new List<PostedEnable>(_dictEnables.Values);
				}
				listOpen.Sort((x,y) => x.EnableId.CompareTo(y.EnableId));
				foreach(PostedEnable posted in listOpen) {
					RaiseEnded(new TwainCaptureEnded {
						EnableId=posted.EnableId,
						Tag=posted.Request.Tag,
						IsRedelivery=posted.IsRedelivery,
						Reason=TwainCaptureEndReason.Error,
						StopReason=TwainCaptureStopReason.Error,
						SourceMayHoldImages=true,
						BitsRequested=TwainBitDepth.Normalize(posted.Request.Bits),
						ErrorText=text,
						EndedAt=DateTime.Now,
					});
				}
				_state=(int)TwainCaptureState.Closed;
				Raise(StatusChanged,new TwainCaptureStatus { State=TwainCaptureState.Closed,EnableId=0,Text=TwainCaptureSession.GetStateText(TwainCaptureState.Closed) },
					"StatusChanged");
			}
			lock(_lock) {
				_isClosing=true;
				_listCommands.Clear();
				_state=(int)TwainCaptureState.Closed;
				_eventClosed.Set();
			}
		}

		///<summary>One sentence for the window and the log: what failed, during what, and what it means for the images.</summary>
		private static string GetFailureText(TwainHelperException failure,string during) {
			string what;
			switch(failure.Failure) {
				case TwainHelperFailure.Missing:
				case TwainHelperFailure.StartFailed:
					what=PREFIX_MESSAGE+" program could not be started for "+during+".";
					break;
				case TwainHelperFailure.TimedOut:
					what=PREFIX_MESSAGE+" program stopped answering during "+during+" and was ended.";
					break;
				case TwainHelperFailure.Crashed:
				case TwainHelperFailure.Replaced:
					what=PREFIX_MESSAGE+" program ended during "+during+".";
					break;
				default:
					what=PREFIX_MESSAGE+" program failed during "+during+".";
					break;
			}
			if(during=="the capture session") {
				what+=" The capture ended. An image the sensor software was sending may not have been received: do not re-expose yet; look for it in "
					+"Unassigned captures.";
			}
			return what+" ("+failure.Message+")";
		}

		private void RaiseEnded(TwainCaptureEnded ended) {
			lock(_lock) {
				_dictEnables.Remove(ended.EnableId);
			}
			Raise(Ended,ended,"Ended");
		}

		private void Raise<T>(Action<T> handler,T value,string name) {
			if(handler==null) {
				return;
			}
			try {
				handler(value);
			}
			catch(Exception ex) {
				WriteLog("A "+name+" handler failed: "+ex.Message);
			}
		}

		private void WriteLog(string text) {
			TwainCaptureSession twin;
			lock(_lock) {
				twin=_twin;
			}
			if(twin==null) {
				twin=new TwainCaptureSession(null,_folders,null) { SessionGuid=SessionGuid };
			}
			twin.WriteCaptureLog(text);
		}
		#endregion Pump

		#region Images (any thread)
		///<summary>On the twin, in the program's process (TwainCaptureSession.QuarantineImage): the same quarantine file and the same rewrite of the
		///journal copy the helper wrote. Works also after the helper is gone.</summary>
		public string QuarantineImage(AcquiredImage image,string reason,bool isPatientUnknown,out string error) {
			TwainCaptureSession twin=GetTwinOrError(out error);
			if(twin==null) {
				return null;
			}
			return twin.QuarantineImage(image,reason,isPatientUnknown,out error);
		}

		///<summary>On the twin, in the program's process (TwainCaptureSession.ConfirmRedeliveredJournalCopy).</summary>
		public bool ConfirmRedeliveredJournalCopy(AcquiredImage image,out string error) {
			TwainCaptureSession twin=GetTwinOrError(out error);
			if(twin==null) {
				return false;
			}
			return twin.ConfirmRedeliveredJournalCopy(image,out error);
		}

		private TwainCaptureSession GetTwinOrError(out string error) {
			error="";
			lock(_lock) {
				if(_twin==null) {
					error="The capture session was never opened.";
				}
				return _twin;
			}
		}
		#endregion Images (any thread)
	}
}
