// S3a-13 (net10 plan): QuickBooks Desktop's 32-bit QBFC runs only in the 32-bit helper program. No project but the helper (and its tests,
// which compile the helper's QuickBooks module against the interop and a fake) references Interop.QBFC10, no other source file uses its types,
// and the helper references no other project of this repository (the protocol file is linked into CodeBase, not the other way round), so it
// can have its own runtime after the switch (O-15). A scan of the repository's project and source files; it needs no build and starts nothing.
// S3a-14 adds the Schick interop, SigPlusNET.dll and MedVisorInterface.dll to the list.
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
			//Its only references: the framework, and the QBFC interop of Required dlls.
			List<string> listHintPaths=Regex.Matches(text,@"<HintPath>([^<]*)</HintPath>").Cast<Match>().Select(x => x.Groups[1].Value).ToList();
			CollectionAssert.AreEqual(new[] { @"..\Required dlls\Interop.QBFC10.dll" },listHintPaths);
			Assert.IsFalse(Regex.IsMatch(text,@"<PackageReference\s+Include="),"The helper uses no package.");
			StringAssert.Contains(text,"<PlatformTarget>x86</PlatformTarget>");
			StringAssert.Contains(text,"<OutputType>WinExe</OutputType>");
			//The protocol goes the other way: linked into CodeBase.
			StringAssert.Contains(File.ReadAllText(Path.Combine(pathRoot,@"CodeBase\xCodeBase.csproj")),
				@"<Compile Include=""..\OpenPmsDentalHelper32\Helper32Protocol.cs"">");
		}
	}
}
