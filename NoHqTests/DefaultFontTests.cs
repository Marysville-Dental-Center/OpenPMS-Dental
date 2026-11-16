// The default font of the Windows Forms programs on .NET 10 (the plan's section 7, "Default font"; S3b-4 put the calls in, S3b-5 adds the
// tests): .NET's default font is Segoe UI 9 where the .NET Framework's was Microsoft Sans Serif 8.25, which about 72 plain forms rely on, so
// every exe's Main calls Application.SetDefaultFont(Microsoft Sans Serif 8.25) first, then SetHighDpiMode(PerMonitorV2), before anything can
// create a window (SetDefaultFont throws once a window exists). The program's Main may first run EHR-3's /ehr-render branch, which ends the
// process before any window exists. Two tests: the source of each Main, and in a fresh process (TestProbeHost, where no window exists yet) the
// font a plain Form gets after those two calls: Microsoft Sans Serif at 8.25 pt times the Windows text scale factor (decision 9), so the test
// also holds on a PC with "Make text bigger". It shows no window (the Form's handle is never created) and needs no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using TestProbeHost;

namespace NoHqTests {
	[TestClass]
	public class DefaultFontTests {
		///<summary>Every Windows Forms exe's Main, by file: the shipped programs, the helper and the tools (decision 17 keeps them all).</summary>
		private static readonly string[] _arrayMains=new[] {
			@"OpenPmsDental\Main Modules\ProgramEntry.cs",@"OpenPmsDentalDbSettings\Program.cs",@"ServiceManager\Program.cs",
			@"OpenPmsDentalHL7\Program.cs",@"UpdateFileCopier\Program.cs",@"OpenPmsDentalHelper32\Program.cs",@"CentralManager\Program.cs",
			@"DatabaseIntegrityCheck\Program.cs",@"DocumentationBuilder\Program.cs",@"MySQLManager\Program.cs",@"xCrudGenerator\Program.cs",
			@"RdlDesign\RdlDesigner.cs",
		};
		private const string LINE_FONT=@"(System\.Windows\.Forms\.)?Application\.SetDefaultFont\(new System\.Drawing\.Font\(""Microsoft Sans Serif"",8\.25f\)\);";
		private const string LINE_DPI=@"(System\.Windows\.Forms\.)?Application\.SetHighDpiMode\((System\.Windows\.Forms\.)?HighDpiMode\.PerMonitorV2\);";
		private const string LINE_EHR_RENDER=@"if\(args\.Length==2 && args\[0\]==""/ehr-render""\) \{ Environment\.Exit\(EhrRenderChild\.Run\(args\[1\]\)\); \}";

		///<summary>The source rule: in each Main, the first statement is SetDefaultFont (after comments, and in ProgramEntry.Main after the
		///branch for /ehr-render), and the next is SetHighDpiMode. Every file of the repository's Windows Forms programs that calls Application.Run
		///is one of _arrayMains, so a new exe cannot leave it out.</summary>
		[TestMethod]
		public void EveryExesMain_SetsTheDefaultFontFirst_ThenTheDpiMode_BeforeAnyWindow() {
			string root=RemovedAssembliesTests.GetRepositoryRoot();
			List<string> listProblems=new List<string>();
			foreach(string relative in _arrayMains) {
				string text=File.ReadAllText(Path.Combine(root,relative)).Replace("\r\n","\n");
				Match matchMain=Regex.Match(text,@"static (void|int) Main\([^)]*\)\s*\{");
				if(!matchMain.Success) {
					listProblems.Add(relative+": no Main.");
					continue;
				}
				string body=text.Substring(matchMain.Index+matchMain.Length);
				string prefix=relative.EndsWith("ProgramEntry.cs") ? @"(\s*//[^\n]*\n)*\s*"+LINE_EHR_RENDER : "";
				if(!Regex.IsMatch(body,"^"+prefix+@"(\s*//[^\n]*\n)*\s*"+LINE_FONT+@"\s*"+LINE_DPI)) {
					listProblems.Add(relative+": Main does not start with SetDefaultFont and SetHighDpiMode.");
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("\r\n",listProblems));
			//Every exe of the repository that runs a Windows Forms message loop is in the list.
			List<string> listRunners=new List<string>();
			foreach(string path in Directory.GetFiles(root,"*.cs",SearchOption.AllDirectories)) {
				string relative=path.Substring(root.Length+1);
				if(Regex.IsMatch(relative,@"^(tests|UnitTests|UnitTestsCore|NoHqTests|third-party|spike|\.claude)\\",RegexOptions.IgnoreCase)
					|| relative.Contains(@"\bin\") || relative.Contains(@"\obj\"))
				{
					continue;
				}
				string text=File.ReadAllText(path);
				if(Regex.IsMatch(text,@"static (void|int) Main\(") && Regex.IsMatch(text,@"(?<![\w.])Application\.Run\(")) {//not webApplication.Run (the Middle Tier server)
					listRunners.Add(relative);
				}
			}
			CollectionAssert.IsSubsetOf(listRunners,_arrayMains,"A Main that runs a window: "+string.Join(", ",listRunners));
		}

		///<summary>The font a plain Form gets after the two calls, in a process with no window yet (TestProbeHost, run once): Microsoft Sans Serif
		///at 8.25 pt times the text scale factor of HKCU\Software\Microsoft\Accessibility (100 when absent), as .NET applies it.</summary>
		[TestMethod]
		public void APlainForm_AfterTheStartUpCalls_HasMicrosoftSansSerif825_TimesTheTextScaleFactor() {
			string folder=Path.GetDirectoryName(typeof(DefaultFontTests).Assembly.Location);
			ProbeRunResult probeRunResult=ProbeProcess.RunOnce(folder,null,typeof(DefaultFontProbe),new string[0],TimeSpan.FromMinutes(2));
			Assert.AreEqual(0,probeRunResult.ExitCode,probeRunResult.Output);
			string line=probeRunResult.Output.Split('\n').Select(x => x.Trim()).Single(x => x.StartsWith("FONT|"));
			string[] arrayParts=line.Split('|');
			Assert.AreEqual("Microsoft Sans Serif",arrayParts[1]);
			float sizeExpected=8.25f*DefaultFontProbe.GetTextScaleFactor()/100f;
			float size=float.Parse(arrayParts[2],CultureInfo.InvariantCulture);
			Assert.AreEqual(sizeExpected,size,0.01f,"8.25 pt times the text scale factor "+DefaultFontProbe.GetTextScaleFactor()+".");
			Assert.AreEqual("False",arrayParts[3],"No window was created.");
		}
	}

	///<summary>The child process of the font test: the two start-up calls of every exe's Main, then a plain Form's font. Its handle is never
	///created, so nothing is shown.</summary>
	public static class DefaultFontProbe {
		public static int Run(string[] args) {
			Application.SetDefaultFont(new System.Drawing.Font("Microsoft Sans Serif",8.25f));
			Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
			using(Form form=new Form()) {
				Console.WriteLine("FONT|"+form.Font.Name+"|"+form.Font.SizeInPoints.ToString("R",CultureInfo.InvariantCulture)+"|"+form.IsHandleCreated);
			}
			return 0;
		}

		///<summary>Windows' "Make text bigger" (TextScaleFactor, percent), 100 when the value is absent.</summary>
		public static int GetTextScaleFactor() {
			using(RegistryKey registryKey=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Accessibility")) {
				object value=registryKey?.GetValue("TextScaleFactor");
				return value is int ? (int)value : 100;
			}
		}
	}
}
