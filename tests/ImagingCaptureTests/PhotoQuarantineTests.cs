// IMG-P4b-3: photos a camera session could not save, kept on this computer (PhotoQuarantine) and filed later to their patient: the files,
// the sidecar, and filing into the slot when it is still empty, else unmounted. A temporary folder and a store of its own; no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ImagingCapture.Photo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ImagingCapture.Tests {
	[TestClass]
	public class PhotoQuarantineTests {
		private string _folder;

		[TestInitialize]
		public void Init() {
			_folder=Path.Combine(Path.GetTempPath(),"opms-photo-unsaved-"+Guid.NewGuid().ToString("N"));
		}

		[TestCleanup]
		public void Cleanup() {
			if(Directory.Exists(_folder)) {
				Directory.Delete(_folder,true);
			}
		}

		private static PhotoUnsaved MakeRecord(long mountItemNum) {
			return new PhotoUnsaved { PatNum=7,PatientName="Test, Sensor",MountNum=50,MountItemNum=mountItemNum,Slot="Upper occlusal",IsFlipped=true,
				DegreesRotated=180,ToothNumbers="2,3",CategoryDefNum=12,DateTimeTaken=new DateTime(2026,11,23,10,14,5,120),
				Reason="The photo could not be saved: the A to Z folder is not reachable." };
		}

		private sealed class Store:IPhotoStore {
			public PhotoMountState PhotoMountState;
			public readonly List<Document> ListImported=new List<Document>();
			public readonly List<Document> ListUpdated=new List<Document>();
			public bool IsImportFailing;

			public List<MountDef> GetPhotoMountDefs() {
				return new List<MountDef>();
			}

			public Mount CreateMount(MountDef mountDef,long patNum,long categoryDefNum) {
				throw new NotSupportedException();
			}

			public PhotoMountState GetMountState(long mountNum) {
				return PhotoMountState;
			}

			public Document ImportPhoto(Bitmap bitmap,long categoryDefNum,long patNum) {
				if(IsImportFailing) {
					throw new Exception("still not reachable");
				}
				Document document=new Document { DocNum=ListImported.Count+1,PatNum=patNum,DocCategory=categoryDefNum };
				ListImported.Add(document);
				return document;
			}

			public void UpdateDocument(Document document,Document documentOld) {
				ListUpdated.Add(document.Copy());
			}

			public Document GetDocument(long docNum) {
				return null;
			}

			public void MoveToRetake(Document document,long mountNum,string slotLabel,long patNum) {
				throw new NotSupportedException();
			}

			public void DeleteDocument(Document document,long patNum) {
				throw new NotSupportedException();
			}

			public void LogEdit(long patNum,string logText,long docNum) {
			}
		}

		[TestMethod]
		public void Write_ThePhotoAndItsSidecar_ListedBack() {
			string error;
			string path=PhotoQuarantine.WriteToFolder(_folder,new Bitmap(16,12),MakeRecord(103),out error);
			Assert.AreEqual("",error);
			Assert.IsTrue(File.Exists(path));
			StringAssert.EndsWith(path,"20261123-101405-120_P7.jpg");
			string path2=PhotoQuarantine.WriteToFolder(_folder,new Bitmap(16,12),MakeRecord(104),out error);
			StringAssert.EndsWith(path2,"_P7_2.jpg","Two photos in the same millisecond keep both.");
			File.WriteAllText(Path.Combine(_folder,"stray.jpg"),"x");//no sidecar: not listed
			File.WriteAllText(Path.Combine(_folder,"other.txt"),"not ours");
			List<PhotoUnsaved> listPhotos=PhotoQuarantine.List(_folder);
			Assert.AreEqual(2,listPhotos.Count);
			PhotoUnsaved photoUnsaved=listPhotos[0];
			Assert.AreEqual(7,photoUnsaved.PatNum);
			Assert.AreEqual("Test, Sensor",photoUnsaved.PatientName);
			Assert.AreEqual(50,photoUnsaved.MountNum);
			Assert.AreEqual(103,photoUnsaved.MountItemNum);
			Assert.AreEqual("Upper occlusal",photoUnsaved.Slot);
			Assert.IsTrue(photoUnsaved.IsFlipped);
			Assert.AreEqual(180,photoUnsaved.DegreesRotated);
			Assert.AreEqual("2,3",photoUnsaved.ToothNumbers);
			Assert.AreEqual(12,photoUnsaved.CategoryDefNum);
			Assert.AreEqual(new DateTime(2026,11,23,10,14,5),photoUnsaved.DateTimeTaken);
			Assert.AreEqual(0,PhotoQuarantine.List(Path.Combine(_folder,"missing")).Count);
			Assert.IsNull(PhotoQuarantine.ParseSidecar("PatNum=7",path),"Only the session's own sidecars.");
			Assert.IsNull(PhotoQuarantine.ParseSidecar(PhotoQuarantine.BuildSidecar(new PhotoUnsaved()),path),"A sidecar without a patient is not filed.");
		}

		[TestMethod]
		public void File_IntoTheEmptySlot_ThenTheFilesGo() {
			string error;
			PhotoQuarantine.WriteToFolder(_folder,new Bitmap(16,12),MakeRecord(103),out error);
			PhotoUnsaved photoUnsaved=PhotoQuarantine.List(_folder).Single();
			Store store=new Store { PhotoMountState=new PhotoMountState { Mount=new Mount { MountNum=50 },
				ListSlots=new List<PhotoSlot> { new PhotoSlot { MountItemNum=103,ItemOrder=4,SlotKind=MountSlotKind.Intraoral,IsFilled=false } } } };
			Assert.AreEqual("",PhotoQuarantine.FileOne(store,photoUnsaved));
			Document document=store.ListUpdated.Single();
			Assert.AreEqual(103,document.MountItemNum);
			Assert.IsTrue(document.IsFlipped);
			Assert.AreEqual(180,document.DegreesRotated);
			Assert.AreEqual("2,3",document.ToothNumbers);
			Assert.AreEqual(12,store.ListImported.Single().DocCategory);
			Assert.AreEqual(7,store.ListImported.Single().PatNum,"The patient the photo was taken for.");
			Assert.AreEqual(0,PhotoQuarantine.List(_folder).Count);
			Assert.AreEqual(0,Directory.GetFiles(_folder).Length,"The photo and its sidecar are removed.");
		}

		[TestMethod]
		public void File_SlotTakenMeanwhile_Unmounted_AndAFailureKeepsTheFiles() {
			string error;
			PhotoQuarantine.WriteToFolder(_folder,new Bitmap(16,12),MakeRecord(103),out error);
			PhotoUnsaved photoUnsaved=PhotoQuarantine.List(_folder).Single();
			Store store=new Store { IsImportFailing=true,PhotoMountState=new PhotoMountState { Mount=new Mount { MountNum=50 },
				ListSlots=new List<PhotoSlot> { new PhotoSlot { MountItemNum=103,ItemOrder=4,SlotKind=MountSlotKind.Intraoral,IsFilled=true } } } };
			StringAssert.Contains(PhotoQuarantine.FileOne(store,photoUnsaved),"still not reachable");
			Assert.AreEqual(1,PhotoQuarantine.List(_folder).Count,"Not filed: the files stay.");
			store.IsImportFailing=false;
			Assert.AreEqual("",PhotoQuarantine.FileOne(store,photoUnsaved));
			Assert.AreEqual(0,store.ListUpdated.Single().MountItemNum,"The slot has a photo now: filed unmounted, flipped and rotated as taken.");
			Assert.IsTrue(store.ListUpdated.Single().IsFlipped);
			photoUnsaved.PathImage=Path.Combine(_folder,"gone.jpg");
			StringAssert.Contains(PhotoQuarantine.FileOne(store,photoUnsaved),"could not be read");
		}
	}
}
