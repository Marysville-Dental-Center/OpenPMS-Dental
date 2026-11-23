// Gallery G-UI3 for the UI-10 slices (docs/specs/ui-refresh-plan.md sections 5.4 and 8; docs/port-notes/UI-10.md): the windows of a slice drawn off
// screen in the new font (Segoe UI 9 pt) through the token override UiTheme.SetFontForTest, the product's own font path, with the designer files
// as they are now. Run it before the slice's fixes (the before folder) and after (the after folder); tools/New-UiGallery.ps1 makes the page.
// Opt-in: OPMS_UI_FIT_GALLERY names the folder, OPMS_UI_FIT_GALLERY_WINDOWS the window types (full names, comma separated; default: the slice 1
// list below), OPMS_UI_FIT_GALLERY_OWNER a TSV (designer_file, window, control, reason) of the "needs the owner" controls: each is drawn outlined
// in red on the window's picture and cropped into owner\ . Nothing is shown; no database.
// OPMS_UI_FIT_GALLERY_FONT (UI-2b, gallery G-UI2): "program" draws in the font the build holds, with no token override (the tokens' own values
// and the default font set to the body token, as each exe's Main sets it), so a build of main draws today's font and a build after the flip
// draws Segoe UI 9; "today" puts today's font through the override. Unset: Segoe UI 9 through the override, as the UI-10 slices drew.
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
	public class FitGallery {
		public const string VARIABLE_FOLDER="OPMS_UI_FIT_GALLERY";
		public const string VARIABLE_WINDOWS="OPMS_UI_FIT_GALLERY_WINDOWS";
		public const string VARIABLE_OWNER="OPMS_UI_FIT_GALLERY_OWNER";
		public const string VARIABLE_FONT="OPMS_UI_FIT_GALLERY_FONT";
		public const string VARIABLE_SCALE="OPMS_UI_FIT_GALLERY_SCALE";

		///<summary>Slice 1 (UI-10-1): FormPatientEdit, FormProvEdit, FormClaimEdit, FormProcEdit, ControlAccount and the preference pages.</summary>
		public static readonly string[] ArraySlice1Windows=new string[] {
			"OpenPmsDental.FormPatientEdit","OpenPmsDental.FormProvEdit","OpenPmsDental.FormClaimEdit","OpenPmsDental.FormProcEdit","OpenPmsDental.ControlAccount",
			"OpenPmsDental.UserControlMainWindow","OpenPmsDental.UserControlMainWindowMisc","OpenPmsDental.UserControlApptGeneral",
			"OpenPmsDental.UserControlApptAppearance","OpenPmsDental.UserControlFamilyGeneral","OpenPmsDental.UserControlFamilyInsurance",
			"OpenPmsDental.UserControlAccountGeneral","OpenPmsDental.UserControlAccountAdjustments","OpenPmsDental.UserControlAccountClaimSend",
			"OpenPmsDental.UserControlAccountClaimReceive","OpenPmsDental.UserControlAccountPayments","OpenPmsDental.UserControlAccountRecAndRepCharges",
			"OpenPmsDental.UserControlTreatPlanGeneral","OpenPmsDental.UserControlTreatPlanFreqLimit","OpenPmsDental.UserControlChartGeneral",
			"OpenPmsDental.UserControlChartProcedures","OpenPmsDental.UserControlImagingGeneral","OpenPmsDental.UserControlManageGeneral",
			"OpenPmsDental.UserControlManageBillingStatements","OpenPmsDental.UserControlOrtho","OpenPmsDental.UserControlEnterpriseGeneral",
			"OpenPmsDental.UserControlEnterpriseAccount","OpenPmsDental.UserControlEnterpriseAppts","OpenPmsDental.UserControlEnterpriseFamily",
			"OpenPmsDental.UserControlEnterpriseManage","OpenPmsDental.UserControlEnterpriseReports","OpenPmsDental.UserControlServerConnections",
			"OpenPmsDental.UserControlExperimentalPrefs",
		};

		[TestMethod]
		public void Gallery_SliceWindows_InTheNewFont() {
			string folder=Environment.GetEnvironmentVariable(VARIABLE_FOLDER);
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: "+VARIABLE_FOLDER+" names the folder for the UI-10 gallery pictures.");
			}
			string windows=Environment.GetEnvironmentVariable(VARIABLE_WINDOWS);
			string[] arrayWindows=string.IsNullOrWhiteSpace(windows) ? ArraySlice1Windows : windows.Split(new[] { ',' },StringSplitOptions.RemoveEmptyEntries);
			List<string[]> listOwner=new List<string[]>();
			string fileOwner=Environment.GetEnvironmentVariable(VARIABLE_OWNER);
			if(!string.IsNullOrEmpty(fileOwner) && File.Exists(fileOwner)) {
				listOwner=File.ReadAllLines(fileOwner).Skip(1).Select(x => x.Split('\t')).Where(x => x.Length>=4).ToList();
			}
			Directory.CreateDirectory(folder);
			string font=(Environment.GetEnvironmentVariable(VARIABLE_FONT)??"").Trim().ToLowerInvariant();
			int countPictures=0;
			List<string> listFailed=new List<string>();
			//UI-13: OPMS_UI_FIT_GALLERY_LOOKS_OFF=1 draws the windows without the UI-13 looks (the before pictures of windows outside the
			//UI-12 pilot, whose designer files UI-13 does not change).
			bool isLooksOff=Environment.GetEnvironmentVariable("OPMS_UI_FIT_GALLERY_LOOKS_OFF")=="1";
			//UI-13 fix round: OPMS_UI_FIT_GALLERY_VISUAL_STYLES=1 draws the windows with Windows' visual styles (comctl32 version 6, as the program
			//runs; VisualStylesScope), for the windows that show a native type the looks leave to Windows (tree and list views, month calendars).
			bool isVisualStyles=Environment.GetEnvironmentVariable("OPMS_UI_FIT_GALLERY_VISUAL_STYLES")=="1";
			//UI-12 slice 1: OPMS_UI_FIT_GALLERY_SCALE (for example 1.5) draws the windows at that scale, as the style pictures do (LayoutManager's
			//scale, the date pickers' and up-down boxes' fonts as Windows scales them); the pictures are named <window>-<percent>.png.
			float scale=1f;
			string textScale=Environment.GetEnvironmentVariable(VARIABLE_SCALE);
			if(!string.IsNullOrWhiteSpace(textScale)) {
				scale=float.Parse(textScale.Trim(),System.Globalization.CultureInfo.InvariantCulture);
			}
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				using VisualStylesScope visualStylesScope=isVisualStyles ? new VisualStylesScope() : null;
				if(isVisualStyles && !visualStylesScope.IsActive) {
					listFailed.Add("Windows' visual styles could not be activated.");
				}
				OpenPmsDental.UI.UiLooks.IsOffForGallery=isLooksOff;
				DesignSheetWindows.IsPrintingWithoutChildren=true;
				List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
				foreach(string window in arrayWindows) {
					ScanType scanType=listScanTypes.FirstOrDefault(x => x.WindowKey==window);
					if(scanType==null) {
						listFailed.Add(window+": not a scanned type");
						continue;
					}
					SetGalleryFont(font);
					try {
						using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
							Control root=builtWindow.Root;
							if(root is FormODBase formODBase && formODBase.PanelClient!=null) {
								root=formODBase.PanelClient;
							}
							if(scale!=1f) {
								builtWindow.LayOutAt(scale);
							}
							SettleAfterHandles(builtWindow);
							if(scale!=1f) {
								GridStyleGallery.ScaleFontsAsWindowsDoes(root,scale);
							}
							HideDatePickerCalendars(root);
							FillSampleContent(root,window);
							List<string[]> listOwnerHere=listOwner.Where(x => x[1]==window).ToList();
							using(Bitmap bitmap=DesignSheetWindows.Capture(root,Rectangle.Empty)) {
								string name=window.Substring(window.LastIndexOf('.')+1);
								if(scale!=1f) {
									name+="-"+(int)Math.Round(scale*100);
								}
								bitmap.Save(Path.Combine(folder,name+".png"),ImageFormat.Png);
								countPictures++;
								foreach(string[] owner in listOwnerHere) {
									Control control=FindByName(root,owner[2]);
									if(control==null) {
										continue;
									}
									Rectangle rectangle=GetBoundsIn(control,root);
									using(Bitmap bitmapOutlined=new Bitmap(bitmap)) {
										using(Graphics g=Graphics.FromImage(bitmapOutlined)) {
											using(Pen pen=new Pen(Color.Red,2)) {
												g.DrawRectangle(pen,rectangle);
											}
										}
										Rectangle rectangleCrop=Rectangle.Inflate(rectangle,60,30);
										rectangleCrop.Intersect(new Rectangle(0,0,bitmapOutlined.Width,bitmapOutlined.Height));
										Directory.CreateDirectory(Path.Combine(folder,"owner"));
										using(Bitmap bitmapCrop=bitmapOutlined.Clone(rectangleCrop,bitmapOutlined.PixelFormat)) {
											bitmapCrop.Save(Path.Combine(folder,"owner",name+"-"+owner[2]+".png"),ImageFormat.Png);
										}
									}
								}
							}
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
			});
			DesignSheetWindows.IsPrintingWithoutChildren=false;
			OpenPmsDental.UI.UiLooks.IsOffForGallery=false;
			File.WriteAllLines(Path.Combine(folder,"gallery-failed.txt"),listFailed);
			Assert.IsTrue(countPictures>0,"No picture made: "+string.Join("; ",listFailed));
		}

		///<summary>UI-12 fix round: creates the window's handles, then lays the window out again at its scale before it is drawn. Creating the
		///handles resizes the window once more (Windows' own anchoring runs on the way, and a control with a minimum size, FormClaimEdit's
		///procedure grid, keeps the difference), which left the right and bottom edges of FormClaimEdit past its picture: its grid ran to the
		///picture's edge and its bottom bar was cut. The second layout puts every control where LayoutManager puts it in the designer's client
		///area, as the window's Load does when it sets the client area (DoCalculateClientArea).</summary>
		public static void SettleAfterHandles(BuiltWindow builtWindow) {
			if(builtWindow.ScanType==null || builtWindow.ScanType.Kind!=BuildKind.LayoutManagerWindow || builtWindow.Form==null) {
				return;
			}
			//A window the designer opens maximized (FormEmailInbox) would take the screen's size with its handle: the picture shows the designer's
			//client area.
			builtWindow.Form.WindowState=FormWindowState.Normal;
			DesignSheetWindows.CreateHandles(builtWindow.Form);
			builtWindow.LayOutAt(builtWindow.Scale);
		}

		///<summary>UI-13 fix round: synthetic content for the windows that show a type only with items (their Load fills them from the database,
		///which the harness does not run): FormAutoNotes' tree (groups with notes, one expanded, one selected), FormQuery's result grid (columns,
		///rows and its caption) and FormProcButtons' list of buttons
		///(a category selected, as SetVisibility shows a category other than the quick buttons).</summary>
		public static void FillSampleContent(Control root,string window) {
			if(window=="OpenPmsDental.FormAutoNotes" && FindByName(root,"treeNotes") is TreeView treeView) {
				treeView.Nodes.Clear();
				string[] arrayGroups=new string[] { "Exams","Fillings","Crowns","Endo","Hygiene" };
				for(int i=0;i<arrayGroups.Length;i++) {
					TreeNode treeNode=treeView.Nodes.Add(arrayGroups[i]);
					for(int j=1;j<=3;j++) {
						treeNode.Nodes.Add(arrayGroups[i]+" note "+j);
					}
					if(i<2) {
						treeNode.Expand();
					}
				}
				treeView.SelectedNode=treeView.Nodes[0].Nodes[1];
			}
			if(window=="OpenPmsDental.FormRpTreatmentFinder" && FindByName(root,"comboClinics") is OpenPmsDental.UI.ComboBoxClinicPicker comboBoxClinicPicker) {
				//UI-12 slice 1: its Load makes the clinic picker a multi select (not on a middle tier client), so it reads "Clinics" and its box
				//stands on the Provider and Billing Type column; the picture shows it so.
				comboBoxClinicPicker.IsMultiSelect=true;
			}
			if(window=="OpenPmsDental.FormQuery" && FindByName(root,"grid2") is DataGridView dataGridView) {
				//A result as the window shows it after a query: its caption over the grid, columns and rows.
				if(FindByName(root,"labelGridCaption") is Label labelCaption) {
					labelCaption.Text="Query results";
				}
				dataGridView.Columns.Clear();
				foreach(string column in new string[] { "PatNum","LName","FName","Balance" }) {
					dataGridView.Columns.Add("col"+column,column);
				}
				string[] arrayNames=new string[] { "Able","Baker","Carter","Dixon","Ellis","Foster" };
				for(int i=0;i<arrayNames.Length;i++) {
					dataGridView.Rows.Add((i+1).ToString(),arrayNames[i],"Sample",(i*37.5).ToString("F2"));
				}
			}
			if(window=="OpenPmsDental.FormProcButtons") {
				if(FindByName(root,"listCategories") is OpenPmsDental.UI.ListBox listBox) {
					listBox.Items.Clear();
					foreach(string category in new string[] { "Quick Buttons","Exams","Restorative","Hygiene" }) {
						listBox.Items.Add(category);
					}
					listBox.SelectedIndex=1;
				}
				foreach(string name in new string[] { "panelQuickButtons","labelEdit" }) {
					Control control=FindByName(root,name);
					if(control!=null) {
						control.Visible=false;
					}
				}
				if(FindByName(root,"listViewButtons") is ListView listView) {
					listView.Items.Clear();
					string[] arrayButtons=new string[] { "Periodic exam","Comprehensive exam","Limited exam","Bitewings","Panoramic","Prophy" };
					for(int i=0;i<arrayButtons.Length;i++) {
						ListViewItem listViewItem=new ListViewItem(arrayButtons[i]);
						if(listView.SmallImageList!=null && listView.SmallImageList.Images.Count>0) {
							listViewItem.ImageIndex=i % listView.SmallImageList.Images.Count;
						}
						listView.Items.Add(listViewItem);
					}
					if(listView.Columns.Count>0) {
						listView.Columns[0].Width=listView.ClientSize.Width-10;
					}
					listView.Items[1].Selected=true;
				}
			}
		}

		///<summary>Hides the calendar of every ODDatePicker as the picker's Load does (the harness builds windows without Load): a calendar left
		///visible shows its top edge as a line under the date box.</summary>
		public static void HideDatePickerCalendars(Control control) {
			foreach(Control child in control.Controls) {
				if(child is OpenPmsDental.UI.ODDatePicker odDatePicker) {
					odDatePicker.HideCalendar();
					continue;
				}
				HideDatePickerCalendars(child);
			}
		}

		///<summary>The font of the gallery's pictures (VARIABLE_FONT): "program" sets no token override and the default font to the body token's
		///family and size; "today" today's font through the override; anything else Segoe UI 9 through the override.</summary>
		public static void SetGalleryFont(string font) {
			if(font=="program") {
				DefaultFontOverride.Set(UiTheme.FontBody.FontFamily.Name==TextFitFontOption.Today.Family ? TextFitFontOption.Today : TextFitFontOption.Segoe9,1f);
				return;
			}
			if(font=="today") {
				DefaultFontOverride.Set(TextFitFontOption.Today,1f);
				UiTheme.SetFontForTest(UiFontOption.Today);
				return;
			}
			DefaultFontOverride.Set(TextFitFontOption.Segoe9,1f);
			UiTheme.SetFontForTest(UiFontOption.UI2b);
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
