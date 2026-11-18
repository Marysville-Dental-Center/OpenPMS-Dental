// The new look's pieces for the WPF text boxes, date pickers and calendars (docs/specs/ui-refresh-plan.md, sections 3.2, 3.3 and 3.6; package
// UI-4b-3, docs/port-notes/UI-4b.md): the keys of the text box templates in Themes/Generic.xaml (SurfaceRaised, a 1 px StrokeStrong border,
// StrokeStrongHover on hover, Stroke when read only or disabled, Danger with a validation error, the 2 px Accent focus line inside the bottom edge;
// the Windows template's layout, so no text moves), the attached IsError the templates show, the token brushes they take
// ({x:Static local:UiLookText.BrushDanger}), the Fluent calendar glyph of the date picker and the chevrons of the calendar. Office content
// (ContentLook.IsPinned, the eForm fill surface) keeps the Windows template: ApplyTemplateLook takes the new templates off again.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CodeBase;

namespace WpfControls.UI {
	///<summary>The templates, brushes and glyphs of the new look for the WPF text boxes, date pickers and calendars.</summary>
	public static class UiLookText {
		///<summary>The corner radius of a text box (DIP). The approved design sheet draws the text box square (its window drawing, G-UI1 part 1), as the
		///Windows text boxes of the WinForms windows are; the combo boxes and date pickers have 4 px corners. Section 3.3's 4 px is one change here.</summary>
		public const double CORNER_TEXT=UiTheme.CORNER_SQUARE;
		///<summary>The height of the focus line inside the bottom edge (DIP, section 3.6).</summary>
		public const double HEIGHT_FOCUS_LINE=UiTheme.LINE_COMBO_FOCUS;
		///<summary>The key of the template of a TextBox or RichTextBox (TextBoxBase) in the new look (Themes/Generic.xaml).</summary>
		public static readonly ComponentResourceKey KeyTemplateTextBox=new ComponentResourceKey(typeof(UiLookText),"TemplateTextBox");
		///<summary>The key of the template of a PasswordBox in the new look.</summary>
		public static readonly ComponentResourceKey KeyTemplatePasswordBox=new ComponentResourceKey(typeof(UiLookText),"TemplatePasswordBox");
		///<summary>The key of the template of the text box inside the date picker: no border of its own (the picker draws the box), the Windows layout.</summary>
		public static readonly ComponentResourceKey KeyTemplateTextBoxInner=new ComponentResourceKey(typeof(UiLookText),"TemplateTextBoxInner");

		///<summary>Set on a text box whose control shows a validation error: its border turns Danger.</summary>
		public static readonly DependencyProperty IsErrorProperty=DependencyProperty.RegisterAttached("IsError",typeof(bool),typeof(UiLookText),
			new FrameworkPropertyMetadata(false));

		public static bool GetIsError(DependencyObject dependencyObject) {
			return (bool)dependencyObject.GetValue(IsErrorProperty);
		}

		public static void SetIsError(DependencyObject dependencyObject,bool value) {
			dependencyObject.SetValue(IsErrorProperty,value);
		}

		public static SolidColorBrush BrushAccent => UiTheme.GetBrushWpf(EnumUiColor.Accent);
		public static SolidColorBrush BrushDanger => UiTheme.GetBrushWpf(EnumUiColor.Danger);
		public static SolidColorBrush BrushStroke => UiTheme.GetBrushWpf(EnumUiColor.Stroke);
		public static SolidColorBrush BrushStrokeStrong => UiTheme.GetBrushWpf(EnumUiColor.StrokeStrong);
		public static SolidColorBrush BrushStrokeStrongHover => UiTheme.GetBrushWpf(EnumUiColor.StrokeStrongHover);
		public static SolidColorBrush BrushTextDisabled => UiTheme.GetBrushWpf(EnumUiColor.TextDisabled);
		public static SolidColorBrush BrushText => UiTheme.GetBrushWpf(EnumUiColor.Text);
		public static CornerRadius CornerRadiusText => new CornerRadius(CORNER_TEXT);
		public static double HeightFocusLine => HEIGHT_FOCUS_LINE;
		///<summary>The focus line's inset at each end: half the corner radius, so it stays inside the corners (none on a square box).</summary>
		public static Thickness MarginFocusLine => new Thickness(CORNER_TEXT/2,0,CORNER_TEXT/2,0);
		///<summary>The Fluent left and right chevrons for a 12 DIP slot (the calendar's month and year arrows).</summary>
		public static Geometry GeometryChevronLeft => UiIcons.GetGeometryWpf(UiGlyph.ChevronLeft,12);
		public static Geometry GeometryChevronRight => UiIcons.GetGeometryWpf(UiGlyph.ChevronRight,12);

		///<summary>The Fluent calendar glyph (calendar_ltr, the icon of EnumIcons.Appt32) for a square of sizeSlot DIP: the date picker's glyph.</summary>
		public static Geometry GetGeometryCalendar(double sizeSlot) {
			IconDesign iconDesign=UiCalendarPaint.GetCalendarDesign((int)sizeSlot);
			if(iconDesign==null) {
				return Geometry.Empty;
			}
			return UiIcons.GetGeometryWpf(iconDesign,sizeSlot);
		}

		///<summary>Gives a text box (TextBox, RichTextBox, PasswordBox) the new look's template and the StrokeStrong border, or, in office content
		///(isPinned), takes them off so the Windows template and border of today show. The border brush is left alone when keepBorderBrush (a control
		///that hides the text box's border itself, the underlined eForm date).</summary>
		public static void ApplyTemplateLook(Control control,bool isPinned,bool keepBorderBrush=false) {
			if(isPinned) {
				control.ClearValue(Control.TemplateProperty);
				if(!keepBorderBrush) {
					control.ClearValue(Control.BorderBrushProperty);
				}
				return;
			}
			control.SetResourceReference(Control.TemplateProperty,control is PasswordBox ? KeyTemplatePasswordBox : KeyTemplateTextBox);
			if(!keepBorderBrush) {
				control.BorderBrush=BrushStrokeStrong;
			}
		}
	}
}
