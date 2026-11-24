// The window frame of the new look (docs/specs/ui-refresh-plan.md, section 3.6 "Window frame", section 6 "Misc Colors"; package UI-3,
// docs/port-notes/UI-3.md): the title bar, its caption buttons and the outline that FormODBase (OpenPmsDental) and FormFrame (WpfControlsOD)
// draw in their PanelBorders. One painter for both, so the two frames give the same pixels for the same title.
//
// Its home is WpfControlsOD because it is the one project both frames reach that also knows the office's Misc Colors: OpenPmsDental
// references WpfControlsOD, WpfControlsOD cannot reach OpenPmsDental (OpenPmsDental/UI/BordersDpi), and CodeBase references no business layer,
// so DefCatMiscColors (OpenPmsDentalBusiness) is not known there. UI-PLAIN-2 (docs/port-notes/UI-PLAIN.md section 8): the drawing itself, its
// types and the DWM corners moved to CodeBase's UiFramePainter, so the plain windows (UiPlainFrame) draw the same frame; this class keeps the
// Misc Colors and hands the rest to it, with the names the frames and the tests always called.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	///<summary>Paints the window frame of the new look for FormODBase and FormFrame (section 3.6) with CodeBase's UiFramePainter, the colours
	///resolved from the office's Misc Colors 11 to 13, item by item (section 6). The floating image windows get their selected and unselected
	///frame. Positions and sizes are today's: only colours and glyphs change.</summary>
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
			return UiFramePainter.GetWindowColors(colorMainBorder,colorMainBorderOutline,colorMainBorderText,
				IsStockMiscColor(DefCatMiscColors.MainBorder,colorMainBorder),
				IsStockMiscColor(DefCatMiscColors.MainBorderOutline,colorMainBorderOutline),
				IsStockMiscColor(DefCatMiscColors.MainBorderText,colorMainBorderText),
				isHighContrast);
		}

		///<summary>The floating image windows' frames (section 3.6). Selected: Accent with white text, the TitleBarOutline, CaptionHover on hover.
		///Unselected: SurfaceSubtle with Text, a 1 px Stroke outline, SubtleHover on hover. Close hover Danger with white on both. Misc Colors do not
		///apply.</summary>
		public static FrameColors GetImageFloatColors(bool isSelected) {
			return GetImageFloatColors(isSelected,UiTheme.IsHighContrast);
		}

		///<summary>GetImageFloatColors with the high-contrast state given, for the tests.</summary>
		public static FrameColors GetImageFloatColors(bool isSelected,bool isHighContrast) {
			return UiFramePainter.GetImageFloatColors(isSelected,isHighContrast);
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

		#region Title fonts, paint and DWM corners (UiFramePainter)
		///<summary>UiFramePainter.GetTitleFonts.</summary>
		public static FrameTitleFonts GetTitleFonts(Control control,Func<float,float> scaleFontODZoom,Func<float,float> scaleMS) {
			return UiFramePainter.GetTitleFonts(control,scaleFontODZoom,scaleMS);
		}

		///<summary>UiFramePainter.CAPTION_GLYPH_SLOT.</summary>
		public const int CAPTION_GLYPH_SLOT=UiFramePainter.CAPTION_GLYPH_SLOT;

		///<summary>UiFramePainter.Paint.</summary>
		public static FramePaintResult Paint(Graphics g,FramePaintArgs args,FrameColors colors) {
			return UiFramePainter.Paint(g,args,colors);
		}

		///<summary>UiFramePainter.GetButtonShape.</summary>
		public static Rectangle GetButtonShape(Rectangle rectangleButton,FramePaintArgs args,bool isClose) {
			return UiFramePainter.GetButtonShape(rectangleButton,args,isClose);
		}

		///<summary>UiFramePainter.GetGlyphArea.</summary>
		public static Rectangle GetGlyphArea(Rectangle rectangleButton,FramePaintArgs args,bool isClose) {
			return UiFramePainter.GetGlyphArea(rectangleButton,args,isClose);
		}

		///<summary>UiFramePainter.GetGlyphRectangle.</summary>
		public static Rectangle GetGlyphRectangle(Rectangle rectangleButton,FramePaintArgs args,bool isClose) {
			return UiFramePainter.GetGlyphRectangle(rectangleButton,args,isClose);
		}

		///<summary>UiFramePainter.GetHitRectangle.</summary>
		public static Rectangle GetHitRectangle(Rectangle rectangleButton,FramePaintArgs args,bool isClose) {
			return UiFramePainter.GetHitRectangle(rectangleButton,args,isClose);
		}

		///<summary>UiFramePainter.GetRectangleOnScreen.</summary>
		public static Rectangle GetRectangleOnScreen(FramePaintArgs args) {
			return UiFramePainter.GetRectangleOnScreen(args);
		}

		///<summary>UiFramePainter.GetWidthBorderMS.</summary>
		public static int GetWidthBorderMS(float scaleMS) {
			return UiFramePainter.GetWidthBorderMS(scaleMS);
		}

		///<summary>UiFramePainter.GetRectangleOnScreen.</summary>
		public static Rectangle GetRectangleOnScreen(Size sizeWindow,bool isMaximized,float scaleMS,Rectangle boundsPanelBorders) {
			return UiFramePainter.GetRectangleOnScreen(sizeWindow,isMaximized,scaleMS,boundsPanelBorders);
		}

		///<summary>UiFramePainter.ENV_DWM_CORNERS.</summary>
		public const string ENV_DWM_CORNERS=UiFramePainter.ENV_DWM_CORNERS;
		public const int DWMWCP_ROUND=UiFramePainter.DWMWCP_ROUND;
		public const int DWMWCP_ROUNDSMALL=UiFramePainter.DWMWCP_ROUNDSMALL;

		///<summary>UiFramePainter.GetDwmCornerPreference.</summary>
		public static int GetDwmCornerPreference() {
			return UiFramePainter.GetDwmCornerPreference();
		}

		///<summary>UiFramePainter.GetDwmCornerPreference.</summary>
		public static int GetDwmCornerPreference(string value) {
			return UiFramePainter.GetDwmCornerPreference(value);
		}

		///<summary>UiFramePainter.ToColorRef.</summary>
		public static int ToColorRef(Color color) {
			return UiFramePainter.ToColorRef(color);
		}

		///<summary>UiFramePainter.ApplyDwmCorners.</summary>
		public static void ApplyDwmCorners(IntPtr hwnd,bool areBordersMS,Color colorOutline) {
			UiFramePainter.ApplyDwmCorners(hwnd,areBordersMS,colorOutline);
		}

		///<summary>UiFramePainter.ApplyDwmCorners.</summary>
		public static void ApplyDwmCorners(IntPtr hwnd,bool areBordersMS,Color colorOutline,int preference) {
			UiFramePainter.ApplyDwmCorners(hwnd,areBordersMS,colorOutline,preference);
		}

		///<summary>UiFramePainter.DwmBorderColorApplied: raised after Windows was asked for the corners and the border colour of a window.</summary>
		public static event Action<IntPtr,Color> DwmBorderColorApplied {
			add {
				UiFramePainter.DwmBorderColorApplied+=value;
			}
			remove {
				UiFramePainter.DwmBorderColorApplied-=value;
			}
		}
		#endregion Title fonts, paint and DWM corners (UiFramePainter)
	}
}
