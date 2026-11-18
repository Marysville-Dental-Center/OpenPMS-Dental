// The control state pictures and checks of package UI-4c-2 (docs/specs/ui-refresh-plan.md, section 7's UI-4c-2 line and section 3.6;
// docs/port-notes/UI-4c.md): the WinForms OdProgressBar and the WPF ProgressBarFlat against the design sheet's progress bar, ODColorPicker's swatch
// in every state, the database integrity warning (WinForms and WPF), ODPictureBox's border and hint, ODCodeRangeFilter's hint, the print preview's
// area around the pages (the page itself unchanged), the WPF signature box's frame (the signature, its picture and its validity unchanged; office
// content keeps today's frame), the colour dialog's frames, the progress window's bar and error text, and the TextBelow strip (UQ-6). Each picture is
// drawn off screen and compared with the design sheet's drawing of the state at the control's own geometry (DialogDrawings) by ChartImageCompare's
// tolerance, as UI-4a and UI-4b did: WinForms every pixel, WPF the solid pixels. No window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToothChartImageTests;

namespace UiRefreshTests {
	[TestClass]
	public class DialogStateTests {
		#region WinForms
		[TestMethod]
		public void WinForms_ProgressBar_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(float scale in ControlStateGallery.ArrayScales) {
					foreach(DialogPictureState state in DialogControls.ArrayProgressStatesWinForms) {
						string name="WinForms OdProgressBar "+state+" at "+(int)Math.Round(scale*100)+"%";
						using(Bitmap bitmapActual=DialogControls.RenderWinFormsProgress(state,scale))
						using(Bitmap bitmapExpected=DialogDrawings.Progress(bitmapActual.Size,state,scale,SheetPainter.Light(EnumUiColor.Surface))) {
							ControlStateTests.CompareAll(listFailures,name,bitmapExpected,bitmapActual);
						}
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_ColorPicker_Swatch_EveryState() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(float scale in ControlStateGallery.ArrayScales) {
					foreach(DialogPictureState state in DialogControls.ArraySwatchStates) {
						string name="WinForms ODColorPicker "+state+" at "+(int)Math.Round(scale*100)+"%";
						Rectangle rectangleSwatch;
						using(Bitmap bitmap=DialogControls.RenderWinFormsColorPicker(state,scale,out rectangleSwatch))
						using(Bitmap bitmapSwatch=bitmap.Clone(rectangleSwatch,PixelFormat.Format32bppArgb))
						using(Bitmap bitmapExpected=DialogDrawings.Swatch(rectangleSwatch.Size,state,scale)) {
							ControlStateTests.CompareAll(listFailures,name,bitmapExpected,bitmapSwatch);
						}
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_ColorPicker_TheBoundsAndTheColourStay() {
			IconRendererTests.RunSta(() => {
				using(OpenPmsDental.UI.ODColorPicker colorPicker=new OpenPmsDental.UI.ODColorPicker()) {
					System.Windows.Forms.Button butColor=ListStateControls.GetPrivate<System.Windows.Forms.Button>(colorPicker,"butColor");
					Assert.AreEqual(new Rectangle(1,0,30,20),butColor.Bounds,"The colour button keeps its designer bounds.");
					Assert.AreEqual(new Size(74,21),colorPicker.Size);
					colorPicker.BackgroundColor=DialogControls.ColorSwatch;
					Assert.AreEqual(DialogControls.ColorSwatch,colorPicker.BackgroundColor,"The colour is the office's data and comes back as set.");
					Assert.AreEqual(DialogControls.ColorSwatch,butColor.BackColor);
				}
			});
		}

		[TestMethod]
		public void WinForms_WarningIntegrity_WarningAndDanger_WithTheWhiteMark() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(bool isRed in new[] { false,true }) {
					string name="WinForms WarningIntegrity "+(isRed ? "red" : "triangle");
					Color colorFill=SheetPainter.Light(isRed ? EnumUiColor.Danger : EnumUiColor.Warning);
					using(Bitmap bitmap=DialogControls.RenderWinFormsWarning(isRed,1f)) {
						CheckPixel(listFailures,name+" background",bitmap,0,0,SheetPainter.Light(EnumUiColor.Surface));
						CheckPixel(listFailures,name+" fill",bitmap,4,14,colorFill);
						CheckPixel(listFailures,name+" fill",bitmap,13,14,colorFill);
						CheckPixel(listFailures,name+" mark",bitmap,9,8,SheetPainter.Light(EnumUiColor.TextOnAccent));
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_PictureBox_StrokeBorder_TextSecondaryHint_ThePictureInside() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				using(Bitmap bitmapChrome=DialogControls.RenderWinFormsPictureBox(hasBorder:true,withText:false))
				using(Bitmap bitmapText=DialogControls.RenderWinFormsPictureBox(hasBorder:true,withText:true))
				using(Bitmap bitmapNoBorder=DialogControls.RenderWinFormsPictureBox(hasBorder:false,withText:false)) {
					Color colorStroke=SheetPainter.Light(EnumUiColor.Stroke);
					foreach(Point point in new[] { new Point(0,0),new Point(60,0),new Point(119,40),new Point(60,79),new Point(0,40) }) {
						CheckPixel(listFailures,"PictureBox border",bitmapChrome,point.X,point.Y,colorStroke);
					}
					CheckPixel(listFailures,"PictureBox without border",bitmapNoBorder,0,0,SheetPainter.Light(EnumUiColor.Surface));
					ControlStateTests.CheckText(listFailures,"PictureBox hint",bitmapChrome,bitmapText,SheetPainter.Light(EnumUiColor.TextSecondary));
				}
				//A picture inside the border starts after it and is the picture's own colours.
				using(Bitmap bitmapPicture=DialogControls.MakeSyntheticPicture())
				using(Bitmap bitmapWith=DialogControls.RenderWinFormsPictureBox(hasBorder:true,withText:false,image:(Bitmap)bitmapPicture.Clone())) {
					CheckPixel(listFailures,"PictureBox border with a picture",bitmapWith,0,40,SheetPainter.Light(EnumUiColor.Stroke));
					CheckPixel(listFailures,"PictureBox picture",bitmapWith,2,40,Color.White);
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_CodeRangeFilter_TheExampleIsAHint() {
			IconRendererTests.RunSta(() => {
				using(OpenPmsDental.UI.ODCodeRangeFilter codeRangeFilter=new OpenPmsDental.UI.ODCodeRangeFilter()) {
					System.Windows.Forms.Label labelExample=ListStateControls.GetPrivate<System.Windows.Forms.Label>(codeRangeFilter,"labelExample");
					Assert.AreEqual(UiTheme.TextSecondary.ToArgb(),labelExample.ForeColor.ToArgb());
					Assert.AreEqual(new Rectangle(2,23,108,16),labelExample.Bounds,"The example keeps its place.");
				}
			});
		}

		[TestMethod]
		public void WinForms_PrintPreview_TheAreaAroundThePagesIsSubtlePressed_ThePagesAreWindows() {
			IconRendererTests.RunSta(() => {
				System.Windows.Forms.Panel panel;
				using(System.Windows.Forms.Form form=DialogControls.MakeHost(out panel)) {
					OpenPmsDental.UI.ODPrintPreviewControl printPreviewControl=new OpenPmsDental.UI.ODPrintPreviewControl();
					Assert.AreEqual(UiTheme.SubtlePressed.ToArgb(),printPreviewControl.BackColor.ToArgb(),"The area around the pages.");
					panel.Controls.Add(printPreviewControl);
					printPreviewControl.Bounds=new Rectangle(10,10,200,150);
					IntPtr handle=printPreviewControl.Handle;
					//Without a document (a document would print into the preview behind a Windows status window, which these tests never show).
					using(Bitmap bitmap=DialogControls.Draw(printPreviewControl)) {
						Assert.AreEqual(UiTheme.SubtlePressed.ToArgb(),bitmap.GetPixel(2,2).ToArgb());
						Assert.AreEqual(UiTheme.SubtlePressed.ToArgb(),bitmap.GetPixel(197,147).ToArgb());
					}
				}
			});
			//The pages, their frame and their shadow are Windows' PrintPreviewControl's drawing, as before: the control draws nothing of its own.
			Type type=typeof(OpenPmsDental.UI.ODPrintPreviewControl);
			System.Reflection.BindingFlags bindingFlags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public
				|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.DeclaredOnly;
			foreach(string method in new[] { "OnPaint","OnPaintBackground","WndProc" }) {
				Assert.IsNull(type.GetMethod(method,bindingFlags),"ODPrintPreviewControl draws nothing of its own ("+method+").");
			}
		}

		[TestMethod]
		public void WinForms_SignatureBox_StrokeStrongFrame_TheSignatureAndItsValidityAsBefore() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				bool isValidNew;
				bool isValidPinned;
				Bitmap bitmapSigNew;
				Bitmap bitmapSigPinned;
				using(Bitmap bitmapNew=DialogControls.RenderSignatureBox(isPinned:false,out isValidNew,out bitmapSigNew))
				using(Bitmap bitmapPinned=DialogControls.RenderSignatureBox(isPinned:true,out isValidPinned,out bitmapSigPinned))
				using(bitmapSigNew)
				using(bitmapSigPinned) {
					Assert.IsTrue(isValidNew && isValidPinned,"The signature is valid in both looks.");
					CheckPixel(listFailures,"Signature frame",bitmapNew,0,40,SheetPainter.Light(EnumUiColor.StrokeStrong));
					CheckPixel(listFailures,"Signature frame",bitmapNew,180,80,SheetPainter.Light(EnumUiColor.StrokeStrong));
					CheckPixel(listFailures,"Signature frame, office content",bitmapPinned,0,40,System.Drawing.SystemColors.ControlDark);
					//The signature's picture (the ink, GetSigImage) is the same pixel for pixel.
					Assert.AreEqual(bitmapSigPinned.Size,bitmapSigNew.Size);
					int countInk=0;
					for(int y=0;y<bitmapSigNew.Height;y++) {
						for(int x=0;x<bitmapSigNew.Width;x++) {
							int argb=bitmapSigNew.GetPixel(x,y).ToArgb();
							if(argb!=bitmapSigPinned.GetPixel(x,y).ToArgb()) {
								listFailures.Add("The signature's picture differs at "+x+","+y+".");
								x=bitmapSigNew.Width;
								y=bitmapSigNew.Height;
								break;
							}
							if(argb!=Color.White.ToArgb() && bitmapSigNew.GetPixel(x,y).A>0) {
								countInk++;
							}
						}
					}
					if(countInk==0) {
						listFailures.Add("The signature's picture has no ink.");
					}
					//Inside the frame the box draws the same (its 1 px frame aside).
					for(int y=1;y<bitmapNew.Height-1;y+=3) {
						for(int x=1;x<bitmapNew.Width-1;x+=3) {
							if(bitmapNew.GetPixel(x,y).ToArgb()!=bitmapPinned.GetPixel(x,y).ToArgb()) {
								listFailures.Add("Inside the frame the box differs at "+x+","+y+".");
								x=bitmapNew.Width;
								y=bitmapNew.Height;
								break;
							}
						}
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_TextBelowStrip_TextOnSurfaceRaised_UQ6() {
			IconRendererTests.RunSta(() => {
				using(OpenPmsDental.ToothChartTextBelowStrip strip=new OpenPmsDental.ToothChartTextBelowStrip()) {
					Assert.AreEqual(UiTheme.Text.ToArgb(),strip.ForeColor.ToArgb());
					Assert.AreEqual(UiTheme.SurfaceRaised.ToArgb(),strip.BackColor.ToArgb());
					Assert.IsTrue(strip.ReadOnly && !strip.Multiline && !strip.TabStop && !strip.Visible,"Its behaviour is as before.");
				}
			});
		}
		#endregion WinForms

		#region WPF
		[TestMethod]
		public void Wpf_ProgressBar_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				foreach(DialogPictureState state in DialogControls.ArrayProgressStatesWpf) {
					string name="WPF ProgressBarFlat "+state;
					using(Bitmap bitmapActual=DialogControls.RenderWpfProgress(state,1f))
					using(Bitmap bitmapExpected=DialogDrawings.Progress(bitmapActual.Size,state,1f,Color.White)) {
						ControlStateTests.CompareSolid(listFailures,name,bitmapExpected,bitmapActual);
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_ProgressBar_TemplateAndMarquee() {
			IconRendererTests.RunSta(() => {
				System.Windows.Controls.ControlTemplate controlTemplate=WpfControls.UI.ProgressBarFlat.GetTemplate();
				Assert.AreSame(controlTemplate,WpfControls.UI.ProgressBarFlat.GetTemplate(),"One template per thread.");
				WpfControls.UI.ProgressBarFlat progressBar=DialogControls.MakeWpfProgress(DialogPictureState.Rest);
				Assert.AreSame(controlTemplate,progressBar.Template);
				System.Windows.Controls.Border borderTrack=(System.Windows.Controls.Border)controlTemplate.FindName("PART_Track",progressBar);
				System.Windows.Controls.Border borderIndicator=(System.Windows.Controls.Border)controlTemplate.FindName("PART_Indicator",progressBar);
				System.Windows.Controls.Border borderMarquee=(System.Windows.Controls.Border)controlTemplate.FindName(WpfControls.UI.ProgressBarFlat.NAME_MARQUEE,progressBar);
				System.Windows.Controls.Border borderOutline=(System.Windows.Controls.Border)controlTemplate.FindName(WpfControls.UI.ProgressBarFlat.NAME_OUTLINE,progressBar);
				AssertBrush(EnumUiColor.SurfaceRaised,borderTrack.Background,"track");
				AssertBrush(EnumUiColor.Accent,borderIndicator.Background,"fill");
				AssertBrush(EnumUiColor.Stroke,borderOutline.BorderBrush,"outline");
				Assert.AreEqual(WpfControls.UI.ProgressBarFlat.CORNER,borderTrack.CornerRadius.TopLeft);
				Assert.AreEqual(System.Windows.Visibility.Collapsed,borderMarquee.Visibility,"No marquee when determinate.");
				progressBar.IsIndeterminate=true;
				Assert.AreEqual(System.Windows.Visibility.Visible,borderMarquee.Visibility,"The marquee when indeterminate.");
				Assert.AreEqual(System.Windows.Visibility.Hidden,borderIndicator.Visibility);
				Assert.IsInstanceOfType(borderMarquee.Background,typeof(System.Windows.Media.DrawingBrush));
				Assert.IsFalse(progressBar.IsMarqueeRunning,"Not loaded (off screen): no animation runs.");
				progressBar.IsIndeterminate=false;
				progressBar.IsEnabled=false;
				AssertBrush(EnumUiColor.TextDisabled,borderIndicator.Background,"disabled fill");
			});
		}

		[TestMethod]
		public void Wpf_ProgressWindow_TheFlatBar_AndTheErrorInDanger() {
			IconRendererTests.RunSta(() => {
				WpfControls.UI.FrmProgressAuto frmProgressAuto=new WpfControls.UI.FrmProgressAuto();
				object progressBar=ListStateControls.GetPrivate<object>(frmProgressAuto,"progressBar");
				Assert.IsInstanceOfType(progressBar,typeof(WpfControls.UI.ProgressBarFlat));
				WpfControls.UI.ProgressBarFlat progressBarFlat=(WpfControls.UI.ProgressBarFlat)progressBar;
				Assert.AreEqual(363,progressBarFlat.Width,"Its size as before.");
				Assert.AreEqual(23,progressBarFlat.Height);
				WpfControls.UI.Label labelError=ListStateControls.GetPrivate<WpfControls.UI.Label>(frmProgressAuto,"labelError");
				Assert.AreEqual(UiTheme.GetColorWpf(EnumUiColor.Danger),labelError.ColorText);
			});
		}

		[TestMethod]
		public void Wpf_WarningIntegrity_WarningAndDanger_WithTheWhiteMark() {
			IconRendererTests.RunSta(() => {
				foreach(bool isRed in new[] { false,true }) {
					WpfControls.UI.WarningIntegrity warningIntegrity=DialogControls.MakeWpfWarning(isRed);
					System.Windows.Shapes.Path path=ListStateControls.GetPrivate<System.Windows.Shapes.Path>(warningIntegrity,"path");
					System.Windows.Controls.TextBlock textMark=ListStateControls.GetPrivate<System.Windows.Controls.TextBlock>(warningIntegrity,"textMark");
					EnumUiColor uiColor=isRed ? EnumUiColor.Danger : EnumUiColor.Warning;
					AssertBrush(uiColor,path.Fill,"fill");
					AssertBrush(uiColor,path.Stroke,"outline");
					AssertBrush(EnumUiColor.TextOnAccent,textMark.Foreground,"mark");
				}
			});
		}

		[TestMethod]
		public void Wpf_WarningIntegrity_Picture() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				foreach(bool isRed in new[] { false,true }) {
					using(Bitmap bitmap=DialogControls.RenderWpfWarning(isRed,1f)) {
						string name="WPF WarningIntegrity "+(isRed ? "red" : "triangle");
						CheckPixel(listFailures,name+" fill",bitmap,4,15,SheetPainter.Light(isRed ? EnumUiColor.Danger : EnumUiColor.Warning));
						CheckPixel(listFailures,name+" background",bitmap,0,0,Color.White);
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_SignatureBox_Frame_AndOfficeContentKeepsToday() {
			IconRendererTests.RunSta(() => {
				WpfControls.UI.SignatureBoxWrapper signatureBoxWrapper=new WpfControls.UI.SignatureBoxWrapper();
				Assert.AreEqual(UiTheme.StrokeStrong.ToArgb(),signatureBoxWrapper.signatureBoxWrapper.BackColor.ToArgb(),"The frame is an input's outline.");
				WpfControls.UI.ContentLook.SetIsPinned(signatureBoxWrapper,true);
				Assert.AreEqual(WpfControls.UI.SignatureBoxWrapper.ColorFramePinned.ToArgb(),signatureBoxWrapper.signatureBoxWrapper.BackColor.ToArgb(),
					"Office content (the eForm) keeps today's frame.");
				Assert.AreEqual(System.Drawing.SystemColors.ControlDark.ToArgb(),WpfControls.UI.SignatureBoxWrapper.ColorFramePinned.ToArgb());
				WpfControls.UI.ContentLook.SetIsPinned(signatureBoxWrapper,false);
				Assert.AreEqual(UiTheme.StrokeStrong.ToArgb(),signatureBoxWrapper.signatureBoxWrapper.BackColor.ToArgb(),"And back.");
				Assert.AreEqual(new Size(364,81),signatureBoxWrapper.signatureBoxWrapper.Size,"The box keeps its size.");
			});
		}

		[TestMethod]
		public void Wpf_ColorDialog_StrokeStrongFrames_TextHandle() {
			IconRendererTests.RunSta(() => {
				WpfControls.UI.FrmColorDialog frmColorDialog=new WpfControls.UI.FrmColorDialog();
				AssertBrush(EnumUiColor.StrokeStrong,ListStateControls.GetPrivate<System.Windows.Controls.Border>(frmColorDialog,"borderSquare").BorderBrush,"square");
				AssertBrush(EnumUiColor.StrokeStrong,ListStateControls.GetPrivate<System.Windows.Controls.Border>(frmColorDialog,"borderHue").BorderBrush,"hue bar");
				AssertBrush(EnumUiColor.Text,ListStateControls.GetPrivate<System.Windows.Shapes.Rectangle>(frmColorDialog,"rectangleHueSlider").Stroke,"hue handle");
			});
		}
		#endregion WPF

		#region Helpers
		private static void CheckPixel(List<string> listFailures,string name,Bitmap bitmap,int x,int y,Color colorExpected) {
			Color colorActual=bitmap.GetPixel(x,y);
			if(!ControlStateTests.IsNear(colorExpected,colorActual,ChartImageCompare.ChannelTolerance)) {
				listFailures.Add(name+" at "+x+","+y+": expected "+ColorTranslator.ToHtml(colorExpected)+", got "+ColorTranslator.ToHtml(Color.FromArgb(colorActual.R,colorActual.G,colorActual.B))+".");
			}
		}

		private static void AssertBrush(EnumUiColor uiColor,System.Windows.Media.Brush brush,string what) {
			Assert.IsInstanceOfType(brush,typeof(System.Windows.Media.SolidColorBrush),what);
			Assert.AreEqual(UiTheme.GetColorWpf(uiColor),((System.Windows.Media.SolidColorBrush)brush).Color,what+" is "+uiColor+".");
		}
		#endregion Helpers
	}
}
