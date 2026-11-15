# Port note: TC-3c (the TC-3 look follow-up: the "W", unerupted teeth and the centre line)

Package TC-3c of the WPF tooth chart track ([docs/specs/wpf-tooth-chart-plan.md](../specs/wpf-tooth-chart-plan.md), section 10's "Open
Dental parity (2026-11-14)", table "TC-3 look follow-up", rows PT-1, LOOK-2 and LOOK-4; sections 2, 3, 5, 7 and 8): branch `chart/tc3c-1`
from `main` (`7b1c76c55`). **Status: done; waiting for the owner's approval of gallery G4** (section 3), after which it merges. Nothing was
downloaded, no program was started and no program file outside `SparksToothChart` was edited; the tests rendered offscreen and ran in the
lead's desktop session as the chart's other tests do.

The owner's rule of 2026-11-14 applies: every tooth chart change looks and behaves as the latest Open Dental (26.2 stable) shows publicly,
with no fork-only rules. The three rows come from the audit of that day (`.git/od-tools/briefs/tc-od-parity.md` in the main checkout, rows
PT-1, LOOK-2 and LOOK-4, and its critic's notes); the screenshots it names are in `.git/od-tools/briefs/odimg/` and were looked at again for
this package (`Capture17.png`, the 2024 chart; `tb.png`, the 26.2 TextBelow image; `pedo.png`, the 2023 mixed-dentition image). There is no
`docs/port-notes/TC-3.md`: TC-3's notes are the plan's section 2 ("TC-3 (2026-11-09 ...)" and its four fix rounds).

Commits: `948463271` (the drawing and the scene, parity and renderer test changes), `b26fa3e47` (the 276 re-recorded baselines),
`9cee85491` (the parity allowances and the blend test), `b669fa134` (the blend test's share from the build PC's run), then the docs
commit.

## 1. What each row asks, and how it is built

### 1.1 PT-1: the "W" plain, in the procedure's colour

- **Open Dental:** in its DirectX 11 screenshots the watch mark is plain dark text, regular weight, with no white outline, in the place the
  fork already uses: above a maxillary apex, below a mandibular one
  (https://i0.wp.com/opendental.blog/wp-content/uploads/2024/08/Capture17.png?ssl=1, "W" above 8 and below 27;
  https://i0.wp.com/opendental.blog/wp-content/uploads/2026/10/image-6.png?ssl=1, "W" under "UCF" below 24/25).
- **Built (`ToothSceneBuilder.AddWatches`):** one text item per watched tooth, "W" in the procedure's colour (`colorWatch`), Arial 15/6 times
  the ratio at weight 400 (`GetFontWatch`, unchanged), at DirectX's place. The white Times New Roman 19/7.4 ultra-bold "W" that DirectX 9
  drew first as an outline (`... (outline)` items) is gone, and with it `GetFontWatchBig` and `SceneBuilding.FontWatchBig`. TC-5's paint
  text, which stacks beside the "W" (`GetWatchPositions`), keeps its place.
- **Caveat kept (od-unknown):** the colour of a TP "W" next to a Condition "W" needs a screenshot; the fork keeps the procedure's colour, as
  every engine did.

### 1.2 LOOK-2: unerupted teeth half-transparent

- **Open Dental:** in its 2023 mixed-dentition screenshot (DirectX 11, https://i0.wp.com/opendental.blog/wp-content/uploads/2023/07/image-4.png?ssl=1)
  the permanent teeth under the shown primary teeth are pale and translucent: the light cyan background shows through them, while the
  primary teeth are opaque.
- **The alpha, measured: 0.63, built as 161 of 255** (`ToothSceneBuilder.UneruptedAlpha`). The enamel colour is 250,250,240, so a lit
  enamel pixel has equal red and green under the white light, whatever its shading. Over the screenshot's background 233,254,254 a
  translucent pixel is alpha times the tooth plus (1 - alpha) times the background, so its green minus red is (1 - alpha) times 21, with
  the tooth's own shading cancelled. Over the unerupted crowns (the upper and lower molars' facial and occlusal views, the upper incisors;
  background pixels left out) green minus red averages 7.78 over 18090 pixels: alpha 0.63 (0.635 on the molars, 0.686 on the incisors,
  0.601 on the lower occlusal views). The image is an 8-bit palette PNG, so single pixels are quantized (green minus red takes the values
  6 to 10, mostly 7 and 9) and the opaque teeth of the same picture show red and green within about 2 of each other: the value is good to
  about 0.05. The same pixels also show that the colour under the translucency is not halved: at 0.63 the tooth behind a crown pixel is
  about 190 of 255 (up to 209), far from DirectX 9's half colour (125 at most), so the WPF chart keeps the tooth's own colour. **Marked
  od-unknown-refined:** a Set Mixed Dentition screenshot of Open Dental 26.2 settles the exact value.
- **Built:**
  - `GetGroupColor`: an unerupted tooth (ShiftO below -10, DirectX's rule) keeps its paint colour's RGB at alpha 161; every other group is
    opaque as before. Both views are translucent (the occlusal view of a permanent tooth is still not drawn under a shown primary tooth).
  - `AddTooth` puts translucent items into `SceneBuilding.ListItems3DTranslucent`, and `AddChartScene` appends them to the 3D layer after
    every opaque item (teeth and implants): WPF draws its models in order with depth testing and does not sort transparent ones (plan 5.3),
    so the unerupted teeth blend over the background and over the opaque teeth in front of which they lie, and an opaque tooth in front of
    them hides them.
  - `ToothChartRenderer.CreateMaterial`: the diffuse, specular and emissive brushes carry the colour's alpha. The diffuse pass blends at that
    alpha and WPF's additive specular and emissive passes add only that part of their light, so the tooth's whole lit colour is blended at
    0.63 (the new renderer test checks it pixel by pixel: `TranslucentTooth_IsItsOpaquePictureBlendedOverTheBackgroundAtItsAlpha`). With
    alpha 255 every brush is the same as before, so no opaque picture changed by it (the 21 perio pictures kept their bytes).
  - **The perio chart is unchanged** (`GetGroupColorPerio`, DirectX's opaque half RGB): `FormPerioGraphical` moves no tooth, so the graphical
    perio chart never has an unerupted tooth, and how Open Dental 26.2 draws it is not public (PERIO-1, od-unknown).
- **What it looks like:** over the module background the unerupted teeth are pale and the background shows through; on the printouts'
  white background they are faint. A translucent tooth whose own front faces overlap (a root seen through a crown) can blend twice there,
  as any unsorted transparent mesh does; the renderer test allows for it (measured 88% of the tooth's pixels within 4 of the exact blend,
  mean -1.07; the test wants 80% and a mean within 2).

### 1.3 LOOK-4: the centre line in the chart's text colour

- **Open Dental:** in the 2024 and 26.2 DirectX 11 screenshots the centre line between the arches is dark like the numbers
  (https://i0.wp.com/odtechblog.wordpress.com/wp-content/uploads/2026/08/image-6.png?ssl=1;
  https://i0.wp.com/opendental.blog/wp-content/uploads/2024/08/Capture17.png?ssl=1; the 2023 mixed-dentition image shows it black too).
- **Which colour definition, and why:** the chart's text colour, `ToothChartData.ColorText`, the colour the numbers are drawn in. It is the
  "Text" chart graphic colour (Definitions, Chart Graphic Colors, item 11, `ControlChart.FillToothChart`: `ColorText=listDefsChartGraphicColor[11]`)
  in the Chart module and the big chart, and "Text on TPs" (item 15) on the Treatment Plan printouts and sheets, which set the printout
  colours. Reasons: in every public screenshot the line has the numbers' colour; the numbers are the only other thing DirectX draws in the
  text colour, right beside the line; the perio chart already draws its 0 mm lines in `ColorText` (TC-7, DirectX's `DrawPerioRow`); and no
  other definition (background, highlight) fits a dark line on Open Dental's default colours. To settle it beyond doubt, a 26.2 screenshot
  with the Text colour changed (the audit's note).
- **Built (`AddNumbersAndLines`):** the line's colour is `ColorText` made opaque (`SourceCentreLine`, "centre line"); its place, width
  (0.32 mm) and pixels stay DirectX's (TC-3's crisp rectangle, exactly DirectX's rows). On the printouts the line was white on white and is
  now seen. The perio chart's own black centre line is unchanged.
- **Caveat kept (od-unknown):** the 2026 Big-layout screenshot shows no centre line between the number rows, so colour and presence there
  are unsettled (a Big-layout check); the highlight boxes are unchanged.

### 1.4 The Simple 2D chart

The Simple 2D chart (`ToothChart2D.cs`) is Open Dental's own 2D chart code (the GPL 24.3 source the fork came from), and no public 26.x
picture of Open Dental's 2D chart shows these items otherwise, so nothing changes there; it already draws all three as Open Dental's 2D
chart does:

- **"W":** `RenderToothWatch` (`ToothChart2D.cs:304-325`) draws the "W" in the procedure's colour in the control's font with no outline;
  its own comment says why: "Drawing a white silhouette around the colored watch W doesn't make sense here because unerupted teeth do not
  change color in this chart." Already PT-1's look.
- **Unerupted teeth:** the 2D chart draws its teeth from one picture (`pictBox.Image` in `ToothChart2D.resx`) and does not change an
  unerupted tooth (the same comment). Open Dental's 2D chart has no translucent tooth to draw, so LOOK-2 does not apply.
- **Centre line:** it is part of that picture, a dark line drawn into the bitmap, not a chart colour (`OnPaint` draws the picture, then the
  watches, paint text, numbers, drawings and free text). Already dark, as LOOK-4 asks; it does not follow the Text definition in Open
  Dental's 2D chart either.

### 1.5 The DirectX and OpenGL engines, and the parity probes

The DirectX and OpenGL engines are not changed (TC-9a retires them); their recordings (`Baselines/legacy`) are untouched. The parity probes
keep comparing what they compared, and what now differs from the DirectX recording on purpose is listed per scene with its reason:

- **Surface probes (`ParityProbes`):** each probe of an unerupted tooth's surface is marked (`ParityProbePoint.IsUnerupted`), and a new
  kind of allowance covers only those probes of one scene (`ParityAllowance.IsUneruptedOnly`, reason `ReasonUnerupted`). Fifteen scenes,
  both sizes: `primary-dentition`, `primary-dentition-fdi`, `primary-dentition-palmer`, `mixed-dentition`, `mixed-dentition-haderup`,
  `unerupted-third-molars`, `missing-and-hidden-primary`, `shifto-unerupted`, `crown-primary`, `extraction-primary`, `filling-primary`,
  `rct-primary`, `sealant-primary`, `text-watch-primary`, `select-primary-letter` (496 probes, each about 30 to 47 lightness points lighter
  than DirectX's half-colour tooth). Every other probe of those scenes still has to pass, and an allowance that covers nothing fails the
  test. No tolerance changed.
- **Text boxes:** 30 numbers beside an unerupted tooth get one allowance each (`ReasonUneruptedNextToNumbers`), in the style of TC-5's:
  `unerupted-third-molars` 1, 16, 17 and 32 (both sizes); `missing-and-hidden-primary` 5, 7, 10, 12, 20, 29, D and G at 410x307 and 5,
  12, 20, 29, D and G at 500x370; `shifto-unerupted` 6, 11, 22 and 27 (both sizes). DirectX 9's dark tooth counts as ink in the number's
  search box and grows DirectX's ink box by up to 3 pixels; the WPF chart's pale tooth does not (looked at in both pictures, number 1 of
  `unerupted-third-molars` at 410x307). The "W" boxes: the outline was never compared, and the coloured "W" still is (28 of 28 within a
  pixel).
- **Centre line (`CentreLineAndHighlightBoxes_FillThePixelsDirectXFills`):** DirectX's line is found by its white, the WPF line by its
  colour (the text colour), and their rows must still be the same at five columns (121 of 121 exact); the printouts, where DirectX's white
  line is invisible on white, are still left out.

### 1.6 Files

- `SparksToothChart/Wpf3D/ToothSceneBuilder.cs`: `UneruptedAlpha`, `GetGroupColor` (translucent), `GetGroupColorPerio` (DirectX's rule),
  `AddTooth` and `AddChartScene` (the order), `AddWatches` (no outline), `AddNumbersAndLines` and `SourceCentreLine` (the colour);
  `GetFontWatchBig` and `FontWatchBig` removed.
- `SparksToothChart/Wpf3D/ToothChartRenderer.cs`: `CreateMaterial` and `CreateBrush` keep the colour's alpha.
- `SparksToothChart/Wpf3D/ToothScene.cs`: the doc of `ToothSceneItem3D.Color`.
- Tests: `WpfSceneTests.cs`, `WpfRendererTests.cs`, `ParityProbeTests.cs`, `Support/ParityProbes.cs`, `WpfImageTests.cs` (G4's note),
  `Scenes/ChartScenes.cs` (the descriptions of `unerupted-third-molars` and `shifto-unerupted`), and the WPF baselines.

## 2. Tests

### 2.1 What was added or changed

- `WpfSceneTests`: `EveryChartScene_HasOpaque3DColoursAndPolygons_ButUneruptedTeeth_AfterTheOpaqueItems` (every 3D colour is opaque or at
  `UneruptedAlpha`, never an implant, and no opaque item follows a translucent one; over 1000 translucent items in the scenes);
  `SemiTransparentColours_ComeOutOfEveryBuilderColourPath_Opaque_OrAtTheUneruptedAlpha` (the alpha is the builder's, whatever the colour's
  own; the perio chart's unerupted tooth stays opaque at half RGB); `UneruptedTeeth_AreTranslucentAtTheirOwnRgb_AfterEveryOpaqueItem` (both
  views, after an implant, the -10 limit, the perio rule); `Items2D_ComeInDirectXOrder` (no outline item; one "W" per watched tooth, Arial
  400 in the procedure's colour; the centre line in the numbers' colour); `CentreLine_IsInTheChartTextColour_Opaque`.
- `WpfRendererTests.TranslucentTooth_IsItsOpaquePictureBlendedOverTheBackgroundAtItsAlpha`: one tooth's facial view rendered opaque and at
  the alpha over the module background; the translucent picture is the opaque one blended at 0.63 (at least 80% of the tooth's pixels
  within 4 per channel, 88% measured; mean difference within 2).
- `ParityProbeTests` and `Support/ParityProbes.cs`: section 1.5.

### 2.2 The baselines

**276 WPF pictures re-recorded, every picture of the normal chart; the 21 perio pictures (649x696, `perio-print`, `perio-save`) kept
their bytes.** Recorded in `tc3c-record-1` into a scratch folder; every re-recorded picture was repeatable and the same in a fresh render
of both layers; the manifest is that of the recording. The reasons:

- **LOOK-4, all 276:** every picture of the normal chart draws the centre line, now in the text colour (dark on the module colours, black
  on the printouts' white, yellow in `colour-custom-chart-colours`). The 135 scenes at 410x307 and at 500x370: `bridge-anterior`,
  `bridge-dark-cn`, `bridge-dark-statuses`, `bridge-dark-tp`, `bridge-light`, `bridge-light-eo-r`, `bridge-on-present-teeth`,
  `buildup-under-moved-neighbour`, `colour-by-provider`, `colour-code-override`, `colour-custom-chart-colours`, `colour-tpi-and-unhandled`,
  `crown-dark-statuses`, `crown-light-statuses`, `crown-on-missing-tooth`, `crown-primary`, `denture-both-light`, `denture-dark-eo-r-cn`,
  `denture-light-tp-ec-r-cn`, `denture-lower-dark`, `denture-partial-dark`, `denture-partial-light`, `denture-upper-dark`,
  `denture-upper-on-present-teeth`, `drawing-circles`, `drawing-dense`, `drawing-edges`, `drawing-erased`, `drawing-recoloured`,
  `drawing-segments`, `empty-fdi`, `empty-haderup`, `empty-palmer`, `empty-universal`, `extraction-anterior`, `extraction-completed`,
  `extraction-completed-graphic`, `extraction-completed-graphic-primary`, `extraction-completed-graphic-under-implant`,
  `extraction-completed-graphic-under-pontic`, `extraction-primary`, `extraction-statuses`, `filling-dark-anterior-combinations`,
  `filling-dark-anterior-single`, `filling-dark-posterior-combinations`, `filling-dark-posterior-single`, `filling-dark-statuses`,
  `filling-light-anterior`, `filling-light-posterior`, `filling-light-statuses`, `filling-mirror-pairs`, `filling-primary`,
  `hidden-premolars`, `implant-anterior`, `implant-on-present-tooth`, `implant-statuses`, `implant-with-crown`,
  `missing-and-hidden-primary`, `missing-anterior`, `missing-many`, `missing-posterior`, `mixed-dentition`, `mixed-dentition-haderup`,
  `movements-combined`, `ortho-brackets`, `ortho-elastics`, `ortho-mode`, `ortho-wire-in-bracket`, `ortho-wires`, `postbu-statuses`,
  `postbu-with-crown`, `primary-dentition`, `primary-dentition-fdi`, `primary-dentition-palmer`, `rct-on-missing-tooth`, `rct-primary`,
  `rct-statuses`, `rct-with-post-and-crown`, `restored-adult`, `retained-root`, `retained-root-statuses`, `rotate-minus`, `rotate-plus`,
  `sealant-premolars`, `sealant-primary`, `sealant-statuses`, `select-fdi`, `select-missing-and-hidden`, `select-one`,
  `select-primary-letter`, `select-range`, `select-several`, `select-whole-arch`, `shiftb-minus`, `shiftb-plus`, `shiftm-fractions`,
  `shiftm-minus`, `shiftm-plus`, `shifto-minus`, `shifto-plus`, `shifto-unerupted`, `size-big-window`, `size-sheet-tall`, `size-sheet-wide`,
  `space-maintainer-quadrant`, `space-maintainer-range`, `space-maintainer-statuses`, `space-maintainer-tooth`, `text-all-kinds`,
  `text-below-cut-to-fit`, `text-below-statuses`, `text-below-with-watch`, `text-label-long`, `text-label-moved`, `text-label-past-edge`,
  `text-labels-at-edges`, `text-labels-over-teeth`, `text-other`, `text-other-eo-r`, `text-other-moved-and-missing`,
  `text-other-with-watch`, `text-watch`, `text-watch-on-missing-tooth`, `text-watch-primary`, `tipb-minus`, `tipb-plus`, `tipm-minus`,
  `tipm-plus`, `treatment-plan`, `unerupted-third-molars`, `veneer-lower`, `veneer-posterior`, `veneer-statuses`, `x-on-implant-tooth`,
  `x-under-moved-neighbour`; and the six size pictures `size-big-window` and `text-all-kinds` at 926x858, `size-sheet-wide` and
  `text-all-kinds` at 640x320, `size-sheet-tall` and `text-all-kinds` at 360x480.
- **PT-1 as well (the "W" without its outline), 7 scenes at both sizes:** `text-watch`, `text-watch-on-missing-tooth`,
  `text-watch-primary`, `restored-adult`, `text-below-with-watch`, `extraction-completed-graphic-under-pontic`, `text-other-with-watch`.
- **LOOK-2 as well (translucent unerupted teeth), 18 scenes at both sizes:** `primary-dentition`, `primary-dentition-fdi`,
  `primary-dentition-palmer`, `mixed-dentition`, `mixed-dentition-haderup`, `unerupted-third-molars`, `missing-and-hidden-primary`,
  `shifto-unerupted`, `filling-primary`, `crown-primary`, `extraction-primary`, `rct-primary`, `sealant-primary`, `text-watch-primary`,
  `select-primary-letter`, `extraction-completed-graphic-primary`, `text-below-cut-to-fit`, `text-other-moved-and-missing`. The
  descriptions of `unerupted-third-molars` and `shifto-unerupted` now say both looks, so their `.txt` changed in that line too.

The legacy recordings (DirectX, OpenGL, Simple 2D, smoothed), the Simple 2D baselines and the call logs are untouched.

### 2.3 Targeted runs (build PC, `.git/od-tools/test-results/`)

- **Builds** through the lane: the worktree's first build, single-project builds of `ToothChartImageTests`, and the final full build of
  `OpenDental.Local.slnf` (0 errors).
- `tc3c-record-1.txt`: the recording (`OPMS_RECORD_WPF_CHART=1` into the scratch folder `tc3c-record-1`, `OPMS_PARITY_FRESH=1`) at
  `948463271`. It recorded 297 pictures (276 changed, 21 with exactly their committed pixels) and found what section 1.5 allows: 496
  surface probes of unerupted teeth in 15 scenes and 30 number boxes beside them. The other failures were the baselines' own (determinism
  and the gallery's mark against the branch baseline, both against the old baselines). BINATests 31, BPERIOTests 55, NoHqTests 94 passed.
- `tc3c-final.txt` (at `9cee85491`, after the full build, with gallery G4 written by the same run): ToothChartImageTests 229: 225
  passed, 1 failed, 3 skipped. The parity probes 16852: 16356 pass and 496 differ and are allowed (section 1.5), 0 fail; the text boxes
  8633: 47 allowed (17 of TC-3 and TC-5, 30 of TC-3c), 0 off; the centre lines 121 of 121 exact; the WPF gate 297 of 297 pictures with
  exactly their `.txt`'s pixels. The one failure was the new blend test's first guess of its share of exact pixels (90%; measured 1273 of
  1442 tooth pixels, 88%, within 4 of the exact blend, mean difference -1.07): the blend is right, and the rest are where the tooth's own
  front faces overlap. `b669fa134` sets the share to 80% from that measurement.
- `tc3c-final-2.txt` (at `b669fa134`, after the full build of `OpenDental.Local.slnf`, 0 errors): **ToothChartImageTests 229: 225 passed,
  0 failed, 4 skipped** (the legacy recorder and its comparison, the gallery writer without `OPMS_CHART_GALLERY`, and the opt-in on-screen
  times); **BINATests 31, BPERIOTests 55 and NoHqTests 94 passed.**
- The call-log and database tests of ToothChartImageTests use their own scratch database as before; TC-3c adds no database test.
- `tools/Test-NoEolFlips.ps1` against `main`: no file changed its line ends.

## 3. Gallery G4

On the build PC, outside the repository like G1 to G3, in the main checkout's `.git/od-tools/test-results/`:

- **`tc3c-gallery-G4/g4-tc3c.html`** (also alone with its pictures in `tc3c-gallery-G4-tc3c-scenes.zip`, 86 MB): the page to show the
  owner, in G2's table layout. Its head says what each row changes, the measured alpha and the Text definition. One row per changed scene
  (135 scenes, 276 pictures), the scenes with a "W" (PT-1) first, then those with unerupted teeth (LOOK-2), then the rest (LOOK-4 only);
  each row has today's DirectX 9 picture (TC-0's recording), the WPF picture before (the approved one, `main` at `7b1c76c55`, as G3 left
  it), the WPF picture now with its parity caption, and the Open Dental screenshot it follows (`od/`: `Capture17.png` and `tb.png` for
  PT-1 and LOOK-4, `pedo.png` for LOOK-2, each with its URL). Made by `tc3c-make_g4_tc3c.py` in the same folder from the gallery's own
  `index.html` and `ChartScenes.cs`; the approved pictures were copied into the gallery's `approved/` from `main`'s baselines.
- **`tc3c-gallery-G4/index.html`** (the whole folder in `tc3c-gallery-G4.zip`, 175 MB, zipped with `tools/New-ToothChartGallery.ps1
  -NoTestRun`): the full gallery (WriteGallery with `OPMS_CHART_GALLERY_NAME=G4` and the approved pictures of `main`). 291 of 291 WPF
  pictures have exactly the pixels of this branch's baselines; the 276 of the normal chart differ from the approved ones (the centre line
  at least), the 15 perio pictures are the same; every DirectX parity caption passes (263 of 263). The render times: a refill with its
  picture 408 ms at 410x307 (median), a selection-only change 10.5 ms.
- The owner's answer goes into `docs/owner-questions.md` with the commit and the date (plan section 8). TC-3c merges only after it.

## 4. Od-unknown left

Kept as built until a screenshot of an Open Dental 26.2 install settles them:

- **LOOK-2's alpha (od-unknown-refined):** 0.63, measured from the 2023 screenshot (section 1.2); a 26.2 Set Mixed Dentition screenshot
  settles it, and whether the colour under the translucency is the tooth's full colour (the measurement says so) or a little darker.
- **PT-1's colour:** a TP "W" next to a Condition "W" (the procedure's colour is kept).
- **LOOK-4:** which definition the line follows (a screenshot with the Text colour changed), and the line in the Big layout (its 2026
  screenshot shows none between the number rows).
- The highlight boxes (LOOK-4's note), the graphical perio chart's teeth (PERIO-1) and the other audit rows the plan lists as od-unknown.
- The audit's critic's note on ortho mode: in its 2022 ortho screenshot the third molars without hardware are pale and translucent. Whether
  that is the unerupted look of LOOK-2 or a rule of ortho mode is open; TC-6 takes it (ortho mode is not changed here).
