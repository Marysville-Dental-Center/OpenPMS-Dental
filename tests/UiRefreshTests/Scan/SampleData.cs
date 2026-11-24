// UI-12 slice 13 (docs/port-notes/UI-12.md, "Slice 13"): the minimal in-memory sample objects that seven windows' Resize, SizeChanged and
// paint handlers read, which the off-screen builder (WindowBuilder) does not give a window: it runs InitializeComponent, not the
// constructor or Load, so the arguments and the fields Load fills are null. Harness only: no product code changes for it, no database, no
// network. Each window gets what its handlers read and nothing more; a handler that only refills a grid from the database is replaced by
// the column part of its own code (the grid's columns as the code sizes them, no rows).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Reflection;
using System.Windows.Forms;
using OpenPmsDental;
using OpenPmsDentalBusiness;

namespace UiRefreshTests.Scan {
	public static class SampleData {
		///<summary>The windows this class gives sample objects (the seven UI-12.md slice 11 listed as not buildable).</summary>
		public static readonly string[] ArrayWindows=new string[] { "OpenPmsDental.FormEhrCarePlans","OpenPmsDental.FormEtrans835ClaimEdit",
			"OpenPmsDental.FormEtrans835ProcEdit","OpenPmsDental.FormMountDefEdit","OpenPmsDental.FormRecallList","OpenPmsDental.FormSchedule",
			"OpenPmsDental.FormSheetDefEdit" };

		private const BindingFlags FLAGS=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;

		///<summary>UI-12 slice 16: the shared controls of the slice whose InitializeComponent raises a handler that reads a field its declaration
		///initializes (the builder makes the object without running field initializers or the constructor): ODDatePicker's OnSizeChanged moves
		///its box with its LayoutManager; ToothChartWrapper's OnResize writes the chart data's size (the constructor's 2D chart and data);
		///ComboBoxClinicPicker draws its text from its clinic lists (empty, in its no-database mode, with its label as declared). Runs after the
		///base constructor and before InitializeComponent (WindowBuilder.CreateBare). Does nothing for any other type.</summary>
		public static void BeforeInitializeComponent(object obj) {
			switch(obj.GetType().FullName) {
				case "OpenPmsDental.UI.ODDatePicker":
					SetField(obj,"LayoutManager",new LayoutManagerForms());
					SetField(obj,"_dateTimeDefault",new DateTime(DateTime.Today.Year,1,1));
					SetField(obj,"_hideCalendarOnLeave",true);
					break;
				case "SparksToothChart.ToothChartWrapper":
					SetField(obj,"drawMode",DrawingMode.Simple2D);
					FieldInfo fieldInfoEngine=obj.GetType().GetField("_engine",FLAGS);
					fieldInfoEngine.SetValue(obj,Enum.Parse(fieldInfoEngine.FieldType,"Simple2D"));
					FieldInfo fieldInfoData=obj.GetType().GetField("tcData",FLAGS);//the test project does not reference SparksToothChart
					fieldInfoData.SetValue(obj,Activator.CreateInstance(fieldInfoData.FieldType));
					break;
				case "OpenPmsDental.UI.ComboBoxClinicPicker":
					SetField(obj,"LayoutManager",new LayoutManagerForms());
					SetField(obj,"_listClinics",new List<Clinic>());
					SetField(obj,"_listIndicesSelected",new List<int>());
					SetField(obj,"_hqDescription","Unassigned");
					SetField(obj,"_showLabel",true);
					((OpenPmsDental.UI.ComboBoxClinicPicker)obj).IsTestModeNoDb=true;
					break;
			}
		}

		///<summary>UI-12 slice 16 fix round: runs after the object's InitializeComponent (WindowBuilder.CreateBare). The Imaging module's Resize
		///handler (LayoutControls) reads the image selector and the Pearl program that its constructor and Load set up, which the builder does
		///not run; it is detached, so the module can be laid out at 150% (its designer already holds the places LayoutControls gives its parts,
		///hand/ControlImages.py). Does nothing for any other type.</summary>
		public static void AfterInitializeComponent(object obj) {
			switch(obj.GetType().FullName) {
				case "OpenPmsDental.ControlImages":
					Detach((Control)obj,"Resize","ContrImages_Resize");
					break;
			}
		}

