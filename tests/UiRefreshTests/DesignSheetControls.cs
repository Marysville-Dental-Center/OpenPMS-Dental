// The design sheet's control drawings (package UI-1, docs/specs/ui-refresh-plan.md section 3.6, docs/port-notes/UI-1.md): every control family
// of section 3.6 drawn from the tokens in each state the plan defines (normal, hover, pressed, focused, disabled, checked, selected, selected
// without focus), and the composites that show the families together (a window frame, the floating image windows, a toolbar, a grid, a menu,
// the module bar, tabs, a calendar, a progress bar, a tooltip). These are drawings of the design for the owner, made before any control
// changes: the control packages (UI-3 to UI-6) draw the real controls this way and show them in G-UI1 part 2. A state the plan gives no look
// of its own is left out ("-"); the glyphs are stand-ins until the Fluent icons of UI-7a.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CodeBase;

namespace UiRefreshTests {
	public enum SheetState {
		Normal,
		Hover,
		Pressed,
		Focused,
		Disabled,
		Checked,
		Selected,
		SelectedNoFocus,
	}

	///<summary>One control family of section 3.6: its name, where it is used, and how it draws a state into a cell. Draw returns null for a state
	///the plan gives no look of its own, "" when drawn, or a short caption for the cell (for example "indeterminate").</summary>
	public class SheetFamily {
		public string Name;
		public string Where;
		public Func<SheetPainter,Rectangle,SheetState,string> Draw;
	}

	public static class DesignSheetControls {
		public static readonly string[] ArrayStateNames=new string[] { "Normal","Hover","Pressed","Focused","Disabled","Checked","Selected","Selected, no focus" };
		///<summary>A matrix cell at 96 DPI.</summary>
		public const int CELL_WIDTH=136;
		public const int CELL_HEIGHT=58;

		///<summary>A rectangle of 96 DPI lengths at the painter's scale, from a device-pixel origin.</summary>
		public static Rectangle R(SheetPainter p,Point origin,int x96,int y96,int width96,int height96) {
			Rectangle rectangle=UiTheme.ScaleRectangle(new Rectangle(x96,y96,width96,height96),p.Scale);
			rectangle.Offset(origin);
			return rectangle;
		}

		public static List<SheetFamily> GetFamilies() {
			List<SheetFamily> list=new List<SheetFamily>();
			list.Add(new SheetFamily { Name="Button",Where="UI.Button, WPF Button",Draw=(p,cell,state) => DrawButton(p,cell,state,isDefault:false) });
			list.Add(new SheetFamily { Name="Default button",Where="the form's AcceptButton; WPF IsPrimary",Draw=(p,cell,state) => DrawButton(p,cell,state,isDefault:true) });
			list.Add(new SheetFamily { Name="Check box",Where="UI.CheckBox, WPF CheckBox",Draw=DrawCheckBox });
			list.Add(new SheetFamily { Name="Radio button",Where="WPF RadioButton (like the Windows one)",Draw=DrawRadioButton });
			list.Add(new SheetFamily { Name="Combo box",Where="UI.ComboBox, WPF ComboBox",Draw=DrawComboBox });
			list.Add(new SheetFamily { Name="List item",Where="UI.ListBox, WPF ListBox",Draw=(p,cell,state) => DrawListOrGridRow(p,cell,state,isGrid:false) });
			list.Add(new SheetFamily { Name="Grid row",Where="GridOD, WPF Grid",Draw=(p,cell,state) => DrawListOrGridRow(p,cell,state,isGrid:true) });
			list.Add(new SheetFamily { Name="Toolbar button",Where="ToolBarOD, WPF ToolBarButton",Draw=DrawToolbarButton });
			list.Add(new SheetFamily { Name="Menu item",Where="MenuOD, context menus, WPF Menu",Draw=DrawMenuItem });
			list.Add(new SheetFamily { Name="Module bar button",Where="ModuleBar",Draw=DrawModuleButton });
			list.Add(new SheetFamily { Name="Tab header",Where="UI.TabControl, WPF TabControl",Draw=DrawTabHeader });
			list.Add(new SheetFamily { Name="Calendar day",Where="MonthCalendarOD, date pickers",Draw=DrawCalendarDay });
			list.Add(new SheetFamily { Name="Splitter",Where="UI.SplitContainer",Draw=DrawSplitter });
			list.Add(new SheetFamily { Name="Minimise, maximise",Where="FormODBase, FormFrame title bar",Draw=(p,cell,state) => DrawCaptionButton(p,cell,state,isClose:false) });
			list.Add(new SheetFamily { Name="Close",Where="FormODBase, FormFrame title bar",Draw=(p,cell,state) => DrawCaptionButton(p,cell,state,isClose:true) });
			return list;
		}

		#region Families
		private static Rectangle WidgetInCell(SheetPainter p,Rectangle cell,int width96,int height96) {
			int width=p.S(width96);
			int height=p.S(height96);
			return new Rectangle(cell.X+p.S(10),cell.Y+(cell.Height-height)/2,width,height);
		}

		public static string DrawButton(SheetPainter p,Rectangle cell,SheetState state,bool isDefault) {
			if(state==SheetState.Checked || state==SheetState.Selected || state==SheetState.SelectedNoFocus) {
				return null;
			}
			Rectangle rectangle=WidgetInCell(p,cell,88,26);
			EnumUiColor uiColorText=EnumUiColor.Text;
			if(state==SheetState.Disabled) {
				//Section 3.2: a disabled button, the default one too, is #F5F5F5 with a Stroke border and TextDisabled.
				p.Fill(EnumUiColor.ButtonHover,rectangle,UiTheme.CORNER_CONTROL);
				p.Outline(EnumUiColor.Stroke,rectangle,p.LineWidth,UiTheme.CORNER_CONTROL);
				uiColorText=EnumUiColor.TextDisabled;
			}
			else if(isDefault) {
				EnumUiColor uiColorFill=state==SheetState.Hover ? EnumUiColor.AccentHover : (state==SheetState.Pressed ? EnumUiColor.AccentPressed : EnumUiColor.Accent);
				p.Fill(uiColorFill,rectangle,UiTheme.CORNER_CONTROL);
				uiColorText=EnumUiColor.TextOnAccent;
				if(state==SheetState.Focused) {
					p.FocusRingOnAccent(rectangle,UiTheme.CORNER_CONTROL);
				}
			}
			else {
				EnumUiColor uiColorFill=state==SheetState.Hover ? EnumUiColor.ButtonHover : (state==SheetState.Pressed ? EnumUiColor.ButtonPressed : EnumUiColor.SurfaceRaised);
				p.Fill(uiColorFill,rectangle,UiTheme.CORNER_CONTROL);
				p.Outline(EnumUiColor.Stroke,rectangle,p.LineWidth,UiTheme.CORNER_CONTROL);
				if(state==SheetState.Focused) {
					p.FocusRing(rectangle,UiTheme.CORNER_CONTROL);
				}
			}
			p.TextCenter(isDefault ? "Save" : "Cancel",p.FontBody,uiColorText,rectangle);
			return "";
		}

