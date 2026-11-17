// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ImagingDeviceManager.TwainProtocol;

namespace ImagingDeviceManager.Helper32 {
	///<summary>Where a TWAIN source runs (S3b-8).</summary>
	public sealed class TwainSourceResolution {
		///<summary>Native: the program's own process and DSM (TwainCaptureSession). Helper32: the 32-bit helper (TwainHelperCaptureSession).
		///</summary>
		public TwainDsmKind Dsm { get; set; }=TwainDsmKind.Native;
		///<summary>The ProductName to open.</summary>
		public string ProductName { get; set; }="";
		///<summary>Why, for the capture log.</summary>
		public string Reason { get; set; }="";
		///<summary>Set when the source cannot be opened at all on either path for a reason the session itself would not report (the 32-bit DSM is
		///missing for a 32-bit choice, the helper failed while it listed the 32-bit sources): the open fails with this text.</summary>
		public string ErrorText { get; set; }
	}

	///<summary>The native-first choice of the DSM for a TWAIN source (decision 23 of docs/specs/net10-plan.md, package S3b-8): a source that the
	///program's own DSM lists always runs in the program's process (in the x64 program the 64-bit TWAINDSM.dll of System32 and the source's 64-bit
	///driver); a source that only the 32-bit DSM lists (SysWOW64\TWAINDSM.dll, which the setup installs, or the legacy twain_32.dll) runs in the
	///32-bit helper. A 32-bit program never uses the helper, which could run nothing it cannot. The choice is made at each open, from what the
	///DSMs list then, so a source whose vendor later installs a 64-bit driver moves to the program's process by itself, and a stored choice from
	///before S3b-8 (a ProductName without the 32-bit marker) keeps working on whichever path lists it. Thread-safe: it keeps no state.</summary>
	public sealed class TwainSourcePath {
		///<summary>How long listing the 32-bit sources may take (the helper's start, the DSM's load, every source's identity).</summary>
		public static readonly TimeSpan TimeSpanListHelper=TimeSpan.FromSeconds(60);
		private readonly DsmLoader _dsmLoaderNative;
		private readonly Func<ITwainHelperChannel> _funcChannel;
		private readonly Func<bool> _funcIs32BitDsmPresent;
		private readonly bool _is64BitProcess;

		///<summary>The program's: its own DSM (NativeTwainDsm), the helper channel funcChannel gives (null: no helper path), and the 32-bit DSM's
		///files of this computer.</summary>
		public TwainSourcePath(Func<ITwainHelperChannel> funcChannel)
			:this(LoadNativeDsm,funcChannel,Is32BitDsmInstalled,Environment.Is64BitProcess)
		{
		}

		///<summary>For tests: a fake native DSM, a channel to a helper (or an in-memory one), whether a 32-bit DSM is there, and the bitness.</summary>
		internal TwainSourcePath(DsmLoader dsmLoaderNative,Func<ITwainHelperChannel> funcChannel,Func<bool> funcIs32BitDsmPresent,bool is64BitProcess) {
			_dsmLoaderNative=dsmLoaderNative;
			_funcChannel=funcChannel;
			_funcIs32BitDsmPresent=funcIs32BitDsmPresent??(() => false);
			_is64BitProcess=is64BitProcess;
		}

		private static ITwainDsm LoadNativeDsm(out string errorText) {
			return NativeTwainDsm.Load(out errorText);
		}

		///<summary>The files the 32-bit helper's DSM loads from, in the order a 32-bit process tries them (NativeTwainDsm.GetCandidatePaths(false)
		///as the helper sees them): SysWOW64\TWAINDSM.dll (the setup's), the legacy twain_32.dll of the Windows folder. The copy beside the helper is
		///not looked for: no release puts one there.</summary>
		public static List<string> Get32BitDsmPaths() {
			string folderSystem32Bit=Environment.Is64BitOperatingSystem ? Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)
				: Environment.SystemDirectory;
			string windowsDir=Environment.GetEnvironmentVariable("windir");
			if(string.IsNullOrEmpty(windowsDir)) {
				windowsDir=Path.GetDirectoryName(Environment.SystemDirectory);
			}
			return new List<string> { Path.Combine(folderSystem32Bit,"TWAINDSM.dll"),Path.Combine(windowsDir,"twain_32.dll") };
		}

