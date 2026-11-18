// The new look's shared pieces for the WPF controls (docs/specs/ui-refresh-plan.md, sections 3.2 and 3.6; package UI-4a, docs/port-notes/UI-4a.md):
// the token brushes for XAML ({x:Static local:UiLook.BrushAccent}), the keys of the shared focus visual styles in Themes/Generic.xaml, and
// ContentLook.IsPinned, which office content (the eForm fill surface, CtrlEFormFill) sets so the controls inside it keep today's look (section 1:
// eForms are content, like sheets).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Windows;
using System.Windows.Media;
using CodeBase;

namespace WpfControls.UI {
	///<summary>The token brushes and focus styles of the new look, for the WPF controls and their XAML.</summary>
	public static class UiLook {
		///<summary>The key of the focus visual style of a plain control (Themes/Generic.xaml): a 2 px Accent ring inside the bounds, 4 px corners.</summary>
		public static readonly ComponentResourceKey KeyFocusVisualStyle=new ComponentResourceKey(typeof(UiLook),"FocusVisualStyle");
		///<summary>The key of the focus visual style of an accent-filled control (the default button): the Accent ring outside a 1 px white inner
		///ring.</summary>
		public static readonly ComponentResourceKey KeyFocusVisualStyleOnAccent=new ComponentResourceKey(typeof(UiLook),"FocusVisualStyleOnAccent");

		public static SolidColorBrush BrushAccent => UiTheme.GetBrushWpf(EnumUiColor.Accent);
		public static SolidColorBrush BrushStroke => UiTheme.GetBrushWpf(EnumUiColor.Stroke);
		public static SolidColorBrush BrushText => UiTheme.GetBrushWpf(EnumUiColor.Text);
		public static SolidColorBrush BrushTextOnAccent => UiTheme.GetBrushWpf(EnumUiColor.TextOnAccent);
		public static SolidColorBrush BrushSurfaceRaised => UiTheme.GetBrushWpf(EnumUiColor.SurfaceRaised);

		///<summary>The token's brush (frozen; its SystemColors partner in high contrast).</summary>
		public static SolidColorBrush Brush(EnumUiColor uiColor) {
			return UiTheme.GetBrushWpf(uiColor);
		}

		///<summary>Gives the element the shared focus visual style (onAccent: the one for an accent fill) from Themes/Generic.xaml. WPF shows it only
		///while the element has keyboard focus and Windows asks for keyboard cues; a mouse click draws no ring.</summary>
		public static void SetFocusVisualStyle(FrameworkElement frameworkElement,bool onAccent) {
			frameworkElement.SetResourceReference(FrameworkElement.FocusVisualStyleProperty,onAccent ? KeyFocusVisualStyleOnAccent : KeyFocusVisualStyle);
		}
	}

	///<summary>Implemented by the controls that keep a pinned copy of today's look for office content.</summary>
	internal interface IContentLook {
		///<summary>Draws the control in the new look, or in today's look when ContentLook.GetIsPinned is true.</summary>
		void ApplyLook();
	}

	///<summary>Office content (the eForm fill surface) sets IsPinned on its root: the value is inherited, so every Button, CheckBox, RadioButton and
	///Label inside it draws today's look (the pinned copies of the looks UI-4a changed), and so do controls added later.</summary>
	public static class ContentLook {
		public static readonly DependencyProperty IsPinnedProperty=DependencyProperty.RegisterAttached("IsPinned",typeof(bool),typeof(ContentLook),
			new FrameworkPropertyMetadata(false,FrameworkPropertyMetadataOptions.Inherits,OnIsPinnedChanged));

		public static bool GetIsPinned(DependencyObject dependencyObject) {
			return (bool)dependencyObject.GetValue(IsPinnedProperty);
		}

		public static void SetIsPinned(DependencyObject dependencyObject,bool value) {
			dependencyObject.SetValue(IsPinnedProperty,value);
		}

		private static void OnIsPinnedChanged(DependencyObject dependencyObject,DependencyPropertyChangedEventArgs e) {
			if(dependencyObject is IContentLook contentLook) {
				contentLook.ApplyLook();
			}
		}
	}
}
