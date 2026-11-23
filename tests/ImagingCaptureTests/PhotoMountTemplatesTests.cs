// IMG-P4a: the photo mount templates of the camera session (Intraoral 5, Ortho 8, Before and after) and their install with each slot's guide.
// No database: FakeStandardTemplateStore. Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ImagingCapture.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ImagingCapture.Tests {
	[TestClass]
	public class PhotoMountTemplatesTests {
		[TestMethod]
		public void PhotoTemplates_ThreeOfThem_SeparateFromTheXrayOnes() {
			CollectionAssert.AreEqual(new[] { "Intraoral photos 5","Ortho photos 8","Before and after photos" },
				StandardMountTemplates.ListPhotoTemplates.Select(x => x.Description).ToArray());
			Assert.IsTrue(StandardMountTemplates.ListPhotoTemplates.All(x => x.IsPhoto));
			Assert.IsTrue(StandardMountTemplates.ListTemplates.All(x => !x.IsPhoto),"The X-ray list is unchanged.");
			Assert.AreEqual(4,StandardMountTemplates.ListTemplates.Count);
		}

		[TestMethod]
		public void PhotoTemplates_SlotsInsideTheMount_NoOverlap_OrderedGuides() {
			foreach(StandardTemplate template in StandardMountTemplates.ListPhotoTemplates) {
				List<Rectangle> listRectangles=template.ListSlots.Select(x => new Rectangle(x.Xpos,x.Ypos,x.Width,x.Height)).ToList();
				if(template.Text!=null) {
					listRectangles.Add(new Rectangle(template.Text.Xpos,template.Text.Ypos,template.Text.Width,template.Text.Height));
				}
				Rectangle rectangleMount=new Rectangle(0,0,template.Width,template.Height);
				for(int i=0;i<listRectangles.Count;i++) {
					Assert.IsTrue(rectangleMount.Contains(listRectangles[i]),template.Description+": rectangle "+i+" inside the mount");
					for(int j=i+1;j<listRectangles.Count;j++) {
						Assert.IsFalse(listRectangles[i].IntersectsWith(listRectangles[j]),template.Description+": "+i+" and "+j+" overlap");
					}
				}
				CollectionAssert.AreEqual(Enumerable.Range(1,template.ListSlots.Count).ToList(),template.ListSlots.Select(x => x.ItemOrder).ToList());
				Assert.IsTrue(template.ListSlots.All(x => x.GuideLabel!="" && x.SlotKind!=MountSlotKind.Radiograph),template.Description+": every slot is guided");
				Assert.AreEqual("",template.ScaleValue,"No scale on photos.");
			}
		}

		[TestMethod]
		public void Intraoral5_TheShootingOrder_AndTheMirrorShots() {
			StandardTemplate template=StandardMountTemplates.Get(StandardTemplateKind.PhotoIntraoral5);
			CollectionAssert.AreEqual(new[] { "Frontal","Right buccal","Left buccal","Upper occlusal","Lower occlusal" },template.ListSlots.Select(x => x.GuideLabel).ToArray());
			CollectionAssert.AreEqual(new[] { false,false,false,true,true },template.ListSlots.Select(x => x.IsMirror).ToArray(),"The occlusal views are mirror shots.");
			StandardTemplate templateOrtho=StandardMountTemplates.Get(StandardTemplateKind.PhotoOrtho8);
			Assert.AreEqual(3,templateOrtho.ListSlots.Count(x => x.SlotKind==MountSlotKind.Extraoral));
			Assert.IsTrue(templateOrtho.ListSlots.Where(x => x.SlotKind==MountSlotKind.Extraoral).All(x => x.Height>x.Width),"Face photos are portrait.");
		}

		[TestMethod]
		public void Install_PhotoTemplates_IntoThePhotoCategory_WithEachSlotsGuide() {
			FakeStandardTemplateStore store=new FakeStandardTemplateStore();
			store.HashSetCategories.Add(30);
			StandardTemplateResult result=new StandardTemplateInstaller(store).Install(0,"",30,"Photos");
			Assert.IsTrue(result.IsDone,result.Error);
			CollectionAssert.AreEqual(new[] { "Intraoral photos 5","Ortho photos 8","Before and after photos" },result.ListAdded.Select(x => x.Description).ToArray(),
				"Photo category only: no X-ray template.");
			Assert.IsTrue(store.ListMountDefs.All(x => x.DefaultCat==30));
			Assert.AreEqual(5+8+2,store.ListMountItemDefGuides.Count,"One guide per photo slot; none for the text items.");
			MountItemDefGuide guide=store.ListMountItemDefGuides.First(x => x.GuideLabel=="Upper occlusal");
			Assert.IsTrue(guide.IsMirror);
			Assert.AreEqual(MountSlotKind.Intraoral,guide.SlotKind);
			MountItemDef mountItemDef=store.ListMountItemDefs.First(x => x.MountItemDefNum==guide.MountItemDefNum);
			Assert.AreEqual(4,mountItemDef.ItemOrder,"The guide is on its own slot.");
		}

		[TestMethod]
		public void Install_BothCategories_AddsBoth_XrayAsBefore() {
			FakeStandardTemplateStore store=new FakeStandardTemplateStore();
			store.HashSetCategories.Add(30);
			StandardTemplateResult result=new StandardTemplateInstaller(store).Install(20,"Radiographs",30,"Photos");
			Assert.IsTrue(result.IsDone,result.Error);
			Assert.AreEqual(7,result.ListAdded.Count);
			Assert.IsTrue(store.ListMountDefs.Where(x => x.Description.Contains("photos")).All(x => x.DefaultCat==30));
			Assert.IsTrue(store.ListMountDefs.Where(x => !x.Description.Contains("photos")).All(x => x.DefaultCat==20));
			StandardTemplateResult resultXrayOnly=new StandardTemplateInstaller(new FakeStandardTemplateStore()).Install(20,"Radiographs");
			Assert.AreEqual(4,resultXrayOnly.ListAdded.Count,"Without a photo category, as before.");
			Assert.AreEqual(0,store.ListMountItemDefGuides.Count(x => x.SlotKind==MountSlotKind.Radiograph),"X-ray slots get no guide.");
		}

		[TestMethod]
		public void Install_ABadPhotoCategory_AddsNothing() {
			FakeStandardTemplateStore store=new FakeStandardTemplateStore();
			StandardTemplateResult result=new StandardTemplateInstaller(store).Install(20,"Radiographs",99,"Missing");
			Assert.IsFalse(result.IsDone);
			Assert.AreEqual(0,store.ListMountDefs.Count);
		}
	}
}
