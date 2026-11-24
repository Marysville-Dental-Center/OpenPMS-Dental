"""UI-13 (docs/port-notes/UI-13.md, step 1): the inventory of every element type the program shows, with its look.

Lists every control class the designer files create and the code creates with "new" (stock Windows Forms types and the program's own), every
owner-drawn part of the program's controls (the grid's header, rows, scroll bars and page bar, tab strips, menus, the time pattern bar ...)
and the WPF controls the program's WPF windows use, with a count of uses, where each is drawn (stock / own control / owner-drawn part / WPF),
whether it is in the new look and by which code, its row in tests/UiRefreshTests/Data/coverage.tsv (or the reason it has none), and the fix.

    python tools/UiFit/element_inventory.py            writes docs/port-notes/UI-13-inventory.tsv
    python tools/UiFit/element_inventory.py --check    compares with the committed file (exit 1 when it differs)

No packages; deterministic (sorted). The counts: "designer" counts `this.<name> = new <Type>();` lines in the designer files of the program's
projects (OpenPmsDental, OpenPmsDentalBusiness, CodeBase, SparksToothChart, and since UI-PLAIN the plain exes and the graph library:
PLAIN_ROOTS); "code" counts `new <Type>(` in their other .cs files, by name
(so a type whose short name is also another library's counts those too: the number is a size, not an exact census). NoHqTests' coverage
test and UiRefreshTests' ElementInventoryTests read the file: every row is styled (yes) with its code and coverage row, or carries a reason.
"""
import argparse
import collections
import glob
import os
import re
import sys

# UI-PLAIN (docs/port-notes/UI-PLAIN.md): the plain windows count too: the plain exes' and the graph library's designer files (RdlDesign, the
# report designer, is its own slice).
PLAIN_ROOTS = ["CentralManager", "DatabaseIntegrityCheck", "DocumentationBuilder", "MySQLManager", "OpenPmsDentalDbSettings", "OpenPmsDentalGraph",
               "OpenPmsDentalHL7", "ServiceManager", "UpdateFileCopier", "xCrudGenerator"]
ROOTS = ["OpenPmsDental", "OpenPmsDentalBusiness", "CodeBase", "SparksToothChart"] + PLAIN_ROOTS
OUT = "docs/port-notes/UI-13-inventory.tsv"
COLUMNS = ["type", "drawn", "designer", "code", "styled", "by", "coverage", "fix_or_reason"]

# Stock Windows Forms types counted in code by their short name (the visual ones; the designer list adds whatever else the designers use).
STOCK_CODE = ["Label", "TextBox", "Button", "CheckBox", "RadioButton", "ComboBox", "ListBox", "CheckedListBox", "GroupBox", "Panel",
              "FlowLayoutPanel", "TableLayoutPanel", "SplitContainer", "Splitter", "TabControl", "TabPage", "PictureBox", "RichTextBox",
              "MaskedTextBox", "NumericUpDown", "DomainUpDown", "DateTimePicker", "MonthCalendar", "TreeView", "ListView", "DataGridView",
              "ProgressBar", "TrackBar", "VScrollBar", "HScrollBar", "LinkLabel", "ContextMenuStrip", "MenuStrip", "ToolStrip", "StatusStrip",
              "ToolTip", "WebBrowser", "PropertyGrid", "PrintPreviewControl", "ElementHost"]

NO_COVERAGE = "(outside coverage.tsv: it lists OpenPmsDental/UI, CodeBase/UI, WpfControlsOD/UI and WpfControlsOD/Themes)"
WINDOWS_LOOK = "keeps Windows' own look (docs/specs/ui-refresh-plan.md section 1: the Windows dialogs and message boxes)"
NONVISUAL = "draws nothing (a component, a data holder or a part of another row's control)"
COMPOSITE = "draws nothing of its own: a user control whose children are rows of this list"