		public static string DrawCheckBox(SheetPainter p,Rectangle cell,SheetState state) {
			if(state==SheetState.Pressed || state==SheetState.SelectedNoFocus) {
				return null;
			}
			int size=p.S(12);
			Rectangle rectangleBox=new Rectangle(cell.X+p.S(12),cell.Y+(cell.Height-size)/2,size,size);
			Rectangle rectangleText=new Rectangle(rectangleBox.Right+p.S(4),cell.Y,p.S(70),cell.Height);
			bool isFilled=state==SheetState.Checked || state==SheetState.Selected;
			if(isFilled) {
				p.Fill(EnumUiColor.Accent,rectangleBox,UiTheme.CORNER_CHECKBOX);
				if(state==SheetState.Checked) {
					p.CheckMark(p.C(EnumUiColor.TextOnAccent),rectangleBox);
				}
				else {
					p.Dash(p.C(EnumUiColor.TextOnAccent),rectangleBox);
				}
			}
			else {
				p.Fill(EnumUiColor.SurfaceRaised,rectangleBox,UiTheme.CORNER_CHECKBOX);
				EnumUiColor uiColorBorder=state==SheetState.Hover ? EnumUiColor.StrokeStrongHover
					: (state==SheetState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.StrokeStrong);
				p.Outline(uiColorBorder,rectangleBox,p.LineWidth,UiTheme.CORNER_CHECKBOX);
			}
			p.TextLeft("Active",p.FontBody,state==SheetState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.Text,rectangleText);
			if(state==SheetState.Focused) {
				//Section 3.6: on a check box the ring goes around the box and its text.
				int widthText=p.MeasureWidth("Active",p.FontBody);
				Rectangle rectangleRing=Rectangle.FromLTRB(rectangleBox.Left-p.S(4),rectangleBox.Top-p.S(4),rectangleText.Left+widthText+p.S(4),rectangleBox.Bottom+p.S(4));
				p.FocusRing(rectangleRing,UiTheme.CORNER_CONTROL);
			}
			return state==SheetState.Selected ? "indeterminate" : "";
		}

		public static string DrawRadioButton(SheetPainter p,Rectangle cell,SheetState state) {
			if(state==SheetState.Pressed || state==SheetState.Selected || state==SheetState.SelectedNoFocus) {
				return null;
			}
			int size=p.S(12);
			Rectangle rectangleCircle=new Rectangle(cell.X+p.S(12),cell.Y+(cell.Height-size)/2,size,size);
			Rectangle rectangleText=new Rectangle(rectangleCircle.Right+p.S(4),cell.Y,p.S(70),cell.Height);
			p.G.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			if(state==SheetState.Checked) {
				using(SolidBrush brush=new SolidBrush(p.C(EnumUiColor.Accent))) {
					p.G.FillEllipse(brush,rectangleCircle);
				}
				float dot=size*0.4f;
				using(SolidBrush brush=new SolidBrush(p.C(EnumUiColor.TextOnAccent))) {
					p.G.FillEllipse(brush,rectangleCircle.X+(size-dot)/2f,rectangleCircle.Y+(size-dot)/2f,dot,dot);
				}
			}
			else {
				EnumUiColor uiColorBorder=state==SheetState.Hover ? EnumUiColor.StrokeStrongHover
					: (state==SheetState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.StrokeStrong);
				using(SolidBrush brush=new SolidBrush(p.C(EnumUiColor.SurfaceRaised))) {
					p.G.FillEllipse(brush,rectangleCircle);
				}
				using(Pen pen=new Pen(p.C(uiColorBorder),p.LineWidth)) {
					p.G.DrawEllipse(pen,UiTheme.GetStrokeRectangle(rectangleCircle,p.LineWidth));
				}
			}
			p.TextLeft("Daily",p.FontBody,state==SheetState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.Text,rectangleText);
			if(state==SheetState.Focused) {
				int widthText=p.MeasureWidth("Daily",p.FontBody);
				p.FocusRing(Rectangle.FromLTRB(rectangleCircle.Left-p.S(4),rectangleCircle.Top-p.S(4),rectangleText.Left+widthText+p.S(4),
					rectangleCircle.Bottom+p.S(4)),UiTheme.CORNER_CONTROL);
			}
			return "";
		}

		public static string DrawComboBox(SheetPainter p,Rectangle cell,SheetState state) {
			if(state==SheetState.Pressed || state==SheetState.Checked || state==SheetState.Selected || state==SheetState.SelectedNoFocus) {
				return null;
			}
			Rectangle rectangle=WidgetInCell(p,cell,112,22);
			bool isDisabled=state==SheetState.Disabled;
			p.Fill(EnumUiColor.SurfaceRaised,rectangle,UiTheme.CORNER_CONTROL);
			EnumUiColor uiColorBorder=state==SheetState.Hover ? EnumUiColor.StrokeStrongHover : (isDisabled ? EnumUiColor.Stroke : EnumUiColor.StrokeStrong);
			p.Outline(uiColorBorder,rectangle,p.LineWidth,UiTheme.CORNER_CONTROL);
			EnumUiColor uiColorText=isDisabled ? EnumUiColor.TextDisabled : EnumUiColor.Text;
			p.TextLeft("Hygienist",p.FontBody,uiColorText,new Rectangle(rectangle.X+p.S(6),rectangle.Y,rectangle.Width-p.S(24),rectangle.Height));
			int sizeChevron=p.S(UiTheme.SIZE_CHEVRON);
			p.Chevron(p.C(uiColorText),new Rectangle(rectangle.Right-p.S(6)-sizeChevron,rectangle.Y+(rectangle.Height-sizeChevron)/2,sizeChevron,sizeChevron));
			if(state==SheetState.Focused) {
				//Section 3.6: a 2 px Accent line inside the bottom edge.
				int width=p.FocusWidth;
				int inset=p.S(UiTheme.CORNER_CONTROL)/2;
				p.Line(EnumUiColor.Accent,rectangle.X+inset,rectangle.Bottom-width,rectangle.Right-inset,rectangle.Bottom-width,width);
			}
			return "";
		}

