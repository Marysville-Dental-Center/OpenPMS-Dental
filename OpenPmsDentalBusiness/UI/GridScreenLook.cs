// The screen look of the WinForms grids, GridOD and GridOld (UI refresh, package UI-5; docs/specs/ui-refresh-plan.md sections 3.2, 3.3 and 3.6,
// the owner's answer UQ-2, docs/port-notes/UI-5.md): the flat Accent title with white text, the SurfaceSubtle header with Stroke separators,
// rows on SurfaceRaised with Gridline lines, hover RowHover, the selection AccentSubtle with its 2 px bar (Accent with focus, SelectionBarNoFocus
// without) in the row's first pixels, the focused row's 1 px Accent outline, the 1 px Stroke outer border; and the title, header and editable
// row heights, which follow the font tokens. Row colours set by the program or the office (a row's or a cell's ColorBackG and ColorText, a
// window's ColorSelectedRow, ColorLborder) still win. Printouts and sheets never come here: they draw with GridPrintLook and the print fonts
// (UQ-5). Pens and brushes are UiTheme's per-thread ones: never dispose them.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using CodeBase;

namespace OpenPmsDental.UI {
	///<summary>The screen look of GridOD and GridOld (UI-5).</summary>
	public static class GridScreenLook {
		#region Sizes
		///<summary>The title's height above its font's line height: 18 at 96 DPI with today's sans serif 10 Bold (line height 16), as before UI-5.</summary>
		public const int PAD_TITLE=2;
		///<summary>The header's height above its font's line height: 15 with today's sans serif 8.5 Bold (line height 13), as before UI-5.</summary>
		public const int PAD_HEADER=2;
		///<summary>An editable row's height above the cell font's line height: 19 with today's 8.5 (line height 13), as before UI-5.</summary>
		public const int PAD_EDITABLE_ROW=6;
		///<summary>Today's editable row height at 96 DPI (GridOD and GridOld EDITABLE_ROW_HEIGHT); printouts keep it.</summary>
		public const int HEIGHT_EDITABLE_ROW_TODAY=19;

		///<summary>The font's line height at 96 DPI in whole pixels (rounded up), whatever the screen's DPI: 13 for sans serif 8.5, 16 for sans serif
		///10 Bold and for Segoe UI 9, 18 for Segoe UI Semibold 10.</summary>
		public static int GetHeightFont96(Font font) {
			return (int)Math.Ceiling(font.GetHeight(96f)-0.001f);
		}

		///<summary>The screen title's height at 96 DPI from the title font token (UI-5: it follows the font, inside the control): 18 today, 20 with
		///Segoe UI Semibold 10 (UI-2b).</summary>
		public static int GetHeightTitle96() {
			return GetHeightFont96(UiTheme.FontTitle)+PAD_TITLE;
		}

		///<summary>The screen header's height at 96 DPI from the grid's bold font token: 15 today, 18 with Segoe UI 9 Bold (UI-2b). A multiline header
		///grows from it as before.</summary>
		public static int GetHeightHeader96() {
			return GetHeightFont96(UiTheme.FontGridBold)+PAD_HEADER;
		}

		///<summary>An editable row's height on screen at 96 DPI: the cell font's line height plus 6, never below today's 19 (22 with Segoe UI 9). A
		///plain row is the cell font's height plus 1 (UQ-2: 14 today, 17 with Segoe UI 9), as the grids already compute it.</summary>
		public static int GetHeightEditableRow96() {
			return Math.Max(HEIGHT_EDITABLE_ROW_TODAY,GetHeightFont96(UiTheme.FontGrid)+PAD_EDITABLE_ROW);
		}
		#endregion Sizes

		#region Colours
		///<summary>The selection's fill: AccentSubtle for a grid that keeps its stock ColorSelectedRow (colorStock: GridOD's 205,220,235, GridOld's
		///Silver), the window's own colour otherwise (the Family and Account modules' DarkSalmon, the report setup's transparent grey).</summary>
		public static Color GetSelectionFill(Color colorSelectedRow,Color colorStock) {
			if(colorSelectedRow.ToArgb()==colorStock.ToArgb()) {
				return UiTheme.AccentSubtle;
			}
			return colorSelectedRow;
		}

