// TC-4 (docs/port-notes/TC-4.md, docs/specs/wpf-tooth-chart-plan.md): Helix Toolkit 3.1.2 for the WPF tooth chart. The release ships the eight
// files of its closure from their packages, each IL-only and not 32-bit-required; the System.* 4.3.x packages of System.Net.Http 4.3.4 put no
// file in any built folder and none is staged (risk W2); no CommunityToolkit.Mvvm analyzer reaches a compilation (the record each build writes,
// Directory.Build.targets); and the types the chart will use load from the program folder with the program's config: a MeshBuilder box, made
// into a WPF MeshGeometry3D, in a child process (TestProbeHost) since S3a-8b of the .NET 10 move. No chart code uses Helix yet, and nothing here
// starts the program.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectFileTests;
using TestProbeHost;

namespace ReleaseFlowTests {
	[TestClass]
	public class HelixPackageTests {
		///<summary>The files of Helix Toolkit's closure that ship, and the package of each.</summary>
		private static readonly string[][] _arrayHelixFiles=new[] {
			new[] { "HelixToolkit.dll","HelixToolkit" },
			new[] { "HelixToolkit.Geometry.dll","HelixToolkit.Geometry" },
			new[] { "HelixToolkit.Maths.dll","HelixToolkit.Maths" },
			new[] { "HelixToolkit.Wpf.dll","HelixToolkit.Wpf" },
			new[] { "CommunityToolkit.Common.dll","CommunityToolkit.Common" },
			new[] { "CommunityToolkit.Diagnostics.dll","CommunityToolkit.Diagnostics" },
			new[] { "CommunityToolkit.Mvvm.dll","CommunityToolkit.Mvvm" },
			new[] { "System.ComponentModel.Annotations.dll","System.ComponentModel.Annotations" },
		};

		///<summary>The files of System.Net.Http 4.3.4 and the System.* 4.3.x packages it brings. The .NET Framework 4.8 has newer assemblies of
		///these names, which the build keeps, so no built folder and no release may hold one (risk W2 of the plan).</summary>
		private static readonly string[] _arrayFrameworkFiles=new[] {
			"System.Net.Http.dll","System.IO.dll","System.IO.Compression.dll","System.Runtime.dll","System.Text.RegularExpressions.dll",
			"System.Security.Cryptography.Algorithms.dll","System.Security.Cryptography.Encoding.dll","System.Security.Cryptography.Primitives.dll",
			"System.Security.Cryptography.X509Certificates.dll","System.Private.Uri.dll",
		};

		private static string FolderProgram {
			get { return Path.Combine(TestPaths.RepoRoot,"OpenPmsDental","bin","Debug"); }
		}

		///<summary>Build-Release.ps1 -ListFiles: "own|name", "third|name" and "doc|path" lines.</summary>
		private static List<string> ListReleaseFiles() {
			ProcessStartInfo processStartInfo=new ProcessStartInfo("powershell.exe");
			processStartInfo.Arguments="-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \""+Path.Combine(TestPaths.RepoRoot,"tools","Build-Release.ps1")
				+"\" -ListFiles";
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
				return taskOut.Result.Split(new[] { "\r\n","\n" },StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
			}
		}

