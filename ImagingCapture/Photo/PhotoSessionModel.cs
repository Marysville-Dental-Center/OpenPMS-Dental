// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OpenPmsDentalBusiness;

namespace ImagingCapture.Photo {
	///<summary>One slot of a photo mount in a camera session (IMG-P4b): the mount item, its guide (mountitemguide) and whether it holds a photo.
	///</summary>
	public sealed class PhotoSlot {
		public long MountItemNum { get; set; }
		public int ItemOrder { get; set; }
		public int Xpos { get; set; }
		public int Ypos { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int RotateOnAcquire { get; set; }
		///<summary>Universal tooth numbers as stored, "" for none.</summary>
		public string ToothNumbers { get; set; }="";
		///<summary>What to shoot, for example "Upper occlusal"; "" when the slot has no guide text.</summary>
		public string GuideLabel { get; set; }="";
		public bool IsMirror { get; set; }
		public MountSlotKind SlotKind { get; set; }
		///<summary>True when a document is in the slot.</summary>
		public bool IsFilled { get; set; }

		///<summary>The guide text, or "Slot N" when the slot has none.</summary>
		public string GetLabel() {
			return GuideLabel!="" ? GuideLabel : "Slot "+ItemOrder.ToString(CultureInfo.InvariantCulture);
		}
	}

	///<summary>Where and how the next photo is saved: the slot (MountItemNum 0 for a quick photo, which stays unmounted), saved flipped for a
	///mirror shot or a mount that flips on acquire, the slot's rotation and teeth.</summary>
	public sealed class PhotoShotPlan {
		public long MountItemNum { get; set; }
		///<summary>The slot's index in the model's ListSlots, -1 for a quick photo.</summary>
		public int IdxSlot { get; set; }=-1;
		public bool IsFlipped { get; set; }
		public int DegreesRotated { get; set; }
		public string ToothNumbers { get; set; }="";
		public string GuideLabel { get; set; }="";
	}

	///<summary>The pure logic of a camera session (owner imaging design, camera and video, section 2): which slot the next photo fills, skipping
	///forward and back, choosing a slot by a click, and advancing after each saved photo. A photo mount is filled in ItemOrder; a quick-photos
	///session has no mount and saves every photo unmounted. Radiograph slots are never filled (design: photos never land in an X-ray slot).
	///No database and no camera.</summary>
	public sealed class PhotoSessionModel {
		private readonly List<PhotoSlot> _listSlots;
		private readonly bool _isFlipOnAcquire;

		///<summary>A photo mount: listSlots are its photo slots in any order (sorted here by ItemOrder). isFlipOnAcquire: the mount's flag.</summary>
		public PhotoSessionModel(IEnumerable<PhotoSlot> listSlots,bool isFlipOnAcquire) {
			_listSlots=(listSlots??Enumerable.Empty<PhotoSlot>()).Where(x => x!=null && x.SlotKind!=MountSlotKind.Radiograph)
				.OrderBy(x => x.ItemOrder).ToList();
			_isFlipOnAcquire=isFlipOnAcquire;
			IsQuickPhotos=false;
			IdxNext=GetNextEmpty(-1);
		}

		private PhotoSessionModel() {
			_listSlots=new List<PhotoSlot>();
			IsQuickPhotos=true;
			IdxNext=-1;
		}

		///<summary>A session without a mount: each photo is saved unmounted.</summary>
		public static PhotoSessionModel CreateQuickPhotos() {
			return new PhotoSessionModel();
		}

		public bool IsQuickPhotos { get; }
		public IReadOnlyList<PhotoSlot> ListSlots => _listSlots;
		///<summary>The slot the next photo fills, -1 when none (quick photos, or every slot is filled).</summary>
		public int IdxNext { get; private set; }
		public PhotoSlot SlotNext => IdxNext>=0 ? _listSlots[IdxNext] : null;
		///<summary>Photos saved in this session.</summary>
		public int CountSaved { get; private set; }

