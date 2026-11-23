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