		///<summary>The CLI header flags of a managed PE file (IMAGE_COR20_HEADER.Flags), read from its bytes; nothing is loaded.</summary>
		private static uint ReadCorFlags(string path) {
			byte[] arrayBytes=File.ReadAllBytes(path);
			int offsetPe=BitConverter.ToInt32(arrayBytes,0x3C);
			Assert.AreEqual(0x00004550u,BitConverter.ToUInt32(arrayBytes,offsetPe),path+": no PE signature.");
			int countSections=BitConverter.ToUInt16(arrayBytes,offsetPe+6);
			int sizeOptional=BitConverter.ToUInt16(arrayBytes,offsetPe+20);
			int offsetOptional=offsetPe+24;
			ushort magic=BitConverter.ToUInt16(arrayBytes,offsetOptional);
			int offsetDirectories=offsetOptional+(magic==0x20B ? 112 : 96);
			uint rvaCli=BitConverter.ToUInt32(arrayBytes,offsetDirectories+14*8);
			Assert.AreNotEqual(0u,rvaCli,path+" has no CLI header (not managed).");
			int offsetSections=offsetOptional+sizeOptional;
			for(int i=0;i<countSections;i++) {
				int offsetSection=offsetSections+i*40;
				uint sizeVirtual=BitConverter.ToUInt32(arrayBytes,offsetSection+8);
				uint rvaSection=BitConverter.ToUInt32(arrayBytes,offsetSection+12);
				uint sizeRaw=BitConverter.ToUInt32(arrayBytes,offsetSection+16);
				uint offsetRaw=BitConverter.ToUInt32(arrayBytes,offsetSection+20);
				if(rvaCli>=rvaSection && rvaCli<rvaSection+Math.Max(sizeVirtual,sizeRaw)) {
					return BitConverter.ToUInt32(arrayBytes,(int)(rvaCli-rvaSection+offsetRaw)+16);
				}
			}
			Assert.Fail(path+": the CLI header is in no section.");
			return 0;
		}

		///<summary>The eight files are on the release's allow-list as files of their packages (Build-Release.ps1 compares each staged file with
		///its package, Test-PackageFiles), and none of the files the .NET Framework replaces is.</summary>
		[TestMethod]
		public void AllowList_ShipsTheHelixFiles_FromTheirPackages_AndNoSystemNetHttp() {
			string script=TestPaths.ReadSource("tools","Build-Release.ps1");
			List<string> listThird=ListReleaseFiles().Where(x => x.StartsWith("third|")).Select(x => x.Substring("third|".Length)).ToList();
			foreach(string[] arrayRow in _arrayHelixFiles) {
				CollectionAssert.Contains(listThird,arrayRow[0]);
				StringAssert.Contains(script,"'"+arrayRow[0]+"'='package:"+arrayRow[1]+"'");
			}
			foreach(string fileName in _arrayFrameworkFiles) {
				Assert.IsFalse(listThird.Contains(fileName,StringComparer.OrdinalIgnoreCase),fileName+" is on the allow-list.");
			}
			//SRTM.dll, which HelixToolkit.Wpf references only for its terrain types (LibraryBindingTests' HelixReferenceTests), ships nowhere.
			Assert.IsFalse(listThird.Contains("SRTM.dll",StringComparer.OrdinalIgnoreCase));
		}

