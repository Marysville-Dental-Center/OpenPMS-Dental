// Pictures of the image viewer window (imaging design P1, owner 2026-11-22) for the owner's gallery. Opt-in: OPMS_IMAGE_VIEWER_GALLERY names
// the folder. The window is filled with synthetic images through its private fields, as FitGallery fills windows with sample content: no
// program, no database. Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class ImageViewerGallery {
		private const string VARIABLE_FOLDER="OPMS_IMAGE_VIEWER_GALLERY";

		[TestMethod]
		public void Gallery_ImageViewerWindow() {
			string folder=Environment.GetEnvironmentVariable(VARIABLE_FOLDER);
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: "+VARIABLE_FOLDER+" names the folder for the image viewer pictures.");
			}
			Directory.CreateDirectory(folder);
			List<string> listFailed=new List<string>();
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				Save(folder,"viewer-mount-item",listFailed,MakeXray(1300,1800),Color.Black,new List<int> { 0,1,2,3 },2,1f,PointF.Empty,
					"Test, Sensor - 11/22/2026: BWX",
					"Image 3 of 4   |   Taken 11/22/2026, Teeth: 12-14   |   Left/Right: previous/next image   |   Wheel: zoom, Home: fit   |   Double-click or Esc: close");
				Save(folder,"viewer-mount-item-zoomed",listFailed,MakeXray(1300,1800),Color.Black,new List<int> { 0,1,2,3 },2,2.5f,new PointF(60,-40),
					"Test, Sensor - 11/22/2026: BWX",
					"Image 3 of 4   |   Taken 11/22/2026, Teeth: 12-14   |   Left/Right: previous/next image   |   Wheel: zoom, Home: fit   |   Double-click or Esc: close");
				Save(folder,"viewer-single-image",listFailed,MakeXray(1800,1300),Color.White,new List<int> { 0 },0,1f,PointF.Empty,
					"Test, Sensor - 11/22/2026: PA",
					"Tooth: 8   |   Wheel: zoom, Home: fit   |   Double-click or Esc: close");
				Save(folder,"viewer-not-loaded",listFailed,null,Color.Black,new List<int> { 0 },0,1f,PointF.Empty,
					"Test, Sensor - 11/22/2026: BWX",
					"Wheel: zoom, Home: fit   |   Double-click or Esc: close");
			});
			File.WriteAllLines(Path.Combine(folder,"gallery-failed.txt"),listFailed);
			Assert.AreEqual(0,listFailed.Count,string.Join("; ",listFailed));
		}

		private static void Save(string folder,string name,List<string> listFailed,Bitmap bitmap,Color colorBack,List<int> listIdxs,int idxShowing,
			float zoom,PointF pointFPan,string title,string info)
		{
			try {
				using FormImageViewer formImageViewer=new FormImageViewer();
				formImageViewer.Size=new Size(900,700);
				SetField(formImageViewer,"_bitmap",bitmap);
				SetField(formImageViewer,"_listIdxs",listIdxs);
				SetField(formImageViewer,"_idxShowing",idxShowing);
				SetField(formImageViewer,"_zoom",zoom);
				SetField(formImageViewer,"_pointFPan",pointFPan);
				formImageViewer.Text=title;
				((Label)GetField(formImageViewer,"labelInfo")).Text=info;
				((Control)GetField(formImageViewer,"canvas")).BackColor=colorBack;
				using Bitmap bitmapPicture=DesignSheetWindows.Capture(formImageViewer,Rectangle.Empty);
				bitmapPicture.Save(Path.Combine(folder,name+".png"),ImageFormat.Png);
			}
			catch(Exception exception) {
				listFailed.Add(name+": "+exception.Message);
			}
		}

		private static object GetField(object target,string name) {
			return target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);
		}

		private static void SetField(object target,string name,object value) {
			target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(target,value);
		}

		///<summary>A synthetic bitewing-like picture: dark film, three bright crowns with roots and a darker bone line. Not a real radiograph.</summary>
		private static Bitmap MakeXray(int width,int height) {
			Bitmap bitmap=new Bitmap(width,height);
			using Graphics g=Graphics.FromImage(bitmap);
			g.SmoothingMode=SmoothingMode.AntiAlias;
			using(LinearGradientBrush brushFilm=new LinearGradientBrush(new Rectangle(0,0,width,height),Color.FromArgb(40,42,46),Color.FromArgb(78,82,88),90f)) {
				g.FillRectangle(brushFilm,0,0,width,height);
			}
			bool isWide=width>height;
			int count=3;
			for(int i=0;i<count;i++) {
				float xCentre=width*(i+1f)/(count+1);
				float widthTooth=width/(count+1.6f);
				float yCrown=isWide ? height*0.18f : height*0.22f;
				float heightCrown=height*0.30f;
				using GraphicsPath pathCrown=new GraphicsPath();
				pathCrown.AddEllipse(xCentre-widthTooth/2,yCrown,widthTooth,heightCrown);
				using PathGradientBrush brushCrown=new PathGradientBrush(pathCrown) { CenterColor=Color.FromArgb(238,240,242),SurroundColors=new[] { Color.FromArgb(150,155,160) } };
				g.FillPath(brushCrown,pathCrown);
				using SolidBrush brushRoot=new SolidBrush(Color.FromArgb(170,174,178));
				g.FillPolygon(brushRoot,new[] {
					new PointF(xCentre-widthTooth*0.32f,yCrown+heightCrown*0.8f),new PointF(xCentre+widthTooth*0.32f,yCrown+heightCrown*0.8f),
					new PointF(xCentre+widthTooth*0.08f,height*0.92f),new PointF(xCentre-widthTooth*0.08f,height*0.92f) });
			}
			using SolidBrush brushBone=new SolidBrush(Color.FromArgb(70,120,124,128));
			g.FillRectangle(brushBone,0,height*0.62f,width,height*0.38f);
			return bitmap;
		}
	}
}
