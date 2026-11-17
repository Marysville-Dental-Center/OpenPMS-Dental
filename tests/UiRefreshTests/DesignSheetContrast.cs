// The contrast rules of the UI refresh's design (docs/specs/ui-refresh-plan.md, sections 3.2 and 3.6; package UI-1, docs/port-notes/UI-1.md):
// the WCAG 2.2 contrast ratio, every text, border, focus and selection pair of the design with the ratio it needs, the ratios section 3.2 states,
// and the four Windows contrast themes the high-contrast mapping is checked in. The design sheet shows the same table the tests check.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using CodeBase;

namespace UiRefreshTests {
	///<summary>Where a pair applies: the light look, the high-contrast mapping, or both.</summary>
	public enum ContrastLook {
		Both,
		LightOnly,
		HighContrastOnly,
		///<summary>Shown on the sheet for the owner, not required.</summary>
		Info,
	}

	///<summary>One pair of section 3.2 or 3.6: a foreground token on a background token, what it is, and the ratio it needs.</summary>
	public class ContrastPair {
		public EnumUiColor Fore;
		public EnumUiColor Back;
		///<summary>text (WCAG 1.4.3, 4.5:1), graphic (1.4.11, 3:1: an outline or icon), focus (2.4.11 and 1.4.11, 3:1) or selection (1.4.11, 3:1).</summary>
		public string Kind;
		public double Required;
		public ContrastLook Look;
		public string Where;

		public ContrastPair(EnumUiColor fore,EnumUiColor back,string kind,ContrastLook look,string where) {
			Fore=fore;
			Back=back;
			Kind=kind;
			Required=kind=="text" ? 4.5 : 3.0;
			if(look==ContrastLook.Info) {
				Required=0;
			}
			Look=look;
			Where=where;
		}

		public override string ToString() {
			return Fore+" on "+Back+" ("+Kind+", "+Where+")";
		}
	}

	///<summary>A ratio that section 3.2 of the plan states, to check the token values against the text the owner read.</summary>
	public class PlanRatio {
		public EnumUiColor? Fore;
		public EnumUiColor? Back;
		///<summary>A colour that is not a token (white as a background, the alternatives section 3.2 compares with).</summary>
		public Color? ColorFore;
		public Color? ColorBack;
		public double Stated;
		///<summary>"or more" in the plan: the computed ratio, rounded as stated, is at least Stated.</summary>
		public bool IsAtLeast;
		public int Decimals;
	}

	///<summary>One Windows contrast theme: the system colours the high-contrast mapping uses, from the theme file's [Control Panel\Colors] section.</summary>
	public class HighContrastPalette {
		public string File;
		public string Name;
		public Dictionary<KnownColor,Color> DictColors=new Dictionary<KnownColor,Color>();

		public Color Resolve(EnumUiColor uiColor) {
			return DictColors[UiTheme.GetKnownColorHighContrast(uiColor)];
		}
	}

	public static class DesignSheetContrast {
		public static readonly Color White=Color.FromArgb(255,255,255);

		///<summary>WCAG 2.2 relative luminance of an sRGB colour.</summary>
		public static double GetLuminance(Color color) {
			return 0.2126*GetLinear(color.R)+0.7152*GetLinear(color.G)+0.0722*GetLinear(color.B);
		}

		private static double GetLinear(int channel) {
			double c=channel/255.0;
			if(c<=0.04045) {
				return c/12.92;
			}
			return Math.Pow((c+0.055)/1.055,2.4);
		}

		///<summary>WCAG 2.2 contrast ratio, 1 to 21, whichever colour is lighter.</summary>
		public static double GetRatio(Color color1,Color color2) {
			double luminance1=GetLuminance(color1);
			double luminance2=GetLuminance(color2);
			return (Math.Max(luminance1,luminance2)+0.05)/(Math.Min(luminance1,luminance2)+0.05);
		}