# type -> (styled, by, coverage, fix_or_reason). styled: yes (in the new look), keeps (a recorded reason), n/a (draws nothing).
LOOKS = {
    # ---- stock Windows Forms types --------------------------------------------------------------------------------------------------------
    "System.Windows.Forms.Label": ("yes", "UI-2a fonts; plain text in Text on the container", NO_COVERAGE, "a label draws its text only: the font tokens (UI-2a/UI-2b) and the grid (UI-12); a LightGray block label gets UiBlockLook, an ActiveCaption grid title UiCaptionLook (Accent, TextOnAccent, the title font) (UI-13)"),
    "System.Windows.Forms.TextBox": ("yes", "UiInputFrame (UI-12) frame; UiScrollLook (UI-13) scroll bars", "CodeBase/UI/UiInputFrame", "single- and multi-line boxes framed like the combo box; their scroll bars in the Fluent look"),
    "System.Windows.Forms.RichTextBox": ("yes", "UiInputFrame (UI-12) frame; UiScrollLook (UI-13) scroll bars", "CodeBase/UI/UiInputFrame", "framed; scroll bars in the Fluent look"),
    "System.Windows.Forms.Button": ("yes", "UiButtonLook (UI-13)", "CodeBase/UI/UiLooks", "the program's button look (flat SurfaceRaised, Stroke, 4 px corners); a button with its own colour as a rounded swatch"),
    "System.Windows.Forms.CheckBox": ("yes", "UiCheckLook (UI-13)", "CodeBase/UI/UiLooks", "the program's check box (box left, the tokens)"),
    "System.Windows.Forms.RadioButton": ("yes", "UiRadioLook (UI-12)", "CodeBase/UI/UiRadioLook", "drawn like the program's check box"),
    "System.Windows.Forms.ComboBox": ("yes", "UiInputFrame (UI-12)", "CodeBase/UI/UiInputFrame", "the combo box's frame and chevron over the stock combo"),
    "System.Windows.Forms.ListBox": ("yes", "UiScrollLook (UI-13) scroll bars; UiBorderLook-free (Windows' flat list border)", "CodeBase/UI/UiScrollLook", "its scroll bars in the Fluent look; the items keep Windows' list (the program's own list is UI.ListBox)"),
    "System.Windows.Forms.GroupBox": ("yes", "UiGroupBoxLook (UI-PLAIN)", "CodeBase/UI/UiLooks", "the plain windows' group boxes (and the program's one hidden legacy group) drawn like UI.GroupBox: 6 px corners, 1 px Stroke, the title inside at the top left"),
    "System.Windows.Forms.Panel": ("yes", "UiBorderLook (UI-13) for bordered panels; UiScrollLook (UI-13) for scrolling ones", "CodeBase/UI/UiLooks", "a 3-D or black border flat StrokeStrong; AutoScroll bars in the Fluent look; a thin dark childless panel (a divider) one 1 px Stroke line (UiSeparatorLook); a plain panel draws only its colour"),
    "System.Windows.Forms.PictureBox": ("yes", "UiBorderLook (UI-13)", "CodeBase/UI/UiLooks", "a 3-D or black border flat StrokeStrong; the picture is content"),
    "System.Windows.Forms.NumericUpDown": ("yes", "UiInputFrame and UiUpDownLook (UI-12)", "CodeBase/UI/UiUpDownLook", "framed; its arrows as the combo box's chevrons"),
    "System.Windows.Forms.DomainUpDown": ("yes", "UiInputFrame and UiUpDownLook (UI-12)", "CodeBase/UI/UiUpDownLook", "framed; its arrows as the combo box's chevrons"),
    "System.Windows.Forms.DateTimePicker": ("yes", "UiInputFrame (UI-12; UI-13 fix round 2: the themed picker's whole drop button covered, the value in Text)", "CodeBase/UI/UiInputFrame", "framed with the combo box's chevron over Windows' whole drop button (DTM_GETDATETIMEPICKERINFO), the value recoloured to the Text token, a disabled picker's box in the disabled grey; its drop-down calendar is the next row"),
    "System.Windows.Forms.DateTimePicker (drop-down calendar)": ("yes", "UiLooks.SetDateTimePickerLook (UI-13): the token colours", "CodeBase/UI/UiLooks", "Windows' own calendar pop-up (a window the picker owns; no paint hook): with Windows 11's theme Windows draws its themed calendar, without a theme (the gallery's harness) the token colours (title Accent, days Text on SurfaceRaised, other months TextDisabled); the program's ODDatePicker (UI-4b) is the date picker drawn in the tokens"),
    "System.Windows.Forms.MonthCalendar": ("yes", "UiLooks.SetMonthCalendarLook (UI-13): the token colours", "CodeBase/UI/UiLooks", "Windows' month calendar control (no owner drawing: Windows draws the days, header and hot tracking in one window): Windows 11's themed calendar (flat arrows, today in a themed frame; the gallery draws it with visual styles), and the token colours where Windows draws it without a theme; the program's own calendar is MonthCalendarOD (UI-4b), but swapping the 33 stock ones is a behaviour change (their events and selection ranges differ)"),
    "System.Windows.Forms.TreeView": ("yes", "UiLooks.AttachExplorerTheme and UiScrollLook (UI-13)", "CodeBase/UI/UiLooks", "Windows 11's Explorer theme (its flat hover and selection, the theme's expand glyph), no dotted lines (ShowLines off); scroll bars in the Fluent look"),
    "System.Windows.Forms.ListView": ("yes", "UiLooks.AttachExplorerTheme and UiScrollLook (UI-13)", "CodeBase/UI/UiLooks", "Windows 11's Explorer theme; scroll bars in the Fluent look"),
    "System.Windows.Forms.DataGridView": ("yes", "UiLooks.SetDataGridViewLook (UI-13); its scroll bars UiScrollLook", "CodeBase/UI/UiLooks", "header SurfaceSubtle with single Gridline lines (no 3-D bevel), lines Gridline, selection AccentSubtle, its frame StrokeStrong"),
    "System.Windows.Forms.ProgressBar": ("yes", "UiProgressLook (UI-13)", "CodeBase/UI/UiLooks", "a rounded Stroke track with the Accent fill"),
    "System.Windows.Forms.TrackBar": ("yes", "UiTrackBarLook (UI-13)", "CodeBase/UI/UiTrackBarLook", "the Fluent slider: thin Stroke track, Accent fill, round thumb, StrokeStrong ticks"),
    "System.Windows.Forms.VScrollBar": ("yes", "UiScrollLook (UI-13)", "CodeBase/UI/UiScrollLook", "flat Surface track, StrokeStrong pill thumb, Fluent chevrons"),
    "System.Windows.Forms.HScrollBar": ("yes", "UiScrollLook (UI-13)", "CodeBase/UI/UiScrollLook", "flat Surface track, StrokeStrong pill thumb, Fluent chevrons"),
    "System.Windows.Forms.LinkLabel": ("yes", "UiLooks.SetLinkLabelLook (UI-13)", "CodeBase/UI/UiLooks", "links in Accent"),
    "System.Windows.Forms.TableLayoutPanel": ("n/a", "", NO_COVERAGE, "draws nothing: a layout container"),
    "System.Windows.Forms.FlowLayoutPanel": ("n/a", "", NO_COVERAGE, "draws nothing: a layout container"),
    "System.Windows.Forms.SplitContainer": ("yes", "UiPlainWindow (UI-PLAIN): the window background token", "CodeBase/UI/UiPlainWindow", "Windows Forms draws its splitter flat in the container's colour (no bevel), the window background token in the plain windows (their graph and dashboard settings panels); the program's own is UI.SplitContainer (UI-4b)"),
    "System.Windows.Forms.TabControl": ("yes", "UiTabLook (UI-PLAIN)", "CodeBase/UI/UiTabLook", "the dashboard's stock tab control (and one made in code) painted over in the program's tab look: the strip SurfaceSubtle, the page's Stroke border, the selected tab's Accent line, at Windows' tab rectangles; bottom, side and button tabs keep Windows' drawing (none in use)"),
    "System.Windows.Forms.TabPage": ("n/a", "", NO_COVERAGE, "draws its colour only (UiTabLook gives a stock page the colour behind its tab control); its tab is drawn by its tab control"),
    "System.Windows.Forms.ContextMenuStrip": ("yes", "UiToolStripRenderer (UI-6)", NO_COVERAGE, "the token renderer every tool strip of the UI thread uses (ToolStripManager.Renderer)"),
    "System.Windows.Forms.MenuStrip": ("yes", "UiToolStripRenderer (UI-6)", NO_COVERAGE, "the token renderer"),
    "System.Windows.Forms.ToolStrip": ("yes", "UiToolStripRenderer (UI-6)", NO_COVERAGE, "the token renderer"),
    "System.Windows.Forms.StatusStrip": ("yes", "UiToolStripRenderer (UI-6)", NO_COVERAGE, "the token renderer"),
    "System.Windows.Forms.ToolStripMenuItem": ("yes", "UiToolStripRenderer (UI-6)", NO_COVERAGE, "a menu item: the token renderer draws it"),
    "System.Windows.Forms.ToolStripSeparator": ("yes", "UiToolStripRenderer (UI-6)", NO_COVERAGE, "a menu separator: the token renderer draws it"),
    "System.Windows.Forms.ToolTip": ("keeps", "", NO_COVERAGE, "a component, not a control: Windows 11 draws its themed tooltip (rounded, flat); the program's own is ToolTipOD (UI-4a)"),
    "System.Windows.Forms.ErrorProvider": ("keeps", "", NO_COVERAGE, "Windows' error icon beside a field (a component)"),
    "System.Windows.Forms.PrintPreviewControl": ("yes", "UiScrollLook (UI-13) scroll bars", "CodeBase/UI/UiScrollLook", "the pages are documents; the scroll bars in the Fluent look (the program's own is ODPrintPreviewControl, UI-4c)"),
    "System.Windows.Forms.PrintPreviewDialog": ("keeps", "", NO_COVERAGE, WINDOWS_LOOK),
    "System.Windows.Forms.Integration.ElementHost": ("n/a", "", NO_COVERAGE, "hosts a WPF control: the WPF rows below"),
    "System.Windows.Forms.ColorDialog": ("keeps", "", NO_COVERAGE, WINDOWS_LOOK),
    "System.Windows.Forms.FontDialog": ("keeps", "", NO_COVERAGE, WINDOWS_LOOK),
    "System.Windows.Forms.OpenFileDialog": ("keeps", "", NO_COVERAGE, WINDOWS_LOOK),
    "System.Windows.Forms.SaveFileDialog": ("keeps", "", NO_COVERAGE, WINDOWS_LOOK),
    "System.Windows.Forms.FolderBrowserDialog": ("keeps", "", NO_COVERAGE, WINDOWS_LOOK),
    "System.Windows.Forms.PrintDialog": ("keeps", "", NO_COVERAGE, WINDOWS_LOOK),
    "System.Windows.Forms.PageSetupDialog": ("keeps", "", NO_COVERAGE, WINDOWS_LOOK),
    "System.Windows.Forms.MessageBox": ("keeps", "", "OpenPmsDental/UI/MessageBox", WINDOWS_LOOK),
    "System.Windows.Forms.WebBrowser": ("keeps", "", NO_COVERAGE, "web content (section 1: HTML is out of scope)"),
    "System.Windows.Forms.CheckedListBox": ("yes", "UiScrollLook (UI-13) scroll bars", "CodeBase/UI/UiScrollLook", "scroll bars in the Fluent look; its items keep Windows' list"),
    "System.Windows.Forms.MaskedTextBox": ("yes", "UiInputFrame (UI-12)", "CodeBase/UI/UiInputFrame", "framed like the combo box"),
    "System.Windows.Forms.Splitter": ("keeps", "", NO_COVERAGE, "Windows Forms draws it flat in the container's colour"),
    "System.Windows.Forms.PropertyGrid": ("keeps", "", NO_COVERAGE, "a developer tool's grid"),
    "System.Windows.Forms.DataGridViewCellStyle": ("n/a", "", NO_COVERAGE, NONVISUAL),
    "System.Windows.Forms.Timer": ("n/a", "", NO_COVERAGE, NONVISUAL),
    "System.Windows.Forms.ImageList": ("n/a", "", NO_COVERAGE, NONVISUAL),
    "System.Windows.Forms.ColumnHeader": ("n/a", "", NO_COVERAGE, NONVISUAL),
    "System.Windows.Forms.BindingSource": ("n/a", "", NO_COVERAGE, NONVISUAL),
    "System.ComponentModel.Container": ("n/a", "", NO_COVERAGE, NONVISUAL),
    "System.ComponentModel.BackgroundWorker": ("n/a", "", NO_COVERAGE, NONVISUAL),
    "System.Drawing.Printing.PrintDocument": ("n/a", "", NO_COVERAGE, NONVISUAL),
    "System.Windows.Forms.DataVisualization.Charting.Chart": ("keeps", "", NO_COVERAGE, "the graph library's charts (GraphQuantityOverTime, GraphIncome): content (section 1: reports and graphs), drawn by the chart library in the graph's own colours"),
    # ---- the graph library (UI-PLAIN) -------------------------------------------------------------------------------------------------------
    "OpenPmsDentalGraph.Legend": ("keeps", "", NO_COVERAGE, "the graph's legend: chart content (the series' colour keys); its four scroll buttons are stock buttons in the program's look (UiButtonLook)"),
    "OpenPmsDentalGraph.Legend.PanelNoFlicker": ("keeps", "", NO_COVERAGE, "the legend's drawing surface: chart content"),
    # ---- the program's own controls -----------------------------------------------------------------------------------------------------
    "OpenPmsDental.UI.Button": ("yes", "UI.Button (UI-4a)", "OpenPmsDental/UI/Button", ""),
    "OpenPmsDental.UI.CheckBox": ("yes", "UI.CheckBox (UI-4a)", "OpenPmsDental/UI/CheckBox", ""),
    "OpenPmsDental.UI.GroupBox": ("yes", "UI.GroupBox (UI-4a)", "OpenPmsDental/UI/GroupBox", ""),
    "OpenPmsDental.UI.PanelOD": ("yes", "PanelOD (UI-4a)", "OpenPmsDental/UI/PanelOD", ""),
    "OpenPmsDental.UI.ComboBox": ("yes", "UI.ComboBox (UI-4b)", "OpenPmsDental/UI/ComboBox", ""),
    "OpenPmsDental.UI.ListBox": ("yes", "UI.ListBox (UI-4b); its scroll bar UiScrollLook (UI-13)", "OpenPmsDental/UI/ListBox", "the list's own VScrollBar in the Fluent look (owner row 'Scrolling still using old style')"),
    "OpenPmsDental.UI.TabControl": ("yes", "UI.TabControl (UI-4b)", "OpenPmsDental/UI/TabControl/*", ""),
    "OpenPmsDental.UI.TabPage": ("yes", "UI.TabPage (UI-4b)", "OpenPmsDental/UI/TabControl/*", ""),
    "OpenPmsDental.UI.SplitContainer": ("yes", "UI.SplitContainer (UI-4b)", "OpenPmsDental/UI/Splitters/*", ""),
    "OpenPmsDental.UI.SplitterPanel": ("yes", "UI.SplitterPanel (UI-4b)", "OpenPmsDental/UI/Splitters/*", ""),
    "OpenPmsDental.UI.MonthCalendarOD": ("yes", "MonthCalendarOD (UI-4b)", "OpenPmsDental/UI/MonthCalendarOD", ""),
    "OpenPmsDental.UI.ODDatePicker": ("yes", "ODDatePicker (UI-4b)", "OpenPmsDental/UI/ODDatePicker", ""),
    "OpenPmsDental.UI.ODDateRangePicker": ("yes", "ODDateRangePicker (UI-4b)", "OpenPmsDental/UI/ODDateRangePicker", ""),
    "OpenPmsDental.UI.ComboBoxClinicPicker": ("yes", "ComboBoxClinicPicker (UI-4b)", "OpenPmsDental/UI/ClinicSelector/ComboBoxClinicPicker", ""),
    "OpenPmsDental.UI.ToolBarOD": ("yes", "ToolBarOD (UI-6)", "OpenPmsDental/UI/ToolBarOD", ""),
    "OpenPmsDental.UI.MenuOD": ("yes", "MenuOD (UI-6)", "OpenPmsDental/UI/MenuOD", ""),
    "OpenPmsDental.UI.ODButtonPanel": ("yes", "ODButtonPanel (UI-6)", "OpenPmsDental/UI/ODButtonPanel", ""),
    "OpenPmsDental.UI.LightSignalGrid": ("yes", "LightSignalGrid (UI-6)", "OpenPmsDental/UI/LightSignalGrid", ""),
    "OpenPmsDental.UI.ODColorPicker": ("yes", "ODColorPicker (UI-4c)", "OpenPmsDental/UI/ODColorPicker", ""),
    "OpenPmsDental.UI.ODCodeRangeFilter": ("yes", "ODCodeRangeFilter (UI-4c)", "OpenPmsDental/UI/ODCodeRangeFilter", ""),
    "OpenPmsDental.UI.ODPictureBox": ("yes", "ODPictureBox (UI-4c)", "OpenPmsDental/UI/ODPictureBox", "border only; the picture is content"),
    "OpenPmsDental.UI.ODPrintPreviewControl": ("yes", "ODPrintPreviewControl (UI-4c)", "OpenPmsDental/UI/ODPrintPreviewControl", ""),
    "OpenPmsDental.UI.WarningIntegrity": ("yes", "WarningIntegrity (UI-4c)", "OpenPmsDental/UI/DatabaseIntegrity/WarningIntegrity", ""),
    "OpenPmsDental.UI.ControlDoubleBuffered": ("n/a", "", "OpenPmsDental/UI/ControlDoubleBuffered", "a double-buffered base control: its owner draws on it"),
    "OpenPmsDental.UI.WindowingSlider": ("yes", "WindowingSlider (UI-4c)", "OpenPmsDental/UI/ImagesModule/WindowingSlider", ""),
    "OpenPmsDental.UI.ToggleDayWeek": ("yes", "ToggleDayWeek (UI-4c)", "OpenPmsDental/UI/Appt/ToggleDayWeek", ""),
    "OpenPmsDental.UI.PinBoard": ("yes", "PinBoard (UI-4c)", "OpenPmsDental/UI/Appt/PinBoard", ""),
    "OpenPmsDental.UI.ControlApptProvSlider": ("yes", "ControlApptProvSlider (UI-4c)", "OpenPmsDental/UI/Appt/ControlApptProvSlider", "the appointment edit window's time pattern"),
    "OpenPmsDental.UI.ControlApptPanel": ("keeps", "", "OpenPmsDental/UI/Appt/ControlApptPanel*", "appointment-book content (UQ-8); its two scroll bars in the Fluent look (UiScrollLook, UI-13)"),
    "OpenPmsDental.UI.ODGridPageNav": ("yes", "ODGridPageNav (UI-12 pilot: flat page buttons, the current page in Accent, chevrons)", NO_COVERAGE, ""),
    "OpenPmsDental.UI.GridOD": ("yes", "GridOD and GridScreenLook (UI-5); its scroll bars UiScrollLook (UI-13)", NO_COVERAGE, "the grid's parts are the owner-drawn rows below"),
    "OpenPmsDental.UI.GridOld": ("yes", "GridOld's scroll bars UiScrollLook (UI-13)", NO_COVERAGE, "the old grid (one use) keeps its cell drawing; its scroll bars in the Fluent look"),
    "OpenPmsDental.UI.SignatureBoxWrapper": ("yes", "SignatureBoxWrapper (UI-4c)", NO_COVERAGE, "border only; the signature is content"),
    "OpenPmsDental.UI.SignatureBox": ("keeps", "", NO_COVERAGE, "the signature pad: content"),
    "OpenPmsDental.ValidDouble": ("yes", "UiInputFrame (UI-12)", "CodeBase/UI/UiInputFrame", "a text box: framed like the combo box"),
    "OpenPmsDental.ValidNum": ("yes", "UiInputFrame (UI-12)", "CodeBase/UI/UiInputFrame", "a text box: framed like the combo box"),
    "OpenPmsDental.ValidDate": ("yes", "UiInputFrame (UI-12)", "CodeBase/UI/UiInputFrame", "a text box: framed like the combo box"),
    "OpenPmsDental.ValidPhone": ("yes", "UiInputFrame (UI-12)", "CodeBase/UI/UiInputFrame", "a text box: framed like the combo box"),
    "OpenPmsDental.ValidTime": ("yes", "UiInputFrame (UI-12)", "CodeBase/UI/UiInputFrame", "a text box: framed like the combo box"),
    "OpenPmsDental.ODtextBox": ("yes", "UiInputFrame (UI-12); UiScrollLook (UI-13)", "CodeBase/UI/UiInputFrame", "a rich text box: framed; scroll bars in the Fluent look"),
    "OpenPmsDental.ODcodeBox": ("yes", "UiInputFrame (UI-12); UiScrollLook (UI-13)", "CodeBase/UI/UiInputFrame", "a rich text box: framed; scroll bars in the Fluent look"),
    "OpenPmsDental.TextBoxWiki": ("yes", "UiInputFrame (UI-12); UiScrollLook (UI-13)", "CodeBase/UI/UiInputFrame", "a rich text box: framed; scroll bars in the Fluent look"),
    "OpenPmsDental.TableTimeBar": ("yes", "TableTimeBar.PaintModern (UI-13)", NO_COVERAGE, "a framed column of 5-minute cells, filled cells Accent/AccentSubtle, cells, letters and slider steps at the window's scale (owner row 'Time pattern on the left looks odd')"),
    "OpenPmsDental.ToothChartTextBelowStrip": ("yes", "ToothChartTextBelowStrip (UI-4c)", NO_COVERAGE, ""),
    "OpenPmsDental.ContrPerio": ("keeps", "", NO_COVERAGE, "the perio chart: clinical content (section 1)"),
    "OpenPmsDental.GraphScheduleDay": ("keeps", "", NO_COVERAGE, "a schedule graph: content"),
    "OpenPmsDental.ControlImageDock": ("yes", "ControlImageDock (UI-4c, the image dock's header)", NO_COVERAGE, ""),
    "OpenPmsDental.PanelScrollManual": ("yes", "UiScrollLook (UI-13) when it scrolls", NO_COVERAGE, "a sheet's scrolling panel: its scroll bars in the Fluent look"),
    "OpenPmsDental.SheetEditMobilePreviewPanel": ("keeps", "", NO_COVERAGE, "a sheet preview: content"),
    "OpenPmsDental.SheetComboBox": ("keeps", "", NO_COVERAGE, "a sheet field's combo box: sheet content (section 1)"),
    "OpenPmsDental.DashFamilyInsurance": ("yes", "ColorOD (UI-11) and its outline in Stroke (UI-13)", NO_COVERAGE, "a patient dashboard widget: its fill from ColorOD, its rounded outline Stroke (was Silver)"),
    "OpenPmsDental.DashIndividualInsurance": ("yes", "ColorOD (UI-11) and its outline in Stroke (UI-13)", NO_COVERAGE, "a patient dashboard widget: its fill from ColorOD, its rounded outline Stroke (was Silver)"),
    "SparksToothChart.ToothChart2D": ("keeps", "", NO_COVERAGE, "the tooth chart's 2-D drawing: clinical content (Open Dental's newest release's look)"),
    "SparksToothChart.ToothChartWrapper": ("keeps", "", NO_COVERAGE, "the tooth chart: clinical content (its look follows Open Dental's newest release)"),
    "SparksToothChart.ToothChartData": ("n/a", "", NO_COVERAGE, NONVISUAL),
    "CodeBase.Controls.ODWebView2": ("keeps", "", NO_COVERAGE, "web content (section 1)"),
    "ODWebView2": ("keeps", "", NO_COVERAGE, "web content (section 1)"),
    "CodeBase.PrintPanel": ("n/a", "", NO_COVERAGE, "a print surface: documents"),
    "CodeBase.UiProgressRing": ("yes", "UiProgressRing (UI-LOAD)", "CodeBase/UI/UiProgressRing", "the busy indicator: an indeterminate Fluent ring in Accent, static when Windows' animations are off"),
    "fyiReporting.RdlViewer.RdlViewer": ("keeps", "", NO_COVERAGE, "the report viewer: reports are documents"),
    "OpenPmsDental.CallFireService.SMSService": ("n/a", "", NO_COVERAGE, NONVISUAL),
    "OpenPmsDental.FolderBrowserDialog": ("keeps", "", "OpenPmsDental/UI/FolderBrowserDialog", WINDOWS_LOOK),
}

