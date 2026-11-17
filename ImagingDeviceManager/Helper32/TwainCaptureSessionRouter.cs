// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using ImagingDeviceManager.Capture;
using ImagingDeviceManager.Journal;

namespace ImagingDeviceManager.Helper32 {
	///<summary>The capture session the program creates (S3b-8): at Open it opens the stored source natively first, in a TwainCaptureSession of the
	///program's process, and only when the program's DSM does not list it asks the 32-bit helper (TwainSourcePath.ResolveNotNative) and becomes a
	///TwainHelperCaptureSession through it; every member and event is Inner's from then on. Before Open it keeps SessionGuid, QuarantineContext and FuncReadMetadataKeys and gives them to the session it
	///creates. Open takes the stored choice (ImagingDevice.TwainName, with or without the 32-bit marker) and opens its ProductName. One Open per
	///instance, as the session window and Test capture use it.</summary>
	public sealed class TwainCaptureSessionRouter:ITwainCaptureSession {
		private readonly TwainSourcePath _sourcePath;
		private readonly Func<ITwainCaptureSession> _funcCreateNative;
		private readonly Func<ITwainCaptureSession> _funcCreateHelper;
		private readonly CaptureFolders _folders;
		private readonly object _lock=new object();
		private ITwainCaptureSession _session;
		private Guid _sessionGuid=Guid.NewGuid();
		private TwainCaptureQuarantineContext _quarantineContext=new TwainCaptureQuarantineContext();
		private Func<string,CaptureMetadataKeys> _funcReadMetadataKeys;
		///<summary>BeginClose or Dispose was called: Open opens nothing more.</summary>
		private bool _isCloseRequested;

		///<summary>The program's: native sessions on its own DSM, helper sessions through funcChannel's helper.</summary>
		public TwainCaptureSessionRouter(TwainSourcePath sourcePath,Func<ITwainHelperChannel> funcChannel)
			:this(sourcePath,() => new TwainCaptureSession(),() => new TwainHelperCaptureSession(funcChannel()),CaptureFolders.CreateDefault())
		{
		}

		///<summary>For tests: the sessions each path creates, and the capture folders of the choice's log line.</summary>
		internal TwainCaptureSessionRouter(TwainSourcePath sourcePath,Func<ITwainCaptureSession> funcCreateNative,Func<ITwainCaptureSession> funcCreateHelper,
			CaptureFolders folders)
		{
			_sourcePath=sourcePath;
			_funcCreateNative=funcCreateNative;
			_funcCreateHelper=funcCreateHelper;
			_folders=folders;
		}

		///<summary>The session Open created, or null before it.</summary>
		public ITwainCaptureSession Inner {
			get {
				lock(_lock) {
					return _session;
				}
			}
		}

		///<summary>The path Open chose (Native before Open).</summary>
		public TwainSourceResolution Resolution { get; private set; }

		public TwainCaptureState State => Inner?.State??TwainCaptureState.Closed;
		public string ProductName => Inner?.ProductName??"";
		public TwainCaptureOptions Options => Inner?.Options??new TwainCaptureOptions();
		public TwainCaptureSourceInfo SourceInfo => Inner?.SourceInfo;

		public Guid SessionGuid {
			get {
				lock(_lock) {
					return _session?.SessionGuid??_sessionGuid;
				}
			}
			set {
				lock(_lock) {
					_sessionGuid=value;
					if(_session!=null) {
						_session.SessionGuid=value;
					}
				}
			}
		}

		public TwainCaptureQuarantineContext QuarantineContext {
			get {
				lock(_lock) {
					return _session?.QuarantineContext??_quarantineContext;
				}
			}
			set {
				lock(_lock) {
					_quarantineContext=value??new TwainCaptureQuarantineContext();
					if(_session!=null) {
						_session.QuarantineContext=_quarantineContext;
					}
				}
			}
		}

		public Func<string,CaptureMetadataKeys> FuncReadMetadataKeys {
			get {
				lock(_lock) {
					return _funcReadMetadataKeys;
				}
			}
			set {
				lock(_lock) {
					_funcReadMetadataKeys=value;
					if(_session!=null) {
						_session.FuncReadMetadataKeys=value;
					}
				}
			}
		}

