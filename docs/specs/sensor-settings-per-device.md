# A sensor's driver settings saved per device in the database

Owner decision of 2026-11-24 (roadmap, "A sensor's driver settings saved per device in the database"). Package **DEVSET**. It changes
section 9.3 of [imaging-capture-design.md](imaging-capture-design.md) and builds on IMG-SRCSET
([port-notes/IMG-SRCSET.md](../port-notes/IMG-SRCSET.md)).

## Why

Capture settings keeps a sensor driver's settings (`[Device.N] Source.<Section>.<Key>`, sent to the driver through `SettingsCap`) in each
computer's `capture.ini`. A USB sensor that moves between computers therefore needs the same values saved on each one. They belong to the
imaging device, not to the computer: saved once, every computer should use them. The driver install, the calibration files and the
hardware bindings stay per computer.

## What moves, and what stays

**Moves to the database: the setting values**, every `[Device.N] Source.*` key (the X-ray source, filters, resolution and the other values
the driver lists).

**Stays in each computer's capture.ini:** `SettingsCap` and `SettingsSchemaCap` (the capability numbers of the driver installed on that
computer, which can differ between driver versions), the driver's description of its settings (`source-settings-<N>.json`, written by the
driver session on that computer), and everything else in `[Device.N]` (session mode, bits, polarity, the other capabilities). A computer
whose driver has no `SettingsCap` (contract below 1.4) cannot send the values: it works as today, and Capture settings says the device's
shared values are not used on this computer.

**Which devices.** Settings are keyed by `ImagingDeviceNum`. A device row with a blank `ComputerName` is used by every computer, so its
values are shared by all of them. A row bound to one computer (`ComputerName` set) is only used there, so its values in effect stay with
that computer, now in the database.

## Storage

A fork-only table, declared in a `ForkTableSet` (`ImagingDeviceSettingTables`) and created by fork migration step **2026112601** (levels
2026112601 to 2026112609 are reserved for this package in the migration README):

```sql
CREATE TABLE `imagingdevicesetting` (
  `ImagingDeviceSettingNum` bigint(20) NOT NULL AUTO_INCREMENT,
  `ImagingDeviceNum` bigint(20) NOT NULL,
  `SettingKey` varchar(255) NOT NULL,
  `SettingValue` mediumtext NOT NULL,
  `DateTEdit` datetime NOT NULL DEFAULT '0001-01-01 00:00:00',
  PRIMARY KEY (`ImagingDeviceSettingNum`),
  UNIQUE KEY `ImagingDeviceKey` (`ImagingDeviceNum`,`SettingKey`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci
```

- One row per `Source.*` key with a value, and a row `Stored` = `1` that marks the device as moved. Without it the device has not moved, and
  every computer uses its own file as today.
- Values are read as untrusted text, with today's limits (`TwainSourceSettings.TextBytesMax`, the key characters). A bad row refuses the
  device's values as a whole, with the reason in the log.
- Business class `ImagingDeviceSettings`, on a generated CRUD class like `SshHostKeyCrud`, parameters in every statement, Middle Tier
  aware. ImagingCapture uses it through a seam (`IDeviceDriverSettingsStore`) so the models stay testable without a database.
- Deleting an imaging device deletes its rows.

**Concurrent saves.** MyISAM has no transactions. A save takes the named lock `opms_imagingdevicesetting_<ImagingDeviceNum>`
(`GET_LOCK`, 10 seconds), checks that the device's latest `DateTEdit` is the one it loaded (else "Another computer changed these settings;
reopen Capture settings"), writes the value rows, writes `Stored` last, writes the security log entry, then releases the lock. A save cut
off part-way leaves a hash mismatch (below), which Capture settings repairs.

## Reading: the effective settings

Every reader of a device's `Source.*` keys reads the **effective settings**: an in-memory copy of `LocalCaptureConfig` with no file path
(so a stray `Save()` cannot write it), in which the device's `Source.*` keys are those of the database when the device has `Stored`=1 (the
file's `Source.*` keys of that device are then ignored, even one the database lacks), or the file's when it does not. The readers:

- the capture session (`TwainCaptureOptions.FromConfig`, `TwainSourceSettings.Build`), on both paths; on the 32-bit path the 64-bit
  program builds the options and sends them to the helper (`WriteOptions`), so the helper, which has no database, needs no change;
- Test capture (`FrmTestCapture`), the settings check (`CaptureSettingsValidator`), the bit-depth decision (`CaptureBitDepthDecision`);
- the recovery filer (`CaptureRecoveryFiler`), which rebuilds the options of a recovered capture;
- Capture settings itself.

