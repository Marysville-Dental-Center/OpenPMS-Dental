/* ====================================================================
    Copyright (C) 2004-2006  fyiReporting Software, LLC

    This file is part of the fyiReporting RDL project.
	
    The RDL project is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

    For additional information, email info@fyireporting.com or visit
    the website www.fyiReporting.com.
*/
// Changed by Open PMS - Dental contributors, 2026-11-16 (package S3b-2b of the .NET 10 move, docs/port-notes/S3b-2.md part B): the toolbar is a
// Panel and the status bar a StatusStrip, in place of the WinForms ToolBar and StatusBar, which .NET has only as shims that cannot be created
// (WFDEV006).
// Changed by Open PMS - Dental contributors, 2026-11-16 (package S3b-FMT of the .NET 10 move, decision 24 of docs/specs/net10-plan.md,
// docs/port-notes/S3b-FMT.md): doubles and floats are written as text as .NET Framework wrote them (ODNumberText.ToStringNetFx, AsNetFx).
using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Xml;
using System.Globalization;
using System.Diagnostics;
using fyiReporting.RDL;
using fyiReporting.RdlViewer;
using System.Runtime.InteropServices;
using CodeBase;

namespace fyiReporting.RdlDesign
{
	/// <summary>
	/// RdlDesigner is used for building and previewing RDL based reports.
	/// </summary>
	public class RdlDesigner : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private MDIChild printChild=null;
		SortedList<DateTime, string> _RecentFiles=null;
		List<string> _CurrentFiles=null;		// temporary variable for current files
		List<string> _Toolbar = null;			// temporary variable for toolbar entries
		List<string> _TempReportFiles = null;		// temporary files created for report browsing
		int _RecentFilesMax = 5;			// # of items in recent files
		Process _ServerProcess = null;		// process for the RdlDesktop.exe --
		private RDL.NeedPassword _GetPassword;
		private string _DataSourceReferencePassword=null;
		private bool bGotPassword=false;
		private bool bMono = false;
		private readonly string DefaultHelpUrl="http://www.fyireporting.com/helpv2/index.html";
		private readonly string DefaultSupportUrl="http://www.fyireporting.com/forum";
		private string _HelpUrl;
		private string _SupportUrl;
		private DialogValidateRdl _ValidateRdl=null;
        private bool _ShowEditLines=true;
        private bool _ShowReportItemOutline = false;
        private bool _ShowTabbedInterface = true;
		private readonly string TEMPRDL = "_tempfile_.rdl";
		private int TEMPRDL_INC=0;

		// Status bar
		private System.Windows.Forms.StatusStrip mainSB;		// S3b-2b: a StatusStrip with ToolStripStatusLabels in place of the WinForms StatusBar (WFDEV006)
		private ToolStripStatusLabel statusPrimary;
		private ToolStripStatusLabel statusSelected;
		private ToolStripStatusLabel statusPosition;

		// Tool bar  --- if you add to this list LOOK AT INIT TOOLBAR FIRST
		bool bSuppressChange=false;
		private System.Windows.Forms.Panel mainTB;		// S3b-2b: a Panel docked at the top in place of the WinForms ToolBar (WFDEV006), which held only these controls
        internal System.Windows.Forms.TabControl mainTC; 
		private SimpleToggle ctlBold=null;
		private SimpleToggle ctlItalic=null;
		private SimpleToggle ctlUnderline=null;
		private ComboBox ctlFont=null;
		private ComboBox ctlFontSize=null;
		private ComboBox ctlForeColor=null;
		private ComboBox ctlBackColor=null;
		private Button ctlNew=null;
		private Button ctlOpen=null;
		private Button ctlSave=null;
		private Button ctlCut=null;
		private Button ctlCopy=null;
		private Button ctlUndo=null;
		private Button ctlPaste=null;
		private Button ctlPrint=null;
		private Button ctlPdf=null;
		private Button ctlXml=null;
		private Button ctlHtml=null;
		private Button ctlMht=null;
		private ComboBox ctlZoom=null;
		private SimpleToggle ctlInsertCurrent=null;
		private SimpleToggle ctlInsertTextbox=null;
		private SimpleToggle ctlInsertChart=null; 
		private SimpleToggle ctlInsertRectangle=null;
		private SimpleToggle ctlInsertTable=null;
		private SimpleToggle ctlInsertMatrix=null;
		private SimpleToggle ctlInsertList=null;
		private SimpleToggle ctlInsertLine=null;
		private SimpleToggle ctlInsertImage=null;
		private SimpleToggle ctlInsertSubreport=null;

		// Edit items
		private System.Windows.Forms.TextBox ctlEditTextbox=null;			// when you're editting textbox's
		private System.Windows.Forms.Label ctlEditLabel=null;

		// Menu Items
		ToolStripSeparator menuFSep1;
		ToolStripSeparator menuFSep2;
		ToolStripSeparator menuFSep3;
		ToolStripSeparator menuFSep4;
		// File
		ToolStripMenuItem menuNew;
		ToolStripMenuItem menuOpen;
		ToolStripMenuItem menuClose;
		ToolStripMenuItem menuSave;
		ToolStripMenuItem menuSaveAs;
		ToolStripMenuItem menuExport;
		ToolStripMenuItem menuExportXml;
		ToolStripMenuItem menuExportPdf;
		ToolStripMenuItem menuExportHtml;
		ToolStripMenuItem menuExportMHtml;
		ToolStripMenuItem menuPrint;
		ToolStripMenuItem menuRecentFile;
		ToolStripMenuItem menuExit;
		// Edit
		ToolStripMenuItem menuEdit;
		ToolStripMenuItem menuEditUndo;
		ToolStripMenuItem menuEditRedo;
		ToolStripMenuItem menuEditCut;
		ToolStripMenuItem menuEditCopy;
		ToolStripMenuItem menuEditPaste;
		ToolStripMenuItem menuEditDelete;
		ToolStripMenuItem menuEditSelectAll;
		ToolStripMenuItem menuEditFind;
		ToolStripMenuItem menuEditFindNext;
		ToolStripMenuItem menuEditReplace;
		ToolStripMenuItem menuEditGoto;
		ToolStripMenuItem menuEditFormatXml;
		// View
		ToolStripMenuItem menuView;
		ToolStripMenuItem menuViewDesigner;
		ToolStripMenuItem menuViewRDL;
		ToolStripMenuItem menuViewPreview;
		ToolStripMenuItem menuViewBrowser;
		ToolStripMenuItem menuViewProperties;
		// Data
		ToolStripMenuItem menuData;
		ToolStripMenuItem menuDataSources;
		ToolStripMenuItem menuDataSets;
		ToolStripMenuItem menuEmbeddedImages;
		ToolStripMenuItem menuNewDataSourceRef;
		// Format
		ToolStripMenuItem menuFormatAlign;
		ToolStripMenuItem menuFormatAlignL;
		ToolStripMenuItem menuFormatAlignC;
		ToolStripMenuItem menuFormatAlignR;
		ToolStripMenuItem menuFormatAlignT;
		ToolStripMenuItem menuFormatAlignM;
		ToolStripMenuItem menuFormatAlignB;

		ToolStripMenuItem menuFormatSize;
		ToolStripMenuItem menuFormatSizeW;
		ToolStripMenuItem menuFormatSizeH;
		ToolStripMenuItem menuFormatSizeB;

		ToolStripMenuItem menuFormatHorz;
		ToolStripMenuItem menuFormatHorzE;
		ToolStripMenuItem menuFormatHorzI;
		ToolStripMenuItem menuFormatHorzD;
		ToolStripMenuItem menuFormatHorzZ;

		ToolStripMenuItem menuFormatVert;
		ToolStripMenuItem menuFormatVertE;
		ToolStripMenuItem menuFormatVertI;
		ToolStripMenuItem menuFormatVertD;
		ToolStripMenuItem menuFormatVertZ;

		ToolStripMenuItem menuFormatPaddingLeft;
		ToolStripMenuItem menuFormatPaddingLeftI;
		ToolStripMenuItem menuFormatPaddingLeftD;
		ToolStripMenuItem menuFormatPaddingLeftZ;

		ToolStripMenuItem menuFormatPaddingRight;
		ToolStripMenuItem menuFormatPaddingRightI;
		ToolStripMenuItem menuFormatPaddingRightD;
		ToolStripMenuItem menuFormatPaddingRightZ;

		ToolStripMenuItem menuFormatPaddingTop;
		ToolStripMenuItem menuFormatPaddingTopI;
		ToolStripMenuItem menuFormatPaddingTopD;
		ToolStripMenuItem menuFormatPaddingTopZ;

		ToolStripMenuItem menuFormatPaddingBottom;
		ToolStripMenuItem menuFormatPaddingBottomI;
		ToolStripMenuItem menuFormatPaddingBottomD;
		ToolStripMenuItem menuFormatPaddingBottomZ;

		ToolStripMenuItem menuFormat;

		// Tools
		ToolStripMenuItem menuTools;
		ToolStripMenuItem menuToolsValidateSchema;
		ToolStripMenuItem menuToolsProcess;
		ToolStripMenuItem menuToolsOptions;

		// Window
		ToolStripMenuItem menuCascade;
		ToolStripMenuItem menuTileH;
		ToolStripMenuItem menuTileV;
		ToolStripMenuItem menuTile;
		private System.Windows.Forms.Button bTable;
		private System.Windows.Forms.Button bLine;
		private System.Windows.Forms.Button bImage;
		private System.Windows.Forms.Button bRectangle;
		private System.Windows.Forms.Button bSubreport;
		private System.Windows.Forms.Button bList;
		private System.Windows.Forms.Button bChart;
		private System.Windows.Forms.Button bText;
		private System.Windows.Forms.Button bMatrix;
		private System.Windows.Forms.Button bPrint;
		private System.Windows.Forms.Button bSave;
		private System.Windows.Forms.Button bOpen;
		private System.Windows.Forms.Button bPaste;
		private System.Windows.Forms.Button bCopy;
		private System.Windows.Forms.Button bCut;
		private System.Windows.Forms.Button bNew;
		private System.Windows.Forms.Button bUndo;
		private System.Windows.Forms.Button bPdf;
		private System.Windows.Forms.Button bXml;
		private System.Windows.Forms.Button bHtml;
		private System.Windows.Forms.Button bMht;
		ToolStripMenuItem menuCloseAll;

		public RdlDesigner(bool mono)
		{
			bMono = mono;
			GetStartupState();
			BuildMenus();
			InitializeComponent();
            
			this.MdiChildActivate +=new EventHandler(RdlDesigner_MdiChildActivate);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.RdlDesigner_Closing);
			_GetPassword = new RDL.NeedPassword(this.GetPassword);

			InitToolbar();
			InitStatusBar();
			this.Controls.Add(menuMain);		// last, so that it docks first, above the toolbar
			this.MainMenuStrip = menuMain;

			// open up the current files if any
			if (_CurrentFiles != null)
			{
				foreach (string file in _CurrentFiles)
				{
					CreateMDIChild(file, null, false);
				}
				_CurrentFiles = null;		// don't need this any longer
			}

