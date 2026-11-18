// The real controls of package UI-4c-2 (dialogs, progress and the rest; docs/port-notes/UI-4c.md) put in each state off screen (DialogControls), and
// the design sheet's drawing of each state at a control's own geometry (DialogDrawings: SheetPainter's shapes with the light tokens, as
// DesignSheetControls draws the progress bar and the combo box's input outline). Used by DialogStateTests and by the G-UI1 part 2f state strips
// (DialogGallery). No window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using CodeBase;

namespace UiRefreshTests {
	///<summary>The states of the pictures: a progress bar at rest (62%), empty, full, disabled, with its target line, indeterminate; a swatch at rest,
	///under the mouse, with keyboard focus, disabled, without a colour (None); the warning triangle and the red one.</summary>
	public enum DialogPictureState {
		Rest,
		Empty,
		Full,
		Disabled,
		Target,
		Indeterminate,
		Hover,
		Focus,
		None,
		Red,
	}

	///<summary>The colour picker with its swatch's hover and focus under the test's control.</summary>
	public class StateColorPicker : OpenPmsDental.UI.ODColorPicker {
		public bool IsFocusForced;
		public bool IsHoverForced;

		protected override bool IsFocusRingShown {
			get { return IsFocusForced; }
		}

		protected override bool IsHoverColor {
			get { return IsHoverForced; }
		}
	}

	public static class DialogControls {
		public static readonly DialogPictureState[] ArrayProgressStatesWinForms=new DialogPictureState[] { DialogPictureState.Rest,DialogPictureState.Empty,
			DialogPictureState.Full,DialogPictureState.Disabled,DialogPictureState.Target };
		public static readonly DialogPictureState[] ArrayProgressStatesWpf=new DialogPictureState[] { DialogPictureState.Rest,DialogPictureState.Empty,
			DialogPictureState.Full,DialogPictureState.Disabled,DialogPictureState.Indeterminate };
		public static readonly DialogPictureState[] ArraySwatchStates=new DialogPictureState[] { DialogPictureState.Rest,DialogPictureState.Hover,
			DialogPictureState.Focus,DialogPictureState.Disabled,DialogPictureState.None };
		///<summary>A data colour for the swatch (an office's appointment colour, say).</summary>
		public static readonly Color ColorSwatch=Color.FromArgb(255,0,128,96);
		///<summary>The progress bar's size at 96 DPI (the design sheet's track) and its value at rest.</summary>
		public const int WIDTH_PROGRESS=200;
		public const int HEIGHT_PROGRESS=14;
		public const int VALUE_REST=62;
		public const int VALUE_TARGET=80;

		public static int GetProgressValue(DialogPictureState state) {
			switch(state) {
				case DialogPictureState.Empty:
					return 0;
				case DialogPictureState.Full:
					return 100;
				default:
					return VALUE_REST;
			}
		}

		#region WinForms
		///<summary>A hidden form (never shown) with a panel in the Surface colour, the parent of the control under test.</summary>
		public static Form MakeHost(out System.Windows.Forms.Panel panel) {
			Form form=new Form();
			form.ShowInTaskbar=false;
			form.Size=new Size(900,700);
			panel=new System.Windows.Forms.Panel();
			panel.BackColor=SheetPainter.Light(EnumUiColor.Surface);
			panel.Bounds=new Rectangle(0,0,880,660);
			form.Controls.Add(panel);
			return form;
		}

		public static Bitmap Draw(Control control) {
			Bitmap bitmap=new Bitmap(control.Width,control.Height,PixelFormat.Format32bppArgb);
			control.DrawToBitmap(bitmap,new Rectangle(0,0,control.Width,control.Height));
			return bitmap;
		}

