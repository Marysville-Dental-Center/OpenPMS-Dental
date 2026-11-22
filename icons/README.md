# Icons

The program's icons are Fluent UI System Icons (Microsoft, MIT licence), drawn as vectors from path data compiled into `CodeBase.dll`.
No icon font and no picture files. The plan is `docs/specs/ui-refresh-plan.md`, sections 3.5, 4.1 to 4.3; the pipeline came with package
UI-7a (`docs/port-notes/UI-7a.md`).

## The files

| File | What |
|---|---|
| `icons/manifest.json` | every icon the program draws: one row per purpose (below) |
| `icons/custom/` | the program's own SVGs, for purposes Fluent has no icon for: the dental icons of UI-8 (below, "The custom icons") |
| `tools/New-DentalIcons.ps1`, `tools/DentalIcons/DentalIconArt.cs` | the drawing of the custom icons: shapes in, `icons/custom/*.svg` out (deterministic, committed) |
| `third-party/fluentui-system-icons/icons/` | the Fluent SVGs the manifest names, unchanged, in the npm package's layout `<name>_<size>_<style>.svg` |
| `third-party/fluentui-system-icons/LICENSE`, `SOURCE.txt` | the MIT licence; the version, download, integrity hash, commit and each file's SHA-256 |
| `tools/New-IconData.ps1` | the generator: manifest and SVGs in, `CodeBase/UI/Icons/IconData.g.cs` out (deterministic, committed) |
| `CodeBase/UI/Icons/IconData.g.cs` | generated: the `UiGlyph` enum and the table of designs (path data in design pixels) |
| `CodeBase/UI/Icons/UiIcons*.cs` | the renderer: the design for a slot, GDI+, `GetBitmap`, `GetImageList`, WPF, and `IIconPathSink` for Direct2D |
| `OpenPmsDental/UI/Direct2d/Direct2d.cs` | `FillGlyph`, `FillIcon` (Direct2D path calls) and `DrawGlyph` (Direct2D into a bitmap, GDI+ with `IconLibrary.OnlyGDI`) |

The version is the newest `@fluentui/svg-icons` on the day it was taken (the owner's rule: always the latest). Today: **1.1.346**
(published 2026-11-13, commit `ba34950ee`, release commit `67e6dec1e`). The repository has no GitHub Releases; its versions are the npm
packages. The approved download is `.git/od-tools/downloads/ui7a-fluentui-svg-icons-20261117/` (SOURCES, SHA256SUMS, MANIFEST).

## A manifest row

```json
{ "enum": "UiGlyph", "name": "ChevronDown", "fluent": "chevron_down", "style": "regular", "sizes": [ 12, 16, 20, 24, 32, 48 ],
  "role": "Text", "purpose": "drop-down arrows of combo boxes, drop-down buttons and menus; expand (UI-4a to UI-6)" }
```

- `enum`: `UiGlyph` (the control glyphs; the generator writes this enum, in manifest order after `None`) or `EnumIcons` (the toolbar
  and module icons; UI-7b added those rows, keyed by the member's name; the enum stays in both `IconLibrary.cs` files, and a member of
  both enums has one row that both libraries draw).
- `name`: the enum member. One row per purpose: a purpose drawn in many places is one row. An `EnumIcons` member's Filled style (the
  "on" state, section 4.3) is a second row named `<member>Filled` with `"style": "filled"`; `IconLibrary.GetDesigns(icon,isFilled:true)`
  finds it and falls back to the Regular row when there is none. Today the seven module bar icons have one.
- `fluent`: the Fluent file name stem, or `custom`: a stem under `icons/custom` (`<custom>_<size>.svg`). Exactly one of the two.
- `style`: `regular`, or `filled` for "on" states (the selected module, a toggled toolbar button), section 4.3.
- `sizes`: the design sizes compiled in, rising: 12, 16, 20, 24, 32, 48 (section 3.5; the 24 design serves 24 to 31 px, so no slot asks
  for 28; an `EnumIcons` row may name 28 when that design is the one it draws at every size, as `Bold`). Take every size Fluent has for
  the icon. A `UiGlyph` slot whose design is missing draws the largest smaller one, centred; a slot below the smallest design draws the
  smallest shrunk to the slot. An `EnumIcons` or `DentalIcon` slot (the toolbars, buttons, lists and the module bar) always draws in the
  box of section 3.5 for the slot: a missing design is replaced by the nearest one, scaled so its Fluent keyline square (10 of 12, 14 of
  16, 16 of 20, 20 of 24, 28 of 32, 40 of 48 px) fills the box's, so every icon of a strip has one size and stroke weight (the UI-9b-4 fix,
  docs/port-notes/UI-9b.md, section 5.8).
