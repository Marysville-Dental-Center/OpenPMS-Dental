// The adjust tools' filters (imaging design P3): ImageAdjust on synthetic pictures. Licensed under the GNU General Public License, version 2 or
// later. See LICENSE in the repository root.
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ImagingTests {
	[TestClass]
	public class ImageAdjustTests {
		private static Bitmap Grey(int width,int height,Func<int,int,int> funcValue,PixelFormat pixelFormat=PixelFormat.Format32bppArgb) {
			Bitmap bitmap=new Bitmap(width,height,pixelFormat);
			for(int y=0;y<height;y++) {
				for(int x=0;x<width;x++) {
					int v=Math.Max(0,Math.Min(255,funcValue(x,y)));
					bitmap.SetPixel(x,y,Color.FromArgb(v,v,v));
				}
			}
			return bitmap;
		}

		private static int Value(Bitmap bitmap,int x,int y) {
			return bitmap.GetPixel(x,y).R;
		}

		private static double StandardDeviation(Bitmap bitmap) {
			double sum=0;
			double sumSquares=0;
			int count=bitmap.Width*bitmap.Height;
			for(int y=0;y<bitmap.Height;y++) {
				for(int x=0;x<bitmap.Width;x++) {
					int v=Value(bitmap,x,y);
					sum+=v;
					sumSquares+=v*v;
				}
			}
			double mean=sum/count;
			return Math.Sqrt(sumSquares/count-mean*mean);
		}

		[TestMethod]
		public void Neutral_ChangesNothing() {
			ImageAdjustSettings settings=new ImageAdjustSettings();
			Assert.IsTrue(settings.IsNeutral());
			Assert.IsTrue(new ImageAdjustSettings { Gamma=0 }.IsNeutral(),"Gamma 0 (a new row) reads as 1.");
			using Bitmap bitmap=Grey(20,20,(x,y) => x*12);
			ImageAdjust.Apply(bitmap,settings);
			Assert.AreEqual(0,Value(bitmap,0,0));
			Assert.AreEqual(228,Value(bitmap,19,5));
		}

		[TestMethod]
		public void Invert_AndGamma() {
			using Bitmap bitmap=Grey(4,1,(x,y) => new[] { 0,64,128,255 }[x]);
			ImageAdjust.Apply(bitmap,new ImageAdjustSettings { IsInverted=true });
			CollectionAssert.AreEqual(new[] { 255,191,127,0 },new[] { Value(bitmap,0,0),Value(bitmap,1,0),Value(bitmap,2,0),Value(bitmap,3,0) });
			using Bitmap bitmapGamma=Grey(3,1,(x,y) => new[] { 0,128,255 }[x]);
			ImageAdjust.Apply(bitmapGamma,new ImageAdjustSettings { Gamma=2 });
			Assert.AreEqual(0,Value(bitmapGamma,0,0),"Black stays black.");
			Assert.AreEqual(255,Value(bitmapGamma,2,0),"White stays white.");
			Assert.AreEqual(181,Value(bitmapGamma,1,0),"Gamma 2 lifts the mid-grey: 255*(128/255)^(1/2).");
		}

		[TestMethod]
		public void Smooth_RemovesASinglePixelOfNoise() {
			using Bitmap bitmap=Grey(9,9,(x,y) => x==4 && y==4 ? 255 : 100);
			ImageAdjust.Apply(bitmap,new ImageAdjustSettings { Smooth=ImageSmooth.Low });
			Assert.AreEqual(100,Value(bitmap,4,4));
			using Bitmap bitmapEdge=Grey(9,9,(x,y) => x<4 ? 20 : 220);
			ImageAdjust.Apply(bitmapEdge,new ImageAdjustSettings { Smooth=ImageSmooth.High });
			Assert.AreEqual(20,Value(bitmapEdge,1,4),"A median keeps a straight edge.");
			Assert.AreEqual(220,Value(bitmapEdge,7,4));
		}

		[TestMethod]
		public void Sharpen_RaisesTheContrastAtAnEdge() {
			using Bitmap bitmap=Grey(40,10,(x,y) => x<20 ? 80 : 160);
			ImageAdjust.Apply(bitmap,new ImageAdjustSettings { Sharpen=100 });
			Assert.IsTrue(Value(bitmap,19,5)<80,"The dark side of the edge gets darker: "+Value(bitmap,19,5));
			Assert.IsTrue(Value(bitmap,20,5)>160,"The light side gets lighter: "+Value(bitmap,20,5));
			Assert.AreEqual(80,Value(bitmap,2,5),"Flat areas far from the edge stay.");
		}

		[TestMethod]
		public void AutoContrast_SpreadsALowContrastPicture() {
			using Bitmap bitmap=Grey(64,64,(x,y) => 110+(x+y)%20);
			double before=StandardDeviation(bitmap);
			ImageAdjust.Apply(bitmap,new ImageAdjustSettings { IsAutoContrast=true });
			double after=StandardDeviation(bitmap);
			Assert.IsTrue(after>before*1.5,"Standard deviation "+before.ToString("F1")+" to "+after.ToString("F1"));
		}

		[TestMethod]
		public void GreyStaysGrey_ColourKeepsItsHue_24BitToo() {
			using Bitmap bitmap=Grey(30,30,(x,y) => x*8,PixelFormat.Format24bppRgb);
			ImageAdjust.Apply(bitmap,new ImageAdjustSettings { Sharpen=60,IsAutoContrast=true,Gamma=1.4 });
			for(int x=0;x<30;x++) {
				Color color=bitmap.GetPixel(x,15);
				Assert.IsTrue(color.R==color.G && color.G==color.B,"Grey at x="+x+": "+color);
			}
			using Bitmap bitmapColour=new Bitmap(16,16,PixelFormat.Format32bppArgb);
			for(int y=0;y<16;y++) {
				for(int x=0;x<16;x++) {
					bitmapColour.SetPixel(x,y,Color.FromArgb(180+x,90,60));
				}
			}
			ImageAdjust.Apply(bitmapColour,new ImageAdjustSettings { IsInverted=true });
			Assert.AreEqual(Color.FromArgb(255-180,255-90,255-60).ToArgb(),bitmapColour.GetPixel(0,0).ToArgb());
		}

		[TestMethod]
		public void ASensorSizedPicture_TakesUnderTwoSecondsWithEveryFilter() {
			using Bitmap bitmap=new Bitmap(1300,1800,PixelFormat.Format32bppArgb);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				using System.Drawing.Drawing2D.LinearGradientBrush brush=new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0,0,1300,1800),Color.Black,Color.White,45f);
				g.FillRectangle(brush,0,0,1300,1800);
			}
			Stopwatch stopwatch=Stopwatch.StartNew();
			ImageAdjust.Apply(bitmap,new ImageAdjustSettings { Smooth=ImageSmooth.High,IsAutoContrast=true,Sharpen=50,Gamma=1.2,IsInverted=true });
			stopwatch.Stop();
			Assert.IsTrue(stopwatch.ElapsedMilliseconds<2000,"Every filter on 1300x1800: "+stopwatch.ElapsedMilliseconds+" ms.");
		}
	}
}
