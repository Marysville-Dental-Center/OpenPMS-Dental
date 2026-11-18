// The shapes of the self-drawn WinForms calendar and date pickers (docs/specs/ui-refresh-plan.md, sections 3.2, 3.3 and 3.6; package UI-4b-3,
// docs/port-notes/UI-4b.md): a day under the mouse (SubtleHover, 4 px corners), the selected day (Accent with white text), today (a 1 px Accent ring,
// a white inner ring when today is also the selected day), the selected day's focus (the Accent ring outside a white inner ring), the Fluent
// chevrons of the month and year arrows, and the Fluent calendar glyph of the date pickers' drop-down button. Drawn with UiControlPaint's shapes
// (UI-4a), so the snapping rule of section 3.3 holds, and the same shapes as the design sheet's calendar (tests/UiRefreshTests/DesignSheetControls.cs,
// DrawCalendarDay and DrawCalendarAndProgress). Windows Forms and System.Drawing only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;

namespace CodeBase {
	///<summary>The token shapes of the self-drawn WinForms calendar and date pickers (section 3.6), in device pixels at a scale.</summary>
	public static class UiCalendarPaint {
		///<summary>The EnumIcons member whose Fluent icon is calendar_ltr (the Appointments module's icon, UI-7b): the date pickers' calendar glyph.
		///The glyph is taken from there so no icon is added for it.</summary>
		public const string ICON_CALENDAR="Appt32";
		///<summary>The size of the calendar glyph and of the arrows' chevrons at 96 DPI.</summary>
		public const int SIZE_GLYPH=12;
		///<summary>The designs of ICON_CALENDAR, found once.</summary>
		private static volatile IconDesign[] _arrayDesignsCalendar;

		#region Days
		///<summary>A day under the mouse (and the header's buttons under the mouse): SubtleHover with 4 px corners.</summary>
		public static void DrawHover(Graphics g,Rectangle rectangle,float scale) {
			UiControlPaint.Fill(g,EnumUiColor.SubtleHover,rectangle,UiTheme.CORNER_CONTROL,scale);
		}

		///<summary>The selected day (section 3.6): Accent with 4 px corners; TextDisabled when the calendar is disabled. Returns the token of its text
		///(TextOnAccent).</summary>
		public static EnumUiColor DrawSelectedDay(Graphics g,Rectangle rectangleDay,bool isEnabled,float scale) {
			UiControlPaint.Fill(g,isEnabled ? EnumUiColor.Accent : EnumUiColor.TextDisabled,rectangleDay,UiTheme.CORNER_CONTROL,scale);
			return EnumUiColor.TextOnAccent;
		}

		///<summary>Today (section 3.6): a 1 px (GetLineWidth) Accent ring inside the day with 4 px corners; when today is also the selected day, a 1 px
		///white ring inside it, so the mark shows on the accent fill.</summary>
		public static void DrawToday(Graphics g,Rectangle rectangleDay,bool isSelected,float scale) {
			int widthLine=UiTheme.GetLineWidth(scale);
			UiControlPaint.Outline(g,EnumUiColor.Accent,rectangleDay,widthLine,UiTheme.CORNER_CONTROL,scale);
			if(isSelected) {
				UiControlPaint.Outline(g,EnumUiColor.TextOnAccent,Rectangle.Inflate(rectangleDay,-widthLine,-widthLine),widthLine,UiTheme.CORNER_CONTROL-1,scale);
			}
		}

		///<summary>The calendar's focus (section 3.6, an accent-filled control): the 2 px Accent ring outside a 1 px white inner ring, on the selected day.</summary>
		public static void DrawSelectedDayFocus(Graphics g,Rectangle rectangleDay,float scale) {
			UiControlPaint.FocusRingOnAccent(g,rectangleDay,UiTheme.CORNER_CONTROL,scale);
		}

		///<summary>The text token of a day: TextOnAccent on the selected day, Text in the month shown, TextDisabled for the days of the months around it and
		///for every day of a disabled calendar.</summary>
		public static EnumUiColor GetDayText(bool isSelected,bool isMonthShown,bool isEnabled) {
			if(isSelected) {
				return EnumUiColor.TextOnAccent;
			}
			if(!isEnabled || !isMonthShown) {
				return EnumUiColor.TextDisabled;
			}
			return EnumUiColor.Text;
		}
		#endregion Days

		#region Glyphs
		///<summary>The square of a glyph SIZE_GLYPH at the scale, centred on a point (device pixels), on whole pixels.</summary>
		public static Rectangle GetGlyphRectangle(PointF pointCentre,float scale) {
			int size=UiTheme.ScaleI(SIZE_GLYPH,scale);
			return new Rectangle((int)Math.Round(pointCentre.X-size/2f),(int)Math.Round(pointCentre.Y-size/2f),size,size);
		}

		///<summary>A month or year arrow (section 3.6): the Fluent chevron, left for back and right for forward, in its square.</summary>
		public static void DrawArrow(Graphics g,Rectangle rectangleGlyph,bool isForward,EnumUiColor uiColor) {
			UiIcons.Draw(g,isForward ? UiGlyph.ChevronRight : UiGlyph.ChevronLeft,rectangleGlyph,uiColor);
		}

		///<summary>The Fluent calendar glyph (calendar_ltr, ICON_CALENDAR's design) for a square of sizeSlot device pixels, or null when the icon set
		///has none.</summary>
		public static IconDesign GetCalendarDesign(int sizeSlot) {
			IconDesign[] arrayDesigns=_arrayDesignsCalendar;
			if(arrayDesigns==null) {
				arrayDesigns=UiIcons.GetDesigns("EnumIcons",ICON_CALENDAR);
				_arrayDesignsCalendar=arrayDesigns;//read-only after this; a second thread at the same moment only finds the same designs
			}
			if(arrayDesigns.Length==0) {
				return null;
			}
			return UiIcons.GetDesign(arrayDesigns,sizeSlot);
		}

		///<summary>The date pickers' calendar glyph in its square, in the token (Text, TextDisabled when disabled).</summary>
		public static void DrawCalendarGlyph(Graphics g,Rectangle rectangleGlyph,EnumUiColor uiColor) {
			IconDesign iconDesign=GetCalendarDesign(rectangleGlyph.Width);
			if(iconDesign==null) {
				return;
			}
			UiIcons.Draw(g,iconDesign,rectangleGlyph,UiTheme.GetColor(uiColor));
		}
		#endregion Glyphs
	}
}
