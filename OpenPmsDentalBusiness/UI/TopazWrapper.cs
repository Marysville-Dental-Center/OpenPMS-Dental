using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDentalBusiness;
using OpenPmsDentalHelper32.Topaz;

namespace OpenPmsDental.UI {
	///<summary>The Topaz signature pad control (Topaz.SigPlusNET in Topaz's SigPlusNET.dll). That file has no published redistribution terms, so the
	///program never references or ships it: it is loaded at run time from the program folder or from Topaz's own installation, and an office that
	///signs with a Topaz pad installs Topaz's software. Without it, GetTopaz throws a TopazNotInstalledException whose message starts with
	///NOT_INSTALLED_MESSAGE, callers keep a null control, and every other method here does nothing (or returns 0 or "") for a null control. Every
	///call goes to the control's own public methods by name, as the compiled calls did, so signatures behave as before when Topaz is installed.
	///
	///Since S3a-14 of the .NET 10 move (docs/port-notes/S3a-14.md) Topaz's control lives in the 32-bit helper program (OpenPmsDentalHelper32, its
	///Topaz module), because the program is 64-bit after stage 3b (open point O-14). The program finds SigPlusNET.dll as before and the helper loads
	///it; GetTopaz returns a TopazSignatureView, which stands in the program's window where Topaz's control stood and shows the helper's picture of
	///it; every call below goes to the helper's control, the same calls in the same order with the same arguments, so stored signatures and their
	///checks do not change. The patient signs in the helper's signing window (ShowSigningWindow), shown over the program's window.</summary>
	public class TopazWrapper {
		///<summary>Shown when a Topaz signature is to be captured or shown and Topaz's software cannot be loaded.</summary>
		public const string NOT_INSTALLED_MESSAGE="Topaz signature pad software is not installed.";
		///<summary>Topaz's .NET signature control assembly.</summary>
		public const string ASSEMBLY_FILE_NAME=TopazCommands.ASSEMBLY_FILE_NAME;
		///<summary>The signature control in that assembly.</summary>
		public const string CONTROL_TYPE_NAME=TopazCommands.CONTROL_TYPE_NAME;
		///<summary>How long the helper may take to load SigPlusNET.dll (its first call also starts the helper).</summary>
		private static readonly TimeSpan _timeSpanLoad=TimeSpan.FromSeconds(60);
		///<summary>How many folder levels below a "Topaz Systems" folder are searched for the assembly.</summary>
		private const int SEARCH_DEPTH=3;
		private static readonly object _lock=new object();
		private static bool _isLoadAttempted;
		private static bool _isInstalled;
		private static string _pathAssembly="";
		private static string _loadError="";
		private static List<string> _listSearchFoldersOverride;
		private static Helper32Client _helper32ClientOverride;

		#region Loading
		///<summary>The folders searched for SigPlusNET.dll, in order: the program folder, every "Topaz Systems" folder under Program Files (x86) and
		///Program Files (and up to three levels below it), and the Windows SigPlus folder. INFERRED: Topaz's installers have put the file in these
		///places; an office whose Topaz software lives elsewhere can copy SigPlusNET.dll into the program folder. The program searches; the helper
		///loads the file found.</summary>
		public static List<string> GetSearchFolders() {
			if(_listSearchFoldersOverride!=null) {
				return new List<string>(_listSearchFoldersOverride);
			}
			List<string> listFolders=new List<string>();
			listFolders.Add(AppDomain.CurrentDomain.BaseDirectory);
			List<string> listProgramFiles=new List<string>() {
				Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
				Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
				Environment.GetEnvironmentVariable("ProgramW6432"),
			};
			foreach(string programFiles in listProgramFiles.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase)) {
				AddFolderAndSubfolders(listFolders,Path.Combine(programFiles,"Topaz Systems"),SEARCH_DEPTH);
			}
			string windows=Environment.GetFolderPath(Environment.SpecialFolder.Windows);
			if(!string.IsNullOrWhiteSpace(windows)) {
				listFolders.Add(Path.Combine(windows,"SigPlus"));
			}
			return listFolders.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
		}

		private static void AddFolderAndSubfolders(List<string> listFolders,string folder,int depth) {
			if(!Directory.Exists(folder)) {
				return;
			}
			listFolders.Add(folder);
			if(depth<=0) {
				return;
			}
			string[] arraySubfolders;
			try {
				arraySubfolders=Directory.GetDirectories(folder);
			}
			catch(Exception ex) {
				ex.DoNothing();//A folder we may not list is skipped.
				return;
			}
			foreach(string subfolder in arraySubfolders.OrderBy(x => x,StringComparer.OrdinalIgnoreCase)) {
				AddFolderAndSubfolders(listFolders,subfolder,depth-1);
			}
		}

