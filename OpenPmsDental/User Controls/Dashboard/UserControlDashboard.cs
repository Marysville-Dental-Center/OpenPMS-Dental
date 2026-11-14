using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	public partial class UserControlDashboard:UserControl {
		private ODThread _threadRefresh;
		private bool _isLoggingOff=false;
		private Action _actionDashboardContentsChanged;
		private Action _actionDashboardClosing;
		private bool _isInitialized;
		private const string PATIENT_DASHBOARD_LOG_DIRECTORY="PatientDashboard";
		private const string REFRESH_THREAD_NAME="DashboardRefresh_Thread";
		private const string SET_THREAD_NAME="DashboardSet_Thread";
		private const string TOOTH_CHART_IMAGE_THREAD_NAME="DashboardToothChartImage_Thread";
		///<summary>The wait for the Chart module's picture that the WPF chart makes in the background (TC-3b): started and replaced by SetData, ended by
		///a refresh or the close, and shown by ShowToothChartImage when the picture arrives for the current patient.</summary>
		private readonly ToothChartDashboardWait _toothChartDashboardWait;
		public LayoutManagerForms LayoutManager=new LayoutManagerForms();

		public List<UserControlDashboardWidget> ListOpenWidgets {
			get {
				return Controls.OfType<UserControlDashboardWidget>().ToList();
			}
		}

		///<summary>The width of the widest open UserControlDashboardWidget.</summary>
		public int WidgetWidth {
			get {
				if(ListOpenWidgets.IsNullOrEmpty()) {
					return 0;
				}
				return ListOpenWidgets.Max(x => x.Width);
			}
		}

		public bool IsInitialized {
			get{
				return _isInitialized;
			}
			private set {
				_isInitialized=value;
				//Unsubscribe first to avoid duplicate subscriptions, or subscription "leaks".
				ODEvent.Fired-=PatientEvent_Fired;
				ODEvent.Fired-=PatientChangedEvent_Fired;
				ODEvent.Fired-=PatientDashboardDataEvent_Fired;
				if(_isInitialized){
					ODEvent.Fired+=PatientEvent_Fired;
					ODEvent.Fired+=PatientChangedEvent_Fired;
					ODEvent.Fired+=PatientDashboardDataEvent_Fired;
				}
			}
		}

		public bool IsSettingData { get; private set; }

		public UserControlDashboard() {
			InitializeComponent();
			ContextMenuStrip=contextMenu;
			_toothChartDashboardWait=new ToothChartDashboardWait(() => IsInitialized && !IsDisposed,() => FormOpenDental.PatNumCur,ShowToothChartImage);
		}

		/// <summary>Initializes a Dashboard.</summary>
		/// <param name="sheetDef">SheetDef for the Patient Dashboard to open.</param>
		/// <param name="actionDashboardContentsChanged">Action is fired when the contents of the Dashboard change.</param>
		/// <param name="actionDashboardClosing">Action is fired when the Dashboard is closing.</param>
		/// in the Appointment Edit window. </param>
		public void Initialize(SheetDef sheetDef,Action actionDashboardContentsChanged,Action actionDashboardClosing) {
			if(sheetDef==null) {
				return;
			}
			AddWidget(sheetDef);
			_actionDashboardContentsChanged=actionDashboardContentsChanged;
			_actionDashboardClosing=actionDashboardClosing;
			//Failed to open any of the previously attached widgets, likely due to user no longer having permissions to widgets.
			//Continue if initializing Dashboard for the first time so that a widget can be added later.
			if(ListOpenWidgets.Count==0) {
				CloseDashboard(false);
				return;
			}
			IsInitialized=true;//Subscribes to relevant events.
			_actionDashboardContentsChanged?.Invoke();//Only after the Patient Dashboard SheetDef is added.
			StartRefreshThread();
		}

		private void PatientEvent_Fired(ODEventArgs e) {
			if(e.EventType!=ODEventType.Patient){
				return;
			}
			if(((e.Tag as Patient)?.PatNum??-1)==FormOpenDental.PatNumCur) {
				RefreshDashboard();
			}
		}

		private void PatientChangedEvent_Fired(ODEventArgs e) {
			if(e.EventType!=ODEventType.Patient){
				return;
			}
			RefreshDashboard();
		}

		private void PatientDashboardDataEvent_Fired(ODEventArgs e) {
			if(e.EventType!=ODEventType.ModuleSelected){
				return;
			}
			if(e==null || e.Tag==null) {
				return;
			}
			SetData(e);
		}

		private void UserControlDashboardWidget_RefreshClicked(UserControlDashboardWidget sender,EventArgs e) {
			Cursor=Cursors.WaitCursor;
			RefreshDashboard();//this spawns a new thread, so reluctant to wrap it in a progress bar.
			Cursor=Cursors.Default;
		}

		///<summary>Causes the refresh thread to either start, or wakeup and refresh the data in the background and refresh the UI on the main thread.</summary>
		public void RefreshDashboard() {
			StartRefreshThread();
		}

		private void StartRefreshThread() {
			//The refresh makes the dashboard's own tooth chart (DashToothChart.RefreshData), newer than a Chart module picture still being made.
			_toothChartDashboardWait.Clear();
			StartRefreshThread((widget) => widget.TryRefreshData(),REFRESH_THREAD_NAME);
		}

		private void StartRefreshThread(Func<UserControlDashboardWidget,bool> funcData,string name,Action postProcess=null) {
			ODThread previousThread=null;
			if(_threadRefresh!=null && !_threadRefresh.HasQuit) {
				previousThread=_threadRefresh;
			}
			_threadRefresh=new ODThread((o) => {
				if(previousThread!=null) {
					//Allow the previous thread to finish updating its data, but quit before updating UI if it hasn't started yet.
					previousThread.QuitSync(Timeout.Infinite);
				}
				foreach(UserControlDashboardWidget widget in ListOpenWidgets) {
					if(funcData(widget)) {
						if(widget.IsDisposed || !widget.IsHandleCreated) {
							continue;
						}
						if(!o.HasQuit) {
							widget.RefreshView();//Invokes back to UI thread for UI update.
						}
					}
				}
			});
			_threadRefresh.Name=name;
			_threadRefresh.AddExceptionHandler(ex => ex.DoNothing());//Don't crash program on Dashboard data fetching failure.
			_threadRefresh.AddExitHandler(new ODThread.WorkerDelegate((o) => postProcess?.Invoke()));
			_threadRefresh.Start();
		}

		private void SetData(ODEventArgs e) {
			//A Chart module refill of the WPF chart brings a request for its picture instead of the picture (TC-3b): the dashboard waits for it. An
			//event with a picture ends the wait, and one with neither leaves it (ToothChartDashboardWait). An event without a patient is ignored, as
			//DashToothChart.SetData ignores it.
			PatientDashboardDataEventArgs dataEvent=e.Tag as PatientDashboardDataEventArgs;
			if(dataEvent!=null && dataEvent.Pat!=null) {
				_toothChartDashboardWait.Wait(dataEvent.Pat.PatNum,dataEvent.ImageToothChartRequest,dataEvent.ImageToothChart!=null);
			}
			//Instantiate the new PatientDashboardEventArgs on the main thread (ctor will create copies of Bitmaps, which cannot happen on a background thread
			//as this has caused cross-threaded exceptions in the Image Module.)
			PatientDashboardDataEventArgs data=new PatientDashboardDataEventArgs(e.Tag);
			StartRefreshThread((widget) => widget.TrySetData(data),SET_THREAD_NAME
				,() => ODException.SwallowAndLogAnyException(PATIENT_DASHBOARD_LOG_DIRECTORY,() => data?.Dispose()));
		}

		///<summary>The Chart module's picture that the WPF chart made in the background has arrived for the current patient (ToothChartDashboardWait),
		///on the UI thread. It is set on the dashboard's own refresh thread, which runs after every set and refresh started before it, as the event's
		///picture was set; the patient is checked again there, and the widgets are redrawn as after every set. The bitmap is disposed when that thread
		///ends.</summary>
		private void ShowToothChartImage(long patNum,Bitmap bitmap) {
			StartRefreshThread((widget) => {
					DashToothChart.SetImageInWidget(widget,patNum,FormOpenDental.PatNumCur,bitmap);
					return widget.SheetDefWidget!=null;//redrawn as after every set, even when the patient changed, so no earlier set's redraw is lost
				},TOOTH_CHART_IMAGE_THREAD_NAME
				,() => ODException.SwallowAndLogAnyException(PATIENT_DASHBOARD_LOG_DIRECTORY,() => bitmap.Dispose()));
		}

		///<summary>Adds a new Widget to the current Dashboard container.</summary>
		public bool AddWidget(SheetDef sheetDef) {
			if(sheetDef==null || !Security.IsAuthorized(EnumPermType.DashboardWidget,sheetDef.SheetDefNum,true)) {
				return false;
			}
			UserControlDashboardWidget widget=null;
			this.InvokeIfRequired(() => { 
				//Trying to open a widget that is already open.
				if(ListOpenWidgets.Any(x => x.SheetDefWidget.SheetDefNum==sheetDef.SheetDefNum)) {
					return;
				}
				widget=new UserControlDashboardWidget(sheetDef,LayoutManager);
				widget.WidgetClosed+=CloseWidget;
				widget.RefreshClicked+=UserControlDashboardWidget_RefreshClicked;
				widget.Anchor=((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
				if(!widget.IsHandleCreated) {
					IntPtr handle=widget.Handle;//Ensure the handle for this object is created on the Main thread.
				}
			});
			if(widget!=null && widget.Initialize()) {
				this.InvokeIfRequired(() => {
					widget.Location=new Point(0,0);
					widget.Anchor=(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left);
					Size size=widget.Size;
					//widget.Size will change on the next line, presumably because it's cross thread
					this.Controls.Add(widget);
					widget.Size=size;
					widget.BringToFront();
				});
			}
			else {//Failed to open the widget, either due to lack of permission or failure to get the widget's SheetDef.
				return false;
			}
			_actionDashboardContentsChanged?.Invoke();
			return true;
		}

		private void menuItemClose_Click(object sender,EventArgs e) {
			CloseDashboard(false);
		}

		private void MenuItemRefresh_Click(object sender,EventArgs e) {
			Cursor=Cursors.WaitCursor;
			RefreshDashboard();//this spawns a new thread, so reluctant to wrap it in a progress bar.
			Cursor=Cursors.Default;
		}

		private void CloseWidget(UserControlDashboardWidget widget,EventArgs e) {
			if(widget==null) {
				return;
			}
			Controls.Remove(widget);
			if(ListOpenWidgets.Count==0) {
				_toothChartDashboardWait.Clear();//a closed dashboard shows no picture that was still being made
				IsInitialized=false;
				_actionDashboardContentsChanged?.Invoke();
				if(!_isLoggingOff) {
					_actionDashboardClosing?.Invoke();
					_actionDashboardContentsChanged=null;
					_actionDashboardClosing=null;
				}
				_threadRefresh?.QuitSync(100);
			}
		}

		public void CloseDashboard(bool isLoggingOff) {
			if(InvokeRequired) {
				this.Invoke(() => { CloseDashboard(isLoggingOff); });
				return;
			}
			_isLoggingOff=isLoggingOff;
			_toothChartDashboardWait.Clear();//a closed dashboard shows no picture that was still being made
			if(ListOpenWidgets.Count==0) { //In case a dashboard manages to be left open with no widgets.
				IsInitialized=false;
				_actionDashboardContentsChanged?.Invoke();
				_actionDashboardClosing?.Invoke();
				_threadRefresh?.QuitSync(100);
			}
			for(int i=ListOpenWidgets.Count-1;i>=0;i--) {
				ListOpenWidgets[i].CloseWidget();
			}
			_isLoggingOff=false;
			//Clear these actions set during UserA's log on.  Otherwise, if UserB logs on and doesn't have a saved Dashboard, when UserB logs off, the 
			//actions defined for UserA (specifically, removing UserOdPrefs) will occur.
			_actionDashboardContentsChanged=null;
			_actionDashboardClosing=null;
		}
	}

}
