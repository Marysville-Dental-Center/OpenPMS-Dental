// The release ships exactly the library files the programs need (those of "Required dlls" and, since S2-4 of the .NET 10 move, the NuGet
// packages' own files at the lock files' versions): tools\Build-Release.ps1's allow-list (which is also what installer\OpenPmsDental.iss
// installs, since it installs the staging folder as a whole, plus the setup files it installs into the Windows system folder) against what the
// built program references, all the way down, and every assembly a shipped file references is shipped too or is part of the .NET 10 shared
// framework (Microsoft.NETCore.App and Microsoft.WindowsDesktop.App), which the self-contained release ships whole as its "runtime" files.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectFileTests;

namespace LibraryBindingTests {
	[TestClass]
	public class ReleaseListTests {
		///<summary>References of shipped files that neither the release nor the .NET Framework has, with why the program still runs. Keep this list
		///short and exact. (MimeKit 2.4.1's BouncyCastle.Crypto left it with the MailKit and MimeKit 4 update, which ships BouncyCastle.Cryptography.)
		///</summary>
		private static readonly Dictionary<string,string> _dictKnownGaps=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase) {
			{ "Health.Direct.Config.Client","Health.Direct.ResolverPlugins uses it only for the Direct configuration service; the program uses only its "
				+"LdapCertResolver" },
			{ "Health.Direct.Config.Store","Health.Direct.ResolverPlugins uses it only for the Direct configuration service; the program uses only its "
				+"LdapCertResolver" },
			//TC-4 (docs/port-notes/TC-4.md): no package or file holds SRTM.dll (SRTM 0.0.8.0, the itinero/srtm reader).
			{ "SRTM","HelixToolkit.Wpf uses it only in its terrain models (HgtTerrainModel and the TerrainVisual3D that loads one); the tooth chart "
				+"never shows terrain (HelixReferenceTests)" },
			//S3b-5: the two references .NET 10's shared framework does not answer, which the .NET Framework did (its System.ServiceModel) or which
			//the .NET builds brought. Recorded for the lead (docs/port-notes/S3b-5.md): each needs a launch check before the office release.
			{ "System.ServiceModel","Health.Direct.ResolverPlugins (a .NET Framework file of Required dlls) references the .NET Framework's WCF for the "
				+"Direct configuration service's client, as Health.Direct.Config.Client above; the program uses only its LdapCertResolver" },
			{ "Microsoft.Windows.SDK.NET","Microsoft.Web.WebView2.Wpf of lib_manual/net5.0-windows10.0.17763.0 (the WebView2 package's build files) "
				+"references the Windows SDK projection for its WinRT interop members, which the program's WebView2 control does not use" },
			{ "WinRT.Runtime","Microsoft.Web.WebView2.Wpf of lib_manual/net5.0-windows10.0.17763.0 references C#/WinRT's runtime for the same WinRT "
				+"interop members (Microsoft.Windows.SDK.NET above)" },
		};

