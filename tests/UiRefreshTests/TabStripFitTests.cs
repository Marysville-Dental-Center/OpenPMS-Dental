// UI-4b-2's tab strip test (docs/specs/ui-refresh-plan.md, section 7's UI-4b-2 line, sections 3.3 and 3.6, UQ-9; docs/port-notes/UI-4b.md): the
// self-drawn tab headers (OpenPmsDental.UI.TabControl) keep today's width formula (TabControl.LayoutTabs: the text's width plus 8 px), and in the
// windows with the most self-drawn tabs on main (FormClaimEdit 10, FormMedical 9, FormFeeSchedTools 6, FormPatientEdit 5) every tab strip keeps
// its tabs in order with none past the strip's right edge, at the window's designer size and at its smallest size, in today's font and in
// UI-2b's Segoe UI 9 (UiTheme.SetFontForTest, the product's own font path, section 5.4). None of the four windows sets a MinimumSize, so "its
// smallest size" is the designer size cut to the smallest desktop the refresh checks (1024 x 768 less a 40 px taskbar, as UI-6's 1024 px
// toolbars). Data\tab-overflow.tsv lists the overflow the program has (for the owner, UQ-9): the test fails when a strip overflows where the
// list does not say so, or no longer overflows where it does (OPMS_UI4B2_WRITE_OVERFLOW=1 rewrites it). The windows are built as the scan builds
// them (WindowBuilder: designer code and LayoutManager, not the window's own constructor), so the tab captions are the designer's. With
// OPMS_UI_TABS_WINDOWS set to a folder, the strips are also drawn off screen into it (for G-UI2 and G-UI3, before and after). No window is shown,
// no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using UiRefreshTests.Scan;
using UiRefreshTests.TextFit;

namespace UiRefreshTests {
	[TestClass]
	public class TabStripFitTests {
		///<summary>Set to 1 to rewrite Data\tab-overflow.tsv from this run.</summary>
		public const string VARIABLE_WRITE="OPMS_UI4B2_WRITE_OVERFLOW";
		///<summary>A folder: the four windows' tab strips are drawn into it (font\window-tabControl-size.png).</summary>
		public const string VARIABLE_PICTURES="OPMS_UI_TABS_WINDOWS";
		public const string FILE_OVERFLOW=@"tests\UiRefreshTests\Data\tab-overflow.tsv";
		///<summary>The smallest desktop the refresh checks: 1024 x 768 less a 40 px taskbar.</summary>
		public static readonly Size SizeSmallestScreen=new Size(1024,728);

		///<summary>The windows with the most self-drawn tabs on main and how many tabs each has in all its tab controls (section 7).</summary>
		public static readonly Tuple<Type,int>[] ArrayWindows=new Tuple<Type,int>[] {
			Tuple.Create(typeof(FormClaimEdit),10),
			Tuple.Create(typeof(FormMedical),9),
			Tuple.Create(typeof(FormFeeSchedTools),6),
			Tuple.Create(typeof(FormPatientEdit),5),
		};

		///<summary>One tab control of a window in one font at one size.</summary>
		public class StripRow {
			public string Window;
			public string TabControl;
			public string Font;
			public string SizeName;
			public Size SizeWindow;
			public int WidthTabControl;
			///<summary>The strip's right edge in the tab control's coordinates: its width, or less where a parent cuts it off.</summary>
			public int RightVisible;
			public List<string> ListTexts=new List<string>();
			public List<Rectangle> ListRectangles=new List<Rectangle>();
			public List<string> ListOverflow=new List<string>();

			public string Key {
				get { return Window+"\t"+TabControl+"\t"+Font+"\t"+SizeName; }
			}
		}

		#region Measuring
		///<summary>The two fonts: today's and UI-2b's Segoe UI 9 (UiFontOption.Today named: the program's tokens are UI-2b's since the flip).</summary>
		public static List<Tuple<string,UiFontOption,TextFitFontOption>> GetFonts() {
			return new List<Tuple<string,UiFontOption,TextFitFontOption>> {
				Tuple.Create("today",UiFontOption.Today,TextFitFontOption.Today),
				Tuple.Create("segoe9",UiFontOption.UI2b,TextFitFontOption.Segoe9),
			};
		}

