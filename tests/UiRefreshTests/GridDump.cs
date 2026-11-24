// UI-12 (the layout grid, docs/port-notes/UI-12.md section 2): the control tree of each grid window, built off screen at the program's font, with
// what the grid planner (tools/UiFit/grid_tools.py) needs: bounds, client area, dock, anchor, the measured text on one line, a group box's title
// band, a check box's CheckAlign. Opt-in: OPMS_UI_GRID_DUMP names the output file (TSV); OPMS_UI_GRID_WINDOWS the window types (full names,
// comma separated; default: every window of Data\grid-windows.tsv). Nothing is shown; no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using UiRefreshTests.Scan;
using UiRefreshTests.TextFit;

namespace UiRefreshTests {
	[TestClass]
	public class GridDump {
		public const string VARIABLE_FILE="OPMS_UI_GRID_DUMP";
		public const string VARIABLE_WINDOWS="OPMS_UI_GRID_WINDOWS";
		public const string HEADER="window\tpath\tname\ttype\tindex\tx\ty\twidth\theight\tclient_x\tclient_y\tclient_width\tclient_height\tvisible\tdock\tanchor"
			+"\tauto_size\tsizes_itself\tlayout_panel_child\tcheck_align\ttext_align\tmultiline\tfont_height\ttext_width\ttext_height\ttitle_bottom\tpreferred_height\ttext";

		///<summary>The windows of the grid (UI-12): the full type names in the first column of Data\grid-windows.tsv.</summary>
		public static List<string> GetGridWindows() {
			return SliceData.ReadLines(SliceData.Folder("grid-windows"),"window\tslice\tmode\tnote").Select(x => x.ArrayFields[0].Trim()).ToList();
		}

