using System;
using System.Drawing;
using SparksToothChart;

namespace OpenPmsDental{
	///<summary>The Patient Dashboard's wait for the Chart module's picture that the WPF chart makes in the background (package TC-3b,
	///docs/specs/wpf-tooth-chart-plan.md). It keeps the one request whose picture the dashboard still wants, and decides when the picture arrives
	///whether the dashboard may show it. UserControlDashboard calls it on its UI thread: Wait for every ModuleSelected event that names a patient,
	///Clear when the dashboard refreshes or closes. A picture is handed to the dashboard only while its request is still the one waited for, the
	///dashboard is open and the patient the picture was made for is the current patient, so the dashboard never shows another patient's chart, nor a
	///picture older than a chart it got after the picture's event. Chart code for the caller, kept here with the relay (TC-2's rule).</summary>
	public class ToothChartDashboardWait{
		private readonly Func<bool> _funcIsOpen;
		private readonly Func<long> _funcGetPatNumCur;
		private readonly Action<long,Bitmap> _actionShow;
		///<summary>Guards _toothChartBitmapRequest, so a refresh started off the UI thread may end the wait too. Never held while the picture is shown.</summary>
		private readonly object _lock=new object();
		private ToothChartBitmapRequest _toothChartBitmapRequest;

		///<param name="funcIsOpen">True while the dashboard is open.</param>
		///<param name="funcGetPatNumCur">The current patient (FormOpenDental.PatNumCur).</param>
		///<param name="actionShow">Shows the patient's picture on the dashboard, on the UI thread. It owns the bitmap and disposes it.</param>
		public ToothChartDashboardWait(Func<bool> funcIsOpen,Func<long> funcGetPatNumCur,Action<long,Bitmap> actionShow){
			_funcIsOpen=funcIsOpen;
			_funcGetPatNumCur=funcGetPatNumCur;
			_actionShow=actionShow;
		}

		///<summary>The request whose picture the dashboard waits for, or null.</summary>
		public ToothChartBitmapRequest RequestWaitedFor{
			get{
				lock(_lock){
					return _toothChartBitmapRequest;
				}
			}
		}

		///<summary>For every ModuleSelected event the dashboard gets that names a patient (DashToothChart.SetData ignores one without), on the UI
		///thread. An event with a picture request (a refill of the WPF chart, ControlChart.FillToothChart) starts the wait for it, in place of any older
		///one. An event with a picture (the other engines) ends the wait, because its picture is newer. An event with neither (the other modules, a
		///refill dated before today) leaves the wait, as such an event always left the chart the dashboard had.</summary>
		public void Wait(long patNum,ToothChartBitmapRequest toothChartBitmapRequest,bool hasPicture){
			if(toothChartBitmapRequest!=null){
				lock(_lock){
					_toothChartBitmapRequest=toothChartBitmapRequest;
				}
				toothChartBitmapRequest.SetActionDone(bitmap => Receive(toothChartBitmapRequest,patNum,bitmap));
				return;
			}
			if(hasPicture){
				Clear();
			}
		}

		///<summary>Ends the wait without a picture: the dashboard refreshes (DashToothChart.RefreshData makes its own chart, for a patient change too) or
		///closes.</summary>
		public void Clear(){
			lock(_lock){
				_toothChartBitmapRequest=null;
			}
		}

		///<summary>On the UI thread, when the picture arrives: hands it to the dashboard when its request is still the one waited for, the dashboard is
		///open and the picture's patient is the current patient; otherwise disposes it.</summary>
		private void Receive(ToothChartBitmapRequest toothChartBitmapRequest,long patNum,Bitmap bitmap){
			bool isWaitedFor;
			lock(_lock){
				isWaitedFor=toothChartBitmapRequest==_toothChartBitmapRequest;
				if(isWaitedFor){
					_toothChartBitmapRequest=null;
				}
			}
			if(!isWaitedFor || !_funcIsOpen() || patNum!=_funcGetPatNumCur()){
				bitmap.Dispose();
				return;
			}
			_actionShow(patNum,bitmap);
		}
	}
}