		///<summary>A 32-bit DSM is installed on this computer (Get32BitDsmPaths).</summary>
		public static bool Is32BitDsmInstalled() {
			return Get32BitDsmPaths().Any(File.Exists);
		}

		///<summary>The 32-bit helper path can be used here: a 64-bit process with a helper channel.</summary>
		public bool CanUseHelper => _is64BitProcess && _funcChannel!=null;

		#region Listing
		///<summary>The sources of the program's own DSM, by ProductName (each once). Empty with errorText when it cannot be loaded or opened.</summary>
		public List<TwainSourceEntry> ListNative(out string errorText) {
			errorText="";
			List<TwainSourceEntry> listSources=new List<TwainSourceEntry>();
			ITwainDsm dsm;
			try {
				dsm=_dsmLoaderNative(out errorText);
			}
			catch(Exception ex) {
				errorText=ex.Message;
				return listSources;
			}
			if(dsm==null) {
				return listSources;
			}
			string library=dsm.LibraryName??"";
			using(TwainSession session=new TwainSession((out string error) => {
				error="";
				return dsm;
			}))
			{
				List<TW_IDENTITY> listIdentities=session.GetSources();
				if(listIdentities==null) {
					errorText=session.ErrorText;
					return listSources;
				}
				foreach(TW_IDENTITY identity in listIdentities) {
					string productName=(identity.ProductName??"").Trim();
					if(productName=="" || listSources.Any(x => string.Equals(x.ProductName,productName,StringComparison.OrdinalIgnoreCase))) {
						continue;
					}
					listSources.Add(new TwainSourceEntry { ProductName=productName,Manufacturer=identity.Manufacturer??"",ProductFamily=identity.ProductFamily??"",
						VersionMajor=identity.Version.MajorNum,VersionMinor=identity.Version.MinorNum,VersionInfo=identity.Version.Info??"",
						Dsm=TwainDsmKind.Native,DsmLibrary=library });
				}
			}
			return listSources;
		}

		///<summary>The sources of the 32-bit DSM, listed by the helper. Empty with errorText when there is no helper path, no 32-bit DSM, or the
		///helper failed (its message). Throws nothing.</summary>
		public List<TwainSourceEntry> ListHelper32(out string errorText) {
			errorText="";
			if(!CanUseHelper) {
				return new List<TwainSourceEntry>();
			}
			if(!_funcIs32BitDsmPresent()) {
				errorText="No 32-bit TWAIN data source manager is installed (TWAINDSM.dll in the 32-bit system folder).";
				return new List<TwainSourceEntry>();
			}
			try {
				ITwainHelperChannel channel=_funcChannel();
				byte[] dataReply;
				string[] arrayHello=channel.Call(TwainHelperCommands.Hello,new string[0],null,out dataReply,TimeSpanListHelper);
				if(arrayHello==null || arrayHello.Length<1 || arrayHello[0]!=TwainHelperCommands.VERSION.ToString(CultureInfo.InvariantCulture)) {
					errorText="The 32-bit helper's TWAIN module is another version ("+(arrayHello==null || arrayHello.Length<1 ? "none" : arrayHello[0])
						+", expected "+TwainHelperCommands.VERSION+"). Update the program again.";
					return new List<TwainSourceEntry>();
				}
				string[] arrayResults=channel.Call(TwainHelperCommands.ListSources,new string[0],null,out dataReply,TimeSpanListHelper);
				if(arrayResults!=null && arrayResults.Length>1) {
					errorText=arrayResults[1]??"";
				}
				return TwainHelperWire.Read(dataReply,TwainHelperWire.ReadSources)??new List<TwainSourceEntry>();
			}
			catch(TwainHelperException ex) {
				errorText="The 32-bit helper could not list the 32-bit TWAIN sources: "+ex.Message;
			}
			catch(InvalidDataException ex) {
				errorText="The 32-bit helper's list of TWAIN sources could not be read: "+ex.Message;
			}
			return new List<TwainSourceEntry>();
		}

