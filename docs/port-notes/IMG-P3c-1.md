# Port note: IMG-P3c-1 (viewing aids, right-drag, undo, All in mount)

Why: the rest of the owner's imaging design, section 2: the magnifier, spot enhance, undo/redo, the unadjusted original, all in mount, and
right-drag brightness/contrast. This builds on IMG-P3b (merged as 218483af1). IMG-P3c-2 follows with filtering 16-bit images before their
window.

## Behaviour (Imaging module; the keys need the image area focused, as Alt+Enter, T and A do)

- **M** cycles the magnifier through 2x, 4x and off: a circle at the pointer, drawn again at that scale, so it stays sharp. On the screen
  only.
- **S** turns the spot enhancer on or off: a circle at the pointer with its grey values stretched to the full range (darkest and lightest 1
  percent to black and white, `ImageAdjust.StretchContrast`). Ctrl+wheel sizes it from 30 to 300 px. On the screen only; nothing is saved.
- **O** shows the selected image's original: the file without brightness/contrast and without filters (a DICOM image keeps its window, which
  is how it is read). An "Original (O to return)" tag shows. O again, or any click, returns.
  - Design difference: the design said "hold O". A held key's release does not reliably reach the image area, so O is a toggle.
- **Right-drag on the selected image changes brightness/contrast:** sideways for contrast, up and down for brightness.
  - While dragging it previews without the filters; on release it saves through `ImageEnhance.Save`, which checks Image Edit and Signed
    Image Edit again and logs.
  - The menu is taken off the panel while dragging, so the release opens no menu; it comes back after. A short right-click still opens it.
  - Not for DICOM images (their window is on its own scale).
  - The Enhance window follows the change (`EventWindowDragged`, `SyncWindowFromImage`).
- **Ctrl+Z / Ctrl+Y** undo and redo the Enhance window's saves for the image it is on, from the window, the image area or the image viewer
  window. Each undo is saved and logged like a change. The history is per image while the window stays on it.
- **"All in mount..."** in the Enhance window (enabled on a mount item) gives every image of the mount the current window and filters,
  after a confirmation.
  - Each image is saved through `ImageEnhance.Save`, so each one's own permission is checked.
  - A DICOM image keeps its window.
  - A message counts the images that were not changed.
  - It is not undoable as one step (the history is cleared).

## Files

- `OpenPmsDental/User Controls/ControlImageDisplay.cs`:
  - the keys, the viewing aids (`DrawViewingAids`, `DrawPictureForAid`, `ApplyViewTransform`);
  - the original (`SetShowingOriginal`, `ShowOriginalPicture`, its tag);
  - right-drag (`RightDrag`, `RightDragEnd`, the mouse hooks);
  - `ApplyAdjustToMount`, `RequestUndo`, `EventUndoRequested`, `EventWindowDragged`.
- `OpenPmsDental/Forms/FormImageEnhance.cs`: `UndoRedo` and its stacks, "All in mount...", Ctrl+Z/Y. The bottom buttons are now Reset / All
  in mount on one row and Save as preset / Close (bottom right) on the next.
- `OpenPmsDental/Main Modules/ControlImages.cs`: undo and drag routed to the Enhance window.
- `OpenPmsDental/Forms/FormImageViewer.cs`: Ctrl+Z/Y.
- `OpenPmsDentalBusiness/Imaging/ImageAdjust.cs`: `StretchContrast`.

## Tests

- `tests/ImagingTests/ViewingAidsWiringTests.cs` (5):
  - the stretch, on pictures;
  - the keys;
  - the aids and the original on the screen only, saving nothing;
  - right-drag: the permission, the save, the menu, not for DICOM;
  - undo/redo and All in mount through the Enhance save.
- The gallery's Enhance pictures are redrawn (G-IMG4).

Targeted x64 run:

| Suite | Passed | Total |
|---|---|---|
| ImagingTests | 141 | 141 |
| CLAIMDCMTests | 41 | 41 |
| S3aM1Tests | 60 (8 skipped) | 68 |
| BIMGTests | 28 | 28 |
| VPEARL1Tests | 23 | 23 |
| M2IMGTests | 25 | 25 |
| ImagingCaptureTests | 806 | 806 |
| AttributionTests | 54 | 54 |
| NoHqTests | 127 | 127 |

UiRefreshTests fails the same 19 tests as main.

The panel menu test (S3aM1) first failed on an Opening handler that cancelled the menu after a drag. That was replaced by taking the menu
off the panel during the drag, so the menu's handlers are as the designer has them.

## Audit

The Enhance window's "All in mount..." first sat on a row of its own under Close; the buttons were rearranged so Close stays bottom right.
The viewing aids are drawn on the module's canvas, which the gallery harness cannot draw without a database, so they have no pictures; their
drawing is tested by source.
