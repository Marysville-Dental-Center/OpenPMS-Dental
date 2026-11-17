# The UI refresh: package plan

**What this is.** The plan for a new look for the whole program: modern icons, buttons and styles, clean and professional. The owner
asked for it on 2026-11-17 ("Do ui refresh. Modernize icons, buttons, styles. Keep it clean and professional") and set six decisions the
same day (section 1). Package briefs are cut from this document the same way as from `docs/specs/net10-plan.md` and
`docs/specs/wpf-tooth-chart-plan.md`.

**Date.** 2026-11-17. **Status.** Proposed, review rounds 1 to 4 applied (section 12). Waiting for the owner's answers to UQ-1 to UQ-9
(section 11) and for gallery G-UI1 part 1 (section 8), which shows the design before any window changes.

**Basis.**
- Four read-only surveys of `main` at `8d34b27a0` (2026-11-17): the shared controls and their colours, the icons, the fonts (with
  measurements on the build PC at 96 DPI), and the tooling for pictures and approval.
- The public Fluent UI System Icons repository and npm registry (checked 2026-11-17).
- The WCAG 2.2 contrast rules (level AA: 4.5:1 for normal text, 3:1 for large text, 3:1 for the outline of a control and for focus).
- Four reviews of this plan (2026-11-17). Their findings were checked against `main` at `ee264d7b5` and applied; where a review was
  wrong or a better fix exists, section 12 says why.
- Nothing was built, run or edited for this plan. Line numbers are at `8d34b27a0` (the code they point to is the same at `ee264d7b5`).
  Each brief greps them again.

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
   - Controls that size themselves grow by themselves with the new font, from the same position: AutoSize labels, radio buttons and
     check boxes, and single-line text boxes, whose height Windows takes from the font. Where that growth would touch a neighbour,
     UI-10 fixes the control's size instead (5.4).
   - Not an exception, but a consequence the owner decides (UQ-9): toolbar buttons, top-level menu items, self-drawn tab headers and
     button-panel items are laid out from their text width today. With a wider font each item grows a little and the items after it
     shift right. Their order stays.
2. **The new look replaces the classic look.** There is no classic option and no theme setting (section 6).
3. **Segoe UI everywhere.** It replaces the Microsoft Sans Serif 8.25 default font of net10-plan decision 9 (the S3b-4 row and the
   "Default font" test row of `docs/specs/net10-plan.md` section 7). This plan supersedes those lines (UI-2b adds the note there).
4. **Fluent UI System Icons** (Microsoft, MIT licence) replace the bitmap icons.
5. **The tooth chart's own drawing keeps Open Dental's look** (`docs/specs/wpf-tooth-chart-plan.md`). The window around it is
   restyled. The chart's font is pinned so the default font change cannot reach it (UI-2a).
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
- Documents on screen keep their look, like their printouts: a filled sheet (`FormSheetFillEdit`), the sheet, claim form and report
  design canvases, print previews, and the eForm fill surface (`WpfControlsOD/UI/Controls/UserControls/CtrlEFormFill.xaml`). eForms
  are patient-facing forms built from the office's eForm definitions, so they are content like sheets: UI-4a and UI-4b give
  `CtrlEFormFill` pinned copies of today's control templates, and its PDF and print output is in the print guard (section 5.2).
- HTML shown in WebView2 (`CodeBase/Controls/ODWebView2`, the wiki, email previews, web forms) carries its own CSS and fonts and is
  out of scope. Only the frame and controls around the WebView2 are restyled.
- Windows high-contrast themes win. When `SystemInformation.HighContrast` (WinForms) or `SystemParameters.HighContrast` (WPF) is set,
  the colour tokens return the matching `SystemColors` (3.2).
- The look switches that already exist keep working: `NoCustomBorders.txt`, `NoD2D.txt` and `NoDpi.txt` beside the exe (section 6).
- The two capture windows of `ImagingDeviceManager` (`ImagingDeviceManager/UI/AcquireStatusWindow.cs`, `CaptureNoticeWindow.cs`)
  are stock Forms that set `SystemFonts.MessageBoxFont` themselves (lines 28 and 20), which is already Segoe UI 9 on Windows 11. They
  keep the Windows look and the system font; window check `InProcChecks.W5b.cs:207-214` already pins that font.
- The title bar does not change colour when a window is inactive, as today (FormODBase and FormFrame have no inactive state). The one
  frame state that exists, the floating image windows' selected and unselected frame, is restyled (3.6).

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

Every file in `OpenPmsDental/UI`, `WpfControlsOD/UI` and `WpfControlsOD/Themes` has an owning package or a reason in the coverage
table (2.1).

**Shared controls (WPF).** About 35 UserControls in `WpfControlsOD/UI/Controls` with XAML templates and brushes set in code. Shared
dictionaries, in `WpfControlsOD/Themes` (not under `UI`): `Generic.xaml` (74 lines), `TabControlDict.xaml` (195), `DataGridDict.xaml` (142). Most used: Label 799, Button 487,
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
- `LayoutManagerForms` records each control's font size once, in a `Control96Info` (its constructor reads `FontSize96` at line 1391).
  It makes one in three places:
  - the constructor's loop over a window's designer controls (`LayoutManagerForms.cs:133-143`, which
    `FormODBase.InitializeLayoutManager` runs at `FormODBase.cs:653`);
  - `AddToList96` (line 211, size at 226-229), for controls added later through `LayoutManager.Add` (177), `Synch` (475) and the
    tooth chart's engine control (1118);
  - `AddToList96Unscaled` (line 245, `new Control96Info` at 246, recursing into the children), which `AddUnscaled` (203) calls for
    UserControls added with all their children at 96 DPI: every preference page (`FormPreferences.cs:578`), the setup wizard's pages
    (`FormSetupWizardProgress.cs:33`, in `Load`) and the Imaging module (`ControlImages`, `FormOpenDental.cs:604`).
- `LayoutManagerForms` rebuilds every control's font when it scales (`LayoutControlBoundsAndFonts`, lines 868-914). It keeps the
  family and changes only the size. It sets a new font only when the size differs (Job 55866: setting a font fires `TextChanged`,
  which once cleared signatures in ProcedureInfo and GroupNote), and it multiplies Label and RadioButton fonts by 0.96 (line 902). The
  new font keeps only Bold: Italic and Underline are dropped. Because of the 0.96 factor a Label's or RadioButton's size always differs
  on the first layout, so today every underlined designer label shows without its underline (for example
  `FormInsBenefits.designer.cs:451, 477, 546, 574, 584, 632` and `FormProcButtonEdit.designer.cs:114`). Open Dental has the same code
  and shows them the same way.
- 47 windows and UserControls keep `AutoScaleMode.Font` with `AutoScaleDimensions` 6F,13F, the Microsoft Sans Serif 8.25
  metric: Central Manager 13, OpenPmsDentalGraph 17, Service Manager 3, MySQL Manager 2, OpenPmsDentalDbSettings 2, the HL7
  `FormDebug`, Update File Copier, Database Integrity Check, Documentation Builder, xCrudGenerator, CodeBase `PrintPanel`,
  `ControlImageDisplay` and `ControlImageDock` (`OpenPmsDental/User Controls`), and SparksToothChart `ToothChart2D` and
  `ToothChartWrapper` (a grep for the assignment that skips comment lines, without UnitTests and PluginExample; `ODWebView2`'s line,
  `CodeBase/Controls/ODWebView2.Designer.cs:29`, is commented out). WinForms rescales such a container's
  children by the ratio of its font to 6×13 when the container is built. FormODBase, FormFrame and `LayoutManagerForms.SetNoAnchorDock`
  force `None`, but a UserControl has already scaled itself in its own constructor before `LayoutManager.Add` reaches it, and the
  plain exes' forms never pass through LayoutManager. With today's font the ratio is 1, so nothing moves today.
- More fixed fonts: 418 designer fonts in Microsoft Sans Serif in 134 files; about 59 code lines naming it outside designers;
  `FontFamily.GenericSansSerif` (which is Microsoft Sans Serif) in about 40 files, GridOD among them; Arial 8 in the module bar. The
  code sites do not all use 8.25: GridOD's cells and `ODButtonPanel` use sans serif 8.5 (`GridOD.cs:3567-3571`,
  `ODButtonPanel.cs:64`), the grid title 10 Bold (`GridOD.cs:3572`), the module bar Arial 8 (`ModuleBar.cs:323`). Of the Microsoft
  Sans Serif designer fonts, 8 are 8 pt and 2 are 8.5 pt.
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

**Look switches that exist today.** Three flag files beside the exe change the look:
- `NoCustomBorders.txt` (`ProgramEntry.cs:87`): `FormODBase.AreBordersMS` and `FormFrame.AreBordersMS`, the standard Windows frame
  instead of the program's own. Checked by window check A6 (`tests/WindowChecksTests/ATests.cs:195`, `InProcChecks.A.cs:25`) and
  `UnitTests/UnitTests/PopupHelperTests.cs`.
- `NoD2D.txt` (`FormOpenDental.cs:325`): `IconLibrary.OnlyGDI`, icons drawn with GDI+ instead of Direct2D.
- `NoDpi.txt` (`ProgramEntry.cs:83`): the program runs DPI-unaware.

**Stock controls with a look the program chooses.** 14 stock `DataGridView`s (4 in the product: `FormQuery`,
`FormEligibilityResponseDisplay` and two allocator reports; 10 in RdlDesign) and 2 stock `MenuStrip`s (`FormCentralManager`,
OpenPmsDentalGraph `FormDashboardEditTab`). No code sets `ToolStripManager.Renderer` today.

**Extra UI threads.** Some windows run on their own UI thread: progress windows (`CodeBase/Progress/ODProgress.cs:171`), the query
monitor (`FormOpenDental.cs:6396`), and others the briefs find with a fresh grep. Anything the new look shares between windows must be
safe on more than one thread (3.2).

### 2.1 Coverage: every shared control file

Every file in `OpenPmsDental/UI`, `WpfControlsOD/UI` and `WpfControlsOD/Themes` (designer, `.resx` and `.xaml.cs` files go with their
main file) has a row here with its package or the reason it keeps its look. The WPF shared dictionaries are not under `UI`, so their
rows give the full path. A NoHq test (UI-1) fails when a file in those three folders has no row, so a new control or a
new shared dictionary cannot slip through. The table lives in `tests/UiRefreshTests/Data/coverage.tsv`; this is its first version.

| Files | Package |
|---|---|
| WinForms `Button`, `CheckBox`, `GroupBox`, `PanelOD`, `ToolTipOD` | UI-4a |
| WinForms `ComboBox`, `ListBox`, `TabControl/*`, `Splitters/*`, `MonthCalendarOD`, `ODDatePicker`, `ODDateRangePicker`, `ClinicSelector/ComboBoxClinicPicker`, `ClinicSelector/FormComboPicker` | UI-4b |
| WinForms `Appt/ControlApptProvSlider`, `Appt/PinBoard`, `Appt/ToggleDayWeek`, `ImagesModule/UnmountedBar`, `ImagesModule/WindowingSlider`, `ImagesModule/ZoomSlider`, `ODColorPicker`, `ODCodeRangeFilter`, `ODPictureBox` (border only), `OdProgressBar`, `BufferedTreeView` (stock look, font and icons only), `MsgBoxCopyPaste`, `DatabaseIntegrity/WarningIntegrity`, `FormPrintPreview` and `ODPrintPreviewControl` (frame, toolbar and the area around the pages; the pages are documents) | UI-4c |
| WinForms `ToolBarOD`, `ODToolBarButton`, `ODToolBarButtonCollection`, `MenuOD`, `ModuleBar`, `ODButtonPanel`, `ODPanelItem`, `LightSignalGrid` | UI-6 |
| WinForms `BordersDpi/FormODBase` | UI-2a (fonts), UI-3 (frame) |
| WinForms `BordersDpi/LayoutManagerForms` | UI-2a |
| WinForms `IconLibrary/*`, `Direct2d/Direct2d` | UI-7a, UI-7b |
| WinForms `Appt/ControlApptPanel*` | keeps its look: appointment-book content (UQ-8); its print path is split in UI-2a |
| WinForms `SheetLayoutController` | keeps its look: sheet content |
| WinForms `MessageBox`, `FolderBrowserDialog` | keep the Windows look (section 1) |
| WinForms `ControlDoubleBuffered`, `FormLauncherHelper`, `ClinicSelector/ClinicPrefHelper`, `BordersDpi/MouseWatcher/*`, `Voice/*`, `TestForWpf` | no drawing of their own; windows among them get the frame and controls like any window |
| WPF `Button`, `CheckBox`, `RadioButton`, `GroupBox`, `Panel`, `PanelAutoScroll`, `Label`, `LinkLabel`, `PictureBox` (border only), `ToolTip`, and their styles in `WpfControlsOD/Themes/Generic.xaml` | UI-4a |
| WPF `ComboBox`, `ComboClinic`, `ListBox`, `ListView`, `TreeView`, `TabControl`, `TabPage`, `SplitContainer`, `DatePicker`, `MonthCalendar`, `TextBox`, `TextPassword`, `TextRich`, `TextVDate`, `TextVDouble`, `TextVInt`, `TextVTime`, `Supplemental/WindowComboPicker`, `Supplemental/WindowMonthCalendar`, `Supplemental/PopupHelper2`, `WpfControlsOD/Themes/TabControlDict.xaml`, their styles in `WpfControlsOD/Themes/Generic.xaml` | UI-4b |
| WPF `ImagingModule/*` (`ImageDockHeader`, `ImageSelector`, `ImageSelectorTypes`, `UnmountedBar`, `WindowImageFloatWindows`, `WindowingSlider`, `ZoomSlider`), `SignatureBoxWrapper` (border only; the signature is content), `Progress/*`, `DatabaseIntegrity/WarningIntegrity`, `Dialogs/FrmColorDialog`, `ToothChart/ToothChartTextBelowStrip` (by UQ-6) | UI-4c |
| WPF `Grid`, `Supplemental/Grid*`, `WpfControlsOD/Themes/DataGridDict.xaml` | UI-5 |
| WPF `ToolBar`, `ToolBarButton`, `Menu`, `MenuItem`, `ContextMenu` | UI-6 |
| WPF `IconLibrary/*` | UI-7a, UI-7b |
| WPF `ColorOD`, `ColorODtemp`, `Graphics` | UI-1 (tokens), UI-11 (ColorOD points at the tokens) |
| WPF `UserControls/CtrlEFormFill` | keeps its look: eForm content (section 1) |
| WPF `WebView2` | its frame only; the HTML is out of scope (section 1) |
| WPF `Dialogs/MsgBox`, `ToothChart/ToothChartRelay`, `ToothChart/ToothChartDashboardWait`, `DatabaseIntegrity/DatabaseIntegrit*`, `TestContr` | no drawing of their own, or a test control |
| Stock `DataGridView` (14) and stock `MenuStrip` (2) | keep the Windows look and take the font; the menu strips take the token renderer (UI-6) |

The WPF `RadioButton` is self-templated, so it does not keep a Windows look by itself. UI-4a draws it like the Windows 11 radio button
in the tokens (circle with a 1 px `StrokeStrong` outline; checked: `Accent` circle with a white centre dot), so WPF radio buttons sit
next to the stock WinForms ones without looking different.

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

**Selection must be visible without colour sense.** `AccentSubtle` on white is only 1.26:1, and `RowHover` against it 1.14:1, so the fill
alone neither meets WCAG 1.4.11 (3:1 for a state) nor tells hover and selection apart. A darker fill does not fix that (`#C5D5EE` is still
1.49:1 on white) and hurts the text. So a selected grid row, list item or tree item also gets a 2 px bar on its left edge, inside its own
bounds: `Accent` (6.8:1 on white) while the control has focus, `StrokeStrong` (3.5:1) when it does not. The fill stays `AccentSubtle`
either way. The bar sits in the row's first 2 px; the briefs check that no cell text or icon starts there.

**Disabled accent button.** A disabled default button draws like any disabled button (`#F5F5F5`, `Stroke` border, `TextDisabled`), as
in Windows 11. No extra token.