			DesignTabChanged(this, new EventArgs());		// force toolbar to get updated
		}

		private void InitStatusBar()
		{
			// S3b-2b: a StatusStrip and three ToolStripStatusLabels in place of the WinForms StatusBar and its StatusBarPanels, which .NET has only
			// as shims that cannot be created (WFDEV006). Same panels, texts, tool tips, sizes and borders; the primary panel springs.
			mainSB = new StatusStrip();

			mainSB.Parent = this;
			mainSB.Dock = DockStyle.Bottom;
			mainSB.Anchor = AnchorStyles.Top | AnchorStyles.Left;

			statusPrimary = new ToolStripStatusLabel();
			statusPrimary.Width = 260;
			statusPrimary.Spring = true;
			statusPrimary.Text = "";
			statusPrimary.BorderSides = ToolStripStatusLabelBorderSides.None;

			statusSelected = new ToolStripStatusLabel();
			statusSelected.AutoSize = true;
			statusSelected.TextAlign = ContentAlignment.MiddleCenter;
			statusSelected.ToolTipText = "Name of selected ReportItem";
			statusSelected.Text = "";
			statusSelected.BorderSides = ToolStripStatusLabelBorderSides.All;
			statusSelected.BorderStyle = Border3DStyle.SunkenOuter;

			statusPosition = new ToolStripStatusLabel();
			statusPosition.AutoSize = true;
			statusPosition.ToolTipText = "Position of selected ReportItem";
			statusPosition.TextAlign = ContentAlignment.MiddleCenter;
			statusPosition.Text = "";
			statusPosition.BorderSides = ToolStripStatusLabelBorderSides.All;
			statusPosition.BorderStyle = Border3DStyle.RaisedInner;
			
			mainSB.Size = new System.Drawing.Size(472, 22);

			mainSB.ShowItemToolTips = true;
			mainSB.Location = new System.Drawing.Point(0, 282);
			mainSB.Name = "mainSB";
			mainSB.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
																	  this.statusPrimary,
																	  this.statusSelected,
																	  this.statusPosition});
			this.Controls.Add(mainSB);
		}

		private void InitToolbar()
		{
			bool bResumeLayout=false;
			// Clear out anything from before
			if (mainTB != null)
			{
				this.SuspendLayout();
				mainTB.SuspendLayout();
				bResumeLayout=true;
				mainTB.Controls.Clear();
				if (ctlBold!=null) {ctlBold=null;}
				if (ctlItalic!=null) {ctlItalic=null;}
				if (ctlUnderline!=null) {ctlUnderline=null;}
				if (ctlFont!=null) {ctlFont=null;}
				if (ctlFontSize!=null) {ctlFontSize=null;}
				if (ctlForeColor!=null) {ctlForeColor=null;}
				if (ctlBackColor!=null) {ctlBackColor=null;}
				if (ctlNew!=null) {ctlNew=null;}
				if (ctlOpen!=null) {ctlOpen=null;}
				if (ctlSave!=null) {ctlSave=null;}
				if (ctlCut!=null) {ctlCut=null;}
				if (ctlCopy!=null) {ctlCopy=null;}
				if (ctlUndo!=null) {ctlUndo=null;}
				if (ctlPaste!=null) {ctlPaste=null;}
				if (ctlPrint!=null) {ctlPrint=null;}
				if (ctlPdf!=null) {ctlPdf=null;}
				if (ctlXml!=null) {ctlXml=null;}
				if (ctlHtml!=null) {ctlHtml=null;}
				if (ctlMht!=null) {ctlMht=null;}
				if (ctlZoom!=null) {ctlZoom=null;}
				if (ctlInsertCurrent!=null) {ctlInsertCurrent=null;}
				if (ctlInsertTextbox!=null) {ctlInsertTextbox=null;}
				if (ctlInsertChart!=null) {ctlInsertChart=null;}
				if (ctlInsertRectangle!=null) {ctlInsertRectangle=null;}
				if (ctlInsertTable!=null) {ctlInsertTable=null;}
				if (ctlInsertMatrix!=null) {ctlInsertMatrix=null;}
				if (ctlInsertList!=null) {ctlInsertList=null;}
				if (ctlInsertLine!=null) {ctlInsertLine=null;}
				if (ctlInsertImage!=null) {ctlInsertImage=null;}
				if (ctlInsertSubreport!=null) {ctlInsertSubreport=null;}
				if (ctlEditTextbox!=null){ctlEditTextbox=null;}
				if (ctlEditLabel!=null){ctlEditLabel=null;}
			}
			else
			{
				mainTB = new Panel();		// S3b-2b: was a WinForms ToolBar (WFDEV006), which docks at the top
				mainTB.Dock = DockStyle.Top;
                mainTB.SizeChanged += new EventHandler(mainTB_SizeChanged);
				mainTB.SuspendLayout();
			}
			const int LINEHEIGHT = 22;
			const int LEFTMARGIN = 5;
			int y = 2;
			int x = LEFTMARGIN;

			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(RdlDesigner));

			// Build the controls the user wants
			foreach (string tbi in _Toolbar)
			{
				switch (tbi)
				{
					case "\n":
					case "Newline":
						y += LINEHEIGHT;
						x = LEFTMARGIN;
						break;
					case "Bold":
						x += InitToolbarBold(x,y);
						break;
					case "Italic":
						x += InitToolbarItalic(x,y);
						break;
					case "Underline":
						x += InitToolbarUnderline(x,y);
						break;
					case "Space":
						x += 5;
						break;
					case "Font":
						x += InitToolbarFont(x,y);
						break;
					case "FontSize":
						x += InitToolbarFontSize(x,y);
						break;
					case "ForeColor":
						ctlForeColor = InitToolbarColor(ref x, y, "Fore Color");
						ctlForeColor.SelectedValueChanged +=new EventHandler(ctlForeColor_Change);
						ctlForeColor.Validated +=new EventHandler(ctlForeColor_Change);
						break;
					case "BackColor":
						ctlBackColor = InitToolbarColor(ref x, y, "Back Color");
						ctlBackColor.SelectedValueChanged +=new EventHandler(ctlBackColor_Change);
						ctlBackColor.Validated +=new EventHandler(ctlBackColor_Change);
						break;
					case "New":
						ctlNew = InitToolbarMenu(ref x, y, "New", bNew.Image, new EventHandler(this.menuFileNewReport_Click));
						break;
					case "Open":
						ctlOpen = InitToolbarMenu(ref x, y, "Open", bOpen.Image, new EventHandler(this.menuFileOpen_Click));
						break;
					case "Save":
						ctlSave = InitToolbarMenu(ref x, y, "Save", bSave.Image, new EventHandler(this.menuFileSave_Click));
						break;
					case "Cut":
						ctlCut = InitToolbarMenu(ref x, y, "Cut", bCut.Image, new EventHandler(this.menuEditCut_Click));
						break;
					case "Copy":
						ctlCopy = InitToolbarMenu(ref x, y, "Copy", bCopy.Image, new EventHandler(this.menuEditCopy_Click));
						break;
					case "Undo":
						ctlUndo = InitToolbarMenu(ref x, y, "Undo", bUndo.Image, new EventHandler(this.menuEditUndo_Click));
						break;
					case "Paste":
						ctlPaste = InitToolbarMenu(ref x, y, "Paste", bPaste.Image, new EventHandler(this.menuEditPaste_Click));
						break;
					case "Print":
						ctlPrint = InitToolbarMenu(ref x, y, "Print", bPrint.Image, new EventHandler(this.menuFilePrint_Click));
						break;
					case "XML":
						ctlXml = InitToolbarMenu(ref x, y, "XML", bXml.Image, new EventHandler(this.menuExportXml_Click));
						break;
					case "PDF":
						ctlPdf = InitToolbarMenu(ref x, y, "PDF", bPdf.Image, new EventHandler(this.menuExportPdf_Click));
						break;
					case "HTML":
						ctlHtml = InitToolbarMenu(ref x, y, "HTML", bHtml.Image, new EventHandler(this.menuExportHtml_Click));
						break;
					case "MHT":
						ctlMht = InitToolbarMenu(ref x, y, "MHT", bMht.Image, new EventHandler(this.menuExportMHtml_Click));
						break;
					case "Edit":
						ctlEditTextbox = InitToolbarEditTextbox(ref x, y);
						break;
					case "Textbox":
						ctlInsertTextbox = InitToolbarInsertToogle(ref x, y, "Textbox", bText.Image);
						break;
					case "Chart":
						ctlInsertChart = InitToolbarInsertToogle(ref x, y, "Chart", bChart.Image);
						break;
					case "Rectangle":
						ctlInsertRectangle = InitToolbarInsertToogle(ref x, y, "Rectangle", bRectangle.Image);
						break;
					case "Table":
						ctlInsertTable = InitToolbarInsertToogle(ref x, y, "Table",	bTable.Image);
						break;
					case "Matrix":
						ctlInsertMatrix = InitToolbarInsertToogle(ref x, y, "Matrix", bMatrix.Image);
						break;
					case "List":
						ctlInsertList = InitToolbarInsertToogle(ref x, y, "List", bList.Image);
						break;
					case "Line":
						ctlInsertLine = InitToolbarInsertToogle(ref x, y, "Line", bLine.Image);
						break;
					case "Image":
						ctlInsertImage = InitToolbarInsertToogle(ref x, y, "Image", bImage.Image);
						break;
					case "Subreport":
						ctlInsertSubreport = InitToolbarInsertToogle(ref x, y, "Subreport", bSubreport.Image);
						break;
					case "Zoom":
						ctlZoom = InitToolbarZoom(ref x, y);
						ctlZoom.SelectedValueChanged +=new EventHandler(ctlZoom_Change);
						ctlZoom.Validated +=new EventHandler(ctlZoom_Change);
						break;
					default:
						break;
				}
			}
            
            // put the tab control in
            if (mainTC == null)
            {
                mainTC = new TabControl();
                mainTC.SelectedIndexChanged += new EventHandler(mainTC_SelectedIndexChanged);
                mainTC.ShowToolTips = true;
            }
            mainTC.Parent = mainTB;
            mainTC.Visible = _ShowTabbedInterface;
            if (_ShowTabbedInterface)
            {   // When tabbed we force the mdi children to be maximized (on reset)
                foreach (MDIChild mc in this.MdiChildren)
                {
                    mc.WindowState = FormWindowState.Maximized;
                }
            }
            mainTC.Location = new Point(0, y + LINEHEIGHT + 1);
            mainTC.Size = new Size(mainTB.Width, LINEHEIGHT);
            if (_ShowTabbedInterface)
                y += LINEHEIGHT;

            mainTB.Parent = this;
			mainTB.BorderStyle = BorderStyle.None;
			mainTB.Name = "mainTB";
			// (the ToolBar's ShowToolTips and DropDownArrows were for ToolBarButtons, of which it had none)
			//			mainTB.Size = new Size(440,20);
			mainTB.TabIndex = 1;
			mainTB.AutoSize = false;
			mainTB.Height = y + LINEHEIGHT + 1;     // 1 was 4	
			if (bResumeLayout)
			{
				mainTB.ResumeLayout();
				this.ResumeLayout();
			}
		}

        void mainTB_SizeChanged(object sender, EventArgs e)
        {
            mainTC.Width = mainTB.Width;
        }

         void mainTC_SelectedIndexChanged(object sender, EventArgs e)
        {
            MDIChild mc = mainTC.SelectedTab == null? null: mainTC.SelectedTab.Tag as MDIChild;
            mdi_Activate(mc);
        }

        [DllImport("user32.dll")]
        public static extern bool LockWindowUpdate(IntPtr hWndLock);


        void mdi_Activate(MDIChild mc)
        {
            if (mc == null)
                return;
            LockWindowUpdate(this.Handle);
            mc.Activate();
            this.Refresh(); //Forces a synchronous redraw of all controls

            LockWindowUpdate(IntPtr.Zero);
        }

		internal int RecentFilesMax
		{
			get {return _RecentFilesMax;}
			set {_RecentFilesMax = value;}
		}

		internal SortedList<DateTime, string> RecentFiles
		{
			get {return _RecentFiles;}
		}

		internal string HelpUrl
		{
			get 
			{
				if (_HelpUrl != null && _HelpUrl.Length > 0)
					return _HelpUrl;
				return DefaultHelpUrl;
			}
			set 
			{
				_HelpUrl = value.Length > 0? value: DefaultHelpUrl;
			}
		}

        internal bool ShowEditLines
		{
			get 
			{
                return _ShowEditLines;
			}
			set 
			{
                _ShowEditLines = value;
			}
		}

        internal bool ShowReportItemOutline
        {
            get
            {
                return _ShowReportItemOutline;
            }
            set
            {
                _ShowReportItemOutline = value;
            }
        }

        internal bool ShowTabbedInterface
        {
            get { return _ShowTabbedInterface; }
            set { _ShowTabbedInterface = value; }
        }

		internal string SupportUrl
		{
			get 
			{
				if (_SupportUrl != null && _SupportUrl.Length > 0)
					return _SupportUrl;
				return DefaultSupportUrl;
			}
			set 
			{
				_SupportUrl = value.Length > 0? value: DefaultSupportUrl;
			}
		}


		internal List<string> Toolbar
		{
			get {return _Toolbar;}
			set 
			{
				_Toolbar = value;
				InitToolbar();			// reset the toolbar
			}
		}

		internal List<string> ToolbarDefault
		{
			get 
			{
				return new List<string>(new string[] {
					"New", "Open", "Save", "Space", "Cut", "Copy", "Paste", "Undo", "Space", 
					"Textbox", "Chart", "Table", "List", "Image", "Matrix", "Subreport", 
					"Rectangle", "Line","Space","Edit", "Newline",
					"Bold", "Italic", "Underline", "Space",
					"Font", "FontSize", "Space", "ForeColor", "BackColor", 
					"Space", "Print", "Space", "Zoom", "Space", "PDF", "HTML", "XML", "MHT"});
				;
			}
		}

		internal List<string> ToolbarAllowDups
		{
			get 
			{
				return new List<string>(new string[] {
				  "Space",
				  "Newline"});
				;
			}
		}

		/// <summary>
		/// All of the possible items that can be placed on a toolbar
		/// </summary>
		internal string[] ToolbarOperations
		{
			get 
			{
				return new string[] 
					{"Newline",
					"Bold",
					"Italic",
					"Underline",
					"Space",
					"Font",
					"FontSize",
					"ForeColor",
					"BackColor",
					"New",
					"Open",
					"Save",
					"Cut",
					"Copy",
					"Undo",
					"Paste",
					"Print",
					"XML",
					"PDF",
					"HTML",
					"Edit",
					"Textbox",
					"Chart",
					"Rectangle",
					"Table",
					"Matrix",
					"List",
					"Line",
					"Image",
					"Subreport",
					"Zoom"   };
			}
		}

		private Button InitToolbarMenu(ref int x, int y, string t, System.EventHandler handler)
		{
			return InitToolbarMenu(ref x, y, t, null, handler);
		}

		private Button InitToolbarMenu(ref int x, int y, string t, Image img, System.EventHandler handler)
		{
			SimpleButton ctl = new SimpleButton(this);	 
			
			ctl.Click += handler;
			if (img == null)
			{
				ctl.Text = t;
				ctl.Font = new Font("Arial", 8, FontStyle.Regular);

				using ( Graphics g = ctl.CreateGraphics() )
				{
					SizeF fs = g.MeasureString( ctl.Text, ctl.Font);
					ctl.Height = (int) fs.Height+8;		// 8 is for margin so entire text shows in button
					ctl.Width = (int) fs.Width+4;		
					ctl.TextAlign = ContentAlignment.MiddleCenter;
				}
			}
			else
			{
				ctl.Image = img;
				ctl.ImageAlign = ContentAlignment.MiddleCenter;
				ctl.Height = img.Height + 5;
				ctl.Width = img.Width + 8;
				ctl.Text = "";
			}
			ctl.Tag = t;
			ctl.Left = x;
			ctl.Top = y;
			ctl.FlatStyle = FlatStyle.Flat;
			ToolTip tip = new ToolTip();
			tip.AutomaticDelay = 500;
			tip.ShowAlways = true;
			tip.SetToolTip(ctl, t);
			mainTB.Controls.Add(ctl);

			x = x+ ctl.Width;

			return ctl;
		}
		
		private System.Windows.Forms.TextBox InitToolbarEditTextbox(ref int x, int y)
		{
			System.Windows.Forms.TextBox tb = new System.Windows.Forms.TextBox();
			Label l = this.ctlEditLabel = new Label();
			l.AutoSize = true;
			l.Font = new Font("Arial", 8, FontStyle.Bold | FontStyle.Italic);
			l.Text = "fx";
			l.Left = x;
			l.Top = y + (tb.Height / 2) - (l.Height / 2);
			mainTB.Controls.Add(l);
			x += l.Width;
			tb.Left = x;
			tb.Width = 230;
			x += tb.Width;
			tb.Top = y;
			tb.Validated += new EventHandler(this.EditTextbox_Validated);	// handler for edit changes
			tb.KeyDown += new KeyEventHandler(EditTextBox_KeyDown);

			mainTB.Controls.Add(tb);
			return tb;
		}

		private SimpleToggle InitToolbarInsertToogle(ref int x, int y, string t)
		{
			return InitToolbarInsertToogle(ref x, y, t, null);
		}

		private SimpleToggle InitToolbarInsertToogle(ref int x, int y, string t,
			Image bImg)
		{
			SimpleToggle ctl = new SimpleToggle();
			ctl.UpColor = this.BackColor;
			ctl.Click +=new EventHandler(this.Insert_Click);	// click handler for all inserts

			if (bImg == null)
			{
				ctl.Text = t;
				using ( Graphics g = ctl.CreateGraphics() )
				{
					SizeF fs = g.MeasureString( ctl.Text, ctl.Font);
					ctl.Height = (int) fs.Height + 8;	// 8 is for margins
					ctl.Width = (int) fs.Width + 12;		
				}
			}
			else
			{
				ctl.Image = bImg;
				ctl.ImageAlign = ContentAlignment.MiddleCenter;
				ctl.Height = bImg.Height + 5;
				ctl.Width = bImg.Width + 8;
				ctl.Text = "";
			}

			ctl.Tag = t;
			ctl.Left = x;
			ctl.Top = y;
			ctl.FlatStyle = FlatStyle.Flat;
			ToolTip tipb = new ToolTip();
			tipb.AutomaticDelay = 500;
			tipb.ShowAlways = true;
			tipb.SetToolTip(ctl, t);
			mainTB.Controls.Add(ctl);

			x = x+ ctl.Width;

			return ctl;
		}

		private int InitToolbarBold(int x, int y)
		{
			ctlBold = new SimpleToggle();
			ctlBold.UpColor = this.BackColor;

			ctlBold.Click +=new EventHandler(ctlBold_Click);
			
			ctlBold.Font = new Font("Courier New", 10, FontStyle.Bold);
			ctlBold.Text = "B";
			using ( Graphics g = ctlBold.CreateGraphics() )
			{
				SizeF fs = g.MeasureString( ctlBold.Text, ctlBold.Font );
				ctlBold.Height = (int) fs.Height + 6;	// 6 is for margins
				ctlBold.Width = ctlBold.Height;		
			}

			ctlBold.Tag = "bold";
			ctlBold.Left = x;
			ctlBold.Top = y;
			ctlBold.FlatStyle = FlatStyle.Flat;
			ctlBold.TextAlign = ContentAlignment.MiddleCenter;

			ToolTip tipb = new ToolTip();
			tipb.AutomaticDelay = 500;
			tipb.ShowAlways = true;
			tipb.SetToolTip(ctlBold, "Bold");
			mainTB.Controls.Add(ctlBold);

			return ctlBold.Width;
		}

		private int InitToolbarItalic(int x, int y)
		{
			ctlItalic = new SimpleToggle();
			ctlItalic.UpColor = this.BackColor;
			ctlItalic.Click +=new EventHandler(ctlItalic_Click);

			ctlItalic.Font = new Font("Courier New", 10, FontStyle.Italic | FontStyle.Bold);
			ctlItalic.Text = "I";
			using ( Graphics g = ctlItalic.CreateGraphics() )
			{
				SizeF fs = g.MeasureString( ctlItalic.Text, ctlItalic.Font );
				ctlItalic.Height = (int) fs.Height + 6;	// 6 is for margins
				ctlItalic.Width = ctlItalic.Height;		
			}

			ctlItalic.Tag = "italic";
			ctlItalic.Left = x;
			ctlItalic.Top = y;
			ctlItalic.FlatStyle = FlatStyle.Flat;
			ToolTip tipb = new ToolTip();
			tipb.AutomaticDelay = 500;
			tipb.ShowAlways = true;
			tipb.SetToolTip(ctlItalic, "Italic");
			mainTB.Controls.Add(ctlItalic);

			return ctlItalic.Width;
		}

		private int InitToolbarUnderline(int x, int y)
		{
			ctlUnderline = new SimpleToggle();
			ctlUnderline.UpColor = this.BackColor;
			ctlUnderline.Click +=new EventHandler(ctlUnderline_Click);
			ctlUnderline.Font = new Font("Courier New", 10, FontStyle.Underline | FontStyle.Bold);
			ctlUnderline.Text = "U";
			using ( Graphics g = ctlUnderline.CreateGraphics() )
			{
				SizeF fs = g.MeasureString( ctlUnderline.Text, ctlUnderline.Font );
				ctlUnderline.Height = (int) fs.Height + 6;	// 6 is for margins
				ctlUnderline.Width = ctlUnderline.Height;		
			}

			ctlUnderline.Tag = "italic";
			ctlUnderline.Left = x;
			ctlUnderline.Top = y;
			ctlUnderline.FlatStyle = FlatStyle.Flat;
			ToolTip tipb = new ToolTip();
			tipb.AutomaticDelay = 500;
			tipb.ShowAlways = true;
			tipb.SetToolTip(ctlUnderline, "Underline");
			mainTB.Controls.Add(ctlUnderline);

			return ctlUnderline.Width;
		}

		private int InitToolbarFont(int x, int y)
		{
			// Create the font
			ctlFont = new ComboBox();
			ctlFont.SelectedValueChanged +=new EventHandler(ctlFont_Change);
			ctlFont.Validated +=new EventHandler(ctlFont_Change);
			ctlFont.Left = x;
			ctlFont.Top = y;
			ctlFont.DropDownStyle = ComboBoxStyle.DropDown;

			foreach (FontFamily ff in FontFamily.Families)
			{
				ctlFont.Items.Add(ff.Name);
			}
			ToolTip tip = new ToolTip();
			tip.AutomaticDelay = 500;
			tip.ShowAlways = true;
			tip.SetToolTip(ctlFont, "Font");
			mainTB.Controls.Add(ctlFont);
			
			return ctlFont.Width;
		}

		private int InitToolbarFontSize(int x, int y)
		{
			// Create the font
			ctlFontSize = new ComboBox();
			ctlFontSize.SelectedValueChanged +=new EventHandler(ctlFontSize_Change);
			ctlFontSize.Validated +=new EventHandler(ctlFontSize_Change);
			ctlFontSize.Width = 42;
			ctlFontSize.Left = x;
			ctlFontSize.Top = y;
			ctlFontSize.DropDownStyle = ComboBoxStyle.DropDown;

			string[] sizes = new string[] {"8","9","10","11","12","14","16","18","20","22","24","26","28","36","48","72"};
			ctlFontSize.Items.AddRange(sizes);
			ToolTip tip = new ToolTip();
			tip.AutomaticDelay = 500;
			tip.ShowAlways = true;
			tip.SetToolTip(ctlFontSize, "Font Size");
			mainTB.Controls.Add(ctlFontSize);

			return ctlFontSize.Width;
		}

		private ComboBox InitToolbarColor(ref int x, int y, string t)
		{
			// Create the font
			ComboBox ctl = new ComboBox();
			ctl.Width = 100;
			ctl.Left = x;
			ctl.Top = y;
			ctl.Tag = t;
			ctl.DropDownStyle = ComboBoxStyle.DropDown;

			ctl.Items.AddRange(StaticLists.ColorList);
			ToolTip tip = new ToolTip();
			tip.AutomaticDelay = 500;
			tip.ShowAlways = true;
			tip.SetToolTip(ctl, t);
			mainTB.Controls.Add(ctl);

			x += ctl.Width;
			return ctl;
		}

		private ComboBox InitToolbarZoom(ref int x, int y)
		{
			ComboBox ctl = new ComboBox();
			ctl.Width = 85;
			ctl.Left = x;
			ctl.Top = y;
			ctl.Tag = "Zoom";
			ctl.DropDownStyle = ComboBoxStyle.DropDownList;

			ctl.Items.AddRange(StaticLists.ZoomList);
			ToolTip tip = new ToolTip();
			tip.AutomaticDelay = 500;
			tip.ShowAlways = true;
			tip.SetToolTip(ctl, "Zoom");
			mainTB.Controls.Add(ctl);

			x += ctl.Width;
			return ctl;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(RdlDesigner));
			this.bTable = new System.Windows.Forms.Button();
			this.bLine = new System.Windows.Forms.Button();
			this.bImage = new System.Windows.Forms.Button();
			this.bRectangle = new System.Windows.Forms.Button();
			this.bSubreport = new System.Windows.Forms.Button();
			this.bList = new System.Windows.Forms.Button();
			this.bChart = new System.Windows.Forms.Button();
			this.bText = new System.Windows.Forms.Button();
			this.bMatrix = new System.Windows.Forms.Button();
			this.bPrint = new System.Windows.Forms.Button();
			this.bSave = new System.Windows.Forms.Button();
			this.bOpen = new System.Windows.Forms.Button();
			this.bPaste = new System.Windows.Forms.Button();
			this.bCopy = new System.Windows.Forms.Button();
			this.bCut = new System.Windows.Forms.Button();
			this.bNew = new System.Windows.Forms.Button();
			this.bUndo = new System.Windows.Forms.Button();
			this.bPdf = new System.Windows.Forms.Button();
			this.bXml = new System.Windows.Forms.Button();
			this.bHtml = new System.Windows.Forms.Button();
			this.bMht = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// bTable
			// 
			this.bTable.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Table",16,16);
			this.bTable.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bTable.Location = new System.Drawing.Point(608, 32);
			this.bTable.Name = "bTable";
			this.bTable.TabIndex = 0;
			this.bTable.Text = "Table";
			this.bTable.Visible = false;
			// 
			// bLine
			// 
			this.bLine.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Line",16,16);
			this.bLine.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bLine.Location = new System.Drawing.Point(608, 288);
			this.bLine.Name = "bLine";
			this.bLine.TabIndex = 1;
			this.bLine.Text = "Line";
			this.bLine.Visible = false;
			// 
			// bImage
			// 
			this.bImage.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Image",16,16);
			this.bImage.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bImage.Location = new System.Drawing.Point(608, 256);
			this.bImage.Name = "bImage";
			this.bImage.TabIndex = 2;
			this.bImage.Text = "Image";
			this.bImage.Visible = false;
			// 
			// bRectangle
			// 
			this.bRectangle.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Rectangle",16,16);
			this.bRectangle.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bRectangle.Location = new System.Drawing.Point(608, 224);
			this.bRectangle.Name = "bRectangle";
			this.bRectangle.TabIndex = 3;
			this.bRectangle.Text = "Rect";
			this.bRectangle.Visible = false;
			// 
			// bSubreport
			// 
			this.bSubreport.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Subreport",16,16);
			this.bSubreport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bSubreport.Location = new System.Drawing.Point(608, 192);
			this.bSubreport.Name = "bSubreport";
			this.bSubreport.TabIndex = 4;
			this.bSubreport.Text = "Subreport";
			this.bSubreport.Visible = false;
			// 
			// bList
			// 
			this.bList.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","ListBullets",16,16);
			this.bList.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bList.Location = new System.Drawing.Point(608, 160);
			this.bList.Name = "bList";
			this.bList.TabIndex = 5;
			this.bList.Text = "List";
			this.bList.Visible = false;
			// 
			// bChart
			// 
			this.bChart.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","ChartBar",16,16);
			this.bChart.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bChart.Location = new System.Drawing.Point(608, 128);
			this.bChart.Name = "bChart";
			this.bChart.TabIndex = 6;
			this.bChart.Text = "Chart";
			this.bChart.Visible = false;
			// 
			// bText
			// 
			this.bText.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","TextBox",16,16);
			this.bText.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bText.Location = new System.Drawing.Point(608, 96);
			this.bText.Name = "bText";
			this.bText.TabIndex = 7;
			this.bText.Text = "Text";
			this.bText.Visible = false;
			// 
			// bMatrix
			// 
			this.bMatrix.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Matrix",16,16);
			this.bMatrix.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bMatrix.Location = new System.Drawing.Point(608, 64);
			this.bMatrix.Name = "bMatrix";
			this.bMatrix.TabIndex = 8;
			this.bMatrix.Text = "Matrix";
			this.bMatrix.Visible = false;
			// 
			// bPrint
			// 
			this.bPrint.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Print",16,16);
			this.bPrint.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bPrint.Location = new System.Drawing.Point(504, 192);
			this.bPrint.Name = "bPrint";
			this.bPrint.TabIndex = 9;
			this.bPrint.Text = "Print";
			this.bPrint.Visible = false;
			// 
			// bSave
			// 
			this.bSave.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Save",16,16);
			this.bSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bSave.Location = new System.Drawing.Point(504, 160);
			this.bSave.Name = "bSave";
			this.bSave.TabIndex = 10;
			this.bSave.Text = "Save";
			this.bSave.Visible = false;
			// 
			// bOpen
			// 
			this.bOpen.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Open",16,16);
			this.bOpen.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bOpen.Location = new System.Drawing.Point(504, 128);
			this.bOpen.Name = "bOpen";
			this.bOpen.TabIndex = 11;
			this.bOpen.Text = "Open";
			this.bOpen.Visible = false;
			// 
			// bPaste
			// 
			this.bPaste.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Paste",16,16);
			this.bPaste.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bPaste.Location = new System.Drawing.Point(504, 96);
			this.bPaste.Name = "bPaste";
			this.bPaste.TabIndex = 12;
			this.bPaste.Text = "Paste";
			this.bPaste.Visible = false;
			// 
			// bCopy
			// 
			this.bCopy.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Copy",16,16);
			this.bCopy.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bCopy.Location = new System.Drawing.Point(504, 64);
			this.bCopy.Name = "bCopy";
			this.bCopy.TabIndex = 13;
			this.bCopy.Text = "Copy";
			this.bCopy.Visible = false;
			// 
			// bCut
			// 
			this.bCut.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Cut",16,16);
			this.bCut.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bCut.Location = new System.Drawing.Point(504, 32);
			this.bCut.Name = "bCut";
			this.bCut.TabIndex = 14;
			this.bCut.Text = "Cut";
			this.bCut.Visible = false;
			// 
			// bNew
			// 
			this.bNew.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","NewDocument",16,16);
			this.bNew.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bNew.Location = new System.Drawing.Point(504, 224);
			this.bNew.Name = "bNew";
			this.bNew.TabIndex = 15;
			this.bNew.Text = "New";
			this.bNew.Visible = false;
			// 
			// bUndo
			// 
			this.bUndo.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Undo",16,16);
			this.bUndo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bUndo.Location = new System.Drawing.Point(504, 256);
			this.bUndo.Name = "bUndo";
			this.bUndo.TabIndex = 16;
			this.bUndo.Text = "Undo";
			this.bUndo.Visible = false;
			// 
			// bPdf
			// 
			this.bPdf.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Pdf",18,16);
			this.bPdf.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bPdf.Location = new System.Drawing.Point(504, 320);
			this.bPdf.Name = "bPdf";
			this.bPdf.TabIndex = 17;
			this.bPdf.Text = "PDF";
			this.bPdf.Visible = false;
			// 
			// bXml
			// 
			this.bXml.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Xml",16,16);
			this.bXml.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bXml.Location = new System.Drawing.Point(504, 288);
			this.bXml.Name = "bXml";
			this.bXml.TabIndex = 18;
			this.bXml.Text = "XML";
			this.bXml.Visible = false;
			// 
			// bHtml
			// 
			this.bHtml.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Html",16,16);
			this.bHtml.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bHtml.Location = new System.Drawing.Point(608, 320);
			this.bHtml.Name = "bHtml";
			this.bHtml.TabIndex = 19;
			this.bHtml.Text = "HTML";
			this.bHtml.Visible = false;
			// 
			// bMht
			// 
			this.bMht.Image = global::CodeBase.UiIcons.GetBitmap("EnumIcons","Mht",16,16);
			this.bMht.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bMht.Location = new System.Drawing.Point(504, 360);
			this.bMht.Name = "bMht";
			this.bMht.TabIndex = 20;
			this.bMht.Text = "HTML";
			this.bMht.Visible = false;
			// 
			// RdlDesigner
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(712, 470);
			this.Controls.Add(this.bMht);
			this.Controls.Add(this.bHtml);
			this.Controls.Add(this.bXml);
			this.Controls.Add(this.bPdf);
			this.Controls.Add(this.bUndo);
			this.Controls.Add(this.bNew);
			this.Controls.Add(this.bCut);
			this.Controls.Add(this.bCopy);
			this.Controls.Add(this.bPaste);
			this.Controls.Add(this.bOpen);
			this.Controls.Add(this.bSave);
			this.Controls.Add(this.bPrint);
			this.Controls.Add(this.bMatrix);
			this.Controls.Add(this.bText);
			this.Controls.Add(this.bChart);
			this.Controls.Add(this.bList);
			this.Controls.Add(this.bSubreport);
			this.Controls.Add(this.bRectangle);
			this.Controls.Add(this.bImage);
			this.Controls.Add(this.bLine);
			this.Controls.Add(this.bTable);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "RdlDesigner";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "fyiReporting Designer";
			this.ResumeLayout(false);

		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			//Before anything creates a window (S3b-4 of docs/specs/net10-plan.md): the .NET Framework's default font, which the forms were laid out
			//with (.NET's is Segoe UI 9; SetDefaultFont throws once a window exists), then per-monitor DPI awareness, the program's mode (the
			//.NET Framework build of the program set it in its app.config, which .NET does not read).
			Application.SetDefaultFont(new System.Drawing.Font("Microsoft Sans Serif",8.25f));
			Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
			//The UI refresh's token renderer (UI-6, docs/specs/ui-refresh-plan.md section 3.6) for every stock menu and tool strip of this thread.
			CodeBase.UiTheme.InitWinForms();
			//The Windows code pages for Encoding.GetEncoding (.NET has only the Unicode encodings, ASCII and Latin-1 built in), before anything reads
			//ANSI text. This program does not call ODInitialize.Initialize, which registers them for the others (S3b-4).
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
			bool bMono;
			string[] args = Environment.GetCommandLineArgs();
			if (args.Length >= 2 &&
				(args[1].ToLower() == "/m" || args[1].ToLower() == "-m"))
			{	// user want to run with mono simplifications
				bMono = true;
			}
			else
			{
				bMono = false;
				Application.EnableVisualStyles();
				Application.DoEvents();				// when Mono this goes into a loop
			}
			Application.Run(new RdlDesigner(bMono));
		}

		private void BuildMenus()
		{
			// FILE MENU
			menuNew = new ToolStripMenuItem("&New Report...", null, new EventHandler(this.menuFileNewReport_Click), (Keys.Control|Keys.N));
			menuOpen = new ToolStripMenuItem("&Open...", null, new EventHandler(this.menuFileOpen_Click), (Keys.Control|Keys.O));
			menuClose = new ToolStripMenuItem("&Close", null, new EventHandler(this.menuFileClose_Click), (Keys.Control|Keys.W));
			menuFSep1 = new ToolStripSeparator();
			menuSave = new ToolStripMenuItem("&Save", null, new EventHandler(this.menuFileSave_Click), (Keys.Control|Keys.S));
			menuSaveAs = new ToolStripMenuItem("Save &As...", null, new EventHandler(this.menuFileSaveAs_Click));
			menuPrint = new ToolStripMenuItem("Print...", null, new EventHandler(this.menuFilePrint_Click), (Keys.Control|Keys.P));
			menuExport = new ToolStripMenuItem("Export");
			menuExportPdf =  new ToolStripMenuItem("PDF...", null, new EventHandler(this.menuExportPdf_Click));
			menuExportXml =  new ToolStripMenuItem("XML...", null, new EventHandler(this.menuExportXml_Click));
			menuExportHtml =  new ToolStripMenuItem("Web Page, HTML...", null, new EventHandler(this.menuExportHtml_Click));
			menuExportMHtml =  new ToolStripMenuItem("Web Archive, single file MHT...", null, new EventHandler(this.menuExportMHtml_Click));
			menuExport.DropDownItems.AddRange(new ToolStripItem[] {menuExportPdf, menuExportHtml, menuExportMHtml, menuExportXml});

			menuFSep2 = new ToolStripSeparator();
			ToolStripMenuItem menuRecentItem = new ToolStripMenuItem("");
			menuRecentFile = new ToolStripMenuItem("Recent &Files");
			menuRecentFile.DropDownItems.AddRange(new ToolStripItem[] { menuRecentItem });
			menuFSep3 = new ToolStripSeparator();
			menuExit = new ToolStripMenuItem("E&xit", null, new EventHandler(this.menuFileExit_Click), (Keys.Control|Keys.Q));
			   
			// Create file menu and add array of sub-menu items
			ToolStripMenuItem menuFile = new ToolStripMenuItem("&File");
			menuFile.DropDownOpening +=new EventHandler(this.menuFile_Popup);
			menuFile.DropDownItems.AddRange(
				new ToolStripItem[] { menuNew, menuOpen, menuClose, menuFSep1, menuSave, menuSaveAs, 
								   menuPrint, menuExport, menuFSep2, menuRecentFile, menuFSep3, menuExit });

			// Intialize the recent file menu
			RecentFilesMenu();

			// EDIT MENU
			menuEditUndo = new ToolStripMenuItem("&Undo", null, new EventHandler(this.menuEditUndo_Click), (Keys.Control|Keys.Z));
			menuEditRedo = new ToolStripMenuItem("&Redo", null, new EventHandler(this.menuEditRedo_Click), (Keys.Control|Keys.Y));
			menuFSep1 = new ToolStripSeparator();
			menuEditCut = new ToolStripMenuItem("Cu&t", null, new EventHandler(this.menuEditCut_Click), (Keys.Control|Keys.X));
			menuEditCopy = new ToolStripMenuItem("&Copy", null, new EventHandler(this.menuEditCopy_Click), (Keys.Control|Keys.C));
			menuEditPaste = new ToolStripMenuItem("&Paste", null, new EventHandler(this.menuEditPaste_Click), (Keys.Control|Keys.V));
			menuEditDelete = new ToolStripMenuItem("&Delete", null, new EventHandler(this.menuEditDelete_Click));
			menuFSep2 = new ToolStripSeparator();
			menuEditSelectAll = new ToolStripMenuItem("Select &All", null, new EventHandler(this.menuEditSelectAll_Click), (Keys.Control|Keys.A));
			menuFSep3 = new ToolStripSeparator();
			menuEditFind = new ToolStripMenuItem("&Find...", null, new EventHandler(this.menuEditFind_Click), (Keys.Control|Keys.F));
			menuEditFindNext = new ToolStripMenuItem("Find Next", null, new EventHandler(this.menuEditFindNext_Click), Keys.F3);
			menuEditReplace = new ToolStripMenuItem("&Replace...", null, new EventHandler(this.menuEditReplace_Click), (Keys.Control|Keys.H));
			menuEditGoto = new ToolStripMenuItem("&Go To...", null, new EventHandler(this.menuEditGoto_Click), (Keys.Control|Keys.G));
			menuFSep4 = new ToolStripSeparator();
			menuEditFormatXml = new ToolStripMenuItem("Format XM&L", null, new EventHandler(this.menuEdit_FormatXml), (Keys.Control|Keys.L));
			// Create edit menu and add array of sub-menu items
			menuEdit = new ToolStripMenuItem("&Edit");
			menuEdit.DropDownOpening +=new EventHandler(this.menuEdit_Popup);
			
			menuEdit.DropDownItems.AddRange(
				new ToolStripItem[] { menuEditUndo, menuEditRedo, menuFSep1, menuEditCut, menuEditPaste, 
								   menuEditDelete, menuFSep2, menuEditSelectAll, menuFSep3,
								   menuEditFind, menuEditFindNext, menuEditReplace, menuEditGoto,
								   menuFSep4, menuEditFormatXml});
			// View Menu
			menuViewDesigner = new ToolStripMenuItem("Designer", null, new EventHandler(this.menuViewDesigner_Click), Keys.F7);
			menuViewRDL = new ToolStripMenuItem("RDL Text", null, new EventHandler(this.menuViewRDL_Click), (Keys.Shift|Keys.F7));
			menuViewPreview = new ToolStripMenuItem("Preview", null, new EventHandler(this.menuViewPreview_Click), Keys.F5);
			menuViewBrowser = new ToolStripMenuItem("Show Report in Browser", null, new EventHandler(this.menuViewBrowser_Click), Keys.F6);
			menuViewProperties = new ToolStripMenuItem("Properties...", null, new EventHandler(this.menuEditProperties_Click), Keys.F4);
			menuView = new ToolStripMenuItem("&View");
			menuView.DropDownOpening += new EventHandler(menuView_Popup);
			menuView.DropDownItems.AddRange(
				new ToolStripItem[] {menuViewDesigner, menuViewRDL, menuViewPreview, 
							  new ToolStripSeparator(), menuViewBrowser, new ToolStripSeparator(), menuViewProperties});
			
			// Data Menu
			menuNewDataSourceRef = new ToolStripMenuItem("&Create Shared Data Source...", null, new EventHandler(this.menuFileNewDataSourceRef_Click));
			menuDataSources = new ToolStripMenuItem("Data &Sources...", null, new EventHandler(this.menuDataSources_Click));
			menuDataSets = new ToolStripMenuItem("&Data Sets");
			menuDataSets.DropDownItems.Add(new ToolStripMenuItem());	// this will actually get build dynamically

			menuEmbeddedImages = new ToolStripMenuItem("&Embedded Images...", null, new EventHandler(this.menuEmbeddedImages_Click));
			menuData = new ToolStripMenuItem("&Data");
			menuData.DropDownOpening +=new EventHandler(this.menuData_Popup);
			menuData.DropDownItems.AddRange(
				new ToolStripItem[] { menuDataSets, menuDataSources, new ToolStripSeparator(), menuEmbeddedImages, new ToolStripSeparator(), menuNewDataSourceRef });

			// Format menu
			menuFormatAlign = new ToolStripMenuItem("&Align");
			menuFormatAlignL = new ToolStripMenuItem("&Lefts", null, new EventHandler(this.menuFormatAlignL_Click));
			menuFormatAlignC = new ToolStripMenuItem("&Centers", null, new EventHandler(this.menuFormatAlignC_Click));
			menuFormatAlignR = new ToolStripMenuItem("&Rights", null, new EventHandler(this.menuFormatAlignR_Click));
			menuFormatAlignT = new ToolStripMenuItem("&Tops", null, new EventHandler(this.menuFormatAlignT_Click));
			menuFormatAlignM = new ToolStripMenuItem("&Middles", null, new EventHandler(this.menuFormatAlignM_Click));
			menuFormatAlignB = new ToolStripMenuItem("&Bottoms", null, new EventHandler(this.menuFormatAlignB_Click));
			menuFormatAlign.DropDownItems.AddRange(
				new ToolStripItem[] {menuFormatAlignL,menuFormatAlignC,menuFormatAlignR,new ToolStripSeparator(),menuFormatAlignT,menuFormatAlignM,menuFormatAlignB });

			menuFormatSize = new ToolStripMenuItem("&Size");
			menuFormatSizeW = new ToolStripMenuItem("&Width", null, new EventHandler(this.menuFormatSizeW_Click));
			menuFormatSizeH = new ToolStripMenuItem("&Height", null, new EventHandler(this.menuFormatSizeH_Click));
			menuFormatSizeB = new ToolStripMenuItem("&Both", null, new EventHandler(this.menuFormatSizeB_Click));
			menuFormatSize.DropDownItems.AddRange(
				new ToolStripItem[] {menuFormatSizeW, menuFormatSizeH, menuFormatSizeB });

			menuFormatHorz = new ToolStripMenuItem("&Horizontal Spacing");
			menuFormatHorzE = new ToolStripMenuItem("&Make Equal", null, new EventHandler(this.menuFormatHorzE_Click));
			menuFormatHorzI = new ToolStripMenuItem("&Increase", null, new EventHandler(this.menuFormatHorzI_Click));
			menuFormatHorzD = new ToolStripMenuItem("&Decrease", null, new EventHandler(this.menuFormatHorzD_Click));
			menuFormatHorzZ = new ToolStripMenuItem("&Zero", null, new EventHandler(this.menuFormatHorzZ_Click));
			menuFormatHorz.DropDownItems.AddRange(
				new ToolStripItem[] {menuFormatHorzE,menuFormatHorzI,menuFormatHorzD,menuFormatHorzZ });

			menuFormatVert = new ToolStripMenuItem("&Vertical Spacing");
			menuFormatVertE = new ToolStripMenuItem("&Make Equal", null, new EventHandler(this.menuFormatVertE_Click));
			menuFormatVertI = new ToolStripMenuItem("&Increase", null, new EventHandler(this.menuFormatVertI_Click));
			menuFormatVertD = new ToolStripMenuItem("&Decrease", null, new EventHandler(this.menuFormatVertD_Click));
			menuFormatVertZ = new ToolStripMenuItem("&Zero", null, new EventHandler(this.menuFormatVertZ_Click));
			menuFormatVert.DropDownItems.AddRange(
				new ToolStripItem[] {menuFormatVertE,menuFormatVertI,menuFormatVertD,menuFormatVertZ });

			menuFormatPaddingLeft = new ToolStripMenuItem("Padding Left");
			menuFormatPaddingLeftI = new ToolStripMenuItem("Increase", null, new EventHandler(this.menuFormatPadding_Click));
			menuFormatPaddingLeftD = new ToolStripMenuItem("Decrease", null, new EventHandler(this.menuFormatPadding_Click));
			menuFormatPaddingLeftZ = new ToolStripMenuItem("Zero", null, new EventHandler(this.menuFormatPadding_Click));
			menuFormatPaddingLeft.DropDownItems.AddRange(
				new ToolStripItem[] {menuFormatPaddingLeftI,menuFormatPaddingLeftD,menuFormatPaddingLeftZ  });

			menuFormatPaddingRight = new ToolStripMenuItem("Padding Right");
			menuFormatPaddingRightI = new ToolStripMenuItem("Increase", null, new EventHandler(this.menuFormatPadding_Click));
			menuFormatPaddingRightD = new ToolStripMenuItem("Decrease", null, new EventHandler(this.menuFormatPadding_Click));
			menuFormatPaddingRightZ = new ToolStripMenuItem("Zero", null, new EventHandler(this.menuFormatPadding_Click));
			menuFormatPaddingRight.DropDownItems.AddRange(
				new ToolStripItem[] {menuFormatPaddingRightI,menuFormatPaddingRightD,menuFormatPaddingRightZ  });

			menuFormatPaddingTop = new ToolStripMenuItem("Padding Top");
			menuFormatPaddingTopI = new ToolStripMenuItem("Increase", null, new EventHandler(this.menuFormatPadding_Click));
			menuFormatPaddingTopD = new ToolStripMenuItem("Decrease", null, new EventHandler(this.menuFormatPadding_Click));
			menuFormatPaddingTopZ = new ToolStripMenuItem("Zero", null, new EventHandler(this.menuFormatPadding_Click));
			menuFormatPaddingTop.DropDownItems.AddRange(
				new ToolStripItem[] {menuFormatPaddingTopI,menuFormatPaddingTopD,menuFormatPaddingTopZ  });

			menuFormatPaddingBottom = new ToolStripMenuItem("Padding Bottom");
			menuFormatPaddingBottomI = new ToolStripMenuItem("Increase", null, new EventHandler(this.menuFormatPadding_Click));
			menuFormatPaddingBottomD = new ToolStripMenuItem("Decrease", null, new EventHandler(this.menuFormatPadding_Click));
			menuFormatPaddingBottomZ = new ToolStripMenuItem("Zero", null, new EventHandler(this.menuFormatPadding_Click));
			menuFormatPaddingBottom.DropDownItems.AddRange(
				new ToolStripItem[] {menuFormatPaddingBottomI,menuFormatPaddingBottomD,menuFormatPaddingBottomZ  });

			menuFormat = new ToolStripMenuItem("F&ormat");
			menuFormat.DropDownOpening +=new EventHandler(menuFormat_Popup);
			menuFormat.DropDownItems.AddRange(
				new ToolStripItem[] { menuFormatAlign, menuFormatSize, menuFormatHorz, menuFormatVert, 
								   new ToolStripSeparator(), 
								   menuFormatPaddingLeft,menuFormatPaddingRight,
								   menuFormatPaddingTop,menuFormatPaddingBottom} );

			// Tools menu
			this.menuToolsProcess = new ToolStripMenuItem("Start Desktop Server", null, new EventHandler(this.menuToolsProcess_Click));
			this.menuToolsValidateSchema = new ToolStripMenuItem("Validate RDL", null, new EventHandler(this.menuToolsValidateSchema_Click));
			this.menuToolsOptions = new ToolStripMenuItem("Options...", null, new EventHandler(this.menuToolsOptions_Click));
			menuTools = new ToolStripMenuItem("&Tools");
			menuTools.DropDownItems.AddRange(
				new ToolStripItem[] { menuToolsValidateSchema, new ToolStripSeparator(), menuToolsProcess, 
								   new ToolStripSeparator(), menuToolsOptions} );
			menuTools.DropDownOpening +=new EventHandler(this.menuTools_Popup);

			// WINDOW MENU
			menuCascade = new ToolStripMenuItem("&Cascade", null, new EventHandler(this.menuWndCascade_Click), (Keys.Control|Keys.Shift|Keys.J));

			menuTileH  = new ToolStripMenuItem("&Horizontally", null, new EventHandler(this.menuWndTileH_Click), (Keys.Control|Keys.Shift|Keys.K));
			menuTileV  = new ToolStripMenuItem("&Vertically", null, new EventHandler(this.menuWndTileV_Click), (Keys.Control|Keys.Shift|Keys.L));
			menuTile = new ToolStripMenuItem("&Tile");
			menuTile.DropDownItems.AddRange(new ToolStripItem[] { menuTileH, menuTileV });

			menuCloseAll = new ToolStripMenuItem("Close &All", null, new EventHandler(this.menuWndCloseAll_Click), (Keys.Control|Keys.Shift|Keys.W));

			// Add the Window menu
			ToolStripMenuItem menuWindow = new ToolStripMenuItem("&Window");
			menuWindow.DropDownOpening +=new EventHandler(this.menuWnd_Popup);
			menuWindow.DropDownItems.AddRange(new ToolStripItem[] { menuCascade, menuTile, menuCloseAll });

			// HELP MENU
			ToolStripMenuItem menuAbout = new ToolStripMenuItem("&About...", null, new EventHandler(this.menuHelpAbout_Click));
			ToolStripMenuItem menuHelpItem = new ToolStripMenuItem("&Help", null, new EventHandler(this.menuHelpHelp_Click));
			ToolStripMenuItem menuSupport = new ToolStripMenuItem("&Support", null, new EventHandler(this.menuHelpSupport_Click));
			ToolStripMenuItem menuHelp = new ToolStripMenuItem("&Help");
			menuHelp.DropDownItems.AddRange(new ToolStripItem[] {menuHelpItem, new ToolStripSeparator(), menuSupport, new ToolStripSeparator(), menuAbout } );

			// S3a-M3: a MenuStrip in place of the WinForms MainMenu. It is added to the form after the toolbar and the status bar (the constructor),
			// so that it docks above the toolbar; the open MDI windows are listed at the end of the Window menu, as MdiList did.
			menuMain = new MenuStrip();
			menuMain.Name = "menuMain";
			menuMain.Dock = DockStyle.Top;
			menuMain.Items.AddRange(new ToolStripItem[]{menuFile, menuEdit, menuView, menuData,
															menuFormat, menuTools, menuWindow, menuHelp});
			menuMain.MdiWindowListItem = menuWindow;
			_dictMenuPopups = new Dictionary<ToolStripMenuItem, EventHandler>();
			_dictMenuPopups.Add(menuFile, this.menuFile_Popup);
			_dictMenuPopups.Add(menuEdit, this.menuEdit_Popup);
			_dictMenuPopups.Add(menuView, this.menuView_Popup);
			_dictMenuPopups.Add(menuData, this.menuData_Popup);
			_dictMenuPopups.Add(menuFormat, this.menuFormat_Popup);
			_dictMenuPopups.Add(menuTools, this.menuTools_Popup);
			_dictMenuPopups.Add(menuWindow, this.menuWnd_Popup);
			IsMdiContainer = true;
		}

		/// <summary>The main menu (a WinForms MainMenu before S3a-M3).</summary>
		MenuStrip menuMain;
		/// <summary>The menus with a Popup (now DropDownOpening) handler, and the handler.</summary>
		Dictionary<ToolStripMenuItem, EventHandler> _dictMenuPopups;

		/// <summary>A WinForms MainMenu raised the Popup of the menus that hold an item before it ran the item's shortcut, so the item's Enabled
		/// state (set in those handlers) was current; a MenuStrip runs the shortcut with the state of the last time the menu was opened. This raises
		/// the same handlers first, from the top menu down, for the item that has the key as its shortcut.</summary>
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (menuMain != null && _dictMenuPopups != null)
			{
				List<ToolStripMenuItem> listPath = FindShortcutPath(menuMain.Items, keyData);
				if (listPath != null)
				{
					foreach (ToolStripMenuItem menuParent in listPath)
					{
						EventHandler eventHandler;
						if (_dictMenuPopups.TryGetValue(menuParent, out eventHandler))
							eventHandler(menuParent, EventArgs.Empty);
					}
				}
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		/// <summary>The menus from the top down to the item whose shortcut is the key (the item not included), or null if no item has it.</summary>
		internal static List<ToolStripMenuItem> FindShortcutPath(ToolStripItemCollection items, Keys keyData)
		{
			foreach (ToolStripItem toolStripItem in items)
			{
				ToolStripMenuItem toolStripMenuItem = toolStripItem as ToolStripMenuItem;
				if (toolStripMenuItem == null)
					continue;
				if (toolStripMenuItem.ShortcutKeys == keyData && keyData != Keys.None)
					return new List<ToolStripMenuItem>();
				List<ToolStripMenuItem> listPath = FindShortcutPath(toolStripMenuItem.DropDownItems, keyData);
				if (listPath != null)
				{
					listPath.Insert(0, toolStripMenuItem);
					return listPath;
				}
			}
			return null;
		}
  
		private bool OkToSave()
		{
			foreach (MDIChild mc in this.MdiChildren)
			{
				if (!mc.OkToClose())
					return false;
			}
			return true;
		}

		private void menuFile_Popup(object sender, EventArgs e)
		{
			// These menus require an MDIChild in order to work
			bool bEnable = this.MdiChildren.Length > 0? true: false;
			menuClose.Enabled = bEnable;
			menuSave.Enabled = bEnable;
			menuSaveAs.Enabled = bEnable;

			MDIChild mc = this.ActiveMdiChild as MDIChild;
			menuPrint.Enabled = menuExport.Enabled = (mc != null && mc.DesignTab == "preview");

			// Recent File is enabled when there exists some files 
			menuRecentFile.Enabled = this._RecentFiles.Count <= 0? false: true;
		}

		private void menuFileClose_Click(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc != null)
				mc.Close();
		}

		private void menuFileExit_Click(object sender, EventArgs e)
		{
			if (!OkToSave())
				return;
			SaveStartupState();
			menuToolsCloseProcess(false);
			CleanupTempFiles();
			Environment.Exit(0);
		}

		private void menuFileOpen_Click(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			OpenFileDialog ofd = new OpenFileDialog();
			if (mc != null)
			{
				try 
				{
					ofd.InitialDirectory = Path.GetDirectoryName(mc.SourceFile);
				}
				catch
				{
				}
			}
			ofd.DefaultExt = "rdl";
			ofd.Filter = "Report files (*.rdl)|*.rdl|" +
				"All files (*.*)|*.*";
			ofd.FilterIndex = 1;
			ofd.CheckFileExists = true;
			ofd.Multiselect = true;
			if (ofd.ShowDialog(this) == DialogResult.OK)
			{
				foreach (string file in ofd.FileNames)
				{
					CreateMDIChild(file, null, false);
				}
				RecentFilesMenu();		// update the menu for recent files
			}
		}

		// Create an MDI child.   Only creates it if not already open.
		private MDIChild CreateMDIChild(string file, string rdl, bool bMenuUpdate)
		{
			MDIChild mcOpen=null;
			if (file != null)
			{
				file = file.Trim();

				foreach (MDIChild mc in this.MdiChildren)
				{
					if (mc.SourceFile != null && file == mc.SourceFile.Trim())	
					{							// we found it
						mcOpen = mc;
						break;
					}
				}
			}
            if (mcOpen == null)
            {
                MDIChild mc = null;
                try
                {
                    mc = new MDIChild(this.ClientRectangle.Width * 3 / 5, this.ClientRectangle.Height * 3 / 5);
                    mc.OnSelectionChanged += new MDIChild.RdlChangeHandler(SelectionChanged);
                    mc.OnSelectionMoved += new MDIChild.RdlChangeHandler(SelectionMoved);
                    mc.OnReportItemInserted += new MDIChild.RdlChangeHandler(ReportItemInserted);
                    mc.OnDesignTabChanged += new MDIChild.RdlChangeHandler(DesignTabChanged);
                    mc.OnOpenSubreport += new DesignCtl.OpenSubreportEventHandler(OpenSubReportEvent);
                    mc.OnHeightChanged += new DesignCtl.HeightEventHandler(HeightChanged);
                    
                    mc.MdiParent = this;
                    if (this._ShowTabbedInterface)
                        mc.WindowState = FormWindowState.Maximized;
                    mc.Viewer.GetDataSourceReferencePassword = _GetPassword;
                    if (file != null)
                    {
                        mc.Viewer.Folder = Path.GetDirectoryName(file);
                        mc.SourceFile = file;
                        mc.Text = Path.GetFileName(file);
                        mc.Viewer.Folder = Path.GetDirectoryName(file);
                        mc.Viewer.ReportName = Path.GetFileNameWithoutExtension(file);
                        NoteRecentFiles(file, bMenuUpdate);
                    }
                    else
                    {
                        mc.SourceRdl = rdl;
                        mc.Viewer.ReportName = mc.Text = "Untitled";
                    }
                    mc.ShowEditLines(this._ShowEditLines);
                    mc.ShowReportItemOutline = this.ShowReportItemOutline;
                    // add to toolbar tab
                    TabPage tp = new TabPage();
                    tp.Location = new System.Drawing.Point(0, 0);
                    tp.Name = mc.Text;
                    tp.TabIndex = 1;
                    tp.Text = mc.Text;
                    tp.Tag = mc;                // tie the mdichild to the tabpage
                    tp.ToolTipText = file;
                    mainTC.Controls.Add(tp);
                    mc.Tab = tp;

                    mc.Show();
                    mcOpen = mc;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    if (mc != null)
                        mc.Close();
                    return null;
                }
            }
            else
            {
                mcOpen.Activate();
            }
			return mcOpen;
		}

		private void DesignTabChanged(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			string tab="";
			if (mc != null)
				tab = mc.DesignTab;
			bool bEnableEdit=false;
			bool bEnableDesign=false;
			bool bEnablePreview=false;
			switch (tab)
			{
				case "edit":
					bEnableEdit=true;
					break;
				case "design":
					bEnableDesign=true;
					break;
				case "preview":
					bEnablePreview=true;
					break;
			}
			if (!bEnableEdit && this._ValidateRdl != null)
			{
				this._ValidateRdl.Close();
			}
			if (ctlBold != null)
				ctlBold.Enabled = bEnableDesign;
			if (ctlItalic != null)
				ctlItalic.Enabled = bEnableDesign;
			if (ctlUnderline != null)
				ctlUnderline.Enabled = bEnableDesign;
			if (ctlFont != null)
				ctlFont.Enabled = bEnableDesign;
			if (ctlFontSize != null)
				ctlFontSize.Enabled = bEnableDesign;
			if (ctlForeColor != null)
				ctlForeColor.Enabled = bEnableDesign;
			if (ctlBackColor != null)
				ctlBackColor.Enabled = bEnableDesign;
			if (ctlCut != null)
				ctlCut.Enabled = bEnableDesign | bEnableEdit;
			if (ctlCopy != null)
				ctlCopy.Enabled = bEnableDesign | bEnableEdit;
			if (ctlUndo != null)
				ctlUndo.Enabled = bEnableDesign | bEnableEdit; 
			if (ctlPaste != null)
				ctlPaste.Enabled = bEnableDesign | bEnableEdit;
			if (ctlPrint != null)
				ctlPrint.Enabled = bEnablePreview;

			if (ctlInsertTextbox != null)
				ctlInsertTextbox.Enabled = bEnableDesign;
			if (ctlInsertChart != null)
				ctlInsertChart.Enabled = bEnableDesign;
			if (ctlInsertRectangle != null)
				ctlInsertRectangle.Enabled = bEnableDesign;
			if (ctlInsertTable != null)
				ctlInsertTable.Enabled = bEnableDesign;
			if (ctlInsertMatrix != null)
				ctlInsertMatrix.Enabled = bEnableDesign;
			if (ctlInsertList != null)
				ctlInsertList.Enabled = bEnableDesign;
			if (ctlInsertLine != null)
				ctlInsertLine.Enabled = bEnableDesign;
			if (ctlInsertImage != null)
				ctlInsertImage.Enabled = bEnableDesign;
			if (ctlInsertSubreport != null)
				ctlInsertSubreport.Enabled = bEnableDesign;
			if (ctlPdf != null)
				ctlPdf.Enabled = bEnablePreview;
			if (ctlXml != null)
				ctlXml.Enabled = bEnablePreview;
			if (ctlHtml != null)
				ctlHtml.Enabled = bEnablePreview;
			if (ctlMht != null)
				ctlMht.Enabled = bEnablePreview;

			this.EnableEditTextBox();

			if (ctlZoom != null)
			{
				ctlZoom.Enabled = bEnablePreview;
				string zText="Actual Size";
				if (mc != null)
				{
					switch(mc.ZoomMode)
					{
						case ZoomEnum.FitWidth:
							zText = "Fit Width";
							break;
						case ZoomEnum.FitPage:
							zText = "Fit Page";
							break;
						case ZoomEnum.UseZoom:
							if (mc.Zoom == 1)
								zText = "Actual Size";
							else
								zText = string.Format("{0:0}", (mc.Zoom*100f).AsNetFx());
							break;
					}
					ctlZoom.Text = zText;
				}
			}
			// when no active sheet
			if (this.ctlSave != null)
				this.ctlSave.Enabled = mc != null;
			
			// Update the status and position information
			SetStatusNameAndPosition();
		}

		private void EnableEditTextBox()
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			bool bEnable = false;

			if (this.ctlEditTextbox == null || mc == null || 
				mc.DesignTab != "design" || mc.DrawCtl.SelectedCount != 1)
			{}
			else
			{
				XmlNode tn = mc.DrawCtl.SelectedList[0] as XmlNode;
				if (tn != null && tn.Name == "Textbox")
				{
					ctlEditTextbox.Text = mc.DrawCtl.GetElementValue(tn, "Value", "");
					bEnable = true;
				}
			}
			if (ctlEditTextbox != null)
			{
				if (!bEnable)
					ctlEditTextbox.Text = "";
				ctlEditTextbox.Enabled = bEnable;
				ctlEditLabel.Enabled = bEnable;
			}
		}

		private void ReportItemInserted(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			// turn off the current selection after an item is inserted
			if (ctlInsertCurrent != null)
			{
				ctlInsertCurrent.Checked = false;
				mc.CurrentInsert = null;
				ctlInsertCurrent = null;
			}
		}
	
		private void OpenSubReportEvent(object sender, SubReportEventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			string file = mc.Viewer.Folder;
			if (e.SubReportName[0] == Path.DirectorySeparatorChar)
				file = file + e.SubReportName;
			else
				file = file + Path.DirectorySeparatorChar + e.SubReportName + ".rdl";

			CreateMDIChild(file, null, true);
		}

        private void HeightChanged(object sender, HeightEventArgs e)
        {
            if (e.Height == null)
            {
                statusPosition.Text = "";
                return;
            }

            RegionInfo rinfo = new RegionInfo(CultureInfo.CurrentCulture.LCID);
            float h = DesignXmlDraw.GetSize(e.Height);
            string sh;
            if (rinfo.IsMetric)
            {
                sh = string.Format("   height={0:0.00}cm        ",
                        (h / (DesignXmlDraw.POINTSIZED / 2.54d)).AsNetFx());
            }
            else
            {
                sh = string.Format("   height={0:0.00}\"        ",
                        (h / DesignXmlDraw.POINTSIZED).AsNetFx());
            }
            statusPosition.Text = sh;
        }

		private void SelectionMoved(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			SetStatusNameAndPosition();
		}
  
		private void SelectionChanged(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;
			// handle edit tab first
			if (mc.RdlEditor.DesignTab == "edit")
			{
				SetStatusNameAndPosition();
				return;
			}

			bSuppressChange = true;			// don't process changes in status bar

			SetStatusNameAndPosition();
			this.EnableEditTextBox();	// handling enabling/disabling of textbox

			StyleInfo si = mc.SelectedStyle;
			if (si == null)
				return;

			if (ctlBold != null)
				ctlBold.Checked = si.IsFontBold()? true: false;
			if (ctlItalic != null)
				ctlItalic.Checked = si.FontStyle == FontStyleEnum.Italic? true: false;
			if (ctlUnderline != null)
				ctlUnderline.Checked = si.TextDecoration == TextDecorationEnum.Underline? true: false;
			if (ctlFont != null)
				ctlFont.Text = si.FontFamily;
			if (ctlFontSize != null)
			{
				string rs = string.Format(NumberFormatInfo.InvariantInfo, "{0:0.#}", si.FontSize.AsNetFx());
				ctlFontSize.Text = rs;
			}
			if (ctlForeColor != null)
				ctlForeColor.Text = ColorTranslator.ToHtml(si.Color);
			if (ctlBackColor != null)
				ctlBackColor.Text = ColorTranslator.ToHtml(si.BackgroundColor);

			bSuppressChange = false;
		}

		private void menuData_Popup(object sender, EventArgs ea)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			bool bEnable = false;
			if (mc != null && mc.DesignTab == "design")
				bEnable = true;

			this.menuDataSources.Enabled = this.menuDataSets.Enabled = this.menuEmbeddedImages.Enabled = bEnable;
			if (!bEnable)
				return;

			// Run thru all the existing DataSets
			menuDataSets.DropDownItems.Clear();
			menuDataSets.DropDownItems.Add(new ToolStripMenuItem("New...", null, new EventHandler(this.menuDataSets_Click)));

			DesignXmlDraw draw = mc.DrawCtl;
			XmlNode rNode = draw.GetReportNode();
			XmlNode dsNode = draw.GetNamedChildNode(rNode, "DataSets");
			if (dsNode != null)
			{
				foreach (XmlNode dNode in dsNode)
				{	
					if (dNode.Name != "DataSet")
						continue;
					XmlAttribute nAttr = dNode.Attributes["Name"];
					if (nAttr == null)	// shouldn't really happen
						continue;
					menuDataSets.DropDownItems.Add(new ToolStripMenuItem(nAttr.Value, null, new EventHandler(this.menuDataSets_Click)));
				}
			}
		}
 
		private void menuDataSources_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.StartUndoGroup("DataSources Dialog");
			DialogDataSources dlgDS = new DialogDataSources(mc.DrawCtl);
			dlgDS.StartPosition = FormStartPosition.CenterParent;
			DialogResult dr = dlgDS.ShowDialog();
			mc.Editor.EndUndoGroup(dr == DialogResult.OK);
			if (dr == DialogResult.OK)
				mc.Modified = true;
		}

		private void menuDataSets_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null || mc.DrawCtl == null || mc.ReportDocument == null)
				return;

			ToolStripMenuItem menu = sender as ToolStripMenuItem;
			if (menu == null)
				return;
			mc.Editor.StartUndoGroup("DataSet Dialog");

			string dsname = menu.Text;

			// Find the dataset we need
            List<XmlNode> ds = new List<XmlNode>();
			DesignXmlDraw draw = mc.DrawCtl;
			XmlNode rNode = draw.GetReportNode();
			XmlNode dsNode = draw.GetCreateNamedChildNode(rNode, "DataSets");
			XmlNode dataset=null;
			
			// find the requested dataset: the menu text equals the name of the dataset
			int dsCount=0;		// count of the datasets
			string onlyOneDsname=null;
			foreach (XmlNode dNode in dsNode)
			{	
				if (dNode.Name != "DataSet")
					continue;
				XmlAttribute nAttr = dNode.Attributes["Name"];
				if (nAttr == null)	// shouldn't really happen
					continue;
				if (dsCount == 0)
					onlyOneDsname = nAttr.Value;		// we keep track of 1st name; 

				dsCount++;
				if (nAttr.Value == dsname)
					dataset = dNode;
			}

			bool bNew = false;
			if (dataset == null)	// This must be the new menu item
			{
				dataset = draw.CreateElement(dsNode, "DataSet", null);	// create empty node
				bNew = true;
			}
			ds.Add(dataset);

			PropertyDialog pd = new PropertyDialog(mc.DrawCtl, ds, PropertyTypeEnum.DataSets);
			DialogResult dr = pd.ShowDialog();
			if (pd.Changed || dr == DialogResult.OK)
			{
				if (dsCount == 1)	
				// if we used to just have one DataSet we may need to fix up DataRegions 
				//	that were defaulting to that name
				{
					dsCount=0;
					bool bUseName = false;
					foreach (XmlNode dNode in dsNode)
					{	
						if (dNode.Name != "DataSet")
							continue;
						XmlAttribute nAttr = dNode.Attributes["Name"];
						if (nAttr == null)	// shouldn't really happen
							continue;

						dsCount++;
						if (onlyOneDsname == nAttr.Value)
							bUseName = true;
					}
					if (bUseName && dsCount > 1)
					{
						foreach (XmlNode drNode in draw.ReportNames.ReportItems)
						{
							switch (drNode.Name)
							{
								// If a DataRegion doesn't have a dataset name specified use previous one
								case "Table": case "List": case "Matrix": case "Chart":
									XmlNode aNode = draw.GetNamedChildNode(drNode, "DataSetName");
									if (aNode == null)
										draw.CreateElement(drNode, "DataSetName", onlyOneDsname);
									break;
								default:
									break;
							}
						}
					}
				}
				mc.Modified = true;
			}
			else if (bNew)	// if canceled and new DataSet get rid of temp node
			{
				dsNode.RemoveChild(dataset);
			}
			if (pd.Delete)	// user must have hit a delete button for this to get set
				dsNode.RemoveChild(dataset);

			if (!dsNode.HasChildNodes)		// If no dataset exists we remove DataSets
				draw.RemoveElement(rNode, "DataSets");

			mc.Editor.EndUndoGroup(pd.Changed || dr == DialogResult.OK);
		}
 
		private void menuEmbeddedImages_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.StartUndoGroup("Embedded Images Dialog");
			DialogEmbeddedImages dlgEI = new DialogEmbeddedImages(mc.DrawCtl);
			dlgEI.StartPosition = FormStartPosition.CenterParent;
			DialogResult dr = dlgEI.ShowDialog();
			mc.Editor.EndUndoGroup(dr == DialogResult.OK);
			if (dr == DialogResult.OK)
				mc.Modified = true;
		}

		private void menuFileNewDataSourceRef_Click(object sender, System.EventArgs e)
		{
			DialogDataSourceRef dlgDS = new DialogDataSourceRef();
			dlgDS.StartPosition = FormStartPosition.CenterParent;
			dlgDS.ShowDialog();
			if (dlgDS.DialogResult == DialogResult.Cancel)
				return;
		}

		private void menuFileNewReport_Click(object sender, System.EventArgs e)
		{
			DialogDatabase dlgDB = new DialogDatabase(this);
			dlgDB.StartPosition = FormStartPosition.CenterParent;
			dlgDB.FormBorderStyle = FormBorderStyle.SizableToolWindow;

			// show modally
			dlgDB.ShowDialog();
			if (dlgDB.DialogResult == DialogResult.Cancel)
				return;
			string rdl = dlgDB.ResultReport;

			// Create the MDI child using the RDL syntax the wizard generates
			MDIChild mc = CreateMDIChild(null, rdl, false);
			mc.Modified = true;
			// Force building of report names for new reports
			if (mc.DrawCtl.ReportNames == null) {}	
			
		}

		private void menuFilePrint_Click(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;
			if (printChild != null)			// already printing
			{
				MessageBox.Show("Can only print one file at a time.", "RDL Design");
				return;
			}

			printChild = mc;

			PrintDocument pd = new PrintDocument();
			pd.DocumentName = mc.SourceFile;
			pd.PrinterSettings.FromPage = 1;
			pd.PrinterSettings.ToPage = mc.PageCount;
			pd.PrinterSettings.MaximumPage = mc.PageCount;
			pd.PrinterSettings.MinimumPage = 1;
			pd.DefaultPageSettings.Landscape = mc.PageWidth > mc.PageHeight? true: false;

			PrintDialog dlg = new PrintDialog();
			dlg.Document = pd;
			dlg.AllowSelection = true;
			dlg.AllowSomePages = true;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				try
				{
					if (pd.PrinterSettings.PrintRange == PrintRange.Selection)
					{
						pd.PrinterSettings.FromPage = mc.PageCurrent;
					}
					mc.Print(pd);
				}
				catch (Exception ex)
				{
					MessageBox.Show("Print error: " + ex.Message, "RDL Design");
				}
			}
			printChild = null;
		}

		private void menuFileSave_Click(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			if (!mc.FileSave())
				return;

			NoteRecentFiles(mc.SourceFile, true);

			if (mc.Editor != null)
				mc.Editor.ClearUndo();

			return;
		}

		private void menuExportXml_Click(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Export("xml");
			return;
		}

		private void menuExportHtml_Click(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Export("html");
			return;
		}

		private void menuExportMHtml_Click(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Export("mht");
			return;
		}

		private void menuExportPdf_Click(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Export("pdf");
			return;
		}

		private void menuFileSaveAs_Click(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			if (!mc.FileSaveAs())
				return;

			mc.Viewer.Folder = Path.GetDirectoryName(mc.SourceFile);
			mc.Viewer.ReportName = Path.GetFileNameWithoutExtension(mc.SourceFile);
			mc.Text = Path.GetFileName(mc.SourceFile);
			
			NoteRecentFiles(mc.SourceFile, true);

			if (mc.Editor != null)
				mc.Editor.ClearUndo();

			return;
		}

		private void menuEdit_Popup(object sender, EventArgs ea)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			// These menus require an MDIChild in order to work
			RdlEditPreview e = GetEditor();			
			menuEdit.DropDownItems.Clear();
			if (e == null || e.DesignTab != "edit")
			{
				menuEditUndo.Text = e == null? "Undo": "Undo " + e.UndoDescription;
				menuEdit.DropDownItems.AddRange(
					new ToolStripItem[] {  menuEditUndo, /* menuEditRedo,*/ menuFSep1,  menuEditCut, menuEditCopy,
									   menuEditPaste, menuEditDelete, menuFSep2, menuEditSelectAll});

				if (mc == null || e == null)
				{
					menuEditUndo.Enabled = menuEditRedo.Enabled = menuEditCut.Enabled = menuEditCopy.Enabled = 
						menuEditPaste.Enabled = menuEditDelete.Enabled = menuEditSelectAll.Enabled =
						 false;
					return;
				}
			}
			else
			{
				menuEditUndo.Text = "Undo";
				menuEdit.DropDownItems.AddRange(
					new ToolStripItem[] { menuEditUndo, menuEditRedo, menuFSep1, menuEditCut, menuEditCopy,
									   menuEditPaste, menuEditDelete, menuFSep2, menuEditSelectAll, menuFSep3,
									   menuEditFind, menuEditFindNext, menuEditReplace, menuEditGoto,
									   menuFSep4, menuEditFormatXml});
			}
			menuEditUndo.Enabled = e.CanUndo;
			menuEditRedo.Enabled = e.CanRedo;
			bool bSelection = e.SelectionLength > 0;	// any text selected?
			menuEditCut.Enabled = bSelection;
			menuEditCopy.Enabled = bSelection;
			menuEditPaste.Enabled = Clipboard.GetDataObject().GetDataPresent(DataFormats.Text);
			menuEditDelete.Enabled = bSelection;
			menuEditSelectAll.Enabled = true;

			bool bAnyText = e.Text.Length > 0;			// any text to search at all?
			menuEditFind.Enabled = menuEditFindNext.Enabled = 
				menuEditReplace.Enabled = menuEditGoto.Enabled = bAnyText;
		}

		private void menuEditUndo_Click(object sender, System.EventArgs ea)
		{
			if (this.ctlEditTextbox != null && ctlEditTextbox.Focused)
			{
				ctlEditTextbox.Undo();
				return;	
			}

			RdlEditPreview e = GetEditor();
			if (e == null)
				return;

			if (e.CanUndo == true)
			{
				e.Undo();

				MDIChild mc = this.ActiveMdiChild as MDIChild;
				if (mc != null && mc.DesignTab == "design")
				{
					e.DesignCtl.SetScrollControls();
				}
				this.SelectionChanged(this, new EventArgs());
			}
		}

		private void menuEditRedo_Click(object sender, System.EventArgs ea)
		{
			RdlEditPreview e = GetEditor();
			if (e == null)
				return;

			if (e.CanRedo == true)
			{
				e.Redo();
			}
		}

		private void menuEditCut_Click(object sender, System.EventArgs ea)
		{
			if (this.ctlEditTextbox != null && ctlEditTextbox.Focused)
			{
				ctlEditTextbox.Cut();
				return;	
			}

			RdlEditPreview e = GetEditor();
			if (e == null)
				return;

			if (e.SelectionLength > 0)
				e.Cut();
		}

		private void menuEditCopy_Click(object sender, System.EventArgs ea)
		{
			if (this.ctlEditTextbox != null && ctlEditTextbox.Focused)
			{
				ctlEditTextbox.Copy();
				return;	
			}
			RdlEditPreview e = GetEditor();
			if (e == null)
				return;

			if (e.SelectionLength > 0)
				e.Copy();
		}

		private void menuEditPaste_Click(object sender, System.EventArgs ea)
		{
			if (this.ctlEditTextbox != null && ctlEditTextbox.Focused)
			{
				ctlEditTextbox.Paste();
				return;	
			}

			RdlEditPreview e = GetEditor();
			if (e == null)
				return;

			if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text) == true ||
				Clipboard.GetDataObject().GetDataPresent(DataFormats.Bitmap) == true)
				e.Paste();
		}

		private void menuEditDelete_Click(object sender, System.EventArgs ea)
		{
			RdlEditPreview e = GetEditor();
			if (e == null)
				return;

			if (e.SelectionLength > 0)
				e.SelectedText="";
		}

		private void menuEditProperties_Click(object sender, System.EventArgs ea)
		{
			RdlEditPreview e = GetEditor();
			if (e == null)
				return;

			e.DesignCtl.menuProperties_Click();
		}

		private void menuEditSelectAll_Click(object sender, System.EventArgs ea)
		{
			if (this.ctlEditTextbox != null && ctlEditTextbox.Focused)
			{
				ctlEditTextbox.SelectAll();
				return;	
			}
			RdlEditPreview e = GetEditor();
			if (e == null)
				return;

			e.SelectAll();
		}

		private void menuEditFind_Click(object sender, System.EventArgs ea)
		{
			RdlEditPreview e = GetEditor();
			if (e == null)
				return;

			FindTab tab = new FindTab(e);
			tab.Show();
		}

		private void menuEditFindNext_Click(object sender, System.EventArgs ea)
		{
			RdlEditPreview e = GetEditor();
			if (e == null)
				return;

			FindTab tab = new FindTab(e);
			tab.Show();		
		} 

		private void menuEdit_FormatXml(object sender, System.EventArgs ea)
		{
			RdlEditPreview e = GetEditor();
			if (e == null)
				return;

			if (e.Text.Length > 0)
			{
				try
				{
					e.Text = DesignerUtility.FormatXml(e.Text);
					e.Modified = true;
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Format XML");
				}
			}
		} 

		private void menuEditReplace_Click(object sender, System.EventArgs ea)
		{
			RdlEditPreview e = GetEditor();
			if (e == null)
				return;
			FindTab tab = new FindTab(e);
			tab.tcFRG.SelectedTab = tab.tabReplace;
			tab.Show();		
		}

		private void menuEditGoto_Click(object sender, System.EventArgs ea)
		{
			RdlEditPreview e = GetEditor();
			if (e == null)
				return;

			FindTab tab = new FindTab(e);
			tab.tcFRG.SelectedTab = tab.tabGoTo;
			tab.Show();
		}

		private void menuHelpAbout_Click(object sender, System.EventArgs ea)
		{
			DialogAbout dlg = new DialogAbout();
			dlg.ShowDialog();
		}

		private void menuHelpHelp_Click(object sender, System.EventArgs ea)
		{
			try
			{
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(HelpUrl) { UseShellExecute = true });//The shell opens the link (the .NET default is false).
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + "\n\n" + "Resetting Help URL to default.", "Help URL Invalid");
				_HelpUrl = DefaultHelpUrl;
			}
		}

		private void menuHelpSupport_Click(object sender, System.EventArgs ea)
		{
			try
			{
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(SupportUrl) { UseShellExecute = true });//The shell opens the link (the .NET default is false).
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + "\n\n" + "Resetting Support URL to default.", "Support URL Invalid");
				_SupportUrl = DefaultSupportUrl;
			}
		}

		internal RdlEditPreview GetEditor()
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return null;
			return mc.Editor;
		}

		private void menuTools_Popup(object sender, EventArgs e)
		{
			// If the server process isn't running then we'll start it up
			if (_ServerProcess != null && _ServerProcess.HasExited)
				_ServerProcess = null;
			menuToolsProcess.Text = this._ServerProcess == null? "Start Desktop": "Stop Desktop";

			MDIChild mc = this.ActiveMdiChild as MDIChild;
			this.menuToolsValidateSchema.Enabled = (mc != null && mc.DesignTab == "edit");
			
		}

		private void menuToolsProcess_Click(object sender, EventArgs e)
		{
			if (_ServerProcess == null)
				menuToolsStartProcess(true);
			else
				menuToolsCloseProcess(true);
		}

		internal void menuToolsStartProcess(bool bMsg)
		{
			if (_ServerProcess != null && !_ServerProcess.HasExited)
				return;

			string pswd = GetPassword();

			try
			{
				string filename = string.Format("{0}{1}",
					AppDomain.CurrentDomain.BaseDirectory, "RdlDesktop.exe");

				ProcessStartInfo psi = new ProcessStartInfo(filename);
				if (pswd != null)
					psi.Arguments = "/p" + pswd;
				psi.RedirectStandardError = psi.RedirectStandardInput = psi.RedirectStandardOutput = true;
				psi.UseShellExecute = false;
				//psi.WindowStyle = ProcessWindowStyle.Hidden;
				psi.CreateNoWindow = true;
				_ServerProcess = Process.Start(psi);
			}
			catch (Exception ex)
			{
				if (bMsg)
					MessageBox.Show(ex.Message, "Unable to start Desktop");
			}

			return;
		}

		internal void menuToolsCloseProcess(bool bMsg)
		{
			if (_ServerProcess == null)
				return;
			if (!_ServerProcess.HasExited)
			{
				try
				{
					_ServerProcess.StandardInput.WriteLine("x");	// x stops the server
				}
				catch  (Exception ex)
				{
					if (bMsg)
						MessageBox.Show(ex.Message, "Error stopping process");
				}
			}
			_ServerProcess = null;
		}

		private void menuToolsOptions_Click(object sender, EventArgs e)
		{
			DialogToolOptions dlg = new DialogToolOptions(this);
			DialogResult rc = dlg.ShowDialog();
		}
		
		private void menuToolsValidateSchema_Click(object sender, EventArgs e)
		{
			if (_ValidateRdl == null)
			{
				_ValidateRdl = new DialogValidateRdl(this);
				_ValidateRdl.Show();
			}
			else
				_ValidateRdl.BringToFront();
			return;
		}

		internal void ValidateSchemaClosing()
		{
			this._ValidateRdl = null;
		}

		private void menuWnd_Popup(object sender, EventArgs e)
		{
			// These menus require an MDIChild in order to work
			bool bEnable = this.MdiChildren.Length > 0? true: false;

			menuCascade.Enabled = bEnable;
			menuTile.Enabled = bEnable;
			menuCloseAll.Enabled = bEnable;
		}

		private void menuWndCascade_Click(object sender, EventArgs e)
		{
			this.LayoutMdi(MdiLayout.Cascade);
		}

		private void menuWndCloseAll_Click(object sender, EventArgs e)
		{
			foreach (Form f in this.MdiChildren)
			{
				f.Close();
			}
		}

		private void menuWndTileH_Click(object sender, EventArgs e)
		{
			this.LayoutMdi(MdiLayout.TileHorizontal);
		}

		private void menuWndTileV_Click(object sender, EventArgs e)
		{
			this.LayoutMdi(MdiLayout.TileVertical);
		}

		private void menuRecentItem_Click(object sender, System.EventArgs e)
		{
			ToolStripMenuItem m = (ToolStripMenuItem) sender;
			int si = m.Text.IndexOf(" ");
			string file = m.Text.Substring(si+1);

			CreateMDIChild(file, null, true);
		}

		private void RdlDesigner_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			SaveStartupState();
			menuToolsCloseProcess(false);
			CleanupTempFiles();
		}
 
		private void NoteRecentFiles(string name, bool bResetMenu)
		{
			if (name == null)
				return;

			name = name.Trim();
			if (_RecentFiles.ContainsValue(name))
			{	// need to move it to top of list; so remove old one
				int loc = _RecentFiles.IndexOfValue(name);
				_RecentFiles.RemoveAt(loc);
			}
			if (_RecentFiles.Count >= _RecentFilesMax)
			{
				_RecentFiles.RemoveAt(0);	// remove the first entry
			}
			_RecentFiles.Add(DateTime.Now, name);
			if (bResetMenu)
				RecentFilesMenu();
			return;
		}

		internal void RecentFilesMenu()
		{
			menuRecentFile.DropDownItems.Clear();
			int mi = 1;
			for (int i=_RecentFiles.Count-1; i >= 0; i--)
			{
				string menuText = string.Format("&{0} {1}", mi++, _RecentFiles.Values[i]);
				ToolStripMenuItem m = new ToolStripMenuItem(menuText);
				m.Click += new EventHandler(this.menuRecentItem_Click);
				menuRecentFile.DropDownItems.Add(m);
			}
		}
 
		internal void ResetPassword()
		{
			bGotPassword = false;
			_DataSourceReferencePassword = null;
		}

		internal string GetPassword()
		{
			if (bGotPassword)
				return _DataSourceReferencePassword;

			DataSourcePassword dlg = new DataSourcePassword();
			DialogResult rc = dlg.ShowDialog();
			bGotPassword = true;
			if (rc == DialogResult.OK)
				_DataSourceReferencePassword = dlg.PassPhrase;

			return _DataSourceReferencePassword;
		}

		private void GetStartupState()
		{
			string optFileName = AppDomain.CurrentDomain.BaseDirectory + "designerstate.xml";
			_RecentFiles = new SortedList<DateTime, string>();
			_CurrentFiles = new List<string>();
			_HelpUrl = DefaultHelpUrl;				// set as default
			_SupportUrl = DefaultSupportUrl;
			
			try
			{
				XmlDocument xDoc = new XmlDocument();
				xDoc.PreserveWhitespace = false;
				xDoc.Load(optFileName);
				XmlNode xNode;
				xNode = xDoc.SelectSingleNode("//designerstate");

				string[] args = Environment.GetCommandLineArgs();
				for (int i=1; i < args.Length; i++)
				{
					string larg = args[i].ToLower();
					if (larg == "/m" || larg == "-m")
						continue;

					if (File.Exists(args[i]))			// only add it if it exists
						_CurrentFiles.Add(args[i]);
				}

				// Loop thru all the child nodes
				foreach(XmlNode xNodeLoop in xNode.ChildNodes)
				{
					switch (xNodeLoop.Name)
					{
						case "RecentFiles":
							DateTime now = DateTime.Now;
							now = now.Subtract(new TimeSpan(0,1,0,0,0));	// subtract an hour
							foreach (XmlNode xN in xNodeLoop.ChildNodes)
							{
								string file = xN.InnerText.Trim();
								if (File.Exists(file))			// only add it if it exists
								{
									_RecentFiles.Add(now, file);
									now = now.AddSeconds(1);
								}
							}
							break;
						case "RecentFilesMax":
							try
							{
								this._RecentFilesMax = Convert.ToInt32(xNodeLoop.InnerText);
							}
							catch 
							{
								this._RecentFilesMax = 5;
							}
							break;
						case "CurrentFiles":
							if (_CurrentFiles.Count > 0)	// don't open other current files if opened with argument
								break;
							foreach (XmlNode xN in xNodeLoop.ChildNodes)
							{
								string file = xN.InnerText.Trim();
								if (File.Exists(file))			// only add it if it exists
									_CurrentFiles.Add(file);
							}
							break;
						case "Toolbar":
							_Toolbar = new List<string>();
							foreach (XmlNode xN in xNodeLoop.ChildNodes)
							{
								string item = xN.InnerText.Trim();
								_Toolbar.Add(item);
							}
							break;
						case "Help":
							if (xNodeLoop.InnerText.Length > 0)		//empty means to use the default
								_HelpUrl = xNodeLoop.InnerText;
							break;
						case "Support":
							if (xNodeLoop.InnerText.Length > 0)		//empty means to use the default
								_SupportUrl = xNodeLoop.InnerText;
							break;
                        case "EditLines":
                            _ShowEditLines = (xNodeLoop.InnerText.ToLower() == "true") ;
                            break;
                        case "OutlineReportItems":
                            this.ShowReportItemOutline = (xNodeLoop.InnerText.ToLower() == "true");
                            break;
                        case "ShowTabbedInterface":
                            this._ShowTabbedInterface = (xNodeLoop.InnerText.ToLower() == "true");
                            break;
                        default:
							break;
					}
				}
			}
			catch (Exception ex)
			{		// Didn't sucessfully get the startup state but don't really care
				Console.WriteLine(string.Format("Exception in GetStartupState ignored.\n{0}\n{1}", ex.Message, ex.StackTrace));
			}

			if (_Toolbar == null)		// Use this as the default toolbar
				_Toolbar = this.ToolbarDefault;
			return;
		}

		private void SaveStartupState()
		{
			try
			{
				int[] colors = GetCustomColors();		// get custom colors

				XmlDocument xDoc = new XmlDocument();
				XmlProcessingInstruction xPI;
				xPI = xDoc.CreateProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
				xDoc.AppendChild(xPI);

				XmlNode xDS = xDoc.CreateElement("designerstate");
				xDoc.AppendChild(xDS);

				XmlNode xN;
				// Loop thru the current files
				XmlNode xFiles = xDoc.CreateElement("CurrentFiles");
				xDS.AppendChild(xFiles);
				foreach (MDIChild mc in this.MdiChildren)
				{
					string file = mc.SourceFile;
					if (file == null)
						continue;
					xN = xDoc.CreateElement("file");
					xN.InnerText = file;
					xFiles.AppendChild(xN);
				}

				// Recent File Count
				XmlNode rfc = xDoc.CreateElement("RecentFilesMax");
				xDS.AppendChild(rfc);
				rfc.InnerText = this._RecentFilesMax.ToString();

				// Loop thru recent files list
				xFiles = xDoc.CreateElement("RecentFiles");
				xDS.AppendChild(xFiles);
				foreach(string f in _RecentFiles.Values)
				{
					xN = xDoc.CreateElement("file");
					xN.InnerText = f;
					xFiles.AppendChild(xN);
				}

				// Help File URL
				XmlNode hfu = xDoc.CreateElement("Help");
				xDS.AppendChild(hfu);
				hfu.InnerText = this._HelpUrl;

                // Show Line numbers
                XmlNode bln = xDoc.CreateElement("EditLines");
                xDS.AppendChild(bln);
                bln.InnerText = this._ShowEditLines ? "true" : "false";

                // Outline reportitems
                XmlNode ori = xDoc.CreateElement("OutlineReportItems");
                xDS.AppendChild(ori);
                ori.InnerText = this.ShowReportItemOutline ? "true" : "false";

                // ShowTabbedInterface
                XmlNode sti = xDoc.CreateElement("ShowTabbedInterface");
                xDS.AppendChild(sti);
                sti.InnerText = this._ShowTabbedInterface ? "true" : "false";

                // Save the toolbar items
				XmlNode xTB = xDoc.CreateElement("Toolbar");
				xDS.AppendChild(xTB);
				foreach(string t in _Toolbar)
				{
					xN = xDoc.CreateElement("item");
					xN.InnerText = t;
					xTB.AppendChild(xN);
				}

				// Save the custom colors
				StringBuilder sb = new StringBuilder();
				foreach (int c in colors)
				{
					sb.Append(c.ToString());
					sb.Append(",");
				}
				sb.Remove(sb.Length-1, 1);	// remove last ","
				
				xN = xDoc.CreateElement("CustomColors");
				xN.InnerText = sb.ToString();
				xDS.AppendChild(xN);

				// Save the file
				string optFileName = AppDomain.CurrentDomain.BaseDirectory + "designerstate.xml";

				xDoc.Save(optFileName);
			}
			catch{}		// still want to leave even on error

			return ;
		}

		static internal int[] GetCustomColors()
		{
			string optFileName = AppDomain.CurrentDomain.BaseDirectory + "designerstate.xml";
			int white = 16777215;	// default to white (magic number)
			int[] cArray = new int[] {white, white, white, white,white, white, white, white,
								    white, white, white, white, white, white, white, white};
			try
			{
				XmlDocument xDoc = new XmlDocument();
				xDoc.PreserveWhitespace = false;
				xDoc.Load(optFileName);
				XmlNode xNode;
				xNode = xDoc.SelectSingleNode("//designerstate");

				string tcolors="";
				// Loop thru all the child nodes
				foreach(XmlNode xNodeLoop in xNode.ChildNodes)
				{
					if (xNodeLoop.Name != "CustomColors")
						continue;
					tcolors = xNodeLoop.InnerText;
					break;
				}
				string[] colorList= tcolors.Split(',');
				int i=0;

				foreach (string c in colorList)
				{
					try {cArray[i] = int.Parse(c); }
					catch {cArray[i] = white;}
					i++;
					if (i >= cArray.Length)		// Only allow 16 custom colors
						break;
				}
			}
			catch
			{		// Didn't sucessfully get the startup state but don't really care
			}
			return cArray;
		}

		static internal void SetCustomColors(int[] colors)
		{
			string optFileName = AppDomain.CurrentDomain.BaseDirectory + "designerstate.xml";

			StringBuilder sb = new StringBuilder();
			foreach (int c in colors)
			{
				sb.Append(c.ToString());
				sb.Append(",");
			}

			sb.Remove(sb.Length-1, 1);	// remove last ","
			try
			{
				XmlDocument xDoc = new XmlDocument();
				xDoc.PreserveWhitespace = false;
				xDoc.Load(optFileName);
				XmlNode xNode;
				xNode = xDoc.SelectSingleNode("//designerstate");

				// Loop thru all the child nodes
				XmlNode cNode = null;
				foreach(XmlNode xNodeLoop in xNode.ChildNodes)
				{
					if (xNodeLoop.Name == "CustomColors")
					{
						cNode = xNodeLoop;
						break;
					}
				}

				if (cNode == null)
				{
					cNode = xDoc.CreateElement("CustomColors");
					xNode.AppendChild(cNode);
				}

				cNode.InnerText = sb.ToString();

				xDoc.Save(optFileName);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Custom Color Save Failed");
			}
			return;
		}

		private void EditTextbox_Validated(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null || mc == null || 
				mc.DesignTab != "design" || mc.DrawCtl.SelectedCount != 1 ||
				mc.Editor == null)
				return;

			mc.Editor.SetSelectedText(ctlEditTextbox.Text);
		}

		private void Insert_Click(object sender, EventArgs e)
		{
			if (ctlInsertCurrent != null)
				ctlInsertCurrent.Checked = false;

			SimpleToggle ctl = (SimpleToggle) sender;
			ctlInsertCurrent = ctl.Checked? ctl: null;

			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;
			mc.SetFocus();

			mc.CurrentInsert = ctlInsertCurrent == null? null: (string) ctlInsertCurrent.Tag;		
		}

		private void ctlBold_Click(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;
			
			mc.ApplyStyleToSelected("FontWeight", ctlBold.Checked? "Bold": "Normal");

			SetMDIChildFocus(mc);
		}

		private void ctlItalic_Click(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;
			
			mc.ApplyStyleToSelected("FontStyle", ctlItalic.Checked? "Italic": "Normal");

			SetMDIChildFocus(mc);
		}

		private void ctlUnderline_Click(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;
			
			mc.ApplyStyleToSelected("TextDecoration", ctlUnderline.Checked? "Underline": "None");

			SetMDIChildFocus(mc);
		}

		private void ctlForeColor_Change(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;
			
			if (!bSuppressChange)
				mc.ApplyStyleToSelected("Color", ctlForeColor.Text);
			SetMDIChildFocus(mc);
		}

		private void ctlBackColor_Change(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;
			if (!bSuppressChange)
				mc.ApplyStyleToSelected("BackgroundColor", ctlBackColor.Text);
			SetMDIChildFocus(mc);
		}

		private void ctlFont_Change(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;
			
			if (!bSuppressChange)
				mc.ApplyStyleToSelected("FontFamily", ctlFont.Text);
			
			SetMDIChildFocus(mc);
		}

		private void ctlFontSize_Change(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;
			
			if (!bSuppressChange)
				mc.ApplyStyleToSelected("FontSize", ctlFontSize.Text + "pt");

			SetMDIChildFocus(mc);
		}

		private void ctlZoom_Change(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;
			mc.SetFocus();
			
			switch (ctlZoom.Text)
			{
				case "Actual Size":
					mc.Zoom = 1;
					break;
				case "Fit Page":
					mc.ZoomMode = ZoomEnum.FitPage;
					break;
				case "Fit Width":
					mc.ZoomMode = ZoomEnum.FitWidth;
					break;
				default:
					string s = ctlZoom.Text.Substring(0, ctlZoom.Text.Length-1);
					float z;
					try
					{
						z = Convert.ToSingle(s) / 100f;
						mc.Zoom = z;
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message, "Zoom Value Invalid");
					}
					break;
			}
		}

		private void RdlDesigner_MdiChildActivate(object sender, EventArgs e)
		{
			if (this._ValidateRdl != null)		// don't keep the validation open when window changes
				this._ValidateRdl.Close();

			DesignTabChanged(sender, e);
			SelectionChanged(sender, e);
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;
			mc.SetFocus();
            mainTC.SelectTab(mc.Tab);
		}

		private void SetMDIChildFocus(MDIChild mc)
		{
			// We don't want to be triggering any change events when the focus is changing
			bool bSuppress = bSuppressChange;
			bSuppressChange = true;
			mc.SetFocus();
			bSuppressChange = bSuppress;
		}

		private void SetStatusNameAndPosition()
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;

			if (mc == null)
			{
				statusPosition.Text = statusSelected.Text = "";
			}
			else if (mc.DesignTab == "design")
				SetStatusNameAndPositionDesign(mc);
			else if (mc.DesignTab == "edit")
				SetStatusNameAndPositionEdit(mc);
			else
			{
				statusPosition.Text = statusSelected.Text = "";
			}
			return;
		}
		
		private void SetStatusNameAndPositionDesign(MDIChild mc)
		{
			if (mc.DrawCtl.SelectedCount <= 0)
			{
				statusPosition.Text = statusSelected.Text = "";
				return;
			}

			// Handle position
			PointF pos = mc.SelectionPosition;
			SizeF sz = mc.SelectionSize;
			string spos;
			if (pos.X == float.MinValue)	// no item selected is probable cause
				spos = "";
			else
			{
				RegionInfo rinfo = new RegionInfo( CultureInfo.CurrentCulture.LCID );
                double m72 = DesignXmlDraw.POINTSIZED;
				if (rinfo.IsMetric)
				{
					if (sz.Width == float.MinValue)	// item is in a table/matrix is probably cause
						spos =  string.Format("   x={0:0.00}cm, y={1:0.00}cm        ",
                            (pos.X / (m72 / 2.54d)).AsNetFx(), (pos.Y / (m72 / 2.54d)).AsNetFx());
					else
						spos =  string.Format("   x={0:0.00}cm, y={1:0.00}cm, w={2:0.00}cm, h={3:0.00}cm        ", 
							(pos.X / (m72 / 2.54d)).AsNetFx(), (pos.Y / (m72 / 2.54d)).AsNetFx(),
							(sz.Width / (m72 / 2.54d)).AsNetFx(), (sz.Height / (m72 / 2.54d)).AsNetFx());
				}
				else
				{
					if (sz.Width == float.MinValue)
						spos =  string.Format("   x={0:0.00}\", y={1:0.00}\"        ", 
							(pos.X/m72).AsNetFx(), (pos.Y/m72).AsNetFx());
					else
						spos =  string.Format("   x={0:0.00}\", y={1:0.00}\", w={2:0.00}\", h={3:0.00}\"        ", 
							(pos.X/m72).AsNetFx(), (pos.Y/m72).AsNetFx(), (sz.Width/m72).AsNetFx(), (sz.Height/m72).AsNetFx());
				}
			}
			if (spos != statusPosition.Text)
				statusPosition.Text = spos;

			// Handle text
			string sname = mc.SelectionName;
			if (sname != statusSelected.Text)
				statusSelected.Text = sname;
			return;
		}

		private void SetStatusNameAndPositionEdit(MDIChild mc)
		{
			string spos = string.Format("Ln {0}  Ch {1}", mc.CurrentLine, mc.CurrentCh);
			if (spos != statusSelected.Text)
				statusSelected.Text = spos;

			if (statusPosition.Text != "")
				statusPosition.Text = "";

			return;
		}

		private void EditTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			// Force scroll up and down
			switch (e.KeyCode)
			{
				case Keys.Enter:
					mc.SetFocus();
					e.Handled = true;
					break;
				case Keys.Escape:
					if (mc.DrawCtl.SelectedCount == 1)
					{
						XmlNode tn = mc.DrawCtl.SelectedList[0] as XmlNode;
						if (tn != null && tn.Name == "Textbox")
						{
							ctlEditTextbox.Text = mc.DrawCtl.GetElementValue(tn, "Value", "");
							e.Handled = true;
						}
					}
					break;
				default:
					break;
			}

		}

		private void menuFormat_Popup(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;

			// Determine if group operation on selected is currently allowed
			bool bEnable = (mc != null && mc.DesignTab == "design" && mc.DrawCtl.AllowGroupOperationOnSelected);
			
			this.menuFormatAlignB.Enabled = this.menuFormatAlignC.Enabled = 
				this.menuFormatAlignL.Enabled = this.menuFormatAlignM.Enabled = 
				this.menuFormatAlignR.Enabled = this.menuFormatAlignT.Enabled = 
				bEnable;

			menuFormatSizeW.Enabled	= menuFormatSizeH.Enabled = menuFormatSizeB.Enabled = bEnable;

			menuFormatHorzE.Enabled = menuFormatHorzI.Enabled = menuFormatHorzD.Enabled =
				menuFormatHorzZ.Enabled = bEnable;

			menuFormatVertE.Enabled = menuFormatVertI.Enabled = menuFormatVertD.Enabled =
				menuFormatVertZ.Enabled = bEnable;

			bEnable  = (mc != null && mc.DesignTab == "design" && mc.DrawCtl.SelectedCount > 0);
			this.menuFormatPaddingBottomI.Enabled =
				this.menuFormatPaddingBottomD.Enabled =
				this.menuFormatPaddingBottomZ.Enabled =
				this.menuFormatPaddingTopI.Enabled =
				this.menuFormatPaddingTopD.Enabled =
				this.menuFormatPaddingTopZ.Enabled =
				this.menuFormatPaddingLeftI.Enabled =
				this.menuFormatPaddingLeftD.Enabled =
				this.menuFormatPaddingLeftZ.Enabled =
				this.menuFormatPaddingRightI.Enabled = 
				this.menuFormatPaddingRightD.Enabled = 
				this.menuFormatPaddingRightZ.Enabled = 
					bEnable;
		}
		
		private void menuFormatAlignC_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.AlignCenters();
		}
		
		private void menuFormatAlignL_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.AlignLefts();
		}

		private void menuFormatAlignR_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.AlignRights();
		}

		private void menuFormatAlignB_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.AlignBottoms();
		}

		private void menuFormatAlignT_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.AlignTops();
		}

		private void menuFormatAlignM_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.AlignMiddles();
		}

		private void menuFormatSizeH_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.SizeHeights();
		}

		private void menuFormatSizeW_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.SizeWidths();
		}

		private void menuFormatSizeB_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.SizeBoth();
		}

		private void menuFormatHorzE_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.HorzSpacingMakeEqual();
		}

		private void menuFormatHorzI_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.HorzSpacingIncrease();
		}

		private void menuFormatHorzD_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.HorzSpacingDecrease();
		}

		private void menuFormatHorzZ_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.HorzSpacingMakeZero();
		}

		private void menuFormatVertE_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.VertSpacingMakeEqual();
		}

		private void menuFormatVertI_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.VertSpacingIncrease();
		}

		private void menuFormatVertD_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.VertSpacingDecrease();
		}

		private void menuFormatVertZ_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			mc.Editor.DesignCtl.VertSpacingMakeZero();
		}

		private void menuView_Popup(object sender, EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			bool bEnable = mc != null;

			menuViewDesigner.Enabled = menuViewRDL.Enabled =
				menuViewPreview.Enabled = bEnable;

			menuViewProperties.Enabled = bEnable && mc.DesignTab == "design";
		}

		private void menuViewDesigner_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;
			mc.RdlEditor.DesignTab = "design";
		}

		private void menuViewRDL_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;
			mc.RdlEditor.DesignTab = "edit";
		}

		private void menuViewBrowser_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			try
			{
				menuToolsStartProcess(true);		// start desktop if not already up

				DesktopConfig dc = DialogToolOptions.DesktopConfiguration;

				string rdlfile = Path.GetFileNameWithoutExtension(mc.SourceFile) + "_" + (++TEMPRDL_INC).ToString() + TEMPRDL;
				string file; 
				if (Path.IsPathRooted(dc.Directory))
					file = dc.Directory + Path.DirectorySeparatorChar + rdlfile;
				else
					file = AppDomain.CurrentDomain.BaseDirectory +  
						 dc.Directory + Path.DirectorySeparatorChar + rdlfile;

				if (_TempReportFiles == null)
				{
					_TempReportFiles = new List<string>();
					_TempReportFiles.Add(file);
				}
				else
				{
					if (!_TempReportFiles.Contains(file))
						_TempReportFiles.Add(file);
				}
				StreamWriter sw = File.CreateText(file);
				sw.Write(mc.SourceRdl);
				sw.Close();
		 // http://localhost:8080/aReport.rdl?rs:Format=HTML
				string url = string.Format("http://localhost:{0}/{1}?rd:Format=HTML", dc.Port, rdlfile);
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });//The shell opens the link (the .NET default is false).
				
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Unable to Show Report");
			}
			
		}

		private void menuViewPreview_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;
			mc.RdlEditor.DesignTab = "preview";
		}

		private void menuFormatPadding_Click(object sender, System.EventArgs e)
		{
			MDIChild mc = this.ActiveMdiChild as MDIChild;
			if (mc == null)
				return;

			ToolStripMenuItem mi = sender as ToolStripMenuItem;

			string padname=null;
			int paddiff=0;
			if (mi == menuFormatPaddingLeftI)
			{
				padname = "PaddingLeft";
				paddiff = 4;
			}
			else if (mi == menuFormatPaddingLeftD)
			{
				padname = "PaddingLeft";
				paddiff = -4;
			}
			else if (mi == menuFormatPaddingLeftZ)
			{
				padname = "PaddingLeft";
				paddiff = 0;
			}
			else if (mi == menuFormatPaddingRightI)
			{
				padname = "PaddingRight";
				paddiff = 4;
			}
			else if (mi == menuFormatPaddingRightD)
			{
				padname = "PaddingRight";
				paddiff = -4;
			}
			else if (mi == menuFormatPaddingRightZ)
			{
				padname = "PaddingRight";
				paddiff = 0;
			}
			else if (mi == menuFormatPaddingTopI)
			{
				padname = "PaddingTop";
				paddiff = 4;
			}
			else if (mi == menuFormatPaddingTopD)
			{
				padname = "PaddingTop";
				paddiff = -4;
			}
			else if (mi == menuFormatPaddingTopZ)
			{
				padname = "PaddingTop";
				paddiff = 0;
			}
			else if (mi == menuFormatPaddingBottomI)
			{
				padname = "PaddingBottom";
				paddiff = 4;
			}
			else if (mi == menuFormatPaddingBottomD)
			{
				padname = "PaddingBottom";
				paddiff = -4;
			}
			else if (mi == menuFormatPaddingBottomZ)
			{
				padname = "PaddingBottom";
				paddiff = 0;
			}

			if (padname != null)
				mc.Editor.DesignCtl.SetPadding(padname, paddiff);

		}

		private void CleanupTempFiles()
		{
			if (_TempReportFiles == null)
				return;
			foreach (string file in _TempReportFiles)
			{
				try		
				{	// It's ok for the delete to fail
					File.Delete(file);
				}
				catch
				{}
			}
			_TempReportFiles = null;
		}
	}
}
