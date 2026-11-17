// No test run sends telemetry (package S4-1 of the .NET 10 plan, docs/specs/net10-plan.md; docs/port-notes/S4-1.md). MSTest.TestAdapter 4.5.3
// brings Microsoft.Testing.Extensions.Telemetry and with it Microsoft.ApplicationInsights, which only a Microsoft Testing Platform program uses.
// The tests run under VSTest instead: every test project of OpenDental.Local.slnf turns the platform off (tests\Directory.Build.props, and
// NoHqTests' own file), Directory.Build.targets takes the telemetry extension's registration and both packages' assemblies out, and
// vstest.console sends telemetry only when VSTEST_TELEMETRY_OPTEDIN is 1. These tests check each of those from the project files (ProjectFile),
// the built output folders (BuildOutput's rule when a project is not built) and this test process. Since S4-2 the runners also set
// TESTINGPLATFORM_TELEMETRY_OPTOUT=1 and DOTNET_CLI_TELEMETRY_OPTOUT=1 for vstest and its test hosts (docs/port-notes/S4-2.md): this test
// host's environment shows it when a runner started it, and TestRunnerScriptTests checks the scripts themselves.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProjectFileTests {
	[TestClass]
	public class TestTelemetryTests {
		///<summary>The registration of Microsoft.Testing.Extensions.Telemetry (its build props) for a platform entry point.</summary>
		private const string TELEMETRY_HOOK="98058041-B5B6-4A75-9834-58E6DF796A22";
		///<summary>MSTest's own registration (MSTest.TestAdapter.props).</summary>
		private const string MSTEST_HOOK="031F8871-2660-4208-8F6B-FC142B40ABFF";
		///<summary>The assembly metadata key Microsoft.Testing.Platform.MSBuild writes into a platform program (IsTestingPlatformApplication).</summary>
		private const string PLATFORM_APPLICATION_KEY="Microsoft.Testing.Platform.Application";
		private static readonly string[] _arrayTelemetryFiles=new[] { "Microsoft.ApplicationInsights.dll","Microsoft.Testing.Extensions.Telemetry.dll" };
		///<summary>The adapter and platform assemblies the build copies next to every test assembly (MSTest.TestAdapter's), which vstest.console
		///loads from there.</summary>
		private static readonly string[] _arrayAdapterFiles=new[] { "MSTest.TestAdapter.dll","MSTestAdapter.PlatformServices.dll",
			"Microsoft.Testing.Platform.dll","Microsoft.Testing.Extensions.MSBuild.dll","Microsoft.Testing.Extensions.TrxReport.Abstractions.dll" };
		private static List<ProjectFile> _listTestProjects;

		///<summary>The test projects the test runner runs: every project of OpenDental.Local.slnf whose file name ends in Tests.csproj (UnitTests,
		///NoHqTests and the tools' tests included).</summary>
		private static List<ProjectFile> TestProjects() {
			if(_listTestProjects==null) {
				_listTestProjects=ProjectFile.GetSolutionFilterProjects(Path.Combine(FixtureFolder.RepoRoot(),"OpenDental.Local.slnf"))
					.Where(x => x.EndsWith("Tests.csproj",StringComparison.OrdinalIgnoreCase))
					.Select(x => ProjectFile.Load(x)).ToList();
			}
			return _listTestProjects;
		}

		[TestMethod]
		public void EveryTestProject_RunsUnderVSTest_WithoutTheTelemetryExtension() {
			List<string> listProblems=new List<string>();
			foreach(ProjectFile projectFile in TestProjects()) {
				foreach(string name in new[] { "EnableMSTestRunner","IsTestingPlatformApplication","GenerateTestingPlatformEntryPoint",
					"TestingPlatformDotnetTestSupport" })
				{
					if(projectFile.GetProperty(name)!="false") {
						listProblems.Add(projectFile.Name+": "+name+"='"+projectFile.GetProperty(name)+"'");
					}
				}
				if(projectFile.GetPackageReference("MSTest.TestAdapter")==null) {
					listProblems.Add(projectFile.Name+" has no MSTest.TestAdapter");
				}
				List<ProjectItem> listHooks=projectFile.GetItems("TestingPlatformBuilderHook");
				//MSTest's own registration (MSTest.TestAdapter.props) is there, so the packages' build files were read; the telemetry one is not.
				if(!listHooks.Any(x => x.Include.Equals(MSTEST_HOOK,StringComparison.OrdinalIgnoreCase))) {
					listProblems.Add(projectFile.Name+" has no MSTest registration: the packages' build files were not read");
				}
				if(listHooks.Any(x => x.Include.Equals(TELEMETRY_HOOK,StringComparison.OrdinalIgnoreCase))) {
					listProblems.Add(projectFile.Name+" registers the telemetry extension");
				}
			}
			Assert.IsTrue(TestProjects().Count>100,"The solution filter lists "+TestProjects().Count+" test projects.");
			Assert.AreEqual(0,listProblems.Count,string.Join("; ",listProblems.Take(20)));
		}

		[TestMethod]
		public void EveryBuiltTestProject_HasNoTelemetryAssembly_AndIsNoPlatformProgram() {
			List<string> listProblems=new List<string>();
			List<string> listNotBuilt=new List<string>();
			foreach(ProjectFile projectFile in TestProjects()) {
				string targetPath=projectFile.GetProperty("TargetPath");
				if(!File.Exists(targetPath)) {
					listNotBuilt.Add(targetPath);
					continue;
				}
				string folder=Path.GetDirectoryName(targetPath);
				foreach(string fileName in _arrayTelemetryFiles) {
					if(File.Exists(Path.Combine(folder,fileName))) {
						listProblems.Add(Path.Combine(folder,fileName));
					}
				}
				//The key is a UTF-8 string of an AssemblyMetadata attribute's blob. Source code that names it (this file) holds it as a UTF-16 string
				//literal, which this search does not find.
				if(Contains(File.ReadAllBytes(targetPath),Encoding.UTF8.GetBytes(PLATFORM_APPLICATION_KEY))) {
					listProblems.Add(targetPath+" is a Microsoft Testing Platform program");
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("; ",listProblems.Take(20)));
			if(listNotBuilt.Count>0) {
				BuildOutput.NotBuilt(listNotBuilt.Count+" test projects are not built, the first "+listNotBuilt[0]+". Build OpenDental.Local.slnf (Debug|x86).");
			}
		}

		[TestMethod]
		public void TheMSTestAdapter_UsesNoTelemetryAssembly() {
			//The adapter and the platform assemblies next to this test assembly, the ones vstest.console runs it with (S4-2): they reference
			//the platform, not the telemetry extension or Application Insights, so leaving those out of the output folders breaks nothing.
			string folder=Path.GetDirectoryName(typeof(TestTelemetryTests).Assembly.Location);
			List<string> listProblems=new List<string>();
			foreach(string fileName in _arrayAdapterFiles) {
				string path=Path.Combine(folder,fileName);
				if(!File.Exists(path)) {
					listProblems.Add(fileName+" is not next to the test assembly");
					continue;
				}
				List<string> listReferences=ReadReferencedAssemblyNames(path);
				if(fileName=="MSTest.TestAdapter.dll" && !listReferences.Contains("Microsoft.Testing.Platform")) {
					listProblems.Add("The adapter does not reference the platform: it is not MSTest 4's");
				}
				foreach(string name in listReferences.Where(x => x.IndexOf("Telemetry",StringComparison.OrdinalIgnoreCase)>=0
					|| x.StartsWith("Microsoft.ApplicationInsights",StringComparison.OrdinalIgnoreCase)))
				{
					listProblems.Add(fileName+" references "+name);
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("; ",listProblems));
		}

		[TestMethod]
		public void EveryBuiltTestProject_HasTheMSTestAdapterNextToIt() {
			//vstest.console finds the adapter next to the test assembly (S4-2: the runners pass no adapter path), so every output folder holds the
			//adapter and its platform assemblies, the same version as the ones next to this test assembly.
			string folderThis=Path.GetDirectoryName(typeof(TestTelemetryTests).Assembly.Location);
			Dictionary<string,string> dictVersions=_arrayAdapterFiles.ToDictionary(x => x,x => FileVersionInfo.GetVersionInfo(Path.Combine(folderThis,x)).FileVersion);
			List<string> listProblems=new List<string>();
			List<string> listNotBuilt=new List<string>();
			foreach(ProjectFile projectFile in TestProjects()) {
				string targetPath=projectFile.GetProperty("TargetPath");
				if(!File.Exists(targetPath)) {
					listNotBuilt.Add(targetPath);
					continue;
				}
				foreach(string fileName in _arrayAdapterFiles) {
					string path=Path.Combine(Path.GetDirectoryName(targetPath),fileName);
					if(!File.Exists(path)) {
						listProblems.Add(path+" is missing");
					}
					else if(FileVersionInfo.GetVersionInfo(path).FileVersion!=dictVersions[fileName]) {
						listProblems.Add(path+" is "+FileVersionInfo.GetVersionInfo(path).FileVersion+", not "+dictVersions[fileName]);
					}
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("; ",listProblems.Take(20)));
			if(listNotBuilt.Count>0) {
				BuildOutput.NotBuilt(listNotBuilt.Count+" test projects are not built, the first "+listNotBuilt[0]+". Build OpenDental.Local.slnf (Debug|x86).");
			}
		}

		[TestMethod]
		public void ThisRun_IsAVSTestRun_WithVSTestTelemetryOff() {
			//vstest.console sends telemetry only when VSTEST_TELEMETRY_OPTEDIN is 1; its test host inherits its environment.
			string optedIn=(Environment.GetEnvironmentVariable("VSTEST_TELEMETRY_OPTEDIN")??"").Trim();
			Assert.IsFalse(optedIn=="1" || optedIn.Equals("true",StringComparison.OrdinalIgnoreCase),"VSTEST_TELEMETRY_OPTEDIN="+optedIn);
			//A VSTest test host (testhost.x86.exe under /Platform:x86, testhost.exe), not a Microsoft Testing Platform program of its own.
			string processName=Process.GetCurrentProcess().ProcessName;
			Assert.IsTrue(processName.StartsWith("testhost",StringComparison.OrdinalIgnoreCase),processName);
		}

		[TestMethod]
		public void ThisRun_ByARunner_HasTheTelemetryOptOuts() {
			//The runners (S4-2) start vstest with both opt-outs: tools\run-tests.ps1 and the lead's copy mark their runs with OPMS_TEST_RUNNER,
			//the lane's -Test runs with OPMS_REQUIRE_BUILD_OUTPUT=1 in the environment of tools\Initialize-BuildEnvironment.ps1. A run from an
			//editor's test window has neither and is not a runner's run.
			bool isRunnerRun=!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OPMS_TEST_RUNNER")) || BuildOutput.IsRequired;
			if(!isRunnerRun) {
				Assert.Inconclusive("Not started by a test runner (no OPMS_TEST_RUNNER, no OPMS_REQUIRE_BUILD_OUTPUT=1).");
			}
			foreach(string name in new[] { "TESTINGPLATFORM_TELEMETRY_OPTOUT","DOTNET_CLI_TELEMETRY_OPTOUT" }) {
				Assert.AreEqual("1",(Environment.GetEnvironmentVariable(name)??"").Trim(),name+" in this test host's environment");
			}
		}

		///<summary>The names of the assemblies the file references, read with Mono.Cecil from a copy of its bytes: nothing is loaded or locked (S3a-8b
		///of the .NET 10 move: .NET has no reflection-only load).</summary>
		private static List<string> ReadReferencedAssemblyNames(string path) {
			using(MemoryStream memoryStream=new MemoryStream(File.ReadAllBytes(path)))
			using(Mono.Cecil.ModuleDefinition moduleDefinition=Mono.Cecil.ModuleDefinition.ReadModule(memoryStream)) {
				return moduleDefinition.AssemblyReferences.Select(x => x.Name).ToList();
			}
		}

		private static bool Contains(byte[] arrayHaystack,byte[] arrayNeedle) {
			for(int i=0;i<=arrayHaystack.Length-arrayNeedle.Length;i++) {
				int j=0;
				while(j<arrayNeedle.Length && arrayHaystack[i+j]==arrayNeedle[j]) {
					j++;
				}
				if(j==arrayNeedle.Length) {
					return true;
				}
			}
			return false;
		}
	}
}
