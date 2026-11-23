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

			public readonly List<string> ListVideosImported=new List<string>();

			public Document ImportVideo(string pathVideo,long categoryDefNum,long patNum) {
				if(IsImportFailing) {
					throw new Exception("still not reachable");
				}
				ListVideosImported.Add(pathVideo);
				Document document=new Document { DocNum=100+ListVideosImported.Count,PatNum=patNum,DocCategory=categoryDefNum,FileName="v.mp4" };
				ListImported.Add(document);
				return document;
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
			Store store=new Store { PhotoMountState=new PhotoMountState { Mount=new Mount { MountNum=50,PatNum=7 },
				ListSlots=new List<PhotoSlot> { new PhotoSlot { MountItemNum=103,ItemOrder=4,SlotKind=MountSlotKind.Intraoral,IsFilled=false } } } };
			Assert.AreEqual("",PhotoQuarantine.FileOne(store,photoUnsaved,out bool isFiled));
			Assert.IsTrue(isFiled);
			Document document=store.ListUpdated.Single();
			Assert.AreEqual(new DateTime(2026,11,23,10,14,5),document.DateCreated,"The time it was taken, not the time it was filed.");
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
			Store store=new Store { IsImportFailing=true,PhotoMountState=new PhotoMountState { Mount=new Mount { MountNum=50,PatNum=7 },
				ListSlots=new List<PhotoSlot> { new PhotoSlot { MountItemNum=103,ItemOrder=4,SlotKind=MountSlotKind.Intraoral,IsFilled=true } } } };
			StringAssert.Contains(PhotoQuarantine.FileOne(store,photoUnsaved,out bool isFiled),"still not reachable");
			Assert.IsFalse(isFiled);
			Assert.AreEqual(1,PhotoQuarantine.List(_folder).Count,"Not filed: the files stay.");
			store.IsImportFailing=false;
			Assert.AreEqual("",PhotoQuarantine.FileOne(store,photoUnsaved,out isFiled));
			Assert.AreEqual(0,store.ListUpdated.Single().MountItemNum,"The slot has a photo now: filed unmounted, flipped and rotated as taken.");
			Assert.IsTrue(store.ListUpdated.Single().IsFlipped);
			photoUnsaved.PathImage=Path.Combine(_folder,"gone.jpg");
			StringAssert.Contains(PhotoQuarantine.FileOne(store,photoUnsaved,out isFiled),"could not be read");
		}

		[TestMethod]
		public void Video_RecordingFile_MovedToUnsaved_Filed() {
			string folderRecording=Path.Combine(_folder,"recording");
			string folderUnsaved=Path.Combine(_folder,"unsaved");
			PhotoUnsaved photoUnsaved=MakeRecord(0);
			photoUnsaved.Slot="Video";
			string error;
			string path=VideoFiles.BeginRecordingFileInFolder(folderRecording,photoUnsaved,out error);
			Assert.AreEqual("",error);
			StringAssert.EndsWith(path,"_P7_video.mp4");
			Assert.IsTrue(File.Exists(Path.ChangeExtension(path,".txt")),"The sidecar is written when the recording starts.");
			File.WriteAllBytes(path,new byte[] { 0,0,0,24,(byte)'f',(byte)'t',(byte)'y',(byte)'p' });
			//the program stopped while recording: the next session moves it to the unsaved photos
			using(FileStream fileStreamOpen=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read)) {
				Assert.AreEqual(0,VideoFiles.RecoverStale(folderRecording,folderUnsaved),"A recording still being written is left alone.");
			}
			Assert.AreEqual(1,VideoFiles.RecoverStale(folderRecording,folderUnsaved));
			Assert.AreEqual(0,Directory.GetFiles(folderRecording).Length);
			PhotoUnsaved photoUnsavedMoved=PhotoQuarantine.List(folderUnsaved).Single();
			StringAssert.EndsWith(photoUnsavedMoved.PathImage,".mp4");
			Assert.AreEqual(7,photoUnsavedMoved.PatNum);
			StringAssert.Contains(photoUnsavedMoved.Reason,"stopped while this video was recording");
			//filed: imported unmounted with its teeth and the time it started, then the files go
			Store store=new Store();
			Assert.AreEqual("",PhotoQuarantine.FileOne(store,photoUnsavedMoved,out bool isFiled));
			Assert.IsTrue(isFiled);
			Assert.AreEqual(1,store.ListVideosImported.Count);
			Document document=store.ListUpdated.Single();
			Assert.AreEqual("2,3",document.ToothNumbers);
			Assert.AreEqual(new DateTime(2026,11,23,10,14,5),document.DateCreated);
			Assert.AreEqual("Video",document.Description,"A recording cut off by a stop has no known length.");
			Assert.AreEqual(0,Directory.GetFiles(folderUnsaved).Length);
		}

		[TestMethod]
		public void Video_LostSidecar_FromTheFileName_DurationKept_OrphanSidecarRemoved() {
			string folderRecording=Path.Combine(_folder,"recording");
			string folderUnsaved=Path.Combine(_folder,"unsaved");
			Directory.CreateDirectory(folderRecording);
			string pathVideo=Path.Combine(folderRecording,"20261123-101405-120_P7_video.mp4");
			File.WriteAllBytes(pathVideo,new byte[] { 1,2,3 });
			string error;
			string pathMoved=VideoFiles.MoveToUnsaved(pathVideo,"The import failed.",out error,folderUnsaved,TimeSpan.FromSeconds(42));
			Assert.IsNotNull(pathMoved,error);
			PhotoUnsaved photoUnsaved=PhotoQuarantine.List(folderUnsaved).Single();
			Assert.AreEqual(7,photoUnsaved.PatNum,"The patient from the file's name when the sidecar is lost.");
			Assert.AreEqual(new DateTime(2026,11,23,10,14,5),photoUnsaved.DateTimeTaken);
			Assert.AreEqual(42,photoUnsaved.DurationSeconds);
			Store store=new Store();
			PhotoQuarantine.FileOne(store,photoUnsaved,out bool isFiled);
			Assert.IsTrue(isFiled);
			Assert.AreEqual("Video 0:42",store.ListUpdated.Single().Description,"The length is kept for filing later.");
			//a file of unknown patient is not moved
			string pathOther=Path.Combine(folderRecording,"other.mp4");
			File.WriteAllBytes(pathOther,new byte[] { 1 });
			Assert.IsNull(VideoFiles.MoveToUnsaved(pathOther,"x",out error,folderUnsaved));
			Assert.IsTrue(File.Exists(pathOther));
			//an old sidecar whose video never started is removed; a new one stays (a recording just starting)
			string pathOrphanOld=Path.Combine(folderRecording,"20261122-090000-000_P7_video.txt");
			string pathOrphanNew=Path.Combine(folderRecording,"20261123-090000-000_P7_video.txt");
			File.WriteAllText(pathOrphanOld,"x");
			File.SetLastWriteTime(pathOrphanOld,DateTime.Now.AddHours(-2));
			File.WriteAllText(pathOrphanNew,"x");
			File.Delete(pathOther);
			VideoFiles.RecoverStale(folderRecording,folderUnsaved);
			Assert.IsFalse(File.Exists(pathOrphanOld));
			Assert.IsTrue(File.Exists(pathOrphanNew));
		}

		[TestMethod]
		public void Video_SpaceRule_AndDuration() {
			Assert.IsTrue(VideoFiles.IsEnoughSpace(3L*1024*1024*1024));
			Assert.IsFalse(VideoFiles.IsEnoughSpace(1L*1024*1024*1024),"Video is off below 2 GB free.");
			Assert.IsTrue(VideoFiles.IsEnoughSpace(-1),"Unknown free space: allowed; a write error stops it.");
			Assert.AreEqual("0:42",VideoFiles.FormatDuration(TimeSpan.FromSeconds(42.3)));
			Assert.AreEqual("2:00",VideoFiles.FormatDuration(TimeSpan.FromMinutes(2)));
		}

		[TestMethod]
		public void File_AnotherPatientsMount_NeverItsSlot() {
			string error;
			PhotoQuarantine.WriteToFolder(_folder,new Bitmap(16,12),MakeRecord(103),out error);
			PhotoUnsaved photoUnsaved=PhotoQuarantine.List(_folder).Single();
			//the mount number was given again to another patient's mount (a deleted mount, the counter reset by a server restart)
			Store store=new Store { PhotoMountState=new PhotoMountState { Mount=new Mount { MountNum=50,PatNum=8 },
				ListSlots=new List<PhotoSlot> { new PhotoSlot { MountItemNum=103,ItemOrder=4,SlotKind=MountSlotKind.Intraoral,IsFilled=false } } } };
			Assert.AreEqual("",PhotoQuarantine.FileOne(store,photoUnsaved,out bool isFiled));
			Assert.AreEqual(0,store.ListUpdated.Single().MountItemNum,"Unmounted, with patient 7.");
			Assert.AreEqual(7,store.ListImported.Single().PatNum);
		}
	}
}
