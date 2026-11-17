// Package S3b-2b of the .NET 10 move (docs/port-notes/S3b-2.md, part B; decision 14 of docs/specs/net10-plan.md): RDL <Code> blocks keep working
// on .NET 10. RdlEngine compiles a report's <Code> block with Roslyn's Visual Basic compiler (VbCodeCompiler) in place of CodeDom's
// VBCodeProvider, which throws on .NET; these tests run the reports of RdlCodeReport through the .NET 10 x64 engine and compare the text with what
// the .NET Framework 4.8 engine gave for the same reports (Fixtures\*.net48.txt, recorded by Record-Net48.ps1), byte for byte. The report
// designer's syntax check (RdlDesign's CodeCtl.CheckCode) uses the same compile. No database, no network, no window.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Text;
using System.Threading;
using fyiReporting.RDL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectFileTests;

namespace RdlCodeTests {
	[TestClass]
	public class RdlCodeBlockTests {
		private CultureInfo _cultureInfoOld;
		private string _currentDirectoryOld;

		///<summary>The records were made in en-US (the build PC's culture; Record-Net48.ps1 checks it). The engine loads the MySQL.NET provider's
		///code module by file name from the current folder, as the program's is its own folder.</summary>
		[TestInitialize]
		public void TestInitialize() {
			_cultureInfoOld=Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
			_currentDirectoryOld=Environment.CurrentDirectory;
			Environment.CurrentDirectory=AppDomain.CurrentDomain.BaseDirectory;
		}

		[TestCleanup]
		public void TestCleanup() {
			Thread.CurrentThread.CurrentCulture=_cultureInfoOld;
			Environment.CurrentDirectory=_currentDirectoryOld;
		}

		#region Helpers
		private static string ReadFixture(string relativePath) {
			string path=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Fixtures",relativePath);
			Assert.IsTrue(File.Exists(path),"Missing fixture: "+path);
			return File.ReadAllText(path,new UTF8Encoding(false));
		}

		private static string FixtureReport() {
			return RdlCodeReport.REPORT_RDL.Replace("@@CONNECTION@@",SecurityElement.Escape("Server=fixture.example.test;Database=none"));
		}

		///<summary>The first difference of two texts, for a failure message.</summary>
		private static string Difference(string expected,string actual) {
			int i=0;
			while(i<expected.Length && i<actual.Length && expected[i]==actual[i]) {
				i++;
			}
			int start=Math.Max(0,i-40);
			return "first difference at "+i+": expected ["+expected.Substring(start,Math.Min(expected.Length-start,120))+"], actual ["
				+actual.Substring(start,Math.Min(actual.Length-start,120))+"]";
		}

		private static void AssertEqualsRecord(string fileName,string actual) {
			string expected=ReadFixture(fileName);
			Assert.IsTrue(expected==actual,fileName+": "+Difference(expected,actual));
		}

		///<summary>The repository root: the first folder above the test assembly with LICENSE and NOTICE.</summary>
		private static string Root() {
			DirectoryInfo directoryInfo=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			while(directoryInfo!=null) {
				if(File.Exists(Path.Combine(directoryInfo.FullName,"LICENSE")) && File.Exists(Path.Combine(directoryInfo.FullName,"NOTICE"))) {
					return directoryInfo.FullName;
				}
				directoryInfo=directoryInfo.Parent;
			}
			Assert.Fail("Repository root not found above "+AppDomain.CurrentDomain.BaseDirectory);
			return null;
		}

