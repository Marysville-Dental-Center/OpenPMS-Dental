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
		///<summary>UI-PLAIN-2: OPMS_UI_FIT_GALLERY_FRAME=1 draws each plain window whole, with its title bar (PlainLookCopy.CaptureWindow: the
		///program's frame when the window has it, else Windows' frame as Windows prints it), named <window>-frame; so does the state "frame"
		///(<window>#frame) for one window. In the window list, "@plainframes" stands for every plain window of PlainLookTests.ArrayPlainAssemblies
		///with the state "frame".</summary>
		public const string VARIABLE_FRAME="OPMS_UI_FIT_GALLERY_FRAME";

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
			bool isFrame=Environment.GetEnvironmentVariable(VARIABLE_FRAME)=="1";
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
				CodeBase.UiLooks.IsOffForGallery=isLooksOff;
				WindowBuilder.IsBuildingByConstructor=true;//UI-PLAIN fix round: OpenPmsDentalDbSettings' windows with their controls
				DesignSheetWindows.IsPrintingWithoutChildren=true;
				DesignSheetWindows.PaintDpiScale=scale;//the insurance summaries' text at the scale's dpi (DesignSheetWindows.PaintDpiScale)
				List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
				if(arrayWindows.Contains("@plainframes")) {
					List<string> listExpanded=arrayWindows.Where(x => x!="@plainframes").ToList();
					listExpanded.AddRange(listScanTypes.Where(x => x.Kind==BuildKind.PlainWindow && PlainLookTests.ArrayPlainAssemblies.Contains(x.Assembly)
						&& !PlainLookTests.ArrayNotWindows.Contains(x.Type.FullName)
						&& x.Type.FullName!="fyiReporting.RdlDesign.MDIChild")//only ever an MDI child: Windows' MDI frame, not the program's
						.Select(x => x.WindowKey+"#frame"));
					arrayWindows=listExpanded.ToArray();
				}
				//UI-11-final: "@all" stands for every scanned type (the final sweep's pictures at 100%), "@densest" for the tenth of them with the
				//most controls (the sweep's 150% pictures; FinalSweep.GetDensest).
				if(arrayWindows.Contains("@all") || arrayWindows.Contains("@densest")) {
					List<string> listExpanded=arrayWindows.Where(x => x!="@all" && x!="@densest").ToList();
					if(arrayWindows.Contains("@all")) {
						listExpanded.AddRange(FinalSweep.GetSweepTypes(listScanTypes).Select(x => x.WindowKey));
					}
					if(arrayWindows.Contains("@densest")) {
						listExpanded.AddRange(FinalSweep.GetDensest(listScanTypes,0.1));
					}
					arrayWindows=listExpanded.Distinct().ToArray();
				}
				foreach(string windowAndState in arrayWindows) {
					//UI-12 slice 2 fix round: "<window>#<state>" draws the window in one of the states of FillSampleState, named <window>-<state>.
					string window=windowAndState;
					string state="";
					int indexHash=windowAndState.IndexOf('#');
					if(indexHash>=0) {
						window=windowAndState.Substring(0,indexHash);
						state=windowAndState.Substring(indexHash+1);
					}
					bool isFrameHere=isFrame;
					if(state=="frame") {
						isFrameHere=true;
						state="";
					}
					ScanType scanType=listScanTypes.FirstOrDefault(x => x.WindowKey==window);
					if(scanType==null) {
						listFailed.Add(window+": not a scanned type");
						continue;
					}
					SetGalleryFont(font);
					//UI-PLAIN-2 fix round: a plain exe's menus and tool strips as its Main has them, the token renderer (UiTheme.InitWinForms) in the
					//after pictures, the stock renderer in the before pictures.
					ToolStripRenderer toolStripRendererBefore=ToolStripManager.Renderer;
					if(!isLooksOff && scanType.Kind==BuildKind.PlainWindow) {
						ToolStripManager.Renderer=new CodeBase.UiToolStripRenderer();
					}
					try {
						//UI-PLAIN-2: the plain windows' frame at the gallery's scale (the harness is a 96 dpi process).
						PlainLookCopy.SetFrameScaleForTest(scanType.Type.Assembly,scale);
						PlainLookCopy.SetLooksOffForGallery(scanType.Type.Assembly,isLooksOff);//the copy a window's constructor attaches
						FillSampleCache(window,state);
						using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
							Control root=builtWindow.Root;
							if(root is FormODBase formODBase && formODBase.PanelClient!=null) {
								root=formODBase.PanelClient;
							}
							if(scale!=1f) {
								builtWindow.LayOutAt(scale);
							}
							SettleAfterHandles(builtWindow);
							//The state before the 150% font pass: a tab page shown lays its page out again (TabControl.LayoutTabs), with
							//LayoutManager's zoom-only fonts.
							//UI-12 slice 13 fix round: a plain exe's Form that scales itself (the report designer's dialogs, per-monitor aware) is
							//scaled as a whole, its bounds and every font, before its state is shown.
							bool isScaledAsPlainExe=scale!=1f && IsScaledByItsAutoScale(builtWindow);
							if(isScaledAsPlainExe) {
								ScaleAsPlainExe(builtWindow.Form,scale);
							}
							Control controlPicture=FillSampleState(root,window,state);
							if(scale!=1f && !isScaledAsPlainExe) {
								GridStyleGallery.ScaleFontsAsWindowsDoes(root,scale);
							}
							HideDatePickerCalendars(root);
							FillSampleContent(root,window,state);
							List<string[]> listOwnerHere=controlPicture==root ? listOwner.Where(x => x[1]==window).ToList() : new List<string[]>();
							//UI-PLAIN-2: a plain window with the program's frame is drawn whole, its title bar on top, in every picture of it (its
							//content is below the band); the before pictures (the looks off: no frame) keep the window's printing, unless "frame" asks
							//for the whole window with Windows' frame.
							bool isWindowPicture=builtWindow.ScanType.Kind==BuildKind.PlainWindow && controlPicture==root;
							bool isFramePicture=isWindowPicture && (isFrameHere
								|| PlainLookCopy.IsTrue(builtWindow.Form.GetType().Assembly,"UiPlainFrame","IsAttached",builtWindow.Form));
							using(Bitmap bitmap=isFramePicture ? PlainLookCopy.CaptureWindow(builtWindow.Form) : DesignSheetWindows.Capture(controlPicture,Rectangle.Empty)) {
								string name=window.Substring(window.LastIndexOf('.')+1);
								//UI-12 slice 12: two windows of one class name in the list (xCrudGenerator's and DocumentationBuilder's Form1) are named
								//<namespace>-<class>, so one picture does not overwrite the other.
								if(arrayWindows.Select(x => x.Split('#')[0]).Distinct().Count(x => x.Substring(x.LastIndexOf('.')+1)==window.Substring(window.LastIndexOf('.')+1))>1) {
									name=window.Replace('.','-');
								}
								if(state!="") {
									name+="-"+state;
								}
								if(isFrameHere && isWindowPicture) {
									name+="-frame";
								}
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
						//UI-PLAIN-2 fix round: the whole exception next to the list, for the failures one frame does not explain.
						File.AppendAllText(Path.Combine(folder,"gallery-failed-detail.txt"),windowAndState+Environment.NewLine+exception+Environment.NewLine+Environment.NewLine);
					}
					finally {
						ClearSampleCache(window,state);
						PlainLookCopy.SetFrameScaleForTest(scanType.Type.Assembly,0);
						PlainLookCopy.SetLooksOffForGallery(scanType.Type.Assembly,false);
						ToolStripManager.Renderer=toolStripRendererBefore;
						UiTheme.SetFontForTest(null);
						DefaultFontOverride.Set(TextFitFontOption.Today,1f);
					}
				}
			});
			DesignSheetWindows.IsPrintingWithoutChildren=false;
			DesignSheetWindows.PaintDpiScale=1f;
			CodeBase.UiLooks.IsOffForGallery=false;
			WindowBuilder.IsBuildingByConstructor=false;
			File.WriteAllLines(Path.Combine(folder,"gallery-failed.txt"),listFailed);
			Assert.IsTrue(countPictures>0,"No picture made: "+string.Join("; ",listFailed));
		}

		///<summary>UI-PLAIN-2 fix round: a new report of synthetic content, as the report designer's File, New Report opens the wizard's RDL.</summary>
		private const string SAMPLE_REPORT_RDL=@"<?xml version='1.0' encoding='UTF-8'?>
