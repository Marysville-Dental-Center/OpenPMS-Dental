// UI-CAL (docs/port-notes/UI-CAL.md): the gallery's pictures of a stock date picker's drop-down open (G-UI3-uical): the window drawn as
// FitGallery draws it, with the program's calendar (LayoutManagerForms.CreateDropCalendar, as UiDateDropLook opens it) on the picker's date
// drawn where the drop-down opens, below the picker (UiDateDropLook.GetDropLocation without a screen). The window's first date picker with a
// drop-down, or the one OPMS_UI_CAL_DROP_WINDOWS names after a '#' (<window>#<picker name>). Opt-in: OPMS_UI_CAL_DROP_GALLERY names the folder;
// OPMS_UI_FIT_GALLERY_SCALE as FitGallery. Pictures <window>-drop[-<percent>].png. Nothing is shown; no database.
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
using UiRefreshTests.Scan;
using UiRefreshTests.TextFit;

namespace UiRefreshTests {
	[TestClass]
	public class CalendarDropGallery {
		public const string VARIABLE_FOLDER="OPMS_UI_CAL_DROP_GALLERY";
		public const string VARIABLE_WINDOWS="OPMS_UI_CAL_DROP_WINDOWS";
		///<summary>A few windows with a stock date picker: a payment, a search, a report, a graph's filter.</summary>
		public static readonly string[] ArrayWindows=new string[] {
			"OpenPmsDental.FormPayment#datePickFrom","OpenPmsDental.FormTaskSearch#dateCreatedFrom","OpenPmsDental.FormRpNetProdDetail#dtPickerFrom",
			"OpenPmsDental.FormIncomeTransferManage#datePickerAsOf","OpenPmsDentalGraph.GraphIncome","OpenPmsDental.ControlTreat",
		};

		[TestMethod]
		public void Gallery_DatePickerDropDowns_InTheProgramsCalendar() {
			string folder=Environment.GetEnvironmentVariable(VARIABLE_FOLDER);
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: "+VARIABLE_FOLDER+" names the folder for the drop-down pictures.");
			}
			string windows=Environment.GetEnvironmentVariable(VARIABLE_WINDOWS);
			string[] arrayWindows=string.IsNullOrWhiteSpace(windows) ? ArrayWindows : windows.Split(new[] { ',' },StringSplitOptions.RemoveEmptyEntries);
			float scale=1f;
			string textScale=Environment.GetEnvironmentVariable(FitGallery.VARIABLE_SCALE);
			if(!string.IsNullOrWhiteSpace(textScale)) {
				scale=float.Parse(textScale.Trim(),System.Globalization.CultureInfo.InvariantCulture);
			}
			Directory.CreateDirectory(folder);
			List<string> listFailed=new List<string>();
			int countPictures=0;
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				//With Windows' visual styles, as the program runs (and as the date pickers' window class is then registered for the later tests).
				using VisualStylesScope visualStylesScope=new VisualStylesScope();
				bool isBuildingBefore=WindowBuilder.IsBuildingByConstructor;
				WindowBuilder.IsBuildingByConstructor=true;
				DesignSheetWindows.IsPrintingWithoutChildren=true;
				try {
					List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
					foreach(string windowAndPicker in arrayWindows) {
						string window=windowAndPicker.Split('#')[0];
						string namePicker=windowAndPicker.Contains("#") ? windowAndPicker.Substring(windowAndPicker.IndexOf('#')+1) : "";
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
								if(scale!=1f) {
									builtWindow.LayOutAt(scale);
								}
								FitGallery.SettleAfterHandles(builtWindow);
								if(scale!=1f) {
									GridStyleGallery.ScaleFontsAsWindowsDoes(root,scale);
								}
								FitGallery.HideDatePickerCalendars(root);
								FitGallery.FillSampleContent(root,window);
								DateTimePicker dateTimePicker=FinalSweep.Walk(root).OfType<DateTimePicker>()
									.FirstOrDefault(x => !x.ShowUpDown && x.Width>0 && (namePicker=="" || x.Name==namePicker));
								if(dateTimePicker==null) {
									listFailed.Add(window+": no date picker with a drop-down shown");
									continue;
								}
								if(!UiDateDropLook.IsProgramCalendar(dateTimePicker)) {
									listFailed.Add(window+": the picker "+dateTimePicker.Name+"'s drop-down is Windows' calendar");
									continue;
								}
								using(Bitmap bitmapWindow=DesignSheetWindows.Capture(root,Rectangle.Empty))
								using(Bitmap bitmap=DrawWithDropDown(bitmapWindow,dateTimePicker,root)) {
									string name=window.Substring(window.LastIndexOf('.')+1)+"-drop";
									if(scale!=1f) {
										name+="-"+(int)Math.Round(scale*100);
									}
									bitmap.Save(Path.Combine(folder,name+".png"),ImageFormat.Png);
									countPictures++;
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
				}
				finally {
					WindowBuilder.IsBuildingByConstructor=isBuildingBefore;
					DesignSheetWindows.IsPrintingWithoutChildren=false;
				}
			});
			File.WriteAllLines(Path.Combine(folder,"drop-failed.txt"),listFailed);
			Assert.AreEqual(0,listFailed.Count,string.Join(Environment.NewLine,listFailed));
			Assert.IsTrue(countPictures>0);
		}

