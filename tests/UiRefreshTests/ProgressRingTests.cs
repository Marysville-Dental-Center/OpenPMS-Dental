// Package UI-LOAD (docs/port-notes/UI-LOAD.md): the vector progress ring (CodeBase.UiProgressRing) that replaced the animated GIFs the program
// showed while it waited. Its drawing at 100% and 150% (pixel checks: the ring's square, the arc in Accent where the frame puts it, nothing
// outside the square), the static ring when Windows' animations are off, Windows' colours in high contrast, the arc's cycle, the timer (it runs
// only while the ring is visible and stops when it is hidden or disposed, and never runs with animations off), the graph's IsLoading, and the
// scan that fails on an animated GIF anywhere in the product's folders or on code that animates a picture (ImageAnimator.Animate). The gallery
// G-UI3-uiload draws its places through FitGallery (ProgressRingGallery.Prepare: every ring at GALLERY_FRAME; OPMS_UI_LOAD_GALLERY_BEFORE
// draws the before pictures, the old indicator in the ring's place). No window is shown; no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiRefreshTests.Scan;
using UiRefreshTests.TextFit;

namespace UiRefreshTests {
	///<summary>The gallery's preparation of the rings in a built window (FitGallery, after its state).</summary>
	public static class ProgressRingGallery {
		///<summary>The frame every ring is drawn at in the gallery: a quarter of the cycle, a 140 degree arc from 20 to 160 degrees (its middle at
		///six o'clock).</summary>
		public const int GALLERY_FRAME=15;
		///<summary>Set: the before pictures. Each ring is hidden, as the old indicator drew nothing of its own there (the allocator warning's
		///animator never loaded a frame), and the graph's loading state gets the old picture box back (Dock Fill, CenterImage) with the GIF this
		///variable names (loadingAnim.gif from the branch point), in front, as GraphQuantityOverTime's designer had it.</summary>
		public const string VARIABLE_BEFORE="OPMS_UI_LOAD_GALLERY_BEFORE";

		public static void Prepare(Control root) {
			string before=Environment.GetEnvironmentVariable(VARIABLE_BEFORE);
			bool isBefore=!string.IsNullOrEmpty(before);
			foreach(UiProgressRing uiProgressRing in FinalSweep.Walk(root).OfType<UiProgressRing>().ToList()) {
				uiProgressRing.Frame=GALLERY_FRAME;
				if(!isBefore) {
					continue;
				}
				bool isShowing=TextFitBoundsRecord.IsDesignedVisible(uiProgressRing);
				uiProgressRing.Visible=false;
				if(isShowing && uiProgressRing.Name=="progressRingLoading" && File.Exists(before)) {
					PictureBox pictureBox=new PictureBox();
					pictureBox.Dock=DockStyle.Fill;
					pictureBox.SizeMode=PictureBoxSizeMode.CenterImage;
					pictureBox.Image=Image.FromFile(before);
					pictureBox.Name="pictureBoxLoading";
					uiProgressRing.Parent.Controls.Add(pictureBox);
					pictureBox.BringToFront();
				}
			}
		}
	}

	[TestClass]
	public class ProgressRingTests {
		private static readonly Color _colorBack=Color.White;

		#region Drawing
		///<summary>Draws the ring into a white square of the size at the scale and frame.</summary>
		private static Bitmap Draw(int size,float scale,int frame,bool isAnimated,bool isHighContrast) {
			Bitmap bitmap=new Bitmap(size,size,PixelFormat.Format24bppRgb);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(_colorBack);
				UiProgressRing.DrawRing(g,new Rectangle(0,0,size,size),scale,frame,isAnimated,isHighContrast);
			}
			return bitmap;
		}

		private static bool IsSame(Color color1,Color color2) {
			return color1.R==color2.R && color1.G==color2.G && color1.B==color2.B;
		}

