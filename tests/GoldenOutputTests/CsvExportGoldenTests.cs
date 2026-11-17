// Golden outputs: the main .csv exports, recorded with the .NET Framework build (docs/port-notes/S3a-4.md). The statement .csv is in
// StatementGoldenTests. Here: the Chart of Accounts export (Setup > Chart of Accounts > Export .csv: Accounts.GetFullList through the grid export
// shared by every grid window since 25.3, GridODExport, written as the window writes it) and the UDS+ patient-level file (RpUdsPlus.ToCsv).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace GoldenOutputTests {
	[TestClass]
	public class CsvExportGoldenTests {
		[TestInitialize]
		public void TestInitialize() {
			GoldenDb.Begin();
		}

		///<summary>The bytes GridODExport.WriteFile writes for the text (UTF-8 with a byte order mark), through a temporary file.</summary>
		private static byte[] WriteThroughFile(string text) {
			string path=Path.Combine(Path.GetTempPath(),"GoldenOutputTests_"+Guid.NewGuid().ToString("N")+".csv");
			try {
				GridODExport.WriteFile(path,text);
				return File.ReadAllBytes(path);
			}
			finally {
				File.Delete(path);
			}
		}

		///<summary>The Chart of Accounts as of the fixture's statement date, inactive accounts shown, with the grid's columns and cells as
		///FormAccounting.FillGrid fills them (headings through Lans in en-US).</summary>
		[TestMethod]
		public void CsvExport_ChartOfAccounts() {
			DataTable table=Accounts.GetFullList(GoldenData.DateStatement,true);
			List<string> listHeadings=new List<string> { Lans.g("TableChartOfAccounts","Type"),Lans.g("TableChartOfAccounts","Description"),
				Lans.g("TableChartOfAccounts","Balance"),Lans.g("TableChartOfAccounts","Bank Number"),Lans.g("TableChartOfAccounts","Inactive") };
			List<IList<string>> listRows=new List<IList<string>>();
			for(int i=0;i<table.Rows.Count;i++) {
				listRows.Add(new List<string> { table.Rows[i]["type"].ToString(),table.Rows[i]["Description"].ToString(),table.Rows[i]["balance"].ToString(),
					table.Rows[i]["BankNumber"].ToString(),table.Rows[i]["inactive"].ToString() });
			}
			Golden.AssertBytes("chart-of-accounts.csv",WriteThroughFile(GridODExport.BuildText(listHeadings,listRows,GridExportFormat.Csv)));
		}

		///<summary>The UDS+ patient-level file for 2025 with a synthetic line map (the Table 6A line names, the fixture's Z codes).</summary>
		[TestMethod]
		public void CsvExport_UdsPlusPatientFile() {
			UdsPlusLineMap udsPlusLineMap=UdsPlusLineMap.Parse("{\"Lines\":["
				+"{\"Line\":\"28\",\"Description\":\"Oral exams\",\"Codes\":\"Z9101\"},"
				+"{\"Line\":\"32\",\"Description\":\"Restorative services\",\"Codes\":\"Z9102-Z9103\"},"
				+"{\"Line\":\"34\",\"Description\":\"Rehabilitative services\",\"Codes\":\"Z9103, Z91*\"}]}");
			UdsPlusResult udsPlusResult=RpUdsPlus.Run(udsPlusLineMap,new DateTime(2025,1,1),new DateTime(2025,12,31),new List<long>(),new List<long>());
			Assert.IsTrue(udsPlusResult.ListVisitRows.Count>0);
			Golden.AssertText("uds-plus-patients.csv",RpUdsPlus.ToCsv(udsPlusResult.ListVisitRows));
		}
	}
}
