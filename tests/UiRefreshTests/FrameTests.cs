// The window frame of package UI-3 (docs/specs/ui-refresh-plan.md, section 3.6 "Window frame", section 6 "Misc Colors"; docs/port-notes/UI-3.md):
// FormODBase and FormFrame paint the same pixels for the same title (both call FramePainter), the floating image windows' selected and
// unselected frames, the office's Misc Colors item by item (all stock; only the text changed; only the outline changed; all changed), the
// Definitions window's "(program default)" note, and the off-screen part of gallery G-UI2 (frame part) into OPMS_UI3_FRAME_OUT (default:
// ui3-frame in the test's output folder):
//   before\frame\*.png   the frame as the old painter drew it (a copy of today's drawing, LegacyFramePainter below)
//   after\frame\*.png    the frame as FramePainter draws it
//   notes.txt            what was drawn
// Nothing is shown on the desktop and no database is used: the windows are built and painted into bitmaps on an STA thread.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class FrameTests {
		public const string VARIABLE_FRAME_OUT="OPMS_UI3_FRAME_OUT";
		private const string MARKER="ui3-frame.txt";
		private static readonly object _lockFolder=new object();
		private static bool _isFolderReady;
		private static readonly Color _colorStockBorder=Color.FromArgb(65,94,154);

		#region Both painters
		[TestMethod]
		public void Frame_FormODBaseAndFormFrame_PaintTheSamePixels() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				foreach(float scale in new[] { 1f,1.25f,1.5f,2f }) {
					foreach(string state in new[] { "rest","hotMin","hotMax","hotX","noMinMax" }) {
						using(TestFrameWindow testFrameWindow=new TestFrameWindow())
						using(FormFrame formFrame=NewFormFrame()) {
							bool hasMinMax=state!="noMinMax";
							Prepare(testFrameWindow,scale,hasMinMax);
							PrepareFrame(formFrame,scale,hasMinMax);
							SetHot(testFrameWindow,state);
							SetHot(formFrame,state);
							using(Bitmap bitmapOD=Paint(testFrameWindow.PaintFrame,testFrameWindow.Size))
							using(Bitmap bitmapFrame=Paint(formFrame.PaintFrame,formFrame.Size)) {
								string difference=CompareBitmaps(bitmapOD,bitmapFrame);
								Assert.IsNull(difference,"FormODBase and FormFrame at "+scale*100+"%, "+state+": "+difference);
							}
						}
					}
				}
			});
		}

		[TestMethod]
		public void Frame_PaintsTheTokens_TitleBarGlyphsAndHover() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				using(TestFrameWindow testFrameWindow=new TestFrameWindow()) {
					Prepare(testFrameWindow,1f,hasMinMax:true);
					FramePaintArgs framePaintArgs=testFrameWindow.GetFramePaintArgs();
					using(Bitmap bitmap=Paint(testFrameWindow.PaintFrame,testFrameWindow.Size)) {
						//The title bar between the title and the buttons: TitleBar, and the outline: TitleBarOutline.
						Assert.AreEqual(UiTheme.TitleBar.ToArgb(),bitmap.GetPixel(framePaintArgs.Width/2+40,3).ToArgb(),"title bar");
						Assert.AreEqual(UiTheme.TitleBarOutline.ToArgb(),bitmap.GetPixel(framePaintArgs.Width/2,0).ToArgb(),"outline");
						//The close glyph is drawn in the title text colour (white), and nothing of the old red.
						FramePaintResult framePaintResult=GetResult(testFrameWindow);
						Rectangle rectangleShapeX=FramePainter.GetButtonShape(framePaintResult.RectangleButX,framePaintArgs,isClose:true);
						//Antialiased: count the pixels nearer the glyph colour than the bar colour.
						Assert.IsTrue(CountNear(bitmap,rectangleShapeX,UiTheme.TitleBarText,UiTheme.TitleBar)>8,"the Dismiss glyph in TitleBarText");
						Rectangle rectangleShapeMin=FramePainter.GetButtonShape(framePaintResult.RectangleButMin,framePaintArgs,isClose:false);
						Assert.IsTrue(CountNear(bitmap,rectangleShapeMin,UiTheme.TitleBarText,UiTheme.TitleBar)>=8,"the Subtract glyph");
						Assert.AreEqual(0,CountColor(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height),Color.FromArgb(232,17,35)),"no old red");
					}
					SetHot(testFrameWindow,"hotX");
					using(Bitmap bitmap=Paint(testFrameWindow.PaintFrame,testFrameWindow.Size)) {
						FramePaintResult framePaintResult=GetResult(testFrameWindow);
						Rectangle rectangleShapeX=FramePainter.GetButtonShape(framePaintResult.RectangleButX,framePaintArgs,isClose:true);
						Assert.AreEqual(UiTheme.Danger.ToArgb(),bitmap.GetPixel(rectangleShapeX.X+1,rectangleShapeX.Y+1).ToArgb(),"close hover Danger");
						//The hover fills the whole height of the title bar as the screen shows it: from the PanelBorders' row 1 (the client area's row 0,
						//the window's top row), and the close button to the window's right column (the client area's last column).
						Assert.AreEqual(1,rectangleShapeX.Y);
						Assert.AreEqual(framePaintArgs.HeightTitleBar+1,rectangleShapeX.Bottom);
						Assert.AreEqual(framePaintArgs.Width-FramePainter.GetWidthBorderMS(1f)*2+1,rectangleShapeX.Right);
					}
					SetHot(testFrameWindow,"hotMax");
					using(Bitmap bitmap=Paint(testFrameWindow.PaintFrame,testFrameWindow.Size)) {
						FramePaintResult framePaintResult=GetResult(testFrameWindow);
						Rectangle rectangleShape=FramePainter.GetButtonShape(framePaintResult.RectangleButMax,framePaintArgs,isClose:false);
						Assert.AreEqual(UiTheme.CaptionHover.ToArgb(),bitmap.GetPixel(rectangleShape.X+1,rectangleShape.Y+1).ToArgb(),"maximise hover CaptionHover");
					}
					SetHot(testFrameWindow,"hotMin");
					using(Bitmap bitmap=Paint(testFrameWindow.PaintFrame,testFrameWindow.Size)) {
						FramePaintResult framePaintResult=GetResult(testFrameWindow);
						Rectangle rectangleShape=FramePainter.GetButtonShape(framePaintResult.RectangleButMin,framePaintArgs,isClose:false);
						Assert.AreEqual(UiTheme.CaptionHover.ToArgb(),bitmap.GetPixel(rectangleShape.X+1,rectangleShape.Y+1).ToArgb(),"minimise hover CaptionHover");
					}
				}
			});
		}

		[TestMethod]
		public void Frame_ButtonRectangles_AreTodays() {
			//The hit rectangles of the caption buttons do not change (decision 1): width = title bar + 4, height = title bar - 1, from y 5, right
			//to left from Width - WidthBorder.
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				foreach(float scale in new[] { 1f,1.5f }) {
					using(TestFrameWindow testFrameWindow=new TestFrameWindow()) {
						Prepare(testFrameWindow,scale,hasMinMax:true);
						FramePaintArgs framePaintArgs=testFrameWindow.GetFramePaintArgs();
						using(Bitmap bitmap=Paint(testFrameWindow.PaintFrame,testFrameWindow.Size)) {
						}
						FramePaintResult framePaintResult=GetResult(testFrameWindow);
						int wEdge=framePaintArgs.HeightTitleBar+framePaintArgs.Scale(4);
						int hEdge=framePaintArgs.HeightTitleBar-1;
						int xRight=framePaintArgs.Width-framePaintArgs.WidthBorder;
						Assert.AreEqual(new Rectangle(xRight-wEdge,5,wEdge,hEdge),framePaintResult.RectangleButX);
						Assert.AreEqual(new Rectangle(xRight-wEdge*2,5,wEdge,hEdge),framePaintResult.RectangleButMax);
						Assert.AreEqual(new Rectangle(xRight-wEdge*3,5,wEdge,hEdge),framePaintResult.RectangleButMin);
						Assert.AreEqual(new Rectangle(xRight-wEdge*3,5,framePaintArgs.Width-(xRight-wEdge*3),framePaintArgs.HeightTitleBar),framePaintResult.RectangleButtons);
					}
				}
			});
		}

		[TestMethod]
		public void Frame_CaptionHover_FillsTheTitleBarAsTheScreenShowsIt() {
			//The owner's finding (2026-11-18, Windows 11): the close button's red started 3 px below the top of the title bar on screen, with blue
			//above it, although the off-screen pictures showed it reaching the top. The frames repainted only the hit rectangle of the buttons on a
			//hover change, which starts 5 px down, so the top rows kept the title bar colour; and the fill ended at the hit rectangle's right edge,
			//short of the window's right edge above 100%. Here each frame is laid out as on screen (PanelBorders at (-1,-1) of the client area, or
			//one border lower maximized; the client area the window less its invisible resize borders), painted whole at rest, then hovered the way
			//the mouse does it: the frame's own invalidation is caught and only that is repainted, as Windows does. The fill must cover the title bar
			//from its top row on screen to its bottom row, and the close fill to the client area's last column, at every scale, in FormODBase,
			//FormFrame and the floating image windows, with the stock colours and an office's own.
			List<string> listFailures=new List<string>();
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				Color colorBorderOffice=Color.FromArgb(32,96,64);
				Color colorOutlineOffice=Color.FromArgb(120,0,0);
				Color colorTextOffice=Color.FromArgb(255,230,120);
				try {
					foreach(string office in new[] { "stock","office" }) {
						Color colorBorder=office=="stock" ? _colorStockBorder : colorBorderOffice;
						Color colorOutline=office=="stock" ? Color.Black : colorOutlineOffice;
						Color colorText=office=="stock" ? Color.White : colorTextOffice;
						TestFrameWindow.SetOfficeColors(colorBorder,colorOutline,colorText);
						SetFormFrameColors(colorBorder,colorOutline,colorText);
						foreach(float scale in new[] { 1f,1.25f,1.5f,1.75f,2f }) {
							foreach(bool isMaximized in new[] { false,true }) {
								string name=office+" "+scale*100+"%"+(isMaximized ? " maximized" : "");
								using(TestFrameWindow testFrameWindow=new TestFrameWindow()) {
									Prepare(testFrameWindow,scale,hasMinMax:true);
									if(isMaximized) {
										testFrameWindow.WindowState=FormWindowState.Maximized;//never shown: only the state
										LayOutAsOnLoad(testFrameWindow);
									}
									FrameColors frameColors=FramePainter.GetColors(EnumFrameLook.Window,colorBorder,colorOutline,colorText);
									CheckHoverAsOnScreen(testFrameWindow,testFrameWindow.PanelBorders,testFrameWindow.PanelClient.Top,testFrameWindow.PaintFrame,
										scale,isMaximized,frameColors,"FormODBase "+name,listFailures);
								}
								using(FormFrame formFrame=NewFormFrame()) {
									PrepareFrame(formFrame,scale,hasMinMax:true);
									if(isMaximized) {
										formFrame.WindowState=FormWindowState.Maximized;
										LayOutAsOnLoad(formFrame);
									}
									FrameColors frameColors=FramePainter.GetColors(EnumFrameLook.Window,colorBorder,colorOutline,colorText);
									CheckHoverAsOnScreen(formFrame,formFrame.PanelBorders,formFrame.ElementHostUI.Top,formFrame.PaintFrame,
										scale,isMaximized,frameColors,"FormFrame "+name,listFailures);
								}
							}
							if(office!="stock") {
								continue;//the office's colours do not reach the floating image windows
							}
							foreach(bool isSelected in new[] { true,false }) {
								using(FormImageFloat formImageFloat=new FormImageFloat()) {
									formImageFloat.Text="Image 1";
									WindowBuilder.GetLayoutManager(formImageFloat).SetScaleMS(scale);
									formImageFloat.Size=new Size((int)Math.Round(430*scale),(int)Math.Round(300*scale));
									IntPtr handle=formImageFloat.Handle;
									formImageFloat.IsImageFloatSelected=isSelected;
									LayOutAsOnLoad(formImageFloat);
									FrameColors frameColors=FramePainter.GetImageFloatColors(isSelected);
									CheckHoverAsOnScreen(formImageFloat,formImageFloat.PanelBorders,formImageFloat.PanelClient.Top,formImageFloat.PaintFrame,
										scale,isMaximized:false,frameColors,"FormImageFloat "+(isSelected ? "selected " : "unselected ")+scale*100+"%",listFailures);
								}
							}
						}
					}
				}
				finally {
					TestFrameWindow.SetOfficeColors(_colorStockBorder,Color.Black,Color.White);
					SetFormFrameColors(_colorStockBorder,Color.Black,Color.White);
				}
			});
			Assert.AreEqual(0,listFailures.Count,listFailures.Count+" failures:\r\n"+string.Join("\r\n",listFailures.Take(40)));
		}

		[TestMethod]
		public void Frame_CaptionHitAreas_CoverTheFills_PerPoint() {
			//The hover-fix review (2026-11-19): each caption button's hit area covers its painted fill. Maximized: all of it, from the screen's top
			//row, and for close its last column, so the screen's top-right pixel closes the window. Normal: the top resize edge (the rows above
			//the button rectangle's top, y 5) stays as it is, and below it the close button reaches the client area's right edge and the hover
			//stays on anywhere in the fill. Every point of the title bar right of the buttons' left edge that the screen shows is moved over the
			//laid-out frame through its own MouseMove, and the hover state is compared with the area worked out here from the geometry.
			List<string> listFailures=new List<string>();
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				foreach(float scale in new[] { 1f,1.5f,2f }) {
					foreach(bool isMaximized in new[] { false,true }) {
						string name=scale*100+"%"+(isMaximized ? " maximized" : " normal");
						using(TestFrameWindow testFrameWindow=new TestFrameWindow()) {
							Prepare(testFrameWindow,scale,hasMinMax:true);
							if(isMaximized) {
								testFrameWindow.WindowState=FormWindowState.Maximized;//never shown: only the state
								LayOutAsOnLoad(testFrameWindow);
							}
							int edgeMouseDown=WindowBuilder.GetLayoutManager(testFrameWindow).WidthBorder();//FormODBase.PanelBorders_MouseDown's top edge
							CheckHitAreasPerPoint(testFrameWindow,testFrameWindow.PanelBorders,testFrameWindow.PanelClient.Top,testFrameWindow.PaintFrame,
								scale,isMaximized,edgeMouseDown,"FormODBase "+name,listFailures);
						}
						using(FormFrame formFrame=NewFormFrame()) {
							PrepareFrame(formFrame,scale,hasMinMax:true);
							if(isMaximized) {
								formFrame.WindowState=FormWindowState.Maximized;
								LayOutAsOnLoad(formFrame);
							}
							CheckHitAreasPerPoint(formFrame,formFrame.PanelBorders,formFrame.ElementHostUI.Top,formFrame.PaintFrame,
								scale,isMaximized,edgeMouseDown:5,"FormFrame "+name,listFailures);
						}
					}
				}
			});
			Assert.AreEqual(0,listFailures.Count,listFailures.Count+" failures:\r\n"+string.Join("\r\n",listFailures.Take(40)));
		}

		[TestMethod]
		public void Frame_CaptionGlyphs_CentredInTheirFills() {
			//Each caption glyph is centred in its painted fill (FramePainter.GetGlyphArea) at every scale, normal and maximized; at 100% it is
			//where it always was (the old glyph area: the button's columns from just inside the outline to the bottom of the title bar).
			List<string> listFailures=new List<string>();
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				foreach(float scale in new[] { 1f,1.25f,1.5f,1.75f,2f }) {
					foreach(bool isMaximized in new[] { false,true }) {
						string name=scale*100+"%"+(isMaximized ? " maximized" : " normal");
						using(TestFrameWindow testFrameWindow=new TestFrameWindow())
						using(FormFrame formFrame=NewFormFrame()) {
							Prepare(testFrameWindow,scale,hasMinMax:true);
							PrepareFrame(formFrame,scale,hasMinMax:true);
							if(isMaximized) {
								testFrameWindow.WindowState=FormWindowState.Maximized;
								LayOutAsOnLoad(testFrameWindow);
								formFrame.WindowState=FormWindowState.Maximized;
								LayOutAsOnLoad(formFrame);
							}
							CheckGlyphsCentred(testFrameWindow,testFrameWindow.GetFramePaintArgs(),testFrameWindow.PaintFrame,scale,"FormODBase "+name,listFailures);
							CheckGlyphsCentred(formFrame,formFrame.GetFramePaintArgs(),formFrame.PaintFrame,scale,"FormFrame "+name,listFailures);
						}
					}
				}
			});
			Assert.AreEqual(0,listFailures.Count,listFailures.Count+" failures:\r\n"+string.Join("\r\n",listFailures.Take(40)));
		}
		#endregion Both painters

		#region FormImageFloat
		[TestMethod]
		public void Frame_FormImageFloat_SelectedAndUnselected() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				try {
					//An office's own title bar colours do not reach the floating image windows (as before).
					TestFrameWindow.SetOfficeColors(Color.DarkGreen,Color.Red,Color.Yellow);
					using(FormImageFloat formImageFloat=new FormImageFloat()) {
						formImageFloat.Size=new Size(420,300);
						formImageFloat.Text="Image 1";
						IntPtr handle=formImageFloat.Handle;
						foreach(bool isSelected in new[] { true,false }) {
							formImageFloat.IsImageFloatSelected=isSelected;
							FramePaintArgs framePaintArgs=formImageFloat.GetFramePaintArgs();
							Assert.AreEqual(isSelected ? EnumFrameLook.ImageFloatSelected : EnumFrameLook.ImageFloatUnselected,framePaintArgs.FrameLook);
							Color colorBar=isSelected ? UiTheme.Accent : UiTheme.SurfaceSubtle;
							Color colorOutline=isSelected ? UiTheme.TitleBarOutline : UiTheme.Stroke;
							Color colorText=isSelected ? UiTheme.TextOnAccent : UiTheme.Text;
							Color colorHover=isSelected ? UiTheme.CaptionHover : UiTheme.SubtleHover;
							SetHot(formImageFloat,"rest");
							using(Bitmap bitmap=Paint(formImageFloat.PaintFrame,formImageFloat.Size)) {
								string name=isSelected ? "selected" : "unselected";
								Assert.AreEqual(colorBar.ToArgb(),bitmap.GetPixel(framePaintArgs.Width/2+40,3).ToArgb(),name+": title bar");
								Assert.AreEqual(colorOutline.ToArgb(),bitmap.GetPixel(framePaintArgs.Width/2,0).ToArgb(),name+": outline");
								Rectangle rectangleText=new Rectangle(30,4,90,framePaintArgs.HeightTitleBar-8);
								Assert.IsTrue(CountColor(bitmap,rectangleText,colorText)>10,name+": the title in "+colorText);
							}
							SetHot(formImageFloat,"hotMax");
							using(Bitmap bitmap=Paint(formImageFloat.PaintFrame,formImageFloat.Size)) {
								FramePaintResult framePaintResult=GetResult(formImageFloat);
								Rectangle rectangleShape=FramePainter.GetButtonShape(framePaintResult.RectangleButMax,framePaintArgs,isClose:false);
								Assert.AreEqual(colorHover.ToArgb(),bitmap.GetPixel(rectangleShape.X+1,rectangleShape.Y+1).ToArgb(),"maximise hover");
							}
							SetHot(formImageFloat,"hotX");
							using(Bitmap bitmap=Paint(formImageFloat.PaintFrame,formImageFloat.Size)) {
								FramePaintResult framePaintResult=GetResult(formImageFloat);
								Rectangle rectangleShape=FramePainter.GetButtonShape(framePaintResult.RectangleButX,framePaintArgs,isClose:true);
								Assert.AreEqual(UiTheme.Danger.ToArgb(),bitmap.GetPixel(rectangleShape.X+1,rectangleShape.Y+1).ToArgb(),"close hover Danger");
							}
						}
					}
				}
				finally {
					TestFrameWindow.SetOfficeColors(_colorStockBorder,Color.Black,Color.White);
				}
			});
		}
		#endregion FormImageFloat

		#region Misc Colors
		[TestMethod]
		public void MiscColors_AllStock_GiveTheTokens() {
			FrameColors frameColors=FramePainter.GetWindowColors(_colorStockBorder,Color.Black,Color.White,isHighContrast:false);
			Assert.AreEqual(UiTheme.GetColorLight(EnumUiColor.TitleBar).ToArgb(),frameColors.TitleBar.ToArgb());
			Assert.AreEqual(0x3C5A96,frameColors.TitleBar.ToArgb()&0xFFFFFF,"Accent #3C5A96");
			Assert.AreEqual(0x2A406C,frameColors.Outline.ToArgb()&0xFFFFFF,"AccentPressed #2A406C");
			Assert.AreEqual(Color.White.ToArgb(),frameColors.TitleBarText.ToArgb());
			Assert.AreEqual(0x5370A6,frameColors.CaptionHover.ToArgb()&0xFFFFFF,"CaptionHover");
			Assert.AreEqual(Color.White.ToArgb(),frameColors.CaptionHoverGlyph.ToArgb());
			Assert.AreEqual(0xC42B1C,frameColors.CloseHover.ToArgb()&0xFFFFFF,"Danger");
			Assert.AreEqual(Color.White.ToArgb(),frameColors.CloseHoverGlyph.ToArgb());
		}

		[TestMethod]
		public void MiscColors_OnlyTheTextChanged_KeepsTheText() {
			Color colorText=Color.FromArgb(255,255,0);
			FrameColors frameColors=FramePainter.GetWindowColors(_colorStockBorder,Color.Black,colorText,isHighContrast:false);
			Assert.AreEqual(0x3C5A96,frameColors.TitleBar.ToArgb()&0xFFFFFF,"the bar stays Accent");
			Assert.AreEqual(0x2A406C,frameColors.Outline.ToArgb()&0xFFFFFF,"the outline stays AccentPressed");
			Assert.AreEqual(colorText.ToArgb(),frameColors.TitleBarText.ToArgb(),"the office's text colour");
			//The office's own title bar: the old hover mix of three parts bar and one part text, and the glyph in the text colour.
			Color colorMix=Color.FromArgb((3*0x3C+255)/4,(3*0x5A+255)/4,(3*0x96+0)/4);
			Assert.AreEqual(colorMix.ToArgb(),frameColors.CaptionHover.ToArgb());
			Assert.AreEqual(colorText.ToArgb(),frameColors.CaptionHoverGlyph.ToArgb());
			Assert.AreEqual(0xC42B1C,frameColors.CloseHover.ToArgb()&0xFFFFFF,"close hover stays Danger");
		}

		[TestMethod]
		public void MiscColors_OnlyTheOutlineChanged_KeepsTheOutline() {
			Color colorOutline=Color.FromArgb(200,0,0);
			FrameColors frameColors=FramePainter.GetWindowColors(_colorStockBorder,colorOutline,Color.White,isHighContrast:false);
			Assert.AreEqual(0x3C5A96,frameColors.TitleBar.ToArgb()&0xFFFFFF);
			Assert.AreEqual(colorOutline.ToArgb(),frameColors.Outline.ToArgb(),"the office's outline");
			Assert.AreEqual(Color.White.ToArgb(),frameColors.TitleBarText.ToArgb());
			Assert.AreEqual(0x5370A6,frameColors.CaptionHover.ToArgb()&0xFFFFFF,"bar and text stock: CaptionHover");
		}

		[TestMethod]
		public void MiscColors_AllChanged_GiveTheOfficesColours() {
			Color colorBorder=Color.FromArgb(0,100,0);
			Color colorOutline=Color.FromArgb(200,0,0);
			Color colorText=Color.FromArgb(255,255,0);
			FrameColors frameColors=FramePainter.GetWindowColors(colorBorder,colorOutline,colorText,isHighContrast:false);
			Assert.AreEqual(colorBorder.ToArgb(),frameColors.TitleBar.ToArgb());
			Assert.AreEqual(colorOutline.ToArgb(),frameColors.Outline.ToArgb());
			Assert.AreEqual(colorText.ToArgb(),frameColors.TitleBarText.ToArgb());
			Color colorMix=Color.FromArgb((3*0+255)/4,(3*100+255)/4,(3*0+0)/4);
			Assert.AreEqual(colorMix.ToArgb(),frameColors.CaptionHover.ToArgb());
			Assert.AreEqual(0xC42B1C,frameColors.CloseHover.ToArgb()&0xFFFFFF);
			//And painted: a window of an office that changed all three draws its colours.
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				try {
					TestFrameWindow.SetOfficeColors(colorBorder,colorOutline,colorText);
					using(TestFrameWindow testFrameWindow=new TestFrameWindow()) {
						Prepare(testFrameWindow,1f,hasMinMax:true);
						FramePaintArgs framePaintArgs=testFrameWindow.GetFramePaintArgs();
						using(Bitmap bitmap=Paint(testFrameWindow.PaintFrame,testFrameWindow.Size)) {
							Assert.AreEqual(colorBorder.ToArgb(),bitmap.GetPixel(framePaintArgs.Width/2+40,3).ToArgb(),"bar");
							Assert.AreEqual(colorOutline.ToArgb(),bitmap.GetPixel(framePaintArgs.Width/2,0).ToArgb(),"outline");
							Assert.IsTrue(CountColor(bitmap,new Rectangle(30,4,90,framePaintArgs.HeightTitleBar-8),colorText)>10,"title text");
						}
					}
				}
				finally {
					TestFrameWindow.SetOfficeColors(_colorStockBorder,Color.Black,Color.White);
				}
			});
		}

		[TestMethod]
		public void MiscColors_HighContrast_TokensWin() {
			FrameColors frameColors=FramePainter.GetWindowColors(Color.FromArgb(0,100,0),Color.Red,Color.Yellow,isHighContrast:true);
			Assert.AreEqual(SystemColors.ActiveCaption.ToArgb(),frameColors.TitleBar.ToArgb());
			Assert.AreEqual(SystemColors.ActiveCaptionText.ToArgb(),frameColors.TitleBarText.ToArgb());
			Assert.AreEqual(SystemColors.WindowFrame.ToArgb(),frameColors.Outline.ToArgb());
		}

		[TestMethod]
		public void MiscColors_StockValues_AreOpenDentals() {
			Assert.IsTrue(FramePainter.IsStockMiscColor(DefCatMiscColors.MainBorder,Color.FromArgb(-12493158)));
			Assert.IsTrue(FramePainter.IsStockMiscColor(DefCatMiscColors.MainBorder,_colorStockBorder));
			Assert.IsTrue(FramePainter.IsStockMiscColor(DefCatMiscColors.MainBorderOutline,Color.Black));
			Assert.IsTrue(FramePainter.IsStockMiscColor(DefCatMiscColors.MainBorderText,Color.White));
			Assert.IsFalse(FramePainter.IsStockMiscColor(DefCatMiscColors.MainBorder,Color.FromArgb(60,90,150)));
			Assert.IsFalse(FramePainter.IsStockMiscColor(DefCatMiscColors.MainBorderText,Color.Black));
			Assert.IsFalse(FramePainter.IsStockMiscColor(DefCatMiscColors.FamilyModuleICE,Color.White),"only items 11 to 13");
			Assert.IsTrue(FramePainter.IsTitleBarMiscColor(11) && FramePainter.IsTitleBarMiscColor(12) && FramePainter.IsTitleBarMiscColor(13));
			Assert.IsFalse(FramePainter.IsTitleBarMiscColor(10) || FramePainter.IsTitleBarMiscColor(14));
		}

		[TestMethod]
		public void Definitions_MiscColors_ShowProgramDefault() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				List<Def> listDefs=new List<Def>();
				for(int i=0;i<14;i++) {
					listDefs.Add(new Def { DefNum=i+1,Category=DefCat.MiscColors,ItemOrder=i,ItemName="Item "+i,ItemValue="",ItemColor=Color.White });
				}
				listDefs[(int)DefCatMiscColors.MainBorder].ItemColor=_colorStockBorder;//stock
				listDefs[(int)DefCatMiscColors.MainBorderOutline].ItemColor=Color.Red;//changed
				listDefs[(int)DefCatMiscColors.MainBorderText].ItemColor=Color.White;//stock
				DefCatOptions defCatOptions=DefL.GetOptionsForDefCats(new List<DefCat> { DefCat.MiscColors })[0];
				using(GridOD gridOD=new GridOD()) {
					gridOD.Size=new Size(500,400);
					DefL.FillGridDefs(gridOD,defCatOptions,listDefs);
					Assert.AreEqual("(program default)",gridOD.ListGridRows[(int)DefCatMiscColors.MainBorder].Cells[1].Text);
					Assert.AreEqual("",gridOD.ListGridRows[(int)DefCatMiscColors.MainBorderOutline].Cells[1].Text,"a changed item has no note");
					Assert.AreEqual("(program default)",gridOD.ListGridRows[(int)DefCatMiscColors.MainBorderText].Cells[1].Text);
					Assert.AreEqual("",gridOD.ListGridRows[(int)DefCatMiscColors.FamilyModuleICE].Cells[1].Text,"white on another item is no title bar colour");
					Assert.AreEqual(_colorStockBorder.ToArgb(),gridOD.ListGridRows[(int)DefCatMiscColors.MainBorder].Cells[2].ColorBackG.ToArgb(),
						"the swatch still shows the stored colour");
				}
			});
		}
		#endregion Misc Colors

		#region DWM trial
		[TestMethod]
		public void DwmCorners_RoundByDefault_TheVariableTurnsThemOff() {
			//The owner kept the rounded corners (2026-11-18): round unless OPENPMS_UI3_DWM_CORNERS says "off" (or "square", "none") or "small".
			Assert.AreEqual(2,FramePainter.DWMWCP_ROUND,"DWMWCP_ROUND");
			Assert.AreEqual(3,FramePainter.DWMWCP_ROUNDSMALL,"DWMWCP_ROUNDSMALL");
			Assert.AreEqual(FramePainter.DWMWCP_ROUND,FramePainter.GetDwmCornerPreference((string)null),"unset: round");
			Assert.AreEqual(FramePainter.DWMWCP_ROUND,FramePainter.GetDwmCornerPreference(""),"empty: round");
			Assert.AreEqual(FramePainter.DWMWCP_ROUND,FramePainter.GetDwmCornerPreference("round"));
			Assert.AreEqual(FramePainter.DWMWCP_ROUND,FramePainter.GetDwmCornerPreference("ROUND"));
			Assert.AreEqual(FramePainter.DWMWCP_ROUND,FramePainter.GetDwmCornerPreference("yes"),"an unknown value gives the default");
			Assert.AreEqual(FramePainter.DWMWCP_ROUNDSMALL,FramePainter.GetDwmCornerPreference("small"));
			Assert.AreEqual(FramePainter.DWMWCP_ROUNDSMALL,FramePainter.GetDwmCornerPreference(" Small "));
			Assert.AreEqual(0,FramePainter.GetDwmCornerPreference("off"),"off: nothing is asked of Windows");
			Assert.AreEqual(0,FramePainter.GetDwmCornerPreference("OFF"));
			Assert.AreEqual(0,FramePainter.GetDwmCornerPreference("square"));
			Assert.AreEqual(0,FramePainter.GetDwmCornerPreference("none"));
			//And through the environment, as the program reads it.
			string valueOld=Environment.GetEnvironmentVariable(FramePainter.ENV_DWM_CORNERS);
			try {
				Environment.SetEnvironmentVariable(FramePainter.ENV_DWM_CORNERS,null);
				Assert.AreEqual(FramePainter.DWMWCP_ROUND,FramePainter.GetDwmCornerPreference(),"rounded corners by default");
				Environment.SetEnvironmentVariable(FramePainter.ENV_DWM_CORNERS,"off");
				Assert.AreEqual(0,FramePainter.GetDwmCornerPreference(),"the variable turns them off");
				Environment.SetEnvironmentVariable(FramePainter.ENV_DWM_CORNERS,"square");
				Assert.AreEqual(0,FramePainter.GetDwmCornerPreference());
				Environment.SetEnvironmentVariable(FramePainter.ENV_DWM_CORNERS,"small");
				Assert.AreEqual(FramePainter.DWMWCP_ROUNDSMALL,FramePainter.GetDwmCornerPreference());
				Environment.SetEnvironmentVariable(FramePainter.ENV_DWM_CORNERS,"round");
				Assert.AreEqual(FramePainter.DWMWCP_ROUND,FramePainter.GetDwmCornerPreference());
			}
			finally {
				Environment.SetEnvironmentVariable(FramePainter.ENV_DWM_CORNERS,valueOld);
			}
			Assert.AreEqual(0x6C402A,FramePainter.ToColorRef(Color.FromArgb(0x2A,0x40,0x6C)),"COLORREF is 0x00BBGGRR");
		}

		[TestMethod]
		public void DwmBorder_ReappliedWhenTheFrameColoursChange() {
			//Windows' 1 px border (DWMWA_BORDER_COLOR) was set once, when the handle was made. It is asked for again, in the outline colour the
			//painter uses, when the office's Misc Colors are loaded or changed (SetBorderColor, in FormODBase and FormFrame) and when a floating
			//image window is selected or not; only with the corners on and the program's own frame.
			List<(IntPtr handle,Color color)> listApplied=new List<(IntPtr handle,Color color)>();
			Action<IntPtr,Color> action=(handle,color) => { lock(listApplied) { listApplied.Add((handle,color)); } };
			string valueOld=Environment.GetEnvironmentVariable(FramePainter.ENV_DWM_CORNERS);
			Color colorOutlineOffice=Color.FromArgb(120,0,0);
			Color? GetLast(IntPtr handle) {
				lock(listApplied) {
					List<(IntPtr handle,Color color)> list=listApplied.Where(x => x.handle==handle).ToList();
					return list.Count==0 ? (Color?)null : list[list.Count-1].color;
				}
			}
			FramePainter.DwmBorderColorApplied+=action;
			try {
				Environment.SetEnvironmentVariable(FramePainter.ENV_DWM_CORNERS,null);//the default: round
				StaRunner.Run(() => {
					NoDatabaseGuard.Install();
					try {
						using(TestFrameWindow testFrameWindow=new TestFrameWindow()) {
							Prepare(testFrameWindow,1f,hasMinMax:true);//makes the handle: applied once
							Assert.AreEqual(UiTheme.TitleBarOutline.ToArgb(),GetLast(testFrameWindow.Handle)?.ToArgb(),"FormODBase: the handle made, the stock outline");
							testFrameWindow.SetOfficeColorsHere(_colorStockBorder,colorOutlineOffice,Color.White);
							Assert.AreEqual(colorOutlineOffice.ToArgb(),GetLast(testFrameWindow.Handle)?.ToArgb(),"FormODBase: the office's outline, re-applied");
							Assert.AreEqual(colorOutlineOffice.ToArgb(),testFrameWindow.GetDwmBorderColor().ToArgb(),"FormODBase: GetDwmBorderColor");
							testFrameWindow.SetOfficeColorsHere(_colorStockBorder,Color.Black,Color.White);
							Assert.AreEqual(UiTheme.TitleBarOutline.ToArgb(),GetLast(testFrameWindow.Handle)?.ToArgb(),"FormODBase: back to stock, the token");
						}
						using(FormFrame formFrame=NewFormFrame()) {
							PrepareFrame(formFrame,1f,hasMinMax:true);
							Assert.AreEqual(UiTheme.TitleBarOutline.ToArgb(),GetLast(formFrame.Handle)?.ToArgb(),"FormFrame: the handle made");
							MethodInfo methodInfo=FindMethod(typeof(FormFrame),"SetBorderColor");
							Assert.IsNotNull(methodInfo,"FormFrame.SetBorderColor");
							methodInfo.Invoke(formFrame,new object[] { DefCatMiscColors.MainBorderOutline,colorOutlineOffice });
							Assert.AreEqual(colorOutlineOffice.ToArgb(),GetLast(formFrame.Handle)?.ToArgb(),"FormFrame: the office's outline, re-applied");
							Assert.AreEqual(colorOutlineOffice.ToArgb(),formFrame.GetDwmBorderColor().ToArgb(),"FormFrame: GetDwmBorderColor");
							methodInfo.Invoke(formFrame,new object[] { DefCatMiscColors.MainBorderOutline,Color.Black });
							Assert.AreEqual(UiTheme.TitleBarOutline.ToArgb(),GetLast(formFrame.Handle)?.ToArgb(),"FormFrame: back to stock");
						}
						//The floating image windows: their own outlines, the office's colours do not reach them.
						TestFrameWindow.SetOfficeColors(_colorStockBorder,colorOutlineOffice,Color.White);
						using(FormImageFloat formImageFloat=new FormImageFloat()) {
							formImageFloat.Size=new Size(430,300);
							IntPtr handle=formImageFloat.Handle;
							formImageFloat.IsImageFloatSelected=true;
							Assert.AreEqual(FramePainter.GetImageFloatColors(isSelected:true).Outline.ToArgb(),GetLast(handle)?.ToArgb(),"FormImageFloat selected");
							formImageFloat.IsImageFloatSelected=false;
							Assert.AreEqual(UiTheme.Stroke.ToArgb(),GetLast(handle)?.ToArgb(),"FormImageFloat unselected: Stroke");
							formImageFloat.IsImageFloatSelected=true;
							Assert.AreEqual(UiTheme.TitleBarOutline.ToArgb(),GetLast(handle)?.ToArgb(),"FormImageFloat selected again: TitleBarOutline");
						}
						//Corners off: nothing is asked of Windows when the colours change.
						Environment.SetEnvironmentVariable(FramePainter.ENV_DWM_CORNERS,"off");
						using(TestFrameWindow testFrameWindow=new TestFrameWindow()) {
							Prepare(testFrameWindow,1f,hasMinMax:true);
							testFrameWindow.SetOfficeColorsHere(_colorStockBorder,colorOutlineOffice,Color.White);
							Assert.IsNull(GetLast(testFrameWindow.Handle),"corners off: nothing applied");
						}
					}
					finally {
						TestFrameWindow.SetOfficeColors(_colorStockBorder,Color.Black,Color.White);
					}
				});
				//The painter's own guard: no corner preference, or the Windows frame (AreBordersMS), asks nothing.
				int countBefore=listApplied.Count;
				FramePainter.ApplyDwmCorners(new IntPtr(1),areBordersMS:false,Color.Red,preference:0);
				FramePainter.ApplyDwmCorners(new IntPtr(1),areBordersMS:true,Color.Red,preference:FramePainter.DWMWCP_ROUND);
				Assert.AreEqual(countBefore,listApplied.Count,"corners off or the Windows frame: nothing applied");
			}
			finally {
				FramePainter.DwmBorderColorApplied-=action;
				Environment.SetEnvironmentVariable(FramePainter.ENV_DWM_CORNERS,valueOld);
			}
		}

		[TestMethod]
		public void Frame_SourceScan_NoOldFrameColours() {
			//The old frame's fixed colours are gone from both frames: the red close, the hot blue-grey and the floating windows' mixed blue.
			foreach(string relative in new[] { @"OpenPmsDental\UI\BordersDpi\FormODBase.cs",@"WpfControlsOD\Frame\FormFrame.cs" }) {
				string text=File.ReadAllText(Path.Combine(RepoRoot(),relative));
				Assert.IsFalse(text.Contains("_colorRedX"),relative+": _colorRedX");
				Assert.IsFalse(text.Contains("_colorButtonHot"),relative+": _colorButtonHot");
				Assert.IsFalse(text.Contains("colorFloatBase"),relative+": colorFloatBase");
				Assert.IsTrue(text.Contains("FramePainter.Paint("),relative+" paints with FramePainter");
				//A hover change repaints all the caption buttons paint (InvalidateCaptionButtons), not their hit rectangle, which starts 5 px below
				//the title bar's top and left the top of the hover fill unpainted on screen (the owner's finding, 2026-11-18).
				string[] arrayCodeLines=text.Split('\n').Select(x => x.Trim()).Where(x => !x.StartsWith("//")).ToArray();
				Assert.IsFalse(arrayCodeLines.Any(x => x.Contains("Invalidate(_rectangleButtons)")),relative+": a hover repaints only the hit rectangle");
				Assert.AreEqual(4,arrayCodeLines.Count(x => x=="InvalidateCaptionButtons();"),relative+": the four hover changes repaint the caption buttons");
			}
		}
		#endregion DWM trial

		#region Gallery G-UI2 (frame part, off screen)
		[TestMethod]
		public void Gallery_FrameParts_RenderOffScreen() {
			string folder=GetFolderOut();
			List<string> listNotes=new List<string>();
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				try {
					//A stock office and two customised ones: only the text changed, and all three changed.
					RenderOffice(folder,"stock",_colorStockBorder,Color.Black,Color.White,listNotes);
					RenderOffice(folder,"office-text",_colorStockBorder,Color.Black,Color.FromArgb(255,230,120),listNotes);
					RenderOffice(folder,"office-all",Color.FromArgb(32,96,64),Color.FromArgb(120,0,0),Color.FromArgb(255,230,120),listNotes);
				}
				finally {
					TestFrameWindow.SetOfficeColors(_colorStockBorder,Color.Black,Color.White);
					SetFormFrameColors(_colorStockBorder,Color.Black,Color.White);
				}
				RenderImageFloats(folder,listNotes);
			});
			File.WriteAllLines(Path.Combine(folder,"notes.txt"),listNotes);
			Assert.IsTrue(Directory.GetFiles(Path.Combine(folder,"after","frame"),"*.png").Length>=20,"the frame pictures were written");
		}

		private static void RenderOffice(string folder,string office,Color colorBorder,Color colorOutline,Color colorText,List<string> listNotes) {
			TestFrameWindow.SetOfficeColors(colorBorder,colorOutline,colorText);
			SetFormFrameColors(colorBorder,colorOutline,colorText);//FormFrame keeps its own copy (until 2026-11-19 its pictures kept the stock colours)
			Color colorHotOld=Color.FromArgb((3*colorBorder.R+colorText.R)/4,(3*colorBorder.G+colorText.G)/4,(3*colorBorder.B+colorText.B)/4);
			foreach(float scale in new[] { 1f,1.5f,2f }) {
				foreach(string state in new[] { "rest","hotMin","hotMax","hotX","maximized" }) {
					string name=office+"-"+(int)(scale*100)+"-"+state;
					using(TestFrameWindow testFrameWindow=new TestFrameWindow())
					using(FormFrame formFrame=NewFormFrame()) {
						Prepare(testFrameWindow,scale,hasMinMax:true);
						PrepareFrame(formFrame,scale,hasMinMax:true);
						SetHot(testFrameWindow,state);
						SetHot(formFrame,state);
						if(state=="maximized") {
							//Laid out again as on screen: the PanelBorders one border lower, the client rows above it off the screen.
							LayOutAsOnLoad(testFrameWindow);
							LayOutAsOnLoad(formFrame);
						}
						FramePaintArgs framePaintArgs=testFrameWindow.GetFramePaintArgs();
						PictureLayout pictureLayout=new PictureLayout(testFrameWindow.PanelBorders,testFrameWindow.PanelClient.Top,state=="maximized");
						PictureLayout pictureLayoutFrame=new PictureLayout(formFrame.PanelBorders,formFrame.ElementHostUI.Top,state=="maximized");
						SaveWindowPicture(folder,"after","FormODBase-"+name,framePaintArgs,pictureLayout,g => testFrameWindow.PaintFrame(g));
						SaveWindowPicture(folder,"after","FormFrame-"+name,formFrame.GetFramePaintArgs(),pictureLayoutFrame,g => formFrame.PaintFrame(g));
						SaveWindowPicture(folder,"before","FormODBase-"+name,framePaintArgs,pictureLayout,
							g => LegacyFramePainter.Paint(g,framePaintArgs,colorBorder,colorText,colorOutline,colorHotOld));
						SaveWindowPicture(folder,"before","FormFrame-"+name,framePaintArgs,pictureLayoutFrame,
							g => LegacyFramePainter.Paint(g,framePaintArgs,colorBorder,colorText,colorOutline,colorHotOld));
					}
				}
			}
			//The main window's larger title (FormOpenDental).
			using(TestFrameWindow testFrameWindow=new TestFrameWindow()) {
				Prepare(testFrameWindow,1f,hasMinMax:true);
				testFrameWindow.SetMainWindowTitle();
				testFrameWindow.Text="Open PMS - Dental - Patient Name";
				FramePaintArgs framePaintArgs=testFrameWindow.GetFramePaintArgs();
				PictureLayout pictureLayout=new PictureLayout(testFrameWindow.PanelBorders,testFrameWindow.PanelClient.Top,isMaximized:false);
				SaveWindowPicture(folder,"after","FormOpenDental-"+office,framePaintArgs,pictureLayout,g => testFrameWindow.PaintFrame(g));
				SaveWindowPicture(folder,"before","FormOpenDental-"+office,framePaintArgs,pictureLayout,
					g => LegacyFramePainter.Paint(g,framePaintArgs,colorBorder,colorText,colorOutline,colorHotOld));
			}
			listNotes.Add(office+": Misc Colors 11 to 13 = "+Hex(colorBorder)+", "+Hex(colorOutline)+", "+Hex(colorText)
				+"; FormODBase and FormFrame at 100%, 150% and 200%, at rest, minimise, maximise and close on hover, and maximized (restore glyph; laid out "
				+"as on screen, the rows above the screen cut off); the main window's title.");
		}

		private static void RenderImageFloats(string folder,List<string> listNotes) {
			foreach(bool isSelected in new[] { true,false }) {
				foreach(string state in new[] { "rest","hotMax","hotX" }) {
					string name="FormImageFloat-"+(isSelected ? "selected" : "unselected")+"-"+state;
					using(FormImageFloat formImageFloat=new FormImageFloat()) {
						formImageFloat.Size=new Size(430,300);
						formImageFloat.Text=isSelected ? "Image 1 (selected)" : "Image 2";
						IntPtr handle=formImageFloat.Handle;
						formImageFloat.IsImageFloatSelected=isSelected;
						LayOutAsOnLoad(formImageFloat);
						SetHot(formImageFloat,state);
						FramePaintArgs framePaintArgs=formImageFloat.GetFramePaintArgs();
						PictureLayout pictureLayout=new PictureLayout(formImageFloat.PanelBorders,formImageFloat.PanelClient.Top,isMaximized:false);
						SaveWindowPicture(folder,"after",name,framePaintArgs,pictureLayout,g => formImageFloat.PaintFrame(g));
						Color colorBase=Color.FromArgb(65,94,154);
						Color colorBorder=isSelected ? ColorOD.Mix(colorBase,Color.White,3,1) : ColorOD.Mix(colorBase,Color.White,1,3);
						Color colorText=isSelected ? Color.White : Color.Black;
						Color colorHot=isSelected ? ColorOD.Mix(colorBorder,colorText,4,1) : ColorOD.Mix(colorBorder,colorText,10,1);
						SaveWindowPicture(folder,"before",name,framePaintArgs,pictureLayout,g => LegacyFramePainter.Paint(g,framePaintArgs,colorBorder,colorText,Color.Black,colorHot));
					}
				}
			}
			listNotes.Add("FormImageFloat: the selected and an unselected floating image window, at rest and with maximise or close on hover (Misc Colors do not apply).");
			listNotes.Add("Each picture is the window as the screen shows it, laid out as on screen: the frame's PanelBorders sits at (-1,-1) of the client "
				+"area (one border lower maximized, the client rows above the screen cut off) and the client panel (here filled with the window "
				+"background, Surface after and the old #FCFDFE before) covers all of it below the title bar. "
				+"The 1 px border around a window on screen is Windows' own (DWM) and is not in these pictures; the frame's own outline lies under the client area.");
		}

		///<summary>Where a laid-out frame's PanelBorders sit in its client area, where its client panel starts, and whether it is maximized: what
		///SaveWindowPicture needs to draw the window as the screen shows it.</summary>
		private class PictureLayout {
			public Rectangle BoundsPanelBorders;
			public int TopClientPanel;
			public bool IsMaximized;

			public PictureLayout(Control panelBorders,int topClientPanel,bool isMaximized) {
				Assert.IsNotNull(panelBorders,"the PanelBorders were not laid out");
				BoundsPanelBorders=panelBorders.Bounds;
				TopClientPanel=topClientPanel;
				IsMaximized=isMaximized;
			}
		}

		///<summary>Draws the window as the screen shows it, laid out as on screen (as Frame_CaptionHover_FillsTheTitleBarAsTheScreenShowsIt lays it
		///out): the client area (the window less its invisible resize borders left, right and below; a maximized window's rows above its border
		///are off the screen and cut off), the frame painted at the PanelBorders' place in it and clipped to them, the client panel filled with the
		///window background. Saved as before\frame\{name}.png or after\frame\{name}.png.</summary>
		private static void SaveWindowPicture(string folder,string beforeOrAfter,string name,FramePaintArgs framePaintArgs,PictureLayout pictureLayout,
			Action<Graphics> actionPaint)
		{
			int widthBorder=GetWidthBorderMS(framePaintArgs);
			int widthClient=framePaintArgs.Width-widthBorder*2;
			int heightClient=framePaintArgs.Height-widthBorder;
			int topOnScreen=pictureLayout.IsMaximized ? widthBorder : 0;
			using(Bitmap bitmapWindow=new Bitmap(widthClient,heightClient-topOnScreen,PixelFormat.Format24bppRgb)) {
				using(Graphics g=Graphics.FromImage(bitmapWindow)) {
					g.Clear(Color.Magenta);
					GraphicsState graphicsState=g.Save();
					g.TranslateTransform(pictureLayout.BoundsPanelBorders.Left,pictureLayout.BoundsPanelBorders.Top-topOnScreen);
					g.SetClip(new Rectangle(Point.Empty,pictureLayout.BoundsPanelBorders.Size));
					actionPaint(g);
					g.Restore(graphicsState);
					Color colorBack=beforeOrAfter=="after" ? UiTheme.Surface : Color.FromArgb(252,253,254);
					using(SolidBrush brush=new SolidBrush(colorBack)) {
						int topPanel=pictureLayout.TopClientPanel-topOnScreen;
						g.FillRectangle(brush,0,topPanel,widthClient,bitmapWindow.Height-topPanel);
					}
				}
				string folderOut=Path.Combine(folder,beforeOrAfter,"frame");
				Directory.CreateDirectory(folderOut);
				bitmapWindow.Save(Path.Combine(folderOut,name+".png"),ImageFormat.Png);
			}
		}

		///<summary>The invisible resize border of the window at its scale: round(8 + 5 x (scale - 1)), away from zero (LayoutManagerForms).</summary>
		private static int GetWidthBorderMS(FramePaintArgs framePaintArgs) {
			float scale=framePaintArgs.ScaleF(1);
			return (int)Math.Round(8+(5*(scale-1)),MidpointRounding.AwayFromZero);
		}

		///<summary>OPMS_UI3_FRAME_OUT, else ui3-frame in the test's output folder. Emptied once per run when it holds the marker of an earlier run (or
		///is empty); any other folder is refused.</summary>
		public static string GetFolderOut() {
			string folder=Environment.GetEnvironmentVariable(VARIABLE_FRAME_OUT);
			if(string.IsNullOrWhiteSpace(folder)) {
				folder=Path.Combine(AppContext.BaseDirectory,"ui3-frame");
			}
			folder=Path.GetFullPath(folder.Trim());
			lock(_lockFolder) {
				if(_isFolderReady) {
					return folder;
				}
				if(Directory.Exists(folder) && Directory.EnumerateFileSystemEntries(folder).Any()) {
					if(!File.Exists(Path.Combine(folder,MARKER))) {
						Assert.Fail(folder+" is not empty and is not an earlier frame gallery ("+MARKER+"): name an empty folder in "+VARIABLE_FRAME_OUT+".");
					}
					foreach(string sub in new[] { "before","after" }) {
						if(Directory.Exists(Path.Combine(folder,sub))) {
							Directory.Delete(Path.Combine(folder,sub),true);
						}
					}
				}
				Directory.CreateDirectory(folder);
				File.WriteAllText(Path.Combine(folder,MARKER),"The frame pictures of package UI-3 (tests/UiRefreshTests/FrameTests.cs); a later run empties this folder.\r\n");
				_isFolderReady=true;
			}
			return folder;
		}
		#endregion Gallery G-UI2 (frame part, off screen)

		#region Helpers
		private static FormFrame NewFormFrame() {
			FormFrame formFrame=new FormFrame();
			formFrame.InitializeFormMaker(new TestFrm());
			return formFrame;
		}

		private static void Prepare(TestFrameWindow testFrameWindow,float scale,bool hasMinMax) {
			testFrameWindow.Text="Patient Edit";
			testFrameWindow.MinimizeBox=hasMinMax;
			testFrameWindow.MaximizeBox=hasMinMax;
			testFrameWindow.SetScale(scale);
			testFrameWindow.Size=new Size((int)Math.Round(460*scale),(int)Math.Round(260*scale));
			IntPtr handle=testFrameWindow.Handle;//the program icon is set when the handle is created (OnHandleCreated)
			LayOutAsOnLoad(testFrameWindow);
		}

		private static void PrepareFrame(FormFrame formFrame,float scale,bool hasMinMax) {
			formFrame.Text="Patient Edit";
			formFrame.MinimizeBox=hasMinMax;
			formFrame.MaximizeBox=hasMinMax;
			formFrame.UIManager_.SetScaleMS(scale);
			formFrame.Size=new Size((int)Math.Round(460*scale),(int)Math.Round(260*scale));
			IntPtr handle=formFrame.Handle;
			LayOutAsOnLoad(formFrame);
		}

		///<summary>Lays the window out as FormODBase.OnLoad does (without showing it): PanelClient and PanelBorders made, then
		///LayoutFormBoundsAndFonts, which puts the PanelBorders at (-1,-1) of the client area (or one border lower, maximized).</summary>
		private static void LayOutAsOnLoad(FormODBase formODBase) {
			LayoutManagerForms layoutManager=WindowBuilder.GetLayoutManager(formODBase);
			if(formODBase.PanelClient==null || formODBase.PanelBorders==null) {
				layoutManager.SetPanelClient(formODBase);
			}
			layoutManager.LayoutFormBoundsAndFonts(formODBase);
		}

		///<summary>Lays the frame out as FormFrame.OnLoad does (without showing it): the PanelBorders and the ElementHost made, then
		///UIManager.LayoutFormBoundsAndFonts.</summary>
		private static void LayOutAsOnLoad(FormFrame formFrame) {
			if(formFrame.PanelBorders==null) {
				formFrame.UIManager_.SetElementHost();
			}
			formFrame.UIManager_.LayoutFormBoundsAndFonts();
		}

		///<summary>FormFrame's three Misc Colors (static, read from the definitions cache in its Load, which these frames never run).</summary>
		private static void SetFormFrameColors(Color colorBorder,Color colorOutline,Color colorText) {
			foreach((string name,Color color) in new[] { ("_colorBorder",colorBorder),("_colorBorderOutline",colorOutline),("_colorBorderText",colorText) }) {
				FieldInfo fieldInfo=typeof(FormFrame).GetField(name,BindingFlags.Static|BindingFlags.NonPublic);
				Assert.IsNotNull(fieldInfo,"FormFrame."+name);
				fieldInfo.SetValue(null,color);
			}
		}

		///<summary>Paints a laid-out frame as the screen shows it into a bitmap of its client area (the window less its invisible resize borders
		///left, right and below), first whole at rest, then for minimise, maximise and close on hover and back at rest, each time repainting only
		///what the frame invalidates (its InvalidateCaptionButtons, caught through the PanelBorders' Invalidated event), as Windows does with
		///the PanelBorders' paint. After each hover the fill's colour must cover exactly the button's columns (the close button's up to the client
		///area's last column) from the title bar's top row on screen to the row above the client panel; at rest no hover colour is left.</summary>
		private static void CheckHoverAsOnScreen(Form form,Control panelBorders,int topClientPanel,Action<Graphics> actionPaint,float scale,
			bool isMaximized,FrameColors frameColors,string name,List<string> listFailures)
		{
			if(panelBorders==null) {
				listFailures.Add(name+": the PanelBorders were not laid out");
				return;
			}
			IntPtr handlePanel=panelBorders.Handle;//Invalidate raises Invalidated only for a control with a handle (never shown: its window is hidden)
			int widthBorderMS=FramePainter.GetWidthBorderMS(scale);
			Size sizeClient=new Size(form.Width-widthBorderMS*2,form.Height-widthBorderMS);
			int topOnScreen=isMaximized ? widthBorderMS : 0;//a maximized window's client rows above its border are off the screen
			using Bitmap bitmap=new Bitmap(sizeClient.Width,sizeClient.Height,PixelFormat.Format24bppRgb);
			using Graphics g=Graphics.FromImage(bitmap);
			g.Clear(Color.Magenta);
			SetHot(form,"rest");
			PaintPanelAsOnScreen(g,panelBorders,actionPaint,new Rectangle(Point.Empty,panelBorders.Size));
			FramePaintResult framePaintResult=GetResult(form);
			foreach(string state in new[] { "hotMin","hotMax","hotX","rest" }) {
				SetHot(form,state);
				List<Rectangle> listInvalidated=new List<Rectangle>();
				InvalidateEventHandler invalidateEventHandler=(sender,e) => listInvalidated.Add(e.InvalidRect);
				panelBorders.Invalidated+=invalidateEventHandler;
				MethodInfo methodInfo=FindMethod(form.GetType(),"InvalidateCaptionButtons");
				Assert.IsNotNull(methodInfo,"InvalidateCaptionButtons on "+form.GetType().Name);
				methodInfo.Invoke(form,null);
				panelBorders.Invalidated-=invalidateEventHandler;
				if(listInvalidated.Count==0) {
					listFailures.Add(name+", "+state+": nothing was invalidated");
					continue;
				}
				Rectangle rectangleInvalid=listInvalidated.Aggregate(Rectangle.Union);
				PaintPanelAsOnScreen(g,panelBorders,actionPaint,rectangleInvalid);
				//The title bar band on screen, in client coordinates.
				Rectangle rectangleBand=Rectangle.FromLTRB(0,topOnScreen,sizeClient.Width,topClientPanel);
				if(state=="rest") {
					int countLeft=CountColor(bitmap,rectangleBand,frameColors.CloseHover)+CountColor(bitmap,rectangleBand,frameColors.CaptionHover);
					if(countLeft>0) {
						listFailures.Add(name+", back at rest: "+countLeft+" pixels of a hover colour are left");
					}
					continue;
				}
				bool isClose=state=="hotX";
				Rectangle rectangleHit=state=="hotX" ? framePaintResult.RectangleButX
					: state=="hotMax" ? framePaintResult.RectangleButMax : framePaintResult.RectangleButMin;
				rectangleHit.Offset(panelBorders.Location);
				Color colorHover=isClose ? frameColors.CloseHover : frameColors.CaptionHover;
				int right=isClose ? sizeClient.Width : Math.Min(rectangleHit.Right,sizeClient.Width);
				Rectangle rectangleExpected=Rectangle.FromLTRB(rectangleHit.Left,topOnScreen,right,topClientPanel);
				Rectangle rectangleFill=GetBounds(bitmap,rectangleBand,colorHover);
				if(rectangleFill!=rectangleExpected) {
					listFailures.Add(name+", "+state+": the hover fill on screen is "+rectangleFill+", the title bar's extent for the button is "
						+rectangleExpected+" (invalidated "+rectangleInvalid+" of the PanelBorders at "+panelBorders.Location+")");
					continue;
				}
				//Its edges are whole (the glyph is inside): the top row, the bottom row, the left and the right column.
				int countEdge=0;
				for(int x=rectangleExpected.Left;x<rectangleExpected.Right;x++) {
					countEdge+=bitmap.GetPixel(x,rectangleExpected.Top).ToArgb()==colorHover.ToArgb() ? 0 : 1;
					countEdge+=bitmap.GetPixel(x,rectangleExpected.Bottom-1).ToArgb()==colorHover.ToArgb() ? 0 : 1;
				}
				for(int y=rectangleExpected.Top;y<rectangleExpected.Bottom;y++) {
					countEdge+=bitmap.GetPixel(rectangleExpected.Left,y).ToArgb()==colorHover.ToArgb() ? 0 : 1;
					countEdge+=bitmap.GetPixel(rectangleExpected.Right-1,y).ToArgb()==colorHover.ToArgb() ? 0 : 1;
				}
				if(countEdge>0) {
					listFailures.Add(name+", "+state+": "+countEdge+" pixels on the edges of the fill "+rectangleExpected+" are not the hover colour");
				}
			}
		}

		///<summary>Moves the mouse over every point of the caption buttons' part of the title bar that the screen shows (the PanelBorders' points in
		///the client area, above the client panel) through the frame's own PanelBorders_MouseMove, and checks the hover state against the hit
		///area worked out here: each button's rectangle (from y 5) and its fill (its columns, the close button's to the client area's last column,
		///from the title bar's top row on screen to its bottom row), the fill's rows above the rectangle's top left out on a normal window (the
		///top resize edge). Also: a mouse down anywhere in a maximized window's fills is on the buttons; on a normal window, a mouse down in the
		///close fill's new columns below the top edge is on the buttons, and the rows above the rectangle keep the resize cursor.</summary>
		private static void CheckHitAreasPerPoint(Form form,Control panelBorders,int topClientPanel,Action<Graphics> actionPaint,float scale,
			bool isMaximized,int edgeMouseDown,string name,List<string> listFailures)
		{
			if(panelBorders==null) {
				listFailures.Add(name+": the PanelBorders were not laid out");
				return;
			}
			IntPtr handlePanel=panelBorders.Handle;
			SetHot(form,"rest");
			using(Bitmap bitmap=Paint(actionPaint,form.Size)) {
			}
			FramePaintResult framePaintResult=GetResult(form);
			FramePaintArgs framePaintArgs=form is FormFrame formFrame ? formFrame.GetFramePaintArgs() : ((FormODBase)form).GetFramePaintArgs();
			//What the screen shows, in the PanelBorders' coordinates, from the geometry (the client area is the window less the invisible borders
			//left, right and below; a maximized window's client rows above its border are off the screen).
			int widthBorderMS=FramePainter.GetWidthBorderMS(scale);
			int leftOnScreen=-panelBorders.Left;
			int topOnScreen=(isMaximized ? widthBorderMS : 0)-panelBorders.Top;
			int rightOnScreen=form.Width-widthBorderMS*2-panelBorders.Left;
			int bottomReachable=topClientPanel-panelBorders.Top;//the client panel covers the rest
			int bottomFill=framePaintArgs.MaxInset+framePaintArgs.HeightTitleBar+1;
			Rectangle[] arrayButtons=new[] { framePaintResult.RectangleButX,framePaintResult.RectangleButMax,framePaintResult.RectangleButMin };
			string[] arrayStates=new[] { "hotX","hotMax","hotMin" };
			Rectangle[] arrayFills=new Rectangle[3];
			for(int i=0;i<3;i++) {
				int right=i==0 ? rightOnScreen : Math.Min(arrayButtons[i].Right,rightOnScreen);
				arrayFills[i]=Rectangle.FromLTRB(arrayButtons[i].Left,topOnScreen,right,bottomFill);
			}
			MethodInfo methodMove=FindMethod(form.GetType(),"PanelBorders_MouseMove");
			MethodInfo methodDown=FindMethod(form.GetType(),"PanelBorders_MouseDown");
			Assert.IsNotNull(methodMove,"PanelBorders_MouseMove on "+form.GetType().Name);
			Assert.IsNotNull(methodDown,"PanelBorders_MouseDown on "+form.GetType().Name);
			int countWrong=0;
			int countDownWrong=0;
			string first=null;
			for(int y=topOnScreen;y<bottomReachable;y++) {
				for(int x=Math.Max(leftOnScreen,arrayButtons[2].Left-3);x<rightOnScreen;x++) {
					Point point=new Point(x,y);
					string stateExpected="rest";
					bool isInFill=false;
					for(int i=0;i<3;i++) {
						bool isInFillHere=arrayFills[i].Contains(point) && (isMaximized || y>=arrayButtons[i].Top);
						if(arrayButtons[i].Contains(point) || isInFillHere) {
							stateExpected=arrayStates[i];
							isInFill=isInFillHere;
							break;
						}
					}
					MouseEventArgs mouseEventArgs=new MouseEventArgs(MouseButtons.None,0,x,y,0);
					methodMove.Invoke(form,new object[] { panelBorders,mouseEventArgs });
					string stateActual=(bool)GetField(form,"_isHotX") ? "hotX" : (bool)GetField(form,"_isHotMax") ? "hotMax"
						: (bool)GetField(form,"_isHotMin") ? "hotMin" : "rest";
					if(stateActual!=stateExpected) {
						countWrong++;
						first??=name+": at "+point+" the hover is "+stateActual+", expected "+stateExpected+" (buttons "+string.Join(", ",arrayButtons)
							+", fills "+string.Join(", ",arrayFills)+")";
					}
					if(!isInFill) {
						continue;
					}
					//A mouse down on the fill: the buttons, maximized everywhere, normal below the top edge the mouse down keeps.
					if(!isMaximized && y<framePaintArgs.MaxInset+edgeMouseDown) {
						continue;
					}
					methodDown.Invoke(form,new object[] { panelBorders,mouseEventArgs });
					string region=GetField(form,"_enumMouseDownRegion").ToString();
					if(region!="Buttons") {
						countDownWrong++;
						first??=name+": a mouse down at "+point+" is on "+region+", expected the buttons";
					}
				}
			}
			if(countWrong>0 || countDownWrong>0) {
				listFailures.Add(countWrong+" hover and "+countDownWrong+" mouse down points wrong; "+first);
			}
			Rectangle rectangleX=framePaintResult.RectangleButX;
			if(isMaximized) {
				//The screen's top-right pixel is the close button's.
				Point pointCorner=new Point(rightOnScreen-1,topOnScreen);
				if(!framePaintResult.RectangleHitX.Contains(pointCorner)) {
					listFailures.Add(name+": the screen's top-right pixel "+pointCorner+" is not in the close hit area "+framePaintResult.RectangleHitX);
				}
			}
			else {
				//The top resize edge stays: above the close rectangle the resize cursor, below it the close button to the client area's right edge.
				MouseEventArgs mouseEventArgsEdge=new MouseEventArgs(MouseButtons.None,0,rightOnScreen-1,rectangleX.Top-1,0);
				methodMove.Invoke(form,new object[] { panelBorders,mouseEventArgsEdge });
				if(form.Cursor!=Cursors.SizeNS || (bool)GetField(form,"_isHotX")) {
					listFailures.Add(name+": above the close rectangle at the right edge the cursor is "+form.Cursor+" (hot "+GetField(form,"_isHotX")+"), expected the top resize");
				}
				Rectangle rectangleExpected=Rectangle.FromLTRB(rectangleX.Left,rectangleX.Top,rightOnScreen,Math.Max(rectangleX.Bottom,bottomFill));
				if(framePaintResult.RectangleHitX!=Rectangle.Union(rectangleX,rectangleExpected)) {
					listFailures.Add(name+": the close hit area is "+framePaintResult.RectangleHitX+", expected "+Rectangle.Union(rectangleX,rectangleExpected));
				}
			}
			SetHot(form,"rest");
		}

		///<summary>Checks that each caption glyph's slot is centred in its painted fill (to the pixel), that at 100% it is where the old glyph area
		///put it, and that the close glyph's painted pixels on hover sit in the middle of the red (within 2 pixels each way: antialiasing).</summary>
		private static void CheckGlyphsCentred(Form form,FramePaintArgs framePaintArgs,Action<Graphics> actionPaint,float scale,string name,List<string> listFailures) {
			SetHot(form,"hotX");
			using(Bitmap bitmap=Paint(actionPaint,form.Size)) {
				FramePaintResult framePaintResult=GetResult(form);
				Rectangle[] arrayButtons=new[] { framePaintResult.RectangleButX,framePaintResult.RectangleButMax,framePaintResult.RectangleButMin };
				int widthLine=UiTheme.GetLineWidth(framePaintArgs.ScaleF(1));
				for(int i=0;i<3;i++) {
					bool isClose=i==0;
					Rectangle rectangleFill=FramePainter.GetButtonShape(arrayButtons[i],framePaintArgs,isClose);
					Rectangle rectangleArea=FramePainter.GetGlyphArea(arrayButtons[i],framePaintArgs,isClose);
					Rectangle rectangleSlot=FramePainter.GetGlyphRectangle(arrayButtons[i],framePaintArgs,isClose);
					if(rectangleArea!=rectangleFill) {
						listFailures.Add(name+", button "+i+": the glyph area "+rectangleArea+" is not the fill "+rectangleFill);
					}
					int leftGap=rectangleSlot.Left-rectangleFill.Left;
					int rightGap=rectangleFill.Right-rectangleSlot.Right;
					int topGap=rectangleSlot.Top-rectangleFill.Top;
					int bottomGap=rectangleFill.Bottom-rectangleSlot.Bottom;
					if(Math.Abs(leftGap-rightGap)>1 || Math.Abs(topGap-bottomGap)>1) {
						listFailures.Add(name+", button "+i+": the glyph slot "+rectangleSlot+" is not centred in the fill "+rectangleFill);
					}
					if(scale==1f) {
						//Where the glyph always was: the old area, the button's columns from just inside the outline to the bottom of the title bar.
						int top=framePaintArgs.MaxInset+widthLine;
						int right=Math.Min(arrayButtons[i].Right,framePaintArgs.Width-framePaintArgs.MaxInset-widthLine);
						Rectangle rectangleOld=Rectangle.FromLTRB(arrayButtons[i].Left,top,right,Math.Max(top,framePaintArgs.MaxInset+framePaintArgs.HeightTitleBar+1));
						int slot=framePaintArgs.Scale(FramePainter.CAPTION_GLYPH_SLOT);
						Rectangle rectangleSlotOld=new Rectangle(rectangleOld.X+(rectangleOld.Width-slot)/2,rectangleOld.Y+(rectangleOld.Height-slot)/2,slot,slot);
						if(rectangleSlot!=rectangleSlotOld) {
							listFailures.Add(name+", button "+i+": at 100% the glyph moved from "+rectangleSlotOld+" to "+rectangleSlot);
						}
					}
				}
				//The close glyph's pixels (nearer its hover glyph colour than the red) in the middle of the red.
				Rectangle rectangleFillX=FramePainter.GetButtonShape(framePaintResult.RectangleButX,framePaintArgs,isClose:true);
				Color colorGlyph=UiTheme.TextOnAccent;
				Color colorBack=UiTheme.Danger;
				int left=int.MaxValue,top2=int.MaxValue,right2=int.MinValue,bottom=int.MinValue;
				for(int y=rectangleFillX.Top;y<rectangleFillX.Bottom;y++) {
					for(int x=rectangleFillX.Left;x<Math.Min(rectangleFillX.Right,bitmap.Width);x++) {
						Color color=bitmap.GetPixel(x,y);
						if(Distance(color,colorGlyph)<Distance(color,colorBack)) {
							left=Math.Min(left,x);
							top2=Math.Min(top2,y);
							right2=Math.Max(right2,x+1);
							bottom=Math.Max(bottom,y+1);
						}
					}
				}
				if(left==int.MaxValue) {
					listFailures.Add(name+": no close glyph pixels in "+rectangleFillX);
				}
				else if(Math.Abs((left-rectangleFillX.Left)-(rectangleFillX.Right-right2))>2 || Math.Abs((top2-rectangleFillX.Top)-(rectangleFillX.Bottom-bottom))>2) {
					listFailures.Add(name+": the close glyph's pixels "+Rectangle.FromLTRB(left,top2,right2,bottom)+" are not in the middle of the fill "+rectangleFillX);
				}
			}
			SetHot(form,"rest");
		}

		///<summary>Paints the frame into g as the PanelBorders show it in the client area: moved to the PanelBorders' location and clipped to
		///rectangleClip (in the PanelBorders' coordinates) and to the PanelBorders.</summary>
		private static void PaintPanelAsOnScreen(Graphics g,Control panelBorders,Action<Graphics> actionPaint,Rectangle rectangleClip) {
			GraphicsState graphicsState=g.Save();
			g.TranslateTransform(panelBorders.Left,panelBorders.Top);
			g.SetClip(Rectangle.Intersect(rectangleClip,new Rectangle(Point.Empty,panelBorders.Size)));
			actionPaint(g);
			g.Restore(graphicsState);
		}

		///<summary>The smallest rectangle that holds every pixel of the colour inside rectangle; empty when there is none.</summary>
		private static Rectangle GetBounds(Bitmap bitmap,Rectangle rectangle,Color color) {
			rectangle.Intersect(new Rectangle(0,0,bitmap.Width,bitmap.Height));
			int left=int.MaxValue;
			int top=int.MaxValue;
			int right=int.MinValue;
			int bottom=int.MinValue;
			for(int y=rectangle.Top;y<rectangle.Bottom;y++) {
				for(int x=rectangle.Left;x<rectangle.Right;x++) {
					if(bitmap.GetPixel(x,y).ToArgb()!=color.ToArgb()) {
						continue;
					}
					left=Math.Min(left,x);
					top=Math.Min(top,y);
					right=Math.Max(right,x+1);
					bottom=Math.Max(bottom,y+1);
				}
			}
			if(left==int.MaxValue) {
				return Rectangle.Empty;
			}
			return Rectangle.FromLTRB(left,top,right,bottom);
		}

		private static MethodInfo FindMethod(Type type,string name) {
			for(Type typeCur=type;typeCur!=null;typeCur=typeCur.BaseType) {
				MethodInfo methodInfo=typeCur.GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly);
				if(methodInfo!=null) {
					return methodInfo;
				}
			}
			return null;
		}

		///<summary>Sets the hover state of the frame's caption buttons (its private fields, as the mouse handlers set them), or maximizes the window.</summary>
		private static void SetHot(Form form,string state) {
			SetField(form,"_isHotMin",state=="hotMin");
			SetField(form,"_isHotMax",state=="hotMax");
			SetField(form,"_isHotX",state=="hotX");
			if(state=="maximized") {
				//The restore glyph is drawn from WindowState. The window is never shown, so this only sets the state.
				form.WindowState=FormWindowState.Maximized;
			}
		}

		private static void SetField(object target,string name,object value) {
			FieldInfo fieldInfo=FindField(target.GetType(),name);
			Assert.IsNotNull(fieldInfo,name+" on "+target.GetType().Name);
			fieldInfo.SetValue(target,value);
		}

		private static object GetField(object target,string name) {
			FieldInfo fieldInfo=FindField(target.GetType(),name);
			Assert.IsNotNull(fieldInfo,name+" on "+target.GetType().Name);
			return fieldInfo.GetValue(target);
		}

		private static FieldInfo FindField(Type type,string name) {
			for(Type typeCur=type;typeCur!=null;typeCur=typeCur.BaseType) {
				FieldInfo fieldInfo=typeCur.GetField(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly);
				if(fieldInfo!=null) {
					return fieldInfo;
				}
			}
			return null;
		}

		///<summary>The caption button rectangles the frame kept from its last paint.</summary>
		private static FramePaintResult GetResult(Form form) {
			FramePaintResult framePaintResult=new FramePaintResult();
			framePaintResult.RectangleButX=(Rectangle)GetField(form,"_rectangleButX");
			framePaintResult.RectangleButMax=(Rectangle)GetField(form,"_rectangleButMax");
			framePaintResult.RectangleButMin=(Rectangle)GetField(form,"_rectangleButMin");
			framePaintResult.RectangleButtons=(Rectangle)GetField(form,"_rectangleButtons");
			framePaintResult.RectangleHitX=(Rectangle)GetField(form,"_rectangleHitX");
			framePaintResult.RectangleHitMax=(Rectangle)GetField(form,"_rectangleHitMax");
			framePaintResult.RectangleHitMin=(Rectangle)GetField(form,"_rectangleHitMin");
			return framePaintResult;
		}

		private static Bitmap Paint(Action<Graphics> actionPaint,Size size) {
			Bitmap bitmap=new Bitmap(size.Width,size.Height,PixelFormat.Format24bppRgb);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.Magenta);
				actionPaint(g);
			}
			return bitmap;
		}

		///<summary>Null when the two bitmaps have the same size and pixels, else where they first differ.</summary>
		private static string CompareBitmaps(Bitmap bitmap1,Bitmap bitmap2) {
			if(bitmap1.Size!=bitmap2.Size) {
				return "sizes "+bitmap1.Size+" and "+bitmap2.Size;
			}
			int countDifferent=0;
			string first=null;
			for(int y=0;y<bitmap1.Height;y++) {
				for(int x=0;x<bitmap1.Width;x++) {
					if(bitmap1.GetPixel(x,y).ToArgb()!=bitmap2.GetPixel(x,y).ToArgb()) {
						countDifferent++;
						if(first==null) {
							first="first at ("+x+","+y+"): "+bitmap1.GetPixel(x,y)+" and "+bitmap2.GetPixel(x,y);
						}
					}
				}
			}
			if(countDifferent==0) {
				return null;
			}
			return countDifferent+" pixels differ, "+first;
		}

		private static int CountColor(Bitmap bitmap,Rectangle rectangle,Color color) {
			int count=0;
			rectangle.Intersect(new Rectangle(0,0,bitmap.Width,bitmap.Height));
			for(int y=rectangle.Top;y<rectangle.Bottom;y++) {
				for(int x=rectangle.Left;x<rectangle.Right;x++) {
					if(bitmap.GetPixel(x,y).ToArgb()==color.ToArgb()) {
						count++;
					}
				}
			}
			return count;
		}

		///<summary>The pixels in the rectangle nearer colorGlyph than colorBack (the sum of the channel differences).</summary>
		private static int CountNear(Bitmap bitmap,Rectangle rectangle,Color colorGlyph,Color colorBack) {
			int count=0;
			rectangle.Intersect(new Rectangle(0,0,bitmap.Width,bitmap.Height));
			for(int y=rectangle.Top;y<rectangle.Bottom;y++) {
				for(int x=rectangle.Left;x<rectangle.Right;x++) {
					Color color=bitmap.GetPixel(x,y);
					if(Distance(color,colorGlyph)<Distance(color,colorBack)) {
						count++;
					}
				}
			}
			return count;
		}

		private static int Distance(Color color1,Color color2) {
			return Math.Abs(color1.R-color2.R)+Math.Abs(color1.G-color2.G)+Math.Abs(color1.B-color2.B);
		}

		private static string Hex(Color color) {
			return "#"+(color.ToArgb()&0xFFFFFF).ToString("X6");
		}

		private static string RepoRoot() {
			string dir=AppContext.BaseDirectory;
			while(dir!=null && !File.Exists(Path.Combine(dir,"OpenDental.sln"))) {
				dir=Path.GetDirectoryName(dir.TrimEnd(Path.DirectorySeparatorChar));
			}
			Assert.IsNotNull(dir,"the repository root above "+AppContext.BaseDirectory);
			return dir;
		}
		#endregion Helpers
	}

	///<summary>A FormODBase window for the frame tests, written like designer code, never shown. Its static Misc Colors are FormODBase's own.</summary>
	public class TestFrameWindow : FormODBase {
		public TestFrameWindow() {
			InitializeComponent();
			InitializeLayoutManager();
		}

		private void InitializeComponent() {
			SuspendLayout();
			ClientSize=new Size(444,222);
			Name="TestFrameWindow";
			Text="Patient Edit";
			ResumeLayout(false);
		}

		public void SetScale(float scale) {
			LayoutManager.SetScaleMS(scale);
		}

		///<summary>The main window's larger title (FormOpenDental sets _fontTitle).</summary>
		public void SetMainWindowTitle() {
			_fontTitle=new Font(UiTheme.FontFamilyName,13f);
		}

		///<summary>Sets FormODBase's three Misc Colors, as FormOpenDental does at start-up. They are static: every FormODBase window draws them.</summary>
		public static void SetOfficeColors(Color colorBorder,Color colorOutline,Color colorText) {
			using(TestFrameWindow testFrameWindow=new TestFrameWindow()) {
				testFrameWindow.SetBorderColor(DefCatMiscColors.MainBorder,colorBorder);
				testFrameWindow.SetBorderColor(DefCatMiscColors.MainBorderOutline,colorOutline);
				testFrameWindow.SetBorderColor(DefCatMiscColors.MainBorderText,colorText);
			}
		}

		///<summary>SetOfficeColors through this window's own SetBorderColor (the hook FormOpenDental and FormDefEdit call), for the DWM border tests.</summary>
		public void SetOfficeColorsHere(Color colorBorder,Color colorOutline,Color colorText) {
			SetBorderColor(DefCatMiscColors.MainBorder,colorBorder);
			SetBorderColor(DefCatMiscColors.MainBorderOutline,colorOutline);
			SetBorderColor(DefCatMiscColors.MainBorderText,colorText);
		}
	}

	///<summary>A WPF window for the FormFrame tests: no XAML, a size, never shown.</summary>
	public class TestFrm : FrmODBase {
		public TestFrm() {
			Width=444;
			Height=222;
			Text="Patient Edit";
		}
	}

	///<summary>The frame as FormODBase and FormFrame drew it before UI-3 (their PanelBorders_Paint up to main 3809f35db, the before set's commit):
	///the "before" pictures of the frame gallery. Only for the pictures; nothing in the program uses it.</summary>
	public static class LegacyFramePainter {
		public static void Paint(Graphics g,FramePaintArgs args,Color colorBorder,Color colorBorderText,Color colorBorderOutline,Color colorButtonHot) {
			Color colorRedX=Color.FromArgb(232,17,35);
			using SolidBrush brushBorder=new SolidBrush(colorBorder);
			using SolidBrush brushText=new SolidBrush(colorBorderText);
			using SolidBrush brushRedX=new SolidBrush(colorRedX);
			using SolidBrush brushButtonHot=new SolidBrush(colorButtonHot);
			using Pen penButtons=new Pen(colorBorderText);
			using Pen penOutline=new Pen(colorBorderOutline);
			Func<float,int> Scale=args.Scale;
			int maxInset=args.MaxInset;
			g.FillRectangle(brushBorder,new Rectangle(0,0,args.Width,args.Height));
			g.DrawRectangle(penOutline,maxInset,maxInset,args.Width-1-maxInset*2,args.Height-1-maxInset*2);
			int wEdge=args.HeightTitleBar+Scale(4);
			int hEdge=args.HeightTitleBar-1;
			int xPos=args.Width-args.WidthBorder-maxInset;
			int yPos=5+maxInset;
			Rectangle rectangleButX=Rectangle.Empty;
			Rectangle rectangleButMax=Rectangle.Empty;
			Rectangle rectangleButMin=Rectangle.Empty;
			if(args.ControlBox) {
				xPos-=wEdge;
				rectangleButX=new Rectangle(xPos,yPos,wEdge,hEdge);
			}
			if(args.ControlBox && args.MaximizeBox) {
				xPos-=wEdge;
				rectangleButMax=new Rectangle(xPos,yPos,wEdge,hEdge);
			}
			if(args.ControlBox && args.MinimizeBox) {
				xPos-=wEdge;
				rectangleButMin=new Rectangle(xPos,yPos,wEdge,hEdge);
			}
			Rectangle rectangleButtons=new Rectangle(xPos,yPos,args.Width-xPos,args.HeightTitleBar);
			g.SmoothingMode=SmoothingMode.Default;
			int indentTop=Scale(5);
			int indentLeft=Scale(10);
			if(args.IsHotX) {
				g.FillRectangle(brushRedX,rectangleButX);
			}
			g.SmoothingMode=SmoothingMode.HighQuality;
			int whSymbol=Scale(9);
			g.DrawLine(penButtons,rectangleButX.X+indentLeft-0.2f,rectangleButX.Y+indentTop,rectangleButX.X+indentLeft+whSymbol-0.2f,rectangleButX.Y+indentTop+whSymbol);
			g.DrawLine(penButtons,rectangleButX.X+indentLeft+0.2f,rectangleButX.Y+indentTop,rectangleButX.X+indentLeft+whSymbol+0.2f,rectangleButX.Y+indentTop+whSymbol);
			g.DrawLine(penButtons,rectangleButX.X+whSymbol+indentLeft-0.2f,rectangleButX.Y+indentTop,rectangleButX.X+indentLeft-0.2f,rectangleButX.Y+indentTop+whSymbol);
			g.DrawLine(penButtons,rectangleButX.X+whSymbol+indentLeft+0.2f,rectangleButX.Y+indentTop,rectangleButX.X+indentLeft+0.2f,rectangleButX.Y+indentTop+whSymbol);
			g.SmoothingMode=SmoothingMode.Default;
			if(args.MaximizeBox) {
				if(args.IsHotMax) {
					g.FillRectangle(brushButtonHot,rectangleButMax);
				}
				if(args.IsMaximized) {
					g.DrawRectangle(penButtons,rectangleButMax.X+indentLeft,rectangleButMax.Y+indentTop+Scale(2),Scale(7),Scale(7));
					g.DrawLine(penButtons,rectangleButMax.X+indentLeft+Scale(2),rectangleButMax.Y+indentTop+Scale(2),rectangleButMax.X+indentLeft+Scale(2),rectangleButMax.Y+indentTop);
					g.DrawLine(penButtons,rectangleButMax.X+indentLeft+Scale(2),rectangleButMax.Y+indentTop,rectangleButMax.X+indentLeft+Scale(9),rectangleButMax.Y+indentTop);
					g.DrawLine(penButtons,rectangleButMax.X+indentLeft+Scale(9),rectangleButMax.Y+indentTop,rectangleButMax.X+indentLeft+Scale(9),rectangleButMax.Y+indentTop+Scale(7));
					g.DrawLine(penButtons,rectangleButMax.X+indentLeft+Scale(9),rectangleButMax.Y+indentTop+Scale(7),rectangleButMax.X+indentLeft+Scale(7),rectangleButMax.Y+indentTop+Scale(7));
				}
				else {
					g.DrawRectangle(penButtons,rectangleButMax.X+indentLeft,rectangleButMax.Y+indentTop,whSymbol,whSymbol);
				}
			}
			if(args.MinimizeBox) {
				if(args.IsHotMin) {
					g.FillRectangle(brushButtonHot,rectangleButMin);
				}
				g.DrawLine(penButtons,rectangleButMin.X+indentLeft,rectangleButMin.Y+indentTop+whSymbol/2,rectangleButMin.X+indentLeft+whSymbol,rectangleButMin.Y+indentTop+whSymbol/2);
			}
			if(args.TitleFonts==null) {
				return;
			}
			Rectangle rectangleIcon=new Rectangle(maxInset+5,maxInset+4,Scale(20),Scale(20));
			if(args.ShowIcon && args.Icon!=null) {
				using Bitmap bitmapIcon=args.Icon.ToBitmap();
				g.DrawImage(bitmapIcon,rectangleIcon);
			}
			if(!args.IsMainWindowTitle) {
				using Font font=new Font(UiTheme.FontFamilyName,args.TitleFonts.FontSize);
				RectangleF rectangleText=new RectangleF(maxInset+args.ScaleF(30),maxInset+args.ScaleF(7),
					rectangleButtons.Left-args.ScaleF(30)-maxInset+args.ScaleF(8),args.TitleFonts.HeightFont+2);
				g.DrawString(args.Text,font,brushText,rectangleText);
			}
			else {
				using Font fontMainWindow=new Font(UiTheme.FontFamilyName,args.TitleFonts.FontSizeMainWindow);
				RectangleF rectangleText=new RectangleF(maxInset+args.ScaleF(30),maxInset+args.ScaleF(3),
					rectangleButtons.Left-args.ScaleF(30)-maxInset+args.ScaleF(8),args.TitleFonts.HeightFontMainWindow+2);
				g.DrawString(args.Text,fontMainWindow,brushText,rectangleText);
			}
		}
	}
}
