// The check of a plug-in DLL before it is loaded (N64-A of the .NET 10 move, docs/port-notes/N64-A.md; item A2 of the native 64-bit
// integration survey, docs/specs/native64-survey.md, row 10). A plug-in runs in the program's process (its hooks get live forms and objects),
// so it must be a .NET assembly of a bitness and a target framework this process can load. Both loaders (PluginLoader.LoadAllPlugins in
// WpfControlsOD for the program, Plugins.LoadAllPlugins for the Middle Tier) go through Plugins.LoadPlugin, which asks GetRefusalReason first
// and gives a plug-in it refuses, or one that fails to load, a clear reason naming it, written to the log (Logger, sub folder Plugins).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using CodeBase;

namespace OpenPmsDentalBusiness {
	///<summary>Whether this process can load a plug-in DLL, from the file's PE header and metadata (CodeBase.PortableExecutableFile), without
	///loading it.</summary>
	public static class PluginImageCheck {
		public const string FRAMEWORK_NET_FRAMEWORK=".NETFramework";
		public const string FRAMEWORK_NET_STANDARD=".NETStandard";
		public const string FRAMEWORK_NET_CORE_APP=".NETCoreApp";

		///<summary>The runtime of this process: ".NETFramework" with version 4.x on the .NET Framework, ".NETCoreApp" with the runtime's
		///version (for example 10.0) on .NET. Environment.Version is 4.0.30319.x on every .NET Framework 4 and the real version on .NET 5 and
		///later.</summary>
		public static void GetProcessFramework(out string framework,out Version version) {
			Version versionEnvironment=Environment.Version;
			if(versionEnvironment.Major>=5) {
				framework=FRAMEWORK_NET_CORE_APP;
				version=new Version(versionEnvironment.Major,versionEnvironment.Minor);
				return;
			}
			framework=FRAMEWORK_NET_FRAMEWORK;
			version=new Version(4,8);
		}

		///<summary>The framework an assembly was built for, as (identifier, version): from its TargetFrameworkAttribute (".NETFramework,
		///Version=v4.8" gives ".NETFramework" and 4.8), or when it has none from the core library it references (mscorlib: .NET Framework with its
		///metadata version, netstandard: .NET Standard of that version, System.Runtime 5 or later: .NET of that version). ("",null) when neither
		///tells (System.Runtime 4.x is both .NET Core and .NET Standard 1.x).</summary>
		public static void GetTargetFramework(PortableExecutableFile portableExecutableFile,out string framework,out Version version) {
			framework="";
			version=null;
			string targetFramework=portableExecutableFile.TargetFramework;
			if(!string.IsNullOrEmpty(targetFramework)) {
				string[] arrayParts=targetFramework.Split(',');
				framework=arrayParts[0].Trim();
				for(int i=1;i<arrayParts.Length;i++) {
					string part=arrayParts[i].Trim();
					if(!part.StartsWith("Version=",StringComparison.OrdinalIgnoreCase)) {
						continue;
					}
					Version.TryParse(part.Substring("Version=".Length).TrimStart('v','V'),out version);
				}
				return;
			}
			Version versionCore=portableExecutableFile.CoreLibraryVersion;
			switch(portableExecutableFile.CoreLibraryName) {
				case "mscorlib":
					framework=FRAMEWORK_NET_FRAMEWORK;
					version=portableExecutableFile.MetadataVersion.StartsWith("v2.") ? new Version(2,0) : new Version(4,0);
					return;
				case "netstandard":
					framework=FRAMEWORK_NET_STANDARD;
					version=versionCore==null ? null : new Version(versionCore.Major,versionCore.Minor);
					return;
				case "System.Runtime":
				case "System.Private.CoreLib":
					if(versionCore!=null && versionCore.Major>=5) {
						framework=FRAMEWORK_NET_CORE_APP;
						version=new Version(versionCore.Major,versionCore.Minor);
					}
					return;
			}
		}

