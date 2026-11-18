// The ImageList part of the icon renderer (docs/specs/ui-refresh-plan.md, section 4.2, item 5; package UI-7a): GetImageList makes a 32-bit
// ImageList of glyphs at one pixel size for ImageLists, TreeViews and ListViews. The caller builds a new one when the DPI changes. Windows Forms
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
			ImageList imageList=new ImageList();
			imageList.ColorDepth=ColorDepth.Depth32Bit;
			imageList.ImageSize=new Size(sizePixels,sizePixels);
			for(int i=0;i<arrayGlyphs.Length;i++) {
				using(Bitmap bitmap=GetBitmap(arrayGlyphs[i],sizePixels,color)) {
					imageList.Images.Add(bitmap);//the list keeps its own copy
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
