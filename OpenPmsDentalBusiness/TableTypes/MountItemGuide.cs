// Image camera session: the guide of one slot of a patient's photo mount. A fork table; Open Dental has no such table.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;

namespace OpenPmsDentalBusiness {
	///<summary>The guide of one slot of a patient's mount (mountitem.MountItemNum; one row per slot, unique key MountItemNum): what the camera
	///should shoot in the slot, whether it is a mirror shot, and whether it is intraoral or extraoral. Copied from the template's MountItemDefGuide
	///when the mount is created (Mounts.CreateMountFromDef), because a mount does not keep the key of its template. A slot without a row has no
	///guide.</summary>
	[Serializable]
	[CrudTable]
	public class MountItemGuide:TableBase {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long MountItemGuideNum;
		///<summary>FK to mountitem.MountItemNum. Unique: one row per slot.</summary>
		public long MountItemNum;
		///<summary>What the camera should shoot in the slot, for example "Upper occlusal".</summary>
		public string GuideLabel;
		///<summary>True when the slot takes a mirror shot.</summary>
		public bool IsMirror;
		///<summary>Whether the slot takes a radiograph, an intraoral photo or an extraoral photo.</summary>
		public MountSlotKind SlotKind;

		///<summary></summary>
		public MountItemGuide Copy() {
			return (MountItemGuide)this.MemberwiseClone();
		}
	}
}
