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

		///<summary>Every icon of every strip, in the strip's ImageList at 100, 150 and 200% of the 22 and 16 px slots, is drawn in one box: the box of
		///section 3.5 for the slot (the same for every icon), its ink inside the box, and the ink extent of the icons of one size class (the extent of
		///the icon in the 24 px box, rounded) within 1 px of each other.</summary>
		[TestMethod]
		public void StripIcons_FillOneBox_InkWithin1PxPerSizeClass_At100_150_200() {
			List<string> listFaults=new List<string>();
			Dictionary<EnumIcons,int> dictClass=new Dictionary<EnumIcons,int>();
			foreach(KeyValuePair<string,EnumIcons[]> pair in GetStrips()) {
				foreach(int size96 in _arraySizes96) {
					foreach(float scale in _arrayScales) {
						int sizePixels=UiTheme.ScaleI(size96,scale);
						int sizeBox=Math.Min(UiTheme.GetIconDesignSize(sizePixels),sizePixels);
						Rectangle rectangleBox=new Rectangle((sizePixels-sizeBox)/2,(sizePixels-sizeBox)/2,sizeBox,sizeBox);
						Dictionary<int,List<KeyValuePair<EnumIcons,int>>> dictExtentsByClass=new Dictionary<int,List<KeyValuePair<EnumIcons,int>>>();
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
								if(!dictClass.ContainsKey(icon)) {
									dictClass[icon]=GetInkExtent(icon,24);
								}
								int sizeClass=dictClass[icon];
								if(!dictExtentsByClass.ContainsKey(sizeClass)) {
									dictExtentsByClass[sizeClass]=new List<KeyValuePair<EnumIcons,int>>();
								}
								dictExtentsByClass[sizeClass].Add(new KeyValuePair<EnumIcons,int>(icon,Math.Max(rectangleInk.Width,rectangleInk.Height)));
							}
						}
						foreach(KeyValuePair<int,List<KeyValuePair<EnumIcons,int>>> pairClass in dictExtentsByClass) {
							int extentMin=pairClass.Value.Min(x => x.Value);
							int extentMax=pairClass.Value.Max(x => x.Value);
							if(extentMax-extentMin>1) {
								listFaults.Add(pair.Key+" at "+(int)Math.Round(scale*100)+"% of "+size96+" px, size class "+pairClass.Key+" px: ink extents "
									+string.Join(", ",pairClass.Value.Select(x => x.Key+" "+x.Value)));
							}
						}
					}
				}
			}
			Assert.AreEqual(0,listFaults.Count,string.Join("\r\n",listFaults));
		}

		///<summary>The faults of the lead's audit, by name: at 150 and 200% Copy, Paste and Save as Draft are no larger than Cut and Undo in the
		///editors, the printer no larger than its neighbours in the wiki and the claims window, and the claims window's document icons (Preview,
		///New document) no smaller than the printer: the extents of icons of one size class are within 1 px, and the whole strip's within its
		///classes' spread.</summary>
		[TestMethod]
		public void AuditedIcons_HaveTheirNeighboursSize_At150_200() {
			foreach(float scale in new float[] { 1f,1.5f,2f }) {
				int sizePixels=UiTheme.ScaleI(ToolBarOD.SIZE_IMAGE_LIST96,scale);
				int extentCopy=GetInkExtent(EnumIcons.Copy,sizePixels);
				int extentPrint=GetInkExtent(EnumIcons.Print,sizePixels);
				foreach(EnumIcons icon in new[] { EnumIcons.Paste,EnumIcons.SaveDraft,EnumIcons.Cut,EnumIcons.Save,EnumIcons.Archive,EnumIcons.Settings,EnumIcons.Preview,
					EnumIcons.NewDocument,EnumIcons.DocumentText,EnumIcons.Complete })
				{
					int extent=GetInkExtent(icon,sizePixels);
					int extentClassOf24=GetInkExtent(icon,24);
					int extentCopyOf24=GetInkExtent(EnumIcons.Copy,24);
					//An icon whose 24 px design has the extent of Copy's has Copy's extent in every box, within 1 px; a smaller or larger one keeps its
					//difference, scaled by the keylines, within 1 px.
					float expected=extentCopy+(extentClassOf24-extentCopyOf24)*UiIcons.GetLiveSize(UiTheme.GetIconDesignSize(sizePixels))/UiIcons.GetLiveSize(24);
					Assert.IsTrue(Math.Abs(extent-expected)<=1.5f,icon+" at "+(int)Math.Round(scale*100)+"%: "+extent+" px, expected "+expected+" (Copy "+extentCopy+").");
				}
				Assert.IsTrue(Math.Abs(extentPrint-extentCopy)<=1,"Print "+extentPrint+", Copy "+extentCopy+" at "+(int)Math.Round(scale*100)+"%.");
			}
		}

		///<summary>The editors' text buttons (the lead's audit): H1, H2 and H3 have the cap height of B and I (within 1 px) in the 20, 24 and 32 px
		///boxes, so they no longer look small next to them; B draws the 28 px design (lighter stems than the 16 to 24 px designs' heavy B).</summary>
		[TestMethod]
		public void HeadingLetters_HaveTheCapHeightOfBoldAndItalic_At100_150_200() {
			Assert.IsTrue(IconLibrary.GetDesigns(EnumIcons.Bold).All(x => x.Size==28 && x.FileName=="text_bold_28_regular.svg"));
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
	}
}