		public static string DrawListOrGridRow(SheetPainter p,Rectangle cell,SheetState state,bool isGrid) {
			if(state==SheetState.Pressed || state==SheetState.Checked) {
				return null;
			}
			int heightRow=isGrid ? p.S(UiTheme.GRID_ROW_HEIGHT) : p.S(18);
			Rectangle rectangleControl=new Rectangle(cell.X+p.S(10),cell.Y+(cell.Height-heightRow)/2-p.LineWidth,p.S(118),heightRow+2*p.LineWidth);
			p.Fill(EnumUiColor.SurfaceRaised,rectangleControl,0);
			Rectangle rectangleRow=Rectangle.Inflate(rectangleControl,-p.LineWidth,-p.LineWidth);
			EnumUiColor uiColorFill=EnumUiColor.SurfaceRaised;
			if(state==SheetState.Hover) {
				uiColorFill=EnumUiColor.RowHover;
			}
			if(state==SheetState.Selected || state==SheetState.SelectedNoFocus) {
				uiColorFill=EnumUiColor.AccentSubtle;
			}
			p.Fill(uiColorFill,rectangleRow,0);
			EnumUiColor uiColorText=state==SheetState.Disabled ? EnumUiColor.TextDisabled
				: (uiColorFill==EnumUiColor.AccentSubtle ? EnumUiColor.TextSelected : EnumUiColor.Text);
			if(isGrid) {
				int xSplit=rectangleRow.X+p.S(44);
				p.TextLeft("11/02",p.FontBody,uiColorText,new Rectangle(rectangleRow.X+p.S(4),rectangleRow.Y,p.S(40),rectangleRow.Height));
				p.TextLeft("Exam",p.FontBody,uiColorText,new Rectangle(xSplit+p.S(4),rectangleRow.Y,p.S(68),rectangleRow.Height));
				p.Line(EnumUiColor.Gridline,xSplit,rectangleRow.Y,xSplit,rectangleRow.Bottom,p.LineWidth);
				p.Line(EnumUiColor.Gridline,rectangleRow.X,rectangleRow.Bottom-p.LineWidth,rectangleRow.Right,rectangleRow.Bottom-p.LineWidth,p.LineWidth);
			}
			else {
				p.TextLeft("Recall",p.FontBody,uiColorText,new Rectangle(rectangleRow.X+p.S(6),rectangleRow.Y,rectangleRow.Width-p.S(8),rectangleRow.Height));
			}
			if(state==SheetState.Selected || state==SheetState.SelectedNoFocus) {
				//Section 3.2: the 2 px bar on the left edge, Accent with focus, StrokeStrong without.
				p.Fill(state==SheetState.Selected ? EnumUiColor.Accent : EnumUiColor.StrokeStrong,new Rectangle(rectangleRow.X,rectangleRow.Y,p.FocusWidth,
					rectangleRow.Height),0);
			}
			if(state==SheetState.Focused) {
				//Section 3.6: the focused row gets a 1 px solid Accent outline inside the row.
				p.Outline(EnumUiColor.Accent,rectangleRow,p.LineWidth,0);
			}
			p.Outline(isGrid ? EnumUiColor.Stroke : (state==SheetState.Disabled ? EnumUiColor.Stroke : EnumUiColor.StrokeStrong),rectangleControl,p.LineWidth,0);
			return "";
		}

		public static string DrawToolbarButton(SheetPainter p,Rectangle cell,SheetState state) {
			if(state==SheetState.Selected || state==SheetState.SelectedNoFocus) {
				return null;
			}
			Rectangle rectangleStrip=new Rectangle(cell.X+p.S(4),cell.Y+(cell.Height-p.S(30))/2,cell.Width-p.S(8),p.S(30));
			p.Fill(EnumUiColor.SurfaceSubtle,rectangleStrip,0);
			p.Line(EnumUiColor.Gridline,rectangleStrip.X,rectangleStrip.Bottom-p.LineWidth,rectangleStrip.Right,rectangleStrip.Bottom-p.LineWidth,p.LineWidth);
			Rectangle rectangle=new Rectangle(rectangleStrip.X+p.S(6),rectangleStrip.Y+p.S(3),p.S(96),p.S(24));
			EnumUiColor uiColorText=EnumUiColor.Text;
			switch(state) {
				case SheetState.Hover:
					p.Fill(EnumUiColor.SubtleHover,rectangle,UiTheme.CORNER_CONTROL);
					break;
				case SheetState.Pressed:
					p.Fill(EnumUiColor.SubtlePressed,rectangle,UiTheme.CORNER_CONTROL);
					break;
				case SheetState.Checked:
					p.Fill(EnumUiColor.AccentSubtle,rectangle,UiTheme.CORNER_CONTROL);
					p.Outline(EnumUiColor.Accent,rectangle,p.LineWidth,UiTheme.CORNER_CONTROL);
					uiColorText=EnumUiColor.TextSelected;
					break;
				case SheetState.Disabled:
					uiColorText=EnumUiColor.TextDisabled;
					break;
			}
			if(state==SheetState.Focused) {
				p.FocusRing(rectangle,UiTheme.CORNER_CONTROL);
			}
			p.TextLeft("Payment",p.FontBody,uiColorText,new Rectangle(rectangle.X+p.S(8),rectangle.Y,p.S(64),rectangle.Height));
			int sizeChevron=p.S(UiTheme.SIZE_CHEVRON);
			p.Chevron(p.C(uiColorText),new Rectangle(rectangle.Right-p.S(6)-sizeChevron,rectangle.Y+(rectangle.Height-sizeChevron)/2,sizeChevron,sizeChevron));
			return state==SheetState.Checked ? "toggled" : "";
		}

		public static string DrawMenuItem(SheetPainter p,Rectangle cell,SheetState state) {
			if(state==SheetState.Focused || state==SheetState.Selected || state==SheetState.SelectedNoFocus) {
				return null;
			}
			Rectangle rectangleDrop=new Rectangle(cell.X+p.S(8),cell.Y+(cell.Height-p.S(28))/2,p.S(120),p.S(28));
			p.Fill(EnumUiColor.SurfaceRaised,rectangleDrop,0);
			p.Outline(EnumUiColor.Stroke,rectangleDrop,p.LineWidth,0);
			Rectangle rectangleItem=Rectangle.Inflate(rectangleDrop,-p.S(3),-p.S(3));
			if(state==SheetState.Hover) {
				p.Fill(EnumUiColor.SubtleHover,rectangleItem,UiTheme.CORNER_MENU_ITEM);
			}
			if(state==SheetState.Pressed) {
				p.Fill(EnumUiColor.SubtlePressed,rectangleItem,UiTheme.CORNER_MENU_ITEM);
			}
			EnumUiColor uiColorText=state==SheetState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.Text;
			Rectangle rectangleIcon=new Rectangle(rectangleItem.X+p.S(4),rectangleItem.Y+(rectangleItem.Height-p.S(UiTheme.MENU_ICON_COLUMN))/2,
				p.S(UiTheme.MENU_ICON_COLUMN),p.S(UiTheme.MENU_ICON_COLUMN));
			if(state==SheetState.Checked) {
				p.CheckMark(p.C(EnumUiColor.Text),rectangleIcon);
			}
			p.TextLeft(state==SheetState.Checked ? "Show inactive" : "Print",p.FontBody,uiColorText,
				new Rectangle(rectangleIcon.Right+p.S(8),rectangleItem.Y,p.S(80),rectangleItem.Height));
			return "";
		}