		///<summary>The first SigPlusNET.dll in GetSearchFolders, or "" when there is none.</summary>
		public static string FindAssemblyPath() {
			foreach(string folder in GetSearchFolders()) {
				string path=Path.Combine(folder,ASSEMBLY_FILE_NAME);
				if(File.Exists(path)) {
					return path;
				}
			}
			return "";
		}

		///<summary>The 32-bit helper that runs Topaz's control: the program's, or the one a test set with ResetLoader.</summary>
		public static Helper32Client GetHelper32Client() {
			lock(_lock) {
				return _helper32ClientOverride??Helper32Client.Default;
			}
		}

		///<summary>True when Topaz's signature control can be created on this computer: a SigPlusNET.dll was found and the helper loaded its control
		///type. Checked once (ResetLoader checks again); no helper is started when no SigPlusNET.dll is found.</summary>
		public static bool IsInstalled() {
			lock(_lock) {
				if(_isLoadAttempted) {
					return _isInstalled;
				}
				_isLoadAttempted=true;
				_isInstalled=false;
				string path=FindAssemblyPath();
				if(path=="") {
					_loadError="";
					return false;
				}
				try {
					string loadError=(_helper32ClientOverride??Helper32Client.Default).Call(TopazCommands.MODULE,TopazCommands.LOAD,_timeSpanLoad,path)[0];
					if(loadError!="") {
						_loadError=loadError;
						return false;
					}
				}
				catch(Helper32Exception ex) {
					_loadError=path+" could not be loaded: "+ex.Message;
					return false;
				}
				_pathAssembly=path;
				_isInstalled=true;
				return true;
			}
		}

		///<summary>NOT_INSTALLED_MESSAGE, followed by the load error when a SigPlusNET.dll was found but could not be used.</summary>
		public static string GetNotInstalledMessage() {
			return NOT_INSTALLED_MESSAGE+GetLoadErrorLine();
		}

		///<summary>"" when no SigPlusNET.dll was found, else a new line and why the one found could not be used.</summary>
		public static string GetLoadErrorLine() {
			lock(_lock) {
				return _loadError=="" ? "" : "\r\n"+_loadError;
			}
		}

		///<summary>Forgets the loaded control type so the next call searches again. With a list, only those folders are searched from then on; null
		///restores the default search. With a client, the controls live in that helper from then on (a copy of the helper a test started); null
		///restores the program's. For tests.</summary>
		public static void ResetLoader(List<string> listSearchFolders=null,Helper32Client helper32Client=null) {
			lock(_lock) {
				_listSearchFoldersOverride=listSearchFolders==null ? null : new List<string>(listSearchFolders);
				_helper32ClientOverride=helper32Client;
				_isLoadAttempted=false;
				_isInstalled=false;
				_pathAssembly="";
				_loadError="";
			}
		}

		///<summary>Calls the control's public instance method of that name whose parameter count matches, in the helper (TopazSignatureView.Call),
		///where the arguments are converted to its parameter types as before. What the method threw comes back as a Helper32Exception (Remote) with
		///the method's message.</summary>
		private static object Call(Control topaz,string methodName,params object[] arrayArgs) {
			return GetView(topaz).Call(methodName,arrayArgs);
		}

		private static TopazSignatureView GetView(Control topaz) {
			if(!(topaz is TopazSignatureView topazSignatureView)) {
				throw new ArgumentException("Not a Topaz control of TopazWrapper.GetTopaz: "+topaz.GetType().FullName);
			}
			return topazSignatureView;
		}

		///<summary>Thrown by GetTopaz when Topaz's software is not installed. The message is GetNotInstalledMessage.</summary>
		public class TopazNotInstalledException:Exception {
			public TopazNotInstalledException(string message):base(message) {
			}
		}
		#endregion Loading

		#region GetTopaz... Methods
		//The plugin calls in this class were added in order to use a Topaz pad in a RDP/Citrix remote session environment so that signature pads can be
		//plugged in on the client computer while the signature displays in the SigPlusNET UI control on the terminal server.  The signature pad and the
		//UI control are always kept in sync, i.e. the state, encryption and compression modes, key data and key string, signature string etc.  This means
		//the GetTopaz... methods could retrieve data from the UI control and not the remote signature pad.  The GetTopazNumberOfTabletPoints method returns
		//0 if sent to the remote device, so it must be called on the local UI control.

