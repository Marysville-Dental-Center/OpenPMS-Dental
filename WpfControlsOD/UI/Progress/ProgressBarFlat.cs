// The progress windows' progress bar in the new look (docs/specs/ui-refresh-plan.md section 3.6, "Dialogs": progress bars in self-drawn progress
// windows fill with Accent; package UI-4c-2, docs/port-notes/UI-4c.md). A WPF ProgressBar with its own template, drawn as the design sheet's progress
// bar (DesignSheetControls.DrawCalendarAndProgress) and the WinForms one (CodeBase.UiDialogPaint.DrawProgress): a SurfaceRaised track with 4 px
// corners, the Accent fill (TextDisabled when disabled) and a 1 px Stroke outline; no gradient, no glow. Indeterminate (the default of
// FrmProgressAuto, a marquee): an Accent segment a third of the track wide moves across the track from left to right, again and again, clipped to
// its corners, only while the bar is indeterminate and visible. Everything else is the ProgressBar's (Value, Maximum, IsIndeterminate, its size).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CodeBase;

namespace WpfControls.UI {
	///<summary>The progress bar of the progress windows (FrmProgressAuto) in the new look.</summary>
	public class ProgressBarFlat:ProgressBar {
		///<summary>The corner radius at 96 DPI (UiTheme.CORNER_CONTROL).</summary>
		public const double CORNER=UiTheme.CORNER_CONTROL;
		///<summary>The marquee segment's width as a share of the track.</summary>
		public const double MARQUEE_WIDTH=0.3;
		///<summary>One pass of the marquee across the track.</summary>
		public static readonly TimeSpan TimeSpanMarquee=TimeSpan.FromSeconds(1.6);
		///<summary>The names of the template's parts besides PART_Track and PART_Indicator.</summary>
		public const string NAME_MARQUEE="marquee";
		public const string NAME_OUTLINE="outline";
		///<summary>The template, built once per UI thread (the progress windows can run on threads of their own).</summary>
		[ThreadStatic]
		private static ControlTemplate _controlTemplate;
		private Border _borderMarquee;
		private TranslateTransform _translateTransformMarquee;
		private bool _isMarqueeRunning;

		public ProgressBarFlat() {
			Template=GetTemplate();
			IsVisibleChanged+=(sender,e) => UpdateMarquee();
			Unloaded+=(sender,e) => UpdateMarquee();
			Loaded+=(sender,e) => UpdateMarquee();
		}

		///<summary>True while the marquee moves: the bar is indeterminate, visible and loaded.</summary>
		public bool IsMarqueeRunning {
			get { return _isMarqueeRunning; }
		}