		public static string DrawModuleButton(SheetPainter p,Rectangle cell,SheetState state) {
			if(state==SheetState.Disabled || state==SheetState.Checked || state==SheetState.SelectedNoFocus) {
				return null;
			}
			Rectangle rectangleBar=new Rectangle(cell.X+p.S(8),cell.Y+p.S(2),p.S(74),cell.Height-p.S(4));
			p.Fill(EnumUiColor.SurfaceSubtle,rectangleBar,0);
			p.Line(EnumUiColor.Gridline,rectangleBar.Right-p.LineWidth,rectangleBar.Y,rectangleBar.Right-p.LineWidth,rectangleBar.Bottom,p.LineWidth);
			Rectangle rectangleButton=new Rectangle(rectangleBar.X,rectangleBar.Y,rectangleBar.Width-p.LineWidth,rectangleBar.Height);
			Rectangle rectangleShape=Rectangle.Inflate(rectangleButton,-p.S(UiTheme.INSET_MODULE_HOVER),-p.S(2));
			EnumUiColor uiColorIcon=EnumUiColor.Text;
			if(state==SheetState.Hover) {
				p.Fill(EnumUiColor.SubtleHover,rectangleShape,UiTheme.CORNER_MODULE_HOVER);
			}
			if(state==SheetState.Pressed) {
				p.Fill(EnumUiColor.SubtlePressed,rectangleShape,UiTheme.CORNER_MODULE_HOVER);
			}
			if(state==SheetState.Selected) {
				p.Fill(EnumUiColor.SurfaceRaised,rectangleShape,UiTheme.CORNER_MODULE_HOVER);
				p.Fill(EnumUiColor.Accent,new Rectangle(rectangleButton.X,rectangleShape.Y,p.S(UiTheme.LINE_MODULE_MARK),rectangleShape.Height),0);
				uiColorIcon=EnumUiColor.Accent;
			}
			if(state==SheetState.Focused) {
				p.FocusRing(rectangleShape,UiTheme.CORNER_MODULE_HOVER);
			}
			//The matrix cell is shorter than a module button: a smaller stand-in icon, so it does not run into the caption.
			int sizeIcon=p.S(18);
			p.RoleIcon(4,p.C(uiColorIcon),new Rectangle(rectangleButton.X+(rectangleButton.Width-sizeIcon)/2,rectangleShape.Y+p.S(2),sizeIcon,sizeIcon));
			p.TextCenter("Account",p.FontCaption,EnumUiColor.Text,new Rectangle(rectangleButton.X,rectangleShape.Bottom-p.S(15),rectangleButton.Width,p.S(14)));
			return state==SheetState.Selected ? "the module shown" : "";
		}

		public static string DrawTabHeader(SheetPainter p,Rectangle cell,SheetState state) {
			if(state==SheetState.Pressed || state==SheetState.Checked || state==SheetState.SelectedNoFocus) {
				return null;
			}
			Rectangle rectangleStrip=new Rectangle(cell.X+p.S(6),cell.Y+(cell.Height-p.S(28))/2,cell.Width-p.S(12),p.S(28));
			p.Fill(EnumUiColor.SurfaceSubtle,rectangleStrip,0);
			p.Line(EnumUiColor.Stroke,rectangleStrip.X,rectangleStrip.Bottom-p.LineWidth,rectangleStrip.Right,rectangleStrip.Bottom-p.LineWidth,p.LineWidth);
			Rectangle rectangle=new Rectangle(rectangleStrip.X+p.S(6),rectangleStrip.Y+p.S(2),p.S(64),rectangleStrip.Height-p.S(2)-p.LineWidth);
			EnumUiColor uiColorText=EnumUiColor.TextSecondary;
			if(state==SheetState.Hover) {
				p.Fill(EnumUiColor.SubtleHover,rectangle,UiTheme.CORNER_CONTROL);
			}
			if(state==SheetState.Selected) {
				uiColorText=EnumUiColor.Text;
				p.Fill(EnumUiColor.Accent,new Rectangle(rectangle.X,rectangle.Bottom-p.S(UiTheme.LINE_TAB_SELECTED),rectangle.Width,p.S(UiTheme.LINE_TAB_SELECTED)),0);
			}
			if(state==SheetState.Disabled) {
				uiColorText=EnumUiColor.TextDisabled;
			}
			if(state==SheetState.Focused) {
				p.FocusRing(rectangle,UiTheme.CORNER_CONTROL);
			}
			p.TextCenter("General",p.FontBody,uiColorText,rectangle);
			return "";
		}

		public static string DrawCalendarDay(SheetPainter p,Rectangle cell,SheetState state) {
			if(state==SheetState.Hover || state==SheetState.Pressed || state==SheetState.SelectedNoFocus) {
				return null;
			}
			Rectangle rectangle=WidgetInCell(p,cell,28,24);
			EnumUiColor uiColorText=EnumUiColor.Text;
			if(state==SheetState.Selected) {
				p.Fill(EnumUiColor.Accent,rectangle,UiTheme.CORNER_CONTROL);
				uiColorText=EnumUiColor.TextOnAccent;
			}
			if(state==SheetState.Checked) {
				//Today: a 1 px Accent ring.
				p.Outline(EnumUiColor.Accent,rectangle,p.LineWidth,UiTheme.CORNER_CONTROL);
			}
			if(state==SheetState.Focused) {
				p.FocusRing(rectangle,UiTheme.CORNER_CONTROL);
			}
			if(state==SheetState.Disabled) {
				uiColorText=EnumUiColor.TextDisabled;
			}
			p.TextCenter("17",p.FontBody,uiColorText,rectangle);
			Rectangle rectangleSelectedFocused=new Rectangle(rectangle.Right+p.S(12),rectangle.Y,rectangle.Width,rectangle.Height);
			if(state==SheetState.Focused) {
				//The selected day with focus: the ring outside a white inner ring.
				p.Fill(EnumUiColor.Accent,rectangleSelectedFocused,UiTheme.CORNER_CONTROL);
				p.FocusRingOnAccent(rectangleSelectedFocused,UiTheme.CORNER_CONTROL);
				p.TextCenter("18",p.FontBody,EnumUiColor.TextOnAccent,rectangleSelectedFocused);
				return "day; selected day";
			}
			return state==SheetState.Checked ? "today" : "";
		}

		public static string DrawSplitter(SheetPainter p,Rectangle cell,SheetState state) {
			if(state!=SheetState.Normal && state!=SheetState.Pressed && state!=SheetState.Focused) {
				return null;
			}
			Rectangle rectangleLeft=new Rectangle(cell.X+p.S(10),cell.Y+p.S(8),p.S(48),cell.Height-p.S(16));
			Rectangle rectangleSplitter=new Rectangle(rectangleLeft.Right,rectangleLeft.Y,p.S(8),rectangleLeft.Height);
			Rectangle rectangleRight=new Rectangle(rectangleSplitter.Right,rectangleLeft.Y,p.S(48),rectangleLeft.Height);
			p.Fill(EnumUiColor.SurfaceRaised,rectangleLeft,0);
			p.Fill(EnumUiColor.SurfaceRaised,rectangleRight,0);
			p.Fill(EnumUiColor.Surface,rectangleSplitter,0);
			int x=rectangleSplitter.X+(rectangleSplitter.Width-p.LineWidth)/2;
			p.Line(state==SheetState.Pressed ? EnumUiColor.Accent : EnumUiColor.Gridline,x,rectangleSplitter.Y,x,rectangleSplitter.Bottom,p.LineWidth);
			if(state==SheetState.Focused) {
				p.FocusRing(rectangleSplitter,0);
			}
			return state==SheetState.Pressed ? "dragging" : "";
		}