		public event Action<AcquiredImage> ImageAcquired;
		public event Action<TwainCaptureEnded> Ended;
		public event Action<TwainCaptureStatus> StatusChanged;
		public event Action<AcquiredImage> ImageQuarantined;
		public event Action<string> Logged;

		///<summary>Native first, deciding by opening and never by enumerating outside a session (S3b-8 section 2.1): an in-process session opens
		///the stored text (exactly, while the 32-bit path is possible: first the whole text, then its ProductName without the marker); that session
		///waits for the other sessions of this process that are closing the source and enumerates the DSM on its own STA worker. Only when the
		///program's DSM is missing or lists no such source does the router ask the 32-bit helper's ListSources (TwainSourcePath.ResolveNotNative);
		///a source it lists opens in a TwainHelperCaptureSession, which becomes Inner. Every other result of the in-process open is the answer.
		///</summary>
		public TwainCaptureOpenResult Open(string productName,TwainCaptureOptions options) {
			lock(_lock) {
				if(_session!=null) {
					return _session.Open(productName,options);//AlreadyOpen, or a second open of a closed session, as that session answers it
				}
			}
			TwainSourceChoice choice=TwainSourceChoice.Parse(productName);
			TwainCaptureOptions optionsUsed=options==null ? new TwainCaptureOptions() : options;
			ITwainCaptureSession sessionNative=Attach(_funcCreateNative());
			if(!_sourcePath.CanUseHelper) {
				//A 32-bit program, or no helper here: the program's own DSM as before S3b-8, by the ProductName.
				Resolution=_sourcePath.ResolveNotNative(choice);
				WriteChoiceLog(choice,Resolution);
				return OpenOrRefuse(sessionNative,choice.ProductName,optionsUsed);
			}
			//Exactly, so that a 32-bit-only "Sensor" is not taken for a 64-bit "Sensor Pro": the earlier listing compared the whole names.
			TwainCaptureOptions optionsExact=optionsUsed.Clone();
			optionsExact.IsSourceNameExact=true;
			List<string> listNames=new List<string> { choice.StoredText };
			if(!string.Equals(choice.ProductName,choice.StoredText,StringComparison.OrdinalIgnoreCase)) {
				listNames.Add(choice.ProductName);
			}
			TwainCaptureOpenResult resultNative=null;
			foreach(string name in listNames) {
				resultNative=OpenOrRefuse(sessionNative,name,optionsExact);
				if(resultNative.IsOpen || !IsNotInNativeDsm(resultNative) || resultNative.Error==TwainCaptureOpenError.NoDsm) {
					break;
				}
			}
			if(resultNative.IsOpen || !IsNotInNativeDsm(resultNative)) {
				//Open, or the program's DSM has the source and it failed there (busy, timed out, refused by the gate, MSG_OPENDS failed): a source
				//the 64-bit DSM lists is never run in 32-bit.
				Resolution=new TwainSourceResolution { Dsm=TwainDsmKind.Native,ProductName=resultNative.IsOpen ? sessionNative.ProductName : choice.ProductName,
					Reason="the 64-bit TWAIN data source manager lists \""+choice.ProductName+"\" (native first)"
					+(resultNative.IsOpen && choice.Dsm==TwainDsmKind.Helper32 ? "; it was chosen from the 32-bit one, and has a 64-bit driver now" : "") };
				WriteChoiceLog(choice,Resolution);
				return resultNative;
			}
			TwainSourceResolution resolution;
			try {
				resolution=_sourcePath.ResolveNotNative(choice);
			}
			catch(Exception ex) {
				resolution=new TwainSourceResolution { ProductName=choice.ProductName,Reason="the choice failed: "+ex.Message };
			}
			Resolution=resolution;
			WriteChoiceLog(choice,resolution);
			if(resolution.ErrorText!=null) {
				WriteCaptureLog("The source cannot be opened: "+resolution.ErrorText);
				//Other, so that the window shows this text (CaptureEndText.GetOpenText) and not only "not found".
				return new TwainCaptureOpenResult { Error=TwainCaptureOpenError.Other,ErrorText=resolution.ErrorText };
			}
			if(resolution.Dsm==TwainDsmKind.Helper32) {
				ITwainCaptureSession sessionHelper=Attach(_funcCreateHelper());
				sessionNative.Dispose();//Never opened: nothing to close.
				return OpenOrRefuse(sessionHelper,resolution.ProductName,optionsUsed);
			}
			if(resultNative.Error==TwainCaptureOpenError.NoDsm) {
				return resultNative;
			}
			//Listed nowhere by that exact name: the in-process session by the ProductName as before S3b-8 (a source whose name starts with it, or
			//not found).
			return OpenOrRefuse(sessionNative,choice.ProductName,optionsUsed);
		}

