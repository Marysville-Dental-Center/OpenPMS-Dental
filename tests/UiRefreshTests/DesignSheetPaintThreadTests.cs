// The two-thread paint test of the UI refresh's tokens (package UI-1, docs/specs/ui-refresh-plan.md section 3.2 "Thread safety",
// docs/port-notes/UI-1.md): GDI+ pens and brushes are not thread-safe, and the program runs some windows on their own UI thread (progress
// windows, the query monitor), so UiTheme hands each thread its own cached pens and brushes. Two self-drawn test controls paint with the token
// pens and brushes on two STA threads at the same time, many times, without "Object is currently in use elsewhere". Never shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UiRefreshTests {
	[TestClass]
	public class DesignSheetPaintThreadTests {
		private const int COUNT_PAINTS=3000;

		[TestMethod]
		public void TokenPens_TwoStaThreadsPaintAtOnce_WithoutAnException() {
			Barrier barrier=new Barrier(2);
			Exception[] arrayExceptions=new Exception[2];
			Pen[] arrayPens=new Pen[2];
			SolidBrush[] arrayBrushes=new SolidBrush[2];
			int[] arrayCountPaints=new int[2];
			Thread[] arrayThreads=new Thread[2];
			for(int t=0;t<2;t++) {
				int index=t;
				arrayThreads[t]=new Thread(() => {
					try {
						using(TestTokenButton testTokenButton=new TestTokenButton())
						using(TestTokenGridRow testTokenGridRow=new TestTokenGridRow())
						using(Font font=new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY))
						using(Bitmap bitmap=new Bitmap(200,60))
						using(Graphics g=Graphics.FromImage(bitmap)) {
							testTokenButton.Size=new Size(90,26);
							testTokenGridRow.Size=new Size(200,17);
							//Both threads start painting at the same moment.
							barrier.SignalAndWait();
							for(int i=0;i<COUNT_PAINTS;i++) {
								testTokenButton.PaintTokens(g,font,i%4);
								testTokenGridRow.PaintTokens(g,font,i%4);
								arrayCountPaints[index]++;
							}
							arrayPens[index]=UiTheme.GetPen(EnumUiColor.Stroke);
							arrayBrushes[index]=UiTheme.GetBrush(EnumUiColor.Accent);
							Assert.AreSame(arrayPens[index],UiTheme.GetPen(EnumUiColor.Stroke),"One pen per token on a thread.");
							Assert.AreNotSame(arrayPens[index],UiTheme.GetPen(EnumUiColor.Stroke,2),"A pen per width.");
							Assert.AreEqual(2f,UiTheme.GetPen(EnumUiColor.Stroke,2).Width);
							Assert.AreEqual(UiTheme.Stroke.ToArgb(),arrayPens[index].Color.ToArgb());
							Assert.AreEqual(255,bitmap.GetPixel(45,13).A,"The button was painted.");
							Assert.AreEqual(255,bitmap.GetPixel(150,38).A,"The row was painted.");
						}
					}
					catch(Exception exception) {
						arrayExceptions[index]=exception;
						try {
							//So the other thread does not wait at the barrier for this one.
							barrier.RemoveParticipant();
						}
						catch(Exception) {
						}
					}
				});
				arrayThreads[t].SetApartmentState(ApartmentState.STA);
				arrayThreads[t].IsBackground=true;
			}
			foreach(Thread thread in arrayThreads) {
				thread.Start();
			}
			foreach(Thread thread in arrayThreads) {
				Assert.IsTrue(thread.Join(TimeSpan.FromMinutes(5)),"A painting thread did not finish.");
			}
			for(int t=0;t<2;t++) {
				if(arrayExceptions[t]!=null) {
					Assert.Fail("Thread "+t+" after "+arrayCountPaints[t]+" paints: "+arrayExceptions[t]);
				}
				Assert.AreEqual(COUNT_PAINTS,arrayCountPaints[t]);
			}
			Assert.AreNotSame(arrayPens[0],arrayPens[1],"Each thread has its own pens.");
			Assert.AreNotSame(arrayBrushes[0],arrayBrushes[1],"Each thread has its own brushes.");
		}

		[TestMethod]
		public void TokenPens_WidthOutOfRange_Throws() {
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => UiTheme.GetPen(EnumUiColor.Stroke,0));
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => UiTheme.GetPen(EnumUiColor.Stroke,UiTheme.PEN_WIDTH_MAX+1));
			Assert.AreEqual((float)UiTheme.PEN_WIDTH_MAX,UiTheme.GetPen(EnumUiColor.Accent,UiTheme.PEN_WIDTH_MAX).Width);
		}

		///<summary>A self-drawn button painted from the tokens in four states (section 3.6): rest, hover, pressed, focused.</summary>
		private class TestTokenButton : Control {
			public void PaintTokens(Graphics g,Font font,int state) {
				g.SmoothingMode=SmoothingMode.AntiAlias;
				g.PixelOffsetMode=PixelOffsetMode.Half;
				EnumUiColor uiColorFill=state==1 ? EnumUiColor.ButtonHover : (state==2 ? EnumUiColor.ButtonPressed : EnumUiColor.SurfaceRaised);
				Rectangle rectangle=new Rectangle(0,0,Width,Height);
				using(GraphicsPath graphicsPath=UiTheme.GetRoundedRectanglePath(rectangle,UiTheme.CORNER_CONTROL)) {
					g.FillPath(UiTheme.GetBrush(uiColorFill),graphicsPath);
				}
				using(GraphicsPath graphicsPath=UiTheme.GetRoundedRectanglePath(UiTheme.GetStrokeRectangle(rectangle,1),UiTheme.CORNER_CONTROL)) {
					g.DrawPath(UiTheme.GetPen(EnumUiColor.Stroke),graphicsPath);
				}
				if(state==3) {
					using(GraphicsPath graphicsPath=UiTheme.GetRoundedRectanglePath(UiTheme.GetStrokeRectangle(rectangle,UiTheme.LINE_FOCUS),UiTheme.CORNER_CONTROL)) {
						g.DrawPath(UiTheme.GetPen(EnumUiColor.Accent,UiTheme.LINE_FOCUS),graphicsPath);
					}
				}
				g.DrawString("Save",font,UiTheme.GetBrush(EnumUiColor.Text),8,6);
			}
		}

		///<summary>A self-drawn grid row painted from the tokens: rest, hover, selected with focus, selected without focus (section 3.2's bar).</summary>
		private class TestTokenGridRow : Control {
			public void PaintTokens(Graphics graphics,Font font,int state) {
				Rectangle rectangle=new Rectangle(0,30,Width,Height);
				EnumUiColor uiColorFill=state==0 ? EnumUiColor.SurfaceRaised : (state==1 ? EnumUiColor.RowHover : EnumUiColor.AccentSubtle);
				graphics.FillRectangle(UiTheme.GetBrush(uiColorFill),rectangle);
				graphics.DrawLine(UiTheme.GetPen(EnumUiColor.Gridline),rectangle.Left,rectangle.Bottom-1,rectangle.Right,rectangle.Bottom-1);
				if(state>=2) {
					graphics.FillRectangle(UiTheme.GetBrush(state==2 ? EnumUiColor.Accent : EnumUiColor.StrokeStrong),rectangle.X,rectangle.Y,UiTheme.LINE_SELECTION_BAR,
						rectangle.Height);
				}
				graphics.DrawString("Row text",font,UiTheme.GetBrush(state>=2 ? EnumUiColor.TextSelected : EnumUiColor.Text),4,31);
			}
		}
	}
}
