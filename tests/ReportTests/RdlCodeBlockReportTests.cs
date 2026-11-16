// Package S3b-2b of the .NET 10 move (docs/port-notes/S3b-2.md, part B; the S3b-2 row and decision 14 of docs/specs/net10-plan.md): a report's
// <Code> block compiles and runs (with Roslyn on .NET 10, CodeDom's VBCodeProvider on .NET Framework), the way the program renders reports: the
// S3a-4 fixture report, whose expressions call VB functions of its <Code> block, gives the pages of the S3a-4 record through the engine and
// builds without a message in the RdlViewer control; a <Code> block with an error logs that error with its line in the Code element. The reports
// and their data come from tests\RdlCodeTests\RdlCodeReport.cs (linked): the rows are read through a connection over a DataTable, so no database
// is asked. This file is compiled into ReportTests, which runs on .NET 10 once S3b-5 retargets it (ReportTests references the business layer,
// which is .NET 10 from S3b-3); tests\RdlCodeTests runs the same reports on .NET 10 now.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using fyiReporting.RDL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RdlCodeTests;

namespace ReportTests {
	[TestClass]
	public class RdlCodeBlockReportTests {
		private string _currentDirectoryOld;

		[TestInitialize]
		public void TestInitialize() {
			RdlReportDb.UseUsEnglish();
			//The engine loads the MySQL.NET provider's code module (MySqlConnector.dll) by file name from the current folder.
			_currentDirectoryOld=Environment.CurrentDirectory;
			Environment.CurrentDirectory=AppDomain.CurrentDomain.BaseDirectory;
		}

		[TestCleanup]
		public void TestCleanup() {
			Environment.CurrentDirectory=_currentDirectoryOld;
		}

		private static string FixtureReport() {
			return RdlCodeReport.REPORT_RDL.Replace("@@CONNECTION@@",SecurityElement.Escape("Server=fixture.example.test;Database=none"));
		}

		private static string ReadFixture(string relativePath) {
			string path=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"RdlCodeFixtures",relativePath);
			Assert.IsTrue(File.Exists(path),"Missing fixture: "+path);
			return File.ReadAllText(path,new UTF8Encoding(false));
		}

		[TestMethod]
		public void RdlCodeReport_Engine_PagesEqualTheS3a4Record() {
			string text=RdlCodeReport.RenderToText(FixtureReport(),RdlCodeReport.FixtureRows());
			Assert.IsFalse(text.Contains(" message: "),"The report compiles and runs without engine messages: "+text);
			Assert.AreEqual(ReadFixture(Path.Combine("golden","rdl-code-report.txt")),text);
			Assert.AreEqual(ReadFixture("rdl-code-report.net48.txt"),text);
		}

		[TestMethod]
		public void RdlCodeReport_Viewer_BuildsOnePageWithoutMessages() {
			DataTable table=RdlCodeReport.FixtureRows();
			Func<string,string,string,IDbConnection> funcCreateOld=RdlEngineConfig.CreateConnection;
			RdlRenderResult result;
			try {
				RdlEngineConfig.CreateConnection=(provider,className,connectionString) => new FixtureConnection(table);
				result=RdlRender.RenderWithViewer(FixtureReport());
			}
			finally {
				RdlEngineConfig.CreateConnection=funcCreateOld;
			}
			Assert.AreEqual(1,result.PageCount,result.Describe());
			Assert.AreEqual(0,result.ListRunMessages.Count,result.Describe());
		}

		[TestMethod]
		public void CodeBlockWithAnError_Engine_LogsTheErrorWithItsLineInTheCodeElement() {
			RdlRenderResult result=RdlRender.RenderWithEngine(RdlCodeReport.CodeOnlyReport(RdlCodeReport.CODE_ONE_ERROR));
			string message=result.ListParseMessages.FirstOrDefault(x => x.Contains("Code element has"));
			Assert.IsNotNull(message,result.Describe());
			Assert.AreEqual("Severity: 4 - Code element has 1 error(s).  Line numbers are relative to Code element.\r\n"
				+"Line 3 '  Return NoSuchThing(1)' : BC30451 'NoSuchThing' is not declared. It may be inaccessible due to its protection level.",message);
			//As the .NET Framework engine logged it (tests\RdlCodeTests\Fixtures\code-one-error.net48.txt).
			StringAssert.Contains(ReadFixture("code-one-error.net48.txt"),"parse message: "+message+"\n");
		}
	}
}
