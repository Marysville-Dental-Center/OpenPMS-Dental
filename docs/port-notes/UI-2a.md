# Port note: UI-2a (the font plumbing)

Package UI-2a of the UI refresh ([docs/specs/ui-refresh-plan.md](../specs/ui-refresh-plan.md), section 7's UI-2a rows, sections 3.4, 5.1,
5.2, 5.4 and 9). UI-2a changes nothing on screen: every font token still holds today's font and the mapping is the identity until UI-2b.
It is cut into slices; each slice adds its own section below.

## UI-2a-1: the tokens and the mapping

Branch `ui/ui2a1-1` from `main` at `3809f35db`. No schema change, no new package, nothing downloaded, no program started, no window shown,
no database used (the tests run behind UI-0b's no-database guard).

### 1. Files

| File | What |
|---|---|
| `CodeBase/UI/UiTheme.Fonts.cs` | the role tokens, `UiFontOption` (Today, UI2b, Compact), the mapping, `SetFontForTest` |
| `OpenPmsDental/UI/BordersDpi/FormODBase.cs`, `WpfControlsOD/Frame/FormFrame.cs` | the window's font is a copy of `FontBody`; the title fonts take `FontFamilyName` at the size the frame computes |
| `OpenPmsDental/UI/BordersDpi/LayoutManagerForms.cs` | `FontInitial` is `FontBody`; the mapping in the constructor, `AddToList96` and `AddToList96Unscaled`; the content types; `MarkContent` and `IsContent`. The rebuild (`LayoutControlBoundsAndFonts`) is not changed |
| `FormSheetFillEdit.cs`, `FormSheetDefEdit.cs`, `FormClaimFormEdit.cs` | `LayoutManagerForms.MarkContent(panelMain)` (`panel1` in the claim form editor) between `InitializeComponent` and `InitializeLayoutManager` |
| `SparksToothChart/ToothChart2D.cs`, `SparksToothChart/ToothChartWrapper.cs` | the tooth chart's pin (section 3) |
| `OpenPmsDental/User Controls/ContrPerio.cs`, `OpenPmsDental/User Controls/Dashboard/UserControlDashboard.cs` | the perio chart's and the patient dashboard's pins |
| `tests/UiRefreshTests/FontTokenTests.cs` | the tokens, the rule, the three recording places, content and pins (section 5) |
| `tests/UiRefreshTests/Scan/TierOneScan.cs`, `BuilderAndScanTests.cs` | the scan's font options go through the token override |
| `tests/WindowChecksTests/InProc/InProcChecks.UF.cs`, `UTests.cs` | the window check UF1 (the signature test), group U's new part `UF` |
| Section 9's font group | `NoHqTests/DefaultFontTests.cs`, `tests/LibWebTests/HtmlGenerationTests.cs`, `tests/ToothChartImageTests/WpfLiveViewTests.cs` and `Support/TestAssemblySetup.cs`, `tests/ToothChartHostTests/HostDb.cs`, `UnitTests/Misc/TestBase.cs`, `tests/WindowChecksTests/InProc/InProcChecks.cs`, `InProcChecks.W5.cs`, `CTests.cs`, `tests/M2TERMTests/TerminologyLayoutTests.cs`, `tests/M2INSPLANTests/LayoutDesignerTests.cs` |

`UiTheme.Fonts.cs` uses only `System` and `System.Drawing` in C# 7.3, like the other WinForms parts of `UiTheme`, so RdlDesign can link it
(UI-6); `NoHqTests/UiThemeScanTests.cs` lists it among the linkable files.

### 2. The tokens, the UI-2b values and the override

| Token | Today (UI-2a) | UI-2b (`UiFontOption.UI2b`) |
|---|---|---|
| `FontBody` | Microsoft Sans Serif 8.25 | Segoe UI 9 (UQ-1) |
| `FontGrid`, `FontGridBold`, `FontGridUnderline`, `FontGridUnderlineBold` | sans serif 8.5 (`FontFamily.GenericSansSerif`), Regular, Bold, Underline, Underline Bold | Segoe UI 9 (UQ-2: the body size) |
| `FontButtonPanel` | sans serif 8.5 | Segoe UI 9 |
| `FontTitle` | sans serif 10 Bold | Segoe UI Semibold 10 |
| `FontModuleCaption` | Arial 8 | Segoe UI 8 |
| `FontFamilyName` (window titles; the frame computes the size) | Microsoft Sans Serif | Segoe UI |

`UiFontOption.Compact` is UQ-1's other option (as UI2b at 8.25 pt) for the galleries and the scan. The tokens are shared instances that nobody
disposes; `FormODBase` and `FormFrame` take a clone each, as each window had its own font before. `LayoutManagerForms.FontInitial` became a
property that returns `FontBody` (one shared instance, as the field was), so the 54 files whose UserControls set `Font=LayoutManagerForms.FontInitial`
follow the token without an edit.

`UiTheme.SetFontForTest(option)` sets every token to an option's values at once (one reference swap, so a paint on another thread never sees a
mix); `null` gives today's back. It throws unless the process is a test process (the MSTest framework is loaded, or the entry assembly is a
test host, `TestProbeHost` or a name ending in `Tests`). It does not set the default font: each exe's `Main` does that, and a test sets it as
UI-0b's `DefaultFontOverride` does.

