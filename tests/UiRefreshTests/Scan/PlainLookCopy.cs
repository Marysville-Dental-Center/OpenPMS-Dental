// UI-PLAIN-2 (docs/port-notes/UI-PLAIN.md section 8): the shared looks and the frame of a plain window, read from the copy of CodeBase's UI files
// its own assembly compiles in (RdlDesign, UpdateFileCopier: they reference no CodeBase), else from CodeBase. A window of the report designer
// attaches its own copy, whose tables CodeBase's methods cannot see, so the checks and the gallery call that copy by reflection.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UiRefreshTests.Scan {
	public static class PlainLookCopy {
		///<summary>The type CodeBase.<name> of the assembly's copy, else CodeBase's.</summary>
		public static Type GetType(Assembly assembly,string name) {
			string fullName="CodeBase."+name;
			return assembly?.GetType(fullName)??typeof(CodeBase.UiPlainWindow).Assembly.GetType(fullName,throwOnError:true);
		}

		///<summary>Calls a public static method of the copy's type with the arguments (the overload whose parameter types take them).</summary>
		public static object Call(Assembly assembly,string nameType,string nameMethod,params object[] arrayArgs) {
			Type type=GetType(assembly,nameType);
			MethodInfo methodInfo=type.GetMethods(BindingFlags.Public|BindingFlags.Static).FirstOrDefault(x => x.Name==nameMethod
				&& x.GetParameters().Length==arrayArgs.Length
				&& x.GetParameters().Select((y,i) => arrayArgs[i]==null || y.ParameterType.IsInstanceOfType(arrayArgs[i])).All(y => y));
			if(methodInfo==null) {
				throw new MissingMethodException(type.FullName,nameMethod);
			}
			try {
				return methodInfo.Invoke(null,arrayArgs);
			}
			catch(TargetInvocationException targetInvocationException) when(targetInvocationException.InnerException!=null) {
				throw targetInvocationException.InnerException;
			}
		}

		public static bool IsTrue(Assembly assembly,string nameType,string nameMethod,params object[] arrayArgs) {
			return (bool)Call(assembly,nameType,nameMethod,arrayArgs);
		}

		///<summary>The frame's scale in the gallery harness (UiPlainFrame.FuncGetScaleForTest) on CodeBase's frame and on the assembly's copy; 0
		///clears it.</summary>
		public static void SetFrameScaleForTest(Assembly assembly,float scale) {
			Func<Form,float> func=scale>0 ? (form => scale) : (Func<Form,float>)null;
			foreach(Type type in new Type[] { GetType(null,"UiPlainFrame"),GetType(assembly,"UiPlainFrame") }.Distinct()) {
				type.GetField("FuncGetScaleForTest",BindingFlags.Public|BindingFlags.Static).SetValue(null,func);
			}
		}

		///<summary>The full window with its title bar, as the screen shows it. With the program's frame (UiPlainFrame on): the window laid out as
		///its Load does (the content below the band), its client area drawn as the gallery draws it (each control printed alone, the band on the
		///window's surface) inside the 1 px border Windows 11 draws in the frame's outline colour. Without it (the before pictures): the window
		///as Windows prints it with its own frame (DrawToBitmap, the whole window), less the invisible resize borders but their last pixel.</summary>
		public static Bitmap CaptureWindow(Form form) {
			Assembly assembly=form.GetType().Assembly;
			if(IsTrue(assembly,"UiPlainFrame","IsAttached",form)) {
				Call(assembly,"UiPlainFrame","LayOutAsOnLoad",form);
			}
			//The gallery's drawing of the whole window (DesignSheetWindows.Capture: the window's own printing, its frame as Windows prints it, the
			//controls each printed at their place in the client area).
			IntPtr handle=form.Handle;
			Point pointClient=form.PointToScreen(Point.Empty);
			Rectangle rectangleClient=new Rectangle(pointClient.X-form.Left,pointClient.Y-form.Top,form.ClientSize.Width,form.ClientSize.Height);
			using(Bitmap bitmapWindow=DesignSheetWindows.Capture(form,Rectangle.Empty)) {
				if(IsTrue(assembly,"UiPlainFrame","IsOn",form)) {
					//The program's frame: the client area (the band and the content) inside the 1 px border Windows draws in the outline colour.
					Bitmap bitmap=new Bitmap(rectangleClient.Width+2,rectangleClient.Height+2,PixelFormat.Format32bppArgb);
					using(Graphics g=Graphics.FromImage(bitmap)) {
						g.Clear(GetOutline(assembly));
						g.DrawImage(bitmapWindow,new Rectangle(1,1,rectangleClient.Width,rectangleClient.Height),rectangleClient,GraphicsUnit.Pixel);
						//The band as the window's Paint draws it (printing a window that was never shown does not raise its Paint reliably).
						g.TranslateTransform(1,1);
						Call(assembly,"UiPlainFrame","PaintTitleBar",form,g);
					}
					return bitmap;
				}
				//Windows' frame: Windows 10 and 11 show 1 px of each side border; the rest is the invisible resize border (the shadow on screen).
				int left=rectangleClient.Left;
				int right=bitmapWindow.Width-rectangleClient.Right;
				int bottom=bitmapWindow.Height-rectangleClient.Bottom;
				Rectangle rectangleCrop=Rectangle.FromLTRB(Math.Max(0,left-1),0,bitmapWindow.Width-Math.Max(0,right-1),bitmapWindow.Height-Math.Max(0,bottom-1));
				return bitmapWindow.Clone(rectangleCrop,PixelFormat.Format32bppArgb);
			}
		}

		///<summary>The outline colour of the program's frame with the stock colours (UiFramePainter.GetWindowColorsStock), from the copy.</summary>
		private static Color GetOutline(Assembly assembly) {
			object frameColors=Call(assembly,"UiFramePainter","GetWindowColorsStock");
			return (Color)frameColors.GetType().GetField("Outline").GetValue(frameColors);
		}
	}
}
