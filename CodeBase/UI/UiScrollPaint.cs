// UI-13 (docs/port-notes/UI-13.md, the owner's G-UI3-ui12p rows "Scrolling still using old style" and "Scrolling bar style is old"): the one look
// of every scroll bar in the program, the Windows 11 Fluent one in the tokens: a flat track in Surface (no hatching, no 3-D arrow blocks), a
// rounded thumb (a pill along the bar, inset across it) in StrokeStrong, TextSecondary under the mouse and Text while dragged, and small Fluent
// chevrons in the arrow squares (StrokeStrong, TextSecondary under the mouse with a SubtleHover square, Text pressed with SubtlePressed). A
// disabled bar shows its track and TextDisabled chevrons, no thumb. Every size follows the bar's thickness (17 px at 96 DPI, the Windows
// metric), so the look scales with dpi and zoom. UiScrollLook (OpenPmsDental/UI) paints it over the stock scroll bar controls (the grids', the
// lists', the combo box picker's) and the scroll bars Windows draws in a window's frame (text boxes, rich text boxes, stock lists, tree and list
// views, scrolling panels). Windows Forms and System.Drawing only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;

namespace CodeBase {
	///<summary>The parts of a scroll bar, from its start (top or left) to its end.</summary>
	public enum EnumUiScrollPart {
		None,
		///<summary>The arrow at the top or left.</summary>
		ArrowStart,
		///<summary>The track between the start arrow and the thumb.</summary>
		TrackStart,
		Thumb,
		///<summary>The track between the thumb and the end arrow.</summary>
		TrackEnd,
		///<summary>The arrow at the bottom or right.</summary>
		ArrowEnd,
	}

	///<summary>The token look of a scroll bar (section 3.6, UI-13), in device pixels.</summary>
	public static class UiScrollPaint {
		///<summary>The bar's thickness at 96 DPI (SM_CXVSCROLL): the size every other measure is relative to.</summary>
		public const int THICKNESS_BAR=17;
		///<summary>The thumb's inset across the bar at 96 DPI on each side: a 17 px bar has a 7 px thumb.</summary>
		public const int INSET_THUMB_ACROSS=5;
		///<summary>The thumb's inset along the bar at 96 DPI at each end, so it never touches an arrow square.</summary>
		public const int INSET_THUMB_ALONG=2;
		///<summary>The arrow chevron's square at 96 DPI.</summary>
		public const int SIZE_ARROW_GLYPH=8;
		///<summary>The inset of an arrow square's hover fill at 96 DPI.</summary>
		public const int INSET_ARROW_FILL=2;

		#region Tokens
		///<summary>The track: Surface, the program's flat container colour (enabled or not).</summary>
		public static EnumUiColor GetTrackColor() {
			return EnumUiColor.Surface;
		}

		///<summary>The thumb: StrokeStrong at rest, TextSecondary under the mouse, Text while dragged, Stroke when disabled (not drawn then).</summary>
		public static EnumUiColor GetThumbColor(EnumUiButtonState buttonState) {
			switch(buttonState) {
				case EnumUiButtonState.Hover:
					return EnumUiColor.TextSecondary;
				case EnumUiButtonState.Pressed:
					return EnumUiColor.Text;
				case EnumUiButtonState.Disabled:
					return EnumUiColor.Stroke;
				default:
					return EnumUiColor.StrokeStrong;
			}
		}

		///<summary>An arrow's chevron: StrokeStrong at rest, TextSecondary under the mouse, Text pressed, TextDisabled when the bar is disabled.</summary>
		public static EnumUiColor GetArrowColor(EnumUiButtonState buttonState) {
			switch(buttonState) {
				case EnumUiButtonState.Hover:
					return EnumUiColor.TextSecondary;
				case EnumUiButtonState.Pressed:
					return EnumUiColor.Text;
				case EnumUiButtonState.Disabled:
					return EnumUiColor.TextDisabled;
				default:
					return EnumUiColor.StrokeStrong;
			}
		}

		///<summary>An arrow square's fill: none at rest (null), SubtleHover under the mouse, SubtlePressed pressed.</summary>
		public static EnumUiColor? GetArrowFill(EnumUiButtonState buttonState) {
			switch(buttonState) {
				case EnumUiButtonState.Hover:
					return EnumUiColor.SubtleHover;
				case EnumUiButtonState.Pressed:
					return EnumUiColor.SubtlePressed;
				default:
					return null;
			}
		}
		#endregion Tokens

		#region Geometry
		///<summary>The scale of a bar from its thickness in device pixels: 1 for the 17 px bar at 96 DPI, 1.5 for 25 or 26 px, never under 1.</summary>
		public static float GetScale(int thickness) {
			return Math.Max(1f,thickness/(float)THICKNESS_BAR);
		}

