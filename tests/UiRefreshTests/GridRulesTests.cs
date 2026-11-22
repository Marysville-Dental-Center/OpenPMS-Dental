// UI-12 (the layout grid, docs/port-notes/UI-12.md): the grid's rules on the windows of Data\grid-windows.tsv, built off screen at the program's
// font. On the windows the planner lays out (mode "tool"): no two siblings overlap (stacked controls the code switches by Visible excepted), a
// label sits 4 to 8 px before the field it names, nothing covers a group box's title, every check box has its box on the left, controls keep the
// container's padding. Every window of the list (tool and hand): no check box the owner's review named keeps its box on the right. A finding the
// planner itself lists (it reports what it could not place) is in Data\grid-known.tsv with the reason; the test fails on any other, and on a
// known row that no longer happens, so the list only shrinks. The rollout slices add their windows to grid-windows.tsv.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using UiRefreshTests.Scan;
using UiRefreshTests.TextFit;

namespace UiRefreshTests {
	[TestClass]
	public class GridRulesTests {
		///<summary>A grid window: its type's full name and its mode (tool or hand).</summary>
		public class GridWindow {
			public string Window="";
			public string Mode="";
		}

		public static List<GridWindow> ReadGridWindows() {
			List<GridWindow> listWindows=new List<GridWindow>();
			string[] arrayLines=File.ReadAllLines(RepoPaths.DataFile("grid-windows.tsv"));
			Assert.AreEqual("window\tslice\tmode\tnote",arrayLines[0],"grid-windows.tsv's header.");
			foreach(string line in arrayLines.Skip(1)) {
				if(line.Trim()=="" || line.StartsWith("#")) {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				Assert.AreEqual(4,arrayFields.Length,"grid-windows.tsv needs window, slice, mode and note: "+line);
				Assert.IsTrue(arrayFields[2]=="tool" || arrayFields[2]=="hand","grid-windows.tsv's mode is tool or hand: "+line);
				listWindows.Add(new GridWindow { Window=arrayFields[0],Mode=arrayFields[2] });
			}
			return listWindows;
		}

		///<summary>The known findings: window, rule, the finding's key, and why.</summary>
		private static HashSet<string> ReadKnown() {
			HashSet<string> hashSetKnown=new HashSet<string>();
			foreach(string line in File.ReadAllLines(RepoPaths.DataFile("grid-known.tsv"))) {
				if(line.Trim()=="" || line.StartsWith("#") || line.StartsWith("window\t")) {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				hashSetKnown.Add(arrayFields[0]+"\t"+arrayFields[1]+"\t"+arrayFields[2]);
			}
			return hashSetKnown;
		}

		[TestMethod]
		public void GridWindows_DataFilesAgree() {
			List<GridWindow> listWindows=ReadGridWindows();
			Assert.IsTrue(listWindows.Count>=47,"The pilot's 47 windows are in grid-windows.tsv.");
			HashSet<string> hashSetWindows=new HashSet<string>(listWindows.Select(x => x.Window));
			foreach(string line in File.ReadAllLines(RepoPaths.DataFile("grid-hand.tsv")).Skip(1)) {
				if(line.Trim()=="") {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				Assert.AreEqual(7,arrayFields.Length,"grid-hand.tsv: designer file, window, control, property, old, new, rule: "+line);
				Assert.IsTrue(hashSetWindows.Contains(arrayFields[1]),"grid-hand.tsv names a window grid-windows.tsv does not list: "+line);
				Assert.IsTrue(File.Exists(Path.Combine(RepoPaths.Root(),arrayFields[0].Replace('/','\\'))),"grid-hand.tsv's designer file: "+line);
			}
		}

		[TestMethod]
		public void GridWindows_FollowTheRules() {
			List<GridWindow> listWindows=ReadGridWindows();
			HashSet<string> hashSetKnown=ReadKnown();
			List<string> listFindings=new List<string>();
			List<string> listFailed=new List<string>();
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
				foreach(GridWindow gridWindow in listWindows) {
					ScanType scanType=listScanTypes.FirstOrDefault(x => x.WindowKey==gridWindow.Window);
					if(scanType==null) {
						listFailed.Add(gridWindow.Window+": not a scanned type");
						continue;
					}
					FitGallery.SetGalleryFont("program");
					try {
						using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
							Control root=builtWindow.Root;
							if(root is FormODBase formODBase && formODBase.PanelClient!=null) {
								root=formODBase.PanelClient;
							}
							CheckWindow(gridWindow,root,listFindings);
							if(gridWindow.Mode=="tool") {
								CheckColumns(gridWindow,root,listFindings);
							}
						}
					}
					catch(Exception exception) {
						listFailed.Add(gridWindow.Window+": "+TierOneScan.Describe(exception));
					}
					finally {
						UiTheme.SetFontForTest(null);
						DefaultFontOverride.Set(TextFitFontOption.Today,1f);
					}
				}
			});
			Assert.AreEqual(0,listFailed.Count,"Windows that could not be built: "+string.Join("; ",listFailed));
			List<string> listNew=listFindings.Where(x => !hashSetKnown.Contains(x)).ToList();
			List<string> listGone=hashSetKnown.Where(x => !listFindings.Contains(x)).ToList();
			Assert.AreEqual(0,listNew.Count,"Grid findings not in grid-known.tsv:"+Environment.NewLine+string.Join(Environment.NewLine,listNew));
			Assert.AreEqual(0,listGone.Count,"grid-known.tsv rows that no longer happen (remove them):"+Environment.NewLine+string.Join(Environment.NewLine,listGone));
		}

