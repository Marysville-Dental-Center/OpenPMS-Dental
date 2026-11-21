# Port note: UI-9b (ImageLists to icons)

Package UI-9b of the UI refresh ([docs/specs/ui-refresh-plan.md](../specs/ui-refresh-plan.md), section 7's UI-9b row, with sections 3.5,
3.6 (Toolbars), 4.2, 4.3 and 4.5, and the review note "Plug-ins and `ImageIndex`"). Four slices: UI-9b-1 the main window and the module
toolbars (this note's sections 1 and 2), UI-9b-2 the imaging module and its tree, UI-9b-3 the other trees, UI-9b-4 the remaining forms'
toolbars and lists. Section 1 is the shared part the later slices build on; each slice adds its own section.

## 1. The shared part

### 1.1 The one icon factory

| Call | What it makes |
|---|---|
| `OpenPmsDental.UI.IconLibrary.GetImageList(EnumIcons[] arrayIcons,int sizePixels)` | a new 32-bit `ImageList`, `sizePixels` square, one image per icon in the order given, each under its member's name as the key (`Images.IndexOfKey("Print")`); each image is `IconLibrary.GetBitmap(icon,size,size)`: the icon's design for the size (3.5: the largest that fits, centred, never stretched up) in its role colour (UQ-4) on transparent |
| `CodeBase.UiIcons.GetImageList(int sizePixels,string[] arrayKeys,Bitmap[] arrayBitmaps)` | the list itself: the only place the program makes an `ImageList` in code (the source scan below); the glyph overload of UI-7a calls it too |

The caller owns the list and passes the real pixel size at the current DPI; a list built at one size is built again when the size changes.
A list may take more pictures after it is made, by key or by index (the program links, plug-ins).

### 1.2 Toolbars: `ToolBarOD.SetImageListIcons`

- `toolBar.SetImageListIcons(params EnumIcons[] arrayIcons)` gives a `ToolBarOD` a list of icons from the factory at its scaled 22 px slot
  (`ToolBarOD.SIZE_IMAGE_LIST96`, `LayoutManager.Scale(22)`: 22 px at 100%, 33 at 150%, 44 at 200%). The toolbar owns it and disposes it; a list a caller set through `ImageList` is never disposed by the toolbar. `IsImageListIcons` says
  which one the toolbar holds: `FormOpenDental.LayoutToolBar` gives the main toolbar its own list again when a plug-in set another, as main
  reset `imageListMain` on every layout.
- **Scale changes** (DPI, program zoom): before the next paint (`ComputeButtonsizes`) the toolbar builds the list again at the new size when
  it no longer matches, then copies the pictures added after the icons (program links, plug-ins) over, each under its key, so every
  `ImageIndex` and key stays.
- **`ImageIndex` keeps working.** A button with an index and no icon draws from the list as before. A list of icons is at the scale
  already, so its pictures are drawn pixel for pixel (sharp); the width formula uses its 96 DPI size (22 px), so a button keeps today's width.
- **`ImageList` keeps working.** A list set through the `ImageList` property (a designer stream, a plug-in's own list) is taken as 96 DPI
  pictures and drawn scaled, exactly as before UI-9b; setting a different list drops the icons, so it is never rebuilt.
- **The order is the compatibility.** A module passes its pictures' purposes in today's `ImageIndex` order (its `ArrayIconsImageList`), so a
  plug-in's button that picks a picture by number through a `LayoutToolBar_end` hook still shows that purpose, now as its icon.
- **Program links.** `ProgramL.LoadToolBar` adds the program rows' `ButtonImage` pictures (office content, 4.5) to the toolbar's list by
  key (`ProgramNum` and `ProgName`) and sets each button's `ImageIndex` from the key, unchanged; a toolbar that has no list gets an empty
  list of icons first (`SetImageListIcons()`), so the pictures are always at the toolbar's scale.
- The toolbar's own buttons take an `EnumIcons` (`new ODToolBarButton(text,EnumIcons.Print,toolTip,tag)`), drawn as vectors in the same
  22 px slot (UI-6's `DrawIconTransparent`), so their width does not change either.

### 1.3 How a later slice uses it

1. Decide each list: a toolbar list becomes `SetImageListIcons(<the purposes in today's index order>)` in the window's constructor, right
   after `InitializeComponent`; a tree or list view list becomes `IconLibrary.GetImageList(<icons>,<DPI size>)` (UI-9b-2, UI-9b-3), built
   again when the DPI changes (`IconLibrary.GetImageListForSize`, section 3.0, keeps one list per size); a list of database pictures or content stays (`keep`, 4.5).
2. Remove the designer's `ImageList` (its field, its `new`, its `ImageStream`, `TransparentColor` and `SetKeyName` lines, the
   `<toolbar>.ImageList = ...` line) and its `.resx` entries (`<list>.ImageStream`, `<list>.TrayLocation`).
3. Turn each button's index into its `EnumIcons` member. Reuse members where the purpose matches (`icons/README.md`, "The mapping"); a new
   member goes into the slice's own block of both `IconLibrary` enums with its manifest row, SVGs from the same Fluent 1.1.346 download
   (SHA-256 lines in `third-party/fluentui-system-icons/SOURCE.txt`) and `tools\New-IconData.ps1`; update the member counts in
   `tests/UiRefreshTests/IconLibraryTests.cs` and `tests/WindowChecksTests/WpfImagesTests.cs`.
4. Record the slice: its row in `icons/picture-slices.tsv`; in `icons/picture-map.tsv` the list's `UI-9b` row gives way to one row per
   picture index (`<window>:<list>.ImageStream#<index>`, the icon, the old picture); in `icons/picture-converted.tsv` one row per index
   (how `ImageList`, control = the toolbar, property `ImageList[<index>]`, 22 x 22). `PictureMapTests` checks those rows: the file's
   `ArrayIconsImageList` holds the member at the index, the control takes it, no designer loads the stream and the `.resx` lost it.
   `tools/Convert-PictureUses.ps1 -Root <worktree> -Slice <slice> -Check` must exit 0. The tool itself needed no change but its record header.

### 1.4 Tests of the shared part (`tests/UiRefreshTests/Toolbars/ToolBarImageListTests.cs`, `tests/WindowChecksTests/SourceScanTests.cs`)

- `IconFactory_MakesADpiSizedListOfIcons_WithTheirKeys`: 22, 27, 33 and 44 px; 32-bit; each image the icon's `GetBitmap`; keys; more
  pictures by key afterwards.
- `ImageIndexButton_StillDrawsItsPicture_At100_150_200`: a button set by `ImageIndex` draws the list's icon, another draws a picture added by
  key (as a program link or plug-in), inside its slot, at 100, 150 and 200%, with today's width.
- `ImageList_BuiltAgainAtAScaleChange_KeepsTheAddedPicturesByKeyAndIndex`; `ImageListSetByTheCaller_StaysA96DpiList_ScaledWhenDrawn`.
- `IconButton_DrawsItsGlyph_At100_150_200`: an `EnumIcons` button's slot is the glyph's design for the slot (20 px at 100%, 32 at 150 and
  200% where the icon has it), pixel for pixel, on the toolbar's background in the icon's role colour.
- `SourceScanTests.ImageLists_TakeTheirDepthFromAStreamOrSetIt` (WindowChecksTests): an `ImageList` made outside a designer is made only by
  the icon factory (`CodeBase/UI/Icons/UiIcons.ImageList.cs`), which sets its `ColorDepth` right after; designers load a stream or set it.

## 2. UI-9b-1: the main window and the module toolbars

On `ui/ui9b1-1` from `main` at `08d293a46`. Visible change: every picture of the main toolbar and of the Appts, Family, Account, Tx Plan
and Chart toolbars is a Fluent icon (Manage and Imaging: Manage has no toolbar of its own, Imaging is UI-9b-2). No schema change, no NuGet
package, no download (the SVGs come from UI-7a's approved download), no program started, no database, no window shown.

### 2.1 The lists

| Window | List (removed with its stream) | `ArrayIconsImageList`, today's index order | Buttons that took an index, now |
|---|---|---|---|
| `FormOpenDental` (main toolbar) | `imageListMain` (6) | PatSelect, CommLog, Email, TaskList, Label, Text | Tasks `TaskList`, Label `Label` |
| `ControlAppt` | `imageListMain` (10) | Print, ApptLists, Call, Unscheduled, BreakAptX, Complete, DeleteX, Patient, Add, Recall | Print `Print`, Lists `ApptLists`, Unsched `Unscheduled`, Rapid Call `Call` |
| `ControlFamily` | `imageListToolBar` (7) | PatSelect, Recall, PatAdd, PatDelete, PatSetGuarantor, PatMoveFam, Insurance | Add Insurance `Insurance` |
| `ControlAccount` | `imageListMain` (5) | PatSelect, Payment, Adjustment, Insurance, Print | Payment `Payment`, Adjustment `Adjustment`, New Claim `Insurance`, Statement `Print` |
| `ControlTreat` | `imageListMain` (4) | PatSelect, Sync, Print, Add | Update Fees `Sync`, Print TP `Print` |
| `ControlChart` | `imageListMain` (6) | PatSelect, Rx, PerioChart, Anesthesia, CommLog, Print | Print `Print`, New Rx and eRx `Rx`, Perio Chart `PerioChart`, Commlog (eCW tight) `CommLog` |

All six lists were 22 x 22, 32-bit, so every button keeps its width (an `EnumIcons` button and a 22 px `ImageIndex` button both take
22 + text + 8 px, section 3.3). `toolbar-overflow.tsv` and the layout snapshot are unchanged; no designer `Location`, `Size` or other layout
line was touched (the lists are components, not controls).

Not converted, by decision:
- `ControlChart.imageListProcButtons`: `keep` (4.5). The procedure buttons' pictures come from the database; the designer stream only gives
  the list its 20 px size and 32-bit depth, and its one picture is cleared before the list is filled (`FillProcButtons`).
- `ControlChart.imageListThumbnails`: imaging content (4.5), no stream.
- `ControlAppt.imageListTasks`: the reminders grid's task pictures (a grid column's list, not a toolbar's); it stays `UI-9b` for UI-9b-3,
  with the task lists' same pictures, so the task pictures change together.

One small look difference, inside the buttons: a button with an icon centres its caption in the space beside the icon, as every icon button of
these toolbars already did (Pat Appts, Make Appt, the family buttons, Select Patient); an `ImageIndex` button left-aligned it. So the captions of
the converted buttons sit about 2 px further right (5 px of slack, centred). Nothing moves.

`ProgramL.cs`: unchanged lookup by key; a toolbar without a list gets an empty list of icons first (1.2).

### 2.2 New `EnumIcons` (block "UI-9b-1" in both enums; 100 WinForms, 111 WPF)

| Member | Purpose | Fluent (regular) | Sizes |
|---|---|---|---|
| `Payment` | Account: Payment (banknote picture) | Money | 16, 20, 24 |
| `Adjustment` | Account: Adjustment (plus and minus picture) | Add Subtract Circle | 16, 20, 24, 48 |
| `ApptLists` | Appts: Lists, the appointment lists (page of lines) | Document Bullet List | 16, 20, 24 |
| `Call` | Appts: Rapid Call (telephone) | Call | 16, 20, 24, 32, 48 |
| `Unscheduled` | Appts: Unsched, send to the Unscheduled List | Calendar Arrow Right | 16, 20, 24 |
| `Rx` | Chart: New Rx, eRx (the letters Rx) | Pill (the plan's example, 4.3) | 16, 20, 24 |
| `Anesthesia` | Chart's list: the anesthesia picture (heart monitor) no button shows; kept at its index for plug-ins | Pulse | 20, 24, 32 |
| `PerioChart` | Chart: Perio Chart (probe in a pocket) | UI-8's custom `PerioChart` dental icon (`IconLibrary.GetDentalIcon`, like `Chart32G`); no manifest row | 16 to 48 |

7 manifest rows after the last `EnumIcons` row, 24 SVGs (`SOURCE.txt` lines added), `IconData.g.cs` regenerated (709 designs). Heart Pulse
was not used for anesthesia: it is already the medical Tx Plan module icon (`TreatPlanMed32`). Reused: `PatSelect`, `CommLog`, `Email`,
`TaskList`, `Label`, `Text`, `Print` (also Statement, whose picture is a printer), `Insurance` (umbrella), `Sync` (Update Fees' circle
arrow), `BreakAptX`, `Complete`, `DeleteX`, `Patient`, `Add`, `Recall`, `PatAdd`, `PatDelete`, `PatSetGuarantor`, `PatMoveFam`.

### 2.3 Tests (Targeted: UiRefreshTests, WindowChecksTests, UnitTests, NoHqTests)

- New `ToolBarImageListTests` (section 1.4) and, for the slice: `ProgramLinkPicture_FromASyntheticProgramRow_ShowsOnTheModuleToolbar` (a
  synthetic program row, its toolbar button row and `EasyNoClinics` in the caches only, no database: `ProgramL.LoadToolBar` puts the picture
  after the Account module's icons by key, sets the button's `ImageIndex`, the picture shows at 100, 150 and 200%, and a second layout
  replaces it by key); `ModuleImageLists_KeepTodaysIndexOrder_AsIcons`; `SliceToolbars_HaveNoImageIndexButtonLeft_TheirButtonsTakeAnIcon`;
  the opt-in `Gallery_ModuleToolbars`.
- Changed: `ToolBarModel` (a module's `ArrayIconsImageList` gives the toolbar its list), `ToolBarFitTests` (the Account module's first button
  is `Payment`, was index 1; the overflow list unchanged), `PictureMapTests` (the `ImageList` record rows), `IconLibraryTests` (100, 111, 111
  and `PerioChart`'s dental icon), `WpfImagesTests` (111), `SourceScanTests` (the icon factory).
- `RenameGuardTests` (NoHqTests): unchanged and green (no pack URI or `BitmapFileName` in this slice). Layout snapshot and bounds record:
  `LayoutSnapshotTests` and the scan pass unchanged.
- Runs: `.git/od-tools/test-results/ui9b1-before.txt` (the before pictures, at `9b0d5cb1c`: the shared part and the tests, the modules not yet
  converted; the two tests that need the conversion fail there by design), `ui9b1-1.txt`, `ui9b1-final.txt` (see the end of this section).

### 2.4 Gallery G-UI3, the UI-9b-1 slice

**Where:** `.git/od-tools/test-results/G-UI3-ui9b1/` (`index.html`, `G-UI3-ui9b1.zip` beside it), the plain-words summary at the top.
Sources `.git/od-tools/test-results/ui9b1-pictures/before` and `.../after`, `toolbars/<toolbar>-<scale>.png`: the main toolbar and the five
module toolbars, each with every button its code can add, a synthetic program link button (a green square picture) and a plug-in button
set by `ImageIndex` 0, at 100, 150 and 200%. 18 pictures, 18 changed. Everything rendered off screen (WinForms `DrawToBitmap`).

**For the lead** (`<repo>` is the main checkout):

```
rem Rebuild the before set: the commit 9b0d5cb1c (shared part and tests, modules not converted), built through the lane
set OPMS_UI9B_PICTURES=<repo>\.git\od-tools\test-results\ui9b1-pictures\before
powershell -NoProfile -ExecutionPolicy Bypass -File <repo>/.git/od-tools/run-tests.ps1 -Root <worktree> -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests -Out <repo>/.git/od-tools/test-results/ui9b1-before.txt
rem The after set: the branch head, built
set OPMS_UI9B_PICTURES=<repo>\.git\od-tools\test-results\ui9b1-pictures\after
powershell -NoProfile -ExecutionPolicy Bypass -File <repo>/.git/od-tools/run-tests.ps1 -Root <worktree> -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests -Out <repo>/.git/od-tools/test-results/ui9b1-after.txt
powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before <repo>\.git\od-tools\test-results\ui9b1-pictures\before -After <repo>\.git\od-tools\test-results\ui9b1-pictures\after -Name G-UI3 -Out <repo>\.git\od-tools\test-results\G-UI3-ui9b1 -Note "<the summary>"
rem In the connected, unlocked session (a shown window): the real toolbars in the running program with the synthetic dump's program
rem links, at the three widths (UT1), and G-UI2's main window and module pictures (group U's part UM), as UI-6's run
set OPMS_WINDOW_CHECKS=1
set OPMS_WINDOW_CHECKS_DUMP=<the synthetic dump, as for group U>
set OPMS_WINDOW_CHECKS_GROUPS=UT,UM
powershell -NoProfile -ExecutionPolicy Bypass -File <repo>/.git/od-tools/run-tests.ps1 -Root <worktree> -Mode Targeted -Platform x64 -RequireBuildOutput -Include WindowChecksTests -Out <repo>/.git/od-tools/test-results/ui9b1-wc.txt
```

The UT1 and UM pictures of UI-6's run (main's code before this package) are their before set. The off-screen set already has every changed
button; the shown-window run adds the office's own program link pictures and the real DPI. On a real desktop the owner may glance at one
module at 125% and 150% (the icons are drawn at the slot's size; program link pictures from the database are scaled as before).

### 2.5 Owner questions (new icon choices, to approve in the gallery)

1. Account, Payment: Money (a banknote, as today's picture). Fine, or Payment (a card)?
2. Account, Adjustment: Add Subtract Circle (plus and minus in a circle, as today's +/-). Fine?
3. Appts, Lists: Document Bullet List (a page with a list). Fine?
4. Appts, Rapid Call: Call (a telephone handset). Fine?
5. Appts, Unsched (send to the Unscheduled List): Calendar Arrow Right (a calendar with an arrow out). Fine, or Clipboard Arrow Right?
6. Chart, New Rx and eRx: Pill (the plan's example; Fluent has no Rx letters). Fine?
7. Chart, Perio Chart: UI-8's custom perio chart icon (gum line and pocket bars) rather than Fluent's ruler (`Probe`). Fine?
8. Chart, the unused anesthesia picture (kept for plug-ins): Pulse (a monitor line). Fine?
9. Reused: Statement prints, so it takes Print (today's picture is a printer); Update Fees takes Sync (today's circle arrow); New Claim and Add
   Insurance take the umbrella (`Insurance`, approved with UI-9a-6). Fine?
10. The captions of the converted buttons sit about 2 px further right (centred beside the icon like the other icon buttons). Fine?
11. (Lead, after the review) At 150% and 200% the icons on one toolbar can differ in size: the 3.5 rule draws the largest Fluent design that
    fits the slot, and Fluent does not make every icon at every size (Print and Select Patient have a 32 px design, Money, Add Subtract
    Circle and the umbrella do not, so at 150% they draw 24 px beside 32 px). Keep the rule, or draw every icon on a toolbar at one size?

**Results.** `.git/od-tools/test-results/ui9b1-final.txt`: all passed (UiRefreshTests 229 + 10 opt-in skipped, WindowChecksTests 29 + 44
opt-in skipped, UnitTests 1,728 + 3, NoHqTests 122). Full x64 filter build: 0 errors. `tools\Test-NoEolFlips.ps1 -Base main`: 34 changed
text files, no line ends changed. `Convert-PictureUses.ps1 -Slice UI-9b-1 -Check` and `-Slice UI-9a-6c -Check`: exit 0. InitSqlTests and
ForkMigrationTests not run (no schema work).

## 3. UI-9b-2: the imaging module and its tree

On `ui/ui9b2-1` from `main` at `30b8637ac`. Visible change: the two image toolbars' pictures (Crop, Pan, Adj, FlipH, -90, +90) are Fluent
icons, and the document tree's list is a list of icons at the DPI size. No schema change, no NuGet package, no download (the SVGs come from
UI-7a's approved download), no program started, no database, no window shown.

### 3.0 The tree helper (the one addition to section 1.3)

`IconLibrary.GetImageListForSize(ImageList imageList,EnumIcons[] arrayIcons,int sizePixels)` is the one small addition to the shared part
for trees and list views: it returns `imageList` when it is already `sizePixels` square and holds the icons, else it disposes the old list
and returns a new `GetImageList(arrayIcons,sizePixels)`. A window keeps the list in a field and calls it where it is shown or its scale may
have changed (`ControlImages.ImageListTree` calls it with `LayoutManager.Scale(16)`), and disposes the last list with the control. The
index order and the keys of `arrayIcons` are today's, so `ImageIndex` and `ImageKey` keep their pictures. Behaviour of the toolbar part is unchanged.

### 3.1 The lists

Only `ControlImages` holds ImageLists in the imaging module (the windows `FormImageFloat`, the mount and document windows, `FormImagingScan*`
and the TWAIN windows hold none). Included: `ControlImages:imageListTools2` (21 pictures, used by `toolBarPaint` and `toolBarDraw`) and
`ControlImages:imageListTree` (8 pictures). Left out, and why: the WPF `ImageSelector` tree draws its own `EnumIcons` (UI-9a-6), thumbnails
and document pictures come from the database or the image store (office content, 4.5, kept).

- Tools list: `ControlImages.ArrayIconsImageList` in today's `ImageIndex` order, given to both toolbars with `SetImageListIcons` right after
  `InitializeComponent`. The six buttons that took an index now take their `EnumIcons` (Crop 7, Pan 10 = Hand, Adj 20 = ArrowsAll, FlipH 11,
  -90 12, +90 13); a plug-in button set by `ImageIndex` still draws the same purpose.
- Tree list: `ControlImages.ArrayIconsImageListTree` (UsbStick, folder, document, radiograph, photo, file, mount, web folder) and
  `ControlImages.ImageListTree` (built at the DPI size, rebuilt when the scale changes, disposed with the control). No code gave the old
  list to a control (the WPF tree draws its own icons), so it stays at its indexes for plug-ins and a later tree.
- The designer's two `ImageList` fields, their streams and the `.resx` entries are removed (`ControlImages.Designer.cs` keeps its mixed line ends).

### 3.2 New `EnumIcons` (block "UI-9b-2" in both enums; 113 WinForms, 113 WPF)

| Member | Purpose | Fluent (regular) | Sizes |
|---|---|---|---|
| `Scan` | tools list index 4: the flatbed scanner picture, kept for plug-ins | Scan (a viewfinder frame) | 16, 20, 24 |
| `UsbStick` | tree list index 0: the memory stick, no node shows it | Usb Stick | 20, 24 |

The WinForms enum also gets the 11 imaging members the WPF enum already had (ArrowsAll, Crop, Flip, Hand, Import, Info, RotateL, RotateR,
ScanMulti, ScanPhoto, ScanXray). The download has no 28, 32 or 48 px design of Scan or UsbStick, so at 150% and 200% they draw the 24 px design
(the 3.5 rule). The reused members: PatSelect, Print, DeleteX, Paste, ZoomIn, ZoomOut, Copy, Export, ImageSelectorFolder, ImageSelectorDoc,
ImageSelectorXray, ImageSelectorPhoto, ImageSelectorFile, ImageSelectorMount and ImageSelectorFolderWeb keep their approved choices.

### 3.3 Tests (Targeted: UiRefreshTests, WindowChecksTests, UnitTests, NoHqTests)

- New `ImagingImageListTests`: `ImagingLists_KeepTodaysIndexOrder_AsIcons`, `ImagingToolbars_HaveNoImageIndexButtonLeft_TheirButtonsTakeAnIcon`,
  `ImagingToolbars_DrawTheirIcons_AndAnImageIndexButton_At100_150_200` (a plug-in button set by ImageIndex 7 draws Crop),
  `TreeList_IsAtTheDpiSize_AndBuiltAgainWhenTheDpiChanges` (16, 24, 32 px, the old list disposed, the same size gives the same list),
  `TreeNode_SetByImageIndexOrImageKey_StillDrawsItsPicture_At100_150_200` and the opt-in `Gallery_ImagingToolbarsAndTree`.
- Changed: `PictureMapTests`, `IconLibraryTests` (113, 113, 113), `WpfImagesTests` (113). `SourceScanTests` green (only the factory makes
  ImageLists). `RenameGuardTests`, layout snapshot and bounds: unchanged and green.
- Runs: `.git/od-tools/test-results/ui9b2-targeted.txt` (first run) and `ui9b2-targeted2.txt` (UiRefreshTests after two test fixes:
  all passed). Full x64 filter build: 0 errors. `Convert-PictureUses.ps1 -Root . -Slice UI-9b-2 -Check`: exit 0 (29 picture uses).

### 3.4 Gallery G-UI3, the UI-9b-2 slice

`.git/od-tools/test-results/G-UI3-ui9b2/index.html` (plain-words summary at the top; any download link in the page is not needed). 9 pictures
rendered off screen: both image toolbars and the tree (synthetic nodes, no patient data) at 100, 150 and 200%. Sources: `ui9b2-gallery-src/before`
(the old streams' pictures, extracted to `ui9b2-pictures/old-lists`) and `.../after`.

### 3.5 Owner questions (new icon choices)

1. The scanner picture (tools list index 4, no button shows it today): Fluent has no flatbed scanner; the new `Scan` is the viewfinder frame. Fine?
2. The memory stick (tree list index 0, no node shows it): Usb Stick. Fine?
3. The scan document, photo, radiograph and multi-page pictures reuse the icons already chosen for the tree (document, photo, radiograph) and
   Document Multiple. Fine?
4. Keep the unused `ControlImages.ImageListTree` (nothing gives it to a control today), or delete it?

### 3.6 For the lead

Nothing needs a shown window: the pictures render off screen. To rebuild the gallery: set `OPMS_UI9B2_OLD_LISTS` to
`.git/od-tools/test-results/ui9b2-pictures/old-lists` and `OPMS_UI9B_PICTURES` to a folder named `before` (then `after`, no old lists needed), run
the Targeted UiRefreshTests, delete the other modules' `toolbars` pictures the UI-9b-1 test also writes, then
`tools\New-UiGallery.ps1 -Before <before> -After <after> -Out <folder> -Name G-UI3`. A check in a shown window (the toolbar's Crop, Pan and Adj
buttons and the tree on a connected desktop) waits for the lead's session.

## 4. UI-9b-3: the other trees and lists

On `ui/ui9b3-1` from `main` at `6c9e1853c`. Visible change: the pictures of the task lists and the reminders grid, the security tree, the two auto
notes trees, the email inbox's six buttons, the setup wizard's grid, labels and three buttons, and the tasks toolbar are Fluent icons; the trees'
lists are at the DPI size. No schema change, no NuGet package, no download (the SVGs come from UI-7a's approved download), no program started, no
database, no window shown.

### 4.1 The lists

| Window | List (removed with its stream) | Today's index order, as the file's array | How it is shown now |
|---|---|---|---|
| `UserControlTasks` | `imageListTree` (5) | `ArrayIconsImageListTasks`: TaskList, CheckBoxChecked, CheckBoxEmpty, TaskListNew, TaskNew (a task list, done, read, a list with new tasks, a new task) | the tree: `GetImageListForSize` at the 16 px slot, again at each resize; the grid's first column: a 16 px list (below) |
| `UserControlTasks` | `imageListMain` (2) | `ArrayIconsImageListMain`: TaskListAdd, Add | `ToolBarMain.SetImageListIcons`; the two buttons take their `EnumIcons` |
| `ControlAppt` | `imageListTasks` (5) | `ArrayIconsImageListTasks`, the same five | the reminders grid's first column: a 16 px list |
| `UserControlSecurityTree` | `imageListPerm` (4, 15 px) | `ArrayIconsImageListPerm`: ImageSelectorFolder (a heading), CheckBoxEmpty (not granted), CheckBoxChecked (granted), CheckBoxPartial (some items) | the tree: `GetImageListForSize` at the 16 px slot, again at each size change |
| `FormAutoNotes`, `FormAutoNoteExport` | `imageListTree` (2 each) | `ArrayIconsImageListTree`: ImageSelectorFolder, ImageSelectorDoc | the tree (every node draws the tree's default index 1, the document, as today) |
| `FormEmailInbox` | `imageListButtonSmall` (6, 16 px) | `ArrayIconsImageListButtons`: Edit, Sync, Reply, Search, ReplyAll, Forward | each of the six `UI.Button`s: `Image = IconLibrary.GetBitmap(icon,16,16)` in the designer (the button draws it at the window's scale, as the UI-9a 16 px buttons) |
| `FormEmailInbox` | `imageListMailboxesLarge` (2, 32 px) | `ArrayIconsImageListMailboxes`: Inbox, Send | nothing showed this list; kept as an array at its indexes (never cut) |
| `FormSetupWizard` | `imageList1` (1) | `ArrayIconsImageList`: Info | the grid's "?" column: a 16 px list |
| `FormSetupWizardProgress` | `imageList1` (3) | Skip `ChevronDoubleRight` (0), Next `ChevronRight` (1), Back `ChevronLeft` (2) | each `UI.Button`: `Image = GetBitmap(icon,16,16)` |
| `UserControlSetupWizFeatures` | `imageList1` (2) | `ArrayIconsImageList`: Info, InfoSelected | the seven information labels: a 16 px list, `label.ImageList`; the click code keeps switching `ImageIndex` 0 and 1 |
| `UserControlSetupWizFeeSched`, `UserControlSetupWizPrinter` | `imageList1` (1 each) | `ArrayIconsImageList`: Info | their three information labels each, as Features |
| `UserControlSetupWizDefinitions`, `...Practice`, `...Schedule` | `imageList1` (1, 2, 1) | map rows only (Info, InfoSelected) | no control showed these lists; removed with their streams |

How each kind of list is made (the shared part, section 1.3, is unchanged):
- Trees: `IconLibrary.GetImageListForSize(list,array,LayoutManager.Scale(16))`, set in the constructor and again where the scale may have changed
  (`UserControlTasks_Resize`, `UserControlSecurityTree_SizeChanged`, a `treeNotes.SizeChanged` handler in the two auto note windows); the old list
  is disposed by the helper, the control disposes the last one.
- A grid column's list stays a 96 DPI, 16 px list from `IconLibrary.GetImageList(array,16)`: `GridOD` draws a column's pictures at `ScaleI(width)`
  itself (and sizes the row from `ScaleI(ImageList.ImageSize.Height)`), so a list already at the DPI size would be scaled twice, and the print paths
  draw a list's pictures at their own size. Like the old bitmaps, the icons are scaled by the grid at 150% and 200% (the trees are crisper, see the
  owner questions).
- Labels and buttons that took a picture from a designer list (the wizard's labels, the email and wizard buttons) keep their 16 px picture size:
  buttons by `Image = IconLibrary.GetBitmap(...,16,16)` as UI-9a does for 16 px buttons, labels through a 16 px list set after
  `InitializeComponent` (the designer keeps `ImageIndex = 0`).
- The security tree's `ItemHeight` is set to `LayoutManager.Scale(16)` where it set `Scale(15)` (the old pictures were 15 px): rows are 1 px taller so
  the 16 px icons show whole. The designer's `ItemHeight = 15` is untouched; no control moved.
- All lists are disposed with their control (`Disposed` handlers). The streams, the fields and the `.resx` entries are removed.

Not converted, and why: `FormEmailEdit:imageListMain` (the compose window's toolbar and its list) and every other window's list stay `UI-9b` for
UI-9b-4 (the "remaining forms' toolbars and lists"); the email inbox's grids use no pictures. The task windows (`FormTaskEdit`, `FormTaskListEdit`,
`FormTaskNoteEdit` and the others) hold no ImageList. `ControlAppt`'s other lists were UI-9b-1's. Office content stays (4.5): none of these lists
holds any.

### 4.2 New `EnumIcons` (block "UI-9b-3" in both enums; 128 WinForms, 128 WPF)

| Member | Purpose | Fluent | Style | Sizes | Role |
|---|---|---|---|---|---|
| `CheckBoxEmpty` | a read task; a permission not granted | Checkbox Unchecked | regular | 12 to 24 | Text |
| `CheckBoxChecked` | a done task; a permission granted | Checkbox Checked | regular | 16 to 24 | Success (the old green check) |
| `CheckBoxPartial` | a permission granted for some items (the old green square) | Checkbox Indeterminate | regular | 16 to 24 | Success |
| `TaskNew` | a new, unread task (the old orange square) | Square | filled | 12 to 48 | Warning (orange) |
| `TaskListNew` | a task list with new tasks (the old orange list) | Clipboard Task List LTR | filled | 20, 24 | Warning |
| `TaskListAdd` | tasks toolbar: Add Task List | Clipboard Task Add | regular | 20, 24 | Text |
| `ChevronLeft`, `ChevronRight` | setup wizard: Back, Next | Chevron Left, Chevron Right | regular | 12 to 48 | Text |
| `ChevronDoubleRight` | setup wizard: Skip | Chevron Double Right | regular | 16, 20 | Text |
| `Reply`, `ReplyAll`, `Forward` | email inbox buttons | Arrow Reply, Arrow Reply All, Arrow Forward | regular | 16 to 48 | Text |
| `Search` | email inbox: Search | Search | regular | 12 to 48 | Text |
| `Inbox` | email inbox: the Inbox mailbox (unused list) | Mail Inbox | regular | 16 to 48 | Text |
| `InfoSelected` | setup wizard: the selected information label | Info | filled | 12 to 48 | Accent |

15 manifest rows, the missing SVGs added to `third-party/fluentui-system-icons` (the same 1.1.346 download, SHA-256 lines in `SOURCE.txt`; no 28 px
design is used, as everywhere: the 24 design serves 24 to 31 px), `IconData.g.cs` regenerated (780 designs). The three new "on" states are Filled so
they differ from their quiet twin by shape, not only by colour (`TaskNew`, `TaskListNew`, `InfoSelected`); `IconLibraryTests` allows exactly these three
rows to be Filled (section 4.3 of the plan says Regular for everything and Filled for "on" states). Reused: TaskList, Add, ImageSelectorFolder,
ImageSelectorDoc, Info, Edit, Sync, Send. `TaskList` and `TaskListNew` have no 16 px design in the download, so at the 16 px slot the 20 px design
is drawn scaled down (the 3.5 rule when no smaller design exists).

### 4.3 Tests (Targeted: UiRefreshTests, WindowChecksTests, UnitTests, NoHqTests)

- New `Toolbars/OtherTreesImageListTests`: `Lists_KeepTodaysIndexOrder_AsIcons`, `Designers_HaveNoStreamLeft_ButtonsAndToolbarTakeTheirIcons`,
  `StatePictures_StayDistinguishable_ByShapeAndKeepTheirMeaningColour` (every state picture of the task lists, the security tree and the information
  labels has a shape of its own at 16, 24 and 32 px; the role colours), `TreeLists_AreAtTheDpiSize_AndBuiltAgainWhenTheDpiChanges` (16, 24, 32, 24 px,
  keys, the old list disposed), `SecurityTreeControl_BuildsItsListAtTheScale_AndAgainWhenTheScaleChanges` (the real control, scales 1.5, 2, 1),
  `TreeNode_SetByImageIndexOrImageKey_StillDrawsItsPicture_At100_150_200`, `GridColumnLists_DrawTheirPictureByTheIndexInTheCell_At100_150_200` (a real
  `GridOD`), `TasksToolbar_DrawsItsIcons_AndAnImageIndexButton_At100_150_200` (a plug-in button set by `ImageIndex` 0 still draws),
  `InformationLabels_ShowTheirPicture_AndTheSelectedOneDiffers`, `IconButtons_DrawTheirPicture_At100_150_200`, and the opt-in
  `Gallery_OtherTreesAndLists`.
- Changed: `PictureMapTests` (a converted row's property may be `ArrayIcons<Name>[<index>]`, the file's own array, with or without the
  `OpenPmsDental.UI.` prefix), `IconLibraryTests` (128, 128, 128; the three Filled members and the roles), `WpfImagesTests` (128).
  `SourceScanTests` (only the factory makes ImageLists), `RenameGuardTests`, the layout snapshot and the bounds record: green, unchanged.
- Records: `icons/picture-slices.tsv` rows UI-9b-3 (User Controls: tasks and security tree), UI-9b-3b (Forms: auto notes, email inbox, setup wizard),
  UI-9b-3c (SetupWizControls) and UI-9b-3d (ControlAppt), because a slice row names one folder; `icons/picture-map.tsv` one row per index;
  `icons/picture-converted.tsv` 37 rows (how `ImageList` for the arrays, `GetBitmap` for the buttons). `Convert-PictureUses.ps1 -Root . -Slice <each>
  -Check` exits 0 for UI-9b-3, 3b, 3c and 3d and, as before, for UI-9b-1 and UI-9a-4.
- Runs in `.git/od-tools/test-results/`: `ui9b3-ui-wc.txt` (UiRefreshTests 247 passed + 9 opt-in skipped; WindowChecksTests 29 + 44 opt-in
  skipped; NoHqTests 122), `ui9b3-unit.txt` (UnitTests 1,728 + 3 skipped, NoHqTests 122), `ui9b3-before.txt` (the before pictures). Full x64 filter
  build: 0 errors. InitSqlTests and ForkMigrationTests not run (no schema work; `db/init/new-database.sql` not regenerated).

### 4.4 Gallery G-UI3, the UI-9b-3 slice

`.git/od-tools/test-results/G-UI3-ui9b3/index.html` (`G-UI3-ui9b3.zip` beside it; plain-words summary at the top; any download link in the page is
not needed). 24 pictures, all changed, rendered off screen with synthetic nodes: the tasks, security and auto notes trees, the tasks and setup wizard
grids, the tasks toolbar, the nine email and wizard buttons, the two information labels, each at 100, 150 and 200%. The before set draws the
extracted pictures of the old streams (`ui9b3-pictures/old-lists`, named `<window>-<list>-NN.png`) with the old sizes, as before the slice.

### 4.5 Owner questions (new icon choices and looks, to approve in the gallery)

1. Tasks: the done task is a green check in a box (Checkbox Checked, Success), the read task an empty box, the new task a solid orange square, the task
   list with new tasks a solid orange list (the old pictures were all coloured). Orange and green are the role colours (UQ-4); the "new" ones are
   Filled so they do not depend on colour. Fine?
2. Security tree: a heading (the old grey box) is a folder; not granted an empty box; granted a green check in a box; granted for some items a
   green box with a dash (the old green square). Fine, or keep a plain box for the headings?
3. Security tree rows are 1 px taller (16 where they were 15) so the 16 px icons show whole. Fine?
4. Setup wizard: the information label is the Fluent info circle (Text colour); the selected one is its solid twin in the accent colour (the old
   pictures were a bright blue and a dark teal i). Fine?
5. Setup wizard buttons: Skip two chevrons, Next a chevron, Back a chevron (as today's pictures), instead of arrows. Fine?
6. Email inbox buttons: Compose is the pencil (Edit), Refresh the circle arrows (Sync, as Update Fees), Reply, Reply All and Forward the curved
   arrows, Search a magnifier. The old ones were round teal badges. Fine?
7. The auto notes trees keep one picture for every node (the document), because the code never sets another index; Fluent folder and document.
   Fine?
8. A grid column's pictures (the task lists, the reminders, the setup wizard's "?" column) are drawn by `GridOD`, which scales a 96 DPI list, so
   at 150% and 200% they are scaled up (a little soft), as the old bitmaps were; the trees are drawn at the real size and are sharp. A small change to
   `GridOD` (an icon list at the DPI size that it does not scale) would make them sharp too. Do it in UI-9b-4 or UI-11, or leave?
9. The unused list kept only as an array (the email mailboxes, `FormEmailInbox.ArrayIconsImageListMailboxes`) and the three unused wizard lists
   removed with no code: fine?

### 4.6 For the lead

Nothing needs a shown window; the pictures render off screen. To rebuild the gallery: set `OPMS_UI9B3_OLD_LISTS` to
`.git/od-tools/test-results/ui9b3-pictures/old-lists` and `OPMS_UI9B_PICTURES` to a folder named `before`, run the Targeted UiRefreshTests, then
again with `OPMS_UI9B_PICTURES` set to a folder named `after` (no old lists needed). Delete the other slices' pictures the UI-9b-1 and UI-9b-2 gallery
tests also write there (`toolbars/<module>-*`, `toolbars/imaging-*`, `trees/imaging-tree-*`), then
`powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before <before> -After <after> -Out <folder> -Name G-UI3 -Note "<summary>"`.
A look at the real windows (the setup wizard, the security group window, the auto notes, the email inbox, the Tasks list in the main window) on a
connected desktop at 125% and 150% waits for the lead's session: they are shown windows and some need the program's database, so no check runs here.

## 5. UI-9b-4: the remaining forms' toolbars and lists

On `ui/ui9b4-1` from `main` at `e2ae5465a`. Visible change: the toolbars and lists of 29 more windows draw Fluent icons, and the pictures in grid columns
(task lists, reminders, the setup wizard's "?" column, the info buttons) are drawn at the real size of the screen, sharp at 150 and 200%. No schema change,
no NuGet package, no download (the SVGs come from UI-7a's approved download, 1.1.346), no program started, no database, no window shown.

### 5.1 The lists

| Window | List (removed with its stream) | Today's index order (the file's array) | How it is shown now |
|---|---|---|---|
| `FormAccounting`, `FormJournal` | `imageListMain` (3) | Add, Edit or Print, Export | `ToolBarMain.SetImageListIcons`; every button takes its `EnumIcons` |
| `FormClaimsSend` | `imageList1` (7) | Preview, NewDocument (Blank), Print, Complete, Send, DocumentText (Get Reports), Label | both toolbars (`ToolBarMain`, `ToolBarHistory`) |
| `FormCareCreditWeb`, `FormWebView` | `imageList1` (2) | ArrowLeft, ArrowRight | the toolbar |
| `FormTPsign`, `FormPrintPreview` | `imageListMain` (3) | Print, ArrowLeft, ArrowRight | the toolbar |
| `FormReportComplex` | `imageListMain` (6) | Print, ArrowLeft, ArrowRight, Export, ZoomIn, ZoomOut | the toolbar |
| `FormReportCustom`, `FormReportForRdl` | `imageListMain` (7) | Print, ArrowLeft, ArrowRight, Export, Pdf, ZoomIn, ZoomOut | the toolbar; the Zoom button's two states set `Icon` instead of `ImageIndex` |
| `FormEmailEdit` | `imageListMain` (20) | Sync, Save, DeleteX, Cut, Copy, Paste, Undo, Link, LinkExternal, Heading1 to 3, Bold, Italic, TextColor, Table, Image, TextFont, SaveDraft, Settings | `toolBarTop` and `toolBarBottom` |
| `FormWikiEdit` | `imageListMain` (21) | the same first 19, then LockOpen, LockClosed | `ToolBarMain` and `toolBar2`; the Lock button sets `Icon` |
| `FormWiki` | `imageListMain` (15) | ArrowLeft, ArrowRight, Settings, Home, Edit, Print, Rename, Archive, History, Import, Add, Copy, Search, ListBullets, NewDocument | the toolbar |
| `FormRecordAudio` | `imageListMain` (2) | Record, Stop | `butStart.Image = IconLibrary.GetBitmap(...,22,22)` where it took `imageListMain.Images[i]` |
| `FormCDSIntervention`, `FormEhrLabOrderEdit2014`, `FormMedical`, `FormSnomeds` | `imageListInfoButton` (1, 16 px) | Info | the info button column of their grids: `GridColumn.ImageListFactory` (5.2) |
| `FormEtrans835s`, `FormRpProcOverpaid`, `FormRpDPPOvercharged`, `FormRpProcNotBilledIns` | `imageListCalendar` (2, 10 px) | ChevronDown, ChevronUp | no control showed these lists; kept as an array at its indexes (never cut) |
| `FormAccountPick`, `FormWebBrowserPrefs`, `FormCentralSecurity` (4, 15 px) | `imageListMain`, `imageList1`, `imageListPerm` | Add, Edit; ArrowLeft, ArrowRight; ImageSelectorFolder, CheckBoxEmpty, CheckBoxChecked, CheckBoxPartial | no control showed them; arrays only |
| `DashboardTabCtrl` (`OpenPmsDentalGraph`) | `tabImageList` (3, 11 x 30 px) | add tab (`Add`), delete tab (`DeleteX`), delete tab highlighted (`DeleteX` with a ring) | `CodeBase.UiIcons` by name (the project cannot reach `IconLibrary`): 11 px at 96 DPI scaled to the DPI, 30 px tall as the old list, built again when the DPI changes (`OnDpiChangedAfterParent`) |
| `UnitTests` `Form4kTests`, `FormTestAllControls` | `imageListMain` (3), `imageList` (1) | Add, Edit, Export; Edit | the developer test windows are converted too |

Kept, by decision (the plan's 4.5, office content): `FormProcButtons.imageListProcButtons` (the procedure buttons' pictures come from the database; the stream only
gave the list its size and depth), `FormProcButtonEdit.imageList` (the seven default procedure button pictures an office stores as a button's picture) and, from
UI-9b-1, `ControlChart.imageListProcButtons`. `FormErx.imageList1` and `ODDateRangePicker.imageListCalendar` never had a picture (no stream; the designer only sets
the depth), so there is nothing to convert; `FormErx`'s Back and Forward buttons keep the picture slot of a list without pictures, as in the original.
`ControlImages` has a colour swatch button whose bitmap is content. The scan (5.4) names these.

The window's own array keeps the old order, so a plug-in that picks a picture by number still gets the same purpose; a list with no toolbar is an array only.
Where a button took an index and the list gave no better icon: the unused `cancel.gif` of the editors is `DeleteX` (a trash can; no button shows it), the
wiki's Archive keeps the delete cross as `Archive` (Fluent Archive box, a new icon) and In-Links is `Import`.

### 5.2 `GridOD`: a column list at the grid's scale

`GridColumn` gets three properties (`OpenPmsDentalBusiness/UI/GridColumn.cs`, `GridOD.cs`):

- `ImageListFactory` (`Func<int,ImageList>`): the grid calls it with the pixel size of `ImageListSize96` (default 16) at its scale, zoom included, when it lays out
  its rows (`ComputeRows`, so also after `SetScaleAndZoom`), and again when that size changes. It sets the column's `ImageList` and `IsImageListAtScale = true`.
  The caller owns the lists: `IconLibrary.GetImageListForSize(list,array,size)` keeps one list per size and disposes the one it replaces, so a column made again
  for each fill gets the same list and nothing is rebuilt.
- `IsImageListAtScale`: the grid draws each picture of the list pixel for pixel (`DrawImage(img,x,y,img.Width,img.Height)`) and takes the row height from the
  list's own height (`ScaleI(16)` equals the list's size, so the row is as high as with the old 96 DPI list). The print and PDF paths draw it at its 96 DPI size
  (`UnscaleImage`), as the 96 DPI list prints.
- A column whose `ImageList` is set by the caller (a 96 DPI list) is untouched: scaled by the grid when drawn, row height from `ScaleI(ImageSize.Height)`.

Converted to the factory: `UserControlTasks` (task lists), `ControlAppt` (reminders), `FormSetupWizard` ("?" column) from UI-9b-3, and the four info button forms.
Their lists are disposed with the control. At 100% the picture is the same size as before; at 150 and 200% it is 24 and 32 px, sharp.

### 5.3 New `EnumIcons` (block "UI-9b-4" in both enums; 147 each)

| Member | Purpose | Fluent (regular) | Sizes |
|---|---|---|---|
| `DocumentText` | Claims: Get Reports (page of text lines) | Document Text | 16 to 48 |
| `Link`, `LinkExternal` | email and wiki editors: link buttons | Link, Link Square | 16 to 48; 16, 20, 24 |
| `Heading1`, `Heading2`, `Heading3` | editors: Heading1 to 3 | Text Header 1 to 3 | 20, 24 |
| `Bold`, `Italic`, `TextColor`, `TextFont` | editors | Text Bold, Text Italic, Text Color, Text Font | 16, 20, 24 |
| `SaveDraft` | editors: Save as Draft | Save Edit | 20, 24, 32 |
| `Settings` | email editor and wiki: Setup (gears) | Settings | 16 to 48 |
| `LockOpen`, `LockClosed` | wiki editor: Unlock, Lock | Lock Open, Lock Closed | 16 to 48 |
| `Home`, `History`, `Archive` | wiki: Home, Hist, Archive | Home, History, Archive | 16 to 48 |
| `Rename` | wiki: Rename | Rename | 16, 20, 24 |
| `Stop` | record audio: stop (Text role, as `Record`) | Stop | 16, 20, 24 |

19 manifest rows, 65 SVGs from the same 1.1.346 download (`SOURCE.txt` lines added), `IconData.g.cs` regenerated (850 designs). Reused: Add, Edit, Export, Print,
Preview, NewDocument, Complete, Send, Label, ArrowLeft, ArrowRight, Pdf, ZoomIn, ZoomOut, Sync, Save, Cut, Copy, Paste, Undo, Table, Image, Import, Search,
ListBullets, DeleteX, Info, ChevronDown, ChevronUp, Record and the permission pictures. The larger designs are used where the download has them (the 32 and 48
designs of Settings, Home, History, Archive and the locks).

### 5.4 Tests (Targeted: UiRefreshTests, WindowChecksTests, UnitTests, NoHqTests)

- New `Toolbars/RemainingFormsImageListTests`: `Lists_KeepTodaysIndexOrder_AsIcons` (the source and the compiled arrays of every list),
  `Designers_HaveNoStreamLeft_ToolbarsTakeTheListAndButtonsTheirIcons`, `StateAndDirectionPictures_StayDistinguishable_ByShape` (every list's pictures differ in
  shape at 16, 24 and 32 px), `ToolbarLists_BuildWithTheirIcons_AndAnImageIndexButtonStillDraws_At100_150_200`,
  `GridColumnFactory_GivesAListAtTheScale_DrawsItPixelForPixel_At100_150_200` (the grid's picture is the list's pixels exactly), `GridColumnFactory_IsAskedAgainWhenTheScaleChanges_AndKeepsTheRowHeight`
  (16, 24, 32 px; the row height equals the old 96 DPI list's), `GridColumn96DpiList_StillDrawsScaledByTheGrid`, the callers of the factory, the record audio
  button, the dashboard tab pictures, and the opt-in `Gallery_RemainingFormsAndGridColumns`.
- `SourceScanTests.DesignerStreams_AreOnlyTheKeptLists_AndToolbarPicturesAreIcons` (WindowChecksTests): the designers that still load an ImageList stream are only
  `ControlChart`, `FormProcButtons` and `FormProcButtonEdit` (office content); no toolbar button takes a picture by number outside `FormErx`. The earlier
  `ImageLists_TakeTheirDepthFromAStreamOrSetIt` (only the factory makes an ImageList in code) stays green.
- Changed: `IconLibraryTests` (147, 147, 147), `WpfImagesTests` (147). `PictureMapTests`, `RenameGuardTests`, the layout snapshot and the bounds record: green,
  unchanged. `tools\Convert-PictureUses.ps1 -Root . -Slice <id> -Check` exits 0 for UI-9b-4, 4b, 4c, 4d, 4e, 4f and 4g (one row each: Forms, Forms Reports,
  ReportingComplex, ReportingForRdl, OpenPmsDental/UI, CentralManager, UnitTests/UI Testing; the dashboard's rows are map rows of slice UI-9a-6j, because its
  project cannot name `EnumIcons`) and for the earlier slices.
- Runs in `.git/od-tools/test-results/`: `ui9b4-ui-wc.txt` (UiRefreshTests 254 passed + 13 opt-in skipped; WindowChecksTests 30 + 44 opt-in skipped; NoHqTests 122),
  `ui9b4-unit.txt` (UnitTests 1,728 + 3 skipped), `ui9b4-before.txt` and `ui9b4-after.txt` (the gallery runs). Full x64 filter build: 0 errors. InitSqlTests and
  ForkMigrationTests not run (no schema work; `db/init/new-database.sql` not regenerated).

### 5.5 Gallery G-UI3, the UI-9b-4 slice

`.git/od-tools/test-results/G-UI3-ui9b4/index.html` (`G-UI3-ui9b4.zip` beside it; plain-words summary at the top; any download link in the page is not needed).
39 pictures, 35 changed (the four grid pictures at 100% are the same): each converted toolbar list as a toolbar with one button for each picture of the list, and
the four grid columns (tasks, reminders, setup wizard, info button), before and after at 100, 150 and 200%, off screen, synthetic data. The before set draws the
extracted pictures of the old streams (`ui9b4-pictures/old-lists`, named `<window>-<list>-NN.png`) and the 96 DPI list scaled by the grid. The dashboard's tab pictures are
not in the gallery (they need the dashboard control); the tab control draws them at the list's size.

### 5.6 Owner questions

1. The editors' unused "cancel" picture is a trash can (`DeleteX`) and the wiki's Archive keeps no red cross but a new Archive box icon. Fine?
2. Heading1 to Heading3, Bold, Italic, Color, Font are the Fluent text-format icons (an H1 and an A with a colour bar). Fine?
3. The dashboard's highlighted delete picture is the delete icon with a ring in the danger colour (the old one was a darker cross with a black border). Fine?
4. `FormErx`'s Back and Forward buttons have no pictures today (an empty list) and keep none. Give them arrows, or leave?
5. The unused lists (calendar arrows, permission pictures, account pick, web browser prefs) are kept as arrays only. Fine, as for the email mailboxes?

### 5.7 For the lead

Nothing needs a shown window. To rebuild the gallery: `OPMS_UI9B4_OLD_LISTS` = `.git/od-tools/test-results/ui9b4-pictures/old-lists` (extract again from `main`
with the streams of the .resx files, one png per picture), `OPMS_UI9B_PICTURES` = a folder named `before`, run the Targeted UiRefreshTests, then again with a folder named
`after`; delete the other slices' pictures the older gallery tests write there (keep `toolbars/Form*` and `grids/*`), then
`powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before <before> -After <after> -Out <folder> -Name G-UI3 -Note "<summary>"`.
A look at the real windows (the editors, the wiki, the claims window, a task list, the dashboard's edit mode) at 125% and 150% on a connected desktop waits for
the lead's session.

## 6. UI-9b is complete

With UI-9b-4 every ImageList of the program is made by the one icon factory or is a list of office content:

| Rule | State |
|---|---|
| Only the icon factory (`UiIcons.ImageList.cs`) makes an ImageList in code | `SourceScanTests.ImageLists_TakeTheirDepthFromAStreamOrSetIt` |
| Designers load a stream only for the lists of office content | `SourceScanTests.DesignerStreams_AreOnlyTheKeptLists_AndToolbarPicturesAreIcons` (ControlChart, FormProcButtons, FormProcButtonEdit) |
| No toolbar button takes a bitmap picture by number | the same test (allow list: `FormErx`, whose list has no pictures) |
| Every converted list keeps today's index order | `OtherTreesImageListTests`, `RemainingFormsImageListTests`, `ToolBarImageListTests`, `ImagingImageListTests` |
| Grid column pictures are sharp at 150 and 200% | `GridColumnFactory_*` (5.2) |
| The scan rerun | `tools\Convert-PictureUses.ps1 -Check` exits 0 for every slice of UI-9a and UI-9b |

What the program-link pictures, plug-in `ImageIndex` buttons and the module toolbars do is unchanged from section 1. The next packages are UI-10 (fit fixes) and UI-2b.
