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
  covers the button's column over the whole title bar height, inside the outline (the old fill started 5 px down, at the hit rectangle's top).
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
  sizes (`GetTitleFonts`, moved unchanged from both frames) and the DWM trial (`ApplyDwmCorners`).
- `OpenPmsDental/UI/BordersDpi/FormODBase.cs` and `WpfControlsOD/Frame/FormFrame.cs`: `PanelBorders_Paint` calls `PaintFrame`, which builds
  `FramePaintArgs` (`GetFramePaintArgs`, public for the tests) and calls `FramePainter.Paint`; `SetBorderColor` keeps the office's colour as it
  is (no more hot colour of its own); `OnHandleCreated` calls the DWM trial; `BackColor` is `UiTheme.Surface`. The old fixed colours
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
the outline as the frames did (in the new colour), and the DWM trial (3.5) also asks Windows to draw its border in the outline colour, which
is the only way the plan's "1 px outline in `AccentPressed`" can show. This reading of the layout code is what the off-screen pictures assume;
the lead's shown-window pictures (section 6) confirm or correct it.

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

### 3.5 DWM rounded corners: a trial, off by default

`FramePainter.ApplyDwmCorners` asks Windows 11 for `DWMWA_WINDOW_CORNER_PREFERENCE` (round or small round) and for `DWMWA_BORDER_COLOR` in
the frame's outline colour, when the window's handle is made, but only while the environment variable `OPENPMS_UI3_DWM_CORNERS` is `round` or
`small`; never with `AreBordersMS` (Windows rounds its own frame). Unset (the default) nothing is asked of Windows and the frame is as before.
What I saw: nothing yet. The owner's desktop session was disconnected, and the corners are Windows' compositing, which no off-screen picture
shows, so the row's rule "keep them only if the harness pictures are clean" cannot be met here. The lead runs UX3 (section 6); if its
pictures and the window on screen are clean, turning the trial into the default is a one-line change in `GetDwmCornerPreference` (return
`DWMWCP_ROUND` when the variable is unset), with the environment variable then kept as the way to turn it off; if not, the trial code is
removed in UI-11-final.

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
- `DwmCorners_OffByDefault` (and the COLORREF order), `Frame_SourceScan_NoOldFrameColours`.
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
   one selected; the rounded-corner trial), into the same folder: `set OPMS_WINDOW_CHECKS_GROUPS=UX` and the same command.
3. `NoCustomBorders.txt`: `set OPMS_WINDOW_CHECKS_GROUPS=A` (A6 makes and deletes the flag file itself; A6 and the rest of group A must stay
   green), `OPMS_WINDOW_CHECKS_OUT=<od-tools>\test-results\ui3-nocustomborders`.
4. `NoD2D.txt`: no check makes this flag file. Create an empty `NoD2D.txt` beside the Debug x64 `OpenPmsDental.exe`, run step 1's command
   with `OPMS_WINDOW_CHECKS_GROUPS=UM` and `OPMS_WINDOW_CHECKS_OUT=<od-tools>\test-results\ui3-nod2d`, then delete the file.
5. The gallery: `tools\New-UiGallery.ps1 -Before <od-tools>\test-results\ui-before\<run with group U> -After <od-tools>\test-results\ui3-groupU
   -Name G-UI2 -ClipNotes <ui-before>\ui-walker\clip-notes.tsv`, plus the UX, NoCustomBorders and NoD2D folders as their own groups or
   galleries; and this package's off-screen part (section 5).
6. The rounded corners: look at UX3's pictures and at an open window on screen with `set OPENPMS_UI3_DWM_CORNERS=round` (and `small`) set
   before starting the program: clean corners (no square title bar corner over a rounded window, no gap at the top, the border in the outline
   colour) keep the trial as the default (3.5); otherwise say so and it stays off.
7. The owner's real-screen check at 125% and 150% (plan, section 8, "Limits and the hand check") is part of G-UI2's approval.

## 7. For the owner

The title bars are the program's own blue with white text, the minimise, maximise and close buttons are flat with thin modern icons, and the
close button turns a calm red under the mouse. Nothing moved. An office that changed its title bar colours in Setup > Definitions > Misc Colors
keeps each colour it changed; items still on their original colour show "(program default)" there, because the new look draws its own blue
in their place. Rounded window corners are being tried and stay off unless the pictures look clean.