		public static string DrawCaptionButton(SheetPainter p,Rectangle cell,SheetState state,bool isClose) {
			if(state!=SheetState.Normal && state!=SheetState.Hover) {
				return null;
			}
			//Two caption buttons in title bar order: minimise (the one shown) and maximise, or maximise and close (the one shown).
			Rectangle rectangleBar=new Rectangle(cell.X+p.S(8),cell.Y+(cell.Height-p.S(30))/2,p.S(92),p.S(30));
			p.Fill(EnumUiColor.TitleBar,rectangleBar,0);
			Rectangle rectangleLeft=new Rectangle(rectangleBar.X,rectangleBar.Y,p.S(46),rectangleBar.Height);
			Rectangle rectangleRight=new Rectangle(rectangleLeft.Right,rectangleBar.Y,rectangleBar.Right-rectangleLeft.Right,rectangleBar.Height);
			Rectangle rectangleButton=isClose ? rectangleRight : rectangleLeft;
			EnumUiColor uiColorGlyph=EnumUiColor.TitleBarText;
			if(state==SheetState.Hover) {
				p.Fill(isClose ? EnumUiColor.Danger : EnumUiColor.CaptionHover,rectangleButton,0);
				uiColorGlyph=EnumUiColor.TextOnAccent;
			}
			p.CaptionGlyph(isClose ? 2 : 0,p.C(uiColorGlyph),rectangleButton);
			p.CaptionGlyph(1,p.C(EnumUiColor.TitleBarText),isClose ? rectangleLeft : rectangleRight);
			return "";
		}
		#endregion Families

		#region Composites
		///<summary>A window with the program's frame (section 3.6): the title bar in TitleBar with its caption buttons and the 1 px outline, a group
		///box with a label, a combo box, a check box and a radio button, the default and a plain button, and a tooltip. Returns its size.</summary>
		public static Size DrawWindowComposite(SheetPainter p,Point origin) {
			Rectangle rectangleWindow=R(p,origin,0,0,440,236);
			p.Fill(EnumUiColor.Surface,rectangleWindow,0);
			Rectangle rectangleTitle=R(p,origin,0,0,440,30);
			p.Fill(EnumUiColor.TitleBar,rectangleTitle,0);
			p.TextLeft("Patient Edit",p.FontBody,EnumUiColor.TitleBarText,R(p,origin,10,0,240,30));
			p.CaptionGlyph(0,p.C(EnumUiColor.TitleBarText),R(p,origin,302,0,46,30));
			p.Fill(EnumUiColor.CaptionHover,R(p,origin,348,0,46,30),0);
			p.CaptionGlyph(1,p.C(EnumUiColor.TextOnAccent),R(p,origin,348,0,46,30));
			p.CaptionGlyph(2,p.C(EnumUiColor.TitleBarText),R(p,origin,394,0,46,30));
			p.Outline(EnumUiColor.TitleBarOutline,rectangleWindow,p.LineWidth,0);
			//A group box: 1 px Stroke, 6 px corners, title in Text on the panel's background.
			Rectangle rectangleGroup=R(p,origin,14,46,240,128);
			p.Outline(EnumUiColor.Stroke,rectangleGroup,p.LineWidth,UiTheme.CORNER_CONTAINER);
			Rectangle rectangleGroupTitle=R(p,origin,24,38,60,16);
			p.Fill(EnumUiColor.Surface,rectangleGroupTitle,0);
			p.TextCenter("Address",p.FontBody,EnumUiColor.Text,rectangleGroupTitle);
			p.TextLeft("City",p.FontBody,EnumUiColor.Text,R(p,origin,26,62,60,22));
			Rectangle rectangleInput=R(p,origin,90,62,150,22);
			p.Fill(EnumUiColor.SurfaceRaised,rectangleInput,0);
			p.Outline(EnumUiColor.StrokeStrong,rectangleInput,p.LineWidth,0);
			p.TextLeft("Springfield",p.FontBody,EnumUiColor.Text,R(p,origin,94,62,140,22));
			p.TextLeft("Language",p.FontBody,EnumUiColor.Text,R(p,origin,26,92,60,22));
			DrawComboBox(p,R(p,origin,80,92,170,22),SheetState.Normal);
			DrawCheckBox(p,R(p,origin,14,118,120,24),SheetState.Checked);
			DrawRadioButton(p,R(p,origin,14,142,120,24),SheetState.Checked);
			DrawRadioButton(p,R(p,origin,104,142,120,24),SheetState.Hover);
			//The tooltip: SurfaceRaised, 1 px Stroke, 4 px corners, Text.
			Rectangle rectangleTip=R(p,origin,270,62,150,24);
			p.Fill(EnumUiColor.SurfaceRaised,rectangleTip,UiTheme.CORNER_CONTROL);
			p.Outline(EnumUiColor.Stroke,rectangleTip,p.LineWidth,UiTheme.CORNER_CONTROL);
			p.TextCenter("Tooltip: the patient's city",p.FontNote,EnumUiColor.Text,rectangleTip);
			p.TextLeft("Hint text in TextSecondary",p.FontBody,EnumUiColor.TextSecondary,R(p,origin,270,96,160,20));
			p.TextLeft("An error in Danger",p.FontBody,EnumUiColor.Danger,R(p,origin,270,118,160,20));
			DrawButton(p,R(p,origin,226,190,120,40),SheetState.Normal,isDefault:true);
			DrawButton(p,R(p,origin,318,190,120,40),SheetState.Normal,isDefault:false);
			return rectangleWindow.Size;
		}

