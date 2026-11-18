# Port note: UI-6 (toolbars, menus and the module bar)

Package UI-6 of the UI refresh ([docs/specs/ui-refresh-plan.md](../specs/ui-refresh-plan.md), section 7's UI-6 row, sections 3.2, 3.3 and
3.6), on `ui/ui6-1` from `main` at `2ae1982f3` (UI-7b merged). Visible change: the toolbars, the menus (the program's own and every stock
context menu, menu strip, tool strip and status strip), the module bar, the Chart module's button panel and the light signal grid draw in the
new look from the UiTheme tokens. No control moves, no bounds change, no font changes (UI-2b), no schema change, no package, no download, no
program started, no database used, no window shown.

## 1. What changed

- **The token renderer** (`CodeBase/UI/UiTheme.Renderer.cs`, new): `UiToolStripRenderer` (a `ToolStripProfessionalRenderer`) and its colour
  table `UiToolStripColorTable`. Menu bar on `Surface`, no line under it; drop-downs `SurfaceRaised` with a 1 px `Stroke` border; item hover
  `SubtleHover` with 2 px corners, an open top-level item `SubtlePressed`; separators `Gridline` (menus) or `Stroke` inset 4 px (tool strips);
  the check mark (Fluent Checkmark, 16 px) and the arrows (Fluent chevrons, 12 px) from UI-7a's glyphs; item text `Text` (a stock selected
  item's `HighlightText` too, since the hover shape is light), disabled `TextDisabled`, colours the program set are kept; tool strips
  `SurfaceSubtle` with a `Gridline` bottom line, buttons only on hover (`SubtleHover`), pressed (`SubtlePressed`) or checked (`AccentSubtle`
  with a 1 px `Accent` border), 4 px corners; status strips `SurfaceSubtle` with a `Gridline` top line. It uses UiTheme's per-thread brushes
  and pens (never disposed) and its pixel helpers; `FillRounded` and `OutlineRounded` are shared with the program's own controls.
- **`UiTheme.InitWinForms()`** (`CodeBase/UI/UiTheme.cs`): sets `ToolStripManager.Renderer` to a new token renderer unless the thread has one.
  Windows Forms keeps that renderer in a `[ThreadStatic]` field of `ToolStripManager` in .NET 10 (the test checks the field and the
  behaviour), so the call is made on every UI thread.
- **The calls.** Every exe's `Main` right after `SetDefaultFont` and `SetHighDpiMode`: CentralManager, DatabaseIntegrityCheck,
  DocumentationBuilder, MySQLManager, OpenPmsDentalDbSettings, OpenPmsDentalHL7, ServiceManager, xCrudGenerator, RdlDesign; in
  `ProgramEntry.Main` after O-34's `RenderOptions` line, which LibWebTests and ToothChartImageTests keep right after the DPI mode. Not in
  `UpdateFileCopier` and `OpenPmsDentalHelper32` (no CodeBase, no menu or tool strip). Extra UI threads: `ODProgress.ShowProgressBase`'s thread
  (every progress window) and the Microsoft 365 sign-in message thread. Without the call, with the reason in the test: the TWAIN worker of
  ImagingDeviceManager (no CodeBase; its two windows are stock Forms that keep the Windows look and have no strip) and the tooth chart's WPF
  threads. The query monitor's own thread in `FormOpenDental.cs` is commented-out code (it opens with `Show()` on the main thread).
- **RdlDesign** (`RdlDesign/ReportDesigner.csproj`): compiles `UiTheme.cs`, `.Colors`, `.Gdi`, `.Metrics`, `.Fonts`, `.Renderer` and the icon
  renderer (`UiIcons.cs`, `UiIcons.Gdi.cs`, `IconData.g.cs`) in as linked files and calls `CodeBase.UiTheme.InitWinForms()`; its menu strip,
  status strip and five context menus take the new look. NoHqTests' `UiThemeScanTests` lists `UiTheme.Renderer.cs` with the linkable files.
- **`MenuOD`**: `MenuRenderer` is the token renderer (set on its own strip, so it holds on any thread); the bar is `Surface` with no lines
  (the SlateGray lines under and beside it are gone). Item widths keep `LayoutItems`' formula (text + 10, section 3.3).
