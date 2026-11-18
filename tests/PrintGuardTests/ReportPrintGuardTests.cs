// The print guard for reports (package UI-2a-3): a ReportComplex report printed by FormReportComplex's page event, an RDL report rendered to
// PDF by the RDL engine, a MigraDoc document made with MigraDocHelper (text, a table and a grid through MigraDocHelper.DrawGrid) and the graph
// dashboard's chart page (FormPrintImage's page bitmap, which it prints and saves as a PDF) are identical with today's font tokens and with
// UI-2b's. Synthetic data; nothing is printed and no window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using fyiReporting.RDL;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.ReportingComplex;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;
using OpenPmsDentalGraph;
using PdfSharp.Pdf;
using CheckBox=System.Windows.Forms.CheckBox;
using Document=MigraDoc.DocumentObjectModel.Document;
using Section=MigraDoc.DocumentObjectModel.Section;

namespace PrintGuardTests {
	[TestClass]
	public class ReportPrintGuardTests {
		[TestMethod]
		public void ReportComplex_PrintedPages_AreTheSameWithUi2bFonts() {
			Guard.AssertSameWithUi2bFonts("ReportComplex",() => {
				ReportComplex reportComplex=new ReportComplex(true,false,false);
				reportComplex.ReportName="Print guard";
				reportComplex.AddTitle("Title","Synthetic Production Report");
				reportComplex.AddSubTitle("Practice","Synthetic Practice");
				reportComplex.AddSubTitle("Dates","01/05/2026 - 02/28/2026");
				DataTable table=new DataTable();
				foreach(string column in new[] { "Date","Patient","Code","Description","Fee" }) {
					table.Columns.Add(column);
				}
				for(int i=0;i<70;i++) {
					table.Rows.Add(new DateTime(2026,1,5).AddDays(i).ToString("MM/dd/yyyy"),"Synthetic, Patient "+i,"D"+(1110+i%40),
						i%6==0 ? "A synthetic description that is long enough to need more than one line" : "Synthetic service "+i,(55.5+i*3.25).ToString("F2"));
				}
				QueryObject queryObject=reportComplex.AddQuery(table,"Procedures","",SplitByKind.None,0);
				queryObject.AddColumn("Date",90,FieldValueType.String);
				queryObject.AddColumn("Patient",170,FieldValueType.String);
				queryObject.AddColumn("Code",60,FieldValueType.String);
				queryObject.AddColumn("Description",260,FieldValueType.String);
				queryObject.AddColumn("Fee",80,FieldValueType.String);
				reportComplex.AddPageNum();
				Assert.IsTrue(reportComplex.SubmitQueries(false,false),"The report's query (a table) is submitted.");
				using(FormReportComplex formReportComplex=new FormReportComplex(reportComplex)) {
					//ResetODprintout's preparation, without its print document (which reads the printer): no section printed yet, and each query's
					//row heights, without wrapping (the ReportsWrapColumns default).
					SetField(formReportComplex,"_lastSectionPrinted",AreaSectionType.None);
					foreach(QueryObject queryObjectReport in reportComplex.ReportObjects.OfType<QueryObject>()) {
						queryObjectReport.CalculateRowHeights(false);
					}
					List<byte> listBytes=new List<byte>();
					for(int page=0;page<10;page++) {
						bool hasMorePages=false;
						listBytes.AddRange(Guard.DrawToPixels(850,1100,g => {
							PrintPageEventArgs printPageEventArgs=new PrintPageEventArgs(g,new Rectangle(0,0,850,1100),new Rectangle(0,0,850,1100),new PageSettings());
							DocumentPrintGuardTests.Invoke(typeof(FormReportComplex),formReportComplex,"pd2_PrintPage",null,printPageEventArgs);
							hasMorePages=printPageEventArgs.HasMorePages;
						}));
						if(!hasMorePages) {
							return listBytes.ToArray();
						}
					}
					Assert.Fail("The report did not end within 10 pages.");
					return null;
				}
			},PrintGuardDb.Begin);
		}