		///<summary>Adds the window's findings as "window TAB rule TAB key".</summary>
		private static void CheckWindow(GridWindow gridWindow,Control root,List<string> listFindings) {
			string window=gridWindow.Window;
			foreach(Control container in Walk(root)) {
				List<Control> listKids=container.Controls.Cast<Control>()
					.Where(x => x.Dock==DockStyle.None && TextFitBoundsRecord.IsDesignedVisible(x) && !(container is FlowLayoutPanel) && !(container is TableLayoutPanel))
					.ToList();
				if(container is System.Windows.Forms.TabControl || container is SplitContainer || TextFitMeasurer.IsOfType(container,"OpenPmsDental.UI.TabControl")) {
					continue;
				}
				//Check boxes and radio buttons: the box on the left, in every grid window (the owner's decision of 2026-11-21); right to left
				//(RightToLeft.Yes) puts it on the right too.
				foreach(Control control in listKids) {
					if(control.GetType().GetProperty("CheckAlign",typeof(ContentAlignment)) is System.Reflection.PropertyInfo propertyInfo
						&& ((ContentAlignment)propertyInfo.GetValue(control)==ContentAlignment.MiddleRight || control.RightToLeft==RightToLeft.Yes)
						&& !string.IsNullOrWhiteSpace(control.Text))
					{
						if(gridWindow.Mode=="tool") {
							listFindings.Add(window+"\tcheck-right\t"+control.Name);
						}
					}
				}
				if(gridWindow.Mode!="tool") {
					continue;
				}
				//Siblings that overlap (by their text's extent for labels and check boxes), stacked controls excepted.
				for(int i=0;i<listKids.Count;i++) {
					for(int j=i+1;j<listKids.Count;j++) {
						Rectangle a=GetInk(listKids[i]);
						Rectangle b=GetInk(listKids[j]);
						Rectangle rectangleCross=Rectangle.Intersect(a,b);
						if(rectangleCross.Width<=2 || rectangleCross.Height<=2) {
							continue;
						}
						if(IsStacked(a,b)) {
							continue;
						}
						string[] arrayNames=new string[] { listKids[i].Name,listKids[j].Name }.OrderBy(x => x,StringComparer.Ordinal).ToArray();
						listFindings.Add(window+"\toverlap\t"+arrayNames[0]+" "+arrayNames[1]);
					}
				}
				//A group box's title: nothing starts above the title band's bottom.
				if(!string.IsNullOrEmpty(container.Text) && (container is System.Windows.Forms.GroupBox || TextFitMeasurer.IsOfType(container,"OpenPmsDental.UI.GroupBox"))) {
					int titleBottom=1+TextFitMeasurer.MeasureLineGdiPlus(container.Text,container.Font,StringFormat.GenericDefault).Height;
					foreach(Control control in listKids) {
						if(control.Top<titleBottom-2) {
							listFindings.Add(window+"\ttitle\t"+container.Name+" "+control.Name);
						}
					}
				}
				//A group box's content keeps the padding: nothing runs past its sides or its bottom.
				if(container is System.Windows.Forms.GroupBox || TextFitMeasurer.IsOfType(container,"OpenPmsDental.UI.GroupBox")) {
					foreach(Control control in listKids) {
						Rectangle rectangleInk=GetInk(control);
						if(rectangleInk.Left<4 || rectangleInk.Right>container.Width-4 || control.Bottom>container.Height-2) {
							listFindings.Add(window+"\tpadding\t"+container.Name+" "+control.Name);
						}
					}
				}
				//A label right-aligned before a single-line field on its row: 4 to 8 px before it.
				foreach(Control label in listKids.Where(x => x is Label && x.Text!="")) {
					if(!(label is Label labelStock) || (labelStock.TextAlign!=ContentAlignment.MiddleRight && labelStock.TextAlign!=ContentAlignment.TopRight)) {
						continue;
					}
					Control field=listKids.Where(x => IsSingleLineField(x) && Math.Abs(CenterY(x)-CenterY(label))<=4 && x.Left>=label.Right-8 && x.Left-label.Right<=40)
						.OrderBy(x => x.Left).FirstOrDefault();
					if(field==null) {
						continue;
					}
					int gap=field.Left-label.Right;
					if(gap<4 || gap>8) {
						listFindings.Add(window+"\tlabel-gap\t"+label.Name+" "+field.Name);
					}
				}
			}
		}

