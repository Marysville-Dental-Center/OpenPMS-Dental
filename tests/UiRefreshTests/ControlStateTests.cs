// The control state pictures of package UI-4a (docs/specs/ui-refresh-plan.md, section 7's UI-4a row and section 3.6; docs/port-notes/UI-4a.md):
// each real control of the package, WinForms and WPF, drawn off screen in each state the design sheet gives it (rest, hover, pressed, focus,
// disabled, checked, indeterminate; the default button), against the design sheet's drawing of that state at the control's own bounds
// (SheetPainter's shapes with the light tokens, the same calls DesignSheetControls makes), by the tolerance of ChartImageCompare.
// The text is the control's own (today's fonts until UI-2b): each control is pictured once without text for the chrome, compared with the
// drawing, and once with text, whose pixels must reach the state's text token. WPF draws its own antialiasing, so for WPF only the pixels the
// drawing paints in a solid token colour are compared (the edges of round shapes are left out); WinForms draws with the same GDI+ shapes as the
// sheet, so every pixel is compared. No window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToothChartImageTests;

namespace UiRefreshTests {
	[TestClass]
	public class ControlStateTests {
		#region WinForms
		[TestMethod]
		public void WinForms_Button_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(bool isPrimary in new[] { false,true }) {
					foreach(SheetState sheetState in new[] { SheetState.Normal,SheetState.Hover,SheetState.Pressed,SheetState.Focused,SheetState.Disabled }) {
						string name="WinForms Button"+(isPrimary ? " (default)" : "")+" "+sheetState;
						using(Bitmap bitmapChrome=StateControls.RenderWinFormsButton(sheetState,isPrimary,"",1f))
						using(Bitmap bitmapText=StateControls.RenderWinFormsButton(sheetState,isPrimary,"Save",1f))
						using(Bitmap bitmapExpected=StateDrawings.Button(bitmapChrome.Size,sheetState,isPrimary,1f)) {
							CompareAll(listFailures,name,bitmapExpected,bitmapChrome);
							CheckText(listFailures,name,bitmapChrome,bitmapText,StateDrawings.ButtonTextColor(sheetState,isPrimary));
						}
					}
				}
			});
			AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_CheckBox_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(SheetState sheetState in StateControls.ArrayCheckStates) {
					foreach(bool isIndeterminate in new[] { false,true }) {
						if(isIndeterminate && sheetState!=SheetState.Checked && sheetState!=SheetState.Disabled) {
							continue;
						}
						string name="WinForms CheckBox "+sheetState+(isIndeterminate ? " indeterminate" : "");
						using(Bitmap bitmapChrome=StateControls.RenderWinFormsCheckBox(sheetState,isIndeterminate,""))
						using(Bitmap bitmapText=StateControls.RenderWinFormsCheckBox(sheetState,isIndeterminate,"Active"))
						using(Bitmap bitmapExpected=StateDrawings.CheckBox(bitmapChrome.Size,new Rectangle(0,3,13,13),sheetState,isIndeterminate,1f)) {
							CompareAll(listFailures,name,bitmapExpected,bitmapChrome);
							CheckText(listFailures,name,bitmapChrome,bitmapText,SheetPainter.Light(sheetState==SheetState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.Text));
						}
					}
				}
			});
			AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_GroupBoxPanelAndToolTip_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				using(Bitmap bitmapChrome=StateControls.RenderWinFormsGroupBox("",enabled:true))
				using(Bitmap bitmapText=StateControls.RenderWinFormsGroupBox("Address",enabled:true))
				using(Bitmap bitmapExpected=StateDrawings.GroupBox(bitmapChrome.Size)) {
					CompareAll(listFailures,"WinForms GroupBox",bitmapExpected,bitmapChrome);
					CheckText(listFailures,"WinForms GroupBox",bitmapChrome,bitmapText,SheetPainter.Light(EnumUiColor.Text));
				}
				float scaleToolTip;
				using(Bitmap bitmapChrome=StateControls.RenderWinFormsToolTip("",out scaleToolTip))
				using(Bitmap bitmapText=StateControls.RenderWinFormsToolTip("The patient's city",out scaleToolTip))
				using(Bitmap bitmapExpected=StateDrawings.ToolTip(bitmapChrome.Size,scaleToolTip)) {
					CompareAll(listFailures,"WinForms ToolTipOD",bitmapExpected,bitmapChrome);
					CheckText(listFailures,"WinForms ToolTipOD",bitmapChrome,bitmapText,SheetPainter.Light(EnumUiColor.Text));
				}
				Rectangle rectangleClient;
				using(Bitmap bitmapPanel=StateControls.RenderWinFormsPanelOD(out rectangleClient))
				using(Bitmap bitmapClient=bitmapPanel.Clone(rectangleClient,PixelFormat.Format32bppArgb))
				using(Bitmap bitmapExpected=StateDrawings.PanelOD(bitmapClient.Size)) {
					CompareAll(listFailures,"WinForms PanelOD (client area)",bitmapExpected,bitmapClient);
				}
			});
			AssertNoFailures(listFailures);
		}
		#endregion WinForms

		#region WPF
		[TestMethod]
		public void Wpf_Button_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			RunWpf(() => {
				foreach(bool isPrimary in new[] { false,true }) {
					foreach(SheetState sheetState in new[] { SheetState.Normal,SheetState.Hover,SheetState.Pressed,SheetState.Focused,SheetState.Disabled }) {
						string name="WPF Button"+(isPrimary ? " (IsPrimary)" : "")+" "+sheetState;
						using(Bitmap bitmapChrome=StateControls.RenderWpfButton(sheetState,isPrimary,"",isPinned:false,1f))
						using(Bitmap bitmapText=StateControls.RenderWpfButton(sheetState,isPrimary,"Save",isPinned:false,1f))
						using(Bitmap bitmapExpected=StateDrawings.Button(bitmapChrome.Size,sheetState,isPrimary,1f,colorBack:Color.White)) {
							CompareSolid(listFailures,name,bitmapExpected,bitmapChrome);
							CheckText(listFailures,name,bitmapChrome,bitmapText,StateDrawings.ButtonTextColor(sheetState,isPrimary));
						}
					}
				}
			});
			AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_CheckBoxAndRadioButton_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			RunWpf(() => {
				foreach(SheetState sheetState in StateControls.ArrayCheckStates) {
					foreach(bool isIndeterminate in new[] { false,true }) {
						if(isIndeterminate && sheetState!=SheetState.Checked && sheetState!=SheetState.Disabled) {
							continue;
						}
						string name="WPF CheckBox "+sheetState+(isIndeterminate ? " indeterminate" : "");
						Rectangle rectangleBox;
						using(Bitmap bitmapChrome=StateControls.RenderWpfCheckBox(sheetState,isIndeterminate,"",isPinned:false,1f,out rectangleBox))
						using(Bitmap bitmapText=StateControls.RenderWpfCheckBox(sheetState,isIndeterminate,"Active",isPinned:false,1f,out rectangleBox))
						using(Bitmap bitmapExpected=StateDrawings.CheckBox(bitmapChrome.Size,rectangleBox,sheetState,isIndeterminate,1f,colorBack:Color.White,isRingOnBounds:true)) {
							CompareSolid(listFailures,name,bitmapExpected,bitmapChrome);
							CheckText(listFailures,name,bitmapChrome,bitmapText,SheetPainter.Light(sheetState==SheetState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.Text));
						}
					}
					if(sheetState==SheetState.Focused) {
						continue;//the radio button's focus is the same shared style as the check box's
					}
					string nameRadio="WPF RadioButton "+sheetState;
					Rectangle rectangleCircle;
					using(Bitmap bitmapChrome=StateControls.RenderWpfRadioButton(sheetState,"",isPinned:false,out rectangleCircle))
					using(Bitmap bitmapText=StateControls.RenderWpfRadioButton(sheetState,"Daily",isPinned:false,out rectangleCircle))
					using(Bitmap bitmapExpected=StateDrawings.RadioButton(bitmapChrome.Size,rectangleCircle,sheetState)) {
						CompareSolid(listFailures,nameRadio,bitmapExpected,bitmapChrome);
						CheckText(listFailures,nameRadio,bitmapChrome,bitmapText,SheetPainter.Light(sheetState==SheetState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.Text));
					}
				}
			});
			AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_GroupBoxLabelAndToolTip_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			RunWpf(() => {
				using(Bitmap bitmapChrome=StateControls.RenderWpfGroupBox(""))
				using(Bitmap bitmapText=StateControls.RenderWpfGroupBox("Address"))
				using(Bitmap bitmapExpected=StateDrawings.GroupBox(bitmapChrome.Size,colorBack:Color.White,colorParent:Color.White)) {
					CompareSolid(listFailures,"WPF GroupBox",bitmapExpected,bitmapChrome);
					CheckText(listFailures,"WPF GroupBox",bitmapChrome,bitmapText,SheetPainter.Light(EnumUiColor.Text));
				}
				using(Bitmap bitmapChrome=StateControls.RenderWpfToolTip(""))
				using(Bitmap bitmapText=StateControls.RenderWpfToolTip("The patient's city"))
				using(Bitmap bitmapExpected=StateDrawings.ToolTip(bitmapChrome.Size,1f,colorCorner:Color.White)) {
					CompareSolid(listFailures,"WPF ToolTip",bitmapExpected,bitmapChrome);
					CheckText(listFailures,"WPF ToolTip",bitmapChrome,bitmapText,SheetPainter.Light(EnumUiColor.Text));
				}
				using(Bitmap bitmapChrome=StateControls.RenderWpfLabel("",isPinned:false))
				using(Bitmap bitmapText=StateControls.RenderWpfLabel("City",isPinned:false)) {
					CheckText(listFailures,"WPF Label",bitmapChrome,bitmapText,SheetPainter.Light(EnumUiColor.Text));
				}
			});
			AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_PinnedLook_IsTodaysLook_AndDiffersFromTheNewLook() {
			//Office content (ContentLook.IsPinned, the eForm fill surface) keeps today's look: the button's gradient and blue border, the black
			//check, the black radio dot and black label text. The new look must differ from it, so the eForm test proves the pin.
			List<string> listFailures=new List<string>();
			RunWpf(() => {
				using(Bitmap bitmapPinned=StateControls.RenderWpfButton(SheetState.Normal,false,"",isPinned:true,1f))
				using(Bitmap bitmapNew=StateControls.RenderWpfButton(SheetState.Normal,false,"",isPinned:false,1f)) {
					//Today's border colour #1C5180 on the left edge, half way down; the new look's Stroke there.
					AssertNear(listFailures,"pinned button border",Color.FromArgb(0x1C,0x51,0x80),bitmapPinned.GetPixel(0,bitmapPinned.Height/2));
					AssertNear(listFailures,"new button border",SheetPainter.Light(EnumUiColor.Stroke),bitmapNew.GetPixel(0,bitmapNew.Height/2));
				}
				Rectangle rectangleBox;
				using(Bitmap bitmapPinned=StateControls.RenderWpfCheckBox(SheetState.Checked,false,"",isPinned:true,1f,out rectangleBox)) {
					//Today's box: a #6F6F6F border on white.
					AssertNear(listFailures,"pinned check box border",Color.FromArgb(0x6F,0x6F,0x6F),bitmapPinned.GetPixel(rectangleBox.X,rectangleBox.Y+rectangleBox.Height/2));
				}
				using(Bitmap bitmapPinned=StateControls.RenderWpfLabel("City",isPinned:true))
				using(Bitmap bitmapBlank=StateControls.RenderWpfLabel("",isPinned:true)) {
					CheckText(listFailures,"pinned label (black)",bitmapBlank,bitmapPinned,Color.Black);
				}
				EFormLookTests.FillPrefs();
				try {
					using(Bitmap bitmapPinned=EFormLookTests.RenderFill(enabled:true))
					using(Bitmap bitmapUnpinned=StateControls.RenderEFormUnpinned()) {
						ChartImageCompareResult result=ChartImageCompare.Compare(bitmapPinned,bitmapUnpinned);
						if(result.IsPass) {
							listFailures.Add("The eForm fill surface without the pin looks like the pinned one: the pin proves nothing.");
						}
					}
				}
				finally {
					OpenPmsDentalBusiness.Prefs.ClearCache();
				}
			});
			AssertNoFailures(listFailures);
		}
		#endregion WPF

		#region Helpers
		///<summary>Runs a WPF test on an STA thread, or ends it inconclusive when WPF draws nothing in this session.</summary>
		internal static void RunWpf(Action action) {
			string inconclusive=null;
			IconRendererTests.RunSta(() => {
				if(!ControlPictures.RendersWpf()) {
					inconclusive="WPF draws nothing in this session: the WPF state pictures wait for a session WPF renders in.";
					return;
				}
				action();
			});
			if(inconclusive!=null) {
				Assert.Inconclusive(inconclusive);
			}
		}

		internal static void AssertNoFailures(List<string> listFailures) {
			Assert.AreEqual(0,listFailures.Count,string.Join("\r\n",listFailures));
		}

		private static void AssertNear(List<string> listFailures,string name,Color colorExpected,Color colorActual) {
			if(!IsNear(colorExpected,colorActual,ChartImageCompare.ChannelTolerance)) {
				listFailures.Add(name+": expected "+ColorTranslator.ToHtml(colorExpected)+", got "+ColorTranslator.ToHtml(Color.FromArgb(colorActual.R,colorActual.G,colorActual.B))+".");
			}
		}

		public static bool IsNear(Color color1,Color color2,int tolerance) {
			return Math.Abs(color1.R-color2.R)<=tolerance && Math.Abs(color1.G-color2.G)<=tolerance && Math.Abs(color1.B-color2.B)<=tolerance;
		}

		///<summary>Every pixel, by ChartImageCompare's rule (WinForms: the control and the sheet draw the same GDI+ shapes).</summary>
		internal static void CompareAll(List<string> listFailures,string name,Bitmap bitmapExpected,Bitmap bitmapActual) {
			ChartImageCompareResult result=ChartImageCompare.Compare(bitmapExpected,bitmapActual);
			if(!result.IsPass) {
				string folder=ControlPictures.GetOutFolder("failures");
				ChartImageCompare.WriteFailureImages(folder,Sanitize(name),bitmapExpected,bitmapActual);
				listFailures.Add(name+": "+result+" Pictures in "+folder+".");
			}
		}

		///<summary>WPF: only the pixels the drawing paints in a solid token colour (or white) are compared, by ChartImageCompare's tolerance and
		///fractions; the antialiased edges of round shapes, which WPF and GDI+ blend differently, are left out.</summary>
		internal static void CompareSolid(List<string> listFailures,string name,Bitmap bitmapExpected,Bitmap bitmapActual) {
			if(bitmapExpected.Size!=bitmapActual.Size) {
				listFailures.Add(name+": the sizes differ ("+bitmapExpected.Size+" and "+bitmapActual.Size+").");
				return;
			}
			HashSet<int> hashSetSolid=new HashSet<int>(Enum.GetValues(typeof(EnumUiColor)).Cast<EnumUiColor>().Select(x => SheetPainter.Light(x).ToArgb()|unchecked((int)0xFF000000)));
			hashSetSolid.Add(Color.White.ToArgb());
			int width=bitmapExpected.Width;
			int height=bitmapExpected.Height;
			bool[] arraySolid=new bool[width*height];
			for(int y=0;y<height;y++) {
				for(int x=0;x<width;x++) {
					arraySolid[y*width+x]=hashSetSolid.Contains(bitmapExpected.GetPixel(x,y).ToArgb());
				}
			}
			int countCompared=0;
			int countDifferent=0;
			Point pointFirst=Point.Empty;
			for(int y=0;y<height;y++) {
				for(int x=0;x<width;x++) {
					//A solid pixel next to a blended one is on the edge of a round shape too (WPF may cover it a little): left out.
					bool isInside=true;
					for(int dy=-1;dy<=1 && isInside;dy++) {
						for(int dx=-1;dx<=1;dx++) {
							int xn=x+dx;
							int yn=y+dy;
							if(xn<0 || yn<0 || xn>=width || yn>=height) {
								continue;
							}
							if(!arraySolid[yn*width+xn]) {
								isInside=false;
								break;
							}
						}
					}
					if(!isInside) {
						continue;
					}
					Color colorExpected=bitmapExpected.GetPixel(x,y);
					countCompared++;
					if(!IsNear(colorExpected,bitmapActual.GetPixel(x,y),ChartImageCompare.ChannelTolerance)) {
						if(countDifferent==0) {
							pointFirst=new Point(x,y);
						}
						countDifferent++;
					}
				}
			}
			//The rule of ChartImageCompare (0.1% of the compared pixels), with at least one pixel allowed for these small pictures.
			int countAllowed=Math.Max(1,(int)(countCompared*ChartImageCompare.MaxFractionDifferent));
			if(countCompared==0 || countDifferent>countAllowed) {
				string folder=ControlPictures.GetOutFolder("failures");
				ChartImageCompare.WriteFailureImages(folder,Sanitize(name),bitmapExpected,bitmapActual);
				listFailures.Add(name+": "+countDifferent+" of "+countCompared+" solid pixels differ (first at "+pointFirst.X+","+pointFirst.Y+"). Pictures in "+folder+".");
			}
		}

		///<summary>The text's pixels (where the picture with text differs from the one without) must include pixels of the state's text colour.</summary>
		internal static void CheckText(List<string> listFailures,string name,Bitmap bitmapChrome,Bitmap bitmapText,Color colorText) {
			int countText=0;
			int countOfColour=0;
			for(int y=0;y<bitmapChrome.Height;y++) {
				for(int x=0;x<bitmapChrome.Width;x++) {
					Color colorWith=bitmapText.GetPixel(x,y);
					if(colorWith.ToArgb()==bitmapChrome.GetPixel(x,y).ToArgb()) {
						continue;
					}
					countText++;
					if(IsNear(colorWith,colorText,ChartImageCompare.ChannelTolerance)) {
						countOfColour++;
					}
				}
			}
			if(countText==0) {
				listFailures.Add(name+": the text draws nothing.");
				return;
			}
			if(countOfColour==0) {
				listFailures.Add(name+": none of the text's "+countText+" pixels is "+ColorTranslator.ToHtml(colorText)+".");
			}
		}

		internal static string Sanitize(string name) {
			return new string(name.Select(x => char.IsLetterOrDigit(x) ? x : '-').ToArray());
		}
		#endregion Helpers
	}
}