		[TestMethod]
		public void Rdl_Pdf_IsTheSameWithUi2bFonts() {
			Guard.AssertSameWithUi2bFonts("Rdl_Pdf",() => {
				RDLParser rdlParser=new RDLParser(RDL_TEXT_REPORT);
				rdlParser.Folder=AppDomain.CurrentDomain.BaseDirectory;
				fyiReporting.RDL.Report report=rdlParser.Parse();
				Assert.AreEqual(0,report.ErrorMaxSeverity,"The synthetic report parses: "
					+(report.ErrorItems==null ? "" : string.Join(" | ",report.ErrorItems.Cast<object>())));
				report.RunGetData(new ListDictionary());
				using(MemoryStreamGen memoryStreamGen=new MemoryStreamGen()) {
					report.RunRender(memoryStreamGen,OutputPresentationType.PDF);
					byte[] arrayPdf=((MemoryStream)memoryStreamGen.GetStream()).ToArray();
					//The file's own dates differ with every render, and with their length the offsets of the objects after them.
					string text=Encoding.GetEncoding(1252).GetString(arrayPdf);
					text=Regex.Replace(text,@"\(D:[^)]*\)","(D:)");
					text=Regex.Replace(text,@"\d{10} \d{5} [nf]","offset");
					text=Regex.Replace(text,@"startxref\s+\d+","startxref");
					return Encoding.GetEncoding(1252).GetBytes(text);
				}
			});
		}

		[TestMethod]
		public void MigraDoc_Pdf_IsTheSameWithUi2bFonts() {
			Guard.AssertSameWithUi2bFonts("MigraDoc_Pdf",() => {
				Document document=MigraDocHelper.CreateDocument();
				PageSetup pageSetup=MigraDocHelper.GetPageSetup(document);
				pageSetup.PageWidth=Unit.FromInch(8.5);
				pageSetup.PageHeight=Unit.FromInch(11);
				pageSetup.TopMargin=Unit.FromInch(.5);
				pageSetup.LeftMargin=Unit.FromInch(.5);
				pageSetup.RightMargin=Unit.FromInch(.5);
				Section section=MigraDocHelper.AddSection(document);
				Paragraph paragraph=section.AddParagraph();
				paragraph.AddFormattedText("Synthetic treatment plan",MigraDocHelper.CreateFont(13,true));
				section.AddParagraph("Text without a font of its own.");
				MigraDoc.DocumentObjectModel.Shapes.TextFrame textFrame=MigraDocHelper.CreateContainer(section,40,60,700,80);
				MigraDocHelper.DrawString(textFrame,"Placed text in MigraDocHelper's font",MigraDocHelper.CreateFont(10),new RectangleF(0,0,400,20));
				//A grid as the appointment and reconcile printouts send one (MigraDocHelper.DrawGrid takes the grid's columns, rows and colours).
				using(GridOD grid=new GridOD()) {
					grid.TranslationName="";
					grid.Size=new Size(560,300);
					grid.BeginUpdate();
					grid.Columns.Add(new GridColumn("Date",90));
					grid.Columns.Add(new GridColumn("Description",300));
					grid.Columns.Add(new GridColumn("Amount",90,HorizontalAlignment.Right));
					for(int i=0;i<12;i++) {
						GridRow gridRow=new GridRow(new DateTime(2026,2,1).AddDays(i).ToString("MM/dd/yyyy"),"Synthetic line "+i,(20.25*i).ToString("F2"));
						gridRow.Bold=(i%5==0);
						grid.ListGridRows.Add(gridRow);
					}
					grid.EndUpdate();
					MigraDocHelper.DrawGrid(section,grid);
				}
				PdfDocumentRenderer pdfDocumentRenderer=new PdfDocumentRenderer();
				pdfDocumentRenderer.Document=document;
				pdfDocumentRenderer.RenderDocument();
				return Guard.GetPdfPages(pdfDocumentRenderer.PdfDocument);
			});
		}