		///<summary>Every text, border, focus and selection pair of sections 3.2 and 3.6.</summary>
		public static List<ContrastPair> GetPairs() {
			List<ContrastPair> listPairs=new List<ContrastPair>();
			//Text, 4.5:1.
			listPairs.Add(new ContrastPair(EnumUiColor.Text,EnumUiColor.Surface,"text",ContrastLook.Both,"window and panel text"));
			listPairs.Add(new ContrastPair(EnumUiColor.Text,EnumUiColor.SurfaceRaised,"text",ContrastLook.Both,"inputs, grids, lists, menus, buttons, tooltips"));
			listPairs.Add(new ContrastPair(EnumUiColor.Text,EnumUiColor.SurfaceSubtle,"text",ContrastLook.Both,"toolbar, grid header, tab strip, unselected image window"));
			listPairs.Add(new ContrastPair(EnumUiColor.Text,EnumUiColor.SubtleHover,"text",ContrastLook.Both,"toolbar, module bar, menu item on hover"));
			listPairs.Add(new ContrastPair(EnumUiColor.Text,EnumUiColor.SubtlePressed,"text",ContrastLook.Both,"toolbar, module bar, menu item pressed"));
			listPairs.Add(new ContrastPair(EnumUiColor.Text,EnumUiColor.ButtonHover,"text",ContrastLook.Both,"button on hover"));
			listPairs.Add(new ContrastPair(EnumUiColor.Text,EnumUiColor.ButtonPressed,"text",ContrastLook.Both,"button pressed"));
			listPairs.Add(new ContrastPair(EnumUiColor.Text,EnumUiColor.RowHover,"text",ContrastLook.Both,"grid and list row on hover"));
			listPairs.Add(new ContrastPair(EnumUiColor.TextSelected,EnumUiColor.AccentSubtle,"text",ContrastLook.Both,"selected row and item, toggled toolbar button"));
			listPairs.Add(new ContrastPair(EnumUiColor.TextSecondary,EnumUiColor.SurfaceRaised,"text",ContrastLook.Both,"hints and captions"));
			listPairs.Add(new ContrastPair(EnumUiColor.TextSecondary,EnumUiColor.Surface,"text",ContrastLook.Both,"hints and captions on a window"));
			listPairs.Add(new ContrastPair(EnumUiColor.TextSecondary,EnumUiColor.SurfaceSubtle,"text",ContrastLook.Both,"unselected tab"));
			listPairs.Add(new ContrastPair(EnumUiColor.TextSecondary,EnumUiColor.SubtleHover,"text",ContrastLook.Both,"unselected tab on hover"));
			listPairs.Add(new ContrastPair(EnumUiColor.TextOnAccent,EnumUiColor.Accent,"text",ContrastLook.Both,"default button, grid title, selected day"));
			listPairs.Add(new ContrastPair(EnumUiColor.TextOnAccent,EnumUiColor.AccentHover,"text",ContrastLook.Both,"default button on hover"));
			listPairs.Add(new ContrastPair(EnumUiColor.TextOnAccent,EnumUiColor.AccentPressed,"text",ContrastLook.Both,"default button pressed"));
			listPairs.Add(new ContrastPair(EnumUiColor.TextOnAccent,EnumUiColor.CaptionHover,"text",ContrastLook.Both,"minimise and maximise glyph on hover"));
			listPairs.Add(new ContrastPair(EnumUiColor.TextOnAccent,EnumUiColor.Danger,"text",ContrastLook.Both,"close glyph on hover"));
			listPairs.Add(new ContrastPair(EnumUiColor.TitleBarText,EnumUiColor.TitleBar,"text",ContrastLook.Both,"window title"));
			listPairs.Add(new ContrastPair(EnumUiColor.Danger,EnumUiColor.SurfaceRaised,"text",ContrastLook.Both,"error text"));
			listPairs.Add(new ContrastPair(EnumUiColor.Danger,EnumUiColor.DangerSubtle,"text",ContrastLook.Both,"error toolbar state"));
			listPairs.Add(new ContrastPair(EnumUiColor.Warning,EnumUiColor.SurfaceRaised,"text",ContrastLook.Both,"warning text"));
			listPairs.Add(new ContrastPair(EnumUiColor.Warning,EnumUiColor.WarningSubtle,"text",ContrastLook.Both,"notify toolbar state"));
			//Outlines and icons, 3:1.
			listPairs.Add(new ContrastPair(EnumUiColor.StrokeStrong,EnumUiColor.SurfaceRaised,"graphic",ContrastLook.Both,"input, combo, list and check box outline"));
			listPairs.Add(new ContrastPair(EnumUiColor.StrokeStrong,EnumUiColor.Surface,"graphic",ContrastLook.Both,"the same outlines on a window"));
			listPairs.Add(new ContrastPair(EnumUiColor.StrokeStrongHover,EnumUiColor.SurfaceRaised,"graphic",ContrastLook.Both,"outline on hover"));
			listPairs.Add(new ContrastPair(EnumUiColor.Success,EnumUiColor.SurfaceRaised,"graphic",ContrastLook.Both,"complete icon"));
			listPairs.Add(new ContrastPair(EnumUiColor.Accent,EnumUiColor.SurfaceSubtle,"graphic",ContrastLook.Both,"selected tab line, module bar mark and icon"));
			listPairs.Add(new ContrastPair(EnumUiColor.Accent,EnumUiColor.SurfaceRaised,"graphic",ContrastLook.Both,"checked box fill, calendar today ring"));
			listPairs.Add(new ContrastPair(EnumUiColor.Accent,EnumUiColor.AccentSubtle,"graphic",ContrastLook.LightOnly,"toggled toolbar button border"));
			//Focus, 3:1.
			listPairs.Add(new ContrastPair(EnumUiColor.Accent,EnumUiColor.Surface,"focus",ContrastLook.Both,"focus ring on a window"));
			listPairs.Add(new ContrastPair(EnumUiColor.Accent,EnumUiColor.SurfaceRaised,"focus",ContrastLook.Both,"focus ring, combo focus line, focused row"));
			listPairs.Add(new ContrastPair(EnumUiColor.Accent,EnumUiColor.SubtleHover,"focus",ContrastLook.Both,"focus ring on a hovered toolbar button"));
			listPairs.Add(new ContrastPair(EnumUiColor.TextOnAccent,EnumUiColor.Accent,"focus",ContrastLook.Both,"white inner ring on an accent fill"));
			//Selection, 3:1 (section 3.2: the bar, since the fill alone is 1.26:1).
			listPairs.Add(new ContrastPair(EnumUiColor.Accent,EnumUiColor.SurfaceRaised,"selection",ContrastLook.Both,"selection bar with focus"));
			listPairs.Add(new ContrastPair(EnumUiColor.StrokeStrong,EnumUiColor.SurfaceRaised,"selection",ContrastLook.Both,"selection bar without focus"));
			listPairs.Add(new ContrastPair(EnumUiColor.Accent,EnumUiColor.AccentSubtle,"selection",ContrastLook.LightOnly,"selection bar with focus, on the fill"));
			listPairs.Add(new ContrastPair(EnumUiColor.AccentSubtle,EnumUiColor.SurfaceRaised,"selection",ContrastLook.HighContrastOnly,
				"selection fill (high contrast: the highlight)"));
			//Shown for the owner, not required.
			listPairs.Add(new ContrastPair(EnumUiColor.StrokeStrong,EnumUiColor.AccentSubtle,"selection",ContrastLook.Info,"selection bar without focus, on the fill"));
			listPairs.Add(new ContrastPair(EnumUiColor.AccentSubtle,EnumUiColor.SurfaceRaised,"selection",ContrastLook.Info,"selection fill alone (light)"));
			listPairs.Add(new ContrastPair(EnumUiColor.RowHover,EnumUiColor.AccentSubtle,"selection",ContrastLook.Info,"hover against selection"));
			listPairs.Add(new ContrastPair(EnumUiColor.TextDisabled,EnumUiColor.SurfaceRaised,"text",ContrastLook.Info,"disabled text (exempt)"));
			return listPairs;
		}