**High contrast.** When Windows runs a high-contrast theme (`SystemInformation.HighContrast`, `SystemParameters.HighContrast`), every
colour token returns its `SystemColors` partner (`Surface` and `SurfaceRaised` → `Window`, `Text` → `WindowText`, `Accent` and the
selection → `Highlight` and `HighlightText`, `Stroke` and `StrokeStrong` → `WindowFrame`, `TextDisabled` → `GrayText`, the title bar →
`ActiveCaption` and `ActiveCaptionText`). The tokens re-read the setting on `SystemEvents.UserPreferenceChanged`. Data colours stay.

**Thread safety.** GDI+ pens and brushes are not thread-safe: two UI threads drawing with one `Pen` at the same moment throw
"Object is currently in use elsewhere". `UiTheme` hands out colours as `Color` values (safe), and its cached GDI+ pens and brushes are
per thread (`[ThreadStatic]` caches, created on first use on each thread). The frozen WPF brushes are safe across threads.
`System.Drawing.Pen` and `SolidBrush` are sealed, so a cached pen cannot ignore `Dispose`; a NoHq source scan bans `using` and
`Dispose` around `UiTheme` pens and brushes instead.

**The title bar** uses `Accent` with white text, and `AccentPressed` as its outline. Misc Colors 11, 12 and 13 stay as an office
override (section 6, UQ-3).

**The grid title** is a flat `Accent` bar with white semibold text. The header row is `SurfaceSubtle` with `Text`.

### 3.3 Sizes, corners and lines

All values are at 96 DPI.

- **Scaling.** `LayoutManager` scales the values, with one rule so lines stay sharp at 125%, 150% and 175% (a 1 px border scaled to
  1.25 or 1.5 px would smear into grey):
  - Borders and separators: `max(1, floor(scale))` device pixels (1 px at 100-175%, 2 px at 200%).
  - Focus ring and the selection bar: `round(2 × scale)` device pixels.
  - Every rectangle that is filled or outlined is snapped to whole device pixels before drawing; a 1 px line is drawn on the pixel
    centre (offset 0.5) when antialiasing is on.
  - Corner radii are rounded to whole device pixels.
  - `UiTheme` has the helpers (UI-1), so every control does it the same way.
