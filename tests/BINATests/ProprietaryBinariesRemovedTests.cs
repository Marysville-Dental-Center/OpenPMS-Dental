// Tests that Open Dental's closed binaries removed by package BIN-A stay removed (docs/port-notes/BIN-A.md): the Suni capture driver
// (xImageDeviceManager.dll and its SuniUSB.dll), the VB helper VBbridges.dll and the Sparks3D tooth chart (Sparks3D.dll, Sparks3DXEngine.dll).
// Nothing in the repository may hold, reference, copy, publish or use them. Reads the source and the built assemblies only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using ProjectFileTests;
using SparksToothChart;

namespace BINATests {
	[TestClass]
	public class ProprietaryBinariesRemovedTests {
		///<summary>The assembly names of the removed binaries.</summary>
		private static readonly string[] _arrayRemovedAssemblies=new[] { "xImageDeviceManager","SuniUSB","VBbridges","Sparks3D","Sparks3DXEngine" };

		[TestMethod]
		public void RequiredDlls_HoldsNoneOfTheRemovedBinaries() {
			string folder=Path.Combine(RepoFiles.Root(),"Required dlls");
			Assert.IsTrue(Directory.Exists(folder));
			List<string> listLeft=new[] { "xImageDeviceManager.dll","SuniUSB.dll","VBbridges.dll" }.Where(x => File.Exists(Path.Combine(folder,x))).ToList();
			Assert.AreEqual(0,listLeft.Count,"Still in Required dlls: "+string.Join(", ",listLeft));
			Assert.IsFalse(Directory.Exists(Path.Combine(folder,"Sparks3D")),@"Required dlls\Sparks3D must stay deleted.");
			List<string> listAnywhere=RepoFiles.Files("*.dll").Where(x => _arrayRemovedAssemblies.Contains(Path.GetFileNameWithoutExtension(x),StringComparer.OrdinalIgnoreCase))
				.ToList();
			Assert.AreEqual(0,listAnywhere.Count,"Still in the repository: "+string.Join(", ",listAnywhere));
		}

		[TestMethod]
		public void Projects_ReferenceNoneOfTheRemovedBinaries() {
			//A Reference, a HintPath (an element, or the attribute an SDK-style project file may use), or a file item named after them
			//(Sparks3DInterface.cs, the Sparks3D licence text).
			Regex regex=new Regex(@"<Reference\s+Include=""(xImageDeviceManager|SuniUSB|VBbridges|Sparks3D|Sparks3DXEngine)\b"
				+@"|<HintPath>[^<]*(xImageDeviceManager|SuniUSB|VBbridges|Sparks3D)[^<]*</HintPath>|\bHintPath\s*=\s*""[^""]*(xImageDeviceManager|SuniUSB|VBbridges|Sparks3D)"
				+@"|<(Compile|None|Content|EmbeddedResource)\s+(Include|Update)=""[^""]*(Sparks3D|SuniUSB|VBbridges|xImageDeviceManager)",RegexOptions.IgnoreCase);
			List<string> listOffenders=new List<string>();
			foreach(string relative in RepoFiles.Files("*.csproj","*.vcxproj","*.props","*.targets","*.sln","*.slnf")) {
				Match match=regex.Match(RepoFiles.Read(relative));
				if(match.Success) {
					listOffenders.Add(relative+" ("+match.Value+")");
				}
			}
			Assert.AreEqual(0,listOffenders.Count,"Still referenced in: "+string.Join(", ",listOffenders));
			//Nor compiled, embedded or copied from a file left in a project folder: an SDK-style project takes what is on disk without naming it.
			List<string> listItems=new List<string>();
			//The projects ProjectFile models: not the Middle Tier server of the Web SDK (S3b-6), whose text the check above reads (S3b-5).
			foreach(string path in ProjectFile.GetModelledSolutionFilterProjects(Path.Combine(RepoFiles.Root(),"OpenDental.Local.slnf"))) {
				ProjectFile projectFile=ProjectFile.Load(path);
				listItems.AddRange(_arrayRemovedAssemblies.SelectMany(x => projectFile.GetItemsMentioning(x,"Compile","None","Content","EmbeddedResource","Reference"))
					.Select(x => projectFile.Name+": "+x));
			}
			Assert.AreEqual(0,listItems.Count,"Still in a project: "+string.Join(", ",listItems));
		}

		[TestMethod]
		public void Source_UsesNoTypeOfTheRemovedBinaries() {
			//Type use: a using directive, a qualified name (namespace dot capital letter, so "Sparks3D.dll" in a comment is not a hit), the Suni
			//capture control, the Sparks3D adapter and its switch, and the Sparks3D licence resource.
			Regex regex=new Regex(@"^\s*using\s+(xImageDeviceManager|VBbridges|Sparks3D)\s*;"
				+@"|\b(xImageDeviceManager|VBbridges|Sparks3D)\.[A-Z]"
				+@"|\bSuniDeviceControl\b|\bSparks3DInterface\b|\bIsSparks3DPresent\b|Resources\.Sparks3D\b",RegexOptions.Multiline);
			List<string> listOffenders=new List<string>();
			foreach(string relative in RepoFiles.Files("*.cs","*.vb","*.xaml")) {
				Match match=regex.Match(RepoFiles.Read(relative));
				if(match.Success) {
					listOffenders.Add(relative+" ("+match.Value.Trim()+")");
				}
			}
			Assert.AreEqual(0,listOffenders.Count,"Still uses a removed binary: "+string.Join(", ",listOffenders));
		}

