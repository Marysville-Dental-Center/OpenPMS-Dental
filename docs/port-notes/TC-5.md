# Port note: TC-5 (free text, the text paint types and the extraction graphic)

Package TC-5 of the WPF tooth chart track ([docs/specs/wpf-tooth-chart-plan.md](../specs/wpf-tooth-chart-plan.md), the TC-5 row of
section 4; sections 2, 3, 5, 7, 8 and 10, owner answers O-20, O-21 and O-22 decided as recommended): branch `chart/tc5-1` from `main`
(`b8e931cea`). **Status: done; waiting for the owner's approval of gallery G2** (section 3), after which it merges. Nothing was downloaded
and no program was started; the tests ran in the lead's desktop session as the chart's other tests do.

Commits: `61f9b4f12` (this note's first part, the setting's business class, TextBelow in the paint type lists), `627d1ddb6` (the drawing,
the text mover, the relay and painter, the Chart preferences page), `35fdc50ac` (a fix the tests found, and free text on the nearest
pixel), `74134ffd4` (tests, scenes, baselines, call logs), `6ce016b89` (parity allowances), then the docs commit with this note.

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
- The 2D chart (`ToothChart2D`) draws the labels with `DrawString` in its own font. It does not move them: its mouse code stays its own (a
  difference the plan keeps for the 2D chart's other tools too).

### 1.2 Paint type Text (15) with text other than "W" (O-22)

- "W" stays the watch mark. Any other text is drawn on the tooth: `ToothChartRelay.SetText` passes it to the new
  `ToothChartWrapper.SetText`, which keeps it on the tooth (`ToothGraphic.PaintText`, `colorPaintText`).
- The WPF chart centres it on the tooth's facial view, on the crown, in the procedure's colour, in bold Arial with a cell height of 13 pixels
  times the ratio. It follows the tooth's mesial shift as the watch mark does. A primary tooth's text shows only when the primary tooth is
  shown. A missing tooth's text still shows, as its watch mark does.

### 1.3 TextBelow (18) (O-21)

- The painter's new case calls the relay's new `SetTextBelow(toothID, color, text)`, and the wrapper keeps it on the tooth
  (`ToothGraphic.TextBelow`, `colorTextBelow`).
- The WPF chart draws it inside the picture, in a band along the top edge over the maxillary teeth and along the bottom edge under the
  mandibular teeth, centred on the tooth, in the procedure's colour, in Arial with a cell height of 13 pixels times the ratio, and cut to fit:
  characters are dropped from the end until the text is no wider than the tooth's place in the layout (its width in
  `ToothGraphic.GetWidth`). The picture keeps its size, so every sheet field keeps its aspect. On a tooth that also has a watch mark (which
  sits in the same band), the text moves in by one line, so both show.
- `ProcedureCodes.GetPaintTypesForList` now lists TextBelow, and the procedure code window shows the paint text box for both Text and
  TextBelow. `FormProcCodeNew` casts the list index to the paint type, which stays valid because the values are 0 to 18 with no gap and
  TextBelow is the last; `PaintTypeListTests` pins this.

### 1.4 The extraction graphic (O-20)

- The setting is the fork's own preference row `ForkToothChartShowCompletedExtractions`, in the new business class
  `OpenPmsDentalBusiness/Logic/ToothChartExtractionGraphic.cs`, read and written by name (`PrefC.GetRawOrDefault`, `PrefC.UpsertRaw`), as
  `RecallProphyPerioSwitch` does. It is not in `PrefName`, and a missing row reads as off, so there is no `Pref.cs`, `PrefInfos.xml`,
  convert-script or migration change, and no adoption change (Open Dental 25.3.48 has no such setting).
- Setup > Preferences > Chart - General has the check box "Show completed extractions with an X on the tooth chart". Saving the page saves
  it with an audit line (Setup) when it changed; the page's `Changed` flag makes the Preferences window send the preference signal, and the
  Chart module's settings snapshot includes the setting, so the chart redraws.
- The completed-extraction skip is in one place since TC-2 (`ToothChartPainter.PaintProcedures`). With the setting on, a completed
  extraction (C, EC or EO) is no longer skipped: it draws the big X in its status's colour, and the tooth stays missing (its missing initial
  hides it, as before). Every caller paints through that one painter, so the Chart module, the big chart, the Treatment Plan printout, sheets
  and the Patient Dashboard all show it.