		[TestMethod]
		public void GraphDashboard_ChartPage_IsTheSameWithUi2bFonts() {
			//FormPrintImage's page: each chart copied through its serializer and drawn into the page bitmap (_bmpSheet), which the window prints
			//(ImageGenericFormat_PrintPage) and saves as a PDF (butExport_Click). Its Load, without the dashboard's database graphs.
			Guard.AssertSameWithUi2bFonts("GraphDashboard_ChartPage",() => {
				using(GraphQuantityOverTime graph=new GraphQuantityOverTime())
				using(DashboardPanelCtrl dashboardPanelCtrl=new DashboardPanelCtrl())
				using(FormPrintImage formPrintImage=new FormPrintImage(dashboardPanelCtrl)) {
					graph.GraphTitle="Synthetic production";
					Chart chart=graph.GetChartForPrinting();
					if(chart.Series.Count==0) {
						chart.Series.Add(new Series("Synthetic") { ChartType=SeriesChartType.StackedArea,ChartArea=chart.ChartAreas[0].Name });
					}
					chart.Series[0].Points.Clear();
					for(int i=0;i<12;i++) {
						chart.Series[0].Points.AddXY(new DateTime(2026,1,1).AddMonths(i),1000+i*137.5);
					}
					Type type=typeof(FormPrintImage);
					SetField(formPrintImage,"_dictCharts",new Dictionary<Point,Chart> { { new Point(0,0),chart } });
					CheckBox checkLandscape=(CheckBox)GetField(formPrintImage,"checkLandscape");
					PrintDocument printDocument=(PrintDocument)GetField(formPrintImage,"printDocument1");
					checkLandscape.Checked=(dashboardPanelCtrl.Columns*8>dashboardPanelCtrl.Rows*5);
					printDocument.DefaultPageSettings.Landscape=checkLandscape.Checked;
					DocumentPrintGuardTests.Invoke(type,formPrintImage,"FillDimensions");
					Bitmap bitmapImage=(Bitmap)DocumentPrintGuardTests.Invoke(type,formPrintImage,"ConvertToBmp",true);
					SetField(formPrintImage,"_bmpImage",bitmapImage);
					((TextBox)GetField(formPrintImage,"textWidth")).Text=bitmapImage.Width.ToString();
					((TextBox)GetField(formPrintImage,"textHeight")).Text=bitmapImage.Height.ToString();
					DocumentPrintGuardTests.Invoke(type,formPrintImage,"MakePage",false);
					SetField(formPrintImage,"_isLoading",false);
					return Guard.GetPixels((Bitmap)GetField(formPrintImage,"_bmpSheet"));
				}
			},PrintGuardDb.Begin);
		}

		#region Helpers
		private static object GetField(object instance,string name) {
			return instance.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(instance);
		}

		private static void SetField(object instance,string name,object value) {
			instance.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(instance,value);
		}

		///<summary>A letter page with a title, text in the RDL default font, in Arial, Times New Roman and Tahoma, bold, right aligned and
		///wrapped, and a line: no data source.</summary>
		private const string RDL_TEXT_REPORT=@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Report xmlns=""http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition"">
  <PageHeight>11in</PageHeight>
  <PageWidth>8.5in</PageWidth>
  <Width>7.5in</Width>
  <TopMargin>.5in</TopMargin>
  <LeftMargin>.5in</LeftMargin>
  <RightMargin>.5in</RightMargin>
  <BottomMargin>.5in</BottomMargin>
  <Body>
    <Height>6in</Height>
    <ReportItems>
      <Textbox Name=""Title"">
        <Top>0in</Top><Left>0in</Left><Height>.4in</Height><Width>7in</Width>
        <Value>Synthetic RDL report</Value>
        <Style><FontFamily>Arial</FontFamily><FontSize>16pt</FontSize><FontWeight>Bold</FontWeight><TextAlign>Center</TextAlign></Style>
      </Textbox>
      <Textbox Name=""DefaultFont"">
        <Top>.6in</Top><Left>0in</Left><Height>.25in</Height><Width>7in</Width>
        <Value>Text in the report's default font</Value>
      </Textbox>
      <Textbox Name=""Times"">
        <Top>1in</Top><Left>0in</Left><Height>.6in</Height><Width>3in</Width>
        <Value>Times New Roman text that is long enough to wrap onto more than one line in this box</Value>
        <CanGrow>true</CanGrow>
        <Style><FontFamily>Times New Roman</FontFamily><FontSize>10pt</FontSize></Style>
      </Textbox>
      <Textbox Name=""Tahoma"">
        <Top>1in</Top><Left>4in</Left><Height>.25in</Height><Width>3in</Width>
        <Value>Tahoma, right aligned</Value>
        <Style><FontFamily>Tahoma</FontFamily><FontSize>9pt</FontSize><TextAlign>Right</TextAlign></Style>
      </Textbox>
      <Line Name=""Rule"">
        <Top>1.8in</Top><Left>0in</Left><Height>0in</Height><Width>7in</Width>
        <Style><BorderStyle><Default>Solid</Default></BorderStyle></Style>
      </Line>
    </ReportItems>
  </Body>
</Report>";
		#endregion Helpers
	}
}
