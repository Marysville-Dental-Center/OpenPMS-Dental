// TC-9b of the WPF tooth chart track (docs/port-notes/TC-9b.md, docs/specs/wpf-tooth-chart-plan.md): SharpDX 4.0.1 and the Tao Framework, the
// libraries of the DirectX and OpenGL tooth charts that TC-9a retired, leave the release with the DirectX End-User Runtime requirement. The
// staged list holds none of the five files; they are retired files of v0.0 (ForkReleases.ListRetiredFiles), which tools\Build-Release.ps1
// refuses and the setup deletes on an upgrade ([InstallDelete]); the DirectX never-ship patterns stay; the setup installs no DirectX runtime;
// THIRD-PARTY-NOTICES.txt lists neither component, while Helix Toolkit's entry keeps the SharpDX MIT notice for HelixToolkit.Maths.
// No setup is run and no database is used.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ReleaseFlowTests {
	[TestClass]
	public class SharpDXTaoRetiredTests {
		///<summary>The five files that left the release, as v0.0 installed them in the program folder.</summary>
		private static readonly string[] _arrayFiles={ "SharpDX.dll","SharpDX.Direct3D9.dll","SharpDX.Mathematics.dll","Tao.OpenGl.dll",
			"Tao.Platform.Windows.dll" };

		///<summary>Build-Release.ps1 -ListFiles: the allow-list as "kind|path" lines (kind "own", "third" or "doc").</summary>
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

		///<summary>The [InstallDelete] section of the setup script, without its comment lines, and the rest of the script.</summary>
		private static void SplitIss(out List<string> listInstallDelete,out string issOther) {
			string iss=TestPaths.ReadSource("installer","OpenPmsDental.iss");
			Match match=Regex.Match(iss,@"(?ms)^\[InstallDelete\]\r?\n(.*?)(?=^\[)");
			Assert.IsTrue(match.Success,"No [InstallDelete] section before the next section.");
			listInstallDelete=match.Groups[1].Value.Split('\n').Select(x => x.Trim()).Where(x => x!="" && !x.StartsWith(";")).ToList();
			issOther=iss.Remove(match.Index,match.Length);
		}

		[TestMethod]
		public void AllowList_ShipsNoSharpDXOrTaoFile() {
			List<string> listLines=ListReleaseFiles();
			Assert.IsTrue(listLines.Contains("third|HelixToolkit.Maths.dll"),"The list is the release's:\r\n"+string.Join("\r\n",listLines));
			List<string> listFound=listLines.Where(x => Regex.IsMatch(x.Split('|').Last(),@"^(SharpDX|Tao)\.",RegexOptions.IgnoreCase)).ToList();
			Assert.AreEqual(0,listFound.Count,"Still staged: "+string.Join(", ",listFound));
		}

		[TestMethod]
		public void TheFiveFiles_AreRetired_DeniedAndDeletedOnAnUpgrade() {
			foreach(string fileName in _arrayFiles) {
				CollectionAssert.Contains(ForkReleases.ListRetiredFiles,fileName,"Never published again by the release channel.");
				Assert.IsFalse(ForkReleases.ListBuildIdFiles.Contains(fileName,StringComparer.OrdinalIgnoreCase),fileName);
			}
			//Build-Release.ps1: off the allow-list (ThirdPartyFiles), on the deny-list with a reason.
			string script=TestPaths.ReadSource("tools","Build-Release.ps1");
			int indexDenyList=script.IndexOf("$DenyList=@(");
			Assert.IsTrue(indexDenyList>0,"Build-Release.ps1 has a deny-list.");
			string scriptAllowed=script.Substring(0,indexDenyList);
			string scriptDenied=script.Substring(indexDenyList);
			foreach(string fileName in _arrayFiles) {
				Assert.IsFalse(scriptAllowed.Contains("'"+fileName+"'"),"Build-Release.ps1 still ships "+fileName);
				Assert.IsTrue(Regex.IsMatch(scriptDenied,@"@\('"+Regex.Escape(fileName)+@"','[^']+'\)"),"Build-Release.ps1 refuses "+fileName+" with a reason.");
			}
			List<string> listInstallDelete;
			string issOther;
			SplitIss(out listInstallDelete,out issOther);
			foreach(string fileName in _arrayFiles) {
				CollectionAssert.Contains(listInstallDelete,@"Type: files; Name: ""{app}\"+fileName+@"""","An upgrade removes "+fileName);
			}
		}

		///<summary>The DirectX files that only Microsoft's installer may redistribute stay on the deny-list (the WPF chart needs none of them).</summary>
		[TestMethod]
		public void DirectXPatterns_StayOnTheDenyList() {
			string script=TestPaths.ReadSource("tools","Build-Release.ps1");
			string scriptDenied=script.Substring(script.IndexOf("$DenyList=@("));
			StringAssert.Contains(scriptDenied,"@('Microsoft.DirectX*.dll',\"DirectX, redistributable only through Microsoft's installer\"),");
			StringAssert.Contains(scriptDenied,"@('d3dx*.dll',\"DirectX, redistributable only through Microsoft's installer\"),");
		}

		///<summary>The setup has never installed the DirectX End-User Runtime, and still installs nothing of DirectX: outside [InstallDelete] the
		///script names no DirectX, D3DX, SharpDX or Tao file or installer.</summary>
		[TestMethod]
		public void Setup_InstallsNoDirectXRuntime() {
			List<string> listInstallDelete;
			string issOther;
			SplitIss(out listInstallDelete,out issOther);
			StringAssert.Contains(issOther,@"Source: ""{#StagingDir}\*""","The setup script is the program's.");
			Assert.IsFalse(Regex.IsMatch(issOther,@"DirectX|d3dx|d3d9|dxsetup|dxwebsetup|SharpDX|\bTao\b",RegexOptions.IgnoreCase),
				"The setup names DirectX, D3DX, SharpDX or Tao outside [InstallDelete].");
		}

		[TestMethod]
		public void Notices_ListNeitherComponent_HelixKeepsTheSharpDXNotice() {
			string notices=TestPaths.ReadSource("THIRD-PARTY-NOTICES.txt");
			List<string> listComponents=Regex.Matches(notices,@"(?m)^--- Component: (.+?)\r?$").Cast<Match>().Select(x => x.Groups[1].Value.Trim()).ToList();
			CollectionAssert.Contains(listComponents,"Helix Toolkit");
			CollectionAssert.DoesNotContain(listComponents,"SharpDX");
			CollectionAssert.DoesNotContain(listComponents,"Tao Framework");
			foreach(string fileName in _arrayFiles) {
				Assert.IsFalse(notices.IndexOf(fileName,StringComparison.OrdinalIgnoreCase)>=0,"The notices still name "+fileName);
			}
			Assert.IsFalse(Regex.IsMatch(notices,@"\bTao\b"),"The notices still name Tao.");
			//HelixToolkit.Maths holds code derived from SharpDX: its MIT notice stays inside the Helix entry, the only place that names SharpDX.
			Match matchHelix=Regex.Match(notices,@"(?ms)^--- Component: Helix Toolkit\r?$(.*?)(?=^--- )");
			Assert.IsTrue(matchHelix.Success,"Helix Toolkit's entry.");
			StringAssert.Contains(matchHelix.Groups[1].Value,"HelixToolkit.Maths.dll holds code derived from the SharpDX and SlimMath projects");
			StringAssert.Contains(matchHelix.Groups[1].Value,"Copyright (c) 2010-2014 SharpDX - Alexandre Mutel");
			Assert.AreEqual(Regex.Matches(matchHelix.Groups[1].Value,"SharpDX").Count,Regex.Matches(notices,"SharpDX").Count,
				"SharpDX is named in Helix Toolkit's entry only.");
		}

		[TestMethod]
		public void ReleaseNotes_SayTheDirectXRuntimeIsNoLongerNeeded() {
			string notes=TestPaths.ReadSource("docs","release-notes","next.md");
			Match match=Regex.Match(notes,@"(?ms)^## The tooth chart: SharpDX, Tao and the DirectX runtime leave the program folder\r?$(.*?)(?=^## )");
			Assert.IsTrue(match.Success,"The release notes' section.");
			StringAssert.Contains(match.Groups[1].Value,"DirectX End-User Runtime");
			foreach(string fileName in _arrayFiles) {
				StringAssert.Contains(match.Groups[1].Value,"`"+fileName+"`");
			}
		}
	}
}