		///<summary>The floating image windows (section 3.6): the selected one in Accent with white text, the unselected one in SurfaceSubtle with Text
		///and a 1 px Stroke outline; caption hover CaptionHover on the selected one, SubtleHover on the other, close hover Danger on both.</summary>
		public static Size DrawFloatingWindows(SheetPainter p,Point origin) {
			for(int i=0;i<2;i++) {
				bool isSelected=i==0;
				int x96=i*230;
				Rectangle rectangleWindow=R(p,origin,x96,0,214,120);
				p.Fill(EnumUiColor.SurfaceRaised,rectangleWindow,0);
				Rectangle rectangleTitle=R(p,origin,x96,0,214,28);
				p.Fill(isSelected ? EnumUiColor.Accent : EnumUiColor.SurfaceSubtle,rectangleTitle,0);
				EnumUiColor uiColorText=isSelected ? EnumUiColor.TextOnAccent : EnumUiColor.Text;
				p.TextLeft(isSelected ? "Image 1 (selected)" : "Image 2",p.FontBody,uiColorText,R(p,origin,x96+8,0,120,28));
				Rectangle rectangleMax=R(p,origin,x96+128,0,43,28);
				p.Fill(isSelected ? EnumUiColor.CaptionHover : EnumUiColor.SubtleHover,rectangleMax,0);
				p.CaptionGlyph(1,p.C(uiColorText),rectangleMax);
				Rectangle rectangleClose=R(p,origin,x96+171,0,43,28);
				if(!isSelected) {
					p.Fill(EnumUiColor.Danger,rectangleClose,0);
				}
				p.CaptionGlyph(2,p.C(isSelected ? EnumUiColor.TextOnAccent : EnumUiColor.TextOnAccent),rectangleClose);
				p.Outline(isSelected ? EnumUiColor.TitleBarOutline : EnumUiColor.Stroke,rectangleWindow,p.LineWidth,0);
				p.TextCenter(isSelected ? "maximise hover" : "maximise and close hover",p.FontNote,EnumUiColor.TextSecondary,R(p,origin,x96,40,214,80));
			}
			return R(p,new Point(0,0),0,0,444,120).Size;
		}

		///<summary>A module toolbar (section 3.6): buttons at rest, on hover, pressed, toggled, a separator, the error and notify states.</summary>
		public static Size DrawToolbarComposite(SheetPainter p,Point origin) {
			Rectangle rectangleStrip=R(p,origin,0,0,760,30);
			p.Fill(EnumUiColor.SurfaceSubtle,rectangleStrip,0);
			p.Line(EnumUiColor.Gridline,rectangleStrip.X,rectangleStrip.Bottom-p.LineWidth,rectangleStrip.Right,rectangleStrip.Bottom-p.LineWidth,p.LineWidth);
			string[] arrayTexts=new string[] { "Payment","Adjustment","New Claim","|","Payment Plan","Quick Procs","Statement" };
			string[] arrayStates=new string[] { "rest","hover","pressed","","toggled","error","notify" };
			int x96=4;
			for(int i=0;i<arrayTexts.Length;i++) {
				if(arrayTexts[i]=="|") {
					int xSeparator=origin.X+p.S(x96+4);
					p.Line(EnumUiColor.Stroke,xSeparator,rectangleStrip.Y+p.S(UiTheme.INSET_TOOLBAR_SEPARATOR),xSeparator,
						rectangleStrip.Bottom-p.S(UiTheme.INSET_TOOLBAR_SEPARATOR),p.LineWidth);
					x96+=10;
					continue;
				}
				int widthText96=(int)Math.Ceiling(p.MeasureWidth(arrayTexts[i],p.FontBody)/p.Scale);
				int width96=widthText96+12+(i<3 ? UiTheme.SIZE_CHEVRON+4 : 0);
				Rectangle rectangle=R(p,origin,x96,3,width96,24);
				EnumUiColor uiColorText=EnumUiColor.Text;
				switch(arrayStates[i]) {
					case "hover":
						p.Fill(EnumUiColor.SubtleHover,rectangle,UiTheme.CORNER_CONTROL);
						break;
					case "pressed":
						p.Fill(EnumUiColor.SubtlePressed,rectangle,UiTheme.CORNER_CONTROL);
						break;
					case "toggled":
						p.Fill(EnumUiColor.AccentSubtle,rectangle,UiTheme.CORNER_CONTROL);
						p.Outline(EnumUiColor.Accent,rectangle,p.LineWidth,UiTheme.CORNER_CONTROL);
						uiColorText=EnumUiColor.TextSelected;
						break;
					case "error":
						p.Fill(EnumUiColor.DangerSubtle,rectangle,UiTheme.CORNER_CONTROL);
						uiColorText=EnumUiColor.Danger;
						break;
					case "notify":
						p.Fill(EnumUiColor.WarningSubtle,rectangle,UiTheme.CORNER_CONTROL);
						uiColorText=EnumUiColor.Warning;
						break;
				}
				p.TextLeft(arrayTexts[i],p.FontBody,uiColorText,new Rectangle(rectangle.X+p.S(6),rectangle.Y,p.S(widthText96+2),rectangle.Height));
				if(i<3) {
					int sizeChevron=p.S(UiTheme.SIZE_CHEVRON);
					p.Chevron(p.C(uiColorText),new Rectangle(rectangle.Right-p.S(4)-sizeChevron,rectangle.Y+(rectangle.Height-sizeChevron)/2,sizeChevron,sizeChevron));
				}
				p.TextCenter(arrayStates[i],p.FontNote,EnumUiColor.TextSecondary,R(p,new Point(rectangle.X,origin.Y),0,32,width96,14));
				x96+=width96+4;
			}
			return R(p,new Point(0,0),0,0,760,48).Size;
		}