		[TestMethod]
		public void Dump_GridWindows_ControlTree() {
			string file=Environment.GetEnvironmentVariable(VARIABLE_FILE);
			if(string.IsNullOrEmpty(file)) {
				Assert.Inconclusive("Opt-in: "+VARIABLE_FILE+" names the file for the grid planner's control tree.");
			}
			string windows=Environment.GetEnvironmentVariable(VARIABLE_WINDOWS);
			List<string> listWindows=string.IsNullOrWhiteSpace(windows) ? GetGridWindows() : windows.Split(new[] { ',' },StringSplitOptions.RemoveEmptyEntries).ToList();
			List<string> listLines=new List<string> { HEADER };
			List<string> listFailed=new List<string>();
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
				WindowBuilder.IsBuildingByConstructor=true;//UI-12 slice 14: the windows made in their constructor, as GridRulesTests and the gallery build them
				foreach(string window in listWindows) {
					ScanType scanType=listScanTypes.FirstOrDefault(x => x.WindowKey==window);
					if(scanType==null) {
						listFailed.Add(window+": not a scanned type");
						continue;
					}
					FitGallery.SetGalleryFont("program");
					try {
						using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
							Control root=builtWindow.Root;
							if(root is FormODBase formODBase && formODBase.PanelClient!=null) {
								root=formODBase.PanelClient;
							}
							Dictionary<Control,AnchorStyles> dictAnchors=GetStoredAnchors(builtWindow.LayoutManager);
							AddRows(window,".",root,0,listLines,dictAnchors);
						}
					}
					catch(Exception exception) {
						listFailed.Add(window+": "+TierOneScan.Describe(exception));
					}
					finally {
						UiTheme.SetFontForTest(null);
						DefaultFontOverride.Set(TextFitFontOption.Today,1f);
					}
				}
				WindowBuilder.IsBuildingByConstructor=false;
			});
			Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(file)));
			File.WriteAllLines(file,listLines,new UTF8Encoding(false));
			File.WriteAllLines(file+".failed.txt",listFailed);
			Assert.IsTrue(listLines.Count>1,"Nothing dumped: "+string.Join("; ",listFailed));
		}

		///<summary>UI-12 slice 5: the anchor of each control as the window's LayoutManager stores it (Control96Info.Anchor). LayoutManager takes
		///every control's designer anchor over when the window is laid out and leaves the control itself at Top|Left (SetNoAnchorDock), so the
		///control's own Anchor says nothing about how it moves when its container resizes. Empty for a window without a LayoutManager.</summary>
		public static Dictionary<Control,AnchorStyles> GetStoredAnchors(LayoutManagerForms layoutManager) {
			Dictionary<Control,AnchorStyles> dictAnchors=new Dictionary<Control,AnchorStyles>();
			if(layoutManager==null) {
				return dictAnchors;
			}
			FieldInfo fieldInfo=typeof(LayoutManagerForms).GetField("_listControl96Infos",BindingFlags.Instance|BindingFlags.NonPublic);
			if(!(fieldInfo?.GetValue(layoutManager) is List<Control96Info> listControl96Infos)) {
				return dictAnchors;
			}
			for(int i=0;i<listControl96Infos.Count;i++) {
				if(listControl96Infos[i].ControlRef!=null) {
					dictAnchors[listControl96Infos[i].ControlRef]=listControl96Infos[i].Anchor;
				}
			}
			return dictAnchors;
		}

		private static void AddRows(string window,string path,Control control,int index,List<string> listLines,Dictionary<Control,AnchorStyles> dictAnchors) {
			listLines.Add(ToLine(window,path,control,index,dictAnchors));
			for(int i=0;i<control.Controls.Count;i++) {
				AddRows(window,TextFitBoundsRecord.GetChildPath(path,control,i),control.Controls[i],i,listLines,dictAnchors);
			}
		}

		private static string ToLine(string window,string path,Control control,int index,Dictionary<Control,AnchorStyles> dictAnchors) {
			//The anchor LayoutManager stores for the control (its designer anchor); the control's own one where LayoutManager does not track it.
			if(!dictAnchors.TryGetValue(control,out AnchorStyles anchor)) {
				anchor=control.Anchor;
			}
			Rectangle rectangleDisplay=control.DisplayRectangle;
			string checkAlign="";
			string textAlign="";
			PropertyInfo propertyCheckAlign=control.GetType().GetProperty("CheckAlign",typeof(ContentAlignment));
			if(propertyCheckAlign!=null) {
				checkAlign=propertyCheckAlign.GetValue(control).ToString();
			}
			PropertyInfo propertyTextAlign=control.GetType().GetProperty("TextAlign");
			if(propertyTextAlign!=null) {
				try {
					textAlign=propertyTextAlign.GetValue(control)?.ToString()??"";
				}
				catch(Exception) {
					textAlign="";
				}
			}
			bool isMultiline=control is TextBoxBase textBoxBase && textBoxBase.Multiline;
			Size sizeText=MeasureText(control);
			int titleBottom=0;
			if(control is System.Windows.Forms.GroupBox || TextFitMeasurer.IsOfType(control,"OpenPmsDental.UI.GroupBox")) {
				//UI.GroupBox draws its title with DrawString at y 1; the stock group box draws it at the top.
				titleBottom=string.IsNullOrEmpty(control.Text) ? 0 : 1+TextFitMeasurer.MeasureLineGdiPlus(control.Text,control.Font,StringFormat.GenericDefault).Height;
			}
			int preferredHeight=0;
			if(control is TextBoxBase || control is System.Windows.Forms.ComboBox || control is DateTimePicker || control is UpDownBase) {
				preferredHeight=control.GetPreferredSize(Size.Empty).Height;
				if(control is System.Windows.Forms.TextBox textBox && !textBox.Multiline) {
					preferredHeight=textBox.PreferredHeight;
				}
			}
			string text=control.Text??"";
			if(control is TextBoxBase || control is System.Windows.Forms.ComboBox || control is ListControl) {
				text="";//content, not a caption
			}
			return string.Join("\t",new string[] {
				Clean(window),Clean(path),Clean(control.Name),Clean(control.GetType().FullName),Int(index),
				Int(control.Left),Int(control.Top),Int(control.Width),Int(control.Height),
				Int(rectangleDisplay.X),Int(rectangleDisplay.Y),Int(control.ClientSize.Width),Int(control.ClientSize.Height),
				TextFitBoundsRecord.IsDesignedVisible(control) ? "1" : "0",control.Dock.ToString(),((int)anchor).ToString(CultureInfo.InvariantCulture),
				control.AutoSize ? "1" : "0",TextFitBoundsRecord.IsSizingItself(control) ? "1" : "0",
				(control.Parent is FlowLayoutPanel || control.Parent is TableLayoutPanel) ? "1" : "0",
				checkAlign,textAlign,isMultiline ? "1" : "0",Int(control.Font.Height),Int(sizeText.Width),Int(sizeText.Height),Int(titleBottom),Int(preferredHeight),
				Clean(text) });
		}

		///<summary>The control's caption on one line, measured the way the control draws it (TextFitMeasurer's method by type); empty for a control
		///without a measured caption.</summary>
		private static Size MeasureText(Control control) {
			//UI-12 slice 1: a clinic picker's label area is sized from its label's text; the planner reads the text's width here (its box starts
			//6 px after it). A tree without it is of a build before slice 1 (main's fixed 37 px label area).
			if(control is OpenPmsDental.UI.ComboBoxClinicPicker comboBoxClinicPicker) {
				if(!comboBoxClinicPicker.ShowLabel) {
					return Size.Empty;
				}
				return new Size(comboBoxClinicPicker.WidthLabelArea-comboBoxClinicPicker.LayoutManager.Scale(6),0);
			}
			if(string.IsNullOrWhiteSpace(control.Text)) {
				return Size.Empty;
			}
			TextFitMethod method=TextFitMeasurer.GetMethod(control);
			if(method==TextFitMethod.MeasureString) {
				using(StringFormat stringFormat=new StringFormat(StringFormat.GenericDefault)) {
					return TextFitMeasurer.MeasureLineGdiPlus(control.Text,control.Font,stringFormat);
				}
			}
			if(method==TextFitMethod.TextRenderer) {
				return TextFitMeasurer.MeasureLineGdi(control.Text,control.Font,TextFitMeasurer.GetStockFlags(control));
			}
			return Size.Empty;
		}

		private static string Int(int value) {
			return value.ToString(CultureInfo.InvariantCulture);
		}

		private static string Clean(string value) {
			return (value??"").Replace("\\","\\\\").Replace("\t","\\t").Replace("\r","").Replace("\n","\\n");
		}
	}
}
