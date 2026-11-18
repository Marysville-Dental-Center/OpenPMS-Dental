using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDentalBusiness;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace OpenPmsDental.UI {
	/*Jordan is the only one allowed to edit this class.
	
This is a template of typical grid code which can be quickly pasted into any form.
 
using OpenPmsDental.UI;

		private void FillGrid(){
			gridMain.BeginUpdate();
			gridMain.Columns.Clear();
			GridColumn gridColumn=new GridColumn(Lan.g("Table...","colname"),100);
			gridMain.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lan.g("Table...","colname"),100,HorizontalAlignment.Center);
			gridMain.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lan.g("Table...","colname"),100);
			gridMain.Columns.Add(gridColumn);
			gridMain.ListGridRows.Clear();
			for(int i=0;i<_list.Count;i++){
				GridRow gridRow=new GridRow();
				gridRow.Cells.Add(_list[i].);
				gridRow.Cells.Add("");
				gridRow.Cells.Add(new GridCell(""){ColorBackG=.., etc});//this is not allowed
			  GridCell gridCell=new GridCell("");//instead of above line
				gridCell.ColorBackG=...;
				gridRow.Cells.Add(cell);
				gridRow.Cells.Add(new GridCell(_list[i].IsHidden?"X":""));
				gridRow.Tag=;
				gridMain.ListGridRows.Add(gridRow);
			}
			gridMain.EndUpdate();
		}
*/
	///<summary>A custom grid/table control used extensively in OD. </summary>
	[DefaultEvent("CellDoubleClick")]
	public class GridOD:System.Windows.Forms.UserControl {
		#region Fields - Public
		///<summary>A func that defines how to create a gridrow row from a datarow. This allows JIT conversion of each row.</summary>
		public Func<DataRow,GridRow> FuncConstructGridRow;
		///<summary>This grid was created solely to draw to a sheet.</summary>
		public bool IsForSheets;
		///<summary>This grid was created solely to draw a document through the sheet methods, without IsForSheets' layout (the sheet design canvas
		///of FormSheetDefEdit). Like IsForSheets, its row heights come from the print fonts (UI-2a-3).</summary>
		public bool IsForDocument;
		///<summary>When drawing to sheets, additional info about rows.</summary>
		public List<GridSheetRow> ListGridSheetRows;
		///<summary>Used when calculating row positions.  Set to 0 when using in FormSheetFillEdit.</summary>
		public int SheetBottomMargin;
		public int SheetPageHeight;
		///<summary>Height of drawn grid on this page.  Set using CalculateHeights() from EndUpdate()</summary>
		public int SheetPrintHeight;
		///<summary>Used when calculating row positions.  Set to 0 when using in FormSheetFillEdit.</summary>
		public int SheetTopMargin;
		///<summary>The position on the page that this grid will print. If this is halfway down the second page, 1100px tall, this value should be 1650, not 550.</summary>
		public int SheetYPos;
		#endregion Fields - Public

		#region Fields - Private Static Drawing
		//The screen colours are the UiTheme tokens (UI refresh, package UI-5; docs/specs/ui-refresh-plan.md sections 3.2 and 3.6, docs/port-notes/UI-5.md):
		//title Accent with white text, header SurfaceSubtle with Stroke separators, rows SurfaceRaised with Gridline lines, hover RowHover,
		//selection AccentSubtle with the 2 px selection bar, the focused row's Accent outline, outer border Stroke (GridScreenLook, shared with
		//GridOld). The pens and brushes come from UiTheme's per-thread caches and are never disposed. Printouts and sheets draw with GridPrintLook
		//(today's colours, UQ-5).
		///<summary>Today's default selection colour (the ColorSelectedRow default). A grid that keeps it draws its selection in AccentSubtle; a colour a
		///window sets (DarkSalmon in the Family and Account modules, a transparent one in the report setup) stays, like the other row colours.</summary>
		private static readonly Color _colorSelectedRowStock=Color.FromArgb(205,220,235);
		///<summary>The title's height on a printout or a sheet at 96 DPI, today's (UQ-5): the screen's follows the font (GridScreenLook.GetHeightTitle96).</summary>
		private const int HEIGHT_TITLE_PRINT=18;
		#endregion Fields - Private Static Drawing

		#region Fields - Private Drawing
		//Fonts--------------------------------------------------------------------------------------------------
		//The screen fonts take the font tokens (UI refresh, UI-2a-3: FontGrid in its four styles, FontTitle), which hold today's sans serif 8.5
		//and 10 Bold until UI-2b. Printouts and sheets never use them: they draw with the print fonts below.
		private Font _fontCell;//=new Font(FontFamily.GenericSansSerif,8.5f);//this.Font is ignored
		private Font _fontCellBold;//=new Font(FontFamily.GenericSansSerif,8.5f,FontStyle.Bold);
		private Font _fontUnderline;//=new Font(FontFamily.GenericSansSerif,8.5f,FontStyle.Underline);
		private Font _fontUnderlineBold;//=new Font(FontFamily.GenericSansSerif,8.5f,FontStyle.Underline | FontStyle.Bold);
		private Font _fontHeader;//=new Font(FontFamily.GenericSansSerif,8.5f,FontStyle.Bold);
		private Font _fontTitle;//=new Font(FontFamily.GenericSansSerif,10,FontStyle.Bold);
		//Print fonts (UI-2a-3, plan section 5.2): PrintPage, the sheet methods (SheetDrawRow, SheetDrawTitle, SheetDrawHeader and their PDF
		//variants) and the row heights of a printout or a sheet use these, pinned to today's fonts, so a printout, a sheet, its PDF and the sheet
		//design canvas keep their look whatever the screen tokens hold. Made with the screen fonts in SetFonts, at the same zoom.
		private Font _fontPrintCell;//=new Font(FontFamily.GenericSansSerif,8.5f);
		private Font _fontPrintCellBold;//=new Font(FontFamily.GenericSansSerif,8.5f,FontStyle.Bold);
		private Font _fontPrintHeader;//=new Font(FontFamily.GenericSansSerif,8.5f,FontStyle.Bold);
		private Font _fontPrintTitle;//=new Font(FontFamily.GenericSansSerif,10,FontStyle.Bold);
		#endregion Fields - Private Drawing

		#region Fields - Private for Properties
		private bool _addButtonEnabled=true;
		private Color _colorSelectedRow=_colorSelectedRowStock;
			//186,199,219);//recent listboxOD color
			//Color.Silver;//gray 192
		private bool _hasAddButton=false;
		private bool _hasAlternateRowsColored=false;
		private bool _hasDropDowns=false;
		private bool _headersVisible=true;
		private bool _hScrollVisible=false;
		private Font _fontForSheets;//disposed
		private Font _fontForSheetsBold;//disposed
		///<summary>HasMultiLineHeaders must be turned on for this to work.</summary>
		private bool _hasAutoWrappedHeaders=false;
		private bool _hasMultilineHeaders;
		///<summary>Default 1. Typical value 1.5</summary>
		private float _scaleMS = 1;
		///<summary>Default 1. Typical value 1.2</summary>
		private float _zoomLocal=1;
		private Point _selectedCell=new Point(-1,-1);
		///<summary>This is the only internal storage for tracking selected row indices.  All properties refer to this same list.</summary>
		private List<int> _listSelectedIndices=new List<int>();
		private GridSelectionMode _selectionMode=GridSelectionMode.OneRow;
		///<summary>This is used instead of vScroll.Visible so that we can have more control over it.</summary>
		private bool _isVScrollVisible=true;
		private bool _sortingAllowByColumn=false;
		private string _title="";
		private bool _titleVisible=true;
		private bool _vScrollVisible=true;
		private bool _wrapText=true;
		#endregion Fields - Private for Properties

		#region Fields - Private
		private ComboBoxGrid comboBox=new ComboBoxGrid();
		///<summary>At 96dpi. 19 instead of 14.  Used for textboxes, comboboxes, and buttons, so that they have room.</summary>
		private const int EDITABLE_ROW_HEIGHT=19;
		///<summary>The GridColumn that was clicked on during the MouseDown event.</summary>
		private GridColumn _gridColumnMouseDown;
		///<summary>The GridRow that was clicked on during the MouseDown event.</summary>
		private GridRow _gridRowMouseDown;
		///<summary>Is set when ComputeRows is run, then used . If any columns are editable, hasEditableColumn is true, and all row heights are slightly taller.</summary>
		private bool _hasEditableColumn;
		///<summary>The screen title's height: its font's line height plus 2 (UI-5), 18 with today's font. If title is not visible, this will be set to 0.
		///96dpi.</summary>
		private int _heightTitle=GridScreenLook.GetHeightTitle96();
		///<summary>_heightTitle for printouts and sheets: today's 18 (UQ-5), or 0 when the title is not visible. 96dpi.</summary>
		private int _heightTitlePrint=HEIGHT_TITLE_PRINT;
		///<summary>Starts out at the header font's line height plus 2 (UI-5; 15 with today's font), but code can push it higher to fit multiline text.
		///If header is not visible, this will be set to 0. 96dpi.</summary>
		private int _heightHeader=GridScreenLook.GetHeightHeader96();
		///<summary>The row the arrow keys or the last click moved to: it gets the 1 px Accent outline while the grid has focus and Windows shows
		///keyboard cues (section 3.6). -1 for none; reset by EndUpdate.</summary>
		private int _indexRowFocused=-1;
		///<summary>_heightHeader for printouts and sheets: the same rule, with the multiline headers measured in the print header font (UI-2a-3), so a
		///taller screen font never changes a printed header. 96dpi.</summary>
		private int _heightHeaderPrint=15;
		private int _heightImage;
		///<summary>The total height of the actual grid area, including the parts hidden by scroll. Current dpi.</summary>
		private int _heightTotal;
		private int _hoverRow=-1;
		private bool _isMouseDown;
		private bool _isMouseDownInHeader;
		///<summary>True to show a triangle pointing up.  False to show a triangle pointing down.  Only works if _sortedByColumnIdx is not -1.</summary>
		private bool _isSortedAscending;
		///<summary>True between BeginUpdate-EndUpdate.</summary>
		private bool _isUpdating;
		///<summary>True while PrintPage lays out and draws a page: the row heights it computes come from the print fonts (UI-2a-3).</summary>
		private bool _isPrinting;
		///<summary>True when the row heights were last computed with the print fonts (a printout, a sheet), false when with the screen fonts.</summary>
		private bool _areRowHeightsForPrint;
		///<summary>This is an alternative to setting up all of the GridRows in advance.  The DataRows get converted to GridRows on the fly.</summary>
		private List<DataRow> _listDataRows;
		///<summary>The links added to the ContextMenuStrip the last time it opened, removed before the next links are added.</summary>
		private List<ToolStripItem> _listToolStripItemsLinks;
		///<summary>The grid's own right-click menu, made on the first right click when no caller set a menu. Disposed with the grid.</summary>
		private ContextMenuStrip _contextMenuStripGrid;
		///<summary>The ContextMenuStrip that ContextMenuStrip_Opening is subscribed to, if any, so that it is unsubscribed when the grid gets another
		///strip (a strip the grid had before must not add the grid's items any more).</summary>
		private ContextMenuStrip _contextMenuStripHooked;
		///<summary>Helps with drag selections. This is a snapshot of the selected indices when the mouse goes down and starts a drag.</summary>
		private List<int> _listSelectedIndicesWhenMouseDown;
		private MouseButtons _mouseButtonLastPressed;
		private Point _mouseClickLocation;
		private int _mouseDownCol;
		private int _mouseDownRow;
		private int _printedRows;
		///<summary>If we are part way through drawing a note when we reach the end of a page, this will contain the remainder of the note still to be printed.  If it is empty string, then we are not in the middle of a note.</summary>
		private string _printNoteRemaining;
		///<summary>Flag used for calculating the width of the last column for printing when the grid is being scaled.</summary>
		private int _printWidth=0;
		private Point _selectedCellOld;
		///<summary>Typically -1 to show no triangle.  Or, specify a column to show a triangle.  The actual sorting happens at mouse down.</summary>
		private int _sortedByColumnIdx = -1;
		///<summary>Tab stop is set at 1/2" in ctor, top aligned, no trimming. Horizontal alignment and wrap are changed as needed.</summary>
		private StringFormat _stringFormat;
		///<summary>TextBoxBase so that it can be a normal TextBox or a RichTextBox.</summary>
		private TextBoxBase textEdit;
		///<summary>Truncates the note to this many characters. UEs can occur at lengths greater than 32,000</summary>
		public const int TEXT_LENGTH_LIMIT=30000;
		///<summary></summary>
		private int _widthNote;
		///<summary>The total width of the grid, including the parts hidden by scroll.</summary>
		private int _widthTotal;
		#endregion Fields - Private

		#region Contructor
		///<summary></summary>
		public GridOD() {
			InitializeComponent();// Required for Windows.Forms Class Composition Designer support
			vScroll=new VScrollBar();
			vScroll.Scroll+=new ScrollEventHandler(vScroll_Scroll);
			hScroll=new HScrollBar();
			hScroll.Scroll+=new ScrollEventHandler(hScroll_Scroll);
			this.Controls.Add(vScroll);
			this.Controls.Add(hScroll);
			float[] arrayTabStops={50.0f};
			_stringFormat=new StringFormat();
			_stringFormat.SetTabStops(0.0f,arrayTabStops);
			_stringFormat.LineAlignment=StringAlignment.Near;//top aligned, but we will consider a center someday
			//_stringFormat.Trimming=StringTrimming.None;//This causes PrintPage(...) to get stuck in an infinite loop
			DoubleBuffered=true;
			Columns=new GridColumnCollection(this);
			SetFonts();
			_heightTotal=0;//instead of	ComputeRows(g);
			LayoutScrolls();
		}
		#endregion Constructor

		#region Designer
		///<summary>Required designer variable.</summary>
		private System.ComponentModel.Container components = null;

		///<summary>Clean up any resources being used.</summary>
		protected override void Dispose(bool disposing) {
			if(disposing) {
				_stringFormat?.Dispose();
				_fontCell?.Dispose();
				_fontCellBold?.Dispose();
				_fontUnderline?.Dispose();
				_fontUnderlineBold?.Dispose();
				_fontHeader?.Dispose();
				_fontTitle?.Dispose();
				_fontPrintCell?.Dispose();
				_fontPrintCellBold?.Dispose();
				_fontPrintHeader?.Dispose();
				_fontPrintTitle?.Dispose();
				_fontForSheets?.Dispose();
				_fontForSheetsBold?.Dispose();
				comboBox?.Dispose();
				_contextMenuStripGrid?.Dispose();
				components?.Dispose();//vScroll and hScroll
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			components = new System.ComponentModel.Container();
		}
		#endregion

		private System.Windows.Forms.VScrollBar vScroll;
		private System.Windows.Forms.HScrollBar hScroll;
		#endregion Designer

		#region Events
		///<summary></summary>
		[Category("OD")]
		[Description("Occurs when a cell is double clicked.")]
		public event ODGridClickEventHandler CellDoubleClick=null;

		///<summary></summary>
		[Category("OD")]
		[Description("Occurs when a combo box item is selected.")]
		public event ODGridClickEventHandler CellSelectionCommitted=null;

		///<summary></summary>
		[Category("OD")]
		[Description("Occurs when a cell is single clicked.")]
		public event ODGridClickEventHandler CellClick=null;

		///<summary></summary>
		[Category("OD")]
		[Description("Event used when cells are editable.  The TextChanged event is passed up from the textbox where the editing is taking place.")]
		public event EventHandler CellTextChanged=null;

		///<summary></summary>
		[Category("OD")]
		[Description("Event used when cells are editable.  LostFocus event is passed up from the textbox where the editing is taking place.")]
		public event ODGridClickEventHandler CellLeave=null;

		///<summary></summary>
		[Category("OD")]
		[Description("Event used when cells are editable.  GotFocus event is passed up from the textbox where the editing is taking place.")]
		public event ODGridClickEventHandler CellEnter=null;

		///<summary></summary>
		[Category("OD")]
		[Description("Event used when cells are editable.  KeyDown event is passed up from the textbox where the editing is taking place.")]
		public event ODGridKeyEventHandler CellKeyDown=null;

		[Category("OD")]
		[Description("Occurs when rows are selected or unselected by the user for any reason, including mouse and keyboard clicks.  Only works for GridSelectionModes.One for now (enhance later).  Excludes programmatic selection.")]
		public event EventHandler SelectionCommitted=null;

		///<summary></summary>
		[Category("OD")]
		[Description("If HasAddButton is true, this event will fire when the add button is clicked.")]
		public event EventHandler TitleAddClick=null;

		///<summary></summary>
		[Category("OD")]
		[Description("If AllowSortingByColumn is true, this event will fire when a column header is clicked and before the grid has sorted. Used to disable pagination so that the entire grid's data is sorted.")]
		public event EventHandler ColumnSortClick=null;

		///<summary></summary>
		[Category("OD")]
		[Description("If AllowSortingByColumn is true, this event will fire when a column header is clicked and the grid is sorted.  Used to reselect rows after sorting.")]
		public event EventHandler ColumnSorted=null;

		///<summary>This is used to dynamically line up "total" boxes below the grid while scrolling.</summary>
		[Category("OD")]
		[Description("If HScrollVisible is true, this event will fire when the horizontal scroll bar moves by mouse, keyboard, or programmatically.")]
		public event ScrollEventHandler HorizScrolled=null;
		#endregion Events

		#region Enums
		///<summary>Deprecated.  Do not use.</summary>
		public enum GridPagingMode {
			///<summary>Default. Grid will not load pages.</summary>
			Disabled,
			///<summary>Grid will load a specific number of rows per page determined by MaxPageRows.</summary>
			Enabled,
			///<summary>Grid will display pertinent information starting at the bottom of the page instead of the top. Includes several niceities like automatically scrolling to the bottom of the page after navigation has occurred. E.g. See the Progress Notes grid in the Chart module.</summary>
			EnabledBottomUp
		}

		///<summary>Used to locally keep track of paging data and if the grid needs to invoke funcs defined by the calling member.</summary>
		private enum PagingIndexState {
			///<summary>Used when an index is beyond the possible index of items we have.</summary>
			NotValid,
			///<summary>Used when an index is less then filtered list size, indicates row filter and creation was already handled.</summary>
			ValidDataPreviouslyLoaded,
			///<summary>Used when an index is valid but needs to run filter logic and then row creation logic if filter logic return true.</summary>
			ValidDataNotLoaded,
		}
		#endregion Enums

		#region Properties - Public
		///<summary></summary>
		[Category("OD")]
		[Description("Set false to disable row selection when user clicks.  Row selection should then be handled by the form using the cellClick event.")]
		[DefaultValue(true)]
		public bool AllowSelection { get; set; }=true;

		protected override bool IsInputKey(Keys keyData) {
			if(keyData==Keys.Up || keyData==Keys.Down){
				return true;//up and down will get sent to our KeyDown event.
			}
			return base.IsInputKey(keyData);
		}

		///<summary>Set true to allow user to click on column headers to sort rows, alternating between ascending and descending.</summary>
		[Category("OD")]
		[Description("Set true to allow user to click on column headers to sort rows, alternating between ascending and descending.")]
		[DefaultValue(false)]
		public bool AllowSortingByColumn {
			get => _sortingAllowByColumn;
			set {
				_sortingAllowByColumn=value;
				if(!_sortingAllowByColumn) {
					_sortedByColumnIdx=-1;
				}
			}
		}

		///<summary></summary>
		[Category("OD")]
		[Description("Set true if you want the up and down arrows to work even when the grid does not have focus.")]
		[DefaultValue(false)]
		public bool ArrowsWhenNoFocus { get; set; }=false;

		///<summary>The background color that is used for selected rows.</summary>
		[Category("OD")]
		[Description("The background color that is used for selected rows.")]
		[DefaultValue(typeof(Color),"205,220,235")]//Silver was the old color.  This one is a lighter blue/gray.
		public Color ColorSelectedRow {
			get => _colorSelectedRow;
			set {
				_colorSelectedRow=value;
				Invalidate();
			}
		}

		///<summary>If right click, and there is 'PatNum:##', 'TaskNum:##', or 'JobNum:##', then the context menu will show a clickable link. Only used task edit, task list, and account commlog.</summary>
		[Category("OD")]
		[Description("If right click, and there is 'PatNum:##', 'TaskNum:##', or 'JobNum:##', then the context menu will show a clickable link. Only used task edit, task list, and account commlog.")]
		[DefaultValue(false)]
		public bool DoShowRightClickLinks { get; set; }=false;

		///<summary>Only affects grids with editable columns. True allows carriage returns within cells. False causes carriage returns to go to the next editable cell.</summary>
		[Category("OD")]
		[Description("Only affects grids with editable columns. True allows carriage returns within cells. False causes carriage returns to go to the next editable cell.")]
		[DefaultValue(false)]
		public bool EditableAcceptsCR { get; set; } =false;

		///<summary>If column is editable and user presses Enter, default behavior is to move right.  For some grids, default behavior needs to move down. Only intended for columns of numbers where you would never go sideways.</summary>
		[Category("OD")]
		[Description("If column is editable and user presses Enter, default behavior is to move right.  For some grids, default behavior needs to move down. Only intended for columns of numbers where you would never go sideways.")]
		[DefaultValue(false)]
		public bool EditableEnterMovesDown { get; set; }

		[Category("OD")]
		[Description("Set true to use RichTextBoxes for editable cells")]
		[DefaultValue(false)]
		public bool EditableUsesRTF { get; set; } =false;

		///<summary>Optional, for GridSelectionMode.OneCell. When set, the grid calls it with (column index, row index) before it opens a text box or a
		///combo box on a cell of an editable or combo column. Returning false leaves that one cell read-only, so that rows of one grid can differ, for
		///example a category that is read-only on some rows only. Null means every cell of those columns can be edited.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Func<int,int,bool> FuncCellIsEditable { get; set; }=null;

		///<summary></summary>
		[Category("OD")]
		[Description("js Not allowed to be used in OD proper. Set to true to show an add button on the right side of the title bar.")]
		//[NotifyParentProperty(true),RefreshProperties(RefreshProperties.Repaint)]
		[DefaultValue(false)]
		public bool HasAddButton {
			get {return _hasAddButton;}
			set {
				_hasAddButton=value;
				Invalidate();
			}
		}

		///<summary></summary>
		[Category("OD")]
		[Description("Set to true to have alternate rows colored a very pale green.")]
		[DefaultValue(false)]
		public bool HasAlternateRowsColored {
			get {
				return _hasAlternateRowsColored;
			}
			set {
				_hasAlternateRowsColored=value;
				Invalidate();
			}
		}

		[Category("OD")]
		[Description("Set to true to wrap headers automatically if the length of the header text is longer than the column. HasMultiLineHeaders must be set to true for this to work.")]
		[DefaultValue(false)]
		public bool HasAutoWrappedHeaders {
			get {
				return _hasAutoWrappedHeaders;
			}
			set {
				_hasAutoWrappedHeaders=value;
				if(value) {
					HasMultilineHeaders=true;
				}
			}
		}

		///<summary>Allow rows to drop down other rows. Leave enough space in the row's first cell to display a drop down arrow. Rows that can drop down must have a parent row set.</summary>
		[Category("OD")]
		[Description("Allow rows to drop down other rows. Leave enough space in the row's first cell to display a drop down arrow. Rows that can drop down must have a parent row set.")]
		[DefaultValue(false)]
		public bool HasDropDowns {
			get {
				return _hasDropDowns;
			}
			set {
				_hasDropDowns=value;
				Invalidate();
			}
		}

		///<summary>Set false to disallow links from being automatically added to right click menus.</summary>
		[Category("OD")]
		[Description("Set false to disallow links from being automatically added to right click menus.")]
		[DefaultValue(true)]
		public bool HasLinkDetect { get; set; } = true;

		///<summary>Allow Headers to be multiple lines tall.</summary>
		[Category("OD")]
		[Description("Set true to allow new line characters in column headers.")]
		[DefaultValue(false)]
		public bool HasMultilineHeaders {
			get {
				return _hasMultilineHeaders;
			}
			set {
				_hasMultilineHeaders=value;
				if(!_hasMultilineHeaders) {
					_hasAutoWrappedHeaders=false;
				}
				Invalidate();
			}
		}

		///<summary>Set false to hide the column header row below the main title.</summary>
		[Category("OD")]
		[Description("Set false to hide the column header row below the main title.")]
		[DefaultValue(true)]
		public bool HeadersVisible {
			get {
				return _headersVisible;
			}
			set {
				if(_headersVisible==value){
					return;
				}
				_headersVisible=value;
				if(_headersVisible){
					_heightHeader=GridScreenLook.GetHeightHeader96();
					_heightHeaderPrint=15;
				}
				else{
					_heightHeader=0;
					_heightHeaderPrint=0;
				}
				LayoutScrolls();
			}
		}

		///<summary>Set true to show a horizontal scrollbar.  Vertical scrollbar always shows, but is disabled if not needed.  If hScroll is not visible, then grid will auto reset width to match width of columns.</summary>
		[Category("OD")]
		[Description("Set true to show a horizontal scrollbar.")]
		[DefaultValue(false)]
		public bool HScrollVisible {
			get {
				return _hScrollVisible;
			}
			set {
				_hScrollVisible=value;
				LayoutScrolls();
			}
		}

		///<summary>The starting column for notes on each row.  Notes are not part of the main row, but are displayed on subsequent lines.</summary>
		[Category("OD")]
		[Description("The starting column for notes on each row.  Notes are not part of the main row, but are displayed on subsequent lines.")]
		[DefaultValue(0)]
		public int NoteSpanStart { get; set; } = 0;

		///<summary>The ending column for notes on each row.  Notes are not part of the main row, but are displayed on subsequent lines.  If this remains 0, then notes will be entirey skipped for this grid.  There is no grid line on the right side of a note.</summary>
		[Category("OD")]
		[Description("The ending column for notes on each row.  Notes are not part of the main row, but are displayed on subsequent lines.  If this remains 0, then notes will be entirely skipped for this grid.  There is no grid line on the right side of a note.")]
		[DefaultValue(0)]
		public int NoteSpanStop { get; set; } = 0;

		///<summary></summary>
		[Category("OD")]
		[Description("None, OneRow, OneCell, MultiExtended. Default is OneRow.")]
		[DefaultValue(typeof(GridSelectionMode),"OneRow")]
		public GridSelectionMode SelectionMode {
			get {
				return _selectionMode;
			}
			set {
				if((GridSelectionMode)value==GridSelectionMode.OneCell) {
					_selectedCell=new Point(-1,-1);
					_listSelectedIndices=new List<int>();
				}
				_selectionMode=value;
			}
		}

		///<summary></summary>
		[Category("OD")]
		[Description("Set to false to disable the context menu for the grid.")]
		[DefaultValue(true)]
		public bool ShowContextMenu { get; set; } = true;

		///<summary>False by default: a ContextMenuStrip that a caller sets shows only its own items, as before. True: the grid adds Copy Cell Text,
		///Copy Rows and the links to it each time it opens, as it does to its own menu. S3a-M1 to S3a-M3b turned the WinForms ContextMenus that
		///callers set on grids (which always got the grid items) into ContextMenuStrips with this set to true.</summary>
		[Category("OD")]
		[Description("Set to true to add Copy Cell Text, Copy Rows and the links to the ContextMenuStrip set on the grid.")]
		[DefaultValue(false)]
		public bool DoAddGridItemsToContextMenuStrip {
			get {
				return _doAddGridItemsToContextMenuStrip;
			}
			set {
				_doAddGridItemsToContextMenuStrip=value;
				HookContextMenuStrip();
			}
		}
		private bool _doAddGridItemsToContextMenuStrip=false;

		///<summary>The title of the grid which shows across the top.</summary>
		[Category("OD")]
		[Description("The title of the grid which shows across the top.")]
		[DefaultValue("")]
		public string Title {
			get {
				return _title;
			}
			set {
				_title=value;
				Invalidate();
			}
		}

		///<summary></summary>
		[Category("OD")]
		[Description("Set false to hide the main title. In rare cases, this could also be used to draw your own title bar.")]
		[DefaultValue(true)]
		public bool TitleVisible {
			get {
				return _titleVisible;
			}
			set {
				if(_titleVisible==value){
					return;
				}
				_titleVisible=value;
				if(_titleVisible){
					_heightTitle=GridScreenLook.GetHeightTitle96();
					_heightTitlePrint=HEIGHT_TITLE_PRINT;
				}
				else{
					_heightTitle=0;
					_heightTitlePrint=0;
				}
				LayoutScrolls();
			}
		}

		///<summary>Uniquely identifies the grid to translate title to another language.  Name it like 'Table...'  Grid contents must be manually translated.</summary>
		[Category("OD")]
		[Description("Uniquely identifies the grid to translate title to another language.  Name it like 'Table...'  Grid contents must be manually translated.")]
		[DefaultValue("")]
		public string TranslationName { get; set; } = "";

		///<summary>Normally true to show vertical scroll. False will hide it IF the grid is short enough so that the vertical scroll is not needed. Do not set to false without reading code comments.</summary>
		[Category("OD")]
		[Description("Normally true to show vertical scroll. False will hide it IF the grid is short enough so that the vertical scroll is not needed. Do not set to false without reading code comments.")]
		[DefaultValue(true)]
		public bool VScrollVisible {
			//There is a known issue if this is set to false. We will not fix it.
			//On initial load, if content is taller than grid, then scrollbar will overlap content on the right.
			//There is almost never any need to set this to false. If you do, be aware of the above issue.
			get {
				return _vScrollVisible;
			}
			set {
				_vScrollVisible=value;
				LayoutScrolls();
			}
		}

		///<summary>Text within each cell will wrap, making some rows taller. Default true.</summary>
		[Category("OD")]
		[Description("Text within each cell will wrap, making some rows taller.")]
		[DefaultValue(true)]
		public bool WrapText { 
			get {
				return _wrapText;
			}
			set{ 
				_wrapText=value;
				if(value) {
					_stringFormat.FormatFlags = 0;
				}
				else {
					_stringFormat.FormatFlags = StringFormatFlags.NoWrap;
				}
			}
		}
		#endregion Properties - Public

		#region Properties - Not Browsable
		//todo: These all need to be reviewed and simplified

		///<summary>The width of the "+" button.</summary>
		protected int AddButtonWidth {
			get;
			private set;
		}

		///<summary>Used to "gray out" AddButton when functionality should be disabled.  Only affects UI; disabling functionality must be implemented in event handler.</summary>
		[Browsable(false)]
		[DefaultValue(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AddButtonEnabled{
			get{
				return _addButtonEnabled;
			}
			set{
				_addButtonEnabled=value;
				Refresh();
			}
		}

		///<summary>Gets the List of GridColumns assigned to the ODGrid control.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GridColumnCollection Columns { get;	} //had to be initialized in constructor

		///<summary>If not set, null. If set, this is used instead of _fontCell and _fontCellBold.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Font FontForSheets{
			set{
				if(_fontForSheets==value){
					return;
				}
				_fontForSheets?.Dispose();
				_fontForSheetsBold?.Dispose();
				_fontForSheets=value;
				_fontForSheetsBold=new Font(_fontForSheets,FontStyle.Bold);
			}
		}

		///<summary>Height of the horizontal scrollbar. Will give the same value whether it's visible or not.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int HScrollHeight {
			get {
				return hScroll.Height;
			}
		}

		///<summary>Gets the position of the horizontal scrollbar.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int HScrollValue {
			get {
				if(_hScrollVisible && hScroll.Enabled) {
					return hScroll.Value;
				}
				return 0;
			}
		}

		///<summary>Gets the List of GridRows assigned to the ODGrid control.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public List<GridRow> ListGridRows { get; } = new List<GridRow>();

		///<summary>Gets the small change value for the vertical scrollbar.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int ScrollSmallChange {
			get {
				return vScroll.SmallChange;
			}
		}

		///<summary>Gets the large change value for the vertical scrollbar.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int ScrollLargeChange {
			get {
				return vScroll.LargeChange;
			}
		}

		///<summary>Gets the maximum value of the vertical scrollbar.</summary>
		[Browsable(false)]
		[DefaultValue(100)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int ScrollMaximum {
			get {
				return vScroll.Maximum;
			}
		}

		///<summary>Gets or sets the value of the vertical scrollbar.  Does all error checking and invalidates.</summary>
		[Browsable(false)]
		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int ScrollValue {
			get {
				return vScroll.Value;
			}
			set {
				if(!vScroll.Enabled) {
					return;
				}
				//Can't do this because UserControlJobEdit:658 etc uses this to reset if > max
				//if(vScroll.Value==value){
				//	return;
				//}
				int newValue;
				if(value>vScroll.Maximum-vScroll.LargeChange){
					newValue=vScroll.Maximum-vScroll.LargeChange;
				}
				else if(value<vScroll.Minimum) {
					newValue=vScroll.Minimum;
				}
				else {
					newValue=value;
				}
				try {
					vScroll.Value=newValue;
				}
				catch(Exception e) {//This should never ever happen.
					//Showing a messagebox is NOT our normal way of handling errors on mouse events, but the user would get a popup for the unhandled exception, anyway.
					MessageBox.Show("Error: Invalid Scroll Value. \r\n"
						+"Scroll value from: "+vScroll.Value+"\r\n"
						+"Scroll value to: "+newValue+"\r\n"
						+"Min scroll value: "+vScroll.Minimum+"\r\n"
						+"Max scroll value: "+vScroll.Maximum+"\r\n"
						+"Large change value: "+vScroll.LargeChange+"\r\n\r\n"
						+e.ToString());
					vScroll.Value=vScroll.Minimum;
				}
				textEdit?.Dispose();
				comboBox.Visible=false;
				Invalidate();
			}
		}

		///<summary>Holds the x,y values of the selected cell if in OneCell mode.  -1,-1 represents no cell selected.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Point SelectedCell {
			get {
				return _selectedCell;
			}
		}

		///<summary>Returns a list of rows selected instead of a list of indices.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public List<GridRow> SelectedGridRows {
			get {
				if(SelectionMode==GridSelectionMode.OneCell) {
					return new List<GridRow>() { ListGridRows[_selectedCell.Y] };
				}
				return _listSelectedIndices.Select(x => ListGridRows[x]).ToList();
			}
		}

		///<summary>Holds the int values of the indices of the selected rows.  To set selected indices, use SetSelected().</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int[] SelectedIndices {
			get {
				if(SelectionMode==GridSelectionMode.OneCell) {
					if(_selectedCell.Y==-1){
						return new int[0];
					}
					return new int[] { _selectedCell.Y };
				}
				_listSelectedIndices.Sort();
				int[] intArray=_listSelectedIndices.ToArray();
				return intArray;
			}
		}		
		#endregion Properties - Not Browsable

		#region Methods - Public
		///<summary>Call this before adding any rows.  You would typically call Rows.Clear after this.</summary>
		public void BeginUpdate() {
			_isUpdating=true;
		}

		///<summary>Must be called after adding rows.  This computes the columns, computes the rows, lays out the scrollbars, clears SelectedIndices, and invalidates.  Does not zero out scrollVal.  Also used to recompute columns and rows when you want a different dpi. Examples are: 96dpi old monitors, 96dpi sheets, 100dpi printing, 144 dpi for 150% monitor, etc.</summary>
		public void EndUpdate(){
			//Sometimes, it seems like scrollVal needs to be reset somehow because it's an inappropriate number, and when you first grab the scrollbar, it jumps.  No time to investigate.
			if(this.IsDisposed) {
				//todo:
				//Occassionally our customers report to us that they get a 'Cannot access a disposed object' from inside this method. We don't have
				//a good idea why this is happening since these errors usually occur on a form that doesn't have any threading. We're putting this here to
				//hopefully prevent some of these errors.
				return;
			}
			_isUpdating=false;
			ComputeColumns();
			using Graphics g=this.CreateGraphics();
			if(IsForSheets){
				ComputeRows(g,doActualCalc:true);
			}
			else if(_hasEditableColumn) {
				//Always truly calculate row height when a column is editable to prevent exceptions with the edit box.
				ComputeRows(g,doActualCalc:true);
			}
			else if(ListGridRows.Count<2000){
				//See notes at bottom of this file. This prevents scroll jumping, but doesn't scale to massive numbers of rows.
				ComputeRows(g,doActualCalc:true);
			}
			else{
				ComputeRows(g,doActualCalc:false);
			}
			FillLastPage(g);
			LayoutScrolls();
			hScroll.Value=0;
			_listSelectedIndices=new List<int>();
			_selectedCell=new Point(-1,-1);
			_indexRowFocused=-1;
			textEdit?.Dispose();
			_sortedByColumnIdx=-1;
			Invalidate();
		}

		///<summary>Exports the grid to a text or Excel file. The user chooses the location of the export file.</summary>
		public void Export(string fileName) {
			string selectedFilePath=ODFileUtils.CombinePaths(Path.GetTempPath(),fileName);
			SaveFileDialog saveFileDialog=new SaveFileDialog();
			saveFileDialog.AddExtension=true;
			saveFileDialog.FileName=fileName;
			if(!Directory.Exists(PrefC.GetString(PrefName.ExportPath))) {
				try {
					Directory.CreateDirectory(PrefC.GetString(PrefName.ExportPath));
					saveFileDialog.InitialDirectory=PrefC.GetString(PrefName.ExportPath);
				}
				catch {
					//initialDirectory will be blank
				}
			}
			else {
				saveFileDialog.InitialDirectory=PrefC.GetString(PrefName.ExportPath);
			}
			saveFileDialog.Filter="Text files(*.txt)|*.txt|Excel Files(*.xls)|*.xls|All files(*.*)|*.*";
			saveFileDialog.FilterIndex=0;
			if(saveFileDialog.ShowDialog()!=DialogResult.OK) {
				return;
			}
			selectedFilePath=saveFileDialog.FileName;
			try {
				using(StreamWriter sw=new StreamWriter(selectedFilePath,false)) {
					String line="";
					for(int i = 0; i < Columns.Count; i++) {
						line+="\""+Columns[i].Heading+"\"";
						if(i < Columns.Count-1) {
							line+="\t";
						}
					}
					sw.WriteLine(line);
					for(int i = 0;i<ListGridRows.Count;i++) {
						line="";
						for(int j = 0; j < Columns.Count; j++) {
							line+="\""+ListGridRows[i].Cells[j].Text.Replace("\r\n",", ")+"\"";
							if(j < Columns.Count-1) {
								line+="\t";
							}
						}
						sw.WriteLine(line);
					}
				}
			}
			catch {
				MessageBox.Show(Lans.g(this,"File in use by another program.  Close and try again."));
				return;
			}
			MessageBox.Show(Lans.g(this,"File created successfully"));
		}

		///<summary>Returns the text in the cell for the given row and column. Will throw if either index is invalid.</summary>
		public string GetText(int cellRow,int cellColumn) {
			return ListGridRows[cellRow].Cells[cellColumn].Text;
		}

		///<summary>This is designed for a very specific scenario with no horizontal scrollbar and all column widths defined.  Use this method to get the ideal width of the grid, based on column widths.  This method does not take into account current grid width at all, preventing circular logic.</summary>
		public int GetIdealWidth(){
			int width=0;
			for(int i=0;i<Columns.Count;i++){
				width+=Columns[i].ColWidth;
			}
			width=ScaleI(width);
			width+=vScroll.Width+2;
			return width;
		}

		///<summary>Returns row. -1 if no valid row.  Supply the y position in pixels. Always returns the value in terms of the currently displaying rows.</summary>
		public int PointToRow(int y) {
			if(y<1+OriginY()) {
				return -1;
			}
			for(int i=0;i<ListGridRows.Count;i++) {
				if(!ListGridRows[i].State.Visible){
					continue;
				}
				if(y>-vScroll.Value+1+OriginY()+ListGridRows[i].State.YPos+ListGridRows[i].State.HeightMain+ListGridRows[i].State.HeightNote) {
					continue;//clicked below this row.
				}
				return i;
			}
			return -1;
		}

		///<summary>Returns col.  Supply the x position in pixels. -1 if no valid column.</summary>
		public int PointToCol(int x) {
			int colRight;//the right edge of each column
			for(int i=0; i < Columns.Count; i++) {
				colRight=0;
				for(int c=0; c < i + 1; c++) {
					colRight+= Columns[c].State.Width;
				}
				if(x > -hScroll.Value + colRight) {
					continue;//clicked to the right of this col
				}
				return i;
			}
			return -1;
		}

			///<summary>(Not used for sheets) If there are more pages to print, it returns -1.  If this is the last page, it returns the yPos of where the printing stopped.  Graphics will be paper, pageNumber resets some class level variables at page 0, bounds are used to contain the grid drawing, and marginTopFirstPage leaves room so as to not overwrite the title and subtitle.</summary>
		public int PrintPage(Graphics g,int pageNumber,Rectangle bounds,int marginTopFirstPage,bool HasHeaderSpaceOnEveryPage=false) {
			//The page draws with the print fonts, and so do the row heights it computes (UI-2a-3). PrintPageHelper turns _isPrinting off itself
			//before it puts the screen's scale and row heights back.
			_isPrinting=true;
			try {
				return PrintPageHelper(g,pageNumber,bounds,marginTopFirstPage,HasHeaderSpaceOnEveryPage);
			}
			finally {
				_isPrinting=false;
			}
		}

		private int PrintPageHelper(Graphics g,int pageNumber,Rectangle bounds,int marginTopFirstPage,bool HasHeaderSpaceOnEveryPage) {
			//Printers ignore TextRenderingHint.AntiAlias.  
			//And they ignore SmoothingMode.HighQuality.
			//They seem to do font themselves instead of letting us have control.
			//g.TextRenderingHint=TextRenderingHint.AntiAlias;//an attempt to fix the printing measurements.
			//g.SmoothingMode=SmoothingMode.HighQuality;
			//g.PageUnit=GraphicsUnit.Display;
			//float pagescale=g.PageScale;
			//g.PixelOffsetMode=PixelOffsetMode.HighQuality;
			//g.		
			float scaleTotal=_scaleMS*_zoomLocal;//example 1.5*1.2
			if(scaleTotal!=1) {
				_printWidth=(int)(Width/scaleTotal);//calc unscaled Width
			}
			//Save the current scale and adjust the rows and columns temporarily for printing.  Restore the rows and column sizes at the end of the method.
			float scaleMS=_scaleMS;
			float zoomLocal=_zoomLocal;
			SetScaleAndZoom(1,1);
			if(_printedRows==0 || scaleMS!=1 || zoomLocal!=1) {
				//Recalculate row heights, 4% larger on the first page of printing, or when using zoom.
				ComputeRows(g,doActualCalc:true);
			}
			else if(!_areRowHeightsForPrint && !AreScreenFontsThePrintFonts()) {
				//A grid printed again at 100%: the heights are the ones the end of the last printout (or a refill) measured on screen, with the
				//screen fonts. While those are the print fonts (today's tokens) that is what this printout always used, so nothing changes. With other
				//screen fonts, measure the heights the same way (on screen) with the print fonts.
				using Graphics graphicsScreen=this.CreateGraphics();
				ComputeRows(graphicsScreen,doActualCalc:true);
			}
			int xPos=bounds.Left;
			//now, try to center in bounds
			if(_widthTotal<bounds.Width) {
				xPos=(int)(bounds.Left+bounds.Width/2-(float)_widthTotal/2);
			}
			SolidBrush textBrush;
			RectangleF textRect;
			Font font=_fontPrintCell;//do not dispose of this font ref.
			//Initialize our pens for drawing.
			int yPos=bounds.Top;
			if(HasHeaderSpaceOnEveryPage) {
				yPos=marginTopFirstPage;//Margin is lower because title and subtitle are printed externally.
			}
			if(pageNumber==0) {
				yPos=marginTopFirstPage;//Margin is lower because title and subtitle are printed externally.
				_printedRows=0;
				_printNoteRemaining="";
			}
			bool isFirstRowOnPage=true;//helps with handling a very tall first row
			#region ColumnHeaders
			//Print column headers on every page.
			g.FillRectangle(Brushes.LightGray,xPos+Columns[0].State.XPos,yPos,_widthTotal,_heightHeaderPrint);
			g.DrawRectangle(Pens.Black,xPos+Columns[0].State.XPos,yPos,_widthTotal,_heightHeaderPrint);
			for(int i=1;i<Columns.Count;i++) {
				g.DrawLine(Pens.Black,xPos+Columns[i].State.XPos,yPos,xPos+Columns[i].State.XPos,yPos+_heightHeaderPrint);
			}
			for(int i=0;i<Columns.Count;i++){
				g.DrawString(Columns[i].Heading,_fontPrintHeader,Brushes.Black,
					xPos+Columns[i].State.XPos + Columns[i].State.Width /2- g.MeasureString(Columns[i].Heading, _fontPrintHeader).Width/2,
					yPos);
			}
			yPos+=_heightHeaderPrint;
			#endregion ColumnHeaders
			Pen gridPen=GridPrintLook.PenGridline;
			Pen lowerPen=new Pen(GridPrintLook.PenGridline.Color);
			if(_printedRows==ListGridRows.Count-1) {//last row
				lowerPen=new Pen(GridPrintLook.PenColumnSeparator.Color);
			}
			else {
				if(ListGridRows.Count>0 && ListGridRows[_printedRows].ColorLborder!=Color.Empty) {
					lowerPen=new Pen(ListGridRows[_printedRows].ColorLborder);
				}
			}
			#region Rows
			while(_printedRows<ListGridRows.Count) {
				#region RowMainPart
				if(_printNoteRemaining=="") {//We are not in the middle of a note from a previous page. If we are in the middle of a note that will get printed next, as it is the next region of code (RowNotePart).
					//Go to next page if it doesn't fit.
					if(yPos+(float)ListGridRows[_printedRows].State.HeightMain > bounds.Bottom) {//The row is too tall to fit
						if(isFirstRowOnPage) {
							//todo some day: handle very tall first rows.  For now, print what we can.
						}
						else {
							break;//Go to next page.
						}
					}
					//There is enough room to print this row.
					//Draw the left vertical gridline
					g.DrawLine(gridPen,
						xPos+Columns[0].State.XPos,
						yPos,
						xPos+Columns[0].State.XPos,
						yPos+ListGridRows[_printedRows].State.HeightMain);
					for(int i=0; i < Columns.Count; i++) {
						//Draw the other vertical gridlines
						g.DrawLine(gridPen,
							xPos+Columns[i].State.Right,
							yPos,
							xPos+Columns[i].State.Right,
							yPos+ListGridRows[_printedRows].State.HeightMain);
						if(ListGridRows[_printedRows].Note=="") {//End of row. Mark with a dark line (lowerPen).
							//Horizontal line which divides the main part of the row from the notes section of the row one column at a time.
							g.DrawLine(lowerPen,
								xPos+Columns[0].State.XPos,
								yPos+ListGridRows[_printedRows].State.HeightMain,
								xPos+Columns[Columns.Count-1].State.Right,
								yPos+ListGridRows[_printedRows].State.HeightMain);
						}
						else {//Middle of row. Still need to print the note part of the row. Mark with a medium line (gridPen).
							//Horizontal line which divides the main part of the row from the notes section of the row one column at a time.
							g.DrawLine(gridPen,
								xPos+Columns[0].State.XPos,
								yPos+ListGridRows[_printedRows].State.HeightMain,
								xPos+Columns[Columns.Count-1].State.Right,
								yPos+ListGridRows[_printedRows].State.HeightMain);
						}
						//text
						if(ListGridRows[_printedRows].Cells.Count-1< i) {
							continue;
						}
						switch(Columns[i].TextAlign) {
							case HorizontalAlignment.Left:
								_stringFormat.Alignment = StringAlignment.Near;
								break;
							case HorizontalAlignment.Center:
								_stringFormat.Alignment = StringAlignment.Center;
								break;
							case HorizontalAlignment.Right:
								_stringFormat.Alignment = StringAlignment.Far;
								break;
						}
						if(ListGridRows[_printedRows].Cells[i].ColorText== Color.Empty) {
							textBrush=new SolidBrush(ListGridRows[_printedRows].ColorText);
						}
						else {
							textBrush=new SolidBrush(ListGridRows[_printedRows].Cells[i].ColorText);
						}
						if(ListGridRows[_printedRows].Cells[i].Bold== YN.Yes) {
							font= _fontPrintCellBold;
						}
						else if(ListGridRows[_printedRows].Cells[i].Bold== YN.No) {
							font= _fontPrintCell;
						}
						else {//unknown.  Use row bold
							if(ListGridRows[_printedRows].Bold) {
								font= _fontPrintCellBold;
							}
							else {
								font= _fontPrintCell;
							}
						}
						//Do not underline if printing grid
						//if(rows[RowsPrinted].Cells[i].Underline==YN.Yes) {//Underline the current cell.  If it is already bold, make the cell bold and underlined.
						//	cellFont=new Font(cellFont,(cellFont.Bold)?(FontStyle.Bold | FontStyle.Underline):FontStyle.Underline);
						//}
						//Some printers will malfunction (BSOD) if print bold colored fonts.  This prevents the error.
						if(textBrush.Color != Color.Black && font.Bold) {
							font= _fontPrintCell;
						}
						if(Columns[i].TextAlign== HorizontalAlignment.Right) {
							textRect=new RectangleF(
								xPos+Columns[i].State.XPos -2,
								yPos,
								Columns[i].State.Width +2,
								ListGridRows[_printedRows].State.HeightMain);
							//shift the rect to account for MS issue with strings of different lengths
							//js- 5/2/11,I don't understand this.  I would like to research it.
							textRect.Location=new PointF
								(textRect.X + g.MeasureString(ListGridRows[_printedRows].Cells[i].Text, font).Width/ textRect.Width,
								textRect.Y);
							//g.DrawString(rows[RowsPrinted].Cells[i].Text,cellFont,textBrush,textRect,_format);

						}
						else {
							textRect=new RectangleF(
								xPos+Columns[i].State.XPos,
								yPos,
								Columns[i].State.Width,
								ListGridRows[_printedRows].State.HeightMain);
							//g.DrawString(rows[RowsPrinted].Cells[i].Text,cellFont,textBrush,textRect,_format);
						}
						g.DrawString(ListGridRows[_printedRows].Cells[i].Text, font, textBrush, textRect, _stringFormat);
					}
					yPos+=(int)((float)ListGridRows[_printedRows].State.HeightMain);//Move yPos down the length of the row (not the note).
				}
				#endregion RowMainPart
				#region NotePart
				if(ListGridRows[_printedRows].Note=="") {
					_printedRows++;//There is no note. Go to next row.
					isFirstRowOnPage=false;
					continue; 
				}
				//Figure out how much vertical distance the rest of the note will take up.
				int noteHeight;
				int noteW=0;
				_stringFormat.Alignment=StringAlignment.Near;
				for(int i=NoteSpanStart;i<=NoteSpanStop;i++) {
					noteW+=Columns[i].State.Width;
				}
				if(_printNoteRemaining=="") {//We are not in the middle of a note.
					_printNoteRemaining=ListGridRows[_printedRows].Note;
				}
				noteHeight=(int)g.MeasureString(_printNoteRemaining,font,noteW,_stringFormat).Height; //This is how much height the rest of the note will take.
				bool roomForRestOfNote=false;
				//Test to see if there's enough room on the page for the rest of the note.
				if(yPos+noteHeight<bounds.Bottom) {
					roomForRestOfNote=true;
				}
				#region PrintRestOfNote
				if(roomForRestOfNote) { //There is enough room
					//print it
					//draw lines for the rest of the note
					if(noteHeight>0) {
						//left vertical gridline
						if(NoteSpanStart!=0) {
							g.DrawLine(gridPen,
								xPos+(float)Columns[NoteSpanStart].State.XPos,
								yPos,
								xPos+(float)Columns[NoteSpanStart].State.XPos,
								yPos+noteHeight);
						}
						//right vertical gridline
						g.DrawLine(gridPen,
							xPos+Columns[Columns.Count-1].State.Right,
							yPos,
							xPos+Columns[Columns.Count-1].State.Right,
							yPos+ noteHeight);
						//left vertical gridline
						g.DrawLine(gridPen,
							xPos+Columns[0].State.XPos,
							yPos,
							xPos+Columns[0].State.XPos,
							yPos+noteHeight);
					}
					//lower horizontal gridline gets marked with the dark lowerPen since this is the end of a row
					g.DrawLine(lowerPen,
						xPos+ Columns[0].State.XPos,
						yPos+ noteHeight,
						xPos+Columns[Columns.Count-1].State.Right,
						yPos+ noteHeight);
					//note text
					if(noteHeight > 0 && NoteSpanStop > 0 && NoteSpanStart < Columns.Count) {
						if(ListGridRows[_printedRows].Bold) {
							font= _fontPrintCellBold;
						}
						else {
							font= _fontPrintCell;
						}
						textBrush=new SolidBrush(ListGridRows[_printedRows].ColorText);
						textRect=new RectangleF(
							xPos+Columns[NoteSpanStart].State.XPos +1,
							yPos,
							Columns[NoteSpanStop].State.Right -Columns[NoteSpanStart].State.XPos,
							noteHeight);
						g.DrawString(_printNoteRemaining, font, textBrush, textRect, _stringFormat);
					}
					_printNoteRemaining="";
					_printedRows++;
					isFirstRowOnPage=false;
					yPos+=noteHeight;
				}
				#endregion PrintRestOfNote
				#region PrintPartOfNote
				else {//The rest of the note will not fit on this page.
					//Print as much as you can.
					noteHeight=bounds.Bottom-yPos;//This is the amount of space remaining.
					if(noteHeight<15) {
						_printWidth=0;//Reset the flag.
						_isPrinting=false;//the screen's scale, fonts and row heights again
						SetScaleAndZoom(scaleMS,zoomLocal);//Reset rows and columns back to current scale.
						return -1; //If noteHeight is less than this we will get a negative value for the rectangle of space remaining because we subtract 15 from this for the rectangle size when using measureString. This is because one line takes 15, and if there is 1 pixel of height available, measureString will fill it with text, which will then get partly cut off. So when we use measureString we will subtract 15 from the noteHeight.
					}							
					SizeF sizeF;
					int charactersFitted;
					int linesFilled;
					string noteFitted;//This is the part of the note we will print.
					//js- I'd like to incorporate ,StringFormat.GenericTypographic into the MeasureString, but can't find the overload.
					sizeF=g.MeasureString(_printNoteRemaining,font,new SizeF(noteW,noteHeight-15),_stringFormat,out charactersFitted,out linesFilled);//Text that fits will be NoteRemaining.Substring(0,charactersFitted).
					noteFitted=_printNoteRemaining.Substring(0,charactersFitted);
					//draw lines for the part of the note that fits on this page
					if(noteHeight>0) {
						//left vertical gridline
						if(NoteSpanStart!=0) {
							g.DrawLine(gridPen,
								xPos+(float)Columns[NoteSpanStart].State.XPos,
								yPos,
								xPos+(float)Columns[NoteSpanStart].State.XPos,
								yPos+noteHeight);
						}
						//right vertical gridline
						g.DrawLine(gridPen,
							xPos+Columns[Columns.Count-1].State.Right,
							yPos,
							xPos+Columns[Columns.Count-1].State.Right,
							yPos+ noteHeight);
						//left vertical gridline
						g.DrawLine(gridPen,
							xPos+Columns[0].State.XPos,
							yPos,
							xPos+Columns[0].State.XPos,
							yPos+noteHeight);
					}
					//lower horizontal gridline gets marked with gridPen since its still the middle of a row (still more note to print)
					g.DrawLine(gridPen,
						xPos+Columns[0].State.XPos,
						yPos+noteHeight,
						xPos+Columns[Columns.Count-1].State.Right,
						yPos+noteHeight);
					//note text
					if(noteHeight > 0 && NoteSpanStop > 0 && NoteSpanStart < Columns.Count) {
						if(ListGridRows[_printedRows].Bold) {
							font= _fontPrintCellBold;
						}
						else {
							font= _fontPrintCell;
						}
						textBrush=new SolidBrush(ListGridRows[_printedRows].ColorText);
						textRect=new RectangleF(
							xPos+(float)Columns[NoteSpanStart].State.XPos +1,
							yPos,
							Columns[NoteSpanStop].State.Right -Columns[NoteSpanStart].State.XPos,
							noteHeight);
						g.DrawString(noteFitted, font, textBrush, textRect, _stringFormat);
					}
					_printNoteRemaining=_printNoteRemaining.Substring(charactersFitted);
					break;
				}
				#endregion PrintPartOfNote
				#endregion Rows
			}
			#endregion Rows
			lowerPen.Dispose();
			_printWidth=0;//Reset the flag.
			_isPrinting=false;//the screen's scale, fonts and row heights again
			SetScaleAndZoom(scaleMS,zoomLocal);//Reset rows and columns back to current scale.
			if(_printedRows==ListGridRows.Count) {//done printing
				//set row heights back to screen heights.
				using(Graphics gfx=this.CreateGraphics()) {
					ComputeRows(gfx,doActualCalc:true);
				}
				return yPos;
			}
			else{//more pages to print
				return -1;
			}
		}

		///<summary>This is an alternative to setting up all of the GridRows in advance.  Set the data here as a List of DataRows.  Then, set FuncConstructGridRow to do the work of filling one DataRow.  DataRows will only be filled as they become visible for the first time or in the background, resulting in massive scalability and good speed.</summary>
		public void SetData(List<DataRow> listDataRows)  {
			_listDataRows=listDataRows;
			ListGridRows.Clear();
			DateTime dt1=DateTime.Now;
			for(int i=0;i<listDataRows.Count;i++){
				GridRow gridRow=new GridRow();
					//FuncConstructGridRow(_listDataRows[i]);//this part happens later
				ListGridRows.Add(gridRow);
			}	
			DateTime dt2=DateTime.Now;
			TimeSpan timeSpan=dt2-dt1;
		}

		///<summary>Defaults are 1,1.</summary>
		public void SetScaleAndZoom(float scaleMS,float zoomLocal){
			if(scaleMS==_scaleMS && zoomLocal==_zoomLocal){
				return;
			}
			_scaleMS = scaleMS;
			_zoomLocal=zoomLocal;
			SetFonts();
			ComputeColumns();
			using Graphics g=this.CreateGraphics();
			ComputeRows(g,doActualCalc:false);
			LayoutScrolls();
		}
		#endregion Methods - Public

		#region Methods - Event Handlers - Painting
		///<summary></summary>
		protected override void OnPaintBackground(PaintEventArgs pea) {
			//base.OnPaintBackground (pea);
			//don't paint background.  This reduces flickering.
		}

		///<summary>Runs any time the control is invalidated.</summary>
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
			if(_isUpdating) {
				return;
			}
			if(Width<1 || Height<1) {
				return;
			}
			//ComputeColumns();//it's only here because I can't figure out how to do it when columns are added. It will be removed.
			Graphics g=e.Graphics;
			//g.SmoothingMode=SmoothingMode.HighQuality;//for the up/down triangles, but it messes up the normal lines, so it's done next to triangles
			g.TextRenderingHint=TextRenderingHint.ClearTypeGridFit;
			DrawBackG(g);
			DrawRows(g);
			DrawTitle(g);
			DrawHeaders(g);//this will draw on top of any grid stuff
			DrawOutline(g);
			string stringException=null;
			if(TranslationName==null) {//If they used "", we won't complain
				stringException="TranslationName cannot be blank";
			}
			if(!string.IsNullOrEmpty(stringException)) {
				Font exceptionFont=new Font(FontFamily.GenericSansSerif,16,FontStyle.Bold);
				RectangleF rectangle=new RectangleF(0,OriginY(),Width,Height-OriginY());
				SolidBrush exceptionBrush=new SolidBrush(Color.Red);
				g.DrawString(stringException,exceptionFont,exceptionBrush,rectangle,_stringFormat);	
			}
		}

		///<summary>Draws the background below the header: SurfaceRaised (UI-5).</summary>
		private void DrawBackG(Graphics g) {
			GridScreenLook.DrawBackground(g,new Rectangle(0,OriginY(),Width,Height-OriginY()));
		}

		///<summary>Draws the background, lines, and text for all rows that are visible.</summary>
		private void DrawRows(Graphics g) {
			if(CultureInfo.CurrentCulture.Name.EndsWith("IN") && _fontCell.Name!="Arial") {
				//India. Not sure on history of this hack, but it seems harmless. Maybe MS not avail in India?
				_fontCell=new Font("Arial",_fontCell.Size);
				_fontCellBold=new Font("Arial",_fontCell.Size,FontStyle.Bold);
			}
			if(CultureInfo.CurrentCulture.Name.EndsWith("IN") && _fontPrintCell.Name!="Arial") {
				//The same for the print fonts: a grid shown on screen printed in Arial in India, because it printed with the screen fonts (UI-2a-3).
				_fontPrintCell=new Font("Arial",_fontPrintCell.Size);
				_fontPrintCellBold=new Font("Arial",_fontPrintCell.Size,FontStyle.Bold);
			}
			int rowEnd=-1;
			bool didAnyHeightChange=false;
			for(int i=0;i<ListGridRows.Count;i++) {
				if(!ListGridRows[i].State.Visible){
					continue;
				}
				if(-vScroll.Value+ListGridRows[i].State.YPos+ListGridRows[i].State.HeightMain+ListGridRows[i].State.HeightNote < 0) {
					continue;//lower edge of row above top of grid area
				}
				if(-vScroll.Value+1+ScaleI(_heightTitle+_heightHeader)+ListGridRows[i].State.YPos > Height) {//row below lower edge of control
					rowEnd=i;
					break;
				}
				if(ListGridRows[i].Cells.Count==0){
					FillRow(i);
				}
				if(!ListGridRows[i].State.IsHeightCalculated){
					int heightBefore=ListGridRows[i].State.HeightTotal;
					ComputeRowHeightOne(g,i);
					if(heightBefore!=ListGridRows[i].State.HeightTotal){
						didAnyHeightChange=true;
					}
				}
				//always compute the yPos of the row below this one in case it's stale
				if(i<ListGridRows.Count-1){//unless this is the last row
					ListGridRows[i+1].State.YPos=ListGridRows[i].State.YPos+ListGridRows[i].State.HeightTotal;
				}
				DrawRow(g,i);
			}
			if(didAnyHeightChange && rowEnd!=-1){
				ComputeRowYposStartingAt(rowEnd);
				LayoutScrolls();
			}
		}

		///<summary>Draws background, lines, image, and text for a single row.</summary>
		private void DrawRow(Graphics g, int rowI) {
			//The values for Ypos, XPos, etc. refer to actual gridline positions.
			//Cells are sequentially drawn top to bottom, left to right.
			//Cell background is drawn, followed by right and bottom gridlines.
			//So backgrounds must start 1 pix R and D to avoid drawing on top of gridlines to their L and top.
			//This rule is consistent, including cell 0,0, where the left "gridline" is the outline of the entire grid.
			//For example, if a cell is 40x10, then the fill rect is size 40x10, drawn at 1,1.
			//Then, its own gridlines are drawn at 40 and 10 on right and bottom
			//These draw right on top of the filled rectangle, resulting in perfect alignment.
			GridRow gridRow=ListGridRows[rowI];
			int top=-vScroll.Value+OriginY()+gridRow.State.YPos;//The gridline above this row.
			int hMain=gridRow.State.HeightMain;
			int hNote=gridRow.State.HeightNote;
			int hTot=gridRow.State.HeightMain+gridRow.State.HeightNote;
			Font font;//do not dispose this ref.
			//The row colours of UI-5 (GridScreenLook): the selection AccentSubtle unless the window set its own ColorSelectedRow; a row the program
			//or the office coloured keeps its colour; white rows are SurfaceRaised; hover darkens by RowHover.
			bool isSelected=_listSelectedIndices.Contains(rowI);
			Color colorSelection=GridScreenLook.GetSelectionFill(ColorSelectedRow,_colorSelectedRowStock);
			Color colorRow;
			//selected row color
			if(isSelected) {
				colorRow=GetSelectedColor(gridRow.ColorBackG,colorSelection);
			}
			//colored row background
			else if(gridRow.ColorBackG.ToArgb()!=Color.White.ToArgb()) {//.ToArgb required for value comparison
				colorRow=gridRow.ColorBackG;
			}
			else if(_hasAlternateRowsColored && rowI%2==0){//color even rows
				colorRow=Color.FromArgb(245,255,248);
			}
			//normal row color
			else {//need to draw over the gray background
				colorRow=GridScreenLook.GetRowBack(gridRow.ColorBackG);
			}
			//hover row background gets blended to make it slightly darker, as RowHover is darker than white
			if(_hoverRow==rowI){
				colorRow=GridScreenLook.BlendHover(colorRow);
			}
			using SolidBrush brushRow=new SolidBrush(colorRow);
			g.FillRectangle(brushRow,
				1,
				top+1,
				_widthTotal,
				hTot);
			//Color Individual Cells.
			for(int i=0;i<gridRow.Cells.Count;i++) {
				if(i>Columns.Count) {
					break;
				}
				if(gridRow.Cells[i].ColorBackG.ToArgb()==Color.Empty.ToArgb() && !gridRow.Cells[i].IsButton) {
					continue;
				}
				//Blend with row background colors. Cell color= Avg(CellColor+RowColor)
				Color colorCell;
				//selected row
				if(isSelected) {
					colorCell=Color.FromArgb(
						(colorSelection.R+gridRow.Cells[i].ColorBackG.R)/2,
						(colorSelection.G+gridRow.Cells[i].ColorBackG.G)/2,
						(colorSelection.B+gridRow.Cells[i].ColorBackG.B)/2);
				}
				//colored row background
				//This is wrong.  We should not have been blending cell color with row color.  Cell is supposed to override row.
				else if(gridRow.ColorBackG!=Color.White) {
					colorCell=Color.FromArgb(
						(gridRow.ColorBackG.R+gridRow.Cells[i].ColorBackG.R)/2,
						(gridRow.ColorBackG.G+gridRow.Cells[i].ColorBackG.G)/2,
						(gridRow.ColorBackG.B+gridRow.Cells[i].ColorBackG.B)/2);
				}
				//normal row color
				else {
					colorCell=gridRow.Cells[i].ColorBackG;
				}
				//blend again for hover. This could result in a blend of row+cell+hover, or selected+cell+hover
				if(_hoverRow==rowI){
					colorCell=GridScreenLook.BlendHover(colorCell);
				}
				if(gridRow.Cells[i].IsButton) {
					Rectangle rectangleButton=new Rectangle(-hScroll.Value+Columns[i].State.XPos+2,top+2,Columns[i].State.Width-4,hTot-4);
					GridScreenLook.DrawCellButton(g,rectangleButton,gridRow.Cells[i].ButtonIsPressed,Enabled,_scaleMS*_zoomLocal);
				}
				else {
					using(SolidBrush brushCell=new SolidBrush(colorCell)) {
						g.FillRectangle(brushCell,-hScroll.Value+Columns[i].State.XPos+1,top+1,Columns[i].State.Width,hTot);
					}
				}
			}
			if(_selectionMode==GridSelectionMode.OneCell && _selectedCell.X!=-1 && _selectedCell.Y!=-1
				&& _selectedCell.Y==rowI) {
				using(SolidBrush brushSelectedCell=new SolidBrush(colorSelection)) {
					g.FillRectangle(brushSelectedCell,
						-hScroll.Value+Columns[_selectedCell.X].State.XPos+1,
						top+1,
						Columns[_selectedCell.X].State.Width,
						hTot);
				}
			}
			Pen penGridline=UiTheme.GetPen(EnumUiColor.Gridline);//shared, never disposed
			//lines for note section
			if(hNote>0) {
				//left vertical gridline for note
				if(NoteSpanStart!=0) {
					g.DrawLine(penGridline,
						-hScroll.Value+Columns[NoteSpanStart].State.XPos,
						top+hMain,
						-hScroll.Value+Columns[NoteSpanStart].State.XPos,
						top+hTot);
				}
				//vertical line on very right of entire grid
				g.DrawLine(penGridline,
					-hScroll.Value+_widthTotal,
					top+hMain+1,
					-hScroll.Value+_widthTotal,
					top+hTot);
				//Horizontal line which divides the main part of the row from the notes section of the row
				g.DrawLine(penGridline,
					-hScroll.Value+Columns[0].State.XPos,
					top+hMain,
					-hScroll.Value+Columns[Columns.Count-1].State.Right,
					top+hMain);
			}
			Pen penRowLower=penGridline;//only the ColorLborder pen below is made here, and disposed a few pages down
			Pen penLborder=null;
			if(rowI==ListGridRows.Count-1) {//last row
				penRowLower=UiTheme.GetPen(EnumUiColor.Stroke);
			}
			else {
				if(gridRow.ColorLborder!=Color.Empty) {
					penLborder=new Pen(gridRow.ColorLborder);
					penRowLower=penLborder;
				}
			}
			for(int i=0;i<Columns.Count;i++) {
				//vertical line on right of cell
				g.DrawLine(penGridline,
					-hScroll.Value+Columns[i].State.Right,
					top+1,
					-hScroll.Value+Columns[i].State.Right,
					top+hMain);
				//lower horizontal gridline
				g.DrawLine(penRowLower,
					-hScroll.Value+Columns[i].State.XPos+1,
					top+hTot,
					-hScroll.Value+Columns[i].State.Right,
					top+hTot);
				//text
				if(gridRow.Cells.Count-1< i) {
					continue;
				}
				switch(Columns[i].TextAlign) {
					case HorizontalAlignment.Left:
						_stringFormat.Alignment = StringAlignment.Near;
						break;
					case HorizontalAlignment.Center:
						_stringFormat.Alignment = StringAlignment.Center;
						break;
					case HorizontalAlignment.Right:
						_stringFormat.Alignment = StringAlignment.Far;
						break;
				}
				Rectangle rectangleText=new Rectangle(-hScroll.Value+Columns[i].State.XPos,top+1,Columns[i].State.Width,hMain);
				if(WrapText && Columns[i].TextAlign==HorizontalAlignment.Left){
					//The line below is a compromise that leaves a bit of extra white space on the right,
					//but still works for sheets and printing.  See comments in ComputeRows.
					rectangleText=new Rectangle(-hScroll.Value+Columns[i].State.XPos,top+1,Columns[i].State.Width,hMain);
				}
				if(gridRow.Cells[i].IsButton){
					//handles both wrap and no wrap
					rectangleText=new Rectangle(-hScroll.Value+Columns[i].State.XPos+2,top+2,Columns[i].State.Width-1,hMain-2);
				}
				if(_hasDropDowns && i==0) {//only draw the dropdown arrow in the first column of the row.
					if(gridRow.State.DropDownState!=ODGridDropDownState.None || gridRow.DropDownParent!=null) {
						rectangleText=new Rectangle(-hScroll.Value+Columns[i].State.XPos+10,top+1,Columns[i].State.Width-10,hMain);
						//might be easier to just not support wrap in a dropdown cell. Oh well.
						if(WrapText && Columns[i].TextAlign==HorizontalAlignment.Left){
							rectangleText=new Rectangle(-hScroll.Value+Columns[i].State.XPos+10,top+1,Columns[i].State.Width-3,hMain);
						}
					}
				}
				Color colorText;
				if(gridRow.Cells[i].ColorText== Color.Empty) {
					if(gridRow.Cells[i].IsButton && !Enabled) { //set the "button" text to gray if this is grid is disabled
						colorText=UiTheme.TextDisabled;
					}
					else {
						colorText=GridScreenLook.GetTextColor(gridRow.ColorText,isSelected);
					}
				}
				else {
					colorText=GridScreenLook.GetTextColor(gridRow.Cells[i].ColorText,isSelected);
				}
				if(gridRow.Cells[i].Bold== YN.Yes) {
					font= _fontCellBold;
				}
				else if(gridRow.Cells[i].Bold== YN.No) {
					font= _fontCell;
				}
				else {//unknown.  Use row bold
					if(gridRow.Bold) {
						font= _fontCellBold;
					}
					else {
						font= _fontCell;
					}
				}
				if(gridRow.Cells[i].Underline== YN.Yes) {//Underline the current cell.  If it is already bold, make the cell bold and underlined.
					if(font == _fontCellBold){
						font= _fontUnderlineBold;
					}
					else{
						font= _fontUnderline;
					}
				}
				//If _hasDropDowns, we don't support images in the first column
				if(_hasDropDowns && i==0) { //only draw the dropdown arrow in the first column of the row.
					Pen penArrow=UiTheme.GetPen(EnumUiColor.Text);//shared, never disposed
					if(gridRow.State.DropDownState== ODGridDropDownState.Up) {//arrow pointing right
						PointF topPoint = new PointF(-hScroll.Value+Columns[i].State.XPos+3,top+(hMain/2)-5);
						PointF botPoint = new PointF(topPoint.X, topPoint.Y+10);
						PointF rightPoint = new PointF(topPoint.X+5, topPoint.Y+5);
						g.DrawPolygon(penArrow, new PointF[] { topPoint, botPoint, rightPoint });
					}
					else if(gridRow.State.DropDownState== ODGridDropDownState.Down) {//arrow pointing down
						PointF leftPoint = new PointF(-hScroll.Value+Columns[i].State.XPos+1,top+(hMain/2)-2);
						PointF rightPoint = new PointF(leftPoint.X+10, leftPoint.Y);
						PointF botPoint = new PointF(leftPoint.X+5, leftPoint.Y+5);
						g.DrawPolygon(penArrow, new PointF[] { leftPoint, botPoint, rightPoint });
					}
					//a small L indicator that shows that a row is a drop down child.
					else if(gridRow.DropDownParent!=null) {
						PointF topPoint = new PointF(-hScroll.Value+Columns[i].State.XPos+4,top+1);
						PointF midPoint = new PointF(-hScroll.Value+Columns[i].State.XPos+4,top+8);
						PointF rightPoint = new PointF(-hScroll.Value+Columns[i].State.XPos+9,top+8);
						g.DrawLines(penArrow, new PointF[] { topPoint, midPoint, rightPoint });
					}
				}
				if(Columns[i].ImageList==null){//text
					using(SolidBrush brush=new SolidBrush(colorText)){
						g.DrawString(gridRow.Cells[i].Text, font, brush, rectangleText, _stringFormat);
					}
				}
				else {//image
					int imageIndex=-1;
					if(gridRow.Cells[i].Text!="") {
						imageIndex= PIn.Int(gridRow.Cells[i].Text);
					}
					if(imageIndex != -1) {
						Image img =Columns[i].ImageList.Images[imageIndex];
						g.DrawImage(img,-hScroll.Value+Columns[i].State.XPos,top,
							ScaleI(img.Width),ScaleI(img.Height));
					}
				}
			}
			penLborder?.Dispose();
			//note text
			if(hNote>0 && NoteSpanStop>0 && NoteSpanStart<Columns.Count) {
				int noteW=0;
				for(int i=NoteSpanStart;i<=NoteSpanStop;i++) {
					noteW+=Columns[i].State.Width;
				}
				if(gridRow.Bold) {
					font= _fontCellBold;
				}
				else {
					font= _fontCell;
				}
				Rectangle rectangleNote=new Rectangle(
					-hScroll.Value+Columns[NoteSpanStart].State.XPos,
					top+hMain+1,
					Columns[NoteSpanStop].State.Right-Columns[NoteSpanStart].State.XPos,
					hNote);
				_stringFormat.Alignment = StringAlignment.Near;
				using(SolidBrush brush=new SolidBrush(GridScreenLook.GetTextColor(gridRow.ColorText,isSelected))){
					if(gridRow.Note.Length<= TEXT_LENGTH_LIMIT) {
						g.DrawString(gridRow.Note,font,brush,rectangleNote,_stringFormat);
					}
					else {
						g.DrawString(gridRow.Note.Substring(0, TEXT_LENGTH_LIMIT)+"\r\n"+Lans.g(this,"...more..."),font,brush,rectangleNote,_stringFormat);
					}
				}
			}
			//The selection bar and the focused row (UI-5, sections 3.2 and 3.6), inside the row's fill (from x 1 to its right gridline, below its top
			//gridline, above its bottom one), drawn last so a cell colour or an image in the first column does not hide them.
			if(Columns.Count==0) {
				return;
			}
			int right=Math.Min(-hScroll.Value+Columns[Columns.Count-1].State.Right,Width-1);
			float scale=_scaleMS*_zoomLocal;
			int widthBorder=UiTheme.GetLineWidth(scale);//the outer border is drawn over the row's first pixel at 200%
			Rectangle rectangleRowInside=new Rectangle(widthBorder,top+1,right-widthBorder,hTot-1);
			if(isSelected && _selectionMode!=GridSelectionMode.OneCell) {
				GridScreenLook.DrawSelectionBar(g,rectangleRowInside,IsSelectionFocused,scale);
			}
			if(rowI==GetIndexRowFocused() && IsFocusedRowShown) {
				GridScreenLook.DrawFocusedRow(g,rectangleRowInside,scale);
			}
		}

		private void DrawTitle(Graphics g) {
			if(_heightTitle==0) {
				return;
			}
			//Flat Accent with white text (UI-5); the font is the title token's.
			GridScreenLook.DrawTitle(g,Width,ScaleI(_heightTitle),_title,_fontTitle,isDesignMode:false);
			if(!HasAddButton) {
				return;
			}
			//Everything from here down is AddButton
			AddButtonWidth=GridScreenLook.DrawAddButton(g,Width,ScaleI(_heightTitle),_addButtonEnabled);
		}

		private void DrawHeaders(Graphics g) {
			if(_heightHeader==0) {
				return;
			}
			//SurfaceSubtle with Text, 1 px Stroke column separators and bottom line (UI-5). The title's Accent ends at its own bottom edge, so
			//there is no line between title and headers.
			GridScreenLook.DrawHeaderBackground(g,Width,ScaleI(_heightTitle)+1,ScaleI(_heightHeader));
			StringFormat stringFormat=new StringFormat();
			stringFormat.Alignment=StringAlignment.Center;
			stringFormat.LineAlignment=StringAlignment.Center;
			for(int i=0;i<Columns.Count;i++) {
				if(i!=0) {
					//vertical lines to right of column header
					GridScreenLook.DrawHeaderSeparator(g,-hScroll.Value+Columns[i].State.XPos,ScaleI(_heightTitle)+1,OriginY());
				}
				RectangleF rect=new RectangleF(Columns[i].State.XPos-hScroll.Value,ScaleI(_heightTitle)+1,
					Columns[i].State.Width,ScaleI(_heightHeader));
				g.DrawString(Columns[i].Heading,_fontHeader,UiTheme.GetBrush(EnumUiColor.Text),rect,stringFormat);
			}
			for(int i=0;i<Columns.Count;i++) {
				if(_sortedByColumnIdx == i) {
					PointF p=new PointF(-hScroll.Value+Columns[i].State.XPos+6,ScaleF(_heightTitle+_heightHeader/2f));
					GridScreenLook.DrawSortArrow(g,p,_isSortedAscending);
				}//if
			}//for
			stringFormat.Dispose();
			//line below headers
			GridScreenLook.DrawHeaderBottomLine(g,Width,OriginY());
		}

		///<summary>Draws outline around entire control: 1 px Stroke (UI-5).</summary>
		private void DrawOutline(Graphics g) {
			if(hScroll.Visible) {//for the little square at the lower right between the two scrollbars
				using(SolidBrush brushCorner=new SolidBrush(Color.FromKnownColor(KnownColor.Control))) {
					g.FillRectangle(brushCorner,Width-vScroll.Width-1,Height-hScroll.Height-1,vScroll.Width,hScroll.Height);
				}
			}
			GridScreenLook.DrawOutline(g,Width,Height,_scaleMS*_zoomLocal);
		}

		///<summary>True while the grid or a control in it (an edit box, a combo box) has focus: the selection bar is Accent then, SelectionBarNoFocus
		///otherwise (section 3.2). The arrows that come through the parent form without focus (ParentForm_KeyDown) keep the unfocused bar.</summary>
		protected virtual bool IsSelectionFocused {
			get { return Focused || ContainsFocus; }
		}

		///<summary>True while the grid has focus and Windows shows keyboard cues: the focused row gets its Accent outline then (section 3.6), so a
		///mouse click draws no outline.</summary>
		protected virtual bool IsFocusedRowShown {
			get { return IsSelectionFocused && ShowFocusCues; }
		}

		///<summary>The focused row, the one the arrow keys move (section 3.6). OneRow: the selected row, since the arrows move from it, so a
		///SetSelected, SetAll or a sort moves the outline with the selection. The multi-select modes: the row the arrow keys or the last click moved
		///to, if it is still a row; else the first selected row. -1 for none.</summary>
		private int GetIndexRowFocused() {
			if(_selectionMode==GridSelectionMode.OneCell || _selectionMode==GridSelectionMode.None) {
				return -1;
			}
			if(_selectionMode==GridSelectionMode.OneRow) {
				if(_listSelectedIndices.Count>0 && _listSelectedIndices[0]>=0 && _listSelectedIndices[0]<ListGridRows.Count) {
					return _listSelectedIndices[0];
				}
				return -1;
			}
			if(_indexRowFocused>=0 && _indexRowFocused<ListGridRows.Count) {
				return _indexRowFocused;
			}
			if(_listSelectedIndices.Count>0) {
				return _listSelectedIndices[0];
			}
			return -1;
		}
		#endregion Methods - Event Handlers - Painting

		#region Methods - Event Handlers
		//Focus changes the selection bar (Accent with focus, SelectionBarNoFocus without) and the focused row's outline (UI-5), so the grid
		//repaints when it or a control in it (an edit box, a combo box) gets or loses focus.
		protected override void OnEnter(EventArgs e) {
			base.OnEnter(e);
			Invalidate();
		}

		protected override void OnLeave(EventArgs e) {
			base.OnLeave(e);
			Invalidate();
		}

		protected override void OnGotFocus(EventArgs e) {
			base.OnGotFocus(e);
			Invalidate();
		}

		protected override void OnLostFocus(EventArgs e) {
			base.OnLostFocus(e);
			Invalidate();
		}

		protected override void OnFontChanged(EventArgs e) {
			base.OnFontChanged(e);
			//irrelevant since we ignore the actual font and just use scale.
		}

		protected override void OnLayout(LayoutEventArgs e){
			base.OnLayout(e);
		}

		///<summary></summary>
		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
			//OnSizeChanged(new EventArgs());//this just causes some sort of infinite loop that crashes VS.
			//In fact, putting other layout code here also tends to crash VS, which I can find no cause for.
		}

		protected override void OnSizeChanged(EventArgs e) {
			base.OnSizeChanged(e);
			if(_isUpdating){
				return;
			}
			float scrollFraction=1;
			if(vScroll.Maximum-vScroll.LargeChange!=0) {
				scrollFraction=(float)vScroll.Value/(vScroll.Maximum-vScroll.LargeChange);
			}
			ComputeColumns();
			//ComputeRows();//not needed because nothing changes
			LayoutScrolls();
			ScrollValue=(int)(scrollFraction*(vScroll.Maximum-vScroll.LargeChange));
		}

		///<summary></summary>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			if(this.ParentForm!=null) {
				this.ParentForm.KeyDown+=new KeyEventHandler(ParentForm_KeyDown);
			}
		}

		///<summary></summary>
		protected override void OnDoubleClick(EventArgs e) {
			base.OnDoubleClick(e);
			//_mouseDownRow could be .5s stale and ListGridRow might have changed. 
			int row=PointToRow(((MouseEventArgs)e).Y);//test based on current ListGridRows
			if(row==-1) {
				return;//double click was in the title or header section
			}
			if(_mouseDownCol==-1) {
				return;//click was to the right of the columns
			}
			_mouseDownRow=row;
			CellDoubleClick?.Invoke(this, new ODGridClickEventArgs(_mouseDownCol,_mouseDownRow,MouseButtons.Left));
		}

		///<summary></summary>
		protected override void OnClick(EventArgs e) {
			base.OnClick(e);
			//todo: consider mouse down.
			if(HasAddButton){ 
				if(((MouseEventArgs)e).X>=Width-ScaleI(_heightTitle)-5 && ((MouseEventArgs)e).Y<=ScaleI(_heightTitle)){
					TitleAddClick?.Invoke(this,new EventArgs());
				}
			}
			if(_hasDropDowns
				&& _mouseDownRow>-1 && _mouseDownCol==0 && ListGridRows[_mouseDownRow].State.DropDownState!=ODGridDropDownState.None
				/*&& ((MouseEventArgs)(e)).X < 10*///uncomment if the user should have to click the DropDown Triangle to for the row to drop down.
				&& !ControlIsDown() //if the user is trying to select the row, don't drop it down.
				&& !ShiftIsDown())
			{
				DropDownRowClick(_mouseDownRow,((MouseEventArgs)(e)).Y);
				return;
			}
			GridRow rowClicked;
			if(_mouseDownRow.Between(0,ListGridRows.Count-1)) {
				rowClicked=ListGridRows[_mouseDownRow];
			}
			else {
				rowClicked=null;
			}
			GridColumn colClicked;
			if(_mouseDownCol.Between(0, Columns.Count-1)) {
				colClicked=Columns[_mouseDownCol];
			}
			else {
				colClicked=null;
			}
			if(_mouseDownRow>-1 && _mouseDownCol>-1 //on a row and not to the side of the columns.
				//Make sure that the row/column selected on mouse down is still the same as it is now. This may not be the case if columns
				//was changed after mouse down but before mouse up.
				&& rowClicked==_gridRowMouseDown && colClicked==_gridColumnMouseDown)
			{
				CellClick?.Invoke(this, new ODGridClickEventArgs(_mouseDownCol,_mouseDownRow,_mouseButtonLastPressed));
			}
		}

		protected override void ScaleControl(SizeF factor, BoundsSpecified specified){
			base.ScaleControl(factor, specified);
			//we must leave grid scaling in place.  Otherwise, grids throughout the program won't resize when forms resize due to dpi changes.
		}
		#endregion Methods - Event Handlers

		#region Methods - Event Handlers - Mouse
		///<summary>The grid's own menu, and a caller's ContextMenuStrip when DoAddGridItemsToContextMenuStrip is true, get the grid items when they open.</summary>
		protected override void OnContextMenuStripChanged(EventArgs e) {
			base.OnContextMenuStripChanged(e);
			HookContextMenuStrip();
		}

		///<summary>Subscribes the ContextMenuStrip to ContextMenuStrip_Opening once, if it is the grid's own or DoAddGridItemsToContextMenuStrip is true,
		///and unsubscribes the strip it was subscribed to before.</summary>
		private void HookContextMenuStrip() {
			if(_contextMenuStripHooked!=null) {
				_contextMenuStripHooked.Opening-=ContextMenuStrip_Opening;
				_contextMenuStripHooked=null;
			}
			if(ContextMenuStrip==null) {
				return;
			}
			ContextMenuStrip.Opening-=ContextMenuStrip_Opening;
			if(ContextMenuStrip==_contextMenuStripGrid || DoAddGridItemsToContextMenuStrip) {
				ContextMenuStrip.Opening+=ContextMenuStrip_Opening;
				_contextMenuStripHooked=ContextMenuStrip;
			}
		}

		///<summary>Just prior to displaying the context menu strip, add the copy items and the wiki links if neccesary.</summary>
		private void ContextMenuStrip_Opening(object sender,CancelEventArgs e) {
			//If multiple grids add the same instance of ContextMenuStrip, then all of them will raise this event any time any of them raise the event.
			//Only allow the event to operate if this is the grid that actually fired the event.
			try {
				if(((ContextMenuStrip)sender).SourceControl.Name != this.Name) {
					return;
				}
			}
			catch(Exception ex) {
				ex.DoNothing();
				return;
			}
			if(this.ContextMenuStrip==null) {
				return;
			}
			//A ContextMenuStrip with no items that will show starts its Opening cancelled (ToolStripDropDown uses DisplayedItems, which leaves out the
			//items that are not Available). The grid's own menu is empty until the first time it opens, and a caller's strip may have only hidden items
			//(ControlFamily's "View SS#", which its own Opening makes visible after this one).
			bool hadNoShownItems=!this.ContextMenuStrip.Items.Cast<ToolStripItem>().Any(x => x.Available);
			AddGridItemsToContextMenuStrip();
			if(hadNoShownItems && this.ContextMenuStrip.Items.Cast<ToolStripItem>().Any(x => x.Available)) {
				e.Cancel=false;
			}
		}

		///<summary>Adds Copy Cell Text (the first time, after a separator if the menu has items), sets it and Copy Rows enabled for the cell and row
		///that were clicked, and adds the links. The same items, order and states as the WinForms ContextMenu had.</summary>
		private void AddGridItemsToContextMenuStrip() {
			//Todo: this is a bad selector:
			ToolStripMenuItem toolStripMenuItemCopy=this.ContextMenuStrip.Items.OfType<ToolStripMenuItem>().FirstOrDefault(x => x.Text == "Copy Cell Text");
			List<ToolStripItem> listToolStripItems=new List<ToolStripItem>();
			if(toolStripMenuItemCopy==null) {//Add it the first time
				toolStripMenuItemCopy=new ToolStripMenuItem("Copy Cell Text",null,CopyCellClick);
				if(this.ContextMenuStrip.Items.Count > 0) {
					listToolStripItems.Add(new ToolStripSeparator());
				}
				listToolStripItems.Add(toolStripMenuItemCopy);
				listToolStripItems.ForEach(x => this.ContextMenuStrip.Items.Add(x));
			}
			if(!(0<=_mouseDownRow && _mouseDownRow<=ListGridRows.Count-1)
				|| !(0<=_mouseDownCol && _mouseDownCol<=ListGridRows[_mouseDownRow].Cells.Count-1)
				|| string.IsNullOrEmpty(ListGridRows[_mouseDownRow].Cells[_mouseDownCol].Text))
			{
				toolStripMenuItemCopy.Enabled = false;
			}
			else {
				toolStripMenuItemCopy.Enabled = true;
			}
			//Copy Rows----------------------------------------------------
			ToolStripMenuItem toolStripMenuItemCopyRows=this.ContextMenuStrip.Items.OfType<ToolStripMenuItem>().FirstOrDefault(x => x.Text == "Copy Rows");
			if(toolStripMenuItemCopyRows==null) {//Add it the first time
				toolStripMenuItemCopyRows=new ToolStripMenuItem("Copy Rows",null,CopyRowsClick);
				this.ContextMenuStrip.Items.Add(toolStripMenuItemCopyRows);
			}
			if(!_listSelectedIndices.Contains(_mouseDownRow)//If clicked row is not selected.
				|| !(0<=_mouseDownCol && _mouseDownCol<=ListGridRows[_mouseDownRow].Cells.Count-1))//If clicked column is not in the row's cells.
			{
				toolStripMenuItemCopyRows.Enabled = false;
			}
			else {
				toolStripMenuItemCopyRows.Enabled = true;
			}
			//Link items----------------------------------------------------
			if(HasLinkDetect) {//Links go below any "Copy Text" menu items.
				ContextMenuStripAddPopupLinks();
			}
		}

		///<summary>Just prior to displaying the context menu strip, add wiki links if neccesary. Links of the row that was right-clicked.</summary>
		private void ContextMenuStripAddPopupLinks() {
			if(ContextMenuStrip!=null && _listToolStripItemsLinks!=null) {
			//Removes all previous wiki and web links from context menu.
				for(int i=_listToolStripItemsLinks.Count-1;i>=0;i--) {//backward because removing
					ContextMenuStrip.Items.Remove(_listToolStripItemsLinks[i]);
				}
			}
			int rowClick = PointToRow(_mouseClickLocation.Y);
			int colClick = PointToCol(_mouseClickLocation.X);
			if(rowClick<0 || colClick<0) {//don't diplay links, not on grid row.
				return;
			}
			if(_mouseButtonLastPressed==MouseButtons.Right && rowClick<=this.ListGridRows.Count) {
				GridRow gridRow = this.ListGridRows[rowClick];
				_listToolStripItemsLinks=new List<ToolStripItem>();
				if(this.ContextMenuStrip.Items.Count>0) {
					_listToolStripItemsLinks.Add(new ToolStripSeparator());
				}
				StringBuilder stringBuilder = new StringBuilder();
				gridRow.Cells.OfType<GridCell>().ToList().ForEach(x => stringBuilder.AppendLine(x.Text));
				stringBuilder.AppendLine(gridRow.Note);
				_listToolStripItemsLinks.AddRange(OpenPmsDentalBusiness.UI.PopupHelper.GetToolStripMenuItemLinks(stringBuilder.ToString(),DoShowRightClickLinks));
				if(_listToolStripItemsLinks.Any(x => !(x is ToolStripSeparator))) {//at least one REAL menu item that is not the divider.
					_listToolStripItemsLinks.ForEach(x => this.ContextMenuStrip.Items.Add(x));
				}
			}
		}

		private void CopyCellClick(object sender,EventArgs e) {
			try {
				string copyText = ListGridRows[_mouseDownRow].Cells[_mouseDownCol].Text;
				ODClipboard.SetClipboard(copyText);
			}
			catch {
				//show a message box?
			}
		}

		/// <summary>Copies selected rows to clipboard in tab-delimited format</summary>
		private void CopyRowsClick(object sender,EventArgs e) {
			List<GridRow> listGridRowsSelected=new List<GridRow>();
			_listSelectedIndices.ForEach(x => listGridRowsSelected.Add(ListGridRows[x]));
			StringBuilder stringBuilder=new StringBuilder();
			for(int i=0;i<listGridRowsSelected.Count;i++) {
				string parsedRow="";
				for(int j=0;j<listGridRowsSelected[i].Cells.Count;j++) {
					if(!string.IsNullOrEmpty(listGridRowsSelected[i].Cells[j].Text)) {
						parsedRow+=listGridRowsSelected[i].Cells[j].Text.Replace("\t","    "); //remove tabs in string so we keep each column separated by tabs
					}
					if(j!=listGridRowsSelected[i].Cells.Count-1) {
						parsedRow+="\t";
					}
				}
				stringBuilder.AppendLine(parsedRow);
			}
			try {
				ODClipboard.SetClipboard(stringBuilder.ToString());
			}
			catch { 
				//show a message box?
			}
		}

		///<summary></summary>
		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
			Focus();
			_mouseButtonLastPressed=e.Button;//used in the click event.
			_mouseClickLocation=e.Location;//stored for later use during context menu display
			_mouseDownRow=PointToRow(e.Y);
			_mouseDownCol=PointToCol(e.X);
			if(_mouseDownRow.Between(0,ListGridRows.Count-1)) {
				_gridRowMouseDown=ListGridRows[_mouseDownRow];
				_indexRowFocused=_mouseDownRow;//the focused row's outline follows the click (UI-5)
			}
			else {
				_gridRowMouseDown=null;
			}
			if(_mouseDownCol.Between(0, Columns.Count-1)) {
				_gridColumnMouseDown = Columns[_mouseDownCol];
			}
			else {
				_gridColumnMouseDown = null;
			}
			if(e.Button==MouseButtons.Right) {
				if(_listSelectedIndices.Contains(_mouseDownRow)) {//If a currently selected row is clicked, then ignore right click.
					return;
				}
				//otherwise, row will be selected. Useful when using context menu.
			}
			_isMouseDown=true;
			if(e.Y < 1+ScaleI(_heightTitle)) {//mouse down was in the title section
				return;
			}
			if(e.Y < 1+OriginY()) {//mouse down was on a column header
				_isMouseDownInHeader=true;
				if(_mouseDownCol!=-1 && Columns[_mouseDownCol].HeaderClick!=null) {
					Columns[_mouseDownCol].HeaderClick(null,null);
					return;
				}
				else if(AllowSortingByColumn) {
					if(_mouseDownCol==-1) {
						return;
					}
					SortByColumn(_mouseDownCol);
					Invalidate();
					return;
				}
				else {
					return;
				}
			}
			if(_mouseDownRow==-1) {//mouse down was below the grid rows
				return;
			}
			if(_mouseDownCol==-1) {//mouse down was to the right of columns
				return;
			}
			if(!AllowSelection) {
				return;//clicks do not trigger selection of rows, but cell click event still gets fired
			}
			switch(_selectionMode) {
				case GridSelectionMode.None:
					return;
				case GridSelectionMode.OneRow:
					_listSelectedIndices.Clear();
					_listSelectedIndices.Add(_mouseDownRow);
					SelectionCommitted?.Invoke(this,new EventArgs());
					if(_mouseDownRow>ListGridRows.Count-1){
						return;
					}
					if(_mouseDownCol>ListGridRows[_mouseDownRow].Cells.Count-1){
						return;//this can happen if programmer forgot to add some cells to a row.
					}
					GridCell cell=ListGridRows[_mouseDownRow].Cells[_mouseDownCol];
					if(cell.IsButton) {
						cell.ButtonIsPressed=true;
						Refresh(); //Force the "button" styling to repaint in the "clicked" style
						cell.ClickEvent.Invoke(this,new EventArgs());
						cell.ButtonIsPressed=false;
					}
					break;
				case GridSelectionMode.OneCell:
					//The current grid could have another control floating on top of it (edit box, combo box, etc) which may require a LostFocus event to fire.
					//Removing focus will trigger a CellLeave event.
					//Grid is not a containerControl, so setting focus to itself should remove focus from others.
					//this.ActiveControl=null;
					//Focus();//probably redundant.  Moved to top of this method
					//Debug.WriteLine("Click");
					_listSelectedIndices.Clear();
					textEdit?.Dispose();//a lot happens right here, including a FillGrid() which sets selectedCell to -1,-1
//todo: I think the comment above is wrong.  Nothing happens here because textEdit is always null before here.
//LostFocus happens before click.  That's where textEdit is cleared.
					comboBox.Visible=false;
					_selectedCell=new Point(_mouseDownCol,_mouseDownRow);
					if(IsCellEditBox(_selectedCell.X,_selectedCell.Y) || IsCellComboBox(_selectedCell.X,_selectedCell.Y)) {
						if(IsCellEditBox(_selectedCell.X,_selectedCell.Y)) {
							CreateEditBox();
						}
						else {
							CreateComboBox();
						}
						//When the additional control is created, added to the controls, and given focus, the chain of events stops and the OnClick event never gets fired.
						//Manually fire the OnClick event because we can guarantee that the user did in fact click on a cell at this point in the mouse down event.
						OnClick(e);
					}
					if(_mouseDownRow>ListGridRows.Count-1){
						return;
					}
					if(_mouseDownCol>ListGridRows[_mouseDownRow].Cells.Count-1){
						return;//this can happen if programmer forgot to add some cells to a row.
					}
					cell=ListGridRows[_mouseDownRow].Cells[_mouseDownCol];
					if(cell.IsButton) {
						cell.ButtonIsPressed=true;
						Refresh(); //Force the "button" styling to repaint in the "clicked" style
						cell.ClickEvent.Invoke(this,new EventArgs());
						cell.ButtonIsPressed=false;
					}
					break;
				case GridSelectionMode.MultiExtended:
					if(ControlIsDown()) {
						//we need to remember exactly which rows were selected the moment the mouse down started.
						//Then, if the mouse gets dragged up or down, the rows between mouse start and mouse end
						//will be set to the opposite of these remembered values.
						_listSelectedIndicesWhenMouseDown=new List<int>(_listSelectedIndices);
						if(_listSelectedIndices.Contains(_mouseDownRow)) {
							_listSelectedIndices.Remove(_mouseDownRow);
						}
						else {
							_listSelectedIndices.Add(_mouseDownRow);
						}
					}
					else if(ShiftIsDown()) {
						if(_listSelectedIndices.Count==0) {
							_listSelectedIndices.Add(_mouseDownRow);
						}
						else {
							int fromRow=_listSelectedIndices[0];
							_listSelectedIndices.Clear();
							if(_mouseDownRow<fromRow) {//dragging down
								for(int i=_mouseDownRow;i<=fromRow;i++) {
									_listSelectedIndices.Add(i);
								}
							}
							else {
								for(int i=fromRow;i<=_mouseDownRow;i++) {
									_listSelectedIndices.Add(i);
								}
							}
						}
					}
					else {//ctrl or shift not down
						_listSelectedIndices.Clear();
						_listSelectedIndices.Add(_mouseDownRow);
					}
					break;
			}
			Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e) {
			base.OnMouseLeave(e);
			_hoverRow=-1;
			Invalidate();
		}

		///<summary>The purpose of this is to allow dragging to select multiple rows.  Only makes sense if selectionMode==MultiExtended.  Doesn't matter whether ctrl is down, because that only affects the mouse down event.</summary>
		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			if(!_isMouseDown) {
				_hoverRow=PointToRow(e.Y);
				Invalidate();
				return;
			}
			//From here down, mouse is down
			if(_selectionMode!=GridSelectionMode.MultiExtended) {
				return;
			}
			if(!AllowSelection) {
				return;//dragging does not change selection of rows
			}
			if(_isMouseDownInHeader) {
				return;//started drag in header, so not allowed to select anything.
			}
			int curRow=PointToRow(e.Y);
			if(curRow==-1 || curRow==_mouseDownRow) {
				return;
			}
			//because mouse might have moved faster than computer could keep up, we have to loop through all rows between
			//todo: the comment on this method is inconsistent with the code below
			if(ControlIsDown()) {
				if(_listSelectedIndicesWhenMouseDown==null) {
					_listSelectedIndices=new List<int>();
				}
				else {
					_listSelectedIndices=new List<int>(_listSelectedIndicesWhenMouseDown);
				}
			}
			else {
				_listSelectedIndices=new List<int>();
			}
			if(_mouseDownRow<curRow) {//dragging down
				for(int i=_mouseDownRow;i<=curRow;i++) {
					if(i==-1) {
						continue;
					}
					if(_listSelectedIndices.Contains(i)) {
						_listSelectedIndices.Remove(i);
					}
					else {
						_listSelectedIndices.Add(i);
					}
				}
			}
			else {//dragging up
				for(int i=curRow;i<=_mouseDownRow;i++) {
					if(_listSelectedIndices.Contains(i)) {
						_listSelectedIndices.Remove(i);
					}
					else {
						_listSelectedIndices.Add(i);
					}
				}
			}
			Invalidate();
		}

		///<summary></summary>
		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			//if(e.Button==MouseButtons.Right){
			//	return;
			//}		
			if(this.ContextMenuStrip==null && this.ShowContextMenu) {
				if(e.Button==MouseButtons.Right) {
					//The grid's own menu. Later right clicks show it as the grid's ContextMenuStrip.
					_contextMenuStripGrid?.Dispose();
					_contextMenuStripGrid=new ContextMenuStrip();
					this.ContextMenuStrip=_contextMenuStripGrid;//OnContextMenuStripChanged subscribes it to ContextMenuStrip_Opening.
					try{
						this.ContextMenuStrip.Show(this,_mouseClickLocation);//triggers autofill via the popup helper.
					}
					catch{ }//This can fail if user double clicks, causing form to close, and also right clicks on the second click.
				}
			}
			_isMouseDown=false;
			_isMouseDownInHeader=false;
		}
		
		void comboBox_GotFocus(object sender,EventArgs e) {
			CellEnter?.Invoke(this, new ODGridClickEventArgs(_selectedCell.X, _selectedCell.Y, MouseButtons.None));
		}

		void comboBox_LostFocus(object sender,EventArgs e) {
			CellLeave?.Invoke(this, new ODGridClickEventArgs(_selectedCellOld.X, _selectedCellOld.Y, MouseButtons.None));
			comboBox.Visible=false;
		}

		void comboBox_KeyDown(object sender,KeyEventArgs e) {
			if(e.KeyCode==Keys.Enter) {//usually move to the next cell
				// No Consideration for EditableAllowsCR because this is a ComboBox.
				if(EditableEnterMovesDown){
					// No need for Dispose() or null assignment because the ComboBox is happy to give focus to the next cell.
					if(_selectedCellOld.Y==ListGridRows.Count-1) {
						return;//can't move down
					}
					if(!IsCellComboBox(_selectedCellOld.X,_selectedCellOld.Y+1)) {
						return;//the cell below is read-only
					}
					_selectedCell=new Point(_selectedCellOld.X,_selectedCellOld.Y+1);
					CreateComboBox();
					return;
				}
				editBox_NextCellRight();
			}
			if(e.KeyCode==Keys.Tab) {
				editBox_NextCellRight();
			}
		}

		void comboBox_SelectionChangeCommitted(object sender,EventArgs e) {
			if(!comboBox.SelectedIndex.Between(0,comboBox.Items.Count-1)) {
				//Combobox loaded with no selection, user opens combobox, does not highlight any option with mouse, and types "Enter".
				return;//Since no selection has been made, there hasn't actually been a SelectionChangeCommitted.
			}
			ListGridRows[SelectedCell.Y].Cells[_selectedCell.X].Text=comboBox.Items[comboBox.SelectedIndex].ToString();
			ListGridRows[SelectedCell.Y].Cells[_selectedCell.X].ComboSelectedIndex=comboBox.SelectedIndex;
			CellSelectionCommitted?.Invoke(this, new ODGridClickEventArgs(SelectedCell.X,_selectedCell.Y,MouseButtons.Left));
		}

		///<summary>When selection mode is OneCell, and user clicks in a column that isEditable, then this edit box will appear.</summary>
		private void CreateEditBox() {
			int hScrollBarHeight=0;
			if(HScrollVisible) {
				hScrollBarHeight=SystemInformation.HorizontalScrollBarHeight;
			}
			//Check if new edit box location is below the display screen
			int editBoxLocationTop=-vScroll.Value+1+OriginY()+ListGridRows[_selectedCell.Y].State.YPos+ListGridRows[_selectedCell.Y].State.HeightMain+hScrollBarHeight;
			if(editBoxLocationTop > this.DisplayRectangle.Bottom) {
				int onScreenPixels=vScroll.Value+DisplayRectangle.Height-OriginY()-(ListGridRows[_selectedCell.Y].State.YPos)-hScrollBarHeight;
				int offScreenPixels=ListGridRows[_selectedCell.Y].State.HeightMain-onScreenPixels;
				if(offScreenPixels>0) {
					ScrollValue+=offScreenPixels;//Scrolling down
				}
			}
			else if(-vScroll.Value+1+OriginY()+ListGridRows[_selectedCell.Y].State.YPos<this.DisplayRectangle.Top+OriginY()) {
				//If new edit box location is above the display screen
				ScrollToIndex(_selectedCell.Y);//Scrolling up
			}
			bool hasVscroll=false;
			if(ListGridRows[_selectedCell.Y].State.HeightMain > DisplayRectangle.Height-OriginY()-hScrollBarHeight){
				hasVscroll=true;
			}
			if(EditableUsesRTF) {
				RichTextBox editRichBox=new RichTextBox();
				editRichBox.Multiline=true;
				editRichBox.BorderStyle=BorderStyle.FixedSingle;
				editRichBox.ScrollBars=RichTextBoxScrollBars.None;
				if(Columns[_selectedCell.X].TextAlign==HorizontalAlignment.Right) {
					editRichBox.SelectionAlignment=HorizontalAlignment.Right;
				}
				//Rich text boxes have strange borders (3D looking) and so we have to manipulate the size and location differently.
				editRichBox.Size=new Size(Columns[_selectedCell.X].State.Width-1,ListGridRows[_selectedCell.Y].State.HeightMain-1);
				editRichBox.Location=new Point(-hScroll.Value+Columns[_selectedCell.X].State.XPos+1,
					-vScroll.Value+OriginY()+ListGridRows[_selectedCell.Y].State.YPos+1);
				if(hasVscroll){
					editRichBox.ScrollBars=RichTextBoxScrollBars.Vertical;
					editRichBox.Height=DisplayRectangle.Height-OriginY()-hScrollBarHeight-1;
					editRichBox.Top=OriginY();
				}
				textEdit=editRichBox;
			}
			else {
				TextBox editTextBox=new TextBox();
				editTextBox.Multiline=true;
				if(Columns[_selectedCell.X].TextAlign==HorizontalAlignment.Right) {
					editTextBox.TextAlign=HorizontalAlignment.Right;
				}
				editTextBox.Size=new Size(Columns[_selectedCell.X].State.Width+1,ListGridRows[_selectedCell.Y].State.HeightMain+1);				
				editTextBox.Location=new Point(-hScroll.Value+Columns[_selectedCell.X].State.XPos,
					-vScroll.Value+OriginY()+ListGridRows[_selectedCell.Y].State.YPos);
				if(hasVscroll){
					editTextBox.ScrollBars=ScrollBars.Vertical;
					editTextBox.Height=DisplayRectangle.Height-OriginY()-hScrollBarHeight;
					editTextBox.Top=OriginY();
				}
				textEdit=editTextBox;
			}
			//If the cell's color is set manually, that color will also show up for this EditBox.
			textEdit.BackColor=ListGridRows[_selectedCell.Y].ColorBackG;
			textEdit.Font=new Font(_fontCell.FontFamily,ScaleMS(_fontCell.Size),_fontCell.Style);
			//As far as I can tell, MS RichTextBox seems to not fully support dpi scaling. 
			//Specifically, if this textbox content is right aligned, the scale is off at high dpi.
			//Not a huge deal, as this is a rare situation.  Did not see this reported anywhere on web.
			//There's probably a workaround by setting rtf or something.
			textEdit.Text=ListGridRows[_selectedCell.Y].Cells[_selectedCell.X].Text;
			textEdit.TextChanged+=new EventHandler(textEdit_TextChanged);
			textEdit.GotFocus+=new EventHandler(textEdit_GotFocus);
			textEdit.LostFocus+=new EventHandler(textEdit_LostFocus);
			textEdit.KeyDown+=new KeyEventHandler(textEdit_KeyDown);
			textEdit.KeyUp+=new KeyEventHandler(textEdit_KeyUp);
			textEdit.AcceptsTab=true;
			this.Controls.Add(textEdit);
			if(EditableUsesRTF) {
				//RichTextBox always allows return
				if(!EditableAcceptsCR) {
					textEdit.SelectAll();//Only select all when not multiline (editableAcceptsCR) i.e. proc list for editing fees selects all for easy overwriting.
				}
			}
			else {
				if(EditableAcceptsCR) {//Allow the edit box to handle carriage returns/multiline text.
					((TextBox)textEdit).AcceptsReturn=true;
				}
				else {
					textEdit.SelectAll();//Only select all when not multiline (editableAcceptsCR) i.e. proc list for editing fees selects all for easy overwriting.
				}
			}
			//Set the cell of the current editBox so that the value of that cell is saved when it loses focus (used for mouse click).
			_selectedCellOld=new Point(_selectedCell.X,_selectedCell.Y);
			textEdit.Focus();
		}

		void textEdit_LostFocus(object sender,EventArgs e) {
			Debug.WriteLine("Lost Focus.");
			//editBox_Leave wouldn't catch all scenarios
			CellLeave?.Invoke(this, new ODGridClickEventArgs(_selectedCellOld.X, _selectedCellOld.Y, MouseButtons.None));
			if(textEdit!=null && (!textEdit.Disposing || !textEdit.IsDisposed)) {
				textEdit.Dispose();
				textEdit=null;
				using Graphics g=this.CreateGraphics();
				if(ListGridRows.Count>_selectedCellOld.Y) {
					//In FormQueryParser, SetFilterControlsAndAction uses an action that calls FillGrid, but number of rows can be different than before.
					//This is a problem when use is editing a cell and clicks Tab.  That should really be fixed, but this makes the grid more resilient.
					//This also happens in FormPayPlanDynamic after unchecking checkShowAttachedP&I while a text cell is selected.
					ComputeRowHeightOne(g,_selectedCellOld.Y);
					ComputeRowYposStartingAt(_selectedCellOld.Y);
				}
				LayoutScrolls();
			}
		}

		void textEdit_GotFocus(object sender,EventArgs e) {
			CellEnter?.Invoke(this, new ODGridClickEventArgs(_selectedCellOld.X, _selectedCellOld.Y, MouseButtons.None));
		}

		void textEdit_KeyDown(object sender,KeyEventArgs e) {
			CellKeyDown?.Invoke(this, new ODGridKeyEventArgs(e));
			if(e.Handled) {
				return;
			}
			if(e.Shift && e.KeyCode == Keys.Enter) {
				ListGridRows[_selectedCell.Y].Cells[_selectedCell.X].Text+="\r\n";
				return;
			}
			if(e.KeyCode==Keys.Enter) {//usually move to the next cell
				if(EditableAcceptsCR) {//When multiline it inserts a carriage return instead of moving to the next cell.
					return;
				}
				if(EditableEnterMovesDown){
					textEdit.Dispose();
					textEdit=null;
					if(_selectedCellOld.Y==ListGridRows.Count-1) {
						return;//can't move down
					}
					if(!IsCellEditBox(_selectedCellOld.X,_selectedCellOld.Y+1)) {
						return;//the cell below is read-only
					}
					_selectedCell=new Point(_selectedCellOld.X,_selectedCellOld.Y+1);
					CreateEditBox();
					return;
				}
				editBox_NextCellRight();
			}
			if(e.KeyCode==Keys.Down) {
				if(EditableAcceptsCR) {//When multiline it moves down inside the text instead of down to the next cell.
					return;
				}
				if(_selectedCellOld.Y<ListGridRows.Count-1 && IsCellEditBox(_selectedCellOld.X,_selectedCellOld.Y+1)) {
					textEdit.Dispose();
					textEdit=null;
					_selectedCell=new Point(_selectedCellOld.X,_selectedCellOld.Y+1);
					CreateEditBox();
				}
			}
			if(e.KeyCode==Keys.Up) {
				if(EditableAcceptsCR) {//When multiline it moves up inside the text instead of up to the next cell.
					return;
				}
				if(_selectedCellOld.Y>0 && IsCellEditBox(_selectedCellOld.X,_selectedCellOld.Y-1)) {
					textEdit.Dispose();
					textEdit=null;
					_selectedCell=new Point(_selectedCellOld.X,_selectedCellOld.Y-1);
					CreateEditBox();
				}
			}
			if(e.KeyCode==Keys.Tab) {
				editBox_NextCellRight();
			}
		}
		
		private void editBox_NextCellRight() {
			textEdit?.Dispose();//This fires editBox_LostFocus, which is where we call CellLeave.
			textEdit=null;
			Focus();//Fires comboBox_LostFocus, which is where we call CellLeave.
			//find the next editable cell to the right.
			for(int i= _selectedCellOld.X+1; i < Columns.Count; i++) {
				if(IsCellEditBox(i,_selectedCellOld.Y)){//textbox
					_selectedCell=new Point(i,_selectedCellOld.Y);
					CreateEditBox();
					return;
				}
				if(IsCellComboBox(i,_selectedCellOld.Y)) {//dropdown
					_selectedCell=new Point(i,_selectedCellOld.Y);
					CreateComboBox();
					return;
				}
			}
			//can't move to the right, so attempt to move down. Rows whose cells are all read-only are skipped.
			for(int row=_selectedCellOld.Y+1;row<ListGridRows.Count;row++) {
				for(int i=0; i < Columns.Count; i++) {
					if(IsCellEditBox(i,row)) {
						_selectedCell=new Point(i,row);
						CreateEditBox();
						return;
					}
					if(IsCellComboBox(i,row)){
						_selectedCell=new Point(i,row);
						CreateComboBox();
						return;
					}
				}
			}
		}

		///<summary>True when the cell gets a text box when selected: its column is editable and FuncCellIsEditable, if set, allows it.</summary>
		private bool IsCellEditBox(int col,int row) {
			if(col<0 || col>=Columns.Count || !Columns[col].IsEditable) {
				return false;
			}
			if(FuncCellIsEditable==null || row<0 || row>=ListGridRows.Count) {
				return true;
			}
			return FuncCellIsEditable(col,row);
		}

		///<summary>True when the cell gets a combo box when selected: its column has ListDisplayStrings and is not a text column, and
		///FuncCellIsEditable, if set, allows it.</summary>
		private bool IsCellComboBox(int col,int row) {
			if(col<0 || col>=Columns.Count || Columns[col].IsEditable || Columns[col].ListDisplayStrings==null) {
				return false;
			}
			if(FuncCellIsEditable==null || row<0 || row>=ListGridRows.Count) {
				return true;
			}
			return FuncCellIsEditable(col,row);
		}
		
		void textEdit_KeyUp(object sender,KeyEventArgs e) {
			if(textEdit==null) {
				return;
			}
			if(textEdit.Text=="") {
				return;
			}
			if(textEdit is RichTextBox richTextBox){
				if(richTextBox.ScrollBars==RichTextBoxScrollBars.Vertical){
					return;
				}
			}
			else{
				if(((TextBox)textEdit).ScrollBars==ScrollBars.Vertical){
					return;
				}
			}
			Graphics g=CreateGraphics();
			int heightText=(int)(1.03f*g.MeasureString(textEdit.Text+"\r\n",_fontCell,textEdit.Width).Height);
			g.Dispose();
			if(heightText < HeightEditableRow()) {//if it's less than one line
			  heightText=HeightEditableRow();//set it to one line
			}
			if(heightText<=textEdit.Height+1) {
				return;
			}
			//it needs to grow
			int bottomVisible=Height-1;
			if(hScroll.Visible){
				bottomVisible-=hScroll.Height;
			}
			if(textEdit.Top==0 && textEdit.Bottom==bottomVisible){
				return;//already as tall as it can get
			}
			textEdit.Height+=HeightEditableRow();
			if(textEdit.Bottom<=bottomVisible){
				return;
			}
			textEdit.Top=bottomVisible-textEdit.Height;//this can hide upper portion of textbox
			if(textEdit.Top<0){
				textEdit.Top=0;
				textEdit.Height=bottomVisible;
				//if(textBoxEdit is TextBox textB){
				//	textB.ScrollBars=ScrollBars.Vertical;//this crashes
				//}
			}
		}

		void textEdit_TextChanged(object sender,EventArgs e) {
			if(textEdit!=null) {
				ListGridRows[_selectedCell.Y].Cells[_selectedCell.X].Text=textEdit.Text;
			}
			CellTextChanged?.Invoke(this, new EventArgs());
		}

		///<summary></summary>
		protected override void OnMouseWheel(MouseEventArgs e) {
			base.OnMouseWheel(e);
			ScrollValue-=e.Delta/3;
		}

		#endregion Methods - Event Handlers - Mouse

		#region Methods - Event Handlers - Key
		protected override void OnKeyDown(KeyEventArgs e) {
			if(_selectionMode!=GridSelectionMode.OneRow) {//row
				base.OnKeyDown(e);
				return;
			}
			//from here down, we are moving selected row up or down
			//If you want this to work when grid does not have focus, then the parent form needs to have KeyPreview turned on.
			//This is already the case for FormODBase
			if(e.KeyCode==Keys.Down) {
				if(_listSelectedIndices.Count>0 && _listSelectedIndices[0] < ListGridRows.Count-1) {
					int selectedNew=_listSelectedIndices[0]+1;
					_listSelectedIndices.Clear();
					_listSelectedIndices.Add(selectedNew);
					_indexRowFocused=selectedNew;
					ScrollToMakeVisible(selectedNew);
					SelectionCommitted?.Invoke(this,new EventArgs());
					hScroll.Value=hScroll.Minimum;
					Invalidate();
				}
			}
			if(e.KeyCode==Keys.Up) {
				if(_listSelectedIndices.Count>0 && _listSelectedIndices[0] > 0) {
					int selectedNew=_listSelectedIndices[0]-1;
					_listSelectedIndices.Clear();
					_listSelectedIndices.Add(selectedNew);
					_indexRowFocused=selectedNew;
					ScrollToMakeVisible(selectedNew);				
					SelectionCommitted?.Invoke(this,new EventArgs());
					Invalidate();
				}
			}
			base.OnKeyDown(e);
		}

		/// <summary>The grid might not have focus, so the up and down arrows can come through the parent form. The only thing you have to do to make it work is to turn on KeyPreview for the parent form.</summary>
		private void ParentForm_KeyDown(Object sender,KeyEventArgs e) {
			if(this.Focused){
				return;//avoid double call
			}
			if(ClientRectangle.Contains(PointToClient(Cursor.Position))){
				//the cursor is hovering over the grid, so it should respond to the up and down arrows
				OnKeyDown(e);
				return;
			}
			if(ArrowsWhenNoFocus){
				//Example, in FormPatientSelect, arrows will still work when focus is on the input boxes.
				OnKeyDown(e);
				return;
			}
		}
		#endregion Methods - Event Handlers - Key

		#region Methods - Sheets
		///<summary>Called from external Sheet   Presumably, the isPrinting distinguishes between printing sheets and displaying sheets.  Its only effect is to draw a slightly smaller font.</summary>
		public void SheetDrawRow(int rowI,Graphics g,int x=0,int y=0,bool isBottom=false,bool isSheetGrid=false,bool isPrintingSheet=false,float scaleMS=1) {
			Font fontLocal=_fontPrintCell;//no need to dispose. Just a ref.
			//Font fontLocalBold=_fontCellBold96;
			if(_fontForSheets!=null) {
				fontLocal=_fontForSheets;
				//fontLocalBold=new Font(_fontForSheets,FontStyle.Regular);
			}
			fontLocal=new Font(fontLocal.FontFamily,fontLocal.Size/scaleMS,fontLocal.Style);
			RectangleF textRect;
			SolidBrush textBrush;
			//selected row color
			if(_listSelectedIndices.Contains(rowI)) {
				g.FillRectangle(new SolidBrush(ColorSelectedRow),
					x+1,
					y-vScroll.Value+1,
					_widthTotal,
					ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote-1);
			}
			//colored row background
			else if(ListGridRows[rowI].ColorBackG!=Color.White) {
				g.FillRectangle(new SolidBrush(ListGridRows[rowI].ColorBackG),
					x+1,
					y-vScroll.Value+1,
					_widthTotal,
					ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote-1);
			}
			//normal row color
			else {//need to draw over the gray background
				g.FillRectangle(new SolidBrush(ListGridRows[rowI].ColorBackG),
					x+1,
					y-vScroll.Value+1,
					_widthTotal,//this is a really simple width value that always works well
					ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote-1);
			}
			if(_selectionMode==GridSelectionMode.OneCell && _selectedCell.X!=-1 && _selectedCell.Y!=-1
			&& _selectedCell.Y==rowI) {
				g.FillRectangle(new SolidBrush(ColorSelectedRow),
					x-hScroll.Value+1+Columns[_selectedCell.X].State.XPos,
					y-vScroll.Value+1,
					Columns[_selectedCell.X].State.Width,
					ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote-1);
			}
			//lines for note section
			if(ListGridRows[rowI].State.HeightNote>0) {
				//left vertical gridline
				if(NoteSpanStart!=0) {
					g.DrawLine(GridPrintLook.PenGridline,
						x-hScroll.Value+1+Columns[NoteSpanStart].State.XPos,
						y-vScroll.Value+1+ListGridRows[rowI].State.HeightMain,
						x-hScroll.Value+1+Columns[NoteSpanStart].State.XPos,
						y-vScroll.Value+1+ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote);
				}
				//Horizontal line which divides the main part of the row from the notes section of the row
				g.DrawLine(GridPrintLook.PenGridline,
					x-hScroll.Value+1+Columns[0].State.XPos+1,
					y-vScroll.Value+1+ListGridRows[rowI].State.HeightMain,
					x-hScroll.Value+1+Columns[Columns.Count-1].State.Right,
					y-vScroll.Value+1+ListGridRows[rowI].State.HeightMain);

			}
			Pen penLower=new Pen(GridPrintLook.PenGridline.Color);//disposed a few pages down
			if(rowI==ListGridRows.Count-1) {//last row
				penLower=new Pen(GridPrintLook.PenColumnSeparator.Color);
			}
			else {
				if(ListGridRows[rowI].ColorLborder!=Color.Empty) {
					penLower=new Pen(ListGridRows[rowI].ColorLborder);
				}
			}
			for(int i=0;i<Columns.Count;i++) {
				//right vertical gridline
				if(rowI == 0) {
					g.DrawLine(GridPrintLook.PenGridline,
						x- hScroll.Value+Columns[i].State.XPos + Columns[i].State.Width,
						y- vScroll.Value+1,
						x- hScroll.Value+Columns[i].State.XPos + Columns[i].State.Width,
						y- vScroll.Value+1+ ListGridRows[rowI].State.HeightMain);
				}
				else {
					g.DrawLine(GridPrintLook.PenGridline,
						x- hScroll.Value + Columns[i].State.XPos + Columns[i].State.Width,
						y- vScroll.Value+1,
						x- hScroll.Value + Columns[i].State.XPos + Columns[i].State.Width,
						y- vScroll.Value+1+ ListGridRows[rowI].State.HeightMain);
				}
				//lower horizontal gridline
				if(i == 0) {
					g.DrawLine(penLower,
						x- hScroll.Value + Columns[i].State.XPos,
						y- vScroll.Value+1+ ListGridRows[rowI].State.HeightMain+ ListGridRows[rowI].State.HeightNote,
						x- hScroll.Value + Columns[i].State.XPos + Columns[i].State.Width,
						y- vScroll.Value+1+ ListGridRows[rowI].State.HeightMain+ ListGridRows[rowI].State.HeightNote);
				}
				else {
					g.DrawLine(penLower,
						x- hScroll.Value + Columns[i].State.XPos +1,
						y- vScroll.Value+1+ ListGridRows[rowI].State.HeightMain+ ListGridRows[rowI].State.HeightNote,
						x- hScroll.Value + Columns[i].State.XPos + Columns[i].State.Width,
						y- vScroll.Value+1+ ListGridRows[rowI].State.HeightMain+ ListGridRows[rowI].State.HeightNote);
				}
				//text
				if(ListGridRows[rowI].Cells.Count-1< i) {
					continue;
				}
				switch(Columns[i].TextAlign) {
					case HorizontalAlignment.Left:
						_stringFormat.Alignment = StringAlignment.Near;
						break;
					case HorizontalAlignment.Center:
						_stringFormat.Alignment = StringAlignment.Center;
						break;
					case HorizontalAlignment.Right:
						_stringFormat.Alignment = StringAlignment.Far;
						break;
				}
				int vertical= y - vScroll.Value+1+1;
				int horizontal= x - hScroll.Value+1+ Columns[i].State.XPos;
				int cellW= Columns[i].State.Width;
				int cellH= ListGridRows[rowI].State.HeightMain;
				if(_hasEditableColumn) {//These cells are taller
					vertical+=2;//so this is to push text down to center it in the cell
					cellH-=3;//to keep it from spilling into the next cell
				}
				if(Columns[i].TextAlign== HorizontalAlignment.Right) {
					if(_hasEditableColumn) {
						horizontal-=4;
						cellW+=2;
					}
					else {
						horizontal-=2;
						cellW+=2;
					}
				}
				textRect=new RectangleF(horizontal, vertical, cellW, cellH);
				if(ListGridRows[rowI].Cells[i].ColorText== Color.Empty) {
					textBrush=new SolidBrush(ListGridRows[rowI].ColorText);
				}
				else {
					textBrush=new SolidBrush(ListGridRows[rowI].Cells[i].ColorText);
				}
				if(ListGridRows[rowI].Cells[i].Bold== YN.Yes) {
					fontLocal=new Font(fontLocal, FontStyle.Bold);
				}
				else if(ListGridRows[rowI].Cells[i].Bold== YN.No) {
					fontLocal=new Font(fontLocal, FontStyle.Regular);
				}
				else {//unknown.  Use row bold
					if(ListGridRows[rowI].Bold) {
						fontLocal=new Font(fontLocal, FontStyle.Bold);
					}
					else {
						fontLocal=new Font(fontLocal, FontStyle.Regular);
					}
				}
				if(ListGridRows[rowI].Cells[i].Underline== YN.Yes) {//Underline the current cell.  If it is already bold, make the cell bold and underlined.
					fontLocal=new Font(fontLocal, (fontLocal.Bold)?(FontStyle.Bold | FontStyle.Underline): FontStyle.Underline);
				}
				if(Columns[i].ImageList==null) {
					if(isPrintingSheet) {
						//Using a slightly smaller font because g.DrawString draws text slightly larger when using the printer's graphics
						Font smallerFont = new Font(fontLocal.FontFamily,(float)(fontLocal.Size*0.96), fontLocal.Style);
						g.DrawString(ListGridRows[rowI].Cells[i].Text, smallerFont, textBrush, textRect, _stringFormat);
						smallerFont.Dispose();
					}
					else {//Viewing the grid normally
						g.DrawString(ListGridRows[rowI].Cells[i].Text, fontLocal, textBrush, textRect, _stringFormat);
					}
				}
				else {
					int imageIndex=-1;
					if(ListGridRows[rowI].Cells[i].Text!="") {
						imageIndex= PIn.Int(ListGridRows[rowI].Cells[i].Text);
					}
					if(imageIndex != -1) {
						Image img =Columns[i].ImageList.Images[imageIndex];
						g.DrawImage(img, horizontal, vertical - 1);
					}
				}
			}
			penLower.Dispose();
			//note text
			if(ListGridRows[rowI].State.HeightNote>0 && NoteSpanStop > 0 && NoteSpanStart < Columns.Count) {
				int noteW=0;
				for(int i= NoteSpanStart; i <= NoteSpanStop; i++) {
					noteW+= Columns[i].State.Width;
				}
				if(ListGridRows[rowI].Bold) {
					fontLocal=new Font(fontLocal, FontStyle.Bold);
				}
				else {
					fontLocal=new Font(fontLocal, FontStyle.Regular);
				}
				textBrush=new SolidBrush(ListGridRows[rowI].ColorText);
				textRect=new RectangleF(
					x- hScroll.Value+1+ Columns[NoteSpanStart].State.XPos +1,
					y- vScroll.Value+1+ ListGridRows[rowI].State.HeightMain+1,
					Columns[NoteSpanStop].State.Right-Columns[NoteSpanStart].State.XPos,
					ListGridRows[rowI].State.HeightNote);
				_stringFormat.Alignment = StringAlignment.Near;
				g.DrawString(ListGridRows[rowI].Note, fontLocal, textBrush, textRect, _stringFormat);
			}
			//Left right and bottom lines of grid.  This creates the outline of the entire grid when not using outline control
			//Outline the Title
			Pen pen=GridPrintLook.PenOutline;//does not need to be disposed.
			if(isSheetGrid) {
				pen=Pens.Black;//System pen, does not need to be disposed.
			}
			//Draw line from LL to UL to UR to LR. top three sides of a rectangle.
			g.DrawLine(pen,x,y,x,y+ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote+1);//left side line
			g.DrawLine(pen,x+Width,y,x+Width,y+ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote+1);//right side line
			if(isBottom) {
				g.DrawLine(pen,x,y+ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote+1,x+Width,y+ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote+1);//bottom line.
			}
			fontLocal.Dispose();
		}

		///<summary></summary>
		public void SheetDrawRowX(int rowI,XGraphics g,int x=0,int y=0,bool isBottom=false,bool isSheetGrid=false) {
			XFont fontNormal=new XFont(_fontPrintCell.Name,_fontPrintCell.Size);//There is no Dispose for xfonts, or we would.
			XFont fontBold=new XFont(_fontPrintCellBold.Name,_fontPrintCellBold.Size,XFontStyleEx.Bold);
			if(_fontForSheets!=null) {
				fontNormal=new XFont(_fontForSheets.Name,_fontForSheets.Size);
				fontBold=new XFont(_fontForSheetsBold.Name,_fontForSheetsBold.Size,XFontStyleEx.Bold);;
			}
			XFont font;
			XRect textRect;
			XStringAlignment _xAlign=XStringAlignment.Near;
			XSolidBrush textBrush;
			//selected row color
			if(_listSelectedIndices.Contains(rowI)) {
				g.DrawRectangle(new XSolidBrush(ColorSelectedRow),
					ToPoints(x+1),
					ToPoints(y-vScroll.Value+1),
					ToPoints(_widthTotal),
					ToPoints(ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote-1));
			}
			//colored row background
			else if(ListGridRows[rowI].ColorBackG!=Color.White) {
				g.DrawRectangle(new XSolidBrush(ListGridRows[rowI].ColorBackG),
					ToPoints(x+1),
					ToPoints(y-vScroll.Value+1),
					ToPoints(_widthTotal),
					ToPoints(ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote-1));
			}
			//normal row color
			else {//need to draw over the gray background
				g.DrawRectangle(new XSolidBrush(ListGridRows[rowI].ColorBackG),
					ToPoints(x+1),
					ToPoints(y-vScroll.Value+1),
					ToPoints(_widthTotal),
					ToPoints(ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote-1));
			}
			if(_selectionMode==GridSelectionMode.OneCell && _selectedCell.X!=-1 && _selectedCell.Y!=-1
			&& _selectedCell.Y==rowI) {
				g.DrawRectangle(new XSolidBrush(ColorSelectedRow),
					ToPoints(x-hScroll.Value+1+Columns[_selectedCell.X].State.XPos),
					ToPoints(y-vScroll.Value+1),
					ToPoints(Columns[_selectedCell.X].State.Width),
					ToPoints(ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote-1));
			}
			XPen gridPen=new XPen(GridPrintLook.PenGridline);
			//lines for note section
			if(ListGridRows[rowI].State.HeightNote>0) {
				//left vertical gridline
				if(NoteSpanStart!=0) {
					g.DrawLine(gridPen,
						ToPoints(x-hScroll.Value+1+Columns[NoteSpanStart].State.XPos),
						ToPoints(y-vScroll.Value+1+ListGridRows[rowI].State.HeightMain),
						ToPoints(x-hScroll.Value+1+Columns[NoteSpanStart].State.XPos),
						ToPoints(y-vScroll.Value+1+ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote));
				}
				//Horizontal line which divides the main part of the row from the notes section of the row
				g.DrawLine(gridPen,
					ToPoints(x-hScroll.Value+1+Columns[0].State.XPos+1),
					ToPoints(y-vScroll.Value+1+ListGridRows[rowI].State.HeightMain),
					ToPoints(x-hScroll.Value+1+Columns[Columns.Count-1].State.Right),
					ToPoints(y-vScroll.Value+1+ListGridRows[rowI].State.HeightMain));
			}
			XPen xPenLower=new XPen(GridPrintLook.PenGridline);
			if(rowI==ListGridRows.Count-1) {//last row
				xPenLower=new XPen(XColor.FromArgb(GridPrintLook.PenColumnSeparator.Color.ToArgb()));
			}
			else {
				if(ListGridRows[rowI].ColorLborder!=Color.Empty) {
					xPenLower=new XPen(ListGridRows[rowI].ColorLborder);
				}
			}
			for(int i=0;i<Columns.Count;i++) {
				//right vertical gridline
				if(rowI == 0) {
					g.DrawLine(gridPen,
						ToPoints(x-hScroll.Value+Columns[i].State.Right),
						ToPoints(y-vScroll.Value+1),
						ToPoints(x-hScroll.Value+Columns[i].State.Right),
						ToPoints(y-vScroll.Value+1+ListGridRows[rowI].State.HeightMain));
				}
				else {
					g.DrawLine(gridPen,
						ToPoints(x-hScroll.Value+Columns[i].State.Right),
						ToPoints(y-vScroll.Value+1),
						ToPoints(x-hScroll.Value+Columns[i].State.Right),
						ToPoints(y-vScroll.Value+1+ ListGridRows[rowI].State.HeightMain));
				}
				//lower horizontal gridline
				if(i == 0) {
					g.DrawLine(xPenLower,
						ToPoints(x-hScroll.Value+Columns[i].State.XPos),
						ToPoints(y-vScroll.Value+1+ListGridRows[rowI].State.HeightMain+ ListGridRows[rowI].State.HeightNote),
						ToPoints(x-hScroll.Value+Columns[i].State.Right),
						ToPoints(y-vScroll.Value+1+ListGridRows[rowI].State.HeightMain+ ListGridRows[rowI].State.HeightNote));
				}
				else {
					g.DrawLine(xPenLower,
						ToPoints(x-hScroll.Value+Columns[i].State.XPos +1),
						ToPoints(y-vScroll.Value+1+ListGridRows[rowI].State.HeightMain+ ListGridRows[rowI].State.HeightNote),
						ToPoints(x-hScroll.Value+Columns[i].State.Right),
						ToPoints(y-vScroll.Value+1+ListGridRows[rowI].State.HeightMain+ ListGridRows[rowI].State.HeightNote));
				}
				//text
				if(ListGridRows[rowI].Cells.Count-1< i) {
					continue;
				}
				float adjH=0;
				switch(Columns[i].TextAlign) {
					case HorizontalAlignment.Left:
						_xAlign= XStringAlignment.Near;
						adjH=1;
						break;
					case HorizontalAlignment.Center:
						_xAlign= XStringAlignment.Center;
						adjH= Columns[i].State.Width/2f;
						break;
					case HorizontalAlignment.Right:
						_xAlign= XStringAlignment.Far;
						adjH= Columns[i].State.Width-2;
						break;
				}
				int vertical=y-vScroll.Value-2;
				int horizontal=x-hScroll.Value+1+Columns[i].State.XPos;
				int cellW=Columns[i].State.Width;
				int cellH=ListGridRows[rowI].State.HeightMain;
				if(_hasEditableColumn) {//These cells are taller
					vertical+=2;//so this is to push text down to center it in the cell
					cellH-=3;//to keep it from spilling into the next cell
				}
				if(Columns[i].TextAlign==HorizontalAlignment.Right){
					if(_hasEditableColumn) {
						horizontal-=4;
						cellW+=2;
					}
					else {
						horizontal-=2;
						cellW+=2;
					}
				}
				textRect=new XRect(p(horizontal + adjH),ToPoints(vertical),ToPoints(cellW),ToPoints(cellH));
				if(ListGridRows[rowI].Cells[i].ColorText==Color.Empty) {
					textBrush=new XSolidBrush(ListGridRows[rowI].ColorText);
				}
				else {
					textBrush=new XSolidBrush(ListGridRows[rowI].Cells[i].ColorText);
				}
				if(ListGridRows[rowI].Cells[i].Bold== YN.Yes) {
					font= fontBold;
				}
				else if(ListGridRows[rowI].Cells[i].Bold== YN.No) {
					font= fontNormal;
				}
				else {//unknown.  Use row bold
					if(ListGridRows[rowI].Bold) {
						font= fontBold;
					}
					else {
						font= fontNormal;
					}
				}
				//do not underline row if we are printing to PDF
				//if(rows[rowI].Cells[i].Underline==YN.Yes) {//Underline the current cell.  If it is already bold, make the cell bold and underlined.
				//	cellFont=new XFont(cellFont,(cellFont.Bold)?(XFontStyleEx.Bold | XFontStyleEx.Underline):XFontStyleEx.Underline);
				//}
				if(Columns[i].ImageList==null) {
					DrawStringX(g, ListGridRows[rowI].Cells[i].Text, font, textBrush, textRect, _xAlign);
				}
				else {
					int imageIndex=-1;
					if(ListGridRows[rowI].Cells[i].Text!="") {
						imageIndex= PIn.Int(ListGridRows[rowI].Cells[i].Text);
					}
					if(imageIndex != -1) {
						XImage img =Columns[i].ImageList.Images[imageIndex];
						g.DrawImage(img, horizontal, vertical - 1);
					}
				}
			}
			//note text
			if(ListGridRows[rowI].State.HeightNote>0 && NoteSpanStop>0 && NoteSpanStart < Columns.Count) {
				int noteW=0;
				for(int i=NoteSpanStart;i<=NoteSpanStop;i++) {
					noteW+=Columns[i].State.Width;
				}
				if(ListGridRows[rowI].Bold) {
					font=fontBold; 
				}
				else {
					font=fontNormal;
				}
				textBrush=new XSolidBrush(ListGridRows[rowI].ColorText);
				textRect=new XRect(
					ToPoints(x-hScroll.Value+1+Columns[NoteSpanStart].State.XPos+1),
					ToPoints(y-vScroll.Value+1+ListGridRows[rowI].State.HeightMain+1),
					ToPoints(Columns[NoteSpanStop].State.Right-Columns[NoteSpanStart].State.XPos),
					ToPoints(ListGridRows[rowI].State.HeightNote));
				_xAlign=XStringAlignment.Near;
				DrawStringX(g,ListGridRows[rowI].Note,font,textBrush,textRect,_xAlign);
			}
			//Left right and bottom lines of grid.  This creates the outline of the entire grid when not using outline control
			//Outline the Title
			XPen pen=new XPen(GridPrintLook.PenOutline.Color);
			if(isSheetGrid) {
				pen=new XPen(Color.Black);
			}
			//Draw line from LL to UL to UR to LR. top three sides of a rectangle.
			g.DrawLine(pen,ToPoints(x),ToPoints(y),ToPoints(x),ToPoints(y+ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote+1));//left side line
			g.DrawLine(pen,ToPoints(x+Width),ToPoints(y),ToPoints(x+Width),ToPoints(y+ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote+1));//right side line
			if(isBottom) {
				g.DrawLine(pen,ToPoints(x),ToPoints(y+ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote)+1,ToPoints(x+Width),ToPoints(y+ListGridRows[rowI].State.HeightMain+ListGridRows[rowI].State.HeightNote+1));//bottom line.
			}
		}

		public void SheetDrawTitle(Graphics g,int x,int y,float scaleMS) {
			Color cTitleTop=GridPrintLook.ColorTitleTop;
			Color cTitleBottom=GridPrintLook.ColorTitleBottom;
			LinearGradientBrush brushTitleBackground=new LinearGradientBrush(new Rectangle(x,y,Width,_heightTitlePrint),cTitleTop,cTitleBottom,LinearGradientMode.Vertical);
			g.FillRectangle(brushTitleBackground,x,y,Width,_heightTitlePrint);
			Font font=new Font(_fontPrintTitle.FontFamily,_fontPrintTitle.Size/scaleMS,_fontPrintTitle.Style);
			g.DrawString(_title,font,GridPrintLook.BrushTitleText,x+(Width/2-g.MeasureString(_title,font).Width/2),y+2);
			//Outline the Title
			//Draw line from LL to UL to UR to LR. top three sides of a rectangle.
			g.DrawLines(GridPrintLook.PenOutline,new Point[] { 
				new Point(x,y+_heightTitlePrint),
				new Point(x,y),
				new Point(x+Width,y),
				new Point(x+Width,y+_heightTitlePrint) });
			//g.DrawRectangle(pen,0,0,Width-1,Height-1);
			if(brushTitleBackground!=null) {
				brushTitleBackground.Dispose();
				brushTitleBackground=null;
			}
		}

		public void SheetDrawTitleX(XGraphics g,int x,int y) {
			Color cTitleTop=GridPrintLook.ColorTitleTop;
			Color cTitleBottom=GridPrintLook.ColorTitleBottom;
			Color cTitleText=GridPrintLook.BrushTitleText.Color;
			LinearGradientBrush brushTitleBackground=new LinearGradientBrush(new Rectangle(x,y,Width,_heightTitlePrint),cTitleTop,cTitleBottom,LinearGradientMode.Vertical);
			XSolidBrush brushTitleText=new XSolidBrush(cTitleText);
			g.DrawRectangle(brushTitleBackground,ToPoints(x),ToPoints(y),ToPoints(Width),ToPoints(_heightTitlePrint));
			XFont xTitleFont=new XFont(_fontPrintTitle.FontFamily.ToString(),_fontPrintTitle.Size,XFontStyleEx.Bold);
			//g.DrawString(title,titleFont,brushTitleText,p((float)x+(float)(Width/2-g.MeasureString(title,titleFont).Width/2)),p(y+2));
			DrawStringX(g,_title,xTitleFont,brushTitleText,new XRect((float)x+(float)(Width/2),y,100,100),XStringAlignment.Center);
			//Outline the Title
			//Draw line from LL to UL to UR to LR. top three sides of a rectangle.
			g.DrawLines(GridPrintLook.PenOutline,new Point[] { 
				new Point(x,y+_heightTitlePrint),
				new Point(x,y),
				new Point(x+Width,y),
				new Point(x+Width,y+_heightTitlePrint) });
			//g.DrawRectangle(pen,0,0,Width-1,Height-1);
			if(brushTitleBackground!=null) {
				brushTitleBackground.Dispose();
				brushTitleBackground=null;
			}
		}

		///<summary>If drawing to printer or pdf, pass in a scaleMS of 1. If drawing to screen, pass in the current ms scale, like 1.5.</summary>
		public void SheetDrawHeader(Graphics g,int x,int y,float scaleMS) {
			Color cOutline=cOutline=Color.Black;
			Color cTitleTop=Color.White;
			Color cTitleBottom=Color.FromArgb(213,213,223);
			Color cTitleText=Color.Black;
			Color cTitleBackG=Color.LightGray;
			g.FillRectangle(new SolidBrush(cTitleBackG),x,y,Width,_heightHeaderPrint);//background
			g.DrawLine(new Pen(Color.FromArgb(102,102,122)),x,y,x+Width,y);//line between title and headers
			Font font=new Font(_fontPrintHeader.FontFamily,_fontPrintHeader.Size/scaleMS,_fontPrintHeader.Style);
			for(int i=0;i<Columns.Count;i++) {
				if(i!=0) {
					//vertical lines separating column headers
					g.DrawLine(new Pen(cOutline), x + (-hScroll.Value + Columns[i].State.XPos), y,
						x+(-hScroll.Value + Columns[i].State.XPos), y + _heightHeaderPrint);
				}
				float widthText=g.MeasureString(Columns[i].Heading, font).Width;
				float xPos=(float)x + (-hScroll.Value + Columns[i].State.XPos + Columns[i].State.Width /2f- widthText/2f);
				g.DrawString(Columns[i].Heading, font, Brushes.Black,xPos,(float)y + 1);
				if(_sortedByColumnIdx == i) {
					PointF p = new PointF(x + (-hScroll.Value+1+ Columns[i].State.XPos +6), y + (float)_heightHeaderPrint / 2f);
					if(_isSortedAscending) { //pointing up
						g.FillPolygon(Brushes.White,new PointF[] {
							new PointF(p.X-4.9f,p.Y+2f), //LLstub
							new PointF(p.X-4.9f,p.Y+2.5f), //LLbase
							new PointF(p.X+4.9f,p.Y+2.5f), //LRbase
							new PointF(p.X+4.9f,p.Y+2f), //LRstub
							new PointF(p.X,p.Y-2.8f)
						}); //Top
						g.FillPolygon(Brushes.Black,new PointF[] {
							new PointF(p.X-4,p.Y+2), //LL
							new PointF(p.X+4,p.Y+2), //LR
							new PointF(p.X,p.Y-2)
						}); //Top
					}
					else { //pointing down
						g.FillPolygon(Brushes.White,new PointF[] { //shaped like home plate
							new PointF(p.X-4.9f,p.Y-2f), //ULstub
							new PointF(p.X-4.9f,p.Y-2.7f), //ULtop
							new PointF(p.X+4.9f,p.Y-2.7f), //URtop
							new PointF(p.X+4.9f,p.Y-2f), //URstub
							new PointF(p.X,p.Y+2.8f)
						}); //Bottom
						g.FillPolygon(Brushes.Black,new PointF[] {
							new PointF(p.X-4,p.Y-2), //UL
							new PointF(p.X+4,p.Y-2), //UR
							new PointF(p.X,p.Y+2)
						}); //Bottom
					}
				}
			} //end for columns.Count
			//Outline the Title
			using(Pen pen=new Pen(cOutline)) {
				g.DrawRectangle(pen,x,y,Width,_heightHeaderPrint);
			}
			g.DrawLine(new Pen(cOutline),x,y+_heightHeaderPrint,x+Width,y+_heightHeaderPrint);
		}

		public void SheetDrawHeaderX(XGraphics g,int x,int y) {
			Color cOutline=cOutline=Color.Black;
			Color cTitleTop=Color.White;
			Color cTitleBottom=Color.FromArgb(213,213,223);
			Color cTitleText=Color.Black;
			Color cTitleBackG=Color.LightGray;
			g.DrawRectangle(new XSolidBrush(cTitleBackG),ToPoints(x),ToPoints(y),ToPoints(Width),ToPoints(_heightHeaderPrint));//background
			g.DrawLine(new XPen(Color.FromArgb(102,102,122)),ToPoints(x),ToPoints(y),ToPoints(x+Width),ToPoints(y));//line between title and headers
			XFont xHeaderFont=new XFont(_fontPrintHeader.FontFamily.Name.ToString(),_fontPrintHeader.Size,XFontStyleEx.Bold);
			for(int i=0; i < Columns.Count; i++) {
				if(i != 0) {
					g.DrawLine(new XPen(cOutline), ToPoints(x + (-hScroll.Value + Columns[i].State.XPos)), ToPoints(y),
						ToPoints(x + (-hScroll.Value + Columns[i].State.XPos)), ToPoints(y + _heightHeaderPrint));
				}
				float xFloat=(float)x + (float)(-hScroll.Value + Columns[i].State.XPos + Columns[i].State.Width /2);//for some reason visual studio would not allow this statement within the DrawString Below.
				DrawStringX(g, Columns[i].Heading, xHeaderFont, XBrushes.Black,new XRect(p(xFloat), ToPoints(y - 3),100,100), XStringAlignment.Center);
			}//end for columns.Count
			//Outline the Title
			XPen pen=new XPen(cOutline);
			g.DrawRectangle(pen,ToPoints(x),ToPoints(y),ToPoints(Width),ToPoints(_heightHeaderPrint));
			g.DrawLine(new XPen(cOutline),ToPoints(x),ToPoints(y+_heightHeaderPrint),ToPoints(x+Width),ToPoints(y+_heightHeaderPrint));
		}
		#endregion Methods - Sheets		

		#region Methods - Selections
		///<summary>Throws exceptions.  Use to set a row selected or not.  Can handle values outside the acceptable range.</summary>
		public void SetSelected(int index,bool setValue=true) {
			if(setValue) {//select specified index
				if(_selectionMode==GridSelectionMode.None) {
					throw new Exception("Selection mode is none.");
				}
				if(index<0 || index>ListGridRows.Count-1) {//check to see if index is within the valid range of values
					return;//if not, then ignore.
				}
				if(_selectionMode==GridSelectionMode.OneRow) {
					_listSelectedIndices.Clear();//clear existing selection before assigning the new one.
				}
				if(!_listSelectedIndices.Contains(index)) {
					_listSelectedIndices.Add(index);
				}
			}
			else {//unselect specified index
				if(_listSelectedIndices.Contains(index)) {
					_listSelectedIndices.Remove(index);
				}
			}
			Invalidate();
		}

		///<summary>Throws exceptions.  Allows setting multiple values all at once</summary>
		public void SetSelected(int[] iArray,bool setValue) {
			if(_selectionMode==GridSelectionMode.None) {
				throw new Exception("Selection mode is none.");
			}
			if(_selectionMode==GridSelectionMode.OneRow) {
				throw new Exception("Selection mode is one.");
			}
			for(int i=0;i<iArray.Length;i++) {
				if(setValue) {//select specified index
					if(iArray[i]<0 || iArray[i]>ListGridRows.Count-1) {//check to see if index is within the valid range of values
						return;//if not, then ignore.
					}
					if(!_listSelectedIndices.Contains(iArray[i])) {
						_listSelectedIndices.Add(iArray[i]);
					}
				}
				else {//unselect specified index
					if(_listSelectedIndices.Contains(iArray[i])) {
						_listSelectedIndices.Remove(iArray[i]);
					}
				}
			}
			Invalidate();
		}

		///<summary>Selects or clears all rows.  Throws exceptions, especially when not MultiExtended.</summary>
		public void SetAll(bool setToValue){
			if(_selectionMode==GridSelectionMode.None) {
				throw new Exception("Selection mode is none.");
			}
			if(_selectionMode==GridSelectionMode.OneRow && setToValue==true) {
				throw new Exception("Selection mode is one.");
			}
			if(_selectionMode==GridSelectionMode.OneCell) {
				throw new Exception("Selection mode is OneCell.");
			}
			_listSelectedIndices.Clear();
			if(setToValue) {//select all
				for(int i=0;i<ListGridRows.Count;i++) {
					_listSelectedIndices.Add(i);
				}
			}
			Invalidate();
		}

		///<summary>Throws exceptions.</summary>
		public void SetSelected(Point setCell) {
			if(_selectionMode!=GridSelectionMode.OneCell) {
				throw new Exception("Selection mode must be OneCell.");
			}
			_selectedCell=setCell;
			textEdit?.Dispose();
			if(IsCellEditBox(_selectedCell.X,_selectedCell.Y)) {
				CreateEditBox();
			}
			Invalidate();
		}

		///<summary>If one row is selected, it returns the index to that row.  If more than one row are selected, it returns the first selected row.  Really only useful for SelectionMode.One.  If no rows selected, returns -1.</summary>
		public int GetSelectedIndex() {
			if(SelectionMode==GridSelectionMode.OneCell) {
				return _selectedCell.Y;
			}
			if(_listSelectedIndices.Count>0) {
				return _listSelectedIndices[0];
			}
			return -1;
		}

		///<summary>Returns the tag of the selected item.  If T does not match the type of tag, it will return the default of T (usually null).</summary>
		public T SelectedTag<T>() {
			if(SelectedIndices.Length > 0) {
				if(ListGridRows[GetSelectedIndex()].Tag is T) {
					return (T)ListGridRows[GetSelectedIndex()].Tag;
				}
			}
			return default(T);
		}

		///<summary>Returns the tags of the selected items.  If T does not match the type of tag, it will not be included in the returned list.</summary>
		public List<T> SelectedTags<T>() {
			List<T> listTags=new List<T>();
			foreach(int selectedIndex in SelectedIndices) {
				if(ListGridRows[selectedIndex].Tag is T) {
					listTags.Add((T)ListGridRows[selectedIndex].Tag);
				}
			}
			return listTags;
		}

		///<summary>Returns a list of tags for all items.  To get only the tags for selected items use SelectedTags().  If T does not match the type of tag, it will not be included in the returned list.</summary>
		public List<T> GetTags<T>() {
			List<T> listTags=new List<T>();
			foreach(GridRow row in ListGridRows) {
				if(row.Tag is T) {
					listTags.Add((T)row.Tag);
				}
			}
			return listTags;
		}
		#endregion Methods - Selections

		#region Methods - Scrolling
		///<summary>Usually called after entering a new list to automatically scroll to the top.</summary>
		public void ScrollToTop() {
			ScrollValue=vScroll.Minimum;//this does all error checking and invalidates
		}

		///<summary>Usually called after entering a new list to automatically scroll to the end.</summary>
		public void ScrollToEnd() {
			ScrollValue=vScroll.Maximum;//this does all error checking and invalidates
		}

		///<summary>The index of the row that is the first row displayed on the ODGrid. Also sets ScrollValue.</summary>
		public void ScrollToIndex(int index) {
			if(index>ListGridRows.Count || index < 0) {
				return;
			}
			ScrollValue=ListGridRows[index].State.YPos;
		}

		///<summary>The index of the row that is the last row to be displayed on the ODGrid. Also sets ScrollValue.</summary>
		public void ScrollToIndexBottom(int index) {
			if(index>ListGridRows.Count || index < 0) {
				return;
			}
			ScrollValue=ListGridRows[index].State.YPos+ListGridRows[index].State.HeightMain+ListGridRows[index].State.HeightNote+OriginY()-Height+3;//+3 accounts for the grid lines.
		}

		///<summary>If the index is currently scrolled out of view, then it scrolls to make that row fully visible.  It only scrolls minimally instead of centering.</summary>
		public void ScrollToMakeVisible(int index) {
			if(index>ListGridRows.Count || index < 0) {
				return;
			}
			if(-vScroll.Value+1+ScaleI(_heightTitle+_heightHeader)+ListGridRows[index].State.YPos+ListGridRows[index].State.HeightTotal > Height) {
				//bottom of row below lower edge of control
				ScrollToIndexBottom(index);
			}
			if(-vScroll.Value+ListGridRows[index].State.YPos < 0) {//top edge of row above top of grid area
				ScrollToIndex(index);
			}	
			//because of the order above, a cell that is taller than grid will be aligned to the top of the cell.
		}

		///<summary>Calcs header height, and resizes and lays out scrollbars.  Gets called when resize, dpi changes, or item visibility changes.  This requires _heightTotal from ComputeRowYposStartingAt in order to work. </summary>
		private void LayoutScrolls() {
			if(_isUpdating){
				return;
			}
			//no way to test suspendLayout state.
			if(Width==0 || Height==0){
				return;
			}
			if(_hasMultilineHeaders) {//added in R12381, v15?
				TextFormatFlags textFormatFlags=TextFormatFlags.Default;
				if(_hasAutoWrappedHeaders){//added in R16725, v17.1
					textFormatFlags=TextFormatFlags.WordBreak;
				}
				//since the header size is at 96dpi, we need to unscale the font
				using Font font=new Font(_fontHeader.FontFamily,UnscaleFontODZoom(_fontHeader.Size));
				for(int i=0;i<Columns.Count;i++){
					Size sizeText=TextRenderer.MeasureText(Columns[i].Heading,font,new Size(Columns[i].State.Width,int.MaxValue),textFormatFlags);
					int heightThisHeader=sizeText.Height-3;
					if(heightThisHeader>_heightHeader){//96dpi
						_heightHeader=heightThisHeader;
					}
				}
				//The same for printouts and sheets, in the print header font (UI-2a-3).
				using Font fontPrint=new Font(_fontPrintHeader.FontFamily,UnscaleFontODZoom(_fontPrintHeader.Size));
				for(int i=0;i<Columns.Count;i++){
					Size sizeText=TextRenderer.MeasureText(Columns[i].Heading,fontPrint,new Size(Columns[i].State.Width,int.MaxValue),textFormatFlags);
					int heightThisHeader=sizeText.Height-3;
					if(heightThisHeader>_heightHeaderPrint){//96dpi
						_heightHeaderPrint=heightThisHeader;
					}
				}
			}
			//Scrollbars could use 96dpi or scaled dpi.  We decided to use scaled, which means we can use scroll values directly for drawing, without scaling them
			vScroll.Width=ScaleI(17);//scroll width is 17 at 96dpi
			hScroll.Height=ScaleI(17);
			vScroll.Location=new Point(this.Width-vScroll.Width-1,OriginY()+1);
			if(this._hScrollVisible) {
				hScroll.Visible=true;
				vScroll.Height=this.Height-OriginY()-hScroll.Height-2;
				if(vScroll.Height<0){
					vScroll.Height=1;
				}
				hScroll.Location=new Point(1,this.Height-hScroll.Height-1);
				hScroll.Width=this.Width-vScroll.Width-2;
				if(_widthTotal<hScroll.Width) {
					hScroll.Value=0;
					hScroll.Enabled=false;
				}
				else {
					hScroll.Enabled=true;
					hScroll.Minimum = 0;
					hScroll.Maximum=_widthTotal;
					if(hScroll.Width<0){//Don't see how this is possible, but leaving it.
						hScroll.LargeChange=0;
					}
					else{
						hScroll.LargeChange=hScroll.Width;
					}
					hScroll.SmallChange=50;
				}
			}
			else {
				hScroll.Visible=false;
				vScroll.Height=this.Height-OriginY()-2;
			}
			if(vScroll.Height<=0) {
				return;
			}
			//All rows can be seen within the container, vertical scroll not needed.
			if(_heightTotal<vScroll.Height) {
				vScroll.Value=0;
				vScroll.Enabled=false;//Whether the vertical scrollbar is shown or not, disable it because it can't do anything.
				vScroll.Visible=VScrollVisible;//Show the vertical scrollbar if VScrollVisible is true even though it isn't needed.
				//The following line is so that ComputeColumns() knows to make the final column a bit shorter to account for the scrollbar.
				//If we use vScroll.Visible, then the scrollbar will not lay correctly when the grid is off the screen.
				_isVScrollVisible=VScrollVisible;
			}
			else {//All rows can't be seen within the container, vertical scroll is needed.
				vScroll.Enabled=true;
				vScroll.Visible=true;
				//This is so that ComputeColumns() knows to make the final column a bit shorter to account for the scrollbar.
				//If we use vScroll.Visible, then the scrollbar will not lay correctly when the grid is off the screen.
				_isVScrollVisible=true;
				vScroll.Minimum = 0;
				vScroll.Maximum=_heightTotal;
				vScroll.LargeChange=vScroll.Height;
				vScroll.SmallChange=(int)(14*3.4f);//it's not an even number so that it is obvious to user that rows moved
			}
			//Checks if old scroll position is less than new vScroll.Minimum or greater than vScroll.Maximum and adjusts if necessary.
			//Fixed an issue for a customer (Task 1562673) where viewing Family Module on patient with lengthy insurance grid and then changing to a 
			//patient with a shorter insurance grid did not properly update the vertical scroll position, resulting in a seemingly blank insurance grid 
			//until the user scrolls vertically.  This made it seem like the second patient did not have insurance.
			ScrollValue=vScroll.Value;
			Invalidate();
		}

		///<summary>Font is ignored.  We base font off of scale, so when scale changes, we change all the fonts here.</summary>
		private void SetFonts(){
			_fontCell?.Dispose();
			_fontCellBold?.Dispose();
			_fontUnderline?.Dispose();
			_fontUnderlineBold?.Dispose();
			_fontHeader?.Dispose();
			_fontTitle?.Dispose();
			_fontPrintCell?.Dispose();
			_fontPrintCellBold?.Dispose();
			_fontPrintHeader?.Dispose();
			_fontPrintTitle?.Dispose();
			//The screen's title and header heights follow the font tokens (UI-5); a multiline header grows again in LayoutScrolls.
			if(_titleVisible) {
				_heightTitle=GridScreenLook.GetHeightTitle96();
			}
			if(_headersVisible) {
				_heightHeader=GridScreenLook.GetHeightHeader96();
			}
			//Screen: the font tokens (UI-2a-3), at the grid's zoom. this.Font is ignored.
			_fontCell=NewScreenFont(UiTheme.FontGrid);
			_fontCellBold=NewScreenFont(UiTheme.FontGridBold);
			_fontUnderline=NewScreenFont(UiTheme.FontGridUnderline);
			_fontUnderlineBold=NewScreenFont(UiTheme.FontGridUnderlineBold);
			_fontHeader=NewScreenFont(UiTheme.FontGridBold);
			_fontTitle=NewScreenFont(UiTheme.FontTitle);
			//Print and sheets: today's fonts, pinned (plan section 5.2). They stay when the tokens change.
			_fontPrintCell=new Font(FontFamily.GenericSansSerif,ScaleFontODZoom(8.5f));
			_fontPrintCellBold=new Font(FontFamily.GenericSansSerif,ScaleFontODZoom(8.5f),FontStyle.Bold);
			_fontPrintHeader=new Font(FontFamily.GenericSansSerif,ScaleFontODZoom(8.5f),FontStyle.Bold);
			_fontPrintTitle=new Font(FontFamily.GenericSansSerif,ScaleFontODZoom(10),FontStyle.Bold);
			Invalidate();
			//Always follow this with both ComputeRows and LayoutScrolls
		}

		///<summary>A new screen font in the token's family, size and style, at the grid's zoom. The caller owns it.</summary>
		private Font NewScreenFont(Font fontToken) {
			return new Font(fontToken.FontFamily,ScaleFontODZoom(fontToken.SizeInPoints),fontToken.Style);
		}

		///<summary>True when the row heights come from the print fonts: a grid that only draws a sheet or a document, or one in PrintPage.</summary>
		private bool IsPrintFonts() {
			return IsForSheets || IsForDocument || _isPrinting;
		}

		///<summary>True while the screen's cell fonts are the print fonts (today's tokens): row heights measured for the screen are then what they
		///were for a printout before the split.</summary>
		private bool AreScreenFontsThePrintFonts() {
			return IsSameFont(_fontCell,_fontPrintCell) && IsSameFont(_fontCellBold,_fontPrintCellBold);
		}

		private static bool IsSameFont(Font font1,Font font2) {
			return font1.Name==font2.Name && font1.SizeInPoints==font2.SizeInPoints && font1.Style==font2.Style;
		}

		private void vScroll_Scroll(object sender,System.Windows.Forms.ScrollEventArgs e) {
			textEdit?.Dispose();
			Invalidate();
			this.Focus();
		}

		private void hScroll_Scroll(object sender,System.Windows.Forms.ScrollEventArgs e) {
			//if(UpDownKey) return;
			if(_hScrollVisible && e.OldValue!=e.NewValue) {
				HorizScrolled?.Invoke(this,e);
			}
			Invalidate();
			this.Focus();
		}

		#endregion Methods - Scrolling

		#region Methods - Sorting
		///<summary>Returns current sort column index.  Use SortForced to maintain current grid sorting after refreshing a grid.  </summary>
		public int GetSortedByColumnIdx(){
			return _sortedByColumnIdx;
		}

		///<summary>Returns current sort order.  Use SortForced to maintain current grid sorting after refreshing a grid.</summary>
		public bool IsSortedAscending(){
			return _isSortedAscending;
		}

		///<summary>Set sortedByColIdx to -1 to clear sorting. Copied from SortByColumn. No need to call fill grid after calling this.  Also used in PatientPortalManager.</summary>
		public void SortForced(int sortedByColumnIdx,bool sortedIsAscending) {
			_isSortedAscending=sortedIsAscending;
			_sortedByColumnIdx=sortedByColumnIdx;
			if(sortedByColumnIdx==-1) {
				return;
			}
			List<GridRow> rowsSorted=new List<GridRow>();
			for(int i=0;i<ListGridRows.Count;i++) {
				rowsSorted.Add(ListGridRows[i]);
			}
			if(Columns[_sortedByColumnIdx].SortingStrategy==GridSortingStrategy.StringCompare) {
				rowsSorted.Sort(SortStringCompare);
			}
			else if(Columns[_sortedByColumnIdx].SortingStrategy==GridSortingStrategy.DateParse) {
				rowsSorted.Sort(SortDateParse);
			}
			else if(Columns[_sortedByColumnIdx].SortingStrategy==GridSortingStrategy.ToothNumberParse) {
				rowsSorted.Sort(SortToothNumberParse);
			}
			else if(Columns[_sortedByColumnIdx].SortingStrategy==GridSortingStrategy.AmountParse) {
				rowsSorted.Sort(SortAmountParse);
			}
			else if(Columns[_sortedByColumnIdx].SortingStrategy==GridSortingStrategy.TimeParse) {
				rowsSorted.Sort(SortTimeParse);
			}
			else if(Columns[_sortedByColumnIdx].SortingStrategy==GridSortingStrategy.VersionNumber) {
				rowsSorted.Sort(SortVersionParse);
			}
			BeginUpdate();
			ListGridRows.Clear();
			for(int i=0;i<rowsSorted.Count;i++) {
				ListGridRows.Add(rowsSorted[i]);
			}
			EndUpdate();
			_sortedByColumnIdx=sortedByColumnIdx;//Must be set again since set to -1 in EndUpdate();
		}

		///<summary>Swaps two rows in the grid. Returns false if either of the indexes is greater than the number of rows in the grid.</summary>
		public bool SwapRows(int indxMoveFrom,int indxMoveTo) {
			if(ListGridRows.Count<=Math.Max(indxMoveFrom,indxMoveTo)
				|| Math.Min(indxMoveFrom,indxMoveTo)<0) 
			{
				return false;
			}
			BeginUpdate();
			GridRow dataRowFrom=ListGridRows[indxMoveFrom];
			ListGridRows[indxMoveFrom]=ListGridRows[indxMoveTo];
			ListGridRows[indxMoveTo]=dataRowFrom;
			EndUpdate();
			return true;
		}

		///<summary>Gets run on mouse down on a column header.</summary>
		private void SortByColumn(int mouseDownCol) {
			if(mouseDownCol==-1) {
				return;
			}
			if(_sortedByColumnIdx==mouseDownCol) {//already sorting by this column
				_isSortedAscending=!_isSortedAscending;//switch ascending/descending.
			}
			else {
				_isSortedAscending=true;//start out ascending
				_sortedByColumnIdx=mouseDownCol;
			}
			if(AllowSortingByColumn) {
				ColumnSortClick?.Invoke(this,new EventArgs());
				_sortedByColumnIdx=mouseDownCol;//Must be set again since set to -1 in EndUpdate();
			}
			List<GridRow> rowsSorted=new List<GridRow>();
			for(int i=0;i<ListGridRows.Count;i++) {
				rowsSorted.Add(ListGridRows[i]);
			}
			if(Columns[_sortedByColumnIdx].SortingStrategy==GridSortingStrategy.StringCompare) {
				rowsSorted.Sort(SortStringCompare);
			}
			else if(Columns[_sortedByColumnIdx].SortingStrategy==GridSortingStrategy.DateParse) {
				rowsSorted.Sort(SortDateParse);
			}
			else if(Columns[_sortedByColumnIdx].SortingStrategy==GridSortingStrategy.ToothNumberParse) {
				rowsSorted.Sort(SortToothNumberParse);
			}
			else if(Columns[_sortedByColumnIdx].SortingStrategy==GridSortingStrategy.AmountParse) {
				rowsSorted.Sort(SortAmountParse);
			}
			else if(Columns[_sortedByColumnIdx].SortingStrategy==GridSortingStrategy.TimeParse) {
				rowsSorted.Sort(SortTimeParse);
			}
			else if(Columns[_sortedByColumnIdx].SortingStrategy==GridSortingStrategy.VersionNumber) {
				rowsSorted.Sort(SortVersionParse);
			}
			BeginUpdate();
			ListGridRows.Clear();
			for(int i=0;i<rowsSorted.Count;i++) {
				ListGridRows.Add(rowsSorted[i]);
			}
			EndUpdate();
			_sortedByColumnIdx=mouseDownCol;//Must be set again since set to -1 in EndUpdate();
			if(AllowSortingByColumn) { //only check this if sorting by column is enabled for the grid
				ColumnSorted?.Invoke(this,new EventArgs());
			}
		}

		private int SortStringCompare(GridRow row1,GridRow row2) {
			string textRow1=row1?.Cells[_sortedByColumnIdx].Text??"";
			return (_isSortedAscending?1:-1)*textRow1.CompareTo(row2?.Cells[_sortedByColumnIdx].Text??"");
		}

		private int SortDateParse(GridRow row1,GridRow row2) {
			string raw1=row1.Cells[_sortedByColumnIdx].Text;
			string raw2=row2.Cells[_sortedByColumnIdx].Text;
			DateTime date1=DateTime.MinValue;
			DateTime date2=DateTime.MinValue;
			//TryParse is a much faster operation than Parse in the event that the input won't parse to a date.
			if(DateTime.TryParse(raw1,out date1) &&
				DateTime.TryParse(raw2,out date2)) {
				return (_isSortedAscending?1:-1)*date1.CompareTo(date2);
			}
			else { //One of the inputs is not a date so default string compare.
				return SortStringCompare(row1,row2);
			}
		}

		private int SortTimeParse(GridRow row1,GridRow row2) {
			string raw1=row1.Cells[_sortedByColumnIdx].Text;
			string raw2=row2.Cells[_sortedByColumnIdx].Text;
			TimeSpan time1;
			TimeSpan time2;
			//TryParse is a much faster operation than Parse in the event that the input won't parse to a date.
			if(TimeSpan.TryParse(raw1,out time1) &&
				TimeSpan.TryParse(raw2,out time2)) {
				return (_isSortedAscending?1:-1)*time1.CompareTo(time2);
			}
			else { //One of the inputs is not a date so default string compare.
				return SortStringCompare(row1,row2);
			}
		}

		private int SortToothNumberParse(GridRow row1,GridRow row2) {
			//remember that teeth could be in international format.
			//fail gracefully
			string raw1=row1.Cells[_sortedByColumnIdx].Text;
			string raw2=row2.Cells[_sortedByColumnIdx].Text;
			if(!Tooth.IsValidEntry(raw1) && !Tooth.IsValidEntry(raw2)) {//both invalid
				return 0;
			}
			int retVal=0;
			if(!Tooth.IsValidEntry(raw1)) {//only first invalid
				retVal=-1; ;
			}
			else if(!Tooth.IsValidEntry(raw2)) {//only second invalid
				retVal=1; ;
			}
			else {//both valid
				string tooth1=Tooth.Parse(raw1);
				string tooth2=Tooth.Parse(raw2);
				int toothInt1=Tooth.ToInt(tooth1);
				int toothInt2=Tooth.ToInt(tooth2);
				retVal=toothInt1.CompareTo(toothInt2);
			}
			return (_isSortedAscending?1:-1)*retVal;
		}

		private int SortVersionParse(GridRow row1,GridRow row2) {
			Version v1, v2;
			if(!Version.TryParse(row1.Cells[_sortedByColumnIdx].Text,out v1)) {
				v1=new Version();//0.0.0.0
			}
			if(!Version.TryParse(row2.Cells[_sortedByColumnIdx].Text,out v2)) {
				v2=new Version();//0.0.0.0
			}
			return (_isSortedAscending?1:-1)*v1.CompareTo(v2);
		}

		private int SortAmountParse(GridRow row1,GridRow row2) {
			//This is here because AmountParse does not sort correctly when the amount contains non-numeric characters
			//We could improve this later with some kind of grid text cleaner that is called before running this sort.
			string raw1=row1.Cells[_sortedByColumnIdx].Text;			
			raw1=raw1.Replace("$","");
			string raw2=row2.Cells[_sortedByColumnIdx].Text;
			raw2=raw2.Replace("$","");
			Decimal amt1=0;
			Decimal amt2=0;
			if(raw1!="") {
				try {
					amt1=Decimal.Parse(raw1);
				}
				catch {
					return 0;//shouldn't happen
				}
			}
			if(raw2!="") {
				try {
					amt2=Decimal.Parse(raw2);
				}
				catch {
					return 0;//shouldn't happen
				}
			}
			return (_isSortedAscending?1:-1)*amt1.CompareTo(amt2);
		}
		#endregion Methods - Sorting		

		#region Methods - Perform Events
		public void PerformCellDoubleClick(object sender,ODGridClickEventArgs e){
			CellDoubleClick?.Invoke(sender,e);
		}
		#endregion Methods - Perform Events

		#region Methods - Private Static
		///<summary>For the given background and selected color, will average the two to return the color of the row or cells background if selected.  If the color is white, will simply return the selected color.</summary>
		private static Color GetSelectedColor(Color colorBackground,Color colorSelected) {
			if(colorBackground.ToArgb()==Color.White.ToArgb()) {
				return colorSelected;
			}
			//colorSelected is 205,220,235
			//If our row is light blue, then averaging it with the above color does not make it stand out enough.
			//Example of a light blue background is 235,249,255
			if(colorBackground.B>200
				&& colorBackground.B-colorBackground.R > 0
				&& colorBackground.B-colorBackground.G > 0)
			{
				//Guaranteed to be light blue.
				//If it's too close to the colorSelected, average it with a darker bluer color
				if(Math.Abs(colorBackground.R-colorSelected.R)<20
					&& Math.Abs(colorBackground.G-colorSelected.G)<20
					&& Math.Abs(colorBackground.B-colorSelected.B)<20)
				{
					Color colorDarker=Color.FromArgb(colorSelected.R-20,colorSelected.G-20,colorSelected.B);
					return Color.FromArgb(
						(colorDarker.R+colorBackground.R)/2,
						(colorDarker.G+colorBackground.G)/2,
						(colorDarker.B+colorBackground.B)/2);
				}
				//any other light blue that's not too similar to colorSelected
				return colorSelected;
			}
			Color newBackgroundColor=Color.FromArgb(
				(colorSelected.R+colorBackground.R)/2,
				(colorSelected.G+colorBackground.G)/2,
				(colorSelected.B+colorBackground.B)/2);
			return newBackgroundColor;
		}

		///<summary>The pdfSharp version of drawstring.  g is used for measurement.  scaleToPix scales xObjects to pixels.</summary>
		private static void DrawStringX(XGraphics xg,string str,XFont xfont,XBrush xbrush,XRect xbounds,XStringAlignment sa) {
			Graphics g=Graphics.FromImage(new Bitmap(100,100));//only used for measurements.
			int topPad=0;// 2;
			int rightPad=5;//helps get measurements better.
			double scaleToPix=1d/ToPoints(1);
			//There are two coordinate systems here: pixels (used by us) and points (used by PdfSharp).
			//MeasureString and ALL related measurement functions must use pixels.
			//DrawString is the ONLY function that uses points.
			//pixels:
			Rectangle bounds=new Rectangle((int)(scaleToPix*xbounds.Left),
				(int)(scaleToPix*xbounds.Top),
				(int)(scaleToPix*xbounds.Width),
				(int)(scaleToPix*xbounds.Height));
			FontStyle fontstyle=FontStyle.Regular;
			if(xfont.Style==XFontStyleEx.Bold) {
				fontstyle=FontStyle.Bold;
			}
			//pixels: (except Size is em-size)
			Font font=new Font(xfont.FontFamily.Name,(float)xfont.Size,fontstyle);
			bool hasNonAscii=str.Any(x => x > 127);
			if(hasNonAscii) {
				XPdfFontOptions options=new XPdfFontOptions(PdfFontEncoding.Unicode,PdfFontEmbedding.TryComputeSubset);
				xfont=new XFont(xfont.FontFamily.Name,xfont.Size,xfont.Style,options);
			}
			else {
				xfont=new XFont(xfont.FontFamily.Name,xfont.Size,xfont.Style);
			}
			//pixels:
			SizeF fit=new SizeF((float)(bounds.Width-rightPad),(float)(font.Height));
			StringFormat format=StringFormat.GenericTypographic;
			//pixels:
			float pixelsPerLine=(float)font.Height-0.5f;//LineSpacingForFont(font.Name) * (float)font.Height;
			float lineIdx=0;
			int chars;
			int lines;
			//points:
			RectangleF layoutRectangle;
			for(int ix=0;ix<str.Length;ix+=chars) {
				if(bounds.Y+topPad+pixelsPerLine*lineIdx>bounds.Bottom) {
					break;
				}
				//pixels:
				g.MeasureString(str.Substring(ix),font,fit,format,out chars,out lines);
				//PdfSharp isn't smart enough to cut off the lower half of a line.
				//if(bounds.Y+topPad+pixelsPerLine*lineIdx+font.Height > bounds.Bottom) {
				//	layoutH=bounds.Bottom-(bounds.Y+topPad+pixelsPerLine*lineIdx);
				//}
				//else {
				//	layoutH=font.Height+2;
				//}
				//use points here:
				float adjustTextDown=10f;//this value was arrived at by trial and error.
				layoutRectangle=new RectangleF(
					(float)xbounds.X,
					//(float)(xbounds.Y+(float)topPad/scaleToPix+(pixelsPerLine/scaleToPix)*lineIdx),
					(float)(xbounds.Y+adjustTextDown+(pixelsPerLine/scaleToPix)*lineIdx),
					(float)xbounds.Width+50,//any amount of extra padding here will not cause malfunction
					0);//layoutH);
				XStringFormat sf=XStringFormats.Default;
				sf.Alignment=sa;
				xg.DrawString(str.Substring(ix,chars),xfont,xbrush,(double)layoutRectangle.Left,(double)layoutRectangle.Top,sf);
				lineIdx+=1;
			}
			g.Dispose();
		}

		///<summary>This line spacing is specifically picked to match the RichTextBox.</summary>
		private static float LineSpacingForFont(string fontName) {
			if(fontName.ToLower()=="arial") {
				return 1.055f;
			}
			else if(fontName.ToLower()=="courier new") {
				return 1.055f;
			}
			//else if(fontName.ToLower()=="microsoft sans serif"){
			//	return 1.00f;
			//}
			return 1.05f;
		}
		
		///<summary>Converts pixels used by us to points used by PdfSharp.</summary>
		private static double ToPoints(int pixels){
			XUnit xunit=XUnit.FromInch((double)pixels/100d);//100 ppi
			return xunit.Point;
				//XUnit.FromInch((double)pixels/100);
		}

		///<summary>Converts pixels used by us to points used by PdfSharp.</summary>
		private static double p(float pixels){
			XUnit xunit=XUnit.FromInch((double)pixels/100d);//100 ppi
			return xunit.Point;
		}
		#endregion Methods - Private Static

		#region Methods - Private
		///<summary>Computes the position of each column and the overall width.  Called from EndUpdate, when adding columns, when Size changes, or scale changes.</summary>
		private void ComputeColumns() {
			if(_hScrollVisible) {
				for(int i=0;i<Columns.Count;i++){
					Columns[i].State.Width=ScaleI(Columns[i].ColWidth);
				}
			}
			else{
				float sumDynamic=0;//sum of the weights of the dynamic columns, typically about 1 or 2.
				int widthFixedSum=0;
				for(int i=0;i<Columns.Count;i++){
					//if(ListGridColumns[i].ColWidth<1){
						//todo: handle this in the ColWidth property instead
						//throw new ApplicationException("Grid column width less than 1 not allowed.");
						//ListGridColumns[i].State.Width=20;//just temporary
					//}
					if(Columns[i].IsWidthDynamic){
						sumDynamic+=Columns[i].DynamicWeight;
					}
					else{
						Columns[i].State.Width=ScaleI(Columns[i].ColWidth);
						widthFixedSum+=Columns[i].State.Width;
					}
				}
				string name=this.Name;
				//Debug.WriteLine(name);
				if(sumDynamic>0){
					int widthExtra=Width-2-widthFixedSum;
					if(_printWidth>0){
						widthExtra=_printWidth-2-widthFixedSum;
					}
					//If the vertical scrollbar is visible, then make some space for it.
					if(_isVScrollVisible){
						widthExtra-=vScroll.Width;
					}
					for(int i=0;i<Columns.Count;i++){
						if(Columns[i].IsWidthDynamic){
							//example sum=1+2.5, width=350/3.5*2.5=250
							Columns[i].State.Width=(int)(widthExtra/sumDynamic*Columns[i].DynamicWeight);
						}
					}
				}
				else if(Columns.Count>0 && !IsForSheets) {//resize the last column automatically
					int widthExtra=Width-2-widthFixedSum+ScaleI(Columns[Columns.Count-1].ColWidth);
					if(_printWidth>0){
						widthExtra=_printWidth-2-widthFixedSum+ScaleI(Columns[Columns.Count-1].ColWidth);
					}
					//If the vertical scrollbar is visible, then make some space for it.
					if(_isVScrollVisible){
						widthExtra-=vScroll.Width;
					}
					if(widthExtra>0){
						Columns[Columns.Count-1].State.Width=widthExtra;
					}
				}
			}
			//widths are all set
			for(int i=0;i<Columns.Count;i++){
				if(i==0){
					Columns[i].State.XPos=0;
				}
				else{
					Columns[i].State.XPos=Columns[i-1].State.XPos+Columns[i-1].State.Width;
				}
				Columns[i].State.Right=Columns[i].State.XPos+Columns[i].State.Width;
			}
			if(Columns.Count>0) {
				_widthTotal=Columns[Columns.Count-1].State.Right;
			}
			Invalidate();
		}

		///<summary>Called from PrintPage() and EndUpdate().  After adding rows to the grid, this calculates the height of each row because some rows may have text wrap and will take up more than one row.  Also, rows with notes must be made much larger because notes start on the second line.  If column images are used, rows will be enlarged to make space for the images.  If doActualCalc is true, then each row height will be truly calculated.  Otherwise, each row will be set to single row height, and then calculated properly when first show.</summary>
		private void ComputeRows(Graphics g,bool doActualCalc) {
			_heightTotal=0;
			if(ListGridRows.Count==0){
				Invalidate();
				return;
			}
			_widthNote=0;
			if(0 <= NoteSpanStart && NoteSpanStart < Columns.Count
				&&  0 < NoteSpanStop && NoteSpanStop <= Columns.Count)
			{
				for(int i=NoteSpanStart;i<=NoteSpanStop;i++) {
					_widthNote+=Columns[i].State.Width;
				}
			}
			_heightImage=0;
			_hasEditableColumn=false;
			for(int i=0;i<Columns.Count;i++) {
				if(Columns[i].IsEditable || Columns[i].ListDisplayStrings != null) {
					_hasEditableColumn=true;
				}
				if(Columns[i].ImageList!=null) {
					if(ScaleI(Columns[i].ImageList.ImageSize.Height)> _heightImage) {
						_heightImage=ScaleI(Columns[i].ImageList.ImageSize.Height)+1;
					}
				}
			}
			DateTime dt1=DateTime.Now;
			_areRowHeightsForPrint=IsPrintFonts();
			int heightEach=(_areRowHeightsForPrint ? _fontPrintCell : _fontCell).Height+1;//assume one row
			if(_hasEditableColumn) {
				heightEach=HeightEditableRow();
			}
			if(_heightImage>heightEach){//_heightImage is already scaled to current dpi
				heightEach=_heightImage;
			}
			for(int i=0;i<ListGridRows.Count;i++) {
				if(doActualCalc){
					//Slow on huge sets of rows, but might be required for printing, etc.
					ComputeRowHeightOne(g,i);
				}
				else{
					//This is just a starting point. As rows are shown, they we be truly calculated.
					ListGridRows[i].State.HeightMain=heightEach;
					ListGridRows[i].State.IsHeightCalculated=false;
				}
			}
			DateTime dt2=DateTime.Now;
			TimeSpan timeSpanHeights=dt2-dt1;//put a breakpoint below this line to look at the timespan
			ComputeRowYposStartingAt(0);
			if(IsForSheets) {
				ComputeSheetSupplemental();
			}
			Invalidate();
			//Always follow this up with LayoutScrolls because of _heightTotal
		}

		private void ComputeRowHeightOne(Graphics g,int i){
			//We can create/dispose 500k fonts per second so we could do that instead.
			Font fontNormal=_fontCell;//do not dispose any of these font refs.
			Font fontBold=_fontCellBold;
			if(IsPrintFonts()) {//a printout or a sheet measures with the fonts it draws with (UI-2a-3)
				fontNormal=_fontPrintCell;
				fontBold=_fontPrintCellBold;
			}
			if(_fontForSheets!=null) {
				fontNormal=_fontForSheets;
				fontBold=_fontForSheetsBold;
			}
			for(int j=0;j<ListGridRows[i].Cells.Count;j++) {
				if(ListGridRows[i].Cells[j].Text!=null && ListGridRows[i].Cells[j].Text.Length>=TEXT_LENGTH_LIMIT){
					ListGridRows[i].Cells[j].Text=ListGridRows[i].Cells[j].Text.Substring(0,TEXT_LENGTH_LIMIT);
				}
			}
			ListGridRows[i].State.HeightMain=0;
			Font font=fontNormal;
			if(ListGridRows[i].Bold) {
				font=fontBold;
			}
			else {
				//Determine if any cells in this row are bold.  If at least one cell is bold, then we need to calculate the row height using bold font.
				//Bold only affects width, but if there is wrap, then width can affect height.
				for(int j=0;j<ListGridRows[i].Cells.Count;j++) {
					if(ListGridRows[i].Cells[j].Bold==YN.Yes) {//We don't care if a cell is underlined because it does not affect the size of the row
						font=fontBold;
						break;
					}
				}
			}
			int heightCell;
			if(WrapText) {//make row taller after measuring
				//for measurement, we need to use a scaled font (not sure why)
				Font fontMeas=new Font(font.FontFamily,ScaleMS(font.Size),font.Style);
				//find the tallest col
				for(int j=0;j<ListGridRows[i].Cells.Count;j++) {
					if(_hasEditableColumn) {
						//todo: clean up this math
						//doesn't seem to calculate right when it ends in a "\r\n". It doesn't make room for the new line. Make it, by adding another one for calculations.

						heightCell=(int)Math.Ceiling(((1.03) 
							*(float)(g.MeasureString(ListGridRows[i].Cells[j].Text+"\r\n",fontMeas,Columns[j].State.Width,_stringFormat).Height))+4);//because textbox will be bigger
						//heightCell=(int)ScaleMS(heightCell);
						if(heightCell < HeightEditableRow()) {
							heightCell=HeightEditableRow();//only used for single line text
						}
					}
					else {//no editable column
						float hTemp;
						//hTemp=g.MeasureString(ListGridRows[i].Cells[j].Text,font,ListGridColumns[j].State.Width+7,_stringFormat).Height;
						//The above line can help it look better on screen, with less wasted space at right.  
						//But both printing and sheets need to calculate slightly differently.
						//The line below is a compromise for now, until a more rigorous overhaul can be done
						//to combine all three into one set of drawing commands.
						hTemp=g.MeasureString(ListGridRows[i].Cells[j].Text,fontMeas,Columns[j].State.Width,_stringFormat).Height;
						//hTemp=ScaleMS(hTemp);
						if(ListGridRows[i].Cells[j].IsButton){
							hTemp=g.MeasureString(ListGridRows[i].Cells[j].Text,fontMeas,Columns[j].State.Width-1,_stringFormat).Height;
							//hTemp=ScaleMS(hTemp);
						}
						if(_hasDropDowns && j==0) {//only draw the dropdown arrow in the first column of the row.
							if(ListGridRows[i].State.DropDownState!=ODGridDropDownState.None || ListGridRows[i].DropDownParent!=null) {
								hTemp=g.MeasureString(ListGridRows[i].Cells[j].Text,fontMeas,Columns[j].State.Width-10,_stringFormat).Height;
								//hTemp=ScaleMS(hTemp);
								//might be easier to just not support wrap in a dropdown cell. Oh well.
								if(WrapText && Columns[j].TextAlign==HorizontalAlignment.Left){
									hTemp=g.MeasureString(ListGridRows[i].Cells[j].Text,fontMeas,Columns[j].State.Width-3,_stringFormat).Height;
									//hTemp=ScaleMS(hTemp);
								}
							}
						}
						if(IsForSheets) {
							hTemp=LineSpacingForFont(font.Name)*hTemp;
						}
						heightCell=(int)Math.Floor(hTemp);
					}
					//if(rows[i].Height==0) {//not set
					//  cellH=(int)Math.Ceiling(g.MeasureString(rows[i].Cells[j].Text,cellFont,_listCurColumnWidths[j]).Height+1);
					//}
					//else {
					//  cellH=rows[i].Height;
					//}
					if(heightCell>ListGridRows[i].State.HeightMain) {
						ListGridRows[i].State.HeightMain=heightCell;
					}
				}
				if(ListGridRows[i].State.HeightMain<4) {
					ListGridRows[i].State.HeightMain=(int)ScaleMS(font.Height);
				}
			}
			else {//text not wrapping
				if(_hasEditableColumn) {
					ListGridRows[i].State.HeightMain=HeightEditableRow();
				}
				else {
					ListGridRows[i].State.HeightMain=(int)ScaleMS(font.Height)+1;
				}
			}
			if(_heightImage>ListGridRows[i].State.HeightMain) {//_heightImage is already scaled to current dpi
				ListGridRows[i].State.HeightMain=_heightImage;
			}
			if(_widthNote>0 && ListGridRows[i].Note!="") {
				string strToMeasure=ListGridRows[i].Note;
				if(strToMeasure.Length>TEXT_LENGTH_LIMIT){
					strToMeasure=strToMeasure.Substring(0,TEXT_LENGTH_LIMIT)+"\r\n"+Lans.g(this,"...more...");
				}
				float heightNote=g.MeasureString(strToMeasure,fontNormal,_widthNote,_stringFormat).Height;//Notes cannot be bold.  Always normal font.
				heightNote=ScaleMS(heightNote);
				ListGridRows[i].State.HeightNote=(int)heightNote;
			}
			if(ListGridRows[i].Cells.Any(x => x.IsButton)){
				if(ListGridRows[i].State.HeightMain<HeightEditableRow()) {
					ListGridRows[i].State.HeightMain=HeightEditableRow();
				}
			}
			if(_hasDropDowns) {
				if(ListGridRows[i].DropDownParent!=null){//if this is a child
					//we have direct access to the parent object, already
					if(ListGridRows[i].DropDownParent.State.DropDownState==ODGridDropDownState.None){
						//if this is an initial ComputeRows, set Up/Down
						//If it's a recompute after a click, then respect the existing Up/Down
						if(ListGridRows[i].DropDownParent.DropDownInitiallyDown) {
							ListGridRows[i].DropDownParent.State.DropDownState=ODGridDropDownState.Down;
						}
						else{
							ListGridRows[i].DropDownParent.State.DropDownState=ODGridDropDownState.Up;
						}
					}
					//we support multiple cascading parents
					GridRow rowCur=ListGridRows[i];
					//Keeps track of which rows have already been listed as a parent of this row. The same parent should not be added to this list more than one time.
					List<GridRow> listParentRows=new List<GridRow>();
					ListGridRows[i].State.Visible=true;
					while(rowCur.DropDownParent!=null) { //Keep going until we get to the topmost parent row in the tree. If DropDownParent==null, that means this row has no parent.
						if(listParentRows.Contains(rowCur)) {
							throw new Exception("You cannot have a parent row and a child row pointing to each other.");
						}
						listParentRows.Add(rowCur);
						if(rowCur.DropDownParent.State.DropDownState!=ODGridDropDownState.Down) { 
							//if any parent is not down
							ListGridRows[i].State.Visible=false;
						}
						//retain height in case we later make it visible again.
						//we choose not to break here for 2 reasons:
						//-a higher parent might be up, causing this row to be not visible
						//-we want go through all the parents to make sure there is not a circular relationship between parent & child rows.
						//go up one parent, then run the same loop.
						rowCur=rowCur.DropDownParent;
					}
				}//if this is a child
			}//if _hasDropdowns
			ListGridRows[i].State.IsHeightCalculated=true;
		}

		private void ComputeRowYposStartingAt(int startingAtIdx){
			DateTime dt1=DateTime.Now;
			int yPos=0;
			if(startingAtIdx>0){
				yPos=ListGridRows[startingAtIdx-1].State.YPos+ListGridRows[startingAtIdx-1].State.HeightTotal;
			}
			for(int i=startingAtIdx;i<ListGridRows.Count;i++) {
				ListGridRows[i].State.YPos=yPos;
				if(!ListGridRows[i].State.Visible){//child not dropped down
					continue;//to the next row. Don't increment yPos
				}
				yPos+=ListGridRows[i].State.HeightMain+ListGridRows[i].State.HeightNote;
			}
			DateTime dt2=DateTime.Now;
			TimeSpan timeSpanYpos=dt2-dt1;//put a breakpoint below this line to look at the timespans
			_heightTotal=yPos;
		}

		///<summary>Fills ListGridSheetRows with supplemental row information.</summary>
		private void ComputeSheetSupplemental() {
			ListGridSheetRows=new List<GridSheetRow>();
			bool drawTitle=false;
			bool drawHeader=true;
			bool drawFooter=false;
			int yPosCur=SheetYPos;
			int bottomCurPage=SheetPageHeight-SheetBottomMargin;
			while(yPosCur>bottomCurPage) {//advance pages until we are using correct y values. Example: grid starts on page three, yPosCur would be something like 2500
				bottomCurPage+=SheetPageHeight-(SheetTopMargin+SheetBottomMargin);
			}
			for(int i=0;i<ListGridRows.Count;i++) {
				#region Split patient accounts on Statement grids.
				if(i==0
					&& (_title.StartsWith("TreatPlanBenefitsFamily") 
					|| _title.StartsWith("TreatPlanBenefitsIndividual")
					|| _title.StartsWith("StatementPayPlanOld")
					|| _title.StartsWith("StatementPayPlan")
					|| _title.StartsWith("StatementDynamicPayPlan")
					|| _title.StartsWith("StatementPayPlanGrid")
					|| _title.StartsWith("StatementInvoicePayment")
					|| _title.StartsWith("StatementMain.NotIntermingled")))
				{
					drawTitle=true;
				}
				else if(_title.StartsWith("StatementMain.NotIntermingled") 
					&& i>0 
					&& ListGridRows[i].Tag.ToString()!=ListGridRows[i-1].Tag.ToString()) //Tag should be PatNum
				{
					yPosCur+=20; //space out grids.
					ListGridSheetRows[i-1].IsBottomRow=true;
					drawTitle=true;
					drawHeader=true;
				}
				#endregion
				#region Page break logic
				if(i==ListGridRows.Count-1 
					&& (_title.StartsWith("StatementPayPlanOld") 
						|| _title.StartsWith("StatementPayPlan") 
						|| _title.StartsWith("StatementDynamicPayPlan") 
						|| _title.StartsWith("StatementPayPlanGrid") 
						|| _title.StartsWith("StatementInvoicePayment")))
				{
					drawFooter=true;
				}
				if(yPosCur //start position of row
					+ListGridRows[i].State.HeightMain //+row height
					+(drawTitle?_heightTitlePrint:0) //+title height if needed
					+(drawHeader?_heightHeaderPrint:0) //+header height if needed
					+(drawFooter?_heightTitlePrint:0) //+footer height if needed.
					>=bottomCurPage) 
				{
					if(i>0) {
						ListGridSheetRows[i-1].IsBottomRow=true;//this row causes a page break. Previous row should be a bottom row.
					}
					yPosCur=bottomCurPage+1;
					bottomCurPage+=SheetPageHeight-(SheetTopMargin+SheetBottomMargin);
					drawHeader=true;
				}
				#endregion
				ListGridSheetRows.Add(new GridSheetRow(yPosCur,drawTitle,drawHeader,false,drawFooter));
				yPosCur+=(drawTitle?_heightTitlePrint:0);
				yPosCur+=(drawHeader?_heightHeaderPrint:0);
				yPosCur+=ListGridRows[i].State.HeightMain;
				yPosCur+=(drawFooter?_heightTitlePrint:0);
				drawTitle=drawHeader=drawFooter=false;//reset all flags for next row.
				if(i==ListGridRows.Count-1){//set print height equal to the bottom of the last row.
					ListGridSheetRows[i].IsBottomRow=true;
					SheetPrintHeight=yPosCur-SheetYPos;
				}
			}
		}

		private bool ControlIsDown() {
			return System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) || System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl);
		}

		///<summary>Creates combo boxes in the appropriate location of the grid so users can select and change them.</summary>
		private void CreateComboBox() {
			GridCell gridCell=ListGridRows[_selectedCell.Y].Cells[_selectedCell.X];
			GridColumn gridColumn=Columns[_selectedCell.X];
			comboBox.FlatStyle=FlatStyle.Popup;
			comboBox.DropDownStyle=ComboBoxStyle.DropDownList;
			int colWidth;
			if(gridColumn.DropDownWidth > 0){
				colWidth=gridColumn.DropDownWidth+1;
			}
			else{
				colWidth=Columns[_selectedCell.X].State.Width+1;
			}
			//int colWidth=(odGridColumn.DropDownWidth > 0) ? odGridColumn.DropDownWidth+1 : ListGridColumns[selectedCell.X].ColWidth+1;
			comboBox.Size=new Size(colWidth,ListGridRows[_selectedCell.Y].State.HeightMain+1);
			if(ListGridRows[_selectedCell.Y].State.YPos>=vScroll.Value) {//Top of row is showing. Put comboBox at top of row.
				comboBox.Location=new Point(-hScroll.Value+1+Columns[_selectedCell.X].State.XPos,-vScroll.Value+1+OriginY()+ListGridRows[_selectedCell.Y].State.YPos);
			}
			else {//The top of the row is up out of view. Put comboBox at top of grid, below header.
				comboBox.Location=new Point(-hScroll.Value+1+Columns[_selectedCell.X].State.XPos,1+OriginY());
			}
			comboBox.Items.Clear();
			for(int i=0;i<gridColumn.ListDisplayStrings.Count;i++) {
				comboBox.Items.Add(gridColumn.ListDisplayStrings[i]);
			}
			comboBox.SelectedIndex=gridCell.ComboSelectedIndex;
			comboBox.Visible=true;
			if(!this.Controls.Contains(comboBox)) {
				comboBox.SelectionChangeCommitted+=new EventHandler(comboBox_SelectionChangeCommitted);
				comboBox.GotFocus+=new EventHandler(comboBox_GotFocus);
				comboBox.LostFocus+=new EventHandler(comboBox_LostFocus);
				comboBox.KeyDown+=new KeyEventHandler(comboBox_KeyDown);
				this.Controls.Add(comboBox);
			}
			comboBox.Focus();
			_selectedCellOld=new Point(_selectedCell.X,_selectedCell.Y);
		}

		private void DropDownRowClick(int mouseDownRow,int mouseDownY) {
			int curRow=PointToRow(mouseDownY);
			if(curRow!=_mouseDownRow) {
				//if they click + dragged, don't expand/collapse the row they initially clicked on.
				return;
			}
			if(ListGridRows[mouseDownRow].State.DropDownState==ODGridDropDownState.Up) {
				ListGridRows[mouseDownRow].State.DropDownState=ODGridDropDownState.Down;
			}
			else if(ListGridRows[mouseDownRow].State.DropDownState==ODGridDropDownState.Down) {
				ListGridRows[mouseDownRow].State.DropDownState=ODGridDropDownState.Up;
			}
			using Graphics g=this.CreateGraphics();
			ComputeRows(g,doActualCalc:true);//We need to set visibility. This is not designed for huge numbers of rows in a grid.
			LayoutScrolls();
		}

		///<summary>So that scroll to end always works properly.</summary>
		private void FillLastPage(Graphics g){
			if(ListGridRows.Count==0){
				return;
			}
			int rowStart=0;//ListGridRows.Count-1;
			for(int i=ListGridRows.Count-1;i>=0;i--) {//start at the end and work backward
				if(ListGridRows[i].State.YPos+ListGridRows[i].State.HeightTotal > _heightTotal-vScroll.Height){//lower edge of row below top of grid area
					continue;
				}
				//we have found the row that would be at the top of the last page
				rowStart=i;
				break;
			}
			for(int i=rowStart;i<ListGridRows.Count;i++) {
				if(ListGridRows[i].Cells.Count==0){
					FillRow(i);	
				}
				ComputeRowHeightOne(g,i);
			}
			ComputeRowYposStartingAt(rowStart);
		}

		///<summary>Fills the cells.  Preserves State.YPos and HeightMain</summary>
		private void FillRow(int rowI){
			GridRow.GridRowState gridRowState=ListGridRows[rowI].State.Copy();
			if(_listDataRows!=null){
				ListGridRows[rowI]=FuncConstructGridRow(_listDataRows[rowI]);
			}
			ListGridRows[rowI].State.YPos=gridRowState.YPos;
			ListGridRows[rowI].State.HeightMain=gridRowState.HeightMain;
			//None of the other state fields have info in them yet
			//row height should be recalculated separately after this
		}

		///<summary>Converts from 96dpi to current total scale.</summary>
		private float ScaleF(float val96){
			float scaleTotal=_scaleMS*_zoomLocal;//example 1.5*1.2
			return val96*scaleTotal;
		}

		///<summary>Scales by (most of) the OD zoom scale.</summary>
		private float ScaleFontODZoom(float val96){
			return val96*_zoomLocal;
		}

		///<summary>Scales by just the MS scale.</summary>
		private float ScaleMS(float val96){
			return val96*_scaleMS;//1.5
		}

		///<summary>Converts from 96dpi to current total scale.</summary>
		private int ScaleI(float val96){
			//This is here because Dpi.Scale is over in OD. Here, Scale is already used by Windows, so we call it ScaleI.  A refactor could use OpenPmsDentalBusiness.UI.DpiScale, but that's too much work, with no benefit.
			float scaleTotal=_scaleMS*_zoomLocal;//example 1.5*1.2
			return (int)(Math.Round(val96*scaleTotal));
		}

		///<summary>The height of an editable row (text boxes, combo boxes and buttons need room), at the current scale: on screen it follows the cell
		///font (UI-5: today's 19 at 96 DPI, 22 with Segoe UI 9); printouts and sheets keep today's 19 (UQ-5).</summary>
		private int HeightEditableRow(){
			if(IsPrintFonts()) {
				return ScaleI(EDITABLE_ROW_HEIGHT);
			}
			return ScaleI(GridScreenLook.GetHeightEditableRow96());
		}

		///<summary>The top left corner of the grid area, right below the title and header.  Converted to current screen dpi.</summary>
		private int OriginY(){
			return ScaleI(_heightTitle+_heightHeader);
		}

		private bool ShiftIsDown() {
			return System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) || System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift);
		}

		private float UnscaleFontODZoom(float valScaled){
			return valScaled/_zoomLocal;
		}
		#endregion Methods - Private

		#region Class - GridColumnCollection
		///<summary>Nested class for the collection of GridColumns.</summary>
		public class GridColumnCollection{
			///<summary>The internal list of columns, exposed through methods.</summary>
			private List<GridColumn> _listGridColumns;
			///<summary>The gridOD that this collection is attached to.</summary>
			private GridOD _gridParent;

			public GridColumnCollection(GridOD grid){
				_listGridColumns=new List<GridColumn>();
				_gridParent=grid;
			}

			public int Count{
				get{
					return _listGridColumns.Count;
				}
			}

			///<summary></summary>
			public GridColumn this[int index]{
				get { 
					return _listGridColumns[index];
				}
			}

			public void Add(GridColumn gridColumn){
				_listGridColumns.Add(gridColumn);
				if(_gridParent._isUpdating){
					return;
				}
				_gridParent.ComputeColumns();

			}

			public void AddRange(IEnumerable<GridColumn> collection){
				_listGridColumns.AddRange(collection);
				if(_gridParent._isUpdating){
					return;
				}
				_gridParent.ComputeColumns();
			}

			public void Clear(){
				_listGridColumns.Clear();
				if(_gridParent._isUpdating){
					return;
				}
				_gridParent.ComputeColumns();
			}

			public List<GridColumn>.Enumerator GetEnumerator(){
				return _listGridColumns.GetEnumerator();
			}

			///<summary>Gets the index of the column with the specified heading.</summary>
			public int GetIndex(string heading){
				for(int i=0;i<_listGridColumns.Count;i++){
					if(_listGridColumns[i].Heading==heading){
						return i;
					}
				}
				return -1;//not found
			}

			public IEnumerable<T> Select<T>(Func<GridColumn,T> selector){
				return _listGridColumns.Select(selector);
			}

			public int Sum(Func<GridColumn,int> selector){
				return _listGridColumns.Sum(selector);
			}

			///<summary>Only use this only for read, not write.</summary>
			public List<GridColumn> ToList(){
				List<GridColumn> listGridColumns=new List<GridColumn>();
				for(int i=0;i<_listGridColumns.Count;i++){
					listGridColumns.Add(_listGridColumns[i].Copy());
				}
				return listGridColumns;
			}

			///<summary>Width of all columns summed.</summary>
		
			public int WidthAll(){
				int retVal=0;
				for(int i=0;i<_listGridColumns.Count;i++){
					retVal+=_listGridColumns[i].State.Width;
				}
				return retVal;
			}

		}
		#endregion Class - GridColumnCollection
	}

	#region Other Classes
	///<summary>This is the comboBox used in the grid.  We don't have access to ComboBoxOD from here.</summary>
	public class ComboBoxGrid:ComboBox{
		protected override bool IsInputKey(Keys keyData) {
			if(keyData==Keys.Tab){
				return true;//this allows the Tab key to hit our KeyDown event.
			}
			return base.IsInputKey(keyData);
		}
	}

	///<summary>When drawing to sheets, this stores additional information about a row.</summary>
	public class GridSheetRow {
		///<summary>YPos relative to top of entire grid.  When printing this includes adjustments for page breaks.  If row has title/header the title/header should be drawn at this position.</summary>
		public int YPos;
		///<summary>Usually only true for some grids, and only for the first row.</summary>
		public bool IsTitleRow;
		///<summary>Usually true if row is at the top of a new page, or when changing patients in a statement grid.</summary>
		public bool IsHeaderRow;
		///<summary>True for rows that require a bold bottom line, at end of entire grid, at page breaks, or at a separation in the grid.</summary>
		public bool IsBottomRow;
		///<summary>Rarely true, usually only for last row in particular grids.</summary>
		public bool IsFooterRow;

		public GridSheetRow(int yPos,bool isTitleRow,bool isHeaderRow,bool isBottomRow,bool isFooterRow) {
			YPos=yPos;
			IsTitleRow=isTitleRow;
			IsHeaderRow=isHeaderRow;
			IsBottomRow=isBottomRow;
			IsFooterRow=isFooterRow;
		}
	}

	///<summary></summary>
	public class ODGridClickEventArgs {
		///<summary></summary>
		public ODGridClickEventArgs(int col,int row,MouseButtons button) {
			this.Col=col;
			this.Row=row;
			this.Button=button;
		}
		public int Row { get; }
		public int Col { get; }
		///<summary>Gets which mouse button was pressed.</summary>
		public MouseButtons Button { get; }
	}

	///<summary></summary>
	public class ODGridKeyEventArgs {
		///<summary></summary>
		public ODGridKeyEventArgs(KeyEventArgs keyEventArgs) {
			this.KeyEventArgs=keyEventArgs;
		}
		///<summary>Gets which key was pressed.</summary>
		public KeyEventArgs KeyEventArgs { get; }

	}

	public class ODGridPageEventArgs {
		public int PageCur;
		public List<int> ListLinkVals;

		public ODGridPageEventArgs(int pageCur,List<int> listLinkVals) {
			this.PageCur=pageCur;
			this.ListLinkVals=listLinkVals;
		}
	}
	#endregion Other Classes

	#region enum GridSelectionMode
	///<summary>Specifies the selection behavior of a Grid.</summary>
	public enum GridSelectionMode {
		///<summary>Nothing can be selected.</summary>
		None,
		///<summary>Only one row can be selected.</summary>
		OneRow,
		///<summary>Only one cell can be selected.</summary>
		OneCell,
		///<summary>Multiple rows can be selected, and the user can use the SHIFT, CTRL, and arrow keys to make selections</summary>
		MultiExtended,
	}
	#endregion enum GridSelectionMode

	#region Delegates - Public
	//This is an obsolete pattern.  The new pattern is to use generic event handlers with custom event args.
	///<summary>Used for Cell specific events.</summary>
	public delegate void ODGridClickEventHandler(object sender,ODGridClickEventArgs e);
	///<summary>Used for Cell specific events.</summary>
	public delegate void ODGridKeyEventHandler(object sender,ODGridKeyEventArgs e);
	///<summary>Used when paging is enabled to update UI with current page and previous two / next two link information.</summary>
	public delegate void ODGridPageChangeEventHandler(object sender, ODGridPageEventArgs e);
	#endregion Delegates - Public
}

