# Port note: UI-3 (the window frame)

Package UI-3 of the UI refresh ([docs/specs/ui-refresh-plan.md](../specs/ui-refresh-plan.md), section 7's UI-3 row, sections 3.2, 3.3 and
3.6 "Window frame", section 6 "Misc Colors"), on `ui/ui3-1` from `main` at `deb903107`. It changes the look of the window frame of every
`FormODBase` window and every WPF window's `FormFrame`, and nothing else: no bounds, no control, no layout. No schema change, no NuGet package,
no download, no database used by the tests, no window shown.

## 1. What changed on screen

- **Title bar:** `TitleBar` (`Accent`, `#3C5A96`; Open Dental's blue was `#415E9A`) with white title text (`TitleBarText`).
- **Caption buttons:** flat. The glyphs are the Fluent ones of UI-7a: `Subtract` (minimise), `Maximize`, `SquareMultiple` (restore, while
  maximized) and `Dismiss` (close), drawn in a 16 px slot (scaled), which gives a 10 to 11 px mark like the old glyphs. Minimise and maximise on
  hover: `CaptionHover` (`#5370A6`) with a white glyph; close on hover: `Danger` (`#C42B1C`, was `#E81123`) with a white glyph. The hover fill
  covers the button's columns over the whole title bar height as the screen shows it, from its top pixel row, and the close button's fill also
  runs to the window's right pixel column, where Windows' rounded corner and border are drawn over it, as on Windows 11's own caption buttons
  (the old fill started 5 px down, at the hit rectangle's top). The glyphs stay where they were. See 3.6.
- **Rounded corners:** Windows 11's rounded window corners and its 1 px border in the outline colour, by default (3.5).
- **Outline:** `TitleBarOutline` (`AccentPressed`, `#2A406C`; was black), 1 px, 2 px at 200% (section 3.3). See 3.3 below: it lies under the
  client area everywhere but along the title bar.
- **Window background:** `Surface` (`#FAFAFA`) in the `FormODBase` and `FormFrame` constructors (was `ColorOD.Background`, `#FCFDFE`). A form
  whose designer sets its own `BackColor` keeps it.
- **Floating image windows** (`FormImageFloat`, `IsImageFloatSelected`): selected, `Accent` with white text, the `TitleBarOutline`, hover
  `CaptionHover`; not selected, `SurfaceSubtle` with `Text` and a 1 px `Stroke` outline, hover `SubtleHover`; close hover `Danger` on both. The
  office's Misc Colors do not apply to them, as before.
- **Not changed:** every position and size (the caption buttons' hit rectangles, the icon, the title's place and font, the title bar height,
  the resize edges), the title font (`UiTheme.FontFamilyName`, Microsoft Sans Serif until UI-2b; 8.25 pt, 13 pt for the main window, at the
  window's zoom), the program icon, the mouse handling, maximize and snap, and the Windows frame with `NoCustomBorders.txt`.

## 2. Files

- `WpfControlsOD/Frame/FramePainter.cs` (new): `FramePainter.Paint` (the whole frame, returning the caption button rectangles), the Misc Colors
  rule (`GetWindowColors`, `IsStockMiscColor`, `IsTitleBarMiscColor`), the floating windows' colours (`GetImageFloatColors`), the title font
  sizes (`GetTitleFonts`, moved unchanged from both frames), the part of the frame the screen shows (`GetRectangleOnScreen`) and the rounded
  corners (`ApplyDwmCorners`, `GetDwmCornerPreference`).
- `OpenPmsDental/UI/BordersDpi/FormODBase.cs` and `WpfControlsOD/Frame/FormFrame.cs`: `PanelBorders_Paint` calls `PaintFrame`, which builds
  `FramePaintArgs` (`GetFramePaintArgs`, public for the tests) and calls `FramePainter.Paint`; `SetBorderColor` keeps the office's colour as it
  is (no more hot colour of its own); `OnHandleCreated` asks for the rounded corners; `BackColor` is `UiTheme.Surface`; a hover change
  repaints what the caption buttons paint (`InvalidateCaptionButtons`, 3.6). The old fixed colours
  (`_colorRedX`, `_colorButtonHot`, the floating windows' `colorFloatBase`) are gone. Both files have mixed line ends in their blobs; they were
  edited byte for byte (the lines outside the changes keep theirs).
- `OpenPmsDental/Logic/DefL.cs`: `FillGridDefs` shows "(program default)" in the value column of Misc Colors items 11, 12 and 13 while they
  hold Open Dental's stock value (translatable through `Lan.g("FormDefinitions",...)`). The swatch still shows the stored colour.
- Tests: `tests/UiRefreshTests/FrameTests.cs` (new), `tests/UiRefreshTests/Data/colour-literals.tsv` (the four old frame literals of
  `FormODBase.cs` cut), `tests/WindowChecksTests/InProc/InProcChecks.UX.cs` (new) and `tests/WindowChecksTests/UTests.cs` (part `UX`).

## 3. Decisions made here

### 3.1 The painter's home: `WpfControlsOD/Frame/FramePainter.cs`

`OpenPmsDental` (FormODBase) references `WpfControlsOD` (FormFrame); `WpfControlsOD` cannot reach `OpenPmsDental/UI/BordersDpi/`. CodeBase
would be reachable from both, but it references no business layer, and the painter needs `DefCatMiscColors` (OpenPmsDentalBusiness) for the
Misc Colors rule, which both frames and the Definitions window share. `WpfControlsOD` references CodeBase (tokens, glyphs) and
OpenPmsDentalBusiness, so it is the one project that has everything. The class is in namespace `OpenPmsDental`, like `FormFrame`.

### 3.2 Misc Colors, item by item (section 6, UQ-3)

- An item that holds Open Dental's stock value gives its token: Main Border (`-12493158`) gives `TitleBar`, Main Border Outline (black) gives
  `TitleBarOutline`, Main Border Text (white) gives `TitleBarText`. An item the office changed gives the office's colour.
- Minimise and maximise hover: `CaptionHover` while Main Border and Main Border Text are both stock; otherwise the office's own title bar keeps
  today's hover formula (three parts bar colour, one part text colour) with the glyph in the text colour, so a hover never clashes with the
  office's colours. Close hover is always `Danger` with white.
- While Windows runs a high-contrast theme the tokens win over the office's colours (section 1: "Windows high-contrast themes win"):
  `ActiveCaption`, `ActiveCaptionText`, `WindowFrame`.
- `FormODBase` keeps its three colours in static fields, set by `FormOpenDental` at start-up and by `FormDefEdit` on a save, as before;
  `FormFrame` reads them from the definitions cache on each Load, as before.

### 3.3 The outline, and what the screen shows of the frame

The frame's `PanelBorders` is laid out at (-1,-1) of the client area, and the client panel (`PanelClient`, or the `ElementHost` of a Frm)
covers everything below the title bar (`LayoutManagerForms.LayoutFormBoundsAndFonts`, `UIManager.LayoutFormBoundsAndFonts`). So of the frame
only the title bar strip is visible, from the panel's row and column 1; the outline the frames draw at the panel's edges was under the client
area or outside the panel before UI-3 too. The 1 px line seen around a window on screen is Windows' own (DWM) border. FramePainter still draws
the outline as the frames did (in the new colour), and the rounded corners (3.5) also ask Windows to draw its border in the outline colour, which
is the only way the plan's "1 px outline in `AccentPressed`" can show. This reading of the layout code is what the off-screen pictures assume;
the owner's capture of a shown window (3.6) confirmed it: the screen shows the PanelBorders from their row and column 1 to the
client area's last column, and Windows' border lies over the client area's row 0.

### 3.4 Small things found on the way

- `FormFrame`'s copy of the floating-window colours compared `GetType().ToString()` (the full name) with `"FormImageFloat"`, which is never
  true, and `FormImageFloat` is a `FormODBase` anyway: the branch was never reached. It is kept (never cut), now testing the type's `Name`, for
  a future WPF floating image window.
- A window without a control box kept drawing the caption glyphs at their last rectangles (or at an empty one, over the icon, when it never
  had a control box), and the close button still lit red on hover, although a click there did nothing (the button area is computed without
  the buttons). `FormDatabaseMaintenance` turns `ControlBox` off while a check runs ("We do NOT want the user to click the X button"), and
  `FormOpenDentalBShell` has none. The painter draws a caption button only where it exists, so these windows show no dead buttons; clicks
  behave as before.
- The 150% off-screen pictures show the title text smaller than on a real 150% screen: the bitmap is drawn at 96 DPI and the title font is
  given in points. The same in the before and after pictures; the real-screen check at 125% and 150% is part of G-UI2's approval (plan,
  section 8).

### 3.5 DWM rounded corners: the default (the owner, 2026-11-18)

`FramePainter.ApplyDwmCorners` asks Windows 11 for `DWMWA_WINDOW_CORNER_PREFERENCE` (round or small round) and for `DWMWA_BORDER_COLOR` in
the frame's outline colour, when the window's handle is made; never with `AreBordersMS` (Windows rounds its own frame, and
`NoCustomBorders.txt` is unchanged). It began as a trial behind the environment variable `OPENPMS_UI3_DWM_CORNERS`; the owner tried it on the
real desktop (Windows 11, `round`) and answered "corners yes", so it is the default now (`GetDwmCornerPreference`):

| `OPENPMS_UI3_DWM_CORNERS` | Corners |
|---|---|
| unset, empty, `round`, or any other value | rounded (`DWMWCP_ROUND`), the border in the outline colour |
| `small` | small rounded (`DWMWCP_ROUNDSMALL`), the border in the outline colour |
| `off`, `square` or `none` | nothing is asked of Windows: the frame as before UI-3 |

The variable is read when each window's handle is made. Windows 10 refuses both attributes, and the frame there is as before.

### 3.6 The caption buttons' hover on screen (the owner's finding, 2026-11-18)

**What the owner saw.** On the real desktop (Windows 11, the UI-3 build with `OPENPMS_UI3_DWM_CORNERS=round`), with the mouse over the main
window's close button, the red started 3 device pixels below the top of the title bar, with a strip of the title bar's blue above it, and did
not run into the window's top-right corner; the off-screen gallery picture showed the red reaching the top. The lead's capture:
`.git/od-tools/test-results/ui3-live-closex-hover.png` (and `-4x`): at 100%, row 0 of the title bar is Windows' border, rows 1 to 3 blue,
the red from row 4 to the title bar's last row (25).

**Why.** Two causes, both outside what the off-screen pictures drew:

1. *The repaint was clipped to the hit rectangle.* On a hover change both frames called `PanelBorders.Invalidate(_rectangleButtons)`
   (`FormODBase.PanelBorders_MouseMove`, lines 951, 966, 978 and 987, and `FormFrame.PanelBorders_MouseMove`, lines 772, 787, 799 and 808,
   before the fix). `_rectangleButtons` is the buttons' hit area, which `FramePainter.Paint` puts at `y = 5 + MaxInset` (the 5 px above are
   the top resize edge). Windows clips the PanelBorders' paint to the invalidated rectangle, so the new fill, which `GetButtonShape` starts
   at the PanelBorders' row 1 (the client area's row 0, the window's top row), only reached the screen from the PanelBorders' row 5 down:
   the client rows 1 to 3 kept the title bar colour (row 0 is under Windows' border). Every button, at every scale and maximized too. The
   off-screen tests and pictures paint the whole frame into a bitmap with no clip, so they showed the fill from the top. (The floating image
   windows' own `MouseMove` invalidates the whole PanelBorders after the base's, which hid this cause there.)
2. *The close fill stopped short of the window's right edge above 100%.* The painter worked in window coordinates (`Width` is the window
   with its invisible resize borders) and ended the close fill at its hit rectangle's right edge, `Width - Scale(15)`, clamped at
   `Width - 1`. The screen shows the PanelBorders up to the client area's last column, `Width - 2 x border` (border = round(8 + 5 x (scale -
   1)): 8, 9, 11, 12, 13), and `Scale(15)` is larger than `2 x border - 1` above 100%: 2 columns of title bar right of the red at 125%, 1 at
   150%, 3 at 175%, 5 at 200%. At 100% they meet, so the owner's capture shows only cause 1.

