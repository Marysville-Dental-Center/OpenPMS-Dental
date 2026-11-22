// UI-13 (docs/port-notes/UI-13.md; the owner, 2026-11-22: "Overall make sure you modernize all element types"): the WPF scroll bar in the same
// Fluent look as the Windows Forms ones (CodeBase/UI/UiScrollPaint): a flat Surface track, a rounded thumb in StrokeStrong (TextSecondary under
// the mouse, Text while dragged) inset 5 px across and 2 px along, small Fluent chevrons in StrokeStrong in the arrow squares (a SubtleHover or
// SubtlePressed square behind them), no thumb and TextDisabled chevrons when disabled. WPF's ScrollBar keeps its behaviour: the template has
// the same parts (PART_Track with its page buttons and thumb, the line buttons with WPF's own commands). The style is implicit (no key), in a
// dictionary that the WPF windows (FrmODBase) and the WPF controls that scroll (the grid, the lists, the tree, the rich text box, the combo
// box's drop-down window) merge into their resources, so every scroll bar under them, the ones inside ScrollViewer templates too, takes it.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Windows;
using System.Windows.Markup;
using CodeBase;

namespace WpfControls.UI {
	///<summary>The implicit WPF ScrollBar style of the Fluent look, as a resource dictionary to merge.</summary>
	public static class UiLookScroll {
		///<summary>The resource keys of the chevron geometries the template uses (added beside the style).</summary>
		public const string KEY_CHEVRON_UP="UiLookScrollChevronUp";
		public const string KEY_CHEVRON_DOWN="UiLookScrollChevronDown";
		public const string KEY_CHEVRON_LEFT="UiLookScrollChevronLeft";
		public const string KEY_CHEVRON_RIGHT="UiLookScrollChevronRight";
		[ThreadStatic]
		private static ResourceDictionary _resourceDictionary;

		///<summary>The dictionary (one per thread: WPF objects belong to their thread): the implicit ScrollBar style and the chevrons. Null when it
		///cannot be made (the scroll bars then keep WPF's look).</summary>
		public static ResourceDictionary GetDictionary() {
			if(_resourceDictionary!=null) {
				return _resourceDictionary;
			}
			try {
				ResourceDictionary resourceDictionary=(ResourceDictionary)XamlReader.Parse(GetXaml());
				resourceDictionary[KEY_CHEVRON_UP]=UiIcons.GetGeometryWpf(UiGlyph.ChevronUp,12);
				resourceDictionary[KEY_CHEVRON_DOWN]=UiIcons.GetGeometryWpf(UiGlyph.ChevronDown,12);
				resourceDictionary[KEY_CHEVRON_LEFT]=UiIcons.GetGeometryWpf(UiGlyph.ChevronLeft,12);
				resourceDictionary[KEY_CHEVRON_RIGHT]=UiIcons.GetGeometryWpf(UiGlyph.ChevronRight,12);
				_resourceDictionary=resourceDictionary;
			}
			catch(Exception) {
				return null;
			}
			return _resourceDictionary;
		}

		///<summary>Merges the dictionary into an element's resources (once), so the scroll bars under it take the look.</summary>
		public static void AddTo(FrameworkElement frameworkElement) {
			ResourceDictionary resourceDictionary=GetDictionary();
			if(frameworkElement==null || resourceDictionary==null || frameworkElement.Resources.MergedDictionaries.Contains(resourceDictionary)) {
				return;
			}
			frameworkElement.Resources.MergedDictionaries.Add(resourceDictionary);
		}

