// Imaging adjust tools: the display filters saved for one image. A fork table; Open Dental has no such table.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;

namespace OpenPmsDentalBusiness {
	///<summary>The display filters saved for one image (document.DocNum; one row per image, unique key DocNum): gamma, sharpen, smooth, auto
	///contrast and invert, shown whenever the image is shown. The image's brightness and contrast stay in document.WindowingMin and WindowingMax.
	///The row exists only while a setting is not neutral (DocumentAdjusts.Upsert deletes it otherwise). The trim columns are for a later video
	///phase.</summary>
	[Serializable]
	[CrudTable]
	public class DocumentAdjust:TableBase {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long DocumentAdjustNum;
		///<summary>FK to document.DocNum. Unique: one row per image.</summary>
		public long DocNum;
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
		///<summary>For a later video phase: the start of the trim in milliseconds. 0 means no trim.</summary>
		public int TrimStartMs;
		///<summary>For a later video phase: the end of the trim in milliseconds. 0 means no trim.</summary>
		public int TrimEndMs;

		///<summary></summary>
		public DocumentAdjust Copy() {
			return (DocumentAdjust)this.MemberwiseClone();
		}

		///<summary>The filter settings of this row.</summary>
		public ImageAdjustSettings ToSettings() {
			ImageAdjustSettings settings=new ImageAdjustSettings();
			settings.Gamma=Gamma;
			settings.Sharpen=Sharpen;
			settings.Smooth=Smooth;
			settings.IsAutoContrast=IsAutoContrast;
			settings.IsInverted=IsInverted;
			return settings;
		}

		///<summary>A row for the image with these settings and no trim. Its key is not set.</summary>
		public static DocumentAdjust FromSettings(long docNum,ImageAdjustSettings settings) {
			DocumentAdjust documentAdjust=new DocumentAdjust();
			documentAdjust.DocNum=docNum;
			documentAdjust.Gamma=settings.Gamma;
			documentAdjust.Sharpen=settings.Sharpen;
			documentAdjust.Smooth=settings.Smooth;
			documentAdjust.IsAutoContrast=settings.IsAutoContrast;
			documentAdjust.IsInverted=settings.IsInverted;
			return documentAdjust;
		}
	}
}