The EZTwain facade sends no `Source.*` keys and is unchanged. The driver's description stays a per-computer file, read and written as
today. When the database wins over `Source.*` keys still in the file, the session log says so once ("capture.ini still has driver settings
for device N; the database's are used").

## Integrity checks

Two hashes, each logged in the Setup security log (`FKey`=ImagingDeviceNum):

- **This computer's gated settings** (`capture-gated-settings/1`, `GatedSettingsHash`): computed from **the file** only, unchanged. A file
  without `Source.*` keys hashes exactly as before (they enter the canonical text only when set), so no existing save or verification record
  changes. When a device moves, the saving computer's file loses its `Source.*` keys and the same save logs the file's new hash.
- **The device's shared values** (new `capture-driver-settings/1`, `DriverSettingsHash`): SHA-256 over the device's `Source.*` values in
  the database (keys ordinal ignoring case, values as written). Each save that changes them writes a security log entry with its own prefix
  `capture-driver-settings/1`, the hash, the computer that saved and the saved values (so the changed keys can be shown later). Every
  computer checks the database's values against the device's latest such entry, whatever computer wrote it. A mismatch (rows changed
  outside the program, a save cut off, rows without an entry) means the session does not send them: the source does not open, with "The
  driver settings of this device were changed outside Open PMS - Dental; review them in Setup." Capture settings opens the device, shows the
  keys that differ from the entry, and a Setup save repairs it.

Test capture's verification record (`capture-verification/2`, `; `-separated pairs) gains a pair `driverSettingsHash=` when the device is
stored; a record without it reads as before (unknown keys already parse). A verified device stays verified on other computers as long as
both hashes match their own latest entries.

## Saving and moving: Capture settings

- The device's `Source.*` rows show as now, marked "(all computers)" for a shared device. Saving writes them to the database (as above),
  then the file's other gated keys, then the log entries.
- **Moving a device.** When the device is not stored yet and this computer's file has `Source.*` keys, the screen says "These driver
  settings are kept on this computer only. Saving moves them to the database, for every computer." The save writes them to the database,
  removes them from this file and logs both hashes. Not stored and no keys: the first save of a value stores the device.
- **Leftovers.** On another computer whose file still has `Source.*` keys for a stored device, the screen says "capture.ini on this
  computer still has driver settings for this device, which are no longer used; Save removes them." That save logs the file's new hash
  (Setup permission, as every save).
- No silent move: values move only through a Setup save, so every change is in the security log.

## Migration

Step 2026112601 only creates the empty table. Nothing reads capture.ini during migration (it is per computer); devices move one at a time
through Capture settings. Until then every computer works exactly as today. `db/init/new-database.sql` is regenerated, and the adoption
tools' `allowed-differences.adoption.json` gains the table's `TableAdded` rule.

## Tests

- Unit (ImagingCaptureTests): the effective view (stored wins and ignores the file's `Source.*` keys; not stored uses the file; the copy
  has no path); the file hash without `Source.*` keys equals the old hash of the same file; `DriverSettingsHash` canonical text; the move
  (rows written, keys removed from the file, both entries); the leftover notice; the mismatch refusal and its repair; the concurrent-save
  refusal; verification records with and without the new pair; a computer without `SettingsCap`.
- Database (new `DeviceSettingsDbTests`): the CRUD with parameters, the unique key, the lock, deletion with the device, the length limits;
  `ForkTables` and `SchemaVerifier` accept the new table.
- ForkMigrationTests: step 2026112601 is idempotent; `db/init/new-database.sql` regenerated with the table.

## Documents

Port note `docs/port-notes/DEVSET.md`; imaging-capture-design 9.3; [admin/intraoral-capture-setup.md](../admin/intraoral-capture-setup.md)
and the how-to guides where they say the driver settings are per computer, plus a how-to for moving a device's settings; the fork migration
README; the roadmap entry moves to "Done".

## Review (2026-11-26)

A separate review agent checked the first draft. Applied above: sharing follows `ComputerName`; named lock and edit-time check instead of
an impossible transaction, `Stored` written last, a repair path; the schema description and the capability numbers stay per computer
(driver versions differ, and the description is written by the driver session, which on the 32-bit path runs in the helper without a
database); the missing readers (recovery filer, settings check, bit-depth decision, Test capture), the pathless effective copy; the log
entry's own prefix with the saved values; the verification record's pair format; a generated CRUD; the reserved migration levels.

## Open points

- Whether the camera's `[Camera.Settings]` (gain, exposure) should move the same way. Not asked; they stay per computer.
