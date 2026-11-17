// The GDI+ pens and brushes of the colour tokens (docs/specs/ui-refresh-plan.md, section 3.2, "Thread safety"; package UI-1). GDI+ objects are
// not thread-safe: two UI threads drawing with one Pen at once throw "Object is currently in use elsewhere". So each thread has its own cache
// ([ThreadStatic]), filled on first use. Callers never dispose what they get: Pen and SolidBrush are sealed, so a cached one cannot ignore
// Dispose, and a NoHq source scan bans using and Dispose around them instead (UiThemeScanTests). System.Drawing only, C# 7.3: RdlDesign can
// compile this file in as a linked file (section 3.6).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CodeBase {
	public static partial class UiTheme {
		///<summary>This thread's pens, by token and width (GetKeyPen). Created on first use on each thread.</summary>
		[ThreadStatic]
		private static Dictionary<int,CachedPen> _dictPens;
		///<summary>This thread's brushes, by token. Created on first use on each thread.</summary>
		[ThreadStatic]
		private static Dictionary<int,CachedBrush> _dictBrushes;

		///<summary>The widest cached pen, in device pixels. A line of 2 px at 200% is 4 px; a wider one is not a token line.</summary>
		public const int PEN_WIDTH_MAX=16;

		///<summary>This thread's 1 px pen of the token's current colour. Never dispose it, never wrap it in using, never change its properties: it is
		///shared by every paint on this thread. For a dash, a cap or a transform, make a pen of your own from GetColor.</summary>
		public static Pen GetPen(EnumUiColor uiColor) {
			return GetPen(uiColor,1);
		}

		///<summary>This thread's pen of the token's current colour, widthPixels device pixels wide (1 to PEN_WIDTH_MAX; use GetLineWidth and
		///GetFocusWidth for the scaled widths). Never dispose it, never wrap it in using, never change its properties.</summary>
		public static Pen GetPen(EnumUiColor uiColor,int widthPixels) {
			if(widthPixels<1 || widthPixels>PEN_WIDTH_MAX) {
				throw new ArgumentOutOfRangeException(nameof(widthPixels),widthPixels,"A token pen is 1 to "+PEN_WIDTH_MAX+" px wide.");
			}
			if(_dictPens==null) {
				_dictPens=new Dictionary<int,CachedPen>();
			}
			Color color=GetColor(uiColor);
			int key=(int)uiColor*(PEN_WIDTH_MAX+1)+widthPixels;
			CachedPen cachedPen;
			if(_dictPens.TryGetValue(key,out cachedPen) && cachedPen.Argb==color.ToArgb()) {
				return cachedPen.Pen;
			}
			//First use on this thread, or the colour changed (Windows switched to or from high contrast). The old pen is not disposed: a paint on
			//this thread may still hold it in a local between two calls. The garbage collector frees it; a theme change is rare.
			cachedPen=new CachedPen(new Pen(color,widthPixels),color);
			_dictPens[key]=cachedPen;
			return cachedPen.Pen;
		}

		///<summary>This thread's solid brush of the token's current colour. Never dispose it, never wrap it in using, never change its colour.</summary>
		public static SolidBrush GetBrush(EnumUiColor uiColor) {
			if(_dictBrushes==null) {
				_dictBrushes=new Dictionary<int,CachedBrush>();
			}
			Color color=GetColor(uiColor);
			int key=(int)uiColor;
			CachedBrush cachedBrush;
			if(_dictBrushes.TryGetValue(key,out cachedBrush) && cachedBrush.Argb==color.ToArgb()) {
				return cachedBrush.Brush;
			}
			//As for pens: the old brush is left to the garbage collector.
			cachedBrush=new CachedBrush(new SolidBrush(color),color);
			_dictBrushes[key]=cachedBrush;
			return cachedBrush.Brush;
		}

		///<summary>A pen and the ARGB value it was made with, so a lookup compares values without asking GDI+. The value, not the Color: a
		///SystemColors colour compares equal to itself after Windows changed it (another high-contrast theme).</summary>
		private class CachedPen {
			public readonly Pen Pen;
			public readonly int Argb;

			public CachedPen(Pen pen,Color color) {
				Pen=pen;
				Argb=color.ToArgb();
			}
		}

		///<summary>A brush and the ARGB value it was made with.</summary>
		private class CachedBrush {
			public readonly SolidBrush Brush;
			public readonly int Argb;

			public CachedBrush(SolidBrush brush,Color color) {
				Brush=brush;
				Argb=color.ToArgb();
			}
		}
	}
}