# The owner-drawn parts of the program's controls (rows the type list cannot see), with the control whose uses they share.
PARTS = [
    ("part: GridOD header and title", "OpenPmsDental.UI.GridOD", "yes", "GridScreenLook (UI-5)", NO_COVERAGE, ""),
    ("part: GridOD rows, selection and lines", "OpenPmsDental.UI.GridOD", "yes", "GridScreenLook (UI-5)", NO_COVERAGE, ""),
    ("part: GridOD scroll bars", "OpenPmsDental.UI.GridOD", "yes", "UiScrollLook (UI-13); the corner square in the track colour", "CodeBase/UI/UiScrollLook", "owner rows 'Scrolling still using old style', 'Scrolling bar style is old'"),
    ("part: GridOD page bar (ODGridPageNav)", "OpenPmsDental.UI.ODGridPageNav", "yes", "ODGridPageNav (UI-12 pilot)", NO_COVERAGE, ""),
    ("part: tab strips", "OpenPmsDental.UI.TabControl", "yes", "UI.TabControl (UI-4b)", "OpenPmsDental/UI/TabControl/*", ""),
    ("part: menus and context menus", "OpenPmsDental.UI.MenuOD", "yes", "MenuOD and UiToolStripRenderer (UI-6)", "OpenPmsDental/UI/MenuOD", ""),
    ("part: toolbars", "OpenPmsDental.UI.ToolBarOD", "yes", "ToolBarOD (UI-6)", "OpenPmsDental/UI/ToolBarOD", ""),
    ("part: the module bar", "", "yes", "ModuleBar (UI-6)", "OpenPmsDental/UI/ModuleBar", ""),
    ("part: the date picker's calendar drop-down (ODDatePicker)", "OpenPmsDental.UI.ODDatePicker", "yes", "MonthCalendarOD (UI-4b)", "OpenPmsDental/UI/MonthCalendarOD", ""),
    ("part: tooltips (ToolTipOD)", "", "yes", "ToolTipOD (UI-4a)", "OpenPmsDental/UI/ToolTipOD", ""),
    ("part: splitters (UI.SplitContainer)", "OpenPmsDental.UI.SplitContainer", "yes", "UI.SplitContainer (UI-4b)", "OpenPmsDental/UI/Splitters/*", ""),
    ("part: progress bars (OdProgressBar)", "", "yes", "OdProgressBar (UI-4c)", "OpenPmsDental/UI/OdProgressBar", ""),
    ("part: the time pattern bar", "OpenPmsDental.TableTimeBar", "yes", "TableTimeBar.PaintModern (UI-13)", NO_COVERAGE, "owner row 'Time pattern on the left looks odd'"),
    ("part: the time pattern's slider handle (a stock button)", "OpenPmsDental.TableTimeBar", "yes", "UiButtonLook swatch (UI-13)", "CodeBase/UI/UiLooks", ""),
    ("part: colour buttons (stock buttons showing a colour)", "", "yes", "UiButtonLook swatch (UI-13)", "CodeBase/UI/UiLooks", "a rounded swatch with a StrokeStrong edge"),
    ("part: the status strip", "", "yes", "UiToolStripRenderer (UI-6)", NO_COVERAGE, ""),
    ("part: message boxes", "", "keeps", "", "OpenPmsDental/UI/MessageBox", WINDOWS_LOOK + "; the copyable one is MsgBoxCopyPaste (UI-4c)"),
    ("part: the aging strip's grey blocks (Ins Rem, DPlan Rem)", "", "yes", "UiBlockLook (UI-13)", "CodeBase/UI/UiLooks", "owner row 'Check spacing top left'"),
    ("part: frame scroll bars of text boxes, lists, tree and list views, scrolling panels", "", "yes", "UiScrollLook (UI-13)", "CodeBase/UI/UiScrollLook", ""),
    ("part: dividers drawn as thin dark panels (FormInsPlan's)", "", "yes", "UiSeparatorLook (UI-13)", "CodeBase/UI/UiLooks", "a 1 px Stroke line in the panel's middle on the parent's colour"),
    ("part: grid titles drawn as ActiveCaption labels (FormQuery's)", "", "yes", "UiCaptionLook (UI-13)", "CodeBase/UI/UiLooks", "Accent with TextOnAccent in the title font token, as GridOD's title"),
    ("part: the input frame's corners printed through WM_PRINT", "", "yes", "UiInputFrame (UI-12; UI-13 clears Windows' corner pixel)", "CodeBase/UI/UiInputFrame", "the dark pixel the Windows border left in each outer corner"),
]