		///<summary>The ratios section 3.2 states, by token. White is SurfaceRaised's value.</summary>
		public static List<PlanRatio> GetPlanRatios() {
			List<PlanRatio> list=new List<PlanRatio>();
			Action<EnumUiColor,EnumUiColor,double,bool> add=(fore,back,stated,isAtLeast) => list.Add(new PlanRatio { Fore=fore,Back=back,Stated=stated,
				IsAtLeast=isAtLeast,Decimals=stated<2 ? 2 : 1 });
			add(EnumUiColor.Text,EnumUiColor.Surface,15.8,false);
			add(EnumUiColor.Text,EnumUiColor.SurfaceRaised,16.5,false);
			add(EnumUiColor.Text,EnumUiColor.SurfaceSubtle,14.9,false);
			add(EnumUiColor.TextSecondary,EnumUiColor.SurfaceSubtle,6.0,false);
			add(EnumUiColor.Text,EnumUiColor.SubtleHover,12.5,true);
			add(EnumUiColor.Text,EnumUiColor.SubtlePressed,12.5,true);
			add(EnumUiColor.Text,EnumUiColor.ButtonHover,13.7,true);
			add(EnumUiColor.Text,EnumUiColor.ButtonPressed,13.7,true);
			add(EnumUiColor.StrokeStrong,EnumUiColor.SurfaceRaised,3.5,false);
			add(EnumUiColor.StrokeStrong,EnumUiColor.Surface,3.3,false);
			add(EnumUiColor.StrokeStrongHover,EnumUiColor.SurfaceRaised,6.2,false);
			add(EnumUiColor.TextSecondary,EnumUiColor.SurfaceRaised,6.7,false);
			add(EnumUiColor.TextDisabled,EnumUiColor.SurfaceRaised,2.6,false);
			add(EnumUiColor.TextOnAccent,EnumUiColor.Accent,6.8,false);
			add(EnumUiColor.Accent,EnumUiColor.SurfaceSubtle,6.1,false);
			add(EnumUiColor.TextOnAccent,EnumUiColor.AccentHover,8.5,false);
			add(EnumUiColor.TextOnAccent,EnumUiColor.AccentPressed,10.3,false);
			add(EnumUiColor.Text,EnumUiColor.AccentSubtle,13.1,false);
			add(EnumUiColor.Accent,EnumUiColor.AccentSubtle,5.4,false);
			add(EnumUiColor.Text,EnumUiColor.RowHover,14.9,false);
			add(EnumUiColor.TextOnAccent,EnumUiColor.CaptionHover,5.0,false);
			add(EnumUiColor.TextOnAccent,EnumUiColor.Danger,5.7,false);
			add(EnumUiColor.Text,EnumUiColor.DangerSubtle,14.0,false);
			add(EnumUiColor.Danger,EnumUiColor.DangerSubtle,4.8,false);
			add(EnumUiColor.Warning,EnumUiColor.SurfaceRaised,5.3,false);
			add(EnumUiColor.Warning,EnumUiColor.WarningSubtle,4.8,false);
			add(EnumUiColor.Success,EnumUiColor.SurfaceRaised,5.4,false);
			add(EnumUiColor.Accent,EnumUiColor.Surface,6.5,false);
			add(EnumUiColor.AccentSubtle,EnumUiColor.SurfaceRaised,1.26,false);
			add(EnumUiColor.RowHover,EnumUiColor.AccentSubtle,1.14,false);
			//Not tokens: today's close button hover (#E81123, 4.6:1) and the darker selection fill section 3.2 turned down (#C5D5EE, 1.49:1).
			list.Add(new PlanRatio { ColorFore=White,ColorBack=Color.FromArgb(0xE8,0x11,0x23),Stated=4.6,Decimals=1 });
			list.Add(new PlanRatio { ColorFore=Color.FromArgb(0xC5,0xD5,0xEE),ColorBack=White,Stated=1.49,Decimals=2 });
			return list;
		}

