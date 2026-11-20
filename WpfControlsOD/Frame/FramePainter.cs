// The window frame of the new look (docs/specs/ui-refresh-plan.md, section 3.6 "Window frame", section 6 "Misc Colors"; package UI-3,
// docs/port-notes/UI-3.md): the title bar, its caption buttons and the outline that FormODBase (OpenPmsDental) and FormFrame (WpfControlsOD)
// draw in their PanelBorders. One painter for both, so the two frames give the same pixels for the same title.
//
// Its home is WpfControlsOD because it is the one project both frames reach that also knows the office's Misc Colors: OpenPmsDental
// references WpfControlsOD, WpfControlsOD cannot reach OpenPmsDental (OpenPmsDental/UI/BordersDpi), and CodeBase references no business layer,
// so DefCatMiscColors (OpenPmsDentalBusiness) is not known there. The tokens and the caption glyphs come from CodeBase (UiTheme, UiIcons).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	///<summary>Which frame a window gets: the ordinary window frame (the office's Misc Colors apply), or one of the floating image windows' two
	///frames, which show which one is selected even without focus (Misc Colors do not apply to them, as before).</summary>
	public enum EnumFrameLook {
		///<summary>Every window: TitleBar with TitleBarText and the TitleBarOutline, or the office's Misc Colors item by item.</summary>
		Window,
		///<summary>The selected floating image window (FormImageFloat with IsImageFloatSelected): Accent with white text.</summary>
		ImageFloatSelected,
		///<summary>A floating image window that is not selected: SurfaceSubtle with Text and a 1 px Stroke outline.</summary>
		ImageFloatUnselected
	}

	///<summary>The colours of one frame, resolved from the tokens and, for an ordinary window, the office's Misc Colors.</summary>
	public class FrameColors {
		///<summary>The title bar, and everything of the frame behind the client area.</summary>
		public Color TitleBar;
		///<summary>The title text and the caption glyphs at rest.</summary>
		public Color TitleBarText;
		///<summary>The 1 px outline around the window.</summary>
		public Color Outline;
		///<summary>The minimise and maximise buttons on hover.</summary>
		public Color CaptionHover;
		///<summary>The minimise and maximise glyphs on hover.</summary>
		public Color CaptionHoverGlyph;
		///<summary>The close button on hover: Danger.</summary>
		public Color CloseHover;
		///<summary>The close glyph on hover: white on Danger.</summary>
		public Color CloseHoverGlyph;
	}

	///<summary>What a frame needs to be painted: the size of the window, its title and caption buttons, which button the mouse is over, and the
	///scale functions of the window's LayoutManager or UIManager, so the painter computes every position exactly as the frames did.</summary>
	public class FramePaintArgs {
		///<summary>The window's width and height (Form.Width and Height, as the frames always used).</summary>
		public int Width;
		public int Height;
		public string Text="";
		public bool ControlBox=true;
		public bool MaximizeBox=true;
		public bool MinimizeBox=true;
		public bool IsMaximized;
		public bool ShowIcon=true;
		///<summary>The icon drawn at the left of the title bar when ShowIcon. Null draws none.</summary>
		public Icon Icon;
		public bool IsHotX;
		public bool IsHotMax;
		public bool IsHotMin;
		public EnumFrameLook FrameLook=EnumFrameLook.Window;
		///<summary>True for the main window, whose title is drawn in the larger title font (FormODBase._fontTitle).</summary>
		public bool IsMainWindowTitle;
		///<summary>The title bar height in device pixels (GetHeightTitleBar).</summary>
		public int HeightTitleBar;
		///<summary>The space right of the close button (WidthBorder).</summary>
		public int WidthBorder;
		///<summary>MaxInset (0 since the frames stopped insetting maximized windows).</summary>
		public int MaxInset;
		///<summary>A 96 DPI length in whole device pixels, the window's own Scale.</summary>
		public Func<float,int> Scale;
		///<summary>A 96 DPI length in device pixels, the window's own ScaleF.</summary>
		public Func<float,float> ScaleF;
		///<summary>The title font sizes and heights (GetTitleFonts).</summary>
		public FrameTitleFonts TitleFonts;
		///<summary>The part of the Graphics that the screen shows as the window, in the PanelBorders' coordinates (GetRectangleOnScreen): its Top is
		///the title bar's top pixel row and Right-1 the window's right pixel column. The caption buttons' hover fills reach them. Empty (a frame
		///painted without its PanelBorders laid out): the whole Width by Height inside MaxInset.</summary>
		public Rectangle RectangleOnScreen;
	}

	///<summary>The title font sizes and their heights, from the window's zoom and the screen it is on.</summary>
	public class FrameTitleFonts {
		public float FontSize;
		public float FontSizeMainWindow;
		public float HeightFont;
		public float HeightFontMainWindow;
	}

	///<summary>The caption button rectangles the painter laid out, for the frames' mouse handling. Today's rectangles, unchanged.</summary>
	public class FramePaintResult {
		public Rectangle RectangleButX;
		public Rectangle RectangleButMax;
		public Rectangle RectangleButMin;
		///<summary>The button area for the hit tests (from y 5, the top resize edge above it). Not for repainting: see RectangleButtonsPaint.</summary>
		public Rectangle RectangleButtons;
		///<summary>Everything the caption buttons paint: the button area and the buttons' fills, from the title bar's top row. The frames invalidate
		///this when a hover changes; RectangleButtons starts 5 px lower and clipped the top of the fill on screen.</summary>
		public Rectangle RectangleButtonsPaint;
		///<summary>Where the mouse hovers and clicks the close button (GetHitRectangle): its rectangle above and all of its painted fill, the top
		///resize edge of a normal window left out. The frames' mouse handling and the snap layouts' hit test use these three.</summary>
		public Rectangle RectangleHitX;
		public Rectangle RectangleHitMax;
		public Rectangle RectangleHitMin;
	}

	///<summary>Paints the window frame of the new look for FormODBase and FormFrame (section 3.6): the title bar in TitleBar (or the office's Misc
	///Colors 11 to 13, item by item, section 6), white title text, flat caption buttons with the Fluent glyphs (Subtract, Maximize,
	///SquareMultiple, Dismiss), CaptionHover on minimise and maximise, Danger on close, and a 1 px outline in TitleBarOutline. The floating image
	///windows get their selected and unselected frame. Positions and sizes are today's: only colours and glyphs change.</summary>
	public static class FramePainter {
		#region Misc Colors (section 6, UQ-3)
		///<summary>Open Dental's stock Main Border (Misc Colors item 11): 65,94,154, #415E9A.</summary>
		public const int MAIN_BORDER_STOCK_ARGB=-12493158;
		///<summary>Open Dental's stock Main Border Outline (item 12): black.</summary>
		public const int MAIN_BORDER_OUTLINE_STOCK_ARGB=-16777216;
		///<summary>Open Dental's stock Main Border Text (item 13): white.</summary>
		public const int MAIN_BORDER_TEXT_STOCK_ARGB=-1;

		///<summary>True when the Misc Colors item holds Open Dental's stock value, so it counts as "not customised" and the title bar takes its token
		///instead (section 6). False for every other item and for a changed colour. The Definitions window shows "(program default)" next to these.</summary>
		public static bool IsStockMiscColor(DefCatMiscColors defCatMiscColors,Color color) {
			int argb=color.ToArgb();
			switch(defCatMiscColors) {
				case DefCatMiscColors.MainBorder:
					return argb==MAIN_BORDER_STOCK_ARGB;
				case DefCatMiscColors.MainBorderOutline:
					return argb==MAIN_BORDER_OUTLINE_STOCK_ARGB;
				case DefCatMiscColors.MainBorderText:
					return argb==MAIN_BORDER_TEXT_STOCK_ARGB;
				default:
					return false;
			}
		}

		///<summary>True for the three Misc Colors items of the title bar (11, 12 and 13).</summary>
		public static bool IsTitleBarMiscColor(int itemOrder) {
			return itemOrder==(int)DefCatMiscColors.MainBorder || itemOrder==(int)DefCatMiscColors.MainBorderOutline
				|| itemOrder==(int)DefCatMiscColors.MainBorderText;
		}

		///<summary>The colours of an ordinary window's frame from the office's three Misc Colors, item by item: an item that holds the stock value
		///gives its token (Main Border gives TitleBar, Main Border Outline gives TitleBarOutline, Main Border Text gives TitleBarText), an item the
		///office changed gives the office's colour. Minimise and maximise hover: CaptionHover while Main Border and Main Border Text are both stock,
		///otherwise today's mix of the two (three parts border, one part text). Close hover: Danger with white. While Windows runs a high-contrast
		///theme the tokens win over the office's colours (section 1).</summary>
		public static FrameColors GetWindowColors(Color colorMainBorder,Color colorMainBorderOutline,Color colorMainBorderText) {
			return GetWindowColors(colorMainBorder,colorMainBorderOutline,colorMainBorderText,UiTheme.IsHighContrast);
		}

		///<summary>GetWindowColors with the high-contrast state given, for the tests.</summary>
		public static FrameColors GetWindowColors(Color colorMainBorder,Color colorMainBorderOutline,Color colorMainBorderText,bool isHighContrast) {
			bool isStockBorder=isHighContrast || IsStockMiscColor(DefCatMiscColors.MainBorder,colorMainBorder);
			bool isStockOutline=isHighContrast || IsStockMiscColor(DefCatMiscColors.MainBorderOutline,colorMainBorderOutline);
			bool isStockText=isHighContrast || IsStockMiscColor(DefCatMiscColors.MainBorderText,colorMainBorderText);
			FrameColors frameColors=new FrameColors();
			frameColors.TitleBar=isStockBorder ? UiTheme.GetColor(EnumUiColor.TitleBar,isHighContrast) : colorMainBorder;
			frameColors.Outline=isStockOutline ? UiTheme.GetColor(EnumUiColor.TitleBarOutline,isHighContrast) : colorMainBorderOutline;
			frameColors.TitleBarText=isStockText ? UiTheme.GetColor(EnumUiColor.TitleBarText,isHighContrast) : colorMainBorderText;
			if(isStockBorder && isStockText) {
				frameColors.CaptionHover=UiTheme.GetColor(EnumUiColor.CaptionHover,isHighContrast);
				frameColors.CaptionHoverGlyph=UiTheme.GetColor(EnumUiColor.TextOnAccent,isHighContrast);
			}
			else {
				//The office's own title bar: the hover colour it always had, mostly the border colour with some of the text colour.
				frameColors.CaptionHover=Color.FromArgb(
					(3*frameColors.TitleBar.R+frameColors.TitleBarText.R)/4,
					(3*frameColors.TitleBar.G+frameColors.TitleBarText.G)/4,
					(3*frameColors.TitleBar.B+frameColors.TitleBarText.B)/4);
				frameColors.CaptionHoverGlyph=frameColors.TitleBarText;
			}
			frameColors.CloseHover=UiTheme.GetColor(EnumUiColor.Danger,isHighContrast);
			frameColors.CloseHoverGlyph=UiTheme.GetColor(EnumUiColor.TextOnAccent,isHighContrast);
			return frameColors;
		}

		///<summary>The floating image windows' frames (section 3.6). Selected: Accent with white text, the TitleBarOutline, CaptionHover on hover.
		///Unselected: SurfaceSubtle with Text, a 1 px Stroke outline, SubtleHover on hover. Close hover Danger with white on both. Misc Colors do not
		///apply.</summary>
		public static FrameColors GetImageFloatColors(bool isSelected) {
			return GetImageFloatColors(isSelected,UiTheme.IsHighContrast);
		}

		///<summary>GetImageFloatColors with the high-contrast state given, for the tests.</summary>
		public static FrameColors GetImageFloatColors(bool isSelected,bool isHighContrast) {
			FrameColors frameColors=new FrameColors();
			if(isSelected) {
				frameColors.TitleBar=UiTheme.GetColor(EnumUiColor.Accent,isHighContrast);
				frameColors.TitleBarText=UiTheme.GetColor(EnumUiColor.TextOnAccent,isHighContrast);
				frameColors.Outline=UiTheme.GetColor(EnumUiColor.TitleBarOutline,isHighContrast);
				frameColors.CaptionHover=UiTheme.GetColor(EnumUiColor.CaptionHover,isHighContrast);
				frameColors.CaptionHoverGlyph=UiTheme.GetColor(EnumUiColor.TextOnAccent,isHighContrast);
			}
			else {
				frameColors.TitleBar=UiTheme.GetColor(EnumUiColor.SurfaceSubtle,isHighContrast);
				frameColors.TitleBarText=UiTheme.GetColor(EnumUiColor.Text,isHighContrast);
				frameColors.Outline=UiTheme.GetColor(EnumUiColor.Stroke,isHighContrast);
				frameColors.CaptionHover=UiTheme.GetColor(EnumUiColor.SubtleHover,isHighContrast);
				frameColors.CaptionHoverGlyph=UiTheme.GetColor(EnumUiColor.Text,isHighContrast);
			}
			frameColors.CloseHover=UiTheme.GetColor(EnumUiColor.Danger,isHighContrast);
			frameColors.CloseHoverGlyph=UiTheme.GetColor(EnumUiColor.TextOnAccent,isHighContrast);
			return frameColors;
		}

		///<summary>The colours for a frame look: the window's from its three Misc Colors, or a floating image window's.</summary>
		public static FrameColors GetColors(EnumFrameLook frameLook,Color colorMainBorder,Color colorMainBorderOutline,Color colorMainBorderText) {
			if(frameLook==EnumFrameLook.ImageFloatSelected) {
				return GetImageFloatColors(isSelected:true);
			}
			if(frameLook==EnumFrameLook.ImageFloatUnselected) {
				return GetImageFloatColors(isSelected:false);
			}
			return GetWindowColors(colorMainBorder,colorMainBorderOutline,colorMainBorderText);
		}
		#endregion Misc Colors (section 6, UQ-3)

		#region Title fonts
		///<summary>The title font sizes as the frames computed them: 8.25 (13 for the main window) at the window's zoom, made smaller on a secondary
		///screen whose DPI is lower than the primary's, and their heights. Null when the size comes out zero, negative or infinite (rare; the frames
		///then drew no title, a blind fix kept as it was).</summary>
		public static FrameTitleFonts GetTitleFonts(Control control,Func<float,float> scaleFontODZoom,Func<float,float> scaleMS) {
			FrameTitleFonts frameTitleFonts=new FrameTitleFonts();
			float fontSize=scaleFontODZoom(8.25f);
			float fontSizeMainWindow=scaleFontODZoom(13f);
			System.Windows.Forms.Screen screen=System.Windows.Forms.Screen.FromControl(control);
			System.Windows.Forms.Screen screenPri=System.Windows.Forms.Screen.PrimaryScreen;
			float scaleRatio=1;
			if(screen!=screenPri){//if we are on the secondary screen
				int dpiScreen=Dpi.GetScreenDpi(screen);//extern DllImport. Example 96
				int dpiScreenPri=Dpi.GetScreenDpi(screenPri);//Example 144
				scaleRatio=(float)dpiScreen/dpiScreenPri;//Example .67
				if(scaleRatio!=1){//example, pri screen is 150%, so it makes the fonts on this screen too big
					//We need to make them smaller
					fontSize=fontSize*scaleRatio;
					fontSizeMainWindow=fontSizeMainWindow*scaleRatio;
				}
			}
			//Rarely seems to be 0, neg, or infinity. Not sure which or why.
			//Blind fix:
			if(fontSize<=0 || float.IsInfinity(fontSize)){
				return null;
			}
			//The window titles' family token (UI-2a-1): Microsoft Sans Serif until UI-2b; the size is computed above.
			using(Font font=new Font(UiTheme.FontFamilyName,fontSize))
			using(Font fontMainWindow=new Font(UiTheme.FontFamilyName,fontSizeMainWindow)) {
				frameTitleFonts.HeightFont=scaleMS(font.Height);
				frameTitleFonts.HeightFontMainWindow=scaleMS(fontMainWindow.Height);
			}
			if(scaleRatio!=1){
				//special edge case where the font is smaller than what we need to measure
				//scale it again?
				frameTitleFonts.HeightFont=frameTitleFonts.HeightFont/scaleRatio;
				frameTitleFonts.HeightFontMainWindow=frameTitleFonts.HeightFontMainWindow/scaleRatio;
			}
			frameTitleFonts.FontSize=fontSize;
			frameTitleFonts.FontSizeMainWindow=fontSizeMainWindow;
			return frameTitleFonts;
		}
		#endregion Title fonts

		#region Paint
		///<summary>The glyph slot of a caption button at 96 DPI. The 16 px Fluent designs draw a 10 to 11 px mark, the size of the old glyphs.</summary>
		public const int CAPTION_GLYPH_SLOT=16;

		///<summary>Paints the whole frame into g (the PanelBorders' Graphics) and returns the caption button rectangles for the mouse handling. The
		///rectangles, the icon and the title text are where the frames always put them; the client area covers the rest of the fill.</summary>
		public static FramePaintResult Paint(Graphics g,FramePaintArgs args,FrameColors colors) {
			FramePaintResult result=new FramePaintResult();
			int maxInset=args.MaxInset;
			FillColor(g,colors.TitleBar,new Rectangle(0,0,args.Width,args.Height));
			//The outline: 1 px, 2 px at 200% (section 3.3: max(1, floor(scale)) device pixels), each pixel row on whole pixels.
			int widthLine=UiTheme.GetLineWidth(args.ScaleF(1));
			using(Pen penOutline=new Pen(colors.Outline)) {
				for(int i=0;i<widthLine;i++) {
					g.DrawRectangle(penOutline,maxInset+i,maxInset+i,args.Width-1-maxInset*2-i*2,args.Height-1-maxInset*2-i*2);
				}
			}
			//The caption buttons: today's rectangles, used for the hit tests.
			int wEdge=args.HeightTitleBar+args.Scale(4);//ours are nearly square instead of default wide
			int hEdge=args.HeightTitleBar-1;
			int xPos=args.Width-args.WidthBorder-maxInset;
			int yPos=5+maxInset;
			if(args.ControlBox){//there should always be a controlBox
				xPos-=wEdge;
				result.RectangleButX=new Rectangle(xPos,yPos,wEdge,hEdge);
			}
			if(args.ControlBox && args.MaximizeBox){
				xPos-=wEdge;
				result.RectangleButMax=new Rectangle(xPos,yPos,wEdge,hEdge);
			}
			if(args.ControlBox && args.MinimizeBox){
				xPos-=wEdge;
				result.RectangleButMin=new Rectangle(xPos,yPos,wEdge,hEdge);
			}
			result.RectangleButtons=new Rectangle(xPos,yPos,args.Width-xPos,args.HeightTitleBar);
			result.RectangleButtonsPaint=result.RectangleButtons;
			//Each button is drawn flat over the whole height of the title bar as the screen shows it, from its top pixel row, and the close button
			//also to the window's right pixel column (Windows 11's own caption buttons; the rounded corner and Windows' border are drawn over it).
			//The hit rectangle above starts 5 px lower, for the top resize edge; the hit areas (GetHitRectangle) add the fill below that edge, and
			//all of it on a maximized window. The glyph is centred in the fill (GetGlyphArea).
			int glyphSlot=args.Scale(CAPTION_GLYPH_SLOT);
			if(args.ControlBox) {
				Rectangle rectangleShape=GetButtonShape(result.RectangleButX,args,isClose:true);
				result.RectangleButtonsPaint=Rectangle.Union(result.RectangleButtonsPaint,rectangleShape);
				result.RectangleHitX=GetHitRectangle(result.RectangleButX,args,isClose:true);
				Color colorGlyph=colors.TitleBarText;
				if(args.IsHotX) {
					FillColor(g,colors.CloseHover,rectangleShape);
					colorGlyph=colors.CloseHoverGlyph;
				}
				UiIcons.Draw(g,UiGlyph.Dismiss,GetGlyphSlot(GetGlyphArea(result.RectangleButX,args,isClose:true),glyphSlot),colorGlyph);
			}
			if(args.ControlBox && args.MaximizeBox) {
				Rectangle rectangleShape=GetButtonShape(result.RectangleButMax,args,isClose:false);
				result.RectangleButtonsPaint=Rectangle.Union(result.RectangleButtonsPaint,rectangleShape);
				result.RectangleHitMax=GetHitRectangle(result.RectangleButMax,args,isClose:false);
				Color colorGlyph=colors.TitleBarText;
				if(args.IsHotMax) {
					FillColor(g,colors.CaptionHover,rectangleShape);
					colorGlyph=colors.CaptionHoverGlyph;
				}
				UiGlyph uiGlyph=args.IsMaximized ? UiGlyph.SquareMultiple : UiGlyph.Maximize;
				UiIcons.Draw(g,uiGlyph,GetGlyphSlot(GetGlyphArea(result.RectangleButMax,args,isClose:false),glyphSlot),colorGlyph);
			}
			if(args.ControlBox && args.MinimizeBox) {
				Rectangle rectangleShape=GetButtonShape(result.RectangleButMin,args,isClose:false);
				result.RectangleButtonsPaint=Rectangle.Union(result.RectangleButtonsPaint,rectangleShape);
				result.RectangleHitMin=GetHitRectangle(result.RectangleButMin,args,isClose:false);
				Color colorGlyph=colors.TitleBarText;
				if(args.IsHotMin) {
					FillColor(g,colors.CaptionHover,rectangleShape);
					colorGlyph=colors.CaptionHoverGlyph;
				}
				UiIcons.Draw(g,UiGlyph.Subtract,GetGlyphSlot(GetGlyphArea(result.RectangleButMin,args,isClose:false),glyphSlot),colorGlyph);
			}
			if(args.TitleFonts==null) {
				return result;
			}
			Rectangle rectangleIcon=new Rectangle(maxInset+5,maxInset+4,args.Scale(20),args.Scale(20));
			if(args.ShowIcon && args.Icon!=null){
				using(Bitmap bitmapIcon=args.Icon.ToBitmap()) {
					g.DrawImage(bitmapIcon,rectangleIcon);
				}
			}
			using(SolidBrush brushText=new SolidBrush(colors.TitleBarText)) {
				if(!args.IsMainWindowTitle){
					using(Font font=new Font(UiTheme.FontFamilyName,args.TitleFonts.FontSize)) {
						RectangleF rectangleText=new RectangleF(maxInset+args.ScaleF(30),maxInset+args.ScaleF(7),
							result.RectangleButtons.Left-args.ScaleF(30)-maxInset+args.ScaleF(8),args.TitleFonts.HeightFont+2);//The 8 is just so we can get closer to the right
						g.DrawString(args.Text,font,brushText,rectangleText);
					}
				}
				else{
					//We actually ignore the font size sent in
					using(Font fontMainWindow=new Font(UiTheme.FontFamilyName,args.TitleFonts.FontSizeMainWindow)) {
						RectangleF rectangleText=new RectangleF(
							x:maxInset+args.ScaleF(30),
							y:maxInset+args.ScaleF(3),
							width:result.RectangleButtons.Left-args.ScaleF(30)-maxInset+args.ScaleF(8),//The 8 is just so we can get closer to the right
							height:args.TitleFonts.HeightFontMainWindow+2);
						g.DrawString(args.Text,fontMainWindow,brushText,rectangleText);
					}
				}
			}
			return result;
		}

		///<summary>The flat shape of a caption button, its hover fill: its hit rectangle's columns (the close button's up to the window's right pixel
		///column), from the title bar's top pixel row on screen to the bottom of the title bar (args.RectangleOnScreen).</summary>
		public static Rectangle GetButtonShape(Rectangle rectangleButton,FramePaintArgs args,bool isClose) {
			Rectangle rectangleOnScreen=GetRectangleOnScreen(args);
			int top=rectangleOnScreen.Top;
			int bottom=Math.Max(top,args.MaxInset+args.HeightTitleBar+1);
			int right=Math.Min(rectangleButton.Right,rectangleOnScreen.Right);
			if(isClose) {
				right=rectangleOnScreen.Right;
			}
			return Rectangle.FromLTRB(rectangleButton.Left,top,Math.Max(rectangleButton.Left,right),bottom);
		}

		///<summary>Where a button's glyph is centred: its painted fill (GetButtonShape), as the screen shows it, at every scale. At 100% the fill is
		///where the glyph was always centred (the hit rectangle's columns from just inside the outline to the bottom of the title bar), so nothing
		///moves; above 100% the close glyph moves right to the middle of its wider fill and every glyph by up to a pixel to the fill's middle row.</summary>
		public static Rectangle GetGlyphArea(Rectangle rectangleButton,FramePaintArgs args,bool isClose) {
			return GetButtonShape(rectangleButton,args,isClose);
		}

		///<summary>The glyph slot (CAPTION_GLYPH_SLOT at the window's scale) centred in a button's painted fill: where Paint draws its glyph.</summary>
		public static Rectangle GetGlyphRectangle(Rectangle rectangleButton,FramePaintArgs args,bool isClose) {
			return GetGlyphSlot(GetGlyphArea(rectangleButton,args,isClose),args.Scale(CAPTION_GLYPH_SLOT));
		}

		///<summary>Where the mouse hovers and clicks a caption button: its rectangle (rectangleButton, from y 5) and all of its painted fill
		///(GetButtonShape) except, on a normal window, the rows above the rectangle's top, which stay the top resize edge (and, with the
		///columns right of the client area, which belong to Windows, the top-right resize corner), as on Windows 11. So the close button's hit
		///area reaches the client area's right edge below that edge, and the hover stays on while the pointer is anywhere in the fill below it.
		///On a maximized window there is no resize edge: the whole fill, from the screen's top row, and for the close button its last column,
		///so the screen's top-right pixel closes the window.</summary>
		public static Rectangle GetHitRectangle(Rectangle rectangleButton,FramePaintArgs args,bool isClose) {
			if(rectangleButton.Width<1 || rectangleButton.Height<1) {
				return rectangleButton;
			}
			Rectangle rectangleShape=GetButtonShape(rectangleButton,args,isClose);
			if(!args.IsMaximized) {
				rectangleShape=Rectangle.FromLTRB(rectangleShape.Left,Math.Max(rectangleShape.Top,rectangleButton.Top),
					rectangleShape.Right,Math.Max(rectangleShape.Bottom,rectangleButton.Top));
			}
			if(rectangleShape.Width<1 || rectangleShape.Height<1) {
				return rectangleButton;
			}
			return Rectangle.Union(rectangleButton,rectangleShape);
		}

		///<summary>args.RectangleOnScreen, or the whole Width by Height inside MaxInset when it is empty.</summary>
		public static Rectangle GetRectangleOnScreen(FramePaintArgs args) {
			if(!args.RectangleOnScreen.IsEmpty) {
				return args.RectangleOnScreen;
			}
			return new Rectangle(args.MaxInset,args.MaxInset,args.Width-args.MaxInset*2,args.Height-args.MaxInset*2);
		}

		///<summary>The width of the invisible resize border left, right and below the window's client area at a screen scale (8 at 100%, 9 at 125%,
		///11 at 150%, 13 at 200%): the frames' WM_NCCALCSIZE and LayoutFormBoundsAndFonts use the same formula.</summary>
		public static int GetWidthBorderMS(float scaleMS) {
			return (int)Math.Round(8+(5*(scaleMS-1)),MidpointRounding.AwayFromZero);
		}

		///<summary>The part of a frame's PanelBorders that the screen shows, in the PanelBorders' coordinates. The window's client area is the window
		///less the invisible resize borders left, right and below (WM_NCCALCSIZE; its top is the window's top); the PanelBorders sit at (-1,-1) of
		///it, so their row and column 0 are outside, and their right edge is the client area's. A maximized window reaches past the screen by the
		///same border on every side, so its client rows above that border are off the screen. Empty when the PanelBorders are not laid out.</summary>
		public static Rectangle GetRectangleOnScreen(Size sizeWindow,bool isMaximized,float scaleMS,Rectangle boundsPanelBorders) {
			if(boundsPanelBorders.Width<1 || boundsPanelBorders.Height<1) {
				return Rectangle.Empty;
			}
			int widthBorderMS=GetWidthBorderMS(scaleMS);
			int top=isMaximized ? widthBorderMS : 0;
			Rectangle rectangleClient=Rectangle.FromLTRB(0,top,sizeWindow.Width-widthBorderMS*2,sizeWindow.Height-widthBorderMS);
			Rectangle rectangleOnScreen=Rectangle.Intersect(rectangleClient,boundsPanelBorders);
			if(rectangleOnScreen.Width<1 || rectangleOnScreen.Height<1) {
				return Rectangle.Empty;
			}
			rectangleOnScreen.Offset(-boundsPanelBorders.X,-boundsPanelBorders.Y);
			return rectangleOnScreen;
		}

		///<summary>The glyph slot centred in a button's shape.</summary>
		private static Rectangle GetGlyphSlot(Rectangle rectangleShape,int glyphSlot) {
			return new Rectangle(rectangleShape.X+(rectangleShape.Width-glyphSlot)/2,rectangleShape.Y+(rectangleShape.Height-glyphSlot)/2,glyphSlot,glyphSlot);
		}

		///<summary>Fills with a brush of its own: the frame's colours may be the office's, not tokens.</summary>
		private static void FillColor(Graphics g,Color color,Rectangle rectangle) {
			using(SolidBrush solidBrush=new SolidBrush(color)) {
				g.FillRectangle(solidBrush,rectangle);
			}
		}
		#endregion Paint

		#region DWM rounded corners
		///<summary>The environment variable that chooses the corners of the program's own frame (UI-3; the owner kept Windows 11's rounded corners
		///on 2026-11-18): unset, empty or "round", rounded corners (the default); "small", small rounded corners; "off" (or "square" or "none"),
		///nothing is asked of Windows and the frame is as it was before UI-3. Any other value gives the default. While corners are asked for, Windows
		///is also asked to draw its 1 px window border in the frame's outline colour (DWMWA_BORDER_COLOR): the frame's own outline lies under the
		///client area or outside the PanelBorders, so the border seen on screen is Windows' own.</summary>
		public const string ENV_DWM_CORNERS="OPENPMS_UI3_DWM_CORNERS";
		///<summary>DWMWA_WINDOW_CORNER_PREFERENCE (Windows 11 build 22000 and later).</summary>
		private const int DWMWA_WINDOW_CORNER_PREFERENCE=33;
		///<summary>DWMWA_BORDER_COLOR (Windows 11 build 22000 and later): a COLORREF, 0x00BBGGRR.</summary>
		private const int DWMWA_BORDER_COLOR=34;
		public const int DWMWCP_ROUND=2;
		public const int DWMWCP_ROUNDSMALL=3;

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(IntPtr hwnd,int dwAttribute,ref int pvAttribute,int cbAttribute);

		///<summary>The corner preference asked of Windows: DWMWCP_ROUND by default, DWMWCP_ROUNDSMALL for "small", 0 (nothing is set) for "off",
		///"square" or "none" in ENV_DWM_CORNERS.</summary>
		public static int GetDwmCornerPreference() {
			string value;
			try {
				value=Environment.GetEnvironmentVariable(ENV_DWM_CORNERS);
			}
			catch(Exception) {
				value=null;//not allowed to read the environment: the default
			}
			return GetDwmCornerPreference(value);
		}

		///<summary>GetDwmCornerPreference for a value of ENV_DWM_CORNERS, for the tests.</summary>
		public static int GetDwmCornerPreference(string value) {
			value=(value??"").Trim();
			if(string.Equals(value,"off",StringComparison.OrdinalIgnoreCase)
				|| string.Equals(value,"square",StringComparison.OrdinalIgnoreCase)
				|| string.Equals(value,"none",StringComparison.OrdinalIgnoreCase))
			{
				return 0;
			}
			if(string.Equals(value,"small",StringComparison.OrdinalIgnoreCase)) {
				return DWMWCP_ROUNDSMALL;
			}
			return DWMWCP_ROUND;
		}

		///<summary>The COLORREF (0x00BBGGRR) of a colour, as DWMWA_BORDER_COLOR takes it.</summary>
		public static int ToColorRef(Color color) {
			return color.R | (color.G<<8) | (color.B<<16);
		}

		///<summary>Asks Windows to round the window's corners and to draw its border in colorOutline (GetDwmCornerPreference: by default; not when
		///ENV_DWM_CORNERS is "off"). Nothing with the Windows frame (AreBordersMS), which Windows 11 rounds by itself. Windows 10 refuses both
		///attributes; the results are ignored. Called again with the same preference when the outline colour changes.</summary>
		public static void ApplyDwmCorners(IntPtr hwnd,bool areBordersMS,Color colorOutline) {
			ApplyDwmCorners(hwnd,areBordersMS,colorOutline,GetDwmCornerPreference());
		}

		///<summary>ApplyDwmCorners with the corner preference given (0: nothing is asked of Windows), for the tests. The frames call it again, with
		///the outline colour the painter uses, whenever that colour changes: the office's Misc Colors loaded or changed (SetBorderColor), a
		///floating image window selected or not (IsImageFloatSelected).</summary>
		public static void ApplyDwmCorners(IntPtr hwnd,bool areBordersMS,Color colorOutline,int preference) {
			if(areBordersMS || hwnd==IntPtr.Zero || preference==0) {
				return;
			}
			try {
				DwmSetWindowAttribute(hwnd,DWMWA_WINDOW_CORNER_PREFERENCE,ref preference,sizeof(int));
				int colorRef=ToColorRef(colorOutline);
				DwmSetWindowAttribute(hwnd,DWMWA_BORDER_COLOR,ref colorRef,sizeof(int));
			}
			catch(Exception) {
				//No dwmapi entry point (not expected on Windows 10 or 11): the frame stays as before.
			}
			DwmBorderColorApplied?.Invoke(hwnd,colorOutline);
		}

		///<summary>Raised after Windows was asked for the corners and the border colour of a window (the handle and the colour), for the tests,
		///which never show a window and so cannot read the border off the screen.</summary>
		public static event Action<IntPtr,Color> DwmBorderColorApplied;
		#endregion DWM rounded corners
	}
}
