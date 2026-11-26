// Image camera session: the guide of each slot of a photo mount template (mountitemdefguide).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenPmsDentalBusiness {
	///<summary>The guide of each slot of a photo mount template (mountitemdefguide, one row per MountItemDefNum). A slot without a row has no guide. Mounts.CreateMountFromDef copies the row to a mountitemguide row for each new mount item. MountItemDefs.Delete, MountItemDefs.DeleteForMount and MountDefs.Delete delete the slot's row. Not cached.</summary>
	public class MountItemDefGuides {
		#region Get Methods
		///<summary>The row of the slot, or null when it has none.</summary>
		public static MountItemDefGuide GetForMountItemDef(long mountItemDefNum) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				return Meth.GetObject<MountItemDefGuide>(MethodBase.GetCurrentMethod(),mountItemDefNum);
			}
			return Crud.MountItemDefGuideCrud.SelectOne("SELECT * FROM mountitemdefguide WHERE MountItemDefNum="+POut.Long(mountItemDefNum));
		}

		///<summary>The rows of these slots, in one query. A slot without a row has none in the list.</summary>
		public static List<MountItemDefGuide> GetForMountItemDefs(List<long> listMountItemDefNums) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				return Meth.GetObject<List<MountItemDefGuide>>(MethodBase.GetCurrentMethod(),listMountItemDefNums);
			}
			if(listMountItemDefNums==null || listMountItemDefNums.Count==0) {
				return new List<MountItemDefGuide>();
			}
			return Crud.MountItemDefGuideCrud.SelectMany("SELECT * FROM mountitemdefguide WHERE MountItemDefNum IN("+string.Join(",",listMountItemDefNums.Distinct().Select(x => POut.Long(x)))+")");
		}
		#endregion Get Methods

		#region Mount type
		///<summary>FB-1: a mount template is either an X-ray mount or a photo mount, never both (mixed templates were confusing to set up).
		///True when any image slot of the template is a photo slot; the capture code treats such a template as a photo mount
		///(PhotoSessionModel.IsPhotoMountDef). Text items are not slots.</summary>
		public static bool IsPhotoMountDef(long mountDefNum) {
			List<long> listMountItemDefNums=MountItemDefs.GetForMountDef(mountDefNum).FindAll(x => x.TextShowing=="").Select(x => x.MountItemDefNum).ToList();
			return GetForMountItemDefs(listMountItemDefNums).Any(x => x.SlotKind!=MountSlotKind.Radiograph);
		}

		///<summary>FB-1: true when the template has both X-ray slots (no row, or a Radiograph row) and photo slots.</summary>
		public static bool IsMixedMountDef(long mountDefNum) {
			List<long> listMountItemDefNums=MountItemDefs.GetForMountDef(mountDefNum).FindAll(x => x.TextShowing=="").Select(x => x.MountItemDefNum).ToList();
			List<MountItemDefGuide> listMountItemDefGuides=GetForMountItemDefs(listMountItemDefNums);
			int countPhoto=listMountItemDefGuides.Count(x => x.SlotKind!=MountSlotKind.Radiograph);
			return countPhoto>0 && countPhoto<listMountItemDefNums.Count;
		}

		///<summary>FB-1: the type is set once for the whole template. Photos: every X-ray slot becomes an intraoral photo slot (an extraoral
		///slot stays extraoral). X-rays: every photo slot becomes a radiograph slot; a slot left with no guide text keeps no row. Text items are
		///left alone. Mounts already made from the template keep the guides they were made with.</summary>
		public static void SetMountType(long mountDefNum,bool isPhotoMount) {
			List<MountItemDef> listMountItemDefs=MountItemDefs.GetForMountDef(mountDefNum).FindAll(x => x.TextShowing=="");
			List<MountItemDefGuide> listMountItemDefGuides=GetForMountItemDefs(listMountItemDefs.Select(x => x.MountItemDefNum).ToList());
			foreach(MountItemDef mountItemDef in listMountItemDefs) {
				MountItemDefGuide mountItemDefGuide=listMountItemDefGuides.Find(x => x.MountItemDefNum==mountItemDef.MountItemDefNum);
				if(isPhotoMount) {
					if(mountItemDefGuide==null) {
						Upsert(new MountItemDefGuide { MountItemDefNum=mountItemDef.MountItemDefNum,GuideLabel="",IsMirror=false,SlotKind=MountSlotKind.Intraoral });
					}
					else if(mountItemDefGuide.SlotKind==MountSlotKind.Radiograph) {
						mountItemDefGuide.SlotKind=MountSlotKind.Intraoral;
						Upsert(mountItemDefGuide);
					}
					continue;
				}
				if(mountItemDefGuide==null) {
					continue;
				}
				if(mountItemDefGuide.GuideLabel.Trim()=="") {
					DeleteForMountItemDef(mountItemDef.MountItemDefNum);//an X-ray slot with no guide keeps no row
					continue;
				}
				if(mountItemDefGuide.SlotKind!=MountSlotKind.Radiograph || mountItemDefGuide.IsMirror) {
					mountItemDefGuide.SlotKind=MountSlotKind.Radiograph;
					mountItemDefGuide.IsMirror=false;//a mirror shot is a photo thing
					Upsert(mountItemDefGuide);
				}
			}
		}
		#endregion Mount type

		#region Modification Methods
		///<summary>Saves the slot's guide: inserts the row, or updates the slot's existing row (the MountItemDefGuideNum of the argument is ignored).</summary>
		public static void Upsert(MountItemDefGuide mountItemDefGuide) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),mountItemDefGuide);
				return;
			}
			MountItemDefGuide mountItemDefGuideOld=GetForMountItemDef(mountItemDefGuide.MountItemDefNum);
			if(mountItemDefGuideOld==null) {
				mountItemDefGuide.MountItemDefGuideNum=0;
				try {
					Crud.MountItemDefGuideCrud.Insert(mountItemDefGuide);
					return;
				}
				catch(Exception) {
					//Another workstation inserted the row for this slot in the meantime (the unique key MountItemDefNum; MyISAM has no transaction): update it.
					mountItemDefGuideOld=GetForMountItemDef(mountItemDefGuide.MountItemDefNum);
					if(mountItemDefGuideOld==null) {
						throw;
					}
				}
			}
			mountItemDefGuide.MountItemDefGuideNum=mountItemDefGuideOld.MountItemDefGuideNum;
			Crud.MountItemDefGuideCrud.Update(mountItemDefGuide,mountItemDefGuideOld);
		}

		///<summary>Deletes the slot's row, if it has one.</summary>
		public static void DeleteForMountItemDef(long mountItemDefNum) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),mountItemDefNum);
				return;
			}
			DataCore.NonQ("DELETE FROM mountitemdefguide WHERE MountItemDefNum="+POut.Long(mountItemDefNum));
		}

		///<summary>Deletes the rows of every slot of this mount template, if it has any.</summary>
		public static void DeleteForMountDef(long mountDefNum) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),mountDefNum);
				return;
			}
			DataCore.NonQ("DELETE mountitemdefguide FROM mountitemdefguide INNER JOIN mountitemdef ON mountitemdef.MountItemDefNum=mountitemdefguide.MountItemDefNum WHERE mountitemdef.MountDefNum="+POut.Long(mountDefNum));
		}
		#endregion Modification Methods
	}
}
