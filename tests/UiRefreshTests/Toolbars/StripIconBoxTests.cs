// The UI-9b-4 fix's tests (docs/port-notes/UI-9b.md, section 5.8; the lead's audit of gallery G-UI3-ui9b4): every icon of an ImageList strip
// fills the same box at 100, 150 and 200%, whatever design sizes the icon has. Before the fix an icon was drawn at its largest design that fits
// the slot, never stretched up, so at 150 and 200% (33 and 44 px toolbar slots) the icons with a 32 px design (Copy, Paste, Save as Draft, the
// printer) were drawn in a 32 px box and their neighbours without one in a 24 px box. Now every icon of the slot has the box of section 3.5
// (UiTheme.GetIconDesignSize) and a missing design is scaled by the Fluent keylines (UiIcons.GetPlacement), so the ink of icons of one size
// class (the same ink extent in their 24 px design) is within 1 px across the strip. Also: the heading letters take the cap height of B and I,
// and the wiki's Rename draws Fluent Rename A. No window is shown, no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;

namespace UiRefreshTests.Toolbars {
	[TestClass]
	public class StripIconBoxTests {
		///<summary>The 96 DPI sizes of the program's ImageList strips: the toolbars' 22 px slot (ToolBarOD.SIZE_IMAGE_LIST96) and the trees', lists'
		///and grid columns' 16 px.</summary>
		private static readonly int[] _arraySizes96=new int[] { ToolBarOD.SIZE_IMAGE_LIST96,16 };
		private static readonly float[] _arrayScales=new float[] { 1f,1.5f,2f };

		///<summary>Every ImageList strip of the program (UI-9b): each static EnumIcons[] array named ArrayIcons* of OpenPmsDental, by its type and name.</summary>
		public static List<KeyValuePair<string,EnumIcons[]>> GetStrips() {
			List<KeyValuePair<string,EnumIcons[]>> listStrips=new List<KeyValuePair<string,EnumIcons[]>>();
			foreach(Type type in typeof(ToolBarOD).Assembly.GetTypes()) {
				foreach(FieldInfo fieldInfo in type.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.DeclaredOnly)) {
					if(fieldInfo.FieldType!=typeof(EnumIcons[]) || !fieldInfo.Name.StartsWith("ArrayIcons")) {
						continue;
					}
					EnumIcons[] arrayIcons=(EnumIcons[])fieldInfo.GetValue(null);
					if(arrayIcons==null || arrayIcons.Length==0) {
						continue;
					}
					listStrips.Add(new KeyValuePair<string,EnumIcons[]>(type.Name+"."+fieldInfo.Name,arrayIcons));
				}
			}
			return listStrips;
		}

		///<summary>The bounds of the pixels with at least half ink (alpha 128) of a picture on transparent; empty when there are none.</summary>
		public static Rectangle GetInkBounds(Bitmap bitmap) {
			int[] arrayInk=IconRendererTests.GetInk(bitmap,isOnWhite:false);
			int xMin=int.MaxValue;
			int yMin=int.MaxValue;
			int xMax=-1;
			int yMax=-1;
			for(int i=0;i<arrayInk.Length;i++) {
				if(arrayInk[i]<128) {
					continue;
				}
				int x=i%bitmap.Width;
				int y=i/bitmap.Width;
				xMin=Math.Min(xMin,x);
				yMin=Math.Min(yMin,y);
				xMax=Math.Max(xMax,x);
				yMax=Math.Max(yMax,y);
			}
			if(xMax<0) {
				return Rectangle.Empty;
			}
			return Rectangle.FromLTRB(xMin,yMin,xMax+1,yMax+1);
		}

		///<summary>The ink extent (the larger side of the ink bounds) of an icon drawn alone in a slot of sizePixels.</summary>
		private static int GetInkExtent(EnumIcons icon,int sizePixels) {
			using(Bitmap bitmap=IconLibrary.GetBitmap(icon,sizePixels,sizePixels)) {
				Rectangle rectangle=GetInkBounds(bitmap);
				return Math.Max(rectangle.Width,rectangle.Height);
			}
		}

