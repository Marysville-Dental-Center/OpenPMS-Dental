// Shared helpers for the library binding tests: repository paths, assembly metadata read with Mono.Cecil (no file is loaded or locked), the
// library files (those of "Required dlls" and, since S2-4 of the .NET 10 move, the NuGet packages' own files at the lock files' versions), the
// binding redirects of a config file, the .NET Framework's global assembly cache, and the projects of OpenDental.Local.slnf.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using ProjectFileTests;

namespace LibraryBindingTests {
	internal static class RepoPaths {
		private static string _root;

		///<summary>The repository root: the first folder above the test assembly that holds LICENSE and NOTICE (not found by the solution's name,
		///which a rename may change; package REN-PROOF).</summary>
		public static string Root {
			get {
				if(_root!=null) {
					return _root;
				}
				DirectoryInfo directoryInfo=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
				while(directoryInfo!=null) {
					if(File.Exists(Path.Combine(directoryInfo.FullName,"LICENSE")) && File.Exists(Path.Combine(directoryInfo.FullName,"NOTICE"))) {
						_root=directoryInfo.FullName.TrimEnd('\\');
						return _root;
					}
					directoryInfo=directoryInfo.Parent;
				}
				throw new InvalidOperationException("LICENSE and NOTICE not found above "+AppDomain.CurrentDomain.BaseDirectory);
			}
		}

		public static string RequiredDlls {
			get { return Path.Combine(Root,"Required dlls"); }
		}

		public static string Combine(params string[] arrayParts) {
			return Path.Combine(Root,Path.Combine(arrayParts));
		}

		///<summary>The path relative to the repository root, for messages.</summary>
		public static string Relative(string path) {
			string full=Path.GetFullPath(path);
			if(full.StartsWith(Root+"\\",StringComparison.OrdinalIgnoreCase)) {
				return full.Substring(Root.Length+1);
			}
			return full;
		}

		public static string ReadText(params string[] arrayParts) {
			return File.ReadAllText(Combine(arrayParts));
		}
	}

	///<summary>A reference from one assembly to another, as its metadata names it.</summary>
	internal class AssemblyReference {
		public string Name;
		public Version Version;
		///<summary>The public key token in lower-case hex; "" for an assembly without a strong name.</summary>
		public string Token;
		///<summary>A Windows Runtime metadata reference (Windows.winmd), which Windows itself resolves at run time.</summary>
		public bool IsWindowsRuntime;
	}

	///<summary>What a file's metadata says about it. IsManaged is false for a native file, which has no assembly identity.</summary>
	internal class AssemblyFacts {
		public string Path;
		public string FileName;
		public bool IsManaged;
		public string Name;
		public Version Version;
		public string Token;
		public List<AssemblyReference> ListReferences=new List<AssemblyReference>();
	}

	internal static class MetadataReader {
		///<summary>Keyed by path, length and last write time, so the many identical copies in the output folders are read once per run.</summary>
		private static readonly Dictionary<string,AssemblyFacts> _dictCache=new Dictionary<string,AssemblyFacts>(StringComparer.OrdinalIgnoreCase);
		private static readonly Dictionary<string,string> _dictHashes=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);

		private static string Key(string path) {
			FileInfo fileInfo=new FileInfo(path);
			return fileInfo.FullName+"|"+fileInfo.Length+"|"+fileInfo.LastWriteTimeUtc.Ticks;
		}

		public static string FormatToken(byte[] arrayToken) {
			if(arrayToken==null || arrayToken.Length==0) {
				return "";
			}
			return string.Concat(arrayToken.Select(x => x.ToString("x2")));
		}

		///<summary>The file's assembly identity and references, read from a copy of its bytes so the file stays unlocked. Nothing is loaded.</summary>
		public static AssemblyFacts Read(string path) {
			string key=Key(path);
			if(_dictCache.TryGetValue(key,out AssemblyFacts assemblyFactsCached)) {
				return assemblyFactsCached;
			}
			AssemblyFacts assemblyFacts=new AssemblyFacts();
			assemblyFacts.Path=path;
			assemblyFacts.FileName=System.IO.Path.GetFileName(path);
			try {
				using(MemoryStream memoryStream=new MemoryStream(File.ReadAllBytes(path)))
				using(ModuleDefinition moduleDefinition=ModuleDefinition.ReadModule(memoryStream)) {
					if(moduleDefinition.Assembly!=null) {
						assemblyFacts.IsManaged=true;
						assemblyFacts.Name=moduleDefinition.Assembly.Name.Name;
						assemblyFacts.Version=moduleDefinition.Assembly.Name.Version;
						assemblyFacts.Token=FormatToken(moduleDefinition.Assembly.Name.PublicKeyToken);
						foreach(AssemblyNameReference assemblyNameReference in moduleDefinition.AssemblyReferences) {
							assemblyFacts.ListReferences.Add(new AssemblyReference {
								Name=assemblyNameReference.Name,
								Version=assemblyNameReference.Version,
								Token=FormatToken(assemblyNameReference.PublicKeyToken),
								IsWindowsRuntime=assemblyNameReference.IsWindowsRuntime,
							});
						}
					}
				}
			}
			catch(BadImageFormatException) {
				//A native file: no metadata.
			}
			_dictCache[key]=assemblyFacts;
			return assemblyFacts;
		}

		///<summary>The managed .dll and .exe files directly in the folder (not its sub folders), by assembly name, ignoring case.</summary>
		public static Dictionary<string,AssemblyFacts> ReadFolder(string folder) {
			Dictionary<string,AssemblyFacts> dictAssemblies=new Dictionary<string,AssemblyFacts>(StringComparer.OrdinalIgnoreCase);
			foreach(string path in Directory.GetFiles(folder).Where(x => IsBinary(x)).OrderBy(x => x,StringComparer.OrdinalIgnoreCase)) {
				AssemblyFacts assemblyFacts=Read(path);
				if(assemblyFacts.IsManaged && !dictAssemblies.ContainsKey(assemblyFacts.Name)) {
					dictAssemblies.Add(assemblyFacts.Name,assemblyFacts);
				}
			}
			return dictAssemblies;
		}

		public static bool IsBinary(string path) {
			string extension=System.IO.Path.GetExtension(path);
			return extension.Equals(".dll",StringComparison.OrdinalIgnoreCase) || extension.Equals(".exe",StringComparison.OrdinalIgnoreCase);
		}

