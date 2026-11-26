# Port note: TC-7 (graphical perio on the WPF chart)

Package TC-7 of the WPF tooth chart track ([docs/specs/wpf-tooth-chart-plan.md](../specs/wpf-tooth-chart-plan.md), the TC-7 row of
section 4; sections 2, 3, 5, 7 and 8; section 10 and its "Open Dental parity (2026-11-14)" part, rows PERIO-1 and ENG-5): branch
`chart/tc7-1` from `main` (`3620550cb`). **Status: done; gallery G3 approved by the owner on 2026-11-15 ("G3 looks good") and merged** (section 3); before that, it
merges. Nothing was downloaded, no program was started and no program file was edited; the tests ran in the lead's desktop session as the
chart's other tests do.

## 1. What the row asks, and how it is built

### 1.1 The owner's rule and what is public

The owner's rule of 2026-11-14 ("keep it the same as OD latest version") applies. How Open Dental 25.x and 26.x draw the graphical perio
chart is not public (audit row PERIO-1, od-unknown), so the WPF chart draws it **as the DirectX chart draws it today** (`DrawScenePerio`,
`DrawPerioRow`, `DrawNumbersAndLinesPerio`, `DrawPerioLegend` in `ToothChartDirectX.cs`), with the two documented Open Dental changes that
touch it (https://www.opendental.com/site/version24_4.html, Chart Module):

- **24.4, the implant on the graphical perio chart.** An implant whose procedure graphics are hidden counts as removed, so no implant is
  drawn for it. The rule stays where B-PERIO put it, in `FormPerioGraphical.LayoutTeeth` (`!HideGraphics`), which passes the implant teeth
  to the chart (the tooth missing, the implant in Gainsboro, a crown in WhiteSmoke); the WPF chart draws exactly the implants it is given,
  with DirectX's model, upside down on the maxilla, in the perio rows' transforms (the lingual rows mirrored). A source test
  (`PerioPrintoutTests.TheWindow_StillPassesOnlyImplantsThatAreNotRemoved_AndOpensThroughTheRelay`) ties the WPF drawing to that rule.
- **24.4, "Recession" instead of "Gingival Margin".** With the Perio Setup option "Use the term Recession" on
  (`PerioMeasures.GetUseRecessionTerm`, the row `PerioRecessionInsteadOfGM`), the legend's gingival margin entry reads "Recession", except
  while the displayed exam has hyperplasia (a gingival margin stored over 100), when it reads "Gingival Margin" again: the rule of the
  Perio Chart's row label (B-PERIO, `PerioMeasures.GetGingMarginTerm`). Graphical Perio Setup already relabels its colour button
  (`FormPerioGraphicalSetup.cs:27-28`, which knows no exam, so only the option counts there). Only the name changes; the line is the same
  gingival margin line in the same colour.

**Od-unknown look items** (kept as DirectX draws them until a screenshot of an Open Dental 26.2 graphical perio chart settles them):
the camera, light and shading of the teeth (LOOK-1); the numbers' and the legend's font, size and places; the legend's layout and its other
words; the shapes and line widths of the marks (droplets, furcation triangles, bars, the GM, CAL and MGJ lines); whether Open Dental's 3D
chart smooths the sloped lines; where Open Dental shows "Recession" (the legend, the setup window or both: the critic's note in the audit,
"TC-7 must choose the legend term", is answered with the legend following the Perio Chart's rule); and whether Open Dental's DirectX 11
chart lights the lingual rows as its DirectX 9 chart did.

### 1.2 The perio scene (as DirectX draws it)

- **3D layer** (TC-1): the four rows (maxillary facial, maxillary lingual, mandibular lingual, mandibular facial; `DrawScenePerio`), with the
  perio light (ambient 0.6, diffuse 0.4); the lingual rows mirrored in Z and drawn with the reversed winding (plan 5.2), their implants
  with them.
- **2D layer, per row, after that row's teeth, in DirectX's order** (`DrawPerioRow`): the millimetre lines (the 0 line in the text colour,
  0.3 mm; grey lines at 3, 6 and 9 mm toward the roots, 0.16 mm); then for each tooth the mobility number (facial rows only, in the tooth's
  mobility colour and the numbers' font, centred by the numbers' measure, 2.5 mm into the crown on the maxilla and 5.5 mm on the mandible),
  the furcation marks of the row's three sites (grade 1 an open "V", 2 the outline, 3 the outline and the filled triangle; 0.38 mm lines; red
  from the red limit), the probing bars (0.6 mm, red from the red limit), the bleeding droplets and the suppuration droplets (flipped in Y on
  the mandible); then the gingival margin, CAL and MGJ lines (0.4 mm).
- **Then** the black centre line, 0.5 mm, drawn as DirectX draws it with the world matrix the last row left set, so it lands on baseY between
  the arches; the numbers 1-32 (never highlighted; no primary letters); and the legend at (-60, baseY-63) mm: bleeding, suppuration and
  probing with its two colours; gingival margin (or Recession), clinical attachment level and mucogingival junction with their 2-pixel line
  samples; furcation 1, 2 and 3 pointing down, each in the colour its grade has under the red limit; the words in black.
- The nine perio colours (`SetPerioColors`: bleeding, suppuration, probing, probing red, gingival margin, CAL, MGJ, furcations, furcations
  red) and the two red limits, as the window sets them; a change in Graphical Perio Setup redraws the chart.
- DirectX's triangle fans (the droplets, the filled furcation) are 2D items of their fan triangles; its horizontal and vertical rectangles
  (the millimetre lines, the centre line, the bars, the legend's samples) fill exactly DirectX's pixels (TC-3's snapping); the sloped lines
  and the triangles are smoothed, where DirectX draws them jagged.

### 1.3 The engine and the relay

- `ToothChartWrapper.PerioMode` is allowed on `Wpf3D` (it threw for every engine but DirectX). Simple 2D and OpenGL still refuse it, so a
  window whose WPF chart fell back to Simple 2D says so and closes, as a failed DirectX start does today.
- `ToothChartWrapper.PerioUseRecessionTerm` (in `ToothChartData`, not cleared by `ResetTeeth`) carries the option; the relay's
  `ApplyGraphicalPerioMode` sets it from the preference for either engine, and the relay's `IsPerioRecessionTerm` reads and sets it (the
  scenes use it). So `FormPerioGraphical` needs no edit. The DirectX chart's legend keeps its fixed "Gingival Margin": it is today's chart,
  retired by TC-9a.
- `FormPerioGraphical` keeps DirectX through `ApplyGraphicalPerioMode` until TC-8. In DEBUG builds the developer switch
  `OPMS_TOOTHCHART_WPF=1` (TC-2) opens it on the WPF chart: launch check L-6 (section 4). The window's chart on screen is then the live view
  (O-34 (b1)); the print and Save to Images use `GetBitmap`, the renderer's picture.
- A WPF render that fails twice returns the Simple 2D picture of the same data (plan 5.4), which for a perio chart is the normal chart
  without the perio marks. That is very unlikely in software rendering, and it is logged; it is noted here for TC-8, which owns the hosts.

### 1.4 The mirrored rows' light (found by the first recording)

The first recording showed the two lingual rows flat grey: lit by the emissive part alone (the colour times 0.6), with no diffuse or
specular light. WPF lights a model whose transform mirrors (negative determinant) as if its normals pointed the other way. The mesh cache's
reversed mesh, used only for such transforms (the lingual perio rows and their implants; the normal chart has none), now carries the normals
negated as well as the triangles reversed, and the lingual rows get DirectX's light (DirectX transforms the normals by the world matrix, and
its handedness flip and the row's flip cancel, so it lights those rows like the facial ones). `WpfMeshTests` pins the negated normals, and the
perio parity probes check the lightness of every tooth of every row against DirectX. No picture of the normal chart changed.

### 1.5 Files

- `SparksToothChart/Wpf3D/ToothSceneBuilder.cs` (the perio scene's 2D layer and the legend's term), `ToothMeshCache.cs` (1.4),
  `ToothChartWrapper.cs` (perio mode on `Wpf3D`, `PerioUseRecessionTerm`), `ToothChartData.cs` (the option's field).
- `WpfControlsOD/UI/ToothChart/ToothChartRelay.cs` (`ApplyGraphicalPerioMode`, `IsPerioRecessionTerm`, comments).
- Tests (section 2); the plan (TC-7's status, 5.2, L-6); `docs/release-notes/next.md`.

## 2. Tests

### 2.1 What was added

- **Scene builder** (`PerioSceneTests`): each row's decorations after the previous row's and in DirectX's order (millimetre lines 0, 3, 6, 9;
  the marks of the row's own sites, mobility only in facial rows; GM, then CAL, then MGJ); then the centre line, the numbers 1-32 with no
  highlight and no primary letter, and the legend last with its nine words; nothing of the normal chart; every polygon opaque; the perio
  light. For one tooth: the order mobility, furcations, probing, bleeding, suppuration per site; the furcation and probing colours at and
  below their red limits; grade 1, 2 and 3 as 2, 3 and 3+1 polygons; a bar 0.6 mm wide and as long as the depth; no bar for 0 or -1. The
  droplet's tip up on the maxilla and down on the mandible. The centre line at baseY, black, across the chart; numbers above and below it.
  Mobility once per tooth, in the facial rows, centred. The legend's term: off, on, on with hyperplasia, and an MGJ over 100 that is not
  hyperplasia; the line's colour unchanged. The legend's colours with custom colours and a furcation red limit of 2. The lingual rows and
  their implants reversed, the facial ones not, and each implant in its arch's two rows. The wrapper allows perio on `Wpf3D`, keeps the
  option through `ResetTeeth`, and refuses perio on Simple 2D. `ApplyGraphicalPerioMode` sets the option before choosing the engine.
- **Images** (`WpfImageTests.ChartScenes_MatchTheWpfBaselines`): every perio scene at 649x696 on a new wrapper (each scene's own, because
  `ResetTeeth` keeps perio measures), and the print and the 750x1000 save of `perio-full-exam`, `perio-missing-and-implants` and
  `perio-recession-term` (`Baselines/wpf-net48/perio-print`, `perio-save`), made by `PerioPrintout`, a line-for-line copy of
  `FormPerioGraphical.RenderPerioPrintout` with synthetic texts drawn around the wrapper's `GetBitmap` (the print into the page's
  750x1000 margin bounds with the default settings, the save into a new white 750x1000 bitmap with high-quality compositing and smoothing).
  `PerioPrintoutTests` keeps the copy in step with the window's source (the texts, fonts, the scaling, the margins, the save's settings, the
  wrapper's 649x696). The image tests never reference the program (plan, appendix item 3).
- **Scenes**: five new perio scenes (`HasLegacyRecording=false`, added after every older scene): `perio-recession-term`,
  `perio-recession-term-hyperplasia`, `perio-implants-both-arches`, `perio-red-limits` (probing red from 6 mm, furcations from grade 3) and
  `perio-custom-colours` (all nine colours changed). `ChartScenes.Prepare` turns the Recession option off for every perio scene first.
- **Parity probes** (`PerioParityTests`, `Support/PerioParityProbes.cs`), against TC-0's DirectX pictures of the ten recorded perio scenes:
  the colour at the visible surface of every tooth of every row (facial or lingual surface, 3x3 mean; at a missing tooth, tooth against
  background), each implant, every probing bar, droplet, grade 2 and 3 furcation (base and fill) and GM, CAL and MGJ segment (where DirectX
  shows the line on top), and the legend's marks, by section 7's rules (lightness within 20 points, hue within 12 degrees). A mark or a line
  is sampled in the WPF picture at the pixel of the 3x3 nearest DirectX's colour, because the WPF chart smooths the sloped lines drawn after
  the marks and DirectX does not. The ink of every number, legend word and red mobility number within one pixel of DirectX's. The centre line
  and the legend's line samples and bars fill exactly DirectX's rows (and the bars its columns).
- **Changed**: `WpfRendererTests.Engine_Wpf3D_AllowsPerio_AndSwitchesBack` (was `..._KeepsPerioDirectXOnly_...`); `WpfMeshTests` (the
  reversed mesh's normals); the gallery writes the perio pictures with their parity captions, and for G3 the print and the save.

### 2.2 The baselines

21 new WPF baselines: the 15 perio scenes at 649x696 and the six printouts; `manifest.txt` lists them. Every other WPF picture of the
recordings had exactly its committed pixels (276 of 276), so none is rewritten. Each new picture was repeatable, and the perio scenes also
gave the same pixels in a fresh render of both layers.

### 2.3 Targeted runs (build PC, `.git/od-tools/test-results/`)

- **Builds** through the lane (`OpenDental.Local.slnf`, Debug x86): the cold build of the fresh worktree and the final full build, 0
  errors each; single-project builds in between.
- `tc7-record-1.txt`: the first recording (`OPMS_RECORD_WPF_CHART=1` into a scratch folder, `OPMS_PARITY_FRESH=1`). It recorded every WPF
  picture (276 with exactly their committed pixels, 21 new) and found the lingual rows' light (section 1.4: 312 of 640 tooth probes, all in
  the lingual rows, and the two lingual implant probes failed), six line and three mark probes that a smoothed edge tinted (hence the
  nearest-pixel sample of section 2.1), and an expectation of the one-tooth test that read a measure's sites in the wrong order.
- `tc7-record-2.txt`: the recording after the fix (`c6d86c408`), from which the 21 baselines were taken (`195132f19`): the perio parity probes
  7178 of 7178, the text boxes 428 of 428 within a pixel (320 numbers, 90 legend words, 18 red mobility numbers), the centre line and the
  legend's samples 160 of 160 exact. The two failures left were the tests' own expectations (the fewest implant probes, a measure's site
  order), fixed in that commit.
- `tc7-final.txt` (at `195132f19`, after the full build, with gallery G3 written by the same run): ToothChartImageTests 227: 224 passed, 0
  failed, 3 skipped (the legacy recorder and its comparison, and the opt-in on-screen times); BPERIOTests 55, P262PerioTests 36 and NoHqTests
  57 passed. 1064 s for the chart's assembly.
- The call-log and database tests of ToothChartImageTests use their own scratch database (`ChartTestDb`, named `tc2_` with the run's time and
  process, dropped at the end), and BPERIOTests and P262PerioTests theirs, as before this package; TC-7 adds no database test.
- `tools/Test-NoEolFlips.ps1` against `main`: no file changed its line ends.

## 3. Gallery G3

On the build PC, outside the repository like G1 and G2, in the main checkout's `.git/od-tools/test-results/`:

- **`tc7-gallery-G3/g3-tc7.html`** (also alone with its 32 pictures in `tc7-gallery-G3-tc7-scenes.zip`, 8 MB): the page to show the owner,
  in G2's table layout. Its head says what TC-7 draws, the two Open Dental 24.4 changes, the od-unknown items and what differs from DirectX
  on purpose. One row per perio scene: today's DirectX picture (TC-0's recording; the smoothed one of `perio-full-exam` beside it) and the
  WPF picture now with its parity caption; the five new scenes are marked; `perio-full-exam`, `perio-missing-and-implants` and
  `perio-recession-term` also show the print and the saved image. Made by `tc7-make_g3_tc7.py` in the same folder from the gallery's own
  `index.html`.
- **`tc7-gallery-G3/index.html`** (the whole folder in `tc7-gallery-G3.zip`, 129 MB, zipped with `tools/New-ToothChartGallery.ps1
  -NoTestRun`): the full gallery as G2 was made (WriteGallery with `OPMS_CHART_GALLERY_NAME=G3` and the approved pictures of `main`, whose
  WPF baselines are those of `3620550cb`). 291 WPF pictures: the 276 approved ones all the same as approved, and the 15 perio pictures new;
  every DirectX parity caption passes (264 of 264, the ten recorded perio scenes' 7178 probes among them).
- The owner's answer goes into `docs/decisions/owner-questions.md` with the commit and the date (plan section 8). TC-7 merges only after it.

## 4. Launch check L-6, for a person

On the build PC's desktop, with a DEBUG build of this branch and the developer switch `OPMS_TOOTHCHART_WPF=1` set for the user (TC-2), on a
scratch database with synthetic patients only (never an office's data). Each step says what to do, what should happen, and what to write
down. Do every step once at 100% display scale and once at 150%.

1. **Before.** Start the program on the scratch database. Open a synthetic patient with a perio exam (or make one: Chart module, Perio Chart,
   Add, then enter probing depths, gingival margins, some bleeding (B) and suppuration (S) sites, an MGJ row, a furcation on a molar and a
   mobility). *Expected:* the Perio Chart opens as before.
2. **Open it.** In the Perio Chart, select the exam and click Graphical. *Expected:* the Graphical Perio Chart opens on the WPF chart (smooth
   teeth; the window is not refused as "you must first select DirectX"): four rows of teeth (upper facial, upper lingual, lower lingual, lower
   facial), all lit alike (the lingual rows are not darker), the probing bars green and red from the red limit, the droplets, the furcation
   marks on the molars, the purple, blue and orange lines, the mobility number on the crown, the numbers between the rows, the black centre
   line and the legend at the bottom. Write down anything that differs from the DirectX chart of the same exam (open it once without the
   switch to compare).
3. **Setup Colors.** Click Setup Colors, change the probing colour and the CAL colour, OK. *Expected:* the chart redraws at once with the new
   colours, the legend's samples too. Put the colours back.
4. **Recession.** Close the graphical chart. In the Perio Chart, Setup: tick "Use the term "Recession" (reverts to Gingival Margin when
   hyperplasia is charted)", OK. Open Graphical again. *Expected:* the legend reads "Recession" where it read "Gingival Margin"; the line and its
   colour are the same. Setup Colors: *Expected:* its label reads "Recession" too. Close it. On an exam with a gingival margin entered with
   Ging Marg + (hyperplasia), open Graphical: *Expected:* the legend reads "Gingival Margin". Untick the option again.
5. **Implants.** Give the patient a completed implant on a missing tooth (Chart module), open Graphical: *Expected:* the implant shows in both
   rows of its arch (upside down on the upper arch). Hide the implant procedure's graphics (Hide Graphics in the procedure, as Open Dental 24.4
   treats a removed implant), open Graphical again: *Expected:* no implant on that tooth.
6. **Print.** Click Print and look at the preview (do not print to paper unless you want to). *Expected:* the title, the clinic, the patient's
   name, DOB, the exam date and the chart below them, filling the width, with the WPF chart's smooth teeth.
7. **Save to Images.** Click Save to Images (a Tooth Charts image category must exist: Setup > Definitions > Image Categories). *Expected:*
   "Saved."; in the Imaging module the new 750x1000 image shows the same page as the print preview.
8. **Write down:** for each step, as expected or what differed, with a screenshot of anything that differed (synthetic data only).