		///<summary>Runs on a LayoutManager window after its InitializeComponent and before InitializeLayoutManager (WindowBuilder.Build): the
		///sample objects of the window's handlers. Does nothing for any other window.</summary>
		public static void BeforeLayoutManager(FormODBase formODBase) {
			switch(formODBase.GetType().FullName) {
				case "OpenPmsDental.FormEhrCarePlans":
					//FormEhrCarePlans_Resize refills the grid from the database (EhrCarePlans.Refresh of the patient Load is given): the harness
					//runs the column part of FillCarePlans instead (Date 66 px, Goal and Instructions share the rest).
					Detach(formODBase,"Resize","FormEhrCarePlans_Resize");
					formODBase.Resize+=(sender,e) => FillCarePlanColumns(formODBase);
					break;
				case "OpenPmsDental.FormEtrans835ClaimEdit":
					//The Resize handler refills the four grids from the ERA claim the constructor is given: a claim with nothing reported.
					Hx835_Claim hx835_Claim=new Hx835_Claim();
					hx835_Claim.ListClaimAdjustments=new List<Hx835_Adj>();
					hx835_Claim.ListAdjudicationInfo=new List<Hx835_Info>();
					hx835_Claim.ListSupplementalInfo=new List<Hx835_Info>();
					hx835_Claim.ListProcs=new List<Hx835_Proc>();
					SetField(formODBase,"_hx835_Claim",hx835_Claim);
					break;
				case "OpenPmsDental.FormEtrans835ProcEdit":
					//The Resize handler refills the three grids from the ERA procedure the constructor is given: one with nothing reported.
					Hx835_Proc hx835_Proc=new Hx835_Proc();
					hx835_Proc.ListProcAdjustments=new List<Hx835_Adj>();
					hx835_Proc.ListRemarks=new List<Hx835_Remark>();
					hx835_Proc.ListSupplementalInfo=new List<Hx835_Info>();
					SetField(formODBase,"_hx835_Proc",hx835_Proc);
					break;
				case "OpenPmsDental.FormMountDefEdit":
					//SizeChanged (CalcRatio) and the drawing's Paint read the mount the caller sets and the items Load reads: a four-image mount.
					MountDef mountDef=new MountDef();
					mountDef.Description="Sample Mount";
					mountDef.Width=1200;
					mountDef.Height=600;
					mountDef.ColorBack=Color.Black;
					mountDef.ColorFore=Color.White;
					mountDef.ColorTextBack=Color.Black;
					SetField(formODBase,"MountDefCur",mountDef);
					List<MountItemDef> listMountItemDefs=new List<MountItemDef>();
					for(int i=0;i<4;i++) {
						MountItemDef mountItemDef=new MountItemDef();
						mountItemDef.Xpos=i*300;
						mountItemDef.Ypos=150;
						mountItemDef.Width=300;
						mountItemDef.Height=300;
						mountItemDef.ItemOrder=i+1;
						mountItemDef.TextShowing="";
						mountItemDef.ToothNumbers="";
						listMountItemDefs.Add(mountItemDef);
					}
					SetField(formODBase,"_listMountItemDefs",listMountItemDefs);
					SetField(formODBase,"_selectedIndex",-1);//its declaration's value (the builder runs no field initializers)
					break;
				case "OpenPmsDental.FormRecallList":
					//The clinic pickers read the clinics preference when their handles are made (OnLoad): the pickers' own no-database mode,
					//drawn as with clinics on.
					SetClinicPickersNoDb(formODBase);
					break;
				case "OpenPmsDental.FormSchedule":
					//Resize refills the grid (FillGrid(false)): from the period table Load reads, else from the database with the clinic
					//picker's clinic. An empty period table (the seven day columns) and the work week, its declaration's value.
					SetClinicPickersNoDb(formODBase);
					DataTable table=new DataTable();
					for(int i=0;i<7;i++) {
						table.Columns.Add("Col"+i);
					}
					SetField(formODBase,"_tableScheds",table);
					FieldInfo fieldInfoFilter=formODBase.GetType().GetField("_scheduleWeekendFilter",FLAGS);
					fieldInfoFilter.SetValue(formODBase,Enum.Parse(fieldInfoFilter.FieldType,"WorkWeek"));
					break;
				case "OpenPmsDental.FormSheetDefEdit":
					//Resize (SetPanelMainSize) and the sheet's Paint read the sheet the caller sets and the margins of the field's declaration:
					//an empty letter-size sheet of one page.
					SheetDef sheetDef=new SheetDef();
					sheetDef.Description="Sample Sheet";
					sheetDef.SheetType=SheetTypeEnum.PatientLetter;
					sheetDef.Width=850;
					sheetDef.Height=1100;
					sheetDef.PageCount=1;
					sheetDef.SheetFieldDefs=new List<SheetFieldDef>();
					SetField(formODBase,"SheetDef_",sheetDef);
					SetField(formODBase,"_margins",new Margins(0,0,40,60));
					break;
				case "OpenPmsDental.FormTasks":
					//UI-12 slice 14: Load collapses the empty lower pane (Panel2Collapsed), so the task list fills the window; its control's tree
					//history is the one InitializeOnStartup starts (the main trunk: empty), which its Resize lays the tree and grid out by.
					((OpenPmsDental.UI.SplitContainer)formODBase.GetType().GetField("splitContainer",FLAGS).GetValue(formODBase)).Panel2Collapsed=true;
					BeforeHosting((UserControl)formODBase.GetType().GetField("userControlTasks1",FLAGS).GetValue(formODBase));
					break;
			}
		}

