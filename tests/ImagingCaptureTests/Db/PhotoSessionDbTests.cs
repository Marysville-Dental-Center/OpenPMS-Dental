// IMG-P4b: the camera session's store on the scratch database (OdPhotoStore): the photo mount definitions, a new photo mount with its
// "Mount Created" entry, its photo slots, and a photo saved into a mirror slot (flipped, in its slot, in the mount's category).
// Synthetic data only. Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ImagingCapture.Photo;
using ImagingCapture.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ImagingCapture.Tests.Db {
	[TestClass]
	public class PhotoSessionDbTests {
		[TestInitialize]
		public void Init() {
			CaptureDb.Begin();
		}

		///<summary>A photo mount definition from the Intraoral 5 template under a name of its own, with its slots' guides, as the installer writes them.
		///</summary>
		private static MountDef CreatePhotoMountDef(long categoryDefNum,string tag) {
			StandardTemplate template=StandardMountTemplates.Get(StandardTemplateKind.PhotoIntraoral5);
			OdStandardTemplateStore store=new OdStandardTemplateStore();
			MountDef mountDef=StandardTemplatePlanner.BuildMountDef(template,categoryDefNum,itemOrder:0);
			mountDef.Description=template.Description+" "+tag+" "+CaptureDb.NewSuffix();
			mountDef.MountDefNum=store.InsertMountDef(mountDef);
			List<MountItemDef> listMountItemDefs=StandardTemplatePlanner.BuildMountItemDefs(template,mountDef.MountDefNum);
			for(int i=0;i<listMountItemDefs.Count;i++) {
				long mountItemDefNum=store.InsertMountItemDef(listMountItemDefs[i]);
				if(i<template.ListSlots.Count) {
					StandardTemplateSlot slot=template.ListSlots[i];
					store.InsertMountItemDefGuide(new MountItemDefGuide { MountItemDefNum=mountItemDefNum,GuideLabel=slot.GuideLabel,IsMirror=slot.IsMirror,
						SlotKind=slot.SlotKind });
				}
			}
			Cache.Refresh(InvalidType.ToolButsAndMounts);
			return MountDefs.GetDeepCopy().Single(x => x.MountDefNum==mountDef.MountDefNum);
		}

		[TestMethod]
		public void PhotoMountDefs_ListsPhotoMountsOnly() {
			Def category=CaptureDb.CreateImageCategory("photo defs");
			MountDef mountDefPhoto=CreatePhotoMountDef(category.DefNum,"list");
			MountDef mountDefXray=CaptureDb.CreateMountDefFromTemplate(StandardTemplateKind.FourBitewingsHorizontal,category.DefNum,"list");
			List<MountDef> listMountDefs=new OdPhotoStore().GetPhotoMountDefs();
			Assert.IsTrue(listMountDefs.Any(x => x.MountDefNum==mountDefPhoto.MountDefNum));
			Assert.IsFalse(listMountDefs.Any(x => x.MountDefNum==mountDefXray.MountDefNum),"An X-ray mount is never offered to the camera.");
			MountDefs.Delete(mountDefPhoto.MountDefNum);
			MountDefs.Delete(mountDefXray.MountDefNum);
		}

		[TestMethod]
		public void CreateMount_Slots_SaveIntoTheMirrorSlot() {
			Def category=CaptureDb.CreateImageCategory("photo session");
			MountDef mountDef=CreatePhotoMountDef(category.DefNum,"session");
			Patient patient=CaptureDb.CreatePatient("photo",CaptureDb.CreateProvider("photo"));
			OdPhotoStore store=new OdPhotoStore();
			Mount mount=store.CreateMount(mountDef,patient.PatNum,category.DefNum);
			Assert.AreEqual(1,CaptureDb.GetLogsForPatient(EnumPermType.ImageCreate,patient.PatNum).Count(x => x.LogText.StartsWith("Mount Created: ")));
			PhotoMountState photoMountState=store.GetMountState(mount.MountNum);
			CollectionAssert.AreEqual(new[] { "Frontal","Right buccal","Left buccal","Upper occlusal","Lower occlusal" },
				photoMountState.ListSlots.Select(x => x.GuideLabel).ToArray());
			PhotoSessionModel model=new PhotoSessionModel(photoMountState.ListSlots,photoMountState.Mount.FlipOnAcquire);
			Assert.IsTrue(model.SelectSlot(3));
			PhotoShotPlan plan=model.PlanShot();
			Bitmap bitmap=new Bitmap(40,30);
			PhotoSaveResult result=PhotoSaver.Save(store,bitmap,plan,patient.PatNum,photoMountState.Mount.DocCategory);
			Assert.AreEqual("",result.Error);
			Document document=Documents.GetByNum(result.Document.DocNum);
			Assert.AreEqual(plan.MountItemNum,document.MountItemNum,"In its slot from the start.");
			Assert.IsTrue(document.IsFlipped,"The upper occlusal is a mirror shot.");
			Assert.AreEqual(category.DefNum,document.DocCategory);
			Assert.AreEqual(ImageType.Photo,document.ImgType);
			PhotoMountState photoMountStateAfter=store.GetMountState(mount.MountNum);
			CollectionAssert.AreEqual(new[] { false,false,false,true,false },photoMountStateAfter.ListSlots.Select(x => x.IsFilled).ToArray());
			Assert.IsNull(store.GetMountState(mount.MountNum+1000000));
		}
	}
}
