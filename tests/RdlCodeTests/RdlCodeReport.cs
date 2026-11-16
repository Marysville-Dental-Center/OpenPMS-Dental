// Package S3b-2b of the .NET 10 move (docs/port-notes/S3b-2.md, part B; decision 14 of docs/specs/net10-plan.md): runs RDL reports whose
// <Code> blocks RdlEngine compiles, without a database, and writes what the engine gives as text. The same file is compiled into the .NET 10
// test assembly (RdlCodeTests) and, by tests\RdlCodeTests\Record-Net48.ps1, into Windows PowerShell 5.1 against the .NET Framework 4.8 build of
// RdlEngine, which compiled a <Code> block with CodeDom's VBCodeProvider; that script recorded the files under Fixtures\. So it is written in the
// C# 5 that PowerShell's Add-Type compiles (no ?., no string interpolation, no pattern matching).
// The report is the fixture report of S3a-4 (tests\GoldenOutputTests\RdlCodeReportGoldenTests.cs, REPORT_RDL, copied unchanged), and its rows
// are the rows that report reads from the golden fixture database, read by the engine through a connection over a DataTable (FixtureConnection,
// handed to the engine by the host's RdlEngineConfig.CreateConnection hook, as the program hands it its MySqlConnector connections), so no
// database is asked. The text is in the format of that test (RenderToText), so the page text equals its record,
// tests\GoldenOutputTests\Fixtures\rdl-code-report.txt.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Text;
using fyiReporting.RDL;

namespace RdlCodeTests {
	public static class RdlCodeReport {
		///<summary>The fixture report of S3a-4, as tests\GoldenOutputTests\RdlCodeReportGoldenTests.cs has it (REPORT_RDL). The connection string
		///(@@CONNECTION@@) is never used here: the data comes from FixtureRows.</summary>
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

		///<summary>A <Code> block with one compile error, on line 3 of the element (line 1 is the empty rest of the line of the start tag): BC30451.
		///</summary>
		public const string CODE_ONE_ERROR=@"
Public Function Value() As String
  Return NoSuchThing(1)
End Function
";

