# Port note: IMG-P1 (double-click opens the image viewer window)

Why: the owner's imaging design (approved 2026-11-22, decisions D1/D2): a double-click on an image in the Imaging module opens that one image in
a standalone window at normal size, never full screen and never opened maximised, and never Item Info. P1 of four phases (P2 teeth, P3 adjust
tools, P4 camera and video), scheduled with the Open PMS session; none of its files is in the UI rollout.

## Behaviour

| Action | Before | Now |
|---|---|---|
| Double-click a filled mount slot | Item Info (FrmDocInfo) | the image viewer window on that image |
| Double-click a single image | Item Info | the image viewer window |
| Double-click an empty slot, or the mount's background | nothing, or Mount Edit | nothing (Mount Edit is Info with no slot selected) |
| Double-click a mount's text item, or in Text drawing mode | its editor | unchanged |
| Double-click an image in the tree | Item Info | the image viewer window |
| Double-click a mount in the tree | Mount Edit | nothing more than the click (the mount opens in the module) |
| Double-click another file in the tree (PDF and others) | its own program | unchanged |
| Item Info, Mount Edit | double-click, Info button, right-click Info | Info button, right-click Info, Alt+Enter (new) |

The window (`OpenPmsDental/Forms/FormImageViewer.cs`): one window, reused by every double-click; about two thirds of the screen the module is
on, centred, or the bounds it last closed with in this run of the program; Left/Right step through the mount's filled image slots in item
order; the wheel zooms at the pointer (0.25x to 32x), a drag pans, Home fits, +/- zoom; a double-click or Esc closes it and the mount selects
the image last shown (`ControlImageDisplay.SelectFromViewer`, as a click does). It shows the image as the module does, at the image's full
resolution: crop, rotation, flip, brightness/contrast, and the drawings shown (a single image's own; a mount's drawings mapped onto the item,
`ControlImageDisplay.GetViewerBitmap`). It changes nothing and makes no database call. It closes when the image leaves the module (its display
is disposed or shows something else) and on a patient change (`ControlImages.CloseFloaters`). Activating it reloads the image, so a change
made in the module meanwhile shows.

Not in P1: the adjust panel in the window (P3); remembering the window's bounds across runs of the program (no per-computer setting was added).

## Files

- `OpenPmsDental/Forms/FormImageViewer.cs` (new). Its two controls are made in code (`BuildControls`): no designer file, so the UI rollout's
  designer data (layout-snapshot.tsv, the UI-13 inventory) needs no rows for it.
- `OpenPmsDental/User Controls/ControlImageDisplay.cs`: `EventViewerRequested`, the double-click, `ProcessCmdKey` (Alt+Enter), the
  "Image viewer window" region (`GetViewerIndexes`, `GetViewerBitmap`, `GetViewerBackColor`, `GetViewerText`, `GetViewerTitle`, `SelectFromViewer`)
- `OpenPmsDental/Main Modules/ControlImages.cs`: `_formImageViewer`, `ShowImageViewer`, the event in `CreateControlImageDisplay`, the tree's
  double-click, `CloseFloaters`

## Tests

- `tests/ImagingTests/ImageViewerWiringTests.cs` (7, source checks: the windows cannot open in a test run).
- `tests/UiRefreshTests/ImageViewerGallery.cs`: the gallery pictures, opt-in (`OPMS_IMAGE_VIEWER_GALLERY=<folder>`), synthetic images only.

Targeted run (x64): AttributionTests 54/54, BIMGTests 28/28, ImagingCaptureTests 806/806, S3aM1Tests 60/60 (8 skipped), VPEARL1Tests 23/23,
ImagingTests 88/89, NoHqTests 126/127. The two failures fail at this branch's base (647febb65) too and touch no file of P1:
ImagingTests `OldImagesModule_Removed_OnlyTheConvertScriptsAndPrefCsNameIt` reads UserControlImagingGeneral.Designer.cs positions that the
UI-12 fix round moved (c0d7db2b7); NoHqTests `TrackedFiles_NameNoPracticeServerOrDeveloperFolder` hits README.md, fixed on main by 9275c6caa.

## Gallery G-IMG1

Only after pictures: the window is new. The harness draws the client area; the window's title bar (patient, date, mount) is drawn by
FormODBase at run time and is not in the pictures.
```
rem the pictures (builds UiRefreshTests first through the build lane)
set OPMS_IMAGE_VIEWER_GALLERY=<S>\after
odbuild.ps1 -Root <worktree> -Platform x64 -TestOnly -Test tests\UiRefreshTests\bin\Debug\UiRefreshTests.dll -TestFilter Name=Gallery_ImageViewerWindow
powershell -NoProfile -ExecutionPolicy Bypass -File tools/New-UiGallery.ps1 -After <S>/after -Name G-IMG1 -Out .git/od-tools/test-results/G-IMG1
```
Audit (before the owner): round 1 found the "This image is not loaded." note grey on a black mount background; it now takes the theme's
text colour that reads on the background (TextOnAccent on dark, TextSecondary on light). Round 2: no findings.
