// UI-13 (docs/port-notes/UI-13.md, step 3): the style sheet of gallery G-UI3-ui13: one picture per element type at 100% and 150%, drawn off
// screen at the program's font the way the gallery harness draws windows (each control printed, then its children). "after" attaches the
// shared looks exactly as a window does (UiInputFrame.AttachTree); "before" (OPMS_UI_ELEMENT_STYLES_BEFORE=1) leaves the stock controls as
// Windows draws them, which is what main shows for these types (the time pattern draws its look in both: its before is the owner row's
// window picture). At 150% the controls, their fonts and the looks' scale are 1.5 times. Opt-in: OPMS_UI_ELEMENT_STYLES names the folder.
// Nothing is shown; no database.
// UI-13 fix round: the sheets are hosted in a FormODBase whose LayoutManager is set to the scale the way FormODBase sets it from a monitor's dpi
// (SetScaleMS), so the looks take their scale through the program's own path (UiLooks.GetScale from the control's window), not a test zoom;
// they are drawn with Windows' visual styles (VisualStylesScope: comctl32 version 6, as the program runs), so the native parts the looks leave
// to Windows (tree and list views, the month calendar) show the picture the program shows; the time pattern is FormProcCodeEdit's own,
// laid out at the scale by its LayoutManager (WindowBuilder, as the window gallery draws windows at 150%) and cropped to the bar, its handle,
// its caption and its Minutes box.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;
using UiRefreshTests.Scan;
using UiRefreshTests.TextFit;

namespace UiRefreshTests {
	[TestClass]
	public class ElementStyleGallery {
		public const string VARIABLE_FOLDER="OPMS_UI_ELEMENT_STYLES";
		public const string VARIABLE_BEFORE="OPMS_UI_ELEMENT_STYLES_BEFORE";

