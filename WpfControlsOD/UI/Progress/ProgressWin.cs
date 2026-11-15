using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDentalBusiness;
using WpfControls;

namespace OpenPmsDental.UI{
/*
How to use ProgressWin:
Jordan is the only one allowed to edit this file.
Jordan personally reviews all progress bars that get added.

Example, Simple======================================================================================================================
			ProgressWin progressWin=new ProgressWin();
			progressWin.ActionMain=() => ;
			progressWin.ShowDialog();
			if(progressWin.IsCancelled){
				return;
			}

Example, Complex=====================================================================================================================
(Obviously, you wouldn't normally use all of these.  Pick and choose.)
ProgressWin progressWin=new ProgressWin();
progressWin.ActionMain=() => ;
progressWin.IsBlocks=true;//marquee is more popular because it requires less programming effort.
progressWin.StartingMessage=Lan.g(this,"Please wait while we do things.");
try{
	progressWin.ShowDialog();
}
catch(Exception ex){
	//do something, or possibly pass ex to a second method to do more.
	//usually, it's best to just return here instead of checking IsSuccess further down
}
if(progressWin.IsCancelled){
	return;//or similar to gracefully handle when they click Cancel
}
//do any cleanup that should be done regardless of an exception
return progressWin.IsSuccess;//If the calling class wants to know if success, for example.

Example Chained Series with common log=======================================================================================================
//this pattern will probably not be used much, but it could come in handy.
ProgressWin progressWin=new ProgressWin();
progressWin.HasHistory=true;
progressWin.HistoryClose=true;
progressWin.ActionMain=() => { 
	ODEvent.Fire(ODEventType.ProgressBar,"Doing first thing");
	//do something
};
progressWin.ShowDialog();
if(progressWin.IsCancelled){
	return;
}
string progressMsg=progressWin.HistoryMsg;
//some intermittent code here, which is why we didn't just have one progress bar in the first place.
progressWin=new ProgressWin();
progressWin.HasHistory=true;
progressWin.HistoryMsg=progressMsg;
progressWin.ActionMain=() => { 
	ODEvent.Fire(ODEventType.ProgressBar,"Doing second thing");
	//do something
};
//dlg will not close here because HistoryClose is not set true
progressWin.ShowDialog();
if(progressWin.IsCancelled){
	return;
}	

Cancel==============================================================================================================================
Cancel no longer aborts the background thread (Thread.Abort throws PlatformNotSupportedException on .NET 5 and later). When the user clicks
Cancel (or X, where allowed), the window closes as cancelled and IsCancelled is true, as before, and the action is asked to stop:
-ProgressWin.CancellationToken is cancelled. A long action should check it, e.g. progressWin.CancellationToken.ThrowIfCancellationRequested(),
 or pass it to the methods it calls.
-ActionCancel is invoked, if set (for an action that waits on something of its own).
-A progress event (ODEvent.Fire with a string or a ProgressBarHelper) fired by the action after the cancel throws OperationCanceledException,
 so an action that reports progress stops at its next report.
-No database command starts for the action after the cancel (S3a-1b): the token is the worker thread's ODCancellation token, which the
 database layer (DataConnection) and the Middle Tier client (RemotingClient) check before every command or request, so the action's next
 query throws OperationCanceledException. The action's ODThread.RunParallel pools share the token: they start no further action, and
 RunParallel then throws on the worker. Other threads the action starts do not get the token.
-Loops that write files or call a vendor check ODCancellation.ThrowIfCancellationRequested() (or this token) before each file or call.
-A write and what belongs to it (its security log entry, the claim that records what a vendor was sent) can be run as one unit with
 ODCancellation.RunUncancellable(() => ...): a cancel then stops the action before or after the unit, never in its middle.
The window waits a moment (about a second) for the action to stop. If it has not, IsWorkerStillRunning is true and a line is logged
(ODCancellation.Log): the action runs on in the background to its next cancellation point or its end, its result is dropped, and an
exception it throws is logged. Either way no database write starts after the cancel, only the one under way (or the rest of a unit
under way) finishes. An action whose writes must not stop part-way at all runs without a Cancel button (ShowCancelButton=false).
Read CancellationToken on the action's own thread (or pass it on): the property gives each run of ShowDialog its own token, also to an
earlier run's action that is still running after a cancel.
A ProgressWin shown again after a cancel makes its new token in ShowDialog: read before that (on the caller's thread, between the
two ShowDialogs), CancellationToken still gives the earlier run's token, which is cancelled. So read it inside ActionMain (or in
ActionCancel), never before ShowDialog to hand it on; or use a new ProgressWin for each run (S3a-1c).

Events==============================================================================================================================
Inside of your background thread, you can fire global events:
ODEvent.Fire(ODEventType.ProgressBar,"Done with step "+i.ToString());
or
ProgressBarHelper progressBarHelper=new ProgressBarHelper("Done with step "+i.ToString(),blockValue:i,blockMax:100);
ODEvent.Fire(ODEventType.ProgressBar,progressBarHelper);

General Guidelines for moving to this new progress bar==============================================================================
There are two main considerations for this transition: 
1. Handle Cancel (described in examples above). This is a new feature that the old progress bar did not have.
2. Shrink the scope (discussed below).
The old progress bar is probably including far too much code within the action. This is a bad pattern.  
The first step is usually to identify the smallest and slowest section of code, usually one line, but sometimes a lot more.
This very small section of code should not include any UI.
If it makes a call to a method, you must examine all of the code in that method to ensure it contains no UI.
Once you've identified the line that needs to be wrapped, you can proceed to code for the new progress bar and remove the old one.
If UI code is intermingled with code that requires a progress bar, then you may end up with a few more progress bars.
They don't need to be chained together as in the example above.  They can simply be a series of individual progress bars.
A series of progress bars is not at all annoying, as long as they each automatically close on their own.
Use wait cursors for everything else:
-if the wait is generally under 5 seconds
-You want to completely lock the user out (of a short action) because the action is impossible to clean up if cancelled.
-UI manipulation and dialogs.

*/

