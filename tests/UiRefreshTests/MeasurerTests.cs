// The shared measuring files (Shared\TextFit*.cs, package UI-0b, docs/port-notes/UI-0b.md): the measuring method by control type, the 0.96
// factor, the classification of section 5.3, the ink check, and the bounds record's format and rules. No database, no window shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiRefreshTests.Scan;
using UiRefreshTests.TextFit;

namespace UiRefreshTests {
	[TestClass]
	public class MeasurerTests {
		[TestMethod]
		public void GetMethod_TextRendererForStockControls_MeasureStringForTheProgramsOwn() {
			StaRunner.Run(() => {
				Assert.AreEqual(TextFitMethod.TextRenderer,TextFitMeasurer.GetMethod(new System.Windows.Forms.Label()));
				Assert.AreEqual(TextFitMethod.TextRenderer,TextFitMeasurer.GetMethod(new LinkLabel()));
				Assert.AreEqual(TextFitMethod.TextRenderer,TextFitMeasurer.GetMethod(new RadioButton()));
				Assert.AreEqual(TextFitMethod.TextRenderer,TextFitMeasurer.GetMethod(new System.Windows.Forms.CheckBox()));
				Assert.AreEqual(TextFitMethod.TextRenderer,TextFitMeasurer.GetMethod(new System.Windows.Forms.Button()));
				Assert.AreEqual(TextFitMethod.TextRenderer,TextFitMeasurer.GetMethod(new System.Windows.Forms.GroupBox()));
				Assert.AreEqual(TextFitMethod.MeasureString,TextFitMeasurer.GetMethod(new OpenPmsDental.UI.Button()));
				Assert.AreEqual(TextFitMethod.MeasureString,TextFitMeasurer.GetMethod(new OpenPmsDental.UI.CheckBox()));
				Assert.AreEqual(TextFitMethod.MeasureString,TextFitMeasurer.GetMethod(new OpenPmsDental.UI.GroupBox()));
				Assert.AreEqual(TextFitMethod.None,TextFitMeasurer.GetMethod(new System.Windows.Forms.TextBox()));
			});
		}

		[TestMethod]
		public void FontSizeUnderLayoutManager_LabelsAndRadioButtonsTakeTheFactor() {
			StaRunner.Run(() => {
				Assert.AreEqual(9f*0.96f,TextFitMeasurer.FontSizeUnderLayoutManager(new System.Windows.Forms.Label(),9f),0.0001f);
				Assert.AreEqual(9f*0.96f,TextFitMeasurer.FontSizeUnderLayoutManager(new RadioButton(),9f),0.0001f);
				Assert.AreEqual(9f,TextFitMeasurer.FontSizeUnderLayoutManager(new System.Windows.Forms.CheckBox(),9f));
				Assert.AreEqual(9f,TextFitMeasurer.FontSizeUnderLayoutManager(new OpenPmsDental.UI.Button(),9f));
			});
		}

		[TestMethod]
		public void Measure_Label_FitsWrapsAndOverflows() {
			StaRunner.Run(() => {
				using(Font font=new Font("Microsoft Sans Serif",8.25f))
				using(System.Windows.Forms.Label label=new System.Windows.Forms.Label()) {
					label.Font=font;
					label.Text="Patient";
					label.Size=new Size(100,20);
					Assert.AreEqual(TextFitKind.Fits,TextFitMeasurer.Measure(label,1f).Kind);
					//Too narrow for one line, one line high: more width.
					label.Size=new Size(20,14);
					TextFitResult result=TextFitMeasurer.Measure(label,1f);
					Assert.AreEqual(TextFitKind.Width,result.Kind);
					Assert.IsTrue(result.NeedWidth>0);
					//Two words, two lines of room: it wraps and fits.
					label.Text="Patient name";
					int widthWord=TextRenderer.MeasureText("Patient",font).Width;
					label.Size=new Size(widthWord+2,40);
					Assert.AreEqual(TextFitKind.Fits,TextFitMeasurer.Measure(label,1f).Kind);
					//Three lines needed, two and a half of room: another line.
					label.Text="Patient name here";
					label.Size=new Size(widthWord+2,32);
					Assert.AreEqual(TextFitKind.Line,TextFitMeasurer.Measure(label,1f).Kind);
					//One line, too short by a pixel: a height case, and the ink check runs.
					label.Text="Name";
					label.Size=new Size(100,TextRenderer.MeasureText("Name",font).Height-1);
					result=TextFitMeasurer.Measure(label,1f);
					Assert.AreEqual(TextFitKind.Height,result.Kind);
					Assert.AreEqual(1,result.NeedHeight);
					Assert.IsTrue(result.IsInkChecked);
					//At 150% the same label is measured with its font 1.5 times larger: it no longer fits.
					label.Size=new Size(TextRenderer.MeasureText("Name",font).Width+2,20);
					Assert.AreEqual(TextFitKind.Fits,TextFitMeasurer.Measure(label,1f).Kind);
					Assert.AreNotEqual(TextFitKind.Fits,TextFitMeasurer.Measure(label,1.5f).Kind);
				}
			});
		}

