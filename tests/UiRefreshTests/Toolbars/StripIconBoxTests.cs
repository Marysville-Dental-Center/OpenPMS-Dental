// The UI-9b-4 fix's tests (docs/port-notes/UI-9b.md, section 5.8; the lead's audit of gallery G-UI3-ui9b4): every icon of an ImageList strip
// fills the same box at 100, 150 and 200%, whatever design sizes the icon has. Before the fix an icon was drawn at its largest design that fits
// the slot, never stretched up, so at 150 and 200% (33 and 44 px toolbar slots) the icons with a 32 px design (Copy, Paste, Save as Draft, the
// printer) were drawn in a 32 px box and their neighbours without one in a 24 px box. Now every icon of the slot has the box of section 3.5
// (UiTheme.GetIconDesignSize) and a missing design is scaled by the Fluent keylines (UiIcons.GetPlacement), so every icon's ink is within 1 px
// of its size class in the strip's box (its design's extent scaled by the keylines). Also: the heading letters take the cap height of B and I,
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

		///<summary>The bounds of the pixels with at least inkMin of ink (alpha; 128: half ink) of a picture on transparent; empty when there are
		///none.</summary>
		public static Rectangle GetInkBounds(Bitmap bitmap,int inkMin=128) {
			int[] arrayInk=IconRendererTests.GetInk(bitmap,isOnWhite:false);
			int xMin=int.MaxValue;
			int yMin=int.MaxValue;
			int xMax=-1;
			int yMax=-1;
			for(int i=0;i<arrayInk.Length;i++) {
				if(arrayInk[i]<inkMin) {
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

		[TestMethod]
		public void Strips_AreFound() {
			List<KeyValuePair<string,EnumIcons[]>> listStrips=GetStrips();
			Assert.IsTrue(listStrips.Count>=30,"Strips found: "+listStrips.Count);
			Assert.IsTrue(listStrips.Any(x => x.Key=="FormEmailEdit.ArrayIconsImageList"));
			Assert.IsTrue(listStrips.Any(x => x.Key=="FormWiki.ArrayIconsImageList"));
			Assert.IsTrue(listStrips.Any(x => x.Key=="FormClaimsSend.ArrayIconsImageList"));
		}

		///<summary>The exact bounds of a design's figures in its own design pixels (the curves flattened, so no control point counts), with no box
		///and no scale.</summary>
		private static RectangleF GetDesignBounds(IconDesign iconDesign) {
			using(System.Drawing.Drawing2D.GraphicsPath graphicsPath=new System.Drawing.Drawing2D.GraphicsPath(System.Drawing.Drawing2D.FillMode.Winding)) {
				iconDesign.GetFigures().WriteTo(new PathSink(graphicsPath),0,0,1);
				graphicsPath.Flatten(null,0.01f);
				return graphicsPath.GetBounds();
			}
		}

		///<summary>The size class of a design in a box of sizeBox: its extent (the larger side of its figures' bounds) as Fluent drew it, times its
		///optical scale, and, when the box is not its own size, times the ratio of the Fluent keyline squares (GetLiveSize). Every icon of one class
		///has this extent in the box, whichever of its designs it draws.</summary>
		private static float GetExtentClass(IconDesign iconDesign,int sizeBox) {
			RectangleF rectangleF=GetDesignBounds(iconDesign);
			float extent=Math.Max(rectangleF.Width,rectangleF.Height)*iconDesign.OpticalScale;
			if(iconDesign.Size!=sizeBox) {
				extent=extent*UiIcons.GetLiveSize(sizeBox)/UiIcons.GetLiveSize(iconDesign.Size);
			}
			return extent;
		}

		///<summary>An icon's size class in the box of a slot of sizePixels (GetExtentClass of the design the slot draws).</summary>
		private static float GetExtentClass(EnumIcons icon,int sizePixels) {
			IconDesign iconDesign=IconLibrary.GetDesign(icon,sizePixels);
			return GetExtentClass(iconDesign,UiIcons.GetDrawRectangle(iconDesign,new Rectangle(0,0,sizePixels,sizePixels)).Width);
		}

		///<summary>Every icon of every strip, in the strip's ImageList at 100, 150 and 200% of the 22 and 16 px slots, is drawn in one box: the box of
		///section 3.5 for the slot (the same for every icon), its ink inside the box, and its ink extent (any ink) within 1 px of its size class in
		///that box (GetExtentClass) rounded out to whole pixels. Before the fix an icon whose largest design is 24 px drew it in a 24 px box in the 33 and
		///44 px slots: 20 px of ink where its class has 28 in the 32 px box of its neighbours.</summary>
		[TestMethod]
		public void StripIcons_FillOneBox_InkWithin1PxOfTheirSizeClass_At100_150_200() {
			List<string> listFaults=new List<string>();
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
									rectangleInk=GetInkBounds(bitmap,inkMin:1);
								}
								Assert.IsFalse(rectangleInk.IsEmpty,what+": no ink.");
								if(!Rectangle.Inflate(rectangleBox,1,1).Contains(rectangleInk)) {
									listFaults.Add(what+": ink "+rectangleInk+" leaves the box "+rectangleBox);
								}
								float extentClass=GetExtentClass(iconDesign,sizeBox);
								int extent=Math.Max(rectangleInk.Width,rectangleInk.Height);
								//Any ink counts (a round tip may cover less than half a pixel), so the ink is the class rounded out to whole pixels, within
								//1 px: an edge on a fraction of a pixel inks that pixel.
								if(extent<Math.Floor(extentClass)-1 || extent>Math.Ceiling(extentClass)+1) {
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

		///<summary>The faults of the lead's audit, by name, at 100, 150 and 200% of the toolbar slot: the editors' Copy, Paste and Save as Draft are no
		///larger than Cut, the printer is no larger than its neighbours in the wiki (List) and the claims window (Preview, New document), and those
		///document icons are no smaller than the printer, within 1 px (sizes of the drawn figures, GetExtentClass).</summary>
		[TestMethod]
		public void AuditedIcons_HaveTheirNeighboursSize_At100_150_200() {
			foreach(float scale in _arrayScales) {
				int sizePixels=UiTheme.ScaleI(ToolBarOD.SIZE_IMAGE_LIST96,scale);
				string at=" at "+(int)Math.Round(scale*100)+"%";
				float extentCut=GetExtentClass(EnumIcons.Cut,sizePixels);
				foreach(EnumIcons icon in new[] { EnumIcons.Copy,EnumIcons.Paste,EnumIcons.SaveDraft }) {
					float extent=GetExtentClass(icon,sizePixels);
					Assert.IsTrue(extent<=extentCut+1,icon+at+": "+extent+" px, Cut "+extentCut+" px.");
				}
				float extentPrint=GetExtentClass(EnumIcons.Print,sizePixels);
				foreach(EnumIcons icon in new[] { EnumIcons.ListBullets,EnumIcons.Preview,EnumIcons.NewDocument }) {
					float extent=GetExtentClass(icon,sizePixels);
					Assert.IsTrue(extentPrint<=extent+1,"Print"+at+": "+extentPrint+" px, "+icon+" "+extent+" px.");
					Assert.IsTrue(extent>=extentPrint-1,icon+at+": "+extent+" px, Print "+extentPrint+" px.");
				}
			}
		}

		///<summary>The height of an icon's figures as drawn in the box of a slot of sizePixels (its design's height, times its placement's scale).</summary>
		private static float GetDrawnHeight(EnumIcons icon,int sizePixels) {
			IconDesign iconDesign=IconLibrary.GetDesign(icon,sizePixels);
			int sizeBox=UiIcons.GetDrawRectangle(iconDesign,new Rectangle(0,0,sizePixels,sizePixels)).Width;
			UiIcons.GetPlacement(iconDesign,sizeBox,out float offset,out float scaleDesign);
			return GetDesignBounds(iconDesign).Height*scaleDesign;
		}

		///<summary>The editors' text buttons (the lead's audit): H1 has the cap height of B and I (within 1 px) in the 20, 24 and 32 px boxes (H2 and
		///H3 take H1's optical scale, 8/7; their digits overshoot as Fluent drew them), so the headings no longer look small next to them; B draws
		///the 28 px design (lighter stems than the 16 to 24 px designs' heavy B).</summary>
		[TestMethod]
		public void HeadingLetters_HaveTheCapHeightOfBoldAndItalic_At100_150_200() {
			Assert.IsTrue(IconLibrary.GetDesigns(EnumIcons.Bold).All(x => x.Size==28 && x.FileName=="text_bold_28_regular.svg" && x.OpticalScale==1f));
			foreach(EnumIcons icon in new[] { EnumIcons.Heading1,EnumIcons.Heading2,EnumIcons.Heading3 }) {
				Assert.IsTrue(IconLibrary.GetDesigns(icon).All(x => Math.Abs(x.OpticalScale-8f/7f)<0.001f),icon+": the optical scale 8/7.");
			}
			foreach(float scale in _arrayScales) {
				int sizePixels=UiTheme.ScaleI(ToolBarOD.SIZE_IMAGE_LIST96,scale);
				float heightBold=GetDrawnHeight(EnumIcons.Bold,sizePixels);
				foreach(EnumIcons icon in new[] { EnumIcons.Heading1,EnumIcons.Italic }) {
					float height=GetDrawnHeight(icon,sizePixels);
					Assert.IsTrue(Math.Abs(height-heightBold)<=1,icon+" at "+(int)Math.Round(scale*100)+"%: cap height "+height+" px, Bold "+heightBold+" px.");
				}
				//Before the fix H1 was 2 design px shorter than B and I at every size (14 of 24 against 16).
				Assert.IsTrue(GetDrawnHeight(EnumIcons.Heading1,sizePixels)>=GetDrawnHeight(EnumIcons.Italic,sizePixels)-0.5f);
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
