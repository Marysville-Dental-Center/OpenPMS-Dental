# The UI refresh: package plan

**What this is.** The plan for a new look for the whole program: modern icons, buttons and styles, clean and professional. The owner
asked for it on 2026-11-17 ("Do ui refresh. Modernize icons, buttons, styles. Keep it clean and professional") and set six decisions the
same day (section 1). Package briefs are cut from this document the same way as from `docs/specs/net10-plan.md` and
`docs/specs/wpf-tooth-chart-plan.md`.

**Date.** 2026-11-17. **Status.** Proposed, review rounds 1 to 5 applied (section 12). Waiting for the owner's answers to UQ-1 to UQ-9
(section 11) and for gallery G-UI1 part 1 (section 8), which shows the design before any window changes.

**Basis.**
- Four read-only surveys of `main` at `8d34b27a0` (2026-11-17): the shared controls and their colours, the icons, the fonts (with
  measurements on the build PC at 96 DPI), and the tooling for pictures and approval.
- The public Fluent UI System Icons repository and npm registry (checked 2026-11-17).
- The WCAG 2.2 contrast rules (level AA: 4.5:1 for normal text, 3:1 for large text, 3:1 for the outline of a control and for focus).
- Five reviews of this plan (2026-11-17). Their findings were checked against `main` at `ee264d7b5` and applied; where a review was
  wrong or a better fix exists, section 12 says why.
- Nothing was built, run or edited for this plan. Line numbers are at `8d34b27a0` (the code they point to is the same at `ee264d7b5`).
  Each brief greps them again.

**Terms.**
- "Self-drawn control": a control the program paints itself (GDI+ in WinForms, a XAML template in WPF). Its look comes from our code.
- "Stock control": a Windows control drawn by Windows (TextBox, RadioButton, DateTimePicker and others). Its look comes from Windows 11.
- "Token": one named value of the new look (a colour, a size, a font), defined once in code.
- "The scan": the text-fit scan of section 5, which finds text that no longer fits after the font change.
- "The harness": the window-check harness in `tests/WindowChecksTests` (`docs/port-notes/WINCHK-HEADLESS.md`).

## For the owner

**What changes on screen.** Every window gets a calm, modern look: flat buttons with thin borders and small rounded corners, the
program's blue on title bars, grid titles and main buttons, flat light-grey toolbars, Segoe UI text, and sharp Microsoft Fluent icons.

**What does not change.** Layouts: nothing moves and no window is replaced (text that no longer fits gets a little room in place).
Printouts and PDFs; office content (sheets, claim forms, eForms, dashboard widgets, colours set in Setup); the tooth chart's drawing.

**The packages, in order** (each waits for the ones above it):
1. UI-0: tools: picture galleries, the text-fit check and a "before" picture set. Nothing changes on screen.
2. UI-1: the colours, sizes and fonts defined in one place, and the design sheet for you to approve.
3. UI-7a and UI-2a: the icon set added, and every font routed through settings that still hold today's font. Nothing changes.
4. UI-3 to UI-8: the new look part by part: window frames, buttons and boxes, lists and tabs, grids, toolbars and menus, new icons.
5. UI-9: the old pictures replaced by the new icons, window by window.
6. UI-10: text-fit fixes, made before the font changes, so no text is ever cut off.
7. UI-2b: the switch to Segoe UI.
8. UI-11: the remaining background colours, a sweep of every window, and the release notes.

**How you approve.** Each visible package shows you a gallery: before and after pictures of real windows, each marked "Approve" or
"Change" by you; nothing goes in before you approve. The first sets the look. A few times you also check 125% and 150% scaling.