**The 12 exe `Main`s** keep their literal `new System.Drawing.Font("Microsoft Sans Serif",8.25f)` as the first statement (no file changed):
`DefaultFontTests.EveryExesMain_SetsTheDefaultFontFirst_ThenTheDpiMode_BeforeAnyWindow` builds the expected literal from `FontBody`
(`new System.Drawing.Font("<family>",<size>f)`), so UI-2b changes the literals and the token together and the test stays as it is; the probe
test compares the plain form's font with `FontBody` times the text scale factor.

### 3. The mapping, the content rules and the pins

- **The rule** (`UiTheme.MapFont`, 3.4): a Microsoft Sans Serif font becomes `FontBody`'s family, at its size for 8, 8.25 and 8.5 pt (a
  0.01 pt tolerance, for sizes that went through a zoom scale and back) and at its own size otherwise, with its full style; other families
  come back as the same instance. While `FontBody` is Microsoft Sans Serif (`IsFontMappingIdentity`) every font comes back as the same
  instance, so the 8 designer fonts at 8 pt and the 2 at 8.5 pt keep their size until the flip. It is the same rule as UI-0b's
  `TextFitFontSwap.MapFont` (a test compares them).
- **Where it runs.** All three places a `Control96Info` is made, before `FontSize96` is read:
  - the constructor (in `InitializeLayoutManager`): `MapFontsOfChildren(formODBase)` walks the window from the top down before the loop
    that records the controls. `UIHelper.GetAllControls` lists a child before its parent, so mapping inside that loop would record an
    inheriting child at its parent's unmapped size; walking from the top first, a child without a font of its own simply inherits its
    parent's mapped font and records it;
  - `AddToList96` (for `Add`, `Synch` and the tooth chart's engine control) and `AddToList96Unscaled` (for `AddUnscaled`): the control
    itself before its record, its children after (recursively, so an inheriting child follows). `Add` expects a font scaled by the zoom
    (or fully, for the text box, combo box, rich text box, tree view, list view and list box, as `AddToList96` and `LayoutChildren` scale
    them), so the rule is applied to the 96 dpi size and the result scaled back the same way; `AddUnscaled`'s fonts are at 96 dpi.
  - Only a control with a font of its own is mapped (its `Font` is not the parent's instance, or, without a parent, not
    `Control.DefaultFont`), so no inheriting control is given a font it did not have.
- **The rebuild** (`LayoutControlBoundsAndFonts`) is not changed: it keeps the control's family, sets a font only when the size differs and
  keeps only Bold. So the underlined labels of `FormInsBenefits` draw without underline today and with the override, as in Open Dental (a test
  checks both). The mapping runs in `InitializeLayoutManager`, before `Load` and before any signature is filled, so its `TextChanged` clears
  nothing (Job 55866).
- **Content** (`LayoutManagerForms.IsContent(control,parent)`): the control itself, then `parent` and its parents, is one of the content
  types (`ToothChartWrapper`, `ContrPerio`, `ODPrintPreviewControl`, `UserControlDashboard`) or a panel marked with `MarkContent`. The
  constructor's walk passes the control's own parent; `Add` and `AddUnscaled` pass their `parent` argument, because they record a control
  before it is parented (the sheet fields at `FormSheetFillEdit.cs:1438`). For the same reason `AddToList96` and `AddToList96Unscaled` make
  the decision once, at the control passed to `Add`, and pass it down to its children (`isInContent`): a walk from a child would go up only
  to that still unparented control and miss the content it is being added to (review fix, `ui/ui2a1-2`).
- **`MarkContent(control)`** records the panel in a weak table. It throws `InvalidOperationException` when a `FormODBase` above the control
  (walking up from it) has already built its layout: the `LayoutManagerForms` constructor records each window it builds, so a mark after
  `InitializeLayoutManager` fails the first time a test opens the window. A control not yet in a window can be marked.
- **The marks**: `FormSheetFillEdit.panelMain`, `FormSheetDefEdit.panelMain`, `FormClaimFormEdit.panel1` (UI-0b's
  `TextFitFontSwap.ArrayContentPanels`; a source test checks that each window marks it between `InitializeComponent` and
  `InitializeLayoutManager`).
- **The pins** (each is today's font, so nothing changes now; from UI-2b on the default font cannot reach the content):
  - `ContrPerio`: `Font` = Microsoft Sans Serif 8.25 in its constructor. `FormPerio` records it at 8.25, as the inherited font was, and
    zooms it; `FormClaimEdit`'s own copy for the claim attachment draws with it.
  - `UserControlDashboard`: Microsoft Sans Serif 8.25 after `InitializeComponent`, before `FormOpenDental`'s `LayoutManager.Add`, which
    records it at 8.25 (the window is not zoomed yet in its constructor) and zooms it; the widgets and their fields without a font of their
    own inherit it.
  - **The tooth chart: `ToothChart2D` and `ToothChartWrapper`.** The plan names `ToothChart2D`'s pin. A fixed 8.25 pt on `ToothChart2D`
    alone would change the 2D chart at a program zoom: LayoutManager never lays the engine control out (`LayoutSkipChildren` returns true for
    the wrapper), and today the chart inherits the wrapper's font, which LayoutManager zooms (8.25 x 1.2 = 9.9 pt at 120%). So the pin is in
    two parts, as the dashboard's is: `ToothChartWrapper` sets Microsoft Sans Serif 8.25 after `InitializeComponent` (recorded and zoomed by
    LayoutManager as the inherited font was; it also covers the wrappers made in code for pictures, `SheetPrinting`, `ControlTreat`,
    `FormSheetDefEdit`, which have no parent and took the default font), and `ToothChart2D` keeps Microsoft Sans Serif at its parent's size
    and style (`OnParentChanged`, `OnParentFontChanged`), or 8.25 on its own (the fallback picture of `GetSimple2DBitmap`). It sets a font
    only when the one it has differs, so inside today's wrapper it sets none. `ToothChartData.cs:87`, `ToothChartWrapper.cs:431` (Arial 9)
    and the WPF chart's Arial are not touched.
- **Not in this slice** (other UI-2a slices own them): `AutoScaleMode` (UI-2a-2, including `ToothChart2D` and `ToothChartWrapper`'s), the
  print fonts and GridOD's fonts (UI-2a-3), the code font sites and the plain forms' designer fonts (UI-2a-4 to UI-2a-n). The program's
  code sites still name their fonts; none reads a token yet except the ones above.

### 4. Controls added after `Load`

A grep of `LayoutManager.Add`, `AddUnscaled` and `Synch` outside `InitializeLayoutManager`: they all go through `AddToList96` or
`AddToList96Unscaled`, so the mapping reaches them. With today's tokens it changes nothing anywhere. After the flip: `FormSheetFillEdit`'s
field text boxes, signature boxes, rich text boxes and the screen tooth chart are added to the marked `panelMain` (content: their stored
fonts stay, and so do the fonts of their children, such as the screen tooth chart's ten Microsoft Sans Serif 8.25 Bold tooth labels and the
signature box's `labelInvalidSig`, because the content decision made at the added control is passed down to its children); the setup wizard's and the preference pages' controls (`AddUnscaled`) and the module controls take the token by the rule, set
before their handlers can matter (the mapping runs only for a control with a Microsoft Sans Serif font of its own, before it is parented).

### 5. Tests

`tests/UiRefreshTests/FontTokenTests.cs` (21 tests, no database, no window):
- the tokens hold today's fonts (and `FontInitial` is `FontBody`); the UI-2b values are written down; `SetFontForTest` sets every token and
  `null` gives today's back; it refuses outside a test process (a source check of its first statement); `FormODBase` takes the token;
- the rule: every distinct Microsoft Sans Serif designer font of the program's designer files (sizes and styles, the 8 and 8.5 pt ones among
  them) comes back unchanged with today's tokens; with the override set to Segoe UI the rule of 3.4, the same as UI-0b's swap;
- the three recording places with the override set to Segoe UI: a test window's designer fonts (and an inheriting child, the recorded
  `FontSize96`, a marked panel, the unchanged rebuild with its 0.96 factor and Bold only); an `Add`ed control at zoom 120% (the mapped 96 dpi
  size, rescaled) and one added to the marked panel; the preference page `UserControlAccountRecAndRepCharges` (5 explicit designer fonts,
  through UI-0b's builder and `AddUnscaled`) and `ControlImages` (4, made with its own constructor, as UI-0b's builder cannot, and added with
  `AddUnscaled`); with today's tokens the designer fonts are unchanged;
- the underlined labels of `FormInsBenefits` draw without underline, in today's font and with the override;
- content: `MarkContent` after `InitializeLayoutManager` throws (also for a nested control), the three windows mark their panels, the content
  types; a Microsoft Sans Serif sheet field added to `FormSheetFillEdit`'s `panelMain` keeps its font with the override (the same field outside
  the panel follows the token); the children of a composite field added there keep theirs too (a `ScreenToothChart`'s tooth labels at
  Microsoft Sans Serif 8.25 Bold, and a panel holding a Microsoft Sans Serif label; the same panel outside follows the token); a perio chart picture (`ContrPerio` on a synthetic exam, the preferences in the cache only) is byte for byte
  the same with the override and a Segoe UI default font, on its own and in a Segoe UI window; the patient dashboard with a widget holding a
  label field with a font name, one without, and a `DashIndividualDiscount` field keeps every field's font with the override, at 100% and at
  zoom 120%; the tooth chart keeps Microsoft Sans Serif 8.25 with a Segoe UI default font and the override, on its own, in a wrapper, and
  (zoomed to 9.9 pt) in a Segoe UI window at 120%.