# WPF controls of the program's WPF windows (counted as XAML elements ui:<Type> in WpfControlsOD and OpenPmsDental).
WPF_LOOKS = {
    "ScrollBar": ("yes", "UiLookScroll (UI-13)", "WpfControlsOD/UI/Controls/UiLookScroll", "the Fluent scroll bar in the WPF grid, lists, tree, rich text and pickers"),
    "ScrollViewer": ("yes", "UiLookScroll (UI-13)", "WpfControlsOD/UI/Controls/UiLookScroll", "its scroll bars in the Fluent look"),
}


# WPF controls whose files are not in WpfControlsOD/UI/Controls.
WPF_PATHS = {
    "ProgressBarFlat": "WpfControlsOD/UI/Progress/ProgressBarFlat",
    "WarningIntegrity": "WpfControlsOD/UI/DatabaseIntegrity/WarningIntegrity",
}


def repo_root():
    here = os.path.dirname(os.path.abspath(__file__))
    return os.path.normpath(os.path.join(here, "..", ".."))


def is_skipped(rel):
    parts = rel.split("/")
    return "bin" in parts or "obj" in parts


def read(path):
    with open(path, "rb") as f:
        return f.read().decode("utf-8-sig", errors="replace")


def scan(root):
    designer = collections.Counter()
    code = collections.Counter()
    own_classes = {}
    for top in ROOTS:
        for path in glob.glob(os.path.join(root, top, "**", "*.cs"), recursive=True):
            rel = os.path.relpath(path, root).replace(os.sep, "/")
            if is_skipped(rel):
                continue
            text = read(path)
            for m in re.finditer(r"\bclass (\w+)\s*:\s*([\w.]+)", text):
                own_classes.setdefault(m.group(1), rel)
            if rel.lower().endswith(".designer.cs"):
                for m in re.finditer(r"this\.\w+ = new ([\w.]+)\(\);", text):
                    t = m.group(1)
                    if t.startswith("UI."):
                        t = "OpenPmsDental." + t
                    designer[t] += 1
            else:
                for m in re.finditer(r"\bnew\s+([\w.]+)\s*\(", text):
                    code[m.group(1)] += 1
            if rel.lower().endswith(".cs"):
                code["MessageBox.Show"] += len(re.findall(r"\bMessageBox\.Show\s*\(", text))
    return designer, code, own_classes