		///<summary>The program folder holds each of the eight files, byte for byte its package's file at the version the program's lock file names,
		///IL-only and not 32-bit-required (they run in the x86 program as they would in a 64-bit one).</summary>
		[TestMethod]
		public void ProgramFolder_HoldsTheHelixFiles_ILOnly_AndNot32BitRequired() {
			if(!File.Exists(Path.Combine(FolderProgram,"OpenPmsDental.exe"))) {
				BuildOutput.NotBuilt(@"OpenPmsDental\bin\Debug is not built. Build OpenDental.Local.slnf (Debug|x86) first.");
				return;
			}
			Dictionary<string,PackageFile> dictPackageFiles=NuGetFiles.ReadCopiedFiles(TestPaths.RepoRoot,Path.Combine(TestPaths.RepoRoot,"OpenPmsDental"));
			Assert.IsNotNull(dictPackageFiles,"OpenPmsDental is built, so it is restored.");
			List<string> listProblems=new List<string>();
			foreach(string[] arrayRow in _arrayHelixFiles) {
				string path=Path.Combine(FolderProgram,arrayRow[0]);
				if(!File.Exists(path)) {
					listProblems.Add(arrayRow[0]+" is not in the program folder.");
					continue;
				}
				if(!dictPackageFiles.TryGetValue(arrayRow[0],out PackageFile packageFile) || !packageFile.Id.Equals(arrayRow[1],StringComparison.OrdinalIgnoreCase)) {
					listProblems.Add(arrayRow[0]+" is not a file the program's restore copies from "+arrayRow[1]+".");
					continue;
				}
				if(packageFile.Path==null || NuGetFiles.Sha256(packageFile.Path)!=NuGetFiles.Sha256(path)) {
					listProblems.Add(arrayRow[0]+" is not "+packageFile+".");
				}
				uint corFlags=ReadCorFlags(path);
				if((corFlags & 0x1)==0 || (corFlags & 0x2)!=0 || (corFlags & 0x20000)!=0) {
					listProblems.Add(arrayRow[0]+": CLI flags 0x"+corFlags.ToString("x")+" (IL-only 0x1 expected, without 32-bit-required 0x2 and 32-bit-preferred 0x20000).");
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("\r\n",listProblems));
		}

		///<summary>Risk W2: no built folder of a project of the solution filter (NuGetFiles.ListSolutionLockFiles: never another worktree's) that
		///gets System.Net.Http 4.3.4 (its lock file names it) holds a file of the System.* 4.3.x
		///packages, and none has a runtimes sub folder (the program's win-x86 restore names runtimes\win\lib files of four of them; the build
		///keeps the .NET Framework's newer assemblies instead). So the programs bind System.Net.Http and the others to the framework, as before.</summary>
		[TestMethod]
		public void BuiltFolders_HoldNoFileOfTheSystemNetHttpPackages() {
			List<string> listProjects=NuGetFiles.ListSolutionLockFiles(TestPaths.RepoRoot)
				.Where(x => NuGetFiles.ReadLockFile(x).Any(y => y.Id.Equals("System.Net.Http",StringComparison.OrdinalIgnoreCase)))
				.Select(x => Path.GetDirectoryName(x)).ToList();
			Assert.IsTrue(listProjects.Count>=40,"The projects that get Helix Toolkit through SparksToothChart: "+listProjects.Count);
			Assert.IsTrue(listProjects.Any(x => Path.GetFileName(x)=="OpenPmsDental"),"The program gets it.");
			List<string> listProblems=new List<string>();
			int countBuilt=0;
			foreach(string folderProject in listProjects) {
				foreach(string configuration in new[] { "Debug","Release" }) {
					string folderBin=Path.Combine(folderProject,"bin",configuration);
					if(!Directory.Exists(folderBin)) {
						continue;
					}
					countBuilt++;
					foreach(string fileName in _arrayFrameworkFiles.Where(x => File.Exists(Path.Combine(folderBin,x)))) {
						listProblems.Add(folderBin.Substring(TestPaths.RepoRoot.Length)+"\\"+fileName);
					}
					if(Directory.Exists(Path.Combine(folderBin,"runtimes"))) {
						listProblems.Add(folderBin.Substring(TestPaths.RepoRoot.Length)+"\\runtimes");
					}
				}
			}
			Assert.AreEqual(0,listProblems.Count,"Files of the System.* 4.3.x packages in built folders:\r\n"+string.Join("\r\n",listProblems));
			if(!Directory.Exists(FolderProgram)) {
				BuildOutput.NotBuilt(@"OpenPmsDental\bin\Debug is not built ("+countBuilt+" built folders checked). Build OpenDental.Local.slnf (Debug|x86) first.");
			}
		}

		///<summary>No compilation of a project of the solution filter (never another worktree's) that gets CommunityToolkit.Mvvm has its analyzers
		///(source generators and code fixes): each SDK-style
		///compilation records the analyzers it is handed (Directory.Build.targets, ODRecordCompilerAnalyzers, obj\&lt;configuration&gt;\
		///&lt;project&gt;.analyzers.txt), and no record names one of CommunityToolkit.Mvvm, or the generators of Microsoft.Extensions.Logging.Abstractions
		///and System.Text.Json, which Directory.Packages.props turns off (Helix's buildTransitive files stay out, so those packages' switches must
		///still come with them from the other projects).</summary>
		[TestMethod]
		public void Compilations_GetNoCommunityToolkitMvvmAnalyzer() {
			string targets=TestPaths.ReadSource("Directory.Build.targets");
			StringAssert.Contains(targets,"<Target Name=\"ODRemoveCommunityToolkitMvvmAnalyzers\" AfterTargets=\"ResolvePackageDependenciesForBuild;ResolveNuGetPackageAssets\">");
			StringAssert.Contains(targets,"Condition=\"'%(Analyzer.NuGetPackageId)' == 'CommunityToolkit.Mvvm'\"");
			List<string> listProjects=NuGetFiles.ListSolutionLockFiles(TestPaths.RepoRoot)
				.Where(x => NuGetFiles.ReadLockFile(x).Any(y => y.Id.Equals("CommunityToolkit.Mvvm",StringComparison.OrdinalIgnoreCase)))
				.Select(x => Path.GetDirectoryName(x)).ToList();
			Assert.IsTrue(listProjects.Count>=40,"The projects that get CommunityToolkit.Mvvm through SparksToothChart: "+listProjects.Count);
			List<string> listProblems=new List<string>();
			List<string> listNotBuilt=new List<string>();
			foreach(string folderProject in listProjects) {
				string folderObj=Path.Combine(folderProject,"obj");
				string[] arrayRecords=Directory.Exists(folderObj) ? Directory.GetFiles(folderObj,"*.analyzers.txt",SearchOption.AllDirectories) : new string[0];
				if(arrayRecords.Length==0) {
					listNotBuilt.Add(folderProject.Substring(TestPaths.RepoRoot.Length));
					continue;
				}
				foreach(string pathRecord in arrayRecords) {
					foreach(string line in File.ReadAllLines(pathRecord).Where(x => x.Trim()!="")) {
						string packageId=line.Split('|')[0];
						if(new[] { "CommunityToolkit.Mvvm","Microsoft.Extensions.Logging.Abstractions","System.Text.Json" }.Contains(packageId,StringComparer.OrdinalIgnoreCase)) {
							listProblems.Add(pathRecord.Substring(TestPaths.RepoRoot.Length)+": "+line);
						}
					}
				}
			}
			Assert.AreEqual(0,listProblems.Count,"Analyzers that reach the compiler:\r\n"+string.Join("\r\n",listProblems));
			if(listNotBuilt.Count>0) {
				BuildOutput.NotBuilt(listNotBuilt.Count+" projects that get CommunityToolkit.Mvvm have no analyzer record ("+listNotBuilt[0]+", ...). Build "
					+"OpenDental.Local.slnf (Debug|x86) first.");
			}
		}

		///<summary>The Helix types the chart will use load in the program's own setting: a process whose application base holds the program folder's
		///files and whose config is OpenPmsDental.exe.config (its binding redirects). A MeshBuilder (HelixToolkit.Geometry, float, System.Numerics)
		///adds a box, ToMesh().ToWndMeshGeometry3D(true) makes it a WPF MeshGeometry3D (HelixToolkit.Wpf), on an STA thread. Every Helix and
		///toolkit assembly loads from the program's files; System.Net.Http, if anything loads it, and the other System.* 4.3.x names come from the
		///.NET Framework. The process (TestProbeHost; a new AppDomain on the program folder before S3a-8b of the .NET 10 move, which .NET cannot
		///create) runs from a scratch folder in the temp folder that holds the four Helix assemblies and every file of the program folder they
		///reference, directly or through each other (read with Mono.Cecil), so nothing is written into the program folder and nothing stays loaded
		///or locked here.</summary>
		[TestMethod]
		public void HelixTypes_TheChartWillUse_LoadFromTheProgramFolder() {
			string pathConfig=Path.Combine(FolderProgram,"OpenPmsDental.exe.config");
			if(!File.Exists(Path.Combine(FolderProgram,"HelixToolkit.Wpf.dll")) || !File.Exists(pathConfig)) {
				BuildOutput.NotBuilt(@"OpenPmsDental\bin\Debug is not built (HelixToolkit.Wpf.dll, OpenPmsDental.exe.config). Build OpenDental.Local.slnf (Debug|x86) first.");
				return;
			}
			string folderProbe=Path.Combine(Path.GetTempPath(),"releaseflow_helix_"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(folderProbe);
			string[] arrayLines;
			List<string> listCopied;
			try {
				listCopied=CopyWithReferences(FolderProgram,new[] { "HelixToolkit","HelixToolkit.Geometry","HelixToolkit.Maths","HelixToolkit.Wpf" },folderProbe);
				using(ProbeProcess probeProcess=ProbeProcess.Start(folderProbe,File.ReadAllText(pathConfig),typeof(HelixLoadProbe),TimeSpan.FromMinutes(3))) {
					arrayLines=ProbeText.SplitList(probeProcess.Call(HelixLoadProbe.COMMAND_RUN));
				}
			}
			finally {
				try {
					Directory.Delete(folderProbe,true);
				}
				catch(Exception) {
					//A file the ended child still held; the temp folder keeps it.
				}
			}
			string report="Copied from the program folder: "+string.Join(", ",listCopied)+"\r\n"+string.Join("\r\n",arrayLines);
			Console.WriteLine(report);
			Assert.AreEqual("ok",arrayLines[0],report);
			StringAssert.StartsWith(arrayLines[1],"positions=24 ","A box: four positions on each of its six faces. "+report);
			Dictionary<string,string> dictLoaded=arrayLines.Skip(2).Select(x => x.Split('|')).GroupBy(x => x[0],StringComparer.OrdinalIgnoreCase)
				.ToDictionary(x => x.Key,x => x.First()[2],StringComparer.OrdinalIgnoreCase);
			foreach(string name in new[] { "HelixToolkit","HelixToolkit.Geometry","HelixToolkit.Maths","HelixToolkit.Wpf" }) {
				Assert.IsTrue(dictLoaded.ContainsKey(name),name+" loaded: "+report);
				Assert.AreEqual(Path.Combine(folderProbe,name+".dll"),dictLoaded[name],true,name+" from the program folder's file (its copy in the probe's folder).");
			}
			//Only the copied closure is in the probe's folder (CopyWithReferences: the Helix assemblies and what they reference), so this sees a
			//System.* 4.3.x file only when something in that closure references it. A file of those packages that the program folder holds but no
			//copied assembly references is not seen here: BuiltFolders_HoldNoFileOfTheSystemNetHttpPackages checks the built folders themselves.
			foreach(string fileName in _arrayFrameworkFiles) {
				string name=Path.GetFileNameWithoutExtension(fileName);
				if(dictLoaded.TryGetValue(name,out string location)) {
					Assert.IsFalse(location.StartsWith(folderProbe,StringComparison.OrdinalIgnoreCase),name+" loaded from the program folder's files: "+location);
				}
			}
		}

		///<summary>Copies the named assemblies of folderFrom, and every assembly of folderFrom they reference, directly or through each other, into
		///folderTo; the references are read with Mono.Cecil from a copy of the bytes, so nothing is loaded or locked. Returns the files copied.
		///</summary>
		private static List<string> CopyWithReferences(string folderFrom,string[] arrayNames,string folderTo) {
			List<string> listCopied=new List<string>();
			Queue<string> queueNames=new Queue<string>(arrayNames);
			HashSet<string> hashSetSeen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			while(queueNames.Count>0) {
				string name=queueNames.Dequeue();
				if(!hashSetSeen.Add(name)) {
					continue;
				}
				string path=Path.Combine(folderFrom,name+".dll");
				if(!File.Exists(path)) {
					continue;//the .NET Framework's, or not there at all: the child's binding finds it, or fails, as the program's would
				}
				File.Copy(path,Path.Combine(folderTo,name+".dll"));
				listCopied.Add(name+".dll");
				using(MemoryStream memoryStream=new MemoryStream(File.ReadAllBytes(path)))
				using(Mono.Cecil.ModuleDefinition moduleDefinition=Mono.Cecil.ModuleDefinition.ReadModule(memoryStream)) {
					foreach(Mono.Cecil.AssemblyNameReference assemblyNameReference in moduleDefinition.AssemblyReferences) {
						queueNames.Enqueue(assemblyNameReference.Name);
					}
				}
			}
			return listCopied;
		}
	}

	///<summary>Runs in the child process of HelixTypes_TheChartWillUse_LoadFromTheProgramFolder (TestProbeHost). Only .NET Framework types in its
	///signature, so it loads there without the test framework; the Helix types are reached by reflection, as the program would bind them.
	///</summary>
	public static class HelixLoadProbe {
		public const string COMMAND_RUN="Run";

		///<summary>The child process's handler: Run, whose lines come back as one ProbeText list.</summary>
		public static string Handle(string command,string[] arrayArgs) {
			if(command==COMMAND_RUN) {
				return ProbeText.JoinList(Run());
			}
			throw new ArgumentException("Unknown command "+command);
		}

		///<summary>"ok" or the error; then "positions=N type"; then one "name|version|location" line per loaded assembly.</summary>
		public static string[] Run() {
			List<string> listLines=new List<string>();
			string result=null;
			Thread thread=new Thread(() => {
				try {
					result=MakeBox();
				}
				catch(Exception ex) {
					result="error: "+ex;
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
			if(result.StartsWith("positions=")) {
				listLines.Add("ok");
			}
			listLines.Add(result);
			foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic)) {
				AssemblyName assemblyName=assembly.GetName();
				listLines.Add(assemblyName.Name+"|"+assemblyName.Version+"|"+assembly.Location);
			}
			return listLines.ToArray();
		}

		private static string MakeBox() {
			Assembly assemblyGeometry=Assembly.Load("HelixToolkit.Geometry");
			Type typeMeshBuilder=assemblyGeometry.GetType("HelixToolkit.Geometry.MeshBuilder",true);
			object meshBuilder=Activator.CreateInstance(typeMeshBuilder);
			MethodInfo methodAddBox=typeMeshBuilder.GetMethods().Single(x => x.Name=="AddBox" && x.GetParameters().Length==4
				&& x.GetParameters()[1].ParameterType==typeof(float));
			Type typeVector3=methodAddBox.GetParameters()[0].ParameterType;
			if(typeVector3.FullName!="System.Numerics.Vector3") {
				return "AddBox takes "+typeVector3.FullName+", not System.Numerics.Vector3";
			}
			methodAddBox.Invoke(meshBuilder,new object[] { Activator.CreateInstance(typeVector3,1f,2f,3f),4f,5f,6f });
			object mesh=typeMeshBuilder.GetMethod("ToMesh",Type.EmptyTypes).Invoke(meshBuilder,null);
			Assembly assemblyWpf=Assembly.Load("HelixToolkit.Wpf");
			MethodInfo methodToWnd=assemblyWpf.GetExportedTypes().SelectMany(x => x.GetMethods(BindingFlags.Public|BindingFlags.Static))
				.Single(x => x.Name=="ToWndMeshGeometry3D" && x.GetParameters().Length==2 && x.GetParameters()[0].ParameterType.IsInstanceOfType(mesh)
					&& x.GetParameters()[1].ParameterType==typeof(bool));
			object meshWpf=methodToWnd.Invoke(null,new object[] { mesh,true });
			object positions=meshWpf.GetType().GetProperty("Positions").GetValue(meshWpf);
			int count=(int)positions.GetType().GetProperty("Count").GetValue(positions);
			return "positions="+count+" "+meshWpf.GetType().FullName+" (from "+methodToWnd.DeclaringType.FullName+")";
		}
	}
}
