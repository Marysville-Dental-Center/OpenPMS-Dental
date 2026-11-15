// How a pull starts UpdateFileCopier (S3a-10 (iii) and (iv) of docs/specs/net10-plan.md): every file the copier needs goes into a folder of
// its own, and the copier, whose manifest asks for administrator rights, starts through ShellExecute so UAC asks for consent. Nothing here
// starts the copier: the launch check in docs/port-notes/S3a-10.md does, by hand.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using ProjectFileTests;

namespace ReleaseFlowTests {
	[TestClass]
	public class FileCopierStartTests {
		[TestMethod]
		public void StartInfo_UsesShellExecute_ForTheElevationPrompt() {
			ProcessStartInfo processStartInfo=ForkReleases.GetFileCopierStartInfo(@"C:\Temp\openpms-dental\ForkReleaseCopier\UpdateFileCopier.exe",
				"\"C:\\Temp\\ForkRelease\" 1234 \"C:\\Program Files (x86)\\Open PMS - Dental\" True True");
			Assert.IsTrue(processStartInfo.UseShellExecute,"CreateProcess refuses a requireAdministrator program (ERROR_ELEVATION_REQUIRED); only "
				+"ShellExecute asks for consent.");
			Assert.AreEqual(@"C:\Temp\openpms-dental\ForkReleaseCopier\UpdateFileCopier.exe",processStartInfo.FileName);
			StringAssert.StartsWith(processStartInfo.Arguments,"\"C:\\Temp\\ForkRelease\" 1234 ");
			Assert.IsFalse(processStartInfo.RedirectStandardOutput || processStartInfo.RedirectStandardError || processStartInfo.RedirectStandardInput,
				"ShellExecute cannot redirect.");
		}

		[TestMethod]
		public void UpdateFileCopier_AsksForAdministratorRights() {
			string manifest=TestPaths.ReadSource("UpdateFileCopier","app.manifest");
			string manifestUncommented=Regex.Replace(manifest,"<!--.*?-->","",RegexOptions.Singleline);
			StringAssert.Contains(manifestUncommented,"<requestedExecutionLevel level=\"requireAdministrator\" uiAccess=\"false\" />");
			StringAssert.Contains(TestPaths.ReadSource("UpdateFileCopier","UpdateFileCopier.csproj"),"<ApplicationManifest>app.manifest</ApplicationManifest>");
		}

		///<summary>OpenFileCopier copies the copier's files with CopyFileCopier and starts it with GetFileCopierStartInfo, UseShellExecute=true, and
		///no longer copies the exe alone.</summary>
		[TestMethod]
		public void OpenFileCopier_CopiesEveryCopierFile_AndStartsItThroughShellExecute() {
			string textPrefL=StripComments(TestPaths.ReadSource("OpenPmsDental","Logic","PrefL.cs"));
			string body=GetMethodBody(textPrefL,"public static bool OpenFileCopier(");
			StringAssert.Contains(body,"ForkReleases.CopyFileCopier(");
			StringAssert.Contains(body,"ForkReleases.FILE_COPIER_FOLDER_NAME");
			StringAssert.Contains(body,"process.StartInfo=ForkReleases.GetFileCopierStartInfo(startFileName,arguments);");
			StringAssert.Contains(body,"process.StartInfo.UseShellExecute=true;");
			Assert.IsFalse(body.Contains("File.Copy("),"The copier's files are copied by CopyFileCopier, every one of them.");
			Assert.IsFalse(Regex.IsMatch(body,@"UseShellExecute\s*=\s*false"));
			int indexStart=body.IndexOf("process.Start()");
			Assert.IsTrue(indexStart>body.IndexOf("process.StartInfo.UseShellExecute=true;"),"UseShellExecute is set before the start.");
			//Every Process this file creates states UseShellExecute (S3a-2's rule; PrefL is S3a-10's).
			int countNewProcess=Regex.Matches(textPrefL,@"new Process\(\)").Count;
			int countShellExecute=Regex.Matches(textPrefL,@"\.UseShellExecute\s*=\s*(true|false)").Count;
			Assert.IsTrue(countShellExecute>=countNewProcess,"new Process(): "+countNewProcess+", UseShellExecute set: "+countShellExecute);
			Assert.IsFalse(Regex.IsMatch(textPrefL,@"Process\.Start\("),"PrefL starts no process by Process.Start(...).");
		}