**The fix.** `FramePainter` learns what the screen shows of the PanelBorders: `FramePaintArgs.RectangleOnScreen`, which both frames fill
from their PanelBorders' bounds and the window's client area (`FramePainter.GetRectangleOnScreen`: the client area is the window less the
invisible borders left, right and below, as `WM_NCCALCSIZE` makes it; maximized, its rows above the border are off the screen). Each button's
fill (`GetButtonShape`) runs from that rectangle's top row to the title bar's last row, and the close fill to its right column; the glyphs are
centred where they were (`GetGlyphArea`, the old shape), so nothing moves. `FramePaintResult.RectangleButtonsPaint` is everything the
buttons paint, and both frames now invalidate it on a hover change (`InvalidateCaptionButtons`). Not changed: the hit rectangles
(`RectangleButtons` and the three buttons'), the title bar height, the client area, the mouse handling, `NoCustomBorders.txt`. A frame painted
without its PanelBorders laid out (`RectangleOnScreen` empty) takes the whole window, so the fill then reaches the bitmap's top row and right
column.

**Shown off screen.** `Frame_CaptionHover_FillsTheTitleBarAsTheScreenShowsIt` lays each frame out as on screen and repaints only what the
frame invalidates, as Windows does. With the old invalidation and close edge put back it failed in all 150 cases, the first being exactly the
owner's picture: "FormODBase stock 100%, hotX: the hover fill on screen is {X=414,Y=4,Width=30,Height=22}, the title bar's extent for the
button is {X=414,Y=0,Width=30,Height=26}", and at 125% the close fill 2 columns narrow (`Width=37` for 39). With the fix it passes.

## 4. Tests

`tests/UiRefreshTests/FrameTests.cs` (no window shown, no database; windows built and painted into bitmaps on an STA thread):

- `Frame_FormODBaseAndFormFrame_PaintTheSamePixels`: a `FormODBase` window and a `FormFrame` (hosting a plain test Frm) with the same title,
  size and caption buttons give the same pixels at 100, 125, 150 and 200%, at rest, with minimise, maximise or close on hover, and without
  minimise and maximise.
- `Frame_PaintsTheTokens_TitleBarGlyphsAndHover`: `TitleBar`, the outline, the Dismiss and Subtract glyphs, no old red, `Danger`,
  `CaptionHover`, the hover over the whole title bar height.
- `Frame_ButtonRectangles_AreTodays`: the hit rectangles of the three buttons and of the button area are today's formula at 100 and 150%.
- `Frame_FormImageFloat_SelectedAndUnselected`: `FormImageFloat` selected and not selected (frame look, bar, outline, title colour, maximise
  hover, close hover), with an office's colours set, which do not reach it.
- Misc Colors: `MiscColors_AllStock_GiveTheTokens`, `MiscColors_OnlyTheTextChanged_KeepsTheText`, `MiscColors_OnlyTheOutlineChanged_KeepsTheOutline`,
  `MiscColors_AllChanged_GiveTheOfficesColours` (also painted through `SetBorderColor` on a window), `MiscColors_HighContrast_TokensWin`,
  `MiscColors_StockValues_AreOpenDentals`.
- `Definitions_MiscColors_ShowProgramDefault`: `DefL.FillGridDefs` with the window's own options for Misc Colors: the note on the stock items
  11 and 13, none on a changed item 12 or on another item, the swatch unchanged.
- `Frame_CaptionHover_FillsTheTitleBarAsTheScreenShowsIt` (3.6): `FormODBase`, `FormFrame` and the floating image windows (selected and not),
  laid out as on screen, at 100, 125, 150, 175 and 200%, normal and maximized, stock and an office's own Misc Colors: after minimise,
  maximise and close on hover, repainting only what the frame invalidates, the fill covers exactly the button's columns (close: to the client
  area's last column) from the title bar's top row on screen to its last row, with whole edges; back at rest no hover colour is left.
- `DwmCorners_RoundByDefault_TheVariableTurnsThemOff` (was `DwmCorners_OffByDefault`): round when unset, empty, `round` or unknown; `small`;
  `off`, `square`, `none`; through the environment too; and the COLORREF order.
- `Frame_SourceScan_NoOldFrameColours`: also that no hover change invalidates only `_rectangleButtons` and that both frames call
  `InvalidateCaptionButtons` on their four hover changes.
- The frame tests now lay their windows out as `OnLoad` does (`LayOutAsOnLoad`: `SetPanelClient` or `SetElementHost`, then
  `LayoutFormBoundsAndFonts`), so `GetFramePaintArgs` sees the PanelBorders where the screen has them.
- `Gallery_FrameParts_RenderOffScreen`: the pictures of section 5.

Window check A6 (NoCustomBorders) is opt-in and needs the connected session (section 6); `PopupHelperTests` (UnitTests) pass. Targeted run
results: `.git/od-tools/test-results/ui3-1-*.txt` (UiRefreshTests, WindowChecksTests, UnitTests and NoHqTests).

## 5. Gallery G-UI2, the frame part drawn off screen

- Folder: `.git/od-tools/test-results/g-ui2-frame-ui3/` (`index.html`, and `g-ui2-frame-ui3.zip` beside it), made by
  `tools/New-UiGallery.ps1 -Before <src>\before -After <src>\after -Name G-UI2` from `.git/od-tools/test-results/ui3-1-frame-pictures/` (the
  test's output, with `notes.txt`). 69 pictures, all changed (the title bar colour is in every one).
- What is in it: `FormODBase` and `FormFrame` windows in a stock office, an office that changed only the title text, and an office that changed
  all three Misc Colors, at 100% and 150%, at rest, with minimise, maximise or close on hover, and maximized (the restore glyph); the main
  window's larger title in each office; the floating image windows selected and not, at rest and with maximise or close on hover.
- The "before" pictures are drawn by a copy of today's painter in the test (`LegacyFramePainter`, from `main` before UI-3), at the same sizes,
  so each pair differs only by the look. Each picture is the window as the screen shows it (3.3): the title bar strip from the panel's row 1,
  and the client area filled with the window background.
- To make it again: run `Gallery_FrameParts_RenderOffScreen` (Targeted run of UiRefreshTests) with `OPMS_UI3_FRAME_OUT=<empty folder>`, then
  the command above.

## 6. For the lead (the pictures that need a shown window)

At the owner's connected desktop (`query user`: Active), after a full Debug x64 lane build of `ui/ui3-1`, in the worktree:

1. G-UI2's main window, modules and dialogs, stock office (the before set is `ui-before`, main `3809f35db`):

       set OPMS_WINDOW_CHECKS=1
       set OPMS_WINDOW_CHECKS_DUMP=<od-tools>\winchk-gate3a\opendental_dev.sql
       set OPMS_WINDOW_CHECKS_GROUPS=UM,UW
       set OPMS_WINDOW_CHECKS_OUT=<od-tools>\test-results\ui3-groupU
       set OPMS_WINDOW_CHECKS_BUDGET_MINUTES=18
       set OPMS_WINDOW_CHECKS_UI_BEFORE=<od-tools>\test-results\ui-before\run5
       powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include WindowChecksTests

   (`UW` and `UM` may need two runs into the same folder to fit the runner's 20 minutes, as UI-0c.md section 5 says; point
   `OPMS_WINDOW_CHECKS_UI_BEFORE` at the before run that holds group U.)
2. The frame checks UX1 to UX3 (a customised office on the main window, About and a Frm; the Imaging module with two floating image windows,
   one selected; the rounded corners: round, small and off), into the same folder: `set OPMS_WINDOW_CHECKS_GROUPS=UX` and the same command.
3. `NoCustomBorders.txt`: `set OPMS_WINDOW_CHECKS_GROUPS=A` (A6 makes and deletes the flag file itself; A6 and the rest of group A must stay
   green), `OPMS_WINDOW_CHECKS_OUT=<od-tools>\test-results\ui3-nocustomborders`.
4. `NoD2D.txt`: no check makes this flag file. Create an empty `NoD2D.txt` beside the Debug x64 `OpenPmsDental.exe`, run step 1's command
   with `OPMS_WINDOW_CHECKS_GROUPS=UM` and `OPMS_WINDOW_CHECKS_OUT=<od-tools>\test-results\ui3-nod2d`, then delete the file.
5. The gallery: `tools\New-UiGallery.ps1 -Before <od-tools>\test-results\ui-before\<run with group U> -After <od-tools>\test-results\ui3-groupU
   -Name G-UI2 -ClipNotes <ui-before>\ui-walker\clip-notes.tsv`, plus the UX, NoCustomBorders and NoD2D folders as their own groups or
   galleries; and this package's off-screen part (section 5).
6. The rounded corners (now the default, 3.5): look at UX3's pictures (round, small and off) and at an open window on screen, and hover the
   close, maximise and minimise buttons at 100% and at 125% or 150%: the fill reaches the title bar's top and the close fill the window's
   right edge under the rounded corner (3.6). `set OPENPMS_UI3_DWM_CORNERS=off` before starting the program shows the frame without them.
7. The owner's real-screen check at 125% and 150% (plan, section 8, "Limits and the hand check") is part of G-UI2's approval.

## 7. For the owner

The title bars are the program's own blue with white text, the minimise, maximise and close buttons are flat with thin modern icons, and the
close button turns a calm red under the mouse, from the top of the title bar into the window's corner, as in Windows 11. Nothing moved. An office that changed its title bar colours in Setup > Definitions > Misc Colors
keeps each colour it changed; items still on their original colour show "(program default)" there, because the new look draws its own blue
in their place. Windows have Windows 11's rounded corners (you said "corners yes"); `OPENPMS_UI3_DWM_CORNERS=off` turns them off on a PC.