		///<summary>UI-12 slice 14: runs on a UserControl the builder hosts (WindowBuilder.HostInFormODBase) before LayoutManager.Add, and on one a
		///window's designer holds: UserControlTasks gets the tree history InitializeOnStartup starts with (the main trunk: an empty list), which
		///its Resize lays the tree and grid out by. UserControlSecurityUserGroup's tab control lays its pages out while LayoutManager.Add walks the
		///control (SetNoAnchorDock's PerformLayout), before the pages' panels are added (FormSecurity adds it in its designer, where the window's
		///layout is suspended): its tab controls are suspended until the Add is done (Dispose resumes them). Null for every other control.</summary>
		public static IDisposable BeforeHosting(UserControl userControl) {
			switch(userControl.GetType().FullName) {
				case "OpenPmsDental.UserControlTasks":
					SetField(userControl,"_listTaskListTreeHistory",new List<TaskList>());
					return null;
				case "OpenPmsDental.ControlFamily":
					//RefreshModuleScreen with no patient (or none with a super family or clones) hides the super family and clones pane; the
					//insurance grid stands in its place.
					((Control)userControl.GetType().GetField("splitContainerSuperClones",FLAGS).GetValue(userControl)).Visible=false;
					return null;
				case "OpenPmsDental.UserControlSecurityUserGroup":
					List<Control> listSuspended=new List<Control>();
					SuspendTabControls(userControl,listSuspended);
					return new ActionOnDispose(() => listSuspended.ForEach(x => x.ResumeLayout(false)));
			}
			return null;
		}

		///<summary>UI-12 slice 14: runs on a UserControl the builder has hosted and laid out (WindowBuilder.HostInFormODBase). UserControlTasks lays
		///its tabs, calendar, tree and grid out in its Resize handler, which a host that sizes it raises (FormTasks docks it, the main window moves
		///it); hosted at its own size it gets none, so the handler runs here as a host's sizing runs it.</summary>
		public static void AfterHosting(UserControl userControl) {
			if(userControl.GetType().FullName=="OpenPmsDental.UserControlTasks") {
				userControl.GetType().GetMethod("UserControlTasks_Resize",FLAGS).Invoke(userControl,new object[] { userControl,EventArgs.Empty });
			}
		}

		private static void SuspendTabControls(Control control,List<Control> listSuspended) {
			foreach(Control child in control.Controls) {
				if(child is OpenPmsDental.UI.TabControl) {
					child.SuspendLayout();
					listSuspended.Add(child);
				}
				SuspendTabControls(child,listSuspended);
			}
		}

		private class ActionOnDispose:IDisposable {
			private readonly Action _action;

			public ActionOnDispose(Action action) {
				_action=action;
			}

			public void Dispose() {
				_action();
			}
		}

		///<summary>UI-12 slice 15: runs on a plain user control after its InitializeComponent and before the builder adds it to its test form
		///(WindowBuilder.Build, UserControlPlain): the sample objects of the handlers that adding it runs. The report designer's DataSetRowsCtl
		///(PropertyDialog's Data page) compares its data table with the data set's fields in VisibleChanged; the harness gives it a data set with
		///no fields and an empty table, as its constructor makes them for a new data set (CreateDataTable). Does nothing for any other control.</summary>
		public static void BeforePlainHost(UserControl userControl) {
			if(userControl.GetType().FullName!="fyiReporting.RdlDesign.DataSetRowsCtl") {
				return;
			}
			Type typeValues=userControl.GetType().Assembly.GetType("fyiReporting.RdlDesign.DataSetValues",true);
			object dataSetValues=Activator.CreateInstance(typeValues,FLAGS,null,new object[] { "DataSet1" },null);
			DataTable tableFields=new DataTable();
			tableFields.Columns.Add(new DataColumn("Name",typeof(string)));
			tableFields.Columns.Add(new DataColumn("QueryName",typeof(string)));
			tableFields.Columns.Add(new DataColumn("Value",typeof(string)));
			tableFields.Columns.Add(new DataColumn("TypeName",typeof(string)));
			typeValues.GetProperty("Fields",FLAGS).SetValue(dataSetValues,tableFields);
			SetField(userControl,"_dsv",dataSetValues);
			SetField(userControl,"_DataTable",new DataTable());
		}

