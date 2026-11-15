using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using CodeBase;

namespace WpfControls.UI {
	/// <summary>This Frm is only used by ProgressOD. Do not call it directly.</summary>
	public partial class FrmProgressAuto:FrmODBase {
		public Action ActionMain;
		public Exception ExceptionGenerated;
		public bool HasHistory;
		public bool HistoryClose;
		public string HistoryMsg;
		///<summary>Default is a continuous indeterminate marquee appearance.  Setting IsBlocks to true allows you to manually control the progress out of 100%.</summary>
		public bool IsBlocks=false;
		public bool ShowCancelButton;
		public string StartingMessage;
		public string StopNotAllowedMessage;
		public string MessageCancel;
		public bool TestSleep;
		///<summary>Invoked on the main thread when the user cancels while the action runs, before the window waits for the action to stop. For an action that waits on something of its own, for example a CancellationTokenSource of the caller.</summary>
		public Action CancelAction { get; set; }
		///<summary>Cancelled when the user cancels while the action runs. ProgressWin passes its own, whose token the action can check (ProgressWin.CancellationToken). Created in Load if not set.</summary>
		public CancellationTokenSource CancellationTokenSource;
		///<summary>Set when the user cancelled and the action had not stopped within WorkerStopWaitMS. The window has closed as cancelled anyway; the background thread finishes on its own and nothing it does after that reaches the window or the caller.</summary>
		public bool IsWorkerStillRunning;
		///<summary>How long a cancel waits for the action to stop before the window closes without it. The action is never aborted (Thread.Abort throws PlatformNotSupportedException on .NET 5 and later).</summary>
		public int WorkerStopWaitMS=1000;

		///<summary>The date and time of the most recent event that this form processed. Used for history.</summary>
		private DateTime _dateTimeLastEvent;
		private EventInfo _eventInfoFired;
		private Thread _threadWorker;
		///<summary>If there's some problem with the action and an exception is thrown, this gets set true.</summary>
		private bool _isForceClose=false;
		///<summary>Set on the main thread when the user cancels while the action runs. From then on the worker thread changes nothing on the window or in ExceptionGenerated, and a progress event fired from the worker thread throws OperationCanceledException (a cancellation point for actions that report progress). Volatile because the worker thread reads it.</summary>
		private volatile bool _isCancelRequested=false;
		///<summary>Set on the main thread (inside the worker's Invoke) when the action has finished, with or without an exception. Used instead of the thread's IsAlive, which stays true for a moment after the action.</summary>
		private bool _isWorkDone=false;

		public FrmProgressAuto() {
			InitializeComponent();
			Load+=FrmProgressAuto_Load;
			FormClosing+=FormProgressAuto_FormClosing;
		}

		private void FrmProgressAuto_Load(object sender,EventArgs e) {
			ODEvent.Fired+=ODEvent_Fired;
			labelError.Visible=false;
			labelMsg.Text=StartingMessage;//this will never be null because of how it's passed in from ProgressOD.
			if(!ShowCancelButton){
				butCancel.Visible=false;
			}
			if(HasHistory){
				//labelMsg.Visible=false;//this will happen with first event
				_dateTimeLastEvent=DateTime.MinValue;
				if(!HistoryMsg.IsNullOrEmpty() && !HistoryMsg.EndsWith("\r\n")){
					textHistoryMsg.Text=HistoryMsg+"\r\n";
				}
				//597x406 client area
			}
			else{
				//540x148 client area = 556x182 form size
				_formFrame.Width=ScaleFormValue(556);
				_formFrame.Height=ScaleFormValue(182);
				textHistoryMsg.Visible=false;
				butCopy.Visible=false;
			}
			if(IsBlocks){
				progressBar.IsIndeterminate=false;
			}
			else{
				progressBar.IsIndeterminate=true;
			}
			if(CancellationTokenSource==null) {
				CancellationTokenSource=new CancellationTokenSource();
			}
			_threadWorker=new Thread(new ThreadStart(DoWork));
			_threadWorker.IsBackground=true;//shuts down automatically when program shuts down
			_threadWorker.Start();
		}

