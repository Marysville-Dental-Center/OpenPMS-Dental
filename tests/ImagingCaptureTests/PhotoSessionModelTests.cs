// IMG-P4b: the camera session's logic (PhotoSessionModel) and its saver (PhotoSaver) without a database or a camera.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ImagingCapture.Photo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ImagingCapture.Tests {
	[TestClass]
	public class PhotoSessionModelTests {
		///<summary>The Intraoral 5 slots: frontal, right buccal, left buccal, upper occlusal (mirror), lower occlusal (mirror).</summary>
		private static List<PhotoSlot> Intraoral5() {
			string[] arrayLabels={ "Frontal","Right buccal","Left buccal","Upper occlusal","Lower occlusal" };
			return arrayLabels.Select((x,i) => new PhotoSlot { MountItemNum=100+i,ItemOrder=i+1,GuideLabel=x,IsMirror=i>=3,SlotKind=MountSlotKind.Intraoral,
				ToothNumbers=i==3 ? "2,3,4" : "",Width=1600,Height=1200 }).ToList();
		}

		[TestMethod]
		public void Mount_FillsInItemOrder_AndEndsComplete() {
			List<PhotoSlot> listSlots=Intraoral5();
			listSlots.Reverse();//any order in: sorted by ItemOrder
			PhotoSessionModel model=new PhotoSessionModel(listSlots,isFlipOnAcquire:false);
			List<string> listTaken=new List<string>();
			while(!model.IsComplete) {
				PhotoShotPlan plan=model.PlanShot();
				listTaken.Add(plan.GuideLabel);
				model.MarkSaved(plan);
			}
			CollectionAssert.AreEqual(new[] { "Frontal","Right buccal","Left buccal","Upper occlusal","Lower occlusal" },listTaken);
			Assert.IsNull(model.PlanShot(),"Nothing more to take.");
			Assert.AreEqual("5 of 5 taken",model.GetProgressText());
			Assert.AreEqual(5,model.CountSaved);
		}

		[TestMethod]
		public void Plan_MirrorShotIsFlipped_RotationAndTeethFromTheSlot() {
			List<PhotoSlot> listSlots=Intraoral5();
			listSlots[3].RotateOnAcquire=180;
			PhotoSessionModel model=new PhotoSessionModel(listSlots,isFlipOnAcquire:false);
			Assert.IsTrue(model.SelectSlot(3));
			PhotoShotPlan plan=model.PlanShot();
			Assert.AreEqual(103,plan.MountItemNum);
			Assert.IsTrue(plan.IsFlipped);
			Assert.AreEqual(180,plan.DegreesRotated);
			Assert.AreEqual("2,3,4",plan.ToothNumbers);
			Assert.AreEqual("Next: Upper occlusal (mirror shot, saved flipped, rotated 180 degrees, teeth 2-4)",model.GetGuideText(x => "2-4"));
			Assert.IsTrue(model.SelectSlot(0));
			Assert.IsFalse(model.PlanShot().IsFlipped);
			Assert.IsFalse(model.IsNextFlipped);
			PhotoSessionModel modelFlip=new PhotoSessionModel(Intraoral5(),isFlipOnAcquire:true);
			Assert.IsTrue(modelFlip.PlanShot().IsFlipped,"A mount that flips on acquire flips every photo.");
			Assert.IsTrue(modelFlip.IsNextFlipped,"The live picture shows it flipped too.");
			Assert.AreEqual("Next: Frontal (saved flipped, as the mount flips on acquire)",modelFlip.GetGuideText());
		}

		[TestMethod]
		public void SkipAndSelect_OnlyEmptySlots_Wrapping() {
			List<PhotoSlot> listSlots=Intraoral5();
			listSlots[1].IsFilled=true;//right buccal taken earlier today
			PhotoSessionModel model=new PhotoSessionModel(listSlots,isFlipOnAcquire:false);
			Assert.AreEqual(0,model.IdxNext);
			Assert.IsTrue(model.SkipForward());
			Assert.AreEqual(2,model.IdxNext,"The filled slot is passed over.");
			Assert.IsTrue(model.SkipBack());
			Assert.AreEqual(0,model.IdxNext);
			Assert.IsTrue(model.SkipBack());
			Assert.AreEqual(4,model.IdxNext,"Back from the first wraps to the last.");
			Assert.IsTrue(model.SkipForward());
			Assert.AreEqual(0,model.IdxNext,"Forward from the last wraps to the first.");
			Assert.IsFalse(model.SelectSlot(1),"A filled slot cannot be chosen.");
			Assert.IsFalse(model.SelectSlot(9));
			//skipped slots stay empty: after the last one, the next empty one is the skipped first
			Assert.IsTrue(model.SelectSlot(4));
			model.MarkSaved(model.PlanShot());
			Assert.AreEqual(0,model.IdxNext);
		}

		[TestMethod]
		public void OneEmptySlotLeft_SkipSaysNo() {
			List<PhotoSlot> listSlots=Intraoral5();
			for(int i=0;i<4;i++) {
				listSlots[i].IsFilled=true;
			}
			PhotoSessionModel model=new PhotoSessionModel(listSlots,isFlipOnAcquire:false);
			Assert.AreEqual(4,model.IdxNext);
			Assert.IsFalse(model.SkipForward());
			Assert.IsFalse(model.SkipBack());
			Assert.AreEqual("4 of 5 taken",model.GetProgressText());
		}

		[TestMethod]
		public void QuickPhotos_EachUnmounted() {
			PhotoSessionModel model=PhotoSessionModel.CreateQuickPhotos();
			Assert.IsTrue(model.IsQuickPhotos);
			Assert.IsFalse(model.IsComplete);
			PhotoShotPlan plan=model.PlanShot("30");
			Assert.AreEqual(0,plan.MountItemNum);
			Assert.AreEqual("30",plan.ToothNumbers);
			Assert.IsFalse(plan.IsFlipped);
			model.MarkSaved(plan);
			model.MarkSaved(model.PlanShot());
			Assert.AreEqual("2 photos saved",model.GetProgressText());
			Assert.IsFalse(model.SkipForward());
		}

		[TestMethod]
		public void BuildSlots_PhotoKindsOnly_FilledFromTheDocuments() {
			List<MountItem> listMountItems=new List<MountItem> {
				new MountItem { MountItemNum=1,ItemOrder=0,TextShowing="Patient" },//text item
				new MountItem { MountItemNum=2,ItemOrder=1,ToothNumbers="8,9" },
				new MountItem { MountItemNum=3,ItemOrder=2 },
				new MountItem { MountItemNum=4,ItemOrder=3 },//no guide: a radiograph slot
				new MountItem { MountItemNum=5,ItemOrder=-1 },//unmounted
			};
			List<MountItemGuide> listGuides=new List<MountItemGuide> {
				new MountItemGuide { MountItemNum=2,GuideLabel=" Frontal ",SlotKind=MountSlotKind.Intraoral },
				new MountItemGuide { MountItemNum=3,GuideLabel="Profile",SlotKind=MountSlotKind.Extraoral },
				new MountItemGuide { MountItemNum=5,GuideLabel="x",SlotKind=MountSlotKind.Intraoral },
			};
			List<Document> listDocuments=new List<Document> { new Document { MountItemNum=3 },null };
			List<PhotoSlot> listSlots=PhotoSessionModel.BuildSlots(listMountItems,listGuides,listDocuments);
			CollectionAssert.AreEqual(new long[] { 2,3 },listSlots.Select(x => x.MountItemNum).ToArray());
			Assert.AreEqual("Frontal",listSlots[0].GuideLabel);
			Assert.AreEqual("8,9",listSlots[0].ToothNumbers);
			Assert.IsFalse(listSlots[0].IsFilled);
			Assert.IsTrue(listSlots[1].IsFilled);
			Assert.IsTrue(PhotoSessionModel.IsPhotoMount(listGuides));
			Assert.IsFalse(PhotoSessionModel.IsPhotoMount(new[] { new MountItemGuide { SlotKind=MountSlotKind.Radiograph } }));
			Assert.IsFalse(PhotoSessionModel.IsPhotoMount(null));
			PhotoSessionModel model=new PhotoSessionModel(new List<PhotoSlot> { new PhotoSlot { ItemOrder=1,SlotKind=MountSlotKind.Radiograph } },false);
			Assert.IsTrue(model.IsComplete,"Never an X-ray slot: a mount of radiograph slots has nothing to fill.");
		}

		private sealed class FakePhotoStore:IPhotoStore {
			public bool IsImportFailing;
			public bool IsUpdateFailing;
			public readonly List<Document> ListImported=new List<Document>();
			public readonly List<Document> ListUpdated=new List<Document>();
			public long CategoryLast;

			public List<MountDef> GetPhotoMountDefs() {
				return new List<MountDef>();
			}

			public Mount CreateMount(MountDef mountDef,long patNum,long categoryDefNum) {
				throw new NotImplementedException();
			}

			public PhotoMountState GetMountState(long mountNum) {
				return null;
			}

			public Document ImportPhoto(Bitmap bitmap,long categoryDefNum,long patNum) {
				if(IsImportFailing) {
					throw new Exception("The A to Z folder is not reachable.");
				}
				CategoryLast=categoryDefNum;
				Document document=new Document { DocNum=ListImported.Count+1,PatNum=patNum,DocCategory=categoryDefNum };
				ListImported.Add(document);
				return document;
			}

			public void UpdateDocument(Document document,Document documentOld) {
				if(IsUpdateFailing) {
					throw new Exception("Lost the database.");
				}
				ListUpdated.Add(document.Copy());
			}
		}

		[TestMethod]
		public void Saver_PlacesThePhotoInItsSlot_FlippedRotatedWithTeeth() {
			FakePhotoStore store=new FakePhotoStore();
			Bitmap bitmap=new Bitmap(8,6);
			PhotoShotPlan plan=new PhotoShotPlan { MountItemNum=103,IdxSlot=3,IsFlipped=true,DegreesRotated=180,ToothNumbers="2,3" };
			PhotoSaveResult result=PhotoSaver.Save(store,bitmap,plan,patNum:7,categoryDefNum:55);
			Assert.AreEqual("",result.Error);
			Assert.IsTrue(result.IsPlaced);
			Assert.AreEqual(55,store.CategoryLast);
			Document document=store.ListUpdated.Single();
			Assert.AreEqual(103,document.MountItemNum);
			Assert.IsTrue(document.IsFlipped);
			Assert.AreEqual(180,document.DegreesRotated);
			Assert.AreEqual("2,3",document.ToothNumbers);
		}

		[TestMethod]
		public void Saver_QuickPhoto_NoUpdate_AndFailures() {
			FakePhotoStore store=new FakePhotoStore();
			Bitmap bitmap=new Bitmap(8,6);
			PhotoSaveResult result=PhotoSaver.Save(store,bitmap,new PhotoShotPlan(),7,55);
			Assert.IsTrue(result.IsPlaced);
			Assert.AreEqual(0,store.ListUpdated.Count,"A quick photo with nothing to set is not updated.");
			store.IsImportFailing=true;
			result=PhotoSaver.Save(store,bitmap,new PhotoShotPlan { MountItemNum=3 },7,55);
			Assert.IsNull(result.Document);
			StringAssert.Contains(result.Error,"could not be saved");
			store.IsImportFailing=false;
			store.IsUpdateFailing=true;
			result=PhotoSaver.Save(store,bitmap,new PhotoShotPlan { MountItemNum=3 },7,55);
			Assert.IsNotNull(result.Document,"The photo is saved even when it cannot be placed.");
			Assert.IsFalse(result.IsPlaced);
			StringAssert.Contains(result.Error,"not placed");
			Assert.AreEqual("No photo to save.",PhotoSaver.Save(store,null,new PhotoShotPlan(),7,55).Error);
		}

		[TestMethod]
		public void Categories_AsTheVideoWindowAndMountAcquire() {
			Assert.AreEqual(9,PhotoSaver.GetQuickPhotoCategory(prefVideoCategory:9,categoryDefNumCurrent:4));
			Assert.AreEqual(4,PhotoSaver.GetQuickPhotoCategory(0,4));
			Assert.AreEqual(12,PhotoSaver.GetMountCategory(new MountDef { DefaultCat=12 },4));
			Assert.AreEqual(4,PhotoSaver.GetMountCategory(new MountDef { DefaultCat=0 },4));
			Assert.AreEqual(4,PhotoSaver.GetMountCategory(null,4));
		}
	}
}
