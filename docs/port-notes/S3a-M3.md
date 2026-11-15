# Port note: S3a-M3 (remaining forms, grids and main menus)

Package S3a-M3 of stage 3a of the .NET 10 move ([docs/specs/net10-plan.md](../specs/net10-plan.md), the S3a-M3 row, wave 2 after S3a-M1, next
to S3a-M2). The lead split it on 2026-11-15: **S3a-M3a** (this note so far) does the row for every file except
`OpenPmsDental/Forms/FormMedical.cs`, which is EHR-7's until that package merges (it is the only EHR-7 file with WinForms menus);
**S3a-M3b** converts FormMedical's two menus after EHR-7 merges and removes the bridges they still need (section 5).

S3a-M3a: branch `net10/s3am3a-1` from `main` (`92f7506a0`). Every WinForms `ContextMenu`, `MenuItem`, `MainMenu` and `Menu` left after S3a-M1
and S3a-M2, and every WinForms `DataGrid`, moves to `ContextMenuStrip`, `ToolStripMenuItem`/`ToolStripSeparator`, `MenuStrip` and `DataGridView`.
On .NET 10 the old types are binary-compatibility shims that cannot be instantiated (WFDEV006,
https://learn.microsoft.com/en-us/dotnet/desktop/winforms/wfdev-diagnostics/wfdev006). The conversion follows S3a-M1 section 3.2 and S3a-M2
section 1. No schema change, no new package or reference, nothing downloaded, no window shown.

Commits: `8d782f580` (forms and reports), `5edb9a9de` (the DataGrids), `a6b60a891` (the report designer's and CentralManager's main menus),
`15fcf3af3` (bridges), `78cb5f6c2` (LayoutManagerForms), `5732fc218` (tests), `32a42aca9` (DGCBColumn fix), and this note with the plan's
status line. Line numbers are those of `main` at `92f7506a0`.

## 1. The files and what changed

The rules, as in S3a-M2: every menu keeps its items, texts (with their ampersands), order, separators, shortcuts (`Shortcut.CtrlX` became
`Keys.Control|Keys.X`), `Visible`/`Enabled`/`Checked` states, click handlers and translation keys; a menu's `Popup` handler is its `Opening`
handler (a submenu's, its `DropDownOpening`); `MenuItems` is `Items` (a submenu's `DropDownItems`); `MenuItem.Index` is
`Owner.Items.IndexOf(item)`; `new MenuItem(text,handler)` is `new ToolStripMenuItem(text,null,handler)`; `MenuItem("-")` is a
`ToolStripSeparator`; a designer menu is a `ContextMenuStrip(this.components)` (a container was added to the eleven designers that had none;
each already disposed `components`). A grid that had a caller's `ContextMenu`, and so showed the grid's own items (Copy Cell Text, Copy Rows,
the links) after the caller's, now has the strip as its `ContextMenuStrip` with `DoAddGridItemsToContextMenuStrip=true`, with the handlers in
the order the Popups had. A toolbar drop-down moves from `ODToolBarButton.DropDownMenu` to `ContextMenuStripDropDown`. A menu that its Opening
fills sets `e.Cancel=false` once it has added items (a strip with no shown items starts its Opening cancelled, S3a-M1 section 3.2).

| Group | Files | Notes |
|---|---|---|
| Forms with designer menus | `FormApptConflicts`, `FormApptEdit`, `FormClaimAttachPasteDXC`, `FormClaimEdit`, `FormClaimPayBatch`, `FormClaimsSend`, `FormEmailEdit`, `FormEmailTemplateEdit`, `FormLabCases`, `FormPayment`, `FormRadOrderList`, `FormRecallList`, `FormRecurringChargesHistory`, `FormScreenGroupEdit`, `FormWiki` (`.cs` and designer) | Designer conversion as above; the code that sets the menus on grids, text boxes, list boxes, a panel and buttons sets `ContextMenuStrip`. `FormRecallList` and `FormInsVerificationList` refill their strip in its Opening and un-cancel it. `FormClaimPayBatch` and `FormTreatPlanCurEdit` set a null `ContextMenuStrip` where they set a null `ContextMenu`: GridOD then makes its own strip, with the items its empty ContextMenu had. |
| Forms with code menus | `FormCareCreditWeb`, `FormErx`, `FormWebView` (Settings drop-down), `FormEmailMessageEdit` (Send/Reply/Cancel drop-down, the default item first), `FormEtrans835Edit`, `FormInsHistSetup`, `FormInsVerificationList` (submenus of users and statuses), `FormPayPlanDynamic`, `FormProcCodes2`, `FormQueryMonitor`, `FormRpEraAutoProcessed`, `FormSheetFillEdit` (the sheet combo box list), `FormTaskSearch`, `FormTreatPlanCurEdit`, `FormUserQuery`, `FormVideo` (settings) | `FormQueryMonitor`, `FormTaskSearch` and `FormUserQuery` make the grid's strip themselves when it has none or only the grid's own (the M2 lead fix's test, `ContextMenuStrip==null || !DoAddGridItemsToContextMenuStrip`); `FormQueryMonitor` finds the grid's separator by type. `FormSheetFillEdit`'s list and `PinBoard`'s menu are new strips at each click, disposed after the message that closed them, so the clicked item's Click runs first. |
| Forms Reports | `FormRpClaimNotSent`, `FormRpOutstandingIns` (code menus, the user submenu by `Owner.Items.IndexOf`), `FormRpDPPOvercharged`, `FormRpInsPendingSupplementals`, `FormRpProcNotBilledIns`, `FormRpProcOverpaid`, `FormRpProcedures`, `FormRpTreatmentFinder` (designer); `FormQuery` (section 3) | |
| ReportingForRdl | `FormReportCustom.designer.cs`, `FormReportForRdl.designer.cs` (the scroll mode menu, unused since its button line is commented out; the comment names `ContextMenuStripDropDown`) | |
| Shared program code | `Logic/ProgramL.cs` (Oryx's drop-down), `SheetFramework/SheetComboBox.cs`, `UI/Appt/PinBoard.cs`, `User Controls/ODcodeBox.cs` (Cut, Copy, Paste, Select All and the comment items with their shortcuts), `UI/ToolBarOD.cs` and `UI/ODToolBarButton.cs` (the DropDownMenu bridge removed, section 5), `Logic/Lan.cs`, `UI/BordersDpi/LayoutManagerForms.cs` (section 3) | |
| OpenPmsDentalBusiness | `UI/GridOD.cs`, `UI/PopupHelper.cs` (bridges for M3b, section 5); GridOD also unsubscribes the strip it had before when it gets another one (the M1 lead note) | |
| RdlDesign | `RdlDesigner.cs` (main menu, section 4), `DesignCtl.cs` (the designer surface's context menus: `ContextMenuStrip`s with the shared Copy, Paste, Delete and Select All items, `ToolStripItem[]` arrays), the ten grid files (section 3), new `DataGridViewSetup.cs` | |
| CentralManager | `FormCentralManager.Designer.cs`, `.cs` (section 4) | |
| Nothing to convert | `UnitTests/UI Testing/FormTestAllControls.cs` and designer (already a `ContextMenuStrip` on main; only comments name menus); `WpfControlsOD` and `OpenPmsDental/UIManagement` (WPF `ContextMenu`/`MenuItem`/`DataGrid`, not WinForms); `CodeBase/Controls/ODWebView2.cs` (WebView2's own context menu); `UserControlTasks.cs`, `ControlImages.cs`, `SheetFields.cs` (comments only) | |

An IL check of the built assemblies (every type, field, signature, local and instruction operand that is a WinForms menu or DataGrid type or
`Control.ContextMenu`/`Form.Menu`, read with Mono.Cecil from `packages/`; not a committed test) found, before: 51 types in
OpenPmsDental.exe, GridOD and PopupHelper, 12 in RdlDesigner.exe, FormCentralManager; after: only `FormMedical`, `Lan` and `GridOD`/`PopupHelper`
(the bridges), and nothing in RdlDesigner.exe, CentralManager.exe, Unit Tests.exe or the other shipped assemblies.

Line ends are as they were (`GridOD.cs` and `LayoutManagerForms.cs` are mixed and stay so; one BOM each); `tools/Test-NoEolFlips.ps1` passes.

## 2. Behaviour

The same items in the same order, with the same texts, separators, shortcuts, states and handlers; the tests compare the designer menus with
what main's WinForms menus held (section 7). What changes, by the nature of the types (S3a-M1 section 2, S3a-M2 section 5) or on purpose:

- The menus are drawn by the ToolStrip renderer, and `ContextMenuStrip.Show` does not block: `FormSheetFillEdit` clears its mouse-down flag
  while the list is open instead of after it closed; nothing else ran code after a `Show`.
- `FormVideo`'s settings: the checked resolution and photo source show a check mark, not a radio bullet (a `ToolStripMenuItem` has no
  `RadioCheck`).
- `FormTreatPlanCurEdit`: right-clicking a procedure with an appointment shows Go To Appointment and Go To Planned Appointment above the grid's
  items. Before, the code set the grids' `ContextMenu` to null, which GridOD replaced with an empty ContextMenu of its own, and a ContextMenu
  wins over a ContextMenuStrip, so the Go To items never showed (on purpose; launch check 2).
- The report designer: a Delete on the new row at the end of a grid does nothing (the DataGrid's `CurrentRowIndex` pointed past the table
  there and the delete threw); the value-expression buttons add a row in that case, as they did when the grid was empty.
- CentralManager: the menu bar is a strip inside the window instead of the native bar above it (section 4).

## 3. The DataGrids

**FormQuery** (User Query). `grid2` is a `DataGridView`: read-only, no new row, no user delete, `AutoGenerateColumns=false`; the DataGrid's
caption (the query's description after Add) is a label above the grid (`labelGridCaption`, the caption's colours and bold font; the grid starts
19 pixels lower). `BindGrid(table)` replaces `TableStyles.Clear`/`SetDataBinding`: one column per table column, in order, with the column's
caption as heading, 75 wide (the DataGrid's `PreferredColumnWidth`) and sortable, a check box column for a bool column and a text column
otherwise. The form makes the columns because the DataGridView would make an image column for a `byte[]` column (a blob in a user query) and
fail to draw it; the DataGrid showed text. `AutoSizeColumns` sets each column's width and `FillForm` its alignment
(`GetCellAlignment`: Left, Center, Right to `MiddleLeft`, `MiddleCenter`, `MiddleRight`); before, they went to an empty table style whose
column styles existed only when its empty mapping name matched the table ("I have no idea why this is failing"), so the widths and
alignments now apply every time. Printing reads a cell's text with `GetGridCellText(row,col)` (the DataGrid's `grid2[row,col].ToString()`; the
indexer of a DataGridView is `[col,row]` and gives a cell), in the grid's order, and falls back to the table's view, which the grid shows in
the same order, while the grid has made no rows. `LayoutManagerForms` skipped the DataGrid's `DataGridTextBox`, a child the grid adds itself;
it now skips the children a DataGridView adds itself (its editing panel and editing control) the same way.

**RdlDesign** (the report designer's dialogs): `DataSetsCtl` (fields), `DataSetRowsCtl` (rows typed in, columns made from the fields),
`DialogValidValues`, `DrillParametersDialog`, `GroupingCtl`, `ModulesClassesCtl` (code modules and classes), `QueryParametersCtl`,
`SortingCtl`, `SubreportCtl`. Each grid keeps its DataTable binding: the table style's `DataGridTextBoxColumn`s are `DataGridViewTextBoxColumn`s
with the same `DataPropertyName` (was `MappingName`), heading, width and read-only setting; `SortingCtl`'s `DataGridBoolColumn` (no null) is a
two-state `DataGridViewCheckBoxColumn`, its rows 14 high as the table style's `PreferredRowHeight`; a table style with `AllowSorting=false`
makes the columns not sortable. `DGCBColumn` (a column style that moved a ComboBox over the edited cell; no grid uses it, its one use is
commented out in `GroupingCtl`) is a `DataGridViewColumn` whose cell is edited with a combo box with the column's `CB` items and style (a
drop-down list, or an editable combo box with `ComboBoxStyle.DropDown`); an empty text is the null value, as the DataGrid's `NullText` was.
New `DataGridViewSetup`: the current row as `CurrentRowIndex` gave it (-1 without a current cell), `SetCurrentRowIndex`, `IsCurrentRowData`
(not the new row), `SetNotSortable`, and `CommitEdits`/`CommitOnLeave`: the DataGrid ended the current row's edit when it lost the focus, so
what was typed reached the table when the user clicked OK; a DataGridView keeps a row's edit until another row is current. Each grid ends the
edit when it loses the focus, and each `Apply`/`IsValid` and value-expression button ends it first; an untouched new row is not added.

## 4. The main menus

**RdlDesigner.** The WinForms `MainMenu` (File, Edit, View, Data, Format, Tools, Window, Help) is a `MenuStrip` (`menuMain`) with the same items,
shortcuts and handlers, the open MDI windows at the end of the Window menu (`MdiWindowListItem`, was `MdiList`), added to the form after the
toolbar and the status bar so that it docks above them, and the form's `MainMenuStrip`. The menus' Popup handlers are DropDownOpening handlers.
A WinForms MainMenu raised the Popup of the menus that hold an item before it ran the item's shortcut, so the item's Enabled state, set in
those handlers (Save without an open report, Cut without a selection), was current; a MenuStrip runs a shortcut with the state of the last
opening. `ProcessCmdKey` therefore raises the same handlers first, from the top menu down, for the item that has the key (`FindShortcutPath`).
The toolbar and status bar are WinForms `ToolBar` and `StatusBar`, also WFDEV006 shims, and not in this row (section 9).

**CentralManager.** `mainMenu` is a `MenuStrip`, added to the form's controls last (so it docks at the top) and its `MainMenuStrip`; the
constructor no longer sets `this.Menu`. A MainMenu sat outside the client area; the strip takes 24 pixels of it, so every control of the form
moves down 24 pixels and the client area grows by 24 (1834 x 953), and the layout stays as it was. User Settings is still shown at Load with
the Middle Tier.

## 5. Bridges: removed, and left for S3a-M3b

| Bridge (S3a-M1 section 3.1, S3a-M2 section 3) | Status |
|---|---|
| `ODToolBarButton.DropDownMenu` and ToolBarOD's `DropDownMenu` branch | **Removed.** Every setter is converted. `tests/P262BillingTests/WiringSourceTests.cs` (the arrow test reads `if(toolBarButton.ContextMenuStripDropDown==null){`) and window check J15 (the Send button's `ContextMenuStripDropDown`) follow. |
| `Lan.C(Control,params Menu[])`, `Lan.C(string,params Menu[])` | **Removed.** No caller passes a WinForms menu. |
| GridOD's ContextMenu path (`OnContextMenuChanged`, `ContextMenu_Popup`, `ContextMenuAddPopupLinks`, `_listMenuItemsLinks`, new `IsWinFormsContextMenuSet` for `OnMouseUp`) | **Left for M3b**, region "Bridge for S3a-M3b" of `GridOD.cs`. Its only caller: `FormMedical.InitOtherConvertMenus` (`gridAllergies`, `gridDiseases`). |
| `PopupHelper.GetContextMenuItemLinks` | **Left for M3b**, region "Bridge for S3a-M3b" of `PopupHelper.cs` (GridOD's bridge calls it). |
| Lan's translation of a grid's WinForms ContextMenu (`TranslateGridWinFormsContextMenu`, `TranslateMenuItems`) | **Left for M3b**, region "Bridge for S3a-M3b" of `Lan.cs` (with GridOD's bridge; FormMedical sets its menus in Load, after `Lan.F`, so today nothing reaches it). |

**For S3a-M3b.** `FormMedical.cs:692-705`: the two `MenuItem`s become `ToolStripMenuItem`s (`new ToolStripMenuItem(text,null,handler)`), the
two `ContextMenu`s `ContextMenuStrip`s with the item, their `Popup` lambdas `Opening` lambdas (they set `Visible` only, which a ToolStripItem
takes before it is shown; they run before the grid's Opening, as the Popups did, and the first opening is never cancelled because the item is
visible by default), and `gridX.ContextMenu=` becomes `gridX.ContextMenuStrip=` with `DoAddGridItemsToContextMenuStrip=true`. Then remove the
three bridge regions, `S3aM1Tests`' comparisons with the bridge (`GridMenuTests`, `MenuPartsTests.PopupHelper`, `LanToolStripTests`' ContextMenu
half), and `FormMedical.cs` and the bridge files from `NoHqTests/WinFormsMenuScanTests.cs` (the test fails until they are removed once they
have nothing left).

## 6. The source scan

`NoHqTests/WinFormsMenuScanTests.cs` scans the C# projects of the solution filter (shipped code, RdlDesign, CentralManager, DocumentationBuilder;
not tests, tools, third-party, UnitTests, UnitTestsCore, xCrudGenerator) and `UnitTests\UI Testing`, with comments and string contents removed.
It fails on a qualified `System.Windows.Forms.ContextMenu`, `MenuItem`, `MainMenu`, `Menu`, any DataGrid type, `Shortcut` or `MenuMerge`
(also through a using alias); in a file that imports `System.Windows.Forms` and none of the WPF namespaces with types of the same names
(`System.Windows.Controls`, `WpfControls.UI`, `WpfControls`), on the bare type names and on `.ContextMenu`, `.Menu` (not `Keys.Menu` or a WPF
type named by its namespace), `.Popup` subscriptions, `.TableStyles`, `.GridColumnStyles`, `SetDataBinding`, `.DropDownMenu`, `.MenuItems`,
`GetContextMenu` and `MergedMenu`. One listed exception: `ODWebView2`'s `e.MenuItems` (WebView2's own context menu). Known pending:
`OpenPmsDental\Forms\FormMedical.cs`, and the regions named "Bridge for S3a-M3b" in `GridOD.cs`, `PopupHelper.cs` and `Lan.cs`; the test also
fails when one of them has no WinForms menu left, so M3b removes it. Self-tests plant each rule's shapes, the WPF and mixed files, the
replacements that must not be found, and the bridge region.

## 7. Tests

New files in `tests/S3aM1Tests` (no new reference, the lock file is unchanged; RdlDesigner.exe is loaded from `RdlDesign\bin` by reflection,
Inconclusive if it has not been built):

- `M3DesignerMenuTests` (5): the 24 converted designer files' menus against the lists written from main's WinForms menus (items, texts,
  handlers, hidden and disabled states, submenus, the Opening handler), each a `ContextMenuStrip` in the components container (CentralManager's
  a `MenuStrip`), no WinForms menu member left; the grids that had a caller's menu in the designer or in code keep the grid items; the
  toolbar drop-downs use `ContextMenuStripDropDown`; CentralManager's strip and the controls moved by 24 pixels.
- `M3LiveMenuTests` (7): `FormRadOrderList` (its strip opened over the grid: See Chart, See Family, a separator, Copy Cell Text, Copy Rows),
  `FormRecurringChargesHistory` (the form's Opening before the grid's) and `FormScreenGroupEdit` (the clicked item's index) built in memory;
  `ODcodeBox`'s six items and shortcuts; `SheetComboBox`'s escaped options and the click that selects; GridOD unhooking the strip it had;
  FormQuery's `BindGrid` (headings, bound names, check box and blob text columns, width and sorting, cell text in the view's order, DBNull,
  alignment).
- `M3RdlDesignGridTests` (5): `DialogValidValues` (columns, an untouched new row not added, a typed row reaching the table when the grid loses
  the focus and the `ValidValues` it returns, Delete on a row and on the new row); `DrillParametersDialog` (the read-only name column, a typed
  value that then passes the dialog's every-parameter-has-a-value check); `DGCBColumn` (the editing combo box with the column's items and
  style, filling it does not mark the cell changed, a drop-down list does not take a text that is not an item, an editable one does, the
  values reach the table, a clone keeps items and style); `RdlDesigner.FindShortcutPath`; the main menu's source (MenuStrip, MDI list,
  DropDownOpening handlers, the shortcut lookup's handlers, shortcuts as Keys).

Changed: `S3aM1Tests/MenuPartsTests` (DropDownMenu is gone), `S3aM1Tests/LanToolStripTests` (the ContextMenu half translates through the bridge's
`TranslateMenuItems`, `Lan.C(string,params Menu[])` being removed). Source-text tests updated where the text they look for moved:
`tests/P262BillingTests/WiringSourceTests.cs` (the arrow's condition; the Statement button also has no `ContextMenuStripDropDown`) and window
check J15 (`tests/WindowChecksTests/InProc/InProcChecks.IJ.cs`). `tests/M2SDVTests/ServiceDateViewUiSourceTests.cs` and
`tests/BACCTTests/WindowTextTests.cs` read ControlAccount lines that S3a-M2 had already updated; nothing moved for them here.

Runs (lane builds first; full lane build 0 errors, no compiler warning in the changed files): S3aM1Tests 59, NoHqTests 105, P262BillingTests 54,
M2SDVTests 47, BACCTTests 41, M2HISTTests 47, MiddleTierMockTests 151, ReportTests 46; the projects whose source-text tests read a changed
file: AttributionTests 50, BAPPTTests 61, BCLAIMTests 79, BERATests 75, BPAYTests 73, BPERIOTests 55, BPRINTTests 71, BSHELLTests 73,
CLAIMDCMTests 41, DbConnectionSecurityTests 247 (+3 skipped), ImagingCaptureTests 806, ImagingTests 81, InventoryTests 93, LibWebTests 65,
LibraryUpdateTests 138, M2CANTests 49, M2CLAIMRCVTests 54, M2EFTests 46, M2ERATests 44, M2INSPLANTests 31, M2INSVTests 17, M2PAYWTests 33,
M2SHEETSTests 53, ODOPTSTests 57, P262LinksTests 61, S0FTests 29, SignatureTests 55, VBRIDGE1Tests 37, VBRIDGE2Tests 64, VDXC2Tests 63,
VEDSTests 83, VGPTests 78, VendorDirectTests 941, WindowChecksTests 3 (its 29 window checks opt-in, skipped). All passed.

## 8. Launch checks (for the gate; screenshots, not run by this package)

Run the Debug x86 build on the test database as an administrator; each step a screenshot of the open menu or grid.

1. **Forms.** Manage > Send Claims: right-click a claim (Go to Account, Delete Claim for a claim without procedures, a separator, Copy Cell
   Text, Copy Rows); the Send E-Claims arrow lists the clearinghouses, and choosing one sends to it; History: right-click (Go To Account and
   the grid items). Recall List: right-click a row of each tab (See Family, See Account, the grid items; with no row selected only the grid
   items). Insurance Verification List: right-click (Go to Patient, Go to Insurance/Patient Plan, Assign to User and Set Verify Status to with
   their submenus, the verify items, the grid items). Claim edit: right-click a procedure (Add Adjustment, enabled with a row selected; gone
   with the preference off), the attachments list (Open, Rename, Remove). Appointment edit: right-click the arrived, seated and dismissed
   times (Now). Payment: right-click the X-Charge panel and the PayConnect and PaySimple buttons (Settings; Check ACH status where shown).
   Insurance History setup: right-click a date box (Edit Date, Remove Date). Procedure codes: right-click a fee column (View Fee Schedule
   Notes). Query Monitor: right-click a row (View Stack Trace with a stack trace, the grid items). Task search, User Query (Go To Patient with
   a PatNum column), Lab Cases, Rad Order List, Recurring Charges History (the items that apply to the row), Screening group (the permission
   items), the ERA auto-processed grids, a dynamic payment plan's charges (Delete, Edit), Appointment conflicts (Select Patient, Send to
   Pinboard).
2. **Treatment plan (the change in section 2).** Treatment Plan > a treatment plan's edit window: right-click a procedure that is scheduled:
   Go To Appointment (and Go To Planned Appointment for a planned one), then Copy Cell Text and Copy Rows; another procedure: only the grid
   items.
3. **Toolbar drop-downs.** A new e-mail: the Send arrow (Send, Direct Messaging with EHR on, Cancel); a received e-mail: Reply, Cancel. E-mail
   edit window: the Autograph arrow (the autographs; choosing one inserts it). Wiki: the Home arrow (Save As Home Page). eRx, CareCredit and the
   web view windows: the Settings arrow (Clear Browser Cache). The Oryx program link's arrow (User Settings) where Oryx is enabled.
4. **Code menus.** Video capture: Settings (resolution and photo source with a check on the current one). A sheet with a combo box: the list
   (with `A&B` and `-` options, as M1's check 7). The pinboard: right-click an appointment (Change Provider). The query text box of User Query:
   right-click (Cut, Copy, Paste, Select All, Comment Selection, Remove Comment From Selection; Ctrl+Shift+C comments the selection).
5. **User Query grid.** Run `SELECT PatNum,LName,FName FROM patient LIMIT 20` (and one with a blob column): the columns and rows as before,
   widths fitted, numbers right-aligned where the report aligns them; Human-readable on and off; Add (the caption shows the description);
   Print Preview prints the same cells.
6. **RdlDesign.** Start RdlDesigner.exe: the menu bar above the toolbar (File ... Help); open a report: File's Save enabled, Window lists the
   report; with no report open, press Ctrl+S: nothing happens; open one and press Ctrl+S right away (without opening File): it saves (the
   shortcut sees the current state). Edit, View, Data (Data Sets lists the report's data sets), Format, Tools. Right-click the designer surface
   on a textbox, a table, a matrix, a chart and a subreport: their menus with the Insert submenu. Dialogs, typing into each grid and clicking
   OK without leaving the row: Data Sets (fields; Delete; Refresh Fields), the data set's Query Parameters, Data (rows typed in), Grouping
   (expressions, Up, Down, the fx button), Sorting (expression, the Sort Ascending check box), Subreport parameters, Drill-through parameters
   (the name column read-only; Refresh Parameters), Report > Code modules and classes, a parameter's Valid Values. Reopen each: the values
   are kept.
7. **CentralManager.** Start CentralManager.exe and log on: the menu bar (Logoff, File, Setup, Reports, Transfer, Help, their items) above the
   window's controls, none of them covered; resize the window; right-click a connection (Refresh); with the Middle Tier, File > User Settings.

## 9. Notes for the lead

- RdlDesign's toolbar (`ToolBar`, `ToolBarButton`) and status bar (`StatusBar`, `StatusBarPanel`) in `RdlDesigner.cs` are WFDEV006 shims too
  (the same list as `ContextMenu` and `DataGrid`); the row does not name them, so they are left. `UIManagement/Helpers/ToolBarHelper.cs`'s
  `ToolBar` is WPF's.
- `FormTreatPlanCurEdit`'s Go To items now show (section 2): a behaviour change on purpose, for the owner to confirm at the gate (launch
  check 2).
- The Delete fix on the report designer's new row (section 2) changes nothing else in those dialogs; `SortingCtl.SwapRow` and
  `GroupingCtl.SwapRow` still swap a third column their tables do not have (Up and Down there throw as on main; left as it is).