		///<summary>Files the release still ships although nothing in the program references them, each with why. Keep this list short and exact.
		///</summary>
		private static readonly Dictionary<string,string> _dictShippedUnused=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase) {
			//Empty since TC-9b (docs/port-notes/TC-9b.md): SharpDX.Direct3D9.dll and SharpDX.Mathematics.dll, which TC-9a left shipped but unused,
			//left the release with SharpDX.dll and the two Tao files.
		};

		private static string ProgramFolder {
			get { return RepoPaths.Combine("OpenPmsDental","bin","Debug"); }
		}

		private static void SkipUnlessBuilt() {
			if(!File.Exists(Path.Combine(ProgramFolder,"OpenPmsDental.exe"))) {
				BuildOutput.NotBuilt(@"OpenPmsDental\bin\Debug is not built. Build OpenDental.Local.slnf (Debug|x64) first.");
			}
		}

		///<summary>The program's own assemblies in its output folder (and the third-party ones built from this repository): the files on the
		///allow-list that are no library files (Libraries). Only the "own" and "third" kinds: the "runtime" files are the shared framework itself
		///(among them its compatibility facades, such as System.Data.dll, which type-forward to packages outside the shared framework), which
		///Build-Release -Full byte-compares with the runtime packs; they are no program assemblies.</summary>
		private static List<AssemblyFacts> GetBuiltAssemblies() {
			List<AssemblyFacts> listAssemblies=new List<AssemblyFacts>();
			foreach(KeyValuePair<string,string> pair in BuildRelease.ListFiles().Where(x => (x.Key=="own" || x.Key=="third") && MetadataReader.IsBinary(x.Value))) {
				string path=Path.Combine(ProgramFolder,pair.Value);
				if(Libraries.Files.ContainsKey(pair.Value) || !File.Exists(path)) {
					continue;//OpenPmsDental.XmlSerializers.dll and OpenPmsDentalBusiness.XmlSerializers.dll come with Release builds only.
				}
				AssemblyFacts assemblyFacts=MetadataReader.Read(path);
				if(assemblyFacts.IsManaged) {
					listAssemblies.Add(assemblyFacts);
				}
			}
			return listAssemblies;
		}

		///<summary>The library files the program folder gets: OpenPmsDental's own references to them and the files of its packages (always copied),
		///the files its projects copy with None or Content items, and every library file that the program's assemblies reference, all the way
		///down.</summary>
		private static HashSet<string> GetLibrariesTheProgramNeeds() {
			ProjectInfo projectInfoProgram=SolutionProjects.ByName("OpenPmsDental.csproj");
			HashSet<string> hashSetNeeded=new HashSet<string>(projectInfoProgram.ListPrimaryReferencesCopied,StringComparer.OrdinalIgnoreCase);
			foreach(ProjectInfo projectInfo in SolutionProjects.Closure(projectInfoProgram)) {
				hashSetNeeded.UnionWith(projectInfo.ListCopyItems);
			}
			Queue<AssemblyFacts> queueAssemblies=new Queue<AssemblyFacts>(GetBuiltAssemblies());
			foreach(string fileName in hashSetNeeded) {
				AssemblyFacts assemblyFacts=Libraries.ByFileName(fileName);
				if(assemblyFacts!=null) {
					queueAssemblies.Enqueue(assemblyFacts);
				}
			}
			HashSet<string> hashSetSeen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			while(queueAssemblies.Count>0) {
				AssemblyFacts assemblyFacts=queueAssemblies.Dequeue();
				if(!hashSetSeen.Add(assemblyFacts.Path)) {
					continue;
				}
				foreach(AssemblyReference assemblyReference in assemblyFacts.ListReferences) {
					if(Libraries.Assemblies.TryGetValue(assemblyReference.Name,out AssemblyFacts assemblyFactsLibrary)) {
						hashSetNeeded.Add(assemblyFactsLibrary.FileName);
						queueAssemblies.Enqueue(assemblyFactsLibrary);
					}
				}
			}
			return hashSetNeeded;
		}

		[TestMethod]
		public void AllowList_NamesExactlyTheLibraryFilesTheProgramsNeed() {
			SkipUnlessBuilt();
			HashSet<string> hashSetNeeded=GetLibrariesTheProgramNeeds();
			List<string> listAllowed=BuildRelease.ListFiles().Where(x => x.Key=="third" && Libraries.Files.ContainsKey(x.Value)).Select(x => x.Value).ToList();
			//The self-contained release ships the shared framework whole (decision 20): a package file that is also a file of the runtime packs
			//(System.CodeDom, System.Diagnostics.EventLog and System.Security.Cryptography.ProtectedData of Microsoft.WindowsDesktop.App) ships as
			//Build-Release's "runtime" file beside the programs, never on the third-party list as well (Build-Release refuses a file on two lists).
			List<string> listRuntime=BuildRelease.ListFiles().Where(x => x.Key=="runtime" && !x.Value.Contains("\\")).Select(x => x.Value).ToList();
			List<string> listProblems=new List<string>();
			foreach(string fileName in hashSetNeeded.Where(x => listAllowed.Contains(x,StringComparer.OrdinalIgnoreCase)
				&& listRuntime.Contains(x,StringComparer.OrdinalIgnoreCase)).OrderBy(x => x))
			{
				listProblems.Add(fileName+": tools\\Build-Release.ps1 lists it as a runtime file and as a third-party file: keep it on the runtime list only.");
			}
			foreach(string fileName in hashSetNeeded.Where(x => !listAllowed.Contains(x,StringComparer.OrdinalIgnoreCase)
				&& !listRuntime.Contains(x,StringComparer.OrdinalIgnoreCase)).OrderBy(x => x))
			{
				listProblems.Add(fileName+": the program needs it, but tools\\Build-Release.ps1 does not ship it (add it to the allow-list, with its notice).");
			}
			foreach(string fileName in listAllowed.Where(x => !hashSetNeeded.Contains(x) && !_dictShippedUnused.ContainsKey(x)).OrderBy(x => x)) {
				listProblems.Add(fileName+": tools\\Build-Release.ps1 ships it, but nothing in the program uses it.");
			}
			//The known unused files: still shipped, and still unused (one that a program file needs again is no longer an exception).
			foreach(string fileName in _dictShippedUnused.Keys) {
				Assert.IsTrue(listAllowed.Contains(fileName,StringComparer.OrdinalIgnoreCase),fileName+" is no longer shipped: remove it from _dictShippedUnused.");
				Assert.IsFalse(hashSetNeeded.Contains(fileName),fileName+" is used again: remove it from _dictShippedUnused.");
			}
			foreach(string fileName in hashSetNeeded.Where(x => !File.Exists(Path.Combine(ProgramFolder,x))).OrderBy(x => x)) {
				listProblems.Add(fileName+": the program needs it, but the build did not copy it to OpenPmsDental\\bin\\Debug.");
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("\r\n",listProblems));
			//The support assemblies the first library step added for the packages that follow: on .NET 10, System.Formats.Asn1 and
			//System.IO.Pipelines are files of the shared framework, so they ship as "runtime" files and not as third-party ones, and
			//Microsoft.Bcl.Cryptography is pruned (the framework has its types): on no list and in no lock file. System.Security.Cryptography.Pkcs,
			//which nothing used, left "Required dlls" in S2-4 of the .NET 10 move: it is no library file, in no lock file and not on the allow-list.
			foreach(string fileName in new[] { "System.Formats.Asn1.dll","System.IO.Pipelines.dll" }) {
				CollectionAssert.Contains(listRuntime,fileName);
				CollectionAssert.DoesNotContain(listAllowed,fileName);
			}
			Assert.IsFalse(BuildRelease.ListFiles().Any(x => x.Value.EndsWith("Microsoft.Bcl.Cryptography.dll",StringComparison.OrdinalIgnoreCase)),
				"Microsoft.Bcl.Cryptography.dll is pruned on .NET 10: on no list.");
			Assert.IsFalse(Libraries.LockedVersions.ContainsKey("Microsoft.Bcl.Cryptography"),"Microsoft.Bcl.Cryptography is pruned on .NET 10: in no lock file.");
			Assert.IsFalse(Libraries.Files.ContainsKey("System.Security.Cryptography.Pkcs.dll"));
			Assert.IsFalse(Libraries.LockedVersions.ContainsKey("System.Security.Cryptography.Pkcs"),"No project gets the package.");
			CollectionAssert.DoesNotContain(listAllowed,"System.Security.Cryptography.Pkcs.dll","Nothing shipped uses it.");
			CollectionAssert.Contains(listAllowed,"Microsoft.Extensions.DependencyInjection.Abstractions.dll",
				"Microsoft.Extensions.Logging.Abstractions 10.0.15 and MySqlConnector 2.6.2 reference it, so it ships.");
		}

		[TestMethod]
		public void Installer_InstallsTheStagingFolderAsAWhole() {
			string iss=RepoPaths.ReadText("installer","OpenPmsDental.iss");
			List<string> listSources=Regex.Matches(iss,@"(?m)^\s*Source:\s*""([^""]+)""(.*)$").Cast<Match>().Select(x => x.Value.Trim()).ToList();
			List<string> listStaging=listSources.Where(x => x.StartsWith(@"Source: ""{#StagingDir}\*""")).ToList();
			Assert.AreEqual(1,listStaging.Count,"One [Files] entry installs the staging folder, which holds exactly the allow-list:\r\n"+string.Join("\r\n",listSources));
			StringAssert.Contains(listStaging[0],"recursesubdirs");
			//The setup files (Build-Release's $SetupFiles: the 64-bit TWAINDSM.dll): each is one [Files] line of its own, a shared system file in
			//the Windows system folder that the uninstall leaves (other programs use it), never in the program folder.
			List<string> listSetupLines=new List<string>();
			foreach(string name in BuildRelease.ListFiles().Where(x => x.Key=="setup").Select(x => x.Value)) {
				//The installer names the two TWAINDSM.dll files through its defines (Build-Release passes the checked download's paths): the 64-bit
				//one as TwainDsmFile, the 32-bit one (setup|x86\TWAINDSM.dll, S3b-8) as TwainDsm32File, which goes to {sys} with the 32bit flag, the
				//32-bit system folder.
				string defineName=null;
				if(name.Equals("TWAINDSM.dll",StringComparison.OrdinalIgnoreCase)) {
					defineName=@"Source: ""{#TwainDsmFile}""";
				}
				else if(name.Equals(@"x86\TWAINDSM.dll",StringComparison.OrdinalIgnoreCase)) {
					defineName=@"Source: ""{#TwainDsm32File}""";
				}
				List<string> listLines=listSources.Where(x => Regex.IsMatch(x,@"^Source:\s*""[^""]*\\"+Regex.Escape(name)+@"""",RegexOptions.IgnoreCase)
					|| (defineName!=null && x.StartsWith(defineName))).ToList();
				Assert.AreEqual(1,listLines.Count,"One [Files] line installs the setup file "+name+":\r\n"+string.Join("\r\n",listSources));
				StringAssert.Contains(listLines[0],@"DestDir: ""{sys}""",listLines[0]);
				Assert.IsFalse(listLines[0].Contains("{app}"),"A setup file is never installed into the program folder: "+listLines[0]);
				Match matchFlags=Regex.Match(listLines[0],@"Flags:\s*([^;]*)");
				Assert.IsTrue(matchFlags.Success,listLines[0]);
				List<string> listFlags=matchFlags.Groups[1].Value.Split(new[] { ' ' },StringSplitOptions.RemoveEmptyEntries).ToList();
				CollectionAssert.Contains(listFlags,"sharedfile",listLines[0]);
				CollectionAssert.Contains(listFlags,"uninsneveruninstall",listLines[0]);
				if(name.StartsWith(@"x86\",StringComparison.OrdinalIgnoreCase)) {
					CollectionAssert.Contains(listFlags,"32bit","The 32-bit setup file goes to the 32-bit system folder: "+listLines[0]);
				}
				listSetupLines.AddRange(listLines);
			}
			foreach(string source in listSources.Except(listStaging).Except(listSetupLines)) {
				//Only the connection settings that setup writes itself.
				StringAssert.Contains(source,"external",source);
				Assert.IsFalse(Regex.IsMatch(source,@"\.(dll|exe)""",RegexOptions.IgnoreCase),"The installer names a program file of its own: "+source);
			}
			//Files an upgrade removes must never be ones the release still ships.
			Match matchDelete=Regex.Match(iss,@"(?ms)^\[InstallDelete\]\s*$(.*?)(?=^\[|\z)");
			if(matchDelete.Success) {
				List<string> listAllowed=BuildRelease.ListFiles().Select(x => x.Value).ToList();
				foreach(Match matchName in Regex.Matches(matchDelete.Groups[1].Value,@"Name:\s*""\{app\}\\([^""]+)""")) {
					Assert.IsFalse(listAllowed.Contains(matchName.Groups[1].Value,StringComparer.OrdinalIgnoreCase),
						"[InstallDelete] removes a file the release ships: "+matchName.Groups[1].Value);
				}
			}
		}

		[TestMethod]
		public void ShippedAssemblies_FindEveryAssemblyTheyReference() {
			SkipUnlessBuilt();
			List<AssemblyFacts> listShipped=GetBuiltAssemblies();
			listShipped.AddRange(GetLibrariesTheProgramNeeds().Select(x => Libraries.ByFileName(x)).Where(x => x!=null));
			HashSet<string> hashSetNames=new HashSet<string>(listShipped.Select(x => x.Name),StringComparer.OrdinalIgnoreCase);
			List<string> listProblems=new List<string>();
			HashSet<string> hashSetGapsSeen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach(AssemblyFacts assemblyFacts in listShipped.OrderBy(x => x.FileName,StringComparer.OrdinalIgnoreCase)) {
				foreach(AssemblyReference assemblyReference in assemblyFacts.ListReferences) {
					//No shipped assembly references Windows Runtime metadata since S3a-5 (PasswordVaultWrapper uses the Credential Manager), and .NET 10
					//could not load one that did.
					if(assemblyReference.IsWindowsRuntime) {
						listProblems.Add(assemblyFacts.FileName+" references the Windows Runtime metadata "+assemblyReference.Name+".");
						continue;
					}
					if(hashSetNames.Contains(assemblyReference.Name) || SharedFramework.Has(assemblyReference.Name)) {
						continue;
					}
					if(_dictKnownGaps.ContainsKey(assemblyReference.Name)) {
						hashSetGapsSeen.Add(assemblyReference.Name);
						continue;
					}
					listProblems.Add(assemblyFacts.FileName+" references "+assemblyReference.Name+" "+assemblyReference.Version
						+", which neither ships nor is part of the shared framework.");
				}
			}
			foreach(string name in _dictKnownGaps.Keys.Where(x => !hashSetGapsSeen.Contains(x))) {
				listProblems.Add("The known gap "+name+" is gone; remove it from the list in ReleaseListTests.");
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("\r\n",listProblems));
		}
	}
}