		///<summary>The framework as the messages name it: ".NET Framework 4.8", ".NET Standard 2.0", ".NET 10.0", ".NET Core 3.1".</summary>
		public static string GetFrameworkDisplayName(string framework,Version version) {
			string versionText=(version==null) ? "" : " "+version.ToString(2);
			switch(framework) {
				case FRAMEWORK_NET_FRAMEWORK:
					return ".NET Framework"+versionText;
				case FRAMEWORK_NET_STANDARD:
					return ".NET Standard"+versionText;
				case FRAMEWORK_NET_CORE_APP:
					return ((version!=null && version.Major<5) ? ".NET Core" : ".NET")+versionText;
				default:
					return framework+versionText;
			}
		}

		///<summary>Null when this process can load the plug-in DLL at dllPath, or a clear reason, naming the plug-in, why it is refused. Reads the
		///file only. See the other overload.</summary>
		public static string GetRefusalReason(string dllPath,string pluginName) {
			GetProcessFramework(out string frameworkProcess,out Version versionProcess);
			return GetRefusalReason(dllPath,pluginName,PortableExecutableFile.GetProcessMachine(),frameworkProcess,versionProcess);
		}

		///<summary>Null when a process of machine type machineProcess (PortableExecutableFile.IMAGE_FILE_MACHINE_...) on frameworkProcess
		///versionProcess can load the plug-in DLL at dllPath, or a clear reason, naming the plug-in, why it is refused:
		///- not a PE file, or a native DLL: not a .NET plug-in;
		///- a .NET assembly the process cannot map (PortableExecutableFile.CanLoadInProcess): 32BITREQUIRED without 32BITPREFERRED (platform
		///  x86) or mixed-mode x86 in a 64-bit process, x64 (PE32+) in a 32-bit one;
		///- a framework the process cannot host: on the .NET Framework, .NET (Core) and .NET Standard 2.1 or later; on .NET, the .NET Framework
		///  (decision 16 of docs/specs/net10-plan.md) and a .NET newer than the process's.
		///An assembly whose framework the file does not tell is not refused for it (it loads as before). A file that cannot be read is not refused
		///either: Assembly.LoadFile then gives its own error, as before.</summary>
		public static string GetRefusalReason(string dllPath,string pluginName,ushort machineProcess,string frameworkProcess,Version versionProcess) {
			PortableExecutableFile portableExecutableFile;
			try {
				portableExecutableFile=PortableExecutableFile.Read(dllPath);
			}
			catch(Exception ex) {
				ex.DoNothing();//LoadFile reports a file that cannot be opened, as before
				return null;
			}
			string namePlugin="The plug-in "+pluginName+" ("+dllPath+")";
			string bitnessProcess=GetProcessBitnessText(machineProcess);
			if(!portableExecutableFile.IsPe) {
				return namePlugin+" is not loaded: it is not a program library (the file has no PE header). Check the plug-in's file name in the program "
					+"link's setup.";
			}
			if(!portableExecutableFile.IsManaged) {
				return namePlugin+" is not loaded: it is "+portableExecutableFile.DescribeBitness()+", not a .NET plug-in assembly. Check the plug-in's "
					+"file name in the program link's setup.";
			}
			if(!portableExecutableFile.CanLoadInProcess(machineProcess)) {
				string rebuild=(machineProcess==PortableExecutableFile.IMAGE_FILE_MACHINE_I386)
					? "Ask its author for a build for any CPU (AnyCPU) or x86."
					: "Ask its author for a build for any CPU (AnyCPU) or "+PortableExecutableFile.GetMachineName(machineProcess)+".";
				string only=(portableExecutableFile.Machine==PortableExecutableFile.IMAGE_FILE_MACHINE_I386) ? "32-bit only"
					: (portableExecutableFile.IsPe32Plus ? "64-bit only" : PortableExecutableFile.GetMachineName(portableExecutableFile.Machine)+" only");
				return namePlugin+" is not loaded: it is "+portableExecutableFile.DescribeBitness()+" ("+only+"), and this program is a "+bitnessProcess
					+" process. "+rebuild;
			}
			GetTargetFramework(portableExecutableFile,out string framework,out Version version);
			string frameworkName=GetFrameworkDisplayName(framework,version);
			string frameworkProcessName=GetFrameworkDisplayName(frameworkProcess,versionProcess);
			if(frameworkProcess==FRAMEWORK_NET_FRAMEWORK) {
				if(framework==FRAMEWORK_NET_CORE_APP) {
					return namePlugin+" is not loaded: it is built for "+frameworkName+", and this program runs on "+frameworkProcessName+". Use a build of "
						+"the plug-in for the .NET Framework (net48).";
				}
				if(framework==FRAMEWORK_NET_STANDARD && version!=null && version>new Version(2,0)) {
					return namePlugin+" is not loaded: it is built for "+frameworkName+", which the .NET Framework cannot load (it loads .NET Standard 2.0 and "
						+"earlier). Use a build of the plug-in for the .NET Framework (net48) or .NET Standard 2.0.";
				}
				return null;
			}
			if(framework==FRAMEWORK_NET_FRAMEWORK) {
				//Decision 16 of docs/specs/net10-plan.md: plug-ins built for the .NET Framework are not supported on .NET.
				return namePlugin+" is not loaded: it is built for "+frameworkName+", and this version of the program runs on "+frameworkProcessName
					+", which does not load .NET Framework plug-ins. Ask its author for a build for "+GetFrameworkDisplayName(FRAMEWORK_NET_CORE_APP,
					versionProcess)+" (net"+versionProcess.ToString(2)+"-windows, any CPU or x64).";
			}
			if(framework==FRAMEWORK_NET_CORE_APP && version!=null && versionProcess!=null && version>versionProcess) {
				return namePlugin+" is not loaded: it is built for "+frameworkName+", newer than the "+frameworkProcessName+" this program runs on. Use a "
					+"build of the plug-in for "+frameworkProcessName+" or earlier.";
			}
			return null;
		}

