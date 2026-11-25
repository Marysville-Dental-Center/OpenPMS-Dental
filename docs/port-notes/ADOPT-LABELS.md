# ADOPT-LABELS: adoption accepts the office database's real version labels

Package ADOPT-LABELS, on branch `adopt/od25347-1` from `main` (3220e2299, rebased onto 4241f2a8a). A read-only validation of the office's production database on
2026-11-25 found `DataBaseVersion` 25.3.47.0 and `ProgramVersion` 25.3.54.0 (one `updatehistory` row, 25.3.54.0 on 2026-05-04), and its 422
tables and 4,409 columns equal the schema-only dump of 2026-10-18 that `Reference/OdSchema-25.3.48.sql` was built from. Open Dental changes
`DataBaseVersion` only when a convert step runs, so the database was never labelled 25.3.48 although it has that schema. Before this package
only the version gate stopped its adoption: `ForkVersions.ListAdoptableVersions` allowed `DataBaseVersion` 24.3.63.0 or 25.3.48.0, and
`Adoption.Preflight` also required `ProgramVersion` to equal it.

No fork migration step, schema, reference or seed changes: the pair is adopted as 25.3.48, so every step does exactly what it does for a
25.3.48 database.

## What changed

### 1. The rule (`OpenPmsDentalBusiness/Misc/ForkMigrations`)

- `ForkVersions.ListAdoptableLabels`: one entry, `DataBaseVersion` 25.3.47.0 with `ProgramVersion` 25.3.54.0, schema version 25.3.48.0
  (`AdoptableVersionLabels`). Exact match of both labels, all four parts. `FindAdoptableLabels`, and `HasAdoptableVersions(ForkDbState)` (the
  allowlist or the pair) for callers that only look at the preferences. `ListAdoptableVersions` and `IsAdoptable` are unchanged.
- `Adoption.Preflight(ForkDbState,SchemaVerifyResult=null)`: for the pair, `VersionSource` is 25.3.48.0 and `Labels` is set. Without a schema
  result `CanAdopt` is false and `IsSchemaCheckPending` true; `Preflight()` (the database overload, which `FormAdoptDatabase`, `Adoption.Run` and
  `HeadlessAdoption` call) then runs `SchemaVerifier.VerifyDatabase25_3_48()` and decides. A pure caller can never get `CanAdopt` for the pair
  without a schema result.
- Exact means: `GetLabelsSchemaProblems` lists every difference except the allowed ones on `patient._portal_active_email` and
  `portal_active_email_uniq`. The HQ-only, legacy and fork tables that `SchemaVerifier`'s documented exceptions allow count as differences
  here: an Open Dental database with these labels has none of them, and the validation found exactly the reference's tables. Any difference
  refuses, and the message names up to 20 of them (`ListSchemaProblems` has all).
- 24.3.63.0 and 25.3.48.0 are unchanged: no schema check at preflight, and `ProgramVersion` must still equal `DataBaseVersion`. Any other pair
  (`ProgramVersion` on the wrong side, 25.3.47.0 twice, 25.3.54.0 twice, 25.3.48.0 with 25.3.54.0, ...) is refused whatever the schema; the
  refusal now lists the pair too.
- The record: `ForkAdoptedFrom` is `source=25.3.48.0;odDataBaseVersion=25.3.47.0;odProgramVersion=25.3.54.0;started=...;adopted=...`
  (`Adoption.FormatAdoptedFromStart`, `ParseAdoptedFromLabels`). `source=` stays the schema version on purpose: the fork migration steps and the
  adoption tools decide what a database still needs from it (`Adoption.ParseAdoptedFromSource`, docs/port-notes/INIT-SQL.md). The adoption log
  line "Adoption from Open Dental 25.3.48.0 started." adds the labels, and so do `AdoptionResult.Labels` and the security log entry. The record
  of a 24.3.63 or 25.3.48 adoption is unchanged. A resumed adoption reads the labels back from its record and does not check the schema again
  (the copy already has the sentinel versions).
- `OdSchemaCompat` is 25.3.48 for the pair, as for a 25.3.48 database (`versionSource.ToString(3)`).
- `ForkVersionGuard`: the pair gets NeedsAdoption (exit code as for 25.3.48) with a message that says the adoption tool checks the schema
  first; the wrong-side pair is refused as an unsupported version, as before.

### 2. The adoption tools (`tools/AdoptionValidation`)

- `HeadlessAdoption.CheckCopy` accepts an `--original` with the pair (`HasAdoptableVersions`); the refusal names both labels. `adopt` itself
  goes through `Adoption.Preflight()`, so the schema check runs on the copy before anything changes.