		///<summary>The template: the track (PART_Track, SurfaceRaised, 4 px corners), the fill (PART_Indicator, Accent, its width set by the
		///ProgressBar), the marquee segment's border (hidden unless indeterminate) and the 1 px Stroke outline on top. Disabled: the fill
		///TextDisabled. Indeterminate: the fill hidden, the marquee shown.</summary>
		public static ControlTemplate GetTemplate() {
			if(_controlTemplate!=null) {
				return _controlTemplate;
			}
			ControlTemplate controlTemplate=new ControlTemplate(typeof(ProgressBar));
			FrameworkElementFactory factoryRoot=new FrameworkElementFactory(typeof(System.Windows.Controls.Grid));//not WpfControls.UI.Grid, the program's grid
			factoryRoot.SetValue(UIElement.SnapsToDevicePixelsProperty,true);
			factoryRoot.SetValue(FrameworkElement.UseLayoutRoundingProperty,true);
			FrameworkElementFactory factoryTrack=new FrameworkElementFactory(typeof(Border),"PART_Track");
			factoryTrack.SetValue(Border.BackgroundProperty,UiLook.Brush(EnumUiColor.SurfaceRaised));
			factoryTrack.SetValue(Border.CornerRadiusProperty,new CornerRadius(CORNER));
			factoryRoot.AppendChild(factoryTrack);
			FrameworkElementFactory factoryIndicator=new FrameworkElementFactory(typeof(Border),"PART_Indicator");
			factoryIndicator.SetValue(FrameworkElement.HorizontalAlignmentProperty,HorizontalAlignment.Left);
			factoryIndicator.SetValue(Border.BackgroundProperty,UiLook.Brush(EnumUiColor.Accent));
			factoryIndicator.SetValue(Border.CornerRadiusProperty,new CornerRadius(CORNER));
			factoryRoot.AppendChild(factoryIndicator);
			FrameworkElementFactory factoryMarquee=new FrameworkElementFactory(typeof(Border),NAME_MARQUEE);
			factoryMarquee.SetValue(Border.CornerRadiusProperty,new CornerRadius(CORNER));
			factoryMarquee.SetValue(UIElement.VisibilityProperty,Visibility.Collapsed);
			factoryRoot.AppendChild(factoryMarquee);
			FrameworkElementFactory factoryOutline=new FrameworkElementFactory(typeof(Border),NAME_OUTLINE);
			factoryOutline.SetValue(Border.BorderBrushProperty,UiLook.Brush(EnumUiColor.Stroke));
			factoryOutline.SetValue(Border.BorderThicknessProperty,new Thickness(1));
			factoryOutline.SetValue(Border.CornerRadiusProperty,new CornerRadius(CORNER));
			factoryRoot.AppendChild(factoryOutline);
			controlTemplate.VisualTree=factoryRoot;
			Trigger triggerDisabled=new Trigger { Property=UIElement.IsEnabledProperty,Value=false };
			triggerDisabled.Setters.Add(new Setter(Border.BackgroundProperty,UiLook.Brush(EnumUiColor.TextDisabled),"PART_Indicator"));
			controlTemplate.Triggers.Add(triggerDisabled);
			Trigger triggerIndeterminate=new Trigger { Property=ProgressBar.IsIndeterminateProperty,Value=true };
			triggerIndeterminate.Setters.Add(new Setter(UIElement.VisibilityProperty,Visibility.Hidden,"PART_Indicator"));
			triggerIndeterminate.Setters.Add(new Setter(UIElement.VisibilityProperty,Visibility.Visible,NAME_MARQUEE));
			controlTemplate.Triggers.Add(triggerIndeterminate);
			controlTemplate.Seal();
			_controlTemplate=controlTemplate;
			return controlTemplate;
		}

		///<summary>The marquee's brush: an Accent segment MARQUEE_WIDTH of the border wide at its left, the rest transparent; its RelativeTransform
		///moves it (X in shares of the border's width).</summary>
		public static DrawingBrush MakeMarqueeBrush(TranslateTransform translateTransform) {
			DrawingGroup drawingGroup=new DrawingGroup();
			drawingGroup.Children.Add(new GeometryDrawing(Brushes.Transparent,null,new RectangleGeometry(new Rect(0,0,1,1))));
			drawingGroup.Children.Add(new GeometryDrawing(UiLook.Brush(EnumUiColor.Accent),null,new RectangleGeometry(new Rect(0,0,MARQUEE_WIDTH,1))));
			DrawingBrush drawingBrush=new DrawingBrush(drawingGroup);
			drawingBrush.Stretch=Stretch.Fill;
			drawingBrush.RelativeTransform=translateTransform;
			return drawingBrush;
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			_borderMarquee=GetTemplateChild(NAME_MARQUEE) as Border;
			_isMarqueeRunning=false;
			if(_borderMarquee!=null) {
				_translateTransformMarquee=new TranslateTransform(-MARQUEE_WIDTH,0);
				_borderMarquee.Background=MakeMarqueeBrush(_translateTransformMarquee);
			}
			UpdateMarquee();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
			base.OnPropertyChanged(e);
			if(e.Property==IsIndeterminateProperty) {
				UpdateMarquee();
			}
		}

		///<summary>Starts the marquee while the bar is indeterminate, visible and loaded, and stops it otherwise, so a hidden or determinate bar runs no
		///animation.</summary>
		private void UpdateMarquee() {
			if(_translateTransformMarquee==null) {
				return;
			}
			bool isRun=IsIndeterminate && IsVisible && IsLoaded;
			if(isRun==_isMarqueeRunning) {
				return;
			}
			_isMarqueeRunning=isRun;
			if(!isRun) {
				_translateTransformMarquee.BeginAnimation(TranslateTransform.XProperty,null);
				return;
			}
			DoubleAnimation doubleAnimation=new DoubleAnimation(-MARQUEE_WIDTH,1,new Duration(TimeSpanMarquee));
			doubleAnimation.RepeatBehavior=RepeatBehavior.Forever;
			_translateTransformMarquee.BeginAnimation(TranslateTransform.XProperty,doubleAnimation);
		}
	}
}
