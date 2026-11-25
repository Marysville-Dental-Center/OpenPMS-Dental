// The screenshots of the README and the project site (package DOCS-SITE, docs/differences.md, docs/screenshots/README.md): the program's own
// module controls and two dialogs, drawn off screen as the galleries draw them (DesignSheetWindows.Capture), with a fictional patient selected
// and the module's data loaded from a scratch database. Opt-in: OPMS_UI_DOC_SCREENSHOTS names the folder for the pictures; skipped otherwise,
// so the default runs keep the no-database rule of this project (Scan\NoDatabaseGuard). The database is local only (127.0.0.1:3306, root,
// blank password), named docs_<run id>, built from db/init/new-database.sql and the fork migrations, filled with the fictional practice of
// DocSampleData (the names of tools/DevDbTool's sample data) and dropped at the end; OPMS_UI_DOC_SCREENSHOTS_KEEP_DB=1 keeps it. When the
// test ends, the process's connection is the no-database guard's again. Nothing is shown; message boxes throw (ODBuild.IsUnitTest).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using OpenPmsDental;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.ForkMigrations;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class DocScreenshots {
		public const string VARIABLE_FOLDER="OPMS_UI_DOC_SCREENSHOTS";
		public const string VARIABLE_KEEP_DB="OPMS_UI_DOC_SCREENSHOTS_KEEP_DB";
		///<summary>The size of the main window's client area in the pictures (100%).</summary>
		public static readonly Size SizeMain=new Size(1440,860);

		[TestMethod]
		public void DocScreenshots_ModulesAndDialogs_WithFictionalData() {
			string folder=Environment.GetEnvironmentVariable(VARIABLE_FOLDER);
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: "+VARIABLE_FOLDER+" names the folder for the README and site screenshots.");
			}
			string skipReason=DocScreenshotDb.GetSkipReason();
			if(skipReason!=null) {
				Assert.Inconclusive(skipReason);
			}
			Directory.CreateDirectory(folder);
			List<string> listFailed=new List<string>();
			List<string> listSaved=new List<string>();
			string dbName=DocScreenshotDb.NewName();
			bool isUnitTestBefore=ODBuild.IsUnitTest;
			//The program's en-US formats, not this computer's user settings, also on the threads the modules load their data on: Windows' en-US
			//with the four-digit short date the program sets at its start (DateTimeOD.NormalizeApplicationShortDateFormat).
			CultureInfo cultureInfo=(CultureInfo)new CultureInfo("en-US",false).Clone();
			cultureInfo.DateTimeFormat.ShortDatePattern="MM/dd/yyyy";
			CultureInfo cultureInfoDefaultBefore=CultureInfo.DefaultThreadCurrentCulture;
			CultureInfo cultureInfoDefaultUiBefore=CultureInfo.DefaultThreadCurrentUICulture;
			CultureInfo.DefaultThreadCurrentCulture=cultureInfo;
			CultureInfo.DefaultThreadCurrentUICulture=cultureInfo;
			try {
				StaRunner.Run(() => {
					ODBuild.IsUnitTest=true;//a message box throws instead of waiting for a person
					Thread.CurrentThread.CurrentCulture=cultureInfo;
					Thread.CurrentThread.CurrentUICulture=cultureInfo;
					DocScreenshotDb.Create(dbName);
					DocSampleData docSampleData=DocSampleData.Insert();
					FormOpenDental.PatNumCur=docSampleData.PatNumSelected;
					DesignSheetWindows.IsPrintingWithoutChildren=true;
					try {
						Save(folder,"appointments",listSaved,listFailed,() => DrawModule(EnumModuleType.Appointments,new ControlAppt(),control => {
							ControlAppt controlAppt=(ControlAppt)control;
							controlAppt.InitializeOnStartup();
							controlAppt.ModuleSelected(docSampleData.PatNumSelected);
							controlAppt.ModuleSelected(docSampleData.DateAppointments);
						}));
						Save(folder,"family",listSaved,listFailed,() => DrawModule(EnumModuleType.Family,new ControlFamily(),control => {
							((ControlFamily)control).InitializeOnStartup();
							((ControlFamily)control).ModuleSelected(docSampleData.PatNumSelected);
						}));
						Save(folder,"account",listSaved,listFailed,() => DrawModule(EnumModuleType.Account,new ControlAccount(),control => {
							((ControlAccount)control).InitializeOnStartup();
							((ControlAccount)control).ModuleSelected(docSampleData.PatNumSelected);
						}));
						Save(folder,"treatment-plan",listSaved,listFailed,() => DrawModule(EnumModuleType.TreatPlan,new ControlTreat(),control => {
							((ControlTreat)control).InitializeOnStartup();
							((ControlTreat)control).ModuleSelected(docSampleData.PatNumSelected);
						}));
						Save(folder,"manage",listSaved,listFailed,() => DrawModule(EnumModuleType.Manage,new ControlManage(),control => {
							((ControlManage)control).InitializeOnStartup();
							((ControlManage)control).ModuleSelected(docSampleData.PatNumSelected);
						}));
						Save(folder,"dialog-patient-edit",listSaved,listFailed,() => {
							FormPatientEdit formPatientEdit=new FormPatientEdit();
							formPatientEdit.Patient=Patients.GetPat(docSampleData.PatNumSelected);
							formPatientEdit.Family=Patients.GetFamily(docSampleData.PatNumSelected);
							formPatientEdit.IsNew=false;
							return DrawDialog(formPatientEdit);
						});
						Save(folder,"dialog-appointment-edit",listSaved,listFailed,
							() => DrawDialog(new FormApptEdit(docSampleData.AptNumSelected)));
					}
					finally {
						DesignSheetWindows.IsPrintingWithoutChildren=false;
					}
				},TimeSpan.FromMinutes(15));
			}
			finally {
				ODBuild.IsUnitTest=isUnitTestBefore;
				CultureInfo.DefaultThreadCurrentCulture=cultureInfoDefaultBefore;
				CultureInfo.DefaultThreadCurrentUICulture=cultureInfoDefaultUiBefore;
				FormOpenDental.PatNumCur=0;
				if(Environment.GetEnvironmentVariable(VARIABLE_KEEP_DB)!="1") {
					try {
						DocScreenshotDb.Drop(dbName);
					}
					catch(Exception) {
						//DocScreenshotDb.DropStale drops it in a later run.
					}
				}
				DocScreenshotDb.RestoreNoDatabaseGuard();
			}
			File.WriteAllLines(Path.Combine(folder,"doc-screenshots-failed.txt"),listFailed);
			Assert.IsTrue(listSaved.Count>0,"No picture made: "+string.Join("; ",listFailed));
		}

		private static void Save(string folder,string name,List<string> listSaved,List<string> listFailed,Func<Bitmap> funcDraw) {
			try {
				using(Bitmap bitmap=funcDraw()) {
					string path=Path.Combine(folder,name+".png");
					bitmap.Save(path,ImageFormat.Png);
					listSaved.Add(path);
				}
			}
			catch(Exception exception) {
				listFailed.Add(name+": "+exception.GetBaseException().GetType().Name+": "+exception.GetBaseException().Message);
				File.AppendAllText(Path.Combine(folder,"doc-screenshots-failed-detail.txt"),name+Environment.NewLine+exception+Environment.NewLine+Environment.NewLine);
			}
		}

		///<summary>The main window's client area as FormOpenDental lays it out: the module bar on the left, the main toolbar on top and the module
		///below it. The menu bar and the title bar are the window's own and are not drawn.</summary>
		private static Bitmap DrawModule(EnumModuleType moduleType,UserControl controlModule,Action<UserControl> actionSelect) {
			using(HostFormOD hostFormOD=new HostFormOD(SizeMain)) {
				LayoutManagerForms layoutManager=hostFormOD.GetLayoutManager();
				if(hostFormOD.PanelClient==null || hostFormOD.PanelBorders==null) {
					layoutManager.SetPanelClient(hostFormOD);
				}
				Size sizeClient=hostFormOD.PanelClient.ClientSize;
				ModuleBar moduleBar=new ModuleBar();
				moduleBar.Bounds=new Rectangle(0,0,51,sizeClient.Height);
				layoutManager.Add(moduleBar,hostFormOD.PanelClient);
				ToolBarOD toolBarMain=new ToolBarOD();
				toolBarMain.Name="ToolBarMain";
				toolBarMain.Bounds=new Rectangle(51,0,sizeClient.Width-51,25);
				toolBarMain.SetImageListIcons(FormOpenDental.ArrayIconsImageList);
				layoutManager.Add(toolBarMain,hostFormOD.PanelClient);
				controlModule.Bounds=new Rectangle(51,25,sizeClient.Width-51,sizeClient.Height-25);
				layoutManager.Add(controlModule,hostFormOD.PanelClient);
				layoutManager.LayoutControlBoundsAndFonts(hostFormOD.PanelClient);
				DesignSheetWindows.CreateHandles(hostFormOD);
				moduleBar.RefreshButtons();
				moduleBar.SelectedModule=moduleType;
				FillMainToolBar(toolBarMain);
				actionSelect(controlModule);
				Settle();
				ThrowIfCallbackFailed();
				return DesignSheetWindows.Capture(hostFormOD.PanelClient,Rectangle.Empty);
			}
		}

		///<summary>The buttons FormOpenDental.LayoutToolBar puts on the main toolbar without a texting integration or a program link.</summary>
		private static void FillMainToolBar(ToolBarOD toolBarMain) {
			toolBarMain.Buttons.Clear();
			toolBarMain.Buttons.Add(NewDropDown("Select Patient",EnumIcons.PatSelect,"Patient"));
			toolBarMain.Buttons.Add(new ODToolBarButton("Commlog",EnumIcons.CommLog,"New Commlog Entry","Commlog"));
			toolBarMain.Buttons.Add(NewDropDown("E-mail",EnumIcons.Email,"Email"));
			ODToolBarButton toolBarButtonLetter=new ODToolBarButton("Letter",-1,"Quick Letter","Letter");
			toolBarButtonLetter.Style=ODToolBarButtonStyle.DropDownButton;
			toolBarButtonLetter.ContextMenuStripDropDown=new ContextMenuStrip();
			toolBarMain.Buttons.Add(toolBarButtonLetter);
			toolBarMain.Buttons.Add(new ODToolBarButton("Forms",-1,"","Form"));
			toolBarMain.Buttons.Add(NewDropDown("Tasks",EnumIcons.TaskList,"Tasklist"));
			toolBarMain.Buttons.Add(NewDropDown("Label",EnumIcons.Label,"Label"));
			toolBarMain.Buttons.Add(new ODToolBarButton("Popups",-1,"Edit popups for this patient","Popups"));
			toolBarMain.Invalidate();
		}

		private static ODToolBarButton NewDropDown(string text,EnumIcons icon,string tag) {
			ODToolBarButton toolBarButton=new ODToolBarButton(text,icon,"",tag);
			toolBarButton.Style=ODToolBarButtonStyle.DropDownButton;
			toolBarButton.ContextMenuStripDropDown=new ContextMenuStrip();
			return toolBarButton;
		}

		///<summary>A dialog with its Load run (FormODBase.OnLoad lays it out and raises Load, which fills it from the database), never shown: its
		///client area, as the galleries draw a window.</summary>
		private static Bitmap DrawDialog(FormODBase formODBase) {
			using(formODBase) {
				formODBase.StartPosition=FormStartPosition.Manual;
				formODBase.Location=new Point(0,0);
				DesignSheetWindows.CreateHandles(formODBase);
				MethodInfo methodInfoOnLoad=typeof(Form).GetMethod("OnLoad",BindingFlags.Instance|BindingFlags.NonPublic);
				methodInfoOnLoad.Invoke(formODBase,new object[] { EventArgs.Empty });
				Settle();
				ThrowIfCallbackFailed();
				Control root=formODBase.PanelClient??(Control)formODBase;
				return DesignSheetWindows.Capture(root,Rectangle.Empty);
			}
		}

		///<summary>Runs the messages the controls posted to themselves (BeginInvoke, invalidations) before the picture.</summary>
		private static void Settle() {
			for(int i=0;i<5;i++) {
				Application.DoEvents();
			}
		}

		private static void ThrowIfCallbackFailed() {
			Exception exception=StaRunner.TakeExceptionInCallback();
			if(exception!=null) {
				throw new ApplicationException("A window procedure failed while the picture was made.",exception);
			}
		}
	}

	///<summary>The fictional practice of the screenshots: the two providers of the new database renamed as tools/DevDbTool's sample providers,
	///nine patients in six families (DevDbTool's sample names and a few more), a day of appointments with procedures, two years of a family's
	///completed work and payments, a treatment plan and four staff members. Procedure codes get plain descriptions of this test's own (the new
	///database names them by their code only). Phone numbers are in the 555-01xx range kept for fiction; e-mail addresses use example.com.</summary>
	public class DocSampleData {
		public long PatNumSelected;
		public long AptNumSelected;
		public DateTime DateAppointments;
		private Provider _providerDentist;
		private Provider _providerHygienist;

		///<summary>Fee, abbreviation and plain description of each code the sample uses.</summary>
		private static readonly Dictionary<string,Tuple<double,string,string>> _dictCodes=new Dictionary<string,Tuple<double,string,string>> {
			{ "D0120",Tuple.Create(62.0,"PerEx","Exam, periodic") },
			{ "D0150",Tuple.Create(98.0,"CompEx","Exam, new patient") },
			{ "D0210",Tuple.Create(145.0,"FMX","X-rays, full mouth") },
			{ "D0220",Tuple.Create(32.0,"PA","X-ray, single tooth") },
			{ "D0274",Tuple.Create(72.0,"4BW","X-rays, four bitewings") },
			{ "D1110",Tuple.Create(112.0,"Pro","Cleaning, adult") },
			{ "D1120",Tuple.Create(84.0,"ChildPro","Cleaning, child") },
			{ "D1206",Tuple.Create(41.0,"Fluor","Fluoride varnish") },
			{ "D2391",Tuple.Create(168.0,"Comp1","Filling, white, one surface") },
			{ "D2392",Tuple.Create(214.0,"Comp2","Filling, white, two surfaces") },
			{ "D2740",Tuple.Create(1260.0,"Crown","Crown, ceramic") },
			{ "D2950",Tuple.Create(305.0,"BU","Crown buildup") },
			{ "D3330",Tuple.Create(1120.0,"RCT","Root canal, molar") },
			{ "D4341",Tuple.Create(265.0,"SRP","Deep cleaning, per quadrant") },
		};

		public static DocSampleData Insert() {
			DocSampleData docSampleData=new DocSampleData();
			docSampleData.InsertAll();
			return docSampleData;
		}

		private void InsertAll() {
			Security.CurUser=Userods.GetUser(1)??new Userod { UserNum=1,UserName="Admin" };
			DateAppointments=new DateTime(2026,12,1);//a Tuesday
			foreach(KeyValuePair<string,Tuple<double,string,string>> pair in _dictCodes) {
				ProcedureCode procedureCode=ProcedureCodes.GetProcCode(pair.Key);
				procedureCode.AbbrDesc=pair.Value.Item2;
				procedureCode.Descript=pair.Value.Item3;
				ProcedureCodes.Update(procedureCode);
			}
			Cache.Refresh(InvalidType.ProcCodes);
			_providerDentist=Providers.GetProv(1);
			_providerDentist.Abbr="DDEV";
			_providerDentist.FName="Dana";
			_providerDentist.LName="Devlin";
			_providerDentist.Suffix="DDS";
			_providerDentist.ProvColor=Color.FromArgb(178,214,255);
			Providers.Update(_providerDentist);
			_providerHygienist=Providers.GetProv(2);
			_providerHygienist.Abbr="RHYG";
			_providerHygienist.FName="Riley";
			_providerHygienist.LName="Hart";
			_providerHygienist.Suffix="RDH";
			_providerHygienist.ProvColor=Color.FromArgb(196,240,196);
			Providers.Update(_providerHygienist);
			Cache.Refresh(InvalidType.Providers);
			List<Operatory> listOperatories=Operatories.GetDeepCopy(isShort:true);
			Operatory operatoryOne=listOperatories[0];
			Operatory operatoryTwo=listOperatories[1];
			Operatory operatoryThree=listOperatories[2];
			SetOperatory(operatoryOne,"Operatory 1",isHygiene:false);
			SetOperatory(operatoryTwo,"Hygiene 1",isHygiene:true);
			SetOperatory(operatoryThree,"Hygiene 2",isHygiene:true);
			//The new database's other operatories are hidden, so the day shows the three in use.
			for(int i=3;i<listOperatories.Count;i++) {
				listOperatories[i].IsHidden=true;
				Operatories.Update(listOperatories[i]);
			}
			Cache.Refresh(InvalidType.Operatories);
			//Families: the first patient of each family is the guarantor.
			Patient patientAlex=InsertPatient("Sampleton","Alex","",new DateTime(1984,3,12),PatientGender.Female,PatientPosition.Married,
				"101 Example Way","555-0101","alex.sampleton@example.com",null);
			Patient patientJamie=InsertPatient("Sampleton","Jamie","",new DateTime(1985,7,4),PatientGender.Male,PatientPosition.Married,
				"101 Example Way","555-0102","jamie.sampleton@example.com",patientAlex);
			Patient patientSam=InsertPatient("Sampleton","Sam","",new DateTime(2015,9,20),PatientGender.Male,PatientPosition.Child,
				"101 Example Way","555-0101","",patientAlex);
			Patient patientMorgan=InsertPatient("Placeholder","Morgan","",new DateTime(1972,11,2),PatientGender.Unknown,PatientPosition.Single,
				"22 Fixture Lane","555-0110","morgan.placeholder@example.com",null);
			Patient patientCasey=InsertPatient("Fixture","Casey","",new DateTime(1999,1,15),PatientGender.Female,PatientPosition.Single,
				"7 Mock Street","555-0120","casey.fixture@example.com",null);
			Patient patientRobin=InsertPatient("Mockford","Robin","",new DateTime(1958,5,30),PatientGender.Male,PatientPosition.Widowed,
				"300 Stub Road","555-0130","",null);
			Patient patientTaylor=InsertPatient("Example","Taylor","",new DateTime(1991,8,8),PatientGender.Female,PatientPosition.Single,
				"45 Sample Court","555-0140","taylor.example@example.com",null);
			Patient patientJordan=InsertPatient("Testwell","Jordan","",new DateTime(1979,2,27),PatientGender.Male,PatientPosition.Married,
				"9 Demo Avenue","555-0150","jordan.testwell@example.com",null);
			Patient patientAvery=InsertPatient("Demo","Avery","",new DateTime(2002,12,5),PatientGender.Female,PatientPosition.Single,
				"18 Trial Road","555-0160","avery.demo@example.com",null);
			PatNumSelected=patientAlex.PatNum;
			//The day's provider schedules, 8:00 to 17:00.
			List<Schedule> listSchedules=new List<Schedule> {
				NewProviderSchedule(_providerDentist.ProvNum,operatoryOne.OperatoryNum),
				NewProviderSchedule(_providerHygienist.ProvNum,operatoryTwo.OperatoryNum),
				NewProviderSchedule(_providerHygienist.ProvNum,operatoryThree.OperatoryNum),
			};
			Schedules.Insert(false,true,listSchedules);
			//The day's appointments, each with its procedures.
			InsertAppointment(patientRobin,8,0,operatoryOne,false,ApptStatus.Complete,"//XXXXXXXXXXXXXX//",
				new[] { "D2740#3","D2950#3" });
			AptNumSelected=InsertAppointment(patientAlex,10,0,operatoryOne,false,ApptStatus.Scheduled,"//XXXXXXXX//",new[] { "D2392#19MO" });
			InsertAppointment(patientMorgan,13,0,operatoryOne,false,ApptStatus.Scheduled,"//XXXXXXXXXXXXXX//",new[] { "D3330#30","D0220#30" });
			InsertAppointment(patientAvery,15,0,operatoryOne,false,ApptStatus.Scheduled,"//XXXXXXXX//",new[] { "D0150","D0210" });
			InsertAppointment(patientJamie,8,0,operatoryTwo,true,ApptStatus.Complete,"//XXXXXXXX//",new[] { "D0120","D1110","D0274" });
			InsertAppointment(patientSam,9,0,operatoryTwo,true,ApptStatus.Scheduled,"/XXXXXX/",new[] { "D0120","D1120","D1206" });
			InsertAppointment(patientCasey,10,30,operatoryTwo,true,ApptStatus.Scheduled,"//XXXXXXXX//",new[] { "D0120","D1110" });
			InsertAppointment(patientTaylor,9,0,operatoryThree,true,ApptStatus.Scheduled,"//XXXXXXXX//",new[] { "D4341","D4341" });
			InsertAppointment(patientJordan,14,0,operatoryThree,true,ApptStatus.Scheduled,"//XXXXXXXX//",new[] { "D0120","D1110","D0274" });
			//The Sampleton family's history: completed work, payments.
			DateTime dateFirst=new DateTime(2025,6,10);
			InsertProcedure(patientAlex,"D0150","",dateFirst,ProcStat.C,0,_providerDentist);
			InsertProcedure(patientAlex,"D0210","",dateFirst,ProcStat.C,0,_providerDentist);
			InsertProcedure(patientAlex,"D1110","",dateFirst,ProcStat.C,0,_providerHygienist);
			InsertProcedure(patientJamie,"D0150","",dateFirst,ProcStat.C,0,_providerDentist);
			InsertProcedure(patientJamie,"D1110","",dateFirst,ProcStat.C,0,_providerHygienist);
			InsertPayment(patientAlex,dateFirst,250,"Check 1042");
			DateTime dateSecond=new DateTime(2025,12,9);
			InsertProcedure(patientAlex,"D0120","",dateSecond,ProcStat.C,0,_providerDentist);
			InsertProcedure(patientAlex,"D1110","",dateSecond,ProcStat.C,0,_providerHygienist);
			InsertProcedure(patientSam,"D0120","",dateSecond,ProcStat.C,0,_providerDentist);
			InsertProcedure(patientSam,"D1120","",dateSecond,ProcStat.C,0,_providerHygienist);
			InsertProcedure(patientSam,"D1206","",dateSecond,ProcStat.C,0,_providerHygienist);
			InsertPayment(patientAlex,dateSecond,420,"Card");
			DateTime dateThird=new DateTime(2026,6,16);
			InsertProcedure(patientAlex,"D0120","",dateThird,ProcStat.C,0,_providerDentist);
			InsertProcedure(patientAlex,"D0274","",dateThird,ProcStat.C,0,_providerDentist);
			InsertProcedure(patientAlex,"D1110","",dateThird,ProcStat.C,0,_providerHygienist);
			InsertProcedure(patientAlex,"D2391","30O",dateThird.AddDays(14),ProcStat.C,0,_providerDentist);
			InsertPayment(patientAlex,dateThird,200,"Check 1187");
			InsertPayment(patientAlex,dateThird.AddDays(14),168,"Card");
			//Alex's treatment plan: the filling of today's appointment and a crown to plan.
			InsertProcedure(patientAlex,"D2950","14",DateTime.MinValue,ProcStat.TP,0,_providerDentist);
			InsertProcedure(patientAlex,"D2740","14",DateTime.MinValue,ProcStat.TP,0,_providerDentist);
			InsertProcedure(patientAlex,"D0220","14",DateTime.MinValue,ProcStat.TP,0,_providerDentist);
			InsertInsurance(patientAlex,new[] { patientAlex,patientJamie,patientSam });
			InsertRecall(patientAlex,new DateTime(2026,6,16));
			InsertRecall(patientJamie,new DateTime(2026,6,16));
			InsertRecall(patientSam,new DateTime(2025,12,9));
			InsertCommlog(patientAlex,new DateTime(2026,11,24,9,12,0),CommItemMode.Phone,"Called to confirm the appointment on 12/1. Confirmed.");
			InsertCommlog(patientAlex,new DateTime(2026,6,30,15,40,0),CommItemMode.Email,"Sent the treatment plan for tooth 14.");
			Ledgers.ComputeAging(patientAlex.PatNum,DateAppointments);
			//Staff for the Manage module's time clock.
			InsertEmployee("Quinn","Sampler","Working");
			InsertEmployee("Rowan","Testa","Working");
			InsertEmployee("Emery","Mockley","Lunch");
			InsertEmployee("Harper","Fakename","Home");
			Cache.Refresh(InvalidType.AllLocal);
		}

		private void SetOperatory(Operatory operatory,string name,bool isHygiene) {
			operatory.OpName=name;
			operatory.Abbrev=isHygiene ? "HYG"+name.Last() : "OP"+name.Last();
			operatory.IsHygiene=isHygiene;
			operatory.ProvDentist=_providerDentist.ProvNum;
			operatory.ProvHygienist=isHygiene ? _providerHygienist.ProvNum : 0;
			Operatories.Update(operatory);
		}

		private Patient InsertPatient(string lName,string fName,string preferred,DateTime birthdate,PatientGender gender,PatientPosition position,
			string address,string phone,string email,Patient patientGuarantor)
		{
			Patient patient=new Patient {
				LName=lName,
				FName=fName,
				Preferred=preferred,
				Birthdate=birthdate,
				Gender=gender,
				Position=position,
				PatStatus=PatientStatus.Patient,
				Address=address,
				City="Springfield",
				State="OR",
				Zip="97000",
				HmPhone=phone,
				WirelessPhone=phone,
				Email=email,
				PriProv=_providerDentist.ProvNum,
				SecProv=_providerHygienist.ProvNum,
				BillingType=PrefC.GetLong(PrefName.PracticeDefaultBillType),
				PreferConfirmMethod=ContactMethod.WirelessPh,
				PreferContactMethod=ContactMethod.WirelessPh,
				PreferRecallMethod=ContactMethod.Email,
				DateFirstVisit=new DateTime(2025,6,10),
			};
			Patients.Insert(patient,false);
			Patient patientOld=patient.Copy();
			patient.Guarantor=patientGuarantor?.PatNum??patient.PatNum;
			Patients.Update(patient,patientOld);
			return patient;
		}

		private Schedule NewProviderSchedule(long provNum,long operatoryNum) {
			return new Schedule {
				SchedDate=DateAppointments,
				StartTime=TimeSpan.FromHours(8),
				StopTime=TimeSpan.FromHours(17),
				SchedType=ScheduleType.Provider,
				ProvNum=provNum,
				Status=SchedStatus.Open,
				Note="",
				Ops=new List<long> { operatoryNum },
			};
		}

		///<summary>Each procedure is "code", "code#tooth" or "code#toothSURF". Returns the AptNum.</summary>
		private long InsertAppointment(Patient patient,int hour,int minute,Operatory operatory,bool isHygiene,ApptStatus apptStatus,string pattern,
			string[] arrayProcedures)
		{
			Appointment appointment=new Appointment {
				PatNum=patient.PatNum,
				AptStatus=apptStatus,
				AptDateTime=DateAppointments.AddHours(hour).AddMinutes(minute),
				Op=operatory.OperatoryNum,
				ProvNum=_providerDentist.ProvNum,
				ProvHyg=isHygiene ? _providerHygienist.ProvNum : 0,
				IsHygiene=isHygiene,
				Pattern=pattern,
				Priority=ApptPriority.Normal,
				ProcDescript=string.Join(", ",arrayProcedures.Select(x => _dictCodes[x.Split('#')[0]].Item2)),
			};
			Appointments.Insert(appointment);
			ProcStat procStat=apptStatus==ApptStatus.Complete ? ProcStat.C : ProcStat.TP;
			foreach(string procedure in arrayProcedures) {
				string[] arrayParts=procedure.Split('#');
				string toothAndSurf=arrayParts.Length>1 ? arrayParts[1] : "";
				InsertProcedure(patient,arrayParts[0],toothAndSurf,DateAppointments,procStat,appointment.AptNum,
					isHygiene && !arrayParts[0].StartsWith("D01") ? _providerHygienist : _providerDentist);
			}
			return appointment.AptNum;
		}

		///<summary>toothAndSurf: the tooth number, then the surfaces in capitals ("19MO"), or empty.</summary>
		private void InsertProcedure(Patient patient,string code,string toothAndSurf,DateTime dateProc,ProcStat procStat,long aptNum,Provider provider) {
			string toothNum=new string(toothAndSurf.TakeWhile(char.IsDigit).ToArray());
			string surf=toothAndSurf.Substring(toothNum.Length);
			Procedure procedure=new Procedure {
				PatNum=patient.PatNum,
				CodeNum=ProcedureCodes.GetCodeNum(code),
				ProcDate=dateProc==DateTime.MinValue ? DateAppointments : dateProc,
				DateTP=dateProc==DateTime.MinValue ? DateAppointments.AddDays(-14) : dateProc,
				ProcFee=_dictCodes[code].Item1,
				ProcStatus=procStat,
				ProvNum=provider.ProvNum,
				ToothNum=toothNum,
				Surf=surf,
				AptNum=aptNum,
				BaseUnits=0,
				UnitQty=1,
				DateEntryC=procStat==ProcStat.C ? dateProc : DateTime.MinValue,
			};
			Procedures.Insert(procedure);
		}

		private void InsertPayment(Patient patient,DateTime datePay,double amount,string note) {
			List<Def> listDefsPayType=Defs.GetDefsForCategory(DefCat.PaymentTypes,true);
			Payment payment=new Payment {
				PatNum=patient.PatNum,
				PayDate=datePay,
				DateEntry=datePay,
				PayAmt=amount,
				PayType=listDefsPayType.Count>0 ? listDefsPayType[0].DefNum : 0,
				PayNote=note,
			};
			Payments.Insert(payment);
			PaySplit paySplit=new PaySplit {
				PatNum=patient.PatNum,
				PayNum=payment.PayNum,
				SplitAmt=amount,
				DatePay=datePay,
				ProvNum=_providerDentist.ProvNum,
			};
			PaySplits.Insert(paySplit);
		}

		///<summary>One dental plan of a fictional carrier for the family: the guarantor subscribes, everyone named is covered (self, spouse,
		///children), with an annual maximum, a deductible and the usual percentages.</summary>
		private void InsertInsurance(Patient patientSubscriber,Patient[] arrayPatientsCovered) {
			Carrier carrier=new Carrier { CarrierName="Example Dental Benefits",Address="1 Sample Plaza",City="Springfield",State="OR",Zip="97000",
				Phone="555-0199" };
			Carriers.Insert(carrier);
			Carriers.RefreshCache();
			InsPlan insPlan=new InsPlan { CarrierNum=carrier.CarrierNum,GroupName="Example Employer Group",GroupNum="EX-1001",PlanType="",
				FeeSched=_providerDentist.FeeSched,CobRule=EnumCobRule.Basic };
			InsPlans.Insert(insPlan);
			InsSub insSub=new InsSub { PlanNum=insPlan.PlanNum,Subscriber=patientSubscriber.PatNum,SubscriberID="EXM0001234",
				DateEffective=new DateTime(2025,1,1),ReleaseInfo=true,AssignBen=true };
			InsSubs.Insert(insSub);
			foreach(Patient patient in arrayPatientsCovered) {
				Relat relat=Relat.Self;
				if(patient.PatNum!=patientSubscriber.PatNum) {
					relat=patient.Position==PatientPosition.Child ? Relat.Child : Relat.Spouse;
				}
				PatPlans.Insert(new PatPlan { PatNum=patient.PatNum,InsSubNum=insSub.InsSubNum,Ordinal=1,Relationship=relat });
			}
			Benefits.Insert(new Benefit { PlanNum=insPlan.PlanNum,BenefitType=InsBenefitType.Limitations,CovCatNum=0,
				CoverageLevel=BenefitCoverageLevel.Individual,MonetaryAmt=1500,TimePeriod=BenefitTimePeriod.CalendarYear });
			Benefits.Insert(new Benefit { PlanNum=insPlan.PlanNum,BenefitType=InsBenefitType.Deductible,CovCatNum=0,
				CoverageLevel=BenefitCoverageLevel.Individual,MonetaryAmt=50,TimePeriod=BenefitTimePeriod.CalendarYear });
			foreach(Tuple<EbenefitCategory,int> tuple in new[] { Tuple.Create(EbenefitCategory.Diagnostic,100),
				Tuple.Create(EbenefitCategory.RoutinePreventive,100),Tuple.Create(EbenefitCategory.Restorative,80),
				Tuple.Create(EbenefitCategory.Endodontics,80),Tuple.Create(EbenefitCategory.Crowns,50) })
			{
				CovCat covCat=CovCats.GetForEbenCat(tuple.Item1);
				if(covCat==null) {
					continue;
				}
				Benefits.Insert(new Benefit { PlanNum=insPlan.PlanNum,BenefitType=InsBenefitType.CoInsurance,CovCatNum=covCat.CovCatNum,
					CoverageLevel=BenefitCoverageLevel.None,Percent=tuple.Item2,TimePeriod=BenefitTimePeriod.CalendarYear });
			}
		}

		///<summary>A prophy recall six months after the last visit.</summary>
		private static void InsertRecall(Patient patient,DateTime datePrevious) {
			Recall recall=new Recall {
				PatNum=patient.PatNum,
				RecallTypeNum=PrefC.GetLong(PrefName.RecallTypeSpecialProphy),
				DatePrevious=datePrevious,
				DateDueCalc=datePrevious.AddMonths(6),
				DateDue=datePrevious.AddMonths(6),
				RecallInterval=new Interval(0,0,6,0),
			};
			Recalls.Insert(recall);
		}

		private static void InsertCommlog(Patient patient,DateTime dateTime,CommItemMode commItemMode,string note) {
			List<Def> listDefsCommlogTypes=Defs.GetDefsForCategory(DefCat.CommLogTypes,true);
			Commlog commlog=new Commlog {
				PatNum=patient.PatNum,
				CommDateTime=dateTime,
				CommType=listDefsCommlogTypes.Count>0 ? listDefsCommlogTypes[0].DefNum : 0,
				Mode_=commItemMode,
				SentOrReceived=CommSentOrReceived.Sent,
				Note=note,
				UserNum=Security.CurUser.UserNum,
			};
			Commlogs.Insert(commlog);
		}

		private static void InsertEmployee(string fName,string lName,string clockStatus) {
			Employees.Insert(new Employee { FName=fName,LName=lName,ClockStatus=clockStatus });
		}
	}

	///<summary>The scratch database of DocScreenshots on the local MariaDB (127.0.0.1:3306, root, blank password): docs_<run id>, the committed
	///db/init/new-database.sql with every fork migration of this build run on it. Only names that start with docs_ are ever dropped.</summary>
	public static class DocScreenshotDb {
		public const string Server="127.0.0.1";
		public const string User="root";
		public const string Password="";
		public const string PREFIX="docs_";

		public static string NewName() {
			return PREFIX+"screens_"+DateTime.UtcNow.ToString("yyMMddHHmmss")+"_"+System.Diagnostics.Process.GetCurrentProcess().Id;
		}

		public static string GetSkipReason() {
			try {
				using(MySqlConnection connection=OpenServer()) {
					string sqlMode=Scalar(connection,"SELECT @@GLOBAL.sql_mode");
					if(sqlMode!="" && sqlMode.ToUpperInvariant()!="NO_AUTO_CREATE_USER") {
						return "Global sql_mode is '"+sqlMode+"'; the business layer expects a blank sql_mode.";
					}
				}
				return null;
			}
			catch(Exception ex) {
				return "No local MariaDB at "+Server+":3306 as "+User+" with a blank password: "+ex.GetBaseException().Message;
			}
		}

		///<summary>Builds the database and leaves the business layer connected to it as user 1 (Admin), with every cache filled.</summary>
		public static void Create(string name) {
			Drop(name);
			using(MySqlConnection connection=OpenServer()) {
				Exec(connection,"CREATE DATABASE `"+name+"` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci");
			}
			string text=File.ReadAllText(FindRepoFile(Path.Combine("db","init","new-database.sql")),new UTF8Encoding(false)).Replace("\r\n","\n");
			using(MySqlConnection connection=OpenServer(name)) {
				//Every statement of the generated file ends with a semicolon at the end of a line, and no value holds a raw line break.
				foreach(string statement in text.Split(new[] { ";\n" },StringSplitOptions.None)) {
					if(statement.Trim()=="") {
						continue;
					}
					Exec(connection,statement);
				}
			}
			RemotingClient.MiddleTierRole=MiddleTierRole.ClientDirect;
			//The constructor that makes no connection: the process may hold the no-database guard's connection, which the default one would open.
			new DataConnection(DatabaseType.MySql).SetDb(Server,name,User,Password,"","",DatabaseType.MySql,true,DevDbTls.GetSslCa(Server));
			if(!Prefs.DictIsNull()) {
				Prefs.ClearCache();
			}
			Cache.Refresh(InvalidType.Prefs);
			ForkMigrationRunner.Run();
			Cache.Refresh(InvalidType.AllLocal);
			Security.CurUser=Userods.GetUser(1)??new Userod { UserNum=1,UserName="Admin" };
		}

		public static void Drop(string name) {
			if(!name.StartsWith(PREFIX)) {
				throw new ArgumentException("Only databases whose name starts with "+PREFIX+" are dropped.");
			}
			using(MySqlConnection connection=OpenServer()) {
				Exec(connection,"DROP DATABASE IF EXISTS `"+name+"`");
			}
		}

		///<summary>Gives the process the no-database guard's connection again (Scan\NoDatabaseGuard), so the windows the other tests build after this
		///one can never read the scratch database, and empties the caches it filled.</summary>
		public static void RestoreNoDatabaseGuard() {
			NoDatabaseGuard.Install();
			PropertyInfo propertyInfo=typeof(DataConnection).GetProperty("ConnectionString",BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public);
			propertyInfo.SetValue(null,NoDatabaseGuard.CONNECTION_STRING);
			Security.CurUser=null;
		}

		private static MySqlConnection OpenServer(string database="") {
			MySqlConnection connection=new MySqlConnection(DevDbTls.Apply("Server="+Server+";Port=3306;User ID="+User+";Password="+Password
				+(database=="" ? "" : ";Database="+database)+";Connection Timeout=15;Allow User Variables=true;Default Command Timeout=600"));
			connection.Open();
			return connection;
		}

		private static void Exec(MySqlConnection connection,string command) {
			using(MySqlCommand mySqlCommand=new MySqlCommand(command,connection)) {
				mySqlCommand.CommandTimeout=600;
				mySqlCommand.ExecuteNonQuery();
			}
		}

		private static string Scalar(MySqlConnection connection,string command) {
			using(MySqlCommand mySqlCommand=new MySqlCommand(command,connection)) {
				object value=mySqlCommand.ExecuteScalar();
				return value==null || value==DBNull.Value ? "" : value.ToString();
			}
		}

		private static string FindRepoFile(string relativePath) {
			DirectoryInfo dir=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			while(dir!=null) {
				string candidate=Path.Combine(dir.FullName,relativePath);
				if(File.Exists(candidate)) {
					return candidate;
				}
				dir=dir.Parent;
			}
			throw new FileNotFoundException("Not found above the test folder: "+relativePath);
		}
	}
}
