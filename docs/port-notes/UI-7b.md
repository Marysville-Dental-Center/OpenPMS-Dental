# Port note: UI-7b (the IconLibrary icons)

Package UI-7b of the UI refresh ([docs/specs/ui-refresh-plan.md](../specs/ui-refresh-plan.md), section 7's UI-7b row, sections 3.5, 4.2 and
4.3), on `ui/ui7b-1` from `main` at `deb903107`. Visible change: every toolbar, button, tree and module bar icon that is an `EnumIcons` now
draws a Fluent UI System Icon, grey with the few role colours of UQ-4. Chart32G, Chart32W and ChartMed32 keep today's pictures until UI-8. No
caller changed, no schema change, no NuGet package, no download (the SVGs come from UI-7a's approved download of the same version), no program
started, no database used, no window shown.

## 1. What changed

- **Manifest** (`icons/manifest.json`): 57 `EnumIcons` rows after the ten `UiGlyph` rows. One row per member that both enums share (the WinForms
  enum's 38 members are all in the WPF enum's 53), so 50 rows for the 50 members that get a Fluent icon, and 7 `<member>Filled` rows: the
  Filled style of the module bar's icons, which UI-6 draws for the selected module (section 3.6). Every size Fluent has is compiled in, but two:
  `person_search_16` and `phone_chat_16` leave their design square (by 0.08 and 0.37 px; UI-7a's table test refuses that), so PatSelect and
  Text draw their 20 px design shrunk at a 16 px slot.
- **SVGs:** 248 more files in `third-party/fluentui-system-icons/icons/`, copied unchanged from
  `.git/od-tools/downloads/ui7a-fluentui-svg-icons-20261117/extracted/package/icons` (1.1.346, the version UI-7a vendored), with their SHA-256
  in `SOURCE.txt`. None needed a generator change (every chosen SVG is plain `<path d>`).
- **Table:** `tools/New-IconData.ps1` regenerated `CodeBase/UI/Icons/IconData.g.cs` (308 designs). The generator and the renderer are unchanged.
- **WinForms `IconLibrary`** (`OpenPmsDental/UI/IconLibrary/IconLibrary.cs`): the same public calls. `Draw(Graphics,...)` fills the Fluent
  design for the slot with Direct2D's path calls (`Direct2d.FillIcon`) into a bitmap over the caller's gradient, as the old vectors did, and
  with GDI+ (`UiIcons.Draw`) when `OnlyGDI` (NoD2D.txt), in design mode, or when Direct2D fails to start (which sets `OnlyGDI`, as before).
  `Draw(Direct2d,...)` fills the design directly. `DrawDisabled` fills in `TextDisabled` (section 4.2, item 6). `DrawWpf(icon)` returns a PNG
  `BitmapImage` of the icon at `Width(icon)` px for the WinForms helpers that fill WPF controls. New: `IsPicture`, `GetDesigns(icon,isFilled)`,
  `GetDesign`, `GetColor` and `DrawColored(g,icon,rectangle,color,colorBack,isFilled)` for UI-6. `IsVector` is now "not a picture".
- **WPF `IconLibrary`** (`WpfControlsOD/UI/IconLibrary/IconLibrary.cs`): `DrawWpf(icon,grid)` adds a `Path` of `grid.Width` DIPs whose `Data` is
  the frozen, cached geometry of the design for the slot's real pixel size at the grid's DPI (`VisualTreeHelper.GetDpi`), filled in the role
  colour's frozen brush. New overload `DrawWpf(icon,grid,brush,isFilled)`, and `GetDesigns`. `IsVector` and `IsWPF` are "has a row".
- **Removed:** 35 of the 38 WinForms `Gen_*.cs` files and all 37 WPF ones, and the WPF `IconSelector.cs` (it only dispatched to them). The
  WinForms `Gen_Chart32G.cs`, `Gen_Chart32W.cs`, `Gen_ChartMed32.cs` stay with a trimmed `IconSelector.cs` (their pictures, until UI-8); the
  plan's "the files go once every `EnumIcons` draws from the table" is read per file. `DrawSvg.cs` stays (a drawing helper, not a `Gen` file;
  nothing calls it now; "never cut unused features").
- **Tests:** section 3. `NoHqTests/UiThemeScanTests.cs`: the coverage test's floor of shared control files goes from 250 to 200 (235 remain);
  `tests/UiRefreshTests/Data/colour-literals.tsv` loses the removed `Gen_*.cs` lines (written by the test's own writer).

## 2. The mapping (section 4.3; the full table is in `icons/README.md`)

| `EnumIcons` | Fluent (regular) | Role |
|---|---|---|
| Appt32, Family32, Account32, TreatPlan32, TreatPlanMed32, Imaging32, Manage32 (module bar; also Filled) | Calendar LTR, People, Receipt, Clipboard Text LTR, Heart Pulse, Image Multiple, Briefcase | Text |
| Chart32 (no code draws it) | Clipboard Pulse | Text |
| Add | Add | Accent |
| DeleteX, PatDelete, BreakAptX | Delete, Person Delete, Calendar Cancel | Danger |
| Complete | Checkmark Circle | Success |
| ArrowLeft, ArrowRight, ArrowsAll | Arrow Left, Arrow Right, Arrow Move | Text |
| CommLog, Email, WebMail, Text, Recall | Chat, Mail, Mail Shield, Phone Chat, Calendar Arrow Repeat All | Text |
| Patient, PatAdd, PatMoveFam, PatSelect, PatSetGuarantor | Person, Person Add, Person Arrow Right, Person Search, Person Money | Text |
| Probe (no code draws it) | Ruler | Text |
| Acquire, Copy, Paste, Crop, Flip, Hand, RotateL, RotateR | Scan Camera, Copy, Clipboard Paste, Crop, Flip Horizontal, Hand Right, Arrow Rotate Counterclockwise, Arrow Rotate Clockwise | Text |
| Export, Import, Print, Info, Video | Arrow Export LTR, Arrow Import, Print, Info, Video | Text |
| ImageSelectorDoc, ScanMulti, ScanPhoto/ImageSelectorPhoto, ScanXray/ImageSelectorXray | Document Text, Document Multiple, Image, Xray | Text |
| ImageSelectorFile, ImageSelectorFolder, ImageSelectorFolderWeb, ImageSelectorMount | Document, Folder, Folder Globe, Layout Cell Four | Text |
| Chart32G, Chart32W, ChartMed32 | today's pictures, until UI-8 | - |

Where the plan's examples were not used: section 4.3 names Receipt Money (Account), Clipboard Task List LTR (Tx Plan) and Clipboard Heart (the
medical Tx Plan) as examples, but none of the three has a 32 px design, so in the module bar's 32 px slot they would draw 24 px beside 32 px
neighbours (section 3.5 never stretches a design up). The package took Receipt, Clipboard Text LTR and Heart Pulse, which have one (owner
question 1). Fluent has no tooth, so Chart32 (which nothing draws) takes Clipboard Pulse until UI-8's tooth.

## 3. Tests (all in the Targeted assemblies)

- `tests/UiRefreshTests/IconLibraryTests.cs`: every `EnumIcons` of both enums has its row (the three pictures none), Regular style, the UQ-4
  role; every `EnumIcons` row is a member or a module icon's Filled row with the same Fluent icon and sizes; each module icon has a 32 px
  design; both libraries hand out the same designs; every member draws non-empty at 16, 20, 22, 24, 32 and 48 px through
  `IconLibrary.Draw` on GDI+ (in its role colour), disabled (in `TextDisabled`), on Direct2D (the same shape as the OnlyGDI path, and through
  `Draw(Direct2d)`), in NoDpi mode (the same pixels DPI-aware and unaware, GDI+ and Direct2D), in WPF (a `Path` with frozen, non-empty geometry
  inside its slot in the role colour; nothing for the pictures, as before); `DrawColored` draws the Filled style in the caller's colour; the
  WinForms `DrawWpf` gives a non-empty image of `Width(icon)` px; the `Gen_*.cs` files are gone but the pictures'.
- `tests/UiRefreshTests/IconRendererTests.cs` (UI-7a's) draws every manifest row, so every new design too, on GDI+, Direct2D and WPF and checks
  the table against the manifest and the SVGs. Its helpers became `internal` for the new tests.
- `tests/UiRefreshTests/IconLibraryPictureTests.cs`: the gallery's pictures (section 4); each run also checks that every icon draws.
- `tests/WindowChecksTests/InProc/InProcChecks.DE.cs`: `WpfImages`, the "no blank image" check of groups D and E, also counts vector icons
  (a `Path`, an `Image` showing a `DrawingImage`) and counts one blank when its geometry or drawing is empty; the note says how many bitmaps
  and vector icons. `tests/WindowChecksTests/WpfImagesTests.cs` checks the rule without a window: every WPF icon is a non-blank `Path`; an
  empty `Path`, an empty `DrawingImage` and an `Image` without a source are blank. Groups D and E themselves run in the lead's session.
- Targeted runs: `od-tools/test-results/ui7b-before.txt` (main's code, the before set), `ui7b-1.txt`, `ui7b-2.txt`, `ui7b-final.txt`.

## 4. Gallery G-UI1 part 3

**Where:** `.git/od-tools/test-results/G-UI1-part3-ui7b/` (`index.html`, its zip beside it). Sources: `test-results/ui7b-icons/before`
(rendered from main's code, the `Gen_*.cs` pictures, before they were removed; 664 PNGs) and `test-results/ui7b-icons/after`.

**How the pictures are made:** `IconLibraryPictureTests.Pictures_EveryEnumIconsDrawsAtEverySlotAndScale`, with `OPMS_UI7B_ICON_PICTURES` set
to the folder, draws each of the 53 names at the 16, 20 and 24 px slots (and 32 px for the six module icons with `Width` 32) at 100, 125, 150
and 200% (the slot's real pixels, `UiTheme.ScaleI`), off screen on white: through the WinForms `IconLibrary.Draw` (Direct2D into a bitmap, as
the program), or, for a name only the WPF library draws (or one the WinForms library drew empty: ImageSelectorFile, ImageSelectorMount), what
the WPF `DrawWpf` puts in its grid (a bitmap scaled into the slot; the old WPF vector shapes laid out and filled with GDI+; or the Fluent
design). It writes `icons/<name>/<slot>px-<scale>.png` and one sheet per scale, `sheet-<scale>.png` (every icon in a row, the same layout
before and after). `Pictures_ModuleIconsFilledInAccent` adds `icons/<name>/32px-<scale>-filled.png`: the selected module's look (Filled,
`Accent`), pictures with no before. Everything rendered while the desktop session was disconnected: none needs a shown window.

**For the lead** (to rebuild the gallery, or the after set at a later commit):

```
set OPMS_UI7B_ICON_PICTURES=D:\Dev\opendental\.git\od-tools\test-results\ui7b-icons\after
powershell -NoProfile -ExecutionPolicy Bypass -File D:/Dev/opendental/.git/od-tools/run-tests.ps1 -Root <worktree> -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests -Out D:/Dev/opendental/.git/od-tools/test-results/ui7b-after.txt
powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before D:\Dev\opendental\.git\od-tools\test-results\ui7b-icons\before -After D:\Dev\opendental\.git\od-tools\test-results\ui7b-icons\after -Name G-UI1 -Out D:\Dev\opendental\.git\od-tools\test-results\G-UI1-part3-ui7b
```

The before set needs main's code before this package (the same test at `249e05539`, the package's first commit, whose IconLibrary is main's).
Nothing of this gallery needs the connected session. On a real desktop the owner may still glance at the module bar and one toolbar at 125%
and 150% (section 8's hand check belongs to UI-3 and UI-2b).

**Summary for the owner** (at the top of the page): see the gallery's note. What changed: the 50 toolbar, button, tree and module bar icons
that are `EnumIcons` are now Fluent icons, single-colour dark grey, with red for delete, cancel and delete patient, green for complete and blue
for Add; the module icons also have a filled version for the selected module (UI-6 uses it). What did not change: window layouts, button and
toolbar sizes, the chart module's tooth pictures (until UI-8), and every icon that is a picture rather than an `EnumIcons` (UI-9).

## 5. Owner questions

1. Module bar icons: Receipt (Account), Clipboard Text LTR (Tx Plan) and Heart Pulse (medical Tx Plan) instead of the plan's examples Receipt
   Money, Clipboard Task List LTR and Clipboard Heart, because those have no 32 px design (they would draw smaller than their neighbours).
   Agree, or keep the examples at 24 px in the 32 px slot?
2. Chart32 (drawn nowhere today) takes Clipboard Pulse until UI-8 makes the tooth. Fine?
3. The three icons with no obvious Fluent counterpart: Probe (Ruler), Acquire "Mount / Acquire from device" (Scan Camera), Set guarantor
   (Person Money). Fine?

## 6. Notes

- The WPF icons take the DPI of the grid when `DrawWpf` runs (before it is in a window, the system DPI). A window moved to a monitor of
  another DPI keeps the geometry chosen at the first; WPF still scales it, so it stays sharp enough; the module bar and toolbars of the main
  window are WinForms and redraw at the new DPI.
- `IconLibrary.DrawColored` and the WPF `DrawWpf(icon,grid,brush,isFilled)` exist for UI-6 (module bar states, toggled toolbar buttons); no
  caller uses them yet.
- `MANIFEST.txt` of the download folder still says "the 53 SVGs": that file belongs to UI-7a's download; `SOURCE.txt` in the repository lists
  every vendored file.