		[TestMethod]
		public void Gallery_ElementStyles_At100And150() {
			string folder=Environment.GetEnvironmentVariable(VARIABLE_FOLDER);
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: "+VARIABLE_FOLDER+" names the folder for the UI-13 style sheet pictures.");
			}
			bool isBefore=Environment.GetEnvironmentVariable(VARIABLE_BEFORE)=="1";
			Directory.CreateDirectory(folder);
			List<string> listFailed=new List<string>();
			int countPictures=0;
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				DesignSheetWindows.IsPrintingWithoutChildren=true;
				using VisualStylesScope visualStylesScope=new VisualStylesScope();
				if(!visualStylesScope.IsActive) {
					listFailed.Add("Windows' visual styles could not be activated: the native parts draw their classic picture.");
				}
				//Before: the stock types without UI-13's looks, as main draws them.
				OpenPmsDental.UI.UiLooks.IsOffForGallery=isBefore;
				foreach(float scale in new float[] { 1f,1.5f }) {
					foreach(KeyValuePair<string,Func<float,Control[]>> pair in GetSheets()) {
						try {
							using(HostFormOD form=new HostFormOD(new Size(100,100))) {
								//The window on a monitor at the scale, as FormODBase sets it from the monitor's dpi.
								form.GetLayoutManager().SetScaleMS(scale);
								form.BackColor=SystemColors.Control;
								form.Font=new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY*scale);
								Panel panel=new Panel { BackColor=SystemColors.Control,Location=Point.Empty };
								form.Controls.Add(panel);
								List<Control> listControls=new List<Control>(pair.Value(scale));
								List<Point> listOffsets=new List<Point>();
								foreach(Control control in listControls) {
									listOffsets.Add(control.Location);
									//The font as Windows scales a point size at the monitor's dpi.
									control.Font=new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY*scale);
									//The program's own controls get the window's LayoutManager, as LayoutManagerForms passes it to them.
									FieldInfo fieldInfo=control.GetType().GetField("LayoutManager",BindingFlags.Instance|BindingFlags.Public);
									if(fieldInfo!=null && fieldInfo.FieldType==typeof(LayoutManagerForms)) {
										fieldInfo.SetValue(control,form.GetLayoutManager());
									}
									panel.Controls.Add(control);
								}
								if(!isBefore) {
									UiInputFrame.AttachTree(panel);
								}
								DesignSheetWindows.CreateHandles(form);
								//Laid out after the handles: a month calendar takes its size from its font then.
								int x=S(8,scale);
								int bottom=0;
								for(int i=0;i<listControls.Count;i++) {
									Control control=listControls[i];
									control.Location=new Point(x,S(8,scale)+listOffsets[i].Y);
									x=control.Right+S(12,scale);
									bottom=Math.Max(bottom,control.Bottom);
								}
								panel.Size=new Size(x-S(4,scale),bottom+S(8,scale));
								form.ClientSize=panel.Size;
								foreach(Control control in panel.Controls) {
									Fill(control);
								}
								using(Bitmap bitmap=DesignSheetWindows.Capture(panel,Rectangle.Empty)) {
									bitmap.Save(Path.Combine(folder,pair.Key+"-"+(int)Math.Round(scale*100)+".png"),ImageFormat.Png);
									countPictures++;
								}
							}
						}
						catch(Exception exception) {
							listFailed.Add(pair.Key+" at "+scale+": "+TierOneScan.Describe(exception));
						}
					}
					try {
						using(Bitmap bitmap=DrawTimePattern(scale)) {
							bitmap.Save(Path.Combine(folder,"14-timepattern-"+(int)Math.Round(scale*100)+".png"),ImageFormat.Png);
							countPictures++;
						}
					}
					catch(Exception exception) {
						listFailed.Add("14-timepattern at "+scale+": "+TierOneScan.Describe(exception));
					}
				}
			});
			DesignSheetWindows.IsPrintingWithoutChildren=false;
			OpenPmsDental.UI.UiLooks.IsOffForGallery=false;
			File.WriteAllLines(Path.Combine(folder,"element-styles-failed.txt"),listFailed);
			Assert.IsTrue(countPictures>0,"No picture made: "+string.Join("; ",listFailed));
		}

		private static int S(int value96,float scale) {
			return (int)Math.Round(value96*scale);
		}

		///<summary>The time pattern as FormProcCodeEdit shows it, through the window's own path: the window built and laid out at the scale by its
		///LayoutManager (the bar's cells, column and letters follow it, SetCellSizesForScale), the fonts as Windows scales them at that dpi, a
		///pattern of 12 cells (//XXXX//XX//) filled as the window's FillTime fills it and its handle placed where FillTime places it
		///(GetSliderLocation); cropped to the bar, its handle, its caption and its Minutes box with an 8 px margin.</summary>
		private static Bitmap DrawTimePattern(float scale) {
			ScanType scanType=WindowBuilder.GetScanTypes().FirstOrDefault(x => x.WindowKey=="OpenPmsDental.FormProcCodeEdit");
			if(scanType==null) {
				throw new Exception("FormProcCodeEdit is not a scanned type.");
			}
			FitGallery.SetGalleryFont("program");
			try {
				using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
					if(scale!=1f) {
						builtWindow.LayOutAt(scale);
					}
					FitGallery.SettleAfterHandles(builtWindow);
					Control root=builtWindow.Root;
					if(root is FormODBase formODBase && formODBase.PanelClient!=null) {
						root=formODBase.PanelClient;
					}
					if(scale!=1f) {
						GridStyleGallery.ScaleFontsAsWindowsDoes(root,scale);
					}
					TableTimeBar tableTimeBar=(TableTimeBar)FindByName(root,"tbTime");
					Control controlSlider=FindByName(root,"butSlider");
					string pattern="//XXXX//XX//";
					for(int i=0;i<tableTimeBar.MaxRows;i++) {
						tableTimeBar.Cell[0,i]=i<pattern.Length ? pattern.Substring(i,1) : "";
						tableTimeBar.BackGColor[0,i]=i<pattern.Length ? Color.White : SystemColors.Control;
					}
					tableTimeBar.Refresh();
					builtWindow.LayoutManager.MoveLocation(controlSlider,tableTimeBar.GetSliderLocation(pattern.Length));
					if(FindByName(root,"textTime2") is TextBox textBox) {
						textBox.Text=(pattern.Length*5).ToString();
					}
					Rectangle rectangleCrop=Rectangle.Empty;
					foreach(string name in new string[] { "tbTime","butSlider","label6","label11","textTime2" }) {
						Control control=FindByName(root,name);
						if(control==null) {
							continue;
						}
						Rectangle rectangle=GetBoundsIn(control,root);
						rectangleCrop=rectangleCrop.IsEmpty ? rectangle : Rectangle.Union(rectangleCrop,rectangle);
					}
					rectangleCrop.Inflate(S(8,scale),S(8,scale));
					rectangleCrop.Intersect(new Rectangle(0,0,root.Width,root.Height));
					return DesignSheetWindows.Capture(root,rectangleCrop);
				}
			}
			finally {
				UiTheme.SetFontForTest(null);
				DefaultFontOverride.Set(TextFitFontOption.Today,1f);
			}
		}

		private static Control FindByName(Control root,string name) {
			foreach(Control child in root.Controls) {
				if(child.Name==name) {
					return child;
				}
				Control found=FindByName(child,name);
				if(found!=null) {
					return found;
				}
			}
			return null;
		}

		private static Rectangle GetBoundsIn(Control control,Control ancestor) {
			Rectangle rectangle=control.Bounds;
			for(Control parent=control.Parent;parent!=null && parent!=ancestor;parent=parent.Parent) {
				rectangle.Offset(parent.Left,parent.Top);
			}
			return rectangle;
		}

		///<summary>Content the controls show once they have handles (items Windows needs a handle for, a value that makes a thumb).</summary>
		private static void Fill(Control control) {
			if(control is TextBox textBox && textBox.Multiline) {
				textBox.SelectionStart=0;
			}
		}

		private static string Lines(int count) {
			List<string> listLines=new List<string>();
			for(int i=1;i<=count;i++) {
				listLines.Add("Line "+i+" of the note");
			}
			return string.Join("\r\n",listLines);
		}

		///<summary>The sheets: a name and the controls (their Top is the offset in the sheet, their Left is set by the sheet).</summary>
		private static List<KeyValuePair<string,Func<float,Control[]>>> GetSheets() {
			List<KeyValuePair<string,Func<float,Control[]>>> list=new List<KeyValuePair<string,Func<float,Control[]>>>();
			void add(string name,Func<float,Control[]> func) {
				list.Add(new KeyValuePair<string,Func<float,Control[]>>(name,func));
			}
			add("01-scrollbars",s => new Control[] {
				new VScrollBar { Size=new Size(S(17,s),S(160,s)),Maximum=100,LargeChange=25,Value=30 },
				new VScrollBar { Size=new Size(S(17,s),S(160,s)),Enabled=false },
				new HScrollBar { Size=new Size(S(200,s),S(17,s)),Maximum=100,LargeChange=40,Value=20 },
			});
			add("02-textbox-scroll",s => new Control[] {
				new TextBox { Multiline=true,ScrollBars=ScrollBars.Vertical,BorderStyle=BorderStyle.Fixed3D,Size=new Size(S(200,s),S(100,s)),Text=Lines(20) },
				new RichTextBox { Size=new Size(S(200,s),S(100,s)),Text=Lines(20),ScrollBars=RichTextBoxScrollBars.ForcedVertical },
			});
			add("03-listbox",s => {
				System.Windows.Forms.ListBox listBox=new System.Windows.Forms.ListBox { Size=new Size(S(180,s),S(120,s)),IntegralHeight=false };
				for(int i=1;i<=30;i++) {
					listBox.Items.Add("Stock list item "+i);
				}
				OpenPmsDental.UI.ListBox listBoxOD=new OpenPmsDental.UI.ListBox { Size=new Size(S(180,s),S(120,s)) };
				for(int i=1;i<=30;i++) {
					listBoxOD.Items.Add("Program's list item "+i);
				}
				return new Control[] { listBox,listBoxOD };
			});
			add("04-treeview-listview",s => {
				TreeView treeView=new TreeView { Size=new Size(S(180,s),S(140,s)) };
				for(int i=1;i<=6;i++) {
					TreeNode treeNode=treeView.Nodes.Add("Group "+i);
					for(int j=1;j<=3;j++) {
						treeNode.Nodes.Add("Item "+i+"."+j);
					}
					treeNode.Expand();
				}
				ListView listView=new ListView { Size=new Size(S(220,s),S(140,s)),View=View.Details,FullRowSelect=true };
				listView.Columns.Add("Name",S(110,s));
				listView.Columns.Add("Value",S(80,s));
				for(int i=1;i<=12;i++) {
					listView.Items.Add(new ListViewItem(new[] { "Row "+i,(i*7).ToString() }));
				}
				return new Control[] { treeView,listView };
			});
			add("05-trackbar",s => new Control[] {
				new Label { Text="No Patient",AutoSize=false,Size=new Size(S(70,s),S(27,s)),TextAlign=ContentAlignment.MiddleLeft },
				new TrackBar { AutoSize=false,Size=new Size(S(330,s),S(27,s)),Maximum=10,Value=4,LargeChange=1 },
				new TrackBar { AutoSize=false,Size=new Size(S(200,s),S(27,s)),Maximum=10,Value=7,Enabled=false },
			});
			add("06-buttons",s => new Control[] {
				new System.Windows.Forms.Button { Text="Stock button",Size=new Size(S(100,s),S(26,s)),UseVisualStyleBackColor=true },
				new System.Windows.Forms.Button { Text="Disabled",Size=new Size(S(90,s),S(26,s)),Enabled=false,UseVisualStyleBackColor=true },
				new System.Windows.Forms.Button { Size=new Size(S(26,s),S(26,s)),BackColor=Color.RoyalBlue,UseVisualStyleBackColor=false },
				new System.Windows.Forms.Button { Size=new Size(S(26,s),S(26,s)),BackColor=Color.White,UseVisualStyleBackColor=false },
				new System.Windows.Forms.Button { Size=new Size(S(12,s),S(15,s)),BackColor=SystemColors.ControlDark,UseVisualStyleBackColor=false },
				new OpenPmsDental.UI.Button { Text="Program's button",Size=new Size(S(120,s),S(26,s)) },
			});
			add("07-checkboxes",s => new Control[] {
				new System.Windows.Forms.CheckBox { Text="Stock check box",Checked=true,AutoSize=false,Size=new Size(S(140,s),S(20,s)) },
				new System.Windows.Forms.CheckBox { Text="Unchecked",AutoSize=false,Size=new Size(S(110,s),S(20,s)) },
				new OpenPmsDental.UI.CheckBox { Text="Program's check box",Checked=true,Size=new Size(S(160,s),S(20,s)) },
			});
			add("08-block-label",s => new Control[] {
				new Label { Text="Ins\r\nRem",BackColor=Color.LightGray,TextAlign=ContentAlignment.MiddleCenter,Size=new Size(S(45,s),S(33,s)) },
				new Label { Text="DPlan Rem",BackColor=Color.LightGray,TextAlign=ContentAlignment.MiddleCenter,Size=new Size(S(45,s),S(33,s)) },
			});
			add("09-progressbar",s => new Control[] {
				new ProgressBar { Size=new Size(S(220,s),S(14,s)),Value=60 },
				new ProgressBar { Size=new Size(S(120,s),S(14,s)),Value=25,Enabled=false },
			});
			add("10-borders",s => new Control[] {
				new Panel { BorderStyle=BorderStyle.Fixed3D,BackColor=Color.White,Size=new Size(S(100,s),S(60,s)) },
				new Panel { BorderStyle=BorderStyle.FixedSingle,BackColor=Color.White,Size=new Size(S(100,s),S(60,s)) },
				new PictureBox { BorderStyle=BorderStyle.Fixed3D,BackColor=Color.White,Size=new Size(S(100,s),S(60,s)) },
			});
			add("11-monthcalendar",s => new Control[] { new MonthCalendar { MaxSelectionCount=1 } });
			add("12-datagridview",s => {
				DataGridView dataGridView=new DataGridView { Size=new Size(S(260,s),S(120,s)),AllowUserToAddRows=false,RowHeadersVisible=false,
					ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.AutoSize };
				dataGridView.Columns.Add("colName","Name");
				dataGridView.Columns.Add("colValue","Value");
				for(int i=1;i<=8;i++) {
					dataGridView.Rows.Add("Row "+i,(i*3).ToString());
				}
				return new Control[] { dataGridView };
			});
			add("13-linklabel",s => new Control[] { new LinkLabel { Text="View provider and clinic specific fees",AutoSize=true } });
			return list;
		}
	}
}
