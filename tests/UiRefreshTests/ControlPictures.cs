// Off-screen pictures of the real controls (package UI-4a, docs/port-notes/UI-4a.md): WinForms controls through DrawToBitmap, WPF controls through
// WPF's software renderer (RenderTargetBitmap), both without a window. Used by the control state tests, the eForm test and the gallery strips.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace UiRefreshTests {
	public static class ControlPictures {
		///<summary>The folder for pictures a test writes (failures, the gallery strips): OPMS_UI_STATES_OUT, or "ui4a-states/name" in the test's
		///output folder.</summary>
		public static string GetOutFolder(string name) {
			string folder=Environment.GetEnvironmentVariable("OPMS_UI_STATES_OUT");
			if(string.IsNullOrEmpty(folder)) {
				folder=Path.Combine(AppContext.BaseDirectory,"ui4a-states");
			}
			folder=Path.Combine(folder,name);
			Directory.CreateDirectory(folder);
			return folder;
		}

		///<summary>Whether WPF renders in this session: a filled square in a RenderTargetBitmap (a session WPF does not draw in leaves it empty).
		///STA thread only.</summary>
		public static bool RendersWpf() {
			System.Windows.Media.DrawingVisual drawingVisual=new System.Windows.Media.DrawingVisual();
			using(System.Windows.Media.DrawingContext drawingContext=drawingVisual.RenderOpen()) {
				drawingContext.DrawRectangle(System.Windows.Media.Brushes.Black,null,new System.Windows.Rect(2,2,12,12));
			}
			using(Bitmap bitmap=RenderVisual(drawingVisual,16,16,1)) {
				return bitmap.GetPixel(8,8).A>=128;
			}
		}

		///<summary>A WPF element laid out at width x height DIP and rendered at the scale (96 DPI times scale) on a white background, as a 32-bit
		///GDI+ bitmap of whole device pixels (rounded up). The element is measured and arranged here when it has no parent. STA thread only.</summary>
		public static Bitmap RenderWpf(System.Windows.FrameworkElement frameworkElement,double width,double height,float scale) {
			if(frameworkElement.Parent==null && System.Windows.Media.VisualTreeHelper.GetParent(frameworkElement)==null) {
				frameworkElement.Measure(new System.Windows.Size(width,height));
				frameworkElement.Arrange(new System.Windows.Rect(new System.Windows.Size(width,height)));
				frameworkElement.UpdateLayout();
			}
			//Whole pixels rounded up, so an edge that ends on a half pixel (22 DIP at 175% is 38.5 px) is not cut off.
			int widthPixels=(int)Math.Ceiling(width*scale-0.001);
			int heightPixels=(int)Math.Ceiling(height*scale-0.001);
			System.Windows.Media.DrawingVisual drawingVisualBack=new System.Windows.Media.DrawingVisual();
			using(System.Windows.Media.DrawingContext drawingContext=drawingVisualBack.RenderOpen()) {
				drawingContext.DrawRectangle(System.Windows.Media.Brushes.White,null,new System.Windows.Rect(0,0,width,height));
			}
			return RenderVisual(new System.Windows.Media.Visual[] { drawingVisualBack,frameworkElement },widthPixels,heightPixels,scale);
		}

		///<summary>A visual rendered into widthPixels x heightPixels at 96 DPI times scale, as a 32-bit GDI+ bitmap.</summary>
		public static Bitmap RenderVisual(System.Windows.Media.Visual visual,int widthPixels,int heightPixels,float scale) {
			return RenderVisual(new System.Windows.Media.Visual[] { visual },widthPixels,heightPixels,scale);
		}

		///<summary>Visuals rendered one over the other into widthPixels x heightPixels at 96 DPI times scale, as a 32-bit GDI+ bitmap.</summary>
		public static Bitmap RenderVisual(System.Windows.Media.Visual[] arrayVisuals,int widthPixels,int heightPixels,float scale) {
			System.Windows.Media.Imaging.RenderTargetBitmap renderTargetBitmap=new System.Windows.Media.Imaging.RenderTargetBitmap(widthPixels,heightPixels,
				96*scale,96*scale,System.Windows.Media.PixelFormats.Pbgra32);
			foreach(System.Windows.Media.Visual visual in arrayVisuals) {
				renderTargetBitmap.Render(visual);
			}
			byte[] arrayBytes=new byte[widthPixels*heightPixels*4];
			renderTargetBitmap.CopyPixels(arrayBytes,widthPixels*4,0);
			Bitmap bitmap=new Bitmap(widthPixels,heightPixels,PixelFormat.Format32bppPArgb);
			BitmapData bitmapData=bitmap.LockBits(new Rectangle(0,0,widthPixels,heightPixels),ImageLockMode.WriteOnly,PixelFormat.Format32bppPArgb);
			for(int y=0;y<heightPixels;y++) {
				Marshal.Copy(arrayBytes,y*widthPixels*4,bitmapData.Scan0+y*bitmapData.Stride,widthPixels*4);
			}
			bitmap.UnlockBits(bitmapData);
			Bitmap bitmapArgb=new Bitmap(widthPixels,heightPixels,PixelFormat.Format32bppArgb);
			using(Graphics g=Graphics.FromImage(bitmapArgb)) {
				g.DrawImageUnscaled(bitmap,0,0);
			}
			bitmap.Dispose();
			return bitmapArgb;
		}
	}
}
