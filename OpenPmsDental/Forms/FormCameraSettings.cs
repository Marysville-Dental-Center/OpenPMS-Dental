// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CodeBase;
using ImagingCapture.Camera;
using ImagingCapture.Photo;
using ImagingCapture.UI;
using ImagingCapture.Uvc;
using ImagingCapture.Video;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	///<summary>Camera settings of this computer (IMG-P4e; owner imaging design, camera and video, section 5), opened from the camera session, which
	///has the camera open: the camera and its picture size, where photos come from, Space in the main window, learning and testing the camera's
	///button, the photo and video settings of the session (capture.ini), and the category quick photos and videos go to (an office-wide
	///preference). It collects the choices; the session applies them after OK. Saving needs Setup, as the design says for camera settings.
	///Controls are built in code (no designer file).</summary>
	public class FormCameraSettings:FormODBase {
		#region Fields
		private readonly IReadOnlyList<UvcDeviceInfo> _listDevices;
		private readonly List<ResolutionChoice> _listResolutionChoices;
		private readonly List<long> _listCategoryDefNums=new List<long>();
		private readonly Action<CaptureLearnMode> _actionLearn;
		private UI.ComboBox comboCamera;
		private UI.ComboBox comboResolution;
		private UI.ComboBox comboPhotoSource;
		private UI.CheckBox checkMainWindowSpace;
		private UI.Button butLearn;
		private UI.Button butButtonTest;
		private Label labelTriggerNotes;
		private UI.ComboBox comboAfterCapture;
		private UI.CheckBox checkClick;
		private ValidNum textIdleMinutes;
		private ValidNum textLimitSeconds;
		private UI.ComboBox comboCategory;
		private UI.Button butOK;
		private UI.Button butCancel;
		#endregion Fields

		///<summary>The camera chosen, or null when it did not change.</summary>
		public UvcDeviceInfo DeviceSelected { get; private set; }
		///<summary>The picture size chosen, or null when it did not change.</summary>
		public UvcMode ModeSelected { get; private set; }
		public CameraPhotoSource PhotoSource { get; private set; }
		public bool IsMainWindowSpace { get; private set; }
		public PhotoSessionConfig PhotoSessionConfigNew { get; private set; }
		///<summary>The quick photo and video category (PrefName.VideoImageCategoryDefault; 0: the category selected in the module).</summary>
		public long CategoryDefNum { get; private set; }

		///<param name="listDevices">The cameras found. deviceCurrent: the one open, or null.</param>
		///<param name="listModes">The open camera's picture sizes (empty without a camera).</param>
		///<param name="actionLearn">Opens Learn or the button test on the session's camera (null: not available now).</param>
		public FormCameraSettings(IReadOnlyList<UvcDeviceInfo> listDevices,UvcDeviceInfo deviceCurrent,IEnumerable<UvcMode> listModes,bool hasStillPin,
			CameraConfig cameraConfig,PhotoSessionConfig photoSessionConfig,List<string> listTriggerNotes,long categoryDefNum,Action<CaptureLearnMode> actionLearn)
		{
			_listDevices=listDevices??new List<UvcDeviceInfo>();
			_listResolutionChoices=listModes==null ? new List<ResolutionChoice>() : ResolutionChoice.GetChoices(listModes,cameraConfig);
			_actionLearn=actionLearn;
			PhotoSessionConfigNew=(photoSessionConfig??new PhotoSessionConfig()).Copy();
			PhotoSource=cameraConfig?.PhotoSource??CameraPhotoSource.Preview;
			IsMainWindowSpace=cameraConfig?.MainWindowSpace??true;
			CategoryDefNum=categoryDefNum;
			BuildControls();
			Fill(deviceCurrent,hasStillPin,listTriggerNotes);
			InitializeLayoutManager();
			Lan.F(this);
		}

		#region Methods - Build
		private void BuildControls() {
			SuspendLayout();
			int xLabel=8;
			int widthLabel=190;
			int xField=xLabel+widthLabel+6;
			int widthField=320;
			int y=8;
			int pitch=27;
			Label AddLabel(string name,string text,int yRow) {
				Label label=new Label { Name=name,Text=text,Location=new Point(xLabel,yRow),Size=new Size(widthLabel,21),TextAlign=ContentAlignment.MiddleRight };
				Controls.Add(label);
				return label;
			}
			Label AddHeading(string name,string text,int yRow) {
				Label label=new Label { Name=name,Text=text,Location=new Point(xLabel,yRow),Size=new Size(widthLabel+6+widthField,20),
					TextAlign=ContentAlignment.BottomLeft,Font=UiTheme.FontTitle };
				Controls.Add(label);
				return label;
			}
			AddHeading("labelHeadingCamera","Camera",y);
			y+=24;
			AddLabel("labelCamera","Camera",y);
			comboCamera=new UI.ComboBox { Name="comboCamera",Location=new Point(xField,y),Size=new Size(widthField,21),TabIndex=1 };
			Controls.Add(comboCamera);
			y+=pitch;
			AddLabel("labelResolution","Picture size",y);
			comboResolution=new UI.ComboBox { Name="comboResolution",Location=new Point(xField,y),Size=new Size(widthField,21),TabIndex=2 };
			Controls.Add(comboResolution);
			y+=pitch;
			AddLabel("labelPhotoSource","Take photos from",y);
			comboPhotoSource=new UI.ComboBox { Name="comboPhotoSource",Location=new Point(xField,y),Size=new Size(widthField,21),TabIndex=3 };
			Controls.Add(comboPhotoSource);
			y+=pitch;
			checkMainWindowSpace=new UI.CheckBox { Name="checkMainWindowSpace",Text="Space in the main window takes a photo",Location=new Point(xField,y),
				Size=new Size(widthField,21),CheckAlign=ContentAlignment.MiddleLeft,TabIndex=4 };
			Controls.Add(checkMainWindowSpace);
			y+=pitch+6;
			AddHeading("labelHeadingButton","Camera button and pedal",y);
			y+=24;
			butLearn=new UI.Button { Name="butLearn",Text="Learn button...",Location=new Point(xField,y),Size=new Size(120,24),TabIndex=5 };
			butLearn.Click+=(sender,e) => _actionLearn?.Invoke(CaptureLearnMode.Learn);
			Controls.Add(butLearn);
			butButtonTest=new UI.Button { Name="butButtonTest",Text="Test button...",Location=new Point(xField+128,y),Size=new Size(110,24),TabIndex=6 };
			butButtonTest.Click+=(sender,e) => _actionLearn?.Invoke(CaptureLearnMode.ButtonTest);
			Controls.Add(butButtonTest);
			y+=pitch+2;
			labelTriggerNotes=new Label { Name="labelTriggerNotes",Text="",Location=new Point(xField,y),Size=new Size(widthField,48),ForeColor=UiTheme.TextSecondary };
			Controls.Add(labelTriggerNotes);
			y+=48+6;
			AddHeading("labelHeadingPhotos","Photos",y);
			y+=24;
			AddLabel("labelAfterCapture","After a press of Capture",y);
			comboAfterCapture=new UI.ComboBox { Name="comboAfterCapture",Location=new Point(xField,y),Size=new Size(widthField,21),TabIndex=7 };
			Controls.Add(comboAfterCapture);
			y+=pitch;
			checkClick=new UI.CheckBox { Name="checkClick",Text="A click on every photo",Location=new Point(xField,y),Size=new Size(widthField,21),
				CheckAlign=ContentAlignment.MiddleLeft,TabIndex=8 };
			Controls.Add(checkClick);
			y+=pitch;
			AddLabel("labelIdleMinutes","Release the camera after (minutes)",y);
			textIdleMinutes=new ValidNum { Name="textIdleMinutes",Location=new Point(xField,y),Size=new Size(60,20),MinVal=PhotoSessionConfig.MinutesIdleMin,
				MaxVal=PhotoSessionConfig.MinutesIdleMax,TabIndex=9 };
			Controls.Add(textIdleMinutes);
			y+=pitch+6;
			AddHeading("labelHeadingVideo","Video",y);
			y+=24;
			AddLabel("labelLimitSeconds","Longest video (seconds)",y);
			textLimitSeconds=new ValidNum { Name="textLimitSeconds",Location=new Point(xField,y),Size=new Size(60,20),MinVal=PhotoSessionConfig.LimitSecondsMin,
				MaxVal=PhotoSessionConfig.LimitSecondsMax,TabIndex=10 };
			Controls.Add(textLimitSeconds);
			y+=pitch;
			Label labelSound=new Label { Name="labelSound",Text="Videos are recorded without sound.",Location=new Point(xField,y),Size=new Size(widthField,21),
				ForeColor=UiTheme.TextSecondary,TextAlign=ContentAlignment.MiddleLeft };
			Controls.Add(labelSound);
			y+=pitch+6;
			AddHeading("labelHeadingCategories","Categories (all computers)",y);
			y+=24;
			AddLabel("labelCategory","Quick photos and videos",y);
			comboCategory=new UI.ComboBox { Name="comboCategory",Location=new Point(xField,y),Size=new Size(widthField,21),TabIndex=11 };
			Controls.Add(comboCategory);
			y+=pitch;
			Label labelCategoryNote=new Label { Name="labelCategoryNote",Text="Photo mounts save into their mount's category.",Location=new Point(xField,y),
				Size=new Size(widthField,21),ForeColor=UiTheme.TextSecondary,TextAlign=ContentAlignment.MiddleLeft };
			Controls.Add(labelCategoryNote);
			y+=pitch+10;
			int xRight=xField+widthField;
			butCancel=new UI.Button { Name="butCancel",Text="Cancel",Location=new Point(xRight-75,y),Size=new Size(75,26),TabIndex=21 };
			butCancel.Click+=(sender,e) => { DialogResult=DialogResult.Cancel; };
			Controls.Add(butCancel);
			butOK=new UI.Button { Name="butOK",Text="OK",Location=new Point(xRight-75-8-75,y),Size=new Size(75,26),TabIndex=20 };
			butOK.Click+=butOK_Click;
			Controls.Add(butOK);
			ClientSize=new Size(xRight+8,y+26+8);
			Name="FormCameraSettings";
			Text="Camera Settings";
			StartPosition=FormStartPosition.CenterParent;
			ResumeLayout(false);
		}

		private void Fill(UvcDeviceInfo deviceCurrent,bool hasStillPin,List<string> listTriggerNotes) {
			List<string> listNames=VideoCaptureSession.GetDeviceNames(_listDevices);
			for(int i=0;i<listNames.Count;i++) {
				comboCamera.Items.Add(listNames[i]);
				if(deviceCurrent!=null && _listDevices[i].IsSameDevice(deviceCurrent)) {
					comboCamera.SelectedIndex=i;
				}
			}
			if(_listDevices.Count==0) {
				comboCamera.Items.Add(Lan.g(this,"(no camera found)"));
				comboCamera.SelectedIndex=0;
			}
			comboCamera.Enabled=_listDevices.Count>0;
			for(int i=0;i<_listResolutionChoices.Count;i++) {
				comboResolution.Items.Add(_listResolutionChoices[i].Text);
				if(_listResolutionChoices[i].IsSelected) {
					comboResolution.SelectedIndex=i;
				}
			}
			if(_listResolutionChoices.Count==0) {
				comboResolution.Items.Add(Lan.g(this,"(needs the camera open)"));
				comboResolution.SelectedIndex=0;
			}
			comboResolution.Enabled=_listResolutionChoices.Count>0;
			comboPhotoSource.Items.Add(Lan.g(this,"The preview picture"));
			comboPhotoSource.Items.Add(Lan.g(this,"The camera's still image (still-pin buttons)"));
			comboPhotoSource.SelectedIndex=PhotoSource==CameraPhotoSource.Still && hasStillPin ? 1 : 0;
			comboPhotoSource.Enabled=hasStillPin;
			checkMainWindowSpace.Checked=IsMainWindowSpace;
			butLearn.Enabled=_actionLearn!=null;
			butButtonTest.Enabled=_actionLearn!=null;
			labelTriggerNotes.Text=string.Join("\r\n",listTriggerNotes??new List<string>());
			comboAfterCapture.Items.Add(Lan.g(this,"Save the photo at once"));
			comboAfterCapture.Items.Add(Lan.g(this,"Freeze first, save on the second press"));
			comboAfterCapture.SelectedIndex=PhotoSessionConfigNew.IsFreezeFirst ? 1 : 0;
			checkClick.Checked=PhotoSessionConfigNew.IsClickOn;
			textIdleMinutes.Value=PhotoSessionConfigNew.MinutesIdle;
			textLimitSeconds.Value=PhotoSessionConfigNew.VideoLimitSeconds;
			comboCategory.Items.Add(Lan.g(this,"(the category selected in the module)"));
			_listCategoryDefNums.Add(0);
			List<Def> listDefs=new List<Def>();
			try {
				listDefs=Defs.GetDefsForCategory(DefCat.ImageCats,isShort:true);
			}
			catch(Exception ex) {
				ex.DoNothing();//no categories to offer: the current one stays
			}
			foreach(Def def in listDefs) {
				comboCategory.Items.Add(def.ItemName);
				_listCategoryDefNums.Add(def.DefNum);
			}
			int idxCategory=_listCategoryDefNums.IndexOf(CategoryDefNum);
			comboCategory.SelectedIndex=idxCategory>=0 ? idxCategory : 0;
		}
		#endregion Methods - Build

		private void butOK_Click(object sender,EventArgs e) {
			if(!textIdleMinutes.IsValid() || !textLimitSeconds.IsValid()) {
				MsgBox.Show(this,"Please fix data entry errors first.");
				return;
			}
			if(!Security.IsAuthorized(EnumPermType.Setup)) {
				return;
			}
			int idxCamera=comboCamera.SelectedIndex;
			DeviceSelected=idxCamera>=0 && idxCamera<_listDevices.Count ? _listDevices[idxCamera] : null;
			int idxResolution=comboResolution.SelectedIndex;
			ModeSelected=idxResolution>=0 && idxResolution<_listResolutionChoices.Count && !_listResolutionChoices[idxResolution].IsSelected
				? _listResolutionChoices[idxResolution].Mode : null;
			PhotoSource=comboPhotoSource.SelectedIndex==1 ? CameraPhotoSource.Still : CameraPhotoSource.Preview;
			IsMainWindowSpace=checkMainWindowSpace.Checked;
			PhotoSessionConfigNew.IsFreezeFirst=comboAfterCapture.SelectedIndex==1;
			PhotoSessionConfigNew.IsClickOn=checkClick.Checked;
			PhotoSessionConfigNew.MinutesIdle=textIdleMinutes.Value;
			PhotoSessionConfigNew.VideoLimitSeconds=textLimitSeconds.Value;
			int idxCategory=comboCategory.SelectedIndex;
			CategoryDefNum=idxCategory>=0 && idxCategory<_listCategoryDefNums.Count ? _listCategoryDefNums[idxCategory] : 0;
			DialogResult=DialogResult.OK;
		}
	}
}
