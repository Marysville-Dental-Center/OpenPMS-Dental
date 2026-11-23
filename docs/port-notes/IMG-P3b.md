# Port note: IMG-P3b (the Enhance window and the presets)

Why: the owner's imaging design (approved 2026-11-22), section 2 and decisions D3 (kept per image), D4 ("Adjusted" tag) and D5 (standard
permissions). This package builds on IMG-P3a (merged as c882349d5): the filters, the tables and the permission.

## Behaviour

- **Enhance** (Imaging toolbar after Info; the A key in the module and in the image viewer window) opens one small tool window owned by the
  main window, at the right edge of the module. It follows the module's selection: a selected mount item, or a single image.
- **Controls:**
  - Brightness (-100 to 100) and contrast (0 to 100) map onto the image's window (`ImageEnhance.ToWindowing`/`FromWindowing`); the module's
    windowing slider follows them.
  - Gamma 0.50-2.00, Sharpen 0-100, Smooth Off/Low/High, Auto contrast and Invert.
  - A histogram of the picture showing.
  - Reset.
- **Presets:**
  - Original, Caries, Endo and Perio are supplied in code (`ImageEnhance.GetBuiltInPresets`); they are starting points, not diagnostic
    settings. The office's own presets (`imageadjustpreset`, not hidden) follow them.
  - "Save as preset..." stores the current settings. A right-click on an office preset renames it, updates it to the current settings or
    deletes it. All four need Image Adjust Presets and are logged under it. Supplied presets cannot be changed.
- **Preview and save:**
  - Moving a slider shows brightness/contrast at once, and the filters 300 ms after it rests.
  - Letting go of a slider, a key on it, a check box, a preset, Reset or closing the window saves (`ControlImageDisplay.SaveAdjust`).
  - A save updates the document's window, upserts the documentadjust row (keeping a video's trim), logs ImageEdit with the old and new
    window and filters plus LogSignedImageEdited, remakes the thumbnail, and refreshes the module's slider and the image viewer window.
- **Permissions:**
  - Changing an image needs Image Edit, plus Signed Image Edit for a signed image (`CanEditImage`, the same check as the module's other
    edits). Without them the window shows the values, disabled, with a note.
  - A DICOM image's brightness/contrast stays on the module's windowing slider, because its window is on its own scale; the Enhance window
    shows its filters.
- **The module's windowing slider** now shows brightness/contrast only while it moves; the filters follow when it is let go. That was the
  slow path IMG-P3a's review noted.
- **The "Adjusted" tag (D4):** an image whose saved filters change it gets a Warning-coloured "Adjusted" tag at its bottom right, on each
  mount slot or on the single image. Brightness/contrast alone do not count. The tag is drawn on the screen only, not in prints or copies.
- **Claim DICOM copies** are now filtered: `ClaimAttachDicomL` passes each document's filters to `ClaimAttachDicom.Render`, which stays
  without the database.

## Differences from the design

- The design's Adjust panel slides in on the module's right. It is a small tool window instead: it also serves the image viewer window,
  and it leaves `ControlImages`' layout code alone.
- It is named Enhance because the image toolbar already has "Adj" (moving an image in its slot).
- P3c brings the magnifier, spot enhance, undo/redo, hold O for the original, "All in mount", right-drag brightness/contrast, and
  filtering 16-bit images before their window.

## Files

| File | Change |
|---|---|
| `OpenPmsDental/Forms/FormImageEnhance.cs` | new; controls made in code |
| `OpenPmsDentalBusiness/Imaging/ImageEnhance.cs` | new: the presets, the mapping, Describe |
| `OpenPmsDental/User Controls/ControlImageDisplay.cs` | EventEnhanceRequested, EventAdjustSaved, InvalidateSettingsColor(doApplyAdjust), the A key, DrawAdjustedTags, GetSelectedImageIdx, CanEditImage, PreviewAdjust, SaveAdjust, GetHistogram, RequestEnhance |
| `OpenPmsDental/Main Modules/ControlImages.cs` | TB.Enhance, the button, ShowEnhance, SetTarget on every enable, the slider's fast preview |
| `OpenPmsDental/Forms/FormImageViewer.cs` | the A key, refresh on EventAdjustSaved |
| `OpenPmsDentalBusiness/Imaging/ClaimAttachDicom.cs`, `OpenPmsDental/Logic/ClaimAttachDicomL.cs` | the filters passed in |
| `tests/UiRefreshTests/Data/toolbar-overflow.tsv` | one line (see below) |

