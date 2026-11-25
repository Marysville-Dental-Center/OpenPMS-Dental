// The UI refresh's tokens (docs/specs/ui-refresh-plan.md, section 3; package UI-1, docs/port-notes/UI-1.md): every colour, size, corner, line,
// spacing and icon size of the new look, defined once. Nothing in the program draws with them yet: each control package (UI-3 to UI-6)
// switches its own controls to them, so UI-1 changes nothing on screen. ColorOD keeps today's values until UI-11.
//
// The class is split into partial files:
// - UiTheme.cs (this file): the high-contrast switch and InitWinForms.
// - UiTheme.Renderer.cs: the token renderer of the stock menus and tool strips (UiToolStripRenderer, UI-6).
// - UiTheme.Colors.cs: the colour tokens as System.Drawing.Color, and their high-contrast partners (SystemColors).
// - UiTheme.Gdi.cs: the cached GDI+ pens and brushes, one cache per thread.
// - UiTheme.Metrics.cs: font family and sizes, corners, lines, spacing, icon sizes and the pixel-snapping helpers of section 3.3.
// - UiTheme.Fonts.cs: the font role tokens of section 3.4 (FontBody and the rest, today's fonts until UI-2b), the mapping of designer
//   fonts (section 5.1) and the test-only override SetFontForTest (UI-2a-1).
// - UiTheme.Wpf.cs: the frozen WPF brushes and colours.
// The others but the WPF part use only Windows Forms, System.Drawing and Microsoft.Win32 and nothing else of CodeBase but the icon renderer
// (UiIcons, for the renderer's glyphs), in C# 7.3, so RdlDesign (which references no CodeBase) can compile them in as linked files (section 3.6,
// Menus; RdlDesign/ReportDesigner.csproj); a NoHq test checks it. The WPF part is in its own file.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CodeBase {
	///<summary>The tokens of the new look (docs/specs/ui-refresh-plan.md, section 3). Colours come back as values (thread-safe); GDI+ pens and brushes
	///are cached per thread and must never be disposed by a caller (GetPen, GetBrush); WPF brushes are frozen. While Windows runs a high-contrast
	///theme every colour token returns its SystemColors partner.</summary>
	public static partial class UiTheme {
		///<summary>Guards the subscription to SystemEvents and the cached high-contrast value.</summary>
		private static readonly object _lock=new object();
		///<summary>0 until first read; then 1 (no) or 2 (yes), the cached SystemInformation.HighContrast, read again on UserPreferenceChanged. An
		///int, so a paint on another thread reads it whole.</summary>
		private static volatile int _highContrastState;
		private static bool _isSubscribed;

		///<summary>True while Windows runs a high-contrast theme (SystemInformation.HighContrast). Read once and again whenever Windows reports a
		///changed user preference (SystemEvents.UserPreferenceChanged), so painting does not ask Windows each time.</summary>
		public static bool IsHighContrast {
			get {
				int highContrastState=_highContrastState;
				if(highContrastState!=0) {
					return highContrastState==2;
				}
				lock(_lock) {
					if(!_isSubscribed) {
						_isSubscribed=true;
						try {
							SystemEvents.UserPreferenceChanged+=SystemEvents_UserPreferenceChanged;
						}
						catch(Exception) {
							//No SystemEvents in this process (a service without a desktop): the value is read once and kept.
						}
					}
					bool isHighContrast=SystemInformation.HighContrast;
					_highContrastState=isHighContrast ? 2 : 1;
					return isHighContrast;
				}
			}
		}

		private static void SystemEvents_UserPreferenceChanged(object sender,UserPreferenceChangedEventArgs e) {
			lock(_lock) {
				_highContrastState=SystemInformation.HighContrast ? 2 : 1;
			}
		}

		///<summary>The one start-up call of every exe's Main, right after SetDefaultFont and SetHighDpiMode (section 3.6, Menus), and on every
		///other UI thread the program starts (UI-6): sets ToolStripManager.Renderer, which Windows Forms keeps per thread, to the token renderer
		///(UiToolStripRenderer), so every stock ContextMenuStrip, MenuStrip, ToolStrip and StatusStrip of this thread draws in the new look. Creates
		///no window, so it may run before EnableVisualStyles and SetCompatibleTextRenderingDefault. Calling it again on the same thread does nothing.
		///</summary>
		public static void InitWinForms() {
			if(!_isRendererWatched) {
				//Windows Forms can put its own renderer back on a system change (a visual style or display change, e.g. a remote desktop
				//connecting), which turned the stock menus back to the old look (window check C20). Set the token renderer again whenever that happens.
				_isRendererWatched=true;
				//Windows Forms can drop its renderer without raising RendererChanged (the getter then makes a new professional renderer), so the
				//thread's own idle event checks it too: a type check, on this thread, whose renderer it is.
				ToolStripManager.RendererChanged+=(sender,e) => ReapplyRenderer();
				Application.Idle+=(sender,e) => ReapplyRenderer();
			}
			ReapplyRenderer();
		}

		///<summary>Per thread, as ToolStripManager.Renderer is.</summary>
		[ThreadStatic]
		private static bool _isRendererWatched;

		private static void ReapplyRenderer() {
			if(ToolStripManager.Renderer is UiToolStripRenderer) {
				return;
			}
			ToolStripManager.Renderer=new UiToolStripRenderer();
		}
	}
}
