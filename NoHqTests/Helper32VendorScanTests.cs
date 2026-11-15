// S3a-13 (net10 plan): QuickBooks Desktop's 32-bit QBFC runs only in the 32-bit helper program. No project but the helper (and its tests,
// which compile the helper's QuickBooks module against the interop and a fake) references Interop.QBFC10, no other source file uses its types,
// and the helper references no other project of this repository (the protocol file is linked into CodeBase, not the other way round), so it
// can have its own runtime after the switch (O-15). A scan of the repository's project and source files; it needs no build and starts nothing.
// S3a-14 added Schick CDR's interop (referenced by the helper and its tests only), SigPlusNET.dll and MedVisorInterface.dll (loaded by the
// helper only).
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
		///<summary>The projects that may reference the 32-bit-only vendor pieces: the helper, and its tests.</summary>
		private static readonly string[] _arrayProjectsAllowed=new[] { @"OpenPmsDentalHelper32\OpenPmsDentalHelper32.csproj",
			@"tests\Helper32Tests\Helper32Tests.csproj" };
		///<summary>The folders whose source may use the vendor pieces' types.</summary>
		private static readonly string[] _arrayFoldersAllowed=new[] { @"OpenPmsDentalHelper32\",@"tests\Helper32Tests\" };
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

		[TestMethod]
		public void OnlyTheHelperAndItsTests_ReferenceTheQuickBooksDesktopInterop() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot().TrimEnd('\\');
			List<string> listProjects=GetFiles(pathRoot,"*.*proj");
			Assert.IsTrue(listProjects.Any(x => x.Equals(@"OpenPmsDental\OpenPmsDental.csproj",StringComparison.OrdinalIgnoreCase)),"The scan sees the program.");
			Regex regexReference=new Regex(@"Interop\.QBFC|QBFC\d+Lib|a8794a33-4648-4735-9ca8-65cd56ebbb84",RegexOptions.IgnoreCase);
			List<string> listReferencing=listProjects.Where(x => regexReference.IsMatch(File.ReadAllText(Path.Combine(pathRoot,x)))).ToList();
			CollectionAssert.AreEquivalent(_arrayProjectsAllowed,listReferencing,"Projects that reference QBFC: "+string.Join(", ",listReferencing));
			//The helper embeds the interop types, so Interop.QBFC10.dll is never copied anywhere a release takes files from.
			string textHelper=File.ReadAllText(Path.Combine(pathRoot,_arrayProjectsAllowed[0]));
			StringAssert.Contains(textHelper,"<EmbedInteropTypes>True</EmbedInteropTypes>");
		}

		[TestMethod]
		public void OnlyTheHelperAndItsTests_UseTheQuickBooksDesktopTypes() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot().TrimEnd('\\');
			Regex regexUse=new Regex(@"^\s*using\s+Interop\.QBFC|\bInterop\.QBFC\d*\s*\.\s*(?!dll\b)\w|\bnew\s+QBSessionManager\b",RegexOptions.Multiline);
			List<string> listUsing=GetFiles(pathRoot,"*.cs")
				.Where(x => !_arrayFoldersAllowed.Any(y => x.StartsWith(y,StringComparison.OrdinalIgnoreCase)))
				.Where(x => regexUse.IsMatch(File.ReadAllText(Path.Combine(pathRoot,x))))
				.ToList();
			Assert.AreEqual(0,listUsing.Count,"QBFC used outside the helper: "+string.Join(", ",listUsing));
			StringAssert.Contains(File.ReadAllText(Path.Combine(pathRoot,@"OpenPmsDentalHelper32\QuickBooks\QuickBooksModule.cs")),"using Interop.QBFC10;");
		}

		[TestMethod]
		public void TheHelper_ReferencesNoProjectOfThisRepository() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot().TrimEnd('\\');
			string text=File.ReadAllText(Path.Combine(pathRoot,_arrayProjectsAllowed[0]));
			Assert.IsFalse(Regex.IsMatch(text,@"<ProjectReference\b"),"The helper references no project.");
			//Its only references: the framework, and the QBFC and Schick CDR interops of Required dlls (S3a-14).
			List<string> listHintPaths=Regex.Matches(text,@"<HintPath>([^<]*)</HintPath>").Cast<Match>().Select(x => x.Groups[1].Value).ToList();
			CollectionAssert.AreEqual(new[] { @"..\Required dlls\Interop.QBFC10.dll",@"..\Required dlls\Interop.CDRDATALib.dll",
				@"..\Required dlls\Interop.CDRDicom.dll" },listHintPaths);
			Assert.IsFalse(Regex.IsMatch(text,@"<PackageReference\s+Include="),"The helper uses no package.");
			StringAssert.Contains(text,"<PlatformTarget>x86</PlatformTarget>");
			StringAssert.Contains(text,"<OutputType>WinExe</OutputType>");
			//The protocol goes the other way: linked into CodeBase.
			StringAssert.Contains(File.ReadAllText(Path.Combine(pathRoot,@"CodeBase\xCodeBase.csproj")),
				@"<Compile Include=""..\OpenPmsDentalHelper32\Helper32Protocol.cs"">");
		}

		[TestMethod]
		public void OnlyTheHelperAndItsTests_ReferenceTheSchickInterop() {
			//S3a-14: Schick CDR DICOM's automation interop (type libraries CDRDicom and CDRDATALib) by name or type library GUID.
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot().TrimEnd('\\');
			List<string> listProjects=GetFiles(pathRoot,"*.*proj");
			Regex regexReference=new Regex(@"Interop\.CDRDATALib|Interop\.CDRDicom|dbd30f40-2455-11d5-81ba-00b0d0403e92|8d82b9c6-374c-48f9-ae2a-a51fb7f44752",
				RegexOptions.IgnoreCase);
			List<string> listReferencing=listProjects.Where(x => regexReference.IsMatch(File.ReadAllText(Path.Combine(pathRoot,x)))).ToList();
			CollectionAssert.AreEquivalent(_arrayProjectsAllowed,listReferencing,"Projects that reference the Schick interop: "+string.Join(", ",listReferencing));
			//Embedded in the helper, as the program embedded it, so neither interop file is copied anywhere a release takes files from.
			string textHelper=File.ReadAllText(Path.Combine(pathRoot,_arrayProjectsAllowed[0]));
			foreach(string name in new[] { "Interop.CDRDATALib","Interop.CDRDicom" }) {
				Match match=Regex.Match(textHelper,@"<Reference Include="""+Regex.Escape(name)+@""">\s*<HintPath>[^<]*</HintPath>\s*<EmbedInteropTypes>True</EmbedInteropTypes>");
				Assert.IsTrue(match.Success,name+" is embedded in the helper.");
			}
		}

		[TestMethod]
		public void OnlyTheHelperAndItsTests_UseTheSchickTypes() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot().TrimEnd('\\');
			Regex regexUse=new Regex(@"^\s*using\s+(CDRDicom|CDRDATALib)\b|\b(CDRDicom|CDRDATALib)\s*\.\s*(Application|ExamDocument|ICDR\w*|IExamDocument\w*)\b",
				RegexOptions.Multiline);
			List<string> listUsing=GetFiles(pathRoot,"*.cs")
				.Where(x => !_arrayFoldersAllowed.Any(y => x.StartsWith(y,StringComparison.OrdinalIgnoreCase)))
				.Where(x => !x.Equals(PATH_THIS_FILE,StringComparison.OrdinalIgnoreCase))//its own patterns and expectations
				.Where(x => regexUse.IsMatch(StripComments(File.ReadAllText(Path.Combine(pathRoot,x)))))
				.ToList();
			Assert.AreEqual(0,listUsing.Count,"Schick CDR's types used outside the helper: "+string.Join(", ",listUsing));
			StringAssert.Contains(File.ReadAllText(Path.Combine(pathRoot,@"OpenPmsDentalHelper32\Schick\SchickModule.cs")),"new CDRDicom.Application()");
		}

		[TestMethod]
		public void OnlyTheHelper_LoadsMedVisorInterfaceDll() {
			//S3a-14: Guru's native MedVisorInterface.dll is declared (DllImport) in the helper's Guru module only, and loaded nowhere else.
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot().TrimEnd('\\');
			Regex regexLoad=new Regex(@"DllImport\s*\(\s*""MedVisorInterface(\.dll)?""|LoadLibrary\w*\s*\([^)]*MedVisorInterface",RegexOptions.IgnoreCase);
			List<string> listLoading=GetFiles(pathRoot,"*.cs")
				.Where(x => regexLoad.IsMatch(StripComments(File.ReadAllText(Path.Combine(pathRoot,x)))))
				.ToList();
			CollectionAssert.AreEquivalent(new[] { @"OpenPmsDentalHelper32\Guru\GuruModule.cs" },listLoading,"Files that load MedVisorInterface.dll: "
				+string.Join(", ",listLoading));
		}

		[TestMethod]
		public void OnlyTheHelper_LoadsSigPlusNetDll() {
			//S3a-14: Topaz's SigPlusNET.dll is loaded (Assembly.Load*) by the helper's Topaz module only. The program only finds the file
			//(TopazWrapper.FindAssemblyPath) and shows the helper's picture of the control; the tests only copy a synthetic stand-in.
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot().TrimEnd('\\');
			Regex regexTopaz=new Regex(@"SigPlusNET|TopazCommands\.ASSEMBLY_FILE_NAME|TopazWrapper\.ASSEMBLY_FILE_NAME|CONTROL_TYPE_NAME");
			Regex regexLoad=new Regex(@"\bAssembly\s*\.\s*(Load|LoadFrom|LoadFile|UnsafeLoadFrom|ReflectionOnlyLoadFrom)\s*\(|\bActivator\s*\.\s*CreateInstance\s*\(");
			List<string> listLoading=GetFiles(pathRoot,"*.cs")
				.Where(x => !x.StartsWith(@"tests\",StringComparison.OrdinalIgnoreCase) && !x.StartsWith(@"NoHqTests\",StringComparison.OrdinalIgnoreCase))
				.Where(x => {
					string text=StripComments(File.ReadAllText(Path.Combine(pathRoot,x)));
					return regexTopaz.IsMatch(text) && regexLoad.IsMatch(text);
				})
				.ToList();
			CollectionAssert.AreEquivalent(new[] { @"OpenPmsDentalHelper32\Topaz\TopazModule.cs" },listLoading,"Files that load SigPlusNET.dll: "
				+string.Join(", ",listLoading));
			string textWrapper=StripComments(File.ReadAllText(Path.Combine(pathRoot,@"OpenPmsDentalBusiness\UI\TopazWrapper.cs")));
			Assert.IsFalse(Regex.IsMatch(textWrapper,@"\bSystem\.Reflection\b|\bGetMethod\s*\(|\bGetField\s*\("),"TopazWrapper reflects on no Topaz type itself.");
			StringAssert.Contains(textWrapper,"new TopazSignatureView(");
			//No project references SigPlusNET.dll or MedVisorInterface.dll as a file.
			Regex regexProjectReference=new Regex(@"<(Reference|HintPath|None|Content)[^>]*(SigPlusNET|MedVisorInterface)|<HintPath>[^<]*(SigPlusNET|MedVisorInterface)",
				RegexOptions.IgnoreCase);
			List<string> listProjects=GetFiles(pathRoot,"*.*proj").Where(x => regexProjectReference.IsMatch(File.ReadAllText(Path.Combine(pathRoot,x)))).ToList();
			Assert.AreEqual(0,listProjects.Count,"Projects that reference SigPlusNET.dll or MedVisorInterface.dll: "+string.Join(", ",listProjects));
		}

		///<summary>The text without // and /* */ comments (string contents are kept; the scanned patterns do not occur in strings).</summary>
		private static string StripComments(string text) {
			text=Regex.Replace(text,@"/\*.*?\*/","",RegexOptions.Singleline);
			return Regex.Replace(text,@"(?<!:)//[^\r\n]*","");
		}
	}
}