		///<summary>A row's or a cell's background: SurfaceRaised for the default white (the GridRow default), the program's or office's colour
		///otherwise.</summary>
		public static Color GetRowBack(Color colorBackG) {
			if(colorBackG.ToArgb()==Color.White.ToArgb()) {
				return UiTheme.SurfaceRaised;
			}
			return colorBackG;
		}

		///<summary>A cell's text colour: the Text token (TextSelected on a selected row) for the default black (the GridRow default), the program's or
		///office's colour otherwise.</summary>
		public static Color GetTextColor(Color colorText,bool isSelected) {
			if(colorText.ToArgb()==Color.Black.ToArgb()) {
				return isSelected ? UiTheme.TextSelected : UiTheme.Text;
			}
			return colorText;
		}

		///<summary>The hovered row's colour: darkened by as much as RowHover is darker than white (each channel), so a white row is RowHover and a
		///coloured or selected row keeps its colour a shade darker, as GridOD blended its hover before.</summary>
		public static Color BlendHover(Color color) {
			Color colorHover=UiTheme.RowHover;
			int r=Math.Max(0,color.R-255+colorHover.R);
			int g=Math.Max(0,color.G-255+colorHover.G);
			int b=Math.Max(0,color.B-255+colorHover.B);
			return Color.FromArgb(color.A,r,g,b);
		}
		#endregion Colours

		#region Drawing
		///<summary>The control's background below the header: SurfaceRaised.</summary>
		public static void DrawBackground(Graphics g,Rectangle rectangle) {
			g.FillRectangle(UiTheme.GetBrush(EnumUiColor.SurfaceRaised),rectangle);
		}

		///<summary>The title: flat Accent from (1,1) across the width and heightTitle device pixels down (the outer border is drawn over its edge),
		///the text in TextOnAccent centred across and in the middle of the title's height. Draws no text in the designer when isDesignMode.</summary>
		public static void DrawTitle(Graphics g,int width,int heightTitle,string title,Font font,bool isDesignMode) {
			g.FillRectangle(UiTheme.GetBrush(EnumUiColor.Accent),1,1,width,heightTitle);
			if(isDesignMode || string.IsNullOrEmpty(title)) {
				return;
			}
			SizeF sizeF=g.MeasureString(title,font);
			//In the middle of the title's height, on a whole pixel: y 2 in today's 18 px title, as before.
			int heightFont=(int)Math.Ceiling(font.GetHeight(g)-0.001f);
			int y=1+Math.Max(0,(heightTitle-heightFont)/2);
			g.DrawString(title,font,UiTheme.GetBrush(EnumUiColor.TextOnAccent),width/2-sizeF.Width/2,y);
		}

		///<summary>The title's add button at its right end: a 1 px AccentPressed divider and the plus sign in TextOnAccent (TextDisabled when the
		///button is disabled). Returns its width (the title's height, as before).</summary>
		public static int DrawAddButton(Graphics g,int width,int heightTitle,bool isEnabled) {
			int addW=heightTitle;
			int dividerX=width-addW-3;
			const int plusSignWidth=4;
			g.DrawLine(UiTheme.GetPen(EnumUiColor.AccentPressed),dividerX,1,dividerX,heightTitle);
			SolidBrush brushPlusSign=UiTheme.GetBrush(isEnabled ? EnumUiColor.TextOnAccent : EnumUiColor.TextDisabled);
			g.FillRectangle(brushPlusSign,width-addW/2-plusSignWidth,2,plusSignWidth,addW-plusSignWidth);//vertical bar in "+" sign
			g.FillRectangle(brushPlusSign,width-addW,(addW-plusSignWidth)/2,addW-plusSignWidth,plusSignWidth);//horizontal bar in "+" sign
			return addW;
		}

		///<summary>The header's background (SurfaceSubtle) from y top, heightHeader device pixels down, across the width.</summary>
		public static void DrawHeaderBackground(Graphics g,int width,int top,int heightHeader) {
			g.FillRectangle(UiTheme.GetBrush(EnumUiColor.SurfaceSubtle),1,top,width,heightHeader);
		}

		///<summary>A 1 px Stroke column separator in the header at x, from top to bottom (exclusive).</summary>
		public static void DrawHeaderSeparator(Graphics g,int x,int top,int bottom) {
			if(bottom<=top) {
				return;
			}
			g.FillRectangle(UiTheme.GetBrush(EnumUiColor.Stroke),x,top,1,bottom-top);
		}

