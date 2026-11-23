# Port note: IMG-DICOM-FIX (one DICOM test; 8-bit data in 16-bit words)

Why: two bugs that IMG-P3c-2's review found outside its diff. The coordinator asked for them as a small package before P4.

## What changes

1. **`DicomHelper.GetRawPerWindowingStep` takes the step from BitsStored.** Data with BitsAllocated 16 and BitsStored 8 or less is on the
   0-255 scale already, so its step is 1. It was divided by 16, which showed such an image nearly black (white came out as 15). 12-bit,
   true 16-bit and other depths keep their factors, so stored windows show as before.
2. **One DICOM test, `ImageHelper.IsDicomPath`:** .dcm or .dicom, in any case, with surrounding spaces ignored. It replaces every
   case-sensitive or lower-cased `.dcm` test (22 call sites) in:
   - ControlImageDisplay, FormImageEnhance and ControlImages;
   - ImageHelper, ImageStore (reading DICOM and the import), MountHelper and Documents (the import's metadata).

   `ImageHelper.IsDicomFileName` and `ClaimAttachDicom.IsDicomFileName` now call it.

   An image imported as "X.DCM" is now shown, filtered and windowed in the module like "x.dcm". A ".dicom" file is DICOM everywhere,
   including the import. The stored file name never changes.

## Tests

- `tests/ImagingTests/DicomCheckTests.cs` (4): the name test, including "X.DCM", ".dicom", paths and non-DICOM names; a scan that finds no
  case-sensitive `.dcm` test left in OpenPmsDental, OpenPmsDentalBusiness or WpfControlsOD; 8-bit data in 16-bit words windowed white to
  255; the other depths unchanged.
- Two tests asserted the old behaviour and now assert the fix:
  - M2IMGTests `RawPerWindowingStep_IsDecidedByBitsStored`: 16/8 gives 1, not 16.
  - CLAIMDCMTests `ReadDocument_UpperCaseExtension_IsReadWithTheSameReader`: `ImageHelper.IsDicomFileName("UPPER.DCM")` is now true.
- `ViewingAidsWiringTests` follows the helper.

Targeted x64 run:

| Suite | Passed | Total |
|---|---|---|
| ImagingTests | 150 | 150 |
| CLAIMDCMTests | 41 | 41 |
| M2IMGTests | 25 | 25 |
| BIMGTests | 28 | 28 |
| VPEARL1Tests | 23 | 23 |
| ImagingCaptureTests | 806 | 806 |
| AttributionTests | 54 | 54 |
| S3aM1Tests | 60 (8 skipped) | 68 |
| NoHqTests | 127 | 127 |

UiRefreshTests fails the base set only.