		///<summary>The four Windows 11 contrast themes, with the colours of their theme files (%WINDIR%\Resources\Ease of Access Themes\*.theme,
		///[Control Panel\Colors]; the names Settings shows them by). The mapping is checked in each.</summary>
		public static List<HighContrastPalette> GetPalettes() {
			List<HighContrastPalette> list=new List<HighContrastPalette>();
			list.Add(MakePalette("hc1.theme","Dusk",window:"45 50 54",windowText:"255 255 255",windowFrame:"255 255 255",highlight:"161 191 222",
				highlightText:"33 45 59",hotTrack:"112 235 222",grayText:"166 166 166",activeTitle:"218 253 255",titleText:"0 0 0",buttonFace:"45 50 54",
				buttonText:"182 246 240"));
			list.Add(MakePalette("hc2.theme","Night sky",window:"0 0 0",windowText:"255 255 255",windowFrame:"255 255 255",highlight:"214 180 253",
				highlightText:"43 43 43",hotTrack:"128 128 255",grayText:"166 166 166",activeTitle:"134 140 255",titleText:"0 0 0",buttonFace:"0 0 0",
				buttonText:"255 238 50"));
			list.Add(MakePalette("hcblack.theme","Aquatic",window:"32 32 32",windowText:"255 255 255",windowFrame:"255 255 255",highlight:"142 227 240",
				highlightText:"38 59 80",hotTrack:"117 233 252",grayText:"166 166 166",activeTitle:"245 197 42",titleText:"0 0 0",buttonFace:"32 32 32",
				buttonText:"255 255 255"));
			list.Add(MakePalette("hcwhite.theme","Desert",window:"255 250 239",windowText:"61 61 61",windowFrame:"0 0 0",highlight:"144 57 9",
				highlightText:"255 245 227",hotTrack:"28 94 117",grayText:"103 103 103",activeTitle:"54 88 121",titleText:"255 255 255",buttonFace:"255 250 239",
				buttonText:"32 32 32"));
			return list;
		}