def count_code(code, t):
    """The uses of a type in code: `new <name>(` with the name written short or with the namespaces it may carry."""
    short = t.split(".")[-1]
    if t.startswith("System.Windows.Forms.Integration."):
        names = [short, "System.Windows.Forms.Integration." + short, "Integration." + short]
    elif t.startswith("System."):
        names = [short, "System.Windows.Forms." + short]
    elif ".UI." in t:
        names = ["UI." + short, "OpenPmsDental.UI." + short]
    else:
        names = [n for n in code if n == short or n.endswith("." + short)]
    return sum(code.get(n, 0) for n in set(names))


def is_drawing(root, rel):
    text = read(os.path.join(root, rel))
    return re.search(r"OnPaint\b|\.Paint\s*\+=|DrawString|FillRectangle|DrawLine", text) is not None


def build(root):
    designer, code, own_classes = scan(root)
    types = set(designer)
    types.update("System.Windows.Forms." + s for s in STOCK_CODE if count_code(code, "System.Windows.Forms." + s) > 0)
    types.add("System.Windows.Forms.MessageBox")
    types.add("System.Windows.Forms.DateTimePicker (drop-down calendar)")
    rows = []
    for t in sorted(types):
        if t == "System.Windows.Forms.Integration.ElementHost" or t.startswith("System."):
            drawn = "stock"
        else:
            drawn = "own control"
        nd = designer.get(t, 0)
        nc = 0 if "(" in t else count_code(code, t)
        if t == "System.Windows.Forms.MessageBox":
            nc = code.get("MessageBox.Show", 0)
            drawn = "stock"
        if t in LOOKS:
            styled, by, coverage, fix = LOOKS[t]
        elif drawn == "own control":
            rel = own_classes.get(t.split(".")[-1])
            if rel and not is_drawing(root, rel):
                styled, by, coverage, fix = "n/a", "", NO_COVERAGE, COMPOSITE
            else:
                styled, by, coverage, fix = "?", "", "", "not in the inventory's table: add its look or its reason (" + (rel or "class not found") + ")"
        else:
            styled, by, coverage, fix = "?", "", "", "not in the inventory's table: add its look or its reason"
        rows.append([t, drawn, str(nd), str(nc), styled, by, coverage, fix])
    for name, host, styled, by, coverage, fix in PARTS:
        nd = designer.get(host, 0) if host else 0
        nc = count_code(code, host) if host else 0
        rows.append([name, "owner-drawn part", str(nd), str(nc), styled, by, coverage, fix])
    xaml = collections.Counter()
    for top in ("WpfControlsOD", "OpenPmsDental"):
        for path in glob.glob(os.path.join(root, top, "**", "*.xaml"), recursive=True):
            rel = os.path.relpath(path, root).replace(os.sep, "/")
            if is_skipped(rel):
                continue
            text = read(path)
            for m in re.finditer(r"<ui:(\w+)\b", text):
                xaml["ui:" + m.group(1)] += 1
            for m in re.finditer(r"<(ScrollViewer|ScrollBar)\b", text):
                xaml[m.group(1)] += 1
    coverage_rows = read_coverage(root)
    for t in sorted(xaml):
        if t in ("ScrollViewer", "ScrollBar"):
            styled, by, coverage, fix = WPF_LOOKS[t]
        else:
            short = t[3:]
            cov = WPF_PATHS.get(short, "WpfControlsOD/UI/Controls/" + short)
            match = [c for c in coverage_rows if c == cov or (c.endswith("*") and cov.startswith(c[:-1]))]
            if match and coverage_rows[match[0]] in ("keeps", "no-drawing"):
                styled, by, coverage, fix = ("keeps" if coverage_rows[match[0]] == "keeps" else "n/a"), "", match[0], "its coverage row: " + coverage_rows[match[0]]
            elif match:
                styled, by, coverage, fix = "yes", "WpfControlsOD's " + short + " (" + coverage_rows[match[0]] + ")", match[0], ""
            else:
                styled, by, coverage, fix = "?", "", "", "a WPF control without a coverage row"
        rows.append(["WPF " + t, "wpf", str(xaml[t]), "0", styled, by, coverage, fix])
    return rows


