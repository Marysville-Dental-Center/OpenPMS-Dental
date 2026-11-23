// Image camera session: the guide of each slot of a patient's photo mount (mountitemguide).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenPmsDentalBusiness {
	///<summary>The guide of each slot of a patient's photo mount (mountitemguide, one row per MountItemNum). A slot without a row has no guide. MountItems.Delete and Mounts.Delete delete the slot's row. Not cached.</summary>
	public class MountItemGuides {
		#region Get Methods
		///<summary>The row of the slot, or null when it has none.</summary>
		public static MountItemGuide GetForMountItem(long mountItemNum) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				return Meth.GetObject<MountItemGuide>(MethodBase.GetCurrentMethod(),mountItemNum);
			}
			return Crud.MountItemGuideCrud.SelectOne("SELECT * FROM mountitemguide WHERE MountItemNum="+POut.Long(mountItemNum));
		}

		///<summary>The rows of these slots, in one query. A slot without a row has none in the list.</summary>
		public static List<MountItemGuide> GetForMountItems(List<long> listMountItemNums) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				return Meth.GetObject<List<MountItemGuide>>(MethodBase.GetCurrentMethod(),listMountItemNums);
			}
			if(listMountItemNums==null || listMountItemNums.Count==0) {
				return new List<MountItemGuide>();
			}
			return Crud.MountItemGuideCrud.SelectMany("SELECT * FROM mountitemguide WHERE MountItemNum IN("+string.Join(",",listMountItemNums.Distinct().Select(x => POut.Long(x)))+")");
		}
		#endregion Get Methods

		#region Modification Methods
		///<summary>Saves the slot's guide: inserts the row, or updates the slot's existing row (the MountItemGuideNum of the argument is ignored).</summary>
		public static void Upsert(MountItemGuide mountItemGuide) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),mountItemGuide);
				return;
			}
			MountItemGuide mountItemGuideOld=GetForMountItem(mountItemGuide.MountItemNum);
			if(mountItemGuideOld==null) {
				mountItemGuide.MountItemGuideNum=0;
				try {
					Crud.MountItemGuideCrud.Insert(mountItemGuide);
					return;
				}
				catch(Exception) {
					//Another workstation inserted the row for this slot in the meantime (the unique key MountItemNum; MyISAM has no transaction): update it.
					mountItemGuideOld=GetForMountItem(mountItemGuide.MountItemNum);
					if(mountItemGuideOld==null) {
						throw;
					}
				}
			}
			mountItemGuide.MountItemGuideNum=mountItemGuideOld.MountItemGuideNum;
			Crud.MountItemGuideCrud.Update(mountItemGuide,mountItemGuideOld);
		}

		///<summary>Gives mountItemNumTo the guide of mountItemNumFrom, when that slot has one (a pasted copy of a mount, IMG-P4a review). Nothing
		///when it has none.</summary>
		public static void CopyForMountItem(long mountItemNumFrom,long mountItemNumTo) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),mountItemNumFrom,mountItemNumTo);
				return;
			}
			MountItemGuide mountItemGuide=GetForMountItem(mountItemNumFrom);
			if(mountItemGuide==null) {
				return;
			}
			Upsert(new MountItemGuide { MountItemNum=mountItemNumTo,GuideLabel=mountItemGuide.GuideLabel,IsMirror=mountItemGuide.IsMirror,
				SlotKind=mountItemGuide.SlotKind });
		}

		///<summary>Deletes the slot's row, if it has one.</summary>
		public static void DeleteForMountItem(long mountItemNum) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),mountItemNum);
				return;
			}
			DataCore.NonQ("DELETE FROM mountitemguide WHERE MountItemNum="+POut.Long(mountItemNum));
		}

		///<summary>Deletes the rows of every slot of this mount, if it has any.</summary>
		public static void DeleteForMount(long mountNum) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),mountNum);
				return;
			}
			DataCore.NonQ("DELETE mountitemguide FROM mountitemguide INNER JOIN mountitem ON mountitem.MountItemNum=mountitemguide.MountItemNum WHERE mountitem.MountNum="+POut.Long(mountNum));
		}
		#endregion Modification Methods
	}
}
