# UI-LOAD audit: gallery G-UI3-uiload

The lead's visual audit and fix round before the owner sees the gallery (the rule of 2026-11-21). Gallery:
`.git/od-tools/test-results/G-UI3-uiload` (zip beside it), 4 pictures, 4 changed, 0 the same. Every changed pixel lies inside the ring's
square (the before and after pictures differ only there: GraphQuantityOverTime at 100% in 474,306 to 538,370; FormWarnToCloseComputers
at 100% in 25,194 to 57,226; the same squares at 150%). The ring is drawn at one animation frame (`ProgressRingGallery.GALLERY_FRAME`).

## The pictures

| Picture | Before | After | Verdict |
|---|---|---|---|
| `windows-100/GraphQuantityOverTime-loading.png` | the grey eight-segment GIF, 64 px, its first frame | the ring, 64 px (Fluent's large ring), 4 px Accent stroke, open side up and right | fine |
| `windows-150/GraphQuantityOverTime-loading-150.png` | the GIF at 64 px (CenterImage never scaled it) | the ring at 64 px: see finding 3 | fine, harness limit noted |
| `windows-100/FormWarnToCloseComputers.png` | nothing in the 50 x 50 slot left of "Close Other OD Programs Now!!" | the ring, 32 px, in the slot | fine, finding 4 |
| `windows-150/FormWarnToCloseComputers-150.png` | nothing in the slot | the ring at 48 px | fine |

The 100% before picture of FormWarnToCloseComputers, drawn by this package's before mode (the ring hidden), has the same bytes as the
frozen reference picture `ui-baseline-final/windows-100/FormWarnToCloseComputers.png`, so the before reconstruction draws what `main` draws.
The graph's loading state has no reference picture (the gallery had no such state before); its before picture puts the old picture box
back exactly as the designer had it (Dock Fill, CenterImage, `loadingAnim.gif` from the branch point, in front of the chart).

## Findings and what was done

1. **The first gallery frame read as a smile, not a spinner** (frame 15: a 140 degree arc along the bottom). Fixed: the gallery frame is
   26, a 250 degree arc open up and right, the way a busy ring is usually pictured. Only the gallery's frame changed; the animation is the
   same.
2. **The graph's ring was half the old spinner's size** (32 px in place of 64 px in a 789 x 302 chart area; the old GIF was 64 x 64).
   Fixed: `UiProgressRing.DiameterAt96` (default 32, Fluent's ProgressRing) is 64 for the graph (Fluent's large ring), so the footprint is
   the old one. The stroke stays 4 px at 96 DPI.
3. **At 150% the graph's ring is not scaled in the picture** (64 px, as at 100%). The ring takes its scale from `UiLooks.GetScale`, as every
   shared look does; the harness builds the graph in a plain host at 96 DPI with no program window around it, so the scale is 1 there, and
   the graph's other looks in the same picture (radio buttons, box frames) are at 1 too. In the program the graph lives in the dashboard's
   plain windows (GraphQuantityOverTimeFilter), where the control's own dpi gives the monitor's scale, so the ring is 96 px at 150%
   (`ProgressRingTests.Ring_Geometry_FluentSizesScaled`). Not changed: a harness limit, the same for all looks of that picture.
4. **The warning's ring sits 8 px lower than the label beside it** (the 50 x 50 slot starts with the label at y 8 but is 16 px taller,
   and the ring is centred in it). Not changed: the slot is the animator's, and the refresh keeps layouts; it reads as an icon beside a
   heading. The old animator showed nothing there at all (its eight frames were Content files, never embedded, so the form's loader found
   none): the ring is the first indicator that window ever showed.
5. **High contrast and animations off** are not in the gallery (the harness draws the light theme with animations on); they are checked
   pixel by pixel in `ProgressRingTests` (Highlight on a WindowFrame track; the static AccentSubtle track with a quarter Accent arc that
   does not move with the frame).

Nothing else was found: no clipping, no colour outside the tokens, no other window changed.
