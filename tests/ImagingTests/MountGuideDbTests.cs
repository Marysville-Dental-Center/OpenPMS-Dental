// IMG-P4a, the data layer of the photo mount guides on a scratch 25.3.48 database upgraded by the fork migration runner: the two fork-only tables
// match their declarations, the guide of a template slot (MountItemDefGuides) and of a patient's mount item (MountItemGuides), the copy made by
// Mounts.CreateMountFromDef, and the deletes that leave no guide row behind. Synthetic data only.
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
	public class MountGuideDbTests {
		[TestInitialize]
		public void TestInitialize() {
			ImagingDb.Begin();
		}

		private static long CountRows(string table) {
			return PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM "+table));
		}

		///<summary>A mount template with this many slots, none with a guide yet.</summary>
		private static MountDef MakeDef(string description,int slots,out List<MountItemDef> listItemDefs) {
			MountDef mountDef=new MountDef { Description=description,ScaleValue="",Width=400,Height=300 };
			mountDef.MountDefNum=MountDefs.Insert(mountDef);
			listItemDefs=new List<MountItemDef>();
			for(int i=0;i<slots;i++) {
				MountItemDef mountItemDef=new MountItemDef { MountDefNum=mountDef.MountDefNum,Xpos=i*10,Ypos=0,ItemOrder=i+1,Width=10,Height=10,
					ToothNumbers="",TextShowing="" };
				mountItemDef.MountItemDefNum=MountItemDefs.Insert(mountItemDef);
				listItemDefs.Add(mountItemDef);
			}
			return mountDef;
		}

		private static MountItemDefGuide MakeDefGuide(long mountItemDefNum,string label,bool isMirror=false,MountSlotKind slotKind=MountSlotKind.Intraoral) {
			return new MountItemDefGuide { MountItemDefNum=mountItemDefNum,GuideLabel=label,IsMirror=isMirror,SlotKind=slotKind };
		}

		#region Declarations
		[TestMethod]
		public void Steps_LevelsAndTables_MatchTheirDeclaration() {
			List<ForkMigrationStep> listSteps=ForkMigrationRunner.GetSteps().Where(x => x.Level>=2026112311 && x.Level<=2026112319).ToList();
			CollectionAssert.AreEqual(new long[] { 2026112311,2026112312 },listSteps.Select(x => x.Level).ToList());
			Assert.IsInstanceOfType(listSteps[0],typeof(MountItemDefGuideTableStep));
			Assert.IsInstanceOfType(listSteps[1],typeof(MountItemGuideTableStep));
			CollectionAssert.IsSubsetOf(new[] { "mountitemdefguide","mountitemguide" },ForkTables.GetTableNames());
			Assert.AreEqual("Image camera session",new ImagingCameraTables().Feature);
			Assert.IsTrue(ForkMigrationRunner.GetAppliedLevels().Contains(2026112311));
			Assert.IsTrue(ForkMigrationRunner.GetAppliedLevels().Contains(2026112312));
			SchemaVerifyResult result=ForkTables.VerifyDatabase(ImagingDb.DbName);
			Assert.AreEqual(0,result.ListDifferences.FindAll(x => x.Table=="mountitemdefguide" || x.Table=="mountitemguide").Count,result.GetReport());
			Assert.AreEqual(0,result.ListMismatches.FindAll(x => x.Table=="mountitemdefguide" || x.Table=="mountitemguide").Count,result.GetReport());
			Assert.AreEqual("UNIQUE KEY",ForkTables.GetSnapshot().GetTable("mountitemdefguide").GetIndex("MountItemDefNum").Kind,"One row per template slot.");
			Assert.AreEqual("UNIQUE KEY",ForkTables.GetSnapshot().GetTable("mountitemguide").GetIndex("MountItemNum").Kind,"One row per mount item.");
			Assert.AreEqual(0,(int)MountSlotKind.Radiograph);
			Assert.AreEqual(1,(int)MountSlotKind.Intraoral);
			Assert.AreEqual(2,(int)MountSlotKind.Extraoral);
		}
		#endregion Declarations

		#region Upsert and Get
		[TestMethod]
		public void MountItemDefGuides_UpsertGetAndDelete() {
			MountDef mountDef=MakeDef("guide def 1",3,out List<MountItemDef> listItemDefs);
			Assert.IsNull(MountItemDefGuides.GetForMountItemDef(listItemDefs[0].MountItemDefNum));
			MountItemDefGuides.Upsert(MakeDefGuide(listItemDefs[0].MountItemDefNum,"Upper occlusal",isMirror:true));
			MountItemDefGuides.Upsert(MakeDefGuide(listItemDefs[1].MountItemDefNum,"Smile",slotKind:MountSlotKind.Extraoral));
			MountItemDefGuide guide=MountItemDefGuides.GetForMountItemDef(listItemDefs[0].MountItemDefNum);
			Assert.IsNotNull(guide);
			Assert.AreEqual("Upper occlusal",guide.GuideLabel);
			Assert.IsTrue(guide.IsMirror);
			Assert.AreEqual(MountSlotKind.Intraoral,guide.SlotKind);
			long numFirst=guide.MountItemDefGuideNum;
			List<MountItemDefGuide> listGuides=MountItemDefGuides.GetForMountItemDefs(listItemDefs.Select(x => x.MountItemDefNum).ToList());
			CollectionAssert.AreEquivalent(new[] { listItemDefs[0].MountItemDefNum,listItemDefs[1].MountItemDefNum },listGuides.Select(x => x.MountItemDefNum).ToList());
			Assert.AreEqual(MountSlotKind.Extraoral,listGuides.First(x => x.MountItemDefNum==listItemDefs[1].MountItemDefNum).SlotKind);
			Assert.AreEqual(0,MountItemDefGuides.GetForMountItemDefs(new List<long>()).Count);
			//A second upsert of the same slot updates its row, whatever key the argument carries.
			MountItemDefGuide guideNew=MakeDefGuide(listItemDefs[0].MountItemDefNum,"Lower occlusal",slotKind:MountSlotKind.Radiograph);
			guideNew.MountItemDefGuideNum=987654;
			MountItemDefGuides.Upsert(guideNew);
			Assert.AreEqual(1,CountRows("mountitemdefguide WHERE MountItemDefNum="+listItemDefs[0].MountItemDefNum));
			guide=MountItemDefGuides.GetForMountItemDef(listItemDefs[0].MountItemDefNum);
			Assert.AreEqual(numFirst,guide.MountItemDefGuideNum);
			Assert.AreEqual("Lower occlusal",guide.GuideLabel);
			Assert.IsFalse(guide.IsMirror);
			Assert.AreEqual(MountSlotKind.Radiograph,guide.SlotKind);
			MountItemDefGuides.DeleteForMountItemDef(listItemDefs[0].MountItemDefNum);
			Assert.IsNull(MountItemDefGuides.GetForMountItemDef(listItemDefs[0].MountItemDefNum));
			Assert.IsNotNull(MountItemDefGuides.GetForMountItemDef(listItemDefs[1].MountItemDefNum),"Another slot's row is untouched.");
			MountItemDefGuides.DeleteForMountItemDef(listItemDefs[2].MountItemDefNum);//no row: nothing happens
			MountDefs.Delete(mountDef.MountDefNum);
		}

		[TestMethod]
		public void MountItemGuides_UpsertGetAndDelete() {
			Patient patient=ImagingDb.CreatePatient("mg1");
			MountDef mountDef=MakeDef("guide def 2",1,out List<MountItemDef> listItemDefs);
			Mount mount=Mounts.CreateMountFromDef(mountDef,patient.PatNum,ImagingDb.GetImageCategory());
			List<MountItem> listItems=MountItems.GetItemsForMount(mount.MountNum);
			Assert.AreEqual(1,listItems.Count);
			long mountItemNum=listItems[0].MountItemNum;
			Assert.IsNull(MountItemGuides.GetForMountItem(mountItemNum));
			MountItemGuides.Upsert(new MountItemGuide { MountItemNum=mountItemNum,GuideLabel="Left lateral",IsMirror=true,SlotKind=MountSlotKind.Intraoral });
			MountItemGuide guide=MountItemGuides.GetForMountItem(mountItemNum);
			Assert.AreEqual("Left lateral",guide.GuideLabel);
			Assert.IsTrue(guide.IsMirror);
			Assert.AreEqual(MountSlotKind.Intraoral,guide.SlotKind);
			MountItemGuides.Upsert(new MountItemGuide { MountItemNum=mountItemNum,GuideLabel="Profile",IsMirror=false,SlotKind=MountSlotKind.Extraoral });
			Assert.AreEqual(1,CountRows("mountitemguide WHERE MountItemNum="+mountItemNum));
			guide=MountItemGuides.GetForMountItem(mountItemNum);
			Assert.AreEqual("Profile",guide.GuideLabel);
			Assert.AreEqual(MountSlotKind.Extraoral,guide.SlotKind);
			CollectionAssert.AreEqual(new[] { mountItemNum },MountItemGuides.GetForMountItems(new List<long> { mountItemNum,mountItemNum+100000 }).Select(x => x.MountItemNum).ToList());
			Assert.AreEqual(0,MountItemGuides.GetForMountItems(null).Count);
			MountItemGuides.DeleteForMountItem(mountItemNum);
			Assert.IsNull(MountItemGuides.GetForMountItem(mountItemNum));
			MountItemGuides.DeleteForMountItem(mountItemNum);//no row: nothing happens
			Mounts.Delete(mount);
			MountDefs.Delete(mountDef.MountDefNum);
		}
		#endregion Upsert and Get

		#region CreateMountFromDef
		[TestMethod]
		public void CreateMountFromDef_CopiesTheGuides_AndLeavesSlotsWithoutOneAlone() {
			Patient patient=ImagingDb.CreatePatient("mg2");
			MountDef mountDef=MakeDef("guide def 3",3,out List<MountItemDef> listItemDefs);
			MountItemDefGuides.Upsert(MakeDefGuide(listItemDefs[0].MountItemDefNum,"Upper occlusal",isMirror:true));
			MountItemDefGuides.Upsert(MakeDefGuide(listItemDefs[2].MountItemDefNum,"Smile",slotKind:MountSlotKind.Extraoral));
			long countBefore=CountRows("mountitemguide");
			Mount mount=Mounts.CreateMountFromDef(mountDef,patient.PatNum,ImagingDb.GetImageCategory());
			List<MountItem> listItems=MountItems.GetItemsForMount(mount.MountNum);
			Assert.AreEqual(3,listItems.Count);
			List<MountItemGuide> listGuides=MountItemGuides.GetForMountItems(listItems.Select(x => x.MountItemNum).ToList());
			Assert.AreEqual(2,listGuides.Count);
			Assert.AreEqual(countBefore+2,CountRows("mountitemguide"));
			MountItemGuide guide0=listGuides.First(x => x.MountItemNum==listItems[0].MountItemNum);
			Assert.AreEqual("Upper occlusal",guide0.GuideLabel);
			Assert.IsTrue(guide0.IsMirror);
			Assert.AreEqual(MountSlotKind.Intraoral,guide0.SlotKind);
			Assert.IsNull(MountItemGuides.GetForMountItem(listItems[1].MountItemNum),"The slot whose template slot has no guide gets no row.");
			MountItemGuide guide2=listGuides.First(x => x.MountItemNum==listItems[2].MountItemNum);
			Assert.AreEqual("Smile",guide2.GuideLabel);
			Assert.IsFalse(guide2.IsMirror);
			Assert.AreEqual(MountSlotKind.Extraoral,guide2.SlotKind);
			//The copy is the mount's own: changing the template afterwards does not change it.
			MountItemDefGuides.Upsert(MakeDefGuide(listItemDefs[0].MountItemDefNum,"Changed"));
			Assert.AreEqual("Upper occlusal",MountItemGuides.GetForMountItem(listItems[0].MountItemNum).GuideLabel);
			//A template without any guide makes a mount without any.
			MountDef mountDefPlain=MakeDef("guide def 4",2,out List<MountItemDef> listItemDefsPlain);
			Mount mountPlain=Mounts.CreateMountFromDef(mountDefPlain,patient.PatNum,ImagingDb.GetImageCategory());
			Assert.AreEqual(0,MountItemGuides.GetForMountItems(MountItems.GetItemsForMount(mountPlain.MountNum).Select(x => x.MountItemNum).ToList()).Count);
			Mounts.Delete(mount);
			Mounts.Delete(mountPlain);
			MountDefs.Delete(mountDef.MountDefNum);
			MountDefs.Delete(mountDefPlain.MountDefNum);
		}
		#endregion CreateMountFromDef

		#region Deletes
		[TestMethod]
		public void CopyForMountItem_APastedMountKeepsItsGuides() {
			Patient patient=ImagingDb.CreatePatient("mg paste");
			MountDef mountDef=MakeDef("guide def paste",2,out List<MountItemDef> listItemDefs);
			Mount mount=Mounts.CreateMountFromDef(mountDef,patient.PatNum,ImagingDb.GetImageCategory());
			List<MountItem> listItems=MountItems.GetItemsForMount(mount.MountNum);
			MountItemGuides.Upsert(new MountItemGuide { MountItemNum=listItems[0].MountItemNum,GuideLabel="Smile",IsMirror=true,SlotKind=MountSlotKind.Extraoral });
			//the paste in ControlImageDisplay: a new mount, a copy of each item, then the guide
			Mount mountCopy=mount.Copy();
			Mounts.Insert(mountCopy);
			List<long> listCopyNums=new List<long>();
			foreach(MountItem mountItemOrig in listItems) {
				MountItem mountItem=mountItemOrig.Copy();
				mountItem.MountNum=mountCopy.MountNum;
				MountItems.Insert(mountItem);
				MountItemGuides.CopyForMountItem(mountItemOrig.MountItemNum,mountItem.MountItemNum);
				listCopyNums.Add(mountItem.MountItemNum);
			}
			MountItemGuide guide=MountItemGuides.GetForMountItem(listCopyNums[0]);
			Assert.IsNotNull(guide);
			Assert.AreEqual("Smile",guide.GuideLabel);
			Assert.IsTrue(guide.IsMirror);
			Assert.AreEqual(MountSlotKind.Extraoral,guide.SlotKind);
			Assert.IsNull(MountItemGuides.GetForMountItem(listCopyNums[1]),"A slot without a guide gets none.");
			Assert.IsNotNull(MountItemGuides.GetForMountItem(listItems[0].MountItemNum),"The original keeps its own.");
			Mounts.Delete(mountCopy);
			Mounts.Delete(mount);
			MountDefs.Delete(mountDef.MountDefNum);
		}

		[TestMethod]
		public void MountItemsDelete_AlsoDeletesTheGuideRow() {
			Patient patient=ImagingDb.CreatePatient("mg3");
			MountDef mountDef=MakeDef("guide def 5",2,out List<MountItemDef> listItemDefs);
			MountItemDefGuides.Upsert(MakeDefGuide(listItemDefs[0].MountItemDefNum,"A"));
			MountItemDefGuides.Upsert(MakeDefGuide(listItemDefs[1].MountItemDefNum,"B"));
			Mount mount=Mounts.CreateMountFromDef(mountDef,patient.PatNum,ImagingDb.GetImageCategory());
			List<MountItem> listItems=MountItems.GetItemsForMount(mount.MountNum);
			MountItems.Delete(listItems[0]);
			Assert.IsNull(MountItemGuides.GetForMountItem(listItems[0].MountItemNum));
			Assert.IsNotNull(MountItemGuides.GetForMountItem(listItems[1].MountItemNum),"The other item's row stays.");
			Assert.AreEqual(2,CountRows("mountitemdefguide WHERE MountItemDefNum IN("+listItemDefs[0].MountItemDefNum+","+listItemDefs[1].MountItemDefNum+")"),
				"The template's rows stay.");
			Mounts.Delete(mount);
			MountDefs.Delete(mountDef.MountDefNum);
		}

		[TestMethod]
		public void MountsDelete_LeavesNoGuideRowOfItsItems() {
			Patient patient=ImagingDb.CreatePatient("mg4");
			MountDef mountDef=MakeDef("guide def 6",2,out List<MountItemDef> listItemDefs);
			MountItemDefGuides.Upsert(MakeDefGuide(listItemDefs[0].MountItemDefNum,"A"));
			MountItemDefGuides.Upsert(MakeDefGuide(listItemDefs[1].MountItemDefNum,"B"));
			Mount mount=Mounts.CreateMountFromDef(mountDef,patient.PatNum,ImagingDb.GetImageCategory());
			Mount mountOther=Mounts.CreateMountFromDef(mountDef,patient.PatNum,ImagingDb.GetImageCategory());
			List<long> listNumsItems=MountItems.GetItemsForMount(mount.MountNum).Select(x => x.MountItemNum).ToList();
			List<long> listNumsItemsOther=MountItems.GetItemsForMount(mountOther.MountNum).Select(x => x.MountItemNum).ToList();
			Assert.AreEqual(2,MountItemGuides.GetForMountItems(listNumsItems).Count);
			Mounts.Delete(mount);
			Assert.AreEqual(0,CountRows("mountitem WHERE MountNum="+mount.MountNum));
			Assert.AreEqual(0,MountItemGuides.GetForMountItems(listNumsItems).Count);
			Assert.AreEqual(2,MountItemGuides.GetForMountItems(listNumsItemsOther).Count,"Another mount's rows stay.");
			Mounts.Delete(mountOther);
			Assert.AreEqual(0,MountItemGuides.GetForMountItems(listNumsItemsOther).Count);
			MountDefs.Delete(mountDef.MountDefNum);
		}

		[TestMethod]
		public void MountItemDefsDelete_AlsoDeletesTheGuideRow() {
			MountDef mountDef=MakeDef("guide def 7",3,out List<MountItemDef> listItemDefs);
			foreach(MountItemDef mountItemDef in listItemDefs) {
				MountItemDefGuides.Upsert(MakeDefGuide(mountItemDef.MountItemDefNum,"slot "+mountItemDef.ItemOrder));
			}
			MountItemDefs.Delete(listItemDefs[0].MountItemDefNum);
			Assert.IsNull(MountItemDefGuides.GetForMountItemDef(listItemDefs[0].MountItemDefNum));
			Assert.IsNotNull(MountItemDefGuides.GetForMountItemDef(listItemDefs[1].MountItemDefNum));
			//All the slots of one template.
			MountItemDefs.DeleteForMount(mountDef.MountDefNum);
			Assert.AreEqual(0,MountItemDefGuides.GetForMountItemDefs(listItemDefs.Select(x => x.MountItemDefNum).ToList()).Count);
			MountDefs.Delete(mountDef.MountDefNum);
		}

		[TestMethod]
		public void MountDefsDelete_LeavesNoGuideRowOfItsSlots() {
			MountDef mountDef=MakeDef("guide def 8",2,out List<MountItemDef> listItemDefs);
			MountDef mountDefOther=MakeDef("guide def 9",1,out List<MountItemDef> listItemDefsOther);
			MountItemDefGuides.Upsert(MakeDefGuide(listItemDefs[0].MountItemDefNum,"A"));
			MountItemDefGuides.Upsert(MakeDefGuide(listItemDefs[1].MountItemDefNum,"B"));
			MountItemDefGuides.Upsert(MakeDefGuide(listItemDefsOther[0].MountItemDefNum,"C"));
			MountDefs.Delete(mountDef.MountDefNum);
			Assert.AreEqual(0,CountRows("mountitemdef WHERE MountDefNum="+mountDef.MountDefNum));
			Assert.AreEqual(0,MountItemDefGuides.GetForMountItemDefs(listItemDefs.Select(x => x.MountItemDefNum).ToList()).Count);
			Assert.IsNotNull(MountItemDefGuides.GetForMountItemDef(listItemDefsOther[0].MountItemDefNum),"Another template's row stays.");
			MountDefs.Delete(mountDefOther.MountDefNum);
			Assert.IsNull(MountItemDefGuides.GetForMountItemDef(listItemDefsOther[0].MountItemDefNum));
		}
		#endregion Deletes
	}
}