		///<summary>RdlDesign's CodeCtl.CheckCode from RdlDesigner.dll in its build folder (this configuration's, else any): no test project
		///references the designer. Not built: BuildOutput's rule.</summary>
		private static MethodInfo GetDesignerCheckCode() {
			string folderBin=Path.Combine(Root(),"RdlDesign","bin");
			string configuration=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar)).Name;
			string path=Path.Combine(folderBin,configuration,"RdlDesigner.dll");
			if(!File.Exists(path) && Directory.Exists(folderBin)) {
				path=Directory.EnumerateFiles(folderBin,"RdlDesigner.dll",SearchOption.AllDirectories).FirstOrDefault()??path;
			}
			if(!File.Exists(path)) {
				BuildOutput.NotBuilt("RdlDesigner.dll is not built ("+path+"). Build ReportDesigner (x64) first.");
			}
			Assembly assembly=Assembly.LoadFrom(path);
			Type type=assembly.GetType("fyiReporting.RdlDesign.CodeCtl",true);
			MethodInfo methodInfo=type.GetMethod("CheckCode",BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public);
			Assert.IsNotNull(methodInfo,"CodeCtl.CheckCode");
			return methodInfo;
		}
		#endregion Helpers

		///<summary>The program's own output carries what VbCodeCompiler needs (S3b-4's handoff, S3b-5): RdlEngine.dll as this test's, and the two
		///Roslyn files at the versions RdlEngine references (decision 14), so a report's &lt;Code&gt; block compiles in the program as it does here.
		///Not built: BuildOutput's rule.</summary>
		[TestMethod]
		public void ProgramOutput_HasTheEngineAndTheRoslynFilesTheCompilerUses() {
			string configuration=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar)).Name;
			string folderProgram=Path.Combine(Root(),"OpenPmsDental","bin",configuration);
			if(!File.Exists(Path.Combine(folderProgram,"OpenPmsDental.dll"))) {
				BuildOutput.NotBuilt(@"OpenPmsDental\bin\"+configuration+" is not built. Build OpenDental.Local.slnf (x64) first.");
			}
			string pathEngine=typeof(RDLParser).Assembly.Location;
			CollectionAssert.AreEqual(File.ReadAllBytes(pathEngine),File.ReadAllBytes(Path.Combine(folderProgram,"RdlEngine.dll")),
				"The program's RdlEngine.dll is this test's.");
			foreach(AssemblyName assemblyNameReference in typeof(RDLParser).Assembly.GetReferencedAssemblies()
				.Where(x => x.Name=="Microsoft.CodeAnalysis" || x.Name=="Microsoft.CodeAnalysis.VisualBasic"))
			{
				string path=Path.Combine(folderProgram,assemblyNameReference.Name+".dll");
				Assert.IsTrue(File.Exists(path),assemblyNameReference.Name+".dll is not in the program's output.");
				Assert.AreEqual(assemblyNameReference.Version,AssemblyName.GetAssemblyName(path).Version,assemblyNameReference.Name);
			}
			Assert.AreEqual(2,typeof(RDLParser).Assembly.GetReferencedAssemblies().Count(x => x.Name.StartsWith("Microsoft.CodeAnalysis",StringComparison.Ordinal)),
				"RdlEngine references both Roslyn files.");
		}

		[TestMethod]
		public void Process_IsDotNet10_X64_WithTheNet10Engine() {
			Assert.IsTrue(Environment.Is64BitProcess,"A 64-bit process.");
			StringAssert.StartsWith(RuntimeInformation.FrameworkDescription,".NET 10.");
			TargetFrameworkAttribute targetFrameworkAttribute=typeof(RDLParser).Assembly.GetCustomAttribute<TargetFrameworkAttribute>();
			Assert.IsNotNull(targetFrameworkAttribute,"RdlEngine's TargetFramework attribute.");
			Assert.AreEqual(".NETCoreApp,Version=v10.0",targetFrameworkAttribute.FrameworkName);
		}

		[TestMethod]
		public void FixtureReport_CodeFunctions_PagesEqualTheNet48Engine_AndTheS3a4Record() {
			string text=RdlCodeReport.RenderToText(FixtureReport(),RdlCodeReport.FixtureRows());
			Assert.IsFalse(text.Contains(" message: "),"The report compiles and runs without engine messages: "+text);
			StringAssert.Contains(text,"Golden RDL <Code> report");
			StringAssert.Contains(text,"GOLDTEST, Ava");
			AssertEqualsRecord("rdl-code-report.net48.txt",text);
			AssertEqualsRecord(Path.Combine("golden","rdl-code-report.txt"),text);
		}

		[TestMethod]
		public void CodeBlockWithAnError_GivesTodaysMessage_WithTheLineInTheCodeElement() {
			string text=RdlCodeReport.RenderToText(RdlCodeReport.CodeOnlyReport(RdlCodeReport.CODE_ONE_ERROR),null);
			StringAssert.Contains(text,"parse message: Severity: 4 - Code element has 1 error(s).  Line numbers are relative to Code element.\r\n"
				+"Line 3 '  Return NoSuchThing(1)' : BC30451 ");
			AssertEqualsRecord("code-one-error.net48.txt",text);
		}

		///<summary>The one difference from the .NET Framework compile: after a syntax error, Roslyn 5.12 also reports the missing ")" (BC30198) where
		///the .NET Framework's vbc (14.8, Visual Basic 2012) reported only "Expression expected" (BC30201). Every error that vbc reported is there,
		///with the same line, line text, number and text, in the same order; the count in the first line counts the extra one.</summary>
		[TestMethod]
		public void CodeBlockWithThreeErrors_TodaysErrorsInOrder_PlusRoslynsMissingParenthesis() {
			string text=RdlCodeReport.RenderToText(RdlCodeReport.CodeOnlyReport(RdlCodeReport.CODE_THREE_ERRORS),null);
			string record=ReadFixture("code-three-errors.net48.txt");
			string lineExpressionExpected="Line 6 '  Return Foo2(' : BC30201 Expression expected.";
			StringAssert.Contains(record,"Code element has 3 error(s).");
			StringAssert.Contains(record,lineExpressionExpected+"\n");
			string expected=record.Replace("Code element has 3 error(s).","Code element has 4 error(s).")
				.Replace(lineExpressionExpected+"\n",lineExpressionExpected+"\r\nLine 6 '  Return Foo2(' : BC30198 ')' expected.\n");
			Assert.IsTrue(expected==text,Difference(expected,text));
		}

		[TestMethod]
		public void CodeBlockCallingUCaseUnqualified_IsStillAnError_NoProjectWideImports() {
			string text=RdlCodeReport.RenderToText(RdlCodeReport.CodeOnlyReport(RdlCodeReport.CODE_NO_VB_IMPORT),null);
			StringAssert.Contains(text,"BC30451 'UCase' is not declared.");
			AssertEqualsRecord("code-no-vb-import.net48.txt",text);
		}

		[TestMethod]
		public void CodeBlockCompileOptions_InferOff_CompareBinary_StrictOff_AsOnNetFramework() {
			string text=RdlCodeReport.RenderToText(RdlCodeReport.CodeOnlyReport(RdlCodeReport.CODE_OPTIONS),null);
			StringAssert.Contains(text,": abc|False|13\n");
			AssertEqualsRecord("code-options.net48.txt",text);
		}

		[TestMethod]
		public void VbCodeCompiler_LoadsTheAssemblyFromMemory_AndReportsErrorsInSourceOrder() {
			List<VbCodeError> listErrors;
			Assembly assembly=VbCodeCompiler.Compile("Imports System\r\nNamespace fyiReporting.vbgen\r\nPublic Class Probe\r\n"
				+"Public Function Twice(ByVal x As Integer) As Integer\r\nReturn x * 2\r\nEnd Function\r\nEnd Class\r\nEnd Namespace\r\n",out listErrors);
			Assert.AreEqual(0,listErrors.Count);
			Assert.IsNotNull(assembly);
			Assert.AreEqual("",assembly.Location,"Loaded from memory, no temporary file.");
			object instance=assembly.CreateInstance("fyiReporting.vbgen.Probe");
			Assert.AreEqual(42,instance.GetType().GetMethod("Twice").Invoke(instance,new object[] { 21 }));
			List<VbCodeError> listCheck=VbCodeCompiler.Check("Imports System\r\nNamespace N\r\nPublic Class C\r\nPublic Function F() As String\r\n"
				+"Return Foo2(\r\nEnd Function\r\nPublic Function G() As String\r\nReturn Foo1()\r\nEnd Function\r\nEnd Class\r\nEnd Namespace\r\n");
			//In the order of their place in the source, as vbc listed them (BC30198 is Roslyn's extra one after a syntax error).
			Assert.AreEqual("5 BC30451|5 BC30201|5 BC30198|8 BC30451",string.Join("|",listCheck.Select(x => x.Line+" "+x.ErrorNumber)));
		}

		[TestMethod]
		public void UnformattedDoubles_AreWrittenAsNetFrameworkWroteThem() {
			//.NET Framework's ToString(): 15 significant digits, no sign on zero. .NET's own would be 0.30000000000000004, 0.8999999999999999 and -0.
			string[][] arrayCases=new[] {
				new[] { "0.1 + 0.2","0.3" },
				new[] { "0.3 * 3","0.9" },
				new[] { "-0.0","0" },
				new[] { "1234.56 * 3","3703.68" },
				new[] { "1E+16","1E+16" },
				new[] { "1.0 / 3.0","0.333333333333333" },
			};
			foreach(string[] arrayCase in arrayCases) {
				string code="\nPublic Function Value() As Double\n  Return "+arrayCase[0]+"\nEnd Function\n";
				string text=RdlCodeReport.RenderToText(RdlCodeReport.CodeOnlyReport(code),null);
				Assert.AreEqual("=== page 1 ===\n1807,1807,36135,1807: "+arrayCase[1]+"\n",text,arrayCase[0]);
			}
		}

		[TestMethod]
		public void SqlOdbcOleDbProviders_CallTheCheckHook_ThenNameOpenPointO10() {
			List<string> listClassNames=new List<string>();
			Action<string,string,string> actionOld=RdlEngineConfig.CheckConnection;
			try {
				RdlEngineConfig.CheckConnection=(provider,className,connectionString) => listClassNames.Add(className);
				foreach(string provider in new[] { "SQL","ODBC","OLEDB" }) {
					NotSupportedException ex=Assert.ThrowsExactly<NotSupportedException>(() => RdlEngineConfig.GetConnection(provider,"Server=db.example.test"));
					StringAssert.Contains(ex.Message,"'"+provider+"'");
					StringAssert.Contains(ex.Message,"O-10");
				}
				//As before, the SQL provider makes no connection without a connection string.
				Assert.IsNull(RdlEngineConfig.GetConnection("SQL",""));
			}
			finally {
				RdlEngineConfig.CheckConnection=actionOld;
			}
			CollectionAssert.AreEqual(new[] { "System.Data.SqlClient.SqlConnection","System.Data.Odbc.OdbcConnection","System.Data.OleDb.OleDbConnection",
				"System.Data.SqlClient.SqlConnection" },listClassNames);
		}

		[TestMethod]
		public void DesignerSyntaxCheck_UsesTheSameCompile() {
			MethodInfo methodInfo=GetDesignerCheckCode();
			List<string> listErrors=(List<string>)methodInfo.Invoke(null,new object[] { RdlCodeReport.CODE_ONE_ERROR });
			CollectionAssert.AreEqual(new[] { "Ln 3- 'NoSuchThing' is not declared. It may be inaccessible due to its protection level." },listErrors);
			listErrors=(List<string>)methodInfo.Invoke(null,new object[] { RdlCodeReport.CODE_THREE_ERRORS });
			CollectionAssert.AreEqual(new[] {
				"Ln 3- 'Foo1' is not declared. It may be inaccessible due to its protection level.",
				"Ln 6- 'Foo2' is not declared. It may be inaccessible due to its protection level.",
				"Ln 6- Expression expected.",
				"Ln 6- ')' expected." },listErrors);
			string codeGood="\nPublic Function Value() As String\n  Return \"fine\"\nEnd Function\n";
			listErrors=(List<string>)methodInfo.Invoke(null,new object[] { codeGood });
			Assert.AreEqual(0,listErrors.Count,string.Join(" | ",listErrors));
		}
	}
}
