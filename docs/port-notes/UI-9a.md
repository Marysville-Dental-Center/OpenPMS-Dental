# Port note: UI-9a (bitmap pictures to icons)

Package UI-9a of the UI refresh ([docs/specs/ui-refresh-plan.md](../specs/ui-refresh-plan.md), section 7's UI-9a row, with sections 3.5,
4.3 and 4.5). Seven slices; each converts the pictures of its folders with one tool and one map. This note has the tool (section 1), how a
later slice uses it (section 2) and one section per slice (UI-9a-1: section 3).

## 1. The tool: `tools/Convert-PictureUses.ps1`

Windows PowerShell 5.1, ASCII, no program, no database, no network. Its help (`Get-Help tools\Convert-PictureUses.ps1 -Full`) has every
rule; in short:

- **Slices.** `icons/picture-slices.tsv`: slice, folder, file name patterns (`UI-9a-1  OpenPmsDental/Forms  FormA*,FormB*,FormC*`).
  `-Slice <slice>` scans the folder's `.cs` and `.resx` files whose names match. A slice is listed there once it is converted:
  `PictureMapTests` checks every listed slice.
- **Inventory** (no switch, or `-Inventory <tsv>`). Every picture use, with file, line, control, the control's type (from
  `this.<control> = new <type>()`), property, the picture's pixel size (read from the image: the `Resources` file or the `.resx` bytes)
  and its map key and target. Kinds: `Resources` (a `Properties.Resources.<name>` picture assigned to `Image` or `BackgroundImage`),
  `ResourcesCode` (any other code use of a `Properties.Resources` picture: reported for a hand edit, never changed), `Resx` (a designer
  `resources.GetObject("<name>")` picture), `ImageList` (a designer ImageList stream: UI-9b), `ResxOnly` (a picture in a slice `.resx` that
  no designer line reads; the resx header comment's samples, `Icon1` and the like, are skipped) and `Converted` (a conversion recorded
  earlier in `icons/picture-converted.tsv`).
- **The map** `icons/picture-map.tsv`: key, target, note, optional size. Keys: `Resources:<name>` (one row covers every window that shows that
  `Properties.Resources` picture: one row per purpose, section 4.3) or `<resx file>:<entry>` (one window's own picture); `*` patterns are
  allowed, an exact key wins. Targets: `EnumIcons.<member>`, `DentalIcon.<member>`, `keep` (the note gives section 4.5's reason; required)
  or `UI-9b`. The size column (`16x16`) gives the bitmap another size than the picture's; keep the width, which is where a button's text
  starts.
- **Apply** (`-Apply`). For each `Resources` or `Resx` use with an icon target:
  - an `OpenPmsDental.UI.Button` with a 22 x 22 picture whose 22 px Icon slot lies inside the button's border at 96 DPI (the slot as
    `Button.DrawTextAndImage` places it: `ImageAlign` MiddleLeft x 6, MiddleRight x width-30, MiddleCenter (the default) centred, y
    centred, plus `AdjustImageLocation`) gets `this.<control>.Icon = OpenPmsDental.UI.EnumIcons.<member>;` in place of the `Image` line, and
    its vertical-only `AdjustImageLocation` (`new System.Drawing.Point(0, n)`) goes: the nudge was for the old picture's art, the Fluent
    icon is centred, and UI.Button fills the Icon slot opaque (Direct2D draws into a bitmap over the button's fill), so a slot moved down
    1 px covered the button's bottom border (seen in the first after set of UI-9a-1). A horizontal nudge stays (it also places the text of a
    MiddleRight button);
  - any other control, a picture of another size, or a slot that would pass the border keeps its `Image` line with
    `OpenPmsDental.UI.IconLibrary.GetBitmap(<icon>,<width>,<height>)`: a transparent 32-bit bitmap of the picture's 96 DPI size with the
    icon centred in the largest square that fits (section 3.5: the largest design that fits, never stretched up), so nothing moves and the
    control draws it as it drew the picture (scaled by LayoutManager, greyed by `ControlPaint` when disabled);
  - a per-window `.resx` entry no designer line reads any more is removed;
  - a `Properties.Resources` picture whose last use in the repository the slice replaced is removed: its `Resources.resx` entry, its
    property in `Resources.Designer.cs` and its image file (unless another `.resx`, project or props file names it). "Last use" is a search
    of every `.cs`, `.xaml` and `.vb` file of the repository (build output, packages, dot folders, the generated `Resources.Designer.cs`
    files and `tools/project-baseline` left out) for `Resources.<name>` and `GetObject("<name>"`;
  - `icons/picture-converted.tsv` records each conversion (slice, file, control, property, key, target, how, width, height); a slice's rows
    are added, other slices' kept.
  Files keep their bytes but the changed lines: the BOM, each line's end, the encoding (a file that does not survive a UTF-8 round trip is
  refused).
