# The UI refresh: package plan

**What this is.** The plan for a new look for the whole program: modern icons, buttons and styles, clean and professional. The owner
asked for it on 2026-11-17 ("Do ui refresh. Modernize icons, buttons, styles. Keep it clean and professional") and set six decisions the
same day (section 1). Package briefs are cut from this document the same way as from `docs/specs/net10-plan.md` and
`docs/specs/wpf-tooth-chart-plan.md`.

**Date.** 2026-11-17. **Status.** Proposed. Waiting for the owner's answers to UQ-1 to UQ-7 (section 11) and for gallery G-UI1 part 1
(section 8), which shows the design before any window changes.

**Basis.**
- Four read-only surveys of `main` at `8d34b27a0` (2026-11-17): the shared controls and their colours, the icons, the fonts (with
  measurements on the build PC at 96 DPI), and the tooling for pictures and approval.
- The public Fluent UI System Icons repository and npm registry (checked 2026-11-17).
- The WCAG 2.2 contrast rules (level AA: 4.5:1 for normal text, 3:1 for large text, 3:1 for the outline of a control and for focus).
- Nothing was built, run or edited for this plan. Line numbers are at `8d34b27a0`. Each brief greps them again.

**Terms.**
- "Self-drawn control": a control the program paints itself (GDI+ in WinForms, a XAML template in WPF). Its look comes from our code.
- "Stock control": a Windows control drawn by Windows (TextBox, RadioButton, DateTimePicker and others). Its look comes from Windows 11.
- "Token": one named value of the new look (a colour, a size, a font), defined once in code.
- "The scan": the text-fit scan of section 5, which finds text that no longer fits after the font change.
- "The harness": the window-check harness in `tests/WindowChecksTests` (`docs/port-notes/WINCHK-HEADLESS.md`).

---

## 1. Goal and the owner's decisions

**Goal.** Every window looks modern, calm and consistent: flat surfaces, thin borders, small rounded corners, one accent colour, one
font, and one icon family. Staff who switch from Open Dental find every control where it was. Only the look changes.

**The owner's decisions (2026-11-17).**
1. **Restyle only.** Every window keeps its layout and its control positions. Staff switch from Open Dental and know where things are.
   - Allowed: colours, borders, corner radii, fonts, icons, and how a control draws inside its own bounds.
   - Not allowed: moving a control, changing a window's layout, new windows in place of old ones.
   - The one exception, needed by decision 3: a control whose text no longer fits may get a little wider or taller, in place, when the
     space is free (section 5). Its position does not change.
2. **The new look replaces the classic look.** There is no classic option and no theme setting (section 6).
3. **Segoe UI everywhere.** It replaces the Microsoft Sans Serif 8.25 default font of net10-plan decision 9 (the S3b-4 row and the
   "Default font" test row of `docs/specs/net10-plan.md` section 7). This plan supersedes those lines (UI-2 adds the note there).
4. **Fluent UI System Icons** (Microsoft, MIT licence) replace the bitmap icons.
5. **The tooth chart's own drawing keeps Open Dental's look** (`docs/specs/wpf-tooth-chart-plan.md`). The window around it is
   restyled. The chart's font is pinned so the default font change cannot reach it (UI-2).
6. **Keep it clean and professional.** Few colours, no gradients, no glows, no decoration.

**Decided by default (say if you disagree; nothing else depends on these).**
- Light look only. The program has no dark mode today, and every self-drawn control assumes a light background. WinForms'
  `Application.SetColorMode` stays on Classic (the default; the program never calls it).
- Stock controls keep the Windows 11 look and only take the new font. Restyling them would mean replacing them with self-drawn ones,
  which risks behaviour and layout.
- Message boxes stay the Windows message box (5,201 `MsgBox.Show` and 2,280 `MessageBox.Show` calls). Windows 11 already draws them
  cleanly. A themed message box would be a new window layout.
- The 17 mouse cursors stay. Fluent has no cursors.
- Data colours stay: appointment, procedure, account, chart graphic and other colours set in Setup > Definitions are the office's content,
  not the theme.
- About 35 resource images that no code uses today are kept as they are (the rule "never cut unused features"). The refresh removes only
  what it replaces (section 6).

---

## 2. What carries the look today

**One idea first.** The look is not in one place. Each shared control hard-codes its own colours, and each WPF control copies the WinForms
values. Changing a colour today means finding it in two or more files.

**Shared controls (WinForms).** 27 self-drawn controls in `OpenPmsDental/UI`, plus `GridOD` in `OpenPmsDentalBusiness/UI/GridOD.cs`.
They paint in `OnPaint` with GDI+.

| Control | File | Today's look | Uses (instances / files) |
|---|---|---|---|
| Window frame | `OpenPmsDental/UI/BordersDpi/FormODBase.cs` (1,816 lines) | own title bar and border; title 65,94,154, white text, close hover 232,17,35; form background 252,253,254; font Microsoft Sans Serif 8.25 (line 137), title fonts (806-807) | about 845 window classes |
| WPF window frame | `WpfControlsOD/Frame/FormFrame.cs` | a copy of FormODBase's border code and colours (44-48, 515), title fonts (627-628) | about 160 Frms |
| Button | `OpenPmsDental/UI/Button.cs` (476 lines) | 3 px corners, white to 225,232,235 gradient, border 28,81,128, hover border 0,110,190, white text glow | 2,969 / 795 |
| Grid | `OpenPmsDentalBusiness/UI/GridOD.cs` (4,675 lines) | title gradient 156,175,230 to 60,90,150; header 223,234,245; outline 77,100,147; selection 205,220,235; fonts sans serif 8.5 and 10 bold (3567-3572) | 568 / 416 |
| Toolbar | `OpenPmsDental/UI/ToolBarOD.cs` (835 lines) | white to 191,201,229 gradient; toggled, pushed, error and notify gradients; Microsoft Sans Serif (89) | 29 / 25 |
| Menus | `OpenPmsDental/UI/MenuOD.cs` (with `MenuRenderer`) | professional renderer, window background | MenuOD 26 files, 111 items; 102 stock context menus in 63 files |
| Module bar | `OpenPmsDental/UI/ModuleBar.cs` (581 lines) | hot 235, pressed 210, selected white, outline 28,81,128; captions Arial 8 (323) | 1 (main window) |
| Check box | `OpenPmsDental/UI/CheckBox.cs` | box grey 50, hover 210,239,255 | 1,290 / 350 |
| Combo box | `OpenPmsDental/UI/ComboBox.cs` | outline 173, hover 229,241,251 | 565 / 233 |
| List box | `OpenPmsDental/UI/ListBox.cs` | selection 186,199,219, slate border | 370 / 212 |
| Group box | `OpenPmsDental/UI/GroupBox.cs` | silver rounded border | 745 / 318 |
| Tabs | `OpenPmsDental/UI/TabControl/TabControl.cs` | grey border, selected 170,190,230 | 55 / 47 |
| Others | `SplitContainer`, `MonthCalendarOD`, `ComboBoxClinicPicker`, `ODDatePicker`, `ODDateRangePicker`, `PanelOD`, `ToolTipOD`, `ODButtonPanel`, `LightSignalGrid`, the appointment and imaging sliders | hard-coded colours, some `ColorOD` | |

**Shared controls (WPF).** About 35 UserControls in `WpfControlsOD/UI/Controls` with XAML templates and brushes set in code. Shared
dictionaries: `Generic.xaml` (74 lines), `TabControlDict.xaml` (195), `DataGridDict.xaml` (142). Most used: Label 799, Button 487,
TextBox 382, CheckBox 170, ComboBox 73, GroupBox 69, ListBox 60, Grid 42 (instances). The WPF controls already use Segoe UI 11.5 DIP
(`WpfControlsOD/Frame/FrmODBase.cs:244-245`), so only their colours, corners and icons change.

