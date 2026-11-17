// The design sheet's pages (package UI-1, gallery G-UI1 part 1; docs/specs/ui-refresh-plan.md sections 3 and 8, docs/port-notes/UI-1.md): the
// colour swatches with names, hex values and contrast ratios; the contrast table; the type ramp in Segoe UI; spacing, corners, lines, icon slots
// and the pixel-snapping rule; every control family's states at 100, 125, 150, 175 and 200%; the owner's options UQ-1 to UQ-4 side by side; and
// the high-contrast mapping in the four Windows contrast themes. Drawn off screen from the tokens, into PNG files.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeBase;

namespace UiRefreshTests {
	public static class DesignSheetPages {
		public static readonly float[] ArrayScales=new float[] { 1f,1.25f,1.5f,1.75f,2f };
		private const int PAGE_WIDTH=1280;

		///<summary>What the look is, in plain words (the top of the sheet and of the gallery page).</summary>
		public const string SUMMARY="The new look is calm and flat: light surfaces, thin grey borders, small rounded corners (4 px on controls, 6 px on boxes), "
			+"and one accent colour, the program's own blue #3C5A96, on the title bars, grid titles, the main button of each window, focus and selection. "
			+"Text is Segoe UI 9 pt, Windows' own font. Toolbars and the module bar are flat light grey and their buttons show only on hover. No gradients, "
			+"no shadows, no text glow. Nothing in the program changes yet: these pages show the design the control packages will follow once you approve it. "
			+"The real windows show only the font options, still in today's colours and controls. Icons here are simple stand-ins; the Fluent icons come later "
			+"(G-UI1 parts 3 and 4).";

		private static readonly Dictionary<EnumUiColor,string> _dictUses=new Dictionary<EnumUiColor,string> {
			{ EnumUiColor.Surface,"window and panel background" },{ EnumUiColor.SurfaceRaised,"inputs, grids, lists, menus, buttons, tooltips" },
			{ EnumUiColor.SurfaceSubtle,"toolbars, module bar, grid header, tab strip" },{ EnumUiColor.SubtleHover,"toolbar, module bar, menu item hover" },
			{ EnumUiColor.SubtlePressed,"toolbar, module bar, menu item pressed" },{ EnumUiColor.ButtonHover,"button hover; disabled button fill" },
			{ EnumUiColor.ButtonPressed,"button pressed" },{ EnumUiColor.Stroke,"button and container borders, dividers" },
			{ EnumUiColor.StrokeStrong,"outline of inputs, combo, list, check box" },{ EnumUiColor.StrokeStrongHover,"those outlines on hover" },
			{ EnumUiColor.Gridline,"grid lines, menu separators" },{ EnumUiColor.Text,"all text" },{ EnumUiColor.TextSecondary,"hints, unselected tabs, captions" },
			{ EnumUiColor.TextDisabled,"disabled text and icons" },{ EnumUiColor.Accent,"title bar, grid title, default button, focus, selection bar" },
			{ EnumUiColor.AccentHover,"default button hover" },{ EnumUiColor.AccentPressed,"default button pressed; window outline" },
			{ EnumUiColor.AccentSubtle,"selected row and item, toggled toolbar button" },{ EnumUiColor.RowHover,"grid and list hover" },
			{ EnumUiColor.CaptionHover,"minimise and maximise hover" },{ EnumUiColor.Danger,"close hover, error text and state" },
			{ EnumUiColor.DangerSubtle,"error toolbar state" },{ EnumUiColor.Warning,"notify toolbar state text" },{ EnumUiColor.WarningSubtle,"notify toolbar state" },
			{ EnumUiColor.Success,"the complete icon" },{ EnumUiColor.TextOnAccent,"text and glyphs on the accent and Danger" },
			{ EnumUiColor.TextSelected,"text on a selection" },{ EnumUiColor.TitleBar,"title bar (Misc Colors 11 per item)" },
			{ EnumUiColor.TitleBarText,"title text (Misc Colors 13 per item)" },{ EnumUiColor.TitleBarOutline,"window outline (Misc Colors 12 per item)" } };

		private static string F(double value) {
			return value.ToString("0.0",CultureInfo.InvariantCulture);
		}

		private static Rectangle Row(SheetPage page,int x96,int width96,int height96) {
			return new Rectangle(page.Margin+page.S(x96),page.Y,page.S(width96),page.S(height96));
		}

		#region Colours
		public static string DrawColours(string folder) {
			using(SheetPage page=new SheetPage(PAGE_WIDTH,2400,1f,SheetPainter.Light)) {
				SheetPainter p=page.Painter;
				page.Heading("G-UI1 part 1, the design: colours (section 3.2)");
				page.Note(SUMMARY);
				page.Note("Each colour has one name (a token) and is defined once in the program (CodeBase/UI/UiTheme). The ratio is the WCAG 2.2 contrast of "
					+"the text or line that sits on it; 4.5:1 is the rule for text, 3:1 for outlines, focus and selection. In a Windows contrast theme every "
					+"token takes the Windows colour named in the last column (the last page shows it).");
				string[] arrayHeads=new string[] { "","Token","Hex","Used for","Contrast","High contrast" };
				int[] arrayX=new int[] { 0,70,230,310,640,820 };
				int[] arrayW=new int[] { 60,160,80,330,180,200 };
				for(int i=0;i<arrayHeads.Length;i++) {
					p.TextLeft(arrayHeads[i],p.FontBold,EnumUiColor.Text,Row(page,arrayX[i],arrayW[i],22));
				}
				page.Space(24);
				List<ContrastPair> listPairs=DesignSheetContrast.GetPairs();
				for(int i=0;i<UiTheme.CountColors;i++) {
					EnumUiColor uiColor=(EnumUiColor)i;
					Rectangle rectangleSwatch=Row(page,0,60,22);
					p.Fill(uiColor,rectangleSwatch,UiTheme.CORNER_CONTROL);
					p.Outline(EnumUiColor.Stroke,rectangleSwatch,1,UiTheme.CORNER_CONTROL);
					p.TextLeft(uiColor.ToString(),p.FontBody,EnumUiColor.Text,Row(page,arrayX[1],arrayW[1],22));
					p.TextLeft(UiTheme.GetHex(uiColor),p.FontBody,EnumUiColor.Text,Row(page,arrayX[2],arrayW[2],22));
					p.TextLeft(_dictUses[uiColor],p.FontBody,EnumUiColor.TextSecondary,Row(page,arrayX[3],arrayW[3],22));
					p.TextLeft(GetContrastText(uiColor,listPairs),p.FontBody,EnumUiColor.Text,Row(page,arrayX[4],arrayW[4],22));
					p.TextLeft(UiTheme.GetKnownColorHighContrast(uiColor).ToString(),p.FontBody,EnumUiColor.TextSecondary,Row(page,arrayX[5],arrayW[5],22));
					page.Space(28);
				}
				return page.Save(Path.Combine(folder,"G-UI1-01-colours.png"));
			}
		}