		[TestMethod]
		public void PullRelease_ChecksTheRuntimeBeforeDownloading() {
			string textPrefL=StripComments(TestPaths.ReadSource("OpenPmsDental","Logic","PrefL.cs"));
			StringAssert.Contains(GetMethodBody(textPrefL,"private static void PullRelease("),"ForkReleases.DownloadAndVerify(folderUpdate,releaseIdentityRunning)");
			StringAssert.Contains(GetMethodBody(textPrefL,"public static bool CheckProgramVersion("),
				"ForkReleases.Decide(releaseManifestPublished,errorPublished,releaseIdentityRunning,");
			StringAssert.Contains(GetMethodBody(textPrefL,"public static bool CopyFromHereToUpdateFiles("),"ForkReleases.GetRefusal(");
			StringAssert.Contains(GetMethodBody(textPrefL,"public static bool CheckForkVersionGuard("),"ForkReleases.GetRefusal(");
			Assert.IsFalse(Regex.IsMatch(textPrefL,@"GetPublishedManifest\(out string _\)"),"No caller drops the reason a manifest is not readable.");
		}

		[TestMethod]
		public void CopyFileCopier_CopiesOnlyTheCopiersFiles_IntoAnEmptiedFolder() {
			string folderSource=TestPaths.NewTempFolder("copier_source");
			string folderDest=Path.Combine(TestPaths.NewTempFolder("copier_temp"),ForkReleases.FILE_COPIER_FOLDER_NAME);
			try {
				foreach(string fileName in new[] { "UpdateFileCopier.exe","UpdateFileCopier.exe.config","UpdateFileCopier.dll","UpdateFileCopier.runtimeconfig.json",
					"UpdateFileCopier.deps.json","UpdateFileCopier.pdb","OpenPmsDental.exe","CodeBase.dll" })
				{
					File.WriteAllText(Path.Combine(folderSource,fileName),"synthetic "+fileName);
				}
				Directory.CreateDirectory(folderDest);
				File.WriteAllText(Path.Combine(folderDest,"UpdateFileCopier.dll"),"an earlier copier's file");
				File.WriteAllText(Path.Combine(folderDest,"Stale.dll"),"left from before");
				string pathExe=ForkReleases.CopyFileCopier(folderSource,folderDest);
				Assert.AreEqual(Path.Combine(folderDest,"UpdateFileCopier.exe"),pathExe);
				CollectionAssert.AreEquivalent(new[] { "UpdateFileCopier.exe","UpdateFileCopier.exe.config","UpdateFileCopier.dll",
					"UpdateFileCopier.runtimeconfig.json","UpdateFileCopier.deps.json" },
					Directory.GetFiles(folderDest).Select(x => Path.GetFileName(x)).ToArray(),"Only the copier's files; symbols and program files stay.");
				Assert.AreEqual("synthetic UpdateFileCopier.dll",File.ReadAllText(Path.Combine(folderDest,"UpdateFileCopier.dll")),"Never two copiers' files.");
				//A release today ships the exe alone, and the copier runs without its .exe.config.
				File.Delete(Path.Combine(folderSource,"UpdateFileCopier.exe.config"));
				File.Delete(Path.Combine(folderSource,"UpdateFileCopier.dll"));
				File.Delete(Path.Combine(folderSource,"UpdateFileCopier.runtimeconfig.json"));
				File.Delete(Path.Combine(folderSource,"UpdateFileCopier.deps.json"));
				ForkReleases.CopyFileCopier(folderSource,folderDest);
				CollectionAssert.AreEqual(new[] { "UpdateFileCopier.exe" },Directory.GetFiles(folderDest).Select(x => Path.GetFileName(x)).ToArray());
				File.Delete(Path.Combine(folderSource,"UpdateFileCopier.exe"));
				Assert.ThrowsExactly<FileNotFoundException>(() => ForkReleases.CopyFileCopier(folderSource,folderDest));
			}
			finally {
				TryDelete(folderSource);
				TryDelete(Path.GetDirectoryName(folderDest));
			}
		}

