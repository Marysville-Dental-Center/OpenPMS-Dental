# Port note: TC-7 (graphical perio on the WPF chart)

Package TC-7 of the WPF tooth chart track ([docs/specs/wpf-tooth-chart-plan.md](../specs/wpf-tooth-chart-plan.md), the TC-7 row of
section 4; sections 2, 3, 5, 7 and 8; section 10 and its "Open Dental parity (2026-11-14)" part, rows PERIO-1 and ENG-5): branch
`chart/tc7-1` from `main` (`3620550cb`). **Status: spec written; the code follows.** It merges only after the owner approves gallery G3.

## 1. What the row asks, and how it is built

### 1.1 The owner's rule and what is public

The owner's rule of 2026-11-14 ("keep it the same as OD latest version") applies. How Open Dental 25.x and 26.x draw the graphical perio
chart is not public (audit row PERIO-1, od-unknown), so the WPF chart draws it **as the DirectX chart draws it today** (`DrawScenePerio`,
`DrawPerioRow`, `DrawNumbersAndLinesPerio`, `DrawPerioLegend` in `ToothChartDirectX.cs`), with the two documented Open Dental changes that
touch it (https://www.opendental.com/site/version24_4.html):

- **24.4, the implant on the graphical perio chart.** An implant whose procedure graphics are hidden counts as removed, so no implant is
  drawn for it. The rule stays where B-PERIO put it, in `FormPerioGraphical.LayoutTeeth` (`!HideGraphics`), which passes the implant teeth
  to the chart; the WPF chart draws exactly the implants it is given, with DirectX's model and the perio rows' transforms (the lingual rows
  mirrored).
- **24.4, "Recession" instead of "Gingival Margin".** With the Perio Setup option on (`PerioMeasures.GetUseRecessionTerm`, the row
  `PerioRecessionInsteadOfGM`), the legend's gingival margin entry reads "Recession", except while the displayed exam has hyperplasia (a
  gingival margin stored over 100), when it reads "Gingival Margin" again: the rule of the Perio Chart's row label (B-PERIO,
  `PerioMeasures.GetGingMarginTerm`). Graphical Perio Setup already relabels its colour button (`FormPerioGraphicalSetup.cs:27-28`). Only the
  label changes; the line is the same gingival margin line in the same colour.

### 1.2 The perio scene (as DirectX draws it)

- **3D layer** (TC-1, unchanged): the four rows (maxillary facial, maxillary lingual, mandibular lingual, mandibular facial; `DrawScenePerio`
  590-628), with the perio light (ambient 0.6, diffuse 0.4); the lingual rows mirrored in Z and drawn with the reversed winding (plan 5.2),
  their implants with them.
- **2D layer, per row, after that row's teeth, in DirectX's order** (`DrawPerioRow`): the millimetre lines (the 0 line in the text colour,
  0.3 mm; grey lines at 3, 6 and 9 mm, 0.16 mm); then for each tooth the mobility number (facial rows only, in the tooth's mobility colour,
  in the numbers' font), the furcation triangles (grade 1 open, 2 closed, 3 filled; red from the red limit), the probing bars (0.6 mm, red
  from the red limit), the bleeding droplets and the suppuration droplets (flipped in Y on the mandible); then the gingival margin, CAL and
  MGJ lines (0.4 mm).
- **Then** the black centre line between the arches (0.5 mm), the numbers 1-32 (never highlighted), and the legend (bleeding, suppuration,
  probing with its two colours; gingival margin or recession, clinical attachment level, mucogingival junction; furcation 1, 2 and 3), in black
  text, at DirectX's places.
- The nine perio colours (`SetPerioColors`: bleeding, suppuration, probing, probing red, gingival margin, CAL, MGJ, furcations, furcations
  red) and the two red limits, as the window sets them.

### 1.3 The engine

- `ToothChartWrapper.PerioMode` is allowed on `Wpf3D` (it threw for every engine but DirectX). Simple 2D and OpenGL still refuse it.
- `FormPerioGraphical` keeps DirectX through `ToothChartRelay.ApplyGraphicalPerioMode` until TC-8. In DEBUG builds the developer switch
  `OPMS_TOOTHCHART_WPF=1` (TC-2) opens it on the WPF chart: launch check L-6.
- The relay passes the office's Recession option to the chart in `ApplyGraphicalPerioMode`, so `FormPerioGraphical` needs no edit.

## 2. Tests (planned)

- Scene builder: the 2D items of each row in DirectX's order, their colours, the red limits, the droplet flip, the legend's term.
- Images: every perio scene at 649x696 against WPF baselines; the print render and the 750x1000 save, made as `RenderPerioPrintout`
  makes them from the wrapper's picture.
- Parity probes against TC-0's DirectX recordings of the perio scenes.
- BPERIOTests, P262PerioTests, NoHqTests.

## 3. Gallery G3

Outside the repository, in the lead's test-results folder (`tc7-gallery-G3/`), as G2.