- **`ToolBarOD`**: `SurfaceSubtle` with a 1 px `Gridline` line at the bottom; buttons transparent at rest; the shape is 1 px in from the
  button's bounds with 4 px corners: hover `SubtleHover`, pressed `SubtlePressed`, the drop-down part pressed `SubtlePressed` on the right of a
  `SubtleHover` shape; toggled `AccentSubtle` with a 1 px `Accent` border and `TextSelected`; error (`IsRed`) `DangerSubtle` with `Danger`
  text (and a `Danger` outline on hover or toggled); notify (a drop-down's `NotificationText`) `WarningSubtle` with `Warning` text, the
  notification count above the chevron as before; disabled `TextDisabled`, icons too; the drop-down arrow the 12 px Fluent chevron;
  separators a 1 px `Stroke` line inset 4 px. No gradients, no outlines between buttons, no shift of the image and text when pressed.
  `ComputeButtonsizes` is unchanged (text + 6, 8 or 12 px, UQ-9). `ImageIndex` and `Bitmap` buttons draw their pictures as before.
- **`ModuleBar`**: `SurfaceSubtle` with a 1 px `Gridline` divider on the right, no lines between buttons; the shape inset 4 px from the sides
  and 2 px from top and bottom, 6 px corners: hover `SubtleHover`, pressed `SubtlePressed`; the selected module `SurfaceRaised` with a 3 px
  `Accent` mark on the left and its icon in its **Filled** style in `Accent` (UI-7b's `<member>Filled` rows); other icons Regular in `Text`;
  captions in `Text` (their Arial 8 font stays until UI-2b, which takes `UiTheme.FontModuleCaption`). The Chart module keeps its picture
  (Chart32G, and Chart32W on the selected white shape) until UI-8. Hover and pressed come from the mouse events (`currentHot`, `_isMouseDown`)
  instead of reading the mouse position in every paint; the behaviour is the same.
- **Icons without a box.** UI-7b's reviewer noted that `IconLibrary.Draw` fills an opaque Direct2D bitmap over the caller's colours, which
  would show its square inside a rounded hover shape. The toolbar and the module bar draw `EnumIcons` with `ToolBarOD.DrawIconTransparent`:
  the same Fluent design filled with GDI+ (`UiIcons.Draw`, antialiased, UI-7a's OnlyGDI path) straight onto the control, so nothing shows behind
  the icon in any state; the pictures (`IsPicture`) draw their bitmaps. `IconLibrary` itself is unchanged.
- **`ODButtonPanel`**: flat `SurfaceRaised` with a 1 px `Stroke` outline (the shadow and the navy outline are gone); button items as the new
  buttons: `SurfaceRaised`, 1 px `Stroke` border, 4 px corners, `ButtonHover` on hover and `ButtonPressed` pressed (new: the panel tracks the
  item under the mouse), text in `Text`; label items `Text` on `SurfaceRaised`. Item widths keep text + 8 (section 3.3). `ODPanelItem` needs no
  change. It no longer calls `Button.DrawSimpleButton` (UI-4a's file), so the two packages share no drawing code.
- **`LightSignalGrid`**: an unlit light `SurfaceRaised`, hover `SubtleHover`; `Gridline` lines, `Stroke` outline, text in `Text`. The lit
  colours are the office's (Setup, Signals) and draw as before, hover darker as before.
- **WPF `ToolBar` and `ToolBarButton`**: the bar `SurfaceSubtle` with a `Gridline` bottom line; separators keep their 7 x 24 and draw a 1 px
  `Stroke` line inset 4 px; buttons transparent (still hit-testable) with the shape 1 px in, 4 px corners: hover `SubtleHover`, pressed
  `SubtlePressed` (new: shown while the mouse button is down), toggled `AccentSubtle` with a 1 px `Accent` border; disabled `TextDisabled`
  for the text and the icon (drawn again in `TextDisabled`); the drop-down arrow the 12 px Fluent chevron; the 1 px column on the right keeps
  its width (no line is drawn there), so no button changes width.
- **WPF menus**: `Menu.xaml` puts the bar on `Surface` (its 1 px border keeps its place in the bar's colour). `MenuItem` and `ContextMenu`
  override `DefaultStyleKey`, so their templates come from `WpfControlsOD/Themes/Generic.xaml`: two blocks appended after the existing styles
  (the `MenuItem` style and the `ContextMenu` style, each with the menus' separator style in its resources), nothing else of the file touched,
  so the merge with UI-4a's blocks is clean. Drop-downs `SurfaceRaised` with a 1 px `Stroke` border, hover `SubtleHover` with 2 px corners,
  `Gridline` separators, the Fluent check mark and sub-menu arrow, disabled `TextDisabled`, the open top-level item `SubtlePressed`. `MenuItem`
  exposes the token brushes and glyphs the templates use as statics (`MenuItem.BrushSurface`, `MenuItem.GeometryCheckmark`, ...).
- **Not changed**: `ODToolBarButton.cs` and `ODToolBarButtonCollection.cs` (data only), every window's layout, the fonts, the item width
  formulas, `IconLibrary`, the print paths.

## 2. Tests

All in the Targeted assemblies (UiRefreshTests, WindowChecksTests, UnitTests, NoHqTests):

- `tests/UiRefreshTests/Toolbars/ToolBarFitTests.cs` (with `ToolBarSources.cs`, `ToolBarModel.cs`): every module toolbar (the main toolbar,
  Appointments, Family, Account, Treatment Plan, Chart, Imaging's WPF toolbar, its image and drawing toolbars) and the main menu, read from
  their `LayoutToolBar`/`LayoutMenu` source in order (every button the code can add, also those behind a preference, marked conditional; the
  office's program link buttons come after them and are not counted), laid out by the controls' own code at window widths 1024, 1366 and
  1920 (the window less 10 px of borders and the 51 px module bar), in today's font and in UI-2b's Segoe UI 9. The items keep their order
  (each starts where the one before ends); what runs past the right edge is listed in `tests/UiRefreshTests/Data/toolbar-overflow.tsv`, and
  the test fails on overflow the list does not have or a listed one that went away (`OPMS_UI6_WRITE_OVERFLOW=1` rewrites it). The list (section
  4) is the same before and after UI-6: the package changes no width.
- `tests/UiRefreshTests/Toolbars/RendererTests.cs`: `ToolStripManager`'s renderer field is `[ThreadStatic]` and a second thread starts without
  the token renderer until it calls `InitWinForms`; on a second UI thread a stock context menu and `MenuOD` take it; every exe's `Main` calls it
  after `SetHighDpiMode` and before `EnableVisualStyles`/`Application.Run`, the two exceptions do not and reference no CodeBase; RdlDesign links
  the files and its built `RdlDesigner.dll` contains `CodeBase.UiToolStripRenderer`; every file that starts an STA thread calls it or is listed
  with its reason; a context menu, a menu strip and a tool strip draw the token colours (border, background, hover, separator, check mark,
  checked button).
- `tests/UiRefreshTests/Toolbars/WpfMenuToolBarTests.cs`: the WPF menu items and context menus use the Generic.xaml templates (colours, check
  mark, arrow, disabled); the WPF toolbar button's states use the tokens.
- `tests/UiRefreshTests/Toolbars/ToolBarPictureTests.cs`: the pictures of section 3 (each run checks that every picture has ink).
- `tests/UiRefreshTests/DesignSheetTokenTests.cs`: UI-1's `InitWinForms_DoesNothingUntilUi6` is now `InitWinForms_SetsTheTokenRendererOnce`.
- `NoHqTests/UiThemeScanTests.cs`: `UiTheme.Renderer.cs` is one of the linkable files; `tests/UiRefreshTests/Data/colour-literals.tsv` is cut by
  its own writer (the literals of the owned files are gone, but the designer-time `#FFEEF5FD`/`Silver` of `ToolBar.xaml`).
- **C20** (`tests/WindowChecksTests/InProc/InProcChecks.W5.cs`): besides MenuOD's font (UI-2a-1), the main thread's renderer and MenuOD's strip
  are the token renderer.
- **UT1** (new window check, `UTests.UT1_Toolbars_KeepOrderAndFitAt1024_1366_1920`, part `UT` of group U, `InProcChecks.UT.cs`): in the running
  program on the scratch copy, the main window at 1024, 1366 and 1920 x 768 with each module; every visible `ToolBarOD` and the main menu keep
  their order; the overflow (now with the copy's program link buttons) is listed for the owner; each toolbar and the menu bar is pictured.
  Opt-in; needs the lead's connected session.
- The scan rerun: UI-0b's tier 1 scan with `OPMS_UI_SCAN=1` (section 5).
- Targeted runs: `od-tools/test-results/ui6-before.txt` (the tests on main's code), `ui6-1.txt` to `ui6-6.txt`, `ui6-final.txt`.

## 3. Gallery G-UI2, the toolbar part

**Where:** `.git/od-tools/test-results/G-UI2-toolbars-ui6/` (`index.html`, its zip beside it). Sources: `test-results/ui6-pictures/before`
(the same test on the branch point's control code: `2ae1982f3`'s `ToolBarOD`, `ModuleBar`, `ODButtonPanel`, `LightSignalGrid`, `MenuOD`, WPF
toolbar and menus, and an empty `InitWinForms`) and `test-results/ui6-pictures/after`.

**The pictures** (all drawn off screen by the controls' own painting; WPF with `RenderTargetBitmap`, which drew in this disconnected
session): `strips/` each module toolbar at the 1366 px window's width with the module's own buttons and ImageList (main, appts, family,
account, txplan, chart, imaging, imaging-paint, imaging-draw), the main menu bar with one item selected (`menu.png`), a stock drop-down menu
(`menu-dropdown.png`), the WPF menu bar and context menu (`menu-wpf.png`); `states/` the toolbar with a button in every state at 100, 125,
150 and 200 % (`toolbar-<scale>.png`: rest, hover, pressed, drop-down rest/hover/pressed, notify, toggled, toggle, disabled, error, error on
hover, label, icon buttons at rest, hover and disabled, an ImageList button), the WPF toolbar's states (`toolbar-wpf.png`), the module bar with
Account selected, Tx Plan on hover and Chart pressed (`modulebar-*`), the button panel at rest and with a button on hover (`buttonpanel-*`), the
light signal grid with unlit, lit and hover lights (`lightsignals-*`). The before set has no hover or pressed look for the module bar and no
hover for the button panel (main's code took the module bar's hover from the real mouse and the panel had none): those before pictures show
the rest look. `toolbar-fit.tsv` beside the pictures has the fit numbers of section 4.

**For the lead** (the pictures that need a shown window, and rebuilding):

```
rem The real toolbars and menu in the running program at the three widths (UT1) and C20, in the connected, unlocked session:
set OPMS_WINDOW_CHECKS=1
set OPMS_WINDOW_CHECKS_DUMP=<the synthetic dump, as for group U>
set OPMS_WINDOW_CHECKS_GROUPS=UT,C
powershell -NoProfile -ExecutionPolicy Bypass -File D:/Dev/opendental/.git/od-tools/run-tests.ps1 -Root <worktree> -Mode Targeted -Platform x64 -RequireBuildOutput -Include WindowChecksTests -Out D:/Dev/opendental/.git/od-tools/test-results/ui6-wc.txt
rem G-UI2's main window and module pictures (UM1-UM21) with the new toolbars, menu and module bar: group U's part UM, as UI-3's run.
set OPMS_WINDOW_CHECKS_GROUPS=UM
powershell -NoProfile -ExecutionPolicy Bypass -File D:/Dev/opendental/.git/od-tools/run-tests.ps1 -Root <worktree> -Mode Targeted -Platform x64 -RequireBuildOutput -Include WindowChecksTests -Out D:/Dev/opendental/.git/od-tools/test-results/ui6-wc-um.txt
rem The off-screen pictures again (after set), and the gallery:
set OPMS_UI6_PICTURES=D:\Dev\opendental\.git\od-tools\test-results\ui6-pictures\after
powershell -NoProfile -ExecutionPolicy Bypass -File D:/Dev/opendental/.git/od-tools/run-tests.ps1 -Root <worktree> -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests -Out D:/Dev/opendental/.git/od-tools/test-results/ui6-after.txt
powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before D:\Dev\opendental\.git\od-tools\test-results\ui6-pictures\before -After D:\Dev\opendental\.git\od-tools\test-results\ui6-pictures\after -Name G-UI2 -Out D:\Dev\opendental\.git\od-tools\test-results\G-UI2-toolbars-ui6
```

The UT1 pictures (`UT1-<width>-<module>-<toolbar>.png`, `UT1-<width>-menu.png`) of a run on main's code before this package are its before
strips; the UM pictures compare with the before set `test-results/ui-before` (run1 to run5). The hand check at 125 % and 150 % of section 8
(a toolbar, a menu) belongs to UI-3's and UI-2b's G-UI2 approvals.

**Summary for the owner** (at the top of the page): what changed: the toolbars are flat light grey and their buttons show only when the mouse
is on them (light grey), pressed (a little darker) or switched on (light blue with a blue edge); the drop-down arrows are thin chevrons; the
error and notice buttons are light red and light yellow; the menus are white with a thin grey edge and light grey hover, with thin check marks
and arrows, and the menu bar has no lines; the module bar is light grey, the open module is white with a blue mark on its left and a filled
blue icon; the Chart module's quick buttons and the signal lights are flat with thin grey edges. What did not change: every toolbar button,
menu item and module button is where it was and as wide as it was; the fonts (Segoe UI comes with UI-2b); the office's signal light colours;
the Chart module's tooth picture (UI-8); the pictures of buttons that still use an ImageList (UI-9).

## 4. Owner questions

1. **Toolbars that do not fit (UQ-9).** With every button the code can add, at 100 %: the Chart toolbar needs 1348 px (1404 in Segoe UI 9):
   at a 1024 px window its last six buttons are cut off, at 1366 px "Layout" (in Segoe UI 9 also the second HL7 button). The Family toolbar's
   "Discount Plan" and the Imaging image toolbar's "Unmount" are cut off at 1024 px. The main toolbar, Appointments, Account, Treatment Plan,
   the WPF Imaging toolbar, the drawing toolbar and the main menu fit at all three widths in both fonts. This is today's program (UI-6 changes
   no width); Segoe UI 9 adds about 4 % to each toolbar. Keep as is (the office sees fewer buttons on a small screen), or a later package
   (not a restyle) gives toolbars an overflow chevron? The HL7 buttons' captions are the office's HL7 definition names, counted here as their
   code names, so the real width depends on the office.
2. **Module bar captions.** The plan's design is Segoe UI 8; today's Arial 8 stays until UI-2b, which changes the font token. Fine?
3. **Disabled ODButtonPanel items** do not exist (the panel has no disabled state); nothing to decide, noted for completeness.

## 5. Notes

- The scan rerun: UI-0b's tier 1 scan (`OPMS_UI_SCAN=1`) found no new overflow, no self-sizing problem and no bounds difference (the package
  changes no bounds and no font): today 0 new overflows, 668 already in today's font, 0 bounds differences; the font options' counts equal
  UI-2a's run (`test-results/ui2a1-2-scan`, 1775 new overflows at 100% in Segoe UI 9). Files in `test-results/ui6-scan`.
- The final Targeted run (`ui6-final.txt`, with `OPMS_UI_SCAN=1` and the pictures): NoHqTests 122/122, UiRefreshTests 104/104,
  WindowChecksTests 29 passed and 43 opt-in skipped, UnitTests 1728 passed and 3 skipped. Full build: 0 errors. The gallery: 41 pictures,
  41 changed.
- High contrast: every colour comes from a token, so a high-contrast theme gives the SystemColors partners (3.2). The WPF menu templates read
  the token brushes when they are loaded: a switch to or from high contrast shows in the menus after the program restarts.
- Keyboard focus: `ToolBarOD`, `ModuleBar` and the WPF toolbar buttons take no keyboard focus today, so they have no focus ring (3.6's ring
  applies to focusable controls); menus show the keyboard's item with the hover colour.
- `ODToolBarButton.cs` is a UTF-16 file and needs no change; it was not opened for editing.
