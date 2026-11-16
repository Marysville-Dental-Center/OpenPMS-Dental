// TC-9b of the WPF tooth chart track (docs/port-notes/TC-9b.md, docs/specs/wpf-tooth-chart-plan.md): SharpDX 4.0.1 and the Tao Framework, the
// libraries of the DirectX and OpenGL tooth charts that TC-9a retired, are gone from the repository, every project and the build output, and
// the licence window's SharpDX and Tao Framework vendor texts went with their resources. What the release does with the five files (the
// allow-list, the deny-list, ListRetiredFiles and the setup's [InstallDelete]) is ReleaseFlowTests' SharpDXTaoRetiredTests.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectFileTests;

namespace BINBTests {
	[TestClass]
	public class SharpDXTaoRemovedTests {
		///<summary>The five files TC-9b deleted from "Required dlls".</summary>
		private static readonly string[] _arrayFiles={ "SharpDX.dll","SharpDX.Direct3D9.dll","SharpDX.Mathematics.dll","Tao.OpenGl.dll",
			"Tao.Platform.Windows.dll" };
		///<summary>An assembly name or file name of SharpDX or the Tao Framework (Tao.FreeGlut, which BIN-B removed, included).</summary>
		private static readonly Regex _regexAssembly=new Regex(@"^(SharpDX(\.\w+)*|Tao\.\w+(\.\w+)*)(\.dll)?$",RegexOptions.IgnoreCase);

		///<summary>The references of a built file, read with Mono.Cecil from a copy of its bytes: nothing is loaded or locked.</summary>
		private static List<string> ReadReferences(string path) {
			using(MemoryStream memoryStream=new MemoryStream(File.ReadAllBytes(path)))
			using(Mono.Cecil.ModuleDefinition moduleDefinition=Mono.Cecil.ModuleDefinition.ReadModule(memoryStream)) {
				return moduleDefinition.AssemblyReferences.Select(x => x.Name).ToList();
			}
		}

		[TestMethod]
		public void TheFiveFiles_AreNotInRequiredDlls() {
			string folder=Path.Combine(TestPaths.RepoRoot,"Required dlls");
			Assert.IsTrue(Directory.Exists(folder),"Required dlls stays for the files without a package (the Direct Project, the CDR and QBFC interop).");
			Assert.IsTrue(File.Exists(Path.Combine(folder,"Health.Direct.Agent.dll")),"The check looks at the right folder.");
			List<string> listFound=Directory.GetFiles(folder,"*",SearchOption.AllDirectories).Select(x => Path.GetFileName(x))
				.Where(x => _regexAssembly.IsMatch(x)).ToList();
			Assert.AreEqual(0,listFound.Count,"Back in Required dlls: "+string.Join(", ",listFound));
			foreach(string fileName in _arrayFiles) {
				Assert.IsTrue(_regexAssembly.IsMatch(fileName),fileName);
			}
		}

		[TestMethod]
		public void NoProject_NamesSharpDXOrTao() {
			string root=TestPaths.RepoRoot;
			List<string> listProjects=TestPaths.GetFiles(root,".csproj",".vbproj",".vcxproj",".targets",".props");
			foreach(string name in new[] { "OpenPmsDental.csproj","xCodeBase.csproj","UnitTests.csproj","SparksToothChart.csproj" }) {
				Assert.IsTrue(listProjects.Any(x => Path.GetFileName(x).Equals(name,StringComparison.OrdinalIgnoreCase)),"No "+name+" under "+root);
			}
			List<string> listViolations=new List<string>();
			foreach(string path in listProjects) {
				XDocument xDocument=XDocument.Load(path);
				foreach(XElement xElement in xDocument.Descendants()) {
					List<string> listValues=new List<string>();
					string local=xElement.Name.LocalName;
					if(local=="Reference" || local=="PackageReference" || local=="PackageVersion" || local=="Content" || local=="None") {
						listValues.Add(((string)xElement.Attribute("Include")??"").Split(',')[0].Trim());
					}
					if(local=="HintPath") {
						listValues.Add(xElement.Value);
					}
					if(xElement.Attribute("HintPath")!=null) {
						listValues.Add((string)xElement.Attribute("HintPath"));
					}
					foreach(string value in listValues) {
						string fileName=Path.GetFileName(value.Trim().Replace('/','\\'));
						if(fileName!="" && _regexAssembly.IsMatch(fileName)) {
							listViolations.Add(path.Substring(root.Length)+": <"+local+"> names "+value.Trim());
						}
					}
				}
			}
			Assert.AreEqual(0,listViolations.Count,"SharpDX or Tao is named again: "+string.Join("; ",listViolations));
		}