- `opticalScale` (optional, `EnumIcons` only, 0.8 to 1.5, not 1): the row's designs are drawn that much larger or smaller about their
  centre, so glyphs of one kind that Fluent draws at different sizes match: the editors' text buttons H1 to H3 (15/14), B (0.92) and I
  (15/16) have one cap height, 15 of the 24 px design's keyline square. Only where a design is scaled anyway (a box of another size): a
  design in a box of its own size (the 20 px designs in the toolbars' 20 px box at 100%) is drawn pixel for pixel, so its 1 px stems stay
  crisp.
- `role`: the default colour token (`Text`, `Accent`, `Danger`, `Success`, `Warning`). The drawing call may pass its own colour: a white
  check mark on the accent, the title bar's text colour on a caption button, `TextDisabled` for a disabled icon.
- `purpose`: where it is drawn.

## Adding or changing an icon

1. Find the icon in the npm package's `icons/` folder of the approved download (or a newer one: see "A new version").
2. Copy its files for the sizes into `third-party/fluentui-system-icons/icons/` unchanged, and add their SHA-256 lines to `SOURCE.txt`.
3. Add the manifest row.
4. Run `powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-IconData.ps1` and commit the manifest, the SVGs, `SOURCE.txt`
   and `IconData.g.cs` together. `tools\New-IconData.ps1 -Check` (and `UiRefreshTests`) fails while they disagree.

The generator refuses an SVG that no row names, a missing file, an SVG whose `viewBox` is not `0 0 <size> <size>`, and SVGs with
anything but `<path d>` elements (groups, clip paths, transforms, fills, even-odd rules). In 1.1.346, 49 of the regular and filled
SVGs set a fill, an opacity or a transform and 5 hold groups and clip paths; none uses the even-odd rule. Teach the generator what such
an icon needs when one is chosen, or pick another.

## The custom icons

Fluent has no tooth chart, perio, probe, implant, X-ray type, sensor or mount icons (its Dentist is a plain tooth without a 32 px design,
and its Xray is a chest film), so the program draws its own (plan section 4.4, package UI-8, `docs/port-notes/UI-8.md`). They are
**original work of this program, under its GPL**, like the logo (`branding/README.md`): drawn as shapes in
`tools/DentalIcons/DentalIconArt.cs`, from public dental anatomy only. No shape is copied from Open Dental's pictures, from IconOrganizer
art, from Fluent or from any other icon set. Every SVG says so in a comment.

- **Style.** Fluent's: designed on a 24 unit square and drawn at 16, 20, 24, 32 and 48 px with the stroke weight of Fluent's Regular icons
  of that size (1, 1, 1.5, 2 and 2.5 px), rounded ends and joins, straight edges on whole pixels, one colour (role `Text`), and a Filled
  twin of every icon (`<name>Filled`, file stem `<stem>_filled`) for "on" states. The 16 px designs leave out what does not fit (section
  3.5's design-for-slot rule); the 48 px design serves the module bar at 150% and above.
- **The enum.** `DentalIcon`, generated into `IconData.g.cs` from the manifest's `DentalIcon` rows (custom icons only), each Regular row
  followed by its Filled twin. `UiIcons.Draw(g,DentalIcon.Tooth,slot,color)`, `UiIcons.GetBitmap`, `UiIcons.GetFilled`,
  `UiIcons.GetGeometryWpf(UiIcons.GetDesign(icon,px),slot,scale)` and `Direct2d.DrawIcon(g,UiIcons.GetDesign(icon,px),slot,color,back)`
  draw them like the glyphs.
- **Changing one.** Edit the shapes in `DentalIconArt.cs`, run `powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-DentalIcons.ps1`
  (with `-Preview <folder>` for a look at every design), then `tools\New-IconData.ps1`, and commit the drawing, the SVGs and
  `IconData.g.cs` together. `tools\New-DentalIcons.ps1 -Check` and `UiRefreshTests` fail while they disagree. The SVGs keep their bytes in
  every checkout (`.gitattributes`: `icons/custom/** -text`; one line, no line end).
- **Procedure buttons.** The 8 default procedure buttons of a new database (`db/init/new-database.sql`) carry the procedure icons' 20 px
  designs as 20x20 bitmaps (UQ-7: new databases only), made by `UiIcons.GetProcButtonPicture` in `tools/DevDbTool`'s scrub of the scratch
  database. An office's buttons never change: `ProcButtons.SetToDefault` keeps Open Dental's pictures.

| Purpose | Today | Icon (`DentalIcon`) | Shape |
|---|---|---|---|
| Module bar: Chart (`Chart32G`; selected `Chart32W`) | tooth pictures (`Gen_Chart32G.cs`, `Gen_Chart32W.cs`) | `Tooth` (selected `ToothFilled`) | a molar: two cusps, two roots |
| Module bar: Chart, medical layout (`ChartMed32`) | red medical cross (`Gen_ChartMed32.cs`) | `ChartMedical` | a chart card with a medical cross |
| Perio chart | none | `PerioChart` | a chart: the gum line over three teeth, pocket depths as bars |
| Perio probe (`Probe`, no code draws it) | probe on the gum | `PerioProbe` | a periodontal probe with a marked tip |
| Bitewing radiograph | none | `XrayBitewing` | a landscape film with its bite tab, teeth meeting at the bite line |
| Periapical radiograph; procedure button PA | radiograph (`ImageSelectorXray`); film picture | `XrayPeriapical` | a portrait film with one whole tooth |
| Panoramic radiograph | none | `XrayPano` | a wide film with both arches |
| Cephalometric radiograph | none | `XrayCeph` | a square film with a head in profile |
| Intraoral sensor | none | `Sensor` | the sensor with its active area and cable |
| Phosphor plate | none | `PhosphorPlate` | a plate with its orientation dot and a sheen |
| Mount | none (`ImageSelectorMount` is UI-7b's Fluent grid) | `Mount` | a frame holding two rows of films |
| Intraoral photo | photo of teeth (`ImageSelectorPhoto`) | `IntraoralPhoto` | a photo with a molar in it |
| Implant | none | `Implant` | crown, abutment and threaded fixture |
| Orthodontics | none | `Ortho` | a front tooth with a bracket and the arch wire |
| Procedure button Amalgam | coloured picture | `ProcAmalgam` | a molar with a solid filling in its crown |
| Procedure button Composite | coloured picture | `ProcComposite` | a front tooth with a filling at its corner |
| Procedure button Crown-PFM | coloured picture (shared with Crown-Ceramic) | `ProcCrownPfm` | a molar capped to the margin, a metal collar line |
| Procedure button Crown-Ceramic | coloured picture (shared with Crown-PFM) | `ProcCrownCeramic` | a molar capped to the margin, a shine on the cap |
| Procedure button RCT | coloured picture | `ProcRct` | a molar with its pulp chamber and both canals filled |
| Procedure buttons RCT BU PFM, RCT BU Ceramic | coloured picture (shared) | `ProcRctBuildUp` | a molar with filled canals and a solid core |

The module bar draws `Chart32G`, `Chart32W` and `ChartMed32` as `Tooth`, `ToothFilled` and `ChartMedical`: both `IconLibrary` copies map
them (`IconLibrary.GetDentalIcon`; their Filled style is the dental icon's Filled twin), and they have no `EnumIcons` row. Their pictures
(`Gen_Chart32G.cs`, `Gen_Chart32W.cs`, `Gen_ChartMed32.cs`, `IconSelector.cs`) are gone (UI-8, `docs/port-notes/UI-8.md`).

## A new version

Download the newest `@fluentui/svg-icons` into a new dated folder under `.git/od-tools/downloads` with SOURCES, SHA256SUMS and MANIFEST,
copy the manifest's files again, update `SOURCE.txt`, `fluentVersion` in the manifest and the version in `THIRD-PARTY-NOTICES.txt`,
run the generator, and compare the drawings (UiRefreshTests draws every row; the gallery shows them).

## Drawing

```csharp
UiIcons.Draw(g,UiGlyph.ChevronDown,rectangleSlot,EnumUiColor.Text);          //GDI+, the slot in device pixels
UiIcons.DrawDisabled(g,UiGlyph.ChevronDown,rectangleSlot);                     //TextDisabled
Bitmap bitmap=UiIcons.GetBitmap(UiGlyph.Dismiss,16,LayoutManager.ScaleMy(),color); //a new 32-bit bitmap at the DPI size; the caller owns it
ImageList imageList=UiIcons.GetImageList(arrayGlyphs,sizePixels,EnumUiColor.Text); //rebuild when the DPI changes
direct2d.FillGlyph(UiGlyph.Checkmark,rectangleSlot,color);                     //between BeginDraw and EndDraw
Direct2d.DrawGlyph(g,UiGlyph.Checkmark,rectangleSlot,color,colorBack);         //Direct2D into a bitmap; GDI+ with NoD2D.txt
UiIcons.DrawWpf(drawingContext,UiGlyph.ChevronDown,rectSlot,brush,scale);      //WPF; scale = device pixels per DIP
Geometry geometry=UiIcons.GetGeometryWpf(UiGlyph.ChevronDown,12,scale);        //frozen, for a Path in XAML code
```

## The mapping (section 4.3)

One row per purpose. UI-7b (the `EnumIcons`), UI-8 (the dental icons) and the UI-9 slices (pictures to icons) add their rows here and to
the manifest; G-UI1 part 3 shows them.

| Purpose | Today | Icon | Source | Package |
|---|---|---|---|---|
| Caption button: minimize | the control's own drawing | Subtract (regular) | Fluent | UI-7a (drawn from UI-3) |
| Caption button: maximize | the control's own drawing | Maximize (regular) | Fluent | UI-7a (UI-3) |
| Caption button: restore | the control's own drawing | Square Multiple (regular) | Fluent | UI-7a (UI-3) |
| Caption button: close; close marks of tabs and panels | the control's own drawing | Dismiss (regular) | Fluent | UI-7a (UI-3) |
| Up arrows: spin and scroll buttons, collapse | the control's own drawing | Chevron Up (regular) | Fluent | UI-7a (UI-4a to UI-6) |
| Drop-down arrows; expand | the control's own drawing | Chevron Down (regular) | Fluent | UI-7a (UI-4a to UI-6) |
| Previous: month calendar, tab scroll | the control's own drawing | Chevron Left (regular) | Fluent | UI-7a (UI-4b, UI-4c) |
| Next: month calendar, tab scroll, sub-menus | the control's own drawing | Chevron Right (regular) | Fluent | UI-7a (UI-4b, UI-4c, UI-6) |
| Check box check mark; checked menu items | the control's own drawing | Checkmark (regular) | Fluent | UI-7a (UI-4a, UI-6) |
| Indeterminate check box | the control's own drawing | Line Horizontal 1 (regular), "the dash" | Fluent | UI-7a (UI-4a) |
| Module bar: Appts (`Appt32`) | calendar page with a grid | Calendar LTR (regular; filled for the selected module) | Fluent | UI-7b |
| Module bar: Family (`Family32`) | group of coloured people | People (regular; filled) | Fluent | UI-7b |
| Module bar: Account (`Account32`) | green dollar sign and card | Receipt (regular; filled); the plan's example Receipt Money has no 32 px design | Fluent | UI-7b |
| Module bar: Tx Plan (`TreatPlan32`) | clipboard with a list | Clipboard Text LTR (regular; filled); the plan's Clipboard Task List LTR has no 32 px design | Fluent | UI-7b |
| Module bar: Tx Plan, medical layout (`TreatPlanMed32`) | clipboard with a list | Heart Pulse (regular; filled); the plan's Clipboard Heart has no 32 px design | Fluent | UI-7b |
| Module bar: Chart (`Chart32G`, `Chart32W`), medical (`ChartMed32`) | tooth; red medical cross | `Tooth` (selected `ToothFilled`), `ChartMedical` (dental icons, above) | custom | UI-8 |
| Module bar: Imaging (`Imaging32`) | scanner and camera | Image Multiple (regular; filled) | Fluent | UI-7b |
| Module bar: Manage (`Manage32`) | purple gears | Briefcase (regular; filled) | Fluent | UI-7b |
| Chart picture no code draws (`Chart32`) | tooth | Clipboard Pulse (regular); the module bar's Chart draws UI-8's tooth | Fluent | UI-7b |
| Add (`Add`) | blue plus | Add (regular), `Accent` | Fluent | UI-7b |
| Delete (`DeleteX`) | red X | Delete (regular), `Danger` | Fluent | UI-7b |
| Left and right arrows (`ArrowLeft`, `ArrowRight`) | blue arrows | Arrow Left, Arrow Right (regular) | Fluent | UI-7b |
| Break appointment (`BreakAptX`) | box with a cross | Calendar Cancel (regular), `Danger` | Fluent | UI-7b |
| Set complete (`Complete`) | blue check swoosh | Checkmark Circle (regular), `Success` | Fluent | UI-7b |
| Commlog (`CommLog`) | three linked circles | Chat (regular) | Fluent | UI-7b |
| Email (`Email`) | blue envelope | Mail (regular) | Fluent | UI-7b |
| Secure web mail, no code draws it (`WebMail`) | blue envelope | Mail Shield (regular) | Fluent | UI-7b |
| Text message (`Text`) | blue speech bubbles | Phone Chat (regular; the 16 px design leaves its square, so 16 px slots draw the 20 px design) | Fluent | UI-7b |
| Recall (`Recall`) | green circling arrows | Calendar Arrow Repeat All (regular) | Fluent | UI-7b |
| Select patient (`PatSelect`) | person with a blue arrow | Person Search (regular; no 16 px design, as Text) | Fluent | UI-7b |
| Patient (`Patient`) | red person | Person (regular) | Fluent | UI-7b |
| Add family member (`PatAdd`) | plus and person | Person Add (regular) | Fluent | UI-7b |
| Delete patient (`PatDelete`) | X and person | Person Delete (regular), `Danger` | Fluent | UI-7b |
| Move to another family (`PatMoveFam`) | person with a green arrow | Person Arrow Right (regular) | Fluent | UI-7b |
| Set guarantor (`PatSetGuarantor`) | green up arrow and person | Person Money (regular) | Fluent | UI-7b |
| Perio probe, no code draws it (`Probe`) | probe on the gum | Ruler (regular) | Fluent | UI-7b |
| Imaging: Mount / Acquire (`Acquire`) | radiation sign | Scan Camera (regular) | Fluent | UI-7b |
| Imaging: pan (`ArrowsAll`) | four-way arrows | Arrow Move (regular) | Fluent | UI-7b |
| Imaging: copy, paste (`Copy`, `Paste`) | two pages; clipboard | Copy, Clipboard Paste (regular) | Fluent | UI-7b |
| Imaging: crop, flip, hand (`Crop`, `Flip`, `Hand`) | framed picture; flip arrow; hand | Crop, Flip Horizontal, Hand Right (regular) | Fluent | UI-7b |
| Imaging: rotate left, right (`RotateL`, `RotateR`) | blue rotate arrows | Arrow Rotate Counterclockwise, Arrow Rotate Clockwise (regular) | Fluent | UI-7b |
| Imaging: export, import (`Export`, `Import`) | yellow folder with an arrow | Arrow Export LTR, Arrow Import (regular) | Fluent | UI-7b |
| Imaging: print, info, video (`Print`, `Info`, `Video`) | printer; blue i; video camera | Print, Info, Video (regular) | Fluent | UI-7b |
| Imaging: scan document; selector document (`ImageSelectorDoc`) | page | Document Text (regular) | Fluent | UI-7b |
| Imaging: scan multi-page (`ScanMulti`) | stack of pages | Document Multiple (regular) | Fluent | UI-7b |
| Imaging: scan photo; selector photo (`ScanPhoto`, `ImageSelectorPhoto`) | photo of teeth | Image (regular) | Fluent | UI-7b |
| Imaging: scan radiograph; selector radiograph (`ScanXray`, `ImageSelectorXray`) | radiograph | Xray (regular) | Fluent | UI-7b |
| Image selector: file, folder, web folder (`ImageSelectorFile`, `ImageSelectorFolder`, `ImageSelectorFolderWeb`) | window; yellow folder; folder with a globe | Document, Folder, Folder Globe (regular) | Fluent | UI-7b |
| Image selector: mount (`ImageSelectorMount`) | nothing (both libraries drew no picture) | Layout Cell Four (regular) | Fluent | UI-7b |
| Move up, move down in a list (`Up`, `Down`) | green arrows (`up.gif`, `down.gif`) | Arrow Up, Arrow Down (regular) | Fluent | UI-9a-1 |
| Move, back, forward, insert, copy, more (`ArrowLeft`, `ArrowRight`) | blue arrows (`Left.gif`, `Right.gif`, FormApptSearchAdvanced's More) | Arrow Left, Arrow Right (UI-7b's rows) | Fluent | UI-9a-1 |
| Print buttons (`Print`) | printers (`butPrint.gif`, `butPrintsmall.gif`) | Print (UI-7b's row; now a WinForms member too) | Fluent | UI-9a-1 |
| Export buttons (`Export`) | sheet with a red arrow (`butExport.gif`) | Arrow Export LTR (UI-7b's row; now a WinForms member too) | Fluent | UI-9a-1 |
| Print preview (`Preview`) | page with a magnifier (`butPreview.gif`) | Document Search (regular); the plan's Document Print has no 16 px design | Fluent | UI-9a-1 |
| Mailing labels (`Label`) | a label (`butlabel.gif`) | Contact Card (regular) | Fluent | UI-9a-1 |
| To the pinboard (`Pin`) | yellow pin (`butPin.gif`, FormApptEdit) | Pin (regular) | Fluent | UI-9a-1 |
| Edit (`Edit`) | yellow pencil (`editPencil.gif`) | Edit (regular) | Fluent | UI-9a-1 |
| A note for the patient (`Note`) | note pad (FormApptsOther) | Note (regular) | Fluent | UI-9a-1 |
| Send claims, send to and update at TSI (`Send`) | red arrow up (FormClaimEdit, FormClaimResend, FormArManager) | Send (regular) | Fluent | UI-9a-1 |
| Clear a value, go, add (`DeleteX`, `ArrowRight`, `Add`) | red X (`deleteX18.gif`), green go circle (`butGoto.gif`), blue plus (FormFeeSchedGroups) | existing members, no new icon | Fluent | UI-9a-2 |
| Show or hide splits (`ChevronDown`, `ChevronUp`) | small green triangles (`arrowDownTriangle.gif`, `arrowUpTriangle.gif`) | Chevron Down, Chevron Up (regular) | Fluent | UI-9a-3 |
| Close the case or plan (`CloseDoor`) | closing door (`close_door.ico`) | Door Arrow Right (regular) | Fluent | UI-9a-3 |
| Dictate (`Microphone`) | microphone (`Microphone_22px.png`) | Mic (regular) | Fluent | UI-9a-3 |
| Zoom (`ZoomIn`, `ZoomOut`) | magnifiers (`butZoomIn.gif`, `butZoomOut.gif`) | Zoom In, Zoom Out (regular) | Fluent | UI-9a-3 |
| Add (FormOrthoChartSetup) | blue plus | existing `Add` | Fluent | UI-9a-3 |
| Record (`Record`) | red record circle (`record.gif`, FormRecordAudio) | Record (regular) | Fluent | UI-9a-4 |
| Pin, print preview, add, delete (FormRecallList, FormRxEdit, FormStatementOptions, FormSheet*) | `butPin22.gif`, `printPreview20.gif`, `Add`, `DeleteX` | existing `Pin`, `Preview`, `Add`, `DeleteX` | Fluent | UI-9a-4 |
| Copy, paste (FormTaskEdit, FormUserQuery) | `Copy.gif`, `butPaste.gif` | existing `Copy`, `Paste` (WinForms members added) | Fluent | UI-9a-5 |
| Excel export, query view (FormQuery) | `butExportExcel.gif`, `butQView.gif` | `ExportExcel` (Document Table), `QueryView` (Table Search) | Fluent | UI-9a-5 |
| Time card arrows, filter delete, label sheet pictures (FormTimeCard, FormRpReferrals, FormRpPatients, FormRpLaserLabels) | window `.resx` pictures, `DeleteX`, `butLabel` | existing `ArrowLeft`, `ArrowRight`, `Print`, `DeleteX`, `Label` | Fluent | UI-9a-5 |
| Main modules, date range picker, Graph dashboard, signature box (ControlAppt, ControlManage, OpenPmsDentalGraph, OpenPmsDentalBusiness) | `.resx` and `Resources` pictures | existing arrows, `DeleteX`, `Sync`, `Preview`, `Edit`; new `PinOff`, `Calculator`, `Backup`, `Deposit`, `Insurance`, `TaskList`, `Move`, `TableDeleteRow`, `TableDeleteColumn`, `TableInsertRow`, `TableInsertColumn`, `ChartAdd`, `Pen`, `Signature` | Fluent | UI-9a-6 |

| Report designer toolbar (RdlDesign: Table, Line, Image, Rectangle, Subreport, List, Chart, Textbox, Matrix, Save, Open, Cut, New, Undo, PDF, XML, HTML, MHT) | 21 `.resx` pictures of hidden designer buttons (16 x 16; the export pictures 18 x 16 to 26 x 16) | new `Table`, `Line`, `Image`, `Rectangle`, `Subreport` (Document Multiple), `ListBullets` (Text Bullet List), `ChartBar` (Data Bar Vertical), `TextBox`, `Matrix` (Grid), `Save`, `Open` (Folder Open), `Cut`, `NewDocument` (Document Add), `Undo` (Arrow Undo), `Pdf` (Document Pdf), `Xml` (Code), `Html` (Globe), `Mht` (Document Globe); existing `Print`, `Copy`, `Paste` | Fluent | UI-9a-7 |
| Central Manager buttons (11 `.resx` pictures), the print button of `MsgBoxCopyPaste`, the drop-down arrow of `ComboBoxMulti` | window `.resx` pictures | existing `Add`, `DeleteX`, `ArrowLeft`, `ArrowRight`, `Up`, `Down`, `Print`, `ChevronDown` | Fluent | UI-9a-7 |
| Account toolbar: Payment (`Payment`) | banknote (module ImageList) | Money (regular) | Fluent | UI-9b-1 |
| Account toolbar: Adjustment (`Adjustment`) | plus and minus (module ImageList) | Add Subtract Circle (regular) | Fluent | UI-9b-1 |
| Appts toolbar: Lists, the appointment lists (`ApptLists`) | page of lines (`apptLists.gif`) | Document Bullet List (regular) | Fluent | UI-9b-1 |
| Appts toolbar: Rapid Call (`Call`) | telephone (`DT Rapid Call.png`) | Call (regular) | Fluent | UI-9b-1 |
| Appts toolbar: Unsched, to the Unscheduled List (`Unscheduled`) | list with a U (`unschedList.gif`) | Calendar Arrow Right (regular) | Fluent | UI-9b-1 |
| Chart toolbar: New Rx, eRx (`Rx`) | the letters Rx (`Rx.gif`) | Pill (regular) | Fluent | UI-9b-1 |
| Chart toolbar: Perio Chart (`PerioChart`) | probe in a pocket (`Probe.gif`) | `PerioChart` (custom dental icon) | UI-8 | UI-9b-1 |
| Chart ImageList: anesthesia, no button shows it (`Anesthesia`, kept at its index for plug-ins) | heart monitor (`Anesth.gif`) | Pulse (regular) | Fluent | UI-9b-1 |
| Main and module toolbars: select patient, commlog, e-mail, tasks, label, text, print, statement, new claim, add insurance, update fees, break, complete, delete, patient, add, recall, family members (the toolbars' ImageList pictures) | module ImageList pictures (`Pat.gif`, `commlog.gif`, `email.gif`, `tasksNicer.gif`, `label.gif`, `Text.gif`, `print.gif`, `butPrint.gif`, `Umbrella.gif` and others) | existing `PatSelect`, `CommLog`, `Email`, `TaskList`, `Label`, `Text`, `Print`, `Insurance`, `Sync`, `BreakAptX`, `Complete`, `DeleteX`, `Patient`, `Add`, `Recall`, `PatAdd`, `PatDelete`, `PatSetGuarantor`, `PatMoveFam` | Fluent | UI-9b-1 |
| Imaging toolbars (Crop, Pan, Adj, FlipH, -90, +90) and the imaging tools list's 21 pictures by index (ImageList) | `ControlImages` tools list (`Pat.gif`, `scan.gif`, `Hand.gif`, `arrowsAll.png` and others) | `Crop`, `Hand`, `ArrowsAll`, `Flip`, `RotateL`, `RotateR`, `Scan` (Fluent Scan) and the reused `PatSelect`, `Print`, `Import`, `Export` and others | Fluent | UI-9b-2 |
| Imaging tree list, 8 pictures by index (ImageList) | `ControlImages` tree list (folder, document, radiograph, photo, file, mount, web folder, memory stick) | `ImageSelectorFolder` and its siblings, `UsbStick` (Fluent Usb Stick) | Fluent | UI-9b-2 |
| Task lists (tree, grid and reminders grid), 5 pictures by index (ImageList) | `UserControlTasks` and `ControlAppt` task lists (a list, a green check, an empty box, an orange list, an orange square) | `TaskList`, `CheckBoxChecked` (Success), `CheckBoxEmpty`, `TaskListNew` (Filled, Warning), `TaskNew` (Filled, Warning) | Fluent | UI-9b-3 |
| Tasks toolbar: Add Task List, Add Task (ImageList) | `TaskListAdd.gif`, `Add.gif` | `TaskListAdd` (Clipboard Task Add), `Add` | Fluent | UI-9b-3 |
| Security tree, 4 pictures by index (ImageList) | `grayBox.gif`, `checkBoxUnchecked.gif`, `checkBoxChecked.gif`, `checkBoxGreen.gif` | `ImageSelectorFolder`, `CheckBoxEmpty`, `CheckBoxChecked`, `CheckBoxPartial` (Checkbox Indeterminate, Success) | Fluent | UI-9b-3 |
| Auto notes trees (ImageList) | folder, document | `ImageSelectorFolder`, `ImageSelectorDoc` | Fluent | UI-9b-3 |
| Email inbox: Compose, Refresh, Reply, Search, Reply All, Forward (button pictures) and the two mailboxes (unused list) | round teal badges (`Email_*.png`) | `Edit`, `Sync`, `Reply`, `Search`, `ReplyAll`, `Forward`, `Inbox`, `Send` | Fluent | UI-9b-3 |
| Setup wizard: information pictures (grid column and labels), Skip, Next, Back | blue and teal i circles, chevron pictures | `Info`, `InfoSelected` (Filled, Accent), `ChevronDoubleRight`, `ChevronRight`, `ChevronLeft` | Fluent | UI-9b-3 |
| Toolbars of the remaining forms: Accounting, Journal, Claims (Preview, Blank, Print, Send E-Claims, Get Reports, Labels), Web pages (Back, Forward), Treatment plan signing, Print preview and the report windows (Print, Back, Forward, Export, PDF, Zoom) (ImageList) | the windows' `.resx` lists (`Add.gif`, `butExport.gif`, `arrowLeft22.gif`, `print.gif`, ...) | `Add`, `Edit`, `Export`, `Print`, `Preview`, `NewDocument`, `Complete`, `Send`, `Label`, `ArrowLeft`, `ArrowRight`, `Pdf`, `ZoomIn`, `ZoomOut` and the new `DocumentText` (Fluent Document Text) | Fluent | UI-9b-4 |
| Email and wiki editors' toolbars and the wiki (ImageList) | `refresh.gif`, `link.gif`, `h1.gif`, `bold.gif`, `home.gif`, `history.gif`, `setupGears.gif`, the padlocks, ... | the new `Link`, `LinkExternal`, `Heading1` to `Heading3`, `Bold`, `Italic`, `TextColor`, `TextFont`, `SaveDraft` (Fluent Save Edit), `Settings`, `LockOpen`, `LockClosed`, `Home`, `Rename`, `History`, `Archive` and the reused `Sync`, `Save`, `Cut`, `Copy`, `Paste`, `Undo`, `Table`, `Image`, `Import`, `Search`, `ListBullets` | Fluent | UI-9b-4 |
| Record audio: record and stop (button pictures) | `record.gif`, `stop.gif` | `Record`, the new `Stop` (Fluent Stop) | Fluent | UI-9b-4 |
| Info buttons in grid columns (CDS, lab orders, medical, SNOMED) and the lists no control showed (calendar arrows, permission pictures, account pick) | `iButton_16px.png`, `arrowDownTriangle.gif`, ... | `Info`, `ChevronDown`, `ChevronUp`, `ImageSelectorFolder`, `CheckBoxEmpty`, `CheckBoxChecked`, `CheckBoxPartial`, `Add`, `Edit` | Fluent | UI-9b-4 |
| Dashboard tab pictures: add tab, delete tab, delete tab highlighted (ImageList) | `addTab11.png`, `deleteTab11.png`, `deleteTabHighlight11.png` | `Add`, `DeleteX` (the highlighted one with a ring), through CodeBase's `UiIcons` | Fluent | UI-9b-4 |

The window pictures behind these rows, and the pictures that stay, are in `icons/picture-map.tsv` (`tools/Convert-PictureUses.ps1`,
`docs/port-notes/UI-9a.md`); `icons/picture-converted.tsv` lists every converted control. The toolbars' ImageList pictures (UI-9b) have
one row per picture index (`<window>:<list>.ImageStream#<index>`); the list is built by `IconLibrary.GetImageList` in the same order
(`ToolBarOD.SetImageListIcons`, `docs/port-notes/UI-9b.md`).

## Drawing an `EnumIcons` icon

The callers keep the `IconLibrary` calls (UI-7b changed no caller). Both copies draw the rows above from the table.

```csharp
IconLibrary.Draw(g,EnumIcons.Add,rectangle,colorGradientTop,colorGradientBottom);   //WinForms: Direct2D into a bitmap over the gradient; GDI+ with OnlyGDI
IconLibrary.Draw(direct2d,EnumIcons.Add,rectangle);                                  //WinForms, between BeginDraw and EndDraw
IconLibrary.DrawDisabled(g,EnumIcons.Add,rectangle);                                 //TextDisabled
IconLibrary.DrawColored(g,EnumIcons.Appt32,rectangle,colorAccent,colorBack,isFilled:true); //a colour of the caller's, the Filled style (UI-6)
BitmapImage bitmapImage=IconLibrary.DrawWpf(EnumIcons.Add);                          //WinForms helpers that fill WPF controls: a PNG of Width(icon) px
IconLibrary.DrawWpf(EnumIcons.Add,grid);                                             //WPF: a Path of grid.Width DIPs at the grid's DPI
IconLibrary.DrawWpf(EnumIcons.Appt32,grid,brush,isFilled:true);                      //WPF, a brush of the caller's, Filled
```