		[TestMethod]
		public void Strips_AreFound() {
			List<KeyValuePair<string,EnumIcons[]>> listStrips=GetStrips();
			Assert.IsTrue(listStrips.Count>=30,"Strips found: "+listStrips.Count);
			Assert.IsTrue(listStrips.Any(x => x.Key=="FormEmailEdit.ArrayIconsImageList"));
			Assert.IsTrue(listStrips.Any(x => x.Key=="FormWiki.ArrayIconsImageList"));
			Assert.IsTrue(listStrips.Any(x => x.Key=="FormClaimsSend.ArrayIconsImageList"));
		}

		///<summary>The ink extent of a design drawn at its own size, as Fluent made it (no box, no keyline scale; its optical scale about the
		///centre): its size class. GDI+, antialiased, ink at half alpha and up.</summary>
		private static float GetNativeExtent(IconDesign iconDesign) {
			using(Bitmap bitmap=new Bitmap(iconDesign.Size,iconDesign.Size,System.Drawing.Imaging.PixelFormat.Format32bppArgb))
			using(System.Drawing.Drawing2D.GraphicsPath graphicsPath=new System.Drawing.Drawing2D.GraphicsPath(System.Drawing.Drawing2D.FillMode.Winding))
			using(Graphics g=Graphics.FromImage(bitmap)) {
				float offset=iconDesign.Size*(1f-iconDesign.OpticalScale)/2f;
				iconDesign.GetFigures().WriteTo(new PathSink(graphicsPath),offset,offset,iconDesign.OpticalScale);
				g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				g.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.Half;
				g.FillPath(Brushes.Black,graphicsPath);
				Rectangle rectangle=GetInkBounds(bitmap);
				return Math.Max(rectangle.Width,rectangle.Height);
			}
		}

		///<summary>Every icon of every strip, in the strip's ImageList at 100, 150 and 200% of the 22 and 16 px slots, is drawn in one box: the box of
		///section 3.5 for the slot (the same for every icon), its ink inside the box, and its ink extent within 1 px of its size class in that box:
		///the extent of the design it draws, at the design's own size, scaled by the Fluent keylines to the box (GetLiveSize). Before the fix an icon
		///whose largest design is 24 px drew it in a 24 px box in the 33 and 44 px slots, 8 px under its class's 28 px in the 32 px box.</summary>
		[TestMethod]
		public void StripIcons_FillOneBox_InkWithin1PxOfTheirSizeClass_At100_150_200() {
			List<string> listFaults=new List<string>();
			Dictionary<IconDesign,float> dictNative=new Dictionary<IconDesign,float>();
			int countChecked=0;
			foreach(KeyValuePair<string,EnumIcons[]> pair in GetStrips()) {
				foreach(int size96 in _arraySizes96) {
					foreach(float scale in _arrayScales) {
						int sizePixels=UiTheme.ScaleI(size96,scale);
						int sizeBox=Math.Min(UiTheme.GetIconDesignSize(sizePixels),sizePixels);
						Rectangle rectangleBox=new Rectangle((sizePixels-sizeBox)/2,(sizePixels-sizeBox)/2,sizeBox,sizeBox);
						using(ImageList imageList=IconLibrary.GetImageList(pair.Value,sizePixels)) {
							for(int i=0;i<pair.Value.Length;i++) {
								EnumIcons icon=pair.Value[i];
								string what=pair.Key+"["+i+"] "+icon+" at "+(int)Math.Round(scale*100)+"% of "+size96+" px";
								IconDesign iconDesign=IconLibrary.GetDesign(icon,sizePixels);
								Assert.IsNotNull(iconDesign,what);
								Assert.AreEqual(rectangleBox,UiIcons.GetDrawRectangle(iconDesign,new Rectangle(0,0,sizePixels,sizePixels)),what+": the box.");
								Rectangle rectangleInk;
								using(Bitmap bitmap=(Bitmap)imageList.Images[i]) {
									rectangleInk=GetInkBounds(bitmap);
								}
								Assert.IsFalse(rectangleInk.IsEmpty,what+": no ink.");
								if(!Rectangle.Inflate(rectangleBox,1,1).Contains(rectangleInk)) {
									listFaults.Add(what+": ink "+rectangleInk+" leaves the box "+rectangleBox);
								}
								if(!dictNative.ContainsKey(iconDesign)) {
									dictNative[iconDesign]=GetNativeExtent(iconDesign);
								}
								float extentClass=dictNative[iconDesign];
								if(iconDesign.Size!=sizeBox) {
									extentClass=extentClass*UiIcons.GetLiveSize(sizeBox)/UiIcons.GetLiveSize(iconDesign.Size);
								}
								int extent=Math.Max(rectangleInk.Width,rectangleInk.Height);
								if(Math.Abs(extent-extentClass)>1f) {
									listFaults.Add(what+": ink extent "+extent+" px, its size class "+extentClass.ToString("0.0")+" px in the "+sizeBox+" px box ("+iconDesign.Size+" px design).");
								}
								countChecked++;
							}
						}
					}
				}
			}
			Assert.AreEqual(0,listFaults.Count,string.Join("\r\n",listFaults));
			Assert.IsTrue(countChecked>1000,"Icons checked: "+countChecked);
		}

