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
		private readonly Func<CaptureLearnMode,List<string>> _funcLearn;
		///<summary>The photo source as saved: kept when the camera is not open to choose another.</summary>
		private readonly CameraPhotoSource _photoSourceSaved;
		private int _idxCameraOpen=-1;
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
		///<summary>The picture size chosen when IsModeChanged; null is Automatic.</summary>
		public UvcMode ModeSelected { get; private set; }
		///<summary>True when another picture size (or Automatic) was chosen for the open camera.</summary>
		public bool IsModeChanged { get; private set; }
		public CameraPhotoSource PhotoSource { get; private set; }
		public bool IsMainWindowSpace { get; private set; }
		public PhotoSessionConfig PhotoSessionConfigNew { get; private set; }
		///<summary>The quick photo and video category (PrefName.VideoImageCategoryDefault; 0: the category selected in the module).</summary>
		public long CategoryDefNum { get; private set; }

		///<param name="listDevices">The cameras found. deviceCurrent: the one open, or null.</param>
		///<param name="listModes">The open camera's picture sizes (empty without a camera).</param>
		///<param name="funcLearn">Opens Learn or the button test on the session's camera and returns the trigger notes after it (null: not available
		///now).</param>
		public FormCameraSettings(IReadOnlyList<UvcDeviceInfo> listDevices,UvcDeviceInfo deviceCurrent,IEnumerable<UvcMode> listModes,bool hasStillPin,
			CameraConfig cameraConfig,PhotoSessionConfig photoSessionConfig,List<string> listTriggerNotes,long categoryDefNum,
			Func<CaptureLearnMode,List<string>> funcLearn)
		{
			_listDevices=listDevices??new List<UvcDeviceInfo>();
			_listResolutionChoices=listModes==null ? new List<ResolutionChoice>() : ResolutionChoice.GetChoices(listModes,cameraConfig);
			_funcLearn=funcLearn;
			PhotoSessionConfigNew=(photoSessionConfig??new PhotoSessionConfig()).Copy();
			PhotoSource=cameraConfig?.PhotoSource??CameraPhotoSource.Preview;
			_photoSourceSaved=PhotoSource;
			IsMainWindowSpace=cameraConfig?.MainWindowSpace??true;
			CategoryDefNum=categoryDefNum;
			BuildControls(listTriggerNotes);
			Fill(deviceCurrent,hasStillPin,listTriggerNotes);
			InitializeLayoutManager();
			Lan.F(this);
		}

		#region Methods - Build
		private void BuildControls(List<string> listTriggerNotes) {
			SuspendLayout();
			//UI-12 slice 14: on the layout grid (UiGrid): label + field rows one pitch apart, the fields and labels the field height, a check box
			//row the check box height, each section a section gap under the one before with its heading a row gap over its rows, the bar a
			//section gap under the last row.
			int xLabel=UiGrid.PAD;
			int widthLabel=192;
			int xField=xLabel+widthLabel+UiGrid.LABEL_GAP;
			int widthField=320;
			int y=UiGrid.PAD;
			int pitch=UiGrid.PITCH;
			int pitchCheck=UiGrid.CHECK_HEIGHT+UiGrid.ROW_GAP;
			int heightHeading=20;
			Label AddLabel(string name,string text,int yRow) {
				Label label=new Label { Name=name,Text=text,Location=new Point(xLabel,yRow),Size=new Size(widthLabel,UiGrid.FIELD_HEIGHT),TextAlign=ContentAlignment.MiddleRight };
				Controls.Add(label);
				return label;
			}
			Label AddHeading(string name,string text,int yRow) {
				Label label=new Label { Name=name,Text=text,Location=new Point(xLabel,yRow),Size=new Size(widthLabel+UiGrid.LABEL_GAP+widthField,heightHeading),
					TextAlign=ContentAlignment.BottomLeft,Font=UiTheme.FontTitle };
				Controls.Add(label);
				return label;
			}
			AddHeading("labelHeadingCamera","Camera",y);
			y+=heightHeading+UiGrid.ROW_GAP;
			AddLabel("labelCamera","Camera",y);
			comboCamera=new UI.ComboBox { Name="comboCamera",Location=new Point(xField,y),Size=new Size(widthField,UiGrid.FIELD_HEIGHT),TabIndex=1 };
			Controls.Add(comboCamera);
			y+=pitch;
			AddLabel("labelResolution","Picture size",y);
			comboResolution=new UI.ComboBox { Name="comboResolution",Location=new Point(xField,y),Size=new Size(widthField,UiGrid.FIELD_HEIGHT),TabIndex=2 };
			Controls.Add(comboResolution);
			y+=pitch;
			AddLabel("labelPhotoSource","Take photos from",y);
			comboPhotoSource=new UI.ComboBox { Name="comboPhotoSource",Location=new Point(xField,y),Size=new Size(widthField,UiGrid.FIELD_HEIGHT),TabIndex=3 };
			Controls.Add(comboPhotoSource);
			y+=pitch;
			checkMainWindowSpace=new UI.CheckBox { Name="checkMainWindowSpace",Text="Space in the main window takes a photo",Location=new Point(xField,y),
				Size=new Size(widthField,UiGrid.CHECK_HEIGHT),CheckAlign=ContentAlignment.MiddleLeft,TabIndex=4 };
			Controls.Add(checkMainWindowSpace);
			y+=UiGrid.CHECK_HEIGHT+UiGrid.SECTION_GAP;
			AddHeading("labelHeadingButton","Camera button and pedal",y);
			y+=heightHeading+UiGrid.ROW_GAP;
			butLearn=new UI.Button { Name="butLearn",Text="Learn button...",Location=new Point(xField,y),Size=new Size(120,UiGrid.FIELD_HEIGHT),TabIndex=5 };
			butLearn.Click+=(sender,e) => Learn(CaptureLearnMode.Learn);
			Controls.Add(butLearn);
			butButtonTest=new UI.Button { Name="butButtonTest",Text="Test button...",Location=new Point(xField+120+UiGrid.ITEM_GAP,y),Size=new Size(112,UiGrid.FIELD_HEIGHT),TabIndex=6 };
			butButtonTest.Click+=(sender,e) => Learn(CaptureLearnMode.ButtonTest);
			Controls.Add(butButtonTest);
			y+=pitch;
			//Fix round: the notes as tall as their lines (GetHeightTriggerNotes), so the Photos heading is a section gap under the last line, as
			//every heading is under the content before it.
			labelTriggerNotes=new Label { Name="labelTriggerNotes",Text=GetTextTriggerNotes(listTriggerNotes),Location=new Point(xField,y),
				Size=new Size(widthField,UiGrid.CHECK_HEIGHT),ForeColor=UiTheme.TextSecondary };
			Controls.Add(labelTriggerNotes);
			labelTriggerNotes.Height=GetHeightTriggerNotes();
			y+=labelTriggerNotes.Height+UiGrid.SECTION_GAP;
			AddHeading("labelHeadingPhotos","Photos",y);
			y+=heightHeading+UiGrid.ROW_GAP;
			AddLabel("labelAfterCapture","After a press of Capture",y);
			comboAfterCapture=new UI.ComboBox { Name="comboAfterCapture",Location=new Point(xField,y),Size=new Size(widthField,UiGrid.FIELD_HEIGHT),TabIndex=7 };
			Controls.Add(comboAfterCapture);
			y+=pitch;
			checkClick=new UI.CheckBox { Name="checkClick",Text="A click on every photo",Location=new Point(xField,y),Size=new Size(widthField,UiGrid.FIELD_HEIGHT),
				CheckAlign=ContentAlignment.MiddleLeft,TabIndex=8 };
			Controls.Add(checkClick);
			y+=pitchCheck;
			AddLabel("labelIdleMinutes","Release the camera after (minutes)",y);
			textIdleMinutes=new ValidNum { Name="textIdleMinutes",Location=new Point(xField,y),Size=new Size(60,UiGrid.FIELD_HEIGHT),MinVal=PhotoSessionConfig.MinutesIdleMin,
				MaxVal=PhotoSessionConfig.MinutesIdleMax,TabIndex=9 };
			Controls.Add(textIdleMinutes);
			y+=UiGrid.FIELD_HEIGHT+UiGrid.SECTION_GAP;
			AddHeading("labelHeadingVideo","Video",y);
			y+=heightHeading+UiGrid.ROW_GAP;
			AddLabel("labelLimitSeconds","Longest video (seconds)",y);
			textLimitSeconds=new ValidNum { Name="textLimitSeconds",Location=new Point(xField,y),Size=new Size(60,UiGrid.FIELD_HEIGHT),MinVal=PhotoSessionConfig.LimitSecondsMin,
				MaxVal=PhotoSessionConfig.LimitSecondsMax,TabIndex=10 };
			Controls.Add(textLimitSeconds);
			y+=pitch;
			//Fix round: the one-line notes one line tall (the check box height), so the next heading is a section gap under their text.
			Label labelSound=new Label { Name="labelSound",Text="Videos are recorded without sound.",Location=new Point(xField,y),Size=new Size(widthField,UiGrid.CHECK_HEIGHT),
				ForeColor=UiTheme.TextSecondary,TextAlign=ContentAlignment.MiddleLeft };
			Controls.Add(labelSound);
			y+=UiGrid.CHECK_HEIGHT+UiGrid.SECTION_GAP;
			AddHeading("labelHeadingCategories","Categories (all computers)",y);
			y+=heightHeading+UiGrid.ROW_GAP;
			AddLabel("labelCategory","Quick photos and videos",y);
			comboCategory=new UI.ComboBox { Name="comboCategory",Location=new Point(xField,y),Size=new Size(widthField,UiGrid.FIELD_HEIGHT),TabIndex=11 };
			Controls.Add(comboCategory);
			y+=pitch;
			Label labelCategoryNote=new Label { Name="labelCategoryNote",Text="Photo mounts save into their mount's category.",Location=new Point(xField,y),
				Size=new Size(widthField,UiGrid.CHECK_HEIGHT),ForeColor=UiTheme.TextSecondary,TextAlign=ContentAlignment.MiddleLeft };
			Controls.Add(labelCategoryNote);
			y+=UiGrid.CHECK_HEIGHT+UiGrid.SECTION_GAP;
			int xRight=xField+widthField;
			butCancel=new UI.Button { Name="butCancel",Text="Cancel",Location=new Point(xRight-75,y),Size=new Size(75,26),TabIndex=21 };
			butCancel.Click+=(sender,e) => { DialogResult=DialogResult.Cancel; };
			Controls.Add(butCancel);
			butOK=new UI.Button { Name="butOK",Text="OK",Location=new Point(xRight-75-UiGrid.DIALOG_GAP-75,y),Size=new Size(75,26),TabIndex=20 };
			butOK.Click+=butOK_Click;
			Controls.Add(butOK);
			ClientSize=new Size(xRight+UiGrid.PAD,y+26+UiGrid.PAD);
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
					_idxCameraOpen=i;
				}
			}
			if(_listDevices.Count==0) {
				comboCamera.Items.Add(Lan.g(this,"(no camera found)"));
				comboCamera.SelectedIndex=0;
			}
			comboCamera.Enabled=_listDevices.Count>0;
			//the picture sizes are the open camera's: another camera chooses its own, after it opens
			comboCamera.SelectionChangeCommitted+=(sender,e) => comboResolution.Enabled=comboCamera.SelectedIndex==_idxCameraOpen && _listResolutionChoices.Count>0;
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
			butLearn.Enabled=_funcLearn!=null;
			butButtonTest.Enabled=_funcLearn!=null;
			labelTriggerNotes.Text=GetTextTriggerNotes(listTriggerNotes);
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

		private void Learn(CaptureLearnMode captureLearnMode) {
			if(_funcLearn==null) {
				return;
			}
			List<string> listNotes=_funcLearn(captureLearnMode);
			if(listNotes!=null) {
				labelTriggerNotes.Text=GetTextTriggerNotes(listNotes);//what Learn saved
				SizeTriggerNotes();
			}
		}

		private static string GetTextTriggerNotes(List<string> listTriggerNotes) {
			return string.Join("\r\n",listTriggerNotes??new List<string>());
		}

		///<summary>UI-12 slice 14 fix round: the trigger notes' height for their text at the label's width and font (one line at least).</summary>
		private int GetHeightTriggerNotes() {
			int heightLine=labelTriggerNotes.GetPreferredSize(new Size(labelTriggerNotes.Width,0)).Height;
			return Math.Max(LayoutManager==null ? UiGrid.CHECK_HEIGHT : LayoutManager.Scale(UiGrid.CHECK_HEIGHT),heightLine);
		}

		///<summary>UI-12 slice 14 fix round: after Learn changes the notes, the label as tall as its new lines; the sections under it and the
		///window move by the difference, so the Photos heading stays a section gap under the last line.</summary>
		private void SizeTriggerNotes() {
			int heightNew=GetHeightTriggerNotes();
			int shift=heightNew-labelTriggerNotes.Height;
			if(shift==0) {
				return;
			}
			int bottomOld=labelTriggerNotes.Bottom;
			foreach(Control control in labelTriggerNotes.Parent.Controls) {
				if(control.Top>=bottomOld) {
					LayoutManager.MoveLocation(control,new Point(control.Left,control.Top+shift));
				}
			}
			LayoutManager.MoveHeight(labelTriggerNotes,heightNew);
			Height+=shift;//The window grows or shrinks at its bottom, under the user's eyes.
		}

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
			bool isCameraChanged=DeviceSelected!=null && idxCamera!=_idxCameraOpen;
			int idxResolution=comboResolution.SelectedIndex;
			IsModeChanged=!isCameraChanged && idxResolution>=0 && idxResolution<_listResolutionChoices.Count && !_listResolutionChoices[idxResolution].IsSelected;
			ModeSelected=IsModeChanged ? _listResolutionChoices[idxResolution].Mode : null;//null with IsModeChanged: Automatic
			//without the camera open the source cannot be chosen: the saved one stays
			PhotoSource=comboPhotoSource.Enabled ? (comboPhotoSource.SelectedIndex==1 ? CameraPhotoSource.Still : CameraPhotoSource.Preview) : _photoSourceSaved;
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
