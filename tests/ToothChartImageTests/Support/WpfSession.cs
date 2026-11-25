// REL-0.1-FIX: the guard of the tests that need WPF to draw. A locked or disconnected remote desktop session draws nothing with WPF's
// RenderTargetBitmap (WINCHK-3; RuntimeBaselineTests.WpfRendersInThisSession reports it), so a test of the chart's pictures is inconclusive
// there, with the reason, and runs in full in a connected session. Linked into ToothChartHostTests and BINATests.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToothChartImageTests {
	public static class WpfSession {
		public const string REASON_NO_RENDER="WPF's RenderTargetBitmap draws nothing in this session (a locked or disconnected remote desktop, "
			+"WINCHK-3): the chart's pictures cannot be checked here; run the test in a connected desktop session.";

		///<summary>Inconclusive, with the reason, when WPF draws nothing in this session (CanRender); nothing otherwise, so the test runs in full
		///wherever WPF draws. whatRan names the checks that already ran before the call, if any.</summary>
		public static void RequireRendering(string whatRan=null) {
			if(!CanRender()) {
				Assert.Inconclusive(REASON_NO_RENDER+(string.IsNullOrEmpty(whatRan) ? "" : " "+whatRan));
			}
		}

		///<summary>Whether WPF's RenderTargetBitmap draws in this session: a red square rendered on an STA thread of its own and read back (the probe
		///of RuntimeBaselineTests.CanRenderWpf and WindowChecksTests' Shot.CanRenderWpf). Asked each time: a session can be connected again.</summary>
		public static bool CanRender() {
			bool canRender=false;
			Thread thread=new Thread(() => canRender=CanRenderOnThisThread());
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground=true;
			thread.Start();
			if(!thread.Join(TimeSpan.FromSeconds(60))) {
				return false;
			}
			return canRender;
		}

		private static bool CanRenderOnThisThread() {
			try {
				System.Windows.Media.Imaging.RenderTargetBitmap renderTargetBitmap=new System.Windows.Media.Imaging.RenderTargetBitmap(8,8,96,96,
					System.Windows.Media.PixelFormats.Pbgra32);
				System.Windows.Media.DrawingVisual drawingVisual=new System.Windows.Media.DrawingVisual();
				using(System.Windows.Media.DrawingContext drawingContext=drawingVisual.RenderOpen()) {
					drawingContext.DrawRectangle(System.Windows.Media.Brushes.Red,null,new System.Windows.Rect(0,0,8,8));
				}
				renderTargetBitmap.Render(drawingVisual);
				byte[] arrayPixels=new byte[8*8*4];
				renderTargetBitmap.CopyPixels(arrayPixels,8*4,0);
				int index=(4*8+4)*4;
				return arrayPixels[index+2]>200 && arrayPixels[index+3]>200;
			}
			catch(Exception) {
				return false;
			}
		}
	}
}