- **Corners.** Controls (buttons, inputs, combo boxes, tooltips, toolbar and module bar hover shapes): 4 px. Check box box: 2 px.
  Containers (group boxes): 6 px. Grids, tab pages, menus and the window frame: square (Windows 11 rounds the window itself if UI-3
  shows that DWM rounding works with the program's own title bar; the surveys could not tell).
- **Lines.** Borders 1 px. Focus ring 2 px in `Accent`, drawn inside the control's bounds. Selected tab underline 2 px. Module bar
  selection mark 3 px.
- **Spacing inside controls.** Steps of 2, 4, 8 and 12 px. Icon to text gap 4 px. Button text padding 8 px left and right. Menu items
  keep a 16 px icon column. Spacing between controls does not change (decision 1).
- **Items laid out from their text keep today's width formula.** `ToolBarOD` (`ToolBarOD.cs:447-476`: text width plus 6, 8 or 12 px),
  `MenuOD` top-level items (`MenuOD.cs:230`: plus 10), `UI.TabControl` headers (`TabControl.cs:546`: plus 8) and `ODButtonPanel`
  items (`ODButtonPanel.cs:80`: plus 8) keep their padding numbers, so only the font changes their widths (UQ-9).

### 3.4 Fonts

| Use | WinForms | WPF | Today |
|---|---|---|---|
| Body: labels, inputs, buttons, menus | Segoe UI 9 pt (UQ-1) | Segoe UI 11.5 DIP (unchanged) | Microsoft Sans Serif 8.25 |
| Grid cells and headers, button-panel items | the body size (UQ-2: or Segoe UI 8.25 for grid cells) | unchanged | sans serif 8.5 |
| Bold text set in designers | Segoe UI Bold, same rule | unchanged | Microsoft Sans Serif Bold |
| Self-drawn titles: grid title, group titles drawn in code | Segoe UI Semibold 10 pt | Segoe UI Semibold 14 DIP (grid title today) | sans serif 10 bold |
| Window title | Segoe UI 9 pt; the main window keeps its larger title size | same (FormFrame) | Microsoft Sans Serif |
| Module bar captions | Segoe UI 8 pt | – | Arial 8 |

Segoe UI 9 pt and 11.5 DIP draw the same at 96 DPI (measured: Font.Height 16, "Ag" 22×15 px), so WinForms buttons, inputs and grids
match the WPF Frms. Labels and radio buttons are the exception: `LayoutManagerForms` multiplies their font by 0.96 (line 902), as it does
today for Microsoft Sans Serif, so they render at 8.64 pt. GDI draws text at whole-pixel font heights, so at 96 DPI 8.64 pt and 9 pt
probably round to the same 12 px em; at other DPIs and zoom steps they differ by a pixel now and then. The plan keeps the factor (it is
today's behaviour and applies to every family); the scan applies it (5.4), and UQ-1 shows real labels with it. Dropping it for Segoe UI
would be an explicit owner choice.

**Font tokens by role.** Each use in the table except the bold row is one token in `UiTheme` (5.1): `FontBody`, `FontGrid` (cells
and headers, with the bold and underline styles GridOD uses), `FontButtonPanel`, `FontTitle` (grid and group titles drawn in code),
`FontModuleCaption`, and `FontFamilyName` for the window titles, whose size the frame computes. Until UI-2b each holds exactly today's
font (Microsoft Sans Serif 8.25; sans serif 8.5; sans serif 8.5; sans serif 10 Bold; Arial 8; Microsoft Sans Serif), so routing a
site through its token changes nothing. UI-2b sets them to the values of the table.

**Rule for other sizes in designers.** From the flip on, Microsoft Sans Serif at 8, 8.25 or 8.5 becomes the body size. Any other size
keeps its number (10 stays 10, in Segoe UI). While `FontBody`'s family is still Microsoft Sans Serif (UI-2a until UI-2b), the mapping
changes neither family nor size, so the 8 designer fonts at 8 pt and the 2 at 8.5 pt keep their size until the flip. The mapping keeps
the style in full: Bold, Italic and Underline. The rebuild then keeps only Bold, as today and as in Open Dental, so a Label or
RadioButton with Italic or Underline shows without it, as today (section 2, 5.1). Other families stay: Courier New (code), Wingdings
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

- **Focus, for every focusable self-drawn control.** Shown when the control has focus and Windows asks for keyboard cues
  (`ShowFocusCues` in WinForms; `FocusVisualStyle` and `IsKeyboardFocused` in WPF), so a mouse click does not draw a ring.
  - Plain controls (button, check box box, combo box, list box, tab header, calendar day, splitter, toolbar and module bar button): a
    2 px `Accent` ring inside the control's bounds, following its corners. On a check box the ring goes around the box and its text.
  - Accent-filled controls (the default button, a checked check box box, the selected calendar day): the same 2 px `Accent` ring
    outside a 1 px white inner ring, so the ring shows against the fill (white on `Accent` 6.8:1, `Accent` on `Surface` 6.5:1).
  - Grid and list: the focused row (the one the arrow keys move) gets a 1 px solid `Accent` outline inside the row;
    the selection bar is `Accent` only while the control has focus (3.2). GridOD can act on arrow keys without focus
    (`ParentForm_KeyDown`, `GridOD.cs:2724`); that behaviour stays and draws the unfocused bar.
  - WPF controls get the same through one `FocusVisualStyle` in `Generic.xaml`.
- **Buttons** (`UI.Button`, WPF `Button`). Flat `SurfaceRaised` fill, 1 px `Stroke` border, 4 px corners, `Text`. Hover `ButtonHover`,
  pressed `ButtonPressed`. Disabled: `#F5F5F5` fill, `TextDisabled`. The icon stays on the left in the same image rectangle. Once it is
  a Fluent icon it is tinted `Text` (or its role colour, UQ-4): `EnumIcons` icons from UI-7b, pictures from the UI-9a slices. Until
  then UI-4a draws the button's icon as today. No gradient and no text glow.
- **The default button.** WinForms: the form's `AcceptButton` (87 designer lines) is filled `Accent` with white text, `AccentHover`
  and `AccentPressed`. WPF Frms have no `AcceptButton`: each Frm wires Enter in its own `KeyDown` (`FrmODBase.cs:810`). UI-4a adds an
  `IsPrimary` property to the WpfControlsOD `Button` that only changes its look (not `IsDefault`, which in WPF also means Enter), and
  sets it on the button each Frm's Enter handler clicks (the brief lists them from a grep). Most `AcceptButton`s are `butSave` (40) or
  `butOK`/`butOk` (34); 13 are something else (`butPrint` 5, `butSend` 2, `butTryAgain`, `butRunReport`, `butEdit`, `butCreate`,
  `butCheck`, `button1`). The scan lists all 87 with their window and text; the owner confirms the 13 in G-UI3, and the rejected ones
  go on an allow list where they stay plain.
- **Toolbars** (`ToolBarOD`, WPF `ToolBar` and `ToolBarButton`). `SurfaceSubtle` background, 1 px `Gridline` line at the bottom.
  Buttons are transparent until hover (`SubtleHover`, 4 px corners) or press (`SubtlePressed`). Toggled: `AccentSubtle` with a 1 px
  `Accent` border. Drop-down arrow: Fluent chevron, 12 px. Separators: 1 px `Stroke`, inset 4 px. Error state: `DangerSubtle` with
  `Danger` text. Notify state: `WarningSubtle` with `Warning` text. Button widths still come from their text with today's padding (they
  grow about 7% with Segoe UI and later buttons shift right, UQ-9; the scan checks the right edge). Toolbar buttons that take a picture
  from the toolbar's `ImageList` by `ImageIndex` keep working (UI-9b).
- **Grids** (`GridOD`, WPF `Grid`). Title: flat `Accent`, white Segoe UI Semibold. Header: `SurfaceSubtle`, `Text`, 1 px `Stroke`
  column separators and bottom line. Cells: `SurfaceRaised`, 1 px `Gridline` lines. Hover `RowHover`, selection `AccentSubtle` with
  `Text` and the 2 px selection bar (3.2). Outer border 1 px `Stroke` (today blue). Row colours set by the program or the office
  (definitions, status colours) still win. Row height follows the font (UQ-2). Printing keeps today's look (UQ-5).
- **Menus** (`MenuOD`, stock `ContextMenuStrip`, WPF `Menu` and `ContextMenu`). Menu bar on `Surface`. Drop-downs `SurfaceRaised` with
  a 1 px `Stroke` border. Item hover `SubtleHover`, 2 px corners. Separators `Gridline`. Check marks and item icons are Fluent 16 px.
  One `ToolStripManager.Renderer` with a colour table from the tokens restyles all 102 stock context menus and the 2 stock menu strips
  at once. It is set by one shared start-up call in CodeBase (`UiTheme.InitWinForms()`), made by every exe's `Main` right after
  `SetDefaultFont` and `SetHighDpiMode`, so Central Manager, the graph and the other programs get it too. WinForms keeps this renderer
  per UI thread, so the same call runs where the program starts another UI thread (2, "Extra UI threads"); the brief confirms that
  behaviour in the .NET 10 source.
  - Two exes do not make the call: `UpdateFileCopier` and `OpenPmsDentalHelper32` (x86) reference no CodeBase and have no menu or tool
    strip. They keep only their font literal (5.1). No CodeBase reference is added to them.
  - RdlDesign also references no CodeBase (`RdlDesign/ReportDesigner.csproj`), but it has a stock `MenuStrip` (`RdlDesigner.cs:1493`),
    a `StatusStrip` (284) and 5 context menus (`DesignCtl.cs`). It compiles in the two files it needs, the WinForms colours of
    `UiTheme` and the renderer, as linked files, the way it already links `CodeBase/Utilities/ODNumberTextNetFx.cs`
    (`ReportDesigner.csproj:47`). So those two files use only WinForms and `System.Drawing` (UI-1 keeps the WPF brushes in a file of
    their own).
- **Module bar** (`ModuleBar`). `SurfaceSubtle` background with a 1 px `Gridline` divider on its right. Hover `SubtleHover` with 6 px
  corners, inset 4 px inside the button's bounds. Selected: `SurfaceRaised` fill, a 3 px `Accent` mark on the left, the icon in its
  Filled style tinted `Accent`, caption in `Text`. Unselected icons Regular style in `Text`. Captions Segoe UI 8. The module icons are
  `EnumIcons`, so their Fluent Regular and Filled styles come from UI-7b, which UI-6 waits for (section 7).
- **Tabs** (`UI.TabControl`, WPF `TabControl`, 11 stock `TabControl`s keep the Windows look). Headers keep their sizes. Unselected:
  transparent, `TextSecondary`. Hover `SubtleHover`. Selected: `Text` and a 2 px `Accent` line along the header's bottom. Page border
  1 px `Stroke`, page background `Surface`.
- **Group boxes and panels.** 1 px `Stroke`, 6 px corners, title in `Text` on the panel's background.
- **Inputs.** Stock TextBox and the program's TextBox subclasses keep the Windows 11 look and take the new font; their `BorderStyle` does
  not change, because that changes the text area. `UI.ComboBox`: `SurfaceRaised`, 1 px `StrokeStrong` border, 4 px corners, Fluent
  chevron; hover border `StrokeStrongHover`; focus 2 px `Accent` line inside the bottom edge. `UI.ListBox`: `SurfaceRaised`, `StrokeStrong`
  border, hover `RowHover`, selection `AccentSubtle` with the selection bar (3.2).
- **Check boxes** (`UI.CheckBox`, WPF `CheckBox`). Same box size as today. Empty: white with a 1 px `StrokeStrong` border and 2 px
  corners. Checked: `Accent` fill with a white Fluent check mark. Indeterminate: `Accent` fill with a white dash. Hover border `StrokeStrongHover`.
  Stock WinForms RadioButtons keep the Windows look; the WPF `RadioButton` is drawn to match them (2.1).
- **Calendars and pickers** (`MonthCalendarOD`, `ODDatePicker`, `ODDateRangePicker`). Selected day `Accent` with white text. Today: a
  1 px `Accent` ring (today orange). Arrows: Fluent chevrons.
- **Splitters.** A 1 px `Gridline` line in the middle of the splitter; `Accent` while dragging.
- **Tooltips** (`ToolTipOD`, WPF `ToolTip`). `SurfaceRaised`, 1 px `Stroke`, 4 px corners, `Text`.
- **Window frame** (`FormODBase`, `FormFrame`). Title bar `Accent` (or the office's Misc Colors), white title text. Caption buttons
  flat: minimise and maximise hover `CaptionHover`, close hover `Danger`. Caption glyphs are Fluent (Subtract, Maximize, Square Multiple,
  Dismiss). 1 px outline in `AccentPressed`. Window background `Surface`. With `NoCustomBorders.txt` (`AreBordersMS`) the Windows
  frame stays: the client area is restyled, and the title bar tokens and Misc Colors do not apply.
  - Floating image windows (`FormImageFloat`) show which one is selected, even without focus (`IsImageFloatSelected`,
    `FormODBase.cs:279`). Today `PanelBorders_Paint` mixes a fixed 65,94,154 with white for the two states (`FormODBase.cs:687-699`;
    a copy in `FormFrame.cs:513-526`, which the UI-3 brief checks is reached), and Misc Colors do not apply. New: selected, `Accent`
    with white text; unselected, `SurfaceSubtle` with `Text` (14.9:1) and a 1 px `Stroke` outline. Caption button hover: `CaptionHover`
    on the selected one, `SubtleHover` on the unselected one; close hover `Danger` on both. Misc Colors still do not apply.
- **Dialogs.** Windows message boxes stay as they are. The program's own small dialogs (input boxes, `MsgBoxCopyPaste`, progress
  windows) are ordinary windows and get the frame and controls above. Progress bars in self-drawn progress windows fill with `Accent`.

### 3.7 What makes sure nothing moves

- No package edits `Location`, `Dock`, `Anchor`, `Margin`, `Padding`, `AutoSize` or `TabIndex` in a designer. The one exception is a
  UI-10 fit fix on a control that sizes itself: `AutoSize = false` with an explicit `Size`, listed in `fit-fixes.tsv` (5.4).
- `Size` changes only in UI-10's fit fixes, each one listed (section 5).
- A layout snapshot test (UI-0) records every designer control's position and size and fails on any change that is not in the fit list.
- The designer snapshot cannot see scaling at run time, so the tier-1 scan and the group U walker also record each control's `Bounds`
  after the window is built, and compare them with the before set (old font). Any difference not in `fit-fixes.tsv` fails, except the
  growth of a control that sizes itself, which has its own rule (5.4): the edge it is anchored to stays, its size may grow, and a new
  overlap with a sibling or with the parent's client edge fails unless it is listed.
- No container scales its children by font: every `AutoScaleMode.Font` container (section 2, Fonts) changes in UI-2a, and a NoHq source
  scan bans assigning `AutoScaleMode.Font` outside `UnitTests` and `PluginExample` from then on (5.1; comments do not count).
- Focus rings, selection marks and hover shapes are drawn inside the control's own rectangle.

---

## 4. Icons

### 4.1 The version and the download

- **Source.** Microsoft's Fluent UI System Icons, https://github.com/microsoft/fluentui-system-icons, MIT licence, "Copyright (c) 2020
  Microsoft Corporation". The repository has no GitHub Releases; versions are its packages.
- **Version.** The newest `@fluentui/svg-icons` package on the day UI-7a runs (rule: always the latest). On 2026-11-17 that is 1.1.346,
  published 2026-11-13 (release commit `67e6dec1e`; the repository's main was `41089846a` on 2026-11-16).
- **What is vendored.** Only the SVG files the manifest uses, in `third-party/fluentui-system-icons/`, with the `LICENSE`, the version,
  the download URL and the package's integrity hash in a `SOURCE.txt`. Not the whole set of 3,002 icons, and not the fonts.
- **Notices.** An entry in `THIRD-PARTY-NOTICES.txt` and in the About box's notices.
- **Could not determine:** the folder layout inside the npm package (the repository keeps `assets/<Name>/SVG/ic_fluent_<name>_<size>_<style>.svg`).
  UI-7a records the layout it finds.

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
- **The table** with every purpose, today's picture, the chosen icon and its source is `icons/README.md`, filled by UI-7b, UI-8 and the
  UI-9 slices and shown in G-UI1 part 3.

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
- Vendor and partner logos (Pearl, CareCredit, Practice Booster, DentalIntel, TSI and others): third-party marks, never redrawn. The
  report designer's fyiReporting logo in its About box (`RdlDesign/DialogAbout.cs:203`) is one of them.
- The 21 `.gif` files in the `RdlDesign` folder (`chart.gif`, `copy.gif` and others): no project file or code uses them; the report
  designer's toolbar takes its pictures from its designer `.resx` (UI-9a-7). They stay by the never-cut rule.
- Database pictures: procedure buttons and program links an office has set, and the 6 seeded vendor logos. The refresh never changes an
  office's rows. New default procedure-button art goes only into `db/init/new-database.sql` (UQ-7).
- The 17 cursors; the report allocator sample animation (`SnuffLight1-8.gif`).
- `loadingAnim.gif` (the spinner) may become a vector progress ring in UI-11; until then it stays.

---

## 5. The font change and the text-fit scan

### 5.1 Where the font is set

The font change comes in two packages (section 7). UI-2a adds the font tokens by role (3.4: `FontBody`, `FontGrid`,
`FontButtonPanel`, `FontTitle`, `FontModuleCaption`, `FontFamilyName`), each holding exactly today's font, and routes every site below
through its role, so nothing visible changes. UI-2b, near the end, sets them to Segoe UI (body 9 pt, or 8.25 after UQ-1; grid by UQ-2)
together with the last fit fixes. The sites:

- the 12 exe `Main`s' `SetDefaultFont` calls: `OpenPmsDental/Main Modules/ProgramEntry.cs:28`, `CentralManager/Program.cs:16`,
  `DatabaseIntegrityCheck/Program.cs:15`, `DocumentationBuilder/Program.cs:15`, `MySQLManager/Program.cs:17`,
  `OpenPmsDentalDbSettings/Program.cs:20`, `OpenPmsDentalHL7/Program.cs:20`, `OpenPmsDentalHelper32/Program.cs:25`,
  `RdlDesign/RdlDesigner.cs:1284`, `ServiceManager/Program.cs:16`, `UpdateFileCopier/Program.cs:14`, `xCrudGenerator/Program.cs:17`.
  Each keeps a literal font as its first statement (today's in UI-2a, `new System.Drawing.Font("Segoe UI",9f)` in UI-2b), so an exe
  does not need to reach CodeBase before any window exists; a test checks the literal equals the token. `SetDefaultFont` still applies
  the Windows text scale factor.
- `FormODBase.cs:137`, `LayoutManagerForms.FontInitial` (`LayoutManagerForms.cs:84`) and `FormFrame.cs:104` (`FontBody`), and the
  title fonts (`FormODBase.cs:806-807`, `FormFrame.cs:627-628`; `FontFamilyName`, at the size the frame computes).
- **A family mapping wherever `LayoutManagerForms` records a control.** One helper turns a Microsoft Sans Serif font into the token
  font by the rule of 3.4, keeping Bold, Italic and Underline. While `FontBody`'s family is Microsoft Sans Serif (until UI-2b) it
  returns the font unchanged, size included, so UI-2a changes no designer font; with the test override (5.4) or after the flip it
  applies the rule. It runs in all three places a `Control96Info` is made (section 2, Fonts), and before `FontSize96` is read, so the
  recorded size is the mapped one:
  - the constructor's loop over the window's designer controls (`LayoutManagerForms.cs:133-143`), before `new Control96Info` (whose
    constructor reads `FontSize96` at line 1391);
  - `AddToList96` (line 211), before line 213, for controls added later through `LayoutManager.Add` (177), `Synch` (475) and the tooth
    chart's engine control (1118);
  - `AddToList96Unscaled` (line 245), before line 246, for `AddUnscaled` (203): the preference pages, the setup wizard's pages and
    `ControlImages`. Their explicit Microsoft Sans Serif designer fonts are mapped there: `ControlImages.Designer.cs` 4,
    `UserControlAccountRecAndRepCharges.Designer.cs` 5 (262-306), `UserControlChartProcedures.Designer.cs:105`,
    `UserControlExperimentalPrefs.Designer.cs:34`.
  - When: the constructor runs in `InitializeLayoutManager` (`FormODBase.cs:651-653`), which each window calls right after
    `InitializeComponent`, before `Load` and before any data or signature is loaded, so setting the font there cannot fire a
    `TextChanged` that clears a signature (Job 55866). Every FormODBase window that calls `InitializeLayoutManager` builds its layout
    there, so the designer fonts of those windows (most of the 418) change without editing designer files. Five FormODBase windows
    never call it; they are handled like plain forms (below).
- **The rebuild** (`LayoutControlBoundsAndFonts`, 868-914) does not change. It keeps the control's current family, sets a new font
  only when the size differs, and keeps only Bold. So a Label or RadioButton loses Italic and Underline on its first layout, exactly as
  today and as in Open Dental (section 2, Fonts); the mapping's full style matters for the other controls, which keep it until a zoom
  or DPI change rebuilds them, also as today. Showing the designer's Italic and Underline on labels would be a visible change against
  Open Dental (the underlined headings of `FormInsBenefits`, for example); it is not part of this plan, and would need its own gallery
  entry and owner question. A font that code sets on a control after it was added keeps the family the code chose, which is why the
  code sites below are edited one by one.
- **Controls added after `Load`.** Some windows add controls later through `LayoutManager.Add`, `AddUnscaled` or `Synch` with handlers
  already attached (`FormSheetFillEdit` fields; the setup wizard's pages, added in `FormSetupWizardProgress_Load`). The brief lists
  every `LayoutManager.Add`, `AddUnscaled` and `Synch` outside `InitializeLayoutManager` from a grep, and checks each one either is
  excluded as content (below) or has no handler a font change could trigger.
- **Content is excluded from the mapping.** The mapping skips a control when it, or any control it sits in, is content:
  - Content types, checked by type: `ToothChartWrapper`, `ContrPerio` (UQ-6) and `ODPrintPreviewControl`. A type needs no call at the
    right moment, so the tooth chart is covered in every window that hosts it (`ControlChart`, `FormGraphics`, `FormSheetFillEdit` and
    others).
  - Marked panels, for panels that are content in one window only: `FormSheetFillEdit`'s `panelMain` (each field `TextBox` takes the
    sheet field's stored font, `FormSheetFillEdit.cs:1414`, and is added at 1436), and the `FormSheetDefEdit` and `FormClaimFormEdit`
    canvases. `LayoutManagerForms.MarkContent(Control)` records the panel. Because the mapping of designer controls runs in
    `InitializeLayoutManager`, a window marks its panels before it calls `InitializeLayoutManager`; `MarkContent` throws when the
    panel's window has already built its layout, so a mark in the wrong place fails the first time a test opens the window.
  - In the constructor's loop the check walks up `Control.Parent`. In `AddToList96` and `AddToList96Unscaled` it walks up from the
    `parent` argument, because `Add` and `AddUnscaled` record a control before it is parented (the sheet fields at
    `FormSheetFillEdit.cs:1436`; `AddUnscaled` adds to the parent only after recording, `LayoutManagerForms.cs:206-207`).
  - RdlDesign's report canvas (`RdlDesign/DesignCtl.cs`) draws report items with the report's fonts and is not under LayoutManager;
    RdlDesign's font edits (below) leave it alone.
  - A test: a synthetic sheet with a Microsoft Sans Serif field keeps that font in `FormSheetFillEdit` after the flip.
- **Content that inherits its font pins it.** Skipping the mapping does not hold a font the control never set: a control without a
  font of its own takes its window's font, which becomes the token. So the content controls that draw with an inherited font pin it to
  Microsoft Sans Serif 8.25 (today's value) in their own constructor, in UI-2a-1: `ToothChart2D` (5.2) and `ContrPerio` (it draws the
  graphical perio chart with `Font`, `ContrPerio.cs:2230`, and sets none; UQ-6). `ContrPerio`'s pin also holds the copy that
  `FormClaimEdit` builds on its own for the claim attachment (5.2). The brief checks every other content control for an inherited font.
- **No container scales by font.** Every `AutoScaleMode.Font` container listed in section 2 changes in its designer in UI-2a:
  - UserControls that a FormODBase window hosts through LayoutManager (`ControlImageDisplay`, `ControlImageDock`, `PrintPanel`,
    `ToothChart2D`, `ToothChartWrapper`) get `AutoScaleMode.None`. LayoutManager scales them already; `SetNoAnchorDock` sets `None`
    today, only later.
  - The 12 OpenPmsDentalGraph UserControls (`DashboardTabCtrl`, `DashboardPanelCtrl`, `DashboardCellCtrl`, `GraphIncome`,
    `GraphQuantityOverTime`, `GraphQuantityOverTimeFilter`, `GroupingOptionsCtrl`, `BaseGraphOptionsCtrl`, `BrokenApptGraphOptionsCtrl`,
    `HqMessagesRealTimeOptionsCtrl`, `IncomeGraphOptionsCtrl`, `ProductionGraphOptionsCtrl`) are not hosted by any FormODBase on
    `main`. They sit only in the graph's plain forms: `FormDashboardEditTab` holds `DashboardTabCtrl` in its designer
    (`FormDashboardEditTab.Designer.cs:32`), the main program opens that form as a plain form (`FormOpenDental.cs:5310`), and the
    others are created inside one another in code (`DashboardTabCtrl.cs:161`, `DashboardPanelCtrl.cs:227, 330`,
    `FormDashboardEditTab.cs:110`). `None` would stop them scaling their children when the plain form scales for DPI, so at 150% the
    dashboard's contents would stay at 96-DPI size inside a scaled form, a layout change. They get what their forms get,
    `AutoScaleMode.Dpi` with 96F,96F, or `Inherit`, whichever the brief shows from the .NET 10 source of `ContainerControl`'s scaling
    scales each nested control exactly once.
  - Forms of the plain exes (Central Manager, Service Manager, MySQL Manager, the DB settings, HL7 debug, Update File Copier, Database
    Integrity Check, Documentation Builder, xCrudGenerator, the graph's own forms) get `AutoScaleMode.Dpi` with `AutoScaleDimensions`
    96F,96F. These exes run PerMonitorV2 (`SetHighDpiMode` in each `Main`) and rely on this scaling at high DPI, so `None` would leave
    them tiny at 150%. `Dpi` keeps the DPI part and drops the font part.
  - With today's font the change is invisible at 96 DPI. Above 96 DPI the plain exes scale by the DPI ratio instead of the font-metric
    ratio, which is nearly the same; G-UI4 pictures them, and the desktop check at 150% (section 8) includes the dashboard and one
    Central Manager window. The brief records every host of each graph control from a grep.
  - The trade-off: under `AutoScaleMode.Font` the plain exes' layout also grows with Windows' text size setting today
    (`SetDefaultFont` applies the factor, so the font-to-6×13 ratio rises). `Dpi` drops that part: at text size 125% the text grows but
    the layout does not, so labels and buttons in Central Manager, Service Manager and the other plain exes can clip. The scan's
    text-scale run includes these forms (5.4), and G-UI4 pictures them at text size 125%. What clips goes to the UI-10 slices as
    size-only fixes, or to the owner. If size-only fixes cannot cover it, the fallback for a form is to keep `AutoScaleMode.Font` with
    `AutoScaleDimensions` set to the new font's metric in UI-2b (the ratio is then 1 at normal text size and grows with it, as today),
    and to list that file in the ban's allow list.
  - The NoHq source scan bans `AutoScaleMode.Font` outside `UnitTests` and `PluginExample` from then on. It matches only an
    assignment (`AutoScaleMode\s*=\s*(System\.Windows\.Forms\.)?AutoScaleMode\.Font`) in the part of a line before any `//`, so
    comments such as `FormODBase.cs:128` and `FormFrame.cs:95` ("AutoScaleMode.Font is a huge problem") and `ODWebView2`'s
    commented-out line do not trip it.
- The 78 plain forms that do not use `LayoutManager` (RdlDesign 21-22, CentralManager 18, OpenPmsDental 5, OpenPmsDentalGraph 5,
  ServiceManager 3, others) mostly take the exe's default font, so they follow its literal. Few of their designer fonts name
  Microsoft Sans Serif: on `main` about 14 outside OpenPmsDental (Central Manager 4, the graph 6, xCrudGenerator 2, RdlDesign 2), plus
  those of OpenPmsDental's plain forms; the brief greps again. RdlDesign's other designer fonts are the 55 Arial 8.25 Bold Italic
  headings of 3.4 and a few Wingdings and Courier New fonts, which stay by the rule of 3.4.
  - A designer file cannot point at the token: the Visual Studio designer writes the property back as a literal the next time it saves
    the form. So a Microsoft Sans Serif 8.25 Regular font on a control whose parent has the default font is removed, and the control
    inherits the default. Any other (Bold, other sizes) stays a literal in the designer; UI-2b changes it together with the exe
    literals, and a NoHq test checks each one's family and size against the token by the rule of 3.4 (UI-2a-4).
- **FormODBase windows without `InitializeLayoutManager`** are handled like plain forms. Five on `main` (a grep of FormODBase subclasses
  whose constructor does not call it), all in `OpenPmsDental/ReportingForRdl/Allocators`: `FormProgressViewer` and, in `MyAllocator1`,
  `FormInstallAllocator_Provider`, `FormReport_GuarantorAllocationCheck`, `FormReport_ProviderIncomeReport` and
  `FormWarnToCloseComputers`. The `LayoutManagerForms` constructor never runs for them, so the mapping never reaches their designer
  fonts, and they are not among the 78 plain forms either. They take FormODBase's own font (`FontBody`), so a control without a font
  follows the token; their explicit fonts follow the plain forms' rule: a default font is removed, any other stays a literal that UI-2b
  changes and the NoHq literal test checks. On `main`: `FormInstallAllocator_Provider.Designer.cs` has 7 Microsoft Sans Serif 8.25 Bold
  fonts (52-187), `FormReport_GuarantorAllocationCheck.designer.cs:130` one at 14.25, and `FormReport_ProviderIncomeReport.Designer.cs:76`
  sets the form's own font to Microsoft Sans Serif 8.25 Bold, which every control in it inherits. `FormWarnToCloseComputers` uses Arial
  and Arial Black (they stay, 3.4); `FormProgressViewer` sets none. No package adds `InitializeLayoutManager` to them: it would start
  LayoutManager's DPI scaling, anchoring and `AutoSize` switch-off in those windows, a layout change. They are in the UI-2a-4 slice
  that owns `ReportingForRdl` (section 7).
- Code that creates screen fonts, each routed through the token of its role (3.4), whose UI-2a value is the font the site uses today:
  - `ToolBarOD.cs:89` (Microsoft Sans Serif 8.25), `UI/ListBox.cs:391` (sans serif 8.25), `ControlAppt.cs:3182, 3187` (the lab
    text box, Microsoft Sans Serif 8.25 Regular and Bold): `FontBody`;
  - `GridOD.cs:3567-3571` (cells, bold, underline, underline bold and header, sans serif 8.5): `FontGrid` with the matching style;
    `GridOD.cs:3572` (title, sans serif 10 Bold): `FontTitle`. GridOD's screen fonts are routed in UI-2a-3, which owns the file;
  - `ODButtonPanel.cs:64` (sans serif 8.5): `FontButtonPanel`;
  - `ModuleBar.cs:323` (Arial 8): `FontModuleCaption`;
  - the rest of the about 59 code lines and about 40 `GenericSansSerif` files that draw on screen: the role the brief names for each.
    A site whose size fits no role uses `FontFamilyName` at its own size and style, which is today's font while the family is
    Microsoft Sans Serif. A site in another family (Arial, for example) stays as it is unless the brief names a role for it.

  Each brief lists its sites from a fresh grep, and sorts each into screen chrome (changes), print (5.2) or document and content (5.2). Code
  that draws into a picture with `Graphics.FromImage` and saves, attaches or exports it makes a document, not screen chrome (for example
  the claim perio attachment, 5.2). The appointment book's fonts are content unless UQ-8 says otherwise (5.2).
- GridOD's India setting switches the grid to Arial (`GridOD.cs:1408`). UI-5 checks whether Segoe UI draws the rupee sign, the likely
  reason; if it does, the switch goes.

### 5.2 What does not change

- **The tooth chart.** `SparksToothChart/ToothChart2D.cs:296-378` draws tooth numbers and "W" with the control's own font, which comes
  from the default. UI-2a pins `ToothChart2D`'s font to Microsoft Sans Serif 8.25 in the product, and sets `ToothChart2D` and
  `ToothChartWrapper` to `AutoScaleMode.None` (5.1), before the default changes. `ToothChartData.cs:87` (GenericSansSerif 8.25),
  `ToothChartWrapper.cs:431` (Arial 9) and the WPF chart's Arial stay as they are.
- **Printouts and documents.** Sheets, statements, claim forms, reports, RDL, MigraDoc, eForm and PDF output keep their own fonts
  (stored per field, or set in their print code). `PdfSharpSetup.FALLBACK_FONT_FAMILY` stays. The documents on screen (a filled sheet,
  the sheet, claim form and report design canvases, print previews, the eForm fill surface) are excluded from the mapping (5.1) and from
  the control restyle (section 1).
- **GridOD's print path.** `PrintPage` (line 997), `SheetDrawRow`, `SheetDrawTitle`, `SheetDrawHeader` and their PDF variants
  (2960, 3193, 3275) draw with the screen fonts today. UI-2a first gives them their own print fonts, pinned to today's sans serif 8.5
  and 10 Bold and today's print colours (UQ-5). `GridOld.cs` and `Tables Old Grids/ContrTable.cs` get the same check.
- **The perio chart's print font.** `FormPerio.cs:1170` sets `contrPerio.Font` to Microsoft Sans Serif 8.25 inside `butPrint_Click`,
  only for `PrinterL.TryPrint`. It is a print site and stays as it is. On screen `ContrPerio` is excluded from the mapping and pins its
  font (5.1, UQ-6).
- **The claim perio attachment.** `FormClaimEdit.butAttachPerioHelper` (`FormClaimEdit.cs:2479-2545`) builds a `ContrPerio` on its own,
  draws the chart into a picture with the patient's name and the practice title above it in Microsoft Sans Serif 12 and 9 Bold (lines
  2506 and 2509), and saves it as a JPG that is attached to the claim. It is a document: both fonts stay Microsoft Sans Serif and are
  on the NoHq allow list, the chart keeps its font through `ContrPerio`'s pin (5.1), and the print guard compares the attachment
  picture before and after.
- **The appointment book** (`OpenPmsDental/UI/Appt/ControlApptPanel*.cs`). Its fonts (`_font`, `_fontMinutes`, `_fontMinutesBold`,
  `_fontOpsHeaders`, lines 142-150, rebuilt at 720-724 from `GenericSansSerif`) draw appointment boxes, time bars and operatory headers,
  and the same panel prints the schedule: `PrintPage` (line 1477) sets `_isPrinting` and draws with the same fonts, and the print colours
  come from `PrintColorBehavior` (`ControlApptPanel.cs:2500-2721`, `ControlApptPanel.TimeBars.cs:24, 60`,
  `ControlApptPanel.PreAuth.cs:26`). The line height also comes from the font (`_heightLine=fontForHeight.Height`, lines 729 and 1732),
  so a taller font makes every row taller and shows fewer hours. Default: these stay as they are, like the data colours (UQ-8). If the
  owner chooses Segoe UI, the package first splits print fonts and print line height from the screen ones, and the print guard covers a
  printed schedule. `ControlApptProvSlider` (its own time text, `ControlApptProvSlider.cs:711`) follows the same answer.

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
come from a static model (designer text and size only), not from running the program. The model did not apply LayoutManager's 0.96
factor for labels and radio buttons (3.4), and it measured before the new button padding and check box glyph; UI-0's scan counts again
with all of that.

**Controls that size themselves are not in the table.** The model counted fixed-size controls only. Two more groups grow by themselves
with the font, from the same position, and can grow into a neighbour:
- `AutoSize` controls. On `main`, 138 lines set `AutoSize = true` on a control or a form (outside `UnitTests` and `PluginExample`; all in
  `InitializeComponent` but `FormProviderSetup.cs:34`, which sets the form's own under `NoCustomBorders.txt`) in 47 files:
  OpenPmsDental 41 files, and one each in CentralManager, CodeBase (`FormProgressExtended`), the HL7 `FormDebug`, RdlDesign
  (`DialogToolOptions`) and two graph files. By type: 94 labels, 28 radio buttons, 12 check boxes, a `FlowLayoutPanel`, a
  `TableLayoutPanel` and 2 forms. Examples: `FormRpLaserLabels.designer.cs:227-716` (20), `FormBillingTypeMerge.designer.cs:85-207`,
  `FormAutoNoteControlEdit.designer.cs:56-145`. `LayoutManagerForms.SetNoAnchorDock` turns `AutoSize` off only for a `ButtonBase`
  (`LayoutManagerForms.cs:395-397`), so in FormODBase windows the labels keep sizing themselves from their text, and `UI.CheckBox`
  (a `Control`, `CheckBox.cs:23`) is not reached either; in the plain forms and the five windows without `InitializeLayoutManager`
  (5.1) every one of them does. Code adds a few more at run time (`RdlViewer.cs:976, 995`, `UpdateFileCopier/FormMain.cs:474`).
- Controls whose height Windows takes from the font. A single-line `TextBox` has `AutoSize` on by default (the designer does not write
  it): its height is the font height plus 7 px with a border by the .NET formula, so 20 px today and about 23 px at Segoe UI 9
  (`Font.Height` 16, measured); the 8.25 pt value was not measured. About 2,700 designer `TextBox`es and `Valid*` boxes, about 250 of
  them multiline. Stock `ComboBox`, `DateTimePicker` and `NumericUpDown` heights also follow the font. The brief takes the exact list
  and formulas from the .NET 10 source.

UI-0's scan counts both groups (5.4) for each UQ-1 option, so the owner's numbers include them.

### 5.4 The scan

**Tier 1, no database** (UI-0b, `tests/UiRefreshTests`). In its own process, it builds every Form and UserControl type (about 892
forms and 153 UserControls, the plain exes' forms included) like this: create an empty object, run only the base constructor
(FormODBase, Form or UserControl), then call the type's `InitializeComponent` and, for a FormODBase whose own constructor calls
`InitializeLayoutManager` (from a source grep), `InitializeLayoutManager` by reflection. The five FormODBase windows that never call it
(5.1) are built without it, so the scan shows them as the product does: unmapped and unscaled. This skips each window's own
constructor, so constructor arguments and database calls do not matter. Types that throw are listed.

A UserControl is not scanned on its own: it has no `LayoutManagerForms` of its own, so the mapping (5.1), the 0.96 factor and
`SetScaleMS` would not reach it, and its explicit designer fonts (19 in `ControlAccount.Designer.cs`, the preference pages') would stay
Microsoft Sans Serif in the scan. Tier 1 hosts each one in a bare test FormODBase (one test class, `InitializeLayoutManager` called
on it) and adds it the way the product does: `LayoutManager.AddUnscaled` for the types the product adds that way (the preference
pages, the setup wizard's pages, `ControlImages`; the brief takes the list from a grep of `AddUnscaled`), `LayoutManager.Add` for the
rest, then one `LayoutControlBoundsAndFonts`. The mapping, the 0.96 factor and the scale steps then apply as in the product. A
UserControl that the product never puts under LayoutManager (a plain exe's, or the 12 graph UserControls, 5.1) is hosted in a plain
test Form instead. This list of which types are under LayoutManager is also the "Microsoft Sans Serif" scan's scope (section 9).

**The font under test.** Setting the application default is not enough. FormODBase sets its own font in its constructor
(`FormODBase.cs:137`, the token after UI-2a-1), and the designer fonts are explicit Microsoft Sans Serif, so a FormODBase window ignores
the default; only plain forms and code that reads `Control.DefaultFont` follow it. So the scan sets the default and also:
- until UI-2a-1 merges: applies UI-0b's test-side font swap to each window once it is built. The swap sets the form's and every
  container's font as well as the controls', so `AutoScaleMode.Font` containers rescale as they would, and the bounds record catches it;
- from UI-2a-1 on: sets a test-only token override (`UiTheme.SetFontForTest`, only in test processes) before any window is built, so the
  product's own path runs: FormODBase's constructor, the mapping of 5.1 and the code sites.

The bounds comparison and the UI-10 galleries use the same mechanism as the scan at that point.

It then walks every visible control with text and measures it with the control's own way of drawing:
- `TextRenderer` for stock Label, LinkLabel, RadioButton, CheckBox, Button and GroupBox;
- `Graphics.MeasureString` for `UI.Button` (`Button.cs:345-361`), `UI.CheckBox` (`CheckBox.cs:206-268`) and `UI.GroupBox`
  (`GroupBox.cs:128-167`).

Labels and radio buttons are measured at the font LayoutManager gives them (× 0.96, 3.4). For the 1-2 px height cases it draws the
control into a bitmap and looks for ink in the last row or column.

**Controls that size themselves** (5.3): every control whose `AutoSize` is on at run time once the window is built (so the designer's
lines, the ones code sets, and single-line text boxes, whose default is on), and the stock controls whose height Windows takes from the
font. For each, the walk records its preferred size in the font under test (`GetPreferredSize`, which is what WinForms applies) and
the bounds it actually gets. It reports a growth that newly overlaps a sibling's bounds or passes the parent's client edge, as
"fixable" (free space for an explicit size, below) or "needs the owner".

**Scales.** A test process cannot change Windows' DPI, so the scan simulates it the way LayoutManager sees it: `SetScaleMS` on each
window's `LayoutManagerForms` (`LayoutManagerForms.cs:626`) at 1.0, 1.25, 1.5, 1.75 and 2.0, plus one program zoom step through
`ZoomTest` (line 104). Stock controls that scale their own fonts from the real DPI are not fully simulated this way, so real 125% and
150% desktops are still checked by hand (section 8). One more run sets the default font as Windows does at text scale 125%
(`SetDefaultFont` applies the factor), at 96 DPI. It matters most for the plain exes' forms, which take the default font and, with
`AutoScaleMode.Dpi`, no longer grow their layout with it (5.1).

**Bounds.** The same walk records every control's `Bounds` once the window is built, for the before set (old font) and for the font
under test. Any difference that is not in `fit-fixes.tsv` is reported: it catches run-time scaling the designer snapshot cannot see
(3.7). Controls that size themselves are keyed apart, with their own rule:
- the edge they are anchored to stays: their `Location` for the usual top-left anchor (LayoutManager sets that anchor on every
  control of a window that does not set `IsLayoutMS`, `LayoutManagerForms.cs:393, 453`); WinForms grows a control anchored only
  right or bottom the other way, which the brief confirms in the .NET 10 source;
- growth of their `Size` is expected and is not a difference;
- a new overlap with a sibling, or a new pass over the parent's client edge, fails unless it is in `fit-fixes.tsv`;
- children of a `FlowLayoutPanel` or `TableLayoutPanel` are placed by the panel, so a grown sibling may move them; they are compared by
  their order and by the panel's bounds (on `main` such panels are in `FormProgressExtended`, `UserControlEmailTemplate`, the graph's
  `Legend` and `DashboardPanelCtrl`; the brief greps again).

Output: a TSV of new overflows and new overlaps only, with the extra pixels needed and whether free space exists to the right or below
inside the parent without touching a neighbour. Each row is "fixable" or "needs the owner".

**When it runs.** At the end of every control package (UI-3 to UI-6, UI-9b), because each changes what fits (button padding, the check
box glyph, toolbar padding, image rectangles). Its counts go into that package's gallery. UI-10 slices always work from the latest run.

**Tier 2, with real data** (UI-0c, the harness). A walker in the new harness group U measures every window the harness opens, after `Load`,
so text set at run time is covered (about 975 such assignments in 241 files). It uses tier 1's measurer and bounds-record writer, not a
second copy: the measuring rules above and the record format are one set of source files that UI-0b owns, so both tiers measure alike
and the before set's bounds record has one format.

**The fixes** (UI-10 slices). Size-only edits to `Width` and `Height` in designers, from the scan's TSV for the new font. A control
that sizes itself and would grow into a neighbour gets the one other listed fix: `AutoSize = false` with an explicit `Size` that fits
the free space (wider, or taller so a label wraps; for a text box, today's height where the new font still fits in it). The brief
checks that the Visual Studio designer keeps that line for each control type it uses it on. A fix that would touch a neighbour goes to
the owner with a picture. Every fix is listed in `tests/UiRefreshTests/Data/fit-fixes.tsv`, which the
layout snapshot test reads (3.7). The fixes merge while `main` still shows the old font: a control a few pixels larger than its text
needs, growing into free space, does not change today's look much, and it means `main` never shows clipped text. The flip (UI-2b)
comes after the last slice.

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
  override, item by item (UQ-3): an item that holds Open Dental's stock value counts as "not customised" and gives its token, and an
  item the office changed gives the office's colour. Stock Main Border (-12493158, `#415E9A`) gives `Accent`, stock Main Border Outline
  (-16777216, black) gives `AccentPressed`, stock Main Border Text (-1, white) gives white. So an office that changed only the text
  colour keeps its text colour on the new title bar, and does not get back the whole old frame. (`Accent` and the stock blue are nearly
  the same colour; in practice the rule mostly replaces the black outline.) The Definitions window keeps editing them and shows
  "(program default)" next to an item that holds the stock value, so staff see why the swatch and the title bar differ. No schema change.
- `ComputerPref.Zoom` and all other settings are unaffected.

**Look switches that stay.** The never-cut rule keeps all three flag files of section 2 working, and "no classic option" does not
remove them:
- `NoCustomBorders.txt`: `AreBordersMS` keeps the standard Windows frame; the client area is restyled; the title bar tokens and Misc
  Colors do not apply. Window check A6 and `PopupHelperTests` stay green (UI-3).
- `NoD2D.txt`: `IconLibrary.OnlyGDI` draws every Fluent icon with GDI+ (UI-7a tests this path).
- `NoDpi.txt`: the program runs DPI-unaware; the look draws at 96 DPI and Windows stretches it, as today (UI-7a tests the icons in
  this mode).

**Code and files that go.**
- The 38 WinForms and 37 WPF `IconLibrary/Gen_*.cs` files (replaced by the generated table).
- Each bitmap whose last use a package replaces: the `Resources.resx` entry, the image file and the ImageList stream, in the package that
  replaces the last use. Bitmaps that no code uses today stay (section 1).
- The leftover comments about `ODColorTheme` (`FormODBase.cs:363`, `GridOD.cs:1761`) and the commented-out `OdThemeModernGrey` line in
  `FormButtonTest.cs:37`.
- `ColorOD` stays, so its 162 uses in 47 files keep working. It keeps returning today's values until each consumer has been restyled:
  each control package switches its own consumers to the tokens, and UI-11 then points `ColorOD` at the tokens for the rest, with a
  gallery of the files that still use it.

---

## 7. Packages

**Rules for all packages.**
- One writer per package, in its own worktree, on its own branch. Packages that share a control file run one after the other.
- Each package runs Targeted tests (NoHqTests included). Core and Full runs are workflow steps, one at a time.
- A package merges only after its gallery is approved (section 8). A package with no visible change (UI-0, UI-1, UI-7a, UI-2a) merges
  when its tests pass and the harness's U0 check shows no changed picture.
- No package is bigger than one writer and one review can handle: at most about 40 designer files, or one module, or one control family
  (WinForms and WPF together). The large jobs are cut into numbered slices below. A name without its slice number (UI-0, UI-2a, UI-9a)
  means all its slices.
- `main` never shows clipped text: the fit fixes merge before the font flips (5.4).
- Until the last package, `main` shows a mixed look. Nothing is in production (owner, 2026-11-15), so that is accepted. A release cut in
  that window says so in its notes.
- Designer files are also edited by the Open Dental 26.2 port. Before it starts, each package checks its files against the open port
  branches and waits for the overlapping ones.

**Order.** Each line needs the lines above it unless it says otherwise.
1. UI-0a (gallery builder) and UI-0b (scan and layout records) side by side, then UI-0c (harness group U, which reuses both), then the
   before set.
2. UI-1 (tokens, design sheet, coverage test).
3. UI-7a (icon pipeline and the glyphs the controls need) and UI-2a-1 (font tokens and mapping, no visible change). They share no files
   and may run side by side. Then UI-2a-2 (no font scaling), UI-2a-3 (print fonts) and UI-2a-4 to UI-2a-n (code font sites), side by
   side where they share no files; the UI-2a-4 slices start after UI-2a-3, whose print guard they run.
4. UI-3 (frame), then UI-4a, UI-4b, UI-4c (controls), UI-5 (grids), UI-6 (toolbars, menus, module bar). UI-7b (the `EnumIcons`) and
   UI-8 (dental icons) own only icon files and may run beside them, but UI-6 starts only after UI-7b has merged: the module bar draws
   the `EnumIcons` in their Fluent Regular and Filled styles (3.6).
5. UI-9a-1 to UI-9a-7 and UI-9b-1 to UI-9b-4 (pictures to icons).
6. UI-10-1 to UI-10-n (fit fixes, made for the new font while `main` still shows the old one).
7. UI-2b (the flip to Segoe UI).
8. UI-11-1 to UI-11-n (chrome `BackColor` values), then UI-11-final (sweep, leftovers, notes).

**UI-0: tools, scans and the before set** (tests and tools only; no product change). Three packages with separate files, then one
closing step. None has a gallery.

**UI-0a: the gallery builder**
- Owns: `tools/New-UiGallery.ps1` (the in-repository successor of the lost `Make-Pairs.ps1`), `tools/ui-gallery-masks.tsv`,
  `tests/WindowChecksTests/UiApproved/approved.tsv` (empty).
- Work: the gallery builder (before, after, diff, % changed, the after picture's masked pixel SHA-256, masks for the clock, dates and
  the copy's database name, clip notes, `index.html` that shows pictures at 1:1 device pixels with a zoom toggle (section 8), a zip with
  forward slashes). The masking and hashing rule is one function that UI-0c's check U0 calls too.
- Tests: the builder's self-tests (no program, no database).

**UI-0b: the text-fit scan and the layout records**
- Owns: the new test project `tests/UiRefreshTests` (the lead adds it to `OpenDental.sln` and `OpenDental.Local.slnf`), the tier 1
  builder and scan, the test-side font swap, `tests/UiRefreshTests/Data/layout-snapshot.tsv` and its test, the bounds record, and the
  shared measuring files `tests/UiRefreshTests/Shared/TextFit*.cs` (the measurer: `TextRenderer` or `MeasureString` by control type,
  the 0.96 factor, the ink check; and the bounds-record writer with its format), written so that `tests/WindowChecksTests` can compile
  them in as linked files.
- Work: the tier 1 text-fit scan with its scales, the text-scale run (the plain exes' forms included), the ink check, the preferred
  size and overlap check of controls that size themselves, and the bounds record with their separate rule (5.4). FormODBase types
  get `InitializeLayoutManager` only when their constructor calls it (5.4). UserControls hosted in a test FormODBase through `Add`
  or `AddUnscaled` (5.4). The layout snapshot and its test. The test-side font swap: it walks a built window and replaces Microsoft
  Sans Serif with a given font (the rule of 3.4, the 0.96 factor included), setting the form's and the containers' fonts as well, so
  galleries and the scan can show real windows in a font option before the product changes (G-UI1 part 1; from UI-2a-1 on, the token override takes its place, 5.4).
- Tests: the scan runs on today's font and reports nothing new; the swap gives the same fonts as the rule of 3.4 on a test window; a
  hosted `ControlAccount` and a hosted preference page get the swapped font on their explicit designer fonts; a test window with an
  `AutoSize` label next to a text box reports the overlap when the label's text grows, and no difference when the label grows into
  free space; `FormInstallAllocator_Provider` is built without `InitializeLayoutManager` (it has no `LayoutManagerForms` after the build).

**UI-0c: harness group U**
- Owns: `tests/WindowChecksTests` group U (`InProc/InProcChecks.U.cs`, `UiGallery/windows.tsv`, the walker), the check U0, and the
  links to UI-0b's shared measuring files in `tests/WindowChecksTests`' project file.
- Needs: UI-0a (U0 uses its masking and hashing rule) and UI-0b (the walker uses its measurer and bounds-record writer, 5.4).
- Work: about 50 common windows the harness does not reach yet (claims, recall, confirmation, schedules, pay plans, billing, procedure
  codes, definitions, security, tasks, medical, Rx, labs and others listed by the tooling survey), opened by their own handlers on
  synthetic patients and closed with Cancel; the main window at fixed sizes 1366×768 and 1920×1080; each module with and without a
  patient. The walker that measures text after `Load` and records bounds with UI-0b's shared files, not its own copy (5.4). The check U0
  (compare each picture with `approved.tsv`).
- Tests: group U runs on the synthetic dump; a window the dump cannot fill is listed with its reason.

**The before set** (the lead, once UI-0a to UI-0c have merged): one full harness run (groups A to K and U) at that commit, in the
owner's connected session, frozen in `.git/od-tools/test-results/ui-before/` with the commit, the dump's hash, the DPI and the bounds
record.

**UI-1: tokens, the design sheet and the coverage test** (no visible change in the program)
- Owns: `CodeBase/UI/UiTheme*.cs` (new: colours as `System.Drawing.Color`, frozen WPF brushes, per-thread cached GDI+ pens and brushes
  (3.2), the high-contrast mapping, font family and sizes, corners, lines, the pixel-snapping helpers of 3.3, spacing, icon sizes, and
  `UiTheme.InitWinForms()`, empty until UI-6), `tests/UiRefreshTests/DesignSheet*.cs`, `tests/UiRefreshTests/Data/coverage.tsv`, the
  NoHq source scans. `ColorOD` is not touched: it keeps today's values (section 6). `UiTheme` is split into partial files so that the
  WinForms colours use only WinForms and `System.Drawing` and RdlDesign can link that file (3.6, Menus); the WPF brushes are in their
  own file.
- Needs: UI-0b (the test project and the builder).
- Work: the tokens of section 3. The design sheet: a test without database that renders the swatches with names, hex values and contrast
  ratios; the type ramp; the spacing and corners; a drawing of every control state made from the tokens (normal, hover, pressed,
  focused, disabled, checked, selected, selected without focus), at 100, 125, 150, 175 and 200%; the high-contrast mapping. It shows the
  options of UQ-1 to UQ-4 side by side. Real windows for G-UI1 part 1 (section 8): `FormPatientEdit`, `FormProcEdit`, the Account
  module, a module toolbar with a grid, and one Frm, each built off-screen with UI-0b's builder and drawn with the test-side font swap
  in each UQ-1 and UQ-2 option.
- Tests: the contrast test (every text, border, focus and selection pair of 3.2 and 3.6 meets its ratio, in the normal and the
  high-contrast mapping); a test that paints two self-drawn test controls with the token pens on two STA threads at once, many times,
  without an exception; the coverage test (each file in `OpenPmsDental/UI`, `WpfControlsOD/UI` and `WpfControlsOD/Themes` has a row
  in `coverage.tsv`); the source scan that flags new `Color.FromArgb` and hex colours in `OpenPmsDental/UI`, `OpenPmsDentalBusiness/UI/GridOD.cs` and
  `WpfControlsOD/UI/Controls` beyond an allow list that shrinks with each package; the scan that bans `using` or `Dispose` around
  `UiTheme` pens and brushes.
- Gallery: **G-UI1 part 1, the design.** Every later package follows what the owner approves here.

**UI-7a: the icon pipeline and the control glyphs** (no visible change in the program)
- Owns: `third-party/fluentui-system-icons/`, `icons/manifest.json`, `icons/README.md`, `tools/New-IconData.ps1`,
  `CodeBase/UI/Icons/*` (the generated table and the renderer: GDI+, Direct2D path calls through `Direct2d`, WPF `StreamGeometry`,
  `GetBitmap`, `GetImageList`), `THIRD-PARTY-NOTICES.txt`, the About box's notices. The generated table and the GDI+ renderer (with
  `GetBitmap`) are files that use only `System.Drawing`, apart from the Direct2D and WPF parts, so RdlDesign can link them (UI-9a-7).
- Work: section 4.1 and 4.2, and the glyph subset the control packages draw: caption buttons (Subtract, Maximize, Square Multiple,
  Dismiss), chevrons (up, down, left, right), the check mark and the dash. These are new `UiGlyph` entries; the `EnumIcons` and both
  `IconLibrary` folders stay as they are until UI-7b.
- Tests: every manifest row draws non-empty at 16, 20, 22, 24, 32 and 48 px on GDI+, Direct2D and WPF; the OnlyGDI path (`NoD2D.txt`)
  and NoDpi mode draw the same glyphs; the generated file matches the manifest and the SVGs; the Fluent licence and notice are present.
- Gallery: none of its own; the glyphs appear in G-UI1 part 2 with the controls.

**UI-2a: the font plumbing** (no visible change in the program). Section 5.1 and 5.2 with each font token still holding today's font
(3.4, font tokens by role) and the mapping still the identity. It is cut into fixed slices: the risky logic (the mapping, the print
split) is kept apart from the mechanical edits. No slice has a gallery; in each, the harness's U0 check must show no changed picture and
the bounds record must equal the before set.

**UI-2a-1: the tokens and the mapping**
- Owns: the font part of `UiTheme` (the role tokens of 3.4 with today's values and their UI-2b values written down beside them, and
  the test-only override `SetFontForTest`, 5.4, which sets them all to an option's values), the 12 exe `Main`s (their literal
  stays today's font; a test checks it equals `FontBody`), `FormODBase.cs` and `FormFrame.cs` (font lines only),
  `LayoutManagerForms.cs` (the mapping in the constructor's loop, in `AddToList96` and in `AddToList96Unscaled`, the content types,
  `MarkContent`; the rebuild is not changed), the marks in
  `FormSheetFillEdit`, `FormSheetDefEdit` and `FormClaimFormEdit` (before their `InitializeLayoutManager`), the font pins in
  `SparksToothChart/ToothChart2D.cs` and `OpenPmsDental/User Controls/ContrPerio.cs`, the font test rows of section 9.
- Work: 5.1's tokens, mapping (the identity while `FontBody` is Microsoft Sans Serif), content rules and pins. Tests that pinned
  "Microsoft Sans Serif" now compare with the token, so the flip later changes no test.
- Tests: section 9's font group; the mapping returns every font unchanged with today's tokens (including the designer fonts at 8 and 8.5
  pt); with the token override set to Segoe UI, an explicit designer font on a preference page (`UserControlAccountRecAndRepCharges`)
  and on `ControlImages`, both added through `AddUnscaled`, follows the token; the underlined labels of `FormInsBenefits` still draw
  without underline, as today; the tooth chart baselines unchanged with the test default font set to Segoe UI (the pin proved before the
  flip); the content-exclusion test (a Microsoft Sans Serif sheet field keeps its font with the token override set to Segoe UI); a perio
  chart picture (`ContrPerio` on a synthetic exam) unchanged with the override set to Segoe UI; `MarkContent` after
  `InitializeLayoutManager` throws; a signature test: open `FormProcEdit` with a signed group note on the synthetic dump, change the
  program zoom and resize the window, save, and check the signature is still valid; the scan's report for Segoe UI through the override
  attached to the brief's result.

**UI-2a-2: no container scales by font**
- Owns: the 47 files with `AutoScaleMode.Font` (section 2; the brief recounts with a grep that skips comments), the NoHq ban.
- Needs: UI-2a-1.
- Work: 5.1, "No container scales by font", the 12 graph UserControls included (`Dpi` with 96F,96F or `Inherit`, as the brief shows;
  never `None`).
- Tests: the ban; a NoHq test that each graph UserControl has the mode the brief chose; the bounds record unchanged from the before set
  at 96 DPI; the scan's text-scale run over the plain exes' forms, with its clip list attached for the UI-10 slices (5.1, the
  trade-off).
- Desktop check: the test process cannot change Windows' DPI, and the plain forms do not scale through LayoutManager, so the scan's
  simulated scales do not reach them. Before UI-2a-2 merges, the owner (or the lead in the owner's connected session) opens the
  dashboard (`FormDashboardEditTab` with its tabs, panels and cells) and Central Manager's main window at 150%, before and after: the
  layout must be the same, apart from the pixel-sized difference between the DPI ratio and the font-metric ratio (5.1; section 8).

**UI-2a-3: print fonts**
- Owns: `GridOD.cs` (print and screen font split), `GridOld.cs` and `Tables Old Grids/ContrTable.cs` if they print, the print guard.
- Needs: UI-2a-1.
- Work: 5.2, GridOD's print path. Print fonts are split out and pinned to today's. GridOD's screen fonts point at `FontGrid` and
  `FontTitle` (5.1), which hold today's sans serif 8.5 and 10 Bold.
- Tests: the print guard (section 9: GridOD print and sheet output, sheets, claim forms, the perio chart print, the claim perio
  attachment, eForm PDF output, reports, RDL and MigraDoc output, identical before and after).

**UI-2a-4 to UI-2a-n: the code font sites and the plain forms' designer fonts**
- Slices: by folder, in the folder order of UI-9a, at most about 40 files each; the brief sets the cut from a fresh grep. The last
  slice is the other projects of UI-9a-7: the plain exes' Microsoft Sans Serif designer fonts (CentralManager 4, xCrudGenerator 2,
  RdlDesign 2; the graph's 6 go with UI-9a-6's folders) and their code font sites.
- Owns per slice: its files among the about 59 code lines naming Microsoft Sans Serif and the about 40 `GenericSansSerif` files, and
  the designer fonts in its folders of the plain forms and of the FormODBase windows without `InitializeLayoutManager` (5.1). The
  brief's grep list has both: the plain forms, and the FormODBase subclasses whose constructor does not call `InitializeLayoutManager`
  (on `main` the five allocator windows of 5.1, in the slice that owns `ReportingForRdl`, which is in UI-9a-6's folders).
- Needs: UI-2a-1, and UI-2a-3 (its print guard).
- Work: each site sorted into screen chrome (to the token of its role, 5.1), print, or document and content (stays, 5.2;
  `Graphics.FromImage` pictures that are saved, attached or exported are documents). Plain forms and the windows without
  `InitializeLayoutManager`: a default font is removed so the control inherits, any other stays a literal for UI-2b (5.1).
- Tests: the "Microsoft Sans Serif" scan with its scope and allow list (section 9: code outside `InitializeComponent` everywhere, and
  the designer code of the plain forms and of the windows without `InitializeLayoutManager`; the designer code of windows and
  UserControls under LayoutManager is exempt, because the mapping covers it); the literal test for the plain forms and those windows;
  the print guard; the scan's report.

**UI-3: the window frame**
- Owns: `FormODBase.cs` and `FormFrame.cs` (frame drawing). Ideally one shared border painter in a new file both call
  (`OpenPmsDental/UI/BordersDpi/` cannot be reached from `WpfControlsOD`, so it lives in `WpfControlsOD/Frame/FramePainter.cs` or in
  CodeBase; the brief decides from the project references), the Definitions window's "(program default)" note for Misc Colors.
- Needs: UI-7a (caption glyphs).
- Work: 3.6 window frame, the floating image windows' selected and unselected frame included; Misc Colors per item (section 6, UQ-3);
  `AreBordersMS` keeps the Windows frame (section 6); try DWM rounded corners (`DWMWA_WINDOW_CORNER_PREFERENCE`) with the program's own
  non-client area and keep them only if the harness pictures are clean.
- Tests: a frame test that both painters give the same pixels for the same title; a frame test of `FormImageFloat` selected and
  unselected; the Misc Colors test with per-item cases (all stock; only the text changed; only the outline changed; all changed);
  window check A6 and `PopupHelperTests` stay green.
- Gallery: **G-UI2** (main window, modules, a few dialogs, the Imaging module with two floating image windows, one selected), with a
  stock and a customised office, and one run each with `NoCustomBorders.txt` and with `NoD2D.txt` beside the exe.

**UI-4a: core controls, part 1**
- Owns: WinForms `Button.cs`, `CheckBox.cs`, `GroupBox.cs`, `PanelOD.cs`, `ToolTipOD.cs`; WPF `Button`, `CheckBox`, `RadioButton`,
  `GroupBox`, `Panel`, `PanelAutoScroll`, `Label`, `LinkLabel`, `PictureBox`, `ToolTip`, the shared `FocusVisualStyle` and these
  controls' styles in `Generic.xaml`; the pinned copies for `CtrlEFormFill` of the templates it changes; the WPF `IsPrimary` property
  and the Frms it is set on.
- Needs: UI-7a (check mark, dash).
- Work: 3.6 for each control, WinForms and WPF together, focus included. Bounds unchanged.
- Tests: per control, a picture test of each state (rest, hover, pressed, focus, disabled, checked) against the design sheet's drawing
  (tolerance as in `ChartImageCompare`); the layout snapshot and the bounds record; `TerminologyLayoutTests` and `LayoutDesignerTests`;
  the eForm fill surface unchanged (picture test of a synthetic eForm); the scan rerun (5.4).
- Gallery: **G-UI1 part 2** (the real controls with state strips, WinForms beside WPF) and the AcceptButton list for G-UI3.

**UI-4b: core controls, part 2**
- Owns: WinForms `ComboBox.cs`, `ListBox.cs`, `TabControl/*`, `Splitters/*`, `MonthCalendarOD`, `ODDatePicker`, `ODDateRangePicker`,
  `ClinicSelector/ComboBoxClinicPicker`, `ClinicSelector/FormComboPicker`; the WPF files of the UI-4b row of 2.1,
  `WpfControlsOD/Themes/TabControlDict.xaml` and their styles in `WpfControlsOD/Themes/Generic.xaml`; the pinned copies for `CtrlEFormFill` of the templates it changes.
- Needs: UI-4a (it shares `Generic.xaml` and the focus style), UI-7a (chevrons).
- Work, tests and gallery as UI-4a, for these controls.

**UI-4c: the remaining shared controls**
- Owns: the UI-4c rows of 2.1: the appointment module's slider, pin board and day/week toggle (their chrome; the appointment book stays
  by UQ-8), the imaging sliders and bars (WinForms and WPF), `ODColorPicker`, `ODCodeRangeFilter`, `ODPictureBox`, `OdProgressBar`,
  `BufferedTreeView`, `MsgBoxCopyPaste`, the database-integrity warnings, `FormPrintPreview` and `ODPrintPreviewControl` (chrome
  only), `SignatureBoxWrapper`, `FrmColorDialog`, the WPF progress windows, the TextBelow strip (UQ-6).
- Needs: UI-4b.
- Work, tests and gallery as UI-4a, for these controls. `TextBelowStripTests` follows UQ-6 (section 9).

**UI-5: grids**
- Owns: `OpenPmsDentalBusiness/UI/GridOD.cs` (screen drawing), `GridOld.cs` if still used, `WpfControlsOD/UI/Controls/Grid.xaml(.cs)`,
  `Supplemental/Grid*`, `WpfControlsOD/Themes/DataGridDict.xaml`.
- Work: 3.6 grids, the selection bar and the focused row. Header and title heights follow the font, inside the control. Row height by
  UQ-2. Print colours stay.
- Tests: `BrandingTests.BrandColor_IsTheProgramsGridTitleBlue` moves to the token; the print guard; a grid picture test of title,
  header, hover, selection with and without focus, a coloured row; the scan rerun.
- Gallery: G-UI1 part 2 and G-UI2.

**UI-6: toolbars, menus and the module bar**
- Owns: `ToolBarOD.cs`, `ODToolBarButton*.cs`, `WpfControlsOD/UI/Controls/ToolBar*.xaml(.cs)`, `MenuOD.cs` (`MenuRenderer`), WPF
  `Menu`, `MenuItem`, `ContextMenu`, `ModuleBar.cs`, `ODButtonPanel.cs`, `ODPanelItem.cs`, `LightSignalGrid`, the body of
  `UiTheme.InitWinForms()` (the token renderer), and its call in every exe's `Main` (not `UpdateFileCopier` or
  `OpenPmsDentalHelper32`, 3.6) and at every extra UI thread's start, and RdlDesign's linked files in `RdlDesign/ReportDesigner.csproj`.
- Needs: UI-7a (chevrons), UI-7b (the module icons in their Fluent Regular and Filled styles).
- Work: 3.6 toolbars, menus, module bar. Item widths keep today's formula (3.3, UQ-9).
- Tests: C20 (MenuOD font equals the form's font); a toolbar test for every module toolbar and the main menu at window widths 1024,
  1366 and 1920 px: the items keep their order and none runs past the right edge (or the overflow is listed for the owner); the
  renderer is set in every exe and on a second UI thread (the test names its two exceptions, `UpdateFileCopier` and
  `OpenPmsDentalHelper32`, and checks RdlDesign through its linked renderer); picture tests of each state; the scan rerun.
- Gallery: G-UI2, with each module toolbar and the menu bar as before and after strips.

**UI-7b: the IconLibrary icons**
- Owns: both `IconLibrary` folders, the `EnumIcons` rows of `icons/manifest.json` and `icons/README.md`.
- Needs: UI-7a.
- Work: map the 38 + 53 `EnumIcons` (module bar included; Chart32G, Chart32W and ChartMed32 keep today's pictures until UI-8); the
  38 + 37 `Gen_*.cs` files go.
- Tests: every `EnumIcons` has a manifest row and draws non-empty at every size on GDI+, Direct2D and WPF, and on the OnlyGDI path and
  in NoDpi mode; `InProcChecks.DE` `WpfImages` counts vector icons (section 9).
- Gallery: **G-UI1 part 3** (today's icon beside the Fluent icon, 16/20/24 px, 100/125/150/200%).

**UI-8: custom dental icons**
- Owns: `icons/custom/*.svg`, their manifest rows, `db/init/new-database.sql` (default procedure-button pictures only, if UQ-7 says yes).
- Needs: UI-7a.
- Work: section 4.4.
- Tests: as UI-7b; a test that the seeded procedure-button pictures are 20×20; a test that no office row is changed by any code path
  (the pictures are only in the new-database script).
- Gallery: **G-UI1 part 4** (each dental icon at each size, beside today's picture).

**Slices.** The remaining work touches hundreds of designer files, the same files the 26.2 port edits. It is cut into fixed slices of
at most about 40 designer files or one module each. Each slice is one package: its own writer and review, its own gallery slice, its
own 26.2 overlap check before it starts. The first cut is below; a brief that finds a slice over the cap splits it in two and says so.
"Forms A" means `OpenPmsDental/Forms/FormA*` and so on.

**UI-9a: bitmap pictures to icons** (Resources and per-form pictures; 186 files, 361 assignments)
- Slices: UI-9a-1 Forms A-C; UI-9a-2 Forms D-L; UI-9a-3 Forms M-Q; UI-9a-4 Forms R-S; UI-9a-5 Forms T-Z and `Forms Reports`;
  UI-9a-6 `Main Modules`, `User Controls`, `UI`, `UIManagement`, `Logic`, `Bridges`, `ReportingForRdl`, `ReportingComplex`,
  `SheetFramework`, `SmartCards`, `Misc`, `Tables Old Grids`, OpenPmsDentalBusiness, OpenPmsDentalGraph,
  SparksToothChart, the WPF `BitmapFileName` uses (11) and resource pictures (7); UI-9a-7 the other projects (below). The 70 per-form
  `.resx` pictures go with their form's slice.
- UI-9a-7, the other projects: CentralManager, RdlDesign, ServiceManager, MySQLManager, xCrudGenerator, OpenPmsDentalDbSettings,
  OpenPmsDentalHL7, DatabaseIntegrityCheck, DocumentationBuilder, UpdateFileCopier, OpenPmsDentalHelper32, CodeBase and ODR. Their
  pictures on `main` (the brief greps again):
  - CentralManager: 11 `.resx` button pictures in 6 designer files (for example `FormCentralConnections.Designer.cs:122-144`, up, down
    and add). It references OpenPmsDental and CodeBase, so it uses `UI.Button.Icon` or `GetBitmap` like the product.
  - CodeBase `Utilities/MsgBoxCopyPaste.cs:89` (a second copy, outside `OpenPmsDental/UI`, of the print button) and
    `ODR/ComboBoxMulti.cs:65` (the drop-down arrow): one each, through the CodeBase renderer's `GetBitmap` (both reference CodeBase).
  - RdlDesign: 21 toolbar pictures on hidden designer buttons (`RdlDesigner.cs:1034-1234`, `.resx`), which `InitToolbarMenu` and
    `InitToolbarInsertToogle` copy to the real toolbar buttons at run time (`RdlDesigner.cs:420-483`). RdlDesign references no
    CodeBase, so it compiles in `CodeBase/UI/Icons/IconData.g.cs` and the GDI+ renderer as linked files, the way it links the token
    colours (3.6) and `ODNumberTextNetFx.cs`; UI-7a keeps that renderer in a file of its own that uses only `System.Drawing`. The
    toolbar button's size comes from its picture (`ctl.Height = img.Height + 5`, `ctl.Width = img.Width + 8`, lines 732-733 and the
    same at 799-800), and each button's place follows from the ones before it, so the generated bitmap keeps today's picture's pixel
    size at 96 DPI and nothing moves. The fyiReporting logo in its About box (`DialogAbout.cs:203`) stays (4.5).
  - The other projects have no pictures on `main`; the slice confirms that.
- Owns per slice: its files' `Image=Properties.Resources.*` lines and `.resx` pictures, and the `Resources.resx` entries whose last use
  it replaces.
- Needs: UI-7b, UI-8.
- Work: switch to `UI.Button.Icon=EnumIcons...` where the control supports it, otherwise `IconLibrary.GetBitmap` at the DPI size.
  Positions unchanged.
- Tests: no designer `Image=` points to a removed picture; `RenameGuardTests` (pack URIs) updated; the layout snapshot and the bounds
  record.
- Gallery: G-UI3, the slice's windows.

**UI-9b: ImageLists to icons** (toolbars and trees; 50 `.resx` files, 55 lists, 244 pictures, 318 `ODToolBarButton` calls)
- Slices: UI-9b-1 the main window and the module toolbars (`Main Modules`, `FormOpenDental`), with `OpenPmsDental/Logic/ProgramL.cs`;
  UI-9b-2 the imaging module and its tree; UI-9b-3 the other trees (tasks, setup wizard, security tree, auto notes, email inbox);
  UI-9b-4 the remaining forms' toolbars and lists.
- Needs: UI-9a for the same folders (a file is in one slice at a time).
- Work: toolbar buttons take `EnumIcons` instead of an index; trees get a DPI-sized ImageList from `GetImageList`. `ImageIndex` keeps
  working: `ToolBarOD` keeps its `ImageList` property, and a button with an index and no icon draws from the list as today. The module
  toolbars keep an `ImageList` built by `GetImageList` that also holds the program-link pictures (`ProgramL.cs:63` looks them up by
  key) and anything a plug-in adds through the six `LayoutToolBar_end` hooks.
- Tests: `SourceScanTests.ImageLists_TakeTheirDepthFromAStreamOrSetIt` allows the one icon factory; the program-link pictures from
  the database still show (a test with a synthetic program row); a test toolbar button set by `ImageIndex` still draws its picture; the
  scan rerun after the last slice.
- Gallery: G-UI3, the slice's windows.

**UI-10: text fit**
- Slices: UI-10-1 FormPatientEdit, FormProvEdit, FormClaimEdit, FormProcEdit, ControlAccount and the preference pages (the largest
  first); then UI-10-2 to UI-10-n, each the next files of the scan's TSV in order, at most about 40 designer files. With 422 files at
  9 pt that is about 11 slices; with 182 files at 8.25 pt about 5.
- Owns per slice: its designer files (size edits only, plus `AutoSize = false` with an explicit `Size` on a control that sizes itself,
  5.4) and its rows of `tests/UiRefreshTests/Data/fit-fixes.tsv`.
- Needs: UQ-1 answered; UI-4a to UI-6 and UI-9b merged (they change what fits), and the scan's run after them. The plain exes' forms
  that clip in the text-scale run (UI-2a-2) are part of the TSV.
- Work: section 5.4, "the fixes", against the new font while `main` still shows the old one.
- Tests: the slice's files report nothing new in the scan for the new font, or every remaining item is named and approved; the layout
  snapshot and the bounds record.
- Gallery: G-UI3, the slice's windows drawn in the new font through the token override (5.4), before and after the fixes, and the
  "needs the owner" list.

**UI-2b: the flip**
- Owns: the token values, the 12 exe literals, the remaining literal designer fonts of the plain forms and of the five windows without
  `InitializeLayoutManager` (5.1, UI-2a-4), the tests' expected
  token if any test names it, and the note in
  `docs/specs/net10-plan.md` (decision 9, the S3b-4 row and the "Default font" row) that this plan replaces them.
- Needs: every UI-10 slice merged.
- Work: the font tokens take their UI-2b values (3.4): body Segoe UI 9 pt (or 8.25, UQ-1), grid by UQ-2, titles Segoe UI Semibold
  10, module captions Segoe UI 8. From then on the mapping applies the size rule of 3.4.
- Tests: section 9's font group; the scan reports nothing new; the bounds record differs only by `fit-fixes.tsv` and by the growth of
  controls that size themselves within their rule (anchored edge unchanged, no new overlap, 5.4); the print guard;
  the tooth chart baselines; the content-exclusion and signature tests.
- Gallery: **G-UI2, font pass** (main window and modules) with the scan's counts, and the owner's desktop check at 125% and 150%
  (section 8).

**UI-11: the rest and the final pass**
- Slices: UI-11-1 to UI-11-n, the designer `BackColor` values that mean chrome rather than data (228 files set one; each brief
  separates chrome from data), about 40 files per slice in the folder order of UI-9a. The last of them is the other projects of
  UI-9a-7 (on `main` about 37 files set one: CentralManager 14, RdlDesign 6, CodeBase 5, ServiceManager 3, MySQLManager 2, and one each in
  DatabaseIntegrityCheck, DocumentationBuilder, OpenPmsDentalHL7, OpenPmsDentalHelper32, UpdateFileCopier, xCrudGenerator and ODR;
  RdlDesign's report canvas colours are content and stay). Then UI-11-final.
- UI-11-final owns: `ColorOD` (it now points at the tokens, section 6), the leftovers of section 6, `loadingAnim.gif` as a vector
  progress ring, the release notes.
- Work: the sweep of every window (G-UI4), fixes it finds, the notes: the classic look is gone; no conversion; Misc Colors still work.
- Tests: the whole Full run; the source scans' allow lists at their final size.
- Gallery: G-UI3 slices for the `BackColor` slices; **G-UI4** for UI-11-final, including the files that still use `ColorOD`.

---

## 8. Galleries and the owner's approval

**Names.** G-UI1 to G-UI4. The tooth chart's galleries are G0 to G5, so the names must not be shortened.

**How a gallery is made.** `tools/New-UiGallery.ps1` takes a before folder, an after folder and the mask file. It writes, per group:
before, after, a diff, % changed, the after picture's pixel SHA-256 and the clip notes, plus `index.html` and a zip. Pictures are
synthetic only (the harness's `winchk_` copies of the synthetic dump; no patient data). WPF draws nothing while the session is locked, so
a gallery run needs the owner's connected, unlocked session; a full set takes about 14 minutes in three runs.

**Pictures at their real size.** The galleries judge 1 px lines and small icons, and a browser at 125% or 150% would blur them by
scaling. `index.html` shows each picture at 1:1 device pixels (CSS size = picture size ÷ `devicePixelRatio`, `image-rendering:
pixelated`), with a zoom toggle for a closer look.

**The galleries, in the order the owner approves them.**
- **G-UI1, the design sheet.** Part 1 (UI-1): tokens, type ramp, a drawing of every control state, the options of UQ-1 to UQ-4, and
  five real windows (`FormPatientEdit`, `FormProcEdit`, the Account module, a module toolbar with a grid, one Frm) in each UQ-1 and
  UQ-2 option, so the font is judged on real windows. These still have today's colours and controls; colour options are judged on the
  sheet, and again on real controls in part 2. Part 2 (UI-4a to UI-4c, UI-5): the real controls, each with a state strip (rest, hover,
  pressed, focus, disabled, checked or selected; the state set in-process or by sending `WM_MOUSEMOVE`), WinForms beside WPF, drawn at
  100, 125, 150, 175 and 200% through LayoutManager's scale. Part 3 (UI-7b): every icon, old beside new. Part 4 (UI-8): the dental
  icons. Approving part 1 sets the look for everything after it.
- **G-UI2, the main window and modules.** The main window at 1366×768 and 1920×1080, the About box, the menu bar, and the seven modules
  with and without a patient, each also at one program zoom step. The tooth chart area of the Chart module is checked as unchanged, not
  shown as a change. UI-3's run adds a stock and a customised Misc Colors office, the Imaging module with two floating image windows
  (one selected, one not), and the `NoCustomBorders.txt` and `NoD2D.txt` runs;
  UI-6's adds the toolbar and menu strips before and after; the appointment module shows both UQ-8 options until it is answered.
- **G-UI3, the common windows.** The about 45 windows the harness already pictures plus the about 50 of group U, with the clip report.
  The UI-9 and UI-10 slices each show their own windows; UI-4a adds the list of the 87 `AcceptButton`s with the 13 that are not OK or
  Save marked for the owner.
- **G-UI4, everything else.** An off-screen sweep of every window with a parameterless constructor (582 candidates), on the scratch copy,
  with a deny list for windows that call a network or e-service on Load and a check that the database name starts with `winchk_`; plus the
  separate programs (report designer, Central Manager, tools), whose forms are pictured once more with the default font at Windows text
  size 125% (5.1, the trade-off). Each window gets a picture or a reason. Approved by exception: the owner
  looks at the list of changes, not every picture.

**How the owner answers.** The lead publishes each gallery as a private page. Each screen has "Approve" or "Change" and a note. The answers
go into `docs/owner-questions.md` (gallery, commit, date, the list of changes) and into
`tests/WindowChecksTests/UiApproved/approved.tsv` (picture, size, masked pixel SHA-256, gallery, commit, date). A package merges only
after its gallery is approved. From then on the check U0 shows only the screens that changed since their approval.

**Limits and the hand check.** The harness pictures are at 100% (96 DPI); it cannot change Windows scaling. The design sheet and
G-UI1 part 2 render 125-200% through LayoutManager's scale, which is close but not the same as Windows scaling (stock controls scale
their own fonts from the real DPI). So G-UI2's approval for UI-3 and for UI-2b includes a short check by the owner on a real desktop at
125% and at 150% (Settings > Display > Scale): the main window, two modules, `FormPatientEdit`, a toolbar, a menu, a grid with a
selected row, the dashboard (`FormDashboardEditTab`) and Central Manager's main window. The lead lists the screens in the gallery
page; the owner notes "fine" or what looks wrong. UI-2a-2, which has no gallery, gets the same check at 150% for the dashboard and
Central Manager before it merges (section 7), because the scan's simulated scales do not reach the plain forms. The same check stays in the
launch checks (`docs/launch-checks-3b.md`, item 8, "the screenshot pass's items for a person") for the final build.

---

## 9. Tests

**Tests that change (they pin today's font or colours).**

| Test | What it pins | Change | Package |
|---|---|---|---|
| `NoHqTests/DefaultFontTests.cs` | `LINE_FONT` regex (line 30) for Microsoft Sans Serif 8.25 in 12 `Main`s; the runtime probe (line 93) | the literal equals the token (UI-2a), then Segoe UI and the token size (UI-2b); the rule "first statement" stays | UI-2a |
| `tests/LibWebTests/HtmlGenerationTests.cs:233` | the old `SetDefaultFont` line | the line built from the token | UI-2a |
| `tests/ToothChartImageTests/WpfLiveViewTests.cs:76` | the old `SetDefaultFont` line | the line built from the token | UI-2a |
| `tests/ToothChartImageTests/Support/TestAssemblySetup.cs:20`, `tests/ToothChartHostTests/HostDb.cs:47` | default font Microsoft Sans Serif in the test process | Segoe UI in UI-2a already, so the 35 Simple 2D baselines prove the chart's pinned font before the flip | UI-2a |
| `UnitTests/Misc/TestBase.cs:92` | the old default | the token | UI-2a |
| `tests/WindowChecksTests/InProc/InProcChecks.cs:28` | the old default in the in-process session | the token | UI-2a |
| `InProcChecks.W5.cs:22-31` (`ExpectDefaultFont`, used by C16, C17, C21, F2, F4, G5) and `CTests.cs:30` | "Microsoft Sans Serif" | compare with the token | UI-2a |
| `InProcChecks.W5.cs:160` (C20) | MenuOD font starts with "Microsoft Sans Serif" | the strip and its items equal the form's font | UI-2a |
| `tests/M2TERMTests/TerminologyLayoutTests.cs` | 5 check boxes measured in Microsoft Sans Serif | the token font (after UI-2b it may report real clipping; UI-10 fixes it) | UI-2a |
| `tests/M2INSPLANTests/LayoutDesignerTests.cs:260` | FormInsPlan labels measured in Microsoft Sans Serif | the token font | UI-2a |
| `tests/AttributionTests/BrandingTests.cs` (`BrandColor_IsTheProgramsGridTitleBlue`) | the literal `_colorTitleBottom=Color.FromArgb(60,90,150);` in GridOD | `#3C5A96` as the `Accent` token; the branding files still checked | UI-1 (token), UI-5 (grid) |
| `tests/WindowChecksTests/SourceScanTests.cs` (`ImageLists_TakeTheirDepthFromAStreamOrSetIt`) | ImageLists only from designers | allow the one icon factory | UI-9b |
| `NoHqTests/RenameGuardTests.cs` | pack URIs and `BitmapFileName` | the remaining ones | UI-9a |
| `UnitTests/UI Testing` (FormButtonTest, Form4kTests, FormTestAllControls and others) | the old pictures | the icons | UI-9a |
| `tests/ToothChartImageTests/TextBelowStripTests.cs:88` | the strip's `ForeColor` is `Color.Black` | by UQ-6: restyled, so it compares with the `Text` token; kept, so it stays | UI-4c |
| `tests/WindowChecksTests/InProc/InProcChecks.DE.cs:38-43` (`WpfImages`, the "no blank image" check of groups D and E) | counts only `System.Windows.Controls.Image` in buttons and toolbars | also counts vector icons (`Path`, `Shape`, an `Image` with a `DrawingImage`) and requires non-empty geometry; otherwise Fluent icons pass with 0 images, a silent loss of coverage | UI-7b |
| Window check A6 (`ATests.cs:195`, `InProcChecks.A.cs:25`) and `UnitTests/UnitTests/PopupHelperTests.cs` | `NoCustomBorders.txt` gives the standard Windows frame | unchanged; must stay green | UI-3 |
| Toolbar `ImageIndex` users: `OpenPmsDental/Logic/ProgramL.cs:63` and plug-ins through the six `LayoutToolBar_end` hooks | module toolbars have an `ImageList` | unchanged; a new test keeps a button set by `ImageIndex` drawing its picture | UI-9b |

Unaffected: the data-colour tests (`BAPPTTests` TimeBarRuleTests, `M2CARRIERTests` ApptViewAndHookTests, `TaskCategoryTests`), the
branding icon tests, and the 1,682 + 18 tooth chart baselines, which must stay identical (decision 5).

**New tests.**
- `tests/UiRefreshTests` (UI-0): the layout snapshot test; the tier 1 text-fit scan with its scales, text-scale run, the overlap check
  of controls that size themselves, and the bounds record with their separate rule;
  the test-side font swap; the token override `UiTheme.SetFontForTest` (UI-2a-1); the design sheet (UI-1); the contrast test with focus, selection and high-contrast pairs (UI-1); the
  two-thread paint test (UI-1); the coverage test (UI-1); control state pictures (UI-4a to UI-6).
- NoHq source scans: no new hard-coded colours in the shared controls (UI-1); no `using` or `Dispose` around `UiTheme` pens and brushes
  (UI-1); no "Microsoft Sans Serif" outside an allow list (UI-2a). Its scope: code outside `InitializeComponent` in every project, and
  the designer code (`InitializeComponent`, wherever it sits) of the plain forms, of the FormODBase windows without
  `InitializeLayoutManager` (5.1) and of UserControls that no FormODBase window hosts. The designer code of windows and UserControls
  under LayoutManager is exempt: the mapping changes those designer fonts (most of the 418) at run time and they stay in the files for
  good (5.1). The scan takes "under LayoutManager" from the same list tier 1 uses to decide whether to call
  `InitializeLayoutManager` or host a UserControl in the test FormODBase (5.4). Its allow list: the token itself, the tooth chart and
  `ContrPerio` pins, print code, document design canvases, `FormPerio`'s print font, the claim perio attachment's two fonts in
  `FormClaimEdit`, and the plain forms' and those windows' literal designer fonts until UI-2b; no `AutoScaleMode.Font` assignment
  outside `UnitTests` and `PluginExample`, comments skipped (UI-2a-2); each exe's font literal, and each literal designer font of a
  plain form or of a window without `InitializeLayoutManager`, matches the token by the rule of 3.4 (UI-2a); no bitmap toolbar
  pictures outside an allow list (UI-9b).
- Print guard (UI-2a-3, again in UI-2b): GridOD's print and sheet output, sheets, claim forms, the perio chart print, the claim perio
  attachment picture, eForm PDF output, reports, RDL and MigraDoc output identical before and after; a printed appointment schedule too
  if UQ-8 changes the appointment book.
- Content and signatures (UI-2a-1): a Microsoft Sans Serif sheet field keeps its font in `FormSheetFillEdit`; a perio chart picture is
  unchanged with the token override set to Segoe UI; a late `MarkContent` throws; a signed group note in `FormProcEdit` stays valid
  after a zoom change, a resize and a save.
- Font mapping (UI-2a-1): with today's tokens every font comes back unchanged; with the override set to Segoe UI, explicit designer
  fonts follow the token in all three recording places (a window's designer controls, an `Add`ed control, a preference page and
  `ControlImages` through `AddUnscaled`); the underlined `FormInsBenefits` labels draw as today.
- Misc Colors per item and the floating image windows' two frames (UI-3); toolbar order and right-edge fit at 1024, 1366 and 1920 px,
  and the renderer on every exe but the two named exceptions and on a second UI thread (UI-6); a toolbar button set by `ImageIndex`
  still draws (UI-9b).
- Icon tests (UI-7a, UI-7b, UI-8): manifest complete, every icon draws at every size on GDI+, Direct2D and WPF, on the OnlyGDI path and
  in NoDpi mode; generated file in step; notices present.
- Harness (UI-0): group U, the walker with its bounds record (UI-0b's shared measuring files), the check U0.

---

## 10. Risks

- **Text that no longer fits.** Segoe UI is wider and taller. 391 to 1,591 fixed-size controls newly overflow (5.3), and some fixes may
  not fit without touching a neighbour, which decision 1 forbids. Mitigations: the two-tier scan with an ink check, size-only fixes, the
  owner sees every case that needs more, and UQ-1's smaller size if the count is too high.
- **Fewer grid rows.** Rows grow from 14 to 16 or 17 px, about 15-20% fewer rows on screen. Mitigation: UQ-2, shown in G-UI2.
- **Toolbars run out of room, and strip items shift.** Toolbar buttons, top-level menu items, tab headers and button-panel items size
  from their text and get about 7-10% wider, so later items shift right and full module toolbars may hit the right edge at 1024-1366
  px. Mitigation: UQ-9; UI-6's order and right-edge test at 1024, 1366 and 1920 px; before and after strips in G-UI2.
- **Windows grow at run time.** 47 containers scale their children by font (`AutoScaleMode.Font`), so a font change moves and
  grows them where no designer file shows it. Mitigation: those containers change in UI-2a, a source scan bans the mode, and the scan
  and the walker compare run-time bounds (3.7).
- **Controls that size themselves grow into their neighbours.** `AutoSize` labels, radio buttons and check boxes (138 in designers) and
  single-line text boxes (about 3 px taller at 9 pt) grow with the font, and no fixed-size count shows it (5.3). Mitigation: the scan
  and the walker record their preferred size and report new overlaps (5.4); UI-10's listed fix (`AutoSize = false` with an explicit
  size); the owner sees what cannot be fixed in place; UQ-1's numbers include them.
- **The plain programs clip at a larger Windows text size.** With `AutoScaleMode.Dpi` their text grows with the text size setting but
  their layout no longer does (5.1). Mitigation: the scan's text-scale run over their forms, G-UI4 at text size 125%, size-only fixes
  in UI-10, and the fallback of 5.1 for a form that size-only fixes cannot cover.
- **Documents on screen change.** The family mapping would also reach filled sheets and the perio chart, and a content control that
  sets no font of its own would take the window's new font anyway. Mitigation: the content types, `MarkContent` and the font pins, with
  their tests (5.1).
- **Printouts change by accident.** GridOD prints with its screen fonts today, and so does the appointment book; some code draws a
  document into a picture (the claim perio attachment). Mitigation: print fonts split and pinned first (UI-2a-3), the appointment book
  left alone unless UQ-8 says otherwise, `Graphics.FromImage` documents sorted as documents (5.1), and the print guard.
- **A font change clears a signature.** Setting a font fires `TextChanged` (Job 55866). Mitigation: the mapping runs once per control,
  when it is recorded (designer controls before data loads; for controls added later the brief checks each `Add`, `AddUnscaled` and
  `Synch` caller, 5.1); the rebuild is unchanged and still sets a font only when the size changes; the signature test (UI-2a).
- **The font plumbing changes the look early.** Routing a site through one body token would turn Arial 8 and sans serif 8.5 into
  Microsoft Sans Serif 8.25, and a mapping that applied the size rule early would change the 8 and 8.5 pt designer fonts. Mitigation:
  tokens by role holding today's values, the mapping as the identity until UI-2b (3.4, 5.1), and U0 with the bounds record in every
  UI-2a slice.
- **Random paint crashes from shared GDI+ objects.** Two UI threads drawing with one cached pen throw "Object is currently in use
  elsewhere". Mitigation: per-thread caches and the two-thread test (3.2).
- **Blurry lines above 100%.** Fractional scales smear 1 px borders. Mitigation: the pixel-snapping rule (3.3), galleries at 125-200%
  and the owner's desktop check (section 8).
- **The tooth chart changes by accident.** The chart test assemblies set Microsoft Sans Serif themselves, so a Simple 2D change would
  pass unseen. Mitigation: the font pin in the product and the test default set to the new font, in the same package.
- **WinForms and WPF drift apart.** The frame, icon library, grid and toolbar each have two copies. Mitigation: one token class, one icon
  table, and each pair restyled in one package.
- **Shared pens and brushes disposed by mistake.** Paint code that wraps a cached token pen in `using` would break it for everyone on
  that thread. `Pen` and `SolidBrush` are sealed, so they cannot be made to ignore `Dispose`. Mitigation: the source scan for `using`
  and `Dispose` around them (UI-1).
- **Blank icons on some PCs.** The Direct2D path fails on some machines (the OnlyGDI fallback). Mitigation: every icon test runs on GDI+
  too.
- **Dental icons that look worse than Open Dental's.** Mitigation: a design pass and G-UI1 part 4 before they replace anything.
- **Staff miss the colours.** Open Dental's icons are colourful (red X, blue check, yellow umbrella). Mitigation: role colours (UQ-4).
- **Gallery noise.** 92 of WINCHK-5's 145 picture pairs differed, mostly from the clock and the copy's name. Mitigation: masks and fixed
  window sizes (UI-0).
- **The off-screen sweep runs Load code that writes or calls out.** Mitigation: scratch copy only, database name check, deny list.
- **Merge conflicts with the 26.2 port.** Both touch designer files. Mitigation: slices of at most about 40 designer files, and the
  overlap check before each slice (section 7).
- **Not determined by the surveys.** Whether DWM rounded corners work with the program's own title bar; what the .NET 10 diagnostic
  WFO5003 covers; how stock TreeView and ListView scale ImageLists at high DPI; whether the report designer, Central Manager, MySQL
  Manager and Database Integrity Check are in the installer (their windows are in scope either way: "Segoe UI everywhere"); whether any
  picture is looked up by a computed resource name; whether the synthetic dump has rows for every group U window (claims, recalls, pay
  plans, tasks, Rx, labs); fit above 96 DPI; texts in other languages; whether WinForms keeps `ToolStripManager.Renderer` per thread in
  .NET 10 (UI-6 checks).

---

## 11. Questions for the owner

**UQ-1. Body font size.**
- (a) Segoe UI 9 pt. **Recommended.** It is Windows' own size and matches the WPF windows (labels aside, 3.4). About 1,591
  controls need checking; about 640 need more width or another line. On top of that, controls that size themselves grow by
  themselves (5.3): 138 `AutoSize` controls, and every single-line text box gets about 3 px taller (about 23 px instead of 20), which
  matters where rows of inputs are stacked closely. UI-0's scan counts the new overlaps for both options.
- (b) Segoe UI 8.25 pt. Same line height as today and about a quarter of the fixes (391), but smaller than the WPF windows.
- G-UI1 part 1 shows both on five real windows. Labels and radio buttons draw at 0.96 of the size, as they do today (3.4); say if you
  want them at the full size instead.
- The counts are from a first model; UI-0's scan counts again before UI-10 starts.

**UQ-2. Grid row height.**
- (a) Rows follow the body font: 17 px at 9 pt (today 14). About 20% fewer rows on screen. **Recommended**: easier to read.
- (b) Compact: grid cells in Segoe UI 8.25, 16 px rows, about 13% fewer rows.

**UQ-3. Title bar.**
- (a) The program's blue (`#3C5A96`) with white text, as in Open Dental today. **Recommended**: familiar to staff and part of the brand.
- (b) A light Windows 11 title bar (`#F3F3F3`, dark text).
- Either way, Misc Colors 11-13 work item by item: an item that still holds Open Dental's stock value gives the new look, and an item the
  office changed keeps the office's colour (section 6). The Definitions window marks stock items "(program default)". Do you agree
  with that rule?

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

**UQ-8. The appointment book's text.** The appointment boxes, time bars and operatory headers draw in Microsoft Sans Serif 8 (or the
size an office's appointment view sets), and the row height comes from that font (5.2).
- (a) Keep them as they are, like the appointment colours: the appointment book is the office's schedule, and its printout matches the
  screen. **Recommended**: no row grows, the same hours fit on screen, and printed schedules cannot change.
- (b) Segoe UI, like the rest of the window. Rows get taller by the font's extra height, so fewer hours fit, and the print path needs its
  own fonts first.
- G-UI2 shows the Appts module both ways. The toolbar, the slider and the rest of the module's chrome are restyled either way.

**UQ-9. Items that are laid out from their text.** Toolbar buttons, top-level menu items, self-drawn tab headers and button-panel items
take their width from their text today (3.3). With Segoe UI each gets about 7-10% wider.
- (a) They reflow: each item grows by its text, the items after it shift right, the order stays the same. **Recommended**: no text is
  cut, and staff find the same buttons in the same order. UI-6 checks the toolbars and the menu bar still fit at 1024, 1366 and 1920 px.
- (b) Each item keeps today's width and its text is cut with "…" when it does not fit. Nothing shifts, but some captions are cut.
- G-UI2 shows each module toolbar and the menu bar before and after.

---

## 12. Review notes

The first review (2026-11-17) raised 19 problems. All were applied (sections 1 to 11). Where the review's facts or fix were not quite
right, the plan follows the code on `main` instead:

- **`AutoScaleMode.Font` containers.** The review's list missed two in the product: `OpenPmsDental/User Controls/ControlImageDisplay`
  and `ControlImageDock` (`git grep AutoScaleMode.Font main`). Its fix, `AutoScaleMode.None` everywhere, would break the plain exes at
  high DPI: they run PerMonitorV2 (`SetHighDpiMode` in each `Main`) and rely on that scaling. The plan uses `AutoScaleMode.Dpi` with
  96F,96F for their forms and `None` only for UserControls that LayoutManager already scales (5.1).
- **The appointment book.** The review said appointment rows have a fixed height, so taller text would clip. In fact the line height
  comes from the font (`_heightLine=fontForHeight.Height`, `ControlApptPanel.cs:729` and again at 1732 after printing), so the whole
  schedule would get taller and show fewer hours. The conclusion holds and is stronger: the plan keeps the appointment book's fonts by
  default and asks UQ-8.
- **Where the family mapping runs.** The plan already put the mapping in `AddToList96` as well as in the rebuild. The review's fix
  makes the rebuild compare the family too. The plan instead maps once where each control is recorded (corrected in round 2: the
  constructor's loop as well as `AddToList96`) and leaves the rebuild keeping each control's current family: a font that code sets after the control was added (for example `ControlAppt.cs:3182`) would otherwise be turned back
  by every rebuild, and every rebuild would be one more chance to fire `TextChanged` (Job 55866). (Corrected in round 3: the rebuild
  does not change at all. Teaching it to keep Italic and Underline would make today's underlined labels show their underline, a visible
  change against Open Dental; see the round 3 notes.)
- **Accessibility tokens.** No `AccentDisabled` token: a disabled default button draws like any disabled button, as Windows 11 does.
  No `TitleInactive` token: the program's title bar has no inactive state today (FormODBase and FormFrame have no activation handling
  for the frame colour), so adding one would be new behaviour, not a restyle. (Round 2: one frame state does exist, the floating image
  windows' selected and unselected frame; it is now in 3.6.) The review's darker selection fill (`#C5D5EE`) was not
  used: it is still only 1.49:1 on white, short of 3:1, and lowers the text contrast; the 2 px `Accent` bar meets 3:1 on its own (3.2).
- **Default buttons that are not OK.** The review said some `AcceptButton`s are Search buttons. None is on `main`: of the 87, 40 are
  `butSave`, 34 `butOK` or `butOk`, 5 `butPrint`, 2 `butSend`, and one each `butTryAgain`, `butRunReport`, `butEdit`, `butCreate`,
  `butCheck` and `button1`. The review's fix still stands for those 13: the owner confirms them in G-UI3.
- **Plug-ins and `ImageIndex`.** The review cited 26 places in PluginExample and plug-in code that index module ImageLists.
  `PluginExample` has no `ImageIndex` or `ImageList` code on `main`. The real users are `OpenPmsDental/Logic/ProgramL.cs:63` (program
  links, by key) and whatever an outside plug-in does through the six `LayoutToolBar_end` hooks (`ControlAccount`, `ControlAppt`,
  `ControlChart`, `ControlFamily`, `ControlTreat`, `FormOpenDental`). `ImageIndex` keeps working either way (UI-9b).
- **`ToolStripManager.Renderer`.** No code sets it on `main`; it was only proposed for `ProgramEntry.cs`. Besides every exe, WinForms
  keeps this setting per UI thread, so it is also set where the program starts another UI thread (3.6, UI-6).
- **Disposal of shared pens.** Not from the review: the old mitigation "pens that ignore `Dispose`" cannot work, because `Pen` and
  `SolidBrush` are sealed. Only the source scan is left (section 10).
- **Fit fixes before the flip.** The review asked for the font flip to land together with the fit fixes. The plan merges the fixes in
  slices first, while `main` still shows the old font, and flips last (UI-2b). That keeps each slice small for review and for the 26.2
  overlap check, and `main` never shows clipped text.

The second review (2026-11-17) raised 10 problems. All were applied. Its facts held on `main`; where its fix was not quite right, or
checking it turned up more, the plan follows the code:

- **The mapping's place.** Right: designer controls are recorded in the `LayoutManagerForms` constructor (133-143, `FontSize96` at
  1391), not in `AddToList96`, so the round 1 plan would have left the designer fonts unmapped. The mapping now runs in both places
  (5.1). The review's fix for content (every `MarkContent` call before `InitializeLayoutManager`) was taken for marked panels only; the
  tooth chart, `ContrPerio` and `ODPrintPreviewControl` are recognised by type, which needs no call order and covers every window that
  hosts them. And checking it showed a gap the review did not name: `ContrPerio` sets no font (its constructor, `ContrPerio.cs:127-135`),
  so it inherits its window's font, which the token changes whether or not the mapping skips it. It now pins its font, like
  `ToothChart2D` (5.1).
- **The claim perio attachment.** Right (`FormClaimEdit.cs:2479-2545`). Besides the review's fix (the two fonts as a document, the
  allow list, the print guard), the chart in the picture comes from a `ContrPerio` built on its own (2485), which takes the application
  default font; after the flip it would draw in Segoe UI. `ContrPerio`'s pin covers it.
- **Plain forms' designer fonts.** Right that a designer cannot hold a reference to the token. But the round 1 plan, and the review
  after it, sized the job from "RdlDesign's 61 designer fonts". Most of those are the 55 Arial 8.25 Bold Italic headings, with a few
  Wingdings and Courier New fonts, which stay by the rule of 3.4; only 2 of RdlDesign's name Microsoft Sans Serif, and about 14 outside
  OpenPmsDental do in all (`git grep 'Font("Microsoft Sans Serif"' main`). Removing an explicit font works only where it equals the default; a Bold or other-size
  font stays a literal, which UI-2b changes and a NoHq test checks (5.1). So UI-2a-4's slices are mostly the code font sites.
- **The renderer in every exe.** Right about `UpdateFileCopier` and `OpenPmsDentalHelper32`. The review missed RdlDesign, which also
  references no CodeBase (`RdlDesign/ReportDesigner.csproj`) but has a stock `MenuStrip` (`RdlDesigner.cs:1493`), a `StatusStrip` and
  5 context menus. Exempting it would leave the report designer's menus in the old look, so it links the token colours and the renderer
  as it already links `ODNumberTextNetFx.cs` (3.6).
- **Plain exes and text size.** Right. The plan notes the trade-off, scans and pictures the plain exes' forms at text size 125%, and
  names a fallback: a form can keep `AutoScaleMode.Font` with the new font's `AutoScaleDimensions` (5.1).
- **UI-6 and UI-7b.** Right. UI-6 now waits for UI-7b. The order keeps UI-6 last in its line rather than moving the module bar's icon
  states into UI-7b, so that UI-7b stays an icon-files-only package.
- The tier 1 scan's font, the UI-0 and UI-2a slices, the floating image windows' frame and the `WpfControlsOD/Themes` path were applied
  as the review proposed.

The third review (2026-11-17) raised 7 problems. All were applied; its facts held on `main` except where noted:

- **`AddUnscaled`.** Right: `AddToList96Unscaled` (`LayoutManagerForms.cs:245-246`) is a third place a `Control96Info` is made, used
  for the preference pages, the setup wizard's pages and `ControlImages`. The mapping runs there too (5.1). Its fix was taken as given.
  `SynchRecursive` (495-504), which also calls `AddToList96`, sits inside a comment block on `main` and has only commented-out callers,
  so it needs nothing.
- **The rebuild's style.** Right, and the round 1 and 2 plans had it wrong: the rebuild keeps only Bold, and the 0.96 factor makes
  every Label and RadioButton rebuild on its first layout, so today the underlined labels of `FormInsBenefits` and
  `FormProcButtonEdit` show without their underline, as in Open Dental. The rebuild now stays as it is (5.1). No owner question is
  added: showing the designer styles is not something the owner asked for, and it would move away from Open Dental's look.
- **One token for every site.** Right. One small correction: `ODButtonPanel.cs:64` uses sans serif 8.5, not 8.25 (the review's
  example); it gets its own role, `FontButtonPanel`. The roles are `FontBody`, `FontGrid`, `FontButtonPanel`, `FontTitle`,
  `FontModuleCaption` and `FontFamilyName` (3.4). The review's fix for the size rule (the helper changes size only when the family is not
  Microsoft Sans Serif) was taken in a slightly stronger form: while the body family is Microsoft Sans Serif the helper returns the
  font unchanged.
- **Tier 1 UserControls.** Right. The plan takes the hosting fix (a bare test FormODBase, `Add` or `AddUnscaled` as the product
  does), not the test-side swap for UserControls, so the scan runs the product's own mapping and scale steps on the largest files (5.4).
- **Pictures outside the named slices.** Right about the projects and counts, with one correction: the report designer's toolbar does
  not load `.gif` files at run time. `InitToolbarMenu` and `InitToolbarInsertToogle` (`RdlDesigner.cs:710`, `778`) copy the
  `.Image` of hidden designer buttons (`bNew.Image` and the others, `RdlDesigner.cs:420-483`), which come from `RdlDesigner.resx`
  (`RdlDesigner.cs:1034-1234`). The 21 `.gif` files in the folder are not used by any project file or code (4.5). Checking it showed one
  more constraint: those toolbar buttons take their size from the picture (`RdlDesigner.cs:732-733`, `799-800`), so the new bitmaps keep
  today's pixel size (UI-9a-7). RdlDesign gets the icons by linking the generated table and the GDI+ renderer, as the review proposed.
  The `DialogAbout.cs:203` picture is the fyiReporting logo and stays (4.5).
- **`ODWebView2` and the ban.** Right: 47 live assignments; the ban matches assignments only, outside comments (5.1).
- **UI-0c's measurer.** Right. UI-0b owns the measuring files and UI-0c links them (section 7).

The fourth review (2026-11-17) raised 4 problems. All were applied; its facts held on `main`, with these corrections and additions:

- **Controls that size themselves.** Right that the plan had no rule for them and that the bounds rule would fail on every one. The
  count on `main` is 138 lines that set `AutoSize = true` in 47 files (41 in OpenPmsDental), not 150 in 38 + others; by type 94
  labels, 28 radio buttons, 12 check boxes, two layout panels and two forms (5.3). Checking it turned up more than the review named:
  `UI.CheckBox` is a `Control`, not a `ButtonBase` (`CheckBox.cs:23`), so the switch-off at `LayoutManagerForms.cs:395-397` does not
  reach it either; and every single-line text box has `AutoSize` on by default without a designer line, so its height follows the font
  (about 20 px today, about 23 px at Segoe UI 9). The walker therefore reads `AutoSize` at run time instead of from the designer, and
  UQ-1 names the text box height. The review's "Location must stay the same" was refined: the anchored edge stays (WinForms grows a
  control anchored only right or bottom the other way), and children of a flow or table layout panel are compared by order (5.4).
- **FormODBase windows without `InitializeLayoutManager`.** Right: the five allocator windows (5.1). Applying it showed that UI-9a-6's
  folder list, which also sets UI-2a-4's and UI-11's cut, did not name `ReportingForRdl` at all, nor `ReportingComplex`,
  `SheetFramework`, `SmartCards`, `Misc` and `Tables Old Grids`; they are now in UI-9a-6. The plan does not add
  `InitializeLayoutManager` to the five windows: that would start LayoutManager's scaling and anchoring there, a layout change.
- **The graph UserControls.** Right (5.1, UI-2a-2). The review's place for the check, G-UI4 at 150%, does not exist: the harness and
  G-UI4 picture at 96 DPI (section 8), and the scan's simulated scales go through LayoutManager, which the plain graph forms do not use.
  So UI-2a-2 gets a desktop check at 150% of the dashboard and Central Manager before it merges, and the same windows join the owner's
  desktop check of section 8. A NoHq test keeps the graph UserControls off `None`.
- **The "Microsoft Sans Serif" scan's scope.** Right. The plan takes the review's first option, a scope, and ties it to the list tier 1
  already needs (which types are under LayoutManager), so the ban and the text-fit scan cannot disagree about a window (section 9, UI-2a-4).