The section 9 font group: the tests that pinned "Microsoft Sans Serif" compare with the token, so the flip changes no test
(`DefaultFontTests`, `HtmlGenerationTests`, `WpfLiveViewTests`, `TestBase`, `InProcChecks.InitializeAsProgramEntry`, `ExpectDefaultFont` (C16,
C17, C21, F2, F4, G5), C20 (the menu strip and its items equal the form's font, which starts with the token's family), `TerminologyLayoutTests`,
`LayoutDesignerTests`). `ToothChartImageTests`' and `ToothChartHostTests`' test default font is Segoe UI 9 already, so the 35 Simple 2D
baselines prove the chart's pin before the flip.

**Window check UF1** (the signature test; it needs a shown window, so it is an opt-in window check, not a UiRefreshTests test):
`InProcChecks.UF.cs`, `UTests.UF1_SignedNote_StaysValid_AfterZoomResizeAndSave`, group U's part `UF`. In the in-process session on the scratch
copy it signs a synthetic note on the patient's D1110 with the program's own signature box (a `procnote` row), opens `FormProcEdit` by the
progress notes' double-click, checks the signature shows and is valid, sets the program zoom to 120% (`ComputerPrefs.LocalComputer.Zoom`, which
every layout reads) and resizes the window, checks again, puts zoom and size back, saves with OK, and checks that the saved note and signature
are the signed ones and the signature is valid for the note; the rows it added are deleted. `FormProcEdit` is the window the plan names; the
program opens group notes in `FormProcGroup`, so the note is the procedure's own signed note.

