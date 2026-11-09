using System;
using System.Drawing;
using SparksToothChart.Wpf3D;

namespace SparksToothChart {
	///<summary>A picture of the chart asked for without waiting (ToothChartWrapper.RequestBitmapInBackground, package TC-3b of
	///docs/specs/wpf-tooth-chart-plan.md): the WPF chart's renderer makes it on its own thread from the scene of the wrapper's data at the time of the
	///request, the same picture GetBitmap returns at that time (the same renderer, size, colours and scene), and the wrapper hands it to ActionDone on
	///its own UI thread when it arrives. Only the newest request of a wrapper is made: a newer one drops it, and so does the wrapper when it is
	///disposed or leaves the WPF engine. A dropped request never calls its action. Used on the wrapper's UI thread only.</summary>
	public sealed class ToothChartBitmapRequest {
		private Action<Bitmap> _actionDone;
		private bool _isDone;
		private bool _isDropped;
		///<summary>The scene the picture is made of, built from the wrapper's data when the request was made.</summary>
		internal readonly ToothScene Scene;

		internal ToothChartBitmapRequest(ToothScene scene) {
			Scene=scene;
		}

		///<summary>True once the picture arrived and was handed to the action (or disposed, when there was none).</summary>
		public bool IsDone {
			get {
				return _isDone;
			}
		}

		///<summary>True when no picture will come: a newer request of the same wrapper replaced this one, or the wrapper was disposed or left the WPF
		///engine before the picture arrived.</summary>
		public bool IsDropped {
			get {
				return _isDropped;
			}
		}

		///<summary>The size of the picture: the wrapper's size when the request was made.</summary>
		public Size Size {
			get {
				return Scene.Size;
			}
		}

		///<summary>Sets what gets the picture when it arrives, on the wrapper's UI thread; the action owns the bitmap and disposes it. Set it before the
		///UI message that made the request ends: the picture never arrives sooner, and a picture that arrives with no action is disposed. An action
		///set on a request that is done or dropped is never called.</summary>
		public void SetActionDone(Action<Bitmap> actionDone) {
			if(_isDone || _isDropped) {
				return;
			}
			_actionDone=actionDone;
		}

		///<summary>On the wrapper's UI thread: hands the picture to the action, once. What the action throws is logged once and goes no further, so the
		///wrapper's next request still starts.</summary>
		internal void Complete(Bitmap bitmap) {
			if(_isDone || _isDropped) {
				bitmap?.Dispose();
				return;
			}
			_isDone=true;
			Action<Bitmap> actionDone=_actionDone;
			_actionDone=null;
			if(actionDone==null || bitmap==null) {
				bitmap?.Dispose();
				return;
			}
			try {
				actionDone(bitmap);
			}
			catch(Exception ex) {
				ToothChartWrapper.LogFallbackOnce(ToothChartFallbackKind.Wpf3DRenderThread,"The WPF tooth chart's picture made in the background could not be "
					+"handed on ("+ex.GetType().Name+": "+ex.Message+").");
			}
		}

		///<summary>No picture will come for this request; its action is let go and never called. Nothing once it is done.</summary>
		internal void Drop() {
			if(_isDone) {
				return;
			}
			_isDropped=true;
			_actionDone=null;
		}
	}
}
