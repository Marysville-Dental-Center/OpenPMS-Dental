// Native first for the vendor pieces the program loads (S3a-14n of the .NET 10 move, docs/port-notes/S3a-14.md, section "S3a-14n: native
// first"; net10-plan decision 23, the owner's of 2026-11-15: "For any integration, use native 64bit first. If the option is not available,
// fall back to 32bit"). One run-time choice per vendor piece and program process, made on first use and logged: the piece is loaded and called
// in the program's own process when it can run at the process's bitness, read from its PE header (a managed DLL that is IL-only without
// 32BITREQUIRED, or of the process's machine type; a native DLL of the process's machine type) or found by activating its COM class once
// (an out-of-process server, or an in-process server registered for this bitness); otherwise the 32-bit helper program's module runs it
// (OpenPmsDentalHelper32, S3a-13 and S3a-14), the fallback. In a 32-bit process the helper, itself 32-bit, can run nothing the process cannot,
// so a 32-bit program always chooses its own process and behaves as before S3a-13 and S3a-14.
// N64-A3 (docs/port-notes/N64-A3.md) added Mediadent / CADI: a late-bound automation server whose ProgID's registration is read in both
// registry views first (ComServerRegistration), and whose activation or first call failing in a way that says this process cannot reach the
// server (ChooseHelperAfterFailure) moves the choice to the helper.
// N64-B6 (docs/port-notes/N64-B6.md) added QuickBooks Desktop's 64-bit QBFC for a 64-bit program: the newest of a list of late-bound session
// managers whose registration (both views) and activation say it can run here (ActivateNewestComServer), and the move to the helper when
// connecting to QuickBooks failed on that path (ChooseHelperAfterConnectionFailure).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CodeBase {
	///<summary>Where a vendor piece runs (NativeFirstChooser).</summary>
	public enum VendorPath {
		///<summary>Loaded and called in the program's own process, as before S3a-13 and S3a-14.</summary>
		InProcess,
		///<summary>Called in the 32-bit helper program's module (OpenPmsDentalHelper32), the fallback.</summary>
		Helper32,
	}

	///<summary>The one run-time choice for one vendor piece in this program process: its own process or the 32-bit helper (the file's header,
	///S3a-14n). The four pieces have one chooser each; the first call that chooses fixes the choice for the life of the process and writes it to
	///the log (Logger, sub folder NativeFirst), and every later call returns it. Thread-safe.</summary>
	public sealed class NativeFirstChooser {
		///<summary>The Logger sub folder of the choices.</summary>
		public const string LOG_SUB_DIRECTORY="NativeFirst";
		///<summary>The class is not registered (for this bitness): what activating a 32-bit in-process server from a 64-bit process gives.</summary>
		public const int REGDB_E_CLASSNOTREG=unchecked((int)0x80040154);
		///<summary>The server process could not be started.</summary>
		public const int CO_E_SERVER_EXEC_FAILURE=unchecked((int)0x80080005);
		///<summary>No such interface: a server's interface cannot be reached from this process (no proxy for it registered for this bitness).
		///Also the HResult of an InvalidCastException from a failed QueryInterface of a COM object.</summary>
		public const int E_NOINTERFACE=unchecked((int)0x80004002);
		///<summary>The type library is not registered (for this bitness).</summary>
		public const int TYPE_E_LIBNOTREGISTERED=unchecked((int)0x8002801D);
		///<summary>Topaz's SigPlusNET.dll, a managed WinForms control (TopazWrapper). The current one is AnyCPU (Topaz's FAQ); an older one may be
		///32-bit only.</summary>
		public static readonly NativeFirstChooser Topaz=new NativeFirstChooser("Topaz SigPlusNET.dll");
		///<summary>Schick CDR DICOM's automation server, CDRDicom.Application (Schick.cs), whose client is its own program, CDRDicom.exe.</summary>
		public static readonly NativeFirstChooser SchickCdr=new NativeFirstChooser("Schick CDR DICOM (the CDRDicom automation server)");
		///<summary>Guru's native MedVisorInterface.dll (Guru.cs). No 64-bit one is known.</summary>
		public static readonly NativeFirstChooser Guru=new NativeFirstChooser("Guru MedVisorInterface.dll");
		///<summary>QuickBooks Desktop's QBFC 10 session manager, an in-process COM server that is 32-bit only (QuickBooks.cs).</summary>
		public static readonly NativeFirstChooser QuickBooksDesktop=new NativeFirstChooser("QuickBooks Desktop QBFC 10 (QBSessionManager)");
		///<summary>Mediadent / CADI's automation server, Mediadent.Application (CADI.cs, N64-A3), called late-bound through IDispatch. Very
		///probably a 32-bit server program of its own (LocalServer32), which a 64-bit caller reaches as well.</summary>
		public static readonly NativeFirstChooser Cadi=new NativeFirstChooser("Mediadent / CADI (the Mediadent.Application automation server)");
		///<summary>QuickBooks Desktop's 64-bit QBFC (15 and later, for QuickBooks Desktop 2022 and later), whose session manager a 64-bit program
		///creates late-bound by its ProgID, the newest first (QuickBooksLateBound.cs, N64-B6). A 32-bit program never uses it: it keeps
		///QuickBooksDesktop's QBFC 10.</summary>
		public static readonly NativeFirstChooser QuickBooksDesktopNative64=new NativeFirstChooser("QuickBooks Desktop 64-bit QBFC (QBSessionManager, late-bound)");

		private readonly object _lock=new object();
		private readonly string _vendorName;
		private VendorPath? _vendorPath;
		private string _reason="";
		private string _logLine="";
		private bool? _is64BitProcessOverride;

		public NativeFirstChooser(string vendorName) {
			_vendorName=vendorName;
		}

		///<summary>The vendor piece, as the log names it.</summary>
		public string VendorName {
			get {
				return _vendorName;
			}
		}

		///<summary>The choice made in this process, or null before the first use.</summary>
		public VendorPath? Choice {
			get {
				lock(_lock) {
					return _vendorPath;
				}
			}
		}

		///<summary>Why the choice was made, or "" before it.</summary>
		public string Reason {
			get {
				lock(_lock) {
					return _reason;
				}
			}
		}

		///<summary>The line the choice wrote to the log, or "" before it (for tests; Logger writes nothing in a unit test).</summary>
		public string LogLine {
			get {
				lock(_lock) {
					return _logLine;
				}
			}
		}

		///<summary>The bitness this chooser decides for: the process's, or the one a test set with ResetForTests.</summary>
		public bool Is64BitProcess {
			get {
				lock(_lock) {
					return _is64BitProcessOverride??Environment.Is64BitProcess;
				}
			}
		}

		///<summary>The rule: the program's own process when the piece can run there, and also in a 32-bit process, whose helper is 32-bit too and
		///could run nothing the process cannot (the vendor's own error then shows, as before); the 32-bit helper only for a 64-bit process that
		///cannot run the piece.</summary>
		public static VendorPath Decide(bool canRunInProcess,bool is64BitProcess) {
			if(canRunInProcess || !is64BitProcess) {
				return VendorPath.InProcess;
			}
			return VendorPath.Helper32;
		}

		///<summary>The choice for a managed DLL at path, made from its PE and CLI headers when it is the first (PeImageInfo.CanRunInProcess). A
		///file whose headers cannot be read stays in this process, where loading it reports why, as before.</summary>
		public VendorPath ChooseForManagedAssembly(string path) {
			lock(_lock) {
				if(_vendorPath!=null) {
					return _vendorPath.Value;
				}
				PeImageInfo peImageInfo=ReadOrNull(path,out string readError);
				if(peImageInfo==null || peImageInfo.Kind!=PeImageKind.Managed) {
					string why=peImageInfo==null ? readError : "it is not a .NET assembly ("+peImageInfo.Describe()+")";
					return SetChoice(VendorPath.InProcess,path+": the header could not be classified ("+why+"); loading it here reports why");
				}
				bool canRun=peImageInfo.CanRunInProcess(_is64BitProcessOverride??Environment.Is64BitProcess);
				return SetChoice(canRun,path+" is "+peImageInfo.Describe());
			}
		}

		///<summary>The choice for a native DLL loaded by its file name (DllImport), made from the PE header of the first fileName found where the
		///loader looks (GetNativeDllSearchFolders; a test passes its own folders). When none is found the piece counts as machineIfNotFound, the
		///vendor's known build, so a missing DLL is reported by the path that would have loaded it.</summary>
		public VendorPath ChooseForNativeDll(string fileName,ushort machineIfNotFound,IEnumerable<string> listFolders=null) {
			lock(_lock) {
				if(_vendorPath!=null) {
					return _vendorPath.Value;
				}
				bool is64BitProcess=_is64BitProcessOverride??Environment.Is64BitProcess;
				string path=FindNativeDll(fileName,listFolders);
				if(path=="") {
					PeImageInfo peImageInfoKnown=new PeImageInfo { Kind=PeImageKind.Native,Machine=machineIfNotFound,
						IsPe32Plus=machineIfNotFound!=PeImageInfo.IMAGE_FILE_MACHINE_I386 };
					return SetChoice(peImageInfoKnown.CanRunInProcess(is64BitProcess),fileName+" was not found; the vendor's known build is "
						+peImageInfoKnown.Describe());
				}
				PeImageInfo peImageInfo=ReadOrNull(path,out string readError);
				if(peImageInfo==null || peImageInfo.Kind==PeImageKind.NotPe) {
					return SetChoice(VendorPath.InProcess,path+": the header could not be classified ("+(peImageInfo==null ? readError : "not a PE file")
						+"); loading it here reports why");
				}
				return SetChoice(peImageInfo.CanRunInProcess(is64BitProcess),path+" is "+peImageInfo.Describe());
			}
		}

		///<summary>Activates the vendor's COM class in this process with activate (for example () => new CDRDicom.Application()) and returns the
		///object; null means the 32-bit helper is chosen and nothing was activated here. The first activation that succeeds chooses this process.
		///The first that fails because the class is not registered for this process's bitness (REGDB_E_CLASSNOTREG) or its server could not start
		///(CO_E_SERVER_EXEC_FAILURE) chooses the helper in a 64-bit process (null); in a 32-bit process it chooses this process and the failure is
		///thrown as before. Any other failure is thrown and chooses nothing, so the next call tries again, as the vendor code did before.</summary>
		public T ActivateComObject<T>(Func<T> activate) where T:class {
			lock(_lock) {
				if(_vendorPath==VendorPath.Helper32) {
					return null;
				}
				T obj;
				try {
					obj=activate();
				}
				catch(Exception ex) when(_vendorPath==null && IsNotActivatableHere(ex)) {
					string why="activating it here failed with 0x"+GetHResult(ex).ToString("X8")+" ("+ex.Message.Trim()+")";
					if(_is64BitProcessOverride??Environment.Is64BitProcess) {
						SetChoice(VendorPath.Helper32,why);
						return null;
					}
					SetChoice(VendorPath.InProcess,why+"; the 32-bit helper could not activate it either");
					throw;
				}
				if(_vendorPath==null) {
					SetChoice(VendorPath.InProcess,"its class activated in this process");
				}
				return obj;
			}
		}

		///<summary>True for the two failures that say the class cannot be activated from this process (also inside a TargetInvocationException).
		///</summary>
		public static bool IsNotActivatableHere(Exception ex) {
			int hResult=GetHResult(ex);
			return hResult==REGDB_E_CLASSNOTREG || hResult==CO_E_SERVER_EXEC_FAILURE;
		}

		///<summary>The choice for a COM class created late-bound from its ProgID (Mediadent / CADI, N64-A3), made from its registration
		///(funcReadRegistration, ComServerRegistration.Read; read only) when it is the first: this process when a LocalServer32 is registered in
		///either registry view (a server program of its own, which COM starts for a caller of either bitness) or an InprocServer32 for this
		///process's bitness; the 32-bit helper (in a 64-bit process) when the only server is an InprocServer32 of the other bitness. A class
		///registered nowhere, or a registry that cannot be read, gives this process without fixing the choice: creating the class there reports
		///what it did before (the vendor's program is not installed), and the next call reads the registry again.</summary>
		public VendorPath ChooseForComServer(Func<ComServerRegistration> funcReadRegistration) {
			lock(_lock) {
				if(_vendorPath!=null) {
					return _vendorPath.Value;
				}
				ComServerRegistration comServerRegistration;
				try {
					comServerRegistration=funcReadRegistration();
				}
				catch(Exception ex) {
					ex.DoNothing();//Read only, and nothing else depends on it: the activation reports what it did before.
					return VendorPath.InProcess;
				}
				if(comServerRegistration==null || !comServerRegistration.IsRegistered) {
					return VendorPath.InProcess;
				}
				bool is64BitProcess=_is64BitProcessOverride??Environment.Is64BitProcess;
				return SetChoice(comServerRegistration.CanCreateFrom(is64BitProcess),comServerRegistration.Describe());
			}
		}

		///<summary>After the vendor's COM object failed in this process (its activation or a call), true when the 32-bit helper is chosen now
		///or was chosen before: the failure says this process cannot reach the server (IsComServerUnreachableHere: REGDB_E_CLASSNOTREG,
		///CO_E_SERVER_EXEC_FAILURE, E_NOINTERFACE, TYPE_E_LIBNOTREGISTERED) and the process is 64-bit. The new choice is logged with the failure;
		///the caller decides whether to retry this call in the helper or to show its message and send the next call there. False in a 32-bit
		///process, whose helper could reach nothing this process cannot (the vendor code's failure shows as before), and for any other failure.
		///</summary>
		public bool ChooseHelperAfterFailure(Exception ex) {
			lock(_lock) {
				if(!IsComServerUnreachableHere(ex)) {
					return false;
				}
				if(!(_is64BitProcessOverride??Environment.Is64BitProcess)) {
					return false;
				}
				if(_vendorPath==VendorPath.Helper32) {
					return true;
				}
				SetChoice(VendorPath.Helper32,"it failed in this process with 0x"+GetHResult(ex).ToString("X8")+" ("+ex.GetType().Name+": "
					+ex.Message.Trim()+")"+(_vendorPath==null ? "" : ", after this process had been chosen ("+_reason+")"));
				return true;
			}
		}

		///<summary>True for the failures that say a COM server cannot be reached from this process (also inside a TargetInvocationException):
		///the class is not registered for this bitness (REGDB_E_CLASSNOTREG), its server program could not start (CO_E_SERVER_EXEC_FAILURE), or
		///its interface cannot be reached from here (E_NOINTERFACE, also an InvalidCastException from a failed QueryInterface;
		///TYPE_E_LIBNOTREGISTERED).</summary>
		public static bool IsComServerUnreachableHere(Exception ex) {
			int hResult=GetHResult(ex);
			return hResult==REGDB_E_CLASSNOTREG || hResult==CO_E_SERVER_EXEC_FAILURE || hResult==E_NOINTERFACE || hResult==TYPE_E_LIBNOTREGISTERED;
		}

		///<summary>For a vendor piece with several COM classes, one per version (QuickBooks Desktop's 64-bit QBFC, N64-B6): activates the first of
		///listProgIds (the newest first) that can run here and returns its object, with its ProgID in progIdActivated; null means the 32-bit helper
		///is chosen and nothing was activated here. Used only while nothing is chosen, so the walk runs once per process; once a class activated,
		///the caller creates that ProgID itself. Each ProgID in turn:
		///- its registration is read (funcReadRegistration, ComServerRegistration.Read, both registry views, read only); one that cannot be
		///created from this process's bitness (registered nowhere, or only as an InprocServer32 of the other bitness) is skipped without being
		///activated. A registry that cannot be read does not skip it: the activation decides.
		///- funcActivate(progId) activates it. Success chooses this process (logged with the ProgID, its registration and the ProgIDs skipped).
		///A failure that says this process cannot reach the server (IsComServerUnreachableHere: REGDB_E_CLASSNOTREG, CO_E_SERVER_EXEC_FAILURE,
		///E_NOINTERFACE, TYPE_E_LIBNOTREGISTERED, also inside a TargetInvocationException) moves on to the next older ProgID. Any other failure is
		///thrown and chooses nothing.
		///When none activated, a 64-bit process chooses the helper (logged with each ProgID's reason). A 32-bit process chooses nothing and gets
		///null: its helper is 32-bit too, and the caller keeps its own 32-bit path (QBFC 10 for QuickBooks).</summary>
		public object ActivateNewestComServer(IList<string> listProgIds,Func<string,ComServerRegistration> funcReadRegistration,Func<string,object> funcActivate,
			out string progIdActivated)
		{
			progIdActivated="";
			lock(_lock) {
				if(_vendorPath==VendorPath.Helper32) {
					return null;
				}
				bool is64BitProcess=_is64BitProcessOverride??Environment.Is64BitProcess;
				List<string> listSkipped=new List<string>();
				foreach(string progId in listProgIds) {
					ComServerRegistration comServerRegistration=null;
					try {
						comServerRegistration=funcReadRegistration(progId);
					}
					catch(Exception ex) {
						ex.DoNothing();//Read only; the activation below decides.
					}
					if(comServerRegistration!=null && !comServerRegistration.CanCreateFrom(is64BitProcess)) {
						listSkipped.Add(comServerRegistration.Describe());
						continue;
					}
					object obj;
					try {
						obj=funcActivate(progId);
					}
					catch(Exception ex) when(IsComServerUnreachableHere(ex)) {
						listSkipped.Add(progId+": activating it here failed with 0x"+GetHResult(ex).ToString("X8")+" ("+ex.Message.Trim()+")");
						continue;
					}
					if(obj==null) {
						listSkipped.Add(progId+": activating it here gave no object");
						continue;
					}
					progIdActivated=progId;
					if(_vendorPath==null) {
						string why=progId+" activated in this process"+(comServerRegistration==null ? "" : " ("+comServerRegistration.Describe()+")");
						if(listSkipped.Count>0) {
							why+="; before it: "+string.Join("; ",listSkipped);
						}
						SetChoice(VendorPath.InProcess,why);
					}
					return obj;
				}
				if(is64BitProcess) {
					SetChoice(VendorPath.Helper32,"none of "+string.Join(", ",listProgIds)+" activated in this process"
						+(listSkipped.Count==0 ? "" : ": "+string.Join("; ",listSkipped)));
				}
				return null;
			}
		}

		///<summary>After connecting to the vendor's program failed on this process's path (QuickBooks Desktop's 64-bit QBFC, N64-B6: creating the
		///session manager, CreateMsgSetRequest, OpenConnection or BeginSession, whatever the failure): true when the 32-bit helper is chosen now or
		///was chosen before, in a 64-bit process; the new choice is logged with the failure and the earlier choice, and the caller sends this call
		///to the helper once and every later call there. False in a 32-bit process, whose helper could do nothing better.</summary>
		public bool ChooseHelperAfterConnectionFailure(Exception ex) {
			lock(_lock) {
				if(!(_is64BitProcessOverride??Environment.Is64BitProcess)) {
					return false;
				}
				if(_vendorPath==VendorPath.Helper32) {
					return true;
				}
				int hResult=GetHResult(ex);
				SetChoice(VendorPath.Helper32,"connecting failed in this process with "+(hResult==0 ? "" : "0x"+hResult.ToString("X8")+" ")+"("+ex.GetType().Name+": "
					+ex.Message.Trim()+")"+(_vendorPath==null ? "" : ", after this process had been chosen ("+_reason+")"));
				return true;
			}
		}

		private static int GetHResult(Exception ex) {
			if(ex is TargetInvocationException && ex.InnerException!=null) {
				ex=ex.InnerException;
			}
			if(ex is COMException) {
				return ex.HResult;
			}
			if(ex is InvalidCastException && ex.HResult==E_NOINTERFACE) {
				return ex.HResult;
			}
			return 0;
		}

		///<summary>Forgets the choice, so the next use chooses again, as a new program process would; with is64BitProcess the chooser decides for
		///that bitness instead of the process's (null: the process's). For tests.</summary>
		public void ResetForTests(bool? is64BitProcess=null) {
			lock(_lock) {
				_vendorPath=null;
				_reason="";
				_logLine="";
				_is64BitProcessOverride=is64BitProcess;
			}
		}

		///<summary>Fixes the choice without looking at the vendor piece, for example the helper's path in an x86 test run; null forgets it. For
		///tests.</summary>
		public void ForceChoiceForTests(VendorPath? vendorPath) {
			lock(_lock) {
				_vendorPath=vendorPath;
				_reason=vendorPath==null ? "" : "set by a test";
				_logLine="";
			}
		}

		private VendorPath SetChoice(bool canRunInProcess,string why) {
			bool is64BitProcess=_is64BitProcessOverride??Environment.Is64BitProcess;
			VendorPath vendorPath=Decide(canRunInProcess,is64BitProcess);
			if(!canRunInProcess && vendorPath==VendorPath.InProcess) {
				why+="; it cannot run in this 32-bit process, and the 32-bit helper could not run it either";
			}
			return SetChoice(vendorPath,why);
		}

		private VendorPath SetChoice(VendorPath vendorPath,string why) {
			bool is64BitProcess=_is64BitProcessOverride??Environment.Is64BitProcess;
			_vendorPath=vendorPath;
			_reason=why;
			_logLine=_vendorName+": "+(vendorPath==VendorPath.InProcess ? "in the program's process" : "in the 32-bit helper program")
				+" ("+(is64BitProcess ? "64" : "32")+"-bit process); "+why+".";
			string logLine=_logLine;
			ODException.SwallowAnyException(() => Logger.WriteLine(logLine,LOG_SUB_DIRECTORY));
			return vendorPath;
		}

		private static PeImageInfo ReadOrNull(string path,out string readError) {
			readError="";
			try {
				return PeImageInfo.Read(path);
			}
			catch(Exception ex) {
				readError=ex.GetType().Name+": "+ex.Message;
				return null;
			}
		}

		///<summary>The folders where Windows' loader looks for a DLL named in a DllImport, in its order for a desktop program (safe DLL search
		///mode): the program's folder, the system folder (in a 32-bit process on 64-bit Windows that is SysWOW64, which file access from this process
		///is redirected to as well), the Windows folder, the current folder and the PATH.</summary>
		public static List<string> GetNativeDllSearchFolders() {
			List<string> listFolders=new List<string>();
			listFolders.Add(AppDomain.CurrentDomain.BaseDirectory);
			listFolders.Add(Environment.SystemDirectory);
			listFolders.Add(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
			listFolders.Add(Environment.CurrentDirectory);
			string pathVariable=Environment.GetEnvironmentVariable("PATH")??"";
			listFolders.AddRange(pathVariable.Split(new[] { ';' },StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().Trim('"')));
			return listFolders.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
		}

		///<summary>The first fileName in listFolders (null: GetNativeDllSearchFolders), or "".</summary>
		public static string FindNativeDll(string fileName,IEnumerable<string> listFolders=null) {
			foreach(string folder in listFolders??GetNativeDllSearchFolders()) {
				try {
					string path=Path.Combine(folder,fileName);
					if(File.Exists(path)) {
						return path;
					}
				}
				catch(Exception ex) {
					ex.DoNothing();//A PATH entry that is not a valid folder is skipped, as the loader skips it.
				}
			}
			return "";
		}
	}

	///<summary>What a file's PE header says it is.</summary>
	public enum PeImageKind {
		///<summary>No PE header (or not one this reader understands).</summary>
		NotPe,
		///<summary>A PE image without a CLI header: native code of its machine type.</summary>
		Native,
		///<summary>A PE image with a CLI header: a .NET assembly.</summary>
		Managed,
	}

	///<summary>The bitness facts of a PE file (an EXE or DLL): its machine type, PE32 or PE32+, and for a .NET assembly its CLI header flags
	///(ECMA-335 II.25.3.3). Read from the file's headers only; nothing is loaded.</summary>
	public sealed class PeImageInfo {
		public const ushort IMAGE_FILE_MACHINE_I386=0x014C;
		public const ushort IMAGE_FILE_MACHINE_AMD64=0x8664;
		public const ushort IMAGE_FILE_MACHINE_ARM64=0xAA64;
		public const uint COMIMAGE_FLAGS_ILONLY=0x00000001;
		public const uint COMIMAGE_FLAGS_32BITREQUIRED=0x00000002;
		public const uint COMIMAGE_FLAGS_32BITPREFERRED=0x00020000;
		private const ushort MAGIC_PE32=0x010B;
		private const ushort MAGIC_PE32_PLUS=0x020B;
		private const int INDEX_CLI_HEADER=14;

		public PeImageKind Kind;
		///<summary>The COFF header's machine type (IMAGE_FILE_MACHINE_...).</summary>
		public ushort Machine;
		///<summary>True for a PE32+ (64-bit) optional header.</summary>
		public bool IsPe32Plus;
		///<summary>The CLI header's flags (COMIMAGE_FLAGS_...); 0 for a native image.</summary>
		public uint CorFlags;

		public bool IsIlOnly {
			get {
				return (CorFlags & COMIMAGE_FLAGS_ILONLY)!=0;
			}
		}

		///<summary>32BITREQUIRED without 32BITPREFERRED: the assembly runs only in a 32-bit process. (Both flags together mean "any CPU, 32-bit
		///preferred", which a DLL loads under in either bitness.)</summary>
		public bool Is32BitRequired {
			get {
				return (CorFlags & COMIMAGE_FLAGS_32BITREQUIRED)!=0 && (CorFlags & COMIMAGE_FLAGS_32BITPREFERRED)==0;
			}
		}

		///<summary>True when the image can be loaded into a process of that bitness, on this process's runtime (the .NET 10 program's, .NET):
		///CanRunInProcess(is64BitProcess,isNetFrameworkProcess).</summary>
		public bool CanRunInProcess(bool is64BitProcess) {
			return CanRunInProcess(is64BitProcess,RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework",StringComparison.Ordinal));
		}

		///<summary>True when the image can be loaded into a process of that bitness: a managed IL-only PE32 image without 32BITREQUIRED (any CPU)
		///in either; any other image (32BITREQUIRED, mixed-mode, native, PE32+) only where its machine type is the process's (x86 for 32-bit, x64
		///for 64-bit). "Any CPU, 32-bit preferred" (32BITREQUIRED with 32BITPREFERRED) loads in either on the .NET Framework, but in a 32-bit
		///process only on .NET, which maps such an image into an x86 process only (S3b-4, PortableExecutableFile.CanLoadInProcess; S3b-5 gives the
		///chooser the same rule): the 64-bit .NET 10 program then takes the helper for it.</summary>
		public bool CanRunInProcess(bool is64BitProcess,bool isNetFrameworkProcess) {
			if(Kind==PeImageKind.NotPe) {
				return false;
			}
			if(Kind==PeImageKind.Managed && IsIlOnly && !IsPe32Plus && Machine==IMAGE_FILE_MACHINE_I386) {
				if(!is64BitProcess) {
					return true;
				}
				bool isPreferred32=(CorFlags & COMIMAGE_FLAGS_32BITREQUIRED)!=0 && (CorFlags & COMIMAGE_FLAGS_32BITPREFERRED)!=0;
				return !Is32BitRequired && (isNetFrameworkProcess || !isPreferred32);
			}
			ushort machineProcess=is64BitProcess ? IMAGE_FILE_MACHINE_AMD64 : IMAGE_FILE_MACHINE_I386;
			return Machine==machineProcess;
		}

		///<summary>For the log, for example "a managed IL-only assembly for any CPU" or "a native x86 DLL".</summary>
		public string Describe() {
			string machine;
			switch(Machine) {
				case IMAGE_FILE_MACHINE_I386:
					machine="x86";
					break;
				case IMAGE_FILE_MACHINE_AMD64:
					machine="x64";
					break;
				case IMAGE_FILE_MACHINE_ARM64:
					machine="ARM64";
					break;
				default:
					machine="machine 0x"+Machine.ToString("X4");
					break;
			}
			switch(Kind) {
				case PeImageKind.Managed:
					if(IsIlOnly && !IsPe32Plus && Machine==IMAGE_FILE_MACHINE_I386) {
						if(Is32BitRequired) {
							return "a managed IL-only assembly that requires 32-bit (x86)";
						}
						return (CorFlags & COMIMAGE_FLAGS_32BITPREFERRED)!=0 ? "a managed IL-only assembly for any CPU, 32-bit preferred"
							: "a managed IL-only assembly for any CPU";
					}
					return "a managed "+(IsIlOnly ? "IL-only" : "mixed-mode")+" "+machine+" assembly";
				case PeImageKind.Native:
					return "a native "+machine+" image";
				default:
					return "not a PE image";
			}
		}

		///<summary>Reads the headers of the file at path (FileShare.ReadWrite|Delete, so a file in use can be read).</summary>
		public static PeImageInfo Read(string path) {
			using(FileStream fileStream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete)) {
				return Read(fileStream);
			}
		}

		///<summary>Reads the headers from a seekable stream: the DOS header's e_lfanew, the "PE\0\0" signature, the COFF header, the optional header's
		///magic and its CLI header directory (index 14), the section table to find that directory in the file, and the CLI header's flags.</summary>
		public static PeImageInfo Read(Stream stream) {
			PeImageInfo notPe=new PeImageInfo { Kind=PeImageKind.NotPe };
			BinaryReader reader=new BinaryReader(stream);
			long length=stream.Length;
			if(length<0x40) {
				return notPe;
			}
			stream.Position=0;
			if(reader.ReadUInt16()!=0x5A4D) {//"MZ"
				return notPe;
			}
			stream.Position=0x3C;
			uint offsetPe=reader.ReadUInt32();
			if(offsetPe>length-24) {
				return notPe;
			}
			stream.Position=offsetPe;
			if(reader.ReadUInt32()!=0x00004550) {//"PE\0\0"
				return notPe;
			}
			ushort machine=reader.ReadUInt16();
			ushort countSections=reader.ReadUInt16();
			stream.Position+=12;//TimeDateStamp, PointerToSymbolTable, NumberOfSymbols
			ushort sizeOptionalHeader=reader.ReadUInt16();
			stream.Position+=2;//Characteristics
			long offsetOptional=stream.Position;
			if(sizeOptionalHeader<2 || offsetOptional+sizeOptionalHeader>length) {
				return notPe;
			}
			ushort magic=reader.ReadUInt16();
			if(magic!=MAGIC_PE32 && magic!=MAGIC_PE32_PLUS) {
				return notPe;
			}
			PeImageInfo peImageInfo=new PeImageInfo { Kind=PeImageKind.Native,Machine=machine,IsPe32Plus=magic==MAGIC_PE32_PLUS };
			int offsetCountDirectories=magic==MAGIC_PE32 ? 92 : 108;
			int offsetDirectories=offsetCountDirectories+4;
			if(sizeOptionalHeader<offsetDirectories) {
				return peImageInfo;
			}
			stream.Position=offsetOptional+offsetCountDirectories;
			uint countDirectories=reader.ReadUInt32();
			if(countDirectories<=INDEX_CLI_HEADER || sizeOptionalHeader<offsetDirectories+(INDEX_CLI_HEADER+1)*8) {
				return peImageInfo;
			}
			stream.Position=offsetOptional+offsetDirectories+INDEX_CLI_HEADER*8;
			uint rvaCli=reader.ReadUInt32();
			uint sizeCli=reader.ReadUInt32();
			if(rvaCli==0 || sizeCli<20) {
				return peImageInfo;
			}
			long offsetSections=offsetOptional+sizeOptionalHeader;
			for(int i=0;i<countSections;i++) {
				long offsetSection=offsetSections+i*40;
				if(offsetSection+40>length) {
					break;
				}
				stream.Position=offsetSection+8;
				uint virtualSize=reader.ReadUInt32();
				uint virtualAddress=reader.ReadUInt32();
				uint sizeRaw=reader.ReadUInt32();
				uint pointerRaw=reader.ReadUInt32();
				uint sizeSection=Math.Max(virtualSize,sizeRaw);
				if(rvaCli<virtualAddress || rvaCli>=(long)virtualAddress+sizeSection) {
					continue;
				}
				long offsetCli=(long)rvaCli-virtualAddress+pointerRaw;
				if(offsetCli+20>length) {
					return notPe;
				}
				stream.Position=offsetCli+16;//cb, MajorRuntimeVersion, MinorRuntimeVersion, MetaData
				peImageInfo.Kind=PeImageKind.Managed;
				peImageInfo.CorFlags=reader.ReadUInt32();
				return peImageInfo;
			}
			return peImageInfo;
		}
	}
}