<Report xmlns='http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition' xmlns:rd='http://schemas.microsoft.com/SQLServer/reporting/reportdesigner'>
	<Description>Sample</Description>
	<Width>7.5in</Width>
	<TopMargin>.25in</TopMargin>
	<LeftMargin>.25in</LeftMargin>
	<RightMargin>.25in</RightMargin>
	<BottomMargin>.25in</BottomMargin>
	<PageHeader>
		<Height>.5in</Height>
		<ReportItems>
			<Textbox Name='Title'><Top>.1in</Top><Left>.1in</Left><Width>4in</Width><Height>.3in</Height><Value>Sample Report</Value><Style><FontSize>15pt</FontSize><FontWeight>Bold</FontWeight></Style></Textbox>
		</ReportItems>
		<PrintOnFirstPage>true</PrintOnFirstPage>
		<PrintOnLastPage>true</PrintOnLastPage>
	</PageHeader>
	<Body>
		<Height>1.5in</Height>
		<ReportItems>
			<Textbox Name='Note'><Top>.2in</Top><Left>.1in</Left><Width>3in</Width><Height>.25in</Height><Value>Synthetic text</Value></Textbox>
		</ReportItems>
	</Body>
	<PageFooter>
		<Height>.3in</Height>
		<PrintOnFirstPage>true</PrintOnFirstPage>
		<PrintOnLastPage>true</PrintOnLastPage>
	</PageFooter>