**The one shared palette.** `WpfControlsOD/UI/ColorOD.cs` holds 6 colours (Background 252,253,254; Control 240; Gridline 180; Hover
229,239,251; Outline 90; DisabledText 160), used about 162 times in 47 files. `GridOD` cannot use it: `OpenPmsDentalBusiness` does not
reference `WpfControlsOD`. That is why the grid keeps its own 27 `Color.FromArgb` values.

**The user-editable look.** Setup > Definitions > Misc Colors items 11, 12 and 13 (Main Border, Main Border Outline, Main Border Text;
defaults 65,94,154, black and white). `FormOpenDental.cs:830-833` and `FormFrame.cs:234-236` feed them to every window's title bar. These
are the only look colours an office can change.

**Fonts.**
- WinForms gets Microsoft Sans Serif 8.25 from three places: `Application.SetDefaultFont` in 12 exe `Main`s (used by the 78 plain forms),
  FormODBase's constructor (used by its subclasses), and `LayoutManagerForms.FontInitial` (55 uses, by UserControls).
- `LayoutManagerForms` rebuilds every control's font when it scales. It keeps the family and changes only the size (lines 868-914).
- More fixed fonts: 418 designer fonts in Microsoft Sans Serif in 134 files; about 59 code lines naming it outside designers;
  `FontFamily.GenericSansSerif` (which is Microsoft Sans Serif) in about 40 files, GridOD among them; Arial 8 in the module bar.
- The WPF Frms already use Segoe UI.

**Icons, from 8 sources.**
- `IconLibrary`, two copies: WinForms `OpenPmsDental/UI/IconLibrary` (38 icons; most drawn as vectors through Direct2D, 11 stored as
  pictures) and WPF `WpfControlsOD/UI/IconLibrary` (53 icons, mostly pictures). Set 429 times in code and 88 times in XAML. Generated by
  Open Dental's IconOrganizer tool, which is not in the repository.
- 108 bitmap entries in 6 `Resources.resx` files; code assigns them 361 times in 186 files (most used: down 53, up 46, butPrintSmall 43,
  Left 39, Right 38, butPrint 25).
- 55 ImageLists in 50 `.resx` files with 244 pictures, mostly 22×22. Toolbars pick them by index (318 `new ODToolBarButton(` calls).
- 70 pictures embedded per form in `.resx` files (button images).
- 11 WPF `BitmapFileName` uses and 7 WPF resource pictures.
- Window icons (`branding/app.ico`, `window.ico`): the program's own logo. Not part of this refresh.
- 14 pictures seeded in the database: 8 procedure buttons and 6 vendor program links.
- 17 cursors.

**Sharpness today.** Only the Direct2D vector icons stay sharp above 100% scaling. Every bitmap is a 22×22 or 32×32 picture stretched by
GDI+, so it is blurry at 125-200%. Fluent vectors fix that as a side effect.

**Open Dental's colour themes are already gone.** `ODColorTheme` is not in this fork's history. What is left: two comments
(`FormODBase.cs:363`, `GridOD.cs:1761`), a commented-out line in `UnitTests/UI Testing/FormButtonTest.cs:37`, and three deprecated
stored settings that no code reads (section 6).

---

## 3. The design

### 3.1 The look in a few sentences

A light, calm surface. Thin neutral borders and 4 px rounded corners on controls. Buttons are flat and white with a thin grey border; the
form's default button (its `AcceptButton`) is filled with the accent colour. Toolbars and the module bar are flat light grey; their buttons
show only on hover. Selection is a pale blue fill. One accent colour, the program's own blue, marks the title bars, grid titles, focus,
selection and the default button. Icons are thin single-colour Fluent line icons. No gradients, no shadows inside windows, no text glow.
It looks like Windows 11 and Microsoft 365 without copying their layouts.

### 3.2 Colour tokens (light only)

**The accent is the program's brand blue, `#3C5A96`.** It is already the logo's colour, the splash screen's bar and the grid title's
bottom colour (`tests/AttributionTests/BrandingTests.cs:42`). It is close to Open Dental's title bar blue (65,94,154 = `#415E9A`), so the
program still feels familiar. White text on it has 6.8:1 contrast, above AA's 4.5:1.

Contrast values are WCAG 2.2 ratios, computed for this plan. Disabled text is exempt from the WCAG contrast rules.

