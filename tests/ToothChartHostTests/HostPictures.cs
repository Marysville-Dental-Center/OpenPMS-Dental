// The checks of a host's chart picture (package TC-8, plan section 7, "Host tests"): its size, opacity (every alpha 255), the pixel format TC-0
// recorded for DirectX (Format24bppRgb), a picture that is not blank, and its baseline in Baselines\ (on .NET x64 Baselines\net10-x64, S3b-5W;
// HostPaths.Baselines) with the image tests' tolerance
// (ChartImageCompare: a pixel differs when a channel differs by more than 8 of 255; at most 0.1% of the pixels and no 16x16 block with more than
// 24). OPMS_RECORD_HOST_BASELINES=1 writes the baselines instead (a deliberate re-record only). A failing comparison writes the expected, actual
// and diff pictures next to the test assembly. OPMS_CHART_GALLERY=<folder> also writes each picture there for gallery G4 (TC-8's part).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToothChartImageTests;

namespace ToothChartHostTests {
	public static class HostPictures {
		public const string EnvRecord="OPMS_RECORD_HOST_BASELINES";
		public const string EnvGallery="OPMS_CHART_GALLERY";

		///<summary>The folder gallery G4's host pictures are written to (OPMS_CHART_GALLERY), or null when no gallery is made.</summary>
		public static string GalleryFolder {
			get {
				string folder=Environment.GetEnvironmentVariable(EnvGallery);
				if(string.IsNullOrWhiteSpace(folder)) {
					return null;
				}
				Directory.CreateDirectory(folder);
				return folder;
			}
		}

		///<summary>Checks one host's picture and compares it with its baseline (name.png in Baselines\), or records the baseline when asked. The WPF
		///chart's pictures have the pixel format TC-0 recorded for DirectX (Format24bppRgb); a 2D computer's picture (O-30) has the one TC-0 recorded
		///for the Simple 2D chart (Format32bppArgb, every alpha 255).</summary>
		public static void Check(string name,Image image,Size sizeExpected,PixelFormat pixelFormatExpected=PixelFormat.Format24bppRgb) {
			Assert.IsNotNull(image,name+": the host made no picture.");
			Bitmap bitmap=image as Bitmap;
			Assert.IsNotNull(bitmap,name+": the picture is a "+image.GetType().Name+", not a Bitmap.");
			Assert.AreEqual(sizeExpected,bitmap.Size,name+": size.");
			Assert.AreEqual(pixelFormatExpected,bitmap.PixelFormat,name+": the pixel format TC-0 recorded for its engine.");
			byte[] arrayBgra=ChartImageCompare.GetPixelsArgb(bitmap);
			for(int i=3;i<arrayBgra.Length;i+=4) {
				if(arrayBgra[i]!=255) {
					Assert.Fail(name+": not opaque (pixel "+(i/4)+" has alpha "+arrayBgra[i]+").");
				}
			}
			BitmapFacts bitmapFacts=ChartImageCompare.DescribeBitmap(bitmap);
			Assert.IsFalse(bitmapFacts.IsBlank,name+": blank.");
			Assert.IsTrue(bitmapFacts.CountColors>=50,name+": only "+bitmapFacts.CountColors+" colours; a chart picture has many more.");
			string galleryFolder=GalleryFolder;
			if(galleryFolder!=null) {
				PngWriter.Save(bitmap,Path.Combine(galleryFolder,name+".wpf.png"));
			}
			string pathBaseline=Path.Combine(HostPaths.Baselines,name+".png");
			if(Environment.GetEnvironmentVariable(EnvRecord)=="1") {
				Directory.CreateDirectory(HostPaths.Baselines);
				PngWriter.Save(bitmap,pathBaseline);
				File.WriteAllText(Path.ChangeExtension(pathBaseline,".txt"),"Host: "+name+"\r\nSize: "+bitmap.Width+"x"+bitmap.Height+"\r\nBitmap: "
					+bitmap.PixelFormat+"\r\nColours in the picture: "+bitmapFacts.CountColors+"\r\nPixels SHA-256: "+ChartImageCompare.Sha256(arrayBgra)
					+"\r\nFramework: "+System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription+", "+(Environment.Is64BitProcess?"x64":"x86")+"\r\n");
				return;
			}
			Assert.IsTrue(File.Exists(pathBaseline),name+": no baseline at "+pathBaseline+" (record with "+EnvRecord+"=1).");
			using(Bitmap bitmapBaseline=LoadBitmap(pathBaseline)) {
				ChartImageCompareResult result=ChartImageCompare.Compare(bitmapBaseline,bitmap);
				if(!result.IsPass) {
					string folderFailures=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"host-failures");
					Directory.CreateDirectory(folderFailures);
					ChartImageCompare.WriteFailureImages(folderFailures,name,bitmapBaseline,bitmap);
					Assert.Fail(name+" differs from its baseline: "+result+" (pictures in "+folderFailures+")");
				}
			}
		}

		///<summary>A copy of the picture at path that does not keep the file open, in 24-bit RGB.</summary>
		public static Bitmap LoadBitmap(string path) {
			using(MemoryStream memoryStream=new MemoryStream(File.ReadAllBytes(path)))
			using(Image image=Image.FromStream(memoryStream)) {
				Bitmap bitmap=new Bitmap(image.Width,image.Height,PixelFormat.Format24bppRgb);
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.DrawImageUnscaled(image,0,0);
				}
				return bitmap;
			}
		}
	}

	///<summary>Forms of the tests, shown only off the desktop, as tests/ToothChartImageTests does (WpfCharts.CreateOffscreenForm).</summary>
	public static class HostForms {
		public static System.Windows.Forms.Form CreateOffscreenForm(Size sizeClient) {
			System.Windows.Forms.Form form=new System.Windows.Forms.Form();
			form.FormBorderStyle=System.Windows.Forms.FormBorderStyle.None;
			form.ShowInTaskbar=false;
			form.StartPosition=System.Windows.Forms.FormStartPosition.Manual;
			form.Location=new Point(-32000,-32000);
			form.ClientSize=sizeClient;
			return form;
		}
	}
}
