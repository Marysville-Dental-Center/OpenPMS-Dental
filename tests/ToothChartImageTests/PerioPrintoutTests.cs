// The graphical perio chart's printout and saved image (package TC-7): the test copy of FormPerioGraphical.RenderPerioPrintout (PerioPrintout) stays
// in step with the window, so the baselines of the print and the 750x1000 save show what the window makes. The pictures themselves are checked by
// WpfImageTests.ChartScenes_MatchTheWpfBaselines (Baselines\wpf-net48\perio-print and perio-save).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using SparksToothChart;

namespace ToothChartImageTests {
	[TestClass]
	public class PerioPrintoutTests {
		private const string FORM_PERIO_GRAPHICAL=@"OpenPmsDental\Forms\FormPerioGraphical.cs";
		private const string FORM_PERIO_GRAPHICAL_DESIGNER=@"OpenPmsDental\Forms\FormPerioGraphical.designer.cs";

		[TestMethod]
		public void TheCopyOfRenderPerioPrintout_FollowsTheWindow() {
			string code=SourceText.ReadCode(FORM_PERIO_GRAPHICAL);
			string method=SourceText.GetMethod(code,"public void RenderPerioPrintout(");
			SourceText.AssertInOrder(method,"RenderPerioPrintout",
				"float y=20;",
				"using Font fontTitle=new Font(\"Arial\",15);",
				"string titleStr=Lan.g(this,\"Periodontal Examination\");",
				"g.DrawString(titleStr,fontTitle,Brushes.Black,new PointF(rectangleMarginBounds.Width/2f-sizeF.Width/2f,y));",
				"using Font font=new Font(\"Arial\",11);",
				"g.DrawString(clinicName,font,Brushes.Black,new PointF(rectangleMarginBounds.Width/2f-sizeF.Width/2f,y));",
				"g.DrawString(patNameStr,font,Brushes.Black,new PointF(rectangleMarginBounds.Width/2f-sizeF.Width/2f,y));",
				"birthdateStr=Lan.g(this,\"DOB:\")+\" \"+birthdateStr;",
				"g.DrawString(birthdateStr,font,Brushes.Black,new PointF(rectangleMarginBounds.Width/2f-sizeF.Width/2f,y));",
				"g.DrawString(examDateStr,font,Brushes.Black,new PointF(rectangleMarginBounds.Width/2f-sizeF.Width/2f,y));",
				"SizeF sizeFAvailable=new SizeF(rectangleMarginBounds.Width,rectangleMarginBounds.Height-y);",
				"using Bitmap bitmapToothChart=_toothChartRelay.GetBitmap();",
				"float ratioBitmapToOutput=(float)bitmapToothChart.Height/(float)sizeFAvailable.Height;",
				"ratioBitmapToOutput=(float)bitmapToothChart.Width/(float)sizeFAvailable.Width;",
				"SizeF sizeFBitmapToOut=new SizeF((float)bitmapToothChart.Width/ratioBitmapToOutput,(float)bitmapToothChart.Height/ratioBitmapToOutput);",
				"g.DrawImage(bitmapToothChart,",
				"x:sizeFAvailable.Width/2f-sizeFBitmapToOut.Width/2f,",
				"y:y,");
			//The print: the margin bounds of a page with margins of 50 (Letter at 100 per inch: 750x1000, PerioPrintout.SizePage), origin at the margin.
			string methodPrint=SourceText.GetMethod(code,"private void butPrint_Click(");
			StringAssert.Contains(methodPrint,"margins:new Margins(50,50,50,50),");
			StringAssert.Contains(methodPrint,"printoutOrigin:PrintoutOrigin.AtMargin);");
			StringAssert.Contains(SourceText.GetMethod(code,"private void pd2_PrintPage("),"RenderPerioPrintout(g,_patient,ev.MarginBounds);");
			//The save: a new 750x1000 bitmap cleared to white with high-quality compositing and smoothing.
			string methodSave=SourceText.GetMethod(code,"private void butSave_Click(");
			SourceText.AssertInOrder(methodSave,"butSave_Click",
				"using Bitmap bitmap=new Bitmap(750,1000);",
				"g.Clear(Color.White);",
				"g.CompositingQuality=System.Drawing.Drawing2D.CompositingQuality.HighQuality;",
				"g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.HighQuality;",
				"RenderPerioPrintout(g,_patient,new Rectangle(0,0,bitmap.Width,bitmap.Height));");
			Assert.AreEqual(new Size(750,1000),PerioPrintout.SizePage);
			//The chart's picture is the window's wrapper at 649x696, the size of the perio scenes.
			StringAssert.Contains(SourceText.ReadCode(FORM_PERIO_GRAPHICAL_DESIGNER),"this.toothChartWrapper.Size = new System.Drawing.Size(649, 696);");
			Assert.AreEqual(new Size(649,696),ChartScenes.SizePerio.Size);
		}

		[TestMethod]
		public void TheWindow_StillPassesOnlyImplantsThatAreNotRemoved_AndOpensThroughTheRelay() {
			//Open Dental 24.4: an implant whose procedure's graphics are hidden was removed, so the window passes no implant for it, and the WPF chart draws
			//exactly the implants it is given (B-PERIO; PerioUiSourceTests pins the same line).
			string code=SourceText.ReadCode(FORM_PERIO_GRAPHICAL);
			string method=SourceText.GetMethod(code,"private void LayoutTeeth(");
			SourceText.AssertInOrder(method,"LayoutTeeth",
				"ToothChartRelay.ApplyGraphicalPerioMode(toothChartWrapper);",
				"_toothChartRelay.SetPerioColors(",
				"_toothChartRelay.IsPerioMode=true;",
				"procedureCode.PaintType==ToothPaintingType.Implant && !listProceduresForTooth[p].HideGraphics",
				"_toothChartRelay.SetImplant(t.ToString(),Color.Gainsboro);");
		}
	}
}