		///<summary>The thumb's drawn pill inside its hit rectangle (the part a click drags): inset INSET_THUMB_ACROSS across the bar and
		///INSET_THUMB_ALONG along it, at least 2 px thick and as long as it is thick. Empty when the hit rectangle is empty.</summary>
		public static Rectangle GetThumbShape(Rectangle rectangleThumbHit,bool isVertical,float scale) {
			if(rectangleThumbHit.Width<=0 || rectangleThumbHit.Height<=0) {
				return Rectangle.Empty;
			}
			int across=isVertical ? rectangleThumbHit.Width : rectangleThumbHit.Height;
			int insetAcross=UiTheme.ScaleI(INSET_THUMB_ACROSS,scale);
			int thickness=Math.Max(2,across-2*insetAcross);
			insetAcross=(across-thickness)/2;
			int insetAlong=UiTheme.ScaleI(INSET_THUMB_ALONG,scale);
			if(isVertical) {
				int length=Math.Max(thickness,rectangleThumbHit.Height-2*insetAlong);
				int top=rectangleThumbHit.Y+(rectangleThumbHit.Height-length)/2;
				return new Rectangle(rectangleThumbHit.X+insetAcross,top,thickness,length);
			}
			int width=Math.Max(thickness,rectangleThumbHit.Width-2*insetAlong);
			int left=rectangleThumbHit.X+(rectangleThumbHit.Width-width)/2;
			return new Rectangle(left,rectangleThumbHit.Y+insetAcross,width,thickness);
		}

		///<summary>The square of an arrow's chevron: SIZE_ARROW_GLYPH at the scale, centred in the arrow's rectangle on whole pixels.</summary>
		public static Rectangle GetArrowGlyphRectangle(Rectangle rectangleArrow,float scale) {
			int size=UiTheme.ScaleI(SIZE_ARROW_GLYPH,scale);
			return new Rectangle(rectangleArrow.X+(rectangleArrow.Width-size)/2,rectangleArrow.Y+(rectangleArrow.Height-size)/2,size,size);
		}

		///<summary>The arrow rectangles of a bar: lengthArrow device pixels at each end (Windows' own arrow length, never more than half the bar).
		///</summary>
		public static void GetArrowRectangles(Rectangle rectangleBar,bool isVertical,int lengthArrow,out Rectangle rectangleStart,out Rectangle rectangleEnd) {
			if(isVertical) {
				int length=Math.Max(0,Math.Min(lengthArrow,rectangleBar.Height/2));
				rectangleStart=new Rectangle(rectangleBar.X,rectangleBar.Y,rectangleBar.Width,length);
				rectangleEnd=new Rectangle(rectangleBar.X,rectangleBar.Bottom-length,rectangleBar.Width,length);
				return;
			}
			int width=Math.Max(0,Math.Min(lengthArrow,rectangleBar.Width/2));
			rectangleStart=new Rectangle(rectangleBar.X,rectangleBar.Y,width,rectangleBar.Height);
			rectangleEnd=new Rectangle(rectangleBar.Right-width,rectangleBar.Y,width,rectangleBar.Height);
		}

		///<summary>The thumb's hit rectangle from its start and end along the bar (relative to the bar's top or left, as Windows reports them in
		///SCROLLBARINFO). Empty when there is no thumb (end not after start).</summary>
		public static Rectangle GetThumbHitRectangle(Rectangle rectangleBar,bool isVertical,int thumbStart,int thumbEnd) {
			if(thumbEnd<=thumbStart) {
				return Rectangle.Empty;
			}
			if(isVertical) {
				return new Rectangle(rectangleBar.X,rectangleBar.Y+thumbStart,rectangleBar.Width,thumbEnd-thumbStart);
			}
			return new Rectangle(rectangleBar.X+thumbStart,rectangleBar.Y,thumbEnd-thumbStart,rectangleBar.Height);
		}

		///<summary>The part of the bar under the point (None outside the bar).</summary>
		public static EnumUiScrollPart GetPart(Point point,Rectangle rectangleBar,bool isVertical,int lengthArrow,int thumbStart,int thumbEnd) {
			if(!rectangleBar.Contains(point)) {
				return EnumUiScrollPart.None;
			}
			GetArrowRectangles(rectangleBar,isVertical,lengthArrow,out Rectangle rectangleStart,out Rectangle rectangleEnd);
			if(rectangleStart.Contains(point)) {
				return EnumUiScrollPart.ArrowStart;
			}
			if(rectangleEnd.Contains(point)) {
				return EnumUiScrollPart.ArrowEnd;
			}
			Rectangle rectangleThumb=GetThumbHitRectangle(rectangleBar,isVertical,thumbStart,thumbEnd);
			if(rectangleThumb.Contains(point)) {
				return EnumUiScrollPart.Thumb;
			}
			int along=isVertical ? point.Y-rectangleBar.Y : point.X-rectangleBar.X;
			if(thumbEnd>thumbStart && along>=thumbEnd) {
				return EnumUiScrollPart.TrackEnd;
			}
			return EnumUiScrollPart.TrackStart;
		}

