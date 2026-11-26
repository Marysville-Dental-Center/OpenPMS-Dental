# ADOPT-PORTAL-EMAIL: adoption keeps the office's patient portal column

Package ADOPT-PORTAL-EMAIL, on branch `adopt/keep-portal-email-1` from `main` (c15d1c6af). Owner decision of 2026-11-25: adoption **keeps** the
office's `patient._portal_active_email` and its unique key `portal_active_email_uniq` instead of dropping them. The office's patient portal
needs that rule (one active patient per e-mail address) for sign-in. It replaces the decision of 2026-10-18 (M1) to drop them. A proper
product setting comes later (docs/roadmap.md, "Not scheduled yet").

The office's definitions, as `SHOW CREATE TABLE` prints them (read from the local `od_253_schema`, which is built from the office's
schema-only dump; the column is the last of patient's 85):

- ``varchar(100) GENERATED ALWAYS AS (case when `PatStatus` = 0 and `Email` <> '' then `Email` else NULL end) VIRTUAL``
- ``UNIQUE KEY `portal_active_email_uniq` (`_portal_active_email`)``

## What changed

### 1. Adoption (`OpenPmsDentalBusiness/Misc/ForkMigrations/Adoption.cs`)

- Step 2 no longer drops anything of the office's: `KeepPortalActiveEmail` leaves the column and `portal_active_email_uniq` as they are and
  logs "Kept column patient._portal_active_email and its unique key portal_active_email_uniq (the office's patient portal: one active patient
  per e-mail address)." Any *other* key on the column is still dropped and logged ("no other office addition is kept"); the office has none.
- `AdoptionResult.WasPortalEmailColumnDropped` is replaced by `IsPortalEmailColumnKept` and `IsPortalEmailIndexKept`;
  `ListIndexesDropped` now lists only those other keys.
- The pre-adoption exact check of the 25.3.47 / 25.3.54 labels (ADOPT-LABELS) is unchanged, and now also requires the office's definitions.

### 2. The schema verifier (`SchemaVerifier.cs`)

- `SchemaException.ActualDefinition`: when set, the exception matches only a difference whose actual definition is exactly that text.
- The column and key exceptions are now `ExtraColumn` / `ExtraIndex` on `patient` with `ActualDefinition`
  `PortalActiveEmailColumnDefinition` / `PortalActiveEmailIndexDefinition` (new constants, plus `PortalActiveEmailAddDdl` for tests and
  tools). Before, they matched any difference of any kind on those names. Any other definition, a non-unique or wider key, another key on
  the column, or the column on another table is a mismatch.
- One list serves every reference (25.3.48, 26.1.70 and the default 26.2.0), so the same rule holds at adoption (the `adopt` and rehearsal
  check M2), at the fork migration runner's end (`ForkMigrationRunnerAdapter` writes `OdSchemaCompat` only on a match) and so at the
  program's start (`ForkVersionGuard` reads `OdSchemaCompat`). No fork migration step alters `patient`: its definition is the same in the
  25.3.48 and 26.2.0 references, and every step ran with the column present in `Od253MigrationTests`, `Migration26_1DbTests` and the new
  test below.

### 3. The program with the column there

- CRUD: `PatientCrud` selects by column name and inserts and updates named columns, so the generated column is never written. Checked by
  the DB test below (insert, update, `GetPat`, `GetFamily`, `SelectMany`, `MergeTwoPatients`).
- A duplicate active address on save is MariaDB's duplicate-key error (1062, naming `portal_active_email_uniq`), as the brief accepts; no
  friendly message yet (roadmap).
- Backups (`MiscData.MakeABackup`: the Backup window, and the backup before an update): they copied every table with `INSERT ... SELECT *`
  (same server) or `SELECT *` and parameterized inserts (another server). Writing a value into a generated column is warning 1906 under the
  blank `sql_mode` the program sets, and an error under a strict one (checked on MariaDB 10.11). `GetBackupColumnList` now names the stored
  columns of a table that has a generated column; every other table is copied with `*` as before. The backup table computes the column
  again (its `CREATE TABLE` is the original's).
- `FormAdoptDatabase`'s explanation says "keeps" (and the UI refresh's pinned runtime text with it).

### 4. Adoption tools

- `migration-preview`: a line saying the column and key are kept, and whether they have the office's definitions (if not: "the schema check
  after adoption will fail. Stop and ask before adopting.").