		private void ODEvent_Fired(ODEventArgs eventArgs) {
			if(_isCancelRequested && (eventArgs.Tag is string || eventArgs.Tag is ProgressBarHelper)) {
				if(Thread.CurrentThread==_threadWorker) {
					//The user cancelled. A progress report from the action is where it stops, in place of the old Thread.Abort: the exception ends the
					//action unless the action catches it, and DoWork drops it.
					throw new OperationCanceledException(CancellationTokenSource.Token);
				}
				return;//The window is closing or closed as cancelled, so it shows no more progress.
			}
			//There seem to be a variety of types used that were clearly intended to be shown in progress bar, so we'll include all.
			//Besides, the msgs only hit this spot if a progress window is open, so it's probably relevant.
			if(eventArgs.Tag is string str){
				ProgressBarHelper progressBarHelper2=new ProgressBarHelper(str);
				try {
					Dispatcher.Invoke(delegate() { UpdateProgress(str,progressBarHelper2); });
				}
				catch(Exception ex) {
					ex.DoNothing();//It's just progress...
				}
			}
			if(eventArgs.Tag is ProgressBarHelper progressBarHelper){
				try {
					Dispatcher.Invoke(delegate() { UpdateProgress(progressBarHelper.LabelValue,progressBarHelper); });
				}
				catch(Exception ex) {
					ex.DoNothing();//It's just progress...
				}
			}
		}

		///<summary>This happens on background thread.</summary>
		private void DoWork(){
			try {
				DoWorkAndReport();
			}
			finally {
				//The window unsubscribes when it closes too, but after a cancel the worker can outlive the window, and its progress events are its
				//cancellation points until it ends (ODEvent_Fired).
				ODEvent.Fired-=ODEvent_Fired;
			}
		}

		///<summary>Runs the action and reports its outcome to the window on the main thread, unless the user cancelled first. The worker thread is never aborted: the window used to abort it here after the action, which on .NET 5 and later throws PlatformNotSupportedException instead.</summary>
		private void DoWorkAndReport(){
			if(ODBuild.IsDebug()){
				if(TestSleep){
					Thread.Sleep(3000);
				}
			}
			try {
				ActionMain();
			}
			catch(ThreadAbortException){//Left from the old abort (harmless; nothing aborts this thread now). Removed after the .NET 10 switch (net10-plan, S3a-1).
				Dispatcher.Invoke(() => {
					_isForceClose=true;
					IsDialogOK=false;
				});
				return;
			}
			catch(Exception ex) when(_isCancelRequested) {
				//The user cancelled and the window has closed as cancelled. The action stopped at a cancellation point (OperationCanceledException from
				//the token or from a progress event) or failed after the cancel; nobody waits for its outcome any more.
				ex.DoNothing();
				return;
			}
			catch(Exception ex) {//normal exception
				Dispatcher.Invoke(() => {
					if(_isCancelRequested) {
						return;//The user's cancel reached the window first, so the window closed as cancelled.
					}
					ExceptionGenerated=ex;
					_isWorkDone=true;
					_isForceClose=true;
					IsDialogOK=false;
				});
				return;
			}
			if(_isCancelRequested) {
				return;//The action finished after the user cancelled: the window has closed as cancelled.
			}
			if(HasHistory && !HistoryClose){
				Dispatcher.Invoke(() => {
					if(_isCancelRequested) {
						return;
					}
					_isWorkDone=true;
					butCancel.Text=Lans.g(this,"Close");
					labelMsg.Text=Lans.g(this,"Done");
					labelMsg.Visible=true;
					progressBar.Visibility=Visibility.Collapsed;
				});
				//do not close form
			}
			else{
				Dispatcher.Invoke(() => {
					if(_isCancelRequested) {
						return;
					}
					_isWorkDone=true;
					IsDialogOK=true;
				});
			}
		}

		private void UpdateProgress(string status,ProgressBarHelper progressBarHelper) {
			if(HasHistory && progressBar.Visibility==Visibility.Collapsed) {
				//Once the progress bar is hidden when history is showing, we never want to process another event.
				return;
			}
			if(HasHistory){
				labelMsg.Visible=false;//was visible until first event
			}
			else{
				labelMsg.Text=status;
			}
			//if(hasProgHelper) {
			if(progressBarHelper.BlockMax!=0) {
				progressBar.Maximum=progressBarHelper.BlockMax;
			}
			if(progressBarHelper.BlockValue!=0 && progressBarHelper.BlockValue>=progressBar.Minimum && progressBarHelper.BlockValue<=progressBar.Maximum) {
				progressBar.Value=progressBarHelper.BlockValue;
			}
			if(progressBarHelper.ProgressStyle==ProgBarStyle.Marquee) {
				progressBar.IsIndeterminate=true;;
			}
			else if(progressBarHelper.ProgressStyle==ProgBarStyle.Blocks) {
				progressBar.IsIndeterminate=false;
			}
			//}
			if(HasHistory) {
				if(_dateTimeLastEvent==DateTime.MinValue) {
					textHistoryMsg.Text=textHistoryMsg.Text+status.PadRight(60);//left aligned
					_dateTimeLastEvent=DateTime.Now;
				}
				else {
					textHistoryMsg.Text=textHistoryMsg.Text+GetElapsedTime(_dateTimeLastEvent,DateTime.Now)+"\r\n"+status.PadRight(60);
					_dateTimeLastEvent=DateTime.Now;
				}
				HistoryMsg=textHistoryMsg.Text;
			}
			if(progressBarHelper.ErrorMsg==""){
				labelError.Visible=false;
			}
			else{
				labelError.Visible=true;
				labelError.Text=progressBarHelper.ErrorMsg;
			}
		}

