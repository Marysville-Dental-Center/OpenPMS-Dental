# UI-LOAD: the busy indicators as a vector progress ring

Package UI-LOAD (docs/specs/ui-refresh-plan.md, section 4.5 and section 7's UI-LOAD status line). The owner said yes (2026-11-24) to
"should loadingAnim.gif become a vector progress ring". Branch `ui/uiload-1` from `6bd8e0d64`. Gallery G-UI3-uiload
(`.git/od-tools/test-results/G-UI3-uiload`, zip beside it); audit [UI-LOAD-audit.md](UI-LOAD-audit.md).

## 1. The ring: `CodeBase.UiProgressRing`

`CodeBase/UI/UiProgressRing.cs`, a `Control` (no `UserControl`, so the window sweep does not build it as a window of its own):

- **Look.** An indeterminate Fluent ProgressRing: one arc in the `Accent` token with round caps, 32 px across with a 4 px stroke at 96 DPI
  (`DiameterAt96`, default 32; 64 is Fluent's large ring), scaled by `UiLooks.GetScale` like every shared look, never larger than the
  control, centred. Drawn with GDI+ (anti-aliased), so it is sharp at any DPI. The background is the control's (its parent's colour by
  default; a transparent back colour is supported).
- **Animation.** `FRAMES_CYCLE` (60) frames of `FRAME_MS` (33 ms): two seconds a cycle. The arc's middle turns twice a cycle while its
  length breathes from 20 to 260 degrees; both ends always move clockwise, never more than 20 degrees a frame (`GetArc`).
- **Timer only while visible.** A Windows Forms timer runs only while `Visible` is true (false while any parent is hidden, so a ring in a
  window that is not shown does not tick), the control is not disposed and Windows' animations are on (`UpdateAnimation`, called on every
  visibility and parent change). Hiding the ring or its window stops the timer; `Dispose` stops and disposes it. `IsAnimating` says
  whether it runs.
- **Reduced animation.** Windows' "Show animations in Windows" (`SystemParametersInfo(SPI_GETCLIENTAREAANIMATION)`, read once and again on
  `UserPreferenceChanged`): when it is off nothing moves and the ring is static: the whole circle as an `AccentSubtle` track with a quarter
  arc in `Accent` from twelve to three o'clock. `SetAnimationForTest` (test processes only) sets it for the tests.