</Report>";

		///<summary>UI-PLAIN-2 fix round, RdlDesigner "report": one new report open, as File, New Report opens it (RdlDesigner.CreateMDIChild with the
		///RDL): a maximized MDI child (the tabbed interface), its tab in the toolbar's tab strip, its buttons merged into the menu bar. The RDL is
		///first tried on a child of its own, because CreateMDIChild shows a message box on an error.</summary>
		internal static void OpenSampleReport(Form form) {
			Type typeChild=form.GetType().Assembly.GetType("fyiReporting.RdlDesign.MDIChild",throwOnError:true);
			using(Form formTry=(Form)Activator.CreateInstance(typeChild,new object[] { 400,300 })) {
				typeChild.GetProperty("SourceRdl").SetValue(formTry,SAMPLE_REPORT_RDL);
			}
			//The handles of the window and its controls, the MDI area's among them, which the child is made in (Show makes them all on screen;
			//the window's handle alone leaves the MDI area without one, and the child's handle then fails when it is activated).
			DesignSheetWindows.CreateHandles(form);
			//The window has loaded (its frame on, its content below the band) before a report is opened, as on screen.
			if(PlainLookCopy.IsTrue(form.GetType().Assembly,"UiPlainFrame","IsAttached",form)) {
				PlainLookCopy.Call(form.GetType().Assembly,"UiPlainFrame","LayOutAsOnLoad",form);
			}
			System.Reflection.MethodInfo methodInfo=form.GetType().GetMethod("CreateMDIChild",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
			object mdiChild=methodInfo.Invoke(form,new object[] { null,SAMPLE_REPORT_RDL,false });
			if(mdiChild==null) {
				throw new InvalidOperationException("RdlDesigner.CreateMDIChild opened no report.");
			}
			//A hidden MDI window activates no child (Windows does on screen): activated as Windows Forms does it on WM_MDIACTIVATE, which merges
			//the maximized child's caption buttons into the menu bar and raises MdiChildActivate (the designer's toolbar follows the report).
			if(form.ActiveMdiChild==null) {
				System.Reflection.MethodInfo methodInfoActivate=typeof(Form).GetMethod("ActivateMdiChild",
					System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic,null,new Type[] { typeof(Form) },null);
				methodInfoActivate.Invoke(form,new object[] { mdiChild });
			}
			Application.DoEvents();
		}

		///<summary>UI-12 slice 13 fix round: whether a plain Form scales itself to the screen's dpi as a whole: the old Form.AutoScale (on for a
		///Form whose designer code sets only AutoScaleBaseSize, as the report designer's dialogs do; Form.OnLoad scales the window and its
		///controls by its font's size at the dpi against that base size, about 1.47 at 150% against 100%). A Form that sets AutoScaleMode (None:
		///the report designer's PropertyDialog, which hosts its property pages unscaled) and the UserControls hosted in a test Form are not.</summary>
		public static bool IsScaledByItsAutoScale(BuiltWindow builtWindow) {
			if(builtWindow.ScanType==null || builtWindow.ScanType.Kind!=BuildKind.PlainWindow || builtWindow.Form==null) {
				return false;
			}
			//Read by reflection: the property is obsolete (and hidden) in .NET's Form.
			return typeof(Form).GetProperty("AutoScale")?.GetValue(builtWindow.Form) is bool isAutoScale && isAutoScale;
		}

		///<summary>UI-12 slice 13 fix round: a plain exe's Form at a scale as its AutoScale draws it on a screen of that dpi (per-monitor aware,
		///RdlDesigner's Main): every font of the window at the scale (Windows draws a point size at the dpi), then the window and its controls
		///scaled by it (Control.Scale, as Form.ApplyAutoScaling does). Every font is read before any is set (ambient fonts).</summary>
		public static void ScaleAsPlainExe(Form form,float scale) {
			List<Tuple<Control,Font>> listFonts=new List<Tuple<Control,Font>>();
			CollectEveryFont(form,listFonts);
			foreach(Tuple<Control,Font> tuple in listFonts) {
				tuple.Item1.Font=new Font(tuple.Item2.FontFamily,tuple.Item2.Size*scale,tuple.Item2.Style);
			}
			form.Scale(new SizeF(scale,scale));
		}

		private static void CollectEveryFont(Control control,List<Tuple<Control,Font>> listFonts) {
			listFonts.Add(Tuple.Create(control,control.Font));
			foreach(Control child in control.Controls) {
				CollectEveryFont(child,listFonts);
			}
		}

		///<summary>UI-12 fix round: creates the window's handles, then lays the window out again at its scale before it is drawn. Creating the
		///handles resizes the window once more (Windows' own anchoring runs on the way, and a control with a minimum size, FormClaimEdit's
		///procedure grid, keeps the difference), which left the right and bottom edges of FormClaimEdit past its picture: its grid ran to the
		///picture's edge and its bottom bar was cut. The second layout puts every control where LayoutManager puts it in the designer's client
		///area, as the window's Load does when it sets the client area (DoCalculateClientArea).
		///<para>UI-12 slice 2 fix round (review of 2026-11-23): a UserControl hosted in its test FormODBase (a module, LayoutManager.Add; a
		///preference or setup wizard page, AddUnscaled) at a scale other than 100% too. Its host's handle was made only by the capture, after the
		///150% font pass (GridStyleGallery.ScaleFontsAsWindowsDoes), and the host's first resize laid the page out again with LayoutManager's
		///zoom-only fonts, so its labels, check boxes and buttons drew at the 100% size beside boxes at the 150% size. With the handles made and
		///the layout done first, the font pass comes last, as for the windows. At 100% the hosted pictures stay as they were drawn.</para></summary>
		public static void SettleAfterHandles(BuiltWindow builtWindow) {
			if(builtWindow.ScanType==null || builtWindow.Form==null) {
				return;
			}
			bool isHosted=builtWindow.ScanType.Kind==BuildKind.UserControlAdd || builtWindow.ScanType.Kind==BuildKind.UserControlAddUnscaled;
			if(isHosted && builtWindow.Scale!=1f) {
				DesignSheetWindows.CreateHandles(builtWindow.Form);
				builtWindow.LayOutAt(builtWindow.Scale);
				return;
			}
			if(builtWindow.ScanType.Kind!=BuildKind.LayoutManagerWindow) {
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
		///<summary>UI-12 slice 6: the windows whose pictures show the run-time texts of their placeholder labels (FillSampleContent).</summary>
		private static List<string> _listRuntimeTextWindows=new List<string> {
			"OpenPmsDental.FormDatabaseMaintInvalidChars","OpenPmsDental.FormMaintHistory","OpenPmsDental.FormMaintRecordEdit",
			"OpenPmsDental.FormTreatPlanAddProcs","OpenPmsDental.FormSupplyStock","OpenPmsDental.FormPrinterEdit","OpenPmsDental.FormTerminalManager"
		};

		public static void FillSampleContent(Control root,string window,string state="") {
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
			if(window=="OpenPmsDental.FormReportSetup" || window=="OpenPmsDental.User_Controls.UserControlReportSetup") {
				//UI-12 slice 1: UserControlReportSetup shows its instructions on the Display Settings page and its user group row on the Security
				//Permissions page (InitializeOnStartup sets Visible by the mode, never both); the first page shows the display mode. UI-12 slice 4
				//fix round: the page alone too (its permission mode is the state "permission").
				SetReportSetupMode(root,isPermissionMode:state=="permission");
			}
			if(window=="OpenPmsDentalGraph.DashboardTabCtrl" && FindByName(root,"tabControl") is System.Windows.Forms.TabControl tabControlDashboard
				&& tabControlDashboard.TabCount==0)
			{
				//UI-PLAIN: the dashboard adds a page per saved dashboard tab from the database (its Load); the picture shows two synthetic ones, the
				//first selected, so the stock tab control's look (UiTabLook) shows.
				tabControlDashboard.TabPages.Add(new System.Windows.Forms.TabPage("Production"));
				tabControlDashboard.TabPages.Add(new System.Windows.Forms.TabPage("Appointments"));
				tabControlDashboard.SelectedIndex=0;
			}
			if(window=="CentralManager.FormCentralReportSetup") {
				//UI-12 slice 4 fix round: both callers open it in permission mode (FormCentralManager, FormCentralSecurity).
				SetReportSetupMode(root,isPermissionMode:true);
			}
			if(window=="OpenPmsDental.FormRecallTypeEdit") {
				//UI-12 slice 5: the special type shows the trigger rows and the Default Interval group, or (Child Prophy, the state "childprophy")
				//the Child Prophy group on their band (comboSpecial_SelectionChangeCommitted sets Visible, never both).
				bool isChildProphy=state=="childprophy";
				foreach(string name in new string[] { "listTriggers","labelTriggers","labelTriggerDisable","butAddTrigger","butRemoveTrigger","groupInterval" }) {
					if(FindByName(root,name) is Control control) {
						control.Visible=!isChildProphy;
					}
				}
				if(FindByName(root,"groupAgeLimit") is Control controlAgeLimit) {
					controlAgeLimit.Visible=isChildProphy;
				}
			}
			if(window=="OpenPmsDental.FormTransactionEdit") {
				//UI-12 slice 5: Load shows the simple panel or the compound one (FillSimple, FillCompound), never both; the picture shows the simple,
				//or (fix round 2, the state "compound", every compound transaction) the compound.
				bool isCompound=state=="compound";
				if(FindByName(root,"panelSimple") is Control controlSimple) {
					controlSimple.Visible=!isCompound;
				}
				if(FindByName(root,"panelCompound") is Control controlCompound) {
					controlCompound.Visible=isCompound;
				}
			}
			if(window=="OpenPmsDental.FormSheetDefEdit") {
				//UI-12 slice 13 fix round: Load shows the internal-sheet note or the editing tools (Tab Order and tips among them), never both
				//(FormSheetDefEdit.cs, IsInternal): the picture shows a sheet that is not internal, or (the state "internal") an internal one.
				bool isInternal=state=="internal";
				foreach(string name in new string[] { "butDelete","butSave","groupAddField","groupShowField","groupPage","groupAlignH","butAlignTop",
					"linkLabelTips","butCopy","butPaste","butUndo","butRedo","butTabOrder" })
				{
					if(FindByName(root,name) is Control control) {
						control.Visible=!isInternal;
					}
				}
				if(FindByName(root,"labelInternal") is Control controlInternal) {
					controlInternal.Visible=isInternal;
				}
			}
			if(window=="OpenPmsDental.FormScheduleDayEdit" && FindByName(root,"labelDate") is Label labelDate) {
				//UI-12 slice 1 (fix round 8): Load and the day arrows set the day of the week over the short date, two lines (the designer's text is
				//a one-line placeholder); the picture shows the widest day name.
				DateTime date=new DateTime(2026,12,30);
				labelDate.Text=date.ToString("dddd")+"\r\n"+date.ToShortDateString();
			}
			if(window=="OpenPmsDental.ControlTreat" && FindByName(root,"userControlIndDis") is Control controlDiscount) {
				//UI-12 slice 1: FillSummary shows the discount plan summary (a patient with a discount plan) or the family and individual insurance
				//summaries, never both; the discount summary stands on the family summary's place. The picture shows the insurance summaries.
				controlDiscount.Visible=false;
			}
			//UI-12 slice 8 fix round: a radio button or check box with no designer text whose text the code sets (grid-runtime-texts) draws
			//with that text, so the picture shows whether it fits beside its box (FormDiseaseDefMerge's two merge choices).
			foreach(string[] arrayRow in GridRulesTests.ReadRuntimeTexts().Where(x => x[0]==window)) {
				Control controlChoice=FindByName(root,arrayRow[1]);
				if((controlChoice is RadioButton || controlChoice is System.Windows.Forms.CheckBox || controlChoice is OpenPmsDental.UI.CheckBox)
					&& controlChoice.Text=="")
				{
					controlChoice.Text=arrayRow[2].Replace("\\n","\r\n");
				}
			}
			if(_listRuntimeTextWindows.Contains(window)) {
				//UI-12 slice 6: Load gives these labels their text (the designer's is a placeholder or empty), and their rows stand where that text
				//goes; the picture shows the first text of each label in grid-runtime-texts (the one the layout script sized it for).
				foreach(string[] arrayRow in GridRulesTests.ReadRuntimeTexts().Where(x => x[0]==window)) {
					if(FindByName(root,arrayRow[1]) is Label label && !label.Text.Contains(" ")) {
						label.Text=arrayRow[2].Replace("\\n","\r\n");
					}
				}
			}
			if(window=="OpenPmsDental.FormTaskEdit" && FindByName(root,"groupReminder") is Control groupReminder && FindByName(root,"panelRepeating") is Control panelRepeating) {
				//UI-12 slice 2 continuation: LoadTask shows the reminder group or the repeating panel (TasksUseRepeating, off by default), both on one
				//bounds; the picture shows the default, a weekly reminder (the frequency panel with the day panel under it).
				groupReminder.Visible=true;
				panelRepeating.Visible=false;
			}
			if(window=="OpenPmsDental.User_Controls.SetupWizard.UserControlSetupWizSchedule") {
				//UI-12 slice 2 continuation: LoadProv names the provider, and a day's box shows its group (each day checked).
				if(FindByName(root,"labelProv") is Label labelProv) {
					labelProv.Text="Let's set up schedules for DMD123.";
				}
				foreach(string day in new string[] { "Monday","Tuesday","Wednesday","Thursday","Friday","Saturday","Sunday" }) {
					if(FindByName(root,"check"+day) is OpenPmsDental.UI.CheckBox checkBox) {
						checkBox.Checked=true;
					}
					if(FindByName(root,"group"+day.Substring(0,3)) is Control groupDay) {
						groupDay.Visible=true;
					}
				}
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

		///<summary>UI-12 slice 8 fix round: synthetic cache content and constructor steps a window needs before it is drawn, for a state of the window
		///list. FormInsHistSetup "rows": AddCategoryRows makes a row per code group marked Show In History (four for a Perio Four Quadrants
		///group); without code groups it shows only its note. ClearSampleCache empties the cache again after the picture.</summary>
		public static void FillSampleCache(string window,string state) {
			if(window=="OpenPmsDental.FormInsHistSetup" && state=="rows") {
				List<OpenPmsDentalBusiness.CodeGroup> listCodeGroups=new List<OpenPmsDentalBusiness.CodeGroup>();
				string[] arrayNames=new string[] { "Exam","Prophylaxis","Fluoride","Bitewing X-rays","Pano/FMX","Perio Scaling" };
				for(int i=0;i<arrayNames.Length;i++) {
					OpenPmsDentalBusiness.CodeGroup codeGroup=new OpenPmsDentalBusiness.CodeGroup();
					codeGroup.CodeGroupNum=i+1;
					codeGroup.GroupName=arrayNames[i];
					codeGroup.ProcCodes="D0120";
					codeGroup.ItemOrder=i;
					codeGroup.ShowInHistory=true;
					codeGroup.HistProcCode="D0120";
					codeGroup.IsPerioFourQuads=arrayNames[i]=="Perio Scaling";
					listCodeGroups.Add(codeGroup);
				}
				OpenPmsDentalBusiness.CodeGroups.FillCacheFromTable(OpenPmsDentalBusiness.Crud.CodeGroupCrud.ListToTable(listCodeGroups));
				//The harness runs InitializeComponent but not the constructor: the rows as the constructor makes them, before the layout manager.
				//The bare object has no field initializers either: the row list as its declaration makes it.
				WindowBuilder.ActionBeforeLayoutManager=formODBase => {
					System.Reflection.BindingFlags bindingFlags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
					System.Reflection.FieldInfo fieldInfo=formODBase.GetType().GetField("_listCategoryRows",bindingFlags);
					fieldInfo.SetValue(formODBase,Activator.CreateInstance(fieldInfo.FieldType));
					formODBase.GetType().GetMethod("AddCategoryRows",bindingFlags).Invoke(formODBase,null);
				};
			}
		}

		///<summary>UI-12 slice 8 fix round: the cache FillSampleCache filled, back to empty (it fills again from the database when read).</summary>
		public static void ClearSampleCache(string window,string state) {
			if(window=="OpenPmsDental.FormInsHistSetup" && state=="rows") {
				OpenPmsDentalBusiness.CodeGroups.ClearCache();
				WindowBuilder.ActionBeforeLayoutManager=null;
			}
		}

		///<summary>UI-12 slice 2 fix round (review of 2026-11-23): the states of the pilot follow-up windows whose fixed texts the default
		///picture does not show, named in the window list as "<window>#<state>". Returns the control to draw: the root, or the part drawn alone.
		///ControlAccount "summary": the family and individual insurance summaries (labelInsRem_MouseEnter shows them; hidden in the designer)
		///with their Annual Max captions. ControlAccount "totalowed": the TOTAL Owed panel drawn alone (the designer puts it 41 px above the
		///aging strip, where the program never shows it, on main too). ControlChart "show" and "draw": the procedure tab control on its Show and
		///Draw pages (the Show page's check boxes, the Draw page's note). FormBackup "archive": its Archive page (the Server Name note).
		///<para>UI-12 slice 3 fix round: the other tab pages of FormDatabaseMaintenance (hidden, old, tools), FormDatabaseMaintenancePat (hidden,
		///old), FormPath (database, removed, sftp), FormUserEdit (clinics, alertsubs) and FormSecurity (usergroups, its user control's User Groups
		///page), and FormCertifications "certcomplete": its Cert Complete view as radioCertification_Click shows it.</para></summary>
		public static Control FillSampleState(Control root,string window,string state) {
			if(state=="") {
				return root;
			}
			if(window=="fyiReporting.RdlDesign.RdlDesigner" && state=="report") {
				OpenSampleReport((Form)root);
				return root;
			}
			if(window=="OpenPmsDental.ControlAccount" && state=="summary") {
				foreach(string name in new string[] { "groupBoxFamilyIns","groupBoxIndIns" }) {
					FindByName(root,name).Visible=true;
				}
				return root;
			}
			if(window=="OpenPmsDental.ControlAccount" && state=="totalowed") {
				return FindByName(root,"panelTotalOwes");
			}
			//UI-11 fix round: the Show tab of the side panel (its four check boxes on the page's colour).
			if(window=="OpenPmsDental.ControlAccount" && state=="show") {
				SelectTabPage(root,"tabControlShow","tabShow");
				return root;
			}
			if(window=="OpenPmsDental.ControlChart" && (state=="show" || state=="draw")) {
				SelectTabPage(root,"tabControlProc",state=="show" ? "tabShow" : "tabDraw");
				return root;
			}
			if(window=="OpenPmsDental.FormBackup" && state=="archive") {
				SelectTabPage(root,"tabControl1","tabPageArchive");
				return root;
			}
			if(window=="OpenPmsDental.User_Controls.UserControlReportSetup" && state=="permission") {
				//UI-12 slice 4 fix round: the page in permission mode (FillSampleContent sets the mode's Visible flags).
				return root;
			}
			//UI-12 slice 16: the parts of the slice's windows that their code shows (hidden in the designer), laid out by the slice's scripts.
			Control controlSixteen=GetSliceSixteenState(root,window,state);
			if(controlSixteen!=null) {
				return controlSixteen;
			}
			//UI-12 slice 3 fix round (review of 2026-11-23): the slice's other tab pages and views, laid out by its scripts.
			string[] arrayTab=GetSliceThreeTabState(window,state) ?? GetSliceElevenTabState(window,state) ?? GetSliceThirteenTabState(window,state);
			if(arrayTab!=null) {
				if(FindByName(root,arrayTab[0]) is System.Windows.Forms.TabControl) {
					SelectStockTabPage(root,arrayTab[0],arrayTab[1]);
				}
				else {
					SelectTabPage(root,arrayTab[0],arrayTab[1]);
				}
				return root;
			}
			if(window=="OpenPmsDental.FormCertifications" && state=="certcomplete") {
				//As radioCertification_Click shows it (the harness runs no Load, which hides the Cert Complete view; the designer hides it too).
				foreach(string name in new string[] { "labelEmpSearch","textEmpSearch","labelCategories","listBoxCategories","labelEmployee","listBoxEmployee",
					"labelReportsTo","comboSupervisor","checkIncomplete","checkSortDate" })
				{
					FindByName(root,name).Visible=false;
				}
				foreach(string name in new string[] { "labelCertification","listBoxCertification","labelCategories2","listBoxCategories2","checkSortDateCertComplete" }) {
					FindByName(root,name).Visible=true;
				}
				((System.Windows.Forms.RadioButton)FindByName(root,"radioCertification")).Checked=true;
				return root;
			}
			if(window=="OpenPmsDental.FormRecallTypeEdit" && state=="childprophy") {
				//UI-12 slice 5: the Child Prophy special type (FillSampleContent sets the Visible flags).
				return root;
			}
			if(window=="OpenPmsDental.FormTransactionEdit" && state=="compound") {
				//UI-12 slice 5 fix round 2: the compound panel (FillSampleContent sets the Visible flags).
				return root;
			}
			if(window=="OpenPmsDental.FormSheetDefEdit" && state=="internal") {
				//UI-12 slice 13 fix round: an internal sheet (FillSampleContent sets the Visible flags as Load does).
				return root;
			}
			if(window=="OpenPmsDental.FormInsHistSetup" && state=="rows") {
				//UI-12 slice 8 fix round: the rows the constructor makes from the code groups FillSampleCache puts in the cache.
				return root;
			}
			if(window=="OpenPmsDentalGraph.DashboardTabCtrl" && state=="setup") {
				SetDashboardSetupMode(root);
				return root;
			}
			throw new ArgumentException("No gallery state "+state+" for "+window+".");
		}

		///<summary>UI-12 slice 4 fix round: UserControlReportSetup's two modes as InitializeOnStartup shows them (its Visible flags; the harness
		///builds the page without it): the instructions with Up and Down, or the user group row with Set All, Set None and the allowed count.</summary>
		private static void SetReportSetupMode(Control root,bool isPermissionMode) {
			foreach(string name in new string[] { "butUp","butDown","labelInstructions" }) {
				if(FindByName(root,name) is Control control) {
					control.Visible=!isPermissionMode;
				}
			}
			foreach(string name in new string[] { "butSetAll","butSetNone","comboUserGroup","labelUserGroup","labelAllowedCount" }) {
				if(FindByName(root,name) is Control control) {
					control.Visible=isPermissionMode;
				}
			}
		}

		///<summary>UI-12 slice 3 fix round: the tab control and page of a slice 3 window's state ("<window>#<state>"), or null. FormSecurity
		///"usergroups" is the User Groups page of its hosted UserControlSecurityUserGroup.</summary>
		private static string[] GetSliceThreeTabState(string window,string state) {
			switch(window+"#"+state) {
				case "OpenPmsDental.FormDatabaseMaintenance#hidden": return new string[] { "tabControlDBM","tabHidden" };
				case "OpenPmsDental.FormDatabaseMaintenance#old": return new string[] { "tabControlDBM","tabOld" };
				case "OpenPmsDental.FormDatabaseMaintenance#tools": return new string[] { "tabControlDBM","tabTools" };
				case "OpenPmsDental.FormDatabaseMaintenancePat#hidden": return new string[] { "tabControlDBM","tabHidden" };
				case "OpenPmsDental.FormDatabaseMaintenancePat#old": return new string[] { "tabControlDBM","tabOld" };
				case "OpenPmsDental.FormPath#database": return new string[] { "tabControlDataStorageType","tabInDatabase" };
				case "OpenPmsDental.FormPath#removed": return new string[] { "tabControlDataStorageType","tabStorageRemoved" };
				case "OpenPmsDental.FormPath#sftp": return new string[] { "tabControlDataStorageType","tabSftp" };
				case "OpenPmsDental.FormUserEdit#clinics": return new string[] { "tabControl1","tabClinics" };
				case "OpenPmsDental.FormUserEdit#alertsubs": return new string[] { "tabControl1","tabAlertSubs" };
				case "OpenPmsDental.FormSecurity#usergroups": return new string[] { "tabControlMain","tabPageUserGroups" };
			}
			return null;
		}

		///<summary>UI-12 slice 11: the other tab pages of FormFeeSchedTools ("<window>#<state>"), laid out by its script, or null.</summary>
		private static string[] GetSliceElevenTabState(string window,string state) {
			switch(window+"#"+state) {
				case "OpenPmsDental.FormFeeSchedTools#increase": return new string[] { "tabControlTools","tabPageIncrease" };
				case "OpenPmsDental.FormFeeSchedTools#clear": return new string[] { "tabControlTools","tabPageClear" };
				case "OpenPmsDental.FormFeeSchedTools#exportimport": return new string[] { "tabControlTools","tabPageExportImport" };
				case "OpenPmsDental.FormFeeSchedTools#globalupdates": return new string[] { "tabControlTools","tabPageGlobalUpdates" };
				case "OpenPmsDental.FormFeeSchedTools#copycode": return new string[] { "tabControlTools","tabPageCopyCode" };
			}
			return null;
		}

		///<summary>UI-12 slice 16: the states the slice's windows show at run time. ControlAppt "search": the search group where ShowSearch puts it
		///(under the pinboard, over the lower calendar panel). ControlImages "note", "import" and "pearl": the note panel, the automatic import
		///panel and the Pearl panel, drawn alone (LayoutControls places them at run time). FormTPsign "typed": both signature boxes with their
		///typed-signature rows; "changeuser" also the Change User buttons and signers' names of an electronic sheet signature box. FormSheetFillEdit "buttons": every button of the rows under the sheet (Load shows some by the sheet's state).
		///FormClaimFormEdit "internal": the internal form's note. FormTerminal "done": the thank-you line the kiosk shows when the forms are done.
		///Null for any other window and state.</summary>
		private static Control GetSliceSixteenState(Control root,string window,string state) {
			switch(window+"#"+state) {
				case "OpenPmsDental.ControlAppt#search":
					Control panelCalendar=FindByName(root,"panelCalendar");
					Control groupSearch=FindByName(root,"groupSearch");
					//As ControlAppt.LayoutSearch places it (ShowSearch): under the pinboard, the column's width, down to the tabs' bottom.
					LayoutManagerForms layoutManagerSearch=root.FindForm() is FormODBase formODBase ? WindowBuilder.GetLayoutManager(formODBase) : null;
					int ySearch=panelCalendar.Top+FindByName(root,"panelCalendarLower").Top+FindByName(root,"pinBoard").Bottom
						+(layoutManagerSearch==null ? UiGrid.ROW_GAP : layoutManagerSearch.Scale(UiGrid.ROW_GAP));
					int heightSearch=Math.Max(layoutManagerSearch==null ? 385 : layoutManagerSearch.Scale(385),FindByName(root,"tabControl").Bottom-ySearch);
					Rectangle rectangleSearch=new Rectangle(panelCalendar.Left,ySearch,panelCalendar.Width,heightSearch);
					if(layoutManagerSearch!=null) {
						layoutManagerSearch.Move(groupSearch,rectangleSearch);//the harness lays the window out again
					}
					else {
						groupSearch.Bounds=rectangleSearch;
					}
					groupSearch.Visible=true;
					groupSearch.BringToFront();
					return root;
				case "OpenPmsDental.ControlImages#note":
					return ShowPart(root,"panelNote");
				case "OpenPmsDental.ControlImages#import":
					return ShowPart(root,"panelImportAuto");
				case "OpenPmsDental.ControlImages#pearl":
					return ShowPart(root,"panelPearl");
				case "OpenPmsDental.FormTPsign#typed":
				case "OpenPmsDental.FormTPsign#changeuser":
					foreach(string name in new string[] { "labelTypeSig","textTypeSig","labelSigPractice","signatureBoxWrapperPractice","labelTypeSigPractice","textTypeSigPractice" }) {
						FindByName(root,name).Visible=true;
					}
					if(state=="changeuser") {
						//UI-12 slice 16 fix round: the Change User button and the signer's name beside both boxes, as AddChangeUserButton places them for a
						//sheet signature box that may be signed electronically (the signer's name as ChangeSigBoxUser writes it, synthetic).
						Form formTPsign=root.FindForm() ?? (Form)root;
						System.Reflection.MethodInfo methodInfo=formTPsign.GetType().GetMethod("AddChangeUserButton",
							System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
						Control panelSig=FindByName(root,"panelSig");
						foreach((string nameBox,OpenPmsDentalBusiness.SheetFieldType sheetFieldType,string user) in new[] {
							("signatureBoxWrapper",OpenPmsDentalBusiness.SheetFieldType.SigBox,"patient1"),
							("signatureBoxWrapperPractice",OpenPmsDentalBusiness.SheetFieldType.SigBoxPractice,"provider1") })
						{
							int countBefore=panelSig.Controls.Count;
							OpenPmsDentalBusiness.SheetField sheetField=new OpenPmsDentalBusiness.SheetField { FieldType=sheetFieldType,CanElectronicallySign=true };
							methodInfo.Invoke(formTPsign,new object[] { sheetField,FindByName(root,nameBox) });
							foreach(Control control in panelSig.Controls.Cast<Control>().Skip(countBefore).OfType<Label>()) {
								control.Text="Signer: "+user;
							}
						}
					}
					return root;
				case "OpenPmsDental.FormSheetFillEdit#buttons":
					foreach(Control control in FindByName(root,"butOK").Parent.Controls) {
						if(control is OpenPmsDental.UI.Button || control is OpenPmsDental.UI.CheckBox) {
							control.Visible=true;
						}
					}
					return root;
				case "OpenPmsDental.FormClaimFormEdit#internal":
					FindByName(root,"labelInternal").Visible=true;
					return root;
				case "OpenPmsDental.FormTerminal#done":
					FindByName(root,"labelThankYou").Visible=true;
					return root;
			}
			return null;
		}

		///<summary>UI-12 slice 16: a panel the code shows, made visible and drawn alone.</summary>
		private static Control ShowPart(Control root,string name) {
			Control control=FindByName(root,name);
			control.Visible=true;
			return control;
		}

		///<summary>UI-12 slice 13: the other tab pages of FormRecallList (its program tab control) and of the report designer's FindTab,
		///DialogToolOptions and DialogDatabase (stock tab controls).</summary>
		private static string[] GetSliceThirteenTabState(string window,string state) {
			switch(window+"#"+state) {
				case "OpenPmsDental.FormRecallList#reminders": return new string[] { "tabControl","tabPageReminders" };
				case "OpenPmsDental.FormRecallList#reactivations": return new string[] { "tabControl","tabPageReactivations" };
				case "fyiReporting.RdlDesign.FindTab#replace": return new string[] { "tcFRG","tabReplace" };
				case "fyiReporting.RdlDesign.FindTab#goto": return new string[] { "tcFRG","tabGoTo" };
				case "fyiReporting.RdlDesign.DialogToolOptions#toolbar": return new string[] { "tabControl1","tpToolbar" };
				case "fyiReporting.RdlDesign.DialogToolOptions#desktop": return new string[] { "tabControl1","tpDesktop" };
				case "fyiReporting.RdlDesign.DialogDatabase#connection": return new string[] { "tcDialog","DBConnection" };
				case "fyiReporting.RdlDesign.DialogDatabase#parameters": return new string[] { "tcDialog","ReportParameters" };
				case "fyiReporting.RdlDesign.DialogDatabase#grouping": return new string[] { "tcDialog","TabularGroup" };
			}
			return null;
		}

		///<summary>UI-12 slice 13: a stock tab control's page shown without the window's SelectedIndexChanged handler (DialogDatabase's builds the
		///report from its data source): the handlers are taken off the control, then the page is selected.</summary>
		private static void SelectStockTabPage(Control root,string nameTabControl,string namePage) {
			System.Windows.Forms.TabControl tabControl=(System.Windows.Forms.TabControl)FindByName(root,nameTabControl);
			System.Reflection.BindingFlags bindingFlags=System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic;
			object key=typeof(System.Windows.Forms.TabControl).GetField("s_selectedIndexChangedEvent",bindingFlags)?.GetValue(null)
				??typeof(System.Windows.Forms.TabControl).GetField("EVENT_SELECTEDINDEXCHANGED",bindingFlags)?.GetValue(null);
			if(key!=null) {
				System.ComponentModel.EventHandlerList eventHandlerList=(System.ComponentModel.EventHandlerList)typeof(System.ComponentModel.Component)
					.GetProperty("Events",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(tabControl);
				eventHandlerList.RemoveHandler(key,eventHandlerList[key]);
			}
			else {
				//.NET's TabControl keeps the handler in a field of its own (_onSelectedIndexChanged).
				System.Reflection.FieldInfo fieldInfo=typeof(System.Windows.Forms.TabControl).GetFields(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)
					.FirstOrDefault(x => x.FieldType==typeof(EventHandler) && x.Name.IndexOf("SelectedIndexChanged",StringComparison.OrdinalIgnoreCase)>=0);
				if(fieldInfo==null) {
					throw new InvalidOperationException("TabControl's SelectedIndexChanged handler not found.");
				}
				fieldInfo.SetValue(tabControl,null);
			}
			tabControl.SelectedTab=tabControl.TabPages.Cast<System.Windows.Forms.TabPage>().First(x => x.Name==namePage);
		}

		///<summary>Shows the tab control's page as a click on its tab does, without SelectedIndexChanged (the windows' handlers read the database,
		///which the harness has not got): the selected index, then the tab control's layout of its pages.</summary>
		private static void SelectTabPage(Control root,string nameTabControl,string namePage) {
			OpenPmsDental.UI.TabControl tabControl=(OpenPmsDental.UI.TabControl)FindByName(root,nameTabControl);
			OpenPmsDental.UI.TabPage tabPage=(OpenPmsDental.UI.TabPage)FindByName(tabControl,namePage);
			int index=tabControl.TabPages.IndexOf(tabPage);
			if(index<0) {
				throw new ArgumentException(namePage+" is not a page of "+nameTabControl+".");
			}
			typeof(OpenPmsDental.UI.TabControl).GetField("_selectedIndex",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)
				.SetValue(tabControl,index);
			tabControl.LayoutTabs();
			tabControl.Invalidate();
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

		///<summary>UI-PLAIN fix round: the dashboard in its setup mode (the state "setup"), as IsEditMode makes it for dashboard tabs from the
		///database: the two synthetic tabs with the delete X (ImageIndex 1, the second under the mouse: 2) and the "Add Tab" tab with the add icon
		///(ImageIndex 0) at the end. IsEditMode gives the tab control its ImageList; it adds the "Add Tab" tab and the X only to tabs that hold a
		///dashboard panel, so the builder adds them to the synthetic ones as it would.</summary>
		public static void SetDashboardSetupMode(Control root) {
			if(!(FindByName(root,"tabControl") is System.Windows.Forms.TabControl tabControl)) {
				throw new InvalidOperationException("DashboardTabCtrl has no tabControl.");
			}
			tabControl.TabPages.Add(new System.Windows.Forms.TabPage("Production"));
			tabControl.TabPages.Add(new System.Windows.Forms.TabPage("Appointments"));
			root.GetType().GetProperty("IsEditMode").SetValue(root,true);
			tabControl.TabPages.Add(new System.Windows.Forms.TabPage { ImageIndex=0,BackColor=SystemColors.Control,Text="Add Tab" });
			tabControl.TabPages[0].ImageIndex=1;
			tabControl.TabPages[1].ImageIndex=2;
			tabControl.SelectedIndex=0;
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