		///<summary>True for a photo mount whose photo slots are all filled: nothing more to take.</summary>
		public bool IsComplete => !IsQuickPhotos && IdxNext==-1;

		public int CountFilled => _listSlots.Count(x => x.IsFilled);

		///<summary>The next empty slot after the next one, wrapping round; the empty slot stays empty. False when there is no other empty slot.
		///</summary>
		public bool SkipForward() {
			if(IdxNext<0) {
				return false;
			}
			int idx=GetNextEmpty(IdxNext);
			if(idx<0 || idx==IdxNext) {
				return false;
			}
			IdxNext=idx;
			return true;
		}

		///<summary>The empty slot before the next one, wrapping round. False when there is no other empty slot.</summary>
		public bool SkipBack() {
			if(IdxNext<0) {
				return false;
			}
			for(int step=1;step<_listSlots.Count;step++) {
				int idx=(IdxNext-step+_listSlots.Count)%_listSlots.Count;
				if(!_listSlots[idx].IsFilled) {
					IdxNext=idx;
					return true;
				}
			}
			return false;
		}

		///<summary>Makes the slot with this index the next one (a click on the mount). False for a filled slot or a bad index.</summary>
		public bool SelectSlot(int idx) {
			if(idx<0 || idx>=_listSlots.Count || _listSlots[idx].IsFilled) {
				return false;
			}
			IdxNext=idx;
			return true;
		}

		///<summary>The plan of the next photo; null when a photo mount is complete. teethQuick: the teeth of a quick photo.</summary>
		public PhotoShotPlan PlanShot(string teethQuick="") {
			if(IsQuickPhotos) {
				return new PhotoShotPlan { MountItemNum=0,IdxSlot=-1,IsFlipped=false,DegreesRotated=0,ToothNumbers=teethQuick??"" };
			}
			PhotoSlot slot=SlotNext;
			if(slot==null) {
				return null;
			}
			return new PhotoShotPlan {
				MountItemNum=slot.MountItemNum,
				IdxSlot=IdxNext,
				IsFlipped=slot.IsMirror || _isFlipOnAcquire,
				DegreesRotated=slot.RotateOnAcquire,
				ToothNumbers=slot.ToothNumbers??"",
				GuideLabel=slot.GuideLabel??"",
			};
		}

		///<summary>After the photo of plan was saved: its slot is filled and the next empty slot after it becomes the next one.</summary>
		public void MarkSaved(PhotoShotPlan plan) {
			if(plan==null) {
				throw new ArgumentNullException(nameof(plan));
			}
			CountSaved++;
			if(IsQuickPhotos || plan.IdxSlot<0 || plan.IdxSlot>=_listSlots.Count) {
				return;
			}
			_listSlots[plan.IdxSlot].IsFilled=true;
			IdxNext=GetNextEmpty(plan.IdxSlot);
		}

		///<summary>The first empty slot after idxFrom, wrapping round to idxFrom itself; -1 when every slot is filled.</summary>
		private int GetNextEmpty(int idxFrom) {
			if(_listSlots.Count==0) {
				return -1;
			}
			for(int step=1;step<=_listSlots.Count;step++) {
				int idx=((idxFrom<0 ? -1 : idxFrom)+step)%_listSlots.Count;
				if(!_listSlots[idx].IsFilled) {
					return idx;
				}
			}
			return -1;
		}