### 6. Runs

- Targeted (x64, `-RequireBuildOutput`): UiRefreshTests 63 of 63 with `OPMS_UI_SCAN=1` (the full scan through the override included) and
  NoHqTests 122 of 122 (`.git/od-tools/test-results/ui2a1-3.txt`); WindowChecksTests 26 passed, 41 opt-in skipped (UF1 among them),
  LibWebTests 65, M2TERMTests 21, M2INSPLANTests 31 (`ui2a1-4.txt`).
- Review fix 1 (`ui/ui2a1-2`, the content decision passed down): full build 0 errors; one Targeted run with `OPMS_UI_SCAN=1`
  (`ui2a1-5.txt`): UiRefreshTests 64 of 64 (the new composite-field test included), NoHqTests 122, WindowChecksTests 26 passed, 41 opt-in
  skipped, LibWebTests 65, M2TERMTests 21, M2INSPLANTests 31. The scan's summary through the override is the same as in section 7 (the scan
  adds no sheet fields; `.git/od-tools/test-results/ui2a1-2-scan/`).
- **Not run here, for the lead's connected session:** ToothChartImageTests and ToothChartHostTests (WPF picture tests; the owner's session
  was disconnected), the window checks with UF1 (`OPMS_WINDOW_CHECKS=1`, `OPMS_WINDOW_CHECKS_GROUPS=UF` or `U`), and the harness's U0 and the
  bounds record against the before set: **the before set does not exist yet** (UI-0c's first connected run makes it), so the slice's merge
  rule (U0 shows no changed picture, the bounds record equals the before set) can be checked only then.
- **No visible change, as far as this session can prove it:** UI-0b's scan on today's font reports nothing new
  (`Scan_TodaysFont_ReportsNothingNew`: no new overflow, self-sizing overlap or bounds difference over 1,001 types at 100% and 150%), and the
  layout snapshot is unchanged (`Snapshot_MatchesTheDesignerFiles`).

### 7. The scan's report for Segoe UI through the override

`Scan_FontOptions_WritesTheCounts` with `OPMS_UI_SCAN=1` now sets `UiTheme.SetFontForTest` before each window is built (the product's own
path: `FormODBase`'s font, the mapping), instead of UI-0b's swap after the build (`OPMS_UI_SCAN_SWAP=1` runs the swap, into `options-swap`).
The files are in `.git/od-tools/test-results/ui2a1-scan/options/` (`summary.txt` and the TSVs). 96 DPI, 0.8 minutes; 1,001 of 1,026 types
built (the same 3 need a database, 7 have no designer code, 15 throw, as in UI-0b).

| At 100% | Segoe UI 9, override | Segoe UI 9, UI-0b's swap | Segoe UI 8.25, override |
|---|---|---|---|
| New overflows | 1,775 in 450 windows | 1,771 in 449 | 637 in 245 |
| ... fixable / need the owner | 1,348 / 427 | 1,344 / 427 | 508 / 129 |
| ... 1-2 px height cases: ink cut / inside | 160 / 796 | 160 / 783 | 104 / 232 |
| Self-sizing overlaps and edge passes | 866 in 152 windows | 856 in 149 | 593 in 112 |
| Bounds differences | 665 in 64 windows | 692 in 86 | 67 in 24 |
| ... of LayoutManager windows | 56 in 20 | 76 in 40 | 49 in 17 |

The two mechanisms agree within a few controls. The differences are expected: the override reaches what the product reaches (the window's
own font, the designer fonts LayoutManager records), while the swap also set every container's font after the build; the
font-driven bounds differences of the plain forms (the `AutoScaleMode.Font` containers, UI-2a-2) are the same in both (458 in 27 windows).
At the other scales, Segoe UI 9: 1,651 (1.2), 1,470 (1.25), 2,504 (1.5), 2,392 (1.75), 1,826 (2) new overflows; the text-scale run as in
UI-0b (today 257, Segoe UI 9 299, Segoe UI 8.25 290 new overflows in about 50 windows).

## UF1: the signature check and a scale change

Branch `fix/uf1-signature` from `main` at `7d99983ff`. The owner's connected run of UF1 (2026-11-17 18:40, `.git/od-tools/test-results/ui2a1-wc-uf/`)
failed all five signature items, the first one ("shows as valid when the window opens") included, and the saved row had no signature.
Two causes:

1. **The check signed with the wrong key data.** The key data of a procedure note is the note followed by `Procedure.UserNum`
   (`Procedures.GetSignatureKeyData`, `Procedures.cs:5674`); `UserNum` is not a database column (`Procedure.cs:161`) and comes from the newest
   `procnote` row (`Procedures.GetOneProc`, `Procedures.cs:369`). The synthetic dump's D1110 has no note, so the check signed with
   `<note>0` and inserted the row with `Security.CurUser.UserNum`; the window, opened from the progress notes (`ControlChart.cs:2189`, a fresh
   `GetOneProc`), checked it with `<note><user>`: "Invalid Signature", no points. Fixed in `InProcChecks.UF.cs`: the check sets the
   procedure's `UserNum` to the row's user before it signs, as `FormProcEdit.signatureBoxWrapper_SignatureChanged` does before `SaveSignature`.
2. **A scale change while the window is open clears the signature (Open Dental's behaviour; owner decision).** The zoom step sets the note
   box's font to its new size (`LayoutManagerForms.LayoutControlBoundsAndFonts`, `LayoutManagerForms.cs:1031`: a font is set only when the
   size differs, Job 55866). A rich text box raises `TextChanged` for every font it is given; `FormProcEdit.textNotes_TextChanged`
   (`FormProcEdit.cs:1886`) takes that for an edit of the note after start-up and calls `signatureBoxWrapper.ClearSignature()`, which sets
   `_signatureChanged`; Save then writes a new `procnote` row with the same note and no signature (`SaveSignature`, `FormProcEdit.cs:2646`,
   reads 0 points as ""; `ProcNotes.IsNoteRowNeeded`). The signature box's own rescale clears nothing. In the program the reachable path is a
   change of the Windows scale while the window is open (moved to a screen with another scale, or the display's scale changed:
   `FormODBase` `WM_DPICHANGED`, `FormODBase.cs:1394`); the program's zoom cannot change while the modal window is open. A resize at the same
   scale and a plain open and save keep the signature. The same code is in Open Dental 24.3.1 (`be14fe1be`: `textNotes_TextChanged`, the
   rebuild's same-size guard, `OnLoad`'s layout before `Load`, `WM_DPICHANGED`), and `FormProcGroup.textNotes_TextChanged` has the same rule
   for group notes, so it is not fork-only and not caused by UI-2a-1. **The product is not changed** (upstream behaviour): whether a font
   change should stop clearing the signature (for example by comparing the note's text before clearing) is for the owner.

What changed:

- `tests/WindowChecksTests/InProc/InProcChecks.UF.cs`: the key data fix; a failed signature item now says what the box shows (valid or
  invalid, points, the window's `_signatureChanged`, whether the note changed), and a saved row without a signature says so; the shared
  body runs UF1 (zoom change, resize, save) and the new **UF2** (the same signed note, opened and saved with no zoom change and no resize).
  `UTests.UF2_SignedNote_StaysValid_AfterOpenAndSave`, group U's part `UF`.
- `tests/UiRefreshTests/SignatureFontChangeTests.cs` (no database, no window shown; `FormProcEdit` built as the scan builds it, its handles
  made, the signed note filled as `FillControlsOnStartup` does): a resize at the same scale keeps the signature and `SaveSignature` keeps it;
  a Windows scale of 150% and a program zoom of 120% each set the note box's font, raise `TextChanged`, clear the signature and make the save
  store none (pinned as Open Dental's behaviour, the owner decision); without `textNotes_TextChanged` the same scale change keeps it.

**The owner's answer (2026-11-17): fix it.** `FormProcEdit` and `FormProcGroup` keep the note's last text (`_textNotesLast`, set when the
window has filled) and clear the signatures in `textNotes_TextChanged` only when the text differs, so a font set alone (a change of Windows
scale or zoom) keeps them and any edit still clears them as before. `SignatureFontChangeTests` now checks that a scale change keeps the
signature and its save, that an edit still clears it, and that a later scale change does not bring it back; UF1 and UF2 must both pass.

Runs (x64): the build lane, 0 errors. Targeted with `-RequireBuildOutput` (`.git/od-tools/test-results/uf1-fix-targeted-1.txt`):
UiRefreshTests 80 passed, 1 opt-in skipped (both new tests passed), WindowChecksTests 27 passed, 42 opt-in skipped (UF1 and UF2 among them),
NoHqTests 122. The window checks with `OPMS_WINDOW_CHECKS_GROUPS=UF` in the owner's connected session (`uf1-fix-1.txt`,
`.git/od-tools/test-results/uf1-fix-1/`, scratch copy `winchk_1117185709`, dropped): **UF2 PASSED** (the signed note shows as valid, is saved
unchanged and stays valid); **UF1 FAILED at the zoom step only**: the signed note shows as valid when the window opens; after the zoom
(the note box at Microsoft Sans Serif 9.9) the box shows no signature, the window's `_signatureChanged` is True and the note is unchanged;
the saved row has the same note and no signature. U0 PASSED (three pictures, not approved yet).

## UI-2a-3: print fonts

Branch `ui/ui2a3-1` from `main` at `deb903107`. No schema change, no new package, nothing downloaded, no program started, no window shown.
The print guard's database tests use one scratch database `ui2a3_<run id>_main` on the local MariaDB (the committed
`db/init/new-database.sql` and the fork migrations, synthetic rows only), dropped at the end of the run. Nothing changes on screen: the
font tokens hold today's fonts, so GridOD's screen fonts and print fonts are the same fonts until UI-2b.

### 1. Files

- `OpenPmsDentalBusiness/UI/GridOD.cs`: the split (section 2).
- `OpenPmsDentalBusiness/UI/GridPrintLook.cs` (new): today's colours of a printed or sheet grid (section 2, colours).
- `OpenPmsDentalBusiness/UI/GridOld.cs`: the same split; GridOld prints (section 3).
- `OpenPmsDental/Forms/FormSheetDefEdit.cs`: the two canvas grids (`CreateDynamicGrid`, `CreateGrid`) are marked `IsForDocument`.
- `OpenPmsDental/Forms/FormClaimEdit.cs`: the Perio Chart attachment picture is made by `public static GetPerioAttachmentBitmap(Patient,
  LayoutManagerForms)`, the same drawing moved out of `butAttachPerioHelper`, so the guard compares the program's own picture.
- `OpenPmsDental/Forms/FormPerio.cs`: the perio chart printout, the same way. `butPrint_Click`'s setup (96 dpi, Microsoft Sans Serif 8.25,
  the printed size; one of plan 5.2's print sites that must stay) is `public static SetUpForPrint(ContrPerio,LayoutManagerForms)`, and
  `pd2_PrintPage`'s drawing (the chart, its four letters, the title, clinic, patient, date, index and red-threshold lines) is `public static
  DrawPrintPage(Graphics,ContrPerio,Patient,` the four index colours, the six red thresholds, `DateTime)`. `butPrint_Click` and
  `pd2_PrintPage` call them, the page is drawn as before, and the guard draws the whole page through them, so a UI-2a-4 slice that sends
  the 8.25 font or a print font to a token fails the guard.
- `tests/PrintGuardTests` (new, wired into `OpenDental.sln` and `OpenDental.Local.slnf`): the print guard (section 4).

### 2. GridOD: screen fonts and print fonts

- **Screen fonts** (`SetFonts`): `_fontCell`, `_fontCellBold`, `_fontUnderline`, `_fontUnderlineBold` take `FontGrid` and its Bold,
  Underline and Underline Bold tokens, `_fontHeader` takes `FontGridBold`, `_fontTitle` takes `FontTitle`, each at the grid's zoom as before
  (`NewScreenFont`: the token's family, size and style). With today's tokens these are the fonts of before: sans serif 8.5 and 10 Bold.
- **Print fonts** (`_fontPrintCell`, `_fontPrintCellBold`, `_fontPrintHeader`, `_fontPrintTitle`): made in `SetFonts` beside the screen
  fonts with today's literals (`FontFamily.GenericSansSerif` 8.5, 8.5 Bold, 8.5 Bold, 10 Bold) at the same zoom, so they are exactly the
  fonts every print path used before. `PrintPage` prints at 100% (`SetScaleAndZoom(1,1)`), as before.
- **Who uses them**: `PrintPage` (its body is now `PrintPageHelper`; `PrintPage` sets `_isPrinting` around it); the sheet methods
  `SheetDrawRow`, `SheetDrawRowX`, `SheetDrawTitle`, `SheetDrawTitleX`, `SheetDrawHeader`, `SheetDrawHeaderX`; and the row heights
  (`ComputeRows`, `ComputeRowHeightOne`) of a grid that only makes a document: `IsForSheets` (SheetDrawingJob, SheetUtil), the new
  `IsForDocument` (the sheet design canvas of `FormSheetDefEdit`, whose rows are drawn by `SheetDrawRow`), and any grid while `PrintPage`
  runs. A sheet field's own font (`FontForSheets`) still wins over both, as before.
- **Header height**: `_heightHeader` grows to fit multiline headers measured in the header font (`LayoutScrolls`); a taller screen font would
  have made printed headers taller. `_heightHeaderPrint` follows the same rule with the print header font, and `PrintPage`, the sheet header
  methods and the sheet layout (`ComputeSheetSupplemental`) use it.
- **A grid printed again at 100%** keeps the row heights the end of the last printout measured on screen (before the split: with the screen
  fonts, which were the print fonts). `PrintPageHelper` measures them again on screen with the print fonts only when the screen fonts are
  not the print fonts (`AreScreenFontsThePrintFonts`), so with today's tokens a second printout is what it always was. The 100% printout of a
  grid that was refilled with 2,000 rows or more between two printouts is the one case where the measured heights could differ from today's
  estimated ones, and only after UI-2b.
- **India**: `DrawRows` switches the cell fonts to Arial in an `IN` culture. Printouts used the screen fonts, so a grid shown on screen printed
  in Arial; the print cell fonts switch with them. UI-5 decides whether the switch stays (section 5.1).
- **Colours (UQ-5, plan 5.2)**: `PrintPage` and the sheet methods draw their gridlines, column separators, outline, title gradient and title
  text with `GridPrintLook`, today's values, not with GridOD's screen colours, so the grid restyle (UI-5) cannot reach a printout. The new
  file sits outside the shared controls' colour scan (`NoHqTests` `UiThemeScanTests` reads `GridOD.cs`), so its literals are not new rows in
  `tests/UiRefreshTests/Data/colour-literals.tsv`, which only shrinks; `GridOD.cs`'s own literal counts are unchanged. The other colours the
  sheet methods name in place (`Color.FromArgb(102,102,122)`, `(213,213,223)`, `Brushes.LightGray`, `Pens.Black`) are already print-only.
- **Unchanged**: the screen paint, `textEdit`'s font, the screen header height, `_heightTitle`, the exception text's GenericSansSerif 16 Bold,
  and every font a print path does not take from the grid (sheet field fonts, the callers' own heading fonts around `PrintPage`).

### 3. GridOld and ContrTable

- **GridOld prints**: the Chart module's progress notes (`ControlChart.cs:1692, 1791`, `gridProg.PrintPage`), and it has the sheet methods
  (no caller: every sheet grid is a GridOD). It gets the same split: screen fonts from the tokens in `LayoutScrollsAndFonts`, print fonts with
  today's literals beside them, `_heightHeaderPrint`, `_isPrinting` around `PrintPage`, the second-printout rule, the India switch. Its
  colours are not GridPrintLook's: GridOld's screen colours are not restyled by any package (UI-5 names GridOD and the WPF grid).
- **ContrTable does not print**: `Tables Old Grids/ContrTable.cs` only paints on screen (`OnPaint`, `CreateGraphics`), so it is not
  changed. Its two literal fonts (Microsoft Sans Serif 8 and 8.25 Bold, lines 97-98) are screen code sites for the UI-2a-4 slice that owns
  `Tables Old Grids`.

### 4. The print guard (`tests/PrintGuardTests`)

Each test renders off screen three times on its own STA thread: with today's tokens and today's default font (Microsoft Sans Serif 8.25, as
each exe's `Main` sets it), with UI-2b's (`UiTheme.SetFontForTest(UiFontOption.UI2b)` and a Segoe UI 9 default font), and with today's
again. The first and third must be the same bytes (the render is repeatable, so a difference means something), and the second must equal
the first. Bitmaps are compared pixel by pixel (32-bit ARGB, with the size); PDFs page by page (size, the fonts' names without the subset
tag, and the content stream; the file's dates and IDs are left out). `PRINT_GUARD_RECORD_DIR=<folder>` writes each output's SHA-256 there, to
compare two builds; outputs that differ are written to that folder, or to `%TEMP%\printguard`.

| Test | What it renders |
|---|---|
| `GridOD_PrintPage_IsTheSameWithUi2bFonts` | GridOD's printout of 40 synthetic rows (wrapped text, bold rows and cells, a coloured cell, notes, a right-aligned column, a two-line header), every page, then the same grid printed again |
| `GridOD_PrintPage_AtZoom120_...` | the same at a program zoom of 120% (`PrintPage` prints at 100% and puts the zoom back), header space on every page |
| `GridOD_SheetOutput_...` | an `IsForSheets` grid over two sheet pages, drawn as `SheetDrawingJob.DrawFieldGridHelper` draws it, into a bitmap and into a PDF |
| `GridOD_SheetCanvasGrid_...` | the sheet design canvas's grid (`IsForDocument`, one dummy row): title, header, row and the row's height |
| `GridOD_ScreenFonts_FollowTheTokens_PrintFontsStay` | the positive control: GridOD's screen picture differs with UI-2b's tokens, its screen fonts follow the tokens (Segoe UI 9, Segoe UI Semibold 10) and its print fonts stay sans serif 8.5 and 10 Bold |
| `GridOld_PrintPage_...`, `GridOld_SheetOutput_...` | GridOld's printout (twice) and its sheet methods (bitmap; PDF without the title, whose gradient brush PDFsharp 6 does not draw and no caller uses) |
| `GridOld_ScreenFonts_FollowTheTokens_PrintFontsStay` | the positive control for GridOld |
| `Sheet_PdfAndPrintedPage_...` | a synthetic patient form (default font, Microsoft Sans Serif, Arial Bold, Times New Roman wrapped, right aligned, a rectangle, a line, a check box) through `SheetDrawingJob.CreatePdf` and `SheetPrintingJob.DrawSheetFirstPage` |
| `SheetGrid_BitmapAndPdf_...` | a ProcsWithFee sheet grid through `SheetDrawingJob.DrawFieldGridHelper` (bitmap and PDF), with the sheet's default font and with Arial 9 |
| `ClaimForm_PrintedPages_...` | a synthetic claim on every internal ADA claim form and the DC217, through `FormClaimPrint.pd2_PrintPage` |
| `PerioChart_PrintAndClaimAttachment_...` | the whole printed perio chart page through the program's own `FormPerio.SetUpForPrint` (the chart on an off-screen window, moved by its LayoutManager) and `FormPerio.DrawPrintPage` (the index colours and red thresholds `FormPerio_Load` gives the window, a fixed print date), and `FormClaimEdit.GetPerioAttachmentBitmap` |
| `ReportComplex_PrintedPages_...` | a ReportComplex report (title, subtitles, a 70-row query, page numbers) through `FormReportComplex.pd2_PrintPage` |
| `Rdl_Pdf_...` | an RDL report (text in the default font, Arial, Times New Roman, Tahoma, a line) rendered to PDF by the RDL engine |
| `MigraDoc_Pdf_...` | a MigraDoc document made with `MigraDocHelper` (text, placed text, a grid through `MigraDocHelper.DrawGrid`) |
| `GraphDashboard_ChartPage_...` | the graph dashboard's chart page: `FormPrintImage`'s page bitmap (which it prints and saves as a PDF), with a `GraphQuantityOverTime` chart |
| `EForm_PdfPages_...` | opt-in (`OPMS_PRINT_GUARD_WPF=1`): the internal Patient Registration eForm's pages as Save to Images makes them (WPF rendering) |

The harness steps that stand in for a window (the claim form's `labelTotPages`, `FormReportComplex`'s print document preparation,
`FormPrintImage`'s Load without the dashboard's database graphs) are the program's own steps on the program's objects, named in each test.
UI-2b runs the guard again after the flip, and the UI-2a-4 slices run it (section 7).

### 5. Runs

- The build lane (x64), the whole filter: 0 errors.
- Targeted (x64, `-RequireBuildOutput`, `.git/od-tools/test-results/ui2a3-targeted-1.txt`): PrintGuardTests 16 passed, 1 opt-in skipped
  (the eForm PDF); UiRefreshTests 80 passed, 1 opt-in skipped (the layout snapshot and `Scan_TodaysFont_ReportsNothingNew` among the passed:
  nothing new at today's tokens); UnitTests 1,728 passed, 3 skipped; PdfLibraryTests 41; NoHqTests 122. The scratch database was dropped.
- While the guard was written, one run of `ReportComplex_PrintedPages_...` (before the repeatability check was added) reported a difference
  between today's render and UI-2b's that no later run showed (eight runs since, each with today's render twice); the repeatability check
  now tells such a flake from a print path that takes a screen font.
- After the perio print fix (branch `ui/ui2a3-2`): the build lane (x64), the whole filter, 0 errors; Targeted
  (`.git/od-tools/test-results/ui2a3-targeted-4.txt`) PrintGuardTests 16 passed, 1 opt-in skipped; UiRefreshTests 80 passed, 1 skipped;
  UnitTests 1,728 passed, 3 skipped; PdfLibraryTests 41; NoHqTests 122. A check run with `SetUpForPrint` sending the chart's font to
  `UiTheme.FontBody` (`ui2a3-mutation-1.txt`, reverted) failed `PerioChart_Print` ("A print path takes a screen font"), so the guard now
  catches the UI-2a-4 edit it is meant to catch.

### 6. For the lead

- **The eForm PDF guard** needs WPF rendering (`RenderTargetBitmap`), so it was not run here. In a connected session, with the worktree
  built: set `OPMS_PRINT_GUARD_WPF=1` and run `run-tests.ps1 -Mode Targeted -Platform x64 -RequireBuildOutput -Include PrintGuardTests`;
  `EForm_PdfPages_AreTheSameWithUi2bFonts` must pass (today it only shows that the eForm pages ignore the font tokens; UI-4a and UI-4b's pinned
  `CtrlEFormFill` templates are what keep them after the control restyle).
- **No visible change**: the merge rule for a UI-2a slice (U0 shows no changed picture, the bounds record equals the before set) needs the
  harness's group U in a connected session; GridOD's screen fonts are the fonts of before, so no picture should change.
- **A before-and-after record** across builds, if wanted: run the guard with `PRINT_GUARD_RECORD_DIR` on `main` and on this branch (the
  tests that need this branch's members, the canvas grid and the attachment, are skipped on `main` by hand) and compare the `.sha256` files.
  Here the comparison is within one build: with today's tokens the print paths' fonts and colours are textually the same literals as on
  `main`.