**Your questions** (section 11; the recommended answer first). **Answered on 2026-11-17: every question as recommended** (UQ-1 9 pt with the 0.96 label factor kept, UQ-2 17 px rows, UQ-3 blue title bar and the per-item Misc Colors rule, UQ-4 grey with role colours, UQ-5 printouts unchanged, UQ-6 agree, UQ-7 new-style pictures for new databases, UQ-8 appointment book font unchanged, UQ-9 items grow with their text):
- UQ-1 Body text size: Segoe UI 9 pt, Windows' own size (or 8.25 pt: fewer fixes, but smaller than the newer windows).
- UQ-2 Grid rows: follow the text, 17 px tall, about 20% fewer rows on screen (or compact 16 px rows).
- UQ-3 Title bar: the program's blue with white text (or light grey); an office's own title bar colours keep working item by item.
- UQ-4 Icon colours: dark grey with a few role colours: red delete, green complete, amber warning, blue main action.
- UQ-5 Printed grids: keep today's look (or follow the new look).
- UQ-6 Perio chart: its grid and legend keep Open Dental's look like the tooth chart; the text strip under the chart gets the new look.
- UQ-7 New databases: new-style pictures on the 8 default procedure buttons (or Open Dental's); an office's pictures never change.
- UQ-8 Appointment book text: keep today's font, so no row grows and printed schedules stay the same (or Segoe UI, taller rows).
- UQ-9 Toolbar buttons, menus and tabs: each grows with its text and the ones after it shift right, order kept (or keep widths, cut text).

---

## 1. Goal and the owner's decisions

**Goal.** Every window looks modern, calm and consistent: flat surfaces, thin borders, small rounded corners, one accent colour, one
font, and one icon family. Staff who switch from Open Dental find every control where it was. Only the look changes.

**The owner's decisions (2026-11-17).**
1. **Restyle only.** Every window keeps its layout and its control positions. Staff switch from Open Dental and know where things are.
   **Replaced on 2026-11-21 for the layouts** (the owner's G-UI3 review: "overall not good. UI lacking consistence. choice of spacing not
   symetrical"): "every window is laid out again on one spacing grid by a tool, same controls, same window sizes where they fit; a pilot
   first, then every window in slices", and "the checkbox box on the left of its text, everywhere". The owner's standard for it: "apply
   general styling sense. Looking for symmetry, consistency, right balance between too much crowding and too much spacing". The rules
   are in [UI-12.md](../port-notes/UI-12.md) section 1; package UI-12 in section 7. The controls, their order, names, texts and events
   still stay; what follows about positions holds until a window's UI-12 slice lays it out.
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
| `SelectionBarNoFocus` | `#616161` | the selection bar without focus (owner, 2026-11-17) | 6.2:1 on white, 4.9:1 on `AccentSubtle` |
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
bounds: `Accent` (6.8:1 on white) while the control has focus, `SelectionBarNoFocus` (`#616161`, 6.2:1 on white, 4.9:1 on the fill; the
owner's darker grey of 2026-11-17) when it does not. The fill stays `AccentSubtle`
either way. The bar sits in the row's first 2 px; the briefs check that no cell text or icon starts there.

**Disabled accent button.** A disabled default button draws like any disabled button (`#F5F5F5`, `Stroke` border, `TextDisabled`), as
in Windows 11. No extra token.

**High contrast.** When Windows runs a high-contrast theme (`SystemInformation.HighContrast`, `SystemParameters.HighContrast`), every
colour token returns its `SystemColors` partner (`Surface` and `SurfaceRaised` → `Window`, `Text` → `WindowText`, `Accent` and the
selection → `Highlight` and `HighlightText`, `Stroke` and `StrokeStrong` → `WindowFrame`, the selection bar without focus →
`HighlightText`, `TextDisabled` → `GrayText`, the title bar →
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
  - "Round" is LayoutManager's rounding (`(int)Math.Round`, a .5 goes to the even neighbour), so the helpers and LayoutManager give the
    same pixels: the focus ring is 2, 2, 3, 4, 4 px at 100, 125, 150, 175 and 200%.
  - Every rectangle that is filled or outlined is snapped to whole device pixels before drawing; a 1 px line is drawn on the pixel
    centre (offset 0.5) when antialiasing is on.
  - Corner radii are rounded to whole device pixels.
  - `UiTheme` has the helpers (UI-1), so every control does it the same way.
- **Corners.** Controls (buttons, inputs, combo boxes, tooltips, toolbar and module bar hover shapes): 4 px. Check box box: 2 px.
  Containers (group boxes): 6 px. Grids, tab pages, menus and the window frame: square (Windows 11 rounds the window itself: UI-3
  asks for DWM rounding with the program's own title bar, which the owner kept on 2026-11-18).
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
- **Tabs** (`UI.TabControl`, WPF `TabControl`, 11 stock `TabControl`s keep the Windows look). Headers keep today's size formula (3.3:
  text width plus 8 px); with the new font their widths follow UQ-9, and UI-4b-2 checks the tab strips (section 7). Unselected:
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
  - Content types, checked by type: `ToothChartWrapper`, `ContrPerio` (UQ-6), `ODPrintPreviewControl` and `UserControlDashboard`. A
    type needs no call at the right moment, so the tooth chart is covered in every window that hosts it (`ControlChart`,
    `FormGraphics`, `FormSheetFillEdit` and others).
  - The patient dashboard is office content, like a sheet: its widgets are sheet definitions (`SheetTypeEnum.PatientDashboardWidget`)
    that the office designs in `FormSheetDefEdit`. `FormOpenDental.cs:223-228` adds `UserControlDashboard` through `LayoutManager.Add`;
    `LayoutSkipChildren` (`LayoutManagerForms.cs:1105`) skips everything inside it, and each `UserControlDashboardWidget` adds its
    fields with `Controls.Add` at the sheet definition's positions (`UserControlDashboardWidget.cs:162-165, 300`), so the mapping never
    records them. Their fonts stay as they are: `DashLabel` takes the sheet field's font, with its fallback "Microsoft Sans Serif" 8
    (`DashLabel.cs:32-44`); `DashFamilyInsurance.cs:48, 70` and `DashIndividualInsurance.cs:59` use `GenericSansSerif` at 8.25; new
    widget and sheet definitions get the font name "Microsoft Sans Serif" (`FormDashboardWidgetSetup.cs:190`, `FormSheetDefs.cs:128`).
    These are content defaults, not screen chrome, and stay (section 9's allow list). The program's own controls inside the widgets
    draw as they do everywhere: the `GridOD` inside `DashApptGrid` takes the grid restyle and its grid font (UI-5), the `UI.GroupBox` of
    `DashIndividualDiscount` the group box restyle with the pinned font below (UI-4a), and the widgets' stock context menus the token
    renderer (UI-6). Positions and sizes stay those of the sheet definition.
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
  - The patient dashboard pins its font on `UserControlDashboard`, not on each widget: Microsoft Sans Serif 8.25 in its constructor,
    which runs before `LayoutManager.Add` (`FormOpenDental.cs:223, 228`). LayoutManager records and rebuilds the dashboard's own font
    (only its children are skipped), so the pinned family keeps today's zoom scaling, and every widget and field without a font of its
    own (for example the labels of `DashIndividualDiscount`, which sets none) inherits it as today. A fixed pin on the widget, which
    LayoutManager never records, would stay at 8.25 at every zoom step. Today the dashboard inherits the same font from the main
    window, so the pin changes nothing now.
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
  Microsoft Sans Serif as a control's font: on `main` 8 outside OpenPmsDental (Central Manager 4, xCrudGenerator 2, RdlDesign 2), plus
  those of OpenPmsDental's plain forms; the brief greps again. The graph's 6 Microsoft Sans Serif designer fonts are chart element
  fonts, not control fonts, and stay (below). RdlDesign's other designer fonts are the 55 Arial 8.25 Bold Italic headings of 3.4 and a
  few Wingdings and Courier New fonts, which stay by the rule of 3.4.
  - A designer file cannot point at the token: the Visual Studio designer writes the property back as a literal the next time it saves
    the form. So a Microsoft Sans Serif 8.25 Regular font on a control whose parent has the default font is removed, and the control
    inherits the default. Any other (Bold, other sizes) stays a literal in the designer; UI-2b changes it together with the exe
    literals, and a NoHq test checks each one's family and size against the token by the rule of 3.4 (UI-2a-4). This rule and that
    test apply only to the font of a control or a form (`Control.Font`, `Form.Font`).
  - **Chart element fonts are report content and stay, in every project.** The fonts of MS Chart elements (a chart area's axis
    `LabelStyle.Font`, a `Title.Font`, a `Legend` font, and properties that set them, such as `GraphQuantityOverTime.TitleFont`,
    `GraphQuantityOverTime.cs:172-174`) belong to the chart, and the dashboard prints the charts: `FormPrintImage`
    (`OpenPmsDentalGraph/Dashboard/FormPrintImage.cs:95-102`) rebuilds each chart and draws it with `DrawToBitmap` into the page it
    prints or saves as PDF. On `main` the six Microsoft Sans Serif ones are all in the graph: `GraphIncome.Designer.cs:86` and `116`
    (axis labels, 11.25), `GraphIncome.Designer.cs:165` (title, 11), `GraphQuantityOverTime.Designer.cs:111` and `134`, and
    `GraphQuantityOverTimeFilter.Designer.cs:108` (`graph.TitleFont`, 10). The charts' other element fonts (Trebuchet MS axis labels)
    stay by the rule of 3.4. No package edits them; section 9's allow list names them, and the print guard covers the dashboard
    printout (5.2).
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
- **The patient dashboard's widgets.** Office content (5.1): the fields keep their sheet fonts and today's defaults, and the dashboard
  pins its inherited font. The program's controls inside them (the appointment widget's grid, a group box) and the context menus are
  restyled like everywhere else; G-UI2 shows the dashboard with a widget open.
- **The graph dashboard's charts.** The charts' element fonts stay on screen and in the dashboard printout (`FormPrintImage`, printed
  and PDF), which the print guard covers (5.1, chart element fonts). The graph's windows and controls around the charts are restyled.
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
| Controls that newly overflow at 100% (UI-0b's scan, measured; of 10,247 with measured text) | 1,771 in 449 windows | 650 in 248 windows |
| ... visibly cut (all but the 1-2 px height cases whose ink stays inside) | 988 | 409 |
| ... only in height | 1,163 (1-2 px: 943, of them 160 cut) | 437 (1-2 px: 350, of them 109 cut) |
| ... need more width | 520 | 168 |
| ... need another line | 88 | 45 |
| ... fixable in free space / need the owner | 1,344 / 427 | 520 / 130 |
| Controls that size themselves: new overlaps or parent-edge passes | 856 in 149 windows (782 fixable) | 587 in 110 windows (547 fixable) |

These are UI-0b's measured counts (`docs/port-notes/UI-0b.md` section 6, `main` at `e7f87b1d8`): every window and UserControl built off
screen, the controls measured the way they draw, LayoutManager's 0.96 factor included, with the ink check for the 1-2 px cases. They
replace the static model's estimates (1,591 in 422 files at 9 pt, 391 in 182 files at 8.25 pt, fixed-size controls only, without the 0.96
factor). The biggest windows at 9 pt: FormPatientEdit 65, FormProvEdit 38, FormClaimEdit 31, ControlAccount 22, FormProcEdit 21, the
preference pages 11-19 each. By type at 9 pt: labels 791, `UI.CheckBox` 600, radio buttons 231, stock buttons 83, `UI.Button` 38. 660
controls already overflow in today's font and are not counted. The scan measures before the new button padding and check box glyph; it runs
again at the end of each control package (5.4).

**Controls that size themselves** (the last row of the table; the static model counted fixed-size controls only). Two groups grow by
themselves with the font, from the same position, and can grow into a neighbour (UI-0b measured, at 9 pt: text boxes 515, `ValidDate`
154, `ValidDouble` 75, `ValidNum` 47, labels 28, combo boxes 15, date pickers 9):
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

UI-0's scan counts both groups (5.4) for each UQ-1 option, so the owner's numbers include them (the table's last row).

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
4. UI-3 (frame), then UI-4a, UI-4b-1, UI-4b-2, UI-4b-3 (controls, one after another), UI-4c-1 and UI-4c-2 (side by side), UI-5
   (grids), UI-6 (toolbars, menus, module bar). UI-7b (the `EnumIcons`) and
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
- **Status (2026-11-17).** Done on `ui/ui0a-1` (`docs/port-notes/UI-0a.md`), not merged yet. The rule is
  `UiPictureMask.GetMaskedPixelSha256` in `tools/UiGallery/UiPictureMask.cs`: the builder compiles it with Add-Type, and UI-0c links the
  file into `tests/WindowChecksTests` for U0 (the port note gives the lines). Masks for About, Diagnostics and the Appointments module's
  date panel; UI-0c adds group U's rows. Self-tests in `tests/ReleaseFlowTests/UiGalleryScriptTests.cs`.

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
- **Status (2026-11-17).** Done on `ui/ui0b-1` (`docs/port-notes/UI-0b.md`), not merged yet: `tests/UiRefreshTests` (22 tests) with the
  shared files `Shared/TextFitMeasurer.cs`, `TextFitBoundsRecord.cs` and `TextFitFontSwap.cs` for UI-0c to link; 1,026 types, 1,001 built
  without a database or a window (3 need a database, 15 throw without their own constructor, 7 have no designer code; listed in the note);
  `Data/layout-snapshot.tsv` (17,751 designer controls) and an empty `Data/fit-fixes.tsv`. The full scan is opt-in (`OPMS_UI_SCAN=1`); its
  measured counts are in section 5.3 and the note's section 6. Found on the way: `LayoutManagerForms.ZoomTest` gives no consistent zoom
  (bounds add it, fonts multiply it), so the zoom step is measured as scale 1.2, which gives the same pixel sizes.

**UI-0c: harness group U**
- Owns: `tests/WindowChecksTests` group U (`InProc/InProcChecks.U.cs`, `UiGallery/windows.tsv`, the walker), the check U0, and the
  links to UI-0b's shared measuring files in `tests/WindowChecksTests`' project file.
- Needs: UI-0a (U0 uses its masking and hashing rule) and UI-0b (the walker uses its measurer and bounds-record writer, 5.4).
- Work: about 50 common windows the harness does not reach yet (claims, recall, confirmation, schedules, pay plans, billing, procedure
  codes, definitions, security, tasks, medical, Rx, labs and others listed by the tooling survey), opened by their own handlers on
  synthetic patients and closed with Cancel; the main window at fixed sizes 1366×768 and 1920×1080; each module with and without a
  patient, and the patient dashboard with a synthetic widget sheet definition (added to the test copy if the dump has none). The walker that measures text after `Load` and records bounds with UI-0b's shared files, not its own copy (5.4). The check U0
  (compare each picture with `approved.tsv`).
- Tests: group U runs on the synthetic dump; a window the dump cannot fill is listed with its reason.
- **Status (2026-11-17).** Done on `ui/ui0c-1` (`docs/port-notes/UI-0c.md`), not merged yet. `tests/WindowChecksTests/UiGallery/windows.tsv`:
  60 common windows (U1 to U60), 8 the dump cannot fill (U61 to U68, SKIPPED with the reason), the main window at both sizes with each
  module (UM1 to UM21) and the patient dashboard (UD1); the walker `Harness/UiWalker.cs` on UI-0b's linked shared files (the free-space
  rule moved into `TextFitMeasurer`); U0 `Harness/UiApprovalCheck.cs` on UI-0a's linked rule, run at the end of a run with group U. The
  self-tests pass without the opt-in. Group U itself was not run: the owner's session was disconnected; the lead runs it with the before
  set (`OPMS_WINDOW_CHECKS_GROUPS=U`, UI-0c.md section 5).

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
- **Status (2026-11-17).** Done on `ui/ui1-1` (`docs/port-notes/UI-1.md`), not merged yet: `CodeBase/UI/UiTheme*.cs` (30 colour tokens: the
  25 of 3.2 and five role names for white on the accent, text on a selection and the title bar, each with its high-contrast partner); the
  sheet and the real windows in `tests/UiRefreshTests/DesignSheet*.cs`; `Data/coverage.tsv` and the NoHq scans in
  `NoHqTests/UiThemeScanTests.cs` with the allow list `Data/colour-literals.tsv` (323 rows). The gallery is in
  `.git/od-tools/test-results/g-ui1-part1/`; the WPF Frm waits for a connected session (WPF draws nothing while it is disconnected). Measured:
  the Account grid's rows are 14 px today, 17 px at Segoe UI 9 and 16 px at 8.25, as UQ-2 says. For the owner: the selection bar without
  focus (`StrokeStrong`) was 3.5:1 on white but 2.7:1 against the selection fill beside it (the note's section 6); the owner chose the
  darker grey (UI-1b: `SelectionBarNoFocus` `#616161`) and approved G-UI1 part 1 on 2026-11-17. Fixed on `ui/ui1-2`:
  the snapping helpers round as LayoutManager rounds (a .5 to the even neighbour, so the focus ring is 2 px at 125%, not 3), checked against
  `LayoutManagerForms` itself; the sheet and the gallery are redrawn.

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
- **Status (2026-11-17).** Done on `ui/ui7a-1` (`docs/port-notes/UI-7a.md`), not merged yet: Fluent UI System Icons 1.1.346, the newest
  `@fluentui/svg-icons` (no GitHub Releases), downloaded into `.git/od-tools/downloads/ui7a-fluentui-svg-icons-20261117`; the 53 SVGs of the
  ten `UiGlyph` rows vendored with the MIT `LICENSE` and `SOURCE.txt` (the npm layout found: `icons/<name>_<size>_<style>.svg`, flat);
  `icons/manifest.json`, `icons/README.md`, `tools/New-IconData.ps1`, the generated `CodeBase/UI/Icons/IconData.g.cs` and the renderer
  `CodeBase/UI/Icons/UiIcons*.cs` (GDI+, `GetBitmap`, `GetImageList`, WPF) with `Direct2d.FillGlyph`, `FillIcon` and `DrawGlyph` for
  Direct2D and the OnlyGDI path; the notice in `THIRD-PARTY-NOTICES.txt` and so in the About box's license list. The dash is Fluent's Line
  Horizontal 1. `UiRefreshTests/IconRendererTests` draws every row at every size on GDI+, Direct2D and WPF (WPF's `RenderTargetBitmap`
  renders in the disconnected session, so nothing waits); no window check is needed.

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
  `SparksToothChart/ToothChart2D.cs`, `OpenPmsDental/User Controls/ContrPerio.cs` and
  `OpenPmsDental/User Controls/Dashboard/UserControlDashboard.cs`, the font test rows of section 9.
- Work: 5.1's tokens, mapping (the identity while `FontBody` is Microsoft Sans Serif), content rules and pins. Tests that pinned
  "Microsoft Sans Serif" now compare with the token, so the flip later changes no test.
- Tests: section 9's font group; the mapping returns every font unchanged with today's tokens (including the designer fonts at 8 and 8.5
  pt); with the token override set to Segoe UI, an explicit designer font on a preference page (`UserControlAccountRecAndRepCharges`)
  and on `ControlImages`, both added through `AddUnscaled`, follows the token; the underlined labels of `FormInsBenefits` still draw
  without underline, as today; the tooth chart baselines unchanged with the test default font set to Segoe UI (the pin proved before the
  flip); the content-exclusion test (a Microsoft Sans Serif sheet field keeps its font with the token override set to Segoe UI); a perio
  chart picture (`ContrPerio` on a synthetic exam) unchanged with the override set to Segoe UI; a synthetic dashboard widget with a
  label sheet field (one with a font name, one without) and a `DashIndividualDiscount` field keeps its fonts, at 100% and at one zoom
  step, with the override set to Segoe UI; `MarkContent` after
  `InitializeLayoutManager` throws; a signature test: open `FormProcEdit` with a signed group note on the synthetic dump, change the
  program zoom and resize the window, save, and check the signature is still valid; the scan's report for Segoe UI through the override
  attached to the brief's result.
- **Status (2026-11-17).** Done on `ui/ui2a1-1` (`docs/port-notes/UI-2a.md`, section UI-2a-1), not merged yet: `CodeBase/UI/UiTheme.Fonts.cs`
  (the six role tokens with today's values, `UiFontOption.UI2b` beside them, `MapFont`, `SetFontForTest`), the font lines of `FormODBase` and
  `FormFrame`, `LayoutManagerForms` (the mapping in its three recording places, the content types, `MarkContent`; the rebuild unchanged), the
  three marks, the pins (`ToothChart2D` together with `ToothChartWrapper`, so the program zoom still scales the 2D chart's text; `ContrPerio`;
  `UserControlDashboard`), section 9's font group and `tests/UiRefreshTests/FontTokenTests.cs`. The signature test is window check UF1 (opt-in,
  a shown window). The scan through the override (Segoe UI 9 at 100%: 1,775 new overflows in 450 windows) is in the port note. Still open,
  for a connected session: ToothChartImageTests and ToothChartHostTests, UF1, and the harness's U0 and bounds record against the before set,
  which does not exist yet.

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
- **Status (2026-11-17).** Done on `ui/ui2a2-1` (`docs/port-notes/UI-2a.md`, section UI-2a-2), not merged yet: the 47 sites changed (5 `None`, 12 graph controls `Inherit`, 30 forms `Dpi` 96F,96F), the NoHq ban and mode tests, the text-scale clip list for UI-10. The desktop check at 150% waits for the lead's connected session.

**UI-2a-3: print fonts**
- Owns: `GridOD.cs` (print and screen font split), `GridOld.cs` and `Tables Old Grids/ContrTable.cs` if they print, the print guard.
- Needs: UI-2a-1.
- Work: 5.2, GridOD's print path. Print fonts are split out and pinned to today's. GridOD's screen fonts point at `FontGrid` and
  `FontTitle` (5.1), which hold today's sans serif 8.5 and 10 Bold.
- Tests: the print guard (section 9: GridOD print and sheet output, sheets, claim forms, the perio chart print, the claim perio
  attachment, eForm PDF output, reports, RDL and MigraDoc output, the graph dashboard's chart page, identical before and after).
- **Status (2026-11-17).** Done on `ui/ui2a3-1` (`docs/port-notes/UI-2a.md`, section UI-2a-3), not merged yet: GridOD's screen fonts take
  `FontGrid` and `FontTitle`; its printout, sheet methods and the row heights of printouts, sheets and the sheet canvas (`IsForDocument`)
  use print fonts pinned to today's and today's print colours (`GridPrintLook`, UQ-5); GridOld (it prints the progress notes) has the same
  split; ContrTable does not print and is unchanged. The print guard is `tests/PrintGuardTests`: every item of section 9 rendered with
  today's tokens and with UI-2b's and compared byte for byte; the eForm PDF needs WPF rendering and waits for a connected session
  (`OPMS_PRINT_GUARD_WPF=1`).

**UI-2a-4 to UI-2a-n: the code font sites and the plain forms' designer fonts**
- Slices: by folder, in the folder order of UI-9a, at most about 40 files each; the brief sets the cut from a fresh grep. The last
  slice is the other projects of UI-9a-7: the plain exes' Microsoft Sans Serif control fonts (CentralManager 4, xCrudGenerator 2,
  RdlDesign 2) and their code font sites. The graph's 6 Microsoft Sans Serif designer fonts are chart element fonts and are not edited
  (5.1).
- Owns per slice: its files among the about 59 code lines naming Microsoft Sans Serif and the about 40 `GenericSansSerif` files, and
  the designer fonts in its folders of the plain forms and of the FormODBase windows without `InitializeLayoutManager` (5.1). The
  brief's grep list has both: the plain forms, and the FormODBase subclasses whose constructor does not call `InitializeLayoutManager`
  (on `main` the five allocator windows of 5.1, in the slice that owns `ReportingForRdl`, which is in UI-9a-6's folders).
- Needs: UI-2a-1, and UI-2a-3 (its print guard).
- Work: each site sorted into screen chrome (to the token of its role, 5.1), print, or document and content (stays, 5.2;
  `Graphics.FromImage` pictures that are saved, attached or exported are documents; sheet, eForm and dashboard-widget font defaults,
  the dashboard fields' `GenericSansSerif` fonts and MS Chart element fonts are content, 5.1). Plain forms and the windows without
  `InitializeLayoutManager`: a default control or form font is removed so the control inherits, any other stays a literal for UI-2b
  (5.1).
- Tests: the "Microsoft Sans Serif" scan with its scope and allow list (section 9: code outside `InitializeComponent` everywhere, and
  the designer code of the plain forms and of the windows without `InitializeLayoutManager`; the designer code of windows and
  UserControls under LayoutManager is exempt, because the mapping covers it); the literal test for the control and form fonts of the
  plain forms and those windows (chart element fonts are not in it, 5.1); the print guard; the scan's report.
- **Status (2026-11-18).** Slice 1 (`OpenPmsDental/Forms`, `OpenPmsDental/Forms Reports`) done on `ui/ui2a4-1`
  (`docs/port-notes/UI-2a.md`, section UI-2a-4), not merged yet: the display field editors measure with `FontGridBold`; the mount layout
  editor's item text names `GenericSansSerif` as the mount draws it; the other 51 Microsoft Sans Serif and `GenericSansSerif` lines
  (the mount editor's two among them) are print, document or content and are the rows of
  `tests/UiRefreshTests/Data/font-literal-allow.tsv`; the slice has no plain form or window without `InitializeLayoutManager` with a
  designer font. The scan and the literal test are `tests/UiRefreshTests/FontLiteralScanTests.cs`; later slices add their folders to
  `FontLiteralScan.ArrayFolders`. Still open, for a connected session: U0 and the bounds record against the before set.
- **Status UI-2a-5 (2026-11-18).** Slice 2 (the other OpenPmsDental folders of UI-9a-6, `OpenPmsDentalBusiness`, `OpenPmsDentalGraph`,
  `SparksToothChart`) done on `ui/ui2a5-1` (`docs/port-notes/UI-2a.md`, section UI-2a-5), not merged yet: 23 screen sites of the scan
  and `ModuleBar`'s Arial captions (5.1 names the role) take their role's token (`FontBody`, `FontButtonPanel`, `FontModuleCaption`,
  `FontTitle`, `FontGrid`, `FontGridBold`, or `FontFamilyName` at the site's own size), 72 allow-list rows for the sites that stay, the generated per-site report
  `tests/UiRefreshTests/Data/font-literal-sites.tsv`; the literal test compares a literal with its parent's font; the five allocator
  windows have no default font to remove (their nine literals stay for UI-2b); three more inherited-font sites are on the UI-2b checklist
  (`SignatureBox`'s signature picture, the dashboard legend's print copy, and `SheetEditMobileCtrl` to decide).
  Still open, for a connected session: U0 and the bounds record against the before set.
- **Status UI-2a-6 (2026-11-18).** Last slice (the other projects of UI-9a-7: CentralManager, RdlDesign, ServiceManager, MySQLManager,
  xCrudGenerator, OpenPmsDentalDbSettings, OpenPmsDentalHL7, DatabaseIntegrityCheck, DocumentationBuilder, UpdateFileCopier,
  OpenPmsDentalHelper32, CodeBase, ODR, WpfControlsOD) done on `ui/ui2a6-1` (`docs/port-notes/UI-2a.md`, section UI-2a-6), not merged yet:
  the 14 projects are in the font scan, 15 allow-list rows (11 exe literals, 4 token definitions, a new sort `token`), the report regenerated;
  `xCrudGenerator`'s default `label10` font removed, the other control fonts stay literals for UI-2b; no new inherited-font site for the checklist.
  Open: the default family of the WPF drawing `Font` (`Graphics.cs:564`), and U0 against the before set in a connected session.

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
- **Status (2026-11-19).** Owner: frame OK 2026-11-19; 125/150% desktop look still to check. Follow-ups from the hover-fix review (port
  note 3.7): each caption button's hit area covers its painted fill (maximized all of it, so the screen's top-right pixel closes the window;
  normal below the top resize edge, which stays), the glyphs centred in their fills, Windows' border colour re-applied when the Misc Colors
  change or a floating image window is selected or not; G-UI2's frame part made again laid out as on screen, with 200%:
  `.git/od-tools/test-results/g-ui2-frame-ui3-2/`.
- **Status (2026-11-18).** Built on `ui/ui3-1` (`docs/port-notes/UI-3.md`), not merged yet: `WpfControlsOD/Frame/FramePainter.cs` (the one
  project both frames reach that also knows `DefCatMiscColors`) paints both frames; Misc Colors item by item; the floating image windows'
  two frames; window background `Surface`; "(program default)" in the Definitions window; Windows 11's rounded corners with the border in
  the outline colour by default (the owner, 2026-11-18: "corners yes"; `OPENPMS_UI3_DWM_CORNERS=off` turns them off, `small` for small
  ones). The owner's finding on the real desktop, the close button's red starting 3 px below the title bar's top, is fixed: a hover change
  repainted only the buttons' hit rectangle (5 px down), and above 100% the close fill stopped short of the window's right edge; the fills
  now cover the title bar from its top row on screen, the close fill to the window's right edge (port note 3.6). Tests in
  `tests/UiRefreshTests/FrameTests.cs` pass, among them one that lays the frames out and repaints them as the screen does; window checks UX1
  to UX3 (part `UX` of group U) wait for the lead's connected session with A6 and the `NoD2D.txt` run. G-UI2's off-screen frame part:
  `.git/od-tools/test-results/g-ui2-frame-ui3/`.

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
- **Status (2026-11-17).** Done on `ui/ui4a-1` (`docs/port-notes/UI-4a.md`), not merged yet: `CodeBase/UI/UiControlPaint.cs` (the 3.6
  shapes from the tokens, the same as the design sheet's); WinForms `Button` (the form's `AcceptButton` as the default button), `CheckBox`
  (the same 13 px box, the Fluent mark and dash), `GroupBox`, `PanelOD` (square: its border is Windows'), `ToolTipOD`; WPF `Button` (with
  `IsPrimary` on 13 Frms), `CheckBox`, `RadioButton`, `GroupBox`, `Label`, `ToolTip`, the two shared focus visual styles in `Generic.xaml`;
  `ContentLook.IsPinned` keeps `CtrlEFormFill` in today's look (the synthetic eForm picture is unchanged). Tests in `tests/UiRefreshTests`
  (`ControlStateTests`, `EFormLookTests`, `ControlStateGallery`); `Data/accept-buttons.tsv` lists 86 `UI.Button` AcceptButtons, 12 of them
  not OK or Save for the owner. Gallery G-UI1 part 2 in `.git/od-tools/test-results/ui4a-G-UI1-part2/`. The WPF controls are not focusable
  today, so their focus ring waits until one is made focusable (a behaviour change, not in UI-4a).

**UI-4b: core controls, part 2** (the UI-4b row of 2.1; on `main` 58 code and XAML files, `.resx` aside, in five control families,
so it is cut into three slices). Each slice owns its controls' WinForms and WPF files, their styles in
`WpfControlsOD/Themes/Generic.xaml` and the pinned copies for `CtrlEFormFill` of the templates it changes. The slices share
`Generic.xaml` and those copies, so they run one after another.
- Needs: UI-4a (it shares `Generic.xaml` and the focus style), UI-7a (chevrons).
- Work, tests and gallery in each slice as UI-4a, for its controls.
- **UI-4b-1: combo boxes, pickers and lists** (21 files). WinForms `ComboBox`, `ListBox`, `ClinicSelector/ComboBoxClinicPicker`,
  `ClinicSelector/FormComboPicker`; WPF `ComboBox`, `ComboClinic`, `ListBox`, `ListView`, `TreeView`,
  `Supplemental/WindowComboPicker`, `Supplemental/PopupHelper2`.
  - **Status (2026-11-17).** Done on `ui/ui4b1-1` (`docs/port-notes/UI-4b.md`, section UI-4b-1), not merged yet: `CodeBase/UI/UiListPaint.cs`
    (the combo box's box, Fluent chevron and focus line, the list border, item fill, selection bar and focused row from the tokens, on
    UI-4a's `UiControlPaint`); WinForms `ComboBox`, `ComboBoxClinicPicker`, `FormComboPicker`, `ListBox`; WPF `ComboBox` (its focus line a
    focus visual style in `Generic.xaml`), `ComboClinic`, `ListBox`, `WindowComboPicker`, `ListView`, `TreeView` (a `TreeViewItem` style in
    `Generic.xaml`), `UiLookLists.cs`. Text positions and bounds unchanged; a window's own `ColorBack` and `ColorSelectedBack` still win.
    `PopupHelper2` draws nothing (`no-drawing`). The eForm fill surface has none of these controls (its picture is unchanged). Tests in
    `tests/UiRefreshTests` (`ListControlStateTests`, `ListControlGallery`); gallery G-UI1 part 2b in
    `.git/od-tools/test-results/ui4b1-G-UI1-part2b/`. One follow-up for the owner: `FrmPatientAddAll` resets a list's selection to today's
    colour.
- **UI-4b-2: tabs and splitters** (12 files). WinForms `TabControl/*` and `Splitters/*`; WPF `TabControl`, `TabPage`,
  `SplitContainer`, `WpfControlsOD/Themes/TabControlDict.xaml`. Its extra test: the tab headers keep today's width formula
  (`TabControl.cs:546`, text width plus 8 px, 3.3), and for the windows with the most self-drawn tabs on `main` (`FormClaimEdit` 10,
  `FormMedical` 9, `FormFeeSchedTools` 6, `FormPatientEdit` 5) the tab strip at the window's designer size and at its smallest size
  keeps the tabs in order with none past the strip's right edge, in the font under test (5.4); an overflow is listed for the owner
  (UQ-9). G-UI2 and G-UI3 show those strips before and after.
  - **Status (2026-11-18).** Done on `ui/ui4b2-1` (`docs/port-notes/UI-4b.md`, section UI-4b-2), not merged yet: `CodeBase/UI/UiTabPaint.cs`
    (the strip, the page border, a tab header and a splitter from the tokens, on UI-4a's `UiControlPaint`); WinForms `TabControl` (strip
    `SurfaceSubtle`, page border `Stroke`, tabs `TextSecondary`, hover `SubtleHover`, the selected tab `Text` with the 2 px `Accent` line, a
    page's `ColorTab` still wins; tab rectangles unchanged), `SplitContainer` and `SplitterPanel` (a 1 px `Gridline` line, `Accent` while
    dragging, panel borders `Stroke`; a window's own `ColorSplitter` and `ColorBorder` still win); WPF `TabControl` and `TabPage`
    (`TabControlDict.xaml`, today's header layout, the shared focus ring) and the WPF `SplitContainer`'s `GridSplitter` (its block in
    `Generic.xaml`). Tests in `tests/UiRefreshTests` (`TabStripFitTests`, `TabSplitStateTests`, `TabSplitGallery`): no tab of the four
    windows runs past its strip's edge in today's font or in Segoe UI 9, at the designer size or cut to a 1024 x 768 desktop
    (`Data/tab-overflow.tsv` lists none). Galleries: G-UI1 part 2c in `.git/od-tools/test-results/ui4b2-G-UI1-part2c/`, the four windows'
    strips before and after (G-UI2, G-UI3) in `.git/od-tools/test-results/ui4b2-G-UI3-tabstrips/`.
- **UI-4b-3: calendars, date pickers and text boxes** (26 files). WinForms `MonthCalendarOD`, `ODDatePicker`, `ODDateRangePicker`;
  WPF `DatePicker`, `MonthCalendar`, `Supplemental/WindowMonthCalendar`, `TextBox`, `TextPassword`, `TextRich`, `TextVDate`,
  `TextVDouble`, `TextVInt`, `TextVTime`.
  - **Status (2026-11-18).** Done on `ui/ui4b3-1` (`docs/port-notes/UI-4b.md`, section UI-4b-3), not merged yet: `CodeBase/UI/UiCalendarPaint.cs`
    (a day's hover `SubtleHover`, the selected day `Accent`, today's 1 px `Accent` ring, the focus ring on the accent, the Fluent chevrons and the
    calendar glyph, on UI-4a's `UiControlPaint`); WinForms `MonthCalendarOD` (every rectangle and text where it was) and `ODDatePicker` (its
    drop-down button flat with the Fluent calendar glyph; the date box is a Windows text box and keeps its look); `ODDateRangePicker` through its
    two pickers; WPF text boxes through the templates of `Generic.xaml`'s UI-4b-3 block (`UiLookText`: the Windows layout, so no text moves;
    `StrokeStrong`, hover `StrokeStrongHover`, read only and disabled `Stroke`, a validation error `Danger`, the 2 px `Accent` focus line inside
    the bottom edge; square, as the approved sheet draws the input), `DatePicker` as a combo box with the calendar glyph, `MonthCalendar` as the
    WinForms one. The eForm fill surface keeps the Windows template (`ContentLook.IsPinned`; `Data/text-pinned-before.png`). Tests in
    `tests/UiRefreshTests` (`TextCalendarStateTests`, `TextPinnedLookTests`, `TextCalendarGallery`); gallery G-UI1 part 2d in
    `.git/od-tools/test-results/ui4b3-G-UI1-part2d/`.

**UI-4c: the remaining shared controls** (the UI-4c rows of 2.1; on `main` 50 files, `.resx` aside, of about 22 unrelated controls,
so it is cut into two slices that share no files and may run side by side).
- Needs: UI-4b (all three slices: it reuses their focus style and chevrons in `Generic.xaml`).
- Work, tests and gallery in each slice as UI-4a, for its controls.
- **UI-4c-1: imaging and appointment chrome** (25 files). WinForms `Appt/ControlApptProvSlider`, `Appt/PinBoard`, `Appt/ToggleDayWeek`
  (their chrome; the appointment book stays by UQ-8), `ImagesModule/UnmountedBar`, `ImagesModule/WindowingSlider`,
  `ImagesModule/ZoomSlider`; WPF `ImagingModule/*` (`ImageDockHeader`, `ImageSelector`, `ImageSelectorTypes`, `UnmountedBar`,
  `WindowImageFloatWindows`, `WindowingSlider`, `ZoomSlider`).
  - **Status (2026-11-18).** Done on `ui/ui4c1-1` (`docs/port-notes/UI-4c.md`, section UI-4c-1), not merged yet: `CodeBase/UI/UiSliderPaint.cs`
    (the windowing handles as small buttons with an input's outline, the zoom cells as toolbar buttons, the value box as an input, the Fluent minus
    and plus, on UI-4a's `UiControlPaint`); WinForms `ToggleDayWeek` (rows as toggles), `PinBoard`, `ControlApptProvSlider` (the slider a button),
    `UnmountedBar` (a flat `Accent` header), `WindowingSlider`, `ZoomSlider`; WPF `ImageDockHeader` (the sheet's floating windows, Fluent caption
    glyphs), `ImageSelector` (a list with the selection bar and Fluent chevrons), `UnmountedBar`, `WindowingSlider`, `ZoomSlider`,
    `WindowImageFloatWindows` (`UiLookImaging`). Kept: the appointments (UQ-8), the providers' colours, the windowing ramp, the mount's colour and
    the unmounted images' marks. Tests in `tests/UiRefreshTests` (`ImagingApptStateTests`, `ImagingApptGallery`); gallery G-UI1 part 2e in
    `.git/od-tools/test-results/ui4c1-G-UI1-part2e/`.
- **UI-4c-2: dialogs, progress and the rest** (25 files). WinForms `ODColorPicker`, `ODCodeRangeFilter`, `ODPictureBox`,
  `OdProgressBar`, `BufferedTreeView`, `MsgBoxCopyPaste`, `DatabaseIntegrity/WarningIntegrity`, `FormPrintPreview` and
  `ODPrintPreviewControl` (chrome only); WPF `SignatureBoxWrapper`, `DatabaseIntegrity/WarningIntegrity`, `Dialogs/FrmColorDialog`,
  `Progress/*`, `ToothChart/ToothChartTextBelowStrip` (UQ-6). `TextBelowStripTests` follows UQ-6 (section 9).
  - **Status (2026-11-18).** Done on `ui/ui4c2-1` (`docs/port-notes/UI-4c.md`, section UI-4c-2), not merged yet: `CodeBase/UI/UiDialogPaint.cs`
    (the progress bar as the sheet's, the colour swatch with the input outline and focus ring, the warning's `Warning` and `Danger`, on UI-4a's
    `UiControlPaint`); WinForms `OdProgressBar`, `ODColorPicker`'s swatch, `ODPictureBox`'s `Stroke` border and hint, `ODCodeRangeFilter`'s hint,
    `WarningIntegrity`, `ODPrintPreviewControl`'s area around the pages (`SubtlePressed`; the pages unchanged); WPF `Progress/ProgressBarFlat`
    in `FrmProgressAuto` (an `Accent` marquee segment when indeterminate; the error line `Danger`), `WarningIntegrity`, `FrmColorDialog`'s
    frames, `SignatureBoxWrapper`'s frame `StrokeStrong` (the signature, its picture and validity unchanged; the eForm keeps `ControlDark` through
    `ContentLook.IsPinned`), the TextBelow strip `Text` on `SurfaceRaised` (UQ-6, `TextBelowStripTests` changed). `BufferedTreeView`,
    `MsgBoxCopyPaste`, `FormPrintPreview` and `ProgressWin` need no change (Windows controls, or drawn by other packages). Tests in
    `tests/UiRefreshTests` (`DialogStateTests`, `DialogGallery`); gallery G-UI1 part 2f in `.git/od-tools/test-results/ui4c2-G-UI1-part2f/`.

**UI-5: grids**
- Owns: `OpenPmsDentalBusiness/UI/GridOD.cs` (screen drawing), `GridOld.cs` if still used, `WpfControlsOD/UI/Controls/Grid.xaml(.cs)`,
  `Supplemental/Grid*`, `WpfControlsOD/Themes/DataGridDict.xaml`.
- Work: 3.6 grids, the selection bar and the focused row. Header and title heights follow the font, inside the control. Row height by
  UQ-2. Print colours stay.
- Tests: `BrandingTests.BrandColor_IsTheProgramsGridTitleBlue` moves to the token; the print guard; a grid picture test of title,
  header, hover, selection with and without focus, a coloured row; the scan rerun.
- Gallery: G-UI1 part 2 and G-UI2.
- **Status (2026-11-18).** Done on `ui/ui5-1`, review fixes on `ui/ui5-2` (`docs/port-notes/UI-5.md`), not merged yet, waiting for G-UI1 part 2 (grids) and G-UI2:
  `OpenPmsDentalBusiness/UI/GridScreenLook.cs` (the WinForms grid look from the tokens) drawn by `GridOD` and `GridOld` (still used: the
  Chart module's progress notes); `WpfControlsOD/UI/Controls/Supplemental/GridLook.cs` for the WPF `Grid` and `Themes/DataGridDict.xaml`.
  Title, header and editable row heights follow the font tokens (18, 15 and 19 px today as before; 20, 18 and 22 with UI-2b's Segoe UI;
  rows 14 and 17); the selection bar and the focused row's outline; a window's own row and selection colours stay. Printouts and sheets keep
  today's look (the print guard's grid, sheet, perio, MigraDoc, report and dashboard outputs have the same SHA-256 before and after). Tests: `tests/UiRefreshTests/GridLookTests.cs`, the branding test on the token,
  the colour scan reads `GridScreenLook.cs`. The India Arial switch stays on screen and in print (today's fonts stay): Segoe UI draws the
  rupee sign (5.1's check), so dropping the screen switch falls to UI-2b once the owner says yes (owner question in the port note).

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
- **Status (2026-11-17).** Done on `ui/ui6-2` (`ui/ui6-1` with main and UI-8 merged in; `docs/port-notes/UI-6.md`), not merged yet,
  waiting for G-UI2's toolbar part (the Chart button draws UI-8's tooth, ToothFilled in `Accent` when open): the token
  renderer `UiToolStripRenderer` (`CodeBase/UI/UiTheme.Renderer.cs`) set by `UiTheme.InitWinForms()` in nine exes' `Main` (not the two
  exceptions), on the progress windows' and the Microsoft 365 message thread, and linked into RdlDesign; `ToolBarOD`, `MenuOD`, `ModuleBar`
  (the selected module's Filled icon in `Accent`), `ODButtonPanel`, `LightSignalGrid`, the WPF toolbar, menu bar, `MenuItem` and `ContextMenu`
  (Generic.xaml) from the tokens; icons drawn over the hover shapes without a box; widths unchanged. Tests: ToolBarFitTests (every module
  toolbar and the menu at 1024, 1366 and 1920 px, today's font and Segoe UI 9, `Data/toolbar-overflow.tsv`), RendererTests,
  WpfMenuToolBarTests, the state pictures, C20 and the new window check UT1 (for the lead's session). Owner question: the Chart toolbar
  is cut off at 1024 and 1366 px today (UQ-9).

**UI-7b: the IconLibrary icons**
- Owns: both `IconLibrary` folders, the `EnumIcons` rows of `icons/manifest.json` and `icons/README.md`.
- Needs: UI-7a.
- Work: map the 38 + 53 `EnumIcons` (module bar included; Chart32G, Chart32W and ChartMed32 keep today's pictures until UI-8); the
  38 + 37 `Gen_*.cs` files go.
- Tests: every `EnumIcons` has a manifest row and draws non-empty at every size on GDI+, Direct2D and WPF, and on the OnlyGDI path and
  in NoDpi mode; `InProcChecks.DE` `WpfImages` counts vector icons (section 9).
- Gallery: **G-UI1 part 3** (today's icon beside the Fluent icon, 16/20/24 px, 100/125/150/200%).
- **Status (2026-11-17).** Done on `ui/ui7b-1` (`docs/port-notes/UI-7b.md`), not merged yet, waiting for G-UI1 part 3
  (`.git/od-tools/test-results/G-UI1-part3-ui7b`, rendered off screen; the before set from the `Gen_*.cs` pictures first). 57 `EnumIcons`
  manifest rows: the 50 members of both enums that get a Fluent icon and the Filled style of the seven module icons (`<member>Filled`); 248
  more 1.1.346 SVGs from UI-7a's download. Both `IconLibrary` copies draw from the table with their callers unchanged (Direct2D path calls,
  GDI+ on the OnlyGDI path, WPF `Path`s); 35 + 37 `Gen_*.cs` files and the WPF `IconSelector` go; Chart32G, Chart32W and ChartMed32 keep
  their pictures (three `Gen_*.cs` files) until UI-8. Account, Tx Plan and medical Tx Plan take Receipt, Clipboard Text LTR and Heart Pulse
  (the examples of 4.3 have no 32 px design; owner question). `InProcChecks.DE` `WpfImages` counts vector icons.

**UI-8: custom dental icons**
- Owns: `icons/custom/*.svg`, their manifest rows, `db/init/new-database.sql` (default procedure-button pictures only, if UQ-7 says yes);
  after UI-7b has merged, the chart icons' lines of both `IconLibrary` copies (Chart32G, Chart32W, ChartMed32 to the dental icons) and the
  three chart `Gen_*.cs` files with `IconSelector.cs`.
- Needs: UI-7a (UI-7b merged, for the chart icons).
- Work: section 4.4.
- Tests: as UI-7b; a test that the seeded procedure-button pictures are 20×20; a test that no office row is changed by any code path
  (the pictures are only in the new-database script).
- Gallery: **G-UI1 part 4** (each dental icon at each size, beside today's picture).
- **Status (2026-11-17).** Done on `ui/ui8-2` (`docs/port-notes/UI-8.md`), not merged yet, waiting for G-UI1 part 4
  (`.git/od-tools/test-results/G-UI1-part4-ui8`, rendered off screen). 20 dental icons of 4.4, Regular and Filled, at 16, 20, 24, 32 and
  48 px, drawn in `tools/DentalIcons/DentalIconArt.cs` (`tools/New-DentalIcons.ps1` writes `icons/custom/*.svg`); Fluent's Dentist has no
  32 px design, so the tooth is drawn here too. Their own enum `DentalIcon` (40 manifest rows; `EnumIcons` rows stay away from the three
  chart icons). Fix round `ui/ui8-2` (main merged in, with UI-7b): both `IconLibrary` copies draw `Chart32G`, `Chart32W` and `ChartMed32`
  as `Tooth`, `ToothFilled` and `ChartMedical` (`IconLibrary.GetDentalIcon`), so the module bar shows the new tooth; the three chart
  `Gen_*.cs` files and `IconSelector.cs` are gone, and G-UI1 part 4 shows the module bar's chart icons before and after. UQ-7: DevDbTool gives the 8 default procedure buttons of a new database the new pictures (20x20,
  the table's format) and `db/init/new-database.sql` holds them; `ProcButtons.SetToDefault` keeps Open Dental's (owner question: should the
  reset give the new ones?).

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
- **Status UI-9a-1 (2026-11-18).** Done on `ui/ui9a1-1` (`docs/port-notes/UI-9a.md`, sections 1 to 3), not merged yet, waiting for G-UI3
  (`.git/od-tools/test-results/G-UI3-ui9a1`, rendered off screen). The tool for every slice, `tools/Convert-PictureUses.ps1` (inventory,
  `-Check`, `-Apply`; `icons/picture-slices.tsv`, `icons/picture-map.tsv`, `icons/picture-converted.tsv`), and `IconLibrary.GetBitmap`.
  Forms A-C: 62 button pictures in 30 windows, 48 as `UI.Button.Icon` and 14 as `GetBitmap` at the picture's size (the Icon slot did not fit
  or the picture is not 22 px); 8 window `.resx` pictures and the `Resources` picture `butPin` (its last use) removed; the claim form
  background kept (4.5); 7 ImageLists left to UI-9b. New `EnumIcons` Up, Down, Edit, Label, Note, Pin, Preview, Send (both enums) and
  Print, Export (WinForms), 8 manifest rows. `PictureMapTests` (map complete, record in the designers, every read picture present, the
  built windows); `RenameGuardTests` unchanged (no pack URI in these WinForms windows). Owner questions in the port note, section 3.6.
- **Status UI-9a-2 (2026-11-18).** Done on `ui/ui9a2-1` (`docs/port-notes/UI-9a.md`, section 4), not merged yet, waiting for G-UI3 (`.git/od-tools/test-results/G-UI3-ui9a2`, rendered off screen). Forms D-L: 61 button pictures in 36 windows, 47 as `UI.Button.Icon` and 14 as `GetBitmap`; 1 window `.resx` picture and the `Resources` picture `butGoto` removed; 6 ImageLists left to UI-9b. No new `EnumIcons` member (deleteX18 to `DeleteX`, butGoto to `ArrowRight`, FormFeeSchedGroups butAdd to `Add`).
- **Status UI-9a-3 (2026-11-18).** Done on `ui/ui9a3-1` (`docs/port-notes/UI-9a.md`, section 5), not merged yet, waiting for G-UI3 (`.git/od-tools/test-results/G-UI3-ui9a3`, rendered off screen). Forms M-Q: 68 button pictures in 30 windows, 47 as `UI.Button.Icon` and 21 as `GetBitmap`; 1 window `.resx` picture and 6 `Resources` pictures removed; the Bola logo kept (4.5); 3 ImageLists left to UI-9b. New `EnumIcons` ChevronDown, ChevronUp, CloseDoor, Microphone, ZoomIn, ZoomOut (both enums, 6 manifest rows). Owner questions in the port note, section 5.6.
- **Status UI-9a-4 (2026-11-18).** Done on `ui/ui9a4-1` (`docs/port-notes/UI-9a.md`, section 6), not merged yet, waiting for G-UI3 (`.git/od-tools/test-results/G-UI3-ui9a4`, rendered off screen). Forms R-S: 66 button pictures in 36 windows, 48 as `UI.Button.Icon` and 18 as `GetBitmap`; 3 `Resources` pictures removed (butPin22, printPreview20, record); the splash picture and the Practice By Numbers logo kept (4.5); 4 ImageLists left to UI-9b. New `EnumIcons` Record (both enums, 1 manifest row). Owner questions in the port note, section 6.5.
- **Status UI-9a-5 (2026-11-18).** Done on `ui/ui9a5-1` (starts from `ui/ui9a4-1`; `docs/port-notes/UI-9a.md`, section 7), not merged yet, waiting for G-UI3 (`.git/od-tools/test-results/G-UI3-ui9a5`, rendered off screen). Forms T-Z and `Forms Reports`: 77 button pictures in 35 windows (23 and 54) as `UI.Button.Icon` or `IconLibrary.GetBitmap`; 2 new `EnumIcons` (ExportExcel, QueryView) and Copy and Paste for WinForms; 6 ImageLists left to UI-9b; 7 `Resources` pictures removed; the label sheet picture boxes of `FormRpLaserLabels` by hand. Bounds unchanged.
- **Status UI-9a-6 (2026-11-19).** Done on `ui/ui9a6-1` (`docs/port-notes/UI-9a.md`, section 8), not merged yet, waiting for G-UI3 (`.git/od-tools/test-results/G-UI3-ui9a6`, rendered off screen). Main Modules, User Controls, UI, Bridges, Logic, Reporting, OpenPmsDentalBusiness, OpenPmsDentalGraph: 45 button and box pictures in 18 windows and controls plus 11 WPF buttons in 5 windows; 14 new `EnumIcons`; ImageLists left to UI-9b. Bounds unchanged.
- **Status UI-9a-7 (2026-11-19).** Done on `ui/ui9a7-1` (`docs/port-notes/UI-9a.md`, section 9), not merged yet, waiting for G-UI3 (`.git/od-tools/test-results/G-UI3-ui9a7`, rendered off screen). The other projects: CentralManager 11 button pictures (`UI.Button.Icon`; its ImageList to UI-9b), RdlDesign 21 toolbar pictures (CodeBase's renderer by name through its linked files, today's pixel size), `MsgBoxCopyPaste` print button and `ComboBoxMulti` drop-down arrow (CodeBase `UiIcons.GetBitmap`); the other projects have no pictures. 18 new `EnumIcons` (the report designer's toolbar). The tool picks `UiIcons.GetBitmap` for a project that cannot reach `IconLibrary`. Bounds unchanged.

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
- **Status UI-9b-1 (2026-11-20).** Done on `ui/ui9b1-1` (`docs/port-notes/UI-9b.md`), not merged yet, waiting for G-UI3
  (`.git/od-tools/test-results/G-UI3-ui9b1`, rendered off screen). The shared part: the one icon factory `IconLibrary.GetImageList` (through
  CodeBase's `UiIcons.GetImageList`, the only place an ImageList is made in code) and `ToolBarOD.SetImageListIcons` (a list of icons at the
  toolbar's scaled 22 px slot, rebuilt at a scale change with the program link and plug-in pictures kept by key and index; a list set through
  `ImageList` draws as before). The main toolbar and the module toolbars (Appts, Family, Account, Tx Plan, Chart) lose their designer
  ImageLists and streams; each window's `ArrayIconsImageList` keeps today's `ImageIndex` order as icons, and every index button takes its
  `EnumIcons`. 8 new `EnumIcons` (Payment, Adjustment, ApptLists, Call, Unscheduled, Rx, Anesthesia; PerioChart draws UI-8's perio chart
  icon). The procedure button list is kept (4.5), the reminders' task list goes to UI-9b-3. Bounds unchanged.
- **Status UI-9b-2 (2026-11-20).** Done on `ui/ui9b2-1` (`docs/port-notes/UI-9b.md`, section 3), not merged yet, waiting for G-UI3
  (`.git/od-tools/test-results/G-UI3-ui9b2`, rendered off screen). The imaging module: `ControlImages`' two ImageLists (the tools list of 21
  pictures for `toolBarPaint` and `toolBarDraw` through `SetImageListIcons`, the tree list of 8 through `IconLibrary.GetImageListForSize` at the DPI
  size) become icons in today's index order, the designer streams are gone, the six index buttons take an `EnumIcons`. 2 new `EnumIcons` (Scan,
  UsbStick) and the 11 imaging members in the WinForms enum (113 each). Owner questions in section 3.5. No window needs showing.

- **Status UI-9b-3 (2026-11-20).** Done on `ui/ui9b3-1` (`docs/port-notes/UI-9b.md`, section 4), not merged yet, waiting for G-UI3
  (`.git/od-tools/test-results/G-UI3-ui9b3`, rendered off screen). The other trees: the task lists and the reminders grid's five pictures, the tasks
  toolbar, the security tree, the two auto notes trees, the email inbox's six buttons and the setup wizard's grid, labels and buttons become icons in
  today's index order (trees through `GetImageListForSize` at the DPI size, grid columns a 96 DPI 16 px list as `GridOD` scales them), the designer
  streams are gone. 15 new `EnumIcons` (128 each); the new, selected and partial states keep their meaning by shape (Filled) and role colour. Left
  for UI-9b-4: `FormEmailEdit` and every other window's lists. Owner questions in section 4.5. No window needs showing.
- **Status UI-9b-4 (2026-11-21).** Done on `ui/ui9b4-1` (`docs/port-notes/UI-9b.md`, sections 5 and 6), not merged yet, waiting for G-UI3
  (`.git/od-tools/test-results/G-UI3-ui9b4`, rendered off screen). The remaining forms' lists: 29 designer lists in 29 windows become icons in today's
  index order (the claims, accounting, journal, web, signing, report and print preview toolbars, the email and wiki editors with the wiki, the record
  audio buttons, the four info button grids, the dashboard tabs; the lists no control showed are kept as arrays); the designer streams are gone.
  19 new `EnumIcons` (147 each). `GridOD` takes a column list at its own scale (`GridColumn.ImageListFactory`) and draws it pixel for pixel, so the
  task, reminder, setup wizard and info button pictures are sharp at 150 and 200%; a 96 DPI list draws as before. UI-9b is complete: kept lists are
  the procedure button lists (database pictures), `ControlChart`'s and the empty `FormErx` and `ODDateRangePicker` lists. Owner questions in section 5.6.

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
- **Status UI-10-1 (2026-11-21).** Done on `ui/ui10-1` (`docs/port-notes/UI-10.md` section 4), merged into main 2026-11-21, the owner reviews G-UI3 at the end (`.git/od-tools/test-results/G-UI3-ui101`, rendered off screen): FormPatientEdit, FormProvEdit, FormClaimEdit, FormProcEdit, ControlAccount and the 28 preference pages (30 designer files edited, 546 controls in `fit-fixes.tsv`: 399 size, 147 `AutoSize` off with an explicit size). In the slice's windows the scan's new overflows at 100% fall from 392 to 86 (54 of them for the owner, with pictures; 32 are 1-2 px of height with the ink inside) and the self-sizing overlaps from 135 to 10 (controls of other designers: `ODDatePicker`, `DashIndividualDiscount`). The scan's TSVs for every file: `tests/UiRefreshTests/Data/fit-scan.tsv`, `fit-scan-files.tsv`; the tool `tools/UiFit/fit_tools.py`.
- **Status UI-10-2 (2026-11-21).** Done on `ui/ui10-2-1` (`docs/port-notes/UI-10.md` section 5), merged into main 2026-11-21, the owner reviews G-UI3 at the end (`.git/od-tools/test-results/G-UI3-ui102`, rendered off screen): the next 60 designer files of the scan's ranking (positions 5 to 79 of `fit-scan-files.tsv` less the files slice 1 owns): 1,052 controls in `fit-fixes.tsv` (529 size, 523 `AutoSize` off with an explicit size). In the slice's windows the scan's new overflows at 100% fall from 457 to 123 (65 of them for the owner, with pictures; 55 are 1-2 px of height with the ink inside) and the self-sizing overlaps from 510 to 33 (controls of other designers or a control's own layout). The needs-owner list: `docs/port-notes/UI-10-2-needs-owner.tsv`.
- **Status UI-10-3 (2026-11-21).** Done on `ui/ui10-3-1` (`docs/port-notes/UI-10.md` section 6), merged into main 2026-11-21, the owner reviews G-UI3 at the end (`.git/od-tools/test-results/G-UI3-ui103`, rendered off screen): the 58 designer files of ranking positions 61 to 120 (456 controls in `fit-fixes.tsv`: 338 size, 118 `AutoSize` off with an explicit size). In the slice's 60 windows the scan's new overflows at 100% fall from 308 to 53 and the self-sizing overlaps from 112 to 37; 149 items for the owner in `docs/port-notes/UI-10-3-needs-owner.tsv`, 57 with pictures.
- **Status UI-10-4 (2026-11-21).** Done on `ui/ui10-4-1` (`docs/port-notes/UI-10.md` section 8), merged into main 2026-11-21, the owner reviews G-UI3 at the end (`.git/od-tools/test-results/G-UI3-ui104`, rendered off screen): the 57 designer files of ranking positions 139 to 198 (277 controls in `fit-fixes.tsv`: 221 size, 56 `AutoSize` off with an explicit size). In the slice's 60 windows the scan's new overflows at 100% fall from 176 to 39 and the self-sizing overlaps from 77 to 41; 115 items for the owner in `docs/port-notes/UI-10-4-needs-owner.tsv`, 39 with pictures.
- **Status UI-10-5 (2026-11-21).** Done on `ui/ui10-5-1` (`docs/port-notes/UI-10.md` section 7), merged into main 2026-11-21, the owner reviews G-UI3 at the end (`.git/od-tools/test-results/G-UI3-ui105`, rendered off screen): the 59 designer files of ranking positions 203 to 265 (195 controls in `fit-fixes.tsv`: 152 size, 43 `AutoSize` off with an explicit size). In the slice's 60 windows the scan's new overflows at 100% fall from 123 to 39 and the self-sizing overlaps from 37 to 6; 90 items for the owner in `docs/port-notes/UI-10-5-needs-owner.tsv`, 39 with pictures.
- **Status UI-10-6 (2026-11-21).** Done on `ui/ui10-6-1` (`docs/port-notes/UI-10.md` section 9), merged into main 2026-11-21, the owner reviews G-UI3 at the end (`.git/od-tools/test-results/G-UI3-ui106`, rendered off screen): the 48 designer files (of 60) of ranking positions 266 to 327 (117 controls in `fit-fixes.tsv`: 108 size, 9 AutoSize off; 76 items for the owner in `docs/port-notes/UI-10-6-needs-owner.tsv`).
- **Status UI-10-7 (2026-11-21).** Done on `ui/ui10-7-1` (`docs/port-notes/UI-10.md` section 10), merged into main 2026-11-21, the owner reviews G-UI3 at the end (`.git/od-tools/test-results/G-UI3-ui107`, rendered off screen): the 48 designer files edited of ranking positions 328 to 387 (95 controls in `fit-fixes.tsv`: 82 size, 13 `AutoSize` off with an explicit size). In the slice's 60 windows the scan's new overflows at 100% fall from 59 to 15 and the self-sizing overlaps from 6 to 0; 69 items for the owner in `docs/port-notes/UI-10-7-needs-owner.tsv`, 15 with pictures.
- **Status UI-10-8 (2026-11-21).** Done on `ui/ui10-8-1` (`docs/port-notes/UI-10.md` section 11), merged into main 2026-11-21, the owner reviews G-UI3 at the end (`.git/od-tools/test-results/G-UI3-ui108`, rendered off screen): the 53 designer files (of 60) of ranking positions 388 to 449 (101 controls in `fit-fixes.tsv`: 84 size, 17 AutoSize off; 65 items for the owner in `docs/port-notes/UI-10-8-needs-owner.tsv`, 15 with pictures).
- **Status UI-10-9 (2026-11-21).** Done on `ui/ui10-9-1` (`docs/port-notes/UI-10.md` section 12), merged into main 2026-11-21, the owner reviews G-UI3 at the end (`.git/od-tools/test-results/G-UI3-ui109`, rendered off screen): the 25 designer files edited of ranking positions 450 to 509 (42 controls in `fit-fixes.tsv`: 35 size, 7 `AutoSize` off with an explicit size). In the slice's 60 windows the scan's new overflows at 100% fall from 11 to 5 and the self-sizing overlaps from 5 to 0; 5 labels for the owner with pictures, 43 grid scroll bars and 35 files that need no edit listed in `docs/port-notes/UI-10-9-needs-owner.tsv`.
- **Status UI-10-10 (2026-11-21).** Done on `ui/ui10-10-1` (`docs/port-notes/UI-10.md` section 13), merged into main 2026-11-21, the owner reviews G-UI3 at the end (`.git/od-tools/test-results/G-UI3-ui1010`, rendered off screen): ranking positions 510 to 569, 22 designer files edited (26 controls in `fit-fixes.tsv`). In the slice's 60 windows the scan's new overflows at 100% fall from 12 to 4; 49 items for the owner in `docs/port-notes/UI-10-10-needs-owner.tsv` (4 labels with pictures, 45 grid scroll bars).
- **Status UI-10-11 (2026-11-21).** Done on `ui/ui10-11-1` (`docs/port-notes/UI-10.md` section 14), merged into main 2026-11-21, the owner reviews G-UI3 at the end (`.git/od-tools/test-results/G-UI3-ui1011`, rendered off screen): the last slice, the 105 designer files of ranking positions 570 to the end (97 controls in 45 files in `fit-fixes.tsv`: 87 size, 10 `AutoSize` off with an explicit size). In the slice's windows the scan's new overflows at 100% fall from 25 to 10 and the self-sizing overlaps from 6 to 0; 83 items for the owner in `docs/port-notes/UI-10-11-needs-owner.tsv` (10 with pictures, 73 grid scroll bars), 60 files need no edit.

**UI-2b: the flip**
- Before the flip: pin the inherited-font sites the UI-2a-4..n slices list (docs/port-notes/UI-2a.md, 'the UI-2b checklist'), such as
  FormEquipment's printed total and FormSheetDefEdit's canvas labels, so printouts and office content keep today's font.
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
- **Status UI-2b (2026-11-21).** Done on `ui/ui2b-1` (`docs/port-notes/UI-2b.md`), not merged yet; the owner reviews G-UI2 at the end
  (`.git/od-tools/test-results/G-UI2-ui2b`, rendered off screen). The pins of the UI-2b checklist (printouts and office content keep
  Microsoft Sans Serif 8.25, `UiTheme.NewFontPinned`, the WPF `Font.ForDocument`), then the flip: the tokens hold `UiFontOption.UI2b`
  (Segoe UI 9, 17 px grid rows, Segoe UI Semibold 10 titles, Segoe UI 8 module captions), the 12 exe literals and 17 literal designer
  fonts follow. The scan at the program's own font reports nothing beyond the needs-owner lists: 90 more size fixes in 33 files whose
  designer code the UI-10 ranking had as "(no designer file)", 50 items in `docs/port-notes/UI-2b-needs-owner.tsv`. The decisions
  taken for the owner's review are in the port note.

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

**UI-12: the layout grid (the owner's decision of 2026-11-21, section 1)**
- Rules: [UI-12.md](../port-notes/UI-12.md) section 1 (8 px container padding, labels 6 px before their fields and right-aligned, one
  field height and one row pitch per container, check boxes with the box on the left, dialog buttons bottom right). Tool:
  `tools/UiFit/grid_tools.py` (planner and applier) with the `GridDump` control tree of `tests/UiRefreshTests`; `GridRulesTests` checks
  the rules on the windows of `tests/UiRefreshTests/Data/grid-windows.tsv`.
- Slices: **UI-12 pilot** (branch `ui/ui12p-1`): the 33 windows of UI-10 slice 1 and the 14 windows of the owner's G-UI3 review, the
  shared input frame (`UiInputFrame`) and the Progress Notes page bar. Then **UI-12-1 to UI-12-n** over the UI-10 ranking order
  (`fit-scan-files.tsv`), about 40 designer files each, then the rest (the windows the ranking does not hold).
- Status of the pilot (2026-11-21, round 2): the owner's answers (row pitch 27: fields 23 px, 4 px apart; a 1920 x 1080 screen, a client
  area up to about 1000 x 1880; FormPreferences grows to show its pages) and the lead's audit applied. 42 of the 47 windows laid out by
  the tool (the 40 of round 1, FormApptEdit and FormPayment laid out in parts through `grid-scope.tsv`), the Preferences pages in the
  Preferences style (one field column per page, labels where the check box text starts); a re-run of the planner on the build plans
  nothing. Round 3 (2026-11-21): all 47 windows laid out: FormInsPlan, FormClaimEdit and FormProcEdit (their tabs by the planner in
  keep-width parts) and the two modules' designer-placed parts (the Chart's Enter Treatment page and button rows, the Account's aging strip
  and side panel) by layout scripts (`tools/UiFit/grid_hand.py`, `tools/UiFit/hand/`); the lead's audit of round 2 fixed; the stock radio
  button drawn like the check box (`UiRadioLook`); `GridRulesTests` in every grid window with the owner's addition (no text past its
  container, equal gaps between consecutive fields, check boxes and buttons); kept as on main with reasons: the Chart's Missing, Movements,
  Show and Ortho pages and the Account's hidden summary groups (UI-12.md section 5); gallery G-UI3-ui12p redrawn from round 3.
  Round 4 (branch `ui/ui12p-2`, the lead's 27 review findings): the Preferences field column at 50% of the page; the clinic picker, one-line
  multi-line boxes and wrapped notes on the grid; groups whose hidden last row the code shows grow in code; code that sizes a window is
  listed by the planner and FormInsBenefits' and FormPayment's sizes follow the designer; one input height per row at 150%; the harness
  draws windows at their designer size after their handles; GridRulesTests checks hidden controls inside their container; gallery
  G-UI3-ui12p redrawn from round 4 (UI-12.md section 4, "Round 4").
  Round 5 (branch `ui/ui12p-3`, the lead's 13 review findings of round 4): the planner keeps a wider gap only where a hidden row or a
  separator line explains it, sizes a wrapped label beside its field to its measured lines and never lets a line start with a hyphen;
  FormPayment's filter captions and second row on the field columns; one content box in FormHL7DefEdit, FormMedLabEdit and FormEmailInbox;
  FormInsPlan's adjustments list holds its lines; the clinic picker's label drawn as a Label draws; gallery G-UI3-ui12p redrawn from
  round 5 (UI-12.md section 4, "Round 5").
  Round 6 (branch `ui/ui12p-4-1`, the polish round: the lead's non-blocking list of round 5): every multi-line text box draws the
  inputs' frame and the up-down boxes' arrows draw in the theme (`UiUpDownLook`); lists hold their items (FormProvEdit's Specialty,
  FormProcCodeEdit's Treatment Area); the message preview's label gaps; the edges and gaps of FormClaimEdit, FormMedLabEdit,
  FormPractice, FormProvEdit, FormProcCodeEdit (its tab control to the content edge), FormScreenEdit, FormPatientEdit and the recurring
  charges page; the planner grows a wide field to its column's edge and stands a Preferences label on a multi-line box's first line;
  gallery G-UI3-ui12p redrawn from round 6 (UI-12.md section 4, "Round 6").
  Round 7 (branch `ui/ui12p-4-2`, the fix round of round 6): every bordered rich text box (ODtextBox) draws the inputs' frame, single-line
  ones too (FormEmailInbox's address and subject boxes, ControlChart's search box); the message preview's address rows on the grid (23 px
  at a 27 px pitch, 8 px before the Attachments tab control); GridRulesTests' `unframed` rule; gallery G-UI3-ui12p redrawn from round 7
  (UI-12.md section 4, "Round 7").
  Slice 1 (UI-12-1, branch `ui/ui12-1-1`, after the owner approved the pilot's look through G-UI3-ui13): the next 50 designer files of
  `fit-scan-files.tsv` (ranks 7 to 81); 47 windows in `grid-windows.tsv` with slice 1: 38 laid out by the planner (24 of them touched by a
  layout script on the planner's tree), 8 by their own layout script (mode hand), ControlTreat kept as on main for a later round; left out:
  the Central Manager's two plain forms, the dashboard surface, EmailPreviewControl (the pilot's); FormPayPlan.cs moves two controls by the
  layout's offsets; gallery G-UI3-ui12-1 (47 windows before/after, 12 at 150%), audit `UI-12-audit-1.md` (UI-12.md, "Slice 1").
  Slice 1 continuation (same branch, the lead's answers of 2026-11-22): the clinic picker's label area sized from its text
  (`ComboBoxClinicPicker.WidthLabelArea`, "Clinics" whole, the box on its column at every scale; the planner, the scripts and
  GridRulesTests follow it; the pilot's pickers moved onto their columns), the report windows on the 8 px margin, ControlTreat laid out by a
  layout script, the planner's rounding rows pinned, the whole window at 150% (before pictures from a build of main); fix rounds 5 to 7 of
  the audit (FormCloneAdd, FormMedical, FormRpLaserLabels, FormRxDefEdit, ControlTreat and its picture); gallery G-UI3-ui12-1 redrawn (63
  pictures with the clinic picker's style pictures), audit passes 4 and 5 (UI-12.md, "Slice 1, continuation").
  Slice 1 fix round 8 (branch `ui/ui12-1-2`, the lead's review of 23 findings): FormCarrierEdit's Carrier Group row back above the CDAnet
  group (outside Canada Load took it off the window), the labels whose text the code sets sized for it (`grid-runtime-texts.tsv`: the
  Canadian address labels, the two-line day of FormScheduleDayEdit), FormFamilyBalancer's three-line check box, the pickers on their
  columns, one right edge and one group height where the slice had broken them, UserControlReportSetup anchored to its page, the
  individual insurance summary's labels whole; GridRulesTests checks no cut text in the grid windows, the pickers' columns and
  FormCarrierEdit's client area outside Canada; audit pass 6 (UI-12.md, "Slice 1, fix round 8").
  Slice 1 fix round 9 (branch `ui/ui12-1-3`, the lead's review of 21 findings): the clinic picker's label drawn as a WinForms Label draws
  (no padding, so it ends on the label column; GridRulesTests checks its ink), right-to-left labels the grid aligns cleared, four new layout
  scripts (FormArManager, FormEmailMessageEdit, FormPaySplitEdit, FormRpTreatmentFinder) and ten changed; gallery redrawn from `d2`, audit
  pass 7 (UI-12.md, "Slice 1, fix round 9").
  Slice 2 (UI-12-2, branch `ui/ui12-2-1`): fit-scan ranks 82 to 142, 46 windows on the grid (27 laid out by the planner, 19 of them
  with a layout script; 19 by their own layout script from main's arrangement where the planner placed them badly), 9 files skipped
  (owner-drawn surfaces, other exes, already on the grid), 5 windows left for the continuation (FormTaskEdit, FormClaimPayBatch,
  FormAdvertisingPatientList, FormPayConnectSetup, UserControlSetupWizSchedule); the pilot's 12 cut texts fixed; the 150% harness draws
  labels, check boxes, buttons and group titles at the scaled font; the clinic picker's box edge rounded once at 150%; no planner rule
  added (the earlier windows re-plan as recorded); gallery G-UI3-ui12-2 (46 windows, 14 at 150%, styles, the pilot follow-up), audit
  `UI-12-audit-2.md` (UI-12.md section 7).
  Slice 2 continuation (2026-11-23, same branch): the 5 windows left laid out by their own layout scripts (51 slice windows on the grid);
  the harness runs a base class's designer only when its constructor does (the setup wizard's pages never had SetupWizControl's title and
  description labels); RendererTests' menu strip test inconclusive in a session that reports 8 bits per pixel or less (a disconnected one);
  the re-plan counts recorded as the lead set them (slice 1 = 461, pilot = 243); gallery and audit extended (UI-12.md section 7,
  "Slice 2, continuation").
  Slice 2 fix round (2026-11-23, branch `ui/ui12-2-2`, the lead's review of the gallery, 11 findings): the history row of FormClaimsSend,
  the date ranges 2 px over their rows, FormFeeScheds' right column on one edge, even list columns (FormLetterMergeEdit, FormRpInsAging's
  new script), FormProcGroup's and FormTaskSearch's edges, icon buttons wide enough for their text, FormPharmacyEdit's picker colour; the
  150% gallery draws hosted modules and pages at the scaled font (the insurance summaries at the scale's dpi) and the pilot follow-up's
  states (ControlAccount's summaries and TOTAL Owed panel, ControlChart's Show and Draw pages, FormBackup's Archive page); ControlChart's
  Show page check boxes and Down whole; re-plans 461 / 243 as recorded (UI-12.md section 7, "Slice 2, fix round").
- Gallery: G-UI3 per slice, the owner's rows first.

**UI-13: every element type in the new look (the owner, 2026-11-22: "Overall make sure you modernize all element types")**
- Why: the owner's review of G-UI3-ui12p (`docs/port-notes/UI-12-owner-review-2.tsv`): the Chart's sliding bar, FormProcCodeEdit's time
  pattern, the old scroll bars (FormProvEdit, ControlAccount) and the Account's aging strip. Restyle only, in shared code, plus the layout
  fixes of the four rows. Note: [UI-13.md](../port-notes/UI-13.md); inventory: `docs/port-notes/UI-13-inventory.tsv`
  (`tools/UiFit/element_inventory.py`).
- Status (2026-11-22, branch `ui/ui13-1`): every scroll bar (WinForms controls, frame scroll bars, WPF) in one Fluent look; the track bar a
  Fluent slider; the time pattern a framed column of cells on the grid with its caption and Minutes box; the Account's aging strip on the
  grid; the stock buttons, check boxes, radio circles on the right, grey label blocks, progress bars, borders, tree and list views, data grid
  views, link labels and calendars in the tokens; the inputs' dark corner pixel gone; the WPF scroll bar style; the element inventory
  (169 types: 125 in the new look, 27 kept with a reason, 17 drawing nothing) and its test; gallery G-UI3-ui13 with the audit
  (`UI-13-audit.md`). Kept with reasons (UI-13.md section 4): Windows' dialogs and message boxes, the stock calendars' inside (Windows 11's
  themed calendar), the ToolTip component, the stock combo box's drop-down list, content.
- Fix round (2026-11-22, branch `ui/ui13-2`): the painted looks scale from the painted control's window (dpi times zoom), the data grid
  view's header flat with Gridline lines and its frame StrokeStrong, stock button captions on one line unless the button holds two, the time
  pattern's cells, letters and slider steps at the window's scale, the ActiveCaption grid title and thin dark divider panels in the look,
  trees without dotted lines; the style sheets and the windows outside the pilot drawn with Windows' visual styles (171 inventory rows: 127
  in the new look).
- Fix round 2 (2026-11-22, branch `ui/ui13-3`): the themed date picker's whole calendar button under the chevron
  (DTM_GETDATETIMEPICKERINFO), its value in the Text token and a disabled picker's box in the disabled grey; the style sheet's program list
  laid out by the window's LayoutManager and painted at the scale's dpi (its rows, text and 26 px scroll bar at 150%), a date picker sheet;
  the 150% frame scroll bar limit recorded (UI-13.md section 4) with a test that the scroll look scales with the bar Windows reports.
- Gallery: G-UI3-ui13 (the owner's four rows, a style sheet per element type at 100% and 150%, the 47 UI-12 windows, one window per type
  outside the pilot).

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
  with and without a patient, each also at one program zoom step. The "with a patient" screens include the patient dashboard open with
  a synthetic widget (labels, the appointment grid, the insurance and discount fields); inside the widget only the grid, the group box
  and the context menu may change (5.1). The tooth chart area of the Chart module is checked as unchanged, not
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
| `tests/ToothChartImageTests/TextBelowStripTests.cs:88` | the strip's `ForeColor` is `Color.Black` | by UQ-6: restyled, so it compares with the `Text` token; kept, so it stays | UI-4c-2 |
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
  `InitializeLayoutManager` or host a UserControl in the test FormODBase (5.4). Its allow list: the token itself, the tooth chart,
  `ContrPerio` and `UserControlDashboard` pins, print code, document design canvases, `FormPerio`'s print font, the claim perio
  attachment's two fonts in `FormClaimEdit`, MS Chart element fonts (axis `LabelStyle.Font`, `Title.Font`, `TitleFont`, `Legend`
  fonts; on `main` the graph's six, 5.1), sheet, eForm and dashboard-widget font defaults and font lists (`SheetFonts.cs:21-51`,
  `FormSheetDefs.cs:128`, `FormDashboardWidgetSetup.cs:190`, `DashLabel.cs:32`), and the plain forms' and those windows' literal
  control and form fonts until UI-2b; no `AutoScaleMode.Font` assignment outside `UnitTests` and `PluginExample`, comments skipped
  (UI-2a-2); each exe's font literal, and each literal control or form font in the designer code of a plain form or of a window
  without `InitializeLayoutManager`, matches the token by the rule of 3.4 (UI-2a; chart element fonts are not control fonts and are
  not checked); no bitmap toolbar pictures outside an allow list (UI-9b).
- Print guard (UI-2a-3, again in UI-2b): GridOD's print and sheet output, sheets, claim forms, the perio chart print, the claim perio
  attachment picture, eForm PDF output, reports, RDL and MigraDoc output, and the graph dashboard's chart page (`FormPrintImage`'s
  bitmap, printed and PDF) identical before and after; a printed appointment schedule too if UQ-8 changes the appointment book.
- Content and signatures (UI-2a-1): a Microsoft Sans Serif sheet field keeps its font in `FormSheetFillEdit`; a perio chart picture is
  unchanged with the token override set to Segoe UI; a synthetic patient dashboard widget (a label sheet field with and without a font
  name, and a `DashIndividualDiscount` field) keeps its fonts with the override set to Segoe UI, at 100% and one zoom step; a late
  `MarkContent` throws; a signed group note in `FormProcEdit` stays valid after a zoom change, a resize and a save.
- Font mapping (UI-2a-1): with today's tokens every font comes back unchanged; with the override set to Segoe UI, explicit designer
  fonts follow the token in all three recording places (a window's designer controls, an `Add`ed control, a preference page and
  `ControlImages` through `AddUnscaled`); the underlined `FormInsBenefits` labels draw as today.
- Misc Colors per item and the floating image windows' two frames (UI-3); tab header width formula, order and right-edge fit in the
  windows with the most tabs (UI-4b-2); toolbar order and right-edge fit at 1024, 1366 and 1920 px,
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
  px. Mitigation: UQ-9; UI-6's order and right-edge test at 1024, 1366 and 1920 px; UI-4b-2's tab strip test; before and after
  strips in G-UI2.
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
  sets no font of its own would take the window's new font anyway; the patient dashboard's widgets come from the database, so tier 1
  cannot see them. Mitigation: the content types, `MarkContent` and the font pins, with their tests (5.1), and the dashboard with a
  synthetic widget in group U and G-UI2.
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

**All answered by the owner on 2026-11-17, each as recommended below.** The packages build to these answers; galleries still need the owner's approval.

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
  cut, and staff find the same buttons in the same order. UI-6 checks the toolbars and the menu bar still fit at 1024, 1366 and 1920 px,
  and UI-4b-2 checks the tab strips of the windows with the most tabs.
- (b) Each item keeps today's width and its text is cut with "…" when it does not fit. Nothing shifts, but some captions are cut.
- G-UI2 shows each module toolbar and the menu bar before and after, and G-UI2 and G-UI3 those tab strips.

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

The fifth review (2026-11-17) raised 4 problems. All were applied; its facts held on `main`, with these refinements:

- **Chart element fonts.** Right: the graph's six Microsoft Sans Serif designer fonts are MS Chart axis and title fonts
  (`GraphIncome.Designer.cs:86, 116, 165`, `GraphQuantityOverTime.Designer.cs:111, 134`, `GraphQuantityOverTimeFilter.Designer.cs:108`),
  and `FormPrintImage.cs:95-102` prints the charts. They stay, the literal rule and test cover control and form fonts only, the allow
  list names them and the print guard covers the dashboard printout (5.1, 5.2, UI-2a-4, section 9). So the plain exes have 8 Microsoft
  Sans Serif control fonts outside OpenPmsDental, not 14. The review also said OpenPmsDental has chart element fonts under
  LayoutManager; a grep of `main` finds none there, so the rule is stated for every project without that example.
- **The patient dashboard.** Right that it is office content the plan did not name (5.1). One change to the fix: the pin goes on
  `UserControlDashboard`, not on each `UserControlDashboardWidget`. LayoutManager records and rebuilds the dashboard's own font with the
  program zoom (`FormOpenDental.cs:228`; only its children are skipped, `LayoutManagerForms.cs:1105`), but never records the widgets,
  which are added with `Controls.Add`. A fixed 8.25 pin on the widget would stop the inherited font following the zoom, a visible
  change; a pin on the dashboard keeps today's behaviour. `UserControlDashboard` is the content type for the same reason. The test also
  covers a field that inherits its font (`DashIndividualDiscount`, which sets none) at one zoom step. Besides the grid, the discount
  widget's `UI.GroupBox` and the context menus take the restyle like everywhere else.
- **UI-4b and UI-4c.** Right: 58 and 50 files on `main` (`.resx` aside). Cut as the review proposed, with file counts per slice
  (21, 12 and 26; 25 and 25). The UI-4b slices run one after another (they share `Generic.xaml` and the `CtrlEFormFill` copies); the
  UI-4c slices share no files and may run side by side.
- **Tab headers.** Right (3.6). The check went to UI-4b-2, which owns `UI.TabControl`, rather than UI-6, using the windows with the
  most self-drawn tabs on `main` (`FormClaimEdit` 10, `FormMedical` 9, `FormFeeSchedTools` 6, `FormPatientEdit` 5).