		///<summary>The pixel on the stroke's middle line at the angle (GDI+'s degrees, clockwise from three o'clock).</summary>
		private static Point GetStrokePoint(Rectangle rectangleRing,int widthStroke,double angle) {
			double radius=(rectangleRing.Width-widthStroke)/2.0;
			double centreX=rectangleRing.X+rectangleRing.Width/2.0;
			double centreY=rectangleRing.Y+rectangleRing.Height/2.0;
			double radians=angle*Math.PI/180;
			return new Point((int)Math.Floor(centreX+radius*Math.Cos(radians)),(int)Math.Floor(centreY+radius*Math.Sin(radians)));
		}

		[TestMethod]
		public void Ring_Geometry_FluentSizesScaled() {
			Assert.AreEqual(new Rectangle(16,16,32,32),UiProgressRing.GetRingRectangle(new Rectangle(0,0,64,64),1f));
			Assert.AreEqual(new Rectangle(8,8,48,48),UiProgressRing.GetRingRectangle(new Rectangle(0,0,64,64),1.5f));
			Assert.AreEqual(4,UiProgressRing.GetStrokeWidth(1f,32));
			Assert.AreEqual(6,UiProgressRing.GetStrokeWidth(1.5f,48));
			//Never larger than the control: a 20 px control holds a 20 px ring with a 4 px stroke at most a quarter of it.
			Assert.AreEqual(new Rectangle(5,0,20,20),UiProgressRing.GetRingRectangle(new Rectangle(0,0,30,20),1f));
			Assert.AreEqual(4,UiProgressRing.GetStrokeWidth(1f,20));
			Assert.AreEqual(2,UiProgressRing.GetStrokeWidth(1f,8));
		}

		[TestMethod]
		public void Ring_Arc_CyclesAndBreathes() {
			UiProgressRing.GetArc(0,out float start0,out float sweep0);
			UiProgressRing.GetArc(UiProgressRing.FRAMES_CYCLE,out float startCycle,out float sweepCycle);
			Assert.AreEqual(start0,startCycle,0.001,"frame FRAMES_CYCLE is frame 0 again");
			Assert.AreEqual(sweep0,sweepCycle,0.001);
			Assert.AreEqual(UiProgressRing.SWEEP_MIN,sweep0,0.001);
			UiProgressRing.GetArc(UiProgressRing.FRAMES_CYCLE/2,out float _,out float sweepHalf);
			Assert.AreEqual(UiProgressRing.SWEEP_MAX,sweepHalf,0.001);
			UiProgressRing.GetArc(ProgressRingGallery.GALLERY_FRAME,out float startGallery,out float sweepGallery);
			Assert.AreEqual(140f,sweepGallery,0.001);
			Assert.AreEqual(20f,startGallery,0.001);
			//Each frame moves both ends of the arc a little and always clockwise: forward, never more than 20 degrees.
			for(int frame=0;frame<UiProgressRing.FRAMES_CYCLE;frame++) {
				UiProgressRing.GetArc(frame,out float start1,out float sweep1);
				UiProgressRing.GetArc(frame+1,out float start2,out float sweep2);
				double deltaStart=((start2-start1)%360+540)%360-180;
				double deltaEnd=((start2+sweep2-start1-sweep1)%360+540)%360-180;
				Assert.IsTrue(deltaStart>0 && deltaStart<=20 && deltaEnd>0 && deltaEnd<=20,"frame "+frame+": "+deltaStart+", "+deltaEnd);
			}
		}

