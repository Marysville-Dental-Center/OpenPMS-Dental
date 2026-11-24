# Port note: UI-PLAIN (the plain windows in the new look)

Why: the owner, 2026-11-22: "Overall make sure you modernize all element types." Slice 12 of UI-12 put the plain exes' windows on the grid
(UI-12.md, "Plain windows" and "Slice 12"), but they still drew in the Windows look: a grey window, stock text boxes, check boxes, radio
buttons, scroll bars, buttons and group boxes, because only FormODBase attached the shared looks. This package gives them the look the
program's windows have, exactly as FormODBase gives it, without changing behaviour: no size, place, text, name, event, tab order or value
changes, no designer file changes. Branch `ui/uiplain-1` from `main` at 2268b57b0 (slice 12).

## 1. How the plain windows get the look

**The looks moved to CodeBase.** The plain exes reference CodeBase, not the program (CentralManager and DocumentationBuilder reference it;
MySQLManager, DatabaseIntegrityCheck, xCrudGenerator, OpenPmsDentalHL7, ServiceManager, OpenPmsDentalDbSettings and the graph library do
not; UpdateFileCopier references nothing and compiles CodeBase files in). So the shared looks moved from `OpenPmsDental/UI` to `CodeBase/UI`
(`git mv`, namespace `CodeBase`): UiInputFrame, UiLooks (with UiButtonLook, UiCheckLook, UiBlockLook, UiCaptionLook, UiSeparatorLook,
UiProgressLook, UiBorderLook), UiRadioLook, UiScrollLook, UiTrackBarLook and UiUpDownLook. They knew two things of the program: the scale of
a FormODBase window (its `GetScaleMy`, dpi times zoom) and the program's zoom (a LayoutManager without a dpi: `ComputerPrefs` Zoom/100). Those
are now two hooks, `UiLooks.FuncGetScaleWindow` and `UiLooks.FuncGetZoom`, which `LayoutManagerForms.SetLooksScale` sets from the static
constructors of LayoutManagerForms and FormODBase, so every program window paints with the same numbers as before; a plain exe has no
FormODBase and no zoom (the control's dpi alone, as the looks already did for a control outside a FormODBase). No project references more
than before; no circular reference.

**One call per window.** `CodeBase.UiPlainWindow.Attach(form)`, in each plain window's constructor right after InitializeComponent (one line):
- the window background token: the window's classic grey (SystemColors.Control), the old look's 252,253,254 (FormCentralUserEdit) or a
  light grey window (SystemColors.ControlLight: the CodeBase progress windows) becomes `UiTheme.Surface`, FormODBase's BackColor; a panel,
  split container, user control, group box, label, check box or radio button inside it in the classic grey or the old background too
  (`IsBackground`, `IsOldBackground`); a colour the window chose otherwise stays (white, transparent, the charts' colours);
- every shared look on every control, as FormODBase attaches them (`UiInputFrame.AttachTree`: the inputs' frame, the radio buttons, the
  up-down boxes, and UiLooks: the Fluent scroll bars, the slider, stock buttons, check boxes, group boxes, tab controls, progress bars,
  flat borders, tree and list views, data grid views, link labels, calendars);
- the controls the window's code adds later (`ControlAdded`, watched on every control once): the progress window's bars, the dashboard's
  tabs, panels and cells;
- nothing while the gallery draws its before pictures (`UiLooks.IsOffForGallery`).

The fonts were already the program's (each exe's Main sets Segoe UI 9, the body token) and the menus already the token renderer
(`UiTheme.InitWinForms` in each Main).

**The windows** (38 constructors): Central Manager's 18 plain windows (FormCentralManager, FormCentralChooseDatabase, FormCentralLogOn,
FormCentralConnectionEdit, FormCentralConnectionGroupEdit, FormCentralConnectionGroups, FormCentralConnections, FormCentralGroupPermEdit,
FormCentralPasswordChange, FormCentralPasswordCheck, FormCentralPatientSearch, FormCentralPatientTransfer, FormCentralProdInc,
FormCentralUserEdit, FormCentralUserGroupEdit, FormCentralUserGroups, FormCentralUserPasswordEdit, FormCentralUserSettings; FormCentralSecurity
and FormCentralReportSetup are FormODBase windows), MySQLManager's FormMySQLUserManager and FormMySQLUserEdit, DatabaseIntegrityCheck's
FormDatabaseCheck, DocumentationBuilder's Form1, xCrudGenerator's Form1, OpenPmsDentalHL7's FormDebug, ServiceManager's FormMain,
FormServiceManage and FormWebConfigSettings, UpdateFileCopier's FormMain, OpenPmsDentalDbSettings' FormDbSettings and FormSecretPrompt (their
controls are made in code: Attach after them), the graph's FormDashboardEditTab, FormDashboardEditCell, FormDashboardNamePrompt,
FormPrintImage and FormPrintSettings, and CodeBase's FormProgressExtended, FormProgressStatus and MsgBoxCopyPaste. The graph's user controls
get the look from the window that hosts them (a dashboard dialog, or a program window through LayoutManager).

