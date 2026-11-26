// Tests for the v0.0 installer (docs/develop/release-process.md, "Building the installer"; docs/release-notes/v0.0.md): the list checks of
// tools/Build-Release.ps1, run against fake staging folders, and what installer/OpenPmsDental.iss and its information page must keep saying.
// The fake files are synthetic text. No setup is run here (tools/Test-Installer.ps1 does that, run elevated by the owner; here it only runs
// unelevated, where it refuses before any change) and no database is used.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ReleaseFlowTests {
	[TestClass]
	public class InstallerTests {
		///<summary>The attribution, word for word as in README.md, NOTICE and the About box (AttributionTests).</summary>
		private const string ATTRIBUTION="Open PMS - Dental is a fork of Open Dental (c) Open Dental Software, Inc. and contributors, licensed under "
			+"the GNU GPL v2. It is not affiliated with or endorsed by Open Dental Software, Inc.";

		///<summary>One file for each kind the release must never ship (report-release.md section 2, docs/port-notes/BIN-A.md and BIN-B.md): Open
		///Dental's closed binaries, the commercial and non-redistributable files, local settings, debug symbols, the uninstaller and the pre-rename
		///names.</summary>
		private static readonly string[] _arrayDenied=new string[] {
			"xImageDeviceManager.dll","SuniUSB.dll","VBbridges.dll","VideoCamLib.dll","Sparks3D.dll","Sparks3DXEngine.dll","EZT4Jpeg.dll","EZT4Pdf.dll",
			"Eztwain4.dll","Bridges.dll","CDT.dll","stdole.dll","Interop.Word.dll","Interop.Microsoft.Office.Core.dll","Interop.VBIDE.dll",
			"Interop.QBFC10.dll","AxInterop.AcroPDFLib.dll","Interop.AcroPDFLib.dll","Windows.winmd","installutil.exe","vcruntime140.dll","MSVCR71.dll",
			"RS232.dll","SigPlusNET.dll","zxing.dll","zxing.presentation.dll","Avalara.AvaTax.RestClient.net45.dll","Ionic.Zip.dll","TWAINDSM.dll",
			"Tamir.SharpSSH.dll","Org.Mentalis.Security.dll","DiffieHellman.dll","OpenPop.dll","BouncyCastle.Crypto.dll","Google.Apis.PlatformServices.dll",
			"Google.Apis.Auth.PlatformServices.dll","FreeDentalConfig.xml","OpenPmsDental.pdb","unins000.exe","unins000.dat","OpenDental.exe","OpenDentBusiness.dll",
			"Dicom.Core.dll","Dicom.Native.dll","fo-dicom.core.dll","NHunspell.dll","Hunspellx86.dll","Hunspellx64.dll",
			"PdfSharp.License.dll","NDde.dll",
		};

		///<summary>The program folder's documents, staged from the repository.</summary>
		private static readonly string[] _arrayDocuments=new string[] {
			"LICENSE","LICENSE-GPL-3.0.txt","LICENSE-AGPL-3.0.txt","LICENSE-LGPL-2.1.txt","NOTICE","THIRD-PARTY-NOTICES.txt",@"db\init\new-database.sql",
			@"db\README.md",
		};

		///<summary>The file the GNU LGPL 2.1 text ships as (LGPL 2.1 sections 1 and 4: a copy of the licence with every copy of the library).</summary>
		private const string LGPL_FILE="LICENSE-LGPL-2.1.txt";

		///<summary>SHA-256 of LICENSE-LGPL-2.1.txt with LF line ends: the FSF's text of the LGPL 2.1 as gnu.org publishes it, byte for byte the
		///copies that xz, gettext, libiconv and libtasn1 ship. A changed byte means the text is no longer the licence verbatim.</summary>
		private const string LGPL_SHA256_LF="20e50fe7aae3e56378ebf0417d9de904f55a0e61e4df315333e632a4d3555d95";

		///<summary>The components of THIRD-PARTY-NOTICES.txt that ship under the LGPL 2.1, and so with its text.</summary>
		private static readonly string[] _arrayLgplComponents=new string[] { "WeCantSpell.Hunspell","Hunspell en_US dictionary","RDL Project","TWAIN" };

		///<summary>SHA-256 of twain-dsm-2.5.1.zip, the TWAIN Working Group's source of the setup's TWAINDSM.dll (the approved download of
		///od-tools\downloads\s3b7-twaindsm-20261116), which the source archive carries.</summary>
		private const string TWAIN_DSM_SOURCE_SHA256="67e5aebda9c7aa85e39a38b8c9c8b99bd720d4b318134b3e274aec97d4977e0f";

		///<summary>Where the source archive carries the TWAIN DSM source, under its top folder.</summary>
		private const string TWAIN_DSM_SOURCE_ENTRY="third-party-source/twain-dsm-2.5.1.zip";

		#region Helpers
		private class ScriptResult {
			public int ExitCode;
			public string Output;
			public List<string> Lines;
		}

		private static string Quote(string arg) {
			return "\""+arg+"\"";
		}

		private static ScriptResult RunBuildRelease(params string[] arrayArgs) {
			return RunScript(Path.Combine(TestPaths.RepoRoot,"tools","Build-Release.ps1"),arrayArgs);
		}

		private static ScriptResult RunScript(string script,params string[] arrayArgs) {
			ProcessStartInfo processStartInfo=new ProcessStartInfo("powershell.exe");
			processStartInfo.Arguments="-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "+Quote(script)+" "+string.Join(" ",arrayArgs);
			processStartInfo.UseShellExecute=false;
			processStartInfo.RedirectStandardOutput=true;
			processStartInfo.RedirectStandardError=true;
			processStartInfo.CreateNoWindow=true;
			using(Process process=Process.Start(processStartInfo)) {
				Task<string> taskOut=process.StandardOutput.ReadToEndAsync();
				Task<string> taskErr=process.StandardError.ReadToEndAsync();
				if(!process.WaitForExit(120000)) {
					process.Kill();
					Assert.Fail(Path.GetFileName(script)+" did not finish in 2 minutes.");
				}
				process.WaitForExit();
				ScriptResult scriptResult=new ScriptResult();
				scriptResult.ExitCode=process.ExitCode;
				scriptResult.Output=taskOut.Result+taskErr.Result;
				scriptResult.Lines=scriptResult.Output.Split(new[] { "\r\n","\n" },StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
				return scriptResult;
			}
		}

		///<summary>The allow-list as Build-Release.ps1 -ListFiles prints it: kind ("own", "third", "doc", "runtime", and "setup" for the file the setup
		///installs outside the program folder) and path.</summary>
		private static List<KeyValuePair<string,string>> ListFiles() {
			ScriptResult scriptResult=RunBuildRelease("-ListFiles");
			Assert.AreEqual(0,scriptResult.ExitCode,scriptResult.Output);
			List<KeyValuePair<string,string>> listFiles=new List<KeyValuePair<string,string>>();
			foreach(string line in scriptResult.Lines) {
				string[] arrayParts=line.Split('|');
				Assert.AreEqual(2,arrayParts.Length,"Unexpected line: "+line);
				listFiles.Add(new KeyValuePair<string,string>(arrayParts[0],arrayParts[1]));
			}
			return listFiles;
		}

		///<summary>A fake staging folder with every allowed file of the program folder (not the setup's TWAINDSM.dll): synthetic text named after the
		///file, and the real THIRD-PARTY-NOTICES.txt.</summary>
		private static string NewFakeStaging(string name) {
			string folder=TestPaths.NewTempFolder(name);
			foreach(KeyValuePair<string,string> pair in ListFiles().Where(x => x.Key!="setup")) {
				string path=Path.Combine(folder,pair.Value);
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				if(pair.Value=="THIRD-PARTY-NOTICES.txt") {
					File.Copy(Path.Combine(TestPaths.RepoRoot,"THIRD-PARTY-NOTICES.txt"),path);
				}
				else {
					File.WriteAllText(path,"synthetic "+pair.Value);
				}
			}
			return folder;
		}

		///<summary>The list checks of Build-Release.ps1 on the folder.</summary>
		private static ScriptResult CheckFolder(string folder) {
			return RunBuildRelease("-CheckFolder",Quote(folder));
		}

		private static void DeleteFolder(string folder) {
			try {
				Directory.Delete(folder,true);
			}
			catch(IOException) { }
			catch(UnauthorizedAccessException) { }
		}

		private static string ReadIss() {
			return TestPaths.ReadSource("installer","OpenPmsDental.iss");
		}

		///<summary>The value of a [Setup] directive (the first line "Name=value").</summary>
		private static string SetupDirective(string iss,string name) {
			Match match=Regex.Match(iss,"(?m)^"+Regex.Escape(name)+@"=(.*?)\r?$");
			Assert.IsTrue(match.Success,"No [Setup] directive "+name);
			return match.Groups[1].Value.Trim();
		}

		private static string ReleaseVersion() {
			Match match=Regex.Match(TestPaths.ReadSource("SharedAssemblyInfo.cs"),@"AssemblyInformationalVersion\(""([^""]+)""\)");
			Assert.IsTrue(match.Success);
			return match.Groups[1].Value;
		}
		#endregion Helpers

		#region Build-Release.ps1 lists
		[TestMethod]
		public void AllowList_ShipsTheRuntimeFiles_TheDocuments_AndEveryBuildIdFile() {
			List<KeyValuePair<string,string>> listFiles=ListFiles();
			List<string> listPaths=listFiles.Select(x => x.Value).ToList();
			//Each .NET 10 exe is its apphost, its assembly, its .runtimeconfig.json and .deps.json, and an app.config becomes <name>.dll.config
			//(S3b-4, S3b-7); no *.exe.config is staged (section 7 of docs/specs/net10-plan.md, "Config file name").
			foreach(string name in new[] { "WebView2Loader.dll","WeCantSpell.Hunspell.dll","ODR.dll","OpenPmsDental.exe","OpenPmsDental.dll","OpenPmsDental.dll.config",
				"OpenPmsDental.runtimeconfig.json","OpenPmsDental.deps.json","UpdateFileCopier.exe","UpdateFileCopier.dll","UpdateFileCopier.runtimeconfig.json",
				"UpdateFileCopier.deps.json","ImagingDeviceManager.dll","ImagingCapture.dll","RdlEngine.dll","RdlViewer.dll","Direct2dWrapper.dll",
				ProgramNames.DbSettingsExeFileName,"OpenPmsDentalDbSettings.dll.config","Microsoft.CodeAnalysis.dll","Microsoft.CodeAnalysis.VisualBasic.dll" })
			{
				CollectionAssert.Contains(listPaths,name);
			}
			Assert.IsFalse(listPaths.Any(x => x.EndsWith(".exe.config",StringComparison.OrdinalIgnoreCase)),"No *.exe.config is staged.");
			//The runtime part: the x64 runtime beside the programs and the helper's x86 runtime in Helper32\, from the runtime packs' lists.
			foreach(string name in new[] { "coreclr.dll","hostfxr.dll","hostpolicy.dll","System.Private.CoreLib.dll","PresentationFramework.dll",
				@"Helper32\coreclr.dll",@"Helper32\hostfxr.dll",@"Helper32\System.Windows.Forms.dll" })
			{
				Assert.IsTrue(listFiles.Any(x => x.Key=="runtime" && x.Value==name),"Not on the runtime list: "+name);
			}
			Assert.IsFalse(listFiles.Any(x => x.Key=="runtime" && x.Value==@"Helper32\PresentationFramework.dll"),"The helper takes WinForms only.");
			//TWAINDSM.dll is a setup file (the 64-bit system folder), never a file of the program folder; so is the 32-bit one (the 32-bit system
			//folder, S3b-8), in its own sub folder of the setup files.
			CollectionAssert.AreEqual(new[] { "TWAINDSM.dll",@"x86\TWAINDSM.dll" },listFiles.Where(x => x.Key=="setup").Select(x => x.Value).ToArray());
			foreach(string path in _arrayDocuments) {
				Assert.IsTrue(listFiles.Any(x => x.Key=="doc" && x.Value==path),"Not staged: "+path);
			}
			foreach(string name in ForkReleases.ListBuildIdFiles) {
				Assert.IsTrue(listFiles.Any(x => x.Key=="own" && x.Value==name),"A build id file is not shipped as the program's own: "+name);
			}
			Assert.AreEqual(listPaths.Count,listPaths.Distinct(StringComparer.OrdinalIgnoreCase).Count(),"No file twice.");
			foreach(string path in listFiles.Where(x => x.Key!="setup").Select(x => x.Value)) {
				string name=Path.GetFileName(path);
				Assert.IsFalse(_arrayDenied.Contains(name,StringComparer.OrdinalIgnoreCase),"Allowed although denied: "+path);
				Assert.IsFalse(name.EndsWith(".pdb",StringComparison.OrdinalIgnoreCase),path);
				Assert.IsFalse(name.StartsWith("unins",StringComparison.OrdinalIgnoreCase),"The uninstaller is not staged: "+path);
				Assert.IsFalse(name.StartsWith("OpenDent",StringComparison.OrdinalIgnoreCase),"A pre-rename name: "+path);
			}
			//The setup files are not files of the program folder (setup|x86\TWAINDSM.dll goes into the 32-bit system folder, S3b-8).
			Assert.IsTrue(listFiles.Where(x => x.Key!="doc" && x.Key!="setup").All(x => !x.Value.Contains("\\") || x.Value.StartsWith(@"Helper32\")),
				"The program's files are all at the top level, but for the one sub folder Helper32\\.");
			Assert.IsTrue(listFiles.Where(x => x.Key=="own" && x.Value.Contains("\\")).All(x => x.Value.StartsWith(@"Helper32\OpenPmsDentalHelper32.")),
				"Only the helper's own files are in Helper32\\.");
		}

		[TestMethod]
		public void CheckFolder_PassesForTheAllowList_AndPrintsTheBuildIdOfForkReleases() {
			string folder=NewFakeStaging("pass");
			try {
				ScriptResult scriptResult=CheckFolder(folder);
				Assert.AreEqual(0,scriptResult.ExitCode,scriptResult.Output);
				CollectionAssert.Contains(scriptResult.Lines,"The check passed.");
				//The fake build id files differ from each other, so a list that differs from ForkReleases.ListBuildIdFiles gives another id.
				CollectionAssert.Contains(scriptResult.Lines,"Build id: "+ForkReleases.ComputeBuildId(folder),scriptResult.Output);
			}
			finally {
				DeleteFolder(folder);
			}
		}

		[TestMethod]
		public void AllowList_ShipsPdfSharpFromTheSourceBuild_AndNoLicenceDll() {
			//PDFsharp and MigraDoc are built from empira's source with an open licence check (docs/develop/third-party-source-builds.md): the closed
			//PdfSharp.License.dll is not on the allow-list but on the deny-list, and an otherwise complete staging folder passes.
			Assert.IsFalse(ListFiles().Any(x => x.Value=="PdfSharp.License.dll"),"PdfSharp.License.dll is not on the allow-list.");
			string script=TestPaths.ReadSource("tools","Build-Release.ps1");
			StringAssert.Contains(script,@"$ThirdPartyFiles[$name+'.dll']='third-party\build\'+$name+'\bin\Release'");
			Assert.IsFalse(script.Contains("PdfSharpLicenceResolved"),"No release blocker any more.");
		}

		[TestMethod]
		public void CheckFolder_FailsForEveryDeniedFile_AnUnknownFile_AndASparks3DFolder() {
			string folder=NewFakeStaging("deny");
			try {
				foreach(string name in _arrayDenied) {
					File.WriteAllText(Path.Combine(folder,name),"synthetic");
				}
				File.WriteAllText(Path.Combine(folder,"Unknown.dll"),"synthetic");
				Directory.CreateDirectory(Path.Combine(folder,"Sparks3D"));
				File.WriteAllText(Path.Combine(folder,"Sparks3D","readme.txt"),"synthetic");
				Directory.CreateDirectory(Path.Combine(folder,"uninstall"));
				ScriptResult scriptResult=CheckFolder(folder);
				Assert.AreEqual(1,scriptResult.ExitCode,scriptResult.Output);
				foreach(string name in _arrayDenied) {
					Assert.IsTrue(scriptResult.Lines.Any(x => x.StartsWith("Denied file: "+name+" (")),"Not refused: "+name+"\r\n"+scriptResult.Output);
				}
				CollectionAssert.Contains(scriptResult.Lines,"Not on the allow-list: Unknown.dll");
				Assert.IsTrue(scriptResult.Lines.Any(x => x.StartsWith("Denied folder: Sparks3D (")),scriptResult.Output);
				CollectionAssert.Contains(scriptResult.Lines,"Unexpected folder: uninstall");
				Assert.IsFalse(scriptResult.Lines.Any(x => x.StartsWith("Missing:")),"Every allowed file is there.\r\n"+scriptResult.Output);
				Assert.IsFalse(scriptResult.Lines.Any(x => x.StartsWith("Not named in THIRD-PARTY-NOTICES.txt")),scriptResult.Output);
			}
			finally {
				DeleteFolder(folder);
			}
		}

		///<summary>A file that an earlier release shipped and this one does not (ForkReleases.ListRetiredFiles, such as the storage library of
		///v0.0) is never staged: it is not on the allow-list, and a leftover copy in the build output or the staging folder is refused with a
		///reason.</summary>
		[TestMethod]
		public void CheckFolder_RefusesARetiredFile() {
			Assert.IsTrue(ForkReleases.ListRetiredFiles.Count>0);
			List<string> listPaths=ListFiles().Select(x => x.Value).ToList();
			foreach(string name in ForkReleases.ListRetiredFiles) {
				Assert.IsFalse(listPaths.Contains(name,StringComparer.OrdinalIgnoreCase),"A retired file is on the allow-list: "+name);
			}
			CollectionAssert.Contains(listPaths,ProgramNames.StorageDllFileName,"The storage library ships under its new name.");
			string folder=NewFakeStaging("retired");
			try {
				foreach(string name in ForkReleases.ListRetiredFiles) {
					File.WriteAllText(Path.Combine(folder,name),"synthetic leftover");
				}
				ScriptResult scriptResult=CheckFolder(folder);
				Assert.AreEqual(1,scriptResult.ExitCode,scriptResult.Output);
				foreach(string name in ForkReleases.ListRetiredFiles) {
					Assert.IsTrue(scriptResult.Lines.Any(x => x.StartsWith("Denied file: "+name+" (")),"Not refused with a reason: "+name+"\r\n"+scriptResult.Output);
				}
				Assert.IsFalse(scriptResult.Lines.Any(x => x.StartsWith("Missing:")),scriptResult.Output);
			}
			finally {
				DeleteFolder(folder);
			}
		}

		[TestMethod]
		public void CheckFolder_FailsWhenTheRuntimeFilesOrTheDocumentsAreMissing() {
			string folder=NewFakeStaging("missing");
			try {
				foreach(string path in new[] { "WebView2Loader.dll","WeCantSpell.Hunspell.dll","ODR.dll","LICENSE",LGPL_FILE,@"db\init\new-database.sql" }) {
					File.Delete(Path.Combine(folder,path));
				}
				ScriptResult scriptResult=CheckFolder(folder);
				Assert.AreEqual(1,scriptResult.ExitCode,scriptResult.Output);
				foreach(string name in new[] { "WebView2Loader.dll","WeCantSpell.Hunspell.dll","ODR.dll",LGPL_FILE }) {
					Assert.IsTrue(scriptResult.Lines.Any(x => x.StartsWith("Missing: "+name+" (")),"No reason for "+name+"\r\n"+scriptResult.Output);
				}
				CollectionAssert.Contains(scriptResult.Lines,"Missing: LICENSE");
				CollectionAssert.Contains(scriptResult.Lines,@"Missing: db\init\new-database.sql");
			}
			finally {
				DeleteFolder(folder);
			}
		}

		[TestMethod]
		public void CheckFolder_FailsWhenTheNoticesDoNotNameAShippedThirdPartyFile() {
			string folder=NewFakeStaging("notices");
			try {
				string path=Path.Combine(folder,"THIRD-PARTY-NOTICES.txt");
				string notices=File.ReadAllText(path);
				StringAssert.Contains(notices,"Files:     Renci.SshNet.dll");
				File.WriteAllText(path,notices.Replace("Files:     Renci.SshNet.dll","Files:     SSH.NET (removed for the test)"));
				ScriptResult scriptResult=CheckFolder(folder);
				Assert.AreEqual(1,scriptResult.ExitCode,scriptResult.Output);
				CollectionAssert.Contains(scriptResult.Lines,"Not named in THIRD-PARTY-NOTICES.txt: Renci.SshNet.dll");
				Assert.AreEqual(1,scriptResult.Lines.Count(x => x.StartsWith("Not named in")),"Only that file.\r\n"+scriptResult.Output);
			}
			finally {
				DeleteFolder(folder);
			}
		}

		[TestMethod]
		public void LgplText_IsTheLicenceVerbatim_AndEveryLgplComponentPointsToIt() {
			//RdlEngine.dll, RdlViewer.dll, WeCantSpell.Hunspell.dll, the en_US word list and the setup's TWAINDSM.dll ship under the LGPL 2.1, which
			//asks for a copy of the licence with every copy of the library. The release stages the text (the allow-list test above) from this file.
			string text=TestPaths.ReadSource(LGPL_FILE).Replace("\r\n","\n");
			StringAssert.StartsWith(text.TrimStart(),"GNU LESSER GENERAL PUBLIC LICENSE");
			StringAssert.Contains(text,"Version 2.1, February 1999");
			StringAssert.Contains(text,"TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION");
			StringAssert.Contains(text,"END OF TERMS AND CONDITIONS");
			string sha256;
			using(SHA256 sha=SHA256.Create()) {
				sha256=string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(text)).Select(x => x.ToString("x2")));
			}
			Assert.AreEqual(LGPL_SHA256_LF,sha256,"The LGPL 2.1 text, unchanged.");
			string notices=TestPaths.ReadSource("THIRD-PARTY-NOTICES.txt").Replace("\r\n","\n");
			foreach(string component in _arrayLgplComponents) {
				Match match=Regex.Match(notices,@"(?m)^--- Component: "+Regex.Escape(component)+@"\n(?:(?!^--- ).*\n)*?License:(.*\n(?:[ \t]+\S.*\n)*)");
				Assert.IsTrue(match.Success,"No License field for "+component);
				StringAssert.Contains(match.Groups[1].Value,LGPL_FILE,component);
			}
			//The tri-licensed WeCantSpell.Hunspell ships under its GPL/LGPL option, so no MPL 1.1 text is needed.
			StringAssert.Contains(notices,"This release uses it under the GPL / LGPL");
			StringAssert.Contains(TestPaths.ReadSource("NOTICE"),LGPL_FILE);
			StringAssert.Contains(TestPaths.ReadSource("installer","setup-info.txt"),LGPL_FILE);
		}

		[TestMethod]
		public void Direct2dWrapper_EachConfigurationBuildsIntoItsOwnFolder() {
			//With one bin\ for Debug and Release, an incremental Release build after a Debug build copied the Debug DLL (debug C runtime) into
			//OpenPmsDental\bin\Release. Build-Release.ps1 compares the staged DLL with Direct2dWrapper\bin\Release.
			//Read as XML without the MSBuild namespace, so the layout of the file (one line or several, the 2003 namespace or none) does not matter.
			System.Xml.Linq.XDocument xDocument=System.Xml.Linq.XDocument.Parse(TestPaths.ReadSource("Direct2dWrapper","Direct2dWrapper.vcxproj"));
			//The folder each configuration resolves to: the OutDir of its property group, with $(Configuration) and $(Platform) expanded.
			Dictionary<string,string> dictOutDirs=new Dictionary<string,string>();
			foreach(System.Xml.Linq.XElement xElementGroup in xDocument.Root.Elements().Where(x => x.Name.LocalName=="PropertyGroup")) {
				Match match=Regex.Match((string)xElementGroup.Attribute("Condition")??"",@"^\s*'\$\(Configuration\)\|\$\(Platform\)'\s*==\s*'(\w+)\|(\w+)'\s*$");
				System.Xml.Linq.XElement xElementOutDir=xElementGroup.Elements().FirstOrDefault(x => x.Name.LocalName=="OutDir");
				if(!match.Success || xElementOutDir==null) {
					continue;
				}
				dictOutDirs.Add(match.Groups[1].Value+"|"+match.Groups[2].Value,
					xElementOutDir.Value.Trim().Replace("$(Configuration)",match.Groups[1].Value).Replace("$(Platform)",match.Groups[2].Value));
			}
			//Since S3b-4 the project has x64 configurations only (the Win32 ones went with the x86 program); the release takes the x64 Release build.
			CollectionAssert.AreEquivalent(new[] { "Debug|x64","Release|x64" },dictOutDirs.Keys.ToArray());
			Assert.AreEqual(2,dictOutDirs.Values.Distinct(StringComparer.OrdinalIgnoreCase).Count(),"One output folder per configuration.");
			Assert.AreEqual(@"$(ProjectDir)bin\x64\Release\",dictOutDirs["Release|x64"]);
			StringAssert.Contains(TestPaths.ReadSource("tools","Build-Release.ps1"),@"'Direct2dWrapper.dll'='Direct2dWrapper\bin\x64\Release'");
		}

		[TestMethod]
		public void BuildRelease_PinsTheCompatibilityVersion() {
			string script=TestPaths.ReadSource("tools","Build-Release.ps1");
			Match match=Regex.Match(script,@"(?m)^\$CompatVersion='([^']+)'");
			Assert.IsTrue(match.Success);
			Assert.AreEqual(ForkVersions.CompatVersion.ToString(4),match.Groups[1].Value);
		}
		#endregion Build-Release.ps1 lists

		#region installer/OpenPmsDental.iss
		[TestMethod]
		public void Iss_NamesTheProduct_TheReleaseVersion_AndItsOwnFolder() {
			string iss=ReadIss();
			string version=ReleaseVersion();
			StringAssert.Contains(iss,"#define AppVersion \""+version+"\"");
			StringAssert.Contains(iss,"#define AppName \"Open PMS - Dental\"");
			Assert.AreEqual("{#AppName}",SetupDirective(iss,"AppName"));
			Assert.AreEqual("{#AppVersion}",SetupDirective(iss,"AppVersion"));
			Assert.AreEqual(version+".0.0",SetupDirective(iss,"VersionInfoVersion"));
			//A 64-bit install into Program Files (decision 19, S3b-7).
			Assert.AreEqual(@"{commonpf64}\Open PMS - Dental",SetupDirective(iss,"DefaultDirName"));
			Assert.AreEqual(ProgramNames.DefaultInstallFolder,@"C:\Program Files\Open PMS - Dental");
			Assert.AreEqual("OpenPmsDental-{#AppVersion}-Setup",SetupDirective(iss,"OutputBaseFilename"));
			//The License Agreement page is the text Build-Release.ps1 writes from installer\setup-license.txt (LicensePage_ test below), not the
			//GPL version 2 of LICENSE: as installed, with PDFsharp and MigraDoc, the program is conveyed under the GPL version 3.
			Assert.AreEqual("{#LicenseTextFile}",SetupDirective(iss,"LicenseFile"));
			StringAssert.Contains(iss,"#define LicenseTextFile \"bin\\setup-license.txt\"");
			Assert.AreEqual("{#InfoFile}",SetupDirective(iss,"InfoBeforeFile"));
			StringAssert.Contains(iss,"#define ExeName \""+ProgramNames.ExeFileName+"\"");
			Assert.IsTrue(Regex.IsMatch(SetupDirective(iss,"AppId"),@"^\{\{[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}\}$"),"AppId is a GUID of its own.");
			Assert.IsFalse(iss.Contains(@"\Open Dental"),"Never Open Dental's folder.");
		}

		[TestMethod]
		public void Iss_AlwaysInstallsPerMachine_HasNoPerUserMode_AndKeepsTheUninstallerOutOfTheTopLevel() {
			string iss=ReadIss();
			Assert.AreEqual("admin",SetupDirective(iss,"PrivilegesRequired"));
			//No per-user mode (decision 7 of docs/specs/net10-plan.md, S3a-11): no override of the privileges, on the command line or in a dialog,
			//so {autoprograms} and {autodesktop} are always the common folders and the connection settings tool can always run.
			Assert.IsFalse(Regex.IsMatch(iss,@"(?m)^\s*PrivilegesRequiredOverridesAllowed\s*="),"No PrivilegesRequiredOverridesAllowed.");
			Assert.IsFalse(iss.IndexOf("/CURRENTUSER",StringComparison.OrdinalIgnoreCase)>=0,"No /CURRENTUSER, not even in the notes.");
			Assert.IsFalse(iss.IndexOf("IsAdminInstallMode",StringComparison.Ordinal)>=0,"No branch for a non-administrative install.");
			Assert.IsFalse(iss.IndexOf("IsAdmin",StringComparison.Ordinal)>=0,"No check of the install mode at all.");
			StringAssert.Contains(iss,@"Name: ""{autodesktop}\{#AppName}""");
			Assert.AreEqual(@"{app}\uninstall",SetupDirective(iss,"UninstallFilesDir"));
			StringAssert.Contains(iss,"procedure RemoveProgramFolder;");
			Assert.IsFalse(iss.Contains("[UninstallDelete]"),"The program folder is removed in code, which keeps an A to Z folder.");
			StringAssert.Contains(iss,"function CheckInstallFolder(");
		}

		///<summary>UpdateFileCopier never deletes a file, so setup is what removes the files of an earlier release that this one no longer ships
		///(ForkReleases.ListRetiredFiles): an upgrade deletes each one from the program folder, and nothing else.</summary>
		[TestMethod]
		public void Iss_DeletesTheRetiredFilesOnAnUpgrade() {
			string iss=ReadIss();
			Match match=Regex.Match(iss,@"(?ms)^\[InstallDelete\]\r?\n(.*?)(?=^\[)");
			Assert.IsTrue(match.Success,"No [InstallDelete] section before the next section.");
			List<string> listEntries=match.Groups[1].Value.Split('\n').Select(x => x.Trim()).Where(x => x!="" && !x.StartsWith(";")).ToList();
			List<string> listExpected=ForkReleases.ListRetiredFiles.Select(x => @"Type: files; Name: ""{app}\"+x+@"""").ToList();
			CollectionAssert.AreEquivalent(listExpected,listEntries,"One entry per retired file, and only files in the program folder.");
			Assert.AreEqual(1,Regex.Matches(iss,@"(?m)^\[InstallDelete\]").Count);
			//Setup never deletes a file this release installs: CheckFolder_RefusesARetiredFile keeps every retired file off the allow-list.
			foreach(string name in ForkReleases.ListRetiredFiles) {
				Assert.IsFalse(ForkReleases.ListBuildIdFiles.Contains(name,StringComparer.OrdinalIgnoreCase),name);
			}
		}

		[TestMethod]
		public void Iss_NeverStartsTheProgram() {
			string iss=ReadIss();
			Assert.IsFalse(Regex.IsMatch(iss,@"(?m)^\[Run\]"),"No [Run] section.");
			Assert.IsFalse(Regex.IsMatch(iss,@"(?m)^\[UninstallRun\]"),"No [UninstallRun] section.");
			Assert.AreEqual("no",SetupDirective(iss,"RestartApplications"));
		}

		[TestMethod]
		public void Iss_WritesTheConfigWithoutAPassword_AndChangesNoPermissions() {
			//Only an administrator changes the connection settings, with the connection settings tool (docs/specs/db-connection-security.md), so
			//the config file no longer gives Users modify rights.
			string iss=ReadIss();
			Assert.IsFalse(iss.Split('\n').Any(x => x.Contains("Permissions:")),"No entry changes permissions.");
			string configLine=iss.Split('\n').Single(x => x.StartsWith(@"Source: ""{tmp}\{#ConfigFileName}"""));
			StringAssert.Contains(configLine,"onlyifdoesntexist");
			StringAssert.Contains(iss,"#define ConfigFileName \""+ProgramNames.ConfigFileName+"\"");
			Assert.IsFalse(Regex.IsMatch(iss,@"(?m)^\[Dirs\]"),"No folder permissions.");
			StringAssert.Contains(iss,"'        <Password></Password>'");
			StringAssert.Contains(iss,"'        <MySQLPassHash></MySQLPassHash>'");
			StringAssert.Contains(iss,"'        <NoShowOnStartup>False</NoShowOnStartup>'");
			foreach(string param in new[] { "{param:DBSERVER|","{param:DBNAME|","{param:DBUSER|","{param:PREREQS|","{param:DBCA|","{param:DBPFX|",
				"{param:DBTHUMBPRINT|","{param:DBSECRETSFILE|" })
			{
				StringAssert.Contains(iss,param);
			}
		}

		[TestMethod]
		public void Iss_LimitsTheFoldersOfUnfiledImages_ToSystemAdministratorsAndTheProgramsUsers() {
			//IMG-ACL (docs/port-notes/IMG-ACL.md): as ImagingFolderAcl does in the program (ImagingFolderAclTests), on the folders that exist.
			string iss=ReadIss();
			StringAssert.Contains(iss,"ImagingAccessGroup = 'OpenPMS-Dental Users';");
			StringAssert.Contains(iss,"'localgroup ' + AddQuotes(ImagingAccessGroup)");
			StringAssert.Contains(iss,"AddQuotes(GetComputerNameString + '\\' + ImagingAccessGroup + ':(OI)(CI)M')");
			StringAssert.Contains(iss,"Result := '*S-1-5-32-545:(OI)(CI)M';");
			StringAssert.Contains(iss,"' /inheritance:r /grant:r *S-1-5-18:(OI)(CI)F *S-1-5-32-544:(OI)(CI)F ' + AccessGrant + ' /Q'");
			StringAssert.Contains(iss,"AddQuotes(Dirs[I]) + ' /reset /Q'");
			//Never through a junction or link: no /T, and nothing changed when one is in the path or inside (an ordinary user can put one there).
			Assert.IsFalse(Regex.IsMatch(iss,@"(?i)'[^'\n]*\s/T[\s']"),"icacls never gets /T.");
			StringAssert.Contains(iss,"AttrReparsePoint = $400;");
			StringAssert.Contains(iss,"if IsReparsePoint(Parent) then begin");
			StringAssert.Contains(iss,"if not ListFoldersWithoutReparsePoints(Dir, Dirs) then begin");
			Assert.IsTrue(iss.IndexOf("' /inheritance:r /grant:r",StringComparison.Ordinal)<iss.IndexOf("' /reset /Q'",StringComparison.Ordinal),
				"The folder's own list first, then the folders inside reset to inherit it, so a failure never leaves it wider than before.");
			StringAssert.Contains(iss,"ExpandConstant('{commonappdata}\\OpenPMS-Dental\\Imaging')");
			foreach(string subfolder in new[] { @"\Photos\unsaved",@"\Video\recording",@"\Capture" }) {
				StringAssert.Contains(iss,"LimitImagingFolder(Imaging + '"+subfolder+"', AccessGrant);");
			}
			StringAssert.Contains(iss,"if not DirExists(Dir) then");
			Assert.IsTrue(Regex.IsMatch(iss,@"CurStep = ssPostInstall then begin\s+LimitImagingFolders;\s+SetUpConnection;"),
				"After the files are in, before the connection settings.");
			Assert.IsFalse(iss.Contains("/add"),"Setup never makes the group or adds anyone to one.");
		}

		[TestMethod]
		public void Iss_SetsUpTheConnectionWithTheSettingsTool_AndNeverPassesASecret() {
			string iss=ReadIss();
			StringAssert.Contains(iss,"#define DbSettingsExeName \""+ProgramNames.DbSettingsExeFileName+"\"");
			StringAssert.Contains(iss,"procedure SetUpConnection;");
			StringAssert.Contains(iss,"CurStep = ssPostInstall");
			StringAssert.Contains(iss,"RunDbSettings('/check', False)");
			StringAssert.Contains(iss,"' /secretsfile='");
			//Every install is an administrative one, so a new connection to a server over the network always runs the tool: /silent in a silent
			//install (tools/Test-Installer.ps1, scenario D), its window otherwise.
			Match matchSetUp=Regex.Match(iss,@"(?ms)^procedure SetUpConnection;\r?\n(.*?)^end;");
			Assert.IsTrue(matchSetUp.Success);
			string setUp=matchSetUp.Groups[1].Value.Replace("\r\n","\n");
			StringAssert.Contains(setUp,"NeedsTool := (not IsLoopbackServer(ConfigServer)) or HasSecuritySwitches;\n    if NeedsTool then begin\n      if WizardSilent then\n"
				+"        ToolExit := RunDbSettings(DbSettingsParams + ' /silent', False)");
			Assert.IsFalse(setUp.Contains("needs the connection settings tool, which needs administrator rights"),"No per-user message.");
			//The tool refuses secrets on its command line; setup never builds one.
			foreach(string forbidden in new[] { "/password=","/dbpassword=","/pfxpassword=","{param:DBPASSWORD","{param:PFXPASSWORD" }) {
				Assert.IsFalse(iss.IndexOf(forbidden,StringComparison.OrdinalIgnoreCase)>=0,"The script names "+forbidden);
			}
		}

		///<summary>The setup's prerequisites since stage 3b (decisions 6, 8, 19 and 20; S3b-7): no .NET runtime, no .NET Framework gate (the program
		///carries its own runtime), the x64 Visual C++ redistributable checked in both registry views and downloaded only from Microsoft with the same
		///signature check, the WebView2 runtime only checked, and the warning below Windows 11 with MinVersion as the hard floor.</summary>
		[TestMethod]
		public void Iss_ChecksThePrerequisites_AndDownloadsOnlyFromMicrosoft() {
			string iss=ReadIss();
			Assert.IsFalse(iss.Contains("NetFx48"),"No .NET Framework gate.");
			Assert.IsFalse(Regex.IsMatch(iss,@"(?i)dotnet-runtime|windowsdesktop-runtime|dotnet\.microsoft\.com/download"),"No .NET runtime download.");
			StringAssert.Contains(iss,@"VcRedistKey = 'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64';");
			StringAssert.Contains(iss,"VcRedistUrl = 'https://aka.ms/vs/17/release/vc_redist.x64.exe';");
			StringAssert.Contains(iss,"Result := HasVcRedistKey(HKLM64) or HasVcRedistKey(HKLM32);");
			Assert.IsFalse(iss.Contains("vc_redist.x86"),"The x86 redistributable went with the x86 program.");
			StringAssert.Contains(iss,@"'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}'");
			StringAssert.Contains(iss,"function IsSignedByMicrosoft(");
			StringAssert.Contains(iss,"if not IsSignedByMicrosoft(Path) then begin");
			//Windows 11 is the baseline: a warning below build 22000 that names Microsoft's list and lets the user go on; MinVersion is the floor.
			StringAssert.Contains(iss,"Windows11Build = 22000;");
			StringAssert.Contains(iss,"DotNetWindowsUrl = 'https://learn.microsoft.com/en-us/dotnet/core/install/windows';");
			StringAssert.Contains(iss,"if Version.Build < Windows11Build then");
			StringAssert.Contains(iss,"SuppressibleMsgBox(OldWindows, mbConfirmation, MB_YESNO, IDYES)");
			Assert.AreEqual("10.0.18362",SetupDirective(iss,"MinVersion"));
			foreach(Match match in Regex.Matches(iss,@"https?://[^\s'""]+")) {
				string url=match.Value.TrimEnd(')',':','.',',',';');
				bool isMicrosoft=url.StartsWith("https://aka.ms/") || url.StartsWith("https://go.microsoft.com/") || url.StartsWith("https://learn.microsoft.com/");
				bool isSource=url.StartsWith("https://github.com/Marysville-Dental-Center/OpenPMS-Dental");
				bool isInnoSetupHelp=url=="https://jrsoftware.org/ishelp/index.php?topic=64bitinstallation";
				Assert.IsTrue(isMicrosoft || isSource || isInnoSetupHelp,"Unexpected address: "+url);
			}
			//The setup downloads only the Visual C++ redistributable: one DownloadPrereq call, with its Microsoft address.
			Assert.AreEqual(1,Regex.Matches(iss,@"DownloadPrereq\(Vc").Count);
		}

		///<summary>The 64-bit install (decision 19, S3b-7): x64compatible in 64-bit mode, Program Files, and the 64-bit TWAINDSM.dll into the 64-bit
		///system folder as a shared file the uninstall leaves, installed when missing or older (no ignoreversion); never into the program folder.
		///</summary>
		[TestMethod]
		public void Iss_InstallsIn64BitMode_AndTheTwainDsmIntoTheSystemFolder() {
			string iss=ReadIss();
			Assert.AreEqual("x64compatible",SetupDirective(iss,"ArchitecturesAllowed"));
			Assert.AreEqual("x64compatible",SetupDirective(iss,"ArchitecturesInstallIn64BitMode"));
			Assert.AreEqual("yes",SetupDirective(iss,"UsePreviousAppDir"));
			Assert.IsFalse(iss.Contains("{commonpf32}"),"Never Program Files (x86).");
			//Never {syswow64}: the 32-bit TWAIN data source manager goes into the 32-bit system folder as {sys} with the 32bit flag (S3b-8,
			//Iss_InstallsThe32BitTwainDsmIntoTheSysWow64Folder), as Inno Setup's sharedfile documentation asks.
			Assert.IsFalse(iss.Contains("{syswow64}"),"Never {syswow64}.");
			string dsmLine=iss.Split('\n').Single(x => x.StartsWith("Source: \"{#TwainDsmFile}\""));
			StringAssert.Contains(dsmLine,"DestDir: \"{sys}\"");
			StringAssert.Contains(dsmLine,"sharedfile");
			StringAssert.Contains(dsmLine,"uninsneveruninstall");
			Assert.IsFalse(dsmLine.Contains("ignoreversion"),"Only when missing or older.");
			StringAssert.Contains(iss,"#define TwainDsmFile \"bin\\setup-files\\TWAINDSM.dll\"");
			//Build-Release passes the checked file and refuses one in the program folder.
			string script=TestPaths.ReadSource("tools","Build-Release.ps1");
			StringAssert.Contains(script,"\"/DTwainDsmFile=$($twainDsm.Path)\"");
			StringAssert.Contains(script,"$TwainDsmSha256='25869f88a2bc76ded62a13aaea9f2084f0165bfdbbb7f46b9c3a43298938d467'");
			StringAssert.Contains(script,"$TwainDsmSigner='O=TWAIN Working Group'");
			StringAssert.Contains(script,"@('TWAINDSM.dll',");
		}

		///<summary>The 32-bit TWAIN data source manager (decision 23, S3b-8; the owner's "32-bit TWAIN manager: yes"): the 32-bit TWAINDSM.dll of the
		///same approved download into the 32-bit system folder, exactly as the 64-bit one (a shared file the uninstall leaves, installed when
		///missing or older), counted in the 32-bit view of SharedDLLs: {sys} with the 32bit flag, never {syswow64} (Inno Setup's sharedfile
		///documentation: the path must be the one every 32-bit installer counts, or a second count is made); Build-Release passes the checked file
		///and pins its own SHA-256 and entry beside the 64-bit one's.</summary>
		[TestMethod]
		public void Iss_InstallsThe32BitTwainDsmIntoTheSysWow64Folder() {
			string iss=ReadIss();
			string dsmLine=iss.Split('\n').Single(x => x.StartsWith("Source: \"{#TwainDsm32File}\""));
			StringAssert.Contains(dsmLine,"DestDir: \"{sys}\"");
			string flags=Regex.Match(dsmLine,@"Flags: ([^;\r\n]+)").Groups[1].Value.Trim();
			CollectionAssert.AreEquivalent(new[] { "sharedfile","uninsneveruninstall","restartreplace","32bit" },flags.Split(' '));
			Assert.IsFalse(dsmLine.Contains("ignoreversion"),"Only when missing or older.");
			Assert.IsFalse(dsmLine.Contains("Check:"),"On every install, as the 64-bit one.");
			StringAssert.Contains(iss,"#define TwainDsm32File \"bin\\setup-files\\x86\\TWAINDSM.dll\"");
			string script=TestPaths.ReadSource("tools","Build-Release.ps1");
			StringAssert.Contains(script,"\"/DTwainDsm32File=$($twainDsm32.Path)\"");
			Assert.AreEqual("dsm_020501/windows/32/TWAINDSM.dll",ScriptValue(script,"TwainDsm32Entry"));
			Assert.AreEqual("cee9a6ff8026dde09eda9f892c57cc928d42e4a4d22bb740fe38ee6e91797a28",ScriptValue(script,"TwainDsm32Sha256"));
			StringAssert.Contains(script,"$SetupFiles=@('TWAINDSM.dll','x86\\TWAINDSM.dll')");
			//The setup's packed list must hold both, so a setup without the 32-bit one fails the release.
			StringAssert.Contains(script,"[void]$expected.Add([System.IO.Path]::GetFullPath($twainDsm32.Path))");
		}

		///<summary>Build-Release's setup-file checks on the approved download itself (-WriteSetupFiles): both DSMs are written, the 64-bit one as
		///TWAINDSM.dll (PE machine 0x8664) and the 32-bit one as x86\TWAINDSM.dll (0x14C), each byte for byte the approved file. A download whose
		///32-bit file is another one is refused (exit 1, the 32-bit check names it) while the 64-bit one still passes. Inconclusive without the
		///approved download (od-tools\downloads\s3b7-twaindsm-20261116 in the common git folder).</summary>
		[TestMethod]
		public void BuildRelease_ChecksBothTwainDsms_AndRefusesAnother32BitFile() {
			string pathZip=FindApprovedTwainDsmZip();
			if(pathZip==null) {
				Assert.Inconclusive("No approved TWAIN DSM download (od-tools\\downloads\\s3b7-twaindsm-20261116\\dsm_020501.zip) in the common git folder.");
			}
			string folder=Path.Combine(Path.GetTempPath(),"ReleaseFlowTests_setupfiles_"+Guid.NewGuid().ToString("N"));
			try {
				string folderOut=Path.Combine(folder,"ok");
				ScriptResult scriptResult=RunBuildRelease("-WriteSetupFiles",Quote(folderOut));
				Assert.AreEqual(0,scriptResult.ExitCode,scriptResult.Output);
				string path64=Path.Combine(folderOut,"TWAINDSM.dll");
				string path32=Path.Combine(folderOut,"x86","TWAINDSM.dll");
				CollectionAssert.Contains(scriptResult.Lines,"Wrote "+path64,scriptResult.Output);
				CollectionAssert.Contains(scriptResult.Lines,"Wrote "+path32,scriptResult.Output);
				Assert.AreEqual(0x8664,ReadPeMachine(path64));
				Assert.AreEqual(0x14C,ReadPeMachine(path32));
				using(FileStream fileStream=File.OpenRead(path32)) {
					Assert.AreEqual("cee9a6ff8026dde09eda9f892c57cc928d42e4a4d22bb740fe38ee6e91797a28",Sha256Hex(fileStream));
				}
				using(FileStream fileStream=File.OpenRead(path64)) {
					Assert.AreEqual("25869f88a2bc76ded62a13aaea9f2084f0165bfdbbb7f46b9c3a43298938d467",Sha256Hex(fileStream));
				}
				//A download whose windows/32 entry is another file (synthetic bytes), the 64-bit entry the approved one.
				string pathOther=Path.Combine(folder,"dsm_other.zip");
				using(ZipArchive zipArchiveApproved=ZipFile.OpenRead(pathZip))
				using(ZipArchive zipArchiveOther=ZipFile.Open(pathOther,ZipArchiveMode.Create)) {
					ZipArchiveEntry zipArchiveEntry64=zipArchiveApproved.GetEntry("dsm_020501/windows/64/TWAINDSM.dll");
					using(Stream streamFrom=zipArchiveEntry64.Open())
					using(Stream streamTo=zipArchiveOther.CreateEntry("dsm_020501/windows/64/TWAINDSM.dll").Open()) {
						streamFrom.CopyTo(streamTo);
					}
					using(StreamWriter streamWriter=new StreamWriter(zipArchiveOther.CreateEntry("dsm_020501/windows/32/TWAINDSM.dll").Open())) {
						streamWriter.Write("synthetic, not the 32-bit TWAIN DSM");
					}
				}
				ScriptResult scriptResultOther=RunBuildRelease("-WriteSetupFiles",Quote(Path.Combine(folder,"other")),"-TwainDsmZip",Quote(pathOther));
				Assert.AreEqual(1,scriptResultOther.ExitCode,scriptResultOther.Output);
				StringAssert.Contains(scriptResultOther.Output,"the 32-bit TWAINDSM.dll is not the approved download");
				StringAssert.Contains(scriptResultOther.Output,"The 32-bit TWAIN DSM check");
				Assert.IsFalse(scriptResultOther.Output.Contains("The 64-bit TWAIN DSM check"),scriptResultOther.Output);
			}
			finally {
				DeleteFolder(folder);
			}
		}

		///<summary>The approved dsm_020501.zip in the common git folder's od-tools\downloads\s3b7-twaindsm-20261116, or null.</summary>
		private static string FindApprovedTwainDsmZip() {
			string folderGit=Path.Combine(TestPaths.RepoRoot,".git");
			string folderCommon=null;
			if(Directory.Exists(folderGit)) {
				folderCommon=folderGit;
			}
			else if(File.Exists(folderGit)) {
				//A worktree: ".git" names its folder under the common folder's worktrees\.
				string line=File.ReadAllText(folderGit).Trim();
				if(line.StartsWith("gitdir:",StringComparison.Ordinal)) {
					string folderWorktree=Path.GetFullPath(Path.Combine(TestPaths.RepoRoot,line.Substring("gitdir:".Length).Trim()));
					string pathCommondir=Path.Combine(folderWorktree,"commondir");
					folderCommon=File.Exists(pathCommondir)
						? Path.GetFullPath(Path.Combine(folderWorktree,File.ReadAllText(pathCommondir).Trim()))
						: Path.GetDirectoryName(Path.GetDirectoryName(folderWorktree));
				}
			}
			if(folderCommon==null) {
				return null;
			}
			string pathZip=Path.Combine(folderCommon,"od-tools","downloads","s3b7-twaindsm-20261116","dsm_020501.zip");
			return File.Exists(pathZip) ? pathZip : null;
		}

		private static int ReadPeMachine(string path) {
			byte[] arrayBytes=File.ReadAllBytes(path);
			int offset=BitConverter.ToInt32(arrayBytes,0x3C);
			return BitConverter.ToUInt16(arrayBytes,offset+4);
		}

		[TestMethod]
		public void Iss_RegistersTheProgramsEventLogSource_OnEveryInstall_AndTheUninstallDeletesIt() {
			string iss=ReadIss();
			StringAssert.Contains(iss,"#define EventLogSource \""+ProgramNames.EventLogSource+"\"");
			string registryLine=iss.Split('\n').Single(x => x.StartsWith("Root: HKLM;"));
			StringAssert.Contains(registryLine,@"SYSTEM\CurrentControlSet\Services\EventLog\Application\{#EventLogSource}");
			//Every install is an administrative one (S3a-11), so the entry has no Check.
			Assert.IsFalse(registryLine.Contains("Check:"),registryLine);
			Assert.AreEqual("uninsdeletekey",Regex.Match(registryLine,@"Flags: ([^;]+)").Groups[1].Value.Trim());
		}
		#endregion installer/OpenPmsDental.iss

		#region The source archive
		private static string Sha256Hex(Stream stream) {
			using(SHA256 sha=SHA256.Create()) {
				return string.Concat(sha.ComputeHash(stream).Select(x => x.ToString("x2")));
			}
		}

		///<summary>TWAINDSM.dll, which the setup installs, is under the LGPL 2.1 (section 4: its source goes with the object code). The source
		///archive, a release asset with its SHA-256 in SHA256SUMS.txt, carries the approved twain-dsm-2.5.1.zip byte for byte, and its
		///SOURCE-COMMITS.txt names it with that SHA-256 and the upstream address.</summary>
		[TestMethod]
		public void SourceArchive_CarriesTheSourceOfTheSetupsTwainDsm() {
			string folder=Path.Combine(Path.GetTempPath(),"ReleaseFlowTests_source_"+Guid.NewGuid().ToString("N"));
			try {
				ScriptResult scriptResult=RunBuildRelease("-WriteSourceArchive",Quote(folder));
				Assert.AreEqual(0,scriptResult.ExitCode,scriptResult.Output);
				string pathArchive=Directory.GetFiles(folder,"OpenPmsDental-*-source.zip").Single();
				CollectionAssert.Contains(scriptResult.Lines,"Wrote "+pathArchive,scriptResult.Output);
				string top=Path.GetFileNameWithoutExtension(pathArchive);
				using(ZipArchive zipArchive=ZipFile.OpenRead(pathArchive)) {
					ZipArchiveEntry zipArchiveEntry=zipArchive.GetEntry(top+"/"+TWAIN_DSM_SOURCE_ENTRY);
					Assert.IsNotNull(zipArchiveEntry,"The source archive does not carry the source of TWAINDSM.dll.");
					Assert.AreEqual(15006960L,zipArchiveEntry.Length);
					using(Stream stream=zipArchiveEntry.Open()) {
						Assert.AreEqual(TWAIN_DSM_SOURCE_SHA256,Sha256Hex(stream),"The approved twain-dsm-2.5.1.zip, byte for byte.");
					}
					//It is the release's source: its own top folder holds the DSM's licence.
					using(ZipArchive zipArchiveSource=new ZipArchive(zipArchiveEntry.Open(),ZipArchiveMode.Read)) {
						Assert.IsTrue(zipArchiveSource.Entries.Any(x => x.FullName.EndsWith("TWAIN_DSM/license.txt")),"No TWAIN_DSM/license.txt in the TWAIN DSM source.");
					}
					ZipArchiveEntry zipArchiveEntryCommits=zipArchive.GetEntry(top+"/SOURCE-COMMITS.txt");
					Assert.IsNotNull(zipArchiveEntryCommits);
					string commits;
					using(StreamReader streamReader=new StreamReader(zipArchiveEntryCommits.Open(),Encoding.UTF8)) {
						commits=streamReader.ReadToEnd();
					}
					StringAssert.Contains(commits,TWAIN_DSM_SOURCE_ENTRY+": SHA-256 "+TWAIN_DSM_SOURCE_SHA256);
					StringAssert.Contains(commits,"https://github.com/twain/twain-dsm/releases/download/v2.5.1/twain-dsm-2.5.1.zip");
					StringAssert.Contains(commits,"GNU LGPL 2.1, LICENSE-LGPL-2.1.txt");
				}
			}
			finally {
				if(Directory.Exists(folder)) {
					Directory.Delete(folder,true);
				}
			}
		}

		///<summary>No source archive without that source: a missing download stops the script with exit 4, another file under its name with exit 1,
		///both before anything is written (and so, in a release, before the build).</summary>
		[TestMethod]
		public void SourceArchive_RefusesAMissingOrOtherTwainDsmSource() {
			string folder=Path.Combine(Path.GetTempPath(),"ReleaseFlowTests_source_"+Guid.NewGuid().ToString("N"));
			try {
				Directory.CreateDirectory(folder);
				string folderOut=Path.Combine(folder,"out");
				string pathOther=Path.Combine(folder,"twain-dsm-2.5.1.zip");
				File.WriteAllText(pathOther,"synthetic, not the TWAIN DSM source");
				ScriptResult scriptResultOther=RunBuildRelease("-WriteSourceArchive",Quote(folderOut),"-TwainDsmSourceZip",Quote(pathOther));
				Assert.AreEqual(1,scriptResultOther.ExitCode,scriptResultOther.Output);
				StringAssert.Contains(scriptResultOther.Output,"is not the approved source of TWAINDSM.dll");
				ScriptResult scriptResultMissing=RunBuildRelease("-WriteSourceArchive",Quote(folderOut),"-TwainDsmSourceZip",
					Quote(Path.Combine(folder,"missing.zip")));
				Assert.AreEqual(4,scriptResultMissing.ExitCode,scriptResultMissing.Output);
				StringAssert.Contains(scriptResultMissing.Output,"Missing: the source of TWAINDSM.dll");
				Assert.IsFalse(Directory.Exists(folderOut),"Nothing is written.");
			}
			finally {
				if(Directory.Exists(folder)) {
					Directory.Delete(folder,true);
				}
			}
		}

		///<summary>The script pins the source's SHA-256 beside the DSM's, checks it before the build, and the documents say where the release
		///carries the source.</summary>
		[TestMethod]
		public void BuildRelease_PinsTheTwainDsmSource_AndTheDocumentsSayWhereItIs() {
			string script=TestPaths.ReadSource("tools","Build-Release.ps1");
			Assert.AreEqual(TWAIN_DSM_SOURCE_SHA256,ScriptValue(script,"TwainDsmSourceSha256"));
			Assert.AreEqual(TWAIN_DSM_SOURCE_ENTRY,ScriptValue(script,"TwainDsmSourceEntry"));
			Assert.AreEqual("s3b7-twaindsm-20261116",ScriptValue(script,"TwainDsmDownloadFolder"));
			Match matchAssert=Regex.Match(script,@"(?ms)^function Assert-SourceArchivePossible \{.*?^\}");
			Assert.IsTrue(matchAssert.Success);
			StringAssert.Contains(matchAssert.Value,"Find-TwainDsmSource");
			Match matchWrite=Regex.Match(script,@"(?ms)^function Write-SourceArchive\(.*?^\}");
			Assert.IsTrue(matchWrite.Success);
			StringAssert.Contains(matchWrite.Value,"Find-TwainDsmSource");
			StringAssert.Contains(matchWrite.Value,"$TwainDsmSourceEntry");
			string notices=TestPaths.ReadSource("THIRD-PARTY-NOTICES.txt").Replace("\r\n","\n");
			Match matchTwain=Regex.Match(notices,@"(?ms)^--- Component: TWAIN\n.*?(?=^--- )");
			Assert.IsTrue(matchTwain.Success);
			string twain=Regex.Replace(matchTwain.Value,@"\s+"," ");
			StringAssert.Contains(twain,"OpenPmsDental-<version>-source.zip");
			StringAssert.Contains(twain,TWAIN_DSM_SOURCE_ENTRY);
			StringAssert.Contains(twain,TWAIN_DSM_SOURCE_SHA256);
			string releaseProcess=TestPaths.ReadSource("docs","develop","release-process.md");
			StringAssert.Contains(releaseProcess,"the source of the setup's `TWAINDSM.dll` as `"+TWAIN_DSM_SOURCE_ENTRY+"`");
			StringAssert.Contains(releaseProcess,"LGPL 2.1 section 4 for the setup's `TWAINDSM.dll`, whose source it carries");
		}
		#endregion The source archive

		#region tools/Test-Installer.ps1
		private static string ReadTestInstaller() {
			return TestPaths.ReadSource("tools","Test-Installer.ps1");
		}

		///<summary>The value of a top-level assignment $Name='value' or $Name="value" in the script.</summary>
		private static string ScriptValue(string script,string name) {
			Match match=Regex.Match(script,@"(?m)^\$"+Regex.Escape(name)+@"=(['""])(.*?)\1\r?$");
			Assert.IsTrue(match.Success,"No assignment $"+name);
			return match.Groups[2].Value;
		}

		///<summary>The owner runs the script elevated (open point O-3 of docs/specs/net10-plan.md); an agent or a test never does. Unelevated, it
		///refuses before its first change. This test runs it with a setup and a log folder that do not exist: unelevated it ends with exit code 2,
		///and if the test runner itself is elevated the script stops at the missing setup with exit code 3. Either way it makes nothing.</summary>
		[TestMethod]
		public void TestInstaller_RefusesToStartUnelevated_AndChangesNothing() {
			string script=ReadTestInstaller();
			int indexCheck=script.IndexOf("[System.Security.Principal.WindowsBuiltInRole]::Administrator",StringComparison.Ordinal);
			Assert.IsTrue(indexCheck>0,"The elevation check.");
			int indexParam=script.IndexOf("\nparam(",StringComparison.Ordinal);
			string beforeCheck=script.Substring(indexParam,indexCheck-indexParam);
			foreach(string change in new[] { "New-Item","Remove-Item","Start-Process","reg.exe","Set-Item","WriteAllText","Invoke-" }) {
				Assert.IsFalse(beforeCheck.Contains(change),"Nothing before the elevation check changes anything: "+change);
			}
			string checkBlock=script.Substring(indexCheck,script.IndexOf("\n}",indexCheck,StringComparison.Ordinal)-indexCheck);
			StringAssert.Contains(checkBlock,"exit 2");
			StringAssert.Contains(checkBlock,"Run as administrator");
			//The first change of the script comes after the checks that stop it unchanged.
			int indexFirstChange=script.IndexOf("New-Item -ItemType Directory -Force -Path $OutDir",StringComparison.Ordinal);
			Assert.IsTrue(indexFirstChange>0);
			foreach(string stop in new[] { "Stop-Unchanged \"No setup found","Stop-Unchanged \"No staging folder","Stop-Unchanged \"This PC has an uninstall entry" }) {
				int indexStop=script.IndexOf(stop,StringComparison.Ordinal);
				Assert.IsTrue(indexStop>0 && indexStop<indexFirstChange,"Before the log folder is made: "+stop);
			}
			string folder=TestPaths.NewTempFolder("testinstaller");
			try {
				string outDir=Path.Combine(folder,"out");
				ScriptResult scriptResult=RunScript(Path.Combine(TestPaths.RepoRoot,"tools","Test-Installer.ps1"),"-Setup",Quote(Path.Combine(folder,"none","OpenPmsDental-0.0-Setup.exe")),
					"-Staging",Quote(Path.Combine(folder,"none","staging")),"-OutDir",Quote(outDir));
				bool isElevated=new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent())
					.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
				if(isElevated) {
					Assert.AreEqual(3,scriptResult.ExitCode,scriptResult.Output);
					StringAssert.Contains(scriptResult.Output,"No setup found");
				}
				else {
					Assert.AreEqual(2,scriptResult.ExitCode,scriptResult.Output);
					StringAssert.Contains(scriptResult.Output,"needs an elevated PowerShell");
					CollectionAssert.Contains(scriptResult.Lines,"Exit code 2");
				}
				StringAssert.Contains(scriptResult.Output,"Nothing was changed.");
				Assert.IsFalse(Directory.Exists(outDir),"No log folder.");
				Assert.AreEqual(0,Directory.GetFileSystemEntries(folder).Length,"Nothing written.");
			}
			finally {
				DeleteFolder(folder);
			}
		}

		///<summary>The header says exactly how the owner runs it and what it changes on the PC, and the port note gives the same line.</summary>
		[TestMethod]
		public void TestInstaller_HeaderGivesTheOwnersCommand_AndWhatItChanges() {
			string script=ReadTestInstaller();
			string header=script.Substring(0,script.IndexOf("#>",StringComparison.Ordinal));
			const string COMMAND=@"powershell -NoProfile -ExecutionPolicy Bypass -File ""<repo>\tools\Test-Installer.ps1"" -Setup ""<dryrun>\OpenPmsDental-<version>-Setup.exe"" -Staging ""<dryrun>\staging""";
			StringAssert.Contains(header.Replace("\r\n","\n"),"\n\n  "+COMMAND+"\n\n","The one line, on a line of its own.");
			StringAssert.Contains(header,"\"Run as administrator\"");
			StringAssert.Contains(header,"The owner runs it, never an agent");
			StringAssert.Contains(header,"WHAT IT CHANGES ON THIS PC");
			StringAssert.Contains(header,@"%TEMP%\OpenPmsDental-installtest-");
			StringAssert.Contains(header,@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{D5640A25-F302-4CAD-96DC-84AAA8B22F98}_is1");
			StringAssert.Contains(header,@"C:\Windows\System32\TWAINDSM.dll");
			StringAssert.Contains(header,@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\SharedDLLs");
			//The 32-bit TWAIN data source manager (S3b-8), treated exactly as the 64-bit one.
			StringAssert.Contains(header,@"C:\Windows\SysWOW64\TWAINDSM.dll");
			StringAssert.Contains(header,@"HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\SharedDLLs");
			StringAssert.Contains(TestPaths.ReadSource("docs","port-notes","S3b-7.md"),COMMAND);
			StringAssert.Contains(header,@"HKLM\SYSTEM\CurrentControlSet\Services\EventLog\Application\"+ProgramNames.EventLogSource);
			StringAssert.Contains(TestPaths.ReadSource("docs","port-notes","S3a-11.md"),COMMAND);
			//ASCII only (Windows PowerShell 5.1 reads a file without a byte order mark in the ANSI code page).
			byte[] arrayBytes=File.ReadAllBytes(Path.Combine(TestPaths.RepoRoot,"tools","Test-Installer.ps1"));
			Assert.IsFalse(arrayBytes.Any(x => x>=0x80),"ASCII only.");
		}

		///<summary>An all-users install: no /CURRENTUSER, the uninstall entry under HKLM in the 32-bit view (the setup is 32-bit; S3b-7 moves it),
		///new %TEMP% folders with /DIR=, and the Event Log source key that the .iss registers and its uninstall deletes.</summary>
		[TestMethod]
		public void TestInstaller_InstallsForAllUsers_AndChecksTheAllUsersEntry_AndTheEventLogSource() {
			string script=ReadTestInstaller();
			string iss=ReadIss();
			Assert.IsFalse(script.IndexOf("/CURRENTUSER",StringComparison.OrdinalIgnoreCase)>=0,"No per-user install.");
			string appId=SetupDirective(iss,"AppId").Substring(1);
			Assert.AreEqual(appId+"_is1",ScriptValue(script,"AppId"));
			//The 64-bit setup's entry is in the 64-bit view (S3b-7); the 32-bit view's is an installation of v0.0, compared at the end.
			Assert.AreEqual(@"HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$AppId",ScriptValue(script,"UninstallKey"));
			Assert.AreEqual(@"HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\$AppId",ScriptValue(script,"UninstallKey32"));
			StringAssert.Contains(script,"Check (Test-Path $UninstallKey) 'all-users uninstall entry written (64-bit view)'");
			StringAssert.Contains(script,"'the 32-bit view''s uninstall entry (an installation of v0.0) is as before the test'");
			StringAssert.Contains(script,"Check (-not (Test-Path $UninstallKeyUser)) 'no per-user uninstall entry'");
			StringAssert.Contains(script,"\"/DIR=`\"$dir`\"\"");
			StringAssert.Contains(script,"Join-Path $env:TEMP (\"OpenPmsDental-installtest-");
			//The desktop task is off explicitly: setup restores the tasks of an installation already on the PC (same AppId, UsePreviousTasks),
			//so the task's 'unchecked' flag alone would not keep a test install off that installation's common desktop shortcut.
			string argumentsLine=script.Split('\n').Single(x => x.TrimStart().StartsWith("$arguments=@("));
			StringAssert.Contains(argumentsLine,"'/NOICONS'");
			StringAssert.Contains(argumentsLine,"'/MERGETASKS=\"!desktopicon\"'");
			Assert.IsTrue(Regex.IsMatch(iss,@"(?m)^Name: ""desktopicon"";"),"The task the script turns off is the .iss's desktop task.");
			StringAssert.Contains(iss.Split('\n').Single(x => x.StartsWith("Name: \"{autodesktop}")),"Tasks: desktopicon");
			//The Event Log source key: the .iss's subkey and value, and the uninstall deletes the key (uninsdeletekey).
			string registryLine=iss.Split('\n').Single(x => x.StartsWith("Root: HKLM;"));
			string subKey=Regex.Match(registryLine,@"Subkey: ""([^""]+)""").Groups[1].Value.Replace("{#EventLogSource}",ProgramNames.EventLogSource);
			Assert.AreEqual(@"HKLM:\"+subKey,ScriptValue(script,"EventLogKey"));
			Assert.AreEqual(Regex.Match(registryLine,@"ValueData: ""([^""]+)""").Groups[1].Value,ScriptValue(script,"EventMessageFile"));
			StringAssert.Contains(registryLine,"ValueType: expandsz");
			StringAssert.Contains(script,"($kind -eq 'ExpandString')");
			StringAssert.Contains(script,"Check (-not (Test-Path $EventLogKey)) \"the Event Log source key is gone $what (uninsdeletekey)\"");
			foreach(string scenario in new[] { "after A","after B","after D" }) {
				StringAssert.Contains(script,"Test-Uninstalled '"+scenario+"'");
			}
			//It never starts the program: no Start-Process but the setup, the uninstaller and the installed connection settings tool, whose loaded
			//modules show the install folder's own runtime (S3b-7).
			List<string> listStarts=Regex.Matches(script,@"Start-Process -FilePath (\S+)").Cast<Match>().Select(x => x.Groups[1].Value).ToList();
			CollectionAssert.AreEquivalent(new[] { "$Setup","$uninstaller","$tool" },listStarts);
			StringAssert.Contains(script,"$tool=Join-Path $dir $DbSettingsExe");
			List<string> listCalls=Regex.Matches(script,@"(?m)^\s*& (\S+)").Cast<Match>().Select(x => x.Groups[1].Value).ToList();
			CollectionAssert.AreEquivalent(new[] { "reg.exe" },listCalls,"Only reg.exe export, for the saved keys.");
			//An installation that was there before keeps its two keys: the script saves them and writes them back.
			StringAssert.Contains(script,"$savedUninstall=Save-HklmKey $UninstallKey");
			StringAssert.Contains(script,"$savedEventLog=Save-HklmKey $EventLogKey");
			StringAssert.Contains(script,"Restore-HklmKey $pair[1] $pair[0]");
		}

		///<summary>The 32-bit TWAINDSM.dll (S3b-8) is treated exactly as the 64-bit one: its path in the 32-bit system folder, its count in the 32-bit
		///view of SharedDLLs (the .iss's 32bit flag), both saved before the test; checked after the install and after the uninstall (x86, the
		///approved version or later, the TWAIN Working Group's signature); at the end the count written back (or deleted when there was none) and
		///the file deleted only when there was none before. An older one stays replaced (the header says so).</summary>
		[TestMethod]
		public void TestInstaller_TreatsThe32BitTwainDsmAsThe64BitOne() {
			string script=ReadTestInstaller();
			StringAssert.Contains(script,"$TwainDsm32Path=Join-Path ([Environment]::GetFolderPath('SystemX86')) 'TWAINDSM.dll'");
			Assert.AreEqual(@"HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\SharedDLLs",ScriptValue(script,"SharedDllsKey32"));
			StringAssert.Contains(script,"$twainDsm32Before=Test-Path -LiteralPath $TwainDsm32Path");
			StringAssert.Contains(script,"$sharedCount32Before=Get-RegValue $SharedDllsKey32 $TwainDsm32CountName");
			StringAssert.Contains(script,"Test-TwainDsm32 'after the install'");
			StringAssert.Contains(script,"Test-TwainDsm32 'after the uninstall (a shared file the uninstall leaves)'");
			StringAssert.Contains(script,"Check (($machine -eq 0x14C) -and ($version -ge $TwainDsmMinimumVersion) -and ($signature.Status -eq 'Valid') -and ($subject -like '*O=TWAIN Working Group*'))");
			StringAssert.Contains(script,"if($null -ne $sharedCount32Before){ Set-ItemProperty -Path $SharedDllsKey32 -Name $TwainDsm32CountName -Value $sharedCount32Before -Type DWord }");
			StringAssert.Contains(script,"elseif($null -ne (Get-RegValue $SharedDllsKey32 $TwainDsm32CountName)){ Remove-ItemProperty -Path $SharedDllsKey32 -Name $TwainDsm32CountName }");
			StringAssert.Contains(script,"if(-not $twainDsm32Before -and (Test-Path -LiteralPath $TwainDsm32Path)){");
			string header=script.Substring(0,script.IndexOf("#>",StringComparison.Ordinal));
			StringAssert.Contains(header,"An OLDER 32-bit TWAINDSM.dll is replaced with 2.5.1 and stays replaced");
			//The .iss's line is the one the script expects: {sys} with the 32bit flag, the 32-bit system folder and the 32-bit view, counted under
			//the System32 path name 32-bit installers use (SharedDllsName32).
			string dsmLine=ReadIss().Split('\n').Single(x => x.StartsWith("Source: \"{#TwainDsm32File}\""));
			StringAssert.Contains(dsmLine,"DestDir: \"{sys}\"");
			StringAssert.Contains(dsmLine,"32bit");
			StringAssert.Contains(script,"$TwainDsm32CountName=Join-Path ([Environment]::SystemDirectory) 'TWAINDSM.dll'");
		}

		///<summary>Scenarios A to C give a database server on this computer, for which a silent setup runs no connection settings tool (the .iss's
		///IsLoopbackServer, and the program's rule); scenario D gives one over the network without the office CA, and expects the tool's /silent
		///run, its refusal (exit code 5, the office CA is missing) and the setup's exit code 10, with the log lines the .iss and the tool write.</summary>
		[TestMethod]
		public void TestInstaller_ScenarioD_RunsTheToolSilent_AndExpectsExitCode10_TheOthersUseALoopbackServer() {
			string script=ReadTestInstaller();
			string iss=ReadIss();
			List<string> listServers=Regex.Matches(script,@"'/DBSERVER=([^']+)'").Cast<Match>().Select(x => x.Groups[1].Value).ToList();
			CollectionAssert.AreEquivalent(new[] { "localhost:3307","127.0.0.1","second.example.test","localhost" },listServers);
			foreach(string server in new[] { "localhost:3307","127.0.0.1","localhost" }) {
				Assert.IsTrue(DataConnectionBase.DbConnectionSecurity.IsLoopbackServer(server),server);
			}
			//second.example.test is passed to the upgrade of scenario B, which keeps the existing file and passes no page value to the tool.
			StringAssert.Contains(script,"'the upgrade passes none of the connection page''s values to the tool'");
			string network=ScriptValue(script,"NetworkServer");
			Assert.AreEqual("dbserver.example.test:3307",network);
			Assert.IsFalse(DataConnectionBase.DbConnectionSecurity.IsLoopbackServer(network));
			StringAssert.Contains(script,"@(\"/DBSERVER=$NetworkServer\",'/DBNAME=opms_test','/DBUSER=opms_user')");
			Assert.IsFalse(Regex.IsMatch(script,@"/DB(CA|PFX|THUMBPRINT|SECRETSFILE)="),"No office CA, no client certificate, no secrets.");
			StringAssert.Contains(script,"Check ($exit -eq 10)");
			//The log lines the script expects are the ones setup writes.
			StringAssert.Contains(script,"$toolLine=\"Running $DbSettingsExe /server=$NetworkServer /database=opms_test /user=opms_user /silent\"");
			StringAssert.Contains(iss,"Log('Running {#DbSettingsExeName} ' + Params);");
			StringAssert.Contains(iss,"Result := '/server=' + AddQuotes(ConfigServer) + ' /database=' + AddQuotes(ConfigDatabase) + ' /user=' + AddQuotes(ConfigUser);");
			StringAssert.Contains(iss,"Log('{#DbSettingsExeName} exit code ' + IntToStr(ResultCode));");
			StringAssert.Contains(script,"exit code 5\")");
			StringAssert.Contains(iss,"Log('Setup exit code ' + IntToStr(DbSettingsFailedExitCode) + ': the connection settings were not completed.');");
			StringAssert.Contains(script,"'Setup exit code 10: the connection settings were not completed'");
			Assert.AreEqual(ProgramNames.DbSettingsExeFileName,ScriptValue(script,"DbSettingsExe"));
			//The tool's result line (DbSettingsRunner.AppendLog) and its refusal without the office CA, exit code 5 (EXIT_REFUSED).
			string runner=Regex.Replace(TestPaths.ReadSource("OpenPmsDentalDbSettings","DbSettingsRunner.cs"),@"""\s*\+\s*""","");
			StringAssert.Contains(runner,"public const int EXIT_REFUSED=5;");
			StringAssert.Contains(runner,"throw new DbSettingsException(EXIT_REFUSED,\"A database server over the network (\"+dbConnectionConfigNew.ServerName+\") needs the office CA certificate file \"");
			StringAssert.Contains(runner,"+\" exit=\"");
			StringAssert.Contains(script,"exit=5 A database server over the network \\($([regex]::Escape($NetworkServer))\\) needs the office CA certificate file");
			//The tool changed nothing: the folder holds what setup installed, and the config is the one setup wrote.
			Match matchD=Regex.Match(script,@"(?ms)#region D\..*?#endregion");
			Assert.IsTrue(matchD.Success);
			StringAssert.Contains(matchD.Value,"Test-ProgramFolder $dirD");
			StringAssert.Contains(matchD.Value,"Test-SetupConfig $dirD $NetworkServer");
			StringAssert.Contains(matchD.Value,"Invoke-Uninstall $dirD");
		}
		#endregion tools/Test-Installer.ps1

		#region installer/OpenPmsDental.iss, release notes

		[TestMethod]
		public void InfoPage_HasTheAttribution_TheSourceLink_AndNotice() {
			string info=TestPaths.ReadSource("installer","setup-info.txt");
			//The attribution comes from NOTICE, shown on the same page; the template does not repeat it (owner, 2026-11-26).
			Assert.IsFalse(info.Contains(ATTRIBUTION),"setup-info.txt does not repeat NOTICE's attribution.");
			StringAssert.Contains(info,"https://github.com/Marysville-Dental-Center/OpenPMS-Dental/tree/v@VERSION@");
			StringAssert.Contains(info,"@NOTICE@");
			StringAssert.Contains(TestPaths.ReadSource("NOTICE"),ATTRIBUTION,"NOTICE, shown on the same page, starts with it.");
			//"Version 2 or later" is the licence of the program's own source files; the installed program, with PDFsharp and MigraDoc, is GPL 3.
			StringAssert.Contains(info,"The program's own source files are free software: you can redistribute them and/or modify them under the terms of "
				+"the GNU General Public License, version 2 or (at your option) any later version.");
			StringAssert.Contains(info,"so it is distributed under the GNU General Public License, version 3, whose text is on the License Agreement page");
			Assert.IsFalse(info.Contains("The program is free software: you can redistribute it"),"Not true of the installed program.");
		}

		[TestMethod]
		public void LicensePage_SaysHowTheInstalledProgramIsLicensed_ThenShowsTheGpl3TextVerbatim() {
			//Inno Setup's License Agreement page, which the user must accept, comes before the information page. It shows the GPL version 3,
			//under which the program is conveyed as installed, after a header that names the AGPL parts and the program's own GPL v2 or later.
			string template=TestPaths.ReadSource("installer","setup-license.txt");
			StringAssert.Contains(template,"@GPL3@");
			string script=TestPaths.ReadSource("tools","Build-Release.ps1");
			StringAssert.Contains(script,"\"/DLicenseTextFile=$licensePath\"");
			string folder=TestPaths.NewTempFolder("setuptexts");
			try {
				ScriptResult scriptResult=RunBuildRelease("-WriteSetupTexts",Quote(folder));
				Assert.AreEqual(0,scriptResult.ExitCode,scriptResult.Output);
				string licensePath=Path.Combine(folder,"setup-license.txt");
				byte[] arrayBytes=File.ReadAllBytes(licensePath);
				Assert.IsTrue(arrayBytes.Length>3 && arrayBytes[0]==0xEF && arrayBytes[1]==0xBB && arrayBytes[2]==0xBF,"UTF-8 with a byte order mark.");
				string license=File.ReadAllText(licensePath);
				Assert.IsFalse(Regex.IsMatch(license,"(?<!\r)\n"),"CRLF line ends only.");
				StringAssert.StartsWith(license,"Open PMS - Dental "+ReleaseVersion()+": license");
				string header=license.Substring(0,license.IndexOf("GNU GENERAL PUBLIC LICENSE"));
				StringAssert.Contains(header,"As installed, the program is conveyed under the GNU General Public License, version 3, below.");
				StringAssert.Contains(header,"PDFsharp and MigraDoc, which are licensed under the GNU Affero General Public License, version 3 (LICENSE-AGPL-3.0.txt");
				StringAssert.Contains(header,"apply to the combination as a whole");
				StringAssert.Contains(header,"The program's own source files are licensed under the GNU General Public License, version 2 or (at your option) any "
					+"later version (LICENSE");
				Assert.IsFalse(license.Contains("@"+"GPL3@") || license.Contains("@"+"VERSION@"),"Every placeholder is filled.");
				string gpl3=TestPaths.ReadSource("LICENSE-GPL-3.0.txt").Replace("\r\n","\n").Trim('\r','\n');
				StringAssert.Contains(license.Replace("\r\n","\n"),gpl3,"The GPL 3.0 text, whole and unchanged.");
				Assert.IsFalse(license.Contains("Version 2, June 1991"),"Not the GPL version 2.");
				string info=File.ReadAllText(Path.Combine(folder,"setup-info.txt"));
				StringAssert.Contains(info,ATTRIBUTION);
				StringAssert.Contains(info,"https://github.com/Marysville-Dental-Center/OpenPMS-Dental/tree/v"+ReleaseVersion());
				StringAssert.Contains(info.Replace("\r\n","\n"),TestPaths.ReadSource("NOTICE").Replace("\r\n","\n").TrimEnd());
			}
			finally {
				DeleteFolder(folder);
			}
		}

		[TestMethod]
		public void ReleaseNotes_HaveTheAttribution_AndTheSourceOfThisRelease() {
			string version=ReleaseVersion();
			string notes=TestPaths.ReadSource("docs","release-notes","v"+version+".md");
			StringAssert.Contains(notes,ATTRIBUTION);
			StringAssert.Contains(notes,"https://github.com/Marysville-Dental-Center/OpenPMS-Dental/tree/v"+version);
			StringAssert.Contains(notes,"OpenPmsDental-"+version+"-Setup.exe");
		}

		[TestMethod]
		public void ReleaseNotes_GiveTheInstalled16BitDefault_AndTheStepThatKeepsPatientCaptureAt8Bits() {
			//The notes once said patient computers "stay at 8 bits (Allow16Bit=0)", while the program installs with Allow16Bit=1: an office that
			//followed them and ran one Test capture would take 16-bit X-rays of patients before the counsel review and the clinic run.
			string catalog=TestPaths.ReadSource("ImagingCapture","Settings","CaptureSettingsCatalog.cs");
			Match match=Regex.Match(catalog,@"Add\(twain,""Allow16Bit"",CaptureSettingKind\.Switch,""([01])""");
			Assert.IsTrue(match.Success,"The Allow16Bit default in CaptureSettingsCatalog.cs");
			//v0.0's notes, as released (before counsel's review of 2026-11-07): set Allow16Bit=0 before patient capture.
			string notesV00=TestPaths.ReadSource("docs","release-notes","v0.0.md");
			StringAssert.Contains(notesV00,"defaults are `[Twain] Allow16Bit=1`");
			StringAssert.Contains(notesV00,"Before any patient capture, set `[Twain] Allow16Bit=0` in Capture settings");
			StringAssert.Contains(notesV00,"\"Before 16-bit X-rays for patients\"");
			StringAssert.Contains(TestPaths.ReadSource("docs","admin","intraoral-capture-setup.md"),"**Before 16-bit X-rays for patients.**");
			//Since 0.1 (owner decision of 2026-11-07): the installed default is named, and patient use at any bit depth waits for the phantom
			//comparison (docs/admin/intraoral-capture-setup.md, section 14).
			string notes=TestPaths.ReadSource("docs","release-notes","v"+ReleaseVersion()+".md");
			Assert.IsFalse(notes.Contains("stay at 8 bits"),"Nothing in the program keeps them at 8 bits by itself.");
			if(ReleaseVersion()!="0.0") {
				StringAssert.Contains(notes,"(`Allow16Bit="+match.Groups[1].Value+"`)");
				StringAssert.Contains(notes,"**No patient X-rays before the phantom comparison:**");
			}
		}
		#endregion installer/OpenPmsDental.iss, release notes
	}
}