		///<summary>The faults of the lead's audit, by name, at 100, 150 and 200% of the toolbar slot: the editors' Copy, Paste and Save as Draft have
		///Cut's ink extent, and the printer has the extent of its neighbours in the wiki (List) and the claims window (Preview, New document, the
		///document icons that looked smaller), within 1 px.</summary>
		[TestMethod]
		public void AuditedIcons_HaveTheirNeighboursSize_At100_150_200() {
			foreach(float scale in _arrayScales) {
				int sizePixels=UiTheme.ScaleI(ToolBarOD.SIZE_IMAGE_LIST96,scale);
				string at=" at "+(int)Math.Round(scale*100)+"%";
				int extentCut=GetInkExtent(EnumIcons.Cut,sizePixels);
				foreach(EnumIcons icon in new[] { EnumIcons.Copy,EnumIcons.Paste,EnumIcons.SaveDraft }) {
					int extent=GetInkExtent(icon,sizePixels);
					Assert.IsTrue(Math.Abs(extent-extentCut)<=1,icon+at+": "+extent+" px, Cut "+extentCut+" px.");
				}
				int extentPrint=GetInkExtent(EnumIcons.Print,sizePixels);
				foreach(EnumIcons icon in new[] { EnumIcons.ListBullets,EnumIcons.Preview,EnumIcons.NewDocument }) {
					int extent=GetInkExtent(icon,sizePixels);
					Assert.IsTrue(Math.Abs(extent-extentPrint)<=1,icon+at+": "+extent+" px, Print "+extentPrint+" px.");
				}
			}
		}

		///<summary>The editors' text buttons (the lead's audit): H1, H2 and H3 have the cap height of B and I (within 1 px) in the 20, 24 and 32 px
		///boxes, so they no longer look small next to them; B draws the 28 px design (lighter stems than the 16 to 24 px designs' heavy B) at 0.96,
		///I's cap height.</summary>
		[TestMethod]
		public void HeadingLetters_HaveTheCapHeightOfBoldAndItalic_At100_150_200() {
			Assert.IsTrue(IconLibrary.GetDesigns(EnumIcons.Bold).All(x => x.Size==28 && x.FileName=="text_bold_28_regular.svg" && Math.Abs(x.OpticalScale-0.96f)<0.001f));
			foreach(EnumIcons icon in new[] { EnumIcons.Heading1,EnumIcons.Heading2,EnumIcons.Heading3 }) {
				Assert.IsTrue(IconLibrary.GetDesigns(icon).All(x => Math.Abs(x.OpticalScale-8f/7f)<0.001f),icon+": the optical scale 8/7.");
			}
			foreach(float scale in _arrayScales) {
				int sizePixels=UiTheme.ScaleI(ToolBarOD.SIZE_IMAGE_LIST96,scale);
				int heightBold;
				using(Bitmap bitmap=IconLibrary.GetBitmap(EnumIcons.Bold,sizePixels,sizePixels)) {
					heightBold=GetInkBounds(bitmap).Height;
				}
				foreach(EnumIcons icon in new[] { EnumIcons.Heading1,EnumIcons.Heading2,EnumIcons.Heading3,EnumIcons.Italic }) {
					using(Bitmap bitmap=IconLibrary.GetBitmap(icon,sizePixels,sizePixels)) {
						int height=GetInkBounds(bitmap).Height;
						Assert.IsTrue(Math.Abs(height-heightBold)<=1,icon+" at "+(int)Math.Round(scale*100)+"%: cap height "+height+" px, Bold "+heightBold+" px.");
					}
				}
			}
		}

