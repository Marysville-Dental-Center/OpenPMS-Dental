// The ImageList part of the icon renderer (docs/specs/ui-refresh-plan.md, section 4.2, item 5; package UI-7a): GetImageList makes a 32-bit
// ImageList of glyphs at one pixel size for ImageLists, TreeViews and ListViews (UI-9b: and of any bitmaps with keys, the one place an ImageList is
// made in code). The caller builds a new one when the DPI changes. Windows Forms
// and System.Drawing only, C# 7.3: RdlDesign can compile this file in as a linked file (UI-9a-7).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CodeBase {
	public static partial class UiIcons {
		///<summary>A new ImageList (32-bit colour, sizePixels square) holding one image per glyph, in the order given, drawn in the colour. Pass the
		///real pixel size at the current DPI; build a new list when it changes. The caller owns the list.</summary>
		public static ImageList GetImageList(UiGlyph[] arrayGlyphs,int sizePixels,Color color) {
			sizePixels=Math.Max(sizePixels,1);
			Bitmap[] arrayBitmaps=new Bitmap[arrayGlyphs.Length];
			for(int i=0;i<arrayGlyphs.Length;i++) {
				arrayBitmaps[i]=GetBitmap(arrayGlyphs[i],sizePixels,color);
			}
			return GetImageList(sizePixels,null,arrayBitmaps);
		}

		///<summary>The one place the program makes an ImageList in code (UI-9b, docs/port-notes/UI-9b.md; the source scan
		///SourceScanTests.ImageLists_TakeTheirDepthFromAStreamOrSetIt allows only this file): a new 32-bit ImageList, sizePixels square, holding the
		///bitmaps in order, each under its key (arrayKeys null, or a null key: no key). The list takes the bitmaps (they go with it); the caller owns
		///the list. OpenPmsDental's IconLibrary.GetImageList fills it with EnumIcons icons at the toolbar's DPI size.</summary>
		public static ImageList GetImageList(int sizePixels,string[] arrayKeys,Bitmap[] arrayBitmaps) {
			return GetImageList(sizePixels,sizePixels,arrayKeys,arrayBitmaps);
		}

		///<summary>GetImageList for a list whose pictures are not square (UI-9b-4: the dashboard's tab pictures, 30 px tall): width by height pixels.</summary>
		public static ImageList GetImageList(int widthPixels,int heightPixels,string[] arrayKeys,Bitmap[] arrayBitmaps) {
			widthPixels=Math.Max(widthPixels,1);
			heightPixels=Math.Max(heightPixels,1);
			ImageList imageList=new ImageList();
			imageList.ColorDepth=ColorDepth.Depth32Bit;
			imageList.ImageSize=new Size(widthPixels,heightPixels);
			for(int i=0;i<arrayBitmaps.Length;i++) {
				//Not disposed: the list keeps the bitmap itself and reads it again whenever it (re)creates its handle, so a disposed bitmap
				//throws "Parameter is not valid" there. The bitmaps go with the list.
				string key=arrayKeys==null || i>=arrayKeys.Length ? null : arrayKeys[i];
				if(key==null) {
					imageList.Images.Add(arrayBitmaps[i]);
				}
				else {
					imageList.Images.Add(key,arrayBitmaps[i]);
				}
			}
			return imageList;
		}

		///<summary>GetImageList in the colour of a token.</summary>
		public static ImageList GetImageList(UiGlyph[] arrayGlyphs,int sizePixels,EnumUiColor uiColor) {
			return GetImageList(arrayGlyphs,sizePixels,UiTheme.GetColor(uiColor));
		}
	}
}
