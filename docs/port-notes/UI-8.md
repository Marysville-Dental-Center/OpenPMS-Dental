# Port note: UI-8 (the custom dental icons)

Package UI-8 of the UI refresh ([docs/specs/ui-refresh-plan.md](../specs/ui-refresh-plan.md), section 7's UI-8 row, sections 3.5, 4.2 and
4.4, the owner's answer UQ-7 of 2026-11-17), on `ui/ui8-1` from `main` at `deb903107`. The program's own dental icons, drawn in Fluent UI
System Icons' style, their manifest rows and their place in the icon table, and new-style pictures on the 8 default procedure buttons of new
databases. Nothing in the program draws the icons yet except those pictures (the module bar's three chart pictures switch once UI-7b's
`IconLibrary` maps them, section 6). No schema change, no NuGet package, nothing downloaded, no program started, no window shown; the only
database used is InitSqlTests' own scratch database on the local MariaDB.

## 1. What changed and what did not (for the owner)

- **New:** 20 dental icons, each in a Regular (outline) and a Filled style, at 16, 20, 24, 32 and 48 pixels: the tooth for the Chart
  module, the medical chart, the perio chart and probe, bitewing, periapical, pano and ceph films, the intraoral sensor, the phosphor plate,
  a mount, an intraoral photo, an implant, orthodontics, and seven procedure pictures (amalgam, composite, PFM crown, ceramic crown, root
  canal, root canal with build-up; PA uses the periapical film). One colour, Fluent's line weights, round ends and corners.
- **Changes the look of:** the 8 default procedure buttons of a **new** database only (Amalgam, Composite, Crown-PFM, Crown-Ceramic, RCT,
  RCT BU PFM, RCT BU Ceramic, PA): one-colour icons instead of Open Dental's coloured pictures, same 20x20 size.
- **Does not change:** an office's procedure buttons (never touched by any code path; "Set to default" in the procedure code tools keeps
  Open Dental's pictures), the tooth chart's own drawing (decision 5), the module bar (still today's chart pictures until the follow-up of
  section 6), any window's layout or bounds.

## 2. Files

| File | What |
|---|---|
| `tools/DentalIcons/DentalIconArt.cs` | the drawing: each icon as shapes on a 24 unit square, per size and style; WPF geometry combines them; a curve fit writes compact SVG path data |
| `tools/New-DentalIcons.ps1` | compiles the drawing (Windows PowerShell 5.1, `Add-Type`) and writes `icons/custom/*.svg`; `-Check` compares; `-Preview <folder>` pictures every design |
| `icons/custom/*.svg` | 200 SVGs: 20 icons x Regular and Filled x 16, 20, 24, 32, 48 px; one `<path d>` each, a comment saying it is the program's own GPL work |
| `.gitattributes` | `icons/custom/** -text`: the SVGs keep their bytes (and the SHA-256 the table names) in every checkout |
| `icons/manifest.json` | 40 `DentalIcon` rows, their own block at the end (each Regular row followed by its Filled twin), and the comment names the enum |
| `tools/New-IconData.ps1` | the `DentalIcon` enum, generated like `UiGlyph`; a `DentalIcon` row must be a custom icon; the header names `icons/custom` |
| `CodeBase/UI/Icons/IconData.g.cs` | regenerated: `DentalIcon` and the 200 designs |
| `CodeBase/UI/Icons/UiIcons.Dental.cs` | `GetDesigns`, `GetDesign`, `GetFilled`, `Draw`, `DrawDisabled`, `GetBitmap` for `DentalIcon`; `GetProcButtonPicture` (linkable: `System`, `System.Drawing`) |
| `OpenPmsDental/UI/Direct2d/Direct2d.cs` | `DrawIcon(g, IconDesign, ...)`: any design the way `DrawGlyph` draws a glyph (Direct2D into a bitmap; GDI+ with `NoD2D.txt`); `DrawGlyph` calls it |
| `tools/DevDbTool/InitSqlScrub.cs` | `SetProcButtonPictures`: the scratch database the init file is made from gets the new pictures (UQ-7) |
| `db/init/new-database.sql` | the 8 `procbutton.ButtonImage` values, edited in place (not regenerated): exactly what DevDbTool writes |
| `tests/UiRefreshTests/DentalIconTests.cs`, `Data/procbutton-pictures-before.tsv` | section 5 |
| `tests/UiRefreshTests/IconRendererTests.cs` | `UiIcons.Dental.cs` in the linkable-file list; the helpers UI-7b also made `internal` (the same lines, the same change) |
| `tests/InitSqlTests/InitSqlFileTests.cs`, `InitSqlDatabaseTests.cs` | the seeded pictures are 20x20 bitmaps, in the file and in the loaded database |
| `icons/README.md` | "The custom icons": originality, style, the enum, how to change one, the procedure buttons, the mapping table |

## 3. The icons

- **First step of 4.4 (Fluent's Dentist).** It is a plain molar, but it has no 32 px design (12, 16, 20, 24, 28, 48 only), and the module
  bar's 32 px slot needs one (UI-7b's rule for the module icons). So the tooth is drawn here too, at every size, in the same family as the
  other dental icons; nothing of Fluent's Dentist is used.
- **Which** (4.4): `Tooth` (Chart module; `ToothFilled` is the selected, "hot", state that `Chart32W` shows today), `ChartMedical` (the
  medical layout's `ChartMed32`: a chart card with a medical cross), `PerioChart`, `PerioProbe`, `XrayBitewing`, `XrayPeriapical`,
  `XrayPano`, `XrayCeph`, `Sensor`, `PhosphorPlate`, `Mount`, `IntraoralPhoto`, `Implant`, `Ortho`, `ProcAmalgam`, `ProcComposite`,
  `ProcCrownPfm`, `ProcCrownCeramic`, `ProcRct`, `ProcRctBuildUp`; each with its `...Filled` twin.
- **Style** (4.4): designed on a 24 unit square and drawn at each size with Fluent's Regular stroke weight for that size, measured on the
  vendored SVGs (16 and 20 px: 1 px; 24: 1.5; 32: 2; 48: 2.5), round caps and joins, frames' straight edges on whole pixels (the stroke's
  outer edge snapped), one colour. Filled: the Regular outline's outer edge filled, the inner details cut out as gaps (as Fluent's Filled
  icons do). The 16 px designs drop what does not fit (the probe's markings, the second bitewing separator, a film's inner tooth becomes a
  solid one, the mount has 2x2 films). 48 px is drawn besides 4.4's 16 to 32 because the module bar at 150% and above asks for it (3.5).
