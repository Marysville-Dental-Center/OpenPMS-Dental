# Port note: IMG-P4a (photo mount templates and slot guides)

Why: the owner's imaging design, camera and video (2026-11-22, C1-C8 as recommended). Mounts held X-rays only. The camera session (P4b)
fills photo mounts slot by slot, so each slot has to say what to shoot. This package adds that data and the photo templates. It adds no
camera code.

## What changes

1. **Two fork tables** (ImagingCameraSteps, levels 2026112311 and 2026112312; 2026112313-19 are reserved for P4):
   - `mountitemdefguide`, one row per template slot (unique MountItemDefNum): GuideLabel, IsMirror, SlotKind.
   - `mountitemguide`, the same per patient mount slot (unique MountItemNum). A Mount keeps no MountDefNum, so
     `Mounts.CreateMountFromDef` copies each slot's guide when it makes the mount. A later template edit does not change existing mounts.
   - `MountSlotKind`: Radiograph=0, Intraoral=1, Extraoral=2. A slot without a row is a radiograph slot with no guide.
   - Each delete path removes the matching guide rows: `MountDefs.Delete`, `MountItemDefs.Delete` and `DeleteForMount`, `Mounts.Delete` and
     `MountItems.Delete`.
   - Pasting a copied mount (the module's Paste) copies each slot's guide too (`MountItemGuides.CopyForMountItem`; found in review).
   - Init SQL is regenerated (462 tables). The adoption rules and runbook list both tables as allowed additions, held empty.
2. **Three photo templates** (`StandardMountTemplates.ListPhotoTemplates`), kept apart from the four X-ray ones. Slots are 1600 x 1200
   (4:3); face photos are portrait. Each slot has a guide and default teeth (Universal), which an office can change.
   - "Intraoral photos 5": Frontal, Right buccal, Left buccal, Upper occlusal (mirror), Lower occlusal (mirror). Text item at the top left.
   - "Ortho photos 8": Face frontal, Smile, Profile right (extraoral), then the intraoral five.
   - "Before and after photos": Before, After. These have no default teeth.
   - Photo templates have no scale.
3. **Add standard templates** (Imaging devices > Add standard templates) gets a **Photo category** picker under the X-ray one.
   - With a photo category, the photo templates are added too and save into it. With only a photo category, only they are added.
   - The installer writes each photo slot's guide row. The Setup log entry lists each slot's guide.
   - The patient-picture warning covers both pickers.
   - Without a photo category the window works exactly as before.
4. **Setup > Mounts > a slot (FormMountItemDefEdit)** gets three fields in the free right-hand column, at the window's own 23 px pitch:
   - **Camera guide** (text, up to 255 characters).
   - **Slot kind** (Radiograph, Intraoral photo, Extraoral photo).
   - **Mirror shot**.

   They are saved after the slot. A radiograph slot with no guide text and no mirror keeps no row.

## Designer file changed

`OpenPmsDental/Forms/FormMountItemDefEdit.designer.cs` (UI rollout batch 7). The coordinator freed it for this package.
- It adds five controls: labelGuideLabel, textGuideLabel, labelSlotKind, comboSlotKind and checkMirror.
- The file's mixed line endings are kept.
- The window's rows in `tests/UiRefreshTests/Data/layout-snapshot/unsliced.tsv` are rewritten with OPMS_UI_LAYOUT_SNAPSHOT_WRITE=1 (five
  added rows only).
- `docs/port-notes/UI-13-inventory.tsv` is regenerated with `tools/UiFit/element_inventory.py`. The designer counts follow the new controls,
  and the code-built counts follow the earlier imaging windows.

## Tests

- `tests/ImagingTests/MountGuideDbTests.cs` (8): the table declarations; Upsert, Get and Delete for both tables; `CreateMountFromDef` copying
  guides; a pasted mount keeping its guides; every delete path.
- `tests/ImagingTests/SlotGuideWiringTests.cs` (3): the slot window's controls; the guide is read on load and saved after the slot; the paste
  copies the guide.
- `tests/ImagingCaptureTests/PhotoMountTemplatesTests.cs` (6):
  - the three templates: slots inside the mount, no overlap, shooting order, mirror shots;
  - installs with a photo category only, with both categories, and with a bad category.
- `tests/ImagingCaptureTests/Db/PhotoTemplatesDbTests.cs` (1): on the scratch database, the install, the guide rows, the copy to a patient's
  mount, and no guide row left after the template or the mount is deleted.

Targeted x64 run:

| Suite | Passed | Total |
|---|---|---|
| ImagingTests | 162 | 162 |
| ImagingCaptureTests | 813 | 813 |
| ForkMigrationTests | 38 (2 skipped) | 40 |
| RegistryTests | 39 | 39 |
| P262SchemaTests | 24 (4 skipped) | 28 |
| M201Tests | 24 (2 skipped) | 26 |
| MaintenanceTests | 111 | 111 |
| InitSqlTests | 21 | 21 |
| AdoptionValidation.Tests | 69 | 69 |
| WindowChecksTests | 30 (44 skipped) | 74 |
| NoHqTests | 127 | 127 |

UiRefreshTests fails the base set only (the disconnected desktop session). The element inventory and layout snapshot tests pass.
