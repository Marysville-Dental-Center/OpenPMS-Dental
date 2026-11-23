// IMG-P3c-2: a DICOM image's noise reduction and sharpening run on its 16-bit (12-bit) data before the window; local contrast, gamma and
// invert after it (ImageAdjust.WindowDicom). Synthetic data only. Licensed under the GNU General Public License, version 2 or later. See
// LICENSE in the repository root.
using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ImagingTests {
	[TestClass]
	public class WindowDicomTests {
		///<summary>A 12-bit image in 16 bits allocated, MONOCHROME2, from funcValue(x,y).</summary>
		private static BitmapDicom Make(int width,int height,Func<int,int,int> funcValue) {
			BitmapDicom bitmapDicom=new BitmapDicom { Width=width,Height=height,BitsAllocated=16,BitsStored=12,
				PhotometricInterp=EnumDicomPhotometricInterp.MONOCHROME2,ArrayDataRaw=new ushort[width*height] };
			for(int y=0;y<height;y++) {
				for(int x=0;x<width;x++) {
					bitmapDicom.ArrayDataRaw[y*width+x]=(ushort)Math.Max(0,Math.Min(4095,funcValue(x,y)));
				}
			}
			return bitmapDicom;
		}

		[TestMethod]
		public void Neutral_IsExactlyTheWindow() {
			BitmapDicom bitmapDicom=Make(20,10,(x,y) => x*200);
			using Bitmap bitmapExpected=DicomHelper.ApplyWindowing(bitmapDicom,0,0);
			using Bitmap bitmapActual=ImageAdjust.WindowDicom(bitmapDicom,0,0,new ImageAdjustSettings());
			for(int x=0;x<20;x++) {
				Assert.AreEqual(bitmapExpected.GetPixel(x,5).ToArgb(),bitmapActual.GetPixel(x,5).ToArgb(),"x="+x);
			}
			using Bitmap bitmapNull=ImageAdjust.WindowDicom(bitmapDicom,0,0,null);
			Assert.AreEqual(bitmapExpected.GetPixel(7,5).ToArgb(),bitmapNull.GetPixel(7,5).ToArgb());
		}

		[TestMethod]
		public void Smooth_OnTheRawData_RemovesASpeck_AndLeavesTheRawUnchanged() {
			BitmapDicom bitmapDicom=Make(9,9,(x,y) => x==4 && y==4 ? 4095 : 1600);
			using Bitmap bitmap=ImageAdjust.WindowDicom(bitmapDicom,0,0,new ImageAdjustSettings { Smooth=ImageSmooth.Low });
			using Bitmap bitmapPlain=DicomHelper.ApplyWindowing(bitmapDicom,0,0);
			Assert.AreEqual(bitmapPlain.GetPixel(1,1).R,bitmap.GetPixel(4,4).R,"The speck takes its surroundings' value.");
			Assert.AreEqual(4095,bitmapDicom.ArrayDataRaw[4*9+4],"The raw data is not changed.");
		}

		[TestMethod]
		public void Sharpen_OnTheRawData_ShowsAStepTheWindowWouldNearlyMerge() {
			//1600 and 1616 in 12 bits: one step apart at the 8-bit window (16 raw values per step)
			BitmapDicom bitmapDicom=Make(40,10,(x,y) => x<20 ? 1600 : 1616);
			using Bitmap bitmapPlain=DicomHelper.ApplyWindowing(bitmapDicom,0,0);
			using Bitmap bitmap=ImageAdjust.WindowDicom(bitmapDicom,0,0,new ImageAdjustSettings { Sharpen=100 });
			int stepPlain=bitmapPlain.GetPixel(20,5).R-bitmapPlain.GetPixel(19,5).R;
			int stepSharpened=bitmap.GetPixel(20,5).R-bitmap.GetPixel(19,5).R;
			Assert.IsTrue(stepSharpened>stepPlain,"The edge is steeper after sharpening on the raw data: "+stepPlain+" to "+stepSharpened);
			Assert.AreEqual(bitmapPlain.GetPixel(2,5).R,bitmap.GetPixel(2,5).R,"Flat areas far from the edge stay.");
		}

		[TestMethod]
		public void InvertAndGamma_AfterTheWindow() {
			BitmapDicom bitmapDicom=Make(4,1,(x,y) => new[] { 0,1024,2048,4095 }[x]);
			using Bitmap bitmapPlain=DicomHelper.ApplyWindowing(bitmapDicom,0,0);
			using Bitmap bitmap=ImageAdjust.WindowDicom(bitmapDicom,0,0,new ImageAdjustSettings { IsInverted=true });
			for(int x=0;x<4;x++) {
				Assert.AreEqual(255-bitmapPlain.GetPixel(x,0).R,bitmap.GetPixel(x,0).R,"x="+x);
			}
		}

		[TestMethod]
		public void FilterBeforeWindow_NothingToDo_IsNull() {
			BitmapDicom bitmapDicom=Make(4,4,(x,y) => 100);
			Assert.IsNull(ImageAdjust.FilterBeforeWindow(bitmapDicom,new ImageAdjustSettings { Gamma=1.3,IsInverted=true,IsAutoContrast=true }),
				"Only noise reduction and sharpening run before the window.");
			Assert.IsNull(ImageAdjust.FilterBeforeWindow(null,new ImageAdjustSettings { Sharpen=50 }));
		}
	}
}
