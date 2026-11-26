// The screenshots of the how-to guides (docs/how-to/, docs/screenshots/README.md): the windows of the features this project added, drawn off
// screen with fictional data, as DocScreenshots draws the README's pictures. Opt-in: OPMS_UI_DOC_SCREENSHOTS names the folder; skipped
// otherwise. The same local scratch database as DocScreenshots (127.0.0.1:3306, root, blank password; dropped at the end), with DocSampleData's
// practice and this test's supplies and maintenance items, all fictional. Licensed under the GNU General Public License, version 2 or later. See
// LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class HowToScreenshots {
		[TestMethod]
		public void HowToScreenshots_ForkFeatures_WithFictionalData() {
			string folder=Environment.GetEnvironmentVariable(DocScreenshots.VARIABLE_FOLDER);
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: "+DocScreenshots.VARIABLE_FOLDER+" names the folder for the how-to screenshots.");
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
					Security.CurUser=Userods.GetUser(1)??new Userod { UserNum=1,UserName="Admin" };
					FormOpenDental.PatNumCur=docSampleData.PatNumSelected;
					DesignSheetWindows.IsPrintingWithoutChildren=true;
					try {
						long procNumComposite=InsertInventory(docSampleData.PatNumSelected);
						MaintItem maintItemSporeTest=InsertMaintenance();
						DocScreenshots.Save(folder,"howto-inventory-setup",listSaved,listFailed,() => DocScreenshots.DrawDialog(new FormInventorySetup()));
						DocScreenshots.Save(folder,"howto-inventory-materials-used",listSaved,listFailed,() => {
							FormMaterialsUsed formMaterialsUsed=new FormMaterialsUsed();
							formMaterialsUsed.ListProcNums=new List<long> { procNumComposite };
							return DocScreenshots.DrawDialog(formMaterialsUsed);
						});
						DocScreenshots.Save(folder,"howto-inventory-stock-count",listSaved,listFailed,() => DocScreenshots.DrawDialog(new FormSupplyCount()));
						DocScreenshots.Save(folder,"howto-maintenance-board",listSaved,listFailed,() => DocScreenshots.DrawDialog(new FormMaintBoard()));
						DocScreenshots.Save(folder,"howto-maintenance-record",listSaved,listFailed,
							() => DocScreenshots.DrawDialog(new FormMaintRecordEdit(maintItemSporeTest,DateTime.Today,DateTime.Today)));
						DocScreenshots.Save(folder,"howto-maintenance-item",listSaved,listFailed,
							() => DocScreenshots.DrawDialog(new FormMaintItemEdit(maintItemSporeTest,isNew:false)));
						DocScreenshots.Save(folder,"howto-db-settings",listSaved,listFailed,DrawDbSettings);
						//The Adopt Database window as it opens on a restored copy of an Open Dental 25.3.48 database: the labels hold what
						//RefreshPreflight writes for such a copy (Adoption.Preflight's message), with a fictional database name and server.
						DocScreenshots.Save(folder,"howto-adopt-database",listSaved,listFailed,() => DocScreenshots.DrawDialog(new FormAdoptDatabase(),form => {
							((Control)GridColumnFitTests.GetField(form,"labelDatabase")).Text="Database: opendental_copy   Server: localhost";
							((Control)GridColumnFitTests.GetField(form,"labelStatus")).Text="Open Dental 25.3.48 database. It can be adopted.";
							foreach(string name in new[] { "textConfirmName","checkRestoredCopy","butAdopt" }) {
								((Control)GridColumnFitTests.GetField(form,name)).Enabled=true;
							}
						}));
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
				if(Environment.GetEnvironmentVariable(DocScreenshots.VARIABLE_KEEP_DB)!="1") {
					try {
						DocScreenshotDb.Drop(dbName);
					}
					catch(Exception) {
						//A failed drop leaves it on the local server: drop docs_screens_* by hand.
					}
				}
				DocScreenshotDb.RestoreNoDatabaseGuard();
			}
			File.WriteAllLines(Path.Combine(folder,"howto-screenshots-failed.txt"),listFailed);
			Assert.IsTrue(listSaved.Count>0,"No picture made: "+string.Join("; ",listFailed));
		}

		///<summary>The connection settings tool (OpenPmsDentalDbSettings.exe, a plain window with its own title bar) filled with a fictional
		///server, database and database user, as setup opens it. Never shown; its Load runs.</summary>
		private static Bitmap DrawDbSettings() {
			Assembly assembly=WindowBuilder.LoadFromBuildFolder("OpenPmsDentalDbSettings","OpenPmsDentalDbSettings");
			Type typeRequest=assembly.GetType("OpenPmsDentalDbSettings.DbSettingsRequest",throwOnError:true);
			object request=Activator.CreateInstance(typeRequest);
			typeRequest.GetField("Server").SetValue(request,"db-server");
			typeRequest.GetField("Database").SetValue(request,"opendental");
			typeRequest.GetField("User").SetValue(request,"opms_app");
			Form form=(Form)Activator.CreateInstance(assembly.GetType("OpenPmsDentalDbSettings.FormDbSettings",throwOnError:true),
				new object[] { "",null,request });
			using(form) {
				form.StartPosition=FormStartPosition.Manual;
				form.Location=new Point(0,0);
				DesignSheetWindows.CreateHandles(form);
				typeof(Form).GetMethod("OnLoad",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(form,new object[] { EventArgs.Empty });
				DocScreenshots.Settle();
				return PlainLookCopy.CaptureWindow(form);
			}
		}

		///<summary>Turns inventory tracking on and adds a fictional supplier, six supplies (two tracked by lot, one an implant), the default materials
		///of a one-surface posterior composite (D2391), and that procedure completed today for the selected patient. Returns its ProcNum.</summary>
		private static long InsertInventory(long patNum) {
			InventoryTracking.SaveSettings(isEnabled:true,expiryAlertDays:30);
			Cache.Refresh(InvalidType.Prefs,InvalidType.Security);
			long supplierNum=Suppliers.Insert(new Supplier { Name="Example Dental Supply",Phone="(555) 010-0150",CustomerId="",Website="",UserName="",
				Password="",Note="" });
			Def defCategory=new Def { Category=DefCat.SupplyCats,ItemName="Restorative",ItemOrder=0 };
			Defs.Insert(defCategory);
			Cache.Refresh(InvalidType.Defs);
			Supply supplyComposite=InsertSupply(supplierNum,defCategory.DefNum,"Composite A2, 4 g syringe","CMP-A2",38.50,"0712345000011",onHand:14,reorder:4);
			Supply supplyBond=InsertSupply(supplierNum,defCategory.DefNum,"Bonding agent, 5 ml","BND-5",62.00,"0712345000028",onHand:6,reorder:2);
			Supply supplyEtch=InsertSupply(supplierNum,defCategory.DefNum,"Etch gel 37%, 3 ml","ETC-37",9.75,"0712345000035",onHand:3,reorder:5);
			InsertSupply(supplierNum,defCategory.DefNum,"Nitrile gloves, medium (box of 200)","GLV-M",11.20,"0712345000042",onHand:22,reorder:10);
			Supply supplyAnesthetic=InsertSupply(supplierNum,defCategory.DefNum,"Lidocaine 2% carpules (box of 50)","LID-2",44.00,"0712345000059",
				onHand:0,reorder:2,isLotTracked:true);
			InventoryTracking.Adjust(supplyAnesthetic.SupplyNum,SupplyLots.GetOrInsert(supplyAnesthetic.SupplyNum,"L24187",DateTime.Today.AddDays(21),0)
				.SupplyLotNum,3,"Opening stock");
			Supply supplyImplant=InsertSupply(supplierNum,defCategory.DefNum,"Implant 4.1 x 10 mm","IMP-4110",289.00,"0712345000066",onHand:0,reorder:1,
				isLotTracked:true,isImplant:true);
			InventoryTracking.Adjust(supplyImplant.SupplyNum,SupplyLots.GetOrInsert(supplyImplant.SupplyNum,"IM-30551",DateTime.Today.AddYears(3),0)
				.SupplyLotNum,2,"Opening stock");
			long codeNum=ProcedureCodes.GetCodeNum("D2391");
			SupplyProcCodes.SyncForCode(codeNum,new List<SupplyProcCode> {
				new SupplyProcCode { SupplyNum=supplyComposite.SupplyNum,Quantity=1 },
				new SupplyProcCode { SupplyNum=supplyBond.SupplyNum,Quantity=1 },
				new SupplyProcCode { SupplyNum=supplyEtch.SupplyNum,Quantity=1 },
			});
			//A stock count in progress: three items counted, one of them one short of what the program holds.
			foreach((Supply Supply,double Qty) count in new[] { (supplyComposite,13.0),(supplyBond,6.0),(supplyEtch,3.0) }) {
				InventoryTracking.SaveCountLine(new SupplyCountLine { SupplyNum=count.Supply.SupplyNum,LotNumber="",QtyCounted=count.Qty },false);
			}
			Patient patient=Patients.GetPat(patNum);
			Procedure procedure=new Procedure();
			procedure.PatNum=patNum;
			procedure.CodeNum=codeNum;
			procedure.ProcDate=DateTime.Today;
			procedure.DateTP=DateTime.Today;
			procedure.ToothNum="30";
			procedure.Surf="O";
			procedure.ProcStatus=ProcStat.C;
			procedure.ProvNum=patient.PriProv;
			procedure.ClinicNum=patient.ClinicNum;
			procedure.ProcFee=165;
			Procedures.Insert(procedure);
			return procedure.ProcNum;
		}

		private static Supply InsertSupply(long supplierNum,long category,string descript,string catalogNumber,double price,string barcode,double onHand,
			double reorder,bool isLotTracked=false,bool isImplant=false)
		{
			Supply supply=new Supply();
			supply.SupplierNum=supplierNum;
			supply.Category=category;
			supply.CatalogNumber=catalogNumber;
			supply.Descript=descript;
			supply.Price=price;
			supply.BarCodeOrID=barcode;
			supply.DispUnitDesc="";
			supply.LevelDesired=(float)(reorder*3);
			supply.LevelOnHand=(float)onHand;
			Supplies.Insert(supply);
			SupplySettings.Upsert(new SupplySetting { SupplyNum=supply.SupplyNum,IsLotTracked=isLotTracked,IsImplant=isImplant,ReorderLevel=reorder });
			return supply;
		}

		///<summary>Five fictional maintenance items with past records, so the board shows items done, due soon, due and overdue. Returns the spore
		///test.</summary>
		private static MaintItem InsertMaintenance() {
			DateTime today=DateTime.Today;
			MaintItem maintItemSpore=InsertMaintItem("Steam sterilizer spore test","Run a biological indicator in a full load; record the result.",
				EnumMaintScheduleType.Interval,1,EnumMaintIntervalUnit.Weeks,today.AddDays(-35),2);
			InsertMaintRecords(maintItemSpore,today.AddDays(-35),7,4,"Negative");
			MaintSteps.Sync(maintItemSpore.MaintItemNum,new List<MaintStep> {
				new MaintStep { ItemOrder=0,Description="Place the indicator in the middle of a full load" },
				new MaintStep { ItemOrder=1,Description="Run a standard cycle" },
				new MaintStep { ItemOrder=2,Description="Incubate the indicator and a control for 24 hours" },
			});
			MaintFields.Sync(maintItemSpore.MaintItemNum,new List<MaintField> {
				new MaintField { ItemOrder=0,FieldName="Indicator result",FieldType=EnumMaintFieldType.PassFail,Unit="",IsRequired=true },
				new MaintField { ItemOrder=1,FieldName="Chamber temperature",FieldType=EnumMaintFieldType.Number,Unit="°C",HasMin=true,ValueMin=132,
					HasMax=true,ValueMax=135,IsRequired=true },
				new MaintField { ItemOrder=2,FieldName="Indicator lot",FieldType=EnumMaintFieldType.Text,Unit="",IsRequired=false },
			});
			MaintItem maintItemWaterline=InsertMaintItem("Dental unit waterline test","Send the water samples of every operatory to the lab.",
				EnumMaintScheduleType.Interval,1,EnumMaintIntervalUnit.Months,today.AddMonths(-3),7);
			InsertMaintRecords(maintItemWaterline,today.AddMonths(-3).AddDays(-2),30,2,"Below 500 CFU/ml");
			MaintItem maintItemEyewash=InsertMaintItem("Eyewash station flush","Run each station for three minutes.",
				EnumMaintScheduleType.Interval,1,EnumMaintIntervalUnit.Weeks,today.AddDays(-30),1);
			InsertMaintRecords(maintItemEyewash,today.AddDays(-30),7,2,"");
			MaintItem maintItemCompressor=InsertMaintItem("Air compressor tank drain","Open the drain valve until only air comes out.",
				EnumMaintScheduleType.Interval,1,EnumMaintIntervalUnit.Weeks,today.AddDays(-6),2);
			InsertMaintRecords(maintItemCompressor,today.AddDays(-6),7,1,"");
			MaintItem maintItemExtinguisher=InsertMaintItem("Fire extinguisher inspection","Check the gauge, pin and tag of each extinguisher.",
				EnumMaintScheduleType.Interval,1,EnumMaintIntervalUnit.Years,today.AddMonths(-2),30);
			InsertMaintRecords(maintItemExtinguisher,today.AddMonths(-2),365,1,"");
			return maintItemSpore;
		}

		private static MaintItem InsertMaintItem(string name,string description,EnumMaintScheduleType scheduleType,int intervalCount,
			EnumMaintIntervalUnit intervalUnit,DateTime dateAnchor,int leadDays)
		{
			MaintItem maintItem=new MaintItem { ItemName=name,Description=description,ScheduleType=scheduleType,IntervalCount=intervalCount,
				IntervalUnit=intervalUnit,DateAnchor=dateAnchor,LeadDays=leadDays };
			MaintItems.Insert(maintItem);
			return maintItem;
		}

		///<summary>Records done on time, every intervalDays from the first date, count of them.</summary>
		private static void InsertMaintRecords(MaintItem maintItem,DateTime dateFirst,int intervalDays,int count,string note) {
			for(int i=0;i<count;i++) {
				DateTime date=dateFirst.AddDays(intervalDays*i);
				MaintRecords.Insert(new MaintRecord { MaintItemNum=maintItem.MaintItemNum,DateDone=date,DateDue=date,UserNum=Security.CurUser.UserNum,
					Note=note },new List<MaintRecordValue>());
			}
		}
	}
}
