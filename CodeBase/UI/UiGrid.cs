// The layout grid of the UI refresh (package UI-12, docs/port-notes/UI-12.md sections 1 and 1.1): the numbers the grid tools lay the windows out
// with, for product code that places controls itself (a window that sizes or moves controls at run time follows the same grid). All values are
// in px at 96 DPI for Segoe UI 9 pt; scale them with the window's DPI (LayoutManager.Scale). The one source is
// tests/UiRefreshTests/Data/grid-numbers.tsv, which the planner (tools/UiFit/grid_tools.py) reads; GridNumbersTests fails when a constant here
// differs from its row there (the row's name in upper case). C# 7.3, System only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.

namespace CodeBase {
	///<summary>The layout grid's numbers (UI-12), in px at 96 DPI.</summary>
	public static class UiGrid {
		///<summary>Container padding on every side (form client area, panel, tab page, group box).</summary>
		public const int PAD=8;
		///<summary>A group box's content starts this far below its title band.</summary>
		public const int TITLE_GAP=4;
		///<summary>A label sits this far before its field.</summary>
		public const int LABEL_GAP=6;
		///<summary>Between field rows.</summary>
		public const int ROW_GAP=4;
		///<summary>The row pitch: FIELD_HEIGHT + ROW_GAP.</summary>
		public const int PITCH=27;
		///<summary>Between consecutive check boxes or radio buttons.</summary>
		public const int LIST_GAP=4;
		///<summary>A check box or radio button list's pitch: CHECK_HEIGHT + LIST_GAP.</summary>
		public const int LIST_PITCH=22;
		///<summary>A caption label above the control it names.</summary>
		public const int CAPTION_GAP=2;
		///<summary>Rows that were clearly apart (SECTION_FROM or more) stay apart by this.</summary>
		public const int SECTION_GAP=12;
		///<summary>The gap from which two rows count as clearly apart.</summary>
		public const int SECTION_FROM=12;
		///<summary>Between containers side by side or stacked (the gutter).</summary>
		public const int SIBLING_GAP=8;
		///<summary>Between items of one row (a field and its button or its unit label).</summary>
		public const int ITEM_GAP=6;
		///<summary>Between two label + field bands of one row (the gutter between field columns).</summary>
		public const int BAND_GAP=16;
		///<summary>Right edges of one column within this share one edge.</summary>
		public const int EDGE_SNAP=16;
		///<summary>A wide field this much short of its column's right edge grows to it.</summary>
		public const int EDGE_GROW=32;
		///<summary>Left edges within this are one column.</summary>
		public const int COLUMN_SNAP=16;
		///<summary>Fields of label + field rows within this are one column.</summary>
		public const int FIELD_COLUMN_SNAP=40;
		///<summary>A container within this of its parent's content edges spans the content.</summary>
		public const int SPAN_SNAP=32;
		///<summary>Widths snap to this.</summary>
		public const int SNAP=4;
		///<summary>A single-line field (text box, combo box, date picker, number field) at the program's font.</summary>
		public const int FIELD_HEIGHT=23;
		///<summary>A check box or radio button row (UI.CheckBox's own height).</summary>
		public const int CHECK_HEIGHT=18;
		///<summary>UI.CheckBox's box (12 + the 1 px line).</summary>
		public const int CHECK_BOX=13;
		///<summary>Between a check box's box and its text.</summary>
		public const int CHECK_TEXT=3;
		///<summary>Between dialog buttons (OK, Cancel), which sit PAD from the bottom right.</summary>
		public const int DIALOG_GAP=8;
		///<summary>A vertical scroll bar at 96 DPI.</summary>
		public const int SCROLL_BAR=17;
		///<summary>Preferences style: a label starts where the check box text beside it starts.</summary>
		public const int LABEL_LEAD=16;
		///<summary>Preferences style: the field column stays within this share of the content width; a longer label wraps.</summary>
		public const double LABEL_CAP=0.5;
		///<summary>Preferences style: a text field this wide or wider ends at the content's right edge.</summary>
		public const int WIDE_FIELD=120;
		///<summary>Field columns of different containers within this (window coordinates) share one left edge.</summary>
		public const int HINT_SNAP=40;
		///<summary>A check box whose one line needs at most this much more than its row has stays one line.</summary>
		public const int CHECK_SLACK=3;
	}
}
