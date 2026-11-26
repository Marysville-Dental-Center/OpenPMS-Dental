// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImagingCapture.Camera;
using ImagingCapture.Settings;
using ImagingCapture.Uvc;
using ImagingCapture.Uvc.Interop;
using ImagingDeviceManager.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ImagingCapture.Tests {
	///<summary>The readable camera settings: the catalog's texts, the kernel-streaming buffers behind power line frequency and low light
	///compensation, and the Camera Picture Settings window's model.</summary>
	[TestClass]
	public class CameraSettingsTests {
		private const int CAPS_BOTH=UvcControls.FlagsAuto|UvcControls.FlagsManual;
		private string _tempDir;
		private string _path;
		private FakeCaptureSettingsStore _store;
		private FakeFrameSource _fakeFrameSource;
		private List<string> _listLog;

		[TestInitialize]
		public void Init() {
			_tempDir=Path.Combine(Path.GetTempPath(),"CameraSettingsTests_"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(_tempDir);
			_path=Path.Combine(_tempDir,"capture.ini");
			_store=new FakeCaptureSettingsStore();
			_fakeFrameSource=new FakeFrameSource(new FakeUvcBackend().AddDevice("Test camera"),new List<UvcMode>(),false);
			_listLog=new List<string>();
		}

		[TestCleanup]
		public void Cleanup() {
			try {
				Directory.Delete(_tempDir,true);
			}
			catch {
				//Best effort.
			}
		}

		private static UvcControlRange Range(int min,int max,int step,int defaultValue,int capsFlags=UvcControls.FlagsManual) {
			return new UvcControlRange(UvcControlStatus.Ok,min,max,step,defaultValue,capsFlags,0);
		}

		private static CameraSettingDef Def(string id) {
			CameraSettingDef cameraSettingDef;
			Assert.IsTrue(CameraSettingCatalog.TryGet(id,out cameraSettingDef),id);
			return cameraSettingDef;
		}

		private static void AssertRoundTrip(string id,UvcControlRange controlRange,string text,int valueExpected,bool isAutoExpected=false) {
			int value;
			bool isAuto;
			string error;
			Assert.IsTrue(CameraSettingCatalog.TryParse(Def(id),controlRange,text,out value,out isAuto,out error),id+" "+text+": "+error);
			Assert.AreEqual(isAutoExpected,isAuto,id+" "+text);
			if(!isAutoExpected) {
				Assert.AreEqual(valueExpected,value,id+" "+text);
			}
			Assert.AreEqual(text,CameraSettingCatalog.Format(Def(id),controlRange,value,isAuto),id+" back to text");
		}

		private static string ParseError(string id,UvcControlRange controlRange,string text) {
			int value;
			bool isAuto;
			string error;
			Assert.IsFalse(CameraSettingCatalog.TryParse(Def(id),controlRange,text,out value,out isAuto,out error),id+" "+text);
			return error;
		}

		#region Catalog
		[TestMethod]
		public void Catalog_IdsAreUniqueAndNamesRead() {
			List<string> listIds=CameraSettingCatalog.All.Select(x => x.Id).ToList();
			Assert.AreEqual(listIds.Count,listIds.Distinct().Count(),string.Join(",",listIds));
			Assert.AreEqual(19,CameraSettingCatalog.All.Count,"Every property of both interfaces, plus power line frequency and low light compensation.");
			Assert.AreEqual("PowerLineFrequency",Def("power line frequency").Id);
			Assert.AreEqual("Gain",Def("GAIN").Id);
			Assert.AreEqual("Iris (aperture)",Def("Iris").DisplayName);
			CameraSettingDef cameraSettingDef;
			Assert.IsFalse(CameraSettingCatalog.TryGet("",out cameraSettingDef));
			Assert.IsFalse(CameraSettingCatalog.TryGet("Shine",out cameraSettingDef));
		}

		[TestMethod]
		public void Catalog_Exposure_ReadsAsSeconds() {
			UvcControlRange controlRange=Range(-11,0,1,-6,CAPS_BOTH);
			AssertRoundTrip("Exposure",controlRange,"1/64 s",-6);
			AssertRoundTrip("Exposure",controlRange,"1/2048 s",-11);
			AssertRoundTrip("Exposure",controlRange,"1 s",0);
			AssertRoundTrip("Exposure",controlRange,"Auto",0,true);
			int value;
			bool isAuto;
			string error;
			Assert.IsTrue(CameraSettingCatalog.TryParse(Def("Exposure"),controlRange,"0.5s",out value,out isAuto,out error),error);
			Assert.AreEqual(-1,value);
			Assert.IsTrue(CameraSettingCatalog.TryParse(Def("Exposure"),controlRange,"1/60",out value,out isAuto,out error),error);
			Assert.AreEqual(-6,value,"Rounded to the nearest power of 2.");
			Assert.AreEqual("1/2048 s to 1 s, or Auto",CameraSettingCatalog.FormatRange(Def("Exposure"),controlRange));
			StringAssert.Contains(ParseError("Exposure",controlRange,"4 s"),"outside what this camera allows");
			StringAssert.Contains(ParseError("Exposure",Range(-11,0,1,-6),"Auto"),"cannot be set to Auto on this camera");
			StringAssert.Contains(ParseError("Exposure",controlRange,"fast"),"is not a value of Exposure");
		}

		[TestMethod]
		public void Catalog_UnitsChoicesAndOnOff() {
			AssertRoundTrip("Iris",Range(14,160,1,28),"f/2.8",28);
			AssertRoundTrip("Iris",Range(14,160,1,28),"f/16",160);
			AssertRoundTrip("WhiteBalance",Range(2800,6500,10,4600,CAPS_BOTH),"4600 K",4600);
			AssertRoundTrip("Hue",Range(-180,180,1,0),"15°",15);
			Assert.AreEqual("15",CameraSettingCatalog.Format(Def("Hue"),Range(-180,180,1,0),15,false,forFile:true),"The file stays plain ASCII.");
			AssertRoundTrip("Gain",Range(0,255,1,0),"20",20);
			//Power line frequency: a choice, its Auto a choice of its own.
			UvcControlRange controlRangePower=Range(0,3,1,1);
			AssertRoundTrip("PowerLineFrequency",controlRangePower,"60 Hz",2);
			AssertRoundTrip("PowerLineFrequency",controlRangePower,"Off",0);
			AssertRoundTrip("PowerLineFrequency",controlRangePower,"Auto",3);
			CollectionAssert.AreEqual(new[] { "Off","50 Hz","60 Hz" },CameraSettingCatalog.GetChoices(Def("PowerLineFrequency"),Range(0,2,1,1)).ToArray(),
				"Only the choices within the camera's range.");
			Assert.AreEqual("Off, 50 Hz or 60 Hz",CameraSettingCatalog.FormatRange(Def("PowerLineFrequency"),Range(0,2,1,1)));
			//On/Off, and backlight compensation reads On/Off only when its range is 0 to 1.
			AssertRoundTrip("LowLightCompensation",Range(0,1,1,0),"On",1);
			AssertRoundTrip("ColorEnable",Range(0,1,1,1),"Off",0);
			AssertRoundTrip("BacklightCompensation",Range(0,1,1,0),"On",1);
			AssertRoundTrip("BacklightCompensation",Range(0,3,1,0),"2",2);
			int value;
			bool isAuto;
			string error;
			Assert.IsTrue(CameraSettingCatalog.TryParse(Def("ColorEnable"),Range(0,1,1,1),"1",out value,out isAuto,out error),error);
			Assert.AreEqual(1,value);
			Assert.IsTrue(CameraSettingCatalog.TryParse(Def("Gain"),Range(0,255,1,0),"  20 ",out value,out isAuto,out error),error);
			StringAssert.Contains(ParseError("Gain",Range(0,255,1,0),"300"),"0 to 255");
			Assert.AreEqual("No value.",ParseError("Gain",Range(0,255,1,0)," "));
		}

		[TestMethod]
		public void Catalog_NoHexAnywhere() {
			foreach(CameraSettingDef cameraSettingDef in CameraSettingCatalog.All) {
				UvcControlRange controlRange=Range(0,255,1,0,CAPS_BOTH);
				string text=CameraSettingCatalog.Format(cameraSettingDef,controlRange,200,false)+" "+CameraSettingCatalog.FormatRange(cameraSettingDef,controlRange);
				Assert.IsFalse(text.Contains("0x"),cameraSettingDef.Id+": "+text);
			}
		}
		#endregion Catalog

		#region Kernel streaming buffers
		[TestMethod]
		public void KsControl_RequestLayout() {
			byte[] buffer=KsControlProperty.BuildRequest(KsControlProperty.PropSetVideoProcAmp,13,KsControlProperty.KSPROPERTY_TYPE_SET,2,UvcControls.FlagsManual);
			Assert.AreEqual(36,buffer.Length,"KSPROPERTY_VIDEOPROCAMP_S.");
			Assert.AreEqual(KsControlProperty.PropSetVideoProcAmp,new Guid(buffer.Take(16).ToArray()));
			Assert.AreEqual(13,KsControlProperty.ReadInt(buffer,16));
			Assert.AreEqual(KsControlProperty.KSPROPERTY_TYPE_SET,KsControlProperty.ReadInt(buffer,20));
			Assert.AreEqual(2,KsControlProperty.ReadInt(buffer,24));
			Assert.AreEqual(UvcControls.FlagsManual,KsControlProperty.ReadInt(buffer,28));
			Assert.AreEqual(KsControlProperty.PropSetCameraControl,KsControlProperty.GetPropSet(UvcControlInterface.CameraControl));
			Assert.AreEqual(new Guid("C6E13360-30AC-11D0-A18C-00A0C9118956"),typeof(IAMVideoProcAmp).GUID,"The property set is IAMVideoProcAmp's IID.");
		}

		[TestMethod]
		public void KsControl_ParseValue() {
			byte[] buffer=KsControlProperty.BuildRequest(KsControlProperty.PropSetVideoProcAmp,13,KsControlProperty.KSPROPERTY_TYPE_GET,1,UvcControls.FlagsManual);
			KsControlProperty.WriteInt(buffer,32,UvcControls.FlagsManual);
			int value;
			int flags;
			int capabilities;
			Assert.IsTrue(KsControlProperty.TryParseValue(buffer,36,out value,out flags,out capabilities));
			Assert.AreEqual(1,value);
			Assert.AreEqual(UvcControls.FlagsManual,flags);
			Assert.AreEqual(UvcControls.FlagsManual,capabilities);
			Assert.IsFalse(KsControlProperty.TryParseValue(buffer,20,out value,out flags,out capabilities),"A short answer.");
			Assert.IsFalse(KsControlProperty.TryParseValue(null,36,out value,out flags,out capabilities));
		}

		///<summary>A KSPROPERTY_DESCRIPTION (40 bytes; MembersListCount at 32) followed by one KSPROPERTY_MEMBERSHEADER (16 bytes) and its members.</summary>
		private static byte[] BuildDescription(int membersFlags,int membersSize,params int[] arrayMembers) {
			byte[] buffer=new byte[KsControlProperty.SizeSupportBuffer];
			KsControlProperty.WriteInt(buffer,32,1);
			KsControlProperty.WriteInt(buffer,40,membersFlags);
			KsControlProperty.WriteInt(buffer,44,membersSize);
			KsControlProperty.WriteInt(buffer,48,1);
			for(int i=0;i<arrayMembers.Length;i++) {
				KsControlProperty.WriteInt(buffer,56+4*i,arrayMembers[i]);
			}
			return buffer;
		}

		[TestMethod]
		public void KsControl_ParseRangeAndDefault() {
			int min;
			int max;
			int step;
			//Stepped range: SteppingDelta, Reserved, Bounds.Min, Bounds.Max.
			byte[] buffer=BuildDescription(KsControlProperty.KSPROPERTY_MEMBER_STEPPEDRANGES,16,1,0,0,2);
			Assert.IsTrue(KsControlProperty.TryParseRange(buffer,72,out min,out max,out step));
			Assert.AreEqual(0,min);
			Assert.AreEqual(2,max);
			Assert.AreEqual(1,step);
			buffer=BuildDescription(KsControlProperty.KSPROPERTY_MEMBER_RANGES,8,0,1);
			Assert.IsTrue(KsControlProperty.TryParseRange(buffer,64,out min,out max,out step));
			Assert.AreEqual(1,max);
			Assert.IsFalse(KsControlProperty.TryParseRange(buffer,30,out min,out max,out step),"Shorter than a description.");
			Assert.IsFalse(KsControlProperty.TryParseRange(BuildDescription(KsControlProperty.KSPROPERTY_MEMBER_RANGES,8,5,1),64,out min,out max,out step),
				"Min above max.");
			Assert.IsFalse(KsControlProperty.TryParseRange(BuildDescription(KsControlProperty.KSPROPERTY_MEMBER_RANGES,-4,0,1),64,out min,out max,out step),
				"A negative member size.");
			int defaultValue;
			buffer=BuildDescription(KsControlProperty.KSPROPERTY_MEMBER_VALUES,4,2);
			Assert.IsTrue(KsControlProperty.TryParseDefault(buffer,60,out defaultValue));
			Assert.AreEqual(2,defaultValue);
			Assert.IsFalse(KsControlProperty.TryParseDefault(BuildDescription(KsControlProperty.KSPROPERTY_MEMBER_RANGES,8,0,1),64,out defaultValue));
			KsControlProperty.GetFallbackRange(UvcControlInterface.VideoProcAmp,UvcControls.VideoProcAmpPowerLineFrequency,out min,out max);
			Assert.AreEqual(2,max,"Off, 50 Hz, 60 Hz.");
			KsControlProperty.GetFallbackRange(UvcControlInterface.CameraControl,UvcControls.CameraControlLowLightCompensation,out min,out max);
			Assert.AreEqual(1,max);
		}
		#endregion Kernel streaming buffers

		#region Settings window model
		///<summary>Gain 64 (0-255, default 32), Exposure 1/8 s with Auto, power line 50 Hz, and Roll, read for the buttons.</summary>
		private CameraSettingsModel CreateModel(string ini) {
			File.WriteAllText(_path,ini);
			_fakeFrameSource.SetControlRange(UvcControlInterface.VideoProcAmp,9,0,255,1,32,UvcControls.FlagsManual);
			_fakeFrameSource.SetControlValue(UvcControlInterface.VideoProcAmp,9,64);
			_fakeFrameSource.SetControlRange(UvcControlInterface.CameraControl,4,-11,0,1,-6,CAPS_BOTH);
			_fakeFrameSource.SetControlValue(UvcControlInterface.CameraControl,4,-3);
			_fakeFrameSource.SetControlRange(UvcControlInterface.VideoProcAmp,UvcControls.VideoProcAmpPowerLineFrequency,0,2,1,1,UvcControls.FlagsManual);
			_fakeFrameSource.SetControlValue(UvcControlInterface.VideoProcAmp,UvcControls.VideoProcAmpPowerLineFrequency,1);
			_fakeFrameSource.SetControlValue(UvcControlInterface.CameraControl,2,0x3F);
			LocalCaptureConfig config=LocalCaptureConfig.Load(_path);
			CameraSettingsModel model=new CameraSettingsModel(_fakeFrameSource,config,CameraConfig.Read(config),"Test camera","PC-1",_store,_listLog.Add);
			model.Load();
			return model;
		}

		private int IndexOf(CameraSettingsModel model,string id) {
			int idx=model.ListRows.ToList().FindIndex(x => x.Def.Id==id);
			Assert.IsTrue(idx>=0,id+" is listed.");
			return idx;
		}

		[TestMethod]
		public void Model_ListsWhatTheCameraOffers_ReadableAndInCatalogOrder() {
			CameraSettingsModel model=CreateModel("[Camera.Settings]\r\nGain=20\r\n[Trigger.ControlPoll]\r\nEnabled=1\r\nControl=Roll\r\nBits=3\r\nActiveLevel=Low\r\n");
			CollectionAssert.AreEqual(new[] { "Gain","PowerLineFrequency","Exposure","Roll" },model.ListRows.Select(x => x.Def.Id).ToArray());
			CameraSettingRow rowGain=model.ListRows[IndexOf(model,"Gain")];
			Assert.AreEqual("64",rowGain.Text);
			Assert.AreEqual("32",rowGain.DefaultText);
			Assert.AreEqual("0 to 255",rowGain.RangeText);
			Assert.AreEqual("20",rowGain.SavedText,"What the file holds, not yet applied: the window shows both.");
			Assert.AreEqual("1/8 s",model.ListRows[IndexOf(model,"Exposure")].Text);
			Assert.AreEqual("50 Hz",model.ListRows[IndexOf(model,"PowerLineFrequency")].Text);
			Assert.IsTrue(model.ListRows[IndexOf(model,"Roll")].IsButtonControl);
			Assert.IsFalse(model.HasChanges);
		}

		[TestMethod]
		public void Model_EditsGoToTheCameraAtOnce_AndUndoPutsThemBack() {
			CameraSettingsModel model=CreateModel("");
			string error;
			Assert.IsTrue(model.SetText(IndexOf(model,"Gain"),"20",out error),error);
			Assert.IsTrue(model.SetText(IndexOf(model,"Exposure"),"Auto",out error),error);
			Assert.IsTrue(model.SetText(IndexOf(model,"PowerLineFrequency"),"60 hz",out error),error);
			Assert.IsTrue(model.SetCameraDefault(IndexOf(model,"Gain"),out error),error);
			CollectionAssert.AreEqual(new[] { "Gain=20","Exposure=Auto","PowerLineFrequency=2","Gain=32" },_fakeFrameSource.ListWrites.Select(x => x.ToString()).ToArray());
			Assert.IsTrue(model.HasChanges);
			StringAssert.Contains(string.Join("\n",_listLog),"Camera setting Exposure (window): 1/8 s -> Auto.");
			StringAssert.Contains(string.Join("\n",_listLog),"Camera setting Gain (camera default): 20 -> 32.");
			Assert.IsFalse(model.SetText(IndexOf(model,"Gain"),"300",out error));
			StringAssert.Contains(error,"outside what this camera allows");
			Assert.AreEqual(4,_fakeFrameSource.ListWrites.Count,"A value the window refuses is not written.");
			List<string> listErrors=model.UndoChanges();
			Assert.AreEqual(0,listErrors.Count,string.Join(" ",listErrors));
			CollectionAssert.AreEqual(new[] { "Gain=64","PowerLineFrequency=1","Exposure=-3" },_fakeFrameSource.ListWrites.Skip(4).Select(x => x.ToString()).ToArray());
			Assert.IsFalse(model.HasChanges);
			Assert.AreEqual("",File.ReadAllText(_path),"Nothing saved.");
		}

		[TestMethod]
		public void Model_ButtonControlIsReadOnly() {
			CameraSettingsModel model=CreateModel("[Level.1]\r\nEnabled=1\r\nControl=Roll\r\nBits=0\r\nActiveLevel=Low\r\nAction=PausePreview\r\n");
			string error;
			Assert.IsFalse(model.SetText(IndexOf(model,"Roll"),"5",out error));
			StringAssert.Contains(error,"The camera's buttons are read from Roll");
			Assert.AreEqual(0,_fakeFrameSource.ListWrites.Count);
		}

		[TestMethod]
		public void Model_CameraRefusal_IsAnErrorAndLogged() {
			CameraSettingsModel model=CreateModel("");
			_fakeFrameSource.FuncWriteStatus=(controlInterface,property,value) => property==9 ? UvcControlStatus.Failed : UvcControlStatus.Ok;
			string error;
			Assert.IsFalse(model.SetText(IndexOf(model,"Gain"),"20",out error));
			StringAssert.Contains(error,"The camera refused it (Failed)");
			Assert.AreEqual("64",model.ListRows[IndexOf(model,"Gain")].Text,"The value read back.");
			Assert.IsFalse(model.HasChanges);
			StringAssert.Contains(string.Join("\n",_listLog),"Camera setting Gain (window): 64 -> 20");
		}

		[TestMethod]
		public void Model_Save_WritesReadableValues_ChecksSetup_AndLogs() {
			CameraSettingsModel model=CreateModel("[Camera.Settings]\r\nPowerLineFrequency=50 Hz\r\nBrightness=40\r\n");
			string error;
			Assert.IsTrue(model.SetText(IndexOf(model,"Gain"),"20",out error),error);
			Assert.IsTrue(model.SetText(IndexOf(model,"Exposure"),"1/64 s",out error),error);
			Assert.IsTrue(model.SetForget(IndexOf(model,"PowerLineFrequency"),true));
			Assert.IsFalse(model.SetForget(IndexOf(model,"Roll"),true),"Nothing saved to forget.");
			List<CaptureSettingChange> listChanges=model.GetPendingChanges();
			CollectionAssert.AreEqual(new[] { "[Camera.Settings] Gain: (empty) -> 20","[Camera.Settings] PowerLineFrequency: 50 Hz -> (empty)",
				"[Camera.Settings] Exposure: (empty) -> 1/64 s" },listChanges.Select(x => x.GetText()).ToArray());
			//Without Setup nothing is saved.
			_store.IsAuthorizedResult=false;
			CameraSettingsSaveResult result=model.Save();
			Assert.IsFalse(result.IsSaved);
			StringAssert.Contains(result.Error,"Not authorized for Setup");
			Assert.IsFalse(File.ReadAllText(_path).Contains("Gain"));
			Assert.AreEqual(0,_store.ListLogs.Count);
			//With it, the file gets the readable values and the security log one Setup entry.
			_store.IsAuthorizedResult=true;
			result=model.Save();
			Assert.IsTrue(result.IsSaved,result.Error);
			Assert.IsTrue(result.IsLogged,result.Error);
			CollectionAssert.Contains(_store.ListPermsChecked,EnumPermType.Setup);
			string text=File.ReadAllText(_path);
			StringAssert.Contains(text,"Gain=20");
			StringAssert.Contains(text,"Exposure=1/64 s");
			StringAssert.Contains(text,"Brightness=40","A value of a setting the camera does not offer stays.");
			Assert.IsFalse(text.Contains("PowerLineFrequency=50"),text);
			Assert.AreEqual(1,_store.ListLogs.Count);
			Assert.AreEqual(EnumPermType.Setup,_store.ListLogs[0].PermType);
			Assert.AreEqual("Camera settings saved: Computer=PC-1; Camera=Test camera; File="+_path
				+"; [Camera.Settings] Gain: (empty) -> 20; [Camera.Settings] PowerLineFrequency: 50 Hz -> (empty); [Camera.Settings] Exposure: (empty) -> 1/64 s",
				_store.ListLogs[0].LogText);
			Assert.IsFalse(model.HasChanges,"What was saved is what Close keeps.");
			Assert.AreEqual(0,model.UndoChanges().Count);
			Assert.AreEqual(2,_fakeFrameSource.ListWrites.Count,"Undo after Save writes nothing.");
			//Saving again with no change logs nothing.
			result=model.Save();
			Assert.IsTrue(result.IsSaved);
			Assert.AreEqual(1,_store.ListLogs.Count);
		}
		#endregion Settings window model
	}
}
