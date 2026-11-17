# Fork-only enum values

Open PMS - Dental adds values to some enums whose numbers are stored in the database, for example `EnumPermType`
(`grouppermission.PermType`) and `AlertType` (`alertitem.Type`). Open Dental keeps adding values to the same enums in its own
releases, numbered upward from its last value. So that a later port of an Open Dental release never reuses one of our numbers,
fork-only values start at 10000.

| Range       | Feature                         |
|-------------|---------------------------------|
| 10000-10049 | Routine maintenance             |
| 10050-10099 | Inventory tracking              |
| 10100-10149 | SSH host keys                   |
| 10150-      | Free; reserve a range here first |

Rules:

- A released value is never renumbered or reused.
- Each fork-only value has a comment that names its feature.
- No two names of an enum share a fork-only value, and every fork-only value is in its feature's range (or is one of the earlier values
  below).
- Before adding a value, check that no code sizes an array from the enum's count, indexes by the value or treats the enum as flags.
  If a high value is unsafe for one enum, say so here and give that enum its own rule.
- Check the type of every column that stores the enum. 10000 needs at least `smallint`.

`OpenPmsDentalBusiness.ForkEnums` holds the first fork-only value of each kind (`FIRST_VALUE`, `FIRST_VALUE_TINYINT`) and tells
whether a value is fork-only (`ForkEnums.IsForkOnly`); tests use it to leave fork-only values out of the checks against Open Dental's
published numbers. It also holds the ranges of the two tables here (`ForkEnums.ListRanges`), the earlier values
(`ForkEnums.ListEarlierValues`) and every enum that has fork-only values (`ForkEnums.ArrayEnums`). `tests/RegistryTests`
(`ForkEnumRangeTests`) checks that every fork-only value in the code is unique in its enum and inside its feature's range, that no other
enum of the business layer has a value from 10000 up (flags enums aside), and that this document lists exactly those ranges and
values.

To add a value: take the next free number of the feature's range (from the bottom in the table above, from the top in the tinyint
table), add it to the table of every fork-only value below, and for a new enum add the enum to `ForkEnums` (`ArrayTinyIntEnums` for a
tinyint column).

## tinyint enums: AlertType, FormType, DocumentMiscType and UserOdFkeyType

`AlertType` is stored in `tinyint(4)` columns (`alertitem.Type`, `alertcategorylink.AlertType`, `alertsub.Type`), and so are `FormType`
(`alertitem.FormToOpen`), `DocumentMiscType` (`documentmisc.DocMiscType`) and `UserOdFkeyType` (`userodpref.FkeyType`). A `tinyint(4)`
holds -128 to 127, so 10000 does not fit, and widening Open Dental's columns would make the schema differ from Open Dental's (the schema
verifier). Their fork-only values therefore count **down from 127**, while Open Dental's count up from 0 (54 for `AlertType`, 21 for
`FormType`, 2 for `DocumentMiscType` and 31 for `UserOdFkeyType` in 26.2, port note P262-SCHEMA); 100 to 127 is the fork-only region
(`ForkEnums.FIRST_VALUE_TINYINT`).

| Range (tinyint enums) | Feature                         |
|-----------------------|---------------------------------|
| 127-120               | Routine maintenance             |
| 119-112               | Inventory tracking              |
| 111-100               | Free; reserve a range here first |

Within a range, values are taken from the top down (routine maintenance: `AlertType` 127 `MaintenanceDueSoon`, 126
`MaintenanceOverdue`; `FormType` 127 `FormMaintenanceBoard`). If Open Dental's own values ever come near 100, a port must stop and
the owner decides.