**UpdateFileCopier** references no CodeBase (it must run while the program's files are replaced); it already compiled UiTheme and the icon
renderer in as linked files, and now links the looks, their painters (UiControlPaint, UiScrollPaint, UiSliderPaint, UiListPaint, UiTabPaint)
and UiPlainWindow too (`UpdateFileCopier.csproj`).

## 2. Looks added or fixed (shared code: they apply to the program's windows too)

| Element | Before | Now | Code |
|---|---|---|---|
| Stock group box (36 in the plain windows, the program's one hidden legacy group) | Windows' etched frame, the title on the line | the program's group box (UI.GroupBox): the parent's colour behind 6 px corners, the box's colour inside, a 1 px Stroke outline over the whole box, the title in Text inside at the top left (5 px in, 1 px down); disabled: TextDisabled | `UiGroupBoxLook` (UiLooks.cs), Paint event |
| Stock tab control with its tabs on top (the dashboard's DashboardTabCtrl, one made in code) | Windows' tabs | the program's tab control (UI.TabControl): the strip SurfaceSubtle, the page's Stroke border, the tabs transparent in TextSecondary, SubtleHover under the mouse, the selected one Text with the 2 px Accent line; at Windows' own tab rectangles (GetTabRect), so a click selects the tab drawn there; its stock pages on the colour behind it. Bottom, side and button tabs and owner-drawn tab controls keep Windows' drawing (none in use) | `UiTabLook` (its own file), painted after WM_PAINT and into WM_PRINT |
| Stock button with an image and a text beside, over or under it (TextImageRelation; the dashboard's Add Col and Add Row) | the look drew the image and the text where each alignment alone put them (the icon in a corner, the caption at the other side) | one block, as Windows Forms lays them out: side by side or one over the other with the icon-text gap, the text wrapped in the content's width when they stand one over the other; on each axis where the two alignments agree, there, else centred | `UiButtonLook.PaintImageAndText`, `GetBlockAlign` |
| Access keys (&OK, &Cancel, Add, Edit, Drop, Print) on stock buttons, check boxes, block and title labels, group boxes | the looks drew the underline always | hidden until Windows shows the keyboard cues (Alt), as Windows draws them (`Control.ShowKeyboardCues`) | `UiLooks.GetPrefixFlags` |
| The progress windows' light grey window (ControlLight) | kept | the token; FormProgressStatus' minimize label returns to the window's colour when the mouse leaves (was ControlLight, the window's colour then) | `UiPlainWindow.IsOldWindowBackground`, FormProgressStatus.cs |

**Converting to the program's controls**: not done. UI.Button and UI.GroupBox live in the program (OpenPmsDental.dll), which most plain
exes do not reference (and UpdateFileCopier must not), and UI.GroupBox is a ContainerControl with a tab stop (a behaviour change). The
stock controls are drawn in the look instead. CentralManager's windows already use UI.Button where their designers had it.

## 3. What keeps its look, and why

- Content: the graph library's charts (GraphQuantityOverTime, GraphIncome: the chart library's drawing in the graph's colours) and the
  legend's drawing surface; the print previews' pages and the preview control's surround (FormPrintImage, FormPrintSettings).
- Windows' own parts (UI-13.md section 4): the stock combo box's drop-down list, the date picker's calendar pop-up, message boxes and dialogs.
- Windows of slice 13 (RdlDesign and ODR, the report designer: their own slice, running in parallel) and the windows that are not plain
  dialogs: WpfControlsOD's FormFrame and FormSnap (the WPF windows' chrome), the program's FormSplash, ToolTipOD, FormComboPicker and
  ModuleSwitchBlurOverlay (a splash, pop-ups and an overlay that draw themselves), the imaging device manager's AcquireStatusWindow and
  CaptureNoticeWindow and the 32-bit helper's TopazSigningWindow (separate processes, left for the lead), PluginExample's and UnitTests'
  forms (samples and the test runner).
- (Fix round: the stock radio button's access key is drawn as Windows draws it, section 7.)
- At 150% the off-screen harness (a 96 dpi process) draws the looks at their 100% size (the check boxes, radio circles, the frames' lines and
  chevrons, the group box corners): they take their scale from the control's dpi, which Windows sets on a 150% screen (UI-13.md section 4).
- (Fix round: FormDbSettings and FormSecretPrompt, which build their controls in their constructors, are in the gallery and PlainLookTests,
  section 7.)

## 4. Tests

- `PlainLookTests` (UiRefreshTests): every plain window of the plain exes, the graph library and CodeBase calls `UiPlainWindow.Attach` in its
  constructor (its IL; UpdateFileCopier's own linked copy); every window of `grid-windows/12.tsv` built as the program builds it has the
  token on the window and on every control that shows its background, every framed input with its frame, every stock button, check box,
  radio button, group box, tab control and scrolling control with its look, and at most 10% of its picture (less its inputs) in the classic
  grey; UiPlainWindow itself (the token, the colours it keeps, controls added later, the light grey window, off for the gallery's before);
  the group box look (outline along the whole box, the title inside); the tab look (strip, Accent line, page border, pages); a button's
  image and text as one block and the access key flag.
- `WindowBuilder.AttachPlainLookAsTheConstructorDoes`: the harness runs a plain window's InitializeComponent, not its constructor; it calls
  Attach when the constructor does (as it calls InitializeLayoutManager for a program window), and on the test form that hosts a plain
  library's user control (its real hosts attach it).
- `ElementInventoryTests` and `tools/UiFit/element_inventory.py`: the plain exes' and the graph's designer files count (PLAIN_ROOTS): 180
  types, 0 without a look or a reason; the stock group box, split container and tab control are in the look, the chart and the legend keep
  theirs.
- NoHqTests' `UiThemeScanTests`: CodeBase/UI is a coverage folder (every file has its row in `coverage.tsv`), the moved looks stay in the
  colour literal scope (UiPlainWindow's 252,253,254 is its one literal), UI-PLAIN is a package.
- `GridRulesTests` unchanged and green: the plain windows' rules of slice 12 hold (the `unframed` rule still skips them; they have the frame
  now, see section 6).

## 5. Gallery and audit

G-UI3-uiplain (`.git/od-tools/test-results/G-UI3-uiplain`, `G-UI3-uiplain.zip`): 59 pictures, all changed. `1-plain-windows`: the 50
plain windows at 100% (the 42 of `grid-windows/12.tsv`, FormCentralSecurity among them, and ServiceManager's three windows, UpdateFileCopier's,
the two progress windows, MsgBoxCopyPaste and ODProgressBar); `2-at-150`: the 9 densest at 150% (slice 12's: FormCentralSecurity,
FormCentralManager, FormCentralPatientSearch, FormCentralConnections, FormCentralProdInc, FormCentralConnectionEdit, GraphQuantityOverTime,
GraphQuantityOverTimeFilter, xCrudGenerator's Form1). Before = `main` (2268b57b0) built and drawn in this session (the worktree switched to
the commit, the projects built through the lane, the pictures drawn, then back); after = this branch's build (`0a42fd505`). The same
session drew the branch with the looks off (`work/before-100`): 49 of 50 pictures byte-identical to main's (FormCentralSecurity differs: a
FormODBase window, whose UI-13 looks the switch turns off too), so nothing but the looks changed. DashboardTabCtrl shows two synthetic tabs
in both (`FitGallery.FillSampleContent`; its tabs come from the database). Every picture opened: [UI-PLAIN-audit.md](UI-PLAIN-audit.md)
(pass 1: 3 findings in 9 pictures, all real, fixed; pass 2: none).

## 6. For the lead

**Commands** (worktree root; `<W>` = `.git/od-tools/test-results/G-UI3-uiplain-src/work`, `<lane>` the lane's odbuild.ps1):
```
python tools/UiFit/element_inventory.py            (--check compares)
powershell -File <lane> -Root . -WaitMinutes 20 -Projects OpenPmsDental,UiRefreshTests,NoHqTests,xCentralManager,xMySQLManager,xDatabaseIntegrityCheck,xDocumentationBuilder,xCrudGenerator,OpenPmsDentalHL7,OpenPmsDentalGraph,ServiceManager,UpdateFileCopier,OpenPmsDentalDbSettings
powershell -File <W>/ugal.ps1 -Name after-100b -Scale 1 -Windows all       (the 50 windows; -LooksOff for the looks-off check)
powershell -File <W>/ugal.ps1 -Name after-150 -Scale 1.5 -Windows dense
rem main's pictures: git switch --detach 2268b57b0, the FitGallery sample hunk applied (<W>/fitgallery-sample.patch), the same build
rem and ugal.ps1 -Name main-100 / main-150, then git checkout the file, git switch ui/uiplain-1, git submodule update, build again
powershell -File tools/New-UiGallery.ps1 -Before <W>/../gal-before -After <W>/../gal-after -Name G-UI3 -Out .git/od-tools/test-results/G-UI3-uiplain
```

**Seen, for the lead**
- GridRulesTests' `unframed` rule still skips the plain windows (`IsPlain`; its comment says no FormODBase attaches the frame there). The
  plain windows now have the frame (PlainLookTests checks it), so the rule could hold for them too; left as slice 12 made it, as the brief
  asks.
- The imaging device manager's two status windows and the 32-bit helper's signing window are plain Forms of separate processes, not in
  this package's list; they would take the same one line (ImagingDeviceManager references CodeBase; the helper is 32-bit).
- xCrudGenerator's Form1 has a button captioned with a developer's first name (main's text, not this package's).
- UI-13.md and UI-12.md name `OpenPmsDental/UI/Ui*Look` paths in their history; the files are in `CodeBase/UI` now (coverage.tsv and the
  inventory say so).

**Results** (`uiplain-final.txt`; the whole filter `OpenDental.Local.slnf` built x64 through the lane: 0 errors): `UiRefreshTests` 296
passed, 49 skipped (opt-in), 0 failed (PlainLookTests, ElementInventoryTests, GridRulesTests, the layout snapshot); `WindowChecksTests` 30
passed (44 opt-in skipped); `UnitTests` 1728 passed (3 skipped); `PrintGuardTests` 19 passed (1 skipped); `NoHqTests` 127 passed. The
gallery runs (`<W>/tests-*.txt`): UiRefreshTests 297 passed each on the final code. `tools/Test-NoEolFlips.ps1 -Base 2268b57b0`: 0 flips.
`element_inventory.py --check`: matches (180 rows: 131 in the new look, 27 kept with a reason, 22 drawing nothing).

## 7. Fix round (branch `ui/uiplain-2`, the review of `ui/uiplain-1`)

| Finding | Fix | Code |
|---|---|---|
| The stock tab control's look painted over the tabs' ImageList images: in DashboardTabCtrl's setup mode the add icon and the delete X disappeared, the text sat over them, and a click left on a tab header deleted the tab with nothing showing | each tab's image (ImageIndex, else ImageKey; disabled through ControlPaint) where Windows draws it, 6 px in from the header's left and centred top to bottom (header X+6, Y+2 for the dashboard's 30 px tall pictures, the rectangle DashboardTabCtrl hit-tests), the text in the rest of the header; on screen and in WM_PRINT (one paint routine); read at each paint, so the ImageList the dashboard switches at run time and a new ImageIndex (Windows repaints, the look follows WM_PAINT) show | `UiTabLook.PaintTabControl`, `GetImageRectangle` |
| Stock DropDownList combo boxes kept Windows' grey read-only face inside the frame's white edge and chevron strip | an enabled one's face in the input fill (the frame's fill, the text boxes' white) with the selected item's text in Text, left and centred top to bottom, cut with an ellipsis; the focus shows as the frame's Accent line. Disabled (the disabled grey), editable (its white edit), owner-drawn, right-to-left and high-contrast combo boxes keep Windows' face | `UiInputFrame.IsFaceDrawn`, `PaintComboFace` (from PaintFrame, so on screen and printed) |
| A plain window's stock AcceptButton was drawn plain, unlike UI.Button's (FormODBase and the Central Manager's UI.Button windows) | filled Accent with the text in TextOnAccent when it is its form's AcceptButton; the owner's list of AcceptButtons kept plain moved from UI.Button to `UiButtonLook` (CodeBase), which both ask | `UiButtonLook.IsPrimary`, `PaintButton`; `UI.Button.IsPrimary` |
| GraphIncome (BackColor Transparent) showed the classic grey around its groups and chart (15.5% of the picture); PlainLookTests measured another picture than the gallery's | the harness draws a transparent control on the first opaque colour up its parents (its host's window token), as Windows Forms shows it on screen; the looks' colour behind a control walks up the same way (a group box inside GraphIncome took the classic grey inside: audit pass 3); PlainLookTests measure the gallery's picture (each control printed alone), so the old picture fails the 10% rule | `DesignSheetWindows.GetColorBehind`, `UiLooks.GetBackColor`, `PlainLookTests.CheckClassicGrey` |
| Inside the program a plain window (MsgBoxCopyPaste, the progress windows, the dashboard's dialogs) drew its looks at dpi times the program's zoom on 100% controls | a control in a window UiPlainWindow attached draws at its dpi alone, as its input frames do; a control outside any FormODBase and plain window keeps the zoom, as before | `UiLooks.GetScale(Control)` |
| FormDbSettings and FormSecretPrompt were not in the gallery | the builder makes the windows that build their controls in code with their own constructor and synthetic arguments (no file, database or network: FormDbSettings reads its file in Load, not raised), while the gallery and PlainLookTests build (`WindowBuilder.IsBuildingByConstructor`; off for the scans, which keep their records) | `WindowBuilder.CreateByItsConstructor` |
| Audit pass 3: FormDbSettings' radio buttons showed their access key's "&" | the stock radio button's access key as Windows draws it: no "&", the underline only with the keyboard cues | `UiRadioLook` (StringFormat.HotkeyPrefix from `UiLooks.GetPrefixFlags`) |

**Tests** (PlainLookTests): `TabLook_TheTabsImages` (an ImageList given after the look, ImageIndex 1 then 0: the image's pixels at its place,
the text after it, painted and printed), `InputFrame_DropDownListFaceInTheInputFill` (the face white over Windows' grey, the text; disabled,
owner-drawn and editable keep Windows' face), `ButtonLook_AcceptButtonIsPrimary` (Accent fill on the AcceptButton only),
`PlainWindow_LooksIgnoreTheProgramsZoom` (zoom 1.25: a check box in a plain window at the dpi scale and drawn as at zoom 1; one in a window
without the look keeps the zoom), `PlainWindows_BuiltByTheirConstructor_DrawTheNewLook` (FormDbSettings and FormSecretPrompt: Attach,
their controls, framed inputs, every look, the grey rule); the slice 12 test measures the gallery's picture.

**Gallery** (`G-UI3-uiplain`, 62 pictures): the 50 windows at 100% and 9 at 150% drawn again from this branch's build, plus
DashboardTabCtrl in setup mode (`FitGallery.SetDashboardSetupMode`, the state `setup`), FormDbSettings and FormSecretPrompt. Before:
`main`'s pictures; the same session's looks-off drawing of this branch reproduced 49 of the 50 byte for byte (FormCentralSecurity: a
FormODBase window), and is the before of the three new ones. Audit pass 3 in UI-PLAIN-audit.md.

**Results** (`uiplain2-final.txt`; the whole filter `OpenDental.Local.slnf` built x64 through the lane: 0 errors): `UiRefreshTests` 301
passed, 49 skipped (opt-in), 0 failed; `WindowChecksTests` 30 passed (44 opt-in skipped); `UnitTests` 1728 passed (3 skipped);
`PrintGuardTests` 19 passed (1 skipped); `NoHqTests` 127 passed. The gallery runs (`work/tests-after3-100.txt`, `after3-150`,
`before3-100`): UiRefreshTests 302 passed each. `tools/Test-NoEolFlips.ps1 -Base 2268b57b0`: 0 flips. `element_inventory.py --check`:
matches.

## 8. UI-PLAIN-2: the report designer and ODR, and the program's title bar on every plain window (branch `ui/uiplain2-1`)

Why: the owner reviewed slice 13's gallery on 2026-11-23 and marked all 21 report designer and ODR pictures "Change: old style"
(UI-12-owner-review-13.tsv); on the UI-PLAIN gallery: "window title is the old style. Not matching the rest". Branch point 857b85266.

**The report designer and ODR.** RdlDesign references no CodeBase: it links the shared looks, their painters, UiPlainWindow and the
frame as files (`ReportDesigner.csproj`, as UpdateFileCopier does; no new project reference). Each of its windows and dialogs (DialogAbout
to SingleCtlDialog, FindTab's two constructors, PropertyDialog with one line, RdlDesigner's main window) calls
`CodeBase.UiPlainWindow.Attach(this)` right after InitializeComponent, MDIChild after its own layout code; the property pages
(ChartAxisCtl, InteractivityCtl, PositionCtl, ReportCtl, ReportParameterCtl, StyleCtl, StyleBorderCtl, StyleTextCtl and the rest) get the
look from the dialog that hosts them, also when it adds them later (ControlAdded). ODR references CodeBase: FormParameterInput calls
Attach; ComboBoxMulti draws the program's combo box frame (UiListPaint: the parent's colour in the corners, StrokeStrong, the Accent focus
line) instead of its blue line, and its list in the program's colours (AccentSubtle with TextSelected, SurfaceRaised with Text). No
licence header, event, value, tab order or dialog result changes.

**The title bar and frame.** FormODBase's frame drawing (FramePaintArgs, FrameColors, FramePaintResult, FrameTitleFonts, EnumFrameLook, the
painter, the DWM corners) moved from WpfControlsOD's FramePainter to `CodeBase/UI/UiFramePainter.cs` (namespace CodeBase). FramePainter
keeps the office's Misc Colors (DefCatMiscColors is the business layer's) and forwards everything else under the names FormODBase,
FormFrame and the tests call; their pixels are unchanged (FrameTests green; PlainFrameTests compares the two painters pixel for pixel).
`CodeBase.UiPlainFrame` puts that frame on a plain window; `UiPlainWindow.Attach` turns it on:
- like FormODBase the window keeps its Windows styles (taskbar entry and icon, system menu, Alt+Space, modal dialogs, a dialog's fixed
  border) and WM_NCCALCSIZE gives the client area the whole window less the invisible resize borders left, right and below
  (`UiFramePainter.GetWidthBorderMS`: 8 px at 100%, 11 at 150%); a maximized window also less that border on top, so its band starts at the
  screen's top row. When the handle is made the window gets the size that keeps the designer's client area; Windows 11's rounded corners
  and border colour are asked for as FormODBase asks them;
- FormODBase moves its controls into a PanelClient; a plain window keeps its controls and moves its content once, at Load (after Windows
  Forms' scaling and the window's own Load code): the band (26 px at 96 dpi) is added to the window's height (on a maximized window to the
  size it restores to) and to its Padding (the docked controls: menus, tool and status strips, the MDI area), and every other control on
  the window moves as its anchors say (`GetShiftedBounds`); a centred window moves up by half the band. A control the code adds later
  moves down too, one it takes off moves back;
- the band is painted on the window's surface (its Paint event) by UiFramePainter with FormODBase's numbers: the band 26 px, the space
  right of the close button 15 px, the 8.25 pt title in the title font token, the icon, the Fluent caption glyphs, the token colours (a plain
  window has no office Misc Colors). The icon is the window's own where Windows showed one (none on a dialog frame or tool window);
- WM_NCHITTEST: the band is HTCAPTION (Windows drags, snaps, maximizes on a double-click, shows the system menu on a right-click), its top
  5 px and left 5 px and corner the resize edges on a resizable window that is not maximized (FormODBase's PanelBorders_MouseDown), the
  caption buttons HTCLOSE, HTMAXBUTTON (Windows 11's snap layouts) and HTMINBUTTON at the painter's hit areas; Windows answers for the
  invisible borders. A click on a caption button (button down and up on the same one) sends the system command Windows' own button sends
  (SC_CLOSE, SC_MINIMIZE, SC_MAXIMIZE or SC_RESTORE); the hover follows WM_NCMOUSEMOVE and WM_NCMOUSELEAVE. The band repaints when the
  code changes the window's text;
- a per-monitor dpi change (WM_DPICHANGED): the content goes back up, Windows Forms scales the window, then the content moves down by the
  new band;
- not for an MDI child (Windows' MDI frame), a window without a border (FormProgressStatus), a window inside another, every window when
  NoCustomBorders.txt is next to the exe (FormODBase's switch), nor while the gallery draws its before pictures. High contrast: the frame
  in the high-contrast tokens, as FormODBase draws it (FormODBase keeps no Windows frame there).

**Seen, for the lead**
- Code that sets a window's ClientSize after its handle exists gets a client area a caption taller than asked (Windows Forms computes the
  window from Windows' standard frame); none of the plain windows does (FormDbSettings and FormSecretPrompt set it in their constructors).
- Code that places a control on the window itself, in the window's coordinates, after Load would place it under the band; none found.
- Windows draws a plain window's title at the system dpi, as FormODBase's: on a 150% screen beside a 100% primary the title text is small
  in both.
- The harness builds RdlDesigner's main window from InitializeComponent only (its menus and toolbar are made in code before it).
- The frame was checked through its messages on hidden windows and in pictures; dragging, snapping and the system menu are Windows' own
  (HTCAPTION) and were not driven on screen (no window is shown in this package). The owner's launch check should try a plain window.

**Tests** (UiRefreshTests): `PlainLookTests` (the constructor check covers RdlDesigner and ODR;
`PlainWindows_Slice13_ReportDesignerAndOdr_DrawTheNewLook`: the 14 report designer and ODR windows of `grid-windows/13.tsv` checked like the
plain exes', with the designer's own copy of the looks, `Scan/PlainLookCopy.cs`); `PlainFrameTests` on real window handles that are never
shown: the client area over the band and the content below it (docked, top, all-sides and bottom anchored controls, a control added and
taken off later), WM_NCHITTEST of the caption, top and left edges and corner, the three buttons, the client area and Windows' borders, a
fixed dialog without maximize and minimize, the close button's click raising FormClosing, the caption text painted and repainted, the
maximized client area and the 150% sizes, FormODBase's pixels, the windows that keep Windows' frame, a report designer dialog's frame.
`WindowBuilder` attaches a library's own copy to the test host of its user controls. NoHqTests: coverage rows for the two new files.

**Gallery** G-UI3-uiplain2 (`.git/od-tools/test-results/G-UI3-uiplain2`, `.zip`): 149 pictures. `1-slice-windows`: slice 13's 21 pictures
(the same states), a window with its title bar; `2-at-150`: the 6 densest at 150%; `3-title-bars` and `4-title-bars-150`: the 61 plain
windows whole at 100% and 150%. Before: this branch with the looks off, whose 21 slice pictures are byte for byte slice 13's `main`
pictures; the title-bar before pictures are Windows' frame as Windows prints it in the 96 dpi test process. Commands:
`G-UI3-uiplain2-src/work/ugal2.ps1 -Name after-100 -Scale 1` (`-LooksOff` for the before; `-Scale 1.5`), then New-UiGallery as in section
6. Every picture opened: [UI-PLAIN-2-audit.md](UI-PLAIN-2-audit.md) (pass 1: four findings, fixed; pass 2: none).