		[TestMethod]
		public void BuiltLibraries_ReferenceNoneOfTheRemovedBinaries() {
			foreach(Assembly assembly in new[] { typeof(Patient).Assembly,typeof(CodeBase.ODFileUtils).Assembly,typeof(ToothChartWrapper).Assembly }) {
				List<string> listReferences=assembly.GetReferencedAssemblies().Select(x => x.Name).ToList();
				foreach(string removed in _arrayRemovedAssemblies) {
					Assert.IsFalse(listReferences.Contains(removed,StringComparer.OrdinalIgnoreCase),assembly.GetName().Name+" still references "+removed);
				}
			}
		}

		[TestMethod]
		public void BuiltProgram_ReferencesNoneOfTheRemovedBinaries() {
			//The program's managed assembly (ProgramAssembly: OpenPmsDental.dll on .NET, where the .exe is the apphost Mono.Cecil cannot read).
			foreach(string pathRelative in new[] { ProgramAssembly.FindRelative(RepoFiles.Root(),@"OpenPmsDental\bin\Debug","OpenPmsDental"),
				@"WpfControlsOD\bin\Debug\WpfControlsOD.dll" })
			{
				string path=Path.Combine(RepoFiles.Root(),pathRelative);
				if(!File.Exists(path)) {
					BuildOutput.NotBuilt(pathRelative+" is not built. Build OpenDental.Local.slnf (Debug|x86) first.");
				}
				List<string> listReferences=RepoFiles.ReferencedAssemblyNames(path);
				foreach(string removed in _arrayRemovedAssemblies) {
					Assert.IsFalse(listReferences.Contains(removed,StringComparer.OrdinalIgnoreCase),pathRelative+" still references "+removed);
				}
			}
		}

		[TestMethod]
		public void ReleaseChannel_PublishesNoSparks3DFolder() {
			Assert.IsFalse(ForkReleases.ListSpecialDirs.Contains("Sparks3D",StringComparer.OrdinalIgnoreCase),
				"The fork release must not publish a Sparks3D folder.");
			string source=RepoFiles.Read(@"UpdateFileCopier\FormMain.cs");
			Match match=Regex.Match(source,@"_listSpecialDirs=new List<string>\(\)\s*\{(?<items>[^}]*)\}");
			Assert.IsTrue(match.Success,"UpdateFileCopier's special folder list not found.");
			StringAssert.DoesNotMatch(match.Groups["items"].Value,new Regex("Sparks3D",RegexOptions.IgnoreCase),
				"UpdateFileCopier must not copy a Sparks3D folder.");
		}

		[TestMethod]
		public void SuniCapture_IsGoneFromScanningSetup_AndTheOldImagesModuleIsRemoved() {
			//The old Images module, which had the Suni capture button, was removed in stage 0 of the .NET 10 move (S0-6).
			Assert.IsFalse(File.Exists(Path.Combine(RepoFiles.Root(),@"OpenPmsDental\Main Modules\ControlImagesOld.cs")));
			Assert.IsFalse(File.Exists(Path.Combine(RepoFiles.Root(),@"OpenPmsDental\Main Modules\ControlImagesOld.designer.cs")));
			string imagingSetup=RepoFiles.Read(@"OpenPmsDental\Forms\FormImagingSetup.cs")+RepoFiles.Read(@"OpenPmsDental\Forms\FormImagingSetup.designer.cs");
			StringAssert.DoesNotMatch(imagingSetup,new Regex(@"groupBoxSuni|upDownExposure|checkBinned|ProgDesc==""Suni"""),
				"The Suni sensor settings were only read by the removed Suni driver.");
		}

		[TestMethod]
		public void LicenceLists_DoNotNameSparks3D() {
			foreach(string relative in new[] { @"OpenPmsDental\Forms\FormLicense.cs",@"OpenPmsDental\Forms\FormRegistrationKey.cs",
				@"OpenPmsDental\Properties\Resources.resx" })
			{
				StringAssert.DoesNotMatch(RepoFiles.Read(relative),new Regex("Sparks3D"),relative);
			}
			Assert.IsFalse(File.Exists(Path.Combine(RepoFiles.Root(),@"OpenPmsDental\Resources\Sparks3D.txt")));
		}