| Token | Value | Used for | Contrast |
|---|---|---|---|
| `Surface` | `#FAFAFA` | window and panel background (replaces `ColorOD.Background`) | Text on it 15.8:1 |
| `SurfaceRaised` | `#FFFFFF` | inputs, grids, lists, menus, buttons, tooltips | Text 16.5:1 |
| `SurfaceSubtle` | `#F3F3F3` | toolbars, module bar, grid header, tab strip | Text 14.9:1, TextSecondary 6.0:1 |
| `SubtleHover` / `SubtlePressed` | `#EBEBEB` / `#E0E0E0` | toolbar, module bar and menu items | Text 12.5:1 or more |
| `ButtonHover` / `ButtonPressed` | `#F5F5F5` / `#EBEBEB` | white buttons | Text 13.7:1 or more |
| `Stroke` | `#D1D1D1` | button border, container borders, group boxes, dividers | (decorative) |
| `StrokeStrong` | `#8A8A8A` | outline of inputs, combo boxes, list boxes, check boxes | 3.5:1 on white, 3.3:1 on `Surface` (AA 3:1) |
| `StrokeStrongHover` | `#616161` | the same outlines on hover | 6.2:1 on white |
| `Gridline` | `#E0E0E0` | grid lines, menu separators | (decorative) |
| `Text` | `#1F1F1F` | all text | 16.5:1 on white |
| `TextSecondary` | `#5C5C5C` | hints, unselected tabs, captions | 6.7:1 on white |
| `TextDisabled` | `#A0A0A0` | disabled text and icons | exempt (2.6:1) |
| `Accent` | `#3C5A96` | title bar, grid title, default button, focus ring, check box fill, selected tab line | white on it 6.8:1; on `SurfaceSubtle` 6.1:1 |
| `AccentHover` / `AccentPressed` | `#324C80` / `#2A406C` | default button and accent states; title bar outline | white on them 8.5:1 / 10.3:1 |
| `AccentSubtle` | `#DCE6F5` | selected grid row, selected list item, toggled toolbar button | Text 13.1:1; Accent 5.4:1 |
| `RowHover` | `#F0F4FA` | grid and list hover | Text 14.9:1 |
| `CaptionHover` | `#5370A6` | title bar minimise and maximise hover | white 5.0:1 |
| `Danger` | `#C42B1C` | close button hover, error text and error toolbar state | white on it 5.7:1 (today's `#E81123` gives 4.6:1) |
| `DangerSubtle` | `#FDE7E9` | error toolbar state background | Text 14.0:1; `Danger` on it 4.8:1 |
| `Warning` / `WarningSubtle` | `#9D5D00` / `#FFF4CE` | notify toolbar state | 5.3:1 on white; `Warning` on `WarningSubtle` 4.8:1 |
| `Success` | `#0F7B0F` | "complete" icon tint | 5.4:1 on white |

**The title bar** uses `Accent` with white text, and `AccentPressed` as its outline. Misc Colors 11, 12 and 13 stay as an office
override (section 6, UQ-3).

**The grid title** is a flat `Accent` bar with white semibold text. The header row is `SurfaceSubtle` with `Text`.

### 3.3 Sizes, corners and lines

All values are at 96 DPI. `LayoutManager` scales them like everything else.

- **Corners.** Controls (buttons, inputs, combo boxes, tooltips, toolbar and module bar hover shapes): 4 px. Check box box: 2 px.
  Containers (group boxes): 6 px. Grids, tab pages, menus and the window frame: square (Windows 11 rounds the window itself if UI-3
  shows that DWM rounding works with the program's own title bar; the surveys could not tell).
- **Lines.** Borders 1 px. Focus ring 2 px in `Accent`, drawn inside the control's bounds. Selected tab underline 2 px. Module bar
  selection mark 3 px.
- **Spacing inside controls.** Steps of 2, 4, 8 and 12 px. Icon to text gap 4 px. Button text padding 8 px left and right. Toolbar
  button padding 6 px. Menu items keep a 16 px icon column. Spacing between controls does not change (decision 1).

### 3.4 Fonts

| Use | WinForms | WPF | Today |
|---|---|---|---|
| Body: labels, inputs, buttons, menus, grid cells | Segoe UI 9 pt (UQ-1) | Segoe UI 11.5 DIP (unchanged) | Microsoft Sans Serif 8.25 |
| Bold text set in designers | Segoe UI Bold, same rule | unchanged | Microsoft Sans Serif Bold |
| Self-drawn titles: grid title, group titles drawn in code | Segoe UI Semibold 10 pt | Segoe UI Semibold 14 DIP (grid title today) | sans serif 10 bold |
| Window title | Segoe UI 9 pt; the main window keeps its larger title size | same (FormFrame) | Microsoft Sans Serif |
| Module bar captions | Segoe UI 8 pt | – | Arial 8 |

Segoe UI 9 pt and 11.5 DIP draw the same at 96 DPI (measured: Font.Height 16, "Ag" 22×15 px), so WinForms windows and WPF Frms match.

**Rule for other sizes in designers.** Microsoft Sans Serif at 8, 8.25 or 8.5 becomes the body size. Any other size keeps its number
(10 stays 10, in Segoe UI). Style is kept in full: Bold, Italic and Underline. Other families stay: Courier New (code), Wingdings
(symbols), Calibri, and Arial where a designer chose it on purpose (55 Arial 8.25 Bold Italic headings stay Arial unless G-UI3 shows they
look wrong).

### 3.5 Icon sizes

Fluent icons come in 12, 16, 20, 24, 28, 32 and 48 px designs. The program's slots are 16, 20, 22 and 32 px (plus a few odd ones).

| Slot | Design used | Where |
|---|---|---|
| 16 px | 16 | menus, trees, small buttons, grid sort mark |
| 20-23 px | 20, centred | toolbar buttons, buttons (the 22 px slot) |
| 24-31 px | 24 | buttons and toolbars above 100% scaling |
| 32-47 px | 32 | module bar |
| 48 px and up | 48 | module bar at 150% and above |

The rule: at the slot's real pixel size (after DPI scaling), draw the largest design that fits, centred. Never stretch a design up.

### 3.6 Control by control

Nothing here moves a control. Every change is how the control paints inside its own bounds.

- **Buttons** (`UI.Button`, WPF `Button`). Flat `SurfaceRaised` fill, 1 px `Stroke` border, 4 px corners, `Text`. Hover `ButtonHover`,
  pressed `ButtonPressed`. Focus: 2 px `Accent` ring inside the border. Disabled: `#F5F5F5` fill, `TextDisabled`. The form's
  `AcceptButton` (set on 87 designer lines) is filled `Accent` with white text, `AccentHover` and `AccentPressed`. The icon stays on the
  left in the same image rectangle, tinted `Text` (or its role colour, UQ-4). No gradient and no text glow.
- **Toolbars** (`ToolBarOD`, WPF `ToolBar` and `ToolBarButton`). `SurfaceSubtle` background, 1 px `Gridline` line at the bottom.
  Buttons are transparent until hover (`SubtleHover`, 4 px corners) or press (`SubtlePressed`). Toggled: `AccentSubtle` with a 1 px
  `Accent` border. Drop-down arrow: Fluent chevron, 12 px. Separators: 1 px `Stroke`, inset 4 px. Error state: `DangerSubtle` with
  `Danger` text. Notify state: `WarningSubtle` with `Warning` text. Button widths still come from their text (they grow about 7% with
  Segoe UI; the scan checks the right edge).
- **Grids** (`GridOD`, WPF `Grid`). Title: flat `Accent`, white Segoe UI Semibold. Header: `SurfaceSubtle`, `Text`, 1 px `Stroke`
  column separators and bottom line. Cells: `SurfaceRaised`, 1 px `Gridline` lines. Hover `RowHover`, selection `AccentSubtle` with
  `Text`. Outer border 1 px `Stroke` (today blue). Row colours set by the program or the office (definitions, status colours) still win.
  Row height follows the font (UQ-2). Printing keeps today's look (UQ-5).
- **Menus** (`MenuOD`, stock `ContextMenuStrip`, WPF `Menu` and `ContextMenu`). Menu bar on `Surface`. Drop-downs `SurfaceRaised` with
  a 1 px `Stroke` border. Item hover `SubtleHover`, 2 px corners. Separators `Gridline`. Check marks and item icons are Fluent 16 px.
  One global `ToolStripManager.Renderer` with a colour table from the tokens restyles all 102 stock context menus at once.
- **Module bar** (`ModuleBar`). `SurfaceSubtle` background with a 1 px `Gridline` divider on its right. Hover `SubtleHover` with 6 px
  corners, inset 4 px inside the button's bounds. Selected: `SurfaceRaised` fill, a 3 px `Accent` mark on the left, the icon in its
  Filled style tinted `Accent`, caption in `Text`. Unselected icons Regular style in `Text`. Captions Segoe UI 8.
- **Tabs** (`UI.TabControl`, WPF `TabControl`, 11 stock `TabControl`s keep the Windows look). Headers keep their sizes. Unselected:
  transparent, `TextSecondary`. Hover `SubtleHover`. Selected: `Text` and a 2 px `Accent` line along the header's bottom. Page border
  1 px `Stroke`, page background `Surface`.
- **Group boxes and panels.** 1 px `Stroke`, 6 px corners, title in `Text` on the panel's background.
- **Inputs.** Stock TextBox and the program's TextBox subclasses keep the Windows 11 look and take the new font; their `BorderStyle` does
  not change, because that changes the text area. `UI.ComboBox`: `SurfaceRaised`, 1 px `StrokeStrong` border, 4 px corners, Fluent
  chevron; hover border `StrokeStrongHover`; focus 2 px `Accent` line inside the bottom edge. `UI.ListBox`: `SurfaceRaised`, `StrokeStrong`
  border, hover `RowHover`, selection `AccentSubtle`.
- **Check boxes** (`UI.CheckBox`, WPF `CheckBox`). Same box size as today. Empty: white with a 1 px `StrokeStrong` border and 2 px
  corners. Checked: `Accent` fill with a white Fluent check mark. Indeterminate: `Accent` fill with a white dash. Hover border `StrokeStrongHover`.
  Stock RadioButtons keep the Windows look.
- **Calendars and pickers** (`MonthCalendarOD`, `ODDatePicker`, `ODDateRangePicker`). Selected day `Accent` with white text. Today: a
  1 px `Accent` ring (today orange). Arrows: Fluent chevrons.
- **Splitters.** A 1 px `Gridline` line in the middle of the splitter; `Accent` while dragging.
- **Tooltips** (`ToolTipOD`, WPF `ToolTip`). `SurfaceRaised`, 1 px `Stroke`, 4 px corners, `Text`.
- **Window frame** (`FormODBase`, `FormFrame`). Title bar `Accent` (or the office's Misc Colors), white title text. Caption buttons
  flat: minimise and maximise hover `CaptionHover`, close hover `Danger`. Caption glyphs are Fluent (Subtract, Maximize, Square Multiple,
  Dismiss). 1 px outline in `AccentPressed`. Window background `Surface`.
- **Dialogs.** Windows message boxes stay as they are. The program's own small dialogs (input boxes, `MsgBoxCopyPaste`, progress
  windows) are ordinary windows and get the frame and controls above. Progress bars in self-drawn progress windows fill with `Accent`.

### 3.7 What makes sure nothing moves

- No package edits `Location`, `Dock`, `Anchor`, `Margin`, `Padding`, `AutoSize` or `TabIndex` in a designer.
- `Size` changes only in UI-10's fit fixes, each one listed (section 5).
- A layout snapshot test (UI-0) records every designer control's position and size and fails on any change that is not in the fit list.
- Focus rings, selection marks and hover shapes are drawn inside the control's own rectangle.

---

## 4. Icons

### 4.1 The version and the download

- **Source.** Microsoft's Fluent UI System Icons, https://github.com/microsoft/fluentui-system-icons, MIT licence, "Copyright (c) 2020
  Microsoft Corporation". The repository has no GitHub Releases; versions are its packages.
- **Version.** The newest `@fluentui/svg-icons` package on the day UI-7 runs (rule: always the latest). On 2026-11-17 that is 1.1.346,
  published 2026-11-13 (release commit `67e6dec1e`; the repository's main was `41089846a` on 2026-11-16).
- **What is vendored.** Only the SVG files the manifest uses, in `third-party/fluentui-system-icons/`, with the `LICENSE`, the version,
  the download URL and the package's integrity hash in a `SOURCE.txt`. Not the whole set of 3,002 icons, and not the fonts.
- **Notices.** An entry in `THIRD-PARTY-NOTICES.txt` and in the About box's notices.
- **Could not determine:** the folder layout inside the npm package (the repository keeps `assets/<Name>/SVG/ic_fluent_<name>_<size>_<style>.svg`).
  UI-7 records the layout it finds.

### 4.2 How the icons are built into the program

Fluent icons are drawn as vectors from path data compiled into the program. No icon font and no picture files.

1. **Manifest.** `icons/manifest.json` lists every icon the program uses: its `EnumIcons` name, its Fluent name and style (or a custom
   file), the sizes it needs, and its colour role (`Text`, `Accent`, `Danger`, `Success`, `Warning`).
2. **Generator.** `tools/New-IconData.ps1` (ASCII only) reads the manifest and the vendored SVGs. It turns each path into a small list of
   drawing commands (move, line, cubic curve, close; arcs and quadratic curves become cubic curves) and writes
   `CodeBase/UI/Icons/IconData.g.cs`. The output is deterministic and committed.
3. **Drawing.**
   - WinForms: a GDI+ `GraphicsPath` from the commands, filled with antialiasing. Where `IconLibrary` draws through Direct2D today, the
     same commands feed `Direct2d`'s path calls (`BeginPath`, `BeginFigure`, `AddLine`, `AddBezier`), so those icons stay on Direct2D and
     the OnlyGDI fallback (`IconLibrary.cs:62, 168, 204`) uses GDI+.
   - WPF: a frozen `StreamGeometry` from the same commands.
   - Every drawn icon is cached per icon, pixel size, colour and state.
4. **The API stays.** `EnumIcons`, `IconLibrary.Draw`, `DrawDisabled` and `DrawWpf` keep their signatures, so the 429 code and 88 XAML
   call sites do not change. The two `IconLibrary` copies share the one table in CodeBase. The 38 + 37 `Gen_*.cs` files go.
5. **Bitmaps where a bitmap is required.** `IconLibrary.GetBitmap(icon, px, colour)` and `GetImageList(icons, px)` make 32-bit images
   at the current DPI for ImageLists, TreeViews, ListViews and designer `Image` properties. They are rebuilt when the DPI changes.
6. **Disabled icons** are drawn in `TextDisabled`, not greyed with `ControlPaint`.

Why not the alternatives: the icon font is 2.9 MB per style, its glyph numbers probably shift between versions, GDI text drawing blurs it,
and it cannot hold our dental icons. Pre-rendered PNGs would mean about 7 files per icon per style and blur at in-between scales.

### 4.3 The mapping

- **Approach.** One row per purpose, not per picture. Duplicates collapse: the same up, down, left, right, print, delete, add, export and
  patient pictures appear in many lists. The survey estimates 120 to 160 distinct purposes (not exact; the ImageList pictures were not
  all compared one by one).
- **Order.** First the 38 + 53 `EnumIcons`. Then new `EnumIcons` members for the bitmap purposes (Up, Down, PrintSmall, Export,
  ExportExcel, Label, Edit, Preview, Pin, ZoomIn, ZoomOut, Paste and others). Then the ImageList and per-form pictures.
- **Style.** Regular for everything, Filled for "on" states (the selected module, a toggled toolbar button).
- **Colour.** Single colour, by role (UQ-4).
- **Examples** (names checked in the Fluent assets): Appts → Calendar LTR; Family → People; Account → Receipt Money; Tx Plan → Clipboard
  Task List LTR (medical variant Clipboard Heart); Imaging → Image Multiple; Manage → Briefcase; Add → Add; DeleteX → Dismiss or Delete;
  Print → Print; print preview → Document Print; Export → Arrow Export LTR; up/down/left/right → Arrow or Chevron; Email → Mail; CommLog
  → Chat; Rapid Call → Call; insurance → Umbrella (as Open Dental) or Shield Checkmark; deposit → Building Bank; tasks → Clipboard Task
  List; setup → Settings; break appointment → Calendar Cancel; imaging tools → Crop, Flip Horizontal, Arrow Rotate, Hand Right, Ruler,
  Pen, Eraser, Scan, Camera, Folder; Rx → Pill; medical → Stethoscope or Heart Pulse; signature → Signature.
- **The table** with every purpose, today's picture, the chosen icon and its source is `icons/README.md`, filled by UI-7 and UI-9 and shown
  in G-UI1 part 3.

### 4.4 Custom dental icons, in the same style

Fluent has a "Dentist" icon and general medical icons, but no tooth chart, perio, probe, implant, X-ray type, sensor or mount icons. We
draw those ourselves.

- **Which.** The tooth (Chart module, with its medical and "hot" variants); perio chart and probe; X-ray types (bitewing, PA, pano, ceph;
  Fluent's Xray is a chest film); intraoral sensor and phosphor plate; mount (a grid of X-rays); intraoral photo; implant; ortho; and new
  default procedure-button pictures (amalgam, composite, PFM crown, ceramic crown, RCT, RCT with build-up, PA).
- **Style.** Drawn on Fluent's 16, 20, 24 and 32 px grids, with the stroke weight of Fluent Regular icons of the same size, rounded ends
  and corners, one colour, and a Filled twin.
- **Where.** `icons/custom/*.svg`, our own work under the program's GPL, like the logo (`branding/README.md`). `icons/README.md` records
  that they are original. No shapes are copied from Open Dental's pictures or from IconOrganizer art.
- **First step.** Look at Fluent's "Dentist" icon. If it is a plain tooth, it may serve as the Chart module icon. The survey could not
  render it.
- **Kept apart from the chart.** These icons are buttons and labels. The tooth chart's own drawing does not change (decision 5).

### 4.5 What is not an icon and stays

- The program's logo, window icons, splash, installer pictures and the About logo (`branding/`; checked by `BrandingTests`).
- Forms and document backgrounds: the ADA claim form (`1500_02_12.gif`), the default sheet backgrounds (Med History, Patient Info), the UDS
  report pictures, `ApptBackTest.gif`, the stationery sample.
- The tooth chart: its drawing, its 25 `.obj` models, the hidden 2D picture box, and its test baselines.
- Imaging content: thumbnails, patient images, previews.
- Vendor and partner logos (Pearl, CareCredit, Practice Booster, DentalIntel, TSI and others): third-party marks, never redrawn.
- Database pictures: procedure buttons and program links an office has set, and the 6 seeded vendor logos. The refresh never changes an
  office's rows. New default procedure-button art goes only into `db/init/new-database.sql` (UQ-7).
- The 17 cursors; the report allocator sample animation (`SnuffLight1-8.gif`).
- `loadingAnim.gif` (the spinner) may become a vector progress ring in UI-11; until then it stays.

---

## 5. The font change and the text-fit scan

### 5.1 Where the font is set

UI-2 adds one font token (`UiTheme.FontFamilyName` = "Segoe UI", `UiTheme.FontSizeBody` = 9 pt, or 8.25 after UQ-1) and changes:

- the 12 exe `Main`s' `SetDefaultFont` calls: `OpenPmsDental/Main Modules/ProgramEntry.cs:28`, `CentralManager/Program.cs:16`,
  `DatabaseIntegrityCheck/Program.cs:15`, `DocumentationBuilder/Program.cs:15`, `MySQLManager/Program.cs:17`,
  `OpenPmsDentalDbSettings/Program.cs:20`, `OpenPmsDentalHL7/Program.cs:20`, `OpenPmsDentalHelper32/Program.cs:25`,
  `RdlDesign/RdlDesigner.cs:1284`, `ServiceManager/Program.cs:16`, `UpdateFileCopier/Program.cs:14`, `xCrudGenerator/Program.cs:17`.
  Each keeps a literal `new System.Drawing.Font("Segoe UI",9f)` as its first statement, so an exe does not need to reach CodeBase before
  any window exists; a test checks the literal equals the token. `SetDefaultFont` still applies the Windows text scale factor.
- `FormODBase.cs:137`, `LayoutManagerForms.FontInitial` (`LayoutManagerForms.cs:84`), `FormFrame.cs:104`, and the title fonts
  (`FormODBase.cs:806-807`, `FormFrame.cs:627-628`).
- **A family mapping in `LayoutManagerForms`** (`AddToList96` at line 226 and the font part of `LayoutControlBoundsAndFonts`, 868-914):
  Microsoft Sans Serif becomes the token font by the rule of 3.4, keeping Bold, Italic and Underline (today the rebuild keeps only Bold).
  Every FormODBase window passes through this one place, so the 418 designer fonts in 134 files change without editing designer files.
- The 78 plain forms that do not use `LayoutManager` (RdlDesign 21-22, CentralManager 18, OpenPmsDental 5, OpenPmsDentalGraph 5,
  ServiceManager 3, others): RdlDesign's 61 designer fonts and the others are edited in their designers.
- Code that creates screen fonts: `ToolBarOD.cs:89`, `UI/ListBox.cs:391`, `ODButtonPanel.cs:64`, `ModuleBar.cs:323`, the appointment
  panels (`UI/Appt/ControlApptPanel.cs`, `ControlApptProvSlider.cs`), `ControlAppt.cs:3182, 3187`, `FormClaimEdit.cs:2506, 2509`,
  `FormPerio.cs:1170`, and the rest of the about 59 code lines and about 40 `GenericSansSerif` files that draw on screen. Each brief
  lists its sites from a fresh grep.
- GridOD's India setting switches the grid to Arial (`GridOD.cs:1408`). UI-5 checks whether Segoe UI draws the rupee sign, the likely
  reason; if it does, the switch goes.

### 5.2 What does not change

- **The tooth chart.** `SparksToothChart/ToothChart2D.cs:296-378` draws tooth numbers and "W" with the control's own font, which comes
  from the default. UI-2 pins `ToothChart2D`'s font to Microsoft Sans Serif 8.25 in the product, before the default changes.
  `ToothChartData.cs:87` (GenericSansSerif 8.25), `ToothChartWrapper.cs:431` (Arial 9) and the WPF chart's Arial stay as they are.
- **Printouts and documents.** Sheets, statements, claim forms, reports, RDL, MigraDoc and PDF output keep their own fonts (stored per
  field, or set in their print code). `PdfSharpSetup.FALLBACK_FONT_FAMILY` stays. The sheet, claim form and sheet-field design windows
  show documents, so their canvas stays.
- **GridOD's print path.** `PrintPage` (line 997), `SheetDrawRow`, `SheetDrawTitle`, `SheetDrawHeader` and their PDF variants
  (2960, 3193, 3275) draw with the screen fonts today. UI-2 first gives them their own print fonts, pinned to today's sans serif 8.5 and
  10 Bold and today's print colours, and only then changes the screen fonts (UQ-5). `GridOld.cs` and `Tables Old Grids/ContrTable.cs`
  get the same check.

### 5.3 How big the problem is

Measured on the build PC at 96 DPI, against Microsoft Sans Serif 8.25, over the real texts in the designers:

| | Segoe UI 9 | Segoe UI 8.25 |
|---|---|---|
| Width (standard controls, median) | +10% | +7% |
| Width (the program's self-drawn controls, median) | +7% | −2% |
| Line height | 15 px (today 13) | 13 px |
| Grid row height | 17 px (today 14) | 16 px |
| Fixed-size controls that newly overflow (of 10,452) | 1,591 in 422 files | 391 in 182 files |
| ... only in height (most by 1-2 px) | 948 | 162 |
| ... need more width | 557 | 206 |
| ... need another line | 86 | 23 |

The biggest files at 9 pt: FormPatientEdit 75, FormProvEdit 35, FormClaimEdit 31, FormProcEdit 26, ControlAccount 26, the preference
pages 15-19 each. Most height-only cases are check boxes 16-17 px tall and labels 13-14 px tall; many may not clip visibly. These counts
come from a static model (designer text and size only), not from running the program.

### 5.4 The scan

**Tier 1, no database** (UI-0, `tests/UiRefreshTests`). In its own process, it sets the font under test as the default, then builds every
Form and UserControl type (about 892 forms and 153 UserControls) like this: create an empty object, run only the base constructor
(FormODBase, Form or UserControl), then call the type's `InitializeComponent` and `InitializeLayoutManager` by reflection. This skips each
window's own constructor, so constructor arguments and database calls do not matter. Types that throw are listed.

It then walks every visible control with text and measures it with the control's own way of drawing:
- `TextRenderer` for stock Label, LinkLabel, RadioButton, CheckBox, Button and GroupBox;
- `Graphics.MeasureString` for `UI.Button` (`Button.cs:345-361`), `UI.CheckBox` (`CheckBox.cs:206-268`) and `UI.GroupBox`
  (`GroupBox.cs:128-167`).

For the 1-2 px height cases it draws the control into a bitmap and looks for ink in the last row or column. It runs at 96 DPI, at 150%
and at one program zoom step.

Output: a TSV of new overflows only, with the extra pixels needed and whether free space exists to the right or below inside the parent
without touching a neighbour. Each row is "fixable by size only" or "needs the owner".

**Tier 2, with real data** (UI-0, the harness). A walker in the new harness group U measures every window the harness opens, after `Load`,
so text set at run time is covered (about 975 such assignments in 241 files).

**The fixes** (UI-10). Size-only edits to `Width` and `Height` in designers, batched by file, from the scan's TSV. A fix that would touch a
neighbour goes to the owner with a picture. Every fix is listed in `tests/UiRefreshTests/Data/fit-fixes.tsv`, which the layout snapshot
test reads (3.7).

---

## 6. What is removed

**The classic look (decision 2).** The old drawing is replaced in place, control by control: gradients, the button text glow, the blue
grid outline, the bitmap toolbar pictures. There is no switch back.

**Stored settings: nothing to do.**
- `PrefName.ColorTheme` (`Pref.cs:745`, marked Deprecated, value 0 in `new-database.sql`): no code reads it. The row stays.
- `PrefName.ThemeSetByUser` (`Pref.cs:2423`, Deprecated): no code reads it. The row stays.
- `userodpref` rows with `FkeyType` 17 (`UserTheme`, `UserOdPref.cs:73`, Deprecated): only the old conversion `To19_3_9` in
  `ConvertDatabases6.cs:2408` touches them. The rows stay.
- These stay because Open Dental keeps them too, and an office's database comes from Open Dental (no migration, owner 2026-11-15). No
  database conversion, no new setting, no new preference.
- **Misc Colors 11, 12 and 13 stay** (Main Border, Main Border Outline, Main Border Text). They keep working as the office's title bar
  override. When all three hold Open Dental's stock values (-12493158, -16777216, -1), the program treats them as "not customised" and
  uses the new tokens (UQ-3). The Definitions window keeps editing them, unchanged.
- `ComputerPref.Zoom` and all other settings are unaffected.

**Code and files that go.**
- The 38 WinForms and 37 WPF `IconLibrary/Gen_*.cs` files (replaced by the generated table).
- Each bitmap whose last use a package replaces: the `Resources.resx` entry, the image file and the ImageList stream, in the package that
  replaces the last use. Bitmaps that no code uses today stay (section 1).
- The leftover comments about `ODColorTheme` (`FormODBase.cs:363`, `GridOD.cs:1761`) and the commented-out `OdThemeModernGrey` line in
  `FormButtonTest.cs:37`.
- `ColorOD` stays as a thin layer over the tokens, so its 162 uses keep working.

---

## 7. Packages

**Rules for all packages.**
- One writer per package, in its own worktree, on its own branch. Packages that share a control file run one after the other.
- Each package runs Targeted tests (NoHqTests included). Core and Full runs are workflow steps, one at a time.
- A package merges only after its gallery is approved (section 8).
- Until the last package, `main` shows a mixed look. Nothing is in production (owner, 2026-11-15), so that is accepted. A release cut in
  that window says so in its notes.
- Designer files are also edited by the Open Dental 26.2 port. Before it starts, each package checks its files against the open port
  branches and waits for the overlapping ones.

**UI-0: tools, scans and the before set** (tests and tools only; no product change)
- Owns: `tools/New-UiGallery.ps1` (the in-repository successor of the lost `Make-Pairs.ps1`), `tools/ui-gallery-masks.tsv`, the new test
  project `tests/UiRefreshTests` (the lead adds it to `OpenDental.sln` and `OpenDental.Local.slnf`), `tests/WindowChecksTests` group U
  (`InProc/InProcChecks.U.cs`, `UiGallery/windows.tsv`, the walker) and `tests/WindowChecksTests/UiApproved/approved.tsv` (empty).
- Work: the gallery builder (before, after, diff, % changed, the after picture's pixel SHA-256, masks for the clock, dates and the copy's
  database name, clip notes, `index.html`, a zip with forward slashes). Group U: about 50 common windows the harness does not reach yet
  (claims, recall, confirmation, schedules, pay plans, billing, procedure codes, definitions, security, tasks, medical, Rx, labs and
  others listed by the tooling survey), opened by their own handlers on synthetic patients and closed with Cancel; the main window at
  fixed sizes 1366×768 and 1920×1080; each module with and without a patient. The tier 1 text-fit scan (5.4). The layout snapshot
  (`tests/UiRefreshTests/Data/layout-snapshot.tsv`) and its test. The check U0 (compare each picture with `approved.tsv`).
- Tests: the builder's self-tests (no program, no database); the scan runs on today's font and reports nothing new.
- Gallery: none. It ends with **the before set**: one full harness run (groups A to K and U) at the base commit, in the owner's connected
  session, frozen in `.git/od-tools/test-results/ui-before/` with the commit, the dump's hash and the DPI.

**UI-1: tokens and the design sheet** (no visible change in the program)
- Owns: `CodeBase/UI/UiTheme.cs` (new: colours as `System.Drawing.Color`, frozen WPF brushes, cached GDI+ pens and brushes that callers
  never dispose, font family and sizes, corners, lines, spacing, icon sizes), `WpfControlsOD/UI/ColorOD.cs` (becomes a facade over
  `UiTheme`), `tests/UiRefreshTests/DesignSheet*.cs`, a NoHq source scan.
- Work: the tokens of section 3. The design sheet: a test without database that renders the swatches with names, hex values and contrast
  ratios; the type ramp; the spacing and corners; and a drawing of every control state made from the tokens (normal, hover, pressed,
  focused, disabled, checked, selected). It shows the options of UQ-1 to UQ-4 side by side.
- Tests: contrast test (every text and border pair of 3.2 meets its ratio); the source scan that flags new `Color.FromArgb` and hex
  colours in `OpenPmsDental/UI`, `OpenPmsDentalBusiness/UI/GridOD.cs` and `WpfControlsOD/UI/Controls` beyond an allow list that shrinks
  with each package.
- Gallery: **G-UI1 part 1, the design.** Every later package follows what the owner approves here.

**UI-2: the font**
- Owns: the 12 exe `Main`s, `FormODBase.cs` (font lines only), `FormFrame.cs` (font lines only), `LayoutManagerForms.cs`,
  `SparksToothChart/ToothChart2D.cs` (font pin only), `GridOD.cs` (print/screen font split and screen fonts), `GridOld.cs` and
  `ContrTable.cs` if they print, the plain forms' designer fonts, the screen font sites of 5.1, the font tests of section 9, and a note
  in `docs/specs/net10-plan.md` (decision 9, the S3b-4 row and the "Default font" row) that this plan replaces them.
- Work: section 5.1 and 5.2. Print fonts are split out and pinned before any screen font changes.
- Tests: section 9's font group; the print guard (GridOD print and sheet output identical before and after); the tooth chart baselines
  unchanged with the test default font set to the new font; the scan's report attached to the brief's result.
- Gallery: **G-UI2, font pass** (main window and modules), with the scan's counts.

**UI-3: the window frame**
- Owns: `FormODBase.cs` and `FormFrame.cs` (frame drawing). Ideally one shared border painter in a new file both call
  (`OpenPmsDental/UI/BordersDpi/` cannot be reached from `WpfControlsOD`, so it lives in `WpfControlsOD/Frame/FramePainter.cs` or in
  CodeBase; the brief decides from the project references).
- Work: 3.6 window frame; Misc Colors as override with the stock-values rule (UQ-3); try DWM rounded corners
  (`DWMWA_WINDOW_CORNER_PREFERENCE`) with the program's own non-client area and keep them only if the harness pictures are clean.
- Tests: a frame test that both painters give the same pixels for the same title; the Misc Colors override test (stock values give the
  token, custom values give the office's colours).
- Gallery: **G-UI2** (main window, modules, a few dialogs).

**UI-4: core controls**
- Owns: `OpenPmsDental/UI/Button.cs`, `CheckBox.cs`, `ComboBox.cs`, `ListBox.cs`, `GroupBox.cs`, `TabControl/*`, `Splitters/*`,
  `MonthCalendarOD`, `ToolTipOD`, `PanelOD`, `ComboBoxClinicPicker`, `ODDatePicker`, `ODDateRangePicker`, the progress windows'
  drawing; WPF twins in `WpfControlsOD/UI/Controls` (Button, CheckBox, ComboBox, ListBox, GroupBox, TabControl, TextBox templates,
  MonthCalendar, ToolTip, SplitContainer), `Generic.xaml`, `TabControlDict.xaml`, `DataGridDict.xaml`.
- Work: 3.6 for each control, WinForms and WPF together, so both look the same. Bounds unchanged.
- Tests: per control, a picture test of each state against the design sheet's drawing (tolerance as in `ChartImageCompare`); the
  layout snapshot; `TerminologyLayoutTests` and `LayoutDesignerTests` in the new font.
- Gallery: **G-UI1 part 2** (the real controls in every state, WinForms beside WPF) and G-UI2.

**UI-5: grids**
- Owns: `OpenPmsDentalBusiness/UI/GridOD.cs` (screen drawing), `GridOld.cs` if still used, `WpfControlsOD/UI/Controls/Grid.xaml(.cs)`.
- Work: 3.6 grids. Header and title heights follow the font, inside the control. Row height by UQ-2. Print colours stay.
- Tests: `BrandingTests.BrandColor_IsTheProgramsGridTitleBlue` moves to the token; the print guard; a grid picture test of title, header,
  hover, selection, a coloured row.
- Gallery: G-UI1 part 2 and G-UI2.

**UI-6: toolbars, menus and the module bar**
- Owns: `ToolBarOD.cs`, `WpfControlsOD/UI/Controls/ToolBar*.xaml(.cs)`, `MenuOD.cs` (`MenuRenderer`), WPF `Menu`, `MenuItem`,
  `ContextMenu`, `ModuleBar.cs`, `ODButtonPanel.cs`, `LightSignalGrid`, and the global `ToolStripManager.Renderer` set in
  `ProgramEntry.cs`.
- Work: 3.6 toolbars, menus, module bar.
- Tests: C20 (MenuOD font equals the form's font); a toolbar test that no module toolbar runs past its right edge at 1366 px; picture
  tests of each state.
- Gallery: G-UI2.

**UI-7: the icon pipeline and the IconLibrary icons**
- Owns: `third-party/fluentui-system-icons/`, `icons/manifest.json`, `icons/README.md`, `tools/New-IconData.ps1`,
  `CodeBase/UI/Icons/*`, both `IconLibrary` folders, `THIRD-PARTY-NOTICES.txt`, the About box's notices.
- Work: section 4.1 and 4.2. Map the 38 + 53 `EnumIcons` (module bar included; Chart32G, Chart32W and ChartMed32 keep today's pictures
  until UI-8).
- Tests: every `EnumIcons` has a manifest row and draws non-empty at 16, 20, 22, 24, 32 and 48 px on both GDI+ and Direct2D; the
  generated file matches the manifest and the SVGs; the Fluent licence and notice are present; the OnlyGDI path draws the same icons.
- Gallery: **G-UI1 part 3** (today's icon beside the Fluent icon, 16/20/24 px, 100/125/150/200%).

**UI-8: custom dental icons**
- Owns: `icons/custom/*.svg`, their manifest rows, `db/init/new-database.sql` (default procedure-button pictures only, if UQ-7 says yes).
- Work: section 4.4.
- Tests: as UI-7; a test that the seeded procedure-button pictures are 20×20; a test that no office row is changed by any code path (the
  pictures are only in the new-database script).
- Gallery: **G-UI1 part 4** (each dental icon at each size, beside today's picture).

**UI-9a: bitmap pictures to icons** (Resources and per-form pictures)
- Owns: the 186 files with `Image=Properties.Resources.*` (361 assignments), the 70 per-form `.resx` pictures, the 11 WPF
  `BitmapFileName` uses, the 7 WPF resource pictures, and the `Resources.resx` entries they free.
- Work: switch to `UI.Button.Icon=EnumIcons...` where the control supports it, otherwise `IconLibrary.GetBitmap` at the DPI size.
  Designer files edited carefully; positions unchanged. Batched by folder if the brief finds it too big.
- Tests: no designer `Image=` points to a removed picture; `RenameGuardTests` (pack URIs) updated; the layout snapshot.
- Gallery: G-UI3.

**UI-9b: ImageLists to icons** (toolbars and trees)
- Owns: the 50 `.resx` files with ImageList streams (55 lists, 244 pictures), the 318 `ODToolBarButton` calls, the TreeViews (imaging
  tree, tasks, setup wizard, security tree, auto notes, email inbox), `OpenPmsDental/Logic/ProgramL.cs` (program-link pictures keep
  working).
- Work: toolbar buttons take `EnumIcons` instead of an index; trees get a DPI-sized ImageList from `GetImageList`.
- Tests: `SourceScanTests.ImageLists_TakeTheirDepthFromAStreamOrSetIt` allows the one icon factory; the program-link pictures from the
  database still show (a test with a synthetic program row).
- Gallery: G-UI3.

**UI-10: text fit**
- Owns: the designer files named by the scan's TSV (size edits only), `tests/UiRefreshTests/Data/fit-fixes.tsv`.
- Work: section 5.4, "the fixes". Batches by file, largest first (FormPatientEdit, FormProvEdit, FormClaimEdit, FormProcEdit,
  ControlAccount, the preference pages).
- Tests: the scan reports nothing new, or every remaining item is named and approved; the layout snapshot.
- Gallery: G-UI3 (before and after pairs of each batch) and the scan's list for "needs the owner".

**UI-11: the rest and the final pass**
- Owns: the designer `BackColor` values that mean chrome rather than data (228 files set one; the brief separates chrome from data), the
  leftovers of section 6, `loadingAnim.gif` as a vector progress ring, the release notes.
- Work: the sweep of every window (G-UI4), fixes it finds, the notes: the classic look is gone; no conversion; Misc Colors still work.
- Tests: the whole Full run; the source scans' allow lists at their final size.
- Gallery: **G-UI4.**

---

## 8. Galleries and the owner's approval

**Names.** G-UI1 to G-UI4. The tooth chart's galleries are G0 to G5, so the names must not be shortened.

**How a gallery is made.** `tools/New-UiGallery.ps1` takes a before folder, an after folder and the mask file. It writes, per group:
before, after, a diff, % changed, the after picture's pixel SHA-256 and the clip notes, plus `index.html` and a zip. Pictures are
synthetic only (the harness's `winchk_` copies of the synthetic dump; no patient data). WPF draws nothing while the session is locked, so
a gallery run needs the owner's connected, unlocked session; a full set takes about 14 minutes in three runs.

**The galleries, in the order the owner approves them.**
- **G-UI1, the design sheet.** Part 1 (UI-1): tokens, type ramp, a drawing of every control state, the options of UQ-1 to UQ-4.
  Part 2 (UI-4, UI-5): the real controls in every state, WinForms beside WPF. Part 3 (UI-7): every icon, old beside new. Part 4 (UI-8):
  the dental icons. Approving part 1 sets the look for everything after it.
- **G-UI2, the main window and modules.** The main window at 1366×768 and 1920×1080, the About box, the menu bar, and the seven modules
  with and without a patient, each also at one program zoom step. The tooth chart area of the Chart module is checked as unchanged, not
  shown as a change.
- **G-UI3, the common windows.** The about 45 windows the harness already pictures plus the about 50 of group U, with the clip report.
- **G-UI4, everything else.** An off-screen sweep of every window with a parameterless constructor (582 candidates), on the scratch copy,
  with a deny list for windows that call a network or e-service on Load and a check that the database name starts with `winchk_`; plus the
  separate programs (report designer, Central Manager, tools). Each window gets a picture or a reason. Approved by exception: the owner
  looks at the list of changes, not every picture.

**How the owner answers.** The lead publishes each gallery as a private page. Each screen has "Approve" or "Change" and a note. The answers
go into `docs/owner-questions.md` (gallery, commit, date, the list of changes) and into
`tests/WindowChecksTests/UiApproved/approved.tsv` (picture, size, masked pixel SHA-256, gallery, commit, date). A package merges only
after its gallery is approved. From then on the check U0 shows only the screens that changed since their approval.

**Limits.** All pictures are at 100% (96 DPI); the harness cannot change Windows scaling. The design sheet renders 125-200% itself. Real
125% and 150% checks on a desktop are added to the launch checks (`docs/launch-checks-3b.md`, item 8, "the screenshot pass's items for a
person").

---

## 9. Tests

**Tests that change (they pin today's font or colours).**

| Test | What it pins | Change | Package |
|---|---|---|---|
| `NoHqTests/DefaultFontTests.cs` | `LINE_FONT` regex (line 30) for Microsoft Sans Serif 8.25 in 12 `Main`s; the runtime probe (line 93) | Segoe UI and the token size; the rule "first statement" stays | UI-2 |
| `tests/LibWebTests/HtmlGenerationTests.cs:233` | the old `SetDefaultFont` line | the new line | UI-2 |
| `tests/ToothChartImageTests/WpfLiveViewTests.cs:76` | the old `SetDefaultFont` line | the new line | UI-2 |
| `tests/ToothChartImageTests/Support/TestAssemblySetup.cs:20`, `tests/ToothChartHostTests/HostDb.cs:47` | default font Microsoft Sans Serif in the test process | the program's new default, so the 35 Simple 2D baselines prove the chart's pinned font | UI-2 |
| `UnitTests/Misc/TestBase.cs:92` | the old default | the new default | UI-2 |
| `tests/WindowChecksTests/InProc/InProcChecks.cs:28` | the old default in the in-process session | the new default | UI-2 |
| `InProcChecks.W5.cs:22-31` (`ExpectDefaultFont`, used by C16, C17, C21, F2, F4, G5) and `CTests.cs:30` | "Microsoft Sans Serif" | compare with the token | UI-2 |
| `InProcChecks.W5.cs:160` (C20) | MenuOD font starts with "Microsoft Sans Serif" | the strip and its items equal the form's font | UI-2 |
| `tests/M2TERMTests/TerminologyLayoutTests.cs` | 5 check boxes measured in Microsoft Sans Serif | the new font (it may then report real clipping) | UI-2 |
| `tests/M2INSPLANTests/LayoutDesignerTests.cs:260` | FormInsPlan labels measured in Microsoft Sans Serif | the new font | UI-2 |
| `tests/AttributionTests/BrandingTests.cs` (`BrandColor_IsTheProgramsGridTitleBlue`) | the literal `_colorTitleBottom=Color.FromArgb(60,90,150);` in GridOD | `#3C5A96` as the `Accent` token; the branding files still checked | UI-1 (token), UI-5 (grid) |
| `tests/WindowChecksTests/SourceScanTests.cs` (`ImageLists_TakeTheirDepthFromAStreamOrSetIt`) | ImageLists only from designers | allow the one icon factory | UI-9b |
| `NoHqTests/RenameGuardTests.cs` | pack URIs and `BitmapFileName` | the remaining ones | UI-9a |
| `UnitTests/UI Testing` (FormButtonTest, Form4kTests, FormTestAllControls and others) | the old pictures | the icons | UI-9a |

Unaffected: the data-colour tests (`BAPPTTests` TimeBarRuleTests, `M2CARRIERTests` ApptViewAndHookTests, `TaskCategoryTests`), the
branding icon tests, and the 1,682 + 18 tooth chart baselines, which must stay identical (decision 5).

**New tests.**
- `tests/UiRefreshTests` (UI-0): the layout snapshot test; the tier 1 text-fit scan; the design sheet (UI-1); the contrast test (UI-1);
  control state pictures (UI-4 to UI-6).
- NoHq source scans: no new hard-coded colours in the shared controls (UI-1); no "Microsoft Sans Serif" outside an allow list (the tooth
  chart, print code, document design canvases) (UI-2); no bitmap toolbar pictures outside an allow list (UI-9b); each exe's font
  literal equals the token (UI-2).
- Print guard (UI-2): GridOD's print and sheet output, sheets, claim forms, reports, RDL and MigraDoc output identical before and after.
- Icon tests (UI-7, UI-8): manifest complete, every icon draws at every size on GDI+, Direct2D and WPF; generated file in step; notices
  present.
- Harness (UI-0): group U, the walker, the check U0.

---

## 10. Risks

- **Text that no longer fits.** Segoe UI is wider and taller. 391 to 1,591 fixed-size controls newly overflow (5.3), and some fixes may
  not fit without touching a neighbour, which decision 1 forbids. Mitigations: the two-tier scan with an ink check, size-only fixes, the
  owner sees every case that needs more, and UQ-1's smaller size if the count is too high.
- **Fewer grid rows.** Rows grow from 14 to 16 or 17 px, about 15-20% fewer rows on screen. Mitigation: UQ-2, shown in G-UI2.
- **Toolbars run out of room.** Toolbar buttons size from their text and get about 7% wider; full module toolbars may hit the right edge
  at 1024-1366 px. Mitigation: UI-6's right-edge test; shorter spacing inside buttons if needed.
- **Printouts change by accident.** GridOD prints with its screen fonts today. Mitigation: print fonts split and pinned first (UI-2),
  and the print guard.
- **The tooth chart changes by accident.** The chart test assemblies set Microsoft Sans Serif themselves, so a Simple 2D change would
  pass unseen. Mitigation: the font pin in the product and the test default set to the new font, in the same package.
- **WinForms and WPF drift apart.** The frame, icon library, grid and toolbar each have two copies. Mitigation: one token class, one icon
  table, and each pair restyled in one package.
- **Shared pens and brushes disposed by mistake.** Paint code that wraps a cached token pen in `using` would break it for everyone.
  Mitigation: the tokens hand out pens and brushes that ignore `Dispose` or a source scan for `using` around them.
- **Blank icons on some PCs.** The Direct2D path fails on some machines (the OnlyGDI fallback). Mitigation: every icon test runs on GDI+
  too.
- **Dental icons that look worse than Open Dental's.** Mitigation: a design pass and G-UI1 part 4 before they replace anything.
- **Staff miss the colours.** Open Dental's icons are colourful (red X, blue check, yellow umbrella). Mitigation: role colours (UQ-4).
- **Gallery noise.** 92 of WINCHK-5's 145 picture pairs differed, mostly from the clock and the copy's name. Mitigation: masks and fixed
  window sizes (UI-0).
- **The off-screen sweep runs Load code that writes or calls out.** Mitigation: scratch copy only, database name check, deny list.
- **Merge conflicts with the 26.2 port.** Both touch designer files. Mitigation: the overlap check before each package (section 7).
- **Not determined by the surveys.** Whether DWM rounded corners work with the program's own title bar; what the .NET 10 diagnostic
  WFO5003 covers; how stock TreeView and ListView scale ImageLists at high DPI; whether the report designer, Central Manager, MySQL
  Manager and Database Integrity Check are in the installer (their windows are in scope either way: "Segoe UI everywhere"); whether any
  picture is looked up by a computed resource name; whether the synthetic dump has rows for every group U window (claims, recalls, pay
  plans, tasks, Rx, labs); fit above 96 DPI; texts in other languages.

---

## 11. Questions for the owner

**UQ-1. Body font size.**
- (a) Segoe UI 9 pt. **Recommended.** It is Windows' own size and matches the WPF windows exactly. About 1,591 controls need checking;
  about 640 need more width or another line.
- (b) Segoe UI 8.25 pt. Same line height as today and about a quarter of the fixes (391), but smaller than the WPF windows.
- G-UI1 part 1 shows both on the same windows.

**UQ-2. Grid row height.**
- (a) Rows follow the body font: 17 px at 9 pt (today 14). About 20% fewer rows on screen. **Recommended**: easier to read.
- (b) Compact: grid cells in Segoe UI 8.25, 16 px rows, about 13% fewer rows.

**UQ-3. Title bar.**
- (a) The program's blue (`#3C5A96`) with white text, as in Open Dental today. **Recommended**: familiar to staff and part of the brand.
- (b) A light Windows 11 title bar (`#F3F3F3`, dark text).
- Either way: an office that changed Misc Colors 11-13 keeps its colours; Open Dental's stock values count as "not customised" and give
  the new look. Do you agree with that rule?

**UQ-4. Icon colours.**
- (a) Single colour (dark grey) with a few role colours: delete red, complete green, warning amber, the accent blue for the main action.
  **Recommended**: clean and still easy to scan.
- (b) Single colour everywhere.
- (c) Keep Open Dental's colourful icons' colours on the Fluent shapes.

**UQ-5. Printed grids.** Printed and PDF grids (and grids on sheets) keep today's look and font. **Recommended**: printouts are documents
patients and payers see, and the refresh is for the screen. Or should they follow the new look?

**UQ-6. The perio chart's grid and legend, and the TextBelow strip.** Recommended: the graphical perio chart's grid (`ContrPerio`) and its
legend count as the tooth chart's own drawing and keep Open Dental's look; the TextBelow strip under the chart is an ordinary window part
and is restyled. Agree?

**UQ-7. Default procedure-button pictures.** New databases get new dental pictures for the 8 default procedure buttons (amalgam,
composite, two crowns, RCT, RCT with build-up, PA) in the new style. **Recommended.** An office's own pictures, and every database that
comes from Open Dental, are never changed. Or keep Open Dental's pictures for new databases too?