		///<summary>The line over the live picture: "Next: Upper occlusal", then "Mirror shot, saved flipped" and the teeth; or why there is none.
		///</summary>
		public string GetGuideText(Func<string,string> funcDisplayTeeth=null) {
			if(IsQuickPhotos) {
				return "Quick photos: each photo is saved unmounted.";
			}
			PhotoSlot slot=SlotNext;
			if(slot==null) {
				return "Every slot is filled. End the session (Esc).";
			}
			List<string> listParts=new List<string>();
			if(slot.IsMirror) {
				listParts.Add("mirror shot, saved flipped");
			}
			if(slot.RotateOnAcquire!=0) {
				listParts.Add("rotated "+slot.RotateOnAcquire.ToString(CultureInfo.InvariantCulture)+" degrees");
			}
			string teeth=slot.ToothNumbers??"";
			if(teeth!="") {
				listParts.Add("teeth "+(funcDisplayTeeth==null ? teeth : funcDisplayTeeth(teeth)));
			}
			string text="Next: "+slot.GetLabel();
			if(listParts.Count>0) {
				text+=" ("+string.Join(", ",listParts)+")";
			}
			return text;
		}

		///<summary>"2 of 5 taken" for a mount, "3 photos saved" for quick photos.</summary>
		public string GetProgressText() {
			if(IsQuickPhotos) {
				return CountSaved==1 ? "1 photo saved" : CountSaved.ToString(CultureInfo.InvariantCulture)+" photos saved";
			}
			return CountFilled.ToString(CultureInfo.InvariantCulture)+" of "+_listSlots.Count.ToString(CultureInfo.InvariantCulture)+" taken";
		}

		///<summary>The photo slots of a mount: its slots (not the text item, not unmounted items) whose guide has a photo kind, with their guides and
		///whether a document is in them. listDocuments: the mount's documents (MountItemNum of each).</summary>
		public static List<PhotoSlot> BuildSlots(IEnumerable<MountItem> listMountItems,IEnumerable<MountItemGuide> listMountItemGuides,
			IEnumerable<Document> listDocuments)
		{
			List<MountItemGuide> listGuides=(listMountItemGuides??Enumerable.Empty<MountItemGuide>()).Where(x => x!=null).ToList();
			HashSet<long> hashSetFilled=new HashSet<long>((listDocuments??Enumerable.Empty<Document>()).Where(x => x!=null && x.MountItemNum!=0)
				.Select(x => x.MountItemNum));
			List<PhotoSlot> listSlots=new List<PhotoSlot>();
			foreach(MountItem mountItem in (listMountItems??Enumerable.Empty<MountItem>()).Where(x => x!=null && x.ItemOrder>0)) {
				MountItemGuide guide=listGuides.FirstOrDefault(x => x.MountItemNum==mountItem.MountItemNum);
				if(guide==null || guide.SlotKind==MountSlotKind.Radiograph) {
					continue;
				}
				listSlots.Add(new PhotoSlot {
					MountItemNum=mountItem.MountItemNum,
					ItemOrder=mountItem.ItemOrder,
					Xpos=mountItem.Xpos,
					Ypos=mountItem.Ypos,
					Width=mountItem.Width,
					Height=mountItem.Height,
					RotateOnAcquire=mountItem.RotateOnAcquire,
					ToothNumbers=mountItem.ToothNumbers??"",
					GuideLabel=(guide.GuideLabel??"").Trim(),
					IsMirror=guide.IsMirror,
					SlotKind=guide.SlotKind,
					IsFilled=hashSetFilled.Contains(mountItem.MountItemNum),
				});
			}
			return listSlots.OrderBy(x => x.ItemOrder).ToList();
		}

		///<summary>True when a mount (by its slots' guides) is a photo mount: at least one slot takes an intraoral or extraoral photo.</summary>
		public static bool IsPhotoMount(IEnumerable<MountItemGuide> listMountItemGuides) {
			return (listMountItemGuides??Enumerable.Empty<MountItemGuide>()).Any(x => x!=null && x.SlotKind!=MountSlotKind.Radiograph);
		}

		///<summary>The same for a mount definition by its slots' guides.</summary>
		public static bool IsPhotoMountDef(IEnumerable<MountItemDefGuide> listMountItemDefGuides) {
			return (listMountItemDefGuides??Enumerable.Empty<MountItemDefGuide>()).Any(x => x!=null && x.SlotKind!=MountSlotKind.Radiograph);
		}
	}
}