		///<summary>The colour's main ratio for the swatch table: what sits on it, or what it sits on.</summary>
		private static string GetContrastText(EnumUiColor uiColor,List<ContrastPair> listPairs) {
			if(uiColor==EnumUiColor.TextDisabled) {
				return F(DesignSheetContrast.GetRatio(SheetPainter.Light(uiColor),SheetPainter.Light(EnumUiColor.SurfaceRaised)))+":1 (exempt)";
			}
			ContrastPair pairOn=listPairs.FirstOrDefault(x => x.Back==uiColor && x.Look!=ContrastLook.Info && x.Look!=ContrastLook.HighContrastOnly);
			if(pairOn!=null) {
				return pairOn.Fore+" on it "+F(DesignSheetContrast.GetRatioLight(pairOn))+":1";
			}
			ContrastPair pairIs=listPairs.FirstOrDefault(x => x.Fore==uiColor && x.Look!=ContrastLook.Info && x.Look!=ContrastLook.HighContrastOnly);
			if(pairIs!=null) {
				return F(DesignSheetContrast.GetRatioLight(pairIs))+":1 on "+pairIs.Back;
			}
			return "(decorative)";
		}

		public static string DrawContrastTable(string folder) {
			List<HighContrastPalette> listPalettes=DesignSheetContrast.GetPalettes();
			using(SheetPage page=new SheetPage(PAGE_WIDTH,2600,1f,SheetPainter.Light)) {
				SheetPainter p=page.Painter;
				page.Heading("G-UI1 part 1: every contrast pair (sections 3.2 and 3.6)");
				page.Note("Each row is something drawn on something else, with the WCAG 2.2 ratio it needs and the ratio it has, in the light look and in the four "
					+"Windows contrast themes. The test DesignSheetTokenTests checks every row. Rows marked \"info\" are shown for you and not required.");
				string[] arrayHeads=new string[] { "Sample","Foreground on background","Where","Needs","Light" }.Concat(listPalettes.Select(x => x.Name)).ToArray();
				int[] arrayX=new int[] { 0,90,370,700,760,830,900,970,1040 };
				int[] arrayW=new int[] { 80,270,320,60,70,70,70,70,80 };
				for(int i=0;i<arrayHeads.Length;i++) {
					p.TextLeft(arrayHeads[i],p.FontBold,EnumUiColor.Text,Row(page,arrayX[i],arrayW[i],22));
				}
				page.Space(24);
				foreach(ContrastPair pair in DesignSheetContrast.GetPairs()) {
					Rectangle rectangleSample=Row(page,0,80,22);
					p.Fill(pair.Back,rectangleSample,0);
					p.Outline(EnumUiColor.Stroke,rectangleSample,1,0);
					if(pair.Kind=="text") {
						p.TextCenter("Aa text",p.FontBody,pair.Fore,rectangleSample);
					}
					else {
						p.Outline(pair.Fore,Rectangle.Inflate(rectangleSample,-page.S(5),-page.S(5)),pair.Kind=="graphic" ? 1 : 2,0);
					}
					p.TextLeft(pair.Fore+" on "+pair.Back,p.FontBody,EnumUiColor.Text,Row(page,arrayX[1],arrayW[1],22));
					p.TextLeft(pair.Where,p.FontBody,EnumUiColor.TextSecondary,Row(page,arrayX[2],arrayW[2],22));
					p.TextLeft(pair.Look==ContrastLook.Info ? "info" : F(pair.Required)+":1",p.FontBody,EnumUiColor.Text,Row(page,arrayX[3],arrayW[3],22));
					if(pair.Look==ContrastLook.HighContrastOnly) {
						p.TextLeft("-",p.FontBody,EnumUiColor.TextSecondary,Row(page,arrayX[4],arrayW[4],22));
					}
					else {
						double ratio=DesignSheetContrast.GetRatioLight(pair);
						p.TextLeft(F(ratio),p.FontBody,pair.Look!=ContrastLook.Info && ratio<pair.Required ? EnumUiColor.Danger : EnumUiColor.Text,Row(page,arrayX[4],arrayW[4],22));
					}
					for(int i=0;i<listPalettes.Count;i++) {
						if(pair.Look==ContrastLook.LightOnly || pair.Look==ContrastLook.Info) {
							p.TextLeft("-",p.FontBody,EnumUiColor.TextSecondary,Row(page,arrayX[5+i],arrayW[5+i],22));
							continue;
						}
						double ratio=DesignSheetContrast.GetRatio(listPalettes[i].Resolve(pair.Fore),listPalettes[i].Resolve(pair.Back));
						p.TextLeft(F(ratio),p.FontBody,ratio<pair.Required ? EnumUiColor.Danger : EnumUiColor.Text,Row(page,arrayX[5+i],arrayW[5+i],22));
					}
					page.Space(26);
				}
				page.Space(8);
				page.Note("For you to look at: the selection bar without focus (StrokeStrong, #8A8A8A) is 3.5:1 against the white row around it, as section 3.2 "
					+"says, but 2.7:1 against the pale blue selection fill next to it. With focus the bar is the accent (5.4:1 on the fill). In a contrast theme "
					+"the selection is the Windows highlight colour, so there the fill itself carries it.");
				return page.Save(Path.Combine(folder,"G-UI1-02-contrast.png"));
			}
		}
		#endregion Colours