		[TestMethod]
		public void DirectX11Settings_ChooseTheWpfChart_ReadOnlyByTheRelay_TC8b() {
			//Until TC-8b this test forbade any read of Open Dental's DirectX 11 settings, which only the removed Sparks3D chart read. Since TC-8b (the
			//plan's parity row ENG-2; owner question BIN-A 5: keep the rows) they choose the WPF 3D chart in the closed chart's place, exactly here:
			//the relay reads and saves the preference (IsChart3DOnAllComputers, SetChart3DOnAllComputers) and reads and saves this computer's column
			//(GetChart3DThisComputer, SetGraphicsChoice), and Setup > Computers' "Use Simple Graphics" writes "Do not use" (ENG-6). File > Graphics and
			//the chart's callers go through the relay. Nothing else in the program, the chart or the business layer names them; Open Dental's own CRUD,
			//table type, preference list and conversion steps are not code that chooses a chart.
			Regex regex=new Regex(@"PrefName\.DirectX11ToothChartUseIfAvail|\.GraphicsUseDirectX11\b|\bGraphicsUseDirectX11\s*=");
			Dictionary<string,int> dictExpected=new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase) {
				//IsChart3DOnAllComputers (PrefC.GetBoolSilent), SetChart3DOnAllComputers (Prefs.UpdateBool), GetChart3DThisComputer (the read),
				//SetGraphicsChoice (the comparison and the write).
				{ @"WpfControlsOD\UI\ToothChart\ToothChartRelay.cs",5 },
				//SetToSimpleGraphics' UPDATE (ENG-6).
				{ @"OpenPmsDentalBusiness\Data Interface\ComputerPrefs.cs",1 },
			};
			string[] arrayOpenDentalOwn=new[] { @"OpenPmsDentalBusiness\Crud\ComputerPrefCrud.cs",@"OpenPmsDentalBusiness\TableTypes\ComputerPref.cs",
				@"OpenPmsDentalBusiness\TableTypes\Pref.cs" };
			Dictionary<string,int> dictFound=new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);
			foreach(string folder in new[] { "OpenPmsDental","OpenPmsDentalBusiness","WpfControlsOD","SparksToothChart","CodeBase" }) {
				foreach(string file in Directory.EnumerateFiles(Path.Combine(RepoFiles.Root(),folder),"*.cs",SearchOption.AllDirectories)) {
					string relative=file.Substring(RepoFiles.Root().Length).TrimStart('\\');
					if(relative.Contains(@"\obj\") || relative.Contains(@"\bin\") || arrayOpenDentalOwn.Contains(relative,StringComparer.OrdinalIgnoreCase)
						|| Path.GetFileName(relative).StartsWith("ConvertDatabases",StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					int count=regex.Matches(GetCode(File.ReadAllText(file))).Count;
					if(count>0) {
						dictFound[relative]=count;
					}
				}
			}
			CollectionAssert.AreEquivalent(dictExpected.Keys.ToList(),dictFound.Keys.ToList(),"Files that read Open Dental's DirectX 11 settings: "
				+string.Join("; ",dictFound.Select(x => x.Key+" "+x.Value)));
			foreach(KeyValuePair<string,int> pair in dictExpected) {
				Assert.AreEqual(pair.Value,dictFound[pair.Key],pair.Key);
			}
			string relay=GetCode(RepoFiles.Read(@"WpfControlsOD\UI\ToothChart\ToothChartRelay.cs"));
			StringAssert.Contains(relay,"return PrefC.GetBoolSilent(PrefName.DirectX11ToothChartUseIfAvail,true);");
			StringAssert.Contains(relay,"return Prefs.UpdateBool(PrefName.DirectX11ToothChartUseIfAvail,isChart3DOnAllComputers);");
			StringAssert.Contains(relay,"return computerPref.GraphicsUseDirectX11;");
			StringAssert.Contains(relay,"computerPref.GraphicsUseDirectX11=chart3DThisComputer;");
			StringAssert.Contains(GetCode(RepoFiles.Read(@"OpenPmsDentalBusiness\Data Interface\ComputerPrefs.cs")),
				"string command=\"UPDATE computerpref SET GraphicsSimple=1,GraphicsUseDirectX11=\"+POut.Int((int)YN.No)");
			//File > Graphics has Open Dental's DirectX 11 groups under the 3D tooth chart's name, through the relay, and no Sparks3D.
			string graphics=RepoFiles.Read(@"OpenPmsDental\Forms\FormGraphics.cs")+RepoFiles.Read(@"OpenPmsDental\Forms\FormGraphics.designer.cs");
			StringAssert.DoesNotMatch(graphics,new Regex(@"radioDirectX11|Sparks3D|IsSparks3DPresent"),"The window names no DirectX 11 control and no Sparks3D.");
			StringAssert.Contains(graphics,"ToothChartRelay.IsChart3DOnAllComputers()");
			StringAssert.Contains(graphics,"ToothChartRelay.SetChart3DOnAllComputers(isChart3DOnAllComputers)");
		}

		///<summary>The source without its comment lines (a line whose text starts with //, the XML comments included).</summary>
		private static string GetCode(string source) {
			return string.Join("\n",source.Replace("\r\n","\n").Split('\n').Where(x => !x.TrimStart().StartsWith("//")));
		}
	}
}