		///<summary>The clear text for an exception from loading the plug-in (Assembly.LoadFile, its type and its constructor), naming it: for a
		///BadImageFormatException (a file this process cannot load, or a library it needs that is) and a FileLoadException (a file that was found
		///but could not be loaded: blocked by Windows, loaded from a network folder, or a library it needs of another version), what it means and
		///what to do; for any other exception its message, as the loader showed before.</summary>
		public static string GetLoadErrorText(Exception ex,string pluginName,string dllPath) {
			Exception exceptionInner=ex;
			while(exceptionInner is System.Reflection.TargetInvocationException && exceptionInner.InnerException!=null) {
				exceptionInner=exceptionInner.InnerException;
			}
			string namePlugin="The plug-in "+pluginName+" ("+dllPath+")";
			if(exceptionInner is BadImageFormatException) {
				return namePlugin+" could not be loaded: it, or a library it needs, is not a .NET assembly that this "
					+GetProcessBitnessText(PortableExecutableFile.GetProcessMachine())+" program can load. Ask its author for a build for any CPU "
					+"(AnyCPU) or for this program's bitness. ("+exceptionInner.GetType().Name+": "+exceptionInner.Message+")";
			}
			if(exceptionInner is FileLoadException) {
				return namePlugin+" could not be loaded: the file, or a library it needs, was found but could not be loaded. A DLL copied from the "
					+"internet or another computer may be blocked by Windows (the file's Properties, Unblock); one in a network folder may not be "
					+"trusted; or a library it needs is of another version. ("+exceptionInner.GetType().Name+": "+exceptionInner.Message+")";
			}
			return ex.Message;
		}

		///<summary>"32-bit", "64-bit" or "ARM64" for the messages.</summary>
		private static string GetProcessBitnessText(ushort machineProcess) {
			switch(machineProcess) {
				case PortableExecutableFile.IMAGE_FILE_MACHINE_I386:
					return "32-bit";
				case PortableExecutableFile.IMAGE_FILE_MACHINE_AMD64:
					return "64-bit";
				default:
					return PortableExecutableFile.GetMachineName(machineProcess);
			}
		}
	}
}