def read_coverage(root):
    rows = {}
    for line in read(os.path.join(root, "tests", "UiRefreshTests", "Data", "coverage.tsv")).splitlines():
        if not line or line.startswith("#") or line.startswith("files\t"):
            continue
        f = line.split("\t")
        if len(f) >= 2:
            rows[f[0]] = f[1]
    return rows


def render(rows):
    lines = ["# UI-13 (docs/port-notes/UI-13.md): every element type the program shows and its look. Written by tools/UiFit/element_inventory.py",
             "# (do not edit by hand: change the tool's tables and run it). drawn: stock (Windows Forms), own control, owner-drawn part, wpf.",
             "# designer/code: uses in designer files / 'new' in code (by name). styled: yes (in the new look, by the code named), keeps (Windows'",
             "# own or content, the reason given), n/a (draws nothing). coverage: the row of tests/UiRefreshTests/Data/coverage.tsv, or why none.",
             "\t".join(COLUMNS)]
    for r in rows:
        lines.append("\t".join(x.replace("\t", " ") for x in r))
    return "\n".join(lines) + "\n"


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--check", action="store_true")
    args = p.parse_args()
    root = repo_root()
    text = render(build(root))
    path = os.path.join(root, OUT)
    if args.check:
        old = read(path).replace("\r\n", "\n") if os.path.exists(path) else ""
        if old != text:
            print("UI-13-inventory.tsv differs from the tool's output: run tools/UiFit/element_inventory.py")
            return 1
        print("UI-13-inventory.tsv matches")
        return 0
    with open(path, "wb") as f:
        f.write(text.encode("utf-8"))
    unknown = [r for r in text.splitlines() if "\t?\t" in r]
    print("rows", len(text.splitlines()) - 5, "unknown", len(unknown))
    for r in unknown:
        print("  ", r.split("\t")[0], "|", r.split("\t")[-1])
    return 0


if __name__ == "__main__":
    sys.exit(main())