		///<summary>The wiki's Rename (index 6 of its list; the lead's audit: the split box of Fluent Rename was hard to read) draws Fluent Rename A, a
		///name with a text cursor, at every size; the list keeps its order.</summary>
		[TestMethod]
		public void WikiRename_IsRenameA() {
			Assert.AreEqual(EnumIcons.Rename,FormWiki.ArrayIconsImageList[6]);
			IconDesign[] arrayDesigns=IconLibrary.GetDesigns(EnumIcons.Rename);
			CollectionAssert.AreEqual(new[] { 16,20,24 },arrayDesigns.Select(x => x.Size).ToArray());
			Assert.IsTrue(arrayDesigns.All(x => x.FileName.StartsWith("rename_a_")),string.Join(",",arrayDesigns.Select(x => x.FileName)));
		}

		///<summary>The box rule is for the icons only: a control glyph (UiGlyph) keeps section 3.5's rule (its largest design that fits, drawn at the
		///design's size, never stretched up), so the controls' pictures do not change.</summary>
		[TestMethod]
		public void ControlGlyphs_KeepTheirDesignSize() {
			IconDesign iconDesign=UiIcons.GetDesign(UiGlyph.Maximize,32);
			Assert.AreEqual(24,iconDesign.Size);
			Assert.IsFalse(UiIcons.IsBoxFilled(iconDesign));
			Assert.AreEqual(new Rectangle(4,4,24,24),UiIcons.GetDrawRectangle(iconDesign,new Rectangle(0,0,32,32)));
			UiIcons.GetPlacement(iconDesign,24,out float offset,out float scale);
			Assert.AreEqual(0f,offset);
			Assert.AreEqual(1f,scale);
			//An icon without a 32 px design (Cut: 16, 20, 24) in a 44 px slot: the 24 design in the 32 px box, its 20 px keyline square to 28 px.
			IconDesign iconDesignCut=IconLibrary.GetDesign(EnumIcons.Cut,44);
			Assert.AreEqual(24,iconDesignCut.Size);
			Assert.AreEqual(new Rectangle(6,6,32,32),UiIcons.GetDrawRectangle(iconDesignCut,new Rectangle(0,0,44,44)));
			UiIcons.GetPlacement(iconDesignCut,32,out offset,out scale);
			Assert.AreEqual(1.4f,scale,0.0001f);
			Assert.AreEqual(-0.8f,offset,0.0001f);
			//An icon with the box's design draws it as before: pixel for pixel.
			IconDesign iconDesignCopy=IconLibrary.GetDesign(EnumIcons.Copy,44);
			Assert.AreEqual(32,iconDesignCopy.Size);
			UiIcons.GetPlacement(iconDesignCopy,32,out offset,out scale);
			Assert.AreEqual(0f,offset);
			Assert.AreEqual(1f,scale);
		}

		///<summary>Writes a design's figures into a GraphicsPath (as UiIcons' own sink).</summary>
		private sealed class PathSink:IIconPathSink {
			private readonly System.Drawing.Drawing2D.GraphicsPath _graphicsPath;
			private PointF _pointCurrent;

			public PathSink(System.Drawing.Drawing2D.GraphicsPath graphicsPath) {
				_graphicsPath=graphicsPath;
			}

			public void BeginFigure(float x,float y) {
				_graphicsPath.StartFigure();
				_pointCurrent=new PointF(x,y);
			}

			public void AddLine(float x,float y) {
				PointF point=new PointF(x,y);
				_graphicsPath.AddLine(_pointCurrent,point);
				_pointCurrent=point;
			}

			public void AddBezier(float x1,float y1,float x2,float y2,float x,float y) {
				PointF point=new PointF(x,y);
				_graphicsPath.AddBezier(_pointCurrent,new PointF(x1,y1),new PointF(x2,y2),point);
				_pointCurrent=point;
			}

			public void EndFigure() {
				_graphicsPath.CloseFigure();
			}
		}
	}
}
