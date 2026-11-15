// How a COM class that a vendor piece is created by (a ProgID such as Mediadent.Application) is registered on this PC, read from the registry
// in both views (N64-A3 of the native 64-bit integration survey, docs/port-notes/N64-A3.md; docs/specs/native64-survey.md, section 3.8). Read
// only: nothing is written, no class is activated and no server is started. NativeFirstChooser.ChooseForComServer decides with it.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using Microsoft.Win32;

namespace CodeBase {
	///<summary>The registry the reading goes to: the default values and keys under HKEY_CLASSES_ROOT in one of its two views (the 64-bit view
	///and the 32-bit one, WOW6432Node). WindowsComRegistryReader reads Windows' registry; a test passes a fake.</summary>
	public interface IComRegistryReader {
		///<summary>The default value of HKEY_CLASSES_ROOT\subKey in that view as a string, or null when the key or its default value is not there.
		///</summary>
		string GetDefaultValue(bool isView64,string subKey);
		///<summary>True when HKEY_CLASSES_ROOT\subKey is there in that view.</summary>
		bool HasKey(bool isView64,string subKey);
	}

	///<summary>Windows' registry, read only (KEY_READ). On a 32-bit Windows both views are the one registry.</summary>
	public sealed class WindowsComRegistryReader:IComRegistryReader {
		public static readonly WindowsComRegistryReader Default=new WindowsComRegistryReader();

		public string GetDefaultValue(bool isView64,string subKey) {
			using(RegistryKey registryKeyRoot=RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot,isView64 ? RegistryView.Registry64 : RegistryView.Registry32))
			using(RegistryKey registryKey=registryKeyRoot.OpenSubKey(subKey,writable:false)) {
				return registryKey?.GetValue("") as string;
			}
		}

		public bool HasKey(bool isView64,string subKey) {
			using(RegistryKey registryKeyRoot=RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot,isView64 ? RegistryView.Registry64 : RegistryView.Registry32))
			using(RegistryKey registryKey=registryKeyRoot.OpenSubKey(subKey,writable:false)) {
				return registryKey!=null;
			}
		}
	}

	///<summary>The registration of one ProgID's class: its class id (HKCR\ProgID\CLSID, or the CLSID of its CurVer ProgID), and which server keys
	///HKCR\CLSID\{clsid} has in each registry view. A LocalServer32 is a server program of its own (out of process), which COM starts for a caller
	///of either bitness and reaches through its proxies; an InprocServer32 is a DLL, which only a process of the view's bitness can load. Read
	///reads both views; nothing is cached.</summary>
	public sealed class ComServerRegistration {
		public string ProgId="";
		///<summary>The class id in registry form ({...}), or "" when the ProgID has none in either view.</summary>
		public string Clsid="";
		public bool HasLocalServer32View64;
		public bool HasLocalServer32View32;
		public bool HasInprocServer32View64;
		public bool HasInprocServer32View32;

		///<summary>True when the class has a server key in either view. False: the vendor's program is not installed (or not registered), and the
		///activation says so as before.</summary>
		public bool IsRegistered {
			get {
				return HasLocalServer || HasInprocServer32View64 || HasInprocServer32View32;
			}
		}

		///<summary>A LocalServer32 in either view: COM starts the server program for a caller of either bitness (a 64-bit caller also finds one
		///registered in the 32-bit view only).</summary>
		public bool HasLocalServer {
			get {
				return HasLocalServer32View64 || HasLocalServer32View32;
			}
		}

		///<summary>An InprocServer32 in the view of that bitness: a process of that bitness can load the server DLL.</summary>
		public bool HasInprocServer(bool is64Bit) {
			return is64Bit ? HasInprocServer32View64 : HasInprocServer32View32;
		}

		///<summary>True when a process of that bitness can create the class: it has a LocalServer32 in either view, or an InprocServer32 for that
		///bitness. False for a class registered nowhere, or only as an InprocServer32 of the other bitness.</summary>
		public bool CanCreateFrom(bool is64BitProcess) {
			return HasLocalServer || HasInprocServer(is64BitProcess);
		}

		///<summary>For the log, for example "Mediadent.Application ({...}) has LocalServer32 in the 32-bit registry view".</summary>
		public string Describe() {
			if(Clsid=="") {
				return ProgId+" has no class id in either registry view";
			}
			string servers="";
			servers+=Describe("LocalServer32",HasLocalServer32View64,HasLocalServer32View32);
			string inproc=Describe("InprocServer32",HasInprocServer32View64,HasInprocServer32View32);
			if(inproc!="") {
				servers+=(servers=="" ? "" : " and ")+inproc;
			}
			if(servers=="") {
				return ProgId+" ("+Clsid+") has no server key in either registry view";
			}
			return ProgId+" ("+Clsid+") has "+servers;
		}

		private static string Describe(string nameKey,bool isInView64,bool isInView32) {
			if(isInView64 && isInView32) {
				return nameKey+" in both registry views";
			}
			if(isInView64) {
				return nameKey+" in the 64-bit registry view only";
			}
			if(isInView32) {
				return nameKey+" in the 32-bit registry view only";
			}
			return "";
		}

		///<summary>Reads progId's registration from both views (null reader: Windows' registry). The class id is the first found, the 64-bit view
		///first; its server keys are looked up in both views, as COM looks for an out-of-process server in both.</summary>
		public static ComServerRegistration Read(string progId,IComRegistryReader comRegistryReader=null) {
			comRegistryReader=comRegistryReader??WindowsComRegistryReader.Default;
			ComServerRegistration comServerRegistration=new ComServerRegistration();
			comServerRegistration.ProgId=progId;
			string clsid=ReadClsid(comRegistryReader,true,progId)??ReadClsid(comRegistryReader,false,progId);
			if(clsid==null) {
				return comServerRegistration;
			}
			comServerRegistration.Clsid=clsid;
			string keyClass=@"CLSID\"+clsid;
			comServerRegistration.HasLocalServer32View64=comRegistryReader.HasKey(true,keyClass+@"\LocalServer32");
			comServerRegistration.HasLocalServer32View32=comRegistryReader.HasKey(false,keyClass+@"\LocalServer32");
			comServerRegistration.HasInprocServer32View64=comRegistryReader.HasKey(true,keyClass+@"\InprocServer32");
			comServerRegistration.HasInprocServer32View32=comRegistryReader.HasKey(false,keyClass+@"\InprocServer32");
			return comServerRegistration;
		}

		///<summary>The class id of HKCR\progId\CLSID in that view in registry form, or that of its CurVer ProgID (one step) when it has none; null
		///when neither has a class id that parses as a GUID.</summary>
		private static string ReadClsid(IComRegistryReader comRegistryReader,bool isView64,string progId) {
			string clsid=ParseClsid(comRegistryReader.GetDefaultValue(isView64,progId+@"\CLSID"));
			if(clsid!=null) {
				return clsid;
			}
			string progIdCurrent=comRegistryReader.GetDefaultValue(isView64,progId+@"\CurVer");
			if(string.IsNullOrWhiteSpace(progIdCurrent) || progIdCurrent.IndexOf('\\')>=0) {
				return null;
			}
			return ParseClsid(comRegistryReader.GetDefaultValue(isView64,progIdCurrent.Trim()+@"\CLSID"));
		}

		private static string ParseClsid(string value) {
			if(!Guid.TryParse(value?.Trim(),out Guid guid)) {
				return null;
			}
			return guid.ToString("B").ToUpperInvariant();
		}
	}
}
