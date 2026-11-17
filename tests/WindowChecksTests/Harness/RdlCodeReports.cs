// The reports with a VB <Code> block that G4, G5 and H3 use (WINCHK-5 fix, the stage 3b item "an RDL report with a VB <Code> function;
// RdlDesign ... checks a <Code> block" of docs/specs/net10-plan.md section 6). Synthetic: the data set reads the scratch copy's patients
// through the program's own report connection (ODR's DataConnection, as the shipped reports do); nothing is written.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.

namespace WindowChecksTests {
	public static class RdlCodeReports {
		///<summary>The menu item (and file name without .rdl) of the report with the three VB functions.</summary>
		public const string NAME_CODE="Window checks Code";
		///<summary>The menu item (and file name without .rdl) of the report whose Code block does not compile.</summary>
		public const string NAME_CODE_ERROR="Window checks Code error";
		///<summary>The page header's text, from Code.Title().</summary>
		public const string TITLE="Window checks: a report with VB Code functions";
		///<summary>The query of the report's data set; G4 runs it too, for the texts the functions must give.</summary>
		public const string QUERY="SELECT LName,FName,PatNum FROM patient ORDER BY PatNum LIMIT 3";
		///<summary>The name the broken Code block uses without declaring it: the compile error (BC30451) names it.</summary>
		public const string NAME_UNDECLARED="NotDeclaredInTheCode";

		///<summary>The report's Code block as RdlDesigner's Code tab shows it (H3 compares it).</summary>
		public const string CODE_BLOCK=@"
Public Function Title() As String
  Return ""Window checks: a report with VB Code functions""
End Function

Public Function PatientName(ByVal lName As String, ByVal fName As String) As String
  Return lName.ToUpperInvariant() & "", "" & fName
End Function

Public Function Triple(ByVal value As Double) As Double
  Return value * 3
End Function
";

		///<summary>A report on the patient table: the page header shows =Code.Title(), each row =Code.PatientName(LName, FName) and
		///=Code.Triple(PatNum).</summary>
		public static string ReportCode() {
			return @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Report Name="""">
  <CodeModules>
    <CodeModule>ODR.dll</CodeModule>
  </CodeModules>
  <PageHeight>11in</PageHeight>
  <PageWidth>8.5in</PageWidth>
  <Width>7.5in</Width>
  <TopMargin>.25in</TopMargin>
  <LeftMargin>.25in</LeftMargin>
  <RightMargin>.25in</RightMargin>
  <BottomMargin>.25in</BottomMargin>
  <Code>"+System.Security.SecurityElement.Escape(CODE_BLOCK)+@"</Code>
  <DataSources>
    <DataSource Name=""DS1"">
      <ConnectionProperties>
        <DataProvider>MySQL.NET</DataProvider>
        <ConnectString>=ODR.DataConnection.GetODConnStr()</ConnectString>
        <IntegratedSecurity>false</IntegratedSecurity>
      </ConnectionProperties>
    </DataSource>
  </DataSources>
  <DataSets>
    <DataSet Name=""Data"">
      <Query>
        <DataSourceName>DS1</DataSourceName>
        <Timeout>10</Timeout>
        <CommandText>"+QUERY+@"</CommandText>
      </Query>
      <Fields>
        <Field Name=""LName""><DataField>LName</DataField></Field>
        <Field Name=""FName""><DataField>FName</DataField></Field>
        <Field Name=""PatNum""><DataField>PatNum</DataField></Field>
      </Fields>
    </DataSet>
  </DataSets>
  <PageHeader>
    <Height>.4in</Height>
    <PrintOnFirstPage>true</PrintOnFirstPage>
    <PrintOnLastPage>true</PrintOnLastPage>
    <ReportItems>
      <Textbox Name=""TextboxTitle"">
        <Top>.05in</Top>
        <Left>0in</Left>
        <Width>6in</Width>
        <Height>.3in</Height>
        <Value>=Code.Title()</Value>
        <Style><FontSize>12pt</FontSize><FontWeight>Bold</FontWeight></Style>
      </Textbox>
    </ReportItems>
  </PageHeader>
  <Body>
    <Height>2in</Height>
    <ReportItems>
      <Table Name=""Table1"">
        <DataSetName>Data</DataSetName>
        <NoRows>No rows</NoRows>
        <Style><BorderStyle><Default>Solid</Default></BorderStyle></Style>
        <TableColumns>
          <TableColumn><Width>3in</Width></TableColumn>
          <TableColumn><Width>2in</Width></TableColumn>
        </TableColumns>
        <Header>
          <TableRows>
            <TableRow>
              <Height>.25in</Height>
              <TableCells>
                <TableCell><ReportItems><Textbox Name=""HeaderPatient""><Value>Patient (Code.PatientName)</Value><Style><FontWeight>Bold</FontWeight></Style></Textbox></ReportItems></TableCell>
                <TableCell><ReportItems><Textbox Name=""HeaderTriple""><Value>PatNum times 3 (Code.Triple)</Value><Style><FontWeight>Bold</FontWeight></Style></Textbox></ReportItems></TableCell>
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
                <TableCell><ReportItems><Textbox Name=""DetailTriple""><Value>=Code.Triple(Fields!PatNum.Value)</Value></Textbox></ReportItems></TableCell>
              </TableCells>
            </TableRow>
          </TableRows>
        </Details>
      </Table>
    </ReportItems>
  </Body>
</Report>
";
		}

		///<summary>A report whose Code block uses a name it never declares (compile error BC30451); the page header shows =Code.Value().</summary>
		public static string ReportCodeError() {
			return @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Report Name="""">
  <PageHeight>11in</PageHeight>
  <PageWidth>8.5in</PageWidth>
  <Width>7.5in</Width>
  <TopMargin>.25in</TopMargin>
  <LeftMargin>.25in</LeftMargin>
  <RightMargin>.25in</RightMargin>
  <BottomMargin>.25in</BottomMargin>
  <Code>
Public Function Value() As String
  Return "+NAME_UNDECLARED+@"
End Function
</Code>
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
</Report>
";
		}
	}
}
