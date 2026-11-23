// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CodeBase;
using OpenPmsDentalBusiness;

namespace ImagingCapture.Photo {
	///<summary>A photo mount of a patient as the camera session reads it: the mount and its photo slots.</summary>
	public sealed class PhotoMountState {
		public Mount Mount { get; set; }
		public List<PhotoSlot> ListSlots { get; set; }=new List<PhotoSlot>();
	}

	///<summary>The database seam of the camera session (IMG-P4b).</summary>
	public interface IPhotoStore {
		///<summary>The mount definitions whose slots take photos (a guide with an intraoral or extraoral kind), in the mount list's order.</summary>
		List<MountDef> GetPhotoMountDefs();
		///<summary>Mounts.CreateMountFromDef for the patient in categoryDefNum, with the "Mount Created" entry the module writes. Returns the mount.
		///</summary>
		Mount CreateMount(MountDef mountDef,long patNum,long categoryDefNum);
		///<summary>The mount and its photo slots (PhotoSessionModel.BuildSlots), or null when it is gone.</summary>
		PhotoMountState GetMountState(long mountNum);
		///<summary>ImageStore.Import of a photo (JPEG) into categoryDefNum for the patient. Throws on failure, leaving no document.</summary>
		Document ImportPhoto(Bitmap bitmap,long categoryDefNum,long patNum);
		///<summary>Documents.Update(document,documentOld).</summary>
		void UpdateDocument(Document document,Document documentOld);
		///<summary>The document, or null when it is gone.</summary>
		Document GetDocument(long docNum);
		///<summary>Retake (IMG-P4b-2), as the X-ray capture session's Retake (design 7.11): the photo moves to a new unmounted item of the mount,
		///described "Retake: slotLabel", with an ImageEdit entry; nothing is deleted. Throws on failure, changing nothing.</summary>
		void MoveToRetake(Document document,long mountNum,string slotLabel,long patNum);
		///<summary>ImageStore.DeleteDocuments for the patient's photo (the file, the row, its drawings; ImageDelete entry). Throws on failure.
		///</summary>
		void DeleteDocument(Document document,long patNum);
		///<summary>An ImageEdit entry for the patient's document.</summary>
		void LogEdit(long patNum,string logText,long docNum);
	}

	///<summary>What PhotoSaver.Save did.</summary>
	public sealed class PhotoSaveResult {
		///<summary>The saved photo, or null when nothing was saved.</summary>
		public Document Document { get; set; }
		///<summary>"" when saved as planned. Set when nothing was saved, or when the photo was saved but could not be placed in its slot.</summary>
		public string Error { get; set; }="";
		///<summary>True when the photo is in its slot (or is a quick photo, which has none).</summary>
		public bool IsPlaced { get; set; }
	}

	///<summary>Saves one photo of a camera session: the JPEG into the patient's images, then the slot, flip, rotation and teeth of the plan in one
	///update. The slot is chosen before the photo is taken, so a photo never waits on a message box unplaced (design 8.4's known limitation).
	///</summary>
	public static class PhotoSaver {
		public static PhotoSaveResult Save(IPhotoStore store,Bitmap bitmap,PhotoShotPlan plan,long patNum,long categoryDefNum) {
			if(store==null) {
				throw new ArgumentNullException(nameof(store));
			}
			PhotoSaveResult result=new PhotoSaveResult();
			if(bitmap==null || plan==null || patNum==0) {
				result.Error="No photo to save.";
				return result;
			}
			Document document;
			try {
				document=store.ImportPhoto(bitmap,categoryDefNum,patNum);
			}
			catch(Exception ex) {
				result.Error="The photo could not be saved: "+ex.Message;
				return result;
			}
			result.Document=document;
			if(plan.MountItemNum==0 && !plan.IsFlipped && plan.DegreesRotated==0 && (plan.ToothNumbers??"")=="") {
				result.IsPlaced=true;//a quick photo with nothing to set
				return result;
			}
			Document documentOld=document.Copy();
			ApplyPlan(document,plan);
			try {
				store.UpdateDocument(document,documentOld);
				result.IsPlaced=true;
			}
			catch(Exception ex) {
				result.Error="The photo was saved but not placed in its slot: "+ex.Message;
			}
			return result;
		}

		///<summary>The plan's fields on the document: slot, flip (set, never cleared), rotation and teeth.</summary>
		public static void ApplyPlan(Document document,PhotoShotPlan plan) {
			document.MountItemNum=plan.MountItemNum;
			if(plan.IsFlipped) {
				document.IsFlipped=true;
			}
			document.DegreesRotated=plan.DegreesRotated;
			document.ToothNumbers=plan.ToothNumbers??"";
		}

		///<summary>The category of a quick photo: the Video category preference when set, else categoryDefNumCurrent (as the Video window).</summary>
		public static long GetQuickPhotoCategory(long prefVideoCategory,long categoryDefNumCurrent) {
			return prefVideoCategory>0 ? prefVideoCategory : categoryDefNumCurrent;
		}

		///<summary>The category of a new photo mount: the definition's default category, else categoryDefNumCurrent (as Mount / Acquire).</summary>
		public static long GetMountCategory(MountDef mountDef,long categoryDefNumCurrent) {
			return mountDef!=null && mountDef.DefaultCat!=0 ? mountDef.DefaultCat : categoryDefNumCurrent;
		}
	}

