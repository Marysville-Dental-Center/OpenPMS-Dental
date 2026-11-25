# RUNBOOK-R4: the adoption runbook brought up to date with the code

2026-11-25. Branch `runbook/r4-1`. Documentation only: [../adoption-runbook.md](../adoption-runbook.md). No code, test or tool changes.

## Why

A review of the runbook against `main` (3220e2299) before an office switch found it older than the code in places where an
administrator following it literally would go wrong, and silent about the programs that share the Open Dental database.

## What changed

- **Step 4, expected output.** The runbook expected 30 fork steps ending at level 2026110781. The code has 38 (`ForkMigrationRunner` finds
  every `[ForkMigration]` class; `NoBillInsBackfillStep` is abstract and skipped) ending at 2026112451. Added the log lines of 2026110801
  (OrthoComposite arguments: nothing to change on an Open Dental database), 2026111401 (`ehrcode`), 2026112303 (Image Adjust Presets grants)
  and 2026112451 (Dental Studio and Podium `programproperty.PropertyDesc`). The verifier's count of this program's tables is 18 (every
  `ForkTableSet` table), not 12; the row check's "Fork tables:" count is 17 (all but `supplyledger`, which has its own line), not 16. The row
  check now names `programproperty`, and its line count is no longer given as fourteen.
- **Step 6.1, allowed differences.** Added `TableAdded ehrcode` and `ColumnChanged programproperty.PropertyDesc`, which
  `tools/SyntheticData/allowed-differences.adoption.json` already allows. The runbook says a line for any other table means Rollback, so
  without them a correct adoption would have been rolled back.
- **`<tools>`.** The tools are self-contained .NET 10 x64 programs, not .NET Framework 4.8 ones.
- **Step 6.2.** The 64-bit setup installs into `C:\Program Files\Open PMS - Dental\` ([../installing.md](../installing.md)).
- **Step 0.0.1.** Records `sql_mode` (global and option files) and the time zone. Open Dental and this program set the global `sql_mode`
  blank at each start, but a restart (step 3.1) brings back the option files' value, by default strict on MariaDB 10.11.
- **Step 0.4.** For each other program: where it names the database, its account's grants (table and column grants name the database and do
  not exist on `opendental_pms`), the tables it inserts into, set against every column Open Dental 25.4 to 26.2 adds as NOT NULL without a
  default (from `Reference/OdSchema-25.3.48.sql` and `OdSchema-26.2.0.sql`), its own additions to the database (adoption drops
  `patient._portal_active_email`), and files it loads from the Open Dental program folder (step 2.1 uninstalls it).
- **Step 1.2.** Every such program is stopped, readers included: `connections`, `copy` and `adopt` refuse while one is connected.
- **Step 9 and Rollback step 5.** Repointing per program: grants, an agreed re-creation of a dropped addition, the configuration, its own
  check; on rollback, its previous build and a list of what it wrote to `opendental_pms`.
- Checklist rows 0.0.1, 0.4, 1.2 and 9 follow. Rows 0.11 and 7.1, which `RegistryTests.PrefNameTests` pins, are unchanged.

## Checked

- Under `STRICT_TRANS_TABLES`, an `INSERT` that leaves out a NOT NULL column without a default fails with error 1364 on a MyISAM table, and
  with a blank `sql_mode` it stores 0 or '' (development MariaDB 10.11.19).
- No column of 25.3.48 is removed or renamed in the 26.2.0 reference, so grants that name 25.3.48 columns apply on `opendental_pms`.

## Not checked

- The step 4 and 6.1 lines were derived from the code (`ForkMigrationRunnerAdapter`, `HeadlessAdoption`, `AdoptionRowCheck`, the step
  attributes), not from a rehearsal: the pinned SDK (10.0.115) was not installed on the PC that made this change. Before it reaches `main`,
  run `tools\AdoptionValidation\Invoke-AdoptionRehearsal.ps1` and `Invoke-RunbookDryRun.ps1` with the current build and compare their log
  with these lines.
- The test suites were not run. The runbook tests (`RegistryTests.PrefNameTests`, `ODOPTSTests.OptionWiringTests`) check text this change
  does not touch.
