// UI-12 pilot (docs/port-notes/UI-12.md): the shared control styles the owner flagged in the G-UI3 review, drawn off screen at the program's font
// at 100% and 150% for gallery G-UI3-ui12p: FormPayment's single-line inputs (text boxes, combo boxes, the number fields and the date pickers),
// and ControlChart's procedure buttons (B/F, V, M, O/I, D, L and the box above them) and the Progress Notes page bar. Opt-in:
// OPMS_UI_GRID_STYLES names the folder. Nothing is shown; no database.
// UI-12 fix round: FormPayment with its splits hidden (FormPayment-hidesplits, the window ToggleShowHideSplits makes), and at 150% the date
// pickers' and up-down boxes' fonts scaled as Windows scales them at 144 dpi (LayoutManager gives them the zoom only and leaves the dpi to
// Windows; this harness's windows are at 96 dpi, so without it they kept their 100% height in the 150% picture).
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
			new string[] { "ControlChart-procbuttons","OpenPmsDental.ControlChart","textSurf","butBF","butV","butM","butOI","butD","butL" },
			new string[] { "ControlChart-pagebar","OpenPmsDental.ControlChart","gridProgPageNav" },
			new string[] { "FormPayment-hidesplits","OpenPmsDental.FormPayment" },
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
				DesignSheetWindows.IsPrintingWithoutChildren=true;
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
								FitGallery.SettleAfterHandles(builtWindow);
								Control root=builtWindow.Root;
								if(root is FormODBase formODBase && formODBase.PanelClient!=null) {
									root=formODBase.PanelClient;
								}
								if(picture[0]=="FormPayment-hidesplits") {
									HideSplits((FormODBase)builtWindow.Form,builtWindow);
								}
								if(scale!=1f) {
									ScaleFontsAsWindowsDoes(root,scale);
								}
								FitGallery.HideDatePickerCalendars(root);
								//The page bar with four pages, the second current, as the Progress Notes grid shows it with paging on.
								if(FindByName(root,"gridProgPageNav") is OpenPmsDental.UI.ODGridPageNav odGridPageNav) {
									odGridPageNav.PagingChangeEventHandler(null,new OpenPmsDental.UI.ODGridPageEventArgs(2,new List<int> { 1,2,3,4 }));
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
			DesignSheetWindows.IsPrintingWithoutChildren=false;
			File.WriteAllLines(Path.Combine(folder,"styles-failed.txt"),listFailed);
			Assert.IsTrue(countPictures>0,"No picture made: "+string.Join("; ",listFailed));
		}

		///<summary>The date pickers and up-down boxes get the zoom only from LayoutManager and their dpi scaling from Windows; at 96 dpi this harness
		///scales their fonts by the scale, so they take the height Windows gives them at that dpi. UI-12 slice 1: the clinic picker too, whose
		///label area is sized from its label's text at its font (ComboBoxClinicPicker.WidthLabelArea): with the font of the scale its box stands
		///where the program draws it, on its column. UI-12 slice 2: the plain labels too (System.Windows.Forms.Label and its subclasses), and
		///the other controls that draw their own text with the font LayoutManager gives them (the zoom only, the dpi part left to Windows): check
		///boxes, radio buttons, group boxes' titles and buttons, stock and the program's. At 96 dpi the harness drew them at the 100% size beside
		///inputs at the 150% size; with the font of the scale the labels, the check boxes and the inputs beside them match, as on a 150% screen.
		///Two passes: every size is read before any is set, because a control whose font is ambient (LayoutManager sets a font only where it
		///differs from the parent's) would otherwise take its scaled group box's font and be scaled twice.</summary>
		internal static void ScaleFontsAsWindowsDoes(Control control,float scale) {
			List<Tuple<Control,Font>> listScaled=new List<Tuple<Control,Font>>();
			CollectFontsAsWindowsScales(control,listScaled);
			foreach(Tuple<Control,Font> tuple in listScaled) {
				tuple.Item1.Font=new Font(tuple.Item2.FontFamily,tuple.Item2.Size*scale,tuple.Item2.Style);
			}
		}

		///<summary>The controls under the control whose font Windows scales (see ScaleFontsAsWindowsDoes), with their fonts now.</summary>
		private static void CollectFontsAsWindowsScales(Control control,List<Tuple<Control,Font>> listScaled) {
			foreach(Control child in control.Controls) {
				//UI-CAL-2: MonthCalendarOD too: before UI-CAL-2 it drew its text in points, which Windows draws at its dpi (the harness drew 100%
				//text in the 150% calendar); since UI-CAL-2 it draws in pixels at its own scale from its 96 dpi size, whatever its font now.
				if(child is DateTimePicker || child is UpDownBase || child is OpenPmsDental.UI.ComboBoxClinicPicker || child is Label
					|| child is System.Windows.Forms.ButtonBase || child is OpenPmsDental.UI.CheckBox || child is OpenPmsDental.UI.MonthCalendarOD)
				{
					listScaled.Add(Tuple.Create(child,child.Font));
					continue;
				}
				if(child is System.Windows.Forms.GroupBox || child is OpenPmsDental.UI.GroupBox) {
					listScaled.Add(Tuple.Create(child,child.Font));
				}
				CollectFontsAsWindowsScales(child,listScaled);
			}
		}

		///<summary>FormPayment as Hide Splits leaves it (the harness builds windows without Load): the fields Load sets for the toggle, then the
		///hidden state the toggle sets (SetSplitsShown(false); the toggle itself reads Visible, false in a window never shown), then the layout of
		///the new height.</summary>
		private static void HideSplits(FormODBase formODBase,BuiltWindow builtWindow) {
			System.Reflection.BindingFlags bindingFlags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
			Control panelSplits=FindByName(formODBase,"panelSplits");
			formODBase.GetType().GetField("_originalHeight",bindingFlags).SetValue(formODBase,formODBase.Height);
			formODBase.GetType().GetField("_heightSplitsShown",bindingFlags).SetValue(formODBase,panelSplits.Height);
			formODBase.GetType().GetMethod("SetSplitsShown",bindingFlags).Invoke(formODBase,new object[] { false });
			builtWindow.LayoutManager.LayoutFormBoundsAndFonts(formODBase);
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

		///<summary>The control's bounds in the ancestor's picture: mapped through the windows (hidden windows map too), so a tab control's or
		///group box's client origin is counted.</summary>
		private static Rectangle GetBoundsIn(Control control,Control ancestor) {
			Rectangle rectangleScreen=control.Parent.RectangleToScreen(control.Bounds);
			Point pointAncestor=ancestor.PointToScreen(Point.Empty);
			rectangleScreen.Offset(-pointAncestor.X,-pointAncestor.Y);
			return rectangleScreen;
		}
	}
}