		///<summary>The preferences a window's InitializeComponent reads, in the preference cache while it runs (Dispose takes them out again and
		///leaves the cache as it was). FormRecallList: its clinic pickers' IncludeAll setters read EasyNoClinics (PrefC.HasClinicsEnabled) before
		///the builder can set their no-database mode; "1" (clinics off) reads no clinic cache. Null for every other window.</summary>
		public static IDisposable PrefsForInitializeComponent(Type type) {
			if(type.FullName!="OpenPmsDental.FormRecallList") {
				return null;
			}
			return new PrefScope(PrefName.EasyNoClinics.ToString(),"1");
		}

		///<summary>One preference in the cache for a while: the cache filled with it alone when empty (cleared again), else the key added (removed
		///again) when the cache has not got it.</summary>
		private class PrefScope:IDisposable {
			private readonly object _prefCache;
			private readonly string _key;
			private readonly bool _wasEmpty;
			private readonly bool _wasAdded;

			public PrefScope(string key,string value) {
				_key=key;
				_prefCache=typeof(Prefs).GetProperty("_prefCache",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
				Pref pref=new Pref { PrefNum=1,PrefName=key,ValueString=value,Comments="" };
				_wasEmpty=(bool)_prefCache.GetType().GetMethod("DictIsNull").Invoke(_prefCache,null);
				if(_wasEmpty) {
					Prefs.FillCacheFromTable(OpenPmsDentalBusiness.Crud.PrefCrud.ListToTable(new List<Pref> { pref }));
					return;
				}
				_wasAdded=(bool)_prefCache.GetType().GetMethod("AddValueForKey").Invoke(_prefCache,new object[] { key,pref,false });
			}

			public void Dispose() {
				if(_wasEmpty) {
					Prefs.ClearCache();
				}
				else if(_wasAdded) {
					_prefCache.GetType().GetMethod("RemoveKey").Invoke(_prefCache,new object[] { _key });
				}
			}
		}

		///<summary>FillCarePlans' columns (FormEhrCarePlans.cs): Date 66 px, Goal and Instructions halves of the rest of the grid's width.</summary>
		private static void FillCarePlanColumns(FormODBase formODBase) {
			OpenPmsDental.UI.GridOD gridOD=(OpenPmsDental.UI.GridOD)formODBase.GetType().GetField("gridCarePlans",FLAGS).GetValue(formODBase);
			gridOD.BeginUpdate();
			gridOD.Columns.Clear();
			int colDatePixCount=66;
			int variablePixCount=gridOD.Width-10-colDatePixCount;
			int colGoalPixCount=variablePixCount/2;
			int colInstructionsPixCount=variablePixCount-colGoalPixCount;
			gridOD.Columns.Add(new OpenPmsDental.UI.GridColumn("Date",colDatePixCount));
			gridOD.Columns.Add(new OpenPmsDental.UI.GridColumn("Goal",colGoalPixCount));
			gridOD.Columns.Add(new OpenPmsDental.UI.GridColumn("Instructions",colInstructionsPixCount));
			gridOD.EndUpdate();
		}

		///<summary>Every clinic picker field of the window (the pickers on tab pages not shown are not in the control tree yet).</summary>
		private static void SetClinicPickersNoDb(Form form) {
			foreach(FieldInfo fieldInfo in form.GetType().GetFields(FLAGS)) {
				if(fieldInfo.GetValue(form) is OpenPmsDental.UI.ComboBoxClinicPicker comboBoxClinicPicker) {
					comboBoxClinicPicker.IsTestModeNoDb=true;
				}
			}
		}

		private static void SetField(object obj,string name,object value) {
			FieldInfo fieldInfo=obj.GetType().GetField(name,FLAGS);
			if(fieldInfo==null) {
				throw new InvalidOperationException(obj.GetType().Name+"."+name+" not found: SampleData is out of date with the window.");
			}
			fieldInfo.SetValue(obj,value);
		}

		///<summary>Removes the window's (or control's) own handler nameMethod from its event nameEvent (an EventHandler).</summary>
		private static void Detach(Control control,string nameEvent,string nameMethod) {
			EventInfo eventInfo=typeof(Control).GetEvent(nameEvent);
			Delegate handler=Delegate.CreateDelegate(eventInfo.EventHandlerType,control,nameMethod);
			eventInfo.RemoveEventHandler(control,handler);
		}
	}
}