		///<summary>Every tab control of the four windows in both fonts at both sizes. With folderPictures, each strip is also drawn into it. Call on an
		///STA thread.</summary>
		public static List<StripRow> MeasureAll(string folderPictures) {
			NoDatabaseGuard.Install();
			List<StripRow> listRows=new List<StripRow>();
			try {
				foreach(Tuple<string,UiFontOption,TextFitFontOption> tupleFont in GetFonts()) {
					DefaultFontOverride.Set(tupleFont.Item3,1f);
					UiTheme.SetFontForTest(tupleFont.Item2);
					try {
						foreach(Tuple<Type,int> tupleWindow in ArrayWindows) {
							listRows.AddRange(MeasureWindow(tupleWindow.Item1,tupleFont.Item1,folderPictures));
						}
					}
					finally {
						UiTheme.SetFontForTest(null);
					}
				}
			}
			finally {
				DefaultFontOverride.Set(TextFitFontOption.Today,1f);
			}
			return listRows;
		}

		private static List<StripRow> MeasureWindow(Type type,string font,string folderPictures) {
			List<StripRow> listRows=new List<StripRow>();
			ScanType scanType=new ScanType { Type=type,Kind=BuildKind.LayoutManagerWindow,Assembly=type.Assembly.GetName().Name,WindowKey=type.FullName };
			using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
				Size sizeDesigner=builtWindow.Form.Size;
				Size sizeSmallest=new Size(Math.Min(sizeDesigner.Width,SizeSmallestScreen.Width),Math.Min(sizeDesigner.Height,SizeSmallestScreen.Height));
				foreach(Tuple<string,Size> tupleSize in new[] { Tuple.Create("designer",sizeDesigner),Tuple.Create("smallest",sizeSmallest) }) {
					LayOutAtSize(builtWindow,tupleSize.Item2);
					DesignSheetWindows.CreateHandles(builtWindow.Form);
					foreach(OpenPmsDental.UI.TabControl tabControl in GetTabControls(builtWindow.Form)) {
						tabControl.LayoutTabs();
						StripRow stripRow=new StripRow { Window=type.Name,TabControl=tabControl.Name,Font=font,SizeName=tupleSize.Item1,SizeWindow=builtWindow.Form.Size,
							WidthTabControl=tabControl.Width,RightVisible=GetRightVisible(tabControl) };
						List<OpenPmsDental.UI.TabPage> listTabPages=tabControl.TabPages.ToList();
						for(int i=0;i<listTabPages.Count;i++) {
							Rectangle rectangle=tabControl.GetTabRect(i);
							stripRow.ListTexts.Add(listTabPages[i].Text);
							stripRow.ListRectangles.Add(rectangle);
							if(rectangle.Right>stripRow.RightVisible) {
								stripRow.ListOverflow.Add(listTabPages[i].Text);
							}
						}
						listRows.Add(stripRow);
						if(!string.IsNullOrEmpty(folderPictures)) {
							SaveStrip(tabControl,Path.Combine(folderPictures,font,type.Name+"-"+tabControl.Name+"-"+tupleSize.Item1+".png"));
						}
					}
				}
			}
			return listRows;
		}

		///<summary>Lays the window out at a window size at 100%, as BuiltWindow.LayOutAt does for its designer size (FormODBase's resize path).</summary>
		public static void LayOutAtSize(BuiltWindow builtWindow,Size sizeWindow) {
			builtWindow.LayOutAt(1f);
			if(builtWindow.Form.Size==sizeWindow) {
				return;
			}
			builtWindow.Form.Bounds=new Rectangle(builtWindow.Form.Location,sizeWindow);
			builtWindow.LayoutManager.LayoutFormBoundsAndFonts((FormODBase)builtWindow.Form);
		}

		///<summary>Every UI.TabControl in the control, front to back as the designer adds them, nested ones after their parent.</summary>
		public static List<OpenPmsDental.UI.TabControl> GetTabControls(Control control) {
			List<OpenPmsDental.UI.TabControl> listTabControls=new List<OpenPmsDental.UI.TabControl>();
			foreach(Control child in control.Controls) {
				if(child is OpenPmsDental.UI.TabControl tabControl) {
					listTabControls.Add(tabControl);
				}
				listTabControls.AddRange(GetTabControls(child));
			}
			return listTabControls;
		}

		///<summary>The tab control's right edge in its own coordinates, cut where a parent's client area ends.</summary>
		public static int GetRightVisible(Control control) {
			int right=control.Width;
			int xOffset=0;
			for(Control child=control;child.Parent!=null;child=child.Parent) {
				xOffset+=child.Left;
				int rightParent=child.Parent.ClientSize.Width-xOffset;
				right=Math.Min(right,rightParent);
			}
			return right;
		}