		///<summary>The state a part is drawn in: Disabled when the bar is, Pressed when it is the pressed part, Hover when it is the part under the
		///mouse and nothing is pressed (or it is pressed), Normal otherwise.</summary>
		public static EnumUiButtonState GetPartState(EnumUiScrollPart part,bool isEnabled,EnumUiScrollPart partHot,EnumUiScrollPart partPressed) {
			if(!isEnabled) {
				return EnumUiButtonState.Disabled;
			}
			if(partPressed!=EnumUiScrollPart.None) {
				return part==partPressed ? EnumUiButtonState.Pressed : EnumUiButtonState.Normal;
			}
			return part==partHot ? EnumUiButtonState.Hover : EnumUiButtonState.Normal;
		}
		#endregion Geometry

		#region Drawing
		///<summary>A whole scroll bar in rectangleBar: the track (colorTrack, empty: GetTrackColor), the two arrows with their chevrons, and the thumb
		///between thumbStart and thumbEnd (relative to the bar's start, as SCROLLBARINFO gives them; none when the end is not after the start or the
		///bar is disabled). lengthArrow is Windows' arrow length (0: no arrows, the track runs end to end).</summary>
		public static void Draw(Graphics g,Rectangle rectangleBar,bool isVertical,bool isEnabled,int lengthArrow,int thumbStart,int thumbEnd,
			EnumUiScrollPart partHot,EnumUiScrollPart partPressed,Color colorTrack)
		{
			if(rectangleBar.Width<=0 || rectangleBar.Height<=0) {
				return;
			}
			float scale=GetScale(isVertical ? rectangleBar.Width : rectangleBar.Height);
			UiControlPaint.Fill(g,colorTrack.IsEmpty ? UiTheme.GetColor(GetTrackColor()) : colorTrack,rectangleBar,UiTheme.CORNER_SQUARE,scale);
			if(lengthArrow>0) {
				GetArrowRectangles(rectangleBar,isVertical,lengthArrow,out Rectangle rectangleStart,out Rectangle rectangleEnd);
				DrawArrow(g,rectangleStart,isVertical ? UiGlyph.ChevronUp : UiGlyph.ChevronLeft,
					GetPartState(EnumUiScrollPart.ArrowStart,isEnabled,partHot,partPressed),scale);
				DrawArrow(g,rectangleEnd,isVertical ? UiGlyph.ChevronDown : UiGlyph.ChevronRight,
					GetPartState(EnumUiScrollPart.ArrowEnd,isEnabled,partHot,partPressed),scale);
			}
			if(!isEnabled) {
				return;
			}
			Rectangle rectangleThumb=GetThumbShape(GetThumbHitRectangle(rectangleBar,isVertical,thumbStart,thumbEnd),isVertical,scale);
			if(rectangleThumb.IsEmpty) {
				return;
			}
			int thickness=isVertical ? rectangleThumb.Width : rectangleThumb.Height;
			//A pill: the corner radius half the thickness (in device pixels, so the scale passed is 1).
			EnumUiColor uiColorThumb=GetThumbColor(GetPartState(EnumUiScrollPart.Thumb,isEnabled,partHot,partPressed));
			UiControlPaint.Fill(g,uiColorThumb,rectangleThumb,thickness/2,1f);
		}

		///<summary>An arrow square: its fill in the state (GetArrowFill, inset INSET_ARROW_FILL, 2 px corners) and its chevron (GetArrowColor).</summary>
		public static void DrawArrow(Graphics g,Rectangle rectangleArrow,UiGlyph uiGlyph,EnumUiButtonState buttonState,float scale) {
			if(rectangleArrow.Width<=0 || rectangleArrow.Height<=0) {
				return;
			}
			EnumUiColor? uiColorFill=GetArrowFill(buttonState);
			if(uiColorFill.HasValue) {
				int inset=UiTheme.ScaleI(INSET_ARROW_FILL,scale);
				UiControlPaint.Fill(g,uiColorFill.Value,Rectangle.Inflate(rectangleArrow,-inset,-inset),UiTheme.CORNER_CHECKBOX,scale);
			}
			UiIcons.Draw(g,uiGlyph,GetArrowGlyphRectangle(rectangleArrow,scale),GetArrowColor(buttonState));
		}
		#endregion Drawing
	}
}
