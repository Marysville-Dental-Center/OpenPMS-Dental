# Port note: TC-5 (free text, the text paint types and the extraction graphic)

Package TC-5 of the WPF tooth chart track ([docs/specs/wpf-tooth-chart-plan.md](../specs/wpf-tooth-chart-plan.md), the TC-5 row of
section 4; sections 2, 3, 5, 7, 8 and 10, owner answers O-20, O-21 and O-22 decided as recommended): branch `chart/tc5-1` from `main`
(`b8e931cea`). **Status: in progress.** The package merges only after the owner approves gallery G2.

## 1. What the row asks, and how it is built

### 1.1 Free text (`ToothInitialType.Text`)

- `ToothChartRelay.AddText(text, location, color, toothInitialNum)` passes the label to the new `ToothChartWrapper.AddText`, which keeps it
  in `ToothChartData.ListFreeTexts` (a `ToothChartFreeText` each: the text, its point in the chart's 410x307 drawing space, its colour and the
  `toothinitial` row's number). `ResetTeeth` clears the list, as it clears the drawings. A label added again with the same number replaces
  the first.
- The point is the top left of the label's text cell in the 410x307 space, the space of the drawings (`ToothChartData.SizeOriginalDrawing`),
  scaled to the picture as the drawings are. `butAddText_Click` puts a new label at 0,0, the top left corner.
- The WPF chart draws every label last, over the teeth, the numbers and the drawings, in Arial with a cell height of 14 pixels times the
  pixel scale ratio, in the label's colour (opaque, as the drawings of the same colour). A label that would run past an edge of the picture is
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