#region Jordans Notes
//I did testing with massive datasets, 8 columns, varying heights
//70k rows takes about 1/3 second to fill all the gridrows, 11 seconds to calculate their heights, and 0ms to set yPos on all rows.
//1M rows takes 10 seconds to fill the gridrows and I assume an unreasonable amount of time to calc heights.
//So the necessary strategy was obvious:
//1. Set all heights to a default of 1 row height instead of wasting time calculating.
//2. Never calculate row heights in advance, but only as they are visible.
//   a. They may change, such as dropdowns
//3. After calculating all row heights that are showing, update yPos for all rows showing and all below.(blazing fast)
//   Also, update total height, and if it changed, then layout scrollbar
//4. If there might be over 50k rows, then programmer might want to use Func to fill rows on the fly.  This is a separate issue from the above.
//5. If there are less than 2000 rows, then calculating heights will take less than 0.31 seconds.
//   So we will always calculate all row heights for less than 2000.  This prevents scroll bar jumping.
//   To prevent scrollbar jumping on massive grids, we would need a different way of calculating scrolling.
//The next remaining slow step, noticeable at about 100k rows, causes a pause of a few seconds when loading:
//List<DataRow> listDataRows=_table.Select().ToList();//We could work on eliminating it, but not urgent

//The general strategy for how the grid works internally is as follows:
//ComputeColumns as they are added and in EndUpdate.
//SetFonts as needed, but this should always be followed by the next two.
//ComputeRows (depends on fonts.  Follow with LayoutScrolls)
//LayoutScrolls (depends on _heightTotal from ComputeRows. It can be called without anything after it)
//Invalidate
//Paint only paints the rows that are showing

//2021-06-15-Todo:
//GridOld is not scaling properly, but that shouldn't matter, since it's not live anywhere.
//Test dropdown rows and text cells
//Convert Chart module to use the new grid
//Modernize the delegates


#endregion Jordans Notes

//2022-05-03-Nathan and others find the grid slightly annoying when using our custom query feature.
//Suggested improvements:
//Allow changing width of column with mouse.
//If one row is selected in multi-select mode, arrow buttons should change the selection.
//If multiple rows are selected, arrow down should select only one row, which is below the top selected row.






