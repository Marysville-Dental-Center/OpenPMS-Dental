// UI-COLS gallery G-UI3-uicols (docs/port-notes/UI-COLS.md): the windows whose grid columns changed, before (main) and after, at 100% and
// 150%, with fictional values in the grids. The Account module and Edit Appointment are drawn whole as the harness builds them
// (WindowBuilder), their grid filled from DisplayFields' defaults the way ControlAccount.FillMain and FormApptEdit.FillProcedures add the
// columns, with made-up rows. Every other changed grid (Data\grid-col-gallery.tsv) is drawn as a strip: a GridOD with the columns of its
// fill code (read from the source, GridColumnFit.ScanSource) as wide as their widths, three rows of made-up values, the first with each
// column's widest normal value. At 150% the grids paint at 144 dpi (DesignSheetWindows.IsGridPaintedAtDpi), as on a 150% screen.
// Opt-in: OPMS_UI_COLS_GALLERY names the folder; OPMS_UI_COLS_GALLERY_BEFORE names a folder of before pictures of the same names, and each
// pair is also saved as one picture (before above after) in the "pairs" folder beside. Nothing is shown; no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class GridColumnGallery {
		public const string VARIABLE_FOLDER="OPMS_UI_COLS_GALLERY";
		public const string VARIABLE_BEFORE="OPMS_UI_COLS_GALLERY_BEFORE";

		[TestMethod]
		public void Gallery_UiCols_BeforeAndAfter() {
			string folder=Environment.GetEnvironmentVariable(VARIABLE_FOLDER);
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: "+VARIABLE_FOLDER+" names the folder for the UI-COLS gallery pictures.");
			}
			Directory.CreateDirectory(folder);
			List<string> listSaved=new List<string>();
			StaRunner.Run(() => {
				try {
					DesignSheetWindows.IsGridPaintedAtDpi=true;
					foreach(float scale in GridColumnFit.ArrayScales) {
						DesignSheetWindows.PaintDpiScale=scale;
						string suffix=scale==1f ? "" : "-"+(int)Math.Round(scale*100);
						Save(folder,"ControlAccount"+suffix,DrawWindow(typeof(ControlAccount),"gridAccount",DisplayFieldCategory.AccountModule,scale),listSaved);
						Save(folder,"FormApptEdit"+suffix,DrawWindow(typeof(FormApptEdit),"gridProc",DisplayFieldCategory.AppointmentEdit,scale),listSaved);
						foreach(KeyValuePair<string,List<ColumnSite>> pair in GetStrips()) {
							Save(folder,pair.Key+suffix,DrawStrip(pair.Value,scale),listSaved);
						}
					}
				}
				finally {
					DesignSheetWindows.IsGridPaintedAtDpi=false;
					DesignSheetWindows.PaintDpiScale=1f;
				}
			});
			string folderBefore=Environment.GetEnvironmentVariable(VARIABLE_BEFORE);
			if(!string.IsNullOrEmpty(folderBefore)) {
				string folderPairs=Path.Combine(Path.GetDirectoryName(folder.TrimEnd('\\','/')),"pairs");
				Directory.CreateDirectory(folderPairs);
				foreach(string name in listSaved) {
					string fileBefore=Path.Combine(folderBefore,name+".png");
					if(!File.Exists(fileBefore)) {
						continue;
					}
					using(Bitmap bitmapBefore=new Bitmap(fileBefore))
					using(Bitmap bitmapAfter=new Bitmap(Path.Combine(folder,name+".png")))
					using(Bitmap bitmapPair=new Bitmap(Math.Max(bitmapBefore.Width,bitmapAfter.Width),bitmapBefore.Height+bitmapAfter.Height+12,PixelFormat.Format24bppRgb)) {
						using(Graphics g=Graphics.FromImage(bitmapPair)) {
							g.Clear(Color.FromArgb(255,0,255));//the gap between before (top) and after (bottom)
							g.DrawImage(bitmapBefore,0,0,bitmapBefore.Width,bitmapBefore.Height);
							g.DrawImage(bitmapAfter,0,bitmapBefore.Height+12,bitmapAfter.Width,bitmapAfter.Height);
						}
						bitmapPair.Save(Path.Combine(folderPairs,name+".png"),ImageFormat.Png);
					}
				}
			}
			File.WriteAllLines(Path.Combine(folder,"pictures.txt"),listSaved);
		}

		private static void Save(string folder,string name,Bitmap bitmap,List<string> listSaved) {
			using(bitmap) {
				bitmap.Save(Path.Combine(folder,name+".png"),ImageFormat.Png);
			}
			listSaved.Add(name);
		}

		///<summary>The window as the harness builds it at the scale, its grid filled from the category's default display fields with made-up
		///rows, drawn whole (a FormODBase window: its client panel).</summary>
		private static Bitmap DrawWindow(Type type,string nameGrid,DisplayFieldCategory displayFieldCategory,float scale) {
			ScanType scanType=WindowBuilder.GetScanTypes().Single(x => x.Type==type);
			List<DisplayField> listDisplayFields=GridColumnFitTests.GetDefaultDisplayFields(displayFieldCategory);
			using(new GridColumnFitTests.PrefCacheEmptyScope())
			using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
				Control root=builtWindow.Root;
				if(root is FormODBase formODBase && formODBase.PanelClient!=null) {
					root=formODBase.PanelClient;
				}
				if(scale!=1f) {
					builtWindow.LayOutAt(scale);
					GridStyleGallery.ScaleFontsAsWindowsDoes(root,scale);
				}
				DesignSheetWindows.CreateHandles(builtWindow.Form);
				GridOD gridOD=(GridOD)GridColumnFitTests.GetField(builtWindow.Root,nameGrid);
				gridOD.BeginUpdate();
				gridOD.Columns.Clear();
				foreach(DisplayField displayField in listDisplayFields) {
					string title=displayField.Description=="" ? displayField.InternalName : displayField.Description;
					HorizontalAlignment horizontalAlignment=HorizontalAlignment.Left;
					if(type==typeof(ControlAccount) && (title=="Charges" || title=="Credits" || title=="Balance")) {
						horizontalAlignment=HorizontalAlignment.Right;
					}
					gridOD.Columns.Add(new GridColumn(title,displayField.ColumnWidth,horizontalAlignment));
				}
				//As both windows' fill code does: a horizontal scroll bar when the widths add up to more than the grid is wide.
				gridOD.HScrollVisible=listDisplayFields.Sum(x => x.ColumnWidth)>gridOD.Width;
				gridOD.ListGridRows.Clear();
				string[][] arrayRows=type==typeof(ControlAccount) ? GetAccountRows() : GetApptRows();
				foreach(string[] arrayValues in arrayRows) {
					GridRow gridRow=new GridRow();
					foreach(GridColumn gridColumn in gridOD.Columns) {
						gridRow.Cells.Add(GetRowValue(arrayValues,gridColumn.Heading));
					}
					gridOD.ListGridRows.Add(gridRow);
				}
				gridOD.EndUpdate();
				return DesignSheetWindows.Capture(root,Rectangle.Empty);
			}
		}

		///<summary>The value of the heading in a made-up row of "heading=value" pairs; "" when the row has none.</summary>
		private static string GetRowValue(string[] arrayValues,string heading) {
			string pair=arrayValues.FirstOrDefault(x => x.StartsWith(heading+"="));
			return pair==null ? "" : pair.Substring(heading.Length+1);
		}

		///<summary>Account rows of a fictional family (the names of DocSampleData), the amounts as AccountModules formats them ("n").</summary>
		private static string[][] GetAccountRows() {
			return new string[][] {
				new[] { "Date=06/10/2025","Patient=Alex Sample","Prov=DOC1","Code=D0150","Description=Comprehensive oral evaluation","Charges=95.00","Balance=95.00" },
				new[] { "Date=06/10/2025","Patient=Alex Sample","Prov=DOC1","Code=D0210","Description=Intraoral, complete series","Charges=150.00","Balance=245.00" },
				new[] { "Date=06/24/2025","Patient=Alex Sample","Description=Insurance payment","Credits=180.00","Balance=65.00" },
				new[] { "Date=09/22/2026","Patient=Jordan Sample","Prov=DOC1","Code=D2392","Tth=14","Description=Resin composite, two surfaces, posterior","Charges=185.00","Balance=250.00" },
				new[] { "Date=11/09/2026","Patient=Alex Sample","Prov=DOC1","Code=D3330","Tth=3","Description=Endodontic therapy, molar","Charges=1,260.00","Balance=1,510.00" },
				new[] { "Date=12/31/2026","Patient=Sam Sample","Prov=HYG1","Code=D1120","Description=Prophylaxis, child","Charges=72.00","Balance=1,582.00" },
				new[] { "Date=12/31/2026","Patient=Jordan Sample","Description=Patient payment, card","Credits=1,000.00","Balance=582.00" },
			};
		}

		///<summary>Procedures of a fictional appointment, the fees as FormApptEdit formats them ("F").</summary>
		private static string[][] GetApptRows() {
			return new string[][] {
				new[] { "Stat=TP","Tth=3","Code=D3330","Description=Endodontic therapy, molar","Fee=1260.00" },
				new[] { "Stat=TP","Tth=3","Code=D2950","Description=Core buildup, including any pins","Fee=310.00" },
				new[] { "Stat=TP","Tth=30","Surf=MODBL","Code=D2394","Description=Resin composite, four or more surfaces, posterior","Fee=295.00" },
				new[] { "Stat=TP","Code=D0220","Description=Intraoral, periapical, first film","Fee=31.00" },
			};
		}

		///<summary>The strips: the blocks of the gallery list, named file-n (n counts the file's blocks in the list, from 1).</summary>
		private static List<KeyValuePair<string,List<ColumnSite>>> GetStrips() {
			List<ColumnSite> listSites=GridColumnFit.ScanSource(RepoPaths.Root(),GridColumnFit.ReadKnown());
			List<string[]> listGallery=File.ReadAllLines(RepoPaths.DataFile("grid-col-gallery.tsv"))
				.Where(x => x.Trim()!="" && !x.StartsWith("#") && x!="file\ttitle").Select(x => x.Split('\t')).ToList();
			List<KeyValuePair<string,List<ColumnSite>>> listStrips=new List<KeyValuePair<string,List<ColumnSite>>>();
			HashSet<int> setBlocks=new HashSet<int>();
			Dictionary<string,int> dictCount=new Dictionary<string,int>();
			foreach(string[] arrayFields in listGallery) {
				List<int> listBlocks=listSites.Where(x => x.File==arrayFields[0] && (x.Title==arrayFields[1] || x.Case==arrayFields[1]))
					.Select(x => x.Block).Distinct().OrderBy(x => x).ToList();
				if(listBlocks.Count==0) {
					throw new Exception("grid-col-gallery.tsv: no column \""+arrayFields[1]+"\" in "+arrayFields[0]);
				}
				foreach(int block in listBlocks) {
					if(!setBlocks.Add(block)) {
						continue;
					}
					string nameFile=Path.GetFileNameWithoutExtension(arrayFields[0]);
					dictCount[nameFile]=(dictCount.ContainsKey(nameFile) ? dictCount[nameFile] : 0)+1;
					listStrips.Add(new KeyValuePair<string,List<ColumnSite>>("strip-"+nameFile+"-"+dictCount[nameFile],
						listSites.Where(x => x.Block==block).OrderBy(x => x.Line).ToList()));
				}
			}
			return listStrips;
		}

		///<summary>A GridOD with the block's columns at the scale, as wide as their widths, titled with the file and line, and three rows: the
		///widest normal values, then two ordinary ones.</summary>
		private static Bitmap DrawStrip(List<ColumnSite> listSites,float scale) {
			using(Form form=new Form()) {
				GridOD gridOD=new GridOD();
				gridOD.TranslationName="";
				gridOD.Title=listSites[0].File.Substring(listSites[0].File.LastIndexOf('/')+1)+" "+(listSites[0].Case??("line "+listSites[0].Line));
				form.Controls.Add(gridOD);
				DesignSheetWindows.CreateHandles(form);
				gridOD.SetScaleAndZoom(scale,1f);
				int width96=listSites.Where(x => x.Width>0).Sum(x => x.Width);
				gridOD.Bounds=new Rectangle(0,0,GridColumnFit.ScaleI(width96,scale)+2,GridColumnFit.ScaleI(170,scale));
				gridOD.BeginUpdate();
				gridOD.Columns.Clear();
				foreach(ColumnSite columnSite in listSites.Where(x => x.Width>0)) {
					HorizontalAlignment horizontalAlignment=columnSite.Alignment=="Right" ? HorizontalAlignment.Right
						: (columnSite.Alignment=="Center" ? HorizontalAlignment.Center : HorizontalAlignment.Left);
					//A heading of two lines is "Date\r\nShowing" in the source, as GridOD gets it from the compiled string.
					string heading=columnSite.Title.Replace("\\r\\n","\r\n").Replace("\\n","\n");
					gridOD.Columns.Add(new GridColumn(heading,columnSite.Width,horizontalAlignment) { IsWidthDynamic=columnSite.IsDynamic });
				}
				gridOD.ListGridRows.Clear();
				for(int row=0;row<3;row++) {
					GridRow gridRow=new GridRow();
					foreach(ColumnSite columnSite in listSites.Where(x => x.Width>0)) {
						gridRow.Cells.Add(GetStripValue(columnSite,row));
					}
					gridOD.ListGridRows.Add(gridRow);
				}
				gridOD.EndUpdate();
				return DesignSheetWindows.Capture(gridOD,Rectangle.Empty);
			}
		}

		///<summary>A made-up value: the column's widest value in row 0, ordinary values of its type after; text by the heading otherwise.</summary>
		private static string GetStripValue(ColumnSite columnSite,int row) {
			if(columnSite.Value!="") {
				if(row==0) {
					return columnSite.Value;
				}
				switch(columnSite.Kind) {
					case "date": return row==1 ? "06/10/2025" : "01/02/2026";
					case "datetime": return row==1 ? "06/10/2025 9:30 AM" : "01/02/2026 11:15 AM";
					case "time": return row==1 ? "9:30 AM" : "11:15 AM";
					case "money": return row==1 ? "1,260.00" : "95.00";
					case "percent": return row==1 ? "80%" : "50%";
					case "phone": return row==1 ? "(555)555-0123" : "(555)555-0188";
					case "tooth": return row==1 ? "3" : "14";
					case "surface": return row==1 ? "MOD" : "O";
					case "code": return row==1 ? "D2392" : "D1110";
					default: return columnSite.Value;
				}
			}
			string title=columnSite.Title.ToLowerInvariant();
			if(title.Contains("desc") || title.Contains("note") || title.Contains("text") || title.Contains("procedures")) {
				return row==0 ? "Resin composite, two surfaces, posterior" : (row==1 ? "Prophylaxis, adult" : "Periodic oral evaluation");
			}
			if(title.Contains("patient") || title.Contains("name") || title.Contains("guarantor") || title.Contains("employee")) {
				return row==1 ? "Jordan Sample" : "Alex Sample";
			}
			if(title.Contains("prov")) {
				return "DOC1";
			}
			return row==0 ? "Sample" : "";
		}
	}
}
