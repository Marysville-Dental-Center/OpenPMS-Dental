// IMG-P3a, the data layer of the image adjust tools on a scratch 25.3.48 database upgraded by the fork migration runner: the two fork-only tables
// match their declarations, the saved filters of an image (DocumentAdjusts), the office's presets (ImageAdjustPresets) and the grant of Image Adjust
// Presets to the Setup groups (ImageAdjustPresetGrants). Synthetic data only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.ForkMigrations;
using OpenPmsDentalBusiness.ForkMigrations.FeatureSteps;
using UnitTestsCore;

namespace ImagingTests {
	[TestClass]
	public class ImageAdjustDbTests {
		[TestInitialize]
		public void TestInitialize() {
			ImagingDb.Begin();
			ImagingDb.SetStorage(DataStorageType.InDatabase);
		}

		private static DocumentAdjust Make(long docNum,double gamma=1,int sharpen=0,ImageSmooth smooth=ImageSmooth.Off,bool isAutoContrast=false,bool isInverted=false) {
			ImageAdjustSettings settings=new ImageAdjustSettings { Gamma=gamma,Sharpen=sharpen,Smooth=smooth,IsAutoContrast=isAutoContrast,IsInverted=isInverted };
			return DocumentAdjust.FromSettings(docNum,settings);
		}

		private static long CountRows(string table) {
			return PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM "+table));
		}

		#region Declarations
		[TestMethod]
		public void Steps_LevelsAndTables_MatchTheirDeclaration() {
			List<ForkMigrationStep> listSteps=ForkMigrationRunner.GetSteps().Where(x => x.Level>=2026112301 && x.Level<=2026112309).ToList();
			CollectionAssert.AreEqual(new long[] { 2026112301,2026112302,2026112303 },listSteps.Select(x => x.Level).ToList());
			Assert.IsInstanceOfType(listSteps[0],typeof(DocumentAdjustTableStep));
			Assert.IsInstanceOfType(listSteps[1],typeof(ImageAdjustPresetTableStep));
			Assert.IsInstanceOfType(listSteps[2],typeof(ImageAdjustPresetGrantStep));
			CollectionAssert.IsSubsetOf(new[] { "documentadjust","imageadjustpreset" },ForkTables.GetTableNames());
			Assert.AreEqual("Image adjust tools",new ImagingAdjustTables().Feature);
			Assert.AreEqual(10150,(int)EnumPermType.ImageAdjustPresets);
			Assert.IsTrue(ForkMigrationRunner.GetAppliedLevels().Contains(2026112301));
			Assert.IsTrue(ForkMigrationRunner.GetAppliedLevels().Contains(2026112302));
			Assert.IsTrue(ForkMigrationRunner.GetAppliedLevels().Contains(2026112303));
			SchemaVerifyResult result=ForkTables.VerifyDatabase(ImagingDb.DbName);
			Assert.AreEqual(0,result.ListDifferences.FindAll(x => x.Table=="documentadjust" || x.Table=="imageadjustpreset").Count,result.GetReport());
			Assert.AreEqual(0,result.ListMismatches.FindAll(x => x.Table=="documentadjust" || x.Table=="imageadjustpreset").Count,result.GetReport());
			SchemaTable schemaTable=ForkTables.GetSnapshot().GetTable("documentadjust");
			Assert.AreEqual("UNIQUE KEY",schemaTable.GetIndex("DocNum").Kind,"One row per image.");
		}
		#endregion Declarations

