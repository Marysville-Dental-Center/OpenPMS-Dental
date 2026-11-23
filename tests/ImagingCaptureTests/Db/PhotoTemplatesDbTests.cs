// IMG-P4a: "Add standard templates" with a photo category on the scratch database: the photo templates, each photo slot's guide row
// (mountitemdefguide), the guides copied to a patient's mount (mountitemguide), and no guide row left when the template is deleted.
// Synthetic data only. Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using ImagingCapture.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ImagingCapture.Tests.Db {
	[TestClass]
	public class PhotoTemplatesDbTests {
		[TestInitialize]
		public void Init() {
			CaptureDb.Begin();
		}

		///<summary>Removes the photo templates an earlier test of this run added, so that this test sees them added.</summary>
		private static void DeletePhotoTemplates() {
			Cache.Refresh(InvalidType.ToolButsAndMounts);
			foreach(MountDef mountDef in MountDefs.GetDeepCopy()) {
				if(StandardMountTemplates.ListPhotoTemplates.Any(x => string.Equals(x.Description,(mountDef.Description??"").Trim(),StringComparison.OrdinalIgnoreCase))) {
					MountDefs.Delete(mountDef.MountDefNum);
				}
			}
			Cache.Refresh(InvalidType.ToolButsAndMounts);
		}

		[TestMethod]
		public void Install_PhotoCategoryOnly_GuideRows_CopiedToAPatientsMount_GoneWithTheTemplate() {
			DeletePhotoTemplates();
			Def categoryPhotos=CaptureDb.CreateImageCategory("photos");
			StandardTemplateResult result=new StandardTemplateInstaller(new OdStandardTemplateStore()).Install(0,"",categoryPhotos.DefNum,categoryPhotos.ItemName);
			Assert.IsTrue(result.IsDone,result.Error);
			CollectionAssert.AreEqual(StandardMountTemplates.ListPhotoTemplates.Select(x => x.Description).ToList(),result.ListAdded.Select(x => x.Description).ToList());
			Cache.Refresh(InvalidType.ToolButsAndMounts);
			foreach(MountDef mountDefAdded in result.ListAdded) {
				StandardTemplate template=StandardMountTemplates.ListPhotoTemplates.Single(x => x.Description==mountDefAdded.Description);
				Assert.AreEqual(categoryPhotos.DefNum,mountDefAdded.DefaultCat);
				List<MountItemDef> listSlots=MountItemDefs.GetForMountDef(mountDefAdded.MountDefNum).Where(x => x.ItemOrder>0).OrderBy(x => x.ItemOrder).ToList();
				List<MountItemDefGuide> listGuides=MountItemDefGuides.GetForMountItemDefs(listSlots.Select(x => x.MountItemDefNum).ToList());
				Assert.AreEqual(template.ListSlots.Count,listGuides.Count,template.Description+": one guide per slot");
				for(int i=0;i<listSlots.Count;i++) {
					MountItemDefGuide guide=listGuides.Single(x => x.MountItemDefNum==listSlots[i].MountItemDefNum);
					Assert.AreEqual(template.ListSlots[i].GuideLabel,guide.GuideLabel);
					Assert.AreEqual(template.ListSlots[i].IsMirror,guide.IsMirror);
					Assert.AreEqual(template.ListSlots[i].SlotKind,guide.SlotKind);
				}
			}
			//A patient's mount from the intraoral template carries the guides.
			MountDef mountDefIntraoral=result.ListAdded.Single(x => x.Description==StandardMountTemplates.DescriptionPhotoIntraoral5);
			Patient patient=CaptureDb.CreatePatient("photo",CaptureDb.CreateProvider("photo"));
			Mount mount=CaptureDb.CreateMountFromDef(MountDefs.GetDeepCopy().Single(x => x.MountDefNum==mountDefIntraoral.MountDefNum),patient,categoryPhotos.DefNum);
			List<MountItem> listMountItems=MountItems.GetItemsForMount(mount.MountNum).Where(x => x.ItemOrder>0).OrderBy(x => x.ItemOrder).ToList();
			List<MountItemGuide> listMountItemGuides=MountItemGuides.GetForMountItems(listMountItems.Select(x => x.MountItemNum).ToList());
			CollectionAssert.AreEqual(new[] { "Frontal","Right buccal","Left buccal","Upper occlusal","Lower occlusal" },
				listMountItems.Select(x => listMountItemGuides.Single(y => y.MountItemNum==x.MountItemNum).GuideLabel).ToArray());
			//Deleting the template leaves no guide row of its slots; the patient's mount keeps its own.
			List<long> listMountItemDefNums=MountItemDefs.GetForMountDef(mountDefIntraoral.MountDefNum).Select(x => x.MountItemDefNum).ToList();
			MountDefs.Delete(mountDefIntraoral.MountDefNum);
			Assert.AreEqual(0,MountItemDefGuides.GetForMountItemDefs(listMountItemDefNums).Count);
			Assert.AreEqual(5,MountItemGuides.GetForMountItems(listMountItems.Select(x => x.MountItemNum).ToList()).Count);
			Mounts.Delete(mount);
			Assert.AreEqual(0,MountItemGuides.GetForMountItems(listMountItems.Select(x => x.MountItemNum).ToList()).Count);
		}
	}
}