		[TestMethod]
		public void NoSource_UsesSharpDXOrTao() {
			string root=TestPaths.RepoRoot;
			Regex regexUsing=new Regex(@"^\s*using\s+(\w+\s*=\s*)?(SharpDX|Tao)(\.\w+)*\s*;",RegexOptions.Multiline);
			List<string> listSources=TestPaths.GetFiles(root,".cs");
			Assert.IsTrue(listSources.Count>1000,"The source files were not found under "+root);
			List<string> listViolations=new List<string>();
			foreach(string path in listSources) {
				foreach(Match match in regexUsing.Matches(File.ReadAllText(path))) {
					listViolations.Add(path.Substring(root.Length)+": "+match.Value.Trim());
				}
			}
			Assert.AreEqual(0,listViolations.Count,"Source uses SharpDX or Tao: "+string.Join("; ",listViolations));
		}

		///<summary>CodeBase (this test's own reference) and, when this checkout has built them, the program, the chart library and the old
		///UnitTests program reference no SharpDX or Tao assembly, and no build output folder holds one of the five files.</summary>
		[TestMethod]
		public void BuiltAssemblies_ReferenceNoSharpDXOrTao_AndNoOutputHoldsTheFiles() {
			string root=TestPaths.RepoRoot;
			Dictionary<string,List<string>> dictReferences=new Dictionary<string,List<string>> {
				{ "CodeBase",typeof(ODZip).Assembly.GetReferencedAssemblies().Select(x => x.Name).ToList() },
			};
			foreach(string relative in new[] { ProgramAssembly.FindRelative(root,@"OpenPmsDental\bin\Debug","OpenPmsDental"),
				@"SparksToothChart\bin\Debug\SparksToothChart.dll",ProgramAssembly.FindRelative(root,@"UnitTests\bin\Debug","UnitTests") })
			{
				string path=Path.Combine(root,relative);
				if(File.Exists(path)) {
					dictReferences.Add(relative,ReadReferences(path));
				}
			}
			List<string> listViolations=new List<string>();
			foreach(KeyValuePair<string,List<string>> pair in dictReferences) {
				listViolations.AddRange(pair.Value.Where(x => _regexAssembly.IsMatch(x)).Select(x => pair.Key+" references "+x));
			}
			//The test's own folder holds the copy-local closure of CodeBase and OpenPmsDentalBusiness; CodeBase referenced both Tao files until TC-9b.
			List<string> listFolders=new List<string> { AppDomain.CurrentDomain.BaseDirectory };
			foreach(string folder in new[] { @"OpenPmsDental\bin\Debug",@"UnitTests\bin\Debug",@"CodeBase\bin\Debug" }) {
				if(Directory.Exists(Path.Combine(root,folder))) {
					listFolders.Add(Path.Combine(root,folder));
				}
			}
			foreach(string folder in listFolders) {
				listViolations.AddRange(_arrayFiles.Where(x => File.Exists(Path.Combine(folder,x))).Select(x => Path.Combine(folder,x)+" is in a build output"));
			}
			Assert.AreEqual(0,listViolations.Count,string.Join("; ",listViolations));
		}

		///<summary>The licence window shows the notices' components only, and every vendor text belongs to one (AttributionTests,
		///ReleaseIdentityTests): with the two components the SharpDX and Tao Framework texts went, with their resources and files. The SharpDX MIT
		///notice that Helix Toolkit's entry carries for HelixToolkit.Maths is in THIRD-PARTY-NOTICES.txt, not a vendor text.</summary>
		[TestMethod]
		public void LicenceWindow_HasNoSharpDXOrTaoText() {
			string root=TestPaths.RepoRoot;
			foreach(string fileName in new[] { "SharpDX.txt","Tao_OpenGL.txt" }) {
				Assert.IsFalse(File.Exists(Path.Combine(root,"OpenPmsDental","Resources",fileName)),@"OpenPmsDental\Resources\"+fileName+" is back.");
			}
			string formLicense=TestPaths.ReadSource("OpenPmsDental","Forms","FormLicense.cs");
			StringAssert.Contains(formLicense,"public static Dictionary<string,string> GetVendorTexts() {","The vendor texts are still read here.");
			StringAssert.DoesNotMatch(formLicense,new Regex(@"\bSharpDX\b|\bTao\b|Tao_OpenGL"),"FormLicense.cs");
			string resx=TestPaths.ReadSource("OpenPmsDental","Properties","Resources.resx");
			StringAssert.Contains(resx,"<data name=\"WeCantSpellHunspell\"","The resource file is the program's.");
			StringAssert.DoesNotMatch(resx,new Regex(@"<data name=""(SharpDX|Tao_OpenGL)""|SharpDX\.txt|Tao_OpenGL\.txt"),"Resources.resx");
			string designer=TestPaths.ReadSource("OpenPmsDental","Properties","Resources.Designer.cs");
			StringAssert.DoesNotMatch(designer,new Regex(@"public static string (SharpDX|Tao_OpenGL)\b|""(SharpDX|Tao_OpenGL)"""),"Resources.Designer.cs");
			string project=TestPaths.ReadSource("OpenPmsDental","OpenPmsDental.csproj");
			StringAssert.DoesNotMatch(project,new Regex(@"SharpDX\.txt|Tao_OpenGL\.txt"),"OpenPmsDental.csproj");
		}
	}
}