		///<summary>Gets a user friendly elapsed time message to display in the history text box.</summary>
		private string GetElapsedTime(DateTime start, DateTime end) {
			TimeSpan timeElapsed=new TimeSpan(end.Ticks-start.Ticks);
			if(timeElapsed.TotalMinutes>2) {
				return "Elapsed Time: "+timeElapsed.TotalMinutes.ToString("F1")+" minutes";
			}
			else if(timeElapsed.TotalSeconds>2) {
				return "Elapsed Time: "+timeElapsed.TotalSeconds.ToString("F0")+" seconds";
			}
			else {
				return "Elapsed Time: "+timeElapsed.TotalMilliseconds.ToString("F0")+" milliseconds";
			}
		}

		private void butCancel_Click(object sender,EventArgs e) {
			if(!MessageCancel.IsNullOrEmpty()) {
				if(!MsgBox.Show(MsgBoxButtons.YesNo,MessageCancel,"")) {
					return;
				}
			}
			IsDialogCancel=true;//also causes window to Close.
		}

		private void butCopy_Click(object sender,EventArgs e) {
			try {
				ODClipboard.SetClipboard(textHistoryMsg.Text);
				MessageBox.Show("Copied");
			}
			catch(Exception) {
				MessageBox.Show("Could not copy contents to the clipboard.  Please try again.");
			}
		}

		private void FormProgressAuto_FormClosing(object sender,CancelEventArgs e) {
			if(IsDialogOK) {
				ODEvent.Fired-=ODEvent_Fired;
				return;
			}
			if(_isForceClose) {
				ODEvent.Fired-=ODEvent_Fired;
				return;
			}
			//From here down, user intentionally tried to close window, either with X or with Cancel button, if visible.
			if(!ShowCancelButton) {//tried to close with X, which is not allowed if Cancel button is not showing.
				MessageBox.Show(StopNotAllowedMessage);//Message is only used in a few places.
				e.Cancel=true;
				return;
			}
			if(!_isWorkDone){//user manually cancelled action with either cancel or X, and they are allowed to.
				CancelWorker();
				if(IsWorkerStillRunning) {
					return;//Stay subscribed: the action's progress events remain its cancellation points until it ends (DoWork unsubscribes).
				}
			}
			else{
				//must be HasHistory.  Button says "Close" here
				//in this case, the action is finished, but the form was kept open to display information to user.
				//User had to manually close form by either "Close" or XClick when finished viewing
				IsDialogOK=true;
			}
			ODEvent.Fired-=ODEvent_Fired;
		}

		///<summary>Cooperative cancellation, in place of the old Thread.Abort of the worker. Runs on the main thread while the window closes as cancelled: marks the cancel (so the worker reports nothing to the window any more), cancels CancellationTokenSource, invokes CancelAction, then waits up to WorkerStopWaitMS for the action to stop. If it has not stopped by then, IsWorkerStillRunning is set and the window closes anyway; the background thread finishes on its own.</summary>
		private void CancelWorker() {
			_isCancelRequested=true;
			try {
				CancellationTokenSource.Cancel();
			}
			catch(Exception ex) {
				ex.DoNothing();//A callback registered on the token failed. The cancel goes on.
			}
			if(CancelAction!=null) {
				try {
					CancelAction();
				}
				catch(Exception ex) {
					ex.DoNothing();//The caller's own cancel failed. The cancel goes on.
				}
			}
			//Bounded wait. The worker does not need this thread to stop: after the cancel it no longer invokes onto the window.
			IsWorkerStillRunning=!_threadWorker.Join(WorkerStopWaitMS);
		}
		
	}
}
