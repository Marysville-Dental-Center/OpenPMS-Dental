// Golden outputs: wikilisthist.ListContent, the wiki list history the program saves before each change of a wiki list (WikiListHists.GenerateFromName:
// the list's DataTable written with DataTable.WriteXml and its schema), recorded with the .NET Framework build (docs/port-notes/S3a-4.md; the plan's
// risk table, "Type names"). The content written by .NET Framework must also read back on .NET 10: reverting the list to the recorded history
// (WikiListHists.RevertFrom, which reads it with DataTable.ReadXml) gives the fixture's rows.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace GoldenOutputTests {
	[TestClass]
	public class WikiListHistGoldenTests {
		private const string FIXTURE="wikilisthist-listcontent.xml";

		[TestInitialize]
		public void TestInitialize() {
			GoldenDb.Begin();
		}

		///<summary>The list's rows as text: one line per row, the cells separated by " | ", line breaks inside a cell shown as \r and \n.</summary>
		private static string DumpList() {
			DataTable table=WikiLists.GetByName(GoldenData.WikiListName);
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.Append(string.Join(" | ",table.Columns.OfType<DataColumn>().Select(x => x.ColumnName))+"\n");
			foreach(DataRow dataRow in table.Rows) {
				stringBuilder.Append(string.Join(" | ",dataRow.ItemArray.Select(x => x.ToString().Replace("\r","\\r").Replace("\n","\\n")))+"\n");
			}
			return stringBuilder.ToString();
		}

		[TestMethod]
		public void WikiListHist_ListContent_IsTheRecordedXml() {
			WikiListHist wikiListHist=WikiListHists.GenerateFromName(GoldenData.WikiListName,Security.CurUser.UserNum);
			Assert.IsNotNull(wikiListHist);
			StringAssert.Contains(wikiListHist.ListContent,"Synthetic Supply");
			Golden.AssertText(FIXTURE,wikiListHist.ListContent);
		}

		[TestMethod]
		public void WikiListHist_RecordedListContent_RevertsToTheFixtureRows() {
			string dumpBefore=DumpList();
			WikiListHist wikiListHistCurrent=WikiListHists.GenerateFromName(GoldenData.WikiListName,Security.CurUser.UserNum);
			//The recorded content with the current column widths: RevertFrom recreates the table from ListHeaders and fills it from ListContent.
			WikiListHist wikiListHistRecorded=new WikiListHist {
				UserNum=Security.CurUser.UserNum,ListName=GoldenData.WikiListName,ListHeaders=wikiListHistCurrent.ListHeaders,
				ListContent=Golden.IsRecordMode ? wikiListHistCurrent.ListContent : Golden.ReadFixtureText(FIXTURE),DateTimeSaved=GoldenData.DateStatement,
			};
			string dumpAfter;
			try {
				WikiListHists.RevertFrom(wikiListHistRecorded,Security.CurUser.UserNum);
				dumpAfter=DumpList();
			}
			finally {
				//The revert changed the list (see below): the fixture's list is written again, so the other test sees it whatever the order.
				GoldenData.SeedWikiList(true);
			}
			//The same rows, except that a line break inside a cell comes back as LF alone: XML reading normalises CRLF to LF (DataTable.ReadXml),
			//which is how a revert has always come out on .NET Framework.
			Assert.AreEqual(dumpBefore.Replace("\\r\\n","\\n"),dumpAfter,"The list reverted to the recorded history holds the same rows.");
			Golden.AssertText("wikilist-rows-reverted.txt",dumpAfter);
			Assert.AreEqual(dumpBefore,DumpList(),"The fixture's list is back.");
		}
	}
}