		///<summary>Two grids (section 3.6): the flat Accent title with white semibold text, the SurfaceSubtle header with Stroke separators, rows on
		///SurfaceRaised with Gridline lines, hover, the selection with its bar and the focused row's outline; left with focus, right without.</summary>
		public static Size DrawGridComposite(SheetPainter p,Point origin,int heightRow96,bool isBothGrids) {
			string[] arrayHeadings=new string[] { "Date","Code","Description","Amount" };
			int[] arrayWidths=new int[] { 80,52,148,66 };
			string[][] arrayRows=new string[][] {
				new string[] { "10/14/2026","D0120","Periodic exam","45.00" },
				new string[] { "10/14/2026","D1110","Prophylaxis, adult","98.00" },
				new string[] { "10/14/2026","D0274","Bitewings, four films","62.00" },
				new string[] { "11/02/2026","D2392","Resin composite, two surfaces","185.00" },
				new string[] { "11/02/2026","","Payment","-120.00" },
				new string[] { "11/09/2026","D0220","Periapical, first film","28.00" } };
			int countGrids=isBothGrids ? 2 : 1;
			int widthGrid96=arrayWidths[0]+arrayWidths[1]+arrayWidths[2]+arrayWidths[3]+2;
			for(int g=0;g<countGrids;g++) {
				bool hasFocus=g==0;
				int x96=g*(widthGrid96+20);
				int heightTitle96=22;
				int heightHeader96=heightRow96+2;
				int height96=heightTitle96+heightHeader96+arrayRows.Length*heightRow96+2;
				Rectangle rectangleGrid=R(p,origin,x96,0,widthGrid96,height96);
				p.Fill(EnumUiColor.SurfaceRaised,rectangleGrid,0);
				Rectangle rectangleTitle=R(p,origin,x96,0,widthGrid96,heightTitle96);
				p.Fill(EnumUiColor.Accent,rectangleTitle,0);
				p.TextCenter(hasFocus ? "Account (with focus)" : "Account (without focus)",p.FontTitle,EnumUiColor.TextOnAccent,rectangleTitle);
				Rectangle rectangleHeader=R(p,origin,x96,heightTitle96,widthGrid96,heightHeader96);
				p.Fill(EnumUiColor.SurfaceSubtle,rectangleHeader,0);
				p.Line(EnumUiColor.Stroke,rectangleHeader.X,rectangleHeader.Bottom-p.LineWidth,rectangleHeader.Right,rectangleHeader.Bottom-p.LineWidth,p.LineWidth);
				int xCol96=x96+1;
				for(int c=0;c<arrayHeadings.Length;c++) {
					p.TextCenter(arrayHeadings[c],p.FontBold,EnumUiColor.Text,R(p,origin,xCol96,heightTitle96,arrayWidths[c],heightHeader96));
					xCol96+=arrayWidths[c];
					if(c<arrayHeadings.Length-1) {
						int xLine=origin.X+p.S(xCol96);
						p.Line(EnumUiColor.Stroke,xLine,rectangleHeader.Y,xLine,rectangleHeader.Bottom,p.LineWidth);
					}
				}
				for(int r=0;r<arrayRows.Length;r++) {
					int y96=heightTitle96+heightHeader96+r*heightRow96;
					Rectangle rectangleRow=R(p,origin,x96+1,y96,widthGrid96-2,heightRow96);
					bool isSelected=r==2;
					bool isHover=hasFocus && r==4;
					EnumUiColor uiColorFill=isSelected ? EnumUiColor.AccentSubtle : (isHover ? EnumUiColor.RowHover : EnumUiColor.SurfaceRaised);
					p.Fill(uiColorFill,rectangleRow,0);
					xCol96=x96+1;
					for(int c=0;c<arrayHeadings.Length;c++) {
						Rectangle rectangleCell=R(p,origin,xCol96+3,y96,arrayWidths[c]-6,heightRow96);
						TextFormatFlags textFormatFlags=TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|TextFormatFlags.EndEllipsis
							|(c==3 ? TextFormatFlags.Right : TextFormatFlags.Left);
						p.Text(arrayRows[r][c],p.FontBody,isSelected ? EnumUiColor.TextSelected : EnumUiColor.Text,rectangleCell,textFormatFlags);
						xCol96+=arrayWidths[c];
						if(c<arrayHeadings.Length-1) {
							int xLine=origin.X+p.S(xCol96);
							p.Line(EnumUiColor.Gridline,xLine,rectangleRow.Y,xLine,rectangleRow.Bottom,p.LineWidth);
						}
					}
					p.Line(EnumUiColor.Gridline,rectangleRow.X,rectangleRow.Bottom-p.LineWidth,rectangleRow.Right,rectangleRow.Bottom-p.LineWidth,p.LineWidth);
					if(isSelected) {
						p.Fill(hasFocus ? EnumUiColor.Accent : EnumUiColor.StrokeStrong,new Rectangle(rectangleRow.X,rectangleRow.Y,p.FocusWidth,rectangleRow.Height),0);
						if(hasFocus) {
							p.Outline(EnumUiColor.Accent,rectangleRow,p.LineWidth,0);
						}
					}
				}
				p.Outline(EnumUiColor.Stroke,rectangleGrid,p.LineWidth,0);
			}
			int heightTotal96=22+heightRow96+2+arrayRows.Length*heightRow96+2;
			return R(p,new Point(0,0),0,0,countGrids*(widthGrid96+20),heightTotal96).Size;
		}

		///<summary>A menu bar on Surface and a drop-down (section 3.6): SurfaceRaised with a 1 px Stroke border, hover SubtleHover with 2 px corners,
		///a Gridline separator, a checked item, a disabled one.</summary>
		public static Size DrawMenuComposite(SheetPainter p,Point origin) {
			Rectangle rectangleBar=R(p,origin,0,0,300,22);
			p.Fill(EnumUiColor.Surface,rectangleBar,0);
			string[] arrayItems=new string[] { "File","Lists","Reports","Setup" };
			int x96=4;
			foreach(string item in arrayItems) {
				int width96=(int)Math.Ceiling(p.MeasureWidth(item,p.FontBody)/p.Scale)+10;
				Rectangle rectangleItem=R(p,origin,x96,1,width96,20);
				if(item=="Reports") {
					p.Fill(EnumUiColor.SubtleHover,rectangleItem,UiTheme.CORNER_MENU_ITEM);
				}
				p.TextCenter(item,p.FontBody,EnumUiColor.Text,rectangleItem);
				if(item=="Reports") {
					Point originDrop=new Point(rectangleItem.X,rectangleItem.Bottom+p.S(1));
					Rectangle rectangleDrop=R(p,originDrop,0,0,170,106);
					p.Fill(EnumUiColor.SurfaceRaised,rectangleDrop,0);
					p.Outline(EnumUiColor.Stroke,rectangleDrop,p.LineWidth,0);
					string[] arrayDrop=new string[] { "Standard","Custom","-","Show inactive","Export (disabled)" };
					int y96=3;
					foreach(string drop in arrayDrop) {
						if(drop=="-") {
							p.Line(EnumUiColor.Gridline,rectangleDrop.X+p.S(6),originDrop.Y+p.S(y96+3),rectangleDrop.Right-p.S(6),originDrop.Y+p.S(y96+3),p.LineWidth);
							y96+=7;
							continue;
						}
						Rectangle rectangleDropItem=R(p,originDrop,3,y96,164,24);
						if(drop=="Custom") {
							p.Fill(EnumUiColor.SubtleHover,rectangleDropItem,UiTheme.CORNER_MENU_ITEM);
						}
						if(drop=="Show inactive") {
							p.CheckMark(p.C(EnumUiColor.Text),R(p,originDrop,7,y96+4,16,16));
						}
						p.TextLeft(drop,p.FontBody,drop.Contains("disabled") ? EnumUiColor.TextDisabled : EnumUiColor.Text,R(p,originDrop,31,y96,130,24));
						y96+=24;
					}
				}
				x96+=width96+2;
			}
			return R(p,new Point(0,0),0,0,300,132).Size;
		}

