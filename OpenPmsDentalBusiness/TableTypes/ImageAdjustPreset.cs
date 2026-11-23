// Imaging adjust tools: an adjust preset the office saved. A fork table; Open Dental has no such table.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;

namespace OpenPmsDentalBusiness {
	///<summary>An adjust preset the office saved: a name with brightness and contrast (the windowing minimum and maximum) and the display filters.
	///The built-in presets live in code, not in this table. Changing the presets needs EnumPermType.ImageAdjustPresets.</summary>
	[Serializable]
	[CrudTable]
	public class ImageAdjustPreset:TableBase {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long ImageAdjustPresetNum;
		///<summary>The name shown in the preset list.</summary>
		public string Description;
		///<summary>The order in the preset list, 0 first.</summary>
		public int ItemOrder;
		///<summary>The windowing minimum, as document.WindowingMin (brightness and contrast).</summary>
		public int WindowingMin;
		///<summary>The windowing maximum, as document.WindowingMax.</summary>
		public int WindowingMax;
		///<summary>The gamma, 1 for none.</summary>
		public double Gamma;
		///<summary>The sharpen amount, 0 to 100, 0 for none.</summary>
		public int Sharpen;
		///<summary>The smoothing strength.</summary>
		public ImageSmooth Smooth;
		///<summary>True to stretch the contrast automatically.</summary>
		public bool IsAutoContrast;
		///<summary>True to show the image inverted.</summary>
		public bool IsInverted;
		///<summary>True when the preset is hidden from the list.</summary>
		public bool IsHidden;

		///<summary></summary>
		public ImageAdjustPreset Copy() {
			return (ImageAdjustPreset)this.MemberwiseClone();
		}

		///<summary>The filter settings of this preset (not its windowing).</summary>
		public ImageAdjustSettings ToSettings() {
			ImageAdjustSettings settings=new ImageAdjustSettings();
			settings.Gamma=Gamma;
			settings.Sharpen=Sharpen;
			settings.Smooth=Smooth;
			settings.IsAutoContrast=IsAutoContrast;
			settings.IsInverted=IsInverted;
			return settings;
		}
	}
}