- The Chart module's date slider is unchanged: the procedure filter already drops an extraction whose dates the slider has not reached, and
  shows one completed after the slider date as planned (`ShouldRowShowGraphical`), so it gets the planned X in either setting.
- The 2D chart draws the X with its existing lines (`ToothChart2D.DrawFacialView`).
- DirectX and OpenGL draw the X too (it is their own `SetBigX`), but none of the text: they leave with TC-9a, and an office reaches the WPF
  chart with TC-8. Until then the text shows on computers set to Simple 2D.

### 1.5 Files

- **New:** `OpenPmsDentalBusiness/Logic/ToothChartExtractionGraphic.cs`, `SparksToothChart/ToothChartFreeText.cs`; tests
  `tests/ToothChartImageTests/TextDrawingTests.cs`, `TextAndExtractionDbTests.cs`, `Simple2DImageTests.cs`, `Baselines/simple2d-net48/**`,
  the new WPF baselines and call logs.
- **Edited, chart:** `ToothChartData.cs` (`ListFreeTexts`), `ToothGraphic.cs` (`PaintText`, `TextBelow`), `ToothChartWrapper.cs`
  (`AddText`, `SetText`, `SetTextBelow`, `TextMoved` from the engine controls), `ToothChartMouse.cs` (the text mover), `ToothChart2D.cs`,
  `Wpf3D/ToothSceneBuilder.cs` (the three kinds of text, `GetFreeTextBoxes`, their fonts), `Wpf3D/ToothChartImageHost.cs` and
  `Wpf3D/ToothChartWpfView.cs` (`TextMoved` passed on).
