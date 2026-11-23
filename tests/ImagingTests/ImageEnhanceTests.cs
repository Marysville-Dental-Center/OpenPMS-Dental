// IMG-P3b: the Enhance window's brightness/contrast mapping onto a document's window, the supplied presets and the log text. Licensed under the
// GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ImagingTests {
	[TestClass]
	public class ImageEnhanceTests {
		[TestMethod]
		public void Windowing_NeutralIsTheFullRange_AndANeverSetWindowReadsNeutral() {
			ImageEnhance.ToWindowing(0,0,out int min,out int max);
			Assert.AreEqual(0,min);
			Assert.AreEqual(255,max);
			ImageEnhance.FromWindowing(0,0,out int brightness,out int contrast);
			Assert.AreEqual(0,brightness,"WindowingMax 0 is a window never set.");
			Assert.AreEqual(0,contrast);
			ImageEnhance.FromWindowing(0,255,out brightness,out contrast);
			Assert.AreEqual(0,brightness);
			Assert.AreEqual(0,contrast);
		}

		[TestMethod]
		public void Windowing_BrightnessMovesTheCentre_ContrastNarrowsIt_RoundTrip() {
			ImageEnhance.ToWindowing(0,100,out int min,out int max);
			Assert.AreEqual(96,min,"Contrast 100: a quarter of the range, centred.");
			Assert.AreEqual(159,max,"127.5 + 63.75/2 = 159.375.");
			ImageEnhance.ToWindowing(40,50,out min,out max);
			Assert.IsTrue((min+max)/2.0<127.5,"Brighter: the window moves down.");
			for(int brightness=-60;brightness<=60;brightness+=20) {
				for(int contrast=10;contrast<=100;contrast+=30) {
					ImageEnhance.ToWindowing(brightness,contrast,out min,out max);
					if(min==0 || max==255){
						continue;//clamped at an end of the range: no exact pair (FromWindowing returns the nearest)
					}
					ImageEnhance.FromWindowing(min,max,out int brightnessBack,out int contrastBack);
					Assert.IsTrue(System.Math.Abs(brightnessBack-brightness)<=1 && System.Math.Abs(contrastBack-contrast)<=1,
						"Round trip "+brightness+"/"+contrast+" through "+min+"-"+max+" gave "+brightnessBack+"/"+contrastBack);
				}
			}
		}

		[TestMethod]
		public void Windowing_AlwaysAValidWindow() {
			foreach(int brightness in new[] { -100,-50,0,50,100 }) {
				foreach(int contrast in new[] { 0,50,100,150,-20 }) {
					ImageEnhance.ToWindowing(brightness,contrast,out int min,out int max);
					Assert.IsTrue(min>=0 && max<=255 && max>min,brightness+"/"+contrast+": "+min+"-"+max);
				}
			}
		}

		[TestMethod]
		public void BuiltInPresets_OriginalFirstAndNeutral_NoneAreRows() {
			List<ImageAdjustPreset> listPresets=ImageEnhance.GetBuiltInPresets();
			CollectionAssert.AreEqual(new[] { "Original","Caries","Endo","Perio" },listPresets.Select(x => x.Description).ToArray());
			Assert.IsTrue(listPresets[0].ToSettings().IsNeutral(),"Original removes every filter.");
			Assert.IsTrue(listPresets.All(x => x.ImageAdjustPresetNum==0),"Supplied in code, never rows.");
			Assert.IsTrue(listPresets.Skip(1).All(x => !x.ToSettings().IsNeutral()),"The others filter.");
		}

		[TestMethod]
		public void Describe_ForTheLog() {
			Assert.AreEqual("none",ImageEnhance.Describe(null));
			Assert.AreEqual("none",ImageEnhance.Describe(new ImageAdjustSettings()));
			Assert.AreEqual("gamma 1.20, sharpen 40, smooth low, auto contrast, inverted",ImageEnhance.Describe(new ImageAdjustSettings {
				Gamma=1.2,Sharpen=40,Smooth=ImageSmooth.Low,IsAutoContrast=true,IsInverted=true }));
		}
	}
}
