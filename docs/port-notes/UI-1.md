# Port note: UI-1 (the tokens, the design sheet and the coverage test)

Package UI-1 of the UI refresh ([docs/specs/ui-refresh-plan.md](../specs/ui-refresh-plan.md), section 7's UI-1 row, sections 3, 8 and 9, the
owner's answers UQ-1 to UQ-4 of 2026-11-17), on `ui/ui1-1` from `main` at `117b0edd0`. No visible change: the tokens are defined and nothing in
the program draws with them yet. `ColorOD` is not touched. No schema change, no new package, nothing downloaded, no program started, no
database used, no window shown.

## 1. Files

| File | What |
|---|---|
| `CodeBase/UI/UiTheme.cs` | the class, `IsHighContrast` (cached, re-read on `SystemEvents.UserPreferenceChanged`), `InitWinForms()` (empty until UI-6) |
| `CodeBase/UI/UiTheme.Colors.cs` | `EnumUiColor`, the light values, the high-contrast partners, `GetColor`, `GetColorLight`, `GetHex`, a property per token |
| `CodeBase/UI/UiTheme.Gdi.cs` | `GetPen`, `GetBrush`: per-thread (`[ThreadStatic]`) cached GDI+ pens and brushes that callers never dispose |
| `CodeBase/UI/UiTheme.Metrics.cs` | the design's font family and sizes, corners, lines, spacing, icon sizes, the pixel-snapping helpers of 3.3 |
| `CodeBase/UI/UiTheme.Wpf.cs` | `GetBrushWpf`, `GetBrushWpfLight` (frozen), `GetColorWpf`, `GetSystemBrushWpf` |
| `tests/UiRefreshTests/DesignSheet*.cs` | the sheet (`Painter`, `Controls`, `Pages`, `Windows`), its tests, the contrast and the two-thread paint tests |
| `tests/UiRefreshTests/Data/coverage.tsv` | section 2.1's table |
| `tests/UiRefreshTests/Data/colour-literals.tsv` | the allow list of the colour-literal scan (323 rows) |
| `NoHqTests/UiThemeScanTests.cs` | the NoHq source scans: coverage, colour literals, no `using`/`Dispose` around token pens, the linkable files |
| `tests/AttributionTests/BrandingTests.cs` | section 9's UI-1 row: the `Accent` token is the brand blue `#3C5A96` (the GridOD literal check stays until UI-5) |

The four WinForms files use only Windows Forms, `System.Drawing` and `Microsoft.Win32`, nothing else of CodeBase, in C# 7.3, so RdlDesign can
compile them in as linked files (UI-6 links them with the renderer); `UiThemeWinFormsFiles_UseOnlyWinFormsAndDrawing` keeps it so. The WPF part
is the fifth file.

## 2. The tokens and their values

Colours (section 3.2), in `EnumUiColor` order; "HC" is the `SystemColors` partner while Windows runs a contrast theme:

| Token | Value | HC | Token | Value | HC |
|---|---|---|---|---|---|
| `Surface` | `#FAFAFA` | Window | `TextDisabled` | `#A0A0A0` | GrayText |
| `SurfaceRaised` | `#FFFFFF` | Window | `Accent` | `#3C5A96` | Highlight |
| `SurfaceSubtle` | `#F3F3F3` | Window | `AccentHover` | `#324C80` | Highlight |
| `SubtleHover` | `#EBEBEB` | Window | `AccentPressed` | `#2A406C` | Highlight |
| `SubtlePressed` | `#E0E0E0` | Window | `AccentSubtle` | `#DCE6F5` | Highlight |
| `ButtonHover` | `#F5F5F5` | Window | `RowHover` | `#F0F4FA` | Window |
| `ButtonPressed` | `#EBEBEB` | Window | `CaptionHover` | `#5370A6` | Highlight |
| `Stroke` | `#D1D1D1` | WindowFrame | `Danger` | `#C42B1C` | Highlight |
| `StrokeStrong` | `#8A8A8A` | WindowFrame | `DangerSubtle` | `#FDE7E9` | Window |
| `StrokeStrongHover` | `#616161` | HotTrack | `Warning` | `#9D5D00` | Highlight |
| `Gridline` | `#E0E0E0` | GrayText | `WarningSubtle` | `#FFF4CE` | Window |
| `Text` | `#1F1F1F` | WindowText | `Success` | `#0F7B0F` | Highlight |
| `TextSecondary` | `#5C5C5C` | WindowText | `TextOnAccent` | `#FFFFFF` | HighlightText |
| `TextSelected` | `#1F1F1F` | HighlightText | `TitleBar` | `#3C5A96` | ActiveCaption |
| `TitleBarText` | `#FFFFFF` | ActiveCaptionText | `TitleBarOutline` | `#2A406C` | WindowFrame |

- The last five are role names for colours section 3.2 gives by use (white on the accent and on `Danger`; text on a selection; the title bar,
  its text and outline). In the light look they equal `#FFFFFF`, `Text`, `Accent`, white and `AccentPressed`; they exist so each has the right
  high-contrast partner (text on the highlight must be the highlight text, not the window text). A control package draws text on
  `AccentSubtle` with `TextSelected` and text or glyphs on an accent fill with `TextOnAccent`. Misc Colors 11 to 13 override `TitleBar`,
  `TitleBarOutline` and `TitleBarText` item by item from UI-3 on (section 6).
- The disabled button fill is `ButtonHover`'s `#F5F5F5` (section 3.2: no extra token).
- High contrast, decided where section 3.2 names no partner: hover and pressed fills take the window colour (the hover outline,
  `StrokeStrongHover`, takes the hot-track colour, so hover still shows), `Gridline` the grey text, every role colour (`Danger`, `Warning`,
  `Success`, `CaptionHover`) the highlight with the highlight text on it.
- `GetPen` and `GetBrush` hand out this thread's pen or brush of the token's current colour (keyed by token and width, 1 to 16 px). A colour
  change (a theme switch) makes a new one on the next call; the old one is left to the garbage collector, since a paint on the same thread may
  still hold it.

Sizes (sections 3.3 to 3.5): `FONT_FAMILY` Segoe UI, `FONT_FAMILY_SEMIBOLD`, `FONT_SIZE_BODY` 9 (UQ-1) and `FONT_SIZE_BODY_COMPACT` 8.25,
`FONT_FACTOR_LABEL` 0.96 (kept, UQ-1), `FONT_SIZE_TITLE` 10, `FONT_SIZE_MODULE_CAPTION` 8, `FONT_SIZE_WPF_BODY` 11.5 and `_TITLE` 14 DIP,
`GRID_ROW_HEIGHT` 17 (UQ-2) and `_COMPACT` 16; corners 4 (controls), 2 (check box), 6 (containers and module hover), 2 (menu item), 0; lines 1
(border), 2 (focus, selection bar, tab line, combo focus line), 1 (white inner ring), 3 (module mark); spacing 2, 4, 8, 12, icon gap 4, button
text padding 8, menu icon column 16, separator inset 4, module hover inset 4, chevron 12; icon designs 12, 16, 20, 24, 28, 32, 48 with
`GetIconDesignSize` (largest that fits, 24 for the 24-31 px slots, never stretched) and `GetIconRectangle`. These are the design's values;
the font role tokens of section 3.4 (`FontBody` and the rest, holding today's fonts until UI-2b) are UI-2a-1's.

Pixel snapping (section 3.3): `ScaleI`, `GetLineWidth` (max(1, floor(scale))), `GetFocusWidth` (round(2 x scale)), `GetCornerRadius`,
`SnapRectangle` (each edge rounded on its own), `ScaleRectangle`, `GetStrokeRectangle` (inset by half the width) and `GetRoundedRectanglePath`.
Found on the way: GDI+ puts pixel centres on whole numbers in its default `PixelOffsetMode`, so a 1 px line at +0.5 falls between two pixels
there. The rule's "on the pixel centre (offset 0.5)" holds with `PixelOffsetMode.Half`; the helper says so, and the sheet draws that way.

## 3. The design sheet and the gallery

`DesignSheetTests` renders, off screen, into `OPMS_UI_SHEET_OUT` (default `design-sheet` in the test's output folder; a later run empties it
only when it holds `design-sheet.txt`):

- `after/sheet/`: `G-UI1-01-colours` (swatches, hex, the main ratio, the partner), `-02-contrast` (every pair with its ratio in the light look
  and the four contrast themes), `-03-type` (the ramp and grids at 17, 16 and 14 px rows), `-04-sizes` (spacing, corners, lines magnified,
  icon slots, the snapping table), `-10-controls-100` to `-200` (15 control families in 8 states, then a window, the floating image windows,
  a toolbar with its error and notify states, grids with and without focus, a menu, the module bar, tabs, a calendar and a progress bar),
  `-20-options` (UQ-1 to UQ-4 side by side, the chosen one first; Misc Colors kept item by item) and `-30-high-contrast` (the same window in the
  light look and Dusk, Night sky, Aquatic and Desert). "-" in a state cell: the plan gives that state no look of its own. Icons are stand-in
  shapes until UI-7a.
- `before/<option>/` and `after/<option>/` for `windows-9pt-17px` (UQ-1 a, UQ-2 a) and `windows-8.25pt-16px`: `FormPatientEdit`,
  `FormProcEdit`, `ControlAccount` and `ControlAccount-toolbar-and-grid`, built with UI-0b's builder (no constructor, no database, never shown),
  in today's font (before) and with the test-side font swap plus GridOD's cell, header and title fonts set as UI-2a/UI-2b will (after). The
  Account module's toolbar gets its captions and pictures (ControlAccount.LayoutToolBar's) and its account grid made-up rows; today's colours
  and controls. Measured rows: 14 px today, 17 px at Segoe UI 9, 16 px at 8.25 (also `GridFonts_RowsFollowTheOption`).
- `after/windows-wpf/FrmProviderPick.png`: the WPF Frm (already Segoe UI 11.5), rendered with `RenderTargetBitmap` in a hidden `HwndSource`.
  **Waits for a connected session:** WPF draws nothing in this disconnected session, not even off screen (the bitmap stays transparent);
  the test notes it and does not fail. Run the test once in the owner's connected session and rebuild the gallery to add it.
- `notes-sheet.txt`, `notes-windows.txt`: what was drawn, the row heights, what waits.

Two things learned on the way, both handled in `DesignSheetWindows.cs` and `DesignSheetPainter.cs`: Windows prints a window's children only
while the window is visible, so each control is printed on its own (WM_PRINT through `DrawToBitmap`) and its children drawn over it, back to
front, clipped to its client area; and the Windows edit control of a never-shown window prints a black client on .NET 10, so a text box's
border, background and text are painted in its own font (the same in before and after). The sheet's own text uses GDI+ with ClearType, since
this session draws GDI text into bitmaps without smoothing; the real windows' text came out smoothed here (the test checks and would note it).

**The gallery** (G-UI1 part 1, not in the repository): `D:\Dev\opendental\.git\od-tools\test-results\g-ui1-part1\` (`index.html`, the zip
beside it, the notes), 19 pictures: 11 sheet pages (only "after"), 8 window pictures beside today's. Rebuild it:

```
set OPMS_UI_SHEET_OUT=<an empty folder>
powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests
powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before <folder>\before -After <folder>\after -Out <od-tools>\test-results\g-ui1-part1 -Name G-UI1 -Note "<the summary>"
```

## 4. The tests

- `DesignSheetTokenTests`: every pair of sections 3.2 and 3.6 (23 text, 7 outline and icon, 4 focus, 4 selection) meets 4.5:1 or 3:1 in the
  light look, and in the four Windows 11 contrast themes through the mapping (the themes' colours are those of
  `%WINDIR%\Resources\Ease of Access Themes\hc1, hc2, hcblack, hcwhite.theme`, checked against the files on this PC); the 32 ratios the plan
  states match the tokens; WCAG known values; every token has a partner in WinForms and WPF; the hex values; frozen shared WPF brushes; the font
  sizes (Segoe UI 9 is 16 px high, rows 17; 8.25 gives 16); the snapping rule at 100 to 200%; icon designs; the size values; `InitWinForms`
  leaves the renderer alone.
- `DesignSheetPaintThreadTests`: two self-drawn test controls paint 3,000 times each with the token pens and brushes on two STA threads at
  once, without an exception; each thread has its own pens; the width limits.
- `DesignSheetTests`: the 11 pages render and are not blank (200% twice as wide as 100%); the 16 window pictures render, today's are equal for
  both options and each option differs from today; the grid rows follow the option; every family of 3.6 draws.
- `NoHqTests/UiThemeScanTests` (runs in every Targeted run): every file of `OpenPmsDental/UI`, `WpfControlsOD/UI` and `WpfControlsOD/Themes`
  matches exactly one row of `coverage.tsv` (designer, `.resx`, `.xaml.cs` files go with their main file), every row matches a file, each row
  names packages of section 7 or `keeps`/`no-drawing` with a note; no new `Color.FromArgb`, `FromRgb`, `FromScRgb` or hex colour in
  `OpenPmsDental/UI`, `OpenPmsDentalBusiness/UI/GridOD.cs` and `WpfControlsOD/UI/Controls` beyond `colour-literals.tsv` (per file and literal; a
  count that drops fails too, so each package cuts the list: `OPMS_UI_COLOUR_LITERALS_WRITE=1` writes it from the source); no `using` and no
  `Dispose` around `UiTheme.GetPen`/`GetBrush` anywhere in the repository (statements, declarations, direct calls, and variables assigned from
  them); the linkable files. Each rule has a self-test on samples. The coverage test is in NoHqTests (section 2.1 calls it a NoHq test), so a
  new control file fails in every package's run; section 9 lists it under `tests/UiRefreshTests`, whose `Data` folder holds its table.

Runs (x64, `-RequireBuildOutput`, results in `.git/od-tools/test-results/ui1-*.txt`): UiRefreshTests 41 of 42 (the opt-in full scan skipped),
AttributionTests 50 of 50, NoHqTests 122 of 122. Full filter x64: 0 errors.

## 5. Coverage table decisions

Section 2.1's rows, with two the table leaves open: `WpfControlsOD/UI/Controls/WebView2` is `keeps` (the HTML is out of scope; the window
around it is restyled with its window), and `WpfControlsOD/Themes/Generic` is `UI-4a,UI-4b` (the styles of both packages' controls).
`OpenPmsDental/UI/FormPrintPreview` and `ODPrintPreviewControl` are UI-4c; `OpenPmsDental/UI/IconLibrary/*` includes `IconSelector`.

## 6. For the owner

- The selection bar without focus (`StrokeStrong`, `#8A8A8A`) is 3.5:1 on the white row around it, as section 3.2 says, but 2.7:1 against the
  pale blue selection fill beside it. With focus the bar is the accent (5.4:1 on the fill), and in a contrast theme the fill itself is the
  highlight. It meets the plan's rule; a darker bar (`StrokeStrongHover`, `#616161`, 4.9:1 on the fill) is the alternative if the owner wants
  the unfocused selection to stand out more. The contrast page shows it.
- The WPF window of the real-window set waits for the next connected session (section 3).
