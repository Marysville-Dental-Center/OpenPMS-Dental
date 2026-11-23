# Port note: IMG-P3c-2 (a DICOM image's filters on its 16-bit data)

Why: the owner's imaging design, section 2: "16-bit images are processed at 16 bits". Before this, IMG-P3a to P3c-1 filtered the 8-bit
picture after the window, so a DICOM image's sharpening and noise reduction saw only the 256 grey steps its window left. The 16-bit sensor
captures of the capture track (CAP) are DICOM, so this is where the depth is.

## What changes

`ImageAdjust.WindowDicom(bitmapDicom, windowingMin, windowingMax, settings)` makes a DICOM image's 8-bit picture:
1. noise reduction (median 3x3 or 5x5) and sharpening (the unsharp mask, same sigma and amount) on the raw 16-bit (or 12-bit) data, clamped
   to its stored bits (`FilterBeforeWindow`, on a copy: the raw data is not changed);
2. the window (`DicomHelper.ApplyWindowing`, unchanged);
3. local contrast, gamma and invert on the windowed picture: these are display mappings of what the window shows.

Neutral or null settings give exactly `DicomHelper.ApplyWindowing`. Non-DICOM images are filtered as before (8-bit, after brightness and
contrast).

Every DICOM path uses it:

| Path | Where |
|---|---|
| The module | `ControlImageDisplay.LoadBitmap` (a single image and mount items), `InvalidateSettingsColor` (sliders, the Enhance previews; without filters while a slider moves, as before) |
| Thumbnails | `ImageHelper.GetImageCroppedDicom` |
| Email attachments, sheet images, pickers | `ImageHelper`'s file loader (`GetBitmapOfDocumentFromDb`) |
| Claim copies | `ClaimAttachDicom.Render` with drawings |

`ControlImageDisplay.ApplyAdjust` now skips DICOM images, which are filtered with their window, so nothing is filtered twice. O (the
original) still shows the plain window.

## Tests

- `tests/ImagingTests/WindowDicomTests.cs` (5), on synthetic 12-bit data:
  - neutral is exactly the window;
  - smoothing removes a speck and leaves the raw data unchanged;
  - sharpening steepens a 16-raw-value step that the window renders as 1 grey step;
  - invert runs after the window;
  - only noise reduction and sharpening run before it.
- `AdjustWiringTests` and `EnhanceWiringTests` follow the new calls: LoadBitmap has 2 ApplyAdjust and 2 WindowDicom; thumbnails and claim
  copies call WindowDicom.

Targeted x64 run:

| Suite | Passed | Total |
|---|---|---|
| ImagingTests | 146 | 146 |
| CLAIMDCMTests | 41 | 41 |
| M2IMGTests | 25 | 25 |
| BIMGTests | 28 | 28 |
| VPEARL1Tests | 23 | 23 |
| ImagingCaptureTests | 806 | 806 |
| AttributionTests | 54 | 54 |
| S3aM1Tests | 60 (8 skipped) | 68 |
| NoHqTests | 127 | 127 |

UiRefreshTests has the base set's failures (18, MenuStrip now Inconclusive).

There is no gallery: the change shows only on real 16-bit images.
