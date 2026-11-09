using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using SparksToothChart;

namespace OpenPmsDental {
	///<summary>The Patient Dashboard's wait for the Chart module's picture that the WPF chart makes in the background (package TC-3b,
	///docs/specs/wpf-tooth-chart-plan.md). It keeps the one request whose picture the dashboard still wants, and decides when the picture arrives
	///whether the dashboard may show it. UserControlDashboard calls it on its UI thread: Wait for every ModuleSelected event, Clear when the dashboard
	///refreshes or closes. A picture is handed to the dashboard only while its request is still the one waited for, the dashboard is open and the
	///patient the picture was made for is the current patient, so the dashboard never shows another patient's chart, nor a picture older than a
	///chart it got after the picture's event.</summary>
	public class DashToothChartImageWait {
		private readonly Func<bool> _funcIsOpen;
		private readonly Func<long> _funcGetPatNumCur;
		private readonly Action<long,Bitmap> _actionShow;
		///<summary>Guards _toothChartBitmapRequest, so a refresh started off the UI thread may end the wait too. Never held while the picture is shown.</summary>
		private readonly object _lock=new object();
		private ToothChartBitmapRequest _toothChartBitmapRequest;

		///<param name="funcIsOpen">True while the dashboard is open.</param>
		///<param name="funcGetPatNumCur">The current patient (FormOpenDental.PatNumCur).</param>
		///<param name="actionShow">Shows the patient's picture on the dashboard. It owns the bitmap and disposes it.</param>
		public DashToothChartImageWait(Func<bool> funcIsOpen,Func<long> funcGetPatNumCur,Action<long,Bitmap> actionShow) {
			_funcIsOpen=funcIsOpen;
			_funcGetPatNumCur=funcGetPatNumCur;
			_actionShow=actionShow;
		}

		///<summary>The request whose picture the dashboard waits for, or null.</summary>
		public ToothChartBitmapRequest RequestWaitedFor {
			get {
				lock(_lock) {
					return _toothChartBitmapRequest;
				}
			}
		}

		///<summary>For every ModuleSelected event the dashboard gets, on the UI thread. An event with a picture request (a refill of the WPF chart) starts
		///the wait for it, in place of any older one. An event with a picture (the other engines) ends the wait, because its picture is newer. An event
		///with neither (the other modules, a refill dated before today) leaves the wait, as such an event always left the chart the dashboard had. An
		///event without a patient is ignored, as DashToothChart.SetData ignores it.</summary>
		public void Wait(PatientDashboardDataEventArgs patientDashboardDataEventArgs) {
			if(patientDashboardDataEventArgs==null || patientDashboardDataEventArgs.Pat==null) {
				return;
			}
			ToothChartBitmapRequest toothChartBitmapRequest=patientDashboardDataEventArgs.ImageToothChartRequest;
			if(toothChartBitmapRequest!=null) {
				long patNum=patientDashboardDataEventArgs.Pat.PatNum;
				lock(_lock) {
					_toothChartBitmapRequest=toothChartBitmapRequest;
				}
				toothChartBitmapRequest.SetActionDone(bitmap => Receive(toothChartBitmapRequest,patNum,bitmap));
				return;
			}
			if(patientDashboardDataEventArgs.ImageToothChart!=null) {
				Clear();
			}
		}

		///<summary>Ends the wait without a picture: the dashboard refreshes (DashToothChart.RefreshData makes its own chart, for a patient change too) or
		///closes.</summary>
		public void Clear() {
			lock(_lock) {
				_toothChartBitmapRequest=null;
			}
		}

		///<summary>On the UI thread, when the picture arrives: hands it to the dashboard when its request is still the one waited for, the dashboard is
		///open and the picture's patient is the current patient; otherwise disposes it.</summary>
		private void Receive(ToothChartBitmapRequest toothChartBitmapRequest,long patNum,Bitmap bitmap) {
			bool isWaitedFor;
			lock(_lock) {
				isWaitedFor=toothChartBitmapRequest==_toothChartBitmapRequest;
				if(isWaitedFor) {
					_toothChartBitmapRequest=null;
				}
			}
			if(!isWaitedFor || !_funcIsOpen() || patNum!=_funcGetPatNumCur()) {
				bitmap.Dispose();
				return;
			}
			_actionShow(patNum,bitmap);
		}

		///<summary>On the dashboard's refresh thread, just before the picture is set: gives a copy to every tooth chart field of the widget
		///(DashToothChart.SetImageToothChart), unless the current patient (patNumCur) is no longer the picture's (the refresh that the patient change
		///started runs next and shows the new patient's chart). True when a field took it, so the widget is redrawn.</summary>
		public static bool TrySetImage(Control controlWidget,long patNum,long patNumCur,Image image) {
			if(patNum!=patNumCur || image==null) {
				return false;
			}
			List<DashToothChart> listDashToothCharts=UIHelper.GetAllControls(controlWidget).OfType<DashToothChart>().ToList();
			for(int i=0;i<listDashToothCharts.Count;i++) {
				listDashToothCharts[i].SetImageToothChart(image);
			}
			return listDashToothCharts.Count>0;
		}
	}
}
