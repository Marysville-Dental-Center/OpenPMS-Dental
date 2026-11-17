// The WPF brushes and colours of the colour tokens (docs/specs/ui-refresh-plan.md, section 3.2; package UI-1, docs/port-notes/UI-1.md). Frozen
// brushes are safe on every thread. While Windows runs a high-contrast theme (SystemParameters.HighContrast) each token gives its SystemColors
// partner, the same one as in WinForms (UiTheme.Colors.cs). This file is the WPF part of UiTheme; RdlDesign does not link it.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Windows;
using System.Windows.Media;

namespace CodeBase {
	public static partial class UiTheme {
		///<summary>The frozen light brushes, indexed by EnumUiColor; made on first use.</summary>
		private static SolidColorBrush[] _arrayBrushesWpfLight;

		///<summary>The token's WPF brush now: frozen, shared, never modified by a caller. Its light value, or its SystemColors brush while Windows runs a
		///high-contrast theme.</summary>
		public static SolidColorBrush GetBrushWpf(EnumUiColor uiColor) {
			if(SystemParameters.HighContrast) {
				return GetSystemBrushWpf(GetKnownColorHighContrast(uiColor));
			}
			return GetBrushWpfLight(uiColor);
		}

		///<summary>The token's light WPF brush (frozen), whatever Windows runs.</summary>
		public static SolidColorBrush GetBrushWpfLight(EnumUiColor uiColor) {
			SolidColorBrush[] arrayBrushes=_arrayBrushesWpfLight;
			if(arrayBrushes==null) {
				arrayBrushes=new SolidColorBrush[CountColors];
				for(int i=0;i<arrayBrushes.Length;i++) {
					SolidColorBrush solidColorBrush=new SolidColorBrush(GetColorWpfLight((EnumUiColor)i));
					solidColorBrush.Freeze();
					arrayBrushes[i]=solidColorBrush;
				}
				//Two threads that both build the array build the same frozen brushes; either array will do.
				_arrayBrushesWpfLight=arrayBrushes;
			}
			return arrayBrushes[(int)uiColor];
		}

		///<summary>The token's WPF colour now (light, or its SystemColors partner in high contrast).</summary>
		public static System.Windows.Media.Color GetColorWpf(EnumUiColor uiColor) {
			return GetBrushWpf(uiColor).Color;
		}

		///<summary>The token's light value as a WPF colour.</summary>
		public static System.Windows.Media.Color GetColorWpfLight(EnumUiColor uiColor) {
			System.Drawing.Color color=GetColorLight(uiColor);
			return System.Windows.Media.Color.FromRgb(color.R,color.G,color.B);
		}

		///<summary>WPF's brush of a system colour that the high-contrast mapping uses (UiTheme.Colors.cs).</summary>
		public static SolidColorBrush GetSystemBrushWpf(System.Drawing.KnownColor knownColor) {
			switch(knownColor) {
				case System.Drawing.KnownColor.Window:
					return SystemColors.WindowBrush;
				case System.Drawing.KnownColor.WindowText:
					return SystemColors.WindowTextBrush;
				case System.Drawing.KnownColor.WindowFrame:
					return SystemColors.WindowFrameBrush;
				case System.Drawing.KnownColor.Highlight:
					return SystemColors.HighlightBrush;
				case System.Drawing.KnownColor.HighlightText:
					return SystemColors.HighlightTextBrush;
				case System.Drawing.KnownColor.HotTrack:
					return SystemColors.HotTrackBrush;
				case System.Drawing.KnownColor.GrayText:
					return SystemColors.GrayTextBrush;
				case System.Drawing.KnownColor.ActiveCaption:
					return SystemColors.ActiveCaptionBrush;
				case System.Drawing.KnownColor.ActiveCaptionText:
					return SystemColors.ActiveCaptionTextBrush;
				case System.Drawing.KnownColor.Control:
					return SystemColors.ControlBrush;
				case System.Drawing.KnownColor.ControlText:
					return SystemColors.ControlTextBrush;
				default:
					throw new ArgumentException("No WPF system brush for "+knownColor+".",nameof(knownColor));
			}
		}
	}
}
