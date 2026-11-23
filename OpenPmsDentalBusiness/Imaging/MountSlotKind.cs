// Image camera session: what kind of picture a photo mount slot takes.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;

namespace OpenPmsDentalBusiness {
	///<summary>What kind of picture a mount slot takes (mountitemdefguide.SlotKind and mountitemguide.SlotKind). Stored as its number.</summary>
	public enum MountSlotKind {
		///<summary>0. A radiograph slot, the default of a slot without a guide.</summary>
		Radiograph=0,
		///<summary>1. An intraoral photo: inside the mouth.</summary>
		Intraoral=1,
		///<summary>2. An extraoral photo: the face or the head.</summary>
		Extraoral=2,
	}
}
