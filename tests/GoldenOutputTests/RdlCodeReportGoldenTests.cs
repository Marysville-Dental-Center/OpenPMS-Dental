// Golden output: a fixture RDL report whose expressions call VB functions of its <Code> block, run against the fixture database by RdlEngine (as
// RdlViewer runs a report: RDLParser.Parse, Report.RunGetData, Report.BuildPages), recorded with the .NET Framework build, where the engine compiles
// the <Code> block with CodeDom's VBCodeProvider (docs/port-notes/S3a-4.md; the plan's decision 14). Stage 3b compiles it with Roslyn instead
// (S3b-2) and must give the same pages.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using fyiReporting.RDL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoldenOutputTests {
	[TestClass]
	public class RdlCodeReportGoldenTests {
		///<summary>The fixture report. Its <Code> block has a function for each kind of value: a string built from two fields, a double computed from
		///a field (no Format, so the engine writes the double's own text), a string formatted inside the VB code, and a total of the doubles. The
		///connection string is filled in at run time (@@CONNECTION@@); it is input, not output.</summary>
		public const string REPORT_RDL=@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Report Name=""GoldenCode"">
  <PageHeight>11in</PageHeight>
  <PageWidth>8.5in</PageWidth>
  <Width>7.5in</Width>
  <TopMargin>.25in</TopMargin>
  <LeftMargin>.25in</LeftMargin>
  <RightMargin>.25in</RightMargin>
  <BottomMargin>.25in</BottomMargin>
  <Code>
Public Function PatientName(ByVal lName As String, ByVal fName As String) As String
  Return lName.ToUpperInvariant() &amp; "", "" &amp; fName
End Function

Public Function Triple(ByVal fee As Double) As Double
  Return fee * 3
End Function

Public Function Money(ByVal fee As Double) As String
  Return fee.ToString(""F2"", System.Globalization.CultureInfo.InvariantCulture)
End Function

Public Function Title() As String
  Return ""Golden RDL &lt;Code&gt; report""
End Function
  </Code>
  <DataSources>
    <DataSource Name=""DS1"">
      <ConnectionProperties>
        <DataProvider>MySQL.NET</DataProvider>
        <ConnectString>@@CONNECTION@@</ConnectString>
        <IntegratedSecurity>false</IntegratedSecurity>
      </ConnectionProperties>
    </DataSource>
  </DataSources>
  <DataSets>
    <DataSet Name=""Data"">
      <Query>
        <DataSourceName>DS1</DataSourceName>
        <Timeout>30</Timeout>
        <CommandText>SELECT patient.LName,patient.FName,procedurecode.ProcCode,procedurelog.ProcFee
FROM procedurelog
INNER JOIN patient ON patient.PatNum=procedurelog.PatNum
INNER JOIN procedurecode ON procedurecode.CodeNum=procedurelog.CodeNum
WHERE procedurecode.ProcCode LIKE 'Z91%'
ORDER BY procedurelog.ProcNum</CommandText>
      </Query>
      <Fields>
        <Field Name=""LName""><DataField>LName</DataField></Field>
        <Field Name=""FName""><DataField>FName</DataField></Field>
        <Field Name=""ProcCode""><DataField>ProcCode</DataField></Field>
        <Field Name=""ProcFee""><DataField>ProcFee</DataField></Field>
      </Fields>
    </DataSet>
  </DataSets>
  <PageHeader>
    <Height>.3in</Height>
    <PrintOnFirstPage>true</PrintOnFirstPage>
    <PrintOnLastPage>true</PrintOnLastPage>
    <ReportItems>
      <Textbox Name=""TextboxTitle"">
        <Top>0in</Top>
        <Left>0in</Left>
        <Width>5in</Width>
        <Height>.25in</Height>
        <Value>=Code.Title()</Value>
      </Textbox>
    </ReportItems>
  </PageHeader>
  <Body>
    <Height>2in</Height>
    <ReportItems>
      <Table Name=""Table1"">
        <DataSetName>Data</DataSetName>
        <NoRows>No rows</NoRows>
        <TableColumns>
          <TableColumn><Width>2in</Width></TableColumn>
          <TableColumn><Width>1in</Width></TableColumn>
          <TableColumn><Width>1.5in</Width></TableColumn>
          <TableColumn><Width>1.5in</Width></TableColumn>
        </TableColumns>
        <Header>
          <TableRows>
            <TableRow>
              <Height>.25in</Height>
              <TableCells>
                <TableCell><ReportItems><Textbox Name=""HeaderPatient""><Value>Patient</Value></Textbox></ReportItems></TableCell>
                <TableCell><ReportItems><Textbox Name=""HeaderCode""><Value>Code</Value></Textbox></ReportItems></TableCell>
                <TableCell><ReportItems><Textbox Name=""HeaderTriple""><Value>Fee times 3</Value></Textbox></ReportItems></TableCell>
                <TableCell><ReportItems><Textbox Name=""HeaderMoney""><Value>Fee</Value></Textbox></ReportItems></TableCell>
              </TableCells>
            </TableRow>
          </TableRows>
        </Header>
        <Details>
          <TableRows>
            <TableRow>
              <Height>.25in</Height>
              <TableCells>
                <TableCell><ReportItems><Textbox Name=""DetailPatient""><Value>=Code.PatientName(Fields!LName.Value, Fields!FName.Value)</Value></Textbox></ReportItems></TableCell>
                <TableCell><ReportItems><Textbox Name=""DetailCode""><Value>=Fields!ProcCode.Value</Value></Textbox></ReportItems></TableCell>
                <TableCell><ReportItems><Textbox Name=""DetailTriple""><Value>=Code.Triple(Fields!ProcFee.Value)</Value></Textbox></ReportItems></TableCell>
                <TableCell><ReportItems><Textbox Name=""DetailMoney""><Value>=Code.Money(Fields!ProcFee.Value)</Value></Textbox></ReportItems></TableCell>
              </TableCells>
            </TableRow>
          </TableRows>
        </Details>
        <Footer>
          <TableRows>
            <TableRow>
              <Height>.25in</Height>
              <TableCells>
                <TableCell><ReportItems><Textbox Name=""FooterLabel""><Value>Total</Value></Textbox></ReportItems></TableCell>
                <TableCell><ReportItems><Textbox Name=""FooterBlank""><Value></Value></Textbox></ReportItems></TableCell>
                <TableCell><ReportItems><Textbox Name=""FooterTriple""><Value>=Code.Triple(Sum(Fields!ProcFee.Value))</Value></Textbox></ReportItems></TableCell>
                <TableCell><ReportItems><Textbox Name=""FooterMoney""><Value>=Code.Money(Sum(Fields!ProcFee.Value))</Value></Textbox></ReportItems></TableCell>
              </TableCells>
            </TableRow>
          </TableRows>
        </Footer>
      </Table>
    </ReportItems>
  </Body>
</Report>";

		[TestInitialize]
		public void TestInitialize() {
			GoldenDb.Begin();
		}

		///<summary>The pages as text: "=== page n ===", then one line per text item in page order, "x,y,w,h: text" with the position and size in
		///hundredths of a point (rounded integers, so no float formatting is involved). Engine messages, if any, come first.</summary>
		public static string RenderToText(string rdl) {
			StringBuilder stringBuilder=new StringBuilder();
			string currentDirectory=Environment.CurrentDirectory;
			//RdlEngine loads the MySqlConnector data provider by file name from the current folder, as the program's is its own folder.
			Environment.CurrentDirectory=AppDomain.CurrentDomain.BaseDirectory;
			try {
				RDLParser rdlParser=new RDLParser(rdl);
				rdlParser.Folder=AppDomain.CurrentDomain.BaseDirectory;
				Report report=rdlParser.Parse();
				AppendMessages(stringBuilder,"parse",report.ErrorItems);
				report.ErrorReset();
				report.RunGetData(new ListDictionary());
				Pages pages=report.BuildPages();
				AppendMessages(stringBuilder,"run",report.ErrorItems);
				int pageNumber=0;
				foreach(Page page in pages) {
					pageNumber++;
					stringBuilder.Append("=== page "+pageNumber+" ===\n");
					foreach(PageItem pageItem in page) {
						if(!(pageItem is PageText pageText)) {
							continue;
						}
						stringBuilder.Append(Hundredths(pageText.X)+","+Hundredths(pageText.Y)+","+Hundredths(pageText.W)+","+Hundredths(pageText.H)+": "
							+(pageText.Text??"")+"\n");
					}
				}
			}
			finally {
				Environment.CurrentDirectory=currentDirectory;
			}
			return stringBuilder.ToString();
		}

		private static string Hundredths(float value) {
			return ((long)Math.Round((double)value*100,MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);
		}

		private static void AppendMessages(StringBuilder stringBuilder,string stage,IList listMessages) {
			if(listMessages==null) {
				return;
			}
			foreach(object message in listMessages) {
				stringBuilder.Append(stage+" message: "+(message?.ToString()??"")+"\n");
			}
		}

		[TestMethod]
		public void RdlReport_VbCodeFunctions_Pages() {
			string rdl=REPORT_RDL.Replace("@@CONNECTION@@",SecurityElement.Escape(GoldenDb.GetConnectionString()));
			string text=RenderToText(rdl);
			Assert.IsFalse(text.Contains(" message: "),"The report compiles and runs without engine messages: "+text);
			StringAssert.Contains(text,"Golden RDL <Code> report");
			StringAssert.Contains(text,"GOLDTEST, Ava");
			Golden.AssertText("rdl-code-report.txt",text);
		}
	}
}
