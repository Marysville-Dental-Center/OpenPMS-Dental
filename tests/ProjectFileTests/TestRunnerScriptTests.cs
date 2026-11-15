// The test runner, tools\run-tests.ps1, under MSTest 4 (package S4-2 of the .NET 10 plan, docs/specs/net10-plan.md; docs/port-notes/S4-2.md):
// it reads the counts from both of vstest's summary formats, compares a run with an earlier results file, and starts vstest without
// telemetry; tools\Initialize-BuildEnvironment.ps1 gives the lane's test runs the same environment. The script's functions are taken from
// its syntax tree and run in a PowerShell process of their own, on sample vstest output in a temporary folder; no test is run, nothing is
// built and nothing outside the temporary folder and that process changes.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProjectFileTests {
	[TestClass]
	public class TestRunnerScriptTests {
		private static string RunTestsPath() {
			return Path.Combine(FixtureFolder.RepoRoot(),"tools","run-tests.ps1");
		}

		[TestMethod]
		public void RunTests_CountsTheBlockSummary_TheLastBlock_WithSkipped() {
			Dictionary<string,string> dictSamples=new Dictionary<string,string>();
			dictSamples["skipped"]=Lines("VSTest version 18.11.0 (x64)","","Starting test execution, please wait...",
				"A total of 1 test files matched the specified pattern.","  Passed First [1 ms]","  Skipped Second [< 1 ms]","",
				"Test Run Successful.","Total tests: 2","     Passed: 1","    Skipped: 1"," Total time: 1.2034 Seconds");
			//An assembly whose tests are all inconclusive has no "Passed:" line (WindowChecksTests): 0 passed, not unknown.
			dictSamples["allskipped"]=Lines("Test Run Successful.","Total tests: 29","    Skipped: 29"," Total time: 1.6388 Seconds");
			dictSamples["failed"]=Lines("  Failed Third [3 ms]","  Error Message:","   Assert.AreEqual failed.","","Test Run Failed.",
				"Total tests: 5","     Passed: 3","     Failed: 2"," Total time: 2.5 Seconds");
			//A test's own output above the summary that looks like one (and the adoption rehearsal's "Total: 6.6 s") does not count.
			dictSamples["decoy"]=Lines("Standard Output Messages:","Total tests: 99","     Passed: 99"," Total time: 1 Seconds","Total: 6.6 s",
				"Passed: 7","","Test Run Successful.","Total tests: 3","     Passed: 3"," Total time: 0.9 Seconds");
			dictSamples["lf"]=string.Join("\n","Test Run Successful.","Total tests: 4","     Passed: 4"," Total time: 1 Seconds","");
			Dictionary<string,string> dictCounts=CountSamples(dictSamples);
			Assert.AreEqual("2/1/0/1/block",dictCounts["skipped"]);
			Assert.AreEqual("29/0/0/29/block",dictCounts["allskipped"]);
			Assert.AreEqual("5/3/2/0/block",dictCounts["failed"]);
			Assert.AreEqual("3/3/0/0/block",dictCounts["decoy"]);
			Assert.AreEqual("4/4/0/0/block",dictCounts["lf"]);
		}

		[TestMethod]
		public void RunTests_CountsTheOneLineSummaries_AddedUp() {
			Dictionary<string,string> dictSamples=new Dictionary<string,string>();
			dictSamples["one"]=Lines("  Passed First [1 ms]",
				"Passed!  - Failed:     0, Passed:   138, Skipped:     1, Total:   139, Duration: 27 s - ProjectFileTests.dll (net48)");
			dictSamples["two"]=Lines("Failed!  - Failed:     2, Passed:     3, Skipped:     0, Total:     5, Duration: 1 s - A.Tests.dll (net48)",
				"Skipped! - Failed:     0, Passed:     0, Skipped:    29, Total:    29, Duration: 2 s - B.Tests.dll (net48)");
			Dictionary<string,string> dictCounts=CountSamples(dictSamples);
			Assert.AreEqual("139/138/0/1/line",dictCounts["one"]);
			Assert.AreEqual("34/3/2/29/line",dictCounts["two"]);
		}

		[TestMethod]
		public void RunTests_ARunWithoutSummary_HasUnknownCounts() {
			Dictionary<string,string> dictSamples=new Dictionary<string,string>();
			dictSamples["aborted"]=Lines("  Passed First [1 ms]","The active test run was aborted. Reason: Test host process crashed","","Test Run Aborted.");
			dictSamples["unknown"]=Lines("Test Run Aborted.","Total tests: Unknown","     Passed: 1"," Total time: 1 Seconds");
			Dictionary<string,string> dictCounts=CountSamples(dictSamples);
			Assert.AreEqual("?/?/?/?/none",dictCounts["aborted"]);
			Assert.AreEqual("?/?/?/?/none",dictCounts["unknown"]);
		}

		[TestMethod]
		public void RunTests_ComparesWithAnEarlierResultsFile_OfEitherRunner() {
			string folder=NewWorkFolder();
			try {
				//The runner before S4-2 wrote no skipped= and passed=? for an assembly without a "Passed:" line.
				File.WriteAllText(Path.Combine(folder,"earlier.txt"),Lines("RUN mode=Full root=C:\\repo started=2026-11-12T10:00:00 assemblies=4",
					"WindowChecksTests exit=0 total=29 passed=? failed=0 seconds=3","EhrTests exit=0 total=129 passed=129 failed=0 seconds=60",
					"ProjectFileTests exit=0 total=139 passed=138 failed=0 seconds=28","GoneTests exit=0 total=3 passed=3 failed=0 seconds=1",
					"DONE ok=True finished=2026-11-12T11:00:00"));
				File.WriteAllText(Path.Combine(folder,"now.txt"),Lines("RUN mode=Full root=C:\\repo started=2026-11-14T10:00:00 assemblies=4",
					"WindowChecksTests exit=0 total=29 passed=0 failed=0 skipped=29 seconds=3",
					"EhrTests exit=0 total=208 passed=208 failed=0 skipped=0 seconds=80",
					"ProjectFileTests exit=1 total=139 passed=137 failed=1 skipped=1 seconds=28",
					"NewTests exit=0 total=2 passed=2 failed=0 skipped=0 seconds=1"));
				List<string> listOutput=RunPowerShell(Harness(RunTestsPath(),folder,
					"$Out=Join-Path $work 'now.txt'\r\nWrite-Comparison (Join-Path $work 'earlier.txt')\r\n"
					+"Get-Content $Out | Where-Object { $_ -like 'COMPARE *' } | ForEach-Object { 'out=' + $_ }"));
				List<string> listCompare=listOutput.Where(x => x.StartsWith("out=COMPARE ")).Select(x => x.Substring("out=COMPARE ".Length)).ToList();
				CollectionAssert.AreEqual(new[] {
					"with="+Path.Combine(folder,"earlier.txt"),
					"EhrTests differs total 129->208, passed 129->208",
					"NewTests new total=2 passed=2 failed=0 skipped=0",
					"ProjectFileTests differs passed 138->137, failed 0->1",
					"WindowChecksTests same total=29 passed=0 failed=0 skipped=29",
					"same=1 differs=2 new=1" },listCompare,string.Join(" | ",listOutput));
			}
			finally {
				Directory.Delete(folder,true);
			}
		}

		[TestMethod]
		public void RunTests_StartsVSTestWithoutTelemetry() {
			//The environment the runner sets before its first vstest, whatever this process had (here an opted-in vstest telemetry).
			List<string> listOutput=RunPowerShell(Harness(RunTestsPath(),Path.GetTempPath(),
				"$env:VSTEST_TELEMETRY_OPTEDIN='1'\r\n$env:TESTINGPLATFORM_TELEMETRY_OPTOUT=$null\r\n$env:DOTNET_CLI_TELEMETRY_OPTOUT='0'\r\n"
				+"$env:OPMS_TEST_RUNNER=$null\r\n$env:OPMS_REQUIRE_BUILD_OUTPUT=$null\r\n"
				+"Set-TestRunEnvironment\r\n"
				+"'T='+$env:TESTINGPLATFORM_TELEMETRY_OPTOUT\r\n'D='+$env:DOTNET_CLI_TELEMETRY_OPTOUT\r\n"
				+"'V='+[string]$env:VSTEST_TELEMETRY_OPTEDIN\r\n'R='+$env:OPMS_TEST_RUNNER\r\n'B='+[string]$env:OPMS_REQUIRE_BUILD_OUTPUT\r\n"
				+"$RequireBuildOutput=$true\r\nSet-TestRunEnvironment\r\n'B2='+[string]$env:OPMS_REQUIRE_BUILD_OUTPUT"));
			CollectionAssert.AreEqual(new[] { "T=1","D=1","V=","R=run-tests.ps1","B=","B2=1" },listOutput,string.Join(" | ",listOutput));
			//The script sets it before it starts the first vstest: a statement of the script itself, not of a function, above the one Start-Process.
			string text=File.ReadAllText(RunTestsPath()).Replace("\r\n","\n");
			int indexSet=text.IndexOf("\nSet-TestRunEnvironment\n",StringComparison.Ordinal);
			int indexStart=text.IndexOf("Start-Process -FilePath $vstest",StringComparison.Ordinal);
			Assert.IsTrue(indexSet>0,"run-tests.ps1 calls Set-TestRunEnvironment at the top level.");
			Assert.IsTrue(indexStart>indexSet,"run-tests.ps1 starts vstest after Set-TestRunEnvironment.");
			Assert.AreEqual(indexStart,text.LastIndexOf("Start-Process",StringComparison.Ordinal),"run-tests.ps1 starts vstest in one place.");
			Assert.IsTrue(text.Contains("'/Platform:x86'"),"run-tests.ps1 runs the tests in the 32-bit test host (S4-2 keeps /Platform:x86).");
		}

		[TestMethod]
		public void InitializeBuildEnvironment_GivesTheLanesTestRunsNoTelemetry() {
			//The lane (-Test) runs vstest in the environment of the repository's Initialize-BuildEnvironment. An empty folder as the repository:
			//no global.json, so no SDK is looked for, and OPMS_NUGET_OFFLINE set, so git is not asked.
			string folder=NewWorkFolder();
			try {
				string helper=Path.Combine(FixtureFolder.RepoRoot(),"tools","Initialize-BuildEnvironment.ps1");
				List<string> listOutput=RunPowerShell("$ErrorActionPreference='Stop'\r\n. "+Quote(helper)+"\r\n"
					+"$env:VSTEST_TELEMETRY_OPTEDIN='1'\r\n$env:TESTINGPLATFORM_TELEMETRY_OPTOUT=$null\r\n$env:DOTNET_CLI_TELEMETRY_OPTOUT=$null\r\n"
					+"$env:OPMS_NUGET_OFFLINE="+Quote(folder)+"\r\n"
					+"$null=Initialize-BuildEnvironment -RepoRoot "+Quote(folder)+"\r\n"
					+"'T='+$env:TESTINGPLATFORM_TELEMETRY_OPTOUT\r\n'D='+$env:DOTNET_CLI_TELEMETRY_OPTOUT\r\n'V='+[string]$env:VSTEST_TELEMETRY_OPTEDIN");
				CollectionAssert.AreEqual(new[] { "T=1","D=1","V=" },listOutput,string.Join(" | ",listOutput));
			}
			finally {
				Directory.Delete(folder,true);
			}
		}

		///<summary>Each sample is written to a .log file and counted by Get-VsTestCounts: total/passed/failed/skipped/format.</summary>
		private static Dictionary<string,string> CountSamples(Dictionary<string,string> dictSamples) {
			string folder=NewWorkFolder();
			try {
				foreach(KeyValuePair<string,string> pair in dictSamples) {
					File.WriteAllText(Path.Combine(folder,pair.Key+".log"),pair.Value,new UTF8Encoding(false));
				}
				List<string> listOutput=RunPowerShell(Harness(RunTestsPath(),folder,
					"foreach($log in Get-ChildItem -LiteralPath $work -Filter *.log){\r\n"
					+"\t$c=Get-VsTestCounts ((Get-Content -LiteralPath $log.FullName -Raw)+'')\r\n"
					+"\t'{0}={1}/{2}/{3}/{4}/{5}' -f $log.BaseName,$c.Total,$c.Passed,$c.Failed,$c.Skipped,$c.Format\r\n}"));
				Dictionary<string,string> dictCounts=new Dictionary<string,string>();
				foreach(string line in listOutput) {
					int index=line.IndexOf('=');
					Assert.IsTrue(index>0,"Unexpected line: "+line);
					dictCounts[line.Substring(0,index)]=line.Substring(index+1);
				}
				Assert.AreEqual(dictSamples.Count,dictCounts.Count,string.Join(" | ",listOutput));
				return dictCounts;
			}
			finally {
				Directory.Delete(folder,true);
			}
		}

		///<summary>A script that defines every function of the script at pathScript (from its syntax tree, without running the script, which would
		///start a test run), sets $work to the folder and then runs the body. A syntax error in the script fails the harness.</summary>
		private static string Harness(string pathScript,string folderWork,string body) {
			return "$ErrorActionPreference='Stop'\r\n"
				+"$parseErrors=$null\r\n"
				+"$ast=[System.Management.Automation.Language.Parser]::ParseFile("+Quote(pathScript)+",[ref]$null,[ref]$parseErrors)\r\n"
				+"if($parseErrors){ $parseErrors | ForEach-Object { 'parse error: '+$_.Message }; exit 2 }\r\n"
				+"foreach($f in $ast.FindAll({ param($x) $x -is [System.Management.Automation.Language.FunctionDefinitionAst] },$true)){ . ([scriptblock]::Create($f.Extent.Text)) }\r\n"
				+"$work="+Quote(folderWork)+"\r\n"
				+body+"\r\n";
		}

		///<summary>Runs the script in Windows PowerShell (the runners' PowerShell) without a profile or a window and returns its output lines.
		///Fails the test on a non-zero exit code or after two minutes.</summary>
		private static List<string> RunPowerShell(string script) {
			ProcessStartInfo processStartInfo=new ProcessStartInfo("powershell.exe");
			processStartInfo.Arguments="-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand "
				+Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
			processStartInfo.UseShellExecute=false;
			processStartInfo.RedirectStandardOutput=true;
			processStartInfo.RedirectStandardError=true;
			processStartInfo.CreateNoWindow=true;
			using(Process process=Process.Start(processStartInfo)) {
				Task<string> taskOut=process.StandardOutput.ReadToEndAsync();
				Task<string> taskErr=process.StandardError.ReadToEndAsync();
				if(!process.WaitForExit(120000)) {
					process.Kill();
					Assert.Fail("PowerShell did not finish in 2 minutes.");
				}
				process.WaitForExit();
				Assert.AreEqual(0,process.ExitCode,taskOut.Result+taskErr.Result);
				return taskOut.Result.Split(new[] { "\r\n","\n" },StringSplitOptions.RemoveEmptyEntries).Select(x => x.TrimEnd()).Where(x => x!="").ToList();
			}
		}

		private static string Lines(params string[] arrayLines) {
			return string.Join("\r\n",arrayLines)+"\r\n";
		}

		private static string Quote(string text) {
			return "'"+text.Replace("'","''")+"'";
		}

		private static string NewWorkFolder() {
			string folder=Path.Combine(Path.GetTempPath(),"opms-runner-tests-"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(folder);
			return folder;
		}
	}
}