- **High contrast.** `UiTheme.IsHighContrast`: the background `Window`, the arc `Highlight` (Accent's partner), the static track
  `WindowFrame`.
- `DrawRing` (static) draws a ring into any rectangle at a scale and frame, for the tests and any owner-drawn place.

Records: a `coverage.tsv` row (`CodeBase/UI/UiProgressRing`, UI-LOAD), an element inventory row (`tools/UiFit/element_inventory.py`,
`UI-13-inventory.tsv`), NoHqTests' colour-literal scopes (it has none) and package list (`UI-LOAD`).

## 2. The places

A search of the program for busy pictures: every tracked `.gif` (and every GIF inside a `.resx`) for more than one frame, `ImageAnimator`
calls, WPF storyboards and spinners. Found:

| Place | Before | Now |
|---|---|---|
| `OpenPmsDentalGraph/Base/GraphQuantityOverTime` (the dashboard's graphs while their data loads) | `pictureBoxLoading`, a PictureBox with `Resources.loadingAnim` (an 8-frame 64 x 64 GIF), Dock Fill, CenterImage, brought in front of the chart while `IsLoading` | `progressRingLoading`, a ring with `DiameterAt96` 64, Dock Fill, in front of the chart while `IsLoading`, as before; now also hidden while the chart is in front, so its timer runs only while loading. `loadingAnim.gif`, its `.resx` entry and its `Resources` property are removed. |
| `OpenPmsDental/ReportingForRdl/Allocators/MyAllocator1/FormWarnToCloseComputers` (the warning before a report allocator's database task: "Close Other OD Programs Now!!") | `imageAnimator1`, the allocator's own `ImageAnimator` UserControl, which on Load looked for the eight `SnuffLight1-8.gif` frames among the assembly's embedded resources | `progressRingWait`, a ring in the same 50 x 50 slot. The frames were Content files, never embedded, so the animator never showed anything (the reference picture shows an empty slot); the ring is the first indicator that window shows. The `ImageAnimator` control and its eight GIFs are removed (nothing else used them), and the Load handler that loaded them. |

Not changed: the marquee progress bars (`FormProgressStatus`, `ODProgressBar`, the WPF `ProgressBarFlat`) are progress bars, restyled in
UI-4c, not pictures; WPF has no spinner; every other `.gif` is a single-frame picture (content or icons). No other animated GIF is in the
repository.

## 3. Tests

New `tests/UiRefreshTests/ProgressRingTests.cs`:

- `Ring_Geometry_FluentSizesScaled`: 32 px and a 4 px stroke at 100%, 48 and 6 at 150%, Fluent's large ring 64 and 96; never larger than
  the control.
- `Ring_Arc_CyclesAndBreathes`: frame 60 is frame 0, the arc 20 to 260 degrees, both ends forward and at most 20 degrees a frame.
- `Ring_Draw_100And150_ArcInAccentInsideItsSquare`: pixel checks at 100% and 150%: three points on the arc solid `Accent`, three points
  of the gap the background, the centre the background, nothing outside the ring's square (its anti-aliased edge allowed), the solid
  arc's pixel count as its area at that scale.
- `Ring_Draw_AnimationsOff_StaticRing`: the quarter arc `Accent`, the track `AccentSubtle`, every pixel the same at any frame.
- `Ring_Draw_HighContrast_WindowsColours`: `Highlight` arc, `WindowFrame` track.
- `Ring_Control_PaintsAtItsFrame`: the control paints exactly `DrawRing`'s pixels; the frame wraps.
- `Ring_Timer_OnlyWhileVisible_StopsWhenHiddenOrDisposed`, `Ring_Timer_AnimationsOff_NeverRuns`.
- `Graph_IsLoading_ShowsTheRingInFrontOfTheChart`, `WarnToCloseComputers_RingInTheAnimatorsPlace`: the two places.
- `Scan_NoAnimatedGif` (every `.gif` of the repository's product and test folders has one frame), `Scan_NoPictureAnimation` (no code calls
  `ImageAnimator.Animate` or uses the removed pictures).
- `Gallery_UiLoad_BeforeAndAfter` (opt-in, below).

`FinalSweep.Check` has a new rule, `busy-gif`: a PictureBox showing an animated picture in any window the harness builds. The sweep found
none. Changed records: the layout snapshot names the two rings where the picture box and the animator were (slices 12 and 9) and drops the
`ImageAnimator` rows (unsliced), `grid-main-sizes.tsv` and `sweep-known.tsv` drop `ImageAnimator`'s row, `icons/picture-map.tsv` drops
`loadingAnim`. `tests/WindowChecksTests/UiApproved/baseline.tsv` keeps its `ImageAnimator.png` row: the reference set is one commit's
(`Baseline_OneCommit_EveryPictureOnce`) and is taken again by the lead. `tools/project-baseline` (the lead's record) still lists the removed
items: `OpenPmsDentalGraph|None|Resources\loadingAnim.gif`, the `ImageAnimator` Compile, EmbeddedResource and resources lines and the
eight `SnuffLight` Content lines are removed on purpose.

## 4. Gallery G-UI3-uiload

`.git/od-tools/test-results/G-UI3-uiload` (zip beside it): 4 pictures, 4 changed. Each place before and after at 100% and 150%, the ring
at one frame (`ProgressRingGallery.GALLERY_FRAME`, 26). The graph is drawn in its loading state (FitGallery's new state
`GraphQuantityOverTime#loading`: `IsLoading` true). The before pictures come from the branch's own build with
`OPMS_UI_LOAD_GALLERY_BEFORE`: each ring hidden and, in the graph's loading state, the old picture box put back as the designer had it,
with `loadingAnim.gif` from `6bd8e0d64`. The 100% before picture of FormWarnToCloseComputers has the same bytes as the reference picture
of `ui-baseline-final`. `<W>` is `.git/od-tools/test-results/G-UI3-uiload-src`.

```
git show 6bd8e0d64:OpenPmsDentalGraph/Resources/loadingAnim.gif > <W>/loadingAnim.gif
OPMS_UI_LOAD_GALLERY=<W> OPMS_UI_LOAD_GALLERY_OLD_GIF=<W>/loadingAnim.gif run-tests.ps1 -Mode Targeted -Platform x64 -Include UiRefreshTests
   (Gallery_UiLoad_BeforeAndAfter draws <W>/before and <W>/after, windows-100 and windows-150, through FitGallery in the program's font)
tools/New-UiGallery.ps1 -Before <W>/before -After <W>/after -Name G-UI3 -Out .git/od-tools/test-results/G-UI3-uiload
```

## 5. Results

See section 6 (filled in at the end of the package).