		[TestMethod]
		public void Ring_Draw_100And150_ArcInAccentInsideItsSquare() {
			Color colorAccent=UiTheme.GetColor(EnumUiColor.Accent,false);
			foreach(float scale in new[] { 1f,1.5f }) {
				using(Bitmap bitmap=Draw(64,scale,ProgressRingGallery.GALLERY_FRAME,isAnimated:true,isHighContrast:false)) {
					Rectangle rectangleRing=UiProgressRing.GetRingRectangle(new Rectangle(0,0,64,64),scale);
					int widthStroke=UiProgressRing.GetStrokeWidth(scale,rectangleRing.Width);
					//The arc runs from 20 to 160 degrees: its middle (six o'clock) and both quarters inside it are solid Accent.
					foreach(double angle in new[] { 90.0,50.0,130.0 }) {
						Point point=GetStrokePoint(rectangleRing,widthStroke,angle);
						Assert.IsTrue(IsSame(colorAccent,bitmap.GetPixel(point.X,point.Y)),scale+" at "+angle+": "+bitmap.GetPixel(point.X,point.Y));
					}
					//Outside the arc the ring's line stays the background: twelve o'clock, nine and three o'clock less the round caps.
					foreach(double angle in new[] { 270.0,200.0,340.0 }) {
						Point point=GetStrokePoint(rectangleRing,widthStroke,angle);
						Assert.IsTrue(IsSame(_colorBack,bitmap.GetPixel(point.X,point.Y)),scale+" at "+angle+": "+bitmap.GetPixel(point.X,point.Y));
					}
					//The centre is the background, and nothing is drawn outside the ring's square.
					Assert.IsTrue(IsSame(_colorBack,bitmap.GetPixel(32,32)));
					int countOutside=0;
					int countAccent=0;
					for(int x=0;x<64;x++) {
						for(int y=0;y<64;y++) {
							Color color=bitmap.GetPixel(x,y);
							if(!rectangleRing.Contains(x,y) && !IsSame(_colorBack,color)) {
								countOutside++;
							}
							if(IsSame(colorAccent,color)) {
								countAccent++;
							}
						}
					}
					Assert.AreEqual(0,countOutside,"pixels outside the ring at "+scale);
					//The solid arc grows with the square of the scale: a 140 degree arc of the stroke's width, less its anti-aliased edges.
					double areaArc=Math.PI*(rectangleRing.Width-widthStroke)*widthStroke*140/360;
					Assert.IsTrue(countAccent>areaArc*0.5 && countAccent<areaArc*1.3,"Accent pixels at "+scale+": "+countAccent+" of about "+areaArc);
				}
			}
		}

		[TestMethod]
		public void Ring_Draw_AnimationsOff_StaticRing() {
			Color colorAccent=UiTheme.GetColor(EnumUiColor.Accent,false);
			Color colorTrack=UiTheme.GetColor(EnumUiColor.AccentSubtle,false);
			foreach(float scale in new[] { 1f,1.5f }) {
				//Whatever the frame: the static ring does not move.
				using(Bitmap bitmap=Draw(64,scale,ProgressRingGallery.GALLERY_FRAME,isAnimated:false,isHighContrast:false))
				using(Bitmap bitmapOther=Draw(64,scale,3,isAnimated:false,isHighContrast:false)) {
					Rectangle rectangleRing=UiProgressRing.GetRingRectangle(new Rectangle(0,0,64,64),scale);
					int widthStroke=UiProgressRing.GetStrokeWidth(scale,rectangleRing.Width);
					//The quarter arc from twelve to three o'clock in Accent, the rest of the circle the AccentSubtle track.
					Point pointArc=GetStrokePoint(rectangleRing,widthStroke,-45);
					Assert.IsTrue(IsSame(colorAccent,bitmap.GetPixel(pointArc.X,pointArc.Y)),scale+": "+bitmap.GetPixel(pointArc.X,pointArc.Y));
					foreach(double angle in new[] { 90.0,180.0,135.0 }) {
						Point point=GetStrokePoint(rectangleRing,widthStroke,angle);
						Assert.IsTrue(IsSame(colorTrack,bitmap.GetPixel(point.X,point.Y)),scale+" at "+angle+": "+bitmap.GetPixel(point.X,point.Y));
					}
					for(int x=0;x<64;x++) {
						for(int y=0;y<64;y++) {
							Assert.AreEqual(bitmap.GetPixel(x,y),bitmapOther.GetPixel(x,y),"the static ring moved at "+x+","+y);
						}
					}
				}
			}
		}

