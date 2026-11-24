using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace CodeBase {
	///<summary>Do not instatiate this class.  It is not meant for public use.  Use ODProgress.ShowProgressExtended() instead.
	///Launch this window in a separate thread so that the progress bar can smoothly spin without waiting on the main thread.
	///This form has a custom UI that is designed to dynamically add up to 10 progress bars (see AddNewProgressBar() for more details).</summary>
	public partial class FormProgressExtended : FormProgressBase {
		///<summary>Progress bars that have already been added to the form.</summary>
		private List<ODProgressBar> _listProgressBars=new List<ODProgressBar>();
		///<summary>Event is fired when the user clicks the pause/resume button.</summary>
		public event ProgressPausedHandler ProgressPaused=null;
		///<summary>Event is fired when the user clicks the cancel button.</summary>
		public event ProgressCanceledHandler ProgressCanceled=null;
		///<summary>True if the user clicked Pause, set back to false when the user clicks Resume.</summary>
		private bool _isPaused;
		/// <summary>True if progress has been completed and the Done event has been called. </summary>
		private bool _isDone;
		///<summary>UI-12 slice 14 fix round: the warning's row, its height and the row gap over it. The designer lays the log panel out without it (the
		///buttons a section gap under the log box); ShowWarning adds it while the warning shows.</summary>
		private int _heightWarningRow;
		///<summary>True while the log panel holds the warning's row.</summary>
		private bool _isWarningRowShown;

		///<summary>Do not instatiate this class.  It is not meant for public use.  Use ODProgress.ShowProgressExtended() instead.
		///Launches a progress window that will constantly spin and display status updates for global ODEvents with corresponding name.
		///eventType must be a Type that contains an event called Fired.</summary>
		public FormProgressExtended(bool hasHistory=false,string cancelButtonText=null) {
			InitializeComponent();
			UiPlainWindow.Attach(this);//UI-PLAIN: the look of the program's windows (docs/port-notes/UI-PLAIN.md)
			_heightWarningRow=labelWarning.Height+UiGrid.ROW_GAP;
			//UI-12 slice 14 fix round: the window as tall as its content, a padding under the buttons, also when a bar or the warning is added. Not
			//AutoSize: with the program's frame on, Windows Forms sized an AutoSize window for Windows' title bar too (22 px of empty band).
			flowLayoutPanel1.SizeChanged+=(sender,e) => FitHeightToContent();
			if(!string.IsNullOrEmpty(cancelButtonText)) {
				butCancel.Text=cancelButtonText;
			}
		}

		///<summary>Creates a new progress bar and adds it to the form.  Returns null if there are more than 10 progress bars.</summary>
		private ODProgressBar AddNewProgressBar(string leftLabel,string topLabel,string rightLabel,int blockValue,int blockMax,string tagString, 
			ProgBarStyle progStyle,int marqSpeed,bool isLeftHidden,bool isTopHidden,bool isRightHidden) 
		{
			if(_listProgressBars.Count>10) {
				return null;
			}
			ODProgressBar pbar=new ODProgressBar(leftLabel,topLabel,rightLabel,blockValue,blockMax,tagString,progStyle,marqSpeed,isLeftHidden,isTopHidden,
				isRightHidden);
			pbar.TabStop=false;
			int rowLocation=tableLayoutPanel1.RowCount;
			tableLayoutPanel1.Controls.Add(pbar,1,rowLocation);
			pbar.Name="pbar"+rowLocation;
			return pbar;
		}

		///<summary>UI-12 slice 14 fix round: the window's height for its content: the flow panel's log panel ends a padding over the bottom. The
		///window's height changes by the difference, so the program's frame (its title band in the client area) is kept.</summary>
		private void FitHeightToContent() {
			int heightClient=flowLayoutPanel1.Top+panel1.Bottom+UiGrid.PAD;
			if(ClientSize.Height!=heightClient) {
				Height+=heightClient-ClientSize.Height;
			}
		}

		///<summary>UI-12 slice 14 fix round: shows or hides the warning with its row: the log panel grows or shrinks by the row, and the buttons and
		///the warning, anchored to its bottom, follow (the window follows the flow panel, FitHeightToContent).</summary>
		private void ShowWarning(bool isShown) {
			labelWarning.Visible=isShown;
			if(isShown==_isWarningRowShown) {
				return;
			}
			_isWarningRowShown=isShown;
			panel1.Height+=isShown ? _heightWarningRow : -_heightWarningRow;
			FitHeightToContent();//the flow panel grows with the log panel, but never shrinks (AutoSize GrowOnly)
		}

		private void UpdateProgressBar(ODProgressBar progBar, ProgressBarHelper progHelper) {
			progBar.ODProgUpdate(progHelper.LabelValue, progHelper.LabelTop, progHelper.PercentValue, (int)progHelper.BlockValue,(int)progHelper.BlockMax, 
				progHelper.TagString,progHelper.ProgressStyle,progHelper.MarqueeSpeed,progHelper.IsValHidden,progHelper.IsTopHidden,progHelper.IsPercentHidden);
		}

		public sealed override void UpdateProgress(string status,ProgressBarHelper progHelper,bool hasProgHelper) {
			if(hasProgHelper) {
				switch(progHelper.ProgressBarEventType) {
					case ProgBarEventType.BringToFront:
						this.TopMost=true;
						this.TopMost=false;
						break;
					case ProgBarEventType.Header:
						this.Text=status;
						break;
					case ProgBarEventType.ProgressLog:
						label4.Text=status;
						break;
					case ProgBarEventType.TextMsg:
						status=status.Trim();//Get rid of leading and trailing new lines.
						textMsg.AppendText((string.IsNullOrWhiteSpace(textMsg.Text) ? "" : "\r\n")+status.PadRight(60));
						break;
					case ProgBarEventType.WarningOff:
					case ProgBarEventType.AllowResume:
						ShowWarning(false);
						butPause.Enabled=true;
						if(_isPaused) {
							butPause.Text="Resume";
						}
						else {
							butPause.Text="Pause";//setting down below wasn't always working when this event type was passed in. 
						}
						break;
					case ProgBarEventType.Done:
						butCancel.Visible=true; //can sometimes be set to invisible with HideButtons. Show if previously invisible.
						butCancel.Text="Close";
						butPause.Enabled=false;
						_isDone=true;
						break;
					case ProgBarEventType.HideButtons://hide pause and cancel. Call Done when "close button" should appear. 
						butPause.Visible=false;
						butCancel.Visible=false;
						break;
					case ProgBarEventType.ProgressBar:
					default:
						if(!_listProgressBars.Exists(x => x.TagString.ToLower()==progHelper.TagString.ToLower())) {//if not already added
							ODProgressBar progBar=AddNewProgressBar(progHelper.LabelValue,progHelper.LabelTop,progHelper.PercentValue,(int)progHelper.BlockValue,
								(int)progHelper.BlockMax,progHelper.TagString,progHelper.ProgressStyle,progHelper.MarqueeSpeed,progHelper.IsValHidden,progHelper.IsTopHidden,
								progHelper.IsPercentHidden);
							if(progBar==null) {
								break;
							}
							_listProgressBars.Add(progBar);
						}
						else {
							ODProgressBar odBar=_listProgressBars.Find(x => x.TagString.ToLower()==progHelper.TagString.ToLower());
							UpdateProgressBar(odBar,progHelper);
						}
						break;
				}
			}
		}

		private void butPause_Click(object sender,EventArgs e) {
			_isPaused=(!_isPaused);
			ProgressPaused?.Invoke(this,new ProgressPausedArgs(_isPaused));
			if(_isPaused) {
				butPause.Text="Resume";
				//We are not going to allow the user to immediately unpause because we want to allow the instantiator of this form to get itself into a state
				//where it can be unpaused. When it is ready to be unpaused, it will fire an event with the tag "AllowResume".
				butPause.Enabled=false;
				ShowWarning(true);
			}
			else {
				butPause.Text="Pause";
			}
		}

		private void butCancel_Click(object sender,EventArgs e) {//also can change to 'close'
			ProgressCanceled?.Invoke(this,new EventArgs());
			//disable button until window is ready to cancel and close
			if(_isDone) {
				DialogResult=DialogResult.OK;
			}
			//process has not been completed. Wait to finish current process and then have instantiator close.
			else {
				butCancel.Enabled=false;
				butPause.Enabled=false;
				ShowWarning(true);
			}
		}

	}//End FormProgressExtended

	public delegate void ProgressCanceledHandler(object sender,EventArgs e);
	public delegate void ProgressPausedHandler(object sender,ProgressPausedArgs e);

	public class ProgressPausedArgs {
		public bool IsPaused;
		public ProgressPausedArgs(bool isPaused) {
			IsPaused=isPaused;
		}
	}
}