		///<summary>Every source a device can be set to, native first: the program's own DSM's, then the 32-bit DSM's that the program's does not list
		///(by ProductName, ignoring case). Each entry's StoredText is what to store. errorText collects the problems of either listing ("" when none).
		///</summary>
		public List<TwainSourceEntry> ListAll(out string errorText) {
			string errorNative;
			List<TwainSourceEntry> listSources=ListNative(out errorNative);
			string error32="";
			if(CanUseHelper) {
				foreach(TwainSourceEntry entry in ListHelper32(out error32)) {
					if(!listSources.Any(x => string.Equals(x.ProductName,entry.ProductName,StringComparison.OrdinalIgnoreCase))) {
						listSources.Add(entry);
					}
				}
			}
			errorText=string.Join(" ",new[] { errorNative,error32 }.Where(x => !string.IsNullOrWhiteSpace(x)));
			return listSources;
		}
		#endregion Listing

		///<summary>The path for the stored choice: native when the program's DSM lists the source (the whole stored text, then its ProductName
		///without the marker), else the helper when the 32-bit DSM lists it, else native (the session reports the source not found, as before).
		///</summary>
		public TwainSourceResolution Resolve(string storedText) {
			TwainSourceChoice choice=TwainSourceChoice.Parse(storedText);
			TwainSourceResolution resolution=new TwainSourceResolution { Dsm=TwainDsmKind.Native,ProductName=choice.ProductName };
			if(!CanUseHelper) {
				resolution.Reason=_is64BitProcess ? "no 32-bit helper here" : "a 32-bit program runs every TWAIN source in its own process";
				return resolution;
			}
			string errorNative;
			List<TwainSourceEntry> listNative=ListNative(out errorNative);
			TwainSourceEntry entryNative=listNative.FirstOrDefault(x => string.Equals(x.ProductName,choice.StoredText,StringComparison.OrdinalIgnoreCase))
				??listNative.FirstOrDefault(x => string.Equals(x.ProductName,choice.ProductName,StringComparison.OrdinalIgnoreCase));
			if(entryNative!=null) {
				resolution.ProductName=entryNative.ProductName;
				resolution.Reason="the 64-bit TWAIN data source manager lists \""+entryNative.ProductName+"\" (native first)"
					+(choice.Dsm==TwainDsmKind.Helper32 ? "; it was chosen from the 32-bit one, and has a 64-bit driver now" : "");
				return resolution;
			}
			string error32;
			List<TwainSourceEntry> list32=ListHelper32(out error32);
			TwainSourceEntry entry32=list32.FirstOrDefault(x => string.Equals(x.ProductName,choice.ProductName,StringComparison.OrdinalIgnoreCase));
			if(entry32!=null) {
				resolution.Dsm=TwainDsmKind.Helper32;
				resolution.ProductName=entry32.ProductName;
				resolution.Reason="only the 32-bit TWAIN data source manager ("+entry32.DsmLibrary+") lists \""+entry32.ProductName+"\"; it runs in the "
					+"32-bit helper";
				return resolution;
			}
			if(choice.Dsm==TwainDsmKind.Helper32) {
				//Chosen from the 32-bit DSM, which does not list it now: say why on the 32-bit side.
				resolution.Dsm=TwainDsmKind.Helper32;
				resolution.ErrorText=error32!="" ? error32 : "The TWAIN source \""+choice.ProductName+"\" is not installed (its 32-bit driver is not "
					+"listed by the 32-bit TWAIN data source manager).";
				resolution.Reason="chosen from the 32-bit TWAIN data source manager, which does not list it";
				return resolution;
			}
			resolution.Reason="no TWAIN data source manager lists \""+choice.ProductName+"\""+(error32=="" ? "" : " ("+error32+")");
			return resolution;
		}
	}
}