	///<summary>IPhotoStore through OpenPmsDentalBusiness.</summary>
	public sealed class OdPhotoStore:IPhotoStore {
		public List<MountDef> GetPhotoMountDefs() {
			List<MountDef> listMountDefs=MountDefs.GetDeepCopy();
			if(listMountDefs.Count==0) {
				return listMountDefs;
			}
			List<MountItemDef> listMountItemDefs=MountItemDefs.GetAll();
			List<MountItemDefGuide> listGuides=MountItemDefGuides.GetForMountItemDefs(listMountItemDefs.Select(x => x.MountItemDefNum).ToList());
			HashSet<long> hashSetPhotoItemDefs=new HashSet<long>(listGuides.Where(x => x.SlotKind!=MountSlotKind.Radiograph).Select(x => x.MountItemDefNum));
			HashSet<long> hashSetPhotoDefs=new HashSet<long>(listMountItemDefs.Where(x => hashSetPhotoItemDefs.Contains(x.MountItemDefNum)).Select(x => x.MountDefNum));
			return listMountDefs.Where(x => hashSetPhotoDefs.Contains(x.MountDefNum)).ToList();
		}

		public Mount CreateMount(MountDef mountDef,long patNum,long categoryDefNum) {
			Mount mount=Mounts.CreateMountFromDef(mountDef,patNum,categoryDefNum);
			string categoryName=Defs.GetDef(DefCat.ImageCats,mount.DocCategory)?.ItemName??"";
			SecurityLogs.MakeLogEntry(EnumPermType.ImageCreate,patNum,"Mount Created: "+mount.Description+" with category "+categoryName);
			return mount;
		}

		public PhotoMountState GetMountState(long mountNum) {
			Mount mount=Mounts.GetByNum(mountNum);
			if(mount==null || mount.MountNum==0) {//GetByNum gives an empty mount when there is none
				return null;
			}
			List<MountItem> listMountItems=MountItems.GetItemsForMount(mountNum);
			List<MountItemGuide> listGuides=MountItemGuides.GetForMountItems(listMountItems.Select(x => x.MountItemNum).ToList());
			Document[] arrayDocuments=Documents.GetDocumentsForMountItems(listMountItems);
			return new PhotoMountState { Mount=mount,ListSlots=PhotoSessionModel.BuildSlots(listMountItems,listGuides,arrayDocuments) };
		}

		public Document ImportPhoto(Bitmap bitmap,long categoryDefNum,long patNum) {
			Patient patient=Patients.GetPat(patNum);
			if(patient==null) {
				throw new ApplicationException("The patient was not found.");
			}
			return ImageStore.Import(bitmap,categoryDefNum,ImageType.Photo,patient);
		}

		public void UpdateDocument(Document document,Document documentOld) {
			Documents.Update(document,documentOld);
		}

		public Document GetDocument(long docNum) {
			return Documents.GetByNum(docNum,doReturnNullIfNotFound:true);
		}

		public void MoveToRetake(Document document,long mountNum,string slotLabel,long patNum) {
			MountItem mountItemUnmounted=ImagingCapture.Save.MountPlacement.CreateUnmountedItem(mountNum);
			MountItems.Insert(mountItemUnmounted);
			Document documentOld=document.Copy();
			document.MountItemNum=mountItemUnmounted.MountItemNum;
			document.Description=PhotoSessionTexts.GetRetakeDescription(slotLabel);
			try {
				Documents.Update(document,documentOld);
			}
			catch {
				document.MountItemNum=documentOld.MountItemNum;
				document.Description=documentOld.Description;
				try {
					MountItems.Delete(mountItemUnmounted);
				}
				catch(Exception exDelete) {
					exDelete.DoNothing();//an empty unmounted item stays; harmless
				}
				throw;
			}
			try {
				SecurityLogs.MakeLogEntry(EnumPermType.ImageEdit,patNum,"Camera session retake: DocNum "+document.DocNum+" moved from "+slotLabel
					+" to an unmounted item of mount "+mountNum,document.DocNum,DateTime.MinValue);
			}
			catch(Exception ex) {
				ex.DoNothing();//the photo has moved; a missing log entry must not report the retake as failed
			}
		}

		public void DeleteDocument(Document document,long patNum) {
			Patient patient=Patients.GetPat(patNum);
			if(patient==null) {
				throw new ApplicationException("The patient was not found.");
			}
			string patFolder=PrefC.AtoZfolderUsed==DataStorageType.InDatabase ? "" : ImageStore.GetPatientFolder(patient,ImageStore.GetPreferredAtoZpath());
			Statements.DetachDocFromStatements(document.DocNum);
			ImageStore.DeleteDocuments(new List<Document> { document },patFolder);
		}

		public void LogEdit(long patNum,string logText,long docNum) {
			SecurityLogs.MakeLogEntry(EnumPermType.ImageEdit,patNum,logText,docNum,DateTime.MinValue);
		}
	}

	///<summary>Texts of the camera session that are stored or logged.</summary>
	public static class PhotoSessionTexts {
		///<summary>The description of a photo moved out of its slot by a retake: "Retake: Upper occlusal".</summary>
		public static string GetRetakeDescription(string slotLabel) {
			return "Retake: "+(slotLabel??"").Trim();
		}
	}
}
