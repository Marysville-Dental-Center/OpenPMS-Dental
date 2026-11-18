// The colours of printed and sheet grids (UI refresh, package UI-2a-3; docs/specs/ui-refresh-plan.md, section 5.2 and the owner's answer to
// UQ-5: printouts keep today's look). Printouts, sheets and their PDFs are documents, so GridOD draws them with these, today's values, and not
// with its screen colours, which the grid restyle (UI-5) moves to the UiTheme tokens. The print fonts are GridOD's own print font fields, pinned
// the same way. Not a theme: these values do not change with the look. They are GridOD's screen colours of today, kept here on their own so
// the shared controls' colour scan (NoHqTests UiThemeScanTests, which reads GridOD.cs) keeps shrinking as UI-5 restyles the screen.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Drawing;

namespace OpenPmsDental.UI {
	///<summary>Today's colours of a printed or sheet grid (UQ-5). Shared and static: never dispose them.</summary>
	public static class GridPrintLook {
		///<summary>The sheet title's gradient, top (GridOD._colorTitleTop today).</summary>
		public static readonly Color ColorTitleTop=Color.FromArgb(156,175,230);
		///<summary>The sheet title's gradient, bottom (GridOD._colorTitleBottom today).</summary>
		public static readonly Color ColorTitleBottom=Color.FromArgb(60,90,150);
		///<summary>The sheet title's text.</summary>
		public static readonly SolidBrush BrushTitleText=(SolidBrush)Brushes.White;
		///<summary>Column separators and the lines below headers and bottom rows (GridOD._penColumnSeparator today).</summary>
		public static readonly Pen PenColumnSeparator=new Pen(Color.FromArgb(120,120,120));
		///<summary>The lines between rows and cells (GridOD._penGridline today).</summary>
		public static readonly Pen PenGridline=new Pen(Color.FromArgb(220,220,220));
		///<summary>The sheet grid's outline (GridOD._penOutline today).</summary>
		public static readonly Pen PenOutline=new Pen(Color.FromArgb(77,100,147));
	}
}