		///<summary>The window's picture with the program's calendar of the picker's drop-down on it, where it opens; the picture grows when the
		///calendar reaches past the window (the drop-down is a window of its own), the new part in the window's background.</summary>
		public static Bitmap DrawWithDropDown(Bitmap bitmapWindow,DateTimePicker dateTimePicker,Control root) {
			Control control=LayoutManagerForms.CreateDropCalendar(dateTimePicker);
			try {
				IUiDropCalendar uiDropCalendar=(IUiDropCalendar)control;
				uiDropCalendar.MinDate=dateTimePicker.MinDate.Date;
				uiDropCalendar.MaxDate=dateTimePicker.MaxDate.Date;
				uiDropCalendar.SelectionStart=dateTimePicker.Value.Date;
				Rectangle rectanglePicker=DesignSheetWindows.GetBoundsIn(dateTimePicker,root);
				int x=dateTimePicker.DropDownAlign==LeftRightAlignment.Right ? dateTimePicker.Width-control.Width : 0;
				Rectangle rectangleCalendar=new Rectangle(rectanglePicker.X+x,rectanglePicker.Bottom,control.Width,control.Height);
				Rectangle rectangleAll=Rectangle.Union(new Rectangle(0,0,bitmapWindow.Width,bitmapWindow.Height),rectangleCalendar);
				Bitmap bitmap=new Bitmap(rectangleAll.Width,rectangleAll.Height,PixelFormat.Format24bppRgb);
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(UiTheme.GetColor(EnumUiColor.Surface));
					g.DrawImage(bitmapWindow,-rectangleAll.X,-rectangleAll.Y,bitmapWindow.Width,bitmapWindow.Height);
					using(Bitmap bitmapCalendar=new Bitmap(control.Width,control.Height,PixelFormat.Format32bppArgb))
					using(Graphics gCalendar=Graphics.FromImage(bitmapCalendar)) {
						MethodInfo methodInfo=typeof(OpenPmsDental.UI.MonthCalendarOD).GetMethod("OnPaint",BindingFlags.Instance|BindingFlags.NonPublic);
						methodInfo.Invoke(control,new object[] { new PaintEventArgs(gCalendar,new Rectangle(0,0,control.Width,control.Height)) });
						g.DrawImage(bitmapCalendar,rectangleCalendar.X-rectangleAll.X,rectangleCalendar.Y-rectangleAll.Y,control.Width,control.Height);
					}
				}
				return bitmap;
			}
			finally {
				control.Dispose();
			}
		}
	}
}