- **How they are drawn.** WPF's `Geometry.Combine` unions the widened strokes and fills and excludes the cuts; its outline is a fine
  polyline, which a curve fit (Schneider, Graphics Gems 1990) turns back into lines and cubic curves within 0.02 px, so an SVG is about as
  long as a Fluent one (1.7 KB on average). The tool checks every design: inside its square, the same fill with the nonzero and even-odd
  rules (the generator reads nonzero), and the fitted outline within the error of the shape. Deterministic: `-Check` passes on this PC.
- **Originality.** Shapes from public dental anatomy only (a molar's two cusps and two roots, an incisor's single root); nothing from Open
  Dental's pictures, IconOrganizer art, Fluent or any other set. Every SVG carries the comment "Open PMS - Dental custom icon, original
  artwork, GPL-2.0-or-later"; `icons/README.md` says so.

## 4. The default procedure buttons (UQ-7)

- `db/init/new-database.sql` is made by `tools/New-InitSql.ps1` (DevDbTool), which runs `ProcButtons.SetToDefault` on a scratch database.
  Editing the file alone would be undone by the next regeneration, so DevDbTool's scrub now sets the 8 pictures
  (`InitSqlScrub.SetProcButtonPictures`, each must be exactly one row with a picture or the export stops), and the file was edited in place
  to the same bytes (the coordinator regenerates it once after the schema packages; the result is the same).