**Earlier values.** Version 0.0 released three tinyint fork-only values before these ranges were set. They keep their numbers, and no
feature may take them in their enum: `DocumentMiscType` 100 and 101 (the release channel, below every range), and `UserOdFkeyType` 114
(the Patient Forms view of package B-EF, inside inventory tracking's range, so in `UserOdFkeyType` inventory tracking has 114 taken).
The release channel's runtime part (package S3b-7 of the .NET 10 move, `docs/port-notes/S3b-7.md`) takes the next two values beside its
first two: `DocumentMiscType` 102 `ForkRuntimeManifest` and 103 `ForkRuntimeSegment`, the same table and column, so no schema change. The
release channel keeps one block, 100 to 103, below every range; `ForkEnums.ListEarlierValues` lists the four with it.

## Every fork-only value

| Enum             | Value | Name                  | Feature             |
|------------------|-------|-----------------------|---------------------|
| EnumPermType     | 10000 | MaintenanceSetup      | Routine maintenance |
| EnumPermType     | 10001 | MaintenanceRecord     | Routine maintenance |
| EnumPermType     | 10050 | InventoryAdjust       | Inventory tracking  |
| EnumPermType     | 10100 | SshHostKeys           | SSH host keys       |
| AlertType        | 127   | MaintenanceDueSoon    | Routine maintenance |
| AlertType        | 126   | MaintenanceOverdue    | Routine maintenance |
| AlertType        | 119   | InventoryLowStock     | Inventory tracking  |
| AlertType        | 118   | InventoryExpiring     | Inventory tracking  |
| FormType         | 127   | FormMaintenanceBoard  | Routine maintenance |
| FormType         | 119   | FormSupplyInventory   | Inventory tracking  |
| DocumentMiscType | 100   | ForkReleaseManifest   | Release channel     |
| DocumentMiscType | 101   | ForkReleaseSegment    | Release channel     |
| DocumentMiscType | 102   | ForkRuntimeManifest   | Release channel     |
| DocumentMiscType | 103   | ForkRuntimeSegment    | Release channel     |
| UserOdFkeyType   | 114   | PatientFormsView      | Patient Forms view  |

## Code that was unsafe for high values (fixed with routine maintenance)

- `EnumPermType`: the security tree's Set All and Set None (`UserControlSecurityTree.SetAll`, `SetNone`) looped over `0..count-1` and
  cast the index to the enum. With fork-only values that loop skips them and visits numbers that are not defined (and that a later Open
  Dental release may define). Both now loop over `Enum.GetValues`. The tests that pinned "values 0 to n without gaps" (RegistryTests
  `EnumPermType_IsContiguous`, M202Tests `EnumPermType_EndsAt276WithoutGaps`) now check the values below 10000 only, and that every
  value from 10000 up is in a reserved range.
- `AlertType`: the Central Manager's user window (`FormCentralUserEdit`) filled and saved its alert list by index (`(AlertType)i`).
  It now uses `Enum.GetValues` in both places.
- Searched and found safe: `FormAudit` (`Enum.GetValues`), `FrmAlertCategoryEdit` (`Enum.GetValues`), the alert menu (switch on the
  value), `GroupPermissions` (no arrays), `SecurityLogs` (`smallint` column). `EnumPermType` columns (`grouppermission.PermType`,
  `securitylog.PermType`) are `smallint(6)`, which holds 10000.

## Its own rule: EhrNotPerformedItem (not stored, indexed by position)

`EhrNotPerformedItem` (`TableTypes/EhrNotPerformed.cs`) names the items of the EHR "not performed" windows. It is **not stored**: an
`ehrnotperformed` row stores its code and code system, and the windows work the item out from the code (`CqmDataEntry.GetNotPerformedItem`,
package EHR-7). The windows index it **by position**: the Add button lists `Enum.GetNames` and passes the picked index
(`FormEhrNotPerformed.butAdd_Click`), the edit window takes `SelectedItemIndex` and names the item by `Enum.GetNames(...)[index]`
(`FormEhrNotPerformedEdit`), and the vital sign and medical windows pass `(int)` of an item. A value from 10000 would break every one of
them, so the rule of the ranges above does not apply. Instead:

- A fork-only value of `EhrNotPerformedItem` is the **next number** after the last value, with no gap, and its comment names its feature.
- A value is never renumbered or reused, as for every enum here. Open Dental has not changed this enum since its EHR code (its four values
  0 to 3); if a later Open Dental release adds a value, a port must stop and the owner decides, as for the tinyint enums.
- `ForkEnumRangeTests` does not see it (it is below 10000 and no column stores it); `EhrTests` (`PickListTests`) checks that the values
  are 0 to 4 without a gap and that this table lists the value.

| Enum                | Value | Name        | Feature                                                                 |
|---------------------|-------|-------------|-------------------------------------------------------------------------|
| EhrNotPerformedItem | 4     | BMIFollowUp | EHR-7, E-34: CMS69's BMI follow-up not performed, for its exception     |
