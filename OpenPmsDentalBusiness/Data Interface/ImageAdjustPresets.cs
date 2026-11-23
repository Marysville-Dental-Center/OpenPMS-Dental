// Imaging adjust tools: the office's own adjust presets (imageadjustpreset).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenPmsDentalBusiness {
	///<summary>The adjust presets the office saved (imageadjustpreset). The built-in presets live in code. Changing them needs
	///EnumPermType.ImageAdjustPresets, which the window checks. Not cached.</summary>
	public class ImageAdjustPresets {
		#region Get Methods
		///<summary>Every preset, hidden ones too, in list order.</summary>
		public static List<ImageAdjustPreset> GetAll() {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				return Meth.GetObject<List<ImageAdjustPreset>>(MethodBase.GetCurrentMethod());
			}
			return Crud.ImageAdjustPresetCrud.SelectMany("SELECT * FROM imageadjustpreset ORDER BY ItemOrder,ImageAdjustPresetNum");
		}
		#endregion Get Methods

		#region Modification Methods
		///<summary></summary>
		public static long Insert(ImageAdjustPreset imageAdjustPreset) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				imageAdjustPreset.ImageAdjustPresetNum=Meth.GetLong(MethodBase.GetCurrentMethod(),imageAdjustPreset);
				return imageAdjustPreset.ImageAdjustPresetNum;
			}
			return Crud.ImageAdjustPresetCrud.Insert(imageAdjustPreset);
		}

		///<summary></summary>
		public static void Update(ImageAdjustPreset imageAdjustPreset) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),imageAdjustPreset);
				return;
			}
			Crud.ImageAdjustPresetCrud.Update(imageAdjustPreset);
		}

		///<summary></summary>
		public static void Delete(long imageAdjustPresetNum) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),imageAdjustPresetNum);
				return;
			}
			Crud.ImageAdjustPresetCrud.Delete(imageAdjustPresetNum);
		}
		#endregion Modification Methods
	}
}
