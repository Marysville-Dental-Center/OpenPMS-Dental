// The "no blank image" rule of groups D and E (InProcChecks.WpfImages) with the Fluent icons of package UI-7b (docs/specs/ui-refresh-plan.md,
// section 9: the check also counts vector icons and requires non-empty geometry). Without the opt-in and without a window: the WPF IconLibrary's
// icons are laid out off screen (no rendering needed) and the rule's two helpers judge them; a shown Frm is judged the same way by D and E.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowChecksTests.InProc;

namespace WindowChecksTests {
	[TestClass]
	public class WpfImagesTests {
		private static void RunSta(Action action) {
			Exception exceptionThrown=null;
			System.Threading.Thread thread=new System.Threading.Thread(() => {
				try {
					action();
				}
				catch(Exception exception) {
					exceptionThrown=exception;
				}
			});
			thread.SetApartmentState(System.Threading.ApartmentState.STA);
			thread.Start();
			thread.Join();
			if(exceptionThrown!=null) {
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exceptionThrown).Throw();
			}
		}

		private static void Layout(System.Windows.FrameworkElement frameworkElement,double size) {
			frameworkElement.Measure(new System.Windows.Size(size,size));
			frameworkElement.Arrange(new System.Windows.Rect(0,0,size,size));
			frameworkElement.UpdateLayout();
		}

		[TestMethod]
		public void EveryWpfIcon_IsAVectorIconThatIsNotBlank() {
			RunSta(() => {
				int countIcons=0;
				foreach(WpfControls.UI.EnumIcons enumIcons in Enum.GetValues(typeof(WpfControls.UI.EnumIcons))) {
					if(enumIcons==WpfControls.UI.EnumIcons.None || !WpfControls.UI.IconLibrary.IsWPF(enumIcons)) {
						continue;
					}
					foreach(double size in new double[] { 16,22,32 }) {
						System.Windows.Controls.Grid grid=new System.Windows.Controls.Grid();
						grid.Width=size;
						grid.Height=size;
						WpfControls.UI.IconLibrary.DrawWpf(enumIcons,grid);
						Layout(grid,size);
						System.Windows.Shapes.Path path=grid.Children.OfType<System.Windows.Shapes.Path>().SingleOrDefault();
						Assert.IsNotNull(path,enumIcons+" at "+size+" DIPs is not a vector icon (a Path).");
						Assert.IsFalse(InProcChecks.IsBlankPath(path),enumIcons+" at "+size+" DIPs is blank.");
					}
					countIcons++;
				}
				Assert.AreEqual(61,countIcons,"The WPF EnumIcons, every one a vector icon: the Fluent icons (UI-7b's and, since UI-9a-1, 8 more) and, since UI-8, "
					+"Chart32G, Chart32W and ChartMed32's dental icons.");
			});
		}

		[TestMethod]
		public void BlankRule_FindsAnEmptyPathAndAnEmptyDrawingImage() {
			RunSta(() => {
				System.Windows.Shapes.Path pathEmpty=new System.Windows.Shapes.Path();
				pathEmpty.Data=System.Windows.Media.Geometry.Empty;
				pathEmpty.Width=22;
				pathEmpty.Height=22;
				Layout(pathEmpty,22);
				Assert.IsTrue(InProcChecks.IsBlankPath(pathEmpty),"A Path with an empty geometry.");
				System.Windows.Shapes.Path pathNoData=new System.Windows.Shapes.Path();
				pathNoData.Width=22;
				pathNoData.Height=22;
				Layout(pathNoData,22);
				Assert.IsTrue(InProcChecks.IsBlankPath(pathNoData),"A Path without a geometry.");
				System.Windows.Controls.Image imageIcon=new System.Windows.Controls.Image();
				imageIcon.Source=CodeBase.UiIcons.GetImageSourceWpf(CodeBase.UiGlyph.Checkmark,16,System.Windows.Media.Colors.Black);
				Layout(imageIcon,16);
				Assert.IsFalse(InProcChecks.IsBlankImage(imageIcon),"A DrawingImage of a glyph.");
				System.Windows.Controls.Image imageEmpty=new System.Windows.Controls.Image();
				imageEmpty.Source=new System.Windows.Media.DrawingImage(new System.Windows.Media.DrawingGroup());
				imageEmpty.Width=16;
				imageEmpty.Height=16;
				Layout(imageEmpty,16);
				Assert.IsTrue(InProcChecks.IsBlankImage(imageEmpty),"A DrawingImage with an empty drawing.");
				System.Windows.Controls.Image imageNoSource=new System.Windows.Controls.Image();
				Assert.IsTrue(InProcChecks.IsBlankImage(imageNoSource),"An Image without a source.");
			});
		}
	}
}