		///<summary>A <Code> block with three compile errors on two lines (3 and 6 of the element), two of them on one line: their order and count.
		///</summary>
		public const string CODE_THREE_ERRORS=@"
Public Function Value() As String
  Return Foo1()
End Function
Public Function Other() As String
  Return Foo2(
End Function
";

		///<summary>A <Code> block that calls UCase unqualified: today's compile has no project-wide import of Microsoft.VisualBasic (the generated
		///source imports System only), so this is a compile error (BC30451) there and must stay one.</summary>
		public const string CODE_NO_VB_IMPORT=@"
Public Function Value() As String
  Return UCase(""a"")
End Function
";

		///<summary>A <Code> block whose result shows today's compile options: Option Infer off (x is an Object, so it takes a string; with Infer on
		///it would be an Integer and the assignment would throw), Option Compare Binary (""a"" = ""A"" is False), Option Strict off (a string
		///converts to an Integer implicitly). Today: abc|False|13.</summary>
		public const string CODE_OPTIONS=@"
Public Function Value() As String
  Dim x = 5
  x = ""abc""
  Dim n As Integer = ""12""
  Return CStr(x) &amp; ""|"" &amp; (""a"" = ""A"").ToString() &amp; ""|"" &amp; CStr(n + 1)
End Function
";

		///<summary>A small report around a <Code> block: its page header shows =Code.Value(). The block is put in as it is (XML-escaped where
		///needed).</summary>
		public static string CodeOnlyReport(string codeBlock) {
			return @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Report Name=""CodeOnly"">
  <PageHeight>11in</PageHeight>
  <PageWidth>8.5in</PageWidth>
  <Width>7.5in</Width>
  <TopMargin>.25in</TopMargin>
  <LeftMargin>.25in</LeftMargin>
  <RightMargin>.25in</RightMargin>
  <BottomMargin>.25in</BottomMargin>
  <Code>"+codeBlock+@"</Code>
  <PageHeader>
    <Height>.3in</Height>
    <PrintOnFirstPage>true</PrintOnFirstPage>
    <PrintOnLastPage>true</PrintOnLastPage>
    <ReportItems>
      <Textbox Name=""TextboxValue"">
        <Top>0in</Top>
        <Left>0in</Left>
        <Width>5in</Width>
        <Height>.25in</Height>
        <Value>=Code.Value()</Value>
      </Textbox>
    </ReportItems>
  </PageHeader>
  <Body>
    <Height>1in</Height>
  </Body>
</Report>";
		}

		///<summary>The rows the fixture report's query returns from the golden fixture database (tests\GoldenOutputTests\GoldenData.cs: Patient1
		///GOLDTEST Ava with Z9101 57.25, Z9102 0.1+0.2 and Z9103 1234.56; Patient2 GOLDZERO Ben with Z9104 0.10), in ProcNum order, with the
		///types MySqlConnector gives (strings, and a double for the double column procedurelog.ProcFee).</summary>
		public static DataTable FixtureRows() {
			DataTable table=new DataTable("Data");
			table.Columns.Add("LName",typeof(string));
			table.Columns.Add("FName",typeof(string));
			table.Columns.Add("ProcCode",typeof(string));
			table.Columns.Add("ProcFee",typeof(double));
			table.Rows.Add("GOLDTEST","Ava","Z9101",57.25);
			table.Rows.Add("GOLDTEST","Ava","Z9102",0.1+0.2);
			table.Rows.Add("GOLDTEST","Ava","Z9103",1234.56);
			table.Rows.Add("GOLDZERO","Ben","Z9104",0.10);
			return table;
		}

		///<summary>Parses and runs the report as RdlViewer does (RDLParser.Parse, Report.RunGetData, Report.BuildPages). With a table, every
		///connection the engine asks the host for (RdlEngineConfig.CreateConnection, the hook through which the program makes its MySqlConnector
		///connections) is a FixtureConnection over the table, for the column types while the report is parsed and for the rows while it runs; the
		///engine still loads the provider's code module (MySqlConnector.dll) from the current folder first, as in the program. The text: engine
		///messages first ("parse message:", "run message:"), then "=== page n ===" and one line per text item in page order, "x,y,w,h: text" with
		///the position and size in hundredths of a point, as RdlCodeReportGoldenTests.RenderToText writes it.</summary>
		public static string RenderToText(string rdl,DataTable table) {
			Func<string,string,string,IDbConnection> funcCreateOld=RdlEngineConfig.CreateConnection;
			try {
				if(table!=null) {
					RdlEngineConfig.CreateConnection=delegate(string provider,string className,string connectionString) {
						return new FixtureConnection(table);
					};
				}
				return RenderToTextInner(rdl);
			}
			finally {
				RdlEngineConfig.CreateConnection=funcCreateOld;
			}
		}

		private static string RenderToTextInner(string rdl) {
			StringBuilder stringBuilder=new StringBuilder();
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
					PageText pageText=pageItem as PageText;
					if(pageText==null) {
						continue;
					}
					stringBuilder.Append(Hundredths(pageText.X)+","+Hundredths(pageText.Y)+","+Hundredths(pageText.W)+","+Hundredths(pageText.H)+": "
						+(pageText.Text??"")+"\n");
				}
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
				stringBuilder.Append(stage+" message: "+(message==null ? "" : message.ToString())+"\n");
			}
		}
	}


	///<summary>A connection over a DataTable in place of the fixture database: every command it makes returns a reader over the table's rows, with
	///the table's column names and types (DataTable.CreateDataReader), whatever its text. It opens and closes without a server.</summary>
	public class FixtureConnection : IDbConnection {
		private readonly DataTable _table;
		private string _connectionString="";
		private ConnectionState _connectionState=ConnectionState.Closed;

		public FixtureConnection(DataTable table) {
			_table=table;
		}

		public string ConnectionString {
			get { return _connectionString; }
			set { _connectionString=value; }
		}

		public int ConnectionTimeout {
			get { return 0; }
		}

		public string Database {
			get { return ""; }
		}

		public ConnectionState State {
			get { return _connectionState; }
		}

		public IDbTransaction BeginTransaction() {
			throw new NotSupportedException("The fixture connection has no transactions.");
		}

		public IDbTransaction BeginTransaction(IsolationLevel il) {
			throw new NotSupportedException("The fixture connection has no transactions.");
		}

		public void ChangeDatabase(string databaseName) {
			throw new NotSupportedException("The fixture connection has no databases.");
		}

		public void Close() {
			_connectionState=ConnectionState.Closed;
		}

		public IDbCommand CreateCommand() {
			return new FixtureCommand(this,_table);
		}

		public void Open() {
			_connectionState=ConnectionState.Open;
		}

		public void Dispose() {
			Close();
		}
	}

	///<summary>The command of a FixtureConnection: its reader is the table's.</summary>
	public class FixtureCommand : IDbCommand {
		private readonly IDbConnection _connection;
		private readonly DataTable _table;
		private string _commandText="";
		private int _commandTimeout;
		private CommandType _commandType=CommandType.Text;

		public FixtureCommand(IDbConnection connection,DataTable table) {
			_connection=connection;
			_table=table;
		}

		public string CommandText {
			get { return _commandText; }
			set { _commandText=value; }
		}

		public int CommandTimeout {
			get { return _commandTimeout; }
			set { _commandTimeout=value; }
		}

		public CommandType CommandType {
			get { return _commandType; }
			set { _commandType=value; }
		}

		public IDbConnection Connection {
			get { return _connection; }
			set { throw new NotSupportedException("The fixture command keeps its connection."); }
		}

		///<summary>The fixture report has no query parameters, so the engine never asks for them.</summary>
		public IDataParameterCollection Parameters {
			get { throw new NotSupportedException("The fixture command has no parameters."); }
		}

		public IDbTransaction Transaction {
			get { return null; }
			set { }
		}

		public UpdateRowSource UpdatedRowSource {
			get { return UpdateRowSource.None; }
			set { }
		}

		public void Cancel() {
		}

		public IDbDataParameter CreateParameter() {
			throw new NotSupportedException("The fixture command has no parameters.");
		}

		public int ExecuteNonQuery() {
			throw new NotSupportedException("The fixture command only reads.");
		}

		public IDataReader ExecuteReader() {
			return _table.CreateDataReader();
		}

		public IDataReader ExecuteReader(CommandBehavior behavior) {
			return _table.CreateDataReader();
		}

		public object ExecuteScalar() {
			throw new NotSupportedException("The fixture command only reads.");
		}

		public void Prepare() {
		}

		public void Dispose() {
		}
	}
}