		[TestMethod]
		public void Ring_Draw_HighContrast_WindowsColours() {
			Color colorHighlight=SystemColors.Highlight;
			using(Bitmap bitmap=Draw(64,1f,ProgressRingGallery.GALLERY_FRAME,isAnimated:true,isHighContrast:true)) {
				Rectangle rectangleRing=UiProgressRing.GetRingRectangle(new Rectangle(0,0,64,64),1f);
				Point point=GetStrokePoint(rectangleRing,4,90);
				Assert.IsTrue(IsSame(colorHighlight,bitmap.GetPixel(point.X,point.Y)),bitmap.GetPixel(point.X,point.Y).ToString());
			}
			using(Bitmap bitmap=Draw(64,1f,0,isAnimated:false,isHighContrast:true)) {
				Rectangle rectangleRing=UiProgressRing.GetRingRectangle(new Rectangle(0,0,64,64),1f);
				Point pointArc=GetStrokePoint(rectangleRing,4,-45);
				Point pointTrack=GetStrokePoint(rectangleRing,4,135);
				Assert.IsTrue(IsSame(colorHighlight,bitmap.GetPixel(pointArc.X,pointArc.Y)));
				Assert.IsTrue(IsSame(SystemColors.WindowFrame,bitmap.GetPixel(pointTrack.X,pointTrack.Y)));
			}
		}

		[TestMethod]
		public void Ring_Control_PaintsAtItsFrame() {
			StaRunner.Run(() => {
				using(UiProgressRing uiProgressRing=new UiProgressRing()) {
					uiProgressRing.Size=new Size(64,64);
					uiProgressRing.BackColor=_colorBack;
					uiProgressRing.Frame=ProgressRingGallery.GALLERY_FRAME+UiProgressRing.FRAMES_CYCLE;
					Assert.AreEqual(ProgressRingGallery.GALLERY_FRAME,uiProgressRing.Frame,"the frame wraps");
					uiProgressRing.Frame=-1;
					Assert.AreEqual(UiProgressRing.FRAMES_CYCLE-1,uiProgressRing.Frame);
					uiProgressRing.Frame=ProgressRingGallery.GALLERY_FRAME;
					UiProgressRing.SetAnimationForTest(true);
					try {
						using(Bitmap bitmap=new Bitmap(64,64,PixelFormat.Format24bppRgb)) {
							uiProgressRing.DrawToBitmap(bitmap,new Rectangle(0,0,64,64));
							float scale=UiLooks.GetScale(uiProgressRing);
							using(Bitmap bitmapExpected=Draw(64,scale,ProgressRingGallery.GALLERY_FRAME,isAnimated:true,isHighContrast:UiTheme.IsHighContrast)) {
								if(!UiTheme.IsHighContrast) {
									for(int x=0;x<64;x++) {
										for(int y=0;y<64;y++) {
											Assert.AreEqual(bitmapExpected.GetPixel(x,y),bitmap.GetPixel(x,y),"at "+x+","+y);
										}
									}
								}
							}
						}
					}
					finally {
						UiProgressRing.SetAnimationForTest(null);
					}
				}
			});
		}
		#endregion Drawing

		#region Timer
		[TestMethod]
		public void Ring_Timer_OnlyWhileVisible_StopsWhenHiddenOrDisposed() {
			StaRunner.Run(() => {
				UiProgressRing.SetAnimationForTest(true);
				try {
					UiProgressRing uiProgressRing=new UiProgressRing();
					Assert.IsTrue(uiProgressRing.Visible,"a ring without a parent is visible");
					uiProgressRing.UpdateAnimation();
					Assert.IsTrue(uiProgressRing.IsAnimating,"visible: the timer runs");
					uiProgressRing.Visible=false;
					Assert.IsFalse(uiProgressRing.IsAnimating,"hidden: the timer stops");
					uiProgressRing.Visible=true;
					Assert.IsTrue(uiProgressRing.IsAnimating,"shown again: it runs again");
					//In a window that is not shown the ring is not visible (Visible asks the parents): no timer.
					using(Form form=new Form()) {
						form.Controls.Add(uiProgressRing);
						Assert.IsFalse(uiProgressRing.Visible);
						Assert.IsFalse(uiProgressRing.IsAnimating,"in a hidden window: no timer");
						form.Controls.Remove(uiProgressRing);
					}
					Assert.IsTrue(uiProgressRing.IsAnimating,"out of the window, visible again: it runs");
					uiProgressRing.Dispose();
					Assert.IsFalse(uiProgressRing.IsAnimating,"disposed: the timer stops");
				}
				finally {
					UiProgressRing.SetAnimationForTest(null);
				}
			});
		}