		///<summary>The header's bottom line (Stroke) at y.</summary>
		public static void DrawHeaderBottomLine(Graphics g,int width,int y) {
			g.DrawLine(UiTheme.GetPen(EnumUiColor.Stroke),0,y,width,y);
		}

		///<summary>The sort triangle centred on point (today's shape), in Text, without today's white halo (it was drawn for the blue header).</summary>
		public static void DrawSortArrow(Graphics g,PointF point,bool isAscending) {
			GraphicsState graphicsState=g.Save();
			g.SmoothingMode=SmoothingMode.HighQuality;
			if(isAscending) {//pointing up
				g.FillPolygon(UiTheme.GetBrush(EnumUiColor.Text),new PointF[] {
					new PointF(point.X-4,point.Y+2),//LL
					new PointF(point.X+4,point.Y+2),//LR
					new PointF(point.X,point.Y-2)});//Top
			}
			else {//pointing down
				g.FillPolygon(UiTheme.GetBrush(EnumUiColor.Text),new PointF[] {
					new PointF(point.X-4,point.Y-2),//UL
					new PointF(point.X+4,point.Y-2),//UR
					new PointF(point.X,point.Y+2)});//Bottom
			}
			g.Restore(graphicsState);
		}

		///<summary>A button cell: the button chrome of section 3.6 (UiControlPaint.DrawButton: SurfaceRaised, 1 px Stroke, 4 px corners; pressed
		///ButtonPressed; disabled #F5F5F5) in today's rectangle. Returns the colour of its text.</summary>
		public static Color DrawCellButton(Graphics g,Rectangle rectangle,bool isPressed,bool isEnabled,float scale) {
			if(rectangle.Width<=0 || rectangle.Height<=0) {
				return UiTheme.Text;
			}
			EnumUiButtonState buttonState=EnumUiButtonState.Normal;
			if(!isEnabled) {
				buttonState=EnumUiButtonState.Disabled;
			}
			else if(isPressed) {
				buttonState=EnumUiButtonState.Pressed;
			}
			//DrawButton's rectangle is the bounds; today's button was outlined one pixel beyond its fill, so it covers one more pixel each way.
			EnumUiColor uiColorText=UiControlPaint.DrawButton(g,new Rectangle(rectangle.X,rectangle.Y,rectangle.Width+1,rectangle.Height+1),buttonState,
				isPrimary:false,isFocused:false,scale);
			return UiTheme.GetColor(uiColorText);
		}

		///<summary>The selection bar (section 3.2) on the left edge of a selected row: round(2 x scale) device pixels wide (GetFocusWidth), inside the
		///row's interior rectangle (its fill without the gridlines), Accent while the grid has focus, SelectionBarNoFocus without.</summary>
		public static void DrawSelectionBar(Graphics g,Rectangle rectangleRow,bool hasFocus,float scale) {
			int width=Math.Min(UiTheme.GetFocusWidth(scale),rectangleRow.Width);
			if(width<=0 || rectangleRow.Height<=0) {
				return;
			}
			g.FillRectangle(UiTheme.GetBrush(hasFocus ? EnumUiColor.Accent : EnumUiColor.SelectionBarNoFocus),rectangleRow.X,rectangleRow.Y,width,rectangleRow.Height);
		}

		///<summary>The focused row (section 3.6): a solid Accent outline, max(1, floor(scale)) device pixels, inside the row's interior rectangle.</summary>
		public static void DrawFocusedRow(Graphics g,Rectangle rectangleRow,float scale) {
			if(rectangleRow.Width<=0 || rectangleRow.Height<=0) {
				return;
			}
			UiControlPaint.Outline(g,EnumUiColor.Accent,rectangleRow,UiTheme.GetLineWidth(scale),0,scale);
		}

		///<summary>The 1 px Stroke outer border around the whole control (section 3.6; today blue), max(1, floor(scale)) device pixels, inside its
		///bounds.</summary>
		public static void DrawOutline(Graphics g,int width,int height,float scale) {
			UiControlPaint.Outline(g,EnumUiColor.Stroke,new Rectangle(0,0,width,height),UiTheme.GetLineWidth(scale),0,scale);
		}
		#endregion Drawing
	}
}
