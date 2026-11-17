# Port note: S3a-M1 (shared menu code)

Package S3a-M1 of stage 3a of the .NET 10 move ([docs/specs/net10-plan.md](../specs/net10-plan.md), the S3a-M1 row, first of wave 2):
branch `net10/s3am1-1` from `main` (`8b1e89fa7`). The shared menu code moves from the WinForms `ContextMenu`/`MenuItem`/`Menu`, which
.NET 10 keeps only as binary-compatibility shims that cannot be instantiated (WFDEV006,
https://learn.microsoft.com/en-us/dotnet/desktop/winforms/wfdev-diagnostics/wfdev006), to `ContextMenuStrip`/`ToolStripMenuItem`/
`ToolStripDropDown`. The callers in the M2 and M3 files still build WinForms menus, so every public member they use keeps working through
a marked bridge (section 3), and the program builds and behaves as before until M2 and M3 convert their callers. No schema change, no new
package or third-party reference, nothing downloaded, no window shown.

Commits: `409132795` (UI: the owned files), `4830e0e99` (wiring of the new test project), `418388e23` (tests), and this note with the
plan's status line.

Line numbers are those of `main` at `8b1e89fa7`.

## 1. The files and what changed

| File | Change |
|---|---|
| `OpenPmsDentalBusiness/UI/GridOD.cs` (1934-2038, 2308-2313) | The grid's own right-click menu, made on the first right click when no caller set a menu (`OnMouseUp`), is a `ContextMenuStrip` (`_contextMenuStripGrid`, disposed with the grid) instead of a `new ContextMenu()`. `OnContextMenuStripChanged` subscribes it to `ContextMenuStrip_Opening` once; the Opening adds "Copy Cell Text" the first time (after a separator if the menu has items), then "Copy Rows", sets both enabled for the clicked cell and row exactly as the Popup did, then removes the last links and adds the clicked row's links from `PopupHelper.GetToolStripMenuItemLinks` (a `ToolStripSeparator` first if the menu has items, added only when there is at least one real link). A `MenuItem("-")` became a `ToolStripSeparator`; the "is it the divider" test (`Text!="-"`) became `!(x is ToolStripSeparator)`. Its empty menu starts its Opening cancelled (`ToolStripDropDown` cancels the opening of a menu without displayed items, which leave out the items that are not `Available`), so the handler sets `e.Cancel=false` when the menu had no item that would show and has one after it added its items (also a caller's strip with only hidden items, section 3.2). New property `DoAddGridItemsToContextMenuStrip` (default `false`, section 3.2). The `ContextMenu` path that callers still use (`OnContextMenuChanged`, `ContextMenu_Popup`, `ContextMenuAddPopupLinks`, `_listMenuItemsLinks`) is unchanged in the region "Bridge for S3a-M3". |
| `OpenPmsDentalBusiness/UI/GridOld.cs` (1466-1551, 1720-1725) | The same for the old grid (ControlChart's `gridProg`): its own menu is a `ContextMenuStrip`; the Opening adds "Copy Cell Text" (`CopyHelperStrip`) and, if `HasLinkDetect`, the links (`PopupHelperStrip`); no "Copy Rows", as before. `DoAddGridItemsToContextMenuStrip` as in GridOD. The `ContextMenu` path stays in the region "Bridge for S3a-M2". One difference that changes nothing today: the bridge decides on the links when the menu is assigned (`OnContextMenuChanged` subscribes `PopupHelper` only if `HasLinkDetect` is true then), the strip path when it opens; no caller sets `HasLinkDetect` to false. |
| `OpenPmsDentalBusiness/UI/PopupHelper.cs` (the menu code; its two `ShellOpen` lines, 164 and 177, are S3a-2's and stay as S3a-2 left them) | New `GetToolStripMenuItemLinks(text,rightClickLinks)` returns the links as `ToolStripMenuItem`s. `GetContextMenuItemLinks` (the bridge) returns the same links as `MenuItem`s. Both come from one private list of links (`GetLinks`, a small `PopupLink` class with the text and the click handler), built and sorted exactly as before (`OrderByDescending(Text=="-").ThenBy(Text)`). |
| `WpfControlsOD/UI/Controls/Supplemental/PopupHelper2.cs` | **No change.** Its `MenuItem` is `WpfControls.UI.MenuItem` (`WpfControlsOD/UI/Controls/MenuItem.cs`, a `System.Windows.Controls.MenuItem`), a WPF type that WFDEV006 does not touch, and the file has no `using System.Windows.Forms`. Its `ShellOpen` lines (162, 175) are S3a-2's. The "keep both files in sync" header stays true for the links themselves (texts, order, handlers). |
| `OpenPmsDental/Logic/Lan.cs` (33-157) | The `ContextMenuStrip[]` overloads of `C` take `ToolStripDropDown[]` (a `ContextMenuStrip`, a `ToolStripDropDownMenu` or a `ToolStripDropDown`; every call with a `ContextMenuStrip` compiles and binds as before, ahead of `C(...,params Control[])`). They translate every top-level `ToolStripMenuItem` with `TranslateToolStripMenuItems` (submenus first, then the item, as `TranslateMenuItems` does for a `MenuItem`) and skip separators and other items; before, the cast `(ToolStripMenuItem)Items[i]` threw `InvalidCastException` on a separator, in every culture. The translation keys are those of the menus they replace: the class type and the English text with its ampersands. The `Menu[]` overloads and `TranslateMenuItems`, and `TranslateGrid`'s `grid.ContextMenu` branch, are bridges (section 3). |
| `OpenPmsDental/UI/ODToolBarButton.cs` (UTF-16, kept) | Comments only: `ContextMenuStripDropDown` is the documented drop-down of a DropDownButton ("If neither is set, the arrow raises ToolBarOD.ButtonDropDownClick"); `DropDownMenu` (type `Menu`) is the bridge. |
| `OpenPmsDental/UI/ToolBarOD.cs` (330-343) | Comments only. The arrow already showed `ContextMenuStripDropDown` below the button (the half-built path, unused until now); it keeps showing a `DropDownMenu` through `GetContextMenu()` (the bridge), and raises `ButtonDropDownClick` when neither is set. The line `if(toolBarButton.DropDownMenu==null && toolBarButton.ContextMenuStripDropDown==null){` that P262BillingTests' regex reads is unchanged. |
| `OpenPmsDentalBusiness/Data Interface/SheetFields.cs` (287-289) | Comments only. `GetComboMenuItems` keeps escaping `&` as `&&` and a lone `-` as `&-`: a `ToolStripMenuItem` treats `&` as the mnemonic prefix as the `MenuItem` did, and `ToolStripItemCollection.Add("-")` of a `ContextMenuStrip` makes a separator as `MenuItem("-")` did. `SetComboFieldValue` reverses it unchanged, so stored field values do not change whichever way FormSheetFillEdit (M3) builds its menu. |
| `ODR/ComboBoxMulti.cs` | The owner-drawn `ContextMenu` becomes a `ContextMenuStrip` (`MakeMenu`): no image or check margin, one `ToolStripMenuItem` per entry with `AutoSize=false` and the size `MeasureItem` gave (box width less 18, 14 high), painted by `MenuItem_Paint` exactly as `DrawItem` painted (Highlight for a selected entry, Window otherwise, the caption in black at the item's corner, in the item's font, the menu font as before). A click toggles the entry (`Items.IndexOf` for `MenuItem.Index`) and fills the text as before; the list stays open because `cMenu_Closing` cancels a close caused by the click (`ToolStripDropDownCloseReason.ItemClicked`), where the ContextMenu closed and showed itself again at the same point; any other close (click outside, Escape, focus change) goes through. The previous list is disposed before a new one is made and in `Dispose`; the unused, never disposed `Font` of `DrawItem` is gone. |

The line ends of every file are as they were (`GridOD.cs` is mixed in its blob, mostly LF with 63 CRLF lines, and stays so);
`tools/Test-NoEolFlips.ps1` passes.

## 2. Behaviour

The same items, in the same order, with the same texts, separators, enabled states and click handlers, on the clicked row; the tests compare
each list with what the bridge puts in a WinForms `ContextMenu` for the same grid, cell, row and selection (section 4). What does change, on
purpose or by the nature of the types:

- The grid's own menu, the toolbar drop-downs that callers move to `ContextMenuStripDropDown` and ComboBoxMulti's list are drawn by the
  ToolStrip renderer instead of as native Win32 menus, and `ContextMenuStrip.Show` does not block until the menu closes (`ContextMenu.Show`
  ran a modal `TrackPopupMenu`). In the grids, `OnMouseUp` therefore clears `_isMouseDown` right away instead of after the menu closed.
  The screenshots of section 5 are the check.
- `Lan.C` on a `ContextMenuStrip` with separators translates instead of throwing. A ToolStrip menu sends no "-" to `Lans.ConvertString`,
  so a converted menu adds no "-" key to the language table (the WinForms menu added one per class type); every other key is the same.
- A `ContextMenuStrip` that a caller sets on a grid shows only its own items, as before (26 grids in the designer files do this today, for example
  `FormBilling`, `FormDatabaseMaintenance`, `FormRpCustomAging`): the grid adds nothing to it unless `DoAddGridItemsToContextMenuStrip` is
  set (section 3.2).
- A grid's `ContextMenu` still wins over its `ContextMenuStrip` (WinForms shows `ContextMenu` first), so a caller that sets
  `grid.ContextMenu=null` (the bridge then makes an empty `ContextMenu`, as before) still never shows a strip it sets afterwards
  (`FormTreatPlanCurEdit.grids_MouseDown`, as today). One edge changes: a grid that made its own menu and then gets a caller's
  `ContextMenuStrip` shows the caller's strip; before, its own `ContextMenu` stayed in front of it. No caller does this today
  (`FormTreatPlanCurEdit` sets `ContextMenu=null` first, see above).

## 3. Bridges for S3a-M2 and S3a-M3 to remove, and the new API they use

### 3.1 Bridges (remove once the last caller is converted)

| Bridge | Where | Callers today (convert them first) | Remove when |
|---|---|---|---|
| The grid's `ContextMenu` path: `OnContextMenuChanged`, `ContextMenu_Popup` (protected virtual, no override), `ContextMenuAddPopupLinks`, `_listMenuItemsLinks` | `GridOD.cs`, region "Bridge for S3a-M3" | every `grid.ContextMenu=...` (designer and code) in M2 files (for example `ControlAccount`, `ControlAppt`, `ControlFamily`, `ControlTreat`) and M3 files (for example `FormClaimsSend`, `FormInsVerificationList`, `FormRecallList`, `FormQueryMonitor`, `FormMedical`, `FormClaimPayBatch`, `FormTreatPlanCurEdit`) | M2 and M3 both merged (the second of them, or the lead) |
| The same for the old grid: `OnContextMenuChanged`, `PopupHelper`, `removeContextMenuLinks`, `CopyHelper`, `_listMenuItemLinks` | `GridOld.cs`, region "Bridge for S3a-M2" | `ControlChart.designer.cs:2503` (`gridProg.ContextMenu = menuProgRight`) | M2 |
| `PopupHelper.GetContextMenuItemLinks` (returns `List<MenuItem>`) | `PopupHelper.cs` | `ODtextBox.cs:457` (switch to `GetToolStripMenuItemLinks`) | M2 |
| `Lan.C(Control,params Menu[])`, `Lan.C(string,params Menu[])`, `TranslateMenuItems` | `Lan.cs:31-58` | `ControlAccount.cs:2274`, `ControlChart.cs:4588`, `UserControlTasks.cs:110` (M2) and the `Lan.C` calls with a `ContextMenu` in M3 files | M2 and M3 both merged |
| `Lan.TranslateGrid`'s `if(grid.ContextMenu!=null)` branch | `Lan.cs:153-155` | the grids above | with the GridOD bridge |
| `ODToolBarButton.DropDownMenu` (type `Menu`) and ToolBarOD's `DropDownMenu` branch (`GetContextMenu().Show`) | `ODToolBarButton.cs`, `ToolBarOD.cs:330-336` | every `.DropDownMenu=` (M2: `FormOpenDental.cs:1805-1835`, `ControlAccount.cs:2297-2316`, `ControlAppt.cs:2717`, `ControlChart.cs:4617-4705`, `ControlFamily.cs:573-578`, `ControlTreat.cs:178`; M3: `FormCareCreditWeb`, `FormClaimsSend`, `FormEmailEdit`, `FormEmailMessageEdit`, `FormErx`, `FormWebView`, `FormWiki`, `ProgramL.cs:80`) and the readers `odToolBarButton.DropDownMenu.GetContextMenu()` (`ControlChart.cs:7588,8191`, `ControlTreat.cs:1554`, M2) | M2 and M3 both merged. Then also update `tests/P262BillingTests/WiringSourceTests.cs:85` (its regex names `toolBarButton.DropDownMenu==null &&`) and the window check J15 (`tests/WindowChecksTests/InProc/InProcChecks.IJ.cs:554-558`, which reads the button's `DropDownMenu` and should read `ContextMenuStripDropDown`) |

### 3.2 New API for M2 and M3

- `GridOD.DoAddGridItemsToContextMenuStrip` and `GridOld.DoAddGridItemsToContextMenuStrip` (designer property, default `false`): when a
  grid's `ContextMenu` becomes a `ContextMenuStrip`, set it to `true` on that grid so it keeps "Copy Cell Text", "Copy Rows" (GridOD) and
  the links after its own items. It may be set before or after the strip (the designer writes `ContextMenuStrip` first). Leave it `false`
  on the grids that already set a `ContextMenuStrip` today. A strip whose own items are all hidden when it opens (the `Visible=false`
  "View SS#"/"View SIN" and "View DOB" items of ControlFamily's `gridPat`, ControlChart's `gridPtInfo` "View DOB", FormQueryMonitor's
  `gridFeed`) still opens with the grid's items: `ToolStripDropDown` starts such an Opening cancelled (it counts the displayed items, which
  leave out the items that are not `Available`), and the grid's Opening un-cancels it when it had no item that would show and has one
  after adding its items (fix on `net10/s3am1-2`). A caller's own Opening that makes a hidden item visible and runs before the grid's
  (subscribed before the grid's handler, that is before the strip is assigned or before `DoAddGridItemsToContextMenuStrip` is set: `HookContextMenuStrip` re-subscribes the grid's handler at both) still has to set `e.Cancel=false` itself. `FormQueryMonitor` relies on the grid's items ("ODGrid will automatically
  attach the default Popups") and finds its separator by `Text=="-"`: on a strip that is a `ToolStripSeparator`.
- `PopupHelper.GetToolStripMenuItemLinks(text,rightClickLinks)`.
- `Lan.C(Control or string,params ToolStripDropDown[])` (was `ContextMenuStrip[]`; calls compile unchanged).
- `ODToolBarButton.ContextMenuStripDropDown`: the strip the arrow shows below the button, in place of `DropDownMenu`.
- Conversion notes from this package: `.MenuItems` to `.Items` (or `.DropDownItems` for a submenu), `Popup` to `Opening` (an empty
  strip's Opening starts with `e.Cancel` true: set it false after adding items), `MenuItem("-")` to `ToolStripSeparator`,
  `MenuItem.Index` to `Owner.Items.IndexOf(item)`, `PerformClick` stays, `OwnerDraw`/`MeasureItem`/`DrawItem` to a fixed size with
  `AutoSize=false` and a `Paint` handler (ComboBoxMulti), a menu that stayed open by showing itself again to a `Closing` handler that
  cancels `ItemClicked`.

## 4. Tests

New test project `tests/S3aM1Tests` (MSTest, x86, net48; in `OpenDental.sln` and `OpenDental.Local.slnf`; references CodeBase,
DataConnectionBase, OpenPmsDentalBusiness, OpenPmsDental and ODR; its lock file is the one the lane's locked restore wrote). Nothing is shown:
the grids get a window handle for their layout only, and the menus' Opening (`OnOpening`) and Popup (`OnPopup`) are raised directly with the
grid set as their source control.

- `GridMenuTests` (10): GridOD's own strip at its first opening and at four more clicks (cells with and without text, rows selected or not,
  links appearing and going), a caller's strip without and with `DoAddGridItemsToContextMenuStrip` (set before and after the strip, opened
  twice, then turned off), a strip shared by two grids (only the clicked one adds), the own strip disposed with the grid; GridOld's own
  strip and a caller's strip; for both grids a caller's strip whose only item is hidden ("View DOB", `Visible=false`): cancelled without
  the grid items, opened with them ("View DOB" hidden, "-", "Copy Cell Text", and "Copy Rows" on GridOD). Every list is compared with the
  bridge's `ContextMenu` for the same click. The tests' opening starts cancelled exactly when `ToolStripDropDown` would: no item is
  `Available`.
- `LanToolStripTests` (3): a strip with separators in en-US (no exception, texts kept); in fr-CA on a scratch database `s3am1_*` (the
  language, languageforeign and preference tables of the reference schema, synthetic rows, dropped afterwards; inconclusive without a
  server or with `S3AM1_TEST_DB=skip`): the strip and the same `ContextMenu` give the same translated texts and the same keys (the strip
  without "-"); the `ToolStripDropDown` and `Control` overloads use the control's class as the key.
- `MenuPartsTests` (6): PopupHelper's ToolStrip links equal the MenuItem links (texts, order, PatNum/TaskNum only with right-click links);
  the toolbar button's members and a `ContextMenuStripDropDown` on a ToolBarOD; ComboBoxMulti's items, size and margins, toggling and
  text, the Closing rule, and the painting; the sheet combo texts are menu items and not separators and round-trip through
  `SetComboFieldValue`.

The source-text tests that read the owned files pass unchanged; none needed an update: `tests/P262BillingTests/WiringSourceTests.cs:82-88`
(`ToolBarOD.cs`), `tests/AttributionTests/BrandingTests.cs:448` (`GridOD.cs`), the toolbar texts of `tests/BPRINTTests/WiringTests.cs` and
`tests/BTPTests/WiringTests.cs` (other files), `tests/ProjectFileTests/RepositoryProjectsTests.cs:204-238` (`ComboBoxMulti.resx`,
unchanged). `UnitTests/UnitTests/PopupHelperTests.cs` calls only `GetURLsFromText`, unchanged.

Targeted runs (lane build first), all green: S3aM1Tests 17/17, BACCTTests 41, M2SHELLTests 39, BPRINTTests 71, MaintenanceTests 111,
InventoryTests 93, NoHqTests 84, MiddleTierMockTests 151, P262BillingTests 54, AttributionTests 50. Full lane build: 0 errors, no new
warning in the changed files. After the review fix (`net10/s3am1-2`, hidden items): full lane build 0 errors; Targeted runs all green,
S3aM1Tests 19/19 and the other nine assemblies with the same counts as above.

## 5. Launch checks (for the gate; screenshots, not run by this package)

Run the Debug x86 build on the test database, logged on as an administrator. Each step: a screenshot of the open menu.

1. **A grid without a caller menu (the grid's own strip).** Lists > Providers (`FormProviderSetup`, whose grid has no menu): right-click a
   cell with text (the right click selects its row): "Copy Cell Text" and "Copy Rows", both enabled. Right-click an empty cell: "Copy Cell
   Text" greyed. On the test database, give one provider the abbreviation `www.example.test` and right-click that row: a separator, then
   "Web - www.example.test" below "Copy Rows"; right-click another row: that link is gone. Click "Copy Cell Text" and paste: the cell's
   text.
2. **A grid with a caller's WinForms menu (the bridge, unchanged).** Manage > Send Claims: right-click a claim: the status items, a
   separator, "Copy Cell Text", "Copy Rows", as before (native menu look).
3. **A grid with a caller's strip.** Billing list or Database Maintenance: right-click: only the window's own items, no "Copy Cell Text".
4. **The old grid.** Chart module, the progress notes: right-click a note: the window's items, then "Copy Cell Text" and the note's links
   (bridge, as before).
5. **Toolbar drop-downs (the bridge).** The arrows of the main toolbar's Patient, E-mail and Letter buttons, the Account module's Payment
   and Adjustment arrows, the Chart module's eRx/Rx and Consent arrows, and the Send arrow of a new e-mail (Send, Direct Messaging,
   Cancel): each opens its menu below the button as before. A DropDownButton with no menu (the Account module's Statement arrow) opens
   the Statement Options window.
6. **ComboBoxMulti (ODR).** A report or query whose parameter window (`ODR.FormParameterInput`) has a multiple-choice enum parameter:
   open the box's list (one entry per value, as wide as the box), click two entries: both highlighted, the list stays open, the box reads
   "A OR B"; click one again: it clears; click outside or press Escape: the list closes.
7. **Sheet combo boxes (FormSheetFillEdit, still a WinForms menu until M3).** A sheet with a combo box whose options include `A&B` and `-`:
   the list shows "A&B" and "-", and choosing them stores `A&B` and `-`.
8. **Translation (optional, needs a non-US culture with translations).** No grid's `ContextMenuStrip` has a separator today, so nothing
   threw before; after M2 and M3, a converted menu with separators opens translated (the tests cover the keys and texts now).

- Lead notes at the merge (round 2's review), for M2/M3: `HookContextMenuStrip` does not unhook a strip the grid had before (keep the hooked strip in a field and unhook it); ComboBoxMulti's list no longer blocks, so `droppedDown` is true while it is open and one click on the arrow may now close it (watch launch check 6).
