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
				CodeBase.UiLooks.IsOffForGallery=isLooksOff;
				WindowBuilder.IsBuildingByConstructor=true;//UI-PLAIN fix round: OpenPmsDentalDbSettings' windows with their controls
				DesignSheetWindows.IsPrintingWithoutChildren=true;
				DesignSheetWindows.PaintDpiScale=scale;//the insurance summaries' text at the scale's dpi (DesignSheetWindows.PaintDpiScale)
				List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
				foreach(string windowAndState in arrayWindows) {
					//UI-12 slice 2 fix round: "<window>#<state>" draws the window in one of the states of FillSampleState, named <window>-<state>.
					string window=windowAndState;
					string state="";
					int indexHash=windowAndState.IndexOf('#');
					if(indexHash>=0) {
						window=windowAndState.Substring(0,indexHash);
						state=windowAndState.Substring(indexHash+1);
					}
					ScanType scanType=listScanTypes.FirstOrDefault(x => x.WindowKey==window);
					if(scanType==null) {
						listFailed.Add(window+": not a scanned type");
						continue;
					}
					SetGalleryFont(font);
					try {
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
							Control controlPicture=FillSampleState(root,window,state);
							if(scale!=1f) {
								GridStyleGallery.ScaleFontsAsWindowsDoes(root,scale);
							}
							HideDatePickerCalendars(root);
							FillSampleContent(root,window,state);
							List<string[]> listOwnerHere=controlPicture==root ? listOwner.Where(x => x[1]==window).ToList() : new List<string[]>();
							using(Bitmap bitmap=DesignSheetWindows.Capture(controlPicture,Rectangle.Empty)) {
								string name=window.Substring(window.LastIndexOf('.')+1);
								//UI-12 slice 12: two windows of one class name in the list (xCrudGenerator's and DocumentationBuilder's Form1) are named
								//<namespace>-<class>, so one picture does not overwrite the other.
								if(arrayWindows.Select(x => x.Split('#')[0]).Distinct().Count(x => x.Substring(x.LastIndexOf('.')+1)==window.Substring(window.LastIndexOf('.')+1))>1) {
									name=window.Replace('.','-');
								}
								if(state!="") {
									name+="-"+state;
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
					}
					finally {
						ClearSampleCache(window,state);
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
			if(window=="OpenPmsDental.ControlAccount" && state=="summary") {
				foreach(string name in new string[] { "groupBoxFamilyIns","groupBoxIndIns" }) {
					FindByName(root,name).Visible=true;
				}
				return root;
			}
			if(window=="OpenPmsDental.ControlAccount" && state=="totalowed") {
				return FindByName(root,"panelTotalOwes");
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
			//UI-12 slice 3 fix round (review of 2026-11-23): the slice's other tab pages and views, laid out by its scripts.
			string[] arrayTab=GetSliceThreeTabState(window,state) ?? GetSliceElevenTabState(window,state);
			if(arrayTab!=null) {
				SelectTabPage(root,arrayTab[0],arrayTab[1]);
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
