// UI-PLAIN-2 (docs/port-notes/UI-PLAIN.md section 8): the program's title bar and frame on a plain window (CodeBase.UiPlainFrame, turned on by
// UiPlainWindow.Attach): the frame's messages on a real window handle that is never shown (WM_NCCALCSIZE through SWP_FRAMECHANGED,
// WM_NCHITTEST, the caption buttons' clicks), the content moved below the band, the caption text, the maximized client area, the frame
// FormODBase paints, and the cases that keep Windows' frame.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class PlainFrameTests {
		private const int WM_NCHITTEST=0x0084;
		private const int WM_NCLBUTTONDOWN=0x00A1;
		private const int WM_NCLBUTTONUP=0x00A2;

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd,int msg,IntPtr wParam,IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern int GetWindowLong(IntPtr hWnd,int nIndex);

		///<summary>A plain test window: 300 by 200 client, a button at the top left, a panel docked at the top, a list anchored to all four sides
		///and an OK button anchored to the bottom right, as a designer makes them, with the look and frame attached as a constructor does.</summary>
		private static Form MakeWindow(FormBorderStyle formBorderStyle=FormBorderStyle.Sizable) {
			Form form=new Form();
			form.FormBorderStyle=formBorderStyle;
			form.StartPosition=FormStartPosition.Manual;
			form.Location=new Point(100,100);
			form.ClientSize=new Size(300,200);
			form.Text="Plain window";
			Panel panelTop=new Panel { Name="panelTop",Dock=DockStyle.Top,Height=20 };
			Button button=new Button { Name="butTopLeft",Text="Top",Bounds=new Rectangle(10,30,75,24) };
			ListBox listBox=new ListBox { Name="listBox",Bounds=new Rectangle(10,60,200,100),Anchor=AnchorStyles.Top|AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right,IntegralHeight=false };
			Button butOK=new Button { Name="butOK",Text="OK",Bounds=new Rectangle(215,166,75,24),Anchor=AnchorStyles.Bottom|AnchorStyles.Right };
			form.Controls.Add(listBox);
			form.Controls.Add(button);
			form.Controls.Add(butOK);
			form.Controls.Add(panelTop);
			UiPlainWindow.Attach(form);
			return form;
		}

		private static int HitTest(Form form,Point pointClient) {
			Point pointScreen=form.PointToScreen(pointClient);
			IntPtr lParam=new IntPtr((pointScreen.Y<<16)|(pointScreen.X & 0xFFFF));
			return SendMessage(form.Handle,WM_NCHITTEST,IntPtr.Zero,lParam).ToInt32();
		}

		[TestMethod]
		public void Frame_ClientAreaOverTheBand_ContentMovedBelowIt() {
			StaRunner.Run(() => {
				using(Form form=MakeWindow()) {
					Assert.IsTrue(UiPlainFrame.IsAttached(form),"Attach gives the frame");
					IntPtr handle=form.Handle;
					//UI-PLAIN-2 fix round: Windows' frame until Load, so Windows Forms' scaling in Form.OnLoad sizes the window as before.
					Assert.IsFalse(UiPlainFrame.IsOn(form),"Windows' frame until the window loads");
					Assert.AreEqual(new Size(300,200),form.ClientSize);
					UiPlainFrame.LayOutAsOnLoad(form);
					Assert.IsTrue(UiPlainFrame.IsOn(form),"hooked at Load");
					//WM_NCCALCSIZE: the invisible borders left, right and below only (8 px at 96 dpi), the client area as the designer made it and the
					//band.
					Assert.AreEqual(new Point(8,0),new Point(form.PointToScreen(Point.Empty).X-form.Left,form.PointToScreen(Point.Empty).Y-form.Top),
						"the client area starts at the window's top");
					Assert.AreEqual(26,UiPlainFrame.GetContentOffset(form),"the band: 26 px at 96 dpi");
					Assert.AreEqual(new Size(300,226),form.ClientSize,"the designer's client area and the band");
					Assert.AreEqual(new Size(316,234),form.Size,"the window: the client area and the invisible borders left, right and below");
					Assert.AreEqual(26,form.Padding.Top,"the docked controls keep below the band (Padding)");
					Assert.AreEqual(new Rectangle(0,26,300,20),form.Controls["panelTop"].Bounds,"the docked panel below the band");
					Assert.AreEqual(new Rectangle(10,56,75,24),form.Controls["butTopLeft"].Bounds,"a control anchored to the top moves down by the band");
					Assert.AreEqual(new Rectangle(10,86,200,100),form.Controls["listBox"].Bounds,"a control anchored to all sides moves down, its size kept");
					Assert.AreEqual(new Rectangle(215,192,75,24),form.Controls["butOK"].Bounds,"a control anchored to the bottom keeps its distance to it");
					//A control the code adds later moves down too; one it takes off moves back.
					Label label=new Label { Name="labelLater",Bounds=new Rectangle(220,40,60,20) };
					form.Controls.Add(label);
					Assert.AreEqual(66,label.Top,"a control added later moves below the band");
					form.Controls.Remove(label);
					Assert.AreEqual(40,label.Top,"a control taken off moves back");
					label.Dispose();
					//Resizing keeps the anchors as before.
					form.Size=new Size(form.Width+40,form.Height+30);
					Assert.AreEqual(new Rectangle(10,86,240,130),form.Controls["listBox"].Bounds,"the list grows with the window");
				}
			});
		}

		///<summary>UI-PLAIN-2 fix round (the review of ui/uiplain2-1): a window that keeps Windows Forms' old AutoScale (its designer code sets only
		///AutoScaleBaseSize, as the report designer's dialogs and ODR's FormParameterInput do) is scaled in Form.OnLoad, which sets its ClientSize
		///through Windows' standard frame. With the frame turned on at Load, after that, the client area is the scaled design and the band, as a
		///window without the frame gets the scaled design: not a caption taller.</summary>
		[TestMethod]
		public void Frame_LegacyAutoScale_ClientAreaIsTheScaledDesignAndTheBand() {
			StaRunner.Run(() => {
				using(Font font=new Font("Segoe UI",9f)) {
					foreach(FormBorderStyle formBorderStyle in new[] { FormBorderStyle.Sizable,FormBorderStyle.FixedDialog }) {
						Size sizeScaled;
						using(Form formWindows=MakeLegacyAutoScaleWindow(font,formBorderStyle,isAttached:false)) {
							IntPtr handleWindows=formWindows.Handle;
							CallOnLoad(formWindows);
							sizeScaled=formWindows.ClientSize;
						}
						Assert.IsTrue(sizeScaled.Height>200,formBorderStyle+": Segoe UI 9 against AutoScaleBaseSize(5,13) grows the design: "+sizeScaled);
						using(Form form=MakeLegacyAutoScaleWindow(font,formBorderStyle,isAttached:true)) {
							IntPtr handle=form.Handle;
							Assert.IsFalse(UiPlainFrame.IsOn(form),formBorderStyle+": Windows' frame before Load");
							CallOnLoad(form);
							Assert.IsTrue(UiPlainFrame.IsOn(form),formBorderStyle+": the frame from Load");
							Assert.AreEqual(26,UiPlainFrame.GetContentOffset(form));
							Assert.AreEqual(new Size(sizeScaled.Width,sizeScaled.Height+26),form.ClientSize,formBorderStyle+": the scaled design and the band");
							Assert.AreEqual(new Size(sizeScaled.Width+16,sizeScaled.Height+26+8),form.Size,formBorderStyle+": and the invisible borders");
							Button butOK=(Button)form.Controls["butOK"];
							Assert.AreEqual(form.ClientSize.Height-butOK.Bottom,sizeScaled.Height-_bottomOKScaled,
								formBorderStyle+": the bottom-anchored button keeps its scaled distance to the bottom");
						}
					}
				}
			});
		}

		///<summary>The OK button's bottom after Load on the last window MakeLegacyAutoScaleWindow made without the frame.</summary>
		private static int _bottomOKScaled;

		///<summary>A window as the report designer's designer code makes it: the default font (Segoe UI 9, the exes' default), AutoScaleBaseSize
		///(5,13) only, a 300 by 200 client area, an OK button anchored to the bottom right.</summary>
		private static Form MakeLegacyAutoScaleWindow(Font font,FormBorderStyle formBorderStyle,bool isAttached) {
			Form form=new Form();
			form.Font=font;
#pragma warning disable CS0618 //AutoScaleBaseSize: the old AutoScale the designer code uses
			form.AutoScaleBaseSize=new Size(5,13);
#pragma warning restore CS0618
			form.FormBorderStyle=formBorderStyle;
			form.StartPosition=FormStartPosition.Manual;
			form.Location=new Point(100,100);
			form.ClientSize=new Size(300,200);
			form.Text="Legacy AutoScale";
			Button butOK=new Button { Name="butOK",Text="OK",Bounds=new Rectangle(215,166,75,24),Anchor=AnchorStyles.Bottom|AnchorStyles.Right };
			form.Controls.Add(butOK);
			if(isAttached) {
				UiPlainWindow.Attach(form);
			}
			else {
				form.Load+=(sender,e) => _bottomOKScaled=butOK.Bottom;
			}
			return form;
		}

		///<summary>Form.OnLoad as Show runs it: Windows Forms' scaling (ApplyAutoScaling), then the Load handlers.</summary>
		private static void CallOnLoad(Form form) {
			System.Reflection.MethodInfo methodInfo=typeof(Form).GetMethod("OnLoad",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
			methodInfo.Invoke(form,new object[] { EventArgs.Empty });
		}

		///<summary>UI-PLAIN-2 fix round: the band's icon is FormODBase's: the window's own icon on a normal frame, else the program icon (on a dialog
		///or tool window frame too), never Windows Forms' default icon.</summary>
		[TestMethod]
		public void Frame_Icon_TheProgramIconNeverTheDefault() {
			StaRunner.Run(() => {
				Assert.IsNotNull(UiPlainFrame.IconProgram,"CodeBase embeds the program icon (branding/app.ico)");
				using(Form form=MakeWindow()) {
					Assert.IsTrue(UiPlainFrame.IsDefaultIcon(form.Icon),"a window that set no icon has Windows Forms' default icon");
					FramePaintArgs framePaintArgs=UiPlainFrame.GetFramePaintArgs(form);
					Assert.IsTrue(framePaintArgs.ShowIcon);
					Assert.AreSame(UiPlainFrame.IconProgram,framePaintArgs.Icon,"no icon of its own: the program icon");
				}
				using(Icon iconOwn=new Icon(SystemIcons.Information,16,16))
				using(Form form=MakeWindow()) {
					form.Icon=iconOwn;
					Assert.AreSame(iconOwn,UiPlainFrame.GetFramePaintArgs(form).Icon,"its own icon on a normal frame");
					form.FormBorderStyle=FormBorderStyle.FixedDialog;
					Assert.AreSame(UiPlainFrame.IconProgram,UiPlainFrame.GetFramePaintArgs(form).Icon,"a dialog frame: the program icon, as FormODBase");
					Assert.IsTrue(UiPlainFrame.GetFramePaintArgs(form).ShowIcon,"shown on a dialog too");
				}
				using(Form form=MakeWindow(FormBorderStyle.SizableToolWindow)) {
					Assert.AreSame(UiPlainFrame.IconProgram,UiPlainFrame.GetFramePaintArgs(form).Icon,"a tool window: the program icon");
				}
			});
		}

		[TestMethod]
		public void Frame_HitTests_EdgesCaptionAndButtons() {
			StaRunner.Run(() => {
				using(Form form=MakeWindow()) {
					UiPlainFrame.LayOutAsOnLoad(form);
					FramePaintResult framePaintResult=UiFramePainter.GetButtons(UiPlainFrame.GetFramePaintArgs(form));
					Point Center(Rectangle rectangle) => new Point(rectangle.X+rectangle.Width/2-1,rectangle.Y+rectangle.Height/2-1);//client = panel - 1
					Assert.AreEqual(UiPlainFrame.HTCAPTION,HitTest(form,new Point(120,14)),"the band is the caption");
					Assert.AreEqual(UiPlainFrame.HTTOP,HitTest(form,new Point(120,1)),"the band's top 5 px resize the top");
					Assert.AreEqual(UiPlainFrame.HTTOPLEFT,HitTest(form,new Point(2,2)),"the top left corner");
					Assert.AreEqual(UiPlainFrame.HTLEFT,HitTest(form,new Point(2,14)),"the band's left 5 px resize the left");
					Assert.AreEqual(UiPlainFrame.HTCLOSE,HitTest(form,Center(framePaintResult.RectangleHitX)),"the close button");
					Assert.AreEqual(UiPlainFrame.HTMAXBUTTON,HitTest(form,Center(framePaintResult.RectangleHitMax)),"the maximize button (Windows 11's snap layouts)");
					Assert.AreEqual(UiPlainFrame.HTMINBUTTON,HitTest(form,Center(framePaintResult.RectangleHitMin)),"the minimize button");
					Assert.AreEqual(UiPlainFrame.HTCLIENT,HitTest(form,new Point(150,190)),"below the band: the client area");
					Assert.AreEqual(UiPlainFrame.HTCLIENT,HitTest(form,new Point(250,26)),"the band's height only");
					//Outside the client area Windows answers: the invisible resize borders.
					Assert.AreEqual(10,HitTest(form,new Point(-4,120)),"the left border (HTLEFT)");
					Assert.AreEqual(11,HitTest(form,new Point(form.ClientSize.Width+3,120)),"the right border (HTRIGHT)");
					Assert.AreEqual(15,HitTest(form,new Point(150,form.ClientSize.Height+3)),"the bottom border (HTBOTTOM)");
					//The buttons' right edge is the client area's: as FormODBase lays them out (the window's width less 15 px and the border).
					Assert.AreEqual(form.ClientSize.Width+1,framePaintResult.RectangleButX.Right,"the close button ends at the client area's right edge");
				}
				using(Form form=MakeWindow(FormBorderStyle.FixedDialog)) {
					form.MaximizeBox=false;
					form.MinimizeBox=false;
					UiPlainFrame.LayOutAsOnLoad(form);
					FramePaintResult framePaintResult=UiFramePainter.GetButtons(UiPlainFrame.GetFramePaintArgs(form));
					Assert.AreEqual(UiPlainFrame.HTCAPTION,HitTest(form,new Point(120,1)),"a dialog that is not resizable: no top edge, the caption");
					Assert.AreEqual(UiPlainFrame.HTCAPTION,HitTest(form,new Point(2,14)),"no left edge in the band");
					Assert.IsTrue(framePaintResult.RectangleHitMax.IsEmpty && framePaintResult.RectangleHitMin.IsEmpty,"only the close button");
					Point pointClose=new Point(framePaintResult.RectangleHitX.X+framePaintResult.RectangleHitX.Width/2-1,14);
					Assert.AreEqual(UiPlainFrame.HTCLOSE,HitTest(form,pointClose),"the close button");
					Assert.AreEqual(UiPlainFrame.HTCAPTION,HitTest(form,new Point(pointClose.X-framePaintResult.RectangleHitX.Width,14)),"no maximize button");
				}
			});
		}

		[TestMethod]
		public void Frame_CloseButtonClick_ClosesAsWindowsCloseButtonDoes() {
			StaRunner.Run(() => {
				using(Form form=MakeWindow()) {
					UiPlainFrame.LayOutAsOnLoad(form);
					FramePaintResult framePaintResult=UiFramePainter.GetButtons(UiPlainFrame.GetFramePaintArgs(form));
					bool isClosing=false;
					form.FormClosing+=(sender,e) => isClosing=true;
					Point pointScreen=form.PointToScreen(new Point(framePaintResult.RectangleHitX.X+10,14));
					IntPtr lParam=new IntPtr((pointScreen.Y<<16)|(pointScreen.X & 0xFFFF));
					SendMessage(form.Handle,WM_NCLBUTTONDOWN,new IntPtr(UiPlainFrame.HTCLOSE),lParam);
					Assert.IsFalse(isClosing,"the button down does nothing yet");
					SendMessage(form.Handle,WM_NCLBUTTONUP,new IntPtr(UiPlainFrame.HTCLOSE),lParam);
					Assert.IsTrue(isClosing,"the button up on the same button sends SC_CLOSE: FormClosing");
				}
			});
			Assert.AreEqual(0xF060,UiPlainFrame.GetSysCommand(UiPlainFrame.HTCLOSE,false));
			Assert.AreEqual(0xF020,UiPlainFrame.GetSysCommand(UiPlainFrame.HTMINBUTTON,false));
			Assert.AreEqual(0xF030,UiPlainFrame.GetSysCommand(UiPlainFrame.HTMAXBUTTON,false),"maximize");
			Assert.AreEqual(0xF120,UiPlainFrame.GetSysCommand(UiPlainFrame.HTMAXBUTTON,true),"restore on a maximized window");
			Assert.AreEqual(0,UiPlainFrame.GetSysCommand(UiPlainFrame.HTCAPTION,false));
		}

		[TestMethod]
		public void Frame_CaptionText_PaintedAndRepaintedWhenTheCodeChangesIt() {
			StaRunner.Run(() => {
				using(Form form=MakeWindow()) {
					UiPlainFrame.LayOutAsOnLoad(form);
					Rectangle rectangleBand=UiPlainFrame.GetTitleBarRectangle(form);
					Assert.AreEqual(new Rectangle(0,0,300,26),rectangleBand);
					Bitmap bitmapA=PaintBand(form);
					FrameColors frameColors=UiFramePainter.GetWindowColorsStock();
					Assert.AreEqual(frameColors.TitleBar.ToArgb(),bitmapA.GetPixel(150,20).ToArgb(),"the band in the TitleBar token");
					int countTextA=CountNear(bitmapA,frameColors.TitleBarText,new Rectangle(30,4,150,18));
					Assert.IsTrue(countTextA>20,"the title text in TitleBarText: "+countTextA);
					form.Text="A much longer caption the code sets later";
					Assert.AreEqual(form.Text,UiPlainFrame.GetFramePaintArgs(form).Text,"the painter reads the window's text");
					Bitmap bitmapB=PaintBand(form);
					int countTextB=CountNear(bitmapB,frameColors.TitleBarText,new Rectangle(30,4,150,18));
					Assert.IsTrue(countTextB>countTextA,"the new text painted: "+countTextA+" then "+countTextB);
					bitmapA.Dispose();
					bitmapB.Dispose();
				}
			});
		}

		[TestMethod]
		public void Frame_PaintsWhatFormODBasePaints() {
			//The band is UiFramePainter's frame (FormODBase's and FormFrame's painter, through FramePainter), with FormODBase's sizes: 26 px, the
			//buttons 30 by 25 from y 5, the close button's right edge 15 px left of the window's right edge.
			StaRunner.Run(() => {
				using(Form form=MakeWindow()) {
					UiPlainFrame.LayOutAsOnLoad(form);
					FramePaintArgs framePaintArgs=UiPlainFrame.GetFramePaintArgs(form);
					Assert.AreEqual(26,framePaintArgs.HeightTitleBar);
					Assert.AreEqual(15,framePaintArgs.WidthBorder);
					Assert.AreEqual(form.Width,framePaintArgs.Width,"the window's width, as FormODBase gives it");
					Assert.AreEqual(form.Height,framePaintArgs.Height);
					FramePaintResult framePaintResult=UiFramePainter.GetButtons(framePaintArgs);
					Assert.AreEqual(new Rectangle(framePaintArgs.Width-15-30,5,30,25),framePaintResult.RectangleButX);
					using(Bitmap bitmapPlain=new Bitmap(framePaintArgs.Width,framePaintArgs.Height))
					using(Bitmap bitmapProgram=new Bitmap(framePaintArgs.Width,framePaintArgs.Height)) {
						using(Graphics g=Graphics.FromImage(bitmapPlain)) {
							UiFramePainter.Paint(g,framePaintArgs,UiFramePainter.GetWindowColorsStock());
						}
						using(Graphics g=Graphics.FromImage(bitmapProgram)) {
							OpenPmsDental.FramePainter.Paint(g,framePaintArgs,OpenPmsDental.FramePainter.GetWindowColors(
								Color.FromArgb(OpenPmsDental.FramePainter.MAIN_BORDER_STOCK_ARGB),Color.Black,Color.White));
						}
						for(int y=0;y<26;y++) {
							for(int x=0;x<framePaintArgs.Width;x++) {
								Assert.AreEqual(bitmapProgram.GetPixel(x,y).ToArgb(),bitmapPlain.GetPixel(x,y).ToArgb(),"pixel "+x+","+y);
							}
						}
					}
				}
			});
		}

		[TestMethod]
		public void Frame_MaximizedClientArea_AndTheScales() {
			Rectangle rectangleWindow=new Rectangle(-8,-8,1936,1048);//a 1920 by 1032 working area, the window hanging 8 px past it
			Assert.AreEqual(new Rectangle(0,0,1920,1032),UiPlainFrame.GetClientRectangle(rectangleWindow,true,1f),"maximized: the screen's working area");
			Assert.AreEqual(new Rectangle(0,-8,1920,1040),UiPlainFrame.GetClientRectangle(rectangleWindow,false,1f),"normal: the top is the window's top");
			Assert.AreEqual(Rectangle.FromLTRB(11,0,489,289),UiPlainFrame.GetClientRectangle(new Rectangle(0,0,500,300),false,1.5f),"11 px borders at 150%");
			Assert.AreEqual(39,UiPlainFrame.Scale(UiPlainFrame.HEIGHT_TITLE_BAR_96,1.5f),"the band at 150%");
			//Moving the content and back (a dpi change takes it up, then down by the new band).
			Rectangle bounds=new Rectangle(10,60,200,100);
			AnchorStyles anchorAll=AnchorStyles.Top|AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right;
			Assert.AreEqual(new Rectangle(10,86,200,100),UiPlainFrame.GetShiftedBounds(bounds,anchorAll,26,26));
			Assert.AreEqual(new Rectangle(10,86,200,74),UiPlainFrame.GetShiftedBounds(bounds,anchorAll,26,0),"a maximized window: the content is 26 px shorter");
			Assert.AreEqual(bounds,UiPlainFrame.GetShiftedBounds(UiPlainFrame.GetShiftedBounds(bounds,anchorAll,26,0),anchorAll,-26,0),"back up");
			Assert.AreEqual(new Rectangle(10,60,200,100),UiPlainFrame.GetShiftedBounds(bounds,AnchorStyles.Bottom,26,0),"bottom only, client not grown");
			Assert.AreEqual(new Rectangle(10,73,200,100),UiPlainFrame.GetShiftedBounds(bounds,AnchorStyles.None,26,0),"neither: stays in the middle");
		}

		[TestMethod]
		public void Frame_KeepsWindowsFrame_Where() {
			StaRunner.Run(() => {
				//The gallery's before pictures.
				CodeBase.UiLooks.IsOffForGallery=true;
				try {
					using(Form form=MakeWindow()) {
						Assert.IsFalse(UiPlainFrame.IsAttached(form),"off while the gallery draws its before pictures");
					}
				}
				finally {
					CodeBase.UiLooks.IsOffForGallery=false;
				}
				//NoCustomBorders.txt (FormODBase.AreBordersMS's switch).
				bool areBordersMSBefore=UiPlainFrame.AreBordersMS;
				UiPlainFrame.AreBordersMS=true;
				try {
					using(Form form=MakeWindow()) {
						Assert.IsFalse(UiPlainFrame.IsAttached(form),"NoCustomBorders.txt keeps Windows' frame");
					}
				}
				finally {
					UiPlainFrame.AreBordersMS=areBordersMSBefore;
				}
				//A window without a border, and one that is not top level.
				using(Form form=MakeWindow(FormBorderStyle.None)) {
					IntPtr handle=form.Handle;
					Assert.IsFalse(UiPlainFrame.IsOn(form),"no border: no frame");
				}
				using(Form formHost=new Form())
				using(Form form=MakeWindow()) {
					form.TopLevel=false;
					formHost.Controls.Add(form);
					IntPtr handle=form.Handle;
					Assert.IsFalse(UiPlainFrame.IsOn(form),"a window inside another: no frame");
				}
			});
		}

		[TestMethod]
		public void Frame_ReportDesignerWindow_HasTheFrameOfItsOwnCopy() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				ScanType scanType=WindowBuilder.GetScanTypes().Find(x => x.WindowKey=="fyiReporting.RdlDesign.DialogNewTable");
				Assert.IsNotNull(scanType,"DialogNewTable");
				using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
					Form form=builtWindow.Form;
					System.Reflection.Assembly assembly=form.GetType().Assembly;
					Assert.IsTrue(PlainLookCopy.IsTrue(assembly,"UiPlainFrame","IsAttached",form),"Attach gives the report designer's dialog its frame");
					PlainLookCopy.Call(assembly,"UiPlainFrame","LayOutAsOnLoad",form);
					Assert.IsTrue(PlainLookCopy.IsTrue(assembly,"UiPlainFrame","IsOn",form));
					Assert.AreEqual(26,(int)PlainLookCopy.Call(assembly,"UiPlainFrame","GetContentOffset",form));
					Assert.AreEqual(UiPlainFrame.HTCAPTION,HitTest(form,new Point(100,14)),"its band is the caption");
					using(Bitmap bitmap=PlainLookCopy.CaptureWindow(form)) {
						Color colorTitleBar=UiFramePainter.GetWindowColorsStock().TitleBar;
						Assert.AreEqual(colorTitleBar.ToArgb(),bitmap.GetPixel(bitmap.Width/2,20).ToArgb(),"the picture shows the band");
					}
				}
			});
		}

		///<summary>UI-PLAIN-2 fix round: the report designer's main window built by its constructor with one report open (the tabbed interface: a
		///maximized MDI child): its MDI area in the background token, and the caption buttons Windows Forms merges into its menu bar drawn as the
		///title bar's Fluent glyphs by the token renderer.</summary>
		[TestMethod]
		public void Frame_ReportDesignerMainWindow_MdiAreaAndMergedButtons() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				bool isBuildingBefore=WindowBuilder.IsBuildingByConstructor;
				try {
					WindowBuilder.IsBuildingByConstructor=true;
					ScanType scanType=WindowBuilder.GetScanTypes().Find(x => x.WindowKey=="fyiReporting.RdlDesign.RdlDesigner");
					Assert.IsNotNull(scanType,"RdlDesigner");
					using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
						Form form=builtWindow.Form;
						Assert.IsTrue(form.IsMdiContainer);
						Assert.IsNotNull(form.MainMenuStrip,"its constructor made the menus");
						MdiClient mdiClient=form.Controls.OfType<MdiClient>().Single();
						Assert.AreEqual(UiTheme.Surface.ToArgb(),mdiClient.BackColor.ToArgb(),"the MDI area in the background token, not AppWorkspace");
						Assert.AreEqual(0,GetWindowLong(mdiClient.Handle,-20) & 0x200,"the MDI area flat: no sunken 3D edge (WS_EX_CLIENTEDGE)");
						FitGallery.OpenSampleReport(form);
						Assert.AreEqual(1,form.MdiChildren.Length,"one report open");
						Assert.AreEqual(FormWindowState.Maximized,form.MdiChildren[0].WindowState,"the tabbed interface maximizes it");
						Assert.IsFalse(PlainLookCopy.IsTrue(form.GetType().Assembly,"UiPlainFrame","IsOn",form.MdiChildren[0]),"an MDI child keeps Windows' MDI frame");
						System.Collections.Generic.List<ToolStripItem> listMerged=form.MainMenuStrip.Items.Cast<ToolStripItem>()
							.Where(x => x.GetType().FullName=="System.Windows.Forms.MdiControlStrip+ControlBoxMenuItem").ToList();
						Assert.AreEqual(3,listMerged.Count,"minimize, restore and close merged into the menu bar");
						CollectionAssert.AreEquivalent(new UiGlyph[] { UiGlyph.Subtract,UiGlyph.SquareMultiple,UiGlyph.Dismiss },
							listMerged.Select(x => UiToolStripRenderer.GetMdiControlGlyph(x).Value).ToArray(),"the title bar's glyphs");
					}
				}
				finally {
					WindowBuilder.IsBuildingByConstructor=isBuildingBefore;
				}
			});
			Assert.IsNull(UiToolStripRenderer.GetMdiControlGlyph(new ToolStripMenuItem("File")),"any other item keeps its picture");
		}

		private static Bitmap PaintBand(Form form) {
			Bitmap bitmap=new Bitmap(form.ClientSize.Width,form.ClientSize.Height,PixelFormat.Format32bppArgb);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.Magenta);
				UiPlainFrame.PaintTitleBar(form,g);
			}
			return bitmap;
		}

		private static int CountNear(Bitmap bitmap,Color color,Rectangle rectangle) {
			int count=0;
			for(int y=rectangle.Top;y<rectangle.Bottom;y++) {
				for(int x=rectangle.Left;x<rectangle.Right;x++) {
					Color colorPixel=bitmap.GetPixel(x,y);
					if(Math.Abs(colorPixel.R-color.R)<40 && Math.Abs(colorPixel.G-color.G)<40 && Math.Abs(colorPixel.B-color.B)<40) {
						count++;
					}
				}
			}
			return count;
		}
	}
}
