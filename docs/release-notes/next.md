# Open PMS - Dental, next version: release notes (unreleased)

Open PMS - Dental is a fork of Open Dental (c) Open Dental Software, Inc. and contributors, licensed under the GNU GPL v2. It is not affiliated with or endorsed by Open Dental Software, Inc.

This file collects the notes of the version after 0.1 until it is released; the release renames it to `v<version>.md`. The notes of 0.1
are [v0.1.md](v0.1.md).

## Tamper detection removed

- **Open Dental's "Database Integrity" row hashes are gone.** The program no longer computes or checks the `SecurityHash` columns of
  appointments, claims, claim procedures, insurance subscribers and verifications, patients, payment plans and payment splits. The columns
  stay in the database with their values, so the schema still matches Open Dental's. The warning triangles in the edit windows, the
  integrity popups of the modules and Database Maintenance's Security Hashes tool are removed. Saving a patient no longer writes the row
  twice. Adoption no longer rehashes (runbook step 5 is no longer needed) and instead checks that every value is left as it was
  ([specs/remove-tamper-detection.md](../specs/remove-tamper-detection.md)).

## Adoption

- **An Open Dental database labelled 25.3.47.0 / 25.3.54.0 can be adopted.** Open Dental changes `DataBaseVersion` only when a convert step
  runs, so a database on Open Dental 25.3.54 reads `DataBaseVersion` 25.3.47.0 and `ProgramVersion` 25.3.54.0 while its schema is that of
  25.3.48. Adoption now accepts exactly that pair, and only when the schema equals the Open Dental 25.3.48 reference exactly (the office's
  `patient._portal_active_email` aside); otherwise it refuses and names the differences. It is adopted as 25.3.48, and the adoption record
  (`ForkAdoptedFrom`), the adoption log and the security log keep the real labels. 24.3.63 and 25.3.48 databases are adopted as before.
- **An Open Dental 26.2 database can be adopted** (any 26.2.x point release, `ProgramVersion` on the same line and not below
  `DataBaseVersion`). Its schema must equal Open Dental's published 26.2 schema exactly (owner decision of 2026-11-26: the published
  documents are the schema); adoption checks it first and names any difference. It is adopted as 26.2.0 with the real labels recorded, and
  the fork migration steps add only the program's own tables: no Open Dental table or row is changed apart from the version preferences.
- The adoption tools follow the same rule: `AdoptionValidation migration-preview` says in its first line whether the versions are adoptable
  (with the schema check), `adopt` accepts the pair as its `--original`, and `startup-check` also models an Open Dental 25.3.54 client.
  `SyntheticData generate` and the rehearsal (`Invoke-AdoptionRehearsal.ps1 -VersionLabels office`) can label a synthetic database the same
  way. Runbook step 0.2 now expects 25.3.47.0 / 25.3.54.0.
- **Adoption keeps a patient portal column.** The patient portal column `patient._portal_active_email` and its unique key
  `portal_active_email_uniq` stay as they are (owner decision of 2026-11-25, replacing the earlier decision to drop them). The office's
  patient portal relies on them for sign-in. The schema check allows them only with the office's exact definitions, both after adoption and
  at the program's start. Two active patients still cannot share an e-mail address: saving one stops with a database error naming
  `portal_active_email_uniq` (a clear message comes with a later setting). Backups made by the program (Backup, and the backup before an
  update) skip the generated column, which the backup table computes again. `migration-preview` says the column and key are kept.