		///<summary>A new Topaz signature control (a TopazSignatureView; its control in the helper is created on its first call). Throws a
		///TopazNotInstalledException when Topaz's software is not installed.</summary>
		public static Control GetTopaz() {
			if(!IsInstalled()) {
				throw new TopazNotInstalledException(GetNotInstalledMessage());
			}
			string pathAssembly;
			lock(_lock) {
				pathAssembly=_pathAssembly;
			}
			Control retVal=new TopazSignatureView(GetHelper32Client(),pathAssembly);
			retVal.HandleDestroyed+=new EventHandler(topazWrapper_HandleDestroyed);
			return retVal;
		}

		///<summary>This allows third-party software to unload device/clean-up.</summary>
		private static void topazWrapper_HandleDestroyed(object sender,EventArgs e) {
			Plugins.HookMethod(null,"TopazWrapper.topazWrapper_HandleDestroyed");
		}

		///<summary>0=disable signature capture.  1=enable capture.</summary>
		public static int GetTopazState(Control topaz) {
			if(topaz==null) {//Topaz's software is not installed (GetTopaz threw), so there is no control.
				return 0;
			}
			int stateCur = 0;
			object[] paramArray = { topaz,stateCur };
			if(Plugins.HookMethod(null,"TopazWrapper.GetTopazState",paramArray)) {
				stateCur=(int)paramArray[1];
			}
			else {
				stateCur=Convert.ToInt32(Call(topaz,"GetTabletState"));
			}
			return stateCur;
		}

		///<summary>Returns 0 if the signature is not valid, i.e. the compression or encryption modes are not the same used when the signature was saved,
		///or the key data is different than that used to save the signature, or the signature string is blank, etc.  Valid signatures should consist of
		///at least 200 points, as this represents approximately 1 second of active signature time.  We will consider any value greater than 0 to mean a
		///signature is valid.</summary>
		public static int GetTopazNumberOfTabletPoints(Control topaz) {
			if(topaz==null) {//Topaz's software is not installed (GetTopaz threw), so there is no control.
				return 0;
			}
			return Convert.ToInt32(Call(topaz,"NumberOfTabletPoints"));
		}

		public static string GetTopazString(Control topaz) {
			if(topaz==null) {//Topaz's software is not installed (GetTopaz threw), so there is no control.
				return "";
			}
			//if in remote session, always get the sig string from the UI control, not the remote sig pad, because a form can have more than one sig box
			//and the sig pad will only have the most recent sig string.
			return (string)Call(topaz,"GetSigString");
		}
		#endregion GetTopaz... Methods

		#region SetTopaz... Methods
		//The SetTopaz... methods have a plugin call in order for HSB signature pads to be used in a RDP/Citrix remote session environment.  It may not be
		//necessary to send all of the SetTopaz... methods to the device connected to the client machine.  The encryption and compression modes and the
		//key data and key string are only necessary when saving the signature, not during signature capture.  Since the captured signature is duplicated
		//on the server in the SigPlusNET control in the OD UI, we can set the compression, encryption, key data, and key string on the control before
		//getting the signature string and validate it using the control as well.  We will leave the plugin calls just in case they are needed in the future.

		public static void ClearTopaz(Control topaz) {
			if(topaz==null) {//Topaz's software is not installed (GetTopaz threw), so there is no control.
				return;
			}
			Call(topaz,"ClearTablet");
			Call(topaz,"SetTabletLogicalXSize",2000);
			Call(topaz,"SetTabletLogicalYSize",600);
			Plugins.HookAddCode(null,"TopazWrapper.ClearTopaz_end",topaz);
		}

		public static void SetTopazCompressionMode(Control topaz,int compressionMode) {
			if(topaz==null) {//Topaz's software is not installed (GetTopaz threw), so there is no control.
				return;
			}
			Call(topaz,"SetSigCompressionMode",compressionMode);
			Plugins.HookAddCode(null,"TopazWrapper.SetTopazCompressionMode_end",topaz,compressionMode);
		}

		public static void SetTopazEncryptionMode(Control topaz,int encryptionMode) {
			if(topaz==null) {//Topaz's software is not installed (GetTopaz threw), so there is no control.
				return;
			}
			Call(topaz,"SetEncryptionMode",encryptionMode);
			Plugins.HookAddCode(null,"TopazWrapper.SetTopazEncryptionMode_end",topaz,encryptionMode);
		}

		public static void SetTopazKeyString(Control topaz,string str) {
			if(topaz==null) {//Topaz's software is not installed (GetTopaz threw), so there is no control.
				return;
			}
			Call(topaz,"SetKeyString",str);
			Plugins.HookAddCode(null,"TopazWrapper.SetTopazKeyString_end",topaz,str);
		}