		[TestMethod]
		public void Measure_UiButton_UsesMeasureStringInItsTextRectangle() {
			StaRunner.Run(() => {
				using(Font font=new Font("Microsoft Sans Serif",8.25f))
				using(OpenPmsDental.UI.Button button=new OpenPmsDental.UI.Button()) {
					button.Font=font;
					button.Text="OK";
					button.Size=new Size(75,24);
					TextFitResult result=TextFitMeasurer.Measure(button,1f);
					Assert.AreEqual(TextFitMethod.MeasureString,result.Method);
					Assert.AreEqual(TextFitKind.Fits,result.Kind);
					Assert.AreEqual(new Size(75,24),result.SizeAvailable);
					button.Text="Supercalifragilistic";
					Assert.AreEqual(TextFitKind.Width,TextFitMeasurer.Measure(button,1f).Kind);
				}
			});
		}

		[TestMethod]
		public void InkCheck_FindsInkPastTheBoxOnly() {
			StaRunner.Run(() => {
				using(Font font=new Font("Segoe UI",9f)) {
					Size sizeText=TextRenderer.MeasureText(TextFitMeasurer.GraphicsMeasure,"Hg",font);
					TextFormatFlags flags=TextFormatFlags.Default;
					Assert.IsFalse(TextFitMeasurer.IsInkPastBoxGdi("Hg",font,sizeText,flags),"The measured box holds the ink.");
					Assert.IsTrue(TextFitMeasurer.IsInkPastBoxGdi("Hg",font,new Size(sizeText.Width,sizeText.Height/2),flags),"Half the height cuts it.");
					using(StringFormat stringFormat=TextFitMeasurer.CreateStringFormat(ContentAlignment.TopLeft)) {
						Size sizeTextPlus=TextFitMeasurer.MeasureLineGdiPlus("Hg",font,stringFormat);
						Assert.IsFalse(TextFitMeasurer.IsInkPastBoxGdiPlus("Hg",font,sizeTextPlus,stringFormat));
						Assert.IsTrue(TextFitMeasurer.IsInkPastBoxGdiPlus("Hg",font,new Size(sizeTextPlus.Width/2,sizeTextPlus.Height),stringFormat));
					}
				}
			});
		}

		[TestMethod]
		public void Classify_FollowsSection53() {
			TextFitResult result=new TextFitResult { SizeAvailable=new Size(100,13) };
			TextFitMeasurer.Classify(result,new Size(90,15),Size.Empty,true);
			Assert.AreEqual(TextFitKind.Height,result.Kind);
			Assert.AreEqual(2,result.NeedHeight);
			result=new TextFitResult { SizeAvailable=new Size(100,13) };
			TextFitMeasurer.Classify(result,new Size(120,13),new Size(100,26),true);
			Assert.AreEqual(TextFitKind.Width,result.Kind,"Room for one line only.");
			Assert.AreEqual(20,result.NeedWidth);
			result=new TextFitResult { SizeAvailable=new Size(100,30) };
			TextFitMeasurer.Classify(result,new Size(220,13),new Size(100,39),true);
			Assert.AreEqual(TextFitKind.Line,result.Kind);
			Assert.AreEqual(9,result.NeedHeight);
			result=new TextFitResult { SizeAvailable=new Size(100,30) };
			TextFitMeasurer.Classify(result,new Size(180,13),new Size(100,26),true);
			Assert.AreEqual(TextFitKind.Fits,result.Kind);
			result=new TextFitResult { SizeAvailable=new Size(100,30) };
			TextFitMeasurer.Classify(result,new Size(180,13),Size.Empty,false);
			Assert.AreEqual(TextFitKind.Width,result.Kind,"A control that cannot wrap needs width.");
		}

