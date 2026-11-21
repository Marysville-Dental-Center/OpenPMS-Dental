// UI-12 pilot (docs/port-notes/UI-12.md): the shared control styles the owner flagged in the G-UI3 review, drawn off screen at the program's font
// at 100% and 150% for gallery G-UI3-ui12p: FormPayment's single-line inputs (text boxes, combo boxes, the number fields and the date pickers),
// and ControlChart's procedure buttons (B/F, V, M, O/I, D, L and the box above them) and the Progress Notes page bar. Opt-in:
// OPMS_UI_GRID_STYLES names the folder. Nothing is shown; no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
	public class GridStyleGallery {
		public const string VARIABLE_FOLDER="OPMS_UI_GRID_STYLES";

		///<summary>Picture name, window type, and the controls whose union (with a margin) is cropped; no controls: the whole window.</summary>
		private static readonly string[][] _arrayPictures=new string[][] {
			new string[] { "FormPayment-inputs","OpenPmsDental.FormPayment" },
			new string[] { "ControlChart-procbuttons","OpenPmsDental.ControlChart","butBF","butV","butM","butOI","butD","butL","panelQuickButtons","groupBox2" },
			new string[] { "ControlChart-pagebar","OpenPmsDental.ControlChart","gridProgPageNav" },
		};

		[TestMethod]
		public void Gallery_SharedControlStyles_At100And150() {
			string folder=Environment.GetEnvironmentVariable(VARIABLE_FOLDER);
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: "+VARIABLE_FOLDER+" names the folder for the UI-12 style pictures.");
			}
			Directory.CreateDirectory(folder);
			int countPictures=0;
			List<string> listFailed=new List<string>();
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
				foreach(string[] picture in _arrayPictures) {
					ScanType scanType=listScanTypes.FirstOrDefault(x => x.WindowKey==picture[1]);
					if(scanType==null) {
						listFailed.Add(picture[1]+": not a scanned type");
						continue;
					}
					foreach(float scale in new float[] { 1f,1.5f }) {
						FitGallery.SetGalleryFont("program");
						try {
							using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
								if(scale!=1f) {
									builtWindow.LayOutAt(scale);
								}
								Control root=builtWindow.Root;
								if(root is FormODBase formODBase && formODBase.PanelClient!=null) {
									root=formODBase.PanelClient;
								}
								Rectangle rectangleCrop=Rectangle.Empty;
								for(int i=2;i<picture.Length;i++) {
									Control control=FindByName(root,picture[i]);
									if(control==null || !TextFitBoundsRecord.IsDesignedVisible(control)) {
										continue;
									}
									Rectangle rectangle=GetBoundsIn(control,root);
									rectangleCrop=rectangleCrop.IsEmpty ? rectangle : Rectangle.Union(rectangleCrop,rectangle);
								}
								if(!rectangleCrop.IsEmpty) {
									rectangleCrop.Inflate((int)(24*scale),(int)(16*scale));
								}
								using(Bitmap bitmap=DesignSheetWindows.Capture(root,rectangleCrop)) {
									bitmap.Save(Path.Combine(folder,picture[0]+"-"+(int)Math.Round(scale*100)+".png"),ImageFormat.Png);
									countPictures++;
								}
							}
						}
						catch(Exception exception) {
							listFailed.Add(picture[0]+" at "+scale+": "+TierOneScan.Describe(exception));
						}
						finally {
							UiTheme.SetFontForTest(null);
							DefaultFontOverride.Set(TextFitFontOption.Today,1f);
						}
					}
				}
			});
			File.WriteAllLines(Path.Combine(folder,"styles-failed.txt"),listFailed);
			Assert.IsTrue(countPictures>0,"No picture made: "+string.Join("; ",listFailed));
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
	}
}