		///<summary>The program's DSM is missing, or lists no source of that name.</summary>
		private static bool IsNotInNativeDsm(TwainCaptureOpenResult result) {
			return result!=null && !result.IsOpen
				&& (result.Error==TwainCaptureOpenError.NoDsm || (result.Error==TwainCaptureOpenError.SourceNotFound && result.IsSourceNotListed));
		}

		///<summary>session, with what was set before Open and its events forwarded while it is Inner, becomes Inner.</summary>
		private ITwainCaptureSession Attach(ITwainCaptureSession session) {
			lock(_lock) {
				session.SessionGuid=_sessionGuid;
				session.QuarantineContext=_quarantineContext;
				session.FuncReadMetadataKeys=_funcReadMetadataKeys;
				session.ImageAcquired+=x => Forward(session,() => ImageAcquired?.Invoke(x));
				session.Ended+=x => Forward(session,() => Ended?.Invoke(x));
				session.StatusChanged+=x => Forward(session,() => StatusChanged?.Invoke(x));
				session.ImageQuarantined+=x => Forward(session,() => ImageQuarantined?.Invoke(x));
				session.Logged+=x => Forward(session,() => Logged?.Invoke(x));
				_session=session;
			}
			return session;
		}

		private void Forward(ITwainCaptureSession session,Action action) {
			if(Inner==session) {
				action();
			}
		}

		///<summary>session.Open, unless BeginClose or Dispose came first (the window was closed while the router was still choosing).</summary>
		private TwainCaptureOpenResult OpenOrRefuse(ITwainCaptureSession session,string productName,TwainCaptureOptions options) {
			lock(_lock) {
				if(_isCloseRequested) {
					return new TwainCaptureOpenResult { Error=TwainCaptureOpenError.Other,ErrorText="The capture session was closed while it was opening." };
				}
			}
			return session.Open(productName,options);
		}

		private void WriteChoiceLog(TwainSourceChoice choice,TwainSourceResolution resolution) {
			WriteCaptureLog("TWAIN source \""+choice.StoredText+"\": "+(resolution.Dsm==TwainDsmKind.Helper32 ? "the 32-bit helper" : "this process")
				+", because "+resolution.Reason+".");
		}

		private void WriteCaptureLog(string text) {
			if(_folders==null) {
				return;
			}
			try {
				new TwainCaptureSession(null,_folders,null) { SessionGuid=SessionGuid }.WriteCaptureLog(text);
			}
			catch {
				//The log is not worth a failed open.
			}
		}

		public int Enable(TwainEnableRequest request) {
			return Inner?.Enable(request)??0;
		}

		public int RequestRedelivery(TwainEnableRequest request) {
			return Inner?.RequestRedelivery(request)??0;
		}

		public void Disable(TwainCaptureStopReason reason) {
			Inner?.Disable(reason);
		}

		public void Cancel() {
			Inner?.Cancel();
		}

		public void BeginClose() {
			lock(_lock) {
				_isCloseRequested=true;
			}
			Inner?.BeginClose();
		}

		public bool WaitForClose(TimeSpan timeout) {
			ITwainCaptureSession session=Inner;
			return session==null || session.WaitForClose(timeout);
		}

		public string QuarantineImage(AcquiredImage image,string reason,bool isPatientUnknown,out string error) {
			ITwainCaptureSession session=Inner;
			if(session==null) {
				error="The capture session was never opened.";
				return null;
			}
			return session.QuarantineImage(image,reason,isPatientUnknown,out error);
		}

		public bool ConfirmRedeliveredJournalCopy(AcquiredImage image,out string error) {
			ITwainCaptureSession session=Inner;
			if(session==null) {
				error="The capture session was never opened.";
				return false;
			}
			return session.ConfirmRedeliveredJournalCopy(image,out error);
		}

		public void Dispose() {
			lock(_lock) {
				_isCloseRequested=true;
			}
			Inner?.Dispose();
		}
	}
}