		///<summary>The tab control's strip and the top of its page (the strip plus 12 px), drawn off screen.</summary>
		private static void SaveStrip(OpenPmsDental.UI.TabControl tabControl,string path) {
			int height=Math.Min(tabControl.Height,tabControl.LayoutManager.Scale(20)+12);
			using(Bitmap bitmap=DesignSheetWindows.Capture(tabControl,new Rectangle(0,0,tabControl.Width,height))) {
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				bitmap.Save(path,ImageFormat.Png);
			}
		}
		#endregion Measuring

		#region The list
		public static string GetReport(List<StripRow> listRows) {
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.AppendLine("window\ttabControl\tfont\tsize\twindow px\ttabControl width\tvisible right\ttabs right\toverflow");
			foreach(StripRow stripRow in listRows) {
				stringBuilder.AppendLine(stripRow.Key+"\t"+stripRow.SizeWindow.Width+"x"+stripRow.SizeWindow.Height+"\t"+stripRow.WidthTabControl+"\t"+stripRow.RightVisible+"\t"
					+(stripRow.ListRectangles.Count==0 ? 0 : stripRow.ListRectangles.Max(x => x.Right))+"\t"+string.Join(" | ",stripRow.ListOverflow));
			}
			return stringBuilder.ToString();
		}

