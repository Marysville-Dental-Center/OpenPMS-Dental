// S3a-13 (net10 plan): QuickBooks Desktop's 32-bit QBFC runs only in the 32-bit helper program. No project but the helper (and its tests,
// which compile the helper's QuickBooks module against the interop and a fake) references Interop.QBFC10, no other source file uses its types,
// and the helper references no other project of this repository (the protocol file is linked into CodeBase, not the other way round), so it
// can have its own runtime after the switch (O-15). A scan of the repository's project and source files; it needs no build and starts nothing.
// S3a-14 added Schick CDR's interop (referenced by the helper and its tests only), SigPlusNET.dll and MedVisorInterface.dll (loaded by the
// helper only).
// S3a-14n (native first, net10-plan decision 23) changed the rule: each vendor piece is loaded through one chooser (CodeBase's
// NativeFirstChooser) and its two paths only, the program's in-process file and the helper's module. WpfControlsOD references the QBFC and
// CDR interops again (embedded) for its two in-process files; nothing else does, and each in-process file reaches the vendor piece only
// through its chooser.
// N64-A3 added Mediadent / CADI: its ProgID is created only in its in-process file and the helper's Cadi module, after its chooser.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NoHqTests {
	[TestClass]
	public class Helper32VendorScanTests {
		///<summary>The projects that may reference the 32-bit-only vendor pieces' interop: the helper and its tests (the fallback path) and, since
		///S3a-14n, WpfControlsOD (the in-process path, types embedded).</summary>
		private static readonly string[] _arrayProjectsAllowed=new[] { @"OpenPmsDentalHelper32\OpenPmsDentalHelper32.csproj",
			@"tests\Helper32Tests\Helper32Tests.csproj",@"WpfControlsOD\WpfControlsOD.csproj" };
		///<summary>The folders whose source may use the vendor pieces' types: the helper's and its tests'.</summary>
		private static readonly string[] _arrayFoldersAllowed=new[] { @"OpenPmsDentalHelper32\",@"tests\Helper32Tests\" };
		///<summary>The program's in-process files of S3a-14n, one per vendor piece: the only files outside the helper that touch the vendor piece.
		///</summary>
		private const string PATH_SCHICK_IN_PROCESS=@"WpfControlsOD\Bridges\SchickInProcess.cs";
		private const string PATH_QUICKBOOKS_IN_PROCESS=@"WpfControlsOD\Bridges\QuickBooksInProcess.cs";
		private const string PATH_GURU_IN_PROCESS=@"WpfControlsOD\Bridges\GuruInProcess.cs";
		private const string PATH_TOPAZ_IN_PROCESS=@"OpenPmsDentalBusiness\UI\TopazInProcess.cs";
		///<summary>This file, which names the patterns it scans for.</summary>
		private const string PATH_THIS_FILE=@"NoHqTests\Helper32VendorScanTests.cs";
		private static readonly string[] _arrayFoldersSkipped=new[] { "bin","obj","packages",".git",".vs",".claude","TestResults","node_modules" };

		private static List<string> GetFiles(string pathRoot,string pattern) {
			List<string> listFiles=new List<string>();
			Stack<string> stackFolders=new Stack<string>();
			stackFolders.Push(pathRoot);
			while(stackFolders.Count>0) {
				string folder=stackFolders.Pop();
				foreach(string folderSub in Directory.GetDirectories(folder)) {
					string name=Path.GetFileName(folderSub);
					if(_arrayFoldersSkipped.Contains(name,StringComparer.OrdinalIgnoreCase) || name.StartsWith(".")) {
						continue;
					}
					stackFolders.Push(folderSub);
				}
				listFiles.AddRange(Directory.GetFiles(folder,pattern));
			}
			return listFiles.Select(x => x.Substring(pathRoot.Length).TrimStart('\\')).OrderBy(x => x,StringComparer.OrdinalIgnoreCase).ToList();
		}

		private static string GetRoot() {
			return RemovedAssembliesTests.GetRepositoryRoot().TrimEnd('\\');
		}

		private static string Read(string pathRelative) {
			return File.ReadAllText(Path.Combine(GetRoot(),pathRelative)).Replace("\r\n","\n");
		}

		///<summary>The source files outside the helper and its tests (and this file) whose text without comments matches regex.</summary>
		private static List<string> GetSourcesMatching(Regex regex) {
			string pathRoot=GetRoot();
			return GetFiles(pathRoot,"*.cs")
				.Where(x => !_arrayFoldersAllowed.Any(y => x.StartsWith(y,StringComparison.OrdinalIgnoreCase)))
				.Where(x => !x.Equals(PATH_THIS_FILE,StringComparison.OrdinalIgnoreCase))
				.Where(x => regex.IsMatch(StripComments(File.ReadAllText(Path.Combine(pathRoot,x)))))
				.ToList();
		}

		///<summary>The interop reference of name in a project's text is embedded (EmbedInteropTypes True), so the interop file is never copied
		///anywhere a release takes files from.</summary>
		private static void AssertEmbedded(string textProject,string name,string what) {
			Match match=Regex.Match(textProject,@"<Reference Include="""+Regex.Escape(name)+@"[^""]*"">\s*(<SpecificVersion>False</SpecificVersion>\s*)?"
				+@"(<HintPath>[^<]*</HintPath>\s*<EmbedInteropTypes>True</EmbedInteropTypes>|<EmbedInteropTypes>True</EmbedInteropTypes>\s*<HintPath>[^<]*</HintPath>)");
			Assert.IsTrue(match.Success,name+" is embedded in "+what+".");
		}

		[TestMethod]
		public void OnlyTheHelperItsTestsAndWpfControlsOD_ReferenceTheQuickBooksDesktopInterop() {
			string pathRoot=GetRoot();
			List<string> listProjects=GetFiles(pathRoot,"*.*proj");
			Assert.IsTrue(listProjects.Any(x => x.Equals(@"OpenPmsDental\OpenPmsDental.csproj",StringComparison.OrdinalIgnoreCase)),"The scan sees the program.");
			Regex regexReference=new Regex(@"Interop\.QBFC|QBFC\d+Lib|a8794a33-4648-4735-9ca8-65cd56ebbb84",RegexOptions.IgnoreCase);
			List<string> listReferencing=listProjects.Where(x => regexReference.IsMatch(File.ReadAllText(Path.Combine(pathRoot,x)))).ToList();
			CollectionAssert.AreEquivalent(_arrayProjectsAllowed,listReferencing,"Projects that reference QBFC: "+string.Join(", ",listReferencing));
			//The helper and WpfControlsOD embed the interop types, so Interop.QBFC10.dll is never copied anywhere a release takes files from.
			AssertEmbedded(Read(_arrayProjectsAllowed[0]),"Interop.QBFC10","the helper");
			AssertEmbedded(Read(_arrayProjectsAllowed[2]),"Interop.QBFC10","WpfControlsOD");
		}

		[TestMethod]
		public void OnlyTheHelperItsTestsAndTheInProcessFile_UseTheQuickBooksDesktopTypes() {
			Regex regexUse=new Regex(@"^\s*using\s+Interop\.QBFC|\bInterop\.QBFC\d*\s*\.\s*(?!dll\b)\w|\bnew\s+QBSessionManager\b",RegexOptions.Multiline);
			List<string> listUsing=GetSourcesMatching(regexUse);
			CollectionAssert.AreEquivalent(new[] { PATH_QUICKBOOKS_IN_PROCESS },listUsing,"QBFC used outside the helper: "+string.Join(", ",listUsing));
			StringAssert.Contains(Read(@"OpenPmsDentalHelper32\QuickBooks\QuickBooksModule.cs"),"using Interop.QBFC10;");
		}

		[TestMethod]
		public void TheHelper_ReferencesNoProjectOfThisRepository() {
			string text=Read(_arrayProjectsAllowed[0]);
			Assert.IsFalse(Regex.IsMatch(text,@"<ProjectReference\b"),"The helper references no project.");
			//Its only references: the framework, and the QBFC and Schick CDR interops of Required dlls (S3a-14).
			List<string> listHintPaths=Regex.Matches(text,@"<HintPath>([^<]*)</HintPath>").Cast<Match>().Select(x => x.Groups[1].Value).ToList();
			CollectionAssert.AreEqual(new[] { @"..\Required dlls\Interop.QBFC10.dll",@"..\Required dlls\Interop.CDRDATALib.dll",
				@"..\Required dlls\Interop.CDRDicom.dll" },listHintPaths);
			Assert.IsFalse(Regex.IsMatch(text,@"<PackageReference\s+Include="),"The helper uses no package.");
			StringAssert.Contains(text,"<PlatformTarget>x86</PlatformTarget>");
			StringAssert.Contains(text,"<OutputType>WinExe</OutputType>");
			//The protocol goes the other way: linked into CodeBase.
			StringAssert.Contains(Read(@"CodeBase\xCodeBase.csproj"),@"<Compile Include=""..\OpenPmsDentalHelper32\Helper32Protocol.cs"">");
		}

		[TestMethod]
		public void OnlyTheHelperItsTestsAndWpfControlsOD_ReferenceTheSchickInterop() {
			//S3a-14: Schick CDR DICOM's automation interop (type libraries CDRDicom and CDRDATALib) by name or type library GUID.
			string pathRoot=GetRoot();
			List<string> listProjects=GetFiles(pathRoot,"*.*proj");
			Regex regexReference=new Regex(@"Interop\.CDRDATALib|Interop\.CDRDicom|dbd30f40-2455-11d5-81ba-00b0d0403e92|8d82b9c6-374c-48f9-ae2a-a51fb7f44752",
				RegexOptions.IgnoreCase);
			List<string> listReferencing=listProjects.Where(x => regexReference.IsMatch(File.ReadAllText(Path.Combine(pathRoot,x)))).ToList();
			CollectionAssert.AreEquivalent(_arrayProjectsAllowed,listReferencing,"Projects that reference the Schick interop: "+string.Join(", ",listReferencing));
			//Embedded in the helper and in WpfControlsOD, as the program embedded them, so neither interop file is copied anywhere a release takes
			//files from.
			foreach(string name in new[] { "Interop.CDRDATALib","Interop.CDRDicom" }) {
				AssertEmbedded(Read(_arrayProjectsAllowed[0]),name,"the helper");
				AssertEmbedded(Read(_arrayProjectsAllowed[2]),name,"WpfControlsOD");
			}
		}

		[TestMethod]
		public void OnlyTheHelperItsTestsAndTheInProcessFile_UseTheSchickTypes() {
			Regex regexUse=new Regex(@"^\s*using\s+(CDRDicom|CDRDATALib)\b|\b(CDRDicom|CDRDATALib)\s*\.\s*(Application|ExamDocument|ICDR\w*|IExamDocument\w*)\b",
				RegexOptions.Multiline);
			List<string> listUsing=GetSourcesMatching(regexUse);
			CollectionAssert.AreEquivalent(new[] { PATH_SCHICK_IN_PROCESS },listUsing,"Schick CDR's types used outside the helper: "+string.Join(", ",listUsing));
			StringAssert.Contains(Read(@"OpenPmsDentalHelper32\Schick\SchickModule.cs"),"new CDRDicom.Application()");
		}

		[TestMethod]
		public void OnlyTheHelperAndTheInProcessFile_LoadMedVisorInterfaceDll() {
			//S3a-14: Guru's native MedVisorInterface.dll is declared (DllImport) in the helper's Guru module; S3a-14n: and in the program's
			//in-process file. Loaded nowhere else.
			string pathRoot=GetRoot();
			Regex regexLoad=new Regex(@"DllImport\s*\(\s*""MedVisorInterface(\.dll)?""|LoadLibrary\w*\s*\([^)]*MedVisorInterface",RegexOptions.IgnoreCase);
			List<string> listLoading=GetFiles(pathRoot,"*.cs")
				.Where(x => regexLoad.IsMatch(StripComments(File.ReadAllText(Path.Combine(pathRoot,x)))))
				.ToList();
			CollectionAssert.AreEquivalent(new[] { @"OpenPmsDentalHelper32\Guru\GuruModule.cs",PATH_GURU_IN_PROCESS },listLoading,
				"Files that load MedVisorInterface.dll: "+string.Join(", ",listLoading));
		}

		[TestMethod]
		public void OnlyTheHelperAndTheInProcessFile_LoadSigPlusNetDll() {
			//S3a-14: Topaz's SigPlusNET.dll is loaded (Assembly.Load*) by the helper's Topaz module; S3a-14n: and by the program's in-process file.
			//TopazWrapper only finds the file (FindAssemblyPath) and goes to one of the two; the tests only copy a synthetic stand-in.
			string pathRoot=GetRoot();
			Regex regexTopaz=new Regex(@"SigPlusNET|TopazCommands\.ASSEMBLY_FILE_NAME|TopazWrapper\.ASSEMBLY_FILE_NAME|CONTROL_TYPE_NAME");
			Regex regexLoad=new Regex(@"\bAssembly\s*\.\s*(Load|LoadFrom|LoadFile|UnsafeLoadFrom|ReflectionOnlyLoadFrom)\s*\(|\bActivator\s*\.\s*CreateInstance\s*\(");
			List<string> listLoading=GetFiles(pathRoot,"*.cs")
				.Where(x => !x.StartsWith(@"tests\",StringComparison.OrdinalIgnoreCase) && !x.StartsWith(@"NoHqTests\",StringComparison.OrdinalIgnoreCase))
				.Where(x => {
					string text=StripComments(File.ReadAllText(Path.Combine(pathRoot,x)));
					return regexTopaz.IsMatch(text) && regexLoad.IsMatch(text);
				})
				.ToList();
			CollectionAssert.AreEquivalent(new[] { @"OpenPmsDentalHelper32\Topaz\TopazModule.cs",PATH_TOPAZ_IN_PROCESS },listLoading,"Files that load SigPlusNET.dll: "
				+string.Join(", ",listLoading));
			string textWrapper=StripComments(Read(@"OpenPmsDentalBusiness\UI\TopazWrapper.cs"));
			Assert.IsFalse(Regex.IsMatch(textWrapper,@"\bSystem\.Reflection\b|\bGetMethod\s*\(|\bGetField\s*\("),"TopazWrapper reflects on no Topaz type itself.");
			StringAssert.Contains(textWrapper,"new TopazSignatureView(");
			//No project references SigPlusNET.dll or MedVisorInterface.dll as a file.
			Regex regexProjectReference=new Regex(@"<(Reference|HintPath|None|Content)[^>]*(SigPlusNET|MedVisorInterface)|<HintPath>[^<]*(SigPlusNET|MedVisorInterface)",
				RegexOptions.IgnoreCase);
			List<string> listProjects=GetFiles(pathRoot,"*.*proj").Where(x => regexProjectReference.IsMatch(File.ReadAllText(Path.Combine(pathRoot,x)))).ToList();
			Assert.AreEqual(0,listProjects.Count,"Projects that reference SigPlusNET.dll or MedVisorInterface.dll: "+string.Join(", ",listProjects));
		}

		[TestMethod]
		public void EachInProcessFile_ReachesItsVendorPieceOnlyThroughItsChooser() {
			//S3a-14n: the one run-time choice per vendor piece. Each in-process file creates or loads the vendor piece only through
			//NativeFirstChooser, and the bridge sends the call to the helper only when the chooser did not pick the program's process.
			//Schick CDR: the application object is activated by the chooser; "new CDRDicom.Application()" is only the activation it is given.
			string textSchick=StripComments(Read(PATH_SCHICK_IN_PROCESS));
			Assert.AreEqual(1,Regex.Matches(textSchick,@"new\s+CDRDicom\.Application\(\)").Count);
			StringAssert.Contains(textSchick,"internal static Func<CDRDicom.Application> FuncCreateApplication=() => new CDRDicom.Application();");
			StringAssert.Contains(textSchick,"NativeFirstChooser.SchickCdr.ActivateComObject(FuncCreateApplication)");
			Assert.AreEqual(1,Regex.Matches(textSchick,@"\bFuncCreateApplication\b").Count-1,"FuncCreateApplication is used by the chooser's call only.");
			string textSchickBridge=Read(@"WpfControlsOD\Bridges\Schick.cs");
			AssertInOrder(textSchickBridge,"if(SchickInProcess.ShowExam(pat.LName,pat.FName,patID)) {\n\t\t\t\treturn;\n\t\t\t}\n",
				"Helper32Client.Default.CallPumping(SchickCommands.MODULE,SchickCommands.SHOW_EXAM,","Schick.cs");
			//QuickBooks Desktop: the session manager is activated by the chooser; each public method asks IsChosen first.
			string textQuickBooks=StripComments(Read(PATH_QUICKBOOKS_IN_PROCESS));
			Assert.AreEqual(1,Regex.Matches(textQuickBooks,@"new\s+QBSessionManager\(\)").Count);
			StringAssert.Contains(textQuickBooks,"internal static Func<QBSessionManager> FuncCreateSessionManager=() => new QBSessionManager();");
			Assert.AreEqual(2,Regex.Matches(textQuickBooks,@"\.ActivateComObject\(FuncCreateSessionManager\)").Count,"IsChosen's and CreateSessionManager's.");
			Assert.AreEqual(2,Regex.Matches(textQuickBooks,@"NativeFirstChooser\.QuickBooksDesktop").Count);
			Assert.AreEqual(1,Regex.Matches(textQuickBooks,@"SessionManager=CreateSessionManager\(\);").Count,"OpenConnection's, as new QBSessionManager() was.");
			string textQuickBooksBridge=Read(@"WpfControlsOD\Bridges\QuickBooks.cs");
			foreach(string pair in new[] { "TestConnection|QuickBooksCommands.TEST_CONNECTION","GetListOfAccounts|QuickBooksCommands.GET_LIST_OF_ACCOUNTS",
				"GetListOfClasses|QuickBooksCommands.GET_LIST_OF_CLASSES","CreateDeposit|QuickBooksCommands.CREATE_DEPOSIT" })
			{
				string[] arrayParts=pair.Split('|');
				Match matchMethod=Regex.Match(textQuickBooksBridge,@"\n\t\tpublic static [^\n(]* "+arrayParts[0]+@"\(");
				Assert.IsTrue(matchMethod.Success,"QuickBooks."+arrayParts[0]);
				int indexMethod=matchMethod.Index;
				string textMethod=textQuickBooksBridge.Substring(indexMethod,textQuickBooksBridge.IndexOf("\n\t\t}\n",indexMethod,StringComparison.Ordinal)-indexMethod);
				AssertInOrder(textMethod,"if(QuickBooksInProcess.IsChosen()) {","QuickBooksInProcess."+arrayParts[0]+"(","QuickBooks."+arrayParts[0]);
				AssertInOrder(textMethod,"QuickBooksInProcess."+arrayParts[0]+"(",arrayParts[1],"QuickBooks."+arrayParts[0]);
			}
			//Guru: the DLL's header decides; Guru.cs calls the in-process functions only after IsChosen.
			string textGuru=StripComments(Read(PATH_GURU_IN_PROCESS));
			StringAssert.Contains(textGuru,"return NativeFirstChooser.Guru.ChooseForNativeDll(DLL_FILE_NAME,PeImageInfo.IMAGE_FILE_MACHINE_I386)==VendorPath.InProcess;");
			string textGuruBridge=Read(@"WpfControlsOD\Bridges\Guru.cs");
			foreach(string call in new[] { "GuruInProcess.MVStart()","GuruInProcess.MVSendPatient(mvPatient)" }) {
				Assert.AreEqual(1,Regex.Matches(textGuruBridge,Regex.Escape(call)).Count,call);
				AssertInOrder(textGuruBridge,"if(GuruInProcess.IsChosen()) {\n\t\t\t\treturn "+call+";\n\t\t\t}\n","Helper32Client.Default.Call(GuruCommands.MODULE,","Guru.cs");
			}
			//Topaz: the file's header decides; TopazWrapper loads in the program's process only when the chooser picked it.
			string textTopazWrapper=StripComments(Read(@"OpenPmsDentalBusiness\UI\TopazWrapper.cs"));
			AssertInOrder(textTopazWrapper,"_vendorPath=NativeFirstChooser.Topaz.ChooseForManagedAssembly(path);\n\t\t\t\tif(_vendorPath==VendorPath.InProcess) {\n"
				+"\t\t\t\t\t_typeSigPlusNET=TopazInProcess.LoadControlType(path,out string loadErrorInProcess);","TopazCommands.MODULE,TopazCommands.LOAD","TopazWrapper.cs");
			//No other file of the program uses the four in-process files.
			Regex regexInProcess=new Regex(@"\b(SchickInProcess|QuickBooksInProcess|GuruInProcess|TopazInProcess)\s*\.");
			List<string> listUsers=GetSourcesMatching(regexInProcess).Where(x => !x.StartsWith(@"tests\",StringComparison.OrdinalIgnoreCase)).ToList();
			CollectionAssert.AreEquivalent(new[] { @"WpfControlsOD\Bridges\Schick.cs",@"WpfControlsOD\Bridges\QuickBooks.cs",@"WpfControlsOD\Bridges\Guru.cs",
				@"OpenPmsDentalBusiness\UI\TopazWrapper.cs" },listUsers,
				"Files that use the in-process paths: "+string.Join(", ",listUsers));
			//The chooser is CodeBase's one class; no other file reads PE headers to choose a vendor path.
			StringAssert.Contains(Read(@"CodeBase\Utilities\NativeFirstChooser.cs"),"public sealed class NativeFirstChooser {");
		}

		[TestMethod]
		public void Cadi_IsCreatedOnlyInTheInProcessFileAndTheHelpersModule_AfterItsChooser() {
			//N64-A3: Mediadent / CADI's automation object (ProgID Mediadent.Application) is created in two places only, the program's in-process
			//file (CADIInProcess.cs, through NativeFirstChooser.Cadi) and the helper's Cadi module (the fallback); CADI.cs asks the chooser first.
			string pathRoot=GetRoot();
			Regex regexProgId=new Regex(@"""Mediadent\.Application""");
			List<string> listNaming=GetFiles(pathRoot,"*.cs")
				.Where(x => !x.StartsWith(@"tests\",StringComparison.OrdinalIgnoreCase) && !x.StartsWith(@"NoHqTests\",StringComparison.OrdinalIgnoreCase))
				.Where(x => regexProgId.IsMatch(StripComments(File.ReadAllText(Path.Combine(pathRoot,x)))))
				.ToList();
			CollectionAssert.AreEquivalent(new[] { @"WpfControlsOD\Bridges\CADIInProcess.cs",@"OpenPmsDentalHelper32\Cadi\CadiModule.cs" },listNaming,
				"Files that name CADI's ProgID: "+string.Join(", ",listNaming));
			string textInProcess=StripComments(Read(@"WpfControlsOD\Bridges\CADIInProcess.cs"));
			Assert.AreEqual(1,Regex.Matches(textInProcess,@"Type\.GetTypeFromProgID\(PROGRAM_ID\)").Count);
			Assert.AreEqual(1,Regex.Matches(textInProcess,@"CADIInProcess\.FuncCreateComObject\(\)").Count,"CADINativeWindow.SendData's creation only.");
			StringAssert.Contains(textInProcess,"return NativeFirstChooser.Cadi.ChooseForComServer(FuncReadRegistration)!=VendorPath.Helper32;");
			StringAssert.Contains(textInProcess,"catch(Exception ex) when(CADIInProcess.IsRetriedInHelper(ex,isCadiReached)) {");
			AssertInOrder(Read(@"WpfControlsOD\Bridges\CADI.cs"),"if(CADIInProcess.IsChosen()) {","SendDataToHelper(cadiPatient);","CADI.cs");
			StringAssert.Contains(Read(@"OpenPmsDentalHelper32\Helper32Host.cs"),"listModules.Add(new Cadi.CadiModule());");
			//No other file of the program uses CADI's in-process file.
			List<string> listUsers=GetSourcesMatching(new Regex(@"\bCADIInProcess\s*\.|\bnew\s+CADINativeWindow\b"))
				.Where(x => !x.StartsWith(@"tests\",StringComparison.OrdinalIgnoreCase)).ToList();
			CollectionAssert.AreEquivalent(new[] { @"WpfControlsOD\Bridges\CADI.cs",@"WpfControlsOD\Bridges\CADIInProcess.cs" },listUsers,
				"Files that use CADI's in-process path: "+string.Join(", ",listUsers));
		}

		///<summary>In text, first occurs once and before second.</summary>
		private static void AssertInOrder(string text,string first,string second,string what) {
			text=text.Replace("\r\n","\n");
			int indexFirst=text.IndexOf(first,StringComparison.Ordinal);
			Assert.IsTrue(indexFirst>=0,what+": "+first+" is there.");
			int indexSecond=text.IndexOf(second,indexFirst,StringComparison.Ordinal);
			Assert.IsTrue(indexSecond>indexFirst,what+": "+second+" comes after "+first);
		}

		///<summary>The text without // and /* */ comments (string contents are kept; the scanned patterns do not occur in strings).</summary>
		private static string StripComments(string text) {
			text=Regex.Replace(text,@"/\*.*?\*/","",RegexOptions.Singleline);
			return Regex.Replace(text,@"(?<!:)//[^\r\n]*","");
		}
	}
}
