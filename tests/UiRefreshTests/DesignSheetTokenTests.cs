// The tokens of the UI refresh (package UI-1, docs/specs/ui-refresh-plan.md section 3, docs/port-notes/UI-1.md): the contrast test (every text,
// border, focus and selection pair of sections 3.2 and 3.6 meets its ratio in the light look and in the high-contrast mapping, checked in the
// four Windows contrast themes), the ratios section 3.2 states, the high-contrast mapping, the WPF brushes, and the sizes and pixel-snapping
// rules of sections 3.3 and 3.5. No database, no window.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UiRefreshTests {
	[TestClass]
	public class DesignSheetTokenTests {
		#region Contrast
		[TestMethod]
		public void Contrast_EveryPairMeetsItsRatio_Light() {
			List<string> listFailures=new List<string>();
			List<ContrastPair> listPairs=DesignSheetContrast.GetPairs().Where(x => x.Look==ContrastLook.Both || x.Look==ContrastLook.LightOnly).ToList();
			Assert.IsTrue(listPairs.Count>=35,"Expected the pairs of sections 3.2 and 3.6.");
			foreach(ContrastPair pair in listPairs) {
				double ratio=DesignSheetContrast.GetRatioLight(pair);
				if(ratio<pair.Required) {
					listFailures.Add(pair+": "+ratio.ToString("0.00")+":1, needs "+pair.Required+":1");
				}
			}
			Assert.AreEqual(0,listFailures.Count,string.Join("\r\n",listFailures));
			//Every kind of pair section 9 names is there.
			foreach(string kind in new[] { "text","graphic","focus","selection" }) {
				Assert.IsTrue(listPairs.Any(x => x.Kind==kind),kind);
			}
		}

		[TestMethod]
		public void Contrast_EveryPairMeetsItsRatio_HighContrast() {
			List<string> listFailures=new List<string>();
			List<ContrastPair> listPairs=DesignSheetContrast.GetPairs().Where(x => x.Look==ContrastLook.Both || x.Look==ContrastLook.HighContrastOnly).ToList();
			List<HighContrastPalette> listPalettes=DesignSheetContrast.GetPalettes();
			Assert.AreEqual(4,listPalettes.Count);
			foreach(HighContrastPalette palette in listPalettes) {
				foreach(ContrastPair pair in listPairs) {
					double ratio=DesignSheetContrast.GetRatio(palette.Resolve(pair.Fore),palette.Resolve(pair.Back));
					if(ratio<pair.Required) {
						listFailures.Add(palette.Name+": "+pair+" = "+UiTheme.GetKnownColorHighContrast(pair.Fore)+" on "+UiTheme.GetKnownColorHighContrast(pair.Back)+": "
							+ratio.ToString("0.00")+":1, needs "+pair.Required+":1");
					}
				}
			}
			Assert.AreEqual(0,listFailures.Count,string.Join("\r\n",listFailures));
		}

		[TestMethod]
		public void Contrast_RatiosStatedInThePlan_MatchTheTokens() {
			List<string> listFailures=new List<string>();
			foreach(PlanRatio planRatio in DesignSheetContrast.GetPlanRatios()) {
				Color colorFore=planRatio.Fore.HasValue ? UiTheme.GetColorLight(planRatio.Fore.Value) : planRatio.ColorFore.Value;
				Color colorBack=planRatio.Back.HasValue ? UiTheme.GetColorLight(planRatio.Back.Value) : planRatio.ColorBack.Value;
				double ratio=Math.Round(DesignSheetContrast.GetRatio(colorFore,colorBack),planRatio.Decimals,MidpointRounding.AwayFromZero);
				bool isOk=planRatio.IsAtLeast ? ratio>=planRatio.Stated : Math.Abs(ratio-planRatio.Stated)<0.0001;
				if(!isOk) {
					listFailures.Add((planRatio.Fore?.ToString()??ColorTranslator.ToHtml(colorFore))+" on "+(planRatio.Back?.ToString()??ColorTranslator.ToHtml(colorBack))
						+": "+ratio+":1, the plan says "+(planRatio.IsAtLeast ? "at least " : "")+planRatio.Stated+":1");
				}
			}
			Assert.AreEqual(0,listFailures.Count,string.Join("\r\n",listFailures));
		}

		[TestMethod]
		public void Contrast_WcagRatio_KnownValues() {
			Assert.AreEqual(21.0,DesignSheetContrast.GetRatio(Color.Black,Color.White),0.001);
			Assert.AreEqual(1.0,DesignSheetContrast.GetRatio(Color.Red,Color.Red),0.001);
			Assert.AreEqual(4.48,DesignSheetContrast.GetRatio(Color.FromArgb(0x77,0x77,0x77),Color.White),0.01,"#777 on white, the classic 4.5 borderline");
			Assert.AreEqual(DesignSheetContrast.GetRatio(Color.Navy,Color.White),DesignSheetContrast.GetRatio(Color.White,Color.Navy),0.0001,"Order does not matter.");
		}
		#endregion Contrast

		#region High contrast mapping
		[TestMethod]
		public void HighContrast_PalettesMatchTheThemeFilesOfThisComputer() {
			string folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"Resources","Ease of Access Themes");
			if(!Directory.Exists(folder)) {
				Assert.Inconclusive("No "+folder+": the palettes are checked against Windows' theme files where Windows has them.");
			}
			List<string> listDifferences=new List<string>();
			foreach(HighContrastPalette palette in DesignSheetContrast.GetPalettes()) {
				string path=Path.Combine(folder,palette.File);
				Assert.IsTrue(File.Exists(path),path);
				Dictionary<string,Color> dictFile=ReadThemeColors(File.ReadAllLines(path));
				foreach(KeyValuePair<KnownColor,string> pair in DesignSheetContrast.DictThemeKeys) {
					Assert.IsTrue(dictFile.ContainsKey(pair.Value),palette.File+" has no "+pair.Value);
					if(dictFile[pair.Value].ToArgb()!=palette.DictColors[pair.Key].ToArgb()) {
						listDifferences.Add(palette.File+" "+pair.Value+": the file has "+ColorTranslator.ToHtml(dictFile[pair.Value])+", the test "
							+ColorTranslator.ToHtml(palette.DictColors[pair.Key]));
					}
				}
			}
			Assert.AreEqual(0,listDifferences.Count,"Windows changed its contrast themes; update DesignSheetContrast.GetPalettes:\r\n"+string.Join("\r\n",listDifferences));
		}

		///<summary>The [Control Panel\Colors] section of a .theme file: name to colour.</summary>
		public static Dictionary<string,Color> ReadThemeColors(string[] arrayLines) {
			Dictionary<string,Color> dict=new Dictionary<string,Color>(StringComparer.OrdinalIgnoreCase);
			bool isInColors=false;
			foreach(string lineRaw in arrayLines) {
				string line=lineRaw.Trim();
				if(line.StartsWith("[")) {
					isInColors=line.Equals(@"[Control Panel\Colors]",StringComparison.OrdinalIgnoreCase);
					continue;
				}
				int indexEquals=line.IndexOf('=');
				if(!isInColors || indexEquals<=0) {
					continue;
				}
				try {
					dict[line.Substring(0,indexEquals).Trim()]=DesignSheetContrast.ParseRgb(line.Substring(indexEquals+1));
				}
				catch(FormatException) {
				}
			}
			return dict;
		}

		[TestMethod]
		public void HighContrast_EveryTokenHasAPartner_InWinFormsAndWpf() {
			HashSet<KnownColor> hashSetInPalettes=new HashSet<KnownColor>(DesignSheetContrast.DictThemeKeys.Keys);
			for(int i=0;i<UiTheme.CountColors;i++) {
				EnumUiColor uiColor=(EnumUiColor)i;
				KnownColor knownColor=UiTheme.GetKnownColorHighContrast(uiColor);
				Assert.IsTrue(hashSetInPalettes.Contains(knownColor),uiColor+" maps to "+knownColor+", which the palettes do not have.");
				Assert.AreEqual(Color.FromKnownColor(knownColor).ToArgb(),UiTheme.GetColor(uiColor,isHighContrast:true).ToArgb(),uiColor.ToString());
				Assert.AreEqual(UiTheme.GetColorLight(uiColor),UiTheme.GetColor(uiColor,isHighContrast:false),uiColor.ToString());
			}
			StaRunnerHelpers.RunSta(() => {
				for(int i=0;i<UiTheme.CountColors;i++) {
					Assert.IsNotNull(UiTheme.GetSystemBrushWpf(UiTheme.GetKnownColorHighContrast((EnumUiColor)i)),((EnumUiColor)i).ToString());
				}
			});
			Assert.AreEqual(Enum.GetValues(typeof(EnumUiColor)).Length,UiTheme.CountColors,"A value for every token.");
			//Section 3.2's mapping, item by item.
			Assert.AreEqual(KnownColor.Window,UiTheme.GetKnownColorHighContrast(EnumUiColor.Surface));
			Assert.AreEqual(KnownColor.Window,UiTheme.GetKnownColorHighContrast(EnumUiColor.SurfaceRaised));
			Assert.AreEqual(KnownColor.WindowText,UiTheme.GetKnownColorHighContrast(EnumUiColor.Text));
			Assert.AreEqual(KnownColor.Highlight,UiTheme.GetKnownColorHighContrast(EnumUiColor.Accent));
			Assert.AreEqual(KnownColor.Highlight,UiTheme.GetKnownColorHighContrast(EnumUiColor.AccentSubtle));
			Assert.AreEqual(KnownColor.HighlightText,UiTheme.GetKnownColorHighContrast(EnumUiColor.TextSelected));
			Assert.AreEqual(KnownColor.WindowFrame,UiTheme.GetKnownColorHighContrast(EnumUiColor.Stroke));
			Assert.AreEqual(KnownColor.WindowFrame,UiTheme.GetKnownColorHighContrast(EnumUiColor.StrokeStrong));
			Assert.AreEqual(KnownColor.GrayText,UiTheme.GetKnownColorHighContrast(EnumUiColor.TextDisabled));
			Assert.AreEqual(KnownColor.ActiveCaption,UiTheme.GetKnownColorHighContrast(EnumUiColor.TitleBar));
			Assert.AreEqual(KnownColor.ActiveCaptionText,UiTheme.GetKnownColorHighContrast(EnumUiColor.TitleBarText));
			//The tokens follow this computer's setting.
			Assert.AreEqual(UiTheme.GetColor(EnumUiColor.Accent,UiTheme.IsHighContrast),UiTheme.Accent);
			Assert.AreEqual(System.Windows.Forms.SystemInformation.HighContrast,UiTheme.IsHighContrast);
		}
		#endregion High contrast mapping

		#region Values
		[TestMethod]
		public void Tokens_HaveSection32sValues() {
			Dictionary<EnumUiColor,string> dictHex=new Dictionary<EnumUiColor,string> {
				{ EnumUiColor.Surface,"#FAFAFA" },{ EnumUiColor.SurfaceRaised,"#FFFFFF" },{ EnumUiColor.SurfaceSubtle,"#F3F3F3" },{ EnumUiColor.SubtleHover,"#EBEBEB" },
				{ EnumUiColor.SubtlePressed,"#E0E0E0" },{ EnumUiColor.ButtonHover,"#F5F5F5" },{ EnumUiColor.ButtonPressed,"#EBEBEB" },{ EnumUiColor.Stroke,"#D1D1D1" },
				{ EnumUiColor.StrokeStrong,"#8A8A8A" },{ EnumUiColor.StrokeStrongHover,"#616161" },{ EnumUiColor.Gridline,"#E0E0E0" },{ EnumUiColor.Text,"#1F1F1F" },
				{ EnumUiColor.TextSecondary,"#5C5C5C" },{ EnumUiColor.TextDisabled,"#A0A0A0" },{ EnumUiColor.Accent,"#3C5A96" },{ EnumUiColor.AccentHover,"#324C80" },
				{ EnumUiColor.AccentPressed,"#2A406C" },{ EnumUiColor.AccentSubtle,"#DCE6F5" },{ EnumUiColor.RowHover,"#F0F4FA" },{ EnumUiColor.CaptionHover,"#5370A6" },
				{ EnumUiColor.Danger,"#C42B1C" },{ EnumUiColor.DangerSubtle,"#FDE7E9" },{ EnumUiColor.Warning,"#9D5D00" },{ EnumUiColor.WarningSubtle,"#FFF4CE" },
				{ EnumUiColor.Success,"#0F7B0F" },{ EnumUiColor.TextOnAccent,"#FFFFFF" },{ EnumUiColor.TextSelected,"#1F1F1F" },{ EnumUiColor.TitleBar,"#3C5A96" },
				{ EnumUiColor.TitleBarText,"#FFFFFF" },{ EnumUiColor.TitleBarOutline,"#2A406C" } };
			Assert.AreEqual(UiTheme.CountColors,dictHex.Count);
			foreach(KeyValuePair<EnumUiColor,string> pair in dictHex) {
				Assert.AreEqual(pair.Value,UiTheme.GetHex(pair.Key),pair.Key.ToString());
				Assert.AreEqual(255,UiTheme.GetColorLight(pair.Key).A,pair.Key.ToString());
			}
			//The role tokens are the colours section 3.2 names for them.
			Assert.AreEqual(UiTheme.GetColorLight(EnumUiColor.Accent),UiTheme.GetColorLight(EnumUiColor.TitleBar));
			Assert.AreEqual(UiTheme.GetColorLight(EnumUiColor.AccentPressed),UiTheme.GetColorLight(EnumUiColor.TitleBarOutline));
			Assert.AreEqual(UiTheme.GetColorLight(EnumUiColor.Text),UiTheme.GetColorLight(EnumUiColor.TextSelected));
		}

		[TestMethod]
		public void Wpf_BrushesAreFrozenAndMatchTheColours() {
			StaRunnerHelpers.RunSta(() => {
				for(int i=0;i<UiTheme.CountColors;i++) {
					EnumUiColor uiColor=(EnumUiColor)i;
					System.Windows.Media.SolidColorBrush brush=UiTheme.GetBrushWpfLight(uiColor);
					Assert.IsTrue(brush.IsFrozen,uiColor.ToString());
					Color color=UiTheme.GetColorLight(uiColor);
					Assert.AreEqual(System.Windows.Media.Color.FromRgb(color.R,color.G,color.B),brush.Color,uiColor.ToString());
					Assert.AreSame(brush,UiTheme.GetBrushWpfLight(uiColor),"One shared brush per token.");
				}
			});
			//Frozen brushes are free-threaded: another thread reads the same brush.
			System.Windows.Media.SolidColorBrush brushOther=null;
			System.Threading.Thread thread=new System.Threading.Thread(() => brushOther=UiTheme.GetBrushWpfLight(EnumUiColor.Accent));
			thread.Start();
			thread.Join();
			Assert.AreEqual(UiTheme.GetColorWpfLight(EnumUiColor.Accent),brushOther.Color);
		}

		[TestMethod]
		public void Fonts_AreTheOwnersAnswers() {
			Assert.AreEqual("Segoe UI",UiTheme.FONT_FAMILY);
			Assert.AreEqual(9f,UiTheme.FONT_SIZE_BODY,"UQ-1 (a)");
			Assert.AreEqual(8.25f,UiTheme.FONT_SIZE_BODY_COMPACT,"UQ-1 (b)");
			Assert.AreEqual(0.96f,UiTheme.FONT_FACTOR_LABEL,"UQ-1: the label factor is kept");
			Assert.AreEqual(17,UiTheme.GRID_ROW_HEIGHT,"UQ-2 (a)");
			Assert.AreEqual(16,UiTheme.GRID_ROW_HEIGHT_COMPACT,"UQ-2 (b)");
			Assert.AreEqual(10f,UiTheme.FONT_SIZE_TITLE);
			Assert.AreEqual(8f,UiTheme.FONT_SIZE_MODULE_CAPTION);
			Assert.AreEqual(11.5,UiTheme.FONT_SIZE_WPF_BODY);
			//Section 3.4: Segoe UI 9 pt and 11.5 DIP draw the same at 96 DPI (Font.Height 16), and grid rows follow the font (height plus 1).
			using(Font font=new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY)) {
				if(font.Name!=UiTheme.FONT_FAMILY) {
					Assert.Inconclusive("Segoe UI is not installed here.");
				}
				Assert.AreEqual(16,font.Height);
				Assert.AreEqual(UiTheme.GRID_ROW_HEIGHT,font.Height+1,"GridOD's row: _fontCell.Height+1");
			}
			using(Font font=new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY_COMPACT)) {
				Assert.AreEqual(UiTheme.GRID_ROW_HEIGHT_COMPACT,font.Height+1);
			}
		}

		[TestMethod]
		public void Sizes_PixelSnapping_Section33() {
			float[] arrayScales=new float[] { 1f,1.25f,1.5f,1.75f,2f };
			int[] arrayLines=new int[] { 1,1,1,1,2 };
			//Rounded as LayoutManager rounds (a .5 to the even neighbour): 2.5 at 125% gives 2, 3.5 at 175% gives 4.
			int[] arrayFocus=new int[] { 2,2,3,4,4 };
			for(int i=0;i<arrayScales.Length;i++) {
				Assert.AreEqual(arrayLines[i],UiTheme.GetLineWidth(arrayScales[i]),"Line at "+arrayScales[i]);
				Assert.AreEqual(arrayFocus[i],UiTheme.GetFocusWidth(arrayScales[i]),"Focus at "+arrayScales[i]);
			}
			Assert.AreEqual(5,UiTheme.GetCornerRadius(UiTheme.CORNER_CONTROL,1.25f));
			Assert.AreEqual(6,UiTheme.GetCornerRadius(UiTheme.CORNER_CONTROL,1.5f));
			Assert.AreEqual(9,UiTheme.GetCornerRadius(UiTheme.CORNER_CONTAINER,1.5f));
			//The .5 corners: 2 x 1.25 = 2.5, 6 x 1.25 = 7.5, 6 x 1.75 = 10.5, 2 x 1.75 = 3.5.
			Assert.AreEqual(2,UiTheme.GetCornerRadius(UiTheme.CORNER_CHECKBOX,1.25f));
			Assert.AreEqual(8,UiTheme.GetCornerRadius(UiTheme.CORNER_CONTAINER,1.25f));
			Assert.AreEqual(10,UiTheme.GetCornerRadius(UiTheme.CORNER_CONTAINER,1.75f));
			Assert.AreEqual(4,UiTheme.GetCornerRadius(UiTheme.CORNER_CHECKBOX,1.75f));
			Assert.AreEqual(12,UiTheme.ScaleI(10,1.25f));
			//Snapping: each edge on its own, so neighbours keep sharing an edge. 12.5 gives 12 and 37.5 gives 38, as LayoutManager rounds.
			Assert.AreEqual(Rectangle.FromLTRB(12,13,38,25),UiTheme.SnapRectangle(new RectangleF(12.5f,12.6f,25f,12.5f)));
			Rectangle rectangleA=UiTheme.ScaleRectangle(new Rectangle(0,0,10,10),1.25f);
			Rectangle rectangleB=UiTheme.ScaleRectangle(new Rectangle(10,0,10,10),1.25f);
			Assert.AreEqual(rectangleA.Right,rectangleB.Left,"Shared edge at 125%.");
			//A 1 px line inside the bounds on the pixel centres; a 2 px line on whole pixels.
			Assert.AreEqual(new RectangleF(10.5f,20.5f,99f,29f),UiTheme.GetStrokeRectangle(new Rectangle(10,20,100,30),1));
			Assert.AreEqual(new RectangleF(11f,21f,98f,28f),UiTheme.GetStrokeRectangle(new Rectangle(10,20,100,30),2));
			using(System.Drawing.Drawing2D.GraphicsPath graphicsPath=UiTheme.GetRoundedRectanglePath(new RectangleF(0,0,20,10),4)) {
				RectangleF rectangleBounds=graphicsPath.GetBounds();
				Assert.AreEqual(0f,rectangleBounds.X,0.001f);
				Assert.AreEqual(0f,rectangleBounds.Y,0.001f);
				Assert.AreEqual(20f,rectangleBounds.Width,0.001f);
				Assert.AreEqual(10f,rectangleBounds.Height,0.001f);
			}
		}

		///<summary>Section 3.3: UiTheme holds the helpers "so every control does it the same way", so a control that scales its bounds with
		///LayoutManager and its inner shapes with UiTheme must get the same pixels. Every 96 DPI value 0 to 200 at the five scales, and every
		///quarter-pixel edge 0 to 100, against LayoutManagerForms itself (no database: ScaleMy is the MS scale).</summary>
		[TestMethod]
		public void Sizes_PixelSnapping_RoundsAsLayoutManager() {
			float[] arrayScales=new float[] { 1f,1.25f,1.5f,1.75f,2f };
			OpenPmsDental.LayoutManagerForms layoutManager=new OpenPmsDental.LayoutManagerForms();
			foreach(float scale in arrayScales) {
				layoutManager.SetScaleMS(scale);
				Assert.AreEqual(scale,layoutManager.ScaleMy(),"LayoutManager's scale.");
				for(int value96=0;value96<=200;value96++) {
					Assert.AreEqual(layoutManager.Scale(value96),UiTheme.ScaleI(value96,scale),"ScaleI("+value96+") at "+scale);
				}
				Assert.AreEqual(layoutManager.Scale(UiTheme.LINE_FOCUS),UiTheme.GetFocusWidth(scale),"Focus at "+scale);
				//The location as LayoutManager.ScalePoint puts it (the size differs on purpose: SnapRectangle rounds each edge on its own).
				Rectangle rectangle96=new Rectangle(10,6,30,22);
				Assert.AreEqual(layoutManager.ScalePoint(rectangle96.Location),UiTheme.ScaleRectangle(rectangle96,scale).Location,"Location at "+scale);
			}
			for(int quarters=0;quarters<=400;quarters++) {
				float edge=quarters/4f;
				Rectangle rectangleSnapped=UiTheme.SnapRectangle(new RectangleF(edge,edge,0,0));
				Assert.AreEqual(layoutManager.Round(edge),rectangleSnapped.Left,"Edge "+edge);
			}
		}

		[TestMethod]
		public void Sizes_IconDesigns_Section35() {
			Dictionary<int,int> dictSlotToDesign=new Dictionary<int,int> { { 10,12 },{ 12,12 },{ 16,16 },{ 19,16 },{ 20,20 },{ 22,20 },{ 23,20 },{ 24,24 },{ 28,24 },
				{ 31,24 },{ 32,32 },{ 40,32 },{ 47,32 },{ 48,48 },{ 64,48 } };
			foreach(KeyValuePair<int,int> pair in dictSlotToDesign) {
				Assert.AreEqual(pair.Value,UiTheme.GetIconDesignSize(pair.Key),"Slot "+pair.Key);
			}
			//Centred, never stretched: a 22 px slot draws the 20 px design one pixel in.
			Assert.AreEqual(new Rectangle(101,201,20,20),UiTheme.GetIconRectangle(new Rectangle(100,200,22,22)));
			Assert.AreEqual(new Rectangle(0,0,10,10),UiTheme.GetIconRectangle(new Rectangle(0,0,10,10)),"Below the smallest design: the slot.");
			CollectionAssert.AreEqual(new[] { 12,16,20,24,28,32,48 },UiTheme.ArrayIconDesignSizes);
		}

		[TestMethod]
		public void Sizes_Section33Values() {
			Assert.AreEqual(4,UiTheme.CORNER_CONTROL);
			Assert.AreEqual(2,UiTheme.CORNER_CHECKBOX);
			Assert.AreEqual(6,UiTheme.CORNER_CONTAINER);
			Assert.AreEqual(1,UiTheme.LINE_BORDER);
			Assert.AreEqual(2,UiTheme.LINE_FOCUS);
			Assert.AreEqual(2,UiTheme.LINE_SELECTION_BAR);
			Assert.AreEqual(2,UiTheme.LINE_TAB_SELECTED);
			Assert.AreEqual(3,UiTheme.LINE_MODULE_MARK);
			Assert.AreEqual(4,UiTheme.GAP_ICON_TEXT);
			Assert.AreEqual(8,UiTheme.PADDING_BUTTON_TEXT);
			Assert.AreEqual(16,UiTheme.MENU_ICON_COLUMN);
			Assert.AreEqual(12,UiTheme.SIZE_CHEVRON);
			CollectionAssert.AreEqual(new[] { 2,4,8,12 },new[] { UiTheme.SPACE_2,UiTheme.SPACE_4,UiTheme.SPACE_8,UiTheme.SPACE_12 });
		}

		[TestMethod]
		public void InitWinForms_DoesNothingUntilUi6() {
			System.Windows.Forms.ToolStripRenderer rendererBefore=System.Windows.Forms.ToolStripManager.Renderer;
			UiTheme.InitWinForms();
			Assert.AreSame(rendererBefore,System.Windows.Forms.ToolStripManager.Renderer);
		}
		#endregion Values
	}

	///<summary>A plain STA run for tests that need no WinForms exception handling (WPF objects).</summary>
	public static class StaRunnerHelpers {
		public static void RunSta(Action action) {
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
	}
}
