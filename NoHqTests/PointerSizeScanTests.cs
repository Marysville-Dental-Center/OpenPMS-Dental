// S3a-12 (net10 plan, decision 19): shipped code is pointer-size-safe before the program moves to 64-bit in stage 3b. A source scan of the C#
// projects of the solution filter (shipped, plus RdlDesign, DocumentationBuilder and CentralManager), except the tests, the tools, the third-party
// sources, UnitTests, UnitTestsCore and xCrudGenerator; the first rule also scans the test projects. Comments are not code; string contents count
// only where a rule reads a path or an entry point. Four rules:
// 1. No IntPtr.Size==4 gate (nor !=4, 4==, Assert.AreEqual(4,IntPtr.Size)): code and tests choose by bitness instead.
// 2. No ToInt32() in the arguments of a call to a P/Invoke (extern) method, and none on a handle-named value: it throws OverflowException in a
//    64-bit process when the value does not fit an int.
// 3. Window longs: the 32-bit entry points (SetWindowLong, GetWindowLong, SetClassLong, GetClassLong) carry only style bits (GWL_STYLE,
//    GWL_EXSTYLE), or are the 32-bit half of a pair that chooses SetWindowLongPtr by bitness; a method named ...Ptr never maps to a 32-bit
//    entry point, and a ...Ptr entry point (which 32-bit user32 does not export) is imported only where the code chooses by bitness.
// 4. Registry views: every Registry.LocalMachine use opens a key that both views share or that is not redirected (the services key, the
//    installer's dependencies); a vendor key is opened with RegistryKey.OpenBaseKey and a named view (Registry32 or Registry64, never
//    Default); no code builds a WOW6432Node path or reads HKEY_LOCAL_MACHINE through Registry.GetValue/SetValue.
// It needs no build, database or network, and starts nothing.
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
	public class PointerSizeScanTests {
		///<summary>HKLM keys that a 32-bit and a 64-bit process see alike, so Registry.LocalMachine is right in either bitness: the services key
		///(HKLM\SYSTEM is not redirected by WOW64) and the installer's dependency records (HKLM\SOFTWARE\Classes is shared since Windows 7, apart
		///from CLSID, DirectShow, Interface, Media Type and MediaFoundation). Compared case-insensitively with the start of the path.</summary>
		private static readonly string[] _arrayKeysShared=new[] { @"System\CurrentControlSet\Services",@"SOFTWARE\Classes\Installer\Dependencies" };
		///<summary>(file, the literal it opens, why it may read the process's own view). Checked and left, recorded in docs/port-notes/S3a-12.md.</summary>
		private static readonly List<Tuple<string,string,string>> _listRegistryAllowed=new List<Tuple<string,string,string>> {
			Tuple.Create(@"RdlDesign\DesignerUtility.cs","Software","RdlDesign lists the ODBC data sources of its own bitness, the ones it can open (unshipped designer)."),
		};
		private static readonly string[] _arrayWindowLong32=new[] { "SetWindowLong","GetWindowLong","SetClassLong","GetClassLong" };
		private static readonly string[] _arrayWindowLongPtr=new[] { "SetWindowLongPtr","GetWindowLongPtr","SetClassLongPtr","GetClassLongPtr" };

		private static readonly Regex _regexSizeGate=new Regex(@"\bIntPtr\s*\.\s*Size\s*[!=]=\s*4\b|\b4\s*[!=]=\s*IntPtr\s*\.\s*Size\b"
			+@"|\bAssert\s*\.\s*AreEqual\s*\(\s*4\s*,\s*IntPtr\s*\.\s*Size\b");
		private static readonly Regex _regexExtern=new Regex(@"\bextern\s+[\w.]+(?:\s*<[^>()]*>)?(?:\s*\[\s*\])?\s+(\w+)\s*\(");
		private static readonly Regex _regexToInt32=new Regex(@"\.\s*ToInt32\s*\(\s*\)");
		private static readonly Regex _regexHandleToInt32=new Regex(@"\b(?:\w*(?:Handle|Hwnd|HWnd)|hwnd\w*|hWnd\w*|h[A-Z]\w*)\s*\.\s*ToInt32\s*\(\s*\)");
		private static readonly Regex _regexBitnessBranch=new Regex(@"\bIntPtr\s*\.\s*Size\s*(?:==\s*8|>\s*4)|\bEnvironment\s*\.\s*Is64BitProcess\b");
		private static readonly Regex _regexDllImport=new Regex(@"\bDllImport\s*\(");
		private static readonly Regex _regexEntryPoint=new Regex(@"\bEntryPoint\s*=\s*""([^""]*)""");
		private static readonly Regex _regexLocalMachine=new Regex(@"\bRegistry\s*\.\s*LocalMachine\b");
		private static readonly Regex _regexBaseKeyDefault=new Regex(@"\bOpenBaseKey\s*\(\s*(?:Microsoft\s*\.\s*Win32\s*\.\s*)?RegistryHive\s*\.\s*LocalMachine\s*,\s*"
			+@"(?:Microsoft\s*\.\s*Win32\s*\.\s*)?RegistryView\s*\.\s*Default\b");
		private static readonly Regex _regexRegistryGetSetValue=new Regex(@"\bRegistry\s*\.\s*(?:GetValue|SetValue)\s*\(\s*@?""HKEY_LOCAL_MACHINE",RegexOptions.IgnoreCase);
		private static readonly Regex _regexLiteral=new Regex(@"@""(?:[^""]|"""")*""|""(?:\\.|[^""\\])*""");

		[TestMethod]
		public void ShippedSource_IsPointerSizeSafe() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			List<string> listFolders=GetScannedProjectFolders(pathRoot,false);
			List<string> listNames=listFolders.Select(x => Path.GetFileName(x)).ToList();
			foreach(string name in new[] { "OpenPmsDentalBusiness","OpenPmsDental","WpfControlsOD","CodeBase","ImagingDeviceManager","ImagingCapture",
				"DataConnectionBase","RdlDesign","CentralManager" })
			{
				Assert.IsTrue(listNames.Contains(name),"The scan must include "+name+".");
			}
			foreach(string name in new[] { "xCrudGenerator","UnitTests","UnitTestsCore","NoHqTests" }) {
				Assert.IsFalse(listNames.Contains(name),"The scan leaves out "+name+".");
			}
			List<SourceFile> listFiles=ReadFiles(pathRoot,listFolders);
			Assert.IsTrue(listFiles.Count>1000,"Expected the shipped source files, found "+listFiles.Count+".");
			HashSet<string> hashSetExterns=GetExternNames(listFiles);
			Assert.IsTrue(hashSetExterns.Count>=100,"Expected the P/Invoke declarations of shipped code, found "+hashSetExterns.Count+".");
			List<string> listProblems=new List<string>();
			int countLocalMachine=0;
			foreach(SourceFile sourceFile in listFiles) {
				listProblems.AddRange(FindSizeGates(sourceFile));
				listProblems.AddRange(FindToInt32(sourceFile,hashSetExterns));
				listProblems.AddRange(FindWindowLongs(sourceFile));
				listProblems.AddRange(FindRegistryViews(sourceFile,ref countLocalMachine));
			}
			Assert.IsTrue(countLocalMachine>=6,"Expected the Registry.LocalMachine reads of shipped code, found "+countLocalMachine+".");
			Assert.AreEqual(0,listProblems.Count,"Not pointer-size-safe for the 64-bit program (S3a-12, docs/port-notes/S3a-12.md):\r\n"
				+string.Join("\r\n",listProblems));
		}

		[TestMethod]
		public void TestSource_HasNoSizeGate() {
			//The tests run in both bitnesses too (the lane's x64 test build): expected layouts come from an x86 and an x64 column.
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			List<string> listFolders=GetScannedProjectFolders(pathRoot,true);
			Assert.IsTrue(listFolders.Any(x => Path.GetFileName(x)=="ImagingDeviceManagerTests"),"The test scan must include ImagingDeviceManagerTests.");
			Assert.IsTrue(listFolders.Any(x => Path.GetFileName(x)=="ImagingCaptureTests"),"The test scan must include ImagingCaptureTests.");
			List<string> listProblems=new List<string>();
			foreach(SourceFile sourceFile in ReadFiles(pathRoot,listFolders)) {
				listProblems.AddRange(FindSizeGates(sourceFile));
			}
			Assert.AreEqual(0,listProblems.Count,"Tests that demand a 32-bit process: "+string.Join(", ",listProblems));
		}

		#region Self-test of the scanner
		[TestMethod]
		public void Scan_SizeGate_IsFound() {
			AssertProblems(new[] { "if(IntPtr.Size!=4) {","\treturn null;","}" },"1 IntPtr.Size==4");
			AssertProblems(new[] { "Assert.AreEqual(4,IntPtr.Size);" },"1 IntPtr.Size==4");
			AssertProblems(new[] { "bool is32=4 == IntPtr.Size;" },"1 IntPtr.Size==4");
			AssertProblems(new[] { "if(IntPtr.Size==8) {","\tx=8;","}","int size=IntPtr.Size;","IntPtr p=Marshal.AllocHGlobal(IntPtr.Size);",
				"//if(IntPtr.Size==4)","string s=\"IntPtr.Size==4\";" });
		}

		[TestMethod]
		public void Scan_ToInt32InAPInvokeCall_IsFound() {
			string[] arrayLines=new[] {
				"[DllImport(\"user32.dll\")]",
				"private static extern int SetWindowLong(IntPtr hWnd,int nIndex,int dwNewLong);",
				"private void Attach(IntPtr hwndRoot) {",
				"\tSetWindowLong(Handle,GWL_STYLE,",
				"\t\towner.ToInt32());",
				"}",
			};
			AssertProblems(arrayLines,"5 ToInt32");
			AssertProblems(new[] { "[DllImport(\"user32.dll\")]","static extern bool SetForegroundWindow(int hWnd);","void F() { SetForegroundWindow(process.MainWindowHandle.ToInt32()); }" },
				"3 ToInt32","3 ToInt32 on a handle");
			AssertProblems(new[] { "int value=hwndRoot.ToInt32();" },"1 ToInt32 on a handle");
			AssertProblems(new[] { "int value=this.Handle.ToInt32();" },"1 ToInt32 on a handle");
			//Not a P/Invoke call, not a handle, a comment, or ToInt64.
			AssertProblems(new[] { "int offset=Marshal.OffsetOf(typeof(MSG),\"message\").ToInt32();","int delta=m.WParam.ToInt32() >> 16;",
				"//SetWindowLong(h,-8,hwnd.ToInt32());","long value=hwndRoot.ToInt64();","int x=Convert.ToInt32(text);" });
		}

		[TestMethod]
		public void Scan_WindowLongForAPointer_IsFound() {
			//A ...Ptr name mapped to the 32-bit entry point (the commented-out declaration FormODBase once had).
			AssertProblems(new[] { "[DllImport(\"user32.dll\", EntryPoint=\"GetWindowLong\")]","private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);" },
				"2 window long");
			//The 32-bit entry point with a pointer-sized value and no choice by bitness.
			AssertProblems(new[] { "[DllImport(\"user32.dll\")]","static extern IntPtr SetWindowLong(IntPtr hWnd,int nIndex,IntPtr dwNewLong);" },"2 window long");
			//The 64-bit entry point alone: 32-bit user32 does not export it.
			AssertProblems(new[] { "[DllImport(\"user32.dll\")]","static extern IntPtr SetWindowLongPtr(IntPtr hWnd,int nIndex,IntPtr dwNewLong);" },"2 window long");
			//The int form with an index that is not style bits.
			AssertProblems(new[] { "[DllImport(\"user32.dll\",EntryPoint=\"SetWindowLongW\")]","static extern int SetOwner(IntPtr hWnd,int nIndex,int dwNewLong);",
				"void F() { SetOwner(h,GWL_HWNDPARENT,0); }" },"3 window long");
			//Good: style bits on the 32-bit entry points.
			AssertProblems(new[] { "[DllImport(\"user32.dll\",EntryPoint=\"GetWindowLong\")]","private static extern int User32_GetWindowLong(IntPtr hWnd, GetWindowLong nIndex);",
				"[DllImport(\"user32.dll\",EntryPoint = \"SetWindowLong\")]","private static extern int User32_SetWindowLong(IntPtr hWnd,GetWindowLong nIndex,int dwNewLong);",
				"void F() {","\tint wl=User32_GetWindowLong(this.Handle, GetWindowLong.GWL_EXSTYLE);","\tUser32_SetWindowLong(this.Handle,GetWindowLong.GWL_EXSTYLE,wl | 0x80000);","}" });
			//Good: the pair that chooses by bitness (NativeMethods.SetWindowLongPtr).
			AssertProblems(new[] {
				"public static IntPtr SetWindowLongPtr(IntPtr hWnd,int nIndex,IntPtr dwNewLong) {",
				"\tif(IntPtr.Size==8) {","\t\treturn SetWindowLongPtr64(hWnd,nIndex,dwNewLong);","\t}","\treturn SetWindowLong32(hWnd,nIndex,dwNewLong);","}",
				"[DllImport(\"user32.dll\",EntryPoint=\"SetWindowLong\",SetLastError=true)]",
				"private static extern IntPtr SetWindowLong32(IntPtr hWnd,int nIndex,IntPtr dwNewLong);",
				"[DllImport(\"user32.dll\",EntryPoint=\"SetWindowLongPtr\",SetLastError=true)]",
				"private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd,int nIndex,IntPtr dwNewLong);" });
		}

		[TestMethod]
		public void Scan_RegistryLocalMachineVendorKey_IsFound() {
			AssertProblems(new[] { "RegistryKey regKey=Registry.LocalMachine.OpenSubKey(\"Software\\\\TROJAN BENEFIT SERVICE\");" },"1 registry");
			AssertProblems(new[] { "RegistryKey hklm=Registry.LocalMachine;","hklm=hklm.OpenSubKey(@\"SOFTWARE\\Vendor\");" },"1 registry");
			AssertProblems(new[] { "using(RegistryKey k=RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,RegistryView.Default)) {}" },"1 registry");
			AssertProblems(new[] { "string path=@\"SOFTWARE\\WOW6432Node\\Vendor\";" },"1 registry");
			AssertProblems(new[] { "object value=Registry.GetValue(@\"HKEY_LOCAL_MACHINE\\SOFTWARE\\Vendor\",\"Path\",null);" },"1 registry");
			//Good: a named view, the shared keys (also through a variable or Path.Combine), and HKCU.
			AssertProblems(new[] {
				"using(RegistryKey k=RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,RegistryView.Registry32)) {}",
				"RegistryKey hklm=Registry.LocalMachine;",
				"hklm=hklm.OpenSubKey(Path.Combine(@\"System\\CurrentControlSet\\Services\\\",service.ServiceName));",
				"string dependenciesPath=@\"SOFTWARE\\Classes\\Installer\\Dependencies\";",
				"using RegistryKey registryKeyDependencies=Registry.LocalMachine.OpenSubKey(dependenciesPath);",
				"RegistryKey keyUser=Registry.CurrentUser.OpenSubKey(@\"Software\\Vendor\");",
				"//Registry.LocalMachine.OpenSubKey(\"Software\\\\Old\");" });
		}

		[TestMethod]
		public void RemoveComments_KeepsCodeAndOptionallyStrings() {
			string[] arrayLines=new[] { "a(\"x//y\");//z","b(@\"p\\q\"\"r\");/*c","d*/e('\"');" };
			CollectionAssert.AreEqual(new[] { "a(\"\");","b(@\"\");","e('');" },RemoveComments(arrayLines,false));
			CollectionAssert.AreEqual(new[] { "a(\"x//y\");","b(@\"p\\q\"\"r\");","e('\"');" },RemoveComments(arrayLines,true));
		}

		private static void AssertProblems(string[] arrayLines,params string[] arrayExpected) {
			SourceFile sourceFile=new SourceFile(@"Test\Planted.cs",arrayLines);
			HashSet<string> hashSetExterns=GetExternNames(new List<SourceFile> { sourceFile });
			int countLocalMachine=0;
			List<string> listProblems=new List<string>();
			listProblems.AddRange(FindSizeGates(sourceFile));
			listProblems.AddRange(FindToInt32(sourceFile,hashSetExterns));
			listProblems.AddRange(FindWindowLongs(sourceFile));
			listProblems.AddRange(FindRegistryViews(sourceFile,ref countLocalMachine));
			List<string> listFound=listProblems.Select(x => x.Substring(x.IndexOf(':')+1)).Select(x => x.Substring(0,FirstWords(x))).ToList();
			CollectionAssert.AreEquivalent(arrayExpected,listFound,"Found: "+string.Join(" | ",listProblems));
		}

		///<summary>The line number and the kind of a problem, for example "4 ToInt32" or "3 ToInt32 on a handle", from "path:4 ToInt32 in ...".</summary>
		private static int FirstWords(string problem) {
			foreach(string kind in new[] { "IntPtr.Size==4","ToInt32 on a handle","ToInt32","window long","registry" }) {
				int index=problem.IndexOf(' ');
				if(problem.Substring(index+1).StartsWith(kind)) {
					return index+1+kind.Length;
				}
			}
			return problem.Length;
		}
		#endregion Self-test of the scanner

		#region Rules
		///<summary>Rule 1.</summary>
		private static List<string> FindSizeGates(SourceFile sourceFile) {
			List<string> listProblems=new List<string>();
			for(int i=0;i<sourceFile.ArrayCode.Length;i++) {
				if(_regexSizeGate.IsMatch(sourceFile.ArrayCode[i])) {
					listProblems.Add(sourceFile.PathRelative+":"+(i+1)+" IntPtr.Size==4 gate: choose by IntPtr.Size or Environment.Is64BitProcess");
				}
			}
			return listProblems;
		}

		///<summary>Rule 2.</summary>
		private static List<string> FindToInt32(SourceFile sourceFile,HashSet<string> hashSetExterns) {
			List<string> listProblems=new List<string>();
			string code=sourceFile.Code;
			foreach(Match match in _regexHandleToInt32.Matches(code)) {
				listProblems.Add(sourceFile.PathRelative+":"+sourceFile.GetLine(match.Index)+" ToInt32 on a handle ("+match.Value+"): pass the IntPtr, or ToInt64");
			}
			foreach(Match match in Regex.Matches(code,@"(?<![\w.])(?:[A-Za-z_]\w*\s*\.\s*)*([A-Za-z_]\w*)\s*\(")) {
				string name=match.Groups[1].Value;
				if(!hashSetExterns.Contains(name) || IsDeclaration(code,match.Groups[1].Index)) {
					continue;
				}
				int indexOpen=match.Index+match.Length-1;
				string arguments=GetBalanced(code,indexOpen);
				foreach(Match matchToInt32 in _regexToInt32.Matches(arguments)) {
					listProblems.Add(sourceFile.PathRelative+":"+sourceFile.GetLine(indexOpen+1+matchToInt32.Index)+" ToInt32 in the arguments of "+name
						+"(), a P/Invoke: pass the IntPtr");
				}
			}
			return listProblems;
		}

		///<summary>Rule 3.</summary>
		private static List<string> FindWindowLongs(SourceFile sourceFile) {
			List<string> listProblems=new List<string>();
			bool hasBitnessBranch=_regexBitnessBranch.IsMatch(sourceFile.Code);
			List<Declaration> listDeclarations=GetDeclarations(sourceFile);
			bool hasPtrImport=listDeclarations.Any(x => _arrayWindowLongPtr.Contains(x.EntryPointBase));
			foreach(Declaration declaration in listDeclarations) {
				string where=sourceFile.PathRelative+":"+declaration.Line+" window long ";
				if(_arrayWindowLongPtr.Contains(declaration.EntryPointBase)) {
					if(!hasBitnessBranch) {
						listProblems.Add(where+declaration.EntryPoint+" imported where nothing chooses by bitness: 32-bit user32 does not export it");
					}
					continue;
				}
				if(!_arrayWindowLong32.Contains(declaration.EntryPointBase)) {
					continue;
				}
				if(declaration.Name.Contains("Ptr")) {
					listProblems.Add(where+declaration.Name+" maps to the 32-bit entry point "+declaration.EntryPoint);
					continue;
				}
				if(declaration.IsPointerForm) {
					if(!hasPtrImport || !hasBitnessBranch) {
						listProblems.Add(where+declaration.Name+" passes a pointer-sized value to the 32-bit "+declaration.EntryPoint+" without the 64-bit "
							+declaration.EntryPointBase+"Ptr chosen by bitness");
					}
					continue;
				}
				foreach(Match match in Regex.Matches(sourceFile.Code,@"(?<![\w])"+Regex.Escape(declaration.Name)+@"\s*\(")) {
					if(IsDeclaration(sourceFile.Code,match.Index)) {
						continue;
					}
					List<string> listArguments=SplitArguments(GetBalanced(sourceFile.Code,match.Index+match.Length-1));
					if(listArguments.Count<2 || !Regex.IsMatch(listArguments[1],@"\bGWL_(?:EX)?STYLE\b|^\s*-\s*(?:16|20)\s*$")) {
						listProblems.Add(sourceFile.PathRelative+":"+sourceFile.GetLine(match.Index)+" window long "+declaration.Name+"() with an index that is "
							+"not GWL_STYLE or GWL_EXSTYLE: a pointer-sized value needs SetWindowLongPtr in a 64-bit process");
					}
				}
			}
			return listProblems;
		}

		///<summary>Rule 4.</summary>
		private static List<string> FindRegistryViews(SourceFile sourceFile,ref int countLocalMachine) {
			List<string> listProblems=new List<string>();
			string text=sourceFile.CodeWithStrings;
			foreach(Match match in _regexBaseKeyDefault.Matches(text)) {
				listProblems.Add(sourceFile.PathRelative+":"+sourceFile.GetLineWithStrings(match.Index)+" registry view Default is the process's own: name Registry32 or Registry64");
			}
			foreach(Match match in _regexRegistryGetSetValue.Matches(text)) {
				listProblems.Add(sourceFile.PathRelative+":"+sourceFile.GetLineWithStrings(match.Index)+" registry HKEY_LOCAL_MACHINE through Registry.GetValue/SetValue reads "
					+"the process's own view: use RegistryKey.OpenBaseKey with a named view");
			}
			foreach(Match match in _regexLiteral.Matches(text)) {
				if(match.Value.IndexOf("WOW6432Node",StringComparison.OrdinalIgnoreCase)>=0) {
					listProblems.Add(sourceFile.PathRelative+":"+sourceFile.GetLineWithStrings(match.Index)+" registry path with WOW6432Node: open the 32-bit view instead");
				}
			}
			foreach(Match match in _regexLocalMachine.Matches(text)) {
				countLocalMachine++;
				//The statement that uses it, or when that opens nothing, the next one too: hklm=Registry.LocalMachine; hklm=hklm.OpenSubKey(...);
				int indexEnd=text.IndexOf(';',match.Index);
				indexEnd=indexEnd<0 ? text.Length : indexEnd;
				string window=text.Substring(match.Index,indexEnd-match.Index);
				if(!window.Contains("OpenSubKey") && indexEnd<text.Length) {
					int indexEndNext=text.IndexOf(';',indexEnd+1);
					window=text.Substring(match.Index,(indexEndNext<0 ? text.Length : indexEndNext)-match.Index);
				}
				List<string> listPaths=GetOpenedPaths(window,text);
				string pathRelative=sourceFile.PathRelative;
				bool isAllowed=listPaths.Count>0 && listPaths.All(path =>
					_arrayKeysShared.Any(x => path.StartsWith(x,StringComparison.OrdinalIgnoreCase))
					|| _listRegistryAllowed.Any(x => x.Item1.Equals(pathRelative,StringComparison.OrdinalIgnoreCase) && x.Item2==path));
				if(!isAllowed) {
					listProblems.Add(sourceFile.PathRelative+":"+sourceFile.GetLineWithStrings(match.Index)+" registry Registry.LocalMachine opens "
						+(listPaths.Count==0 ? "a key the scan cannot read" : string.Join(", ",listPaths))+": a vendor key names its view "
						+"(RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,RegistryView.Registry32))");
				}
			}
			return listProblems;
		}
		#endregion Rules

		#region Helpers
		private class SourceFile {
			public string PathRelative;
			///<summary>Per line, comments removed and string and character literals emptied (the quotes stay).</summary>
			public string[] ArrayCode;
			///<summary>ArrayCode joined with \n.</summary>
			public string Code;
			///<summary>Comments removed, literals kept, joined with \n.</summary>
			public string CodeWithStrings;
			private readonly int[] _arrayLineStarts;
			private readonly int[] _arrayLineStartsWithStrings;

			public SourceFile(string pathRelative,string[] arrayLines) {
				PathRelative=pathRelative;
				ArrayCode=RemoveComments(arrayLines,false);
				Code=string.Join("\n",ArrayCode);
				CodeWithStrings=string.Join("\n",RemoveComments(arrayLines,true));
				_arrayLineStarts=GetLineStarts(Code);
				_arrayLineStartsWithStrings=GetLineStarts(CodeWithStrings);
			}

			public int GetLine(int index) {
				return FindLine(_arrayLineStarts,index);
			}

			public int GetLineWithStrings(int index) {
				return FindLine(_arrayLineStartsWithStrings,index);
			}

			private static int[] GetLineStarts(string text) {
				List<int> listStarts=new List<int> { 0 };
				for(int i=0;i<text.Length;i++) {
					if(text[i]=='\n') {
						listStarts.Add(i+1);
					}
				}
				return listStarts.ToArray();
			}

			private static int FindLine(int[] arrayStarts,int index) {
				int line=Array.BinarySearch(arrayStarts,index);
				return (line>=0 ? line : ~line-1)+1;
			}
		}

		private class Declaration {
			public string Name;
			public string EntryPoint;
			///<summary>EntryPoint without a trailing A or W.</summary>
			public string EntryPointBase;
			///<summary>Returns IntPtr or takes an IntPtr after the window handle: the pointer-sized form.</summary>
			public bool IsPointerForm;
			public int Line;
		}

		///<summary>The DllImport declarations of the file: each extern method with the entry point its DllImport names (or its own name).</summary>
		private static List<Declaration> GetDeclarations(SourceFile sourceFile) {
			List<Declaration> listDeclarations=new List<Declaration>();
			string text=sourceFile.CodeWithStrings;
			foreach(Match match in _regexExtern.Matches(text)) {
				string name=match.Groups[1].Value;
				int indexAttribute=-1;
				foreach(Match matchImport in _regexDllImport.Matches(text.Substring(0,match.Index))) {
					indexAttribute=matchImport.Index;
				}
				string entryPoint=name;
				if(indexAttribute>=0 && match.Index-indexAttribute<800) {
					Match matchEntry=_regexEntryPoint.Match(text.Substring(indexAttribute,match.Index-indexAttribute));
					if(matchEntry.Success) {
						entryPoint=matchEntry.Groups[1].Value;
					}
				}
				string entryPointBase=Regex.Replace(entryPoint,"(?<=Long(?:Ptr)?)[AW]$","");
				string signature=text.Substring(match.Index,GetBalancedEnd(text,match.Index+match.Length-1)-match.Index);
				string returnType=Regex.Match(signature,@"\bextern\s+([\w.]+)").Groups[1].Value;
				List<string> listParameters=SplitArguments(GetBalanced(text,match.Index+match.Length-1));
				bool isPointerForm=returnType=="IntPtr" || returnType=="UIntPtr" || listParameters.Skip(1).Any(x => Regex.IsMatch(x,@"\b(?:IntPtr|UIntPtr)\b"));
				listDeclarations.Add(new Declaration { Name=name,EntryPoint=entryPoint,EntryPointBase=entryPointBase,IsPointerForm=isPointerForm,
					Line=sourceFile.GetLineWithStrings(match.Groups[1].Index) });
			}
			return listDeclarations;
		}

		private static HashSet<string> GetExternNames(List<SourceFile> listFiles) {
			HashSet<string> hashSetNames=new HashSet<string>();
			foreach(SourceFile sourceFile in listFiles) {
				foreach(Match match in _regexExtern.Matches(sourceFile.Code)) {
					hashSetNames.Add(match.Groups[1].Value);
				}
			}
			return hashSetNames;
		}

		///<summary>True when the name at index is the one an extern declaration (or a method definition) declares, not a call.</summary>
		private static bool IsDeclaration(string code,int index) {
			int indexLineStart=code.LastIndexOf('\n',Math.Max(0,index-1))+1;
			string before=code.Substring(indexLineStart,index-indexLineStart);
			return Regex.IsMatch(before,@"\bextern\b|\b(?:public|private|internal|protected|static)\b[^=(]*\s$");
		}

		///<summary>The text between the parenthesis at indexOpen and its partner (the whole rest when there is none).</summary>
		private static string GetBalanced(string text,int indexOpen) {
			int indexEnd=GetBalancedEnd(text,indexOpen);
			return text.Substring(indexOpen+1,Math.Max(0,indexEnd-indexOpen-2));
		}

		///<summary>The index after the parenthesis that closes the one at indexOpen.</summary>
		private static int GetBalancedEnd(string text,int indexOpen) {
			int depth=0;
			for(int i=indexOpen;i<text.Length;i++) {
				if(text[i]=='(') {
					depth++;
				}
				else if(text[i]==')') {
					depth--;
					if(depth==0) {
						return i+1;
					}
				}
			}
			return text.Length;
		}

		///<summary>The arguments of an argument list, split at the commas outside parentheses, brackets and braces.</summary>
		private static List<string> SplitArguments(string arguments) {
			List<string> listArguments=new List<string>();
			int depth=0;
			int indexStart=0;
			for(int i=0;i<arguments.Length;i++) {
				char c=arguments[i];
				if(c=='(' || c=='[' || c=='{' || c=='<') {
					depth++;
				}
				else if(c==')' || c==']' || c=='}' || c=='>') {
					depth--;
				}
				else if(c==',' && depth==0) {
					listArguments.Add(arguments.Substring(indexStart,i-indexStart));
					indexStart=i+1;
				}
			}
			if(arguments.Trim().Length>0) {
				listArguments.Add(arguments.Substring(indexStart));
			}
			return listArguments;
		}

		///<summary>The registry paths the OpenSubKey calls of a statement window open: the start of each call's first argument, a literal
		///(@"System\...\Services\"+name), the literal the file assigns to an identifier (dependenciesPath+"\\"+name), or the first argument of a
		///Path.Combine. An empty string for a start the scan cannot read.</summary>
		private static List<string> GetOpenedPaths(string window,string text) {
			List<string> listPaths=new List<string>();
			foreach(Match match in Regex.Matches(window,@"\bOpenSubKey\s*\(")) {
				List<string> listArguments=SplitArguments(GetBalanced(window,match.Index+match.Length-1));
				listPaths.Add(listArguments.Count==0 ? "" : GetLeadingPath(listArguments[0],text));
			}
			return listPaths;
		}

		private static string GetLeadingPath(string argument,string text) {
			argument=argument.Trim();
			Match matchCombine=Regex.Match(argument,@"^(?:System\s*\.\s*IO\s*\.\s*)?Path\s*\.\s*Combine\s*\(");
			if(matchCombine.Success) {
				List<string> listArguments=SplitArguments(GetBalanced(argument,matchCombine.Length-1));
				return listArguments.Count==0 ? "" : GetLeadingPath(listArguments[0],text);
			}
			Match matchLiteral=_regexLiteral.Match(argument);
			if(matchLiteral.Success && matchLiteral.Index==0) {
				return Unescape(matchLiteral.Value);
			}
			Match matchIdentifier=Regex.Match(argument,@"^[A-Za-z_]\w*\b");
			if(matchIdentifier.Success) {
				Match matchAssigned=Regex.Match(text,@"\b"+Regex.Escape(matchIdentifier.Value)+@"\s*=\s*(@""(?:[^""]|"""")*""|""(?:\\.|[^""\\])*"")\s*;");
				if(matchAssigned.Success) {
					return Unescape(matchAssigned.Groups[1].Value);
				}
			}
			return "";
		}

		private static string Unescape(string literal) {
			if(literal.StartsWith("@")) {
				return literal.Substring(2,literal.Length-3).Replace("\"\"","\"");
			}
			return Regex.Unescape(literal.Substring(1,literal.Length-2));
		}

		private static List<SourceFile> ReadFiles(string pathRoot,List<string> listFolders) {
			List<SourceFile> listFiles=new List<SourceFile>();
			foreach(string folder in listFolders) {
				foreach(string path in Directory.EnumerateFiles(folder,"*.cs",SearchOption.AllDirectories)) {
					if(IsBuildFolder(folder,path)) {
						continue;
					}
					listFiles.Add(new SourceFile(path.Substring(pathRoot.Length+1),File.ReadAllLines(path)));
				}
			}
			return listFiles;
		}

		///<summary>Each line with comments removed, and with the contents of string and character literals emptied (the quotes stay) unless
		///isKeepingStrings. Block comments and verbatim strings may span lines: the state carries from one line to the next. Interpolation holes
		///count as string text. The same remover as ShellStartScanTests (S3a-2), with literals optionally kept.</summary>
		private static string[] RemoveComments(string[] arrayLines,bool isKeepingStrings) {
			string[] arrayResult=new string[arrayLines.Length];
			bool isInBlockComment=false;
			bool isInVerbatimString=false;
			for(int index=0;index<arrayLines.Length;index++) {
				string line=arrayLines[index];
				StringBuilder stringBuilder=new StringBuilder();
				int i=0;
				while(i<line.Length) {
					if(isInBlockComment) {
						int indexEnd=line.IndexOf("*/",i,StringComparison.Ordinal);
						if(indexEnd<0) {
							i=line.Length;
							continue;
						}
						isInBlockComment=false;
						i=indexEnd+2;
						continue;
					}
					if(isInVerbatimString) {
						if(line[i]=='"') {
							if(i+1<line.Length && line[i+1]=='"') {
								if(isKeepingStrings) {
									stringBuilder.Append("\"\"");
								}
								i+=2;
								continue;
							}
							isInVerbatimString=false;
							stringBuilder.Append('"');
						}
						else if(isKeepingStrings) {
							stringBuilder.Append(line[i]);
						}
						i++;
						continue;
					}
					char c=line[i];
					if(c=='/' && i+1<line.Length && line[i+1]=='/') {
						break;
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
								if(isKeepingStrings && i+1<line.Length) {
									stringBuilder.Append(line[i]).Append(line[i+1]);
								}
								i+=2;
								continue;
							}
							if(isKeepingStrings) {
								stringBuilder.Append(line[i]);
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
				arrayResult[index]=stringBuilder.ToString();
			}
			return arrayResult;
		}

		///<summary>The folders of the C# projects in the solution filter: the shipped ones, RdlDesign, DocumentationBuilder and CentralManager among
		///them (not tests\, tools\, third-party\, the legacy UnitTests and UnitTestsCore, xCrudGenerator, or this project); or, with isTests, the
		///test projects (tests\ and this project).</summary>
		private static List<string> GetScannedProjectFolders(string pathRoot,bool isTests) {
			string pathSlnf=Directory.GetFiles(pathRoot,"*.slnf").Single();
			List<string> listFolders=new List<string>();
			foreach(Match match in Regex.Matches(File.ReadAllText(pathSlnf),"\"([^\"]+\\.csproj)\"")) {
				string pathProject=match.Groups[1].Value.Replace("\\\\","\\");
				string firstPart=pathProject.Split('\\')[0];
				bool isTestProject=new[] { "tests","NoHqTests" }.Contains(firstPart,StringComparer.OrdinalIgnoreCase);
				if(isTests!=isTestProject) {
					continue;
				}
				if(!isTests && new[] { "tools","third-party","UnitTests","UnitTestsCore","xCrudGenerator" }.Contains(firstPart,StringComparer.OrdinalIgnoreCase)) {
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
		#endregion Helpers
	}
}
