// The intraoral camera's setup windows for the how-to guide docs/how-to/set-up-the-intraoral-camera.md (docs/screenshots/README.md):
// Camera Picture Settings on a fake camera, and Learn's Other buttons step with buttons learned from scripted readings, drawn off screen
// (a hidden HwndSource, RenderTargetBitmap) as HowToCaptureScreenshots draws the X-ray session. Opt-in: OPMS_UI_DOC_SCREENSHOTS names the
// folder; skipped otherwise. No device, database or program. Licensed under the GNU General Public License, version 2 or later. See LICENSE
// in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using ImagingCapture.Camera;
using ImagingCapture.Settings;
using ImagingCapture.UI;
using ImagingCapture.Uvc;
using ImagingDeviceManager.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingCapture.Tests {
	[TestClass]
	public class HowToCameraScreenshots {
		private const BindingFlags FlagsField=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
		private static readonly ControlId _controlRoll=new ControlId(UvcControlInterface.CameraControl,2);
		private static readonly ControlId _controlBrightness=new ControlId(UvcControlInterface.VideoProcAmp,0);
		private static readonly ControlId _controlGain=new ControlId(UvcControlInterface.VideoProcAmp,9);
		//A fictional handpiece: its keys are active-low bits of Roll; bit 0 changes when it rests in its holder.
		private const int ROLL_IDLE=0x3F;
		private const int BIT_REST=0x1;
		private const int BIT_PHOTO=0x8;
		private const int BIT_ON_OFF=0x4;
		private const int BIT_UP=0x20;
		private const int BIT_NEXT=0x2;

		[TestMethod]
		public void HowToScreenshots_CameraSetup() {
			string folder=Environment.GetEnvironmentVariable("OPMS_UI_DOC_SCREENSHOTS");
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: OPMS_UI_DOC_SCREENSHOTS names the folder for the how-to screenshots.");
			}
			Directory.CreateDirectory(folder);
			string tempDir=Path.Combine(Path.GetTempPath(),"HowToCameraScreenshots_"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(tempDir);
			Exception exception=null;
			Thread thread=new Thread(() => {
				try {
					DrawPictureSettings(Path.Combine(folder,"howto-camera-picture-settings.png"),Path.Combine(tempDir,"capture.ini"));
					DrawLearnButtons(Path.Combine(folder,"howto-camera-learn-buttons.png"),Path.Combine(tempDir,"capture-learn.ini"));
				}
				catch(Exception ex) {
					exception=ex;
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
			try {
				Directory.Delete(tempDir,true);
			}
			catch {
				//Best effort.
			}
			if(exception!=null) {
				throw new AssertFailedException("The pictures were not made: "+exception,exception);
			}
		}

		private static FakeFrameSource MakeCamera() {
			FakeFrameSource fakeFrameSource=new FakeFrameSource(new FakeUvcBackend().AddDevice("Example intraoral camera"),new List<UvcMode>(),false);
			int capsBoth=UvcControls.FlagsAuto|UvcControls.FlagsManual;
			SetControl(fakeFrameSource,UvcControlInterface.VideoProcAmp,0,0,255,1,128,capsBoth,140);//Brightness
			SetControl(fakeFrameSource,UvcControlInterface.VideoProcAmp,1,0,100,1,32,UvcControls.FlagsManual,32);//Contrast
			SetControl(fakeFrameSource,UvcControlInterface.VideoProcAmp,3,0,100,1,64,UvcControls.FlagsManual,64);//Saturation
			SetControl(fakeFrameSource,UvcControlInterface.VideoProcAmp,4,0,7,1,3,UvcControls.FlagsManual,3);//Sharpness
			SetControl(fakeFrameSource,UvcControlInterface.VideoProcAmp,7,2800,6500,10,4600,capsBoth,4600);//White balance
			SetControl(fakeFrameSource,UvcControlInterface.VideoProcAmp,8,0,1,1,0,UvcControls.FlagsManual,0);//Backlight compensation
			SetControl(fakeFrameSource,UvcControlInterface.VideoProcAmp,9,0,255,1,0,UvcControls.FlagsManual,20);//Gain
			SetControl(fakeFrameSource,UvcControlInterface.VideoProcAmp,UvcControls.VideoProcAmpPowerLineFrequency,0,3,1,1,UvcControls.FlagsManual,2);
			SetControl(fakeFrameSource,UvcControlInterface.CameraControl,4,-11,0,1,-6,capsBoth,-6);//Exposure
			SetControl(fakeFrameSource,UvcControlInterface.CameraControl,UvcControls.CameraControlLowLightCompensation,0,1,1,0,UvcControls.FlagsManual,0);
			SetControl(fakeFrameSource,UvcControlInterface.CameraControl,2,0,255,1,ROLL_IDLE,UvcControls.FlagsManual,ROLL_IDLE);//Roll: the keys
			//White balance set by the camera itself.
			fakeFrameSource.WriteControl(UvcControlInterface.VideoProcAmp,7,4600,true);
			return fakeFrameSource;
		}

		private static void SetControl(FakeFrameSource fakeFrameSource,UvcControlInterface controlInterface,int property,int min,int max,int step,
			int defaultValue,int capsFlags,int value)
		{
			fakeFrameSource.SetControlRange(controlInterface,property,min,max,step,defaultValue,capsFlags);
			fakeFrameSource.SetControlValue(controlInterface,property,value);
		}

		///<summary>Camera Picture Settings: power line frequency saved, Exposure just changed to 1/125 s and selected.</summary>
		private static void DrawPictureSettings(string path,string pathIni) {
			File.WriteAllText(pathIni,"[Camera.Settings]\r\nPowerLineFrequency=60 Hz\r\n"
				+"[Trigger.ControlPoll]\r\nEnabled=1\r\nControl=Roll\r\nBits=3\r\nActiveLevel=Low\r\n");
			LocalCaptureConfig config=LocalCaptureConfig.Load(pathIni);
			CameraSettingsModel model=new CameraSettingsModel(MakeCamera(),config,CameraConfig.Read(config),"Example intraoral camera","EXAMPLE-PC",
				new FakeCaptureSettingsStore(),null);
			FrmCameraSettingsControls frm=new FrmCameraSettingsControls(model);
			Render(frm,path,() => {
				int idx=-1;
				for(int i=0;i<model.ListRows.Count;i++) {
					if(model.ListRows[i].Def.Id=="Exposure") {
						idx=i;
					}
				}
				string error;
				Assert.IsTrue(model.SetText(idx,"1/128 s",out error),error);
				Invoke(frm,"SelectRow",idx);
				Invoke(frm,"RefreshView");
			});
		}

		///<summary>Learn's Other buttons step after the baselines, the photo key and three buttons: on/off and brighter on Brightness, next slot;
		///"Setting: lower" picked for the next one.</summary>
		private static void DrawLearnButtons(string path,string pathIni) {
			File.WriteAllText(pathIni,"");
			FakeFrameSource fakeFrameSource=MakeCamera();
			LocalCaptureConfig config=LocalCaptureConfig.Load(pathIni);
			FrmCaptureLearn frm=new FrmCaptureLearn(fakeFrameSource,config,CaptureLearnMode.Learn);
			List<ImageCategoryItem> listCategories=new List<ImageCategoryItem> { new ImageCategoryItem(20,"Photos","",false) };
			ImageCategoryRule imageCategoryRule=new ImageCategoryRule(() => listCategories);
			CaptureLearnModel model=new CaptureLearnModel(CaptureLearnMode.Learn,CameraConfig.Read(config),imageCategoryRule,20,true,false,false);
			Dictionary<ControlId,int> dictValues=new Dictionary<ControlId,int> { { _controlRoll,ROLL_IDLE },{ _controlBrightness,140 },{ _controlGain,20 } };
			long timeMs=1000;
			Action<int> hold=ms => {
				for(int i=0;i<ms/50;i++) {
					timeMs+=50;
					model.OnSample(new ControlSample(timeMs,dictValues));
					model.Tick(timeMs);
				}
			};
			Action<int> toggle=bits => dictValues[_controlRoll]^=bits;
			Action<int> pressThree=bits => {
				Assert.IsTrue(model.StartRecording(timeMs),model.GetResultText());
				hold(300);
				for(int i=0;i<3;i++) {
					toggle(bits);
					hold(150);
					toggle(bits);
					hold(300);
				}
				hold(CaptureLearnModel.KeySettleMs+100);
			};
			model.SetControls(new[] { _controlRoll,_controlBrightness,_controlGain });
			Assert.IsTrue(model.StartRecording(timeMs));
			hold(LearnAnalyzer.BaselineMs+100);
			Assert.IsTrue(model.GoNext(),model.GetResultText());
			toggle(BIT_REST);
			Assert.IsTrue(model.StartRecording(timeMs));
			hold(LearnAnalyzer.BaselineMs+100);
			toggle(BIT_REST);
			hold(500);
			Assert.IsTrue(model.GoNext(),model.GetResultText());
			pressThree(BIT_PHOTO);
			Assert.IsTrue(model.GoNext(),model.GetResultText());
			Assert.AreEqual(LearnPage.Buttons,model.Page);
			Assert.IsTrue(model.SelectButtonAction(LevelAction.Toggle));
			Assert.IsTrue(model.SetSetting("Brightness"));
			Assert.IsTrue(model.SetHighest("200"));
			pressThree(BIT_ON_OFF);
			Assert.IsTrue(model.SelectButtonAction(LevelAction.Raise));
			Assert.IsTrue(model.SetSetting("Brightness"));
			Assert.IsTrue(model.SetHighest("200"));
			pressThree(BIT_UP);
			Assert.IsTrue(model.SelectButtonAction(LevelAction.NextSlot));
			pressThree(BIT_NEXT);
			Assert.AreEqual(3,model.Buttons.Count,model.GetResultText());
			Assert.IsTrue(model.SelectButtonAction(LevelAction.Lower));
			Assert.IsTrue(model.SetSetting("Brightness"));
			Assert.IsTrue(model.SetHighest("200"));
			//The window without its sampler and timer: the model above stands in for the camera's readings.
			typeof(FrmCaptureLearn).GetField("_captureLearnModel",FlagsField).SetValue(frm,model);
			typeof(FrmCaptureLearn).GetField("_imageCategoryRule",FlagsField).SetValue(frm,imageCategoryRule);
			typeof(FrmCaptureLearn).GetField("_learnSampler",FlagsField).SetValue(frm,new LearnSampler(fakeFrameSource,50,model.GetControlsToRead()));
			Invoke(frm,"FillComboCategory");
			Render(frm,path,() => Invoke(frm,"RefreshView"),isLoadRaised:false);
		}

		private static void Invoke(object target,string method,params object[] arrayArgs) {
			target.GetType().GetMethod(method,FlagsField).Invoke(target,arrayArgs);
		}

		///<summary>Draws a Frm without a window (a hidden HwndSource, Measure, Arrange, RenderTargetBitmap), after raising its Load as FrmODBase's
		///WinForms frame raises it when the window opens, then actionAfterLoad.</summary>
		private static void Render(System.Windows.FrameworkElement frm,string path,Action actionAfterLoad,bool isLoadRaised=true) {
			double width=frm.Width;
			double height=frm.Height;
			System.Windows.Interop.HwndSourceParameters hwndSourceParameters=new System.Windows.Interop.HwndSourceParameters("HowToFrm",
				(int)Math.Ceiling(width),(int)Math.Ceiling(height));
			hwndSourceParameters.WindowStyle=unchecked((int)0x80000000);//WS_POPUP without WS_VISIBLE: never shown
			System.Windows.Interop.HwndSource hwndSource=new System.Windows.Interop.HwndSource(hwndSourceParameters);
			try {
				hwndSource.RootVisual=frm;
				for(Type type=frm.GetType();type!=null && isLoadRaised;type=type.BaseType) {
					FieldInfo fieldInfo=type.GetField("Load",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.DeclaredOnly);
					if(fieldInfo!=null) {
						((EventHandler)fieldInfo.GetValue(frm))?.Invoke(frm,EventArgs.Empty);
						break;
					}
				}
				actionAfterLoad();
				for(int i=0;i<3;i++) {
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
	}
}