- **Check** (`-Check`). Writes nothing; prints what `-Apply` would change; exits 1 when a use has no map row or something would change.
  `-Check -Record <path>` writes the planned record to another path (the gallery's before run).
- **Refused:** a map row whose target names no member, a `keep` row without a reason, two rows for one key, a picture whose size cannot
  be read, `-Apply` without `-Slice`.
- `IconLibrary.GetBitmap(EnumIcons,width,height)` and `GetBitmap(DentalIcon,width,height)` are new in `OpenPmsDental/UI/IconLibrary`
  (GDI+, so they work in design mode, with OnlyGDI and before any window exists; 96 DPI resolution, role colour).

## 2. How a later slice uses it

1. Add the slice's row to `icons/picture-slices.tsv` (UI-9a-2: `OpenPmsDental/Forms  FormD*,FormE*,...,FormL*`). A slice name has one
   row (one folder); a slice of several folders (UI-9a-6, UI-9a-7) gives each folder a name of its own (`UI-9a-6a`, `UI-9a-6b`, ...).
2. `powershell -NoProfile -ExecutionPolicy Bypass -File tools\Convert-PictureUses.ps1 -Slice <slice>`: the "NO MAP ROW" lines are the
   rows to add. Most `Resources:*` rows exist already (UI-9a-1's up, down, Left, Right, butPrint, butPrintSmall, butPreview, butExport,
   butLabel, butPin, editPencil); add the window `.resx` rows, a `UI-9b` row per ImageList and `keep` rows with their 4.5 reason.
3. A purpose with no `EnumIcons` member: add the member to both enums (`OpenPmsDental/UI/IconLibrary/IconLibrary.cs` and
   `WpfControlsOD/UI/IconLibrary/IconLibrary.cs`, in a block of the slice's own after the last block, so slices running side by side do not
   meet), its manifest row after the last `EnumIcons` row, the SVGs from the same 1.1.346 download
   (`.git/od-tools/downloads/ui7a-fluentui-svg-icons-20261117/extracted/package/icons`) with their SHA-256 lines in
   `third-party/fluentui-system-icons/SOURCE.txt`, then `tools\New-IconData.ps1`; update the member counts in
   `tests/UiRefreshTests/IconLibraryTests.cs` and `tests/WindowChecksTests/WpfImagesTests.cs`.
4. Commit the plan pictures first if the gallery needs a before set: `-Check -Record <folder>\planned-record.tsv`, build, run
   `PictureMapTests.Gallery_ConvertedWindowsAndControls` with `OPMS_UI9A_PICTURES=<folder>\before` and `OPMS_UI9A_RECORD` set to the
   planned record (section 3.5).
5. `-Apply`, build, the slice's Targeted run (`PictureMapTests` checks the map, the record and the built windows), the after pictures,
   `tools\Test-NoEolFlips.ps1 -Base main`.
6. Other projects (UI-9a-7) need another `GetBitmap` call (RdlDesign links the renderer, CodeBase calls `UiIcons.GetBitmap`): the
   generated text is in one place of the tool (the `-Apply` part, "GetBitmap"); give it a parameter then.

## 3. UI-9a-1: Forms A-C

On `ui/ui9a1-1` from `main` at `a0264972c`. Visible change: 62 button pictures in 30 windows are Fluent icons. No schema change, no NuGet
package, no download (the SVGs come from UI-7a's approved download), no program started, no database, no window shown.

### 3.1 What the tool found (`OpenPmsDental/Forms`, `FormA*`, `FormB*`, `FormC*`: 402 files)

| Kind | Uses | What happened |
|---|---|---|
| `Resources` | 54 | converted: up, down, Left, Right, butPrint, butPrintSmall, butPreview, butExport, butLabel, butPin, editPencil |
| `Resx` | 8 | converted: FormApptEdit butPin, FormApptSearchAdvanced butMore, FormApptsOther butNote, FormArManager butSend, butUpdateStatus, butExcludedSend, FormClaimEdit butSend, FormClaimResend butSend; their `.resx` entries removed |
| `ResourcesCode` | 3 | `keep`: the ADA claim form background `_1500_02_12` in `FormClaimFormEdit.cs` (2) and `FormClaimPrint.cs` (section 4.5, office content) |
| `ImageList` | 7 | `UI-9b`: FormAccounting, FormAccountPick, FormAutoNoteExport, FormAutoNotes, FormCareCreditWeb, FormCDSIntervention, FormClaimsSend |

No `BackgroundImage`, no PictureBox, no window icon in Forms A-C (the 132 `Icon1` entries the first draft listed were the resx header's
sample comment). The `Resources.resx` picture `butPin` had its last use in `FormApptsOther` and is gone with `OpenPmsDental/Resources/butPin.gif`
(the stage 1 record `tools/project-baseline/items.txt` still lists the file: the lead re-takes that record; its opt-in check reports the
file as removed on purpose). The other ten `Resources` pictures are still used by later slices' windows (and `Forms Reports`, `Bridges`)
and stay until their last slice.

### 3.2 The map (`icons/picture-map.tsv`) and the new `EnumIcons`

| Picture | Purpose | `EnumIcons` | Fluent | New in UI-9a-1 |
|---|---|---|---|---|
| up.gif (green arrow up) | move up in a list | `Up` | Arrow Up | member and row |
| down.gif (green arrow down) | move down in a list | `Down` | Arrow Down | member and row |
| Left.gif, Right.gif (blue arrows); FormApptSearchAdvanced butMore | move, back, forward, insert, copy, more | `ArrowLeft`, `ArrowRight` | Arrow Left, Arrow Right | - |
| butPrint.gif, butPrintsmall.gif | print, postcard preview | `Print` | Print | WinForms member (the WPF row existed) |
| butPreview.gif | print preview | `Preview` | Document Search | member and row |
| butExport.gif | export | `Export` | Arrow Export LTR | WinForms member (the WPF row existed) |
| butlabel.gif | mailing labels | `Label` | Contact Card | member and row |
| butPin.gif, FormApptEdit butPin | to the pinboard | `Pin` | Pin | member and row |
| editPencil.gif | edit | `Edit` | Edit | member and row |
| FormApptsOther butNote | a note for the patient | `Note` | Note | member and row |
| red up arrow (claims, TSI) | send; update at TSI | `Send` | Send | member and row |

All role `Text` (UQ-4). The WinForms enum has 48 members (UI-7b's 38 and 10), the WPF enum 61 (53 and 8); every WinForms member is a WPF
member (WpfConverter parses the names). 8 manifest rows after the last `EnumIcons` row, 38 SVGs (16, 20, 24, 32 and 48 px where Fluent has
them), `IconData.g.cs` regenerated (546 designs).

Where the plan's examples were not used: section 4.3 names Document Print for print preview; it has no 16 px design and the preview
pictures sit in 16 px slots, so `Preview` takes Document Search (a page with a magnifier, as today's picture), which has one.

### 3.3 The conversion

62 picture lines (`icons/picture-converted.tsv`): 48 `UI.Button.Icon` (22 px pictures on buttons with room for the slot) and 14
`IconLibrary.GetBitmap`: the 16 x 14 print pictures as 16 x 16 (the map's size; the icon would be 12 px in a 14 px slot), the 18 x 9
label pictures as 18 x 16 (likewise), butPreview 17 x 16, butMore 13 x 15, FormApptsOther butPin 20 x 23, and four 22 px arrows on buttons
whose slot would pass the border (FormClaimPrint butBack and butFwd, 18 x 23; FormAlertCategorySetup and FormClaimForms butCopy, 23 px
high), drawn 22 x 22 and transparent as the pictures were. 8 vertical nudges (`AdjustImageLocation (0, 1)`) removed. Bounds unchanged:
the layout snapshot and the scan's bounds record pass as they are (`Data/layout-snapshot.tsv` not rewritten).

Known behaviour differences, both small: an `Icon` button no longer widens itself for a picture when its text changes at run time (UI.Button's
`OnTextChanged` counts `Image` but not `Icon`; with today's texts nothing changes, and a translated longer text widens it 22 px less); a
`GetBitmap` picture on a scaled window is scaled like the old picture (LayoutManager scales the bitmap; the `Icon` buttons draw the vector
at the real size).

### 3.4 Tests (Targeted: UiRefreshTests, WindowChecksTests, UnitTests, NoHqTests)

- New `tests/UiRefreshTests/PictureMapTests.cs`: the map has a row for every picture use the tool inventories in every slice of
  `icons/picture-slices.tsv`, and the slice has nothing left to convert (the tool's own `-Check` exits 0); every recorded conversion is in its
  designer line and the control has no picture line left; every `resources.GetObject` and `Properties.Resources` picture the program's code
  reads still has its resource (the "no designer `Image=` points to a removed picture" test, over all of `OpenPmsDental`); `GetBitmap` has
  the asked size, 96 DPI, the icon in its role colour, centred; each converted window, built off screen by UI-0b's builder, shows the icon
  (`Icon` set, no `Image`) or a non-empty bitmap of the recorded size; the opt-in gallery test.
- Changed: `IconLibraryTests` (the member counts 48, 61, 61), `WindowChecksTests/WpfImagesTests` (61 WPF icons, each a non-blank vector).
- `RenameGuardTests` (NoHqTests): unchanged and green. Its pack URI check covers the WPF resources; Forms A-C are WinForms and have no
  pack URI or `BitmapFileName` (those are UI-9a-6's).
- Layout snapshot and bounds record: `LayoutSnapshotTests` and `Scan_TodaysFont_ReportsNothingNew` pass unchanged.
- Runs: `.git/od-tools/test-results/ui9a1-before.txt` (the before pictures, on the commit before the conversion; the four new tests that need
  the conversion fail there by design), `ui9a1-1.txt`, `ui9a1-2.txt`, `ui9a1-final.txt` (all passed: UiRefreshTests 146 + 4 opt-in skipped,
  WindowChecksTests 29 + 43 opt-in skipped, UnitTests 1,728 + 3, NoHqTests 122). Full x64 filter build: 0 errors.

### 3.5 Gallery G-UI3, the UI-9a-1 slice

**Where:** `.git/od-tools/test-results/G-UI3-ui9a1/` (`index.html`, `G-UI3-ui9a1.zip` beside it). Sources:
`.git/od-tools/test-results/ui9a1-pictures/before` and `.../after`: `windows/<window>.png` (each of the 30 windows, its content off screen)
and `controls/<window>.<control>.png` (each of the 62 buttons on its own, so the ones on a hidden tab page show too). 92 pictures, 91
changed; `windows/FormClaimPrint.png` is the same before and after (its two small arrow buttons do not show in the off-screen picture of
the whole window, beside its print preview; their `controls/` pictures show them). The owner's summary is at the top of the page.

Everything rendered while the desktop session was disconnected (WinForms `DrawToBitmap` through UI-0b's builder); no picture needs a shown
window or WPF.

**For the lead** (to rebuild the gallery, or the after set at a later commit; `<repo>` is the main checkout):

```
rem the before set: the package's first commit (3bb395a4f, the tool and tests before -Apply), built
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Convert-PictureUses.ps1 -Slice UI-9a-1 -Check -Record <repo>\.git\od-tools\test-results\ui9a1-pictures\planned-record.tsv
set OPMS_UI9A_PICTURES=<repo>\.git\od-tools\test-results\ui9a1-pictures\before
set OPMS_UI9A_RECORD=<repo>\.git\od-tools\test-results\ui9a1-pictures\planned-record.tsv
powershell -NoProfile -ExecutionPolicy Bypass -File <repo>/.git/od-tools/run-tests.ps1 -Root <worktree> -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests -Out <repo>/.git/od-tools/test-results/ui9a1-before.txt
rem the after set: the branch head, built (OPMS_UI9A_RECORD unset: the committed icons/picture-converted.tsv)
set OPMS_UI9A_RECORD=
set OPMS_UI9A_PICTURES=<repo>\.git\od-tools\test-results\ui9a1-pictures\after
powershell -NoProfile -ExecutionPolicy Bypass -File <repo>/.git/od-tools/run-tests.ps1 -Root <worktree> -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests -Out <repo>/.git/od-tools/test-results/ui9a1-after.txt
powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before <repo>\.git\od-tools\test-results\ui9a1-pictures\before -After <repo>\.git\od-tools\test-results\ui9a1-pictures\after -Name G-UI3 -Out <repo>\.git\od-tools\test-results\G-UI3-ui9a1 -Note "<the summary>"
```

The harness's group U pictures of these windows (UI-0c, a shown window) are not needed for this slice: the off-screen set has every changed
button. On a real desktop the owner may glance at one converted window at 125% and 150% (the `GetBitmap` pictures scale as today's did).

### 3.6 Owner questions

1. Mailing labels: Contact Card (a card with a person and lines) for the label picture. Fine, or Tag?
2. Print preview: Document Search (a page with a magnifier, like today's picture) instead of the plan's example Document Print, which has no
   16 px design for the 16 px preview slots. Fine?
3. "Update TSI" had the same red arrow as "Send to TSI" and now has the same Send icon. Fine, or a refresh icon (Arrow Sync)?
4. The 16 x 14 print pictures and the 18 x 9 label pictures are drawn as 16 px icons (the bitmap 2 px or 7 px taller, the width and the
   text where they were). Fine?

## 4. UI-9a-2: Forms D-L

On `ui/ui9a2-1` from `main` at `296aa44d8`. Visible change: 61 button pictures in 36 windows (`FormD*` to `FormL*`) are Fluent icons. No schema change,
no NuGet package, no download, no new icon (every picture means what an earlier member means), no program started, no database, no window shown.

### 4.1 What the tool found (`OpenPmsDental/Forms`, `FormD*` to `FormL*`: 726 files, 67 picture uses)

| Kind | Uses | What happened |
|---|---|---|
| `Resources` | 60 | converted: up, down, Left, Right, butPrint, butPrintSmall, butPreview, butLabel, editPencil, deleteX18, butGoto |
| `Resx` | 1 | converted: FormFeeSchedGroups butAdd (its `.resx` entry removed) |
| `ImageList` | 6 | `UI-9b`: FormEhrLabOrderEdit2014, FormEmailEdit, FormEmailInbox (2), FormEtrans835s, FormJournal |

61 lines: 47 as `UI.Button.Icon`, 14 as `IconLibrary.GetBitmap` (the 16 x 14 print pictures as 16 x 16, butPreview 17 x 16, the label picture 18 x 16, the
18 x 18 clear-value X, and the 22 px arrows or plus whose slot would pass the border or whose button is not a `UI.Button`). 8 vertical nudges removed.
The `Resources` picture `butGoto` had its last use in `FormEcwDiagAdv` and is gone with `OpenPmsDental/Resources/butGoto.gif` (like `butPin` in
UI-9a-1: the lead re-takes `tools/project-baseline/items.txt`). The other pictures are still used by later slices' windows. Bounds unchanged.

### 4.2 The three new map rows (all reuse an earlier member)

| Picture | Purpose | `EnumIcons` |
|---|---|---|
| deleteX18.gif (red X, 18 x 18) | clear the value (FormDefEdit) | `DeleteX` (drawn 18 x 18) |
| butGoto.gif (green circle with an arrow) | run the query (FormEcwDiagAdv) | `ArrowRight` |
| FormFeeSchedGroups butAdd (blue plus) | add | `Add` |

No member added, so the member counts (48 and 61) and `IconData.g.cs` are unchanged.

### 4.3 Tests (Targeted: UiRefreshTests, WindowChecksTests, UnitTests, NoHqTests)

`PictureMapTests` covers the slice through its row in `icons/picture-slices.tsv`; no new test was needed. Runs: `.git/od-tools/test-results/ui9a2-before.txt`
(before pictures; the check that the slice is converted fails there by design), `ui9a2-final.txt`.

### 4.4 Gallery G-UI3, the UI-9a-2 slice

**Where:** `.git/od-tools/test-results/G-UI3-ui9a2/` (`index.html`). Sources: `.git/od-tools/test-results/ui9a2-pictures/before` and `.../after`
(`windows/<window>.png`, `controls/<window>.<control>.png`), rendered off screen with the tool's gallery test.

**For the lead** (rebuild; `<repo>` is the main checkout; the before set is built on the commit "UI-9a-2: slice row and map rows"):

```
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Convert-PictureUses.ps1 -Slice UI-9a-2 -Check -Record <repo>\.git\od-tools\test-results\ui9a2-pictures\planned-record.tsv
set OPMS_UI9A_PICTURES=<repo>\.git\od-tools\test-results\ui9a2-pictures\before
set OPMS_UI9A_RECORD=<repo>\.git\od-tools\test-results\ui9a2-pictures\planned-record.tsv
run-tests.ps1 -Root <worktree> -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests
rem after: OPMS_UI9A_RECORD empty, OPMS_UI9A_PICTURES=...\after, same run; then tools\New-UiGallery.ps1 -Before ...\before -After ...\after -Name G-UI3 -Out ...\G-UI3-ui9a2 -Note "<summary>"
```

### 4.5 Owner questions

No new icon choice. For the gallery: the red X for "clear value" is `DeleteX`, the green "go" circle of the eCW query button is a plain `ArrowRight`
(the circle is gone), and the plus of FormFeeSchedGroups is `Add`. Fine?

## 5. UI-9a-3: Forms M-Q

On `ui/ui9a3-1` from `main` at `1a4fa9152`. Visible change: 68 button pictures in 30 windows (`FormM*` to `FormQ*`) are Fluent icons. No schema change, no NuGet
package, no download beyond UI-7a's approved 1.1.346 SVGs, no program started, no database, no window shown.

### 5.1 What the tool found (`OpenPmsDental/Forms`, `FormM*` to `FormQ*`: 443 files, 72 picture uses)

| Kind | Uses | What happened |
|---|---|---|
| `Resources` | 68 | converted: up, down, Left, Right, butPrint, butPrintSmall, butExport, editPencil (earlier rows) and close_door, arrowDownTriangle, arrowUpTriangle, Microphone_22px, butZoomIn, butZoomOut (new rows); `bola_launch` kept |
| `Resx` | 1 | converted: FormOrthoChartSetup butAdd (its `.resx` entry removed) |
| `ImageList` | 3 | `UI-9b`: FormMedical, FormProcButtonEdit, FormProcButtons |

The show/hide triangles are also assigned in code (`FormPayment.cs`, `FormQueryParser.cs`); the tool converted those lines too, so
`PictureMapTests.Record_EveryConversionIsInItsDesignerLine_WithAKnownIcon` now accepts a code line that omits `this.`. The six `Resources` pictures
close_door, arrowDownTriangle, arrowUpTriangle, Microphone_22px, butZoomIn and butZoomOut had their last use here and are gone with their files
(the lead re-takes `tools/project-baseline/items.txt`, as after UI-9a-1 and UI-9a-2). `FormMountDefEdit` needs arguments the off-screen builder cannot give, so it joins
`_arrayWindowsNeedingArguments` (its designer lines are still checked). Bounds unchanged.

### 5.2 New `EnumIcons` (a "UI-9a-3" block in both enums; 6 manifest rows; 578 designs in `IconData.g.cs`)

| Picture | Purpose | `EnumIcons` | Fluent |
|---|---|---|---|
| arrowDownTriangle.gif, arrowUpTriangle.gif (10 x 10) | show or hide the splits (FormPayment), the query (FormQueryParser) | `ChevronDown`, `ChevronUp` | Chevron Down, Chevron Up |
| close_door.ico | close the case or payment plan | `CloseDoor` | Door Arrow Right |
| Microphone_22px.png | dictate (FormPerio) | `Microphone` | Mic |
| butZoomIn.gif, butZoomOut.gif | zoom the screen print | `ZoomIn`, `ZoomOut` | Zoom In, Zoom Out |

Existing members reused: Up, Down, ArrowLeft, ArrowRight, Print, Export, Edit, Add. The member counts are 55 (WinForms) and 68 (WPF).
The generator's size list has no 28 px design, so those SVGs were not vendored (the SHA-256 lines of the vendored ones are in `third-party/fluentui-system-icons/SOURCE.txt`).
`bola_launch.png` (324 x 129, a Panel background) is a partner logo and stays (section 4.5).

### 5.3 The conversion

68 lines (`icons/picture-converted.tsv`): 47 as `UI.Button.Icon` and 21 as `IconLibrary.GetBitmap` (16 px print pictures as 16 x 16, the 10 x 10 chevrons,
the three door pictures as 16 x 16, 22 px arrows whose slot would pass the border). `close_door.ico` holds a 16, a 128 and a 256 px design; the map draws it
16 x 16. Vertical nudges of `butUp` removed as in UI-9a-1.

### 5.4 Tests (Targeted: UiRefreshTests, WindowChecksTests, UnitTests, NoHqTests)

`PictureMapTests` covers the slice through its row in `icons/picture-slices.tsv`; `IconLibraryTests` and `WpfImagesTests` counts updated. Runs:
`.git/od-tools/test-results/ui9a3-before.txt` (before pictures; the slice-done check fails there by design), `ui9a3-after.txt`, `ui9a3-final.txt`.

### 5.5 Gallery G-UI3, the UI-9a-3 slice

**Where:** `.git/od-tools/test-results/G-UI3-ui9a3/` (`index.html`, zip beside it): 92 pictures (30 windows, 62 controls), 91 changed, rendered off screen.
Sources: `.git/od-tools/test-results/ui9a3-pictures/before` and `.../after-slice` (the gallery test's after set holds every slice's windows; `after-slice`
keeps this slice's names).

**For the lead** (rebuild; `<repo>` is the main checkout; the before set is built on the commit "UI-9a-3: slice row, map rows and six new icons" with the planned record):

```
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Convert-PictureUses.ps1 -Slice UI-9a-3 -Check -Record <repo>\.git\od-tools\test-results\ui9a3-pictures\planned-record.tsv
set OPMS_UI9A_PICTURES=<repo>\.git\od-tools\test-results\ui9a3-pictures\before
set OPMS_UI9A_RECORD=<repo>\.git\od-tools\test-results\ui9a3-pictures\planned-record.tsv
run-tests.ps1 -Root <worktree> -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests
rem after: OPMS_UI9A_RECORD empty, OPMS_UI9A_PICTURES=...\after, same run; keep the before set's names; then tools\New-UiGallery.ps1 -Before ...\before -After ...\after-slice -Name G-UI3 -Out ...\G-UI3-ui9a3 -Note "<summary>"
```

### 5.6 Owner questions (new icon choices, to approve in the gallery)

1. Show or hide splits and query (10 x 10 triangles): Chevron Down and Chevron Up. Fine, or Caret Down and Caret Up (filled triangles, closer to today)?
2. Close Case and Close Plan (door picture): Door Arrow Right. Fine?
3. Dictate (FormPerio butListen): Mic. Fine?
4. Zoom In and Zoom Out (FormPrntScrn): magnifier with a plus or a minus. Fine?
5. FormOrthoChartSetup add: the plain `Add` plus, as in UI-9a-2. Fine?

## 6. UI-9a-4: Forms R-S

On `ui/ui9a4-1` from `main` at `67ca33835`. Visible change: 66 button pictures in 36 windows (`FormR*`, `FormS*`) are Fluent icons. No schema change, no NuGet
package, no download beyond UI-7a's approved 1.1.346 SVGs, no program started, no window shown, scratch databases not used.

### 6.1 What the tool found (`OpenPmsDental/Forms`, `FormR*`, `FormS*`: 372 files, 72 picture uses)

| Kind | Uses | What happened |
|---|---|---|
| `Resources` | 67 | 66 converted: up, down, Left, Right, butPrint, butPrintSmall, butPreview, butLabel, butExport, deleteX18 (earlier rows) and butPin22, record, printPreview20, Add, DeleteX (new rows); `PracticeByNumbers_100x24` kept (vendor logo, 4.5) |
| `Resx` | 1 | `keep`: `FormSplash` `$this.BackgroundImage` (the splash picture, branding, 4.5) |
| `ImageList` | 4 | `UI-9b`: FormRecordAudio, FormSetupWizard, FormSetupWizardProgress, FormSnomeds |

66 lines: 48 as `UI.Button.Icon`, 18 as `IconLibrary.GetBitmap` (16 x 14 print pictures as 16 x 16, label and preview pictures, the 22 px
pictures whose slot would pass the border, the 18 x 18 remove X, the record picture, and the one code-made button of `FormSheetDefEdit`). 3 vertical nudges
removed as before. The `Resources` pictures `butPin22`, `printPreview20` and `record` had their last use here and are gone with their files
(and the `<None Remove>` line of `record.gif` in `OpenPmsDental.csproj`; the lead re-takes `tools/project-baseline/items.txt` as after UI-9a-1 to UI-9a-3).
Bounds unchanged.

### 6.2 New `EnumIcons` (a "UI-9a-4" block in both enums; 1 manifest row; 5 SVGs; 583 designs in `IconData.g.cs`)

| Picture | Purpose | `EnumIcons` | Fluent |
|---|---|---|---|
| record.gif (FormRecordAudio butStart) | start recording | `Record` | Record (a circle with a ring) |

Existing members reused: Pin (butPin22, the recall list's "to pinboard"), Preview (printPreview20), Add, DeleteX, Up, Down, ArrowLeft, ArrowRight, Print,
Export, Label. Member counts now 56 (WinForms) and 69 (WPF).

### 6.3 Tests (Targeted: UiRefreshTests, WindowChecksTests, UnitTests, NoHqTests)

`PictureMapTests` covers the slice through its row in `icons/picture-slices.tsv`. Its built-window test now skips a window whose build needs the
preference cache ("is an invalid pref name": `FormRecallList`, `FormSchedule`) like one that needs a database, and `FormSheetDefEdit` joins the windows that need arguments
(its designer lines are still checked). `IconLibraryTests` and `WpfImagesTests` counts updated. Runs: `.git/od-tools/test-results/ui9a4-before.txt` (before
pictures; the slice-done check fails there by design), `ui9a4-final.txt` (all passed: UiRefreshTests 193 + 8 opt-in skipped, WindowChecksTests 29 + 43 opt-in skipped,
UnitTests 1,728 + 3, NoHqTests 122). Full x64 filter build: 0 errors. `Test-NoEolFlips.ps1 -Base main`: no line ends changed.

### 6.4 Gallery G-UI3, the UI-9a-4 slice

**Where:** `.git/od-tools/test-results/G-UI3-ui9a4/` (`index.html`, zip beside it): 85 pictures (33 windows, 52 buttons), all changed, rendered off screen.
Sources: `.git/od-tools/test-results/ui9a4-pictures/before` and `.../after-slice`. Not pictured: `FormRecallList`, `FormSchedule` (preference cache) and
`FormSheetDefEdit` (arguments); 14 buttons. The lead can picture them in a connected session on the scratch copy (the synthetic dump) with the harness group U.

**For the lead** (rebuild; `<repo>` is the main checkout; the before set is built on the commit "UI-9a-4: slice row, map rows and the Record icon" with the planned record):

```
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Convert-PictureUses.ps1 -Slice UI-9a-4 -Check -Record <repo>\.git\od-tools\test-results\ui9a4-pictures\planned-record.tsv
set OPMS_UI9A_PICTURES=<repo>\.git\od-tools\test-results\ui9a4-pictures\before
set OPMS_UI9A_RECORD=<repo>\.git\od-tools\test-results\ui9a4-pictures\planned-record.tsv
run-tests.ps1 -Root <worktree> -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests
rem after: OPMS_UI9A_RECORD empty, OPMS_UI9A_PICTURES=...\after, same run; keep the before set's names (after-slice); then tools\New-UiGallery.ps1 -Before ...\before -After ...\after-slice -Name G-UI3 -Out ...\G-UI3-ui9a4 -Note "<summary>"
```

### 6.5 Owner questions (new icon choices, to approve in the gallery)

1. Start recording (FormRecordAudio butStart, red record circle): Record, a circle with a ring (regular). Fine, or Mic (already used for dictate)?
2. The recall list's "to pinboard" (butPin22) uses the same Pin as the other pinboard buttons, and the 20 x 20 print preview (printPreview20) the same Preview
   (Document Search) as the 16 px ones. Fine?
3. The remove X (FormSheetFieldComboBox butRemove, 18 x 18) is `DeleteX` as in UI-9a-2; the sheet field windows' delete buttons use `DeleteX` too. Fine?

## 7. UI-9a-5: Forms T-Z and Forms Reports

On `ui/ui9a5-1`, which starts from `ui/ui9a4-1` (UI-9a-4 awaits the owner), so the shared map, manifest and enum blocks stay in one line of history. Visible
change: 77 button pictures in 35 windows (`FormT*` to `FormZ*`, and `OpenPmsDental/Forms Reports`) are Fluent icons. No schema change, no NuGet package, no download
beyond UI-7a's approved 1.1.346 SVGs, no program started, no window shown, no scratch database used. The slice has two folders, so `icons/picture-slices.tsv`
names them `UI-9a-5a` (Forms, `FormT*` to `FormZ*`) and `UI-9a-5b` (`Forms Reports`).

### 7.1 What the tool found

| Slice | Kind | Uses | What happened |
|---|---|---|---|
| UI-9a-5a | `Resources` | 18 | converted: Left, Right, butPrint, butPrintSmall, butPreview, butExport, editPencil (earlier rows), Copy, butPaste (new rows) |
| UI-9a-5a | `Resx` | 5 | converted: FormTimeCard butRight, butLeft, butPrint, butPrevEmp, butNextEmp (the `.resx` entries removed) |
| UI-9a-5a | `ImageList` | 5 | `UI-9b`: FormTPsign, FormWebBrowserPrefs, FormWebView, FormWiki, FormWikiEdit |
| UI-9a-5b | `Resources` | 50 | converted: Left, Right, butPrintSmall, butPreview, butExport, butLabel, Add, DeleteX (earlier rows), butExportExcel, butQView (new rows) |
| UI-9a-5b | `Resx` | 2 | converted: FormRpReferrals and FormRpPatients butDeleteFilter (red X, `DeleteX`) |
| UI-9a-5b | `ImageList` | 3 | `UI-9b`: FormRpDPPOvercharged, FormRpProcNotBilledIns, FormRpProcOverpaid |

`FormRpLaserLabels.cs` builds 30 label cells in code (`PictureBox`, `StretchImage`, 51 x 17): the tool reports them as hand edits, done here as
`IconLibrary.GetBitmap(DeleteX|Label,51,17)` (the icon in the largest square that fits, so the picture stays undistorted; the 18 x 18 and 18 x 9 pictures were
stretched). The seven `Resources` pictures butExport, butExportExcel, butPaste, butPreview, butPrint, butQView and butlabel had their last use here and are
gone with their files (the lead re-takes `tools/project-baseline/items.txt`, as after the earlier slices); `DeleteX`, `Right` and others stay (used by
`UnitTests` or `OpenPmsDentalBusiness` resources). 77 lines in `icons/picture-converted.tsv`: 44 as `UI.Button.Icon`, 33 as `GetBitmap`. Bounds unchanged.

### 7.2 New `EnumIcons`

A "UI-9a-5" block in both enums, 2 manifest rows, 5 SVGs (SHA-256 lines in `third-party/fluentui-system-icons/SOURCE.txt`), `IconData.g.cs` regenerated
(588 designs).

| Picture | Purpose | `EnumIcons` | Fluent |
|---|---|---|---|
| butExportExcel.gif | export the query to Excel (FormQuery) | `ExportExcel` | Document Table (16, 20, 24 px) |
| butQView.gif | the query view (FormQuery) | `QueryView` | Table Search (20, 24 px; the 22 px slot draws the 20 px design) |

`Copy` and `Paste` already had manifest rows and WPF members (the imaging toolbar); this slice adds them to the WinForms enum. Member counts: 60 WinForms, 71 WPF
(`IconLibraryTests`, `WpfImagesTests` updated).

### 7.3 Tests (Targeted: UiRefreshTests, WindowChecksTests, UnitTests, NoHqTests)

`PictureMapTests` covers both slices through their rows in `icons/picture-slices.tsv`. Runs: `.git/od-tools/test-results/ui9a5-before.txt` (before pictures on the commit
before `-Apply`; the slice-done check fails there by design), `ui9a5-final.txt` (all passed: UiRefreshTests 193 + 8 opt-in skipped, WindowChecksTests 29 + 43 opt-in
skipped, UnitTests 1,728 + 3, NoHqTests 122). Full x64 build: 0 errors. `Test-NoEolFlips.ps1 -Base ui/ui9a4-1`: no line ends changed. InitSqlTests and
ForkMigrationTests not run (no schema work).

### 7.4 Gallery G-UI3, the UI-9a-5 slice

**Where:** `.git/od-tools/test-results/G-UI3-ui9a5/` (`index.html`, zip beside it): 112 pictures (35 windows, 77 buttons), rendered off screen. Sources:
`.git/od-tools/test-results/ui9a5-pictures/before` and `.../after-slice`. Every window of the slice pictured.

**For the lead** (rebuild; `<repo>` is the main checkout; the before set is built on the commit "UI-9a-5: ExportExcel and QueryView icons, the laser label pictures by hand, counts"
with the planned record, `planned-record.tsv` = the `-Check -Record` output of both slices):

```
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Convert-PictureUses.ps1 -Slice UI-9a-5a -Check -Record <repo>\.git\od-tools\test-results\ui9a5-pictures\planned-5a.tsv
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Convert-PictureUses.ps1 -Slice UI-9a-5b -Check -Record <repo>\.git\od-tools\test-results\ui9a5-pictures\planned-5b.tsv
rem join them (one header) into planned-record.tsv
set OPMS_UI9A_PICTURES=<repo>\.git\od-tools\test-results\ui9a5-pictures\before
set OPMS_UI9A_RECORD=<repo>\.git\od-tools\test-results\ui9a5-pictures\planned-record.tsv
run-tests.ps1 -Root <worktree> -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests
rem after: OPMS_UI9A_RECORD empty, OPMS_UI9A_PICTURES=...\after, same run; keep the before set's names (after-slice); then tools\New-UiGallery.ps1 -Before ...\before -After ...\after-slice -Name G-UI3 -Out ...\G-UI3-ui9a5 -Note "<summary>"
```

### 7.5 Owner questions (new icon choices, to approve in the gallery)

1. Excel export (FormQuery butExportExcel): Document Table (a page with a table). Fine, or the plain Export icon?
2. Query view (FormQuery butQView): Table Search (a table with a magnifier). Fine, or Database Search?
3. Copy and paste (FormTaskEdit, FormUserQuery): the imaging toolbar's `Copy` and `Paste`. Fine?
4. The time card's previous and next employee buttons use the same arrows as previous and next period; the filter delete buttons (FormRpReferrals, FormRpPatients) use `DeleteX`. Fine?
5. The label sheet cells of FormRpLaserLabels (cleared X and label) are 17 px icons in their 51 x 17 boxes, not stretched. Fine?