	///<summary>Replacement for ODProgress and ProgressOD. Uses WPF.  Create an instance, set ActionMain, and ShowDialog.  Shows a progress bar dialog in the main thread, and runs the action in a background thread.  Once the long action has completed, then the progress window will automatically close.  User can Cancel at any time: the window closes as cancelled and the action is asked to stop (see "Cancel" at the top of this file); it is not aborted.  Sample boilerplate is at top of this file.</summary>
	public class ProgressWin{
		///<summary>This action can include computations, db calls, etc.  It can read from UI elements, but should not write to them because it's on a different thread.</summary>
		public Action ActionMain=null;
		///<summary>Invoked on the main thread when the user cancels while ActionMain runs, before the window waits for the action to stop. Used by a few actions that wait on a CancellationTokenSource of their own.</summary>
		public Action ActionCancel=null;
		///<summary>True if the user cancelled and ActionMain had not stopped within about a second. The action runs on in the background to its end; whatever it returns or throws then is dropped. IsCancelled is true as well.</summary>
		public bool IsWorkerStillRunning;
		///<summary>Indicates that the progress form is in "history" mode which will show a text box with all messages it takes action on and then will stay open, forcing the user to click Close.</summary>
		public bool HasHistory=false;
		///<summary>Used with HasHistory and HistoryMsg.  Set to true to close a progress even though it does have history.</summary>
		public bool ForceCloseHistory=false;
		///<summary>Used to chain progress windows together with a common history log.  Use with HasHistory and HistoryClose.  Set this to prepopluate the log and also to get the generated log.</summary>
		public string HistoryMsg="";
		///<summary>Indicates user clicked cancel. Very common to test this.  Cancel button always shows, so always account for cancel even if you don't explicitly test this.</summary>
		public bool IsCancelled;
		///<summary>Very rarely used.  Normally use IsCancelled instead.  True if the action was successful.  False if there was an exception or the user clicked Cancel.  So you would only used this if you also had a try/catch.  This would be used at the end, after the try/catch, and usually after testing for IsCancelled.</summary>
		public bool IsSuccess;
		///<summary>Default is a continuous indeterminate marquee appearance.  Setting IsBlocks to true allows you to manually control the progress out of 100%. See the example above for how to fire an event to advance the progress.</summary>
		public bool IsBlocks=false;
		///<summary>Always true except in 7 places: db update, PayConnect, EServicesSetup, clock in/out, FormMessagingButSetup, and FormMedLabEdit. (Jordan-I don't remember approving phone number synch)  Do not set to false anywhere else.</summary>
		public bool ShowCancelButton=true;
		///<summary>Shows "Please Wait..." by default, unless you set something else here.</summary>
		public string StartingMessage="Please Wait...";
		public string StopNotAllowedMessage="Not allowed to stop because it would corrupt the database.";
		///<summary>Set this string to show a warning message when the user attempts to cancel. Do necessary translations before setting this. It should follow a format similar to this: "You should not Cancel because ... Cancel anyway?" They will get a yes/no msgbox.</summary>
		public string MessageCancel;
		///<summary>If this is set to true, a 3 second sleep will be added to the Action, giving you time to click Cancel during testing.  In case you forget to remove it when you're done, it only works in Debug.  It's totally harmless to leave it in place permanently.</summary>
		public bool TestSleep;
		///<summary>Cancelled when the user cancels while ActionMain runs. A new one for each ShowDialog after a cancel.</summary>
		private CancellationTokenSource _cancellationTokenSource=new CancellationTokenSource();
		///<summary>Every source this ProgressWin has given a run of ShowDialog, the current one included. An earlier run's action that still runs after its cancel keeps reading its own (cancelled) source through CancellationToken, not the next run's (S3a-1b). Locked, because the worker threads read it.</summary>
		private List<CancellationTokenSource> _listCancellationTokenSources=new List<CancellationTokenSource>();