		public static Dictionary<string,string> ReadOverflowList() {
			Dictionary<string,string> dict=new Dictionary<string,string>();
			string path=Path.Combine(RepoPaths.Root(),FILE_OVERFLOW);
			if(!File.Exists(path)) {
				return dict;
			}
			foreach(string line in File.ReadAllLines(path)) {
				if(line.Trim()=="" || line.StartsWith("#")) {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				dict[string.Join("\t",arrayFields.Take(4))]=arrayFields.Length>4 ? arrayFields[4] : "";
			}
			return dict;
		}

		public static void WriteOverflowList(List<StripRow> listRows) {
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.Append("# The self-drawn tab strips whose tabs run past the strip's right edge (UI-4b-2, docs/port-notes/UI-4b.md; TabStripFitTests): window,\r\n");
			stringBuilder.Append("# tab control, font (today: Microsoft Sans Serif 8.25; segoe9: UI-2b's Segoe UI 9), size (designer; smallest: cut to a 1024 x 728 desktop),\r\n");
			stringBuilder.Append("# the tabs past the edge. Written by the test with OPMS_UI4B2_WRITE_OVERFLOW=1. For the owner (UQ-9): a tab strip does not wrap or\r\n");
			stringBuilder.Append("# scroll, so a tab past the edge is cut off and cannot be clicked there.\r\n");
			foreach(StripRow stripRow in listRows.Where(x => x.ListOverflow.Count>0)) {
				stringBuilder.Append(stripRow.Key+"\t"+string.Join(" | ",stripRow.ListOverflow)+"\r\n");
			}
			File.WriteAllText(Path.Combine(RepoPaths.Root(),FILE_OVERFLOW),stringBuilder.ToString(),new UTF8Encoding(false));
		}
		#endregion The list

		#region Tests
		[TestMethod]
		public void TabHeaders_KeepTodaysWidthFormula_TextWidthPlus8() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(float scale in new[] { 1f,1.5f,2f }) {
					using(Form form=new Form()) {
						OpenPmsDental.UI.TabControl tabControl=new OpenPmsDental.UI.TabControl();
						tabControl.LayoutManager.SetScaleMS(scale);
						tabControl.Bounds=new Rectangle(0,0,UiTheme.ScaleI(400,scale),UiTheme.ScaleI(120,scale));
						string[] arrayTexts=new string[] { "General","Insurance","Misc","Ortho" };
						foreach(string text in arrayTexts) {
							tabControl.TabPages.Add(new OpenPmsDental.UI.TabPage(text));
						}
						form.Controls.Add(tabControl);
						IntPtr handle=tabControl.Handle;
						tabControl.LayoutTabs();
						using(Graphics g=tabControl.CreateGraphics()) {
							int xExpected=2;
							for(int i=0;i<arrayTexts.Length;i++) {
								//TabControl.LayoutTabs (section 3.3): the text's width in the control's font, scaled by the Windows part of the scale, plus 8 px scaled.
								int widthExpected=(int)tabControl.LayoutManager.ScaleMS(g.MeasureString(arrayTexts[i],tabControl.Font).Width)+tabControl.LayoutManager.Scale(8);
								Rectangle rectangle=tabControl.GetTabRect(i);
								if(rectangle.Width!=widthExpected || rectangle.X!=xExpected) {
									listFailures.Add("Scale "+scale+", tab "+arrayTexts[i]+": "+rectangle+", expected x "+xExpected+" and width "+widthExpected+".");
								}
								xExpected+=widthExpected;
							}
						}
					}
				}
			});
			Assert.AreEqual(0,listFailures.Count,string.Join("\r\n",listFailures));
		}

		[TestMethod]
		public void TabStrips_FourWindows_KeepTheirOrder_AndFit_OrTheOverflowIsListed() {
			string folderPictures=Environment.GetEnvironmentVariable(VARIABLE_PICTURES);
			List<StripRow> listRows=null;
			StaRunner.Run(() => listRows=MeasureAll(folderPictures));
			string report=GetReport(listRows);
			Console.WriteLine(report);
			if(!string.IsNullOrWhiteSpace(folderPictures)) {
				Directory.CreateDirectory(folderPictures);
				File.WriteAllText(Path.Combine(folderPictures,"tab-fit.tsv"),report,new UTF8Encoding(false));
			}
			//Each window has the tabs section 7 counts, in both fonts and at both sizes.
			foreach(Tuple<Type,int> tupleWindow in ArrayWindows) {
				foreach(var group in listRows.Where(x => x.Window==tupleWindow.Item1.Name).GroupBy(x => x.Font+" "+x.SizeName)) {
					Assert.AreEqual(tupleWindow.Item2,group.Sum(x => x.ListTexts.Count),tupleWindow.Item1.Name+" "+group.Key+": the self-drawn tabs.");
				}
			}
			//Order: the first tab at x 2, each next one where the one before it ends, in the designer's order, none without width.
			foreach(StripRow stripRow in listRows) {
				for(int i=0;i<stripRow.ListRectangles.Count;i++) {
					int xExpected=i==0 ? 2 : stripRow.ListRectangles[i-1].Right;
					Assert.AreEqual(xExpected,stripRow.ListRectangles[i].X,stripRow.Key.Replace("\t"," ")+": tab "+i+" ("+stripRow.ListTexts[i]+") is out of order.");
					Assert.IsTrue(stripRow.ListRectangles[i].Width>0,stripRow.Key.Replace("\t"," ")+": tab "+i+" has no width.");
				}
			}
			//Segoe UI 9 is wider than today's font: each tab grows (UQ-9), so the strips are measured in the new font for real.
			foreach(StripRow stripRow in listRows.Where(x => x.Font=="segoe9")) {
				StripRow stripRowToday=listRows.Single(x => x.Font=="today" && x.Window==stripRow.Window && x.TabControl==stripRow.TabControl && x.SizeName==stripRow.SizeName);
				Assert.IsTrue(stripRow.ListRectangles.Last().Right>stripRowToday.ListRectangles.Last().Right,stripRow.Key.Replace("\t"," ")
					+": the strip is not wider in Segoe UI 9 than in today's font.");
			}
			if(ProjectFileTests.BuildOutput.IsRequiredValue(Environment.GetEnvironmentVariable(VARIABLE_WRITE))) {
				WriteOverflowList(listRows);
			}
			Dictionary<string,string> dictListed=ReadOverflowList();
			List<string> listProblems=new List<string>();
			foreach(StripRow stripRow in listRows) {
				string overflow=string.Join(" | ",stripRow.ListOverflow);
				string listed;
				bool isListed=dictListed.TryGetValue(stripRow.Key,out listed);
				if(stripRow.ListOverflow.Count>0 && (!isListed || listed!=overflow)) {
					listProblems.Add(stripRow.Key.Replace("\t"," ")+": past the right edge: "+overflow+(isListed ? " (listed: "+listed+")" : " (not in the list)"));
				}
				if(stripRow.ListOverflow.Count==0 && isListed) {
					listProblems.Add(stripRow.Key.Replace("\t"," ")+": fits now, but the list says "+listed);
				}
			}
			Assert.AreEqual(0,listProblems.Count,FILE_OVERFLOW+" does not match ("+VARIABLE_WRITE+"=1 rewrites it):\r\n"+string.Join("\r\n",listProblems));
		}
		#endregion Tests
	}
}