		///<summary>Every file UpdateFileCopier's build writes, apart from symbols and documentation, is a file CopyFileCopier copies, so a file the
		///copier needs (3b's .dll, .runtimeconfig.json and .deps.json) is never left behind.</summary>
		[TestMethod]
		public void EveryFileOfTheCopiersBuild_IsCopied() {
			string configuration=Path.GetFileName(Path.GetDirectoryName(typeof(FileCopierStartTests).Assembly.Location));
			string folder=Path.Combine(TestPaths.RepoRoot,"UpdateFileCopier","bin",configuration);
			BuildOutput.RequireFile(Path.Combine(folder,"UpdateFileCopier.exe"),Path.Combine(folder,"UpdateFileCopier.exe")+" is not built. Build "
				+"OpenDental.Local.slnf ("+configuration+"|x86) first.");
			List<string> listBuilt=Directory.GetFiles(folder,"*",SearchOption.AllDirectories)
				.Select(x => x.Substring(folder.Length+1))
				.Where(x => !x.EndsWith(".pdb",StringComparison.OrdinalIgnoreCase) && !x.EndsWith(".xml",StringComparison.OrdinalIgnoreCase))
				.ToList();
			List<string> listCopied=ForkReleases.GetFileCopierFiles(folder);
			List<string> listLeft=listBuilt.Where(x => !listCopied.Contains(x,StringComparer.OrdinalIgnoreCase)).ToList();
			Assert.AreEqual(0,listLeft.Count,"UpdateFileCopier's build writes files a pull would not copy: "+string.Join(", ",listLeft));
			Assert.AreEqual("UpdateFileCopier.exe",listCopied[0]);
			string folderDest=Path.Combine(TestPaths.NewTempFolder("copier_real"),ForkReleases.FILE_COPIER_FOLDER_NAME);
			try {
				ForkReleases.CopyFileCopier(folder,folderDest);
				foreach(string fileName in listCopied) {
					CollectionAssert.AreEqual(File.ReadAllBytes(Path.Combine(folder,fileName)),File.ReadAllBytes(Path.Combine(folderDest,fileName)),fileName);
				}
			}
			finally {
				TryDelete(Path.GetDirectoryName(folderDest));
			}
		}

		private static void TryDelete(string folder) {
			try {
				if(Directory.Exists(folder)) {
					Directory.Delete(folder,true);
				}
			}
			catch(IOException) {
			}
			catch(UnauthorizedAccessException) {
			}
		}

		private static string GetMethodBody(string text,string signature) {
			int indexStart=text.IndexOf(signature);
			Assert.IsTrue(indexStart>=0,signature);
			int indexBrace=text.IndexOf('{',indexStart);
			int depth=0;
			for(int i=indexBrace;i<text.Length;i++) {
				if(text[i]=='{') {
					depth++;
				}
				else if(text[i]=='}') {
					depth--;
					if(depth==0) {
						return text.Substring(indexBrace,i-indexBrace+1);
					}
				}
			}
			return text.Substring(indexBrace);
		}

		///<summary>Removes // and /* */ comments (not inside string literals).</summary>
		private static string StripComments(string text) {
			return Regex.Replace(text,@"""(?:[^""\\]|\\.)*""|//[^\n]*|/\*.*?\*/",m => m.Value.StartsWith("/") ? "" : m.Value,RegexOptions.Singleline);
		}
	}
}
