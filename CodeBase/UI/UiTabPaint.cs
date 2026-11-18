// The shapes of the self-drawn WinForms tabs and splitters (docs/specs/ui-refresh-plan.md, sections 3.2, 3.3 and 3.6; package UI-4b-2,
// docs/port-notes/UI-4b.md): the tab strip on SurfaceSubtle, the page's 1 px Stroke border, a tab header (transparent with TextSecondary, SubtleHover
// with 4 px corners under the mouse, the selected one Text with the 2 px Accent line along the edge that faces the page, TextDisabled when disabled,
// a page's own ColorTab still filling its header), and a splitter (its area in the window's colour, a 1 px Gridline line in its middle, Accent while
// it is dragged). Drawn with UiControlPaint's shapes (UI-4a), so the snapping rule of section 3.3 holds, and the same shapes as the design sheet's
// drawing of the families (tests/UiRefreshTests/DesignSheetControls.cs: DrawTabHeader, DrawTabsComposite, DrawSplitter). Windows Forms and
// System.Drawing only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;

namespace CodeBase {
	///<summary>Where a tab strip sits beside its page: the selected tab's line is on the header's edge that faces the page.</summary>
	public enum EnumUiTabSide {
		///<summary>The strip above the page: the line along the header's bottom.</summary>
		Top,
		///<summary>The strip below the page: the line along the header's top.</summary>
		Bottom,
		///<summary>The strip left of the page: the line along the header's right edge.</summary>
		Left,
	}

	///<summary>The token shapes of the self-drawn WinForms tabs and splitters (section 3.6), in device pixels at a scale.</summary>
	public static class UiTabPaint {
		#region Tabs
		///<summary>The tab strip's background (section 3.6): SurfaceSubtle, square.</summary>
		public static void DrawStrip(Graphics g,Rectangle rectangleStrip,float scale) {
			UiControlPaint.Fill(g,EnumUiColor.SurfaceSubtle,rectangleStrip,0,scale);
		}

		///<summary>The page's border (section 3.6): 1 px Stroke (scaled by the line rule of 3.3) inside the rectangle, square (3.3: tab pages are
		///square).</summary>
		public static void DrawPageBorder(Graphics g,Rectangle rectanglePage,float scale) {
			UiControlPaint.Outline(g,EnumUiColor.Stroke,rectanglePage,UiTheme.GetLineWidth(scale),0,scale);
		}

		///<summary>A tab header in its rectangle (section 3.6, the design sheet's DrawTabHeader): colorTab (a page's own colour) fills it with 4 px
		///corners when it is not empty; otherwise SubtleHover under the mouse (not on the selected or a disabled tab), else nothing (the strip shows);
		///the selected one gets the 2 px Accent line along the edge that faces the page. Returns the token of its text: TextDisabled when disabled,
		///Text when selected or on a page's own colour, TextSecondary otherwise.</summary>
		public static EnumUiColor DrawHeader(Graphics g,Rectangle rectangleHeader,EnumUiTabSide tabSide,bool isSelected,bool isHover,bool isEnabled,Color colorTab,
			float scale)
		{
			bool hasColorTab=colorTab.A!=0;
			if(hasColorTab) {
				UiControlPaint.Fill(g,colorTab,rectangleHeader,UiTheme.CORNER_CONTROL,scale);
			}
			else if(isHover && isEnabled && !isSelected) {
				UiControlPaint.Fill(g,EnumUiColor.SubtleHover,rectangleHeader,UiTheme.CORNER_CONTROL,scale);
			}
			if(isSelected) {
				UiControlPaint.Fill(g,EnumUiColor.Accent,GetSelectedLine(rectangleHeader,tabSide,scale),0,scale);
			}
			if(!isEnabled) {
				return EnumUiColor.TextDisabled;
			}
			if(isSelected || hasColorTab) {
				return EnumUiColor.Text;
			}
			return EnumUiColor.TextSecondary;
		}

		///<summary>The selected tab's line (section 3.3: 2 px, scaled as ScaleI scales): inside the header along the edge that faces the page.</summary>
		public static Rectangle GetSelectedLine(Rectangle rectangleHeader,EnumUiTabSide tabSide,float scale) {
			int width=Math.Max(1,UiTheme.ScaleI(UiTheme.LINE_TAB_SELECTED,scale));
			switch(tabSide) {
				case EnumUiTabSide.Bottom:
					return new Rectangle(rectangleHeader.X,rectangleHeader.Y,rectangleHeader.Width,width);
				case EnumUiTabSide.Left:
					return new Rectangle(rectangleHeader.Right-width,rectangleHeader.Y,width,rectangleHeader.Height);
				default:
					return new Rectangle(rectangleHeader.X,rectangleHeader.Bottom-width,rectangleHeader.Width,width);
			}
		}
		#endregion Tabs

		#region Splitters
		///<summary>A splitter's area (section 3.6, the design sheet's DrawSplitter): colorBack (empty: Surface, the window's colour), with a 1 px line
		///(scaled by the line rule of 3.3) along its middle, Gridline at rest and Accent while it is dragged. isVertical: the splitter is a vertical
		///bar between a left and a right panel (Orientation.Vertical).</summary>
		public static void DrawSplitter(Graphics g,Rectangle rectangleSplitter,bool isVertical,bool isDragging,Color colorBack,float scale) {
			if(rectangleSplitter.Width<=0 || rectangleSplitter.Height<=0) {
				return;
			}
			UiControlPaint.Fill(g,colorBack.IsEmpty ? UiTheme.GetColor(EnumUiColor.Surface) : colorBack,rectangleSplitter,0,scale);
			UiControlPaint.Fill(g,isDragging ? EnumUiColor.Accent : EnumUiColor.Gridline,GetSplitterLine(rectangleSplitter,isVertical,scale),0,scale);
		}

		///<summary>The splitter's line: GetLineWidth wide, in the middle of its area (rounded down, as the design sheet places it), as long as the area.</summary>
		public static Rectangle GetSplitterLine(Rectangle rectangleSplitter,bool isVertical,float scale) {
			int width=UiTheme.GetLineWidth(scale);
			if(isVertical) {
				return new Rectangle(rectangleSplitter.X+(rectangleSplitter.Width-width)/2,rectangleSplitter.Y,width,rectangleSplitter.Height);
			}
			return new Rectangle(rectangleSplitter.X,rectangleSplitter.Y+(rectangleSplitter.Height-width)/2,rectangleSplitter.Width,width);
		}
		#endregion Splitters
	}
}
