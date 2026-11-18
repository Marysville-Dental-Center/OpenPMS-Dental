// The token renderer of the stock menus and tool strips (docs/specs/ui-refresh-plan.md, section 3.6, Menus; package UI-6, docs/port-notes/UI-6.md):
// one ToolStripRenderer with a colour table from the tokens restyles every stock ContextMenuStrip, MenuStrip, ToolStrip and StatusStrip of a UI
// thread at once, and the program's own MenuOD. UiTheme.InitWinForms sets it as ToolStripManager.Renderer, which Windows Forms keeps per thread
// (a [ThreadStatic] field of ToolStripManager in .NET 10's source; UI-6's tests check it), so every exe's Main and every other UI thread call it.
// Menu bar on Surface; drop-downs SurfaceRaised with a 1 px Stroke border; item hover SubtleHover with 2 px corners; separators Gridline; the
// check mark and the sub-menu arrow are Fluent glyphs; tool strips SurfaceSubtle with buttons that show only on hover, pressed or checked.
// Windows Forms, System.Drawing and the icon renderer (UiIcons) only, C# 7.3: RdlDesign compiles this file in as a linked file (section 3.6).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CodeBase {
	///<summary>The renderer of the stock menus and tool strips in the new look (section 3.6). Set per UI thread by UiTheme.InitWinForms. It draws with
	///UiTheme's per-thread brushes, so one instance must stay on the thread that made it (InitWinForms makes one per thread).</summary>
	public class UiToolStripRenderer:ToolStripProfessionalRenderer {
		public UiToolStripRenderer():base(new UiToolStripColorTable()) {
			RoundedEdges=false;
		}

		#region Strips
		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e) {
			EnumUiColor uiColor=EnumUiColor.SurfaceSubtle;
			if(e.ToolStrip is ToolStripDropDown) {
				uiColor=EnumUiColor.SurfaceRaised;
			}
			else if(e.ToolStrip is MenuStrip) {
				uiColor=EnumUiColor.Surface;
			}
			e.Graphics.FillRectangle(UiTheme.GetBrush(uiColor),e.AffectedBounds);
		}

		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) {
			int widthLine=UiTheme.GetLineWidth(GetScale(e.ToolStrip));
			Rectangle rectangle=new Rectangle(Point.Empty,e.ToolStrip.Size);
			if(e.ToolStrip is ToolStripDropDown) {
				//The drop-down's 1 px Stroke border, inside its bounds.
				SolidBrush solidBrushBorder=UiTheme.GetBrush(EnumUiColor.Stroke);
				e.Graphics.FillRectangle(solidBrushBorder,0,0,rectangle.Width,widthLine);
				e.Graphics.FillRectangle(solidBrushBorder,0,rectangle.Height-widthLine,rectangle.Width,widthLine);
				e.Graphics.FillRectangle(solidBrushBorder,0,0,widthLine,rectangle.Height);
				e.Graphics.FillRectangle(solidBrushBorder,rectangle.Width-widthLine,0,widthLine,rectangle.Height);
				return;
			}
			if(e.ToolStrip is MenuStrip) {
				return;//The menu bar has no border.
			}
			if(e.ToolStrip is StatusStrip) {
				e.Graphics.FillRectangle(UiTheme.GetBrush(EnumUiColor.Gridline),0,0,rectangle.Width,widthLine);
				return;
			}
			//A tool strip: the 1 px Gridline line at the bottom, as the program's toolbar.
			e.Graphics.FillRectangle(UiTheme.GetBrush(EnumUiColor.Gridline),0,rectangle.Height-widthLine,rectangle.Width,widthLine);
		}

		protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) {
			//No gradient column: the drop-down's SurfaceRaised background shows through.
		}
		#endregion Strips

		#region Items
		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
			ToolStripItem toolStripItem=e.Item;
			if(!toolStripItem.Enabled || (!toolStripItem.Selected && !toolStripItem.Pressed)) {
				return;
			}
			float scale=GetScale(e.ToolStrip);
			Rectangle rectangle;
			EnumUiColor uiColor=EnumUiColor.SubtleHover;
			if(toolStripItem.IsOnDropDown) {
				//Inside the drop-down, a little in from its sides.
				int inset=UiTheme.ScaleI(1,scale);
				rectangle=new Rectangle(inset,0,toolStripItem.Width-2*inset,toolStripItem.Height);
			}
			else {
				rectangle=new Rectangle(0,0,toolStripItem.Width,toolStripItem.Height);
				if(toolStripItem.Pressed) {
					uiColor=EnumUiColor.SubtlePressed;//its drop-down is open
				}
			}
			FillRounded(e.Graphics,uiColor,rectangle,UiTheme.GetCornerRadius(UiTheme.CORNER_MENU_ITEM,scale));
		}

		protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e) {
			ToolStripButton toolStripButton=e.Item as ToolStripButton;
			bool isChecked=toolStripButton!=null && toolStripButton.Checked;
			DrawButtonShape(e.Graphics,e.ToolStrip,e.Item,isChecked,e.Item.Pressed);
		}

		protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e) {
			DrawButtonShape(e.Graphics,e.ToolStrip,e.Item,false,e.Item.Pressed);
		}

		protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e) {
			ToolStripSplitButton toolStripSplitButton=e.Item as ToolStripSplitButton;
			if(toolStripSplitButton==null) {
				base.OnRenderSplitButtonBackground(e);
				return;
			}
			DrawButtonShape(e.Graphics,e.ToolStrip,e.Item,false,toolStripSplitButton.ButtonPressed || toolStripSplitButton.DropDownButtonPressed);
			Color colorArrow=UiTheme.GetColor(toolStripSplitButton.Enabled ? EnumUiColor.Text : EnumUiColor.TextDisabled);
			DrawArrow(new ToolStripArrowRenderEventArgs(e.Graphics,toolStripSplitButton,toolStripSplitButton.DropDownButtonBounds,colorArrow,ArrowDirection.Down));
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
			if(e.TextDirection!=ToolStripTextDirection.Horizontal) {
				base.OnRenderItemText(e);
				return;
			}
			Color color=e.TextColor;
			if(!e.Item.Enabled) {
				color=UiTheme.GetColor(EnumUiColor.TextDisabled);
			}
			else if(IsDefaultTextColor(color)) {
				color=UiTheme.GetColor(EnumUiColor.Text);
			}
			TextRenderer.DrawText(e.Graphics,e.Text,e.TextFont,e.TextRectangle,color,e.TextFormat);
		}

		protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e) {
			float scale=GetScale(e.ToolStrip);
			Rectangle rectangleImage=e.ImageRectangle;
			if(e.Item.Image!=null) {
				//An item with its own picture shows Checked as a toggled button around the picture.
				Rectangle rectangleBox=Rectangle.Inflate(rectangleImage,UiTheme.ScaleI(2,scale),UiTheme.ScaleI(2,scale));
				int radiusBox=UiTheme.GetCornerRadius(UiTheme.CORNER_MENU_ITEM,scale);
				FillRounded(e.Graphics,EnumUiColor.AccentSubtle,rectangleBox,radiusBox);
				OutlineRounded(e.Graphics,EnumUiColor.Accent,rectangleBox,radiusBox,UiTheme.GetLineWidth(scale));
				return;
			}
			int size=UiTheme.ScaleI(UiTheme.MENU_ICON_COLUMN,scale);
			Rectangle rectangleGlyph=new Rectangle(rectangleImage.X+(rectangleImage.Width-size)/2,rectangleImage.Y+(rectangleImage.Height-size)/2,size,size);
			UiIcons.Draw(e.Graphics,UiGlyph.Checkmark,rectangleGlyph,UiTheme.GetColor(e.Item.Enabled ? EnumUiColor.Text : EnumUiColor.TextDisabled));
		}

		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) {
			UiGlyph uiGlyph=UiGlyph.ChevronDown;
			switch(e.Direction) {
				case ArrowDirection.Right:
					uiGlyph=UiGlyph.ChevronRight;
					break;
				case ArrowDirection.Left:
					uiGlyph=UiGlyph.ChevronLeft;
					break;
				case ArrowDirection.Up:
					uiGlyph=UiGlyph.ChevronUp;
					break;
			}
			float scale=GetScale(e.Item?.Owner);
			int size=UiTheme.ScaleI(UiTheme.SIZE_CHEVRON,scale);
			Rectangle rectangleArrow=e.ArrowRectangle;
			Rectangle rectangleGlyph=new Rectangle(rectangleArrow.X+(rectangleArrow.Width-size)/2,rectangleArrow.Y+(rectangleArrow.Height-size)/2,size,size);
			bool isEnabled=e.Item==null || e.Item.Enabled;
			UiIcons.Draw(e.Graphics,uiGlyph,rectangleGlyph,UiTheme.GetColor(isEnabled ? EnumUiColor.Text : EnumUiColor.TextDisabled));
		}

		protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e) {
			float scale=GetScale(e.ToolStrip);
			int widthLine=UiTheme.GetLineWidth(scale);
			Size size=e.Item.Size;
			if(e.Vertical) {
				//A tool strip's separator: 1 px Stroke, 4 px in from the top and the bottom.
				int inset=UiTheme.ScaleI(UiTheme.INSET_TOOLBAR_SEPARATOR,scale);
				e.Graphics.FillRectangle(UiTheme.GetBrush(EnumUiColor.Stroke),(size.Width-widthLine)/2,inset,widthLine,Math.Max(0,size.Height-2*inset));
				return;
			}
			//A menu's separator: 1 px Gridline across the drop-down, a little in from its sides.
			int insetSide=UiTheme.ScaleI(UiTheme.SPACE_4,scale);
			e.Graphics.FillRectangle(UiTheme.GetBrush(EnumUiColor.Gridline),insetSide,(size.Height-widthLine)/2,Math.Max(0,size.Width-2*insetSide),widthLine);
		}
		#endregion Items

		#region Helpers
		///<summary>The strip's scale against 96 DPI (its monitor's DPI).</summary>
		private static float GetScale(ToolStrip toolStrip) {
			if(toolStrip==null) {
				return 1f;
			}
			return toolStrip.DeviceDpi/96f;
		}

		///<summary>True for the colours a stock item draws its text in when the program set none (HighlightText for a selected item, which the
		///professional look puts on a dark selection): then the item takes the Text token, as the hover shape is light.</summary>
		private static bool IsDefaultTextColor(Color color) {
			int argb=color.ToArgb();
			return argb==SystemColors.ControlText.ToArgb() || argb==SystemColors.MenuText.ToArgb() || argb==SystemColors.WindowText.ToArgb()
				|| argb==SystemColors.HighlightText.ToArgb() || argb==Color.Black.ToArgb();
		}

		///<summary>A tool strip button's shape (section 3.6, Toolbars): nothing at rest, SubtleHover on hover, SubtlePressed pressed, AccentSubtle with
		///a 1 px Accent border checked; 4 px corners, 1 px in from the item's edges.</summary>
		private static void DrawButtonShape(Graphics g,ToolStrip toolStrip,ToolStripItem toolStripItem,bool isChecked,bool isPressed) {
			if(!toolStripItem.Enabled && !isChecked) {
				return;
			}
			float scale=GetScale(toolStrip);
			int radius=UiTheme.GetCornerRadius(UiTheme.CORNER_CONTROL,scale);
			Rectangle rectangle=new Rectangle(0,0,toolStripItem.Width,toolStripItem.Height);
			rectangle.Inflate(-UiTheme.ScaleI(1,scale),-UiTheme.ScaleI(1,scale));
			if(isChecked) {
				FillRounded(g,EnumUiColor.AccentSubtle,rectangle,radius);
				OutlineRounded(g,EnumUiColor.Accent,rectangle,radius,UiTheme.GetLineWidth(scale));
				return;
			}
			if(isPressed) {
				FillRounded(g,EnumUiColor.SubtlePressed,rectangle,radius);
				return;
			}
			if(toolStripItem.Selected) {
				FillRounded(g,EnumUiColor.SubtleHover,rectangle,radius);
			}
		}

		///<summary>Fills the rectangle with the token's brush, with rounded corners of the radius in device pixels (0: square), antialiased on whole
		///pixels (section 3.3).</summary>
		public static void FillRounded(Graphics g,EnumUiColor uiColor,Rectangle rectangle,int radius) {
			if(rectangle.Width<=0 || rectangle.Height<=0) {
				return;
			}
			if(radius<=0) {
				g.FillRectangle(UiTheme.GetBrush(uiColor),rectangle);
				return;
			}
			GraphicsState graphicsState=g.Save();
			g.SmoothingMode=SmoothingMode.AntiAlias;
			g.PixelOffsetMode=PixelOffsetMode.Half;
			using(GraphicsPath graphicsPath=UiTheme.GetRoundedRectanglePath(rectangle,radius)) {
				g.FillPath(UiTheme.GetBrush(uiColor),graphicsPath);
			}
			g.Restore(graphicsState);
		}

		///<summary>Outlines the rectangle inside its bounds with a line of the width, rounded corners of the radius (section 3.3).</summary>
		public static void OutlineRounded(Graphics g,EnumUiColor uiColor,Rectangle rectangle,int radius,int widthLine) {
			if(rectangle.Width<=0 || rectangle.Height<=0) {
				return;
			}
			GraphicsState graphicsState=g.Save();
			g.SmoothingMode=radius>0 ? SmoothingMode.AntiAlias : SmoothingMode.None;
			g.PixelOffsetMode=PixelOffsetMode.Half;
			using(GraphicsPath graphicsPath=UiTheme.GetRoundedRectanglePath(UiTheme.GetStrokeRectangle(rectangle,widthLine),Math.Max(0,radius-widthLine/2f))) {
				g.DrawPath(UiTheme.GetPen(uiColor,widthLine),graphicsPath);
			}
			g.Restore(graphicsState);
		}
		#endregion Helpers
	}

	///<summary>The token colours for what ToolStripProfessionalRenderer still draws itself (grips, overflow buttons, panels): flat, no gradients.</summary>
	public class UiToolStripColorTable:ProfessionalColorTable {
		public UiToolStripColorTable() {
			UseSystemColors=false;
		}

		private static Color C(EnumUiColor uiColor) {
			return UiTheme.GetColor(uiColor);
		}

		public override Color ButtonCheckedGradientBegin { get { return C(EnumUiColor.AccentSubtle); } }
		public override Color ButtonCheckedGradientMiddle { get { return C(EnumUiColor.AccentSubtle); } }
		public override Color ButtonCheckedGradientEnd { get { return C(EnumUiColor.AccentSubtle); } }
		public override Color ButtonCheckedHighlight { get { return C(EnumUiColor.AccentSubtle); } }
		public override Color ButtonCheckedHighlightBorder { get { return C(EnumUiColor.Accent); } }
		public override Color ButtonPressedBorder { get { return C(EnumUiColor.SubtlePressed); } }
		public override Color ButtonPressedGradientBegin { get { return C(EnumUiColor.SubtlePressed); } }
		public override Color ButtonPressedGradientMiddle { get { return C(EnumUiColor.SubtlePressed); } }
		public override Color ButtonPressedGradientEnd { get { return C(EnumUiColor.SubtlePressed); } }
		public override Color ButtonPressedHighlight { get { return C(EnumUiColor.SubtlePressed); } }
		public override Color ButtonPressedHighlightBorder { get { return C(EnumUiColor.SubtlePressed); } }
		public override Color ButtonSelectedBorder { get { return C(EnumUiColor.SubtleHover); } }
		public override Color ButtonSelectedGradientBegin { get { return C(EnumUiColor.SubtleHover); } }
		public override Color ButtonSelectedGradientMiddle { get { return C(EnumUiColor.SubtleHover); } }
		public override Color ButtonSelectedGradientEnd { get { return C(EnumUiColor.SubtleHover); } }
		public override Color ButtonSelectedHighlight { get { return C(EnumUiColor.SubtleHover); } }
		public override Color ButtonSelectedHighlightBorder { get { return C(EnumUiColor.SubtleHover); } }
		public override Color CheckBackground { get { return C(EnumUiColor.AccentSubtle); } }
		public override Color CheckPressedBackground { get { return C(EnumUiColor.AccentSubtle); } }
		public override Color CheckSelectedBackground { get { return C(EnumUiColor.AccentSubtle); } }
		public override Color GripDark { get { return C(EnumUiColor.Stroke); } }
		public override Color GripLight { get { return C(EnumUiColor.SurfaceRaised); } }
		public override Color ImageMarginGradientBegin { get { return C(EnumUiColor.SurfaceRaised); } }
		public override Color ImageMarginGradientMiddle { get { return C(EnumUiColor.SurfaceRaised); } }
		public override Color ImageMarginGradientEnd { get { return C(EnumUiColor.SurfaceRaised); } }
		public override Color ImageMarginRevealedGradientBegin { get { return C(EnumUiColor.SurfaceRaised); } }
		public override Color ImageMarginRevealedGradientMiddle { get { return C(EnumUiColor.SurfaceRaised); } }
		public override Color ImageMarginRevealedGradientEnd { get { return C(EnumUiColor.SurfaceRaised); } }
		public override Color MenuBorder { get { return C(EnumUiColor.Stroke); } }
		public override Color MenuItemBorder { get { return C(EnumUiColor.SubtleHover); } }
		public override Color MenuItemPressedGradientBegin { get { return C(EnumUiColor.SubtlePressed); } }
		public override Color MenuItemPressedGradientMiddle { get { return C(EnumUiColor.SubtlePressed); } }
		public override Color MenuItemPressedGradientEnd { get { return C(EnumUiColor.SubtlePressed); } }
		public override Color MenuItemSelected { get { return C(EnumUiColor.SubtleHover); } }
		public override Color MenuItemSelectedGradientBegin { get { return C(EnumUiColor.SubtleHover); } }
		public override Color MenuItemSelectedGradientEnd { get { return C(EnumUiColor.SubtleHover); } }
		public override Color MenuStripGradientBegin { get { return C(EnumUiColor.Surface); } }
		public override Color MenuStripGradientEnd { get { return C(EnumUiColor.Surface); } }
		public override Color OverflowButtonGradientBegin { get { return C(EnumUiColor.SurfaceSubtle); } }
		public override Color OverflowButtonGradientMiddle { get { return C(EnumUiColor.SurfaceSubtle); } }
		public override Color OverflowButtonGradientEnd { get { return C(EnumUiColor.SurfaceSubtle); } }
		public override Color RaftingContainerGradientBegin { get { return C(EnumUiColor.Surface); } }
		public override Color RaftingContainerGradientEnd { get { return C(EnumUiColor.Surface); } }
		public override Color SeparatorDark { get { return C(EnumUiColor.Gridline); } }
		public override Color SeparatorLight { get { return C(EnumUiColor.SurfaceRaised); } }
		public override Color StatusStripGradientBegin { get { return C(EnumUiColor.SurfaceSubtle); } }
		public override Color StatusStripGradientEnd { get { return C(EnumUiColor.SurfaceSubtle); } }
		public override Color ToolStripBorder { get { return C(EnumUiColor.Gridline); } }
		public override Color ToolStripContentPanelGradientBegin { get { return C(EnumUiColor.Surface); } }
		public override Color ToolStripContentPanelGradientEnd { get { return C(EnumUiColor.Surface); } }
		public override Color ToolStripDropDownBackground { get { return C(EnumUiColor.SurfaceRaised); } }
		public override Color ToolStripGradientBegin { get { return C(EnumUiColor.SurfaceSubtle); } }
		public override Color ToolStripGradientMiddle { get { return C(EnumUiColor.SurfaceSubtle); } }
		public override Color ToolStripGradientEnd { get { return C(EnumUiColor.SurfaceSubtle); } }
		public override Color ToolStripPanelGradientBegin { get { return C(EnumUiColor.Surface); } }
		public override Color ToolStripPanelGradientEnd { get { return C(EnumUiColor.Surface); } }
	}
}
