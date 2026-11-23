using System;
using System.Collections.Generic;

namespace OpenPmsDentalBusiness {
	///<summary>The Imaging module's Enhance window (imaging design P3, IMG-P3b): the supplied presets and the brightness/contrast mapping onto a
	///document's windowing (WindowingMin and WindowingMax, 0 to 255, which the module's windowing slider also sets).</summary>
	public static class ImageEnhance {
		///<summary>The supplied presets, in the order shown, before the office's own (imageadjustpreset rows). Not rows: ImageAdjustPresetNum 0.
		///Starting points for reading an image, not diagnostic settings; the office saves its own.</summary>
		public static List<ImageAdjustPreset> GetBuiltInPresets() {
			return new List<ImageAdjustPreset> {
				//Original: the image as taken, brightness/contrast included.
				new ImageAdjustPreset { Description="Original",WindowingMin=0,WindowingMax=255,Gamma=1 },
				//Caries: more contrast and crisper edges, slightly darker mid-greys.
				new ImageAdjustPreset { Description="Caries",WindowingMin=25,WindowingMax=230,Gamma=0.9,Sharpen=40 },
				//Endo: local contrast for canals and apices, a little sharpening and lift.
				new ImageAdjustPreset { Description="Endo",WindowingMin=0,WindowingMax=255,Gamma=1.1,Sharpen=30,IsAutoContrast=true },
				//Perio: lifted mid-greys for the bone level, low noise.
				new ImageAdjustPreset { Description="Perio",WindowingMin=0,WindowingMax=255,Gamma=1.3,Sharpen=20,Smooth=ImageSmooth.Low },
			};
		}

		///<summary>The window for brightness (-100 to 100, positive brighter) and contrast (0 to 100, positive stronger), clamped to 0-255. Brightness
		///moves the window's centre; contrast narrows it, to a quarter of the full range at 100.</summary>
		public static void ToWindowing(int brightness,int contrast,out int windowingMin,out int windowingMax) {
			brightness=Math.Max(-100,Math.Min(100,brightness));
			contrast=Math.Max(0,Math.Min(100,contrast));
			double centre=127.5-brightness*1.275;
			double width=255*Math.Pow(4,-contrast/100.0);
			windowingMin=Math.Max(0,Math.Min(254,(int)Math.Round(centre-width/2)));
			windowingMax=Math.Max(windowingMin+1,Math.Min(255,(int)Math.Round(centre+width/2)));
		}

		///<summary>Brightness and contrast for a window. A window never set (WindowingMax 0) is the full range: 0 and 0. Not every window has an exact
		///pair, because ToWindowing clamps: the nearest is returned.</summary>
		public static void FromWindowing(int windowingMin,int windowingMax,out int brightness,out int contrast) {
			if(windowingMax==0) {
				windowingMin=0;
				windowingMax=255;
			}
			double width=Math.Max(1,windowingMax-windowingMin);
			double centre=(windowingMin+windowingMax)/2.0;
			contrast=Math.Max(0,Math.Min(100,(int)Math.Round(-100*Math.Log(width/255.0)/Math.Log(4))));
			brightness=Math.Max(-100,Math.Min(100,(int)Math.Round((127.5-centre)/1.275)));
		}

		///<summary>A short description of the filters for the security log, for example "gamma 1.20, sharpen 40, auto contrast"; "none" when
		///neutral.</summary>
		public static string Describe(ImageAdjustSettings settings) {
			if(settings==null || settings.IsNeutral()) {
				return "none";
			}
			List<string> listParts=new List<string>();
			if(settings.HasGamma()) {
				listParts.Add("gamma "+settings.Gamma.ToString("0.00",System.Globalization.CultureInfo.InvariantCulture));
			}
			if(settings.Sharpen>0) {
				listParts.Add("sharpen "+settings.Sharpen);
			}
			if(settings.Smooth!=ImageSmooth.Off) {
				listParts.Add("smooth "+settings.Smooth.ToString().ToLowerInvariant());
			}
			if(settings.IsAutoContrast) {
				listParts.Add("auto contrast");
			}
			if(settings.IsInverted) {
				listParts.Add("inverted");
			}
			return string.Join(", ",listParts);
		}
	}
}