- The picture: `UiIcons.GetProcButtonPicture(icon)`, the icon's 20 px design in the light look's `Text` colour (#1F1F1F) on white, written as
  the same bitmap file the table holds today (54 byte header identical to Open Dental's pictures: 20x20, 32 bits, BI_RGB, 96 DPI; bottom-up;
  opaque). RCT BU PFM and RCT BU Ceramic share `ProcRctBuildUp`, as they share one picture today; PA shows `XrayPeriapical`. Today Crown-PFM
  and Crown-Ceramic share a picture; now they differ (a metal collar line, a shine).
- No office row: `ProcButtons.SetToDefault` (the procedure code tools' "Procedure Buttons" reset, which deletes and re-adds an office's
  buttons when asked) keeps Open Dental's pictures; no fork migration step or convert script touches `procbutton`; the new pictures appear in
  no source file but the init file, and only DevDbTool calls `GetProcButtonPicture` (section 5's test).

## 5. Tests

`tests/UiRefreshTests/DentalIconTests.cs` (UiRefreshTests; off screen, no window, no database):

- `Manifest_EveryIconOf44HasARegularRowThenItsFilledTwin_AndTheEnumFollowsTheRows`: the 20 icons of 4.4, each a custom Regular row followed
  by its Filled twin, sizes 16 to 48, role `Text`; `DentalIcon` is `None` then the rows; `GetFilled`; every file of `icons/custom` in a row.
- `Svgs_AreTheDrawingsOutput_OriginalWork_OnePathEach`: ASCII, one line, the header, the originality comment, one path of absolute commands;
  `tools\New-DentalIcons.ps1 -Check`.
- `Readme_SaysTheCustomIconsAreOriginal`.
- `Gdi_EveryIconDrawsAtEverySlot_TheDesignForTheSlot_FilledHasMoreInk`: slots 12 to 64 pick the design of 3.5; non-empty, not filling the
  slot, nothing outside the drawn square; the Filled twin has more ink at every size; `DrawDisabled` is `TextDisabled`.
- `Direct2d_EveryIconDrawsAtEverySlot_AsGdiDoes_AndTheOnlyGdiPathDrawsTheSame`: the wrapper's `FillIcon` against GDI+, and
  `Direct2d.DrawIcon` with `OnlyGDI` false (still false afterwards) and true (NoD2D.txt): the same shape.
- `NoDpi_EveryIconDrawsTheSamePixels`: DPI-unaware and per-monitor aware, GDI+ and Direct2D, the same pixels.
- `Wpf_EveryIconHasItsGeometryAtEverySlot_AndRendersAsGdiDraws`: frozen geometry inside the slot, `FillContains` agrees with GDI+; the
  pixels where WPF renders.
- `SeededProcButtonPictures_Are20x20Bitmaps_OfTheDentalIcons_TheOtherRowsUnchanged`: the 8 rows, Open Dental's header, opaque, 20x20, the
  icon's shape, and on this PC the exact bytes of `GetProcButtonPicture`; no other row has a picture.
- `NoCodePathGivesTheNewPicturesToAnOfficesRows`: section 4's three points.
- `DentalIconPictures_ForTheGallery`: draws every design each run; with `OPMS_UI8_ICON_PICTURES` set it writes gallery G-UI1 part 4's
  pictures (section 7).

`IconRendererTests` draws every manifest row (these 40 too) on GDI+, Direct2D and WPF and checks the table against the SVGs.
`tests/InitSqlTests`: `File_DefaultProcButtonPicturesAre20x20Bitmaps` and `Rows_DefaultProcButtonPicturesAre20x20Bitmaps` (the loaded scratch
database).

Runs (x64, `-RequireBuildOutput`, after the full build; `.git/od-tools/test-results/ui8-2.txt`): UiRefreshTests 90 of 91 (the opt-in font scan
skipped, as before; the Direct2D tests ran with the built wrapper and WPF rendered in this session, so nothing was inconclusive), InitSqlTests
21 of 21, NoHqTests 122 of 122. `ui8-1.txt` is the run that wrote the gallery's pictures before the init file was edited (the two picture
tests failed there, as they must). ForkMigrationTests was not run (the coordinator's, after regenerating the init file).

## 6. For the lead

- **The chart pictures (follow-up after UI-7b merges).** `IconLibrary` is UI-7b's file, so the module bar still draws `Chart32G`,
  `Chart32W` and `ChartMed32` from `Gen_*.cs`. To switch: in both `IconLibrary` copies map `Chart32G` to `DentalIcon.Tooth`, `Chart32W` to
  `DentalIcon.ToothFilled` and `ChartMed32` to `DentalIcon.ChartMedical` (Filled: `ChartMedicalFilled`) through
  `UiIcons.GetDesigns(DentalIcon)` (or `UiIcons.GetDesigns("DentalIcon", name)`), make `IsPicture` false for them, delete
  `Gen_Chart32G.cs`, `Gen_Chart32W.cs`, `Gen_ChartMed32.cs` and `IconSelector.cs`, and drop `_arrayPictures` in UI-7b's
  `IconLibraryTests` (its `GenFiles_GoneButThePictures` expects the three files). UI-7b's tests also forbid `EnumIcons` rows for these three,
  which is why the dental icons are their own enum, not `EnumIcons` rows.
- **Merging.** `icons/manifest.json` (both packages append rows at the end) and `CodeBase/UI/Icons/IconData.g.cs` will conflict with UI-7b:
  keep both blocks of rows (UI-7b's `EnumIcons` rows, then this `DentalIcon` block), then rerun
  `powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-IconData.ps1` (deterministic). `icons/README.md`: UI-7b changes the field
  list and appends mapping rows; this package adds a section before "A new version" and changes the `icons/custom/` line of the files table.
  `tests/UiRefreshTests/IconRendererTests.cs`: the same seven `private` to `internal` changes as UI-7b's (identical hunks).
- **The init file.** When you regenerate `db/init/new-database.sql` (`tools/New-InitSql.ps1`), DevDbTool writes the same 8 pictures; then
  `SeededProcButtonPictures_...` and InitSqlTests pass unchanged. InitSqlTests' picture tests were run here; ForkMigrationTests
  (`InitSqlRegenerationTests`) was not (your run).
- **Gallery G-UI1 part 4.** Rendered off screen here (GDI+, no window needed): `.git/od-tools/test-results/G-UI1-part4-ui8/` (`index.html` and the zip; 221 pictures: 51 beside today's picture, the rest new; the
plain-words summary at the top), made from `.git/od-tools/test-results/ui8-pictures/` (`before/`, `after/`, `procbutton-pictures.tsv`). To make it again:

  ```
  set OPMS_UI8_ICON_PICTURES=<folder>
  powershell -NoProfile -ExecutionPolicy Bypass -File .git\od-tools\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests
  powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before <folder>\before -After <folder>\after -Out .git\od-tools\test-results\G-UI1-part4-ui8 -Name G-UI1 -Note "<the summary>"
  ```

  Nothing of this gallery needs a shown window or WPF rendering. The module bar with the new chart icons is G-UI2's (UI-6, after the
  follow-up above).

## 7. Questions for the owner

- **"Set to default" for procedure buttons** (Setup, procedure code tools): it deletes an office's buttons and adds the defaults back with
  Open Dental's pictures, which this package left alone (UQ-7 says new databases only). Should that reset give the new pictures too? It
  changes only an office that asks for the reset.
- G-UI1 part 4: approve or change each icon (the gallery page).
