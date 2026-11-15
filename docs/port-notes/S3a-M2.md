# Port note: S3a-M2 (program modules)

Package S3a-M2 of stage 3a of the .NET 10 move ([docs/specs/net10-plan.md](../specs/net10-plan.md), the S3a-M2 row, wave 2 after S3a-M1):
branch `net10/s3am2-1` from `main` (`a2d1d7d4a`, S3a-M1 merged). The program modules' menus move from the WinForms `ContextMenu`/`MenuItem`
(binary-compatibility shims on .NET 10 that cannot be instantiated, WFDEV006,
https://learn.microsoft.com/en-us/dotnet/desktop/winforms/wfdev-diagnostics/wfdev006) to `ContextMenuStrip`/`ToolStripMenuItem`/
`ToolStripSeparator`, the way S3a-M1 converted the shared menu code ([S3a-M1.md](S3a-M1.md), section 3.2), and the Images module's copy puts
`NodeTypeAndKey` on the clipboard as JSON under the program's own format instead of with the BinaryFormatter. No schema change, no new
package or third-party reference, nothing downloaded, no window shown.

Commits: `8a9c4b76b` (UI: the owned files and `PatientL`), `d8eb552bc` (the clipboard format), `42b54adc4` (GridOld's bridge removed, the
source-text tests that read moved text), `d604c2e0a` (tests), `95e3fa4dc` (`FormSheetFillEdit`, section 4), and this note with the plan's
status line. Line numbers are those of `main` at `a2d1d7d4a`.

## 1. The files and what changed

Every designer menu keeps its items, texts (with their ampersands), order, separators, `Visible`/`Enabled` states, click handlers and its
Popup handler (now its `Opening`); the designer converter wrote `ContextMenuStrip(this.components)`, `ToolStripMenuItem`, `ToolStripSeparator`
for a `"-"` item (its `Text` line became a `Name` line), `Items.AddRange`/`DropDownItems.AddRange` of `ToolStripItem[]`, and dropped the
`MenuItem.Index` lines (the order is the order of `AddRange`). The two designers without a components container
(`ControlImageDisplay`, `EmailPreviewControl`) got one, which their `Dispose` already disposes. A grid that had a caller's `ContextMenu`, and so
the grid's own items (Copy Cell Text, Copy Rows, the links) after the caller's, now has the strip as its `ContextMenuStrip` with
`DoAddGridItemsToContextMenuStrip=true`, in the same handler order as the grid's Popup and the caller's Popup had. A toolbar drop-down moves
from `ODToolBarButton.DropDownMenu` to `ContextMenuStripDropDown`; the code that showed a button's menu itself
(`odToolBarButton.DropDownMenu.GetContextMenu().Show`) shows the strip. A menu that its Opening fills (a strip without items starts its Opening
cancelled, S3a-M1 section 3.2) sets `e.Cancel` from what it added.

| File | Change |
|---|---|
| `Main Modules/FormOpenDental.Designer.cs`, `FormOpenDental.cs` | `menuSplitter` (Dock to Bottom/Right), `menuPatient`, `menuEmail`, `menuLetter`, `menuLabel`, `menuTask` (and the unused separator `menuItem14`) are strips; the splitter panel's `ContextMenuStrip`; the main toolbar's Select Patient, E-mail, Letter, Tasks and Label arrows use `ContextMenuStripDropDown`. `menuEmail_Popup`, `menuLetter_Popup`, `menuLabel_Popup` build `ToolStripMenuItem`s (`"-"` via `Items.Add("-")`, a separator) and un-cancel; `menuPatient_Popup` opens only when `PatientL` put items in it (as the empty native menu did not show); the click handlers cast to `ToolStripMenuItem`. The main menu (`MenuOD`, already ToolStrip) and its translation (`Lan.C("MainMenu",menuMain)`) are unchanged. The zoom hook `FormOpenDental.menuItemZoom_Click_end` passed `this.Menu`, the form's WinForms `MainMenu`, which is never set: it passes the same single null. The unused `UserQuery_ClickEvent` casts to `ToolStripMenuItem`. |
| `Logic/PatientL.cs` (not in the row: its menu helpers have no caller but `FormOpenDental`) | `RemoveAllFromMenu`, `AddFamilyToMenu`, `AddPatsToMenu`, `ButtonSelect` take the patient menu's `ContextMenuStrip`; `ButtonSelect` uses `Items.IndexOf`. Same items ("-" a separator, "FAMILY" without a handler). |
| `Main Modules/ControlAccount.Designer.cs`, `.cs`, `.SalesTax.cs`, `.ServiceDateView.cs`, `.StatementOptions.cs` | The eight menus are strips; `gridAccount` and `gridServiceDateView` share `contextMenuAcctGrid` with the grid items; Payment, Adjustment, New Claim, Payment Plan and Quick Procs use `ContextMenuStripDropDown` (Statement keeps no menu, 26.2). Quick Procs' Opening fills and un-cancels; Sales Tax is `Items.Insert(0,...)`; the clicked definition is found by `Items.IndexOf`. The Service Date View items (a separator, the toggle with its check, the window) and the handler order (its Opening before `contextMenuAcctGrid_Popup`) as before. **The Statement Options window** read the entries with `x.Visible`: a `ToolStripItem` is `Visible` only while its menu is shown, so it reads `Available` (the item's own state), or the window would have listed nothing. |
| `Main Modules/ControlChart.designer.cs`, `.cs`, `.ChartLetters.cs` | The six menus are strips; `gridProg` (a `GridOld`) shows `menuProgRight` with the grid items; the chart letter items are `ToolStripMenuItem`s and a `ToolStripSeparator`. The Rx, eRx, Ortho, Consent and Tooth Chart arrows use `ContextMenuStripDropDown`; the Consent and Tooth Chart buttons show it. The Layout arrow's menu is one strip filled again at each toolbar layout (it was a new `ContextMenu` each time; it is not disposed there because a click on one of its items lays the toolbar out again); its items keep their checks, and a click checks the clicked one among `Owner.Items`. `gridPtInfo`'s masked DOB item (Opening `MenuItemPopupUnmaskDOB`): the separator is the first `ToolStripSeparator` (the grid's, as the first `"-"` was) and the count of shown items uses `Available`. `gridOrtho`'s menu (Delete, then the grid items). The strips that `ModuleUnselected` and `FillOrtho` replace are disposed after the current message (`DisposeContextMenuStripLater`: a link on the old strip can change the module). |
| `Main Modules/ControlAppt.Designer.cs`, `.cs` | `menuReminderEdit` on `gridReminders` with the grid items; `menuRecall` (Make Family Recall) on the Make Recall arrow. |
| `Main Modules/ControlFamily.designer.cs`, `.cs` | `contextMenuInsurance`, `contextMenuDiscount` on the Add Insurance and Discount Plan arrows; `gridPat`'s strip (masked SS#/SIN and DOB items, made in `FillPatientData`, reset in `ModuleUnselected`, the old strip disposed after the current message) with the grid items; the Opening handlers find the separator by type and count with `Available`. |
| `Main Modules/ControlTreat.designer.cs`, `.cs` | `contextMenuPreAuthGrid` on `gridPreAuth` with the grid items; `menuConsent` (filled in its Opening, opens only with an item) on the Consent arrow and button. |
| `Main Modules/ControlImages.Designer.cs`, `.cs` | `menuTree`, `menuForms`, `menuMounts` are strips (`menuTree_Click` dispatches by `menuTree.Items.IndexOf`; none of the three is attached since the WPF image selector, as before). The WPF menus of the toolbar are unchanged. Only these menu lines: the Topaz lines are S3a-14's, and S3a-12 may touch the file for a P/Invoke. |
| `User Controls/ControlImageDisplay.Designer.cs`, `.cs`, new `ControlImageDisplay.Clipboard.cs` | `menuPanelMain` is the image panel's `ContextMenuStrip` (`panelMain_MouseDown` hides every item when nothing is showing, so the strip does not open, as the native menu did not). The clipboard format (section 2). |
| `User Controls/UserControlTasks.Designer.cs`, `.cs` | `menuEdit` on `gridMain` with the grid items (four separators), `menuTask` on the Add Task arrow, `menuFilter` (unused). The Set Priority and Navigate to Attachment submenus are filled in `DropDownItems`; `IgnoreTaskClick` disables every item of the strip. `Lan.C(this,menuEdit)` binds to the `ToolStripDropDown` overload (same keys). |
| `User Controls/EmailPreviewControl.Designer.cs`, `.cs` | `contextMenuAttachments` on `gridAttachments` with the grid items. |
| `User Controls/ODtextBox.cs` | The text box's menu is its `ContextMenuStrip` with the same 18 items at the same indexes (the five suggestions, separators at 5, 8 and 12, Insert Date `Ctrl+D`, Insert Quick Paste Note `Ctrl+Q`, Cut `Ctrl+X`, Copy `Ctrl+C`, Paste `Ctrl+V`, ...), the links from `PopupHelper.GetToolStripMenuItemLinks` after a separator, `menuItem_Click` by `Items.IndexOf`. A strip's shortcuts are processed for the focused control's `ContextMenuStrip` (ToolStripManager), as the `ContextMenu`'s were by the control (launch check 11). |
| `User Controls/UserControlSecurityUserGroup.Designer.cs`, `.cs` | `contextMenuUsers` (Copy User, added in Load) on `gridUsers` with the grid items. |
| `OpenPmsDentalBusiness/UI/GridOld.cs` | M1's "Bridge for S3a-M2" removed (section 3). `OnMouseUp` makes the grid's own strip when no `ContextMenuStrip` is set (it no longer checks `ContextMenu`). |
| `OpenPmsDental/Forms/FormSheetFillEdit.cs` (S3a-M3's file, two lines, section 4) | A sheet text box (`ODtextBox`) opened its menu with `textBox.ContextMenu.Show`, null now: `textBox.ContextMenuStrip.Show`, and the kiosk's empty menu is an empty `ContextMenuStrip`. |

Line ends are as they were (`FormOpenDental.Designer.cs`, `ControlAppt.*` and `ControlImages.Designer.cs` are mixed in their blobs and stay
so; every edit kept each line's own end); `tools/Test-NoEolFlips.ps1` passes.

## 2. The image clipboard format

`ControlImageDisplay`'s copy put the `NodeTypeAndKey` object on a WPF `DataObject` (`dataObject.SetData(nodeTypeAndKey)`, which the clipboard
stores with the BinaryFormatter) and the database name or Middle Tier address as a second entry, `"stringDbNameOrUri"`; the paste read
`GetData(typeof(NodeTypeAndKey))`. The BinaryFormatter is gone on .NET 9 and later
(https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-migration-guide/winforms-wpf-ole-guidance).

Now (`NodeTypeAndKeyClipboard`, `User Controls/ControlImageDisplay.Clipboard.cs`):

- **Format** `OpenPmsDental.NodeTypeAndKey.Json`. **Data**: UTF-8 JSON in a `MemoryStream`, for example
  `{"NodeType":"Document","PriKey":123,"DbNameOrUri":"opendental"}` (`NodeType` `Document` or `Mount` by name; `DbNameOrUri` is what
  `"stringDbNameOrUri"` held, now in the same object). A stream goes to and from the clipboard as its bytes, in WPF and in WinForms, without a
  serializer (a string under a custom format would go through the BinaryFormatter in WPF on .NET Framework).
- **The paste reads only that format.** Anything else gives no key (an earlier version's copy, an object of another program, bytes that are
  not the JSON of a document or mount with a key above 0, more than 4096 bytes), and the paste goes on as for a copy from another program
  (the bitmap, then the file names). Zeros after the JSON (a clipboard memory block is rounded up) are allowed. Nothing on the clipboard is
  ever deserialized.
- The copy still puts the bitmap and the file names (`DataFormats.Bitmap`, `DataFormats.FileDrop`) as before.
- **Change for the release notes:** an image copied in an earlier version pastes into this one as a picture or a file (a new document)
  rather than as a copy of the document or mount with its settings; both sides of a copy within this version work as before.

## 3. M1's bridges: removed, and left for S3a-M3

| Bridge (S3a-M1 section 3.1) | Status |
|---|---|
| GridOld's `ContextMenu` path (region "Bridge for S3a-M2": `OnContextMenuChanged`, `PopupHelper`, `removeContextMenuLinks`, `CopyHelper`, `_listMenuItemLinks`) | **Removed.** Its only caller was `ControlChart`'s `gridProg`; no M3 file sets a `GridOld`'s `ContextMenu` (grep). S3aM1Tests' three GridOld tests keep their expected lists (the ones the bridge gave when M1 compared them) without the bridge comparison. |
| GridOD's `ContextMenu` path (region "Bridge for S3a-M3") | **Left for M3.** No M2 file uses it any more; M3's callers: `FormApptConflicts`, `FormClaimEdit`, `FormClaimPayBatch`, `FormClaimsSend`, `FormEmailTemplateEdit`, `FormEtrans835Edit`, `FormInsVerificationList`, `FormLabCases`, `FormMedical`, `FormPayPlanDynamic`, `FormProcCodes2`, `FormQueryMonitor`, `FormRadOrderList`, `FormRecallList`, `FormTaskSearch`, `FormUserQuery`, `FormTreatPlanCurEdit` and the designer files. |
| `PopupHelper.GetContextMenuItemLinks` | **Left for M3.** Its M2 caller (`ODtextBox`) is converted; GridOD's bridge still calls it (and `MenuPartsTests` compares with it). Remove with GridOD's bridge. |
| `Lan.C(Control or string,params Menu[])`, `TranslateMenuItems` | **Left for M3.** The M2 calls (`ControlAccount`, `ControlChart`, `UserControlTasks`) now bind to the `ToolStripDropDown` overload; the `Lan.C` calls with a `ContextMenu` in M3 files remain. |
| `Lan.TranslateGrid`'s `grid.ContextMenu` branch | **Left for M3**, with GridOD's bridge. |
| `ODToolBarButton.DropDownMenu` and ToolBarOD's `DropDownMenu` branch | **Left for M3.** Every M2 setter and reader is converted. M3's setters: `FormCareCreditWeb`, `FormClaimsSend`, `FormEmailEdit`, `FormEmailMessageEdit`, `FormErx`, `FormWebView`, `FormWiki`, `ProgramL.cs:80`, and two that M1's list did not name: `ReportingForRdl/FormReportCustom.cs` and `ReportingForRdl/FormReportForRdl.cs`. Then `tests/P262BillingTests/WiringSourceTests.cs:85` and window check J15 (`InProcChecks.IJ.cs:554-558`, the e-mail window's Send button, an M3 file) as M1's note says. |

## 4. Overlaps for the lead

- `OpenPmsDental/Logic/PatientL.cs` is not in the row; its four menu helpers have no caller but `FormOpenDental` (M2), so M2 converted them. If
  M3 also converts the file, take one of the two (the signatures are `ContextMenuStrip`).
- `OpenPmsDental/Forms/FormSheetFillEdit.cs:1408,1438` (M3's file): without this, a right click on a sheet's text field would throw (the
  text box's `ContextMenu` is null now). The two lines are the conversion M3 would make; if M3 changed them too, keep M3's if it shows
  `textBox.ContextMenuStrip` and keeps the kiosk's empty menu.
- The source-text tests the plan gives to M3 (`tests/P262BillingTests/WiringSourceTests.cs:48-69`, `tests/M2SDVTests/ServiceDateViewUiSourceTests.cs:35`,
  `tests/BACCTTests/WindowTextTests.cs:58`) read `ControlAccount` lines that M2 changed, so M2 updated them (what they look for moved):
  `WindowTextTests.cs:58` (Sales Tax is `Items.Insert(0,new ToolStripMenuItem(...`), `ServiceDateViewUiSourceTests.cs:36-37` (`Items.Add(`),
  `WiringSourceTests.cs:54` (`Items.OfType<ToolStripMenuItem>()`, and a new check for `Available`), `:65` (the designer's `Items.AddRange` of
  `ToolStripItem[]`), `:75` (the Service Date View entry as a `ToolStripMenuItem`). M2 also updated `tests/BIMGTests/WiringTests.cs:115` (its
  row's wiring test: the designer's `Items.AddRange`, each item a `ToolStripMenuItem`, no `Index` lines) and the window check of the Letter
  drop-down (`tests/WindowChecksTests/InProc/InProcChecks.I.cs:271-276`, `menuLetter` is a strip, its Opening takes `CancelEventArgs`).
- `ControlImages.cs`: only the `menuTree_Click` line changed (S3a-12 may touch the file for a P/Invoke; its Topaz lines are S3a-14's).

## 5. Behaviour

The same items, in the same order, with the same texts, separators, states, shortcuts and handlers (section 6). What changes, by the nature of
the types or on purpose:

- The menus are drawn by the ToolStrip renderer instead of as native menus, and `ContextMenuStrip.Show` does not block (S3a-M1 section 2).
- A `ToolStripItem`'s `Visible` is false while its menu is not shown; the code that read it (the Statement Options window, the masked SSN/DOB
  Opening handlers) reads `Available`. Separators are found by type, not by the `"-"` text.
- The grid menus that `ModuleUnselected` and `FillOrtho` replace are disposed after the current message (the old `ContextMenu`s were left to
  the garbage collector with their native menus); the Chart's Layout menu is one strip filled again instead of a new menu at every toolbar
  layout.
- The menus filled in their Opening open only with an item, as the empty native menus did not show.
- Translation: the same keys; a converted menu adds no `"-"` key (S3a-M1 section 2).
- The image clipboard (section 2).

## 6. Tests

New files in `tests/S3aM1Tests` (it now also references `PresentationCore`, `WindowsBase` and `WpfControlsOD`; its lock file is unchanged, it
already held them): `M2Support.cs` (describing a strip, its items' click and Opening handlers read from their event lists, an STA runner, the
repository's files), and

- `M2DesignerMenuTests` (3): every designer menu of the owned files is a `ContextMenuStrip` in the components container with the items of
  the WinForms `ContextMenu` on `main` (text, click handlers, hidden, submenus, separators; the expected lists were written from the designer
  files on `main`) and its Opening handler; the grids with a caller's menu keep the grid items; no WinForms `ContextMenu`, `MenuItem`,
  `MainMenu`, `Menu`, `MenuItems`, `Popup`, `Shortcut.`, `DropDownMenu` or `GetContextMenu` is left in the owned files (code, not comments or
  strings).
- `M2ModuleMenuTests` (10): `ControlAccount`, `ControlAppt`, `ControlFamily`, `ControlTreat`, `UserControlTasks`, `EmailPreviewControl` and
  `ControlImageDisplay` built in memory on an STA thread (never shown): the same lists live, with the click handlers wired, the grids' strips
  and `DoAddGridItemsToContextMenuStrip`; the Statement entries `Available` (not `Visible`); `gridPat`'s strip replaced and the old one disposed
  by `ModuleUnselected`; the Tasks submenu; `ODtextBox`'s 18 items with their shortcuts and indexes, its Opening (spell check off: the
  suggestion items hidden, Cut/Copy disabled without a selection, the link added once after a separator) twice; `PatientL`'s patient menu on a
  strip (history, separator, FAMILY, members, `ButtonSelect`, empty menu cancelled). `FormOpenDental` (it shows its splash window), the Chart
  module and the Images module need a screen or a database: their designer menus are checked from the designer text, the rest by the launch
  checks.
- `M2ClipboardTests` (6): the JSON round trip; malformed and other JSON give nothing; the copy's WPF `DataObject` read back twice; the same
  data object read by a WinForms `DataObject` through its OLE interface (the stream goes through a memory block as on the clipboard); an
  earlier version's copy, an object or a long stream under the format are not read, trailing zeros are; `ControlImageDisplay` uses the format
  and no longer the object. Private data objects only: the system clipboard is never touched.

Source-text tests updated where the text they look for moved: section 4. The other source-text tests that read a changed file pass unchanged.

Targeted runs (lane builds first), all green: ApptTests 76, BAPPTTests 61, BACCTTests 41, BIMGTests 28, BTASKTests 29, S3aM1Tests 38 (19 + 19),
NoHqTests 84, MiddleTierMockTests 151, M2SDVTests 47, P262BillingTests 54; and the projects whose source-text tests read a changed file:
AttributionTests 50, BSHELLTests 73, BTPTests 38, DbConnectionSecurityTests 247 (+3 skipped), GroupNoteTests 54, ImagingCaptureTests 803,
ImagingTests 81, InventoryTests 93, LibSpellTests 23, LibWebTests 65, M2APPTTests 37, M2CARRIERTests 55, M2IMGTests 25, M2MISCTests 97,
M2SHELLTests 39, MaintenanceTests 111, P262ApptTests 16, ReleaseFlowTests 181 (+4 skipped), S0FTests 29, SshHostKeyTests 46,
TaskCategoryTests 21, VPEARL1Tests 23. WindowChecksTests builds (it launches the program, so it is the gate's).

## 7. Launch checks (for the gate; screenshots, not run by this package)

Run the Debug x86 build on the test database, logged on as an administrator, with a patient that has two family members, a referral (From)
with an e-mail address, an insurance plan, a payment plan, a few procedures and progress notes, an image and a mount. Each step: a screenshot
of the open menu.

1. **Main window.** The main menu (File ... Help) opens as before. Toolbar arrows: **Select Patient** (the last patients, a separator,
   FAMILY, the family; choosing one selects that patient; after logging on as another user, with clinics on and the preference to keep the patient off, the history is empty and
   the arrow opens nothing until a patient is chosen); **E-mail** (Referrals:, then `From <name> <address>`; choosing it opens a new e-mail to
   the referral); **Letter** (Merge, a separator, Referrals:, the referral; Merge opens Letter Merge); **Tasks** (`for <user> (n)`,
   `Reminders (n)`, Reminders hidden with repeating tasks on); **Label** (the label kinds or custom label sheets, a separator, the carriers,
   a separator, Referrals:, the referral). With the task list docked: right-click the splitter bar: Dock to Bottom / Dock to Right with the
   check on the current one; choose the other: the list moves.
2. **Account module.** Right-click a procedure row: Add Adjustment, Refund, Edit Charge (payment plan rows), the DentalXChange attachment
   items (claims), a separator, Service Date View, Service Date View Window, a separator, Copy Cell Text, Copy Rows (and a link for a row with
   `www.example.test` in its note). Toggle Service Date View and right-click its grid: the same menu, the toggle checked. Arrows: Payment
   (Allocate Unearned; Income Transfer with the preference on; Get CareCredit Self-Service Link with CareCredit enabled), Adjustment (Apply
   Sales Tax, Add Multiple), New Claim (Primary, Secondary, Medical, Other), the Payment Plan button (Payment Plan, Insurance Payment Plan),
   Quick Procs (the quick charge definitions, Sales Tax first when a sales tax code is set; "No quick charge procedures defined..." with none).
   The Statement arrow opens the Statement Options window with Statement and the eight entries.
3. **Chart module.** Right-click a progress note: the items for the selection (Delete, Set Complete, ...), the chart letter items (New Chart
   Letter... with a patient; Edit Document, Properties, Delete Chart Letter on a chart letter row), then Copy Cell Text and the note's links.
   With Patient DOB masked (Setup > Security): right-click the Birthdate row of the patient info grid: View DOB (enabled with the
   permission); another row: no View DOB (the separator above Copy Cell Text shows, as before); leave the module and come back: no duplicate items. Right-click an
   ortho chart row: Delete, Copy Cell Text, Copy Rows. Arrows and buttons: New Rx (Rx Manage), eRx (with eRx enabled: Refresh, Pending
   Prescriptions, Refill Requests, Transaction Errors), Ortho (the other ortho tabs), Consent (the consent sheets; the button opens the same
   menu), Tooth Chart (Show Big, Save to Images; the button opens it too), Layout (Add/Edit Layouts, a separator, the custom layouts with a
   check on the current one, a separator, the internal layout); choose another layout: the chart changes and the check moves.
4. **Appointments module.** Right-click a reminder in the reminders grid: Done (affects all users), Go To, a separator, Copy Cell Text,
   Copy Rows. Make Recall arrow: Make Family Recall.
5. **Family module.** With SSN masked: right-click the SS# row: View SS# (View SIN in en-CA/fr-CA); another row: no View SS#; leave and
   return: no duplicates. Add Insurance arrow: Plans for Family; Discount Plan arrow: Drop Discount Plan.
6. **Treatment Plan module.** Consent arrow and button (the consent sheets). Pre-authorization grid right-click: the DentalXChange attachment
   items (hidden without ClaimConnect), Copy Cell Text, Copy Rows.
7. **Images module.** Right-click a shown image: Copy, Copy without drawings, Paste, Flip Horizontally, Rotate Left, Rotate Right, Rotate 180,
   Delete, Info, Find Tasks; with nothing selected: no menu; from the Chart module's image window: no Delete and no Paste. Rotate Left
   rotates.
8. **Image clipboard copy and paste.** Copy a cropped and rotated image (toolbar Copy or the menu), paste it into the same patient: a new
   document with the same crop and rotation (the copy's key); copy a mount and paste: a copy of the mount with its items. Paste the same copy
   into Paint: the picture; into Explorer: the file. Copy an image in the previous release, paste here: a new document from the picture (no
   key, section 2). Connected to another database (or through the Middle Tier), paste a copy from this one: a new document from the picture.
9. **Tasks.** Right-click a task: Done, a separator, Edit Properties, Set Priority (a submenu of the priorities), a separator, Cut, Copy,
   Paste, Delete, a separator, Subscribe, Unsubscribe, a separator, Send to Me, Send to Me & Go To, Go To, Mark as Read, Archive/Unarchive on
   a task list, Navigate to Attachment (a submenu of the attachments), then Copy Cell Text and Copy Rows. Add Task arrow: Reminder.
10. **E-mail preview.** An e-mail with an attachment: right-click the attachment: Open, Rename, Remove (Rename and Remove only while
    composing), Copy Cell Text, Copy Rows.
11. **ODtextBox.** In a commlog note: right-click: Insert Date (Ctrl+D), Insert Quick Paste Note (Ctrl+Q), Insert Auto Note, Cut (Ctrl+X),
    Copy (Ctrl+C), Paste (Ctrl+V) (Cut and Copy greyed without a selection), Paste Plain Text and Edit Auto Note where they apply, and the
    links of the text (`www.example.test`) at the bottom. With the focus in the box, press Ctrl+D and Ctrl+Q without opening the menu: the
    date, the Quick Paste window. Right-click a misspelled word: the suggestions, Add to Dictionary, Disable Spell Check. A read-only box:
    Copy works, the others say the box is read only. A sheet's text field (Fill Sheet): right-click opens the same menu; in kiosk mode no menu.
12. **Security.** Setup > Security > Security Settings, the users grid: right-click a user: Copy User, Copy Cell Text, Copy Rows.
13. **Translation (optional, a non-US culture with translations).** The converted menus show their translated texts (the same keys as
    before).
