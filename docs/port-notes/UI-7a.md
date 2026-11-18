# Port note: UI-7a (the icon pipeline and the control glyphs)

Package UI-7a of the UI refresh ([docs/specs/ui-refresh-plan.md](../specs/ui-refresh-plan.md), section 7's UI-7a row, sections 3.5, 4.1 and
4.2), on `ui/ui7a-1` from `main` at `3809f35db`. No visible change: nothing in the program draws with the new code yet; the control packages
(UI-3 to UI-6) switch their glyphs to it, and UI-7b moves the `EnumIcons` onto the same table. `EnumIcons`, both `IconLibrary` folders and the
`Gen_*.cs` files are unchanged. No schema change, no NuGet package, no program started, no database used, no window shown.

## 1. The download

- **Newest version on 2026-11-17:** `@fluentui/svg-icons` **1.1.346**, the npm registry's `latest` (published 2026-11-13T20:05:48Z; no newer
  version). The repository (https://github.com/microsoft/fluentui-system-icons) publishes no GitHub Releases (the releases API returns an
  empty list), so the npm package is the official release, as section 4.1 says. Its `gitHead` is `ba34950eeb56d33045e4832121c68cae679e1c85`;
  the release commit `67e6dec1ee956af3ab4c746a2b3bb6a2e5520a44` ("publish 1.1.346", the plan's `67e6dec1e`) is its child. The repository's
  main was `41089846a` (2026-11-16).
- **Where:** `.git/od-tools/downloads/ui7a-fluentui-svg-icons-20261117/` (common git folder): `svg-icons-1.1.346.tgz` (its SHA-1 and SHA-512
  equal the registry's `shasum` and `integrity`), `LICENSE` (the repository's at the package's commit: the npm package has none, its
  `package.json` says MIT), `extracted/` (the tarball unpacked), `SOURCES.txt`, `SHA256SUMS.txt`, `MANIFEST.txt`.
- **The layout found** (section 4.1's "could not determine"): `package/icons/<name>_<size>_<style>.svg`, one folder of 20,901 SVGs (styles
  regular, filled, light and color) and 36 sub folders (`LTR`, `RTL` and one per language) with 946 direction- and language-specific
  variants, plus `README.md`, `CHANGELOG.md` and `package.json`; 21,850 files in all. Every SVG is `<svg viewBox="0 0 N N"><path d/></svg>`
  except 49 regular and filled ones with a fill, opacity or transform and 5 with groups and clip paths; none uses the even-odd rule.
- **Vendored:** `third-party/fluentui-system-icons/`: the 53 SVGs the manifest names, unchanged, in the package's layout (`icons/`), the MIT
  `LICENSE` and `SOURCE.txt` (version, download URL, integrity hash, commits, the layout, each file's SHA-256). `.gitattributes` keeps the
  folder byte for byte (`-text`), so the hashes hold in every checkout.

## 2. Files

| File | What |
|---|---|
| `icons/manifest.json` | the ten `UiGlyph` rows: enum, name, Fluent stem (or `custom`), style, design sizes, colour role, purpose |
| `icons/README.md` | the manifest's fields, how to add an icon or take a new version, the drawing calls, the mapping table (4.3) for UI-7b, UI-8, UI-9 |
| `tools/New-IconData.ps1` | the generator (ASCII, Windows PowerShell 5.1); `-Check` compares instead of writing |
| `CodeBase/UI/Icons/IconData.g.cs` | generated: `UiGlyph` (`None` and the ten glyphs), `FLUENT_VERSION`, `MANIFEST_SHA256`, the 53 designs |
| `CodeBase/UI/Icons/UiIcons.cs` | `IconDesign`, `IconFigures` (the parsed commands), `IIconPathSink`, the design for a slot, `GetDrawRectangle`, `WriteFigures` |
| `CodeBase/UI/Icons/UiIcons.Gdi.cs` | GDI+: `Draw`, `DrawDisabled`, `GetPath`, `GetBitmap` (per-thread path cache) |
| `CodeBase/UI/Icons/UiIcons.ImageList.cs` | `GetImageList` |
| `CodeBase/UI/Icons/UiIcons.Wpf.cs` | WPF: `GetGeometryWpf` (frozen `StreamGeometry`, cached), `DrawWpf`, `GetImageSourceWpf` |
| `OpenPmsDental/UI/Direct2d/Direct2d.cs` | new region "Fluent glyphs": `FillGlyph`, `FillIcon`, `DrawGlyph` and the private `GlyphPathSink` |
| `THIRD-PARTY-NOTICES.txt` | the component "Fluent UI System Icons" (1.1.346, MIT, Microsoft), which the About box's license list shows |
| `tests/UiRefreshTests/IconRendererTests.cs` | section 5 |
| `tests/AttributionTests/FluentIconsNoticeTests.cs`, `ReleaseIdentityTests.cs` | the notices (section 5) |

## 3. The pipeline

- **The glyphs** (section 7's row): caption buttons `Subtract`, `Maximize`, `SquareMultiple`, `Dismiss`; `ChevronUp`, `ChevronDown`,
  `ChevronLeft`, `ChevronRight`; `Checkmark`; `Dash`, all regular. The dash is Fluent's `line_horizontal_1` (a 12 px wide bar in the 16 px
  design, for the indeterminate check box of 3.6), not `subtract` (10 px), which is the minimize button's; both rows exist, one per purpose.
  Sizes are every design of 12, 16, 20, 24, 32 and 48 that Fluent has: `Maximize` has no 12 and no 32, `Subtract` no 32, `SquareMultiple`
  no 12, `Dash` only 16, 20 and 24. 28 is not compiled in, since the 24 design serves 24 to 31 px (3.5).
- **The generator** turns each `<path d>` into absolute M, L, C and Z commands in design pixels: H and V become lines, S a cubic with the
  reflected control point, Q and T cubics, A cubics of at most 90 degrees (SVG 1.1, F.6.5; packed arc flags like `a.5.5 0 01.5.5` read
  right). Every figure is closed (filling closes them anyway). Numbers are rounded to 4 decimals, invariant. The file is ASCII, CRLF, no BOM,
  deterministic. The colour role is the manifest's; caption, check mark and dash rows have the role `Text`, and the control passes the
  colour it needs (`TitleBarText`, `TextOnAccent`).
- **The design for a slot** (3.5): the largest design of the icon no larger than `UiTheme.GetIconDesignSize(slot)`, centred on whole pixels
  (`GetDrawRectangle`, as `UiTheme.GetIconRectangle`); never stretched up. A missing design falls back to the largest smaller one (Maximize
  at 32 px draws its 24 design); a slot below the smallest design (Dash at 12 px) draws the smallest shrunk to the slot.
- **GDI+:** a `GraphicsPath` (`FillMode.Winding`, the SVG's nonzero rule), filled with `SmoothingMode.AntiAlias` and
  `PixelOffsetMode.Half` (pixel centres at +0.5, as in the SVG, Direct2D and WPF; UI-1's note found the same for 1 px lines). Paths are cached
  per thread by design and drawn size ([ThreadStatic], as UiTheme's pens; GDI+ objects are not thread-safe), at (0,0), and translated at
  draw time; the Graphics' own settings are restored. `GetBitmap` makes a new 32-bit ARGB bitmap with 96 DPI in its header (so it draws 1:1
  wherever a designer Image goes), on transparent; the caller owns it. `GetImageList` adds one per glyph to a 32-bit `ImageList`.
  Disabled is `TextDisabled` (4.2, item 6).
- **Direct2D:** CodeBase references nothing of the program, so the figures go through `IIconPathSink`; `Direct2d.FillGlyph`/`FillIcon` feed
  them to the wrapper's own `BeginPath`, `BeginFigure`, `AddLine`, `AddBezier`, `EndFigure` and `EndPath` (fill mode winding in both C++
  wrappers). `Direct2d.DrawGlyph(g, ...)` draws on a GDI+ `Graphics` the way `IconLibrary.Draw` draws its vector icons: Direct2D into a bitmap
  over the background colour (the bitmap target has no transparency), copied pixel for pixel; with `IconLibrary.OnlyGDI` (NoD2D.txt) it fills
  with GDI+, and a Direct2D that cannot start sets `OnlyGDI` from then on, as `IconLibrary.Draw` does.
- **WPF:** a frozen `StreamGeometry` (`FillRule.Nonzero`), chosen for the slot's real pixel size `round(sizeSlot*scale)` and placed on whole
  device pixels, in DIPs; cached for every thread (frozen). `DrawWpf` and `GetImageSourceWpf` (a frozen `DrawingImage` that keeps the slot).
- **Linkable for RdlDesign** (UI-9a-7): `IconData.g.cs` uses only `System`; `UiIcons.cs` and `UiIcons.Gdi.cs` only `System` and
  `System.Drawing` (and `UiTheme`'s linkable files); `UiIcons.ImageList.cs` Windows Forms besides; all in C# 7.3. The WPF part is its own
  file. `LinkableFiles_UseOnlySystemDrawingAndWinForms` keeps it so.

## 4. Decisions made here

- `UiGlyph` is generated from the manifest (its rows in manifest order after `None`), so the enum and the table cannot disagree. `EnumIcons`
  rows (UI-7b) are looked up by name (`UiIcons.GetDesigns("EnumIcons", name)`), since `EnumIcons` lives in the program, not in CodeBase.
- The renderer class is `CodeBase.UiIcons`, beside `UiTheme`; `IconLibrary` keeps its name and signatures for UI-7b, which routes its calls
  to the table (4.2, item 4).
- The path data is a string of space-separated tokens read once per design (a small culture-free parser), not float arrays: smaller,
  readable in a diff, and the generator stays simple.
- The OnlyGDI switch for glyphs lives in `Direct2d.DrawGlyph`, the one place that chooses between the two; `UiIcons.Draw` is always GDI+.

## 5. Tests

`tests/UiRefreshTests/IconRendererTests.cs` (UiRefreshTests; all off screen, no window):

- `Gdi_EveryManifestRowDrawsAtEverySize`: every manifest row at 16, 20, 22, 24, 32 and 48 px: non-empty, not filling the slot, nothing outside
  the drawn square. `Gdi_GlyphApiDrawsTheDesign_AndKeepsTheGraphicsSettings`, `Gdi_GetBitmapScales_AndGetImageListHoldsEveryGlyph`,
  `Gdi_DrawsOnTwoThreadsAtOnce`.
- `Direct2d_EveryManifestRowDrawsAtEverySize_AsGdiDoes`: the program's wrapper (`Direct2d.FromBitmap`, `FillIcon`) at every size, the same
  shape as GDI+. `Direct2d_OnlyGdiPath_DrawsTheSameGlyphs`: `Direct2d.DrawGlyph` with `OnlyGDI` false (and still false afterwards, so Direct2D
  did draw) and true give the same shape. Both need `Direct2dWrapper.dll` (built by the full build; the test loads it from the program's or
  the wrapper's output folder) and are inconclusive without it.
- `NoDpi_DrawsTheSameGlyphs`: every glyph at every size, the thread DPI-unaware (as `Dpi.SetUnaware` makes the program with NoDpi.txt) and
  per-monitor aware: the same pixels, on GDI+ and on Direct2D.
- `Wpf_EveryManifestRowHasItsGeometryAtEverySize_AsGdiDraws` (geometry only, any session): frozen, non-empty, inside the slot, cached, and
  `FillContains` agrees with GDI+ at the pixel centres GDI+ covers fully or not at all; 150% picks the 24 design for a 16 DIP slot.
  `Wpf_EveryManifestRowRendersAtEverySize_AsGdiDraws`: `RenderTargetBitmap` of a `DrawingVisual`, the same shape as GDI+; inconclusive when
  WPF draws nothing (a filled square stays blank). **In this disconnected session it rendered and passed**: WPF's software renderer draws a
  `DrawingVisual` into a `RenderTargetBitmap` here, unlike the `HwndSource`-hosted Frm of UI-1, so nothing waits for a connected run.
- `Generator_Check_TheCommittedFileIsTheOutputOfTheManifestAndTheSvgs` (runs `tools\New-IconData.ps1 -Check`),
  `Table_HoldsEveryManifestRowAndSize_InOrder_WithTheSvgsHashes` (every row and size in order, file names, each SVG's SHA-256 in the table and
  in `SOURCE.txt`, viewBox, the figures inside the design square, no vendored file outside the manifest, the manifest's hash, the version),
  `UiGlyph_IsNoneThenTheManifestsUiGlyphRows_AndHoldsTheRowsGlyphSubset`, `Generator_ConvertsArcsLinesAndRelativeCommands_AndRefusesAnUnlistedSvg`
  (a scratch root in %TEMP%: H/V/relative lines exactly, two circles from packed-flag arcs within 0.02 px, `-Check` passes and then fails
  on a changed file, an unlisted SVG is refused), `Design_IsTheLargestThatFits_NeverStretched_Centred`,
  `LinkableFiles_UseOnlySystemDrawingAndWinForms`.

`tests/AttributionTests`: `FluentIconsNoticeTests` (the vendored MIT text with Microsoft's copyright line; `SOURCE.txt` names the version,
download, integrity and repository; THIRD-PARTY-NOTICES.txt's component with the table's `FLUENT_VERSION`, copyright, MIT, source and
files; the About box's license list entry with the version and the MIT text); `ReleaseIdentityTests` lists the component among the shipped
ones. Build-Release's checks are unaffected: nothing new ships as a file (the path data is in `CodeBase.dll`), and the source archive
(`git archive`) carries `third-party/fluentui-system-icons` with its licence.

Window checks: none. Every drawing is off screen (bitmaps, render targets); no test needs a shown window.

Found on the way: `ImageList.Images.Add(bitmap)` keeps the bitmap itself and reads it again whenever the list (re)creates its handle, so
a bitmap disposed after `Add` throws "Parameter is not valid" on the first `Images[i]`. `GetImageList` leaves its bitmaps to the list;
`Gdi_GetBitmapScales_AndGetImageListHoldsEveryGlyph` reads every image back.

Runs (x64, `-RequireBuildOutput`, after the full build; results in `.git/od-tools/test-results/ui7a-3.txt`): UiRefreshTests 57 of 58 (the
opt-in font scan skipped, as before), AttributionTests 54 of 54, ReleaseFlowTests 260 of 267 (7 inconclusive: Release-build and other-configuration checks, which a Debug build does not make), NoHqTests 122 of
122. Full filter x64: 0 errors. `tools/Test-NoEolFlips.ps1 -Base main`: no file changed its line ends. InitSqlTests and ForkMigrationTests
were not run (no schema change).

## 6. For the next packages

- UI-3 (caption buttons): `UiIcons.Draw(g, UiGlyph.Subtract/Maximize/SquareMultiple/Dismiss, slot, UiTheme.GetColor(EnumUiColor.TitleBarText))`.
- UI-4a (check box): `Checkmark` and `Dash` in `TextOnAccent` on the accent fill; UI-4a to UI-6: the chevrons (`UiTheme.SIZE_CHEVRON` is
  12, which has a 12 design for every chevron).
- UI-7b: add the `EnumIcons` rows (enum `EnumIcons`, its member names), route `IconLibrary.Draw`, `DrawDisabled` and `DrawWpf` to
  `UiIcons.GetDesigns("EnumIcons", name)`, `Direct2d.FillIcon` and `UiIcons.GetGeometryWpf(IconDesign, ...)`, then delete the `Gen_*.cs`.
- UI-9a-7 (RdlDesign): link `IconData.g.cs`, `UiIcons.cs`, `UiIcons.Gdi.cs` (and `UiIcons.ImageList.cs` if needed) with UiTheme's four files.
- The design sheet's stand-in glyphs (`DesignSheetPainter`, "stand-ins until the Fluent icons of UI-7a") are G-UI1 part 1's approved
  pictures; G-UI1 part 2 draws the controls with these glyphs.