		public ProgressWin(){
			//no parameters
		}

		///<summary>Cancelled when the user cancels while ActionMain runs. ActionMain can check it (IsCancellationRequested, ThrowIfCancellationRequested) or pass it on, to stop early; the window does not abort the action. Read on the thread of an action of this ProgressWin, it is that run's token: after a cancel and a second ShowDialog, the first action, if it still runs, sees its own cancelled token and the second action a new one. Read on another thread after a cancel and before the next ShowDialog, it is still the cancelled token of the earlier run (see "Cancel" at the top of this file).</summary>
		public CancellationToken CancellationToken {
			get {
				CancellationToken cancellationTokenThread=ODCancellation.Current;//the token FrmProgressAuto set on the worker thread of a run
				lock(_listCancellationTokenSources) {
					if(cancellationTokenThread.CanBeCanceled && _listCancellationTokenSources.Any(x => x.Token==cancellationTokenThread)) {
						return cancellationTokenThread;
					}
					return _cancellationTokenSource.Token;
				}
			}
		}

		///<summary>This can be surrounded with a try/catch.</summary>
		public void ShowDialog(){
			if(ActionMain==null) {
				return;
			}
			lock(_listCancellationTokenSources) {
				if(_cancellationTokenSource.IsCancellationRequested) {
					//Shown again after a cancel. The earlier source is kept (not disposed): an earlier action that is still running reads it through
					//CancellationToken on its own thread.
					_cancellationTokenSource=new CancellationTokenSource();
				}
				if(!_listCancellationTokenSources.Contains(_cancellationTokenSource)) {
					_listCancellationTokenSources.Add(_cancellationTokenSource);
				}
			}
			WpfControls.UI.FrmProgressAuto frmProgressAuto=new WpfControls.UI.FrmProgressAuto();
			frmProgressAuto.ActionMain=ActionMain;
			frmProgressAuto.CancellationTokenSource=_cancellationTokenSource;
			frmProgressAuto.HasHistory=HasHistory;
			frmProgressAuto.HistoryClose=ForceCloseHistory;
			frmProgressAuto.HistoryMsg=HistoryMsg;
			frmProgressAuto.IsBlocks=IsBlocks;
			frmProgressAuto.ShowCancelButton=ShowCancelButton;
			frmProgressAuto.StartingMessage=StartingMessage;
			frmProgressAuto.StopNotAllowedMessage=StopNotAllowedMessage;
			frmProgressAuto.MessageCancel=MessageCancel;
			frmProgressAuto.TestSleep=TestSleep;
			//Set the CancelAction callback to be executed when the cancel button is clicked
			frmProgressAuto.CancelAction=ActionCancel;
			frmProgressAuto.ShowDialog();
			IsWorkerStillRunning=frmProgressAuto.IsWorkerStillRunning;
			if(frmProgressAuto.ExceptionGenerated!=null){
				//leave both of the below false
			}
			else if(frmProgressAuto.IsDialogOK){
				IsSuccess=true;
			}
			else {
				IsCancelled=true;
			}
			HistoryMsg=frmProgressAuto.HistoryMsg;
			if(frmProgressAuto.ExceptionGenerated!=null){
				if(frmProgressAuto.ExceptionGenerated.InnerException!=null){
					ExceptionDispatchInfo exceptionDispatchInfo=
						ExceptionDispatchInfo.Capture(frmProgressAuto.ExceptionGenerated.InnerException);//this can happen when methods are invoked by reflection
					exceptionDispatchInfo.Throw();
				}
				ExceptionDispatchInfo exceptionDispatchInfo2=ExceptionDispatchInfo.Capture(frmProgressAuto.ExceptionGenerated);
				exceptionDispatchInfo2.Throw();
			}
		}


	}
}