		#region DocumentAdjusts
		[TestMethod]
		public void DocumentAdjusts_UpsertGetAndNeutralDelete() {
			Patient patient=ImagingDb.CreatePatient("adj1");
			Document document1=ImagingDb.CreateDocument(patient,"",DateTime.Now);
			Document document2=ImagingDb.CreateDocument(patient,"",DateTime.Now);
			Document document3=ImagingDb.CreateDocument(patient,"",DateTime.Now);
			Assert.IsNull(DocumentAdjusts.GetForDoc(document1.DocNum),"No row until something is not neutral.");
			DocumentAdjusts.Upsert(Make(document1.DocNum,gamma:1.5,sharpen:40,smooth:ImageSmooth.High,isAutoContrast:true,isInverted:true));
			DocumentAdjusts.Upsert(Make(document2.DocNum,sharpen:10));
			DocumentAdjust documentAdjust=DocumentAdjusts.GetForDoc(document1.DocNum);
			Assert.IsNotNull(documentAdjust);
			Assert.AreEqual(1.5,documentAdjust.Gamma);
			Assert.AreEqual(40,documentAdjust.Sharpen);
			Assert.AreEqual(ImageSmooth.High,documentAdjust.Smooth);
			Assert.IsTrue(documentAdjust.IsAutoContrast);
			Assert.IsTrue(documentAdjust.IsInverted);
			Assert.AreEqual(0,documentAdjust.TrimStartMs);
			Assert.AreEqual(1.5,documentAdjust.ToSettings().Gamma);
			long numFirst=documentAdjust.DocumentAdjustNum;
			//GetForDocs: one query, rows only for the images that have one.
			List<DocumentAdjust> listRows=DocumentAdjusts.GetForDocs(new List<long> { document1.DocNum,document2.DocNum,document3.DocNum });
			CollectionAssert.AreEquivalent(new[] { document1.DocNum,document2.DocNum },listRows.Select(x => x.DocNum).ToList());
			Assert.AreEqual(0,DocumentAdjusts.GetForDocs(new List<long>()).Count);
			//A second upsert of the same image updates its row.
			DocumentAdjusts.Upsert(Make(document1.DocNum,gamma:0.8));
			Assert.AreEqual(2,CountRows("documentadjust WHERE DocNum IN("+document1.DocNum+","+document2.DocNum+")"));
			documentAdjust=DocumentAdjusts.GetForDoc(document1.DocNum);
			Assert.AreEqual(numFirst,documentAdjust.DocumentAdjustNum);
			Assert.AreEqual(0.8,documentAdjust.Gamma);
			Assert.AreEqual(0,documentAdjust.Sharpen);
			//Neutral settings with no trim delete the row; a trim alone keeps it.
			DocumentAdjusts.Upsert(Make(document1.DocNum));
			Assert.IsNull(DocumentAdjusts.GetForDoc(document1.DocNum));
			DocumentAdjust documentAdjustTrim=Make(document1.DocNum);
			documentAdjustTrim.TrimEndMs=5000;
			DocumentAdjusts.Upsert(documentAdjustTrim);
			Assert.AreEqual(5000,DocumentAdjusts.GetForDoc(document1.DocNum).TrimEndMs);
			DocumentAdjusts.Upsert(Make(document1.DocNum));
			Assert.IsNull(DocumentAdjusts.GetForDoc(document1.DocNum));
			Assert.IsNotNull(DocumentAdjusts.GetForDoc(document2.DocNum),"Another image's row is untouched.");
			DocumentAdjusts.DeleteForDoc(document2.DocNum);
			Assert.IsNull(DocumentAdjusts.GetForDoc(document2.DocNum));
			DocumentAdjusts.DeleteForDoc(document3.DocNum);//no row: nothing happens
		}

		[TestMethod]
		public void DocumentsDelete_AlsoDeletesTheAdjustRow() {
			Patient patient=ImagingDb.CreatePatient("adj2");
			Document document=ImagingDb.CreateDocument(patient,"",DateTime.Now);
			Document documentOther=ImagingDb.CreateDocument(patient,"",DateTime.Now);
			DocumentAdjusts.Upsert(Make(document.DocNum,sharpen:30));
			DocumentAdjusts.Upsert(Make(documentOther.DocNum,sharpen:30));
			Documents.Delete(document);
			Assert.IsNull(DocumentAdjusts.GetForDoc(document.DocNum));
			Assert.IsNotNull(DocumentAdjusts.GetForDoc(documentOther.DocNum));
		}
		#endregion DocumentAdjusts

		#region ImageAdjustPresets
		[TestMethod]
		public void ImageAdjustPresets_InsertUpdateDelete_OrderedByItemOrder() {
			DataCore.NonQ("DELETE FROM imageadjustpreset");
			Assert.AreEqual(0,ImageAdjustPresets.GetAll().Count);
			ImageAdjustPreset presetB=new ImageAdjustPreset { Description="B",ItemOrder=2,WindowingMin=10,WindowingMax=200,Gamma=1.2,Sharpen=20,Smooth=ImageSmooth.Low,
				IsAutoContrast=true,IsInverted=false,IsHidden=false };
			ImageAdjustPreset presetA=new ImageAdjustPreset { Description="A",ItemOrder=1,Gamma=1 };
			long numB=ImageAdjustPresets.Insert(presetB);
			long numA=ImageAdjustPresets.Insert(presetA);
			Assert.AreNotEqual(0,numB);
			Assert.AreEqual(numB,presetB.ImageAdjustPresetNum);
			List<ImageAdjustPreset> listPresets=ImageAdjustPresets.GetAll();
			CollectionAssert.AreEqual(new[] { numA,numB },listPresets.Select(x => x.ImageAdjustPresetNum).ToList(),"By ItemOrder.");
			ImageAdjustPreset presetRead=listPresets[1];
			Assert.AreEqual("B",presetRead.Description);
			Assert.AreEqual(10,presetRead.WindowingMin);
			Assert.AreEqual(200,presetRead.WindowingMax);
			Assert.AreEqual(1.2,presetRead.Gamma);
			Assert.AreEqual(ImageSmooth.Low,presetRead.Smooth);
			Assert.IsTrue(presetRead.IsAutoContrast);
			Assert.IsFalse(presetRead.IsInverted);
			Assert.AreEqual(20,presetRead.ToSettings().Sharpen);
			presetRead.IsHidden=true;
			presetRead.ItemOrder=0;
			presetRead.Description="B renamed";
			ImageAdjustPresets.Update(presetRead);
			listPresets=ImageAdjustPresets.GetAll();
			CollectionAssert.AreEqual(new[] { numB,numA },listPresets.Select(x => x.ImageAdjustPresetNum).ToList());
			Assert.AreEqual("B renamed",listPresets[0].Description);
			Assert.IsTrue(listPresets[0].IsHidden);
			ImageAdjustPresets.Delete(numB);
			Assert.AreEqual(1,ImageAdjustPresets.GetAll().Count);
			ImageAdjustPresets.Delete(numA);
			Assert.AreEqual(0,ImageAdjustPresets.GetAll().Count);
		}
		#endregion ImageAdjustPresets

