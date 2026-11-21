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