		///<summary>Fields named by a label on their row share one left edge with the fields of other rows, group boxes and tab pages whose left edge
		///is within 40 px of theirs (window coordinates): the grid's field columns. A Preferences page has one field column.</summary>
		private static void CheckColumns(GridWindow gridWindow,Control root,List<string> listFindings) {
			List<Tuple<string,int>> listFields=new List<Tuple<string,int>>();
			foreach(Control container in Walk(root)) {
				if(container is FlowLayoutPanel || container is TableLayoutPanel || container is SplitContainer) {
					continue;
				}
				List<Control> listKids=container.Controls.Cast<Control>().Where(x => x.Dock==DockStyle.None && TextFitBoundsRecord.IsDesignedVisible(x)).ToList();
				foreach(Control label in listKids.Where(x => x is Label && !string.IsNullOrWhiteSpace(x.Text))) {
					Control field=listKids.Where(x => IsSingleLineField(x) && Math.Abs(CenterY(x)-CenterY(label))<=4 && GetInk(x).Left>=label.Right-8
						&& GetInk(x).Left-label.Right<=40).OrderBy(x => GetInk(x).Left).FirstOrDefault();
					if(field==null) {
						continue;
					}
					Point point=root.PointToClient(container.PointToScreen(new Point(GetInk(field).Left,0)));
					listFields.Add(Tuple.Create(field.Name,point.X));
				}
			}
			bool isPreferencesPage=gridWindow.Window.Substring(gridWindow.Window.LastIndexOf('.')+1).StartsWith("UserControl");
			if(isPreferencesPage && listFields.Count>1) {
				int xPage=listFields.GroupBy(x => x.Item2).OrderByDescending(x => x.Count()).ThenByDescending(x => x.Key).First().Key;
				foreach(Tuple<string,int> tuple in listFields.Where(x => x.Item2!=xPage)) {
					listFindings.Add(gridWindow.Window+"	column	"+tuple.Item1);
				}
				return;
			}
			List<Tuple<string,int>> listSorted=listFields.OrderBy(x => x.Item2).ToList();
			int i=0;
			while(i<listSorted.Count) {
				List<Tuple<string,int>> listCluster=listSorted.Skip(i).TakeWhile(x => x.Item2-listSorted[i].Item2<=40).ToList();
				int xMax=listCluster.Max(x => x.Item2);
				foreach(Tuple<string,int> tuple in listCluster.Where(x => x.Item2!=xMax)) {
					listFindings.Add(gridWindow.Window+"	column	"+tuple.Item1);
				}
				i+=listCluster.Count;
			}
		}