		///<summary>OdProgressBar (200 x 14 at 96 DPI, 0 to 100) in the state, drawn by its own paint (DrawBar, the body of OnPaint) into a bitmap.</summary>
		public static Bitmap RenderWinFormsProgress(DialogPictureState state,float scale) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				OpenPmsDental.UI.OdProgressBar progressBar=new OpenPmsDental.UI.OdProgressBar();
				progressBar.LayoutManager.SetScaleMS(scale);
				panel.Controls.Add(progressBar);
				progressBar.Bounds=new Rectangle(10,10,UiTheme.ScaleI(WIDTH_PROGRESS,scale),UiTheme.ScaleI(HEIGHT_PROGRESS,scale));
				progressBar.Minimum=0;
				progressBar.Maximum=100;
				progressBar.Value=GetProgressValue(state);
				if(state==DialogPictureState.Disabled) {
					progressBar.Enabled=false;
				}
				if(state==DialogPictureState.Target) {
					progressBar.TargetValue=VALUE_TARGET;
				}
				Bitmap bitmap=new Bitmap(progressBar.Width,progressBar.Height,PixelFormat.Format32bppArgb);
				using(Graphics g=Graphics.FromImage(bitmap)) {
					progressBar.DrawBar(g);
				}
				return bitmap;
			}
		}

		///<summary>ODColorPicker (74 x 21 at 96 DPI, the None button shown) in the state with the data colour (None: transparent). Returns the whole
		///picker's picture and the colour button's bounds in it.</summary>
		public static Bitmap RenderWinFormsColorPicker(DialogPictureState state,float scale,out Rectangle rectangleSwatch) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				StateColorPicker colorPicker=new StateColorPicker();
				colorPicker.LayoutManager.SetScaleMS(scale);
				panel.Controls.Add(colorPicker);
				colorPicker.Bounds=new Rectangle(10,10,UiTheme.ScaleI(74,scale),UiTheme.ScaleI(21,scale));
				colorPicker.AllowTransparentColor=true;
				System.Windows.Forms.Button butColor=ListStateControls.GetPrivate<System.Windows.Forms.Button>(colorPicker,"butColor");
				OpenPmsDental.UI.Button butNone=ListStateControls.GetPrivate<OpenPmsDental.UI.Button>(colorPicker,"butNone");
				//As the window's LayoutManager places them at the scale.
				butColor.Bounds=UiTheme.ScaleRectangle(new Rectangle(1,0,30,20),scale);
				butNone.Bounds=UiTheme.ScaleRectangle(new Rectangle(34,0,40,21),scale);
				butNone.LayoutManager.SetScaleMS(scale);
				butColor.Font=new Font(butColor.Font.FontFamily,butColor.Font.Size*scale);
				butNone.Font=new Font(butNone.Font.FontFamily,butNone.Font.Size*scale);
				colorPicker.BackgroundColor=state==DialogPictureState.None ? Color.Transparent : ColorSwatch;
				switch(state) {
					case DialogPictureState.Hover:
						colorPicker.IsHoverForced=true;
						break;
					case DialogPictureState.Focus:
						colorPicker.IsFocusForced=true;
						break;
					case DialogPictureState.Disabled:
						colorPicker.Enabled=false;
						break;
				}
				IntPtr handle=colorPicker.Handle;
				foreach(Control control in colorPicker.Controls) {
					handle=control.Handle;//a hidden window does not create its children's windows, and DrawToBitmap prints only those that have one
				}
				rectangleSwatch=butColor.Bounds;
				return Draw(colorPicker);
			}
		}

		///<summary>The WinForms database integrity warning (18 x 18 at 96 DPI; it scales from its size) as the triangle or the red one.</summary>
		public static Bitmap RenderWinFormsWarning(bool isRed,float scale) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				OpenPmsDental.UI.WarningIntegrity warningIntegrity=new OpenPmsDental.UI.WarningIntegrity();
				panel.Controls.Add(warningIntegrity);
				int size=UiTheme.ScaleI(18,scale);
				warningIntegrity.Bounds=new Rectangle(10,10,size,size);
				ListStateControls.CallPrivate(warningIntegrity,"SetColor",isRed);
				IntPtr handle=warningIntegrity.Handle;
				return Draw(warningIntegrity);
			}
		}

		///<summary>ODPictureBox (120 x 80) without a picture, showing its hint (withText) or an empty hint, with or without its border.</summary>
		public static Bitmap RenderWinFormsPictureBox(bool hasBorder,bool withText,Image image=null) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				OpenPmsDental.UI.ODPictureBox pictureBox=new OpenPmsDental.UI.ODPictureBox();
				panel.Controls.Add(pictureBox);
				pictureBox.Bounds=new Rectangle(10,10,120,80);
				pictureBox.HasBorder=hasBorder;
				pictureBox.TextNullImage=withText ? "No picture" : "";
				if(image!=null) {
					pictureBox.Image=image;
				}
				IntPtr handle=pictureBox.Handle;
				return Draw(pictureBox);
			}
		}

		///<summary>A synthetic picture for the picture box: a white card with a blue square and a dark bar (no patient picture).</summary>
		public static Bitmap MakeSyntheticPicture() {
			Bitmap bitmap=new Bitmap(160,100,PixelFormat.Format32bppArgb);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.White);
				using(SolidBrush solidBrush=new SolidBrush(Color.FromArgb(255,70,110,180))) {
					g.FillRectangle(solidBrush,20,20,50,50);
				}
				g.FillRectangle(Brushes.DimGray,90,40,50,12);
			}
			return bitmap;
		}

		///<summary>The WinForms signature box wrapper that the WPF SignatureBoxWrapper hosts, with a synthetic signature, its frame colour as the WPF
		///host sets it (isPinned: office content).</summary>
		public static Bitmap RenderSignatureBox(bool isPinned,out bool isValid,out Bitmap bitmapSignature) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				WpfControls.UI.SignatureBoxWrapper signatureBoxWrapperWpf=new WpfControls.UI.SignatureBoxWrapper();
				WpfControls.UI.ContentLook.SetIsPinned(signatureBoxWrapperWpf,isPinned);
				OpenPmsDental.UI.SignatureBoxWrapper signatureBoxWrapper=signatureBoxWrapperWpf.signatureBoxWrapper;
				Color colorFrame=signatureBoxWrapper.BackColor;
				//Moved out of the WPF host into a WinForms panel only for the picture; the WPF host shows it the same way.
				ListStateControls.GetPrivate<System.Windows.Forms.Integration.WindowsFormsHost>(signatureBoxWrapperWpf,"windowsFormsHost").Child=null;
				signatureBoxWrapper.BackColor=colorFrame;
				panel.Controls.Add(signatureBoxWrapper);
				signatureBoxWrapper.Bounds=new Rectangle(10,10,364,81);
				const string keyData="Synthetic key data for the signature frame test";
				signatureBoxWrapper.FillSignature(false,keyData,MakeSignature(keyData));
				IntPtr handle=signatureBoxWrapper.Handle;
				foreach(Control control in signatureBoxWrapper.Controls) {
					handle=control.Handle;
				}
				isValid=signatureBoxWrapper.IsValid;
				bitmapSignature=signatureBoxWrapper.GetSigImage();
				Bitmap bitmap=Draw(signatureBoxWrapper);
				signatureBoxWrapper.Dispose();
				return bitmap;
			}
		}

		///<summary>A signature from the program's own box for the key data, as the box makes it when a user signs (as SignatureFontChangeTests).</summary>
		public static string MakeSignature(string keyData) {
			using(OpenPmsDental.UI.SignatureBox signatureBox=new OpenPmsDental.UI.SignatureBox()) {
				signatureBox.SetTabletState(1);
				signatureBox.SetPointList(new List<PointF> { new PointF(10,10),new PointF(40,30),new PointF(70,12),new PointF(100,35),new PointF(0,0),
					new PointF(20,40),new PointF(90,8) });
				signatureBox.SetKeyString("0000000000000000");
				signatureBox.SetAutoKeyData(keyData);
				return signatureBox.GetSigString();
			}
		}
		#endregion WinForms

		#region WPF
		///<summary>The WPF progress windows' bar (ProgressBarFlat, 200 x 14, 0 to 100) in the state. Indeterminate shows the marquee segment at the left
		///end (its animation's first pass at 0; off screen no animation runs).</summary>
		public static WpfControls.UI.ProgressBarFlat MakeWpfProgress(DialogPictureState state) {
			WpfControls.UI.ProgressBarFlat progressBar=new WpfControls.UI.ProgressBarFlat();
			progressBar.Width=WIDTH_PROGRESS;
			progressBar.Height=HEIGHT_PROGRESS;
			progressBar.Minimum=0;
			progressBar.Maximum=100;
			progressBar.Value=GetProgressValue(state);
			if(state==DialogPictureState.Disabled) {
				progressBar.IsEnabled=false;
			}
			progressBar.ApplyTemplate();
			if(state==DialogPictureState.Indeterminate) {
				progressBar.IsIndeterminate=true;
				ListStateControls.GetPrivate<System.Windows.Media.TranslateTransform>(progressBar,"_translateTransformMarquee").X=0;
			}
			return progressBar;
		}

		public static Bitmap RenderWpfProgress(DialogPictureState state,float scale) {
			return ControlPictures.RenderWpf(MakeWpfProgress(state),WIDTH_PROGRESS,HEIGHT_PROGRESS,scale);
		}

		///<summary>The WPF database integrity warning (18 x 18) as the triangle or the red one.</summary>
		public static WpfControls.UI.WarningIntegrity MakeWpfWarning(bool isRed) {
			WpfControls.UI.WarningIntegrity warningIntegrity=new WpfControls.UI.WarningIntegrity();
			warningIntegrity.Width=18;
			warningIntegrity.Height=18;
			ListStateControls.CallPrivate(warningIntegrity,"SetColor",isRed);
			return warningIntegrity;
		}

		public static Bitmap RenderWpfWarning(bool isRed,float scale) {
			return ControlPictures.RenderWpf(MakeWpfWarning(isRed),18,18,scale);
		}
		#endregion WPF
	}

	public static class DialogDrawings {
		private static Bitmap Start(Size size,Color colorBack,float scale,out Graphics g,out SheetPainter p) {
			Bitmap bitmap=new Bitmap(size.Width,size.Height,PixelFormat.Format32bppArgb);
			g=Graphics.FromImage(bitmap);
			g.Clear(colorBack);
			p=new SheetPainter(g,scale,SheetPainter.Light);
			return bitmap;
		}

		///<summary>The design sheet's progress bar (DesignSheetControls.DrawCalendarAndProgress) at the control's size: the SurfaceRaised track with 4 px
		///corners, the Accent fill over the value's share (TextDisabled disabled; indeterminate, the marquee's first third clipped to the track), the
		///1 px Stroke outline over both; the WinForms target line (the caller's red, 2 px inside a 4 px slot) as the control draws it.</summary>
		public static Bitmap Progress(Size size,DialogPictureState state,float scale,Color colorBack) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(size,colorBack,scale,out g,out p);
			using(g)
			using(p) {
				Rectangle rectangleTrack=new Rectangle(Point.Empty,size);
				p.Fill(EnumUiColor.SurfaceRaised,rectangleTrack,UiTheme.CORNER_CONTROL);
				if(state==DialogPictureState.Indeterminate) {
					int widthSegment=(int)Math.Round(size.Width*WpfControls.UI.ProgressBarFlat.MARQUEE_WIDTH);
					g.SetClip(new Rectangle(0,0,widthSegment,size.Height));
					p.Fill(EnumUiColor.Accent,rectangleTrack,UiTheme.CORNER_CONTROL);
					g.ResetClip();
				}
				else {
					int value=DialogControls.GetProgressValue(state);
					int widthFill=(int)Math.Round(size.Width*value/100.0,MidpointRounding.AwayFromZero);
					if(widthFill>0) {
						p.Fill(state==DialogPictureState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.Accent,new Rectangle(0,0,widthFill,size.Height),UiTheme.CORNER_CONTROL);
					}
				}
				p.Outline(EnumUiColor.Stroke,rectangleTrack,p.LineWidth,UiTheme.CORNER_CONTROL);
				if(state==DialogPictureState.Target) {
					int x=(int)(size.Width*(DialogControls.VALUE_TARGET/100.0));
					g.SmoothingMode=SmoothingMode.None;
					g.PixelOffsetMode=PixelOffsetMode.Default;
					g.FillRectangle(Brushes.Red,x,1,2,size.Height-2);
				}
			}
			return bitmap;
		}

		///<summary>The swatch: the picker's background behind the corners, the colour (SurfaceRaised for None, with the diagonal StrokeStrong line) inside
		///4 px corners, the input outline of the state (StrokeStrong, StrokeStrongHover, Stroke disabled) as the sheet's combo box, the focus ring.</summary>
		public static Bitmap Swatch(Size size,DialogPictureState state,float scale) {
			Graphics g;
			SheetPainter p;
			Color colorBack=SheetPainter.Light(EnumUiColor.Surface);
			Bitmap bitmap=Start(size,colorBack,scale,out g,out p);
			using(g)
			using(p) {
				Rectangle rectangle=new Rectangle(Point.Empty,size);
				bool isNone=state==DialogPictureState.None;
				p.Fill(isNone ? p.C(EnumUiColor.SurfaceRaised) : DialogControls.ColorSwatch,rectangle,UiTheme.CORNER_CONTROL);
				if(isNone) {
					int inset=UiTheme.GetCornerRadius(UiTheme.CORNER_CONTROL,scale);
					g.SmoothingMode=SmoothingMode.AntiAlias;
					g.PixelOffsetMode=PixelOffsetMode.Half;
					using(Pen pen=new Pen(p.C(EnumUiColor.StrokeStrong),p.LineWidth)) {
						g.DrawLine(pen,inset,size.Height-inset,size.Width-inset,inset);
					}
				}
				EnumUiColor uiColorBorder=EnumUiColor.StrokeStrong;
				if(state==DialogPictureState.Hover) {
					uiColorBorder=EnumUiColor.StrokeStrongHover;
				}
				if(state==DialogPictureState.Disabled) {
					uiColorBorder=EnumUiColor.Stroke;
				}
				p.Outline(uiColorBorder,rectangle,p.LineWidth,UiTheme.CORNER_CONTROL);
				if(state==DialogPictureState.Focus) {
					p.FocusRing(rectangle,UiTheme.CORNER_CONTROL);
				}
			}
			return bitmap;
		}
	}
}