		///<summary>The module bar (section 3.6): SurfaceSubtle with a Gridline divider; hover SubtleHover, the selected module SurfaceRaised with the
		///3 px Accent mark and its icon in Accent.</summary>
		public static Size DrawModuleBarComposite(SheetPainter p,Point origin) {
			string[] arrayModules=new string[] { "Appts","Family","Account","Treat' Plan","Chart","Imaging" };
			for(int i=0;i<arrayModules.Length;i++) {
				Rectangle cell=R(p,origin,-8,i*58,96,58);
				SheetState state=i==2 ? SheetState.Selected : (i==3 ? SheetState.Hover : SheetState.Normal);
				Rectangle rectangleBar=new Rectangle(cell.X+p.S(8),cell.Y,p.S(74),cell.Height);
				p.Fill(EnumUiColor.SurfaceSubtle,rectangleBar,0);
				p.Line(EnumUiColor.Gridline,rectangleBar.Right-p.LineWidth,rectangleBar.Y,rectangleBar.Right-p.LineWidth,rectangleBar.Bottom,p.LineWidth);
				Rectangle rectangleButton=new Rectangle(rectangleBar.X,rectangleBar.Y,rectangleBar.Width-p.LineWidth,rectangleBar.Height);
				Rectangle rectangleShape=Rectangle.Inflate(rectangleButton,-p.S(UiTheme.INSET_MODULE_HOVER),-p.S(2));
				EnumUiColor uiColorIcon=EnumUiColor.Text;
				if(state==SheetState.Hover) {
					p.Fill(EnumUiColor.SubtleHover,rectangleShape,UiTheme.CORNER_MODULE_HOVER);
				}
				if(state==SheetState.Selected) {
					p.Fill(EnumUiColor.SurfaceRaised,rectangleShape,UiTheme.CORNER_MODULE_HOVER);
					p.Fill(EnumUiColor.Accent,new Rectangle(rectangleButton.X,rectangleShape.Y,p.S(UiTheme.LINE_MODULE_MARK),rectangleShape.Height),0);
					uiColorIcon=EnumUiColor.Accent;
				}
				int sizeIcon=p.S(24);
				p.RoleIcon(4,p.C(uiColorIcon),new Rectangle(rectangleButton.X+(rectangleButton.Width-sizeIcon)/2,rectangleShape.Y+p.S(5),sizeIcon,sizeIcon));
				p.TextCenter(arrayModules[i],p.FontCaption,EnumUiColor.Text,new Rectangle(rectangleButton.X,rectangleShape.Bottom-p.S(18),rectangleButton.Width,p.S(16)));
			}
			return R(p,new Point(0,0),0,0,74,arrayModules.Length*58).Size;
		}

		///<summary>Tabs (section 3.6): unselected TextSecondary, hover SubtleHover, the selected one Text with the 2 px Accent line; the page with a
		///1 px Stroke border on Surface.</summary>
		public static Size DrawTabsComposite(SheetPainter p,Point origin) {
			Rectangle rectangleStrip=R(p,origin,0,0,320,28);
			p.Fill(EnumUiColor.SurfaceSubtle,rectangleStrip,0);
			Rectangle rectanglePage=R(p,origin,0,27,320,80);
			p.Fill(EnumUiColor.Surface,rectanglePage,0);
			p.Outline(EnumUiColor.Stroke,rectanglePage,p.LineWidth,0);
			string[] arrayTabs=new string[] { "General","Insurance","Notes" };
			int x96=4;
			for(int i=0;i<arrayTabs.Length;i++) {
				int width96=(int)Math.Ceiling(p.MeasureWidth(arrayTabs[i],p.FontBody)/p.Scale)+8+12;
				Rectangle rectangle=R(p,origin,x96,2,width96,25);
				EnumUiColor uiColorText=EnumUiColor.TextSecondary;
				if(i==2) {
					p.Fill(EnumUiColor.SubtleHover,rectangle,UiTheme.CORNER_CONTROL);
				}
				if(i==0) {
					uiColorText=EnumUiColor.Text;
					p.Fill(EnumUiColor.Accent,new Rectangle(rectangle.X,rectangle.Bottom-p.S(UiTheme.LINE_TAB_SELECTED),rectangle.Width,p.S(UiTheme.LINE_TAB_SELECTED)),0);
				}
				p.TextCenter(arrayTabs[i],p.FontBody,uiColorText,rectangle);
				x96+=width96+2;
			}
			p.TextLeft("selected, unselected, hover",p.FontNote,EnumUiColor.TextSecondary,R(p,origin,10,40,300,20));
			return R(p,new Point(0,0),0,0,320,108).Size;
		}

		///<summary>A month (section 3.6): Fluent chevrons, today with the 1 px Accent ring, the selected day in Accent with white text; and a progress
		///bar filled with Accent.</summary>
		public static Size DrawCalendarAndProgress(SheetPainter p,Point origin) {
			Rectangle rectangleCalendar=R(p,origin,0,0,232,104);
			p.Fill(EnumUiColor.SurfaceRaised,rectangleCalendar,0);
			p.Outline(EnumUiColor.Stroke,rectangleCalendar,p.LineWidth,0);
			p.TextCenter("November 2026",p.FontBold,EnumUiColor.Text,R(p,origin,30,2,172,22));
			using(System.Drawing.Drawing2D.Matrix matrix=new System.Drawing.Drawing2D.Matrix()) {
				Rectangle rectangleLeft=R(p,origin,8,7,12,12);
				matrix.RotateAt(90,new PointF(rectangleLeft.X+rectangleLeft.Width/2f,rectangleLeft.Y+rectangleLeft.Height/2f));
				p.G.Transform=matrix;
				p.Chevron(p.C(EnumUiColor.Text),rectangleLeft);
				p.G.ResetTransform();
			}
			using(System.Drawing.Drawing2D.Matrix matrix=new System.Drawing.Drawing2D.Matrix()) {
				Rectangle rectangleRight=R(p,origin,212,7,12,12);
				matrix.RotateAt(-90,new PointF(rectangleRight.X+rectangleRight.Width/2f,rectangleRight.Y+rectangleRight.Height/2f));
				p.G.Transform=matrix;
				p.Chevron(p.C(EnumUiColor.Text),rectangleRight);
				p.G.ResetTransform();
			}
			string[] arrayDays=new string[] { "S","M","T","W","T","F","S" };
			for(int d=0;d<7;d++) {
				p.TextCenter(arrayDays[d],p.FontNote,EnumUiColor.TextSecondary,R(p,origin,4+d*32,26,32,18));
			}
			for(int week=0;week<2;week++) {
				for(int d=0;d<7;d++) {
					int day=15+week*7+d;
					Rectangle rectangleDay=R(p,origin,6+d*32,46+week*28,28,24);
					EnumUiColor uiColorText=EnumUiColor.Text;
					if(day==17) {
						p.Outline(EnumUiColor.Accent,rectangleDay,p.LineWidth,UiTheme.CORNER_CONTROL);
					}
					if(day==19) {
						p.Fill(EnumUiColor.Accent,rectangleDay,UiTheme.CORNER_CONTROL);
						uiColorText=EnumUiColor.TextOnAccent;
					}
					p.TextCenter(day.ToString(),p.FontBody,uiColorText,rectangleDay);
				}
			}
			Rectangle rectangleTrack=R(p,origin,250,40,200,14);
			p.Fill(EnumUiColor.SurfaceRaised,rectangleTrack,UiTheme.CORNER_CONTROL);
			p.Fill(EnumUiColor.Accent,R(p,origin,250,40,124,14),UiTheme.CORNER_CONTROL);
			p.Outline(EnumUiColor.Stroke,rectangleTrack,p.LineWidth,UiTheme.CORNER_CONTROL);
			p.TextLeft("Progress: Accent fill",p.FontNote,EnumUiColor.TextSecondary,R(p,origin,250,58,200,18));
			return R(p,new Point(0,0),0,0,460,104).Size;
		}
		#endregion Composites
	}
}