		public static string Sha256(string path) {
			string key=Key(path);
			if(_dictHashes.TryGetValue(key,out string hash)) {
				return hash;
			}
			using(SHA256 sha256=SHA256.Create()) {
				hash=string.Concat(sha256.ComputeHash(File.ReadAllBytes(path)).Select(x => x.ToString("x2")));
			}
			_dictHashes[key]=hash;
			return hash;
		}
	}

	///<summary>The files of the "Required dlls" folder: since S2-4 of the .NET 10 move only the third-party files that have no NuGet package (the
	///plan's "Left in Required dlls after stage 2"). Every file by name, ignoring case.</summary>
	internal static class RequiredDlls {
		private static Dictionary<string,string> _dictFiles;

		///<summary>File name to full path, ignoring case.</summary>
		public static Dictionary<string,string> Files {
			get {
				if(_dictFiles==null) {
					_dictFiles=Directory.GetFiles(RepoPaths.RequiredDlls).ToDictionary(x => Path.GetFileName(x),x => x,StringComparer.OrdinalIgnoreCase);
				}
				return _dictFiles;
			}
		}
	}

	///<summary>The third-party library files the projects build with and the programs ship: the files of "Required dlls" and, since S2-4 of the
	///.NET 10 move, which deleted them from "Required dlls", the files of the NuGet packages of the lock files (PackageFiles), each the package's
	///own file at the one version every lock file of the solution filter names, in the offline package folder (or the global packages folder the
	///program's restore used, NuGetFiles.FindPackageFolder). Every file by name, and the managed ones by assembly name. Without the packages (no
	///offline folder and no restore yet) a check that needs them is inconclusive, a failure with OPMS_REQUIRE_BUILD_OUTPUT=1.</summary>
	internal static class Libraries {
		private static Dictionary<string,string> _dictFiles;
		private static Dictionary<string,AssemblyFacts> _dictAssemblies;
		private static Dictionary<string,string> _dictLockedVersions;
		private static Dictionary<string,string> _dictPackagesOfFiles;

		///<summary>File name to full path, ignoring case: the files of "Required dlls" and those of the packages.</summary>
		public static Dictionary<string,string> Files {
			get {
				if(_dictFiles==null) {
					Load();
				}
				return _dictFiles;
			}
		}

		///<summary>Assembly name to its facts, ignoring case.</summary>
		public static Dictionary<string,AssemblyFacts> Assemblies {
			get {
				if(_dictAssemblies==null) {
					Dictionary<string,AssemblyFacts> dictAssemblies=new Dictionary<string,AssemblyFacts>(StringComparer.OrdinalIgnoreCase);
					foreach(string path in Files.Values.Where(x => MetadataReader.IsBinary(x)).OrderBy(x => Path.GetFileName(x),StringComparer.OrdinalIgnoreCase)) {
						AssemblyFacts assemblyFacts=MetadataReader.Read(path);
						if(assemblyFacts.IsManaged && !dictAssemblies.ContainsKey(assemblyFacts.Name)) {
							dictAssemblies.Add(assemblyFacts.Name,assemblyFacts);
						}
					}
					_dictAssemblies=dictAssemblies;
				}
				return _dictAssemblies;
			}
		}

		///<summary>The managed library file with this file name, or null.</summary>
		public static AssemblyFacts ByFileName(string fileName) {
			return Assemblies.Values.FirstOrDefault(x => x.FileName.Equals(fileName,StringComparison.OrdinalIgnoreCase));
		}

		///<summary>Package id (ignoring case) to the one version the lock files of the solution filter name for it. Throws for a package two lock files
		///name at different versions (central package management pins one).</summary>
		public static Dictionary<string,string> LockedVersions {
			get {
				if(_dictLockedVersions!=null) {
					return _dictLockedVersions;
				}
				Dictionary<string,string> dictVersions=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
				Dictionary<string,string> dictWhere=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
				foreach(string pathProject in SolutionProjects.ProjectPaths) {
					string pathLock=Path.Combine(Path.GetDirectoryName(pathProject),"packages.lock.json");
					if(!File.Exists(pathLock)) {
						continue;
					}
					foreach(LockedPackage lockedPackage in NuGetFiles.ReadLockFile(pathLock)) {
						string version;
						if(!dictVersions.TryGetValue(lockedPackage.Id,out version)) {
							dictVersions[lockedPackage.Id]=lockedPackage.Version;
							dictWhere[lockedPackage.Id]=RepoPaths.Relative(pathLock);
						}
						else if(!version.Equals(lockedPackage.Version,StringComparison.OrdinalIgnoreCase)) {
							throw new InvalidOperationException(lockedPackage.Id+" is "+version+" in "+dictWhere[lockedPackage.Id]+" but "+lockedPackage.Version+" in "
								+RepoPaths.Relative(pathLock)+": the program and its tests must get one version.");
						}
					}
				}
				_dictLockedVersions=dictVersions;
				return _dictLockedVersions;
			}
		}

		///<summary>The package id a library file comes from, or null for a file of "Required dlls" (or no library file).</summary>
		public static string PackageOf(string fileName) {
			if(_dictPackagesOfFiles==null) {
				Load();
			}
			string packageId;
			return _dictPackagesOfFiles.TryGetValue(fileName,out packageId) ? packageId : null;
		}

		///<summary>The folder of the package at its locked version: the offline folder's, else the global packages folder of the program's restore;
		///ends the test as not built when neither is there (no offline folder and no restore yet), and fails it when the offline folder or the
		///restore is there but holds no such package.</summary>
		public static string GetLockedPackageFolder(string packageId) {
			string version=LockedVersions[packageId];
			string pathAssets=RepoPaths.Combine("OpenPmsDental","obj","project.assets.json");
			string folderPackage=NuGetFiles.FindPackageFolder(RepoPaths.Root,packageId,version,pathAssets);
			if(folderPackage!=null) {
				return folderPackage;
			}
			if(NuGetFiles.GetOfflineFolder(RepoPaths.Root)==null && !File.Exists(pathAssets)) {
				BuildOutput.NotBuilt("The package files are not there: no offline package folder (OPMS_NUGET_OFFLINE, or od-tools\\nuget-packages in the common "
					+"git folder) and no restore of OpenPmsDental to name the global packages folder. Build OpenDental.Local.slnf first.");
			}
			Assert.Fail(packageId+" "+version+", which the lock files name, is neither in the offline folder nor in the program's global packages folder.");
			return null;
		}

		///<summary>The files of "Required dlls" and of the mapped packages of the lock files; a file both would bring fails, so no file has two
		///sources.</summary>
		private static void Load() {
			Dictionary<string,string> dictFiles=new Dictionary<string,string>(RequiredDlls.Files,StringComparer.OrdinalIgnoreCase);
			Dictionary<string,string> dictPackagesOfFiles=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
			foreach(KeyValuePair<string,string> pair in LockedVersions.OrderBy(x => x.Key,StringComparer.OrdinalIgnoreCase)) {
				string[] arrayAssets=PackageFiles.GetAssets(pair.Key,"the lock files");
				if(arrayAssets.Length==0) {
					continue;
				}
				string folderPackage=GetLockedPackageFolder(pair.Key);
				foreach(string asset in arrayAssets) {
					string fileName=asset.Substring(asset.LastIndexOf('/')+1);
					if(dictFiles.ContainsKey(fileName)) {
						throw new InvalidOperationException(fileName+" comes from "+pair.Key+" "+pair.Value+" and from "
							+(dictPackagesOfFiles.ContainsKey(fileName) ? dictPackagesOfFiles[fileName] : "\"Required dlls\"")+": one source per file.");
					}
					dictFiles[fileName]=Path.Combine(folderPackage,asset.Replace('/','\\'));
					dictPackagesOfFiles[fileName]=pair.Key;
				}
			}
			_dictPackagesOfFiles=dictPackagesOfFiles;
			_dictFiles=dictFiles;
		}
	}

	///<summary>The files each package of the lock files brings into the output of an exe or test project that gets it (NuGet's assets of the
	///project's win-x64 target, which the build copies flat; a library's own build copies none on .NET), as assets of the package: the paths in
	///the package's folder, with '/'. Since S3b-5 of the .NET 10 move these are the .NET 10 assets: lib\net10.0 (or the nearest .NET one), and
	///runtimes\win\lib\net10.0 where a package has a Windows implementation (System.Management and the like), the file the win-x64 target picks
	///in place of the lib folder's PlatformNotSupported stub; the support packages of the .NET Framework build (System.Memory and the like) are
	///part of the shared framework and pruned from every lock file. Named per package, not made from its id: a package's files need not carry
	///its id (SSH.NET brings Renci.SshNet.dll; Microsoft.Web.WebView2 the native WebView2Loader.dll, its assemblies come from its build files'
	///lib_manual references, which no lock file names). GetAssets throws for a package in neither list, so a package that a later package adds
	///fails every check that reads the projects until it is mapped here, instead of dropping out of the binding checks. RequiredDllsTests
	///checks every mapping against the assets the restore picks, and each file of the package at its locked version.</summary>
	internal static class PackageFiles {
		///<summary>Package id (NuGet's ids ignore case) to its assets, at least one each.</summary>
		private static readonly Dictionary<string,string[]> _dictAssets=new Dictionary<string,string[]>(StringComparer.OrdinalIgnoreCase) {
			{ "BouncyCastle.Cryptography",new[] { "lib/net6.0/BouncyCastle.Cryptography.dll" } },
			{ "Google.Apis",new[] { "lib/net6.0/Google.Apis.dll" } },
			{ "Google.Apis.Auth",new[] { "lib/net6.0/Google.Apis.Auth.dll" } },
			{ "Google.Apis.Core",new[] { "lib/net6.0/Google.Apis.Core.dll" } },
			{ "Google.Apis.Gmail.v1",new[] { "lib/net6.0/Google.Apis.Gmail.v1.dll" } },
			{ "MailKit",new[] { "lib/net10.0/MailKit.dll" } },
			{ "MimeKit",new[] { "lib/net10.0/MimeKit.dll" } },
			{ "Microsoft.Identity.Client",new[] { "lib/net8.0/Microsoft.Identity.Client.dll" } },
			{ "Microsoft.IdentityModel.Abstractions",new[] { "lib/net9.0/Microsoft.IdentityModel.Abstractions.dll" } },
			//The WebView2 SDK, 1.0.4314.35 on .NET 10: the native x64 loader (runtimes/win-x64/native), which Directory.Build.targets lands flat;
			//its three assemblies come from the package's build files (lib_manual), not from the restore's assets (S3b-0 item 5).
			{ "Microsoft.Web.WebView2",new[] { "runtimes/win-x64/native/WebView2Loader.dll" } },
			{ "MySqlConnector",new[] { "lib/net10.0/MySqlConnector.dll" } },
			{ "Newtonsoft.Json",new[] { "lib/net6.0/Newtonsoft.Json.dll" } },
			{ "SSH.NET",new[] { "lib/net10.0/Renci.SshNet.dll" } },
			{ "Microsoft.Extensions.DependencyInjection.Abstractions",new[] { "lib/net10.0/Microsoft.Extensions.DependencyInjection.Abstractions.dll" } },
			{ "Microsoft.Extensions.Logging.Abstractions",new[] { "lib/net10.0/Microsoft.Extensions.Logging.Abstractions.dll" } },
			//The Middle Tier server's Windows service support (S3b-6), which MiddleTierHostTests gets through the server.
			{ "Microsoft.Extensions.Hosting.WindowsServices",new[] { "lib/net10.0/Microsoft.Extensions.Hosting.WindowsServices.dll" } },
			//Roslyn, which RdlEngine compiles a report's <Code> block with (S3b-2b, decision 14).
			{ "Microsoft.CodeAnalysis.Common",new[] { "lib/net10.0/Microsoft.CodeAnalysis.dll" } },
			{ "Microsoft.CodeAnalysis.VisualBasic",new[] { "lib/net10.0/Microsoft.CodeAnalysis.VisualBasic.dll" } },
			//The .NET Framework assemblies that are packages on .NET (S3b-1, batch D), with their Windows implementations.
			{ "System.CodeDom",new[] { "lib/net10.0/System.CodeDom.dll" } },
			{ "System.Diagnostics.EventLog",new[] { "runtimes/win/lib/net10.0/System.Diagnostics.EventLog.dll",
				"runtimes/win/lib/net10.0/System.Diagnostics.EventLog.Messages.dll" } },
			{ "System.DirectoryServices.AccountManagement",new[] { "runtimes/win/lib/net10.0/System.DirectoryServices.AccountManagement.dll" } },
			{ "System.DirectoryServices.Protocols",new[] { "runtimes/win/lib/net10.0/System.DirectoryServices.Protocols.dll" } },
			{ "System.IO.Ports",new[] { "runtimes/win/lib/net10.0/System.IO.Ports.dll" } },
			{ "System.Management",new[] { "runtimes/win/lib/net10.0/System.Management.dll" } },
			{ "System.Security.Cryptography.ProtectedData",new[] { "lib/net10.0/System.Security.Cryptography.ProtectedData.dll" } },
			{ "System.ServiceProcess.ServiceController",new[] { "runtimes/win/lib/net10.0/System.ServiceProcess.ServiceController.dll" } },
			{ "System.Speech",new[] { "runtimes/win/lib/net10.0/System.Speech.dll" } },
			//The community port of the Windows Forms chart control (decision 10).
			{ "WinForms.DataVisualization",new[] { "lib/net9.0-windows7.0/WinForms.DataVisualization.dll",
				"lib/net9.0-windows7.0/WinForms.DataVisualization.Utilities.dll" } },
			//TC-4: Helix Toolkit 3.1.2 for the WPF tooth chart (SparksToothChart), and the packages of its closure that bring a file the build keeps.
			{ "HelixToolkit",new[] { "lib/net8.0/HelixToolkit.dll" } },
			{ "HelixToolkit.Geometry",new[] { "lib/net8.0/HelixToolkit.Geometry.dll" } },
			{ "HelixToolkit.Maths",new[] { "lib/net8.0/HelixToolkit.Maths.dll" } },
			{ "HelixToolkit.Wpf",new[] { "lib/net8.0-windows7.0/HelixToolkit.Wpf.dll" } },
			{ "CommunityToolkit.Common",new[] { "lib/net8.0/CommunityToolkit.Common.dll" } },
			{ "CommunityToolkit.Diagnostics",new[] { "lib/net8.0/CommunityToolkit.Diagnostics.dll" } },
			{ "CommunityToolkit.Mvvm",new[] { "lib/net8.0/CommunityToolkit.Mvvm.dll" } },
		};

		///<summary>Packages of the lock files that bring no library file, each with its reason. An entry here exempts the package from the binding
		///checks: no file of it is counted for any project that gets it, so nothing of it is checked for its references, redirects or output. A
		///new entry therefore needs the lead's agreement, and its reason is written next to it. RequiredDllsTests checks that the build copies no
		///file of such a package, and that no file in any of its folders has the name of a library file.</summary>
		private static readonly Dictionary<string,string> _dictNoLibraries=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase) {
			{ "Mono.Cecil","the tests' reader of assembly metadata (S1-6), whose files were never in Required dlls" },
			{ "System.ValueTuple","no assembly for .NET Framework 4.7.1 and later (lib\\net471\\_._): the framework's own (SystemValueTuple_ComesFromTheFramework)" },
			//TC-4: System.Net.Http 4.3.4 (HelixToolkit names it for net48) and the packages of the .NET Standard 1.x era it brings. Their files are
			//older versions of .NET Framework 4.8 assemblies, so the build's conflict resolution keeps the framework's and copies none
			//(NuGetFiles.IsReplacedByTheFramework; HelixPackageTests checks every built folder), or they hold only placeholders (_._).
			{ "System.Net.Http","its net46 file (4.1.1.3) is older than the .NET Framework's System.Net.Http 4.2.0.0, which the build keeps (TC-4, risk W2)" },
			{ "System.IO","its net462 file is older than the .NET Framework's facade, which the build keeps (TC-4)" },
			{ "System.IO.Compression","its net46 file is older than the .NET Framework's System.IO.Compression 4.2.0.0, which the build keeps (TC-4)" },
			{ "System.Runtime","its net462 file is older than the .NET Framework's facade, which the build keeps (TC-4)" },
			{ "System.Security.Cryptography.Algorithms","its file is older than the .NET Framework's facade, which the build keeps (TC-4)" },
			{ "System.Security.Cryptography.Encoding","its file is older than the .NET Framework's facade, which the build keeps (TC-4)" },
			{ "System.Security.Cryptography.Primitives","its file is older than the .NET Framework's facade, which the build keeps (TC-4)" },
			{ "System.Security.Cryptography.X509Certificates","its file is older than the .NET Framework's facade, which the build keeps (TC-4)" },
			{ "System.Text.RegularExpressions","its net463 file is older than the .NET Framework's facade, which the build keeps (TC-4)" },
			{ "System.Private.Uri","no assembly for .NET Framework (placeholders only; TC-4)" },
			{ "Microsoft.NETCore.Platforms","no assembly: the runtime graph of the .NET Standard 1.x packages (TC-4)" },
			{ "Microsoft.NETCore.Targets","no assembly: the runtime graph of the .NET Standard 1.x packages (TC-4)" },
			{ "runtime.any.System.IO","no assembly for .NET Framework (lib\\net45\\_._), batch H2 (TC-4)" },
			{ "runtime.any.System.Runtime","no assembly for .NET Framework (lib\\net45\\_._), batch H2 (TC-4)" },
			//S4-1 of the .NET 10 move (docs/port-notes/S4-1.md): MSTest 4 and its closure, batch B. Like Mono.Cecil, test tools of the test projects
			//(and UnitTestsCore) only, which no program references and "Required dlls" never held; vstest loads the adapter next to each test
			//assembly, and the generated .dll.config of each test project binds them.
			{ "MSTest.TestFramework","the test framework (S4-1): test projects only; its assemblies are no library of the programs" },
			{ "MSTest.TestAdapter","the VSTest adapter (S4-1), whose two assemblies its build files reference from buildTransitive\\net462 (the "
				+"MSTest.TestFramework files there are byte for byte those of MSTest.TestFramework's lib\\net462); test projects only" },
			{ "MSTest.Analyzers","Roslyn analyzers only (S4-1)" },
			{ "Microsoft.NET.Test.Sdk","no assembly for .NET Framework (lib\\net462\\_._): build files only (S4-1)" },
			{ "Microsoft.CodeCoverage","the code coverage collector's shim (S4-1), test projects only; the collector's own files (build\\netstandard2.0) "
				+"are copied on publish only (FoldersNotCopied)" },
			{ "Microsoft.TestPlatform.ObjectModel","VSTest's object model, which the MSTest adapter uses (S4-1): test projects only" },
			{ "Microsoft.Testing.Platform","the Microsoft Testing Platform, which the MSTest adapter references (S4-1): test projects only" },
			{ "Microsoft.Testing.Platform.MSBuild","the platform's MSBuild support (S4-1): test projects only" },
			{ "Microsoft.Testing.Extensions.TrxReport.Abstractions","the platform's TRX abstractions, which the MSTest adapter references (S4-1): test "
				+"projects only" },
			{ "Microsoft.Testing.Extensions.Telemetry","the platform's telemetry extension: Directory.Build.targets keeps its assembly out of every "
				+"output folder (S4-1; tests\\ProjectFileTests, TestTelemetryTests)" },
			{ "Microsoft.ApplicationInsights","the telemetry extension's client: Directory.Build.targets keeps its assembly out of every output folder "
				+"(S4-1; tests\\ProjectFileTests, TestTelemetryTests)" },
			{ "System.Collections.Immutable","the MSTest adapter's (through System.Reflection.Metadata and Microsoft.TestPlatform.ObjectModel, S4-1): "
				+"test projects only" },
			{ "System.Reflection.Metadata","the MSTest adapter's (S4-1): test projects only" },
			//Stage 3b of the .NET 10 move (S3b-5 maps the lock files of the .NET 10 build).
			{ "Microsoft.TestPlatform.TestHost","VSTest's .NET test host (testhost.dll), which Microsoft.NET.Test.Sdk brings to a .NET test project: "
				+"test projects only" },
			{ "Microsoft.AspNetCore.TestHost","the in-process test server of MiddleTierHostTests (S3b-6): that test project only" },
			{ "Microsoft.CodeAnalysis.Analyzers","Roslyn analyzers only, which Microsoft.CodeAnalysis.Common names (S3b-2b)" },
			{ "Microsoft.XmlSerializer.Generator","the build's generator of the XmlSerializers files (S3b-3, S3b-4): build files and its own tool only" },
			{ "runtime.native.System.IO.Ports","System.IO.Ports' native part for other operating systems (S3b-3): no Windows file" },
			{ "runtime.android-arm.runtime.native.System.IO.Ports","native, Android only (S3b-3)" },
			{ "runtime.android-arm64.runtime.native.System.IO.Ports","native, Android only (S3b-3)" },
			{ "runtime.android-x64.runtime.native.System.IO.Ports","native, Android only (S3b-3)" },
			{ "runtime.android-x86.runtime.native.System.IO.Ports","native, Android only (S3b-3)" },
			{ "runtime.linux-arm.runtime.native.System.IO.Ports","native, Linux only (S3b-3)" },
			{ "runtime.linux-arm64.runtime.native.System.IO.Ports","native, Linux only (S3b-3)" },
			{ "runtime.linux-bionic-arm64.runtime.native.System.IO.Ports","native, Linux only (S3b-3)" },
			{ "runtime.linux-bionic-x64.runtime.native.System.IO.Ports","native, Linux only (S3b-3)" },
			{ "runtime.linux-musl-arm.runtime.native.System.IO.Ports","native, Linux only (S3b-3)" },
			{ "runtime.linux-musl-arm64.runtime.native.System.IO.Ports","native, Linux only (S3b-3)" },
			{ "runtime.linux-musl-x64.runtime.native.System.IO.Ports","native, Linux only (S3b-3)" },
			{ "runtime.linux-x64.runtime.native.System.IO.Ports","native, Linux only (S3b-3)" },
			{ "runtime.maccatalyst-arm64.runtime.native.System.IO.Ports","native, macOS only (S3b-3)" },
			{ "runtime.maccatalyst-x64.runtime.native.System.IO.Ports","native, macOS only (S3b-3)" },
			{ "runtime.osx-arm64.runtime.native.System.IO.Ports","native, macOS only (S3b-3)" },
			{ "runtime.osx-x64.runtime.native.System.IO.Ports","native, macOS only (S3b-3)" },
		};

		///<summary>Folders of a package of _dictNoLibraries that no build copies from, with the reason: the check that such a package holds no file of a
		///library file's name (RequiredDllsTests) skips them. Package id to folder prefixes, with '/'.</summary>
		private static readonly Dictionary<string,string[]> _dictFoldersNotCopied=new Dictionary<string,string[]>(StringComparer.OrdinalIgnoreCase) {
			//The code coverage collector and its own dependencies (System.Memory, System.Text.Json and the like, older or other builds than the
			//libraries'), which Microsoft.CodeCoverage.targets copies only after ComputeFilesToPublish, into a Microsoft.CodeCoverage sub folder.
			{ "Microsoft.CodeCoverage",new[] { "build/netstandard2.0/" } },
		};

		///<summary>True when the file (its path in the package's folder, with '/' or '\') is in a folder of the package that no build copies from
		///(_dictFoldersNotCopied).</summary>
		public static bool IsInFolderNotCopied(string packageId,string relativePath) {
			string[] arrayFolders;
			if(!_dictFoldersNotCopied.TryGetValue(packageId,out arrayFolders)) {
				return false;
			}
			string path=relativePath.Replace('\\','/');
			return arrayFolders.Any(x => path.StartsWith(x,StringComparison.OrdinalIgnoreCase));
		}

		///<summary>The mapped packages, by id.</summary>
		public static IEnumerable<string> MappedIds {
			get { return _dictAssets.Keys; }
		}

		///<summary>True for a package of the lock files that brings no library file.</summary>
		public static bool BringsNoLibraries(string packageId) {
			return _dictNoLibraries.ContainsKey(packageId);
		}

		///<summary>The assets of the package the build copies: none for one of _dictNoLibraries. Throws for a package neither list knows, and for a
		///mapping that is not at least one asset of a lib or runtimes\win-x86\native folder. where: the lock file, for the message.</summary>
		public static string[] GetAssets(string packageId,string where) {
			if(BringsNoLibraries(packageId)) {
				return new string[0];
			}
			string[] arrayAssets;
			if(!_dictAssets.TryGetValue(packageId,out arrayAssets)) {
				throw new InvalidOperationException(where+" names the package "+packageId+", which PackageFiles in tests\\LibraryBindingTests\\TestSupport.cs does not "
					+"map: add the files it brings (its assets, or, if it brings none, the reason), so the binding checks cover it.");
			}
			CheckMapping(packageId,arrayAssets,where);
			return arrayAssets;
		}

		///<summary>The file names of the package's assets (GetAssets).</summary>
		public static string[] GetFileNames(string packageId,string where) {
			return GetAssets(packageId,where).Select(x => x.Substring(x.LastIndexOf('/')+1)).ToArray();
		}

		///<summary>Throws unless the assets are at least one, each a file of a lib folder, of runtimes/win/lib/&lt;framework&gt; (a package's Windows
		///implementation, which the win-x64 target picks) or of runtimes/win-x64/native, and none named as a file "Required dlls" holds (a file has
		///one source).</summary>
		public static void CheckMapping(string packageId,string[] arrayAssets,string where) {
			List<string> listWrong=arrayAssets.Where(x => !Regex.IsMatch(x,@"^(lib/[^/]+|runtimes/win/lib/[^/]+|runtimes/win-x64/native)/[^/]+\.dll$",
				RegexOptions.IgnoreCase)).ToList();
			List<string> listInRequiredDlls=arrayAssets.Where(x => RequiredDlls.Files.ContainsKey(x.Substring(x.LastIndexOf('/')+1))).ToList();
			if(arrayAssets.Length==0 || listWrong.Count>0 || listInRequiredDlls.Count>0) {
				throw new InvalidOperationException("PackageFiles maps the package "+packageId+" ("+where+") to "
					+(arrayAssets.Length==0 ? "no file"
					: listWrong.Count>0 ? string.Join(", ",listWrong)+", which is no file of a lib, runtimes/win/lib or runtimes/win-x64/native folder"
					: string.Join(", ",listInRequiredDlls)+", which \"Required dlls\" holds too")
					+": every mapped package brings at least one file, from its package only.");
			}
		}
	}

	internal class BindingRedirect {
		public string Name;
		public string Token;
		public Version OldLow;
		public Version OldHigh;
		public Version NewVersion;

		public bool Covers(Version version) {
			return OldLow<=version && version<=OldHigh;
		}

		public override string ToString() {
			return Name+" ("+Token+") "+OldLow+"-"+OldHigh+" -> "+NewVersion;
		}
	}

	internal static class ConfigFiles {
		private static readonly XNamespace _xNamespaceBinding="urn:schemas-microsoft-com:asm.v1";

		///<summary>Every bindingRedirect of the config's runtime section, with the identity of its dependentAssembly.</summary>
		public static List<BindingRedirect> ReadRedirects(string path) {
			List<BindingRedirect> listRedirects=new List<BindingRedirect>();
			XDocument xDocument=XDocument.Load(path);
			foreach(XElement xElementDependent in xDocument.Descendants(_xNamespaceBinding+"dependentAssembly")) {
				XElement xElementIdentity=xElementDependent.Element(_xNamespaceBinding+"assemblyIdentity");
				foreach(XElement xElementRedirect in xElementDependent.Elements(_xNamespaceBinding+"bindingRedirect")) {
					string[] arrayOld=((string)xElementRedirect.Attribute("oldVersion")??"").Split('-');
					listRedirects.Add(new BindingRedirect {
						Name=(string)xElementIdentity?.Attribute("name")??"",
						Token=((string)xElementIdentity?.Attribute("publicKeyToken")??"").ToLowerInvariant(),
						OldLow=Version.Parse(arrayOld[0].Trim()),
						OldHigh=Version.Parse(arrayOld[arrayOld.Length-1].Trim()),
						NewVersion=Version.Parse(((string)xElementRedirect.Attribute("newVersion")).Trim()),
					});
				}
			}
			return listRedirects;
		}

		///<summary>The redirect element for a message: what to add to a config.</summary>
		public static string FormatRedirect(AssemblyFacts assemblyFacts) {
			return "<dependentAssembly><assemblyIdentity name=\""+assemblyFacts.Name+"\" publicKeyToken=\""+assemblyFacts.Token+"\" culture=\"neutral\" />"
				+"<bindingRedirect oldVersion=\"0.0.0.0-"+assemblyFacts.Version+"\" newVersion=\""+assemblyFacts.Version+"\" /></dependentAssembly>";
		}
	}

	///<summary>The shared framework the .NET 10 build runs on (Microsoft.NETCore.App and Microsoft.WindowsDesktop.App of the runtime this test
	///runs on, the version the program's self-contained folder carries), looked up by file name. Nothing is loaded. It takes the place of the
	///.NET Framework's global assembly cache, which these checks read before S3b-5 (.NET has none: Assembly.GlobalAssemblyCache is always false).
	///</summary>
	internal static class SharedFramework {
		private static List<string> _listFolders;

		///<summary>The folders of the two shared frameworks: this runtime's (System.Private.CoreLib's folder) and the Windows Desktop one of the
		///same version beside it.</summary>
		public static List<string> Folders {
			get {
				if(_listFolders==null) {
					string folderCore=Path.GetDirectoryName(typeof(object).Assembly.Location);
					string folderDesktop=Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(folderCore)),"Microsoft.WindowsDesktop.App",
						Path.GetFileName(folderCore));
					_listFolders=new[] { folderCore,folderDesktop }.Where(x => Directory.Exists(x)).ToList();
				}
				return _listFolders;
			}
		}

		public static bool Has(string name) {
			return Folders.Any(x => File.Exists(Path.Combine(x,name+".dll")));
		}
	}

	///<summary>A C# project of OpenDental.Local.slnf, as far as these tests need it.</summary>
	internal class ProjectInfo {
		public string Path;
		public string Folder;
		public string AssemblyName;
		public bool IsExe;
		public bool IsWeb;
		///<summary>Ends with "Tests", the rule Directory.Build.targets uses for the shared test config.</summary>
		public bool IsTest;
		public bool AutoGeneratesRedirects;
		///<summary>Names of the library files (Libraries) this project references (embedded interop types excluded) or copies to its output: the
		///files of "Required dlls" by a HintPath, and the files of the packages of its lock file (since S2-1 of the .NET 10 move, as PackageFiles
		///maps them).</summary>
		public List<string> ListLibraries=new List<string>();
		///<summary>Primary references into "Required dlls" that are copied to the output whether the code uses them or not, and the files of the
		///packages of its lock file, which NuGet copies to the output of a .NET Framework project used or not.</summary>
		public List<string> ListPrimaryReferencesCopied=new List<string>();
		///<summary>Files of "Required dlls" that None or Content items copy to the output (and to the output of every project that references this one).</summary>
		public List<string> ListCopyItems=new List<string>();
		public List<string> ListProjectReferences=new List<string>();
		///<summary>app.config, App.config or Web.config next to the project, or null.</summary>
		public string ConfigPath;

		///<summary>The folder its Debug build writes the primary output to, or null when it is not built.</summary>
		public string OutputFolder {
			get {
				foreach(string relative in new[] { @"bin\Debug",@"bin\x86\Debug","bin" }) {
					string folder=System.IO.Path.Combine(Folder,relative);
					if(File.Exists(System.IO.Path.Combine(folder,PrimaryOutputName))) {
						return folder;
					}
				}
				return null;
			}
		}

		///<summary>The managed assembly: AssemblyName.dll for an exe too on .NET, where the .exe is the native apphost (S3b-5).</summary>
		public string PrimaryOutputName {
			get { return AssemblyName+".dll"; }
		}
	}

	internal static class SolutionProjects {
		private static List<ProjectInfo> _listProjects;

		///<summary>The C# projects of OpenDental.Local.slnf.</summary>
		public static List<ProjectInfo> All {
			get {
				if(_listProjects!=null) {
					return _listProjects;
				}
				_listProjects=ProjectPaths.Select(x => Read(x)).ToList();
				return _listProjects;
			}
		}

		///<summary>The full paths of the C# project files of OpenDental.Local.slnf, in its order, without the projects of an SDK that ProjectFile
		///does not model (since S3b-6 the Middle Tier server, a Microsoft.NET.Sdk.Web project on .NET 10, which has no binding redirects).</summary>
		public static List<string> ProjectPaths {
			get {
				string slnf=RepoPaths.ReadText("OpenDental.Local.slnf");
				return Regex.Matches(slnf,@"""([^""]+\.csproj)""").Cast<Match>().Select(x => RepoPaths.Combine(x.Groups[1].Value.Replace(@"\\",@"\")))
					.Where(x => ProjectFile.GetUnmodelledSdk(x)==null).ToList();
			}
		}

		///<summary>The project's facts from its evaluated properties and items (Debug|x86, with Directory.Build.props and the other files it imports),
		///so they hold for the classic project files and the SDK-style ones of the .NET 10 move alike: no 2003 MSBuild namespace, metadata as
		///child elements or attributes, AutoGenerateBindingRedirects from tests\Directory.Build.props.</summary>
		private static ProjectInfo Read(string path) {
			ProjectFile projectFile=ProjectFile.Load(path);
			ProjectInfo projectInfo=new ProjectInfo();
			projectInfo.Path=projectFile.FullPath;
			projectInfo.Folder=projectFile.Folder;
			projectInfo.AssemblyName=projectFile.GetProperty("AssemblyName").Trim();
			if(projectInfo.AssemblyName=="") {
				projectInfo.AssemblyName=System.IO.Path.GetFileNameWithoutExtension(path);//MSBuild's default, the project's name
			}
			string outputType=projectFile.GetProperty("OutputType").Trim();
			projectInfo.IsExe=outputType.Equals("Exe",StringComparison.OrdinalIgnoreCase) || outputType.Equals("WinExe",StringComparison.OrdinalIgnoreCase);
			projectInfo.IsWeb=projectFile.GetProperty("ProjectTypeGuids").IndexOf("349c5851-65df-11da-9384-00065b846f21",StringComparison.OrdinalIgnoreCase)>=0;
			projectInfo.IsTest=System.IO.Path.GetFileNameWithoutExtension(path).EndsWith("Tests");
			projectInfo.AutoGeneratesRedirects=projectFile.GetProperty("AutoGenerateBindingRedirects").Trim().Equals("true",StringComparison.OrdinalIgnoreCase)
				&& projectFile.GetProperty("GenerateBindingRedirectsOutputType").Trim().Equals("true",StringComparison.OrdinalIgnoreCase);
			foreach(ProjectItem projectItemReference in projectFile.GetItems("Reference")) {
				string fileName=GetRequiredDllsFileName(projectItemReference.GetMetadata("HintPath"));
				if(fileName==null || projectItemReference.GetMetadata("EmbedInteropTypes").Trim().Equals("true",StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
				projectInfo.ListLibraries.Add(fileName);
				if(!projectItemReference.GetMetadata("Private").Trim().Equals("false",StringComparison.OrdinalIgnoreCase)) {
					projectInfo.ListPrimaryReferencesCopied.Add(fileName);
				}
			}
			foreach(string fileName in GetLibrariesOfPackages(projectInfo.Folder)) {
				projectInfo.ListLibraries.Add(fileName);
				projectInfo.ListPrimaryReferencesCopied.Add(fileName);
			}
			foreach(ProjectItem projectItem in projectFile.GetItems("None").Concat(projectFile.GetItems("Content"))) {
				string fileName=GetRequiredDllsFileName(projectItem.Include);
				if(fileName!=null && projectItem.HasMetadata("CopyToOutputDirectory")) {
					projectInfo.ListLibraries.Add(fileName);
					projectInfo.ListCopyItems.Add(fileName);
				}
			}
			foreach(ProjectItem projectItemReference in projectFile.GetItems("ProjectReference")) {
				if(projectItemReference.Include.EndsWith(".csproj",StringComparison.OrdinalIgnoreCase)) {
					projectInfo.ListProjectReferences.Add(projectItemReference.FullPath);
				}
			}
			projectInfo.ConfigPath=Directory.GetFiles(projectInfo.Folder,"*.config")
				.FirstOrDefault(x => new[] { "app.config","web.config" }.Contains(System.IO.Path.GetFileName(x).ToLowerInvariant()));
			return projectInfo;
		}

		///<summary>The library files that the packages of the project's packages.lock.json bring (every package the restore gives it for .NET
		///Framework 4.8, its own, those of the projects it references and their dependencies), as PackageFiles maps them; it throws for a package
		///it does not know, so none drops out of the checks.</summary>
		private static List<string> GetLibrariesOfPackages(string folderProject) {
			List<string> listFiles=new List<string>();
			string pathLock=System.IO.Path.Combine(folderProject,"packages.lock.json");
			if(!File.Exists(pathLock)) {
				return listFiles;
			}
			foreach(LockedPackage lockedPackage in NuGetFiles.ReadLockFile(pathLock)) {
				foreach(string fileName in PackageFiles.GetFileNames(lockedPackage.Id,RepoPaths.Relative(pathLock))) {
					if(!listFiles.Contains(fileName,StringComparer.OrdinalIgnoreCase)) {
						listFiles.Add(fileName);
					}
				}
			}
			return listFiles;
		}

		///<summary>The file name when the path points into "Required dlls" (..\Required dlls\X.dll, ..\..\Required dlls\X.dll), otherwise null.</summary>
		private static string GetRequiredDllsFileName(string path) {
			if(string.IsNullOrWhiteSpace(path)) {
				return null;
			}
			Match match=Regex.Match(path.Trim(),@"(?:^|\\)Required dlls\\([^\\]+)$",RegexOptions.IgnoreCase);
			return match.Success ? match.Groups[1].Value : null;
		}

		public static ProjectInfo ByName(string projectFileName) {
			return All.Single(x => System.IO.Path.GetFileName(x.Path).Equals(projectFileName,StringComparison.OrdinalIgnoreCase));
		}

		///<summary>The project and the projects it references, all the way down.</summary>
		public static List<ProjectInfo> Closure(ProjectInfo projectInfo) {
			List<ProjectInfo> listProjects=new List<ProjectInfo>();
			HashSet<string> hashSetSeen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			Stack<ProjectInfo> stackProjects=new Stack<ProjectInfo>();
			stackProjects.Push(projectInfo);
			while(stackProjects.Count>0) {
				ProjectInfo projectInfoCur=stackProjects.Pop();
				if(!hashSetSeen.Add(projectInfoCur.Path)) {
					continue;
				}
				listProjects.Add(projectInfoCur);
				foreach(string pathReference in projectInfoCur.ListProjectReferences) {
					ProjectInfo projectInfoReference=All.FirstOrDefault(x => x.Path.Equals(pathReference,StringComparison.OrdinalIgnoreCase));
					if(projectInfoReference!=null) {
						stackProjects.Push(projectInfoReference);
					}
				}
			}
			return listProjects;
		}

		///<summary>The library files the project and the projects it references (all the way down) reference or copy.</summary>
		public static HashSet<string> LibrariesInClosure(ProjectInfo projectInfo) {
			HashSet<string> hashSetFiles=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach(ProjectInfo projectInfoCur in Closure(projectInfo)) {
				hashSetFiles.UnionWith(projectInfoCur.ListLibraries);
			}
			return hashSetFiles;
		}

		///<summary>True when the project, directly or through the projects it references, uses a managed library assembly (Libraries).</summary>
		public static bool LoadsLibraryAssemblies(ProjectInfo projectInfo) {
			return LibrariesInClosure(projectInfo).Any(x => Libraries.ByFileName(x)!=null);
		}
	}

	internal static class BindingRules {
		///<summary>How .NET binds a reference between the assemblies (S3b-5; the .NET Framework's binding-redirect rule retired with the .NET
		///Framework build): the runtime loads the one file of a name that its .deps.json names, and takes it for any reference to the same name and
		///key at that version or below; a reference to a higher version than the file fails to load (FileLoadException). There is no config to
		///redirect with. So each reference from one of the assemblies to another of them (strong-named, the same key) must ask for the version
		///there or a lower one. Returns one line per reference that asks for more.</summary>
		public static List<string> FindReferencesAboveTheVersionThere(Dictionary<string,AssemblyFacts> dictAssemblies,string label) {
			List<string> listProblems=new List<string>();
			foreach(AssemblyFacts assemblyFacts in dictAssemblies.Values.OrderBy(x => x.FileName,StringComparer.OrdinalIgnoreCase)) {
				foreach(AssemblyReference assemblyReference in assemblyFacts.ListReferences) {
					if(!dictAssemblies.TryGetValue(assemblyReference.Name,out AssemblyFacts assemblyFactsTarget)) {
						continue;
					}
					//The loader ignores the version of an assembly without a strong name, and another key is another assembly.
					if(assemblyReference.Token=="" || assemblyReference.Token!=assemblyFactsTarget.Token) {
						continue;
					}
					if(assemblyReference.Version>assemblyFactsTarget.Version) {
						listProblems.Add(label+": "+assemblyFacts.FileName+" references "+assemblyReference.Name+" "+assemblyReference.Version+", but "
							+assemblyFactsTarget.FileName+" is "+assemblyFactsTarget.Version+", lower: it does not load on .NET.");
					}
				}
			}
			return listProblems;
		}
	}

	internal static class BuildRelease {
		private static List<KeyValuePair<string,string>> _listFiles;

		///<summary>The allow-list as tools\Build-Release.ps1 -ListFiles prints it: kind ("own", "third", "doc", "runtime" or "setup") and path
		///(a "runtime" file of the 32-bit helper starts with Helper32\). Nothing is built.</summary>
		public static List<KeyValuePair<string,string>> ListFiles() {
			if(_listFiles!=null) {
				return _listFiles;
			}
			ProcessStartInfo processStartInfo=new ProcessStartInfo("powershell.exe");
			processStartInfo.Arguments="-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \""+RepoPaths.Combine("tools","Build-Release.ps1")+"\" -ListFiles";
			processStartInfo.UseShellExecute=false;
			processStartInfo.RedirectStandardOutput=true;
			processStartInfo.RedirectStandardError=true;
			processStartInfo.CreateNoWindow=true;
			using(Process process=Process.Start(processStartInfo)) {
				Task<string> taskOut=process.StandardOutput.ReadToEndAsync();
				Task<string> taskErr=process.StandardError.ReadToEndAsync();
				if(!process.WaitForExit(120000)) {
					process.Kill();
					Assert.Fail("Build-Release.ps1 -ListFiles did not finish in 2 minutes.");
				}
				process.WaitForExit();
				Assert.AreEqual(0,process.ExitCode,taskOut.Result+taskErr.Result);
				_listFiles=new List<KeyValuePair<string,string>>();
				foreach(string line in taskOut.Result.Split(new[] { "\r\n","\n" },StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())) {
					string[] arrayParts=line.Split('|');
					Assert.AreEqual(2,arrayParts.Length,"Unexpected line: "+line);
					_listFiles.Add(new KeyValuePair<string,string>(arrayParts[0],arrayParts[1]));
				}
			}
			return _listFiles;
		}
	}
}
