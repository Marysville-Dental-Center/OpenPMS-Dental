// The sizes of the UI refresh (docs/specs/ui-refresh-plan.md, sections 3.3, 3.4 and 3.5; package UI-1, docs/port-notes/UI-1.md): the font family
// and sizes of the design, corners, lines, spacing, icon sizes, and the pixel-snapping helpers that keep lines sharp at 125%, 150% and 175%.
// All values are at 96 DPI. System.Drawing only, C# 7.3: RdlDesign can compile this file in as a linked file (section 3.6).
// The fonts here are the design's values (the owner's answers UQ-1 and UQ-2 of 2026-11-17). The role tokens that the program's fonts go through
// (FontBody, FontGrid, ...) are UI-2a-1's and hold today's fonts until the flip (UI-2b, section 3.4).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CodeBase {
	public static partial class UiTheme {
		#region Fonts (section 3.4)
		///<summary>The font family of the new look, everywhere (decision 3).</summary>
		public const string FONT_FAMILY="Segoe UI";
		///<summary>The family of the self-drawn titles (grid title, group titles drawn in code).</summary>
		public const string FONT_FAMILY_SEMIBOLD="Segoe UI Semibold";
		///<summary>Today's family (net10-plan decision 9), which the flip (UI-2b) replaces.</summary>
		public const string FONT_FAMILY_TODAY="Microsoft Sans Serif";
		///<summary>Body text, WinForms: labels, inputs, buttons, menus, grid cells and headers, button-panel items, the window title (UQ-1: 9 pt,
		///Windows' own size).</summary>
		public const float FONT_SIZE_BODY=9f;
		///<summary>UQ-1's other option, kept for the galleries: 8.25 pt, the line height of today's font.</summary>
		public const float FONT_SIZE_BODY_COMPACT=8.25f;
		///<summary>LayoutManagerForms draws labels and radio buttons at this factor of their font (today's behaviour, kept by UQ-1): 9 pt becomes
		///8.64 pt.</summary>
		public const float FONT_FACTOR_LABEL=0.96f;
		///<summary>Self-drawn titles (grid title, group titles drawn in code), in FONT_FAMILY_SEMIBOLD.</summary>
		public const float FONT_SIZE_TITLE=10f;
		///<summary>The module bar's captions.</summary>
		public const float FONT_SIZE_MODULE_CAPTION=8f;
		///<summary>The WPF body size in DIP (FrmODBase; unchanged): draws like 9 pt at 96 DPI.</summary>
		public const double FONT_SIZE_WPF_BODY=11.5;
		///<summary>The WPF grid title in DIP, Semibold (unchanged).</summary>
		public const double FONT_SIZE_WPF_TITLE=14;
		///<summary>Grid rows follow the body font (UQ-2): 17 px at Segoe UI 9.</summary>
		public const int GRID_ROW_HEIGHT=17;
		///<summary>UQ-2's other option, kept for the galleries: grid cells in Segoe UI 8.25, 16 px rows.</summary>
		public const int GRID_ROW_HEIGHT_COMPACT=16;
		#endregion Fonts (section 3.4)

		#region Corners (section 3.3)
		///<summary>Buttons, inputs, combo boxes, tooltips, toolbar and module bar hover shapes.</summary>
		public const int CORNER_CONTROL=4;
		///<summary>The check box's box.</summary>
		public const int CORNER_CHECKBOX=2;
		///<summary>Containers (group boxes).</summary>
		public const int CORNER_CONTAINER=6;
		///<summary>A menu item's hover shape.</summary>
		public const int CORNER_MENU_ITEM=2;
		///<summary>The module bar's hover shape.</summary>
		public const int CORNER_MODULE_HOVER=6;
		///<summary>Grids, tab pages, menus and the window frame are square.</summary>
		public const int CORNER_SQUARE=0;
		#endregion Corners (section 3.3)

		#region Lines (section 3.3)
		///<summary>Borders and separators.</summary>
		public const int LINE_BORDER=1;
		///<summary>The focus ring, in Accent, inside the control's bounds.</summary>
		public const int LINE_FOCUS=2;
		///<summary>The white inner ring between an accent-filled control and its focus ring.</summary>
		public const int LINE_FOCUS_INNER=1;
		///<summary>The bar on the left edge of a selected grid row, list item or tree item (section 3.2).</summary>
		public const int LINE_SELECTION_BAR=2;
		///<summary>The selected tab's underline.</summary>
		public const int LINE_TAB_SELECTED=2;
		///<summary>The combo box's focus line inside its bottom edge.</summary>
		public const int LINE_COMBO_FOCUS=2;
		///<summary>The module bar's selection mark on the left.</summary>
		public const int LINE_MODULE_MARK=3;
		#endregion Lines (section 3.3)

		#region Spacing (section 3.3)
		///<summary>The spacing steps inside controls: 2, 4, 8 and 12 px. Spacing between controls does not change (decision 1).</summary>
		public const int SPACE_2=2;
		public const int SPACE_4=4;
		public const int SPACE_8=8;
		public const int SPACE_12=12;
		///<summary>The gap between an icon and its text.</summary>
		public const int GAP_ICON_TEXT=4;
		///<summary>A button's text padding, left and right.</summary>
		public const int PADDING_BUTTON_TEXT=8;
		///<summary>A menu's icon column.</summary>
		public const int MENU_ICON_COLUMN=16;
		///<summary>A toolbar separator's inset from the top and bottom.</summary>
		public const int INSET_TOOLBAR_SEPARATOR=4;
		///<summary>The module bar's hover shape, inset inside the button's bounds.</summary>
		public const int INSET_MODULE_HOVER=4;
		///<summary>The drop-down chevron's size.</summary>
		public const int SIZE_CHEVRON=12;
		#endregion Spacing (section 3.3)

		#region Icon sizes (section 3.5)
		///<summary>The sizes Fluent UI System Icons are designed in.</summary>
		public static readonly int[] ArrayIconDesignSizes=new int[] { 12,16,20,24,28,32,48 };

		///<summary>The design to draw in a slot of slotPixels device pixels (after DPI scaling): the largest design that fits, never stretched up
		///(section 3.5: 16 gets 16, 20-23 gets 20, 24-31 gets 24, 32-47 gets 32, 48 and up gets 48). The program's slots are 16, 20, 22 and 32 px at 96 DPI; a
		///slot below the smallest design gets the smallest, drawn down to the slot.</summary>
		public static int GetIconDesignSize(int slotPixels) {
			int sizeDesign=ArrayIconDesignSizes[0];
			for(int i=0;i<ArrayIconDesignSizes.Length;i++) {
				//28 is a design of its own, but section 3.5 keeps 24 for the 24-31 px slots: the program has no 28 px slot.
				if(ArrayIconDesignSizes[i]==28) {
					continue;
				}
				if(ArrayIconDesignSizes[i]<=slotPixels) {
					sizeDesign=ArrayIconDesignSizes[i];
				}
			}
			return sizeDesign;
		}

		///<summary>The square the icon is drawn in: its design size (GetIconDesignSize), centred in the slot on whole device pixels.</summary>
		public static Rectangle GetIconRectangle(Rectangle rectangleSlot) {
			int sizeDesign=GetIconDesignSize(Math.Min(rectangleSlot.Width,rectangleSlot.Height));
			int size=Math.Min(sizeDesign,Math.Min(rectangleSlot.Width,rectangleSlot.Height));
			return new Rectangle(rectangleSlot.X+(rectangleSlot.Width-size)/2,rectangleSlot.Y+(rectangleSlot.Height-size)/2,size,size);
		}
		#endregion Icon sizes (section 3.5)

		#region Pixel snapping (section 3.3)
		///<summary>A 96 DPI length at the scale, in whole device pixels, rounded exactly as LayoutManagerForms.Scale rounds: (int)Math.Round, which
		///takes a .5 to the even neighbour (2 x 1.25 = 2.5 gives 2, 10 x 1.25 = 12.5 gives 12, 2 x 1.75 = 3.5 gives 4). So a control that scales its
		///bounds with LayoutManager and its inner shapes with UiTheme gets the same pixels.</summary>
		public static int ScaleI(int value96,float scale) {
			return (int)Math.Round(value96*scale);
		}

		///<summary>Borders and separators: max(1, floor(scale)) device pixels, so 1 px at 100% to 175% and 2 px at 200% (a 1.25 or 1.5 px line would
		///smear into grey).</summary>
		public static int GetLineWidth(float scale) {
			return Math.Max(1,(int)Math.Floor(scale+0.0001f));
		}

		///<summary>The focus ring and the selection bar: round(2 x scale) device pixels, rounded as ScaleI (and LayoutManager) rounds: 2, 2, 3, 4, 4 px
		///at 100, 125, 150, 175 and 200%.</summary>
		public static int GetFocusWidth(float scale) {
			return Math.Max(1,ScaleI(LINE_FOCUS,scale));
		}

		///<summary>A corner radius at the scale, in whole device pixels.</summary>
		public static int GetCornerRadius(int radius96,float scale) {
			return ScaleI(radius96,scale);
		}

		///<summary>A rectangle snapped to whole device pixels: each edge rounded on its own, so two rectangles that share an edge before still share it.
		///Rounded as LayoutManagerForms.Round rounds ((int)Math.Round, a .5 to the even neighbour: 12.5 gives 12, 37.5 gives 38).</summary>
		public static Rectangle SnapRectangle(RectangleF rectangleF) {
			int left=(int)Math.Round(rectangleF.Left);
			int top=(int)Math.Round(rectangleF.Top);
			int right=(int)Math.Round(rectangleF.Right);
			int bottom=(int)Math.Round(rectangleF.Bottom);
			return Rectangle.FromLTRB(left,top,right,bottom);
		}

		///<summary>A 96 DPI rectangle at the scale, snapped to whole device pixels.</summary>
		public static Rectangle ScaleRectangle(Rectangle rectangle96,float scale) {
			return SnapRectangle(new RectangleF(rectangle96.X*scale,rectangle96.Y*scale,rectangle96.Width*scale,rectangle96.Height*scale));
		}

		///<summary>The path along which a line widthLine px wide is drawn so that it lies inside the bounds on whole pixels: inset by half the width,
		///so a 1 px line is on the pixel centres (offset 0.5) and a 2 px line covers two whole pixels. For Graphics.DrawRectangle or a path with
		///antialiasing on and PixelOffsetMode.Half (GDI+ then puts pixel centres at .5; with the default mode a line on .5 falls between two
		///pixels). Fill with the bounds themselves in the same mode.</summary>
		public static RectangleF GetStrokeRectangle(Rectangle rectangleBounds,int widthLine) {
			float half=widthLine/2f;
			return new RectangleF(rectangleBounds.X+half,rectangleBounds.Y+half,rectangleBounds.Width-widthLine,rectangleBounds.Height-widthLine);
		}

		///<summary>A rounded rectangle path; radius 0 gives a plain rectangle. Use GetStrokeRectangle for an outline and the bounds for a fill. The
		///caller disposes the path (it is not cached).</summary>
		public static GraphicsPath GetRoundedRectanglePath(RectangleF rectangleF,float radius) {
			GraphicsPath graphicsPath=new GraphicsPath();
			float diameter=Math.Min(radius*2,Math.Min(rectangleF.Width,rectangleF.Height));
			if(diameter<=0) {
				graphicsPath.AddRectangle(rectangleF);
				return graphicsPath;
			}
			graphicsPath.AddArc(rectangleF.X,rectangleF.Y,diameter,diameter,180,90);
			graphicsPath.AddArc(rectangleF.Right-diameter,rectangleF.Y,diameter,diameter,270,90);
			graphicsPath.AddArc(rectangleF.Right-diameter,rectangleF.Bottom-diameter,diameter,diameter,0,90);
			graphicsPath.AddArc(rectangleF.X,rectangleF.Bottom-diameter,diameter,diameter,90,90);
			graphicsPath.CloseFigure();
			return graphicsPath;
		}
		#endregion Pixel snapping (section 3.3)
	}
}
