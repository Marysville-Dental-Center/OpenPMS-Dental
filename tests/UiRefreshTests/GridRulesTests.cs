// UI-12 (the layout grid, docs/port-notes/UI-12.md): the grid's rules on the windows of Data\grid-windows.tsv, built off screen at the program's
// font. On the windows the planner lays out (mode "tool"): no two siblings overlap (stacked controls the code switches by Visible excepted), a
// label sits 4 to 8 px before the field it names, nothing covers a group box's title, every check box has its box on the left, controls keep the
// container's padding. Every window of the list (tool and hand): no check box the owner's review named keeps its box on the right. A finding the
// planner itself lists (it reports what it could not place) is in Data\grid-known.tsv with the reason; the test fails on any other, and on a
// known row that no longer happens, so the list only shrinks. The rollout slices add their windows to grid-windows.tsv.
// Round 3 (the owner's addition of 2026-11-21): every rule in every window of the list, the windows laid out by layout scripts too; no text past
// its container's edge; consecutive fields, check boxes and buttons of a column one gap apart.
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
			///<summary>The rollout slice whose grid-windows file holds the row (pilot, 1, 2, ...).</summary>
			public string Slice="";
		}

		public static List<GridWindow> ReadGridWindows() {
			List<GridWindow> listWindows=new List<GridWindow>();
			//UI-12T: one file per slice, Data\grid-windows\<slice>.tsv; the slice column names the file the row is in.
			foreach(SliceLine sliceLine in SliceData.ReadLines(SliceData.Folder("grid-windows"),"window\tslice\tmode\tnote")) {
				string line=sliceLine.Line;
				string[] arrayFields=sliceLine.ArrayFields;
				Assert.AreEqual(4,arrayFields.Length,"grid-windows needs window, slice, mode and note: "+line);
				Assert.AreEqual(sliceLine.Slice,arrayFields[1],"grid-windows: a row is in its slice's file: "+line);
				Assert.IsTrue(arrayFields[2]=="tool" || arrayFields[2]=="hand","grid-windows's mode is tool or hand: "+line);
				listWindows.Add(new GridWindow { Window=arrayFields[0],Mode=arrayFields[2],Slice=arrayFields[1] });
			}
			return listWindows;
		}

		///<summary>The known findings: window, rule, the finding's key, and why.</summary>
		private static HashSet<string> ReadKnown() {
			HashSet<string> hashSetKnown=new HashSet<string>();
			foreach(SliceLine sliceLine in SliceData.ReadLines(SliceData.Folder("grid-known"),"window\trule\tkey\twhy")) {
				string[] arrayFields=sliceLine.ArrayFields;
				hashSetKnown.Add(arrayFields[0]+"\t"+arrayFields[1]+"\t"+arrayFields[2]);
			}
			return hashSetKnown;
		}

		///<summary>UI-12 slice 1 (fix round 8): the texts a window's code gives a label or check box at run time (Data\grid-runtime-texts.tsv:
		///window, control, text with \n for a line break, its width on one line at Segoe UI 9, why). The layout scripts size the control for the
		///widest of its texts (grid_hand.py's Hand.lw); the test sets each text and measures it in the laid-out control.</summary>
		public static List<string[]> ReadRuntimeTexts() {
			List<string[]> listRows=new List<string[]>();
			foreach(SliceLine sliceLine in SliceData.ReadLines(SliceData.Folder("grid-runtime-texts"),"window\tcontrol\ttext\twidth\twhy")) {
				Assert.AreEqual(5,sliceLine.ArrayFields.Length,"grid-runtime-texts needs window, control, text, width and why: "+sliceLine.Line);
				listRows.Add(sliceLine.ArrayFields);
			}
			return listRows;
		}

		///<summary>UI-12 slice 1 (fix round 8): no covered text (the owner's addition of 2026-11-21) in the grid windows, measured as the text-fit
		///scan measures it (TextFitMeasurer at the program's font): every label and check box or radio button the designer shows holds its text,
		///on one line or wrapped in its height (a check box of three lines kept three lines tall: FormFamilyBalancer's Changed Since check box);
		///and every text of grid-runtime-texts.tsv fits the control the code gives it to (FormClinicEdit's Canadian address labels,
		///FormScheduleDayEdit's two-line day).</summary>
		private static void CheckText(GridWindow gridWindow,Control root,List<string[]> listRuntimeTexts,List<string> listFindings) {
			string window=gridWindow.Window;
			foreach(Control control in Walk(root)) {
				if(control==root || !(control is Label || IsCheck(control)) || string.IsNullOrWhiteSpace(control.Text) || control.Width<=0 || control.Height<=0) {
					continue;
				}
				if(control.Dock!=DockStyle.None || !TextFitBoundsRecord.IsDesignedVisible(control) || control.Name=="") {
					continue;
				}
				if(IsCut(control,out string detail)) {
					listFindings.Add(window+"\ttext\t"+control.Name);
					ListTextDetails.Add(window+" "+control.Name+" "+detail);
				}
			}
			foreach(string[] arrayRow in listRuntimeTexts.Where(x => x[0]==window)) {
				Control control=Walk(root).FirstOrDefault(x => x.Name==arrayRow[1]);
				Assert.IsNotNull(control,"grid-runtime-texts.tsv names a control the window does not have: "+string.Join(" ",arrayRow.Take(2)));
				string textDesigner=control.Text;
				try {
					control.Text=arrayRow[2].Replace("\\n","\r\n");
					int width=TextFitMeasurer.MeasureLineGdi(control.Text.Split(new string[] { "\r\n" },StringSplitOptions.None).OrderByDescending(x => x.Length).First(),
						control.Font,TextFitMeasurer.GetStockFlags(control)).Width;
					if(Math.Abs(width-int.Parse(arrayRow[3]))>2 && !control.Text.Contains("\r\n")) {
						listFindings.Add(window+"\truntime-width\t"+control.Name+" measures "+width);
					}
					if(IsCut(control,out string detail)) {
						listFindings.Add(window+"\truntime-text\t"+control.Name);
						ListTextDetails.Add(window+" "+control.Name+" (runtime) "+detail);
					}
				}
				finally {
					control.Text=textDesigner;
				}
			}
		}

		///<summary>The control's text does not fit it. A label as the text-fit scan measures it (TextFitMeasurer): wrapped lines that need more
		///height than it has, or a line 3 px or more too wide or too tall for it (1-2 px is the measure's rounding, the scan's "fixable" class).
		///A check box or radio button: its text wrapped as it draws it (the program's check box: GDI+ in its width less the 12 px box and 3 px;
		///a stock one: TextRenderer in its width less what its preferred size adds to the text, 2 px of rounding allowed as for labels), its line
		///breaks kept; two lines or more that
		///its height does not hold are cut (one line is the grid's 18 and 20 px row, whatever the measure's line spacing). The scan's own
		///classification measures a text with line breaks on one line, so it would read FormFamilyBalancer's three-line check box as too wide
		///rather than too short.</summary>
		private static bool IsCut(Control control,out string detail) {
			if(IsCheck(control)) {
				Size sizeWrapped;
				Size sizeLine;
				if(TextFitMeasurer.IsOfType(control,TextFitMeasurer.TYPE_UI_CHECKBOX)) {
					using(StringFormat stringFormat=new StringFormat()) {
						sizeWrapped=TextFitMeasurer.MeasureWrappedGdiPlus(control.Text,control.Font,control.Width-12-3,stringFormat);
						sizeLine=TextFitMeasurer.MeasureWrappedGdiPlus("Ag",control.Font,1000,stringFormat);
					}
				}
				else {
					TextFormatFlags flags=TextFitMeasurer.GetStockFlags(control);
					int widthExtra=Math.Max(0,control.GetPreferredSize(Size.Empty).Width-TextFitMeasurer.MeasureLineGdi(control.Text,control.Font,flags).Width);
					sizeWrapped=TextFitMeasurer.MeasureWrappedGdi(control.Text,control.Font,control.Width-widthExtra+2,flags);
					sizeLine=TextFitMeasurer.MeasureLineGdi("Ag",control.Font,flags);
				}
				int countLines=(int)Math.Round(sizeWrapped.Height/(double)Math.Max(1,sizeLine.Height));
				detail="wrapped "+sizeWrapped.Width+"x"+sizeWrapped.Height+" ("+countLines+" lines of "+sizeLine.Height+") in "+control.Width+"x"+control.Height;
				return countLines>=2 && sizeWrapped.Height>control.Height+3;
			}
			TextFitResult result=TextFitMeasurer.Measure(control,1f);
			detail=result.Kind+" "+control.Width+"x"+control.Height+" needs +"+result.NeedWidth+" x +"+result.NeedHeight;
			if(result.Method==TextFitMethod.None) {
				return false;
			}
			return result.Kind==TextFitKind.Line || (result.Kind==TextFitKind.Width && (result.NeedWidth>=3 || result.NeedHeight>=3));
		}

		///<summary>The measurements of the text findings, for the failure message.</summary>
		private static List<string> ListTextDetails=new List<string>();

		///<summary>UI-12 slice 1 (fix round 8): outside Canada FormCarrierEdit's Load hides the CDAnet group and the Is CDAnet Carrier check box and
		///sets the window 560 px wide and their heights less tall (this.Height-groupCDAnet.Height-checkIsCDAnet.Height). Every other control, the
		///Carrier Group row Load shows when a CarrierGroupNames definition exists too, stays inside the smaller client area above the bottom bar
		///(the bar is anchored to the bottom and moves up with it).</summary>
		[TestMethod]
		public void FormCarrierEdit_OutsideCanada_EveryControlFits() {
			List<string> listProblems=new List<string>();
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				ScanType scanType=WindowBuilder.GetScanTypes().First(x => x.WindowKey=="OpenPmsDental.FormCarrierEdit");
				FitGallery.SetGalleryFont("program");
				try {
					using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
						Control root=((FormODBase)builtWindow.Root).PanelClient;
						Control groupCDAnet=root.Controls["groupCDAnet"];
						Control checkIsCDAnet=root.Controls["checkIsCDAnet"];
						int heightClient=root.Height-groupCDAnet.Height-checkIsCDAnet.Height;
						int widthClient=560-(builtWindow.Form.Width-root.Width);
						Assert.IsTrue(widthClient>500 && widthClient<=560,"The client width at 560 px: "+widthClient);
						//The bottom bar (anchored to the bottom in the designer; LayoutManager keeps it on the client area's bottom).
						List<Control> listBar=new List<Control> { root.Controls["butSave"],root.Controls["butDelete"] };
						Assert.IsTrue(listBar.All(x => x!=null),"Save and Delete stand on the window.");
						int yBar=listBar.Min(x => x.Top)-(root.Height-heightClient);
						foreach(Control control in root.Controls) {
							if(control==groupCDAnet || control==checkIsCDAnet || listBar.Contains(control)) {
								continue;
							}
							if(!TextFitBoundsRecord.IsDesignedVisible(control) && control.Name!="labelCarrierGroupName" && control.Name!="comboCarrierGroupName") {
								continue;
							}
							if(control.Bottom>yBar-UiGrid.PAD || control.Right>widthClient-UiGrid.PAD) {
								listProblems.Add(control.Name+" "+control.Bounds+" (client "+widthClient+" x "+heightClient+", bar at "+yBar+")");
							}
						}
						//Save moves left with the window's right edge (anchored right): it stays right of Delete.
						if(listBar[0].Left-(root.Width-widthClient)<listBar[1].Right+UiGrid.DIALOG_GAP) {
							listProblems.Add("butSave over butDelete");
						}
					}
				}
				finally {
					UiTheme.SetFontForTest(null);
					DefaultFontOverride.Set(TextFitFontOption.Today,1f);
				}
			});
			Assert.AreEqual(0,listProblems.Count,"Controls outside FormCarrierEdit's client area outside Canada: "+string.Join("; ",listProblems));
		}

		///<summary>UI-12 slice 1 fix round 9: the clinic picker draws its label ("Clinic", "Clinics") as a right-aligned WinForms Label draws its
		///text, so in a window where it stands in a column of labels its text ends on the labels' ink edge. Drawn as the gallery draws the window
		///(DesignSheetWindows.Capture), the picker's rightmost ink pixel left of its box is within 1 px of the nearest one of the right-aligned
		///labels whose bounds end where its label area does (6 px before its box). Before this round its paint left out TextBoxControl, so
		///TextRenderer added its side padding and the text ended 3-4 px short of the column (FormPayment 92 against 95-96).</summary>
		[TestMethod]
		public void ClinicPicker_LabelInk_OnTheLabelColumn() {
			string[] arrayWindows=new string[] { "OpenPmsDental.FormPayment","OpenPmsDental.FormAdjust","OpenPmsDental.FormPaySplitEdit","OpenPmsDental.FormRxEdit" };
			List<string> listProblems=new List<string>();
			List<string> listMeasured=new List<string>();
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
				foreach(string window in arrayWindows) {
					ScanType scanType=listScanTypes.First(x => x.WindowKey==window);
					FitGallery.SetGalleryFont("program");
					try {
						using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
							Control root=builtWindow.Root;
							if(root is FormODBase formODBase && formODBase.PanelClient!=null) {
								root=formODBase.PanelClient;
							}
							FitGallery.SettleAfterHandles(builtWindow);
							FitGallery.HideDatePickerCalendars(root);
							List<OpenPmsDental.UI.ComboBoxClinicPicker> listPickers=Walk(root).OfType<OpenPmsDental.UI.ComboBoxClinicPicker>()
								.Where(x => x.ShowLabel && TextFitBoundsRecord.IsDesignedVisible(x)).ToList();
							using(Bitmap bitmap=DesignSheetWindows.Capture(root,Rectangle.Empty)) {
								int countCompared=0;
								foreach(OpenPmsDental.UI.ComboBoxClinicPicker picker in listPickers) {
									int xLabelEnd=picker.Left+picker.WidthLabelArea-(int)Math.Round(UiGrid.LABEL_GAP*picker.DeviceDpi/96f);
									List<Label> listLabels=picker.Parent.Controls.OfType<Label>()
										.Where(x => TextFitBoundsRecord.IsDesignedVisible(x) && x.TextAlign.ToString().EndsWith("Right") && x.RightToLeft!=RightToLeft.Yes
											&& !string.IsNullOrWhiteSpace(x.Text) && Math.Abs(x.Right-xLabelEnd)<=1)
										.ToList();
									if(listLabels.Count==0) {
										continue;
									}
									Point pointPicker=GetOffset(picker,root);
									int inkPicker=GetInkRight(bitmap,new Rectangle(pointPicker.X,pointPicker.Y,picker.WidthLabelArea,picker.Height));
									List<int> listInkLabels=listLabels.Select(x => {
										Point point=GetOffset(x,root);
										return GetInkRight(bitmap,new Rectangle(point.X,point.Y,x.Width,x.Height));
									}).Where(x => x>=0).ToList();
									if(inkPicker<0 || listInkLabels.Count==0) {
										listProblems.Add(window+" "+picker.Name+": no ink found");
										continue;
									}
									countCompared++;
									int diffNearest=listInkLabels.Min(x => Math.Abs(x-inkPicker));
									listMeasured.Add(window+" "+picker.Name+" ink "+inkPicker+", labels "+string.Join(",",listInkLabels));
									if(diffNearest>1) {
										listProblems.Add(window+" "+picker.Name+": its label's ink ends at "+inkPicker+", the column's labels at "+string.Join(",",listInkLabels));
									}
								}
								if(countCompared==0) {
									listProblems.Add(window+": no clinic picker in a column of right-aligned labels");
								}
							}
						}
					}
					finally {
						UiTheme.SetFontForTest(null);
						DefaultFontOverride.Set(TextFitFontOption.Today,1f);
					}
				}
			});
			Assert.AreEqual(0,listProblems.Count,"Clinic pickers whose label is off the label column: "+string.Join("; ",listProblems)
				+Environment.NewLine+"Measured: "+string.Join("; ",listMeasured));
		}

		///<summary>The control's top left in the root's coordinates (the sum of the locations up the parents; group boxes, panels and tab pages
		///have their client area at their bounds' origin).</summary>
		private static Point GetOffset(Control control,Control root) {
			Point point=Point.Empty;
			for(Control controlAt=control;controlAt!=null && controlAt!=root;controlAt=controlAt.Parent) {
				point.Offset(controlAt.Left,controlAt.Top);
			}
			return point;
		}

		///<summary>The rightmost column of the rectangle with a dark (text) pixel, or -1.</summary>
		private static int GetInkRight(Bitmap bitmap,Rectangle rectangle) {
			rectangle.Intersect(new Rectangle(0,0,bitmap.Width,bitmap.Height));
			for(int x=rectangle.Right-1;x>=rectangle.Left;x--) {
				for(int y=rectangle.Top;y<rectangle.Bottom;y++) {
					Color color=bitmap.GetPixel(x,y);
					if(0.299*color.R+0.587*color.G+0.114*color.B<110) {
						return x;
					}
				}
			}
			return -1;
		}

		[TestMethod]
		public void GridWindows_DataFilesAgree() {
			List<GridWindow> listWindows=ReadGridWindows();
			Assert.IsTrue(listWindows.Count>=47,"The pilot's 47 windows are in grid-windows.tsv.");
			HashSet<string> hashSetWindows=new HashSet<string>(listWindows.Select(x => x.Window));
			foreach(SliceLine sliceLine in SliceData.ReadLines(SliceData.Folder("grid-hand"),"designer_file\twindow\tcontrol\tproperty\told\tnew\trule")) {
				string line=sliceLine.Line;
				string[] arrayFields=sliceLine.ArrayFields;
				Assert.AreEqual(7,arrayFields.Length,"grid-hand.tsv: designer file, window, control, property, old, new, rule: "+line);
				Assert.IsTrue(hashSetWindows.Contains(arrayFields[1]),"grid-hand.tsv names a window grid-windows.tsv does not list: "+line);
				Assert.IsTrue(File.Exists(Path.Combine(RepoPaths.Root(),arrayFields[0].Replace('/','\\'))),"grid-hand.tsv's designer file: "+line);
			}
		}

		///<summary>UI-12 fix round: the input frame's choice per text box. A rich text box (ODtextBox) with a border is framed whether it is
		///single-line or multi-line (FormEmailInbox's address and subject boxes, ControlChart's search box); a single-line stock TextBox keeps the
		///one-line rule (framed with a 3-D border, not with a single line); a borderless box and the text inside an up-down box are not.</summary>
		[TestMethod]
		public void InputFrame_SingleLineODtextBox_IsFramed() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				using(ODtextBox odTextBoxSingle=new ODtextBox { Multiline=false })
				using(ODtextBox odTextBoxMulti=new ODtextBox { Multiline=true })
				using(ODtextBox odTextBoxBorderless=new ODtextBox { Multiline=false,BorderStyle=BorderStyle.None })
				using(RichTextBox richTextBoxSingle=new RichTextBox { Multiline=false,BorderStyle=BorderStyle.FixedSingle })
				using(TextBox textBox3D=new TextBox { BorderStyle=BorderStyle.Fixed3D })
				using(TextBox textBoxSingleLine=new TextBox { BorderStyle=BorderStyle.FixedSingle })
				using(TextBox textBoxMulti=new TextBox { Multiline=true,BorderStyle=BorderStyle.FixedSingle,Height=80 })
				using(NumericUpDown numericUpDown=new NumericUpDown())
				{
					Assert.AreEqual(BorderStyle.Fixed3D,odTextBoxSingle.BorderStyle,"An ODtextBox's border is the RichTextBox's 3-D one.");
					Assert.IsTrue(OpenPmsDental.UI.UiInputFrame.IsFramed(odTextBoxSingle),"A single-line ODtextBox with a border draws the input frame.");
					Assert.IsTrue(OpenPmsDental.UI.UiInputFrame.IsFramed(odTextBoxMulti),"A multi-line ODtextBox draws the input frame.");
					Assert.IsFalse(OpenPmsDental.UI.UiInputFrame.IsFramed(odTextBoxBorderless),"A borderless ODtextBox has no frame.");
					Assert.IsTrue(OpenPmsDental.UI.UiInputFrame.IsFramed(richTextBoxSingle),"A single-line RichTextBox with a border draws the input frame.");
					Assert.IsTrue(OpenPmsDental.UI.UiInputFrame.IsFramed(textBox3D),"A single-line TextBox with a 3-D border draws the input frame.");
					Assert.IsFalse(OpenPmsDental.UI.UiInputFrame.IsFramed(textBoxSingleLine),"A single-line TextBox with a single line keeps the one-line rule.");
					Assert.IsTrue(OpenPmsDental.UI.UiInputFrame.IsFramed(textBoxMulti),"A multi-line TextBox with a border draws the input frame.");
					Assert.IsTrue(OpenPmsDental.UI.UiInputFrame.IsFramed(numericUpDown),"An up-down box draws the input frame.");
					foreach(Control control in numericUpDown.Controls) {
						Assert.IsFalse(OpenPmsDental.UI.UiInputFrame.IsFramed(control),"The parts of an up-down box have their box's frame.");
					}
				}
			});
		}

		[TestMethod]
		public void GridWindows_FollowTheRules() {
			List<GridWindow> listWindows=ReadGridWindows();
			HashSet<string> hashSetKnown=ReadKnown();
			List<string[]> listRuntimeTexts=ReadRuntimeTexts();
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
							CheckColumns(gridWindow,root,listFindings);
							CheckText(gridWindow,root,listRuntimeTexts,listFindings);
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
			Assert.AreEqual(0,listNew.Count,"Grid findings not in grid-known.tsv:"+Environment.NewLine+string.Join(Environment.NewLine,listNew)
				+Environment.NewLine+"Text measured:"+Environment.NewLine+string.Join(Environment.NewLine,ListTextDetails));
			Assert.AreEqual(0,listGone.Count,"grid-known.tsv rows that no longer happen (remove them):"+Environment.NewLine+string.Join(Environment.NewLine,listGone));
		}

		///<summary>Adds the window's findings as "window TAB rule TAB key".</summary>
		private static void CheckWindow(GridWindow gridWindow,Control root,List<string> listFindings) {
			string window=gridWindow.Window;
			foreach(Control container in Walk(root)) {
				if(container is OpenPmsDental.UI.ODDatePicker || container is OpenPmsDental.UI.ComboBoxClinicPicker) {
					continue;//they lay out their own parts
				}
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
				//Round 3: the rules below hold in every grid window, the windows laid out by their layout scripts (mode hand) too; the parts a hand
				//window keeps as on main are in grid-known.tsv with the reason.
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
				//No text runs past its container's edge (the owner's addition of 2026-11-21: "Text shouldn't be covered by other elements"): a label's
				//or a check box's text extent inside the container's client area. A panel that scrolls holds what runs past its edge.
				if(!(container is ScrollableControl scrollableControl && scrollableControl.AutoScroll)) {
					foreach(Control control in listKids.Where(x => (x is Label || IsCheck(x)) && !string.IsNullOrWhiteSpace(x.Text) && x.Width>0 && x.Height>0)) {
						Rectangle rectangleInk=GetInk(control);
						if(rectangleInk.Left<0 || rectangleInk.Top<0 || rectangleInk.Right>container.ClientSize.Width || rectangleInk.Bottom>container.ClientSize.Height) {
							listFindings.Add(window+"	frame	"+container.Name+" "+control.Name);
						}
					}
				}
				//UI-12 fix round: a control hidden in the designer (the window's code shows it in some setups) stays inside its container's client
				//area too, or it is cut or never seen once shown (FormProcEdit's End time box and Now button past panel1's edge).
				//Layout containers only (the window, panels, group boxes, tab pages, UserControls): a list box's or grid's own hidden scroll bars are
				//theirs to place.
				bool isLayoutContainer=container==root || container is Form || container is UserControl || container is Panel || container is System.Windows.Forms.TabPage
					|| container is System.Windows.Forms.GroupBox || TextFitMeasurer.IsOfType(container,"OpenPmsDental.UI.GroupBox") || TextFitMeasurer.IsOfType(container,"OpenPmsDental.UI.TabPage");
				if(isLayoutContainer && !(container is ScrollableControl scrollableControlHidden && scrollableControlHidden.AutoScroll)) {
					foreach(Control control in container.Controls.Cast<Control>().Where(x => x.Dock==DockStyle.None && !TextFitBoundsRecord.IsDesignedVisible(x)
						&& x.Width>0 && x.Height>0 && x.Name!=""))//an unnamed control is a custom control's own part
					{
						if(control.Left<0 || control.Top<0 || control.Right>container.ClientSize.Width || control.Bottom>container.ClientSize.Height) {
							listFindings.Add(window+"\thidden-frame\t"+container.Name+" "+control.Name);
						}
					}
				}
				//Equal gaps (the owner's addition: "Spacing between items should be the same"): consecutive items of one kind stacked in one column of
				//a container (single-line fields, check boxes and radio buttons, buttons) stand one distance apart; a pair closer than a section gap
				//(12 px) whose gap differs from the container's usual one for that kind by more than 1 px is a finding.
				foreach(Func<Control,bool> funcKind in new Func<Control,bool>[] { IsSingleLineField,IsCheck,IsButton }) {
					List<Control> listOfKind=listKids.Where(funcKind).ToList();
					List<Tuple<Control,Control,int>> listPairs=new List<Tuple<Control,Control,int>>();
					foreach(Control controlA in listOfKind) {
						Control controlB=listOfKind.Where(x => x!=controlA && Math.Abs(x.Left-controlA.Left)<=1 && x.Top>=controlA.Bottom-1 && x.Top-controlA.Bottom<12)
							.OrderBy(x => x.Top).FirstOrDefault();
						if(controlB==null) {
							continue;
						}
						//Not when another control of the container stands between them.
						if(listKids.Any(x => x!=controlA && x!=controlB && x.Top>=controlA.Bottom-1 && x.Bottom<=controlB.Top+1 && x.Right>controlA.Left && x.Left<controlA.Right)) {
							continue;
						}
						listPairs.Add(Tuple.Create(controlA,controlB,controlB.Top-controlA.Bottom));
					}
					if(listPairs.Count<2) {
						continue;
					}
					int gapUsual=listPairs.GroupBy(x => x.Item3).OrderByDescending(x => x.Count()).ThenBy(x => x.Key).First().Key;
					foreach(Tuple<Control,Control,int> tuple in listPairs.Where(x => Math.Abs(x.Item3-gapUsual)>1)) {
						listFindings.Add(window+"	gap	"+tuple.Item1.Name+" "+tuple.Item2.Name);
					}
				}
				//UI-12 fix round: every text box with a border draws the one input frame (the owner's addition, "text also"), the single-line
				//rich text boxes too (FormEmailInbox's address boxes and ControlChart's search box are single-line ODtextBoxes and drew plain
				//white bars). A single-line stock TextBox keeps the one-line rule (framed with a 3-D border), and the text inside an up-down box
				//has its box's frame. In a window the harness builds as a form, the frame is attached too.
				foreach(Control control in listKids.Where(x => x is TextBoxBase && !(x.Parent is UpDownBase) && ((TextBoxBase)x).BorderStyle!=BorderStyle.None)) {
					if(control is TextBox textBox && (!textBox.Multiline || OpenPmsDental.UI.UiInputFrame.IsOneLineTall(textBox)) && textBox.BorderStyle!=BorderStyle.Fixed3D) {
						continue;
					}
					if(!OpenPmsDental.UI.UiInputFrame.IsFramed(control) || (root.FindForm()!=null && !OpenPmsDental.UI.UiInputFrame.IsAttached(control))) {
						listFindings.Add(window+"\tunframed\t"+control.Name);
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
					//UI-12 slice 9 fix round: the label's text is centred on its field (the vertical centres within 1 px). The planner centred
					//FormTaskListEdit's Date box on the two-line note beside it and left the label 4 px higher, its text over the box's centre.
					//From slice 9 on: 14 labels of the pilot and slices 1, 2 and 5 stand 2 to 4 px off their fields (UI-12.md, "Slice 9, fix
					//round"), left to those slices' owners; slice 8 ran at the same time.
					if(IsLabelCentreChecked(gridWindow.Slice) && Math.Abs(CenterY(field)-CenterY(label))>1) {
						listFindings.Add(window+"\tlabel-centre\t"+label.Name+" "+field.Name);
					}
				}
				//UI-12 slice 1 (fix round 8): a clinic picker under a field of a column (the next row, its box within 40 px of the field's left edge)
				//has its box on that column: the same left edge, and under a combo box of about its width (right edges within 16 px, the grid's edge
				//snap) the same right edge too (FormRpDPPOvercharged's and FormRpProcOverpaid's Clinics under Providers). A picker that ends on the
				//picker button after the combo box (FormAdjust) or well past it is a wider field on the column. The column check does not see a
				//picker: it names itself, no label stands before it.
				foreach(Control picker in listKids.Where(x => x is OpenPmsDental.UI.ComboBoxClinicPicker comboBoxClinicPicker && comboBoxClinicPicker.ShowLabel)) {
					Rectangle rectangleBox=GetBox(picker);
					Control fieldAbove=listKids.Where(x => x!=picker && IsSingleLineField(x) && x.Bottom<=picker.Top+1 && picker.Top-x.Bottom<12
						&& Math.Abs(GetBox(x).Left-rectangleBox.Left)<=40).OrderByDescending(x => x.Bottom).FirstOrDefault();
					if(fieldAbove==null) {
						continue;
					}
					Rectangle rectangleAbove=GetBox(fieldAbove);
					bool isComboAbove=fieldAbove is System.Windows.Forms.ComboBox || fieldAbove.GetType().Name.Contains("ComboBox");
					if(Math.Abs(rectangleAbove.Left-rectangleBox.Left)>1
						|| (isComboAbove && Math.Abs(rectangleAbove.Right-rectangleBox.Right)<=16 && Math.Abs(rectangleAbove.Right-rectangleBox.Right)>1))
					{
						listFindings.Add(window+"\tpicker-column\t"+picker.Name+" "+fieldAbove.Name);
					}
				}
			}
		}

		///<summary>Fields named by a label on their row share one left edge with the fields of other rows, group boxes and tab pages whose left edge
		///is within 40 px of theirs (window coordinates): the grid's field columns. A Preferences page has one field column.</summary>
		private static void CheckColumns(GridWindow gridWindow,Control root,List<string> listFindings) {
			List<Tuple<string,int>> listFields=new List<Tuple<string,int>>();
			//Round 3: the tab pages a field stands in (their names from the window down). Fields of two tab pages of one tab control are never on
			//screen together, so they need not share a column: a field is compared with the fields of its own page and of the pages around it.
			Dictionary<string,string> dictionaryPages=new Dictionary<string,string>();
			foreach(Control container in Walk(root)) {
				if(container is FlowLayoutPanel || container is TableLayoutPanel || container is SplitContainer) {
					continue;
				}
				List<Control> listKids=container.Controls.Cast<Control>().Where(x => x.Dock==DockStyle.None && TextFitBoundsRecord.IsDesignedVisible(x)).ToList();
				foreach(Control label in listKids.Where(x => x is Label && !string.IsNullOrWhiteSpace(x.Text))) {
					Control field=listKids.Where(x => IsSingleLineField(x) && Math.Abs(CenterY(x)-CenterY(label))<=4 && GetBox(x).Left>=label.Right-8
						&& GetBox(x).Left-label.Right<=40).OrderBy(x => GetBox(x).Left).FirstOrDefault();
					if(field==null) {
						continue;
					}
					//The x in the window: the control's Left and its containers' (the designer's coordinates, as the planner sums them).
					int x=GetBox(field).Left;
					for(Control controlUp=container;controlUp!=null && controlUp!=root;controlUp=controlUp.Parent) {
						x+=controlUp.Left;
					}
					listFields.Add(Tuple.Create(field.Name,x));
					string pages="";
					for(Control controlUp=container;controlUp!=null && controlUp!=root;controlUp=controlUp.Parent) {
						if(controlUp is System.Windows.Forms.TabPage || TextFitMeasurer.IsOfType(controlUp,"OpenPmsDental.UI.TabPage")) {
							pages="/"+controlUp.Name+pages;
						}
					}
					dictionaryPages[field.Name]=pages;
				}
			}
			bool isPreferencesPage=gridWindow.Window.Substring(gridWindow.Window.LastIndexOf('.')+1).StartsWith("UserControl");
			if(isPreferencesPage && listFields.Count>1) {
				int xPage=listFields.GroupBy(x => x.Item2).OrderByDescending(x => x.Count()).ThenByDescending(x => x.Key).First().Key;
				foreach(Tuple<string,int> tuple in listFields.Where(x => Math.Abs(x.Item2-xPage)>1)) {//1 px: rounding of nested group boxes
					listFindings.Add(gridWindow.Window+"	column	"+tuple.Item1);
				}
				return;
			}
			HashSet<string> hashSetFound=new HashSet<string>();
			foreach(string pagesContext in dictionaryPages.Values.Distinct()) {
				//The fields on screen with this page: its own and those of the pages around it (a prefix of its path).
				List<Tuple<string,int>> listSorted=listFields.Where(x => (pagesContext+"/").StartsWith(dictionaryPages[x.Item1]+"/"))
					.OrderBy(x => x.Item2).ToList();
				int i=0;
				while(i<listSorted.Count) {
					List<Tuple<string,int>> listCluster=listSorted.Skip(i).TakeWhile(x => x.Item2-listSorted[i].Item2<=40).ToList();
					int xMax=listCluster.Max(x => x.Item2);
					foreach(Tuple<string,int> tuple in listCluster.Where(x => xMax-x.Item2>1)) {
						if(hashSetFound.Add(tuple.Item1)) {
							listFindings.Add(gridWindow.Window+"	column	"+tuple.Item1);
						}
					}
					i+=listCluster.Count;
				}
			}
		}

		private static IEnumerable<Control> Walk(Control control) {
			yield return control;
			//The date picker and the clinic picker lay out their own parts.
			if(control is OpenPmsDental.UI.ODDatePicker || control is OpenPmsDental.UI.ComboBoxClinicPicker) {
				yield break;
			}
			foreach(Control child in control.Controls) {
				foreach(Control control2 in Walk(child)) {
					yield return control2;
				}
			}
		}

		///<summary>UI-12 slice 9 fix round: the label-centre rule holds in the windows of slice 9 and later slices.</summary>
		private static bool IsLabelCentreChecked(string slice) {
			return int.TryParse(slice,out int number) && number>=9;
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

		private static bool IsCheck(Control control) {
			return control is RadioButton || control is System.Windows.Forms.CheckBox || TextFitMeasurer.IsOfType(control,TextFitMeasurer.TYPE_UI_CHECKBOX);
		}

		private static bool IsButton(Control control) {
			return (control is System.Windows.Forms.Button || control.GetType().FullName=="OpenPmsDental.UI.Button") && control.Height<=30;
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

		///<summary>The input box a field column aligns: a clinic picker's box after its label area; otherwise the ink.</summary>
		private static Rectangle GetBox(Control control) {
			if(control is OpenPmsDental.UI.ComboBoxClinicPicker comboBoxClinicPicker && comboBoxClinicPicker.ShowLabel) {
				int widthLabel=comboBoxClinicPicker.WidthLabelArea;
				return new Rectangle(control.Left+widthLabel,control.Top,control.Width-widthLabel,control.Height);
			}
			return GetInk(control);
		}

		///<summary>The part of the bounds the control draws in: a label's or check box's text extent on the side it is aligned to.</summary>
		private static Rectangle GetInk(Control control) {
			Rectangle rectangle=control.Bounds;
			//A date picker's box sits inside its bounds (its own layout); a clinic picker draws its own label before its box.
			if(control is OpenPmsDental.UI.ODDatePicker odDatePicker && odDatePicker.Controls.Find("textDate",false).FirstOrDefault() is Control controlDate) {
				return new Rectangle(rectangle.X+controlDate.Left,rectangle.Y,controlDate.Width,rectangle.Height);
			}
			if(control is OpenPmsDental.UI.ComboBoxClinicPicker comboBoxClinicPicker && comboBoxClinicPicker.ShowLabel) {
				//Fix round: its box and its own label, which it draws right-aligned 6 px before the box (another control over the label covers it); slice 1:
				//its label area is sized from the text (WidthLabelArea).
				int widthLabel=comboBoxClinicPicker.WidthLabelArea;
				int widthText;
				using(StringFormat stringFormat=new StringFormat(StringFormat.GenericTypographic)) {
					widthText=TextFitMeasurer.MeasureLineGdiPlus(comboBoxClinicPicker.IsMultiSelect ? "Clinics" : "Clinic",comboBoxClinicPicker.Font,stringFormat).Width;
				}
				int xText=Math.Max(0,widthLabel-(int)Math.Round(UiGrid.LABEL_GAP*comboBoxClinicPicker.DeviceDpi/96f)-widthText);
				return new Rectangle(rectangle.X+xText,rectangle.Y,rectangle.Width-xText,rectangle.Height);
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