The toolbar-overflow line (the UI session's data file): with the old "today" font at 1024 px, the Imaging toolbar's Paste and Forms move
into the overflow, now that it has Teeth and Enhance. The default Segoe font still fits. Written by the suite's own
`OPMS_UI6_WRITE_OVERFLOW=1` rule.

## Tests

- `tests/ImagingTests/ImageEnhanceTests.cs` (5): the mapping, always a valid window, the supplied presets, Describe.
- `tests/ImagingTests/EnhanceWiringTests.cs` (7): the button and the selection, the keys and the viewer, the save's permission, log, trim
  and thumbnail, the preset permission and logs, the sliders, the screen-only tag, claim DICOM.
- `tests/ImagingTests/WiringTests.cs`: 13 IsAuthorizedToEdit, 14 LogSignedImageEdited.
- `tests/ImagingTests/AdjustWiringTests.cs`: the new InvalidateSettingsColor signature.
- `tests/UiRefreshTests/ImageViewerGallery.cs`: the Enhance window with no image, filtered, and without permission.

Targeted x64 run:

| Suite | Passed | Total |
|---|---|---|
| ImagingTests | 132 | 132 |
| CLAIMDCMTests | 41 | 41 |
| ImagingCaptureTests | 806 | 806 |
| BIMGTests | 28 | 28 |
| VPEARL1Tests | 23 | 23 |
| M2IMGTests | 25 | 25 |
| AttributionTests | 54 | 54 |
| S3aM1Tests | 60 (8 skipped) | 68 |
| NoHqTests | 127 | 127 |

UiRefreshTests fails the same 19 tests as main.

## Gallery G-IMG3 and audit

Audit round 1 found the track bars set their size before turning AutoSize off. Windows then kept them 45 px tall over the rows below, so
four sliders and the Auto contrast box were hidden. That would have happened in the program too. Fixed: AutoSize off first, 23 px.

Round 2 found one nit, left as it is: the shared track bar look draws a disabled slider like an enabled one, though it cannot move.

## Review (one independent review, 2026-11-23)

Fixed:

- **Saving moved into one method that checks again.** `ImageEnhance.Save` (business layer) checks Image Edit and Signed Image Edit
  again, writes the window and the filters (keeping a video's trim), logs and removes the thumbnail. The display's
  `SaveAdjust(document, ...)` calls it, and the window falls back to it when the display is gone. Tested in `EnhanceSaveDbTests`.
  - A change made with the wheel or keys (no MouseUp) could be lost when the selection moved, or logged against the image now at that
    index. It is now saved by the timer once the value rests and no button is down, and saving is by document, not by index.
  - A signing while the window was open left it editable for a user without Signed Image Edit. `SetTarget` now checks the permission again
    on every call, and the save checks it too: a refused save disables the window.
- **The module's slider is no longer undone.** A window saved with the module's slider was reverted by the window's stale
  brightness/contrast, or logged as an Enhance change. `windowingSlider_ScrollComplete` now calls `SyncWindowFromImage`.
- **The window closes on a patient change** (`CloseFloaters`).
- **A change waiting is shown with its filters** before the window moves to another image or closes (`FlushPending`).
- **Nits:**
  - A preset's window is used exactly (`_windowFixed`).
  - A preset saved from a DICOM image has no window (WindowingMax 0) and leaves an image's window.
  - The presets stay usable for managing on an image the user may not change.
  - The A key opens on the display that asked.
  - The context menu and the bold font are disposed.
  - A mount's thumbnail is not redrawn on every save, as the slider never did.