		///<summary>The theme file's key for each system colour the mapping uses.</summary>
		public static readonly Dictionary<KnownColor,string> DictThemeKeys=new Dictionary<KnownColor,string> {
			{ KnownColor.Window,"Window" },{ KnownColor.WindowText,"WindowText" },{ KnownColor.WindowFrame,"WindowFrame" },{ KnownColor.Highlight,"Hilight" },
			{ KnownColor.HighlightText,"HilightText" },{ KnownColor.HotTrack,"HotTrackingColor" },{ KnownColor.GrayText,"GrayText" },
			{ KnownColor.ActiveCaption,"ActiveTitle" },{ KnownColor.ActiveCaptionText,"TitleText" },{ KnownColor.Control,"ButtonFace" },
			{ KnownColor.ControlText,"ButtonText" } };

		private static HighContrastPalette MakePalette(string file,string name,string window,string windowText,string windowFrame,string highlight,
			string highlightText,string hotTrack,string grayText,string activeTitle,string titleText,string buttonFace,string buttonText)
		{
			HighContrastPalette palette=new HighContrastPalette { File=file,Name=name };
			palette.DictColors[KnownColor.Window]=ParseRgb(window);
			palette.DictColors[KnownColor.WindowText]=ParseRgb(windowText);
			palette.DictColors[KnownColor.WindowFrame]=ParseRgb(windowFrame);
			palette.DictColors[KnownColor.Highlight]=ParseRgb(highlight);
			palette.DictColors[KnownColor.HighlightText]=ParseRgb(highlightText);
			palette.DictColors[KnownColor.HotTrack]=ParseRgb(hotTrack);
			palette.DictColors[KnownColor.GrayText]=ParseRgb(grayText);
			palette.DictColors[KnownColor.ActiveCaption]=ParseRgb(activeTitle);
			palette.DictColors[KnownColor.ActiveCaptionText]=ParseRgb(titleText);
			palette.DictColors[KnownColor.Control]=ParseRgb(buttonFace);
			palette.DictColors[KnownColor.ControlText]=ParseRgb(buttonText);
			return palette;
		}

		///<summary>"R G B" as a theme file writes it (anything after the third number is ignored, as Windows does).</summary>
		public static Color ParseRgb(string text) {
			System.Text.RegularExpressions.Match match=System.Text.RegularExpressions.Regex.Match(text,@"^\s*(\d+)\s+(\d+)\s+(\d+)");
			if(!match.Success) {
				throw new FormatException("Not R G B: "+text);
			}
			return Color.FromArgb(int.Parse(match.Groups[1].Value),int.Parse(match.Groups[2].Value),int.Parse(match.Groups[3].Value));
		}

		///<summary>The ratio a pair has in the light look.</summary>
		public static double GetRatioLight(ContrastPair pair) {
			return GetRatio(UiTheme.GetColorLight(pair.Fore),UiTheme.GetColorLight(pair.Back));
		}
	}
}
