// S3a-2 (net10 plan): every start of a program, document or URL in shipped code says whether it goes through the shell. ProcessStartInfo's
// UseShellExecute is true by default on .NET Framework and false on .NET, and only the shell opens documents and URLs and asks for consent to a
// program that needs elevation; so no code calls Process.Start(string...) outside CodeBase.ODFileUtils (ShellOpen and ProcessStart set it), and
// every new Process()/new ProcessStartInfo( block sets UseShellExecute explicitly. A source scan of the C# projects of the solution filter
// (shipped, plus RdlDesign, DocumentationBuilder and CentralManager), except the tests, the tools, the third-party sources, UnitTests,
// UnitTestsCore and xCrudGenerator, whose starts redirect output or never ship. It needs no build, database or network, and starts nothing.
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
		///<summary>Sites another package of the plan owns and fixes, reported but not failed until its fix is on main: PrefL's start of the update
		///file copier belongs to S3a-10 (the Known overlaps of the plan). Remove an entry once that package has merged.</summary>
		private static readonly string[] _arrayPathsKnownPending=new[] { @"OpenPmsDental\Logic\PrefL.cs" };
		///<summary>How many lines after a construction the scan looks for UseShellExecute when no start call comes first.</summary>
		private const int MAX_BLOCK_LINES=80;

		private static readonly Regex _regexStaticStart=new Regex(@"(?<![\w.])(?:System\s*\.\s*Diagnostics\s*\.\s*)?Process\s*\.\s*Start\s*\(");
		private static readonly Regex _regexConstruction=new Regex(@"\bnew\s+(?:System\s*\.\s*Diagnostics\s*\.\s*)?(Process|ProcessStartInfo)\s*[({]");
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

		///<summary>Scans lines (not the helper file) and checks the problems found, each as "line kind".</summary>
		private static void AssertProblems(string[] arrayLines,params string[] arrayExpected) {
			ScanResult scanResult=Scan(arrayLines,false);
			CollectionAssert.AreEqual(arrayExpected,scanResult.ListProblems.Select(x => x.Substring(0,x.IndexOf(':'))+" "+GetKind(x)).ToArray(),
				"Found: "+string.Join(" | ",scanResult.ListProblems));
		}

		private static string GetKind(string problem) {
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

		///<summary>Scans one file's lines. isHelper: the file is ODFileUtils.cs, which may call Process.Start with a path.</summary>
		private static ScanResult Scan(string[] arrayLinesRaw,bool isHelper) {
			ScanResult scanResult=new ScanResult();
			string[] arrayLines=new string[arrayLinesRaw.Length];
			bool isInBlockComment=false;
			bool isInVerbatimString=false;
			for(int i=0;i<arrayLinesRaw.Length;i++) {
				arrayLines[i]=RemoveCommentsAndStrings(arrayLinesRaw[i],ref isInBlockComment,ref isInVerbatimString);
			}
			string codeAll=string.Join("\n",arrayLines);
			for(int i=0;i<arrayLines.Length;i++) {
				string line=arrayLines[i];
				if(!isHelper) {
					foreach(Match match in _regexStaticStart.Matches(line)) {
						string argument=GetRestOfCall(arrayLines,i,match.Index+match.Length);
						if(!IsStartInfoArgument(argument,codeAll)) {
							scanResult.ListProblems.Add((i+1)+": Process.Start("+Shorten(argument)+") outside ODFileUtils");
						}
					}
				}
				foreach(Match match in _regexConstruction.Matches(line)) {
					scanResult.CountConstructions++;
					bool isProcess=match.Groups[1].Value=="Process";
					string block=GetBlock(arrayLines,i,match.Index);
					if(_regexUseShellExecute.IsMatch(block)) {
						continue;
					}
					if(isProcess && _regexStartInfoFromVariable.IsMatch(block)) {
						continue;//The StartInfo comes from a variable whose own new ProcessStartInfo( block is checked.
					}
					scanResult.ListProblems.Add((i+1)+": new "+match.Groups[1].Value+"( block without UseShellExecute");
				}
			}
			return scanResult;
		}

		///<summary>The code from a construction to the end of the first line at or after it that starts a process, or MAX_BLOCK_LINES lines.</summary>
		private static string GetBlock(string[] arrayLines,int indexLine,int indexChar) {
			StringBuilder stringBuilder=new StringBuilder();
			for(int i=indexLine;i<arrayLines.Length && i<=indexLine+MAX_BLOCK_LINES;i++) {
				string line=(i==indexLine) ? arrayLines[i].Substring(indexChar) : arrayLines[i];
				stringBuilder.Append(line).Append('\n');
				if(_regexStartCall.IsMatch(line) || (i==indexLine && _regexStartCall.IsMatch(arrayLines[i]))) {
					break;
				}
			}
			return stringBuilder.ToString();
		}

		///<summary>The argument text of a call up to its closing parenthesis (at most the next five lines).</summary>
		private static string GetRestOfCall(string[] arrayLines,int indexLine,int indexChar) {
			StringBuilder stringBuilder=new StringBuilder();
			int depth=1;
			for(int i=indexLine;i<arrayLines.Length && i<=indexLine+5;i++) {
				string line=(i==indexLine) ? arrayLines[i].Substring(indexChar) : arrayLines[i];
				foreach(char c in line) {
					if(c=='(') {
						depth++;
					}
					else if(c==')') {
						depth--;
						if(depth==0) {
							return stringBuilder.ToString().Trim();
						}
					}
					stringBuilder.Append(c);
				}
				stringBuilder.Append(' ');
			}
			return stringBuilder.ToString().Trim();
		}

		///<summary>True when the argument is a ProcessStartInfo: constructed in place, or a variable that the file declares as one.</summary>
		private static bool IsStartInfoArgument(string argument,string codeAll) {
			if(Regex.IsMatch(argument,@"^new\s+(?:System\s*\.\s*Diagnostics\s*\.\s*)?ProcessStartInfo\b")) {
				return true;
			}
			Match match=Regex.Match(argument,@"^[A-Za-z_]\w*$");
			if(!match.Success) {
				return false;
			}
			return Regex.IsMatch(codeAll,@"\bProcessStartInfo\s+"+Regex.Escape(argument)+@"\b");
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
