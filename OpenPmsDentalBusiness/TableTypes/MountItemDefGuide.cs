// Image camera session: the guide of one photo mount template slot. A fork table; Open Dental has no such table.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;

namespace OpenPmsDentalBusiness {
	///<summary>The guide of one slot of a mount template (mountitemdef.MountItemDefNum; one row per slot, unique key MountItemDefNum): what the
	///camera should shoot in the slot, whether it is a mirror shot, and whether it is intraoral or extraoral. A slot without a row has no guide.
	///Mounts.CreateMountFromDef copies the row to a MountItemGuide for each new mount item.</summary>
	[Serializable]
	[CrudTable]
	public class MountItemDefGuide:TableBase {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long MountItemDefGuideNum;
		///<summary>FK to mountitemdef.MountItemDefNum. Unique: one row per slot.</summary>
		public long MountItemDefNum;
		///<summary>What the camera should shoot in the slot, for example "Upper occlusal".</summary>
		public string GuideLabel;
		///<summary>True when the slot takes a mirror shot.</summary>
		public bool IsMirror;
		///<summary>Whether the slot takes a radiograph, an intraoral photo or an extraoral photo.</summary>
		public MountSlotKind SlotKind;

		///<summary></summary>
		public MountItemDefGuide Copy() {
			return (MountItemDefGuide)this.MemberwiseClone();
		}
	}
}