		private static IEnumerable<Control> Walk(Control control) {
			yield return control;
			foreach(Control child in control.Controls) {
				foreach(Control control2 in Walk(child)) {
					yield return control2;
				}
			}
		}

		private static int CenterY(Control control) {
			return control.Top+control.Height/2;
		}

		private static bool IsSingleLineField(Control control) {
			string name=control.GetType().Name;
			if(control is TextBoxBase textBoxBase && textBoxBase.Multiline) {
				return false;
			}
			return control.Height<=30 && (control is TextBoxBase || control is System.Windows.Forms.ComboBox || control is DateTimePicker || control is UpDownBase
				|| name.Contains("ComboBox") || name.Contains("DatePicker"));
		}

		///<summary>Stacked on (nearly) the same bounds: controls the code switches by Visible.</summary>
		private static bool IsStacked(Rectangle a,Rectangle b) {
			Rectangle rectangleCross=Rectangle.Intersect(a,b);
			long areaSmall=Math.Min((long)a.Width*a.Height,(long)b.Width*b.Height);
			long areaLarge=Math.Max((long)a.Width*a.Height,(long)b.Width*b.Height);
			//Controls switched at run time stand on about the same bounds; a small box inside a large list is an overlap (round 2: the Age box in
			//FormPatientEdit's Family Relationships list passed as stacked).
			return areaSmall>0 && (long)rectangleCross.Width*rectangleCross.Height>=0.6*areaSmall && areaSmall>=0.4*areaLarge;
		}

		///<summary>The part of the bounds the control draws in: a label's or check box's text extent on the side it is aligned to.</summary>
		private static Rectangle GetInk(Control control) {
			Rectangle rectangle=control.Bounds;
			//A date picker's box sits inside its bounds (its own layout); a clinic picker draws its own label before its box.
			if(control is OpenPmsDental.UI.ODDatePicker odDatePicker && odDatePicker.Controls.Find("textDate",false).FirstOrDefault() is Control controlDate) {
				return new Rectangle(rectangle.X+controlDate.Left,rectangle.Y,controlDate.Width,rectangle.Height);
			}
			if(control is OpenPmsDental.UI.ComboBoxClinicPicker comboBoxClinicPicker && comboBoxClinicPicker.ShowLabel) {
				int widthLabel=(int)Math.Round(37*comboBoxClinicPicker.DeviceDpi/96f);
				return new Rectangle(rectangle.X+widthLabel,rectangle.Y,rectangle.Width-widthLabel,rectangle.Height);
			}
			if(string.IsNullOrWhiteSpace(control.Text)) {
				return rectangle;
			}
			if(control is Label label) {
				int width=TextFitMeasurer.MeasureLineGdi(label.Text,label.Font,TextFitMeasurer.GetStockFlags(label)).Width+4;
				if(width>=rectangle.Width) {
					return rectangle;
				}
				string align=label.TextAlign.ToString();
				if(align.EndsWith("Right")) {
					return new Rectangle(rectangle.Right-width,rectangle.Y,width,rectangle.Height);
				}
				if(align.EndsWith("Center")) {
					return new Rectangle(rectangle.X+(rectangle.Width-width)/2,rectangle.Y,width,rectangle.Height);
				}
				return new Rectangle(rectangle.X,rectangle.Y,width,rectangle.Height);
			}
			if(TextFitMeasurer.IsOfType(control,TextFitMeasurer.TYPE_UI_CHECKBOX)) {
				using(StringFormat stringFormat=new StringFormat(StringFormat.GenericDefault)) {
					int width=16+TextFitMeasurer.MeasureLineGdiPlus(control.Text,control.Font,stringFormat).Width+2;
					if(width<rectangle.Width) {
						return new Rectangle(rectangle.X,rectangle.Y,width,rectangle.Height);
					}
				}
			}
			return rectangle;
		}
	}
}