- `MigrationPreview.Run(db,database,verifySchema)`: the result has `ProgramVersion` and `PreflightVersions`, and the first printed line is
  "Versions: adoptable. ..." or "Versions: NOT adoptable. ..." with the preflight message. The `migration-preview` command connects the business
  layer to the database (read-only; it only reads `information_schema`) and passes `SchemaVerifier.VerifyDatabase25_3_48`, so runbook step 0.2
  sees the schema verdict a week before the day.
- `OdStartupCheck.StandardScenarios` adds an Open Dental 25.3.54 client (`VersionOfficeProgram`) as update server and as workstation. On the
  unadopted pair the model calls the 25.3.54 update server's start a conversion (DataBaseVersion is older than the client); Open Dental would
  find no convert step to run. On an adopted copy every 25.3.54 client is refused like the others (V10).
- `rehearse --version-labels reference|office` and `Invoke-AdoptionRehearsal.ps1 -VersionLabels reference|office`. With `office`: the
  generated database has the pair; A2's control client is the one of the copy's `ProgramVersion` (25.3.54, Proceeds or ConvertsAndProceeds);
  A3 also requires the labels in the preflight and a "adoptable" verdict from the preview's schema check; V8 also requires the labels in
  `ForkAdoptedFrom`. With `reference` (the default) every check is as before, and V8 now also requires that no labels are recorded. The
  rehearsal connects the business layer to the source before the preview (read-only); the copy's connection check clears the pool first.

### 3. `tools/SyntheticData`

- `GenerationSettings.Labels` (`VersionLabels.Reference` or `Office`), `--version-labels reference|office` on `generate`. Only the two
  preference values change; `SyntheticDataInfo` adds `;labels=office` before `;tool=` for the office labels (the as-of parser reads by field).

### 4. Documents

- Runbook step 0.2 expects 25.3.47.0 / 25.3.54.0 (or 25.3.48.0 twice) and the "Versions: adoptable" line; step 4's expected first lines and
  the checklist row 0.2 follow. Rebased onto `main` after RUNBOOK-R4 (`runbook/r4-1`, merge 4241f2a8a) rewrote other parts of the
  runbook; the rebase applied without conflict.
- Release notes of the next version, the READMEs of ForkMigrations, AdoptionValidation and SyntheticData.

## Tests

- `tests/ReleaseFlowTests/AdoptionVersionLabelsTests.cs` (no database): the list holds the one pair; wrong side and near pairs are not found;
  without a schema result the pair waits; a matching schema (with and without the office column) is accepted as 25.3.48; a changed column, a
  missing table and an extra column are each refused and named; an HQ-only and a legacy table that `SchemaVerifier` allows still refuse;
  seven other pairs are refused on a matching schema; 24.3.63 and 25.3.48 unchanged; the guard; the `ForkAdoptedFrom` text, its parsing, and a
  resume of an unfinished labels adoption.
- `tests/ForkMigrationTests/OfficeLabelsAdoptionDbTests.cs` (scratch database `odlabels`, the 25.3.48 reference schema plus the office
  column): the pair is adopted with `Adoption.Run` (baseline runner) and records the exact `ForkAdoptedFrom`, log line, `OdSchemaCompat`,
  sentinels and guard results; four drifts (extra column, changed column, HQ-only table, missing table) are refused by `Preflight()` and
  `Run`, naming the difference, with nothing changed; four other pairs on the same schema are refused.
- `tools/AdoptionValidation/AdoptionValidation.Tests/OdStartupCheckTests.cs`: the 25.3.54 scenarios exist; a 25.3.54 client opens the
  unadopted pair and is refused on an adopted copy.
- The rehearsal on the pair: `Invoke-AdoptionRehearsal.ps1 -VersionLabels office`.

## Decisions (INFERRED)

- `source=` keeps the schema version (25.3.48.0) rather than the real `DataBaseVersion`: the steps' "from 24.3" and "before 25.4" decisions
  then hold without change, and the labels are extra fields every reader of the record ignores. The alternative, `source=25.3.47.0`, gives the
  same answers for those two decisions today but would make any future "is it 25.3.48" check wrong.
- Only the exact pair is listed, not "any 25.3.x whose schema matches": the brief asked for that pair, and a wider rule would accept a database
  the office has not validated. A later pair is one more `ListAdoptableLabels` entry with its tests.
- The strict comparison ignores only the office column and key, not the other documented exceptions (section 1).

## Checks

- Build and tests: NOT RUN in this package's session. The pinned .NET SDK 10.0.115 (`global.json`) is not installed on the development PC
  (10.0.403 and 9.0.320 are), and the brief said not to install it; a check build with the installed 10.0.403 was not made either. Before
  this reaches `main`: the full build of `OpenDental.Local.slnf`, `tools/run-tests.ps1` (ReleaseFlowTests, ForkMigrationTests,
  AdoptionValidation.Tests, SyntheticData.Tests at least), and `Invoke-AdoptionRehearsal.ps1 -VersionLabels office` plus the default
  rehearsal.