		[TestMethod]
		public void Ring_Timer_AnimationsOff_NeverRuns() {
			StaRunner.Run(() => {
				UiProgressRing.SetAnimationForTest(false);
				try {
					using(UiProgressRing uiProgressRing=new UiProgressRing()) {
						uiProgressRing.UpdateAnimation();
						Assert.IsTrue(uiProgressRing.Visible);
						Assert.IsFalse(uiProgressRing.IsAnimating,"animations off: no timer");
						uiProgressRing.Visible=false;
						uiProgressRing.Visible=true;
						Assert.IsFalse(uiProgressRing.IsAnimating);
						//Animations turned on again: the next visibility change starts it.
						UiProgressRing.SetAnimationForTest(true);
						uiProgressRing.UpdateAnimation();
						Assert.IsTrue(uiProgressRing.IsAnimating);
						UiProgressRing.SetAnimationForTest(false);
						uiProgressRing.UpdateAnimation();
						Assert.IsFalse(uiProgressRing.IsAnimating);
					}
				}
				finally {
					UiProgressRing.SetAnimationForTest(null);
				}
			});
		}
		#endregion Timer

		#region The places
		///<summary>GraphQuantityOverTime shows the ring while IsLoading, in front of the chart as the picture was, and hides it otherwise.</summary>
		[TestMethod]
		public void Graph_IsLoading_ShowsTheRingInFrontOfTheChart() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				UiProgressRing.SetAnimationForTest(true);
				try {
					Assembly assembly=WindowBuilder.LoadProgramAssembly("OpenPmsDentalGraph");
					Type type=assembly.GetType("OpenPmsDentalGraph.GraphQuantityOverTime",throwOnError:true);
					using(UserControl userControl=(UserControl)Activator.CreateInstance(type)) {
						UiProgressRing uiProgressRing=FinalSweep.Walk(userControl).OfType<UiProgressRing>().Single();
						Assert.AreEqual("progressRingLoading",uiProgressRing.Name);
						Control chart=FinalSweep.Walk(userControl).First(x => x.Name=="chart1");
						Assert.AreSame(chart.Parent,uiProgressRing.Parent);
						Assert.IsFalse(uiProgressRing.Visible,"not loading: hidden");
						PropertyInfo propertyInfo=type.GetProperty("IsLoading");
						propertyInfo.SetValue(userControl,true);
						Assert.IsTrue(uiProgressRing.Visible,"loading: shown");
						Assert.AreEqual(0,uiProgressRing.Parent.Controls.IndexOf(uiProgressRing),"loading: in front of the chart");
						Assert.IsTrue(uiProgressRing.IsAnimating,"loading: turning");
						Assert.AreEqual(DockStyle.Fill,uiProgressRing.Dock);
						propertyInfo.SetValue(userControl,false);
						Assert.IsFalse(uiProgressRing.Visible,"loaded: hidden");
						Assert.IsFalse(uiProgressRing.IsAnimating,"loaded: the timer stopped");
						Assert.AreEqual(0,chart.Parent.Controls.IndexOf(chart),"loaded: the chart in front");
					}
				}
				finally {
					UiProgressRing.SetAnimationForTest(null);
				}
			});
		}

		///<summary>The allocator warning shows the ring where the frame animator was (50 x 50 beside "Close Other OD Programs Now!!").</summary>
		[TestMethod]
		public void WarnToCloseComputers_RingInTheAnimatorsPlace() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				Type type=typeof(OpenPmsDental.Reporting.Allocators.MyAllocator1.FormWarnToCloseComputers);
				ScanType scanType=WindowBuilder.GetScanTypes().Single(x => x.Type==type);
				using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
					UiProgressRing uiProgressRing=FinalSweep.Walk(builtWindow.Root).OfType<UiProgressRing>().Single();
					Assert.AreEqual("progressRingWait",uiProgressRing.Name);
					Assert.AreEqual("panel1",uiProgressRing.Parent.Name);
					Assert.IsFalse(uiProgressRing.TabStop);
					Assert.IsTrue(TextFitBoundsRecord.IsDesignedVisible(uiProgressRing));
				}
			});
		}
		#endregion The places

		#region The scan
		///<summary>Folders the scan skips: build output, tools' caches, the repository's own folders and the documentation.</summary>
		private static readonly string[] _arraySkipFolders=new string[] { "bin","obj",".git",".claude",".vs","packages","node_modules","TestResults",
			"docs","site" };

		private static IEnumerable<string> EnumerateFiles(string folder,string pattern) {
			foreach(string file in Directory.EnumerateFiles(folder,pattern)) {
				yield return file;
			}
			foreach(string folderChild in Directory.EnumerateDirectories(folder)) {
				string name=Path.GetFileName(folderChild);
				if(_arraySkipFolders.Contains(name,StringComparer.OrdinalIgnoreCase)) {
					continue;
				}
				foreach(string file in EnumerateFiles(folderChild,pattern)) {
					yield return file;
				}
			}
		}

		///<summary>No animated GIF is left in the repository's product and test folders (a busy indicator is the ring now, UI-LOAD): every .gif
		///has one frame.</summary>
		[TestMethod]
		public void Scan_NoAnimatedGif() {
			string root=RepoPaths.Root();
			List<string> listAnimated=new List<string>();
			int countGifs=0;
			foreach(string file in EnumerateFiles(root,"*.gif")) {
				countGifs++;
				try {
					using(Image image=Image.FromFile(file)) {
						if(image.FrameDimensionsList.Contains(FrameDimension.Time.Guid) && image.GetFrameCount(FrameDimension.Time)>1) {
							listAnimated.Add(file.Substring(root.Length));
						}
					}
				}
				catch(OutOfMemoryException) {
					//Not a picture GDI+ reads: not an animation it could show.
				}
			}
			Assert.IsTrue(countGifs>=40,"The scan found "+countGifs+" GIF files: the folders moved?");
			Assert.AreEqual(0,listAnimated.Count,"Animated GIFs (a busy indicator is CodeBase.UiProgressRing, UI-LOAD):\r\n"+string.Join("\r\n",listAnimated));
		}

		///<summary>No code animates a picture (System.Drawing.ImageAnimator.Animate), and no PictureBox gets the removed spinner; the ring is the
		///only busy indicator drawn in a picture's place.</summary>
		[TestMethod]
		public void Scan_NoPictureAnimation() {
			string root=RepoPaths.Root();
			Regex regex=new Regex(@"\bImageAnimator\s*\.\s*(Animate|UpdateFrames)\b|\bloadingAnim\b|\bSnuffLight\d");
			List<string> listFound=new List<string>();
			int countFiles=0;
			foreach(string file in EnumerateFiles(root,"*.cs")) {
				if(file.EndsWith("ProgressRingTests.cs",StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
				countFiles++;
				string text=File.ReadAllText(file);
				foreach(Match match in regex.Matches(text)) {
					listFound.Add(file.Substring(root.Length)+": "+match.Value);
				}
			}
			foreach(string file in EnumerateFiles(root,"*.resx").Concat(EnumerateFiles(root,"*.csproj")).Concat(EnumerateFiles(root,"*.xaml"))) {
				string text=File.ReadAllText(file);
				if(Regex.IsMatch(text,@"\bloadingAnim\b|SnuffLight\d")) {
					listFound.Add(file.Substring(root.Length));
				}
			}
			Assert.IsTrue(countFiles>=1000,"The scan read "+countFiles+" C# files: the folders moved?");
			Assert.AreEqual(0,listFound.Count,"Picture animation (a busy indicator is CodeBase.UiProgressRing, UI-LOAD):\r\n"+string.Join("\r\n",listFound));
		}
		#endregion The scan
	}
}