		public static void SetTopazAutoKeyANSIData(Control topaz,string data) {
			if(topaz==null) {//Topaz's software is not installed (GetTopaz threw), so there is no control.
				return;
			}
			Call(topaz,"AutoKeyStart");
			Call(topaz,"SetAutoKeyANSIData",data);
			Call(topaz,"AutoKeyFinish");
			Plugins.HookAddCode(null,"TopazWrapper.SetTopazAutoKeyANSIData_end",topaz,data);
		}

		///<summary>Topaz Systems Inc. deprecated SetAutoKeyData() and AutoKeyAddData(). Use SetAutoKeyANSIData() instead.</summary>
		public static void SetTopazAutoKeyData(Control topaz,string data) {
			if(topaz==null) {//Topaz's software is not installed (GetTopaz threw), so there is no control.
				return;
			}
			Call(topaz,"AutoKeyStart");
			Call(topaz,"SetAutoKeyData",data);
			Call(topaz,"AutoKeyFinish");
			Plugins.HookAddCode(null,"TopazWrapper.SetTopazAutoKeyData_end",topaz,data);
		}

		public static void SetTopazSigString(Control topaz,string signature) {
			if(topaz==null) {//Topaz's software is not installed (GetTopaz threw), so there is no control.
				return;
			}
			try {
				Call(topaz,"SetSigString",signature);
			}
			catch(Helper32Exception ex) when(ex.Failure==Helper32Failure.Remote) {
				//The signature could have become corrupted within the database (or from wherever it is coming from).
				//The Topaz dll can throw exceptions for invalid signatures even if the signature data is a valid ASCII string.
				//E.g. A customer had a plethora of treatment plan signatures with a value of '1' which is a valid ASCII string but is not a valid signature.
				//This caused Topaz to throw a System.IndexOutOfRangeException: 'Index was outside the bounds of the array.'
				//No 'tablet points' should be present on the signature pad at this time which is how calling methods will know the signature was invalid.
				//Since S3a-14 what Topaz threw comes from the helper as a Remote failure; a failure of the helper itself is not swallowed here.
				ex.DoNothing();//Do nothing so that the program doesn't crash.
			}
			//this hook shouldn't be implemented or necessary, we only set/get the sig string on the local UI control, so this command shouldn't be sent to
			//the remote connection sig pad
			Plugins.HookAddCode(null,"TopazWrapper.SetTopazSigString_end",topaz,signature);
		}

		///<summary>0=disable signature capture.  1=enable.</summary>
		public static void SetTopazState(Control topaz,int state) {
			if(topaz==null) {//Topaz's software is not installed (GetTopaz threw), so there is no control.
				return;
			}
			//the topaz state should only be set on the actual sig pad device, not the local UI control if using in remote session
			if(Plugins.HookMethod(null,"TopazWrapper.SetTopazState",topaz,state)) {
				return;
			}
			Call(topaz,"SetTabletState",state);
		}
		#endregion SetTopaz... Methods

		#region Signing
		///<summary>Shows the helper's signing window with topaz's control over the window of controlOwner, until the patient presses OK or Cancel
		///(Cancel clears the pad). The caller has cleared the pad and turned capture on, as before S3a-14 when the patient signed into the box in the
		///program's window. Meanwhile this thread's windows are disabled, as for a modal window of the program, and keep painting
		///(Helper32Client.CallPumping, which Schick.cs uses too). True for OK; false also for a null control. Throws a Helper32Exception when the
		///helper fails.</summary>
		public static bool ShowSigningWindow(Control topaz,Control controlOwner) {
			if(topaz==null) {//Topaz's software is not installed (GetTopaz threw), so there is no control.
				return false;
			}
			IntPtr handleOwner=IntPtr.Zero;
			if(controlOwner!=null && controlOwner.IsHandleCreated) {
				handleOwner=NativeMethods.GetAncestor(controlOwner.Handle,NativeMethods.GA_ROOT);//a WinForms form or the WPF window that hosts it
			}
			return GetView(topaz).Sign(handleOwner,Lans.g("TopazWrapper","Sign on the Topaz pad"),Lans.g("TopazWrapper","OK"),
				Lans.g("TopazWrapper","Clear"),Lans.g("TopazWrapper","Cancel"));
		}

		private static class NativeMethods {
			public const uint GA_ROOT=2;

			[DllImport("user32.dll")]
			public static extern IntPtr GetAncestor(IntPtr hWnd,uint flags);
		}
		#endregion Signing

