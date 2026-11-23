using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	///<summary>The Imaging module's Enhance window (imaging design section 2, IMG-P3b): brightness, contrast, gamma, sharpen, smooth, auto contrast
	///and invert for the selected image, and the presets. A small tool window beside the module that follows the module's selection (a mount item
	///or a single image); the image viewer window's A key selects its image and opens it. A slider previews brightness/contrast at once and the
	///filters a moment later; letting go of it, a check box or a preset saves. Saving needs Image Edit (and Signed Image Edit for a signed image),
	///as the module's other edits; making or changing presets needs Image Adjust Presets. A DICOM image's brightness/contrast stays on the
	///module's windowing slider (its window is on another scale); its filters are here. Controls are made in code (no designer file).</summary>
	public class FormImageEnhance:FormODBase {
		#region Fields - Private
		private const int WIDTH_CONTENT=300;
		private UI.Button butClose;
		private UI.Button butReset;
		private UI.Button butSavePreset;
		private UI.CheckBox checkAutoContrast;
		private UI.CheckBox checkInvert;
		private ControlDoubleBuffered histogram;
		private int[] _arrayHistogram;
		private ControlImageDisplay _controlImageDisplay;
		///<summary>The document as last saved, for the next save's change and log.</summary>
		private Document _documentOld;
		private Document _document;
		private FlowLayoutPanel flowPresets;
		private int _idx=-1;
		private bool _isDicom;
		///<summary>True while the controls are filled in code, so their events do nothing.</summary>
		private bool _isFilling;
		private bool _isEditable;
		///<summary>True once brightness or contrast was changed for this image (a slider, a preset, Reset). Until then a filter change keeps the
		///image's window exactly: not every window has an exact brightness/contrast pair.</summary>
		private bool _isWindowTouched;
		private Label labelNote;
		private Label labelTarget;
		private List<Label> _listLabelsValue=new List<Label>();
		private ImageAdjustSettings _settings=new ImageAdjustSettings();
		private ImageAdjustSettings _settingsOld=new ImageAdjustSettings();
		private Timer _timerFilter;
		private TrackBar trackBrightness;
		private TrackBar trackContrast;
		private TrackBar trackGamma;
		private TrackBar trackSharpen;
		private TrackBar trackSmooth;
		#endregion Fields - Private

		#region Constructor
		public FormImageEnhance() {
			BuildControls();
			InitializeLayoutManager();
			Lan.F(this);
			_timerFilter=new Timer { Interval=300 };
			_timerFilter.Tick+=timerFilter_Tick;
			SetTarget(null);
		}
		#endregion Constructor

		#region Methods - Public
		///<summary>The image this window changes: the selected image of controlImageDisplay (a mount item or a single image), or none. Called
		///whenever the module's selection may have changed; nothing happens when the image is the same.</summary>
		public void SetTarget(ControlImageDisplay controlImageDisplay){
			int idx=-1;
			Document document=null;
			if(controlImageDisplay!=null && !controlImageDisplay.IsDisposed){
				idx=controlImageDisplay.GetSelectedImageIdx();
				document=idx>-1 ? controlImageDisplay.GetDocumentShowing(idx) : null;
			}
			if(controlImageDisplay==_controlImageDisplay && document==_document && idx==_idx && document!=null){
				return;
			}
			SaveIfChanged();//a change still waiting for its save (a key on a slider) goes to the image it was made on
			_controlImageDisplay=document==null ? null : controlImageDisplay;
			_document=document;
			_idx=document==null ? -1 : idx;
			_timerFilter?.Stop();
			if(_document==null){
				labelTarget.Text=Lan.g(this,"No image selected");
				labelNote.Text=Lan.g(this,"Select one image: a single image, or an image in a mount.");
				_isEditable=false;
				_arrayHistogram=null;
				EnableControls();
				histogram.Invalidate();
				return;
			}
			_documentOld=_document.Copy();
			_isWindowTouched=false;
			_settings=(_controlImageDisplay.GetAdjustSettings(_document)??new ImageAdjustSettings()).Copy();
			_settingsOld=_settings.Copy();
			_isDicom=_document.FileName!=null && _document.FileName.ToLower().EndsWith(".dcm");
			string target=_document.DateCreated.ToShortDateString()+": "+_document.Description;
			if(_document.ToothNumbers!=""){
				target+=", "+Lan.g(this,"teeth")+" "+Tooth.DisplayRange(_document.ToothNumbers);
			}
			labelTarget.Text=target;
			_isEditable=_controlImageDisplay.CanEditImage(_idx,suppressMessage:true);
			if(!_isEditable){
				labelNote.Text=Lan.g(this,"Changing this image needs the Image Edit permission (and Signed Image Edit for a signed image).");
			}
			else if(_isDicom){
				labelNote.Text=Lan.g(this,"DICOM: brightness and contrast are on the module's slider.");
			}
			else{
				labelNote.Text="";
			}
			FillControls();
			EnableControls();
			RefreshHistogram();
		}
		#endregion Methods - Public

		#region Methods - Event Handlers
		private void butClose_Click(object sender,EventArgs e) {
			Close();
		}

		private void butPreset_Click(object sender,EventArgs e) {
			if(!_isEditable){
				return;
			}
			ImageAdjustPreset imageAdjustPreset=(ImageAdjustPreset)((Control)sender).Tag;
			_settings=imageAdjustPreset.ToSettings();
			if(!_isDicom){
				ImageEnhance.FromWindowing(imageAdjustPreset.WindowingMin,imageAdjustPreset.WindowingMax,out int brightness,out int contrast);
				_isWindowTouched=true;
				_isFilling=true;
				trackBrightness.Value=brightness;
				trackContrast.Value=contrast;
				_isFilling=false;
			}
			FillFilterControls();
			Preview(doFilter:true);
			SaveIfChanged();
		}

		private void butPreset_MouseUp(object sender,MouseEventArgs e) {
			if(e.Button!=MouseButtons.Right){
				return;
			}
			ImageAdjustPreset imageAdjustPreset=(ImageAdjustPreset)((Control)sender).Tag;
			if(imageAdjustPreset.ImageAdjustPresetNum==0){
				return;//a supplied preset cannot be renamed or deleted
			}
			ContextMenuStrip contextMenuStrip=new ContextMenuStrip();
			contextMenuStrip.Items.Add(Lan.g(this,"Rename..."),null,(s,a) => RenamePreset(imageAdjustPreset));
			contextMenuStrip.Items.Add(Lan.g(this,"Update to the current settings"),null,(s,a) => UpdatePreset(imageAdjustPreset));
			contextMenuStrip.Items.Add(Lan.g(this,"Delete"),null,(s,a) => DeletePreset(imageAdjustPreset));
			contextMenuStrip.Show((Control)sender,e.Location);
		}

		private void butReset_Click(object sender,EventArgs e) {
			if(!_isEditable){
				return;
			}
			_settings=new ImageAdjustSettings();
			if(!_isDicom){
				_isWindowTouched=true;
				_isFilling=true;
				trackBrightness.Value=0;
				trackContrast.Value=0;
				_isFilling=false;
			}
			FillFilterControls();
			Preview(doFilter:true);
			SaveIfChanged();
		}

		private void butSavePreset_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(EnumPermType.ImageAdjustPresets)){
				return;
			}
			InputBox inputBox=new InputBox(Lan.g(this,"Name of the new preset"));
			inputBox.ShowDialog();
			if(inputBox.IsDialogCancel || string.IsNullOrWhiteSpace(inputBox.StringResult)){
				return;
			}
			List<ImageAdjustPreset> listPresets=ImageAdjustPresets.GetAll();
			ImageAdjustPreset imageAdjustPreset=new ImageAdjustPreset();
			imageAdjustPreset.Description=inputBox.StringResult.Trim();
			imageAdjustPreset.ItemOrder=listPresets.Count==0 ? 0 : listPresets.Max(x => x.ItemOrder)+1;
			CopyCurrentInto(imageAdjustPreset);
			ImageAdjustPresets.Insert(imageAdjustPreset);
			SecurityLogs.MakeLogEntry(EnumPermType.ImageAdjustPresets,0,"Image adjust preset added: "+imageAdjustPreset.Description);
			FillPresets();
		}

		private void check_Click(object sender,EventArgs e) {
			if(_isFilling || !_isEditable){
				return;
			}
			_settings.IsAutoContrast=checkAutoContrast.Checked;
			_settings.IsInverted=checkInvert.Checked;
			Preview(doFilter:true);
			SaveIfChanged();
		}

		private void histogram_Paint(object sender,PaintEventArgs e) {
			Graphics g=e.Graphics;
			g.Clear(UiTheme.SurfaceSubtle);
			if(_arrayHistogram==null){
				return;
			}
			int max=Math.Max(1,_arrayHistogram.Max());
			using SolidBrush solidBrush=new SolidBrush(UiTheme.TextSecondary);
			float widthBar=histogram.Width/256f;
			for(int v=0;v<256;v++){
				float height=histogram.Height*(float)Math.Sqrt(_arrayHistogram[v]/(double)max);//square root, so the dark peak leaves room for the rest
				g.FillRectangle(solidBrush,v*widthBar,histogram.Height-height,Math.Max(1,widthBar),height);
			}
		}

		private void timerFilter_Tick(object sender,EventArgs e) {
			_timerFilter.Stop();
			Preview(doFilter:true);
		}

		private void track_Commit(object sender,EventArgs e) {
			//letting go of a slider, or a key on it
			if(_isFilling || !_isEditable){
				return;
			}
			_timerFilter.Stop();
			Preview(doFilter:true);
			SaveIfChanged();
		}

		private void track_ValueChanged(object sender,EventArgs e) {
			FillValueLabels();
			if(_isFilling || !_isEditable){
				return;
			}
			if(sender==trackBrightness || sender==trackContrast){
				_isWindowTouched=true;
			}
			_settings.Gamma=trackGamma.Value/100.0;
			_settings.Sharpen=trackSharpen.Value;
			_settings.Smooth=(ImageSmooth)trackSmooth.Value;
			//brightness/contrast at once (cheap); the filters once the slider rests
			Preview(doFilter:_settings.IsNeutral());
			if(!_settings.IsNeutral()){
				_timerFilter.Stop();
				_timerFilter.Start();
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs e) {
			SaveIfChanged();
			_timerFilter.Stop();
			base.OnFormClosing(e);
		}
		#endregion Methods - Event Handlers

		#region Methods - Private
		///<summary>Makes the window's controls on the UI-12 grid: 8 px padding, labels 6 px before their field, a 27 px row pitch, the buttons at the
		///bottom.</summary>
		private void BuildControls(){
			SuspendLayout();
			int y=8;
			labelTarget=new Label { Location=new Point(8,y),Size=new Size(WIDTH_CONTENT,20),Name="labelTarget",TextAlign=ContentAlignment.MiddleLeft };
			labelTarget.Font=new Font(UiTheme.FontBody,FontStyle.Bold);
			Controls.Add(labelTarget);
			y+=24;
			flowPresets=new FlowLayoutPanel { Location=new Point(8,y),Size=new Size(WIDTH_CONTENT,54),Name="flowPresets",WrapContents=true,AutoScroll=true };
			Controls.Add(flowPresets);
			y+=58;
			histogram=new ControlDoubleBuffered { Location=new Point(8,y),Size=new Size(WIDTH_CONTENT,40),Name="histogram",TabStop=false };
			histogram.Paint+=histogram_Paint;
			Controls.Add(histogram);
			y+=48;
			trackBrightness=AddTrackRow("Brightness",-100,100,ref y);
			trackContrast=AddTrackRow("Contrast",0,100,ref y);
			trackGamma=AddTrackRow("Gamma",50,200,ref y);
			trackSharpen=AddTrackRow("Sharpen",0,100,ref y);
			trackSmooth=AddTrackRow("Smooth",0,2,ref y);
			checkAutoContrast=new UI.CheckBox { Location=new Point(90,y),Size=new Size(WIDTH_CONTENT-82,23),Name="checkAutoContrast",Text="Auto contrast",
				CheckAlign=ContentAlignment.MiddleLeft };
			checkAutoContrast.CheckedChanged+=check_Click;
			Controls.Add(checkAutoContrast);
			y+=27;
			checkInvert=new UI.CheckBox { Location=new Point(90,y),Size=new Size(WIDTH_CONTENT-82,23),Name="checkInvert",Text="Invert",CheckAlign=ContentAlignment.MiddleLeft };
			checkInvert.CheckedChanged+=check_Click;
			Controls.Add(checkInvert);
			y+=27;
			labelNote=new Label { Location=new Point(8,y),Size=new Size(WIDTH_CONTENT,34),Name="labelNote",ForeColor=UiTheme.TextSecondary };
			Controls.Add(labelNote);
			y+=38;
			butReset=new UI.Button { Location=new Point(8,y),Size=new Size(75,26),Name="butReset",Text="Reset" };
			butReset.Click+=butReset_Click;
			Controls.Add(butReset);
			butSavePreset=new UI.Button { Location=new Point(91,y),Size=new Size(120,26),Name="butSavePreset",Text="Save as preset..." };
			butSavePreset.Click+=butSavePreset_Click;
			Controls.Add(butSavePreset);
			butClose=new UI.Button { Location=new Point(8+WIDTH_CONTENT-75,y),Size=new Size(75,26),Name="butClose",Text="Close" };
			butClose.Click+=butClose_Click;
			Controls.Add(butClose);
			ClientSize=new Size(WIDTH_CONTENT+16,y+26+8);
			Name="FormImageEnhance";
			Text="Enhance";
			ShowIcon=false;
			ShowInTaskbar=false;
			MinimizeBox=false;
			MaximizeBox=false;
			StartPosition=FormStartPosition.Manual;
			ResumeLayout(false);
			FillPresets();
		}

		private TrackBar AddTrackRow(string text,int min,int max,ref int y){
			Label label=new Label { Location=new Point(8,y),Size=new Size(76,23),Text=text,TextAlign=ContentAlignment.MiddleRight,Name="label"+text };
			Controls.Add(label);
			//AutoSize off before the size: with it on, Windows keeps a track bar 45 px tall, over the rows below.
			TrackBar trackBar=new TrackBar { AutoSize=false,TickStyle=TickStyle.None };
			trackBar.Location=new Point(90,y);
			trackBar.Size=new Size(170,23);
			trackBar.Minimum=min;
			trackBar.Maximum=max;
			trackBar.Name="track"+text;
			trackBar.SmallChange=1;
			trackBar.LargeChange=Math.Max(1,(max-min)/10);
			trackBar.ValueChanged+=track_ValueChanged;
			trackBar.MouseUp+=track_Commit;
			trackBar.KeyUp+=track_Commit;
			Controls.Add(trackBar);
			Label labelValue=new Label { Location=new Point(264,y),Size=new Size(44,23),TextAlign=ContentAlignment.MiddleRight,Name="labelValue"+text,
				ForeColor=UiTheme.TextSecondary,Tag=trackBar };
			Controls.Add(labelValue);
			_listLabelsValue.Add(labelValue);
			y+=27;
			return trackBar;
		}

		private void CopyCurrentInto(ImageAdjustPreset imageAdjustPreset){
			imageAdjustPreset.Gamma=_settings.HasGamma() ? _settings.Gamma : 1;
			imageAdjustPreset.Sharpen=_settings.Sharpen;
			imageAdjustPreset.Smooth=_settings.Smooth;
			imageAdjustPreset.IsAutoContrast=_settings.IsAutoContrast;
			imageAdjustPreset.IsInverted=_settings.IsInverted;
			ImageEnhance.ToWindowing(trackBrightness.Value,trackContrast.Value,out int windowingMin,out int windowingMax);
			imageAdjustPreset.WindowingMin=windowingMin;
			imageAdjustPreset.WindowingMax=windowingMax;
		}

		private void DeletePreset(ImageAdjustPreset imageAdjustPreset){
			if(!Security.IsAuthorized(EnumPermType.ImageAdjustPresets)){
				return;
			}
			if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,Lan.g(this,"Delete the preset")+" "+imageAdjustPreset.Description+"?")){
				return;
			}
			ImageAdjustPresets.Delete(imageAdjustPreset.ImageAdjustPresetNum);
			SecurityLogs.MakeLogEntry(EnumPermType.ImageAdjustPresets,0,"Image adjust preset deleted: "+imageAdjustPreset.Description);
			FillPresets();
		}

		private void EnableControls(){
			bool isEnabled=_document!=null && _isEditable;
			foreach(Control control in new Control[] { trackGamma,trackSharpen,trackSmooth,checkAutoContrast,checkInvert,butReset,flowPresets }){
				control.Enabled=isEnabled;
			}
			trackBrightness.Enabled=isEnabled && !_isDicom;
			trackContrast.Enabled=isEnabled && !_isDicom;
			butSavePreset.Enabled=_document!=null;
		}

		private void FillControls(){
			_isFilling=true;
			if(_isDicom){
				trackBrightness.Value=0;
				trackContrast.Value=0;
			}
			else{
				ImageEnhance.FromWindowing(_document.WindowingMin,_document.WindowingMax,out int brightness,out int contrast);
				trackBrightness.Value=brightness;
				trackContrast.Value=contrast;
			}
			_isFilling=false;
			FillFilterControls();
		}

		private void FillFilterControls(){
			_isFilling=true;
			trackGamma.Value=Math.Max(50,Math.Min(200,(int)Math.Round((_settings.HasGamma() ? _settings.Gamma : 1)*100)));
			trackSharpen.Value=Math.Max(0,Math.Min(100,_settings.Sharpen));
			trackSmooth.Value=(int)_settings.Smooth;
			checkAutoContrast.Checked=_settings.IsAutoContrast;
			checkInvert.Checked=_settings.IsInverted;
			_isFilling=false;
			FillValueLabels();
		}

		///<summary>The preset buttons: the supplied ones, then the office's own that are not hidden (a right-click on one renames, updates or
		///deletes it).</summary>
		private void FillPresets(){
			flowPresets.Controls.Clear();
			List<ImageAdjustPreset> listPresets=ImageEnhance.GetBuiltInPresets();
			try{
				listPresets.AddRange(ImageAdjustPresets.GetAll().Where(x => !x.IsHidden));
			}
			catch(Exception ex){
				ex.DoNothing();//no database (the gallery's pictures): the supplied ones only
			}
			foreach(ImageAdjustPreset imageAdjustPreset in listPresets){
				UI.Button button=new UI.Button { Text=imageAdjustPreset.Description,Tag=imageAdjustPreset,Size=new Size(70,23),Margin=new Padding(0,0,4,4) };
				button.Click+=butPreset_Click;
				button.MouseUp+=butPreset_MouseUp;
				flowPresets.Controls.Add(button);
			}
		}

		private void FillValueLabels(){
			foreach(Label label in _listLabelsValue){
				TrackBar trackBar=(TrackBar)label.Tag;
				if(trackBar==trackGamma){
					label.Text=(trackBar.Value/100.0).ToString("0.00");
				}
				else if(trackBar==trackSmooth){
					label.Text=Lan.g(this,((ImageSmooth)trackBar.Value).ToString());
				}
				else if(trackBar==trackBrightness && trackBar.Value>0){
					label.Text="+"+trackBar.Value;
				}
				else{
					label.Text=trackBar.Value.ToString();
				}
			}
		}

		///<summary>Shows the image with the window's values, without saving. doFilter false shows brightness/contrast only (fast, while a slider
		///moves).</summary>
		private void Preview(bool doFilter){
			if(_document==null || _controlImageDisplay==null || _controlImageDisplay.IsDisposed){
				return;
			}
			int windowingMin=_document.WindowingMin;
			int windowingMax=_document.WindowingMax;
			if(!_isDicom && _isWindowTouched){
				ImageEnhance.ToWindowing(trackBrightness.Value,trackContrast.Value,out windowingMin,out windowingMax);
				if(windowingMin==0 && windowingMax==255 && _documentOld.WindowingMax==0){
					windowingMax=0;//never set stays never set
				}
			}
			_controlImageDisplay.PreviewAdjust(_idx,windowingMin,windowingMax,_settings,doFilter);
			if(doFilter){
				RefreshHistogram();
			}
		}

		private void RefreshHistogram(){
			_arrayHistogram=_controlImageDisplay?.GetHistogram(_idx);
			histogram.Invalidate();
		}

		private void RenamePreset(ImageAdjustPreset imageAdjustPreset){
			if(!Security.IsAuthorized(EnumPermType.ImageAdjustPresets)){
				return;
			}
			InputBox inputBox=new InputBox(Lan.g(this,"New name"),imageAdjustPreset.Description);
			inputBox.ShowDialog();
			if(inputBox.IsDialogCancel || string.IsNullOrWhiteSpace(inputBox.StringResult)){
				return;
			}
			string descriptionOld=imageAdjustPreset.Description;
			imageAdjustPreset.Description=inputBox.StringResult.Trim();
			ImageAdjustPresets.Update(imageAdjustPreset);
			SecurityLogs.MakeLogEntry(EnumPermType.ImageAdjustPresets,0,"Image adjust preset renamed: "+descriptionOld+" to "+imageAdjustPreset.Description);
			FillPresets();
		}

		///<summary>Saves what changed since the last save on the image: its window and its filters (ControlImageDisplay.SaveAdjust logs it).</summary>
		private void SaveIfChanged(){
			if(_document==null || !_isEditable || _controlImageDisplay==null || _controlImageDisplay.IsDisposed){
				return;
			}
			bool isWindowChanged=_document.WindowingMin!=_documentOld.WindowingMin || _document.WindowingMax!=_documentOld.WindowingMax;
			bool isFilterChanged=ImageEnhance.Describe(_settings)!=ImageEnhance.Describe(_settingsOld);
			if(!isWindowChanged && !isFilterChanged){
				return;
			}
			_controlImageDisplay.SaveAdjust(_idx,_documentOld,_settingsOld);
			_documentOld=_document.Copy();
			_settingsOld=_settings.Copy();
		}

		private void UpdatePreset(ImageAdjustPreset imageAdjustPreset){
			if(!Security.IsAuthorized(EnumPermType.ImageAdjustPresets)){
				return;
			}
			CopyCurrentInto(imageAdjustPreset);
			ImageAdjustPresets.Update(imageAdjustPreset);
			SecurityLogs.MakeLogEntry(EnumPermType.ImageAdjustPresets,0,"Image adjust preset changed: "+imageAdjustPreset.Description+" ("
				+ImageEnhance.Describe(imageAdjustPreset.ToSettings())+")");
			FillPresets();
		}
		#endregion Methods - Private
	}
}
