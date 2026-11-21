// Gallery G-UI3 for the UI-10 slices (docs/specs/ui-refresh-plan.md sections 5.4 and 8; docs/port-notes/UI-10.md): the windows of a slice drawn off
// screen in the new font (Segoe UI 9 pt) through the token override UiTheme.SetFontForTest, the product's own font path, with the designer files
// as they are now. Run it before the slice's fixes (the before folder) and after (the after folder); tools/New-UiGallery.ps1 makes the page.
// Opt-in: OPMS_UI_FIT_GALLERY names the folder, OPMS_UI_FIT_GALLERY_WINDOWS the window types (full names, comma separated; default: the slice 1
// list below), OPMS_UI_FIT_GALLERY_OWNER a TSV (designer_file, window, control, reason) of the "needs the owner" controls: each is drawn outlined
// in red on the window's picture and cropped into owner\ . Nothing is shown; no database.
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
			int countPictures=0;
			List<string> listFailed=new List<string>();
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
				foreach(string window in arrayWindows) {
					ScanType scanType=listScanTypes.FirstOrDefault(x => x.WindowKey==window);
					if(scanType==null) {
						listFailed.Add(window+": not a scanned type");
						continue;
					}
					DefaultFontOverride.Set(TextFitFontOption.Segoe9,1f);
					UiTheme.SetFontForTest(UiFontOption.UI2b);
					try {
						using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
							Control root=builtWindow.Root;
							if(root is FormODBase formODBase && formODBase.PanelClient!=null) {
								root=formODBase.PanelClient;
							}
							List<string[]> listOwnerHere=listOwner.Where(x => x[1]==window).ToList();
							using(Bitmap bitmap=DesignSheetWindows.Capture(root,Rectangle.Empty)) {
								string name=window.Substring(window.LastIndexOf('.')+1);
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
			File.WriteAllLines(Path.Combine(folder,"gallery-failed.txt"),listFailed);
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
