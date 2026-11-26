// The X-ray capture session window for the how-to guides (docs/how-to/, docs/screenshots/README.md): FrmCaptureSession on the
// fake sensor of CaptureControllerHarness, two of four slots exposed with synthetic pictures, and the last image in its own window, drawn off screen (a hidden HwndSource,
// RenderTargetBitmap) as the UI galleries draw WPF windows. Opt-in: OPMS_UI_DOC_SCREENSHOTS names the folder; skipped otherwise. No device,
// database or program. Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Threading;
using ImagingCapture.UI;
using ImagingDeviceManager.Capture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ImagingCapture.Tests {
	[TestClass]
	public class HowToCaptureScreenshots {
		[TestMethod]
		public void HowToScreenshots_XraySession() {
			string folder=Environment.GetEnvironmentVariable("OPMS_UI_DOC_SCREENSHOTS");
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: OPMS_UI_DOC_SCREENSHOTS names the folder for the how-to screenshots.");
			}
			Directory.CreateDirectory(folder);
			Exception exception=null;
			Thread thread=new Thread(() => {
				try {
					Draw(Path.Combine(folder,"howto-xray-session.png"));
				}
				catch(Exception ex) {
					exception=ex;
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
			if(exception!=null) {
				throw new AssertFailedException("The picture was not made: "+exception,exception);
			}
		}

		private static void Draw(string path) {
			CaptureControllerHarness harness=new CaptureControllerHarness(countSlots:4);
			harness.Setup.Patient=new Patient { PatNum=CaptureControllerHarness.PatNum,FName="Ava",LName="Example",PriProv=3,
				Birthdate=new DateTime(1984,3,14),Gender=PatientGender.Female };
			//The provider combo reads the provider cache: three fictional providers, no database.
			Providers.FillCacheFromTable(OpenPmsDentalBusiness.Crud.ProviderCrud.ListToTable(new System.Collections.Generic.List<Provider> {
				new Provider { ProvNum=3,Abbr="DOC",FName="Robin",LName="Sample",ItemOrder=0 },
				new Provider { ProvNum=4,Abbr="HYG",FName="Casey",LName="Demo",ItemOrder=1,IsSecondary=true },
			}));
			harness.Setup.ProvNumDefault=3;
			harness.Started();
			ExposeTooth(harness,0);
			ExposeTooth(harness,1);
			harness.Status(TwainCaptureState.WaitingForImage);//ready for slot 3
			FrmCaptureSession frmCaptureSession=new FrmCaptureSession(harness.Controller);
			Render(frmCaptureSession,path,() => harness.Dispatcher.RunPending());
			//The last image in a window of its own (a double-click on the session's preview): the preview's picture, rotation and caption.
			System.Windows.Controls.Image imagePreview=(System.Windows.Controls.Image)typeof(FrmCaptureSession).GetField("imagePreview",
				System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public).GetValue(frmCaptureSession);
			FrmCaptureImageWindow frmCaptureImageWindow=new FrmCaptureImageWindow();
			frmCaptureImageWindow.SetImage(imagePreview.Source as System.Windows.Media.Imaging.BitmapSource,imagePreview.LayoutTransform,"2 of 4: Max R, #4,5");
			Render(frmCaptureImageWindow,Path.Combine(Path.GetDirectoryName(path),"howto-xray-image-window.png"),() => { });
		}

		///<summary>Draws a Frm without a window (a hidden HwndSource, Measure, Arrange, RenderTargetBitmap), after raising its Load as FrmODBase's
		///WinForms frame raises it when the window opens (FormFrame_Load). pump runs what the session posted, between layout passes.</summary>
		private static void Render(System.Windows.FrameworkElement frm,string path,Action pump) {
			double width=frm.Width;
			double height=frm.Height;
			System.Windows.Interop.HwndSourceParameters hwndSourceParameters=new System.Windows.Interop.HwndSourceParameters("HowToFrm",
				(int)Math.Ceiling(width),(int)Math.Ceiling(height));
			hwndSourceParameters.WindowStyle=unchecked((int)0x80000000);//WS_POPUP without WS_VISIBLE: never shown
			System.Windows.Interop.HwndSource hwndSource=new System.Windows.Interop.HwndSource(hwndSourceParameters);
			try {
				hwndSource.RootVisual=frm;
				for(Type type=frm.GetType();type!=null;type=type.BaseType) {
					System.Reflection.FieldInfo fieldInfo=type.GetField("Load",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic
						|System.Reflection.BindingFlags.DeclaredOnly);
					if(fieldInfo!=null) {
						((EventHandler)fieldInfo.GetValue(frm))?.Invoke(frm,EventArgs.Empty);
						break;
					}
				}
				for(int i=0;i<3;i++) {
					pump();
					frm.Measure(new System.Windows.Size(width,height));
					frm.Arrange(new System.Windows.Rect(0,0,width,height));
					frm.UpdateLayout();
					System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(new Action(() => { }),System.Windows.Threading.DispatcherPriority.ContextIdle);
				}
				System.Windows.Media.Imaging.RenderTargetBitmap renderTargetBitmap=new System.Windows.Media.Imaging.RenderTargetBitmap((int)Math.Ceiling(width),
					(int)Math.Ceiling(height),96,96,System.Windows.Media.PixelFormats.Pbgra32);
				System.Windows.Media.DrawingVisual drawingVisual=new System.Windows.Media.DrawingVisual();
				using(System.Windows.Media.DrawingContext drawingContext=drawingVisual.RenderOpen()) {
					drawingContext.DrawRectangle(System.Windows.Media.Brushes.White,null,new System.Windows.Rect(0,0,width,height));
				}
				renderTargetBitmap.Render(drawingVisual);
				renderTargetBitmap.Render(frm);
				System.Windows.Media.Imaging.PngBitmapEncoder pngBitmapEncoder=new System.Windows.Media.Imaging.PngBitmapEncoder();
				pngBitmapEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(renderTargetBitmap));
				using(FileStream fileStream=File.Create(path)) {
					pngBitmapEncoder.Save(fileStream);
				}
			}
			finally {
				hwndSource.RootVisual=null;
				hwndSource.Dispose();
			}
		}

		///<summary>One exposure of a synthetic periapical-like picture: two bright teeth with roots on a dark background, slightly different per slot.</summary>
		private static void ExposeTooth(CaptureControllerHarness harness,int seed) {
			harness.Status(TwainCaptureState.WaitingForImage);
			int width=300;
			int height=400;
			AcquiredImage image=TestImages.Image8(width,height,(x,y) => {
				double value=40+y*0.05;
				for(int t=0;t<2;t++) {
					double cx=90+t*120+seed*6;
					double crownY=110;
					double dx=(x-cx)/55.0;
					double dyCrown=(y-crownY)/70.0;
					if(dx*dx+dyCrown*dyCrown<1) {
						value=Math.Max(value,200-dyCrown*20);
					}
					double rootHalf=30*(1-(y-crownY)/290.0);
					if(y>crownY && y<crownY+290 && Math.Abs(x-cx)<rootHalf) {
						value=Math.Max(value,160);
						if(Math.Abs(x-cx)<rootHalf*0.25) {
							value=110;//the canal
						}
					}
				}
				return (byte)Math.Min(255,value);
			});
			harness.Twain.RaiseImage(image,AcquiredImageKind.Normal,TwainCaptureStopReason.None,0);
			harness.Dispatcher.RunPending();
			harness.Ended(TwainCaptureEndReason.Completed);
		}
	}
}
