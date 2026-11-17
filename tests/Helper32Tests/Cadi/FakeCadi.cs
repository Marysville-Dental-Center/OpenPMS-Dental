// A fake of Mediadent / CADI's automation object (ProgID Mediadent.Application) for the CADI tests (N64-A3, docs/port-notes/N64-A3.md): a
// DynamicObject, so the late-bound calls (dynamic) of today's code, of the program's in-process path and of the helper's Cadi module reach it as
// they reach CADI's IDispatch, by name with their arguments. It records each call with each argument's value and type, its creation and its
// release, answers OleClientWindowHandle, throws what a test sets, and posts CADI's exit notice (WM_CLOSE) to the window it was given. No COM
// class is created and CADI is never started.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace Helper32Tests.Cadi {
	internal sealed class FakeCadi:DynamicObject {
		private const uint WM_CLOSE=0x0010;
		public readonly List<string> ListCalls=new List<string>();
		///<summary>Every window handle CADI was given (OleClientWindowHandle), for the checks and the cleanup.</summary>
		public readonly List<IntPtr> ListHandles=new List<IntPtr>();
		///<summary>What OleClientWindowHandle answers (0: success).</summary>
		public int ResultClientWindowHandle=0;
		///<summary>Thrown by the creation (each time) when set.</summary>
		public Exception ExceptionCreate;
		///<summary>Thrown by the call of that name (each time).</summary>
		public readonly Dictionary<string,Exception> DictThrows=new Dictionary<string,Exception>();
		public int CountCreated;

		[DllImport("user32.dll")]
		private static extern bool IsWindow(IntPtr hWnd);
		[DllImport("user32.dll")]
		private static extern bool IsWindowVisible(IntPtr hWnd);
		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd,uint msg,IntPtr wParam,IntPtr lParam);
		[DllImport("user32.dll")]
		private static extern bool DestroyWindow(IntPtr hWnd);

		///<summary>The creation: Type.GetTypeFromProgID("Mediadent.Application") and Activator.CreateInstance, which start CADI.</summary>
		public object Create() {
			CountCreated++;
			ListCalls.Add("create Mediadent.Application");
			if(ExceptionCreate!=null) {
				throw ExceptionCreate;
			}
			return this;
		}

		///<summary>Marshal.ReleaseComObject.</summary>
		public void Release(object comObject) {
			ListCalls.Add("Marshal.ReleaseComObject("+(ReferenceEquals(comObject,this) ? "cadi" : "another object")+")");
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder,object[] args,out object result) {
			ListCalls.Add(binder.Name+"("+string.Join(",",args.Select((x,i) => Format(binder.Name,i,x)))+")");
			if(binder.Name=="OleClientWindowHandle" && args.Length>0 && args[0] is int) {
				ListHandles.Add(new IntPtr((int)args[0]));
			}
			if(DictThrows.TryGetValue(binder.Name,out Exception exception)) {
				throw exception;
			}
			result=binder.Name=="OleClientWindowHandle" ? (object)ResultClientWindowHandle : null;
			return true;
		}

		///<summary>An argument as its value and type; the window handle as what it is (a hidden window of this thread), since each run has its own.
		///</summary>
		private static string Format(string name,int index,object arg) {
			if(arg==null) {
				return "null";
			}
			if(name=="OleClientWindowHandle" && index==0 && arg is int) {
				IntPtr hWnd=new IntPtr((int)arg);
				return (IsWindow(hWnd) ? (IsWindowVisible(hWnd) ? "visible window" : "hidden window") : "no window")+":Int32";
			}
			if(arg is string) {
				return "\""+arg+"\":String";
			}
			if(arg is double) {
				return ((double)arg).ToString("G17",CultureInfo.InvariantCulture)+":Double";
			}
			return Convert.ToString(arg,CultureInfo.InvariantCulture)+":"+arg.GetType().Name;
		}

		///<summary>CADI closes: its exit notice (WM_CLOSE) to the last window it was given, as CADI posts it (sent here, so it is handled now).
		///</summary>
		public void CloseCadi() {
			ListCalls.Add("CADI exits: WM_CLOSE");
			SendMessage(ListHandles.Last(),WM_CLOSE,IntPtr.Zero,IntPtr.Zero);
		}

		///<summary>True when the last window CADI was given still exists.</summary>
		public bool IsLastWindowThere() {
			return ListHandles.Count>0 && IsWindow(ListHandles.Last());
		}

		///<summary>Destroys the windows of this fake's runs that are still there (this thread made them).</summary>
		public void DestroyWindows() {
			foreach(IntPtr hWnd in ListHandles) {
				if(IsWindow(hWnd)) {
					DestroyWindow(hWnd);
				}
			}
		}
	}
}