		#region Helper Methods

		public static void FillSignatureANSI(Control topaz,string keyData,string signature,OpenPmsDental.UI.SignatureBoxWrapper.SigMode sigMode) {
			if(topaz==null) {//Topaz's software is not installed (GetTopaz threw), so there is no control.
				return;
			}
			ClearTopaz(topaz);
			SetTopazCompressionMode(topaz,0);
			SetTopazEncryptionMode(topaz,0);
			SetTopazKeyString(topaz,"0000000000000000");//Clear out the key string
			SetTopazEncryptionMode(topaz,2);//high encryption
			SetTopazCompressionMode(topaz,2);//high compression
			string hashedKeyData;
			switch(sigMode) {
				case OpenPmsDental.UI.SignatureBoxWrapper.SigMode.TreatPlan:
					hashedKeyData=TreatPlans.GetHashStringForSignature(keyData);//Passed in key data has not been hashed yet.
					SetTopazAutoKeyANSIData(topaz,hashedKeyData);
					break;
				case OpenPmsDental.UI.SignatureBoxWrapper.SigMode.OrthoChart:
					hashedKeyData=OrthoCharts.GetHashStringForSignature(keyData);//Passed in key data has not been hashed yet.
					SetTopazAutoKeyANSIData(topaz,hashedKeyData);
					break;
				case OpenPmsDental.UI.SignatureBoxWrapper.SigMode.Document:
				case OpenPmsDental.UI.SignatureBoxWrapper.SigMode.Default:
				default:
					SetTopazAutoKeyANSIData(topaz,keyData);
					break;
			}
			SetTopazSigString(topaz,signature);
		}

		///<summary>Use as a last resort. Enumerates all encodings to find the first encoding that successfully decrypts the signature.</summary>
		public static void FillSignatureEncodings(Control topaz,string keyData,string signature,OpenPmsDental.UI.SignatureBoxWrapper.SigMode sigMode) {
			if(topaz==null) {//Topaz's software is not installed (GetTopaz threw), so there is no control.
				return;
			}
			//Figure out if key data needs to be hashed depending on the SigMode.
			string keyDataHashed;
			switch(sigMode) {
				case OpenPmsDental.UI.SignatureBoxWrapper.SigMode.TreatPlan:
					keyDataHashed=TreatPlans.GetHashStringForSignature(keyData);
					break;
				case OpenPmsDental.UI.SignatureBoxWrapper.SigMode.OrthoChart:
					keyDataHashed=OrthoCharts.GetHashStringForSignature(keyData);
					break;
				case OpenPmsDental.UI.SignatureBoxWrapper.SigMode.Document:
				case OpenPmsDental.UI.SignatureBoxWrapper.SigMode.Default:
				default:
					keyDataHashed=keyData;
					break;
			}
			//Use reflection to directly set encoded bytes as the KeyData that Topaz should use when decrypting the signature.
			//Since S3a-14 the reflection runs in the helper, on its control: the control's private Sig field and the private SetAutoKeyData() method
			//that accepts a byte array from the Sig field. This method allows us to bypass the incorrect encodings that are forced on us in the public
			//methods.
			TopazSignatureView topazSignatureView=GetView(topaz);
			if(!topazSignatureView.CanSetAutoKeyBytes()) {
				return;
			}
			List<Encoding> listEncodings=Encoding.GetEncodings().Select(x => x.GetEncoding()).ToList();
			for(int i=0;i<listEncodings.Count;i++) {
				//Completely clear and reset the signature pad in-between each encoding.
				ClearTopaz(topaz);
				SetTopazCompressionMode(topaz,0);
				SetTopazEncryptionMode(topaz,0);
				SetTopazKeyString(topaz,"0000000000000000");//Clear out the key string
				SetTopazEncryptionMode(topaz,2);//high encryption
				SetTopazCompressionMode(topaz,2);//high compression
				try {
					Call(topaz,"AutoKeyStart");
					//Invoke SetAutoKeyData() with the encoded bytes of keyDataHashed.
					topazSignatureView.SetAutoKeyBytes(listEncodings[i].GetBytes(keyDataHashed));
					Call(topaz,"AutoKeyFinish");
				}
				catch(Helper32Exception ex) when(ex.Failure==Helper32Failure.Remote) {
					ex.DoNothing();
					continue;
				}
				SetTopazSigString(topaz,signature);
				if(GetTopazNumberOfTabletPoints(topaz) > 0) {
					break;
				}
			}
		}

		#endregion

	}
}
