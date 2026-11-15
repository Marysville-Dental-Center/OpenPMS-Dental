// Golden outputs: the statement PDF's text (the statement sheet filled and drawn by SheetDrawingJob.CreatePdf, as the statement window and the
// billing batch make it) for the first family and for the second family's limited statement, whose total is a sum of doubles of about -1.1e-16
// (-0.80+0.10+0.70, docs/port-notes/S3a-3.md section 4); and the statement .csv export (Statements.SaveStatementAsCSV). Recorded with the .NET
// Framework build (docs/port-notes/S3a-4.md).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using PdfLibraryTests;
using PdfSharp.Pdf;

namespace GoldenOutputTests {
	[TestClass]
	public class StatementGoldenTests {
		[TestInitialize]
		public void TestInitialize() {
			GoldenDb.Begin();
			PdfSharpSetup.Init();
		}

		///<summary>The statement as the program makes it: the statement sheet, filled, made into a PDF (as StatementPdfDbTests of LIB-PDF).</summary>
		private static PdfDocument CreateStatementPdf(Statement statement) {
			DataSet dataSet=AccountModules.GetAccount(Statements.GetPatNumForGetAccount(statement),statement,doShowHiddenPaySplits:statement.IsReceipt);
			SheetDef sheetDef=SheetUtil.GetStatementSheetDef(statement);
			Sheet sheet=SheetUtil.CreateSheet(sheetDef,statement.PatNum,statement.HidePayment);
			sheet.Parameters.Add(new SheetParameter(true,"Statement") { ParamValue=statement });
			SheetFiller.FillFields(sheet,dataSet,statement);
			SheetUtil.CalculateHeights(sheet,dataSet,statement);
			return new SheetDrawingJob().CreatePdf(sheet,statement,null,dataSet,null,null);
		}

		///<summary>The text of every page, read back from the saved PDF with PdfReader: one line per text-showing operator, each page after a line
		///"=== page n ===". No normalisation: the statement's dates are the fixture's.</summary>
		public static string GetPdfText(PdfDocument pdfDocument) {
			PdfDocument pdfDocumentRead=PdfRead.Open(PdfRead.Save(pdfDocument));
			StringBuilder stringBuilder=new StringBuilder();
			for(int i=0;i<pdfDocumentRead.PageCount;i++) {
				stringBuilder.Append("=== page "+(i+1)+" ===\n");
				stringBuilder.Append(PdfRead.ExtractText(pdfDocumentRead.Pages[i]));
			}
			return stringBuilder.ToString();
		}

		[TestMethod]
		public void StatementPdf_Family_Text() {
			Statement statement=Statements.GetStatement(GoldenData.StatementNumFamily);
			string text=GetPdfText(CreateStatementPdf(statement));
			StringAssert.Contains(text,"Goldtest");
			Golden.AssertText("statement-family.pdf.txt",text);
		}

		///<summary>The limited statement of the second family: a procedure of 0.10, an adjustment of 0.70 and a payment of 0.80. Its total is a sum of
		///doubles of about -1.1e-16, which .NET Framework writes "$0.00" (SheetFiller.GetMoneyText).</summary>
		[TestMethod]
		public void StatementPdf_LimitedNearZeroTotal_Text() {
			Statement statement=Statements.GetStatement(GoldenData.StatementNumLimited);
			Assert.AreEqual(StmtType.LimitedStatement,statement.StatementType);
			string text=GetPdfText(CreateStatementPdf(statement));
			StringAssert.Contains(text,"Goldzero");
			StringAssert.Contains(text,"$0.00");
			Assert.IsFalse(text.Contains("-$0.00"),"A total that rounds to zero has no sign.");
			Golden.AssertText("statement-limited-near-zero.pdf.txt",text);
		}

		///<summary>The .csv export of a statement (Statements.WriteStatementToCSV, which SaveStatementAsCSV calls with the patient's image folder). The
		///file is written to a temporary folder here; its name and folder are not part of the output.</summary>
		private static byte[] GetStatementCsv(Statement statement) {
			MethodInfo methodInfo=typeof(Statements).GetMethod("WriteStatementToCSV",BindingFlags.Static|BindingFlags.NonPublic);
			Assert.IsNotNull(methodInfo,"Statements.WriteStatementToCSV");
			string folder=Path.Combine(Path.GetTempPath(),"GoldenOutputTests_"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(folder);
			try {
				string path=(string)methodInfo.Invoke(null,new object[] { statement,"statement.csv",folder });
				return File.ReadAllBytes(path);
			}
			finally {
				try {
					Directory.Delete(folder,true);
				}
				catch(Exception) {
				}
			}
		}

		///<summary>The family statement with the guarantor's balance and insurance estimate, as Statements.cs fills them for a new statement.</summary>
		[TestMethod]
		public void StatementCsv_Family() {
			Statement statement=Statements.GetStatement(GoldenData.StatementNumFamily);
			Patient patientGuarantor=Patients.GetPat(GoldenData.Patient1.Guarantor);
			statement.BalTotal=patientGuarantor.BalTotal;
			statement.InsEst=patientGuarantor.InsEst;
			Golden.AssertBytes("statement-family.csv",GetStatementCsv(statement));
		}

		///<summary>The limited statement with balances computed in doubles, as FormStatementOptions sums a super family's guarantor balances and
		///insurance estimates (0.10+0.20 and 0.70+0.10): the export writes them with the double's default text.</summary>
		[TestMethod]
		public void StatementCsv_LimitedWithComputedBalances() {
			Statement statement=Statements.GetStatement(GoldenData.StatementNumLimited);
			statement.BalTotal=new[] { 0.10,0.20 }.Sum();
			statement.InsEst=new[] { 0.70,0.10 }.Sum();
			Golden.AssertBytes("statement-limited-computed.csv",GetStatementCsv(statement));
		}
	}
}
