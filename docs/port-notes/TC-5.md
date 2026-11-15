# Port note: TC-5 (free text, the text paint types and the extraction graphic)

Package TC-5 of the WPF tooth chart track ([docs/specs/wpf-tooth-chart-plan.md](../specs/wpf-tooth-chart-plan.md), the TC-5 row of
section 4; sections 2, 3, 5, 7 and 8 and section 10's owner answers O-20, O-21 and O-22): branch `chart/tc5-1` from `main` (`b8e931cea`),
fix round 1 of the review on `chart/tc5-2`, then the **Open Dental parity round** on `chart/tc5-4` (from `chart/tc5-2`). **Status: done;
waiting for the owner's approval of gallery G2** (section 3, its fourth version), after which it merges. Nothing was downloaded and no
program was started; the tests ran in the lead's desktop session as the chart's other tests do.

**The parity round.** The owner's rule of 2026-11-14, "Overall for tc changes, keep it the same as OD latest version", and the audit of that
day (`.git/od-tools/briefs/tc-od-parity.md` in the main checkout; the plan's section 10, "Open Dental parity (2026-11-14)") replace O-20,
O-21 and O-22 where they differ: the extraction graphic is **on by default** and a plain switch (no implant or pontic exception), the tooth
stays hidden under the X as Open Dental 26.2's screenshot shows; **TextBelow is not in the tooth picture** but in a one-line strip under the
Chart module's date slider; paint text other than "W" is drawn **beyond the root tip, in the watch mark's band**; and the Draw tab's
**Change Color recolours free text** too. The owner's earlier 2026-11-14 idea of showing the extracted tooth under the X (an abandoned
`chart/tc5-3`) is not built: it would differ from Open Dental's screenshot (audit row EXT-3).

Commits: `61f9b4f12` (this note's first part, the setting's business class, TextBelow in the paint type lists), `627d1ddb6` (the drawing,
the text mover, the relay and painter, the Chart preferences page), `35fdc50ac` (a fix the tests found, and free text on the nearest
pixel), `74134ffd4` (tests, scenes, baselines, call logs), `6ce016b89` (parity allowances), `ebda24a76` (this note). Fix round 1:
`fd85e4a59` (the code), `06b17717a` (tests and scenes), `a6df3a9eb` (baselines and parity allowances), then the docs commits. The parity
round: `5c420ccb4` (the code), `55f68ad88` (tests and scenes), then the baselines and call logs and the docs commits.

## 1. What the row asks, and how it is built

### 1.1 Free text (`ToothInitialType.Text`)

- `ToothChartRelay.AddText(text, location, color, toothInitialNum)` passes the label to the new `ToothChartWrapper.AddText`, which keeps it
  in `ToothChartData.ListFreeTexts` (a `ToothChartFreeText` each: the text, its point in the chart's 410x307 drawing space, its colour and the
  `toothinitial` row's number). `ResetTeeth` clears the list, as it clears the drawings. A label added again with the same number replaces
  the first.
- The point is the top left of the label's text cell in the 410x307 space, the space of the drawings (`ToothChartData.SizeOriginalDrawing`),
  scaled to the picture as the drawings are. `butAddText_Click` puts a new label at 0,0, the top left corner.
- The WPF chart draws every label last, over the teeth, the numbers and the drawings, in Arial with a cell height of 14 pixels times the
  pixel scale ratio, in the label's colour (opaque, as the drawings of the same colour), its top left on the nearest whole pixel of its point
  (DirectX drew no free text, so there is no D3DX rule to follow). A label that would run past an edge of the picture is
  drawn inside it (moved left or up by what is too much), so a label is never lost off the picture and can always be picked up again.
- **Move.** With `CursorTool.MoveText` (the Draw tab's "Move Text"), a press on a label picks it up (the top-most one under the mouse), the
  label follows the mouse, and the release raises `ToothChartWrapper.TextMoved` with the label's number and its new top left in the 410x307
  space. TC-2's relay helper (`ToothChartRelay.MoveTextLabel`) clamps and rounds the point, and `ControlChart` saves the label and refills.
  A press that is not on a label does nothing. The shared mouse class (`ToothChartMouse`) does this for the image host and for the live view.
  The press does not change the label's point (fix round 1): the first move puts the label at the point it is drawn at plus the drag, so a
  press that never reaches the release (the mouse capture lost, a dialog, Alt-Tab) leaves the chart's data as it was.
- **Change Color (the parity round).** Open Dental's manual: Change Color is "to change the color of an existing drawing or text"
  (https://www.opendental.com/manual/chartdrawing.html). With `CursorTool.ColorChanger`, the shared mouse looks for a drawing near the
  press first, as DirectX did; when there is none, the top-most free-text label under the press (the boxes of
  `ToothSceneBuilder.GetFreeTextBoxes`, as Move Text picks one up) takes the pen colour (`ToothChartData.ColorDrawing`) at once, and the new
  event `TextColorChanged` (`ToothChartTextColorChangedEventArgs`: the label's number and the colour) goes through the image host or the live
  view, the wrapper and the relay to `ControlChart.toothChartRelay_TextColorChanged`, which sets the `ToothInitialType.Text` row's
  `ColorDraw` to the Draw tab's colour box, saves it (`ToothInitials.Update`) and refills. A press on no label and no drawing does nothing.
- The 2D chart (`ToothChart2D`) draws the labels with `DrawString` in the control's own font. It does not move or recolour them: its mouse
  code stays its own (a difference the plan keeps for the 2D chart's other tools too).
- **The 2D chart's text size.** The 2D chart draws all of TC-5's text (free text and paint text) in the control's fixed font, Microsoft
  Sans Serif 8.25 pt, at every chart size, as it draws its own tooth numbers and its "W" watch mark; it is not scaled as the WPF chart's
  text is (14 pixels times the ratio for free text, the watch mark's 15 for paint text). It measures and draws with
  `StringFormat.GenericTypographic` (`ToothChart2D.CreateTextFormat`), without the padding `GenericDefault` adds on each side, so a text is
  centred as in the WPF chart. Measured by `TextDrawingTests.TextSizes_The2DChartsFontIsFixed_TheWpfChartsScales` (written to
  `%TEMP%\ToothChartImageTests\tc5-text-sizes.txt`): at 410x307 the 2D chart's cell is 13 px and the WPF chart's paint text cell 15 px and
  free text cell 14 px; at 926x858 (ratio 2.259) the 2D cell stays 13 px while the WPF cells are 34 and 32 px. So in the big chart window
  the 2D chart's text stays small.

### 1.2 Paint type Text (15) with text other than "W" (O-22, revised in the parity round)

- "W" stays the watch mark. Any other text goes to the new `ToothChartWrapper.SetText` through `ToothChartRelay.SetText`, which keeps it on
  the tooth (`ToothGraphic.PaintText`, `colorPaintText`).
- The WPF chart draws it **beyond the root tip, in the watch mark's band**, as Open Dental draws it ("Fracture" above maxillary 9 in its 2021
  tip, https://odtechblog.files.wordpress.com/2021/01/text-paint-type.png; "UCF" under mandibular 24 and 25 in its 26.2 screenshot,
  https://i0.wp.com/opendental.blog/wp-content/uploads/2026/10/image-6.png?ssl=1): on the watch mark's line (153 pixels times the ratio
  above the centre over a maxillary tooth, 138 below it under a mandibular one, as `AddWatches` places the "W"), at the watch mark's x with
  the tooth's mesial shift, centred on the tooth and kept inside the picture across, in the procedure's colour, in the watch mark's own font
  (`GetFontPaintText` is `GetFontWatch`: Arial 15/6 times the ratio, regular weight, not bold). When the tooth's place also has a "W"
  (`GetWatchPositions`), the text goes one line nearer the tooth and the "W" keeps the edge, as "UCF" over "W" in the 26.2 screenshot (seen
  on the mandible; the maxilla mirrors it). A primary tooth's text shows only when the primary tooth is shown; a missing tooth's text still
  shows, as its watch mark does. Round 2's crown placement (`PaintTextCrownOffsetMm`, bold) is removed.
- The 2D chart draws it in its own watch rows (`RenderToothWatch`: y 0 at the top, `Height-Font.Size-8` at the bottom), centred on the
  tooth, one line nearer the tooth under a "W", in its regular fixed font (1.1).

### 1.3 TextBelow (18) (O-21, revised in the parity round)

- Open Dental 26.2: "the procedure's custom text displays below the odontogram", and its screenshot shows "Occlusal Guard" in a one-line
  box under the date slider and above the patient notes box, not in the tooth picture and tied to no tooth
  (https://opendental.blog/version-26-2/). So the tooth picture no longer draws TextBelow at all: the scene builder's and the 2D chart's
  edge bands, their cut-to-fit helpers and font, `ToothGraphic.TextBelow`, the wrapper's and the relay's `SetTextBelow`, the
  `IToothChartPaintTarget` member and the painter's case are removed (the painter draws nothing for TextBelow again, as on `main`).
  **The pictures used by sheets, Treatment Plan printouts, the big chart window and the Patient Dashboard no longer carry TextBelow**; whether
  Open Dental shows it there is unknown (a screenshot of an Open Dental 26.2 printout would settle it).
- **The strip.** The Chart module has a one-line, read-only text box, `textTextBelow` (`OpenPmsDental.ToothChartTextBelowStrip`, in
  `WpfControlsOD`), in the standard text-box font (the control's default) with black text on white, the same whatever engine draws the chart.
  `ControlChart.LayoutTextBelow` puts it right under the date slider (`panelToothTrackBar`), as wide as the slider, in every layout: after the
  sheet layout, `LayoutTextBelowAfterSheet` finds the control that starts right under the slider (the treatment notes in the standard layout,
  the tooth chart in a layout with the slider above it, such as Open Dental's "Big" layout) and, while the strip has text, starts that control
  under the strip with its bottom edge kept; without text the control has the sheet's place and the strip is hidden.
- **Its text.** `FillToothChart` fills it (`FillTextBelow`) from `_listDataRowsProcsForGraphical`, the rows it paints, so it follows the date
  slider, the chart view's status filters and the selected treatment plans. `ToothChartPainter.GetTextBelowStripText` reads them before the
  painter's skips, so TextBelow procedures of every treatment area count (tooth, arch, quadrant and mouth: Open Dental's examples, occlusal
  guards and clear aligners, are mouth and arch procedures, which the painter leaves out in the Chart module); Hide Graphics leaves one out,
  as the painter does. The codes' paint texts are joined with "; " in ProcDate order (rows of one date in the list's order). How Open Dental
  stacks several items, and how it cuts a long text, is not known (its screenshot shows one); one line clipped by the box is the closest
  reading. With no patient the strip is empty and hidden.
- `ProcedureCodes.GetPaintTypesForList` lists TextBelow, and the procedure code window shows the paint text box for both Text and
  TextBelow. `FormProcCodeNew` casts the list index to the paint type, which stays valid because the values are 0 to 18 with no gap and
  TextBelow is the last; `PaintTypeListTests` pins this.

### 1.4 The extraction graphic (O-20, revised in the parity round)

- The setting is the fork's own preference row `ForkToothChartShowCompletedExtractions`, in the new business class
  `OpenPmsDentalBusiness/Logic/ToothChartExtractionGraphic.cs`, read and written by name (`PrefC.GetRawOrDefault`, `PrefC.UpsertRaw`), as
  `RecallProphyPerioSwitch` does. It is not in `PrefName`, so there is no `Pref.cs`, `PrefInfos.xml`, convert-script or migration change,
  and no adoption change (Open Dental 25.3.48 has no such setting).
- **On by default**, as in Open Dental 26.2 ("The preference is enabled by default.", https://opendental.blog/version-26-2/): a missing
  row reads as on (`GetRawOrDefault(PREF_NAME,"1")`), so a new database, an adopted one and every fork office from before TC-5 see the X
  after the upgrade, as Open Dental 26.2 offices do. While the preference cache is not loaded `GetRawOrDefault` gives the same default, and
  `IsOn`'s catch (only an unexpected fault reaches it) returns it too, so a chart painted before the cache loads looks like one painted after
  it in an office that never changed the setting. `Save` is unchanged: saving the default over a missing row writes nothing, and the first
  time the office turns it off the row is inserted as "0".
- The check box has no `PrefInfos.xml` entry either (the plan, review row 10), so the Preferences window gives it no help text; that is as
  planned.
- Setup > Preferences > Chart - General has the check box "Show completed extractions with an X on the tooth chart", ticked for an office
  that never changed it. Saving the page saves it with an audit line (Setup) when it changed; the page's `Changed` flag makes the
  Preferences window send the preference signal, and the Chart module's settings snapshot includes the setting, so the chart redraws.
- The completed-extraction skip is in one place since TC-2 (`ToothChartPainter.PaintProcedures`). With the setting on, a completed
  extraction (C, EC or EO) is not skipped: it draws the big X in its status's colour, and the tooth stays missing (its missing initial
  hides it, as Open Dental 26.2's screenshot shows: 19 and 20 hidden, a thin X over each space). Every caller paints through that one
  painter, so the Chart module, the big chart, the Treatment Plan printout, sheets and the Patient Dashboard all show it.
- **A plain switch (the parity round).** Round 2's exception (no X where a completed implant or a completed bridge's or denture's pontic
  fills the tooth's place, `GetToothNumsReplaced`) is removed: Open Dental's release note describes a plain switch ("Now they do."), and
  no public source shows an exception. With the setting on, a completed extraction draws its X in the painter's normal order, whatever
  follows on that tooth. The scenes `extraction-completed-graphic-under-implant` and `-under-pontic` are kept and now show the X over the
  implant and over the pontic; `ExtractionGraphic_CompletedImplantOrPontic_KeepsTheX_InPainterOrder` pins it (the X of 19, 14 and 4 and
  of every other completed extraction, in the list's order).
- A completed primary extraction's X is drawn at the primary tooth's place, over the permanent tooth beneath, as a planned one is (scene
  `extraction-completed-graphic-primary`: A and T completed, K planned).
- The Chart module's date slider is unchanged: the procedure filter already drops an extraction whose dates the slider has not reached, and
  shows one completed after the slider date as planned (`ShouldRowShowGraphical`), so it gets the planned X in either setting.
- The 2D chart draws the X with its existing lines (`ToothChart2D.DrawFacialView`).
- DirectX and OpenGL draw the X too (it is their own `SetBigX`), but none of the text: they leave with TC-9a, and an office reaches the WPF
  chart with TC-8. Until then the text shows on computers set to Simple 2D. The TextBelow strip shows on every engine.

### 1.5 Files

- **New:** `OpenPmsDentalBusiness/Logic/ToothChartExtractionGraphic.cs`, `SparksToothChart/ToothChartFreeText.cs`,
  `WpfControlsOD/UI/ToothChart/ToothChartTextBelowStrip.cs` (the parity round); tests `tests/ToothChartImageTests/TextDrawingTests.cs`,
  `TextAndExtractionDbTests.cs`, `TextBelowStripTests.cs` (the parity round), `Simple2DImageTests.cs`, `Baselines/simple2d-net48/**`, the new
  WPF baselines and call logs.
- **Edited, chart:** `ToothChartData.cs` (`ListFreeTexts`), `ToothGraphic.cs` (`PaintText`), `ToothChartWrapper.cs` (`AddText`, `SetText`,
  `TextMoved` and `TextColorChanged` from the engine controls, `ToothChartTextColorChangedEventArgs`), `ToothChartMouse.cs` (the text mover,
  the colour changer on labels), `ToothChart2D.cs`, `Wpf3D/ToothSceneBuilder.cs` (free text, paint text in the watch band,
  `GetFreeTextBoxes`, their fonts), `Wpf3D/ToothChartImageHost.cs` and `Wpf3D/ToothChartWpfView.cs` (`TextMoved` and `TextColorChanged`
  passed on).
- **Edited, relay:** `WpfControlsOD/UI/ToothChart/ToothChartRelay.cs` (`AddText`, `SetText`, `TextColorChanged`, the painter's
  completed-extraction skip, `GetTextBelowStripText` and its separator, the settings snapshot).
- **Edited, program and business layer:** `ProcedureCodes.cs` (`GetPaintTypesForList`), `FormProcCodeEdit.cs`, `FormProcCodeNew.cs`
  (comment), `UserControlChartGeneral.cs`/`.Designer.cs`, `Enumerations.cs` (`TextBelow`'s summary), and in the parity round
  `ControlChart.cs` and `ControlChart.designer.cs` (the strip, `FillTextBelow`, `LayoutTextBelowAfterSheet`, `LayoutTextBelow`,
  `toothChartRelay_TextColorChanged`): the plan gives this package the Chart module for the strip, as no other stream owns it now.
- **Edited, tests:** `tests/P262SchemaTests/PaintTypeListTests.cs`; in `tests/ToothChartImageTests`: `Scenes/ChartScenes.cs` (new scenes,
  `HasLegacyRecording`, the completed-extraction helper, descriptions of the older text scenes), `SceneTests.cs`,
  `ParityProbeTests.cs` and `LegacyRecorder.cs` (scenes without an old recording left out), `MovedLabelTests.cs` (comment),
  `Support/CallLogFixtures.cs` (`CallLogUser.IsExtractionGraphicOn`, tri-state since the parity round), `Support/ParityProbes.cs`,
  `Support/WpfGallery.cs` (the 2D chart's new pictures, the new scenes' cells), `WpfImageTests.cs` (G2's note and its strip picture).

## 2. Tests

### 2.1 What was added

- **Scene builder** (`TextDrawingTests`): the label is the last 2D item, at the drawing space's point as the drawings put it (within a pixel,
  at 410x307, 500x370, 926x858 and 360x480), opaque, Arial 14 times the ratio; a label past an edge is drawn inside, an empty label draws
  nothing, a label added again replaces the first, `Copy` holds its own labels and `ResetTeeth` clears them. Paint text (the parity round):
  on the watch mark's line above maxillary 3 and 14 (a shifted tooth) and below mandibular 30 and shown primary K, centred at the watch
  mark's x; with a "W" on 9 and on 24, one line nearer the tooth, the "W" at the edge, on both arches; the watch mark's size, regular weight,
  the procedure's colour; none on a hidden primary tooth or for empty text; "W" stays the watch mark; drawn after the watches and before
  the numbers; no TextBelow item in the picture; at 410x307, 926x858 and 360x480. The 2D chart's paint text only in its two watch rows.
- **Text mover** (`TextDrawingTests`): on the shared mouse at 410x307, 615x461 and 926x858, a press picks up the top-most label, a move keeps
  it under the mouse, the release raises `TextMoved` with the label's number and its new point (`start + distance / ratio`); a press off a
  label, a click without a move and the other four tools raise nothing and move nothing; a label drawn inside the edge is dragged from where
  it is seen. Through the image host (synthesised `OnMouseDown`/`Move`/`Up`), the wrapper's and the relay's `TextMoved`; through the live
  view (`MouseDownForTests`, on an off-screen shown form), the relay's `TextMoved` and the view showing the label where the mouse took it.
- **Change Color on text** (`TextDrawingTests`, the parity round), as the text mover's tests: on the shared mouse at three sizes, the
  top-most of two labels under the press takes the pen colour, the one under it and a far one keep theirs, `TextColorChanged` carries the
  label's number and the colour, the label does not move, a press on nothing does nothing; a drawing near the press is recoloured first and
  the label keeps its colour; through the image host, the wrapper's and the relay's `TextColorChanged` and the scene drawing the label in the
  new colour; through the live view, the relay's event and the view showing the new colour.
- **The TextBelow strip** (`TextBelowStripTests`, the parity round): the strip on an off-screen shown form, filled from the Chart module's
  own row filter (`GetChartModuleRows`) for synthetic patients on the scratch database: hidden for a patient with no TextBelow procedure,
  shown for one Mouth procedure ("Occlusal Guard"), "Implant planned; Aligners; Occlusal Guard" for a tooth, an arch and a mouth procedure
  entered out of date order, with a Hide Graphics one left out; the slider at 2022-02-15 drops the guard, before every date it hides, no
  patient hides it; read-only, one line, no tab stop, black, the form's font. `GetTextBelowStripText` alone on a hand-made table (ProcDate
  order, rows of one date in list order, other paint types and hidden rows left out). `ControlChart`'s source: the strip in the designer,
  filled from the painted rows before the painter, laid out under the slider after the sheet layout. And gallery G2's picture of the strip
  (`TextBelowStripPicture`: the WPF chart of `text-all-kinds`, a date slider as the Chart module's and the strip with "IMP; IMP").
- **Database** (`TextAndExtractionDbTests`, on the scratch database of the call-log tests, `ChartTestDb`: a new database on the local MariaDB
  named `tc2_` with the run's time and process, dropped when the run ends; synthetic patients only): a new database has no row and reads
  **on** (the parity round); a missing row reads on, saving on writes nothing, the first save off inserts "0", a later save updates the one
  row, the name is not in `PrefName` and starts with "Fork" (as `RecallSwitchDbTests` checks its row); the call-log user's three states
  (null deletes the row and reads on, false writes "0", true "1"); the settings snapshot sees a change (now a turn off); the painter against
  the date slider with the setting on and off (before every date nothing; at 2022-02-10 the planned X of 19 and 30 either way, and with
  the setting on the X of 31 in C's colour and of 2 in EO's; today all three completed ones with the setting on); the Treatment Plan
  printout, sheets and the big chart draw the X after the missing tooth; the X under a completed implant, a bridge pontic and a denture
  pontic, in list order (`ExtractionGraphic_CompletedImplantOrPontic_KeepsTheX_InPainterOrder`, the parity round); the free-text add,
  move and save round trip (Add Text at 0,0, a drag on a 615x461 image host of 151 and 92 pixels saved as 101,61, drawn there by the next
  refill, then a delete) under en-US and de-DE; the Chart preferences page's source (the check box, the save with its audit line,
  `Changed` for the signal).
- **Call logs.** `CallLogUser.IsExtractionGraphicOn` is tri-state since the parity round: **false** (every scene that sets nothing) writes
  "0", so the scenes older than TC-5 run with the setting off and log exactly what they log on `main`; **null** deletes the row, so the
  default applies (on), which the `*_ExtractionGraphic` scenes now use; **true** writes "1" (the database tests). New logs with the setting
  unset (on): the Chart module (`Paint_Today` and every slider date), the Treatment Plan printout, sheets, the dashboard and the big chart.
- **Images**: `ChartScenes_MatchTheWpfBaselines` with the new and re-recorded WPF pictures; `Simple2DImageTests` gates the 2D chart's
  pictures of the scenes with TC-5 drawing and checks that the older ones now differ from TC-0's 2D recording;
  `Scenes_DrawDifferentPictures_InSimple2D` lists `text-below-statuses` again among the scenes that draw the empty chart (TextBelow is not
  in the picture). The `simple2d-net48` baselines pin GDI+ text drawn with ClearType (`TextRenderingHint.ClearTypeGridFit`) under this PC's
  ClearType settings (font smoothing, ClearType tuning, DPI); on another PC the text's pixels may differ. Re-record them there
  (`OPMS_RECORD_2D_CHART=1`) rather than treat such a difference as a defect.
- **Fix round 1** (`TextDrawingTests`, `TextAndExtractionDbTests`): the 2D and WPF text sizes (1.1); a MoveText press with no move and no
  release keeps the label's stored point; a source pin of the Chart module's `toothChartRelay_TextMoved` (the `Find` of the text label,
  `ToothChartRelay.MoveTextLabel(toothInitial,e.Location)`, `ToothInitials.Update(toothInitial)`, `FillToothChart(retainSelection:true)`, in
  that order) and of its subscription `_toothChartRelay.TextMoved+=toothChartRelay_TextMoved;`, because the round trip's `TextMoved`
  handler is a copy of it. (Round 1's TextBelow cut tests left with the bands in the parity round.)
- **P262SchemaTests** (`PaintTypeListTests`): every paint type in number order, TextBelow last with no gap (the cast of `FormProcCodeNew`),
  the same list whatever the code has, and the paint text box for Text and TextBelow in the window's source.

### 2.2 The baselines and call logs

- **Rounds 1 and 2 (`chart/tc5-1`, `chart/tc5-2`):** 14 WPF pictures re-recorded (`text-other`, `text-other-eo-r`,
  `text-below-statuses`, `text-label-long`, `text-labels-over-teeth`, `text-labels-at-edges`, `text-label-moved`, each at 410x307 and
  500x370) and 21 new (`extraction-completed-graphic`, `-primary`, `-under-implant`, `-under-pontic`, `text-below-with-watch`,
  `text-below-cut-to-fit`, `text-other-moved-and-missing`, `text-label-past-edge` at both sizes, and `text-all-kinds` at those and
  926x858, 640x320 and 360x480). Their DirectX pictures (the parity probes' target) draw nothing of TC-5, so the parity allowance list
  names what the new drawing covers: the upper numbers' ink boxes of `text-label-long`, whose label runs along that row, one allowance
  per number (5, 9 and 11 at 410x307; 2 to 12 at 500x370).
- **The parity round (`chart/tc5-4`), WPF: 22 pictures re-recorded and 2 new.** Re-recorded: `text-other`, `text-other-eo-r`,
  `text-other-moved-and-missing` (the paint text moves from the crown to the watch band), `text-below-statuses`, `text-below-with-watch`,
  `text-below-cut-to-fit` (no band any more), `extraction-completed-graphic-under-implant`, `-under-pontic` (the X now over the implant
  and the pontic), each at 410x307 and 500x370, and `text-all-kinds` at its five sizes (paint text moved, no band). New:
  `text-other-with-watch` ("UCF" over "W" on 24, "Fx" over "W" on 9) at 410x307 and 500x370. The crown-text parity allowances of round 2
  (`text-other` 14 and 19, `text-other-eo-r` 5 and 28) are removed: the text no longer covers those probes. `text-label-*` kept their
  pixels. Every other WPF picture of the scratch recording had the committed pixel SHA-256; their `.txt` differ only in the Windows build
  line, so they are not rewritten. The manifest is that of the recording.
- **The parity round, Simple 2D:** the same 21 pictures re-recorded (all but `text-below-statuses`) plus `text-other-with-watch` at two
  sizes; `text-below-statuses` leaves `Simple2DImageTests` (its 2D picture is TC-0's again) and its two 2D baselines are deleted.
- **The parity round, call logs (15 re-recorded).** Every setting-off log is byte-identical to `main`'s again: the eleven logs of scenes
  with the Paint fixtures' TextBelow procedure lose round 1's `SetTextBelow` line (`ControlChart/Paint_PrintRecolor`, `Paint_Today`,
  `Paint_TreatPlans`; `ControlTreat/Paint_AllStatuses`, `Paint_PlanOnly`, `Paint_ShowCompleted`; `FormToothChartBig/Paint`;
  `SheetPrinting/Paint_Automation_AllStatuses`, `Paint_Dashboard`, `Paint_Sheet`, `Paint_Sheet_ShowCompleted`; checked against `main`
  for all 44 of `main`'s logs). The unset-setting (default on) logs that changed lose the same line: `ControlChart/Paint_Today_ExtractionGraphic`,
  `ControlTreat/Paint_ShowCompleted_ExtractionGraphic`, `SheetPrinting/Paint_Dashboard_ExtractionGraphic` and
  `Paint_Sheet_ShowCompleted_ExtractionGraphic`; the other unset-setting logs (the slider patient has no TextBelow) are unchanged, because
  unset now gives exactly what the setting on gave.
- The new `.txt` files have the two lines of TC-1's fix round ("Fresh render", "Screen DPI seen by the test host"); every re-recorded and
  new picture was repeatable and gave the same pixels in a fresh render.

### 2.3 Targeted runs (build PC, `.git/od-tools/test-results/`)

All on the branch at `6ce016b89`, after a full build of the solution filter through the lane (0 errors):

- `tc5-final-image.txt`: ToothChartImageTests 196 passed, 0 failed, 3 skipped (the legacy recorder and its comparison, and the opt-in
  on-screen times, which are switched on by their variables only); NoHqTests 57 passed. The same run wrote gallery G2 (section 3), so it took
  1158 s.
- `tc5-final-other.txt`: P262SchemaTests 24 passed, 4 skipped (the 26.2 document tests, which find the Open Dental documents only in the main
  checkout, as before this package); RegistryTests 39, BINATests 23, BSHELLTests 72 and NoHqTests 57 passed.
- The recording runs (`tc5-record-1.txt`, `tc5-record-2.txt`) recorded every WPF baseline into a scratch folder, the 2D baselines and the call
  logs; the first found the text mover's null reference (`35fdc50ac`). A run of the gate with the re-recorded baselines then found the
  parity probes the new drawing covers (`6ce016b89`).

Fix round 1, on `chart/tc5-2`, after a full build through the lane (0 errors):

- `tc5-2-record.txt` (at `06b17717a` and the text-size test, committed with `a6df3a9eb`): recorded the 2D baselines, the call logs (none changed) and every WPF baseline into a scratch
  folder (270 with the committed pixels, 4 new), with the parity wildcards removed: it listed the numbers of `text-label-long` that fail
  without an allowance (section 2.2) and found two faults of the new test and painter code, fixed in `a6df3a9eb`.
- `tc5-2-final.txt` (at `a6df3a9eb`): ToothChartImageTests 201 passed, 0 failed, 3 skipped (as above); P262SchemaTests 24 passed, 4
  skipped (as above); RegistryTests 39, BINATests 23, BSHELLTests 72 and NoHqTests 57 passed. The same run wrote gallery G2's second
  version (section 3), 947 s.

The parity round, on `chart/tc5-4`, after a full build through the lane (0 errors):

- `tc5-4-record.txt` and `tc5-4-record-2.txt` (at `55f68ad88`, the second with `text-other-with-watch` narrowed to one "UCF"): recorded
  every WPF baseline into a scratch folder, the 2D baselines and the call logs (section 2.2). In the first, the only failure was the
  parity probe test against the then-committed crown-text pictures (expected: the new pictures were still in the scratch folder); the
  second passed (ToothChartImageTests 207 passed, 0 failed, 4 skipped) and gave the first run's pixels for every picture but the
  narrowed scene.
- `tc5-4-final.txt` (at `bef0e2c8e`, after a full build of the solution filter through the lane, 0 errors): ToothChartImageTests 211, 208
  passed, 0 failed, 3 skipped (the legacy recorder and its comparison, and the opt-in on-screen times); P262SchemaTests 24 passed, 4
  skipped (the 26.2 document tests, as before); RegistryTests 39, BINATests 23, BSHELLTests 72 and NoHqTests 57 passed. The same run
  wrote gallery G2's fourth version (section 3), 983 s.

## 3. Gallery G2

On the build PC, outside the repository like G1: `.git/od-tools/test-results/tc5-gallery-G2/` of the main checkout, the **fourth version**,
made after the Open Dental parity round (`chart/tc5-4`). The first version (before the fixes) is kept as `tc5-gallery-G2-round1/` and the
second (after fix round 1) as `tc5-gallery-G2-round2/`, each with its zips; a third version for the abandoned `chart/tc5-3` (the tooth shown
under the X) was never finished. The owner should look at the fourth version only.

**What changed from the second version (the parity round):**

- The extraction graphic is on by default and a plain switch: `extraction-completed-graphic-under-implant` and `-under-pontic` now show
  the X over the implant and over the pontic; the tooth stays hidden under the X, as in Open Dental 26.2's screenshot.
- TextBelow is not in the picture any more: `text-below-statuses`, `text-below-with-watch`, `text-below-cut-to-fit` and `text-all-kinds`
  show no band. A new row, **`text-below-strip`** (`strip/text-below-strip.png`, written by `WriteGallery` for G2 through
  `TextBelowStripPicture`), shows the Chart module's strip under the date slider, below the WPF chart of `text-all-kinds`.
- Paint text other than "W" sits beyond the root tip in the watch band (`text-other`, `text-other-eo-r`, `text-other-moved-and-missing`,
  `text-all-kinds`), with a new scene, `text-other-with-watch`, for the stacking with a "W" ("UCF" over "W" on 24, as in Open Dental's
  screenshot; "Fx" under "W" on 9).
- Change Color on free text (no picture: the gallery holds still scenes).

**The fourth version:**

- `g2-tc5.html` (also alone in `tc5-gallery-G2-tc5-scenes.zip`, 17 MB): the 20 rows of this package, before and after, at every size:
  today's DirectX and Simple 2D pictures (TC-0's recording), the Simple 2D chart now, the WPF picture as approved in G1 (the WPF baselines
  of `main` at `f19da99ce`) and the WPF picture now, plus the `text-below-strip` row. Its head says what changed from the second version.
  This is the page to show the owner. It is made by `make_g2_tc5.py` in the lead's scratch folder (its fourth-version text; the scene
  descriptions come from the gallery's own `index.html`), and `make_g2_artifact.py` there turns it into the shareable page
  (`g2/tooth-chart-gallery-g2.html` with its 131 pictures; the strip row falls in its TextBelow group).
- `index.html` (the whole folder in `tc5-gallery-G2.zip`, 126 MB, zipped with `tools/New-ToothChartGallery.ps1 -NoTestRun`): the full
  gallery as G1 was made (WriteGallery with `OPMS_CHART_GALLERY_NAME=G2` and the approved pictures of `main`). 276 of 276 WPF pictures
  have exactly the pixels of this branch's baselines; 253 have an approved picture, 241 of them the same and 12 different (`text-other`,
  `text-other-eo-r` and the four `text-label-*` scenes at both sizes; `text-below-statuses` is the same as approved again), and 23 are new.
  Every DirectX parity caption passes (253 of 253). The render times did not move: a refill with its picture 333 ms at 410x307 and 646 ms
  at 615x461 (medians), a selection-only change 11 and 16 ms.
- The owner's answer goes into `docs/owner-questions.md` with the commit and the date (plan section 8); TC-6's hardware completes G2 later
  (its second part), so this answer covers TC-5's drawing.

## 4. Launch check L-5 (TC-5's part), for a person

On the build PC's desktop, with a DEBUG build of this branch and the developer switch `OPMS_TOOTHCHART_WPF=1` set for the user (TC-2), on a
scratch database with synthetic patients only (never an office's data). Each step says what to do, what should happen, and what to write
down. Do every step once at 100% display scale and once at 150%.

1. **Before.** Start the program on the scratch database. Open a synthetic patient in the Chart module. Write down which chart shows: the WPF
   chart (smooth teeth; File > Graphics still names DirectX, which is expected until TC-8).
2. **Add free text.** Draw tab > Add Text, type `Synthetic note`, OK. *Expected:* the label appears at the chart's top left, in the colour of
   the Draw tab's colour box, and "Move Text" is selected.
3. **Move it.** Press on the label, drag it to the middle of the lower teeth and let go. *Expected:* the label follows the mouse while you
   drag and stays where you let go; nothing else moves. Press beside the label and drag: nothing moves.
4. **It was saved.** Select another patient and come back, then close and start the program again. *Expected:* the label is where you left
   it both times.
5. **Edit and delete.** Double-click the label in the Draw tab's list, change the text to `Synthetic edit`, OK. *Expected:* the chart shows
   the new text at the same place. Select it in the list and press Delete. *Expected:* it is gone from the chart and the list.
6. **Edges.** Add a label and drag it past the right edge, then past the bottom edge. *Expected:* it stays fully inside the chart and can be
   picked up again from where it shows.
7. **Text procedures.** Setup > Procedure Codes: make (or edit) a test code with paint type Text and paint text `Fx`. Chart it as a
   treatment-planned procedure on tooth 3. *Expected:* `Fx` above the root tip of 3, at the top edge of the chart where a "W" would go,
   in regular (not bold) letters, in the treatment-planned colour. Set it complete. *Expected:* the completed colour. Chart a code with
   paint text `W` on 3 too. *Expected:* the "W" at the very top and `Fx` one line lower, nearer the tooth. Chart `Fx` on 30 and a `W` on
   30. *Expected:* below the root tip of 30: `Fx` one line above the "W", which is at the very bottom.
8. **TextBelow.** Edit a test code: the paint type list ends with `TextBelow`; choose it. *Expected:* the Paint Text box shows (as for Text).
   Enter `Occlusal Guard`, set its treatment area to Mouth, OK. Chart it for the patient. *Expected:* a one-line white box with black text
   `Occlusal Guard` appears right under the date slider and above the treatment notes; nothing is drawn in the tooth picture for it. Make
   a second TextBelow code `Aligners` (area Arch) and chart it with an earlier procedure date. *Expected:* the box reads
   `Aligners; Occlusal Guard` (oldest first). Move the date slider to before the guard's date. *Expected:* `Occlusal Guard` drops out of
   the box. Move it before both dates. *Expected:* the box is gone and the treatment notes are back right under the slider. Move the slider
   back to today. Select a patient with no TextBelow procedure. *Expected:* no box.
9. **The Big layout.** In the Chart module, choose a layout that has the date slider above the tooth chart (Open Dental's "Big" layout, or
   a custom Chart Layout sheet with the slider above the chart), on the patient of step 8. *Expected:* the TextBelow box sits right under
   the slider there too, and the chart under it is a little shorter; nothing overlaps. Switch back to the standard layout.
10. **The extraction graphic.** Chart an extraction on 30 and set it complete. *Expected:* 30 is missing and a thin X in the completed
    colour is drawn over its place (the setting is on by default, as in Open Dental 26.2). Setup > Preferences > Chart - General:
    *Expected:* "Show completed extractions with an X on the tooth chart" is ticked. Untick it, OK. *Expected:* the Chart module redraws,
    30 is missing and the X is gone; Audit Trail (Setup) has "Show completed extractions with an X on the tooth chart changed from 'True'
    to 'False'". On a second workstation (or after its next refresh) the X is gone too. Tick it again, OK. *Expected:* the X is back. Move
    the date slider before the extraction's completion date: *Expected:* the X in the planned colour and tooth 30 back, as before. Move the
    slider back to today. Chart an implant on 30 and set it complete. *Expected:* the implant shows and the X stays over it (a plain
    switch, as in Open Dental).
11. **Change Color on text.** Draw tab: add a label, pick a different colour in the colour box, choose "Change Color" and click on the
    label's letters (away from any drawing). *Expected:* the label takes the new colour at once. Select another patient and come back.
    *Expected:* it keeps the new colour. Click on a drawing with Change Color: *Expected:* the drawing is recoloured as before.
12. **Printouts.** With the setting on and the texts above, open the Treatment Plan print preview and a sheet with a tooth chart.
    *Expected:* the X and the `Fx` text show there (the same painter and picture); the TextBelow box does not (it is part of the Chart
    module, not of the picture).
13. **The 2D chart.** File > Graphics: choose Simple 2D, restart. *Expected:* the free text, `Fx` in the top and bottom rows where the 2D
    chart draws its "W", and the X show in the 2D chart; the TextBelow box under the slider is the same as with the WPF chart; free text
    cannot be moved or recoloured there (as planned). In the big chart window the 2D chart's text keeps its small size (as its tooth
    numbers do). Set Graphics back.
14. **Write down:** for each step, as expected or what differed, with a screenshot of anything that differed (synthetic data only).
