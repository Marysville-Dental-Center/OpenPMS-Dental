# Port note: S3b-5W (the WPF tooth chart's pictures on .NET 10 x64, gallery G5)

Package S3b-5W of stage 3b of the .NET 10 move ([docs/specs/net10-plan.md](../specs/net10-plan.md), the S3b-5 row; the S3b-5 item of the 3b
section and section 8 of [docs/specs/wpf-tooth-chart-plan.md](../specs/wpf-tooth-chart-plan.md)): the part of S3b-5 that needs WPF to draw,
which waited for the owner's desktop session ([S3b-5.md](S3b-5.md), section 9). Branch `net10/s3b5w-1` from `net10/switch` (`652b9720e`, S3b-1
to S3b-7 and S3b-FMT merged), 2026-11-16. **Status: done on the branch; gallery G5 waits for the owner, and S3b-5W merges only after his
answer** (`docs/owner-questions.md`).

The owner's remote desktop session was Active for every recording and every picture run, checked with `query user` and a red-square probe
(WPF's `RenderTargetBitmap`, as WindowChecksTests' `Shot.CanRenderWpf`) before each; it stayed Active throughout. No program with a window
was started; the picture tests draw off screen as before. WindowChecksTests was not run (the lead's screenshot pass). Scratch databases only
on 127.0.0.1:3306: the host tests with the prefix `tc8s3b5w_` (their prefix rule takes `tc8` and letters or digits), the image tests' call
logs with their fixed `tc2_`; all dropped (none left after the runs). No schema change, no package, no download.

Commits: `c7ba82cf0` (the runtime choice of the sets, the G5 comparison, the watch order), `3c39ef732` (the tests' .NET 10 cases),
`7c95e3a54` (the pictures), then this note with the plans and the owner question. Review fix round: branch `net10/s3b5w-2`, section 9
(the DirectX parity probes read the pictures of their runtime; no picture changed, G5 unchanged).

## 1. What was recorded

The image and host tests compare on .NET 10 x64 with full sets of their own, recorded by the tests' own recorders in a 64-bit .NET 10
process; the .NET Framework sets (`wpf-net48`, `wpf-net48-x64`, `simple2d-net48`, `simple2d-net48-x64`, the host tests' `Baselines\`) are
unchanged:

| Set | Pictures | Recorder | Size |
|---|---|---|---|
| `tests\ToothChartImageTests\Baselines\wpf-net10-x64` | 326 (every picture of `wpf-net48`: 305 chart scenes at their sizes, 15 perio at 649x696, 3 prints, 3 saved images), with the `.txt` facts and `manifest.txt` | `WpfImageTests` (`OPMS_RECORD_WPF_CHART=1`) | 41 MB |
| `tests\ToothChartImageTests\Baselines\simple2d-net10-x64` | 35 | `Simple2DImageTests` (`OPMS_RECORD_2D_CHART=1`) | 5.7 MB |
| `tests\ToothChartHostTests\Baselines\net10-x64` | 9 (`big-chart`, `dashboard`, `perio-chart`, `perio-save`, `referral-letter`, `sheet`, `sheet-editor-preview`, `sheet-simple2d`, `treatplan-sheet`) | `HostPictures.Check` (`OPMS_RECORD_HOST_BASELINES=1`) on a scratch database with synthetic patients | 1.7 MB |

Each folder has a `README.md` with its numbers and reasons. Every WPF picture was repeatable when it was recorded (a second `GetBitmap` and a
fresh render of both layers without the layer cache gave the same pixels, as the recorder checks), none blank; two recordings in two
processes (section 5) wrote the same 326 PNG files byte for byte. The `.txt` files say `Framework: .NET 10.0, x64` (the host's
`.NET 10.0.14, x64`, a line the host recorder now writes).

**How the tests choose** (by runtime and bitness; `WpfCharts.IsNetFramework`, `IsNet10X64`):
- `WpfCharts.ForThisBitness`: in a 64-bit .NET process a path under `Baselines\wpf-net48` or `simple2d-net48` becomes the same path under
  `wpf-net10-x64` or `simple2d-net10-x64`, whether or not the file exists, so a missing picture fails as "no baseline at ..." instead of
  passing against a .NET Framework picture (`WpfImageTests.CheckScene` and `PerioPrintout.Check` now map before the existence check). On
  .NET Framework x64 TC-10's rule is unchanged (the x64 copy where there is one); in a 32-bit process the path as given.
- The WPF recorder writes to `wpf-net10-x64` on .NET x64 (TC-10's refusal of a 64-bit recording without `OPMS_RECORD_WPF_CHART_OUT` stays
  for .NET Framework x64); `Simple2DImageTests.Baselines2DForThisProcess` gives `simple2d-net10-x64` on .NET x64 for recording and comparing.
- `HostPaths.Baselines` gives `Baselines\net10-x64` on .NET x64 and `Baselines\` (now also `HostPaths.BaselinesNet48`) otherwise.
- `WpfCharts.GetNet48X64Record`: a picture's .NET Framework x64 record for G5 (TC-10's x64 copy, otherwise the 32-bit picture, which the
  64-bit .NET Framework draws within tolerance).

**The program's default font in the tests.** The first recording drew all 35 Simple 2D pictures and the host's `sheet-simple2d` in .NET's
default font (Segoe UI 9): `ToothChart2D` draws its tooth numbers and texts with the control's `Font`, and the program sets Microsoft Sans
Serif 8.25 in `Main` since S3b-4 (`Application.SetDefaultFont`), which the test processes did not. Every Simple 2D picture was beyond
tolerance against .NET Framework (up to 7,859 pixels, 219 in one block). The `[AssemblyInitialize]` of ToothChartImageTests
(`Support\TestAssemblySetup.cs`) and ToothChartHostTests (`HostDb.cs`) now set the program's font first, as UnitTests' `TestBase` does
(S3b-5), and the three sets were recorded again (`s3b5w-record2.txt`); after it 34 of the 35 Simple 2D pictures are identical to .NET
Framework's. The WPF chart's text uses its own GDI fonts and did not change (the 326 files of both recordings are byte for byte the same).

## 2. The comparison with .NET Framework 4.8 x64 (G5)

`RuntimeBaselineTests.WriteG5Gallery` (`OPMS_G5_GALLERY=<folder>`) compares every committed .NET 10 x64 picture with its .NET Framework 4.8
x64 record using the tests' own `ChartImageCompare` (a pixel is different when a channel differs by more than 8 of 255; a picture passes
with at most 0.1% different pixels and no 16x16 window with more than 24), and reports for each the changed pixels (any channel apart), the
pixels beyond the tolerance, the worst block and where, the largest channel difference and the box around the changes
(`comparison.tsv`, `summary.txt`). It draws nothing. **370 pictures: 193 identical, 173 within tolerance, 4 beyond it.**

| Engine / group | Pictures | Identical | Within | Beyond | Most changed pixels | Most beyond the channel tolerance | Worst block | Largest channel difference |
|---|---|---|---|---|---|---|---|---|
| WPF / sizes | 9 | 3 | 6 | 0 | 20 | 6 | 6 | 53 |
| WPF / free-text | 15 | 6 | 9 | 0 | 15 | 6 | 5 | 23 |
| WPF / paint | 158 | 79 | 79 | 0 | 24 | 3 | 1 | 53 |
| WPF / known-difference | 6 | 2 | 4 | 0 | 16 | 1 | 1 | 13 |
| WPF / colours | 8 | 4 | 4 | 0 | 16 | 1 | 1 | 15 |
| WPF / interaction | 26 | 12 | 14 | 0 | 23 | 4 | 3 | 19 |
| WPF / empty | 20 | 8 | 12 | 0 | 22 | 4 | 1 | 53 |
| WPF / states | 40 | 16 | 24 | 0 | 24 | 2 | 2 | 35 |
| WPF / ortho | 23 | 11 | 12 | 0 | 15 | 2 | 2 | 13 |
| WPF / perio | 21 | 18 | 3 | 0 | 789 | 36 | 4 | 187 |
| Simple 2D / free-text | 15 | 14 | 1 | 0 | 140,733 | 0 | 0 | 8 |
| Simple 2D / paint | 20 | 20 | 0 | 0 | 0 | 0 | 0 | 0 |
| Program hosts | 9 | 0 | 5 | 4 | 108,379 | 2,941 | 161 | 255 |

**The WPF chart, by its record.**
- **The 163 pictures with a .NET Framework x64 copy** (`wpf-net48-x64`: the 142 at 410x307, the 15 perio scenes, 3 prints, 3 saved
  images): 157 identical. `410x307/shiftm-plus` (3 pixels, 2 beyond, channel 20), `410x307/shifto-unerupted` and `410x307/missing-many`
  (1 pixel each, channel 5 and 4): single 3D edge pixels of the new render DLL (wpfgfx_cor3). The three `perio-save` pictures: 789 pixels,
  36 beyond; every one is on the left pixel column (x 0) or the top row (y 118) of the chart picture drawn into the 750x1000 page:
  `PerioPrintout.RenderSave` (as `FormPerioGraphical.RenderPerioPrintout`) scales the 649x696 chart to the page width with GDI+
  `DrawImage(float x, float y, float width, float height)`, and on .NET 10 that outermost column is a partial blend (for example 187 where
  .NET Framework had the line's 0) and row 118 is 254 instead of 255. The chart picture itself is identical and so is the print
  (`perio-print`); the edge is a sub-pixel placement of the page's `DrawImage` (the same expression in single precision), not visible at
  normal size, and within tolerance. Not changed.
- **The 163 pictures with only the 32-bit picture:** 2 identical, 161 within tolerance (at most 24 pixels, 6 beyond the channel tolerance,
  6 in one block, channel 53). These are the 64-bit differences TC-10 measured on .NET Framework, where the same pictures were within
  tolerance in a 64-bit process (`wpf-net48-x64\README.md`), not .NET 10's.
- The determinism test's picture (`410x307/restored-adult`) has on .NET 10 x64 exactly the pixels of .NET Framework x64
  (`73782da6...`, TC-10), in one process and in a second one.

**Simple 2D** (after the font, section 1): the 18 pictures of `simple2d-net48-x64` and 16 of the 32-bit set identical; `926x858/
text-all-kinds` within tolerance of its 32-bit picture (140,733 pixels apart by at most 8 of 255: the 64-bit GDI+ scaling of the background,
within tolerance on .NET Framework x64 too, TC-10).

**The hosts.** The host tests were 32-bit until S3b-5 and the program was never built for x64 on .NET Framework, so each host picture is
compared with its 32-bit picture. Five within tolerance (`big-chart` 6 pixels, `referral-letter` 21, `sheet` and `treatplan-sheet` 20,
`sheet-editor-preview` 15, at most 1 beyond). **Four beyond, each one of the 64-bit differences of TC-10.md section 3, which the image tests
show unchanged between .NET Framework x64 and .NET 10 x64:**

| Picture | Beyond (changed) | Worst block | Where | Reason (TC-10) |
|---|---|---|---|---|
| `perio-chart` (649x696) | 1,380 (1,565) | 112 at 334,648 | rows 651-666, plus about 30 single 3D pixels | the perio legend's second row ("Suppuration", "Clinical Attachment Level", "Furcation 2") at y 648 instead of 649: TC-10's 1,380 pixels exactly |
| `perio-save` (750x1000) | 2,585 (108,379) | 161 at 29,851 | the legend in the page; the scaled chart a little apart everywhere | the same legend row, in the page; the rest within the channel tolerance (the 64-bit 3D layer, scaled) |
| `dashboard` (410x307) | 31 (34) | 31 at 111,134 | rows 142-149 | the tooth number "5" at x 121 instead of 120: TC-10's 31 pixels exactly |
| `sheet-simple2d` (500x370) | 2,941 (43,144) | 130 at 163,184 | the teeth and their numbers | the Simple 2D chart's 64-bit GDI+ anti-aliasing (TC-10: 2,916 to 4,175 pixels, up to 146 in one block, at 500x370); the image tests' 16 Simple 2D pictures at 500x370 are identical between .NET Framework x64 and .NET 10 x64 |

The one-pixel text cells come from `ToothSceneBuilder.AddText`'s `Math.Ceiling`/`Math.Floor` of a single-precision projection that lands on
a pixel border: the 32-bit JIT computed it on the x87 unit at double precision, every 64-bit JIT (.NET Framework's and .NET 10's) with SSE
in single precision. They are the 64-bit program's state; whether the rounding should give the 32-bit position again is a drawing change
for the owner (question 2 of the G5 entry), not made here.

## 3. The drawing change: the watches in a stable order

The chart plan's note (section 4, "The watches are ordered by `Hashtable` enumeration") asked TC-10 and S3b-5 to give the watches a stable
order before pictures are compared across processes on .NET 10. `ToothSceneBuilder.AddWatches` (WPF) and `ToothChart2D.DrawWatches` (Simple
2D) kept the watched teeth in a `Hashtable` keyed by the permanent tooth, as DirectX did, and drew the "W"s in its order. That order follows
the string hash, which .NET randomises per process, so two overlapping "W"s of other colours (a moved tooth beside its neighbour) could be
drawn in another order from run to run. Both now draw from `ToothSceneBuilder.GetWatchTeeth`: the same selection (a permanent tooth's watch
even when the tooth is missing; a visible primary tooth's watch in its permanent tooth's place), sorted by tooth number (1 to 32). The
smallest change: one shared method, the drawing of each "W" unchanged. `TextDrawingTests.Watches_AreDrawnInToothNumberOrder_InEveryProcess`
pins the order (with a primary tooth, K in 20's place). The 12 scenes with a watch that have a .NET Framework x64 picture are identical to it.

## 4. The tests' .NET 10 cases

ToothChartImageTests had not run on .NET 10 before (S3b-5 left the picture projects to this package). Besides the pictures, three tests
assumed .NET Framework; none was cut:
- `ChartBitnessTests.NormalBuild_StaysX86` is now `NormalBuild_IsX86OnNetFramework_AndX64OnNet10`: on .NET Framework TC-10's x86 rule; on
  .NET 10 `Directory.Build.Net.props` sets `PlatformTarget` x64 (S3b-1), no project of the chart's stack sets another, and the normal
  build's `SparksToothChart\bin\Debug\SparksToothChart.dll` is PE32+ for AMD64, IL-only.
- `HelixLibraries_AreILOnlyAndNot32BitRequired` and the test above cross-check the PE facts with the runtime's own header reader:
  `AssemblyName.GetAssemblyName(...).ProcessorArchitecture` on .NET Framework; on .NET, where that property is always `None` (SYSLIB0037),
  `System.Reflection.Metadata`'s `PEReader` with the same rule (`ChartBitnessTests.GetClrProcessorArchitecture`). The Helix files are PE32
  I386 IL-only, MSIL, on .NET 10 as before.
- `WpfLiveViewTests.Program_DrawsEveryWpfWindowInSoftware_AsTheseTestsDo`: S3b-4 put `Application.SetDefaultFont` and
  `Application.SetHighDpiMode` first in `ProgramEntry.Main` (both before any window, as the net10 plan requires); the render mode follows
  them, still before any window, and the test reads it there.

New: `RuntimeBaselineTests` (ToothChartImageTests): `WpfRendersInThisSession` (the red-square check; inconclusive, not failed, in a locked
or disconnected session, so a run says why its pictures fail), `Net10X64Sets_HoldEveryPicture_RecordedOnNet10X64` (the three .NET 10 x64
sets hold exactly the pictures of their .NET Framework sets, 326, 35 and 9, each with a `.txt` recorded on .NET 10 x64), and
`WriteG5Gallery` (opt-in, section 6). The gate's evidence line names the .NET 10 x64 set; the render-time report names the runtime from
`RuntimeInformation.FrameworkDescription`.

The live view's real-input tests (the plan's TC-10 row: `WpfLiveViewInputTests`, WPF's internal input types by reflection) pass on .NET 10:
`RealInput_ClickDragPenAndAHeldStillPen_...`, `RealInput_MovesWpfRaisesAtAnUnchangedDevicePosition_...` and
`RealInput_AMoveBackToTheSamePointAfterTheMouseLeft_IsHandedOn`. The big chart at 3x on .NET 10 x64: matches its picture, first draw about
2.1 s, 96.7 MB added by the wrapper and its first draw, six more draws -3.4 MB (no layer kept); the render loop -25.3 MB private bytes between
image 50 and 500.

## 5. The runs

All Targeted, x64, `-RequireBuildOutput`, in `.git/od-tools/test-results/`:

| File | What | Result |
|---|---|---|
| `s3b5w-record.txt` | the first recording (before the font, section 1) | recorded 326 + 35 + 9; the tests that need the pictures failed in that run, as expected, plus the three .NET Framework assumptions of section 4 |
| `s3b5w-run1.txt`, `s3b5w-run2.txt` | against the first recording, with the section 4 fixes | all passed; run 2 wrote the first G5 (which showed the Simple 2D font) |
| `s3b5w-record2.txt` | the recording with the program's font (the committed pictures) | recorded; WPF byte for byte the first recording's; only the tests that need the pictures failed in that run |
| `s3b5w-run3.txt` | **run 1 against the committed pictures** | ToothChartImageTests 267 (261 passed, 6 skipped), ToothChartHostTests 20 (19, the gallery skipped), BINATests 22, NoHqTests 115: all passed |
| `s3b5w-run4.txt` | **run 2 against the committed pictures**, writing the final G5 | the same counts, all passed |

The six skipped image tests are the opt-in ones (the legacy, mouse and transform recorders of the retired engines, the legacy comparison,
the gallery writer and the on-screen times; `WriteG5Gallery` ran in runs 2 and 4 and is skipped otherwise). In both runs the gate compared
326 WPF pictures with the .NET 10 x64 set, every one with exactly its `.txt`'s pixels. BINATests' `WpfChart_StartsOnAHiddenForm` (the
headless failure of S3b-5) and `Simple2D_StartsOnAHiddenForm` pass. Lane builds: ToothChartImageTests, ToothChartHostTests, BINATests and
NoHqTests x64, 0 errors; the full `OpenDental.Local.slnf` build x64 at the end (section 7).

## 6. Gallery G5

In the main checkout's `.git/od-tools/test-results/g5-gallery/` (not in the repository), zipped beside it as `g5-gallery.zip` (forward-slash
entry names; 107 MB, 921 files). Made by run 4 (`OPMS_G5_GALLERY`, `OPMS_G5_SUMMARY` for the owner's summary, `OPMS_G5_COMMIT`) at
`7c95e3a54`. `index.html`: the summary at the top (the counts, what changed and why in plain words, the questions), a table per engine and
group, then one row per picture, the not-compared, beyond-tolerance and within-tolerance ones first and the identical ones last, each with
the .NET Framework 4.8 x64 picture, the .NET 10 x64 picture, the difference image (the old picture in grey, pixels changed within the
tolerance in orange, beyond it in red) and its numbers; a group filter as in the earlier galleries; relative image paths
(`net48-x64/`, `net10-x64/`, `diff/`, each with `wpf/`, `simple2d/`, `host/`). `comparison.tsv` and `summary.txt` hold the numbers. The
lead publishes it; the owner's answer goes in `docs/owner-questions.md`.

## 7. Checks

- Full lane build of `OpenDental.Local.slnf` x64 (no scoped restore) after the last code commit: 0 errors, 180 warnings
  (`s3b5w-fullbuild.txt`).
- `tools\Test-NoEolFlips.ps1 -Base net10/switch`: no changed text file changed its line ends. The new files are LF in the index like their
  neighbours (the picture `.txt` files, the READMEs, the new test file); the PNGs are binary (`.gitattributes`).
- No `tc8s3b5w_` or `tc2_` database left on 127.0.0.1:3306 after the runs.

## 8. For the lead

- **G5 waits for the owner** (`docs/owner-questions.md`, the first gallery entry): approve the .NET 10 pictures; and keep the 64-bit
  one-pixel text cells (the default) or ask for the rounding change.
- **The .NET Framework x64 sets** (`wpf-net48-x64`, `simple2d-net48-x64`) are kept unchanged, as the brief says; on this branch nothing
  reads them but G5 (`GetNet48X64Record`). They can retire with .NET Framework when `main` takes the switch, as their README says.
- **The lane's failure output** still shows only "Error Message:" without the message lines (TC-10's suggestion); the run logs
  (`<out>.txt.logs\*.log`) have them.

## 9. Fix round 1 (`net10/s3b5w-2`): the parity probes on the pictures of their runtime

The review found that the DirectX parity probes still read the WPF pictures from `Baselines\wpf-net48` directly
(`ParityProbeTests.ForEachRecordedPicture`, `PerioParityTests.ForEachRecordedPerioPicture`), so on .NET 10 x64 the three `ParityProbes_*`
and three `PerioParity_*` tests probed the .NET Framework 32-bit pictures, not the `wpf-net10-x64` set that
`ChartScenes_MatchTheWpfBaselines` holds against fresh renders, and would have lost their input when `wpf-net48` retires. Both now take the
path through `WpfCharts.ForThisBitness`, as `WpfImageTests.CheckScene` does (a 32-bit process: `wpf-net48`; .NET Framework x64: its
`wpf-net48-x64` copy where there is one; .NET 10 x64: `wpf-net10-x64`), and both class comments name that chain (`0298b509b`).

**What the probes found on the .NET 10 x64 pictures.** The surface-centre probes (16,852 probes, 645 allowed, 0 failed) and the perio probes
(7,178 colour probes and 428 text boxes, 0 failed) pass with the allowance lists unchanged. The text-box probe
(`ParityProbes_NumbersAndMarks_SitWhereDirectXDrawsThem_WithinOnePixel`) failed on two boxes, both tooth 5's number "5" at 410x307:
`colour-custom-chart-colours` and `select-whole-arch`, DirectX's ink box 120,142 to 126,150 and the WPF one 122,142 to 127,150 (the left edge
2 pixels off; the 32-bit picture's within 1). Looked at pixel by pixel in the four pictures (DirectX, `wpf-net48`, `wpf-net48-x64`,
`wpf-net10-x64`) over x 116-130, y 141-151: the 64-bit glyph is the 32-bit glyph one pixel to the right, and the .NET Framework x64 and
.NET 10 x64 pixels there are the same. So it is not a .NET 10 change but TC-10's 64-bit text cell (section 2: `ToothSceneBuilder.AddText`
rounds a single-precision projection that lands on a pixel border, x87 in the 32-bit JIT, SSE in every 64-bit one), the same "5" one pixel
right that the dashboard host picture shows, and the subject of the owner's question 2 of G5. On these two scenes' colours (the selection
highlight behind the selected arch's numbers; the custom chart colours) the moved glyph's faint left column falls under the probe's 0.4 ink
threshold, so the box reads one pixel more. Not a drawing fix (the rounding is the owner's question): two named allowances,
`ParityProbeTests.TextAllowances64Bit`, which the class's static constructor adds to `TextAllowances` only in a 64-bit process, so the 32-bit
run (where these boxes are within one pixel and an allowance that covers nothing fails) is unchanged; their reason says to remove them if the
rounding is changed to give the 32-bit place again (`76d385447`). The text-box probe now reports 8,633 boxes, 53 allowed (51 before plus
these two), 0 failed. That the two new allowances are used at all shows the probes read the 64-bit pictures: on `wpf-net48` the same boxes
are within one pixel.

**The runs** (all Targeted, x64, `-RequireBuildOutput`, after the full build; the session `Active` by `query user` before and after each,
and the image tests' own red-square check):

| File | What | Result |
|---|---|---|
| `s3b5w2-run1.txt` | ToothChartImageTests after the path fix (NoHqTests not yet built) | 1 failed: the text-box probe's two boxes above |
| `s3b5w2-run2.txt` | with the two allowances (scoped build, so NoHqTests found the program unbuilt) | ToothChartImageTests passed; NoHqTests 8 "not built" |
| `s3b5w2-pass1-image.txt`, `s3b5w2-pass1-host.txt` | **pass 1** after the full build | ToothChartImageTests 267 (260 passed, 7 skipped), ToothChartHostTests 20 (19, the gallery skipped), BINATests 22, NoHqTests 115: all passed |
| `s3b5w2-pass2-image.txt`, `s3b5w2-pass2-host.txt` | **pass 2** | the same counts, all passed; the parity evidence (`%TEMP%\ToothChartImageTests\parity-*.txt`) the same numbers as pass 1 |

The seven skipped image tests are the six opt-in ones of section 5 and `WriteG5Gallery` (opt-in, not run: no picture changed, so G5 stays as
made at `7c95e3a54`). Full lane build of `OpenDental.Local.slnf` x64 after the last code commit: 0 errors. No scratch database left on
127.0.0.1:3306.