		#region Grants
		[TestMethod]
		public void GrantPlan_SetupGroupsGetIt_OthersDoNot_ExistingRowsAreNotDuplicated() {
			List<(long UserGroupNum,int PermType)> listRows=new List<(long,int)> {
				(30,(int)EnumPermType.Setup),
				(30,(int)EnumPermType.Setup),
				(30,(int)EnumPermType.AppointmentEdit),
				(40,(int)EnumPermType.Setup),
				(40,(int)EnumPermType.ImageAdjustPresets),//has it already
				(50,(int)EnumPermType.SecurityAdmin),
				(60,(int)EnumPermType.ImageEdit),
			};
			List<ImageAdjustPresetGrants.Grant> listGrants=ImageAdjustPresetGrants.Plan(listRows);
			Assert.AreEqual(1,listGrants.Count);
			Assert.AreEqual(30,listGrants[0].UserGroupNum);
			Assert.AreEqual(EnumPermType.ImageAdjustPresets,listGrants[0].PermType);
			listRows.AddRange(listGrants.Select(x => (x.UserGroupNum,(int)x.PermType)));
			Assert.AreEqual(0,ImageAdjustPresetGrants.Plan(listRows).Count);
			Assert.AreEqual("Granted ImageAdjustPresets to 2 Setup user group(s)",ImageAdjustPresetGrants.Describe(EnumPermType.ImageAdjustPresets,2));
		}

		[TestMethod]
		public void GrantStep_OnTheScratchDatabase_InsertsOnlyTheMissingRows_Once() {
			long groupSetup=UserGroupT.CreateUserGroup("A-IMG adjust setup "+Guid.NewGuid().ToString("N").Substring(0,8));
			long groupHas=UserGroupT.CreateUserGroup("A-IMG adjust has "+Guid.NewGuid().ToString("N").Substring(0,8));
			long groupOther=UserGroupT.CreateUserGroup("A-IMG adjust other "+Guid.NewGuid().ToString("N").Substring(0,8));
			foreach((long group,EnumPermType perm) in new[] { (groupSetup,EnumPermType.Setup),(groupHas,EnumPermType.Setup),(groupHas,EnumPermType.ImageAdjustPresets),
				(groupOther,EnumPermType.SecurityAdmin) })
			{
				DataCore.NonQ("INSERT INTO grouppermission (NewerDate,NewerDays,UserGroupNum,PermType,FKey) VALUES('0001-01-01',30,"+group+","+(int)perm+",0)");
			}
			ForkMigrationContext context=new ForkMigrationContext(ImagingDb.DbName);
			List<string> listLog=ImageAdjustPresetGrants.Run();
			Assert.AreEqual(1,listLog.Count);
			StringAssert.StartsWith(listLog[0],"Granted ImageAdjustPresets to ");
			Func<long,long> countRows=x => PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM grouppermission WHERE UserGroupNum="+x+" AND PermType="+(int)EnumPermType.ImageAdjustPresets));
			Assert.AreEqual(1,countRows(groupSetup));
			Assert.AreEqual(1,countRows(groupHas),"Not duplicated.");
			Assert.AreEqual(0,countRows(groupOther),"Security Admin alone does not get it.");
			Assert.AreEqual(30,PIn.Int(DataCore.GetScalar("SELECT NewerDays FROM grouppermission WHERE UserGroupNum="+groupHas+" AND PermType="+(int)EnumPermType.ImageAdjustPresets)),
				"An existing row is not touched.");
			Assert.AreEqual(0,PIn.Int(DataCore.GetScalar("SELECT NewerDays FROM grouppermission WHERE UserGroupNum="+groupSetup+" AND PermType="+(int)EnumPermType.ImageAdjustPresets)));
			Assert.AreEqual(0,PIn.Int(DataCore.GetScalar("SELECT FKey FROM grouppermission WHERE UserGroupNum="+groupSetup+" AND PermType="+(int)EnumPermType.ImageAdjustPresets)));
			Assert.AreEqual(0,ImageAdjustPresetGrants.Run().Count,"A second run inserts nothing.");
			//The step skips a database whose ForkMigrationsApplied holds its level.
			DataCore.NonQ("DELETE FROM grouppermission WHERE UserGroupNum="+groupSetup+" AND PermType="+(int)EnumPermType.ImageAdjustPresets);
			new ImageAdjustPresetGrantStep().Apply(context);
			Assert.AreEqual(0,context.ListChanges.Count);
			Assert.AreEqual(0,countRows(groupSetup),"Runs once per database: a removed grant is not given back.");
		}
		#endregion Grants
	}
}
