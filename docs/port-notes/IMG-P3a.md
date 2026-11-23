# Port note: IMG-P3a (adjust tools: storage, permission and filters)

Why: the owner's imaging design (approved 2026-11-22). Section 2 covers the X-ray adjust tools modelled on Apteryx XrayVision, plus decisions:
- D3: the settings are kept per image and the original file is never changed;
- D4: images with filters are tagged "Adjusted";
- D5: standard Open Dental permissions, set per user group.

P3 comes in three packages:
- P3a: storage, permission and filters, with no UI;
- P3b: the adjust panel and presets;
- P3c: the magnifier, spot enhance, undo, hold-O and all-in-mount.

The schema follows the coordinator's plan: fork-only tables, no column on an Open Dental table.

## What P3a does

- **Filters** (`OpenPmsDentalBusiness/Imaging/ImageAdjust.cs`), applied in this order:
  1. a median noise filter (`ImageSmooth`: Off, Low 3x3, High 5x5);
  2. adaptive local contrast (CLAHE, 8x8 tiles, clip 2);
  3. an unsharp mask (sharpen 0-100, sigma 1.5 px, up to 1.5 times the detail);
  4. gamma (0.5-2, above 1 lifts the mid-greys);
  5. invert.

  `ImageAdjustSettings.IsNeutral` is true when nothing is set; D4's tag is `!IsNeutral()`.
  - A grey image is filtered as one plane and stays grey.
  - A colour image is filtered per channel, with the local contrast taken from its luminance so the hue stays.
  - 24 and 32 bpp formats are filtered; any other format is left alone.
  - A 1300x1800 image with every filter at once takes under 2 s (tested).
- **Storage** (`ImagingAdjustSteps.cs`, levels 2026112301-2026112303; 2026112304-2026112309 kept for the feature):
  - `documentadjust`: one row per image, unique on DocNum. It holds Gamma, Sharpen, Smooth, IsAutoContrast, IsInverted, and the trim points TrimStartMs/TrimEndMs for P4's videos (0 = no trim).
  - `imageadjustpreset`: the office's own presets. It holds Description, ItemOrder, WindowingMin/Max, the filters, and IsHidden. The supplied Caries/Endo/Perio presets are in code (P3b), not rows.

  Both tables are created empty.
  - `DocumentAdjusts.Upsert` deletes the row when the settings are neutral and nothing is trimmed.
  - `Documents.Delete` deletes the image's row.
  - CRUD is from xCrudGenerator.
  - `db/init/new-database.sql` is regenerated with `tools/New-InitSql.ps1`: the two tables, the grant row and the levels.
- **Permission** `EnumPermType.ImageAdjustPresets=10150`:
  - "Image Adjust Presets": create, change or delete the adjust presets.
  - It sits in the new fork range 10150-10199 "Imaging" (`ForkEnums`, `docs/port-notes/FORK-ENUMS.md`).
  - Setup > Security shows it in the Imaging Module node, after Image Export.
  - Step 2026112303 (`ImageAdjustPresetGrants`, modelled on `MaintPermissionGrants`) grants it to every user group that has Setup. It inserts only missing rows, once per database.
  - The adoption tools have its plan: `tools/AdoptionValidation/ImageAdjustGrantPlan.cs`, used by MigrationPreview and AdoptionRowCheck. Both tables have TableAdded rules, and the adoption runbook is updated.
- **Where the filters apply**, always after brightness/contrast and only to what is shown, printed or exported (the file never changes):
  - `ControlImageDisplay.LoadBitmap`: a single image (DICOM and other) and mount items;
  - `InvalidateSettingsColor`: the brightness/contrast slider;
  - the category's print to PDF;
  - `ImageHelper.ApplyDocumentSettingsToImage`, the shared export/print path, with the COLORFUNCTION flag.

  The display reads each document's settings once, a mount's in one query (`GetAdjustSettings`). `ClearObjects` empties that cache.

  LoadBitmap has a shortcut that copies the picture showing as the raw image when brightness/contrast is untouched. It now also requires `IsAdjustNeutral`; otherwise a filtered picture would become the raw and be filtered twice.

## Not yet

- 16-bit filtering. The design asked for filters on the 16-bit original before the 8-bit mapping. P3a filters the 8-bit picture after windowing, so for now a 16-bit image is filtered at 8 bits. Planned with P3c.
- No UI sets the filters yet (P3b). Until then every image is neutral, so the module looks exactly as before.

## Tests (targeted, x64)

| Suite | Result |
|---|---|
| ImagingTests | 118/118: ImageAdjustTests (7, the filters), ImageAdjustDbTests (the tables, the round trips, Documents.Delete, the grant plan), AdjustWiringTests (4, where the filters apply) |
| ForkMigrationTests | 38 (2 skipped) |
| RegistryTests | 39/39 |
| P262SchemaTests | 24 (4 skipped) |
| M201Tests | 24 (2 skipped) |
| MaintenanceTests | 111/111 |
| InitSqlTests | 21/21 |
| ImagingCaptureTests | 806/806 |
| BIMGTests | 28/28 |
| VPEARL1Tests | 23/23 |
| AttributionTests | 54/54 |
| S3aM1Tests | 60 (8 skipped) |
| NoHqTests | 127/127 |
| UiRefreshTests | the same 19 failures as main (the desktop-session drawing tests) |

The data layer's own run, before the display wiring, also passed: InventoryTests 93/93, EhrTests 328 (2 skipped), SshHostKeyTests 46/46, AdoptionValidation.Tests 69/69.

There is no gallery: P3a changes nothing visible.