		[TestMethod]
		public void BoundsRecord_WritesAndReadsTheSameRows() {
			StaRunner.Run(() => {
				using(Form form=new Form())
				using(Panel panel=new Panel())
				using(System.Windows.Forms.Label label=new System.Windows.Forms.Label())
				using(System.Windows.Forms.Label labelUnnamed=new System.Windows.Forms.Label()) {
					form.ClientSize=new Size(300,200);
					panel.Name="panel";
					panel.Bounds=new Rectangle(10,10,200,100);
					label.Name="label";
					label.AutoSize=true;
					label.Text="Text";
					label.Location=new Point(5,5);
					label.Anchor=AnchorStyles.Top|AnchorStyles.Right;
					labelUnnamed.Bounds=new Rectangle(5,40,50,20);
					labelUnnamed.Visible=false;
					panel.Controls.Add(label);
					panel.Controls.Add(labelUnnamed);
					form.Controls.Add(panel);
					List<TextFitBoundsRow> listRows=TextFitBoundsRecord.Collect("Test.Window",form);
					Assert.AreEqual(4,listRows.Count);
					Assert.AreEqual(".",listRows[0].Path);
					Assert.AreEqual("panel/label",listRows[2].Path);
					Assert.AreEqual("panel/#1:Label",listRows[3].Path);
					Assert.IsTrue(listRows[2].IsSizingItself);
					Assert.IsTrue(listRows[2].IsVisible,"Visible as designed although its window is not shown.");
					Assert.IsFalse(listRows[3].IsVisible);
					Assert.AreEqual("panel/label",TextFitBoundsRecord.GetPath(form,label));
					StringWriter stringWriter=new StringWriter();
					TextFitBoundsRecord.Write(stringWriter,listRows);
					List<TextFitBoundsRow> listRead=TextFitBoundsRecord.Read(new StringReader(stringWriter.ToString()));
					Assert.AreEqual(listRows.Count,listRead.Count);
					for(int i=0;i<listRows.Count;i++) {
						Assert.AreEqual(TextFitBoundsRecord.ToLine(listRows[i]),TextFitBoundsRecord.ToLine(listRead[i]));
					}
					Assert.AreEqual(0,TextFitBoundsRecord.Compare(listRows,listRead,null).Count);
				}
			});
		}

		[TestMethod]
		public void BoundsRecord_SelfSizingRule_KeepsTheAnchoredEdge() {
			TextFitBoundsRow rowBefore=new TextFitBoundsRow { Window="W",Path="label",Bounds=new Rectangle(100,10,40,13),IsSizingItself=true,IsVisible=true,
				Anchor=AnchorStyles.Top|AnchorStyles.Left };
			TextFitBoundsRow rowGrown=new TextFitBoundsRow { Window="W",Path="label",Bounds=new Rectangle(100,10,46,15),IsSizingItself=true,IsVisible=true,
				Anchor=AnchorStyles.Top|AnchorStyles.Left };
			Assert.AreEqual("",TextFitBoundsRecord.CompareRow(rowBefore,rowGrown),"Growth from the same corner is no difference.");
			TextFitBoundsRow rowMoved=new TextFitBoundsRow { Window="W",Path="label",Bounds=new Rectangle(94,10,46,15),IsSizingItself=true,IsVisible=true,
				Anchor=AnchorStyles.Top|AnchorStyles.Left };
			Assert.AreEqual("anchored-edge",TextFitBoundsRecord.CompareRow(rowBefore,rowMoved));
			//Anchored right only: the right edge stays, so growing to the left is fine.
			rowBefore.Anchor=AnchorStyles.Top|AnchorStyles.Right;
			rowMoved.Anchor=AnchorStyles.Top|AnchorStyles.Right;
			Assert.AreEqual("",TextFitBoundsRecord.CompareRow(rowBefore,rowMoved));
			TextFitBoundsRow rowFixed=new TextFitBoundsRow { Window="W",Path="text",Bounds=new Rectangle(1,2,3,4) };
			TextFitBoundsRow rowFixedMoved=new TextFitBoundsRow { Window="W",Path="text",Bounds=new Rectangle(1,2,3,5) };
			Assert.AreEqual("bounds",TextFitBoundsRecord.CompareRow(rowFixed,rowFixedMoved));
			//Listed in fit-fixes.tsv: no difference.
			List<TextFitBoundsDifference> listDifferences=TextFitBoundsRecord.Compare(new List<TextFitBoundsRow> { rowFixed },
				new List<TextFitBoundsRow> { rowFixedMoved },new HashSet<string> { TextFitBoundsRecord.GetFitFixKey("W","text") });
			Assert.AreEqual(0,listDifferences.Count);
		}

		[TestMethod]
		public void GetFreeSpace_StopsAtTheNearestSiblingBesideOrUnder() {
			Rectangle rectangle=new Rectangle(10,10,50,20);
			List<Rectangle> listSiblings=new List<Rectangle> {
				new Rectangle(80,12,40,20),//beside, 20 px to the right
				new Rectangle(200,0,10,100),//beside, further
				new Rectangle(10,45,30,10),//under, 15 px below
				new Rectangle(0,0,300,300),//a panel behind it: overlaps, does not count
			};
			Point pointFree=TierOneScan.GetFreeSpace(rectangle,listSiblings,new Size(300,300));
			Assert.AreEqual(new Point(20,15),pointFree);
		}
	}
}
