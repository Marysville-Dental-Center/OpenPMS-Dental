# Port note: TC-6 (orthodontic hardware, ortho mode, retained roots, space maintainers)

Package TC-6 of the WPF tooth chart track ([docs/specs/wpf-tooth-chart-plan.md](../specs/wpf-tooth-chart-plan.md), the TC-6 row of
section 4; sections 2, 3, 5, 7 and 8; section 10's O-23 and O-24 and its "Open Dental parity (2026-11-14)" table TC-6, rows ORTHO-1,
ORTHO-2, RR-1 and SM-1): branch `chart/tc6-1` from `chart/tc3c-2` (TC-3c, which changes the same scene builder; the owner approved its
gallery G4 on 2026-11-15 and it is merged on `main` as `cd67aa44f`, so the branch's base is in `main`). **Status: done; waiting for the
owner's approval of gallery G5** (section 4), after which it merges.
Nothing was downloaded, no program was started and no program file outside `SparksToothChart` and the chart's relay was edited; the tests
rendered offscreen in the lead's desktop session, as the chart's other tests do.

**The owner's rule of 2026-11-14** ("Overall for tc changes, keep it the same as OD latest version"): every tooth chart change looks and
behaves as the latest Open Dental (26.2 stable) shows publicly, with no fork-only rules. The rule is the owner's confirmation of the parity
rows; the plan's O-23 and O-24 now say that the rows revised them under that rule, with the 2026-11-08 answers kept as history. The rows,
their sources and the critic's notes are in `.git/od-tools/briefs/tc-od-parity.md` (in the main checkout's `.git`, outside the repository);
the screenshots are in `.git/od-tools/briefs/odimg/`, and this package looked at each one itself and measured them (section 1):
`cap13.png` (Open Dental 2024, ortho mode, https://i0.wp.com/opendental.blog/wp-content/uploads/2024/08/Capture13.png?ssl=1),
`ortho9.png` (2022, ortho mode with brackets, NiTi wires and elastics, and its hardware list; ortho10.png is that patient's Ortho Chart
tab, no chart picture), `Capture17.png` (2024, the normal chart), `retained-chart.png`
(https://odtechblog.files.wordpress.com/2021/01/retained-chart.png) and `space-main-chart.png`
(https://odtechblog.files.wordpress.com/2021/01/space-main-chart.png).

Commits: `0260989ca` (spec: O-23 and O-24), `55879ffee` (the drawing and the relay), `da9aa92b4` (scenes and tests), `9ac978e8b` (the
brackets matte, after the first recording; G5's note), `bb6df788a` (the baselines), then the docs commit.

## 1. What each row asks, and how it is built

### 1.1 ORTHO-1: ortho mode, facial rows only

- **Open Dental:** "The odontogram displays the facial/buccal view of the teeth only" (https://www.opendental.com/manual/ortho.html); in
  Capture13 and ortho9 two facial rows close to the centre line, the Palmer numbers 8..1 1..8 above and below the line between them, and no
  occlusal rows. Palmer is in https://www.opendental.com/manual/graphicaltoothchart.html; the callers already switch Universal to Palmer in
  ortho mode (`ControlChart.FillToothChart`, `FormToothChartBig`), unchanged.
- **Enlarged or re-spaced (the critic's question, measured):** the brief said "enlarged"; the screenshots say the teeth keep their size.
  In Capture13 an upper central incisor is about 70 pixels from apex to edge, as in the normal chart (Capture17: 69), and the 16 teeth
  stand at the same x positions (the numbers 8 and 1 at the same columns). What changes is the rows' height: the crowns' edges are 22
  pixels from the centre line in Capture13 (upper 22, lower 21 to 23) against 68 and 70 in Capture17, so the rows move 46 pixels nearer
  the line, 46.5 on ortho9's molars (its incisors 43.5: that patient's teeth are moved). At 410 pixels across 130 mm (3.154 pixels a mm)
  that is **14.6 mm** (`ToothSceneBuilder.OrthoFacialShiftMm`). So "enlarged" is built as Open Dental's screenshots show it: re-spaced, not
  scaled (the task's "enlarged as Open Dental's ortho mode shows it").
- **Built:** `ToothChartData.IsOrthoMode` (set by `ToothChartWrapper.SetOrthoMode`, which the relay's `SetOrthoMode` now calls; ResetTeeth
  keeps it, as the callers set it before every refill). The scene builder draws no occlusal views in ortho mode and puts each facial view
  at `ToothSceneBuilder.GetTransYfacial` = DirectX's place moved 14.6 mm toward the line (`ToothChartData.GetTransYfacial` itself is
  unchanged: the OpenGL chart reads it). Everything drawn in a tooth's facial world follows (the X, root canal lines, build-up, implant,
  brackets); the watch band and the paint text move with the rows (they stay beyond the root tips, O-22); the numbers, the centre line,
  drawings and free text stay where they are. The rendered rows land where Capture13's do: the upper incisors' edges 22.5 pixels above the
  line, the lower 22 below (`ortho-mode-hardware` at 410x307).
- **Hit-testing follows the layout:** `ToothChartData.GetToothAtPoint` picks the upper teeth in the upper half and the nearest tooth across,
  so it needs no change: a click on any crown in ortho mode selects that tooth, at 410x307, 500x370 and 926x858, with and without moved
  teeth (`WpfOrthoTests.OrthoMode_HitTesting_FollowsTheLayout_EveryCrownSelectsItsTooth`, through `ToothChartMouse`).
- **Not a cut:** ortho mode can still be turned off (the Ortho tab's check box); the normal layout is unchanged.

### 1.2 ORTHO-2: brackets, wires and elastics

- **Open Dental:** in ortho9 and Capture13 each bracket is a small grey square on the crown's facial surface (9 by 7 pixels), with four
  darker tie wings (2-pixel blocks) above and below a slot, its middle about 13 pixels (4 mm) from the incisal or occlusal edge; the NiTi
  wires are one-pixel grey lines (C0C0C0) through the slots; the elastics thin pale-yellow lines (E7D887) from bracket to bracket, across
  the arches. "Show Graphics" draws the hardware (ortho.html; https://opendental.blog/channeling-the-chart-module/).
- **Built:** five boxes per bracket in the 3D layer, made with Helix Toolkit's `MeshBuilder.AddBox` (`ToothMeshCache.BuildBoxMeshSet`, one
  unit box, `ToothSceneShapes.Box`; each item scales it by its transform; TC-4's rules: `ToMesh().ToWndMeshGeometry3D(true)`, no
  interpolated strings): the base 2.8 x 2.2 mm in the hardware's colour, and four tie wings 0.65 x 0.8 mm in front of it at 0.7 of that
  colour, leaving a 0.6 mm slot. The bracket sits on the model's crown: its middle 4 mm from the crown's edge toward the root, its face on
  the crown's most facial point under it (`GetBracketPlacement`, per tooth model), and it takes the tooth's facial world, so it follows
  every movement and rotation and the mirrored teeth 9-24. It is lit like the teeth: a silver base comes out 178 (ortho9: 180), the wings
  about 125 (ortho9: 124). Only a crown that is drawn gets one (a shown tooth, a pontic, a crown on an implant; not a retained root).
- **Wires and elastics:** lines 0.32 mm wide (one pixel at 410x307, two at the big chart's 926x858) in the 2D layer, in their colours, in
  the painter's order, after the teeth's own 2D items and under the watches and numbers (`AddOrthoWires`). A wire in a bracket runs across
  the slot; a wire between brackets from the end of one slot to the facing end of the next; an elastic from the middle of one bracket to
  the middle of the other. The painter is unchanged (`ToothChartPainter.PaintOrthoHardware` splits a wire range into in-bracket and
  between-bracket segments; `ControlChart.DrawOrthoHardware` paints only with "Show Graphics" on, the chosen exam date and "Show Hidden").
- **In the normal layout too:** the hardware is drawn on the facial views whenever a caller paints it: the Chart module with "Show
  Graphics" on, and as before the big chart, sheets and the Treatment Plan printout (which always paint the patient's hardware).

### 1.3 RR-1: retained root

- **Open Dental:** in retained-chart.png tooth 9 shows only its root, filled in the status colour and shaded (188,0,0 in the middle, 147 to
  175 at the sides: lit), with its scalloped edge at the crown line, and in the occlusal row a small crescent of the same colour; "once the
  root's extraction is complete, the root graphic is gone" (https://opendental.blog/tips-tricks-33/).
- **Built:** a tooth with `IsRetainedRoot` (the relay's `SetRetainedRoot`, a no-op before) draws only its root, its cementum groups, in the
  status colour, lit like the teeth (cementum's specular 0.1), in both views. The crescent is not a drawing of its own: it is the root seen
  from occlusal, as the occlusal view's 110-degree turn shows it once the crown no longer covers it (the near side of the root, its back
  faces culled), and it comes out as retained-chart.png's crescent on maxillary 9 (`retained-root-extraction-planned`). An unerupted
  tooth's root is translucent beneath the opaque teeth like the rest of an unerupted tooth (LOOK-2). The sealant "S" is not drawn on a
  retained root (it has no crown).
- **A completed extraction hides it:** the root is drawn only on a tooth that is shown. Completing an extraction marks the tooth missing
  (missingteeth.html: "the associated tooth is automatically marked missing"), so the root is gone; with the completed-extraction graphic
  on, the X is drawn over the empty place (O-20). A planned extraction keeps the root, with the X over it.

### 1.4 SM-1: space maintainer

- **Open Dental:** in space-main-chart.png tooth 9 is missing and a thick solid bar in the TP colour crosses its space: 198,0,0 flat (the
  TP colour itself, unlit), 4 to 5 pixels high (about 1.5 mm), from the crown of 8 to the crown of 10 (tooth 9's 8.5 mm), its middle 24.1
  mm above the centre line (the facial row's line at 29 mm, so 5 mm toward the crowns): crown height. "Designed to not ever draw in the
  same space where a tooth already exists" (https://www.opendentalsoft.com:8085/forum/viewtopic.php?t=7522, as the brief quotes it).
- **Built (`AddSpaceMaintainers`):** for every permanent tooth's space with a space maintainer (on the permanent tooth, or on its primary
  tooth) and no tooth in it, a bar of the unit box, unlit in the procedure's colour (`ToothSceneItem3D.IsFlat`: an emissive-only material;
  the rendered bar is 197,0,0 for 198,0,0), across the space's width, 1.5 mm high and 1 mm deep at the depth of the teeth's middles, its
  middle 5 mm from the facial row's line toward the crowns (24 mm; in ortho mode on the moved row). Neighbouring spaces of the same colour
  make one bar. A tooth is in the space when it is shown and erupted (a retained root counts), a pontic, or a crown on an implant; a
  missing or hidden tooth and an unerupted one (its crown is under the gum) leave it empty, so a lost primary molar over an unerupted
  premolar gets its bar. The bar is behind the teeth's facial surfaces, so a neighbour moved into the space covers it.
- **Tooth, range and quadrant:** the painter is unchanged and sends each tooth (a range's teeth; a quadrant's UR 4-8, UL 9-13, LL 20-24,
  LR 25-29); a bar appears only across the missing ones, none over the present ones (the older scenes `space-maintainer-tooth`,
  `-quadrant` and `-statuses` paint only present teeth, so their pictures are unchanged).

### 1.5 The Simple 2D chart, DirectX and OpenGL

- **The Simple 2D chart draws none of it, as Open Dental's 2D chart:** Open Dental's own relay (its 24.3 source, the fork's first commit
  `be14fe1be`, `WpfControlsOD/UI/ToothChart/ToothChartRelay.cs` lines 323-330, 386-393, 443-450 and 470-477) sent `SetBracket`,
  `SetOrthoMode`, `SetRetainedRoot`, `SetSpaceMaintainer` and the wire and elastic calls only to Sparks3D (its DirectX 11 chart), "not
  supported" otherwise; and its Graphics page lists retained roots and space maintainers as unavailable with the older technology
  (graphics.html, as the brief quotes it). So `ToothChart2D` is unchanged: no hardware, no ortho layout (the callers' Palmer numbers
  still show), no retained root and no space maintainer. `SceneTests.Scenes_DrawDifferentPictures_InSimple2D` pins it (the hardware scenes
  draw the empty chart's pixels, ortho mode the Palmer chart's).
- **DirectX and OpenGL are not changed** (TC-9a retires them): they never read the new flags, and their recordings are untouched.

### 1.6 Files

- `SparksToothChart/ToothGraphic.cs` (`IsRetainedRoot`, `IsSpaceMaintainer`, `HasBracket` and their colours, cleared by Reset),
  `ToothChartData.cs` (`IsOrthoMode`, `ListOrthoWires`, deep in Copy), new `ToothChartOrthoWire.cs`, `ToothChartWrapper.cs` (the seven
  setters; ResetTeeth clears the wires).
- `SparksToothChart/Wpf3D/ToothSceneBuilder.cs` (the region "Orthodontics, retained roots and space maintainers (TC-6)", the facial view,
  the occlusal view, the watch band), `ToothScene.cs` (`ToothSceneShapes.Box`, `ToothSceneItem3D.IsFlat`), `ToothMeshCache.cs` (the Helix
  box), `ToothChartRenderer.cs` (the flat material).
- `WpfControlsOD/UI/ToothChart/ToothChartRelay.cs`: the no-ops call the wrapper.
- Tests: new `WpfOrthoTests.cs`; `Scenes/ChartScenes.cs` (15 new scenes), `SceneTests.cs`, `ParityProbeTests.cs`,
  `Support/ParityProbes.cs`, `WpfImageTests.cs` (G5's note); the WPF baselines.

## 2. Od-unknown left

Built as the closest documented reading, kept until a screenshot of an Open Dental 26.2 install settles each:

- **Ortho mode's row height:** 14.6 mm, measured at 410 pixels across; a 26.2 ortho-mode screenshot at a known size settles it to the
  pixel. Also: whether the watch "W" and paint text move with the rows (built: yes, beyond the root tips) and whether the sealant "S" shows
  (built: no, there is no occlusal row) - an ortho-mode screenshot with a watch and a sealant.
- **The pale third molars of ortho9:** they are unerupted teeth (moved more than 10 mm toward the root), drawn with LOOK-2's look; no
  ortho-mode rule fades them (Capture13's third molars are opaque). An ortho-mode screenshot with erupted third molars without hardware
  would confirm.
- **The bracket's exact shape and its place on primary teeth:** the 2.8 x 2.2 mm base and the wings are from ortho9's 9 x 7 pixels; a close
  26.2 screenshot of one bracket, and one on a primary tooth, settle them. The hardware's colours come from the hardware specs (the
  critic: colour-coding is in no source); a wire is drawn across a missing tooth's place (as the painter sends it).
- **Elastics over the occlusal row in the normal layout:** built over it (2D layer, under the numbers); ortho9 and Capture13 are ortho mode.
  A normal-layout screenshot with elastics settles it.
- **The retained root's occlusal look on a mandibular tooth:** retained-chart.png shows only maxillary 9's crescent; the mandibular
  occlusal view's 110 and 120 degree turns show more of the root (a small cone above the crown line). A screenshot of a mandibular
  retained root settles it. Also a retained root on a tooth marked missing without an extraction (built: hidden, as any missing tooth).
- **The space maintainer's bar in the primary dentition and across spaces of different colours:** built across the permanent tooth's
  space, joined only within one colour; a pedo screenshot settles it. Which statuses and engines draw RR and SM, and the paint-type check
  of procedurecodeedit.html, are the critic's open rows (plan section 10).
- Hidden hardware, Show Hidden and the exam-date picker stay as the callers had them (the critic's "missing" note): the chart draws what it
  is given.

## 3. Tests and baselines

### 3.1 What was added or changed

- **New `WpfOrthoTests`** (9 tests): ortho mode draws no occlusal view, moves every facial item by 14.6 mm toward the line at the same size
  and x, and leaves the 32 Palmer numbers and the centre line exactly where they were; hit-testing: the facial centre of every crown, in two
  ortho scenes at three sizes, gives its tooth through `GetToothAtPoint` and a `ToothChartMouse` click; brackets: five boxes per bracketed
  tooth (none elsewhere), the base's transform is the unit box scaled on the bracket's world (the facial world with the movements: moved,
  rotated, tipped and mirrored teeth, both layouts), its middle on the crown, its face on the crown's most facial point under it (within
  0.3 mm, never behind it) and 2.5 to 5.5 mm from the crown's edge; wires and elastics: their number and the painter's order, after the teeth
  and under the numbers, one quad each about one pixel wide, and their ends at the slot ends or bracket middles (a mirrored tooth, a tipped
  tooth, the mandible's right-to-left order, an elastic between a moved tooth and the other arch); retained roots: cementum only, lit, in
  both views, gone with the tooth, translucent beneath on an unerupted tooth, the X over a planned extraction; space maintainers: flat, across
  the missing tooth's space at 24 mm (in ortho mode on the moved row), joined across two spaces, only the missing teeth of a quadrant, the
  lost primary molars over unerupted premolars, none over a pontic or a present tooth, one behind a moved neighbour; the Helix box (24
  positions with normals, every triangle counter-clockwise from outside, the reversed mesh); in a render, the bar's pixels are the TP colour
  within 2 and the bracket's base and wing are matte (178 and 124 for ortho9's 180 and 124); the wrapper's state (ResetTeeth clears the
  hardware, retained roots and space maintainers but keeps ortho mode; Copy copies the wires; an invalid tooth is ignored).
- `SceneTests.Scenes_DrawDifferentPictures_InSimple2D`: the 2D chart draws the hardware scenes as the empty chart and the ortho-mode scenes
  as the Palmer chart (section 1.5).
- `ParityProbeTests` and `Support/ParityProbes.cs`: allowances with reasons for what DirectX 9 never drew: `ortho-mode` (every probe: the
  layout), the facial probes of `ortho-brackets`, `ortho-wires`, `ortho-wire-in-bracket` and `ortho-elastics` (a bracket under the probe),
  `retained-root` and `retained-root-statuses` (the crown is gone); and four number boxes (numbers 3 and 14 of `retained-root-statuses` at
  410x307: the crescent above them; of `ortho-mode` at 500x370: no occlusal row above them). Every allowance covers a probe (the tests fail
  an unused one).
- The new scenes run through every other scene test (opaque colours, groups and descriptions, the live view against the renderer, the WPF
  gate, the gallery).

### 3.2 The baselines

**16 WPF pictures re-recorded, 29 added; every other picture keeps its bytes.** Recorded in `tc6-record-3` (the recorder into a scratch
folder, `OPMS_PARITY_FRESH=1`) at `9ac978e8b`; each picture repeatable and the same in a fresh render of both layers; only the pictures
whose pixels changed or are new were copied, with their `.txt` and their manifest rows (the 281 others, the 21 perio pictures among them,
had exactly their committed pixels).

- **Re-recorded, 410x307 and 500x370:** `ortho-brackets`, `ortho-wires`, `ortho-wire-in-bracket`, `ortho-elastics` (the hardware,
  ORTHO-2); `ortho-mode` (the layout, ORTHO-1, with its brackets); `retained-root` and `retained-root-statuses` (the root alone, RR-1);
  `space-maintainer-range` (bars across the missing 20 and 29, SM-1). The descriptions of `retained-root`, `retained-root-statuses` and
  `ortho-brackets` now say what the WPF chart draws (their `.txt`'s Description line).
- **Kept on purpose:** `space-maintainer-tooth`, `space-maintainer-quadrant` and `space-maintainer-statuses` paint space maintainers on
  present teeth only, so no bar is drawn, as Open Dental never draws one over a tooth.
- **Added (new scenes, `HasLegacyRecording` false):** `ortho-mode-hardware` (also 926x858), `ortho-mode-hardware-off`,
  `ortho-hardware-normal-layout`, `ortho-hardware-moved-teeth`, `ortho-mode-moved-teeth`, `ortho-mode-other-work`,
  `retained-root-extraction-planned`, `retained-root-extraction-completed`, `retained-root-moved-and-mirrored`,
  `space-maintainer-missing-tooth`, `space-maintainer-range-two-missing`, `space-maintainer-quadrant-missing`,
  `space-maintainer-primary`, `space-maintainer-not-over-teeth`.
- The legacy recordings (DirectX, OpenGL, Simple 2D, smoothed), the Simple 2D TC-5 baselines and the call logs are untouched (the painter
  is unchanged).

### 3.3 Builds and Targeted runs (build PC, `.git/od-tools/test-results/`)

- **Builds** through the lane: single-project builds of `SparksToothChart` and `ToothChartImageTests` while iterating, then the full build of
  `OpenDental.Local.slnf` at `bb6df788a`: 0 errors (116 warnings, none in a file of this package).
- `tc6-record-1.txt`: cut off by a shell timeout that ended the test host (not a test failure); its parity output gave the allowances.
- `tc6-record-2.txt` and `tc6-record-3.txt`: the recordings (section 3.2). The first drew the brackets with the specular part (section 1.2,
  fixed in `9ac978e8b`); the second is the one copied. Its two failures were the tests' own (the bracket test's footprint and the
  `ortho-wire-in-bracket` allowance), fixed in `bb6df788a`.
- **`tc6-final.txt`** (at `bb6df788a` after the full build, with gallery G5 written by the same run): **ToothChartImageTests 239: 236
  passed, 0 failed, 3 skipped** (the legacy recorder and its comparison, and the opt-in on-screen times); **BINATests 31, BPERIOTests 55,
  LibraryBindingTests 20 and NoHqTests 94 passed.** LibraryBindingTests was added because `SparksToothChart.dll` now references Helix's
  assemblies. The WPF gate: 326 of 326 pictures with exactly their `.txt`'s pixels; the parity probes 16852: 16207 pass, 645 differ and are
  allowed, 0 fail; the text boxes 8633: 51 allowed, 0 off; the live view against the renderer 1090 probes, 0 fail; the determinism check
  the committed bytes.
- `tools/Test-NoEolFlips.ps1` against `chart/tc3c-2` and against `main`: 31 changed text files, none changed its line ends.

## 4. Gallery G5

On the build PC, outside the repository like G1 to G4, in the main checkout's `.git/od-tools/test-results/`:

- **`tc6-gallery-G5/g5-tc6.html`** (also alone with its pictures in `tc6-gallery-G5-tc6-scenes.zip`, 8 MB): the page to show the owner,
  in G4's table layout. Its head says what each row changes (ORTHO-1, ORTHO-2, RR-1, SM-1) and the measurements; one row per changed or new
  scene (22 scenes, 45 pictures: 16 changed, 29 new), ortho mode first, then the hardware, the retained roots and the space maintainers;
  each row has today's DirectX 9 picture (none for a new scene), the WPF picture before (the approved one: `chart/tc3c-2`'s baselines, TC-3c
  as the owner approved it with G4), the WPF picture now with its parity caption, and the Open Dental screenshot it follows (`od/`:
  `cap13.png` and `ortho9.png` for ORTHO-1 and ORTHO-2, `retained-chart.png` for RR-1, `space-main-chart.png` for SM-1, each with its URL).
  Made by `tc6-make_g5_tc6.py` in the same folder from the gallery's own `index.html`.
- **`tc6-gallery-G5/index.html`** (the whole folder in `tc6-gallery-G5.zip`, 169 MB, zipped with `tools/New-ToothChartGallery.ps1
  -NoTestRun`): the full gallery (WriteGallery with `OPMS_CHART_GALLERY_NAME=G5` and the approved pictures of `chart/tc3c-2`): 320 WPF
  pictures, 291 with an approved picture, 16 of them different (TC-6's), 29 new; every DirectX parity caption passes. The render times: a
  refill with its picture 388 ms at 410x307 (median).
- The owner's answer goes into `docs/owner-questions.md` with the commit and the date (plan section 8). TC-6 merges only after it.

## 5. Launch check L-5 (TC-6's part), for a person

On the build PC's desktop, with a DEBUG build of this branch and the developer switch `OPMS_TOOTHCHART_WPF=1` set for the user (TC-2), on a
scratch database with synthetic patients only (never an office's data). Each step says what to do, what should happen, and what to write
down. Do every step once at 100% display scale and once at 150%.

1. **Before.** Start the program on the scratch database. Open a synthetic patient with all teeth in the Chart module. Write down which
   chart shows (the WPF chart: smooth teeth).
2. **Hardware.** Setup > Preferences > Ortho: make sure "Show Ortho button and tab in Chart Module" is ticked, and that hardware specs
   exist for a bracket (silver), a wire (silver) and an elastic (a yellow). Chart module, Ortho tab: tick Show Graphics. With "Add Single"
   add brackets on 2 to 15 and 18 to 31, a wire `2-15`, a wire `18-31` and an elastic `6,30`. *Expected:* a small grey square with four darker
   corners on each crown's front, a thin grey line through the squares of each arch, and a thin yellow line from the bracket of 6 to that
   of 30, in the normal layout (the occlusal rows still there). Untick Show Graphics. *Expected:* the hardware is gone. Tick it again.
3. **Ortho mode.** Tick "Tooth chart ortho mode". *Expected:* the occlusal rows are gone; the upper and lower front views sit close to
   the centre line with the numbers 8 7 6 5 4 3 2 1 1 2 ... 8 above and below the line between them; the teeth are the same size as
   before; the hardware is on the moved teeth. Compare with Open Dental's ortho screenshot (Capture13). Click on several crowns, upper and
   lower, left and right. *Expected:* each click selects the tooth clicked (its number is highlighted). Untick ortho mode. *Expected:* the
   normal chart is back.
4. **Moved teeth.** Movements tab: rotate 6 by 20 and tip 10 by 8, Apply. *Expected:* the brackets turn and tilt with the teeth and the wire
   follows. Reset the movements.
5. **Show Big.** With ortho mode ticked, open Show Big. *Expected:* the big chart in ortho mode, with the hardware, Palmer numbers.
6. **Retained root.** Make (or edit) a test code with paint type RetainedRoot; chart it as treatment-planned on 9. *Expected:* only the
   root of 9, in the treatment-planned colour, and a small crescent of that colour in the occlusal row (as retained-chart.png). Chart an
   extraction on 9 (planned). *Expected:* the X over the root. Set the extraction complete. *Expected:* the root is gone; with the
   completed-extraction setting on (the default), the X in the completed colour over the empty place.
7. **Space maintainer.** Mark 20 missing (Missing/Primary tab). Make a test code with paint type SpaceMaintainer (treatment area Tooth) and
   chart it on 20. *Expected:* a solid bar in the treatment-planned colour across the space of 20 at crown height, from 21 to 19 (as
   space-main-chart.png). Chart it on 21 (present). *Expected:* no bar over 21. With a quadrant code (area Quad) on UR, and 4 and 5 marked
   missing: *Expected:* one bar across 4 and 5 only.
8. **Printouts.** Open the Treatment Plan print preview and a sheet with a tooth chart. *Expected:* the brackets, wires, retained root and
   bar show there too (the same painter and picture); ortho mode does not (the printouts use the normal layout, as before).
9. **The 2D chart.** File > Graphics: choose Simple 2D, restart. *Expected:* no hardware, no ortho layout (only the Palmer numbers in ortho
   mode), no retained root and no bar, as Open Dental's own 2D chart. Set Graphics back.
10. **Write down:** for each step, as expected or what differed, with a screenshot of anything that differed (synthetic data only).
