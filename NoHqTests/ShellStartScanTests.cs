// S3a-2 (net10 plan): every start of a program, document or URL in shipped code says whether it goes through the shell. ProcessStartInfo's
// UseShellExecute is true by default on .NET Framework and false on .NET, and only the shell opens documents and URLs and asks for consent to a
// program that needs elevation; so no code calls Process.Start(string...) outside CodeBase.ODFileUtils (ShellOpen and ProcessStart set it), and
// every new Process()/new ProcessStartInfo( block sets UseShellExecute explicitly. A source scan of the C# projects of the solution filter
// (shipped, plus RdlDesign, DocumentationBuilder and CentralManager), except the tests, the tools, the third-party sources, UnitTests,
// UnitTestsCore and xCrudGenerator, whose starts redirect output or never ship. It needs no build, database or network, and starts nothing.
// S3a-2f tightened it: Diagnostics.Process.Start( and Process.Start as a method group are starts; a using that hides Process is refused;
// target-typed new() of a Process or ProcessStartInfo is a construction; UseShellExecute counts only on the constructed object's own name or
// initializer, within its scope and before its name gets a new object; Process.Start(x) needs x's last assignment to be a checked construction.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NoHqTests {
	[TestClass]
	public class ShellStartScanTests {
		///<summary>The one file that may call Process.Start with a path: the helper itself. Its own ProcessStartInfo blocks are still checked.</summary>
		private const string PATH_HELPER=@"CodeBase\Utilities\ODFileUtils.cs";
		///<summary>Sites another package of the plan owns and fixes, reported but not failed until its fix is on main. Empty: PrefL's start of
		///the update file copier (S3a-10's) was the one entry and S3a-10 merged first (fa5640fa9), so the lead removed it at the scan's merge.
		///</summary>
		private static readonly string[] _arrayPathsKnownPending=new string[0];
		///<summary>How many lines after a construction the scan looks for UseShellExecute when no start call comes first.</summary>
		private const int MAX_BLOCK_LINES=80;

		///<summary>The qualifier Process may carry: System.Diagnostics., or Diagnostics. in a file with using System; (S3a-2f), global:: in front.</summary>
		private const string PATTERN_QUALIFIER=@"(?:global\s*::\s*)?(?:(?:System\s*\.\s*)?Diagnostics\s*\.\s*)?";
		///<summary>A dotted name assigned to: group 1, without a leading this. (x, _x, x.StartInfo).</summary>
		private const string PATTERN_TARGET=@"(?<![\w.])(?:this\s*\.\s*)?([A-Za-z_]\w*(?:\s*\.\s*[A-Za-z_]\w*)*)";
		private static readonly Regex _regexStaticStart=new Regex(@"(?<![\w.])"+PATTERN_QUALIFIER+@"Process\s*\.\s*Start\s*\(");
		///<summary>Process.Start not called but passed or assigned as a method group (S3a-2f): any overload, string ones among them.</summary>
		private static readonly Regex _regexStartMethodGroup=new Regex(@"(?<![\w.])"+PATTERN_QUALIFIER+@"Process\s*\.\s*Start\b(?!\s*\()");
		///<summary>using static System.Diagnostics.Process; or an alias of Process or of System.Diagnostics, which would hide a start from the
		///other rules (S3a-2f).</summary>
		private static readonly Regex _regexUsingHidesProcess=new Regex(@"\busing\s+(?:static\s+(?:global\s*::\s*)?System\s*\.\s*Diagnostics\s*\.\s*Process"
			+@"|[A-Za-z_]\w*\s*=\s*(?:global\s*::\s*)?System\s*\.\s*Diagnostics(?:\s*\.\s*Process)?)\s*;");
		private static readonly Regex _regexConstruction=new Regex(@"\bnew\s+"+PATTERN_QUALIFIER+@"(Process|ProcessStartInfo)\b(?=\s*[({])");
		///<summary>A target-typed new() assigned to a name (S3a-2f): a Process or ProcessStartInfo when the name ends in StartInfo or the file declares
		///it as one.</summary>
		private static readonly Regex _regexTargetTypedNew=new Regex(PATTERN_TARGET+@"\s*=(?![=>])\s*new\b(?=\s*[({])");
		///<summary>The name a construction is assigned to, at the end of the code before it.</summary>
		private static readonly Regex _regexTargetBefore=new Regex(PATTERN_TARGET+@"\s*=\s*\z");
		private static readonly Regex _regexStartCall=new Regex(@"\.\s*Start\s*\(|\bODFileUtils\s*\.\s*ProcessStart\s*\(");
		private static readonly Regex _regexUseShellExecute=new Regex(@"\bUseShellExecute\s*=");
		private static readonly Regex _regexStartInfoFromVariable=new Regex(@"\bStartInfo\s*=\s*[A-Za-z_]\w*\s*[;,}]");

		[TestMethod]
		public void ShippedSource_EveryStartSetsUseShellExecute() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			List<string> listFolders=GetScannedProjectFolders(pathRoot);
			List<string> listNames=listFolders.Select(x => Path.GetFileName(x)).ToList();
			foreach(string name in new[] { "OpenPmsDentalBusiness","OpenPmsDental","WpfControlsOD","CodeBase","RdlDesign","RdlViewer","DocumentationBuilder",
				"CentralManager","UpdateFileCopier" })
			{
				Assert.IsTrue(listNames.Contains(name),"The scan must include "+name+".");
			}
			foreach(string name in new[] { "xCrudGenerator","UnitTests","UnitTestsCore","NoHqTests" }) {
				Assert.IsFalse(listNames.Contains(name),"The scan leaves out "+name+".");
			}
			List<string> listOffenders=new List<string>();
			List<string> listPending=new List<string>();
			int countFiles=0;
			int countConstructions=0;
			foreach(string folder in listFolders) {
				foreach(string path in Directory.EnumerateFiles(folder,"*.cs",SearchOption.AllDirectories)) {
					if(IsBuildFolder(folder,path)) {
						continue;
					}
					countFiles++;
					string pathRelative=path.Substring(pathRoot.Length+1);
					ScanResult scanResult=Scan(File.ReadAllLines(path),pathRelative.Equals(PATH_HELPER,StringComparison.OrdinalIgnoreCase));
					countConstructions+=scanResult.CountConstructions;
					List<string> listFound=scanResult.ListProblems.Select(x => pathRelative+":"+x).ToList();
					if(_arrayPathsKnownPending.Contains(pathRelative,StringComparer.OrdinalIgnoreCase)) {
						listPending.AddRange(listFound);
						continue;
					}
					listOffenders.AddRange(listFound);
				}
			}
			Assert.IsTrue(countFiles>1000,"Expected the shipped source files, found "+countFiles+".");
			Assert.IsTrue(countConstructions>=30,"Expected the new Process()/new ProcessStartInfo( blocks of shipped code, found "+countConstructions+".");
			if(listPending.Count>0) {
				Console.WriteLine("Known pending (owned by another package): "+string.Join(", ",listPending));
			}
			Assert.AreEqual(0,listOffenders.Count,"UseShellExecute is true on .NET Framework and false on .NET. Open documents and URLs with "
				+"ODFileUtils.ShellOpen, start programs with ODFileUtils.ProcessStart, and set UseShellExecute in every new Process()/new "
				+"ProcessStartInfo( block (true unless it redirects a standard stream): "+string.Join(", ",listOffenders));
		}

		[TestMethod]
		public void KnownPending_FilesExist() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			foreach(string pathRelative in _arrayPathsKnownPending) {
				Assert.IsTrue(File.Exists(Path.Combine(pathRoot,pathRelative)),"A known pending site that no longer exists: "+pathRelative);
			}
		}

		#region Self-test of the scanner
		[TestMethod]
		public void Scan_StaticStartWithAPath_IsFound() {
			AssertProblems(new[] { "Process.Start(url);" },"1 Process.Start");
			AssertProblems(new[] { "System.Diagnostics.Process.Start(\"Example.exe\",\"-a\");" },"1 Process.Start");
			AssertProblems(new[] { "using(Process process=Process.Start(path,args)) {","}" },"1 Process.Start");
			AssertProblems(new[] { "x=Process . Start ( path );" },"1 Process.Start");
			AssertProblems(new[] { "ODException.SwallowAnyException(() => Process.Start(target));" },"1 Process.Start");
		}

		[TestMethod]
		public void Scan_StaticStartWithAStartInfo_IsAllowed() {
			AssertProblems(new[] { "Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });" });
			AssertProblems(new[] { "System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute=true });" });
			AssertProblems(new[] {
				"ProcessStartInfo psi=new ProcessStartInfo(path);",
				"psi.UseShellExecute=false;",
				"psi.RedirectStandardOutput=true;",
				"_process=Process.Start(psi);"
			});
		}

		[TestMethod]
		public void Scan_StaticStartWithAnUndeclaredVariable_IsFound() {
			AssertProblems(new[] { "string psi=\"x\";","Process.Start(psi);" },"2 Process.Start");
		}

		[TestMethod]
		public void Scan_HelperFile_MayCallProcessStartWithAPath() {
			ScanResult scanResult=Scan(new[] { "return Process.Start(path,args);" },true);
			Assert.AreEqual(0,scanResult.ListProblems.Count);
		}

		[TestMethod]
		public void Scan_BlockWithoutUseShellExecute_IsFound() {
			AssertProblems(new[] {
				"Process process=new Process();",
				"process.StartInfo.FileName=path;",
				"process.Start();"
			},"1 new Process");
			AssertProblems(new[] {
				"ProcessStartInfo processStartInfo=new ProcessStartInfo(path);",
				"processStartInfo.Arguments=\"-a\";",
				"Process.Start(processStartInfo);"
			},"1 new ProcessStartInfo");
			AssertProblems(new[] {
				"using Process process=new Process {",
				"\tStartInfo=new ProcessStartInfo {",
				"\t\tFileName=exePath,",
				"\t}",
				"};",
				"process.Start();"
			},"1 new Process","2 new ProcessStartInfo");
		}

		[TestMethod]
		public void Scan_BlockWithUseShellExecute_IsAllowed() {
			AssertProblems(new[] {
				"Process process=new Process();",
				"process.StartInfo.UseShellExecute=true;",
				"process.StartInfo.FileName=path;",
				"ODFileUtils.ProcessStart(process);"
			});
			AssertProblems(new[] {
				"using Process process=new Process {",
				"\tStartInfo=new ProcessStartInfo {",
				"\t\tFileName=exePath,",
				"\t\tUseShellExecute=true",
				"\t}",
				"};",
				"process.Start();"
			});
			AssertProblems(new[] {
				"Process process=new Process();",
				"if(isSilent) {",
				"\tprocess.StartInfo.UseShellExecute=false;",
				"\tprocess.StartInfo.RedirectStandardInput=true;",
				"}",
				"else {",
				"\tprocess.StartInfo.UseShellExecute=true;",
				"}",
				"process.Start();"
			});
		}

		[TestMethod]
		public void Scan_ProcessWithACheckedStartInfo_IsAllowed() {
			AssertProblems(new[] {
				"ProcessStartInfo processStartInfo=new ProcessStartInfo(path);",
				"processStartInfo.UseShellExecute=true;",
				"Process process=new Process();",
				"process.StartInfo=processStartInfo;",
				"process.Start();"
			});
		}

		[TestMethod]
		public void Scan_EachBlockIsCheckedOnItsOwn() {
			//The first block's UseShellExecute does not cover the second.
			AssertProblems(new[] {
				"Process process1=new Process();",
				"process1.StartInfo.UseShellExecute=true;",
				"process1.Start();",
				"Process process2=new Process();",
				"process2.StartInfo.FileName=path;",
				"process2.Start();"
			},"4 new Process");
		}

		[TestMethod]
		public void Scan_BlockWithoutAStart_IsCheckedOverItsLines() {
			List<string> listLines=new List<string> { "_process=new Process();" };
			for(int i=0;i<MAX_BLOCK_LINES+5;i++) {
				listLines.Add("x++;");
			}
			listLines.Add("_process.StartInfo.UseShellExecute=true;");
			AssertProblems(listLines.ToArray(),"1 new Process");
		}

		[TestMethod]
		public void Scan_InstanceStartsAndTheHelper_AreNotStaticStarts() {
			AssertProblems(new[] { "process.Start();","_processOnScreenKeyboard.Start();","ODFileUtils.ProcessStart(path);","ODFileUtils.ShellOpen(url);",
				"stopwatch.Start();","MyProcess.Start(x);" });
		}

		[TestMethod]
		public void Scan_CommentsAndStrings_AreNotCode() {
			AssertProblems(new[] {
				"//Process.Start(path);",
				"x=1;//Process p=new Process();",
				"///<summary>Calls Process.Start(path).</summary>",
				"/* Process.Start(path);",
				"Process p=new Process(); */",
				"string s=\"Process.Start(path)\";",
				"string t=@\"new Process() \"\" Process.Start(x)\";",
				"string u=@\"first line",
				"Process.Start(path); second line\";",
				"char c='\"';",
				"string v=\"\\\"Process.Start(x)\";"
			});
		}

		[TestMethod]
		public void Scan_CodeAfterAStringOrComment_IsStillScanned() {
			AssertProblems(new[] { "string s=\"//\";Process.Start(path);" },"1 Process.Start");
			AssertProblems(new[] { "/* note */ Process.Start(path);" },"1 Process.Start");
			AssertProblems(new[] { "x=$\"{a}\";Process.Start(path);" },"1 Process.Start");
		}

		[TestMethod]
		public void RemoveCommentsAndStrings_KeepsTheCode() {
			bool isInBlockComment=false;
			bool isInVerbatimString=false;
			Assert.AreEqual("a=\"\";b();",RemoveCommentsAndStrings("a=\"x//y\";b();//z",ref isInBlockComment,ref isInVerbatimString));
			Assert.AreEqual("a=@\"",RemoveCommentsAndStrings("a=@\"line one",ref isInBlockComment,ref isInVerbatimString));
			Assert.IsTrue(isInVerbatimString);
			Assert.AreEqual("\";c();",RemoveCommentsAndStrings("line \"\"two\";c();",ref isInBlockComment,ref isInVerbatimString));
			Assert.IsFalse(isInVerbatimString);
			Assert.AreEqual("d();",RemoveCommentsAndStrings("d();/*e();",ref isInBlockComment,ref isInVerbatimString));
			Assert.IsTrue(isInBlockComment);
			Assert.AreEqual(" f();",RemoveCommentsAndStrings("*/ f();",ref isInBlockComment,ref isInVerbatimString));
			Assert.IsFalse(isInBlockComment);
		}

		//S3a-2f: the tightened rules. Each has a planted bad line that fails and a good line that passes.

		[TestMethod]
		public void Scan_DiagnosticsQualifiedStart_IsFound() {
			//After using System; the call can be written Diagnostics.Process.Start(, which the first lookbehind (no '.' before Process) let through.
			AssertProblems(new[] { "Diagnostics.Process.Start(path);" },"1 Process.Start");
			AssertProblems(new[] { "x=global::System.Diagnostics.Process.Start(url,args);" },"1 Process.Start");
			AssertProblems(new[] { "Diagnostics.Process.Start(new Diagnostics.ProcessStartInfo(url) { UseShellExecute=true });" });
		}

		[TestMethod]
		public void Scan_StartAsAMethodGroup_IsFound() {
			AssertProblems(new[] { "ODException.SwallowAnyException(Process.Start);" },"1 method group");
			AssertProblems(new[] { "Func<string,Process> funcStart=System.Diagnostics.Process.Start;" },"1 method group");
			AssertProblems(new[] { "listUrls.Select(Diagnostics.Process.Start).ToList();" },"1 method group");
			AssertProblems(new[] { "process.StartInfo.FileName=path;","Process[] arrayProcesses=Process.GetProcessesByName(name);","ODFileUtils.ProcessStart(path);",
				"Process process=Process.GetCurrentProcess();","DateTime dateTime=process.StartTime;" });
		}

		[TestMethod]
		public void Scan_UsingThatHidesProcess_IsFound() {
			AssertProblems(new[] { "using static System.Diagnostics.Process;" },"1 using");
			AssertProblems(new[] { "using Proc=System.Diagnostics.Process;" },"1 using");
			AssertProblems(new[] { "using Diag = global::System.Diagnostics;" },"1 using");
			Assert.AreEqual(1,Scan(new[] { "using static System.Diagnostics.Process;" },true).ListProblems.Count,"The helper file too.");
			AssertProblems(new[] { "using System;","using System.Diagnostics;","using static System.Math;","using EventSourceAlias=System.Diagnostics.Tracing;" });
		}

		[TestMethod]
		public void Scan_TargetTypedNew_IsChecked() {
			AssertProblems(new[] { "Process process=new();","process.StartInfo.FileName=path;","process.Start();" },"1 new Process");
			AssertProblems(new[] { "ProcessStartInfo processStartInfo=new(path);","ODFileUtils.ProcessStart(processStartInfo);" },"1 new ProcessStartInfo");
			AssertProblems(new[] {
				"using Process process=new() {",
				"\tStartInfo=new() { FileName=path }",
				"};",
				"process.Start();"
			},"1 new Process","2 new ProcessStartInfo");
			AssertProblems(new[] { "private Process _process;","private void Open() {","\t_process=new();","\t_process.Start();","}" },"3 new Process");
			//Good lines.
			AssertProblems(new[] { "Process process=new();","process.StartInfo.UseShellExecute=true;","process.Start();" });
			AssertProblems(new[] { "ProcessStartInfo processStartInfo=new(path) { UseShellExecute=true };","Process.Start(processStartInfo);" });
			AssertProblems(new[] { "using Process process=new() {","\tStartInfo=new() { FileName=path, UseShellExecute=true }","};","process.Start();" });
			Assert.AreEqual(2,Scan(new[] { "Process process=new() {","\tStartInfo=new() { UseShellExecute=true }","};" },false).CountConstructions);
			//Other types' new() are not constructions.
			ScanResult scanResult=Scan(new[] { "var point=new { X=1 };","Size size=new(1,2);","List<int> listNums=new();" },false);
			Assert.AreEqual(0,scanResult.CountConstructions);
			Assert.AreEqual(0,scanResult.ListProblems.Count);
		}

		[TestMethod]
		public void Scan_UseShellExecuteOfAnotherObject_DoesNotCount() {
			//A start info's block holds another start info's flag.
			AssertProblems(new[] {
				"ProcessStartInfo processStartInfo1=new ProcessStartInfo(path1);",
				"ProcessStartInfo processStartInfo2=new ProcessStartInfo(path2);",
				"processStartInfo2.UseShellExecute=true;",
				"Process.Start(processStartInfo1);"
			},"1 new ProcessStartInfo");
			//No start within the method: the block ends with its scope, so it cannot borrow the next method's flag.
			AssertProblems(new[] {
				"private Process GetProcess() {",
				"\tProcess process=new Process();",
				"\tprocess.StartInfo.FileName=path;",
				"\treturn process;",
				"}",
				"private void StartOther() {",
				"\tProcess process=new Process();",
				"\tprocess.StartInfo.UseShellExecute=true;",
				"\tODFileUtils.ProcessStart(process);",
				"}"
			},"2 new Process");
			//The flag in another construction's initializer.
			AssertProblems(new[] {
				"Process process=new Process();",
				"ProcessStartInfo processStartInfoOther=new ProcessStartInfo(path) { UseShellExecute=true };",
				"process.StartInfo.FileName=path;",
				"process.Start();"
			},"1 new Process");
			//The same name given a new object before the flag: the flag is the second object's.
			AssertProblems(new[] {
				"pibridge=new Process();",
				"pibridge.StartInfo.FileName=path;",
				"pibridge=new Process();",
				"pibridge.StartInfo.UseShellExecute=true;",
				"pibridge.Start();"
			},"1 new Process");
			//Good lines: the flag on the object's own name.
			AssertProblems(new[] {
				"Process proc=new Process();",
				"proc.StartInfo=new ProcessStartInfo(path,\"-a\");",
				"proc.StartInfo.UseShellExecute=true;",
				"proc.Start();"
			});
			AssertProblems(new[] { "this._process=new Process();","_process.StartInfo.UseShellExecute=true;","_process.Start();" });
			AssertProblems(new[] { "Process process=new Process();","process.StartInfo=new ProcessStartInfo(path) { UseShellExecute=true };","process.Start();" });
		}

		[TestMethod]
		public void Scan_StaticStartWithAStartInfoBuiltElsewhere_IsFound() {
			//A parameter, a loop variable and a method's return value are start infos the scan never checked.
			AssertProblems(new[] { "public static void Open(ProcessStartInfo processStartInfo) {","\tProcess.Start(processStartInfo);","}" },"2 Process.Start");
			AssertProblems(new[] { "ProcessStartInfo processStartInfo=ForkReleases.GetFileCopierStartInfo(path,args);","Process.Start(processStartInfo);" },
				"2 Process.Start");
			AssertProblems(new[] { "foreach(ProcessStartInfo processStartInfo in listStartInfos) {","\tProcess.Start(processStartInfo);","}" },"2 Process.Start");
			//Reassigned after its construction.
			AssertProblems(new[] {
				"ProcessStartInfo processStartInfo=new ProcessStartInfo(path) { UseShellExecute=true };",
				"processStartInfo=GetOtherStartInfo();",
				"Process.Start(processStartInfo);"
			},"3 Process.Start");
			//Good lines: the last assignment before the call is a checked construction.
			AssertProblems(new[] {
				"ProcessStartInfo processStartInfo;",
				"processStartInfo=new ProcessStartInfo(path);",
				"processStartInfo.UseShellExecute=true;",
				"Process.Start(processStartInfo);"
			});
			AssertProblems(new[] {
				"ProcessStartInfo processStartInfo=GetOtherStartInfo();",
				"processStartInfo=new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute=true };",
				"Process.Start(this.processStartInfo);"
			});
		}

		///<summary>Scans lines (not the helper file) and checks the problems found, each as "line kind".</summary>
		private static void AssertProblems(string[] arrayLines,params string[] arrayExpected) {
			ScanResult scanResult=Scan(arrayLines,false);
			CollectionAssert.AreEqual(arrayExpected,scanResult.ListProblems.Select(x => x.Substring(0,x.IndexOf(':'))+" "+GetKind(x)).ToArray(),
				"Found: "+string.Join(" | ",scanResult.ListProblems));
		}

		private static string GetKind(string problem) {
			if(problem.Contains("using directive")) {
				return "using";
			}
			if(problem.Contains("Process.Start method group")) {
				return "method group";
			}
			if(problem.Contains("Process.Start(")) {
				return "Process.Start";
			}
			if(problem.Contains("new ProcessStartInfo(")) {
				return "new ProcessStartInfo";
			}
			return "new Process";
		}
		#endregion Self-test of the scanner

		private class ScanResult {
			///<summary>Each as "line: what".</summary>
			public List<string> ListProblems=new List<string>();
			public int CountConstructions;
		}

		///<summary>A new Process(...)/new ProcessStartInfo(...), or a target-typed new() of one.</summary>
		private class Construction {
			///<summary>Where new starts in the code.</summary>
			public int Index;
			///<summary>Just after the type name (after new for new()): the ( or { follows.</summary>
			public int IndexAfterType;
			///<summary>"Process" or "ProcessStartInfo".</summary>
			public string Kind;
			///<summary>The name it is assigned to, without this. and white space (process, _process, proc.StartInfo, or StartInfo inside an
			///initializer), or null when it is assigned to no name (an argument, a return value).</summary>
			public string Target;
			public bool IsTargetTyped;
		}

		///<summary>Scans one file's lines. isHelper: the file is ODFileUtils.cs, which may call Process.Start with a path.</summary>
		private static ScanResult Scan(string[] arrayLinesRaw,bool isHelper) {
			ScanResult scanResult=new ScanResult();
			bool isInBlockComment=false;
			bool isInVerbatimString=false;
			StringBuilder stringBuilder=new StringBuilder();
			List<int> listLineStarts=new List<int>();
			for(int i=0;i<arrayLinesRaw.Length;i++) {
				listLineStarts.Add(stringBuilder.Length);
				stringBuilder.Append(RemoveCommentsAndStrings(arrayLinesRaw[i],ref isInBlockComment,ref isInVerbatimString)).Append('\n');
			}
			string code=stringBuilder.ToString();
			//Each as (index in the code, what), put in the order of the code at the end.
			List<Tuple<int,string>> listFound=new List<Tuple<int,string>>();
			foreach(Match match in _regexUsingHidesProcess.Matches(code)) {
				listFound.Add(Tuple.Create(match.Index,"using directive that hides Process from the scan: "+Shorten(match.Value)));
			}
			if(!isHelper) {
				foreach(Match match in _regexStaticStart.Matches(code)) {
					string argument=GetRestOfCall(code,match.Index+match.Length);
					if(!IsStartInfoArgument(argument,code,match.Index)) {
						listFound.Add(Tuple.Create(match.Index,"Process.Start("+Shorten(argument)+") outside ODFileUtils"));
					}
				}
				foreach(Match match in _regexStartMethodGroup.Matches(code)) {
					listFound.Add(Tuple.Create(match.Index,"Process.Start method group outside ODFileUtils"));
				}
			}
			foreach(Construction construction in GetConstructions(code)) {
				scanResult.CountConstructions++;
				if(!SetsUseShellExecute(code,listLineStarts,construction)) {
					listFound.Add(Tuple.Create(construction.Index,"new "+construction.Kind+"( block "+(construction.IsTargetTyped ? "(target-typed new()) " : "")
						+"without UseShellExecute"));
				}
			}
			foreach(Tuple<int,string> tuple in listFound.OrderBy(x => x.Item1)) {
				scanResult.ListProblems.Add((GetLineIndex(listLineStarts,tuple.Item1)+1)+": "+tuple.Item2);
			}
			return scanResult;
		}

		///<summary>The constructions of the code, explicit (new Process(, new ProcessStartInfo {) and target-typed (Process process=new();
		///StartInfo=new() {), in the order of the code.</summary>
		private static List<Construction> GetConstructions(string code) {
			List<Construction> listConstructions=new List<Construction>();
			foreach(Match match in _regexConstruction.Matches(code)) {
				Construction construction=new Construction();
				construction.Index=match.Index;
				construction.IndexAfterType=match.Index+match.Length;
				construction.Kind=match.Groups[1].Value;
				int indexFrom=Math.Max(0,match.Index-300);
				Match matchTarget=_regexTargetBefore.Match(code.Substring(indexFrom,match.Index-indexFrom));
				construction.Target=matchTarget.Success ? Regex.Replace(matchTarget.Groups[1].Value,@"\s+","") : null;
				listConstructions.Add(construction);
			}
			foreach(Match match in _regexTargetTypedNew.Matches(code)) {
				string target=Regex.Replace(match.Groups[1].Value,@"\s+","");
				string kind=GetDeclaredKind(code,target);
				if(kind==null) {
					continue;//Another type: an anonymous object, a Size, a list.
				}
				Construction construction=new Construction();
				construction.Index=match.Index+match.Length-"new".Length;
				construction.IndexAfterType=match.Index+match.Length;
				construction.Kind=kind;
				construction.Target=target;
				construction.IsTargetTyped=true;
				listConstructions.Add(construction);
			}
			return listConstructions.OrderBy(x => x.Index).ToList();
		}

		///<summary>"Process" or "ProcessStartInfo" when a target-typed new() assigned to this name makes one: the name ends in StartInfo (a
		///Process's property, also inside an initializer) or the file declares the name (its last part) as one. Null otherwise.</summary>
		private static string GetDeclaredKind(string code,string target) {
			string[] arrayParts=target.Split('.');
			string nameLast=arrayParts[arrayParts.Length-1];
			if(nameLast=="StartInfo") {
				return "ProcessStartInfo";
			}
			Match match=Regex.Match(code,@"\b(?:"+PATTERN_QUALIFIER+@")(Process|ProcessStartInfo)\s+"+Regex.Escape(nameLast)+@"\b(?!\s*\()");
			return match.Success ? match.Groups[1].Value : null;
		}

		///<summary>True when the construction sets UseShellExecute itself: in its own initializer, or (assigned to a name) by an assignment to that
		///name's UseShellExecute in its block. A Process also passes when its StartInfo is a variable or a new ProcessStartInfo that sets it (each
		///ProcessStartInfo construction is checked on its own). An assignment to another object's UseShellExecute never counts (S3a-2f).</summary>
		private static bool SetsUseShellExecute(string code,List<int> listLineStarts,Construction construction) {
			int indexAfterConstruction=GetConstructionEnd(code,construction.IndexAfterType);
			string textConstruction=code.Substring(construction.Index,indexAfterConstruction-construction.Index);
			if(_regexUseShellExecute.IsMatch(textConstruction)) {
				return true;
			}
			bool isProcess=construction.Kind=="Process";
			if(isProcess && _regexStartInfoFromVariable.IsMatch(textConstruction)) {
				return true;//The StartInfo comes from a variable whose own new ProcessStartInfo( block is checked.
			}
			if(construction.Target==null) {
				return false;
			}
			string block=GetBlock(code,listLineStarts,construction,indexAfterConstruction);
			string patternName=GetNamePattern(construction.Target);
			if(!isProcess) {
				return Regex.IsMatch(block,patternName+@"\s*\.\s*UseShellExecute\s*=");
			}
			return Regex.IsMatch(block,patternName+@"\s*\.\s*StartInfo\s*\.\s*UseShellExecute\s*=")
				|| Regex.IsMatch(block,patternName+@"\s*\.\s*StartInfo\s*=\s*[A-Za-z_]\w*\s*;")
				|| Regex.IsMatch(block,patternName+@"\s*\.\s*StartInfo\s*=\s*new\b[^;]*\bUseShellExecute\s*=");
		}

		///<summary>The index just after a construction: after its argument list and, when there is one, its object initializer.</summary>
		private static int GetConstructionEnd(string code,int indexAfterType) {
			int i=SkipWhiteSpace(code,indexAfterType);
			if(i<code.Length && code[i]=='(') {
				i=SkipWhiteSpace(code,SkipBalanced(code,i,'(',')'));
			}
			if(i<code.Length && code[i]=='{') {
				return SkipBalanced(code,i,'{','}');
			}
			return i;
		}

		private static int SkipWhiteSpace(string code,int index) {
			while(index<code.Length && char.IsWhiteSpace(code[index])) {
				index++;
			}
			return index;
		}

		///<summary>The index after the bracket that closes the one at index (or the end of the code).</summary>
		private static int SkipBalanced(string code,int index,char charOpen,char charClose) {
			int depth=0;
			for(int i=index;i<code.Length;i++) {
				if(code[i]==charOpen) {
					depth++;
				}
				else if(code[i]==charClose) {
					depth--;
					if(depth==0) {
						return i+1;
					}
				}
			}
			return code.Length;
		}

		///<summary>The code from a construction to the first of: the end of the first line at or after its line that starts a process,
		///MAX_BLOCK_LINES lines, the end of the scope it is in (S3a-2f), and the next new object assigned to the same name (S3a-2f).</summary>
		private static string GetBlock(string code,List<int> listLineStarts,Construction construction,int indexAfterConstruction) {
			int indexLine=GetLineIndex(listLineStarts,construction.Index);
			int indexEnd=GetLineStart(code,listLineStarts,indexLine+MAX_BLOCK_LINES+1);
			Match matchStart=_regexStartCall.Match(code,listLineStarts[indexLine]);
			if(matchStart.Success && matchStart.Index<indexEnd) {
				indexEnd=GetLineStart(code,listLineStarts,GetLineIndex(listLineStarts,matchStart.Index)+1);
			}
			int depth=0;
			for(int i=construction.Index;i<indexEnd;i++) {
				if(code[i]=='{') {
					depth++;
				}
				else if(code[i]=='}') {
					depth--;
					if(depth<0) {
						indexEnd=i;//The closing brace of the block the construction is in.
						break;
					}
				}
			}
			Match matchAgain=new Regex(GetNamePattern(construction.Target)+@"\s*=(?![=>])\s*new\b").Match(code,Math.Min(indexAfterConstruction,code.Length));
			if(matchAgain.Success && matchAgain.Index<indexEnd) {
				indexEnd=matchAgain.Index;
			}
			return code.Substring(construction.Index,Math.Max(0,indexEnd-construction.Index));
		}

		///<summary>A pattern for the dotted name, with or without this. in front, not part of a longer name.</summary>
		private static string GetNamePattern(string name) {
			return @"(?<![\w.])(?:this\s*\.\s*)?"+string.Join(@"\s*\.\s*",name.Split('.').Select(x => Regex.Escape(x)))+@"\b";
		}

		private static int GetLineIndex(List<int> listLineStarts,int index) {
			int indexFound=listLineStarts.BinarySearch(index);
			return indexFound>=0 ? indexFound : ~indexFound-1;
		}

		private static int GetLineStart(string code,List<int> listLineStarts,int indexLine) {
			return indexLine<listLineStarts.Count ? listLineStarts[indexLine] : code.Length;
		}

		///<summary>The argument text of a call up to its closing parenthesis (at most the next five lines; line ends as spaces).</summary>
		private static string GetRestOfCall(string code,int index) {
			StringBuilder stringBuilder=new StringBuilder();
			int depth=1;
			int countLineEnds=0;
			for(int i=index;i<code.Length;i++) {
				char c=code[i];
				if(c=='(') {
					depth++;
				}
				else if(c==')') {
					depth--;
					if(depth==0) {
						break;
					}
				}
				else if(c=='\n') {
					countLineEnds++;
					if(countLineEnds>5) {
						break;
					}
					stringBuilder.Append(' ');
					continue;
				}
				stringBuilder.Append(c);
			}
			return stringBuilder.ToString().Trim();
		}

		///<summary>True when the argument is a ProcessStartInfo whose construction the scan checks: one constructed in place, or a name whose last
		///assignment before the call is a new ProcessStartInfo (or a target-typed new() of a name declared as one). A parameter, a loop variable or
		///a start info that a method returns is not (S3a-2f): start those with ODFileUtils.ProcessStart(ProcessStartInfo).</summary>
		private static bool IsStartInfoArgument(string argument,string code,int indexCall) {
			string patternNewStartInfo=@"^new\s+"+PATTERN_QUALIFIER+@"ProcessStartInfo\b";
			if(Regex.IsMatch(argument,patternNewStartInfo)) {
				return true;
			}
			Match match=Regex.Match(argument,@"^(?:this\s*\.\s*)?([A-Za-z_]\w*)$");
			if(!match.Success) {
				return false;
			}
			string name=match.Groups[1].Value;
			Match matchLast=null;
			foreach(Match matchAssignment in new Regex(GetNamePattern(name)+@"\s*=(?![=>])\s*").Matches(code.Substring(0,indexCall))) {
				matchLast=matchAssignment;
			}
			if(matchLast==null) {
				return false;
			}
			string textAssigned=code.Substring(matchLast.Index+matchLast.Length);
			if(Regex.IsMatch(textAssigned,patternNewStartInfo)) {
				return true;
			}
			return Regex.IsMatch(textAssigned,@"^new\s*[({]") && GetDeclaredKind(code,name)=="ProcessStartInfo";
		}

		private static string Shorten(string text) {
			return text.Length>60 ? text.Substring(0,60)+"..." : text;
		}

		///<summary>The line with comments removed and the contents of string and character literals emptied (the quotes stay). Block comments and
		///verbatim strings may span lines: the two flags carry that state from one line to the next. Interpolation holes count as string text.</summary>
		private static string RemoveCommentsAndStrings(string line,ref bool isInBlockComment,ref bool isInVerbatimString) {
			StringBuilder stringBuilder=new StringBuilder();
			int i=0;
			while(i<line.Length) {
				if(isInBlockComment) {
					int indexEnd=line.IndexOf("*/",i,StringComparison.Ordinal);
					if(indexEnd<0) {
						return stringBuilder.ToString();
					}
					isInBlockComment=false;
					i=indexEnd+2;
					continue;
				}
				if(isInVerbatimString) {
					if(line[i]=='"') {
						if(i+1<line.Length && line[i+1]=='"') {
							i+=2;
							continue;
						}
						isInVerbatimString=false;
						stringBuilder.Append('"');
					}
					i++;
					continue;
				}
				char c=line[i];
				if(c=='/' && i+1<line.Length && line[i+1]=='/') {
					return stringBuilder.ToString();
				}
				if(c=='/' && i+1<line.Length && line[i+1]=='*') {
					isInBlockComment=true;
					i+=2;
					continue;
				}
				if(c=='@' && i+1<line.Length && line[i+1]=='"') {
					stringBuilder.Append("@\"");
					isInVerbatimString=true;
					i+=2;
					continue;
				}
				if((c=='$' && i+2<line.Length && line[i+1]=='@' && line[i+2]=='"') || (c=='@' && i+2<line.Length && line[i+1]=='$' && line[i+2]=='"')) {
					stringBuilder.Append("@\"");
					isInVerbatimString=true;
					i+=3;
					continue;
				}
				if(c=='"' || c=='\'') {
					stringBuilder.Append(c);
					i++;
					while(i<line.Length && line[i]!=c) {
						if(line[i]=='\\') {
							i++;
						}
						i++;
					}
					if(i<line.Length) {
						stringBuilder.Append(c);
						i++;
					}
					continue;
				}
				stringBuilder.Append(c);
				i++;
			}
			return stringBuilder.ToString();
		}

		///<summary>The folders of the C# projects in the solution filter that ship or build shipped files, RdlDesign, DocumentationBuilder and
		///CentralManager among them: not tests\, tools\, third-party\, the legacy UnitTests and UnitTestsCore, xCrudGenerator, or this project.</summary>
		private static List<string> GetScannedProjectFolders(string pathRoot) {
			string pathSlnf=Directory.GetFiles(pathRoot,"*.slnf").Single();
			List<string> listFolders=new List<string>();
			foreach(Match match in Regex.Matches(File.ReadAllText(pathSlnf),"\"([^\"]+\\.csproj)\"")) {
				string pathProject=match.Groups[1].Value.Replace("\\\\","\\");
				string firstPart=pathProject.Split('\\')[0];
				if(new[] { "tests","tools","third-party","UnitTests","UnitTestsCore","xCrudGenerator","NoHqTests" }.Contains(firstPart,StringComparer.OrdinalIgnoreCase)) {
					continue;
				}
				listFolders.Add(Path.GetDirectoryName(Path.Combine(pathRoot,pathProject)));
			}
			return listFolders.Distinct().ToList();
		}

		private static bool IsBuildFolder(string folder,string path) {
			string[] arrayParts=path.Substring(folder.Length+1).Split(Path.DirectorySeparatorChar);
			return arrayParts.Any(x => x.Equals("bin",StringComparison.OrdinalIgnoreCase) || x.Equals("obj",StringComparison.OrdinalIgnoreCase));
		}
	}
}
