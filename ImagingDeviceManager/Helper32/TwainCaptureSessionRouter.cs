// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using ImagingDeviceManager.Capture;
using ImagingDeviceManager.Journal;

namespace ImagingDeviceManager.Helper32 {
	///<summary>The capture session the program creates (S3b-8): at Open it asks TwainSourcePath where the stored source runs, native first, and
	///becomes a TwainCaptureSession in the program's process or a TwainHelperCaptureSession through the 32-bit helper; every member and event is
	///that session's from then on. Before Open it keeps SessionGuid, QuarantineContext and FuncReadMetadataKeys and gives them to the session it
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

		public TwainCaptureOpenResult Open(string productName,TwainCaptureOptions options) {
			ITwainCaptureSession session;
			lock(_lock) {
				if(_session!=null) {
					return _session.Open(productName,options);//AlreadyOpen, or a second open of a closed session, as that session answers it
				}
			}
			TwainSourceResolution resolution;
			try {
				resolution=_sourcePath.Resolve(productName);
			}
			catch(Exception ex) {
				resolution=new TwainSourceResolution { ProductName=TwainSourceChoice.GetProductName(productName),Reason="the choice failed: "+ex.Message };
			}
			Resolution=resolution;
			string line="TWAIN source \""+(productName??"").Trim()+"\": "+(resolution.Dsm==TwainDsmKind.Helper32 ? "the 32-bit helper" : "this process")
				+", because "+resolution.Reason+".";
			lock(_lock) {
				session=resolution.Dsm==TwainDsmKind.Helper32 ? _funcCreateHelper() : _funcCreateNative();
				session.SessionGuid=_sessionGuid;
				session.QuarantineContext=_quarantineContext;
				session.FuncReadMetadataKeys=_funcReadMetadataKeys;
				session.ImageAcquired+=x => ImageAcquired?.Invoke(x);
				session.Ended+=x => Ended?.Invoke(x);
				session.StatusChanged+=x => StatusChanged?.Invoke(x);
				session.ImageQuarantined+=x => ImageQuarantined?.Invoke(x);
				session.Logged+=x => Logged?.Invoke(x);
				_session=session;
			}
			WriteCaptureLog(line);
			if(resolution.ErrorText!=null) {
				WriteCaptureLog("The source cannot be opened: "+resolution.ErrorText);
				//Other, so that the window shows this text (CaptureEndText.GetOpenText) and not only "not found".
				return new TwainCaptureOpenResult { Error=TwainCaptureOpenError.Other,ErrorText=resolution.ErrorText };
			}
			return session.Open(resolution.ProductName,options);
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
			Inner?.Dispose();
		}
	}
}