- `adopt` / rehearsal M2 and I2 (`HeadlessAdoption.IsSchemaExactMatch`): the kept column and key count as allowed, and only as the verifier
  allowed them (so only with the office's definitions); the log line names them.
- Rehearsal: A4 reports what was kept; V5 is now "patient._portal_active_email and its unique key kept, exactly as the office has them"
  (definitions equal and one key on the column).
- `tools/SyntheticData/allowed-differences.adoption.json`: the `ColumnRemoved patient._portal_active_email` and
  `TotalChanged patients.portal_active_email_nonnull` rules are gone, so a removal is now an unexpected difference.
- The data-layer round trip still leaves the column out (no class field; MariaDB computes it); wording only.

### 5. Documents

- Runbook: "What adoption does" item 2, the step 0.4 bullet on other programs' additions, step 0.2 (the preview line), step 4's expected
  lines, step 6.1's allowed list. `docs/develop/release-process.md` adoption step 2.
- `docs/decisions/owner-questions.md` switch-over question 12 records the decision; `docs/roadmap.md` "Not scheduled yet" the setting; release notes.
- Not changed: `docs/reports/m1-validation-report.md`, a dated record of the M1 rehearsal as it ran.

## Tests

- `tests/ForkMigrationTests/SchemaSnapshotTests.cs` `Compare_PortalActiveEmail_AllowedOnlyWithTheOfficesDefinition`: the constants equal what
  the parser gives for the office's DDL; allowed against all three exception lists; four other column definitions, three other key
  definitions, an extra key on the column and the column on another table are mismatches.
- `tests/ForkMigrationTests/PortalEmailKeptDbTests.cs` (new): a fresh development database with the office's column and key, adopted with
  every fork step: both kept with the office's definitions, 26.2.0 verify matches with them as the only allowed patient differences,
  `OdSchemaCompat` 26.2.0 and the guard opens it, every step again changes nothing; patients insert, update, load and merge; a second active
  patient with the same address fails with MariaDB's 1062 on insert and on reactivation; inactive and blank addresses save; `MakeABackup`
  copies patients and computes the column again.
- Updated for "kept": `AdoptionDbTests` (ReleaseFlowTests), `OfficeLabelsAdoptionDbTests`, `Migration26_1DbTests`,
  `AdoptionVersionLabelsTests` and `SchemaSnapshotTests` (the office's exact expression), `RehearsalIntegrationTests`,
  `DataLayerRoundTripTests`, SyntheticData `UnitTests` and `DatabaseTests` (a removal is now unexpected), `DbGrantsTests` comment.

## Checks

On the development PC, 2026-11-25, local MariaDB 10.11.

- Build: `OpenDental.Local.slnf`, Debug x64, SDK 10.0.115: 0 errors.
- Targeted tests, x64 (`tools/run-tests.ps1 -Mode Targeted -Platform x64`): ForkMigrationTests 51 passed (2 skipped), ReleaseFlowTests 270
  (7 skipped), SyntheticData.Tests 59 (1 skipped), AdoptionValidation.Tests 70, RegistryTests 39, P262SchemaTests 24 (4 skipped),
  UiRefreshTests 415 (26 skipped), all with 0 failed. The first run had 2 failures in M201Tests (`Od253_MatchesThe26_1ReferenceExactly`,
  `Od253_MatchesTheLoadedOd261SchemaDatabase`, which expected the column gone) and 2 in NoHqTests (a new verifier message); commit
  f2acfc024 updated those tests and `brand-kept.tsv`, after which M201Tests 24 passed (2 skipped) and NoHqTests 137/137. That commit
  changed test code only, so the other suites were not re-run.
- Rehearsal `Invoke-AdoptionRehearsal.ps1 -VersionLabels office` (medium, 2 runs): PASS, 32 of 32 checks in each run, X1 to X3 PASS.
  A3: the 25.3.47.0 / 25.3.54.0 database matches the 25.3.48 reference (422 tables). A4: kept column True, kept key True, other keys
  dropped none. M2: 430 of 430 tables plus 18 of this program; 2 differences, 0 mismatches (the column and the key). V3: 126 allowed, 0
  unexpected. V4: 146 of 146 totals identical (`patients.portal_active_email_nonnull` no longer changes). V5: both definitions the
  office's, 1 key on the column. V10: 14 of 14 release-client scenarios refused.
- Rehearsal, default labels (medium, 2 runs): PASS, 32 of 32 checks in each run, X1 to X3 PASS.
- Runbook dry run (`Invoke-RunbookDryRun.ps1`, on a source kept by a one-run office-label rehearsal, 30 of 30): Dry run PASS. Step 6.1:
  0 unexpected differences, and no `_portal_active_email` line. The adopt log lines equal the runbook's step 4 and 5 lines. The
  schema-match line of step 5 now ends with "and the office's patient._portal_active_email and its key portal_active_email_uniq as the
  office has them (kept)."; the runbook was corrected to say so. The step 0.2 preview line equals the runbook's.
- The scratch databases of the dry run were dropped afterwards.