		///<summary>The dictionary's XAML, the token colours written in (UiTheme.GetHex).</summary>
		public static string GetXaml() {
			string track=UiTheme.GetHex(UiScrollPaint.GetTrackColor());
			string thumb=UiTheme.GetHex(UiScrollPaint.GetThumbColor(EnumUiButtonState.Normal));
			string thumbHover=UiTheme.GetHex(UiScrollPaint.GetThumbColor(EnumUiButtonState.Hover));
			string thumbPressed=UiTheme.GetHex(UiScrollPaint.GetThumbColor(EnumUiButtonState.Pressed));
			string arrow=UiTheme.GetHex(UiScrollPaint.GetArrowColor(EnumUiButtonState.Normal));
			string arrowHover=UiTheme.GetHex(UiScrollPaint.GetArrowColor(EnumUiButtonState.Hover));
			string arrowPressed=UiTheme.GetHex(UiScrollPaint.GetArrowColor(EnumUiButtonState.Pressed));
			string arrowDisabled=UiTheme.GetHex(UiScrollPaint.GetArrowColor(EnumUiButtonState.Disabled));
			string fillHover=UiTheme.GetHex(UiScrollPaint.GetArrowFill(EnumUiButtonState.Hover).Value);
			string fillPressed=UiTheme.GetHex(UiScrollPaint.GetArrowFill(EnumUiButtonState.Pressed).Value);
			int across=UiScrollPaint.INSET_THUMB_ACROSS;
			int along=UiScrollPaint.INSET_THUMB_ALONG;
			int glyph=UiScrollPaint.SIZE_ARROW_GLYPH;
			int insetFill=UiScrollPaint.INSET_ARROW_FILL;
			double radius=(UiScrollPaint.THICKNESS_BAR-2*across)/2d;
			string arrowStyle(string key,string keyGeometry) {
				return "<Style x:Key='"+key+"' TargetType='RepeatButton'>"
					+"<Setter Property='Focusable' Value='False'/><Setter Property='IsTabStop' Value='False'/>"
					+"<Setter Property='Template'><Setter.Value><ControlTemplate TargetType='RepeatButton'>"
					+"<Grid Background='Transparent'><Border x:Name='fill' Margin='"+insetFill+"' CornerRadius='2' Background='Transparent'/>"
					+"<Path x:Name='glyph' Width='"+glyph+"' Height='"+glyph+"' Stretch='Uniform' Fill='"+arrow+"' Data='{DynamicResource "+keyGeometry+"}'"
					+" HorizontalAlignment='Center' VerticalAlignment='Center'/></Grid>"
					+"<ControlTemplate.Triggers>"
					+"<Trigger Property='IsMouseOver' Value='True'><Setter TargetName='fill' Property='Background' Value='"+fillHover+"'/>"
					+"<Setter TargetName='glyph' Property='Fill' Value='"+arrowHover+"'/></Trigger>"
					+"<Trigger Property='IsPressed' Value='True'><Setter TargetName='fill' Property='Background' Value='"+fillPressed+"'/>"
					+"<Setter TargetName='glyph' Property='Fill' Value='"+arrowPressed+"'/></Trigger>"
					+"<Trigger Property='IsEnabled' Value='False'><Setter TargetName='glyph' Property='Fill' Value='"+arrowDisabled+"'/></Trigger>"
					+"</ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>";
			}
			string thumbStyle(string key,string margin) {
				return "<Style x:Key='"+key+"' TargetType='Thumb'>"
					+"<Setter Property='Focusable' Value='False'/><Setter Property='IsTabStop' Value='False'/>"
					+"<Setter Property='Template'><Setter.Value><ControlTemplate TargetType='Thumb'>"
					+"<Border x:Name='pill' Margin='"+margin+"' CornerRadius='"+radius.ToString(System.Globalization.CultureInfo.InvariantCulture)+"' Background='"+thumb+"'/>"
					+"<ControlTemplate.Triggers>"
					+"<Trigger Property='IsMouseOver' Value='True'><Setter TargetName='pill' Property='Background' Value='"+thumbHover+"'/></Trigger>"
					+"<Trigger Property='IsDragging' Value='True'><Setter TargetName='pill' Property='Background' Value='"+thumbPressed+"'/></Trigger>"
					+"</ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>";
			}
			string pageStyle="<Style x:Key='UiLookScrollPage' TargetType='RepeatButton'>"
				+"<Setter Property='Focusable' Value='False'/><Setter Property='IsTabStop' Value='False'/>"
				+"<Setter Property='Template'><Setter.Value><ControlTemplate TargetType='RepeatButton'><Border Background='Transparent'/>"
				+"</ControlTemplate></Setter.Value></Setter></Style>";
			string templateVertical="<ControlTemplate x:Key='UiLookScrollVertical' TargetType='ScrollBar'>"
				+"<Grid Background='"+track+"'><Grid.RowDefinitions>"
				+"<RowDefinition Height='{DynamicResource {x:Static SystemParameters.VerticalScrollBarButtonHeightKey}}'/><RowDefinition Height='*'/>"
				+"<RowDefinition Height='{DynamicResource {x:Static SystemParameters.VerticalScrollBarButtonHeightKey}}'/></Grid.RowDefinitions>"
				+"<RepeatButton Grid.Row='0' Command='ScrollBar.LineUpCommand' Style='{StaticResource UiLookScrollArrowUp}'/>"
				+"<Track x:Name='PART_Track' Grid.Row='1' IsDirectionReversed='True'>"
				+"<Track.DecreaseRepeatButton><RepeatButton Command='ScrollBar.PageUpCommand' Style='{StaticResource UiLookScrollPage}'/></Track.DecreaseRepeatButton>"
				+"<Track.IncreaseRepeatButton><RepeatButton Command='ScrollBar.PageDownCommand' Style='{StaticResource UiLookScrollPage}'/></Track.IncreaseRepeatButton>"
				+"<Track.Thumb><Thumb Style='{StaticResource UiLookScrollThumbVertical}'/></Track.Thumb></Track>"
				+"<RepeatButton Grid.Row='2' Command='ScrollBar.LineDownCommand' Style='{StaticResource UiLookScrollArrowDown}'/></Grid>"
				+"<ControlTemplate.Triggers><Trigger Property='IsEnabled' Value='False'><Setter TargetName='PART_Track' Property='Visibility' Value='Hidden'/></Trigger>"
				+"</ControlTemplate.Triggers></ControlTemplate>";
			string templateHorizontal="<ControlTemplate x:Key='UiLookScrollHorizontal' TargetType='ScrollBar'>"
				+"<Grid Background='"+track+"'><Grid.ColumnDefinitions>"
				+"<ColumnDefinition Width='{DynamicResource {x:Static SystemParameters.HorizontalScrollBarButtonWidthKey}}'/><ColumnDefinition Width='*'/>"
				+"<ColumnDefinition Width='{DynamicResource {x:Static SystemParameters.HorizontalScrollBarButtonWidthKey}}'/></Grid.ColumnDefinitions>"
				+"<RepeatButton Grid.Column='0' Command='ScrollBar.LineLeftCommand' Style='{StaticResource UiLookScrollArrowLeft}'/>"
				+"<Track x:Name='PART_Track' Grid.Column='1'>"
				+"<Track.DecreaseRepeatButton><RepeatButton Command='ScrollBar.PageLeftCommand' Style='{StaticResource UiLookScrollPage}'/></Track.DecreaseRepeatButton>"
				+"<Track.IncreaseRepeatButton><RepeatButton Command='ScrollBar.PageRightCommand' Style='{StaticResource UiLookScrollPage}'/></Track.IncreaseRepeatButton>"
				+"<Track.Thumb><Thumb Style='{StaticResource UiLookScrollThumbHorizontal}'/></Track.Thumb></Track>"
				+"<RepeatButton Grid.Column='2' Command='ScrollBar.LineRightCommand' Style='{StaticResource UiLookScrollArrowRight}'/></Grid>"
				+"<ControlTemplate.Triggers><Trigger Property='IsEnabled' Value='False'><Setter TargetName='PART_Track' Property='Visibility' Value='Hidden'/></Trigger>"
				+"</ControlTemplate.Triggers></ControlTemplate>";
			return "<ResourceDictionary xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>"
				+arrowStyle("UiLookScrollArrowUp",KEY_CHEVRON_UP)+arrowStyle("UiLookScrollArrowDown",KEY_CHEVRON_DOWN)
				+arrowStyle("UiLookScrollArrowLeft",KEY_CHEVRON_LEFT)+arrowStyle("UiLookScrollArrowRight",KEY_CHEVRON_RIGHT)
				+thumbStyle("UiLookScrollThumbVertical",across+","+along+","+across+","+along)
				+thumbStyle("UiLookScrollThumbHorizontal",along+","+across+","+along+","+across)
				+pageStyle+templateVertical+templateHorizontal
				+"<Style TargetType='ScrollBar'><Setter Property='Template' Value='{StaticResource UiLookScrollVertical}'/>"
				+"<Style.Triggers><Trigger Property='Orientation' Value='Horizontal'><Setter Property='Template' Value='{StaticResource UiLookScrollHorizontal}'/>"
				+"</Trigger></Style.Triggers></Style>"
				+"</ResourceDictionary>";
		}
	}
}
