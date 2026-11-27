# ADOPT-262: adoption accepts Open Dental 26.2 databases

Package ADOPT-262, on `main`. Open Dental's current stable version is 26.2, and this program follows its schema (26.2.0), but adoption
accepted only 24.3.63, 25.3.48 and the 25.3.47 / 25.3.54 label pair. The 26.2.0 reference (`Reference/OdSchema-26.2.0.sql`) is built from
Open Dental's published 26.2 database documents, with some NULL, default and key details inferred. The owner decided on 2026-11-26 that the
published documents are the schema ("you can just treat it as the real dump"; docs/decisions/owner-questions.md, item 13).

## What changed

- `ForkVersions.ListAdoptableLines`: one entry, the 26.2 line, schema version 26.2.0 (`AdoptableVersionLine`). `FindAdoptableLabels` returns
  labels for a pair on the line: `DataBaseVersion` 26.2.x and `ProgramVersion` 26.2.y with y not below x (Open Dental changes
  `DataBaseVersion` only when a convert step runs, so a later program can read a lower database version). So a 26.2 database goes through
  the same path as the office's pair (ADOPT-LABELS): the schema is checked at preflight, any difference refuses and is named, and the
  database is adopted as 26.2.0 with `odDataBaseVersion=` and `odProgramVersion=` in `ForkAdoptedFrom`. `ListAdoptableVersions`,
  `IsAdoptable` and the office pair are unchanged; `IsAdoptableSource` also accepts the line's schema version (a resumed adoption);
  `DescribeAdoptable` names everything that is adoptable, for messages.
- `SchemaVerifier.VerifyDatabaseAs(version)`: the reference by schema version (25.3.48, 26.1.70, 26.2.0). `Adoption.Preflight` and the
  adoption tools (`AdoptionValidation migration-preview`, the rehearsal) use it instead of always 25.3.48; `MigrationPreview.Run` passes the
  labels' schema version to its `verifySchema` callback.
- `ForkVersionGuard`: a 26.2 database gets NeedsAdoption; an unsupported version's message lists `DescribeAdoptable()`.
- No fork migration step changes. For a 26.2.0 source every Open Dental schema step finds nothing to add, and the seed and data steps that
  belong to older sources (`IsFrom24_3`, `IsFromBefore25_4`) are skipped, so only the program's own tables are added.

## Tests

- `ReleaseFlowTests.AdoptionLine26_2Tests` (pure): the line's labels and what is off it, the schema check before acceptance, a drifted
  schema refused, the record and its resume, the guard.
- `P262SchemaTests.Adoption26_2DbTests` (scratch MariaDB): a database built from the 26.2.0 reference with synthetic rows in the 26.2 columns
  and tables, labelled 26.2.5.0 / 26.2.9.0, adopted with the real runner: accepted after the schema check, recorded as 26.2.0 with the real
  labels, its schema still equal to the reference (fork tables aside), and no Open Dental table changed except `preference` and
  `securitylog`; drifted schemas and labels off the line are refused with nothing changed.

## Docs

The how-to guide [Move from Open Dental](../how-to/move-from-open-dental.md), [release-process.md](../develop/release-process.md#adopting-a-database),
[installing.md](../admin/installing.md), the [release notes](../release-notes/next.md) and the roadmap (item removed).