- **Edited, relay:** `WpfControlsOD/UI/ToothChart/ToothChartRelay.cs` (`AddText`, `SetText`, the new `SetTextBelow` at the end of the
  relay, `IToothChartPaintTarget.SetTextBelow`, the painter's TextBelow case and completed-extraction skip, the settings snapshot).
- **Edited, program and business layer:** `ProcedureCodes.cs` (`GetPaintTypesForList`), `FormProcCodeEdit.cs`, `FormProcCodeNew.cs`
  (comment), `UserControlChartGeneral.cs`/`.Designer.cs`, `Enumerations.cs` (`TextBelow`'s summary).
- **Edited, tests:** `tests/P262SchemaTests/PaintTypeListTests.cs`; in `tests/ToothChartImageTests`: `Scenes/ChartScenes.cs` (new scenes,
  `HasLegacyRecording`, the TextBelow and completed-extraction helpers, descriptions of the older text scenes), `SceneTests.cs`,
  `ParityProbeTests.cs` and `LegacyRecorder.cs` (scenes without an old recording left out), `MovedLabelTests.cs` (comment),
  `Support/CallLogFixtures.cs` (`CallLogUser.IsExtractionGraphicOn` and the scenes with the graphic on), `Support/RecordingPaintTarget.cs`,
  `Support/WpfGallery.cs` (the 2D chart's new pictures, the new scenes' cells), `WpfImageTests.cs` (G2's note).
- No file of `ControlChart`, `ControlTreat`, `FormOpenDental` or another stage-owned program file is edited: TC-2's `TextMoved` handler in
  `ControlChart` already saves a move.

## 2. Tests

### 2.1 What was added

- **Scene builder** (`TextDrawingTests`): the label is the last 2D item, at the drawing space's point as the drawings put it (within a pixel,
  at 410x307, 500x370, 926x858 and 360x480), opaque, Arial 14 times the ratio; a label past an edge is drawn inside, an empty label draws
  nothing, a label added again replaces the first, `Copy` holds its own labels and `ResetTeeth` clears them. Paint text: centred on the crown
  of maxillary and mandibular teeth, a missing tooth, a shifted tooth and a shown primary tooth; none on a hidden primary tooth or for empty
  text; "W" stays the watch mark; drawn after the watches and before the numbers. TextBelow: the top and bottom bands, one line in under a
  watch mark, cut to fit every tooth's width, centred, inside the picture, a primary tooth only when shown; `GetTextBelowFitted` itself.
- **Text mover** (`TextDrawingTests`): on the shared mouse at 410x307, 615x461 and 926x858, a press picks up the top-most label, a move keeps
  it under the mouse, the release raises `TextMoved` with the label's number and its new point (`start + distance / ratio`); a press off a
  label, a click without a move and the other four tools raise nothing and move nothing; a label drawn inside the edge is dragged from where
  it is seen. Through the image host (synthesised `OnMouseDown`/`Move`/`Up`), the wrapper's and the relay's `TextMoved`; through the live
  view (`MouseDownForTests`, on an off-screen shown form), the relay's `TextMoved` and the view showing the label where the mouse took it.
- **Database** (`TextAndExtractionDbTests`, on the scratch database of the call-log tests, `ChartTestDb`: a new database on the local MariaDB
  named `tc2_` with the run's time and process, dropped when the run ends; synthetic patients only): a new database has no row and reads off; a missing
  row reads off, saving off writes nothing, the first save inserts the row, a later save updates the one row, the name is not in `PrefName`
  and starts with "Fork" (as `RecallSwitchDbTests` checks its row); the settings snapshot sees a change; the painter against the date slider
  with the setting on and off (before every date nothing; at 2022-02-10 the planned X of 19 and 30 either way, and with the setting on the X
  of 31 in C's colour and of 2 in EO's; today all three completed ones with the setting on); the Treatment Plan printout, sheets and the
  big chart draw the X after the missing tooth; the free-text add, move and save round trip (Add Text at 0,0, a drag on a 615x461 image host
  of 151 and 92 pixels saved as 101,61, drawn there by the next refill, then a delete) under en-US and de-DE; the Chart preferences page's
  source (the check box, the save with its audit line, `Changed` for the signal).
- **Call logs**: every caller's logs re-recorded; the only changed lines are the Paint fixtures' TextBelow procedure, which now logs
  `SetTextBelow` (it logged nothing before), and the header counts. New logs with the setting on: the Chart module (`Paint_Today` and every
  slider date), the Treatment Plan printout, sheets, the dashboard and the big chart.
- **Images**: `ChartScenes_MatchTheWpfBaselines` with the 31 new or re-recorded WPF pictures; `Simple2DImageTests` gates the 2D chart's
  pictures of the 14 scenes with TC-5 drawing (28 at the standard sizes, 31 with `text-all-kinds`' extra sizes) and checks that the older ones
  now differ from TC-0's 2D recording; `Scenes_DrawDifferentPictures_InSimple2D` moves the text scenes to the drawn list.
- **P262SchemaTests** (`PaintTypeListTests`): every paint type in number order, TextBelow last with no gap (the cast of `FormProcCodeNew`),
  the same list whatever the code has, and the paint text box for Text and TextBelow in the window's source.

### 2.2 The baselines

- **Re-recorded (14),** because this package draws what they hold: `text-other`, `text-other-eo-r`, `text-below-statuses`,
  `text-label-long`, `text-labels-over-teeth`, `text-labels-at-edges`, `text-label-moved`, each at 410x307 and 500x370. Their DirectX
  pictures (the parity probes' target) draw nothing for it, so the parity probes' allowance lists name what the new drawing covers, each
  with its reason ("new drawing, not a difference of look"): the facial surface probes of the teeth whose paint text covers the probe's centre
  (`text-other` 14 and 19, `text-other-eo-r` 5 and 28; the text of 3 and 30 misses it), and the upper numbers' ink boxes of
  `text-label-long`, whose label runs along that row (its black ink joins the numbers' ink in the search box). The TextBelow bands and the
  other labels miss every probe.
- **New (17):** `extraction-completed-graphic`, `extraction-completed-graphic-primary`, `text-below-with-watch`, `text-below-cut-to-fit`,
  `text-other-moved-and-missing`, `text-label-past-edge` at 410x307 and 500x370, and `text-all-kinds` at those and 926x858, 640x320 and
  360x480.
- Every other WPF baseline is unchanged: a recording of every scene into a scratch folder gave the committed pixel SHA-256 for all of them,
  so only these files and the manifest's rows for them change. The new `.txt` files have the two lines of TC-1's fix round ("Fresh render",
  "Screen DPI seen by the test host"); every re-recorded and new picture was repeatable and gave the same pixels in a fresh render.

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

## 3. Gallery G2

On the build PC, outside the repository like G1: `.git/od-tools/test-results/tc5-gallery-G2/` of the main checkout.

- `g2-tc5.html` (also alone in `tc5-gallery-G2-tc5-scenes.zip`, 15 MB): the 16 scenes of this package, before and after, at every size:
  today's DirectX and Simple 2D pictures (TC-0's recording), the Simple 2D chart now, the WPF picture as approved in G1 (from `main` at
  `b8e931cea`) and the WPF picture now. This is the page to show the owner.
- `index.html` (the whole folder in `tc5-gallery-G2.zip`, 125 MB, made with `tools/New-ToothChartGallery.ps1 -NoTestRun`): the full
  gallery as G1 was made (WriteGallery with `OPMS_CHART_GALLERY_NAME=G2` and G1's approved pictures from `main`), with a new column for the
  2D chart's new pictures. 270 of 270 WPF pictures have exactly the pixels of this branch's baselines; 239 are the same as approved, 14
  differ (the re-recorded text scenes) and 17 are new. Every parity caption passes, the allowed ones as "known difference". The render
  times did not move: a refill with its picture 356 ms at 410x307 and 741 ms at 615x461, a selection-only change 15 and 17 ms (G1's and
  TC-3's software render: 316 to 384 ms and 559 to 728 ms).
- The owner's answer goes into `docs/owner-questions.md` with the commit and the date (plan section 8); TC-6's hardware completes G2 later
  (its second part), so this answer covers TC-5's drawing.

## 4. Launch check L-5 (TC-5's part), for a person

On the build PC's desktop, with a DEBUG build of this branch and the developer switch `OPMS_TOOTHCHART_WPF=1` set for the user (TC-2), on a
scratch database with synthetic patients only (never an office's data). Each step says what to do, what should happen, and what to write
down. Do every step once at 100% display scale and once at 150%.

1. **Before.** Start the program on the scratch database. Open a synthetic patient in the Chart module. Write down which chart shows: the WPF
   chart (smooth teeth; Setup > Graphics still names DirectX, which is expected until TC-8).
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
   treatment-planned procedure on tooth 3. *Expected:* `Fx` in bold on the crown of 3, in the treatment-planned colour. Set it complete.
   *Expected:* the completed colour. A code with paint text `W` still shows the watch mark at the top edge above 3.
8. **TextBelow.** Edit a test code: the paint type list ends with `TextBelow`; choose it. *Expected:* the Paint Text box shows (as for Text).
   Enter `IMP`, OK. Chart it on 14 and on 19. *Expected:* `IMP` at the very top of the chart above 14 and at the very bottom below 19,
   centred on the tooth. Chart a TextBelow code with a long text (`Implant planned`) on 25: *Expected:* shortened to the tooth's width. Chart
   the `W` code on 14 too: *Expected:* the watch mark at the edge and `IMP` one line further in, both readable.
9. **The extraction graphic.** Chart an extraction on 30 and set it complete. *Expected:* 30 is missing and there is no X (the setting is off
   at first). Setup > Preferences > Chart - General: tick "Show completed extractions with an X on the tooth chart", OK. *Expected:* the
   Chart module redraws and shows the X over 30's place in the completed colour; Audit Trail (Setup) has "Show completed extractions with an X
   on the tooth chart changed from 'False' to 'True'". On a second workstation (or after its next refresh) the X shows too. Move the date
   slider before the extraction's completion date: *Expected:* the X in the planned colour and tooth 30 back, as before. Untick the setting:
   *Expected:* the X is gone.
10. **Printouts.** With the setting on and the texts above, open the Treatment Plan print preview and a sheet with a tooth chart.
    *Expected:* the X and the texts show there too (both use the same painter and the WPF chart's picture).
11. **The 2D chart.** Setup > Graphics: choose Simple 2D, restart. *Expected:* the free text, `Fx`, `IMP` and the X show in the 2D chart;
    free text cannot be moved there (as planned). Set Graphics back.
12. **Write down:** for each step, as expected or what differed, with a screenshot of anything that differed (synthetic data only).