		#region Type and sizes
		public static string DrawType(string folder) {
			using(SheetPage page=new SheetPage(PAGE_WIDTH,1800,1f,SheetPainter.Light)) {
				SheetPainter p=page.Painter;
				page.Heading("G-UI1 part 1: the type ramp (section 3.4)");
				page.Note("Segoe UI everywhere, 9 pt body text (your answer to UQ-1). Labels and radio buttons draw at 0.96 of the size, as they do today. "
					+"The WPF windows already use Segoe UI 11.5, which draws the same as 9 pt. Printouts, sheets, the appointment book and the tooth chart keep "
					+"their own fonts.");
				const string SAMPLE="Patient name, Birthdate 01/02/1980, Balance $1,234.50 (0123456789)";
				List<Tuple<string,Font>> listRows=new List<Tuple<string,Font>> {
					Tuple.Create("Body: labels, inputs, buttons, menus. Segoe UI 9 (chosen)",new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY)),
					Tuple.Create("Labels and radio buttons: 9 x 0.96 = 8.64",new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY*UiTheme.FONT_FACTOR_LABEL)),
					Tuple.Create("Bold set in designers: Segoe UI 9 Bold",new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY,FontStyle.Bold)),
					Tuple.Create("Grid cells and headers: the body size",new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY)),
					Tuple.Create("Self-drawn titles: Segoe UI Semibold 10",new Font(UiTheme.FONT_FAMILY_SEMIBOLD,UiTheme.FONT_SIZE_TITLE)),
					Tuple.Create("Module bar captions: Segoe UI 8",new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_MODULE_CAPTION)),
					Tuple.Create("The other option (UQ-1 b): Segoe UI 8.25",new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY_COMPACT)),
					Tuple.Create("Today: Microsoft Sans Serif 8.25",new Font(UiTheme.FONT_FAMILY_TODAY,8.25f)),
					Tuple.Create("Today's grid: sans serif 8.5",new Font(FontFamily.GenericSansSerif,8.5f)),
					Tuple.Create("Today's grid title: sans serif 10 Bold",new Font(FontFamily.GenericSansSerif,10f,FontStyle.Bold)) };
				foreach(Tuple<string,Font> row in listRows) {
					using(Font font=row.Item2) {
						p.TextLeft(row.Item1,p.FontBody,EnumUiColor.TextSecondary,Row(page,0,380,24));
						p.TextLeft(SAMPLE,font,EnumUiColor.Text,Row(page,390,560,24));
						p.TextLeft("line "+font.Height+" px",p.FontNote,EnumUiColor.TextSecondary,Row(page,960,120,24));
						page.Space(30);
					}
				}
				page.Space(10);
				page.Heading("Grid rows follow the text (UQ-2)");
				page.Note("Each grid holds the same rows; the box under each is the same height, so you see how many rows fit.");
				int y=page.Y;
				int[] arrayHeights=new int[] { UiTheme.GRID_ROW_HEIGHT,UiTheme.GRID_ROW_HEIGHT_COMPACT,14 };
				string[] arrayNames=new string[] { "17 px rows, Segoe UI 9 (chosen)","16 px rows, Segoe UI 8.25","14 px rows, today" };
				for(int i=0;i<3;i++) {
					Point origin=new Point(page.Margin+page.S(i*420),y);
					p.TextLeft(arrayNames[i],p.FontBold,EnumUiColor.Text,DesignSheetControls.R(p,origin,0,0,400,22));
					Rectangle rectangleClip=DesignSheetControls.R(p,origin,0,26,400,150);
					p.G.SetClip(rectangleClip);
					DrawGridRowsSample(p,new Point(rectangleClip.X,rectangleClip.Y),arrayHeights[i],i);
					p.G.ResetClip();
					p.Outline(EnumUiColor.Stroke,rectangleClip,1,0);
				}
				page.Y=y+page.S(190);
				return page.Save(Path.Combine(folder,"G-UI1-03-type.png"));
			}
		}

		///<summary>Rows of a grid in a font option: 0 Segoe UI 9, 1 Segoe UI 8.25, 2 today's sans serif 8.5; drawn in the new colours.</summary>
		private static void DrawGridRowsSample(SheetPainter p,Point origin,int heightRow,int option) {
			using(Font font=option==0 ? new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY) : (option==1 ? new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY_COMPACT)
				: new Font(FontFamily.GenericSansSerif,8.5f)))
			{
				string[] arrayDescriptions=new string[] { "Periodic exam","Prophylaxis, adult","Bitewings, four films","Resin composite","Payment","Periapical",
					"Fluoride varnish","Sealant, per tooth","Insurance payment","Adjustment" };
				for(int r=0;r<arrayDescriptions.Length;r++) {
					Rectangle rectangleRow=new Rectangle(origin.X,origin.Y+r*heightRow,p.S(400),heightRow);
					p.Fill(r==2 ? EnumUiColor.AccentSubtle : EnumUiColor.SurfaceRaised,rectangleRow,0);
					p.Text("11/"+(r+2).ToString("00")+"/2026",font,EnumUiColor.Text,new Rectangle(rectangleRow.X+3,rectangleRow.Y,80,heightRow),
						TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine);
					p.Text(arrayDescriptions[r],font,EnumUiColor.Text,new Rectangle(rectangleRow.X+90,rectangleRow.Y,220,heightRow),
						TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine);
					p.Line(EnumUiColor.Gridline,rectangleRow.X,rectangleRow.Bottom-1,rectangleRow.Right,rectangleRow.Bottom-1,1);
					if(r==2) {
						p.Fill(EnumUiColor.Accent,new Rectangle(rectangleRow.X,rectangleRow.Y,2,heightRow),0);
					}
				}
			}
		}

		public static string DrawSizes(string folder) {
			using(SheetPage page=new SheetPage(PAGE_WIDTH,1800,1f,SheetPainter.Light)) {
				SheetPainter p=page.Painter;
				page.Heading("G-UI1 part 1: spacing, corners, lines and icon sizes (sections 3.3 and 3.5)");
				page.Note("Sizes at 100%. Spacing inside controls steps 2, 4, 8 and 12 px; spacing between controls does not change. The magnified copies on the "
					+"right show each pixel, so you can see the lines are sharp.");
				int y=page.Y;
				int[] arraySpaces=new int[] { 2,4,8,12 };
				for(int i=0;i<arraySpaces.Length;i++) {
					Point origin=new Point(page.Margin,y+page.S(i*24));
					p.Fill(EnumUiColor.Accent,DesignSheetControls.R(p,origin,0,4,arraySpaces[i],14),0);
					p.TextLeft(arraySpaces[i]+" px",p.FontBody,EnumUiColor.Text,DesignSheetControls.R(p,origin,20,0,80,22));
				}
				//Corners, lines.
				Point originShapes=new Point(page.Margin+page.S(130),y);
				Tuple<string,int,int>[] arrayShapes=new Tuple<string,int,int>[] { Tuple.Create("Controls 4 px",UiTheme.CORNER_CONTROL,1),
					Tuple.Create("Check box 2 px",UiTheme.CORNER_CHECKBOX,1),Tuple.Create("Group box 6 px",UiTheme.CORNER_CONTAINER,1),
					Tuple.Create("Grids, menus: square",0,1),Tuple.Create("Focus ring 2 px",UiTheme.CORNER_CONTROL,2) };
				for(int i=0;i<arrayShapes.Length;i++) {
					Rectangle rectangle=DesignSheetControls.R(p,originShapes,i*130,0,110,40);
					p.Fill(EnumUiColor.SurfaceRaised,rectangle,arrayShapes[i].Item2);
					p.Outline(arrayShapes[i].Item3==2 ? EnumUiColor.Accent : EnumUiColor.StrokeStrong,rectangle,arrayShapes[i].Item3,arrayShapes[i].Item2);
					p.TextCenter(arrayShapes[i].Item1,p.FontNote,EnumUiColor.TextSecondary,DesignSheetControls.R(p,originShapes,i*130,44,110,16));
				}
				Point originLines=new Point(page.Margin+page.S(130),y+page.S(72));
				Tuple<string,int,EnumUiColor>[] arrayLines=new Tuple<string,int,EnumUiColor>[] { Tuple.Create("Border 1 px",UiTheme.LINE_BORDER,EnumUiColor.Stroke),
					Tuple.Create("Focus 2 px",UiTheme.LINE_FOCUS,EnumUiColor.Accent),Tuple.Create("Tab line 2 px",UiTheme.LINE_TAB_SELECTED,EnumUiColor.Accent),
					Tuple.Create("Module mark 3 px",UiTheme.LINE_MODULE_MARK,EnumUiColor.Accent),Tuple.Create("Selection bar 2 px",UiTheme.LINE_SELECTION_BAR,EnumUiColor.Accent) };
				for(int i=0;i<arrayLines.Length;i++) {
					Rectangle rectangle=DesignSheetControls.R(p,originLines,i*130,0,110,12);
					p.Fill(arrayLines[i].Item3,new Rectangle(rectangle.X,rectangle.Y,rectangle.Width,p.S(arrayLines[i].Item2)),0);
					p.TextCenter(arrayLines[i].Item1,p.FontNote,EnumUiColor.TextSecondary,DesignSheetControls.R(p,originLines,i*130,10,110,16));
				}
				//Magnified: a 4 px corner with its 1 px border, and a focus ring, at 8 times.
				Rectangle rectangleSource=new Rectangle(originShapes.X,originShapes.Y,p.S(24),p.S(24));
				Rectangle rectangleSourceFocus=new Rectangle(originShapes.X+p.S(4*130),originShapes.Y,p.S(24),p.S(24));
				using(Bitmap bitmapCopy=page.Bitmap.Clone(rectangleSource,page.Bitmap.PixelFormat))
				using(Bitmap bitmapCopyFocus=page.Bitmap.Clone(rectangleSourceFocus,page.Bitmap.PixelFormat)) {
					p.G.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
					p.G.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.Half;
					p.G.DrawImage(bitmapCopy,new Rectangle(page.Margin+page.S(800),y,p.S(192),p.S(192)));
					p.G.DrawImage(bitmapCopyFocus,new Rectangle(page.Margin+page.S(1010),y,p.S(192),p.S(192)));
					p.G.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.Half;
					p.G.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.Default;
				}
				p.TextCenter("A 4 px corner, 8 times",p.FontNote,EnumUiColor.TextSecondary,new Rectangle(page.Margin+page.S(800),y+p.S(196),p.S(192),p.S(16)));
				p.TextCenter("The focus ring, 8 times",p.FontNote,EnumUiColor.TextSecondary,new Rectangle(page.Margin+page.S(1010),y+p.S(196),p.S(192),p.S(16)));
				page.Y=y+page.S(230);
				//Icon slots.
				page.Heading("Icon sizes (section 3.5): the largest design that fits the slot, centred, never stretched");
				y=page.Y;
				int[] arraySlots=new int[] { 16,22,24,32,48 };
				string[] arraySlotNames=new string[] { "16: menus, trees","22: toolbar, buttons","24: above 100%","32: module bar","48: module bar 150%" };
				for(int i=0;i<arraySlots.Length;i++) {
					Point origin=new Point(page.Margin+page.S(i*160),y);
					Rectangle rectangleSlot=DesignSheetControls.R(p,origin,0,0,arraySlots[i],arraySlots[i]);
					p.Fill(EnumUiColor.SurfaceRaised,rectangleSlot,0);
					p.Outline(EnumUiColor.Stroke,rectangleSlot,1,0);
					Rectangle rectangleIcon=UiTheme.GetIconRectangle(rectangleSlot);
					p.RoleIcon(4,p.C(EnumUiColor.Text),rectangleIcon);
					p.TextLeft(arraySlotNames[i],p.FontNote,EnumUiColor.TextSecondary,DesignSheetControls.R(p,origin,0,52,156,16));
					p.TextLeft("draws the "+UiTheme.GetIconDesignSize(arraySlots[i])+" px design",p.FontNote,EnumUiColor.TextSecondary,DesignSheetControls.R(p,origin,0,68,156,16));
				}
				page.Y=y+page.S(96);
				//The snapping rule.
				page.Heading("Sharp lines at every scale (section 3.3)");
				page.Note("Windows scaling would turn a 1 px line into 1.25 or 1.5 px, which smears into grey. So lines, rings and corners are rounded to whole pixels:");
				string[] arrayHeads=new string[] { "Scale","Border","Focus and selection","Control corner","Group box corner","Check box corner" };
				for(int i=0;i<arrayHeads.Length;i++) {
					p.TextLeft(arrayHeads[i],p.FontBold,EnumUiColor.Text,Row(page,i*170,165,22));
				}
				page.Space(24);
				foreach(float scale in ArrayScales) {
					string[] arrayValues=new string[] { (scale*100).ToString("0",CultureInfo.InvariantCulture)+"%",UiTheme.GetLineWidth(scale)+" px",
						UiTheme.GetFocusWidth(scale)+" px",UiTheme.GetCornerRadius(UiTheme.CORNER_CONTROL,scale)+" px",
						UiTheme.GetCornerRadius(UiTheme.CORNER_CONTAINER,scale)+" px",UiTheme.GetCornerRadius(UiTheme.CORNER_CHECKBOX,scale)+" px" };
					for(int i=0;i<arrayValues.Length;i++) {
						p.TextLeft(arrayValues[i],p.FontBody,EnumUiColor.Text,Row(page,i*170,165,22));
					}
					page.Space(24);
				}
				return page.Save(Path.Combine(folder,"G-UI1-04-sizes.png"));
			}
		}
		#endregion Type and sizes

		#region Controls
		///<summary>Every control family in every state at one scale, then the composites.</summary>
		public static string DrawControls(string folder,float scale) {
			List<SheetFamily> listFamilies=DesignSheetControls.GetFamilies();
			int widthLabel96=180;
			int width96=widthLabel96+DesignSheetControls.ArrayStateNames.Length*DesignSheetControls.CELL_WIDTH+40;
			using(SheetPage page=new SheetPage(width96,(listFamilies.Count+2)*DesignSheetControls.CELL_HEIGHT+1300,scale,SheetPainter.Light)) {
				SheetPainter p=page.Painter;
				string percent=(scale*100).ToString("0",CultureInfo.InvariantCulture)+"%";
				page.Heading("G-UI1 part 1: every control, every state, at "+percent+" (section 3.6)");
				page.Note("Drawn from the tokens with the scaling rule of section 3.3. \"-\" means the plan gives that state no look of its own (the control "
					+"package shows it as it is today, or settles it in G-UI1 part 2). Icons and arrows are stand-ins.");
				Point originMatrix=new Point(page.Margin,page.Y);
				for(int s=0;s<DesignSheetControls.ArrayStateNames.Length;s++) {
					p.TextCenter(DesignSheetControls.ArrayStateNames[s],p.FontBold,EnumUiColor.Text,
						DesignSheetControls.R(p,originMatrix,widthLabel96+s*DesignSheetControls.CELL_WIDTH,0,DesignSheetControls.CELL_WIDTH,22));
				}
				for(int f=0;f<listFamilies.Count;f++) {
					int y96=24+f*DesignSheetControls.CELL_HEIGHT;
					p.TextLeft(listFamilies[f].Name,p.FontBold,EnumUiColor.Text,DesignSheetControls.R(p,originMatrix,0,y96+8,widthLabel96,20));
					p.TextLeft(listFamilies[f].Where,p.FontNote,EnumUiColor.TextSecondary,DesignSheetControls.R(p,originMatrix,0,y96+28,widthLabel96,16));
					for(int s=0;s<DesignSheetControls.ArrayStateNames.Length;s++) {
						Rectangle cell=DesignSheetControls.R(p,originMatrix,widthLabel96+s*DesignSheetControls.CELL_WIDTH,y96,DesignSheetControls.CELL_WIDTH,
							DesignSheetControls.CELL_HEIGHT-14);
						string caption=listFamilies[f].Draw(p,cell,(SheetState)s);
						if(caption==null) {
							p.TextCenter("-",p.FontBody,EnumUiColor.TextDisabled,cell);
							continue;
						}
						if(caption!="") {
							p.TextLeft(caption,p.FontNote,EnumUiColor.TextSecondary,new Rectangle(cell.X+p.S(10),cell.Bottom,cell.Width,p.S(14)));
						}
					}
					p.Line(EnumUiColor.Gridline,originMatrix.X,originMatrix.Y+p.S(y96+DesignSheetControls.CELL_HEIGHT-1),page.Width-page.Margin,
						originMatrix.Y+p.S(y96+DesignSheetControls.CELL_HEIGHT-1),p.LineWidth);
				}
				page.Y=originMatrix.Y+p.S(24+listFamilies.Count*DesignSheetControls.CELL_HEIGHT+16);
				//The composites.
				page.Heading("Together: a window, the floating image windows, a toolbar, grids, a menu, the module bar, tabs, a calendar");
				Point origin=new Point(page.Margin,page.Y);
				Size sizeWindow=DesignSheetControls.DrawWindowComposite(p,origin);
				DesignSheetControls.DrawFloatingWindows(p,new Point(origin.X+sizeWindow.Width+p.S(30),origin.Y));
				DesignSheetControls.DrawModuleBarComposite(p,new Point(page.Width-page.Margin-p.S(80),origin.Y));
				DesignSheetControls.DrawCalendarAndProgress(p,new Point(origin.X+sizeWindow.Width+p.S(30),origin.Y+p.S(140)));
				page.Y=origin.Y+Math.Max(sizeWindow.Height,p.S(6*58))+p.S(20);
				origin=new Point(page.Margin,page.Y);
				Size sizeToolbar=DesignSheetControls.DrawToolbarComposite(p,origin);
				page.Y+=sizeToolbar.Height+p.S(16);
				origin=new Point(page.Margin,page.Y);
				Size sizeGrids=DesignSheetControls.DrawGridComposite(p,origin,UiTheme.GRID_ROW_HEIGHT,isBothGrids:true);
				Size sizeMenu=DesignSheetControls.DrawMenuComposite(p,new Point(origin.X+sizeGrids.Width+p.S(10),origin.Y));
				page.Y+=Math.Max(sizeGrids.Height,sizeMenu.Height)+p.S(16);
				origin=new Point(page.Margin,page.Y);
				Size sizeTabs=DesignSheetControls.DrawTabsComposite(p,origin);
				page.Y+=sizeTabs.Height+p.S(8);
				return page.Save(Path.Combine(folder,"G-UI1-10-controls-"+(scale*100).ToString("0",CultureInfo.InvariantCulture)+".png"));
			}
		}
		#endregion Controls

		#region Options
		public static string DrawOptions(string folder) {
			using(SheetPage page=new SheetPage(PAGE_WIDTH,2000,1f,SheetPainter.Light)) {
				SheetPainter p=page.Painter;
				page.Heading("G-UI1 part 1: your options UQ-1 to UQ-4, side by side (answered 2026-11-17, the first of each chosen)");
				//UQ-1.
				page.Heading("UQ-1 Body text size");
				int y=page.Y;
				string[] arrayNames=new string[] { "Segoe UI 9 pt (chosen)","Segoe UI 8.25 pt","Today: Microsoft Sans Serif 8.25" };
				Font[] arrayFonts=new Font[] { new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY),new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY_COMPACT),
					new Font(UiTheme.FONT_FAMILY_TODAY,8.25f) };
				try {
					for(int i=0;i<3;i++) {
						Point origin=new Point(page.Margin+page.S(i*410),y);
						p.TextLeft(arrayNames[i],p.FontBold,EnumUiColor.Text,DesignSheetControls.R(p,origin,0,0,400,22));
						DrawFormFragment(p,DesignSheetControls.R(p,origin,0,28,390,120),arrayFonts[i]);
					}
				}
				finally {
					foreach(Font font in arrayFonts) {
						font.Dispose();
					}
				}
				page.Y=y+page.S(160);
				page.Note("The five real windows (FormPatientEdit, FormProcEdit, the Account module, its toolbar and grid, a WPF window) are in the gallery "
					+"groups windows-9pt-17px and windows-8.25pt-16px, each beside today's picture.");
				//UQ-2.
				page.Heading("UQ-2 Grid row height");
				y=page.Y;
				int[] arrayRows=new int[] { UiTheme.GRID_ROW_HEIGHT,UiTheme.GRID_ROW_HEIGHT_COMPACT };
				string[] arrayRowNames=new string[] { "Rows follow the text: 17 px (chosen)","Compact: 16 px, cells in Segoe UI 8.25" };
				for(int i=0;i<2;i++) {
					Point origin=new Point(page.Margin+page.S(i*620),y);
					p.TextLeft(arrayRowNames[i],p.FontBold,EnumUiColor.Text,DesignSheetControls.R(p,origin,0,0,600,22));
					using(SheetPainter painterGrid=new SheetPainter(p.G,1f,SheetPainter.Light)) {
						if(i==1) {
							painterGrid.FontBody.Dispose();
							painterGrid.FontBody=new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY_COMPACT);
							painterGrid.FontBold.Dispose();
							painterGrid.FontBold=new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY_COMPACT,FontStyle.Bold);
						}
						DesignSheetControls.DrawGridComposite(painterGrid,new Point(origin.X,origin.Y+p.S(28)),arrayRows[i],isBothGrids:false);
					}
				}
				page.Y=y+page.S(200);
				//UQ-3.
				page.Heading("UQ-3 Title bar, and the office's Misc Colors item by item");
				y=page.Y;
				DrawTitleBar(p,new Point(page.Margin,y),"The program's blue, white text (chosen)",SheetPainter.Light(EnumUiColor.TitleBar),SheetPainter.Light(EnumUiColor.TitleBarText),
					SheetPainter.Light(EnumUiColor.TitleBarOutline));
				DrawTitleBar(p,new Point(page.Margin+page.S(620),y),"Light Windows 11 title bar",SheetPainter.Light(EnumUiColor.SurfaceSubtle),
					SheetPainter.Light(EnumUiColor.Text),SheetPainter.Light(EnumUiColor.Stroke));
				y+=page.S(70);
				//An office that changed only Main Border Text (a synthetic yellow) keeps it on the new title bar; the stock items give the tokens.
				DrawTitleBar(p,new Point(page.Margin,y),"An office that kept Open Dental's stock colours: the tokens",SheetPainter.Light(EnumUiColor.TitleBar),
					SheetPainter.Light(EnumUiColor.TitleBarText),SheetPainter.Light(EnumUiColor.TitleBarOutline));
				DrawTitleBar(p,new Point(page.Margin+page.S(620),y),"An office that changed only the text colour keeps its text colour",
					SheetPainter.Light(EnumUiColor.TitleBar),Color.FromArgb(255,224,112),SheetPainter.Light(EnumUiColor.TitleBarOutline));
				page.Y=y+page.S(76);
				//UQ-4.
				page.Heading("UQ-4 Icon colours (stand-in shapes: delete, complete, warning, main action, a plain icon)");
				y=page.Y;
				Tuple<string,Color[]>[] arrayIconOptions=new Tuple<string,Color[]>[] {
					Tuple.Create("Grey with a few role colours (chosen)",new Color[] { SheetPainter.Light(EnumUiColor.Danger),SheetPainter.Light(EnumUiColor.Success),
						SheetPainter.Light(EnumUiColor.Warning),SheetPainter.Light(EnumUiColor.Accent),SheetPainter.Light(EnumUiColor.Text) }),
					Tuple.Create("One colour everywhere",new Color[] { SheetPainter.Light(EnumUiColor.Text),SheetPainter.Light(EnumUiColor.Text),
						SheetPainter.Light(EnumUiColor.Text),SheetPainter.Light(EnumUiColor.Text),SheetPainter.Light(EnumUiColor.Text) }),
					Tuple.Create("Open Dental's colourful icons' colours",new Color[] { Color.FromArgb(220,20,20),Color.FromArgb(30,90,200),Color.FromArgb(240,190,20),
						Color.FromArgb(40,150,40),Color.FromArgb(90,90,160) }) };
				for(int i=0;i<arrayIconOptions.Length;i++) {
					Point origin=new Point(page.Margin+page.S(i*410),y);
					p.TextLeft(arrayIconOptions[i].Item1,p.FontBold,EnumUiColor.Text,DesignSheetControls.R(p,origin,0,0,400,22));
					Rectangle rectangleStrip=DesignSheetControls.R(p,origin,0,28,260,34);
					p.Fill(EnumUiColor.SurfaceSubtle,rectangleStrip,0);
					for(int k=0;k<5;k++) {
						p.RoleIcon(k,arrayIconOptions[i].Item2[k],DesignSheetControls.R(p,origin,8+k*50,33,24,24));
					}
				}
				page.Y=y+page.S(76);
				return page.Save(Path.Combine(folder,"G-UI1-20-options.png"));
			}
		}

		private static void DrawTitleBar(SheetPainter p,Point origin,string caption,Color colorBar,Color colorText,Color colorOutline) {
			Rectangle rectangle=DesignSheetControls.R(p,origin,0,0,600,58);
			p.Fill(SheetPainter.Light(EnumUiColor.Surface),rectangle,0);
			p.Fill(colorBar,DesignSheetControls.R(p,origin,0,0,600,30),0);
			p.Text(caption,p.FontBody,colorText,DesignSheetControls.R(p,origin,10,0,420,30),TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine);
			for(int k=0;k<3;k++) {
				p.CaptionGlyph(k,colorText,DesignSheetControls.R(p,origin,462+k*46,0,46,30));
			}
			p.Outline(colorOutline,rectangle,1,0);
		}

		///<summary>A label, a text box, a check box and a button in a font, in the new colours.</summary>
		private static void DrawFormFragment(SheetPainter p,Rectangle rectangle,Font font) {
			p.Fill(EnumUiColor.Surface,rectangle,0);
			p.Outline(EnumUiColor.Stroke,rectangle,1,UiTheme.CORNER_CONTAINER);
			using(Font fontLabel=new Font(font.FontFamily,font.Size*UiTheme.FONT_FACTOR_LABEL)) {
				p.Text("Last name, first name",fontLabel,EnumUiColor.Text,new Rectangle(rectangle.X+10,rectangle.Y+12,130,20),
					TextFormatFlags.Right|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine);
			}
			Rectangle rectangleInput=new Rectangle(rectangle.X+146,rectangle.Y+12,220,20);
			p.Fill(EnumUiColor.SurfaceRaised,rectangleInput,0);
			p.Outline(EnumUiColor.StrokeStrong,rectangleInput,1,0);
			p.Text("Sample, Alex",font,EnumUiColor.Text,new Rectangle(rectangleInput.X+3,rectangleInput.Y,rectangleInput.Width-6,rectangleInput.Height),
				TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine);
			Rectangle rectangleBox=new Rectangle(rectangle.X+146,rectangle.Y+46,12,12);
			p.Fill(EnumUiColor.Accent,rectangleBox,UiTheme.CORNER_CHECKBOX);
			p.CheckMark(p.C(EnumUiColor.TextOnAccent),rectangleBox);
			p.Text("Send appointment reminders by text",font,EnumUiColor.Text,new Rectangle(rectangleBox.Right+4,rectangle.Y+40,220,24),
				TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine);
			Rectangle rectangleButton=new Rectangle(rectangle.Right-170,rectangle.Bottom-36,75,26);
			p.Fill(EnumUiColor.Accent,rectangleButton,UiTheme.CORNER_CONTROL);
			p.Text("Save",font,EnumUiColor.TextOnAccent,rectangleButton,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine);
			Rectangle rectangleCancel=new Rectangle(rectangle.Right-88,rectangle.Bottom-36,75,26);
			p.Fill(EnumUiColor.SurfaceRaised,rectangleCancel,UiTheme.CORNER_CONTROL);
			p.Outline(EnumUiColor.Stroke,rectangleCancel,1,UiTheme.CORNER_CONTROL);
			p.Text("Cancel",font,EnumUiColor.Text,rectangleCancel,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine);
		}
		#endregion Options

		#region High contrast
		public static string DrawHighContrast(string folder) {
			List<HighContrastPalette> listPalettes=DesignSheetContrast.GetPalettes();
			using(SheetPage page=new SheetPage(PAGE_WIDTH,1400,1f,SheetPainter.Light)) {
				SheetPainter p=page.Painter;
				page.Heading("G-UI1 part 1: Windows contrast themes (section 3.2)");
				page.Note("When Windows runs a contrast theme (Settings > Accessibility > Contrast themes), every token takes a Windows colour instead, so the "
					+"program follows the theme the person chose. Below, the same window drawn in the light look and in each of the four Windows 11 contrast "
					+"themes (their colours as Windows defines them). Data colours (appointment and procedure colours) stay.");
				int y=page.Y;
				List<Tuple<string,Func<EnumUiColor,Color>>> listLooks=new List<Tuple<string,Func<EnumUiColor,Color>>> {
					Tuple.Create<string,Func<EnumUiColor,Color>>("Light (the new look)",SheetPainter.Light) };
				foreach(HighContrastPalette palette in listPalettes) {
					HighContrastPalette paletteThis=palette;
					listLooks.Add(Tuple.Create<string,Func<EnumUiColor,Color>>(palette.Name+" ("+palette.File+")",x => paletteThis.Resolve(x)));
				}
				for(int i=0;i<listLooks.Count;i++) {
					int column=i%2;
					int row=i/2;
					Point origin=new Point(page.Margin+page.S(column*620),y+page.S(row*300));
					p.TextLeft(listLooks[i].Item1,p.FontBold,EnumUiColor.Text,DesignSheetControls.R(p,origin,0,0,600,22));
					using(SheetPainter painterLook=new SheetPainter(p.G,1f,listLooks[i].Item2)) {
						DrawHighContrastPanel(painterLook,new Point(origin.X,origin.Y+p.S(26)));
					}
				}
				page.Y=y+page.S(((listLooks.Count+1)/2)*300);
				page.Heading("The mapping");
				int count=0;
				foreach(IGrouping<KnownColor,EnumUiColor> grouping in Enumerable.Range(0,UiTheme.CountColors).Select(x => (EnumUiColor)x)
					.GroupBy(x => UiTheme.GetKnownColorHighContrast(x)))
				{
					p.TextLeft(grouping.Key+": "+string.Join(", ",grouping),p.FontBody,EnumUiColor.Text,Row(page,0,1200,22));
					page.Space(24);
					count++;
				}
				return page.Save(Path.Combine(folder,"G-UI1-30-high-contrast.png"));
			}
		}

		///<summary>A small window in one look: title bar, text, a button with focus, the default button, a check box, a combo box, a grid with a
		///selected row, a toolbar with a toggled button and the error state.</summary>
		private static void DrawHighContrastPanel(SheetPainter p,Point origin) {
			Rectangle rectangleWindow=DesignSheetControls.R(p,origin,0,0,600,262);
			p.Fill(EnumUiColor.Surface,rectangleWindow,0);
			p.Fill(EnumUiColor.TitleBar,DesignSheetControls.R(p,origin,0,0,600,28),0);
			p.TextLeft("Procedure Edit",p.FontBody,EnumUiColor.TitleBarText,DesignSheetControls.R(p,origin,10,0,300,28));
			for(int k=0;k<3;k++) {
				p.CaptionGlyph(k,p.C(EnumUiColor.TitleBarText),DesignSheetControls.R(p,origin,462+k*46,0,46,28));
			}
			p.Outline(EnumUiColor.TitleBarOutline,rectangleWindow,1,0);
			Point originToolbar=new Point(origin.X+1,origin.Y+p.S(29));
			Rectangle rectangleStrip=DesignSheetControls.R(p,originToolbar,0,0,598,30);
			p.Fill(EnumUiColor.SurfaceSubtle,rectangleStrip,0);
			p.Line(EnumUiColor.Gridline,rectangleStrip.X,rectangleStrip.Bottom-1,rectangleStrip.Right,rectangleStrip.Bottom-1,1);
			Rectangle rectangleToggled=DesignSheetControls.R(p,originToolbar,6,3,90,24);
			p.Fill(EnumUiColor.AccentSubtle,rectangleToggled,UiTheme.CORNER_CONTROL);
			p.Outline(EnumUiColor.Accent,rectangleToggled,1,UiTheme.CORNER_CONTROL);
			p.TextCenter("Toggled",p.FontBody,EnumUiColor.TextSelected,rectangleToggled);
			Rectangle rectangleError=DesignSheetControls.R(p,originToolbar,104,3,90,24);
			p.Fill(EnumUiColor.DangerSubtle,rectangleError,UiTheme.CORNER_CONTROL);
			p.TextCenter("Error",p.FontBody,EnumUiColor.Danger,rectangleError);
			p.TextCenter("Plain",p.FontBody,EnumUiColor.Text,DesignSheetControls.R(p,originToolbar,202,3,90,24));
			p.TextLeft("Hint text",p.FontBody,EnumUiColor.TextSecondary,DesignSheetControls.R(p,origin,310,32,100,24));
			p.TextLeft("Disabled",p.FontBody,EnumUiColor.TextDisabled,DesignSheetControls.R(p,origin,420,32,100,24));
			DesignSheetControls.DrawButton(p,DesignSheetControls.R(p,origin,0,66,120,40),SheetState.Focused,isDefault:false);
			DesignSheetControls.DrawButton(p,DesignSheetControls.R(p,origin,100,66,120,40),SheetState.Normal,isDefault:true);
			DesignSheetControls.DrawCheckBox(p,DesignSheetControls.R(p,origin,200,66,120,40),SheetState.Checked);
			DesignSheetControls.DrawComboBox(p,DesignSheetControls.R(p,origin,290,66,140,40),SheetState.Focused);
			DesignSheetControls.DrawListOrGridRow(p,DesignSheetControls.R(p,origin,440,66,140,40),SheetState.Selected,isGrid:false);
			DesignSheetControls.DrawGridComposite(p,DesignSheetControls.R(p,origin,10,112,0,0).Location,UiTheme.GRID_ROW_HEIGHT,isBothGrids:false);
		}
		#endregion High contrast
	}
}
